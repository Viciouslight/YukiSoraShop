using Application.Services.Interfaces;
using Application.DTOs;
using Application;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Extensions;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class ViewCartModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _uow;

        public ViewCartModel(IProductService productService, IUnitOfWork uow)
        {
            _productService = productService;
            _uow = uow;
        }

        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
        public int TotalItems => CartItems.Sum(item => item.Quantity);

        public void OnGet()
        {
            LoadCartFromSession();
        }

        public IActionResult OnPostAddSampleItems()
        {
            LoadCartFromSession();

            // Add sample products to cart
            var sampleProducts = _productService.GetAllProducts().Take(3);
            foreach (var product in sampleProducts)
            {
                var existingItem = CartItems.FirstOrDefault(item => item.Product.Id == product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity += 1;
                }
                else
                {
                    CartItems.Add(new CartItemDto
                    {
                        Product = product,
                        Quantity = 1
                    });
                }
            }

            SaveCartToSession();
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateQuantity(int productId, string action)
        {
            LoadCartFromSession();

            var cartItem = CartItems.FirstOrDefault(item => item.Product.Id == productId);
            if (cartItem != null)
            {
                if (action == "increase")
                {
                    cartItem.Quantity++;
                }
                else if (action == "decrease" && cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }

                SaveCartToSession();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveItem(int productId)
        {
            LoadCartFromSession();

            var cartItem = CartItems.FirstOrDefault(item => item.Product.Id == productId);
            if (cartItem != null)
            {
                CartItems.Remove(cartItem);
                SaveCartToSession();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckout()
        {
            LoadCartFromSession();
            if (!CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage();
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
            {
                return RedirectToPage("/Auth/Login");
            }

            var subtotal = CartItems.Sum(i => i.TotalPrice);
            var tax = subtotal * 0.10m;
            var grandTotal = subtotal + tax;

            var order = new Order
            {
                AccountId = userId,
                Status = "Pending",
                Subtotal = subtotal,
                ShippingFee = 0,
                GrandTotal = grandTotal,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = HttpContext.Session.GetString("UserName") ?? "customer",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = HttpContext.Session.GetString("UserName") ?? "customer",
                IsDeleted = false
            };

            await _uow.OrderRepository.AddAsync(order);

            foreach (var item in CartItems)
            {
                // Ensure product exists in DB; skip if not found
                var productEntity = await _uow.ProductRepository.GetByIdAsync(item.Product.Id);
                if (productEntity == null) continue;

                var od = new OrderDetail
                {
                    Order = order,
                    ProductId = productEntity.Id,
                    Quantity = item.Quantity,
                    UnitPrice = productEntity.Price,
                    LineTotal = productEntity.Price * item.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = order.CreatedBy,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = order.ModifiedBy,
                    IsDeleted = false
                };

                await _uow.OrderDetailRepository.AddAsync(od);
            }

            await _uow.SaveChangesAsync();

            // Clear cart after creating order
            HttpContext.Session.Remove("ShoppingCart");

            return Redirect($"/Orders/Pay?OrderId={order.Id}");
        }

        private void LoadCartFromSession()
        {
            CartItems = HttpContext.Session.GetObject<List<CartItemDto>>("ShoppingCart") ?? new List<CartItemDto>();
        }

        private void SaveCartToSession()
        {
            HttpContext.Session.SetObject("ShoppingCart", CartItems);
        }

    }
}

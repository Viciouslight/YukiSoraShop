using Application.Services.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Extensions;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class CartModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public CartModel(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
        public int TotalItems => CartItems.Sum(item => item.Quantity);

        public void OnGet()
        {
            LoadCartFromSession();
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

            var createdBy = HttpContext.Session.GetString("UserName") ?? "customer";
            var orderItems = CartItems.Select(ci => new OrderItemInput
            {
                ProductId = ci.Product.Id,
                Quantity = ci.Quantity
            });

            var order = await _orderService.CreateOrderFromCartAsync(userId, orderItems, createdBy);

            // Clear cart after creating order
            HttpContext.Session.Remove("ShoppingCart");

            return Redirect($"/Orders/Checkout?OrderId={order.Id}");
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

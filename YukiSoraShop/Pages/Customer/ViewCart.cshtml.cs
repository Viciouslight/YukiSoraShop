using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Extensions;
using YukiSoraShop.Models;
using YukiSoraShop.Services;

namespace YukiSoraShop.Pages.Customer
{
    public class ViewCartModel : PageModel
    {
        private readonly IProductService _productService;

        public ViewCartModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<CartItem> CartItems { get; set; } = new();
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
                    CartItems.Add(new CartItem
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

        private void LoadCartFromSession()
        {
            CartItems = HttpContext.Session.GetObject<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();
        }

        private void SaveCartToSession()
        {
            HttpContext.Session.SetObject("ShoppingCart", CartItems);
        }

    }
}

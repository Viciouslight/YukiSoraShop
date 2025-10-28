using Application.Services.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Extensions;

namespace YukiSoraShop.Pages.Customer
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<ProductDto> Products { get; set; } = new();

        public void OnGet()
        {
            Products = _productService.GetAllProducts();
        }

        public ProductDto? GetProductById(int id)
        {
            return _productService.GetProductById(id);
        }

        public IActionResult OnPostAddToCart(int id)
        {
            // Load cart from session
            var cart = HttpContext.Session.GetObject<List<CartItemDto>>("ShoppingCart") ?? new List<CartItemDto>();

            var product = _productService.GetProductById(id);
            if (product != null)
            {
                var existing = cart.FirstOrDefault(i => i.Product.Id == product.Id);
                if (existing != null)
                {
                    existing.Quantity += 1;
                }
                else
                {
                    cart.Add(new CartItemDto { Product = product, Quantity = 1 });
                }

                HttpContext.Session.SetObject("ShoppingCart", cart);
            }

            return RedirectToPage();
        }
    }
}

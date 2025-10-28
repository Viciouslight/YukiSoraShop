using Application.Services.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    public class CustomerCatalogModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public CustomerCatalogModel(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public List<ProductDto> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            Products = await _productService.GetAllProductsDtoAsync();
        }

        // Removed sync helper; prefer async methods on service

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCartAsync(int id)
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
                return RedirectToPage("/Auth/Login");

            await _cartService.AddItemAsync(accountId, id, 1);
            return RedirectToPage();
        }
    }
}

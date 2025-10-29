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

        public List<ProductDTO> Products { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int Size { get; set; } = Application.DTOs.Pagination.PaginationDefaults.DefaultPageSize;
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public async Task OnGetAsync()
        {
            var paged = await _productService.GetProductsPagedAsync(Page, Size, Search, Category);
            Products = paged.Items.ToList();
            TotalPages = paged.TotalPages;
            TotalItems = paged.TotalItems;
        }

        // Removed sync helper; prefer async methods on service

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCartAsync(int id)
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
                return RedirectToPage("/Auth/Login");

            await _cartService.AddItemAsync(accountId, id, 1);
            return RedirectToPage(new { Page, Size, Search, Category });
        }
    }
}

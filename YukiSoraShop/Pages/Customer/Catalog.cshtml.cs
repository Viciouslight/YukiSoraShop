using Application.Services.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Application.DTOs.Pagination;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YukiSoraShop.Pages.Customer
{
    public class CustomerCatalogModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ILogger<CustomerCatalogModel> _logger;

        public CustomerCatalogModel(IProductService productService, ICartService cartService, ILogger<CustomerCatalogModel> logger)
        {
            _productService = productService;
            _cartService = cartService;
            _logger = logger;
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
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var size = Size <= 0 ? PaginationDefaults.DefaultPageSize : Math.Min(Size, PaginationDefaults.MaxPageSize);
                var page = Page <= 0 ? PaginationDefaults.DefaultPageNumber : Page;

                var paged = await _productService.GetProductsPagedAsync(page, size, Search, Category);
                Products = paged.Items.ToList();
                TotalPages = paged.TotalPages;
                TotalItems = paged.TotalItems;

                // Clamp current page to bounds for UI
                if (TotalPages > 0 && Page > TotalPages) Page = TotalPages;
                if (Page <= 0) Page = 1;
                Size = size;

                // Load categories for filter dropdown
                var cats = await _productService.GetAllCategoriesAsync();
                CategoryOptions = cats
                    .Select(c => new SelectListItem { Value = c.CategoryName, Text = c.CategoryName, Selected = c.CategoryName == Category })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load catalog: page={Page}, size={Size}, search={Search}, category={Category}", Page, Size, Search, Category);
                TempData["Error"] = "Không thể tải danh sách sản phẩm. Vui lòng thử lại sau.";
                Products = new List<ProductDTO>();
                TotalPages = 0;
                TotalItems = 0;
                CategoryOptions = new List<SelectListItem>();
            }
        }

        // Removed sync helper; prefer async methods on service

        [ValidateAntiForgeryToken]
        public IActionResult OnPostAddToCart(int id)
        {
            // Chỉ cần điều hướng sang ProductDetails
            return RedirectToPage("/Customer/ProductDetails", new { id });
        }

    }
}
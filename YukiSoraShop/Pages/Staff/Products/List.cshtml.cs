using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator")]
    public class StaffProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductsModel> _logger;

        public StaffProductsModel(IProductService productService, ILogger<StaffProductsModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int Size { get; set; } = Application.DTOs.Pagination.PaginationDefaults.DefaultPageSize;
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var size = Size <= 0 ? Application.DTOs.Pagination.PaginationDefaults.DefaultPageSize : Math.Min(Size, Application.DTOs.Pagination.PaginationDefaults.MaxPageSize);
                var page = Page <= 0 ? Application.DTOs.Pagination.PaginationDefaults.DefaultPageNumber : Page;

                var paged = await _productService.GetProductsPagedEntitiesAsync(page, size, Search);
                Products = paged.Items.ToList();
                TotalPages = paged.TotalPages;
                TotalItems = paged.TotalItems;

                if (TotalPages > 0 && Page > TotalPages) Page = TotalPages;
                if (Page <= 0) Page = 1;
                Size = size;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products list: page={Page}, size={Size}, search={Search}", Page, Size, Search);
                TempData["Error"] = "Không thể tải danh sách sản phẩm. Vui lòng thử lại sau.";
                Products = new List<Product>();
                TotalPages = 0;
                TotalItems = 0;
            }

            return Page();
        }
    }
}

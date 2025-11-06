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
        public int Size { get; set; } = PaginationDefaults.DefaultPageSize;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

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
                var products = paged.Items.ToList();

                if (!string.IsNullOrEmpty(Sort))
                {
                    products = Sort switch
                    {
                        "price_asc" => products.OrderBy(p => p.Price).ToList(),
                        "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                        _ => products
                    };
                }

                Products = products;
                TotalPages = paged.TotalPages;
                TotalItems = paged.TotalItems;

                if (TotalPages > 0 && Page > TotalPages) Page = TotalPages;
                if (Page <= 0) Page = 1;
                Size = size;

                var cats = await _productService.GetAllCategoriesAsync();
                CategoryOptions = cats
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryName,
                        Text = c.CategoryName,
                        Selected = c.CategoryName == Category
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load catalog: page={Page}, size={Size}, search={Search}, category={Category}",
                    Page, Size, Search, Category);

                TempData["Error"] = "Không thể tải danh sách sản phẩm. Vui lòng thử lại sau.";
                Products = new List<ProductDTO>();
                TotalPages = 0;
                TotalItems = 0;
                CategoryOptions = new List<SelectListItem>();
            }
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostAddToCart(int id)
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
                return RedirectToPage("/Auth/Login");

            // ✅ Kiểm tra quyền
            var isAdmin = User.IsInRole("Administrator");
            var isStaff = User.IsInRole("Moderator");

            if (isAdmin)
            {
                TempData["Error"] = "Tài khoản quản trị không thể thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }

            if (isStaff)
            {
                TempData["Error"] = "Nhân viên không thể thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }

            // ✅ Khách hàng hợp lệ
            await _cartService.AddItemAsync(accountId, id, 1);
            TempData["Success"] = "🎉 Đã thêm sản phẩm vào giỏ hàng!";
            TempData.Keep(); // Giữ TempData sau redirect

            // Cập nhật lại số lượng sản phẩm trong giỏ
            var items = await _cartService.GetItemsAsync(accountId);
            TempData["CartCount"] = items?.Sum(i => i.Quantity) ?? 0;
            TempData.Keep();

            return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });

        }

    }
}

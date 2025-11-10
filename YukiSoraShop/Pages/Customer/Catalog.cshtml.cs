using Application.Services.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Application.DTOs.Pagination;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Selecting;

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

        private int GetAccountIdFromUser()
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(accountIdStr, out var accountId) ? accountId : 0;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCart(int id, CancellationToken ct = default)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }

            var accountId = GetAccountIdFromUser();
            if (accountId <= 0)
            {
                return RedirectToPage("/Auth/Login");
            }

            // Role restrictions
            if (User.IsInRole("Administrator"))
            {
                TempData["Error"] = "Tài khoản quản trị không thể thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }

            if (User.IsInRole("Moderator"))
            {
                TempData["Error"] = "Nhân viên không thể thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }

            try
            {
                var product = await _productService.GetProductDtoByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
                }

                // If product has variants, redirect to details page
                if (product.ProductDetails != null && product.ProductDetails.Any())
                {
                    TempData["Info"] = "Sản phẩm có nhiều biến thể. Vui lòng chọn biến thể trước khi thêm vào giỏ hàng.";
                    return RedirectToPage("/Customer/ProductDetails", new { id });
                }

                // Check availability
                if (!product.IsAvailable || product.Stock <= 0)
                {
                    TempData["Error"] = "Sản phẩm tạm thời không có sẵn.";
                    return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
                }

                // Add to cart (price comes from product entity)
                await _cartService.AddItemAsync(accountId, id, 1, ct);

                // Update cart count
                var items = await _cartService.GetItemsAsync(accountId, ct);
                TempData["CartCount"] = items?.Sum(i => i.Quantity) ?? 0;

                TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product {ProductId} to cart for account {AccountId}", id, accountId);
                TempData["Error"] = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại sau.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort });
            }
        }
    }
}
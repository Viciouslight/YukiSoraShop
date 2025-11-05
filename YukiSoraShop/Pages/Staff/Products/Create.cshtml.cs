using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffProductCreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductCreateModel> _logger;

        public StaffProductCreateModel(IProductService productService, ILogger<StaffProductCreateModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        [ValidateNever]
        public Product Product { get; set; } = new();

        [BindProperty]
        public List<ProductDetail> ProductDetails { get; set; } = new();


        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoryOptions();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoryOptions();

            // Validate Product
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra các lỗi trong biểu mẫu.";
                return Page();
            }

            try
            {
                var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError("Product.CategoryId", "Danh mục không hợp lệ.");
                    return Page();
                }

                Product.CategoryName = category.CategoryName ?? string.Empty;
                var username = HttpContext.User?.Identity?.Name ?? "system";

                Product.CreatedAt = Product.ModifiedAt = DateTime.UtcNow;
                Product.CreatedBy = Product.ModifiedBy = username;

                // 🔥 Kiểm tra bắt buộc có ít nhất 1 ProductDetail
                if (ProductDetails == null || !ProductDetails.Any())
                {
                    //ModelState.AddModelError(string.Empty, "Bạn phải nhập ít nhất một biến thể sản phẩm (thông tin chi tiết).");
                    TempData["Error"] = "Vui lòng nhập thông tin chi tiết sản phẩm.";
                    return Page();
                }

                // 🔥 Kiểm tra từng ProductDetail có hợp lệ không
                foreach (var detail in ProductDetails)
                {
                    // Nếu tất cả đều trống → báo lỗi luôn
                    bool allEmpty =
                        string.IsNullOrWhiteSpace(detail.Color) &&
                        string.IsNullOrWhiteSpace(detail.Size) &&
                        string.IsNullOrWhiteSpace(detail.Material) &&
                        string.IsNullOrWhiteSpace(detail.Origin) &&
                        string.IsNullOrWhiteSpace(detail.ImageUrl) &&
                        string.IsNullOrWhiteSpace(detail.Description) &&
                        !detail.AdditionalPrice.HasValue;

                    if (allEmpty)
                    {
                        ModelState.AddModelError(string.Empty, "Mỗi biến thể sản phẩm phải có ít nhất một thông tin được nhập.");
                        TempData["Error"] = "Vui lòng nhập đầy đủ thông tin cho từng biến thể sản phẩm.";
                        return Page();
                    }

                    // Nếu có dữ liệu → validate model
                    if (!TryValidateModel(detail))
                    {
                        TempData["Error"] = "Vui lòng kiểm tra lại thông tin chi tiết sản phẩm.";
                        return Page();
                    }

                    detail.CreatedAt = detail.ModifiedAt = DateTime.UtcNow;
                    detail.CreatedBy = detail.ModifiedBy = username;
                }

                Product.ProductDetails = ProductDetails;

                var success = await _productService.CreateProductAsync(Product);
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products/List");
                }

                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductName}", Product?.ProductName);
                TempData["Error"] = "Đã xảy ra lỗi. Vui lòng thử lại.";
            }

            return Page();
        }

        private async Task LoadCategoryOptions()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            }).ToList();
        }
    }
}

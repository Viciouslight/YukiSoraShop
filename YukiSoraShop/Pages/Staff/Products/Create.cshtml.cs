using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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
        public Product Product { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền Staff
            

            await LoadCategoryOptions();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError("Product.CategoryId", "Danh mục không hợp lệ.");
                    await LoadCategoryOptions();
                    return Page();
                }
                Product.CategoryName = (category.CategoryName ?? string.Empty).Trim();

                // Clear ModelState and validate with correct prefix so hidden fields don't block
                ModelState.Clear();
                if (!TryValidateModel(Product, nameof(Product)))
                {
                    // Log specific validation errors to help debugging
                    var errors = ModelState
                        .Where(kvp => kvp.Value?.Errors?.Count > 0)
                        .SelectMany(kvp => kvp.Value!.Errors.Select(err => new { Field = kvp.Key, Error = err.ErrorMessage }))
                        .ToList();
                    foreach (var e in errors)
                    {
                        _logger.LogWarning("Product create validation error: {Field} - {Error}", e.Field, e.Error);
                    }
                    TempData["Error"] = "Vui lòng kiểm tra các lỗi ở biểu mẫu và thử lại.";
                    await LoadCategoryOptions();
                    return Page();
                }

                // Set thông tin cơ bản
                Product.CreatedAt = DateTime.UtcNow;
                Product.CreatedBy = HttpContext.User?.Identity?.Name ?? "system";
                Product.ModifiedAt = DateTime.UtcNow;
                Product.ModifiedBy = HttpContext.User?.Identity?.Name ?? "system";
                Product.IsDeleted = false;

                // Lưu sản phẩm
                var success = await _productService.CreateProductAsync(Product);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products/List");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại.";
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại.");
                _logger.LogError(ex, "Error creating product {ProductName}", Product?.ProductName);
            }

            await LoadCategoryOptions();
            return Page();
        }

        private async Task LoadCategoryOptions()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                CategoryOptions = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories: create page");
                CategoryOptions = new List<SelectListItem>();
            }
        }
    }
}

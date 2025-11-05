using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffProductEditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductEditModel> _logger;

        public StaffProductEditModel(IProductService productService, ILogger<StaffProductEditModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public List<ProductDetail> ProductDetails { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadCategoryOptions();

            Product = await _productService.GetProductByIdAsync(id);
            if (Product == null) return NotFound();

            ProductDetails = Product.ProductDetails?.ToList() ?? new List<ProductDetail>();

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoryOptions();

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

                Product.ModifiedAt = DateTime.UtcNow;
                Product.ModifiedBy = username;

                // Xử lý ProductDetails
                foreach (var detail in ProductDetails)
                {
                    bool hasAnyField = !string.IsNullOrWhiteSpace(detail.Color) ||
                                       !string.IsNullOrWhiteSpace(detail.Size) ||
                                       !string.IsNullOrWhiteSpace(detail.Material) ||
                                       !string.IsNullOrWhiteSpace(detail.Origin) ||
                                       !string.IsNullOrWhiteSpace(detail.ImageUrl) ||
                                       !string.IsNullOrWhiteSpace(detail.Description) ||
                                       detail.AdditionalPrice.HasValue;

                    if (hasAnyField)
                    {
                        detail.ModifiedAt = DateTime.UtcNow;
                        detail.ModifiedBy = username;
                    }
                }

                Product.ProductDetails = ProductDetails;

                var success = await _productService.UpdateProductAsync(Product);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products/List");
                }

                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sản phẩm.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductName}", Product?.ProductName);
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

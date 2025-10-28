using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator")]
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
            // Kiểm tra quyền Staff
            

            if (!ModelState.IsValid)
            {
                await LoadCategoryOptions();
                return Page();
            }

            try
            {
                // Set thông tin cơ bản
                Product.CreatedAt = DateTime.UtcNow;
                Product.CreatedBy = HttpContext.User?.Identity?.Name ?? "system";
                Product.ModifiedAt = DateTime.UtcNow;
                Product.ModifiedBy = HttpContext.User?.Identity?.Name ?? "system";
                Product.IsDeleted = false;

                // Thiết lập CategoryName dựa trên CategoryId để đảm bảo ràng buộc dữ liệu
                var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError(string.Empty, "Danh mục không hợp lệ.");
                    await LoadCategoryOptions();
                    return Page();
                }
                Product.CategoryName = category.CategoryName;

                // Lưu sản phẩm
                var success = await _productService.CreateProductAsync(Product);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products");
                }
                else
                {
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

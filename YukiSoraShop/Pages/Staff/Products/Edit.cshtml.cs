using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Domain.Entities;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator")]
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

        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductEntityByIdAsync(id);
            if (product == null) return RedirectToPage("/Staff/Products/List");
            Product = product;
            await LoadCategoryOptions();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // ensure CategoryName stays in sync before validate (field not bound from form)
                var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError("Product.CategoryId", "Danh mục không hợp lệ.");
                    await LoadCategoryOptions();
                    return Page();
                }
                Product.CategoryName = (category.CategoryName ?? string.Empty).Trim();

                ModelState.Remove("Product.CategoryName");
                if (!TryValidateModel(Product))
                {
                    await LoadCategoryOptions();
                    return Page();
                }

                Product.ModifiedAt = DateTime.UtcNow;
                Product.ModifiedBy = HttpContext.User?.Identity?.Name ?? "system";

                var ok = await _productService.UpdateProductAsync(Product);
                if (ok)
                {
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products/List");
                }
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sản phẩm.";
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", Product?.Id);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật sản phẩm.");
            }

            await LoadCategoryOptions();
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

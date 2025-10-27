using Application.Models;
using Application.Services.Interfaces;
using YukiSoraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YukiSoraShop.Pages.Staff.Products
{
    public class CreateModel : PageModel
    {
        private readonly IAuthorizationService _authService;
        private readonly IProductService _productService;

        public CreateModel(IAuthorizationService authService, IProductService productService)
        {
            _authService = authService;
            _productService = productService;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền Staff
            if (!_authService.IsStaff())
            {
                Response.Redirect("/Auth/Login");
                return Page();
            }

            await LoadCategoryOptions();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra quyền Staff
            if (!_authService.IsStaff())
            {
                Response.Redirect("/Auth/Login");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoryOptions();
                return Page();
            }

            try
            {
                // Set thông tin cơ bản
                Product.CreatedAt = DateTime.Now;
                Product.CreatedBy = _authService.GetCurrentUserName();
                Product.ModifiedAt = DateTime.Now;
                Product.ModifiedBy = _authService.GetCurrentUserName();
                Product.IsDeleted = false;

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
                Console.WriteLine($"Error creating product: {ex.Message}");
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
                Console.WriteLine($"Error loading categories: {ex.Message}");
                CategoryOptions = new List<SelectListItem>();
            }
        }
    }
}

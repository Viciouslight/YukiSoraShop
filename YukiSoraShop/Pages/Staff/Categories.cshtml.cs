using Application.Models;
using Application.Services.Interfaces;
using YukiSoraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Staff
{
    public class CategoriesModel : PageModel
    {
        private readonly IAuthorizationService _authService;
        private readonly IProductService _productService;

        public CategoriesModel(IAuthorizationService authService, IProductService productService)
        {
            _authService = authService;
            _productService = productService;
        }

        public List<Category> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền Staff
            if (!_authService.IsStaff())
            {
                Response.Redirect("/Auth/Login");
                return Page();
            }

            try
            {
                // Lấy danh sách danh mục
                Categories = await _productService.GetAllCategoriesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading categories: {ex.Message}");
                Categories = new List<Category>();
            }

            return Page();
        }
    }
}

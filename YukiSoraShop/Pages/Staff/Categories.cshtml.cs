using Application.Models;
using Application.Services.Interfaces;
using YukiSoraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Staff
{
    public class CategoriesModel : PageModel
    {
        private readonly IAuthorizationService _authService;
        private readonly IProductService _productService;
        private readonly ILogger<CategoriesModel> _logger;

        public CategoriesModel(IAuthorizationService authService, IProductService productService, ILogger<CategoriesModel> logger)
        {
            _authService = authService;
            _productService = productService;
            _logger = logger;
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
                _logger.LogError(ex, "Error loading categories list");
                Categories = new List<Category>();
            }

            return Page();
        }
    }
}

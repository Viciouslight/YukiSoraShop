using Application.Models;
using Application.Services.Interfaces;
using YukiSoraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Staff
{
    public class ProductsModel : PageModel
    {
        private readonly IAuthorizationService _authService;
        private readonly IProductService _productService;
        private readonly ILogger<ProductsModel> _logger;

        public ProductsModel(IAuthorizationService authService, IProductService productService, ILogger<ProductsModel> logger)
        {
            _authService = authService;
            _productService = productService;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new();

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
                // Lấy danh sách sản phẩm
                Products = await _productService.GetAllProductsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products list");
                Products = new List<Product>();
            }

            return Page();
        }
    }
}

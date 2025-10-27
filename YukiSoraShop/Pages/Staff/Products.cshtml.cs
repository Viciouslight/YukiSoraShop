using Application.Models;
using Application.Services.Interfaces;
using YukiSoraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Staff
{
    public class ProductsModel : PageModel
    {
        private readonly IAuthorizationService _authService;
        private readonly IProductService _productService;

        public ProductsModel(IAuthorizationService authService, IProductService productService)
        {
            _authService = authService;
            _productService = productService;
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
                // Log error
                Console.WriteLine($"Error loading products: {ex.Message}");
                Products = new List<Product>();
            }

            return Page();
        }
    }
}

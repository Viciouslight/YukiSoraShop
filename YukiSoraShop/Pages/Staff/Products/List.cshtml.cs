using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator")]
    public class StaffProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductsModel> _logger;

        public StaffProductsModel(IProductService productService, ILogger<StaffProductsModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền Staff
            

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

using Application.Services.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Customer
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Product> Products { get; set; } = new();

        public void OnGet()
        {
            Products = _productService.GetAllProducts();
        }
    }
}

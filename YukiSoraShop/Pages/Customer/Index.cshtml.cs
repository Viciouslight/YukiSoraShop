using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Models;
using YukiSoraShop.Services;

namespace YukiSoraShop.Pages
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

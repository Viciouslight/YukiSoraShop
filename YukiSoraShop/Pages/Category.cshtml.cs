using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Models;
using YukiSoraShop.Data;
namespace YukiSoraShop.Pages
{
    public class CategoryModel : PageModel
    {
        public List<string> Categories { get; set; }
        public List<Product> Products { get; set; }

        public void OnGet(string category)
        {
            var allProducts = ProductData.GetProducts();
            Categories = allProducts.Select(p => p.Category).Distinct().ToList();

            Products = string.IsNullOrEmpty(category)
                ? new List<Product>()
                : allProducts.Where(p => p.Category == category).ToList();
        }
    }
}

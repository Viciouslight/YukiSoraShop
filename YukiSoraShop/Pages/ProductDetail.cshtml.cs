using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Models;
using YukiSoraShop.Data;
namespace YukiSoraShop.Pages
{
    public class ProductDetailModel : PageModel
    {
        public Product Product { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = ProductData.GetProducts().FirstOrDefault(p => p.Id == id);
            if (Product == null) return NotFound();
            return Page();
        }
    }
}

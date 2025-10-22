using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public class ProductViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Category { get; set; }
        }
        public IList<ProductViewModel> Products { get; set; }

        public void OnGet()
        {
            Products = new List<ProductViewModel>
            {
                new ProductViewModel { Id = 101, Name = "Áo Thun Unisex", Price = 199000, StockQuantity = 150, Category = "Th?i trang" },
                new ProductViewModel { Id = 102, Name = "S?c D? Phòng 10000mAh", Price = 250000, StockQuantity = 80, Category = "?i?n t?" },
                new ProductViewModel { Id = 103, Name = "Bàn Phím C?", Price = 899000, StockQuantity = 35, Category = "Ph? ki?n PC" },
                new ProductViewModel { Id = 104, Name = "Tai Nghe Không Dây X1", Price = 450000, StockQuantity = 120, Category = "?i?n t?" }
            };
        }
    }
}

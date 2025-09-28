using YukiSoraShop.Models;

namespace YukiSoraShop.Data

{
    public class ProductData
    {
        public static List<Product> GetProducts() => new List<Product>
{
    new Product
    {
        Id = 1,
        Name = "Dây chuyền vàng 24K",
        Category = "Dây chuyền",
        Description = "Dây chuyền vàng sang trọng, phù hợp đi tiệc hoặc làm quà tặng.",
        Price = 1500000,
        ImageUrl = "/images/daychuyennu.jpg",
        Material = "Vàng 24K",
        Size = "45cm",
        Gender = "Nữ",
        StockQuantity = 12,
        Rating = 4.8,
        Tags = new List<string> { "sang trọng", "quà tặng", "vàng" }
    },
    new Product
    {
        Id = 2,
        Name = "Vòng tay bạc nữ",
        Category = "Vòng",
        Description = "Vòng tay bạc nhẹ nhàng, tinh tế, phù hợp với phong cách tối giản.",
        Price = 800000,
        ImageUrl = "/images/vongtaybacnu.jpg",
        Material = "Bạc S925",
        Size = "16cm",
        Gender = "Nữ",
        StockQuantity = 20,
        Rating = 4.5,
        Tags = new List<string> { "bạc", "nữ tính", "tối giản" }
    },
    new Product
    {
        Id = 3,
        Name = "Nhẫn kim cương",
        Category = "Nhẫn",
        Description = "Nhẫn kim cương cao cấp, thiết kế độc đáo, tôn vinh vẻ đẹp quý phái.",
        Price = 5000000,
        ImageUrl = "/images/nhankimcuong.jpg",
        Material = "Vàng trắng + Kim cương",
        Size = "Size 6",
        Gender = "Unisex",
        StockQuantity = 5,
        Rating = 4.9,
        Tags = new List<string> { "kim cương", "cao cấp", "sang trọng" }
    }
};

    }
}

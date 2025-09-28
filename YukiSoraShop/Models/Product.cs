namespace YukiSoraShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }         // Tên sản phẩm
        public string Category { get; set; }     // Phân loại: Dây chuyền, vòng, nhẫn...
        public string Description { get; set; }  // Mô tả
        public decimal Price { get; set; }       // Giá
        public string ImageUrl { get; set; }     // Đường dẫn ảnh

        // Thêm chi tiết
        public string Material { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public int StockQuantity { get; set; }
        public double Rating { get; set; }
        public List<string> Tags { get; set; }
    }
}

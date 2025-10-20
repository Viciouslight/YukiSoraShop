namespace Application.DTOs
{
    public class CartItemDto
    {
        public ProductDto Product { get; set; } = new ProductDto();
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
    }
}


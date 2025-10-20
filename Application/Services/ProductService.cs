using Application.DTOs;
using Application.Services.Interfaces;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private static readonly List<ProductDto> _products = new()
        {
            new ProductDto
            {
                Id = 1,
                Name = "iPhone 15 Pro",
                Description = "Latest iPhone with advanced camera system",
                Price = 25000000,
                ImageUrl = "https://via.placeholder.com/300x300/007bff/ffffff?text=iPhone+15",
                Category = "Electronics",
                Stock = 50,
                IsAvailable = true
            },
            new ProductDto
            {
                Id = 2,
                Name = "Samsung Galaxy S24",
                Description = "Flagship Android smartphone with AI features",
                Price = 22000000,
                ImageUrl = "https://via.placeholder.com/300x300/28a745/ffffff?text=Galaxy+S24",
                Category = "Electronics",
                Stock = 30,
                IsAvailable = true
            },
            new ProductDto
            {
                Id = 3,
                Name = "MacBook Air M3",
                Description = "Ultra-thin laptop with M3 chip",
                Price = 35000000,
                ImageUrl = "https://via.placeholder.com/300x300/6c757d/ffffff?text=MacBook+Air",
                Category = "Computers",
                Stock = 20,
                IsAvailable = true
            },
            new ProductDto
            {
                Id = 4,
                Name = "Dell XPS 13",
                Description = "Premium Windows laptop for professionals",
                Price = 28000000,
                ImageUrl = "https://via.placeholder.com/300x300/dc3545/ffffff?text=Dell+XPS",
                Category = "Computers",
                Stock = 15,
                IsAvailable = true
            },
            new ProductDto
            {
                Id = 5,
                Name = "AirPods Pro",
                Description = "Wireless earbuds with noise cancellation",
                Price = 6000000,
                ImageUrl = "https://via.placeholder.com/300x300/17a2b8/ffffff?text=AirPods",
                Category = "Accessories",
                Stock = 100,
                IsAvailable = true
            }
        };

        public ProductDto? GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public List<ProductDto> GetAllProducts()
        {
            return _products;
        }

        public List<ProductDto> GetProductsByCategory(string category)
        {
            return _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}

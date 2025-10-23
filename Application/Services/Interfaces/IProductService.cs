using Application.DTOs;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        // DTO methods for display
        ProductDto? GetProductById(int id);
        List<ProductDto> GetAllProducts();
        List<ProductDto> GetProductsByCategory(string category);
        
        // Entity methods for CRUD operations
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductEntityByIdAsync(int id);
        Task<bool> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        
        // Category methods
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
    }
}

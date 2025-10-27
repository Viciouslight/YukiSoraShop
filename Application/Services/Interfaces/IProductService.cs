using Application.DTOs;
using Application.Models;
using System.Collections.Generic;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        // DTO methods for display
        List<ProductDto> GetAllProducts();
        List<ProductDto> GetProductsByCategory(string category);
        List<ProductDto> GetProductsByName(string Name);


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

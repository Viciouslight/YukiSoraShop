using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        // DTO methods for display (async)
        Task<List<ProductDto>> GetAllProductsDtoAsync();
        Task<List<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<List<ProductDto>> GetProductsByNameAsync(string name);
        Task<ProductDto?> GetProductDtoByIdAsync(int id);


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

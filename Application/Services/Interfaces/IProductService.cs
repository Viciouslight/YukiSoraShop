using Application.DTOs;
using Application.DTOs.Pagination;
using Domain.Entities;
using System.Collections.Generic;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        // DTO methods for display (async)
        Task<List<ProductDTO>> GetAllProductsDtoAsync();
        Task<PagedResult<ProductDTO>> GetProductsPagedAsync(int pageNumber, int pageSize, string? search = null, string? category = null);
        Task<List<ProductDTO>> GetProductsByCategoryAsync(string category);
        Task<List<ProductDTO>> GetProductsByNameAsync(string name);
        Task<ProductDTO?> GetProductDtoByIdAsync(int id);


        // Entity methods for CRUD operations
        Task<List<Product>> GetAllProductsAsync();
        Task<PagedResult<Product>> GetProductsPagedEntitiesAsync(int pageNumber, int pageSize, string? search = null, string? category = null);
        Task<Product?> GetProductEntityByIdAsync(int id);
        Task<bool> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        
        // Category methods
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<int> GetTotalProductsAsync();
    }
}

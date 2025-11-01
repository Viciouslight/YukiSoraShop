using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Application.Extensions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // DTO methods for display (async)
        public async Task<List<ProductDTO>> GetAllProductsDtoAsync()
        {
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails");
            return await query.ProjectTo<ProductDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<PagedResult<ProductDTO>> GetProductsPagedAsync(int pageNumber, int pageSize, string? search = null, string? category = null)
        {
            var filter = new PaginationFilter { PageNumber = pageNumber, PageSize = pageSize, Search = search, Category = category };
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails")
                .FilterBySearch(filter.Search)
                .FilterByCategory(filter.Category)
                .OrderByDescending(p => p.Id)
                .ProjectTo<ProductDTO>(_mapper.ConfigurationProvider);
            return await query.ToPagedResultAsync(filter.PageNumber, filter.PageSize);
        }

        public async Task<ProductDTO?> GetProductDtoByIdAsync(int id)
        {
            var product = await _uow.ProductRepository.FindOneAsync(p => p.Id == id, includeProperties: "ProductDetails");
            if (product == null) return null;
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<List<ProductDTO>> GetProductsByNameAsync(string name)
        {
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails").FilterBySearch(name);
            return await query.ProjectTo<ProductDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<List<ProductDTO>> GetProductsByCategoryAsync(string category)
        {
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails").FilterByCategory(category);
            return await query.ProjectTo<ProductDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        // Entity methods for CRUD operations
        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _uow.ProductRepository.GetAllAsync();
                return products.ToList();
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        public async Task<PagedResult<Product>> GetProductsPagedEntitiesAsync(int pageNumber, int pageSize, string? search = null, string? category = null)
        {
            var filter = new PaginationFilter { PageNumber = pageNumber, PageSize = pageSize, Search = search, Category = category };
            var query = _uow.ProductRepository.GetAllQueryable()
                .AsNoTracking()
                .FilterBySearch(filter.Search)
                .FilterByCategory(filter.Category)
                .OrderByDescending(p => p.Id);
            return await Application.Extensions.QueryableExtensions.ToPagedResultAsync(query, filter.PageNumber, filter.PageSize);
        }

        public async Task<Product?> GetProductEntityByIdAsync(int id)
        {
            try
            {
                return await _uow.ProductRepository.GetByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CreateProductAsync(Product product)
        {
            try
            {
                await _uow.ProductRepository.AddAsync(product);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                _uow.ProductRepository.Update(product);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _uow.ProductRepository.GetByIdAsync(id);
                if (product != null)
                {
                    _uow.ProductRepository.SoftDelete(product);
                    await _uow.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Category methods
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _uow.CategoryRepository.GetAllAsync();
                return categories.ToList();
            }
            catch (Exception)
            {
                return new List<Category>();
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _uow.CategoryRepository.GetByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _uow.ProductRepository.GetCountAsync();
        }
    }
}

using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Pagination;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;

        public ProductService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // DTO methods for display (async)
        public async Task<List<ProductDTO>> GetAllProductsDtoAsync()
        {
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails");
            return await query
                .Select(product => new ProductDTO
                {
                    Id = product.Id,
                    Name = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ProductDetails.Select(pd => pd.ImageUrl).FirstOrDefault(),
                    Category = product.CategoryName,
                    Stock = product.StockQuantity,
                    IsAvailable = !product.IsDeleted,
                })
                .ToListAsync();
        }

        public async Task<PagedResult<ProductDTO>> GetProductsPagedAsync(int pageNumber, int pageSize, string? search = null, string? category = null)
        {
            var filter = new PaginationFilter { PageNumber = pageNumber, PageSize = pageSize, Search = search, Category = category };
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails");
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => EF.Functions.Like(p.ProductName, $"%{filter.Search}%"));
            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(p => p.CategoryName == filter.Category);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(product => new ProductDTO
                {
                    Id = product.Id,
                    Name = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ProductDetails.Select(pd => pd.ImageUrl).FirstOrDefault(),
                    Category = product.CategoryName,
                    Stock = product.StockQuantity,
                    IsAvailable = !product.IsDeleted,
                })
                .ToListAsync();

            return new PagedResult<ProductDTO>
            {
                Items = items,
                TotalItems = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ProductDTO?> GetProductDtoByIdAsync(int id)
        {
            var product = await _uow.ProductRepository.FindOneAsync(p => p.Id == id, includeProperties: "ProductDetails");
            if (product == null) return null;
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ProductDetails.Select(pd => pd.ImageUrl).FirstOrDefault(),
                Category = product.CategoryName,
                Stock = product.StockQuantity,
                IsAvailable = !product.IsDeleted,
            };
        }

        public async Task<List<ProductDTO>> GetProductsByNameAsync(string name)
        {
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails");
            return await query
                .Where(p => EF.Functions.Like(p.ProductName, $"%{name}%"))
                .Select(product => new ProductDTO
                {
                    Id = product.Id,
                    Name = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ProductDetails.Select(pd => pd.ImageUrl).FirstOrDefault(),
                    Category = product.CategoryName,
                    Stock = product.StockQuantity,
                    IsAvailable = !product.IsDeleted,
                })
                .ToListAsync();
        }

        public async Task<List<ProductDTO>> GetProductsByCategoryAsync(string category)
        {
            var query = _uow.ProductRepository.GetAllQueryable("ProductDetails");
            return await query
                .Where(p => p.CategoryName == category)
                .Select(product => new ProductDTO
                {
                    Id = product.Id,
                    Name = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ProductDetails.Select(pd => pd.ImageUrl).FirstOrDefault(),
                    Category = product.CategoryName,
                    Stock = product.StockQuantity,
                    IsAvailable = !product.IsDeleted,
                })
                .ToListAsync();
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
            var query = _uow.ProductRepository.GetAllQueryable();
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => EF.Functions.Like(p.ProductName, $"%{filter.Search}%"));
            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(p => p.CategoryName == filter.Category);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalItems = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
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

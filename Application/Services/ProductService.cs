using Application.DTOs;
using Application.Models;
using Application.Services.Interfaces;
using Application.IRepository;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public List<ProductDto> GetAllProducts()
        {
            var product = _productRepository.GetAllAsync().Result;
            return product.Select(product => new ProductDto
            {
                Id = product.Id,
                Name = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ProductDetails.FirstOrDefault()?.ImageUrl,
                Category = product.CategoryName,
                Stock = product.StockQuantity,
                IsAvailable = product.IsDeleted,
            }).ToList();
        }

        // DTO methods for display (using static data for now)
        public List<ProductDto> GetProductsByName(string name)
        {
            var products = _productRepository.GetAllAsync().Result;

            return products
                .Where(p => p.ProductName.Contains(name, StringComparison.OrdinalIgnoreCase)) // lọc theo tên
                .Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ProductDetails.FirstOrDefault()?.ImageUrl,
                    Category = product.CategoryName,
                    Stock = product.StockQuantity,
                    IsAvailable = !product.IsDeleted,
                })
                .ToList();
        }

        public List<ProductDto> GetProductsByCategory(string category)
        {
            var products = _productRepository.GetAllAsync().Result;

            return products
                .Where(p => p.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase)) // lọc theo category
                .Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ProductDetails.FirstOrDefault()?.ImageUrl,
                    Category = product.CategoryName,
                    Stock = product.StockQuantity,
                    IsAvailable = !product.IsDeleted,
                })
                .ToList();
        }


        // Entity methods for CRUD operations
        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return products.ToList();
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductEntityByIdAsync(int id)
        {
            try
            {
                return await _productRepository.GetByIdAsync(id);
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
                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();
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
                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();
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
                var product = await _productRepository.GetByIdAsync(id);
                if (product != null)
                {
                    _productRepository.SoftDelete(product);
                    await _productRepository.SaveChangesAsync();
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
                var categories = await _categoryRepository.GetAllAsync();
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
                return await _categoryRepository.GetByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

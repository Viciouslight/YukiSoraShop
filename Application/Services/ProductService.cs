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

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // DTO methods for display (using static data for now)
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

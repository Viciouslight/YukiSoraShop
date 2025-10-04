using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        Product? GetProductById(int id);
        List<Product> GetAllProducts();
        List<Product> GetProductsByCategory(string category);
    }
}

using YukiSoraShop.Models;

namespace YukiSoraShop.Services
{
    public interface IProductService
    {
        Product? GetProductById(int id);
        List<Product> GetAllProducts();
        List<Product> GetProductsByCategory(string category);
    }
}

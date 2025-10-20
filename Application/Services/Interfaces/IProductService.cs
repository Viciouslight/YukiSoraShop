using Application.DTOs;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        ProductDto? GetProductById(int id);
        List<ProductDto> GetAllProducts();
        List<ProductDto> GetProductsByCategory(string category);
    }
}

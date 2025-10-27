using Application.Models;

namespace Application.IRepository;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    Task<Category> GetCategoryWithProductsAsync(int id);
}

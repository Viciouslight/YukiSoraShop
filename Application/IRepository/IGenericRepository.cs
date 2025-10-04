using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepository
{
    public interface IGenericRepository<TModel> where TModel : BaseFullEntity
    {
        Task AddAsync(TModel model);
        void Update(TModel model);
        void Delete(TModel model);
        void SoftDelete(TModel model);
        Task<IEnumerable<TModel>> GetAllAsync();
        Task<TModel> GetByIdAsync(int id);
        IQueryable<TModel> GetAllQueryable(string includeProperties = "");
        Task<TModel> FindOneAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "");
    }
}

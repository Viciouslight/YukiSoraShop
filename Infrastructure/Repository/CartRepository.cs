using Application.IRepository;
using Application.Models;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly AppDbContext _appDbContext;
        public CartRepository(AppDbContext dbContext) : base(dbContext)
        {
            _appDbContext = dbContext;
        }
    }
}

using Application.IRepository;
using Application.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _appDbContext;
        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
            _appDbContext = dbContext;
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return  _appDbContext.Products
        .Include(p => p.ProductDetails)
        .ToListAsync();
        }
    }
}

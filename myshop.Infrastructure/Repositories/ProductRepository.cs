using Microsoft.EntityFrameworkCore;
using myshop.Application.Contracts;
using myshop.Domain.Entities;
using myshop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Product?> GetProductWithCategoryAsync(int id)
            => await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
            => await _db.Products.Include(p => p.Category).ToListAsync();
    }
}

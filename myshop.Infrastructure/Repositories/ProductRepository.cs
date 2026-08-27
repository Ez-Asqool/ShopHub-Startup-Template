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
            => await _db.Products.Include(p => p.Category).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
            => await _db.Products.Include(p => p.Category).AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> ids)
            => await _db.Products.Include(p => p.Category).AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync();

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            string? search, string? sortBy, int? categoryId, int pageNumber, int pageSize)
        {
            IQueryable<Product> query = _db.Products.Include(p => p.Category).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            query = sortBy switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetArchivedPagedAsync(
            string? search, int pageNumber, int pageSize)
        {
            IQueryable<Product> query = _db.Products
                .IgnoreQueryFilters()
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            query = query.OrderByDescending(p => p.UpdatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdIncludingDeletedAsync(int id)
            => await _db.Products.IgnoreQueryFilters().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }
}

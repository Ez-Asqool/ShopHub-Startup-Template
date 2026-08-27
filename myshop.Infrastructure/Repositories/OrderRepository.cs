using Microsoft.EntityFrameworkCore;
using myshop.Application.Contracts;
using myshop.Domain.Entities;
using myshop.Domain.Entities.Enums;
using myshop.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myshop.Infrastructure.Repositories
{
    public class OrderRepository : Repository<OrderHeader>, IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<OrderHeader>> GetOrdersByUserIdAsync(string userId)
            => await _db.OrderHeaders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .Where(o => o.ApplicationUserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();

        public async Task<OrderHeader?> GetOrderWithDetailsAsync(int id)
            => await _db.OrderHeaders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<(IEnumerable<OrderHeader> Items, int TotalCount)> GetAllPagedAsync(
            string? search, OrderStatus? status, int pageNumber, int pageSize)
        {
            IQueryable<OrderHeader> query = _db.OrderHeaders.Include(o => o.OrderDetails).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTrim = search.Trim();
                if (int.TryParse(searchTrim, out var searchId))
                    query = query.Where(o => o.Id == searchId || o.RecipientName.Contains(searchTrim));
                else
                    query = query.Where(o => o.RecipientName.Contains(searchTrim));
            }

            if (status.HasValue)
                query = query.Where(o => o.OrderStatus == status.Value);

            query = query.OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

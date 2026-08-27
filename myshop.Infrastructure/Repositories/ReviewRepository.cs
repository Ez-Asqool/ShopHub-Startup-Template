using Microsoft.EntityFrameworkCore;
using myshop.Application.Contracts;
using myshop.Domain.Entities;
using myshop.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myshop.Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        private readonly ApplicationDbContext _db;

        public ReviewRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
            => await _db.Reviews.AsNoTracking()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<Review?> GetByProductAndUserAsync(int productId, string userId)
            => await _db.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.ApplicationUserId == userId);
    }
}

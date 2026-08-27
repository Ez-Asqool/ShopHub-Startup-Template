using myshop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myshop.Application.Contracts
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task<Review?> GetByProductAndUserAsync(int productId, string userId);
    }
}

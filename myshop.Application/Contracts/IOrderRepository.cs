using myshop.Domain.Entities;
using myshop.Domain.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myshop.Application.Contracts
{
    public interface IOrderRepository : IRepository<OrderHeader>
    {
        Task<IEnumerable<OrderHeader>> GetOrdersByUserIdAsync(string userId);
        Task<OrderHeader?> GetOrderWithDetailsAsync(int id);
        Task<(IEnumerable<OrderHeader> Items, int TotalCount)> GetAllPagedAsync(
            string? search, OrderStatus? status, int pageNumber, int pageSize);
    }
}

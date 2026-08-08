using myshop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Contracts
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetProductWithCategoryAsync(int id);
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            string? search, string? sortBy, int? categoryId, int pageNumber, int pageSize);
    }
}

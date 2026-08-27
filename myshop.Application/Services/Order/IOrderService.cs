using myshop.Application.Common;
using myshop.Application.Services.Order.Dto;
using myshop.Domain.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myshop.Application.Services.Order
{
    public enum OrderOperationResult
    {
        Success,
        CartEmpty,
        InsufficientStock,
        Failure
    }

    public class OrderResult
    {
        public OrderOperationResult Status { get; set; }
        public List<string>? InsufficientStockProductNames { get; set; }
        public int? OrderId { get; set; }

        public static OrderResult Success(int? orderId = null) => new() { Status = OrderOperationResult.Success, OrderId = orderId };
        public static OrderResult CartEmpty() => new() { Status = OrderOperationResult.CartEmpty };
        public static OrderResult Failure() => new() { Status = OrderOperationResult.Failure };
        public static OrderResult InsufficientStock(List<string> productNames) =>
            new() { Status = OrderOperationResult.InsufficientStock, InsufficientStockProductNames = productNames };
    }

    public interface IOrderService
    {
        Task<OrderResult> CheckStockAsync();
        Task<OrderResult> CreateOrderAsync(CheckoutDto dto);
        Task<IEnumerable<OrderSummaryDto>> GetMyOrdersAsync(string userId);
        Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId, string userId);

        Task<PagedResult<AdminOrderSummaryDto>> GetAllOrdersPagedAsync(string? search, OrderStatus? status, int pageNumber, int pageSize);
        Task<OrderDetailsDto?> GetOrderDetailsForAdminAsync(int orderId);
        Task<OrderStatsDto> GetOrderStatsAsync();
    }
}

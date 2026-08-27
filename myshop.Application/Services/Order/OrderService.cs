using myshop.Application.Common;
using myshop.Application.Contracts;
using myshop.Application.Services.Order.Dto;
using myshop.Domain.Entities;
using myshop.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myshop.Application.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartService _cartService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICartService cartService,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _cartService = cartService;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResult> CheckStockAsync()
        {
            try
            {
                var cart = _cartService.GetCart();
                var (validationError, _) = await ValidateCartStockAsync(cart);
                return validationError ?? OrderResult.Success();
            }
            catch
            {
                return OrderResult.Failure();
            }
        }

        public async Task<OrderResult> CreateOrderAsync(CheckoutDto dto)
        {
            try
            {
                var cart = _cartService.GetCart();
                var (validationError, products) = await ValidateCartStockAsync(cart);
                if (validationError != null)
                    return validationError;

                SimulateFakePayment();

                var orderHeader = new OrderHeader
                {
                    ApplicationUserId = dto.UserId,
                    RecipientName = dto.RecipientName,
                    Address = dto.Address,
                    City = dto.City,
                    PhoneNumber = dto.PhoneNumber,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = OrderStatus.Processing,
                    PaymentStatus = PaymentStatus.Paid
                };

                foreach (var item in cart)
                {
                    var product = products[item.ProductId];
                    orderHeader.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                orderHeader.TotalPrice = orderHeader.OrderDetails.Sum(d => d.UnitPrice * d.Quantity);

                foreach (var item in cart)
                {
                    var product = products[item.ProductId];
                    product.Stock -= item.Quantity;
                    _productRepository.Update(product);
                }

                await _orderRepository.AddAsync(orderHeader);
                await _unitOfWork.SaveChangesAsync();

                _cartService.ClearCart();

                return OrderResult.Success(orderHeader.Id);
            }
            catch
            {
                return OrderResult.Failure();
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetMyOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                ItemCount = o.OrderDetails.Count
            });
        }

        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId, string userId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.ApplicationUserId != userId)
                return null;

            return MapToDetailsDto(order);
        }

        public async Task<PagedResult<AdminOrderSummaryDto>> GetAllOrdersPagedAsync(string? search, OrderStatus? status, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 6;

            var (items, totalCount) = await _orderRepository.GetAllPagedAsync(search, status, pageNumber, pageSize);

            return new PagedResult<AdminOrderSummaryDto>
            {
                Items = items.Select(o => new AdminOrderSummaryDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    ItemCount = o.OrderDetails.Count,
                    RecipientName = o.RecipientName,
                    City = o.City
                }),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<OrderDetailsDto?> GetOrderDetailsForAdminAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            return order == null ? null : MapToDetailsDto(order);
        }

        public async Task<OrderStatsDto> GetOrderStatsAsync()
        {
            var orders = (await _orderRepository.GetAllAsync()).ToList();

            return new OrderStatsDto
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalPrice),
                ProcessingCount = orders.Count(o => o.OrderStatus == OrderStatus.Processing),
                DeliveredCount = orders.Count(o => o.OrderStatus == OrderStatus.Delivered)
            };
        }

        private static OrderDetailsDto MapToDetailsDto(OrderHeader order) => new()
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice,
            OrderStatus = order.OrderStatus,
            PaymentStatus = order.PaymentStatus,
            RecipientName = order.RecipientName,
            Address = order.Address,
            City = order.City,
            PhoneNumber = order.PhoneNumber,
            Items = order.OrderDetails.Select(d => new OrderLineItemDto
            {
                ProductId = d.ProductId,
                ProductName = d.Product.Name,
                ProductImg = d.Product.Img,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity
            }).ToList()
        };

        private async Task<(OrderResult? Error, Dictionary<int, Domain.Entities.Product> Products)> ValidateCartStockAsync(List<myshop.Application.Models.CartItem> cart)
        {
            var products = new Dictionary<int, Domain.Entities.Product>();

            if (cart == null || cart.Count == 0)
                return (OrderResult.CartEmpty(), products);

            var insufficientStockProductNames = new List<string>();
            foreach (var item in cart)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || product.Stock < item.Quantity)
                {
                    insufficientStockProductNames.Add(product?.Name ?? item.ProductName);
                    continue;
                }

                products[item.ProductId] = product;
            }

            if (insufficientStockProductNames.Count > 0)
                return (OrderResult.InsufficientStock(insufficientStockProductNames), products);

            return (null, products);
        }

        private void SimulateFakePayment()
        {
        }
    }
}

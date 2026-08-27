using myshop.Domain.Entities.Enums;
using System;
using System.Collections.Generic;

namespace myshop.Application.Services.Order.Dto
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string RecipientName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string? PhoneNumber { get; set; }
        public List<OrderLineItemDto> Items { get; set; } = new();
    }

    public class OrderLineItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImg { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}

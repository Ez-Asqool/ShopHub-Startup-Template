using myshop.Domain.Entities.Enums;
using System;

namespace myshop.Application.Services.Order.Dto
{
    public class AdminOrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int ItemCount { get; set; }
        public string RecipientName { get; set; }
        public string City { get; set; }
    }
}

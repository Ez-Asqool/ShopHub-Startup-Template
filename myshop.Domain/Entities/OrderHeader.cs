using myshop.Domain.Common;
using myshop.Domain.Entities.Enums;
using System;
using System.Collections.Generic;

namespace myshop.Domain.Entities
{
    public class OrderHeader : BaseEntity
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        //User Data
        public string RecipientName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string? PhoneNumber { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}

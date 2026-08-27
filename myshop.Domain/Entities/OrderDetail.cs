using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using myshop.Domain.Common;

namespace myshop.Domain.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int Id { get; set; }

        public int OrderHeaderId { get; set; }
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }

        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }
    }
}

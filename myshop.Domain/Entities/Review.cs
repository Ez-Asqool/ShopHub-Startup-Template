using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using myshop.Domain.Common;

namespace myshop.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }

        public string ApplicationUserId { get; set; }
        public string ReviewerName { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}

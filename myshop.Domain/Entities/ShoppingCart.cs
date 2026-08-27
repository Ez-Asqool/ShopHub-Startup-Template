using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using myshop.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Domain.Entities
{
    public class ShoppingCart : BaseEntity
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public int Count { get; set; }

        public string ApplicationUserId { get; set; }

        //[ForeignKey("ApplicationUserId")]
        //[ValidateNever]
        //public ApplicationUser ApplicationUser { get; set; }
    }
}

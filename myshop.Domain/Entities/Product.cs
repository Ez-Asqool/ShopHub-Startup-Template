using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using myshop.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Domain.Entities
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        [DisplayName("Image")]
        [ValidateNever]
        public string Img { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }

        public int Stock { get; set; }
    }
}

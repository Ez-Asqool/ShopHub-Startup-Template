using System.ComponentModel.DataAnnotations;
using myshop.Domain.Common;

namespace myshop.Domain.Entities
{
    public class Category : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}

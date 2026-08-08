using System;

namespace myshop.Application.Services.Category.Dto
{
    public class CategoryAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ProductCount { get; set; }
    }
}

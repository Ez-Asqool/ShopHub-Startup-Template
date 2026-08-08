using System;

namespace myshop.Application.Services.Category.Dto
{
    public class CategoryStatsDto
    {
        public int TotalCategories { get; set; }
        public string? NewestName { get; set; }
        public DateTime? NewestCreatedTime { get; set; }
        public double AvgProductsPerCategory { get; set; }
        public int EmptyCategoriesCount { get; set; }
    }
}

namespace myshop.Application.Services.Product.Dto
{
    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public decimal CatalogValue { get; set; }
        public decimal AvgPrice { get; set; }
        public int CategoriesCount { get; set; }
    }
}

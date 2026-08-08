using myshop.Application.Services.Category.Dto;
using myshop.Application.Services.Product.Dto;

namespace myshop.Web.ViewModels
{
    public class HomeIndexVM
    {
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public string? Search { get; set; }
    }
}

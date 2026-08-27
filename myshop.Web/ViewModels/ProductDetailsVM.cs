using myshop.Application.Services.Product.Dto;
using myshop.Application.Services.Review.Dto;

namespace myshop.Web.ViewModels
{
    public class ProductDetailsVM
    {
        public ProductDto Product { get; set; }
        public ProductReviewsDto Reviews { get; set; }
    }
}

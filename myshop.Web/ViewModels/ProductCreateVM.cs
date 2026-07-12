using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using myshop.Application.Services.Product.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myshop.Web.ViewModels
{
    public class ProductCreateVM
    {
        public ProductCreateDto Product { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}

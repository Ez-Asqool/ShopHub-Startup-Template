using myshop.Application.Services.Category.Dto;
using myshop.Application.Services.Product.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Services.Product
{
    public enum ProductOperationResult
    {
        Success,
        NotFound,
        InvalidCategory
    }

    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductUpdateDto?> GetProductForEditAsync(int id);
        Task<ProductOperationResult> CreateProductAsync(ProductCreateDto dto, Stream? imageStream, string? imageExtension);
        Task<ProductOperationResult> UpdateProductAsync(ProductUpdateDto dto, Stream? imageStream, string? imageExtension);
        Task<ProductOperationResult> DeleteProductAsync(int id);
        Task<IEnumerable<CategoryLookupDto>> GetCategoriesForDropdownAsync();

    }
}

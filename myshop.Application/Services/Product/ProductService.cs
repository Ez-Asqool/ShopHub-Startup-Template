using AutoMapper;
using myshop.Application.Common;
using myshop.Application.Contracts;
using myshop.Application.Services.Category.Dto;
using myshop.Application.Services.Product.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private const string ProductImageFolder = "uploads/products";

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IFileService fileService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _fileService = fileService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PagedResult<ProductDto>> GetProductsPagedAsync(string? search, string? sortBy, int? categoryId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 6;

            var (items, totalCount) = await _productRepository.GetPagedAsync(search, sortBy, categoryId, pageNumber, pageSize);

            return new PagedResult<ProductDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDto>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductWithCategoryAsync(id);
            if (product == null)
                return null;

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductUpdateDto?> GetProductForEditAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            return _mapper.Map<ProductUpdateDto>(product);
        }

        public async Task<IEnumerable<CategoryLookupDto>> GetCategoriesForDropdownAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryLookupDto>>(categories);
        }

        public async Task<ProductStatsDto> GetStatsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var productList = products.ToList();
            var totalProducts = productList.Count;
            var catalogValue = productList.Sum(p => p.Price);

            return new ProductStatsDto
            {
                TotalProducts = totalProducts,
                CatalogValue = catalogValue,
                AvgPrice = totalProducts > 0 ? catalogValue / totalProducts : 0,
                CategoriesCount = categories.Count()
            };
        }

        public async Task<ProductOperationResult> CreateProductAsync(
            ProductCreateDto dto, Stream? imageStream, string? imageExtension)
        {
            bool categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return ProductOperationResult.InvalidCategory;

            var product = _mapper.Map<Domain.Entities.Product>(dto);

            if (imageStream != null && imageExtension != null)
            {
                var imageError = ValidateImage(imageStream, imageExtension, imageStream.Length);
                if (imageError != null)
                    return imageError.Value;

                product.Img = await _fileService.SaveImageAsync(imageStream, imageExtension, ProductImageFolder);
            }
            else
            {
                product.Img = string.Empty;
            }

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return ProductOperationResult.Success;
        }

        public async Task<ProductOperationResult> UpdateProductAsync(
            ProductUpdateDto dto, Stream? imageStream, string? imageExtension)
        {
            var existing = await _productRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                return ProductOperationResult.NotFound;

            bool categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return ProductOperationResult.InvalidCategory;

            if (imageStream != null && imageExtension != null)
            {
                var imageError = ValidateImage(imageStream, imageExtension, imageStream.Length);
                if (imageError != null)
                    return imageError.Value;

                if (!string.IsNullOrEmpty(existing.Img))
                    _fileService.DeleteImage(existing.Img);

                dto.Img = await _fileService.SaveImageAsync(imageStream, imageExtension, ProductImageFolder);
            }
            else
            {
                dto.Img = existing.Img;
            }

            _mapper.Map(dto, existing);
            _productRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return ProductOperationResult.Success;
        }

        public async Task<ProductOperationResult> DeleteProductAsync(int id)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null)
                return ProductOperationResult.NotFound;

            if (!string.IsNullOrEmpty(existing.Img))
                _fileService.DeleteImage(existing.Img);

            _productRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            return ProductOperationResult.Success;
        }

        private ProductOperationResult? ValidateImage(Stream imageStream, string imageExtension, long imageSizeInBytes)
        {
            var validationResult = _fileService.ValidateImage(imageStream, imageExtension, imageSizeInBytes);
            return validationResult switch
            {
                ImageValidationResult.InvalidExtension => ProductOperationResult.InvalidImageExtension,
                ImageValidationResult.FileTooLarge => ProductOperationResult.InvalidImageSize,
                ImageValidationResult.InvalidContent => ProductOperationResult.InvalidImageContent,
                _ => null
            };
        }
    }
}

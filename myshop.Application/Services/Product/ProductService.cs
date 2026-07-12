using AutoMapper;
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
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private const string ProductImageFolder = "Images/Products";

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IImageService imageService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
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

        public async Task<ProductOperationResult> CreateProductAsync(
            ProductCreateDto dto, Stream? imageStream, string? imageExtension)
        {
            bool categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return ProductOperationResult.InvalidCategory;

            var product = _mapper.Map<Domain.Entities.Product>(dto);

            if (imageStream != null && imageExtension != null)
            {
                product.Img = await _imageService.SaveImageAsync(imageStream, imageExtension, ProductImageFolder);
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
                if (!string.IsNullOrEmpty(existing.Img))
                    _imageService.DeleteImage(existing.Img);

                dto.Img = await _imageService.SaveImageAsync(imageStream, imageExtension, ProductImageFolder);
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
                _imageService.DeleteImage(existing.Img);

            _productRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            return ProductOperationResult.Success;
        }
    }
}

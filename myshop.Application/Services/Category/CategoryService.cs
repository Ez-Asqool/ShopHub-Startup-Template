using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using myshop.Application.Contracts;
using myshop.Application.Services.Category.Dto;
using myshop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private const string CategoriesCacheKey = "Categories:All";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMemoryCache cache)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            if (_cache.TryGetValue(CategoriesCacheKey, out IEnumerable<CategoryDto>? cached) && cached != null)
                return cached;

            var categories = await _categoryRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories).ToList();

            _cache.Set(CategoriesCacheKey, dtos, CacheDuration);
            return dtos;
        }

        public async Task<CategoryUpdateDto?> GetCategoryForEditAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            return _mapper.Map<CategoryUpdateDto>(category);
        }

        public async Task<bool> CreateCategoryAsync(CategoryCreateDto dto)
        {
            bool exists = await _categoryRepository.ExistsAsync(c => c.Name == dto.Name);
            if (exists)
                return false;

            var category = _mapper.Map<Domain.Entities.Category>(dto);
            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            return true;
        }

        public async Task<CategoryOperationResult> UpdateCategoryAsync(CategoryUpdateDto dto)
        {
            var existing = await _categoryRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                return CategoryOperationResult.NotFound;

            bool duplicateExists = await _categoryRepository.ExistsAsync(
                c => c.Name == dto.Name && c.Id != dto.Id);
            if (duplicateExists)
                return CategoryOperationResult.DuplicateName;

            _mapper.Map(dto, existing);
            _categoryRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            return CategoryOperationResult.Success;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var existing = await _categoryRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            _categoryRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            return true;
        }

        public async Task<IEnumerable<CategoryAdminDto>> GetCategoriesForAdminAsync(string? search, string? sort)
        {
            var categories = await _categoryRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            var counts = products
                .GroupBy(p => p.CategoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            var list = categories.Select(c => new CategoryAdminDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedTime = c.CreatedTime,
                ProductCount = counts.TryGetValue(c.Id, out var n) ? n : 0
            });

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                list = list.Where(c =>
                    c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            list = sort switch
            {
                "oldest" => list.OrderBy(c => c.CreatedTime),
                "name" => list.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase),
                "products" => list.OrderByDescending(c => c.ProductCount),
                _ => list.OrderByDescending(c => c.CreatedTime)
            };

            return list.ToList();
        }

        public async Task<CategoryStatsDto> GetStatsAsync()
        {
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            var products = await _productRepository.GetAllAsync();
            var counts = products
                .GroupBy(p => p.CategoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            var newest = categories.OrderByDescending(c => c.CreatedTime).FirstOrDefault();
            var totalProducts = counts.Values.Sum();
            var emptyCount = categories.Count(c => !counts.ContainsKey(c.Id));

            return new CategoryStatsDto
            {
                TotalCategories = categories.Count,
                NewestName = newest?.Name,
                NewestCreatedTime = newest?.CreatedTime,
                AvgProductsPerCategory = categories.Count > 0 ? (double)totalProducts / categories.Count : 0,
                EmptyCategoriesCount = emptyCount
            };
        }
    }
}

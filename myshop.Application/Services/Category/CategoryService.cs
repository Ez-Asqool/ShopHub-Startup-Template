using AutoMapper;
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
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
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
            return CategoryOperationResult.Success;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var existing = await _categoryRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            _categoryRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}

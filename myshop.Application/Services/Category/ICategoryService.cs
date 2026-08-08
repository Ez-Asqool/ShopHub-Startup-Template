using myshop.Application.Services.Category.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Services.Category
{

    public enum CategoryOperationResult
    {
        Success,
        NotFound,
        DuplicateName
    }

    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryUpdateDto?> GetCategoryForEditAsync(int id);
        Task<bool> CreateCategoryAsync(CategoryCreateDto dto);
        Task<CategoryOperationResult> UpdateCategoryAsync(CategoryUpdateDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<IEnumerable<CategoryAdminDto>> GetCategoriesForAdminAsync(string? search, string? sort);
        Task<CategoryStatsDto> GetStatsAsync();
    }
}

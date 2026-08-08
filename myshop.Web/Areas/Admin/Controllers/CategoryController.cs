using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Category;
using myshop.Application.Services.Category.Dto;
using myshop.Domain.Constants;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetList(string? search, string? sort)
        {
            var items = await _categoryService.GetCategoriesForAdminAsync(search, sort);
            var stats = await _categoryService.GetStatsAsync();

            return Json(new { items, stats });
        }

        [HttpGet]
        public async Task<IActionResult> GetForEdit(int id)
        {
            var category = await _categoryService.GetCategoryForEditAsync(id);
            if (category == null)
                return NotFound();

            return Json(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryCreateDto dto)
        {
            var validationError = ValidateFields(dto.Name, dto.Description);
            if (validationError != null)
                return Json(new { success = false, message = validationError });

            var created = await _categoryService.CreateCategoryAsync(dto);
            if (!created)
                return Json(new { success = false, message = "A category with this name already exists." });

            return Json(new { success = true, message = "Category created successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] CategoryUpdateDto dto)
        {
            dto.Id = id;

            var validationError = ValidateFields(dto.Name, dto.Description);
            if (validationError != null)
                return Json(new { success = false, message = validationError });

            var result = await _categoryService.UpdateCategoryAsync(dto);

            return result switch
            {
                CategoryOperationResult.Success => Json(new { success = true, message = "Category updated successfully" }),
                CategoryOperationResult.NotFound => Json(new { success = false, message = "This category no longer exists." }),
                CategoryOperationResult.DuplicateName => Json(new { success = false, message = "A category with this name already exists." }),
                _ => Json(new { success = false, message = "Something went wrong." })
            };
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            if (!deleted)
                return Json(new { success = false, message = "Error while deleting" });

            return Json(new { success = true, message = "Category deleted" });
        }

        private static string? ValidateFields(string? name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
                return "Enter a category name";
            if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10)
                return "Add a description of at least 10 characters";
            return null;
        }
    }
}

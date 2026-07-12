using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Category;
using myshop.Application.Services.Category.Dto;
using myshop.Domain.Entities;

namespace myshop.Web.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            if (ModelState.IsValid)
            {
                if (await _categoryService.CreateCategoryAsync(dto))
                {
                    TempData["Create"] = "Item has Created Successfully";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Name", "A category with this name already exists.");
            }
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var dto = await _categoryService.GetCategoryForEditAsync(id.Value);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryUpdateDto dto)
        {
            if (ModelState.IsValid)
            {
                var result = await _categoryService.UpdateCategoryAsync(dto);

                switch (result)
                {
                    case CategoryOperationResult.Success:
                        TempData["Update"] = "Data has Updated Successfully";
                        return RedirectToAction("Index");

                    case CategoryOperationResult.NotFound:
                        ModelState.AddModelError("", "This category no longer exists. It may have been deleted.");
                        break;

                    case CategoryOperationResult.DuplicateName:
                        ModelState.AddModelError("Name", "A category with this name already exists.");
                        break;
                }
            }
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var dto = await _categoryService.GetCategoryForEditAsync(id.Value);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            if (await _categoryService.DeleteCategoryAsync(id.Value))
            {
                TempData["Delete"] = "Item has Deleted Successfully";
                return RedirectToAction("Index");
            }

            return NotFound();
        }
    }
}

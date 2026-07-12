using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using myshop.Application.Services.Product;
using myshop.Application.Services.Product.Dto;
using myshop.Domain.Entities;
using myshop.Entities.Models;
using myshop.Infrastructure.Data;
using myshop.Web.ViewModels;

namespace myshop.Web.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var products = await _productService.GetAllProductsAsync();
            return Json(new { data = products });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _productService.GetCategoriesForDropdownAsync();
            var vm = new ProductCreateVM
            {
                Product = new ProductCreateDto(),
                CategoryList = categories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateVM vm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                Stream? imageStream = file?.OpenReadStream();
                string? extension = file != null ? Path.GetExtension(file.FileName) : null;

                var result = await _productService.CreateProductAsync(vm.Product, imageStream, extension);

                switch (result)
                {
                    case ProductOperationResult.Success:
                        TempData["Create"] = "Item has Created Successfully";
                        return RedirectToAction("Index");

                    case ProductOperationResult.InvalidCategory:
                        ModelState.AddModelError("Product.CategoryId", "Selected category does not exist.");
                        break;
                }
            }

            var categories = await _productService.GetCategoriesForDropdownAsync();
            vm.CategoryList = categories.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var productDto = await _productService.GetProductForEditAsync(id.Value);
            if (productDto == null)
                return NotFound();

            var categories = await _productService.GetCategoriesForDropdownAsync();
            var vm = new ProductEditVM
            {
                Product = productDto,
                CategoryList = categories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductEditVM vm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                Stream? imageStream = file?.OpenReadStream();
                string? extension = file != null ? Path.GetExtension(file.FileName) : null;

                var result = await _productService.UpdateProductAsync(vm.Product, imageStream, extension);

                switch (result)
                {
                    case ProductOperationResult.Success:
                        TempData["Update"] = "Data has Updated Successfully";
                        return RedirectToAction("Index");

                    case ProductOperationResult.NotFound:
                        ModelState.AddModelError("", "This product no longer exists.");
                        break;

                    case ProductOperationResult.InvalidCategory:
                        ModelState.AddModelError("Product.CategoryId", "Selected category does not exist.");
                        break;
                }
            }

            var categories = await _productService.GetCategoriesForDropdownAsync();
            vm.CategoryList = categories.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(vm);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return Json(new { success = false, message = "Error while Deleting" });

            var result = await _productService.DeleteProductAsync(id.Value);

            if (result == ProductOperationResult.NotFound)
                return Json(new { success = false, message = "Error while Deleting" });

            return Json(new { success = true, message = "file has been Deleted" });
        }
    }
}

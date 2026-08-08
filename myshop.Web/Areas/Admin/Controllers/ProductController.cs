using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Product;
using myshop.Application.Services.Product.Dto;
using myshop.Domain.Constants;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _productService.GetCategoriesForDropdownAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetList(string? search, int? categoryId, int page = 1, int pageSize = 5)
        {
            var paged = await _productService.GetProductsPagedAsync(search, "name_asc", categoryId, page, pageSize);
            var stats = await _productService.GetStatsAsync();

            return Json(new
            {
                items = paged.Items,
                totalCount = paged.TotalCount,
                page = paged.PageNumber,
                pageSize = paged.PageSize,
                stats
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetForEdit(int id)
        {
            var product = await _productService.GetProductForEditAsync(id);
            if (product == null)
                return NotFound();

            return Json(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto, IFormFile? file)
        {
            var validationError = ValidateProductFields(dto.Name, dto.Description, dto.Price);
            if (validationError != null)
                return Json(new { success = false, message = validationError });

            Stream? imageStream = file?.OpenReadStream();
            string? extension = file != null ? Path.GetExtension(file.FileName) : null;

            var result = await _productService.CreateProductAsync(dto, imageStream, extension);
            return Json(ToResponse(result, "Product created successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductUpdateDto dto, IFormFile? file)
        {
            dto.Id = id;

            var validationError = ValidateProductFields(dto.Name, dto.Description, dto.Price);
            if (validationError != null)
                return Json(new { success = false, message = validationError });

            Stream? imageStream = file?.OpenReadStream();
            string? extension = file != null ? Path.GetExtension(file.FileName) : null;

            var result = await _productService.UpdateProductAsync(dto, imageStream, extension);
            return Json(ToResponse(result, "Product updated successfully"));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return Json(new { success = false, message = "Error while deleting" });

            var result = await _productService.DeleteProductAsync(id.Value);

            if (result == ProductOperationResult.NotFound)
                return Json(new { success = false, message = "Error while deleting" });

            return Json(new { success = true, message = "Product deleted" });
        }

        private static string? ValidateProductFields(string? name, string? description, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
                return "Enter a product name";
            if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10)
                return "Add a description of at least 10 characters";
            if (price <= 0)
                return "Enter a valid price";
            return null;
        }

        private static object ToResponse(ProductOperationResult result, string successMessage) => result switch
        {
            ProductOperationResult.Success => new { success = true, message = successMessage },
            ProductOperationResult.NotFound => new { success = false, message = "This product no longer exists." },
            ProductOperationResult.InvalidCategory => new { success = false, message = "Selected category does not exist." },
            ProductOperationResult.InvalidImageExtension => new { success = false, message = "Only these image types are allowed: .jpg, .jpeg, .png, .webp" },
            ProductOperationResult.InvalidImageSize => new { success = false, message = "Image size must not exceed 2 MB." },
            ProductOperationResult.InvalidImageContent => new { success = false, message = "The uploaded file's content does not match a valid image type." },
            _ => new { success = false, message = "Something went wrong." }
        };
    }
}

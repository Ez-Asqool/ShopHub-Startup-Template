using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Product;

namespace myshop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private const int DefaultPageSize = 6;

        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(string? search, string? sortBy, int page = 1, int pageSize = DefaultPageSize)
        {
            var result = await _productService.GetProductsPagedAsync(search, sortBy, null, page, pageSize);

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;

            return View(result);
        }
    }
}

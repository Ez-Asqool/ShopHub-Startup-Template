using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Category;
using myshop.Application.Services.Product;
using myshop.Domain.Entities;
using myshop.Web.ViewModels;
using System.Diagnostics;

namespace myshop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(
            ILogger<HomeController> logger,
            IProductService productService,
            ICategoryService categoryService)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var vm = new HomeIndexVM
            {
                Products = await _productService.GetAllProductsAsync(),
                Categories = await _categoryService.GetAllCategoriesAsync(),
                Search = search
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
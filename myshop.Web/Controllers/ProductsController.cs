using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Product;
using myshop.Application.Services.Review;
using myshop.Infrastructure.Identity;
using myshop.Web.ViewModels;

namespace myshop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private const int DefaultPageSize = 6;

        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(IProductService productService, IReviewService reviewService, UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _reviewService = reviewService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, string? sortBy, int page = 1, int pageSize = DefaultPageSize)
        {
            var result = await _productService.GetProductsPagedAsync(search, sortBy, null, page, pageSize);

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var reviews = await _reviewService.GetProductReviewsAsync(id, currentUserId);

            var vm = new ProductDetailsVM
            {
                Product = product,
                Reviews = reviews
            };

            return View(vm);
        }
    }
}

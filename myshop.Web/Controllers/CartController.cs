using Microsoft.AspNetCore.Mvc;
using myshop.Application.Contracts;
using myshop.Application.Models;
using myshop.Application.Services.Product;
using myshop.Web.ViewModels;

namespace myshop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            var products = (await _productService.GetAllProductsAsync()).ToDictionary(p => p.Id);

            var vm = new CartIndexVM
            {
                Items = cart.Select(c => new CartLineVM
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    CategoryName = products.TryGetValue(c.ProductId, out var p) ? p.CategoryName : null,
                    ImageUrl = c.ImageUrl,
                    Price = c.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            _cartService.AddItem(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity < 1 ? 1 : quantity,
                ImageUrl = product.Img
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int productId)
        {
            _cartService.RemoveItem(productId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IncreaseQuantity(int productId)
        {
            _cartService.IncreaseQuantity(productId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DecreaseQuantity(int productId)
        {
            _cartService.DecreaseQuantity(productId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }
    }
}

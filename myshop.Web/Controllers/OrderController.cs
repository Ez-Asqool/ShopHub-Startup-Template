using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Contracts;
using myshop.Application.Services.Order;
using myshop.Application.Services.Order.Dto;
using myshop.Infrastructure.Identity;
using myshop.Web.ViewModels;
using System.Text.Json;

namespace myshop.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ICartService cartService, UserManager<ApplicationUser> userManager, IEmailService emailService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckStock()
        {
            var result = await _orderService.CheckStockAsync();

            return result.Status switch
            {
                OrderOperationResult.Success => Json(new { ok = true }),
                OrderOperationResult.CartEmpty => Json(new { ok = false, message = "Your cart is empty" }),
                OrderOperationResult.InsufficientStock => Json(new { ok = false, message = BuildInsufficientStockMessage(result.InsufficientStockProductNames) }),
                _ => Json(new { ok = false, message = "Could not verify stock. Please try again." })
            };
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = _cartService.GetCart();
            var user = await _userManager.GetUserAsync(User);

            var vm = new CheckoutVM
            {
                Items = cart,
                Subtotal = cart.Sum(i => i.Price * i.Quantity),
                RecipientName = user?.Name ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                City = user?.City ?? string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Items = _cartService.GetCart();
                vm.Subtotal = vm.Items.Sum(i => i.Price * i.Quantity);
                return View(vm);
            }

            var cart = _cartService.GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var dto = new CheckoutDto
            {
                UserId = _userManager.GetUserId(User)!,
                RecipientName = vm.RecipientName,
                Address = vm.Address,
                City = vm.City,
                PhoneNumber = vm.PhoneNumber
            };

            SavePendingCheckout(dto);
            return RedirectToAction(nameof(Payment));
        }

        [HttpGet]
        public IActionResult Payment()
        {
            var dto = GetPendingCheckout();
            if (dto == null)
                return RedirectToAction(nameof(Checkout));

            var cart = _cartService.GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var vm = new PaymentVM
            {
                Items = cart,
                Subtotal = cart.Sum(i => i.Price * i.Quantity),
                RecipientName = dto.RecipientName,
                Address = dto.Address,
                City = dto.City,
                PhoneNumber = dto.PhoneNumber
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment()
        {
            var dto = GetPendingCheckout();
            if (dto == null)
                return Json(new { success = false, message = "Your checkout session expired.", redirect = Url.Action(nameof(Checkout)) });

            var result = await _orderService.CreateOrderAsync(dto);

            switch (result.Status)
            {
                case OrderOperationResult.Success:
                    ClearPendingCheckout();

                    try
                    {
                        var userId = _userManager.GetUserId(User)!;
                        var user = await _userManager.GetUserAsync(User);
                        var order = await _orderService.GetOrderDetailsAsync(result.OrderId!.Value, userId);
                        if (order != null && user?.Email != null)
                        {
                            var orderUrl = Url.Action(nameof(OrderDetails), "Order", new { id = order.Id }, Request.Scheme)!;
                            var myOrdersUrl = Url.Action(nameof(MyOrders), "Order", null, Request.Scheme)!;
                            await _emailService.SendOrderConfirmationEmailAsync(user.Email, order, orderUrl, myOrdersUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send order confirmation email for order {OrderId}", result.OrderId);
                    }

                    return Json(new { success = true, orderId = result.OrderId });

                case OrderOperationResult.CartEmpty:
                    ClearPendingCheckout();
                    return Json(new { success = false, message = "Your cart is empty", redirect = Url.Action("Index", "Cart") });

                case OrderOperationResult.InsufficientStock:
                    return Json(new { success = false, message = BuildInsufficientStockMessage(result.InsufficientStockProductNames) });

                default:
                    return Json(new { success = false, message = "Payment could not be completed. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User)!;
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
                return NotFound();

            return View(order);
        }

        private static string BuildInsufficientStockMessage(List<string>? productNames) =>
            "Not enough stock for: " + string.Join(", ", productNames ?? new List<string>());

        private const string PendingCheckoutSessionKey = "PendingCheckout";

        private void SavePendingCheckout(CheckoutDto dto) =>
            HttpContext.Session.SetString(PendingCheckoutSessionKey, JsonSerializer.Serialize(dto));

        private CheckoutDto? GetPendingCheckout()
        {
            var json = HttpContext.Session.GetString(PendingCheckoutSessionKey);
            return json == null ? null : JsonSerializer.Deserialize<CheckoutDto>(json);
        }

        private void ClearPendingCheckout() => HttpContext.Session.Remove(PendingCheckoutSessionKey);
    }
}

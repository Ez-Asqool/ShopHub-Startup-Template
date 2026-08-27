using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Order;
using myshop.Domain.Constants;
using myshop.Domain.Entities.Enums;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetList(string? search, string? status, int page = 1, int pageSize = 6)
        {
            OrderStatus? statusFilter = Enum.TryParse<OrderStatus>(status, out var parsed) ? parsed : null;

            var paged = await _orderService.GetAllOrdersPagedAsync(search, statusFilter, page, pageSize);
            var stats = await _orderService.GetOrderStatsAsync();

            return Json(new
            {
                items = paged.Items.Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalPrice,
                    o.ItemCount,
                    o.RecipientName,
                    o.City,
                    OrderStatus = o.OrderStatus.ToString(),
                    PaymentStatus = o.PaymentStatus.ToString()
                }),
                totalCount = paged.TotalCount,
                page = paged.PageNumber,
                pageSize = paged.PageSize,
                stats
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _orderService.GetOrderDetailsForAdminAsync(id);
            if (order == null)
                return NotFound();

            return Json(new
            {
                order.Id,
                order.OrderDate,
                order.TotalPrice,
                order.RecipientName,
                order.Address,
                order.City,
                order.PhoneNumber,
                OrderStatus = order.OrderStatus.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                Items = order.Items.Select(i => new { i.ProductId, i.ProductName, i.ProductImg, i.UnitPrice, i.Quantity })
            });
        }
    }
}

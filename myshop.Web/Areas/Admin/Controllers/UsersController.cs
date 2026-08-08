using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.User;
using myshop.Domain.Constants;
using System.Security.Claims;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetList(string? search, string? role, string? status, int page = 1, int pageSize = 6)
        {
            var paged = await _userManagementService.GetUsersForAdminAsync(search, role, status, page, pageSize);
            var stats = await _userManagementService.GetStatsAsync();

            return Json(new
            {
                items = paged.Items.Select(u => new { u.Id, u.UserName, u.Email, u.Name, u.Role, u.IsLocked, isSelf = u.Id == CurrentUserId }),
                totalCount = paged.TotalCount,
                page = paged.PageNumber,
                pageSize = paged.PageSize,
                stats
            });
        }

        [HttpPost]
        public async Task<IActionResult> Promote(string id)
        {
            var result = await _userManagementService.PromoteToAdminAsync(id);
            return Json(ToResponse(result, "User promoted to Admin"));
        }

        [HttpPost]
        public async Task<IActionResult> Demote(string id)
        {
            var result = await _userManagementService.DemoteToCustomerAsync(id, CurrentUserId);
            return Json(ToResponse(result, "User demoted to Customer"));
        }

        [HttpPost]
        public async Task<IActionResult> Lock(string id)
        {
            var result = await _userManagementService.LockUserAsync(id, CurrentUserId);
            return Json(ToResponse(result, "User account locked"));
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(string id)
        {
            var result = await _userManagementService.UnlockUserAsync(id);
            return Json(ToResponse(result, "User account unlocked"));
        }

        [HttpDelete]
        [Authorize(Policy = "AdminOnlyPolicy")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userManagementService.DeleteUserAsync(id, CurrentUserId);
            return Json(ToResponse(result, "User deleted"));
        }

        private static object ToResponse(UserOperationResult result, string successMessage) => result switch
        {
            UserOperationResult.Success => new { success = true, message = successMessage },
            UserOperationResult.CannotModifySelf => new { success = false, message = "You cannot perform this action on your own account." },
            UserOperationResult.NotFound => new { success = false, message = "User not found. They may have already been removed." },
            _ => new { success = false, message = "Something went wrong." }
        };
    }
}

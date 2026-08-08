using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myshop.Application.Common;
using myshop.Application.Services.User;
using myshop.Application.Services.User.Dto;
using myshop.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myshop.Infrastructure.Identity
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)) ||
                    (u.Name != null && u.Name.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<UserListItemDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                items.Add(new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Name = user.Name,
                    Role = roles.Contains(Roles.Admin) ? Roles.Admin : Roles.Customer,
                    IsLocked = await _userManager.IsLockedOutAsync(user)
                });
            }

            return new PagedResult<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<UserListItemDto>> GetUsersForAdminAsync(string? search, string? role, string? status, int pageNumber, int pageSize)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)) ||
                    (u.Name != null && u.Name.Contains(search)));
            }

            var users = await query.OrderBy(u => u.UserName).ToListAsync();

            var items = new List<UserListItemDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                items.Add(new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Name = user.Name,
                    Role = roles.Contains(Roles.Admin) ? Roles.Admin : Roles.Customer,
                    IsLocked = await _userManager.IsLockedOutAsync(user)
                });
            }

            IEnumerable<UserListItemDto> filtered = items;

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
                filtered = filtered.Where(u => u.Role == role);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                bool wantLocked = status == "Locked";
                filtered = filtered.Where(u => u.IsLocked == wantLocked);
            }

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;

            var paged = filteredList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<UserListItemDto>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<UserStatsDto> GetStatsAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            int adminsCount = 0;
            int lockedCount = 0;
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                    adminsCount++;
                if (await _userManager.IsLockedOutAsync(user))
                    lockedCount++;
            }

            return new UserStatsDto
            {
                TotalUsers = users.Count,
                AdminsCount = adminsCount,
                LockedCount = lockedCount,
                ActiveCount = users.Count - lockedCount
            };
        }

        public async Task<UserOperationResult> PromoteToAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserOperationResult.NotFound;

            if (await _userManager.IsInRoleAsync(user, Roles.Customer))
                await _userManager.RemoveFromRoleAsync(user, Roles.Customer);

            if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
                await _userManager.AddToRoleAsync(user, Roles.Admin);

            return UserOperationResult.Success;
        }

        public async Task<UserOperationResult> DemoteToCustomerAsync(string userId, string currentUserId)
        {
            if (string.Equals(userId, currentUserId, StringComparison.OrdinalIgnoreCase))
                return UserOperationResult.CannotModifySelf;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserOperationResult.NotFound;

            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                await _userManager.RemoveFromRoleAsync(user, Roles.Admin);

            if (!await _userManager.IsInRoleAsync(user, Roles.Customer))
                await _userManager.AddToRoleAsync(user, Roles.Customer);

            return UserOperationResult.Success;
        }

        public async Task<UserOperationResult> LockUserAsync(string userId, string currentUserId)
        {
            if (string.Equals(userId, currentUserId, StringComparison.OrdinalIgnoreCase))
                return UserOperationResult.CannotModifySelf;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserOperationResult.NotFound;

            if (!user.LockoutEnabled)
                await _userManager.SetLockoutEnabledAsync(user, true);

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return UserOperationResult.Success;
        }

        public async Task<UserOperationResult> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserOperationResult.NotFound;

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            return UserOperationResult.Success;
        }

        public async Task<UserOperationResult> DeleteUserAsync(string userId, string currentUserId)
        {
            if (string.Equals(userId, currentUserId, StringComparison.OrdinalIgnoreCase))
                return UserOperationResult.CannotModifySelf;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserOperationResult.NotFound;

            await _userManager.DeleteAsync(user);
            return UserOperationResult.Success;
        }
    }
}

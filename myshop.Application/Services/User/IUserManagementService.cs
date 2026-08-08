using myshop.Application.Common;
using myshop.Application.Services.User.Dto;
using System.Threading.Tasks;

namespace myshop.Application.Services.User
{
    public enum UserOperationResult
    {
        Success,
        NotFound,
        CannotModifySelf
    }

    public interface IUserManagementService
    {
        Task<PagedResult<UserListItemDto>> GetUsersAsync(string? search, int pageNumber, int pageSize);
        Task<PagedResult<UserListItemDto>> GetUsersForAdminAsync(string? search, string? role, string? status, int pageNumber, int pageSize);
        Task<UserStatsDto> GetStatsAsync();
        Task<UserOperationResult> PromoteToAdminAsync(string userId);
        Task<UserOperationResult> DemoteToCustomerAsync(string userId, string currentUserId);
        Task<UserOperationResult> LockUserAsync(string userId, string currentUserId);
        Task<UserOperationResult> UnlockUserAsync(string userId);
        Task<UserOperationResult> DeleteUserAsync(string userId, string currentUserId);
    }
}

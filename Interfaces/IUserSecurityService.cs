using ApiGMPKlik.DTOs;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Interfaces
{
    /// <summary>
    /// Service untuk mengelola UserSecurity dengan role-based filtering
    /// </summary>
    public interface IUserSecurityService
    {
        Task<ApiResponse<UserSecurityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserSecurityDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserSecurityDto>>> GetAllAsync(UserSecurityFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaginatedList<UserSecurityDto>>> GetPaginatedAsync(UserSecurityFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserSecurityDto>> CreateAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserSecurityDto>> UpdateAsync(int id, UpdateUserSecurityDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> LockUserAsync(string userId, DateTime lockedUntil, string? reason = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UnlockUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ResetFailedAttemptsAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RecordLoginSuccessAsync(string userId, string? ipAddress = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RecordLoginFailureAsync(string userId, string? ipAddress = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> IsUserLockedAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<int>> CountAsync(UserSecurityFilterDto? filter = null, CancellationToken cancellationToken = default);
    }
}

using ApiGMPKlik.DTOs;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Interfaces
{
    /// <summary>
    /// Service untuk mengelola UserProfile dengan role-based filtering
    /// </summary>
    public interface IUserProfileService
    {
        Task<ApiResponse<UserProfileDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserProfileDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserProfileDto>>> GetAllAsync(UserProfileFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaginatedList<UserProfileDto>>> GetPaginatedAsync(UserProfileFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserProfileDto>> CreateAsync(CreateUserProfileDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserProfileDto>> UpdateAsync(int id, UpdateUserProfileDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserProfileDto>> UpdateByUserIdAsync(string userId, UpdateUserProfileDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<int>> CountAsync(UserProfileFilterDto? filter = null, CancellationToken cancellationToken = default);
    }
}

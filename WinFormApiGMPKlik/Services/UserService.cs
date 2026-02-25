using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    /// <summary>
    /// Service untuk mengelola User di WinForm
    /// </summary>
    public class UserService
    {
        private readonly ApiClientService _apiClient;

        public UserService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllAsync(UserFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/user";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");
                if (filter.BranchId.HasValue) queryParams.Add($"branchId={filter.BranchId.Value}");
                if (filter.IsActive.HasValue) queryParams.Add($"isActive={filter.IsActive.Value}");
                if (filter.IsDeleted.HasValue) queryParams.Add($"isDeleted={filter.IsDeleted.Value}");
                if (!string.IsNullOrEmpty(filter.Role)) queryParams.Add($"role={Uri.EscapeDataString(filter.Role)}");
                if (filter.CreatedFrom.HasValue) queryParams.Add($"createdFrom={filter.CreatedFrom.Value:yyyy-MM-dd}");
                if (filter.CreatedTo.HasValue) queryParams.Add($"createdTo={filter.CreatedTo.Value:yyyy-MM-dd}");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<UserResponseDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<UserResponseDto>>> GetPaginatedAsync(
            UserFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/user/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search)) endpoint += $"&search={Uri.EscapeDataString(filter.Search)}";
                if (filter.BranchId.HasValue) endpoint += $"&branchId={filter.BranchId.Value}";
                if (filter.IsActive.HasValue) endpoint += $"&isActive={filter.IsActive.Value}";
                if (!string.IsNullOrEmpty(filter.Role)) endpoint += $"&role={Uri.EscapeDataString(filter.Role)}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<UserResponseDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<UserResponseDto>> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<UserResponseDto>($"api/user/{id}", ct);
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<LoginResponseDto>("api/auth/login", dto, ct);
        }

        public async Task<ApiResponse<UserResponseDto>> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<UserResponseDto>("api/auth/register", dto, ct);
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateAsync(string id, UpdateUserDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<UserResponseDto>($"api/user/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/user/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/user/{id}/restore", new { }, ct);
        }

        public async Task<ApiResponse<bool>> ActivateAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/user/{id}/activate", new { }, ct);
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/user/{id}/deactivate", new { }, ct);
        }

        public async Task<ApiResponse<bool>> AssignRolesAsync(string userId, List<string> roles, bool removeExisting = false, CancellationToken ct = default)
        {
            var dto = new AssignRolesDto { UserId = userId, Roles = roles, RemoveExisting = removeExisting };
            return await _apiClient.PostAsync<bool>($"api/user/{userId}/roles", dto, ct);
        }

        public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<string>>($"api/user/{userId}/roles", ct);
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            var dto = new { CurrentPassword = currentPassword, NewPassword = newPassword };
            return await _apiClient.PostAsync<bool>($"api/user/{userId}/change-password", dto, ct);
        }
    }

    /// <summary>
    /// Service untuk mengelola UserProfile di WinForm
    /// </summary>
    public class UserProfileService
    {
        private readonly ApiClientService _apiClient;

        public UserProfileService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<UserProfileDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<UserProfileDto>($"api/user-profile/{id}", ct);
        }

        public async Task<ApiResponse<UserProfileDto>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<UserProfileDto>($"api/user-profile/user/{userId}", ct);
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetAllAsync(UserProfileFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/user-profile";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");
                if (!string.IsNullOrEmpty(filter.Gender)) queryParams.Add($"gender={Uri.EscapeDataString(filter.Gender)}");
                if (!string.IsNullOrEmpty(filter.City)) queryParams.Add($"city={Uri.EscapeDataString(filter.City)}");
                if (filter.BranchId.HasValue) queryParams.Add($"branchId={filter.BranchId.Value}");
                if (filter.NewsletterSubscribed.HasValue) queryParams.Add($"newsletterSubscribed={filter.NewsletterSubscribed.Value}");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<UserProfileDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<UserProfileDto>>> GetPaginatedAsync(
            UserProfileFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/user-profile/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search)) endpoint += $"&search={Uri.EscapeDataString(filter.Search)}";
                if (filter.BranchId.HasValue) endpoint += $"&branchId={filter.BranchId.Value}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<UserProfileDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<UserProfileDto>> CreateAsync(CreateUserProfileDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<UserProfileDto>("api/user-profile", dto, ct);
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateAsync(int id, UpdateUserProfileDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<UserProfileDto>($"api/user-profile/{id}", dto, ct);
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateByUserIdAsync(string userId, UpdateUserProfileDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<UserProfileDto>($"api/user-profile/user/{userId}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/user-profile/{id}", ct);
        }

        public async Task<ApiResponse<bool>> DeleteByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/user-profile/user/{userId}", ct);
        }
    }

    /// <summary>
    /// Service untuk mengelola UserSecurity di WinForm
    /// </summary>
    public class UserSecurityService
    {
        private readonly ApiClientService _apiClient;

        public UserSecurityService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<UserSecurityDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<UserSecurityDto>($"api/user-security/{id}", ct);
        }

        public async Task<ApiResponse<UserSecurityDto>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<UserSecurityDto>($"api/user-security/user/{userId}", ct);
        }

        public async Task<ApiResponse<List<UserSecurityDto>>> GetAllAsync(UserSecurityFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/user-security";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");
                if (filter.IsLocked.HasValue) queryParams.Add($"isLocked={filter.IsLocked.Value}");
                if (filter.MinFailedAttempts.HasValue) queryParams.Add($"minFailedAttempts={filter.MinFailedAttempts.Value}");
                if (filter.BranchId.HasValue) queryParams.Add($"branchId={filter.BranchId.Value}");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<UserSecurityDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<UserSecurityDto>>> GetPaginatedAsync(
            UserSecurityFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/user-security/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search)) endpoint += $"&search={Uri.EscapeDataString(filter.Search)}";
                if (filter.IsLocked.HasValue) endpoint += $"&isLocked={filter.IsLocked.Value}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<UserSecurityDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<bool>> LockUserAsync(string userId, DateTime lockedUntil, string? reason = null, CancellationToken ct = default)
        {
            var dto = new LockUserDto { LockedUntil = lockedUntil, Reason = reason };
            return await _apiClient.PostAsync<bool>($"api/user-security/{userId}/lock", dto, ct);
        }

        public async Task<ApiResponse<bool>> UnlockUserAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<bool>($"api/user-security/{userId}/unlock", new { }, ct);
        }

        public async Task<ApiResponse<bool>> ResetFailedAttemptsAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<bool>($"api/user-security/{userId}/reset-attempts", new { }, ct);
        }

        public async Task<ApiResponse<bool>> IsUserLockedAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<bool>($"api/user-security/{userId}/is-locked", ct);
        }
    }
}

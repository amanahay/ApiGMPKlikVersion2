using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    /// <summary>
    /// Service untuk mengelola Role dan Permission di WinForm
    /// </summary>
    public class RoleService
    {
        private readonly ApiClientService _apiClient;

        public RoleService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        #region Role Methods

        public async Task<ApiResponse<List<RoleDto>>> GetAllAsync(RoleFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/role";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");
                if (filter.IsActive.HasValue) queryParams.Add($"isActive={filter.IsActive.Value}");
                if (!string.IsNullOrEmpty(filter.SortBy)) queryParams.Add($"sortBy={filter.SortBy}");
                if (filter.SortDescending) queryParams.Add("sortDescending=true");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<RoleDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<RoleDto>>> GetPaginatedAsync(
            RoleFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/role/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search)) endpoint += $"&search={Uri.EscapeDataString(filter.Search)}";
                if (filter.IsActive.HasValue) endpoint += $"&isActive={filter.IsActive.Value}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<RoleDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<RoleDto>> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<RoleDto>($"api/role/{id}", ct);
        }

        public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<RoleDto>("api/role", dto, ct);
        }

        public async Task<ApiResponse<RoleDto>> UpdateAsync(string id, UpdateRoleDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<RoleDto>($"api/role/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/role/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(string id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/role/{id}/restore", new { }, ct);
        }

        public async Task<ApiResponse<List<PermissionDto>>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<PermissionDto>>($"api/role/{roleId}/permissions", ct);
        }

        #endregion
    }

    /// <summary>
    /// Service untuk mengelola Permission di WinForm
    /// </summary>
    public class PermissionService
    {
        private readonly ApiClientService _apiClient;

        public PermissionService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<PermissionDto>>> GetAllAsync(PermissionFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/permission";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");
                if (!string.IsNullOrEmpty(filter.Module)) queryParams.Add($"module={Uri.EscapeDataString(filter.Module)}");
                if (filter.IsActive.HasValue) queryParams.Add($"isActive={filter.IsActive.Value}");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<PermissionDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<PermissionDto>>> GetPaginatedAsync(
            PermissionFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/permission/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search)) endpoint += $"&search={Uri.EscapeDataString(filter.Search)}";
                if (!string.IsNullOrEmpty(filter.Module)) endpoint += $"&module={Uri.EscapeDataString(filter.Module)}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<PermissionDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PermissionDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<PermissionDto>($"api/permission/{id}", ct);
        }

        public async Task<ApiResponse<PermissionDto>> CreateAsync(CreatePermissionDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<PermissionDto>("api/permission", dto, ct);
        }

        public async Task<ApiResponse<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<PermissionDto>($"api/permission/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/permission/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/permission/{id}/restore", new { }, ct);
        }

        public async Task<ApiResponse<List<string>>> GetModulesAsync(CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<string>>("api/permission/modules", ct);
        }
    }

    /// <summary>
    /// Service untuk mengelola RolePermission di WinForm
    /// </summary>
    public class RolePermissionService
    {
        private readonly ApiClientService _apiClient;

        public RolePermissionService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<RolePermissionDto>> AssignPermissionAsync(AssignPermissionToRoleDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<RolePermissionDto>("api/role-permission", dto, ct);
        }

        public async Task<ApiResponse<bool>> RemovePermissionAsync(string roleId, int permissionId, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/role-permission?roleId={roleId}&permissionId={permissionId}", ct);
        }

        public async Task<ApiResponse<bool>> BulkAssignAsync(BulkAssignPermissionDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<bool>("api/role-permission/bulk", dto, ct);
        }

        public async Task<ApiResponse<List<RolePermissionDto>>> GetByRoleAsync(string roleId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<RolePermissionDto>>($"api/role-permission/role/{roleId}", ct);
        }

        public async Task<ApiResponse<bool>> HasPermissionAsync(string roleId, string permissionCode, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<bool>($"api/role-permission/has-permission?roleId={roleId}&permissionCode={permissionCode}", ct);
        }

        public async Task<ApiResponse<bool>> SyncRolePermissionsAsync(string roleId, List<int> permissionIds, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<bool>($"api/role-permission/sync/{roleId}", permissionIds, ct);
        }
    }
}

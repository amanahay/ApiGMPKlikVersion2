using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    /// <summary>
    /// Service untuk mengelola Branch di WinForm
    /// </summary>
    public class BranchService
    {
        private readonly ApiClientService _apiClient;

        public BranchService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<BranchDto>>> GetAllAsync(BranchFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/branch";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");
                if (!string.IsNullOrEmpty(filter.City)) queryParams.Add($"city={Uri.EscapeDataString(filter.City)}");
                if (!string.IsNullOrEmpty(filter.Province)) queryParams.Add($"province={Uri.EscapeDataString(filter.Province)}");
                if (filter.IsActive.HasValue) queryParams.Add($"isActive={filter.IsActive.Value}");
                if (filter.IsMainBranch.HasValue) queryParams.Add($"isMainBranch={filter.IsMainBranch.Value}");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<BranchDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<BranchDto>>> GetPaginatedAsync(
            BranchFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/branch/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search)) endpoint += $"&search={Uri.EscapeDataString(filter.Search)}";
                if (!string.IsNullOrEmpty(filter.City)) endpoint += $"&city={Uri.EscapeDataString(filter.City)}";
                if (!string.IsNullOrEmpty(filter.Province)) endpoint += $"&province={Uri.EscapeDataString(filter.Province)}";
                if (filter.IsActive.HasValue) endpoint += $"&isActive={filter.IsActive.Value}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<BranchDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<BranchDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<BranchDto>($"api/branch/{id}", ct);
        }

        public async Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<BranchDto>("api/branch", dto, ct);
        }

        public async Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<BranchDto>($"api/branch/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, bool hardDelete = false, CancellationToken ct = default)
        {
            var endpoint = $"api/branch/{id}";
            if (hardDelete) endpoint += "?hardDelete=true";
            return await _apiClient.DeleteAsync(endpoint, ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/branch/{id}/restore", new { }, ct);
        }

        public async Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/branch/{id}/activate", new { }, ct);
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/branch/{id}/deactivate", new { }, ct);
        }

        public async Task<ApiResponse<bool>> CodeExistsAsync(string code, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<bool>($"api/branch/code-exists?code={Uri.EscapeDataString(code)}", ct);
        }
    }
}

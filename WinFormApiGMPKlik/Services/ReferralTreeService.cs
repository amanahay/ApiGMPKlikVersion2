using ApiGMPKlik.DTOs;
using ApiGMPKlik.Shared;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    /// <summary>
    /// Service untuk mengelola Referral Tree di WinForm
    /// </summary>
    public class ReferralTreeService
    {
        private readonly ApiClientService _apiClient;

        public ReferralTreeService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<ReferralTreeDto>>> GetAllAsync(ReferralTreeFilterDto? filter = null, CancellationToken ct = default)
        {
            var endpoint = "api/referral-tree";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.RootUserId)) queryParams.Add($"rootUserId={Uri.EscapeDataString(filter.RootUserId)}");
                if (!string.IsNullOrEmpty(filter.ReferredUserId)) queryParams.Add($"referredUserId={Uri.EscapeDataString(filter.ReferredUserId)}");
                if (filter.Level.HasValue) queryParams.Add($"level={filter.Level.Value}");
                if (filter.MaxLevel.HasValue) queryParams.Add($"maxLevel={filter.MaxLevel.Value}");
                if (filter.IsActive.HasValue) queryParams.Add($"isActive={filter.IsActive.Value}");
                
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }
            
            return await _apiClient.GetAsync<List<ReferralTreeDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<PaginatedList<ReferralTreeDto>>> GetPaginatedAsync(
            ReferralTreeFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var endpoint = $"api/referral-tree/paginated?page={page}&pageSize={pageSize}";
            
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.RootUserId)) endpoint += $"&rootUserId={Uri.EscapeDataString(filter.RootUserId)}";
                if (filter.Level.HasValue) endpoint += $"&level={filter.Level.Value}";
            }
            
            return await _apiClient.GetAsync<PaginatedList<ReferralTreeDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<ReferralTreeDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<ReferralTreeDto>($"api/referral-tree/{id}", ct);
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetTreeByRootUserIdAsync(string rootUserId, int? maxLevel = null, CancellationToken ct = default)
        {
            var endpoint = $"api/referral-tree/tree/{rootUserId}";
            if (maxLevel.HasValue) endpoint += $"?maxLevel={maxLevel.Value}";
            return await _apiClient.GetAsync<List<ReferralTreeNodeDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<TreeNodeDto<ReferralTreeNodeData>>> GetTreeStructureAsync(string rootUserId, int? maxLevel = null, CancellationToken ct = default)
        {
            var endpoint = $"api/referral-tree/structure/{rootUserId}";
            if (maxLevel.HasValue) endpoint += $"?maxLevel={maxLevel.Value}";
            return await _apiClient.GetAsync<TreeNodeDto<ReferralTreeNodeData>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetReferralsByUserIdAsync(string userId, int? level = null, CancellationToken ct = default)
        {
            var endpoint = $"api/referral-tree/referrals/{userId}";
            if (level.HasValue) endpoint += $"?level={level.Value}";
            return await _apiClient.GetAsync<List<ReferralTreeNodeDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetAncestorsByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<ReferralTreeNodeDto>>($"api/referral-tree/ancestors/{userId}", ct);
        }

        public async Task<ApiResponse<ReferralTreeDto>> CreateAsync(CreateReferralTreeDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<ReferralTreeDto>("api/referral-tree", dto, ct);
        }

        public async Task<ApiResponse<ReferralTreeDto>> UpdateAsync(int id, UpdateReferralTreeDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<ReferralTreeDto>($"api/referral-tree/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/referral-tree/{id}", ct);
        }

        public async Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/referral-tree/{id}/activate", new { }, ct);
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/referral-tree/{id}/deactivate", new { }, ct);
        }

        public async Task<ApiResponse<ReferralStatisticsDto>> GetStatisticsAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<ReferralStatisticsDto>($"api/referral-tree/statistics/{userId}", ct);
        }

        public async Task<ApiResponse<bool>> MoveDownlineAsync(string userIdToMove, string newParentUserId, CancellationToken ct = default)
        {
            var dto = new MoveDownlineRequestDto { UserIdToMove = userIdToMove, NewParentId = newParentUserId };
            return await _apiClient.PostAsync<bool>("api/referral-tree/move", dto, ct);
        }

        public async Task<ApiResponse<bool>> AssignOrphanUserAsync(string orphanUserId, string targetParentUserId, CancellationToken ct = default)
        {
            var dto = new AssignOrphanRequestDto { OrphanUserId = orphanUserId, TargetParentId = targetParentUserId };
            return await _apiClient.PostAsync<bool>("api/referral-tree/assign-orphan", dto, ct);
        }

        public async Task<ApiResponse<bool>> AutoPromoteDownlinesAsync(string deletedUserId, string? deletedBy = null, CancellationToken ct = default)
        {
            var endpoint = $"api/referral-tree/auto-promote/{deletedUserId}";
            if (!string.IsNullOrEmpty(deletedBy)) endpoint += $"?deletedBy={Uri.EscapeDataString(deletedBy)}";
            return await _apiClient.PostAsync<bool>(endpoint, new { }, ct);
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetDirectDownlinesAsync(string userId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<ReferralTreeNodeDto>>($"api/referral-tree/direct-downlines/{userId}", ct);
        }

        public async Task<ApiResponse<bool>> CanAddReferralAsync(string rootUserId, string referredUserId, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<bool>($"api/referral-tree/can-add?rootUserId={rootUserId}&referredUserId={referredUserId}", ct);
        }
    }
}

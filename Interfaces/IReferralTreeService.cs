using ApiGMPKlik.DTOs;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Interfaces
{
    /// <summary>
    /// Service untuk mengelola ReferralTree dengan role-based filtering
    /// </summary>
    public interface IReferralTreeService
    {
        Task<ApiResponse<ReferralTreeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ReferralTreeDto>>> GetAllAsync(ReferralTreeFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaginatedList<ReferralTreeDto>>> GetPaginatedAsync(ReferralTreeFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ReferralTreeNodeDto>>> GetTreeByRootUserIdAsync(string rootUserId, int? maxLevel = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<TreeNodeDto<ReferralTreeNodeData>>> GetTreeStructureAsync(string rootUserId, int? maxLevel = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ReferralTreeNodeDto>>> GetReferralsByUserIdAsync(string userId, int? level = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ReferralTreeNodeDto>>> GetAncestorsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<ReferralTreeDto>> CreateAsync(CreateReferralTreeDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<ReferralTreeDto>> UpdateAsync(int id, UpdateReferralTreeDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<ReferralStatisticsDto>> GetStatisticsAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CanAddReferralAsync(string rootUserId, string referredUserId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ReferralExistsAsync(string rootUserId, string referredUserId, CancellationToken cancellationToken = default);
        Task<ApiResponse<int>> CountAsync(ReferralTreeFilterDto? filter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pindahkan downline ke upline baru (dengan validasi anti-looping & anti-duplicate)
        /// </summary>
        Task<ApiResponse<bool>> MoveDownlineAsync(string userIdToMove, string newParentUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menempatkan user tanpa upline (orphan) ke dalam tree
        /// </summary>
        Task<ApiResponse<bool>> AssignOrphanUserAsync(string orphanUserId, string targetParentUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validasi apakah target adalah descendant dari source (untuk cek looping)
        /// </summary>
        Task<bool> IsDescendantAsync(string potentialAncestorId, string potentialDescendantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Auto-promote downline saat user dihapus (downline naik 1 level ke parent yang dihapus)
        /// </summary>
        Task<ApiResponse<bool>> AutoPromoteDownlinesAsync(string deletedUserId, string? deletedBy = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get semua downline langsung (Level + 1) dari user
        /// </summary>
        Task<ApiResponse<List<ReferralTreeNodeDto>>> GetDirectDownlinesAsync(string userId, CancellationToken cancellationToken = default);


    }
}

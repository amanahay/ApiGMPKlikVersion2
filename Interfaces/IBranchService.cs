using ApiGMPKlik.DTOs;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Interfaces
{
    /// <summary>
    /// Service untuk mengelola Branch dengan role-based filtering
    /// </summary>
    public interface IBranchService
    {
        Task<ApiResponse<BranchDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<BranchDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<BranchDto>>> GetAllAsync(BranchFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaginatedList<BranchDto>>> GetPaginatedAsync(BranchFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
        Task<ApiResponse<int>> CountAsync(BranchFilterDto? filter = null, CancellationToken cancellationToken = default);
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}

using ApiGMPKlik.Shared;
using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.DTOs
{
    // ==================== REFERRAL TREE DTOs ====================

    public class ReferralTreeDto
    {
        public int Id { get; set; }
        public string RootUserId { get; set; } = string.Empty;
        public string? RootUserName { get; set; }
        public string? RootUserEmail { get; set; }
        public string ReferredUserId { get; set; } = string.Empty;
        public string? ReferredUserName { get; set; }
        public string? ReferredUserEmail { get; set; }
        public int Level { get; set; }
        public string? ParentUserId { get; set; }
        public string? ParentUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReferralTreeNodeDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int Level { get; set; }
        public string? ParentUserId { get; set; }
        public decimal CommissionPercent { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public List<ReferralTreeNodeDto> Children { get; set; } = new();
        public int TotalDescendants { get; set; }
        public int DirectReferrals { get; set; }
    }

    public class CreateReferralTreeDto
    {
        [Required]
        [StringLength(450)]
        public string RootUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string ReferredUserId { get; set; } = string.Empty;

        [Range(1, 3)]
        public int Level { get; set; }

        [StringLength(450)]
        public string? ParentUserId { get; set; }

        [Range(0, 100)]
        public decimal CommissionPercent { get; set; } = 0;
    }

    public class UpdateReferralTreeDto
    {
        [Range(0, 100)]
        public decimal? CommissionPercent { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ReferralTreeFilterDto
    {
        public string? RootUserId { get; set; }
        public string? ReferredUserId { get; set; }
        public int? Level { get; set; }
        public int? MaxLevel { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "Level";
        public bool SortDescending { get; set; } = false;
    }

    public class ReferralStatisticsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int TotalReferrals { get; set; }
        public int Level1Count { get; set; }
        public int Level2Count { get; set; }
        public int Level3Count { get; set; }
        public decimal TotalCommission { get; set; }
        public List<LevelStatistics> LevelStats { get; set; } = new();
    }

    public class LevelStatistics
    {
        public int Level { get; set; }
        public int Count { get; set; }
        public decimal CommissionPercent { get; set; }
    }

    public class ReferralTreeResponse
    {
        public List<TreeNodeDto<ReferralTreeNodeData>> Nodes { get; set; } = new();
        public TreeMetadata TreeInfo { get; set; } = new();
    }

    public class ReferralTreeNodeData
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public decimal CommissionPercent { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public int DirectReferrals { get; set; }
        public int TotalDescendants { get; set; }
    }
    // DTO untuk Move Downline
    public class MoveDownlineRequestDto
    {
        [Required]
        public string UserIdToMove { get; set; } = string.Empty; // User yang dipindahkan

        [Required]
        public string NewParentId { get; set; } = string.Empty;   // Upline baru tujuan
    }

    // DTO untuk Assign Orphan
    public class AssignOrphanRequestDto
    {
        [Required]
        public string OrphanUserId { get; set; } = string.Empty;    // User tanpa upline

        [Required]
        public string TargetParentId { get; set; } = string.Empty;  // Upline yang dituju
    }
}

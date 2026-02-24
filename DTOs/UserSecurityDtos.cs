using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.DTOs
{
    // ==================== USER SECURITY DTOs ====================

    public class UserSecurityDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? FullName { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastFailedLoginAt { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public DateTime? TermsAcceptedAt { get; set; }
        public string? TermsAcceptedVersion { get; set; }
        public DateTime? PrivacyPolicyAcceptedAt { get; set; }
        public string? PrivacyPolicyAcceptedVersion { get; set; }
        public string? ReferralByUserId { get; set; }
        public string? ReferralByUserName { get; set; }
        public string? CachedPermissions { get; set; }
        public DateTime? PermissionsCachedAt { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
    }

    public class UpdateUserSecurityDto
    {
        public DateTime? TermsAcceptedAt { get; set; }

        [StringLength(50)]
        public string? TermsAcceptedVersion { get; set; }

        public DateTime? PrivacyPolicyAcceptedAt { get; set; }

        [StringLength(50)]
        public string? PrivacyPolicyAcceptedVersion { get; set; }

        [StringLength(450)]
        public string? ReferralByUserId { get; set; }
    }

    public class LockUserDto
    {
        [Required]
        public DateTime LockedUntil { get; set; }

        public string? Reason { get; set; }
    }

    public class UserSecurityFilterDto
    {
        public string? Search { get; set; }
        public bool? IsLocked { get; set; }
        public int? MinFailedAttempts { get; set; }
        public int? BranchId { get; set; }
        public string? SortBy { get; set; } = "LastLoginAt";
        public bool SortDescending { get; set; } = true;
    }

    public class SecurityAuditDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
    }
}

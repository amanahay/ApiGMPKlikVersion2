using ApiGMPKlik.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models
{
    public class ApplicationUser : IdentityUser, IAuditable, ISoftDeletable, IEntity<string>
    {
        // ========== Basic Info ==========
        public string? FullName { get; set; }
        public string? Avatar { get; set; }

        // ========== IAuditable Properties (REQUIRED) ==========
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedAt { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        // ========== ISoftDeletable Properties (REQUIRED) ==========
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        [StringLength(450)]
        public string? DeletedBy { get; set; }

        // ========== Additional Status ==========
        public bool IsActive { get; set; } = true;

        // ========== BRANCH Association ==========
        public int? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        // ========== Navigation Properties ==========
        public virtual UserProfile? Profile { get; set; }
        public virtual UserSecurity? Security { get; set; }
        public virtual ICollection<ReferralTree> ReferralAncestors { get; set; } = new List<ReferralTree>();
        public virtual ICollection<ReferralTree> ReferralDescendants { get; set; } = new List<ReferralTree>();
        public virtual ICollection<UserSecurityQuestion> SecurityQuestions { get; set; } = new List<UserSecurityQuestion>();
        // ========== IAuditable Methods (REQUIRED) ==========
        /// <summary>
        /// Mark entity sebagai baru dibuat
        /// </summary>
        public void MarkAsCreated(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
            IsActive = true;
            IsDeleted = false;
        }

        /// <summary>
        /// Mark entity sebagai dimodifikasi
        /// </summary>
        public void MarkAsUpdated(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = userId;
        }

        // ========== ISoftDeletable Methods (REQUIRED) ==========
        /// <summary>
        /// Soft delete entity (mark as deleted, keep data)
        /// </summary>
        public void SoftDelete(string deletedByUserId)
        {
            if (string.IsNullOrWhiteSpace(deletedByUserId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(deletedByUserId));

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedByUserId;
            IsActive = false;

            // Also update modified timestamp
            MarkAsUpdated(deletedByUserId);
        }

        /// <summary>
        /// Restore soft deleted entity
        /// </summary>
        public void Restore(string restoredByUserId)
        {
            if (string.IsNullOrWhiteSpace(restoredByUserId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(restoredByUserId));

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            IsActive = true;

            // Also update modified timestamp
            MarkAsUpdated(restoredByUserId);
        }

        /// <summary>
        /// Check if entity is valid for operation (not deleted and active)
        /// </summary>
        public bool IsValidForOperation() => IsActive && !IsDeleted;
    }

    public class UserProfile : IEntity<int>, IAuditable, ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public string? Gender { get; set; }
        public string? About { get; set; }
        public string? Promotion { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? Address { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public string? Linkedin { get; set; }
        public string? Telegram { get; set; }
        public string? Website { get; set; }
        public decimal? Balance { get; set; }
        public string? TaxId { get; set; }
        public decimal? Commission { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? HeirName { get; set; }
        public string? HeirPhone { get; set; }
        public string? Language { get; set; } = "id";
        public string? TimeZone { get; set; }
        public string? Locale { get; set; }
        public bool NewsletterSubscribed { get; set; } = false;
        public string? Preferences { get; set; }
        public string? Metadata { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public decimal? UsedBudgetThisMonth { get; set; }
        public DateTime? BudgetResetDate { get; set; }

        // IAuditable Properties
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        // ISoftDeletable Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // IAuditable Methods
        public void MarkAsCreated(string userId)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
        }

        public void MarkAsUpdated(string userId)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = userId;
        }

        // ISoftDeletable Methods
        public void SoftDelete(string userId)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = userId;
        }

        public void Restore(string userId)
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        public bool IsValidForOperation() => !IsDeleted;
    }

    public class UserSecurity : BaseEntity
    {
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAt { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        // ========== PROPERTY BARU UNTUK BYPASS PASSWORD ==========
        public DateTime? LastPasswordChangeAt { get; set; }
        public string? PasswordChangedBy { get; set; }
        public bool RequirePasswordChange { get; set; } = false;
        // =========================================================
        public DateTime? TermsAcceptedAt { get; set; }
        public string? TermsAcceptedVersion { get; set; }
        public DateTime? PrivacyPolicyAcceptedAt { get; set; }
        public string? PrivacyPolicyAcceptedVersion { get; set; }
        public string? ReferralByUserId { get; set; }
        public string? CachedPermissions { get; set; }
        public DateTime? PermissionsCachedAt { get; set; }
    }
}
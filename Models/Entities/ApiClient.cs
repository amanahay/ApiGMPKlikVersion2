using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models.Entities
{
    public class ApiClient : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ApiKeyPrefix { get; set; } = string.Empty; // Untuk lookup (e.g., "gmp_k2x9")

        [Required]
        [StringLength(500)] // Hash SHA256 dalam Base64
        public string ApiKeyHash { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }

        public DateTime? LastUsedAt { get; set; }

        [StringLength(50)]
        public string? IpWhitelist { get; set; } // Comma separated IPs, null = any

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        [StringLength(450)]
        public string? RevokedBy { get; set; }

        // Link ke User (opsional, untuk inheritance permission)
        [StringLength(450)]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Scopes/Permissions khusus untuk API Key (JSON array)
        public string? AllowedPermissions { get; set; }

        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        [NotMapped]
        public bool IsActiveAndValid => IsActive && !IsDeleted && !IsRevoked && !IsExpired;

        public void Revoke(string revokedBy)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
            IsActive = false;
        }

        public void UpdateLastUsed()
        {
            LastUsedAt = DateTime.UtcNow;
        }
    }
}
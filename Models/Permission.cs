using ApiGMPKlik.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models
{
    public class Permission : IEntity<int>, IAuditable, ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Module { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;

        // IAuditable
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        // ISoftDeletable
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // IActivatable
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; }

        // Navigation
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public void MarkAsCreated(string userId)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
            IsActive = true;
            IsDeleted = false;
        }

        public void MarkAsUpdated(string userId)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = userId;
        }

        public void SoftDelete(string userId)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = userId;
            IsActive = false;
        }

        public void Restore(string userId)
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            IsActive = true;
            MarkAsUpdated(userId);
        }

        public bool IsValidForOperation() => IsActive && !IsDeleted;
    }
}

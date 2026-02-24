// ApplicationRole.cs
using ApiGMPKlik.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models
{
    public class ApplicationRole : IdentityRole, IEntity<string>, IAuditable, ISoftDeletable
    {
        [StringLength(500)]
        public string? Description { get; set; }

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

        public void MarkAsCreated(string userId)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
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
        }

        public void Restore(string userId)
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            MarkAsUpdated(userId);
        }

        public bool IsValidForOperation() => !IsDeleted;

        // Navigation
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
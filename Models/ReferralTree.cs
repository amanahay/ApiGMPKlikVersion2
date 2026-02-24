using ApiGMPKlik.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models
{
    public class ReferralTree : IEntity<int>, IAuditable, ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string RootUserId { get; set; } = string.Empty;

        [ForeignKey("RootUserId")]
        public virtual ApplicationUser RootUser { get; set; } = null!;

        [StringLength(450)]
        public string ReferredUserId { get; set; } = string.Empty;

        [ForeignKey("ReferredUserId")]
        public virtual ApplicationUser ReferredUser { get; set; } = null!;

        [Range(1, 3)]
        public int Level { get; set; }

        [StringLength(450)]
        public string? ParentUserId { get; set; }

        [ForeignKey("ParentUserId")]
        public virtual ApplicationUser? ParentUser { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionPercent { get; set; } = 0;

        public bool IsActive { get; set; } = true;

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

        public bool IsValidForOperation() => !IsDeleted && IsActive;
    }
}
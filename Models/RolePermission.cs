using ApiGMPKlik.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models
{
    public class RolePermission : IEntity<int>, IAuditable, ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;

        [ForeignKey("RoleId")]
        public virtual ApplicationRole Role { get; set; } = null!;

        [Required]
        public int PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

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
}
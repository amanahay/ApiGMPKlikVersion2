using ApiGMPKlik.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models
{
    public class MasterSecurityQuestion : IEntity<int>, IAuditable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty; // e.g. "Nama Ahli Waris"

        [StringLength(500)]
        public string? Description { get; set; } // Penjelasan jika perlu

        public bool IsRequired { get; set; } = true; // Wajib dijawab?

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "System";
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

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
    }
}
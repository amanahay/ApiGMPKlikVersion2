using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models
{
    public class Branch : NamedEntity
    {
        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsMainBranch { get; set; } = false;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public void AddUser(ApplicationUser user, string currentUserId)
        {
            user.BranchId = Id;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            Users.Add(user);
        }
    }
}
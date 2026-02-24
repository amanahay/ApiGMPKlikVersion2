using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.DTOs
{
    public class CreateUserProfileDto
    {
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Gender { get; set; }

        [StringLength(2000)]
        public string? About { get; set; }

        [StringLength(1000)]
        public string? Promotion { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(255)]
        public string? BirthPlace { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(255)]
        public string? Facebook { get; set; }

        [StringLength(255)]
        public string? Instagram { get; set; }

        [StringLength(255)]
        public string? Twitter { get; set; }

        [StringLength(255)]
        public string? Youtube { get; set; }

        [StringLength(255)]
        public string? Linkedin { get; set; }

        [StringLength(255)]
        public string? Telegram { get; set; }

        [StringLength(255)]
        public string? Website { get; set; }

        [Range(0, 999999999999.99)]
        public decimal? Balance { get; set; }

        [StringLength(50)]
        public string? TaxId { get; set; }

        [Range(0, 100)]
        public decimal? Commission { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(200)]
        public string? HeirName { get; set; }

        [StringLength(20)]
        public string? HeirPhone { get; set; }

        [StringLength(10)]
        public string? Language { get; set; } = "id";

        [StringLength(50)]
        public string? TimeZone { get; set; }

        [StringLength(20)]
        public string? Locale { get; set; }

        public bool NewsletterSubscribed { get; set; } = false;

        public string? Preferences { get; set; }

        public string? Metadata { get; set; }

        [Range(0, 999999999999.99)]
        public decimal? MonthlyBudget { get; set; }

        [Range(0, 999999999999.99)]
        public decimal? UsedBudgetThisMonth { get; set; }

        public DateTime? BudgetResetDate { get; set; }
    }

    public class UpdateUserProfileDto
    {
        [StringLength(50)]
        public string? Gender { get; set; }

        [StringLength(2000)]
        public string? About { get; set; }

        [StringLength(1000)]
        public string? Promotion { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(255)]
        public string? BirthPlace { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(255)]
        public string? Facebook { get; set; }

        [StringLength(255)]
        public string? Instagram { get; set; }

        [StringLength(255)]
        public string? Twitter { get; set; }

        [StringLength(255)]
        public string? Youtube { get; set; }

        [StringLength(255)]
        public string? Linkedin { get; set; }

        [StringLength(255)]
        public string? Telegram { get; set; }

        [StringLength(255)]
        public string? Website { get; set; }

        [Range(0, 999999999999.99)]
        public decimal? Balance { get; set; }

        [StringLength(50)]
        public string? TaxId { get; set; }

        [Range(0, 100)]
        public decimal? Commission { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(200)]
        public string? HeirName { get; set; }

        [StringLength(20)]
        public string? HeirPhone { get; set; }

        [StringLength(10)]
        public string? Language { get; set; }

        [StringLength(50)]
        public string? TimeZone { get; set; }

        [StringLength(20)]
        public string? Locale { get; set; }

        public bool? NewsletterSubscribed { get; set; }

        public string? Preferences { get; set; }

        public string? Metadata { get; set; }

        [Range(0, 999999999999.99)]
        public decimal? MonthlyBudget { get; set; }

        [Range(0, 999999999999.99)]
        public decimal? UsedBudgetThisMonth { get; set; }

        public DateTime? BudgetResetDate { get; set; }
    }

    public class UserProfileFilterDto
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public int? BranchId { get; set; }
        public bool? NewsletterSubscribed { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}

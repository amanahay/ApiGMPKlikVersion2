// DTOs/UserDtos.cs
namespace ApiGMPKlik.DTOs
{
    // Request DTOs

    public class RegisterUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Username { get; set; } // Optional, jika kosong akan digenerate otomatis
        public List<string>? Roles { get; set; }
        public int? BranchId { get; set; }

        // UserProfile data - disesuaikan dengan model yang ada
        public string? Avatar { get; set; } // Ada di ApplicationUser, bukan UserProfile
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; } // BirthDate, bukan DateOfBirth
        public string? BirthPlace { get; set; }
        public string? About { get; set; } // About, bukan Bio
        public string? Gender { get; set; } // Perlu ditambahkan ke UserProfile jika belum ada

        // Social Media & Financial (ada di UserProfile)
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public string? Linkedin { get; set; }
        public string? Telegram { get; set; }
        public string? Website { get; set; }
        public string? TaxId { get; set; }
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
        public decimal? Commission { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsGuest => BranchId == null || BranchId == 0;
    }

    public class LoginDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty; // Bisa username atau email
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public class AssignRolesDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public bool RemoveExisting { get; set; } = false;
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public int? BranchId { get; set; }
        public string? Avatar { get; set; } // Ada di ApplicationUser

        // Profile updates - disesuaikan dengan model
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? About { get; set; }
        public string? Gender { get; set; }

        // Social Media
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public string? Linkedin { get; set; }
        public string? Telegram { get; set; }
        public string? Website { get; set; }

        // Financial
        public string? TaxId { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? HeirName { get; set; }
        public string? HeirPhone { get; set; }

        // Settings
        public string? Language { get; set; }
        public string? TimeZone { get; set; }
        public string? Locale { get; set; }
        public bool? NewsletterSubscribed { get; set; }
        public string? Preferences { get; set; }
        public string? Metadata { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public decimal? UsedBudgetThisMonth { get; set; }
        public decimal? Commission { get; set; }
    }

    // UserProfile DTO - disesuaikan dengan model
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? FullName { get; set; }
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
        public string? Language { get; set; }
        public string? TimeZone { get; set; }
        public string? Locale { get; set; }
        public bool NewsletterSubscribed { get; set; }
        public string? Preferences { get; set; }
        public string? Metadata { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public decimal? UsedBudgetThisMonth { get; set; }
        public DateTime? BudgetResetDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        // Security Info
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }

    // Response DTOs

    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; } // Ada di ApplicationUser
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }

        // Extended info untuk admin
        public UserProfileDto? Profile { get; set; }
        public bool HasSecurityQuestions { get; set; }

    }

    // Public User DTO (untuk endpoint public by username)
    public class PublicUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? About { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }

    // Filter DTO untuk GetAll
    public class UserFilterDto
    {
        public int? BranchId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string? Search { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    // Request DTOs
    public class ForgotPasswordRequestDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
    }

    public class GetSecurityQuestionsDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
    }

    public class VerifySecurityAnswersDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public List<SecurityAnswerDto> Answers { get; set; } = new List<SecurityAnswerDto>();
    }

    public class SecurityAnswerDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; } = string.Empty; // Plain text, akan di-hash di server
    }

    public class ResetPasswordWithVerificationDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty; // Token sementara setelah jawaban benar
        public string NewPassword { get; set; } = string.Empty;
    }

    // Response DTOs
    public class SecurityQuestionDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class ForgotPasswordInitiateResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<SecurityQuestionDto> Questions { get; set; } = new List<SecurityQuestionDto>();
        public int AttemptsRemaining { get; set; } = 3;
    }

    // DTO untuk user submit jawaban (pertanyaan sudah ditentukan sistem)
    public class UserSecurityAnswerDto
    {
        public int MasterQuestionId { get; set; } // ID pertanyaan dari sistem
        public string Answer { get; set; } = string.Empty; // Jawaban user
    }

    // Response untuk Get Questions (tampilkan ke user saat setup)
    public class MasterSecurityQuestionDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }
}
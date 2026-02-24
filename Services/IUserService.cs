using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Models;
using ApiGMPKlik.Shared;
using Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGMPKlik.Services
{
    public class SecurityQuestionSetupDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty; // Will be hashed
    }
    public interface IUserService
    {
        Task<ApiResponse<UserResponseDto>> RegisterAsync(RegisterUserDto dto);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ApiResponse<string>> LogoutAsync(string userId);
        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(string userId, bool includeDeleted = false);
        Task<ApiResponse<UserResponseDto>> GetUserByEmailAsync(string email);
        Task<ApiResponse<UserResponseDto>> GetUserByUsernameAsync(string username);
        Task<ApiResponse<PublicUserDto>> GetPublicUserByUsernameAsync(string username);
        Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync(UserFilterDto? filter = null, int page = 1, int pageSize = 10);
        Task<ApiResponse<UserResponseDto>> UpdateUserAsync(string userId, UpdateUserDto dto);
        Task<ApiResponse<bool>> DeleteUserAsync(string userId, string deletedBy);
        Task<ApiResponse<bool>> RestoreUserAsync(string userId);
        Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId);
        Task<ApiResponse<bool>> AssignRolesAsync(string userId, List<string> roles, bool removeExisting = false);
        Task<ApiResponse<bool>> RemoveFromRoleAsync(string userId, string role);
        Task<ApiResponse<bool>> CreateRoleAsync(string roleName);
        Task<ApiResponse<List<string>>> GetAllRolesAsync();
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<ApiResponse<bool>> ResetPasswordAsync(string userId, string newPassword);
        Task<ApiResponse<string>> GeneratePasswordResetTokenAsync(string email);
        Task<bool> IsSuperAdminAsync(string userId);
        Task<ApiResponse<bool>> BypassPasswordAsync(BypassPasswordRequestDto dto);

        /// <summary>
        /// Inisiasi lupa password - kembalikan pertanyaan keamanan user
        /// </summary>
        Task<ApiResponse<ForgotPasswordInitiateResponseDto>> GetSecurityQuestionsAsync(string usernameOrEmail);

        /// <summary>
        /// Verifikasi jawaban pertanyaan keamanan
        /// </summary>
        Task<ApiResponse<string>> VerifySecurityAnswersAsync(VerifySecurityAnswersDto dto);

        /// <summary>
        /// Reset password setelah verifikasi jawaban benar
        /// </summary>
        Task<ApiResponse<bool>> ResetPasswordAfterVerificationAsync(ResetPasswordWithVerificationDto dto);

        /// <summary>
        /// Setup/update pertanyaan keamanan (untuk profil user)
        /// </summary>
        Task<ApiResponse<bool>> SetupSecurityQuestionsAsync(string userId, List<SecurityQuestionSetupDto> questions);
        // Ambil daftar pertanyaan wajib dari sistem (untuk ditampilkan ke user)
        Task<ApiResponse<List<MasterSecurityQuestionDto>>> GetMandatorySecurityQuestionsAsync();

        // Setup jawaban user (pertanyaan sudah ditentukan, hanya submit jawaban)
        Task<ApiResponse<bool>> SetupSecurityAnswersAsync(string userId, List<UserSecurityAnswerDto> answers);

        Task<ApiResponse<bool>> DeleteUserAsync(string userId, string deletedBy, bool autoPromoteDownlines = true);

    }

    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReferralTreeService _referralTreeService;
        private readonly ApplicationDbContext _context;  // ← TAMBAHKAN INI

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor, IReferralTreeService referralTreeService,
            ApplicationDbContext context)  // ← TAMBAHKAN PARAMETER INI
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _referralTreeService = referralTreeService;
            _context = context;  // ← ASSIGN KE FIELD
        }
        public async Task<ApiResponse<UserResponseDto>> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<UserResponseDto>.Error(
                        "Registration failed",
                        "Email already registered",
                        409);
                }

                string username = dto.Username ?? GenerateUsernameFromEmail(dto.Email);

                var existingUsername = await _userManager.FindByNameAsync(username);
                if (existingUsername != null)
                {
                    username = $"{username}{Random.Shared.Next(100, 999)}";
                }

                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    BranchId = dto.BranchId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true,
                    Avatar = dto.Avatar // Set avatar jika ada
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => new ErrorDetail
                    {
                        Message = e.Description,
                        Code = e.Code
                    }).ToList();

                    return ApiResponse<UserResponseDto>.ValidationError(errors);
                }

                // Buat UserProfile dengan data yang ada
                if (dto.Address != null || dto.BirthDate.HasValue || dto.About != null ||
                    dto.Gender != null || dto.Facebook != null || dto.Instagram != null ||
                    dto.Twitter != null || dto.Youtube != null || dto.Linkedin != null ||
                    dto.Telegram != null || dto.Website != null || dto.TaxId != null ||
                    dto.BankName != null || dto.BankAccountNumber != null || dto.HeirName != null ||
                    dto.HeirPhone != null || dto.Language != null || dto.TimeZone != null ||
                    dto.Locale != null || dto.Preferences != null || dto.Metadata != null ||
                    dto.MonthlyBudget.HasValue || dto.UsedBudgetThisMonth.HasValue || dto.Commission.HasValue)
                {
                    user.Profile = new UserProfile
                    {
                        UserId = user.Id,
                        Address = dto.Address,
                        BirthDate = dto.BirthDate,
                        BirthPlace = dto.BirthPlace,
                        About = dto.About,
                        Gender = dto.Gender,
                        Facebook = dto.Facebook,
                        Instagram = dto.Instagram,
                        Twitter = dto.Twitter,
                        Youtube = dto.Youtube,
                        Linkedin = dto.Linkedin,
                        Telegram = dto.Telegram,
                        Website = dto.Website,
                        TaxId = dto.TaxId,
                        BankName = dto.BankName,
                        BankAccountNumber = dto.BankAccountNumber,
                        HeirName = dto.HeirName,
                        HeirPhone = dto.HeirPhone,
                        Language = dto.Language ?? "id",
                        TimeZone = dto.TimeZone,
                        Locale = dto.Locale,
                        NewsletterSubscribed = dto.NewsletterSubscribed,
                        Preferences = dto.Preferences,
                        Metadata = dto.Metadata,
                        MonthlyBudget = dto.MonthlyBudget,
                        UsedBudgetThisMonth = dto.UsedBudgetThisMonth,
                        Commission = dto.Commission,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _userManager.UpdateAsync(user);
                }

                // Buat UserSecurity
                user.Security = new UserSecurity
                {
                    UserId = user.Id,
                    LastLoginAt = null,
                    FailedLoginAttempts = 0
                };
                await _userManager.UpdateAsync(user);

                var roles = dto.Roles ?? new List<string> { "Tamu" };
                var roleResult = await AssignRolesAsync(user.Id, roles, false);

                if (!roleResult.IsSuccess)
                {
                    await _userManager.DeleteAsync(user);
                    return ApiResponse<UserResponseDto>.Error(
                        "Failed to assign roles",
                        roleResult.Errors?.Select(e => e.Message).ToList() ?? new List<string>(),
                        400);
                }

                var userDto = await MapToUserResponseDto(user, true);

                _logger.LogInformation("User registered successfully: {Email} with Username: {Username}", dto.Email, username);

                return ApiResponse<UserResponseDto>.Created(userDto, "User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return ApiResponse<UserResponseDto>.Danger("Internal server error", ex);
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                ApplicationUser? user = null;

                // FIX: Gunakan Include untuk load Security agar tidak duplicate
                if (dto.UsernameOrEmail.Contains("@"))
                {
                    user = await _userManager.Users
                        .Include(u => u.Security)
                        .FirstOrDefaultAsync(u => u.Email == dto.UsernameOrEmail);
                }
                else
                {
                    user = await _userManager.Users
                        .Include(u => u.Security)
                        .FirstOrDefaultAsync(u => u.UserName == dto.UsernameOrEmail);
                }

                if (user == null)
                    return ApiResponse<LoginResponseDto>.Error("Login failed", "Invalid credentials", 401);

                if (!user.IsActive)
                    return ApiResponse<LoginResponseDto>.Warning("Account inactive", new List<string> { "Please contact administrator" });

                if (user.IsDeleted)
                    return ApiResponse<LoginResponseDto>.Error("Login failed", "Account not found", 401);

                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);

                if (result.IsLockedOut)
                    return ApiResponse<LoginResponseDto>.Danger("Account locked", "Too many failed attempts. Try again later.");

                if (!result.Succeeded)
                    return ApiResponse<LoginResponseDto>.Error("Login failed", "Invalid credentials", 401);

                // FIX: Cek apakah Security sudah ada di DB atau belum
                if (user.Security == null)
                {
                    // Double check ke database (safety net)
                    var existingSecurity = await _userManager.Users
                        .Where(u => u.Id == user.Id)
                        .Select(u => u.Security)
                        .FirstOrDefaultAsync();

                    if (existingSecurity != null)
                    {
                        user.Security = existingSecurity;
                    }
                    else
                    {
                        // Baru buat jika memang belum ada
                        user.Security = new UserSecurity
                        {
                            UserId = user.Id,
                            LastLoginAt = DateTime.UtcNow,
                            LastLoginIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                            FailedLoginAttempts = 0
                        };
                    }
                }
                else
                {
                    // Update existing
                    user.Security.LastLoginAt = DateTime.UtcNow;

                    var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    if (!string.IsNullOrEmpty(ipAddress))
                        user.Security.LastLoginIp = ipAddress;

                    user.Security.FailedLoginAttempts = 0;
                }

                // Update user (ini akan update Security juga karena relasi sudah load)
                await _userManager.UpdateAsync(user);

                var token = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                var response = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = await MapToUserResponseDto(user, true)
                };

                return ApiResponse<LoginResponseDto>.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse<LoginResponseDto>.Danger("Login failed", ex);
            }
        }

        public Task<ApiResponse<string>> LogoutAsync(string userId)
        {
            return Task.FromResult(ApiResponse<string>.Success("Logged out successfully"));
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(string userId, bool includeDeleted = false)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .Include(u => u.Security)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponse<UserResponseDto>.NotFound("User");

            if (user.IsDeleted && !includeDeleted)
                return ApiResponse<UserResponseDto>.NotFound("User");

            var dto = await MapToUserResponseDto(user, true);
            return ApiResponse<UserResponseDto>.Success(dto);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .Include(u => u.Security)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.IsDeleted)
                return ApiResponse<UserResponseDto>.NotFound("User");

            var dto = await MapToUserResponseDto(user, true);
            return ApiResponse<UserResponseDto>.Success(dto);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByUsernameAsync(string username)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .Include(u => u.Security)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null || user.IsDeleted)
                return ApiResponse<UserResponseDto>.NotFound("User");

            var dto = await MapToUserResponseDto(user, true);
            return ApiResponse<UserResponseDto>.Success(dto);
        }

        public async Task<ApiResponse<PublicUserDto>> GetPublicUserByUsernameAsync(string username)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted && u.IsActive);

            if (user == null)
                return ApiResponse<PublicUserDto>.NotFound("User");

            var roles = await _userManager.GetRolesAsync(user);

            var dto = new PublicUserDto
            {
                Username = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Avatar = user.Avatar, // Dari ApplicationUser
                About = user.Profile?.About,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                IsActive = user.IsActive
            };

            return ApiResponse<PublicUserDto>.Success(dto);
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync(UserFilterDto? filter = null, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users
                .Include(u => u.Profile)
                .Include(u => u.Branch)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.BranchId.HasValue)
                {
                    query = query.Where(u => u.BranchId == filter.BranchId.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == filter.IsActive.Value);
                }

                if (filter.IsDeleted.HasValue)
                {
                    query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);
                }
                else
                {
                    query = query.Where(u => !u.IsDeleted);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                        (u.Email != null && u.Email.ToLower().Contains(search)) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.Role))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(filter.Role);
                    var userIds = usersInRole.Select(u => u.Id).ToList();
                    query = query.Where(u => userIds.Contains(u.Id));
                }

                if (filter.CreatedFrom.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
                }
                if (filter.CreatedTo.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    query = filter.SortBy.ToLower() switch
                    {
                        "username" => filter.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                        "email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                        "fullname" => filter.SortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                        "isactive" => filter.SortDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
                        _ => filter.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
                    };
                }
                else
                {
                    query = query.OrderByDescending(u => u.CreatedAt);
                }
            }
            else
            {
                query = query.Where(u => !u.IsDeleted)
                            .OrderByDescending(u => u.CreatedAt);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<UserResponseDto>();
            foreach (var user in users)
            {
                dtos.Add(await MapToUserResponseDto(user, true));
            }

            return ApiResponse<List<UserResponseDto>>.Paginated(dtos, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponse<UserResponseDto>.NotFound("User");

            var isTargetSuperAdmin = await IsSuperAdminAsync(userId);
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);
            var isCurrentSuperAdmin = currentUser != null && await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");

            if (isTargetSuperAdmin && !isCurrentSuperAdmin)
            {
                return ApiResponse<UserResponseDto>.Forbidden("SuperAdmin hanya dapat diubah oleh SuperAdmin itu sendiri");
            }

            if (isTargetSuperAdmin && dto.IsActive.HasValue && !dto.IsActive.Value)
            {
                return ApiResponse<UserResponseDto>.Error("Operasi Dilarang", "Status SuperAdmin tidak dapat dinonaktifkan", 403);
            }

            // Update ApplicationUser
            user.FullName = dto.FullName ?? user.FullName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.BranchId = dto.BranchId ?? user.BranchId;
            user.Avatar = dto.Avatar ?? user.Avatar;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            // Update atau Create Profile
            if (user.Profile == null && HasAnyProfileData(dto))
            {
                user.Profile = new UserProfile { UserId = user.Id };
            }

            if (user.Profile != null)
            {
                user.Profile.About = dto.About ?? user.Profile.About;
                user.Profile.Address = dto.Address ?? user.Profile.Address;
                user.Profile.BirthDate = dto.BirthDate ?? user.Profile.BirthDate;
                user.Profile.BirthPlace = dto.BirthPlace ?? user.Profile.BirthPlace;
                user.Profile.Gender = dto.Gender ?? user.Profile.Gender;
                user.Profile.Facebook = dto.Facebook ?? user.Profile.Facebook;
                user.Profile.Instagram = dto.Instagram ?? user.Profile.Instagram;
                user.Profile.Twitter = dto.Twitter ?? user.Profile.Twitter;
                user.Profile.Youtube = dto.Youtube ?? user.Profile.Youtube;
                user.Profile.Linkedin = dto.Linkedin ?? user.Profile.Linkedin;
                user.Profile.Telegram = dto.Telegram ?? user.Profile.Telegram;
                user.Profile.Website = dto.Website ?? user.Profile.Website;
                user.Profile.TaxId = dto.TaxId ?? user.Profile.TaxId;
                user.Profile.BankName = dto.BankName ?? user.Profile.BankName;
                user.Profile.BankAccountNumber = dto.BankAccountNumber ?? user.Profile.BankAccountNumber;
                user.Profile.HeirName = dto.HeirName ?? user.Profile.HeirName;
                user.Profile.HeirPhone = dto.HeirPhone ?? user.Profile.HeirPhone;
                user.Profile.Language = dto.Language ?? user.Profile.Language;
                user.Profile.TimeZone = dto.TimeZone ?? user.Profile.TimeZone;
                user.Profile.Locale = dto.Locale ?? user.Profile.Locale;
                user.Profile.NewsletterSubscribed = dto.NewsletterSubscribed ?? user.Profile.NewsletterSubscribed;
                user.Profile.Preferences = dto.Preferences ?? user.Profile.Preferences;
                user.Profile.Metadata = dto.Metadata ?? user.Profile.Metadata;
                user.Profile.MonthlyBudget = dto.MonthlyBudget ?? user.Profile.MonthlyBudget;
                user.Profile.UsedBudgetThisMonth = dto.UsedBudgetThisMonth ?? user.Profile.UsedBudgetThisMonth;
                user.Profile.Commission = dto.Commission ?? user.Profile.Commission;
                user.Profile.ModifiedAt = DateTime.UtcNow;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new ErrorDetail { Message = e.Description }).ToList();
                return ApiResponse<UserResponseDto>.ValidationError(errors);
            }

            return ApiResponse<UserResponseDto>.Success(
                await MapToUserResponseDto(user, true),
                "User updated successfully");
        }

        private bool HasAnyProfileData(UpdateUserDto dto)
        {
            return dto.About != null || dto.Address != null || dto.BirthDate.HasValue ||
                   dto.BirthPlace != null || dto.Gender != null || dto.Facebook != null ||
                   dto.Instagram != null || dto.Twitter != null || dto.Youtube != null ||
                   dto.Linkedin != null || dto.Telegram != null || dto.Website != null ||
                   dto.TaxId != null || dto.BankName != null || dto.BankAccountNumber != null ||
                   dto.HeirName != null || dto.HeirPhone != null || dto.Language != null ||
                   dto.TimeZone != null || dto.Locale != null || dto.NewsletterSubscribed.HasValue ||
                   dto.Preferences != null || dto.Metadata != null || dto.MonthlyBudget.HasValue ||
                   dto.UsedBudgetThisMonth.HasValue || dto.Commission.HasValue;
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string userId, string deletedBy)
        {
            if (await IsSuperAdminAsync(userId))
            {
                return ApiResponse<bool>.Error("Akses Ditolak", "SuperAdmin tidak dapat dihapus", 403);
            }

            var deleter = await _userManager.FindByIdAsync(deletedBy);
            var isDeleterSuperAdmin = await _userManager.IsInRoleAsync(deleter!, "SuperAdmin");

            if (!isDeleterSuperAdmin && !await _userManager.IsInRoleAsync(deleter!, "Admin"))
            {
                return ApiResponse<bool>.Forbidden("Hanya SuperAdmin atau Admin yang dapat menghapus user");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            // Panggil AutoPromoteDownlinesAsync sebelum soft delete user
            // Pastikan IReferralTreeService diinject di constructor UserService
            var promoteResult = await _referralTreeService.AutoPromoteDownlinesAsync(userId, deletedBy);

            if (!promoteResult.IsSuccess)
            {
                _logger.LogWarning("Auto-promote gagal untuk {UserId}: {Message}", userId, promoteResult.Message);
                // Lanjutkan tetap delete user meskipun promote gagal, atau bisa return error tergantung kebutuhan
            }

            user.SoftDelete(deletedBy);
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? ApiResponse<bool>.Success(true, "User dihapus dan downline dipromosikan")
                : ApiResponse<bool>.Error("Delete failed", "Failed to update user", 500);
        }

        public async Task<ApiResponse<bool>> RestoreUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            user.Restore(userId);

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? ApiResponse<bool>.Success(true, "User restored successfully")
                : ApiResponse<bool>.Error("Restore failed", "Failed to update user", 500);
        }

        public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<List<string>>.NotFound("User");

            var roles = await _userManager.GetRolesAsync(user);
            return ApiResponse<List<string>>.Success(roles.ToList());
        }

        public async Task<ApiResponse<bool>> AssignRolesAsync(string userId, List<string> roles, bool removeExisting = false)
        {
            if (await IsSuperAdminAsync(userId))
            {
                return ApiResponse<bool>.Error("Akses Ditolak", "Role SuperAdmin tidak dapat diubah", 403);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }

            if (removeExisting)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            var result = await _userManager.AddToRolesAsync(user, roles);

            return result.Succeeded
                ? ApiResponse<bool>.Success(true, "Roles assigned successfully")
                : ApiResponse<bool>.Error("Failed to assign roles",
                    result.Errors.Select(e => e.Description).ToList(), 400);
        }

        public async Task<ApiResponse<bool>> RemoveFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded
                ? ApiResponse<bool>.Success(true, $"Removed from role {role}")
                : ApiResponse<bool>.Error("Failed to remove role",
                    result.Errors.Select(e => e.Description).ToList(), 400);
        }

        public async Task<ApiResponse<bool>> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return ApiResponse<bool>.Error("Role already exists", "Conflict", 409);

            var result = await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper()
            });
            return result.Succeeded
                ? ApiResponse<bool>.Success(true, "Role created successfully")
                : ApiResponse<bool>.Error("Failed to create role",
                    result.Errors.Select(e => e.Description).ToList(), 400);
        }

        public async Task<ApiResponse<List<string>>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles
                .Select(r => r.Name!)
                .ToListAsync();

            return ApiResponse<List<string>>.Success(roles);
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded
                ? ApiResponse<bool>.Success(true, "Password changed successfully")
                : ApiResponse<bool>.Error("Failed to change password",
                    result.Errors.Select(e => e.Description).ToList(), 400);
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            return result.Succeeded
                ? ApiResponse<bool>.Success(true, "Password reset successfully")
                : ApiResponse<bool>.Error("Failed to reset password",
                    result.Errors.Select(e => e.Description).ToList(), 400);
        }

        public async Task<ApiResponse<string>> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiResponse<string>.NotFound("User");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return ApiResponse<string>.Success(token, "Token generated");
        }

        public async Task<bool> IsSuperAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, "SuperAdmin");
        }
        public async Task<ApiResponse<bool>> BypassPasswordAsync(BypassPasswordRequestDto dto)
        {
            try
            {
                // Validasi input tidak boleh kosong
                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Username) ||
                    string.IsNullOrWhiteSpace(dto.Pin) ||
                    string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = "Email, Username, PIN, dan NewPassword wajib diisi" }
            });
                }

                // 1. Validasi PIN Developer dari Configuration
                var devPin = _configuration["StaticAuth:Pin"];
                if (string.IsNullOrEmpty(devPin) || dto.Pin != devPin)
                {
                    _logger.LogWarning("Bypass password attempt with invalid PIN for email: {Email}", dto.Email);
                    return ApiResponse<bool>.Error(
                        "Akses Ditolak",
                        "PIN Developer tidak valid",
                        403);
                }

                // 2. Cari user berdasarkan Email (case insensitive)
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Bypass password failed: Email {Email} not found", dto.Email);
                    return ApiResponse<bool>.NotFound("User dengan email tersebut");
                }

                // 3. Verifikasi kecocokan Username (case sensitive untuk keamanan ekstra)
                if (!string.Equals(user.UserName, dto.Username, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Bypass password failed: Username mismatch for email {Email}. Expected: {Expected}, Got: {Got}",
                        dto.Email, user.UserName, dto.Username);
                    return ApiResponse<bool>.Error(
                        "Verifikasi Gagal",
                        "Username tidak cocok dengan Email yang terdaftar",
                        400);
                }

                // 4. Dapatkan info IP untuk audit
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var isTargetSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");

                // Jika target adalah SuperAdmin, log sebagai CRITICAL (tetapi tetap izinkan karena PIN valid)
                if (isTargetSuperAdmin)
                {
                    _logger.LogCritical("🚨 SUPERADMIN BYPASS DETECTED: Email={Email}, Username={Username}, IP={IP}, Time={Time}",
                        dto.Email, dto.Username, ipAddress, DateTime.UtcNow);
                }

                // 5. Validasi kekuatan password baru
                var validators = _userManager.PasswordValidators;
                var passwordValidationTasks = validators.Select(v => v.ValidateAsync(_userManager, user, dto.NewPassword));
                var validationResults = await Task.WhenAll(passwordValidationTasks);

                var errors = validationResults
                    .Where(r => !r.Succeeded)
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (errors.Any())
                {
                    var errorDetails = errors.Select(e => new ErrorDetail
                    {
                        Message = e.Description,
                        Code = e.Code
                    }).ToList();

                    return ApiResponse<bool>.ValidationError(errorDetails, "Password baru tidak memenuhi kriteria keamanan");
                }

                // 6. Reset password langsung tanpa token (bypass)
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Failed to remove password for user {UserId}", user.Id);
                    return ApiResponse<bool>.Error(
                        "Gagal mengubah password",
                        "Tidak dapat menghapus password lama",
                        500);
                }

                var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add new password for user {UserId}", user.Id);
                    return ApiResponse<bool>.Error(
                        "Gagal mengubah password",
                        addResult.Errors.Select(e => e.Description).ToList(),
                        500);
                }

                // 7. Update security info
                if (user.Security != null)
                {
                    user.Security.LastPasswordChangeAt = DateTime.UtcNow;
                    user.Security.PasswordChangedBy = $"DEV_BYPASS_{ipAddress}"; // Sertakan IP di audit
                    user.Security.RequirePasswordChange = false;
                }

                // 8. Simpan perubahan (termasuk Security jika ada)
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogWarning("Password changed but failed to update security info for {UserId}", user.Id);
                    // Tetap return sukses karena password sudah berubah, tapi log warning
                }

                // 9. Log aktivitas bypass (penting untuk audit)
                _logger.LogWarning("✅ DEVELOPER BYPASS SUCCESS: User={UserId} ({Email}), IP={IP}, IsSuperAdmin={IsSuperAdmin}",
                    user.Id, user.Email, ipAddress, isTargetSuperAdmin);

                return ApiResponse<bool>.Success(true,
                    $"Password untuk user '{user.UserName}' berhasil diubah via Developer Bypass");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BypassPasswordAsync for email: {Email}", dto.Email);
                return ApiResponse<bool>.Danger("Internal server error", ex);
            }
        }
        public async Task<ApiResponse<ForgotPasswordInitiateResponseDto>> GetSecurityQuestionsAsync(string usernameOrEmail)
        {
            try
            {
                ApplicationUser? user = null;

                if (usernameOrEmail.Contains("@"))
                    user = await _userManager.FindByEmailAsync(usernameOrEmail);
                else
                    user = await _userManager.FindByNameAsync(usernameOrEmail);

                if (user == null || user.IsDeleted)
                    return ApiResponse<ForgotPasswordInitiateResponseDto>.NotFound("User");

                // Cek apakah user punya security questions
                var questions = await _context.UserSecurityQuestions
                    .Where(q => q.UserId == user.Id)
                    .OrderBy(q => q.SortOrder)
                    .Select(q => new SecurityQuestionDto
                    {
                        Id = q.Id,
                        Question = q.Question,
                        SortOrder = q.SortOrder
                    })
                    .ToListAsync();

                if (!questions.Any())
                {
                    return ApiResponse<ForgotPasswordInitiateResponseDto>.Error(
                        "Tidak dapat reset password",
                        "User belum mengatur pertanyaan keamanan. Hubungi Administrator.",
                        400);
                }

                // Cek attempts di UserSecurity (anti brute force)
                if (user.Security?.FailedLoginAttempts >= 3)
                {
                    return ApiResponse<ForgotPasswordInitiateResponseDto>.Error(
                        "Akun terkunci",
                        "Terlalu banyak percobaan gagal. Hubungi Administrator.",
                        403);
                }

                var response = new ForgotPasswordInitiateResponseDto
                {
                    UserId = user.Id,
                    Username = user.UserName!,
                    Questions = questions,
                    AttemptsRemaining = 3 - (user.Security?.FailedLoginAttempts ?? 0)
                };

                return ApiResponse<ForgotPasswordInitiateResponseDto>.Success(response,
                    "Silakan jawab pertanyaan keamanan berikut");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security questions for {Identifier}", usernameOrEmail);
                return ApiResponse<ForgotPasswordInitiateResponseDto>.Danger("Error", ex);
            }
        }

        public async Task<ApiResponse<string>> VerifySecurityAnswersAsync(VerifySecurityAnswersDto dto)
        {
            try
            {
                ApplicationUser? user = null;

                if (dto.UsernameOrEmail.Contains("@"))
                    user = await _userManager.FindByEmailAsync(dto.UsernameOrEmail);
                else
                    user = await _userManager.FindByNameAsync(dto.UsernameOrEmail);

                if (user == null || user.IsDeleted)
                    return ApiResponse<string>.NotFound("User");

                // Validasi minimal 3 pertanyaan harus dijawab
                if (dto.Answers.Count < 3)
                {
                    return ApiResponse<string>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = "Minimal 3 pertanyaan harus dijawab" }
            });
                }

                // Ambil semua pertanyaan user dari DB
                var dbQuestions = await _context.UserSecurityQuestions
                    .Where(q => q.UserId == user.Id)
                    .ToListAsync();

                int correctAnswers = 0;

                foreach (var answer in dto.Answers)
                {
                    var question = dbQuestions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question == null) continue;

                    // Verify answer (case insensitive, trim whitespace)
                    // Anda bisa ganti dengan BCrypt.Verify jika pakai BCrypt
                    var inputHash = HashAnswer(answer.Answer);
                    if (inputHash.Equals(question.AnswerHash, StringComparison.OrdinalIgnoreCase))
                    {
                        correctAnswers++;
                    }
                }

                // Minimal 2 dari 3 benar (toleransi typo 1)
                if (correctAnswers < 2)
                {
                    // Increment failed attempts
                    if (user.Security != null)
                    {
                        user.Security.FailedLoginAttempts++;
                        user.Security.LastFailedLoginAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    int remaining = 3 - (user.Security?.FailedLoginAttempts ?? 0);

                    if (remaining <= 0)
                    {
                        return ApiResponse<string>.Error("Akun terkunci",
                            "Terlalu banyak percobaan gagal. Hubungi Administrator.", 403);
                    }

                    return ApiResponse<string>.Error("Verifikasi gagal",
                        $"Jawaban tidak cocok. Sisa percobaan: {remaining} kali.", 400);
                }

                // Reset failed attempts on success
                if (user.Security != null)
                {
                    user.Security.FailedLoginAttempts = 0;
                }

                // Generate temporary verification token (valid 15 menit)
                var verificationToken = Guid.NewGuid().ToString("N");

                // Simpan token ke cache/memory (atau buat tabel khusus)
                // Di contoh ini saya simpan ke UserSecurity.TempToken (perlu tambah field)
                if (user.Security != null)
                {
                    // Anda perlu tambah field TempToken dan TempTokenExpiry di UserSecurity
                    // Atau pakai IMemoryCache/Redis
                    user.Security.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                    await _userManager.UpdateAsync(user);
                }

                // Log success
                _logger.LogInformation("Security questions verified for user {UserId}", user.Id);

                return ApiResponse<string>.Success(verificationToken,
                    "Verifikasi berhasil. Silakan reset password dalam 15 menit.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying security answers");
                return ApiResponse<string>.Danger("Error", ex);
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAfterVerificationAsync(ResetPasswordWithVerificationDto dto)
        {
            try
            {
                ApplicationUser? user = null;

                if (dto.UsernameOrEmail.Contains("@"))
                    user = await _userManager.FindByEmailAsync(dto.UsernameOrEmail);
                else
                    user = await _userManager.FindByNameAsync(dto.UsernameOrEmail);

                if (user == null || user.IsDeleted)
                    return ApiResponse<bool>.NotFound("User");

                // Validasi token (cek expiry)
                if (user.Security?.PasswordResetTokenExpiry == null ||
                    user.Security.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return ApiResponse<bool>.Error("Token expired",
                        "Sesi reset password sudah expired. Silakan ulangi dari awal.", 401);
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ValidationError(
                        result.Errors.Select(e => new ErrorDetail { Message = e.Description }).ToList(),
                        "Password tidak valid");
                }

                // Clear token expiry
                if (user.Security != null)
                {
                    user.Security.PasswordResetTokenExpiry = null;
                    user.Security.LastPasswordChangeAt = DateTime.UtcNow;
                    user.Security.PasswordChangedBy = "SELF_RESET_VIA_QUESTIONS";
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("Password reset successful for user {UserId} via security questions", user.Id);

                return ApiResponse<bool>.Success(true,
                    "Password berhasil direset. Silakan login dengan password baru.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password after verification");
                return ApiResponse<bool>.Danger("Error", ex);
            }
        }
        public async Task<ApiResponse<bool>> SetupSecurityQuestionsAsync(string userId, List<SecurityQuestionSetupDto> questions)
        {
            try
            {
                // Validasi: Harus ada tepat 3 pertanyaan
                if (questions == null || questions.Count != 3)
                {
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = "Harus menyediakan tepat 3 pertanyaan keamanan" }
            });
                }

                // Validasi: Pertanyaan dan jawaban tidak boleh kosong
                if (questions.Any(q => string.IsNullOrWhiteSpace(q.Question) || string.IsNullOrWhiteSpace(q.Answer)))
                {
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = "Pertanyaan dan jawaban tidak boleh kosong" }
            });
                }

                // Cek user exists
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                // Hapus pertanyaan lama jika ada (replace)
                var existingQuestions = await _context.UserSecurityQuestions
                    .Where(q => q.UserId == userId)
                    .ToListAsync();

                if (existingQuestions.Any())
                {
                    _context.UserSecurityQuestions.RemoveRange(existingQuestions);
                    await _context.SaveChangesAsync(); // Save dulu untuk menghindari konflik
                }

                // Tambah pertanyaan baru
                for (int i = 0; i < questions.Count; i++)
                {
                    var q = questions[i];
                    var securityQuestion = new UserSecurityQuestion
                    {
                        UserId = userId,
                        Question = q.Question.Trim(),
                        AnswerHash = HashAnswer(q.Answer), // Gunakan helper yang sudah ada
                        SortOrder = i + 1,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    await _context.UserSecurityQuestions.AddAsync(securityQuestion);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Security questions setup successful for user {UserId}", userId);

                return ApiResponse<bool>.Success(true, "Pertanyaan keamanan berhasil disimpan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up security questions for user {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal menyimpan pertanyaan keamanan", ex);
            }
        }
        public async Task<ApiResponse<List<MasterSecurityQuestionDto>>> GetMandatorySecurityQuestionsAsync()
        {
            try
            {
                var questions = await _context.MasterSecurityQuestions
                    .Where(q => q.IsActive && q.IsRequired)
                    .OrderBy(q => q.SortOrder)
                    .Select(q => new MasterSecurityQuestionDto
                    {
                        Id = q.Id,
                        Question = q.Question,
                        Description = q.Description,
                        SortOrder = q.SortOrder
                    })
                    .ToListAsync();

                if (!questions.Any())
                {
                    return ApiResponse<List<MasterSecurityQuestionDto>>.Error(
                        "Belum diatur",
                        "Sistem belum mengatur pertanyaan keamanan. Hubungi Administrator.",
                        500);
                }

                return ApiResponse<List<MasterSecurityQuestionDto>>.Success(questions,
                    $"Silakan jawab {questions.Count} pertanyaan keamanan berikut");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mandatory security questions");
                return ApiResponse<List<MasterSecurityQuestionDto>>.Danger("Error", ex);
            }
        }

        public async Task<ApiResponse<bool>> SetupSecurityAnswersAsync(string userId, List<UserSecurityAnswerDto> answers)
        {
            try
            {
                // Ambil semua pertanyaan wajib
                var requiredQuestions = await _context.MasterSecurityQuestions
                    .Where(q => q.IsActive && q.IsRequired)
                    .ToListAsync();

                // Validasi: User harus jawab SEMUA pertanyaan wajib
                if (answers.Count != requiredQuestions.Count)
                {
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = $"Harus menjawab semua {requiredQuestions.Count} pertanyaan" }
            });
                }

                // Validasi: Semua pertanyaan wajib harus dijawab
                var answeredQuestionIds = answers.Select(a => a.MasterQuestionId).ToList();
                var missingQuestions = requiredQuestions
                    .Where(q => !answeredQuestionIds.Contains(q.Id))
                    .Select(q => q.Question)
                    .ToList();

                if (missingQuestions.Any())
                {
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = $"Pertanyaan belum dijawab: {string.Join(", ", missingQuestions)}" }
            });
                }

                // Validasi: Jawaban tidak boleh kosong
                if (answers.Any(a => string.IsNullOrWhiteSpace(a.Answer)))
                {
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = "Semua jawaban wajib diisi" }
            });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                // Hapus jawaban lama jika ada (update)
                var existingAnswers = await _context.UserSecurityQuestions
                    .Where(q => q.UserId == userId)
                    .ToListAsync();

                if (existingAnswers.Any())
                {
                    _context.UserSecurityQuestions.RemoveRange(existingAnswers);
                    await _context.SaveChangesAsync();
                }

                // Simpan jawaban baru
                foreach (var answer in answers)
                {
                    var masterQuestion = requiredQuestions.FirstOrDefault(q => q.Id == answer.MasterQuestionId);
                    if (masterQuestion == null) continue;

                    var userQuestion = new UserSecurityQuestion
                    {
                        UserId = userId,
                        Question = masterQuestion.Question, // Simpan snapshot pertanyaan
                        AnswerHash = HashAnswer(answer.Answer),
                        SortOrder = masterQuestion.SortOrder,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    await _context.UserSecurityQuestions.AddAsync(userQuestion);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Security answers setup successful for user {UserId}", userId);

                return ApiResponse<bool>.Success(true,
                    "Pertanyaan keamanan berhasil disimpan. Data ini digunakan untuk reset password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up security answers for user {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal menyimpan jawaban", ex);
            }
        }
        public async Task<ApiResponse<bool>> DeleteUserAsync(string userId, string deletedBy, bool autoPromoteDownlines = true)
        {
            if (await IsSuperAdminAsync(userId))
            {
                return ApiResponse<bool>.Error("Akses Ditolak", "SuperAdmin tidak dapat dihapus", 403);
            }

            var deleter = await _userManager.FindByIdAsync(deletedBy);
            var isDeleterSuperAdmin = await _userManager.IsInRoleAsync(deleter!, "SuperAdmin");

            if (!isDeleterSuperAdmin && !await _userManager.IsInRoleAsync(deleter!, "Admin"))
            {
                return ApiResponse<bool>.Forbidden("Hanya SuperAdmin atau Admin yang dapat menghapus user");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Auto-promote downlines JIKA diminta dan user punya referral tree
                if (autoPromoteDownlines)
                {
                    // Inject IReferralTreeService atau panggil langsung jika ada di scope yang sama
                    // Asumsi: Anda injek IReferralTreeService di constructor UserService
                    var promoteResult = await _referralTreeService.AutoPromoteDownlinesAsync(userId, deletedBy);

                    if (!promoteResult.IsSuccess && promoteResult.Type != ResponseType.Success)
                    {
                        _logger.LogWarning("Auto-promote gagal untuk user {UserId}: {Message}", userId, promoteResult.Message);
                        // Lanjutkan delete user meskipun promote gagal? 
                        // Atau rollback? Tergantung kebutuhan bisnis.
                        // Di sini kita lanjutkan tapi log warning.
                    }
                }
                else
                {
                    // Jika tidak auto-promote, hapus semua downline-nya juga (hard delete tree)
                    var downlines = await _context.ReferralTrees
                        .Where(r => r.RootUserId == userId || r.ReferredUserId == userId)
                        .ToListAsync();

                    if (downlines.Any())
                    {
                        _context.ReferralTrees.RemoveRange(downlines);
                        await _context.SaveChangesAsync();
                    }
                }

                // 2. Soft delete user
                user.SoftDelete(deletedBy);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<bool>.Error("Delete failed", "Failed to update user", 500);
                }

                await transaction.CommitAsync();

                return ApiResponse<bool>.Success(true,
                    autoPromoteDownlines
                        ? "User dihapus dan downline otomatis naik level"
                        : "User dihapus beserta tree-nya");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal menghapus user", ex);
            }
        }
        // Helper untuk hash jawaban (sama seperti hash password)
        private string HashAnswer(string answer)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(answer.ToLower().Trim()));
            return Convert.ToBase64String(bytes);
        }
        private string GenerateUsernameFromEmail(string email)
        {
            var localPart = email.Split('@')[0];
            var cleanUsername = new string(localPart.Where(char.IsLetterOrDigit).ToArray()).ToLower();
            if (cleanUsername.Length > 20) cleanUsername = cleanUsername.Substring(0, 20);

            var random = Random.Shared.Next(100, 999);
            return $"{cleanUsername}{random}";
        }

        private async Task<UserResponseDto> MapToUserResponseDto(ApplicationUser user, bool includeProfile = false)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var hasSecurityQuestions = await _context.UserSecurityQuestions.AnyAsync(q => q.UserId == user.Id);
            var dto = new UserResponseDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar, // Dari ApplicationUser
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                DeletedAt = user.DeletedAt,
                DeletedBy = user.DeletedBy,
                CreatedAt = user.CreatedAt,
                HasSecurityQuestions = hasSecurityQuestions,
                Roles = roles.ToList(),
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name
            };

            if (includeProfile && user.Profile != null)
            {
                dto.Profile = new UserProfileDto
                {
                    About = user.Profile.About,
                    Promotion = user.Profile.Promotion,
                    BirthDate = user.Profile.BirthDate,
                    BirthPlace = user.Profile.BirthPlace,
                    Address = user.Profile.Address,
                    Gender = user.Profile.Gender,
                    Facebook = user.Profile.Facebook,
                    Instagram = user.Profile.Instagram,
                    Twitter = user.Profile.Twitter,
                    Youtube = user.Profile.Youtube,
                    Linkedin = user.Profile.Linkedin,
                    Telegram = user.Profile.Telegram,
                    Website = user.Profile.Website,
                    Balance = user.Profile.Balance,
                    TaxId = user.Profile.TaxId,
                    Commission = user.Profile.Commission,
                    BankName = user.Profile.BankName,
                    BankAccountNumber = user.Profile.BankAccountNumber,
                    HeirName = user.Profile.HeirName,
                    HeirPhone = user.Profile.HeirPhone,
                    Language = user.Profile.Language,
                    TimeZone = user.Profile.TimeZone,
                    Locale = user.Profile.Locale,
                    NewsletterSubscribed = user.Profile.NewsletterSubscribed,
                    Preferences = user.Profile.Preferences,
                    Metadata = user.Profile.Metadata,
                    MonthlyBudget = user.Profile.MonthlyBudget,
                    UsedBudgetThisMonth = user.Profile.UsedBudgetThisMonth,
                    BudgetResetDate = user.Profile.BudgetResetDate,
                    CreatedAt = user.Profile.CreatedAt,
                    UpdatedAt = user.Profile.ModifiedAt,
                    LastLoginAt = user.Security?.LastLoginAt,
                    LastLoginIp = user.Security?.LastLoginIp,
                    FailedLoginAttempts = user.Security?.FailedLoginAttempts ?? 0,
                    LockedUntil = user.Security?.LockedUntil
                };
            }

            return dto;
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email!),
                new Claim("Username", user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
using ApiGMPKlik.DTOs;
using ApiGMPKlik.Services;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _configuration; // TAMBAHKAN INI

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger,
            IConfiguration configuration) // TAMBAHKAN PARAMETER INI
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration; // INJECT KE FIELD
        }

        #region Authentication

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", dto.Email);
            var result = await _userService.RegisterAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Error("Unauthorized", "Invalid token", 401));

            var result = await _userService.LogoutAsync(userId);
            return Ok(result);
        }

        #endregion

        #region User Management

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<List<UserResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? branchId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isDeleted = null,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            // Cek apakah user adalah SuperAdmin untuk akses data yang dihapus
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            if (isDeleted == true && !isSuperAdmin)
            {
                return StatusCode(403, ApiResponse<object>.Forbidden("Hanya SuperAdmin yang dapat melihat user yang dihapus"));
            }

            var filter = new UserFilterDto
            {
                BranchId = branchId,
                IsActive = isActive,
                IsDeleted = isDeleted,
                Search = search,
                Role = role,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _userService.GetAllUsersAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            // SuperAdmin bisa lihat yang dihapus, Admin biasa tidak
            var includeDeleted = isSuperAdmin;

            var result = await _userService.GetUserByIdAsync(id, includeDeleted);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-username/{username}")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(ApiResponse<PublicUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var result = await _userService.GetPublicUserByUsernameAsync(username);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("profile/{username}")]
        [Authorize] // Authenticated users bisa lihat detail lebih lengkap
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfileByUsername(string username)
        {
            var result = await _userService.GetUserByUsernameAsync(username);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var result = await _userService.GetUserByEmailAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            if (currentUserId != id && !isAdmin)
                return StatusCode(403, ApiResponse<object>.Forbidden("Anda tidak memiliki akses untuk mengubah user ini"));

            var result = await _userService.UpdateUserAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _userService.DeleteUserAsync(id, currentUserId!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "SuperAdmin")] // Hanya SuperAdmin yang bisa restore
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _userService.RestoreUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Role Management

        [HttpGet("roles")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _userService.GetAllRolesAsync();
            return Ok(result);
        }

        [HttpPost("roles")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var result = await _userService.CreateRoleAsync(roleName);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{userId}/roles")]
        [Authorize(Roles = "Admin,SuperAdmin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var result = await _userService.GetUserRolesAsync(userId);
            return Ok(result);
        }

        [HttpPost("assign-roles")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
        {
            var result = await _userService.AssignRolesAsync(dto.UserId, dto.Roles, dto.RemoveExisting);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{userId}/roles/{role}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var result = await _userService.RemoveFromRoleAsync(userId, role);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Password Management

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Error("Unauthorized", "Invalid token", 401));

            var result = await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("reset-password")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            var result = await _userService.ResetPasswordAsync(dto.UserId, dto.NewPassword);
            return StatusCode(result.StatusCode, result);
        }

        #endregion
        #region Developer Emergency Access

        /// <summary>
        /// EMERGENCY BYPASS: Ubah password tanpa password lama
        /// Khusus Developer/SuperAdmin dengan verifikasi PIN
        /// Required: Email + Username harus cocok + PIN Developer dari appsettings
        /// </summary>
        [HttpPost("dev/bypass-password")]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BypassPassword([FromBody] BypassPasswordRequestDto dto)
        {
            // Log warning khusus untuk endpoint ini
            _logger.LogWarning("Developer Bypass Password endpoint accessed by User: {User} for Target: {Email}",
                User.Identity?.Name, dto.Email);

            var result = await _userService.BypassPasswordAsync(dto);

            // Return dengan status code sesuai hasil
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Verifikasi saja tanpa mengubah password (untuk check kredensial)
        /// </summary>
        [HttpPost("dev/verify-credentials")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyCredentials([FromBody] VerifyCredentialsDto dto)
        {
            var devPin = _configuration["StaticAuth:Pin"];
            if (dto.Pin != devPin)
            {
                return StatusCode(403, ApiResponse<object>.Forbidden("PIN tidak valid"));
            }

            var user = await _userService.GetUserByEmailAsync(dto.Email);
            if (!user.IsSuccess || user.Data == null)
            {
                return StatusCode(404, ApiResponse<object>.NotFound("User"));
            }

            var isMatch = string.Equals(user.Data.Username, dto.Username, StringComparison.Ordinal);

            return Ok(ApiResponse<object>.Success(new
            {
                EmailExists = true,
                UsernameMatch = isMatch,
                ExpectedUsername = user.Data.Username,
                ProvidedUsername = dto.Username
            }, isMatch ? "Kredensial valid" : "Username tidak cocok"));
        }

        #endregion
        #region Forgot Password (Security Questions)

        [HttpPost("forgot-password/initiate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ForgotPasswordInitiateResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordInitiate([FromBody] ForgotPasswordRequestDto dto)
        {
            var result = await _userService.GetSecurityQuestionsAsync(dto.UsernameOrEmail);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("forgot-password/verify")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] VerifySecurityAnswersDto dto)
        {
            var result = await _userService.VerifySecurityAnswersAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("forgot-password/reset")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordReset([FromBody] ResetPasswordWithVerificationDto dto)
        {
            var result = await _userService.ResetPasswordAfterVerificationAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("security-questions/setup")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetupSecurityQuestions([FromBody] List<SecurityQuestionSetupDto> dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.SetupSecurityQuestionsAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        #endregion
        [HttpGet("security-questions/mandatory")]
        [AllowAnonymous] // Bisa diakses sebelum login (untuk registrasi)
        public async Task<IActionResult> GetMandatoryQuestions()
        {
            var result = await _userService.GetMandatorySecurityQuestionsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("security-questions/answers")]
        [Authorize]
        public async Task<IActionResult> SetupAnswers([FromBody] List<UserSecurityAnswerDto> dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.SetupSecurityAnswersAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }
        #region Branch Management

        /// <summary>
        /// Ganti Branch untuk User tertentu (Admin/SuperAdmin only)
        /// dengan logging dan validasi role
        /// </summary>
        [HttpPut("{id}/change-branch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeBranch(
            string id,
            [FromBody] ChangeBranchRequestDto dto)
        {
            try
            {
                // Ambil data user target
                var targetUserResult = await _userService.GetUserByIdAsync(id);
                if (!targetUserResult.IsSuccess)
                    return NotFound(targetUserResult);

                var targetUser = targetUserResult.Data!;
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Cek apakah target adalah SuperAdmin/Admin
                var isTargetAdmin = targetUser.Roles.Contains("Admin") || targetUser.Roles.Contains("SuperAdmin");
                var isCurrentSuperAdmin = User.IsInRole("SuperAdmin");

                // Hanya SuperAdmin yang boleh ganti branch Admin/SuperAdmin lain
                if (isTargetAdmin && !isCurrentSuperAdmin)
                {
                    return StatusCode(403, ApiResponse<object>.Forbidden(
                        "Hanya SuperAdmin yang dapat mengubah branch untuk user dengan role Admin/SuperAdmin"));
                }

                // Simpan info branch lama untuk response
                var oldBranchId = targetUser.BranchId;
                var oldBranchName = targetUser.BranchName;

                // Update branch via existing service
                var updateDto = new UpdateUserDto
                {
                    BranchId = dto.NewBranchId
                };

                var result = await _userService.UpdateUserAsync(id, updateDto);

                if (!result.IsSuccess)
                    return StatusCode(result.StatusCode, result);

                // Log perubahan
                _logger.LogInformation(
                    "Branch changed for User {UserId} from {OldBranchId} to {NewBranchId} by {ChangedBy}",
                    id, oldBranchId, dto.NewBranchId, currentUserId);

                // Tambahkan info branch lama ke response
                return Ok(ApiResponse<UserResponseDto>.Success(
                    result.Data!,
                    $"Branch berhasil diubah{(oldBranchId.HasValue ? $" dari branch {oldBranchId}" : "")} ke branch {dto.NewBranchId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing branch for user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.Danger("Gagal mengganti branch", ex));
            }
        }

        /// <summary>
        /// Batch update branch untuk multiple users
        /// </summary>
        [HttpPut("batch/change-branch")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BatchChangeBranchResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BatchChangeBranch([FromBody] BatchChangeBranchDto dto)
        {
            try
            {
                if (dto.UserIds == null || !dto.UserIds.Any())
                {
                    return BadRequest(ApiResponse<object>.Error(
                        "Daftar User ID tidak boleh kosong", "ValidationError", 400));
                }

                var results = new List<UserBranchChangeResult>();
                int successCount = 0;
                int failedCount = 0;
                var isCurrentSuperAdmin = User.IsInRole("SuperAdmin");

                foreach (var userId in dto.UserIds.Distinct())
                {
                    try
                    {
                        // Cek user exists dan role-nya
                        var userResult = await _userService.GetUserByIdAsync(userId);
                        if (!userResult.IsSuccess)
                        {
                            results.Add(new UserBranchChangeResult
                            {
                                UserId = userId,
                                Success = false,
                                Message = "User tidak ditemukan"
                            });
                            failedCount++;
                            continue;
                        }

                        // Cek restriction role
                        var user = userResult.Data!;
                        var isTargetAdmin = user.Roles.Contains("Admin") || user.Roles.Contains("SuperAdmin");

                        if (isTargetAdmin && !isCurrentSuperAdmin)
                        {
                            results.Add(new UserBranchChangeResult
                            {
                                UserId = userId,
                                Success = false,
                                Message = "Tidak memiliki izin untuk mengubah Admin/SuperAdmin"
                            });
                            failedCount++;
                            continue;
                        }

                        // Update branch
                        var oldBranchId = user.BranchId;
                        var updateDto = new UpdateUserDto { BranchId = dto.NewBranchId };
                        var updateResult = await _userService.UpdateUserAsync(userId, updateDto);

                        if (updateResult.IsSuccess)
                        {
                            results.Add(new UserBranchChangeResult
                            {
                                UserId = userId,
                                Success = true,
                                OldBranchId = oldBranchId,
                                NewBranchId = dto.NewBranchId,
                                Message = "Berhasil"
                            });
                            successCount++;
                        }
                        else
                        {
                            results.Add(new UserBranchChangeResult
                            {
                                UserId = userId,
                                Success = false,
                                Message = updateResult.Message
                            });
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(new UserBranchChangeResult
                        {
                            UserId = userId,
                            Success = false,
                            Message = ex.Message
                        });
                        failedCount++;
                    }
                }

                var resultDto = new BatchChangeBranchResultDto
                {
                    TotalProcessed = dto.UserIds.Count(),
                    SuccessCount = successCount,
                    FailedCount = failedCount,
                    Results = results
                };

                return Ok(ApiResponse<BatchChangeBranchResultDto>.Success(
                    resultDto,
                    $"Batch update selesai. Berhasil: {successCount}, Gagal: {failedCount}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch branch change");
                return StatusCode(500, ApiResponse<object>.Danger("Gagal melakukan batch update branch", ex));
            }
        }

        #endregion
    }

    // Additional DTOs for Controller
    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
    public class VerifyCredentialsDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }
    public class ChangeBranchRequestDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Branch ID harus lebih besar dari 0")]
        public int NewBranchId { get; set; }

        public string? Reason { get; set; }
    }

    public class BatchChangeBranchDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Minimal harus ada 1 user")]
        public List<string> UserIds { get; set; } = new();

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Branch ID harus lebih besar dari 0")]
        public int NewBranchId { get; set; }
    }

    public class UserBranchChangeResult
    {
        public string UserId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? OldBranchId { get; set; }
        public int? NewBranchId { get; set; }
    }

    public class BatchChangeBranchResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<UserBranchChangeResult> Results { get; set; } = new();
    }
}
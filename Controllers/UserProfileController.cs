using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IUserProfileService userProfileService, ILogger<UserProfileController> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,BranchAdmin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<UserProfileDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? gender = null,
            [FromQuery] string? city = null,
            [FromQuery] int? branchId = null,
            [FromQuery] bool? newsletterSubscribed = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var filter = new UserProfileFilterDto
            {
                Search = search,
                Gender = gender,
                City = city,
                BranchId = branchId,
                NewsletterSubscribed = newsletterSubscribed,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _userProfileService.GetPaginatedAsync(filter, page, pageSize, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _userProfileService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByUserId(string userId, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Jika bukan admin dan bukan pemilik data, cek akses
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && currentUserId != userId)
            {
                return StatusCode(403, ApiResponse<object>.Forbidden("Anda tidak memiliki akses ke profile ini"));
            }

            var result = await _userProfileService.GetByUserIdAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userProfileService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userProfileService.UpdateAsync(id, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateByUserId(string userId, [FromBody] UpdateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && currentUserId != userId)
            {
                return StatusCode(403, ApiResponse<object>.Forbidden("Anda tidak memiliki akses untuk mengupdate profile ini"));
            }

            var result = await _userProfileService.UpdateByUserIdAsync(userId, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _userProfileService.DeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteByUserId(string userId, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && currentUserId != userId)
            {
                return StatusCode(403, ApiResponse<object>.Forbidden("Anda tidak memiliki akses untuk menghapus profile ini"));
            }

            var result = await _userProfileService.DeleteByUserIdAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("exists/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Exists(int id, CancellationToken cancellationToken = default)
        {
            var result = await _userProfileService.ExistsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId}/exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExistsByUserId(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userProfileService.ExistsByUserIdAsync(userId, cancellationToken);
            return Ok(result);
        }
    }
}
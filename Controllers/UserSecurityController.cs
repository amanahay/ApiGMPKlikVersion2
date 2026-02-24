using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserSecurityController : ControllerBase
    {
        private readonly IUserSecurityService _userSecurityService;
        private readonly ILogger<UserSecurityController> _logger;

        public UserSecurityController(IUserSecurityService userSecurityService, ILogger<UserSecurityController> logger)
        {
            _userSecurityService = userSecurityService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<UserSecurityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isLocked = null,
            [FromQuery] int? minFailedAttempts = null,
            [FromQuery] int? branchId = null,
            [FromQuery] string? sortBy = "LastLoginAt",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var filter = new UserSecurityFilterDto
            {
                Search = search,
                IsLocked = isLocked,
                MinFailedAttempts = minFailedAttempts,
                BranchId = branchId,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _userSecurityService.GetPaginatedAsync(filter, page, pageSize, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserSecurityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserSecurityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUserId(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.GetByUserIdAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("user/{userId}/create")]
        [ProducesResponseType(typeof(ApiResponse<UserSecurityDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.CreateAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserSecurityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserSecurityDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.UpdateAsync(id, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("user/{userId}/lock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockUser(string userId, [FromBody] LockUserDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.LockUserAsync(userId, dto.LockedUntil, dto.Reason, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("user/{userId}/unlock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockUser(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.UnlockUserAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("user/{userId}/reset-attempts")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetFailedAttempts(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.ResetFailedAttemptsAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/is-locked")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> IsUserLocked(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.IsUserLockedAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("exists/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Exists(int id, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.ExistsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId}/exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExistsByUserId(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userSecurityService.ExistsByUserIdAsync(userId, cancellationToken);
            return Ok(result);
        }
    }
}
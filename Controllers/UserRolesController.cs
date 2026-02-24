using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/user-roles")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(IUserRoleService userRoleService, ILogger<UserRolesController> logger)
        {
            _userRoleService = userRoleService;
            _logger = logger;
        }

        [HttpPost("assign")]
        [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleToUserDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.AssignRoleAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("user/{userId}/role/{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole(string userId, string roleName, CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.RemoveRoleAsync(userId, roleName, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<List<UserRoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.GetUserRolesAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("role/{roleName}/users")]
        [Authorize(Roles = "Admin,SuperAdmin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<List<UserRoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersInRole(string roleName, CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.GetUsersInRoleAsync(roleName, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> IsInRole(
            [FromQuery] string userId,
            [FromQuery] string roleName,
            CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.IsInRoleAsync(userId, roleName, cancellationToken);
            return Ok(result);
        }

        [HttpPost("user/{userId}/bulk-assign")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BulkAssign(string userId, [FromBody] List<string> roleNames, CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.BulkAssignAsync(userId, roleNames, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("user/{userId}/bulk-remove")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BulkRemove(string userId, [FromBody] List<string> roleNames, CancellationToken cancellationToken = default)
        {
            var result = await _userRoleService.BulkRemoveAsync(userId, roleNames, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
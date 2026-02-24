using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/role-permissions")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;
        private readonly ILogger<RolePermissionsController> _logger;

        public RolePermissionsController(IRolePermissionService rolePermissionService, ILogger<RolePermissionsController> logger)
        {
            _rolePermissionService = rolePermissionService;
            _logger = logger;
        }

        [HttpPost("assign")]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionToRoleDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _rolePermissionService.AssignPermissionAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("role/{roleId}/permission/{permissionId:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemovePermission(string roleId, int permissionId, CancellationToken cancellationToken = default)
        {
            var result = await _rolePermissionService.RemovePermissionAsync(roleId, permissionId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("bulk-assign")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BulkAssign([FromBody] BulkAssignPermissionDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _rolePermissionService.BulkAssignAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-role/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<List<RolePermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRole(
            string roleId,
            [FromQuery] string? module = null,
            CancellationToken cancellationToken = default)
        {
            var filter = new RolePermissionFilterDto { RoleId = roleId, Module = module };
            var result = await _rolePermissionService.GetByRoleAsync(roleId, filter, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-permission/{permissionId:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<RolePermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPermission(int permissionId, CancellationToken cancellationToken = default)
        {
            var result = await _rolePermissionService.GetByPermissionAsync(permissionId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> HasPermission(
            [FromQuery] string roleId,
            [FromQuery] string permissionCode,
            CancellationToken cancellationToken = default)
        {
            var result = await _rolePermissionService.HasPermissionAsync(roleId, permissionCode, cancellationToken);
            return Ok(result);
        }

        [HttpPost("sync/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SyncRolePermissions(
            string roleId,
            [FromBody] List<int> permissionIds,
            CancellationToken cancellationToken = default)
        {
            var result = await _rolePermissionService.SyncRolePermissionsAsync(roleId, permissionIds, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
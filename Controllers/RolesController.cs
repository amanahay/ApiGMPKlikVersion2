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
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "SortOrder",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var filter = new RoleFilterDto
            {
                Search = search,
                IsActive = isActive,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _roleService.GetPaginatedAsync(filter, page, pageSize, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("name/{name}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.GetByNameAsync(name, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new role: {RoleName}", dto.Name);
            var result = await _roleService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.UpdateAsync(id, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.DeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(string id, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.RestoreAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("exists/{name}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Exists(string name, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.ExistsAsync(name, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{roleId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolePermissions(string roleId, CancellationToken cancellationToken = default)
        {
            var result = await _roleService.GetRolePermissionsAsync(roleId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
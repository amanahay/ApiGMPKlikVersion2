using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/role-claims")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RoleClaimsController : ControllerBase
    {
        private readonly IRoleClaimService _roleClaimService;
        private readonly ILogger<RoleClaimsController> _logger;

        public RoleClaimsController(IRoleClaimService roleClaimService, ILogger<RoleClaimsController> logger)
        {
            _roleClaimService = roleClaimService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleClaimDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateRoleClaimDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _roleClaimService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _roleClaimService.DeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("role/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<List<RoleClaimDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRole(string roleId, CancellationToken cancellationToken = default)
        {
            var result = await _roleClaimService.GetByRoleAsync(roleId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("role/{roleId}/claim")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveClaim(
            string roleId,
            [FromQuery] string claimType,
            [FromQuery] string claimValue,
            CancellationToken cancellationToken = default)
        {
            var result = await _roleClaimService.RemoveClaimAsync(roleId, claimType, claimValue, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
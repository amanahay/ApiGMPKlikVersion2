using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/user-claims")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserClaimsController : ControllerBase
    {
        private readonly IUserClaimService _userClaimService;
        private readonly ILogger<UserClaimsController> _logger;

        public UserClaimsController(IUserClaimService userClaimService, ILogger<UserClaimsController> logger)
        {
            _userClaimService = userClaimService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserClaimDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateUserClaimDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userClaimService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _userClaimService.DeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<List<UserClaimDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userClaimService.GetByUserAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserClaimDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userClaimService.UpdateAsync(id, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("user/{userId}/claim")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveClaim(
            string userId,
            [FromQuery] string claimType,
            [FromQuery] string claimValue,
            CancellationToken cancellationToken = default)
        {
            var result = await _userClaimService.RemoveClaimAsync(userId, claimType, claimValue, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/user-logins")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserLoginsController : ControllerBase
    {
        private readonly IUserLoginService _userLoginService;
        private readonly ILogger<UserLoginsController> _logger;

        public UserLoginsController(IUserLoginService userLoginService, ILogger<UserLoginsController> logger)
        {
            _userLoginService = userLoginService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserLoginDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateUserLoginDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _userLoginService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            string userId,
            [FromQuery] string loginProvider,
            [FromQuery] string providerKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _userLoginService.DeleteAsync(userId, loginProvider, providerKey, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<List<UserLoginDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _userLoginService.GetByUserAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("find")]
        [ProducesResponseType(typeof(ApiResponse<UserLoginDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Find(
            [FromQuery] string loginProvider,
            [FromQuery] string providerKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _userLoginService.FindAsync(loginProvider, providerKey, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
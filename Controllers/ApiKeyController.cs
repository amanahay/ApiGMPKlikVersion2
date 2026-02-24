using ApiGMPKlik.Services;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")] // JWT only untuk management
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeyController> _logger;

        public ApiKeyController(IApiKeyService apiKeyService, ILogger<ApiKeyController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "APIKEY_MANAGE")]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var (apiKey, client) = await _apiKeyService.GenerateApiKeyAsync(
                    request.ClientName,
                    userId,
                    request.ExpiresAt,
                    request.Permissions);

                _logger.LogInformation("API Key created by user {UserId}", userId);

                return Ok(ApiResponse<object>.Success(new
                {
                    ApiKey = apiKey, // Hanya ditampilkan sekali!
                    ClientId = client.Id,
                    Prefix = client.ApiKeyPrefix,
                    ExpiresAt = client.ExpiresAt,
                    Message = "Simpan API Key ini sekarang. Tidak akan ditampilkan lagi!"
                }, "API Key berhasil dibuat"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API Key");
                return StatusCode(500, ApiResponse<object>.Danger("Gagal membuat API Key", ex.Message));
            }
        }

        [HttpPost("{id}/rotate")]
        [Authorize(Policy = "APIKEY_MANAGE")]
        public async Task<IActionResult> Rotate(int id, [FromQuery] bool revokeImmediately = true)
        {
            var rotatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            try
            {
                // Generate new key
                var (newKey, newClient) = await _apiKeyService.GenerateApiKeyAsync($"Rotated-{id}");

                // Revoke old
                await _apiKeyService.RevokeApiKeyAsync(id, rotatedBy);

                return Ok(ApiResponse<object>.Success(new
                {
                    NewApiKey = newKey,
                    NewClientId = newClient.Id,
                    Message = "API Key berhasil dirotasi. Key lama telah direvoke."
                }, "Rotasi berhasil"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error("Rotasi gagal", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "APIKEY_MANAGE")]
        public async Task<IActionResult> Revoke(int id)
        {
            var revokedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            try
            {
                await _apiKeyService.RevokeApiKeyAsync(id, revokedBy);
                return Ok(ApiResponse<object>.Success(null!, "API Key berhasil direvoke"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error("Revoke gagal", ex.Message));
            }
        }
    }

    public class CreateApiKeyRequest
    {
        public string ClientName { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public List<string>? Permissions { get; set; }
    }
}

using ApiGMPKlik.Services;
using ApiGMPKlik.Shared;
using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ApiGMPKlik.Infrastructure
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            IApiKeyService apiKeyService)
            : base(options, loggerFactory, encoder)
        {
            _apiKeyService = apiKeyService;
            _logger = loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Cek apakah header API Key ada
            if (!Request.Headers.TryGetValue(ApiKeyDefaults.ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                // Tidak ada API Key header, return NoResult agar bisa fallback ke auth lain
                return AuthenticateResult.NoResult();
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.Fail("API Key is missing");
            }

            try
            {
                // Validasi API Key
                var isValid = await _apiKeyService.ValidateApiKeyAsync(providedApiKey);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid API Key provided from IP {Ip}",
                        Context.Connection.RemoteIpAddress);
                    return AuthenticateResult.Fail("Invalid API Key");
                }

                // Ambil client data
                var client = await _apiKeyService.GetClientByApiKeyAsync(providedApiKey);
                if (client == null)
                {
                    return AuthenticateResult.Fail("API Client not found");
                }

                // Build claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                    new Claim(ClaimTypes.Name, client.ClientName),
                    new Claim("AuthType", "ApiKey"),
                    new Claim("ClientId", client.Id.ToString())
                };

                // Add User claims if linked
                if (client.UserId != null)
                {
                    claims.Add(new Claim("UserId", client.UserId));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, client.UserId));
                }

                // Add permissions from API Key
                if (!string.IsNullOrEmpty(client.AllowedPermissions))
                {
                    var permissions = JsonConvert.DeserializeObject<List<string>>(client.AllowedPermissions);
                    if (permissions != null)
                    {
                        foreach (var perm in permissions)
                        {
                            claims.Add(new Claim("Permission", perm));
                        }
                    }
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                _logger.LogDebug("API Key authentication successful for client: {ClientName}", client.ClientName);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API Key");
                return AuthenticateResult.Fail("Error validating API Key");
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/json";

            var response = ApiResponse<object>.Unauthorized("API Key is missing or invalid");
            var json = JsonConvert.SerializeObject(response);
            await Response.WriteAsync(json);
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = "application/json";

            var response = ApiResponse<object>.Forbidden("You do not have permission to access this resource");
            var json = JsonConvert.SerializeObject(response);
            await Response.WriteAsync(json);
        }
    }
}

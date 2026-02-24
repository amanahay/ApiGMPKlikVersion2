using ApiGMPKlik.Models.Entities;
using Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace ApiGMPKlik.Services
{
    public static class ApiKeyDefaults
    {
        public const string AuthenticationScheme = "ApiKey";
        public const string ApiKeyHeaderName = "X-API-Key";
    }

    public interface IApiKeyService
    {
        Task<(string apiKey, ApiClient client)> GenerateApiKeyAsync(string clientName, string? userId = null,
            DateTime? expiresAt = null, List<string>? permissions = null);
        Task<bool> ValidateApiKeyAsync(string apiKey);
        Task<ApiClient?> GetClientByApiKeyAsync(string apiKey);
        Task RevokeApiKeyAsync(int clientId, string revokedBy);
        Task RotateApiKeyAsync(int clientId, string rotatedBy, bool revokeOldImmediately = true);
        string HashApiKey(string apiKey);
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(ApplicationDbContext context, ILogger<ApiKeyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(string apiKey, ApiClient client)> GenerateApiKeyAsync(
            string clientName,
            string? userId = null,
            DateTime? expiresAt = null,
            List<string>? permissions = null)
        {
            // Generate secure random key: prefix.randomString
            var prefix = $"gmp_{GenerateRandomString(4)}";
            var keyPart = GenerateRandomString(32);
            var fullApiKey = $"{prefix}.{keyPart}";

            // Hash only the key part (not prefix) for storage
            var keyHash = HashApiKey(keyPart);

            var client = new ApiClient
            {
                ClientName = clientName,
                ApiKeyPrefix = prefix,
                ApiKeyHash = keyHash,
                ExpiresAt = expiresAt,
                UserId = userId,
                AllowedPermissions = permissions != null ? JsonConvert.SerializeObject(permissions) : null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ApiClients.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("API Key generated for client {ClientName} with prefix {Prefix}",
                clientName, prefix);

            // Return full key (only time it's visible)
            return (fullApiKey, client);
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.Contains('.'))
                return false;

            var parts = apiKey.Split('.', 2);
            if (parts.Length != 2) return false;

            var prefix = parts[0];
            var keyPart = parts[1];

            var client = await _context.ApiClients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ApiKeyPrefix == prefix && !c.IsDeleted);

            if (client == null) return false;
            if (!client.IsActiveAndValid) return false;

            // Verify hash
            var providedHash = HashApiKey(keyPart);
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(providedHash),
                Encoding.UTF8.GetBytes(client.ApiKeyHash)))
            {
                _logger.LogWarning("Invalid API Key attempt for prefix {Prefix}", prefix);
                return false;
            }

            // Update last used
            client.UpdateLastUsed();
            _context.Update(client);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ApiClient?> GetClientByApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.Contains('.'))
                return null;

            var parts = apiKey.Split('.', 2);
            var prefix = parts[0];

            return await _context.ApiClients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ApiKeyPrefix == prefix && !c.IsDeleted);
        }

        public async Task RevokeApiKeyAsync(int clientId, string revokedBy)
        {
            var client = await _context.ApiClients.FindAsync(clientId);
            if (client == null) throw new InvalidOperationException("API Client not found");

            client.Revoke(revokedBy);
            await _context.SaveChangesAsync();

            _logger.LogInformation("API Key {ClientId} revoked by {RevokedBy}", clientId, revokedBy);
        }

        public async Task RotateApiKeyAsync(int clientId, string rotatedBy, bool revokeOldImmediately = true)
        {
            var oldClient = await _context.ApiClients.FindAsync(clientId);
            if (oldClient == null) throw new InvalidOperationException("API Client not found");

            // Create new key
            var (newKey, newClient) = await GenerateApiKeyAsync(
                $"{oldClient.ClientName} (Rotated)",
                oldClient.UserId,
                oldClient.ExpiresAt,
                oldClient.AllowedPermissions != null ?
                    JsonConvert.DeserializeObject<List<string>>(oldClient.AllowedPermissions) : null);

            if (revokeOldImmediately)
            {
                oldClient.Revoke(rotatedBy);
            }
            else
            {
                // Set old key to expire in 7 days (grace period)
                oldClient.ExpiresAt = DateTime.UtcNow.AddDays(7);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("API Key rotated for {ClientId} by {RotatedBy}", clientId, rotatedBy);

            // Return new key through separate method or event
        }

        public string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

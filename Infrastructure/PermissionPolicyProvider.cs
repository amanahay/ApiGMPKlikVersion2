using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApiGMPKlik.Infrastructure
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "AllPermissions";

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options, IMemoryCache cache)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _cache = cache;
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Check if it's a permission-based policy (we assume all policies not in fallback are permissions)
            if (policyName.StartsWith("PERM_", StringComparison.OrdinalIgnoreCase) ||
                IsLikelyPermissionCode(policyName))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(policyName));
                return policy.Build();
            }

            // Fallback to standard policies
            return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        private bool IsLikelyPermissionCode(string policyName)
        {
            // Permission codes are typically UPPER_CASE with underscores
            return policyName.All(c => char.IsUpper(c) || c == '_' || char.IsDigit(c))
                   && policyName.Contains('_');
        }
    }
}

using ApiGMPKlik.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ApiGMPKlik.Infrastructure
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Add standard policies
            services.AddAuthorization(options =>
            {
                // Admin policy
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireRole("Admin"));

                // Combined JWT + Permission policies can be added here if needed
            });

            return services;
        }

        /// <summary>
        /// Extension method untuk menambahkan API Key authentication scheme.
        /// CATATAN: Method ini HARUS dipanggil DI DALAM konfigurasi AddAuthentication() utama di Program.cs,
        /// bukan dipisah dengan AddAuthentication() baru yang akan menimpa konfigurasi sebelumnya.
        /// </summary>
        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder)
        {
            builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyDefaults.AuthenticationScheme,
                options => { });

            return builder;
        }
    }
}
using Microsoft.AspNetCore.Authorization;

namespace ApiGMPKlik.Infrastructure
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Check if user has the required permission claim
            var permissionClaims = context.User.FindAll("Permission").Select(c => c.Value).ToList();

            if (permissionClaims.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {User} does not have permission {Permission}",
                    context.User.Identity?.Name, requirement.Permission);
            }

            return Task.CompletedTask;
        }
    }
}

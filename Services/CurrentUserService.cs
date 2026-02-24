using ApiGMPKlik.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGMPKlik.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value
            ?? User?.FindFirst("Username")?.Value;

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public int? BranchId
        {
            get
            {
                var branchIdClaim = User?.FindFirst("BranchId")?.Value
                    ?? User?.FindFirst("branch_id")?.Value;

                if (int.TryParse(branchIdClaim, out int branchId))
                    return branchId;

                return null;
            }
        }

        public IReadOnlyList<string> Roles
        {
            get
            {
                var roles = User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                return roles?.AsReadOnly() ?? new List<string>().AsReadOnly();
            }
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

        public bool IsInAnyRole(params string[] roles) => roles.Any(r => IsInRole(r));

        public bool IsSuperAdmin() => IsInRole("SuperAdmin");

        public bool IsAdmin() => IsInRole("Admin") || IsSuperAdmin();

        public bool IsBranchAdmin() => IsInRole("BranchAdmin");

        public bool CanAccessAllData() => IsSuperAdmin() || IsAdmin();

        public bool CanAccessBranchData(int branchId) =>
            IsSuperAdmin() ||
            (IsAdmin() && BranchId == branchId) ||
            (IsBranchAdmin() && BranchId == branchId);

        public bool CanAccessOwnData(string ownerId) =>
            IsSuperAdmin() ||
            UserId == ownerId ||
            (IsAdmin() && !string.IsNullOrEmpty(ownerId));
    }
}
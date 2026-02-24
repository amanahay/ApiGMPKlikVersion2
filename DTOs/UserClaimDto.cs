using System.Security.Claims;

namespace ApiGMPKlik.DTOs
{
    public class UserClaimDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
    }

    public class CreateUserClaimDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
    }

    public class UpdateUserClaimDto
    {
        public string ClaimValue { get; set; } = string.Empty;
    }
}

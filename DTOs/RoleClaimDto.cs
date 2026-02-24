// RoleClaimDtos.cs
namespace ApiGMPKlik.DTOs
{
    public class RoleClaimDto
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
    }

    public class CreateRoleClaimDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
    }
}
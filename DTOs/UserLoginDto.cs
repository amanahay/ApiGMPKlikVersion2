// UserLoginDtos.cs
namespace ApiGMPKlik.DTOs
{
    public class UserLoginDto
    {
        public string UserId { get; set; } = string.Empty;
        public string LoginProvider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string? ProviderDisplayName { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateUserLoginDto
    {
        public string UserId { get; set; } = string.Empty;
        public string LoginProvider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string? ProviderDisplayName { get; set; }
    }
}
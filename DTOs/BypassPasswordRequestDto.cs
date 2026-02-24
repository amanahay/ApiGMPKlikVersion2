namespace ApiGMPKlik.DTOs
{
    // Emergency Bypass Password untuk Developer/SuperAdmin
    public class BypassPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty; // PIN dari StaticAuth:Pin
        public string NewPassword { get; set; } = string.Empty;
    }
}

namespace WinFormApiGMPKlik.Models
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "https://localhost:7001";
        public string ApiVersion { get; set; } = "v1";
        public int TimeoutSeconds { get; set; } = 30;
        public bool UseHttps { get; set; } = true;
    }

    public class AuthSettings
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class AppSettings
    {
        public ApiSettings Api { get; set; } = new();
        public AuthSettings Auth { get; set; } = new();
        public UISettings UI { get; set; } = new();
    }

    public class UISettings
    {
        public string Theme { get; set; } = "Light";
        public string AccentColor { get; set; } = "#007ACC";
        public bool ShowAnimations { get; set; } = true;
        public int GridPageSize { get; set; } = 20;
    }
}

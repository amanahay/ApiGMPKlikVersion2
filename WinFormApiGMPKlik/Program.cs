using System.Text.Json;
using WinFormApiGMPKlik.Forms;
using WinFormApiGMPKlik.Models;
using WinFormApiGMPKlik.Services;

namespace WinFormApiGMPKlik
{
    /// <summary>
    /// Entry point aplikasi WinForm ApiGMPKlik
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            try
            {
                // Load application settings
                var settings = LoadSettings();

                // Initialize API Client
                var apiClient = new ApiClientService(settings.Api);

                // Check if user has valid auth token
                if (!string.IsNullOrEmpty(settings.Auth.Token) && settings.Auth.ExpiresAt > DateTime.Now)
                {
                    // Set auth token and show dashboard
                    apiClient.SetAuthSettings(settings.Auth);
                    var dashboard = new DashboardForm(apiClient, settings);
                    Application.Run(dashboard);
                }
                else
                {
                    // Show login form
                    var loginForm = new LoginForm(apiClient, settings);
                    Application.Run(loginForm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Terjadi kesalahan saat memulai aplikasi:\n\n{ex.Message}\n\nAplikasi akan ditutup.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load application settings from configuration file
        /// </summary>
        private static AppSettings LoadSettings()
        {
            var settings = new AppSettings();
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loadedSettings != null)
                    {
                        settings = loadedSettings;
                    }
                }
            }
            catch
            {
                // Use default settings if loading fails
            }

            // Load saved auth from isolated storage or config
            var authPath = Path.Combine(AppContext.BaseDirectory, "auth.config");
            try
            {
                if (File.Exists(authPath))
                {
                    var json = File.ReadAllText(authPath);
                    var auth = JsonSerializer.Deserialize<AuthSettings>(json);
                    if (auth != null)
                    {
                        settings.Auth = auth;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return settings;
        }

        /// <summary>
        /// Save authentication settings
        /// </summary>
        public static void SaveAuthSettings(AuthSettings auth)
        {
            try
            {
                var authPath = Path.Combine(AppContext.BaseDirectory, "auth.config");
                var json = JsonSerializer.Serialize(auth, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(authPath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        /// <summary>
        /// Clear saved authentication
        /// </summary>
        public static void ClearAuthSettings()
        {
            try
            {
                var authPath = Path.Combine(AppContext.BaseDirectory, "auth.config");
                if (File.Exists(authPath))
                {
                    File.Delete(authPath);
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}

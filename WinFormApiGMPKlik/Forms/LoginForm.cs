using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Models;
using WinFormApiGMPKlik.Services;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    /// <summary>
    /// Form Login untuk aplikasi
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly ApiClientService _apiClient;
        private readonly AppSettings _settings;
        private TextBox? _txtUsername;
        private TextBox? _txtPassword;
        private Button? _btnLogin;
        private Label? _lblError;
        private CheckBox? _chkRemember;

        public LoginForm(ApiClientService apiClient, AppSettings settings)
        {
            _apiClient = apiClient;
            _settings = settings;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form Settings
            Text = "Login - Api GMPKlik";
            Size = new Size(450, 550);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            // Logo/Icon
            var lblIcon = new Label
            {
                Text = "🏢",
                Font = new Font("Segoe UI", 72F, FontStyle.Regular, GraphicsUnit.Point),
                AutoSize = true,
                Location = new Point(165, 30)
            };

            // Title
            var lblTitle = new Label
            {
                Text = "Api GMPKlik",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(115, 130)
            };

            var lblSubtitle = new Label
            {
                Text = "Silakan login untuk melanjutkan",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(125, 175)
            };

            // Username
            var lblUsername = new Label
            {
                Text = "Username / Email",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(50, 220),
                AutoSize = true
            };

            _txtUsername = new TextBox
            {
                Location = new Point(50, 245),
                Size = new Size(330, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password
            var lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(50, 290),
                AutoSize = true
            };

            _txtPassword = new TextBox
            {
                Location = new Point(50, 315),
                Size = new Size(330, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // Remember Me
            _chkRemember = new CheckBox
            {
                Text = "Ingat saya",
                Location = new Point(50, 360),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // Error Label
            _lblError = new Label
            {
                Text = "",
                ForeColor = Color.Red,
                Location = new Point(50, 390),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Login Button
            _btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(50, 425),
                Size = new Size(330, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += OnLoginClick;

            // Footer
            var lblFooter = new Label
            {
                Text = "© 2026 GMPKlik. All rights reserved.",
                Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(135, 490)
            };

            // Add Controls
            Controls.AddRange(new Control[]
            {
                lblIcon, lblTitle, lblSubtitle,
                lblUsername, _txtUsername,
                lblPassword, _txtPassword,
                _chkRemember, _lblError,
                _btnLogin, lblFooter
            });

            // Accept Button
            AcceptButton = _btnLogin;

            ResumeLayout(false);
            PerformLayout();
        }

        private async void OnLoginClick(object? sender, EventArgs e)
        {
            var username = _txtUsername?.Text.Trim();
            var password = _txtPassword?.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username dan password harus diisi");
                return;
            }

            _btnLogin!.Enabled = false;
            _btnLogin.Text = "⏳ Logging in...";
            HideError();

            try
            {
                var userService = new UserService(_apiClient);
                var result = await userService.LoginAsync(new LoginDto
                {
                    UsernameOrEmail = username,
                    Password = password,
                    RememberMe = _chkRemember?.Checked ?? false
                });

                if (result.IsSuccess && result.Data != null)
                {
                    // Save auth settings
                    _settings.Auth.Token = result.Data.Token;
                    _settings.Auth.RefreshToken = result.Data.RefreshToken;
                    _settings.Auth.ExpiresAt = result.Data.ExpiresAt;
                    _settings.Auth.Username = result.Data.User.Username;
                    _settings.Auth.Roles = result.Data.User.Roles;

                    // Set token for API client
                    _apiClient.SetAuthToken(result.Data.Token);

                    // Open Dashboard
                    var dashboard = new DashboardForm(_apiClient, _settings);
                    dashboard.Show();
                    Hide();
                }
                else
                {
                    ShowError(result.Message ?? "Login gagal. Periksa kredensial Anda.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Terjadi kesalahan: {ex.Message}");
            }
            finally
            {
                _btnLogin.Enabled = true;
                _btnLogin.Text = "Login";
            }
        }

        private void ShowError(string message)
        {
            if (_lblError != null)
            {
                _lblError.Text = message;
                _lblError.Visible = true;
            }
        }

        private void HideError()
        {
            if (_lblError != null)
            {
                _lblError.Text = "";
                _lblError.Visible = false;
            }
        }
    }
}

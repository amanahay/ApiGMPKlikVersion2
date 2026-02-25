using WinFormApiGMPKlik.Models;
using WinFormApiGMPKlik.Services;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    /// <summary>
    /// Dashboard Utama aplikasi WinForm ApiGMPKlik
    /// </summary>
    public partial class DashboardForm : Form
    {
        private readonly ApiClientService _apiClient;
        private readonly AppSettings _settings;
        private readonly Dictionary<string, Button> _navButtons = new();
        private readonly Dictionary<string, Form> _openForms = new();
        private Panel? _contentPanel;
        private Panel? _sidebarPanel;
        private Label? _statusLabel;
        private Label? _userLabel;

        #region Services (Lazy Load)
        private BranchService? _branchService;
        private RoleService? _roleService;
        private PermissionService? _permissionService;
        private RolePermissionService? _rolePermissionService;
        private UserService? _userService;
        private UserProfileService? _userProfileService;
        private UserSecurityService? _userSecurityService;
        private ReferralTreeService? _referralTreeService;
        private DataPriceService? _dataPriceService;
        private WilayahProvinsiService? _wilayahProvinsiService;
        private WilayahKotaKabService? _wilayahKotaKabService;
        private WilayahKecamatanService? _wilayahKecamatanService;
        private WilayahKelurahanDesaService? _wilayahKelurahanDesaService;
        private WilayahDusunService? _wilayahDusunService;

        public BranchService BranchService => _branchService ??= new BranchService(_apiClient);
        public RoleService RoleService => _roleService ??= new RoleService(_apiClient);
        public PermissionService PermissionService => _permissionService ??= new PermissionService(_apiClient);
        public RolePermissionService RolePermissionService => _rolePermissionService ??= new RolePermissionService(_apiClient);
        public UserService UserService => _userService ??= new UserService(_apiClient);
        public UserProfileService UserProfileService => _userProfileService ??= new UserProfileService(_apiClient);
        public UserSecurityService UserSecurityService => _userSecurityService ??= new UserSecurityService(_apiClient);
        public ReferralTreeService ReferralTreeService => _referralTreeService ??= new ReferralTreeService(_apiClient);
        public DataPriceService DataPriceService => _dataPriceService ??= new DataPriceService(_apiClient);
        public WilayahProvinsiService WilayahProvinsiService => _wilayahProvinsiService ??= new WilayahProvinsiService(_apiClient);
        public WilayahKotaKabService WilayahKotaKabService => _wilayahKotaKabService ??= new WilayahKotaKabService(_apiClient);
        public WilayahKecamatanService WilayahKecamatanService => _wilayahKecamatanService ??= new WilayahKecamatanService(_apiClient);
        public WilayahKelurahanDesaService WilayahKelurahanDesaService => _wilayahKelurahanDesaService ??= new WilayahKelurahanDesaService(_apiClient);
        public WilayahDusunService WilayahDusunService => _wilayahDusunService ??= new WilayahDusunService(_apiClient);
        #endregion

        public DashboardForm(ApiClientService apiClient, AppSettings settings)
        {
            _apiClient = apiClient;
            _settings = settings;
            InitializeComponent();
            SetupForm();
            InitializeDashboard();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            // Main Form Settings
            Text = "Api GMPKlik - Dashboard";
            Size = new Size(1400, 900);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 246, 250);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            
            ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Create Menu Strip
            var menuStrip = new MenuStrip();
            
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&Logout", null, OnLogoutClick);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, (s, e) => Application.Exit());
            
            var viewMenu = new ToolStripMenuItem("&View");
            viewMenu.DropDownItems.Add("&Refresh", null, (s, e) => RefreshDashboard());
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add("&Theme", null);
            
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, OnAboutClick);
            
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, helpMenu });
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);

            // Create Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            var titleLabel = new Label
            {
                Text = "🏢 Api GMPKlik Management System",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            _userLabel = new Label
            {
                Text = $"👤 {_settings.Auth.Username}",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(headerPanel.Width - 200, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            headerPanel.Controls.AddRange(new Control[] { titleLabel, _userLabel });
            headerPanel.SizeChanged += (s, e) => 
            {
                _userLabel.Location = new Point(headerPanel.Width - _userLabel.Width - 20, 25);
            };

            Controls.Add(headerPanel);

            // Create Sidebar Panel
            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(45, 52, 70),
                Padding = new Padding(0, 20, 0, 0)
            };

            // Add Navigation Buttons
            int btnTop = 20;
            int btnHeight = 45;
            
            AddNavButton("Dashboard", "📊", ref btnTop, btnHeight, OnDashboardClick, true);
            AddNavButton("Branch", "🏢", ref btnTop, btnHeight, OnBranchClick);
            AddNavButton("Users", "👥", ref btnTop, btnHeight, OnUsersClick);
            AddNavButton("Roles", "🔐", ref btnTop, btnHeight, OnRolesClick);
            AddNavButton("Permissions", "🔑", ref btnTop, btnHeight, OnPermissionsClick);
            AddNavButton("Referral Tree", "🌳", ref btnTop, btnHeight, OnReferralTreeClick);
            AddNavButton("Data Price", "💰", ref btnTop, btnHeight, OnDataPriceClick);
            
            // Wilayah Section
            btnTop += 10;
            var wilayahLabel = new Label
            {
                Text = "📍 WILAYAH",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(150, 160, 180),
                AutoSize = true,
                Location = new Point(20, btnTop)
            };
            _sidebarPanel.Controls.Add(wilayahLabel);
            btnTop += 30;
            
            AddNavButton("Provinsi", "🗺️", ref btnTop, btnHeight, OnProvinsiClick);
            AddNavButton("Kota/Kab", "🏙️", ref btnTop, btnHeight, OnKotaKabClick);
            AddNavButton("Kecamatan", "📍", ref btnTop, btnHeight, OnKecamatanClick);
            AddNavButton("Kelurahan", "🏘️", ref btnTop, btnHeight, OnKelurahanClick);
            AddNavButton("Dusun", "🛖", ref btnTop, btnHeight, OnDusunClick);

            Controls.Add(_sidebarPanel);

            // Create Content Panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 246, 250),
                Padding = new Padding(20)
            };
            Controls.Add(_contentPanel);

            // Create Status Bar
            var statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready")
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var versionLabel = new ToolStripStatusLabel("v1.0.0");
            statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, versionLabel });
            Controls.Add(statusStrip);

            // Set initial content
            ShowDashboardContent();
        }

        private void AddNavButton(string name, string icon, ref int top, int height, EventHandler clickHandler, bool isActive = false)
        {
            var btn = new Button
            {
                Text = $"  {icon}  {name}",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = isActive ? Color.FromArgb(0, 122, 204) : Color.Transparent,
                ForeColor = isActive ? Color.White : Color.FromArgb(200, 210, 230),
                Size = new Size(_sidebarPanel!.Width - 20, height),
                Location = new Point(10, top),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Tag = name
            };

            btn.Click += (s, e) =>
            {
                SetActiveNavButton(name);
                clickHandler(s, e);
            };

            btn.MouseEnter += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(0, 122, 204))
                    btn.BackColor = Color.FromArgb(60, 70, 90);
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(0, 122, 204))
                    btn.BackColor = Color.Transparent;
            };

            _sidebarPanel.Controls.Add(btn);
            _navButtons[name] = btn;
            top += height + 5;
        }

        private void SetActiveNavButton(string name)
        {
            foreach (var btn in _navButtons.Values)
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(200, 210, 230);
            }

            if (_navButtons.TryGetValue(name, out var activeBtn))
            {
                activeBtn.BackColor = Color.FromArgb(0, 122, 204);
                activeBtn.ForeColor = Color.White;
            }
        }

        #region Navigation Event Handlers

        private void OnDashboardClick(object? sender, EventArgs e)
        {
            ShowDashboardContent();
        }

        private void OnBranchClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new BranchListForm(this));
        }

        private void OnUsersClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new UserListForm(this));
        }

        private void OnRolesClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new RoleListForm(this));
        }

        private void OnPermissionsClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new PermissionListForm(this));
        }

        private void OnReferralTreeClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new ReferralTreeForm(this));
        }

        private void OnDataPriceClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new DataPriceListForm(this));
        }

        private void OnProvinsiClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new ProvinsiListForm(this));
        }

        private void OnKotaKabClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new KotaKabListForm(this));
        }

        private void OnKecamatanClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new KecamatanListForm(this));
        }

        private void OnKelurahanClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new KelurahanListForm(this));
        }

        private void OnDusunClick(object? sender, EventArgs e)
        {
            ShowFormInPanel(new DusunListForm(this));
        }

        private void OnLogoutClick(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Apakah Anda yakin ingin logout?", "Konfirmasi Logout", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var loginForm = new LoginForm(_apiClient, _settings);
                loginForm.Show();
                Close();
            }
        }

        private void OnAboutClick(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Api GMPKlik Management System\n\n" +
                "Version: 1.0.0\n" +
                "Built with .NET 10.0\n\n" +
                "© 2026 GMPKlik. All rights reserved.",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        private void ShowFormInPanel(Form form)
        {
            _contentPanel!.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(form);
            form.Show();
            _statusLabel!.Text = $"Active: {form.Text}";
        }

        private void ShowDashboardContent()
        {
            _contentPanel!.Controls.Clear();

            var dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            // Welcome Section
            var welcomePanel = new Panel
            {
                Size = new Size(_contentPanel.Width - 60, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20)
            };
            welcomePanel.Paint += (s, e) => UIHelpers.DrawRoundedBorder(e.Graphics, welcomePanel.ClientRectangle, Color.FromArgb(220, 220, 220), 10);

            var welcomeLabel = new Label
            {
                Text = $"Selamat Datang, {_settings.Auth.Username}! 👋",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(45, 52, 70),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var subLabel = new Label
            {
                Text = "Kelola data bisnis Anda dengan mudah dan efisien.",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(120, 130, 150),
                AutoSize = true,
                Location = new Point(20, 65)
            };

            welcomePanel.Controls.AddRange(new Control[] { welcomeLabel, subLabel });
            dashboardPanel.Controls.Add(welcomePanel);

            // Stats Cards
            var statsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 150),
                Width = _contentPanel.Width - 60,
                Height = 150,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false
            };

            statsPanel.Controls.Add(CreateStatCard("👥 Users", "Loading...", Color.FromArgb(0, 122, 204)));
            statsPanel.Controls.Add(CreateStatCard("🏢 Branches", "Loading...", Color.FromArgb(46, 204, 113)));
            statsPanel.Controls.Add(CreateStatCard("🔐 Roles", "Loading...", Color.FromArgb(155, 89, 182)));
            statsPanel.Controls.Add(CreateStatCard("🌳 Referrals", "Loading...", Color.FromArgb(241, 196, 15)));

            dashboardPanel.Controls.Add(statsPanel);

            // Recent Activity Panel
            var activityPanel = new Panel
            {
                Location = new Point(0, 320),
                Size = new Size(_contentPanel.Width - 60, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20)
            };
            activityPanel.Paint += (s, e) => UIHelpers.DrawRoundedBorder(e.Graphics, activityPanel.ClientRectangle, Color.FromArgb(220, 220, 220), 10);

            var activityTitle = new Label
            {
                Text = "📋 Menu Cepat",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(45, 52, 70),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            activityPanel.Controls.Add(activityTitle);

            // Quick Action Buttons
            var quickActions = new FlowLayoutPanel
            {
                Location = new Point(20, 60),
                Width = activityPanel.Width - 60,
                Height = 200,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true
            };

            quickActions.Controls.Add(CreateQuickActionBtn("➕ Add User", Color.FromArgb(0, 122, 204), OnUsersClick));
            quickActions.Controls.Add(CreateQuickActionBtn("➕ Add Branch", Color.FromArgb(46, 204, 113), OnBranchClick));
            quickActions.Controls.Add(CreateQuickActionBtn("➕ Add Role", Color.FromArgb(155, 89, 182), OnRolesClick));
            quickActions.Controls.Add(CreateQuickActionBtn("📊 View Reports", Color.FromArgb(241, 196, 15), (s, e) => { }));
            quickActions.Controls.Add(CreateQuickActionBtn("⚙️ Settings", Color.FromArgb(149, 165, 166), (s, e) => { }));

            activityPanel.Controls.Add(quickActions);
            dashboardPanel.Controls.Add(activityPanel);

            _contentPanel.Controls.Add(dashboardPanel);
            _statusLabel!.Text = "Dashboard Ready";

            // Load stats asynchronously
            _ = LoadDashboardStatsAsync(statsPanel);
        }

        private Panel CreateStatCard(string title, string value, Color accentColor)
        {
            var card = new Panel
            {
                Size = new Size(240, 130),
                Margin = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            card.Paint += (s, e) => UIHelpers.DrawRoundedBorder(e.Graphics, card.ClientRectangle, accentColor, 10);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(120, 130, 150),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var valueLabel = new Label
            {
                Text = value,
                Name = "ValueLabel",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(20, 50)
            };

            card.Controls.AddRange(new Control[] { titleLabel, valueLabel });
            return card;
        }

        private Button CreateQuickActionBtn(string text, Color backColor, EventHandler clickHandler)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(150, 80),
                Margin = new Padding(10),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = ControlPaint.Light(backColor, 0.1f) },
                Cursor = Cursors.Hand
            };
            btn.Click += clickHandler;
            return btn;
        }

        private async Task LoadDashboardStatsAsync(Panel statsPanel)
        {
            try
            {
                var userResult = await UserService.GetAllAsync(new UserFilterDto { IsDeleted = false });
                var branchResult = await BranchService.GetAllAsync(new BranchFilterDto { IsActive = true });
                var roleResult = await RoleService.GetAllAsync(new RoleFilterDto { IsActive = true });
                var referralResult = await ReferralTreeService.GetAllAsync(new ReferralTreeFilterDto { IsActive = true });

                UpdateStatCard(statsPanel, 0, userResult.Data?.Count.ToString() ?? "0");
                UpdateStatCard(statsPanel, 1, branchResult.Data?.Count.ToString() ?? "0");
                UpdateStatCard(statsPanel, 2, roleResult.Data?.Count.ToString() ?? "0");
                UpdateStatCard(statsPanel, 3, referralResult.Data?.Count.ToString() ?? "0");
            }
            catch
            {
                // Silent fail - stats will show 0
            }
        }

        private void UpdateStatCard(Panel statsPanel, int index, string value)
        {
            if (index < statsPanel.Controls.Count)
            {
                var card = statsPanel.Controls[index];
                var valueLabel = card.Controls.Find("ValueLabel", true).FirstOrDefault() as Label;
                if (valueLabel != null)
                {
                    valueLabel.Text = value;
                }
            }
        }

        private void RefreshDashboard()
        {
            ShowDashboardContent();
        }

        private void InitializeDashboard()
        {
            // Any additional initialization
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}

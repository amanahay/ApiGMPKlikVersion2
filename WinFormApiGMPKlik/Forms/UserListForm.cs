using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Services;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    public partial class UserListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private TextBox? _txtSearch;
        private Label? _lblStatus;
        private List<UserResponseDto> _users = new();

        public UserListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Manajemen User";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            // Toolbar
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblTitle = new Label
            {
                Text = "👥 Manajemen User",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(45, 52, 70),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            _txtSearch = new TextBox
            {
                Location = new Point(400, 20),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "🔍 Cari user..."
            };
            _txtSearch.TextChanged += async (s, e) => await LoadDataAsync();

            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah User", Color.FromArgb(0, 122, 204), OnAddClick);
            btnAdd.Location = new Point(680, 18);
            btnAdd.Size = new Size(150, 35);

            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(850, 18);
            btnRefresh.Size = new Size(120, 35);

            toolbar.Controls.AddRange(new Control[] { lblTitle, _txtSearch, btnAdd, btnRefresh });

            // Data Grid
            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 200, DataPropertyName = "Id" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username", Width = 150, DataPropertyName = "Username" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", Width = 200, DataPropertyName = "Email" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Nama Lengkap", Width = 180, DataPropertyName = "FullName" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhoneNumber", HeaderText = "Telepon", Width = 120, DataPropertyName = "PhoneNumber" });
            _dataGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BranchName", HeaderText = "Branch", Width = 150, DataPropertyName = "BranchName" });
            _dataGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 });

            _dataGrid.CellContentClick += OnCellClick;

            // Status Bar
            var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.White };
            _lblStatus = new Label { Text = "Memuat data...", Dock = DockStyle.Fill, Padding = new Padding(20, 10, 20, 10), ForeColor = Color.Gray };
            statusPanel.Controls.Add(_lblStatus);

            var contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Color.FromArgb(245, 246, 250) };
            contentPanel.Controls.Add(_dataGrid);

            Controls.AddRange(new Control[] { contentPanel, toolbar, statusPanel });

            ResumeLayout(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _lblStatus!.Text = "Memuat data...";
                var filter = new UserFilterDto { Search = _txtSearch?.Text, IsDeleted = false };
                var result = await _dashboard.UserService.GetAllAsync(filter);
                
                if (result.IsSuccess && result.Data != null)
                {
                    _users = result.Data;
                    _dataGrid!.InvokeIfRequired(() =>
                    {
                        _dataGrid.DataSource = null;
                        _dataGrid.DataSource = _users;
                        _lblStatus.Text = $"Total: {_users.Count} user";
                    });
                }
            }
            catch (Exception ex)
            {
                _lblStatus!.Text = $"Error: {ex.Message}";
            }
        }

        private void OnAddClick(object? sender, EventArgs e) => UIHelpers.ShowInfo("Fitur tambah user akan segera hadir!");
        private void OnCellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != _dataGrid!.Columns["Actions"]!.Index) return;
            UIHelpers.ShowInfo($"Edit user: {_users[e.RowIndex].Username}");
        }
    }
}

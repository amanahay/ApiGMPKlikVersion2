using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    public partial class PermissionListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<PermissionDto> _permissions = new();

        public PermissionListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Manajemen Permission";
            Size = new Size(1100, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "🔑 Manajemen Permission", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(46, 204, 113), (s, e) => UIHelpers.ShowInfo("Fitur tambah permission akan segera hadir!"));
            btnAdd.Location = new Point(680, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(850, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Kode", Width = 200, DataPropertyName = "Code" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Nama", Width = 200, DataPropertyName = "Name" },
                new DataGridViewTextBoxColumn { Name = "Module", HeaderText = "Modul", Width = 150, DataPropertyName = "Module" },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Deskripsi", Width = 250, DataPropertyName = "Description" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit permission: {_permissions[e.RowIndex].Name}"); };

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
                var result = await _dashboard.PermissionService.GetAllAsync(new PermissionFilterDto { IsActive = true });
                if (result.IsSuccess && result.Data != null)
                {
                    _permissions = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _permissions; _lblStatus.Text = $"Total: {_permissions.Count} permission"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }
}

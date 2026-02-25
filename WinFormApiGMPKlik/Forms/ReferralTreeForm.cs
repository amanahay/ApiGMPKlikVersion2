using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    public partial class ReferralTreeForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private TextBox? _txtSearch;
        private TreeView? _treeView;
        private List<ReferralTreeDto> _referrals = new();

        public ReferralTreeForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Referral Tree";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "🌳 Referral Tree", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            
            _txtSearch = new TextBox { Location = new Point(350, 20), Size = new Size(250, 30), Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "🔍 Cari user..." };
            var btnView = UIHelpers.CreateStyledButton("👁️ Lihat Tree", Color.FromArgb(0, 122, 204), OnViewTreeClick);
            btnView.Location = new Point(620, 18); btnView.Size = new Size(120, 35);
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(46, 204, 113), (s, e) => UIHelpers.ShowInfo("Fitur tambah referral akan segera hadir!"));
            btnAdd.Location = new Point(760, 18); btnAdd.Size = new Size(120, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(900, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, _txtSearch, btnView, btnAdd, btnRefresh });

            // Split Container
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 300, BackColor = Color.FromArgb(245, 246, 250) };
            
            // Tree View
            _treeView = new TreeView { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10F) };
            _treeView.Nodes.Add(new TreeNode("🌳 Referral Tree (Pilih user untuk melihat)"));
            split.Panel1.Controls.Add(_treeView);

            // Data Grid
            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "RootUserName", HeaderText = "Root User", Width = 150, DataPropertyName = "RootUserName" },
                new DataGridViewTextBoxColumn { Name = "ReferredUserName", HeaderText = "Referred User", Width = 150, DataPropertyName = "ReferredUserName" },
                new DataGridViewTextBoxColumn { Name = "Level", HeaderText = "Level", Width = 80, DataPropertyName = "Level" },
                new DataGridViewTextBoxColumn { Name = "CommissionPercent", HeaderText = "Komisi %", Width = 100, DataPropertyName = "CommissionPercent" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            split.Panel2.Controls.Add(_dataGrid);

            var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.White };
            _lblStatus = new Label { Text = "Memuat data...", Dock = DockStyle.Fill, Padding = new Padding(20, 10, 20, 10), ForeColor = Color.Gray };
            statusPanel.Controls.Add(_lblStatus);

            Controls.AddRange(new Control[] { split, toolbar, statusPanel });
            ResumeLayout(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _lblStatus!.Text = "Memuat data...";
                var result = await _dashboard.ReferralTreeService.GetAllAsync(new ReferralTreeFilterDto { IsActive = true });
                if (result.IsSuccess && result.Data != null)
                {
                    _referrals = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _referrals; _lblStatus.Text = $"Total: {_referrals.Count} referral"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }

        private async void OnViewTreeClick(object? sender, EventArgs e)
        {
            var search = _txtSearch?.Text;
            if (string.IsNullOrEmpty(search)) { UIHelpers.ShowWarning("Masukkan username atau user ID"); return; }
            
            _lblStatus!.Text = "Memuat tree...";
            // In real implementation, search by username first to get user ID
            // Then call GetTreeByRootUserIdAsync
            UIHelpers.ShowInfo($"Melihat tree untuk: {search}");
            _lblStatus.Text = "Tree dimuat";
        }
    }
}

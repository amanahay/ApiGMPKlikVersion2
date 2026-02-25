using ApiGMPKlik.DTOs;
using WinFormApiGMPKlik.Services;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    /// <summary>
    /// Form untuk mengelola Branch
    /// </summary>
    public partial class BranchListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private TextBox? _txtSearch;
        private Label? _lblStatus;
        private List<BranchDto> _branches = new();

        public BranchListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Manajemen Branch";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            // Toolbar Panel
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };

            // Title
            var lblTitle = new Label
            {
                Text = "🏢 Manajemen Branch",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(45, 52, 70),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Search Box
            _txtSearch = new TextBox
            {
                Location = new Point(400, 20),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "🔍 Cari branch..."
            };
            _txtSearch.TextChanged += async (s, e) => await LoadDataAsync();

            // Add Button
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah Branch", Color.FromArgb(0, 122, 204), OnAddClick);
            btnAdd.Location = new Point(680, 18);
            btnAdd.Size = new Size(150, 35);

            // Refresh Button
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(850, 18);
            btnRefresh.Size = new Size(120, 35);

            toolbar.Controls.AddRange(new Control[] { lblTitle, _txtSearch, btnAdd, btnRefresh });

            // Data Grid
            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50, DataPropertyName = "Id" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Kode", Width = 100, DataPropertyName = "Code" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Nama Branch", Width = 200, DataPropertyName = "Name" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "City", HeaderText = "Kota", Width = 150, DataPropertyName = "City" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Province", HeaderText = "Provinsi", Width = 150, DataPropertyName = "Province" });
            _dataGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsMainBranch", HeaderText = "Main", Width = 60, DataPropertyName = "IsMainBranch" });
            _dataGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" });
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserCount", HeaderText = "Users", Width = 80, DataPropertyName = "UserCount" });

            // Action Buttons Column
            var actionCol = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "Aksi",
                Text = "✏️",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            _dataGrid.Columns.Add(actionCol);

            _dataGrid.CellContentClick += OnCellClick;

            // Status Bar
            var statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            _lblStatus = new Label
            {
                Text = "Memuat data...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.Gray
            };
            statusPanel.Controls.Add(_lblStatus);

            // Layout
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 246, 250)
            };
            contentPanel.Controls.Add(_dataGrid);

            Controls.AddRange(new Control[] { contentPanel, toolbar, statusPanel });

            ResumeLayout(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _lblStatus!.Text = "Memuat data...";
                
                var filter = new BranchFilterDto
                {
                    Search = _txtSearch?.Text
                };

                var result = await _dashboard.BranchService.GetAllAsync(filter);
                
                if (result.IsSuccess && result.Data != null)
                {
                    _branches = result.Data;
                    _dataGrid!.InvokeIfRequired(() =>
                    {
                        _dataGrid.DataSource = null;
                        _dataGrid.DataSource = _branches;
                        _lblStatus.Text = $"Total: {_branches.Count} branch";
                    });
                }
                else
                {
                    _lblStatus.Text = $"Error: {result.Message}";
                }
            }
            catch (Exception ex)
            {
                _lblStatus!.Text = $"Error: {ex.Message}";
            }
        }

        private void OnAddClick(object? sender, EventArgs e)
        {
            using var form = new BranchEditForm(null);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadDataAsync();
            }
        }

        private void OnCellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != _dataGrid!.Columns["Actions"]!.Index) return;

            var branch = _branches[e.RowIndex];
            using var form = new BranchEditForm(branch);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadDataAsync();
            }
        }
    }

    /// <summary>
    /// Form untuk tambah/edit Branch
    /// </summary>
    public partial class BranchEditForm : Form
    {
        private BranchDto? _branch;
        private TextBox? _txtName, _txtCode, _txtAddress, _txtCity, _txtProvince, _txtPostalCode, _txtPhone, _txtEmail;
        private CheckBox? _chkIsMainBranch, _chkIsActive;

        public BranchEditForm(BranchDto? branch)
        {
            _branch = branch;
            InitializeComponent();
            if (branch != null) LoadData();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = _branch == null ? "Tambah Branch" : "Edit Branch";
            Size = new Size(500, 550);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            int y = 20;
            int labelWidth = 120;
            int controlWidth = 320;

            // Name
            var lblName = new Label { Text = "Nama *:", Location = new Point(20, y), Width = labelWidth };
            _txtName = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // Code
            var lblCode = new Label { Text = "Kode *:", Location = new Point(20, y), Width = labelWidth };
            _txtCode = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // Address
            var lblAddress = new Label { Text = "Alamat:", Location = new Point(20, y), Width = labelWidth };
            _txtAddress = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // City
            var lblCity = new Label { Text = "Kota:", Location = new Point(20, y), Width = labelWidth };
            _txtCity = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // Province
            var lblProvince = new Label { Text = "Provinsi:", Location = new Point(20, y), Width = labelWidth };
            _txtProvince = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // Postal Code
            var lblPostal = new Label { Text = "Kode Pos:", Location = new Point(20, y), Width = labelWidth };
            _txtPostalCode = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // Phone
            var lblPhone = new Label { Text = "Telepon:", Location = new Point(20, y), Width = labelWidth };
            _txtPhone = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 40;

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, y), Width = labelWidth };
            _txtEmail = new TextBox { Location = new Point(150, y), Width = controlWidth };
            y += 50;

            // Checkboxes
            _chkIsMainBranch = new CheckBox { Text = "Main Branch", Location = new Point(150, y), Width = 150 };
            _chkIsActive = new CheckBox { Text = "Aktif", Location = new Point(320, y), Width = 100, Checked = true };
            y += 60;

            // Buttons
            var btnSave = UIHelpers.CreateStyledButton("💾 Simpan", Color.FromArgb(0, 122, 204), OnSaveClick);
            btnSave.Location = new Point(150, y);
            btnSave.Size = new Size(120, 40);

            var btnCancel = UIHelpers.CreateStyledButton("❌ Batal", Color.FromArgb(149, 165, 166), (s, e) => DialogResult = DialogResult.Cancel);
            btnCancel.Location = new Point(290, y);
            btnCancel.Size = new Size(120, 40);

            Controls.AddRange(new Control[]
            {
                lblName, _txtName!, lblCode, _txtCode!, lblAddress, _txtAddress!,
                lblCity, _txtCity!, lblProvince, _txtProvince!, lblPostal, _txtPostalCode!,
                lblPhone, _txtPhone!, lblEmail, _txtEmail!, _chkIsMainBranch, _chkIsActive,
                btnSave, btnCancel
            });

            ResumeLayout(false);
        }

        private void LoadData()
        {
            if (_branch == null) return;
            _txtName!.Text = _branch.Name;
            _txtCode!.Text = _branch.Code ?? "";
            _txtAddress!.Text = _branch.Address ?? "";
            _txtCity!.Text = _branch.City ?? "";
            _txtProvince!.Text = _branch.Province ?? "";
            _txtPostalCode!.Text = _branch.PostalCode ?? "";
            _txtPhone!.Text = _branch.Phone ?? "";
            _txtEmail!.Text = _branch.Email ?? "";
            _chkIsMainBranch!.Checked = _branch.IsMainBranch;
            _chkIsActive!.Checked = _branch.IsActive;
        }

        private async void OnSaveClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName?.Text))
            {
                UIHelpers.ShowError("Nama branch harus diisi");
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtCode?.Text))
            {
                UIHelpers.ShowError("Kode branch harus diisi");
                return;
            }

            // Here you would call the API to save
            // For now, just close with OK
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

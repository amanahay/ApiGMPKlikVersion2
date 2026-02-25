using ApiGMPKlik.DTOs.Address;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    // ========== PROVINSI ==========
    public partial class ProvinsiListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<ProvinsiListDto> _data = new();

        public ProvinsiListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Manajemen Provinsi";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "🗺️ Manajemen Provinsi", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(231, 76, 60), (s, e) => UIHelpers.ShowInfo("Fitur tambah provinsi akan segera hadir!"));
            btnAdd.Location = new Point(680, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(850, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "KodeProvinsi", HeaderText = "Kode", Width = 100, DataPropertyName = "KodeProvinsi" },
                new DataGridViewTextBoxColumn { Name = "Nama", HeaderText = "Nama Provinsi", Width = 250, DataPropertyName = "Nama" },
                new DataGridViewTextBoxColumn { Name = "KotaKabCount", HeaderText = "Kota/Kab", Width = 100, DataPropertyName = "KotaKabCount" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit: {_data[e.RowIndex].Nama}"); };

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
                var result = await _dashboard.WilayahProvinsiService.GetListAsync(1, 100);
                if (result.IsSuccess && result.Data != null)
                {
                    _data = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _data; _lblStatus.Text = $"Total: {_data.Count} provinsi"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }

    // ========== KOTA/KAB ==========
    public partial class KotaKabListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<KotaKabListDto> _data = new();

        public KotaKabListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Manajemen Kota/Kabupaten";
            Size = new Size(1100, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "🏙️ Manajemen Kota/Kabupaten", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(52, 152, 219), (s, e) => UIHelpers.ShowInfo("Fitur tambah kota/kab akan segera hadir!"));
            btnAdd.Location = new Point(750, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(920, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "KodeKotaKabupaten", HeaderText = "Kode", Width = 100, DataPropertyName = "KodeKotaKabupaten" },
                new DataGridViewTextBoxColumn { Name = "Nama", HeaderText = "Nama", Width = 200, DataPropertyName = "Nama" },
                new DataGridViewTextBoxColumn { Name = "Jenis", HeaderText = "Jenis", Width = 100, DataPropertyName = "Jenis" },
                new DataGridViewTextBoxColumn { Name = "ProvinsiNama", HeaderText = "Provinsi", Width = 150, DataPropertyName = "ProvinsiNama" },
                new DataGridViewTextBoxColumn { Name = "KecamatanCount", HeaderText = "Kecamatan", Width = 100, DataPropertyName = "KecamatanCount" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit: {_data[e.RowIndex].Nama}"); };

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
                var result = await _dashboard.WilayahKotaKabService.GetListAsync(1, 100);
                if (result.IsSuccess && result.Data != null)
                {
                    _data = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _data; _lblStatus.Text = $"Total: {_data.Count} kota/kabupaten"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }

    // ========== KECAMATAN ==========
    public partial class KecamatanListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<KecamatanListDto> _data = new();

        public KecamatanListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Manajemen Kecamatan";
            Size = new Size(1100, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "📍 Manajemen Kecamatan", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(155, 89, 182), (s, e) => UIHelpers.ShowInfo("Fitur tambah kecamatan akan segera hadir!"));
            btnAdd.Location = new Point(750, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(920, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "KodeKecamatan", HeaderText = "Kode", Width = 100, DataPropertyName = "KodeKecamatan" },
                new DataGridViewTextBoxColumn { Name = "Nama", HeaderText = "Nama", Width = 200, DataPropertyName = "Nama" },
                new DataGridViewTextBoxColumn { Name = "KotaKabNama", HeaderText = "Kota/Kab", Width = 150, DataPropertyName = "KotaKabNama" },
                new DataGridViewTextBoxColumn { Name = "ProvinsiNama", HeaderText = "Provinsi", Width = 150, DataPropertyName = "ProvinsiNama" },
                new DataGridViewTextBoxColumn { Name = "KelurahanDesaCount", HeaderText = "Kel/Desa", Width = 100, DataPropertyName = "KelurahanDesaCount" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit: {_data[e.RowIndex].Nama}"); };

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
                var result = await _dashboard.WilayahKecamatanService.GetListAsync(1, 100);
                if (result.IsSuccess && result.Data != null)
                {
                    _data = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _data; _lblStatus.Text = $"Total: {_data.Count} kecamatan"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }

    // ========== KELURAHAN ==========
    public partial class KelurahanListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<KelurahanDesaListDto> _data = new();

        public KelurahanListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Manajemen Kelurahan/Desa";
            Size = new Size(1200, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "🏘️ Manajemen Kelurahan/Desa", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(46, 204, 113), (s, e) => UIHelpers.ShowInfo("Fitur tambah kelurahan akan segera hadir!"));
            btnAdd.Location = new Point(800, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(970, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "KodeKelurahanDesa", HeaderText = "Kode", Width = 100, DataPropertyName = "KodeKelurahanDesa" },
                new DataGridViewTextBoxColumn { Name = "Nama", HeaderText = "Nama", Width = 200, DataPropertyName = "Nama" },
                new DataGridViewTextBoxColumn { Name = "Jenis", HeaderText = "Jenis", Width = 100, DataPropertyName = "Jenis" },
                new DataGridViewTextBoxColumn { Name = "KecamatanNama", HeaderText = "Kecamatan", Width = 150, DataPropertyName = "KecamatanNama" },
                new DataGridViewTextBoxColumn { Name = "KotaKabNama", HeaderText = "Kota/Kab", Width = 150, DataPropertyName = "KotaKabNama" },
                new DataGridViewTextBoxColumn { Name = "DusunCount", HeaderText = "Dusun", Width = 80, DataPropertyName = "DusunCount" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit: {_data[e.RowIndex].Nama}"); };

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
                var result = await _dashboard.WilayahKelurahanDesaService.GetListAsync(1, 100);
                if (result.IsSuccess && result.Data != null)
                {
                    _data = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _data; _lblStatus.Text = $"Total: {_data.Count} kelurahan/desa"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }

    // ========== DUSUN ==========
    public partial class DusunListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<DusunListDto> _data = new();

        public DusunListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Manajemen Dusun";
            Size = new Size(1100, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "🛖 Manajemen Dusun", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(241, 196, 15), (s, e) => UIHelpers.ShowInfo("Fitur tambah dusun akan segera hadir!"));
            btnAdd.Location = new Point(750, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(920, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "Nama", HeaderText = "Nama Dusun", Width = 250, DataPropertyName = "Nama" },
                new DataGridViewTextBoxColumn { Name = "KelurahanDesaNama", HeaderText = "Kelurahan/Desa", Width = 200, DataPropertyName = "KelurahanDesaNama" },
                new DataGridViewTextBoxColumn { Name = "KecamatanNama", HeaderText = "Kecamatan", Width = 150, DataPropertyName = "KecamatanNama" },
                new DataGridViewTextBoxColumn { Name = "RwCount", HeaderText = "RW", Width = 80, DataPropertyName = "RwCount" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit: {_data[e.RowIndex].Nama}"); };

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
                var result = await _dashboard.WilayahDusunService.GetListAsync(1, 100);
                if (result.IsSuccess && result.Data != null)
                {
                    _data = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _data; _lblStatus.Text = $"Total: {_data.Count} dusun"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }
}

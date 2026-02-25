using ApiGMPKlik.DTOs.DataPrice;
using WinFormApiGMPKlik.Utils;

namespace WinFormApiGMPKlik.Forms
{
    public partial class DataPriceListForm : Form
    {
        private readonly DashboardForm _dashboard;
        private DataGridView? _dataGrid;
        private Label? _lblStatus;
        private List<DataPriceRangeResponseDto> _dataPrices = new();

        public DataPriceListForm(DashboardForm dashboard)
        {
            _dashboard = dashboard;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = "Data Price Range";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            var lblTitle = new Label { Text = "💰 Data Price Range", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 52, 70), AutoSize = true, Location = new Point(20, 20) };
            var btnAdd = UIHelpers.CreateStyledButton("➕ Tambah", Color.FromArgb(241, 196, 15), (s, e) => UIHelpers.ShowInfo("Fitur tambah data price akan segera hadir!"));
            btnAdd.Location = new Point(680, 18); btnAdd.Size = new Size(150, 35);
            var btnRefresh = UIHelpers.CreateStyledButton("🔄 Refresh", Color.FromArgb(149, 165, 166), async (s, e) => await LoadDataAsync());
            btnRefresh.Location = new Point(850, 18); btnRefresh.Size = new Size(120, 35);
            toolbar.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnRefresh });

            _dataGrid = UIHelpers.CreateStyledDataGridView();
            _dataGrid.Dock = DockStyle.Fill;
            _dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60, DataPropertyName = "Id" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Nama", Width = 200, DataPropertyName = "Name" },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Kode", Width = 100, DataPropertyName = "Code" },
                new DataGridViewTextBoxColumn { Name = "MinPrice", HeaderText = "Min Price", Width = 120, DataPropertyName = "MinPrice", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { Name = "MaxPrice", HeaderText = "Max Price", Width = 120, DataPropertyName = "MaxPrice", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { Name = "Currency", HeaderText = "Currency", Width = 80, DataPropertyName = "Currency" },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Kategori", Width = 120, DataPropertyName = "Category" },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 60, DataPropertyName = "IsActive" },
                new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Aksi", Text = "✏️", UseColumnTextForButtonValue = true, Width = 80 }
            });
            _dataGrid.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == _dataGrid.Columns["Actions"]!.Index) UIHelpers.ShowInfo($"Edit: {_dataPrices[e.RowIndex].Name}"); };

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
                var result = await _dashboard.DataPriceService.GetPagedAsync(1, 100);
                if (result.IsSuccess && result.Data != null)
                {
                    _dataPrices = result.Data;
                    _dataGrid!.InvokeIfRequired(() => { _dataGrid.DataSource = null; _dataGrid.DataSource = _dataPrices; _lblStatus.Text = $"Total: {_dataPrices.Count} data price"; });
                }
            }
            catch (Exception ex) { _lblStatus!.Text = $"Error: {ex.Message}"; }
        }
    }
}

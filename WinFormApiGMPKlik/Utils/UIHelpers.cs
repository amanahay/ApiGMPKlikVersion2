using System.Drawing.Drawing2D;

namespace WinFormApiGMPKlik.Utils
{
    /// <summary>
    /// Helper class untuk UI styling
    /// </summary>
    public static class UIHelpers
    {
        public static void DrawRoundedBorder(Graphics g, Rectangle rect, Color color, int radius)
        {
            using (var path = GetRoundedRect(rect, radius))
            using (var pen = new Pen(color, 1))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRect(Graphics g, Rectangle rect, Color color, int radius)
        {
            using (var path = GetRoundedRect(rect, radius))
            using (var brush = new SolidBrush(color))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(brush, path);
            }
        }

        public static GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        public static Button CreateStyledButton(string text, Color backColor, EventHandler? clickHandler = null)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Height = 35,
                Cursor = Cursors.Hand,
                Padding = new Padding(15, 5, 15, 5)
            };

            if (clickHandler != null)
                btn.Click += clickHandler;

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }

        public static TextBox CreateStyledTextBox(string placeholder = "")
        {
            var txt = new TextBox
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
                Height = 30
            };

            if (!string.IsNullOrEmpty(placeholder))
            {
                // Simple placeholder implementation
                txt.Text = placeholder;
                txt.ForeColor = Color.Gray;
                
                txt.Enter += (s, e) =>
                {
                    if (txt.Text == placeholder)
                    {
                        txt.Text = "";
                        txt.ForeColor = Color.Black;
                    }
                };

                txt.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txt.Text))
                    {
                        txt.Text = placeholder;
                        txt.ForeColor = Color.Gray;
                    }
                };
            }

            return txt;
        }

        public static DataGridView CreateStyledDataGridView()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(45, 52, 70),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
                    Padding = new Padding(10, 8, 10, 8),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                ColumnHeadersHeight = 45,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 70, 90),
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                    Padding = new Padding(8, 6, 8, 6),
                    SelectionBackColor = Color.FromArgb(0, 122, 204),
                    SelectionForeColor = Color.White
                },
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(240, 240, 240),
                ReadOnly = true,
                RowHeadersVisible = false,
                RowTemplate = { Height = 40 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 250, 252)
            };

            return dgv;
        }

        public static Panel CreateCardPanel(int width, int height)
        {
            var panel = new Panel
            {
                Size = new Size(width, height),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20)
            };

            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel.ClientRectangle, Color.FromArgb(220, 220, 220), 10);
            return panel;
        }

        public static void ShowLoading(Form form, bool show)
        {
            var loadingPanel = form.Controls.Find("LoadingPanel", true).FirstOrDefault();
            
            if (show && loadingPanel == null)
            {
                loadingPanel = new Panel
                {
                    Name = "LoadingPanel",
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(200, 255, 255, 255)
                };

                var spinner = new Label
                {
                    Text = "⏳",
                    Font = new Font("Segoe UI", 48F, FontStyle.Regular, GraphicsUnit.Point),
                    AutoSize = true,
                    Location = new Point((loadingPanel.Width - 60) / 2, (loadingPanel.Height - 60) / 2)
                };

                loadingPanel.Controls.Add(spinner);
                form.Controls.Add(loadingPanel);
                loadingPanel.BringToFront();
            }
            else if (!show && loadingPanel != null)
            {
                form.Controls.Remove(loadingPanel);
                loadingPanel.Dispose();
            }
        }

        public static DialogResult ShowConfirmation(string message, string title = "Konfirmasi")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static void ShowInfo(string message, string title = "Informasi")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message, string title = "Peringatan")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    /// Extension methods untuk Control
    /// </summary>
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        public static async Task<T> InvokeIfRequired<T>(this Control control, Func<T> func)
        {
            if (control.InvokeRequired)
                return await Task.Run(() => (T)control.Invoke(func)!);
            else
                return func();
        }
    }
}

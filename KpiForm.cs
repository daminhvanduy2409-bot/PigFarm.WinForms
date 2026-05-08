using System;
using System.Drawing;
using System.Windows.Forms;

namespace PigFarm.WinForms
{
    public class KpiForm : Form
    {
        private DataGridView dgv = new();
        private ComboBox cboFilter = new();
        private Button btnRefresh = new();

        public KpiForm()
        {
            Text = "KPI - FCR / ADG";
            Size = new Size(1100, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadKpi();
        }

        private void BuildUI()
        {
            var lblTitle = new Label
            {
                Text = "BÁO CÁO FCR / ADG TỪNG BATCH",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblFilter = new Label { Text = "Lọc:", Location = new Point(20, 55), AutoSize = true, Font = new Font("Arial", 9) };
            cboFilter = new ComboBox
            {
                Location = new Point(60, 52),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilter.Items.AddRange(new[] { "Tất cả", "Active", "Closed" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => LoadKpi();

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(200, 50),
                Size = new Size(100, 28),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadKpi();

            dgv = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(1060, 370),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Font = new Font("Arial", 9)
            };

            Controls.AddRange(new Control[] { lblTitle, lblFilter, cboFilter, btnRefresh, dgv });
        }

        private void LoadKpi()
        {
            string filter = cboFilter.SelectedItem?.ToString() ?? "Tất cả";
            string where = filter == "Tất cả" ? "" : $"WHERE Status='{filter}'";

            var dt = DatabaseHelper.Query(
                $@"SELECT
                    BatchCode         AS [Mã Batch],
                    CONVERT(VARCHAR,ImportDate,103) AS [Ngày Vào],
                    Status            AS [Trạng Thái],
                    InitialCount      AS [SL Vào],
                    InitialAvgWeight  AS [TL Vào (kg)],
                    CurrentCount      AS [SL Hiện Tại],
                    LatestAvgWeight   AS [TL Mới Nhất (kg)],
                    ROUND(TotalFeedKg,0)         AS [Tổng Cám (kg)],
                    ROUND(InitialBiomassKg,1)    AS [Biomass Đầu (kg)],
                    ROUND(TotalMovedWeightKg,1)  AS [KL Xuất/Chết (kg)],
                    ROUND(CurrentInventoryWeightKg,1) AS [KL Tồn (kg)],
                    ROUND(BiomassGainKg,1)       AS [Tăng Trưởng (kg)],
                    FCR                          AS [FCR],
                    ADG_gPerDay                  AS [ADG (g/ngày)],
                    DaysOnFeed                   AS [Ngày Nuôi],
                    CONVERT(VARCHAR,LastSnapshotDate,103) AS [Ngày Cân Cuối]
                   FROM vw_BatchKPI
                   {where}
                   ORDER BY ImportDate DESC");

            dgv.DataSource = dt;

            // Color code rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var status = row.Cells["Trạng Thái"].Value?.ToString();
                if (status == "Active")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220);
                else
                    row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

                // Highlight bad FCR (> 3.0)
                var fcrVal = row.Cells["FCR"].Value;
                if (fcrVal != null && fcrVal != DBNull.Value)
                {
                    if (decimal.TryParse(fcrVal.ToString(), out decimal fcr) && fcr > 3.0m)
                        row.Cells["FCR"].Style.BackColor = Color.FromArgb(255, 200, 200);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PigFarm.WinForms
{
    public partial class MainForm : Form
    {
        // ── KPI card panels ──────────────────────────────────────
        private KpiCard cardBatches = null!;
        private KpiCard cardPigs = null!;
        private KpiCard cardFeed = null!;
        private KpiCard cardFcr = null!;
        private KpiCard cardAdg = null!;
        private KpiCard cardMortality = null!;

        // ── Sidebar nav buttons ──────────────────────────────────
        private NavButton navDashboard = null!;
        private NavButton navBatch = null!;
        private NavButton navFeed = null!;
        private NavButton navMovement = null!;
        private NavButton navSnapshot = null!;
        private NavButton navKpi = null!;

        // ── Content panels ───────────────────────────────────────
        private Panel pnlSidebar = null!;
        private Panel pnlTopBar = null!;
        private Panel pnlContent = null!;
        private Panel pnlKpiCards = null!;
        private Panel pnlCharts = null!;
        private Panel pnlBatchGrid = null!;

        // ── Grid ─────────────────────────────────────────────────
        private DataGridView dgvBatches = null!;

        // ── Mini charts (owner-draw) ─────────────────────────────
        private ChartPanel chartFeed = null!;
        private ChartPanel chartGrowth = null!;

        // ── Timer ────────────────────────────────────────────────
        private System.Windows.Forms.Timer refreshTimer = null!;

        // ── Colors ───────────────────────────────────────────────
        internal static readonly Color ColSidebar = Color.FromArgb(28, 37, 54);
        internal static readonly Color ColTopbar = Color.FromArgb(255, 255, 255);
        internal static readonly Color ColBg = Color.FromArgb(243, 244, 248);
        internal static readonly Color ColAccent = Color.FromArgb(0, 122, 204);
        internal static readonly Color ColGreen = Color.FromArgb(39, 174, 96);
        internal static readonly Color ColOrange = Color.FromArgb(230, 126, 34);
        internal static readonly Color ColRed = Color.FromArgb(192, 57, 43);
        internal static readonly Color ColPurple = Color.FromArgb(142, 68, 173);
        internal static readonly Color ColTeal = Color.FromArgb(26, 188, 156);

        public MainForm()
        {
            Text = "PigFarm Pro — Dashboard";
            Size = new Size(1280, 780);
            MinimumSize = new Size(1024, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColBg;
            Font = new Font("Segoe UI", 9f);
            DoubleBuffered = true;

            BuildLayout();
            LoadDashboard();

            refreshTimer = new System.Windows.Forms.Timer { Interval = 60_000 };
            refreshTimer.Tick += (s, e) => LoadDashboard();
            refreshTimer.Start();
        }

        // ════════════════════════════════════════════════════════
        //  LAYOUT
        // ════════════════════════════════════════════════════════
        private void BuildLayout()
        {
            BuildSidebar();
            BuildTopBar();
            BuildContentArea();
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = ColSidebar
            };

            // Logo / title
            var lblLogo = new Label
            {
                Text = "🐷 PigFarm Pro",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Bounds = new Rectangle(0, 20, 200, 45),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var sep = new Panel { Bounds = new Rectangle(20, 65, 160, 1), BackColor = Color.FromArgb(60, 80, 110) };

            int y = 80;
            navDashboard = new NavButton("⊞  Dashboard", y); y += 46;
            navBatch = new NavButton("☰  Batch", y); y += 46;
            navFeed = new NavButton("🌾  Nhập Cám", y); y += 46;
            navMovement = new NavButton("⇄  Biến Động", y); y += 46;
            navSnapshot = new NavButton("⚖  Cân Đàn", y); y += 46;
            navKpi = new NavButton("📊  KPI Report", y);

            navDashboard.Active = true;

            navDashboard.Click += (s, e) => { SetNav(navDashboard); LoadDashboard(); };
            navBatch.Click += (s, e) => { SetNav(navBatch); new BatchForm().ShowDialog(); LoadDashboard(); };
            navFeed.Click += (s, e) => { SetNav(navFeed); new FeedEntryForm().ShowDialog(); LoadDashboard(); };
            navMovement.Click += (s, e) => { SetNav(navMovement); new MovementForm().ShowDialog(); LoadDashboard(); };
            navSnapshot.Click += (s, e) => { SetNav(navSnapshot); new SnapshotForm().ShowDialog(); LoadDashboard(); };
            navKpi.Click += (s, e) => { SetNav(navKpi); new KpiForm().ShowDialog(); };

            pnlSidebar.Controls.AddRange(new Control[]
            {
                lblLogo, sep,
                navDashboard, navBatch, navFeed, navMovement, navSnapshot, navKpi
            });
            Controls.Add(pnlSidebar);
        }

        private void BuildTopBar()
        {
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = ColTopbar,
                Padding = new Padding(0)
            };
            // Shadow line
            pnlTopBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220)),
                    0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(220, 12),
                AutoSize = true
            };

            var lblTime = new Label
            {
                Name = "lblTime",
                Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(900, 17),
                AutoSize = true
            };

            var btnRefresh = new Button
            {
                Text = "⟳ Refresh",
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(100, 32),
                Location = new Point(1140, 10),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadDashboard();

            pnlTopBar.Controls.AddRange(new Control[] { lblTitle, lblTime, btnRefresh });
            Controls.Add(pnlTopBar);

            var clockTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
            clockTimer.Tick += (s, e) =>
            {
                var lbl = pnlTopBar.Controls["lblTime"] as Label;
                if (lbl != null) lbl.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm");
            };
            clockTimer.Start();
        }

        private void BuildContentArea()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBg,
                Padding = new Padding(20, 12, 20, 12),
                AutoScroll = true
            };

            // KPI cards row
            pnlKpiCards = new Panel
            {
                Bounds = new Rectangle(0, 0, 1040, 110),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            cardBatches = new KpiCard("Batch Active", "--", "trang trại", ColAccent);
            cardPigs = new KpiCard("Tổng Đầu Con", "--", "con", ColGreen);
            cardFeed = new KpiCard("Cám Tháng Này", "--", "kg", ColOrange);
            cardFcr = new KpiCard("FCR Trung Bình", "--", "kg cám/kg tăng", ColPurple);
            cardAdg = new KpiCard("ADG Trung Bình", "--", "g/ngày", ColTeal);
            cardMortality = new KpiCard("Tỷ Lệ Chết", "--", "%", ColRed);

            LayoutCards();

            pnlContent.Controls.Add(pnlKpiCards);

            // Charts row
            pnlCharts = new Panel
            {
                Bounds = new Rectangle(0, 120, 1040, 200),
                BackColor = Color.Transparent
            };

            chartFeed = new ChartPanel("📈 Cám 30 Ngày Gần Nhất", ColOrange)
            {
                Bounds = new Rectangle(0, 0, 505, 195)
            };
            chartGrowth = new ChartPanel("📈 Tăng Trưởng Đàn (Batch Active nhất)", ColGreen)
            {
                Bounds = new Rectangle(515, 0, 505, 195)
            };

            pnlCharts.Controls.Add(chartFeed);
            pnlCharts.Controls.Add(chartGrowth);
            pnlContent.Controls.Add(pnlCharts);

            // Batch grid
            pnlBatchGrid = new Panel
            {
                Bounds = new Rectangle(0, 330, 1040, 340),
                BackColor = Color.White
            };
            pnlBatchGrid.Paint += PaintCard;

            var lblGrid = new Label
            {
                Text = "BATCH ĐANG NUÔI",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(16, 12),
                AutoSize = true
            };

            dgvBatches = BuildStyledGrid();
            dgvBatches.Bounds = new Rectangle(10, 38, 1020, 290);

            pnlBatchGrid.Controls.Add(lblGrid);
            pnlBatchGrid.Controls.Add(dgvBatches);
            pnlContent.Controls.Add(pnlBatchGrid);

            Controls.Add(pnlContent);

            // Resize handler
            Resize += (s, e) => RelayoutOnResize();
        }

        private void LayoutCards()
        {
            var cards = new[] { cardBatches, cardPigs, cardFeed, cardFcr, cardAdg, cardMortality };
            pnlKpiCards.Controls.Clear();
            int cardW = (pnlKpiCards.Width - 5 * 12) / 6;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Bounds = new Rectangle(i * (cardW + 12), 0, cardW, 105);
                pnlKpiCards.Controls.Add(cards[i]);
            }
        }

        private void RelayoutOnResize()
        {
            int w = pnlContent.ClientSize.Width - 40;
            pnlKpiCards.Width = w;
            LayoutCards();
            pnlCharts.Width = w;
            chartFeed.Width = (w - 10) / 2;
            chartGrowth.Width = (w - 10) / 2;
            chartGrowth.Left = chartFeed.Right + 10;
            pnlBatchGrid.Width = w;
            dgvBatches.Width = pnlBatchGrid.Width - 20;
        }

        // ════════════════════════════════════════════════════════
        //  DATA LOADING
        // ════════════════════════════════════════════════════════
        private void LoadDashboard()
        {
            try
            {
                var summary = KpiService.GetDashboardSummary();
                cardBatches.SetValue(summary.ActiveBatches.ToString());
                cardPigs.SetValue(summary.TotalActivePigs.ToString("N0"));
                cardFeed.SetValue(summary.TotalFeedMonth.ToString("N0"));
                cardFcr.SetValue(summary.AvgFcr > 0 ? summary.AvgFcr.ToString("F2") : "—");
                cardAdg.SetValue(summary.AvgAdg > 0 ? summary.AvgAdg.ToString("N0") : "—");
                cardMortality.SetValue(summary.MortalityPct.ToString("F2"));

                // Grid
                var kpis = KpiService.GetAllKpi("Active");
                LoadBatchGrid(kpis);

                // Charts
                var feedTrend = KpiService.GetFeedTrend30Days();
                var feedPoints = new List<(string label, double value)>();
                foreach (System.Data.DataRow r in feedTrend.Rows)
                    feedPoints.Add((r["Ngay"].ToString()!, Convert.ToDouble(r["TongCam"])));
                chartFeed.SetData(feedPoints);

                if (kpis.Count > 0)
                {
                    var growthDt = KpiService.GetGrowthTrend(kpis[0].BatchID);
                    var growthPoints = new List<(string label, double value)>();
                    foreach (System.Data.DataRow r in growthDt.Rows)
                        growthPoints.Add((r["Ngay"].ToString()!, Convert.ToDouble(r["TLTB"])));
                    chartGrowth.SetData(growthPoints);
                    chartGrowth.Title = $"📈 Tăng Trưởng — {kpis[0].BatchCode}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dashboard:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBatchGrid(List<BatchKpi> kpis)
        {
            dgvBatches.Rows.Clear();
            foreach (var k in kpis)
            {
                int i = dgvBatches.Rows.Add(
                    k.BatchCode,
                    k.Barn,
                    k.ImportDate.ToString("dd/MM/yyyy"),
                    k.InitialCount,
                    k.CurrentCount,
                    k.LatestAvgWeight.ToString("F1"),
                    k.TotalFeedKg.ToString("N0"),
                    k.FCR.HasValue ? k.FCR.Value.ToString("F2") : "—",
                    k.ADG_gPerDay.HasValue ? k.ADG_gPerDay.Value.ToString("N0") : "—",
                    k.MortalityPct.ToString("F2") + "%",
                    k.DaysOnFeed,
                    k.Status
                );

                var row = dgvBatches.Rows[i];
                // FCR color
                if (k.FCR.HasValue)
                    row.Cells[7].Style.ForeColor = k.FCR.Value > 3.0m ? ColRed : ColGreen;
                // ADG color
                if (k.ADG_gPerDay.HasValue)
                    row.Cells[8].Style.ForeColor = k.ADG_gPerDay.Value < 600 ? ColOrange : ColGreen;
            }
        }

        // ════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════
        private DataGridView BuildStyledGrid()
        {
            var grid = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(235, 235, 235),
                Font = new Font("Segoe UI", 9f),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 28 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 247, 250),
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                ForeColor = Color.FromArgb(50, 50, 50),
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(4, 0, 4, 0)
            };
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 252),
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.FromArgb(30, 30, 30)
            };
            grid.EnableHeadersVisualStyles = false;

            var cols = new[]
            {
                ("Mã Batch",    80),  ("Chuồng",     70),
                ("Ngày Vào",    90),  ("SL Vào",     65),
                ("SL Hiện Tại", 80),  ("TL TB (kg)", 80),
                ("Tổng Cám",    90),  ("FCR",        60),
                ("ADG g/ngày",  85),  ("Tỷ Lệ Chết", 85),
                ("Ngày Nuôi",   80),  ("Status",     70)
            };
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (var (name, w) in cols)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = name,
                    Width = w,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });
            }
            return grid;
        }

        private void SetNav(NavButton active)
        {
            foreach (Control c in pnlSidebar.Controls)
                if (c is NavButton nb) nb.Active = (nb == active);
        }

        private static void PaintCard(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(220, 220, 225));
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            DrawRoundRect(g, pen, rect, 8);
        }

        internal static void DrawRoundRect(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }

    // ════════════════════════════════════════════════════════════
    //  KpiCard control
    // ════════════════════════════════════════════════════════════
    internal class KpiCard : Panel
    {
        private readonly Label _lblValue;
        private readonly Label _lblUnit;
        private readonly Color _accent;

        public KpiCard(string title, string value, string unit, Color accent)
        {
            _accent = accent;
            BackColor = Color.White;
            Cursor = Cursors.Default;
            DoubleBuffered_Set(true);

            var lblTitle = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 130, 130),
                Location = new Point(14, 12),
                AutoSize = true
            };

            _lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(12, 30),
                AutoSize = true
            };

            _lblUnit = new Label
            {
                Text = unit,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.Gray,
                Location = new Point(14, 75),
                AutoSize = true
            };

            Controls.AddRange(new Control[] { lblTitle, _lblValue, _lblUnit });
            Paint += OnPaint;
        }

        public void SetValue(string v) { _lblValue.Text = v; }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // Left accent bar
            using var brush = new SolidBrush(_accent);
            g.FillRectangle(brush, new Rectangle(0, 0, 5, Height));
            // Border
            using var pen = new Pen(Color.FromArgb(230, 230, 235));
            MainForm.DrawRoundRect(g, pen, new Rectangle(0, 0, Width - 1, Height - 1), 6);
        }

        private void DoubleBuffered_Set(bool value)
        {
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, value);
        }
    }

    // ════════════════════════════════════════════════════════════
    //  NavButton control
    // ════════════════════════════════════════════════════════════
    internal class NavButton : Panel
    {
        private bool _active;
        private readonly Label _lbl;

        public bool Active
        {
            get => _active;
            set { _active = value; Invalidate(); _lbl.ForeColor = value ? Color.White : Color.FromArgb(180, 190, 210); }
        }

        public new event EventHandler? Click;

        public NavButton(string text, int y)
        {
            Bounds = new Rectangle(0, y, 200, 42);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;

            _lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(180, 190, 210),
                Bounds = new Rectangle(0, 0, 200, 42),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0)
            };
            _lbl.Click += (s, e) => Click?.Invoke(this, e);
            MouseClick += (s, e) => Click?.Invoke(this, e);
            Controls.Add(_lbl);
            Paint += OnPaint;
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            if (_active)
            {
                using var b = new SolidBrush(Color.FromArgb(50, 70, 100));
                e.Graphics.FillRectangle(b, ClientRectangle);
                using var accent = new SolidBrush(MainForm.ColAccent);
                e.Graphics.FillRectangle(accent, new Rectangle(0, 0, 4, Height));
            }
        }
    }

    // ════════════════════════════════════════════════════════════
    //  ChartPanel — simple line chart owner-draw
    // ════════════════════════════════════════════════════════════
    internal class ChartPanel : Panel
    {
        public string Title { get; set; }
        private List<(string label, double value)> _data = new();
        private readonly Color _lineColor;

        public ChartPanel(string title, Color lineColor)
        {
            Title = title;
            _lineColor = lineColor;
            BackColor = Color.White;
            Paint += OnPaint;
            DoubleBuffered_Set(true);
        }

        public void SetData(List<(string, double)> data)
        {
            _data = data;
            Invalidate();
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Border
            using var borderPen = new Pen(Color.FromArgb(220, 220, 225));
            MainForm.DrawRoundRect(g, borderPen, new Rectangle(0, 0, Width - 1, Height - 1), 8);

            // Title
            using var titleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            g.DrawString(Title, titleFont, Brushes.DimGray, new PointF(12, 10));

            if (_data.Count < 2) { g.DrawString("Chưa có dữ liệu", new Font("Segoe UI", 9f), Brushes.LightGray, new PointF(Width / 2f - 50, Height / 2f)); return; }

            int padL = 50, padR = 20, padT = 35, padB = 30;
            int chartW = Width - padL - padR;
            int chartH = Height - padT - padB;

            double maxVal = _data.Max(d => d.value);
            double minVal = _data.Min(d => d.value);
            if (maxVal == minVal) maxVal = minVal + 1;

            // Grid lines
            using var gridPen = new Pen(Color.FromArgb(240, 240, 240));
            for (int gi = 0; gi <= 4; gi++)
            {
                int gy = padT + (int)(chartH * gi / 4.0);
                g.DrawLine(gridPen, padL, gy, padL + chartW, gy);
                double gVal = maxVal - (maxVal - minVal) * gi / 4.0;
                g.DrawString(gVal.ToString("N0"), new Font("Segoe UI", 7f), Brushes.LightGray, new PointF(2, gy - 7));
            }

            // Line
            var points = new PointF[_data.Count];
            for (int i = 0; i < _data.Count; i++)
            {
                float px = padL + i * chartW / (float)(_data.Count - 1);
                float py = padT + (float)((maxVal - _data[i].value) / (maxVal - minVal) * chartH);
                points[i] = new PointF(px, py);
            }

            using var linePen = new Pen(_lineColor, 2f) { LineJoin = LineJoin.Round };
            g.DrawLines(linePen, points);

            // Fill under line
            var fillPath = new System.Drawing.Drawing2D.GraphicsPath();
            fillPath.AddLines(points);
            fillPath.AddLine(points[^1].X, padT + chartH, padL, padT + chartH);
            fillPath.CloseFigure();
            using var fillBrush = new LinearGradientBrush(
                new PointF(0, padT), new PointF(0, padT + chartH),
                Color.FromArgb(60, _lineColor), Color.FromArgb(5, _lineColor));
            g.FillPath(fillBrush, fillPath);

            // Dots
            using var dotBrush = new SolidBrush(_lineColor);
            foreach (var pt in points)
                g.FillEllipse(dotBrush, pt.X - 3, pt.Y - 3, 6, 6);
        }

        private void DoubleBuffered_Set(bool v)
        {
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, v);
        }
    }
}
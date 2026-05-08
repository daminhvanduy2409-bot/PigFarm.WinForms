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
        // ── KPI cards ────────────────────────────────────────────
        private KpiCard cardBatches = null!;
        private KpiCard cardPigs = null!;
        private KpiCard cardFeed = null!;
        private KpiCard cardFcr = null!;
        private KpiCard cardAdg = null!;
        private KpiCard cardMortality = null!;

        // ── Sidebar nav ──────────────────────────────────────────
        private NavButton navDashboard = null!;
        private NavButton navBatch = null!;
        private NavButton navFeed = null!;
        private NavButton navMovement = null!;
        private NavButton navSnapshot = null!;
        private NavButton navKpi = null!;

        // ── Layout panels ────────────────────────────────────────
        private Panel pnlSidebar = null!;
        private Panel pnlTopBar = null!;
        private Panel pnlContent = null!;
        private FlowLayoutPanel pnlKpiCards = null!;
        private TableLayoutPanel pnlCharts = null!;
        private Panel pnlBatchGrid = null!;

        // ── Grid + Charts ────────────────────────────────────────
        private DataGridView dgvBatches = null!;
        private ChartPanel chartFeed = null!;
        private ChartPanel chartGrowth = null!;

        // ── Auto-refresh ─────────────────────────────────────────
        private System.Windows.Forms.Timer refreshTimer = null!;

        // ── Color palette ────────────────────────────────────────
        internal static readonly Color ColSidebar = Color.FromArgb(28, 37, 54);
        internal static readonly Color ColTopbar = Color.White;
        internal static readonly Color ColBg = Color.FromArgb(243, 244, 248);
        internal static readonly Color ColAccent = Color.FromArgb(0, 122, 204);
        internal static readonly Color ColGreen = Color.FromArgb(39, 174, 96);
        internal static readonly Color ColOrange = Color.FromArgb(230, 126, 34);
        internal static readonly Color ColRed = Color.FromArgb(192, 57, 43);
        internal static readonly Color ColPurple = Color.FromArgb(142, 68, 173);
        internal static readonly Color ColTeal = Color.FromArgb(26, 188, 156);

        // ── Sidebar width at 96 DPI ──────────────────────────────
        private const int SidebarBaseW = 200;

        public MainForm()
        {
            Text = "PigFarm Pro — Dashboard";
            MinimumSize = new Size(1024, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColBg;
            Font = new Font("Segoe UI", 9f);
            DoubleBuffered = true;
            // AutoScaleMode = Dpi → WinForms tự scale layout khi DPI thay đổi
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96f, 96f);

            BuildLayout();

            // Set kích thước SAU khi layout xong để StartPosition hoạt động
            ClientSize = new Size(1280, 780);

            LoadDashboard();

            refreshTimer = new System.Windows.Forms.Timer { Interval = 60_000 };
            refreshTimer.Tick += (s, e) => LoadDashboard();
            refreshTimer.Start();
        }

        // ════════════════════════════════════════════════════════
        // LAYOUT
        // ════════════════════════════════════════════════════════
        private void BuildLayout()
        {
            // Thứ tự Add quan trọng: Sidebar → TopBar → Content
            BuildSidebar();
            BuildTopBar();
            BuildContentArea();
        }

        // ── Sidebar ──────────────────────────────────────────────
        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = SidebarBaseW,
                BackColor = ColSidebar
            };

            var lblLogo = new Label
            {
                Text = "🐷 PigFarm Pro",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 56,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var sep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(60, 80, 110),
                Margin = new Padding(20, 0, 20, 0)
            };

            // Nav buttons dùng FlowLayout để tự stack theo DPI
            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };

            navDashboard = new NavButton("⊞ Dashboard");
            navBatch = new NavButton("☰ Batch");
            navFeed = new NavButton("🌾 Nhập Cám");
            navMovement = new NavButton("⇄ Biến Động");
            navSnapshot = new NavButton("⚖ Cân Đàn");
            navKpi = new NavButton("📊 KPI Report");

            navDashboard.Active = true;

            navDashboard.Click += (s, e) => { SetNav(navDashboard); LoadDashboard(); };
            navBatch.Click += (s, e) => { SetNav(navBatch); new BatchForm().ShowDialog(); LoadDashboard(); };
            navFeed.Click += (s, e) => { SetNav(navFeed); new FeedEntryForm().ShowDialog(); LoadDashboard(); };
            navMovement.Click += (s, e) => { SetNav(navMovement); new MovementForm().ShowDialog(); LoadDashboard(); };
            navSnapshot.Click += (s, e) => { SetNav(navSnapshot); new SnapshotForm().ShowDialog(); LoadDashboard(); };
            navKpi.Click += (s, e) => { SetNav(navKpi); new KpiForm().ShowDialog(); };

            navFlow.Controls.AddRange(new Control[]
                { navDashboard, navBatch, navFeed, navMovement, navSnapshot, navKpi });

            // Add theo thứ tự ngược (Dock.Top stack từ dưới lên)
            pnlSidebar.Controls.Add(navFlow);
            pnlSidebar.Controls.Add(sep);
            pnlSidebar.Controls.Add(lblLogo);

            Controls.Add(pnlSidebar);
        }

        // ── TopBar ────────────────────────────────────────────────
        private void BuildTopBar()
        {
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = ColTopbar
            };
            pnlTopBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220)),
                    0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);

            var lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblTime = new Label
            {
                Name = "lblTime",
                Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnRefresh = new Button
            {
                Text = "⟳ Refresh",
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(100, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadDashboard();

            pnlTopBar.Controls.AddRange(new Control[] { lblTitle, lblTime, btnRefresh });

            // Căn phải responsive
            pnlTopBar.Resize += (s, e) =>
            {
                btnRefresh.Location = new Point(pnlTopBar.Width - btnRefresh.Width - 16, 10);
                lblTime.Location = new Point(btnRefresh.Left - lblTime.Width - 16, 18);
            };

            Controls.Add(pnlTopBar);

            var clockTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
            clockTimer.Tick += (s, e) =>
            {
                if (pnlTopBar.Controls["lblTime"] is Label lbl)
                    lbl.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm");
            };
            clockTimer.Start();
        }

        // ── Content area ─────────────────────────────────────────
        private void BuildContentArea()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBg,
                Padding = new Padding(16, 12, 16, 12),
                AutoScroll = true
            };

            // ── 1. KPI Cards — FlowLayout tự wrap ─────────────────
            pnlKpiCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 118,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                AutoSize = false
            };

            cardBatches = new KpiCard("Batch Active", "--", "trang trại", ColAccent);
            cardPigs = new KpiCard("Tổng Đầu Con", "--", "con", ColGreen);
            cardFeed = new KpiCard("Cám Tháng Này", "--", "kg", ColOrange);
            cardFcr = new KpiCard("FCR Trung Bình", "--", "kg cám/kg tăng", ColPurple);
            cardAdg = new KpiCard("ADG Trung Bình", "--", "g/ngày", ColTeal);
            cardMortality = new KpiCard("Tỷ Lệ Chết", "--", "%", ColRed);

            pnlKpiCards.Controls.AddRange(new Control[]
                { cardBatches, cardPigs, cardFeed, cardFcr, cardAdg, cardMortality });

            // Resize cards đều nhau khi form thay đổi kích thước
            pnlContent.Resize += (s, e) => ResizeKpiCards();

            // ── 2. Spacer ──────────────────────────────────────────
            var spacer1 = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent };

            // ── 3. Charts — TableLayoutPanel 1 row 2 col ──────────
            pnlCharts = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 200,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            pnlCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            pnlCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            pnlCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            chartFeed = new ChartPanel("📈 Cám 30 Ngày Gần Nhất", ColOrange) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
            chartGrowth = new ChartPanel("📈 Tăng Trưởng Đàn (Batch Active nhất)", ColGreen) { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 0) };

            pnlCharts.Controls.Add(chartFeed, 0, 0);
            pnlCharts.Controls.Add(chartGrowth, 1, 0);

            // ── 4. Spacer ──────────────────────────────────────────
            var spacer2 = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent };

            // ── 5. Batch Grid ─────────────────────────────────────
            pnlBatchGrid = new Panel
            {
                Dock = DockStyle.Top,
                Height = 340,
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
            dgvBatches.Dock = DockStyle.Fill;

            // Bọc grid để có padding bên trong
            var gridWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 38, 10, 8)
            };
            gridWrapper.Controls.Add(dgvBatches);

            pnlBatchGrid.Controls.Add(gridWrapper);
            pnlBatchGrid.Controls.Add(lblGrid);

            // ── Add vào pnlContent (Dock.Top stack từ dưới lên) ──
            pnlContent.Controls.Add(pnlBatchGrid);
            pnlContent.Controls.Add(spacer2);
            pnlContent.Controls.Add(pnlCharts);
            pnlContent.Controls.Add(spacer1);
            pnlContent.Controls.Add(pnlKpiCards);

            Controls.Add(pnlContent);
        }

        // ── Resize KPI cards đều nhau ─────────────────────────────
        private void ResizeKpiCards()
        {
            int total = pnlContent.ClientSize.Width - pnlContent.Padding.Horizontal;
            int count = pnlKpiCards.Controls.Count;
            int gap = 10;
            int cardW = (total - gap * (count - 1)) / count;
            cardW = Math.Max(cardW, 120);

            foreach (Control c in pnlKpiCards.Controls)
            {
                c.Size = new Size(cardW, 105);
                c.Margin = new Padding(0, 0, gap, 0);
            }

            // Chiều cao FlowPanel = card height + top/bottom padding
            pnlKpiCards.Height = 118;
        }

        // ════════════════════════════════════════════════════════
        // DATA LOADING
        // ════════════════════════════════════════════════════════
        private void LoadDashboard()
        {
            try
            {
                var s = KpiService.GetDashboardSummary();

                cardBatches.SetValue(s.ActiveBatches.ToString());
                cardPigs.SetValue(s.TotalActivePigs.ToString("N0"));
                cardFeed.SetValue(s.TotalFeedMonth.ToString("N0"));
                cardFcr.SetValue(s.AvgFcr > 0 ? s.AvgFcr.ToString("F2") : "—");
                cardAdg.SetValue(s.AvgAdg > 0 ? s.AvgAdg.ToString("N0") : "—");
                cardMortality.SetValue(s.MortalityPct.ToString("F2"));

                var kpis = KpiService.GetAllKpi("Active");
                LoadBatchGrid(kpis);

                // Feed chart
                var feedTrend = KpiService.GetFeedTrend30Days();
                var feedPoints = new List<(string, double)>();
                foreach (System.Data.DataRow r in feedTrend.Rows)
                    feedPoints.Add((r["Ngay"].ToString()!, Convert.ToDouble(r["TongCam"])));
                chartFeed.SetData(feedPoints);

                // Growth chart
                if (kpis.Count > 0)
                {
                    var growthDt = KpiService.GetGrowthTrend(kpis[0].BatchID);
                    var growthPts = new List<(string, double)>();
                    foreach (System.Data.DataRow r in growthDt.Rows)
                        growthPts.Add((r["Ngay"].ToString()!, Convert.ToDouble(r["TLTB"])));
                    chartGrowth.SetData(growthPts);
                    chartGrowth.Title = $"📈 Tăng Trưởng — {kpis[0].BatchCode}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dashboard:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBatchGrid(System.Collections.Generic.List<BatchKpi> kpis)
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
                    k.Status);

                var row = dgvBatches.Rows[i];
                if (k.FCR.HasValue)
                    row.Cells[7].Style.ForeColor = k.FCR.Value > 3.0m ? ColRed : ColGreen;
                if (k.ADG_gPerDay.HasValue)
                    row.Cells[8].Style.ForeColor = k.ADG_gPerDay.Value < 600 ? ColOrange : ColGreen;
            }
        }

        // ════════════════════════════════════════════════════════
        // HELPERS
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.RowTemplate.Height = 28;

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
                ("Mã Batch", 80), ("Chuồng", 70), ("Ngày Vào", 90),
                ("SL Vào", 65),   ("SL Hiện Tại", 80), ("TL TB (kg)", 80),
                ("Tổng Cám", 90), ("FCR", 60), ("ADG g/ngày", 85),
                ("Tỷ Lệ Chết", 85), ("Ngày Nuôi", 80), ("Status", 70)
            };

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (var (name, w) in cols)
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = name,
                    Width = w,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

            return grid;
        }

        private void SetNav(NavButton active)
        {
            foreach (Control c in pnlSidebar.Controls)
                if (c is FlowLayoutPanel flp)
                    foreach (Control nb in flp.Controls)
                        if (nb is NavButton btn) btn.Active = (btn == active);
        }

        internal static void PaintCard(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(220, 220, 225));
            DrawRoundRect(e.Graphics, pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1), 8);
        }

        internal static void DrawRoundRect(Graphics g, Pen pen, Rectangle rect, int r)
        {
            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }

    // ════════════════════════════════════════════════════════════
    // KpiCard
    // ════════════════════════════════════════════════════════════
    internal class KpiCard : Panel
    {
        private readonly Label _lblValue;
        private readonly Color _accent;

        public KpiCard(string title, string value, string unit, Color accent)
        {
            _accent = accent;
            BackColor = Color.White;
            Cursor = Cursors.Default;
            SetDoubleBuffered(true);

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
            var lblUnit = new Label
            {
                Text = unit,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.Gray,
                Location = new Point(14, 75),
                AutoSize = true
            };

            Controls.AddRange(new Control[] { lblTitle, _lblValue, lblUnit });
            Paint += OnPaint;
        }

        public void SetValue(string v) { _lblValue.Text = v; }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(_accent);
            g.FillRectangle(b, new Rectangle(0, 0, 5, Height));
            using var pen = new Pen(Color.FromArgb(230, 230, 235));
            MainForm.DrawRoundRect(g, pen, new Rectangle(0, 0, Width - 1, Height - 1), 6);
        }

        private void SetDoubleBuffered(bool v) =>
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?.SetValue(this, v);
    }

    // ════════════════════════════════════════════════════════════
    // NavButton
    // ════════════════════════════════════════════════════════════
    internal class NavButton : Panel
    {
        private bool _active;
        private readonly Label _lbl;

        public new event EventHandler? Click;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                _lbl.ForeColor = value ? Color.White : Color.FromArgb(180, 190, 210);
                Invalidate();
            }
        }

        public NavButton(string text)
        {
            // Kích thước cố định cho NavButton — WinForms AutoScaleMode=Dpi sẽ scale
            Size = new Size(200, 42);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;

            _lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(180, 190, 210),
                Dock = DockStyle.Fill,
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
            if (!_active) return;
            using var b = new SolidBrush(Color.FromArgb(50, 70, 100));
            e.Graphics.FillRectangle(b, ClientRectangle);
            using var accent = new SolidBrush(MainForm.ColAccent);
            e.Graphics.FillRectangle(accent, new Rectangle(0, 0, 4, Height));
        }
    }

    // ════════════════════════════════════════════════════════════
    // ChartPanel — owner-draw line chart
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
            SetDoubleBuffered(true);
        }

        public void SetData(List<(string, double)> data) { _data = data; Invalidate(); }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var borderPen = new Pen(Color.FromArgb(220, 220, 225));
            MainForm.DrawRoundRect(g, borderPen, new Rectangle(0, 0, Width - 1, Height - 1), 8);

            using var tf = new Font("Segoe UI", 9f, FontStyle.Bold);
            g.DrawString(Title, tf, Brushes.DimGray, new PointF(12, 10));

            if (_data.Count < 2)
            {
                g.DrawString("Chưa có dữ liệu", new Font("Segoe UI", 9f),
                    Brushes.LightGray, new PointF(Width / 2f - 50, Height / 2f));
                return;
            }

            int pL = 50, pR = 20, pT = 35, pB = 30;
            int cW = Width - pL - pR;
            int cH = Height - pT - pB;

            double maxV = _data.Max(d => d.value);
            double minV = _data.Min(d => d.value);
            if (maxV == minV) maxV = minV + 1;

            using var gridPen = new Pen(Color.FromArgb(240, 240, 240));
            for (int gi = 0; gi <= 4; gi++)
            {
                int gy = pT + (int)(cH * gi / 4.0);
                g.DrawLine(gridPen, pL, gy, pL + cW, gy);
                double gVal = maxV - (maxV - minV) * gi / 4.0;
                g.DrawString(gVal.ToString("N0"), new Font("Segoe UI", 7f),
                    Brushes.LightGray, new PointF(2, gy - 7));
            }

            var pts = new PointF[_data.Count];
            for (int i = 0; i < _data.Count; i++)
            {
                float px = pL + i * cW / (float)(_data.Count - 1);
                float py = pT + (float)((maxV - _data[i].value) / (maxV - minV) * cH);
                pts[i] = new PointF(px, py);
            }

            using var lp = new Pen(_lineColor, 2f) { LineJoin = LineJoin.Round };
            g.DrawLines(lp, pts);

            var fp = new GraphicsPath();
            fp.AddLines(pts);
            fp.AddLine(pts[^1].X, pT + cH, pL, pT + cH);
            fp.CloseFigure();
            using var fb = new LinearGradientBrush(
                new PointF(0, pT), new PointF(0, pT + cH),
                Color.FromArgb(60, _lineColor), Color.FromArgb(5, _lineColor));
            g.FillPath(fb, fp);

            using var db = new SolidBrush(_lineColor);
            foreach (var pt in pts) g.FillEllipse(db, pt.X - 3, pt.Y - 3, 6, 6);
        }

        private void SetDoubleBuffered(bool v) =>
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?.SetValue(this, v);
    }
}
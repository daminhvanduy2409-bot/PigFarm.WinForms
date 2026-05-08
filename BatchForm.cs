using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace PigFarm.WinForms
{
    public class BatchForm : Form
    {
        private TextBox txtCode = new(), txtCount = new(), txtWeight = new(), txtStage = new(), txtNote = new();
        private DateTimePicker dtpImport = new();
        private ComboBox cboStatus = new();
        private DataGridView dgv = new();
        private Button btnSave = new(), btnRefresh = new();

        public BatchForm()
        {
            Text = "Quản Lý Batch";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadList();
        }

        private void BuildUI()
        {
            var pnl = new Panel { Location = new Point(0, 0), Size = new Size(380, 580), BackColor = Color.AliceBlue };

            int y = 15;
            pnl.Controls.Add(MakeLabel("Mã Batch:", 10, y));
            txtCode = MakeTxt(120, y, 230); pnl.Controls.Add(txtCode); y += 35;

            pnl.Controls.Add(MakeLabel("Ngày Vào:", 10, y));
            dtpImport = new DateTimePicker { Location = new Point(120, y), Width = 230, Format = DateTimePickerFormat.Short };
            pnl.Controls.Add(dtpImport); y += 35;

            pnl.Controls.Add(MakeLabel("SL Con Vào:", 10, y));
            txtCount = MakeTxt(120, y, 230); pnl.Controls.Add(txtCount); y += 35;

            pnl.Controls.Add(MakeLabel("TLTB/Con (kg):", 10, y));
            txtWeight = MakeTxt(120, y, 230); pnl.Controls.Add(txtWeight); y += 35;

            pnl.Controls.Add(MakeLabel("Giai Đoạn:", 10, y));
            txtStage = MakeTxt(120, y, 230); txtStage.Text = "Thịt"; pnl.Controls.Add(txtStage); y += 35;

            pnl.Controls.Add(MakeLabel("Trạng Thái:", 10, y));
            cboStatus = new ComboBox { Location = new Point(120, y), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new[] { "Active", "Closed" });
            cboStatus.SelectedIndex = 0;
            pnl.Controls.Add(cboStatus); y += 35;

            pnl.Controls.Add(MakeLabel("Ghi Chú:", 10, y));
            txtNote = new TextBox { Location = new Point(120, y), Width = 230, Height = 60, Multiline = true };
            pnl.Controls.Add(txtNote); y += 75;

            btnSave = new Button
            {
                Text = "💾 Lưu Batch",
                Location = new Point(10, y),
                Size = new Size(340, 38),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;
            pnl.Controls.Add(btnSave);

            dgv = new DataGridView
            {
                Location = new Point(390, 10),
                Size = new Size(480, 540),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            dgv.CellDoubleClick += Dgv_DoubleClick;

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(390, 555),
                Size = new Size(100, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadList();

            Controls.AddRange(new Control[] { pnl, dgv, btnRefresh });
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtCount.Text) || string.IsNullOrWhiteSpace(txtWeight.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc.", "Thiếu thông tin");
                return;
            }
            if (!int.TryParse(txtCount.Text, out int count) || !decimal.TryParse(txtWeight.Text, out decimal weight))
            {
                MessageBox.Show("SL con và TLTB phải là số.", "Lỗi nhập liệu");
                return;
            }
            try
            {
                DatabaseHelper.Execute(
                    @"INSERT INTO Batch(BatchCode,ImportDate,InitialCount,InitialAvgWeight,Stage,Status,Note)
                      VALUES(@code,@date,@cnt,@wt,@stage,@status,@note)",
                    DatabaseHelper.P("@code", txtCode.Text.Trim()),
                    DatabaseHelper.P("@date", dtpImport.Value.Date),
                    DatabaseHelper.P("@cnt", count),
                    DatabaseHelper.P("@wt", weight),
                    DatabaseHelper.P("@stage", txtStage.Text.Trim()),
                    DatabaseHelper.P("@status", cboStatus.SelectedItem!.ToString()),
                    DatabaseHelper.P("@note", txtNote.Text.Trim()));

                MessageBox.Show("Đã lưu batch thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCode.Clear(); txtCount.Clear(); txtWeight.Clear(); txtNote.Clear();
                LoadList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Dgv_DoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            txtCode.Text = row.Cells["BatchCode"].Value?.ToString();
            txtCount.Text = row.Cells["InitialCount"].Value?.ToString();
            txtWeight.Text = row.Cells["InitialAvgWeight"].Value?.ToString();
            txtStage.Text = row.Cells["Stage"].Value?.ToString();
            txtNote.Text = row.Cells["Note"].Value?.ToString();
        }

        private void LoadList()
        {
            dgv.DataSource = DatabaseHelper.Query(
                "SELECT BatchID, BatchCode, CONVERT(VARCHAR,ImportDate,103) AS ImportDate, InitialCount, InitialAvgWeight, Stage, Status, Note FROM Batch ORDER BY ImportDate DESC");
        }

        private Label MakeLabel(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Arial", 9) };
        private TextBox MakeTxt(int x, int y, int w) => new TextBox { Location = new Point(x, y), Width = w };
    }
}

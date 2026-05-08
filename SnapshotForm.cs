using System;
using System.Drawing;
using System.Windows.Forms;

namespace PigFarm.WinForms
{
    public class SnapshotForm : Form
    {
        private ComboBox cboBatch = new();
        private DateTimePicker dtpDate = new();
        private TextBox txtCount = new(), txtAvgWeight = new(), txtNote = new();
        private DataGridView dgv = new();
        private Button btnSave = new();

        public SnapshotForm()
        {
            Text = "Cân Định Kỳ - Snapshot";
            Size = new Size(900, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadBatches();
        }

        private void BuildUI()
        {
            var pnl = new Panel { Location = new Point(0, 0), Size = new Size(380, 510), BackColor = Color.LightCyan };
            int y = 15;

            pnl.Controls.Add(MakeLabel("Batch:", 10, y));
            cboBatch = new ComboBox { Location = new Point(140, y), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cboBatch.SelectedIndexChanged += (s, e) => LoadHistory();
            pnl.Controls.Add(cboBatch); y += 35;

            pnl.Controls.Add(MakeLabel("Ngày Cân:", 10, y));
            dtpDate = new DateTimePicker { Location = new Point(140, y), Width = 220, Format = DateTimePickerFormat.Short };
            pnl.Controls.Add(dtpDate); y += 35;

            pnl.Controls.Add(MakeLabel("Số Con Hiện Tại:", 10, y));
            txtCount = new TextBox { Location = new Point(140, y), Width = 220 };
            pnl.Controls.Add(txtCount); y += 35;

            pnl.Controls.Add(MakeLabel("TLTB/Con (kg):", 10, y));
            txtAvgWeight = new TextBox { Location = new Point(140, y), Width = 220 };
            pnl.Controls.Add(txtAvgWeight); y += 35;

            pnl.Controls.Add(MakeLabel("Ghi Chú:", 10, y));
            txtNote = new TextBox { Location = new Point(140, y), Width = 220, Height = 60, Multiline = true };
            pnl.Controls.Add(txtNote); y += 75;

            btnSave = new Button
            {
                Text = "💾 Lưu Snapshot",
                Location = new Point(10, y),
                Size = new Size(340, 38),
                BackColor = Color.MediumPurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;
            pnl.Controls.Add(btnSave);

            dgv = new DataGridView
            {
                Location = new Point(390, 10),
                Size = new Size(480, 490),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };

            Controls.AddRange(new Control[] { pnl, dgv });
        }

        private void LoadBatches()
        {
            var batches = DatabaseHelper.Query("SELECT BatchID, BatchCode FROM Batch ORDER BY BatchCode");
            cboBatch.DataSource = batches;
            cboBatch.DisplayMember = "BatchCode";
            cboBatch.ValueMember = "BatchID";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboBatch.SelectedValue == null) { MessageBox.Show("Chọn batch."); return; }
            if (!int.TryParse(txtCount.Text, out int cnt) || cnt < 0)
            { MessageBox.Show("Số con phải là số không âm."); return; }
            if (!decimal.TryParse(txtAvgWeight.Text, out decimal wt) || wt <= 0)
            { MessageBox.Show("TLTB phải là số dương."); return; }

            try
            {
                DatabaseHelper.Execute(
                    "INSERT INTO BatchSnapshot(SnapshotDate,BatchID,CurrentCount,AvgWeight,Note) VALUES(@d,@b,@c,@w,@n)",
                    DatabaseHelper.P("@d", dtpDate.Value.Date),
                    DatabaseHelper.P("@b", cboBatch.SelectedValue!),
                    DatabaseHelper.P("@c", cnt),
                    DatabaseHelper.P("@w", wt),
                    DatabaseHelper.P("@n", txtNote.Text.Trim()));

                txtCount.Clear(); txtAvgWeight.Clear(); txtNote.Clear();
                LoadHistory();
                MessageBox.Show("Đã lưu snapshot!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void LoadHistory()
        {
            if (cboBatch.SelectedValue == null) return;
            dgv.DataSource = DatabaseHelper.Query(
                @"SELECT CONVERT(VARCHAR,SnapshotDate,103) AS [Ngày Cân],
                         CurrentCount AS [Số Con],
                         AvgWeight AS [TLTB (kg)],
                         CurrentCount * AvgWeight AS [Tổng KL (kg)],
                         Note AS [Ghi Chú]
                  FROM BatchSnapshot
                  WHERE BatchID=@b
                  ORDER BY SnapshotDate DESC",
                DatabaseHelper.P("@b", cboBatch.SelectedValue!));
        }

        private Label MakeLabel(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Arial", 9) };
    }
}

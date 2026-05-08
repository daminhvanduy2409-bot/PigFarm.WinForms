using System;
using System.Drawing;
using System.Windows.Forms;

namespace PigFarm.WinForms
{
    public class MovementForm : Form
    {
        private ComboBox cboBatch = new(), cboMovType = new();
        private DateTimePicker dtpDate = new();
        private TextBox txtQty = new(), txtWeight = new(), txtNote = new();
        private DataGridView dgv = new();
        private Button btnSave = new();

        public MovementForm()
        {
            Text = "Biến Động Đàn";
            Size = new Size(900, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadComboBoxes();
        }

        private void BuildUI()
        {
            var pnl = new Panel { Location = new Point(0, 0), Size = new Size(380, 560), BackColor = Color.LavenderBlush };
            int y = 15;

            pnl.Controls.Add(MakeLabel("Batch:", 10, y));
            cboBatch = new ComboBox { Location = new Point(140, y), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cboBatch.SelectedIndexChanged += (s, e) => LoadHistory();
            pnl.Controls.Add(cboBatch); y += 35;

            pnl.Controls.Add(MakeLabel("Loại Biến Động:", 10, y));
            cboMovType = new ComboBox { Location = new Point(140, y), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            pnl.Controls.Add(cboMovType); y += 35;

            pnl.Controls.Add(MakeLabel("Ngày:", 10, y));
            dtpDate = new DateTimePicker { Location = new Point(140, y), Width = 220, Format = DateTimePickerFormat.Short };
            pnl.Controls.Add(dtpDate); y += 35;

            pnl.Controls.Add(MakeLabel("Số Con:", 10, y));
            txtQty = new TextBox { Location = new Point(140, y), Width = 220 };
            pnl.Controls.Add(txtQty); y += 35;

            pnl.Controls.Add(MakeLabel("Tổng Kg:", 10, y));
            txtWeight = new TextBox { Location = new Point(140, y), Width = 220 };
            pnl.Controls.Add(txtWeight); y += 35;

            pnl.Controls.Add(MakeLabel("Ghi Chú:", 10, y));
            txtNote = new TextBox { Location = new Point(140, y), Width = 220, Height = 60, Multiline = true };
            pnl.Controls.Add(txtNote); y += 75;

            btnSave = new Button
            {
                Text = "💾 Lưu",
                Location = new Point(10, y),
                Size = new Size(340, 38),
                BackColor = Color.DarkOrange,
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

            Controls.AddRange(new Control[] { pnl, dgv });
        }

        private void LoadComboBoxes()
        {
            var batches = DatabaseHelper.Query("SELECT BatchID, BatchCode FROM Batch ORDER BY BatchCode");
            cboBatch.DataSource = batches;
            cboBatch.DisplayMember = "BatchCode";
            cboBatch.ValueMember = "BatchID";

            var types = DatabaseHelper.Query("SELECT MovementTypeID, TypeName FROM MovementType ORDER BY MovementTypeID");
            cboMovType.DataSource = types;
            cboMovType.DisplayMember = "TypeName";
            cboMovType.ValueMember = "MovementTypeID";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboBatch.SelectedValue == null || cboMovType.SelectedValue == null)
            { MessageBox.Show("Chọn batch và loại biến động."); return; }
            if (!int.TryParse(txtQty.Text, out int qty) || qty <= 0)
            { MessageBox.Show("Số con phải là số nguyên dương."); return; }
            if (!decimal.TryParse(txtWeight.Text, out decimal wt) || wt < 0)
            { MessageBox.Show("Tổng kg phải là số không âm."); return; }

            try
            {
                DatabaseHelper.Execute(
                    "INSERT INTO PigMovement(MovementDate,BatchID,MovementTypeID,Quantity,TotalWeightKg,Note) VALUES(@d,@b,@m,@q,@w,@n)",
                    DatabaseHelper.P("@d", dtpDate.Value.Date),
                    DatabaseHelper.P("@b", cboBatch.SelectedValue!),
                    DatabaseHelper.P("@m", cboMovType.SelectedValue!),
                    DatabaseHelper.P("@q", qty),
                    DatabaseHelper.P("@w", wt),
                    DatabaseHelper.P("@n", txtNote.Text.Trim()));

                txtQty.Clear(); txtWeight.Clear(); txtNote.Clear();
                LoadHistory();
                MessageBox.Show("Đã lưu!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void LoadHistory()
        {
            if (cboBatch.SelectedValue == null) return;
            dgv.DataSource = DatabaseHelper.Query(
                @"SELECT CONVERT(VARCHAR,pm.MovementDate,103) AS [Ngày],
                         mt.TypeName AS [Loại],
                         pm.Quantity AS [Số Con],
                         pm.TotalWeightKg AS [Tổng Kg],
                         pm.Note AS [Ghi Chú]
                  FROM PigMovement pm
                  JOIN MovementType mt ON pm.MovementTypeID=mt.MovementTypeID
                  WHERE pm.BatchID=@b
                  ORDER BY pm.MovementDate DESC",
                DatabaseHelper.P("@b", cboBatch.SelectedValue!));
        }

        private Label MakeLabel(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Arial", 9) };
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PigFarm.WinForms
{
    public class FeedEntryForm : Form
    {
        private ComboBox cboBatch = new(), cboFeedType = new();
        private DateTimePicker dtpDate = new();
        private TextBox txtQty = new(), txtNote = new();
        private DataGridView dgv = new();
        private Button btnSave = new();

        public FeedEntryForm()
        {
            Text = "Nhập Cám Hằng Ngày";
            Size = new Size(900, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadComboBoxes();
        }

        private void BuildUI()
        {
            var pnl = new Panel { Location = new Point(0, 0), Size = new Size(380, 560), BackColor = Color.Honeydew };
            int y = 15;

            pnl.Controls.Add(MakeLabel("Batch:", 10, y));
            cboBatch = new ComboBox { Location = new Point(130, y), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cboBatch.SelectedIndexChanged += (s, e) => LoadHistory();
            pnl.Controls.Add(cboBatch); y += 35;

            pnl.Controls.Add(MakeLabel("Mã Cám:", 10, y));
            cboFeedType = new ComboBox { Location = new Point(130, y), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            pnl.Controls.Add(cboFeedType); y += 35;

            pnl.Controls.Add(MakeLabel("Ngày:", 10, y));
            dtpDate = new DateTimePicker { Location = new Point(130, y), Width = 230, Format = DateTimePickerFormat.Short };
            pnl.Controls.Add(dtpDate); y += 35;

            pnl.Controls.Add(MakeLabel("Số Kg:", 10, y));
            txtQty = new TextBox { Location = new Point(130, y), Width = 230 };
            pnl.Controls.Add(txtQty); y += 35;

            pnl.Controls.Add(MakeLabel("Ghi Chú:", 10, y));
            txtNote = new TextBox { Location = new Point(130, y), Width = 230, Height = 60, Multiline = true };
            pnl.Controls.Add(txtNote); y += 75;

            btnSave = new Button
            {
                Text = "💾 Lưu",
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

            Controls.AddRange(new Control[] { pnl, dgv });
        }

        private void LoadComboBoxes()
        {
            var batches = DatabaseHelper.Query("SELECT BatchID, BatchCode FROM Batch WHERE Status='Active' ORDER BY BatchCode");
            cboBatch.DataSource = batches;
            cboBatch.DisplayMember = "BatchCode";
            cboBatch.ValueMember = "BatchID";

            var feeds = DatabaseHelper.Query("SELECT FeedTypeID, FeedCode + ' - ' + FeedName AS Label FROM FeedType ORDER BY FeedCode");
            cboFeedType.DataSource = feeds;
            cboFeedType.DisplayMember = "Label";
            cboFeedType.ValueMember = "FeedTypeID";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboBatch.SelectedValue == null || cboFeedType.SelectedValue == null)
            { MessageBox.Show("Chọn batch và loại cám."); return; }
            if (!decimal.TryParse(txtQty.Text, out decimal qty) || qty <= 0)
            { MessageBox.Show("Số kg phải là số dương."); return; }

            try
            {
                DatabaseHelper.Execute(
                    "INSERT INTO FeedDaily(FeedDate,BatchID,FeedTypeID,QuantityKg,Note) VALUES(@d,@b,@f,@q,@n)",
                    DatabaseHelper.P("@d", dtpDate.Value.Date),
                    DatabaseHelper.P("@b", cboBatch.SelectedValue!),
                    DatabaseHelper.P("@f", cboFeedType.SelectedValue!),
                    DatabaseHelper.P("@q", qty),
                    DatabaseHelper.P("@n", txtNote.Text.Trim()));

                txtQty.Clear(); txtNote.Clear();
                LoadHistory();
                MessageBox.Show("Đã lưu!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void LoadHistory()
        {
            if (cboBatch.SelectedValue == null) return;
            dgv.DataSource = DatabaseHelper.Query(
                @"SELECT CONVERT(VARCHAR,fd.FeedDate,103) AS [Ngày],
                         ft.FeedCode AS [Mã Cám], ft.FeedName AS [Tên Cám],
                         fd.QuantityKg AS [Số Kg], fd.Note AS [Ghi Chú]
                  FROM FeedDaily fd
                  JOIN FeedType ft ON fd.FeedTypeID=ft.FeedTypeID
                  WHERE fd.BatchID=@b
                  ORDER BY fd.FeedDate DESC",
                DatabaseHelper.P("@b", cboBatch.SelectedValue!));
        }

        private Label MakeLabel(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Arial", 9) };
    }
}

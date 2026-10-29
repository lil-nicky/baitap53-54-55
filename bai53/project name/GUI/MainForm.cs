using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CafePOSSystem.BUS;

namespace CafePOSSystem.GUI
{
    public class MainForm : Form
    {
        private readonly CafeBUS bus = new CafeBUS();
        private readonly FlowLayoutPanel tablesPanel = new FlowLayoutPanel();
        private readonly ComboBox categories = new ComboBox();
        private readonly ComboBox menu = new ComboBox();
        private readonly NumericUpDown quantity = new NumericUpDown();
        private readonly DataGridView orderGrid = new DataGridView();
        private readonly Label selectedLabel = new Label();
        private int selectedTableId = 0;

        public MainForm()
        {
            Text = "Cafe POS System";
            Width = 1100; Height = 700; StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            Load += (s, e) => { LoadCategories(); LoadTables(); };
        }

        private void BuildUI()
        {
            var left = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(10) };
            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            tablesPanel.Dock = DockStyle.Fill;
            tablesPanel.AutoScroll = true;
            left.Controls.Add(new Label { Text = "SƠ ĐỒ BÀN", Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI", 13, FontStyle.Bold) });
            left.Controls.Add(tablesPanel);

            selectedLabel.Text = "Chưa chọn bàn";
            selectedLabel.Dock = DockStyle.Top; selectedLabel.Height = 32;
            selectedLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            categories.DropDownStyle = ComboBoxStyle.DropDownList;
            categories.Dock = DockStyle.Top; categories.Height = 32;
            categories.SelectedIndexChanged += (s, e) => LoadMenu();
            menu.DropDownStyle = ComboBoxStyle.DropDownList;
            menu.Dock = DockStyle.Top; menu.Height = 32;
            quantity.Minimum = 1; quantity.Maximum = 999; quantity.Value = 1;
            quantity.Dock = DockStyle.Top;

            var add = new Button { Text = "Thêm món", Dock = DockStyle.Top, Height = 38 };
            add.Click += (s, e) => AddItem();
            var pay = new Button { Text = "Thanh toán", Dock = DockStyle.Top, Height = 38 };
            pay.Click += (s, e) => Pay();
            orderGrid.Dock = DockStyle.Fill; orderGrid.ReadOnly = true;
            orderGrid.AllowUserToAddRows = false; orderGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            orderGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            right.Controls.Add(orderGrid);
            right.Controls.Add(pay);
            right.Controls.Add(add);
            right.Controls.Add(quantity);
            right.Controls.Add(menu);
            right.Controls.Add(categories);
            right.Controls.Add(selectedLabel);
            Controls.Add(right); Controls.Add(left);
        }

        private void LoadCategories()
        {
            categories.DataSource = bus.GetCategories();
            categories.DisplayMember = "TenDanhMuc";
            categories.ValueMember = "MaDanhMuc";
            if (categories.Items.Count > 0) categories.SelectedIndex = 0;
        }

        private void LoadMenu()
        {
            if (categories.SelectedValue == null || !(categories.SelectedValue is int)) return;
            menu.DataSource = bus.GetMenu((int)categories.SelectedValue);
            menu.DisplayMember = "TenMon"; menu.ValueMember = "MaMon";
        }

        private void LoadTables()
        {
            tablesPanel.Controls.Clear();
            DataTable dt = bus.GetTables();
            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["MaBan"]);
                string name = row["TenBan"].ToString();
                string status = row["TrangThai"].ToString();
                var btn = new Button
                {
                    Text = name + Environment.NewLine + status,
                    Width = 125, Height = 75, Tag = id,
                    BackColor = status == "Trống" ? Color.LightGreen : Color.LightCoral,
                    Margin = new Padding(5)
                };
                btn.Click += (s, e) =>
                {
                    selectedTableId = (int)((Button)s).Tag;
                    selectedLabel.Text = "Đang chọn: " + name;
                    LoadOrder();
                };
                tablesPanel.Controls.Add(btn);
            }
        }

        private void LoadOrder()
        {
            orderGrid.DataSource = bus.GetOrder(selectedTableId);
            if (orderGrid.Columns["DonGia"] != null) orderGrid.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            if (orderGrid.Columns["ThanhTien"] != null) orderGrid.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
        }

        private void AddItem()
        {
            try
            {
                if (selectedTableId == 0 || menu.SelectedValue == null) throw new InvalidOperationException("Hãy chọn bàn và món ăn.");
                bus.AddItem(selectedTableId, Convert.ToInt32(menu.SelectedValue), (int)quantity.Value);
                LoadTables(); LoadOrder();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể thêm món", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void Pay()
        {
            try
            {
                if (selectedTableId == 0) throw new InvalidOperationException("Hãy chọn bàn.");
                decimal total = bus.Pay(selectedTableId);
                MessageBox.Show("Thanh toán thành công: " + total.ToString("N0") + " VNĐ", "Hóa đơn");
                LoadTables(); LoadOrder();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}

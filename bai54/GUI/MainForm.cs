
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelManagement.BUS;
using HotelManagement.DTO;

namespace HotelManagement.GUI
{
    public partial class MainForm : Form
    {
        private readonly PhongBUS phongBUS = new PhongBUS();
        private readonly HotelBUS hotelBUS = new HotelBUS();

        private readonly DataGridView grid = new DataGridView();
        private readonly ComboBox cboLoai = new ComboBox();
        private readonly ComboBox cboTrangThai = new ComboBox();

        private DataTable rooms;

        public MainForm()
        {
            Text = "HOTEL MANAGEMENT SYSTEM";
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(8),
                AutoSize = false
            };

            cboLoai.Width = 130;
            cboTrangThai.Width = 140;
            cboLoai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;

            cboLoai.Items.AddRange(new object[]
                { "Tất cả", "Đơn", "Đôi", "VIP" });
            cboTrangThai.Items.AddRange(new object[]
                { "Tất cả", "Trống", "Đang ở", "Đang dọn dẹp" });

            cboLoai.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 0;

            var btnTai = new Button { Text = "Tải phòng", Width = 100 };
            var btnCheckIn = new Button { Text = "Check-in", Width = 100 };
            var btnDichVu = new Button { Text = "Gọi dịch vụ", Width = 110 };
            var btnCheckOut = new Button { Text = "Check-out", Width = 110 };

            top.Controls.Add(new Label
            {
                Text = "Loại phòng:",
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0)
            });
            top.Controls.Add(cboLoai);
            top.Controls.Add(new Label
            {
                Text = "Trạng thái:",
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0)
            });
            top.Controls.Add(cboTrangThai);
            top.Controls.Add(btnTai);
            top.Controls.Add(btnCheckIn);
            top.Controls.Add(btnDichVu);
            top.Controls.Add(btnCheckOut);

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grid.AllowUserToAddRows = false;

            Controls.Add(grid);
            Controls.Add(top);

            btnTai.Click += (s, e) => LoadRooms();
            cboLoai.SelectedIndexChanged += (s, e) => ApplyFilter();
            cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();

            btnCheckIn.Click += (s, e) => DoCheckIn();
            btnDichVu.Click += (s, e) => DoService();
            btnCheckOut.Click += (s, e) => DoCheckOut();

            Load += (s, e) => LoadRooms();
        }

        private void LoadRooms()
        {
            try
            {
                rooms = phongBUS.GetAll();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải phòng: " + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            if (rooms == null) return;

            grid.DataSource = phongBUS.Filter(
                rooms,
                cboLoai.SelectedItem?.ToString(),
                cboTrangThai.SelectedItem?.ToString());
        }

        private string SelectedRoom()
        {
            if (grid.CurrentRow == null)
                throw new Exception("Hãy chọn một phòng.");

            return Convert.ToString(
                grid.CurrentRow.Cells["SoPhong"].Value);
        }

        private void DoCheckIn()
        {
            try
            {
                string soPhong = SelectedRoom();

                string hoTen = Ask("Họ tên khách:");
                if (hoTen == null) return;

                string cccd = Ask("CCCD:");
                if (cccd == null) return;

                string sdt = Ask("Số điện thoại (có thể bỏ trống):");
                if (sdt == null) return;

                var kh = new KhachHangDTO
                {
                    HoTen = hoTen,
                    CCCD = cccd,
                    SoDienThoai = sdt
                };

                int maPhieu = hotelBUS.CheckIn(kh, soPhong);

                MessageBox.Show(
                    "Check-in thành công. Mã phiếu: " + maPhieu);

                LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi check-in: " + ex.Message);
            }
        }

        private void DoService()
        {
            try
            {
                string soPhong = SelectedRoom();

                string maPhieuText = Ask(
                    "Nhập mã phiếu đang ở của phòng " + soPhong + ":");
                if (maPhieuText == null) return;

                string maDVText = Ask("Nhập mã dịch vụ (MaDV):");
                if (maDVText == null) return;

                string soLuongText = Ask("Nhập số lượng:");
                if (soLuongText == null) return;

                int maPhieu = int.Parse(maPhieuText);
                int maDV = int.Parse(maDVText);
                int soLuong = int.Parse(soLuongText);

                hotelBUS.AddService(maPhieu, maDV, soLuong);

                MessageBox.Show("Đã lưu dịch vụ.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gọi dịch vụ: " + ex.Message);
            }
        }

        private void DoCheckOut()
        {
            try
            {
                string soPhong = SelectedRoom();

                string text = Ask(
                    "Nhập mã phiếu đang ở của phòng " + soPhong + ":");
                if (text == null) return;

                int maPhieu = int.Parse(text);

                if (MessageBox.Show(
                    "Xác nhận checkout phòng " + soPhong + "?",
                    "Xác nhận thanh toán",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                decimal tongTien = hotelBUS.CheckOut(maPhieu);

                MessageBox.Show(
                    "CHECK-OUT THÀNH CÔNG\nTổng thanh toán: " +
                    tongTien.ToString("N0") + " VNĐ");

                LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi check-out: " + ex.Message);
            }
        }

        private string Ask(string message)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Nhập thông tin";
                dialog.Width = 400;
                dialog.Height = 160;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;

                var label = new Label
                {
                    Text = message,
                    Left = 10,
                    Top = 10,
                    Width = 360
                };

                var input = new TextBox
                {
                    Left = 10,
                    Top = 35,
                    Width = 360
                };

                var ok = new Button
                {
                    Text = "OK",
                    Left = 200,
                    Top = 70,
                    DialogResult = DialogResult.OK
                };

                var cancel = new Button
                {
                    Text = "Hủy",
                    Left = 285,
                    Top = 70,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(input);
                dialog.Controls.Add(ok);
                dialog.Controls.Add(cancel);
                dialog.AcceptButton = ok;
                dialog.CancelButton = cancel;

                return dialog.ShowDialog(this) == DialogResult.OK
                    ? input.Text : null;
            }
        }
    }
}

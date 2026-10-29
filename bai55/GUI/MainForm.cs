
using System;
using System.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagement.BUS;
using InventoryManagement.DTO;

namespace InventoryManagement.GUI
{
    public partial class MainForm : Form
    {
        private readonly VatTuBUS vatTuBUS = new VatTuBUS();
        private readonly NhaCungCapBUS nccBUS = new NhaCungCapBUS();
        private readonly KhoBUS khoBUS = new KhoBUS();

        // Tab vật tư
        private DataGridView dgvVatTu = new DataGridView();
        private TextBox txtMaVT = new TextBox();
        private TextBox txtTenVT = new TextBox();
        private TextBox txtDVT = new TextBox();
        private TextBox txtTimVT = new TextBox();
        private NumericUpDown numTonBanDau = new NumericUpDown();

        // Tab nhà cung cấp
        private DataGridView dgvNCC = new DataGridView();
        private TextBox txtTenNCC = new TextBox();
        private TextBox txtSDTNCC = new TextBox();
        private TextBox txtDiaChiNCC = new TextBox();
        private TextBox txtTimNCC = new TextBox();
        private int maNCCDangChon = 0;

        // Tab nhập xuất
        private ComboBox cboLoaiPhieu = new ComboBox();
        private ComboBox cboNCC = new ComboBox();
        private ComboBox cboVatTu = new ComboBox();
        private NumericUpDown numSoLuong = new NumericUpDown();
        private NumericUpDown numDonGia = new NumericUpDown();
        private DataGridView dgvChiTiet = new DataGridView();
        private readonly List<ChiTietPhieuDTO> chiTiet =
            new List<ChiTietPhieuDTO>();

        // Tab báo cáo
        private NumericUpDown numNguong = new NumericUpDown();
        private DataGridView dgvCanhBao = new DataGridView();
        private Label lblTongGiaTri = new Label();

        private TabControl tabs = new TabControl();

        public MainForm()
        {
            Text = "HỆ THỐNG QUẢN LÝ NHẬP - XUẤT KHO";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1150, 760);
            MinimumSize = new Size(950, 650);
            Font = new Font("Segoe UI", 9F);

            TaoGiaoDien();
            Load += (s, e) => NapTatCaDuLieu();
        }

        private Label TaoLabel(
            Control parent, string text, int x, int y, int width = 100)
        {
            Label l = new Label();
            l.Text = text;
            l.SetBounds(x, y, width, 25);
            l.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(l);
            return l;
        }

        private TextBox TaoTextBox(
            Control parent, int x, int y, int width = 150)
        {
            TextBox t = new TextBox();
            t.SetBounds(x, y, width, 27);
            parent.Controls.Add(t);
            return t;
        }

        private Button TaoButton(
            Control parent, string text, int x, int y,
            int width, Action action)
        {
            Button b = new Button();
            b.Text = text;
            b.SetBounds(x, y, width, 32);
            b.Click += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            };
            parent.Controls.Add(b);
            return b;
        }

        private DataGridView TaoGrid(Control parent,
            int x, int y, int width, int height)
        {
            DataGridView dgv = new DataGridView();
            dgv.SetBounds(x, y, width, height);
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            parent.Controls.Add(dgv);
            return dgv;
        }

        private NumericUpDown TaoSo(
            Control parent, int x, int y, int width,
            decimal min, decimal max, decimal value)
        {
            NumericUpDown n = new NumericUpDown();
            n.SetBounds(x, y, width, 27);
            n.Minimum = min;
            n.Maximum = max;
            n.DecimalPlaces = 2;
            n.Increment = 1;
            n.Value = value;
            parent.Controls.Add(n);
            return n;
        }

        private void TaoGiaoDien()
        {
            tabs.Dock = DockStyle.Fill;
            Controls.Add(tabs);

            TabPage tabVT = new TabPage("Quản lý vật tư");
            TabPage tabNCC = new TabPage("Nhà cung cấp");
            TabPage tabKho = new TabPage("Phiếu nhập / xuất");
            TabPage tabBC = new TabPage("Cảnh báo & báo cáo");

            tabs.TabPages.Add(tabVT);
            tabs.TabPages.Add(tabNCC);
            tabs.TabPages.Add(tabKho);
            tabs.TabPages.Add(tabBC);

            TaoTabVatTu(tabVT);
            TaoTabNCC(tabNCC);
            TaoTabKho(tabKho);
            TaoTabBaoCao(tabBC);
        }

        // =========================
        // TAB 1: QUẢN LÝ VẬT TƯ
        // =========================

        private void TaoTabVatTu(TabPage tab)
        {
            TaoLabel(tab, "Mã vật tư", 15, 15);
            txtMaVT = TaoTextBox(tab, 110, 15, 160);

            TaoLabel(tab, "Tên vật tư", 290, 15);
            txtTenVT = TaoTextBox(tab, 380, 15, 220);

            TaoLabel(tab, "Đơn vị tính", 620, 15, 85);
            txtDVT = TaoTextBox(tab, 710, 15, 120);

            TaoLabel(tab, "Tồn ban đầu", 15, 55, 90);
            numTonBanDau = TaoSo(
                tab, 110, 55, 160, 0, 1000000000, 0);

            TaoButton(tab, "Thêm", 290, 52, 85, ThemVatTu);
            TaoButton(tab, "Sửa", 380, 52, 85, SuaVatTu);
            TaoButton(tab, "Xóa", 470, 52, 85, XoaVatTu);
            TaoButton(tab, "Làm mới", 560, 52, 95, LamMoiVatTu);

            TaoLabel(tab, "Tìm mã / tên", 15, 100, 95);
            txtTimVT = TaoTextBox(tab, 110, 100, 260);

            TaoButton(tab, "Tìm kiếm", 380, 97, 95, TimVatTu);
            TaoButton(tab, "Tải tất cả", 480, 97, 95, NapVatTu);

            dgvVatTu = TaoGrid(tab, 15, 145, 1080, 520);
            dgvVatTu.SelectionChanged += (s, e) => ChonDongVatTu();
        }

        private void NapVatTu()
        {
            dgvVatTu.DataSource = vatTuBUS.GetAll();
        }

        private void ThemVatTu()
        {
            VatTuDTO vt = new VatTuDTO
            {
                MaVatTu = txtMaVT.Text.Trim(),
                TenVatTu = txtTenVT.Text.Trim(),
                DVT = txtDVT.Text.Trim(),
                SoLuongTon = numTonBanDau.Value
            };

            vatTuBUS.Them(vt);
            NapVatTu();
            NapComboVatTu();
            MessageBox.Show("Thêm vật tư thành công.");
        }

        private void SuaVatTu()
        {
            VatTuDTO vt = new VatTuDTO
            {
                MaVatTu = txtMaVT.Text.Trim(),
                TenVatTu = txtTenVT.Text.Trim(),
                DVT = txtDVT.Text.Trim()
            };

            vatTuBUS.Sua(vt);
            NapVatTu();
            NapComboVatTu();
            MessageBox.Show("Cập nhật vật tư thành công.");
        }

        private void XoaVatTu()
        {
            if (string.IsNullOrWhiteSpace(txtMaVT.Text))
                throw new Exception("Hãy chọn vật tư cần xóa.");

            if (MessageBox.Show(
                "Xóa vật tư " + txtMaVT.Text + "?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            vatTuBUS.Xoa(txtMaVT.Text.Trim());
            NapVatTu();
            NapComboVatTu();
            LamMoiVatTu();
        }

        private void TimVatTu()
        {
            dgvVatTu.DataSource = vatTuBUS.GetAll(txtTimVT.Text.Trim());
        }

        private void LamMoiVatTu()
        {
            txtMaVT.Clear();
            txtTenVT.Clear();
            txtDVT.Clear();
            numTonBanDau.Value = 0;
            dgvVatTu.ClearSelection();
            txtMaVT.Focus();
        }

        private void ChonDongVatTu()
        {
            if (dgvVatTu.CurrentRow == null ||
                dgvVatTu.CurrentRow.IsNewRow)
                return;

            DataGridViewRow r = dgvVatTu.CurrentRow;

            if (r.Cells["MaVatTu"].Value == null)
                return;

            txtMaVT.Text = Convert.ToString(r.Cells["MaVatTu"].Value);
            txtTenVT.Text = Convert.ToString(r.Cells["TenVatTu"].Value);
            txtDVT.Text = Convert.ToString(r.Cells["DVT"].Value);
            numTonBanDau.Value = Convert.ToDecimal(
                r.Cells["SoLuongTon"].Value);
        }

        // =========================
        // TAB 2: NHÀ CUNG CẤP
        // =========================

        private void TaoTabNCC(TabPage tab)
        {
            TaoLabel(tab, "Tên nhà cung cấp", 15, 15, 120);
            txtTenNCC = TaoTextBox(tab, 140, 15, 250);

            TaoLabel(tab, "Điện thoại", 410, 15, 90);
            txtSDTNCC = TaoTextBox(tab, 500, 15, 180);

            TaoLabel(tab, "Địa chỉ", 15, 55);
            txtDiaChiNCC = TaoTextBox(tab, 140, 55, 350);

            TaoButton(tab, "Thêm", 510, 52, 85, ThemNCC);
            TaoButton(tab, "Sửa", 600, 52, 85, SuaNCC);
            TaoButton(tab, "Xóa", 690, 52, 85, XoaNCC);
            TaoButton(tab, "Làm mới", 780, 52, 95, LamMoiNCC);

            TaoLabel(tab, "Tìm kiếm", 15, 100);
            txtTimNCC = TaoTextBox(tab, 140, 100, 280);

            TaoButton(tab, "Tìm", 430, 97, 80, TimNCC);
            TaoButton(tab, "Tải tất cả", 515, 97, 95, NapNCC);

            dgvNCC = TaoGrid(tab, 15, 145, 1080, 520);
            dgvNCC.SelectionChanged += (s, e) => ChonDongNCC();
        }

        private void NapNCC()
        {
            dgvNCC.DataSource = nccBUS.GetAll();
        }

        private void ThemNCC()
        {
            NhaCungCapDTO ncc = new NhaCungCapDTO
            {
                TenNCC = txtTenNCC.Text.Trim(),
                DienThoai = txtSDTNCC.Text.Trim(),
                DiaChi = txtDiaChiNCC.Text.Trim()
            };

            nccBUS.Them(ncc);
            NapNCC();
            NapComboNCC();
            MessageBox.Show("Thêm nhà cung cấp thành công.");
        }

        private void SuaNCC()
        {
            if (maNCCDangChon <= 0)
                throw new Exception("Hãy chọn nhà cung cấp cần sửa.");

            NhaCungCapDTO ncc = new NhaCungCapDTO
            {
                MaNCC = maNCCDangChon,
                TenNCC = txtTenNCC.Text.Trim(),
                DienThoai = txtSDTNCC.Text.Trim(),
                DiaChi = txtDiaChiNCC.Text.Trim()
            };

            nccBUS.Sua(ncc);
            NapNCC();
            NapComboNCC();
            MessageBox.Show("Cập nhật nhà cung cấp thành công.");
        }

        private void XoaNCC()
        {
            if (maNCCDangChon <= 0)
                throw new Exception("Hãy chọn nhà cung cấp cần xóa.");

            if (MessageBox.Show(
                "Xóa nhà cung cấp này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            nccBUS.Xoa(maNCCDangChon);
            LamMoiNCC();
            NapNCC();
            NapComboNCC();
        }

        private void TimNCC()
        {
            dgvNCC.DataSource = nccBUS.GetAll(txtTimNCC.Text.Trim());
        }

        private void LamMoiNCC()
        {
            maNCCDangChon = 0;
            txtTenNCC.Clear();
            txtSDTNCC.Clear();
            txtDiaChiNCC.Clear();
            dgvNCC.ClearSelection();
        }

        private void ChonDongNCC()
        {
            if (dgvNCC.CurrentRow == null ||
                dgvNCC.CurrentRow.IsNewRow)
                return;

            DataGridViewRow r = dgvNCC.CurrentRow;

            if (r.Cells["MaNCC"].Value == null)
                return;

            maNCCDangChon = Convert.ToInt32(r.Cells["MaNCC"].Value);
            txtTenNCC.Text = Convert.ToString(r.Cells["TenNCC"].Value);
            txtSDTNCC.Text = Convert.ToString(r.Cells["DienThoai"].Value);
            txtDiaChiNCC.Text = Convert.ToString(r.Cells["DiaChi"].Value);
        }

        // =========================
        // TAB 3: LẬP PHIẾU NHẬP / XUẤT
        // =========================

        private void TaoTabKho(TabPage tab)
        {
            TaoLabel(tab, "Loại phiếu", 15, 15);
            cboLoaiPhieu.SetBounds(110, 15, 150, 27);
            cboLoaiPhieu.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoaiPhieu.Items.AddRange(new object[] { "NHAP", "XUAT" });
            cboLoaiPhieu.SelectedIndex = 0;
            cboLoaiPhieu.SelectedIndexChanged +=
                (s, e) => CapNhatTrangThaiNCC();
            tab.Controls.Add(cboLoaiPhieu);

            TaoLabel(tab, "Nhà cung cấp", 280, 15, 100);
            cboNCC.SetBounds(385, 15, 300, 27);
            cboNCC.DropDownStyle = ComboBoxStyle.DropDownList;
            tab.Controls.Add(cboNCC);

            TaoLabel(tab, "Vật tư", 15, 60);
            cboVatTu.SetBounds(110, 60, 280, 27);
            cboVatTu.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVatTu.SelectedIndexChanged += (s, e) => CapNhatGiaTuVatTu();
            tab.Controls.Add(cboVatTu);

            TaoLabel(tab, "Số lượng", 410, 60, 80);
            numSoLuong = TaoSo(tab, 490, 60, 120,
                0.01M, 1000000000, 1);

            TaoLabel(tab, "Đơn giá", 630, 60, 70);
            numDonGia = TaoSo(tab, 700, 60, 150,
                0, 1000000000, 0);

            TaoButton(tab, "Thêm vào phiếu", 865, 57, 150, ThemChiTiet);

            dgvChiTiet = TaoGrid(tab, 15, 110, 1080, 400);
            dgvChiTiet.Columns.Add("MaVatTu", "Mã vật tư");
            dgvChiTiet.Columns.Add("TenVatTu", "Tên vật tư");
            dgvChiTiet.Columns.Add("SoLuong", "Số lượng");
            dgvChiTiet.Columns.Add("DonGia", "Đơn giá");
            dgvChiTiet.Columns.Add("ThanhTien", "Thành tiền");

            dgvChiTiet.Columns["MaVatTu"].FillWeight = 70;
            dgvChiTiet.Columns["TenVatTu"].FillWeight = 160;

            TaoButton(tab, "Xóa dòng chọn", 15, 525, 120, XoaChiTiet);
            TaoButton(tab, "Xóa toàn bộ", 145, 525, 120, XoaTatCaChiTiet);
            TaoButton(tab, "Lưu phiếu kho", 280, 525, 140, LuuPhieuKho);

            Label huongDan = TaoLabel(tab,
                "Phiếu NHAP bắt buộc chọn nhà cung cấp. Phiếu XUAT không cần nhà cung cấp.",
                15, 575, 1000);
            huongDan.ForeColor = Color.DarkBlue;
        }

        private void CapNhatTrangThaiNCC()
        {
            cboNCC.Enabled = cboLoaiPhieu.Text == "NHAP";
        }

        private void NapComboNCC()
        {
            DataTable dt = nccBUS.GetAll();
            cboNCC.DataSource = null;
            cboNCC.DisplayMember = "TenNCC";
            cboNCC.ValueMember = "MaNCC";
            cboNCC.DataSource = dt;
            CapNhatTrangThaiNCC();
        }

        private void NapComboVatTu()
        {
            DataTable dt = vatTuBUS.GetAll();
            cboVatTu.DataSource = null;
            cboVatTu.DisplayMember = "TenVatTu";
            cboVatTu.ValueMember = "MaVatTu";
            cboVatTu.DataSource = dt;
        }

        private void CapNhatGiaTuVatTu()
        {
            // Đơn giá nhập/xuất được người dùng nhập.
            // Không tự lấy SoLuongTon làm đơn giá.
        }

        private void ThemChiTiet()
        {
            if (cboVatTu.SelectedItem == null)
                throw new Exception("Chưa có vật tư để chọn.");

            DataRowView row = cboVatTu.SelectedItem as DataRowView;
            if (row == null)
                throw new Exception("Không đọc được vật tư đã chọn.");

            string ma = Convert.ToString(row["MaVatTu"]);
            string ten = Convert.ToString(row["TenVatTu"]);

            foreach (ChiTietPhieuDTO ct in chiTiet)
            {
                if (ct.MaVatTu == ma)
                    throw new Exception(
                        "Vật tư này đã có trong phiếu. Mỗi mã vật tư chỉ thêm một lần.");
            }

            ChiTietPhieuDTO item = new ChiTietPhieuDTO
            {
                MaVatTu = ma,
                SoLuong = numSoLuong.Value,
                DonGia = numDonGia.Value
            };

            chiTiet.Add(item);

            dgvChiTiet.Rows.Add(
                ma,
                ten,
                item.SoLuong.ToString("N2"),
                item.DonGia.ToString("N2"),
                (item.SoLuong * item.DonGia).ToString("N2"));

            TinhTongPhieu();
        }

        private void XoaChiTiet()
        {
            if (dgvChiTiet.CurrentRow == null)
                throw new Exception("Hãy chọn dòng cần xóa.");

            int index = dgvChiTiet.CurrentRow.Index;
            if (index < 0 || index >= chiTiet.Count)
                return;

            chiTiet.RemoveAt(index);
            dgvChiTiet.Rows.RemoveAt(index);
            TinhTongPhieu();
        }

        private void XoaTatCaChiTiet()
        {
            chiTiet.Clear();
            dgvChiTiet.Rows.Clear();
            TinhTongPhieu();
        }

        private void TinhTongPhieu()
        {
            decimal tong = 0;
            foreach (ChiTietPhieuDTO ct in chiTiet)
                tong += ct.SoLuong * ct.DonGia;

            Text = "HỆ THỐNG QUẢN LÝ KHO | Tổng phiếu: "
                + tong.ToString("N2");
        }

        private void LuuPhieuKho()
        {
            string loai = cboLoaiPhieu.Text;

            if (loai != "NHAP" && loai != "XUAT")
                throw new Exception("Hãy chọn loại phiếu.");

            if (chiTiet.Count == 0)
                throw new Exception("Phiếu chưa có vật tư.");

            int? maNCC = null;

            if (loai == "NHAP")
            {
                if (cboNCC.SelectedValue == null)
                    throw new Exception("Phiếu nhập phải chọn nhà cung cấp.");

                maNCC = Convert.ToInt32(cboNCC.SelectedValue);
            }

            if (MessageBox.Show(
                "Xác nhận lưu phiếu " + loai + "?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int maPhieu = khoBUS.LuuPhieu(loai, maNCC, chiTiet);

            MessageBox.Show(
                "Lưu phiếu thành công.\nMã phiếu: " + maPhieu,
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            XoaTatCaChiTiet();
            NapVatTu();
            NapComboVatTu();
            NapCanhBao();
            HienTongGiaTri();
        }

        // =========================
        // TAB 4: CẢNH BÁO VÀ BÁO CÁO
        // =========================

        private void TaoTabBaoCao(TabPage tab)
        {
            TaoLabel(tab, "Ngưỡng tồn kho", 15, 15, 110);
            numNguong = TaoSo(tab, 130, 15, 150,
                0, 1000000000, 10);

            TaoButton(tab, "Lọc tồn kho thấp", 295, 12, 150,
                NapCanhBao);

            TaoButton(tab, "Tính giá trị tồn", 455, 12, 150,
                HienTongGiaTri);

            lblTongGiaTri = TaoLabel(tab,
                "Tổng giá trị tồn kho: 0", 15, 55, 700);
            lblTongGiaTri.Font = new Font(
                "Segoe UI", 12, FontStyle.Bold);

            dgvCanhBao = TaoGrid(tab, 15, 100, 1080, 560);
        }

        private void NapCanhBao()
        {
            dgvCanhBao.DataSource =
                khoBUS.CanhBaoTonKho(numNguong.Value);
        }

        private void HienTongGiaTri()
        {
            decimal tong = khoBUS.TongGiaTriTonKho();
            lblTongGiaTri.Text =
                "Tổng giá trị tồn kho (ước tính): "
                + tong.ToString("N2");
        }

        // =========================
        // NẠP DỮ LIỆU
        // =========================

        private void NapTatCaDuLieu()
        {
            try
            {
                NapVatTu();
                NapNCC();
                NapComboNCC();
                NapComboVatTu();
                NapCanhBao();
                HienTongGiaTri();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không tải được dữ liệu.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}


using System;
using System.Data;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS
{
    public class HotelBUS
    {
        private readonly HotelDAL dal = new HotelDAL();

        public DataTable GetServices()
        {
            return dal.GetServices();
        }

        public int CheckIn(KhachHangDTO kh, string soPhong)
        {
            if (kh == null)
                throw new ArgumentException("Chưa nhập thông tin khách.");

            if (string.IsNullOrWhiteSpace(kh.HoTen))
                throw new ArgumentException("Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(kh.CCCD))
                throw new ArgumentException("CCCD không được để trống.");

            if (kh.CCCD.Trim().Length > 20)
                throw new ArgumentException("CCCD tối đa 20 ký tự.");

            if (string.IsNullOrWhiteSpace(soPhong))
                throw new ArgumentException("Chưa chọn phòng.");

            kh.HoTen = kh.HoTen.Trim();
            kh.CCCD = kh.CCCD.Trim();
            kh.SoDienThoai = kh.SoDienThoai?.Trim();

            return dal.CheckIn(kh, soPhong.Trim());
        }

        public void AddService(int maPhieu, int maDV, int soLuong)
        {
            if (maPhieu <= 0)
                throw new ArgumentException("Mã phiếu không hợp lệ.");

            if (maDV <= 0)
                throw new ArgumentException("Chưa chọn dịch vụ.");

            if (soLuong <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.");

            dal.AddService(maPhieu, maDV, soLuong);
        }

        public decimal CheckOut(int maPhieu)
        {
            if (maPhieu <= 0)
                throw new ArgumentException("Mã phiếu không hợp lệ.");

            return dal.CheckOut(maPhieu);
        }
    }
}

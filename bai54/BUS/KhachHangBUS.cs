
using System;
using System.Data;
using HotelManagement.DAL;

namespace HotelManagement.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL dal = new KhachHangDAL();

        public DataTable GetAll()
        {
            return dal.GetAll();
        }

        public int Add(string hoTen, string cccd, string sdt)
        {
            if (string.IsNullOrWhiteSpace(hoTen))
                throw new ArgumentException("Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(cccd))
                throw new ArgumentException("CCCD không được để trống.");

            if (cccd.Length < 9 || cccd.Length > 20)
                throw new ArgumentException("CCCD phải có từ 9 đến 20 ký tự.");

            return dal.Add(hoTen.Trim(), cccd.Trim(), sdt?.Trim());
        }
    }
}

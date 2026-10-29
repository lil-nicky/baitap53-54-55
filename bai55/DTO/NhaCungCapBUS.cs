
using System;
using System.Data;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class NhaCungCapBUS
    {
        private readonly NhaCungCapDAL dal = new NhaCungCapDAL();

        public DataTable GetAll(string tuKhoa = "")
        {
            return dal.GetAll(tuKhoa);
        }

        public void Them(NhaCungCapDTO ncc)
        {
            if (ncc == null || string.IsNullOrWhiteSpace(ncc.TenNCC))
                throw new ArgumentException("Tên nhà cung cấp không được để trống.");

            ncc.TenNCC = ncc.TenNCC.Trim();
            dal.Insert(ncc);
        }

        public void Sua(NhaCungCapDTO ncc)
        {
            if (ncc == null || ncc.MaNCC <= 0 ||
                string.IsNullOrWhiteSpace(ncc.TenNCC))
                throw new ArgumentException("Thông tin nhà cung cấp không hợp lệ.");

            if (!dal.Update(ncc))
                throw new Exception("Không tìm thấy nhà cung cấp cần sửa.");
        }

        public void Xoa(int maNCC)
        {
            if (maNCC <= 0)
                throw new ArgumentException("Mã nhà cung cấp không hợp lệ.");

            if (!dal.Delete(maNCC))
                throw new Exception("Không tìm thấy nhà cung cấp cần xóa.");
        }
    }
}

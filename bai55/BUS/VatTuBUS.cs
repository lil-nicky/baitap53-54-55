
using System;
using System.Data;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class VatTuBUS
    {
        private readonly VatTuDAL dal = new VatTuDAL();

        public DataTable GetAll(string tuKhoa = "")
        {
            return dal.GetAll(tuKhoa);
        }

        public void Them(VatTuDTO vt)
        {
            if (vt == null ||
                string.IsNullOrWhiteSpace(vt.MaVatTu) ||
                string.IsNullOrWhiteSpace(vt.TenVatTu))
                throw new ArgumentException("Mã và tên vật tư không được để trống.");

            if (vt.MaVatTu.Trim().Length > 20)
                throw new ArgumentException("Mã vật tư tối đa 20 ký tự.");

            if (vt.SoLuongTon < 0)
                throw new ArgumentException("Tồn kho không được âm.");

            vt.MaVatTu = vt.MaVatTu.Trim();
            vt.TenVatTu = vt.TenVatTu.Trim();

            dal.Insert(vt);
        }

        public void Sua(VatTuDTO vt)
        {
            if (vt == null || string.IsNullOrWhiteSpace(vt.MaVatTu) ||
                string.IsNullOrWhiteSpace(vt.TenVatTu))
                throw new ArgumentException("Mã và tên vật tư không được để trống.");

            if (!dal.Update(vt))
                throw new Exception("Không tìm thấy vật tư cần sửa.");
        }

        public void Xoa(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                throw new ArgumentException("Hãy nhập mã vật tư.");

            if (!dal.Delete(ma.Trim()))
                throw new Exception("Không tìm thấy vật tư cần xóa.");
        }
    }
}

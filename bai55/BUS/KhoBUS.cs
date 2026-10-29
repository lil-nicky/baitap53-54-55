
using System;
using System.Collections.Generic;
using System.Data;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class KhoBUS
    {
        private readonly KhoDAL dal = new KhoDAL();

        public int LuuPhieu(
            string loaiPhieu,
            int? maNCC,
            List<ChiTietPhieuDTO> chiTiet)
        {
            if (loaiPhieu == "NHAP" && !maNCC.HasValue)
                throw new ArgumentException("Phiếu nhập phải chọn nhà cung cấp.");

            return dal.LuuPhieu(loaiPhieu, maNCC, chiTiet);
        }

        public DataTable CanhBaoTonKho(decimal nguong)
        {
            if (nguong < 0)
                throw new ArgumentException("Ngưỡng tồn kho không được âm.");

            return dal.GetCanhBaoTonKho(nguong);
        }

        public decimal TongGiaTriTonKho()
        {
            return dal.TinhTongGiaTriTonKho();
        }
    }
}


using System.Data;
using HotelManagement.DAL;

namespace HotelManagement.BUS
{
    public class PhongBUS
    {
        private readonly PhongDAL dal = new PhongDAL();

        public DataTable GetAll()
        {
            return dal.GetAll();
        }

        public DataView Filter(DataTable dt, string loaiPhong,
                               string trangThai)
        {
            DataView view = new DataView(dt);

            string loai = (loaiPhong ?? "").Replace("'", "''");
            string trang = (trangThai ?? "").Replace("'", "''");

            string dieuKien = "";

            if (loai != "" && loai != "Tất cả")
                dieuKien = "[LoaiPhong] = '" + loai + "'";

            if (trang != "" && trang != "Tất cả")
            {
                if (dieuKien != "") dieuKien += " AND ";
                dieuKien += "[TrangThai] = '" + trang + "'";
            }

            view.RowFilter = dieuKien;
            return view;
        }
    }
}

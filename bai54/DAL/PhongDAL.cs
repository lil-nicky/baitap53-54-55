
using System.Data;
using System.Data.SqlClient;

namespace HotelManagement.DAL
{
    public class PhongDAL
    {
        public DataTable GetAll()
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                @"SELECT SoPhong, LoaiPhong, GiaTheoGio, TrangThai
                  FROM dbo.Phong ORDER BY SoPhong", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}

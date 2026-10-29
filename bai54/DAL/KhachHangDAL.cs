
using System;
using System.Data;
using System.Data.SqlClient;

namespace HotelManagement.DAL
{
    public class KhachHangDAL
    {
        public DataTable GetAll()
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM KhachHang ORDER BY MaKH", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public int Add(string hoTen, string cccd, string sdt)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO KhachHang(HoTen, CCCD, SDT)
                VALUES (@HoTen, @CCCD, @SDT);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
            {
                cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = hoTen;
                cmd.Parameters.Add("@CCCD", SqlDbType.VarChar, 20).Value = cccd;
                cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(sdt) ? (object)DBNull.Value : sdt;

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }
    }
}

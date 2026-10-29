
using System;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class NhaCungCapDAL
    {
        public DataTable GetAll(string tuKhoa = "")
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT MaNCC, TenNCC, DienThoai, DiaChi
                FROM dbo.NhaCungCap
                WHERE TenNCC LIKE @TuKhoa
                   OR DienThoai LIKE @TuKhoa
                ORDER BY MaNCC DESC;", conn))
            {
                cmd.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 150)
                    .Value = "%" + (tuKhoa ?? "").Trim() + "%";

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                return dt;
            }
        }

        public bool Insert(NhaCungCapDTO ncc)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO dbo.NhaCungCap(TenNCC, DienThoai, DiaChi)
                VALUES (@Ten, @DienThoai, @DiaChi);", conn))
            {
                cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 150).Value = ncc.TenNCC;
                cmd.Parameters.Add("@DienThoai", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(ncc.DienThoai)
                    ? (object)DBNull.Value : ncc.DienThoai;
                cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(ncc.DiaChi)
                    ? (object)DBNull.Value : ncc.DiaChi;

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(NhaCungCapDTO ncc)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE dbo.NhaCungCap
                SET TenNCC = @Ten, DienThoai = @DienThoai, DiaChi = @DiaChi
                WHERE MaNCC = @Ma;", conn))
            {
                cmd.Parameters.Add("@Ma", SqlDbType.Int).Value = ncc.MaNCC;
                cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 150).Value = ncc.TenNCC;
                cmd.Parameters.Add("@DienThoai", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(ncc.DienThoai)
                    ? (object)DBNull.Value : ncc.DienThoai;
                cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(ncc.DiaChi)
                    ? (object)DBNull.Value : ncc.DiaChi;

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int maNCC)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM dbo.NhaCungCap WHERE MaNCC = @Ma;", conn))
            {
                cmd.Parameters.Add("@Ma", SqlDbType.Int).Value = maNCC;
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

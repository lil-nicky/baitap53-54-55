
using System;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class VatTuDAL
    {
        public DataTable GetAll(string tuKhoa = "")
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT MaVatTu, TenVatTu, DVT, SoLuongTon
                FROM dbo.VatTu
                WHERE MaVatTu LIKE @TuKhoa
                   OR TenVatTu LIKE @TuKhoa
                ORDER BY MaVatTu;", conn))
            {
                cmd.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 150)
                    .Value = "%" + (tuKhoa ?? "").Trim() + "%";

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                return dt;
            }
        }

        public bool Insert(VatTuDTO vt)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO dbo.VatTu
                    (MaVatTu, TenVatTu, DVT, SoLuongTon)
                VALUES
                    (@Ma, @Ten, @DVT, @Ton);", conn))
            {
                cmd.Parameters.Add("@Ma", SqlDbType.VarChar, 20).Value = vt.MaVatTu;
                cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 150).Value = vt.TenVatTu;
                cmd.Parameters.Add("@DVT", SqlDbType.NVarChar, 20).Value =
                    (object)vt.DVT ?? DBNull.Value;
                cmd.Parameters.Add("@Ton", SqlDbType.Decimal).Value = vt.SoLuongTon;
                cmd.Parameters["@Ton"].Precision = 18;
                cmd.Parameters["@Ton"].Scale = 2;

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(VatTuDTO vt)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE dbo.VatTu
                SET TenVatTu = @Ten, DVT = @DVT
                WHERE MaVatTu = @Ma;", conn))
            {
                cmd.Parameters.Add("@Ma", SqlDbType.VarChar, 20).Value = vt.MaVatTu;
                cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 150).Value = vt.TenVatTu;
                cmd.Parameters.Add("@DVT", SqlDbType.NVarChar, 20).Value =
                    (object)vt.DVT ?? DBNull.Value;

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(string maVatTu)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM dbo.VatTu WHERE MaVatTu = @Ma;", conn))
            {
                cmd.Parameters.Add("@Ma", SqlDbType.VarChar, 20).Value = maVatTu;
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

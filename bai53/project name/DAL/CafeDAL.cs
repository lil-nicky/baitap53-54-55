using System;
using System.Data;
using System.Data.SqlClient;
using CafePOSSystem.DTO;

namespace CafePOSSystem.DAL
{
    public class CafeDAL
    {
        public DataTable GetTables()
        {
            using (var cn = Db.CreateConnection())
            using (var da = new SqlDataAdapter(
                "SELECT MaBan,TenBan,TrangThai FROM BanAn ORDER BY MaBan", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetCategories()
        {
            using (var cn = Db.CreateConnection())
            using (var da = new SqlDataAdapter(
                "SELECT MaDanhMuc,TenDanhMuc FROM DanhMuc ORDER BY TenDanhMuc", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetMenu(int maDanhMuc)
        {
            using (var cn = Db.CreateConnection())
            using (var cmd = new SqlCommand(
                "SELECT MaMon,TenMon,DonGia FROM MonAn WHERE MaDanhMuc=@id ORDER BY TenMon", cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = maDanhMuc;
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetOrder(int maBan)
        {
            const string sql = @"
SELECT m.MaMon,m.TenMon,ct.SoLuong,ct.DonGia,
       CAST(ct.SoLuong*ct.DonGia AS decimal(18,2)) AS ThanhTien
FROM HoaDon h
JOIN ChiTietHoaDon ct ON h.MaHoaDon=ct.MaHoaDon
JOIN MonAn m ON m.MaMon=ct.MaMon
WHERE h.MaBan=@ban AND h.TrangThai=0
ORDER BY m.TenMon";
            using (var cn = Db.CreateConnection())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public void AddItem(int maBan, int maMon, int soLuong)
        {
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            using (var cn = Db.CreateConnection())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        int maHoaDon;
                        using (var cmd = new SqlCommand(
                            "SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan=@ban AND TrangThai=0 ORDER BY MaHoaDon DESC",
                            cn, tx))
                        {
                            cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                            object result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                using (var create = new SqlCommand(
                                    "INSERT INTO HoaDon(MaBan,GioVao,TongTien,TrangThai) VALUES(@ban,GETDATE(),0,0); SELECT CAST(SCOPE_IDENTITY() AS int);",
                                    cn, tx))
                                {
                                    create.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                                    maHoaDon = (int)create.ExecuteScalar();
                                }
                            }
                            else maHoaDon = Convert.ToInt32(result);
                        }

                        decimal price;
                        using (var priceCmd = new SqlCommand("SELECT DonGia FROM MonAn WHERE MaMon=@mon", cn, tx))
                        {
                            priceCmd.Parameters.Add("@mon", SqlDbType.Int).Value = maMon;
                            object p = priceCmd.ExecuteScalar();
                            if (p == null) throw new InvalidOperationException("Món ăn không tồn tại.");
                            price = Convert.ToDecimal(p);
                        }

                        using (var cmd = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM ChiTietHoaDon WHERE MaHoaDon=@hd AND MaMon=@mon)
 UPDATE ChiTietHoaDon SET SoLuong=SoLuong+@sl WHERE MaHoaDon=@hd AND MaMon=@mon;
ELSE
 INSERT INTO ChiTietHoaDon(MaHoaDon,MaMon,SoLuong,DonGia) VALUES(@hd,@mon,@sl,@gia);", cn, tx))
                        {
                            cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHoaDon;
                            cmd.Parameters.Add("@mon", SqlDbType.Int).Value = maMon;
                            cmd.Parameters.Add("@sl", SqlDbType.Int).Value = soLuong;
                            cmd.Parameters.Add("@gia", SqlDbType.Decimal).Value = price;
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand("UPDATE BanAn SET TrangThai=N'Có khách' WHERE MaBan=@ban", cn, tx))
                        {
                            cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        public decimal Pay(int maBan)
        {
            using (var cn = Db.CreateConnection())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        int maHoaDon;
                        using (var cmd = new SqlCommand(
                            "SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan=@ban AND TrangThai=0 ORDER BY MaHoaDon DESC",
                            cn, tx))
                        {
                            cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                            object id = cmd.ExecuteScalar();
                            if (id == null) throw new InvalidOperationException("Bàn chưa có hóa đơn chưa thanh toán.");
                            maHoaDon = Convert.ToInt32(id);
                        }

                        decimal total;
                        using (var cmd = new SqlCommand(
                            "SELECT ISNULL(SUM(SoLuong*DonGia),0) FROM ChiTietHoaDon WHERE MaHoaDon=@hd", cn, tx))
                        {
                            cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHoaDon;
                            total = Convert.ToDecimal(cmd.ExecuteScalar());
                        }
                        using (var cmd = new SqlCommand(
                            "UPDATE HoaDon SET GioRa=GETDATE(),TongTien=@tong,TrangThai=1 WHERE MaHoaDon=@hd; UPDATE BanAn SET TrangThai=N'Trống' WHERE MaBan=@ban;",
                            cn, tx))
                        {
                            cmd.Parameters.Add("@tong", SqlDbType.Decimal).Value = total;
                            cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHoaDon;
                            cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                        return total;
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }
    }
}

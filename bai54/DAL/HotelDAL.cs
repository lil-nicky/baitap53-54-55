
using System;
using System.Data;
using System.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL
{
    public class HotelDAL
    {
        public DataTable GetServices()
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT MaDV, TenDV, DonGia FROM dbo.DichVu ORDER BY TenDV",
                conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public int CheckIn(KhachHangDTO kh, string soPhong)
        {
            using (SqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Khóa phòng để hạn chế hai lượt check-in đồng thời.
                        string trangThai;
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT TrangThai FROM dbo.Phong
                              WITH (UPDLOCK, HOLDLOCK)
                              WHERE SoPhong = @SoPhong",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@SoPhong", SqlDbType.VarChar, 10)
                                .Value = soPhong;

                            object result = cmd.ExecuteScalar();
                            if (result == null)
                                throw new Exception("Không tìm thấy phòng.");

                            trangThai = result.ToString();
                        }

                        if (trangThai != "Trống")
                            throw new Exception("Phòng không ở trạng thái Trống.");

                        int maKH;

                        // Tìm khách bằng CCCD.
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT MaKH FROM dbo.KhachHang
                              WHERE CCCD = @CCCD",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@CCCD", SqlDbType.VarChar, 20)
                                .Value = kh.CCCD;

                            object result = cmd.ExecuteScalar();

                            if (result != null)
                            {
                                maKH = Convert.ToInt32(result);

                                // Cập nhật thông tin khách đã tồn tại.
                                using (SqlCommand update = new SqlCommand(
                                    @"UPDATE dbo.KhachHang
                                      SET HoTen = @HoTen,
                                          SoDienThoai = @SDT
                                      WHERE MaKH = @MaKH",
                                    conn, tran))
                                {
                                    update.Parameters.Add("@HoTen",
                                        SqlDbType.NVarChar, 100).Value = kh.HoTen;
                                    update.Parameters.Add("@SDT",
                                        SqlDbType.VarChar, 15).Value =
                                        string.IsNullOrWhiteSpace(kh.SoDienThoai)
                                        ? (object)DBNull.Value : kh.SoDienThoai;
                                    update.Parameters.Add("@MaKH",
                                        SqlDbType.Int).Value = maKH;
                                    update.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (SqlCommand insert = new SqlCommand(
                                    @"INSERT INTO dbo.KhachHang
                                      (HoTen, CCCD, SoDienThoai)
                                      VALUES (@HoTen, @CCCD, @SDT);
                                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                                    conn, tran))
                                {
                                    insert.Parameters.Add("@HoTen",
                                        SqlDbType.NVarChar, 100).Value = kh.HoTen;
                                    insert.Parameters.Add("@CCCD",
                                        SqlDbType.VarChar, 20).Value = kh.CCCD;
                                    insert.Parameters.Add("@SDT",
                                        SqlDbType.VarChar, 15).Value =
                                        string.IsNullOrWhiteSpace(kh.SoDienThoai)
                                        ? (object)DBNull.Value : kh.SoDienThoai;

                                    maKH = Convert.ToInt32(insert.ExecuteScalar());
                                }
                            }
                        }

                        int maPhieu;

                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO dbo.PhieuDatPhong
                              (SoPhong, MaKH, NgayNhanPhong, TongTienThanhToan)
                              VALUES (@SoPhong, @MaKH, GETDATE(), 0);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@SoPhong", SqlDbType.VarChar, 10)
                                .Value = soPhong;
                            cmd.Parameters.Add("@MaKH", SqlDbType.Int)
                                .Value = maKH;
                            maPhieu = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE dbo.Phong
                              SET TrangThai = N'Đang ở'
                              WHERE SoPhong = @SoPhong",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@SoPhong", SqlDbType.VarChar, 10)
                                .Value = soPhong;
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return maPhieu;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public void AddService(int maPhieu, int maDV, int soLuong)
        {
            using (SqlConnection conn = Db.GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Chỉ cho phép gọi dịch vụ khi phiếu chưa checkout.
                        using (SqlCommand cmd = new SqlCommand(
                            @"IF NOT EXISTS
                              (SELECT 1 FROM dbo.PhieuDatPhong
                               WHERE MaPhieu = @MaPhieu
                                 AND NgayTraPhong IS NULL)
                              THROW 50001, N'Phiếu không tồn tại hoặc đã checkout.', 1;

                              IF NOT EXISTS
                              (SELECT 1 FROM dbo.DichVu WHERE MaDV = @MaDV)
                              THROW 50002, N'Không tìm thấy dịch vụ.', 1;

                              INSERT INTO dbo.SuDungDichVu
                              (MaPhieu, MaDV, SoLuong, ThoiGian)
                              VALUES (@MaPhieu, @MaDV, @SoLuong, GETDATE());",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@MaPhieu", SqlDbType.Int)
                                .Value = maPhieu;
                            cmd.Parameters.Add("@MaDV", SqlDbType.Int)
                                .Value = maDV;
                            cmd.Parameters.Add("@SoLuong", SqlDbType.Int)
                                .Value = soLuong;

                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public decimal CheckOut(int maPhieu)
        {
            using (SqlConnection conn = Db.GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        string soPhong;
                        DateTime ngayNhan;
                        decimal giaTheoGio;

                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT p.SoPhong, f.NgayNhanPhong,
                                     p.GiaTheoGio
                              FROM dbo.PhieuDatPhong f
                              JOIN dbo.Phong p ON p.SoPhong = f.SoPhong
                              WHERE f.MaPhieu = @MaPhieu
                                AND f.NgayTraPhong IS NULL",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@MaPhieu", SqlDbType.Int)
                                .Value = maPhieu;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new Exception(
                                        "Phiếu không tồn tại hoặc đã checkout.");

                                soPhong = reader.GetString(0);
                                ngayNhan = reader.GetDateTime(1);
                                giaTheoGio = reader.GetDecimal(2);
                            }
                        }

                        DateTime ngayTra = DateTime.Now;

                        // Tính theo giờ, làm tròn lên giờ kế tiếp.
                        double soGio = Math.Max(1,
                            Math.Ceiling((ngayTra - ngayNhan).TotalHours));

                        decimal tienPhong = (decimal)soGio * giaTheoGio;
                        decimal tienDichVu;

                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COALESCE(SUM(s.SoLuong * d.DonGia), 0)
                              FROM dbo.SuDungDichVu s
                              JOIN dbo.DichVu d ON d.MaDV = s.MaDV
                              WHERE s.MaPhieu = @MaPhieu",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@MaPhieu", SqlDbType.Int)
                                .Value = maPhieu;
                            tienDichVu = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        decimal tongTien = tienPhong + tienDichVu;

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE dbo.PhieuDatPhong
                              SET NgayTraPhong = @NgayTra,
                                  TongTienThanhToan = @TongTien
                              WHERE MaPhieu = @MaPhieu
                                AND NgayTraPhong IS NULL",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@NgayTra", SqlDbType.DateTime)
                                .Value = ngayTra;
                            cmd.Parameters.Add("@TongTien", SqlDbType.Decimal)
                                .Value = tongTien;
                            cmd.Parameters["@TongTien"].Precision = 18;
                            cmd.Parameters["@TongTien"].Scale = 2;
                            cmd.Parameters.Add("@MaPhieu", SqlDbType.Int)
                                .Value = maPhieu;

                            if (cmd.ExecuteNonQuery() != 1)
                                throw new Exception("Không thể cập nhật hóa đơn.");
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE dbo.Phong
                              SET TrangThai = N'Đang dọn dẹp'
                              WHERE SoPhong = @SoPhong",
                            conn, tran))
                        {
                            cmd.Parameters.Add("@SoPhong", SqlDbType.VarChar, 10)
                                .Value = soPhong;
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return tongTien;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

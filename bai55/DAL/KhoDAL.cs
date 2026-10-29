
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class KhoDAL
    {
        public int LuuPhieu(
            string loaiPhieu,
            int? maNCC,
            List<ChiTietPhieuDTO> chiTiet)
        {
            if (loaiPhieu != "NHAP" && loaiPhieu != "XUAT")
                throw new ArgumentException("Loại phiếu không hợp lệ.");

            if (chiTiet == null || chiTiet.Count == 0)
                throw new ArgumentException("Phiếu phải có ít nhất một vật tư.");

            // Mỗi vật tư chỉ xuất hiện một lần vì khóa chính là
            // (MaPhieu, MaVatTu).
            var gop = new Dictionary<string, ChiTietPhieuDTO>(
                StringComparer.OrdinalIgnoreCase);

            foreach (ChiTietPhieuDTO ct in chiTiet)
            {
                if (string.IsNullOrWhiteSpace(ct.MaVatTu) ||
                    ct.SoLuong <= 0 || ct.DonGia < 0)
                    throw new ArgumentException("Số lượng hoặc đơn giá không hợp lệ.");

                if (gop.ContainsKey(ct.MaVatTu))
                    throw new ArgumentException(
                        "Vật tư " + ct.MaVatTu + " bị trùng trong phiếu.");

                gop.Add(ct.MaVatTu, ct);
            }

            decimal tongTien = 0;
            foreach (ChiTietPhieuDTO ct in gop.Values)
                tongTien += ct.SoLuong * ct.DonGia;

            using (SqlConnection conn = Db.GetConnection())
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        int maPhieu;

                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO dbo.PhieuKho(LoaiPhieu, MaNCC, TongTien)
                            OUTPUT INSERTED.MaPhieu
                            VALUES (@Loai, @MaNCC, @Tong);",
                            conn, trans))
                        {
                            cmd.Parameters.Add("@Loai", SqlDbType.NVarChar, 10)
                                .Value = loaiPhieu;

                            cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value =
                                loaiPhieu == "NHAP" && maNCC.HasValue
                                ? (object)maNCC.Value : DBNull.Value;

                            SqlParameter pTong =
                                cmd.Parameters.Add("@Tong", SqlDbType.Decimal);
                            pTong.Precision = 18;
                            pTong.Scale = 2;
                            pTong.Value = tongTien;

                            maPhieu = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (ChiTietPhieuDTO ct in gop.Values)
                        {
                            // Cập nhật tồn kho có điều kiện, không cho xuất âm.
                            string sqlCapNhat = loaiPhieu == "NHAP"
                                ? @"UPDATE dbo.VatTu
                                    SET SoLuongTon = SoLuongTon + @SL
                                    WHERE MaVatTu = @Ma;"
                                : @"UPDATE dbo.VatTu
                                    SET SoLuongTon = SoLuongTon - @SL
                                    WHERE MaVatTu = @Ma
                                      AND SoLuongTon >= @SL;";

                            using (SqlCommand cmd = new SqlCommand(
                                sqlCapNhat, conn, trans))
                            {
                                cmd.Parameters.Add("@Ma", SqlDbType.VarChar, 20)
                                    .Value = ct.MaVatTu;

                                SqlParameter pSL =
                                    cmd.Parameters.Add("@SL", SqlDbType.Decimal);
                                pSL.Precision = 18;
                                pSL.Scale = 2;
                                pSL.Value = ct.SoLuong;

                                int rows = cmd.ExecuteNonQuery();

                                if (rows == 0)
                                {
                                    if (loaiPhieu == "XUAT")
                                        throw new InvalidOperationException(
                                            "Không đủ hàng trong kho hoặc mã vật tư không tồn tại: "
                                            + ct.MaVatTu);

                                    throw new InvalidOperationException(
                                        "Mã vật tư không tồn tại: " + ct.MaVatTu);
                                }
                            }

                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO dbo.ChiTietPhieuKho
                                    (MaPhieu, MaVatTu, SoLuong, DonGia)
                                VALUES (@MaPhieu, @MaVatTu, @SL, @Gia);",
                                conn, trans))
                            {
                                cmd.Parameters.Add("@MaPhieu", SqlDbType.Int)
                                    .Value = maPhieu;
                                cmd.Parameters.Add("@MaVatTu", SqlDbType.VarChar, 20)
                                    .Value = ct.MaVatTu;

                                SqlParameter pSL =
                                    cmd.Parameters.Add("@SL", SqlDbType.Decimal);
                                pSL.Precision = 18;
                                pSL.Scale = 2;
                                pSL.Value = ct.SoLuong;

                                SqlParameter pGia =
                                    cmd.Parameters.Add("@Gia", SqlDbType.Decimal);
                                pGia.Precision = 18;
                                pGia.Scale = 2;
                                pGia.Value = ct.DonGia;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return maPhieu;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable GetCanhBaoTonKho(decimal nguong)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT MaVatTu, TenVatTu, DVT, SoLuongTon
                FROM dbo.VatTu
                WHERE SoLuongTon < @Nguong
                ORDER BY SoLuongTon, MaVatTu;", conn))
            {
                SqlParameter p = cmd.Parameters.Add("@Nguong", SqlDbType.Decimal);
                p.Precision = 18;
                p.Scale = 2;
                p.Value = nguong;

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
                return dt;
            }
        }

        public decimal TinhTongGiaTriTonKho()
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COALESCE(SUM(SoLuongTon * GiaNhap.Gia), 0)
                FROM dbo.VatTu v
                OUTER APPLY
                (
                    SELECT TOP 1 DonGia AS Gia
                    FROM dbo.ChiTietPhieuKho ct
                    INNER JOIN dbo.PhieuKho p ON p.MaPhieu = ct.MaPhieu
                    WHERE ct.MaVatTu = v.MaVatTu
                      AND p.LoaiPhieu = N'NHAP'
                    ORDER BY p.NgayLap DESC, p.MaPhieu DESC
                ) AS GiaNhap;", conn))
            {
                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }
}

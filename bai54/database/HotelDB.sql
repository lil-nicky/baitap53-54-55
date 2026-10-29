
USE HotelDB;
GO

-- Bảng Phong đã tồn tại theo cấu trúc của đề.
-- Kiểm tra đúng kiểu dữ liệu và tên cột trước khi chạy.
-- SoPhong: varchar(10), khóa chính
-- LoaiPhong: nvarchar(50)
-- GiaTheoGio: decimal(18,2)
-- TrangThai: nvarchar(30)

IF OBJECT_ID('dbo.KhachHang', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang
    (
        MaKH INT IDENTITY(1,1) PRIMARY KEY,
        HoTen NVARCHAR(100) NOT NULL,
        CCCD VARCHAR(20) NOT NULL UNIQUE,
        SoDienThoai VARCHAR(15) NULL
    );
END;
GO

IF OBJECT_ID('dbo.DichVu', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DichVu
    (
        MaDV INT IDENTITY(1,1) PRIMARY KEY,
        TenDV NVARCHAR(100) NOT NULL,
        DonGia DECIMAL(18,2) NOT NULL
    );
END;
GO

IF OBJECT_ID('dbo.PhieuDatPhong', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhieuDatPhong
    (
        MaPhieu INT IDENTITY(1,1) PRIMARY KEY,
        SoPhong VARCHAR(10) NOT NULL,
        MaKH INT NOT NULL,
        NgayNhanPhong DATETIME NOT NULL DEFAULT GETDATE(),
        NgayTraPhong DATETIME NULL,
        TongTienThanhToan DECIMAL(18,2) NOT NULL DEFAULT 0,

        CONSTRAINT FK_Phieu_Phong
            FOREIGN KEY (SoPhong) REFERENCES dbo.Phong(SoPhong),

        CONSTRAINT FK_Phieu_KhachHang
            FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
    );
END;
GO

IF OBJECT_ID('dbo.SuDungDichVu', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SuDungDichVu
    (
        MaSD INT IDENTITY(1,1) PRIMARY KEY,
        MaPhieu INT NOT NULL,
        MaDV INT NOT NULL,
        SoLuong INT NOT NULL CHECK (SoLuong > 0),
        ThoiGian DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_SD_Phieu
            FOREIGN KEY (MaPhieu)
            REFERENCES dbo.PhieuDatPhong(MaPhieu),

        CONSTRAINT FK_SD_DichVu
            FOREIGN KEY (MaDV)
            REFERENCES dbo.DichVu(MaDV)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu)
BEGIN
    INSERT INTO dbo.DichVu (TenDV, DonGia)
    VALUES
        (N'Nước suối', 15000),
        (N'Giặt là', 50000),
        (N'Ăn sáng', 80000),
        (N'Nước ngọt', 20000);
END;
GO

SELECT * FROM dbo.Phong;
SELECT * FROM dbo.KhachHang;
SELECT * FROM dbo.DichVu;
SELECT * FROM dbo.PhieuDatPhong;
SELECT * FROM dbo.SuDungDichVu;
GO


USE InventoryDB;
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.NhaCungCap
    WHERE DienThoai = '0901234567'
)
BEGIN
    INSERT INTO dbo.NhaCungCap(TenNCC, DienThoai, DiaChi)
    VALUES (N'Công ty Vật tư An Phát', '0901234567', N'Hà Nội');
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.NhaCungCap
    WHERE DienThoai = '0912345678'
)
BEGIN
    INSERT INTO dbo.NhaCungCap(TenNCC, DienThoai, DiaChi)
    VALUES (N'Công ty Minh Phát', '0912345678', N'Hồ Chí Minh');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.VatTu WHERE MaVatTu = 'VT001')
    INSERT INTO dbo.VatTu(MaVatTu, TenVatTu, DVT, SoLuongTon)
    VALUES ('VT001', N'Giấy A4', N'Ream', 100);

IF NOT EXISTS (SELECT 1 FROM dbo.VatTu WHERE MaVatTu = 'VT002')
    INSERT INTO dbo.VatTu(MaVatTu, TenVatTu, DVT, SoLuongTon)
    VALUES ('VT002', N'Mực in', N'Hộp', 20);

IF NOT EXISTS (SELECT 1 FROM dbo.VatTu WHERE MaVatTu = 'VT003')
    INSERT INTO dbo.VatTu(MaVatTu, TenVatTu, DVT, SoLuongTon)
    VALUES ('VT003', N'Bút bi', N'Cái', 50);
GO

SELECT * FROM dbo.NhaCungCap;
SELECT * FROM dbo.VatTu;
SELECT * FROM dbo.PhieuKho;
SELECT * FROM dbo.ChiTietPhieuKho;

-- =====================================================
-- TEST FIRMA SEED SCRIPT
-- Master DB'ye 1 test firma + admin kullanıcı ile ilişki
-- =====================================================

USE Accounting_Master;
GO

DECLARE @AdminUserId UNIQUEIDENTIFIER;
DECLARE @CompanyId UNIQUEIDENTIFIER = NEWID();

-- İlk admin kullanıcıyı bul
SELECT TOP 1 @AdminUserId = Id
FROM Users
ORDER BY CreatedAt ASC;

IF @AdminUserId IS NULL
BEGIN
    RAISERROR('Admin kullanıcı bulunamadı. Önce uygulamadan İlk Kurulum yapın.', 16, 1);
    RETURN;
END

-- Test firma ekle
INSERT INTO Companies
(
    Id, Code, Name, TaxNumber, TaxOffice, Address, Phone, Email,
    ServerName, DatabaseName, DbUserName, DbPasswordEncrypted,
    IntegratedSecurity, IsActive,
    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
VALUES
(
    @CompanyId,
    'TEST01',
    'Test A.Ş.',
    '1234567890',
    'İstanbul',
    'Test Adres',
    '02120000000',
    'info@test.com',
    '.',
    'TestA_Db',
    '',
    '',
    1,  -- IntegratedSecurity = true
    1,  -- IsActive = true
    GETUTCDATE(), 'seed', GETUTCDATE(), 'seed'
);

-- Admin kullanıcı ile ilişkilendir (default firma)
INSERT INTO UserCompanies
(
    Id, UserId, CompanyId, IsDefault
)
VALUES
(
    NEWID(),
    @AdminUserId,
    @CompanyId,
    1
);

PRINT 'Test firma eklendi. CompanyId: ' + CAST(@CompanyId AS NVARCHAR(50));
GO

-- Firma DB'sini oluştur (boş)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TestA_Db')
BEGIN
    CREATE DATABASE TestA_Db;
    PRINT 'TestA_Db oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'TestA_Db zaten mevcut.';
END
GO
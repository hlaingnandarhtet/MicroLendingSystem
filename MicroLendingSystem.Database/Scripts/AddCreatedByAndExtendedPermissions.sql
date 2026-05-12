/*
  Run after backup. Adds CreatedById to Borrowers/Loans, inserts Permissions 21–30 if missing, RolePermissions for Admin/Staff.
*/

SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.Borrowers', N'CreatedById') IS NULL
BEGIN
    ALTER TABLE dbo.Borrowers ADD CreatedById INT NULL;
    ALTER TABLE dbo.Borrowers ADD CONSTRAINT FK_Borrowers_Users_CreatedBy
        FOREIGN KEY (CreatedById) REFERENCES dbo.Users(Id) ON DELETE SET NULL;
END;

IF COL_LENGTH(N'dbo.Loans', N'CreatedById') IS NULL
BEGIN
    ALTER TABLE dbo.Loans ADD CreatedById INT NULL;
    ALTER TABLE dbo.Loans ADD CONSTRAINT FK_Loans_Users_CreatedBy
        FOREIGN KEY (CreatedById) REFERENCES dbo.Users(Id) ON DELETE SET NULL;
END;

DECLARE @adminUserId INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Email = N'admin@gmail.com' ORDER BY Id);
IF @adminUserId IS NULL SET @adminUserId = (SELECT TOP 1 Id FROM dbo.Users ORDER BY Id);

IF @adminUserId IS NOT NULL
BEGIN
    UPDATE dbo.Borrowers SET CreatedById = @adminUserId WHERE CreatedById IS NULL;
    UPDATE dbo.Loans SET CreatedById = @adminUserId WHERE CreatedById IS NULL;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'Borrower_Read')
    INSERT INTO dbo.Permissions (Name) VALUES (N'Borrower_Read');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'Borrower_Create')
    INSERT INTO dbo.Permissions (Name) VALUES (N'Borrower_Create');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'Borrower_Update')
    INSERT INTO dbo.Permissions (Name) VALUES (N'Borrower_Update');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'Borrower_Delete')
    INSERT INTO dbo.Permissions (Name) VALUES (N'Borrower_Delete');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'LoanSetting_Read')
    INSERT INTO dbo.Permissions (Name) VALUES (N'LoanSetting_Read');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'LoanSetting_Create')
    INSERT INTO dbo.Permissions (Name) VALUES (N'LoanSetting_Create');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'LoanSetting_Update')
    INSERT INTO dbo.Permissions (Name) VALUES (N'LoanSetting_Update');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'LoanSetting_Delete')
    INSERT INTO dbo.Permissions (Name) VALUES (N'LoanSetting_Delete');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'Transaction_List')
    INSERT INTO dbo.Permissions (Name) VALUES (N'Transaction_List');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'LoanRequest_List')
    INSERT INTO dbo.Permissions (Name) VALUES (N'LoanRequest_List');

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 1, p.Id FROM dbo.Permissions p
WHERE p.Name IN (
    N'Borrower_Read', N'Borrower_Create', N'Borrower_Update', N'Borrower_Delete',
    N'LoanSetting_Read', N'LoanSetting_Create', N'LoanSetting_Update', N'LoanSetting_Delete',
    N'Transaction_List', N'LoanRequest_List')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = 1 AND rp.PermissionId = p.Id);

DECLARE @staffId INT = 2;
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @staffId, p.Id FROM dbo.Permissions p
WHERE p.Name IN (
    N'Loan_Read', N'Loan_Create', N'Loan_Repay', N'Loan_Update', N'Loan_Delete',
    N'Borrower_Read', N'Borrower_Create', N'Borrower_Update',
    N'LoanSetting_Read', N'Transaction_List', N'LoanRequest_List')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = @staffId AND rp.PermissionId = p.Id);

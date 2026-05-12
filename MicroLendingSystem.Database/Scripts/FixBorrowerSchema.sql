-- FixBorrowerSchema.sql
-- Run this to ensure Borrowers table has nullable foreign keys and correct constraints.

SET NOCOUNT ON;

-- 1. Make columns nullable
ALTER TABLE dbo.Borrowers ALTER COLUMN DocumentId INT NULL;
ALTER TABLE dbo.Borrowers ALTER COLUMN UserId INT NULL;
ALTER TABLE dbo.Borrowers ALTER COLUMN CreatedById INT NULL;

-- 2. Drop existing FKs if they are too strict or named differently (optional but safer)
-- Note: Replace FK names if they differ in your database.
-- These names are from the migrations and scripts.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Borrowers__Docum__534D60F1')
    ALTER TABLE dbo.Borrowers DROP CONSTRAINT FK__Borrowers__Docum__534D60F1;

-- 3. Add FKs with ON DELETE SET NULL for DocumentId
ALTER TABLE dbo.Borrowers ADD CONSTRAINT FK_Borrowers_Documents 
    FOREIGN KEY (DocumentId) REFERENCES dbo.Documents(Id) ON DELETE SET NULL;

-- 4. Ensure UserId FK exists
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Borrowers_Users_UserId')
    ALTER TABLE dbo.Borrowers ADD CONSTRAINT FK_Borrowers_Users_UserId 
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL;

-- 5. Ensure CreatedById FK exists
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Borrowers_Users_CreatedBy')
    ALTER TABLE dbo.Borrowers ADD CONSTRAINT FK_Borrowers_Users_CreatedBy 
        FOREIGN KEY (CreatedById) REFERENCES dbo.Users(Id) ON DELETE SET NULL;

PRINT 'Borrower schema fixed successfully.';

-- Run against MicroLoanDB (or your target database) before using loan settings / disbursements.

IF OBJECT_ID(N'dbo.LoanSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoanSettings (
        Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PlanName      NVARCHAR(150)     NOT NULL,
        InterestRate  DECIMAL(5,2)      NOT NULL,
        LoanTerm      INT               NOT NULL,
        CalculationType INT             NOT NULL,
        CreatedAt     DATETIME          NOT NULL DEFAULT (GETUTCDATE()),
        UpdatedAt     DATETIME          NULL,
        IsDeleted     BIT               NOT NULL DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.Disbursements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Disbursements (
        Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoanId           INT               NOT NULL,
        Amount           DECIMAL(18,2)   NOT NULL,
        TransactionType  INT               NOT NULL,
        TransactionDate  DATE              NOT NULL,
        CreatedAt        DATETIME          NOT NULL DEFAULT (GETUTCDATE()),
        IsDeleted        BIT               NOT NULL DEFAULT (0),
        CONSTRAINT FK_Disbursements_Loans FOREIGN KEY (LoanId) REFERENCES dbo.Loans (Id)
    );
END
GO

IF COL_LENGTH(N'dbo.Loans', N'LoanSettingId') IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.LoanSettings)
        INSERT INTO dbo.LoanSettings (PlanName, InterestRate, LoanTerm, CalculationType, IsDeleted)
        VALUES (N'Default Plan', 12.00, 12, 1, 0);

    DECLARE @DefaultSettingId INT = (SELECT TOP (1) Id FROM dbo.LoanSettings ORDER BY Id);

    ALTER TABLE dbo.Loans ADD LoanSettingId INT NULL;
    ALTER TABLE dbo.Loans ADD CalculationType INT NOT NULL DEFAULT (1);
    ALTER TABLE dbo.Loans ADD TotalRepayableAmount DECIMAL(18,2) NULL;
    ALTER TABLE dbo.Loans ADD RemainingBalance DECIMAL(18,2) NULL;

    UPDATE dbo.Loans SET LoanSettingId = @DefaultSettingId WHERE LoanSettingId IS NULL;

    ALTER TABLE dbo.Loans ALTER COLUMN LoanSettingId INT NOT NULL;

    ALTER TABLE dbo.Loans ADD CONSTRAINT FK_Loans_LoanSettings FOREIGN KEY (LoanSettingId) REFERENCES dbo.LoanSettings (Id);
END
GO

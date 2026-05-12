-- ============================================================
-- Drop legacy Disbursements / Repayments tables (if present).
-- Disbursements and repayments are recorded in dbo.Transactions.
-- Run ONCE against MicroLoanDB after migrating data if needed.
-- ============================================================

USE [MicroLoanDB];
GO

IF OBJECT_ID(N'dbo.Repayments', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Repayments];
END
GO

IF OBJECT_ID(N'dbo.Disbursements', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Disbursements];
END
GO

PRINT 'DropLegacyDisbursementRepaymentTables completed.';
GO

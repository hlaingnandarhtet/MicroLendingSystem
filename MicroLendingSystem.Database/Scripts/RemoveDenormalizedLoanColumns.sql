-- ============================================================
-- Migration: RemoveDenormalizedLoanColumns
-- Description: Drop InterestRate and LoanTerm from the Loans
--   table. These values are now always read from the linked
--   LoanSetting record via the LoanSettingId FK.
--   Also tightens LoanSettingId to NOT NULL.
-- Run ONCE against MicroLoanDB.
-- ============================================================

USE [MicroLoanDB];
GO

-- 1. Make LoanSettingId NOT NULL
--    (requires all existing rows already have a value).
ALTER TABLE [dbo].[Loans]
    ALTER COLUMN [LoanSettingId] INT NOT NULL;
GO

-- 2. Drop the denormalized columns.
ALTER TABLE [dbo].[Loans]
    DROP COLUMN [InterestRate];
GO

ALTER TABLE [dbo].[Loans]
    DROP COLUMN [LoanTerm];
GO

PRINT 'Migration RemoveDenormalizedLoanColumns completed successfully.';
GO

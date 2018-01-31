IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionDate
GO

CREATE VIEW [dbo].[AnalyticsDimFinancialTransactionDate]
AS
SELECT d.DateKey AS [TransactionDateKey]
    ,*
FROM AnalyticsSourceDate d
WHERE d.DateKey >= (
        SELECT MIN(x.TransactionDateKey)
        FROM AnalyticsFactFinancialTransaction x
        )
    AND d.DateKey <= (
        SELECT MAX(x.TransactionDateKey)
        FROM AnalyticsFactFinancialTransaction x
        )
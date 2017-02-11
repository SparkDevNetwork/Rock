IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionDate
GO

CREATE VIEW [dbo].[AnalyticsDimFinancialTransactionDate]
AS
SELECT d.DateKey AS [TransactionDateKey]
    ,*
FROM AnalyticsDimDate d
WHERE d.DateKey >= (
        SELECT MIN(ft.TransactionDateKey)
        FROM AnalyticsFactFinancialTransaction ft
        )
    AND d.DateKey <= (
        SELECT MAX(ft.TransactionDateKey)
        FROM AnalyticsFactFinancialTransaction ft
        )
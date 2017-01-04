IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionType]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionType
GO

CREATE VIEW AnalyticsDimFinancialTransactionType
AS
SELECT dv.Id [TransactionTypeId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = 'FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5'
IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionSource]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionSource
GO

CREATE VIEW AnalyticsDimFinancialTransactionSource
AS
SELECT dv.Id [CurrencyTypeId]
    ,dv.Value [Name]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '4F02B41E-AB7D-4345-8A97-3904DDD89B01'

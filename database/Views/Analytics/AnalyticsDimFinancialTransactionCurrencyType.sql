IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionCurrencyType]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionCurrencyType
GO

CREATE VIEW AnalyticsDimFinancialTransactionCurrencyType
AS
SELECT dv.Id [CurrencyTypeId]
    ,dv.Value [Name]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '1D1304DE-E83A-44AF-B11D-0C66DD600B81'

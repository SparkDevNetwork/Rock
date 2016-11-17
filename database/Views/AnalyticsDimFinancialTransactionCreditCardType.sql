IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionCreditCardType]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionCreditCardType
GO

CREATE VIEW AnalyticsDimFinancialTransactionCreditCardType
AS
SELECT dv.Id [CreditCardTypeId]
    ,dv.Value [Name]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9'

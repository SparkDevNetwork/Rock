IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialAccount]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialAccount
GO

CREATE VIEW AnalyticsDimFinancialAccount
AS
SELECT fa.Id [AccountId]
    ,fa.[Name]
    ,fa.[PublicName]
    ,fa.[Description]
    ,fa.[PublicDescription]
    ,CASE fa.[IsTaxDeductible]
        WHEN 1
            THEN 'Taxable'
        ELSE 'Not Taxable'
        END [TaxStatus]
    ,fa.[GlCode]
    ,fa.[Order]
    ,CASE fa.IsActive
        WHEN 1
            THEN 'Active'
        ELSE 'Inctive'
        END [ActiveStatus]
    ,CASE fa.IsPublic
        WHEN 1
            THEN 'Public'
        ELSE 'Non Public'
        END [PublicStatus]
    ,fa.[StartDate]
    ,fa.[EndDate]
    ,dvAccountType.Value [AccountType]
    ,CASE 
        WHEN fa.ImageBinaryFileId IS NULL
            THEN ''
        ELSE CONCAT (
                'GetImage.ashx?id='
                ,fa.ImageBinaryFileId
                )
        END [ImageUrl]
    ,fa.[ImageBinaryFileId]
    ,fa.[Url]
    ,c.Name [CampusName]
	,c.ShortCode [CampusShortCode]
    ,fa.[ParentAccountId]
FROM FinancialAccount fa
LEFT JOIN DefinedValue dvAccountType ON fa.AccountTypeValueId = dvAccountType.Id
LEFT JOIN Campus c ON fa.CampusId = c.Id
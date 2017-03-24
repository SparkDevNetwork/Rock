IF OBJECT_ID(N'[dbo].[AnalyticsFactFinancialTransaction]', 'V') IS NOT NULL
    DROP VIEW AnalyticsFactFinancialTransaction
GO

--select count(*) from AnalyticsFactFinancialTransaction 2185726
CREATE VIEW AnalyticsFactFinancialTransaction
AS
SELECT asft.*
    ,isnull(tt.NAME, 'None') [TransactionType]
    ,isnull(ts.Value, 'None') [TransactionSource]
    ,CASE asft.IsScheduled
        WHEN 1
            THEN 'Scheduled'
        ELSE 'Non-Scheduled'
        END [ScheduleType]
    ,adfcAuthorizedFamilyKey.Id [AuthorizedFamilyKey]
	,adpcProcessedByPerson.Id [ProcessedByPersonKey]
    ,adfcGivingUnit.Id [GivingUnitKey]
    ,isnull(fg.NAME, 'None') [FinancialGateway]
    ,isnull(et.FriendlyName, 'None') [EntityTypeName]
    ,isnull(ct.Value, 'None') [CurrencyType]
    ,isnull(cct.Value, 'None') [CreditCardType]
FROM AnalyticsSourceFinancialTransaction asft
JOIN AnalyticsDimFinancialTransactionType tt ON tt.TransactionTypeId = asft.TransactionTypeValueId
LEFT JOIN DefinedValue ts ON ts.Id = asft.SourceTypeValueId
LEFT JOIN DefinedValue ct ON ct.Id = asft.CurrencyTypeValueId
LEFT JOIN DefinedValue cct ON cct.Id = asft.CreditCardTypeValueId
LEFT JOIN PersonAlias paProcessedByPerson ON asft.ProcessedByPersonAliasId = paProcessedByPerson.Id
LEFT JOIN AnalyticsDimPersonCurrent adpcProcessedByPerson ON adpcProcessedByPerson.PersonId = paProcessedByPerson.PersonId
LEFT JOIN AnalyticsDimFamilyCurrent adfcGivingUnit ON adfcGivingUnit.FamilyId = asft.GivingGroupId
LEFT JOIN AnalyticsDimFamilyCurrent adfcAuthorizedFamilyKey ON adfcAuthorizedFamilyKey.FamilyId = asft.AuthorizedFamilyId
LEFT JOIN FinancialGateway fg ON asft.FinancialGatewayId = fg.Id
LEFT JOIN EntityType et ON et.Id = asft.EntityTypeId
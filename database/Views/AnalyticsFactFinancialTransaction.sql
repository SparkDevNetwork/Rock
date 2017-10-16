IF OBJECT_ID(N'[dbo].[AnalyticsFactFinancialTransaction]', 'V') IS NOT NULL
    DROP VIEW AnalyticsFactFinancialTransaction
GO

CREATE VIEW AnalyticsFactFinancialTransaction
AS
SELECT asft.*
    ,isnull(tt.Value, 'None') [TransactionType]
    ,isnull(ts.Value, 'None') [TransactionSource]
    ,CASE asft.IsScheduled
        WHEN 1
            THEN 'Scheduled'
        ELSE 'Non-Scheduled'
        END [ScheduleType]
    ,adphAuthorizedPerson.PrimaryFamilyKey [AuthorizedFamilyKey]
	,adpcAuthorizedPerson.PrimaryFamilyKey [AuthorizedCurrentFamilyKey]
	,adpcProcessedByPerson.Id [ProcessedByPersonKey]
	,adfcGivingUnit.Id [GivingUnitKey]
	,adfcCurrentGivingUnit.Id [GivingUnitCurrentKey]
    ,isnull(fg.NAME, 'None') [FinancialGateway]
    ,isnull(et.FriendlyName, 'None') [EntityTypeName]
    ,isnull(ct.Value, 'None') [CurrencyType]
    ,isnull(cct.Value, 'None') [CreditCardType]
FROM AnalyticsSourceFinancialTransaction asft
LEFT JOIN DefinedValue tt ON tt.Id = asft.TransactionTypeValueId
LEFT JOIN DefinedValue ts ON ts.Id = asft.SourceTypeValueId
LEFT JOIN DefinedValue ct ON ct.Id = asft.CurrencyTypeValueId
LEFT JOIN DefinedValue cct ON cct.Id = asft.CreditCardTypeValueId
LEFT JOIN PersonAlias paProcessedByPerson ON asft.ProcessedByPersonAliasId = paProcessedByPerson.Id
LEFT JOIN AnalyticsDimPersonCurrent adpcProcessedByPerson ON adpcProcessedByPerson.PersonId = paProcessedByPerson.PersonId
LEFT JOIN AnalyticsDimPersonCurrent adpcAuthorizedPerson ON adpcAuthorizedPerson.Id = asft.AuthorizedCurrentPersonKey
LEFT JOIN AnalyticsDimPersonHistorical adphAuthorizedPerson ON adphAuthorizedPerson.Id = asft.AuthorizedPersonKey
LEFT JOIN AnalyticsDimFamilyCurrent adfcCurrentGivingUnit ON adfcCurrentGivingUnit.FamilyId = adpcAuthorizedPerson.GivingGroupId
LEFT JOIN AnalyticsDimFamilyCurrent adfcGivingUnit ON adfcGivingUnit.FamilyId = asft.GivingGroupId
LEFT JOIN FinancialGateway fg ON asft.FinancialGatewayId = fg.Id
LEFT JOIN EntityType et ON et.Id = asft.EntityTypeId
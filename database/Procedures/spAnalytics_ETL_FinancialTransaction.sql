IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_FinancialTransaction]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_FinancialTransaction
GO

--
CREATE PROCEDURE [dbo].[spAnalytics_ETL_FinancialTransaction]
AS
BEGIN
    -- insert records into [AnalyticsSourceFinancialTransaction] from the source FinancialTransaction/FinancialTransactionDetail tables that haven't been added yet
	INSERT INTO [dbo].[AnalyticsSourceFinancialTransaction] (
        [TransactionKey]
        ,[TransactionDateKey]
        ,[TransactionDateTime]
        ,[TransactionCode]
        ,[Summary]
        ,[TransactionTypeValueId]
        ,[SourceTypeValueId]
        ,[IsScheduled]
        ,[AuthorizedPersonAliasId]
        ,[ProcessedByPersonAliasId]
        ,[ProcessedDateTime]
        ,[GivingGroupId]
        ,[GivingId]
        ,[BatchId]
        ,[FinancialGatewayId]
        ,[EntityTypeId]
        ,[EntityId]
        ,[TransactionId]
        ,[TransactionDetailId]
        ,[AccountId]
        ,[CurrencyTypeValueId]
        ,[CreditCardTypeValueId]
        ,[DaysSinceLastTransactionOfType]
        ,[IsFirstTransactionOfType]
        ,[AuthorizedFamilyId]
        ,[Amount]
        ,[ModifiedDateTime]
        ,[Guid]
        )
    SELECT *
    FROM (
        SELECT CONCAT (
                ft.Id
                ,'_'
                ,ftd.Id
                ) [TransactionKey]
            ,convert(INT, (convert(CHAR(8), ft.TransactionDateTime, 112))) [TransactionDateKey]
            ,ft.TransactionDateTime
            ,ft.TransactionCode [TransactionCode]
            ,ft.Summary [TransactionSummary]
            ,ft.TransactionTypeValueId [TransactionTypeValueId]
            ,ft.SourceTypeValueId [SourceTypeValueId]
            ,CASE ft.ScheduledTransactionId
                WHEN NULL
                    THEN 0
                ELSE 1
                END [IsScheduled]
            ,ft.AuthorizedPersonAliasId
            ,ft.ProcessedByPersonAliasId
            ,ft.ProcessedDateTime
            ,p.GivingGroupId
            ,p.GivingId
            ,ft.BatchId
            ,ft.FinancialGatewayId
            ,ftd.EntityTypeId
            ,ftd.EntityId
            ,ft.Id [TransactionId]
            ,ftd.Id [TransactionDetailId]
            ,ftd.AccountId
            ,fpd.CurrencyTypeValueId
            ,fpd.CreditCardTypeValueId
            ,NULL [DaysSinceLastTransactionOfType] -- come back and fill this in later
            ,0 [IsFirstTransactionOfType] -- come back and fill this in later
            ,NULL [AuthorizedFamilyId] -- fill this in later
            --TODO: ,1 [Count]
            ,ftd.Amount [Amount]
            ,ftd.ModifiedDateTime
            ,NEWID() [Guid]
        FROM FinancialTransaction ft
        JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
        JOIN PersonAlias paAuthorizedPerson ON ft.AuthorizedPersonAliasId = paAuthorizedPerson.Id
        JOIN Person p ON p.Id = paAuthorizedPerson.PersonId
        LEFT JOIN EntityType et ON ftd.EntityTypeId = et.Id
        LEFT JOIN FinancialPaymentDetail fpd ON ft.FinancialPaymentDetailId = fpd.Id
        ) x
    WHERE x.TransactionKey NOT IN (
            SELECT TransactionKey
            FROM AnalyticsSourceFinancialTransaction
            )

	-- remove records from AnalyticsSourceFinancialTransaction that no longer exist in the source FinancialTransaction/FinancialTransactionDetail tables
    DELETE
    FROM AnalyticsSourceFinancialTransaction
    WHERE TransactionKey NOT IN (
            SELECT CONCAT (
                    ft.Id
                    ,'_'
                    ,ftd.Id
                    ) [TransactionKey]
            FROM FinancialTransaction ft
            JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
            )


	-- TODO what about modified records
END

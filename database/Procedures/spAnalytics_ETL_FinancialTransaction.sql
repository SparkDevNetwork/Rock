IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_FinancialTransaction]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_FinancialTransaction
GO

-- EXECUTE [dbo].[spAnalytics_ETL_FinancialTransaction] 
CREATE PROCEDURE [dbo].[spAnalytics_ETL_FinancialTransaction]
AS
BEGIN
    DECLARE @MinDateTime DATETIME = DATEFROMPARTS(1900, 1, 1)
        ,@EtlDateTime DATETIME = SysDateTime();

    -- insert records into [AnalyticsSourceFinancialTransaction] from the source FinancialTransaction/FinancialTransactionDetail 
    -- tables that haven't been added yet
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
        ,[Count]
        ,[Amount]
        ,[ModifiedDateTime]
        ,[Guid]
        )
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
        ,NULL [AuthorizedFamilyId] -- TODo: fill this in later
        ,1 [Count]
        ,ftd.Amount [Amount]
        ,@EtlDateTime [ModifiedDateTime]
        ,NEWID() [Guid]
    FROM FinancialTransaction ft
    JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
    JOIN PersonAlias paAuthorizedPerson ON ft.AuthorizedPersonAliasId = paAuthorizedPerson.Id
    JOIN Person p ON p.Id = paAuthorizedPerson.PersonId
    LEFT JOIN EntityType et ON ftd.EntityTypeId = et.Id
    LEFT JOIN FinancialPaymentDetail fpd ON ft.FinancialPaymentDetailId = fpd.Id
    WHERE ft.TransactionDateTime > @MinDateTime
        AND CONCAT (
            ft.Id
            ,'_'
            ,ftd.Id
            ) NOT IN (
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

    -- figure out IsFirstTransaction
    UPDATE AnalyticsSourceFinancialTransaction
    SET IsFirstTransactionOfType = 1
        ,DaysSinceLastTransactionOfType = NULL
    WHERE Id IN (
            SELECT asft.Id
            FROM (
                SELECT min(ft.TransactionDateTime) [DateTimeOfFirstTransactionOfType]
                    ,ft.TransactionTypeValueId
                    ,ft.GivingId
                FROM AnalyticsSourceFinancialTransaction ft
                GROUP BY ft.TransactionTypeValueId
                    ,GivingId
                ) firstTran
            CROSS APPLY (
                SELECT min(asft.Id) [Id]
                FROM AnalyticsSourceFinancialTransaction asft
                WHERE asft.GivingId = firstTran.GivingId
                    AND asft.TransactionTypeValueId = firstTran.TransactionTypeValueId
                    AND asft.TransactionDateTime = firstTran.DateTimeOfFirstTransactionOfType
                ) asft
            )
        AND IsFirstTransactionOfType = 0

    -- Update [DaysSinceLastTransactionOfType]
    -- get the number of days since the last transaction of this giving group of the same TransactionType
    -- but don't count it as a previous transaction if it was on the same date
    -- To optimize, add a WHERE DaysSinceLastTransactionOfType is NULL, but at the risk of the number being wrong due to a new transaction with an earlier date getting added 
    UPDATE asft
    SET DaysSinceLastTransactionOfType = x.[CalcDaysSinceLastTransactionOfType]
    FROM AnalyticsSourceFinancialTransaction asft
    CROSS APPLY (
        SELECT TOP 1 DATEDIFF(day, previousTran.TransactionDateTime, asft.TransactionDateTime) [CalcDaysSinceLastTransactionOfType]
        FROM AnalyticsSourceFinancialTransaction previousTran
        WHERE previousTran.GivingId = asft.GivingId
            AND previousTran.TransactionTypeValueId = asft.TransactionTypeValueId
            AND convert(DATE, previousTran.TransactionDateTime) < convert(DATE, asft.TransactionDateTime)
        ORDER BY previousTran.TransactionDateTime DESC
        ) x
        
		
	-- TODO update [AuthorizedFamilyId]
    -- TODO what about modified records
END

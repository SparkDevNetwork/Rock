IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_FinancialTransaction]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_FinancialTransaction
GO

-- truncate table [AnalyticsSourceFinancialTransaction]
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
        ,[TransactionFrequency]
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
        ,CASE 
            WHEN ft.ScheduledTransactionId IS NULL
                THEN 0
            ELSE 1
            END [IsScheduled]
        ,dvtf.Value [TransactionFrequency]
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
    LEFT JOIN FinancialScheduledTransaction fst ON ft.ScheduledTransactionId = fst.Id
    LEFT JOIN DefinedValue dvtf ON fst.TransactionFrequencyValueId = dvtf.Id
    WHERE ft.TransactionDateTime > @MinDateTime
        AND ftd.Id NOT IN (
            SELECT TransactionDetailId
            FROM AnalyticsSourceFinancialTransaction
            )

    -- remove records from AnalyticsSourceFinancialTransaction that no longer exist in the source FinancialTransaction/FinancialTransactionDetail tables
    DELETE
    FROM AnalyticsSourceFinancialTransaction
    WHERE TransactionDetailId NOT IN (select Id from FinancialTransactionDetail)

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
        AND IsFirstTransactionOfType != 1

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
    WHERE isnull(asft.DaysSinceLastTransactionOfType, 0) != x.CalcDaysSinceLastTransactionOfType

    -- update just in case any records where modified since originally inserted
    UPDATE asft
    SET asft.[TransactionKey] = x.[TransactionKey]
        ,asft.[TransactionDateKey] = x.[TransactionDateKey]
        ,asft.[TransactionDateTime] = x.[TransactionDateTime]
        ,asft.[TransactionCode] = x.[TransactionCode]
        ,asft.[Summary] = x.[Summary]
        ,asft.[TransactionTypeValueId] = x.[TransactionTypeValueId]
        ,asft.[SourceTypeValueId] = x.[SourceTypeValueId]
        ,asft.[IsScheduled] = x.[IsScheduled]
        ,asft.[TransactionFrequency] = x.[TransactionFrequency]
        ,asft.[AuthorizedPersonAliasId] = x.[AuthorizedPersonAliasId]
        ,asft.[ProcessedByPersonAliasId] = x.[ProcessedByPersonAliasId]
        ,asft.[ProcessedDateTime] = x.[ProcessedDateTime]
        ,asft.[GivingGroupId] = x.[GivingGroupId]
        ,asft.[GivingId] = x.[GivingId]
        ,asft.[BatchId] = x.[BatchId]
        ,asft.[FinancialGatewayId] = x.[FinancialGatewayId]
        ,asft.[EntityTypeId] = x.[EntityTypeId]
        ,asft.[EntityId] = x.[EntityId]
        ,asft.[AccountId] = x.[AccountId]
        ,asft.[CurrencyTypeValueId] = x.[CurrencyTypeValueId]
        ,asft.[CreditCardTypeValueId] = x.[CreditCardTypeValueId]
        ,asft.[AuthorizedFamilyId] = x.[AuthorizedFamilyId]
        ,asft.[Amount] = x.[Amount]
		,asft.ModifiedDateTime = @EtlDateTime
    FROM [AnalyticsSourceFinancialTransaction] asft
    JOIN (
        SELECT CONCAT (
                ft.Id
                ,'_'
                ,ftd.Id
                ) [TransactionKey]
            ,convert(INT, (convert(CHAR(8), ft.TransactionDateTime, 112))) [TransactionDateKey]
            ,ft.TransactionDateTime
            ,ft.TransactionCode [TransactionCode]
            ,ft.Summary [Summary]
            ,ft.TransactionTypeValueId [TransactionTypeValueId]
            ,ft.SourceTypeValueId [SourceTypeValueId]
      ,CASE 
                WHEN ft.ScheduledTransactionId IS NULL
                    THEN 0
                ELSE 1
                END [IsScheduled]
            ,dvtf.Value [TransactionFrequency]
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
        LEFT JOIN FinancialScheduledTransaction fst ON ft.ScheduledTransactionId = fst.Id
        LEFT JOIN DefinedValue dvtf ON fst.TransactionFrequencyValueId = dvtf.Id
        WHERE ft.TransactionDateTime > @MinDateTime
        ) x ON x.[TransactionKey] = asft.[TransactionKey]
        AND (
            asft.[TransactionKey] != x.[TransactionKey]
            OR asft.[TransactionDateKey] != x.[TransactionDateKey]
            OR asft.[TransactionDateTime] != x.[TransactionDateTime]
            OR asft.[TransactionCode] != x.[TransactionCode]
            OR asft.[Summary] != x.[Summary]
            OR asft.[TransactionTypeValueId] != x.[TransactionTypeValueId]
            OR asft.[SourceTypeValueId] != x.[SourceTypeValueId]
            OR asft.[IsScheduled] != x.[IsScheduled]
            OR asft.[TransactionFrequency] != x.[TransactionFrequency]
            OR asft.[AuthorizedPersonAliasId] != x.[AuthorizedPersonAliasId]
            OR asft.[ProcessedByPersonAliasId] != x.[ProcessedByPersonAliasId]
            OR asft.[ProcessedDateTime] != x.[ProcessedDateTime]
            OR asft.[GivingGroupId] != x.[GivingGroupId]
            OR asft.[GivingId] != x.[GivingId]
            OR asft.[BatchId] != x.[BatchId]
            OR asft.[FinancialGatewayId] != x.[FinancialGatewayId]
            OR asft.[EntityTypeId] != x.[EntityTypeId]
            OR asft.[EntityId] != x.[EntityId]
            OR asft.[AccountId] != x.[AccountId]
            OR asft.[CurrencyTypeValueId] != x.[CurrencyTypeValueId]
            OR asft.[CreditCardTypeValueId] != x.[CreditCardTypeValueId]
            OR asft.[AuthorizedFamilyId] != x.[AuthorizedFamilyId]
            OR asft.[Amount] != x.[Amount]
            )

    /* Updating these PersonKeys depends on AnalyticsSourcePersonHistorical getting populated and updated. 
  -- It is probably best to schedule the ETL of AnalyticsSourcePersonHistorical to occur before spAnalytics_ETL_FinancialTransaction
  -- However, if not, it will catch up on the next run of spAnalytics_ETL_FinancialTransaction
  */
    -- Update PersonKeys for whatever PersonKey the person had at the time of the transaction
    UPDATE asft
    SET [AuthorizedPersonKey] = x.PersonKey
    FROM AnalyticsSourceFinancialTransaction asft
    CROSS APPLY (
        SELECT TOP 1 ph.Id [PersonKey]
        FROM AnalyticsSourcePersonHistorical ph
        JOIN PersonAlias pa ON asft.AuthorizedPersonAliasId = pa.Id
        WHERE ph.PersonId = pa.PersonId
            AND asft.[TransactionDateTime] < ph.[ExpireDate]
        ORDER BY ph.[ExpireDate] DESC
        ) x
    WHERE isnull(asft.AuthorizedPersonKey, 0) != x.PersonKey

    -- Update PersonKeys and AuthorizedFamilyId for whatever PersonKey/FamilyId is current right now
    UPDATE asft
    SET [AuthorizedCurrentPersonKey] = x.PersonKey
        ,[AuthorizedFamilyId] = x.PrimaryFamilyId
    FROM AnalyticsSourceFinancialTransaction asft
    CROSS APPLY (
        SELECT TOP 1 pc.Id [PersonKey]
            ,pc.PrimaryFamilyId
        FROM AnalyticsDimPersonCurrent pc
        JOIN PersonAlias pa ON asft.AuthorizedPersonAliasId = pa.Id
        WHERE pc.PersonId = pa.PersonId
        ) x
    WHERE isnull(asft.AuthorizedCurrentPersonKey, 0) != x.PersonKey
        OR (isnull(asft.AuthorizedFamilyId, 0) != x.PrimaryFamilyId)
END
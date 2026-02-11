ALTER PROCEDURE dbo.spGivingAutomation_UpdateGivingJourneyStages
    @CurrentRockDateTime DATETIME,
    @TransactionTypeIds dbo.IdList READONLY,                -- List of allowed TransactionTypeValueIds ( Defined in Giving Automation Configuration Block )
    @FinancialAccountIds dbo.IdList READONLY,               -- List of allowed FinancialAccounts ( Defined in Giving Automation Configuration Block )
    @NewGiverFirstGaveDays INT = 150,                       -- Max days since the first gift for someone to qualify for New Giver
    @NewGiverContributionCountBetweenMinimum INT = 1,       -- Minimum number of gifts for New Giver
    @NewGiverContributionCountBetweenMaximum INT = 5,       -- Maximum number of gifts for New Giver
    @ConsistentGiverLastGaveDays INT = 32,                  -- Maximum days since last gift to still be considered active for Consistent
    @ConsistentGiverMeanFrequency INT = 32,                 -- Max average days between gifts (last 2 years) to qualify for Consistent
    @OccasionalGiverLastGaveDays INT = 150,                 -- Maximum days since last gift to be considered Occasional/active but not consistent
    @OccasionalGiverMeanFrequency INT = 94,                 -- Max average days between gifts (last 2 years) to qualify for Occasional
    @LapsedGiverNoGiftDays INT = 150,                       -- Minimum days without a gift to be considered for Lapsed
    @LapsedGiverMeanFrequency INT = 100                     -- Max average days between gifts (last 2 years) to qualify for Lapsed
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME = DATEADD(MONTH, -24, @CurrentRockDateTime);
    DECLARE @JourneyChangeDateValue VARCHAR(30) = CONVERT(VARCHAR(30), @CurrentRockDateTime, 126);

    DECLARE @AttrId_CurrentJourneyStage INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = '13C55AEA-6D88-4470-B3AE-EE5138F044DF');
    DECLARE @AttrId_PreviousJourneyStage INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = 'B35CE867-6017-484E-9EC7-AEB93CD4B2D8');
    DECLARE @AttrId_JourneyStageChangeDate INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = '8FFE3554-43F2-40D8-8803-446559D2B1F7');

    DECLARE @RecordTypeId_Nameless INT = (SELECT TOP 1 Id FROM DefinedValue WHERE [Guid] = '721300ED-1267-4DA0-B4F2-6C6B5B17B1C5'); -- PERSON_RECORD_TYPE_NAMELESS
    DECLARE @RecordTypeId_RestUser INT = (SELECT TOP 1 Id FROM DefinedValue WHERE [Guid] = 'E2261A84-831D-4234-9BE0-4D628BBE751E'); -- PERSON_RECORD_TYPE_RESTUSER
    DECLARE @GiverAnonymousGuid UNIQUEIDENTIFIER = '802235DC-3CA5-94B0-4326-AACE71180F48'; -- GIVER_ANONYMOUS
    DECLARE @AnonymousVisitorGuid UNIQUEIDENTIFIER = '7EBC167B-512D-4683-9D80-98B6BB02E1B9'; -- ANONYMOUS_VISITOR

    -- =============================================
    -- CALCULATIONS
    -- =============================================
    ;WITH EligiblePersons AS (
        SELECT
            p.Id AS PersonId,
            p.GivingId
        FROM dbo.Person p
        WHERE p.GivingId IS NOT NULL
            AND p.GivingId <> ''
            AND (@RecordTypeId_Nameless IS NULL OR p.RecordTypeValueId <> @RecordTypeId_Nameless)
            AND (@RecordTypeId_RestUser IS NULL OR p.RecordTypeValueId <> @RecordTypeId_RestUser)
            AND p.[Guid] <> @GiverAnonymousGuid
            AND p.[Guid] <> @AnonymousVisitorGuid
    ),

    EligibleGivingIds AS (
        SELECT DISTINCT GivingId
        FROM EligiblePersons
    ),

    RefundTotals AS (
        SELECT
            ftr.OriginalTransactionId,
            SUM(ftd.Amount) AS TotalRefundedAmount
        FROM dbo.FinancialTransactionRefund ftr
        JOIN dbo.FinancialTransactionDetail ftd ON ftd.TransactionId = ftr.Id
        GROUP BY ftr.OriginalTransactionId
    ),

    EligibleTransactions AS (
        SELECT
            p.GivingId,
            ft.TransactionDateTime
        FROM dbo.FinancialTransaction ft
        JOIN dbo.PersonAlias pa ON pa.Id = ft.AuthorizedPersonAliasId
        JOIN dbo.Person p ON p.Id = pa.PersonId
        LEFT JOIN RefundTotals rt ON rt.OriginalTransactionId = ft.Id
        WHERE ft.TransactionDateTime <= @CurrentRockDateTime
            
            -- 1. Transaction type filter
            AND ft.TransactionTypeValueId IN (SELECT Id FROM @TransactionTypeIds)

            /*
               2. Account filter -- include the transaction if it has AT LEAST ONE line item
               in one of the allowed financial accounts.

               NOTE:
               - A FinancialTransaction may have multiple FinancialTransactionDetail rows (line items).
               - Some of those line items may belong to accounts NOT in the allowed list.
               - The transaction is still included as long as ONE line item is in an allowed account.
            */
            AND EXISTS (
                SELECT 1
                FROM dbo.FinancialTransactionDetail ftd_account
                WHERE ftd_account.TransactionId = ft.Id
                  AND ftd_account.AccountId IN (SELECT Id FROM @FinancialAccountIds)
            )

            -- 3. Must have at least one positive detail (across ALL details, not just filtered accounts)
            AND EXISTS (
                SELECT 1
                FROM dbo.FinancialTransactionDetail ftd_pos
                WHERE ftd_pos.TransactionId = ft.Id
                  AND ftd_pos.Amount > 0
            )

            -- 4. Refund logic: exclude only FULLY REFUNDED transactions
            AND (
                -- No refunds at all... include
                NOT EXISTS ( SELECT 1 FROM dbo.FinancialTransactionRefund ftr0 WHERE ftr0.OriginalTransactionId = ft.Id )
                OR
                (
                    -- Sum of ALL original txns details + sum of ALL refund details != 0... not fully refunded.
                    (SELECT SUM(ftd_all.Amount)
                     FROM dbo.FinancialTransactionDetail ftd_all
                     WHERE ftd_all.TransactionId = ft.Id)
                    + ISNULL(rt.TotalRefundedAmount, 0.00) <> 0
                )
            )

            -- 5. Meets person record requirements (at least one eligible person exists for this GivingId)
            AND EXISTS ( SELECT 1 FROM EligibleGivingIds eg WHERE eg.GivingId = p.GivingId )
    ),

    GivingStats AS (
        SELECT
            elg.GivingId,
            MIN(et.TransactionDateTime) AS FirstGiftDate,
            MAX(et.TransactionDateTime) AS LastGiftDate,
            MIN(CASE WHEN et.TransactionDateTime >= @StartDate THEN et.TransactionDateTime END) AS FirstGiftDate24Months,
            SUM(CASE WHEN et.TransactionDateTime >= @StartDate THEN 1 ELSE 0 END) AS GiftCount24Months
        FROM EligibleGivingIds elg
        LEFT JOIN EligibleTransactions et ON et.GivingId = elg.GivingId
        GROUP BY elg.GivingId
    ),

    Calculations AS (
        SELECT
            GivingId,
            FirstGiftDate,
            LastGiftDate,
            ISNULL(GiftCount24Months, 0) AS GiftCount24Months,
            CASE
                WHEN FirstGiftDate IS NOT NULL THEN 1
                ELSE 0
            END AS HasGivenEver,
           
           /*
                1/11/2026 - MSE

                "Ghost Transaction" Logic:

                If the days since the last transaction is greater than the average (mean)
                interval between gifts over the last two years, we assume the giving
                pattern has slowed.

                We simulate a "ghost" transaction at the current date to adjust the mean.

                Calculation Logic:
                    - Current Mean Interval = (LastTxnDate - FirstTxnDate) / (TxnCount - 1)
                    - DaysSinceLastTxn = (@CurrentRockDateTime - LastTxnDate)

                If DaysSinceLastTxn > Current Mean:
                    Adjusted Mean = ((LastTxnDate - FirstTxnDate) + DaysSinceLastTxn) / (TxnCount)

                Simplifies to:
                    (@CurrentRockDateTime - FirstTxnDate) / TxnCount
            */
            CASE
                WHEN ISNULL(GiftCount24Months, 0) < 2 THEN NULL

                WHEN DATEDIFF(DAY, LastGiftDate, @CurrentRockDateTime) >
                     (DATEDIFF(DAY, FirstGiftDate24Months, LastGiftDate) * 1.0 / (ISNULL(GiftCount24Months, 0) - 1))
                THEN DATEDIFF(DAY, FirstGiftDate24Months, @CurrentRockDateTime) * 1.0 / ISNULL(GiftCount24Months, 0)

                /*
                    Standard Calculation:
                        - Time since last transaction is within historical average interval.
                        - Mean = (LastTxnDate - FirstTxnDate) / (TxnCount - 1)
                */
                ELSE DATEDIFF(DAY, FirstGiftDate24Months, LastGiftDate) * 1.0 / (ISNULL(GiftCount24Months, 0) - 1)
            END AS MeanFrequency
        FROM GivingStats
    ),

    JourneyCalculations AS (
        SELECT
            GivingId,

            /*
                1/11/2026 - MSE

                Journey stage classification is computed once per GivingId, and all individuals associated 
                with that GivingId are assigned the same stage. With these updates to stage classification logic, 
                we should no longer encounter "unclassified" cases.

                Previous approach:
                    - Relied on ALL-TIME transaction data and MEDIAN giving frequency.
                    - Calculating the median over many years often masked recent changes in giving behavior.
                    
                    - Example: A donor who gave infrequently for many years but recently began giving monthly 
                      could take considerable time to reach the CONSISTENT category, since their historical giving habits 
                      keep the all-time median elevated, effectively slowing their progression and delaying recognition 
                      of their change in heart and behavior.
                    
                    - Gaps in the previous stage classification logic left a noticeable number of individuals unclassified,
                      not due to having no transactions, but because they didn't fall into any stage.
                    - Reference: https://github.com/SparkDevNetwork/Rock/issues/6419
            */
            CASE
                -- 0. NON-GIVER: No qualifying gifts.
                WHEN HasGivenEver = 0 THEN 0
                
                -- 1. NEW: First qualifying gift is within @NewGiverFirstGaveDays AND gift count is within configured range.
                WHEN FirstGiftDate IS NOT NULL
                     AND DATEDIFF(DAY, FirstGiftDate, @CurrentRockDateTime) <= @NewGiverFirstGaveDays
                     AND GiftCount24Months BETWEEN @NewGiverContributionCountBetweenMinimum AND @NewGiverContributionCountBetweenMaximum
                THEN 1
                
                -- 2. CONSISTENT: Last gift within @ConsistentGiverLastGaveDays AND MeanFrequency <= @ConsistentGiverMeanFrequency.
                WHEN DATEDIFF(DAY, LastGiftDate, @CurrentRockDateTime) <= @ConsistentGiverLastGaveDays
                     AND MeanFrequency <= @ConsistentGiverMeanFrequency
                THEN 2
                
                -- 3. OCCASIONAL: Last gift within @OccasionalGiverLastGaveDays AND MeanFrequency <= @OccasionalGiverMeanFrequency.
                WHEN DATEDIFF(DAY, LastGiftDate, @CurrentRockDateTime) <= @OccasionalGiverLastGaveDays
                     AND MeanFrequency <= @OccasionalGiverMeanFrequency
                THEN 3
                
                -- 4. LAPSED: Last Gift > @LapsedGiverNoGiftDays days AND Mean Frequency < @LapsedGiverMeanFrequency.
                WHEN DATEDIFF(DAY, LastGiftDate, @CurrentRockDateTime) > @LapsedGiverNoGiftDays 
                     AND MeanFrequency < @LapsedGiverMeanFrequency 
             THEN 4
                
                -- 5. FORMER: Catch-all for any remaining givers not classified above.
                ELSE 5
            END AS NewJourneyStage
        FROM Calculations
    )

    -- =============================================
    -- OUTPUT & MERGE INTO ATTRIBUTE VALUE
    -- =============================================

    -- This is only populated with people who's journey stage has changed.
    SELECT
        elg.PersonId,
        jc.NewJourneyStage,
        av.Value AS CurrentStageValue,
        TRY_CAST(av.Value AS INT) AS CurrentStageInt
    INTO #JourneyUpdates
    FROM JourneyCalculations jc
    JOIN EligiblePersons elg ON elg.GivingId = jc.GivingId
    LEFT JOIN dbo.AttributeValue av ON av.EntityId = elg.PersonId AND av.AttributeId = @AttrId_CurrentJourneyStage
    WHERE
        jc.NewJourneyStage != ISNULL(TRY_CAST(av.Value AS INT), -1);

    CREATE CLUSTERED INDEX IX_PersonId ON #JourneyUpdates (PersonId);

    -- Previous Journey Stage
    MERGE dbo.AttributeValue AS target
    USING (
        SELECT PersonId, ISNULL(CurrentStageValue, '') AS CurrentStageValue
        FROM #JourneyUpdates
    ) AS source
    ON target.EntityId = source.PersonId AND target.AttributeId = @AttrId_PreviousJourneyStage
    WHEN MATCHED THEN
        UPDATE SET Value = source.CurrentStageValue, ModifiedDateTime = @CurrentRockDateTime
    WHEN NOT MATCHED BY TARGET AND source.CurrentStageValue <> '' THEN
        INSERT (IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime)
        VALUES (0, @AttrId_PreviousJourneyStage, source.PersonId, source.CurrentStageValue, NEWID(), @CurrentRockDateTime, @CurrentRockDateTime);
    
    -- Current Journey Stage
    MERGE dbo.AttributeValue AS target
    USING (
        SELECT PersonId, CAST(NewJourneyStage AS VARCHAR(10)) AS NewStageValue
        FROM #JourneyUpdates
    ) AS source
    ON target.EntityId = source.PersonId AND target.AttributeId = @AttrId_CurrentJourneyStage
    WHEN MATCHED THEN
        UPDATE SET Value = source.NewStageValue, ModifiedDateTime = @CurrentRockDateTime
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime)
        VALUES (0, @AttrId_CurrentJourneyStage, source.PersonId, source.NewStageValue, NEWID(), @CurrentRockDateTime, @CurrentRockDateTime);
    
    -- Journey Stage Change Date
    MERGE dbo.AttributeValue AS target
    USING #JourneyUpdates AS source
    ON target.EntityId = source.PersonId AND target.AttributeId = @AttrId_JourneyStageChangeDate
    WHEN MATCHED THEN
        UPDATE SET Value = @JourneyChangeDateValue, ModifiedDateTime = @CurrentRockDateTime
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime)
        VALUES (0, @AttrId_JourneyStageChangeDate, source.PersonId, @JourneyChangeDateValue, NEWID(), @CurrentRockDateTime, @CurrentRockDateTime);
    
    -- Output
    SELECT COUNT(*) AS ChangesCount FROM #JourneyUpdates;
    
    DROP TABLE #JourneyUpdates;
END
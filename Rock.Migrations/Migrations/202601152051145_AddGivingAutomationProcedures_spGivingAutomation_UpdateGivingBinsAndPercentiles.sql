CREATE PROCEDURE dbo.spGivingAutomation_UpdateGivingBinsAndPercentiles
    @CurrentRockDateTime DATETIME,
    @TransactionTypeIds dbo.IdList READONLY,        -- List of allowed TransactionTypeValueIds ( Defined in Giving Automation Configuration Block )
    @FinancialAccountIds dbo.IdList READONLY        -- List of allowed FinancialAccounts ( Defined in Giving Automation Configuration Block )
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EndDate DATETIME = @CurrentRockDateTime;
    DECLARE @StartDate DATETIME = DATEADD(MONTH, -12, @EndDate);

    DECLARE @AttrId_GivingBin        INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = '7FBB63CC-F4FC-4F7E-A8C5-44DC3D0F0720');
    DECLARE @AttrId_GivingPercentile INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = 'D03ACAB8-EB0C-4835-A04C-4C357014D935');

    DECLARE @RecordTypeId_Nameless INT = (SELECT TOP 1 Id FROM DefinedValue WHERE [Guid] = '721300ED-1267-4DA0-B4F2-6C6B5B17B1C5'); -- PERSON_RECORD_TYPE_NAMELESS
    DECLARE @RecordTypeId_RestUser INT = (SELECT TOP 1 Id FROM DefinedValue WHERE [Guid] = 'E2261A84-831D-4234-9BE0-4D628BBE751E'); -- PERSON_RECORD_TYPE_RESTUSER
    DECLARE @GiverAnonymousGuid UNIQUEIDENTIFIER = '802235DC-3CA5-94B0-4326-AACE71180F48'; -- GIVER_ANONYMOUS

    -- =============================================
    -- CALCULATIONS
    -- =============================================

    ;WITH RefundTotals AS (
        SELECT 
            ftr.OriginalTransactionId,
            SUM(ftd.Amount) AS TotalRefundedAmount
        FROM dbo.FinancialTransactionRefund ftr
        JOIN dbo.FinancialTransactionDetail ftd ON ftd.TransactionId = ftr.Id
        GROUP BY ftr.OriginalTransactionId
    ),

    TwelveMonthTransactions AS (
        SELECT
            p.GivingId,
            ft.Id,
            SUM(ftd.Amount) + ISNULL(rt.TotalRefundedAmount, 0.00) AS NetAmount
        FROM dbo.FinancialTransaction ft
        JOIN dbo.PersonAlias pa ON pa.Id = ft.AuthorizedPersonAliasId
        JOIN dbo.Person p ON p.Id = pa.PersonId
        JOIN dbo.FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
        LEFT JOIN RefundTotals rt ON rt.OriginalTransactionId = ft.Id
        WHERE 
            ft.TransactionDateTime >= @StartDate
            AND ft.TransactionDateTime <= @CurrentRockDateTime
            
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

            -- NOTE: We intentionally do not apply all the person eligibility filters here.
            -- We include all GivingIds in the percentile distribution, and only apply eligibility filters later when updating AttributeValues.
            AND p.GivingId IS NOT NULL 
            AND p.GivingId <> ''
            AND p.[Guid] <> @GiverAnonymousGuid
        GROUP BY
            p.GivingId,
            ft.Id,
            rt.TotalRefundedAmount
    ),

    GiverTotals AS (
        SELECT 
            GivingId,
            SUM(NetAmount) AS Total12MonthGiving
        FROM TwelveMonthTransactions
        GROUP BY GivingId
    ),

    /*
        1/11/2026 - MSE

        Percentile Calculation

        Previous implementation:
        - Built a PercentileLowerRange[0..99] array of percentile cutoffs from the sorted 12-month giving totals.
        - Assigned a giver's percentile by finding which cutoff their total crossed.
        - This is an approximation (bucketed cutoffs), and not a direct percentile rank computed from the full distribution.

        - MAJOR FLAW: Percentile AttributeValues were updated ONLY for GivingIds with new or modified transactions since the last job run.
          Although the job rebuilt the global 12-month percentile cutoffs using everyone's current 12-month data, it did NOT update stored
          AttributeValues for GivingIds without new gifts. This caused some givers to retain high percentile values even after years of no giving,
          because their AttributeValue never changed unless they gave again.

        Current implementation (this procedure):
        - Recalculates Total12MonthGiving for all GivingIds on each run.
        - Uses a cumulative distribution (CUME_DIST) to compute each giver's percentile directly from the full ordered list of totals.
        - https://learn.microsoft.com/en-us/sql/t-sql/functions/cume-dist-transact-sql?view=sql-server-ver17
          
        - This produces a true percentile rank ("what percent of givers have total giving less than or equal to this giver").
        - Removes stale bin/percentile values for people who no longer have qualifying 12-month giving history.

        Why this is better:
        - Percentiles stay current as the overall giving distribution shifts -- changes to any giver should automatically affect everyone.
        - Stored AttributeValues are updated consistently, rather than only when a giver has new transactions.
    */

    GiverPercentiles AS (
        SELECT
            GivingId,
            Total12MonthGiving,
            CUME_DIST() OVER (ORDER BY Total12MonthGiving) AS Percentile
        FROM GiverTotals
    ),

    GiverBins AS (
        SELECT
            GivingId,
            Total12MonthGiving,
            Percentile,
            CASE
                WHEN Percentile >= 0.95 THEN 1  -- >= 95th percentile
                WHEN Percentile >= 0.80 THEN 2  -- >= 80th percentile
                WHEN Percentile >= 0.60 THEN 3  -- >= 60th percentile
                ELSE 4                          --  < 60th percentile
            END AS Bin
        FROM GiverPercentiles
    )

    SELECT
        p.Id AS PersonId,
        gb.Bin,

        /*
            1/11/2026 - MSE

            - CUME_DIST() returns a decimal in (0,1], so ROUND(Percentile * 100) results in 1-100.
            - We store the percentile as 0-99 by using FLOOR(Percentile * 100), and cap the maximum (Percentile = 1) at 99.
        */
        CASE
            WHEN gb.Percentile >= 1 THEN 99
            ELSE CAST(FLOOR(gb.Percentile * 100) AS INT)
        END AS PercentileValue
    INTO #GivingData
    FROM GiverBins gb
    JOIN dbo.Person p ON p.GivingId = gb.GivingId
    WHERE p.GivingId IS NOT NULL
      AND p.GivingId <> ''
      AND p.RecordTypeValueId IS NOT NULL
      AND ( @RecordTypeId_Nameless IS NULL OR p.RecordTypeValueId <> @RecordTypeId_Nameless )
      AND ( @RecordTypeId_RestUser IS NULL OR p.RecordTypeValueId <> @RecordTypeId_RestUser )
      AND p.[Guid] <> @GiverAnonymousGuid;

    CREATE CLUSTERED INDEX IX_PersonId ON #GivingData (PersonId);

    -- =============================================
    -- MERGE INTO ATTRIBUTE VALUE
    -- =============================================

    -- Giving Bin
    ;WITH CurrentBin AS (
        SELECT 
            av.EntityId AS PersonId,
            CAST(av.Value AS INT) AS CurrentBin
        FROM AttributeValue av
        WHERE av.AttributeId = @AttrId_GivingBin
    )
    MERGE AttributeValue AS target
    USING (
        SELECT gd.PersonId, CAST(gd.Bin AS VARCHAR(10)) AS NewValue
        FROM #GivingData gd
        LEFT JOIN CurrentBin cb ON cb.PersonId = gd.PersonId
        WHERE gd.Bin IS NOT NULL
          AND (cb.CurrentBin IS NULL OR cb.CurrentBin != gd.Bin)
    ) AS source ON (target.AttributeId = @AttrId_GivingBin AND target.EntityId = source.PersonId)
    WHEN MATCHED THEN
        UPDATE SET
            Value = source.NewValue,
            ModifiedDateTime = @CurrentRockDateTime
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (IsSystem, AttributeId, EntityId, Value, Guid, CreatedDateTime, ModifiedDateTime)
        VALUES (0, @AttrId_GivingBin, source.PersonId, source.NewValue, NEWID(), @CurrentRockDateTime, @CurrentRockDateTime);

    -- Giving Percentile
    ;WITH CurrentPercentile AS (
        SELECT 
            av.EntityId AS PersonId,
            CAST(av.Value AS INT) AS CurrentPercentile
        FROM AttributeValue av
        WHERE av.AttributeId = @AttrId_GivingPercentile
    )
    MERGE AttributeValue AS target
    USING (
        SELECT gd.PersonId, CAST(gd.PercentileValue AS VARCHAR(10)) AS NewValue
        FROM #GivingData gd
        LEFT JOIN CurrentPercentile cp ON cp.PersonId = gd.PersonId
        WHERE gd.PercentileValue IS NOT NULL
          AND (cp.CurrentPercentile IS NULL OR cp.CurrentPercentile != gd.PercentileValue)
    ) AS source ON (target.AttributeId = @AttrId_GivingPercentile AND target.EntityId = source.PersonId)
    WHEN MATCHED THEN
        UPDATE SET
            Value = source.NewValue,
            ModifiedDateTime = @CurrentRockDateTime
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (IsSystem, AttributeId, EntityId, Value, Guid, CreatedDateTime, ModifiedDateTime)
        VALUES (0, @AttrId_GivingPercentile, source.PersonId, source.NewValue, NEWID(), @CurrentRockDateTime, @CurrentRockDateTime);

    -- =============================================
    -- CLEAR STALE VALUES
    -- =============================================

    -- People who haven't given in the last 12 months should no longer have a bin/percentile.
    DELETE av
    FROM AttributeValue av
    JOIN dbo.Person p ON p.Id = av.EntityId
    WHERE av.AttributeId IN (@AttrId_GivingBin, @AttrId_GivingPercentile)
      AND p.GivingId IS NOT NULL
      AND p.GivingId <> ''
      AND p.RecordTypeValueId IS NOT NULL
      AND ( @RecordTypeId_Nameless IS NULL OR p.RecordTypeValueId <> @RecordTypeId_Nameless )
      AND ( @RecordTypeId_RestUser IS NULL OR p.RecordTypeValueId <> @RecordTypeId_RestUser )
      AND p.[Guid] <> @GiverAnonymousGuid
      AND NOT EXISTS ( SELECT 1 FROM #GivingData gd WHERE gd.PersonId = av.EntityId );

    DROP TABLE #GivingData;

END
GO
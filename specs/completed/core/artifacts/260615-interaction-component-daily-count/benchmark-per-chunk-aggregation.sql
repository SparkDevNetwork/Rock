-- =========================================================================
-- Benchmark: InteractionComponentDailyCount per-chunk aggregation
--
-- Measures the wall-clock and I/O cost of aggregating 30 days of Interaction
-- rows using the shape proposed in specs/260615-interaction-component-daily-count.md.
-- Output validates whether BackfillChunkDays = 30 is a workable default on
-- production-sized Rock instances.
--
-- NON-DESTRUCTIVE. SELECT-only. No INSERT, UPDATE, DELETE, or DDL.
-- Safe to run against production. The only "writes" are to a session-local
-- table variable, which is discarded when the batch ends.
--
-- HOW TO USE
--   1. In SSMS, press Ctrl+M to "Include Actual Execution Plan" before running.
--      The execution plan is the most important diagnostic if the query is slow.
--   2. Run the script. Stats output appears in the "Messages" tab.
--   3. Run it a SECOND time. First-pass timing includes cold buffer-pool reads;
--      the second pass is the warm-cache number. Report both.
--   4. Optional: change @ChunkLowerBound below to test other 30-day windows
--      (e.g. a historically heavy month, or 30 days from 5 years ago).
--
-- HOW TO READ THE OUTPUT
--   - "SQL Server Execution Times: ... elapsed time = N ms"           <-- wall clock
--   - "SQL Server Execution Times: ... CPU time = N ms"                <-- CPU
--   - "Table 'Interaction'. Scan count N, logical reads N, ..."        <-- I/O work
--   - Indexes used appear in the Actual Execution Plan tab; the goal is
--     to see IX_InteractionComponentId_InteractionDateTime drive the seek.
--
-- WHAT WE'RE WATCHING FOR
--   - Elapsed time fits comfortably inside RockCleanup's CommandTimeout x 4
--     (default 900s x 4 = 3600s). A 30-day chunk should be a small fraction
--     of that on healthy systems.
--   - The query plan drives FROM @EnabledComponents (loop join into
--     Interaction via IX_InteractionComponentId_InteractionDateTime), not
--     FROM Interaction (which would force a key lookup per row).
--   - PersonAlias is reached via PK seek per row, not a full scan.
-- =========================================================================

SET NOCOUNT ON;
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- -------------------------------------------------------------------------
-- Configuration
-- -------------------------------------------------------------------------
DECLARE @ChunkLowerBound DATETIME = DATEADD( DAY, -30, CAST( CAST( GETDATE() AS DATE ) AS DATETIME ) );
DECLARE @ChunkUpperBound DATETIME = CAST( CAST( GETDATE() AS DATE ) AS DATETIME );

DECLARE @AnonymousVisitorPersonId INT =
    ( SELECT [Id] FROM [Person] WHERE [Guid] = '7EBC167B-512D-4683-9D80-98B6BB02E1B9' );

PRINT '=== Benchmark configuration ===';
PRINT 'ChunkLowerBound:          ' + CONVERT( VARCHAR(30), @ChunkLowerBound, 121 );
PRINT 'ChunkUpperBound:          ' + CONVERT( VARCHAR(30), @ChunkUpperBound, 121 );
PRINT 'AnonymousVisitorPersonId: ' + ISNULL( CONVERT( VARCHAR(10), @AnonymousVisitorPersonId ), '<NULL>' );
PRINT '';

-- -------------------------------------------------------------------------
-- Sanity check: how many Interactions fall in this 30-day window?
-- Use this to scale expectations: a window with 200M interactions will
-- obviously take longer than a window with 2M.
-- -------------------------------------------------------------------------
PRINT '=== Sanity check: Interaction row count for window ===';

SELECT COUNT_BIG(*) AS [InteractionsInWindow]
FROM [Interaction]
WHERE [InteractionDateTime] >= @ChunkLowerBound
  AND [InteractionDateTime] <  @ChunkUpperBound;

-- -------------------------------------------------------------------------
-- Materialize the "enabled components" set the way the production task will.
--
-- NOTE: The new InteractionChannel.EnableComponentDailyCounts column does
-- not exist yet on production. For benchmarking, this script treats EVERY
-- InteractionComponent as eligible, which is the WORST CASE for the new
-- task. Real production runs will filter to a subset (channels whose
-- medium opted in: Website, Content Channel, URL Shortener, System Events,
-- Chat). If the benchmark runs acceptably with all components, the real
-- production query will only get faster.
-- -------------------------------------------------------------------------
DECLARE @EnabledComponents TABLE ( [Id] INT PRIMARY KEY );

INSERT INTO @EnabledComponents ( [Id] )
SELECT [Id] FROM [InteractionComponent];

DECLARE @EnabledComponentCount INT = ( SELECT COUNT(*) FROM @EnabledComponents );
PRINT '';
PRINT '=== Enabled component count (worst case = all components) ===';
PRINT 'Components in @EnabledComponents: ' + CONVERT( VARCHAR(10), @EnabledComponentCount );
PRINT '';

-- -------------------------------------------------------------------------
-- The aggregation, exactly the shape the new RockCleanup task will use for
-- the per-chunk INSERT, but emitted as a SELECT instead of an INSERT.
--
-- This is what we are timing. The ORDER BY at the end is intentional: it
-- mirrors the natural key of the destination table, so the spool/sort cost
-- is included in the timing (matches production reality).
-- -------------------------------------------------------------------------
PRINT '=== Per-chunk aggregation (SELECT-only equivalent of the INSERT) ===';

SELECT
    [src].[InteractionComponentId],
    [src].[InteractionDate],
    [src].[Operation],
    CONVERT( INT, CONVERT( VARCHAR(8), [src].[InteractionDate], 112 ) ) AS [InteractionDateKey],
    [src].[LoggedInInteractionCount],
    [src].[AnonymousInteractionCount],
    [src].[LoggedInSessionCount],
    [src].[AnonymousSessionCount],
    [src].[LoggedInInteractionCount] + [src].[AnonymousInteractionCount] AS [TotalInteractionCount],
    [src].[LoggedInSessionCount]      + [src].[AnonymousSessionCount]     AS [TotalSessionCount],
    [src].[AverageInteractionLength]
FROM (
    SELECT
        [i].[InteractionComponentId],
        CAST( [i].[InteractionDateTime] AS DATE )    AS [InteractionDate],
        ISNULL( [i].[Operation], '' )                 AS [Operation],

        SUM( CASE WHEN [pa].[PersonId] IS NOT NULL AND [pa].[PersonId] <> @AnonymousVisitorPersonId THEN 1 ELSE 0 END ) AS [LoggedInInteractionCount],
        SUM( CASE WHEN [pa].[PersonId] IS NULL     OR  [pa].[PersonId]  = @AnonymousVisitorPersonId THEN 1 ELSE 0 END ) AS [AnonymousInteractionCount],

        COUNT( DISTINCT CASE WHEN [pa].[PersonId] IS NOT NULL AND [pa].[PersonId] <> @AnonymousVisitorPersonId THEN [i].[InteractionSessionId] END ) AS [LoggedInSessionCount],
        COUNT( DISTINCT CASE WHEN [pa].[PersonId] IS NULL     OR  [pa].[PersonId]  = @AnonymousVisitorPersonId THEN [i].[InteractionSessionId] END ) AS [AnonymousSessionCount],

        -- Interaction.InteractionLength is double (float) in EF; CAST to keep the
        -- benchmark numerically equivalent to the production INSERT projection.
        AVG( CAST( [i].[InteractionLength] AS DECIMAL(18, 2) ) ) AS [AverageInteractionLength]
    FROM @EnabledComponents [ec]
    INNER JOIN [Interaction] [i]
        ON  [i].[InteractionComponentId] = [ec].[Id]
        AND [i].[InteractionDateTime]   >= @ChunkLowerBound
        AND [i].[InteractionDateTime]   <  @ChunkUpperBound
    LEFT JOIN [PersonAlias] [pa]
        ON  [pa].[Id] = [i].[PersonAliasId]
    GROUP BY
        [i].[InteractionComponentId],
        CAST( [i].[InteractionDateTime] AS DATE ),
        ISNULL( [i].[Operation], '' )
) AS [src]
ORDER BY
    [src].[InteractionComponentId],
    [src].[InteractionDate],
    [src].[Operation];

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;

-- =========================================================================
-- After running, please report back (in a comment on this artifact or in a
-- Performance validation section of the spec):
--   - Rock version and rough database size (Interaction row count overall)
--   - InteractionsInWindow value
--   - @EnabledComponents count
--   - Elapsed time (cold) and elapsed time (warm)
--   - CPU time (warm)
--   - Top-3 tables by logical reads from STATISTICS IO
--   - Whether the actual plan drove from @EnabledComponents (good) or
--     from Interaction with a date-range scan and key lookups (bad)
-- =========================================================================

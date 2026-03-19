-- =====================================================================================================
-- Author:        Rock
-- Create Date:
-- Modified Date: 03-13-2026
-- Description:   Populates [dbo].[BackgroundCheck] for semi-random people up to @BackgroundCheckCount.
--
-- Change History:
--                 03-13-2026 NA - Add configurable seed script for BackgroundCheck rows in mixed states.
-- =====================================================================================================

-- ===============================
-- Configuration
-- ===============================
DECLARE @BackgroundCheckCount INT = 1000;
DECLARE @CreatedByPersonAliasId INT = 10; -- why: background checks are typically seeded as if created by an admin/system user
DECLARE @ForeignId INT = 1; -- why: PMM uses ForeignId = 1, but callers can override this to mimic other providers
DECLARE @WorkflowTypeId INT = 18; -- set to NULL to skip WorkflowId assignment entirely
DECLARE @WorkflowAssignmentPercent TINYINT = 35; -- 0..100; only this % of generated rows will attempt to link to a workflow
DECLARE @RequestDateStart DATE = '2025-01-01';
DECLARE @RequestDateEnd   DATE = '2026-03-01';
DECLARE @PackageNamesCsv NVARCHAR(400) = N'basic,standard';
DECLARE @StatusesCsv NVARCHAR(400) = N'InvitationExpired,ReportCreated,ReportCompleted';
DECLARE @PopulateResponseData BIT = 1; -- why: completed/created rows feel more realistic when some response payload exists

-- ===============================
-- Preconditions
-- ===============================
IF OBJECT_ID('dbo.BackgroundCheck') IS NULL
BEGIN
    RAISERROR('dbo.BackgroundCheck not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.Person') IS NULL
BEGIN
    RAISERROR('dbo.Person not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.PersonAlias') IS NULL
BEGIN
    RAISERROR('dbo.PersonAlias not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.Workflow') IS NULL
BEGIN
    RAISERROR('dbo.Workflow not found.', 16, 1);
    RETURN;
END;

IF @BackgroundCheckCount <= 0
BEGIN
    RAISERROR('@BackgroundCheckCount must be greater than 0.', 16, 1);
    RETURN;
END;

IF @RequestDateEnd < @RequestDateStart
BEGIN
    RAISERROR('@RequestDateEnd must be on or after @RequestDateStart.', 16, 1);
    RETURN;
END;

IF @WorkflowAssignmentPercent NOT BETWEEN 0 AND 100
BEGIN
    RAISERROR('@WorkflowAssignmentPercent must be between 0 and 100.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.PersonAlias pa
    WHERE pa.Id = @CreatedByPersonAliasId
)
BEGIN
    RAISERROR('@CreatedByPersonAliasId does not exist in dbo.PersonAlias.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Person p
    WHERE p.PrimaryAliasId IS NOT NULL
)
BEGIN
    RAISERROR('No Person.PrimaryAliasId values were found. Please populate the [Person] table first.', 16, 1);
    RETURN;
END;

-- ===============================
-- Parse Configurable Options
-- ===============================
DECLARE @PackageOptions TABLE
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Value] NVARCHAR(100) NOT NULL
);

INSERT INTO @PackageOptions ([Value])
SELECT DISTINCT TOP (1000) LEFT(LTRIM(RTRIM([value])), 100)
FROM STRING_SPLIT(@PackageNamesCsv, ',')
WHERE LTRIM(RTRIM([value])) <> N'';

IF NOT EXISTS (SELECT 1 FROM @PackageOptions)
BEGIN
    RAISERROR('No package names were provided. Check @PackageNamesCsv.', 16, 1);
    RETURN;
END;

DECLARE @StatusOptions TABLE
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Value] NVARCHAR(25) NOT NULL
);

INSERT INTO @StatusOptions ([Value])
SELECT DISTINCT TOP (1000) LEFT(LTRIM(RTRIM([value])), 25)
FROM STRING_SPLIT(@StatusesCsv, ',')
WHERE LTRIM(RTRIM([value])) <> N'';

IF NOT EXISTS (SELECT 1 FROM @StatusOptions)
BEGIN
    RAISERROR('No statuses were provided. Check @StatusesCsv.', 16, 1);
    RETURN;
END;

-- ===============================
-- Random Source Pools
-- ===============================
IF OBJECT_ID('tempdb..#EligiblePrimaryAlias') IS NOT NULL DROP TABLE #EligiblePrimaryAlias;
IF OBJECT_ID('tempdb..#EligibleWorkflow') IS NOT NULL DROP TABLE #EligibleWorkflow;

CREATE TABLE #EligiblePrimaryAlias
(
    RowNum INT IDENTITY(1,1) PRIMARY KEY,
    Id INT NOT NULL
);

CREATE TABLE #EligibleWorkflow
(
    RowNum INT IDENTITY(1,1) PRIMARY KEY,
    Id INT NOT NULL
);

INSERT INTO #EligiblePrimaryAlias (Id)
SELECT p.PrimaryAliasId
FROM dbo.Person p
WHERE p.PrimaryAliasId IS NOT NULL
ORDER BY NEWID(); -- why: create a shuffled pool once, then pick different rows from it per insert row

IF @WorkflowTypeId IS NOT NULL
BEGIN
    INSERT INTO #EligibleWorkflow (Id)
    SELECT w.Id
    FROM dbo.Workflow w
    WHERE w.WorkflowTypeId = @WorkflowTypeId
    ORDER BY NEWID(); -- why: shuffle candidate workflows once, then randomly pick among the existing instances
END;

DECLARE @EligibleAliasCount INT = (SELECT COUNT(*) FROM #EligiblePrimaryAlias);
DECLARE @EligibleWorkflowCount INT = (SELECT COUNT(*) FROM #EligibleWorkflow);
DECLARE @PackageOptionCount INT = (SELECT COUNT(*) FROM @PackageOptions);
DECLARE @StatusOptionCount INT = (SELECT COUNT(*) FROM @StatusOptions);
DECLARE @RequestDateSpanDays INT = DATEDIFF(DAY, @RequestDateStart, @RequestDateEnd);

IF @EligibleAliasCount = 0
BEGIN
    RAISERROR('No eligible Person.PrimaryAliasId values were found.', 16, 1);
    RETURN;
END;

-- ===============================
-- Insert
-- ===============================
;WITH RequestedRows AS
(
    SELECT TOP (@BackgroundCheckCount)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum
    FROM sys.all_objects a
    CROSS JOIN sys.all_objects b
)
INSERT INTO dbo.BackgroundCheck
(
      PersonAliasId
    , WorkflowId
    , RequestDate
    , ResponseDate
    , RecordFound
    , ResponseData
    , ResponseDocumentId
    , CreatedDateTime
    , ModifiedDateTime
    , CreatedByPersonAliasId
    , ModifiedByPersonAliasId
    , [Guid]
    , ForeignId
    , ForeignGuid
    , ForeignKey
    , ProcessorEntityTypeId
    , [Status]
    , PackageName
    , ResponseId
    , RequestId
    , ConnectionRequestId
)
SELECT
      pa.Id AS PersonAliasId
    , wf.WorkflowId
    , rd.RequestDate
    , CASE WHEN sm.HasResponse = 1 THEN DATEADD(HOUR, rt.ResponseOffsetHours, rd.RequestDate) ELSE NULL END AS ResponseDate
    , CASE WHEN sm.IsCompleted = 1 THEN rt.RecordFound ELSE NULL END AS RecordFound
    , CASE
          WHEN @PopulateResponseData = 1 AND sm.HasResponse = 1
              THEN CONCAT(
                     N'{"status":"', so.[Value],
                     N'","packageName":"', po.[Value],
                     N'","requestId":"', rt.RequestId,
                     N'","responseId":"', rt.ResponseId,
                     N'","recordFound":',
                     CASE
                         WHEN sm.IsCompleted = 1 THEN CASE WHEN rt.RecordFound = 1 THEN N'true' ELSE N'false' END
                         ELSE N'null'
                     END,
                     N'}'
                 )
          ELSE NULL
      END AS ResponseData
    , NULL AS ResponseDocumentId
    , DATEADD(MINUTE, -rt.CreatedLeadMinutes, rd.RequestDate) AS CreatedDateTime
    , CASE
          WHEN sm.HasResponse = 1
              THEN DATEADD(MINUTE, rt.ModifiedLagMinutes, DATEADD(HOUR, rt.ResponseOffsetHours, rd.RequestDate))
          ELSE DATEADD(MINUTE, rt.ModifiedLagMinutes, rd.RequestDate)
      END AS ModifiedDateTime
    , @CreatedByPersonAliasId AS CreatedByPersonAliasId
    , @CreatedByPersonAliasId AS ModifiedByPersonAliasId
    , NEWID() AS [Guid]
    , @ForeignId AS ForeignId
    , NULL AS ForeignGuid
    , NULL AS ForeignKey
    , NULL AS ProcessorEntityTypeId
    , so.[Value] AS [Status]
    , po.[Value] AS PackageName
    , CASE WHEN sm.HasResponse = 1 THEN rt.ResponseId ELSE NULL END AS ResponseId
    , rt.RequestId
    , NULL AS ConnectionRequestId
FROM RequestedRows rr
CROSS APPLY
(
    -- why: choose a different primary alias candidate per row instead of reusing a single TOP (1) result
    SELECT ((ABS(CHECKSUM(NEWID(), rr.RowNum, N'person')) % @EligibleAliasCount) + 1) AS AliasRowNum
) pr
JOIN #EligiblePrimaryAlias pa
    ON pa.RowNum = pr.AliasRowNum
CROSS APPLY
(
    SELECT ((ABS(CHECKSUM(NEWID(), rr.RowNum, N'package')) % @PackageOptionCount) + 1) AS PackageOptionId
) pir
JOIN @PackageOptions po
    ON po.Id = pir.PackageOptionId
CROSS APPLY
(
    SELECT ((ABS(CHECKSUM(NEWID(), rr.RowNum, N'status')) % @StatusOptionCount) + 1) AS StatusOptionId
) sir
JOIN @StatusOptions so
    ON so.Id = sir.StatusOptionId
CROSS APPLY
(
    -- why: statuses not recognized as "created/completed" are treated like incomplete requests and keep response fields NULL
    SELECT
          CASE WHEN so.[Value] IN (N'ReportCreated', N'ReportCompleted') THEN 1 ELSE 0 END AS HasResponse
        , CASE WHEN so.[Value] = N'ReportCompleted' THEN 1 ELSE 0 END AS IsCompleted
) sm
CROSS APPLY
(
    SELECT DATEADD(
               DAY,
               CASE WHEN @RequestDateSpanDays <= 0 THEN 0 ELSE ABS(CHECKSUM(NEWID(), rr.RowNum, N'date')) % (@RequestDateSpanDays + 1) END,
               CAST(@RequestDateStart AS DATETIME)
           ) AS RequestDate
) rd
CROSS APPLY
(
    SELECT
          LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 24) AS RequestId
        , LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 24) AS ResponseId
        , 1 + (ABS(CHECKSUM(NEWID(), rr.RowNum, N'response-hours')) % 240) AS ResponseOffsetHours -- 1..240 hours after request when a response exists
        , CAST(ABS(CHECKSUM(NEWID(), rr.RowNum, N'record-found')) % 2 AS BIT) AS RecordFound
        , ABS(CHECKSUM(NEWID(), rr.RowNum, N'created-lead')) % 720 AS CreatedLeadMinutes -- up to 12 hours before request
        , 1 + (ABS(CHECKSUM(NEWID(), rr.RowNum, N'modified-lag')) % 120) AS ModifiedLagMinutes -- 1..120 minutes after request/response
) rt
CROSS APPLY
(
    -- why: only some rows should reference an existing workflow, and if none exist for the configured type then WorkflowId stays NULL
    SELECT CASE
        WHEN @WorkflowTypeId IS NULL OR @EligibleWorkflowCount = 0 THEN NULL
        WHEN (ABS(CHECKSUM(NEWID(), rr.RowNum, N'use-workflow')) % 100) >= @WorkflowAssignmentPercent THEN NULL
        ELSE ((ABS(CHECKSUM(NEWID(), rr.RowNum, N'workflow-row')) % @EligibleWorkflowCount) + 1)
    END AS WorkflowRowNum
) wfr
LEFT JOIN #EligibleWorkflow ew
    ON ew.RowNum = wfr.WorkflowRowNum
CROSS APPLY
(
    SELECT ew.Id AS WorkflowId
) wf
ORDER BY NEWID(); -- why: avoid predictable clustering by person/status/package when the source pool is small

/*
 Script to populate Step data for Step Types that belong to a specified Step Program.
*/

DECLARE @StepProgramId INT = 1; -- Step Program to populate
DECLARE @EndDate   DATETIME = GETDATE(); -- Today's date
DECLARE @StartDate DATETIME = DATEADD(YEAR, -1, @EndDate); -- One year before today
DECLARE @NumberOfSteps INT = 1000; -- Number of steps to insert

;WITH DateRange AS (
    SELECT DATEDIFF(SECOND, @StartDate, @EndDate) AS SecondsDiff
)
, RandomSteps AS (
    SELECT TOP (@NumberOfSteps)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum,
        ABS(CHECKSUM(NEWID())) % (SELECT SecondsDiff FROM DateRange) AS RandomStartOffset,
        ABS(CHECKSUM(NEWID())) % (SELECT SecondsDiff FROM DateRange) AS RandomEndOffset
    FROM sys.all_objects a
    CROSS JOIN sys.all_objects b
)
, RandomStepTypes AS (
    SELECT ROW_NUMBER() OVER (ORDER BY NEWID()) AS RowNum, Id
    FROM StepType
    WHERE StepProgramId = @StepProgramId
)
, RandomStatuses AS (
    SELECT ROW_NUMBER() OVER (ORDER BY NEWID()) AS RowNum, Id, IsCompleteStatus
    FROM StepStatus
    WHERE StepProgramId = @StepProgramId
)
, RandomPeople AS (
    SELECT ROW_NUMBER() OVER (ORDER BY NEWID()) AS RowNum, Id
    FROM PersonAlias
)
, RandomCampuses AS (
    SELECT ROW_NUMBER() OVER (ORDER BY NEWID()) AS RowNum, Id
    FROM Campus
)
INSERT INTO Step (
      StepTypeId
    , PersonAliasId
    , StepStatusId
    , CampusId
    , [Order]
    , StartDateTime
    , EndDateTime
    , CompletedDateTime
    , StartDateKey
    , EndDateKey
    , CompletedDateKey
    , CreatedDateTime
    , ModifiedDateTime
    , Guid
)
SELECT 
      st.Id AS StepTypeId
    , pa.Id AS PersonAliasId
    , ss.Id AS StepStatusId
    , c.Id  AS CampusId
    , r.RowNum AS [Order]
    , DATEADD(SECOND, r.RandomStartOffset, @StartDate) AS StartDateTime
    , CASE 
          WHEN ss.IsCompleteStatus = 1 
          THEN DATEADD(SECOND,
                       CASE WHEN r.RandomEndOffset < r.RandomStartOffset 
                            THEN r.RandomStartOffset + 1
                            ELSE r.RandomEndOffset END,
                       @StartDate) 
          ELSE NULL 
      END AS EndDateTime
    , CASE 
          WHEN ss.IsCompleteStatus = 1 
          THEN DATEADD(SECOND,
                       CASE WHEN r.RandomEndOffset < r.RandomStartOffset 
                            THEN r.RandomStartOffset + 1
                            ELSE r.RandomEndOffset END,
                       @StartDate) 
          ELSE NULL 
      END AS CompletedDateTime
    , CONVERT(INT, FORMAT(DATEADD(SECOND, r.RandomStartOffset, @StartDate), 'yyyyMMdd')) AS StartDateKey
    , CASE 
          WHEN ss.IsCompleteStatus = 1 
          THEN CONVERT(INT, FORMAT(
                          DATEADD(SECOND,
                              CASE WHEN r.RandomEndOffset < r.RandomStartOffset 
                                   THEN r.RandomStartOffset + 1
                                   ELSE r.RandomEndOffset END,
                              @StartDate), 'yyyyMMdd'))
          ELSE NULL 
      END AS EndDateKey
    , CASE 
          WHEN ss.IsCompleteStatus = 1 
          THEN CONVERT(INT, FORMAT(
                          DATEADD(SECOND,
                              CASE WHEN r.RandomEndOffset < r.RandomStartOffset 
                                   THEN r.RandomStartOffset + 1
                                   ELSE r.RandomEndOffset END,
                              @StartDate), 'yyyyMMdd'))
          ELSE NULL 
      END AS CompletedDateKey
    , GETDATE() AS CreatedDateTime
    , GETDATE() AS ModifiedDateTime
    , NEWID() AS Guid
FROM RandomSteps r
JOIN RandomStepTypes st
    ON st.RowNum = ((r.RowNum - 1) % (SELECT COUNT(*) FROM RandomStepTypes)) + 1
JOIN RandomStatuses ss
    ON ss.RowNum = ((r.RowNum - 1) % (SELECT COUNT(*) FROM RandomStatuses)) + 1
JOIN RandomPeople pa
    ON pa.RowNum = ((r.RowNum - 1) % (SELECT COUNT(*) FROM RandomPeople)) + 1
JOIN RandomCampuses c
    ON c.RowNum = ((r.RowNum - 1) % (SELECT COUNT(*) FROM RandomCampuses)) + 1;

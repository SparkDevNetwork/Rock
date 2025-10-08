/* =============================================================================================
 Author:        Code Copilot (w/ N.A.)
 Created:       2025-09-17
 Description:   SQL Server 2016–compatible populate script to add N fake Schedules
                for a configured CategoryId, with readable names and valid iCal content.
                - iCal is DAILY with DTSTART @ 00:01 and DTEND @ 23:59 on the same day.
                - Leaves Description, Weekly*, Foreign* columns NULL.
 ============================================================================================= */

SET NOCOUNT ON;

/* ===============================
   Configuration
   =============================== */
DECLARE @CategoryId INT = 360;                  -- TODO: set this to your Category Id
DECLARE @RowCount   INT = 5000;                 -- number of schedules to create
DECLARE @DateMin    DATE = '2021-01-01';        -- iCal date window start
DECLARE @DateMax    DATE = GETDATE();        -- iCal date window end
DECLARE @EffStartDate DATE = '2021-05-01';      -- Schedule.EffectiveStartDate per example
DECLARE @CheckInStartOffset INT = 0;            -- minutes
DECLARE @CheckInEndOffset   INT = 1439;         -- minutes
DECLARE @IsActive BIT = 1;
DECLARE @DoCommit BIT = 1;                      -- 0 = preview/rollback; 1 = commit

/* ===============================
   Preconditions
   =============================== */
IF OBJECT_ID('dbo.[Schedule]') IS NULL
BEGIN
    RAISERROR('dbo.[Schedule] not found.', 16, 1);
    RETURN;
END;

IF @CategoryId IS NULL
BEGIN
    RAISERROR('@CategoryId must be set.', 16, 1);
    RETURN;
END;

IF (@DateMax < @DateMin)
BEGIN
    RAISERROR('@DateMax must be >= @DateMin.', 16, 1);
    RETURN;
END;

DECLARE @SpanDays INT = DATEDIFF(DAY, @DateMin, @DateMax);

/* ===============================
   Build a tally of N rows
   =============================== */
IF OBJECT_ID('tempdb..#N') IS NOT NULL DROP TABLE #N;
SELECT TOP (@RowCount) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
INTO #N
FROM sys.all_objects a CROSS JOIN sys.all_objects b;

/* ===============================
   Insert rows
   =============================== */
DECLARE @Inserted INT = 0;
BEGIN TRAN;
BEGIN TRY
    INSERT INTO [dbo].[Schedule] (
          [Name]
        , [Description]
        , [iCalendarContent]
        , [CheckInStartOffsetMinutes]
        , [CheckInEndOffsetMinutes]
        , [EffectiveStartDate]
        , [EffectiveEndDate]
        , [CategoryId]
        , [Guid]
        , [ForeignKey]
        , [WeeklyDayOfWeek]
        , [WeeklyTimeOfDay]
        , [ForeignGuid]
        , [ForeignId]
        , [IsActive]
    )
    SELECT 
        -- Semi-random readable name, trimmed to 100
        LEFT(
            N'Daily ' + DATENAME(WEEKDAY, RandDate) + N' ' + CONVERT(NVARCHAR(10), RandDate, 120) + N' #' + CONVERT(NVARCHAR(10), n.n)
        , 100) AS [Name],
        NULL AS [Description],
        -- iCal content with CRLF line breaks
        (
            N'BEGIN:VCALENDAR' + CHAR(13) + CHAR(10) +
            N'BEGIN:VEVENT'  + CHAR(13) + CHAR(10) +
            N'DTEND:'   + Ymd + N'T235900' + CHAR(13) + CHAR(10) +
            N'DTSTART:' + Ymd + N'T000100' + CHAR(13) + CHAR(10) +
            N'RRULE:FREQ=DAILY' + CHAR(13) + CHAR(10) +
            N'END:VEVENT' + CHAR(13) + CHAR(10) +
            N'END:VCALENDAR'
        ) AS [iCalendarContent],
        @CheckInStartOffset AS [CheckInStartOffsetMinutes],
        @CheckInEndOffset   AS [CheckInEndOffsetMinutes],
        @EffStartDate       AS [EffectiveStartDate],
        NULL                AS [EffectiveEndDate],
        @CategoryId         AS [CategoryId],
        NEWID()             AS [Guid],
        NULL                AS [ForeignKey],
        NULL                AS [WeeklyDayOfWeek],
        NULL                AS [WeeklyTimeOfDay],
        NULL                AS [ForeignGuid],
        NULL                AS [ForeignId],
        @IsActive           AS [IsActive]
    FROM #N n
    CROSS APPLY (
        SELECT DATEADD(DAY, CASE WHEN @SpanDays <= 0 THEN 0 ELSE ABS(CHECKSUM(NEWID())) % (@SpanDays + 1) END, CAST(@DateMin AS DATETIME)) AS RandDate
    ) d
    CROSS APPLY (
        SELECT CONVERT(CHAR(8), d.RandDate, 112) AS Ymd
    ) fmt;

    SET @Inserted = @@ROWCOUNT;

    IF (@DoCommit = 1)
    BEGIN
        COMMIT TRAN;
        PRINT CONCAT('Inserted Schedule rows: ', @Inserted, ' (CategoryId=', @CategoryId, ')');
    END
    ELSE
    BEGIN
        ROLLBACK TRAN;
        PRINT CONCAT('Preview only. Would have inserted Schedule rows: ', @Inserted, ' (CategoryId=', @CategoryId, ')');
    END
END TRY
BEGIN CATCH
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    RAISERROR('Populate_Schedules failed: %s', 16, 1, @ErrMsg);
    RETURN;
END CATCH;

/* ===============================
   Verify sample
   =============================== */
SELECT TOP (25)
      [Id]
    , [Name]
    , [CategoryId]
    , [EffectiveStartDate]
    , [IsActive]
FROM dbo.[Schedule] WITH (NOLOCK)
WHERE [CategoryId] = @CategoryId
ORDER BY [Id] DESC;

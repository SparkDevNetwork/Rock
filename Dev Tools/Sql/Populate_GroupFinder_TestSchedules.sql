SET NOCOUNT ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON

-- =============================================
-- Description: Creates a handful of weekly schedules (spanning every day of the week and all three
--              time-of-day buckets Morning/Afternoon/Evening) and assigns them across the map-located
--              Small Groups (GroupTypeId 25), so the Group Finder "When" filter and the card schedule
--              text have data. Weekly schedules use WeeklyDayOfWeek + WeeklyTimeOfDay, which is what both
--              the day/time filter and Schedule.FriendlyScheduleText read - no iCalendar content needed.
--              Idempotent: schedules are guarded by guid and the group assignment is deterministic.
-- Date: 2026-08-05
-- =============================================

DECLARE @GroupTypeId INT = 25

-- Day-of-week ints follow .NET DayOfWeek: Sunday = 0 ... Saturday = 6.
-- Times sit safely inside their bucket (Morning 5-11, Afternoon 12-16, Evening 17-20).
DECLARE @Sched TABLE ([Ord] INT, [Guid] UNIQUEIDENTIFIER, [Name] NVARCHAR(50), [Dow] INT, [Time] TIME(7))
INSERT INTO @Sched VALUES
 (0,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E501','Weekly - Monday Morning',    1, '09:00'),
 (1,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E502','Weekly - Monday Evening',    1, '18:30'),
 (2,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E503','Weekly - Tuesday Evening',   2, '19:00'),
 (3,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E504','Weekly - Wednesday Morning', 3, '09:30'),
 (4,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E505','Weekly - Wednesday Evening', 3, '18:30'),
 (5,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E506','Weekly - Thursday Afternoon',4, '14:00'),
 (6,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E507','Weekly - Friday Evening',    5, '19:00'),
 (7,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E508','Weekly - Saturday Morning',  6, '10:00'),
 (8,'7F1A2B3C-2222-4A5B-9C0D-A0B1C2D3E509','Weekly - Sunday Afternoon',  0, '16:00')

BEGIN TRANSACTION

-- ---------------------------------------------
-- Schedules
-- ---------------------------------------------
INSERT INTO [Schedule] ([Name],[WeeklyDayOfWeek],[WeeklyTimeOfDay],[IsActive],[Order],[AutoInactivateWhenComplete],[Guid],[CreatedDateTime],[ModifiedDateTime])
SELECT s.[Name], s.[Dow], s.[Time], 1, s.[Ord], 0, s.[Guid], GETDATE(), GETDATE()
FROM @Sched s
WHERE NOT EXISTS (SELECT 1 FROM [Schedule] sch WHERE sch.[Guid] = s.[Guid])

DECLARE @Count INT = (SELECT COUNT(*) FROM @Sched)

-- ---------------------------------------------
-- Assign a schedule to every map-located Small Group, cycling through the set by a stable row number so
-- days and times spread evenly and re-runs land the same assignment.
-- ---------------------------------------------
;WITH [geo] AS (
    SELECT g.[Id], ROW_NUMBER() OVER (ORDER BY g.[Id]) AS [rn]
    FROM [Group] g
    WHERE g.[GroupTypeId] = @GroupTypeId AND g.[IsActive] = 1
      AND EXISTS (SELECT 1 FROM [GroupLocation] gl JOIN [Location] l ON l.[Id] = gl.[LocationId] WHERE gl.[GroupId] = g.[Id] AND l.[GeoPoint] IS NOT NULL)
),
[sched] AS (
    SELECT sch.[Id], ROW_NUMBER() OVER (ORDER BY s.[Ord]) AS [rk]
    FROM @Sched s
    JOIN [Schedule] sch ON sch.[Guid] = s.[Guid]
)
UPDATE g
SET g.[ScheduleId] = sched.[Id], g.[ModifiedDateTime] = GETDATE()
FROM [Group] g
JOIN [geo] ON [geo].[Id] = g.[Id]
JOIN [sched] ON [sched].[rk] = ([geo].[rn] % @Count) + 1

COMMIT TRANSACTION

-- ---------------------------------------------
-- Verify: group counts by assigned schedule.
-- ---------------------------------------------
SELECT sch.[Name] AS [Schedule], sch.[WeeklyDayOfWeek] AS [Dow], CAST(sch.[WeeklyTimeOfDay] AS NVARCHAR(16)) AS [Time], COUNT(g.[Id]) AS [Groups]
FROM [Schedule] sch
LEFT JOIN [Group] g ON g.[ScheduleId] = sch.[Id] AND g.[GroupTypeId] = @GroupTypeId
WHERE sch.[Guid] IN (SELECT [Guid] FROM @Sched)
GROUP BY sch.[Name], sch.[WeeklyDayOfWeek], sch.[WeeklyTimeOfDay]
ORDER BY sch.[Name]

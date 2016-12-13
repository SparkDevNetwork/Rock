IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceSchedule]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceSchedule
GO

CREATE VIEW AnalyticsDimAttendanceSchedule
AS
SELECT s.Id [ScheduleId]
    ,s.NAME [Name]
FROM [Schedule] s
WHERE (isnull(s.NAME, '') != '')
    OR Id IN (
        SELECT ScheduleId
        FROM Attendance
        )

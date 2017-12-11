IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceDate
GO

CREATE VIEW [dbo].[AnalyticsDimAttendanceDate]
AS
SELECT d.DateKey AS [AttendanceDateKey]
    ,*
FROM AnalyticsSourceDate d
WHERE d.DateKey >= (
        SELECT MIN(x.AttendanceDateKey)
        FROM AnalyticsFactAttendance x
        )
    AND d.DateKey <= (
        SELECT MAX(x.AttendanceDateKey)
        FROM AnalyticsFactAttendance x
        )

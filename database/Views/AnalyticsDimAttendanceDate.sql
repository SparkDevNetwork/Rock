IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceDate
GO

CREATE VIEW [dbo].[AnalyticsDimAttendanceDate]
AS
SELECT d.DateKey AS [AttendanceDateKey]
    ,*
FROM AnalyticsDimDate d
WHERE d.DateKey >= (
        SELECT MIN(fa.AttendanceDateKey)
        FROM AnalyticsFactAttendance fa
        )
    AND d.DateKey <= (
        SELECT MAX(fa.AttendanceDateKey)
        FROM AnalyticsFactAttendance fa
        )
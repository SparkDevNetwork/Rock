IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceGroup]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceGroup
GO

CREATE VIEW AnalyticsDimAttendanceGroup
AS
SELECT g.Id [GroupId]
    ,g.NAME [Name]
FROM [Group] g

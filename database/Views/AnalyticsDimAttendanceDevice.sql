IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceDevice]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceDevice
GO

CREATE VIEW AnalyticsDimAttendanceDevice
AS
SELECT d.Id [DeviceId]
	,d.Name [Name] 
FROM [Device] d

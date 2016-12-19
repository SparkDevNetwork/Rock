IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceAttendanceType]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceAttendanceType
GO

-- The intention of this is to do the same thing that Attendance Analytics has in "Attendance Type" drop down list
CREATE VIEW AnalyticsDimAttendanceAttendanceType
AS
SELECT gt.Id [AttendanceTypeId]
    ,gt.[Name]
    ,gt.[Description]
    ,gt.[Order]
FROM [GroupType] gt
WHERE GroupTypePurposeValueId IN (
        SELECT Id
        FROM DefinedValue
        WHERE [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' -- GroupTypePurpose Checkin
        )

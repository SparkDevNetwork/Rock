IF OBJECT_ID(N'[dbo].[AnalyticsFactAttendance]', 'V') IS NOT NULL
    DROP VIEW AnalyticsFactAttendance
GO

-- select top 10000 * from AnalyticsFactAttendance order by Id desc
CREATE VIEW AnalyticsFactAttendance
AS
SELECT asa.*
    ,adphPerson.PrimaryFamilyKey [FamilyKey]
	,adpcPerson.PrimaryFamilyKey [CurrentFamilyKey]
    ,isnull(at.NAME, 'None') [AttendanceTypeName]
    ,isnull(l.NAME, 'None') [LocationName]
    ,isnull(c.NAME, 'None') [CampusName]
    ,isnull(c.ShortCode, 'None') [CampusShortCode]
    ,isnull(s.NAME, 'None') [ScheduleName]
    ,isnull(g.NAME, 'None') [GroupName]
    ,isnull(gt.NAME, 'None') [AreaName] -- (aka GroupType.Name)
    ,isnull(d.NAME, 'None') [DeviceName]
    ,CASE asa.RSVP
        WHEN 0
            THEN 'No'
        WHEN 1
            THEN 'Yes'
        WHEN 2
            THEN 'Maybe'
        ELSE 'None'
        END [RSVPStatus]
FROM AnalyticsSourceAttendance asa
LEFT JOIN AnalyticsDimPersonCurrent adpcPerson ON adpcPerson.Id = asa.CurrentPersonKey
LEFT JOIN AnalyticsDimPersonHistorical adphPerson ON adphPerson.Id = asa.PersonKey
LEFT JOIN GroupType at ON at.Id = asa.AttendanceTypeId
LEFT JOIN Location l ON l.Id = asa.LocationId
LEFT JOIN Campus c ON c.Id = asa.CampusId
LEFT JOIN Schedule s ON s.Id = asa.ScheduleId
LEFT JOIN [Group] g ON g.Id = asa.GroupId
LEFT JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
LEFT JOIN [Device] d ON d.Id = asa.DeviceId
IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_Attendance]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_Attendance
GO

-- EXECUTE [dbo].[spAnalytics_ETL_Attendance] 
CREATE PROCEDURE [dbo].spAnalytics_ETL_Attendance
AS
BEGIN
    DECLARE @MinDateTime DATETIME = DATEFROMPARTS(1900, 1, 1)
        ,@EtlDateTime DATETIME = SysDateTime();

    -- insert records into [[AnalyticsSourceAttendance]] from the source [Attendance] table that haven't been added yet
    INSERT INTO [dbo].[AnalyticsSourceAttendance] (
        [AttendanceId]
        ,[AttendanceDateKey]
        ,[AttendanceTypeId]
        ,[Count]
        ,[LocationId]
        ,[CampusId]
        ,[ScheduleId]
        ,[GroupId]
        ,[PersonAliasId]
        ,[DeviceId]
        ,[SearchTypeName]
        ,[StartDateTime]
        ,[EndDateTime]
        ,[RSVP]
        ,[DidAttend]
        ,[Note]
        ,[SundayDate]
        ,[IsFirstAttendanceOfType]
        ,[DaysSinceLastAttendanceOfType]
        ,[Guid]
        )
    SELECT a.Id [AttendanceId]
        ,convert(INT, (convert(CHAR(8), a.StartDateTime, 112))) [AttendanceDateKey]
        ,NULL [AttendanceTypeId] -- fill in later
        ,CASE 
            WHEN a.DidAttend = 1
                THEN 1
            ELSE 0
            END [Count]
        ,a.LocationId
        ,a.CampusId
        ,a.ScheduleId
        ,a.GroupId
        ,a.PersonAliasId
        ,a.DeviceId
        ,isnull(dvSearchType.Value, 'None') [SearchTypeName]
        ,a.StartDateTime
        ,a.EndDateTime
        ,a.RSVP
        ,a.DidAttend
        ,a.Note
        ,a.SundayDate
        ,0 -- [IsFirstAttendanceOfType] fill in later
        ,NULL -- [DaysSinceLastAttendanceOfType] fill in later
        ,NEWID() [Guid]
    FROM Attendance a
    LEFT JOIN DefinedValue dvSearchType ON dvSearchType.Id = a.SearchTypeValueId
    WHERE isnull(a.DidNotOccur, 0) = 0
        AND a.Id NOT IN (
            SELECT AttendanceId
            FROM AnalyticsSourceAttendance
            )

    -- remove records from AnalyticsSourceAttendance that no longer exist in the source Attendance table
    DELETE
    FROM AnalyticsSourceAttendance
    WHERE AttendanceId NOT IN (
            SELECT Id
            FROM Attendance
            WHERE isnull(DidNotOccur, 0) = 0
            )

    -- Figure Out AttendanceType
    UPDATE asa
    SET [AttendanceTypeId] = pgt.Id
    FROM AnalyticsSourceAttendance asa
    JOIN [Group] g ON asa.GroupId = g.Id
    JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
    JOIN [GroupTypeAssociation] gta ON gta.ChildGroupTypeId = gt.Id
    JOIN [GroupType] pgt ON pgt.Id = gta.GroupTypeId
        AND pgt.GroupTypePurposeValueId = (
            SELECT Id
            FROM DefinedValue
            WHERE [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' -- GroupTypePurpose Checkin
            )
    WHERE isnull(asa.AttendanceTypeId, 0) != pgt.Id

    /* Updating these PersonKeys depends on AnalyticsSourcePersonHistorical getting populated and updated. 
  -- It is probably best to schedule the ETL of AnalyticsSourcePersonHistorical to occur before spAnalytics_ETL_Attendance
  -- However, if not, it will catch up on the next run of spAnalytics_ETL_Attendance
  */
    -- Update PersonKeys for whatever PersonKey the person had at the time of the Attendance
    UPDATE asa
    SET [PersonKey] = x.PersonKey
    FROM AnalyticsSourceAttendance asa
    CROSS APPLY (
        SELECT TOP 1 ph.Id [PersonKey]
        FROM AnalyticsSourcePersonHistorical ph
        JOIN PersonAlias pa ON asa.PersonAliasId = pa.Id
        WHERE ph.PersonId = pa.PersonId
            AND asa.[StartDateTime] < ph.[ExpireDate]
        ORDER BY ph.[ExpireDate] DESC
        ) x
    WHERE isnull(asa.[PersonKey], 0) != isnull(x.PersonKey, 0)

    -- Update PersonKeys for whatever PersonKey is current right now
    UPDATE asa
    SET [CurrentPersonKey] = x.PersonKey
    FROM AnalyticsSourceAttendance asa
    CROSS APPLY (
        SELECT max(pc.Id) [PersonKey]
        FROM AnalyticsDimPersonCurrent pc
        JOIN PersonAlias pa ON asa.PersonAliasId = pa.Id
        WHERE pc.PersonId = pa.PersonId
        ) x
    WHERE isnull(asa.[CurrentPersonKey], 0) != isnull(x.PersonKey, 0)

    -- figure out IsFirstAttendanceOfType
    UPDATE asa
    SET asa.IsFirstAttendanceOfType = 1
        ,DaysSinceLastAttendanceOfType = NULL
    FROM (
        SELECT x.*
        FROM (
            SELECT min(asa.StartDateTime) [DateTimeOfFirstAttendanceOfType]
                ,asa.AttendanceTypeId
                ,pa.PersonId
            FROM AnalyticsSourceAttendance asa
            JOIN PersonAlias pa ON asa.PersonAliasId = pa.Id
            WHERE asa.AttendanceTypeId IS NOT NULL
            GROUP BY asa.AttendanceTypeId
                ,pa.PersonId
            ) firstTran
        CROSS APPLY (
            SELECT TOP 1 a.*
            FROM AnalyticsSourceAttendance a
            JOIN PersonAlias pa ON a.PersonAliasId = pa.Id
            WHERE a.AttendanceTypeId IS NOT NULL
                AND pa.PersonId = firstTran.PersonId
                AND a.AttendanceTypeId = firstTran.AttendanceTypeId
                AND a.[StartDateTime] = firstTran.[DateTimeOfFirstAttendanceOfType]
            ) x
        ) asa
    WHERE asa.IsFirstAttendanceOfType = 0

    -- update just in case any records where modified since originally inserted
    UPDATE asa
    SET asa.[Count] = x.[Count]
        ,asa.[LocationId] = x.[LocationId]
        ,asa.[CampusId] = x.[CampusId]
        ,asa.[ScheduleId] = x.[ScheduleId]
        ,asa.[GroupId] = x.[GroupId]
        ,asa.[PersonAliasId] = x.[PersonAliasId]
        ,asa.[DeviceId] = x.[DeviceId]
        ,asa.[SearchTypeName] = x.[SearchTypeName]
        ,asa.[StartDateTime] = x.[StartDateTime]
        ,asa.[EndDateTime] = x.[EndDateTime]
        ,asa.[RSVP] = x.[RSVP]
        ,asa.[DidAttend] = x.[DidAttend]
        ,asa.[Note] = x.[Note]
        ,asa.[SundayDate] = x.[SundayDate]
    FROM [AnalyticsSourceAttendance] asa
    JOIN (
        SELECT a.Id [AttendanceId]
            ,convert(INT, (convert(CHAR(8), a.StartDateTime, 112))) [AttendanceDateKey]
            ,CASE 
                WHEN a.DidAttend = 1
                    THEN 1
                ELSE 0
                END [Count]
            ,a.LocationId
            ,a.CampusId
            ,a.ScheduleId
            ,a.GroupId
            ,a.PersonAliasId
            ,a.DeviceId
            ,isnull(dvSearchType.Value, 'None') [SearchTypeName]
            ,a.StartDateTime
            ,a.EndDateTime
            ,a.RSVP
            ,a.DidAttend
            ,a.Note
            ,a.SundayDate
        FROM Attendance a
        LEFT JOIN DefinedValue dvSearchType ON dvSearchType.Id = a.SearchTypeValueId
        WHERE isnull(a.DidNotOccur, 0) = 0
        ) x ON x.AttendanceId = asa.AttendanceId
        AND (
            asa.[LocationId] != x.[LocationId]
            OR asa.[CampusId] != x.[CampusId]
            OR asa.[ScheduleId] != x.[ScheduleId]
            OR asa.[GroupId] != x.[GroupId]
            OR asa.[PersonAliasId] != x.[PersonAliasId]
            OR asa.[DeviceId] != x.[DeviceId]
            OR asa.[SearchTypeName] != x.[SearchTypeName]
            OR asa.[StartDateTime] != x.[StartDateTime]
            OR asa.[EndDateTime] != x.[EndDateTime]
            OR asa.[RSVP] != x.[RSVP]
            OR asa.[DidAttend] != x.[DidAttend]
            OR asa.[Note] != x.[Note]
            OR asa.[SundayDate] != x.[SundayDate]
            )

    -- Update [DaysSinceLastAttendanceOfType]
    -- get the number of days since the last attendance of this person of the same AttendanceType
    -- but don't count it as a previous attendance if it was on the same date
    -- To optimize, add a WHERE DaysSinceLastAttendanceOfType is NULL, but at the risk of the number being wrong due to a new attendance with an earlier date getting added 
    UPDATE asa
    SET DaysSinceLastAttendanceOfType = x.[CalcDaysSinceLastAttendanceOfType]
    FROM AnalyticsSourceAttendance asa
    CROSS APPLY (
        SELECT TOP 1 DATEDIFF(day, previousAttendanceOfType.StartDateTime, asa.StartDateTime) [CalcDaysSinceLastAttendanceOfType]
        FROM AnalyticsSourceAttendance previousAttendanceOfType
        WHERE previousAttendanceOfType.CurrentPersonKey is not null and previousAttendanceOfType.CurrentPersonKey = asa.CurrentPersonKey
            AND previousAttendanceOfType.AttendanceTypeId = asa.AttendanceTypeId
            AND convert(DATE, previousAttendanceOfType.StartDateTime) < convert(DATE, asa.StartDateTime)
        ORDER BY previousAttendanceOfType.StartDateTime DESC
        ) x
    WHERE asa.CurrentPersonKey is not null and isnull(asa.DaysSinceLastAttendanceOfType, 0) != isnull(x.[CalcDaysSinceLastAttendanceOfType], 0)
END
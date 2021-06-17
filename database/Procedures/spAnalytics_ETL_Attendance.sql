IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_Attendance]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_Attendance
GO

CREATE PROCEDURE [dbo].[spAnalytics_ETL_Attendance]
AS
BEGIN
    DECLARE @MinDateTime DATETIME = DATEFROMPARTS(1900, 1, 1)
    DECLARE @EtlDateTime DATETIME = SYSDATETIME();

    -- insert records into [[AnalyticsSourceAttendance]] from the source [Attendance] table that haven't been added yet
    INSERT INTO [dbo].[AnalyticsSourceAttendance] (
          [AttendanceId]
        , [AttendanceDateKey]
        , [AttendanceTypeId]
        , [Count]
        , [LocationId]
        , [CampusId]
        , [ScheduleId]
        , [GroupId]
        , [PersonAliasId]
        , [DeviceId]
        , [SearchTypeName]
        , [StartDateTime]
        , [EndDateTime]
        , [RSVP]
        , [DidAttend]
        , [Note]
        , [SundayDate]
        , [IsFirstAttendanceOfType]
        , [DaysSinceLastAttendanceOfType]
        , [Guid]
        )
    SELECT 
		  a.Id [AttendanceId]
        , CONVERT(INT, (CONVERT(CHAR(8), a.[StartDateTime], 112))) [AttendanceDateKey]
        , NULL [AttendanceTypeId] -- fill in later
        , CASE WHEN a.DidAttend = 1 THEN 1 ELSE 0 END [Count]
        , o.[LocationId]
        , a.[CampusId]
        , o.[ScheduleId]
        , o.[GroupId]
        , a.[PersonAliasId]
        , a.[DeviceId]
        , ISNULL(dvSearchType.[Value], 'None') [SearchTypeName]
        , a.[StartDateTime]
        , a.[EndDateTime]
        , a.[RSVP]
        , a.[DidAttend]
        , a.[Note]
        , o.[SundayDate]
        , 0 -- [IsFirstAttendanceOfType] fill in later
        , NULL -- [DaysSinceLastAttendanceOfType] fill in later
        , NEWID() [Guid]
    FROM [Attendance] a
	JOIN [AttendanceOccurrence] o ON o.[Id] = a.[OccurrenceId]
    LEFT JOIN [DefinedValue] dvSearchType ON dvSearchType.[Id] = a.[SearchTypeValueId]
    WHERE ISNULL(o.[DidNotOccur], 0) = 0
        AND a.[Id] NOT IN (SELECT [AttendanceId] FROM [AnalyticsSourceAttendance])

    -- remove records from AnalyticsSourceAttendance that no longer exist in the source Attendance table
    DELETE
    FROM [AnalyticsSourceAttendance]
    WHERE [AttendanceId] NOT IN (
            SELECT a.[Id]
            FROM [Attendance] a
			JOIN [AttendanceOccurrence] o ON o.[Id] = a.[OccurrenceId]
            WHERE ISNULL(o.[DidNotOccur], 0) = 0
            )

    -- Figure Out AttendanceType
    UPDATE asa
    SET [AttendanceTypeId] = pgt.[Id]
    FROM [AnalyticsSourceAttendance] asa
    JOIN [Group] g ON asa.[GroupId] = g.[Id]
    JOIN [GroupType] gt ON g.[GroupTypeId] = gt.[Id]
    JOIN [GroupTypeAssociation] gta ON gta.[ChildGroupTypeId] = gt.Id
    JOIN [GroupType] pgt ON pgt.[Id] = gta.[GroupTypeId]
        AND pgt.[GroupTypePurposeValueId] = (
            SELECT [Id]
            FROM [DefinedValue]
            WHERE [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' -- GroupTypePurpose Checkin
            )
    WHERE isnull(asa.AttendanceTypeId, 0) != pgt.Id

    /* Updating these PersonKeys depends on AnalyticsSourcePersonHistorical getting populated and updated. 
  -- It is probably best to schedule the ETL of AnalyticsSourcePersonHistorical to occur before spAnalytics_ETL_Attendance
  -- However, if not, it will catch up on the next run of spAnalytics_ETL_Attendance
  */
    -- Update PersonKeys for whatever PersonKey the person had at the time of the Attendance
    UPDATE asa
    SET [PersonKey] = x.[PersonKey]
    FROM [AnalyticsSourceAttendance] asa
    CROSS APPLY (
        SELECT TOP 1 ph.[Id] [PersonKey]
        FROM [AnalyticsSourcePersonHistorical] ph
        JOIN [PersonAlias] pa ON asa.[PersonAliasId] = pa.[Id]
        WHERE ph.[PersonId] = pa.[PersonId]
            AND asa.[StartDateTime] < ph.[ExpireDate]
        ORDER BY ph.[ExpireDate] DESC
        ) x
    WHERE ISNULL(asa.[PersonKey], 0) != ISNULL(x.PersonKey, 0)

    -- Update PersonKeys for whatever PersonKey is current right now
    UPDATE asa
    SET [CurrentPersonKey] = x.[PersonKey]
    FROM [AnalyticsSourceAttendance] asa
    CROSS APPLY (
        SELECT MAX(pc.[Id]) [PersonKey]
        FROM [AnalyticsDimPersonCurrent] pc
        JOIN [PersonAlias] pa ON asa.[PersonAliasId] = pa.[Id]
        WHERE pc.[PersonId] = pa.[PersonId]
        ) x
    WHERE ISNULL(asa.[CurrentPersonKey], 0) != ISNULL(x.[PersonKey], 0)

    -- figure out IsFirstAttendanceOfType
    UPDATE asa
    SET   asa.[IsFirstAttendanceOfType] = 1
        , [DaysSinceLastAttendanceOfType] = NULL
    FROM (
        SELECT x.*
        FROM (
            SELECT
				  MIN(asa.[StartDateTime]) [DateTimeOfFirstAttendanceOfType]
                , asa.[AttendanceTypeId]
                , pa.[PersonId]
            FROM [AnalyticsSourceAttendance] asa
            JOIN [PersonAlias] pa ON asa.[PersonAliasId] = pa.Id
            WHERE asa.[AttendanceTypeId] IS NOT NULL
            GROUP BY asa.[AttendanceTypeId], pa.[PersonId]
            ) firstTran
        CROSS APPLY (
            SELECT TOP 1 a.*
            FROM [AnalyticsSourceAttendance] a
            JOIN [PersonAlias] pa ON a.[PersonAliasId] = pa.[Id]
            WHERE a.[AttendanceTypeId] IS NOT NULL
                AND pa.[PersonId] = firstTran.[PersonId]
                AND a.[AttendanceTypeId] = firstTran.[AttendanceTypeId]
                AND a.[StartDateTime] = firstTran.[DateTimeOfFirstAttendanceOfType]
            ) x
        ) asa
    WHERE asa.[IsFirstAttendanceOfType] = 0

    -- update just in case any records where modified since originally inserted
    UPDATE asa
    SET   asa.[Count] = x.[Count]
        , asa.[LocationId] = x.[LocationId]
        , asa.[CampusId] = x.[CampusId]
        , asa.[ScheduleId] = x.[ScheduleId]
        , asa.[GroupId] = x.[GroupId]
        , asa.[PersonAliasId] = x.[PersonAliasId]
        , asa.[DeviceId] = x.[DeviceId]
        , asa.[SearchTypeName] = x.[SearchTypeName]
        , asa.[StartDateTime] = x.[StartDateTime]
        , asa.[EndDateTime] = x.[EndDateTime]
        , asa.[RSVP] = x.[RSVP]
        , asa.[DidAttend] = x.[DidAttend]
        , asa.[Note] = x.[Note]
        , asa.[SundayDate] = x.[SundayDate]
    FROM [AnalyticsSourceAttendance] asa
    JOIN (
        SELECT a.Id [AttendanceId]
            , CONVERT(INT, (CONVERT(CHAR(8), a.[StartDateTime], 112))) [AttendanceDateKey]
            , CASE WHEN a.[DidAttend] = 1 THEN 1 ELSE 0 END [Count]
            , o.[LocationId]
            , a.[CampusId]
            , o.[ScheduleId]
            , o.[GroupId]
            , a.[PersonAliasId]
            , a.[DeviceId]
            , ISNULL(dvSearchType.[Value], 'None') [SearchTypeName]
            , a.[StartDateTime]
            , a.[EndDateTime]
            , a.[RSVP]
            , a.[DidAttend]
            , a.[Note]
            , o.[SundayDate]
        FROM [Attendance] a
		JOIN [AttendanceOccurrence] o on o.[Id] = a.[OccurrenceId]
        LEFT JOIN [DefinedValue] dvSearchType ON dvSearchType.[Id] = a.[SearchTypeValueId]
        WHERE isnull(o.[DidNotOccur], 0) = 0
        ) x ON x.[AttendanceId] = asa.[AttendanceId]
        AND (
               COALESCE(asa.[LocationId], '') != COALESCE(x.[LocationId], '')
            OR COALESCE(asa.[CampusId], '') != COALESCE(x.[CampusId], '')
            OR COALESCE(asa.[ScheduleId], '') != COALESCE(x.[ScheduleId], '')
            OR COALESCE(asa.[GroupId], '') != COALESCE(x.[GroupId], '')
            OR COALESCE(asa.[PersonAliasId], '') != COALESCE(x.[PersonAliasId], '')
            OR COALESCE(asa.[DeviceId], '') != COALESCE(x.[DeviceId], '')
            OR COALESCE(asa.[SearchTypeName], '') != COALESCE(x.[SearchTypeName], '')
            OR COALESCE(asa.[StartDateTime], '') != COALESCE(x.[StartDateTime], '')
            OR COALESCE(asa.[EndDateTime], '') != COALESCE(x.[EndDateTime], '')
            OR COALESCE(asa.[RSVP], '') != COALESCE(x.[RSVP], '')
            OR COALESCE(asa.[DidAttend], '') != COALESCE(x.[DidAttend], '')
            OR COALESCE(asa.[Note], '') != COALESCE(x.[Note], '')
            OR COALESCE(asa.[SundayDate], '') != COALESCE(x.[SundayDate], '')
            )

    -- Update [DaysSinceLastAttendanceOfType]
    -- get the number of days since the last attendance of this person of the same AttendanceType
    -- but don't count it as a previous attendance if it was on the same date
    -- To optimize, add a WHERE DaysSinceLastAttendanceOfType is NULL, but at the risk of the number being wrong due to a new attendance with an earlier date getting added 
    UPDATE asa
    SET [DaysSinceLastAttendanceOfType] = x.[CalcDaysSinceLastAttendanceOfType]
    FROM [AnalyticsSourceAttendance] asa
    CROSS APPLY (
        SELECT TOP 1 DATEDIFF(DAY, previousAttendanceOfType.[StartDateTime], asa.[StartDateTime]) [CalcDaysSinceLastAttendanceOfType]
        FROM [AnalyticsSourceAttendance] previousAttendanceOfType
        WHERE previousAttendanceOfType.[CurrentPersonKey] IS NOT NULL AND previousAttendanceOfType.[CurrentPersonKey] = asa.[CurrentPersonKey]
            AND previousAttendanceOfType.[AttendanceTypeId] = asa.[AttendanceTypeId]
            AND CONVERT(DATE, previousAttendanceOfType.[StartDateTime]) < CONVERT(DATE, asa.[StartDateTime])
        ORDER BY previousAttendanceOfType.[StartDateTime] DESC
        ) x
    WHERE asa.[CurrentPersonKey] IS NOT NULL AND ISNULL(asa.[DaysSinceLastAttendanceOfType], 0) != ISNULL(x.[CalcDaysSinceLastAttendanceOfType], 0)
END
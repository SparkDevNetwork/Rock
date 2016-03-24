IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]
GO
/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance] '15,16,17,18,19,20,21,22', '2015-01-01 00:00:00', '2015-12-31 23:59:59', null, 0
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]
	  @GroupIds varchar(max) 
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL@CampusIds
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    -- Manipulate dates to only be those dates who's SundayDate value would fall between the selected date range ( so that sunday date does not need to be used in where clause )
	SET @StartDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
	SET @EndDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
    IF @EndDate < @StartDate SET @EndDate = DATEADD( day, 6 + DATEDIFF( day, @EndDate, @StartDate ), @EndDate )

	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
    WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @GroupIds ) ) 
    AND [StartDateTime] BETWEEN @StartDate AND @EndDate
	AND [DidAttend] = 1
	AND ( 
		( @CampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @CampusIds ) ) ) OR  
		( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)	
	AND ( @ScheduleIds IS NULL OR A.[ScheduleId] IN ( SELECT * FROM ufnUtility_CsvToTable( @ScheduleIds ) ) )

	SELECT 
		PD.[PersonId],
		PD.[CampusId],
		PD.[GroupId],
        PD.[GroupName],
		PD.[ScheduleId],
		PD.[StartDateTime],
		PD.[LocationId],
		R.[Name] AS [RoleName],
		L.[Name] AS [LocationName]
	FROM (
		SELECT 
			P.[PersonId],
			A.[CampusId],
			A.[GroupId],
            A.[GroupName],
			A.[ScheduleId],
			A.[StartDateTime],
			A.[LocationId]
		FROM @PersonIdTbl P
		CROSS APPLY (
			SELECT TOP 1 
				A.[CampusId],
				A.[GroupId],
				A.[ScheduleId],
				A.[LocationId],
				G.[Name] as GroupName,
   				A.[StartDateTime]
			FROM (
				SELECT 
					A.[PersonAliasId],
					A.[CampusId],
					A.[GroupId],
					A.[ScheduleId],
					A.[LocationId],
					A.[StartDateTime]
 				FROM [Attendance] A
				WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @GroupIds ) ) 
				AND [StartDateTime] BETWEEN @StartDate AND @EndDate
				AND [DidAttend] = 1
			) A
			INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId] 
			INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
			AND PA.[PersonId] = P.[PersonId]
			AND ( 
				( @CampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @CampusIds ) ) ) OR  
				( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
			)
			AND ( @ScheduleIds IS NULL OR A.[ScheduleId] IN ( SELECT * FROM ufnUtility_CsvToTable( @ScheduleIds ) ) )
			ORDER BY A.[StartDateTime] DESC
		) A
	) PD
	OUTER APPLY (
		SELECT TOP 1 R.[Name]
		FROM [GroupMember] M 
		INNER JOIN [GroupTypeRole] R
			ON R.[Id] = M.[GroupRoleId]
		WHERE M.[GroupId] = PD.[GroupId]
		AND M.[PersonId] = PD.[PersonId]
		AND M.[GroupMemberStatus] <> 0
		ORDER BY R.[Order]
	) R
	LEFT OUTER JOIN [Location] L
		ON L.[Id] = PD.[LocationId]
END
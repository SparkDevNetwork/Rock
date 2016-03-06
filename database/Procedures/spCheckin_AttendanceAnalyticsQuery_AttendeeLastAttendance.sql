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

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]
	  @GroupIds varchar(max) 
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0

AS

BEGIN

	DECLARE @LocGroupIds varchar(max) = @GroupIds
	DECLARE @LocStartDate DateTime = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
	DECLARE @LocEndDate	DateTime = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
	DECLARE @LocCampusIds varchar(max) = @CampusIds
	DECLARE @LocIncludeNullCampusIds bit = @IncludeNullCampusIds

    -- Check for enddate that is previous to start date
    IF @LocEndDate < @LocStartDate SET @LocEndDate = DATEADD( day, 6 + DATEDIFF( day, @LocEndDate, @LocStartDate ), @LocEndDate )

	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
    WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocGroupIds ) ) 
    AND [StartDateTime] BETWEEN @LocStartDate AND @LocEndDate
	AND [DidAttend] = 1
	AND ( 
		( @LocCampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocCampusIds ) ) ) OR  
		( @LocIncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)	

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
				WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocGroupIds ) ) 
				AND [StartDateTime] BETWEEN @LocStartDate AND @LocEndDate
				AND [DidAttend] = 1
			) A
			INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId] 
			INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
			AND PA.[PersonId] = P.[PersonId]
			AND ( 
				( @LocCampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocCampusIds ) ) ) OR  
				( @LocIncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
			)
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
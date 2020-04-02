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
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
                                                                               
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]
	  @GroupIds varchar(max) 
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	SELECT B.[PersonId], B.[CampusId], B.[CampusName], B.[GroupId], B.[GroupName], B.[ScheduleId], B.[StartDateTime], B.[LocationId], B.[RoleName], B.[LocationName] 
	FROM
	(
		SELECT PA.[PersonId], ROW_NUMBER() OVER (PARTITION BY PA.[PersonId] ORDER BY A.[StartDateTime] DESC) AS PersonRowNumber,
			A.[CampusId], CA.[Name] AS [CampusName], O.[GroupId], G.[Name] AS [GroupName], O.[ScheduleId], A.[StartDateTime], O.[LocationId], R.[RoleName], L.[Name] AS [LocationName]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
		INNER JOIN [Group] G ON G.[Id] = O.[GroupId]
		INNER JOIN @GroupTbl [G2] ON [G2].[Id] = G.[Id]
		OUTER APPLY (
			SELECT TOP 1 R.[Name] AS [RoleName]
			FROM [GroupMember] M 
			INNER JOIN [GroupTypeRole] R
				ON R.[Id] = M.[GroupRoleId]
			WHERE M.[GroupId] = G.[Id]
			AND M.[PersonId] = PA.[PersonId]
			AND M.[GroupMemberStatus] <> 0
			ORDER BY R.[Order]
		) R
		LEFT OUTER JOIN [Location] L
			ON L.[Id] = O.[LocationId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
		LEFT OUTER JOIN [Campus] [CA] ON [A].[CampusId] = [CA].[Id]
		WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
		AND [DidAttend] = 1
		AND ( 
			( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		)
		AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	) B
	WHERE B.PersonRowNumber = 1

END
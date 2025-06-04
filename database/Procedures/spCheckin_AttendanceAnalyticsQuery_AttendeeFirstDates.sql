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
	<param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
	  @GroupTypeIds varchar(max)
	, @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT value FROM STRING_SPLIT(@CampusIds,',')

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT value FROM STRING_SPLIT(@ScheduleIds,',')

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT value FROM STRING_SPLIT(@GroupIds,',')

	DECLARE @GroupTypeTbl TABLE ( [Id] int )
	INSERT INTO @GroupTypeTbl SELECT value FROM STRING_SPLIT(@GroupTypeIds,',')

	-- Get all the attendees
	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
	INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
	LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
	LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
    WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
	AND [DidAttend] = 1
	AND ( 
		( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
		( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)
	AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )

	-- Get the first 5 occasions on which each person attended any of the selected group types regardless of group or campus.
	-- Multiple attendances on the same date are considered as a single occasion.
	SELECT DISTINCT
	    [PersonId]
	    , [TimeAttending]
	    , [StartDate]
	FROM (
	    SELECT 
	        [P].[Id] AS [PersonId]
	        , DENSE_RANK() OVER ( PARTITION BY [P].[Id] ORDER BY [AO].[OccurrenceDate] ) AS [TimeAttending]
	        , [AO].[OccurrenceDate] AS [StartDate]
	    FROM
	        [Attendance] [A]
	        INNER JOIN [AttendanceOccurrence] [AO] ON [AO].[Id] = [A].[OccurrenceId]
	        INNER JOIN [Group] [G] ON [G].[Id] = [AO].[GroupId]
	        INNER JOIN [PersonAlias] [PA] ON [PA].[Id] = [A].[PersonAliasId] 
	        INNER JOIN [Person] [P] ON [P].[Id] = [PA].[PersonId]
	        INNER JOIN @GroupTypeTbl [GT] ON [GT].[id] = [G].[GroupTypeId]
	    WHERE 
	        [P].[Id] IN ( SELECT [PersonId] FROM @PersonIdTbl )
	        AND [DidAttend] = 1
	) [X]
    WHERE [X].[TimeAttending] <= 5

END
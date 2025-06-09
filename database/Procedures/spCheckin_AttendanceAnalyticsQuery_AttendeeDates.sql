/*
<doc>
	<summary>
 		This function returns attendee person ids and the dates they attended based on selected filter criteria
	</summary>

	<returns>
		* PersonId
		* SundayDate
		* MonthDate
		* Year Date
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
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-12 00:00:00', null, 0, null
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] @GroupIds VARCHAR(max)
	,@StartDate DATETIME = NULL
	,@EndDate DATETIME = NULL
	,@CampusIds VARCHAR(max) = NULL
	,@IncludeNullCampusIds BIT = 0
	,@ScheduleIds VARCHAR(max) = NULL
	WITH RECOMPILE
AS
BEGIN
	--  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ([Id] INT)

	INSERT INTO @CampusTbl
	SELECT value
	FROM STRING_SPLIT(@CampusIds, ',')

	DECLARE @ScheduleTbl TABLE ([Id] INT)

	INSERT INTO @ScheduleTbl
	SELECT value
	FROM STRING_SPLIT(@ScheduleIds, ',')

	DECLARE @GroupTbl TABLE ([Id] INT)

	INSERT INTO @GroupTbl
	SELECT value
	FROM STRING_SPLIT(@GroupIds, ',')

	-- Get all the attendance
	SELECT PA.[PersonId]
		,A.[SundayDate]
		,DATEADD(day, - (DATEPART(day, [SundayDate])) + 1, [SundayDate]) AS [MonthDate]
		,DATEADD(day, - (DATEPART(dayofyear, [SundayDate])) + 1, [SundayDate]) AS [YearDate]
	FROM (
		SELECT [PersonAliasId]
			,O.[GroupId]
			,[CampusId]
			,o.[SundayDate]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
		WHERE o.SundayDate BETWEEN @startDateSundayDate AND @endDateSundayDate AND A.[DidAttend] = 1 AND ((@CampusIds IS NULL OR [C].[Id] IS NOT NULL) OR (@IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL)) AND (@ScheduleIds IS NULL OR [S].[Id] IS NOT NULL)
		) A
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
END
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
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
	<param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates] 14, '15,16,17,18,19,20,21,22', '2015-01-01 00:00:00', '2015-12-31 23:59:59', null, 0
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
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

    -- Manipulate dates to only be those dates who's SundayDate value would fall between the selected date range ( so that sunday date does not need to be used in where clause )
	SET @StartDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
	SET @EndDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
    IF @EndDate < @StartDate SET @EndDate = DATEADD( day, 6 + DATEDIFF( day, @EndDate, @StartDate ), @EndDate )

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	DECLARE @GroupTypeTbl TABLE ( [Id] int )
	INSERT INTO @GroupTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupTypeIds,'') )

	-- Get all the attendees
	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN @GroupTbl [G] ON [G].[Id] = A.[GroupId]
	LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
	LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
    WHERE [StartDateTime] BETWEEN @StartDate AND @EndDate
	AND [DidAttend] = 1
	AND ( 
		( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
		( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)
	AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )

    -- Get the first 5 times they attended any of the selected group types (any group or campus)
	SELECT 
		P.[PersonId],
		D.[TimeAttending],
		D.[StartDate]
	FROM @PersonIdTbl P
	CROSS APPLY ( 
		SELECT TOP 5 
			ROW_NUMBER() OVER (ORDER BY [StartDate]) AS [TimeAttending],
			[StartDate]
        FROM (
            SELECT DISTINCT [StartDate]
		    FROM [vCheckin_GroupTypeAttendance] A
			INNER JOIN @GroupTypeTbl [G] ON [G].[id] = [A].[GroupTypeId]
		    AND A.[PersonId] = P.[PersonId]
        ) S
	) D

END
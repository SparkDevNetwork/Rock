﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates]
GO

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
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] '15,16,17,18,19,20,21,22', '2015-01-01 00:00:00', '2015-12-31 23:59:59', null, 0
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates]
	  @GroupIds varchar(max)
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
	SET @EndDate = DATEADD( second, -1, DATEADD( day, 1, @EndDate ) )

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	-- Get all the attendance
	SELECT 
		PA.[PersonId],
		A.[SundayDate],
		DATEADD( day, -( DATEPART( day, [SundayDate] ) ) + 1, [SundayDate] ) AS [MonthDate],
		DATEADD( day, -( DATEPART( dayofyear, [SundayDate] ) ) + 1, [SundayDate] ) AS [YearDate]
	FROM (
		SELECT 
			[PersonAliasId],
			O.[GroupId],
			[CampusId],
			DATEADD( day, ( 6 - ( DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), [StartDateTime] ) % 7 ) ), CONVERT( date, [StartDateTime] ) ) AS [SundayDate]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
        WHERE [StartDateTime] BETWEEN @StartDate AND @EndDate
		AND A.[DidAttend] = 1
		AND ( 
			( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		)
		AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	) A 
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]

END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]
GO

/*
<doc>
	<summary>
 		This function returns the people that attended based on selected filter criteria
	</summary>

	<returns>
		* Id 
		* NickName
		* LastName
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
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees] '15,16,17,18,19,20,21,22', '2015-01-01 00:00:00', '2015-12-31 23:59:59', null, 0, 0, 0
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]
	  @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    -- Manipulate dates to only be those dates who's SundayDate value would fall between the selected date range ( so that sunday date does not need to be used in where clause )
	SET @StartDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
	SET @EndDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
    IF @EndDate < @StartDate SET @EndDate = DATEADD( day, 6 + DATEDIFF( day, @EndDate, @StartDate ), @EndDate )

	-- Get all the attendees
    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person who attended
	    SELECT	
		    P.[Id],
		    P.[NickName],
		    P.[LastName],
		    P.[Email],
		    P.[BirthDate],
            P.[ConnectionStatusValueId]
	    FROM (
		    SELECT DISTINCT PA.[PersonId]
			FROM (
				SELECT 
					A.[PersonAliasId],
					A.[CampusId],
					A.[ScheduleId]
 				FROM [Attendance] A
				WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @GroupIds ) ) 
				AND [StartDateTime] BETWEEN @StartDate AND @EndDate
				AND [DidAttend] = 1
			) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
			WHERE ( 
				( @CampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @CampusIds ) ) ) OR  
				( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
			)
			AND ( @ScheduleIds IS NULL OR A.[ScheduleId] IN ( SELECT * FROM ufnUtility_CsvToTable( @ScheduleIds ) ) )
	    ) [Attendee]
	    INNER JOIN [Person] P 
			ON P.[Id] = [Attendee].[PersonId]

    END
    ELSE
    BEGIN

        DECLARE @AdultRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
        DECLARE @ChildRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

        IF @IncludeParentsWithChild = 1 
        BEGIN

            -- Child attended, also include their parents
	        SELECT	
		        C.[Id],
		        C.[NickName],
		        C.[LastName],
		        C.[Email],
		        C.[BirthDate],
                C.[ConnectionStatusValueId],
		        A.[Id] AS [ParentId],
		        A.[NickName] AS [ParentNickName],
		        A.[LastName] AS [ParentLastName],
		        A.[Email] AS [ParentEmail],
		        A.[BirthDate] AS [ParentBirthDate]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						A.[ScheduleId]
 					FROM [Attendance] A
					WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @GroupIds ) ) 
					AND [StartDateTime] BETWEEN @StartDate AND @EndDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				WHERE ( 
					( @CampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @CampusIds ) ) ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR A.[ScheduleId] IN ( SELECT * FROM ufnUtility_CsvToTable( @ScheduleIds ) ) )
	        ) [Attendee]
	        INNER JOIN [Person] C 
				ON C.[Id] = [Attendee].[PersonId]
            INNER JOIN [GroupMember] GMC
                ON GMC.[PersonId] = C.[Id]
	            AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [GroupMember] GMA
	            ON GMA.[GroupId] = GMC.[GroupId]
	            AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [Person] A 
				ON A.[Id] = GMA.[PersonId]

        END
        
        IF @IncludeChildrenWithParents = 1
        BEGIN

            -- Parents attended, include their children
	        SELECT	
		        A.[Id],
		        A.[NickName],
		        A.[LastName],
		        A.[Email],
		        A.[BirthDate],
                A.[ConnectionStatusValueId],
		        C.[Id] AS [ChildId],
		        C.[NickName] AS [ChildNickName],
		        C.[LastName] AS [ChildLastName],
		        C.[Email] AS [ChildEmail],
		        C.[BirthDate] AS [ChildBirthDate]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						A.[ScheduleId]
 					FROM [Attendance] A
					WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @GroupIds ) ) 
					AND [StartDateTime] BETWEEN @StartDate AND @EndDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				WHERE ( 
					( @CampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @CampusIds ) ) ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR A.[ScheduleId] IN ( SELECT * FROM ufnUtility_CsvToTable( @ScheduleIds ) ) )
	        ) [Attendee]
	        INNER JOIN [Person] A 
				ON A.[Id] = [Attendee].[PersonId]
            INNER JOIN [GroupMember] GMA
                ON GMA.[PersonId] = A.[Id]
	            AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [GroupMember] GMC
	            ON GMC.[GroupId] = GMA.[GroupId]
	            AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [Person] C 
				ON C.[Id] = GMC.[PersonId]

        END

    END

END
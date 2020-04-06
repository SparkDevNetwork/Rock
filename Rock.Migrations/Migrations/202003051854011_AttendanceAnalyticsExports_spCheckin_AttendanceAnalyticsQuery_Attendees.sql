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
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, 0, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]
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

	-- Get all the attendees
    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person who attended
	    SELECT	
		    P.[Id],
		    P.[NickName],
		    P.[LastName],
			P.[Gender],
		    P.[Email],
            P.[GivingId],
		    P.[BirthDate],
            P.[ConnectionStatusValueId],
			P.[GraduationYear]
	    FROM (
		    SELECT DISTINCT PA.[PersonId]
			FROM (
				SELECT 
					A.[PersonAliasId],
					A.[CampusId],
					O.[ScheduleId]
 				FROM [Attendance] A
				INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
				INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
				WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
				AND [DidAttend] = 1
			) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
			LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
			LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
		    WHERE ( 
			    ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			    ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		    )
		    AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
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
				C.[Gender],
                C.[Email],
                C.[GivingId],
                C.[BirthDate],
                C.[ConnectionStatusValueId],
				C.[GraduationYear],
                A.[Id] AS [ParentId],
                A.[NickName] AS [ParentNickName],
                A.[LastName] AS [ParentLastName],
                A.[Email] AS [ParentEmail],
                A.[GivingId] as [ParentGivingId],
                A.[BirthDate] AS [ParentBirthDate],
				A.[Gender] AS [ParentGender]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						O.[ScheduleId]
 					FROM [Attendance] A
					INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
					INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
					WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
				LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
				WHERE ( 
					( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
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
				A.[Gender],
                A.[Email],
                A.[GivingId],
                A.[BirthDate],
                A.[ConnectionStatusValueId],
                C.[Id] AS [ChildId],
                C.[NickName] AS [ChildNickName],
                C.[LastName] AS [ChildLastName],
                C.[Email] AS [ChildEmail],
                C.[GivingId] as [ChildGivingId],
                C.[BirthDate] AS [ChildBirthDate],
				C.[Gender] as [ChildGender],
				C.[GraduationYear] as [ChildGraduationYear]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						O.[ScheduleId]
 					FROM [Attendance] A
					INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
					INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
					WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
				LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
				WHERE ( 
					( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
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
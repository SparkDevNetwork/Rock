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

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]
	  @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0

AS

BEGIN

	DECLARE @LocGroupIds varchar(max) = @GroupIds
	DECLARE @LocStartDate DateTime = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
	DECLARE @LocEndDate	DateTime = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
	DECLARE @LocCampusIds varchar(max) = @CampusIds
	DECLARE @LocIncludeNullCampusIds bit = @IncludeNullCampusIds
	DECLARE @LocIncludeParentsWithChild bit = @IncludeParentsWithChild
	DECLARE @LocIncludeChildrenWithParents bit = @IncludeChildrenWithParents

    -- Check for enddate that is previous to start date
    IF @LocEndDate < @LocStartDate SET @LocEndDate = DATEADD( day, 6 + DATEDIFF( day, @LocEndDate, @LocStartDate ), @LocEndDate )

	-- Get all the attendees
    IF @LocIncludeChildrenWithParents = 0 AND @LocIncludeParentsWithChild = 0
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
					A.[CampusId]
 				FROM [Attendance] A
				WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocGroupIds ) ) 
				AND [StartDateTime] BETWEEN @LocStartDate AND @LocEndDate
				AND [DidAttend] = 1
			) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
			WHERE ( 
				( @LocCampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocCampusIds ) ) ) OR  
				( @LocIncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
			)
	    ) [Attendee]
	    INNER JOIN [Person] P 
			ON P.[Id] = [Attendee].[PersonId]

    END
    ELSE
    BEGIN

        DECLARE @AdultRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
        DECLARE @ChildRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

        IF @LocIncludeParentsWithChild = 1 
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
						A.[CampusId]
 					FROM [Attendance] A
					WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocGroupIds ) ) 
					AND [StartDateTime] BETWEEN @LocStartDate AND @LocEndDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				WHERE ( 
					( @LocCampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocCampusIds ) ) ) OR  
					( @LocIncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
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
        
        IF @LocIncludeChildrenWithParents = 1
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
						A.[CampusId]
 					FROM [Attendance] A
					WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocGroupIds ) ) 
					AND [StartDateTime] BETWEEN @LocStartDate AND @LocEndDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				WHERE ( 
					( @LocCampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @LocCampusIds ) ) ) OR  
					( @LocIncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
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
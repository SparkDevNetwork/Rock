/*
<doc>
	<summary>
 		This function returns any person ids for people that have attended previously but who have not attended since the beginning date
	</summary>

	<returns>
		* PersonId 
		* SundayDate - Last time attended
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDateTime' datatype='datetime'>Beginning date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees] 14, '15,16,17,18,19,20,21,22', '2015-01-01 00:00:00', '2015-12-31 23:59:59', null, 0, 0, 0
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees]
	  @GroupTypeId int
	, @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0

AS

BEGIN

	-- Create local variables to help with parameter-sniffing issue on query plan optimization
	DECLARE @LocGroupTypeId int = @GroupTypeId
	DECLARE @LocGroupIds varchar(max) = @GroupIds
	DECLARE @LocStartDate DateTime = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
	DECLARE @LocEndDate	DateTime = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
	DECLARE @LocCampusIds varchar(max) = @CampusIds
	DECLARE @LocIncludeNullCampusIds bit = @IncludeNullCampusIds
	DECLARE @LocIncludeParentsWithChild bit = @IncludeParentsWithChild
	DECLARE @LocIncludeChildrenWithParents bit = @IncludeChildrenWithParents
	
    -- Check for enddate that is previous to start date
    IF @LocEndDate < @LocStartDate SET @LocEndDate = DATEADD( day, 6 + DATEDIFF( day, @LocEndDate, @LocStartDate ), @LocEndDate )

	DECLARE @PersonIdTbl TABLE ( [Id] INT NOT NULL )

    -- Find all the person ids for people who belong to any of the selected groups and have not attended any group/campus of selected group type
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT M.[PersonId]
	FROM [Group] G 
	INNER JOIN [GroupMember] M
		ON M.[GroupId] = G.[Id]
		AND M.[GroupMemberStatus] = 1
    WHERE G.[Id] IN ( SELECT * FROM ufnUtility_CsvToTable( @LocGroupIds ) ) 
	AND M.[PersonId] NOT IN (
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
    )


    IF @LocIncludeChildrenWithParents = 0 AND @LocIncludeParentsWithChild = 0
    BEGIN

        -- Just return the person 
	    SELECT	
		    P.[Id],
		    P.[NickName],
		    P.[LastName],
		    P.[Email],
		    P.[BirthDate],
            P.[ConnectionStatusValueId]
		FROM @PersonIdTbl M
	    INNER JOIN [Person] P ON P.[Id] = M.[Id]

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
			FROM @PersonIdTbl M
	        INNER JOIN [Person] C 
				ON C.[Id] = M.[Id]
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
			FROM @PersonIdTbl M
	        INNER JOIN [Person] A 
				ON A.[Id] = M.[Id]
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

    -- Get the first 5 times they attended this group type (any group or campus)
	SELECT 
		P.[Id] AS [PersonId],
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
		    WHERE A.[GroupTypeId] = @LocGroupTypeId
		    AND A.[PersonId] = P.[Id]
        ) S
	) D

    -- Get the last time they attended
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
			P.[Id] AS [PersonId],
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
                G.[Name] AS [GroupName],
				A.[ScheduleId],
				A.[LocationId],
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
				AND [StartDateTime] < @LocStartDate
				AND [DidAttend] = 1
			) A
			INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId] 
			INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
			AND PA.[PersonId] = P.[Id]
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
/*
<doc>
    <summary>
         This function returns any person ids for people that have attended previously but who have not attended since the beginning date
    </summary>

    <returns>
        * PersonId 
        * SundayDate - Last time attended
    </returns>
    <param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
    <param name='StartDateTime' datatype='datetime'>Beginning date range filter</param>
    <param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
    <param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
    <param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
    <remarks>    
    </remarks>
    <code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null, 0, 0
    </code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees]
      @GroupTypeIds varchar(max)
    , @GroupIds varchar(max)
    , @StartDate datetime = NULL
    , @EndDate datetime = NULL
    , @CampusIds varchar(max) = NULL
    , @IncludeNullCampusIds bit = 0
    , @ScheduleIds varchar(max) = NULL
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0
    WITH RECOMPILE

AS

BEGIN

   	--  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

    DECLARE @PersonIdTbl TABLE ( [Id] INT NOT NULL )

    DECLARE @CampusTbl TABLE ( [Id] int )
    INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

    DECLARE @ScheduleTbl TABLE ( [Id] int )
    INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

    DECLARE @GroupTbl TABLE ( [Id] int )
    INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

    DECLARE @GroupTypeTbl TABLE ( [Id] int )
    INSERT INTO @GroupTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupTypeIds,'') )

    -- Find all the person ids for people who belong to any of the selected groups and have not attended any group/campus of selected group type
    INSERT INTO @PersonIdTbl
    SELECT DISTINCT M.[PersonId]
    FROM @GroupTbl G
    INNER JOIN [GroupMember] M
        ON M.[GroupId] = G.[Id]
        AND M.[GroupMemberStatus] = 1
    WHERE M.[PersonId] NOT IN (
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
    )

    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person 
 SELECT    
            P.[Id],
            P.[NickName],
            P.[LastName],
            P.[Gender],
            P.[Email],
            P.[GivingId],
            P.[BirthDate],
            P.[ConnectionStatusValueId]
        FROM @PersonIdTbl M
        INNER JOIN [Person] P ON P.[Id] = M.[Id]

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
                A.[Id] AS [ParentId],
                A.[NickName] AS [ParentNickName],
                A.[LastName] AS [ParentLastName],
                A.[Email] AS [ParentEmail],
                A.[GivingId] as [ParentGivingId],
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
        
        IF @IncludeChildrenWithParents = 1
        BEGIN

            -- Parents attended, include their children
            SELECT    
                A.[Id],
                A.[NickName],
                A.[LastName],
                A.[Gender],
                A.[Email],
                A.[GivingId] as [GivingId],
                A.[BirthDate],
                A.[ConnectionStatusValueId],
                C.[Id] AS [ChildId],
                C.[NickName] AS [ChildNickName],
                C.[LastName] AS [ChildLastName],
                C.[Email] AS [ChildEmail],
                C.[GivingId] as [ChildGivingId],
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
            INNER JOIN @GroupTypeTbl [G] ON [G].[id] = [A].[GroupTypeId]
            AND A.[PersonId] = P.[Id]
        ) S
    ) D

    -- Get the last time they attended
    SELECT 
        PD.[PersonId],
        PD.[CampusId],
		CA.[Name] AS [CampusName],
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
                    O.[GroupId],
                    O.[ScheduleId],
                    O.[LocationId],
                    A.[StartDateTime]
                 FROM [Attendance] A
                INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
                INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
                WHERE o.[SundayDate] < @startDateSundayDate
                AND [DidAttend] = 1
            ) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId] 
            INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
            LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
            LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
            WHERE PA.[PersonId] = P.[Id]
            AND ( 
                ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
                ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
            )
            AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
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
	LEFT OUTER JOIN [Campus] CA ON PD.[CampusId] = CA.[Id]
END
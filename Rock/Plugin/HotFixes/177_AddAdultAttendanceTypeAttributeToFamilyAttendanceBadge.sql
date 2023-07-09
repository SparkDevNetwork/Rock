ALTER PROCEDURE[dbo].[spCheckin_BadgeAttendance]
                  @PersonId int
                  , @RoleGuid uniqueidentifier = null
                  , @ReferenceDate datetime = null
                  , @MonthCount int = 24
				  , @ShowAsIndividual bit = 0
              AS
              BEGIN
                  DECLARE @cROLE_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
              
                  DECLARE @cROLE_CHILD uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
              
                  DECLARE @cGROUP_TYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
              
                  DECLARE @StartDay datetime
              
                  DECLARE @LastDay datetime

                  -- if role( adult / child ) is unknown determine it

    IF( @RoleGuid IS NULL )

    BEGIN
        SELECT TOP 1 @RoleGuid = gtr.[Guid]

            FROM[GroupTypeRole] gtr
               INNER JOIN[GroupMember] gm ON gm.[GroupRoleId] = gtr.[Id]

                INNER JOIN[Group] g ON g.[Id] = gm.[GroupId]

            WHERE gm.[PersonId] = @PersonId
AND g.[GroupTypeId] = (SELECT[ID] FROM [GroupType] WHERE[Guid] = @cGROUP_TYPE_FAMILY)

    END

	-- if start date null get today's date

    IF( @ReferenceDate IS NULL)

        SET @ReferenceDate = getdate()

    -- set data boundaries
    SET @LastDay = [dbo].[ufnUtility_GetLastDayOfMonth] (@ReferenceDate) -- last day is most recent day
SET @StartDay = DATEADD( M, DATEDIFF(M, 0, DATEADD(month, ((@MonthCount -1) * -1), @LastDay)), 0) -- start day is the 1st of the first full month of the oldest day

	-- make sure last day is not in future( in case there are errant checkin data)

    IF( @LastDay > getdate())

        SET @LastDay = getdate()

    --PRINT 'Last Day: ' + CONVERT( VARCHAR, @LastDay, 101)
	--PRINT 'Start Day: ' + CONVERT( VARCHAR, @StartDay, 101)

    DECLARE @familyMemberPersonIds table( [PersonId] int);

		IF( @RoleGuid = @cROLE_CHILD OR @ShowAsIndividual = 1 )
	        INSERT INTO @familyMemberPersonIds SELECT @PersonId
        ELSE IF( @RoleGuid = @cROLE_ADULT )
			INSERT INTO @familyMemberPersonIds SELECT[Id] FROM[dbo].[ufnCrm_FamilyMembersOfPersonId]
        (@PersonId) 

	-- query for attendance data

    SELECT
        COUNT( b.[Attended]) AS[AttendanceCount]
		, (SELECT[dbo].[ufnUtility_GetNumberOfSundaysInMonth] (DATEPART(year, b.[SundayDate]), DATEPART( month, b.[SundayDate]), 'True' )) AS[SundaysInMonth]
		, DATEPART( month, b.[SundayDate]) AS[Month]
		, DATEPART( year, b.[SundayDate]) AS[Year]
     FROM(
         SELECT
             s.[SundayDate], NULL AS [Attended]
        FROM
             dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
 
         UNION ALL
 
             SELECT
                 DISTINCT ao.[SundayDate], 1 AS[Attended]
             FROM
 
                 [AttendanceOccurrence] ao
                 INNER JOIN[dbo].[ufnCheckin_WeeklyServiceGroups]() wg ON ao.[GroupId] = wg.[Id]
 
                 INNER JOIN [Attendance] a ON ao.[Id] = a.[OccurrenceId] AND a.[DidAttend] = 1
 
                 INNER JOIN [PersonAlias] pa ON a.[PersonAliasId] = pa.[Id] AND pa.[PersonId] IN (SELECT[PersonId] FROM @familyMemberPersonIds )

            WHERE
                ao.[OccurrenceDate] BETWEEN @StartDay AND @LastDay
    ) b
    GROUP BY DATEPART( month, b.[SundayDate]), DATEPART( year, b.[SundayDate])

    OPTION( MAXRECURSION 1000)
END

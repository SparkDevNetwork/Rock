/*
<doc>
	<summary>
 		This function returns the attendance data needed for the Attendance Badge. If no family role (adult/child)
		is given it is looked up.  If the individual is an adult it will return family attendance if it's a child
		it will return the individual's attendance. If a person is in two families once as a child once as an
		adult it will pick the first role it finds.
	</summary>

	<returns>
		* AttendanceCount
		* SundaysInMonth
		* Month
		* Year
	</returns>
	<param name="PersonId" datatype="int">Person the badge is for</param>
	<param name="Role Guid" datatype="uniqueidentifier">The role of the person in the family (optional)</param>
	<param name="Reference Date" datatype="datetime">A date in the last month for the badge (optional, default is today)</param>
	<param name="Number of Months" datatype="int">Number of months to display (optional, default is 24)</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_BadgeAttendance] 2 -- Ted Decker (adult)
		EXEC [dbo].[spCheckin_BadgeAttendance] 4 -- Noah Decker (child)
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_BadgeAttendance]
	@PersonId int 
	, @RoleGuid uniqueidentifier = null
	, @ReferenceDate datetime = null
	, @MonthCount int = 24
AS
BEGIN

	DECLARE @cROLE_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cROLE_CHILD uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
	DECLARE @cGROUP_TYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
    DECLARE @StartDay datetime
	DECLARE @LastDay datetime

	-- if role (adult/child) is unknown determine it
	IF (@RoleGuid IS NULL)
	BEGIN
		SELECT TOP 1 @RoleGuid = gtr.[Guid] 
			FROM [GroupTypeRole] gtr
				INNER JOIN [GroupMember] gm ON gm.[GroupRoleId] = gtr.[Id]
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
			WHERE gm.[PersonId] = @PersonId 
				AND g.[GroupTypeId] = (SELECT [ID] FROM [GroupType] WHERE [Guid] = @cGROUP_TYPE_FAMILY)
	END

	-- if start date null get today's date
	IF @ReferenceDate is null
	BEGIN
		SET @ReferenceDate = getdate()
	END

	-- set data boundaries
	SET @LastDay = dbo.ufnUtility_GetLastDayOfMonth(@ReferenceDate) -- last day is most recent day

	-- make sure last day is not in future (in case there are errant checkin data)
	-- Moved above @startDay so that month calculation isn't effected by errant data.
	IF (@LastDay > getdate())
	BEGIN
		SET @LastDay = getdate()
	END

	SET @StartDay = DATEADD(M, DATEDIFF(M, 0, DATEADD(month, ((@MonthCount -1) * -1), @LastDay)), 0) -- start day is the 1st of the first full month of the oldest day

	declare @familyMemberPersonIds table (personId INT, isperson bit); 
    declare @groupIds table (groupId int);

    insert into @familyMemberPersonIds SELECT id, CASE WHEN id = @personid THEN 1 ELSE 0 END IsPerson FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId);
    insert into @groupIds SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]();

	DECLARE @SundayList TABLE (AttendedSunday DATE);

	-- Grab all the sundays that attender or family attended
	INSERT INTO @SundayList(AttendedSunday)
	SELECT
		O.[SundayDate] AS [AttendedSunday]
	FROM Attendance a 
	INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
	INNER JOIN @groupIds gid ON gid.groupId = o.groupid
	WHERE
		a.PersonAliasId IN (
			SELECT pa.id 
			FROM personalias pa
			INNER JOIN @familyMemberPersonIds fmid
				ON fmid.personId = pa.PersonId 
				AND 1 = CASE WHEN @RoleGuid = @cROLE_CHILD THEN fmid.isPerson ELSE 1 END -- Determine if not child and if so show family attendance vs just individual
			)

	-- Results set
	SELECT 
		COUNT([Attended]) AS [AttendanceCount]
		, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
		, DATEPART(month, [SundayDate]) AS [Month]
		, DATEPART(year, [SundayDate]) AS [Year]
	FROM (
		SELECT s.[SundayDate], a.[Attended]
			FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
			LEFT OUTER JOIN (	
				-- For some reason splitting this into a temptable increased performance by 600%
				SELECT DISTINCT sl.AttendedSunday, 1 as [Attended] FROM @SundayList sl
				) a ON a.[AttendedSunday] = s.[SundayDate]
	) [CheckinDates]
	GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
	OPTION (MAXRECURSION 1000)
	
END
/* ====================================================== 
-- NewSpring Script #8: 
-- Creates the custom metrics 
   ====================================================== */

-- Make sure you're using the right Rock database:
--USE Rock

SET NOCOUNT ON;

-- Set common variables 
DECLARE @SQL AS NVARCHAR(MAX);
DECLARE @True bit = 1;
DECLARE @False bit = 0;
DECLARE @Order int = 0;
DECLARE @MetricSourceSQLId AS INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764');
DECLARE @ScheduleEntityTypeId AS INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Schedule');
DECLARE @MetricCategoryEntityTypeId AS INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.MetricCategory');
DECLARE @MetricScheduleCategoryId AS INT = (SELECT [Id] FROM [Category] WHERE EntityTypeId = @ScheduleEntityTypeId AND Name = 'Metrics');
DECLARE @MetricScheduleId AS INT = (SELECT [Id] FROM [Schedule] WHERE CategoryId = @MetricScheduleCategoryId AND Name = 'Metric Schedule');
DECLARE @CampusEntityTypeId AS INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Campus');
DECLARE @InsertedId AS INT;
DECLARE @NextStepsCategoryId AS INT = (SELECT TOP 1 Id From [Category] WHERE EntityTypeId = @MetricCategoryEntityTypeId AND Name = 'Next Steps');
DECLARE @AttendanceCategoryId AS INT = (SELECT TOP 1 Id From [Category] WHERE EntityTypeId = @MetricCategoryEntityTypeId AND Name = 'Attendance');
DECLARE @FuseAttendanceCategoryId AS INT = (SELECT TOP 1 Id From [Category] WHERE EntityTypeId = @MetricCategoryEntityTypeId AND Name = 'Fuse Attendance' AND ParentCategoryId = @AttendanceCategoryId);
DECLARE @KSAttendanceCategoryId AS INT = (SELECT TOP 1 Id From [Category] WHERE EntityTypeId = @MetricCategoryEntityTypeId AND Name = 'KidSpring Attendance' AND ParentCategoryId = @AttendanceCategoryId);

/* ======================================================
--	TOTAL HOME GROUP MEMBERS 
--	The number of active members of active groups falling under "Home Groups"
   ======================================================*/

SET @SQL = N'
	DECLARE @GroupMemberStatusActive int = 1;

	SELECT
		COUNT(DISTINCT gm.PersonId) as Value
		, g.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM 
		[GroupMember] gm
		JOIN [Group] g ON gm.GroupId = g.Id
		JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
	WHERE
		gt.Name = ''Home Group''
		AND g.IsActive = 1
		AND gm.GroupMemberStatus = @GroupMemberStatusActive
	GROUP BY
		g.CampusId;
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Total Active Home Group Members'
	, 'Metric to track the number of active members in active Home Groups'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @NextStepsCategoryId, @Order, NEWID() );

/* ======================================================
--	TOTAL HOME/FUSE GROUP MEMBERS 
--	The number of active members of active groups falling under "Home Groups" or "Fuse Groups"
   ======================================================*/

SET @SQL = N'
	DECLARE @GroupMemberStatusActive int = 1;

	SELECT
		COUNT(DISTINCT gm.PersonId) as Value
		, g.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM 
		[GroupMember] gm
		JOIN [Group] g ON gm.GroupId = g.Id
		JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
	WHERE
		(
			gt.Name = ''Home Group''
			OR gt.Name = ''Fuse Group''
		)
		AND g.IsActive = 1
		AND gm.GroupMemberStatus = @GroupMemberStatusActive
	GROUP BY
		g.CampusId;
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Total Active Home/Fuse Group Members'
	, 'The number of people that are active members of active groups falling under Home Groups or Fuse Groups'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @NextStepsCategoryId, @Order, NEWID() );

/* ======================================================
--	AVERAGE HOME/FUSE GROUP SIZE 
--	The average number of active members of an active group falling under "Home Groups" or "Fuse Groups"
   ======================================================*/

SET @SQL = N'
	DECLARE @GroupMemberStatusActive int = 1;

	SELECT
		AVG(sub.NumMembers) as Value
		, sub.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM 
		(
			SELECT 
				COUNT(DISTINCT gm.PersonId) AS NumMembers
				, g.CampusId
			FROM
				[GroupMember] gm
				JOIN [Group] g ON gm.GroupId = g.Id
				JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
			WHERE
				(
					gt.Name = ''Home Group''
					OR gt.Name = ''Fuse Group''
				)
				AND g.IsActive = 1
				AND gm.GroupMemberStatus = @GroupMemberStatusActive
			GROUP BY
				g.Id
				, g.CampusId
		) sub
	GROUP BY
		sub.CampusId;
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Average Home/Fuse Group Size'
	, 'The average number of people that are active members of an active group falling under Home Groups or Fuse Groups'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @NextStepsCategoryId, @Order, NEWID() );

/* ======================================================
--	PERCENT OF SUNDAY ATTENDANCE IN A GROUP 
--	The percent of the latest sunday attendance population 
--	that are active members of an active group falling under 
--	"Home Groups" or "Fuse Groups"
   ======================================================*/

SET @SQL = N'
	DECLARE @GroupMemberStatusActive int = 1;
	DECLARE @serviceAttendanceMetricId AS INT = 3340;
	DECLARE @today AS DATE = GETDATE();
	DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));

	WITH cte_AttendanceByCampusOnMostRecentSunday AS (
		SELECT
			SUM(YValue) AS Attendance
			, EntityId AS CampusId
		FROM
			[MetricValue]
		WHERE
			MetricId = @serviceAttendanceMetricId
			AND CONVERT(DATE, MetricValueDateTime) = @recentSundayDate
		GROUP BY
			EntityId
	),
	cte_PeopleInAGroupByCampus AS (
		SELECT
			COUNT(DISTINCT gm.PersonId) as PeopleInAGroup
			, g.CampusId
		FROM 
			[GroupMember] gm
			JOIN [Group] g ON gm.GroupId = g.Id
			JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
		WHERE
			(
				gt.Name = ''Home Group''
				OR gt.Name = ''Fuse Group''
			)
			AND g.IsActive = 1
			AND gm.GroupMemberStatus = @GroupMemberStatusActive
		GROUP BY
			g.CampusId
	)
	SELECT
		CONVERT(INT, ROUND(CONVERT(DECIMAL, ISNULL(g.PeopleInAGroup, 0)) / a.Attendance * 100, 0)) as Value
		, a.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM 
		[cte_AttendanceByCampusOnMostRecentSunday] a
		LEFT JOIN [cte_PeopleInAGroupByCampus] g ON g.CampusId = a.CampusId;
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Percent of Sunday Attendance in a Group'
	, 'The percent of the latest sunday attendance population that are active members of an active group falling under Home Groups or Fuse Groups'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @NextStepsCategoryId, @Order, NEWID() );

/* ======================================================
--	NUMBER OF ACTIVE HOME/FUSE GROUP LEADERS 
--	The number of active leaders in active Home or Fuse Groups
   ======================================================*/

SET @SQL = N'
	DECLARE @GroupMemberStatusActive int = 1;

	SELECT
		COUNT(DISTINCT gm.PersonId) as Value
		, g.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM 
		[GroupMember] gm
		JOIN [Group] g ON gm.GroupId = g.Id
		JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
		JOIN [GroupTypeRole] gtr ON gm.GroupRoleId = gtr.Id 
	WHERE
		(
			gt.Name = ''Home Group''
			OR gt.Name = ''Fuse Group''
		)
		AND gtr.Name = ''Leader''
		AND gm.GroupMemberStatus = @GroupMemberStatusActive
		AND g.IsActive = 1
	GROUP BY
		g.CampusId;
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Number of Home/Fuse Group Leaders'
	, 'The number of active leaders in active Home or Fuse Groups'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @NextStepsCategoryId, @Order, NEWID() );

/* ======================================================
--	KidSpring 4 Week Percent of Return 
--	The percent of attendances that a KidSpring newcomer completes,
--	including their initial visit and three weeks thereafter
   ======================================================*/

SET @SQL = N'
	DECLARE @today AS DATE = GETDATE();
	DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));
	DECLARE @firstTimeSundayDate AS DATE = DATEADD(WEEK, -3, @recentSundayDate);

	WITH cte_GroupIds AS (
		SELECT
			g.Id
		FROM
			[Group] g
			JOIN [Group] p ON g.ParentGroupId = p.Id
		WHERE
			p.Name IN (
				''Nursery Attendee'', 
				''Preschool Attendee'', 
				''Elementary Attendee'',
				''Special Needs Attendee'')
	),
	cte_FirstTimePersonIds AS (
		SELECT
			pa.PersonId AS Id
			, MAX(a.CampusId) AS CampusId
		FROM
			[Attendance] a
			JOIN [PersonAlias] pa ON pa.Id = a.PersonAliasId
			JOIN [cte_GroupIds] g ON g.Id = a.GroupId
		WHERE
			a.DidAttend = 1
		GROUP BY
			pa.PersonId
		HAVING
			CONVERT(DATE, MIN([StartDateTime])) = @firstTimeSundayDate
	),
	cte_4WeekAttendance AS (
		SELECT
			ftp.Id
			, COUNT(DISTINCT CONVERT(DATE, a.StartDateTime)) AS Attendances
			, ftp.CampusId
		FROM
			[cte_FirstTimePersonIds] ftp
			JOIN [PersonAlias] pa ON pa.PersonId = ftp.Id
			JOIN [Attendance] a ON a.PersonAliasId = pa.Id
		WHERE
			a.StartDateTime BETWEEN @firstTimeSundayDate AND @recentSundayDate
		GROUP BY
			ftp.Id
			, ftp.CampusId
	)
	SELECT
		CONVERT(INT, ROUND(AVG(CONVERT(DECIMAL, Attendances)) / 4 * 100, 0)) AS Value
		, CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM
		[cte_4WeekAttendance]
	GROUP BY
		CampusId
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'KidSpring 4 Week Percent of Return'
	, 'The percent of attendances that a KidSpring newcomer completes including their initial visit and three weeks thereafter'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @KSAttendanceCategoryId, @Order, NEWID() );

/* ======================================================
--	Fuse 4 Week Percent of Return 
--	The percent of attendances that a Fuse newcomer completes, 
--	including their initial visit and three weeks thereafter
   ======================================================*/

SET @SQL = N'
	DECLARE @today AS DATE = GETDATE();
	DECLARE @recentWednesday AS DATE = CONVERT(DATE, DATEADD(DAY, 4 - DATEPART(DW, @today), @today));
	DECLARE @firstTimeWednesday AS DATE = DATEADD(WEEK, -3, @recentWednesday);

	WITH cte_GroupIds AS (
		SELECT
			g.Id
		FROM
			[Group] g
			JOIN [Group] p ON g.ParentGroupId = p.Id
		WHERE
			p.Name = ''Fuse Attendee''
	),
	cte_FirstTimePersonIds AS (
		SELECT
			pa.PersonId AS Id
			, MAX(a.CampusId) AS CampusId
		FROM
			[Attendance] a
			JOIN [PersonAlias] pa ON pa.Id = a.PersonAliasId
			JOIN [cte_GroupIds] g ON g.Id = a.GroupId
		WHERE
			a.DidAttend = 1
		GROUP BY
			pa.PersonId
		HAVING
			CONVERT(DATE, MIN([StartDateTime])) = @firstTimeWednesday
	),
	cte_4WeekAttendance AS (
		SELECT
			ftp.Id
			, COUNT(DISTINCT CONVERT(DATE, a.StartDateTime)) AS Attendances
			, ftp.CampusId
		FROM
			[cte_FirstTimePersonIds] ftp
			JOIN [PersonAlias] pa ON pa.PersonId = ftp.Id
			JOIN [Attendance] a ON a.PersonAliasId = pa.Id
		WHERE
			a.StartDateTime BETWEEN @firstTimeWednesday AND @recentWednesday
		GROUP BY
			ftp.Id
			, ftp.CampusId
	)
	SELECT
		CONVERT(INT, ROUND(AVG(CONVERT(DECIMAL, Attendances)) / 4 * 100, 0)) AS Value
		, CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''19:00'' AS ScheduleDate
	FROM
		[cte_4WeekAttendance]
	GROUP BY
		CampusId
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Fuse 4 Week Percent of Return'
	, 'The percent of attendances that a Fuse newcomer completes including their initial visit and three weeks thereafter'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @FuseAttendanceCategoryId, @Order, NEWID() );


/* ======================================================
--	Fuse 1st timers
--	The number of attendances that are Fuse newcomers
   ======================================================*/

SET @SQL = N'
	DECLARE @today AS DATE = GETDATE();
	DECLARE @recentWednesday AS DATE = CONVERT(DATE, DATEADD(DAY, 4 - DATEPART(DW, @today), @today));

	WITH cte_GroupIds AS (
		SELECT
			g.Id
		FROM
			[Group] g
			JOIN [Group] p ON g.ParentGroupId = p.Id
		WHERE
			p.Name = ''Fuse Attendee''
	),
	cte_FirstTimePersonIds AS (
		SELECT
			pa.PersonId AS Id
			, MAX(a.CampusId) AS CampusId
		FROM
			[Attendance] a
			JOIN [PersonAlias] pa ON pa.Id = a.PersonAliasId
			JOIN [cte_GroupIds] g ON g.Id = a.GroupId
		WHERE
			a.DidAttend = 1
		GROUP BY
			pa.PersonId
		HAVING
			CONVERT(DATE, MIN([StartDateTime])) = @recentWednesday
	)
	SELECT
		COUNT(Id) AS Value
		, CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''19:00'' AS ScheduleDate
	FROM
		[cte_FirstTimePersonIds]
	GROUP BY
		CampusId
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'Fuse 1st Time Attendance'
	, 'The number of attendances that are Fuse newcomers'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @FuseAttendanceCategoryId, @Order, NEWID() );

/* ======================================================
--	KidSpring 1st timers
--	The number of attendances that are KidSpring newcomers
   ======================================================*/

SET @SQL = N'
	DECLARE @today AS DATE = GETDATE();
	DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));

	WITH cte_GroupIds AS (
		SELECT
			g.Id
		FROM
			[Group] g
			JOIN [Group] p ON g.ParentGroupId = p.Id
		WHERE
			p.Name IN (
				''Nursery Attendee'', 
				''Preschool Attendee'', 
				''Elementary Attendee'',
				''Special Needs Attendee'')
	),
	cte_FirstTimePersonIds AS (
		SELECT
			pa.PersonId AS Id
			, MAX(a.CampusId) AS CampusId
		FROM
			[Attendance] a
			JOIN [PersonAlias] pa ON pa.Id = a.PersonAliasId
			JOIN [cte_GroupIds] g ON g.Id = a.GroupId
		WHERE
			a.DidAttend = 1
		GROUP BY
			pa.PersonId
		HAVING
			CONVERT(DATE, MIN([StartDateTime])) = @recentSundayDate
	)
	SELECT
		COUNT(Id) AS Value
		, CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM
		[cte_FirstTimePersonIds]
	GROUP BY
		CampusId
';

INSERT [Metric] (
	IsSystem
	, Title
	, [Description]
	, IsCumulative
	, SourceValueTypeId
	, SourceSql
	, XAxisLabel
	, YAxisLabel
	, ScheduleId
	, EntityTypeId
	, [Guid]
	, ForeignId)
VALUES (
	0
	, 'KidSpring 1st Time Attendance'
	, 'The number of attendances that are KidSpring newcomers'
	, @False
	, @MetricSourceSQLId
	, @SQL
	, ''
	, ''
	, @MetricScheduleId
	, @CampusEntityTypeId
	, NEWID()
	, NULL );

SELECT @InsertedId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @InsertedId, @FuseAttendanceCategoryId, @Order, NEWID() );
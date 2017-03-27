/* ====================================================== 
-- NewSpring Script #20: 
-- Convert one off metrics to work with partitions 
  
--  Assumptions:
--  Existing metrics structure exists according to script 7
--  Existing new group structure
   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

-- Set common variables 
DECLARE @IsSystem bit = 0
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @CreatedDateTime AS DATETIME = GETDATE();
DECLARE @foreignKey AS NVARCHAR(15) = 'Metrics 2.0';
DECLARE @fuseScheduleId AS NVARCHAR(10) = (SELECT Id FROM Schedule WHERE Name = 'Fuse');
DECLARE @sundayCalculation AS NVARCHAR(200) = 'CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today)) AS MetricValueDateTime';
DECLARE @wednesdaySubCalc AS NVARCHAR(200) = 'CONVERT(DATE, DATEADD(DAY, (DATEDIFF (DAY, ''20110105'', @today) / 7) * 7, ''20110105''))';
DECLARE @wednesdayCalculation AS NVARCHAR(200) = @wednesdaySubCalc + ' AS MetricValueDateTime';
DECLARE @etidSchedule AS INT = (SELECT Id FROM EntityType WHERE NAME = 'Rock.Model.Schedule');
DECLARE @etidGroup AS INT = (SELECT Id FROM EntityType WHERE NAME = 'Rock.Model.Group');

/* ====================================================== */
-- create the group conversion table
/* ====================================================== */
IF object_id('tempdb..#groupConversion') IS NOT NULL
BEGIN
	drop table #groupConversion
END

select ogt.Name OldGroupType, og.id OldGroupId, og.name OldGroup, ogl.Id OldLocationId, ogl.name OldLocation, gt.Name GroupTypeName, ng.Id GroupId, ng.Name GroupName, ng.CampusId
into #groupConversion
from [group] og
	inner join grouptype ogt
	on og.grouptypeid = ogt.id
	and ogt.name not like 'NEW %'    
	inner join grouplocation gl
	on og.id = gl.groupid
	and og.isactive = 1    
	inner join location ogl
	on gl.LocationId = ogl.id
	and ogl.name is not null    
	inner join [group] ng
	on ng.name = ogl.name
	inner join grouptype gt
	on ng.grouptypeid = gt.id
	and gt.name like 'NEW %'
	inner join grouplocation ngl
	on ng.id = ngl.groupid
	and ogl.id = ngl.LocationId;

/*************************************
 * Next Steps -> Financial Coaching -> Coaching Assignments
*************************************/
DECLARE @metricId AS INT = (
	SELECT 
		m.Id 
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Coaching Assignments'
		AND c.Name = 'Financial Coaching'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT 
	COUNT(gm.PersonId) AS Value, 
	' + @sundayCalculation + ',
	g.CampusId,
	gm.GroupId,
	NULL AS ScheduleId
FROM
	[Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
	JOIN [GroupMember] gm ON gm.GroupId = g.Id
WHERE
	gm.GroupMemberStatus = 1
	AND g.Name = ''Financial Coaching Volunteer''
	AND gt.Name = ''NEW Next Steps Volunteer''
GROUP BY
	g.CampusId,
	gm.GroupId;
'
WHERE Id = @metricId;

/*************************************
 * Next Steps -> Financial Coaching -> Coaching Attendees
*************************************/
SET @metricId = (
	SELECT 
		m.Id 
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Coaching Attendees'
		AND c.Name = 'Financial Coaching'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @recentSunday AS DATETIME = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));

SELECT 
	COUNT(1) AS Value, 
	@recentSunday AS MetricValueDateTime,
	g.CampusId,
	a.GroupId,
	a.ScheduleId
FROM
	Attendance a
	JOIN [Group] g ON a.GroupId = g.Id
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
WHERE
	a.SundayDate = @recentSunday
	AND g.Name = ''Financial Coaching Attendee''
	AND gt.Name = ''NEW Next Steps Attendee''
GROUP BY
	g.CampusId,
	a.ScheduleId,
	a.GroupId;
'
WHERE Id = @metricId;

/*************************************
 * Next Steps -> Community Groups -> Avg Fuse Group Size
*************************************/
SET @metricId = (
	SELECT 
		m.Id 
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Avg Fuse Group Size'
		AND c.Name = 'Community Groups'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @GroupMemberStatusActive int = 1;

SELECT
	AVG(sub.NumMembers) as Value
	, ' + @wednesdayCalculation + '
	, sub.CampusId,
	NULL AS GroupId,
	NULL AS ScheduleId
FROM 
	(
		SELECT 
			COUNT(DISTINCT gm.PersonId) AS NumMembers
			, g.CampusId
		FROM
			[GroupMember] gm
			INNER JOIN [Group] g ON gm.GroupId = g.Id
			INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
		WHERE
			gt.Name = ''Fuse Group''
			AND g.IsActive = 1
			AND g.ParentGroupId IS NOT NULL
			AND gm.GroupMemberStatus = @GroupMemberStatusActive
			AND g.CampusId IS NOT NULL
		GROUP BY
			g.Id, g.CampusId
	) sub
GROUP BY
	sub.CampusId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Fuse HS attendance
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Fuse HS Attendance'
		AND c.Name = 'Fuse Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT 
	COUNT(1) AS Value, ' 
	+ @wednesdayCalculation + ', 
	CampusId AS EntityId, 
	NULL AS GroupId,
	' + @fuseScheduleId + '
FROM PersonAlias PA 
INNER JOIN (
	SELECT PersonAliasId, A.CampusId
	FROM [Attendance] A	
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''NEW Fuse Attendee'')
	WHERE DidAttend = 1	
	AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
	AND StartDateTime < CONVERT(DATE, @today)
	AND (
		CASE WHEN ISNUMERIC(LEFT(G.Name, 1)) = 1
		THEN LEFT(G.Name, 1) ELSE NULL END
	) IN ( 9, 1 ) -- 9th, 10th, 11th, 12th grade
) Attendance
	ON PA.Id = Attendance.PersonAliasId	
GROUP BY CampusId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Fuse MS attendance
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Fuse MS Attendance'
		AND c.Name = 'Fuse Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT 
	COUNT(1) AS Value, ' 
	+ @wednesdayCalculation + ', 
	CampusId AS EntityId, 
	NULL AS GroupId,' 
	+  @fuseScheduleId + '
FROM PersonAlias PA 
INNER JOIN (
	SELECT PersonAliasId, A.CampusId
	FROM [Attendance] A	
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''NEW Fuse Attendee'')
	WHERE DidAttend = 1	
	AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
	AND StartDateTime < CONVERT(DATE, @today)
	AND (
		CASE WHEN ISNUMERIC(LEFT(G.Name, 1)) = 1
		THEN LEFT(G.Name, 1) ELSE NULL END
	) IN ( 6,7,8 ) -- 6th, 7th, 8th grade
) Attendance
	ON PA.Id = Attendance.PersonAliasId	
GROUP BY CampusId	
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> Fuse Attendance -> Fuse 4 Week Percent of Return
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Fuse 4 Week Percent of Return'
		AND c.Name = 'Fuse Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @wednesdayDW AS INT = 4;
DECLARE @recentWednesdayDate AS DATE = ' + @wednesdaySubCalc + ';

IF @recentWednesdayDate > @today
BEGIN
	SET @recentWednesdayDate = DATEADD(DAY, -7, @recentWednesdayDate);
END

DECLARE @firstTimeWednesdayDate AS DATE = DATEADD(WEEK, -3, @recentWednesdayDate);
DECLARE @firstReturnWednesdayDate AS DATE = DATEADD(WEEK, -2, @recentWednesdayDate);

WITH Attendances AS (
	SELECT
		a.Id,
		a.PersonAliasId,
		a.GroupId,
		CONVERT(DATE, a.StartDateTime) AS WednesdayDate,
		a.CampusId
	FROM
		[Attendance] a
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN [Group] p ON p.Id = g.ParentGroupId
	WHERE
		a.DidAttend = 1
		AND p.Name = ''NEW Fuse Attendee''
), FirstTimePersonIds AS (
	SELECT
		pa.PersonId
	FROM
		[Attendances] a
		JOIN [PersonAlias] pa ON pa.Id = a.PersonAliasId
	GROUP BY
		pa.PersonId
	HAVING
		MIN(a.WednesdayDate) = @firstTimeWednesdayDate
), FirstTimePersonIdsWithCampus AS (
	SELECT
		ftp.PersonId,
		MAX(a.CampusId) AS CampusId
	FROM
		FirstTimePersonIds ftp
		JOIN PersonAlias pa ON pa.PersonId = ftp.PersonId
		JOIN Attendances a ON a.PersonAliasId = pa.Id
	WHERE
		a.WednesdayDate = @firstTimeWednesdayDate
	GROUP BY
		ftp.PersonId
), Returnees AS (
	SELECT
		DISTINCT ftp.PersonId
	FROM
		FirstTimePersonIdsWithCampus ftp
		JOIN PersonAlias pa ON pa.PersonId = ftp.PersonId
		JOIN Attendances a ON a.PersonAliasId = pa.Id
	WHERE
		a.WednesdayDate BETWEEN @firstReturnWednesdayDate AND @recentWednesdayDate
)
SELECT
	CONVERT(INT, ROUND(CONVERT(DECIMAL, COUNT(r.PersonId)) / CONVERT(DECIMAL, COUNT(ftp.PersonId)) * 100, 0)) AS Value, ' 
	+ @wednesdayCalculation + ',
	ftp.CampusId, 
	NULL AS GroupId,' 
	+ @fuseScheduleId + '
FROM
	FirstTimePersonIdsWithCampus ftp
	LEFT JOIN Returnees r ON r.PersonId = ftp.PersonId
GROUP BY
	ftp.CampusId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> Fuse Attendance -> Fuse First Timers
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Fuse First Timers'
		AND c.Name = 'Fuse Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @recentWednesday AS DATE = ' + @wednesdaySubCalc + ';

;WITH cte_GroupIds AS (
	SELECT
		g.Id
	FROM
		[Group] g
		JOIN [Group] p ON g.ParentGroupId = p.Id
	WHERE
		p.Name = ''NEW Fuse Attendee''
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
	COUNT(Id) AS Value, ' + @wednesdayCalculation + '
	, CampusId,
	NULL AS GroupId,
	' + @fuseScheduleId + '
FROM
	[cte_FirstTimePersonIds]
GROUP BY
	CampusId;
'
WHERE Id = @metricId;


/* ====================================================== */
-- Attendance -> KidSpring Attendance -> 4 Week Percent of Return
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = '4 Week Percent of Return'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));
DECLARE @firstTimeSundayDate AS DATE = DATEADD(WEEK, -3, @recentSundayDate);
DECLARE @firstReturnSundayDate AS DATE = DATEADD(WEEK, -2, @recentSundayDate);

WITH Attendances AS (
	SELECT
		a.Id,
		a.PersonAliasId,
		a.GroupId,
		a.SundayDate,
		a.CampusId
	FROM
		[Attendance] a
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN [Group] p ON p.Id = g.ParentGroupId
	WHERE
		a.DidAttend = 1
		AND p.Name IN (
			''NEW Nursery Attendee'', 
			''NEW Preschool Attendee'', 
			''NEW Elementary Attendee'',
			''NEW Special Needs Attendee''
		)
), FirstTimePersonIds AS (
	SELECT
		pa.PersonId
	FROM
		[Attendances] a
		JOIN [PersonAlias] pa ON pa.Id = a.PersonAliasId
	GROUP BY
		pa.PersonId
	HAVING
		MIN(a.SundayDate) = @firstTimeSundayDate
), FirstTimePersonIdsWithCampus AS (
	SELECT
		ftp.PersonId,
		MAX(a.CampusId) AS CampusId
	FROM
		FirstTimePersonIds ftp
		JOIN PersonAlias pa ON pa.PersonId = ftp.PersonId
		JOIN Attendances a ON a.PersonAliasId = pa.Id
	WHERE
		a.SundayDate = @firstTimeSundayDate
	GROUP BY
		ftp.PersonId
), Returnees AS (
	SELECT
		DISTINCT ftp.PersonId
	FROM
		FirstTimePersonIdsWithCampus ftp
		JOIN PersonAlias pa ON pa.PersonId = ftp.PersonId
		JOIN Attendances a ON a.PersonAliasId = pa.Id
	WHERE
		a.SundayDate BETWEEN @firstReturnSundayDate AND @recentSundayDate
)
SELECT
	CONVERT(INT, ROUND(CONVERT(DECIMAL, COUNT(r.PersonId)) / CONVERT(DECIMAL, COUNT(ftp.PersonId)) * 100, 0)) AS Value, ' 
	+ @sundayCalculation + ',
	ftp.CampusId AS CampusId,
	NULL AS GroupId,
	NULL AS ScheduleId
FROM
	FirstTimePersonIdsWithCampus ftp
	LEFT JOIN Returnees r ON r.PersonId = ftp.PersonId
GROUP BY
	ftp.CampusId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> KidSpring Attendance -> First Time Guests
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'First Time Guests'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
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
			''NEW Nursery Attendee'', 
			''NEW Preschool Attendee'', 
			''NEW Elementary Attendee'',
			''NEW Special Needs Attendee'')
),
cte_FirstTimePersonIds AS (
	SELECT
		pa.PersonId AS Id
		, MAX(a.CampusId) AS CampusId
		, MAX(a.GroupId) AS GroupId
		, MAX(a.ScheduleId) AS ScheduleId
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
	, ' + @sundayCalculation + '
	, CampusId
	, GroupId
	, ScheduleId
FROM
	[cte_FirstTimePersonIds]
GROUP BY
	CampusId,
	GroupId,
	ScheduleId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> KidSpring Attendance -> Nursery Attendance
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Nursery Attendance'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT COUNT(1) AS Value, 
    ' + @sundayCalculation + ',
	CampusId,
	GroupId,
	ScheduleId
FROM PersonAlias PA 
INNER JOIN (
	SELECT PersonAliasId, A.CampusId, A.ScheduleId, A.GroupId
	FROM [Attendance] A
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''NEW Nursery Attendee'')
	WHERE DidAttend = 1
		AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
		AND StartDateTime < CONVERT(DATE, @today)
) Attendance
	ON PA.Id = Attendance.PersonAliasId	
INNER JOIN (
	-- iCal DTStart: constant 22 characters, only interested in Service Time
	SELECT Id, STUFF(SUBSTRING(iCalendarContent, PATINDEX(''%DTSTART%'', iCalendarContent) +17, 4), 3, 0, '':'') AS Value
	FROM Schedule
	WHERE EffectiveStartDate < @today
) Schedule
ON Schedule.Id = Attendance.ScheduleId
GROUP BY Attendance.CampusId, ScheduleId, GroupId
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> KidSpring Attendance -> Preschool Attendance
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Preschool Attendance'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT COUNT(1) AS Value, 
    ' + @sundayCalculation + ',
	CampusId AS EntityId,
	GroupId,
	ScheduleId
FROM PersonAlias PA 
INNER JOIN (
	SELECT PersonAliasId, A.CampusId, A.ScheduleId, A.GroupId
	FROM [Attendance] A
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''NEW Preschool Attendee'')
	WHERE DidAttend = 1
		AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
		AND StartDateTime < CONVERT(DATE, @today)
) Attendance
	ON PA.Id = Attendance.PersonAliasId	
INNER JOIN (
	-- iCal DTStart: constant 22 characters, only interested in Service Time
	SELECT Id, STUFF(SUBSTRING(iCalendarContent, PATINDEX(''%DTSTART%'', iCalendarContent) +17, 4), 3, 0, '':'') AS Value
	FROM Schedule
	WHERE EffectiveStartDate < @today
) Schedule
ON Schedule.Id = Attendance.ScheduleId
GROUP BY Attendance.CampusId, ScheduleId, GroupId
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> KidSpring Attendance -> Elementary Attendance
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Elementary Attendance'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT COUNT(1) AS Value, 
	' + @sundayCalculation + ',
	CampusId,
	GroupId,
	ScheduleId
FROM PersonAlias PA 
INNER JOIN (
	SELECT PersonAliasId, A.CampusId, A.ScheduleId, A.GroupId
	FROM [Attendance] A
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''NEW Elementary Attendee'')
	WHERE DidAttend = 1
		AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
		AND StartDateTime < CONVERT(DATE, @today)
) Attendance
	ON PA.Id = Attendance.PersonAliasId	
INNER JOIN (
	-- iCal DTStart: constant 22 characters, only interested in Service Time
	SELECT Id, STUFF(SUBSTRING(iCalendarContent, PATINDEX(''%DTSTART%'', iCalendarContent) +17, 4), 3, 0, '':'') AS Value
	FROM Schedule
	WHERE EffectiveStartDate < @today
) Schedule
ON Schedule.Id = Attendance.ScheduleId
GROUP BY Attendance.CampusId, ScheduleId, GroupId
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> KidSpring Attendance -> Special Needs Attendance
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Special Needs Attendance'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT COUNT(1) AS Value, 
	' + @sundayCalculation + ',
	CampusId AS EntityId,
	GroupId,
	ScheduleId
FROM PersonAlias PA 
INNER JOIN (
	SELECT PersonAliasId, A.CampusId, A.ScheduleId, A.GroupId
	FROM [Attendance] A
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''NEW Special Needs Attendee'')
	WHERE DidAttend = 1
		AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
		AND StartDateTime < CONVERT(DATE, @today)
) Attendance
	ON PA.Id = Attendance.PersonAliasId	
INNER JOIN (
	-- iCal DTStart: constant 22 characters, only interested in Service Time
	SELECT Id, STUFF(SUBSTRING(iCalendarContent, PATINDEX(''%DTSTART%'', iCalendarContent) +17, 4), 3, 0, '':'') AS Value
	FROM Schedule
	WHERE EffectiveStartDate < @today
) Schedule
ON Schedule.Id = Attendance.ScheduleId
GROUP BY Attendance.CampusId, ScheduleId, GroupId
'
WHERE Id = @metricId;

/* ====================================================== */
-- Attendance -> KidSpring Attendance -> First Time Volunteers
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'First Time Volunteers'
		AND c.Name = 'KidSpring Attendance'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

WITH KSFirstServe AS (
	SELECT PersonAliasId, MIN(StartDateTime) AS FirstServe
	FROM [Attendance] A
	INNER JOIN [Group] G
		ON A.GroupId = G.Id
		-- restricted to KS Volunteer grouptypes
		AND G.GroupTypeId IN ( 49, 50, 51, 52, 53, 54 ) 
	WHERE DidAttend = 1	
	GROUP BY PersonAliasId
)
SELECT COUNT(1) AS Value
	, ' + @sundayCalculation + '
	, Attendance.CampusId
	, Attendance.GroupId
	, Attendance.ScheduleId
FROM PersonAlias PA 
INNER JOIN (
	SELECT A.PersonAliasId, A.CampusId, A.ScheduleId, StartDateTime AS ScheduleDate, g.Id AS GroupId
	FROM [Attendance] A
	INNER JOIN [Group] G
	    ON A.GroupId = G.Id
		-- restricted to KS Volunteer grouptypes
		AND G.GroupTypeId IN ( 49, 50, 51, 52, 53, 54 ) 
	INNER JOIN KSFirstServe K
	    ON A.PersonAliasId = K.PersonAliasId
	    AND A.StartDateTime = K.FirstServe
    	-- limit to first time visits here
    	AND K.FirstServe >= DATEADD(dd, DATEDIFF(dd, 1, @today), 0)
    	AND K.FirstServe < CONVERT(DATE, @today)
    WHERE A.DidAttend = 1
) Attendance
ON PA.Id = Attendance.PersonAliasId
INNER JOIN (
	-- iCal DTStart: constant 22 characters, only interested in Service Time
	SELECT Id, STUFF(SUBSTRING(iCalendarContent, PATINDEX(''%DTSTART%'', iCalendarContent) +17, 4), 3, 0, '':'') AS Value
	FROM Schedule
	WHERE EffectiveStartDate < @today
) Schedule
ON Schedule.Id = Attendance.ScheduleId
GROUP BY CampusId, ScheduleId, GroupId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Guest Services -> GS Unique Volunteers
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'GS Unique Volunteers'
		AND c.Name = 'Guest Services'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT 
	COUNT(gm.PersonId) AS Value, 
	' + @sundayCalculation + ',
	g.CampusId AS EntityId,
	NULL AS GroupId,
	NULL AS ScheduleId
FROM 
	GroupMember gm
	JOIN [Group] G ON gm.GroupId = G.Id
	JOIN GroupType gt ON gt.Id = g.GroupTypeId
WHERE  
	gt.Name = ''NEW Guest Services Volunteer''
	AND gm.GroupMemberStatus = 1
GROUP BY 
	g.CampusId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Next Steps -> Baptism -> Baptism Attendees
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Baptism Attendees'
		AND c.Name = 'Baptism'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));

SELECT
	COUNT(a.PersonAliasId),
	@recentSundayDate AS MetricValueDateTime,
	g.CampusId,
	NULL AS GroupId,
	a.ScheduleId
FROM
	[Group] g
	JOIN GroupType gt ON g.GroupTypeId = gt.Id
	JOIN Attendance a ON a.GroupId = g.Id
WHERE
	gt.Name = ''NEW Next Steps Attendee''
	AND g.Name = ''Baptism Attendee''
	AND a.SundayDate = @recentSundayDate
GROUP BY
	g.CampusId,
	a.ScheduleId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Next Steps -> Baptism -> Baptism Assignments
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Baptism Assignments'
		AND c.Name = 'Baptism'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT 
	COUNT(gm.PersonId) AS Value, 
	' + @sundayCalculation + ',
	g.CampusId,
	gm.GroupId,
	NULL AS ScheduleId
FROM
	[Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
	JOIN [GroupMember] gm ON gm.GroupId = g.Id
WHERE
	gm.GroupMemberStatus = 1
	AND g.Name = ''Baptism Attendee''
	AND gt.Name = ''NEW Next Steps Attendee''
GROUP BY
	g.CampusId,
	gm.GroupId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Next Steps -> Ownership Class -> Ownership Attendees
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Ownership Attendees'
		AND c.Name = 'Ownership Class'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));

SELECT
	COUNT(a.PersonAliasId),
	@recentSundayDate AS MetricValueDateTime,
	g.CampusId,
	NULL AS GroupId,
	a.ScheduleId
FROM
	[Group] g
	JOIN GroupType gt ON g.GroupTypeId = gt.Id
	JOIN Attendance a ON a.GroupId = g.Id
WHERE
	gt.Name = ''NEW Next Steps Attendee''
	AND g.Name = ''Ownership Class Attendee''
	AND a.SundayDate = @recentSundayDate
GROUP BY
	g.CampusId,
	a.ScheduleId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Next Steps -> Ownership Class -> Ownership Assignments
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Ownership Assignments'
		AND c.Name = 'Ownership Class'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();

SELECT 
	COUNT(gm.PersonId) AS Value, 
	' + @sundayCalculation + ',
	g.CampusId,
	gm.GroupId,
	NULL AS ScheduleId
FROM
	[Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
	JOIN [GroupMember] gm ON gm.GroupId = g.Id
WHERE
	gm.GroupMemberStatus = 1
	AND g.Name = ''Ownership Class Volunteer''
	AND gt.Name = ''NEW Next Steps Attendee''
GROUP BY
	g.CampusId,
	gm.GroupId;
'
WHERE Id = @metricId;

/* ====================================================== */
-- Volunteers -> Unique Volunteers
/* ====================================================== */
SET @metricId = (
	SELECT 
		m.Id
	FROM 
		Metric m
		JOIN MetricCategory mc ON mc.MetricId = m.Id
		JOIN Category c ON c.Id = mc.CategoryId
	WHERE
		m.Title = 'Unique Volunteers'
		AND c.Name = 'Volunteers'
);

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Group',
		@etidGroup,
		@False,
		1,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

DELETE FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Schedule';
BEGIN
	INSERT INTO MetricPartition(
		MetricId,
		Label,
		EntityTypeId,
		IsRequired,
		[Order],
		EntityTypeQualifierColumn,
		EntityTypeQualifierValue,
		[Guid],
		ForeignKey
	) VALUES (
		@metricId,
		'Schedule',
		@etidSchedule,
		@False,
		2,
		'',
		'',
		NEWID(),
		@foreignKey
	);
END

UPDATE Metric SET ForeignKey='Metrics 2.0 - 1Off', SourceSql = '
DECLARE @today AS DATE = GETDATE();
DECLARE @recentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @today), @today));

WITH IncludedGroups(Id, ParentGroupId, Name) AS (
	SELECT 
		g.Id, g.ParentGroupId, g.Name
	FROM 
		[Group] g
		JOIN GroupType gt ON g.GroupTypeId = gt.Id
	WHERE
		gt.Name IN (
			''NEW Creativity & Tech Attendee'',
			''NEW Creativity & Tech Volunteer'',
			''NEW Elementary Volunteer'',
			''NEW Fuse Volunteer'',
			''NEW Guest Services Attendee'',
			''NEW Guest Services Volunteer'',
			''NEW KS Production Volunteer'',
			''NEW KS Support Volunteer'',
			''NEW Next Steps Attendee'',
			''NEW Next Steps volunteer'',
			''NEW Nursery Volunteer'',
			''NEW Preschool Volunteer'',
			''NEW Special needs volunteer''
		)
)
SELECT
	COUNT(DISTINCT pa.PersonId) AS Value,
	@recentSundayDate AS ScheduleDate,
	a.CampusId AS EntityId,
	NULL AS GroupId,
	NULL AS ScheduleId
FROM 
	IncludedGroups g
	INNER JOIN Attendance a ON a.GroupId = g.Id
	INNER JOIN PersonAlias pa ON pa.Id = a.PersonAliasId
WHERE 
	CONVERT(DATE, a.StartDateTime) = @recentSundayDate
	AND a.DidAttend = 1
GROUP BY
	a.CampusId;
'
WHERE Id = @metricId;
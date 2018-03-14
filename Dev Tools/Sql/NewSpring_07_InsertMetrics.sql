/* ====================================================== 
-- NewSpring Script #7: 
-- Creates the metric structure 
  
--  Assumptions:
--  Existing ChurchMetrics data (CSV) will map by name to 
--  the following metrics:
	
	Attendance
	Guest Services
	Next Steps
	People
	Volunteers

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

SET NOCOUNT ON;

-- Set common variables 
DECLARE @msg nvarchar(500) = ''
DECLARE @IsSystem bit = 0
DECLARE @Delimiter nvarchar(5) = ' - '
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @Order int = 0
DECLARE @CampusEntityTypeId int
DECLARE @ScheduleEntityTypeId int
DECLARE @MetricCategoryEntityTypeId int
DECLARE @MetricSourceSQLId int
DECLARE @MetricSourceManualId int
DECLARE @CreatedDateTime AS DATETIME = GETDATE();

SELECT @CampusEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Campus'
SELECT @ScheduleEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Schedule'
SELECT @MetricCategoryEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.MetricCategory'

-- get metric source types (core)
SELECT @MetricSourceManualId = [Id] FROM DefinedValue WHERE [Guid] = '1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E'
SELECT @MetricSourceSQLId = [Id] FROM DefinedValue WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'

/* ====================================================== */
-- metric category and schedules
/* ====================================================== */
DECLARE @MetricScheduleCategoryId int, @SundayScheduleId int, @FuseScheduleId int, @MetriciCalSchedule nvarchar(max)

SELECT @MetricScheduleCategoryId = [Id] FROM [Category]
WHERE EntityTypeId = @ScheduleEntityTypeId
AND Name = 'Metrics'

-- create schedule category
IF @MetricScheduleCategoryId IS NULL
BEGIN
	INSERT [Category] (IsSystem, ParentCategoryId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Guid], [Order])
	VALUES ( @IsSystem, NULL, @ScheduleEntityTypeId, '', '', 'Metrics', NEWID(), @Order )

	SET @MetricScheduleCategoryId = SCOPE_IDENTITY()
END

-- create the metric schedule
SELECT @SundayScheduleId = [Id] FROM Schedule
WHERE CategoryId = @MetricScheduleCategoryId
AND Name = 'Sunday Metric Schedule'

IF @SundayScheduleId IS NULL
BEGIN

	SELECT @MetriciCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20150928T020001
DTSTAMP:20150928T201239Z
DTSTART:20150928T020000
RRULE:FREQ=WEEKLY;BYDAY=MO
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, [Description], iCalendarContent, EffectiveStartDate, EffectiveEndDate, CategoryId, [Guid])
	SELECT 'Sunday Metric Schedule', 'The job schedule to run Sunday metrics', @MetriciCalSchedule, GETDATE(), GETDATE(), @MetricScheduleCategoryId, NEWID()

	SELECT @SundayScheduleId = SCOPE_IDENTITY()
END

SELECT @FuseScheduleId = [Id] FROM Schedule
WHERE CategoryId = @MetricScheduleCategoryId
AND Name = 'Fuse Metric Schedule'

IF @FuseScheduleId IS NULL
BEGIN

	SELECT @MetriciCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20150928T020001
DTSTAMP:20150928T201239Z
DTSTART:20150928T020000
RRULE:FREQ=WEEKLY;BYDAY=TH
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, [Description], iCalendarContent, EffectiveStartDate, EffectiveEndDate, CategoryId, [Guid])
	SELECT 'Fuse Metric Schedule', 'The job schedule to run Fuse metrics', @MetriciCalSchedule, GETDATE(), GETDATE(), @MetricScheduleCategoryId, NEWID()

	SELECT @FuseScheduleId = SCOPE_IDENTITY()
END

/* ====================================================== */
-- create the initial metric structure
/* ====================================================== */
IF object_id('tempdb..#metricTypes') IS NOT NULL
BEGIN
	drop table #metricTypes
END
create table #metricTypes (
	ID int IDENTITY(1,1),
	parentType nvarchar(255),
	childType nvarchar(255),
	parentTypeId int,
	childTypeId int
)

INSERT #metricTypes (parentType, childType)
VALUES
('Attendance', 'Adult Attendance'),
('Attendance', 'Fuse Attendance'),
('Attendance', 'KidSpring Attendance'),
('Attendance', 'Volunteer Attendance'),
('Finance', 'General Giving'),
('Finance', 'Step Up'),
('Finance', 'Financial Coaching'),
('Guest Services', 'VIP Room'),
('Next Steps', 'Baptism'),
('Next Steps', 'Ownership Class Attendance'),
('Next Steps', 'Care Room'),
('Next Steps', 'Salvations'),
('Next Steps', 'Fuse Salvations'),
('People', 'Owners'),
('People', 'Families'),
('People', 'Owners & Attendees'),
('Volunteers', 'Creativity & Tech'),
('Volunteers', 'Fuse'),
('Volunteers', 'Guest Services'),
('Volunteers', 'KidSpring'),
('Volunteers', 'Next Steps')

/* ====================================================== */
-- groups needing metrics
/* ====================================================== */
IF object_id('tempdb..#metricGroups') IS NOT NULL
BEGIN
	drop table #metricGroups
END
create table #metricGroups (
	ID int IDENTITY(1,1),
	groupTypeName nvarchar(255),
	groupName nvarchar(255)
)

INSERT #metricGroups
VALUES 
-- certain attendee groups
('Creativity & Tech Attendee', 'Choir'), 
('Creativity & Tech Attendee', 'Special Event Attendee'), 
('Guest Services Attendee', 'VIP Room Attendee'), 
('Guest Services Attendee', 'Special Event Attendee'), 
('Guest Services Attendee', 'Auditorium Reset Team'), 
('Guest Services Attendee', 'Awake Team'), 
('Guest Services Attendee', 'Facility Cleaning Crew'), 
('Guest Services Attendee', 'Greeting Team'), 
('Guest Services Attendee', 'Load In'), 
('Guest Services Attendee', 'Load Out'), 
('Guest Services Attendee', 'Office Team'), 
('Guest Services Attendee', 'Parking Team'), 
('Guest Services Attendee', 'VHQ Team'), 

-- all volunteer groups
('Creativity & Tech Volunteer',  'Band'),
('Creativity & Tech Volunteer',  'Band Green Room'),
('Creativity & Tech Volunteer',  'Editorial Team'),
('Creativity & Tech Volunteer',  'IT Team'),
('Creativity & Tech Volunteer',  'Load In'),
('Creativity & Tech Volunteer',  'Load Out'),
('Creativity & Tech Volunteer',  'New Serve Team'),
('Creativity & Tech Volunteer',  'Office Team'),
('Creativity & Tech Volunteer',  'Production Team'),
('Creativity & Tech Volunteer',  'Social Media/PR Team'),
('Creativity & Tech Volunteer',  'Special Event Volunteer'),
('Elementary Volunteer',  'Base Camp Volunteer'),
('Elementary Volunteer',  'Elementary Early Bird Volunteer'),
('Elementary Volunteer',  'Elementary Service Leader'),
('Elementary Volunteer',  'Elementary Area Leader'),
('Elementary Volunteer',  'ImagiNation Volunteer'),
('Elementary Volunteer',  'Jump Street Volunteer'),
('Elementary Volunteer',  'Shockwave Volunteer'),
('Fuse Volunteer',  'Atrium'),
('Fuse Volunteer',  'Campus Safety'),
('Fuse Volunteer',  'Care'),
('Fuse Volunteer',  'Check-In'),
('Fuse Volunteer',  'Fuse Group Leader'),
('Fuse Volunteer',  'Fuse Guest'),
('Fuse Volunteer',  'Game Room'),
('Fuse Volunteer',  'Greeter'),
('Fuse Volunteer',  'VIP Team'),
('Fuse Volunteer',  'Leadership Team'),
('Fuse Volunteer',  'Load In'),
('Fuse Volunteer',  'Load Out'),
('Fuse Volunteer',  'Lounge'),
('Fuse Volunteer',  'New Serve'),
('Fuse Volunteer',  'Next Steps'),
('Fuse Volunteer',  'Office Team'),
('Fuse Volunteer',  'Parking'),
('Fuse Volunteer',  'Pick-Up'),
('Fuse Volunteer',  'Production'),
('Fuse Volunteer',  'Snack Bar'),
('Fuse Volunteer',  'Special Event Volunteer'),
('Fuse Volunteer',  'Sports'),
('Fuse Volunteer',  'Spring Zone'),
('Fuse Volunteer',  'Student Leader'),
('Fuse Volunteer',  'Sunday Fuse Team'),
('Fuse Volunteer',  'Usher'),
('Fuse Volunteer',  'VHQ'),
('Fuse Volunteer',  'Worship'),
('Guest Services Volunteer',  'Area Leader'),
('Guest Services Volunteer',  'Campus Safety'),
('Guest Services Volunteer',  'Facilities Volunteer'),
('Guest Services Volunteer',  'Finance Team'),
('Guest Services Volunteer',  'Hispanic Team'),
('Guest Services Volunteer',  'Network Fuse Team'),
('Guest Services Volunteer',  'Network Office Team'),
('Guest Services Volunteer',  'Network Sunday Team'),
('Guest Services Volunteer',  'VIP Room Volunteer'),
('Guest Services Volunteer',  'Guest Services Team'),
('Guest Services Volunteer',  'New Serve Team'),
('Guest Services Volunteer',  'Receptionist'),
('Guest Services Volunteer',  'Service Leader'),
('Guest Services Volunteer',  'Sign Language Team'),
('Guest Services Volunteer',  'Special Event Volunteer'),
('Guest Services Volunteer',  'Usher Team'),
('Production Volunteer',  'Elementary Production'),
('Production Volunteer',  'Elementary Production Service Leader'),
('Production Volunteer',  'Preschool Production'),
('Production Volunteer',  'Preschool Production Service Leader'),
('Production Volunteer',  'Production Area Leader'),
('Production Volunteer',  'Production Service Leader'),
('Production Volunteer',  'KidSpring Production'),
('Support Volunteer',  'Advocate'),
('Support Volunteer',  'Check-In Volunteer'),
('Support Volunteer',  'First Time Team Volunteer'),
('Support Volunteer',  'KidSpring Greeter'),
('Support Volunteer',  'Guest Services Service Leader'),
('Support Volunteer',  'Guest Services Area Leader'),
('Support Volunteer',  'KidSpring Office Team'),
('Support Volunteer',  'Load In'),
('Support Volunteer',  'Load Out'),
('Support Volunteer',  'New Serve Area Leader'),
('Support Volunteer',  'New Serve Team'),
('Support Volunteer',  'Sunday Support Volunteer'),
('Next Steps Volunteer',  'Baptism Volunteer'),
('Next Steps Volunteer',  'Budget Class Volunteer'),
('Next Steps Volunteer',  'Care Office Team'),
('Next Steps Volunteer',  'Care Visitation Team'),
('Next Steps Volunteer',  'Church Online Volunteer'),
('Next Steps Volunteer',  'Events Office Team'),
('Next Steps Volunteer',  'Financial Coaching Volunteer'),
('Next Steps Volunteer',  'Financial Coaching Office Team'),
('Next Steps Volunteer',  'Group Leader'),
('Next Steps Volunteer',  'Groups Office Team'),
('Next Steps Volunteer',  'Load In'),
('Next Steps Volunteer',  'Load Out'),
('Next Steps Volunteer',  'New Serve Team'),
('Next Steps Volunteer',  'Next Steps Area'),
('Next Steps Volunteer',  'Ownership Class Volunteer'),
('Next Steps Volunteer',  'Prayer Team'),
('Next Steps Volunteer',  'Resource Center'),
('Next Steps Volunteer',  'Special Event Volunteer'),
('Next Steps Volunteer',  'Sunday Care Team'),
('Next Steps Volunteer',  'Writing Team'),
('Nursery Volunteer',  'Nursery Early Bird Volunteer'),
('Nursery Volunteer',  'Wonder Way Service Leader'),
('Nursery Volunteer',  'Wonder Way Area Leader'),
('Nursery Volunteer',  'Wonder Way 1 Volunteer'),
('Nursery Volunteer',  'Wonder Way 2 Volunteer'),
('Nursery Volunteer',  'Wonder Way 3 Volunteer'),
('Nursery Volunteer',  'Wonder Way 4 Volunteer'),
('Nursery Volunteer',  'Wonder Way 5 Volunteer'),
('Nursery Volunteer',  'Wonder Way 6 Volunteer'),
('Nursery Volunteer',  'Wonder Way 7 Volunteer'),
('Nursery Volunteer',  'Wonder Way 8 Volunteer'),
('Preschool Volunteer',  'Base Camp Jr. Volunteer'),
('Preschool Volunteer',  'Fire Station Volunteer'),
('Preschool Volunteer',  'Lil'' Spring Volunteer'),
('Preschool Volunteer',  'Toys Volunteer'),
('Preschool Volunteer',  'Pop''s Garage Volunteer'),
('Preschool Volunteer',  'Preschool Early Bird Volunteer'),
('Preschool Volunteer',  'Preschool Service Leader'),
('Preschool Volunteer',  'Preschool Area Leader'),
('Preschool Volunteer',  'Spring Fresh Volunteer'),
('Preschool Volunteer',  'Toys Volunteer'),
('Preschool Volunteer',  'Treehouse Volunteer'),
('Special Needs Volunteer',  'Spring Zone Jr. Volunteer'),
('Special Needs Volunteer',  'Spring Zone Service Leader'),
('Special Needs Volunteer',  'Spring Zone Area Leader'),
('Special Needs Volunteer',  'Spring Zone Volunteer'),
('Creativity & Tech Volunteer',  'Design Team'),
('Creativity & Tech Volunteer',  'IT Team'),
('Creativity & Tech Volunteer',  'NewSpring Store Team'),
('Creativity & Tech Volunteer',  'Social Media/PR Team'),
('Creativity & Tech Volunteer',  'Video Production Team'),
('Creativity & Tech Volunteer',  'Web Dev Team'),
('Event Attendee',  'Event Attendee'),
('Event Volunteer',  'Event Volunteer'),
('Fuse Volunteer',  'Fuse Office Team'),
('Fuse Volunteer',  'Special Event Attendee'),
('Fuse Volunteer',  'Special Event Volunteer'),
('Guest Services Volunteer',  'Events Team'),
('Guest Services Volunteer',  'Finance Office Team'),
('Guest Services Volunteer',  'GS Office Team'),
('Guest Services Volunteer',  'Receptionist'),
('Guest Services Volunteer',  'Special Event Attendee'),
('Guest Services Volunteer',  'Special Event Volunteer'),
('Support Volunteer',  'Office Team'),
('Next Steps Volunteer',  'Groups Office Team'),
('Next Steps Volunteer',  'NS Office Team'),
('Next Steps Volunteer',  'Writing Team')


/* ====================================================== */
-- set up metrics for each group
/* ====================================================== */

DECLARE @MetricServiceRolesId int = null, @MetricServiceRosterId int = null, @MetricTotalRolesId int = null,
	@MetricTotalRosterId int = null, @MetricUniqueServingId int = null, @MetricServiceRolesSQL nvarchar(max), 
	@MetricServiceRosterSQL nvarchar(max), @MetricTotalRolesSQL nvarchar(max), @MetricTotalRosterSQL nvarchar(max),
	@MetricUniqueServingSQL nvarchar(max), @ServiceRolesTitle varchar(255), @ServiceRosterTitle varchar(255), 
	@TotalRolesTitle varchar(255), @TotalRosterTitle varchar(255), @UniqueServingTitle varchar(255),
	@MetricFirstAttendedId int = null, @MetricFirstAttendedSQL nvarchar(max), @MetricMiddleSchoolId int,
	@MetricMiddleSchoolSQL nvarchar(max), @MetricHighSchoolId int, @MetricHighSchoolSQL nvarchar(max)

-- metric title names used for the group
SELECT @ServiceRolesTitle = 'Service Attendance', @ServiceRosterTitle = 'Service Roster', @TotalRolesTitle = 'Total Attendance',
	@TotalRosterTitle = 'Total Roster', @UniqueServingTitle = 'Unique Serving'

-- Service Roles Query
SELECT @MetricServiceRolesSQL = N'
	/* ====================================================================== */
	-- Returns roles filled served by Campus and Service from the previous day
	-- Returns a timestamp of 00:00:00 when no schedule is assigned
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, Attendance.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + LEFT(ISNULL(Schedule.Value, ''00:00''), 5) AS ScheduleDate
	FROM PersonAlias PA 
	INNER JOIN (
		SELECT PersonAliasId, CampusId, ScheduleId
		FROM [Attendance] 
		WHERE DidAttend = 1
			AND GroupId = {{GroupId}}
			AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0)
			AND StartDateTime < CONVERT(DATE, GETDATE())
	) Attendance
	ON PA.Id = Attendance.PersonAliasId	
	INNER JOIN (
		-- iCal DTStart: constant 22 characters, only interested in Service Time
		SELECT Id, STUFF(SUBSTRING(iCalendarContent, PATINDEX(''%DTSTART%'', iCalendarContent) +17, 4), 3, 0, '':'') AS Value
		FROM Schedule
		WHERE EffectiveStartDate < GETDATE()
	) Schedule
	ON Schedule.Id = Attendance.ScheduleId
	GROUP BY Attendance.CampusId, Schedule.Value
'

-- Service Roster Query
SELECT @MetricServiceRosterSQL = N'
	/* ====================================================================== */
	-- Returns roster numbers by Campus and Service from the previous day
	-- Returns a timestamp of 00:00:00 when no schedule is assigned
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, Campus.Id AS EntityId, 
		DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ISNULL(CONVERT(datetime, LEFT(Schedule.Value, ISNULL(NULLIF(CHARINDEX('P', Schedule.Value),0), NULLIF(CHARINDEX('A', Schedule.Value),0))) + 'M'), ''00:00'') AS ScheduleDate	
	FROM GroupMember GM
	INNER JOIN [Group] G
		ON GM.GroupId = G.Id
	-- Filter by Campus
	INNER JOIN (
		SELECT AV.EntityId AS MemberId, C.Id
		FROM [Attribute] CA
		INNER JOIN AttributeValue AV
			ON AV.AttributeId = CA.Id
			AND CA.[Key] = ''Campus''
			AND CA.EntityTypeQualifierColumn = ''GroupTypeId''
			AND CA.EntityTypeQualifierValue = {{GroupTypeId}}
			AND AV.Value <> ''''
		INNER JOIN Campus C
			ON CONVERT(UNIQUEIDENTIFIER, AV.Value) = C.[Guid]
	) Campus
		ON GM.Id = Campus.MemberId
	-- Filter by Schedule
	LEFT JOIN (
		SELECT AV.EntityId, DV.Value
		FROM DefinedValue DV
		LEFT JOIN (
			SELECT EntityId, r.value(''.'', ''UNIQUEIDENTIFIER'') AS Schedule
			FROM (
				-- Denormalize the comma-delimited GUID string
				SELECT Value, EntityId, CAST(''<n>'' + REPLACE(Value, '','', ''</n><n>'') + ''</n>'' AS XML) AS Schedules
				FROM [Attribute] SA
				INNER JOIN AttributeValue AV
					ON AV.AttributeId = SA.Id
					AND SA.[Key] = ''Schedule''
					AND SA.EntityTypeQualifierColumn = ''GroupTypeId''
					AND SA.EntityTypeQualifierValue = {{GroupTypeId}}
					AND AV.Value <> ''''
			) AS nodes 
			-- Parse the xml as a table (for joining)
			CROSS APPLY Schedules.nodes(''n'') AS parse(r)		
		) AV 
		ON AV.Schedule = DV.[Guid]
		WHERE EntityId IS NOT NULL
	) Schedule
		ON GM.Id = Schedule.EntityId
	WHERE GM.GroupId = {{GroupId}}
		AND GM.GroupMemberStatus = 1
	GROUP BY Campus.Id, Schedule.Value
'

-- Total Roles Query
SELECT @MetricTotalRolesSQL = N'
	/* ====================================================================== */
	-- Returns total roles filled served by Campus from the previous day
	-- Returns a timestamp of 00:00:00 since this is by total and not service
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, Attendance.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM PersonAlias PA 
	INNER JOIN (
		SELECT PersonAliasId, CampusId, ScheduleId
		FROM [Attendance] 
		WHERE DidAttend = 1
			AND GroupId = {{GroupId}}
			AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0)
			AND StartDateTime < CONVERT(DATE, GETDATE())
	) Attendance
	ON PA.Id = Attendance.PersonAliasId	
	GROUP BY CampusId
'

-- Total Roster Query
SELECT @MetricTotalRosterSQL = N'
	/* ====================================================================== */
	-- Returns total roster numbers by Campus from the previous day
	-- Returns a timestamp of 00:00:00 since this is by total and not service
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, Campus.Id AS EntityId, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM GroupMember GM
	INNER JOIN [Group] G
		ON GM.GroupId = G.Id
	-- Filter by Campus
	INNER JOIN (
		SELECT AV.EntityId AS MemberId, C.Id
		FROM [Attribute] CA
		INNER JOIN AttributeValue AV
			ON AV.AttributeId = CA.Id
			AND CA.[Key] = ''Campus''
			AND CA.EntityTypeQualifierColumn = ''GroupTypeId''
			AND CA.EntityTypeQualifierValue = {{GroupTypeId}}
			AND AV.Value <> ''
		INNER JOIN Campus C
			ON CONVERT(UNIQUEIDENTIFIER, AV.Value) = C.[Guid]
	) Campus
		ON GM.Id = Campus.MemberId	
	WHERE GM.GroupId = {{GroupId}}
		AND GM.GroupMemberStatus = 1
	GROUP BY Campus.Id
'

-- Total Unique Serving Query
SELECT @MetricUniqueServingSQL = N'
	/* ====================================================================== */
	-- Returns total unique serving by Campus from the previous day
	-- Returns a timestamp of 00:00:00 since this is by total and not service
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, Attendance.CampusId AS EntityId, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM PersonAlias PA 
	INNER JOIN (
		SELECT PersonAliasId, CampusId
		FROM [Attendance]	
		WHERE DidAttend = 1
			AND GroupId = {{GroupId}}
			AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0)
			AND StartDateTime < CONVERT(DATE, GETDATE())
	) Attendance
		ON PA.Id = Attendance.PersonAliasId	
	GROUP BY CampusId
'

-- First Attended Query
SELECT @MetricFirstAttendedSQL = N'
	/* ====================================================================== */
	-- Returns first attendances by Campus from the previous day
	-- Returns a timestamp of 00:00:00 since this is by total and not service
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, Attendance.CampusId AS EntityId
		, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''00:00'' AS ScheduleDate
	FROM PersonAlias PA 
	INNER JOIN (
		SELECT PersonAliasId, A.CampusId, StartDateTime AS ScheduleDate, ROW_NUMBER() OVER (
			PARTITION BY PersonAliasId, A.CampusId ORDER BY StartDateTime
		) AS RowNumber
		FROM [Attendance] A
		INNER JOIN [Group] G
			ON A.GroupId = G.Id
			AND G.GroupTypeId = {{GroupTypeId}}
		WHERE DidAttend = 1	
	) Attendance
	ON PA.Id = Attendance.PersonAliasId
	AND ScheduleDate >= DATEADD(dd, DATEDIFF(dd, 1, @GETDATE()), 0)
	AND ScheduleDate < CONVERT(DATE, @GETDATE())
	AND RowNumber = 1
	GROUP BY CampusId
'

-- Fuse MS Total
SELECT @MetricMiddleSchoolSQL  = N'
	/* ====================================================================== */
	-- Returns total MS attended by Campus from the previous day
	-- Returns a schedule of 19:00 (Fuse 7pm service)
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, CampusId AS EntityId, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''19:00'' AS ScheduleDate
	FROM PersonAlias PA 
	INNER JOIN (
		SELECT PersonAliasId, A.CampusId
		FROM [Attendance] A	
		INNER JOIN [Group] G
			ON A.GroupId = G.Id
			AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''Fuse Attendee'')
		WHERE DidAttend = 1	
		AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0)
		AND StartDateTime < CONVERT(DATE, GETDATE())
		AND (
			CASE WHEN ISNUMERIC(LEFT(G.Name, 1)) = 1
			THEN LEFT(G.Name, 1) ELSE NULL END
		) IN ( 6,7,8 ) -- 6th, 7th, 8th grade
	) Attendance
		ON PA.Id = Attendance.PersonAliasId	
	GROUP BY CampusId	
'

-- Fuse HS Total
SELECT @MetricHighSchoolSQL = N'
	/* ====================================================================== */
	-- Returns total HS attended by Campus from the previous day
	-- Returns a schedule of 19:00 (Fuse 7pm service)
	/* ====================================================================== */
	SELECT COUNT(1) AS Value, CampusId AS EntityId, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + ''19:00'' AS ScheduleDate
	FROM PersonAlias PA 
	INNER JOIN (
		SELECT PersonAliasId, A.CampusId
		FROM [Attendance] A	
		INNER JOIN [Group] G
			ON A.GroupId = G.Id
			AND G.GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Name] = ''Fuse Attendee'')
		WHERE DidAttend = 1	
		AND StartDateTime >= DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0)
		AND StartDateTime < CONVERT(DATE, GETDATE())
		AND (
			CASE WHEN ISNUMERIC(LEFT(G.Name, 1)) = 1
			THEN LEFT(G.Name, 1) ELSE NULL END
		) IN ( 9, 1 ) -- 9th, 10th, 11th, 12th grade
	) Attendance
		ON PA.Id = Attendance.PersonAliasId	
	GROUP BY CampusId	
'

/* ====================================================== */
-- setup metric categories
/* ====================================================== */

DECLARE @ParentCategoryName nvarchar(255), @ParentCategoryId int, 
	@ChildCategoryName nvarchar(255), @ChildCategoryId int, 
	@MetricServiceId int, @MetricTotalId int

DECLARE @scopeIndex int, @numItems int
SELECT @scopeIndex = min(Id) FROM #metricTypes
SELECT @numItems = count(1) + @scopeIndex FROM #metricTypes

WHILE @scopeIndex < @numItems
BEGIN
	
	SELECT @ParentCategoryName = parentType, @ChildCategoryName = childType,
		@ParentCategoryId = parentTypeId, @ChildCategoryId = childTypeId
	FROM #metricTypes WHERE id = @scopeIndex

	SELECT @msg = 'Creating categories for ' + @ParentCategoryName + ' / ' + @ChildCategoryName
	RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

	IF @ParentCategoryName <> ''
	BEGIN

		/* ====================================================== */
		-- create the parent if it doesn't exist
		/* ====================================================== */
		IF @ParentCategoryId IS NULL
		BEGIN
			INSERT [Category] (IsSystem, ParentCategoryId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Guid], [Order])
			VALUES ( @IsSystem, NULL, @MetricCategoryEntityTypeId, '', '', @ParentCategoryName, NEWID(), @Order )

			SET @ParentCategoryId = SCOPE_IDENTITY()
		
			UPDATE #metricTypes 
			SET parentTypeId = @ParentCategoryId
			WHERE parentType = @ParentCategoryName
		END	

		IF @ChildCategoryId IS NULL AND @ChildCategoryName <> ''
		BEGIN

			INSERT [Category] (IsSystem, ParentCategoryId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Guid], [Order])
			VALUES ( @IsSystem, @ParentCategoryId, @MetricCategoryEntityTypeId, '', '', @ChildCategoryName, NEWID(), @Order )

			SET @ChildCategoryId = SCOPE_IDENTITY()
		
			UPDATE #metricTypes 
			SET childTypeId = @ChildCategoryId
			WHERE childType = @ChildCategoryName
		END

		-- create non-volunteer metrics
		IF @ParentCategoryName <> 'Volunteers'
		BEGIN
			
			SELECT @MetricServiceId = NULL, @MetricTotalId = NULL

			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, EntityTypeId, [Guid], IconCssClass, CreatedDateTime)
			VALUES ( 0, 'Service Numbers', 'Metric to track ' + @ChildCategoryName + ' by service', @False, 
				@MetricSourceManualId, '', '', '', @CampusEntityTypeId, NEWID(), '', @CreatedDateTime )

			SELECT @MetricServiceId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricServiceId, @ChildCategoryId, @Order, NEWID() )

			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, EntityTypeId, [Guid], IconCssClass, CreatedDateTime)
			VALUES ( 0, 'Total Numbers', 'Metric to track ' + @ChildCategoryName + ' roles by campus and service', @False, 
				@MetricSourceManualId, '', '', '', @CampusEntityTypeId, NEWID(), '', @CreatedDateTime )

			SELECT @MetricTotalId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricTotalId, @ChildCategoryId, @Order, NEWID() )

		END
		-- end non-volunteer metrics
	END
	
	SELECT @ParentCategoryId = NULL, @ChildCategoryId = NULL, @ParentCategoryName = '', @ChildCategoryName = ''

	SELECT @scopeIndex = @scopeIndex +1
END

SELECT @msg = 'Finished creating categories'
RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

/* ====================================================== */
-- create metrics for groups
/* ====================================================== */
DECLARE @GroupId int, @GroupName nvarchar(255), @GroupTypeId int, 
	@GroupTypeName nvarchar(255), @ParentGroupTypeName nvarchar(255)

SELECT @scopeIndex = min(Id) FROM #metricGroups
SELECT @numItems = count(1) + @scopeIndex FROM #metricGroups

WHILE @scopeIndex < @numItems
BEGIN
	
	SELECT @ParentCategoryId = NULL, @GroupId = NULL, @GroupName = '',
		@GroupTypeId = NULL, @GroupTypeName = '', @ChildCategoryId = NULL

	SELECT @GroupTypeName = groupTypeName, @GroupName = groupName
	FROM #metricGroups
	WHERE ID = @scopeIndex
	
	-- lookup grouptype and group id's for parsing
	SELECT @GroupTypeId = GT.[Id], @GroupId = G.[Id]
	FROM GroupType GT
	INNER JOIN [Group] G
	ON GT.Id = G.GroupTypeId		
	WHERE GT.Name = @GroupTypeName
		AND G.Name = @GroupName

	-- get the parent grouptype name 
	SELECT @ParentGroupTypeName = GT.Name
	FROM GroupType GT
	INNER JOIN GroupTypeAssociation GTA
	ON GT.Id = GTA.GroupTypeId
	AND GTA.ChildGroupTypeId = @GroupTypeId
	AND GTA.GroupTypeId <> @GroupTypeId

	-- lookup parent category
	SELECT @ParentCategoryId = C.[Id] 
	FROM Category C
	INNER JOIN Category PC
	ON C.ParentCategoryId = PC.Id
	WHERE C.EntityTypeId = @MetricCategoryEntityTypeId
	AND C.Name = @ParentGroupTypeName
	AND PC.ParentCategoryId IS NULL

	-- check that the parameters and category exist
	IF @GroupTypeID IS NOT NULL AND @ParentCategoryId IS NOT NULL
	BEGIN

		SELECT @msg = 'Creating metrics for ' + @GroupTypeName + ' / ' + @GroupName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

		-- create a category for the group
		INSERT [Category] (IsSystem, ParentCategoryId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Guid], [Order])
		VALUES ( @IsSystem, @ParentCategoryId, @MetricCategoryEntityTypeId, '', '', @GroupName, NEWID(), @Order )

		SET @ChildCategoryId = SCOPE_IDENTITY()
		
		-- set up variables for group metrics
		SELECT @MetricServiceRolesId = NULL, @MetricServiceRosterId = NULL, 
			@MetricTotalRolesId = NULL, @MetricTotalRosterId = NULL, @MetricUniqueServingId = NULL
		
		-- update the metric queries with the current GroupTypeId and GroupId
		SELECT @MetricServiceRolesSQL = REPLACE(REPLACE(@MetricServiceRolesSQL, '{{GroupTypeId}}', @GroupTypeId), '{{GroupId}}', @GroupId )
		SELECT @MetricServiceRosterSQL = REPLACE(REPLACE(@MetricServiceRosterSQL, '{{GroupTypeId}}', @GroupTypeId), '{{GroupId}}', @GroupId )
		SELECT @MetricTotalRolesSQL = REPLACE(REPLACE(@MetricTotalRolesSQL, '{{GroupTypeId}}', @GroupTypeId), '{{GroupId}}', @GroupId )
		SELECT @MetricTotalRosterSQL = REPLACE(REPLACE(@MetricTotalRosterSQL, '{{GroupTypeId}}', @GroupTypeId), '{{GroupId}}', @GroupId )
		SELECT @MetricUniqueServingSQL = REPLACE(REPLACE(@MetricUniqueServingSQL, '{{GroupTypeId}}', @GroupTypeId), '{{GroupId}}', @GroupId )

		/* ============================ */
		-- {Group} Service Roles
		/* ============================ */
		SELECT @MetricServiceRolesId = M.[Id]
		FROM Metric M
		INNER JOIN MetricCategory MC
		ON M.Id = MC.MetricId
		WHERE EntityTypeId = @CampusEntityTypeId
			AND SourceValueTypeId = @MetricSourceSQLId
			AND MC.CategoryId = @ChildCategoryId
			AND Title = @GroupName + ' ' + @ServiceRolesTitle

		-- create if it doesn't exist
		IF @MetricServiceRolesId IS NULL
		BEGIN
			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, ScheduleId, EntityTypeId, [Guid], ForeignId, IconCssClass, CreatedDateTime)
			VALUES ( 0, @GroupName + ' ' + @ServiceRolesTitle, 'Metric to track ' + @GroupName + ' roles by campus and service', @False, 
				@MetricSourceSQLId, @MetricServiceRolesSQL, '', '', @SundayScheduleId, @CampusEntityTypeId, NEWID(), @GroupId, '', @CreatedDateTime )

			SELECT @MetricServiceRolesId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricServiceRolesId, @ChildCategoryId, @Order, NEWID() )
		END

		/* ============================ */
		-- {Group} Service Roster
		/* ============================ */
		SELECT @MetricServiceRosterId = M.[Id] 
		FROM Metric M
		INNER JOIN MetricCategory MC
		ON M.Id = MC.MetricId
		WHERE EntityTypeId = @CampusEntityTypeId
			AND SourceValueTypeId = @MetricSourceSQLId
			AND MC.CategoryId = @ChildCategoryId
			AND Title = @GroupName + ' ' + @ServiceRosterTitle

		-- create if it doesn't exist
		IF @MetricServiceRosterId IS NULL
		BEGIN
			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, ScheduleId, EntityTypeId, [Guid], ForeignId, IconCssClass, CreatedDateTime)
			VALUES ( 0, @GroupName + ' ' + @ServiceRosterTitle, 'Metric to track ' + @GroupName + ' roster by campus and service', @False, 
				@MetricSourceSQLId, @MetricServiceRosterSQL, '', '', @SundayScheduleId, @CampusEntityTypeId, NEWID(), @GroupId, '', @CreatedDateTime )

			SELECT @MetricServiceRosterId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricServiceRosterId, @ChildCategoryId, @Order, NEWID() )
		END

		/* ============================ */
		-- {Group} Total Roles
		/* ============================ */
		SELECT @MetricTotalRolesId = M.[Id] 
		FROM Metric M
		INNER JOIN MetricCategory MC
		ON M.Id = MC.MetricId
		WHERE EntityTypeId = @CampusEntityTypeId
			AND SourceValueTypeId = @MetricSourceSQLId
			AND MC.CategoryId = @ChildCategoryId
			AND Title = @GroupName + ' ' + @TotalRolesTitle

		-- create if it doesn't exist
		IF @MetricTotalRolesId IS NULL
		BEGIN
			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, ScheduleId, EntityTypeId, [Guid], ForeignId, IconCssClass, CreatedDateTime)
			VALUES ( 0, @GroupName + ' ' + @TotalRolesTitle, 'Metric to track ' + @GroupName + ' total roles filled by campus', @False, 
				@MetricSourceSQLId, @MetricTotalRolesSQL, '', '', @SundayScheduleId, @CampusEntityTypeId, NEWID(), @GroupId, '', @CreatedDateTime )

			SELECT @MetricTotalRolesId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricTotalRolesId, @ChildCategoryId, @Order, NEWID() )
		END

		/* ============================ */
		-- {Group} Total Roster
		/* ============================ */
		SELECT @MetricTotalRosterId = M.[Id] 
		FROM Metric M
		INNER JOIN MetricCategory MC
		ON M.Id = MC.MetricId
		WHERE EntityTypeId = @CampusEntityTypeId
			AND SourceValueTypeId = @MetricSourceSQLId
			AND MC.CategoryId = @ChildCategoryId
			AND Title = @GroupName + ' ' + @TotalRosterTitle

		-- create if it doesn't exist
		IF @MetricTotalRosterId IS NULL
		BEGIN
			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, ScheduleId, EntityTypeId, [Guid], ForeignId, IconCssClass, CreatedDateTime)
			VALUES ( 0, @GroupName + ' ' + @TotalRosterTitle, 'Metric to track ' + @GroupName + ' total roster by campus', @False, 
				@MetricSourceSQLId, @MetricTotalRosterSQL, '', '', @SundayScheduleId, @CampusEntityTypeId, NEWID(), @GroupId, '', @CreatedDateTime )

			SELECT @MetricTotalRosterId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricTotalRosterId, @ChildCategoryId, @Order, NEWID() )
		END

		/* ============================ */
		-- {Group} Unique Serving
		/* ============================ */
		SELECT @MetricUniqueServingId = M.[Id] 
		FROM Metric M
		INNER JOIN MetricCategory MC
		ON M.Id = MC.MetricId
		WHERE EntityTypeId = @CampusEntityTypeId
			AND SourceValueTypeId = @MetricSourceSQLId
			AND MC.CategoryId = @ChildCategoryId
			AND Title = @GroupName + ' ' + @UniqueServingTitle

		-- create if it doesn't exist
		IF @MetricUniqueServingId IS NULL
		BEGIN
			INSERT [Metric] (IsSystem, Title, [Description], IsCumulative, SourceValueTypeId, SourceSql, XAxisLabel, YAxisLabel, ScheduleId, EntityTypeId, [Guid], ForeignId, IconCssClass, CreatedDateTime)
			VALUES ( 0, @GroupName + ' ' + @UniqueServingTitle, 'Metric to track ' + @GroupName + ' total unique volunteers by campus', @False, 
				@MetricSourceSQLId, @MetricUniqueServingSQL, '', '', @SundayScheduleId, @CampusEntityTypeId, NEWID(), @GroupId, '', @CreatedDateTime )

			SELECT @MetricUniqueServingId = SCOPE_IDENTITY()

			INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
			VALUES ( @MetricUniqueServingId, @ChildCategoryId, @Order, NEWID() )
		END

		-- reset queries to parameterized state
		SELECT @MetricServiceRolesSQL = REPLACE(REPLACE(@MetricServiceRolesSQL, @GroupTypeId, '{{GroupTypeId}}'), @GroupId, '{{GroupId}}' )
		SELECT @MetricServiceRosterSQL = REPLACE(REPLACE(@MetricServiceRosterSQL, @GroupTypeId, '{{GroupTypeId}}'), @GroupId, '{{GroupId}}' )
		SELECT @MetricTotalRolesSQL = REPLACE(REPLACE(@MetricTotalRolesSQL, @GroupTypeId, '{{GroupTypeId}}'), @GroupId, '{{GroupId}}' )
		SELECT @MetricTotalRosterSQL = REPLACE(REPLACE(@MetricTotalRosterSQL, @GroupTypeId, '{{GroupTypeId}}'), @GroupId, '{{GroupId}}' )
		SELECT @MetricUniqueServingSQL = REPLACE(REPLACE(@MetricUniqueServingSQL, @GroupTypeId, '{{GroupTypeId}}'), @GroupId, '{{GroupId}}' )

	END
	ELSE BEGIN
		SELECT @msg = 'Could not find metric category for ' + @GroupTypeName + ' / ' + @GroupName + ' / ' + @ParentCategoryName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
	END
	-- end grouptype check

	SELECT @scopeIndex = @scopeIndex +1
END
-- end groups loop

-- Insert HS attendance to Fuse
DECLARE @newId AS INT; 
DECLARE @fuseAttendanceCategoryId AS INT;

SELECT 
	@fuseAttendanceCategoryId = c.Id
FROM 
	Category c
	JOIN MetricCategory mc ON mc.CategoryId = c.Id
WHERE
	c.Name = 'Fuse Attendance'
GROUP BY
	c.Id;

INSERT [Metric] (
	IsSystem, 
	Title, 
	[Description], 
	IsCumulative, 
	SourceValueTypeId, 
	SourceSql, 
	XAxisLabel, 
	YAxisLabel, 
	ScheduleId, 
	EntityTypeId, 
	[Guid],
	IconCssClass,
	CreatedDateTime)
VALUES ( 
	@IsSystem, 
	'Fuse HS Attendance', 
	'Metric to track Fuse HS attendees by campus', 
	@False, 
	@MetricSourceSQLId, 
	@MetricHighSchoolSQL, 
	'', 
	'', 
	@FuseScheduleId, 
	@CampusEntityTypeId, 
	NEWID(),
	'',
	@CreatedDateTime);

SELECT @newId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @newId, @fuseAttendanceCategoryId, @Order, NEWID() );

-- Insert MS
INSERT [Metric] (
	IsSystem, 
	Title, 
	[Description], 
	IsCumulative, 
	SourceValueTypeId, 
	SourceSql, 
	XAxisLabel, 
	YAxisLabel, 
	ScheduleId, 
	EntityTypeId, 
	[Guid],
	IconCssClass,
	CreatedDateTime)
VALUES ( 
	@IsSystem, 
	'Fuse MS Attendance', 
	'Metric to track Fuse MS attendees by campus', 
	@False, 
	@MetricSourceSQLId, 
	@MetricMiddleSchoolSQL, 
	'', 
	'', 
	@FuseScheduleId, 
	@CampusEntityTypeId, 
	NEWID(),
	'',
	@CreatedDateTime);

SELECT @newId = SCOPE_IDENTITY();

INSERT [MetricCategory] (MetricId, CategoryId, [Order], [Guid])
VALUES ( @newId, @fuseAttendanceCategoryId, @Order, NEWID() );
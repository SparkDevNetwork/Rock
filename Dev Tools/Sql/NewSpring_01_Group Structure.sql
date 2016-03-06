/* ====================================================== */
-- NewSpring Script #1: 
-- Inserts campuses, groups, grouptypes AND locations.

-- Make sure you're using the right Rock database:

--USE Rock

/* ====================================================== */

-- Enable production mode for performance
SET NOCOUNT ON

-- Set common variables 
DECLARE @msg nvarchar(500) = ''
DECLARE @IsSystem bit = 0
DECLARE @Delimiter nvarchar(5) = ' - '
DECLARE @CollegeName nvarchar(50) = 'NewSpring College'
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @Order int = 0
DECLARE @BooleanFieldTypeId int
DECLARE @GroupEntityTypeId int
DECLARE @CampusEntityTypeId int
DECLARE @ScheduleEntityTypeId int
DECLARE @CheckInAreaPurposeId int
DECLARE @CampusLocationTypeId int
DECLARE @SourceTypeSQLId int

SELECT @BooleanFieldTypeId = [Id] FROM [FieldType] WHERE [Name] = 'Boolean'
SELECT @GroupEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Group'
SELECT @CampusEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Campus'
SELECT @ScheduleEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Schedule'

/* Check-in Template Purpose Type */
SELECT @CheckInAreaPurposeId = 142

/* Location Type: Campus */
SELECT @CampusLocationTypeId = [Id] FROM DefinedValue WHERE [Guid] = 'C0D7AE35-7901-4396-870E-3AAF472AAE88'

UPDATE [group] SET campusId = NULL
DELETE FROM Campus WHERE id = 1

/* ====================================================== */
-- create campuses & campus locations
/* ====================================================== */

INSERT Campus (IsSystem, Name, ShortCode, [Guid], IsActive)
VALUES
(@IsSystem, 'Aiken', 'AKN', NEWID(), @True),
(@IsSystem, 'Anderson', 'AND', NEWID(), @True),
(@IsSystem, 'Boiling Springs', 'BSP', NEWID(), @True),
(@IsSystem, 'Central', 'CEN', NEWID(), @False),
(@IsSystem, 'Charleston', 'CHS', NEWID(), @True),
(@IsSystem, 'Clemson', 'CLE', NEWID(), @True),
(@IsSystem, 'Columbia', 'COL', NEWID(), @True),
(@IsSystem, 'Florence', 'FLO', NEWID(), @True),
(@IsSystem, 'Greenville', 'GVL', NEWID(), @True),
(@IsSystem, 'Greenwood', 'GWD', NEWID(), @True),
(@IsSystem, 'Greer', 'GRR', NEWID(), @False),
(@IsSystem, 'Hilton Head', 'HHD', NEWID(), @True),
(@IsSystem, 'Lexington', 'LEX', NEWID(), @True),
(@IsSystem, 'Myrtle Beach', 'MYR', NEWID(), @True),
(@IsSystem, 'Northeast Columbia', 'NEC', NEWID(), @True),
(@IsSystem, 'Powdersville', 'POW', NEWID(), @True),
(@IsSystem, 'Rock Hill', 'RKH', NEWID(), @True),
(@IsSystem, 'Spartanburg', 'SPA', NEWID(), @True),
(@IsSystem, 'Simpsonville', 'SIM', NEWID(), @False),
(@IsSystem, 'Sumter', 'SUM', NEWID(), @True),
(@IsSystem, 'Web', 'WEB', NEWID(), @False)

-- create top-level campus locations
IF NOT EXISTS ( SELECT l.Id FROM Campus c INNER JOIN Location l ON c.Name = l.Name AND l.LocationTypeValueId = @CampusLocationTypeId )
BEGIN
	INSERT Location (Name, IsActive, LocationTypeValueId, [Guid])
	SELECT name, @True, @CampusLocationTypeId, NEWID() FROM Campus
END

-- match campus to locations
UPDATE c
SET LocationId = l.Id
FROM Campus c
INNER JOIN Location l
ON c.Name = l.Name
AND l.LocationTypeValueId = @CampusLocationTypeId

/* ====================================================== */
-- create Sunday service times and the rest of the week
/* ====================================================== */
DECLARE @ServiceParentCategoryId int, @ServiceScheduleId int, 
	@ServiceiCalSchedule nvarchar(max), @SundayParentName varchar(255),
	@ServiceName varchar(255), @WeekdayParentName varchar(255)

SELECT @SundayParentName = 'Service Times'

-- create parent category
SELECT @ServiceParentCategoryId = [Id] FROM [Category]
WHERE EntityTypeId = @ScheduleEntityTypeId
	AND Name = @SundayParentName

IF @ServiceParentCategoryId IS NULL 
BEGIN
	INSERT [Category] (IsSystem, ParentCategoryId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Guid], [Order])
	VALUES ( @IsSystem, NULL, @ScheduleEntityTypeId, '', '', @SundayParentName, NEWID(), @Order )

	SET @ServiceParentCategoryId = SCOPE_IDENTITY()
END

/* ====================================================== */
-- create the main service schedules (Sunday/Wednesday)
/* ====================================================== */
SELECT @ServiceName = 'Sunday 09:15'

SELECT @ServiceScheduleId = [Id] FROM Schedule
WHERE CategoryId = @ServiceParentCategoryId
AND Name = @ServiceName

IF @ServiceScheduleId IS NULL
BEGIN

	SELECT @ServiceiCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T100000
DTSTAMP:20130501T00000Z
DTSTART:20130501T091500
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, Description, iCalendarContent, CategoryId, [Guid])
	SELECT @ServiceName, '', @ServiceiCalSchedule, @ServiceParentCategoryId, NEWID()
END

-- Sunday 11:15
SELECT @ServiceName = 'Sunday 11:15'

SELECT @ServiceScheduleId = [Id] FROM Schedule
WHERE CategoryId = @ServiceParentCategoryId
AND Name = @ServiceName

IF @ServiceScheduleId IS NULL
BEGIN

	SELECT @ServiceiCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T100000
DTSTAMP:20130501T00000Z
DTSTART:20130501T111500
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, Description, iCalendarContent, CategoryId, [Guid])
	SELECT @ServiceName, '', @ServiceiCalSchedule, @ServiceParentCategoryId, NEWID()
END

-- Sunday 4pm
SELECT @ServiceName = 'Sunday 16:00'

SELECT @ServiceScheduleId = [Id] FROM Schedule
WHERE CategoryId = @ServiceParentCategoryId
AND Name = @ServiceName

IF @ServiceScheduleId IS NULL
BEGIN

	SELECT @ServiceiCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T100000
DTSTAMP:20130501T00000Z
DTSTART:20130501T160000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, Description, iCalendarContent, CategoryId, [Guid])
	SELECT @ServiceName, '', @ServiceiCalSchedule, @ServiceParentCategoryId, NEWID()
END

-- Sunday 6pm
SELECT @ServiceName = 'Sunday 18:00'

SELECT @ServiceScheduleId = [Id] FROM Schedule
WHERE CategoryId = @ServiceParentCategoryId
AND Name = @ServiceName

IF @ServiceScheduleId IS NULL
BEGIN

	SELECT @ServiceiCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T100000
DTSTAMP:20130501T00000Z
DTSTART:20130501T180000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, Description, iCalendarContent, CategoryId, [Guid])
	SELECT @ServiceName, '', @ServiceiCalSchedule, @ServiceParentCategoryId, NEWID()
END

-- Fuse Service
SELECT @ServiceName = 'Fuse'

SELECT @ServiceScheduleId = [Id] FROM Schedule
WHERE CategoryId = @ServiceParentCategoryId
AND Name = @ServiceName

IF @ServiceScheduleId IS NULL
BEGIN

	SELECT @ServiceiCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T100000
DTSTAMP:20130501T00000Z
DTSTART:20130501T190000
RRULE:FREQ=WEEKLY;BYDAY=WE
SEQUENCE:0
UID:' + CONVERT(VARCHAR(36), NEWID()) + '
END:VEVENT
END:VCALENDAR'
	
	INSERT [Schedule] (Name, Description, iCalendarContent, CategoryId, [Guid])
	SELECT @ServiceName, '', @ServiceiCalSchedule, @ServiceParentCategoryId, NEWID()
END

/* ====================================================== */
-- create the weekday service schedules
/* ====================================================== */
SELECT @ServiceParentCategoryId = NULL
SELECT @WeekdayParentName = 'Weekdays'

-- create parent category
SELECT @ServiceParentCategoryId = [Id] FROM [Category]
WHERE EntityTypeId = @ScheduleEntityTypeId
	AND Name = @WeekdayParentName

IF @ServiceParentCategoryId IS NULL 
BEGIN
	INSERT [Category] (IsSystem, ParentCategoryId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Guid], [Order])
	VALUES ( @IsSystem, NULL, @ScheduleEntityTypeId, '', '', @WeekdayParentName, NEWID(), @Order )

	SELECT @ServiceParentCategoryId = SCOPE_IDENTITY()
END

-- table variable to hold the weekdays
DECLARE @Weekdays TABLE ( EachDay varchar(255 ) )

INSERT @Weekdays ( EachDay )
VALUES ( 'Monday' ), ('Tuesday'), ('Wednesday'), ('Thursday'), ('Friday'), ('Saturday' )

IF NOT EXISTS (SELECT [Id] FROM Schedule WHERE Name IN (SELECT EachDay FROM @Weekdays ) )
BEGIN
	
	SELECT @ServiceiCalSchedule = N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T100000
DTSTAMP:20130501T00000Z
DTSTART:20130501T060000
RRULE:FREQ=WEEKLY;BYDAY={{Day}}
SEQUENCE:0
UID: {{UID}}
END:VEVENT
END:VCALENDAR'

	-- insert schedule content
	INSERT [Schedule] (Name, Description, CategoryId, [Guid], iCalendarContent)
	SELECT EachDay, '', @ServiceParentCategoryId, NEWID(), REPLACE(
		REPLACE(@ServiceiCalSchedule, '{{Day}}', LEFT(UPPER(EachDay), 2) )
		, '{{UID}}', NEWID())
	FROM @Weekdays

END

/* ====================================================== */
-- Special Needs Structure
/* ====================================================== */
DECLARE @SpecialNeedsAttributeId INT
DECLARE @SpecialNeedsGroupTypeId INT
SELECT @SpecialNeedsGroupTypeId = (
	SELECT [Id] FROM [GroupType]
	WHERE [Name] = 'Check In By Special Needs'	
);

IF @SpecialNeedsGroupTypeId IS NULL
BEGIN
	INSERT [GroupType] ( [IsSystem], [Name], [Description], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [GroupTypePurposeValueId],
		[TakesAttendance], [AttendanceRule], [AttendancePrintTo], [Order], [InheritedGroupTypeId], [LocationSelectionMode], [AllowedScheduleTypes], [SendAttendanceReminder], [Guid] ) 
	VALUES ( @IsSystem, 'Check In By Special Needs', 'Indicates if this group is for those who have special needs.', 'Group', 'Member', @False, @False, @False, 145,
		@False, 1, 0, 0, 15, 0, 0, 0, NEWID() );
	
	SET @SpecialNeedsGroupTypeId = SCOPE_IDENTITY()

	INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [CanView], [CanEdit], [Guid] )
	VALUES ( @IsSystem, @SpecialNeedsGroupTypeId, 'Member', 0, @False, @False, @False, NEWID() )

	DECLARE @SpecialNeedsGroupMemberId int
	SET @SpecialNeedsGroupMemberId = SCOPE_IDENTITY()

	UPDATE [GroupType]
	SET DefaultGroupRoleId = @SpecialNeedsGroupMemberId
	WHERE [Id] = @SpecialNeedsGroupTypeId
END


SELECT @SpecialNeedsAttributeId = Id FROM [Attribute] WHERE [Key] = 'IsSpecialNeeds' 
	AND EntityTypeid = @GroupEntityTypeId
IF @SpecialNeedsAttributeId IS NULL or @SpecialNeedsAttributeId = ''
BEGIN

	INSERT [Attribute] ( [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) 
	VALUES ( @IsSystem, @BooleanFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @SpecialNeedsGroupTypeId, 'IsSpecialNeeds', 'Is Special Needs',
		'Indicates IF this group caters to those who have special needs.', @False, @False, 'True', @False, @False, NEWID()
	);

	SET @SpecialNeedsAttributeId = SCOPE_IDENTITY()
END

/* ====================================================== */
-- base check-in areas
/* ====================================================== */
IF object_id('tempdb..#topAreas') IS NOT NULL
BEGIN
	drop table #topAreas
END
create table #topAreas (
	ID int IDENTITY(1,1),
	parentArea nvarchar(255),
	childArea nvarchar(255),
	inheritedType int
)

-- Check-in Area, GroupType, Inherited Type
INSERT #topAreas
VALUES
('Creativity & Tech', 'Creativity & Tech Attendee', 15),
('Creativity & Tech', 'Creativity & Tech Volunteer', 15),
('Central Events', 'Event Attendee', 15),
('Central Events', 'Event Volunteer', 15),
('Fuse', 'Fuse Attendee', 17),
('Fuse', 'Fuse Volunteer', 15),
('Guest Services', 'Guest Services Attendee', 15),
('Guest Services', 'Guest Services Volunteer', 15),
('KidSpring', 'Nursery Attendee', 15),
('KidSpring', 'Preschool Attendee', 15),
('KidSpring', 'Elementary Attendee', 17),
('KidSpring', 'Special Needs Attendee', @SpecialNeedsGroupTypeId),
('KidSpring', 'Nursery Volunteer', 15),
('KidSpring', 'Preschool Volunteer', 15),
('KidSpring', 'Elementary Volunteer', 15),
('KidSpring', 'Special Needs Volunteer', 15),
('KidSpring', 'Support Volunteer', 15),
('KidSpring', 'Production Volunteer', 15),
('Next Steps', 'Next Steps Attendee', 15),
('Next Steps', 'Next Steps Volunteer', 15)

/* ====================================================== */
-- campus group structure
/* ====================================================== */
IF object_id('tempdb..#campusGroups') IS NOT NULL
BEGIN
	drop table #campusGroups
END
create table #campusGroups (
	ID int IDENTITY(1,1),
	groupTypeName nvarchar(255),
	groupName nvarchar(255),
	locationName nvarchar(255),
)

-- GroupType, Group, Location
INSERT #campusGroups
VALUES
-- child attendee structure
('Elementary Attendee', 'Base Camp', 'Base Camp'), 
('Elementary Attendee', 'ImagiNation 1st', 'ImagiNation 1st'), 
('Elementary Attendee', 'ImagiNation K', 'ImagiNation K'), 
('Elementary Attendee', 'Jump Street 2nd', 'Jump Street 2nd'), 
('Elementary Attendee', 'Jump Street 3rd', 'Jump Street 3rd'), 
('Elementary Attendee', 'Shockwave 4th', 'Shockwave 4th'), 
('Elementary Attendee', 'Shockwave 5th', 'Shockwave 5th'), 
('Nursery Attendee', 'Cuddlers', 'Wonder Way 1'), 
('Nursery Attendee', 'Cuddlers', 'Wonder Way 2'), 
('Nursery Attendee', 'Crawlers', 'Wonder Way 3'), 
('Nursery Attendee', 'Crawlers', 'Wonder Way 4'), 
('Nursery Attendee', 'Walkers', 'Wonder Way 5'), 
('Nursery Attendee', 'Walkers', 'Wonder Way 6'), 
('Nursery Attendee', 'Walkers', 'Wonder Way 7'), 
('Nursery Attendee', 'Young Walkers', ''), 
('Nursery Attendee', 'Older Walkers', ''), 
('Nursery Attendee', 'Toddlers', 'Wonder Way 8'), 
('Nursery Attendee', 'Young Toddlers', ''), 
('Nursery Attendee', 'Older Toddlers', ''), 
('Preschool Attendee', '24-29 mo.', 'Fire Station'),
('Preschool Attendee', '24-29 mo.', 'Lil'' Spring'),
('Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
('Preschool Attendee', '30-31 mo.', 'Fire Station'),
('Preschool Attendee', '30-31 mo.', 'Lil'' Spring'),
('Preschool Attendee', '30-31 mo.', 'Pop''s Garage'),
('Preschool Attendee', '32-33 mo.', 'Fire Station'),
('Preschool Attendee', '32-33 mo.', 'Lil'' Spring'),
('Preschool Attendee', '32-33 mo.', 'Pop''s Garage'),
('Preschool Attendee', '34-35 mo.', 'Fire Station'),
('Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
('Preschool Attendee', '34-35 mo.', 'Pop''s Garage'),
('Preschool Attendee', '36-37 mo.', 'Spring Fresh'),
('Preschool Attendee', '38-39 mo.', 'Spring Fresh'),
('Preschool Attendee', '40-41 mo.', 'SpringTown Toys'),
('Preschool Attendee', '42-43 mo.', 'SpringTown Toys'),
('Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
('Preschool Attendee', '46-47 mo.', 'SpringTown Toys'),
('Preschool Attendee', '48-49 mo.', 'Treehouse'),
('Preschool Attendee', '50-51 mo.', 'Treehouse'),
('Preschool Attendee', '52-53 mo.', 'Treehouse'),
('Preschool Attendee', '54-55 mo.', 'Treehouse'),
('Preschool Attendee', '56-57 mo.', 'Treehouse'),
('Preschool Attendee', '58-59 mo.', 'Treehouse'),
('Preschool Attendee', '60-72 mo.', 'Treehouse'),
('Preschool Attendee', 'Base Camp Jr.', 'Base Camp Jr.'), 
('Special Needs Attendee', 'Spring Zone Jr.', 'Spring Zone Jr.'), 
('Special Needs Attendee', 'Spring Zone', 'Spring Zone'), 

-- adult attendee/volunteer structure from COL
('Creativity & Tech Attendee', 'Choir', 'Choir'), 
('Creativity & Tech Attendee', 'Load In', 'Load In'), 
('Creativity & Tech Attendee', 'Load Out', 'Load Out'), 
('Creativity & Tech Attendee', 'Special Event Attendee', 'Special Event Attendee'), 
('Creativity & Tech Volunteer', 'Band', 'Band'), 
('Creativity & Tech Volunteer', 'Band Green Room', 'Band Green Room'),
('Creativity & Tech Volunteer', 'Editorial Team', 'Editorial Team'), 
('Creativity & Tech Volunteer', 'IT Team', 'IT Team'), 
('Creativity & Tech Volunteer', 'New Serve Team', 'New Serve Team'), 
('Creativity & Tech Volunteer', 'Office Team', 'Office Team'), 
('Creativity & Tech Volunteer', 'Production Team', 'Production Team'), 
('Creativity & Tech Volunteer', 'Social Media/PR Team', 'Social Media/PR Team'), 
('Creativity & Tech Volunteer', 'Special Event Volunteer', 'Special Event Volunteer'), 
('Elementary Volunteer', 'Base Camp Volunteer', 'Base Camp Volunteer'), 
('Elementary Volunteer', 'Elementary Early Bird Volunteer', 'Elementary Early Bird Volunteer'), 
('Elementary Volunteer', 'Elementary Service Leader', 'Elementary Service Leader'), 
('Elementary Volunteer', 'Elementary Area Leader', 'Elementary Area Leader'), 
('Elementary Volunteer', 'ImagiNation Volunteer', 'ImagiNation Volunteer'), 
('Elementary Volunteer', 'Jump Street Volunteer', 'Jump Street Volunteer'), 
('Elementary Volunteer', 'Shockwave Volunteer', 'Shockwave Volunteer'), 
('Fuse Attendee', '10th Grade Student', '10th Grade Student'), 
('Fuse Attendee', '11th Grade Student', '11th Grade Student'), 
('Fuse Attendee', '12th Grade Student', '12th Grade Student'), 
('Fuse Attendee', '6th Grade Student', '6th Grade Student'), 
('Fuse Attendee', '7th Grade Student', '7th Grade Student'), 
('Fuse Attendee', '8th Grade Student', '8th Grade Student'), 
('Fuse Attendee', '9th Grade Student', '9th Grade Student'), 
('Fuse Attendee', 'Special Event Attendee', 'Special Event Attendee'), 
('Fuse Volunteer', 'Atrium', 'Atrium'), 
('Fuse Volunteer', 'Campus Safety', 'Campus Safety'), 
('Fuse Volunteer', 'Care', 'Care'), 
('Fuse Volunteer', 'Check-In', 'Check-In'), 
('Fuse Volunteer', 'Fuse Group Leader', 'Fuse Group Leader'), 
('Fuse Volunteer', 'Fuse Guest', 'Fuse Guest'), 
('Fuse Volunteer', 'Game Room', 'Game Room'), 
('Fuse Volunteer', 'Greeter', 'Greeter'), 
('Fuse Volunteer', 'VIP Team', 'VIP Team'), 
('Fuse Volunteer', 'Leadership Team', 'Leadership Team'), 
('Fuse Volunteer', 'Load In', 'Load In'), 
('Fuse Volunteer', 'Load Out', 'Load Out'), 
('Fuse Volunteer', 'Lounge', 'Lounge'), 
('Fuse Volunteer', 'New Serve', 'New Serve'), 
('Fuse Volunteer', 'Next Steps', 'Next Steps'), 
('Fuse Volunteer', 'Office Team', 'Office Team'), 
('Fuse Volunteer', 'Parking', 'Parking'), 
('Fuse Volunteer', 'Pick-Up', 'Pick-Up'), 
('Fuse Volunteer', 'Production', 'Production'), 
('Fuse Volunteer', 'Snack Bar', 'Snack Bar'), 
('Fuse Volunteer', 'Special Event Volunteer', 'Special Event Volunteer'), 
('Fuse Volunteer', 'Sports', 'Sports'), 
('Fuse Volunteer', 'Spring Zone', 'Spring Zone'), 
('Fuse Volunteer', 'Student Leader', 'Student Leader'), 
('Fuse Volunteer', 'Sunday Fuse Team', 'Sunday Fuse Team'), 
('Fuse Volunteer', 'Usher', 'Usher'), 
('Fuse Volunteer', 'VHQ', 'VHQ'), 
('Fuse Volunteer', 'Worship', 'Worship'), 
('Guest Services Attendee', 'VIP Room Attendee', 'VIP Room Attendee'), 
('Guest Services Attendee', 'Special Event Attendee', 'Special Event Attendee'), 
('Guest Services Attendee', 'Auditorium Reset Team', 'Auditorium Reset Team'), 
('Guest Services Attendee', 'Awake Team', 'Awake Team'), 
('Guest Services Attendee', 'Facility Cleaning Crew', 'Facility Cleaning Crew'), 
('Guest Services Attendee', 'Greeting Team', 'Greeting Team'), 
('Guest Services Attendee', 'Load In', 'Load In'), 
('Guest Services Attendee', 'Load Out', 'Load Out'), 
('Guest Services Attendee', 'Office Team', 'Office Team'), 
('Guest Services Attendee', 'Parking Team', 'Parking Team'), 
('Guest Services Attendee', 'VHQ Team', 'VHQ Team'), 
('Guest Services Volunteer', 'Area Leader', 'Area Leader'), 
('Guest Services Volunteer', 'Campus Safety', 'Campus Safety'), 
('Guest Services Volunteer', 'Facilities Volunteer', 'Facilities Volunteer'), 
('Guest Services Volunteer', 'Finance Team', 'Finance Team'), 
('Guest Services Volunteer', 'Hispanic Team', 'Hispanic Team'),
('Guest Services Volunteer', 'Network Fuse Team', 'Network Fuse Team'),
('Guest Services Volunteer', 'Network Office Team', 'Network Office Team'),
('Guest Services Volunteer', 'Network Sunday Team', 'Network Sunday Team'),
('Guest Services Volunteer', 'VIP Room Volunteer', 'VIP Room Volunteer'), 
('Guest Services Volunteer', 'Guest Services Team', 'Guest Services Team'), 
('Guest Services Volunteer', 'New Serve Team', 'New Serve Team'), 
('Guest Services Volunteer', 'Receptionist', 'Receptionist'), 
('Guest Services Volunteer', 'Service Leader', 'Service Leader'), 
('Guest Services Volunteer', 'Sign Language Team', 'Sign Language Team'), 
('Guest Services Volunteer', 'Special Event Volunteer', 'Special Event Volunteer'), 
('Guest Services Volunteer', 'Usher Team', 'Usher Team'), 
('Production Volunteer', 'Elementary Production', 'Elementary Production'), 
('Production Volunteer', 'Elementary Production Service Leader', 'Elementary Production Service Leader'), 
('Production Volunteer', 'Preschool Production', 'Preschool Production'), 
('Production Volunteer', 'Preschool Production Service Leader', 'Preschool Production Service Leader'), 
('Production Volunteer', 'Production Area Leader', 'Production Area Leader'), 
('Production Volunteer', 'Production Service Leader', 'Production Service Leader'), 
('Production Volunteer', 'KidSpring Production', 'KidSpring Production'), 
('Support Volunteer', 'Advocate', 'Advocate'), 
('Support Volunteer', 'Check-In Volunteer', 'Check-In Volunteer'), 
('Support Volunteer', 'First Time Team Volunteer', 'First Time Team Volunteer'), 
('Support Volunteer', 'KidSpring Greeter', 'KidSpring Greeter'), 
('Support Volunteer', 'Guest Services Service Leader', 'Guest Services Service Leader'), 
('Support Volunteer', 'Guest Services Area Leader', 'Guest Services Area Leader'), 
('Support Volunteer', 'KidSpring Office Team', 'KidSpring Office Team'), 
('Support Volunteer', 'Load In', 'Load In'),
('Support Volunteer', 'Load Out', 'Load Out'), 
('Support Volunteer', 'New Serve Area Leader', 'New Serve Area Leader'), 
('Support Volunteer', 'New Serve Team', 'New Serve Team'), 
('Support Volunteer', 'Sunday Support Volunteer', 'Sunday Support Volunteer'), 
('Next Steps Attendee', '90 DTC Participant', '90 DTC Participant'), 
('Next Steps Attendee', 'Andy Stanley Bible Study', 'Andy Stanley Bible Study'), 
('Next Steps Attendee', 'Baptism Attendee', 'Baptism Attendee'), 
('Next Steps Attendee', 'Beth Moore Bible Study', 'Beth Moore Bible Study'), 
('Next Steps Attendee', 'Budget Class Attendee', 'Budget Class Attendee'), 
('Next Steps Attendee', 'Connect Care Participant', 'Connect Care Participant'), 
('Next Steps Attendee', 'Connect Event Attendee', 'Connect Event Attendee'), 
('Next Steps Attendee', 'Connect Group Participant', 'Connect Group Participant'), 
('Next Steps Attendee', 'Creativity & Tech Basics', 'Creativity & Tech Basics'), 
('Next Steps Attendee', 'Creativity & Tech First Look', 'Creativity & Tech First Look'), 
('Next Steps Attendee', 'Creativity & Tech First Serve', 'Creativity & Tech First Serve'), 
('Next Steps Attendee', 'Financial Coaching Attendee', 'Financial Coaching Attendee'), 
('Next Steps Attendee', 'Financial Peace University', 'Financial Peace University'), 
('Next Steps Attendee', 'Fuse Basics', 'Fuse Basics'), 
('Next Steps Attendee', 'Fuse First Look', 'Fuse First Look'), 
('Next Steps Attendee', 'Fuse First Serve', 'Fuse First Serve'), 
('Next Steps Attendee', 'Guest Services Basics', 'Guest Services Basics'), 
('Next Steps Attendee', 'Guest Services First Look', 'Guest Services First Look'), 
('Next Steps Attendee', 'Guest Services First Serve', 'Guest Services First Serve'), 
('Next Steps Attendee', 'KidSpring Basics', 'KidSpring Basics'), 
('Next Steps Attendee', 'KidSpring First Look', 'KidSpring First Look'), 
('Next Steps Attendee', 'KidSpring First Serve', 'KidSpring First Serve'), 
('Next Steps Attendee', 'Load In', 'Load In'), 
('Next Steps Attendee', 'Load Out', 'Load Out'), 
('Next Steps Attendee', 'Next Steps Basics', 'Next Steps Basics'), 
('Next Steps Attendee', 'Next Steps First Look', 'Next Steps First Look'), 
('Next Steps Attendee', 'Next Steps First Serve', 'Next Steps First Serve'), 
('Next Steps Attendee', 'Opportunities Tour', 'Opportunities Tour'), 
('Next Steps Attendee', 'Ownership Class Attendee', 'Ownership Class Attendee'), 
('Next Steps Attendee', 'Ownership Class Current Owner', 'Ownership Class Current Owner'), 
('Next Steps Attendee', 'Special Event Attendee', 'Special Event Attendee'), 
('Next Steps Attendee', 'Welcome & Wanted Participant', 'Welcome & Wanted Participant'), 
('Next Steps Volunteer', 'Baptism Volunteer', 'Baptism Volunteer'), 
('Next Steps Volunteer', 'Budget Class Volunteer', 'Budget Class Volunteer'), 
('Next Steps Volunteer', 'Care Office Team', 'Care Office Team'), 
('Next Steps Volunteer', 'Care Visitation Team', 'Care Visitation Team'), 
('Next Steps Volunteer', 'Church Online Volunteer', 'Church Online Volunteer'), 
('Next Steps Volunteer', 'Events Office Team', 'Events Office Team'), 
('Next Steps Volunteer', 'Financial Coaching Volunteer', 'Financial Coaching Volunteer'), 
('Next Steps Volunteer', 'Financial Coaching Office Team', 'Financial Coaching Office Team'), 
('Next Steps Volunteer', 'Group Leader', 'Group Leader'), 
('Next Steps Volunteer', 'Groups Office Team', 'Groups Office Team'), 
('Next Steps Volunteer', 'New Serve Team', 'New Serve Team'), 
('Next Steps Volunteer', 'Next Steps Area', 'Next Steps Area'), 
('Next Steps Volunteer', 'Ownership Class Volunteer', 'Ownership Class Volunteer'), 
('Next Steps Volunteer', 'Prayer Team', 'Prayer Team'), 
('Next Steps Volunteer', 'Resource Center', 'Resource Center'), 
('Next Steps Volunteer', 'Special Event Volunteer', 'Special Event Volunteer'), 
('Next Steps Volunteer', 'Sunday Care Team', 'Sunday Care Team'), 
('Next Steps Volunteer', 'Writing Team', 'Writing Team'), 
('Nursery Volunteer', 'Nursery Early Bird Volunteer', 'Nursery Early Bird Volunteer'), 
('Nursery Volunteer', 'Wonder Way Service Leader', 'Wonder Way Service Leader'), 
('Nursery Volunteer', 'Wonder Way Area Leader', 'Wonder Way Area Leader'), 
('Nursery Volunteer', 'Wonder Way 1 Volunteer', 'Wonder Way 1 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 2 Volunteer', 'Wonder Way 2 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 3 Volunteer', 'Wonder Way 3 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 4 Volunteer', 'Wonder Way 4 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 5 Volunteer', 'Wonder Way 5 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 6 Volunteer', 'Wonder Way 6 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 7 Volunteer', 'Wonder Way 7 Volunteer'), 
('Nursery Volunteer', 'Wonder Way 8 Volunteer', 'Wonder Way 8 Volunteer'), 
('Preschool Volunteer', 'Base Camp Jr. Volunteer', 'Base Camp Jr. Volunteer'), 
('Preschool Volunteer', 'Fire Station Volunteer', 'Fire Station Volunteer'), 
('Preschool Volunteer', 'Lil'' Spring Volunteer', 'Lil'' Spring Volunteer'), 
('Preschool Volunteer', 'Toys Volunteer', 'Toys Volunteer'), 
('Preschool Volunteer', 'Pop''s Garage Volunteer', 'Pop''s Garage Volunteer'),
('Preschool Volunteer', 'Preschool Early Bird Volunteer', 'Preschool Early Bird Volunteer'), 
('Preschool Volunteer', 'Preschool Service Leader', 'Preschool Service Leader'), 
('Preschool Volunteer', 'Preschool Area Leader', 'Preschool Area Leader'), 
('Preschool Volunteer', 'Spring Fresh Volunteer', 'Spring Fresh Volunteer'), 
('Preschool Volunteer', 'Toys Volunteer', 'Toys Volunteer'), 
('Preschool Volunteer', 'Treehouse Volunteer', 'Treehouse Volunteer'), 
('Special Needs Volunteer', 'Spring Zone Jr. Volunteer', 'Spring Zone Jr. Volunteer'), 
('Special Needs Volunteer', 'Spring Zone Service Leader', 'Spring Zone Service Leader'), 
('Special Needs Volunteer', 'Spring Zone Area Leader', 'Spring Zone Area Leader'), 
('Special Needs Volunteer', 'Spring Zone Volunteer', 'Spring Zone Volunteer')

/* ====================================================== */
-- central group structure
/* ====================================================== */
IF object_id('tempdb..#centralGroups') IS NOT NULL
BEGIN
	drop table #centralGroups
END
create table #centralGroups (
	ID int IDENTITY(1,1),
	groupTypeName nvarchar(255),
	groupName nvarchar(255),
	locationName nvarchar(255),
)

-- GroupType, Group, Location
INSERT #centralGroups
VALUES
('Creativity & Tech Volunteer', 'Design Team', 'Design Team'), 
('Creativity & Tech Volunteer', 'IT Team', 'IT Team'), 
('Creativity & Tech Volunteer', 'NewSpring Store Team', 'NewSpring Store Team'), 
('Creativity & Tech Volunteer', 'Social Media/PR Team', 'Social Media/PR Team'), 
('Creativity & Tech Volunteer', 'Video Production Team', 'Video Production Team'), 
('Creativity & Tech Volunteer', 'Web Dev Team', 'Web Dev Team'), 
('Event Attendee', 'Event Attendee', 'Event Attendee'), 
('Event Volunteer', 'Event Volunteer', 'Event Volunteer'),
('Fuse Volunteer', 'Fuse Office Team', 'Fuse Office Team'), 
('Fuse Volunteer', 'Special Event Attendee', 'Special Event Attendee'), 
('Fuse Volunteer', 'Special Event Volunteer', 'Special Event Volunteer'), 
('Guest Services Volunteer', 'Events Team', 'Events Team'), 
('Guest Services Volunteer', 'Finance Office Team', 'Finance Office Team'), 
('Guest Services Volunteer', 'GS Office Team', 'GS Office Team'), 
('Guest Services Volunteer', 'Receptionist', 'Receptionist'), 
('Guest Services Volunteer', 'Special Event Attendee', 'Special Event Attendee'), 
('Guest Services Volunteer', 'Special Event Volunteer', 'Special Event Volunteer'), 
('Support Volunteer', 'Office Team', 'Office Team'), 
('Next Steps Volunteer', 'Groups Office Team', 'Groups Office Team'), 
('Next Steps Volunteer', 'NS Office Team', 'NS Office Team'), 
('Next Steps Volunteer', 'Writing Team', 'Writing Team')

/* ====================================================== */
-- college grouptype
/* ====================================================== */
DECLARE @collegeArea nvarchar(255) = 'NewSpring College', 
	@collegeLocation nvarchar(255) = 'Class Attendee',
	@collegeAttendance int = 2, @collegeInheritedType int = NULL

/* ====================================================== */
-- college group structure
/* ====================================================== */
IF object_id('tempdb..#collegeGroups') IS NOT NULL
BEGIN
	drop table #collegeGroups
END
create table #collegeGroups (
	ID int IDENTITY(1,1),
	groupTypeName nvarchar(255),
	groupName nvarchar(255),
	locationName nvarchar(255),
)

-- GroupType, Group, Location
INSERT #collegeGroups
VALUES
('NewSpring College', 'All-Staff I', 'All-Staff I'), 
('NewSpring College', 'All-Staff II', 'All-Staff II'), 
('NewSpring College', 'All-Staff III', 'All-Staff III'),
('NewSpring College', 'All-Staff IV', 'All-Staff IV'),
('NewSpring College', 'Audio I', 'Audio I'), 
('NewSpring College', 'Audio II', 'Audio II'), 
('NewSpring College', 'Audio Lab I', 'Audio Lab I'), 
('NewSpring College', 'Audio Lab II', 'Audio Lab II'), 
('NewSpring College', 'Basic Christian Issues', 'Basic Christian Issues'), 
('NewSpring College', 'Children''s Ministry I', 'Children''s Ministry I'), 
('NewSpring College', 'Children''s Ministry II', 'Children''s Ministry II'), 
('NewSpring College', 'Forum I', 'Forum I'), 
('NewSpring College', 'Forum II', 'Forum II'), 
('NewSpring College', 'Health in Ministry I', 'Health in Ministry I'), 
('NewSpring College', 'Health in Ministry II', 'Health in Ministry II'), 
('NewSpring College', 'Leadership I', 'Leadership I'), 
('NewSpring College', 'Leadership II', 'Leadership II'), 
('NewSpring College', 'Ministry Leadership Lab', 'Ministry Leadership Lab'), 
('NewSpring College', 'Set Up Group', 'Set Up Group'), 
('NewSpring College', 'Student Ministry Foundations I', 'Student Ministry Foundations I'), 
('NewSpring College', 'Student Ministry Foundations II', 'Student Ministry Foundations II'), 
('NewSpring College', 'Student Ministry Lead Lab I', 'Student Ministry Lead Lab I'), 
('NewSpring College', 'Student Ministry Lead Lab II', 'Student Ministry Lead Lab II'), 
('NewSpring College', 'Survey of the Bible I', 'Survey of the Bible I'),
('NewSpring College', 'Survey of the Bible II', 'Survey of the Bible II'),
('NewSpring College', 'Theology of Worship', 'Theology of Worship')

/* ====================================================== */
-- DELETE existing areas
/* ====================================================== */

DELETE FROM location
WHERE id in (
	SELECT distinct locationId
	FROM grouplocation gl
	INNER JOIN [group] g
	ON gl.groupid = g.id
	AND g.GroupTypeId in (14, 18, 19, 20, 21, 22, 24, 26)
)

DELETE FROM GroupTypeAssociation
WHERE GroupTypeId in (14, 18, 19, 20, 21, 22, 24, 26)
or ChildGroupTypeId in (14, 18, 19, 20, 21, 22, 24, 26)

DELETE FROM [Group]
WHERE GroupTypeId in (14, 18, 19, 20, 21, 22, 24, 26)

DELETE FROM GroupType
WHERE id in (14, 18, 19, 20, 21, 22, 24, 26)

/* ====================================================== */
-- setup check-in config areas
/* ====================================================== */

DECLARE @AreaName nvarchar(255), @AreaId int, @AreaLocation nvarchar(255), @AreaLocationId int, 
	@ParentAreaName nvarchar(255), @ParentAreaId int, @DefaultRoleId int, @AttendanceRule int, 
	@AreaGroupId int, @InheritedTypeId int

DECLARE @scopeIndex int, @numItems int		
SELECT @scopeIndex = min(Id) FROM #topAreas
SELECT @numItems = count(1) + @scopeIndex FROM #topAreas

WHILE @scopeIndex <= @numItems
BEGIN
	
	SELECT @ParentAreaName = '', @AreaName = '', @InheritedTypeId = NULL, @ParentAreaId = NULL, 
		@AreaId = NULL, @AreaLocation = '', @AreaLocationId = NULL, @AreaGroupId = NULL, @DefaultRoleId = NULL,
		@AttendanceRule = 0, @AreaGroupId = NULL, @InheritedTypeId = NULL
	SELECT @ParentAreaName = parentArea, @AreaName = childArea, @InheritedTypeId = inheritedType
	FROM #topAreas WHERE id = @scopeIndex

	IF @AreaName <> ''
	BEGIN

		SELECT @msg = 'Creating ' + @ParentAreaName + ' / ' + @AreaName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

		/* ====================================================== */
		-- create the parent if it doesn't exist
		/* ====================================================== */
		SELECT @ParentAreaId = [Id] FROM GroupType
		WHERE name = @ParentAreaName AND IsSystem = @IsSystem

		IF @ParentAreaId IS NULL
		BEGIN

			INSERT GroupType (IsSystem, Name, [Description], GroupTerm, GroupMemberTerm, AllowMultipleLocations, 
				ShowInGroupList, ShowInNavigation, TakesAttendance, AttendanceRule, AttendancePrintTo,
				[Order], InheritedGroupTypeId, LocationSelectionMode, GroupTypePurposeValueId, [Guid],
				AllowedScheduleTypes, SendAttendanceReminder)
			SELECT @IsSystem, @ParentAreaName, @ParentAreaName + ' Area', 'Group', 'Member', @False, @True, @False, 
				@False, 0, 0, 0, NULL, 0, @CheckInAreaPurposeId, NEWID(), 0, @False

			SELECT @ParentAreaId = SCOPE_IDENTITY()

			-- allow children of this grouptype
			INSERT GroupTypeAssociation
			VALUES (@ParentAreaId, @ParentAreaId)

			/* ====================================================== */
			-- set default grouptype role
			/* ====================================================== */
			INSERT GroupTypeRole (isSystem, GroupTypeId, Name, [Order], IsLeader,
				[Guid], CanView, CanEdit)
			VALUES (@IsSystem, @ParentAreaId, 'Member', 0, 0, NEWID(), 0, 0)

			SELECT @DefaultRoleId = SCOPE_IDENTITY()

			UPDATE GroupType SET DefaultGroupRoleId = @DefaultRoleId WHERE Id = @ParentAreaId
		END

		/* ====================================================== */
		-- create the child grouptype
		/* ====================================================== */
		INSERT GroupType (IsSystem, Name, [Description], GroupTerm, GroupMemberTerm, AllowMultipleLocations, 
			ShowInGroupList, ShowInNavigation, TakesAttendance, AttendanceRule, AttendancePrintTo,
			[Order], InheritedGroupTypeId, LocationSelectionMode, GroupTypePurposeValueId, [Guid],
			AllowedScheduleTypes, SendAttendanceReminder)
		SELECT @IsSystem, @AreaName, @AreaName + ' GroupType', 'Group', 'Member', @True, @True, @True, 
			@True, 0, 0, 0, @InheritedTypeId, 0, NULL, NEWID(), 0, @False

		SELECT @AreaId = SCOPE_IDENTITY()

		-- SET parent association
		INSERT GroupTypeAssociation
		VALUES (@ParentAreaId, @AreaId)

		-- allow children of this grouptype
		INSERT GroupTypeAssociation
		VALUES (@AreaId, @AreaId)

		/* ====================================================== */
		-- set default grouptype role
		/* ====================================================== */
		INSERT GroupTypeRole (isSystem, GroupTypeId, Name, [Order], IsLeader,
			[Guid], CanView, CanEdit)
		VALUES (@IsSystem, @AreaId, 'Member', 0, 0, NEWID(), 0, 0)

		SELECT @DefaultRoleId = SCOPE_IDENTITY()

		UPDATE GroupType SET DefaultGroupRoleId = @DefaultRoleId WHERE id = @AreaId

		/* ====================================================== */
		-- create matching group
		/* ====================================================== */
		INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, Name,
			[Description], IsSecurityRole, IsActive, [Order], [Guid], [IsPublic])
		SELECT @IsSystem, NULL, @AreaId, @AreaName, @AreaName + ' Group', @False, @True, @scopeIndex, NEWID(), @True

		/* ====================================================== */
		-- create group locations under campus locations
		/* ====================================================== */
		SELECT @AreaLocationId = [Id] FROM Location
		WHERE name = @ParentAreaName AND IsActive = @True
		
		IF @AreaLocationId IS NULL
		BEGIN

			INSERT Location (ParentLocationId, Name, IsActive, [Guid])
			SELECT c.LocationId, @ParentAreaName, 1, NEWID()
			FROM Campus c
			WHERE c.IsActive = @True
		END

	END
	-- end empty area name

	SELECT @scopeIndex = @scopeIndex + 1
END
-- end check-in config areas


/* ====================================================== */
-- insert campus groups
/* ====================================================== */
DECLARE @GroupTypeName nvarchar(255), @GroupTypeId int, @GroupTypeGroupId int,
	@GroupName nvarchar(255), @GroupId int, @LocationName nvarchar(255), @LocationId int,
	@ParentGroupTypeName nvarchar(255), @ParentGroupTypeId int, @ParentLocationId int, @GroupLocationId int

DECLARE @campusGroupId int, @numCampusGroups int
SELECT @campusGroupId = min(Id) FROM #campusGroups
SELECT @numCampusGroups = count(1) + @campusGroupId FROM #campusGroups

WHILE @campusGroupId < @numCampusGroups
BEGIN
	
	SELECT @GroupTypeName = '', @GroupTypeId = NULL, @GroupTypeGroupId = NULL, @LocationName = '',
		@GroupName = '', @GroupId = NULL, @LocationName = '', @ParentGroupTypeName = '', 
		@LocationId = NULL, @ParentGroupTypeId = NULL, @ParentLocationId = NULL, @GroupLocationId = NULL

	SELECT @GroupTypeName = groupTypeName, @GroupName = groupName, @LocationName = locationName
	FROM #campusGroups
	WHERE Id = @campusGroupId

	/* ====================================================== */
	-- get parent grouptype
	/* ====================================================== */
	SELECT @GroupTypeId = gt.[Id], @GroupTypeGroupId = g.[Id]
	FROM GroupType gt
	INNER JOIN [Group] g
	ON gt.id = g.GroupTypeId
	AND gt.name = @GroupTypeName
	AND g.name = @GroupTypeName
	
	IF @GroupTypeId IS NOT NULL
	BEGIN
		
		SELECT @msg = 'Creating ' + @GroupTypeName + ' / ' + @GroupName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

		/* ====================================================== */
		-- create child group if it doesn't exist
		/* ====================================================== */
		SELECT @GroupId = [Id] FROM [Group]
		WHERE GroupTypeId = @GroupTypeId
		AND name = @GroupName
				
		IF @GroupId IS NULL
		BEGIN

			INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, Name,
				[Description], IsSecurityRole, IsActive, [Order], [Guid], [IsPublic])
			SELECT @IsSystem, @GroupTypeGroupId, @GroupTypeId, @GroupName, @GroupName + ' Group', @False, @True, @Order, NEWID(), @True

			SELECT @GroupId = SCOPE_IDENTITY()
			SELECT @Order = @Order + 1
		END

		/* ====================================================== */
		-- get parent grouptype name
		/* ====================================================== */
		SELECT @ParentGroupTypeName = gt.[Name], @ParentGroupTypeId = gt.[Id]
		FROM GroupType gt
		INNER JOIN GroupTypeAssociation gta
		ON gt.Id = gta.GroupTypeId
		AND gta.ChildGroupTypeId = @GroupTypeId
		AND gta.GroupTypeId <> @GroupTypeId

		/* ====================================================== */
		-- create campus level locations if they don't exist
		/* ====================================================== */

		SELECT @LocationId = l.[Id]
		FROM Location l
		INNER JOIN Location l2
		ON l.ParentLocationId = l2.Id
		WHERE l.name = @LocationName 
		AND l2.name = @ParentGroupTypeName

		IF @LocationId IS NULL AND @LocationName <> ''
		BEGIN

			INSERT Location (ParentLocationId, Name, IsActive, [Guid])
			SELECT l.Id, @LocationName, 1, NEWID()
			FROM Location l
			INNER JOIN Campus c
			ON c.LocationId = l.ParentLocationId
			AND l.Name = @ParentGroupTypeName
			WHERE c.IsActive = @True
		END

		/* ====================================================== */
		-- NOTE: either the group or the location was just created,
		-- so a groupLocation needs to be created
		/* ====================================================== */
		INSERT GroupLocation (Groupid, LocationId, IsMailingLocation, IsMappedLocation, [Guid])
		SELECT @GroupId, l.Id, @False, @False, NEWID()
		FROM Location l
		INNER JOIN Location pl
		ON l.ParentLocationId = pl.Id
		AND pl.Name = @ParentGroupTypeName
		AND l.name = @LocationName

	END
	-- end grouptype not empty

	SELECT @campusGroupId = @campusGroupid + 1
END
-- end campus groups

/* ====================================================== */
-- Add IsSpecialNeeds attribute value to spring zone groups
/* ====================================================== */

INSERT [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Value] ,[Guid] ) 
SELECT 0, @SpecialNeedsAttributeId, [Id], 'True', NEWID()
FROM [Group] WHERE Name = 'Spring Zone' or Name = 'Spring Zone Jr.'

/* ====================================================== */
-- Add Central hierarchy separately from campus groups
/* ====================================================== */

RAISERROR ( 'Starting Central grouptypes & groups', 0, 0 ) WITH NOWAIT

DECLARE @CampusName nvarchar(255), @CampusCode nvarchar(255), @CampusId int = 0, @CampusLocationId int = 0
SELECT @CampusName = 'Central', @CampusCode = 'CEN', @Order = 0

SELECT @CampusId = [Id] FROM Campus
WHERE Name = @CampusName
AND ShortCode = @CampusCode

SELECT @CampusLocationId = [Id] FROM Location
WHERE name = @CampusName 
AND LocationTypeValueId = @CampusLocationTypeId

/* ====================================================== */
-- create central grouptypes
/* ====================================================== */
SELECT @scopeIndex = min(Id) FROM #centralGroups
SELECT @numItems = @scopeIndex + count(1) FROM #centralGroups

WHILE @scopeIndex < @numItems
BEGIN
	
	SELECT @GroupTypeName = '', @GroupTypeId = NULL, @GroupTypeGroupId = NULL, @LocationName = '',
		@GroupName = '', @GroupId = NULL, @LocationName = '', @ParentGroupTypeName = '', 
		@LocationId = NULL, @ParentGroupTypeId = NULL, @ParentLocationId = NULL

	SELECT @GroupTypeName = groupTypeName, @GroupName = groupName, @LocationName = locationName
	FROM #centralGroups
	WHERE Id = @scopeIndex

	IF @GroupName <> ''
	BEGIN

		/* ====================================================== */
		-- lookup parent grouptype
		/* ====================================================== */
		SELECT @GroupTypeId = gt.[Id], @GroupTypeGroupId = g.[Id]
		FROM GroupType gt
		INNER JOIN [Group] g
		ON gt.id = g.GroupTypeId
		AND gt.name = @GroupTypeName
		AND g.name = @GroupTypeName

		SELECT @msg = 'Creating ' + @GroupTypeName + ' / ' + @GroupName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
		
		/* ====================================================== */
		-- create central group if it doesn't exist
		/* ====================================================== */		
		SELECT @GroupId = [Id] FROM [Group]
		WHERE name = @GroupName
		AND GroupTypeId = @GroupTypeId
		
		IF @GroupId IS NULL
		BEGIN
		
			INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, Name, [Description], IsSecurityRole, IsActive, [Order], [Guid])
			SELECT @IsSystem, @GroupTypeGroupId, @GroupTypeId, @GroupName, @GroupName + ' Group', 0, 1, @scopeIndex, NEWID()

			SELECT @GroupId = SCOPE_IDENTITY()
		END

		/* ====================================================== */
		-- get parent grouptype name
		/* ====================================================== */
		SELECT @ParentGroupTypeName = gt.[Name], @ParentGroupTypeId = gt.[Id]
		FROM GroupType gt
		INNER JOIN GroupTypeAssociation gta
		ON gt.Id = gta.GroupTypeId
		AND gta.ChildGroupTypeId = @GroupTypeId
		AND gta.GroupTypeId <> @GroupTypeId

		/* ====================================================== */
		-- INSERT central locations
		/* ====================================================== */
		SELECT @ParentLocationId = [Id] FROM Location
		WHERE Name = @ParentGroupTypeName
		AND ParentLocationId = @CampusLocationId

		IF @ParentLocationId IS NULL
		BEGIN
			INSERT Location (ParentLocationId, Name, IsActive, [Guid])
			SELECT @CampusLocationId, @ParentGroupTypeName, @True, NEWID()

			SELECT @ParentLocationId = SCOPE_IDENTITY()
		END

		SELECT @LocationId = [Id] FROM Location
		WHERE Name = @LocationName
		AND ParentLocationId = @ParentLocationId

		IF @LocationId IS NULL
		BEGIN
			INSERT Location (ParentLocationId, Name, IsActive, [Guid])
			SELECT @ParentLocationId, @LocationName, @True, NEWID()

			SELECT @LocationId = SCOPE_IDENTITY()
		END

		/* ====================================================== */
		-- insert group location
		/* ====================================================== */
		INSERT GroupLocation (Groupid, LocationId, IsMailingLocation, IsMappedLocation, [Guid])
		SELECT @GroupId, @LocationId, @False, @False, NEWID()
			
	END
	-- end name not empty	

	SET @scopeIndex = @scopeIndex + 1
END
-- end central

/* ====================================================== */
-- Add NewSpring College hierarchy
/* ====================================================== */

-- NOTE: @CampusCode, @CampusLocationId, @CampusId already SET to Central

/* ====================================================== */
-- create college check-in area
/* ====================================================== */
INSERT grouptype (IsSystem, Name, [Description], GroupTerm, GroupMemberTerm, AllowMultipleLocations, 
	ShowInGroupList, ShowInNavigation, TakesAttendance, AttendanceRule, AttendancePrintTo,
	[Order], InheritedGroupTypeId, LocationSelectionMode, GroupTypePurposeValueId, [Guid],
	AllowedScheduleTypes, SendAttendanceReminder)
SELECT @IsSystem, @CollegeName, @CollegeName + ' Area', 'Group', 'Member', @False, @False, @False, 
	@False, 0, 0, 0, NULL, 0, @CheckInAreaPurposeId, NEWID(), 0, @False

SELECT @ParentAreaId = SCOPE_IDENTITY()

INSERT GroupTypeRole (isSystem, GroupTypeId, Name, [Order], IsLeader, [Guid], CanView, CanEdit)
VALUES (@IsSystem, @ParentAreaId, 'Member', 0, @False, NEWID(), @False, @False)

SELECT @DefaultRoleId = SCOPE_IDENTITY()

UPDATE grouptype
SET DefaultGroupRoleId = @DefaultRoleId
WHERE id = @ParentAreaId

/* ====================================================== */
-- class attendee grouptype
/* ====================================================== */
INSERT grouptype (IsSystem, Name, [Description], GroupTerm, GroupMemberTerm, AllowMultipleLocations, 
	ShowInGroupList, ShowInNavigation, TakesAttendance, AttendanceRule, AttendancePrintTo,
	[Order], InheritedGroupTypeId, LocationSelectionMode, GroupTypePurposeValueId, [Guid],
	AllowedScheduleTypes, SendAttendanceReminder)
SELECT @IsSystem, @CollegeName, @CollegeName + ' GroupType', 'Group', 'Member', @True, @True, @True, 
	@True, 0, 0, 0, NULL, 0, NULL, NEWID(), 0, @False

SELECT @AreaId = SCOPE_IDENTITY()

INSERT GroupTypeRole (isSystem, GroupTypeId, Name, [Order], IsLeader, [Guid], CanView, CanEdit)
VALUES (@IsSystem, @AreaId, 'Member', 0, @False, NEWID(), @False, @False)

SELECT @DefaultRoleId = SCOPE_IDENTITY()

UPDATE grouptype
SET DefaultGroupRoleId = @DefaultRoleId
WHERE id = @AreaId

/* ====================================================== */
-- SET grouptype associations
/* ====================================================== */
INSERT GroupTypeAssociation
VALUES (@ParentAreaId, @AreaId)

INSERT GroupTypeAssociation
VALUES (@AreaId, @AreaId)

/* ====================================================== */
-- create child group IF it doesn't exist
/* ====================================================== */
SELECT @AreaGroupId = [Id] FROM [Group]
WHERE GroupTypeId = @AreaId
AND name = @CollegeName
				
IF @AreaGroupId IS NULL
BEGIN

	INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, Name,
		[Description], IsSecurityRole, IsActive, [Order], [Guid], [IsPublic])
	SELECT @IsSystem, NULL, @AreaId, @CollegeName, @CollegeName, @False, @True, 0, NEWID(), @True

	SELECT @AreaGroupId = SCOPE_IDENTITY()
END

/* ====================================================== */
-- create top-level college location
/* ====================================================== */
INSERT Location (ParentLocationId, Name, IsActive, [Guid])
SELECT @CampusLocationId, @CollegeName, @True, NEWID()

SET @ParentLocationId = SCOPE_IDENTITY()

/* ====================================================== */
-- create college groups
/* ====================================================== */
SELECT @scopeIndex = 0, @numItems = 0, @Order = 0
SELECT @scopeIndex = min(Id) FROM #collegeGroups
SELECT @numItems = @scopeIndex + count(1) FROM #collegeGroups
		
WHILE @scopeIndex < @numItems
BEGIN
	SELECT @GroupTypeName = '', @GroupName = '', @LocationName = '', @GroupId = NULL
	SELECT @GroupTypeName = groupTypeName, @GroupName = groupName, @LocationName = locationName
	FROM #collegeGroups WHERE id = @scopeIndex

	IF @GroupName <> ''
	BEGIN

		SELECT @msg = 'Creating ' + @GroupTypeName + ' / ' + @GroupName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

		/* ====================================================== */
		-- create child group
		/* ====================================================== */
		INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, Name, [Description], IsSecurityRole, IsActive, [Order], [Guid])
		SELECT @IsSystem, @AreaGroupId, @AreaId, @GroupName, @GroupName + ' Group', @False, @True, @scopeIndex, NEWID()

		SELECT @GroupId = SCOPE_IDENTITY()

		INSERT Location (ParentLocationId, Name, IsActive, [Guid])
		SELECT @ParentLocationId, @LocationName, @True, NEWID()

		SELECT @LocationId = SCOPE_IDENTITY()

		INSERT GroupLocation (Groupid, LocationId, IsMailingLocation, IsMappedLocation, [Guid])
		SELECT @GroupId, @LocationId, @False, @False, NEWID()
	END	

	SET @scopeIndex = @scopeIndex + 1
END
-- end college		

use master

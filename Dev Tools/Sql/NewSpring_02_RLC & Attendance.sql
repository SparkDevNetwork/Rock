/* ====================================================== 
-- NewSpring Script #2: 
-- Inserts attendances, assignments, jobs, and group schedules.
  
--  Assumptions:
--  We only import attendances with valid RLC's
--  We only import active assignments with valid RLC's
--  We only import schedules that match current service times

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

/* ====================================================== */

-- Enable production mode for performance
SET NOCOUNT ON

-- Set the F1 database name
DECLARE @F1 nvarchar(255) = 'F1'

/* ====================================================== */
-- Start value lookups
/* ====================================================== */
declare @IsSystem int = 0, @Order int = 0,  @TextFieldTypeId int = 1, @True int = 1, @False int = 0
declare @ScheduleDefinedTypeId decimal, @GroupCategoryId bigint, @ChildCategoryId bigint, @RLCID bigint, 
	@NameSearchValueId bigint, @PersonId bigint, @GroupTypeId bigint, @GroupId bigint, @LocationId bigint, 
	@CampusId bigint, @GroupMemberEntityId bigint, @PersonEntityTypeId bigint, @DefinedValueFieldTypeId bigint, 
	@ScheduleAttributeId bigint, @BreakoutGroupAttributeId bigint, @TeamConnectorTypeId bigint, 
	@TeamConnectorAttributeId bigint, @CampusFieldTypeId bigint, @CampusAttributeId bigint, @FuseScheduleId int

select @GroupCategoryId = Id from Category where [Guid] = '56B3C72A-6CE7-4CAE-8105-9F16EE772530'
select @ChildCategoryId = Id from Category where [Guid] = '752DC692-836E-4A3E-B670-4325CD7724BF'
select @GroupMemberEntityId = Id from EntityType where [Guid] = '49668B95-FEDC-43DD-8085-D2B0D6343C48'
select @PersonEntityTypeId = Id from EntityType where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7' 
select @DefinedValueFieldTypeId = Id from FieldType where [Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7'
select @CampusFieldTypeId = Id from FieldType where [Guid] = '1B71FEF4-201F-4D53-8C60-2DF21F1985ED'
select @NameSearchValueId = Id from DefinedValue where [Guid] = '071D6DAA-3063-463A-B8A1-7D9A1BE1BB31'
select @ScheduleDefinedTypeId = Id from DefinedType where [Guid] = '26ECDD90-A2FA-4732-B3D1-32AC93953EFA'
select @BreakoutGroupAttributeId = Id from Attribute where [Guid] = 'BF976365-01E7-4C98-9BE2-4D5B78EDBF48'
select @TeamConnectorTypeId = Id from DefinedType where [Guid] = '418C4656-6A85-4642-B8BA-BEEB1A0FF869'

/* ====================================================== */
-- Create indexes on F1 tables for speedier lookups
/* ====================================================== */
if not exists (select top 1 object_id from F1.sys.indexes where name = 'IX_Assignments' )
begin
	CREATE INDEX IX_Assignments ON F1..Staffing_Assignment (Individual_ID, RLC_ID, JobID, Is_Active, Staffing_Schedule_Name)
end

if not exists (select top 1 object_id from F1.sys.indexes where name = 'IX_Attendance' )
begin
	CREATE INDEX IX_Attendance ON F1..Attendance (Individual_ID, RLC_ID, Start_Date_Time, Check_In_Time)
end

/* ====================================================== */
-- Create person Breakout Group attribute for attendees
/* ====================================================== */
if @BreakoutGroupAttributeId is null
begin
	insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [AllowSearch], [Guid] )
	select @IsSystem, @TextFieldTypeId, @PersonEntityTypeId, '', '', 'BreakoutGroup', 'Breakout Group', 'The normal KidSpring small group that this person goes to.',
		@Order, @True, @False, @False, @True, 'BF976365-01E7-4C98-9BE2-4D5B78EDBF48'

	select @BreakoutGroupAttributeId = SCOPE_IDENTITY()

	insert AttributeCategory
	select @BreakoutGroupAttributeId, @ChildCategoryId
end

/* ====================================================== */
-- Create team connector defined type
/* ====================================================== */
if @TeamConnectorTypeId is null
begin
	insert DefinedType ( [IsSystem], [FieldTypeId], [Order], [Name], [Description], [Guid], CategoryId )
	select @IsSystem, @TextFieldTypeId, @Order, 'Team Connector', 'The team connector for this group member.',
		'418C4656-6A85-4642-B8BA-BEEB1A0FF869', @GroupCategoryId

	select @TeamConnectorTypeId = SCOPE_IDENTITY()
end

-- Insert 25 team connectors into the defined type
if not exists (select Id from DefinedValue where DefinedTypeId = @TeamConnectorTypeId )
begin 
	;with teamConnectorValues as (
		(
			SELECT 1 AS teamNumber
			UNION ALL
			SELECT teamNumber + 1 AS teamNumber
			FROM teamConnectorValues
			WHERE teamConnectorValues.teamNumber < 25
		)
	)
	insert DefinedValue ([IsSystem], [DefinedTypeId], [Order], [Value], [Guid] )
	select @IsSystem, @TeamConnectorTypeId, @Order, 'TC ' + convert(varchar(10), teamNumber), NEWID()
	from teamConnectorValues	
end

/* ====================================================== */
-- Create services lookup
/* ====================================================== */
if object_id('tempdb..#services') is not null
begin
	drop table #services
end
create table #services (
	serviceId int,
	serviceName nvarchar(255),
	serviceTime time
)

insert into #services
select Id, Name, STUFF(SUBSTRING(iCalendarContent, PATINDEX('%DTSTART%', iCalendarContent) +17, 4), 3, 0, ':')
from schedule

select @FuseScheduleId = serviceId 
from #services
where serviceName like 'Fuse%'

/* ====================================================== */
-- Create schedule defined type
/* ====================================================== */
if @ScheduleDefinedTypeId is null
begin
	insert DefinedType ( [IsSystem], [FieldTypeId], [Order], [Name], [Description], [Guid], CategoryId )
	select @IsSystem, @TextFieldTypeId, @Order, 'Schedules', 'The schedules that can be assigned to a group member.',
		'26ECDD90-A2FA-4732-B3D1-32AC93953EFA', @GroupCategoryId

	select @ScheduleDefinedTypeId = SCOPE_IDENTITY()
end

/* ====================================================== */
-- Create schedule lookup
/* ====================================================== */
if object_id('tempdb..#schedules') is not null
begin
	drop table #schedules
end
create table #schedules (
	scheduleF1 nvarchar(255),
	scheduleRock nvarchar(255),
	dvGuid uniqueidentifier DEFAULT NULL
)

insert into #schedules (scheduleF1, scheduleRock)
select distinct Staffing_Schedule_Name, case 
	when Staffing_Schedule_Name like '8:30%' 
		then null
	when Staffing_Schedule_Name = '9:15' 
		or Staffing_Schedule_Name = '9:15 Schedule' 
		then '9:15 AM'
	when Staffing_Schedule_Name = '9:15 A' 
		then '9:15 A Shift'
	when Staffing_Schedule_Name like '10:00%' 
		then null
	when Staffing_Schedule_Name = '11:15'
		or Staffing_Schedule_Name = '11:15 Schedule' 
		then '11:15 AM'
	when Staffing_Schedule_Name = '11:15 A' 
		then '11:15 A Shift'
	when Staffing_Schedule_Name like '11:30%' 
		then null
	when Staffing_Schedule_Name = '4:00' 
		or Staffing_Schedule_Name = '4:15' 
		then '4:00 PM'
	when Staffing_Schedule_Name = '6:00'
		then '6:00 PM'
	when Staffing_Schedule_Name = 'Base Schedule'
		then null
	when Staffing_Schedule_Name like 'Every Other%'
		then null
	else Staffing_Schedule_Name
	end as 'schedule'
from OPENQUERY( [SERVER], 'SELECT * FROM F1.dbo.Staffing_Assignment')

-- Remove any old schedules
delete from DefinedValue where DefinedTypeId = @ScheduleDefinedTypeId

-- Create defined values for all the schedules
;with distinctSchedules as (
	select distinct scheduleRock 
	from #schedules
)
insert DefinedValue ([IsSystem], [DefinedTypeId], [Order], [Value], [Guid] )
select @IsSystem, @ScheduleDefinedTypeId, @Order, s.scheduleRock, NEWID()
from distinctSchedules s
where scheduleRock is not null

update s
set dvGuid = dv.[Guid]
from #schedules s
inner join DefinedValue dv
on dv.DefinedTypeId = @ScheduleDefinedTypeId
and dv.Value = s.scheduleRock

/* ====================================================== */
-- Create assignments lookup 
/* ====================================================== */
if object_id('tempdb..#assignments') is not null
begin
	drop table #assignments
end
create table #assignments (
	ID int IDENTITY(1,1) NOT NULL,
	JobID bigint,
	JobTitle nvarchar(255),
	PersonID bigint,
	ScheduleName nvarchar(255),
	BreakoutGroup nvarchar(255),
	IsBreakoutTag bit
)

/* ====================================================== */
-- Create RLC lookup
/* ====================================================== */
if object_id('tempdb..#rlcMap') is not null
begin
	drop table #rlcMap
end
create table #rlcMap (
	ID int IDENTITY(1,1),
	RLC_ID bigint,
	Code nvarchar(3),
	GroupType nvarchar(255),
	GroupName nvarchar(255),
	LocationName nvarchar(255) DEFAULT NULL
)

insert #rlcMap 
values
(1443499, 'AKN', 'Elementary Attendee', 'Base Camp', NULL),
(1443500, 'AKN', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1249711, 'AND', 'Creativity & Tech Attendee', 'Choir', NULL),
(1290760, 'AND', 'Creativity & Tech Volunteer', 'Band', NULL),
(1098338, 'AND', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1506943, 'AND', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1258670, 'AND', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(1239826, 'AND', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1062481, 'AND', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1219466, 'AND', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1340597, 'AND', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1068550, 'AND', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(778083, 'AND', 'Elementary Attendee', 'Base Camp', NULL),
(776134, 'AND', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(776132, 'AND', 'Elementary Attendee', 'ImagiNation K', NULL),
(776131, 'AND', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(776130, 'AND', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(778081, 'AND', 'Elementary Attendee', 'Shockwave 4th', NULL),
(778082, 'AND', 'Elementary Attendee', 'Shockwave 5th', NULL),
(800903, 'AND', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1382568, 'AND', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1313359, 'AND', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(802734, 'AND', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(800897, 'AND', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(802731, 'AND', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(802732, 'AND', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(797125, 'AND', 'Fuse Attendee', '10th Grade Student', NULL),
(797126, 'AND', 'Fuse Attendee', '10th Grade Student', NULL),
(797128, 'AND', 'Fuse Attendee', '11th Grade Student', NULL),
(797127, 'AND', 'Fuse Attendee', '11th Grade Student', NULL),
(797130, 'AND', 'Fuse Attendee', '12th Grade Student', NULL),
(797129, 'AND', 'Fuse Attendee', '12th Grade Student', NULL),
(797117, 'AND', 'Fuse Attendee', '6th Grade Student', NULL),
(797104, 'AND', 'Fuse Attendee', '6th Grade Student', NULL),
(797119, 'AND', 'Fuse Attendee', '7th Grade Student', NULL),
(797118, 'AND', 'Fuse Attendee', '7th Grade Student', NULL),
(797121, 'AND', 'Fuse Attendee', '8th Grade Student', NULL),
(797120, 'AND', 'Fuse Attendee', '8th Grade Student', NULL),
(797124, 'AND', 'Fuse Attendee', '9th Grade Student', NULL),
(797123, 'AND', 'Fuse Attendee', '9th Grade Student', NULL),
(1362461, 'AND', 'Fuse Attendee', 'Special Event Attendee', NULL),
(1364338, 'AND', 'Fuse Volunteer', 'Atrium', NULL),
(1269081, 'AND', 'Fuse Volunteer', 'Atrium', NULL),
(880596, 'AND', 'Fuse Volunteer', 'Campus Safety', NULL),
(880597, 'AND', 'Fuse Volunteer', 'Campus Safety', NULL),
(1324790, 'AND', 'Fuse Volunteer', 'Care', NULL),
(1269085, 'AND', 'Fuse Volunteer', 'Check-In', NULL),
(1269087, 'AND', 'Fuse Volunteer', 'Check-In', NULL),
(1269091, 'AND', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1269092, 'AND', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1325467, 'AND', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(880601, 'AND', 'Fuse Volunteer', 'Fuse Guest', NULL),
(880581, 'AND', 'Fuse Volunteer', 'Game Room', NULL),
(880584, 'AND', 'Fuse Volunteer', 'Greeter', NULL),
(1355492, 'AND', 'Fuse Volunteer', 'Leadership Team', NULL),
(1269090, 'AND', 'Fuse Volunteer', 'Leadership Team', NULL),
(1269088, 'AND', 'Fuse Volunteer', 'Lounge', NULL),
(1357085, 'AND', 'Fuse Volunteer', 'Lounge', NULL),
(1269089, 'AND', 'Fuse Volunteer', 'Lounge', NULL),
(1239817, 'AND', 'Fuse Volunteer', 'New Serve', NULL),
(815318, 'AND', 'Fuse Volunteer', 'Office Team', NULL),
(880589, 'AND', 'Fuse Volunteer', 'Parking', NULL),
(880590, 'AND', 'Fuse Volunteer', 'Pick-Up', NULL),
(880595, 'AND', 'Fuse Volunteer', 'Production', NULL),
(880591, 'AND', 'Fuse Volunteer', 'Snack Bar', NULL),
(1362462, 'AND', 'Fuse Volunteer', 'Special Event Volunteer', NULL),
(1269083, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(880576, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(880586, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(1355490, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(880600, 'AND', 'Fuse Volunteer', 'Spring Zone', NULL),
(1161745, 'AND', 'Fuse Volunteer', 'Student Leader', NULL),
(1388929, 'AND', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1088020, 'AND', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(880592, 'AND', 'Fuse Volunteer', 'Usher', NULL),
(880593, 'AND', 'Fuse Volunteer', 'VHQ', NULL),
(880594, 'AND', 'Fuse Volunteer', 'VIP Team', NULL),
(1259794, 'AND', 'Fuse Volunteer', 'Worship', NULL),
(1200493, 'AND', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(800226, 'AND', 'Guest Services Attendee', 'Awake Team', NULL),
(800228, 'AND', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(800231, 'AND', 'Guest Services Attendee', 'Greeting Team', NULL),
(1200494, 'AND', 'Guest Services Attendee', 'Load In', NULL),
(1200495, 'AND', 'Guest Services Attendee', 'Load Out', NULL),
(800894, 'AND', 'Guest Services Attendee', 'Office Team', NULL),
(800290, 'AND', 'Guest Services Attendee', 'Parking Team', NULL),
(1335361, 'AND', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1458501, 'AND', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(800294, 'AND', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274607, 'AND', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474621, 'AND', 'Guest Services Volunteer', 'Area Leader', NULL),
(825143, 'AND', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1054718, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(913891, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(1228615, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(801019, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(809737, 'AND', 'Guest Services Volunteer', 'Finance Team', NULL),
(800229, 'AND', 'Guest Services Volunteer', 'Finance Team', NULL),
(800286, 'AND', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239835, 'AND', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1453818, 'AND', 'Guest Services Volunteer', 'Receptionist', NULL),
(1036958, 'AND', 'Guest Services Volunteer', 'Service Leader', NULL),
(1125425, 'AND', 'Guest Services Volunteer', 'Sign Language Team', NULL),
(1335363, 'AND', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1458500, 'AND', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(800293, 'AND', 'Guest Services Volunteer', 'Usher Team', NULL),
(800230, 'AND', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473764, 'AND', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1285784, 'AND', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390935, 'AND', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473765, 'AND', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1285791, 'AND', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1214197, 'AND', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1214198, 'AND', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244643, 'AND', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1285789, 'AND', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390936, 'AND', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473766, 'AND', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1214193, 'AND', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1214194, 'AND', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233805, 'AND', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1214195, 'AND', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1214196, 'AND', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244642, 'AND', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1214191, 'AND', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1085365, 'AND', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1214192, 'AND', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238139, 'AND', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143828, 'AND', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143827, 'AND', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244640, 'AND', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1214190, 'AND', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1285782, 'AND', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390937, 'AND', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304029, 'AND', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1390938, 'AND', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285793, 'AND', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1390939, 'AND', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1285785, 'AND', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1285792, 'AND', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1285804, 'AND', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1285803, 'AND', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1285786, 'AND', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232876, 'AND', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1285788, 'AND', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1285790, 'AND', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1285797, 'AND', 'Next Steps Volunteer', 'Group Leader', NULL),
(1285798, 'AND', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1285806, 'AND', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1285795, 'AND', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1285783, 'AND', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1285802, 'AND', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1285796, 'AND', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285794, 'AND', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1285801, 'AND', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1285805, 'AND', 'Next Steps Volunteer', 'Writing Team', NULL),
(776138, 'AND', 'Nursery Attendee', 'Crawlers', 'Wonder Way 3'),
(776140, 'AND', 'Nursery Attendee', 'Crawlers', 'Wonder Way 4'),
(776129, 'AND', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(776139, 'AND', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 2'),
(776135, 'AND', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 7'),
(776133, 'AND', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 8'),
(776137, 'AND', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 5'),
(776136, 'AND', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 6'),
(802747, 'AND', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(800895, 'AND', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(802748, 'AND', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(802749, 'AND', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(802750, 'AND', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(802751, 'AND', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(802752, 'AND', 'Nursery Volunteer', 'Wonder Way 6 Volunteer', NULL),
(802753, 'AND', 'Nursery Volunteer', 'Wonder Way 7 Volunteer', NULL),
(802754, 'AND', 'Nursery Volunteer', 'Wonder Way 8 Volunteer', NULL),
(961212, 'AND', 'Nursery Volunteer', 'Wonder Way 8 Volunteer', NULL),
(802760, 'AND', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(800558, 'AND', 'Preschool Attendee', '24-29 mo.', 'Fire Station'),
(800560, 'AND', 'Preschool Attendee', '30-31 mo.', 'Lil'' Spring'),
(800559, 'AND', 'Preschool Attendee', '32-33 mo.', 'Pop''s Garage'),
(800561, 'AND', 'Preschool Attendee', '36-37 mo.', 'Spring Fresh'),
(800563, 'AND', 'Preschool Attendee', '38-39 mo.', 'SpringTown Toys'),
(800562, 'AND', 'Preschool Attendee', '40-41 mo.', 'SpringTown Toys'),
(800618, 'AND', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(930856, 'AND', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(800904, 'AND', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(800896, 'AND', 'Preschool Volunteer', 'Fire Station Volunteer', NULL),
(802737, 'AND', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(802736, 'AND', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(802742, 'AND', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(802761, 'AND', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(802739, 'AND', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(802741, 'AND', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(802740, 'AND', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(802762, 'AND', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(800898, 'AND', 'Production Volunteer', 'Elementary Production ', NULL),
(802759, 'AND', 'Production Volunteer', 'Preschool Production', NULL),
(1382567, 'AND', 'Production Volunteer', 'Preschool Production ', NULL),
(778084, 'AND', 'Special Needs Attendee', 'Spring Zone', NULL),
(1102924, 'AND', 'Special Needs Attendee', 'Spring Zone Jr.', NULL),
(1344518, 'AND', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1102965, 'AND', 'Special Needs Volunteer', 'Spring Zone Jr. Volunteer', NULL),
(1297084, 'AND', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(800900, 'AND', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(800899, 'AND', 'Support Volunteer', 'Advocate', NULL),
(802755, 'AND', 'Support Volunteer', 'Check-In Volunteer', NULL),
(802757, 'AND', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1291447, 'AND', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(905603, 'AND', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(935226, 'AND', 'Support Volunteer', 'KidSpring Greeter', NULL),
(874836, 'AND', 'Support Volunteer', 'KidSpring Greeter', NULL),
(800902, 'AND', 'Support Volunteer', 'KidSpring Office Team', NULL),
(802764, 'AND', 'Support Volunteer', 'KidSpring Office Team', NULL),
(802766, 'AND', 'Support Volunteer', 'New Serve Team', NULL),
(802765, 'AND', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1290761, 'BSP', 'Creativity & Tech Volunteer', 'Band', NULL),
(1211162, 'BSP', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1506999, 'BSP', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1258668, 'BSP', 'Creativity & Tech Attendee', 'Load In', NULL),
(1258669, 'BSP', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1239827, 'BSP', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1219521, 'BSP', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1211160, 'BSP', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1211161, 'BSP', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1212114, 'BSP', 'Elementary Attendee', 'Base Camp', NULL),
(1212116, 'BSP', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1212115, 'BSP', 'Elementary Attendee', 'ImagiNation K', NULL),
(1212117, 'BSP', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1212118, 'BSP', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1212119, 'BSP', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1212120, 'BSP', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1212610, 'BSP', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1212614, 'BSP', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1212612, 'BSP', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(1212613, 'BSP', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1212615, 'BSP', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1212616, 'BSP', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1212617, 'BSP', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1211700, 'BSP', 'Fuse Attendee', '10th Grade Student', NULL),
(1211701, 'BSP', 'Fuse Attendee', '10th Grade Student', NULL),
(1211703, 'BSP', 'Fuse Attendee', '11th Grade Student', NULL),
(1211704, 'BSP', 'Fuse Attendee', '11th Grade Student', NULL),
(1211705, 'BSP', 'Fuse Attendee', '12th Grade Student', NULL),
(1211707, 'BSP', 'Fuse Attendee', '12th Grade Student', NULL),
(1211689, 'BSP', 'Fuse Attendee', '6th Grade Student', NULL),
(1211690, 'BSP', 'Fuse Attendee', '6th Grade Student', NULL),
(1211691, 'BSP', 'Fuse Attendee', '7th Grade Student', NULL),
(1211693, 'BSP', 'Fuse Attendee', '7th Grade Student', NULL),
(1211694, 'BSP', 'Fuse Attendee', '8th Grade Student', NULL),
(1211696, 'BSP', 'Fuse Attendee', '8th Grade Student', NULL),
(1211698, 'BSP', 'Fuse Attendee', '9th Grade Student', NULL),
(1211699, 'BSP', 'Fuse Attendee', '9th Grade Student', NULL),
(1364339, 'BSP', 'Fuse Volunteer', 'Atrium', NULL),
(1212047, 'BSP', 'Fuse Volunteer', 'Campus Safety', NULL),
(1361833, 'BSP', 'Fuse Volunteer', 'Care', NULL),
(1348508, 'BSP', 'Fuse Volunteer', 'Check-In', NULL),
(1212049, 'BSP', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1343195, 'BSP', 'Fuse Volunteer', 'Game Room', NULL),
(1212023, 'BSP', 'Fuse Volunteer', 'Greeter', NULL),
(1361834, 'BSP', 'Fuse Volunteer', 'Leadership Team', NULL),
(1212022, 'BSP', 'Fuse Volunteer', 'Load In', NULL),
(1212025, 'BSP', 'Fuse Volunteer', 'Lounge', NULL),
(1239818, 'BSP', 'Fuse Volunteer', 'New Serve', NULL),
(1212027, 'BSP', 'Fuse Volunteer', 'Pick-Up', NULL),
(1212050, 'BSP', 'Fuse Volunteer', 'Production', NULL),
(1212028, 'BSP', 'Fuse Volunteer', 'Snack Bar', NULL),
(1212029, 'BSP', 'Fuse Volunteer', 'Sports', NULL),
(1310527, 'BSP', 'Fuse Volunteer', 'Student Leader', NULL),
(1388934, 'BSP', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1212795, 'BSP', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1212030, 'BSP', 'Fuse Volunteer', 'Usher', NULL),
(1212031, 'BSP', 'Fuse Volunteer', 'VHQ', NULL),
(1212032, 'BSP', 'Fuse Volunteer', 'VIP Team', NULL),
(1259795, 'BSP', 'Fuse Volunteer', 'Worship', NULL),
(1212788, 'BSP', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1212794, 'BSP', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1212798, 'BSP', 'Guest Services Attendee', 'Greeting Team', NULL),
(1212801, 'BSP', 'Guest Services Attendee', 'Load In', NULL),
(1212802, 'BSP', 'Guest Services Attendee', 'Load Out', NULL),
(1212816, 'BSP', 'Guest Services Attendee', 'Office Team', NULL),
(1212807, 'BSP', 'Guest Services Attendee', 'Parking Team', NULL),
(1212812, 'BSP', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274611, 'BSP', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474626, 'BSP', 'Guest Services Volunteer', 'Area Leader', NULL),
(1212790, 'BSP', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1212815, 'BSP', 'Guest Services Volunteer', 'Finance Team', NULL),
(1212663, 'BSP', 'Guest Services Volunteer', 'Finance Team', NULL),
(1212799, 'BSP', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1212804, 'BSP', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1384264, 'BSP', 'Guest Services Volunteer', 'Service Leader', NULL),
(1212809, 'BSP', 'Guest Services Volunteer', 'Service Leader', NULL),
(1212810, 'BSP', 'Guest Services Volunteer', 'Usher Team', NULL),
(1212796, 'BSP', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473768, 'BSP', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1242912, 'BSP', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390942, 'BSP', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1242917, 'BSP', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1230092, 'BSP', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1230093, 'BSP', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244646, 'BSP', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1242919, 'BSP', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390943, 'BSP', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1230090, 'BSP', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1230091, 'BSP', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233806, 'BSP', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1230088, 'BSP', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1230089, 'BSP', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244645, 'BSP', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1230083, 'BSP', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1230087, 'BSP', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238140, 'BSP', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1211642, 'BSP', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1211644, 'BSP', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244644, 'BSP', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1230082, 'BSP', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1242910, 'BSP', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390944, 'BSP', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304030, 'BSP', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1242911, 'BSP', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1242916, 'BSP', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1242932, 'BSP', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1242931, 'BSP', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1242913, 'BSP', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232877, 'BSP', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1242918, 'BSP', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1242923, 'BSP', 'Next Steps Volunteer', 'Group Leader', NULL),
(1242924, 'BSP', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1301525, 'BSP', 'Next Steps Attendee', 'Load In', NULL),
(1301526, 'BSP', 'Next Steps Attendee', 'Load Out', NULL),
(1283766, 'BSP', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1242920, 'BSP', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1242909, 'BSP', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1242930, 'BSP', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1242921, 'BSP', 'Next Steps Volunteer', 'Resource Center', NULL),
(1242929, 'BSP', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1242933, 'BSP', 'Next Steps Volunteer', 'Writing Team', NULL),
(1212602, 'BSP', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1212601, 'BSP', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1212123, 'BSP', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(1212627, 'BSP', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1212630, 'BSP', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1212631, 'BSP', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1212632, 'BSP', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1212629, 'BSP', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1212628, 'BSP', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1212606, 'BSP', 'Preschool Attendee', '30-31 mo.', 'Pop''s Garage'),
(1212607, 'BSP', 'Preschool Attendee', '36-37 mo.', 'SpringTown Toys'),
(1212608, 'BSP', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1212609, 'BSP', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1212634, 'BSP', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1212635, 'BSP', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1212636, 'BSP', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1212640, 'BSP', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1212638, 'BSP', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1212639, 'BSP', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1212641, 'BSP', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1212642, 'BSP', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1212643, 'BSP', 'Production Volunteer', 'Elementary Production ', NULL),
(1212644, 'BSP', 'Production Volunteer', 'Preschool Production', NULL),
(1212645, 'BSP', 'Production Volunteer', 'Production Area Leader', NULL),
(1212646, 'BSP', 'Production Volunteer', 'Production Service Leader', NULL),
(1212121, 'BSP', 'Special Needs Attendee', 'Spring Zone', NULL),
(1212647, 'BSP', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1212648, 'BSP', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1212618, 'BSP', 'Support Volunteer', 'Advocate', NULL),
(1212619, 'BSP', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1212625, 'BSP', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1212620, 'BSP', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1212621, 'BSP', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1212623, 'BSP', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1212624, 'BSP', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1212655, 'BSP', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1212649, 'BSP', 'Support Volunteer', 'Load In', NULL),
(1212650, 'BSP', 'Support Volunteer', 'Load Out', NULL),
(1212653, 'BSP', 'Support Volunteer', 'New Serve Team', NULL),
(1127856, 'CEN', 'Creativity & Tech Volunteer', 'Web Dev Team', NULL),
(1062479, 'CEN', 'Creativity & Tech Volunteer', 'Design Team', NULL),
(920635, 'CEN', 'Creativity & Tech Volunteer', 'Design Team', NULL),
(1339310, 'CEN', 'Creativity & Tech Volunteer', 'Design Team', NULL),
(1506903, 'CEN', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(977592, 'CEN', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(1339312, 'CEN', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(1407196, 'CEN', 'Creativity & Tech Volunteer', 'NewSpring Store Team', NULL),
(1339305, 'CEN', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1068549, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(1340566, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(1339311, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(1264975, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(1062480, 'CEN', 'Creativity & Tech Volunteer', 'Web Dev Team', NULL),
(1339313, 'CEN', 'Creativity & Tech Volunteer', 'Web Dev Team', NULL),
(1293788, 'CEN', 'Fuse Attendee', 'Special Event Attendee', NULL),
(1293789, 'CEN', 'Fuse Volunteer', 'Fuse Office Team', NULL),
(1340555, 'CEN', 'Fuse Volunteer', 'Fuse Office Team', NULL),
(1264974, 'CEN', 'Fuse Volunteer', 'Office Team', NULL),
(1353571, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1348848, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1345803, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1114611, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1388038, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1017062, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1190220, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1327890, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(935951, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1184051, 'CEN', 'Guest Services Volunteer', 'Events Team', NULL),
(1166486, 'CEN', 'Guest Services Volunteer', 'Finance Office Team', NULL),
(939371, 'CEN', 'Guest Services Volunteer', 'GS Office Team', NULL),
(1428460, 'CEN', 'Guest Services Volunteer', 'Network Fuse Team', NULL),
(1428458, 'CEN', 'Guest Services Volunteer', 'Network Office Team', NULL),
(1428459, 'CEN', 'Guest Services Volunteer', 'Network Sunday Team', NULL),
(1166485, 'CEN', 'Guest Services Volunteer', 'Receptionist', NULL),
(1005523, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(938310, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1006455, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(945459, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1211334, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(844951, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1314404, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1508295, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1444489, 'CEN', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1444488, 'CEN', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1419279, 'CEN', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1473844, 'CEN', 'Next Steps Volunteer', 'Church Online Volunteer', NULL),
(1340553, 'CEN', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1166489, 'CEN', 'Next Steps Volunteer', 'NS Office Team', NULL),
(939376, 'CEN', 'Next Steps Volunteer', 'NS Office Team', NULL),
(803636, 'CEN', 'Next Steps Volunteer', 'Writing Team', NULL),
(939370, 'CEN', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1339332, 'CEN', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1257874, 'CHS', 'Creativity & Tech Volunteer', 'Band', NULL),
(1104832, 'CHS', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1507000, 'CHS', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1257875, 'CHS', 'Creativity & Tech Attendee', 'Load In', NULL),
(1257876, 'CHS', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1239828, 'CHS', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1219522, 'CHS', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1068567, 'CHS', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340611, 'CHS', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1068566, 'CHS', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(802875, 'CHS', 'Elementary Attendee', 'Base Camp', NULL),
(802866, 'CHS', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(802865, 'CHS', 'Elementary Attendee', 'ImagiNation K', NULL),
(802867, 'CHS', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(802869, 'CHS', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(802873, 'CHS', 'Elementary Attendee', 'Shockwave 4th', NULL),
(802874, 'CHS', 'Elementary Attendee', 'Shockwave 5th', NULL),
(802143, 'CHS', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1093018, 'CHS', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(802145, 'CHS', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(802141, 'CHS', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(802142, 'CHS', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(802144, 'CHS', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(818312, 'CHS', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(854706, 'CHS', 'Fuse Attendee', '10th Grade Student', NULL),
(854707, 'CHS', 'Fuse Attendee', '10th Grade Student', NULL),
(854708, 'CHS', 'Fuse Attendee', '11th Grade Student', NULL),
(854709, 'CHS', 'Fuse Attendee', '11th Grade Student', NULL),
(854710, 'CHS', 'Fuse Attendee', '12th Grade Student', NULL),
(854711, 'CHS', 'Fuse Attendee', '12th Grade Student', NULL),
(854715, 'CHS', 'Fuse Attendee', '6th Grade Student', NULL),
(854716, 'CHS', 'Fuse Attendee', '6th Grade Student', NULL),
(854717, 'CHS', 'Fuse Attendee', '7th Grade Student', NULL),
(854718, 'CHS', 'Fuse Attendee', '7th Grade Student', NULL),
(854719, 'CHS', 'Fuse Attendee', '8th Grade Student', NULL),
(854720, 'CHS', 'Fuse Attendee', '8th Grade Student', NULL),
(854712, 'CHS', 'Fuse Attendee', '9th Grade Student', NULL),
(854713, 'CHS', 'Fuse Attendee', '9th Grade Student', NULL),
(1364341, 'CHS', 'Fuse Volunteer', 'Atrium', NULL),
(885280, 'CHS', 'Fuse Volunteer', 'Campus Safety', NULL),
(1300266, 'CHS', 'Fuse Volunteer', 'Care', NULL),
(885270, 'CHS', 'Fuse Volunteer', 'Check-In', NULL),
(885256, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885257, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885258, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885259, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885260, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885261, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885262, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885263, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885264, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885265, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885266, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885267, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885268, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885269, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1361842, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885282, 'CHS', 'Fuse Volunteer', 'Fuse Guest', NULL),
(885272, 'CHS', 'Fuse Volunteer', 'Game Room', NULL),
(1023094, 'CHS', 'Fuse Volunteer', 'Greeter', NULL),
(1361839, 'CHS', 'Fuse Volunteer', 'Leadership Team', NULL),
(1395638, 'CHS', 'Fuse Volunteer', 'Load In', NULL),
(885274, 'CHS', 'Fuse Volunteer', 'Lounge', NULL),
(1239819, 'CHS', 'Fuse Volunteer', 'New Serve', NULL),
(1361840, 'CHS', 'Fuse Volunteer', 'Next Steps', NULL),
(854795, 'CHS', 'Fuse Volunteer', 'Office Team', NULL),
(885275, 'CHS', 'Fuse Volunteer', 'Parking', NULL),
(885276, 'CHS', 'Fuse Volunteer', 'Pick-Up', NULL),
(885283, 'CHS', 'Fuse Volunteer', 'Production', NULL),
(885277, 'CHS', 'Fuse Volunteer', 'Snack Bar', NULL),
(885271, 'CHS', 'Fuse Volunteer', 'Sports', NULL),
(885273, 'CHS', 'Fuse Volunteer', 'Sports', NULL),
(1361841, 'CHS', 'Fuse Volunteer', 'Sports', NULL),
(1300265, 'CHS', 'Fuse Volunteer', 'Student Leader', NULL),
(1388935, 'CHS', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1088021, 'CHS', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(885278, 'CHS', 'Fuse Volunteer', 'Usher', NULL),
(1023095, 'CHS', 'Fuse Volunteer', 'VHQ', NULL),
(885279, 'CHS', 'Fuse Volunteer', 'VIP Team', NULL),
(854714, 'CHS', 'Fuse Volunteer', 'VIP Team', NULL),
(854721, 'CHS', 'Fuse Volunteer', 'VIP Team', NULL),
(1259796, 'CHS', 'Fuse Volunteer', 'Worship', NULL),
(1300378, 'CHS', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1446307, 'CHS', 'Guest Services Attendee', 'Awake Team', NULL),
(802112, 'CHS', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(802104, 'CHS', 'Guest Services Attendee', 'Greeting Team', NULL),
(802113, 'CHS', 'Guest Services Attendee', 'Load In', NULL),
(802114, 'CHS', 'Guest Services Attendee', 'Load Out', NULL),
(802116, 'CHS', 'Guest Services Attendee', 'Office Team', NULL),
(802108, 'CHS', 'Guest Services Attendee', 'Parking Team', NULL),
(1342573, 'CHS', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(802109, 'CHS', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274615, 'CHS', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474627, 'CHS', 'Guest Services Volunteer', 'Area Leader', NULL),
(823056, 'CHS', 'Guest Services Volunteer', 'Campus Safety', NULL),
(809740, 'CHS', 'Guest Services Volunteer', 'Finance Team', NULL),
(1040968, 'CHS', 'Guest Services Volunteer', 'Finance Team', NULL),
(802105, 'CHS', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239836, 'CHS', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1384265, 'CHS', 'Guest Services Volunteer', 'Service Leader', NULL),
(1036957, 'CHS', 'Guest Services Volunteer', 'Service Leader', NULL),
(1342572, 'CHS', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1491024, 'CHS', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(802107, 'CHS', 'Guest Services Volunteer', 'Usher Team', NULL),
(951929, 'CHS', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473775, 'CHS', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1276073, 'CHS', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390959, 'CHS', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473776, 'CHS', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1276078, 'CHS', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1213989, 'CHS', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1213990, 'CHS', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244651, 'CHS', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1276081, 'CHS', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390960, 'CHS', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473777, 'CHS', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1213987, 'CHS', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1213988, 'CHS', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233807, 'CHS', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1213985, 'CHS', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1213986, 'CHS', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244649, 'CHS', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1213983, 'CHS', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1213984, 'CHS', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238141, 'CHS', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143836, 'CHS', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1213981, 'CHS', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244650, 'CHS', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1213982, 'CHS', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1276071, 'CHS', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390961, 'CHS', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304031, 'CHS', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1390962, 'CHS', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285766, 'CHS', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1276074, 'CHS', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1276079, 'CHS', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1276064, 'CHS', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1276075, 'CHS', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232879, 'CHS', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1276080, 'CHS', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1276077, 'CHS', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1276065, 'CHS', 'Next Steps Volunteer', 'Group Leader', NULL),
(1276066, 'CHS', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1301527, 'CHS', 'Next Steps Attendee', 'Load In', NULL),
(1283767, 'CHS', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1276069, 'CHS', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1276072, 'CHS', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1276042, 'CHS', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1276070, 'CHS', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285767, 'CHS', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1276041, 'CHS', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1276040, 'CHS', 'Next Steps Volunteer', 'Writing Team', NULL),
(870959, 'CHS', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(802861, 'CHS', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1267824, 'CHS', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 4'),
(1101309, 'CHS', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(802146, 'CHS', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(818316, 'CHS', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(870960, 'CHS', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1102949, 'CHS', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1267825, 'CHS', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1210227, 'CHS', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(818313, 'CHS', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(870961, 'CHS', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1394880, 'CHS', 'Preschool Attendee', '30-31 mo.', 'Lil'' Spring'),
(802863, 'CHS', 'Preschool Attendee', '42-43 mo.', 'SpringTown Toys'),
(1102946, 'CHS', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(827174, 'CHS', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(818317, 'CHS', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1396339, 'CHS', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(870966, 'CHS', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1210230, 'CHS', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(818319, 'CHS', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(818320, 'CHS', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(818318, 'CHS', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1102962, 'CHS', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(818324, 'CHS', 'Production Volunteer', 'Elementary Production', NULL),
(802881, 'CHS', 'Special Needs Attendee', 'Spring Zone', NULL),
(1497243, 'CHS', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(818325, 'CHS', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(818321, 'CHS', 'Support Volunteer', 'Advocate', NULL),
(818322, 'CHS', 'Support Volunteer', 'Check-In Volunteer', NULL),
(818323, 'CHS', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1093013, 'CHS', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1090403, 'CHS', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(919056, 'CHS', 'Support Volunteer', 'KidSpring Greeter', NULL),
(802139, 'CHS', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1093017, 'CHS', 'Support Volunteer', 'Load In', NULL),
(1028225, 'CHS', 'Support Volunteer', 'Load In', NULL),
(818328, 'CHS', 'Support Volunteer', 'New Serve Team', NULL),
(818327, 'CHS', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1456001, 'CLE', 'Elementary Attendee', 'Base Camp', NULL),
(1456002, 'CLE', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1456000, 'CLE', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1456116, 'CLE', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1290762, 'COL', 'Creativity & Tech Volunteer', 'Band', NULL),
(1104833, 'COL', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1507001, 'COL', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1258667, 'COL', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(1239829, 'COL', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1327325, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1219523, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1300462, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1286526, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1068569, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1068568, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340614, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340615, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(800687, 'COL', 'Elementary Attendee', 'Base Camp', NULL),
(800682, 'COL', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(800681, 'COL', 'Elementary Attendee', 'ImagiNation K', NULL),
(1408702, 'COL', 'Elementary Attendee', 'ImagiNation K', NULL),
(800683, 'COL', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1408709, 'COL', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(800684, 'COL', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1408711, 'COL', 'Elementary Attendee', 'Shockwave 4th', NULL),
(800685, 'COL', 'Elementary Attendee', 'Shockwave 4th', NULL),
(800686, 'COL', 'Elementary Attendee', 'Shockwave 5th', NULL),
(802185, 'COL', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1382563, 'COL', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(802186, 'COL', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(802180, 'COL', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1413377, 'COL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(935220, 'COL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(802182, 'COL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(935221, 'COL', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(802178, 'COL', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1413379, 'COL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(935222, 'COL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(802179, 'COL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(797369, 'COL', 'Fuse Attendee', '10th Grade Student', NULL),
(797368, 'COL', 'Fuse Attendee', '10th Grade Student', NULL),
(797370, 'COL', 'Fuse Attendee', '11th Grade Student', NULL),
(797371, 'COL', 'Fuse Attendee', '11th Grade Student', NULL),
(797372, 'COL', 'Fuse Attendee', '12th Grade Student', NULL),
(797373, 'COL', 'Fuse Attendee', '12th Grade Student', NULL),
(797374, 'COL', 'Fuse Attendee', '6th Grade Student', NULL),
(797375, 'COL', 'Fuse Attendee', '6th Grade Student', NULL),
(797381, 'COL', 'Fuse Attendee', '7th Grade Student', NULL),
(797380, 'COL', 'Fuse Attendee', '7th Grade Student', NULL),
(797378, 'COL', 'Fuse Attendee', '8th Grade Student', NULL),
(797379, 'COL', 'Fuse Attendee', '8th Grade Student', NULL),
(797376, 'COL', 'Fuse Attendee', '9th Grade Student', NULL),
(797377, 'COL', 'Fuse Attendee', '9th Grade Student', NULL),
(1154931, 'COL', 'Fuse Volunteer', 'Atrium', NULL),
(1202544, 'COL', 'Fuse Volunteer', 'Atrium', NULL),
(885204, 'COL', 'Fuse Volunteer', 'Campus Safety', NULL),
(885205, 'COL', 'Fuse Volunteer', 'Campus Safety', NULL),
(1361878, 'COL', 'Fuse Volunteer', 'Care', NULL),
(885174, 'COL', 'Fuse Volunteer', 'Check-In', NULL),
(885176, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885177, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885191, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885192, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885193, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885194, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885195, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885196, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885197, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885198, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885199, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885200, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885201, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885202, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1361884, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885210, 'COL', 'Fuse Volunteer', 'Fuse Guest', NULL),
(885180, 'COL', 'Fuse Volunteer', 'Game Room', NULL),
(885181, 'COL', 'Fuse Volunteer', 'Greeter', NULL),
(1361879, 'COL', 'Fuse Volunteer', 'Leadership Team', NULL),
(1349719, 'COL', 'Fuse Volunteer', 'Leadership Team', NULL),
(885183, 'COL', 'Fuse Volunteer', 'Load In', NULL),
(885184, 'COL', 'Fuse Volunteer', 'Lounge', NULL),
(1239820, 'COL', 'Fuse Volunteer', 'New Serve', NULL),
(815440, 'COL', 'Fuse Volunteer', 'Office Team', NULL),
(885185, 'COL', 'Fuse Volunteer', 'Parking', NULL),
(885186, 'COL', 'Fuse Volunteer', 'Pick-Up', NULL),
(885208, 'COL', 'Fuse Volunteer', 'Production', NULL),
(885187, 'COL', 'Fuse Volunteer', 'Snack Bar', NULL),
(885178, 'COL', 'Fuse Volunteer', 'Sports', NULL),
(885182, 'COL', 'Fuse Volunteer', 'Sports', NULL),
(1361882, 'COL', 'Fuse Volunteer', 'Sports', NULL),
(885209, 'COL', 'Fuse Volunteer', 'Spring Zone', NULL),
(1088078, 'COL', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(885188, 'COL', 'Fuse Volunteer', 'Usher', NULL),
(885189, 'COL', 'Fuse Volunteer', 'VHQ', NULL),
(885190, 'COL', 'Fuse Volunteer', 'VIP Team', NULL),
(1259797, 'COL', 'Fuse Volunteer', 'Worship', NULL),
(1300379, 'COL', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1232752, 'COL', 'Guest Services Attendee', 'Awake Team', NULL),
(882614, 'COL', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(802169, 'COL', 'Guest Services Attendee', 'Greeting Team', NULL),
(802177, 'COL', 'Guest Services Attendee', 'Office Team', NULL),
(802176, 'COL', 'Guest Services Attendee', 'Parking Team', NULL),
(1342575, 'COL', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(802172, 'COL', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274618, 'COL', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474630, 'COL', 'Guest Services Volunteer', 'Area Leader', NULL),
(823058, 'COL', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1040958, 'COL', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(1386965, 'COL', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(809741, 'COL', 'Guest Services Volunteer', 'Finance Team', NULL),
(1040967, 'COL', 'Guest Services Volunteer', 'Finance Team', NULL),
(802170, 'COL', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(931944, 'COL', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239837, 'COL', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1384266, 'COL', 'Guest Services Volunteer', 'Service Leader', NULL),
(1036956, 'COL', 'Guest Services Volunteer', 'Service Leader', NULL),
(1249740, 'COL', 'Guest Services Volunteer', 'Sign Language Team', NULL),
(1342574, 'COL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1491025, 'COL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(802171, 'COL', 'Guest Services Volunteer', 'Usher Team', NULL),
(931945, 'COL', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473782, 'COL', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1285726, 'COL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390907, 'COL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473783, 'COL', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1285733, 'COL', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1214058, 'COL', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1214061, 'COL', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244654, 'COL', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1285731, 'COL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390908, 'COL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473784, 'COL', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1214069, 'COL', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1214070, 'COL', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233808, 'COL', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1214054, 'COL', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1214055, 'COL', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244653, 'COL', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1214056, 'COL', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1214057, 'COL', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238142, 'COL', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143845, 'COL', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143844, 'COL', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244652, 'COL', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1214053, 'COL', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1285724, 'COL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390906, 'COL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304032, 'COL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1390925, 'COL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285768, 'COL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1390909, 'COL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1285727, 'COL', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1285734, 'COL', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1285747, 'COL', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1285746, 'COL', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1285728, 'COL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232880, 'COL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1285730, 'COL', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1285732, 'COL', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1285738, 'COL', 'Next Steps Volunteer', 'Group Leader', NULL),
(1285740, 'COL', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1285750, 'COL', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1285736, 'COL', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1423948, 'COL', 'Next Steps Volunteer', 'NS Office Team', NULL),
(1285725, 'COL', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1285745, 'COL', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1285737, 'COL', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285769, 'COL', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1285743, 'COL', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1285748, 'COL', 'Next Steps Volunteer', 'Writing Team', NULL),
(800676, 'COL', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(800675, 'COL', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1375038, 'COL', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 5'),
(1408697, 'COL', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 6'),
(858500, 'COL', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(1005207, 'COL', 'Nursery Attendee', 'Young Toddlers', 'Wonder Way 4'),
(802181, 'COL', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(935213, 'COL', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(818121, 'COL', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(935214, 'COL', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(802183, 'COL', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(935215, 'COL', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(858501, 'COL', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1005221, 'COL', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1005223, 'COL', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1375311, 'COL', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(1375297, 'COL', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(1413374, 'COL', 'Nursery Volunteer', 'Wonder Way 6 Volunteer', NULL),
(1382560, 'COL', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(818120, 'COL', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1059150, 'COL', 'Preschool Attendee', '24-29 mo.', 'Lil'' Spring'),
(800677, 'COL', 'Preschool Attendee', '30-31 mo.', 'Pop''s Garage'),
(1059151, 'COL', 'Preschool Attendee', '36-37 mo.', 'Spring Fresh'),
(800678, 'COL', 'Preschool Attendee', '46-47 mo.', 'SpringTown Toys'),
(800679, 'COL', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1408701, 'COL', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(800680, 'COL', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(818122, 'COL', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1060521, 'COL', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1060524, 'COL', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(935216, 'COL', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(818123, 'COL', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1382561, 'COL', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(818126, 'COL', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(818127, 'COL', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1060526, 'COL', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(1060527, 'COL', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(935217, 'COL', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(818124, 'COL', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1413376, 'COL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(935218, 'COL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(818125, 'COL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1382565, 'COL', 'Production Volunteer', 'Elementary Production', NULL),
(818131, 'COL', 'Production Volunteer', 'Elementary Production', NULL),
(1382564, 'COL', 'Production Volunteer', 'Production Area Leader', NULL),
(802878, 'COL', 'Special Needs Attendee', 'Spring Zone', NULL),
(1294812, 'COL', 'Special Needs Attendee', 'Spring Zone Jr.', NULL),
(1386015, 'COL', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1294818, 'COL', 'Special Needs Volunteer', 'Spring Zone Jr. Volunteer', NULL),
(1386014, 'COL', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(818132, 'COL', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(818128, 'COL', 'Support Volunteer', 'Advocate', NULL),
(818129, 'COL', 'Support Volunteer', 'Check-In Volunteer', NULL),
(818130, 'COL', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1382566, 'COL', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1396766, 'COL', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1402257, 'COL', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1402255, 'COL', 'Support Volunteer', 'KidSpring Greeter', NULL),
(802187, 'COL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(818133, 'COL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(818135, 'COL', 'Support Volunteer', 'New Serve Team', NULL),
(818134, 'COL', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1290763, 'FLO', 'Creativity & Tech Volunteer', 'Band', NULL),
(1068572, 'FLO', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1507028, 'FLO', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1071052, 'FLO', 'Creativity & Tech Attendee', 'Load In', NULL),
(1071053, 'FLO', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1239830, 'FLO', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1068571, 'FLO', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1071051, 'FLO', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(802843, 'FLO', 'Elementary Attendee', 'Base Camp', NULL),
(802844, 'FLO', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(802845, 'FLO', 'Elementary Attendee', 'ImagiNation K', NULL),
(802846, 'FLO', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(802847, 'FLO', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(802848, 'FLO', 'Elementary Attendee', 'Shockwave 4th', NULL),
(802849, 'FLO', 'Elementary Attendee', 'Shockwave 5th', NULL),
(802202, 'FLO', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(821418, 'FLO', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(802203, 'FLO', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(802197, 'FLO', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(802199, 'FLO', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(802195, 'FLO', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(818095, 'FLO', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(797411, 'FLO', 'Fuse Attendee', '10th Grade Student', NULL),
(797412, 'FLO', 'Fuse Attendee', '10th Grade Student', NULL),
(797413, 'FLO', 'Fuse Attendee', '11th Grade Student', NULL),
(797415, 'FLO', 'Fuse Attendee', '11th Grade Student', NULL),
(797416, 'FLO', 'Fuse Attendee', '12th Grade Student', NULL),
(797417, 'FLO', 'Fuse Attendee', '12th Grade Student', NULL),
(797399, 'FLO', 'Fuse Attendee', '6th Grade Student', NULL),
(797401, 'FLO', 'Fuse Attendee', '6th Grade Student', NULL),
(797403, 'FLO', 'Fuse Attendee', '7th Grade Student', NULL),
(797406, 'FLO', 'Fuse Attendee', '7th Grade Student', NULL),
(797407, 'FLO', 'Fuse Attendee', '8th Grade Student', NULL),
(797409, 'FLO', 'Fuse Attendee', '8th Grade Student', NULL),
(797418, 'FLO', 'Fuse Attendee', '9th Grade Student', NULL),
(797419, 'FLO', 'Fuse Attendee', '9th Grade Student', NULL),
(1364344, 'FLO', 'Fuse Volunteer', 'Atrium', NULL),
(885240, 'FLO', 'Fuse Volunteer', 'Campus Safety', NULL),
(885241, 'FLO', 'Fuse Volunteer', 'Campus Safety', NULL),
(1361889, 'FLO', 'Fuse Volunteer', 'Care', NULL),
(885228, 'FLO', 'Fuse Volunteer', 'Check-In', NULL),
(885212, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885213, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885215, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885216, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885217, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885218, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885219, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885220, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885221, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885222, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885223, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885225, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885226, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885227, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1361895, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(885239, 'FLO', 'Fuse Volunteer', 'Fuse Guest', NULL),
(885230, 'FLO', 'Fuse Volunteer', 'Game Room', NULL),
(1023096, 'FLO', 'Fuse Volunteer', 'Greeter', NULL),
(1361892, 'FLO', 'Fuse Volunteer', 'Leadership Team', NULL),
(885232, 'FLO', 'Fuse Volunteer', 'Lounge', NULL),
(1239821, 'FLO', 'Fuse Volunteer', 'New Serve', NULL),
(1361893, 'FLO', 'Fuse Volunteer', 'Next Steps', NULL),
(815451, 'FLO', 'Fuse Volunteer', 'Office Team', NULL),
(885233, 'FLO', 'Fuse Volunteer', 'Parking', NULL),
(885234, 'FLO', 'Fuse Volunteer', 'Pick-Up', NULL),
(885238, 'FLO', 'Fuse Volunteer', 'Production', NULL),
(885235, 'FLO', 'Fuse Volunteer', 'Snack Bar', NULL),
(885229, 'FLO', 'Fuse Volunteer', 'Sports', NULL),
(885231, 'FLO', 'Fuse Volunteer', 'Sports', NULL),
(1361894, 'FLO', 'Fuse Volunteer', 'Sports', NULL),
(1344496, 'FLO', 'Fuse Volunteer', 'Spring Zone', NULL),
(1361896, 'FLO', 'Fuse Volunteer', 'Student Leader', NULL),
(1388940, 'FLO', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1088079, 'FLO', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(885236, 'FLO', 'Fuse Volunteer', 'Usher', NULL),
(1023097, 'FLO', 'Fuse Volunteer', 'VHQ', NULL),
(819675, 'FLO', 'Fuse Volunteer', 'VIP Team', NULL),
(819674, 'FLO', 'Fuse Volunteer', 'VIP Team', NULL),
(885237, 'FLO', 'Fuse Volunteer', 'VIP Team', NULL),
(1259798, 'FLO', 'Fuse Volunteer', 'Worship', NULL),
(1300383, 'FLO', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1417899, 'FLO', 'Guest Services Attendee', 'Awake Team', NULL),
(802216, 'FLO', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(802208, 'FLO', 'Guest Services Attendee', 'Greeting Team', NULL),
(802218, 'FLO', 'Guest Services Attendee', 'Load In', NULL),
(802219, 'FLO', 'Guest Services Attendee', 'Load Out', NULL),
(802217, 'FLO', 'Guest Services Attendee', 'Office Team', NULL),
(802212, 'FLO', 'Guest Services Attendee', 'Parking Team', NULL),
(1342578, 'FLO', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(802213, 'FLO', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274622, 'FLO', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474633, 'FLO', 'Guest Services Volunteer', 'Area Leader', NULL),
(823061, 'FLO', 'Guest Services Volunteer', 'Campus Safety', NULL),
(854801, 'FLO', 'Guest Services Volunteer', 'Finance Team', NULL),
(1040966, 'FLO', 'Guest Services Volunteer', 'Finance Team', NULL),
(802210, 'FLO', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239838, 'FLO', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1036955, 'FLO', 'Guest Services Volunteer', 'Service Leader', NULL),
(1491026, 'FLO', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(802211, 'FLO', 'Guest Services Volunteer', 'Usher Team', NULL),
(825706, 'FLO', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473785, 'FLO', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1283450, 'FLO', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390965, 'FLO', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473786, 'FLO', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1283455, 'FLO', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1214031, 'FLO', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1214032, 'FLO', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244658, 'FLO', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1283458, 'FLO', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390966, 'FLO', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473787, 'FLO', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1214033, 'FLO', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1214034, 'FLO', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233809, 'FLO', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1214027, 'FLO', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1214028, 'FLO', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244656, 'FLO', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1214029, 'FLO', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1214030, 'FLO', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238143, 'FLO', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143866, 'FLO', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143865, 'FLO', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244655, 'FLO', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1214026, 'FLO', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1283448, 'FLO', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390967, 'FLO', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304033, 'FLO', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1390968, 'FLO', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285770, 'FLO', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1390969, 'FLO', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1283451, 'FLO', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1283456, 'FLO', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1283468, 'FLO', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1283467, 'FLO', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1283452, 'FLO', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1283457, 'FLO', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1283454, 'FLO', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1283461, 'FLO', 'Next Steps Volunteer', 'Group Leader', NULL)

insert #rlcMap 
values
(1283462, 'FLO', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1232881, 'FLO', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1301529, 'FLO', 'Next Steps Attendee', 'Load In', NULL),
(1301530, 'FLO', 'Next Steps Attendee', 'Load Out', NULL),
(1283769, 'FLO', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1283459, 'FLO', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1283449, 'FLO', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1283466, 'FLO', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1283460, 'FLO', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285771, 'FLO', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1283465, 'FLO', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1283469, 'FLO', 'Next Steps Volunteer', 'Writing Team', NULL),
(802839, 'FLO', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(802838, 'FLO', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(931362, 'FLO', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 4'),
(858502, 'FLO', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(802196, 'FLO', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(802200, 'FLO', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(818096, 'FLO', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(858503, 'FLO', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(931363, 'FLO', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(821416, 'FLO', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(802198, 'FLO', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(931364, 'FLO', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1294275, 'FLO', 'Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
(802841, 'FLO', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(802842, 'FLO', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(827250, 'FLO', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(818097, 'FLO', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1294276, 'FLO', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(931365, 'FLO', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(821417, 'FLO', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(818100, 'FLO', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(818101, 'FLO', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(818098, 'FLO', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(818099, 'FLO', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(818105, 'FLO', 'Production Volunteer', 'Elementary Production', NULL),
(821421, 'FLO', 'Production Volunteer', 'Production Area Leader', NULL),
(802880, 'FLO', 'Special Needs Attendee', 'Spring Zone', NULL),
(821419, 'FLO', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(818106, 'FLO', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(818102, 'FLO', 'Support Volunteer', 'Advocate', NULL),
(818103, 'FLO', 'Support Volunteer', 'Check-In Volunteer', NULL),
(818104, 'FLO', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(821420, 'FLO', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(802209, 'FLO', 'Support Volunteer', 'KidSpring Greeter', NULL),
(802201, 'FLO', 'Support Volunteer', 'KidSpring Office Team', NULL),
(818107, 'FLO', 'Support Volunteer', 'KidSpring Office Team', NULL),
(818143, 'FLO', 'Support Volunteer', 'Load In', NULL),
(818145, 'FLO', 'Support Volunteer', 'Load Out', NULL),
(818109, 'FLO', 'Support Volunteer', 'New Serve Team', NULL),
(821427, 'FLO', 'Support Volunteer', 'New Serve Team', NULL),
(818108, 'FLO', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1445840, 'GRR', 'Elementary Attendee', 'Base Camp', NULL),
(1445841, 'GRR', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1450442, 'GRR', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1440326, 'GRR', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1362086, 'GVL', 'Creativity & Tech Attendee', 'Choir', NULL),
(1290764, 'GVL', 'Creativity & Tech Volunteer', 'Band', NULL),
(1104835, 'GVL', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1507029, 'GVL', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1258665, 'GVL', 'Creativity & Tech Attendee', 'Load In', NULL),
(1258666, 'GVL', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1239831, 'GVL', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1474090, 'GVL', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1219524, 'GVL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1068581, 'GVL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1068580, 'GVL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340621, 'GVL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(800633, 'GVL', 'Elementary Attendee', 'Base Camp', NULL),
(800627, 'GVL', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(800628, 'GVL', 'Elementary Attendee', 'ImagiNation K', NULL),
(800629, 'GVL', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(800630, 'GVL', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(800631, 'GVL', 'Elementary Attendee', 'Shockwave 4th', NULL),
(800632, 'GVL', 'Elementary Attendee', 'Shockwave 5th', NULL),
(802243, 'GVL', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(821424, 'GVL', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(802244, 'GVL', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(802238, 'GVL', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(802240, 'GVL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(802236, 'GVL', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(817137, 'GVL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(797443, 'GVL', 'Fuse Attendee', '10th Grade Student', NULL),
(797445, 'GVL', 'Fuse Attendee', '10th Grade Student', NULL),
(797446, 'GVL', 'Fuse Attendee', '11th Grade Student', NULL),
(797447, 'GVL', 'Fuse Attendee', '11th Grade Student', NULL),
(797449, 'GVL', 'Fuse Attendee', '12th Grade Student', NULL),
(797451, 'GVL', 'Fuse Attendee', '12th Grade Student', NULL),
(797436, 'GVL', 'Fuse Attendee', '6th Grade Student', NULL),
(797437, 'GVL', 'Fuse Attendee', '6th Grade Student', NULL),
(797438, 'GVL', 'Fuse Attendee', '7th Grade Student', NULL),
(797439, 'GVL', 'Fuse Attendee', '7th Grade Student', NULL),
(797440, 'GVL', 'Fuse Attendee', '8th Grade Student', NULL),
(797442, 'GVL', 'Fuse Attendee', '8th Grade Student', NULL),
(797452, 'GVL', 'Fuse Attendee', '9th Grade Student', NULL),
(797454, 'GVL', 'Fuse Attendee', '9th Grade Student', NULL),
(1364345, 'GVL', 'Fuse Volunteer', 'Atrium', NULL),
(884616, 'GVL', 'Fuse Volunteer', 'Campus Safety', NULL),
(884618, 'GVL', 'Fuse Volunteer', 'Campus Safety', NULL),
(1319351, 'GVL', 'Fuse Volunteer', 'Care', NULL),
(884587, 'GVL', 'Fuse Volunteer', 'Check-In', NULL),
(884609, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884610, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884611, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884612, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884613, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884614, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884601, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884602, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884603, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884604, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884605, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884606, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884607, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884608, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(884622, 'GVL', 'Fuse Volunteer', 'Fuse Guest', NULL),
(884590, 'GVL', 'Fuse Volunteer', 'Game Room', NULL),
(884591, 'GVL', 'Fuse Volunteer', 'Greeter', NULL),
(1361991, 'GVL', 'Fuse Volunteer', 'Leadership Team', NULL),
(886238, 'GVL', 'Fuse Volunteer', 'Leadership Team', NULL),
(884593, 'GVL', 'Fuse Volunteer', 'Load In', NULL),
(884594, 'GVL', 'Fuse Volunteer', 'Lounge', NULL),
(1239822, 'GVL', 'Fuse Volunteer', 'New Serve', NULL),
(1361992, 'GVL', 'Fuse Volunteer', 'Next Steps', NULL),
(815456, 'GVL', 'Fuse Volunteer', 'Office Team', NULL),
(884595, 'GVL', 'Fuse Volunteer', 'Parking', NULL),
(884596, 'GVL', 'Fuse Volunteer', 'Pick-Up', NULL),
(884620, 'GVL', 'Fuse Volunteer', 'Production', NULL),
(884597, 'GVL', 'Fuse Volunteer', 'Snack Bar', NULL),
(884588, 'GVL', 'Fuse Volunteer', 'Sports', NULL),
(884592, 'GVL', 'Fuse Volunteer', 'Sports', NULL),
(1361993, 'GVL', 'Fuse Volunteer', 'Sports', NULL),
(1388941, 'GVL', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1088080, 'GVL', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(884598, 'GVL', 'Fuse Volunteer', 'Usher', NULL),
(884599, 'GVL', 'Fuse Volunteer', 'VHQ', NULL),
(884600, 'GVL', 'Fuse Volunteer', 'VIP Team', NULL),
(819686, 'GVL', 'Fuse Volunteer', 'VIP Team', NULL),
(819687, 'GVL', 'Fuse Volunteer', 'VIP Team', NULL),
(1259799, 'GVL', 'Fuse Volunteer', 'Worship', NULL),
(1300384, 'GVL', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(843865, 'GVL', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(802245, 'GVL', 'Guest Services Attendee', 'Greeting Team', NULL),
(802255, 'GVL', 'Guest Services Attendee', 'Load In', NULL),
(802256, 'GVL', 'Guest Services Attendee', 'Load Out', NULL),
(802254, 'GVL', 'Guest Services Attendee', 'Office Team', NULL),
(802250, 'GVL', 'Guest Services Attendee', 'Parking Team', NULL),
(1342580, 'GVL', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(802251, 'GVL', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274623, 'GVL', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474636, 'GVL', 'Guest Services Volunteer', 'Area Leader', NULL),
(823063, 'GVL', 'Guest Services Volunteer', 'Campus Safety', NULL),
(809754, 'GVL', 'Guest Services Volunteer', 'Finance Team', NULL),
(1040965, 'GVL', 'Guest Services Volunteer', 'Finance Team', NULL),
(802248, 'GVL', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239839, 'GVL', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1036954, 'GVL', 'Guest Services Volunteer', 'Service Leader', NULL),
(1342579, 'GVL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1491027, 'GVL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(802249, 'GVL', 'Guest Services Volunteer', 'Usher Team', NULL),
(825763, 'GVL', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473792, 'GVL', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1285697, 'GVL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390926, 'GVL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473793, 'GVL', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1285704, 'GVL', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1214153, 'GVL', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1214154, 'GVL', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244663, 'GVL', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1285702, 'GVL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390927, 'GVL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473794, 'GVL', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1214149, 'GVL', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1214150, 'GVL', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233810, 'GVL', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1214151, 'GVL', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1214152, 'GVL', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244661, 'GVL', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1214095, 'GVL', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(867637, 'GVL', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1214096, 'GVL', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238144, 'GVL', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143876, 'GVL', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143877, 'GVL', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244660, 'GVL', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1214094, 'GVL', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1285696, 'GVL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390928, 'GVL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304034, 'GVL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1390929, 'GVL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285772, 'GVL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1390930, 'GVL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1285698, 'GVL', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1285705, 'GVL', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1285715, 'GVL', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1285714, 'GVL', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1285699, 'GVL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232882, 'GVL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1285701, 'GVL', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1285708, 'GVL', 'Next Steps Volunteer', 'Group Leader', NULL),
(1285709, 'GVL', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1301531, 'GVL', 'Next Steps Attendee', 'Load In', NULL),
(1301532, 'GVL', 'Next Steps Attendee', 'Load Out', NULL),
(1285717, 'GVL', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1285706, 'GVL', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1285695, 'GVL', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1285713, 'GVL', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1285707, 'GVL', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285773, 'GVL', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1285712, 'GVL', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1285716, 'GVL', 'Next Steps Volunteer', 'Writing Team', NULL),
(800622, 'GVL', 'Nursery Attendee', 'Crawlers', 'Wonder Way 3'),
(800623, 'GVL', 'Nursery Attendee', 'Crawlers', 'Wonder Way 4'),
(800620, 'GVL', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(800621, 'GVL', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 2'),
(828967, 'GVL', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 6'),
(1165257, 'GVL', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 7'),
(828965, 'GVL', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 5'),
(817139, 'GVL', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(802237, 'GVL', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(802239, 'GVL', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(802241, 'GVL', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(817142, 'GVL', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(828968, 'GVL', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(818140, 'GVL', 'Nursery Volunteer', 'Wonder Way 6 Volunteer', NULL),
(1268147, 'GVL', 'Nursery Volunteer', 'Wonder Way 7 Volunteer', NULL),
(821422, 'GVL', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(817140, 'GVL', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(800624, 'GVL', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1080425, 'GVL', 'Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
(1259860, 'GVL', 'Preschool Attendee', '46-47 mo.', 'Police Station'),
(800625, 'GVL', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(800626, 'GVL', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1200645, 'GVL', 'Preschool Attendee', '50-51 mo.', 'Spring Fresh'),
(800634, 'GVL', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(817143, 'GVL', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1080430, 'GVL', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(817145, 'GVL', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(821423, 'GVL', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(817150, 'GVL', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(817154, 'GVL', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1200646, 'GVL', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(1268148, 'GVL', 'Preschool Volunteer', 'Police Station Volunteer', NULL),
(817146, 'GVL', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(817148, 'GVL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(856972, 'GVL', 'Production Volunteer', 'Elementary Production ', NULL),
(856973, 'GVL', 'Production Volunteer', 'Preschool Production', NULL),
(1127217, 'GVL', 'Production Volunteer', 'Production Area Leader', NULL),
(854476, 'GVL', 'Production Volunteer', 'Production Service Leader', NULL),
(800635, 'GVL', 'Special Needs Attendee', 'Spring Zone', NULL),
(1087936, 'GVL', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(817167, 'GVL', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(817156, 'GVL', 'Support Volunteer', 'Advocate', NULL),
(817157, 'GVL', 'Support Volunteer', 'Check-In Volunteer', NULL),
(817159, 'GVL', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(923364, 'GVL', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(854474, 'GVL', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(802242, 'GVL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(817170, 'GVL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(818141, 'GVL', 'Support Volunteer', 'Load In', NULL),
(818142, 'GVL', 'Support Volunteer', 'Load Out', NULL),
(817173, 'GVL', 'Support Volunteer', 'New Serve Team', NULL),
(1407498, 'GVL', 'Support Volunteer', 'New Serve Team', NULL),
(817172, 'GVL', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1290765, 'GWD', 'Creativity & Tech Volunteer', 'Band', NULL),
(1104836, 'GWD', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1239832, 'GWD', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1258906, 'GWD', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1258904, 'GWD', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1184042, 'GWD', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1068642, 'GWD', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340623, 'GWD', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340624, 'GWD', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(872798, 'GWD', 'Elementary Attendee', 'Base Camp', NULL),
(872799, 'GWD', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(872800, 'GWD', 'Elementary Attendee', 'ImagiNation K', NULL),
(872802, 'GWD', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(872803, 'GWD', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(872804, 'GWD', 'Elementary Attendee', 'Shockwave 4th', NULL),
(872806, 'GWD', 'Elementary Attendee', 'Shockwave 5th', NULL),
(872826, 'GWD', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(872828, 'GWD', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(872829, 'GWD', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(872830, 'GWD', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(872832, 'GWD', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(872834, 'GWD', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(872835, 'GWD', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1154644, 'GWD', 'Fuse Attendee', '10th Grade Student', NULL),
(1154645, 'GWD', 'Fuse Attendee', '10th Grade Student', NULL),
(1154646, 'GWD', 'Fuse Attendee', '11th Grade Student', NULL),
(1154648, 'GWD', 'Fuse Attendee', '11th Grade Student', NULL),
(1154649, 'GWD', 'Fuse Attendee', '12th Grade Student', NULL),
(1154650, 'GWD', 'Fuse Attendee', '12th Grade Student', NULL),
(1154636, 'GWD', 'Fuse Attendee', '6th Grade Student', NULL),
(1154635, 'GWD', 'Fuse Attendee', '6th Grade Student', NULL),
(1154637, 'GWD', 'Fuse Attendee', '7th Grade Student', NULL),
(1154638, 'GWD', 'Fuse Attendee', '7th Grade Student', NULL),
(1154639, 'GWD', 'Fuse Attendee', '8th Grade Student', NULL),
(1154640, 'GWD', 'Fuse Attendee', '8th Grade Student', NULL),
(1154641, 'GWD', 'Fuse Attendee', '9th Grade Student', NULL),
(1154642, 'GWD', 'Fuse Attendee', '9th Grade Student', NULL),
(1364346, 'GWD', 'Fuse Volunteer', 'Atrium', NULL),
(1154420, 'GWD', 'Fuse Volunteer', 'Campus Safety', NULL),
(1327880, 'GWD', 'Fuse Volunteer', 'Care', NULL),
(1154401, 'GWD', 'Fuse Volunteer', 'Check-In', NULL),
(1154439, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1154436, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1154435, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1361998, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1154426, 'GWD', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1154404, 'GWD', 'Fuse Volunteer', 'Game Room', NULL),
(1154405, 'GWD', 'Fuse Volunteer', 'Greeter', NULL),
(1361996, 'GWD', 'Fuse Volunteer', 'Leadership Team', NULL),
(1347575, 'GWD', 'Fuse Volunteer', 'Leadership Team', NULL),
(1154408, 'GWD', 'Fuse Volunteer', 'Lounge', NULL),
(1239823, 'GWD', 'Fuse Volunteer', 'New Serve', NULL),
(1154398, 'GWD', 'Fuse Volunteer', 'Office Team', NULL),
(1154409, 'GWD', 'Fuse Volunteer', 'Parking', NULL),
(1154424, 'GWD', 'Fuse Volunteer', 'Production', NULL),
(1154412, 'GWD', 'Fuse Volunteer', 'Snack Bar', NULL),
(1154402, 'GWD', 'Fuse Volunteer', 'Sports', NULL),
(1154407, 'GWD', 'Fuse Volunteer', 'Sports', NULL),
(1310517, 'GWD', 'Fuse Volunteer', 'Sports', NULL),
(1154425, 'GWD', 'Fuse Volunteer', 'Spring Zone', NULL),
(1388943, 'GWD', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1103859, 'GWD', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1154413, 'GWD', 'Fuse Volunteer', 'Usher', NULL),
(1154415, 'GWD', 'Fuse Volunteer', 'VHQ', NULL),
(1154416, 'GWD', 'Fuse Volunteer', 'VIP Team', NULL),
(1259800, 'GWD', 'Fuse Volunteer', 'Worship', NULL),
(1300387, 'GWD', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1164462, 'GWD', 'Guest Services Attendee', 'Awake Team', NULL),
(872861, 'GWD', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(872863, 'GWD', 'Guest Services Attendee', 'Greeting Team', NULL),
(872894, 'GWD', 'Guest Services Attendee', 'Office Team', NULL),
(872870, 'GWD', 'Guest Services Attendee', 'Parking Team', NULL),
(872874, 'GWD', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274626, 'GWD', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474637, 'GWD', 'Guest Services Volunteer', 'Area Leader', NULL),
(872858, 'GWD', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1040960, 'GWD', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(872893, 'GWD', 'Guest Services Volunteer', 'Finance Office Team', NULL),
(1040973, 'GWD', 'Guest Services Volunteer', 'Finance Team', NULL),
(872864, 'GWD', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(872869, 'GWD', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239840, 'GWD', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1036953, 'GWD', 'Guest Services Volunteer', 'Service Leader', NULL),
(1342581, 'GWD', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(872873, 'GWD', 'Guest Services Volunteer', 'Usher Team', NULL),
(872862, 'GWD', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473795, 'GWD', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1473796, 'GWD', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1390973, 'GWD', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473797, 'GWD', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1390975, 'GWD', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1390976, 'GWD', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1283392, 'GWD', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390972, 'GWD', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1283397, 'GWD', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1214163, 'GWD', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1214164, 'GWD', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244666, 'GWD', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1283400, 'GWD', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1214165, 'GWD', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1214166, 'GWD', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233811, 'GWD', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1214159, 'GWD', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1214161, 'GWD', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244665, 'GWD', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1214156, 'GWD', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1214158, 'GWD', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238145, 'GWD', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143885, 'GWD', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143884, 'GWD', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244664, 'GWD', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1214162, 'GWD', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1283390, 'GWD', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1390974, 'GWD', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304035, 'GWD', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285774, 'GWD', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1283393, 'GWD', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1283398, 'GWD', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1283383, 'GWD', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1283382, 'GWD', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1283394, 'GWD', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232883, 'GWD', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1283399, 'GWD', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1283396, 'GWD', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1283384, 'GWD', 'Next Steps Volunteer', 'Group Leader', NULL),
(1283385, 'GWD', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1328869, 'GWD', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1283770, 'GWD', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1283388, 'GWD', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1283391, 'GWD', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1283381, 'GWD', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1283389, 'GWD', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285775, 'GWD', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1283380, 'GWD', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(872878, 'GWD', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(872875, 'GWD', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1162872, 'GWD', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 4'),
(1162869, 'GWD', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(872809, 'GWD', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(872811, 'GWD', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(872812, 'GWD', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1162885, 'GWD', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1162886, 'GWD', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(872808, 'GWD', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(872810, 'GWD', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(872793, 'GWD', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1162891, 'GWD', 'Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
(872794, 'GWD', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(872797, 'GWD', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(872792, 'GWD', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(872818, 'GWD', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1162892, 'GWD', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(872819, 'GWD', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(872820, 'GWD', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(872821, 'GWD', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(872822, 'GWD', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(872823, 'GWD', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(872825, 'GWD', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1443716, 'GWD', 'Production Volunteer', 'Elementary Production', NULL),
(872845, 'GWD', 'Production Volunteer', 'Elementary Production ', NULL),
(1443718, 'GWD', 'Production Volunteer', 'Elementary Production Service Leader', NULL),
(872846, 'GWD', 'Production Volunteer', 'Preschool Production', NULL),
(1443713, 'GWD', 'Production Volunteer', 'Preschool Production ', NULL),
(1443714, 'GWD', 'Production Volunteer', 'Preschool Production Service Leader', NULL),
(872847, 'GWD', 'Production Volunteer', 'Production Service Leader', NULL),
(872807, 'GWD', 'Special Needs Attendee', 'Spring Zone', NULL),
(1443719, 'GWD', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1443720, 'GWD', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(872848, 'GWD', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(872837, 'GWD', 'Support Volunteer', 'Advocate', NULL),
(872839, 'GWD', 'Support Volunteer', 'Check-In Volunteer', NULL),
(872841, 'GWD', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1443711, 'GWD', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(872842, 'GWD', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1040041, 'GWD', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1106575, 'GWD', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1443708, 'GWD', 'Support Volunteer', 'New Serve Area Leader', NULL),
(1443710, 'GWD', 'Support Volunteer', 'New Serve Team', NULL),
(872871, 'GWD', 'Support Volunteer', 'New Serve Team', NULL),
(1445842, 'HHD', 'Elementary Attendee', 'Base Camp', NULL),
(1445843, 'HHD', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1473798, 'HHD', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1473799, 'HHD', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1473800, 'HHD', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1290766, 'LEX', 'Creativity & Tech Volunteer', 'Band', NULL),
(1246845, 'LEX', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1289970, 'LEX', 'Creativity & Tech Attendee', 'Load In', NULL),
(1289971, 'LEX', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1289969, 'LEX', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1246846, 'LEX', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1340627, 'LEX', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1289915, 'LEX', 'Elementary Attendee', 'Base Camp', NULL),
(1289916, 'LEX', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1289917, 'LEX', 'Elementary Attendee', 'ImagiNation K', NULL),
(1289918, 'LEX', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1289919, 'LEX', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1289920, 'LEX', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1289921, 'LEX', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1289940, 'LEX', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1289946, 'LEX', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1289943, 'LEX', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(1289944, 'LEX', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1289947, 'LEX', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1289948, 'LEX', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1289949, 'LEX', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1310559, 'LEX', 'Fuse Attendee', '10th Grade Student', NULL),
(1310560, 'LEX', 'Fuse Attendee', '10th Grade Student', NULL),
(1310561, 'LEX', 'Fuse Attendee', '11th Grade Student', NULL),
(1310562, 'LEX', 'Fuse Attendee', '11th Grade Student', NULL),
(1310563, 'LEX', 'Fuse Attendee', '12th Grade Student', NULL),
(1310564, 'LEX', 'Fuse Attendee', '12th Grade Student', NULL),
(1310550, 'LEX', 'Fuse Attendee', '6th Grade Student', NULL),
(1310551, 'LEX', 'Fuse Attendee', '6th Grade Student', NULL),
(1310552, 'LEX', 'Fuse Attendee', '7th Grade Student', NULL),
(1310553, 'LEX', 'Fuse Attendee', '7th Grade Student', NULL),
(1310554, 'LEX', 'Fuse Attendee', '8th Grade Student', NULL),
(1310555, 'LEX', 'Fuse Attendee', '8th Grade Student', NULL),
(1310556, 'LEX', 'Fuse Attendee', '9th Grade Student', NULL),
(1310558, 'LEX', 'Fuse Attendee', '9th Grade Student', NULL),
(1348933, 'LEX', 'Fuse Volunteer', 'Atrium', NULL),
(1348935, 'LEX', 'Fuse Volunteer', 'Atrium', NULL),
(1310537, 'LEX', 'Fuse Volunteer', 'Atrium', NULL),
(1310533, 'LEX', 'Fuse Volunteer', 'Campus Safety', NULL),
(1310534, 'LEX', 'Fuse Volunteer', 'Campus Safety', NULL),
(1343610, 'LEX', 'Fuse Volunteer', 'Check-In', NULL),
(1348934, 'LEX', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1310535, 'LEX', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1310566, 'LEX', 'Fuse Volunteer', 'Fuse Office Team', NULL),
(1348936, 'LEX', 'Fuse Volunteer', 'Game Room', NULL),
(1310536, 'LEX', 'Fuse Volunteer', 'Greeter', NULL),
(1362004, 'LEX', 'Fuse Volunteer', 'Leadership Team', NULL),
(1310539, 'LEX', 'Fuse Volunteer', 'Load In', NULL),
(1343630, 'LEX', 'Fuse Volunteer', 'Load In', NULL),
(1310540, 'LEX', 'Fuse Volunteer', 'Lounge', NULL),
(1310541, 'LEX', 'Fuse Volunteer', 'New Serve', NULL),
(1362005, 'LEX', 'Fuse Volunteer', 'Next Steps', NULL),
(1310542, 'LEX', 'Fuse Volunteer', 'Parking', NULL),
(1310543, 'LEX', 'Fuse Volunteer', 'Pick-Up ', NULL),
(1310531, 'LEX', 'Fuse Volunteer', 'Production', NULL),
(1310544, 'LEX', 'Fuse Volunteer', 'Snack Bar', NULL),
(1343612, 'LEX', 'Fuse Volunteer', 'Sports', NULL),
(1310545, 'LEX', 'Fuse Volunteer', 'Sports', NULL),
(1362006, 'LEX', 'Fuse Volunteer', 'Sports', NULL),
(1289845, 'LEX', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1388944, 'LEX', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1310546, 'LEX', 'Fuse Volunteer', 'Usher', NULL),
(1310547, 'LEX', 'Fuse Volunteer', 'VHQ', NULL),
(1310548, 'LEX', 'Fuse Volunteer', 'VIP Team', NULL),
(1310532, 'LEX', 'Fuse Volunteer', 'Worship', NULL),
(1289844, 'LEX', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1390182, 'LEX', 'Guest Services Attendee', 'Awake Team', NULL),
(1267528, 'LEX', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1267530, 'LEX', 'Guest Services Attendee', 'Greeting Team', NULL),
(1289847, 'LEX', 'Guest Services Attendee', 'Load In', NULL),
(1289848, 'LEX', 'Guest Services Attendee', 'Load Out', NULL),
(1267521, 'LEX', 'Guest Services Attendee', 'Office Team', NULL),
(1267533, 'LEX', 'Guest Services Attendee', 'Parking Team', NULL),
(1267537, 'LEX', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274627, 'LEX', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474638, 'LEX', 'Guest Services Volunteer', 'Area Leader', NULL),
(1267527, 'LEX', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1267538, 'LEX', 'Guest Services Volunteer', 'Finance Team', NULL),
(1267532, 'LEX', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1289849, 'LEX', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1384270, 'LEX', 'Guest Services Volunteer', 'Service Leader', NULL),
(1267534, 'LEX', 'Guest Services Volunteer', 'Service Leader', NULL),
(1267535, 'LEX', 'Guest Services Volunteer', 'Sign Language Team', NULL),
(1491028, 'LEX', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1267536, 'LEX', 'Guest Services Volunteer', 'Usher Team', NULL),
(1267529, 'LEX', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473801, 'LEX', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1263119, 'LEX', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1390997, 'LEX', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473802, 'LEX', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1263117, 'LEX', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1263118, 'LEX', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1263115, 'LEX', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1473803, 'LEX', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1263105, 'LEX', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1263106, 'LEX', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1263107, 'LEX', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1263108, 'LEX', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1263110, 'LEX', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1263109, 'LEX', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1263112, 'LEX', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1263113, 'LEX', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1263114, 'LEX', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1263100, 'LEX', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1263101, 'LEX', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1263102, 'LEX', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1263104, 'LEX', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1390999, 'LEX', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1263131, 'LEX', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304036, 'LEX', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1263120, 'LEX', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1263149, 'LEX', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1263129, 'LEX', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1263125, 'LEX', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1263140, 'LEX', 'Next Steps Volunteer', 'Group Leader', NULL),
(1263144, 'LEX', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1301533, 'LEX', 'Next Steps Attendee', 'Load In', NULL),
(1301534, 'LEX', 'Next Steps Attendee', 'Load Out', NULL),
(1283772, 'LEX', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1263135, 'LEX', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1263132, 'LEX', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1263153, 'LEX', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1263136, 'LEX', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285777, 'LEX', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1263156, 'LEX', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1289906, 'LEX', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1289905, 'LEX', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1289907, 'LEX', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(1289924, 'LEX', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1289927, 'LEX', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1289928, 'LEX', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1289929, 'LEX', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1289930, 'LEX', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1289926, 'LEX', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1289925, 'LEX', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1289910, 'LEX', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1289912, 'LEX', 'Preschool Attendee', '36-37 mo.', 'SpringTown Toys'),
(1289913, 'LEX', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1289914, 'LEX', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1289931, 'LEX', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1289932, 'LEX', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1289933, 'LEX', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1289937, 'LEX', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1289935, 'LEX', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1289936, 'LEX', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1289938, 'LEX', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1289939, 'LEX', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1289957, 'LEX', 'Production Volunteer', 'Elementary Production ', NULL),
(1289958, 'LEX', 'Production Volunteer', 'Preschool Production', NULL),
(1289959, 'LEX', 'Production Volunteer', 'Production Area Leader', NULL),
(1289960, 'LEX', 'Production Volunteer', 'Production Service Leader', NULL),
(1289922, 'LEX', 'Special Needs Attendee', 'Spring Zone', NULL),
(1297161, 'LEX', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1289961, 'LEX', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1289962, 'LEX', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1289950, 'LEX', 'Support Volunteer', 'Advocate', NULL),
(1289951, 'LEX', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1289952, 'LEX', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1289953, 'LEX', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1289954, 'LEX', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1289955, 'LEX', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1289956, 'LEX', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1289968, 'LEX', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1289965, 'LEX', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1289963, 'LEX', 'Support Volunteer', 'Load In', NULL),
(1289964, 'LEX', 'Support Volunteer', 'Load Out', NULL),
(1289966, 'LEX', 'Support Volunteer', 'New Serve Team', NULL),
(1290767, 'MYR', 'Creativity & Tech Volunteer', 'Band', NULL),
(1104837, 'MYR', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1258663, 'MYR', 'Creativity & Tech Attendee', 'Load In', NULL),
(1258664, 'MYR', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1239833, 'MYR', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1342768, 'MYR', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1219525, 'MYR', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1164430, 'MYR', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(842063, 'MYR', 'Elementary Attendee', 'Base Camp', NULL),
(842064, 'MYR', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(842069, 'MYR', 'Elementary Attendee', 'ImagiNation K', NULL),
(842065, 'MYR', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(842066, 'MYR', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(842067, 'MYR', 'Elementary Attendee', 'Shockwave 4th', NULL),
(842068, 'MYR', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1197360, 'MYR', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1421596, 'MYR', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1405683, 'MYR', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(1405684, 'MYR', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(842083, 'MYR', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1200984, 'MYR', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1200986, 'MYR', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1098605, 'MYR', 'Fuse Attendee', '10th Grade Student', NULL),
(1098606, 'MYR', 'Fuse Attendee', '10th Grade Student', NULL),
(1098607, 'MYR', 'Fuse Attendee', '10th Grade Student', NULL),
(1098608, 'MYR', 'Fuse Attendee', '11th Grade Student', NULL),
(1098609, 'MYR', 'Fuse Attendee', '11th Grade Student', NULL),
(1098610, 'MYR', 'Fuse Attendee', '12th Grade Student', NULL),
(1098611, 'MYR', 'Fuse Attendee', '12th Grade Student', NULL),
(1098597, 'MYR', 'Fuse Attendee', '6th Grade Student', NULL),
(1098598, 'MYR', 'Fuse Attendee', '6th Grade Student', NULL),
(1098599, 'MYR', 'Fuse Attendee', '7th Grade Student', NULL),
(1098600, 'MYR', 'Fuse Attendee', '7th Grade Student', NULL),
(1098601, 'MYR', 'Fuse Attendee', '8th Grade Student', NULL),
(1098602, 'MYR', 'Fuse Attendee', '8th Grade Student', NULL),
(1098603, 'MYR', 'Fuse Attendee', '9th Grade Student', NULL),
(1098604, 'MYR', 'Fuse Attendee', '9th Grade Student', NULL),
(1364348, 'MYR', 'Fuse Volunteer', 'Atrium', NULL),
(1126284, 'MYR', 'Fuse Volunteer', 'Campus Safety', NULL),
(1126285, 'MYR', 'Fuse Volunteer', 'Campus Safety', NULL),
(1274587, 'MYR', 'Fuse Volunteer', 'Care', NULL),
(1126274, 'MYR', 'Fuse Volunteer', 'Check-In', NULL),
(1104824, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104829, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104825, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104826, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104827, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104828, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1098809, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104819, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104820, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1104822, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1126289, 'MYR', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1126276, 'MYR', 'Fuse Volunteer', 'Game Room', NULL),
(1126277, 'MYR', 'Fuse Volunteer', 'Greeter', NULL),
(1365227, 'MYR', 'Fuse Volunteer', 'Leadership Team', NULL),
(1271963, 'MYR', 'Fuse Volunteer', 'Leadership Team', NULL),
(1271751, 'MYR', 'Fuse Volunteer', 'Lounge', NULL),
(1271750, 'MYR', 'Fuse Volunteer', 'Lounge', NULL),
(1239824, 'MYR', 'Fuse Volunteer', 'New Serve', NULL),
(1147615, 'MYR', 'Fuse Volunteer', 'Office Team', NULL),
(1126278, 'MYR', 'Fuse Volunteer', 'Parking', NULL),
(1126279, 'MYR', 'Fuse Volunteer', 'Pick-Up', NULL),
(1126287, 'MYR', 'Fuse Volunteer', 'Production', NULL),
(1126280, 'MYR', 'Fuse Volunteer', 'Snack Bar', NULL),
(1126275, 'MYR', 'Fuse Volunteer', 'Sports', NULL),
(1365244, 'MYR', 'Fuse Volunteer', 'Sports', NULL),
(1126288, 'MYR', 'Fuse Volunteer', 'Spring Zone', NULL),
(1362008, 'MYR', 'Fuse Volunteer', 'Student Leader', NULL),
(1388945, 'MYR', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1088082, 'MYR', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1126281, 'MYR', 'Fuse Volunteer', 'Usher', NULL),
(1126282, 'MYR', 'Fuse Volunteer', 'VHQ', NULL),
(1104830, 'MYR', 'Fuse Volunteer', 'VIP Team', NULL),
(1259801, 'MYR', 'Fuse Volunteer', 'Worship', NULL),
(1300388, 'MYR', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1462144, 'MYR', 'Guest Services Attendee', 'Awake Team', NULL),
(842125, 'MYR', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(842126, 'MYR', 'Guest Services Attendee', 'Greeting Team', NULL),
(842129, 'MYR', 'Guest Services Attendee', 'Load In', NULL),
(842130, 'MYR', 'Guest Services Attendee', 'Load Out', NULL),
(842119, 'MYR', 'Guest Services Attendee', 'Office Team', NULL),
(1493512, 'MYR', 'Guest Services Attendee', 'Office Team', NULL),
(842131, 'MYR', 'Guest Services Attendee', 'Parking Team', NULL),
(842134, 'MYR', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274628, 'MYR', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474639, 'MYR', 'Guest Services Volunteer', 'Area Leader', NULL),
(842124, 'MYR', 'Guest Services Volunteer', 'Campus Safety', NULL),
(842122, 'MYR', 'Guest Services Volunteer', 'Finance Team', NULL),
(1040970, 'MYR', 'Guest Services Volunteer', 'Finance Team', NULL),
(1264772, 'MYR', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1239841, 'MYR', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1036951, 'MYR', 'Guest Services Volunteer', 'Service Leader', NULL),
(1342585, 'MYR', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1491029, 'MYR', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(842133, 'MYR', 'Guest Services Volunteer', 'Usher Team', NULL),
(1039818, 'MYR', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473804, 'MYR', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1283472, 'MYR', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1391002, 'MYR', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473805, 'MYR', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1283477, 'MYR', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1213999, 'MYR', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1214000, 'MYR', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244669, 'MYR', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1283480, 'MYR', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1391003, 'MYR', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473806, 'MYR', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1213995, 'MYR', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1213996, 'MYR', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233812, 'MYR', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1213993, 'MYR', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1213994, 'MYR', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244668, 'MYR', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1213997, 'MYR', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1213998, 'MYR', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238146, 'MYR', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143893, 'MYR', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143894, 'MYR', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244667, 'MYR', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1213992, 'MYR', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1283470, 'MYR', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1391004, 'MYR', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304037, 'MYR', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1391005, 'MYR', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285778, 'MYR', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1391006, 'MYR', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1283473, 'MYR', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1283478, 'MYR', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1283491, 'MYR', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1283490, 'MYR', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1283474, 'MYR', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1283479, 'MYR', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1283476, 'MYR', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1283483, 'MYR', 'Next Steps Volunteer', 'Group Leader', NULL),
(1283484, 'MYR', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1283771, 'MYR', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1283481, 'MYR', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1283471, 'MYR', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1283489, 'MYR', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1283482, 'MYR', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285779, 'MYR', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1283488, 'MYR', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1159891, 'MYR', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(842059, 'MYR', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1403998, 'MYR', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 4'),
(1326083, 'MYR', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(1405678, 'MYR', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(842073, 'MYR', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1159892, 'MYR', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1326091, 'MYR', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1404036, 'MYR', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1420937, 'MYR', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1405679, 'MYR', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(992086, 'MYR', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1404022, 'MYR', 'Preschool Attendee', '32-33 mo.', 'Lil'' Spring'),
(842062, 'MYR', 'Preschool Attendee', '42-43 mo.', 'SpringTown Toys'),
(1304932, 'MYR', 'Preschool Attendee', '54-55 mo.', 'Treehouse'),
(1074498, 'MYR', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1404064, 'MYR', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1404063, 'MYR', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(842078, 'MYR', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1421597, 'MYR', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1405680, 'MYR', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1405682, 'MYR', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1200649, 'MYR', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1304995, 'MYR', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(842089, 'MYR', 'Production Volunteer', 'Elementary Production', NULL),
(1420939, 'MYR', 'Production Volunteer', 'Production Area Leader', NULL),
(1420942, 'MYR', 'Production Volunteer', 'Production Service Leader', NULL),
(842070, 'MYR', 'Special Needs Attendee', 'Spring Zone', NULL),
(1404065, 'MYR', 'Special Needs Volunteer', 'Spring Zone Jr. Volunteer', NULL),
(976858, 'MYR', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(842086, 'MYR', 'Support Volunteer', 'Advocate', NULL),
(842087, 'MYR', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1443773, 'MYR', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1420946, 'MYR', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1420954, 'MYR', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1040042, 'MYR', 'Support Volunteer', 'KidSpring Greeter', NULL),
(842098, 'MYR', 'Support Volunteer', 'KidSpring Office Team', NULL),
(842094, 'MYR', 'Support Volunteer', 'Load In', NULL),
(842095, 'MYR', 'Support Volunteer', 'Load Out', NULL),
(842097, 'MYR', 'Support Volunteer', 'New Serve Team', NULL),
(1445844, 'NEC', 'Elementary Attendee', 'Base Camp', NULL),
(1445845, 'NEC', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1473807, 'NEC', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1473808, 'NEC', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1473809, 'NEC', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1440426, 'NEC', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1440422, 'NEC', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1465007, 'New', 'NewSpring College', 'Worship Leader ABCs', NULL),
(1378245, 'New', 'NewSpring College', 'All-Staff', NULL),
(1466469, 'New', 'NewSpring College', 'All-Staff', NULL),
(1465005, 'New', 'NewSpring College', 'Audio I', NULL),
(1465009, 'New', 'NewSpring College', 'Audio Lab I', NULL),
(1375136, 'New', 'NewSpring College', 'Builders & Shepherds', NULL),
(1347705, 'New', 'NewSpring College', 'Character Forum', NULL),
(1465002, 'New', 'NewSpring College', 'Children''s Ministry I', NULL),
(1347707, 'New', 'NewSpring College', 'Christian Beliefs I', NULL),
(1375103, 'New', 'NewSpring College', 'Christian Beliefs II', NULL),
(1347703, 'New', 'NewSpring College', 'Communication I', NULL),
(1375142, 'New', 'NewSpring College', 'Ephesians', NULL),
(1387993, 'New', 'NewSpring College', 'Events Team', NULL),
(1465004, 'New', 'NewSpring College', 'Health in Ministry II', NULL),
(1465003, 'New', 'NewSpring College', 'Health in Ministry I', NULL),
(1347715, 'New', 'NewSpring College', 'Leadership Forum', NULL),
(1375138, 'New', 'NewSpring College', 'Leadership I', NULL),
(1465006, 'New', 'NewSpring College', 'Life of Jesus', NULL),
(1387581, 'New', 'NewSpring College', 'Office Team', NULL),
(1347706, 'New', 'NewSpring College', 'Small Group', NULL),
(1465001, 'New', 'NewSpring College', 'Student Ministry Foundations I', NULL),
(1465008, 'New', 'NewSpring College', 'Student Ministry Lead Lab I', NULL),
(1347540, 'New', 'NewSpring College', 'Survey of the Bible I', NULL),
(1477140, 'New', 'NewSpring College', 'Survey of the Bible I', NULL),
(1347714, 'New', 'NewSpring College', 'Working Group', NULL),
(1335239, 'POW', 'Creativity & Tech Volunteer', 'Band', NULL),
(1335240, 'POW', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1335237, 'POW', 'Creativity & Tech Attendee', 'Load In', NULL),
(1335238, 'POW', 'Creativity & Tech Attendee', 'Load Out', NULL),
(1335235, 'POW', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1335234, 'POW', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1384261, 'POW', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1335236, 'POW', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1340633, 'POW', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1340631, 'POW', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1336796, 'POW', 'Elementary Attendee', 'Base Camp', NULL),
(1336797, 'POW', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1336799, 'POW', 'Elementary Attendee', 'ImagiNation K', NULL),
(1336800, 'POW', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1336804, 'POW', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1336807, 'POW', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1336809, 'POW', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1336828, 'POW', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1336830, 'POW', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1336834, 'POW', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(1336829, 'POW', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1336831, 'POW', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1336832, 'POW', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1336833, 'POW', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1368374, 'POW', 'Fuse Attendee', '10th Grade Student', NULL),
(1368375, 'POW', 'Fuse Attendee', '10th Grade Student', NULL),
(1368376, 'POW', 'Fuse Attendee', '11th Grade Student', NULL),
(1368377, 'POW', 'Fuse Attendee', '11th Grade Student', NULL),
(1368378, 'POW', 'Fuse Attendee', '12th Grade Student', NULL),
(1368379, 'POW', 'Fuse Attendee', '12th Grade Student', NULL),
(1368381, 'POW', 'Fuse Attendee', '6th Grade Student', NULL),
(1368382, 'POW', 'Fuse Attendee', '6th Grade Student', NULL),
(1368383, 'POW', 'Fuse Attendee', '7th Grade Student', NULL),
(1368384, 'POW', 'Fuse Attendee', '7th Grade Student', NULL),
(1368385, 'POW', 'Fuse Attendee', '8th Grade Student', NULL),
(1368386, 'POW', 'Fuse Attendee', '8th Grade Student', NULL),
(1368372, 'POW', 'Fuse Attendee', '9th Grade Student', NULL),
(1368373, 'POW', 'Fuse Attendee', '9th Grade Student', NULL),
(1368406, 'POW', 'Fuse Volunteer', 'Atrium', NULL),
(1368399, 'POW', 'Fuse Volunteer', 'Campus Safety', NULL),
(1368407, 'POW', 'Fuse Volunteer', 'Care', NULL),
(1368408, 'POW', 'Fuse Volunteer', 'Check-In', NULL),
(1368398, 'POW', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1368409, 'POW', 'Fuse Volunteer', 'Game Room', NULL),
(1368410, 'POW', 'Fuse Volunteer', 'Greeter', NULL),
(1368411, 'POW', 'Fuse Volunteer', 'Leadership Team', NULL),
(1368415, 'POW', 'Fuse Volunteer', 'Load In', NULL),
(1368412, 'POW', 'Fuse Volunteer', 'Lounge', NULL),
(1368413, 'POW', 'Fuse Volunteer', 'New Serve', NULL),
(1368414, 'POW', 'Fuse Volunteer', 'Next Steps', NULL),
(1384262, 'POW', 'Fuse Volunteer', 'Office Team', NULL),
(1368416, 'POW', 'Fuse Volunteer', 'Parking', NULL),
(1368419, 'POW', 'Fuse Volunteer', 'Pick-Up', NULL),
(1368400, 'POW', 'Fuse Volunteer', 'Production', NULL),
(1368420, 'POW', 'Fuse Volunteer', 'Snack Bar', NULL),
(1368421, 'POW', 'Fuse Volunteer', 'Sports', NULL),
(1368402, 'POW', 'Fuse Volunteer', 'Spring Zone', NULL),
(1368404, 'POW', 'Fuse Volunteer', 'Student Leader', NULL),
(1335220, 'POW', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1388946, 'POW', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1368422, 'POW', 'Fuse Volunteer', 'Usher', NULL),
(1368423, 'POW', 'Fuse Volunteer', 'VHQ', NULL),
(1368403, 'POW', 'Fuse Volunteer', 'VIP Team', NULL),
(1368401, 'POW', 'Fuse Volunteer', 'Worship', NULL),
(1335217, 'POW', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1335219, 'POW', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1335224, 'POW', 'Guest Services Attendee', 'Greeting Team', NULL),
(1335226, 'POW', 'Guest Services Attendee', 'Load In', NULL),
(1335227, 'POW', 'Guest Services Attendee', 'Load Out', NULL),
(1381563, 'POW', 'Guest Services Attendee', 'Office Team', NULL),
(1335229, 'POW', 'Guest Services Attendee', 'Parking Team', NULL),
(1335232, 'POW', 'Guest Services Attendee', 'VHQ Team', NULL),
(1335223, 'POW', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474651, 'POW', 'Guest Services Volunteer', 'Area Leader', NULL),
(1335218, 'POW', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1335233, 'POW', 'Guest Services Volunteer', 'Finance Team', NULL),
(1335225, 'POW', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1335228, 'POW', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1384272, 'POW', 'Guest Services Volunteer', 'Service Leader', NULL),
(1335230, 'POW', 'Guest Services Volunteer', 'Service Leader', NULL),
(1491030, 'POW', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1335231, 'POW', 'Guest Services Volunteer', 'Usher Team', NULL),
(1335221, 'POW', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473810, 'POW', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1391007, 'POW', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1335017, 'POW', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473811, 'POW', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1335038, 'POW', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1335039, 'POW', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1335036, 'POW', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1473812, 'POW', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1335046, 'POW', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1335047, 'POW', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1335048, 'POW', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1335043, 'POW', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1335044, 'POW', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1335045, 'POW', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1335040, 'POW', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1335041, 'POW', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1335042, 'POW', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1335049, 'POW', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1335050, 'POW', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1335052, 'POW', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1335054, 'POW', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1335028, 'POW', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1391009, 'POW', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1335029, 'POW', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1391010, 'POW', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1335018, 'POW', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1335020, 'POW', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1335007, 'POW', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1335008, 'POW', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1335053, 'POW', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1335021, 'POW', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1335023, 'POW', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1335024, 'POW', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1335012, 'POW', 'Next Steps Volunteer', 'Group Leader', NULL),
(1335014, 'POW', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1335026, 'POW', 'Next Steps Attendee', 'Load In', NULL),
(1335027, 'POW', 'Next Steps Attendee', 'Load Out', NULL),
(1335005, 'POW', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1335015, 'POW', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1335030, 'POW', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1335009, 'POW', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1335016, 'POW', 'Next Steps Volunteer', 'Resource Center', NULL),
(1335031, 'POW', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1335010, 'POW', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1335006, 'POW', 'Next Steps Volunteer', 'Writing Team', NULL),
(1336790, 'POW', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1336789, 'POW', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1336791, 'POW', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(1336815, 'POW', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1336818, 'POW', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1336819, 'POW', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1336820, 'POW', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1336817, 'POW', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1336816, 'POW', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1336792, 'POW', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1336793, 'POW', 'Preschool Attendee', '36-37 mo.', 'SpringTown Toys'),
(1336794, 'POW', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1473716, 'POW', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1336795, 'POW', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1336822, 'POW', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1336823, 'POW', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1336824, 'POW', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1336825, 'POW', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1405828, 'POW', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1336826, 'POW', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1336827, 'POW', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1336850, 'POW', 'Production Volunteer', 'Elementary Production ', NULL),
(1336851, 'POW', 'Production Volunteer', 'Preschool Production', NULL),
(1407551, 'POW', 'Production Volunteer', 'Preschool Production ', NULL),
(1336852, 'POW', 'Production Volunteer', 'Production Area Leader', NULL),
(1336853, 'POW', 'Production Volunteer', 'Production Service Leader', NULL),
(1336812, 'POW', 'Special Needs Attendee', 'Spring Zone', NULL),
(1336841, 'POW', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL)

insert #rlcMap 
values
(1336835, 'POW', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1336840, 'POW', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1336843, 'POW', 'Support Volunteer', 'Advocate', NULL),
(1336844, 'POW', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1336845, 'POW', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1336846, 'POW', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1336847, 'POW', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1405827, 'POW', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1336848, 'POW', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1336849, 'POW', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1384263, 'POW', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1336857, 'POW', 'Support Volunteer', 'Load In', NULL),
(1336858, 'POW', 'Support Volunteer', 'Load Out', NULL),
(1336860, 'POW', 'Support Volunteer', 'New Serve Team', NULL),
(1445846, 'RKH', 'Elementary Attendee', 'Base Camp', NULL),
(1445847, 'RKH', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1468903, 'SIM', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1445848, 'SIM', 'Elementary Attendee', 'Base Camp', NULL),
(1445849, 'SIM', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1450446, 'SIM', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1473817, 'SIM', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1473818, 'SIM', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1473819, 'SIM', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1440736, 'SIM', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1290768, 'SPA', 'Creativity & Tech Volunteer', 'Band', NULL),
(1104838, 'SPA', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1507034, 'SPA', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1258661, 'SPA', 'Creativity & Tech Attendee', 'Load In', NULL),
(1239834, 'SPA', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1100586, 'SPA', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1300448, 'SPA', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1219527, 'SPA', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1068650, 'SPA', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1068649, 'SPA', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(872719, 'SPA', 'Elementary Attendee', 'Base Camp', NULL),
(872720, 'SPA', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(872721, 'SPA', 'Elementary Attendee', 'ImagiNation K', NULL),
(872722, 'SPA', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(872723, 'SPA', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(872724, 'SPA', 'Elementary Attendee', 'Shockwave 4th', NULL),
(872725, 'SPA', 'Elementary Attendee', 'Shockwave 5th', NULL),
(872750, 'SPA', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1190757, 'SPA', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(872752, 'SPA', 'Elementary Volunteer', 'Elementary Early Bird Volunteer', NULL),
(872753, 'SPA', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(872754, 'SPA', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(872755, 'SPA', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(952366, 'SPA', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1024122, 'SPA', 'Fuse Attendee', '10th Grade Student', NULL),
(1024123, 'SPA', 'Fuse Attendee', '10th Grade Student', NULL),
(1024127, 'SPA', 'Fuse Attendee', '11th Grade Student', NULL),
(1024129, 'SPA', 'Fuse Attendee', '11th Grade Student', NULL),
(1024131, 'SPA', 'Fuse Attendee', '12th Grade Student', NULL),
(1024132, 'SPA', 'Fuse Attendee', '12th Grade Student', NULL),
(1024133, 'SPA', 'Fuse Attendee', '6th Grade Student', NULL),
(1024134, 'SPA', 'Fuse Attendee', '6th Grade Student', NULL),
(1024135, 'SPA', 'Fuse Attendee', '7th Grade Student', NULL),
(1024136, 'SPA', 'Fuse Attendee', '7th Grade Student', NULL),
(1024137, 'SPA', 'Fuse Attendee', '8th Grade Student', NULL),
(1024138, 'SPA', 'Fuse Attendee', '8th Grade Student', NULL),
(1024124, 'SPA', 'Fuse Attendee', '9th Grade Student', NULL),
(1024125, 'SPA', 'Fuse Attendee', '9th Grade Student', NULL),
(1364349, 'SPA', 'Fuse Volunteer', 'Atrium', NULL),
(1024086, 'SPA', 'Fuse Volunteer', 'Campus Safety', NULL),
(1024081, 'SPA', 'Fuse Volunteer', 'Campus Safety', NULL),
(1362015, 'SPA', 'Fuse Volunteer', 'Care', NULL),
(1023364, 'SPA', 'Fuse Volunteer', 'Check-In', NULL),
(1024103, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024105, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024107, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024108, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024109, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024110, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024111, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024112, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024114, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024118, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024119, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1024082, 'SPA', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1362016, 'SPA', 'Fuse Volunteer', 'Game Room', NULL),
(1023359, 'SPA', 'Fuse Volunteer', 'Greeter', NULL),
(1388947, 'SPA', 'Fuse Volunteer', 'Leadership Team', NULL),
(1362017, 'SPA', 'Fuse Volunteer', 'Leadership Team', NULL),
(1024095, 'SPA', 'Fuse Volunteer', 'Lounge', NULL),
(1239825, 'SPA', 'Fuse Volunteer', 'New Serve', NULL),
(872181, 'SPA', 'Fuse Volunteer', 'Office Team', NULL),
(1023361, 'SPA', 'Fuse Volunteer', 'Parking', NULL),
(1023366, 'SPA', 'Fuse Volunteer', 'Pick-Up', NULL),
(1024084, 'SPA', 'Fuse Volunteer', 'Production', NULL),
(1024090, 'SPA', 'Fuse Volunteer', 'Snack Bar', NULL),
(1024097, 'SPA', 'Fuse Volunteer', 'Sports', NULL),
(1023358, 'SPA', 'Fuse Volunteer', 'Sports', NULL),
(1362024, 'SPA', 'Fuse Volunteer', 'Spring Zone', NULL),
(1362022, 'SPA', 'Fuse Volunteer', 'Student Leader', NULL),
(1088084, 'SPA', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1023363, 'SPA', 'Fuse Volunteer', 'Usher', NULL),
(1024077, 'SPA', 'Fuse Volunteer', 'VHQ', NULL),
(1024087, 'SPA', 'Fuse Volunteer', 'VIP Team', NULL),
(1024140, 'SPA', 'Fuse Volunteer', 'VIP Team', NULL),
(1024142, 'SPA', 'Fuse Volunteer', 'VIP Team', NULL),
(1259802, 'SPA', 'Fuse Volunteer', 'Worship', NULL),
(1197512, 'SPA', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1267382, 'SPA', 'Guest Services Attendee', 'Awake Team', NULL),
(871837, 'SPA', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(871839, 'SPA', 'Guest Services Attendee', 'Greeting Team', NULL),
(871853, 'SPA', 'Guest Services Attendee', 'Office Team', NULL),
(871845, 'SPA', 'Guest Services Attendee', 'Parking Team', NULL),
(871848, 'SPA', 'Guest Services Attendee', 'VHQ Team', NULL),
(1274629, 'SPA', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1474653, 'SPA', 'Guest Services Volunteer', 'Area Leader', NULL),
(871836, 'SPA', 'Guest Services Volunteer', 'Campus Safety', NULL),
(871852, 'SPA', 'Guest Services Volunteer', 'Finance Team', NULL),
(1040972, 'SPA', 'Guest Services Volunteer', 'Finance Team', NULL),
(871840, 'SPA', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1135970, 'SPA', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1384273, 'SPA', 'Guest Services Volunteer', 'Service Leader', NULL),
(1036952, 'SPA', 'Guest Services Volunteer', 'Service Leader', NULL),
(1342589, 'SPA', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1491031, 'SPA', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(871847, 'SPA', 'Guest Services Volunteer', 'Usher Team', NULL),
(871838, 'SPA', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1473820, 'SPA', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1242949, 'SPA', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1391012, 'SPA', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473821, 'SPA', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1242954, 'SPA', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1211284, 'SPA', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1211287, 'SPA', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1244672, 'SPA', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1242956, 'SPA', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1391013, 'SPA', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473822, 'SPA', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1211277, 'SPA', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1211278, 'SPA', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1233814, 'SPA', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1211281, 'SPA', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1211282, 'SPA', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1244671, 'SPA', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1211279, 'SPA', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1211280, 'SPA', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1238147, 'SPA', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1143904, 'SPA', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1143905, 'SPA', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1244670, 'SPA', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1211276, 'SPA', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1242946, 'SPA', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1391014, 'SPA', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1304038, 'SPA', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1391015, 'SPA', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1285780, 'SPA', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1391016, 'SPA', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1242947, 'SPA', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1242953, 'SPA', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1242979, 'SPA', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1242974, 'SPA', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1242973, 'SPA', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1242950, 'SPA', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1232885, 'SPA', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1242955, 'SPA', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1242952, 'SPA', 'Next Steps Volunteer', 'Financial Coaching Office Team', NULL),
(1242963, 'SPA', 'Next Steps Volunteer', 'Group Leader', NULL),
(1242964, 'SPA', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1283768, 'SPA', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1242960, 'SPA', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1242945, 'SPA', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1242971, 'SPA', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1242961, 'SPA', 'Next Steps Volunteer', 'Resource Center', NULL),
(1285781, 'SPA', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1242970, 'SPA', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1242975, 'SPA', 'Next Steps Volunteer', 'Writing Team', NULL),
(872784, 'SPA', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(872782, 'SPA', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1081247, 'SPA', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 4'),
(1413277, 'SPA', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 5'),
(872786, 'SPA', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 3'),
(872730, 'SPA', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(872732, 'SPA', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(872733, 'SPA', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(872734, 'SPA', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1081249, 'SPA', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1413356, 'SPA', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(1190759, 'SPA', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(872731, 'SPA', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(872713, 'SPA', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1413278, 'SPA', 'Preschool Attendee', '24-29 mo.', 'Spring Fresh'),
(1081248, 'SPA', 'Preschool Attendee', '36-37 mo.', 'Lil'' Spring'),
(872717, 'SPA', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(872718, 'SPA', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1073454, 'SPA', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1073455, 'SPA', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1081250, 'SPA', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(872742, 'SPA', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1190758, 'SPA', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(872744, 'SPA', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(872745, 'SPA', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1413357, 'SPA', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(872746, 'SPA', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(872749, 'SPA', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(872767, 'SPA', 'Production Volunteer', 'Elementary Production ', NULL),
(872769, 'SPA', 'Production Volunteer', 'Preschool Production', NULL),
(1190764, 'SPA', 'Production Volunteer', 'Production Area Leader', NULL),
(872770, 'SPA', 'Production Volunteer', 'Production Service Leader', NULL),
(872727, 'SPA', 'Special Needs Attendee', 'Spring Zone', NULL),
(930573, 'SPA', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(872771, 'SPA', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(872757, 'SPA', 'Support Volunteer', 'Advocate', NULL),
(1190763, 'SPA', 'Support Volunteer', 'Check-In Volunteer', NULL),
(872764, 'SPA', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1190762, 'SPA', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(872765, 'SPA', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1190765, 'SPA', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(872766, 'SPA', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(928181, 'SPA', 'Support Volunteer', 'KidSpring Greeter', NULL),
(872292, 'SPA', 'Support Volunteer', 'KidSpring Office Team', NULL),
(872781, 'SPA', 'Support Volunteer', 'New Serve Team', NULL),
(872780, 'SPA', 'Support Volunteer', 'Sunday Support Volunteer ', NULL),
(1445850, 'SUM', 'Elementary Attendee', 'Base Camp', NULL),
(1445851, 'SUM', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1450447, 'SUM', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1473823, 'SUM', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1473824, 'SUM', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1473825, 'SUM', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1440775, 'SUM', 'Next Steps Attendee', 'Ownership Class Attendee', NULL)



/* ====================================================== */
-- Start RLC loop
/* ====================================================== */

declare @scopeIndex int, @numItems int
declare @GroupTypeName nvarchar(255), @GroupName nvarchar(255), @GroupLocation nvarchar(255), @GroupMemberId bigint, @IsBreakoutTag bit,
	@JobTitle nvarchar(255), @ScheduleName nvarchar(255), @CampusName nvarchar(255), @CampusCode nvarchar(255), @CampusGuid uniqueidentifier,
	@JobId bigint, @GroupRoleId bigint, @BreakoutGroup nvarchar(255), @CampusAssignmentId bigint, @LocationName nvarchar(255), @ParentTypeName nvarchar(255)

select @scopeIndex = min(ID) from #rlcMap
select @numItems = count(1) + @scopeIndex from #rlcMap

while @scopeIndex <= @numItems
begin
	
	select @RLCID = null, @CampusCode = '', @CampusName = '', @GroupTypeName = '', @GroupName = '', @GroupTypeId = null, @ParentTypeName = null,
		@GroupId = null, @CampusId = null, @CampusGuid = null, @LocationId = null, @CampusAssignmentId = null, @LocationName = null

	select @RLCID = RLC_ID, @CampusCode = Code, @GroupTypeName = GroupType, @GroupName = GroupName, @LocationName = LocationName
	from #rlcMap where ID = @scopeIndex
	
	declare @msg nvarchar(500)
	select @msg = 'Starting ' + @CampusCode + ' / ' + @GroupTypeName + ' / ' + @GroupName + ' (' + ltrim(str(@RLCID, 25, 0)) + ')'
	RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
	
	select @CampusId = [Id], @CampusName = [Name], @CampusGuid = [Guid]
	from [Campus]
	where ShortCode = @CampusCode

	select @GroupTypeId = ID 
	from [GroupType]
	where name = @GroupTypeName

	select @ParentTypeName = gt.Name
	from [GroupTypeAssociation] gta
	inner join [GroupType] gt
	on gta.GroupTypeId = gt.id
	and gta.GroupTypeId <> @GroupTypeId
	and gta.ChildGroupTypeId = @GroupTypeId

	select @GroupId = ID
	from [Group]
	where GroupTypeId = @GroupTypeId
	and Name = @GroupName

	if @LocationName is null
	begin
		set @LocationName = @GroupName
	end

	;with locationChildren as (
		select l.id, l.parentLocationId, l.name, l.name as 'ParentName'
		from location l
		where name = @CampusName
		union all 
		select l2.id, l2.parentlocationId, l2.name, lc.name as 'ParentName'
		from location l2
		inner join locationChildren lc
		on lc.id = l2.ParentLocationId				
	)
	select @LocationId = Id from locationChildren
	where Name = @LocationName
	and ParentName = @ParentTypeName
	
	if @GroupId is not null and @LocationId is not null
	begin
		
		/* ====================================================== */
		-- Create attendances that match this RLC
		-- Note: Attendance is tied to Person Alias Id
		/* ====================================================== */
		insert [Attendance] (LocationId, GroupId, SearchTypeValueId, StartDateTime, 
			DidAttend, Note, [Guid], CreatedDateTime, CampusId, PersonAliasId, RSVP, ScheduleId)
		select @LocationId, @GroupId, @NameSearchValueId, ISNULL(Check_In_Time, Start_Date_Time), @True,
			 Tag_Comment, NEWID(), Check_In_Time, @CampusId, p.Id, @False, 
			 CASE
				WHEN @GroupTypeName like 'Fuse%' THEN @FuseScheduleId
				WHEN s2.serviceId IS NOT NULL THEN s2.serviceId
				ELSE s.serviceId
			END as ScheduleId
		from [CEN-SQLDEV001].F1.dbo.Attendance a
		inner join PersonAlias p
			on a.Individual_ID = p.ForeignId
			and a.RLC_ID = @RLCID
		left join #services s
			on s.serviceName = DATENAME(DW, a.Start_Date_Time)
		left join #services s2
			on s2.serviceTime = CONVERT(TIME, a.Start_Date_time) AND s2.serviceName LIKE CONCAT(DATENAME(DW, a.Start_Date_Time), '%')

		select @JobTitle = null, @JobId = null, @PersonId = null, @ScheduleName = null, 
			@GroupRoleId = null, @GroupMemberId = null, @ScheduleAttributeId = null,
			@CampusAttributeId = null, @TeamConnectorAttributeId = null, @BreakoutGroup = null

		/* ====================================================== */
		-- Create campus group member attribute
		/* ====================================================== */
		select @CampusAttributeId = Id from Attribute where EntityTypeId = @GroupMemberEntityId
			and EntityTypeQualifierValue = @GroupTypeId and [Key] = 'Campus'

		if @CampusAttributeId is null
		begin

			insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
				[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
			select @IsSystem, @CampusFieldTypeId, @GroupMemberEntityId, 'GroupTypeId',  @GroupTypeId, 'Campus', 
				'Campus', 'This group member''s campus.', '', @Order, @True, @False, @True, NEWID()

			select @CampusAttributeId = SCOPE_IDENTITY()			
		end

		/* ====================================================== */
		-- Create team connector group member attribute
		/* ====================================================== */
		select @TeamConnectorAttributeId = Id from Attribute where EntityTypeId = @GroupMemberEntityId
			and EntityTypeQualifierValue = @GroupTypeId and Name = 'Team Connector'
		if @TeamConnectorAttributeId is null
		begin
			insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
				[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
			select @IsSystem, @DefinedValueFieldTypeId, @GroupMemberEntityId, 'GroupTypeId',  @GroupTypeId, 'TeamConnector', 
				'Team Connector', 'The team connector for this group member.', '', @Order, @True, @True, @False, NEWID()

			select @TeamConnectorAttributeId = SCOPE_IDENTITY()

			insert AttributeQualifier ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
			select @IsSystem, @TeamConnectorAttributeId, 'definedtype', @TeamConnectorTypeId, NEWID()

			insert AttributeQualifier ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
			select @IsSystem, @TeamConnectorAttributeId, 'allowmultiple', 'False', NEWID()

			insert AttributeQualifier ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
			select @IsSystem, @TeamConnectorAttributeId, 'displaydescription', 'False', NEWID()
		end
		
		/* ====================================================== */
		-- Create schedule group member attribute
		/* ====================================================== */
		select @ScheduleAttributeId = Id from Attribute where EntityTypeId = @GroupMemberEntityId
			and EntityTypeQualifierValue = @GroupTypeId and Name = 'Schedule'
		if @ScheduleAttributeId is null
		begin
			insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
				[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
			select @IsSystem, @DefinedValueFieldTypeId, @GroupMemberEntityId, 'GroupTypeId',  @GroupTypeId, 'Schedule', 
				'Schedule', 'The schedule(s) assigned to this group member.', '', @Order, @True, @True, @False, NEWID()

			select @ScheduleAttributeId = SCOPE_IDENTITY()

			insert AttributeQualifier ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
			select @IsSystem, @ScheduleAttributeId, 'definedtype', @ScheduleDefinedTypeId, NEWID()

			insert AttributeQualifier ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
			select @IsSystem, @ScheduleAttributeId, 'allowmultiple', 'True', NEWID()

			insert AttributeQualifier ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
			select @IsSystem, @ScheduleAttributeId, 'displaydescription', 'False', NEWID()
		end

		/* ====================================================== */
		-- Create group member roles from assignments
		-- Note: Group member is tied to Person Id
		/* ====================================================== */
		insert into #assignments
		select JobID, Job_Title, p.PersonId, Staffing_Schedule_Name, 'TC ' + ltrim(rtrim(right(BreakoutGroupName, len(BreakoutGroupName) - charindex('TC', BreakoutGroupName) - 1))), @False
		from [CEN-SQLDEV001].F1.dbo.Staffing_Assignment sa
		inner join PersonAlias p
		on sa.Individual_ID = p.ForeignId
		and RLC_ID = @RLCID and Is_Active = @True

		insert into #assignments
		select NULL, '', p.PersonId, '', BreakoutGroupName, @True
		from [CEN-SQLDEV001].F1.dbo.ActivityAssignment aa
		inner join PersonAlias p
		on aa.Individual_ID = p.ForeignId
		and RLC_ID = @RLCID
		-- ignore fuse attendee because dirty data
		and aa.activity_name <> 'Fuse Attendee'

		declare @childIndex int, @childItems int
		select @childIndex = min(ID) from #assignments
		select @childItems = count(1) + @childIndex from #assignments

		while @childIndex <= @childItems
		begin

			select @JobId = JobID, @JobTitle = JobTitle, @PersonId = PersonID, @ScheduleName = ScheduleName, @BreakoutGroup = BreakoutGroup, @IsBreakoutTag = IsBreakoutTag
			from #assignments where ID = @childIndex

			if @PersonId is not null
			begin
				-- Lookup or create the title as a role 
				select @GroupRoleId = Id from [GroupTypeRole] where GroupTypeId = @GroupTypeId and Name = @JobTitle
				if @GroupRoleId is null
				begin
					-- Title is empty, just use the default group role
					if @JobTitle is null or @jobTitle = ''
					begin
						select @JobTitle = 'Member'
						select @GroupRoleId = Id from [GroupTypeRole] where GroupTypeId = @GroupTypeId and Name = 'Member'
					end

					-- Create a new role with the job title
					if @GroupRoleId is null
					begin
						insert GroupTypeRole ([IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], 
							[Guid], [ForeignId], [CanView], [CanEdit])
						select @IsSystem, @GroupTypeId, @JobTitle, @Order, @False, NEWID(), @JobId, @True, @False

						select @GroupRoleId = SCOPE_IDENTITY()
					end
				end
				-- end lookup/create role
								
				select @GroupMemberId = Id from GroupMember where GroupId = @GroupId 
					and GroupRoleId = @GroupRoleId and PersonId = @PersonId

				-- Create group member with role if it doesn't exist
				if @GroupMemberId is null and @GroupRoleId is not null
				begin
					
					insert GroupMember (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, [Guid], ForeignId)
					select @IsSystem, @GroupId, @PersonId, @GroupRoleId, @True, NEWID(), @JobId

					select @GroupMemberId = SCOPE_IDENTITY()
				end
				
				-- Create campus attribute if it doesn't already exist
				select @CampusAssignmentId = Id from AttributeValue
				where AttributeId = @CampusAttributeId and EntityId = @GroupMemberId
				
				if @CampusAssignmentId is null
				begin
					insert AttributeValue ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
					select @IsSystem, @CampusAttributeId, @GroupMemberId, @CampusGuid, NEWID()
				end

				-- Create schedule attribute
				if @ScheduleName is not null and @ScheduleName <> ''
				begin
					declare @currentSchedule nvarchar(255) = null, @newSchedule nvarchar(255) = null
					select @newSchedule = dvGuid from #schedules where scheduleF1 = @ScheduleName

					if @newSchedule is not null
					begin
						select @currentSchedule = Value from AttributeValue where AttributeId = @ScheduleAttributeId
							and EntityId = @GroupMemberId
						if @currentSchedule is null
						begin
							insert AttributeValue ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
							select @IsSystem, @ScheduleAttributeId, @GroupMemberId, @newSchedule, NEWID()
						end
						else 
						begin
							update AttributeValue
							set Value = convert(nvarchar(255), @newSchedule) + ',' + convert(nvarchar(255), @currentSchedule)
							from AttributeValue where AttributeId = @ScheduleAttributeId and EntityId = @GroupMemberId
						end
					end
				end

				-- Create breakout attribute
				if @BreakoutGroup is not null and @BreakoutGroup <> ''
				begin
					-- this is an attendee breakout, create it as a person attribute
					if @IsBreakoutTag = @True
					begin

						declare @currentTag nvarchar(250) = null
						select @currentTag = Value from AttributeValue where AttributeId = @BreakoutGroupAttributeId
							and EntityId = @PersonId

						if @currentTag is null
						begin
							insert AttributeValue ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
							select @IsSystem, @BreakoutGroupAttributeId, @PersonId, @BreakoutGroup, NEWID()
						end
						else 
						begin
							update AttributeValue
							set Value = convert(nvarchar(255), @BreakoutGroup) + ',' + convert(nvarchar(255), @currentTag)
							from AttributeValue where AttributeId = @BreakoutGroupAttributeId and EntityId = @PersonId
						end
					end
					-- this is a volunteer breakout, assign them a team connector
					else 
					begin
						declare @currentConnector nvarchar(255) = null, @newConnector nvarchar(255) = null
						select @newConnector = [Guid] from DefinedValue where DefinedTypeId = @TeamConnectorTypeId 
							and Value = @BreakoutGroup

						if @newConnector is not null
						begin
							select @currentConnector = Value from AttributeValue where AttributeId = @TeamConnectorAttributeId
								and EntityId = @GroupMemberId
							if @currentConnector is null
							begin
								insert AttributeValue ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
								select @IsSystem, @TeamConnectorAttributeId, @GroupMemberId, @newConnector, NEWID()
							end
							else 
							begin
								update AttributeValue
								set Value = convert(nvarchar(255), @newConnector) + ',' + convert(nvarchar(255), @currentConnector)
								from AttributeValue where AttributeId = @TeamConnectorAttributeId and EntityId = @GroupMemberId
							end
						end
					end
				end
				-- end breakout group 

				select @JobId = null, @JobTitle = null, @PersonId = null, @ScheduleName = null, 
					@GroupRoleId = null, @GroupMemberId = null, @BreakoutGroup = null, @CampusAssignmentId = null
			end
			-- end personId not null

			set @childIndex = @childIndex + 1
		end
		-- end child items loop

		delete from #assignments
	end
	else begin
		
		select @msg = 'Could not find Group ID for ' + @GroupTypeName + ' / ' + @GroupName + ' / ' + @LocationName + ' (' + ltrim(str(@RLCID, 25, 0)) + ')'
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

	end
	-- end groupId not null

	set @scopeIndex = @scopeIndex + 1
end
-- end rlc loop

RAISERROR ( N'Completed successfully.', 0, 0 ) WITH NOWAIT

USE [master]


/* ====================================================== 


/* ====================================================
// look for missing groups
==================================================== */
select distinct grouptype, groupname
from #rlcMap
where grouptype + groupname not in 
(
	select gt.name + g.name
	from [group] g
	inner join grouptype gt
	on g.grouptypeid = gt.id
)
order by grouptype, groupname

select code, groupname, count(1) 
from #rlcmap
group by code, groupname
having count(1) > 1
order by count(1) desc

/* ====================================================
// look for missing locations
==================================================== */
;with locationChildren as (
	select l.id, l.parentLocationId, l.name, l.name as 'ParentName'
	from location l
	where name = 'greenville'
	union all 
	select l2.id, l2.parentlocationId, l2.name, lc.name as 'ParentName'
	from location l2
	inner join locationChildren lc
	on lc.id = l2.ParentLocationId				
)
--select * from locationChildren
select '(''' + grouptype + ''', ''' + groupname + '''),' from #rlcmap r
--select * from #rlcmap r
left join locationChildren lc
on isnull(r.locationname, r.groupname) = lc.name
and (replace(replace(r.grouptype, ' Attendee', ''), ' Volunteer', '') = lc.parentname or lc.parentname = 'KidSpring')
where r.code = 'gvl'
and lc.id is null
order by grouptype, groupname



/* ====================================================
// script to insert new locations that are missing
==================================================== */

drop table #newLocations

create table #newLocations ( id int IDENTITY(1, 1), groupType nvarchar(100) null, groupName nvarchar(100) null, parentLocation nvarchar(100) null)

insert #newLocations
values
--('Guest Services Attendee', 'Special Event Attendee', 'Guest Services'),
--('Guest Services Volunteer', 'Receptionist', 'Guest Services')


declare @scopeIndex int, @numItems int
declare @GroupTypeName nvarchar(255), @GroupName nvarchar(255), @ParentLocation nvarchar(255), @msg nvarchar(max),
	@GroupId int, @LocationId int, @True int = 1, @False int = 0

select @scopeIndex = min(ID) from #newLocations
select @numItems = count(1) + @scopeIndex from #newLocations

while @scopeIndex <= @numItems
begin
	
	select @GroupTypeName = groupType, @GroupName = groupName, @ParentLocation = parentLocation
	from #newLocations
	where id = @scopeIndex 

	if @GroupTypeName is not null and @GroupName is not null
	begin
		
		select @GroupId = G.[ID] 
		from [Group] G
		inner join [GroupType] GT
		on G.GroupTypeId = GT.Id
		where G.Name = @GroupName
		and GT.Name = @GroupTypeName

		select @msg = 'Starting ' + @GroupTypeName + ' / ' + @GroupName + ' / ' + @ParentLocation
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

		-- check for existing location
		SELECT @LocationId = l.[Id]
		FROM Location l
		INNER JOIN Location l2
		ON l.ParentLocationId = l2.Id
		WHERE l.name = @GroupName
		AND l2.name = @ParentLocation

		IF @LocationId IS NULL AND @GroupName <> ''
		BEGIN

			INSERT Location (ParentLocationId, Name, IsActive, [Guid])
			SELECT l.Id, @GroupName, 1, NEWID()
			FROM Location l
			INNER JOIN Campus c
			ON c.LocationId = l.ParentLocationId
			AND l.Name = @ParentLocation
			WHERE c.IsActive = @True

		END

		-- insert group location
		INSERT GroupLocation (Groupid, LocationId, IsMailingLocation, IsMappedLocation, [Guid])
		SELECT @GroupId, l.Id, @False, @False, NEWID()
		FROM Location l
		INNER JOIN Location pl
		ON l.ParentLocationId = pl.Id
		AND pl.Name = @ParentLocation
		AND l.name = @GroupName

	end
	else begin
		
		select @msg = 'Could not find Group ID for ' + @GroupTypeName + ' / ' + @GroupName + ' / ' + @ParentLocation
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

	end

	select @GroupTypeName = null, @GroupName = null, @ParentLocation = null, @GroupId = null, @LocationId = null

	select @scopeIndex = @scopeIndex +1
end

 ====================================================== */

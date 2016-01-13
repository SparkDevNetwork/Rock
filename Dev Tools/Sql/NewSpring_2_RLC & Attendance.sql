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
from F1..Staffing_Assignment

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
(1098338, 'AKN', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1062481, 'AND', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1249711, 'AND', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1258670, 'AND', 'Creativity & Tech Attendee', 'Choir', NULL),
(1239826, 'AND', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(1219466, 'AND', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1340597, 'AND', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1062480, 'AND', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1062479, 'AND', 'Creativity & Tech Volunteer', 'Web Dev Team', NULL),
(1068550, 'AND', 'Creativity & Tech Volunteer', 'Design Team', NULL),
(1068549, 'AND', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1290760, 'AND', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(797125, 'AND', 'Creativity & Tech Volunteer', 'Band', NULL),
(797126, 'AND', 'Fuse Attendee', '10th Grade Student', NULL),
(797128, 'AND', 'Fuse Attendee', '10th Grade Student', NULL),
(797127, 'AND', 'Fuse Attendee', '11th Grade Student', NULL),
(797130, 'AND', 'Fuse Attendee', '11th Grade Student', NULL),
(797129, 'AND', 'Fuse Attendee', '12th Grade Student', NULL),
(797117, 'AND', 'Fuse Attendee', '12th Grade Student', NULL),
(797104, 'AND', 'Fuse Attendee', '6th Grade Student', NULL),
(797119, 'AND', 'Fuse Attendee', '6th Grade Student', NULL),
(797118, 'AND', 'Fuse Attendee', '7th Grade Student', NULL),
(797121, 'AND', 'Fuse Attendee', '7th Grade Student', NULL),
(797120, 'AND', 'Fuse Attendee', '8th Grade Student', NULL),
(797124, 'AND', 'Fuse Attendee', '8th Grade Student', NULL),
(797123, 'AND', 'Fuse Attendee', '9th Grade Student', NULL),
(1364338, 'AND', 'Fuse Attendee', '9th Grade Student', NULL),
(880596, 'AND', 'Fuse Volunteer', 'Atrium', NULL),
(1324790, 'AND', 'Fuse Volunteer', 'Campus Safety', NULL),
(1269085, 'AND', 'Fuse Volunteer', 'Care', NULL),
(1269087, 'AND', 'Fuse Volunteer', 'Check-In', NULL),
(1269083, 'AND', 'Fuse Volunteer', 'Check-In', NULL),
(880576, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(880601, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(1388929, 'AND', 'Fuse Volunteer', 'Fuse Guest', NULL),
(880581, 'AND', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(880584, 'AND', 'Fuse Volunteer', 'Game Room', NULL),
(1269091, 'AND', 'Fuse Volunteer', 'Greeter', NULL),
(1269092, 'AND', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(880586, 'AND', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1269088, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(880594, 'AND', 'Fuse Volunteer', 'Lounge', NULL),
(1355492, 'AND', 'Fuse Volunteer', 'VIP Team', NULL),
(1357085, 'AND', 'Fuse Volunteer', 'Leadership Team', NULL),
(880597, 'AND', 'Fuse Volunteer', 'Lounge', NULL),
(1269089, 'AND', 'Fuse Volunteer', 'Campus Safety', NULL),
(1239817, 'AND', 'Fuse Volunteer', 'Lounge', NULL),
(815318, 'AND', 'Fuse Volunteer', 'New Serve', NULL),
(880589, 'AND', 'Fuse Volunteer', 'Office Team', NULL),
(880590, 'AND', 'Fuse Volunteer', 'Parking', NULL),
(880595, 'AND', 'Fuse Volunteer', 'Pick-Up', NULL),
(1269090, 'AND', 'Fuse Volunteer', 'Production', NULL),
(880591, 'AND', 'Fuse Volunteer', 'Leadership Team', NULL),
(1362461, 'AND', 'Fuse Volunteer', 'Snack Bar', NULL),
(1362462, 'AND', 'Fuse Attendee', 'Special Event Attendee', NULL),
(1355490, 'AND', 'Fuse Volunteer', 'Special Event Volunteer', NULL),
(880600, 'AND', 'Fuse Volunteer', 'Sports', NULL),
(1161745, 'AND', 'Fuse Volunteer', 'Spring Zone', NULL),
(880592, 'AND', 'Fuse Volunteer', 'Student Leader', NULL),
(880593, 'AND', 'Fuse Volunteer', 'Usher', NULL),
(1269081, 'AND', 'Fuse Volunteer', 'VHQ', NULL),
(1259794, 'AND', 'Fuse Volunteer', 'Atrium', NULL),
(1200493, 'AND', 'Fuse Volunteer', 'Worship', NULL),
(1088020, 'AND', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(800226, 'AND', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(825143, 'AND', 'Guest Services Attendee', 'Awake Team', NULL),
(1054718, 'AND', 'Guest Services Volunteer', 'Campus Safety', NULL),
(913891, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(1228615, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(801019, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(800228, 'AND', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(809737, 'AND', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(800229, 'AND', 'Guest Services Volunteer', 'Finance Team', NULL),
(1274607, 'AND', 'Guest Services Volunteer', 'Finance Team', NULL),
(800230, 'AND', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(800231, 'AND', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(800286, 'AND', 'Guest Services Attendee', 'Greeting Team', NULL),
(1200494, 'AND', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1200495, 'AND', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1239835, 'AND', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(800894, 'AND', 'Guest Services Volunteer', 'New Serve Team', NULL),
(800290, 'AND', 'Guest Services Attendee', 'Office Team', NULL),
(1036958, 'AND', 'Guest Services Attendee', 'Parking Team', NULL),
(1125425, 'AND', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1335361, 'AND', 'Guest Services Volunteer', 'Sign Language Team', NULL),
(1335363, 'AND', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(800293, 'AND', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(800294, 'AND', 'Guest Services Volunteer', 'Usher Team', NULL),
(800558, 'AND', 'Guest Services Attendee', 'VHQ Team', NULL),
(870961, 'AND', 'Preschool Attendee', '24-29 mo.', 'Fire Station'),
(1059150, 'AND', 'Preschool Attendee', '30-31 mo.', 'Lil'' Spring'),
(931364, 'AND', 'Preschool Attendee', '32-33 mo.', 'Pop''s Garage'),
(800624, 'AND', 'Preschool Attendee', '36-37 mo.', 'Spring Fresh'),
(872793, 'AND', 'Preschool Attendee', '38-39 mo.', 'SpringTown Police'),
(1289910, 'AND', 'Preschool Attendee', '40-41 mo.', 'SpringTown Toys'),
(992086, 'AND', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1336792, 'AND', 'Support Volunteer', 'Advocate', NULL),
(872713, 'AND', 'Elementary Attendee', 'Base Camp', NULL),
(1413278, 'AND', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(800560, 'AND', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1212606, 'AND', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1394880, 'AND', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(800677, 'AND', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(800559, 'AND', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1404022, 'AND', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1294275, 'AND', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1080425, 'AND', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1162891, 'AND', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(800561, 'AND', 'Next Steps Volunteer', 'District Leader', NULL),
(1212607, 'AND', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1059151, 'AND', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1200645, 'AND', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1289912, 'AND', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1336793, 'AND', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1081248, 'AND', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(800563, 'AND', 'Next Steps Volunteer', 'Group Leader', NULL),
(800562, 'AND', 'Next Steps Volunteer', 'Group Training', NULL),
(802863, 'AND', 'Next Steps Volunteer', 'Groups Connector', NULL),
(842062, 'AND', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(802841, 'AND', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1259860, 'AND', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(800625, 'AND', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(872794, 'AND', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(872717, 'AND', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(800678, 'AND', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(800618, 'AND', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1212608, 'AND', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(800679, 'AND', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1408701, 'AND', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(802842, 'AND', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(800626, 'AND', 'Next Steps Volunteer', 'Events Office Team', NULL),
(872797, 'AND', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1289913, 'AND', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1336794, 'AND', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1473716, 'AND', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(872718, 'AND', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1102946, 'AND', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1304932, 'AND', 'Next Steps Volunteer', 'Prayer Team', NULL),
(800899, 'AND', 'Next Steps Volunteer', 'Resource Center', NULL),
(1212618, 'AND', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(818321, 'AND', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(818128, 'AND', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(818102, 'AND', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(817156, 'AND', 'Next Steps Volunteer', 'Writing Team', NULL),
(872837, 'AND', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1289950, 'AND', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(842086, 'AND', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1336843, 'AND', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(872757, 'AND', 'Support Volunteer', 'Check-In Volunteer', NULL),
(778083, 'AND', 'Nursery Attendee', 'Crawlers', 'Wonder Way 3'),
(1212114, 'AND', 'Nursery Attendee', 'Crawlers', 'Wonder Way 4'),
(802875, 'AND', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1456001, 'AND', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 2'),
(800687, 'AND', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(802843, 'AND', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1445840, 'AND', 'Production Volunteer', 'Elementary Production ', NULL),
(1285784, 'AND', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1390935, 'AND', 'Preschool Volunteer', 'Fire Station Volunteer', NULL),
(1285785, 'AND', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1285791, 'AND', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1285792, 'AND', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1285804, 'AND', 'Guest Services Volunteer', 'Area Leader', NULL),
(1285803, 'AND', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1244643, 'AND', 'Elementary Attendee', 'ImagiNation K', NULL),
(1214197, 'AND', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1214198, 'AND', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1285800, 'AND', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1285786, 'AND', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1285789, 'AND', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1285788, 'AND', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1214193, 'AND', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1214194, 'AND', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1233805, 'AND', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1285797, 'AND', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1285787, 'AND', 'Support Volunteer', 'New Serve Team', NULL),
(1285799, 'AND', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1285798, 'AND', 'Preschool Volunteer', 'Police Volunteer', NULL),
(1214195, 'AND', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1214196, 'AND', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1244642, 'AND', 'Production Volunteer', 'Preschool Production', NULL),
(1214191, 'AND', 'Production Volunteer', 'Preschool Production ', NULL),
(1214192, 'AND', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1238139, 'AND', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1285806, 'AND', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1285795, 'AND', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1143828, 'AND', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1143827, 'AND', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(1244640, 'AND', 'Special Needs Attendee', 'Spring Zone', NULL),
(1232876, 'AND', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1214190, 'AND', 'Special Needs Attendee', 'Spring Zone Jr.', NULL),
(1285782, 'AND', 'Special Needs Volunteer', 'Spring Zone Jr. Volunteer', NULL),
(1390937, 'AND', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1304029, 'AND', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1390938, 'AND', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1285783, 'AND', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1285802, 'AND', 'Nursery Attendee', 'Toddlers', 'Wonder Way 7'),
(1285796, 'AND', 'Nursery Attendee', 'Toddlers', 'Wonder Way 8'),
(1285793, 'AND', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1390939, 'AND', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1285794, 'AND', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1285801, 'AND', 'Nursery Attendee', 'Walkers', 'Wonder Way 5'),
(1285805, 'AND', 'Nursery Attendee', 'Walkers', 'Wonder Way 6'),
(1473764, 'AND', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1473768, 'AND', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1473775, 'AND', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(1473782, 'AND', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1473785, 'AND', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1211700, 'AND', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1211701, 'AND', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1211703, 'AND', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(1211704, 'AND', 'Nursery Volunteer', 'Wonder Way 6 Volunteer', NULL),
(1211705, 'AND', 'Guest Services Volunteer', 'Receptionist', NULL),
(1211707, 'AND', 'Nursery Volunteer', 'Wonder Way 7 Volunteer', NULL),
(1211689, 'AND', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1211690, 'AND', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1211691, 'AND', 'Nursery Volunteer', 'Wonder Way 8 Volunteer', NULL),
(1211693, 'AND', 'Nursery Volunteer', 'Wonder Way 8 Volunteer', NULL),
(1211694, 'AND', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1211696, 'BSP', 'Preschool Attendee', '30-31 mo.', 'Pop''s Garage'),
(1211698, 'BSP', 'Preschool Attendee', '36-37 mo.', 'SpringTown Toys'),
(1211699, 'BSP', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1364339, 'BSP', 'Support Volunteer', 'Advocate', NULL),
(1212047, 'BSP', 'Elementary Attendee', 'Base Camp', NULL),
(1361833, 'BSP', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1348508, 'BSP', 'Fuse Attendee', '10th Grade Student', NULL),
(1212049, 'BSP', 'Fuse Attendee', '10th Grade Student', NULL),
(1388934, 'BSP', 'Fuse Attendee', '11th Grade Student', NULL),
(1343195, 'BSP', 'Fuse Attendee', '11th Grade Student', NULL),
(1212023, 'BSP', 'Fuse Attendee', '12th Grade Student', NULL),
(1361834, 'BSP', 'Fuse Attendee', '12th Grade Student', NULL),
(1212022, 'BSP', 'Fuse Attendee', '6th Grade Student', NULL),
(1212025, 'BSP', 'Fuse Attendee', '6th Grade Student', NULL),
(1239818, 'BSP', 'Fuse Attendee', '7th Grade Student', NULL),
(1212027, 'BSP', 'Fuse Attendee', '7th Grade Student', NULL),
(1212050, 'BSP', 'Fuse Attendee', '8th Grade Student', NULL),
(1212028, 'BSP', 'Fuse Attendee', '8th Grade Student', NULL),
(1212029, 'BSP', 'Fuse Attendee', '9th Grade Student', NULL),
(1212032, 'BSP', 'Fuse Attendee', '9th Grade Student', NULL),
(1212030, 'BSP', 'Fuse Volunteer', 'Atrium', NULL),
(1212031, 'BSP', 'Fuse Volunteer', 'Campus Safety', NULL),
(1259795, 'BSP', 'Fuse Volunteer', 'Care', NULL),
(1212788, 'BSP', 'Fuse Volunteer', 'Check-In', NULL),
(1212790, 'BSP', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1212795, 'BSP', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1212794, 'BSP', 'Fuse Volunteer', 'Game Room', NULL),
(1212815, 'BSP', 'Fuse Volunteer', 'Greeter', NULL),
(1212663, 'BSP', 'Fuse Volunteer', 'Leadership Team', NULL),
(1274611, 'BSP', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(1212796, 'BSP', 'Fuse Volunteer', 'Lounge', NULL),
(1212798, 'BSP', 'Fuse Volunteer', 'New Serve', NULL),
(1212799, 'BSP', 'Fuse Volunteer', 'Pick-Up', NULL),
(1212801, 'BSP', 'Fuse Volunteer', 'Production', NULL),
(1212802, 'BSP', 'Fuse Volunteer', 'Snack Bar', NULL),
(1212804, 'BSP', 'Fuse Volunteer', 'Sports', NULL),
(1212816, 'BSP', 'Fuse Volunteer', 'VIP Team', NULL),
(1212807, 'BSP', 'Fuse Volunteer', 'Usher', NULL),
(1384264, 'BSP', 'Fuse Volunteer', 'VHQ', NULL),
(1212809, 'BSP', 'Fuse Volunteer', 'Worship', NULL),
(1212810, 'BSP', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1212812, 'BSP', 'Guest Services Volunteer', 'Campus Safety', NULL),
(800633, 'BSP', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(872798, 'BSP', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1445842, 'BSP', 'Guest Services Volunteer', 'Finance Team', NULL),
(1289915, 'BSP', 'Guest Services Volunteer', 'Finance Team', NULL),
(842063, 'BSP', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1445844, 'BSP', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1336796, 'BSP', 'Guest Services Attendee', 'Greeting Team', NULL),
(1445846, 'BSP', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1445848, 'BSP', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(872719, 'BSP', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1445850, 'BSP', 'Guest Services Volunteer', 'New Serve Team', NULL),
(930856, 'BSP', 'Guest Services Attendee', 'Office Team', NULL),
(1212609, 'BSP', 'Guest Services Attendee', 'Parking Team', NULL),
(827174, 'BSP', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(800680, 'BSP', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(827250, 'BSP', 'Guest Services Volunteer', 'Usher Team', NULL),
(800634, 'BSP', 'Guest Services Attendee', 'VHQ Team', NULL),
(872792, 'BSP', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1289914, 'BSP', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1074498, 'BSP', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1336795, 'BSP', 'Creativity & Tech Volunteer', 'Band', NULL),
(1073454, 'BSP', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(800904, 'BSP', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1212634, 'BSP', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(818317, 'BSP', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(818122, 'BSP', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(818097, 'BSP', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(817143, 'BSP', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(872818, 'BSP', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1289931, 'BSP', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1404064, 'BSP', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1336822, 'BSP', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1073455, 'BSP', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1443500, 'BSP', 'Next Steps Volunteer', 'Care Office Team', NULL),
(800903, 'BSP', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1212610, 'BSP', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(802143, 'BSP', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1456002, 'BSP', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(802185, 'BSP', 'Next Steps Volunteer', 'Events Office Team', NULL),
(802202, 'BSP', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1445841, 'BSP', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(802243, 'BSP', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(872826, 'BSP', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1445843, 'BSP', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1289940, 'BSP', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1197360, 'BSP', 'Next Steps Volunteer', 'Group Leader', NULL),
(1445845, 'BSP', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1336828, 'BSP', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1445847, 'BSP', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1290761, 'BSP', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1211162, 'BSP', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1239827, 'BSP', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1211160, 'BSP', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1258668, 'BSP', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1258669, 'BSP', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1219521, 'BSP', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1211161, 'BSP', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1242912, 'BSP', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1390942, 'BSP', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1242911, 'BSP', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1242917, 'BSP', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1242916, 'BSP', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1242932, 'BSP', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1242931, 'BSP', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1244646, 'BSP', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1230092, 'BSP', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1230093, 'BSP', 'Next Steps Volunteer', 'Resource Center', NULL),
(1242913, 'BSP', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1242919, 'BSP', 'Next Steps Volunteer', 'Writing Team', NULL),
(1390943, 'BSP', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1242918, 'BSP', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1230090, 'BSP', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1230091, 'BSP', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1233806, 'BSP', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1242923, 'BSP', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1242924, 'BSP', 'Production Volunteer', 'Elementary Production ', NULL),
(1230088, 'BSP', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1230089, 'BSP', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1244645, 'BSP', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1230083, 'BSP', 'Guest Services Volunteer', 'Area Leader', NULL),
(1230087, 'BSP', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1238140, 'BSP', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1283766, 'BSP', 'Elementary Attendee', 'ImagiNation K', NULL),
(1242920, 'BSP', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1211642, 'BSP', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1211644, 'BSP', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1244644, 'BSP', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1301525, 'BSP', 'Support Volunteer', 'KidSpring Greeter', NULL),
(920635, 'BSP', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1353571, 'BSP', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1348848, 'BSP', 'Support Volunteer', 'Load In Volunteer', NULL),
(1345803, 'BSP', 'Support Volunteer', 'Load Out Volunteer', NULL),
(1114611, 'BSP', 'Support Volunteer', 'New Serve Team', NULL),
(1388038, 'BSP', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1017062, 'BSP', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1190220, 'BSP', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1327890, 'BSP', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(935951, 'BSP', 'Production Volunteer', 'Preschool Production', NULL),
(1005523, 'BSP', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(938310, 'BSP', 'Production Volunteer', 'Production Area Leader', NULL),
(1006455, 'BSP', 'Production Volunteer', 'Production Service Leader', NULL),
(945459, 'BSP', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1211334, 'BSP', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1293789, 'BSP', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(977592, 'BSP', 'Special Needs Attendee', 'Spring Zone', NULL),
(1184051, 'BSP', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1166486, 'BSP', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(939371, 'BSP', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1127856, 'BSP', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(939370, 'BSP', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1166489, 'BSP', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(844951, 'BSP', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1166485, 'BSP', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1314404, 'BSP', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1232877, 'BSP', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1230082, 'BSP', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1242910, 'BSP', 'Fuse Volunteer', 'Student Leader', NULL),
(1390944, 'BSP', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1304030, 'CEN', 'Creativity & Tech Volunteer', 'Design Team', NULL),
(1242909, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1242930, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1242921, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1242929, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1242933, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1473795, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1473798, 'CEN', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1473792, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1473801, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(854706, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(885256, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(854707, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(885257, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(854708, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(885258, 'CEN', 'Fuse Volunteer', 'Fuse Office Team', NULL),
(854709, 'CEN', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(885259, 'CEN', 'Guest Services Volunteer', 'Events Team', NULL),
(854710, 'CEN', 'Guest Services Volunteer', 'Finance Office Team', NULL),
(885260, 'CEN', 'Guest Services Volunteer', 'GS Office Team', NULL),
(854711, 'CEN', 'Creativity & Tech Volunteer', 'Web Dev Team', NULL),
(885261, 'CEN', 'Support Volunteer', 'Office Team', NULL),
(854715, 'CEN', 'Next Steps Volunteer', 'NS Office Team', NULL),
(885262, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(854716, 'CEN', 'Guest Services Volunteer', 'Receptionist', NULL),
(885263, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(854717, 'CEN', 'Creativity & Tech Volunteer', 'Design Team', NULL),
(885264, 'CEN', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(854718, 'CEN', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(885265, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(854719, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(885266, 'CEN', 'Creativity & Tech Volunteer', 'Web Dev Team', NULL),
(854720, 'CEN', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(885267, 'CEN', 'Next Steps Volunteer', 'NS Office Team', NULL),
(854712, 'CEN', 'Next Steps Volunteer', 'Writing Team', NULL),
(885268, 'CEN', 'Support Volunteer', 'KidSpring Office Team', NULL),
(854713, 'CEN', 'Next Steps Attendee', 'Financial Peace University', NULL),
(885269, 'CEN', 'Fuse Attendee', 'Special Event Attendee', NULL),
(885280, 'CEN', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1300266, 'CEN', 'Next Steps Volunteer', 'Church Online Volunteer', NULL),
(885270, 'CEN', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(885271, 'CEN', 'Fuse Volunteer', 'Fuse Office Team', NULL),
(885282, 'CEN', 'Fuse Volunteer', 'Office Team', NULL),
(854795, 'CEN', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1388935, 'CEN', 'Guest Services Volunteer', 'HR Team', NULL),
(885272, 'CEN', 'Guest Services Volunteer', 'Network Fuse Team', NULL),
(1023094, 'CEN', 'Guest Services Volunteer', 'Network Office Team', NULL),
(885273, 'CEN', 'Guest Services Volunteer', 'Network Sunday Team', NULL),
(1361839, 'CEN', 'Creativity & Tech Volunteer', 'NewSpring Store Team', NULL),
(1395638, 'CEN', 'Creativity & Tech Volunteer', 'Video Production Team', NULL),
(885274, 'CEN', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1239819, 'CHS', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1361840, 'CHS', 'Preschool Attendee', '30-31 mo.', 'Lil'' Spring'),
(885275, 'CHS', 'Preschool Attendee', '42-43 mo.', 'SpringTown Toys'),
(885276, 'CHS', 'Preschool Attendee', '54-55 mo.', 'Treehouse'),
(885283, 'CHS', 'Support Volunteer', 'Advocate', NULL),
(885277, 'CHS', 'Elementary Attendee', 'Base Camp', NULL),
(1361841, 'CHS', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1300265, 'CHS', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(885279, 'CHS', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(854714, 'CHS', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(854721, 'CHS', 'Fuse Attendee', '10th Grade Student', NULL),
(885278, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1023095, 'CHS', 'Fuse Attendee', '10th Grade Student', NULL),
(1259796, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1300378, 'CHS', 'Fuse Attendee', '11th Grade Student', NULL),
(823056, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802112, 'CHS', 'Fuse Attendee', '11th Grade Student', NULL),
(809740, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1088021, 'CHS', 'Fuse Attendee', '12th Grade Student', NULL),
(1040968, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1274615, 'CHS', 'Fuse Attendee', '12th Grade Student', NULL),
(951929, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802104, 'CHS', 'Fuse Attendee', '6th Grade Student', NULL),
(802105, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802113, 'CHS', 'Fuse Attendee', '6th Grade Student', NULL),
(802114, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1239836, 'CHS', 'Fuse Attendee', '7th Grade Student', NULL),
(802116, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802108, 'CHS', 'Fuse Attendee', '7th Grade Student', NULL),
(1384265, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1036957, 'CHS', 'Fuse Attendee', '8th Grade Student', NULL),
(1342573, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1342572, 'CHS', 'Fuse Attendee', '8th Grade Student', NULL),
(802107, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802109, 'CHS', 'Fuse Attendee', '9th Grade Student', NULL),
(1445849, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(872750, 'CHS', 'Fuse Attendee', '9th Grade Student', NULL),
(1445851, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802755, 'CHS', 'Fuse Volunteer', 'Campus Safety', NULL),
(1212619, 'CHS', 'Fuse Volunteer', 'Care', NULL),
(1212625, 'CHS', 'Fuse Volunteer', 'Check-In', NULL),
(818322, 'CHS', 'Fuse Volunteer', 'Sports', NULL),
(818129, 'CHS', 'Fuse Volunteer', 'Fuse Guest', NULL),
(818103, 'CHS', 'Fuse Volunteer', 'Office Team', NULL),
(817157, 'CHS', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(872839, 'CHS', 'Fuse Volunteer', 'Game Room', NULL),
(1289951, 'CHS', 'Fuse Volunteer', 'Greeter', NULL),
(1289952, 'CHS', 'Fuse Volunteer', 'Sports', NULL),
(842087, 'CHS', 'Fuse Volunteer', 'Leadership Team', NULL),
(1336844, 'CHS', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(1336845, 'CHS', 'Fuse Volunteer', 'Lounge', NULL),
(1190763, 'CHS', 'Fuse Volunteer', 'New Serve', NULL),
(872764, 'CHS', 'Fuse Volunteer', 'Next Steps', NULL),
(776138, 'CHS', 'Fuse Volunteer', 'Parking', NULL),
(776140, 'CHS', 'Fuse Volunteer', 'Pick-Up', NULL),
(1212602, 'CHS', 'Fuse Volunteer', 'Production', NULL),
(870959, 'CHS', 'Fuse Volunteer', 'Snack Bar', NULL),
(800676, 'CHS', 'Fuse Volunteer', 'Sports', NULL),
(802839, 'CHS', 'Fuse Volunteer', 'Student Leader', NULL),
(800622, 'CHS', 'Fuse Volunteer', 'VIP Team', NULL),
(800623, 'CHS', 'Fuse Volunteer', 'VIP Team', NULL),
(872878, 'CHS', 'Fuse Volunteer', 'VIP Team', NULL),
(1289906, 'CHS', 'Fuse Volunteer', 'Usher', NULL),
(1159891, 'CHS', 'Fuse Volunteer', 'VHQ', NULL),
(1336790, 'CHS', 'Fuse Volunteer', 'Worship', NULL),
(872784, 'CHS', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(776129, 'CHS', 'Guest Services Volunteer', 'Campus Safety', NULL),
(776139, 'CHS', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1212601, 'CHS', 'Guest Services Volunteer', 'Finance Team', NULL),
(802861, 'CHS', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(800675, 'CHS', 'Guest Services Volunteer', 'Finance Team', NULL),
(802838, 'CHS', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(800620, 'CHS', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(800621, 'CHS', 'Guest Services Attendee', 'Greeting Team', NULL),
(872875, 'CHS', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1289905, 'CHS', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(842059, 'CHS', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1336789, 'CHS', 'Guest Services Volunteer', 'New Serve Team', NULL),
(872782, 'CHS', 'Guest Services Attendee', 'Office Team', NULL),
(1382568, 'CHS', 'Guest Services Attendee', 'Parking Team', NULL),
(1212614, 'CHS', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1093018, 'CHS', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1382563, 'CHS', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(821418, 'CHS', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(821424, 'CHS', 'Guest Services Volunteer', 'Usher Team', NULL),
(1473804, 'CHS', 'Guest Services Attendee', 'VHQ Team', NULL),
(1339310, 'CHS', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1339312, 'CHS', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1339305, 'CHS', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1340566, 'CHS', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1339311, 'CHS', 'Creativity & Tech Volunteer', 'Band', NULL),
(1339313, 'CHS', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(872828, 'CHS', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1340553, 'CHS', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(939376, 'CHS', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(803636, 'CHS', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1473807, 'CHS', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1257874, 'CHS', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1104832, 'CHS', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1257875, 'CHS', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1257876, 'CHS', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1239828, 'CHS', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1068567, 'CHS', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1219522, 'CHS', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1340611, 'CHS', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1068566, 'CHS', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1276073, 'CHS', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1390959, 'CHS', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1276074, 'CHS', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1276078, 'CHS', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1276079, 'CHS', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1276064, 'CHS', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1244651, 'CHS', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1213989, 'CHS', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1213990, 'CHS', 'Next Steps Volunteer', 'Group Leader', NULL),
(1276075, 'CHS', 'Next Steps Volunteer', 'Group Training', NULL),
(1276081, 'CHS', 'Next Steps Volunteer', 'Groups Connector', NULL),
(1276080, 'CHS', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1213987, 'CHS', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1213988, 'CHS', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1233807, 'CHS', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1276065, 'CHS', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1276076, 'CHS', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1276067, 'CHS', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1276066, 'CHS', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1213985, 'CHS', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1213986, 'CHS', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1244649, 'CHS', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1213983, 'CHS', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1213984, 'CHS', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1238141, 'CHS', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1283767, 'CHS', 'Production Volunteer', 'Elementary Production', NULL),
(1276069, 'CHS', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1143836, 'CHS', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1213981, 'CHS', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1244650, 'CHS', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1301527, 'CHS', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1327325, 'CHS', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(797369, 'CHS', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(885176, 'CHS', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(797368, 'CHS', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(885177, 'CHS', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(797370, 'CHS', 'Next Steps Volunteer', 'Prayer Team', NULL),
(885191, 'CHS', 'Next Steps Volunteer', 'Resource Center', NULL),
(797371, 'CHS', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(885192, 'CHS', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(797372, 'CHS', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(885193, 'CHS', 'Next Steps Volunteer', 'Writing Team', NULL),
(797373, 'CHS', 'Guest Services Volunteer', 'Area Leader', NULL),
(885194, 'CHS', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(797374, 'CHS', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(885195, 'CHS', 'Elementary Attendee', 'ImagiNation K', NULL),
(885196, 'CHS', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(797375, 'CHS', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(797381, 'CHS', 'Fuse Volunteer', 'Atrium', NULL),
(885197, 'CHS', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(797380, 'CHS', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(885198, 'CHS', 'Support Volunteer', 'KidSpring Greeter', NULL),
(797378, 'CHS', 'Support Volunteer', 'KidSpring Office Team', NULL),
(885199, 'CHS', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(797379, 'CHS', 'Guest Services Attendee', 'Awake Team', NULL),
(885200, 'CHS', 'Support Volunteer', 'Load In Volunteer', NULL),
(797376, 'CHS', 'Support Volunteer', 'Load In Volunteer', NULL),
(885201, 'CHS', 'Support Volunteer', 'New Serve Team', NULL),
(797377, 'CHS', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(885202, 'CHS', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1154931, 'CHS', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1202544, 'CHS', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(885204, 'CHS', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(885174, 'CHS', 'Elementary Attendee', 'Shockwave 4th', NULL),
(885178, 'CHS', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1361884, 'CHS', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(885210, 'CHS', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(815440, 'CHS', 'Special Needs Attendee', 'Spring Zone', NULL),
(885180, 'CHS', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(885181, 'CHS', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(885182, 'CHS', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(885190, 'CHS', 'Nursery Attendee', 'Toddlers', 'Wonder Way 4'),
(1361879, 'CHS', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(885183, 'CHS', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(885184, 'CHS', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(885205, 'CHS', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1239820, 'CHS', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(885185, 'CHS', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(885186, 'CHS', 'Next Steps Attendee', 'Financial Peace University', NULL),
(885208, 'CHS', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(1349719, 'CHS', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(885187, 'CHS', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1361882, 'CHS', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(885209, 'CHS', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(885188, 'CHS', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(885189, 'CHS', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1259797, 'CHS', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1300379, 'CLE', 'Elementary Attendee', 'Base Camp', NULL),
(1232752, 'CLE', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(823058, 'CLE', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1040958, 'CLE', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(882614, 'COL', 'Preschool Attendee', '24-29 mo.', 'Lil'' Spring'),
(809741, 'COL', 'Preschool Attendee', '30-31 mo.', 'Pop''s Garage'),
(1040967, 'COL', 'Preschool Attendee', '36-37 mo.', 'Spring Fresh'),
(1274618, 'COL', 'Preschool Attendee', '46-47 mo.', 'SpringTown Toys'),
(1088078, 'COL', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(931945, 'COL', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(802169, 'COL', 'Support Volunteer', 'Advocate', NULL),
(802170, 'COL', 'Elementary Attendee', 'Base Camp', NULL),
(931944, 'COL', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1239837, 'COL', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(802177, 'COL', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(802176, 'COL', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1384266, 'COL', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1036956, 'COL', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1249740, 'COL', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1342575, 'COL', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1342574, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(802171, 'COL', 'Fuse Attendee', '10th Grade Student', NULL),
(802172, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1289946, 'COL', 'Fuse Attendee', '10th Grade Student', NULL),
(1421596, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1336830, 'COL', 'Fuse Attendee', '11th Grade Student', NULL),
(1190757, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1313359, 'COL', 'Fuse Attendee', '11th Grade Student', NULL),
(1212612, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802145, 'COL', 'Fuse Attendee', '12th Grade Student', NULL),
(802186, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802203, 'COL', 'Fuse Attendee', '12th Grade Student', NULL),
(802244, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(872829, 'COL', 'Fuse Attendee', '6th Grade Student', NULL),
(1289943, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1405683, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1336834, 'COL', 'Fuse Attendee', '6th Grade Student', NULL),
(872752, 'COL', 'Fuse Attendee', '7th Grade Student', NULL),
(818324, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1382565, 'COL', 'Fuse Attendee', '7th Grade Student', NULL),
(818131, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(818105, 'COL', 'Fuse Attendee', '8th Grade Student', NULL),
(1443716, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(842089, 'COL', 'Fuse Attendee', '8th Grade Student', NULL),
(800898, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1212643, 'COL', 'Fuse Attendee', '9th Grade Student', NULL),
(856972, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(872845, 'COL', 'Fuse Attendee', '9th Grade Student', NULL),
(1289957, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1336850, 'COL', 'Fuse Volunteer', 'Atrium', NULL),
(872767, 'COL', 'Fuse Volunteer', 'Atrium', NULL),
(1443718, 'COL', 'Fuse Volunteer', 'Campus Safety', NULL),
(802734, 'COL', 'Fuse Volunteer', 'Check-In', NULL),
(1212613, 'COL', 'Fuse Volunteer', 'Sports', NULL),
(802141, 'COL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802180, 'COL', 'Fuse Volunteer', 'Fuse Guest', NULL),
(802197, 'COL', 'Fuse Volunteer', 'Office Team', NULL),
(802238, 'COL', 'Fuse Volunteer', 'Game Room', NULL),
(872830, 'COL', 'Fuse Volunteer', 'Greeter', NULL),
(1289944, 'COL', 'Fuse Volunteer', 'Sports', NULL),
(1405684, 'COL', 'Fuse Volunteer', 'VIP Team', NULL),
(1336829, 'COL', 'Fuse Volunteer', 'Leadership Team', NULL),
(872753, 'COL', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(800896, 'COL', 'Fuse Volunteer', 'Lounge', NULL),
(802757, 'COL', 'Fuse Volunteer', 'Campus Safety', NULL),
(1212620, 'COL', 'Fuse Volunteer', 'New Serve', NULL),
(1212621, 'COL', 'Fuse Volunteer', 'Parking', NULL),
(818323, 'COL', 'Fuse Volunteer', 'Pick-Up', NULL),
(818130, 'COL', 'Fuse Volunteer', 'Production', NULL),
(818104, 'COL', 'Fuse Volunteer', 'Leadership Team', NULL),
(817159, 'COL', 'Fuse Volunteer', 'Snack Bar', NULL),
(872841, 'COL', 'Fuse Volunteer', 'Sports', NULL),
(1289953, 'COL', 'Fuse Volunteer', 'Spring Zone', NULL),
(1289954, 'COL', 'Fuse Volunteer', 'Usher', NULL),
(1443773, 'COL', 'Fuse Volunteer', 'VHQ', NULL),
(1336846, 'COL', 'Fuse Volunteer', 'Worship', NULL),
(1336847, 'COL', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1190762, 'COL', 'Guest Services Attendee', 'Awake Team', NULL),
(872765, 'COL', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1291447, 'COL', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(1093013, 'COL', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1382566, 'COL', 'Guest Services Volunteer', 'Finance Team', NULL),
(821420, 'COL', 'Guest Services Volunteer', 'Finance Team', NULL),
(923364, 'COL', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1443711, 'COL', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1289955, 'COL', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1420946, 'COL', 'Guest Services Attendee', 'Greeting Team', NULL),
(1405827, 'COL', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1190765, 'COL', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(905603, 'COL', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1232879, 'COL', 'Guest Services Attendee', 'Office Team', NULL),
(1213982, 'COL', 'Guest Services Attendee', 'Parking Team', NULL),
(1276071, 'COL', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1390961, 'COL', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1304031, 'COL', 'Guest Services Volunteer', 'Sign Language Team', NULL),
(1390962, 'COL', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1276072, 'COL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1276042, 'COL', 'Guest Services Volunteer', 'Usher Team', NULL),
(1276070, 'COL', 'Guest Services Attendee', 'VHQ Team', NULL),
(1285766, 'COL', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1285767, 'COL', 'Production Volunteer', 'Elementary Production', NULL),
(1276041, 'COL', 'Production Volunteer', 'Elementary Production', NULL),
(1276040, 'COL', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1473810, 'COL', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1473817, 'COL', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1473820, 'COL', 'Guest Services Volunteer', 'Area Leader', NULL),
(1473823, 'COL', 'Creativity & Tech Volunteer', 'Band', NULL),
(1474621, 'COL', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1474626, 'COL', 'Creativity & Tech Volunteer', 'IT Team', NULL),
(1474627, 'COL', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1474630, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1474633, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1290762, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1104833, 'COL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1258667, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1239829, 'COL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1068569, 'COL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1219523, 'COL', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1300462, 'COL', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1286526, 'COL', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1068568, 'COL', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1285726, 'COL', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1390907, 'COL', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1285727, 'COL', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1285733, 'COL', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1285734, 'COL', 'Next Steps Volunteer', 'District Leader', NULL),
(1285747, 'COL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1285746, 'COL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1244654, 'COL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1214058, 'COL', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1214061, 'COL', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(1285742, 'COL', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1285728, 'COL', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1285731, 'COL', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1390908, 'COL', 'Next Steps Volunteer', 'Group Leader', NULL),
(1285730, 'COL', 'Next Steps Volunteer', 'Group Training', NULL),
(1285732, 'COL', 'Next Steps Volunteer', 'Groups Connector', NULL),
(1214069, 'COL', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1214070, 'COL', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1233808, 'COL', 'Elementary Attendee', 'ImagiNation K', NULL),
(1285738, 'COL', 'Elementary Attendee', 'ImagiNation K', NULL),
(1285729, 'COL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1285741, 'COL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(797411, 'COL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(885212, 'COL', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(797412, 'COL', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(885213, 'COL', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(797413, 'COL', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(885215, 'COL', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(797415, 'COL', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(885216, 'COL', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(797416, 'COL', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(885217, 'COL', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(797417, 'COL', 'Next Steps Volunteer', 'New Serve Team', NULL),
(885218, 'COL', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(797399, 'COL', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(885219, 'COL', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(797401, 'COL', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(885220, 'COL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(797403, 'COL', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(885221, 'COL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(797406, 'COL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(885222, 'COL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(797407, 'COL', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(885223, 'COL', 'Next Steps Volunteer', 'Prayer Team', NULL),
(797409, 'COL', 'Next Steps Volunteer', 'Resource Center', NULL),
(885225, 'COL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(797418, 'COL', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(885226, 'COL', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(797419, 'COL', 'Next Steps Volunteer', 'Writing Team', NULL),
(885227, 'COL', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(885240, 'COL', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(885228, 'COL', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(885229, 'COL', 'Support Volunteer', 'KidSpring Greeter', NULL),
(885239, 'COL', 'Support Volunteer', 'KidSpring Greeter', NULL),
(815451, 'COL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1388940, 'COL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(885230, 'COL', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1023096, 'COL', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(885231, 'COL', 'Support Volunteer', 'New Serve Team', NULL),
(1361892, 'COL', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(885232, 'COL', 'Nursery Attendee', 'Older Toddlers', 'Wonder Way 5'),
(885241, 'COL', 'Nursery Attendee', 'Older Walkers', 'Wonder Way 6'),
(1239821, 'COL', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1361893, 'COL', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(885233, 'COL', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(885234, 'COL', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(885238, 'COL', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(885235, 'COL', 'Production Volunteer', 'Production Area Leader', NULL),
(1361894, 'COL', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1344496, 'COL', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1361896, 'COL', 'Elementary Attendee', 'Shockwave 5th', NULL),
(819675, 'COL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(819674, 'COL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(885237, 'COL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(885236, 'COL', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1023097, 'COL', 'Fuse Volunteer', 'Care', NULL),
(1259798, 'COL', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(1300383, 'COL', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(823061, 'COL', 'Special Needs Attendee', 'Spring Zone', NULL),
(802216, 'COL', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(854801, 'COL', 'Special Needs Attendee', 'Spring Zone Jr.', NULL),
(1040966, 'COL', 'Special Needs Volunteer', 'Spring Zone Jr. Volunteer', NULL),
(1274622, 'COL', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(825706, 'COL', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(802208, 'COL', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1088079, 'COL', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(802210, 'COL', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(802218, 'COL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(802219, 'COL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1239838, 'COL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(802217, 'COL', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(802212, 'COL', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(1036955, 'COL', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1342578, 'COL', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(802211, 'COL', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(802213, 'COL', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1212623, 'COL', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1090403, 'COL', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1396766, 'COL', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(854474, 'COL', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(872842, 'COL', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1420954, 'COL', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1336848, 'COL', 'Next Steps Volunteer', 'NS Office Team', NULL),
(872766, 'COL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(776134, 'COL', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(1212116, 'COL', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(802866, 'COL', 'Nursery Volunteer', 'Wonder Way 6 Volunteer', NULL),
(800682, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(802844, 'COL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(800627, 'COL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(872799, 'COL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1289916, 'COL', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(842064, 'COL', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1336797, 'COL', 'Nursery Attendee', 'Young Toddlers', 'Wonder Way 4'),
(872720, 'FLO', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(776132, 'FLO', 'Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
(1212115, 'FLO', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(802865, 'FLO', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(800681, 'FLO', 'Support Volunteer', 'Advocate', NULL),
(802845, 'FLO', 'Elementary Attendee', 'Base Camp', NULL),
(800628, 'FLO', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(872800, 'FLO', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1289917, 'FLO', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(842069, 'FLO', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1336799, 'FLO', 'Support Volunteer', 'Check-In Volunteer', NULL),
(872721, 'FLO', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1408702, 'FLO', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(800897, 'FLO', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1212615, 'FLO', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(802142, 'FLO', 'Production Volunteer', 'Elementary Production', NULL),
(1413377, 'FLO', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(935220, 'FLO', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(802182, 'FLO', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(802199, 'FLO', 'Guest Services Volunteer', 'Area Leader', NULL),
(802240, 'FLO', 'Fuse Attendee', '10th Grade Student', NULL),
(872832, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1289947, 'FLO', 'Fuse Attendee', '10th Grade Student', NULL),
(842083, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1336831, 'FLO', 'Fuse Attendee', '11th Grade Student', NULL),
(872754, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(776131, 'FLO', 'Fuse Attendee', '11th Grade Student', NULL),
(1212117, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802867, 'FLO', 'Fuse Attendee', '12th Grade Student', NULL),
(800683, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1408709, 'FLO', 'Fuse Attendee', '12th Grade Student', NULL),
(802846, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(800629, 'FLO', 'Fuse Attendee', '6th Grade Student', NULL),
(872802, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1289918, 'FLO', 'Fuse Attendee', '6th Grade Student', NULL),
(842065, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1285740, 'FLO', 'Fuse Attendee', '7th Grade Student', NULL),
(1214054, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1214055, 'FLO', 'Fuse Attendee', '7th Grade Student', NULL),
(1244653, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1214056, 'FLO', 'Fuse Attendee', '8th Grade Student', NULL),
(1214057, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1238142, 'FLO', 'Fuse Attendee', '8th Grade Student', NULL),
(1285750, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1285736, 'FLO', 'Fuse Attendee', '9th Grade Student', NULL),
(1143845, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1143844, 'FLO', 'Fuse Attendee', '9th Grade Student', NULL),
(1244652, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1232880, 'FLO', 'Fuse Volunteer', 'Campus Safety', NULL),
(1214053, 'FLO', 'Fuse Volunteer', 'Check-In', NULL),
(1285724, 'FLO', 'Fuse Volunteer', 'Sports', NULL),
(1390906, 'FLO', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1304032, 'FLO', 'Fuse Volunteer', 'Office Team', NULL),
(1285725, 'FLO', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1285745, 'FLO', 'Fuse Volunteer', 'Game Room', NULL),
(1285737, 'FLO', 'Fuse Volunteer', 'Greeter', NULL),
(1285768, 'FLO', 'Fuse Volunteer', 'Sports', NULL),
(1285769, 'FLO', 'Fuse Volunteer', 'Leadership Team', NULL),
(1285743, 'FLO', 'Fuse Volunteer', 'Lounge', NULL),
(1285748, 'FLO', 'Fuse Volunteer', 'Campus Safety', NULL),
(1474636, 'FLO', 'Fuse Volunteer', 'New Serve', NULL),
(1474637, 'FLO', 'Fuse Volunteer', 'Next Steps', NULL),
(1474638, 'FLO', 'Fuse Volunteer', 'Parking', NULL),
(1474639, 'FLO', 'Fuse Volunteer', 'Pick-Up', NULL),
(1474651, 'FLO', 'Fuse Volunteer', 'Production', NULL),
(1474653, 'FLO', 'Fuse Volunteer', 'Snack Bar', NULL),
(1364341, 'FLO', 'Fuse Volunteer', 'Sports', NULL),
(1364344, 'FLO', 'Fuse Volunteer', 'Spring Zone', NULL),
(1364345, 'FLO', 'Fuse Volunteer', 'Student Leader', NULL),
(1290763, 'FLO', 'Fuse Volunteer', 'VIP Team', NULL),
(1068572, 'FLO', 'Fuse Volunteer', 'VIP Team', NULL),
(1239830, 'FLO', 'Fuse Volunteer', 'VIP Team', NULL),
(1071052, 'FLO', 'Fuse Volunteer', 'Usher', NULL),
(1071053, 'FLO', 'Fuse Volunteer', 'VHQ', NULL),
(1068571, 'FLO', 'Fuse Volunteer', 'Worship', NULL),
(1071051, 'FLO', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1283450, 'FLO', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1283451, 'FLO', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1283455, 'FLO', 'Guest Services Volunteer', 'Finance Team', NULL),
(1283456, 'FLO', 'Guest Services Volunteer', 'Finance Team', NULL),
(1283468, 'FLO', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1283467, 'FLO', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1244658, 'FLO', 'Guest Services Attendee', 'Greeting Team', NULL),
(1214031, 'FLO', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1214032, 'FLO', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1283464, 'FLO', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1283452, 'FLO', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1283458, 'FLO', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1283457, 'FLO', 'Guest Services Attendee', 'Office Team', NULL),
(1283454, 'FLO', 'Guest Services Attendee', 'Parking Team', NULL),
(1214033, 'FLO', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(797443, 'FLO', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(884609, 'FLO', 'Guest Services Volunteer', 'Usher Team', NULL),
(797445, 'FLO', 'Guest Services Attendee', 'VHQ Team', NULL),
(884610, 'FLO', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(797446, 'FLO', 'Elementary Attendee', 'ImagiNation K', NULL),
(884611, 'FLO', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(797447, 'FLO', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(884612, 'FLO', 'Fuse Volunteer', 'Atrium', NULL),
(797449, 'FLO', 'Creativity & Tech Volunteer', 'Band', NULL),
(884613, 'FLO', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(797451, 'FLO', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(884614, 'FLO', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(797436, 'FLO', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(884601, 'FLO', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(797437, 'FLO', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(884602, 'FLO', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(797438, 'FLO', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(884603, 'FLO', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(797439, 'FLO', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(884604, 'FLO', 'Next Steps Volunteer', 'Care Office Team', NULL),
(797440, 'FLO', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(884605, 'FLO', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(797442, 'FLO', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(884606, 'FLO', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(797452, 'FLO', 'Next Steps Volunteer', 'District Leader', NULL),
(884607, 'FLO', 'Next Steps Volunteer', 'Events Office Team', NULL),
(797454, 'FLO', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(884608, 'FLO', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(884616, 'FLO', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(1319351, 'FLO', 'Next Steps Attendee', 'Fuse Basics', NULL),
(884587, 'FLO', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(884588, 'FLO', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(884622, 'FLO', 'Support Volunteer', 'KidSpring Greeter', NULL),
(815456, 'FLO', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1388941, 'FLO', 'Support Volunteer', 'KidSpring Office Team', NULL),
(884590, 'FLO', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(884591, 'FLO', 'Next Steps Attendee', 'Fuse First Look', NULL),
(884592, 'FLO', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(884600, 'FLO', 'Next Steps Volunteer', 'Group Leader', NULL),
(1361991, 'FLO', 'Next Steps Volunteer', 'Group Training', NULL),
(884593, 'FLO', 'Next Steps Volunteer', 'Groups Connector', NULL),
(884594, 'FLO', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(884618, 'FLO', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1239822, 'FLO', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1361992, 'FLO', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(884595, 'FLO', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(884596, 'FLO', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(884620, 'FLO', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(884597, 'FLO', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1361993, 'FLO', 'Next Steps Volunteer', 'Next Steps Area', NULL)


insert #rlcMap
values
(886238, 'FLO', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(819686, 'FLO', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(819687, 'FLO', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(884598, 'FLO', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(884599, 'FLO', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1259799, 'FLO', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1300384, 'FLO', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(823063, 'FLO', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(843865, 'FLO', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(809754, 'FLO', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1040965, 'FLO', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1274623, 'FLO', 'Next Steps Volunteer', 'Prayer Team', NULL),
(825763, 'FLO', 'Next Steps Volunteer', 'Resource Center', NULL),
(802245, 'FLO', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(802248, 'FLO', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1088080, 'FLO', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(802255, 'FLO', 'Next Steps Volunteer', 'Writing Team', NULL),
(802256, 'FLO', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1239839, 'FLO', 'Guest Services Attendee', 'Awake Team', NULL),
(802254, 'FLO', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(802250, 'FLO', 'Support Volunteer', 'Load In Volunteer', NULL),
(1036954, 'FLO', 'Support Volunteer', 'Load Out Volunteer', NULL),
(1342580, 'FLO', 'Support Volunteer', 'New Serve Team', NULL),
(1342579, 'FLO', 'Support Volunteer', 'New Serve Team', NULL),
(802249, 'FLO', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(802251, 'FLO', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1336800, 'FLO', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(872722, 'FLO', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(776130, 'FLO', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1212118, 'FLO', 'Production Volunteer', 'Production Area Leader', NULL),
(802869, 'FLO', 'Elementary Attendee', 'Shockwave 4th', NULL),
(800684, 'FLO', 'Elementary Attendee', 'Shockwave 5th', NULL),
(802847, 'FLO', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(800630, 'FLO', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(872803, 'FLO', 'Special Needs Attendee', 'Spring Zone', NULL),
(1289919, 'FLO', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(842066, 'FLO', 'Fuse Volunteer', 'Care', NULL),
(1336804, 'FLO', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(872723, 'FLO', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(802731, 'FLO', 'Nursery Attendee', 'Toddlers', 'Wonder Way 4'),
(1212616, 'FLO', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(802144, 'FLO', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(935221, 'FLO', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(802178, 'FLO', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(802195, 'FLO', 'Next Steps Attendee', 'Financial Peace University', NULL),
(802236, 'FLO', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(872834, 'FLO', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1289948, 'FLO', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1200984, 'FLO', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1336832, 'FLO', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(872755, 'FLO', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1085365, 'FLO', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(867637, 'FLO', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(935226, 'FLO', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(874836, 'FLO', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1212624, 'GRR', 'Elementary Attendee', 'Base Camp', NULL),
(919056, 'GRR', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1402257, 'GRR', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1402255, 'GRR', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(802209, 'GVL', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1040041, 'GVL', 'Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
(1289956, 'GVL', 'Preschool Attendee', '36-37 mo.', 'Spring Fresh'),
(1040042, 'GVL', 'Preschool Attendee', '44-45 mo.', 'SpringTown Police'),
(1336849, 'GVL', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(928181, 'GVL', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(800902, 'GVL', 'Support Volunteer', 'Advocate', NULL),
(802764, 'GVL', 'Elementary Attendee', 'Base Camp', NULL),
(1212655, 'GVL', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1339332, 'GVL', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(802139, 'GVL', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(802187, 'GVL', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(818133, 'GVL', 'Support Volunteer', 'Check-In Volunteer', NULL),
(802201, 'GVL', 'Nursery Attendee', 'Crawlers', 'Wonder Way 3'),
(818107, 'GVL', 'Nursery Attendee', 'Crawlers', 'Wonder Way 4'),
(802242, 'GVL', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(817170, 'GVL', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 2'),
(1106575, 'GVL', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1289968, 'GVL', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1289965, 'GVL', 'Production Volunteer', 'Elementary Production ', NULL),
(842098, 'GVL', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1384263, 'GVL', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(872292, 'GVL', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(802737, 'GVL', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1212635, 'GVL', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1396339, 'GVL', 'Elementary Attendee', 'ImagiNation K', NULL),
(1060521, 'GVL', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1060524, 'GVL', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1294276, 'GVL', 'Guest Services Volunteer', 'Area Leader', NULL),
(1080430, 'GVL', 'Fuse Volunteer', 'Atrium', NULL),
(1162892, 'GVL', 'Fuse Attendee', '10th Grade Student', NULL),
(1289932, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1404063, 'GVL', 'Fuse Attendee', '10th Grade Student', NULL),
(1081250, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1214034, 'GVL', 'Fuse Attendee', '11th Grade Student', NULL),
(1233809, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1283461, 'GVL', 'Fuse Attendee', '11th Grade Student', NULL),
(1283453, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1283463, 'GVL', 'Fuse Attendee', '12th Grade Student', NULL),
(1283462, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1214027, 'GVL', 'Fuse Attendee', '12th Grade Student', NULL),
(1214028, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1244656, 'GVL', 'Fuse Attendee', '6th Grade Student', NULL),
(1214029, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1214030, 'GVL', 'Fuse Attendee', '6th Grade Student', NULL),
(1238143, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1283769, 'GVL', 'Fuse Attendee', '7th Grade Student', NULL),
(1283459, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1143866, 'GVL', 'Fuse Attendee', '7th Grade Student', NULL),
(1143865, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1244655, 'GVL', 'Fuse Attendee', '8th Grade Student', NULL),
(1301529, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1301530, 'GVL', 'Fuse Attendee', '8th Grade Student', NULL),
(1214026, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1283448, 'GVL', 'Fuse Attendee', '9th Grade Student', NULL),
(1390967, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1304033, 'GVL', 'Fuse Attendee', '9th Grade Student', NULL),
(1390968, 'GVL', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1283449, 'GVL', 'Fuse Volunteer', 'Campus Safety', NULL),
(1283466, 'GVL', 'Fuse Volunteer', 'Care', NULL),
(1283460, 'GVL', 'Fuse Volunteer', 'Check-In', NULL),
(1285770, 'GVL', 'Fuse Volunteer', 'Sports', NULL),
(1285771, 'GVL', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1283465, 'GVL', 'Fuse Volunteer', 'Office Team', NULL),
(1283469, 'GVL', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1232881, 'GVL', 'Fuse Volunteer', 'Game Room', NULL),
(1444489, 'GVL', 'Fuse Volunteer', 'Greeter', NULL),
(1293788, 'GVL', 'Fuse Volunteer', 'Sports', NULL),
(1419279, 'GVL', 'Fuse Volunteer', 'VIP Team', NULL),
(1446307, 'GVL', 'Fuse Volunteer', 'Leadership Team', NULL),
(1462144, 'GVL', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(1417899, 'GVL', 'Fuse Volunteer', 'Lounge', NULL),
(1246845, 'GVL', 'Fuse Volunteer', 'Campus Safety', NULL),
(1390965, 'GVL', 'Fuse Volunteer', 'New Serve', NULL),
(1362086, 'GVL', 'Fuse Volunteer', 'Next Steps', NULL),
(1290764, 'GVL', 'Fuse Volunteer', 'Parking', NULL),
(1104835, 'GVL', 'Fuse Volunteer', 'Pick-Up', NULL),
(1239831, 'GVL', 'Fuse Volunteer', 'Production', NULL),
(1068581, 'GVL', 'Fuse Volunteer', 'Snack Bar', NULL),
(1258665, 'GVL', 'Fuse Volunteer', 'Sports', NULL),
(1258666, 'GVL', 'Fuse Volunteer', 'Leadership Team', NULL),
(1219524, 'GVL', 'Fuse Volunteer', 'VIP Team', NULL),
(1068580, 'GVL', 'Fuse Volunteer', 'VIP Team', NULL),
(1154644, 'GVL', 'Fuse Volunteer', 'Usher', NULL),
(1154439, 'GVL', 'Fuse Volunteer', 'VHQ', NULL),
(1154645, 'GVL', 'Fuse Volunteer', 'Worship', NULL),
(1154646, 'GVL', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1154648, 'GVL', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1154649, 'GVL', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1154650, 'GVL', 'Guest Services Volunteer', 'Finance Team', NULL),
(1154636, 'GVL', 'Guest Services Volunteer', 'Finance Team', NULL),
(1154635, 'GVL', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1154637, 'GVL', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1154638, 'GVL', 'Guest Services Attendee', 'Greeting Team', NULL),
(1154639, 'GVL', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1154640, 'GVL', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1154641, 'GVL', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1154436, 'GVL', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1154642, 'GVL', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1154435, 'GVL', 'Guest Services Attendee', 'Office Team', NULL),
(1364346, 'GVL', 'Guest Services Attendee', 'Parking Team', NULL),
(1154420, 'GVL', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1327880, 'GVL', 'Guest Services Attendee', 'Special Event Attendee', NULL),
(1154401, 'GVL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1154402, 'GVL', 'Guest Services Volunteer', 'Usher Team', NULL),
(1154426, 'GVL', 'Guest Services Attendee', 'VHQ Team', NULL),
(1154398, 'GVL', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1388943, 'GVL', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1154404, 'GVL', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1154405, 'GVL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1154407, 'GVL', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1154416, 'GVL', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1361996, 'GVL', 'Creativity & Tech Attendee', 'Choir', NULL),
(1154408, 'GVL', 'Creativity & Tech Volunteer', 'Band', NULL),
(1239823, 'GVL', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1154409, 'GVL', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1154424, 'GVL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1347575, 'GVL', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1154412, 'GVL', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1310517, 'GVL', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1154413, 'GVL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1154415, 'GVL', 'Support Volunteer', 'Load In Volunteer', NULL),
(1259800, 'GVL', 'Support Volunteer', 'Load Out Volunteer', NULL),
(1300387, 'GVL', 'Support Volunteer', 'New Serve Team', NULL),
(1164462, 'GVL', 'Support Volunteer', 'New Serve Team', NULL),
(872858, 'GVL', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1040960, 'GVL', 'Preschool Volunteer', 'Police Volunteer', NULL),
(872861, 'GVL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1040973, 'GVL', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1274626, 'GVL', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(872862, 'GVL', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(872863, 'GVL', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(872864, 'GVL', 'Next Steps Volunteer', 'Care Office Team', NULL),
(872869, 'GVL', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1103859, 'GVL', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1239840, 'GVL', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(872894, 'GVL', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(872870, 'GVL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1036953, 'GVL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1342581, 'GVL', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(872873, 'GVL', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(872874, 'GVL', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1212649, 'GVL', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1093017, 'GVL', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1028225, 'GVL', 'Next Steps Volunteer', 'Group Leader', NULL),
(818143, 'GVL', 'Next Steps Volunteer', 'Group Training', NULL),
(818141, 'GVL', 'Next Steps Volunteer', 'Groups Connector', NULL),
(1289963, 'GVL', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(842094, 'GVL', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1336857, 'GVL', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1212650, 'GVL', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(818145, 'GVL', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(818142, 'GVL', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1289964, 'GVL', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(842095, 'GVL', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1336858, 'GVL', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1443708, 'GVL', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(802766, 'GVL', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1212653, 'GVL', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(818328, 'GVL', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(818135, 'GVL', 'Next Steps Volunteer', 'Events Office Team', NULL),
(818109, 'GVL', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(821427, 'GVL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(817173, 'GVL', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1407498, 'GVL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1443710, 'GVL', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(872871, 'GVL', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1289966, 'GVL', 'Next Steps Volunteer', 'Prayer Team', NULL),
(842097, 'GVL', 'Next Steps Volunteer', 'Resource Center', NULL),
(1336860, 'GVL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(872781, 'GVL', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(802747, 'GVL', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1212627, 'GVL', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(802146, 'GVL', 'Production Volunteer', 'Preschool Production', NULL),
(802181, 'GVL', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(802196, 'GVL', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(817139, 'GVL', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(872809, 'GVL', 'Next Steps Volunteer', 'Writing Team', NULL),
(1289924, 'GVL', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1405678, 'GVL', 'Production Volunteer', 'Production Area Leader', NULL),
(1336815, 'GVL', 'Production Volunteer', 'Production Service Leader', NULL),
(872730, 'GVL', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1375038, 'GVL', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1408697, 'GVL', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(802741, 'GVL', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1268148, 'GVL', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(802736, 'GVL', 'Special Needs Attendee', 'Spring Zone', NULL),
(1212636, 'GVL', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1285697, 'GVL', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1390926, 'GVL', 'Support Volunteer', 'Sunday Support Volunteer', NULL),
(1285698, 'GVL', 'Nursery Attendee', 'Toddlers', 'Wonder Way 6'),
(1285704, 'GVL', 'Nursery Attendee', 'Toddlers', 'Wonder Way 7'),
(1285705, 'GVL', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1285715, 'GVL', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1285714, 'GVL', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1244663, 'GVL', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1214153, 'GVL', 'Nursery Attendee', 'Walkers', 'Wonder Way 5'),
(1214154, 'GVL', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1285699, 'GVL', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1285702, 'GVL', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1390927, 'GVL', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1285701, 'GVL', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1214149, 'GVL', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(1214150, 'GVL', 'Nursery Volunteer', 'Wonder Way 6 Volunteer', NULL),
(1233810, 'GVL', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1285708, 'GVL', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1285700, 'GVL', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1285710, 'GVL', 'Nursery Volunteer', 'Wonder Way 7 Volunteer', NULL),
(1285709, 'GVL', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1214151, 'GVL', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1214152, 'GWD', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1244661, 'GWD', 'Preschool Attendee', '34-35 mo.', 'Lil'' Spring'),
(1214095, 'GWD', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(1214096, 'GWD', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1238144, 'GWD', 'Support Volunteer', 'Advocate', NULL),
(1285717, 'GWD', 'Elementary Attendee', 'Base Camp', NULL),
(1285706, 'GWD', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1143876, 'GWD', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1143877, 'GWD', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1244660, 'GWD', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1301531, 'GWD', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1232882, 'GWD', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1214094, 'GWD', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1285696, 'GWD', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1390928, 'GWD', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1304034, 'GWD', 'Production Volunteer', 'Elementary Production', NULL),
(1390929, 'GWD', 'Production Volunteer', 'Elementary Production ', NULL),
(1285695, 'GWD', 'Production Volunteer', 'Elementary Production Service Leader', NULL),
(1285713, 'GWD', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1285707, 'GWD', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1285772, 'GWD', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1290766, 'GWD', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1289970, 'GWD', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1289971, 'GWD', 'Elementary Attendee', 'ImagiNation K', NULL),
(1246846, 'GWD', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1310559, 'GWD', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1310560, 'GWD', 'Guest Services Volunteer', 'Area Leader', NULL),
(1310561, 'GWD', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1310562, 'GWD', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1310563, 'GWD', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1310564, 'GWD', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1310550, 'GWD', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1310551, 'GWD', 'Fuse Attendee', '10th Grade Student', NULL),
(1310552, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1310553, 'GWD', 'Fuse Attendee', '10th Grade Student', NULL),
(1310554, 'GWD', 'Fuse Attendee', '11th Grade Student', NULL),
(1310555, 'GWD', 'Fuse Attendee', '11th Grade Student', NULL),
(1310556, 'GWD', 'Fuse Attendee', '12th Grade Student', NULL),
(1310558, 'GWD', 'Fuse Attendee', '12th Grade Student', NULL),
(1348933, 'GWD', 'Fuse Attendee', '6th Grade Student', NULL),
(1310533, 'GWD', 'Fuse Attendee', '6th Grade Student', NULL),
(1343610, 'GWD', 'Fuse Attendee', '7th Grade Student', NULL),
(1343612, 'GWD', 'Fuse Attendee', '7th Grade Student', NULL),
(1310545, 'GWD', 'Fuse Attendee', '8th Grade Student', NULL),
(1348934, 'GWD', 'Fuse Attendee', '8th Grade Student', NULL),
(1310535, 'GWD', 'Fuse Attendee', '9th Grade Student', NULL),
(1348936, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1310536, 'GWD', 'Fuse Attendee', '9th Grade Student', NULL),
(1348935, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1310537, 'GWD', 'Fuse Volunteer', 'Atrium', NULL),
(1362004, 'GWD', 'Fuse Volunteer', 'Campus Safety', NULL),
(1310539, 'GWD', 'Fuse Volunteer', 'Care', NULL),
(1310540, 'GWD', 'Fuse Volunteer', 'Check-In', NULL),
(1310534, 'GWD', 'Fuse Volunteer', 'Sports', NULL),
(1310541, 'GWD', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1310542, 'GWD', 'Fuse Volunteer', 'Office Team', NULL),
(1310531, 'GWD', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1310544, 'GWD', 'Fuse Volunteer', 'Game Room', NULL),
(1362006, 'GWD', 'Fuse Volunteer', 'Greeter', NULL),
(1310548, 'GWD', 'Fuse Volunteer', 'Sports', NULL),
(1343630, 'GWD', 'Fuse Volunteer', 'VIP Team', NULL),
(1310546, 'GWD', 'Fuse Volunteer', 'Leadership Team', NULL),
(1310547, 'GWD', 'Fuse Volunteer', 'Lounge', NULL),
(1310532, 'GWD', 'Fuse Volunteer', 'New Serve', NULL),
(1289844, 'GWD', 'Fuse Volunteer', 'Parking', NULL),
(1390182, 'GWD', 'Fuse Volunteer', 'Production', NULL),
(1267527, 'GWD', 'Fuse Volunteer', 'Leadership Team', NULL),
(1267528, 'GWD', 'Fuse Volunteer', 'Snack Bar', NULL),
(1267538, 'GWD', 'Fuse Volunteer', 'Sports', NULL),
(1274627, 'GWD', 'Fuse Volunteer', 'Usher', NULL),
(1267529, 'GWD', 'Fuse Volunteer', 'VHQ', NULL),
(1267530, 'GWD', 'Fuse Volunteer', 'Worship', NULL),
(1267532, 'GWD', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1289847, 'GWD', 'Guest Services Attendee', 'Awake Team', NULL),
(1289848, 'GWD', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1289849, 'GWD', 'Guest Services Volunteer', 'Facilities Volunteer', NULL),
(1267521, 'GWD', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1289845, 'GWD', 'Guest Services Volunteer', 'Finance Team', NULL),
(1267533, 'GWD', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1384270, 'GWD', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1267534, 'GWD', 'Guest Services Attendee', 'Greeting Team', NULL),
(1267536, 'GWD', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1267537, 'GWD', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(870966, 'GWD', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(935216, 'GWD', 'Guest Services Volunteer', 'New Serve Team', NULL),
(818123, 'GWD', 'Guest Services Attendee', 'Office Team', NULL),
(931365, 'GWD', 'Guest Services Attendee', 'Parking Team', NULL),
(817145, 'GWD', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(872819, 'GWD', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1289933, 'GWD', 'Guest Services Volunteer', 'Usher Team', NULL),
(842078, 'GWD', 'Guest Services Attendee', 'VHQ Team', NULL),
(1336823, 'GWD', 'Support Volunteer', 'New Serve Area Leader', NULL),
(872742, 'GWD', 'Support Volunteer', 'New Serve Team', NULL),
(1212640, 'GWD', 'Support Volunteer', 'New Serve Team', NULL),
(1210230, 'GWD', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1382561, 'GWD', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(821417, 'GWD', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(821423, 'GWD', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(872820, 'GWD', 'Production Volunteer', 'Preschool Production', NULL),
(1289937, 'GWD', 'Production Volunteer', 'Preschool Production ', NULL),
(1421597, 'GWD', 'Production Volunteer', 'Preschool Production Service Leader', NULL),
(1190758, 'GWD', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(802742, 'GWD', 'Creativity & Tech Volunteer', 'Band', NULL),
(1212638, 'GWD', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(818319, 'GWD', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(818126, 'GWD', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(818100, 'GWD', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(817150, 'GWD', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(872821, 'GWD', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1289935, 'GWD', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1405680, 'GWD', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1336824, 'GWD', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(872744, 'GWD', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(802759, 'GWD', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1212644, 'GWD', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(856973, 'GWD', 'Next Steps Volunteer', 'Events Office Team', NULL),
(872846, 'GWD', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1289958, 'GWD', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1336851, 'GWD', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(872769, 'GWD', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1382567, 'GWD', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1443713, 'GWD', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1407551, 'GWD', 'Next Steps Volunteer', 'Group Training', NULL),
(1443714, 'GWD', 'Next Steps Volunteer', 'Groups Connector', NULL),
(802761, 'GWD', 'Next Steps Volunteer', 'Group Leader', NULL),
(1212639, 'GWD', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(818320, 'GWD', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(818127, 'GWD', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(818101, 'GWD', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(817154, 'GWD', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(872822, 'GWD', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1289936, 'GWD', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1405682, 'GWD', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1336825, 'GWD', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1405828, 'GWD', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(872745, 'GWD', 'Production Volunteer', 'Production Service Leader', NULL),
(1212645, 'GWD', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1382564, 'GWD', 'Elementary Attendee', 'Shockwave 5th', NULL),
(821421, 'GWD', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1285773, 'GWD', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1285712, 'GWD', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1285716, 'GWD', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1301532, 'GWD', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1290765, 'GWD', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1104836, 'GWD', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1239832, 'GWD', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1184042, 'GWD', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1068642, 'GWD', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1283392, 'GWD', 'Next Steps Volunteer', 'Resource Center', NULL),
(1390972, 'GWD', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1283393, 'GWD', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1283397, 'GWD', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1283398, 'GWD', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1244666, 'GWD', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1214163, 'GWD', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1214164, 'GWD', 'Special Needs Attendee', 'Spring Zone', NULL),
(1283394, 'GWD', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1283400, 'GWD', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1283399, 'GWD', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1283396, 'GWD', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1214165, 'GWD', 'Nursery Attendee', 'Toddlers', 'Wonder Way 4'),
(1214166, 'GWD', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1233811, 'GWD', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1283395, 'GWD', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1283386, 'GWD', 'Guest Services Volunteer', 'Finance Office Team', NULL),
(1283384, 'GWD', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1283385, 'GWD', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1214159, 'GWD', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(1214161, 'GWD', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1340627, 'GWD', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1244665, 'GWD', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1214156, 'GWD', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1214158, 'GWD', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1238145, 'GWD', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1328869, 'GWD', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1283770, 'GWD', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1283388, 'GWD', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1098605, 'GWD', 'Fuse Volunteer', 'Spring Zone', NULL),
(1104824, 'GWD', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1098606, 'GWD', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1098607, 'GWD', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1104829, 'HHD', 'Elementary Attendee', 'Base Camp', NULL),
(1098608, 'HHD', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1104825, 'HHD', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1098609, 'HHD', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1104826, 'HHD', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1098610, 'LEX', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1104827, 'LEX', 'Preschool Attendee', '36-37 mo.', 'SpringTown Toys'),
(1098611, 'LEX', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1104828, 'LEX', 'Support Volunteer', 'Advocate', NULL),
(1098597, 'LEX', 'Elementary Attendee', 'Base Camp', NULL),
(1098809, 'LEX', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1098598, 'LEX', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1098599, 'LEX', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1098600, 'LEX', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1104819, 'LEX', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1098601, 'LEX', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1104820, 'LEX', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1098602, 'LEX', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1098603, 'LEX', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1104822, 'LEX', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1098604, 'LEX', 'Production Volunteer', 'Elementary Production ', NULL),
(1364348, 'LEX', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1126284, 'LEX', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1274587, 'LEX', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1126274, 'LEX', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1126275, 'LEX', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1126289, 'LEX', 'Elementary Attendee', 'ImagiNation K', NULL),
(1147615, 'LEX', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1388945, 'LEX', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1126276, 'LEX', 'Guest Services Volunteer', 'Area Leader', NULL),
(1126277, 'LEX', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1271751, 'LEX', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1365227, 'LEX', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1271750, 'LEX', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1126285, 'LEX', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1239824, 'LEX', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1126278, 'LEX', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1126279, 'LEX', 'Support Volunteer', 'Load In Volunteer', NULL),
(1126287, 'LEX', 'Support Volunteer', 'Load Out Volunteer', NULL),
(1126280, 'LEX', 'Support Volunteer', 'New Serve Team', NULL),
(1365244, 'LEX', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1126288, 'LEX', 'Creativity & Tech Volunteer', 'Band', NULL),
(1362008, 'LEX', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1271963, 'LEX', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1104830, 'LEX', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1126281, 'LEX', 'Fuse Attendee', '10th Grade Student', NULL),
(1126282, 'LEX', 'Fuse Attendee', '10th Grade Student', NULL),
(1259801, 'LEX', 'Fuse Attendee', '11th Grade Student', NULL),
(1300388, 'LEX', 'Fuse Attendee', '11th Grade Student', NULL),
(842124, 'LEX', 'Fuse Attendee', '12th Grade Student', NULL),
(842125, 'LEX', 'Fuse Attendee', '12th Grade Student', NULL),
(842122, 'LEX', 'Fuse Attendee', '6th Grade Student', NULL),
(1040970, 'LEX', 'Fuse Attendee', '6th Grade Student', NULL),
(1274628, 'LEX', 'Fuse Attendee', '7th Grade Student', NULL),
(1039818, 'LEX', 'Fuse Attendee', '7th Grade Student', NULL),
(842126, 'LEX', 'Fuse Attendee', '8th Grade Student', NULL),
(1264772, 'LEX', 'Fuse Attendee', '8th Grade Student', NULL),
(842129, 'LEX', 'Fuse Attendee', '9th Grade Student', NULL),
(842130, 'LEX', 'Fuse Attendee', '9th Grade Student', NULL),
(1239841, 'LEX', 'Fuse Volunteer', 'Atrium', NULL),
(842119, 'LEX', 'Fuse Volunteer', 'Campus Safety', NULL),
(842131, 'LEX', 'Fuse Volunteer', 'Check-In', NULL),
(1088082, 'LEX', 'Fuse Volunteer', 'Sports', NULL),
(1036951, 'LEX', 'Fuse Volunteer', 'Sports', NULL),
(1342585, 'LEX', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(842133, 'LEX', 'Fuse Volunteer', 'Fuse Guest', NULL),
(842134, 'LEX', 'Fuse Volunteer', 'Game Room', NULL),
(1127217, 'LEX', 'Fuse Volunteer', 'Greeter', NULL),
(1289959, 'LEX', 'Fuse Volunteer', 'Atrium', NULL),
(1420939, 'LEX', 'Fuse Volunteer', 'Atrium', NULL),
(1336852, 'LEX', 'Fuse Volunteer', 'Leadership Team', NULL),
(1190764, 'LEX', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(1212646, 'LEX', 'Fuse Volunteer', 'Lounge', NULL),
(854476, 'LEX', 'Fuse Volunteer', 'Campus Safety', NULL),
(872847, 'LEX', 'Fuse Volunteer', 'New Serve', NULL),
(1289960, 'LEX', 'Fuse Volunteer', 'Parking', NULL),
(1420942, 'LEX', 'Fuse Volunteer', 'Production', NULL),
(1336853, 'LEX', 'Fuse Volunteer', 'Snack Bar', NULL),
(872770, 'LEX', 'Fuse Volunteer', 'Sports', NULL),
(778081, 'LEX', 'Fuse Volunteer', 'VIP Team', NULL),
(1212119, 'LEX', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(802873, 'LEX', 'Fuse Volunteer', 'Usher', NULL),
(1408711, 'LEX', 'Fuse Volunteer', 'VHQ', NULL),
(800685, 'LEX', 'Fuse Volunteer', 'Worship', NULL),
(802848, 'LEX', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(800631, 'LEX', 'Guest Services Attendee', 'Awake Team', NULL),
(872804, 'LEX', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1289920, 'LEX', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(842067, 'LEX', 'Guest Services Volunteer', 'Finance Team', NULL),
(1336807, 'LEX', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(872724, 'LEX', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(778082, 'LEX', 'Guest Services Attendee', 'Greeting Team', NULL),
(1212120, 'LEX', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(802874, 'LEX', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(800686, 'LEX', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(802849, 'LEX', 'Guest Services Volunteer', 'New Serve Team', NULL),
(800632, 'LEX', 'Guest Services Attendee', 'Office Team', NULL),
(872806, 'LEX', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1289921, 'LEX', 'Guest Services Attendee', 'Parking Team', NULL),
(842068, 'LEX', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1336809, 'LEX', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(872725, 'LEX', 'Guest Services Volunteer', 'Usher Team', NULL),
(802732, 'LEX', 'Guest Services Attendee', 'VHQ Team', NULL),
(1212617, 'LEX', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(818312, 'LEX', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1413379, 'LEX', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(935222, 'LEX', 'Production Volunteer', 'Preschool Production', NULL),
(1143885, 'LEX', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1143884, 'LEX', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1244664, 'LEX', 'Production Volunteer', 'Production Area Leader', NULL),
(1232883, 'LEX', 'Production Volunteer', 'Production Service Leader', NULL),
(1214162, 'LEX', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1283390, 'LEX', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1390974, 'LEX', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1304035, 'LEX', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1283391, 'LEX', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1283381, 'LEX', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1283389, 'LEX', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1285774, 'LEX', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1285775, 'LEX', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1283380, 'LEX', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1391007, 'LEX', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(802179, 'LEX', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1473765, 'LEX', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1473776, 'LEX', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1473783, 'LEX', 'Next Steps Volunteer', 'Group Training', NULL),
(1473786, 'LEX', 'Next Steps Volunteer', 'Groups Connector', NULL),
(1473793, 'LEX', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1473796, 'LEX', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1473799, 'LEX', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1473802, 'LEX', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1473805, 'LEX', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1473808, 'LEX', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1263119, 'LEX', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1390997, 'LEX', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1263120, 'LEX', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1263149, 'LEX', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1263117, 'LEX', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1263118, 'LEX', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1263115, 'LEX', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1263125, 'LEX', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1263105, 'LEX', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1263106, 'LEX', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1263107, 'LEX', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1347705, 'LEX', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1347706, 'LEX', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1347707, 'LEX', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1347715, 'LEX', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1375103, 'LEX', 'Next Steps Volunteer', 'Resource Center', NULL),
(1375136, 'LEX', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1375138, 'LEX', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1375142, 'LEX', 'Next Steps Volunteer', 'Group Leader', NULL),
(1378245, 'LEX', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1263128, 'LEX', 'Special Needs Attendee', 'Spring Zone', NULL),
(1263142, 'LEX', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1263144, 'LEX', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1263108, 'LEX', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1263110, 'LEX', 'Next Steps Volunteer', 'District Leader', NULL),
(1263109, 'LEX', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1263112, 'LEX', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1263113, 'LEX', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1263114, 'LEX', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1283772, 'LEX', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(1263135, 'LEX', 'Fuse Volunteer', 'Fuse Office Team', NULL),
(1263100, 'LEX', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1263101, 'LEX', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1263102, 'LEX', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1301533, 'LEX', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1301534, 'LEX', 'Fuse Volunteer', 'Next Steps', NULL),
(1263129, 'LEX', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1263104, 'LEX', 'Fuse Volunteer', 'Pick-Up ', NULL),
(1390999, 'LEX', 'Guest Services Volunteer', 'Sign Language Team', NULL),
(1263131, 'LEX', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1304036, 'LEX', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1263132, 'LEX', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1263153, 'MYR', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1263136, 'MYR', 'Preschool Attendee', '32-33 mo.', 'Lil'' Spring'),
(1285777, 'MYR', 'Preschool Attendee', '42-43 mo.', 'SpringTown Toys'),
(1263156, 'MYR', 'Preschool Attendee', '54-55 mo.', 'Treehouse'),
(1263140, 'MYR', 'Support Volunteer', 'Advocate', NULL),
(1473811, 'MYR', 'Elementary Attendee', 'Base Camp', NULL),
(1473818, 'MYR', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1473821, 'MYR', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1473824, 'MYR', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1335020, 'MYR', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1283383, 'MYR', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1361878, 'MYR', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1290767, 'MYR', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1104837, 'MYR', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1239833, 'MYR', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1258663, 'MYR', 'Production Volunteer', 'Elementary Production', NULL),
(1258664, 'MYR', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1342768, 'MYR', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1219525, 'MYR', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1164430, 'MYR', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1283472, 'MYR', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1391002, 'MYR', 'Elementary Attendee', 'ImagiNation K', NULL),
(1283473, 'MYR', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1283477, 'MYR', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1335217, 'MYR', 'Guest Services Volunteer', 'Area Leader', NULL),
(1335218, 'MYR', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1335219, 'MYR', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1335233, 'MYR', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1335223, 'MYR', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1335221, 'MYR', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(1335224, 'MYR', 'Guest Services Attendee', 'Awake Team', NULL),
(1335225, 'MYR', 'Support Volunteer', 'Load In Volunteer', NULL),
(1335226, 'MYR', 'Support Volunteer', 'Load Out Volunteer', NULL),
(1335227, 'MYR', 'Support Volunteer', 'New Serve Team', NULL),
(1335228, 'MYR', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1381563, 'MYR', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1335229, 'MYR', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(1384272, 'MYR', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1335230, 'MYR', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1335220, 'MYR', 'Fuse Attendee', '10th Grade Student', NULL),
(1335231, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1335232, 'MYR', 'Fuse Attendee', '10th Grade Student', NULL),
(818095, 'MYR', 'Fuse Attendee', '10th Grade Student', NULL),
(817137, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(872835, 'MYR', 'Fuse Attendee', '11th Grade Student', NULL),
(1289949, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1200986, 'MYR', 'Fuse Attendee', '11th Grade Student', NULL),
(1336833, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(952366, 'MYR', 'Fuse Attendee', '12th Grade Student', NULL),
(802739, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1060526, 'MYR', 'Fuse Attendee', '12th Grade Student', NULL),
(1060527, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1200646, 'MYR', 'Fuse Attendee', '6th Grade Student', NULL),
(1413357, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(778084, 'MYR', 'Fuse Attendee', '6th Grade Student', NULL),
(1212121, 'MYR', 'Fuse Attendee', '7th Grade Student', NULL),
(802881, 'MYR', 'Fuse Attendee', '7th Grade Student', NULL),
(802878, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(802880, 'MYR', 'Fuse Attendee', '8th Grade Student', NULL),
(800635, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(872807, 'MYR', 'Fuse Attendee', '8th Grade Student', NULL),
(1289922, 'MYR', 'Fuse Attendee', '9th Grade Student', NULL),
(842070, 'MYR', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1336812, 'MYR', 'Fuse Attendee', '9th Grade Student', NULL),
(872727, 'MYR', 'Fuse Volunteer', 'Atrium', NULL),
(1344518, 'MYR', 'Fuse Volunteer', 'Campus Safety', NULL),
(1497243, 'MYR', 'Fuse Volunteer', 'Care', NULL),
(1386015, 'MYR', 'Fuse Volunteer', 'Check-In', NULL),
(821419, 'MYR', 'Fuse Volunteer', 'Sports', NULL),
(1087936, 'MYR', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1443719, 'MYR', 'Fuse Volunteer', 'Office Team', NULL),
(1297161, 'MYR', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1336841, 'MYR', 'Fuse Volunteer', 'Game Room', NULL),
(1102924, 'MYR', 'Fuse Volunteer', 'Greeter', NULL),
(1294812, 'MYR', 'Fuse Volunteer', 'Lounge', NULL),
(1102965, 'MYR', 'Fuse Volunteer', 'Leadership Team', NULL),
(1294818, 'MYR', 'Fuse Volunteer', 'Lounge', NULL),
(1404065, 'MYR', 'Fuse Volunteer', 'Campus Safety', NULL),
(1297084, 'MYR', 'Fuse Volunteer', 'New Serve', NULL),
(1212647, 'MYR', 'Fuse Volunteer', 'Parking', NULL),
(1386014, 'MYR', 'Fuse Volunteer', 'Pick-Up', NULL),
(1443720, 'MYR', 'Fuse Volunteer', 'Production', NULL),
(1289961, 'MYR', 'Fuse Volunteer', 'Snack Bar', NULL),
(1336835, 'MYR', 'Fuse Volunteer', 'Sports', NULL),
(930573, 'MYR', 'Fuse Volunteer', 'Spring Zone', NULL),
(800900, 'MYR', 'Fuse Volunteer', 'Student Leader', NULL),
(1212648, 'MYR', 'Fuse Volunteer', 'Leadership Team', NULL),
(1283478, 'MYR', 'Fuse Volunteer', 'VIP Team', NULL),
(1283491, 'MYR', 'Fuse Volunteer', 'Usher', NULL),
(1244669, 'MYR', 'Fuse Volunteer', 'VHQ', NULL),
(1213999, 'MYR', 'Fuse Volunteer', 'Worship', NULL),
(1214000, 'MYR', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1283474, 'MYR', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1283480, 'MYR', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1283479, 'MYR', 'Guest Services Volunteer', 'Finance Team', NULL),
(1213995, 'MYR', 'Guest Services Volunteer', 'Finance Team', NULL),
(1213996, 'MYR', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1233812, 'MYR', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1283483, 'MYR', 'Guest Services Attendee', 'Greeting Team', NULL),
(1283475, 'MYR', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1283484, 'MYR', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1213993, 'MYR', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1213994, 'MYR', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1244668, 'MYR', 'Guest Services Attendee', 'Office Team', NULL),
(1213997, 'MYR', 'Guest Services Attendee', 'Parking Team', NULL),
(1213998, 'MYR', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1238146, 'MYR', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1283771, 'MYR', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1283481, 'MYR', 'Guest Services Volunteer', 'Usher Team', NULL),
(1143893, 'MYR', 'Guest Services Attendee', 'VHQ Team', NULL),
(1143894, 'MYR', 'Production Volunteer', 'Production Area Leader', NULL),
(1244667, 'MYR', 'Production Volunteer', 'Production Service Leader', NULL),
(1213992, 'MYR', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1283470, 'MYR', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1391004, 'MYR', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1304037, 'MYR', 'Creativity & Tech Volunteer', 'Band', NULL),
(1283471, 'MYR', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1283489, 'MYR', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1283482, 'MYR', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1285778, 'MYR', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1285779, 'MYR', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1283488, 'MYR', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1361889, 'MYR', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1283382, 'MYR', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1283490, 'MYR', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1473844, 'MYR', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1289969, 'MYR', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1335234, 'MYR', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1263139, 'MYR', 'Special Needs Attendee', 'Spring Zone', NULL),
(1335011, 'MYR', 'Special Needs Volunteer', 'Spring Zone Jr. Volunteer', NULL),
(1506903, 'MYR', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1506943, 'MYR', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1335239, 'MYR', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1335240, 'MYR', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1384261, 'MYR', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1335237, 'MYR', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1335238, 'MYR', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1335236, 'MYR', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1340633, 'MYR', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1368374, 'MYR', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1368375, 'MYR', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1368376, 'MYR', 'Next Steps Volunteer', 'Group Leader', NULL),
(1368377, 'MYR', 'Next Steps Volunteer', 'Group Training', NULL),
(1368378, 'MYR', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1368379, 'MYR', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1368381, 'MYR', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1368382, 'MYR', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1368383, 'MYR', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1368384, 'MYR', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1368385, 'MYR', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1368386, 'MYR', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1368372, 'MYR', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1368373, 'MYR', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1368406, 'MYR', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1368399, 'MYR', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1368407, 'MYR', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1368408, 'MYR', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1368398, 'MYR', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1384262, 'MYR', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1388946, 'MYR', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1368409, 'MYR', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1368410, 'MYR', 'Next Steps Volunteer', 'Resource Center', NULL),
(1368403, 'MYR', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1368411, 'MYR', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(1368415, 'MYR', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1368412, 'MYR', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1368413, 'MYR', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1368414, 'MYR', 'Nursery Attendee', 'Toddlers', 'Wonder Way 4'),
(1368416, 'MYR', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1368419, 'MYR', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1368400, 'MYR', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1368420, 'MYR', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1368421, 'MYR', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(1368402, 'MYR', 'Guest Services Attendee', 'Office Team', NULL),
(1368404, 'MYR', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(1368422, 'MYR', 'Next Steps Volunteer', 'Groups Connector', NULL),
(1368423, 'MYR', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1368401, 'MYR', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1197512, 'MYR', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1267382, 'MYR', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(871836, 'MYR', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(871837, 'MYR', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(871852, 'MYR', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1040972, 'MYR', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1274629, 'MYR', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(871838, 'NEC', 'Elementary Attendee', 'Base Camp', NULL),
(871839, 'NEC', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(871840, 'NEC', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1135970, 'NEC', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(871853, 'NEC', 'Next Steps Attendee', 'Financial Peace University', NULL),
(871845, 'NEC', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1384273, 'NEC', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1165257, 'POW', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1162872, 'POW', 'Preschool Attendee', '36-37 mo.', 'SpringTown Toys'),
(1403998, 'POW', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1081247, 'POW', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1413277, 'POW', 'Support Volunteer', 'Advocate', NULL),
(802740, 'POW', 'Elementary Attendee', 'Base Camp', NULL),
(1212641, 'POW', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(818318, 'POW', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(935217, 'POW', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(818124, 'POW', 'Support Volunteer', 'Check-In Volunteer', NULL),
(818098, 'POW', 'Support Volunteer', 'Check-In Volunteer', NULL),
(817146, 'POW', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(872823, 'POW', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1289938, 'POW', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(1200649, 'POW', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(1336826, 'POW', 'Production Volunteer', 'Elementary Production ', NULL),
(872746, 'POW', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(802762, 'POW', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1212642, 'POW', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(1102962, 'POW', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(1413376, 'POW', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(935218, 'POW', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(818125, 'POW', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(818099, 'POW', 'Elementary Attendee', 'ImagiNation K', NULL),
(817148, 'POW', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(872825, 'POW', 'Guest Services Volunteer', 'Area Leader', NULL),
(1289939, 'POW', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1304995, 'POW', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1336827, 'POW', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1335017, 'POW', 'Support Volunteer', 'KidSpring Greeter', NULL),
(1335018, 'POW', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1335007, 'POW', 'Support Volunteer', 'Load In Volunteer', NULL),
(1335008, 'POW', 'Support Volunteer', 'Load Out Volunteer', NULL),
(1335038, 'POW', 'Support Volunteer', 'New Serve Team', NULL),
(1335039, 'POW', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1335036, 'POW', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(1335023, 'POW', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(1335046, 'POW', 'Production Volunteer', 'Preschool Production', NULL),
(1335047, 'POW', 'Production Volunteer', 'Preschool Production ', NULL),
(1335012, 'POW', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1335014, 'POW', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(1335043, 'POW', 'Production Volunteer', 'Production Area Leader', NULL),
(1335044, 'POW', 'Production Volunteer', 'Production Service Leader', NULL),
(1335045, 'POW', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1335040, 'POW', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1335041, 'POW', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1335042, 'POW', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1335005, 'POW', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1335015, 'POW', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(1335049, 'POW', 'Guest Services Volunteer', 'Campus Safety', NULL),
(1335050, 'POW', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1335052, 'POW', 'Guest Services Volunteer', 'Finance Team', NULL),
(1335026, 'POW', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1335027, 'POW', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(1335053, 'POW', 'Guest Services Attendee', 'Greeting Team', NULL),
(1335054, 'POW', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1335028, 'POW', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1391009, 'POW', 'Guest Services Attendee', 'Load In/Load Out', NULL),
(1335029, 'POW', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1391010, 'POW', 'Guest Services Attendee', 'Office Team', NULL),
(1335030, 'POW', 'Guest Services Attendee', 'Parking Team', NULL),
(1335009, 'POW', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1335016, 'POW', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1335031, 'POW', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1335010, 'POW', 'Guest Services Volunteer', 'Usher Team', NULL),
(1335006, 'POW', 'Guest Services Attendee', 'VHQ Team', NULL),
(1335013, 'POW', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(1335021, 'POW', 'Special Needs Attendee', 'Spring Zone', NULL),
(1506999, 'POW', 'Special Needs Volunteer', 'Spring Zone Area Leader', NULL),
(1507000, 'POW', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(1507001, 'POW', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1507028, 'POW', 'Next Steps Volunteer', 'District Leader', NULL),
(1507029, 'POW', 'Creativity & Tech Volunteer', 'Band', NULL),
(1325467, 'POW', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1361842, 'POW', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1507034, 'POW', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(872749, 'POW', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(776137, 'POW', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1361998, 'POW', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(776136, 'POW', 'Fuse Attendee', '10th Grade Student', NULL),
(1212123, 'POW', 'Fuse Attendee', '10th Grade Student', NULL),
(1290768, 'POW', 'Fuse Attendee', '11th Grade Student', NULL),
(1104838, 'POW', 'Fuse Attendee', '11th Grade Student', NULL),
(1100586, 'POW', 'Fuse Attendee', '12th Grade Student', NULL),
(1239834, 'POW', 'Fuse Attendee', '12th Grade Student', NULL),
(1068650, 'POW', 'Fuse Attendee', '6th Grade Student', NULL),
(1258661, 'POW', 'Fuse Attendee', '6th Grade Student', NULL),
(1219527, 'POW', 'Fuse Attendee', '7th Grade Student', NULL),
(1300448, 'POW', 'Fuse Attendee', '7th Grade Student', NULL),
(1068649, 'POW', 'Fuse Attendee', '8th Grade Student', NULL),
(1024122, 'POW', 'Fuse Attendee', '8th Grade Student', NULL),
(1024103, 'POW', 'Fuse Attendee', '9th Grade Student', NULL),
(1024123, 'POW', 'Fuse Attendee', '9th Grade Student', NULL),
(1024105, 'POW', 'Fuse Volunteer', 'Atrium', NULL),
(1101309, 'POW', 'Fuse Volunteer', 'Campus Safety', NULL),
(1387993, 'POW', 'Fuse Volunteer', 'Care', NULL),
(1386965, 'POW', 'Fuse Volunteer', 'Check-In', NULL),
(872893, 'POW', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1390936, 'POW', 'Fuse Volunteer', 'Office Team', NULL),
(1390960, 'POW', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1390966, 'POW', 'Fuse Volunteer', 'Game Room', NULL),
(1390973, 'POW', 'Fuse Volunteer', 'Greeter', NULL),
(1391003, 'POW', 'Fuse Volunteer', 'VIP Team', NULL),
(1473766, 'POW', 'Fuse Volunteer', 'Leadership Team', NULL),
(1473777, 'POW', 'Fuse Volunteer', 'Load In/Load Out', NULL),
(1473784, 'POW', 'Fuse Volunteer', 'Lounge', NULL),
(1473787, 'POW', 'Fuse Volunteer', 'New Serve', NULL),
(1473794, 'POW', 'Fuse Volunteer', 'Next Steps', NULL),
(1473797, 'POW', 'Fuse Volunteer', 'Parking', NULL),
(1473800, 'POW', 'Fuse Volunteer', 'Pick-Up', NULL),
(1473803, 'POW', 'Fuse Volunteer', 'Production', NULL),
(1473806, 'POW', 'Fuse Volunteer', 'Snack Bar', NULL),
(1473809, 'POW', 'Fuse Volunteer', 'Sports', NULL),
(1024127, 'POW', 'Fuse Volunteer', 'Spring Zone', NULL),
(1024107, 'POW', 'Fuse Volunteer', 'Student Leader', NULL),
(858500, 'POW', 'Fuse Volunteer', 'Usher', NULL),
(858502, 'POW', 'Fuse Volunteer', 'VHQ', NULL),
(828965, 'POW', 'Fuse Volunteer', 'Worship', NULL),
(1162869, 'POW', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1024129, 'POW', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1024108, 'POW', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1024131, 'POW', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1024109, 'POW', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1024132, 'POW', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1024110, 'POW', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1024133, 'POW', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1024111, 'POW', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1024134, 'POW', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1024112, 'POW', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1024135, 'POW', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1024136, 'POW', 'Next Steps Attendee', 'Fuse First Look', NULL),
(1024114, 'POW', 'Next Steps Volunteer', 'Group Leader', NULL),
(1024137, 'POW', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(1024138, 'POW', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1024124, 'POW', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1473812, 'POW', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1473819, 'POW', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1024118, 'POW', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1024125, 'POW', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1024119, 'POW', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1473822, 'POW', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1473825, 'POW', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1364349, 'POW', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1024086, 'POW', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1285790, 'POW', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1276077, 'POW', 'Next Steps Volunteer', 'Load In/Load Out', NULL),
(1283476, 'POW', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1335024, 'POW', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1289907, 'POW', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1493512, 'POW', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1335048, 'POW', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1361895, 'POW', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1326083, 'POW', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1362015, 'POW', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1023364, 'POW', 'Next Steps Volunteer', 'Resource Center', NULL),
(1024082, 'POW', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(872181, 'POW', 'Next Steps Volunteer', 'Sunday Care Team', NULL)

insert #rlcMap
values
(1388947, 'POW', 'Next Steps Volunteer', 'Writing Team', NULL),
(1362016, 'POW', 'Next Steps Volunteer', 'Groups Connector', NULL),
(1023359, 'POW', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1024097, 'POW', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1362017, 'POW', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(1024095, 'POW', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1336791, 'POW', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(872786, 'POW', 'Next Steps Volunteer', 'Group Training', NULL),
(800895, 'POW', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1340555, 'POW', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1310566, 'POW', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1264974, 'POW', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(1024081, 'POW', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1239825, 'POW', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1023361, 'POW', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(1023366, 'POW', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(1024084, 'RKH', 'Elementary Attendee', 'Base Camp', NULL),
(1024090, 'RKH', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1023358, 'SIM', 'Elementary Attendee', 'Base Camp', NULL),
(1362024, 'SIM', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1362022, 'SIM', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1024087, 'SIM', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(1024140, 'SIM', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1024142, 'SIM', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1023363, 'SIM', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1024077, 'SIM', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1259802, 'SPA', 'Preschool Attendee', '24-29 mo.', 'Pop''s Garage'),
(1242949, 'SPA', 'Preschool Attendee', '24-29 mo.', 'Spring Fresh'),
(1391012, 'SPA', 'Preschool Attendee', '36-37 mo.', 'Lil'' Spring'),
(1242947, 'SPA', 'Preschool Attendee', '44-45 mo.', 'SpringTown Toys'),
(1242954, 'SPA', 'Preschool Attendee', '48-49 mo.', 'Treehouse'),
(1242953, 'SPA', 'Support Volunteer', 'Advocate', NULL),
(1242979, 'SPA', 'Elementary Attendee', 'Base Camp', NULL),
(1242974, 'SPA', 'Preschool Attendee', 'Base Camp Jr.', NULL),
(1242973, 'SPA', 'Preschool Volunteer', 'Base Camp Jr. Volunteer', NULL),
(1244672, 'SPA', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1388944, 'SPA', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1211284, 'SPA', 'Support Volunteer', 'Check-In Volunteer', NULL),
(1335025, 'SPA', 'Nursery Attendee', 'Crawlers', 'Wonder Way 2'),
(1283486, 'SPA', 'Nursery Attendee', 'Cuddlers', 'Wonder Way 1'),
(1212630, 'SPA', 'Elementary Volunteer', 'Elementary Area Leader', NULL),
(818316, 'SPA', 'Elementary Volunteer', 'Elementary Early Bird ', NULL),
(935213, 'SPA', 'Production Volunteer', 'Elementary Production ', NULL),
(1211287, 'SPA', 'Elementary Volunteer', 'Elementary Service Leader', NULL),
(1242950, 'SPA', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(818121, 'SPA', 'Support Volunteer', 'First Time Team Volunteer', NULL),
(802200, 'SPA', 'Support Volunteer', 'Guest Services Area Leader', NULL),
(802237, 'SPA', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(872811, 'SPA', 'Support Volunteer', 'Guest Services Service Leader', NULL),
(1456000, 'SPA', 'Elementary Attendee', 'ImagiNation 1st', NULL),
(1450442, 'SPA', 'Elementary Attendee', 'ImagiNation K', NULL),
(1450446, 'SPA', 'Elementary Volunteer', 'ImagiNation Volunteer', NULL),
(1450447, 'SPA', 'Guest Services Volunteer', 'Area Leader', NULL),
(1444488, 'SPA', 'Elementary Attendee', 'Jump Street 2nd', NULL),
(1264973, 'SPA', 'Elementary Attendee', 'Jump Street 3rd', NULL),
(1242956, 'SPA', 'Elementary Volunteer', 'Jump Street Volunteer', NULL),
(1289927, 'SPA', 'Support Volunteer', 'KidSpring Greeter', NULL),
(842073, 'SPA', 'Support Volunteer', 'KidSpring Office Team', NULL),
(1336818, 'SPA', 'Preschool Volunteer', 'Lil'' Spring Volunteer', NULL),
(872732, 'SPA', 'Support Volunteer', 'New Serve Team', NULL),
(802748, 'SPA', 'Nursery Volunteer', 'Nursery Early Bird Volunteer', NULL),
(1212631, 'SPA', 'Preschool Volunteer', 'Pop''s Garage Volunteer', NULL),
(870960, 'SPA', 'Preschool Volunteer', 'Preschool Area Leader', NULL),
(935214, 'SPA', 'Preschool Volunteer', 'Preschool Early Bird Volunteer', NULL),
(802183, 'SPA', 'Production Volunteer', 'Preschool Production', NULL),
(818096, 'SPA', 'Preschool Volunteer', 'Preschool Service Leader', NULL),
(802239, 'SPA', 'Production Volunteer', 'Production Area Leader', NULL),
(872812, 'SPA', 'Production Volunteer', 'Production Service Leader', NULL),
(1289928, 'SPA', 'Elementary Attendee', 'Shockwave 4th', NULL),
(1159892, 'SPA', 'Elementary Attendee', 'Shockwave 5th', NULL),
(1336819, 'SPA', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(872733, 'SPA', 'Elementary Volunteer', 'Shockwave Volunteer', NULL),
(802749, 'SPA', 'Preschool Volunteer', 'Spring Fresh Volunteer', NULL),
(1212632, 'SPA', 'Special Needs Attendee', 'Spring Zone', NULL),
(1102949, 'SPA', 'Special Needs Volunteer', 'Spring Zone Service Leader', NULL),
(935215, 'SPA', 'Guest Services Attendee', 'Auditorium Reset Team', NULL),
(858501, 'SPA', 'Guest Services Attendee', 'Awake Team', NULL),
(858503, 'SPA', 'Guest Services Volunteer', 'Campus Safety', NULL),
(802241, 'SPA', 'Guest Services Attendee', 'Facility Cleaning Crew', NULL),
(1162885, 'SPA', 'Guest Services Volunteer', 'Finance Team', NULL),
(1289929, 'SPA', 'Guest Services Volunteer', 'Finance Team', NULL),
(1326091, 'SPA', 'Guest Services Attendee', 'VIP Room Attendee', NULL),
(1336820, 'SPA', 'Guest Services Volunteer', 'VIP Room Volunteer', NULL),
(872734, 'SPA', 'Guest Services Attendee', 'Greeting Team', NULL),
(802750, 'SPA', 'Guest Services Volunteer', 'Guest Services Team', NULL),
(1267825, 'SPA', 'Guest Services Volunteer', 'New Serve Team', NULL),
(1005221, 'SPA', 'Guest Services Attendee', 'Office Team', NULL),
(1005223, 'SPA', 'Guest Services Attendee', 'Parking Team', NULL),
(1428460, 'SPA', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1391013, 'SPA', 'Guest Services Volunteer', 'Service Coordinator', NULL),
(1242955, 'SPA', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(1242952, 'SPA', 'Fuse Volunteer', 'Sunday Fuse Team', NULL),
(1428458, 'SPA', 'Guest Services Volunteer', 'Usher Team', NULL),
(1211277, 'SPA', 'Guest Services Attendee', 'VHQ Team', NULL),
(1211278, 'SPA', 'Special Needs Volunteer', 'Spring Zone Volunteer', NULL),
(1233814, 'SPA', 'Support Volunteer', 'Sunday Support Volunteer ', NULL),
(1242963, 'SPA', 'Nursery Attendee', 'Toddlers', 'Wonder Way 4'),
(1242951, 'SPA', 'Nursery Attendee', 'Toddlers', 'Wonder Way 5'),
(1242965, 'SPA', 'Preschool Volunteer', 'Toys Volunteer', NULL),
(1242964, 'SPA', 'Creativity & Tech Volunteer', 'Editorial Team', NULL),
(1211281, 'SPA', 'Preschool Volunteer', 'Treehouse Volunteer', NULL),
(1211282, 'SPA', 'Creativity & Tech Volunteer', 'Band', NULL),
(1244671, 'SPA', 'Creativity & Tech Volunteer', 'Band Green Room', NULL),
(1428459, 'SPA', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(931363, 'SPA', 'Creativity & Tech Volunteer', 'New Serve Team', NULL),
(817142, 'SPA', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1211279, 'SPA', 'Creativity & Tech Volunteer', 'Load In/Load Out', NULL),
(1211280, 'SPA', 'Creativity & Tech Volunteer', 'Production Team', NULL),
(1238147, 'SPA', 'Creativity & Tech Volunteer', 'Office Team', NULL),
(1283768, 'SPA', 'Creativity & Tech Volunteer', 'Social Media/PR Team', NULL),
(1242960, 'SPA', 'Fuse Attendee', '10th Grade Student', NULL),
(1143904, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1143905, 'SPA', 'Fuse Attendee', '10th Grade Student', NULL),
(1335235, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1244670, 'SPA', 'Fuse Attendee', '11th Grade Student', NULL),
(1407196, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1362005, 'SPA', 'Fuse Attendee', '11th Grade Student', NULL),
(1456116, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1232885, 'SPA', 'Fuse Attendee', '12th Grade Student', NULL),
(1211276, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1242946, 'SPA', 'Fuse Attendee', '12th Grade Student', NULL),
(1391014, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1304038, 'SPA', 'Fuse Attendee', '6th Grade Student', NULL),
(1391015, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1242945, 'SPA', 'Fuse Attendee', '6th Grade Student', NULL),
(1242971, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1242961, 'SPA', 'Fuse Attendee', '7th Grade Student', NULL),
(1285780, 'SPA', 'Fuse Attendee', '7th Grade Student', NULL),
(1301526, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1423948, 'SPA', 'Fuse Attendee', '8th Grade Student', NULL),
(1162886, 'SPA', 'Fuse Attendee', '8th Grade Student', NULL),
(1289930, 'SPA', 'Fuse Attendee', '9th Grade Student', NULL),
(1387581, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1440426, 'SPA', 'Fuse Attendee', '9th Grade Student', NULL),
(1440775, 'SPA', 'Fuse Volunteer', 'Fuse Group Leader', NULL),
(1390925, 'SPA', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1390975, 'SPA', 'Fuse Volunteer', 'Atrium', NULL),
(1391005, 'SPA', 'Fuse Volunteer', 'Campus Safety', NULL),
(1440326, 'SPA', 'Fuse Volunteer', 'Care', NULL),
(1440422, 'SPA', 'Fuse Volunteer', 'Check-In', NULL),
(1310543, 'SPA', 'Fuse Volunteer', 'Fuse Guest', NULL),
(1404036, 'SPA', 'Fuse Volunteer', 'Office Team', NULL),
(1081249, 'SPA', 'Fuse Volunteer', 'Leadership Team', NULL),
(802751, 'SPA', 'Fuse Volunteer', 'Game Room', NULL),
(1285781, 'SPA', 'Fuse Volunteer', 'Greeter', NULL),
(1468903, 'SPA', 'Fuse Volunteer', 'Sports', NULL),
(1375311, 'SPA', 'Fuse Volunteer', 'Leadership Team', NULL),
(1375297, 'SPA', 'Fuse Volunteer', 'Lounge', NULL),
(1258906, 'SPA', 'Nursery Attendee', 'Walkers', 'Wonder Way 3'),
(1474090, 'SPA', 'Fuse Volunteer', 'Campus Safety', NULL),
(828968, 'SPA', 'Fuse Volunteer', 'New Serve', NULL),
(1413356, 'SPA', 'Fuse Volunteer', 'Parking', NULL),
(802752, 'SPA', 'Fuse Volunteer', 'Pick-Up', NULL),
(1413374, 'SPA', 'Fuse Volunteer', 'Production', NULL),
(1453818, 'SPA', 'Fuse Volunteer', 'Snack Bar', NULL),
(818140, 'SPA', 'Fuse Volunteer', 'Sports', NULL),
(802753, 'SPA', 'Fuse Volunteer', 'Spring Zone', NULL),
(1267535, 'SPA', 'Fuse Volunteer', 'Student Leader', NULL),
(1340614, 'SPA', 'Fuse Volunteer', 'VIP Team', NULL),
(1340621, 'SPA', 'Fuse Volunteer', 'VIP Team', NULL),
(1242970, 'SPA', 'Fuse Volunteer', 'VIP Team', NULL),
(1242975, 'SPA', 'Fuse Volunteer', 'Usher', NULL),
(1340623, 'SPA', 'Fuse Volunteer', 'VHQ', NULL),
(1340631, 'SPA', 'Fuse Volunteer', 'Worship', NULL),
(1340615, 'SPA', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1340624, 'SPA', 'Next Steps Attendee', 'Baptism Attendee', NULL),
(1491024, 'SPA', 'Next Steps Volunteer', 'Baptism Volunteer', NULL),
(1491025, 'SPA', 'Next Steps Attendee', 'Budget Class Attendee', NULL),
(1491026, 'SPA', 'Next Steps Volunteer', 'Budget Class Volunteer', NULL),
(1491027, 'SPA', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1491028, 'SPA', 'Next Steps Volunteer', 'Care Office Team', NULL),
(1491029, 'SPA', 'Next Steps Volunteer', 'Care Visitation Team', NULL),
(1491030, 'SPA', 'Next Steps Attendee', 'Creativity & Tech First Serve', NULL),
(1491031, 'SPA', 'Next Steps Attendee', 'Creativity & Tech Basics', NULL),
(1390909, 'SPA', 'Next Steps Attendee', 'Creativity & Tech First Look', NULL),
(1390969, 'SPA', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1390930, 'SPA', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1390976, 'SPA', 'Nursery Volunteer', 'Wonder Way 1 Volunteer', NULL),
(1391006, 'SPA', 'Nursery Volunteer', 'Wonder Way 2 Volunteer', NULL),
(1391016, 'SPA', 'Nursery Volunteer', 'Wonder Way 3 Volunteer', NULL),
(1458501, 'SPA', 'Next Steps Attendee', 'Financial Coaching Attendee', NULL),
(1458500, 'SPA', 'Next Steps Volunteer', 'Financial Coaching Volunteer', NULL),
(1268147, 'SPA', 'Next Steps Volunteer', 'Financial Planning Office Team', NULL),
(802754, 'SPA', 'Next Steps Attendee', 'Fuse Basics', NULL),
(1154425, 'SPA', 'Next Steps Attendee', 'Fuse First Look', NULL),
(961212, 'SPA', 'Next Steps Attendee', 'Fuse First Serve', NULL),
(1212629, 'SPA', 'Next Steps Volunteer', 'Group Leader', NULL),
(1210227, 'SPA', 'Next Steps Volunteer', 'Group Training', NULL),
(1382560, 'SPA', 'Next Steps Volunteer', 'Groups Connector', NULL),
(821416, 'SPA', 'Next Steps Volunteer', 'Groups Office Team', NULL),
(821422, 'SPA', 'Next Steps Attendee', 'Guest Services Basics', NULL),
(1466469, 'SPA', 'Next Steps Attendee', 'Guest Services First Look', NULL),
(1465005, 'SPA', 'Next Steps Attendee', 'Guest Services First Serve', NULL),
(1465009, 'SPA', 'Next Steps Attendee', 'KidSpring Basics', NULL),
(1465002, 'SPA', 'Next Steps Attendee', 'KidSpring First Look', NULL),
(1347703, 'SPA', 'Next Steps Attendee', 'KidSpring First Serve', NULL),
(1465003, 'SPA', 'Next Steps Volunteer', 'New Serve Team', NULL),
(1465004, 'SPA', 'Next Steps Volunteer', 'Next Steps Area', NULL),
(1465006, 'SPA', 'Next Steps Attendee', 'Next Steps Basics', NULL),
(1465001, 'SPA', 'Next Steps Attendee', 'Next Steps First Look', NULL),
(1465008, 'SPA', 'Next Steps Attendee', 'Next Steps First Serve', NULL),
(1347540, 'SPA', 'Next Steps Volunteer', 'Events Office Team', NULL),
(1465007, 'SPA', 'Next Steps Attendee', 'Opportunities Tour', NULL),
(1310527, 'SPA', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(1440736, 'SPA', 'Next Steps Attendee', 'Ownership Class Attendee', NULL),
(872808, 'SPA', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1289926, 'SPA', 'Next Steps Attendee', 'Ownership Class Current Owner', NULL),
(1420937, 'SPA', 'Next Steps Volunteer', 'Ownership Class Volunteer', NULL),
(1336817, 'SPA', 'Next Steps Volunteer', 'Prayer Team', NULL),
(1190759, 'SPA', 'Next Steps Volunteer', 'Resource Center', NULL),
(802760, 'SPA', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1477140, 'SPA', 'Nursery Volunteer', 'Wonder Way 4 Volunteer', NULL),
(1212628, 'SPA', 'Next Steps Volunteer', 'Special Event Volunteer', NULL),
(818313, 'SPA', 'Nursery Volunteer', 'Wonder Way 5 Volunteer', NULL),
(818120, 'SPA', 'Next Steps Volunteer', 'Sunday Care Team', NULL),
(1264975, 'SPA', 'Next Steps Volunteer', 'Writing Team', NULL),
(1508295, 'SPA', 'Guest Services Volunteer', 'Special Event Volunteer', NULL),
(802198, 'SPA', 'Next Steps Attendee', 'Special Event Attendee', NULL),
(1347714, 'SPA', 'Nursery Volunteer', 'Wonder Way Area Leader', NULL),
(817140, 'SPA', 'Nursery Volunteer', 'Wonder Way Service Leader', NULL),
(872810, 'SUM', 'Elementary Attendee', 'Base Camp', NULL),
(1289925, 'SUM', 'Elementary Volunteer', 'Base Camp Volunteer', NULL),
(1405679, 'SUM', 'Next Steps Attendee', 'Andy Stanley Bible Study', NULL),
(1336816, 'SUM', 'Next Steps Attendee', 'Beth Moore Bible Study', NULL),
(872731, 'SUM', 'Next Steps Attendee', 'Financial Peace University', NULL),
(1005207, 'SUM', 'Guest Services Volunteer', 'Guest Services Volunteer', NULL),
(1258904, 'SUM', 'Next Steps Attendee', 'Ownership Class Attendee', NULL)


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
		from F1..Attendance a
		inner join PersonAlias p
			on a.Individual_ID = p.ForeignId
			and a.RLC_ID = @RLCID
		left join #services s
			on s.serviceName = DATENAME(DW, a.Start_Date_Time)
		left join #services s2
			on s2.serviceTime = CONVERT(TIME, a.Start_Date_time)

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
		from F1..Staffing_Assignment sa
		inner join PersonAlias p
		on sa.Individual_ID = p.ForeignId
		and RLC_ID = @RLCID and Is_Active = @True

		insert into #assignments
		select NULL, '', p.PersonId, '', BreakoutGroupName, @True
		from F1..ActivityAssignment aa
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



-- get hard-coded rlcs to insert
select '(' + ltrim(str(RLC_ID, 25, 0)) + ', ' + '''' + GroupType + ''', ''' + GroupName + '''),'
from #importedMap
order by grouptype, groupname

-- double check all grouptypes/groups exist
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



select * from #rlcMap



 ====================================================== */

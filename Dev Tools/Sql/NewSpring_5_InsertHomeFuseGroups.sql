/* ====================================================== 
-- NewSpring Script #5: 
-- Imports home and fuse groups from F1.
  
--  Assumptions:
--  No GroupMemberStatus in F1 so everyone is a Member
--  Groups must be marked as 'Fuse' or 'Home'

   ====================================================== */
-- Make sure you're using the right Rock database:

USE Rock

/* ====================================================== */

-- Enable production mode for performance
SET NOCOUNT ON

-- Set the F1 database name
DECLARE @F1 nvarchar(255) = 'F1'

/* ====================================================== */
-- Start value lookups
/* ====================================================== */
DECLARE @IsSystem int = 0, @Order int = 0,  @TextFieldTypeId int = 1, @True int = 1, @False int = 0, @Output nvarchar(255)

DECLARE @SmallGroupTypeId int, @SmallGroupMemberId int, @FuseGroupTypeId int, @FuseGroupMemberId int, @FuseGroupId int, @HomeGroupId int

SELECT @SmallGroupTypeId = Id, 
	@SmallGroupMemberId = DefaultGroupRoleId
FROM [GroupType]
WHERE [Name] = 'Small Group'


SELECT @FuseGroupTypeId = Id, 
	@FuseGroupMemberId = DefaultGroupRoleId
FROM [GroupType]
WHERE [Name] = 'Fuse Group'

IF @FuseGroupTypeId is null
BEGIN
	INSERT [GroupType] ( [IsSystem], [Name], [Description], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [TakesAttendance], 
		[AttendanceRule], [AttendancePrintTo], [Order], [InheritedGroupTypeId], [LocationSelectionMode], [AllowedScheduleTypes], [SendAttendanceReminder], [Guid] ) 
	VALUES ( @IsSystem, 'Fuse Group', 'Grouptype for Fuse groups.', 'Group', 'Member', @True, @True, @True, @True, 1, 0, 0, 15, 0, 0, 0, NEWID() );

	SET @FuseGroupTypeId = SCOPE_IDENTITY()

	INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [CanView], [CanEdit], [Guid] )
	VALUES ( @IsSystem, @FuseGroupTypeId, 'Member', 0, @False, @False, @False, NEWID() )

	SET @FuseGroupMemberId = SCOPE_IDENTITY()

	UPDATE [GroupType]
	SET DefaultGroupRoleId = @FuseGroupMemberId
	WHERE [Id] = @FuseGroupTypeId
end

select @FuseGroupId = ID
from [Group] 
where Name = 'Fuse Groups'
and GroupTypeId = @FuseGroupTypeId

if @FuseGroupId is null
BEGIN
	insert [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
	select @False, NULL, @FuseGroupTypeId, NULL, 'Fuse Groups', 'Parent group for Fuse Groups', @False, @True, @Order, @True, NEWID()

	select @FuseGroupId = SCOPE_IDENTITY()
end

select @HomeGroupId = ID
from [Group] 
where Name = 'Home Groups'
and GroupTypeId = @SmallGroupTypeId

if @HomeGroupId is null
begin
	insert [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
	select @False, NULL, @SmallGroupTypeId, NULL, 'Home Groups', 'Parent group for Home Groups', @False, @True, @Order, @True, NEWID()

	select @HomeGroupId = SCOPE_IDENTITY()
end

/* ====================================================== */
-- Create group lookup
/* ====================================================== */
if object_id('tempdb..#groupAssignments') is not null
begin
	drop table #groupAssignments
end
create table #groupAssignments (
	ID int IDENTITY(1,1) NOT NULL,
	groupName nvarchar(255),
	groupId bigint,	
	groupType nvarchar(255),
	created datetime
)

declare @scopeIndex int, @numItems int
select @scopeIndex = min(ID) from Campus
select @numItems = count(1) + @scopeIndex from Campus

while @scopeIndex < @numItems
begin
	
	declare @CampusId int, @CampusName nvarchar(255), @GroupTypeId int, @GroupTypeName nvarchar(255), @CampusFuseGroupId int, @GroupRoleId int,
		 @CampusHomeGroupId int, @F1GroupId int, @ParentGroupId int, @ChildGroupId int, @GroupName nvarchar(255), @IndividualId int, @CreatedDate datetime

	select @CampusId = ID, @CampusName = Name
	from Campus where ID = @scopeIndex

	-- Create campus fuse group hierarchy
	select @CampusFuseGroupId = Id from [Group]
	where Name = @CampusName
	and CampusId = @CampusId
	and ParentGroupId = @FuseGroupId
	and GroupTypeId = @FuseGroupTypeId
	
	if @CampusFuseGroupId is null
	begin
		insert [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
		select @False, @FuseGroupId, @FuseGroupTypeId, @CampusId, @CampusName, @CampusName + ' Fuse Groups', @False, @True, @Order, @True, NEWID()

		select @CampusFuseGroupId = SCOPE_IDENTITY()
	end

	-- Create campus home group hierarchy
	select @CampusHomeGroupId = Id from [Group]
	where Name = @CampusName
	and CampusId = @CampusId
	and ParentGroupId = @HomeGroupId
	and GroupTypeId = @SmallGroupTypeId
	
	if @CampusHomeGroupId is null
	begin
		insert [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
		select @False, @HomeGroupid, @SmallGroupTypeId, @CampusId, @CampusName, @CampusName + ' Home Groups', @False, @True, @Order, @True, NEWID()

		select @CampusHomeGroupId = SCOPE_IDENTITY()
	end

	-- Filter groups by the current campus
	insert into #groupAssignments (groupName, groupId, groupType, created)
	select top 1000 LTRIM(RTRIM(Group_Name)), Group_ID, 
		LTRIM(RTRIM(SUBSTRING( Group_Type_Name, charindex(' ', Group_Type_Name) +1, 
			len(group_type_name) - charindex(' ', reverse(group_type_name))
		))) as groupType,
		max(Created_Date) as Created
	from F1..Groups
	where Group_Type_Name not like 'People List'
		and Group_Type_Name not like 'Inactive%'
		and Group_Name not like '%Wait%'
		and Group_Type_Name like ('' + @CampusName + '%')
	group by LTRIM(RTRIM(Group_Name)), Group_ID, 
		LTRIM(RTRIM(SUBSTRING( Group_Type_Name, charindex(' ', Group_Type_Name) +1, 
			len(group_type_name) - charindex(' ', reverse(group_type_name))
		)))
			
	/* ====================================================== */
	-- Start creating child groups
	/* ====================================================== */
	declare @childIndex int, @childItems int
	select @childIndex = min(ID) from #groupAssignments
	select @childItems = count(1) + @childIndex from #groupAssignments

	while @childIndex < @childItems
	begin
		
		select @GroupName = groupName, @F1GroupId = GroupId, @GroupTypeName = groupType, @CreatedDate = created
		from #groupAssignments ga
		where @childIndex = ga.ID

		-- Look up GroupType and Group
		if ( @GroupTypeName like '%Fuse%' )
		begin
			select @GroupTypeId = @FuseGroupTypeId
			select @ParentGroupId = @CampusFuseGroupId
			select @GroupRoleId = @FuseGroupMemberId
		end
		else if @GroupTypeName like '%Home%'
		begin
			select @GroupTypeId = @SmallGroupTypeId
			select @ParentGroupId = @CampusHomeGroupId
			select @GroupRoleId = @SmallGroupMemberId
		end

		select @ChildGroupId = Id from [Group]
		where Name = @GroupName 
		and CampusId = @CampusId
		and ParentGroupId = @ParentGroupId
		and GroupTypeId = @GroupTypeId

		select @Output = 'Starting ' + @CampusName + ' / ' + @GroupTypeName + ' / ' + @GroupName
		RAISERROR ( @Output, 0, 0 ) WITH NOWAIT

		-- Create group if it doesn't exist
		if @ChildGroupId is null
		begin

			insert [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], CreatedDateTime, IsPublic, ForeignId, [Guid])
			select @False, @ParentGroupId, @GroupTypeId, @CampusId, @GroupName, @CampusName + ' ' + @GroupName, @False, @True, @Order, @CreatedDate, @True, @F1GroupId, NEWID()

			select @ChildGroupId = SCOPE_IDENTITY()
		end
		
		-- Create memberships
		insert [GroupMember] (IsSyStem, GroupId, PersonId, GroupROleId, GroupMemberStatus, IsNotified, CreatedDateTime, [Guid])
		select @False, @ChildGroupId, p.PersonId, @GroupRoleId, @True, @False, g.Created_Date, NEWID()
		from F1..Groups g
		inner join PersonAlias p
		on g.Individual_ID = p.ForeignId
		and g.Group_ID = @F1GroupId

		select @GroupTypeId = null, @GroupTypeName = null, @F1GroupId = null, @ParentGroupId = null, @ChildGroupId = null, @GroupRoleId = null

		-- advance to next group
		select @childIndex = @childIndex + 1
	end

	select @CampusId = null, @CampusName = null, @CampusFuseGroupId = null, @CampusHomeGroupId = null

	delete from #groupAssignments
	select @scopeIndex = @scopeIndex + 1
end

-- completed successfully
RAISERROR ( N'Completed successfully.', 0, 0 ) WITH NOWAIT

use master


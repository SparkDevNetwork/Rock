/* ====================================================== 
-- NewSpring Script #5: 
-- Imports home and fuse groups from F1.
  
--  Assumptions:
--  Campuses must be setup
--  Groups must be marked as 'Fuse' or 'Home'
--  Fuse groups begin with a Grade designation
--  You may have to clear cache after running this is you are getting null reference exceptions (attributes are cached)

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

/* ====================================================== */

-- Enable production mode for performance
SET NOCOUNT ON

-- Set constant variables 
DECLARE @F1 nvarchar(255) = 'F1'
DECLARE @CheckInByAgeId int = 15
DECLARE @CheckInByGradeId int = 17
DECLARE @GroupEntityTypeId int = 16
DECLARE @BooleanFieldTypeId int = 3
DECLARE @DefinedTypeGradeId int = 51
DECLARE @DecimalRangeFieldTypeId int = 47
DECLARE @SingleSelectFieldTypeId int = 6
DECLARE @DefinedValueFieldTypeId int = 16
DECLARE @DefinedTypeMaritalStatusId int = 7
DECLARE @DefinedValueMeetingLocationId int = 209
DECLARE @GenderAttributeName nvarchar(255) = 'Gender'
DECLARE @MaritalAttributeName nvarchar(255) = 'Marital Status'
DECLARE @ChildCareAttributeName nvarchar(255) = 'Childcare'
DECLARE @SmallGroupTopicName nvarchar(255) = 'Small Group Topic'
DECLARE @CategoryTypeAttributeKey nvarchar(255) = 'Topic'

/* ====================================================== */
-- Start value lookups
/* ====================================================== */
DECLARE @IsSystem int = 0, @Order int = 0,  @TextFieldTypeId int = 1, @True int = 1, @False int = 0, @Output nvarchar(255)

DECLARE @SmallGroupTypeId int, @SmallGroupMemberId int, @SmallGroupId int, @SmallGroupLeaderId int, 
	@FuseGroupTypeId int, @FuseGroupMemberId int, @FuseGroupId int, @FuseGroupLeaderId int

-- use the Small Group grouptype that ships with core
SELECT @SmallGroupTypeId = Id, 
	@SmallGroupMemberId = DefaultGroupRoleId
FROM [GroupType]
WHERE [Name] = 'Small Group'

UPDATE [GroupType]
SET IconCssClass = 'fa fa-home' 
WHERE Id = @SmallGroupTypeId

-- fix small group attributes
DECLARE @coedGuid UNIQUEIDENTIFIER = '043CF720-FB99-49EA-890F-16486434D427';
DECLARE @ladiesGuid UNIQUEIDENTIFIER = '74269154-0885-4BE7-B367-FAB6F5668D30';
DECLARE @mensGuid UNIQUEIDENTIFIER = '6C31860D-2BF7-42E8-B09B-2A93F7E6E0B9';
DECLARE @marriedGuid UNIQUEIDENTIFIER = '15ACA3BA-EE45-4C42-B065-6E6BD5A054DD';

DECLARE @categoryTypeAttributeId int = (SELECT Id FROM Attribute WHERE [Key] = @CategoryTypeAttributeKey AND EntityTypeId = @GroupEntityTypeId AND EntityTypeQualifierValue = @SmallGroupTypeId)
DELETE FROM AttributeValue WHERE AttributeId = @categoryTypeAttributeId

DECLARE @smallGroupTopicId int = (SELECT Id FROM DefinedType WHERE Name = @SmallGroupTopicName)
DELETE FROM DefinedValue WHERE DefinedTypeId = @smallGroupTopicId

INSERT INTO DefinedValue (IsSystem, DefinedTypeId, [Order], [Value], [Description], [Guid], CreatedDateTime) VALUES 
	( @IsSystem, @smallGroupTopicId, @Order, 'Coed', 'Married or single men and women', @coedGuid, GETDATE() ),
	( @IsSystem, @smallGroupTopicId, @Order, 'Ladies', 'Married or single women', @ladiesGuid, GETDATE() ),
	( @IsSystem, @smallGroupTopicId, @Order, 'Mens', 'Married or single men', @mensGuid, GETDATE() ),
	( @IsSystem, @smallGroupTopicId, @Order, 'Married', 'Married couples', @marriedGuid, GETDATE() )

-- create a Leader role for Small Group 
SELECT @SmallGroupLeaderId = Id
FROM [GroupTypeRole]
WHERE [Name] = 'Leader'
AND GroupTypeId = @SmallGroupTypeId

IF @SmallGroupLeaderId IS NULL
BEGIN
	INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [CanView], [CanEdit], [Guid] )
	VALUES ( @IsSystem, @SmallGroupTypeId, 'Leader', 0, @False, @False, @False, NEWID() )

	SET @SmallGroupLeaderId = SCOPE_IDENTITY()
END

-- create the Home Groups parent group
SELECT @SmallGroupId = ID
FROM [Group] 
where Name = 'Home Groups'
and GroupTypeId = @SmallGroupTypeId

IF @SmallGroupId is null
BEGIN
	INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
	SELECT @False, NULL, @SmallGroupTypeId, NULL, 'Home Groups', 'Parent group for Home Groups', @False, @True, @Order, @True, NEWID()

	SELECT @SmallGroupId = SCOPE_IDENTITY()
END

-- update the Small Group grouptype to allow custom schedules
IF NOT EXISTS (SELECT ID FROM GroupType WHERE ID = @SmallGroupTypeId AND AllowedScheduleTypes = 3)
BEGIN
	UPDATE GroupType
	SET AllowedScheduleTypes = 3
	WHERE ID = @SmallGroupTypeId
END

-- update the Small Group grouptype to inherit from Check In By Age
IF NOT EXISTS (SELECT ID FROM GroupType WHERE ID = @SmallGroupTypeId AND InheritedGroupTypeId = @CheckInByAgeId)
BEGIN
	UPDATE GroupType
	SET InheritedGroupTypeId = @CheckInByAgeId
	WHERE ID = @SmallGroupTypeId
END

-- update the Small Group grouptype to allow subgroups
IF NOT EXISTS (SELECT GroupTypeId FROM GroupTypeAssociation WHERE GroupTypeId = @SmallGroupTypeId AND ChildGroupTypeId = @SmallGroupTypeId)
BEGIN
	INSERT GroupTypeAssociation
	SELECT @SmallGroupTypeId, @SmallGroupTypeId
END

-- create the Fuse Group grouptype
SELECT @FuseGroupTypeId = Id, 
	@FuseGroupMemberId = DefaultGroupRoleId
FROM [GroupType]
WHERE [Name] = 'Fuse Group'

IF @FuseGroupTypeId is null
BEGIN
	INSERT [GroupType] ( [IsSystem], [Name], [Description], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [IconCssClass], [TakesAttendance], 
		[AttendanceRule], [AttendancePrintTo], [Order], [InheritedGroupTypeId], [LocationSelectionMode], [AllowedScheduleTypes], [SendAttendanceReminder], [Guid] ) 
	VALUES ( @IsSystem, 'Fuse Group', 'Grouptype for Fuse groups.', 'Group', 'Member', @True, @True, @True, 'fa fa-fire', @True, 1, 0, 0, @CheckInByGradeId, 19, 3, 0, NEWID() )

	SET @FuseGroupTypeId = SCOPE_IDENTITY()

	-- allow child subgroups
	INSERT GroupTypeAssociation
	SELECT @FuseGroupTypeId, @FuseGroupTypeId

	INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [CanView], [CanEdit], [Guid] )
	VALUES ( @IsSystem, @FuseGroupTypeId, 'Leader', 0, @False, @False, @False, NEWID() )

	SET @FuseGroupLeaderId = SCOPE_IDENTITY()

	INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [CanView], [CanEdit], [Guid] )
	VALUES ( @IsSystem, @FuseGroupTypeId, 'Member', 0, @False, @False, @False, NEWID() )

	SET @FuseGroupMemberId = SCOPE_IDENTITY()

	UPDATE [GroupType]
	SET DefaultGroupRoleId = @FuseGroupMemberId
	WHERE [Id] = @FuseGroupTypeId
END

-- create a Leader role for the group
IF @FuseGroupLeaderId IS NULL
BEGIN

	SELECT @FuseGroupLeaderId = Id
	FROM [GroupTypeRole]
	WHERE [Name] = 'Leader'
	AND GroupTypeId = @FuseGroupTypeId
END


-- create the Fuse Group grouptype
SELECT @FuseGroupId = ID
FROM [Group] 
WHERE Name = 'Fuse Groups'
AND GroupTypeId = @FuseGroupTypeId

IF @FuseGroupId is null
BEGIN
	INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
	SELECT @False, NULL, @FuseGroupTypeId, NULL, 'Fuse Groups', 'Parent group for Fuse Groups', @False, @True, @Order, @True, NEWID()

	SELECT @FuseGroupId = SCOPE_IDENTITY()
END

-- set the Fuse Group grouptype to inherit from Check In By Grade
IF NOT EXISTS (SELECT ID FROM GroupType WHERE ID = @FuseGroupTypeId AND InheritedGroupTypeId = @CheckInByGradeId)
BEGIN
	UPDATE GroupType
	SET InheritedGroupTypeId = @CheckInByGradeId
	WHERE ID = @FuseGroupTypeId
END

-- start setting attributes on Home/Fuse Groups
DECLARE @SmallGroupGenderId int, @SmallGroupMaritalId int, @SmallGroupChildcareId int, @FuseGroupGenderId int, 
	@AgeRangeAttributeId int, @GradeRangeAttributeId int

-- Age Range Attribute already exists in core
SELECT @AgeRangeAttributeId = ID
FROM [Attribute]
WHERE EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @CheckInByAgeId
AND Name = 'Age Range'

-- Grade Range Attribute already exists in core
SELECT @GradeRangeAttributeId = ID
FROM [Attribute]
WHERE EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @CheckInByGradeId
AND Name = 'Grade Range'

-- create a Small Group gender attribute
SELECT @SmallGroupGenderId = Id
FROM [Attribute]
WHERE FieldTypeId = @SingleSelectFieldTypeId
AND EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @SmallGroupTypeId
AND Name = @GenderAttributeName

IF @SmallGroupGenderId IS NULL
BEGIN

	INSERT [Attribute] (IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], DefaultValue,
		[Order], [IsGridColumn], [IsMultiValue], [IsRequired], [AllowSearch], [Guid])
	SELECT @IsSystem, @SingleSelectFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @SmallGroupTypeId, REPLACE(@GenderAttributeName, ' ', ''), @GenderAttributeName, 'The gender of the group members', '',
		@Order, @True, @False, @False, @False, NEWID()

	SET @SmallGroupGenderId = SCOPE_IDENTITY()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupGenderId, 'fieldtype', 'ddl', NEWID()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupGenderId, 'values', 'Male, Female', NEWID()
END

-- create a marital status attribute
SELECT @SmallGroupMaritalId = Id
FROM [Attribute]
WHERE FieldTypeId = @DefinedValueFieldTypeId
AND EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @SmallGroupTypeId
AND Name = @MaritalAttributeName

IF @SmallGroupMaritalId IS NULL
BEGIN

	INSERT [Attribute] (IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], DefaultValue,
		[Order], [IsGridColumn], [IsMultiValue], [IsRequired], [AllowSearch], [Guid])
	SELECT @IsSystem, @DefinedValueFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @SmallGroupTypeId, REPLACE(@MaritalAttributeName, ' ', ''), @MaritalAttributeName, 'The marital status of the group members', '',
		@Order, @True, @False, @False, @False, NEWID()

	SET @SmallGroupMaritalId = SCOPE_IDENTITY()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupMaritalId, 'allowmultiple', 'False', NEWID()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupMaritalId, 'definedtype', @DefinedTypeMaritalStatusId, NEWID()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupMaritalId, 'displaydescription', 'False', NEWID()

END

-- create a childcare attribute
SELECT @SmallGroupChildcareId = Id
FROM [Attribute]
WHERE EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @SmallGroupTypeId
AND Name = @ChildCareAttributeName

IF @SmallGroupChildcareId IS NULL
BEGIN

	INSERT [Attribute] (IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], DefaultValue,
		[Order], [IsGridColumn], [IsMultiValue], [IsRequired], [AllowSearch], [Guid])
	SELECT @IsSystem, @BooleanFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @SmallGroupTypeId, REPLACE(@ChildCareAttributeName, ' ', ''), @ChildCareAttributeName, 'Does this group provide childcare?', 'False',
		@Order, @True, @False, @False, @False, NEWID()

	SET @SmallGroupChildcareId = SCOPE_IDENTITY()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupChildcareId, 'falsetext', 'No', NEWID()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupChildcareId, 'truetext', 'Yes', NEWID()

END

-- create a Fuse Group gender attribute
SELECT @FuseGroupGenderId = Id
FROM [Attribute]
WHERE FieldTypeId = @SingleSelectFieldTypeId
AND EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @FuseGroupTypeId
AND Name = @GenderAttributeName

IF @FuseGroupGenderId IS NULL
BEGIN

	INSERT [Attribute] (IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], DefaultValue,
		[Order], [IsGridColumn], [IsMultiValue], [IsRequired], [AllowSearch], [Guid])
	SELECT @IsSystem, @SingleSelectFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @FuseGroupTypeId, REPLACE(@GenderAttributeName, ' ', ''), @GenderAttributeName, 'The gender of the group members', '',
		@Order, @True, @False, @False, @False, NEWID()

	SET @FuseGroupGenderId = SCOPE_IDENTITY()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @FuseGroupGenderId, 'fieldtype', 'ddl', NEWID()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @FuseGroupGenderId, 'values', 'Male, Female', NEWID()
END

-- create a small Group gender attribute
SELECT @SmallGroupGenderId = Id
FROM [Attribute]
WHERE FieldTypeId = @SingleSelectFieldTypeId
AND EntityTypeId = @GroupEntityTypeId
AND EntityTypeQualifierValue = @SmallGroupTypeId
AND Name = @GenderAttributeName

IF @SmallGroupGenderId IS NULL
BEGIN

	INSERT [Attribute] (IsSystem, FieldTypeId, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, [Key], Name, [Description], DefaultValue,
		[Order], [IsGridColumn], [IsMultiValue], [IsRequired], [AllowSearch], [Guid])
	SELECT @IsSystem, @SingleSelectFieldTypeId, @GroupEntityTypeId, 'GroupTypeId', @SmallGroupTypeId, REPLACE(@GenderAttributeName, ' ', ''), @GenderAttributeName, 'The gender of the group members', '',
		@Order, @True, @False, @False, @False, NEWID()

	SET @SmallGroupGenderId = SCOPE_IDENTITY()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupGenderId, 'fieldtype', 'ddl', NEWID()

	INSERT [AttributeQualifier] (IsSystem, AttributeId, [Key], Value, [Guid]) 
	SELECT @False, @SmallGroupGenderId, 'values', 'Male, Female', NEWID()
END

/* ====================================================== */
-- Create group lookup
/* ====================================================== */
IF OBJECT_ID('tempdb..#groupAssignments') is not null
BEGIN
	DROP TABLE #groupAssignments
END
CREATE TABLE #groupAssignments (
	ID int IDENTITY(1,1) NOT NULL,
	groupId bigint,
	groupName nvarchar(255),
	groupDescription nvarchar(max),
	groupType nvarchar(255),
	groupVisibility bit,
	groupGender nvarchar(255),
	groupMarital nvarchar(255),
	groupStart datetime,
	groupAgeMin nvarchar(255),
	groupAgeMax nvarchar(255),
	groupRecurrence nvarchar(255),
	groupDay nvarchar(255),
	groupTime nvarchar(255),
	groupAddress1 nvarchar(255),
	groupAddress2 nvarchar(255),
	groupCity nvarchar(255),
	groupState nvarchar(255),
	groupZip nvarchar(255),
	groupCountry nvarchar(255),	
	groupChildren bit
)

DECLARE @scopeIndex int, @numItems int
SELECT @scopeIndex = min(ID) FROM Campus
SELECT @numItems = count(1) + @scopeIndex FROM Campus

WHILE @scopeIndex < @numItems
BEGIN

	DECLARE @CampusId int, @CampusName nvarchar(255), @GroupTypeId int, @CampusFuseGroupId int, @LeaderRoleId int, @MemberRoleId int,
		 @CampusHomeGroupId int, @ParentGroupId int, @ChildGroupId int, @GroupScheduleId int, @GroupLocationId int

	SELECT @CampusId = ID, @CampusName = Name
	FROM Campus where ID = @scopeIndex

	-- Create campus fuse group hierarchy
	SELECT @CampusFuseGroupId = Id FROM [Group]
	WHERE Name = @CampusName
	AND CampusId = @CampusId
	AND ParentGroupId = @FuseGroupId
	AND GroupTypeId = @FuseGroupTypeId

	IF @CampusFuseGroupId is null
	BEGIN
		INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
		SELECT @False, @FuseGroupId, @FuseGroupTypeId, @CampusId, @CampusName, @CampusName + ' Fuse Groups', @False, @True, @Order, @True, NEWID()

		SELECT @CampusFuseGroupId = SCOPE_IDENTITY()
	END

	-- Create campus home group hierarchy
	SELECT @CampusHomeGroupId = Id FROM [Group]
	WHERE Name = @CampusName
	AND CampusId = @CampusId
	AND ParentGroupId = @SmallGroupid
	AND GroupTypeId = @SmallGroupTypeId

	IF @CampusHomeGroupId is null
	BEGIN
		INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], IsPublic, [Guid])
		SELECT @False, @SmallGroupId, @SmallGroupTypeId, @CampusId, @CampusName, @CampusName + ' Home Groups', @False, @True, @Order, @True, NEWID()

		SELECT @CampusHomeGroupId = SCOPE_IDENTITY()
	END

	-- Filter groups by the current campus
	INSERT INTO #groupAssignments (groupId, groupName, groupDescription, groupType, groupVisibility, groupStart, groupGender, groupMarital, groupAgeMin, groupAgeMax, groupRecurrence, groupDay, groupTime, 
		groupAddress1, groupAddress2, groupCity, groupState, groupZip, groupCountry, groupChildren )
	SELECT Group_ID, LTRIM(RTRIM(Group_Name)), LTRIM(RTRIM(Description)), LTRIM(RTRIM(SUBSTRING( Group_Type_Name, CHARINDEX(' ', Group_Type_Name) +1, 
			LEN(Group_Type_Name) - CHARINDEX(' ', REVERSE(Group_Type_Name))
		))) as groupType, isSearchable, start_date, gender_name, marital_status_name, start_age_range, end_age_range, RecurrenceType, ScheduleDay, StartHour, 
		address1, address2, city, StProvince, PostalCode, country, HasChildcare
	FROM F1..GroupsDescription
	WHERE Group_Type_Name NOT LIKE 'People List'
		AND Group_Type_Name NOT LIKE 'Inactive%'
		AND Group_Name NOT LIKE '%Wait%'
		AND Group_Type_Name LIKE ('' + @CampusName + '%')

	-- remove duplicate groups that don't have a schedule
	IF EXISTS ( SELECT groupName FROM #groupAssignments GROUP BY groupName, groupType HAVING COUNT(1) > 1)
	BEGIN
		IF OBJECT_ID('tempdb..#groupsToDelete') is not null
		BEGIN
			DROP TABLE #groupsToDelete
		END

		SELECT groupName, groupType
		INTO #groupsToDelete
		FROM #groupAssignments
		GROUP BY groupName, groupType
		HAVING COUNT(1) > 1;		

		DELETE ga
		FROM #groupAssignments ga
		INNER JOIN #groupsToDelete gtd
		ON ga.groupName = gtd.groupName
		AND ga.groupType = gtd.groupType
		WHERE ga.groupRecurrence IS NULL
	END


	/* ====================================================== */
	-- Start creating child groups
	/* ====================================================== */
	DECLARE @F1GroupId int, @GroupName nvarchar(255), @Description nvarchar(max), @GroupTypeName nvarchar(255), 
		@IsPublic bit, @ScheduleStart datetime, @Gender nvarchar(255), @MaritalStatus nvarchar(255), @MinGrade nvarchar(255), @MaxGrade nvarchar(255),
		@MinAge nvarchar(255), @MaxAge nvarchar(255), @ScheduleRecurrence nvarchar(255), @ScheduleDay nvarchar(255),
		@ScheduleTime nvarchar(255), @LocationStreet nvarchar(255), @LocationStreet2 nvarchar(255), @LocationCity nvarchar(255),
		@LocationState nvarchar(255), @LocationZip nvarchar(255), @LocationCountry nvarchar(255), @HasChildcare bit

	DECLARE @childIndex int, @childItems int
	SELECT @childIndex = min(ID) FROM #groupAssignments
	SELECT @childItems = count(1) + @childIndex FROM #groupAssignments

	WHILE @childIndex < @childItems
	BEGIN

		SELECT @F1GroupId = GroupId, @GroupName = groupName, @Description = groupDescription, @GroupTypeName = groupType, @IsPublic = groupVisibility,
			@Gender = groupGender, @MaritalStatus = groupMarital, @MinAge = groupAgeMin, @MaxAge = groupAgeMax, @ScheduleRecurrence = groupRecurrence,
			@ScheduleStart = groupStart, @ScheduleDay = groupDay, @ScheduleTime = groupTime, @LocationStreet = groupAddress1, @LocationStreet2 = groupAddress2, 
			@LocationCity = groupCity, @LocationState = groupState, @LocationZip = groupZip, @LocationCountry = groupCountry, @HasChildcare = groupChildren
		FROM #groupAssignments ga
		where @childIndex = ga.ID

		-- Look up GroupType and Group
		IF ( @GroupTypeName like '%Fuse%' )
		BEGIN
			SELECT @GroupTypeId = @FuseGroupTypeId
			SELECT @ParentGroupId = @CampusFuseGroupId
			SELECT @MemberRoleId = @FuseGroupMemberId
			SELECT @LeaderRoleId = @FuseGroupLeaderId
		END
		ELSE IF @GroupTypeName like '%Home%'
		BEGIN
			SELECT @GroupTypeId = @SmallGroupTypeId
			SELECT @ParentGroupId = @CampusHomeGroupId
			SELECT @MemberRoleId = @SmallGroupMemberId
			SELECT @LeaderRoleId = @SmallGroupLeaderId
		END

		SELECT @ChildGroupId = Id FROM [Group]
		WHERE Name = @GroupName 
		AND CampusId = @CampusId
		AND ParentGroupId = @ParentGroupId
		AND GroupTypeId = @GroupTypeId

		-- Create group (+ attributes + location + schedule) if it doesn't exist
		IF @ChildGroupId IS NULL AND @GroupTypeId IS NOT NULL
		BEGIN

			SELECT @Output = 'Starting ' + @CampusName + ' / ' + @GroupTypeName + ' / ' + @GroupName
			RAISERROR ( @Output, 0, 0 ) WITH NOWAIT

			INSERT [Group] (IsSystem, ParentGroupId, GroupTypeId, CampusId, Name, [Description], IsSecurityRole, IsActive, [Order], CreatedDateTime, IsPublic, ForeignKey, ForeignId, [Guid])
			SELECT @False, @ParentGroupId, @GroupTypeId, @CampusId, @GroupName, @Description, @False, @True, @Order, @ScheduleStart, @IsPublic, @F1GroupId, @F1GroupId, NEWID()

			SELECT @ChildGroupId = SCOPE_IDENTITY()

			-- set age range attribute
			INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
			SELECT @IsSystem, @AgeRangeAttributeId, @ChildGroupId, @MinAge + ',' + @MaxAge, NEWID()

			-- Fuse Group attributes only
			IF @GroupTypeId = @FuseGroupTypeId
			BEGIN
				-- set gender attribute
				INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
				SELECT @IsSystem, @FuseGroupGenderId, @ChildGroupId, @Gender, NEWID()

				-- parse the grade range
				IF ISNUMERIC(LEFT(@GroupName, 1)) = 1
				BEGIN

					SELECT @MinGrade = [Guid]
					FROM DefinedValue
					WHERE DefinedTypeId = @DefinedTypeGradeId
					AND [Order] = REPLACE(LEFT(REPLACE(SUBSTRING(REPLACE(@GroupName, '-', 'th'), 0, PATINDEX('%[0-9]th%', @GroupName)+1), 't', ''), 2), 'h', '')

					SELECT @MaxGrade = [Guid]
					FROM DefinedValue
					WHERE DefinedTypeId = @DefinedTypeGradeId
					AND [Order] = REPLACE(REPLACE(REVERSE(SUBSTRING(REVERSE(REPLACE(@GroupName, '-', 'th') ), PATINDEX('%ht[0-9]%', REVERSE( REPLACE(@GroupName, '-', 'th'))) +2, 2)), ',', ''), 'h', '')

					-- set grade range attribute
					INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
					SELECT @IsSystem, @GradeRangeAttributeId, @ChildGroupId, @MinGrade + ',' + @MaxGrade, NEWID()
				END 
			END

			-- Small Group attributes only
			IF @GroupTypeId = @SmallGroupTypeId
			BEGIN
				-- set gender attribute
				INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
				SELECT @IsSystem, @SmallGroupGenderId, @ChildGroupId, @Gender, NEWID()

				-- set marital status
				INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
				SELECT @IsSystem, @SmallGroupMaritalId, @ChildGroupId, (SELECT TOP 1 [Guid] FROM DefinedValue WHERE DefinedTypeId = @DefinedTypeMaritalStatusId AND Value = @MaritalStatus), NEWID()

				-- childcare
				INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
				SELECT @IsSystem, @SmallGroupChildcareId, @ChildGroupId, CASE WHEN @HasChildcare = 1 THEN 'True' ELSE 'False' END, NEWID()

				-- category / type
				INSERT [AttributeValue] (IsSystem, AttributeId, EntityId, Value, [Guid])
				SELECT @IsSystem, @categoryTypeAttributeId, @ChildGroupId, CASE 
					WHEN @Gender = 'Coed' AND @MaritalStatus = 'Married or Single' THEN CONVERT(NVARCHAR(MAX), @coedGuid)
					WHEN @Gender = 'Female' THEN CONVERT(NVARCHAR(255), @ladiesGuid)
					WHEN @Gender = 'Male' THEN CONVERT(NVARCHAR(255), @mensGuid)
					WHEN @MaritalStatus = 'Married' THEN CONVERT(NVARCHAR(255), @marriedGuid)
					ELSE CONVERT(NVARCHAR(255), '')
				END, NEWID()
			END

			-- create group schedule
			IF @ScheduleRecurrence IS NOT NULL
			BEGIN
				INSERT [Schedule] ([Name], [Description], iCalendarContent, [Guid], CreatedDateTime, WeeklyTimeOfDay, WeeklyDayOfWeek)
				SELECT @GroupName + ' Schedule', @ScheduleDay + ' @ ' + @ScheduleTime, '', NEWID(), @ScheduleStart, CONVERT(time, @ScheduleTime), CASE @ScheduleDay
					WHEN 'Sunday'   THEN 0
					WHEN 'Monday'	THEN 1  
					WHEN 'Tuesday'	THEN 2  
					WHEN 'Wednesday'THEN 3  
					WHEN 'Thursday'	THEN 4  
					WHEN 'Friday'	THEN 5  
					WHEN 'Saturday'	THEN 6  
				END

				SELECT @GroupScheduleId = SCOPE_IDENTITY()

				UPDATE [Group] 
				SET ScheduleId = @GroupScheduleId
				WHERE Id = @ChildGroupId
			END

			SELECT @GroupLocationId = Id
			FROM Location 
			WHERE IsActive = @True
				AND Street1 = @LocationStreet
				AND Street2 = @LocationStreet2
				AND City = @LocationCity
				AND [State] = @LocationState
				AND Country = @LocationCountry
				AND PostalCode = @LocationZip

			-- create group location if an address exists
			IF @GroupLocationId IS NULL AND @LocationStreet IS NOT NULL
			BEGIN
				INSERT [Location] (IsActive, Street1, Street2, City, [State], Country, PostalCode, [Guid])
				SELECT @True, @LocationStreet, @LocationStreet2, @LocationCity, @LocationState, @LocationCountry, @LocationZip, NEWID()

				SELECT @GroupLocationId = SCOPE_IDENTITY()
			END

			IF @GroupLocationId IS NOT NULL
			BEGIN
				-- assign group to location
				INSERT [GroupLocation] (GroupId, LocationId, GroupLocationTypeValueId, IsMailingLocation, IsMappedLocation, [Guid])
				SELECT @ChildGroupId, @GroupLocationId, @DefinedValueMeetingLocationId, @False, @False, NEWID()

				SELECT @GroupLocationId = SCOPE_IDENTITY()
			END
		END
		ELSE BEGIN
			SELECT @Output = 'Could not create ' + ISNULL(@CampusName,'') + ' / ' + ISNULL(@GroupTypeName,'') + ' / ' + ISNULL(@GroupName,'')
			RAISERROR ( @Output, 0, 0 ) WITH NOWAIT
		END

		IF @ChildGroupId IS NOT NULL
		BEGIN
			-- Create group memberships
			INSERT [GroupMember] (IsSystem, GroupId, PersonId, GroupMemberStatus, IsNotified, CreatedDateTime, [Guid], GroupRoleId)
			SELECT @False, @ChildGroupId, p.PersonId, @True, @False, g.Created_Date, NEWID(),
				 CASE Group_member_type_name WHEN 'Leader' THEN @LeaderRoleId ELSE @MemberRoleId END 
			FROM F1..Groups g
			INNER JOIN PersonAlias p
				ON g.Individual_ID = p.ForeignKey
				AND g.Group_ID = @F1GroupId
		END

		-- reset all variables
		SELECT @GroupTypeId = null, @GroupTypeName = null, @F1GroupId = null, @ParentGroupId = null, @ChildGroupId = null, @GroupScheduleId = null,
			@GroupLocationId = null, @LeaderRoleId = null, @MemberRoleId = null, @GroupName = null, @Description = null, @GroupTypeName = null, 
			@IsPublic = null, @MinGrade = null, @MaxGrade = null, @Gender = null, @MaritalStatus = null, @MinAge = null, @MaxAge = null, 
			@ScheduleRecurrence = null, @ScheduleDay = null, @ScheduleTime = null, @LocationStreet = null, @LocationStreet2 = null,
			@LocationCity = null, @LocationState = null, @LocationZip = null, @LocationCountry = null, @HasChildcare = null

		-- advance to next group
		SELECT @childIndex = @childIndex + 1
	END

	SELECT @CampusId = null, @CampusName = null, @CampusFuseGroupId = null, @CampusHomeGroupId = null

	DELETE FROM #groupAssignments
	SELECT @scopeIndex = @scopeIndex + 1
END

-- completed successfully
RAISERROR ( N'Completed successfully.', 0, 0 ) WITH NOWAIT

--use master



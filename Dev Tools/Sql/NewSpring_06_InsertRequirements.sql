/* ====================================================== 
-- NewSpring Script #6: 
-- Imports Requirements from F1.
  
--  Assumptions:
--  We only import the following requirement names:

	Background Check
	Band Audition
	Band Next Steps Conversation
	Campus Safety Field Training
	Campus Safety Next Steps Conversation
	Care Interview
	Child Protection Policy
	Confidentiality Agreement Signed
	Driver Agreement
	Financial Coaching Confidentiality Agree
	Financial Coaching Interview
	Financial Coaching Training
	Fuse GL Interview
	Fuse GL Training
	Fuse NS Conversation
	Fuse Training
	Group Leader Interview
	Intellectual Property Agreement
	KidSpring Incident Report
	KidSpring NS Conversation
	License
	Next Steps NS Conversation
	Ownership Paperwork
	Video Release Form
	Worship Interview

   ====================================================== */
-- Make sure you're using the right Rock database:

USE spark

/* ====================================================== */

-- Enable production mode for performance
SET NOCOUNT ON

-- Set the F1 database name
DECLARE @F1 nvarchar(255) = 'F1'

/* ====================================================== */
-- Start value lookups
/* ====================================================== */
declare @IsSystem int = 0, @Order int = 0,  @TextFieldTypeId int = 1, @True int = 1, @False int = 0,
	@PersonEntityTypeId int = 15, @AttributeEntityTypeId int = 49, @BooleanFieldTypeId int = 3,
	@DDLFieldTypeId int = 6, @DateFieldTypeId int = 11, @DocumentFieldTypeId int = 32, @VideoFieldTypeId int = 80,
	@BackgroundCategoryId int, @FuseCategoryId int, @CSCategoryId int, @CreativeCategoryId int, @FCCategoryId int,
	@ProductionCategoryId int, @GSCategoryId int, @CareCategoryId int, @KidSpringCategoryId int, @NSCategoryId int,
	@MembershipCategoryId int

/* ====================================================== */
-- Get or create the attribute categories
/* ====================================================== */

-- Background Checks
select @BackgroundCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId 
and name = 'Background Check Information'

if @BackgroundCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Background Check Information', 'Information related to safety and security of organization', 
		@Order, 'fa fa-check-square-o', NEWID()

	select @BackgroundCategoryId = SCOPE_IDENTITY()
end

-- Campus Safety
select @CSCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Campus Safety New Serve'

if @CSCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Campus Safety New Serve', 'Information related to Campus Safety New Serve', 
		@Order, 'fa fa-cab', NEWID()

	select @CSCategoryId = SCOPE_IDENTITY()
end

-- Care
select @CareCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Care New Serve'

if @CareCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Care New Serve', 'Information related to Care New Serve', 
		@Order, 'fa fa-heartbeat', NEWID()

	select @CareCategoryId = SCOPE_IDENTITY()
end

-- Creative
select @CreativeCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Creative New Serve'

if @CreativeCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Creative New Serve', 'Information related to Creative New Serve', 
		@Order, 'fa fa-paint-brush', NEWID()

	select @CreativeCategoryId = SCOPE_IDENTITY()
end

-- Financial Coaching
select @FCCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Financial Coaching New Serve'

if @FCCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Financial Coaching New Serve', 'Information related to Financial Coaching New Serve', 
		@Order, 'fa fa-money', NEWID()

	select @FCCategoryId = SCOPE_IDENTITY()
end

-- Fuse
select @FuseCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Fuse New Serve'

if @FuseCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Fuse New Serve', 'Information related to Fuse New Serve', 
		@Order, 'fa fa-bomb', NEWID()

	select @FuseCategoryId = SCOPE_IDENTITY()
end

-- KidSpring
select @KidSpringCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'KidSpring New Serve'

if @KidSpringCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'KidSpring New Serve', 'Information related to KidSpring New Serve', 
		@Order, 'fa fa-child', NEWID()

	select @KidSpringCategoryId = SCOPE_IDENTITY()
end

-- Guest Services
select @GSCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Guest Services New Serve'

if @GSCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Guest Services', 'Information related to Guest Services New Serve', 
		@Order, 'fa fa-graduation-cap', NEWID()

	select @GSCategoryId = SCOPE_IDENTITY()
end

-- Next Steps
select @MembershipCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and (name = 'Next Steps'
or name = 'Membership')

if @MembershipCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Next Steps', 'Information related to Next Steps', 
		@Order, 'fa fa-graduation-cap', NEWID()

	select @MembershipCategoryId = SCOPE_IDENTITY()
end

-- Next Steps New Serve
select @NSCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId
and name = 'Next Steps New Serve'

if @NSCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Next Steps New Serve', 'Information related to Next Steps New Serve', 
		@Order, 'fa fa-level-up', NEWID()

	select @NSCategoryId = SCOPE_IDENTITY()
end

-- Production/Worship
select @ProductionCategoryId = [Id] from Category
where EntityTypeId = @AttributeEntityTypeId 
and name = 'Service Production New Serve'

if @ProductionCategoryId is null
begin
	insert Category ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, [Description], [Order], [IconCssClass], [Guid] )
	select @IsSystem, @AttributeEntityTypeId, 'EntityTypeId', @PersonEntityTypeId, 'Service Production New Serve', 'Information related to Production New Serve', 
		@Order, 'fa fa-microphone', NEWID()

	select @ProductionCategoryId = SCOPE_IDENTITY()
end

/* ====================================================== */
-- Create requirement types
/* ====================================================== */
if object_id('tempdb..#requirements') is not null
begin
	drop table #requirements
end
create table #requirements (
	ID int IDENTITY(1,1),
	requirementType nvarchar(255),
	attributeName nvarchar(255),
	attributeDescription nvarchar(255),
	categoryId int
)

insert #requirements (requirementType, attributeName, attributeDescription, categoryId)
values 
('Background Check', 'Background Check', 'Background Check Information', @BackgroundCategoryId ),
('Band Audition', 'Band Audition', 'Requirement for Worship Volunteers/Contractors', @ProductionCategoryId ),
('Band Next Steps Conversation', 'SP Next Steps Convo', 'Requirement for Worship Volunteers/Contractors', @ProductionCategoryId ),
('Campus Safety Field Training', 'Field Training', 'Requirement for Campus Safety Volunteers', @CSCategoryId ),
('Campus Safety Next Steps Conversation', 'CS Next Steps Convo', 'Requirement for Campus Safety Volunteers', @CSCategoryId ),
('Care Interview', 'Care Next Steps Convo', 'Requirement for Care Volunteers', @CareCategoryId ),
('Child Protection Policy', 'Child Protection Policy', 'Requirement for Care Volunteers', @CareCategoryId  ),
('Confidentiality Agreement Signed', 'Care Confidentiality Agreement', 'Requirement for Care Volunteers', @CareCategoryId  ),
('Driver Agreement', 'Driver Agreement', 'Occasional requirement for multiple ministries. One agreement applies to all ministries.', NULL ),
('Financial Coaching Confidentiality Agree', 'FC Confidentiality Agreement', 'Requirement for Financial Coaches.', @FCCategoryId ),
('Financial Coaching Interview', 'FC Next Steps Convo', 'Requirement for Financial Coaches.', @FCCategoryId ),
('Financial Coaching Training', 'FC Training', 'Requirement for Financial Coaches.', @FCCategoryId ),
('Fuse GL Interview', 'Group Leader Interview', 'Occasional requirement for Fuse Volunteers.', @FuseCategoryId ),
('Fuse GL Training', 'Group Leader Training', 'Occasional requirement for Fuse Volunteers.', @FuseCategoryId ),
('Fuse NS Conversation', 'Fuse Next Steps Convo', 'Requirement for Fuse Volunteers.', @FuseCategoryId ),
('Fuse Training', 'Basic Training', 'Requirement for Fuse Volunteers', @FuseCategoryId ),
('Group Leader Interview', 'Group Leader Interview', 'Occasional requirement for Next Steps Volunteers.', @NSCategoryId ),
('Intellectual Property Agreement', 'IP Agreement', 'Occasional requirement for Creative and Service Production volunteers', @CreativeCategoryId ),
('KidSpring Incident Report', 'Incident Report', 'Requirement for KidSpring Volunteers',  @KidSpringCategoryId),
('KidSpring NS Conversation', 'KS Next Steps Convo', 'Requirement for KidSpring Volunteers', @KidSpringCategoryId),
('License', 'Medic License', 'Occasional requirement for Campus Safety Volunteers', @CSCategoryId),
('Next Steps NS Conversation', 'NS Next Steps Convo', 'Occasional requirement for Next Steps Volunteers', @NSCategoryId ),
('Ownership Paperwork', 'Ownership Paperwork', 'Requirement for the person becoming an Owner', @MembershipCategoryId),
('Video Release Form', 'Image/Video Release', 'Requirement for Worship Volunteers/Contractors', @ProductionCategoryId ),
('Worship Interview', 'Worship Interview', 'Requirement for Worship Volunteers/Contractors', @ProductionCategoryId )

/* ====================================================== */
-- Create attribute lookup
/* ====================================================== */
if object_id('tempdb..#attributeAssignment') is not null
begin
	drop table #attributeAssignment
end
create table #attributeAssignment (	
	personid int,
	attributeId int,
	value nvarchar(255),
	filterDate datetime
)

declare @scopeIndex int, @numItems int
select @scopeIndex = min(ID) from #requirements
select @numItems = count(1) + @scopeIndex from #requirements

while @scopeIndex < @numItems
begin
	declare @msg nvarchar(255), @AssignmentType nvarchar(255), @AttributeName nvarchar(255), @AttributeDescription nvarchar(255), 
		@AttributeCategoryId int, @DocumentAttributeId int, @DateAttributeId int

	select @AssignmentType = requirementType, @AttributeName = attributeName, 
		@AttributeDescription = attributeDescription, @AttributeCategoryId = categoryId
	from #requirements
	where ID = @scopeIndex

	if @AssignmentType is not null
	begin

		select @msg = 'Starting ' + @AssignmentType + ' / '+ @AttributeName
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
		
		-- depending on what assignment this is, take different actions
		if @AssignmentType = 'Background Check'
		begin

			declare @CheckedAttributeId int, @ResultAttributeId int
			
			select @AttributeName = 'Background Checked'

			-- Background Checked
			select @CheckedAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @CheckedAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @BooleanFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					'Does person have a valid background check on record', '', @Order, @False, @False, @False, NEWID()

				select @CheckedAttributeId = SCOPE_IDENTITY()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@CheckedAttributeId, @AttributeCategoryId)
			end

			select @AttributeName = 'Background Check Date'

			-- Background Check Date
			select @DateAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @DateAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DateFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					'Date person last passed/failed a background check', '', @Order, @False, @False, @False, NEWID()

				select @DateAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'displayDiff', 'False', NEWID()

				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'format', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@DateAttributeId, @AttributeCategoryId)
			end
			
			select @AttributeName = 'Background Check Result'

			-- Background Check Result
			select @ResultAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @ResultAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DDLFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					'Result of last background check', '', @Order, @False, @False, @False, NEWID()

				select @ResultAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @ResultAttributeId, 'fieldtype', 'ddl', NEWID()

				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @ResultAttributeId, 'values', 'Pass,Fail', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@ResultAttributeId, @AttributeCategoryId)
			end

			select @AttributeName = 'Background Check Document'

			-- Background Check Document
			select @DocumentAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @DocumentAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DocumentFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					'The last background check', '', @Order, @False, @False, @False, NEWID()

				select @DocumentAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DocumentAttributeId, 'binaryFileType', '5c701472-8a6b-4bbe-aec6-ec833c859f2d', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@DocumentAttributeId, @AttributeCategoryId)
			end
			
			-- Start inserting attribute assignments
			insert #attributeAssignment
			select pa.PersonId, @CheckedAttributeId, 'True', convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
				and r.Requirement_Status_Name like '%Approved%'
				and datediff(year, r.requirement_date, getdate()) < 3

			insert #attributeAssignment
			select pa.PersonId, @DateAttributeId, left(r.requirement_date, 19), convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
				and r.Requirement_Status_Name like '%Approved%'
				and datediff(year, r.requirement_date, getdate()) < 3

			insert #attributeAssignment
			select pa.PersonId, @ResultAttributeId, 
				CASE r.Requirement_Status_Name 
					WHEN 'Approved' THEN 'Pass'
					WHEN 'Not Approved' THEN 'Fail'
					ELSE '' 
				END as value, convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
				and r.Requirement_Status_Name like '%Approved%'
				and datediff(year, r.requirement_date, getdate()) < 3

			insert #attributeAssignment
			select pa.PersonId, @DocumentAttributeId, '', convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
				and r.Requirement_Status_Name like '%Approved%'
				and datediff(year, r.requirement_date, getdate()) < 3			
		end
		-- Ownership Date/Paperwork
		else if @AssignmentType = 'Ownership Paperwork'
		begin

			select @AttributeName = 'Ownership Paperwork'

			select @DocumentAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @DocumentAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DocumentFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					@AttributeDescription, '', @Order, @False, @False, @False, NEWID()

				select @DocumentAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DocumentAttributeId, 'binaryFileType', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@DocumentAttributeId, @AttributeCategoryId)
			end

			select @AttributeName = 'Ownership Date'
			
			select @DateAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @DateAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DateFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					'The date the person became an Owner.', '', @Order, @False, @False, @False, NEWID()

				select @DateAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'displayDiff', 'False', NEWID()

				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'format', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@DateAttributeId, @AttributeCategoryId)
			end

			insert #attributeAssignment
			select pa.PersonId, @DocumentAttributeId, '', convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType

			insert #attributeAssignment
			select pa.PersonId, @DateAttributeId, left(r.requirement_date, 19), convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
		end
		-- Driver Agreement
		else if @AssignmentType = 'Driver Agreement'
		begin

			select @DocumentAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @DocumentAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DocumentFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					@AttributeDescription, '', @Order, @False, @False, @False, NEWID()

				select @DocumentAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DocumentAttributeId, 'binaryFileType', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values 
					(@DocumentAttributeId, @CreativeCategoryId),
					(@DocumentAttributeId, @CSCategoryId),
					(@DocumentAttributeId, @FuseCategoryId),
					(@DocumentAttributeId, @GSCategoryId),
					(@DocumentAttributeId, @ProductionCategoryId)
			end

			select @DateAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName + ' Date'
			
			if @DateAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DateFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', '') + 'Date', @AttributeName + ' Date', 
					@AttributeDescription, '', @Order, @False, @False, @False, NEWID()

				select @DateAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'displayDiff', 'False', NEWID()

				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'format', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values 
					(@DateAttributeId, @CreativeCategoryId),
					(@DateAttributeId, @CSCategoryId),
					(@DateAttributeId, @FuseCategoryId),
					(@DateAttributeId, @GSCategoryId),
					(@DateAttributeId, @ProductionCategoryId)
			end


			insert #attributeAssignment
			select pa.PersonId, @DocumentAttributeId, '', convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType

			insert #attributeAssignment
			select pa.PersonId, @DateAttributeId, left(r.requirement_date, 19), convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
		end
		-- Everything else
		else begin
			
			select @DocumentAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName
			
			if @DocumentAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DocumentFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', ''), @AttributeName, 
					@AttributeDescription, '', @Order, @False, @False, @False, NEWID()

				select @DocumentAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DocumentAttributeId, 'binaryFileType', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@DocumentAttributeId, @AttributeCategoryId)
			end

			select @DateAttributeId = [Id] from Attribute
			where EntityTypeId = @PersonEntityTypeId
			and name = @AttributeName + ' Date'
			
			if @DateAttributeId is null
			begin
				insert Attribute ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], 
					[Key], [Name], [Description], [DefaultValue], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
				select @IsSystem, @DateFieldTypeId, @PersonEntityTypeId, '', '', REPLACE(@AttributeName, ' ', '') + 'Date', @AttributeName + ' Date', 
					@AttributeDescription, '', @Order, @False, @False, @False, NEWID()

				select @DateAttributeId = SCOPE_IDENTITY()

				-- set additional attribute fields
				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'displayDiff', 'False', NEWID()

				insert AttributeQualifier (IsSystem, AttributeId, [Key], Value, [Guid])
				select @IsSystem, @DateAttributeId, 'format', '', NEWID()

				insert AttributeCategory (AttributeId, CategoryId)
				values (@DateAttributeId, @AttributeCategoryId)
			end

			insert #attributeAssignment
			select pa.PersonId, @DocumentAttributeId, '', convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType

			insert #attributeAssignment
			select pa.PersonId, @DateAttributeId, left(r.requirement_date, 19), convert(datetime, left(r.requirement_date, 19), 120)
			from F1..Requirement r
			inner join PersonAlias pa
				on r.Individual_ID = pa.ForeignId
			where r.Requirement_Name = @AssignmentType
		end
	end
	
	-- reset variables
	select @AssignmentType = null, @DocumentAttributeId = null, 
		@DateAttributeId = null, @AttributeName = null, 
		@AttributeDescription = null, @AttributeCategoryId = null

	select @scopeIndex = @scopeIndex + 1
	
end
-- end while requirements

-- remove duplicate attributes and values
;WITH duplicates (personId, attributeId, id) 
AS (
    SELECT personId, attributeId, ROW_NUMBER() OVER (
		PARTITION BY personId, attributeId
		ORDER BY filterDate desc
    ) AS id
    FROM #attributeAssignment
)
delete from duplicates
where id > 1

-- remove the existing value for this person/attribute
delete av
from AttributeValue av
inner join #attributeAssignment a
on a.personId = av.EntityId
and a.attributeId = av.AttributeId

-- insert attribute values
insert AttributeValue ( [IsSystem], [AttributeId], [EntityId], [Value], [CreatedDateTime], [ModifiedDateTime], [Guid] )
select @False, attributeId, personId, value, filterDate, NULL, NEWID()
from #attributeAssignment


-- completed successfully
RAISERROR ( N'Completed successfully.', 0, 0 ) WITH NOWAIT

use master


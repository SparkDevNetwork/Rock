-- Create the Presence user and grant access to the PresenceController.
DECLARE @personGuid UNIQUEIDENTIFIER = '86CF11D9-66BC-4CE0-9037-F8AFCBCD608A'
-- Will not run if Presence user already exists
IF NOT EXISTS (SELECT [Id] FROM [Person] WHERE [Guid] = @personGuid)
BEGIN
	DECLARE @personAliasGuid UNIQUEIDENTIFIER = '86CF11D9-66BC-4CE0-9037-F8AFCBCD608A'
	DECLARE @groupMemberGuid UNIQUEIDENTIFIER = 'C213A210-1792-4869-AB9E-5549CA801629'
	DECLARE @userLoginGuid UNIQUEIDENTIFIER = '0CEB5F7F-AD1D-493E-8E7F-B6C12DCB5646'
	DECLARE @personId INT

	-- Create a unique family
	INSERT INTO [Group] ([IsSystem], [GroupTypeId], [CampusId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid], [IsPublic], [IsArchived])
	VALUES (
		  0 --[IsSystem]
		, 10 --[GroupTypeId]
		, (SELECT [Id] FROM [Campus] WHERE [Guid] = '76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8') --[CampusId]
		, 'Presence Family' --[Name]
		, 0 --[IsSecurityRole]
		, 1 --[IsActive]
		, 0 --[Order]
		, '6DBD34C3-A8CE-412B-85A3-BA741644DC0A' --[Guid]
		, 1 --[IsPublic]
		, 0 --[IsArchived]
		)

	-- Create Person
	INSERT INTO [Person] (
		  [IsSystem]
		, [RecordTypeValueId]
		, [RecordStatusValueId]
		, [ConnectionStatusValueId]
		, [IsDeceased]
		, [FirstName]
		, [NickName]
		, [LastName]
		, [Gender]
		, [IsEmailActive]
		, [EmailPreference]
		, [Guid]
		, [CreatedDateTime]
		)
	VALUES (
		  1 --[IsSystem]
		, (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'E2261A84-831D-4234-9BE0-4D628BBE751E') --[RecordTypeValueId]
		, (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E') --[RecordStatusValueId]
		, (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'B91BA046-BC1E-400C-B85D-638C1F4E0CE2') --[ConnectionStatusValueId]
		, 0 --[IsDeceased]
		, 'Presence' --[FirstName]
		, 'Presence' --[NickName]
		, 'Presence' --[LastName]
		, 0 --[Gender]
		, 1 --[IsEmailActive]
		, 0 --[EmailPreference]
		, @personGuid --[Guid]
		, SYSDATETIME() --[CreatedDateTime]
		)

	SET @personId = SCOPE_IDENTITY()

	-- Create Person Alias
	INSERT INTO [PersonAlias] (
		  [PersonId]
		, [AliasPersonId]
		, [AliasPersonGuid]
		, [Guid]
		)
	VALUES (
		  @personId --[PersonId]
		, @personId --[AliasPersonId]
		, @personGuid --[AliasPersonGuid]
		, @personAliasGuid --[Guid]
		)

	-- Create GroupMember
	INSERT INTO [GroupMember] (
		  [IsSystem]
		, [GroupId]
		, [PersonId]
		, [GroupRoleId]
		, [GroupMemberStatus]
		, [Guid]
		)
	VALUES (
		  0 --[IsSystem]
		, (SELECT [Id] FROM [Group] WHERE [Guid] = '6DBD34C3-A8CE-412B-85A3-BA741644DC0A') --[GroupId]
		, @personId --[PersonId]
		, (SELECT [Id] FROM [GroupTypeRole] WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42') --[GroupRoleId]
		, 0 --[GroupMemberStatus]
		, @groupMemberGuid --[Guid]
		)

	-- Create UserLogin
	INSERT INTO [UserLogin] (
		  [UserName]
		, [Password]
		, [IsConfirmed]
		, [LastPasswordChangedDateTime]
		, [ApiKey]
		, [PersonId]
		, [Guid]
		, [EntityTypeId]
		, [CreatedDateTime]
		, [ModifiedDateTime]
	)
	VALUES (
		  'presence' --[UserName]
		, '$2a$11$bRBPqjG.Lzalcf6EN8msrebJEmzshVZdUIBgiVvqHgUzwRZDhfd46' --[Password]
		, 1 --[IsConfirmed]
		, SYSDATETIME() --[LastPasswordChangedDateTime]
		, REPLACE(NEWID(), '-', '') --[ApiKey]
		, @personId --[PersonId]
		, @userLoginGuid --[Guid]
		, (SELECT [Id] FROM [EntityType] WHERE [Guid] = '4E9B798F-BB68-4C0E-9707-0928D15AB020') --[EntityTypeId]
		, SYSDATETIME() --[CreatedDateTime]
		, SYSDATETIME() --[ModifiedDateTime]
	)

	-- Create the PresenceController
	IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PresenceController') 
	BEGIN
		INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
		VALUES ( 'Presence', 'Rock.Rest.Controllers.PresenceController', NEWID() )
	END

	-- Assign user view and edit permissions to the PresenceController
	INSERT INTO [Auth] ( 
		  [EntityTypeId]
		, [EntityId]
		, [Order]
		, [Action]
		, [AllowOrDeny]
		, [SpecialRole]
		, [Guid] 
		, [PersonAliasId]
	) 
	VALUES (
		  (SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D') --[EntityTypeId]
		, (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PresenceController') --[EntityId]
		, 0 --[Order]
		, 'View' --[Action]
		, 'A' --[AllowOrDeny]
		, 0
		, '7F060287-44E7-4772-BBFB-C1E1634BB400' --[Guid]
		, (SELECT ID FROM [PersonAlias] WHERE [Guid] = @personAliasGuid) --[PersonAliasId]
	)

	INSERT INTO [Auth] ( 
		[EntityTypeId]
		, [EntityId]
		, [Order]
		, [Action]
		, [AllowOrDeny]
		, [SpecialRole]
		, [Guid]
		, [PersonAliasId]
	) 
	VALUES (
		  (SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D') --[EntityTypeId]
		, (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PresenceController') --[EntityId]
		, 0 --[Order]
		, 'Edit' --[Action]
		, 'A' --[AllowOrDeny]
		, 0 --[SpecialRole]
		,'4C1A6DD6-5104-439C-8F3C-5140ABD1D5DD' --[Guid]
		, (SELECT ID FROM [PersonAlias] WHERE [Guid] = @personAliasGuid) --[PersonAliasId]
	)

END
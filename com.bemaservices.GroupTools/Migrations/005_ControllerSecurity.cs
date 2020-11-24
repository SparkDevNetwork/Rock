using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 5, "1.9.4" )]
    public class ControllerSecurity : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"-- Create the Group Finder user and grant access to the GroupToolsController.
DECLARE @personGuid UNIQUEIDENTIFIER = '830581CE-9F59-482C-9C16-C4188CD0A70F'
-- Will not run if Group Finder user already exists
IF NOT EXISTS (SELECT [Id] FROM [Person] WHERE [Guid] = @personGuid)
BEGIN
	DECLARE @personAliasGuid UNIQUEIDENTIFIER = 'AD1016D2-397D-481C-B07C-A196A2AB040D'
	DECLARE @groupMemberGuid UNIQUEIDENTIFIER = 'CF017A02-A7F1-4375-B6D9-44397A922F53'
	DECLARE @userLoginGuid UNIQUEIDENTIFIER = 'DAC8013C-2B53-4BCE-9AB9-64B0FCAD9535'
	DECLARE @personId INT

	-- Create a unique family
	INSERT INTO [Group] ([IsSystem], [GroupTypeId], [CampusId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid], [IsPublic], [IsArchived])
	VALUES (
		  0 --[IsSystem]
		, 10 --[GroupTypeId]
		, (SELECT [Id] FROM [Campus] WHERE [Guid] = '76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8') --[CampusId]
		, 'Group Finder Family' --[Name]
		, 0 --[IsSecurityRole]
		, 1 --[IsActive]
		, 0 --[Order]
		, '95688EE0-8E7B-4A8A-A644-5E7B8844D29C' --[Guid]
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
		, '' --[FirstName]
		, '' --[NickName]
		, 'Group Finder' --[LastName]
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
		, (SELECT [Id] FROM [Group] WHERE [Guid] = '95688EE0-8E7B-4A8A-A644-5E7B8844D29C') --[GroupId]
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
		  '13799fbd-b357-44ce-abdf-87c8e06b9a85' --[UserName]
		, NULL --[Password]
		, 1 --[IsConfirmed]
		, SYSDATETIME() --[LastPasswordChangedDateTime]
		, '6FuURHoPOhKk04168thMRtAl' --[ApiKey]
		, @personId --[PersonId]
		, @userLoginGuid --[Guid]
		, (SELECT [Id] FROM [EntityType] WHERE [Guid] = '4E9B798F-BB68-4C0E-9707-0928D15AB020') --[EntityTypeId]
		, SYSDATETIME() --[CreatedDateTime]
		, SYSDATETIME() --[ModifiedDateTime]
	)

	-- Create the GroupToolsController
	IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'com.bemaservices.GroupTools.Controllers.GroupToolsController') 
	BEGIN
		INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
		VALUES ( 'GroupTools', 'com.bemaservices.GroupTools.Controllers.GroupToolsController', NEWID() )
	END

	-- Assign user view and edit permissions to the Group FinderController
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
		, (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'com.bemaservices.GroupTools.Controllers.GroupToolsController') --[EntityId]
		, 0 --[Order]
		, 'View' --[Action]
		, 'A' --[AllowOrDeny]
		, 0
		, '73B115EC-0D39-417A-9113-40EAFBE09E9F' --[Guid]
		, (SELECT ID FROM [PersonAlias] WHERE [Guid] = @personAliasGuid) --[PersonAliasId]
	)

END" );
            }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}


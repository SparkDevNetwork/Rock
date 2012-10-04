namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameFamilyMemberBlock : RockMigration_0
    {
        public override void Up()
        {
			Sql( @"
	-- Change family member block path to more generic group members
	UPDATE [cmsBlock] SET [Path] = '~/Blocks/Crm/PersonDetail/GroupMembers.ascx' WHERE [Guid] = '3E14B410-22CB-49CC-8A1F-C30ECD0E816A'
	
	-- Add Family Group Type
	DECLARE @GroupTypeId int
	INSERT INTO [groupsGroupType] ([IsSystem],[Name],[Description],[DefaultGroupRoleId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
		VALUES(1,'Family','Family Members',NULL,GETDATE(),GETDATE(),1,1,'790E3215-3B10-442B-AF69-616C0DCB998E')
	SET @GroupTypeId = SCOPE_IDENTITY()

	-- Add Adult Role
	DECLARE @GroupRoleId int
	INSERT INTO [groupsGroupRole] ([IsSystem],[Name],[Description],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
		VALUES(1,'Adult','Adult Family Member',0,GETDATE(),GETDATE(),1,1,'2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')
	SET @GroupRoleId = SCOPE_IDENTITY()
	INSERT INTO [groupsGroupTypeRole] ([GroupTypeId],[GroupRoleId])
		VALUES(@GroupTypeId, @GroupRoleId)

	-- Add Child Role
	INSERT INTO [groupsGroupRole] ([IsSystem],[Name],[Description],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
		VALUES(1,'Child','Child Family Member',1,GETDATE(),GETDATE(),1,1,'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')
	SET @GroupRoleId = SCOPE_IDENTITY()
	INSERT INTO [groupsGroupTypeRole] ([GroupTypeId],[GroupRoleId])
		VALUES(@GroupTypeId, @GroupRoleId)
" );
			// Add the default group type attribute and set it to 'Family' for the group members block on person detail page
			AddBlockAttribute( "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Group Type", "Behavior", "The type of group to display.  Any group of this type that person belongs to will be displayed", 0, "0", "B84EB1CB-E719-4444-B739-B0112AA20BBA" );
			Sql( @"
	DECLARE @GroupTypeId int
	SET @GroupTypeId = (SELECT [Id] FROM [groupsGroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E')
	DECLARE @AttributeId int
	SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = 'B84EB1CB-E719-4444-B739-B0112AA20BBA')
	DECLARE @BlockInstanceId int
	SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '44BDB19E-9967-45B9-A272-81F9C12FFE20')
	INSERT INTO [coreAttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
		VALUES(1,@AttributeId,@BlockInstanceId,0,CAST(@GroupTypeId as varchar),GETDATE(),GETDATE(),1,1,NEWID())
" );
		}
        
        public override void Down()
        {
			DeleteBlockAttribute( "B84EB1CB-E719-4444-B739-B0112AA20BBA" );

			Sql( @"
	-- Delete any family type groups
	DECLARE @GroupTypeId int
	SET @GroupTypeId = (SELECT [Id] FROM [groupsGroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E')
	DELETE [groupsGroup] WHERE [GroupTypeId] = @GroupTypeId

	-- Delete the family type roles
	DELETE [groupsGroupRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
	DELETE [groupsGroupRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'

	-- Delete the family group type
	DELETE [groupsGroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'

	-- Revert the block path back to the original family member block
	UPDATE [cmsBlock] SET [Path] = '~/Blocks/Crm/PersonDetail/Family.ascx' WHERE [Guid] = '3E14B410-22CB-49CC-8A1F-C30ECD0E816A'
" );
		}
    }
}

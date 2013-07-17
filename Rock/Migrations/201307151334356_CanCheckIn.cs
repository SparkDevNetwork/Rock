//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CanCheckIn : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateEntityType( "Rock.Model.GroupRole", "D155C373-9E47-4C6A-BADD-792F31AF5FBA" );

            Sql( @"
    DECLARE @KnownRelationshipGroupType int
    DECLARE @AllowRoleId int
    DECLARE @CanRoleId int

    SET @KnownRelationshipGroupType = (SELECT [Id] FROM [GroupType] WHERE [Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF')

    -- Allow  by
    SET @AllowRoleId = (
	    SELECT [ID] 
	    FROM [GroupRole] 
	    WHERE [GroupTypeId] = @KnownRelationshipGroupType
	    AND ( [Name] = 'Allow Check-in By' OR [Name] = 'Allow check in by' OR [Guid] = 'FF9869F1-BC56-4410-8A12-CAFC32C62257')
    )

    IF @AllowRoleId IS NOT NULL
    BEGIN
	    UPDATE [GroupRole] SET 
		     [IsSystem] = 1
		    ,[Name] = 'Allow check in by'
		    ,[Description] = 'A person that can check in the owner of this known relationship group'
		    ,[Guid] = 'FF9869F1-BC56-4410-8A12-CAFC32C62257'
	    WHERE [Id] = @AllowRoleId
    END
    ELSE
    BEGIN
	    INSERT INTO [GroupRole] (
		     [IsSystem]
		    ,[GroupTypeId]
		    ,[Name]
		    ,[Description]
		    ,[SortOrder]
		    ,[Guid]
		    ,[IsLeader] )
	    VALUES (
		    1
		    ,@KnownRelationshipGroupType
		    ,'Allow check in by'
		    ,'A person that can check in the owner of this known relationship group'
		    ,0
		    ,'FF9869F1-BC56-4410-8A12-CAFC32C62257'
		    ,0 )
	    SET @AllowRoleId = SCOPE_IDENTITY()
    END

    -- Can Check-in
    SET @CanRoleId = (
	    SELECT [ID] 
	    FROM [GroupRole] 
	    WHERE [GroupTypeId] = @KnownRelationshipGroupType
	    AND [Name] = 'Can check in')

    IF @CanRoleId IS NOT NULL
    BEGIN
	    UPDATE [GroupRole] SET 
		     [IsSystem] = 1
		    ,[Name] = 'Can check in'
		    ,[Description] = 'A person that can be checked in by the owner of this known relationship group'
		    ,[Guid] = 'DC8E5C35-F37C-4B49-A5C6-DF3D94FC808F'
	    WHERE [Id] = @CanRoleId
    END
    ELSE
    BEGIN
	    INSERT INTO [GroupRole] (
		     [IsSystem]
		    ,[GroupTypeId]
		    ,[Name]
		    ,[Description]
		    ,[SortOrder]
		    ,[Guid]
		    ,[IsLeader] )
	    VALUES (
		    1
		    ,@KnownRelationshipGroupType
		    ,'Can check in'
		    ,'A person that can be checked in by the owner of this known relationship group'
		    ,0
		    ,'DC8E5C35-F37C-4B49-A5C6-DF3D94FC808F'
		    ,0 )
	    SET @CanRoleId = SCOPE_IDENTITY()
    END

    -- Group Role Field Type
    DECLARE @GroupRoleFieldTypeId int
    SET @GroupRoleFieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '3BB25568-E793-4D12-AE80-AC3FDA6FD8A8')

    DECLARE @BoolFieldType int
    SET @BoolFieldType = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A')

    -- Group Role Entity Type
    DECLARE @EntityTypeId int
    SET @EntityTypeId = ( SELECT [Id] FROM [EntityType]	WHERE [Name] = 'Rock.Model.GroupRole' )

    -- Can check in attribute
    DECLARE @CanCheckInAttributeId int
    SET @CanCheckInAttributeId = (
	    SELECT [Id]
	    FROM [Attribute]
	    WHERE [EntityTypeId] = @EntityTypeId
	    AND [Key] = 'CanCheckin'
    )
    IF @CanCheckInAttributeId IS NULL
    BEGIN
	    INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[IsMultiValue],[IsRequired],[Guid])
		    VALUES (1, @BoolFieldType, @EntityTypeId, 'GroupTypeId', CAST(@KnownRelationshipGroupType AS varchar), 'CanCheckin', 'Can Checkin', 'Should this type of known-relationship allow person to check in the related person?', 0, 1, 0, 0, '610A5BE8-8FDE-46AA-8F9D-1AF7F1F23441')
	    SET @CanCheckInAttributeId = SCOPE_IDENTITY()
    END
    ELSE
    BEGIN
	    UPDATE [Attribute] SET
		     [IsSystem] = 1
		    ,[Guid] = '610A5BE8-8FDE-46AA-8F9D-1AF7F1F23441'
	    WHERE [Id] = @CanCheckInAttributeId
    END

    -- Inverse Relationship attribute
    DECLARE @InverseAttributeId int
    SET @InverseAttributeId = (
	    SELECT [Id]
	    FROM [Attribute]
	    WHERE [EntityTypeId] = @EntityTypeId
	    AND [Key] = 'InverseRelationship'
    )
    IF @InverseAttributeId IS NULL
    BEGIN
	    INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[IsMultiValue],[IsRequired],[Guid])
		    VALUES (1, @GroupRoleFieldTypeId, @EntityTypeId, 'GroupTypeId', CAST(@KnownRelationshipGroupType AS varchar), 'InverseRelationship', 'Inverse Relationship', 'The opposite relationship (role) that should be added to the related person whenever this relationship type is added', 1, 1, 0, 0, 'C91148D9-D663-493A-86E8-5000BD281852')
	    SET @InverseAttributeId = SCOPE_IDENTITY()
    END
    ELSE
    BEGIN
	    UPDATE [Attribute] SET
		     [IsSystem] = 1
		    ,[Guid] = 'C91148D9-D663-493A-86E8-5000BD281852'
	    WHERE [Id] = @InverseAttributeId
    END

    -- Update 'Can check in' role to allow checkin
    IF NOT EXISTS(
	    SELECT [Id] 
	    FROM [AttributeValue]
	    WHERE [AttributeId] = @CanCheckInAttributeId
	    AND [EntityId] = @CanRoleId
    )
    BEGIN
	    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
		    VALUES (1, @CanCheckInAttributeId, @CanRoleId, 0, 'True', NEWID())
    END


    -- Update inverse role values
    DECLARE @RoleGuid uniqueidentifier
    IF NOT EXISTS(
	    SELECT [Id] 
	    FROM [AttributeValue]
	    WHERE [AttributeId] = @InverseAttributeId
	    AND [EntityId] = @CanRoleId
    )
    BEGIN
	    SET @RoleGuid = (SELECT [Guid] FROM [GroupRole] WHERE [ID] = @AllowRoleId)
	    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
		    VALUES (1, @InverseAttributeId, @CanRoleId, 0, CAST(@RoleGuid as varchar(100)), NEWID())
    END
    IF NOT EXISTS(
	    SELECT [Id] 
	    FROM [AttributeValue]
	    WHERE [AttributeId] = @InverseAttributeId
	    AND [EntityId] = @AllowRoleId
    )
    BEGIN
	    SET @RoleGuid = (SELECT [Guid] FROM [GroupRole] WHERE [ID] = @CanRoleId)
	    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
		    VALUES (1, @InverseAttributeId, @AllowRoleId, 0, CAST(@RoleGuid as varchar(100)), NEWID())
    END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

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
    public partial class FixAbilityLevelAttribute : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

            -- Add 'Childhood Information' category
            DECLARE @AttributeEntityTypeId int
            SELECT @AttributeEntityTypeId = [Id] FROM [EntityType] WHERE [Guid] = '5997C8D3-8840-4591-99A5-552919F90CBD'

            DECLARE @PersonEntityTypeId int
            SELECT @PersonEntityTypeId = [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

            DECLARE @CategoryId int
            INSERT INTO [Category] ([IsSystem], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconSmallFileId], [IconLargeFileId], [IconCssClass], [Guid])
                VALUES (1, NULL, @AttributeEntityTypeId, N'EntityTypeId', @PersonEntityTypeId, N'Childhood Information', NULL, NULL, NULL, '752dc692-836e-4a3e-b670-4325cd7724bf')
            SET @CategoryId = SCOPE_IDENTITY()

            -- Update AbilityLevel attribute
            DECLARE @DefinedValueFieldTypeId INT
            SELECT @DefinedValueFieldTypeId = [Id] FROM [FieldType] WHERE [Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7'

            DECLARE @AbilityLevelGuid uniqueidentifier
            SET @AbilityLevelGuid = '4abf0bf2-49ba-4363-9d85-ac48a0f7e92a'

            DECLARE @AttributeId int;
            SELECT @AttributeId = [Id] FROM [Attribute] WHERE [Guid] = @AbilityLevelGuid;

            -- Change the attribute to be a DefinedValue
            UPDATE [Attribute] SET [FieldTypeId] = @DefinedValueFieldTypeId, [Description]=N'The ability level of the child (used with children''s check-in).', [DefaultValue]=N'' WHERE [Guid]=@AbilityLevelGuid

            -- Add the attribute to the new category
            INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId]) VALUES (@AttributeId, @CategoryId)

            DECLARE @AbilityLevelDefinedTypeId INT
            SELECT @AbilityLevelDefinedTypeId = [Id] FROM [DefinedType] WHERE [Guid] = '7BEEF4D4-0860-4913-9A3D-857634D1BF7C'

            INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid]) VALUES (1, @AttributeId, N'definedtype', @AbilityLevelDefinedTypeId, '29dde675-6983-49a4-a8b2-0e775e476b20')
            INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid]) VALUES (1, @AttributeId, N'allowmultiple', N'False', '0c322a3e-8bd3-4a53-b257-b11a64c20b74')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
            DECLARE @DefinedTypeFieldTypeId INT
            SELECT @DefinedTypeFieldTypeId = [Id] FROM [FieldType] WHERE [Guid] = 'BC48720C-3610-4BCF-AE66-D255A17F1CDF'

            -- Remove the attribute qualifiers
            IF EXISTS ( SELECT Id FROM [AttributeQualifier] WHERE [Guid] = '29dde675-6983-49a4-a8b2-0e775e476b20' )
            BEGIN
                DELETE [AttributeQualifier] WHERE [Guid] = '29dde675-6983-49a4-a8b2-0e775e476b20'
            END

            IF EXISTS ( SELECT Id FROM [AttributeQualifier] WHERE [Guid] = '0c322a3e-8bd3-4a53-b257-b11a64c20b74' )
            BEGIN
                DELETE [AttributeQualifier] WHERE [Guid] = '0c322a3e-8bd3-4a53-b257-b11a64c20b74'
            END

            -- Change the AbilityLevel attribute back to a DefinedType with a value of the Ability Level DefinedTypeId (which is all incorrect)
            DECLARE @AbilityLevelGuid uniqueidentifier
            SET @AbilityLevelGuid = '4abf0bf2-49ba-4363-9d85-ac48a0f7e92a'
            DECLARE @AbilityLevelDefinedTypeId INT
            SELECT @AbilityLevelDefinedTypeId = [Id] FROM [DefinedType] WHERE [Guid] = '7BEEF4D4-0860-4913-9A3D-857634D1BF7C'

            DECLARE @AttributeId int;
            SELECT @AttributeId = [Id] FROM [Attribute] WHERE [Guid] = @AbilityLevelGuid;

            UPDATE [Attribute] SET [FieldTypeId]=@DefinedTypeFieldTypeId, [DefaultValue]=@AbilityLevelDefinedTypeId WHERE [Guid]=@AbilityLevelGuid

            -- the 'Childhood Information' category
            DECLARE @CategoryId int
            SELECT @CategoryId = [Id] FROM [Category] WHERE [Guid] = '752dc692-836e-4a3e-b670-4325cd7724bf'

            -- Remove the attribute from the category:
            DELETE [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId

            -- remove 'Childhood Information' category
            IF EXISTS ( SELECT Id FROM [Category] WHERE [Guid] = '752dc692-836e-4a3e-b670-4325cd7724bf' )
            BEGIN
                DELETE [Category] WHERE [Guid] = '752dc692-836e-4a3e-b670-4325cd7724bf'
            END
            
" );
        }
    }
}

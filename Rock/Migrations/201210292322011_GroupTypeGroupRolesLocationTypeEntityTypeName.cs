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
    public partial class GroupTypeGroupRolesLocationTypeEntityTypeName : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // GroupTypeAssociationColumnFix
            RenameColumn( table: "dbo.crmGroupTypeAssociation", name: "ChildGroupTypeId", newName: "GroupTypeId" );
            RenameColumn( table: "dbo.crmGroupTypeAssociation", name: "ParentGroupTypeId", newName: "ChildGroupTypeId" );
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_ChildGroupTypeId', 'IX_GroupTypeId', 'INDEX'" );
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_ParentGroupTypeId', 'IX_ChildGroupTypeId', 'INDEX'" );
            Sql( "sp_rename '[FK_dbo.crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId]', 'FK_dbo.crmGroupTypeAssociation_crmGroupType_GroupTypeId'" );
            Sql( "sp_rename '[FK_dbo.crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId]' , 'FK_dbo.crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId'" );

            // LocationTypes
            AddDefinedType( "Location", "Location Type", "Location Types", "2e68d37c-fb7b-4aa5-9e09-3785d52156cb" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Home", "Home", "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Office", "Office", "E071472A-F805-4FC4-917A-D5E3C095C35C" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Business", "Business", "C89D123C-8645-4B96-8C71-6C87B5A96525" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Sports Field", "Sports Field", "F560DC25-E964-46C4-8CEF-0E67BB922163" );

            // GroupRolesSortOrder
            DropIndex( "dbo.crmGroupRole", new string[] { "Order" } );
            RenameColumn( "dbo.crmGroupRole", "Order", "SortOrder" );
            CreateIndex( "dbo.crmGroupRole", new string[] { "SortOrder" } );

            // GroupRolesUI
            AddBlockType( "GroupRoles", "Allows for the configuration fof Group Roles", "~/Blocks/Crm/GroupRoles.ascx", "89315EBC-D4BD-41E6-B1F1-929D19E66608" );
            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Group Roles", "Manage Group Roles", "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9" );
            AddBlock( "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9", "89315EBC-D4BD-41E6-B1F1-929D19E66608", "Group Roles", "Content", "1064932B-F0DB-4F39-B438-24703A14198B" );

            // GroupRolesRequiredFieldFix
            AlterColumn( "dbo.crmGroupRole", "Name", c => c.String( nullable: false, maxLength: 100 ) );
            AlterColumn( "dbo.crmGroupRole", "Description", c => c.String() );

            // EntityTypeName
            CreateTable(
    "dbo.coreEntityType",
    c => new
    {
        Id = c.Int( nullable: false, identity: true ),
        Name = c.String( maxLength: 100 ),
        FriendlyName = c.String( maxLength: 100 ),
        Guid = c.Guid( nullable: false ),
    } )
    .PrimaryKey( t => t.Id );

            CreateIndex( "dbo.coreEntityType", "Name", true );
            CreateIndex( "dbo.coreEntityType", "Guid", true );

            Sql( "UPDATE [CmsAuth] SET [EntityType] = 'Rock.' + [EntityType] WHERE [EntityType] <> 'Global'" );
            Sql( "UPDATE [CoreAudit] SET [EntityType] = 'Rock.' + [EntityType]" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // EntityTypeName
            Sql( "UPDATE [CmsAuth] SET [EntityType] = REPLACE([EntityType], 'Rock.', '')" );
            Sql( "UPDATE [coreAudit] SET [EntityType] = REPLACE([EntityType], 'Rock.', '')" );

            DropTable( "dbo.coreEntityType" );
            
            // GroupRolesRequiredFieldFix
            AlterColumn( "dbo.crmGroupRole", "Description", c => c.String( nullable: false ) );
            AlterColumn( "dbo.crmGroupRole", "Name", c => c.String( maxLength: 100 ) );

            // GroupRolesUI
            DeleteBlock( "1064932B-F0DB-4F39-B438-24703A14198B" );
            DeletePage( "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9" );
            DeleteBlockType( "89315EBC-D4BD-41E6-B1F1-929D19E66608" );

            // GroupRolesSortOrder
            DropIndex( "dbo.crmGroupRole", new string[] { "SortOrder" } );
            RenameColumn( "dbo.crmGroupRole", "SortOrder", "Order" );
            CreateIndex( "dbo.crmGroupRole", new string[] { "Order" } );

            // LocationTypes
            DeleteDefinedValue( "F560DC25-E964-46C4-8CEF-0E67BB922163" );
            DeleteDefinedValue( "C89D123C-8645-4B96-8C71-6C87B5A96525" );
            DeleteDefinedValue( "E071472A-F805-4FC4-917A-D5E3C095C35C" );
            DeleteDefinedValue( "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );
            DeleteDefinedType( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb" );

            // GroupTypeAssociationColumnFix
            Sql( "sp_rename '[FK_dbo.crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId]' , 'FK_dbo.crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId'" );
            Sql( "sp_rename '[FK_dbo.crmGroupTypeAssociation_crmGroupType_GroupTypeId]', 'FK_dbo.crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId'" );
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_ChildGroupTypeId', 'IX_ParentGroupTypeId', 'INDEX'" );
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_GroupTypeId', 'IX_ChildGroupTypeId', 'INDEX'" );
            RenameColumn( table: "dbo.crmGroupTypeAssociation", name: "ChildGroupTypeId", newName: "ParentGroupTypeId" );
            RenameColumn( table: "dbo.crmGroupTypeAssociation", name: "GroupTypeId", newName: "ChildGroupTypeId" );
        }
    }
}

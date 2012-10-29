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
    public partial class GroupTypeAssociationColumnFix : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn(table: "dbo.crmGroupTypeAssociation", name: "ChildGroupTypeId", newName: "GroupTypeId");
            RenameColumn(table: "dbo.crmGroupTypeAssociation", name: "ParentGroupTypeId", newName: "ChildGroupTypeId");
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_ChildGroupTypeId', 'IX_GroupTypeId', 'INDEX'");
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_ParentGroupTypeId', 'IX_ChildGroupTypeId', 'INDEX'" );
            Sql( "sp_rename 'FK_crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId', 'FK_crmGroupTypeAssociation_crmGroupType_GroupTypeId'" );
            Sql( "sp_rename 'FK_crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId' , 'FK_crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "sp_rename 'FK_crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId' , 'FK_crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId'" );
            Sql( "sp_rename 'FK_crmGroupTypeAssociation_crmGroupType_GroupTypeId', 'FK_crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId'" );
            
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_ChildGroupTypeId', 'IX_ParentGroupTypeId', 'INDEX'" );           
            Sql( "sp_rename 'crmGroupTypeAssociation.IX_GroupTypeId', 'IX_ChildGroupTypeId', 'INDEX'" );
            RenameColumn(table: "dbo.crmGroupTypeAssociation", name: "ChildGroupTypeId", newName: "ParentGroupTypeId");
            RenameColumn(table: "dbo.crmGroupTypeAssociation", name: "GroupTypeId", newName: "ChildGroupTypeId");
        }
    }
}

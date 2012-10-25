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
    public partial class RenameGroupTables : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.groupsGroup", newName: "crmGroup");
            RenameTable(name: "dbo.groupsMember", newName: "crmGroupMember");
            RenameTable(name: "dbo.groupsGroupRole", newName: "crmGroupRole");
            RenameTable(name: "dbo.groupsGroupType", newName: "crmGroupType");
            RenameTable(name: "dbo.groupGroupLocation", newName: "crmGroupLocation");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameTable(name: "dbo.crmGroupLocation", newName: "groupGroupLocation");
            RenameTable(name: "dbo.crmGroupType", newName: "groupsGroupType");
            RenameTable(name: "dbo.crmGroupRole", newName: "groupsGroupRole");
            RenameTable(name: "dbo.crmGroupMember", newName: "groupsMember");
            RenameTable(name: "dbo.crmGroup", newName: "groupsGroup");
        }
    }
}

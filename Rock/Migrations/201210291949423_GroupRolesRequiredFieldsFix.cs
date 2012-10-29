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
    public partial class GroupRolesRequiredFieldsFix : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.crmGroupRole", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.crmGroupRole", "Description", c => c.String());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.crmGroupRole", "Description", c => c.String(nullable: false));
            AlterColumn("dbo.crmGroupRole", "Name", c => c.String(maxLength: 100));
        }
    }
}

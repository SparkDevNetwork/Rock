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
    public partial class UpdateLocation : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn(table: "dbo.GroupLocation", name: "LocationTypeValueId", newName: "GroupLocationTypeValueId");
            AddColumn("dbo.Location", "IsLocation", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupLocation", "IsMailing", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupLocation", "IsLocation", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupLocation", "IsLocation");
            DropColumn("dbo.GroupLocation", "IsMailing");
            DropColumn("dbo.Location", "IsLocation");
            RenameColumn(table: "dbo.GroupLocation", name: "GroupLocationTypeValueId", newName: "LocationTypeValueId");
        }
    }
}

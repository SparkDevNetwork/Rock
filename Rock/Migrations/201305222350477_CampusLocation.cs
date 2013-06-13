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
    public partial class CampusLocation : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Campus", "LocationId", c => c.Int());
            AddForeignKey("dbo.Campus", "LocationId", "dbo.Location", "Id");
            CreateIndex("dbo.Campus", "LocationId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Campus", new[] { "LocationId" });
            DropForeignKey("dbo.Campus", "LocationId", "dbo.Location");
            DropColumn("dbo.Campus", "LocationId");
        }
    }
}

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
    public partial class AddMetricValueCampusId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.MetricValue", "CampusId", c => c.Int());
            CreateIndex("dbo.MetricValue", "CampusId");
            AddForeignKey("dbo.MetricValue", "CampusId", "dbo.Campus", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.MetricValue", "CampusId", "dbo.Campus");
            DropIndex("dbo.MetricValue", new[] { "CampusId" });
            DropColumn("dbo.MetricValue", "CampusId");
        }
    }
}

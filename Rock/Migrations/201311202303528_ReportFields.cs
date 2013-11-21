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
    public partial class ReportFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ReportField",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReportId = c.Int(nullable: false),
                        ReportFieldType = c.Int(nullable: false),
                        ShowInGrid = c.Boolean(nullable: false),
                        DataSelectComponentEntityTypeId = c.Int(),
                        Selection = c.String(),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.DataSelectComponentEntityTypeId)
                .ForeignKey("dbo.Report", t => t.ReportId, cascadeDelete: true)
                .Index(t => t.DataSelectComponentEntityTypeId)
                .Index(t => t.ReportId);
            
            CreateIndex( "dbo.ReportField", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ReportField", "ReportId", "dbo.Report");
            DropForeignKey("dbo.ReportField", "DataSelectComponentEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.ReportField", new[] { "ReportId" });
            DropIndex("dbo.ReportField", new[] { "DataSelectComponentEntityTypeId" });
            DropTable("dbo.ReportField");
        }
    }
}

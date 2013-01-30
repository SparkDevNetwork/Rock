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
    public partial class Report : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Report",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        ReportFilterId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReportFilter", t => t.ReportFilterId, cascadeDelete: true)
                .Index(t => t.ReportFilterId);
            
            CreateIndex( "dbo.Report", "Guid", true );
            CreateTable(
                "dbo.ReportFilter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilterType = c.Int(nullable: false),
                        ParentId = c.Int(),
                        EntityTypeId = c.Int(),
                        Selection = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReportFilter", t => t.ParentId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.ParentId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.ReportFilter", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.ReportFilter", new[] { "EntityTypeId" });
            DropIndex("dbo.ReportFilter", new[] { "ParentId" });
            DropIndex("dbo.Report", new[] { "ReportFilterId" });
            DropForeignKey("dbo.ReportFilter", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.ReportFilter", "ParentId", "dbo.ReportFilter");
            DropForeignKey("dbo.Report", "ReportFilterId", "dbo.ReportFilter");
            DropTable("dbo.ReportFilter");
            DropTable("dbo.Report");
        }
    }
}

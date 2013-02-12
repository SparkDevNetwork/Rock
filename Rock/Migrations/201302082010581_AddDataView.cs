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
    public partial class AddDataView : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Report", "ReportFilterId", "dbo.ReportFilter");
            DropForeignKey("dbo.ReportFilter", "ParentId", "dbo.ReportFilter");
            DropForeignKey("dbo.ReportFilter", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.Report", new[] { "ReportFilterId" });
            DropIndex("dbo.ReportFilter", new[] { "ParentId" });
            DropIndex("dbo.ReportFilter", new[] { "EntityTypeId" });
            CreateTable(
                "dbo.DataView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CategoryId = c.Int(),
                        EntityTypeId = c.Int(),
                        DataViewFilterId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.DataViewFilter", t => t.DataViewFilterId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.DataViewFilterId);
            
            CreateIndex( "dbo.DataView", "Guid", true );
            CreateTable(
                "dbo.DataViewFilter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpressionType = c.Int(nullable: false),
                        ParentId = c.Int(),
                        EntityTypeId = c.Int(),
                        Selection = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataViewFilter", t => t.ParentId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.ParentId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.DataViewFilter", "Guid", true );
            AddColumn("dbo.Report", "CategoryId", c => c.Int());
            AddColumn("dbo.Report", "DataViewId", c => c.Int());
            AddForeignKey("dbo.Report", "CategoryId", "dbo.Category", "Id");
            AddForeignKey("dbo.Report", "DataViewId", "dbo.DataView", "Id");
            CreateIndex("dbo.Report", "CategoryId");
            CreateIndex("dbo.Report", "DataViewId");
            DropColumn("dbo.Report", "ReportFilterId");
            DropTable("dbo.ReportFilter");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
            // TableName: ReportFilter
            // The given key was not present in the dictionary.
            // TableName: ReportFilter
            // The given key was not present in the dictionary.
            // TableName: ReportFilter
            // The given key was not present in the dictionary.
            // TableName: ReportFilter
            // The given key was not present in the dictionary.
            // TableName: ReportFilter
            // The given key was not present in the dictionary.
            // TableName: ReportFilter
            // The given key was not present in the dictionary.
            AddColumn("dbo.Report", "ReportFilterId", c => c.Int());
            DropIndex("dbo.Report", new[] { "DataViewId" });
            DropIndex("dbo.Report", new[] { "CategoryId" });
            DropIndex("dbo.DataViewFilter", new[] { "EntityTypeId" });
            DropIndex("dbo.DataViewFilter", new[] { "ParentId" });
            DropIndex("dbo.DataView", new[] { "DataViewFilterId" });
            DropIndex("dbo.DataView", new[] { "EntityTypeId" });
            DropIndex("dbo.DataView", new[] { "CategoryId" });
            DropForeignKey("dbo.Report", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.Report", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.DataViewFilter", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.DataViewFilter", "ParentId", "dbo.DataViewFilter");
            DropForeignKey("dbo.DataView", "DataViewFilterId", "dbo.DataViewFilter");
            DropForeignKey("dbo.DataView", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.DataView", "CategoryId", "dbo.Category");
            DropColumn("dbo.Report", "DataViewId");
            DropColumn("dbo.Report", "CategoryId");
            DropTable("dbo.DataViewFilter");
            DropTable("dbo.DataView");
            CreateIndex("dbo.ReportFilter", "EntityTypeId");
            CreateIndex("dbo.ReportFilter", "ParentId");
            CreateIndex("dbo.Report", "ReportFilterId");
            AddForeignKey("dbo.ReportFilter", "EntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.ReportFilter", "ParentId", "dbo.ReportFilter", "Id");
            AddForeignKey("dbo.Report", "ReportFilterId", "dbo.ReportFilter", "Id", cascadeDelete: true);
        }
    }
}

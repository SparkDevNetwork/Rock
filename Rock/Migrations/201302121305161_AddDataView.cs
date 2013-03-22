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
    public partial class AddDataView : RockMigration_4
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

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Data Views", "Edit the available Data Views that are used for filtering data", "Default", "FDA8A444-9132-4905-857B-41B3A38C6D22" );
            AddPage( "FDA8A444-9132-4905-857B-41B3A38C6D22", "Data View Detail", "", "Default", "18FAD918-5B42-4523-BBE1-FF2A08C647BF" );
            AddBlockType( "Reporting - Data View Detail", "", "~/Blocks/Reporting/DataViewDetail.ascx", "EB279DF9-D817-4905-B6AC-D9883F0DA2E4" );
            AddBlockType( "Reporting - Data View List", "", "~/Blocks/Reporting/DataViewList.ascx", "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9" );
            AddBlock( "18FAD918-5B42-4523-BBE1-FF2A08C647BF", "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "Data View Detail", "", "Content", 0, "49B797DC-8D45-48CB-8AF6-849BDFBC6ABE" );
            AddBlock( "FDA8A444-9132-4905-857B-41B3A38C6D22", "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9", "Data Views", "", "Content", 0, "5FAAD78F-2681-4B4B-83D2-02B7C712D6EA" );
            AddBlockTypeAttribute( "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "20E83814-FBB9-47A1-9B99-702EB6750937" );

            // Attrib Value for Data Views:Detail Page Guid
            AddBlockAttributeValue( "5FAAD78F-2681-4B4B-83D2-02B7C712D6EA", "20E83814-FBB9-47A1-9B99-702EB6750937", "18fad918-5b42-4523-bbe1-ff2a08c647bf" );

            Sql( @"
UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-filter'
WHERE [Guid] = 'FDA8A444-9132-4905-857B-41B3A38C6D22'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "20E83814-FBB9-47A1-9B99-702EB6750937" );
            DeleteBlock( "49B797DC-8D45-48CB-8AF6-849BDFBC6ABE" );
            DeleteBlock( "5FAAD78F-2681-4B4B-83D2-02B7C712D6EA" );
            DeleteBlockType( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4" );
            DeleteBlockType( "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9" );
            DeletePage( "18FAD918-5B42-4523-BBE1-FF2A08C647BF" );
            DeletePage( "FDA8A444-9132-4905-857B-41B3A38C6D22" );

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

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
    public partial class AddCategoryIcon : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Category", "FileId", "dbo.BinaryFile");
            DropForeignKey("dbo.WorkflowType", "IconSmallFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.WorkflowType", "IconLargeFileId", "dbo.BinaryFile");
            DropIndex("dbo.Category", new[] { "FileId" });
            DropIndex("dbo.WorkflowType", new[] { "IconSmallFileId" });
            DropIndex("dbo.WorkflowType", new[] { "IconLargeFileId" });
            AddColumn("dbo.Category", "IconSmallFileId", c => c.Int());
            AddColumn("dbo.Category", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.Category", "IconCssClass", c => c.String());
            AddForeignKey("dbo.Category", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.Category", "IconLargeFileId", "dbo.BinaryFile", "Id");
            CreateIndex("dbo.Category", "IconSmallFileId");
            CreateIndex("dbo.Category", "IconLargeFileId");
            DropColumn("dbo.Category", "FileId");
            DropColumn("dbo.WorkflowType", "IconSmallFileId");
            DropColumn("dbo.WorkflowType", "IconLargeFileId");
            DropColumn("dbo.WorkflowType", "IconCssClass");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.WorkflowType", "IconCssClass", c => c.String());
            AddColumn("dbo.WorkflowType", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.WorkflowType", "IconSmallFileId", c => c.Int());
            AddColumn("dbo.Category", "FileId", c => c.Int());
            DropIndex("dbo.Category", new[] { "IconLargeFileId" });
            DropIndex("dbo.Category", new[] { "IconSmallFileId" });
            DropForeignKey("dbo.Category", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Category", "IconSmallFileId", "dbo.BinaryFile");
            DropColumn("dbo.Category", "IconCssClass");
            DropColumn("dbo.Category", "IconLargeFileId");
            DropColumn("dbo.Category", "IconSmallFileId");
            CreateIndex("dbo.WorkflowType", "IconLargeFileId");
            CreateIndex("dbo.WorkflowType", "IconSmallFileId");
            CreateIndex("dbo.Category", "FileId");
            AddForeignKey("dbo.WorkflowType", "IconLargeFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.WorkflowType", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.Category", "FileId", "dbo.BinaryFile", "Id");
        }
    }
}

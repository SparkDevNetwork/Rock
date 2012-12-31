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
    public partial class AddWorkflowTypeIcon : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.WorkflowType", "FileId", "dbo.BinaryFile");
            DropIndex("dbo.WorkflowType", new[] { "FileId" });
            AddColumn("dbo.WorkflowType", "IconSmallFileId", c => c.Int());
            AddColumn("dbo.WorkflowType", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.WorkflowType", "IconCssClass", c => c.String());
            AddForeignKey("dbo.WorkflowType", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.WorkflowType", "IconLargeFileId", "dbo.BinaryFile", "Id");
            CreateIndex("dbo.WorkflowType", "IconSmallFileId");
            CreateIndex("dbo.WorkflowType", "IconLargeFileId");
            DropColumn("dbo.WorkflowType", "FileId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.WorkflowType", "FileId", c => c.Int());
            DropIndex("dbo.WorkflowType", new[] { "IconLargeFileId" });
            DropIndex("dbo.WorkflowType", new[] { "IconSmallFileId" });
            DropForeignKey("dbo.WorkflowType", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.WorkflowType", "IconSmallFileId", "dbo.BinaryFile");
            DropColumn("dbo.WorkflowType", "IconCssClass");
            DropColumn("dbo.WorkflowType", "IconLargeFileId");
            DropColumn("dbo.WorkflowType", "IconSmallFileId");
            CreateIndex("dbo.WorkflowType", "FileId");
            AddForeignKey("dbo.WorkflowType", "FileId", "dbo.BinaryFile", "Id");
        }
    }
}

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
    public partial class PageIconFile : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Page", "IconFileId", c => c.Int());
            AddForeignKey("dbo.Page", "IconFileId", "dbo.BinaryFile", "Id");
            CreateIndex("dbo.Page", "IconFileId");
            DropColumn("dbo.Page", "IconUrl");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Page", "IconUrl", c => c.String(maxLength: 150));
            DropIndex("dbo.Page", new[] { "IconFileId" });
            DropForeignKey("dbo.Page", "IconFileId", "dbo.BinaryFile");
            DropColumn("dbo.Page", "IconFileId");
        }
    }
}

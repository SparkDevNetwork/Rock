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
    public partial class ExceptionLog_Page_SiteFKs : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddForeignKey("dbo.ExceptionLog", "SiteId", "dbo.Site", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ExceptionLog", "PageId", "dbo.Page", "Id", cascadeDelete: true);
            CreateIndex("dbo.ExceptionLog", "SiteId");
            CreateIndex("dbo.ExceptionLog", "PageId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.ExceptionLog", new[] { "PageId" });
            DropIndex("dbo.ExceptionLog", new[] { "SiteId" });
            DropForeignKey("dbo.ExceptionLog", "PageId", "dbo.Page");
            DropForeignKey("dbo.ExceptionLog", "SiteId", "dbo.Site");
        }
    }
}

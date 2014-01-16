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
    public partial class Add404FieldsToSite : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "PageNotFoundPageId", c => c.Int());
            AddColumn("dbo.Site", "PageNotFoundPageRouteId", c => c.Int());
            CreateIndex("dbo.Site", "PageNotFoundPageId");
            CreateIndex("dbo.Site", "PageNotFoundPageRouteId");
            AddForeignKey("dbo.Site", "PageNotFoundPageId", "dbo.Page", "Id");
            AddForeignKey("dbo.Site", "PageNotFoundPageRouteId", "dbo.PageRoute", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Site", "PageNotFoundPageRouteId", "dbo.PageRoute");
            DropForeignKey("dbo.Site", "PageNotFoundPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "PageNotFoundPageRouteId" });
            DropIndex("dbo.Site", new[] { "PageNotFoundPageId" });
            DropColumn("dbo.Site", "PageNotFoundPageRouteId");
            DropColumn("dbo.Site", "PageNotFoundPageId");
        }
    }
}

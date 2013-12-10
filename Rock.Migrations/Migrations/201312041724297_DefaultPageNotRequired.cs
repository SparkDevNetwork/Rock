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
    public partial class DefaultPageNotRequired : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Site", "DefaultPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "DefaultPageId" });
            AlterColumn("dbo.Site", "DefaultPageId", c => c.Int());
            CreateIndex("dbo.Site", "DefaultPageId");
            AddForeignKey("dbo.Site", "DefaultPageId", "dbo.Page", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Site", "DefaultPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "DefaultPageId" });
            AlterColumn("dbo.Site", "DefaultPageId", c => c.Int(nullable: false));
            CreateIndex("dbo.Site", "DefaultPageId");
            AddForeignKey("dbo.Site", "DefaultPageId", "dbo.Page", "Id");
        }
    }
}

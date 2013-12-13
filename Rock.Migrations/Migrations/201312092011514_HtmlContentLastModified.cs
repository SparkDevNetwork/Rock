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
    public partial class HtmlContentLastModified : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.HtmlContent", "LastModifiedDateTime", c => c.DateTime());
            AddColumn("dbo.HtmlContent", "LastModifiedPersonId", c => c.Int());
            CreateIndex("dbo.HtmlContent", "LastModifiedPersonId");
            AddForeignKey("dbo.HtmlContent", "LastModifiedPersonId", "dbo.Person", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.HtmlContent", "LastModifiedPersonId", "dbo.Person");
            DropIndex("dbo.HtmlContent", new[] { "LastModifiedPersonId" });
            DropColumn("dbo.HtmlContent", "LastModifiedPersonId");
            DropColumn("dbo.HtmlContent", "LastModifiedDateTime");
        }
    }
}

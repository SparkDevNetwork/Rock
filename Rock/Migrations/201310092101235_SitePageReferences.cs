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
    public partial class SitePageReferences : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "LoginPageId", c => c.Int());
            AddColumn("dbo.Site", "LoginPageRouteId", c => c.Int());
            AddColumn("dbo.Site", "RegistrationPageId", c => c.Int());
            AddColumn("dbo.Site", "RegistrationPageRouteId", c => c.Int());
            CreateIndex("dbo.Site", "LoginPageId");
            CreateIndex("dbo.Site", "LoginPageRouteId");
            CreateIndex("dbo.Site", "RegistrationPageId");
            CreateIndex("dbo.Site", "RegistrationPageRouteId");
            AddForeignKey("dbo.Site", "LoginPageId", "dbo.Page", "Id");
            AddForeignKey("dbo.Site", "LoginPageRouteId", "dbo.PageRoute", "Id");
            AddForeignKey("dbo.Site", "RegistrationPageId", "dbo.Page", "Id");
            AddForeignKey("dbo.Site", "RegistrationPageRouteId", "dbo.PageRoute", "Id");
            DropColumn("dbo.Site", "LoginPageReference");
            DropColumn("dbo.Site", "RegistrationPageReference");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Site", "RegistrationPageReference", c => c.String(maxLength: 260));
            AddColumn("dbo.Site", "LoginPageReference", c => c.String(maxLength: 260));
            DropForeignKey("dbo.Site", "RegistrationPageRouteId", "dbo.PageRoute");
            DropForeignKey("dbo.Site", "RegistrationPageId", "dbo.Page");
            DropForeignKey("dbo.Site", "LoginPageRouteId", "dbo.PageRoute");
            DropForeignKey("dbo.Site", "LoginPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "RegistrationPageRouteId" });
            DropIndex("dbo.Site", new[] { "RegistrationPageId" });
            DropIndex("dbo.Site", new[] { "LoginPageRouteId" });
            DropIndex("dbo.Site", new[] { "LoginPageId" });
            DropColumn("dbo.Site", "RegistrationPageRouteId");
            DropColumn("dbo.Site", "RegistrationPageId");
            DropColumn("dbo.Site", "LoginPageRouteId");
            DropColumn("dbo.Site", "LoginPageId");
        }
    }
}

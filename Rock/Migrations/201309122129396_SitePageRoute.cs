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
    public partial class SitePageRoute : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Site", "DefaultPageRouteId", c => c.Int() );
            DropIndex( "dbo.Site", new string[] { "DefaultPageId" } );
            AlterColumn( "dbo.Site", "DefaultPageId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.Site", "DefaultPageId" );
            CreateIndex( "dbo.Site", "DefaultPageRouteId" );
            AddForeignKey( "dbo.Site", "DefaultPageRouteId", "dbo.PageRoute", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Site", "DefaultPageRouteId", "dbo.PageRoute" );
            DropIndex( "dbo.Site", new[] { "DefaultPageRouteId" } );
            DropIndex( "dbo.Site", new string[] { "DefaultPageId" } );
            AlterColumn( "dbo.Site", "DefaultPageId", c => c.Int() );
            CreateIndex( "dbo.Site", "DefaultPageId" );
            DropColumn( "dbo.Site", "DefaultPageRouteId" );
        }
    }
}

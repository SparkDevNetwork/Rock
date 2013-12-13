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
    public partial class SiteUpdateRemoveIcons : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropColumn("dbo.Site", "FaviconUrl");
            DropColumn("dbo.Site", "AppleTouchIconUrl");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Site", "AppleTouchIconUrl", c => c.String(maxLength: 150));
            AddColumn("dbo.Site", "FaviconUrl", c => c.String(maxLength: 150));
        }
    }
}

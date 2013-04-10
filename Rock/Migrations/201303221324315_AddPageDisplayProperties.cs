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
    public partial class AddPageDisplayProperties : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Page", "PageDisplayTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.Page", "PageDisplayBreadCrumb", c => c.Boolean(nullable: false));
            AddColumn("dbo.Page", "PageDisplayIcon", c => c.Boolean(nullable: false));
            AddColumn("dbo.Page", "PageDisplayDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.Page", "BreadCrumbDisplayName", c => c.Boolean(nullable: false));
            AddColumn("dbo.Page", "BreadCrumbDisplayIcon", c => c.Boolean(nullable: false));

            Sql( @"
    -- Set new field defaults for all pages
    UPDATE [Page] SET
        PageDisplayTitle = 1,
        PageDisplayBreadcrumb = 1,
        PageDisplayIcon = 1,
        PageDisplayDescription = 1,
        BreadCrumbDisplayName = 1,
        BreadCrumbDisplayIcon = 0

    -- Update home page to not show title, crumbs, icon or description and add home icon
    UPDATE [Page] SET
	    [Name] = 'Home',
	    [IconCssClass] = 'icon-home',
	    [PageDisplayTitle] = 0,
	    [PageDisplayBreadCrumb] = 0,
	    [PageDisplayIcon] = 0,
	    [PageDisplayDescription] = 0,
	    [BreadCrumbDisplayIcon] = 1
    WHERE [Guid] = '20F97A93-7949-4C2A-8A5E-C756FE8585CA'

    -- Update all pages that don't have blocks to not show in the breadcrumb trail
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0
    WHERE [Id] NOT IN (SELECT DISTINCT ISNULL([PageId],0) FROM [Block])

    -- Some blocks handle the breadcrumbs, update the pages with those blocks to not display page name in breadcrumbs
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0
    WHERE [Guid] IN (
	    '64521E8E-3BA7-409C-A18F-4ACAAC6758CE'  -- Marketing Campaign Detail
    )

" );
        }
       
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Page", "BreadCrumbDisplayIcon");
            DropColumn("dbo.Page", "BreadCrumbDisplayName");
            DropColumn("dbo.Page", "PageDisplayDescription");
            DropColumn("dbo.Page", "PageDisplayIcon");
            DropColumn("dbo.Page", "PageDisplayBreadCrumb");
            DropColumn("dbo.Page", "PageDisplayTitle");
        }
    }
}

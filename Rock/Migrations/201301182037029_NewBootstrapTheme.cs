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
    public partial class NewBootstrapTheme : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

                update Page set IconCssClass = 'icon-book' where Id = 17
                update Page set IconCssClass = 'icon-money' where Id = 43
                update Page set IconCssClass = 'icon-briefcase' where Id = 44
                update Page set IconCssClass = 'icon-wrench' where Id = 105

                update Page set IconCssClass = 'icon-globe' where Id = 51
                update Page set IconCssClass = 'icon-building' where Id = 85
                update Page set IconCssClass = 'icon-unlock' where Id = 96
                update Page set IconCssClass = 'icon-map-marker' where Id = 38
                update Page set IconCssClass = 'icon-search' where Id = 88
                update Page set IconCssClass = 'icon-time' where Id = 102
                update Page set IconCssClass = 'icon-envelope' where Id = 46
                update Page set IconCssClass = 'icon-tags' where Id = 90
                update Page set IconCssClass = 'icon-sign-blank' where Id = 104
                update Page set IconCssClass = 'icon-icon-book' where Id = 84
                update Page set IconCssClass = 'icon-icon-bar-chart' where Id = 94
                update Page set IconCssClass = 'icon-group' where Id = 110

                update Page set IconCssClass = 'icon-desktop' where Id = 2
                update Page set IconCssClass = 'icon-th-large' where Id = 7
                update Page set IconCssClass = 'icon-lightbulb' where Id = 101
                update Page set IconCssClass = 'icon-sitemap' where Id = 103

                update Page set IconCssClass = 'icon-sitemap' where Id = 97
                update Page set IconCssClass = 'icon-pushpin' where Id = 71
                update Page set IconCssClass = 'icon-asterisk' where Id = 99
                update Page set IconCssClass = 'icon-th-list' where Id = 61

                update Page set MenuDisplayDescription = 0 where ParentPageId = 77
                update Page set MenuDisplayDescription = 0 where ParentPageId = 76
                update Page set MenuDisplayDescription = 0 where ParentPageId = 78
                update Page set MenuDisplayDescription = 0 where ParentPageId = 79

             " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

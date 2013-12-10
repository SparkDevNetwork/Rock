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
    public partial class HtmlContentDetail : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"Update [BlockType] set [Path] = '~/Blocks/Cms/HtmlContentDetail.ascx' where [Guid] = '19b61d65-37e3-459f-a44f-def0089118a3'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"Update [BlockType] set [Path] = '~/Blocks/Cms/HtmlContent.ascx' where [Guid] = '19b61d65-37e3-459f-a44f-def0089118a3'" );
        }
    }
}

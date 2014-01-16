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
    public partial class FixHeaderMarkupOnHomepage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"UPDATE [Block]
	                SET
		                [PreHtml] = REPLACE(REPLACE([PreHtml], '<h3', '<h4'), '</h3>', '</h4>')
	                WHERE [Guid] = '6A648E77-ABA9-4AAF-A8BB-027A12261ED9'");

            Sql(@"UPDATE [Block]
	                SET
		                [PreHtml] = REPLACE(REPLACE([PreHtml], '<h3', '<h4'), '</h3>', '</h4>')
	                WHERE [Guid] = '62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

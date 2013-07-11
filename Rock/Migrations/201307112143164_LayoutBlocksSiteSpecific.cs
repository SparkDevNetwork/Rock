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
    public partial class LayoutBlocksSiteSpecific : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Block", "SiteId", c => c.Int());
            AddForeignKey("dbo.Block", "SiteId", "dbo.Site", "Id", cascadeDelete: true);
            CreateIndex("dbo.Block", "SiteId");

            Sql( @"
    DECLARE @RockSiteId int
    SET @RockSiteId = (SELECT [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4')

    UPDATE [Block]
    SET [SiteId] = @RockSiteId
    WHERE [PageId] IS NULL
    AND [Layout] IN ('Default', 'PersonDetail', 'TwoColumnLeft')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Block", new[] { "SiteId" });
            DropForeignKey("dbo.Block", "SiteId", "dbo.Site");
            DropColumn("dbo.Block", "SiteId");
        }
    }
}

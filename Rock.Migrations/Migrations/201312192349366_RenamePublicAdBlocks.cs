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
    public partial class RenamePublicAdBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Cms/AdDetail.ascx' AND [GUID] != '98D27912-C4BD-4E94-AEE1-AFBF688D7264'"); // delete block type with the new path as it was probably auto added by the framework
            Sql(@"UPDATE [BlockType] set [Path] = '~/Blocks/Cms/AdDetail.ascx' where [Guid] = '98D27912-C4BD-4E94-AEE1-AFBF688D7264'");

            Sql(@"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Cms/AdList.ascx' AND [GUID] != '5A880084-7237-449A-9855-3FA02B6BD79F'"); // delete block type with the new path as it was probably auto added by the framework
            Sql(@"UPDATE [BlockType] set [Path] = '~/Blocks/Cms/AdList.ascx' where [Guid] = '5A880084-7237-449A-9855-3FA02B6BD79F'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

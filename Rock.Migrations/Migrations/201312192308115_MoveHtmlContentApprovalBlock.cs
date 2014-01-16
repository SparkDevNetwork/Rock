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
    public partial class MoveHtmlContentApprovalBlock : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Cms/HtmlContentApproval.ascx' AND [GUID] != '79E4D7D2-3F18-43A9-9A62-E02F09C6051C'"); // delete block type with the new path as it was probably auto added by the framework
            Sql(@"UPDATE [BlockType] set [Path] = '~/Blocks/Cms/HtmlContentApproval.ascx' where [Guid] = '79E4D7D2-3F18-43A9-9A62-E02F09C6051C'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

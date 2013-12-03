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
    public partial class AddHtmlContentApprovalBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Core - HTML Content Approval", "HTML Content Approval", "~/Blocks/Administration/HtmlContentApproval.ascx", "79E4D7D2-3F18-43A9-9A62-E02F09C6051C" );
            AddBlock( "9DF95EFF-88B4-401A-8F5F-E3B8DB02A308", "", "79E4D7D2-3F18-43A9-9A62-E02F09C6051C", "HTML Content Approval", "Main", 0, "D6691357-A904-43EE-815C-FEE7A752E3AA" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "D6691357-A904-43EE-815C-FEE7A752E3AA" );
            DeleteBlockType( "79E4D7D2-3F18-43A9-9A62-E02F09C6051C" );
        }
    }
}

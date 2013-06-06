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
    public partial class GroupMemberListBlockAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Crm - Group Member List:Group
            AddBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "", "Either pick a specific group or choose <none> to have group be determined by the groupId page parameter", 0, "", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A" ); // Group
        }
    }
}

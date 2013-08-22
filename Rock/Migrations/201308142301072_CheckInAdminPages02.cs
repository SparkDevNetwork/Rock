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
    public partial class CheckInAdminPages02 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete old location of Checkin in menu
            Sql( @"delete from [Page] where [Guid] in ('FB0A7D8A-F9F4-4081-B15B-7970D20698E3','F9B48E2A-7D49-45B6-AA88-D731AD887B0F');" );

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Check-in", "", "Default", "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "icon-check-sign" );
            AddPage( "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "Schedule Builder", "", "Default", "A286D16B-FDDF-4D89-B98F-D51188B611E6", "icon-calendar" );
            
            AddPage( "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "Configuration", "", "Default", "4AB679AF-C8CC-427C-A615-0BF9F52E8E3E", "" );
            
            AddBlockType( "Administration - Check-In Group Type List", "", "~/Blocks/Administration/CheckInGroupTypeList.ascx", "12E586CF-DB55-4654-A13E-1F825BBA1C7C" );
            
            // Add Block to Page: Check-in
            AddBlock( "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "12E586CF-DB55-4654-A13E-1F825BBA1C7C", "Check In Group Type List", "", "Content", 0, "883CE93C-2AF9-443B-9531-B8E5277D3CEA" );

            // Add Block to Page: Schedule Builder
            AddBlock( "A286D16B-FDDF-4D89-B98F-D51188B611E6", "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4", "Check-in Schedule Builder", "", "Content", 0, "C211AA9D-039B-492F-BBEB-7AEB33E920F3" );

            // Attrib for BlockType: Administration - Check In Group Type List:Schedule Builder Page
            AddBlockTypeAttribute( "12E586CF-DB55-4654-A13E-1F825BBA1C7C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Schedule Builder Page", "ScheduleBuilderPage", "", "", 0, "", "01EAE18B-86F3-4149-9F23-0530BA500CF0" );

            // Attrib for BlockType: Administration - Check In Group Type List:Configure Groups Page
            AddBlockTypeAttribute( "12E586CF-DB55-4654-A13E-1F825BBA1C7C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configure Groups Page", "ConfigureGroupsPage", "", "Page for configuration of Check-in Groups/Locations", 0, "", "589150D6-6D67-479E-910B-62B4389842B1" );

            // Attrib Value for Block:Check In Group Type List, Attribute:Schedule Builder Page, Page:Check-in
            AddBlockAttributeValue( "883CE93C-2AF9-443B-9531-B8E5277D3CEA", "01EAE18B-86F3-4149-9F23-0530BA500CF0", "a286d16b-fddf-4d89-b98f-d51188b611e6" );

            // Attrib Value for Block:Check In Group Type List, Attribute:Configure Groups Page, Page:Check-in
            AddBlockAttributeValue( "883CE93C-2AF9-443B-9531-B8E5277D3CEA", "589150D6-6D67-479E-910B-62B4389842B1", "4ab679af-c8cc-427c-a615-0bf9f52e8e3e" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Administration - Check In Group Type List:Configure Groups Page
            DeleteAttribute( "589150D6-6D67-479E-910B-62B4389842B1" );
            // Attrib for BlockType: Administration - Check In Group Type List:Schedule Builder Page
            DeleteAttribute( "01EAE18B-86F3-4149-9F23-0530BA500CF0" );

            // Remove Block: Check-in Schedule Builder, from Page: Schedule Builder
            DeleteBlock( "C211AA9D-039B-492F-BBEB-7AEB33E920F3" );
            // Remove Block: Check In Group Type List, from Page: Check-in
            DeleteBlock( "883CE93C-2AF9-443B-9531-B8E5277D3CEA" );

            DeleteBlockType( "12E586CF-DB55-4654-A13E-1F825BBA1C7C" ); // Administration - Check In Group Type List

            DeletePage( "4AB679AF-C8CC-427C-A615-0BF9F52E8E3E" ); // Configuration
            DeletePage( "A286D16B-FDDF-4D89-B98F-D51188B611E6" ); // Schedule Builder
            DeletePage( "C646A95A-D12D-4A67-9BE6-C9695C0267ED" ); // Check-in
        }
    }
}

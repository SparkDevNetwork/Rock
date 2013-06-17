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
    public partial class ScheduleListDetailBlocks : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Schedules", "Configure Schedules used throughout the system", "Default", "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "icon-calendar" );
            AddPage( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "Schedule Detail", "", "Default", "8A4D4487-6E58-4104-969C-6DA06A7FC599" );

            AddBlockType( "Administration - Schedule List", "", "~/Blocks/Administration/ScheduleList.ascx", "C1B934D1-2139-471E-B2B8-B22FF4499B2F" );
            AddBlockType( "Administration - Schedule Detail", "", "~/Blocks/Administration/ScheduleDetail.ascx", "59C9C862-570C-4410-99B6-DD9064B5E594" );
            AddBlock( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "Schedule List", "", "Content", 0, "85829468-7DC0-4A14-A0E2-E1D5E7A57B18" );
            AddBlock( "8A4D4487-6E58-4104-969C-6DA06A7FC599", "59C9C862-570C-4410-99B6-DD9064B5E594", "Schedule Detail", "", "Content", 0, "38915E79-FF53-4C8F-B5C1-C6532ED39929" );

            AddBlockTypeAttribute( "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "00F227B2-C977-4BA6-816A-F45C6FE9EF5A" );

            // Attrib Value for Schedule List:Detail Page
            AddBlockAttributeValue( "85829468-7DC0-4A14-A0E2-E1D5E7A57B18", "00F227B2-C977-4BA6-816A-F45C6FE9EF5A", "8a4d4487-6e58-4104-969c-6da06a7fc599" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "00F227B2-C977-4BA6-816A-F45C6FE9EF5A" ); // Detail Page
            DeleteBlock( "85829468-7DC0-4A14-A0E2-E1D5E7A57B18" ); // Schedule List
            DeleteBlock( "38915E79-FF53-4C8F-B5C1-C6532ED39929" ); // Schedule Detail

            DeleteBlockType( "C1B934D1-2139-471E-B2B8-B22FF4499B2F" ); // Administration - Schedule List
            DeleteBlockType( "59C9C862-570C-4410-99B6-DD9064B5E594" ); // Administration - Schedule Detail

            DeletePage( "8A4D4487-6E58-4104-969C-6DA06A7FC599" ); // Schedule Detail
            DeletePage( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1" ); // Schedule List
        }
    }
}

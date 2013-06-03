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
    public partial class ScheduleCategory : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Schedule", "CheckInStartOffsetMinutes", c => c.Int());
            AddColumn("dbo.Schedule", "CheckInEndOffsetMinutes", c => c.Int());
            AddColumn("dbo.Schedule", "CategoryId", c => c.Int());
            AddForeignKey("dbo.Schedule", "CategoryId", "dbo.Category", "Id");
            CreateIndex("dbo.Schedule", "CategoryId");
            DropColumn("dbo.Schedule", "StartTime");
            DropColumn("dbo.Schedule", "EndTime");
            DropColumn("dbo.Schedule", "CheckInStartTime");
            DropColumn("dbo.Schedule", "CheckInEndTime");
            DropColumn("dbo.Schedule", "IsShared");

            DeleteBlock( "85829468-7DC0-4A14-A0E2-E1D5E7A57B18" ); // Schedule List

            Sql( "UPDATE [Page] SET [Layout] = 'TwoColumnLeft' WHERE [Guid] = 'F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1'" );

            AddBlock( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "ADE003C7-649B-466A-872B-B8AC952E7841", "Schedule Categories", "", "LeftContent", 0, "35D97498-A085-4745-928C-7E119ADF8833" );
            AddBlock( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "59C9C862-570C-4410-99B6-DD9064B5E594", "Schedule Detail", "", "RightContent", 0, "09A8B2C4-F07E-4FF2-ADD7-AD8CC1DFE3B1" );
            AddBlock( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "", "RightContent", 1, "72F40FDD-EE99-45EE-ADED-94D50624E3F9" );

            // Attrib Value for Schedule Categories:Entity Type
            AddBlockAttributeValue( "35D97498-A085-4745-928C-7E119ADF8833", "06D414F0-AA20-4D3C-B297-1530CCD64395", "Rock.Model.Schedule" );
            // Attrib Value for Schedule Categories:Detail Page
            AddBlockAttributeValue( "35D97498-A085-4745-928C-7E119ADF8833", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", "f5d6d7dd-fd5f-494c-83dc-e2af63c705d1" );
            // Attrib Value for Schedule Categories:Page Parameter Key
            AddBlockAttributeValue( "35D97498-A085-4745-928C-7E119ADF8833", "AA057D3E-00CC-42BD-9998-600873356EDB", "ScheduleId" );
            // Attrib Value for Category Detail:Entity Type
            AddBlockAttributeValue( "72F40FDD-EE99-45EE-ADED-94D50624E3F9", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "Rock.Model.Schedule" );

            DeletePage( "8A4D4487-6E58-4104-969C-6DA06A7FC599" ); // Schedule Detail
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddPage( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "Schedule Detail", "", "Default", "8A4D4487-6E58-4104-969C-6DA06A7FC599" );
            AddBlock( "8A4D4487-6E58-4104-969C-6DA06A7FC599", "59C9C862-570C-4410-99B6-DD9064B5E594", "Schedule Detail", "", "Content", 0, "38915E79-FF53-4C8F-B5C1-C6532ED39929" );

            DeleteBlock( "09A8B2C4-F07E-4FF2-ADD7-AD8CC1DFE3B1" ); // Schedule Detail
            DeleteBlock( "72F40FDD-EE99-45EE-ADED-94D50624E3F9" ); // Category Detail
            DeleteBlock( "35D97498-A085-4745-928C-7E119ADF8833" ); // Schedule Categories

            Sql( "UPDATE [Page] SET [Layout] = 'Default' WHERE [Guid] = 'F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1'" );

            AddBlock( "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1", "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "Schedule List", "", "Content", 0, "85829468-7DC0-4A14-A0E2-E1D5E7A57B18" );
            AddBlockAttributeValue( "85829468-7DC0-4A14-A0E2-E1D5E7A57B18", "00F227B2-C977-4BA6-816A-F45C6FE9EF5A", "8a4d4487-6e58-4104-969c-6da06a7fc599" );

            AddColumn( "dbo.Schedule", "IsShared", c => c.Boolean( nullable: false ) );
            AddColumn("dbo.Schedule", "CheckInEndTime", c => c.Time());
            AddColumn("dbo.Schedule", "CheckInStartTime", c => c.Time());
            AddColumn("dbo.Schedule", "EndTime", c => c.Time(nullable: false));
            AddColumn("dbo.Schedule", "StartTime", c => c.Time(nullable: false));
            DropIndex("dbo.Schedule", new[] { "CategoryId" });
            DropForeignKey("dbo.Schedule", "CategoryId", "dbo.Category");
            DropColumn("dbo.Schedule", "CategoryId");
            DropColumn("dbo.Schedule", "CheckInEndOffsetMinutes");
            DropColumn("dbo.Schedule", "CheckInStartOffsetMinutes");
        }
    }
}

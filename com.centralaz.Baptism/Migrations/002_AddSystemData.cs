using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class AddSystemData : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "0B2C38A7-D79C-4F85-9757-F1B045D32C8A", "Baptism Blackout Times", "", "", "FFC06491-1BE9-4F3B-AC76-81A47E17D0AE" );

            RockMigrationHelper.AddGroupType( "Baptism Locations", "", "Group", "Member", false, true, true, "", 0, null, 0, null, "32F8592C-AE11-44A7-A053-DE43789811D9", false );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "32F8592C-AE11-44A7-A053-DE43789811D9", Rock.SystemGuid.FieldType.CATEGORY, "Blackout Dates", "", 0, "ffc06491-1be9-4f3b-ac76-81a47e17d0ae", "D58F0DB5-09AA-4A5A-BC17-CE3E3985D6F8" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "32F8592C-AE11-44A7-A053-DE43789811D9", Rock.SystemGuid.FieldType.CATEGORY, "Service Times", "", 0, "4fecc91b-83f9-4269-ae03-a006f401c47e", "B7371337-57CB-4CB3-994C-72258729950F" );

            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Baptisms", "", "F08B7F8F-5EC6-4CEC-8C20-5AD5EC5A3800", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "F08B7F8F-5EC6-4CEC-8C20-5AD5EC5A3800", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Baptism Scheduler", "", "B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Baptism Detail", "", "AD8BE3C5-54F5-48F7-B699-01D53238CD23", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Blackout Date Detail", "", "A3882EAE-F086-467B-9F3C-DC0DB75403F7", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Baptism Add Baptism Block", "Block for adding a baptism", "~/Plugins/com_centralaz/Baptism/BaptismAddBaptism.ascx", "com_centralaz > Baptism", "034AEBD6-FE85-4CC0-B060-25E93DCE20EB" );
            RockMigrationHelper.UpdateBlockType( "Baptism Add Blackout Date Block", "Block for adding blackout dates to baptism schedules", "~/Plugins/com_centralaz/Baptism/BaptismAddBlackoutDate.ascx", "com_centralaz > Baptism", "9DEC1094-FEC0-4E3A-B5C3-F08E2C296DB2" );
            RockMigrationHelper.UpdateBlockType( "Baptism Campus Detail Block", "Detail block for Baptism scheduling", "~/Plugins/com_centralaz/Baptism/BaptismCampusDetail.ascx", "com_centralaz > Baptism", "697B2414-42CE-4093-9546-DAF26E9B34CB" );
            
            // Add Block to Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlock( "F08B7F8F-5EC6-4CEC-8C20-5AD5EC5A3800", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Baptism Locations", "Main", "", "", 0, "B38DDC5A-F3D8-4E84-920F-C6465CF76667" );

            // Add Block to Page: Baptism Scheduler, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4", "", "697B2414-42CE-4093-9546-DAF26E9B34CB", "Baptism Campus Detail Block", "Main", "", "", 0, "94256870-5A3A-4BAE-A489-6BA9F94C0ED0" );

            // Add Block to Page: Add Baptism, Site: Rock RMS
            RockMigrationHelper.AddBlock( "AD8BE3C5-54F5-48F7-B699-01D53238CD23", "", "034AEBD6-FE85-4CC0-B060-25E93DCE20EB", "Baptism Add Baptism Block", "Main", "", "", 0, "65E56326-077D-461A-A6D6-7F2A6173AD0D" );

            // Add Block to Page: Add Blackout Date, Site: Rock RMS
            RockMigrationHelper.AddBlock( "A3882EAE-F086-467B-9F3C-DC0DB75403F7", "", "9DEC1094-FEC0-4E3A-B5C3-F08E2C296DB2", "Baptism Add Blackout Date Block", "Main", "", "", 0, "A96204AC-45E0-4F61-9635-A8C39924A920" );

            // Attrib for BlockType: Baptism Campus Detail Block:Add Baptism Page
            RockMigrationHelper.AddBlockTypeAttribute( "697B2414-42CE-4093-9546-DAF26E9B34CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Baptism Page", "AddBaptismPage", "", "", 0, @"", "2639B25C-6748-4973-B310-BC0F31A304D5" );

            // Attrib for BlockType: Baptism Campus Detail Block:Add Blackout Day Page
            RockMigrationHelper.AddBlockTypeAttribute( "697B2414-42CE-4093-9546-DAF26E9B34CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Blackout Day Page", "AddBlackoutDayPage", "", "", 0, @"", "CB638267-0065-4FFC-A665-BCE475BD022D" );

            // Attrib for BlockType: Baptism Add Baptism Block:Limit To Valid Service Times
            RockMigrationHelper.AddBlockTypeAttribute( "034AEBD6-FE85-4CC0-B060-25E93DCE20EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit To Valid Service Times", "LimitToValidServiceTimes", "", "", 0, @"False", "F35743FC-A145-4CEA-970C-72D412B540ED" );

            // Attrib for BlockType: Baptism Campus Detail Block:Report Font
            RockMigrationHelper.AddBlockTypeAttribute( "697B2414-42CE-4093-9546-DAF26E9B34CB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report Font", "ReportFont", "", "", 0, @"Gotham", "23982096-27F9-48F7-9568-525F575FDC6C" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Filter Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Include Group Types Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"32f8592c-ae11-44a7-a053-de43789811d9" );

            // Attrib Value for Block:Baptism Locations, Attribute:Exclude Group Types Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Group Type Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Description Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display System Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Active Status Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Limit to Security Role Groups Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Detail Page Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"b248d7e3-ad38-4e83-9e3c-3cc6d3814ab4" );

            // Attrib Value for Block:Baptism Campus Detail Block, Attribute:Add Baptism Page Page: Baptism Scheduler, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "94256870-5A3A-4BAE-A489-6BA9F94C0ED0", "2639B25C-6748-4973-B310-BC0F31A304D5", @"ad8be3c5-54f5-48f7-b699-01d53238cd23" );

            // Attrib Value for Block:Baptism Campus Detail Block, Attribute:Add Blackout Day Page Page: Baptism Scheduler, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "94256870-5A3A-4BAE-A489-6BA9F94C0ED0", "CB638267-0065-4FFC-A665-BCE475BD022D", @"a3882eae-f086-467b-9f3c-dc0db75403f7" );


        }

        public override void Down()
        {
            // Attrib for BlockType: Baptism Campus Detail Block:Report Font
            RockMigrationHelper.DeleteAttribute( "23982096-27F9-48F7-9568-525F575FDC6C" );
            // Attrib for BlockType: Baptism Add Baptism Block:Limit To Valid Service Times
            RockMigrationHelper.DeleteAttribute( "F35743FC-A145-4CEA-970C-72D412B540ED" );
            // Attrib for BlockType: Baptism Campus Detail Block:Add Blackout Day Page
            RockMigrationHelper.DeleteAttribute( "CB638267-0065-4FFC-A665-BCE475BD022D" );
            // Attrib for BlockType: Baptism Campus Detail Block:Add Baptism Page
            RockMigrationHelper.DeleteAttribute( "2639B25C-6748-4973-B310-BC0F31A304D5" );
            // Remove Block: Baptism Add Blackout Date Block, from Page: Add Blackout Date, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A96204AC-45E0-4F61-9635-A8C39924A920" );
            // Remove Block: Baptism Add Baptism Block, from Page: Add Baptism, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "65E56326-077D-461A-A6D6-7F2A6173AD0D" );
            // Remove Block: Baptism Campus Detail Block, from Page: Baptism Scheduler, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "94256870-5A3A-4BAE-A489-6BA9F94C0ED0" );
            // Remove Block: Baptism Locations, from Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B38DDC5A-F3D8-4E84-920F-C6465CF76667" );
            RockMigrationHelper.DeleteBlockType( "697B2414-42CE-4093-9546-DAF26E9B34CB" ); // Baptism Campus Detail Block
            RockMigrationHelper.DeleteBlockType( "9DEC1094-FEC0-4E3A-B5C3-F08E2C296DB2" ); // Baptism Add Blackout Date Block
            RockMigrationHelper.DeleteBlockType( "034AEBD6-FE85-4CC0-B060-25E93DCE20EB" ); // Baptism Add Baptism Block
            RockMigrationHelper.DeletePage( "A3882EAE-F086-467B-9F3C-DC0DB75403F7" ); //  Page: Add Blackout Date, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "AD8BE3C5-54F5-48F7-B699-01D53238CD23" ); //  Page: Add Baptism, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4" ); //  Page: Baptism Scheduler, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F08B7F8F-5EC6-4CEC-8C20-5AD5EC5A3800" ); //  Page: Baptisms, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeleteAttribute( "B7371337-57CB-4CB3-994C-72258729950F" );
            RockMigrationHelper.DeleteAttribute( "D58F0DB5-09AA-4A5A-BC17-CE3E3985D6F8" );
            RockMigrationHelper.DeleteGroupType( "32F8592C-AE11-44A7-A053-DE43789811D9" );

            RockMigrationHelper.DeleteCategory( "FFC06491-1BE9-4F3B-AC76-81A47E17D0AE" );
        }
    }
}
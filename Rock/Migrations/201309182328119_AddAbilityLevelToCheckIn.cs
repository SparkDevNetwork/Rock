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
    public partial class AddAbilityLevelToCheckIn : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Ability Level Select page...
            AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "Ability Select", "", "Checkin", "A1CBDAA4-94DD-4156-8260-5A3781E39FD0", "" );
            //AddBlockType( "Check In - Ability Level Select", "Check-in Ability Level Select block", "~/Blocks/CheckIn/AbilityLevelSelect.ascx", "605389F5-5BC5-438F-8757-110328B0CED3" );

            Sql( "UPDATE [Page] SET [Order]=5, SiteId=2 WHERE [Guid] = 'A1CBDAA4-94DD-4156-8260-5A3781E39FD0'" );

            // Add Block to Page: Ability Select
            AddBlock( "A1CBDAA4-94DD-4156-8260-5A3781E39FD0", "605389F5-5BC5-438F-8757-110328B0CED3", "Ability Level Select", "", "Content", 0, "C175A9ED-612E-4B25-BED4-CF713D922179" );

            // Add Block to Page: Ability Select
            AddBlock( "A1CBDAA4-94DD-4156-8260-5A3781E39FD0", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "9E480898-1623-4E6D-A393-B1A20B8CEE70" );

            // Attrib for BlockType: Check In - Ability Level Select:Previous Page
            AddBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "60572273-EC82-4F5E-9153-4ED79CAEFFE5" );

            // Attrib Value for Block:Ability Level Select, Attribute:Previous Page (Person Select)
            AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "60572273-EC82-4F5E-9153-4ED79CAEFFE5", "bb8cf87f-680f-48f9-9147-f4951e033d17" );

            // Attrib for BlockType: Check In - Ability Level Select:Workflow Type Id
            AddBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for Check-in", 0, "0", "1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E" );

            // Attrib Value for Block:Ability Level Select, Attribute:Workflow Type Id (Check in Workflow)
            AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E", "10" );

            // Attrib for BlockType: Check In - Ability Level Select:Home Page
            AddBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "5CC185C4-2ACC-4EDD-8073-9B54A638B225" );

            // Attrib Value for Block:Ability Level Select, Attribute:Home Page, (Welcome)
            AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "5CC185C4-2ACC-4EDD-8073-9B54A638B225", "432b615a-75ff-4b14-9c99-3e769f866950" );

            // Attrib for BlockType: Check In - Ability Level Select:Next Page
            AddBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "D5CA9580-D867-4539-9BC3-609F13D4CDE4" );

            // Attrib Value for Block:Ability Level Select, Attribute:Next Page (Group Type Select)
            AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "D5CA9580-D867-4539-9BC3-609F13D4CDE4", "60e3ea1f-fd6b-4f0e-9c72-a9960e13427c" );

            // Attrib Value for Block:Idle Redirect, Attribute:New Location (redirect to "welcome" block?)
            AddBlockAttributeValue( "9E480898-1623-4E6D-A393-B1A20B8CEE70", "C4204D6E-715E-4E3A-BA1B-949D20D26487", "welcome" );

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds
            AddBlockAttributeValue( "9E480898-1623-4E6D-A393-B1A20B8CEE70", "A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD", "20" );

            // now, inject Ability Level between the Person Select and the Group Search pages...

            // Attrib Value for Block:Person Select, Attribute:Next Page (Ability Select page)
            AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "F680429D-A228-43FE-A54E-927F95ACC030", "a1cbdaa4-94dd-4156-8260-5a3781e39fd0" );

            // Attrib Value for Block:Group Type Select, Attribute:Previous Page (Ability Select page)
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "39D260A5-A976-4DA9-B3E0-7381E9B8F3D5", "a1cbdaa4-94dd-4156-8260-5a3781e39fd0" );

            // Add attributes and values for the Block's Workflow Activity attribute:
            // Search block
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "EDC6A39C-211D-429A-BB1E-6156F16B4618" );
            AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "EDC6A39C-211D-429A-BB1E-6156F16B4618", "Family Search" );

            // Family Select block
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "B602450A-0C1F-401A-87BC-9A804461E887" );
            AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "B602450A-0C1F-401A-87BC-9A804461E887", "Person Search" );

            // Ability Level Select block
            AddBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "19E1825C-A722-470D-B8E6-2E96B250E39F" );
            AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "19E1825C-A722-470D-B8E6-2E96B250E39F", "Ability Level Search" );

            // Group Type Select block
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "11FB556B-3E88-4189-8E54-2B92E076F426" );
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "11FB556B-3E88-4189-8E54-2B92E076F426", "Group Search" );

            //  Location Select block
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "0E65FA3B-FB48-4DEC-B0F3-9394FBC21818" );
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "0E65FA3B-FB48-4DEC-B0F3-9394FBC21818", "Schedule Search" );

            // Group Select block
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "868173D9-B662-4899-87EB-1F560917C787" );
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "868173D9-B662-4899-87EB-1F560917C787", "Location Search" );

            // Time Select block
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the Workflow Activity to run upon selection.", 10, "", "12DF930E-6460-4A66-9326-E39BEAFC6F9D" );
            AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "12DF930E-6460-4A66-9326-E39BEAFC6F9D", "Save Attendance" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Workflow Activity attribute:
            DeleteAttribute( "EDC6A39C-211D-429A-BB1E-6156F16B4618" );
            DeleteAttribute( "B602450A-0C1F-401A-87BC-9A804461E887" );
            DeleteAttribute( "19E1825C-A722-470D-B8E6-2E96B250E39F" );
            DeleteAttribute( "11FB556B-3E88-4189-8E54-2B92E076F426" );
            DeleteAttribute( "0E65FA3B-FB48-4DEC-B0F3-9394FBC21818" );
            DeleteAttribute( "868173D9-B662-4899-87EB-1F560917C787" );
            DeleteAttribute( "12DF930E-6460-4A66-9326-E39BEAFC6F9D" );

            // Attribs for BlockType: Check In - Ability Level Select
            DeleteAttribute( "D5CA9580-D867-4539-9BC3-609F13D4CDE4" );
            DeleteAttribute( "5CC185C4-2ACC-4EDD-8073-9B54A638B225" );
            DeleteAttribute( "1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E" );
            DeleteAttribute( "60572273-EC82-4F5E-9153-4ED79CAEFFE5" );

            DeleteBlock( "9E480898-1623-4E6D-A393-B1A20B8CEE70" );
            DeleteBlock( "C175A9ED-612E-4B25-BED4-CF713D922179" );

            // Now switch the Attribute:Next Page of the 'Block:Person Select' back to the original (Group Type Select page)
            AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "F680429D-A228-43FE-A54E-927F95ACC030", "60E3EA1F-FD6B-4F0E-9C72-A9960E13427C" );

            // and switch the Attribute:Previous Page of the 'Block:Group Type Select' back to the original (Person Select page)
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "39D260A5-A976-4DA9-B3E0-7381E9B8F3D5", "bb8cf87f-680f-48f9-9147-f4951e033d17" );

            DeletePage( "A1CBDAA4-94DD-4156-8260-5A3781E39FD0" );
        }
    }
}

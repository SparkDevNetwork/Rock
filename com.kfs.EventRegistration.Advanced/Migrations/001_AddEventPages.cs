using Rock.Plugin;

namespace com.kfs.EventRegistration.Advanced.Migrations
{
    [MigrationNumber( 1, "1.6.9" )]
    public class AddEventPages : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Advanced Events", "", "D2E89042-3834-44E3-AA29-F1A30CC0D00B", "fa fa-clipboard" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "D2E89042-3834-44E3-AA29-F1A30CC0D00B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Registration Instance", "", "8638EEC0-CC28-43D6-B752-C96E4D99132A", "fa fa-file-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "8638EEC0-CC28-43D6-B752-C96E4D99132A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Registration Detail", "", "1DF0A5E6-D3ED-4127-97EF-8BC99397D4D4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "1DF0A5E6-D3ED-4127-97EF-8BC99397D4D4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Event Registrant", "", "B62C91D4-1C91-4D07-8B6D-43A520E5483A", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "1DF0A5E6-D3ED-4127-97EF-8BC99397D4D4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Event Payment", "", "F482EA05-4BFB-417B-AB6F-A80680E07B97", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "1DF0A5E6-D3ED-4127-97EF-8BC99397D4D4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Event Audit Log", "", "6C0F181C-1C6B-4588-98E3-B050CCDE7DF5", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "8638EEC0-CC28-43D6-B752-C96E4D99132A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Linkage Detail", "", "A90A5F60-96CC-4415-8C36-2318BEA6D981", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "8638EEC0-CC28-43D6-B752-C96E4D99132A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Advanced Payment Reminders", "", "239B33A6-7C22-4B45-8CE2-1419ED876C2A", "fa fa-bell-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "8638EEC0-CC28-43D6-B752-C96E4D99132A", "7CFA101B-2D20-4523-9EC5-3F30502797A5", "Advanced Registration Group", "", "D04B0912-F29E-4198-8F13-29284FEA63B7", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Advanced Registration Instance Detail", "Template block for editing an event registration instance.", "~/Plugins/com_kfs/Event/RegistrationInstanceDetail.ascx", "KFS > Advanced Event Registration", "1242F1EE-C333-4903-9937-E330236A77DB" );
            RockMigrationHelper.UpdateBlockType( "Advanced Registration Group Detail", "Displays the details of the given group.", "~/Plugins/com_kfs/Event/GroupDetail.ascx", "KFS > Advanced Event Registration", "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA" );

            // Add Block to Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2E89042-3834-44E3-AA29-F1A30CC0D00B", "", "ADE003C7-649B-466A-872B-B8AC952E7841", "Category Tree View", "Sidebar1", "", "", 0, "52F47E1D-2EC6-4A21-A497-E48EF0822B67" );
            // Add Block to Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlock( "8638EEC0-CC28-43D6-B752-C96E4D99132A", "", "1242F1EE-C333-4903-9937-E330236A77DB", "Advanced Registration Instance Detail", "Main", "", "", 0, "4FFF1227-2176-4430-BE5C-B552190CF7C0" );
            // Add Block to Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D04B0912-F29E-4198-8F13-29284FEA63B7", "", "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "Group Detail", "Main", "", "", 0, "C3A48375-99E4-44AD-887B-658AF9589C04" );
            // Add Block to Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2E89042-3834-44E3-AA29-F1A30CC0D00B", "", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "Main", "", "", 0, "D16A806E-EE8A-4940-BE84-6B6224060E23" );
            // Add Block to Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2E89042-3834-44E3-AA29-F1A30CC0D00B", "", "91354899-304E-44C7-BD0D-55F42E6505D3", "Registration Template Detail", "Main", "", "", 1, "4726275D-295C-4ECB-9BF6-D564FEC0BBCF" );
            // Add Block to Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2E89042-3834-44E3-AA29-F1A30CC0D00B", "", "632F63A9-5629-4731-BE6A-AB534EDD9BC9", "Registration Instance List", "Main", "", "", 2, "9D8FF32B-30F2-41A0-A9E7-5A2DEFFD1379" );
            // Add Block to Page: Advanced Linkage Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "A90A5F60-96CC-4415-8C36-2318BEA6D981", "", "D341EF12-406B-477D-8A85-16EBDDF2B04B", "Registration Instance Linkage Detail", "Main", "", "", 0, "2ED8A211-E2C2-45B9-9C9A-5CF7972792EA" );
            // Add Block to Page: Advanced Payment Reminders, Site: Rock RMS
            RockMigrationHelper.AddBlock( "239B33A6-7C22-4B45-8CE2-1419ED876C2A", "", "ED56CD0A-0A8D-4758-A689-55B7BEC1B589", "Registration Instance Send Payment Reminder", "Main", "", "", 0, "E3D6B6A1-8302-40EC-911E-39F98D644A82" );
            // Add Block to Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1DF0A5E6-D3ED-4127-97EF-8BC99397D4D4", "", "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "Registration Detail", "Main", "", "", 0, "D1615F3D-0062-490D-9BA5-820EEBB166DF" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D16A806E-EE8A-4940-BE84-6B6224060E23'" );  // Page: Advanced Events,  Zone: Main,  Block: Category Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D16A806E-EE8A-4940-BE84-6B6224060E23'" );  // Page: Advanced Events,  Zone: Main,  Block: Category Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D16A806E-EE8A-4940-BE84-6B6224060E23'" );  // Page: Advanced Events,  Zone: Main,  Block: Category Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '4726275D-295C-4ECB-9BF6-D564FEC0BBCF'" );  // Page: Advanced Events,  Zone: Main,  Block: Registration Template Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '4726275D-295C-4ECB-9BF6-D564FEC0BBCF'" );  // Page: Advanced Events,  Zone: Main,  Block: Registration Template Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '4726275D-295C-4ECB-9BF6-D564FEC0BBCF'" );  // Page: Advanced Events,  Zone: Main,  Block: Registration Template Detail
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '9D8FF32B-30F2-41A0-A9E7-5A2DEFFD1379'" );  // Page: Advanced Events,  Zone: Main,  Block: Registration Instance List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '9D8FF32B-30F2-41A0-A9E7-5A2DEFFD1379'" );  // Page: Advanced Events,  Zone: Main,  Block: Registration Instance List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '9D8FF32B-30F2-41A0-A9E7-5A2DEFFD1379'" );  // Page: Advanced Events,  Zone: Main,  Block: Registration Instance List

            // Attrib for BlockType: Advanced Registration Instance Detail:Default Account
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Default Account", "DefaultAccount", "", "The default account to use for new registration instances", 0, @"2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5", "AA2605C0-7AA5-464E-A4FD-19AC043A10F7" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Registration Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", "The page for editing registration and registrant information", 1, @"", "D6A9EE42-4CD2-44FF-A877-9ED8B26DD931" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Content Item Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemPage", "", "The page for viewing details about a content channel item", 6, @"", "78A25D4A-8EB3-4740-8525-7246BC766608" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Transaction Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "TransactionDetailPage", "", "The page for viewing details about a payment", 7, @"", "19729BE2-F277-4C5E-B354-9DAC9FD0CC9D" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Calendar Item Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Item Page", "CalendarItemPage", "", "The page to view calendar item details", 3, @"", "C5AAA33F-D455-4C8E-A8B7-A5BAB63A4F3B" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Linkage Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Linkage Page", "LinkagePage", "", "The page for editing registration linkages", 2, @"", "62B9F5FC-90AD-4E37-A254-747A4B77EE32" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Display Discount Codes
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Discount Codes", "DisplayDiscountCodes", "", "Display the discount code used with a payment", 9, @"False", "16607F5C-0DF8-478D-A444-69B9128EC822" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page for viewing details about a group", 4, @"", "998CE2BD-E76B-4C09-ACAF-BFCEFAF8A1D5" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Payment Reminder Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Payment Reminder Page", "PaymentReminderPage", "", "The page for manually sending payment reminders.", 8, @"", "951A2E6D-4FD5-4E3D-B5BD-B64D8F948576" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Group Modal Page
            RockMigrationHelper.AddBlockTypeAttribute( "1242F1EE-C333-4903-9937-E330236A77DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Modal Page", "GroupModalPage", "", "The modal page to view and edit details for a group", 5, @"", "630A3DFE-D24D-4BFC-ADE8-10617371B2BE" );
            // Attrib for BlockType: Advanced Registration Group Detail:Registration Instance Page
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "", "The page to display registration details.", 7, @"", "FB6D93FE-FA19-4E6D-B340-1077733A3EF9" );
            // Attrib for BlockType: Advanced Registration Group Detail:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The style of maps to use", 5, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "F0F6B577-BC2A-422E-9020-27F08DA13030" );
            // Attrib for BlockType: Advanced Registration Group Detail:Group Map Page
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Map Page", "GroupMapPage", "", "The page to display detailed group map.", 6, @"", "333C2F91-2AD1-4D27-9DDD-DEA2227105A9" );
            // Attrib for BlockType: Advanced Registration Group Detail:Attendance Page
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Page", "AttendancePage", "", "The page to display attendance list.", 7, @"", "63DA6018-AE9E-4634-954B-BC9F78CFA1FC" );
            // Attrib for BlockType: Advanced Registration Group Detail:Event Item Occurrence Page
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Item Occurrence Page", "EventItemOccurrencePage", "", "The page to display event item occurrence details.", 8, @"", "B9BBE433-90DC-4209-8D57-E28E5E3195B3" );
            // Attrib for BlockType: Advanced Registration Group Detail:Content Item Page
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemPage", "", "The page to display registration details.", 9, @"", "CC2C1D0B-C4B1-40F3-B5F2-AA30360DC12C" );
            // Attrib for BlockType: Advanced Registration Group Detail:Enable Dialog Mode
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Dialog Mode", "EnableDialogMode", "", "When enabled, the Save and Cancel buttons will be associated with the buttons of the Modal.", 13, @"False", "E17CC768-D064-4F1B-AF8A-EC2E1DF2293D" );
            // Attrib for BlockType: Advanced Registration Group Detail:Limit to Group Types that are shown in navigation
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Group Types that are shown in navigation", "LimitToShowInNavigationGroupTypes", "", "", 4, @"False", "F368ACB6-2D3A-4CFA-A79B-5534C8BB921C" );
            // Attrib for BlockType: Advanced Registration Group Detail:Group Types Include
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types Include", "GroupTypes", "", "Select group types to show in this block.  Leave all unchecked to show all but the excluded group types.", 0, @"", "1D0D7EC6-A3FB-426B-9FB0-737A761D445E" );
            // Attrib for BlockType: Advanced Registration Group Detail:Group Types Exclude
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types Exclude", "GroupTypesExclude", "", "Select group types to exclude from this block.", 1, @"", "462B90FE-ECBB-4875-9AE8-5B36306AAE42" );
            // Attrib for BlockType: Advanced Registration Group Detail:Show Edit
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Edit", "ShowEdit", "", "", 2, @"True", "B6FAFC7A-84EE-4C90-BA9B-4F75EB512587" );
            // Attrib for BlockType: Advanced Registration Group Detail:Limit to Security Role Groups
            RockMigrationHelper.AddBlockTypeAttribute( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "", 3, @"False", "8AE46157-C5DC-4AD1-A128-77E7906707CC" );
            // Attrib Value for Block:Category Tree View, Attribute:Page Parameter Key Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "AA057D3E-00CC-42BD-9998-600873356EDB", @"RegistrationTemplateId" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"a01e3e99-a8ad-4c6c-baac-98795738ba70" );
            // Attrib Value for Block:Category Tree View, Attribute:Detail Page Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"d2e89042-3834-44e3-aa29-f1a30cc0d00b" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Qualifier Property Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "2D5FF74A-D316-4924-BCD2-6AA338D8DAAC", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity type Qualifier Value Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "F76C5EEF-FD45-4BD6-A903-ED5AB53BB928", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Friendly Name Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "07213E2C-C239-47CA-A781-F7A908756DC2", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Show Unnamed Entity Items Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"True" );
            // Attrib Value for Block:Category Tree View, Attribute:Exclude Categories Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "61398707-FCCE-4AFD-8374-110BCA275F34", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Root Category Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "2080E00B-63E4-4874-B06A-ADE1B4F3B3AD", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Default Icon CSS Class Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52F47E1D-2EC6-4A21-A497-E48EF0822B67", "D2596ADF-4455-42A4-848F-6DFD816C2867", @"fa fa-list-ol" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Default Account Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "AA2605C0-7AA5-464E-A4FD-19AC043A10F7", @"2a6f9e5f-6859-44f1-ab0e-ce9cf6b08ee5" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Registration Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "D6A9EE42-4CD2-44FF-A877-9ED8B26DD931", @"1df0a5e6-d3ed-4127-97ef-8bc99397d4d4" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Content Item Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "78A25D4A-8EB3-4740-8525-7246BC766608", @"d18e837c-9e65-4a38-8647-dff04a595d97" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Transaction Detail Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "19729BE2-F277-4C5E-B354-9DAC9FD0CC9D", @"b67e38cb-2ef1-43ea-863a-37daa1c7340f" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Calendar Item Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "C5AAA33F-D455-4C8E-A8B7-A5BAB63A4F3B", @"7fb33834-f40a-4221-8849-bb8c06903b04" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Linkage Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "62B9F5FC-90AD-4E37-A254-747A4B77EE32", @"a90a5f60-96cc-4415-8c36-2318bea6d981" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Display Discount Codes Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "16607F5C-0DF8-478D-A444-69B9128EC822", @"False" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Group Detail Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "998CE2BD-E76B-4C09-ACAF-BFCEFAF8A1D5", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Payment Reminder Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "951A2E6D-4FD5-4E3D-B5BD-B64D8F948576", @"239b33a6-7c22-4b45-8ce2-1419ed876c2a" );
            // Attrib Value for Block:Advanced Registration Instance Detail, Attribute:Group Modal Page Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FFF1227-2176-4430-BE5C-B552190CF7C0", "630A3DFE-D24D-4BFC-ADE8-10617371B2BE", @"d04b0912-f29e-4198-8f13-29284fea63b7" );
            // Attrib Value for Block:Group Detail, Attribute:Map Style Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "F0F6B577-BC2A-422E-9020-27F08DA13030", @"fdc5d6ba-a818-4a06-96b1-9ef31b4087ac" );
            // Attrib Value for Block:Group Detail, Attribute:Group Map Page Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "333C2F91-2AD1-4D27-9DDD-DEA2227105A9", @"60995c8c-862f-40f5-afbb-13b49cda77eb" );
            // Attrib Value for Block:Group Detail, Attribute:Enable Dialog Mode Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "E17CC768-D064-4F1B-AF8A-EC2E1DF2293D", @"True" );
            // Attrib Value for Block:Group Detail, Attribute:Limit to Group Types that are shown in navigation Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "F368ACB6-2D3A-4CFA-A79B-5534C8BB921C", @"False" );
            // Attrib Value for Block:Group Detail, Attribute:Group Types Include Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "1D0D7EC6-A3FB-426B-9FB0-737A761D445E", @"" );
            // Attrib Value for Block:Group Detail, Attribute:Group Types Exclude Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "462B90FE-ECBB-4875-9AE8-5B36306AAE42", @"" );
            // Attrib Value for Block:Group Detail, Attribute:Show Edit Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "B6FAFC7A-84EE-4C90-BA9B-4F75EB512587", @"True" );
            // Attrib Value for Block:Group Detail, Attribute:Limit to Security Role Groups Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C3A48375-99E4-44AD-887B-658AF9589C04", "8AE46157-C5DC-4AD1-A128-77E7906707CC", @"False" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "84ED8FE5-94E5-479E-B1A4-433591B22387" );
            // Attrib for BlockType: Page Properties:Enable Full Edit Mode
            RockMigrationHelper.AddBlockTypeAttribute( "C7988C3E-822D-4E73-882E-9B7684398BAA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Full Edit Mode", "EnableFullEditMode", "", "Have the block initially show a readonly summary view, in a panel, with Edit and Delete buttons. Also include Save and Cancel buttons.", 0, @"False", "8DECFD06-9424-438F-B096-B494663071A7" );
            // Attrib Value for Block:Category Detail, Attribute:Entity Type Qualifier Property Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D16A806E-EE8A-4940-BE84-6B6224060E23", "620957FF-BC28-4A89-A74F-C917DA5CFD47", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Entity Type Qualifier Value Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D16A806E-EE8A-4940-BE84-6B6224060E23", "985128EE-D40C-4598-B14B-7AD728ECCE38", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Entity Type Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D16A806E-EE8A-4940-BE84-6B6224060E23", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", @"a01e3e99-a8ad-4c6c-baac-98795738ba70" );
            // Attrib Value for Block:Category Detail, Attribute:Exclude Categories Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D16A806E-EE8A-4940-BE84-6B6224060E23", "AB542995-876F-4B8F-8417-11D83369289E", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Root Category Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D16A806E-EE8A-4940-BE84-6B6224060E23", "C3B72ADF-93D7-42CF-A103-8A7661A6926B", @"" );
            // Attrib Value for Block:Registration Instance List, Attribute:Detail Page Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9D8FF32B-30F2-41A0-A9E7-5A2DEFFD1379", "E4886210-4889-46A2-9667-DD0D9F340ADF", @"8638eec0-cc28-43d6-b752-c96e4d99132a" );
            // Attrib Value for Block:Registration Detail, Attribute:Batch Name Prefix Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "03770487-DC57-4DEE-A0DB-F8A5B22F96C2", @"Event Registration" );
            // Attrib Value for Block:Registration Detail, Attribute:Transaction Page Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "352DE424-2299-404E-B86F-FEA46D588D2D", @"f482ea05-4bfb-417b-ab6f-a80680e07b97" );
            // Attrib Value for Block:Registration Detail, Attribute:Registrant Page Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "DA8437F4-B8A6-4C52-9111-8ADD868E0392", @"b62c91d4-1c91-4d07-8b6d-43a520e5483a" );
            // Attrib Value for Block:Registration Detail, Attribute:Group Detail Page Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "B936B357-776B-438F-B6BE-06CB26AFDFB4", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Registration Detail, Attribute:Group Member Page Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "C19CCA3D-48E8-4CF3-BADE-C509CC3C3434", @"3905c63f-4d57-40f0-9721-c60a2f681911" );
            // Attrib Value for Block:Registration Detail, Attribute:Source Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "429649E9-B86D-4C15-B392-0A87D3AFC31A", @"be7ecf50-52bc-4774-808d-574ba842db98" );
            // Attrib Value for Block:Registration Detail, Attribute:Transaction Detail Page Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "9E53BDB7-BD53-4335-A862-20A3454C0CCF", @"b67e38cb-2ef1-43ea-863a-37daa1c7340f" );
            // Attrib Value for Block:Registration Detail, Attribute:Audit Page Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D1615F3D-0062-490D-9BA5-820EEBB166DF", "DAE22517-2B39-4CB5-9687-5EB4F4104628", @"6c0f181c-1c6b-4588-98e3-b050ccde7df5" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Page Properties:Enable Full Edit Mode
            RockMigrationHelper.DeleteAttribute( "8DECFD06-9424-438F-B096-B494663071A7" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "84ED8FE5-94E5-479E-B1A4-433591B22387" );
            // Attrib for BlockType: Advanced Registration Group Detail:Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute( "8AE46157-C5DC-4AD1-A128-77E7906707CC" );
            // Attrib for BlockType: Advanced Registration Group Detail:Show Edit
            RockMigrationHelper.DeleteAttribute( "B6FAFC7A-84EE-4C90-BA9B-4F75EB512587" );
            // Attrib for BlockType: Advanced Registration Group Detail:Group Types Exclude
            RockMigrationHelper.DeleteAttribute( "462B90FE-ECBB-4875-9AE8-5B36306AAE42" );
            // Attrib for BlockType: Advanced Registration Group Detail:Group Types Include
            RockMigrationHelper.DeleteAttribute( "1D0D7EC6-A3FB-426B-9FB0-737A761D445E" );
            // Attrib for BlockType: Advanced Registration Group Detail:Limit to Group Types that are shown in navigation
            RockMigrationHelper.DeleteAttribute( "F368ACB6-2D3A-4CFA-A79B-5534C8BB921C" );
            // Attrib for BlockType: Advanced Registration Group Detail:Enable Dialog Mode
            RockMigrationHelper.DeleteAttribute( "E17CC768-D064-4F1B-AF8A-EC2E1DF2293D" );
            // Attrib for BlockType: Advanced Registration Group Detail:Content Item Page
            RockMigrationHelper.DeleteAttribute( "CC2C1D0B-C4B1-40F3-B5F2-AA30360DC12C" );
            // Attrib for BlockType: Advanced Registration Group Detail:Event Item Occurrence Page
            RockMigrationHelper.DeleteAttribute( "B9BBE433-90DC-4209-8D57-E28E5E3195B3" );
            // Attrib for BlockType: Advanced Registration Group Detail:Attendance Page
            RockMigrationHelper.DeleteAttribute( "63DA6018-AE9E-4634-954B-BC9F78CFA1FC" );
            // Attrib for BlockType: Advanced Registration Group Detail:Group Map Page
            RockMigrationHelper.DeleteAttribute( "333C2F91-2AD1-4D27-9DDD-DEA2227105A9" );
            // Attrib for BlockType: Advanced Registration Group Detail:Map Style
            RockMigrationHelper.DeleteAttribute( "F0F6B577-BC2A-422E-9020-27F08DA13030" );
            // Attrib for BlockType: Advanced Registration Group Detail:Registration Instance Page
            RockMigrationHelper.DeleteAttribute( "FB6D93FE-FA19-4E6D-B340-1077733A3EF9" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Group Modal Page
            RockMigrationHelper.DeleteAttribute( "630A3DFE-D24D-4BFC-ADE8-10617371B2BE" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Payment Reminder Page
            RockMigrationHelper.DeleteAttribute( "951A2E6D-4FD5-4E3D-B5BD-B64D8F948576" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "998CE2BD-E76B-4C09-ACAF-BFCEFAF8A1D5" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Display Discount Codes
            RockMigrationHelper.DeleteAttribute( "16607F5C-0DF8-478D-A444-69B9128EC822" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Linkage Page
            RockMigrationHelper.DeleteAttribute( "62B9F5FC-90AD-4E37-A254-747A4B77EE32" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Calendar Item Page
            RockMigrationHelper.DeleteAttribute( "C5AAA33F-D455-4C8E-A8B7-A5BAB63A4F3B" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Transaction Detail Page
            RockMigrationHelper.DeleteAttribute( "19729BE2-F277-4C5E-B354-9DAC9FD0CC9D" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Content Item Page
            RockMigrationHelper.DeleteAttribute( "78A25D4A-8EB3-4740-8525-7246BC766608" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Registration Page
            RockMigrationHelper.DeleteAttribute( "D6A9EE42-4CD2-44FF-A877-9ED8B26DD931" );
            // Attrib for BlockType: Advanced Registration Instance Detail:Default Account
            RockMigrationHelper.DeleteAttribute( "AA2605C0-7AA5-464E-A4FD-19AC043A10F7" );

            // Remove Block: Registration Detail, from Page: Advanced Registration Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D1615F3D-0062-490D-9BA5-820EEBB166DF" );
            // Remove Block: Registration Instance Send Payment Reminder, from Page: Advanced Payment Reminders, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E3D6B6A1-8302-40EC-911E-39F98D644A82" );
            // Remove Block: Registration Instance Linkage Detail, from Page: Advanced Linkage Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2ED8A211-E2C2-45B9-9C9A-5CF7972792EA" );
            // Remove Block: Registration Instance List, from Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9D8FF32B-30F2-41A0-A9E7-5A2DEFFD1379" );
            // Remove Block: Registration Template Detail, from Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4726275D-295C-4ECB-9BF6-D564FEC0BBCF" );
            // Remove Block: Category Detail, from Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D16A806E-EE8A-4940-BE84-6B6224060E23" );
            // Remove Block: Group Detail, from Page: Advanced Registration Group, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C3A48375-99E4-44AD-887B-658AF9589C04" );
            // Remove Block: Advanced Registration Instance Detail, from Page: Advanced Registration Instance, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4FFF1227-2176-4430-BE5C-B552190CF7C0" );
            // Remove Block: Category Tree View, from Page: Advanced Events, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "52F47E1D-2EC6-4A21-A497-E48EF0822B67" );

            RockMigrationHelper.DeleteBlockType( "F73B6981-0EAB-4F13-90E1-EAF7E39B73CA" ); // Advanced Registration Group Detail
            RockMigrationHelper.DeleteBlockType( "1242F1EE-C333-4903-9937-E330236A77DB" ); // Advanced Registration Instance Detail
            RockMigrationHelper.DeletePage( "D04B0912-F29E-4198-8F13-29284FEA63B7" ); //  Page: Advanced Registration Group, Layout: Dialog, Site: Rock RMS
            RockMigrationHelper.DeletePage( "239B33A6-7C22-4B45-8CE2-1419ED876C2A" ); //  Page: Advanced Payment Reminders, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "A90A5F60-96CC-4415-8C36-2318BEA6D981" ); //  Page: Advanced Linkage Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6C0F181C-1C6B-4588-98E3-B050CCDE7DF5" ); //  Page: Advanced Event Audit Log, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F482EA05-4BFB-417B-AB6F-A80680E07B97" ); //  Page: Advanced Event Payment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "B62C91D4-1C91-4D07-8B6D-43A520E5483A" ); //  Page: Advanced Event Registrant, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1DF0A5E6-D3ED-4127-97EF-8BC99397D4D4" ); //  Page: Advanced Registration Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "8638EEC0-CC28-43D6-B752-C96E4D99132A" ); //  Page: Advanced Registration Instance, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "D2E89042-3834-44E3-AA29-F1A30CC0D00B" ); //  Page: Advanced Events, Layout: Left Sidebar, Site: Rock RMS
        }
    }
}

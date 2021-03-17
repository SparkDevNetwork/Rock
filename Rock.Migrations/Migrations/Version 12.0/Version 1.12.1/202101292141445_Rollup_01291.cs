// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_01291 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateFamilyAnalyticsStoredProcedures();
            AddEventRegistrationMatchingUp();
            UpdateConnectionOpportunityUp();
            CommunicationWizardPolish();
            MassPushNotificationsUp();
            InteractionIndexPersonalDeviceIdUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            AddEventRegistrationMatchingDown();
            MassPushNotificationsDown();
        }

                /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Add/Update BlockType Mass Push Notifications
            RockMigrationHelper.UpdateBlockType("Mass Push Notifications","Used for creating and sending a mass push notification to recipients.","~/Blocks/Communication/MassPushNotifications.ascx","Communication","238F0FE3-E7DD-44DA-B09C-DBF3C01312AF");

            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "403C6DAA-AF5B-4121-BC68-2F97FE8902BF");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "DA440EBC-6F1F-4E9A-A1A0-C652CC1C241E");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "D3CE5611-87D5-4D8F-80D0-81EF8393ACEE");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "D43ABA56-6D1E-45DE-8164-0BB99E06589E");

            // Attribute for BlockType: Email Preference Entry:Allow Inactivating Family
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Inactivating Family", "AllowInactivatingFamily", "Allow Inactivating Family", @"If the person chooses the 'Not Involved' choice show the option of inactivating the whole family. This will not show if the person is a member of more than one family or is not an adult.", 11, @"True", "1EAE76B7-B404-4D2D-98F3-499B986BAE8B" );

            // Attribute for BlockType: Select Check-In Area:Check-in Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17E8F764-562A-4E94-980D-FF1B15640670", "7522975C-C224-489A-985D-B44580DFC5BD", "Check-in Areas", "CheckinConfigurationTypes", "Check-in Areas", @"Select the Check Areas to display, or select none to show all.", 3, @"", "8C3D6EF6-2F9F-4D25-8AE4-51109CDE5028" );

            // Attribute for BlockType: Person Profile :Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Profile Page", "PersonProfilePage", "Profile Page", @"The Page to go to when a family member of the attendee is clicked.", 6, @"F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3", "287CA8C4-5DA8-4AB8-9ADA-F712AE8A3AF8" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA440EBC-6F1F-4E9A-A1A0-C652CC1C241E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "6E86C431-CACB-42D0-9E90-7A68490DF0B5" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA440EBC-6F1F-4E9A-A1A0-C652CC1C241E", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "21DD4DA9-08A5-4821-AAFF-35605B694412" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D3CE5611-87D5-4D8F-80D0-81EF8393ACEE", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "9EF1598B-34EC-4516-9EAF-DE4C9627F160" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D3CE5611-87D5-4D8F-80D0-81EF8393ACEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "5FBE2010-AC0A-4493-B443-8C407D71C670" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "396F858B-5BFB-4FBC-8360-6895929005F8" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "DC5B107A-98ED-4D9C-A910-8E01EFD98B39" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "AF847A0D-FFC4-4828-848E-913E7282F8D4" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "FDCC8F0F-CEDF-47E3-82CA-5A0C6F106994" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "F2FB9171-57EA-4257-8553-6E9FEF0C9012" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "1C09772B-3B37-4BF6-9F0E-5420BB3F1A5D" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "91B1DD2E-2BDA-455A-8533-7D121480F3DB" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "290BE10A-DCF5-44F3-AF4B-D17287FB0714" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "343312AF-079B-4967-80A3-7D07E4BD202D" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D43ABA56-6D1E-45DE-8164-0BB99E06589E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "11F13721-B675-43EE-B6D5-654D7FF13BB1" );

            // Attribute for BlockType: Mass Push Notifications:Personal Device Active Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "238F0FE3-E7DD-44DA-B09C-DBF3C01312AF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Personal Device Active Duration", "PersonalDeviceActiveDuration", "Personal Device Active Duration", @"The number of days that the device must have an interaction in order for it to be considered an active device.", 0, @"365", "CEB35693-E0B6-4FBD-A0E0-E60D924EA12B" );

            // Attribute for BlockType: Mass Push Notifications:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "238F0FE3-E7DD-44DA-B09C-DBF3C01312AF", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 1, @"", "D4C670B1-085E-4865-8FCD-0662FBD4DB17" );

            // Attribute for BlockType: Mass Push Notifications:Send Immediately
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "238F0FE3-E7DD-44DA-B09C-DBF3C01312AF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send Immediately", "SendImmediately", "Send Immediately", @"Should communication be sent right away (vs. just being queued for scheduled job to send)?", 2, @"True", "9238B846-EA4C-4259-8B74-15947456B9A0" );
            RockMigrationHelper.UpdateFieldType("Checkin Configuration Type","","Rock","Rock.Field.Types.CheckinConfigurationTypeFieldType","7522975C-C224-489A-985D-B44580DFC5BD");

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Send Immediately Attribute for BlockType: Mass Push Notifications
            RockMigrationHelper.DeleteAttribute("9238B846-EA4C-4259-8B74-15947456B9A0");

            // Enabled Lava Commands Attribute for BlockType: Mass Push Notifications
            RockMigrationHelper.DeleteAttribute("D4C670B1-085E-4865-8FCD-0662FBD4DB17");

            // Personal Device Active Duration Attribute for BlockType: Mass Push Notifications
            RockMigrationHelper.DeleteAttribute("CEB35693-E0B6-4FBD-A0E0-E60D924EA12B");

            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("343312AF-079B-4967-80A3-7D07E4BD202D");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("290BE10A-DCF5-44F3-AF4B-D17287FB0714");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("AF847A0D-FFC4-4828-848E-913E7282F8D4");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("11F13721-B675-43EE-B6D5-654D7FF13BB1");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("1C09772B-3B37-4BF6-9F0E-5420BB3F1A5D");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("DC5B107A-98ED-4D9C-A910-8E01EFD98B39");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("F2FB9171-57EA-4257-8553-6E9FEF0C9012");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("91B1DD2E-2BDA-455A-8533-7D121480F3DB");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("FDCC8F0F-CEDF-47E3-82CA-5A0C6F106994");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("396F858B-5BFB-4FBC-8360-6895929005F8");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("9EF1598B-34EC-4516-9EAF-DE4C9627F160");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("5FBE2010-AC0A-4493-B443-8C407D71C670");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("21DD4DA9-08A5-4821-AAFF-35605B694412");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("6E86C431-CACB-42D0-9E90-7A68490DF0B5");

            // Allow Inactivating Family Attribute for BlockType: Email Preference Entry
            RockMigrationHelper.DeleteAttribute("1EAE76B7-B404-4D2D-98F3-499B986BAE8B");

            // Profile Page Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute("287CA8C4-5DA8-4AB8-9ADA-F712AE8A3AF8");

            // Check-in Areas Attribute for BlockType: Select Check-In Area
            RockMigrationHelper.DeleteAttribute("8C3D6EF6-2F9F-4D25-8AE4-51109CDE5028");

            // Delete BlockType Mass Push Notifications
            RockMigrationHelper.DeleteBlockType("238F0FE3-E7DD-44DA-B09C-DBF3C01312AF"); // Mass Push Notifications

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("D43ABA56-6D1E-45DE-8164-0BB99E06589E"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("D3CE5611-87D5-4D8F-80D0-81EF8393ACEE"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("DA440EBC-6F1F-4E9A-A1A0-C652CC1C241E"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("403C6DAA-AF5B-4121-BC68-2F97FE8902BF"); // Structured Content View

        }

        /// <summary>
        /// MB: Update Family Analytics Stored procedures
        /// </summary>
        private void UpdateFamilyAnalyticsStoredProcedures()
        {
            Sql( MigrationSQL._202101292141445_Rollup_01291_spCrm_FamilyAnalyticsAttendance );
            Sql( MigrationSQL._202101292141445_Rollup_01291_spCrm_FamilyAnalyticsGiving );
        }

        /// <summary>
        /// SK: Add "Event Registration Matching" page (w/block) under the existing "Fundraising Matching" page
        /// </summary>
        private void AddEventRegistrationMatchingUp()
        {
            // Add Page Event Registration Matching to Site:Rock RMS     
            RockMigrationHelper.AddPage( true, "142627AE-6590-48E3-BFCA-3669260B8CF2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Event Registration Matching", "", "507F7AC2-75A2-49AA-9EE4-F6DFCD34A3DC", "" );
            // Add/Update BlockType Event Registration Matching 
            RockMigrationHelper.UpdateBlockType( "Event Registration Matching", "Used to assign a Registration to a Transaction Detail record", "~/Blocks/Finance/EventRegistrationMatching.ascx", "Finance", "7651F50F-3E32-4437-B71A-FED1855098AD" );
            // Add Block Event Registration Matching to Page: Event Registration Matching, Site: Rock RMS        
            RockMigrationHelper.AddBlock( true, "507F7AC2-75A2-49AA-9EE4-F6DFCD34A3DC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7651F50F-3E32-4437-B71A-FED1855098AD".AsGuid(), "Event Registration Matching", "Main", @"", @"", 0, "95A1DD2E-B599-4124-9047-C6A357F98923" );
        }

        /// <summary>
        /// SK: Add "Event Registration Matching" page (w/block) under the existing "Fundraising Matching" page
        /// </summary>
        private void AddEventRegistrationMatchingDown()
        {
            // Remove Block: Event Registration Matching, from Page: Event Registration Matching, Site: Rock RMS    
            RockMigrationHelper.DeleteBlock( "95A1DD2E-B599-4124-9047-C6A357F98923" );
            // Delete BlockType Event Registration Matching              
            RockMigrationHelper.DeleteBlockType( "7651F50F-3E32-4437-B71A-FED1855098AD" ); // Event Registration Matching  
            // Delete Page Event Registration Matching from Site:Rock RMS      
            RockMigrationHelper.DeletePage( "507F7AC2-75A2-49AA-9EE4-F6DFCD34A3DC" ); //  Page: Event Registration Matching, Layout: Full Width, Site: Rock RMS  
        }

        /// <summary>
        /// SK: Updated Connection Opportunity  ShowStatusOnTransfer and ShowConnectButton 
        /// </summary>
        private void UpdateConnectionOpportunityUp()
        {
            Sql( @"UPDATE [ConnectionOpportunity] SET [ShowStatusOnTransfer] = 1, [ShowConnectButton] = 1" );
        }

        /// <summary>
        /// GJ: Communication Wizard Polish
        /// </summary>
        private void CommunicationWizardPolish()
        {
            // Site: Rock 
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "FullWorksurface", "Full Worksurface", "", "C2467799-BB45-4251-8EE6-F0BF27201535" ); 
            // Update New Communication Page to Use Full Worksurface
            RockMigrationHelper.UpdatePageLayout( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "C2467799-BB45-4251-8EE6-F0BF27201535" );
            // Add Block Page Menu to Layout: Full Worksurface, Site: Rock RMS 
            RockMigrationHelper.AddBlock( true, null,"C2467799-BB45-4251-8EE6-F0BF27201535".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu","Navigation",@"",@"",0,"4709764E-5378-4B7D-AC85-A5D06BE86ECA");
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Include Current Parameters // Attribute Value: False 
            RockMigrationHelper.AddBlockAttributeValue("4709764E-5378-4B7D-AC85-A5D06BE86ECA","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"False");
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Template // Attribute Value: {% include '~~/Assets/Lava/PageNav.lava' %} 
            RockMigrationHelper.AddBlockAttributeValue("4709764E-5378-4B7D-AC85-A5D06BE86ECA","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageNav.lava' %}");
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Root Page // Attribute Value: 20f97a93-7949-4c2a-8a5e-c756fe8585ca 
            RockMigrationHelper.AddBlockAttributeValue("4709764E-5378-4B7D-AC85-A5D06BE86ECA","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"20f97a93-7949-4c2a-8a5e-c756fe8585ca");
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Number of Levels // Attribute Value: 3 
            RockMigrationHelper.AddBlockAttributeValue("4709764E-5378-4B7D-AC85-A5D06BE86ECA","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"3");
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Include Current QueryString // Attribute Value: False 
            RockMigrationHelper.AddBlockAttributeValue("4709764E-5378-4B7D-AC85-A5D06BE86ECA","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False");
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Is Secondary Block // Attribute Value: False 
            RockMigrationHelper.AddBlockAttributeValue("4709764E-5378-4B7D-AC85-A5D06BE86ECA","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False");
            // Add Block Login Status to Layout: Full Worksurface, Site: Rock RMS 
            RockMigrationHelper.AddBlock( true, null,"C2467799-BB45-4251-8EE6-F0BF27201535".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"04712F3D-9667-4901-A49D-4507573EF7AD".AsGuid(), "Login Status","Login",@"",@"",0,"0E3484CA-06EA-46CD-A55E-A1A480965DC9");
            // Add Block Smart Search to Layout: Full Worksurface, Site: Rock RMS 
            RockMigrationHelper.AddBlock( true, null,"C2467799-BB45-4251-8EE6-F0BF27201535".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"9D406BD5-88C1-45E5-AFEA-70F9CFB66C74".AsGuid(), "Smart Search","Header",@"",@"",0,"AD1A8AD8-3E94-45F3-A4B4-6BD2AE72A133");
        }

        /// <summary>
        /// DH: Add Mass Push Notifications block and page
        /// </summary>
        private void MassPushNotificationsUp()
        {
            // Add/Update BlockType Mass Push Notifications
            RockMigrationHelper.UpdateBlockType( "Mass Push Notifications",
                "Used for creating and sending a mass push notification to recipients.",
                "~/Blocks/Communication/MassPushNotifications.ascx",
                "Communication",
                Rock.SystemGuid.BlockType.MASS_PUSH_NOTIFICATIONS );


            // Attribute for BlockType: Mass Push Notifications:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5EB6A086-8969-4E3A-AD42-2B2AD13E728F",
                "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D",
                "Enabled Lava Commands",
                "EnabledLavaCommands", 
                "Enabled Lava Commands",
                @"The Lava commands that should be enabled for this block.",
                1,
                @"",
                "4345F1B2-C741-4A17-96EA-00D6596E6D9B" );

            // Attribute for BlockType: Mass Push Notifications:Personal Device Active Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5EB6A086-8969-4E3A-AD42-2B2AD13E728F",
                "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                "Personal Device Active Duration",
                "PersonalDeviceActiveDuration",
                "Personal Device Active Duration",
                @"The number of days that the device must have an interaction in order for it to be considered an active device.",
                0,
                @"365",
                "4C9F6D43-3444-4EF5-AF95-242C0A3FD8ED" );

            // Attribute for BlockType: Mass Push Notifications:Send Immediately
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5EB6A086-8969-4E3A-AD42-2B2AD13E728F",
                "1EDAFDED-DFE6-4334-B019-6EECBA89E05A",
                "Send Immediately",
                "SendImmediately",
                "Send Immediately",
                @"Should communication be sent right away (vs. just being queued for scheduled job to send)?",
                2,
                @"True",
                "5409A366-44BB-4A2E-949B-CD87C86F17C2" );

            // Add Page Mass Push Notification to Site:Rock RMS
            RockMigrationHelper.AddPage( true,
                "7F79E512-B9DB-4780-9887-AD6D63A39050",
                "D65F783D-87A9-4CC9-8110-E83466A0EADB",
                "Mass Push Notification",
                "",
                Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS,
                "" );

            // Swap the positions of the Email Analytics and the Mass Push Notifications pages.
            Sql( $@"DECLARE @newOrder int
                DECLARE @oldOrder int
                SELECT @newOrder = [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.EMAIL_ANALYTICS}'
                SELECT @oldOrder = [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS}'
                UPDATE [Page] SET [Order] = @newOrder WHERE [Guid] = '{Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS}'
                UPDATE [Page] SET [Order] = @oldOrder WHERE [Guid] = '{Rock.SystemGuid.Page.EMAIL_ANALYTICS}'" );

            // Add Block Mass Push Notifications to Page: Mass Push Notification, Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS.AsGuid(),
                null,
                "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),
                Rock.SystemGuid.BlockType.MASS_PUSH_NOTIFICATIONS.AsGuid(),
                "Mass Push Notifications",
                "Main",
                @"",
                @"",
                0,
                "E19A4300-C6A8-45E4-A83B-2E5E87764932" );
RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.None,
                "532D3528-54BA-4259-B8AD-728EA0364206" );

            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.None,
                "BE6993F7-C51F-4014-9A9D-1185E1266EE4" );

            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS,
                2,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                "A8186BAB-4EDB-4609-841D-99EBEA977B0C" );
        }

        /// <summary>
        /// DH: Add Mass Push Notifications block and page
        /// </summary>
        private void MassPushNotificationsDown()
        {
            // Remove Block: Mass Push Notifications, from Page: Mass Push Notification, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E19A4300-C6A8-45E4-A83B-2E5E87764932" );

            // Swap the positions of the Email Analytics and the Mass Push Notifications pages.
            Sql( $@"DECLARE @newOrder int
                DECLARE @oldOrder int
                SELECT @newOrder = [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.EMAIL_ANALYTICS}'
                SELECT @oldOrder = [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS}'
                UPDATE [Page] SET [Order] = @newOrder WHERE [Guid] = '{Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS}'
                UPDATE [Page] SET [Order] = @oldOrder WHERE [Guid] = '{Rock.SystemGuid.Page.EMAIL_ANALYTICS}'" );

            // Delete Page Mass Push Notification from Site:Rock RMS
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.MASS_PUSH_NOTIFICATIONS ); //  Page: Mass Push Notification, Layout: Full Width, Site: Rock RMS

            // Send Immediately Attribute for BlockType: Mass Push Notifications
            RockMigrationHelper.DeleteAttribute( "5409A366-44BB-4A2E-949B-CD87C86F17C2" );

            // Enabled Lava Commands Attribute for BlockType: Mass Push Notifications
            RockMigrationHelper.DeleteAttribute( "4345F1B2-C741-4A17-96EA-00D6596E6D9B" );

            // Personal Device Active Duration Attribute for BlockType: Mass Push Notifications
            RockMigrationHelper.DeleteAttribute( "4C9F6D43-3444-4EF5-AF95-242C0A3FD8ED" );
            
            // Delete BlockType Mass Push Notifications
            RockMigrationHelper.DeleteBlockType( Rock.SystemGuid.BlockType.MASS_PUSH_NOTIFICATIONS ); // Mass Push Notifications
        }

        /// <summary>
        /// BW: Add interaction index change one time job
        /// </summary>
        private void InteractionIndexPersonalDeviceIdUp()
        {
            Sql( $@"
                IF NOT EXISTS(
                    SELECT [Id]
                    FROM [ServiceJob]
                    WHERE
                        [Class] = 'Rock.Jobs.PostV122_UpdateInteractionIndex' AND
                        [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_122_INTERACTION_PERSONAL_DEVICE_ID}'
                )
                BEGIN
                    INSERT INTO [ServiceJob] (
                        [IsSystem]
                        , [IsActive]
                        , [Name]
                        , [Description]
                        , [Class]
                        , [CronExpression]
                        , [NotificationStatus]
                        , [Guid]
                    )
                    VALUES (
                        1
                        , 1
                        , 'Rock Update Helper v12.2 - Interaction Index Update'
                        , 'Adds PersonalDeviceId to the included fields in the Component/Date Interaction index. After all the operations are done, this job will delete itself.'
                        , 'Rock.Jobs.PostV122_UpdateInteractionIndex'
                        , '0 0 2 1/1 * ? *'
                        , 1
                        , '{SystemGuid.ServiceJob.DATA_MIGRATIONS_122_INTERACTION_PERSONAL_DEVICE_ID}'
                    );
                END" );
        }
    }
}

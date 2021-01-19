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
    public partial class Rollup_0119 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "09479BFB-4F35-4BE4-9AA3-FD0DE1F1A877");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "A3BF7A25-17B6-4C73-851D-E4B517788576");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "3E76A9E4-F9A9-4C8F-A1FD-84CBAED51FF9");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC");

            // Attribute for BlockType: Group Detail:Add Administrate Security to Group Creator
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Administrate Security to Group Creator", "AddAdministrateSecurityToGroupCreator", "Add Administrate Security to Group Creator", @"If enabled, the person who creates a new group will be granted 'Administrate' security rights to the group.  This was the behavior in previous versions of Rock.  If disabled, the group creator will not be able to edit security or possibly perform other functions without the Rock administrator settings up a role that is allowed to perform such functions.", 19, @"False", "648F075B-1A6E-4A7A-9474-7C786231F158" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Show Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Audience Filter", "ShowAudienceFilter", "Show Audience Filter", @"When enabled the audience filter will be shown.", 8, @"False", "A386D856-6EF5-4EDC-8989-52F595D588F1" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Show Date Range Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "Show Date Range Filter", @"Determines whether the date range filters are shown.", 10, @"True", "98BC86EA-DEE4-4702-8767-94D0544F7905" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Show Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Campus Filter", "ShowCampusFilter", "Show Campus Filter", @"This setting will control if/when the campus dropdown filter shown.", 4, @"0", "63858271-ECC2-49E3-BD5F-3FF2C33E4FAF" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down filter.", 5, @"", "C90FC5EE-6E4F-4215-95D4-810203BA6CF5" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by type that are displayed in the campus drop-down filter.", 6, @"", "A9EFBA2F-F8C4-4C54-BC15-39EA98358706" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Filter Audiences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Filter Audiences", "FilterAudiences", "Filter Audiences", @"Determines which audiences should be displayed in the filter.", 9, @"", "1ABEDAA3-24D6-4926-982F-95FF3145FA29" );

            // Attribute for BlockType: Event Item Occurrences Search Lava:Event Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "EC0D9528-1A22-404E-A776-566404987363", "Event Calendar", "EventCalendar", "Event Calendar", @"This  setting would override any setting in the query string if provided.", 7, @"", "B64F1141-9243-4AFA-B9D2-3FA403BD62A9" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A3BF7A25-17B6-4C73-851D-E4B517788576", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "563D2870-5E6D-4742-91A7-790480DEA688" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A3BF7A25-17B6-4C73-851D-E4B517788576", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "20B4B707-A561-4FCF-99DC-9D7B1D0BB908" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3E76A9E4-F9A9-4C8F-A1FD-84CBAED51FF9", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "AABCAF10-E689-4690-BB0A-B74DF8210402" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3E76A9E4-F9A9-4C8F-A1FD-84CBAED51FF9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "FAECFE46-6A64-49F2-AAF7-3AB24EF9C139" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "8FC0382E-E9E6-4BA1-9CF9-2D721AE2D661" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "F78B5DD5-09DD-47A6-88A3-17E191CF6C81" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "D321593A-7F21-4891-9028-AE899EC0643F" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "BD45EE5F-95B8-4544-ACF8-7994B0A2906F" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "6C768E61-23C0-49B3-B6F7-DE959E0E30CA" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "72DBC3C1-D590-48BB-ACE0-311B7C0B5514" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "E8E7E32C-40B7-41B5-AC9C-64E956237D9F" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "703F9E23-DE16-4A14-A63E-9F90B178ADF7" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "13852081-90F6-41A4-A3FC-099BD7EE89A6" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "E5F953AA-DF89-48B3-97F8-99E414C3E014" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Show Date Range Filter Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("98BC86EA-DEE4-4702-8767-94D0544F7905");

            // Filter Audiences Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("1ABEDAA3-24D6-4926-982F-95FF3145FA29");

            // Show Audience Filter Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("A386D856-6EF5-4EDC-8989-52F595D588F1");

            // Event Calendar Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("B64F1141-9243-4AFA-B9D2-3FA403BD62A9");

            // Campus Statuses Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("A9EFBA2F-F8C4-4C54-BC15-39EA98358706");

            // Campus Types Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("C90FC5EE-6E4F-4215-95D4-810203BA6CF5");

            // Show Campus Filter Attribute for BlockType: Event Item Occurrences Search Lava
            RockMigrationHelper.DeleteAttribute("63858271-ECC2-49E3-BD5F-3FF2C33E4FAF");

            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("E8E7E32C-40B7-41B5-AC9C-64E956237D9F");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("72DBC3C1-D590-48BB-ACE0-311B7C0B5514");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("BD45EE5F-95B8-4544-ACF8-7994B0A2906F");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("D321593A-7F21-4891-9028-AE899EC0643F");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("13852081-90F6-41A4-A3FC-099BD7EE89A6");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("F78B5DD5-09DD-47A6-88A3-17E191CF6C81");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("703F9E23-DE16-4A14-A63E-9F90B178ADF7");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("E5F953AA-DF89-48B3-97F8-99E414C3E014");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("6C768E61-23C0-49B3-B6F7-DE959E0E30CA");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("8FC0382E-E9E6-4BA1-9CF9-2D721AE2D661");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("AABCAF10-E689-4690-BB0A-B74DF8210402");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("FAECFE46-6A64-49F2-AAF7-3AB24EF9C139");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("563D2870-5E6D-4742-91A7-790480DEA688");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("20B4B707-A561-4FCF-99DC-9D7B1D0BB908");

            // Add Administrate Security to Group Creator Attribute for BlockType: Group Detail
            RockMigrationHelper.DeleteAttribute("648F075B-1A6E-4A7A-9474-7C786231F158");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("95E80C4A-D0A8-4674-BCA8-D8A351B9EDAC"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("3E76A9E4-F9A9-4C8F-A1FD-84CBAED51FF9"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("A3BF7A25-17B6-4C73-851D-E4B517788576"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("09479BFB-4F35-4BE4-9AA3-FD0DE1F1A877"); // Structured Content View
        }
    }
}

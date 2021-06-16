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
    public partial class Rollup_0105 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "62377D4C-D8BF-4721-BDCF-9B44AC485306");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "6F1D17F4-B1A6-4D1B-9B73-18FFF3502F7E");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "7AED7110-ABDA-4F45-AF86-AEA93B51BED5");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D");

            // Attribute for BlockType: Web Farm Settings:Node CPU Chart Hours
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4280625A-C69A-4B47-A4D3-89B61F43C967", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node CPU Chart Hours", "CpuChartHours", "Node CPU Chart Hours", @"The amount of hours represented by the width of the Node CPU charts.", 2, @"4", "3B906118-C504-4AE3-9E2F-71C2A6533701" );

            // Attribute for BlockType: Web Farm Node Detail:Node CPU Chart Hours
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "95F38562-6CEF-4798-8A4F-05EBCDFB07E0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node CPU Chart Hours", "CpuChartHours", "Node CPU Chart Hours", @"The amount of hours represented by the width of the Node CPU chart.", 2, @"24", "7490A6B8-D8B8-4B53-8B37-CA52D2CDD5AC" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F1D17F4-B1A6-4D1B-9B73-18FFF3502F7E", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "7BD005AB-65B2-406A-B82E-DBEC44A4CE63" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F1D17F4-B1A6-4D1B-9B73-18FFF3502F7E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "63B3A2FF-8B7D-499C-B2B0-FF31EA1DF485" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7AED7110-ABDA-4F45-AF86-AEA93B51BED5", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "A6D78571-073C-4C76-84AF-99BFFB0729B2" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7AED7110-ABDA-4F45-AF86-AEA93B51BED5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "AA0FEEE4-4E0E-455E-9DD6-960433C856E9" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "93CCD225-B98F-43D6-A1F6-59DB8B24F76B" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "9954845B-2378-4C89-8968-1F18B42559C3" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "A9F48EF1-E05E-4CF6-8101-E1C19B61D3F3" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "4F65B4CE-13B4-43A3-9FED-E21DF02F8403" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "73A368C9-FE49-47AC-882B-D13081F763A6" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "F8E50B17-A154-4F72-877F-022177287205" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "AA1C3276-17E8-4661-86D9-E0F5E5268E58" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "AFC01BEE-37FC-4BA3-A912-725760CA27C6" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "728D6E80-E89D-4BF7-9638-80DA27F64734" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "A46F1D7A-CD4E-450A-88B1-7F44BB8A31D0" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("A9F48EF1-E05E-4CF6-8101-E1C19B61D3F3");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("4F65B4CE-13B4-43A3-9FED-E21DF02F8403");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("F8E50B17-A154-4F72-877F-022177287205");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("73A368C9-FE49-47AC-882B-D13081F763A6");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("728D6E80-E89D-4BF7-9638-80DA27F64734");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("9954845B-2378-4C89-8968-1F18B42559C3");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("AFC01BEE-37FC-4BA3-A912-725760CA27C6");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("A46F1D7A-CD4E-450A-88B1-7F44BB8A31D0");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("AA1C3276-17E8-4661-86D9-E0F5E5268E58");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("93CCD225-B98F-43D6-A1F6-59DB8B24F76B");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("A6D78571-073C-4C76-84AF-99BFFB0729B2");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("AA0FEEE4-4E0E-455E-9DD6-960433C856E9");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("7BD005AB-65B2-406A-B82E-DBEC44A4CE63");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("63B3A2FF-8B7D-499C-B2B0-FF31EA1DF485");

            // Node CPU Chart Hours Attribute for BlockType: Web Farm Node Detail
            RockMigrationHelper.DeleteAttribute("7490A6B8-D8B8-4B53-8B37-CA52D2CDD5AC");

            // Node CPU Chart Hours Attribute for BlockType: Web Farm Settings
            RockMigrationHelper.DeleteAttribute("3B906118-C504-4AE3-9E2F-71C2A6533701");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("BCA47D5B-A60D-4B7B-8D01-D2498F0DA29D"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("7AED7110-ABDA-4F45-AF86-AEA93B51BED5"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("6F1D17F4-B1A6-4D1B-9B73-18FFF3502F7E"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("62377D4C-D8BF-4721-BDCF-9B44AC485306"); // Structured Content View
        }

    }
}

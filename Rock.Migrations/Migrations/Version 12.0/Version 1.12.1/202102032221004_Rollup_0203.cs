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
    public partial class Rollup_0203 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateMobileBlockTypeEntities();
            CodeGenMigrationsUp();
            KPIStyling();
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "A8BBE3F8-F3CC-4C0A-AB2F-5085F5BF59E7");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "863E5638-B310-407E-A54E-2C069979881D");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "15DD270A-A0BB-45BF-AA36-FE37856C60DE");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "FC2879AC-5967-43E7-8759-6888BF21CE21");

            // Attribute for BlockType: Select Check-In Area:Check-in Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17E8F764-562A-4E94-980D-FF1B15640670", "7522975C-C224-489A-985D-B44580DFC5BD", "Check-in Areas", "CheckinConfigurationTypes", "Check-in Areas", @"Select the Check Areas to display, or select none to show all.", 3, @"", "12E6CCA0-3850-4201-9684-39AB974C8325" );

            // Attribute for BlockType: Transaction Entry (V2):Enable Fee Coverage
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Fee Coverage", "EnableFeeCoverage", "Enable Fee Coverage", @"Determines if the fee coverage feature is enabled or not.", 27, @"False", "0C1A2615-C1CA-4C99-B6CB-9C3F40B91A08" );

            // Attribute for BlockType: Transaction Entry (V2):Fee Coverage Default State
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Fee Coverage Default State", "FeeCoverageDefaultState", "Fee Coverage Default State", @"Determines if checkbox for 'Cover the fee' defaults to checked.", 28, @"False", "C788EAEF-C802-4B32-BEA1-1A1C61A9B0DF" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "863E5638-B310-407E-A54E-2C069979881D", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "20C706F6-D690-401B-83A6-9BD41661AAD2" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "863E5638-B310-407E-A54E-2C069979881D", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "8F90D46F-3420-40FD-8E63-923295326C22" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "15DD270A-A0BB-45BF-AA36-FE37856C60DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "DA732233-F11D-48C9-8E12-2077FE03397D" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "15DD270A-A0BB-45BF-AA36-FE37856C60DE", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "96CC7902-C81F-463F-A3A1-85D36ACE3618" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "38D098A9-EA3C-429F-BCEE-160D5B0BC6F6" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "E9E43710-BA95-4CBC-BE22-FE98F644E009" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "D42DAC96-8C89-426E-A447-60DBCD6E6963" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "D8666F25-3577-4C35-9B53-C8CA31211CF0" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "ADACDBD7-6AE2-4DED-927B-4CF281F99D89" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "4C157357-192D-48E0-A0AB-FF9E22E26EB3" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "F664DAD3-82FC-452A-BD78-1DD92D3C097B" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "F60A2F75-6472-4A6B-BFAC-1C158BA1E6A5" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "597BAA37-499F-4676-B89C-A39551E25D23" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC2879AC-5967-43E7-8759-6888BF21CE21", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "BC076E71-0353-44CA-ABD3-3AF888C400E5" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // Fee Coverage Default State Attribute for BlockType: Transaction Entry (V2)
            RockMigrationHelper.DeleteAttribute("C788EAEF-C802-4B32-BEA1-1A1C61A9B0DF");

            // Enable Fee Coverage Attribute for BlockType: Transaction Entry (V2)
            RockMigrationHelper.DeleteAttribute("0C1A2615-C1CA-4C99-B6CB-9C3F40B91A08");

            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("D8666F25-3577-4C35-9B53-C8CA31211CF0");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("38D098A9-EA3C-429F-BCEE-160D5B0BC6F6");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("4C157357-192D-48E0-A0AB-FF9E22E26EB3");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("ADACDBD7-6AE2-4DED-927B-4CF281F99D89");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("597BAA37-499F-4676-B89C-A39551E25D23");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("D42DAC96-8C89-426E-A447-60DBCD6E6963");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("F60A2F75-6472-4A6B-BFAC-1C158BA1E6A5");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("BC076E71-0353-44CA-ABD3-3AF888C400E5");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("F664DAD3-82FC-452A-BD78-1DD92D3C097B");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("E9E43710-BA95-4CBC-BE22-FE98F644E009");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("96CC7902-C81F-463F-A3A1-85D36ACE3618");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("DA732233-F11D-48C9-8E12-2077FE03397D");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("8F90D46F-3420-40FD-8E63-923295326C22");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("20C706F6-D690-401B-83A6-9BD41661AAD2");

            // Check-in Areas Attribute for BlockType: Select Check-In Area
            RockMigrationHelper.DeleteAttribute("12E6CCA0-3850-4201-9684-39AB974C8325");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("FC2879AC-5967-43E7-8759-6888BF21CE21"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("15DD270A-A0BB-45BF-AA36-FE37856C60DE"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("863E5638-B310-407E-A54E-2C069979881D"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("A8BBE3F8-F3CC-4C0A-AB2F-5085F5BF59E7"); // Structured Content View

        }

        /// <summary>
        /// Run this before CodeGenMigrationsUp
        /// </summary>
        private void CreateMobileBlockTypeEntities()
        {
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Cms.StructuredContentView","Structured Content View","Rock.Blocks.Types.Mobile.Cms.StructuredContentView, Rock, Version=1.12.1.1, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_CMS_STRUCTUREDCONTENTVIEW_BLOCK_TYPE);
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Communication.CommunicationView","Communication View","Rock.Blocks.Types.Mobile.Communication.CommunicationView, Rock, Version=1.12.1.1, Culture=neutral, PublicKeyToken=null",false,false,Rock.SystemGuid.EntityType.MOBILE_COMMUNICATION_COMMUNICATIONVIEW_BLOCK_TYPE);
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView","Calendar Event Item Occurrence View","Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView, Rock, Version=1.12.1.1, Culture=neutral, PublicKeyToken=null",false,false,Rock.SystemGuid.EntityType.MOBILE_EVENTS_CALENDAREVENTITEMOCCURRENCEVIEW_BLOCK_TYPE);
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava","Event Item Occurrence List By Audience Lava","Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava, Rock, Version=1.12.1.1, Culture=neutral, PublicKeyToken=null",false,false,Rock.SystemGuid.EntityType.MOBILE_EVENTS_EVENTITEMOCCURRENCELISTBYAUDIENCELAVA_BLOCK_TYPE);
        }

        /// <summary>
        /// GJ: KPI styling
        /// </summary>
        private void KPIStyling()
        {
            Sql( MigrationSQL._202102032221004_Rollup_0203_KPIStyling );
        }
    }
}

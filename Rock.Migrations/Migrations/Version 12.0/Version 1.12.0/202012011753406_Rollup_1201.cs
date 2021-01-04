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
    public partial class Rollup_1201 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "8E201840-2DB8-410D-B7AE-BCC6C6B3EDF4");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "9FF1F7D0-3769-4988-9D16-063ADDDC5CD8");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "FF2D0E9B-3852-4328-9B1E-BDEFCBEBCFB9");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "F370CEA2-7431-4188-BDD5-E82BF1162417");

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FF1F7D0-3769-4988-9D16-063ADDDC5CD8", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "F7E6AF6A-3549-4BB2-AEB0-D42FCD1C70E2" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FF1F7D0-3769-4988-9D16-063ADDDC5CD8", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "6A4076B8-ED97-46C9-85CF-B8A35C1D6A29" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF2D0E9B-3852-4328-9B1E-BDEFCBEBCFB9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "63D57430-EBF1-4179-8B73-075A9A6142B8" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF2D0E9B-3852-4328-9B1E-BDEFCBEBCFB9", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "E52758EC-8A3A-4DC7-BC67-CB579DB9AF5B" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "782A7658-59BE-41EE-8925-D3ADA53F6F68" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "A9A88882-9785-40FB-991B-EEEB546BF14E" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "22F87BA3-4043-42F1-BD38-E2CD6C4AB5FA" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "A428171F-F0BF-43A3-BC4E-68B4A545B593" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "A02AEFFA-9B22-4C48-B3E6-098AB9B82FA9" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "FA788AB0-A2B1-4F03-B363-7671A4E9700A" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "5BB5B17E-E2B7-4603-A86F-4ECED29E5C33" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "039DD690-2464-41F2-8BB6-DDC634FFBCC5" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "DD663F57-2B70-4461-87AD-7ED501124C5D" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F370CEA2-7431-4188-BDD5-E82BF1162417", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "44DC8EC6-14C8-4732-9968-EDA17352BCE6" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("44DC8EC6-14C8-4732-9968-EDA17352BCE6");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("DD663F57-2B70-4461-87AD-7ED501124C5D");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("039DD690-2464-41F2-8BB6-DDC634FFBCC5");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("5BB5B17E-E2B7-4603-A86F-4ECED29E5C33");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("FA788AB0-A2B1-4F03-B363-7671A4E9700A");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("A02AEFFA-9B22-4C48-B3E6-098AB9B82FA9");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("A428171F-F0BF-43A3-BC4E-68B4A545B593");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("22F87BA3-4043-42F1-BD38-E2CD6C4AB5FA");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("A9A88882-9785-40FB-991B-EEEB546BF14E");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("782A7658-59BE-41EE-8925-D3ADA53F6F68");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("E52758EC-8A3A-4DC7-BC67-CB579DB9AF5B");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("63D57430-EBF1-4179-8B73-075A9A6142B8");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("6A4076B8-ED97-46C9-85CF-B8A35C1D6A29");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("F7E6AF6A-3549-4BB2-AEB0-D42FCD1C70E2");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("F370CEA2-7431-4188-BDD5-E82BF1162417"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("FF2D0E9B-3852-4328-9B1E-BDEFCBEBCFB9"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("9FF1F7D0-3769-4988-9D16-063ADDDC5CD8"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("8E201840-2DB8-410D-B7AE-BCC6C6B3EDF4"); // Structured Content View
        }
    }
}

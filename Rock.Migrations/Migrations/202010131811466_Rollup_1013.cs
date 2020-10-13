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
    public partial class Rollup_1013 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateYouTubeShortcodeDocumentation();
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "102ABD17-177F-46AE-810C-CD520E980982");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "C023630E-BF88-4950-9D80-D3A48ADEE8D1");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "66066C09-43C1-4991-A96A-4FCCD4AE7C91");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B");

            // Attribute for BlockType: Public Profile Edit:View Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "View Template", "ViewTemplate", "View Template", @"The lava template to use to format the view details.", 17, @"{% include '~/Assets/Lava/PublicProfile.lava' %}", "7EA671AB-45BA-4FEA-B353-89ABC398A5BC" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C023630E-BF88-4950-9D80-D3A48ADEE8D1", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "7B974EBC-01F9-43B0-BD12-8B512F348D5E" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C023630E-BF88-4950-9D80-D3A48ADEE8D1", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "7D87C6DB-A726-4333-AB23-A49CF41A1D1E" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66066C09-43C1-4991-A96A-4FCCD4AE7C91", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "8E1556A6-9F52-4647-AFDE-26DC031A12AA" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66066C09-43C1-4991-A96A-4FCCD4AE7C91", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "5157E498-0CD7-4818-B504-A9810ED49003" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "EF57C1BB-3A40-4ACF-81BA-BB5695C9C204" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "6243B18D-5114-4FAC-8207-6CB81C984FE4" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "661F4A9F-1E87-473F-BFFD-47ADB6EB08BC" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "B6216B7F-D0B6-40DE-92C4-D01A37BC9C09" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "F139BD3A-063E-4D58-A52E-AA57B1C0ED9D" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "FB8D1604-B22E-4CF9-9331-757EB0469EF5" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "E4D5F10E-5090-4D87-8465-09B5474F8498" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "8BF80B56-F7E7-4332-907F-069FB0AE39CB" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "CD6AFD72-5798-4BC9-BB3F-5E09A3491028" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "960AE1D3-B267-4A4D-A685-48B6CCD193EA" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("960AE1D3-B267-4A4D-A685-48B6CCD193EA");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("CD6AFD72-5798-4BC9-BB3F-5E09A3491028");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("8BF80B56-F7E7-4332-907F-069FB0AE39CB");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("E4D5F10E-5090-4D87-8465-09B5474F8498");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("FB8D1604-B22E-4CF9-9331-757EB0469EF5");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("F139BD3A-063E-4D58-A52E-AA57B1C0ED9D");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("B6216B7F-D0B6-40DE-92C4-D01A37BC9C09");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("661F4A9F-1E87-473F-BFFD-47ADB6EB08BC");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("6243B18D-5114-4FAC-8207-6CB81C984FE4");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("EF57C1BB-3A40-4ACF-81BA-BB5695C9C204");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("5157E498-0CD7-4818-B504-A9810ED49003");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("8E1556A6-9F52-4647-AFDE-26DC031A12AA");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("7D87C6DB-A726-4333-AB23-A49CF41A1D1E");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("7B974EBC-01F9-43B0-BD12-8B512F348D5E");

            // View Template Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("7EA671AB-45BA-4FEA-B353-89ABC398A5BC");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("F3C731C9-A9D3-4DA2-8D0C-3CFFD7974C3B"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("66066C09-43C1-4991-A96A-4FCCD4AE7C91"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("C023630E-BF88-4950-9D80-D3A48ADEE8D1"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("102ABD17-177F-46AE-810C-CD520E980982"); // Structured Content View
        }

        private void UpdateYouTubeShortcodeDocumentation()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation] = REPLACE([Documentation], 'with the id', 'with the id you')
                WHERE [Guid] = '2fa4d446-3f63-4dfd-8c6a-55dba76aeb83'" );
        }

    }
}

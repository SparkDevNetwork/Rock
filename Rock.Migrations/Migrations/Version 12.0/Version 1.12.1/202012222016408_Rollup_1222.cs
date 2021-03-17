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
    public partial class Rollup_1222 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "F45C98FD-A8E5-4BD0-A965-E5F68D72C6F6");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "AB8326CD-80FA-41A1-9C13-0C2059D1D003");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "56D13591-FA51-4C6F-BD1C-571092DD5829");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4");

            // Attribute for BlockType: Login:Username Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Username Field Label", "UsernameFieldLabel", "Username Field Label", @"The label to use for the username field.  For example, this allows an organization to customize it to 'Username / Email' in cases where both are supported.", 0, @"Username", "70ED0BE3-AF56-4AA2-96A5-3878C10468F3" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AB8326CD-80FA-41A1-9C13-0C2059D1D003", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "63541778-9B13-4880-90B8-D52A091F98E1" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AB8326CD-80FA-41A1-9C13-0C2059D1D003", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "57A101C3-6158-4F2E-895F-52839AC3E31F" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56D13591-FA51-4C6F-BD1C-571092DD5829", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "0728E5AB-96E2-4E3D-9FEC-8AB99732EF70" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56D13591-FA51-4C6F-BD1C-571092DD5829", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "52159008-499D-4550-99E4-B5A2364F7076" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "3E562979-A43D-4823-B517-47449F1603E2" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "9440C6A3-5178-4D5A-9CF9-BCE2D2F66D67" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "C4F87F31-2D8B-4EE9-BAA9-85D906D5DF77" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "24FF9DF4-5AAB-4CD2-8FA3-71224B04FE99" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "DD0A5A30-E517-4C7A-92E1-1C13C26DDFE1" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "2D5E66ED-0F3B-4F99-AEC1-6D26E30608BC" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "666FA16B-1868-4F11-BE92-5927F0FA2D0D" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "8E9D27F6-4C9D-4AA6-85D7-E68450A4E8A5" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "D7050DEA-3EFF-4DFA-AC52-4EE54107DC75" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "3718355B-15FE-4FDE-909A-868446B011D3" );

            // Attribute for BlockType: Event Item Personalized Registration:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "Registration Page", "Registration Page", @"The registration page to redirect to.", 3, @"", "58A8A00F-60DB-4A61-AEB6-A3BA3861E45C" );

            // Attribute for BlockType: Event Item Personalized Registration:Registrant List Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Registrant List Lava Template", "RegistrantListLavaTemplate", "Registrant List Lava Template", @"This template will be used in creating the text the displays for the checkbox. If the template returns no text the family member will not be displayed.", 4, @"{{ Person.FullName }} <small>({{ Person.AgeClassification }})</small>", "083CD8F8-9D53-4F69-B1AB-AE93E16D3163" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Registrant List Lava Template Attribute for BlockType: Event Item Personalized Registration
            RockMigrationHelper.DeleteAttribute("083CD8F8-9D53-4F69-B1AB-AE93E16D3163");

            // Registration Page Attribute for BlockType: Event Item Personalized Registration
            RockMigrationHelper.DeleteAttribute("58A8A00F-60DB-4A61-AEB6-A3BA3861E45C");

            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("3718355B-15FE-4FDE-909A-868446B011D3");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("D7050DEA-3EFF-4DFA-AC52-4EE54107DC75");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("8E9D27F6-4C9D-4AA6-85D7-E68450A4E8A5");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("666FA16B-1868-4F11-BE92-5927F0FA2D0D");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("2D5E66ED-0F3B-4F99-AEC1-6D26E30608BC");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("DD0A5A30-E517-4C7A-92E1-1C13C26DDFE1");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("24FF9DF4-5AAB-4CD2-8FA3-71224B04FE99");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("C4F87F31-2D8B-4EE9-BAA9-85D906D5DF77");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("9440C6A3-5178-4D5A-9CF9-BCE2D2F66D67");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("3E562979-A43D-4823-B517-47449F1603E2");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("52159008-499D-4550-99E4-B5A2364F7076");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("0728E5AB-96E2-4E3D-9FEC-8AB99732EF70");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("57A101C3-6158-4F2E-895F-52839AC3E31F");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("63541778-9B13-4880-90B8-D52A091F98E1");

            // Username Field Label Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("70ED0BE3-AF56-4AA2-96A5-3878C10468F3");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("4FEDB7BC-6BE2-4E1E-9A98-E47633B0C0F4"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("56D13591-FA51-4C6F-BD1C-571092DD5829"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("AB8326CD-80FA-41A1-9C13-0C2059D1D003"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("F45C98FD-A8E5-4BD0-A965-E5F68D72C6F6"); // Structured Content View
        }
    }
}

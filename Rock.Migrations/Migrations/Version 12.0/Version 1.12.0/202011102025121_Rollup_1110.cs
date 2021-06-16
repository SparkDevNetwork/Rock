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
    public partial class Rollup_1110 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateForgotUserNameTemplateUpPartTwo();
            FixCheckinManagerLogoutParentPage();
            KPIShortcode();
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "D8DCDC0C-220B-40C7-8BDB-1A5750AB7EB4");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "E455555C-545B-4FD4-BF7E-4FA775871947");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "9D2300A5-6487-4C51-B2AB-1E60A5B1DA49");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "375EA635-289E-4988-9ECE-FE4075363154");

            // Attribute for BlockType: Edit Label:Labelary URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5ACB281A-CE85-426F-92A6-771F3B8AEF8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Labelary URL", "LabelaryUrl", "Labelary URL", @"This is the URL template used to display the label preview. The values in the default template are: 0 = dpmm, 1 = width, 2 = height, 3 = Label Index, 4 = label text.", 0, @"https://labelary2.cfapps.io/v1/printers/{0}dpmm/labels/{1}x{2}/{3}/{4}", "B610C2AC-1CF5-46EB-BDD2-98FBD31F0130" );

            // Attribute for BlockType: My Connection Opportunities:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0AA4BE93-9029-4790-8750-AE9CD75C9CAC" );

            // Attribute for BlockType: My Connection Opportunities:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "729E3B53-E834-43DC-9C69-2A212ECFC138" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E455555C-545B-4FD4-BF7E-4FA775871947", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "D5FAFF9C-BDB5-4378-B46E-902B7397B424" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E455555C-545B-4FD4-BF7E-4FA775871947", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "70BDD76E-9AB9-460E-85A4-1961C3AB2E85" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D2300A5-6487-4C51-B2AB-1E60A5B1DA49", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "B8F59AF6-5C00-4A50-8442-AE3A9AD9B69E" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D2300A5-6487-4C51-B2AB-1E60A5B1DA49", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "36261DF2-F52E-4627-9381-E82F71877712" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "5D148853-8E81-41D3-B916-FEE3E14B56AC" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "C1CDDA8C-5CC7-4669-BC3D-5C5E2F0D57AC" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "6E88DA93-B200-4FB1-B3E6-FA3B25037400" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "7D9F410C-2CEF-41BB-AB0C-EB868A5918AA" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "5266DD46-E2D6-455D-BE90-2A385A86DE4A" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "0B989D8F-FBD0-4A8B-8113-8C8FD4350994" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "7E55ACCF-46A7-4C7D-AB4B-6199E2387472" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "2CC14DBC-E959-48ED-9B34-04126649F0A4" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "639C8A66-A010-4A99-99E2-A093C1489B4A" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "375EA635-289E-4988-9ECE-FE4075363154", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "B036910B-0F11-41EC-ACEB-6565253089CB" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("B036910B-0F11-41EC-ACEB-6565253089CB");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("639C8A66-A010-4A99-99E2-A093C1489B4A");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("2CC14DBC-E959-48ED-9B34-04126649F0A4");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("7E55ACCF-46A7-4C7D-AB4B-6199E2387472");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("0B989D8F-FBD0-4A8B-8113-8C8FD4350994");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("5266DD46-E2D6-455D-BE90-2A385A86DE4A");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("7D9F410C-2CEF-41BB-AB0C-EB868A5918AA");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("6E88DA93-B200-4FB1-B3E6-FA3B25037400");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("C1CDDA8C-5CC7-4669-BC3D-5C5E2F0D57AC");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("5D148853-8E81-41D3-B916-FEE3E14B56AC");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("36261DF2-F52E-4627-9381-E82F71877712");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("B8F59AF6-5C00-4A50-8442-AE3A9AD9B69E");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("70BDD76E-9AB9-460E-85A4-1961C3AB2E85");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("D5FAFF9C-BDB5-4378-B46E-902B7397B424");

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: My Connection Opportunities
            RockMigrationHelper.DeleteAttribute("729E3B53-E834-43DC-9C69-2A212ECFC138");

            // core.CustomActionsConfigs Attribute for BlockType: My Connection Opportunities
            RockMigrationHelper.DeleteAttribute("0AA4BE93-9029-4790-8750-AE9CD75C9CAC");

            // Labelary URL Attribute for BlockType: Edit Label
            RockMigrationHelper.DeleteAttribute("B610C2AC-1CF5-46EB-BDD2-98FBD31F0130");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("375EA635-289E-4988-9ECE-FE4075363154"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("9D2300A5-6487-4C51-B2AB-1E60A5B1DA49"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("E455555C-545B-4FD4-BF7E-4FA775871947"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("D8DCDC0C-220B-40C7-8BDB-1A5750AB7EB4"); // Structured Content View

        }
    
        /// <summary>
        /// SK: Update Forgot User Names Communication Template
        /// </summary>
        private void UpdateForgotUserNameTemplateUpPartTwo()
        {
            string oldValue = @"{% assign isChangeable =  SupportsChangePassword | Contains: User.UserName %}";
            string newValue = @"{% assign isChangeable =  Result.SupportsChangePassword | Contains: User.UserName %}";

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );
            Sql( $@"UPDATE
                        [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE 
                        {targetColumn} LIKE '%{oldValue}%'
                        AND [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'" );
        }

        /// <summary>
        /// MP: Fix Checkin Manager Logout Parent Page
        /// </summary>
        private void FixCheckinManagerLogoutParentPage()
        {
            // Fix CHECK_IN_MANAGER_LOGOUT page so the Parent Page is CHECK_IN_MANAGER_LOGIN
            Sql( $@"
                UPDATE [Page]
                SET [ParentPageId] = (
                    SELECT TOP 1 [Id]
                    FROM [Page] x
                    WHERE x.[Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGIN}' )
                WHERE [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGOUT}' AND [ParentPageId] != (
                    SELECT TOP 1 [Id]
                    FROM [Page] x
                    WHERE x.[Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGIN}' )" );
        }

        /// <summary>
        /// GJ: KPI Shortcode
        /// </summary>
        private void KPIShortcode()
        {
            Sql( MigrationSQL._202011102025121_Rollup_1110_kpisortcode );
        }
    }
}

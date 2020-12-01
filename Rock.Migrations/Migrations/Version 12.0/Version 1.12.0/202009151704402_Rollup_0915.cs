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
    public partial class Rollup_0915 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            AddModelMapIcons();
            RemoveSampleReactBlock();
            ShowMobileApplicationsPage();
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
            
            // Add/Update BlockType Cache Reader
            RockMigrationHelper.UpdateBlockType("Cache Reader","Shows information about what's being cached in Rock.","~/Blocks/Utility/CacheReader.ascx","Utility","B2859CA9-F796-4D83-A83B-62AA44FC6BC5");

            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "7C1DB8A0-B373-4D25-B1AF-FC2AEBBA7045");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "9FFAC80C-F194-4063-9121-D54F24188ED8");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "8E3E2C15-29CE-45DA-80CA-A2B441D46646");

            // Attribute for BlockType: Redirect:Permanent Redirect
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B97FB779-5D3E-4663-B3B5-3C2C227AE14A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Permanent Redirect", "PermanentRedirect", "Permanent Redirect", @"If enabled, the redirect will be performed with a 301 status code which will indicate to search engines that this page has permanently moved to the new location. <span class='badge badge-warning'>Do not enable if using Lava.</span>", 3, @"False", "A61FF3BD-06F0-4AB4-AD6B-2BC7C83CB0FD" );

            // Attribute for BlockType: Exception List:Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 6, @"180", "771BFD26-310E-4C67-B057-5B58938F0E34" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFAC80C-F194-4063-9121-D54F24188ED8", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "B13B9E8F-AE33-4CF7-8CAB-E52BB2528F67" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFAC80C-F194-4063-9121-D54F24188ED8", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "49677619-DF1D-4ADE-A090-67EB10B07BD2" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E3E2C15-29CE-45DA-80CA-A2B441D46646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "3D898746-5140-4BCD-87A0-5CD2E927661B" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E3E2C15-29CE-45DA-80CA-A2B441D46646", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "67BFC1FA-4830-4409-8971-2983E35DE09F" );

            // Attribute for BlockType: Cache Reader:Show Email Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2859CA9-F796-4D83-A83B-62AA44FC6BC5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Address", "ShowEmailAddress", "Show Email Address", @"Should the email address be shown?", 1, @"True", "36EE9A76-ACF9-45C3-A8F0-061A454A4B91" );

            // Attribute for BlockType: Cache Reader:Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2859CA9-F796-4D83-A83B-62AA44FC6BC5", "3D045CAE-EA72-4A04-B7BE-7FD1D6214217", "Email", "Email", "Email", @"The Email address to show.", 2, @"ted@rocksolidchurchdemo.com", "4C519911-FC14-4692-85C6-86CB9D1BDE0E" );

            // Attribute for BlockType: Content:Context Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Context Entity Type", "ContextEntityType", "Context Entity Type", @"The type of entity that will provide context for this block", 3, @"", "D8CC5DFB-267C-4154-8E34-8DC02CF8B421" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // Context Entity Type Attribute for BlockType: Content
            RockMigrationHelper.DeleteAttribute("D8CC5DFB-267C-4154-8E34-8DC02CF8B421");

            // Email Attribute for BlockType: Cache Reader
            RockMigrationHelper.DeleteAttribute("4C519911-FC14-4692-85C6-86CB9D1BDE0E");

            // Show Email Address Attribute for BlockType: Cache Reader
            RockMigrationHelper.DeleteAttribute("36EE9A76-ACF9-45C3-A8F0-061A454A4B91");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("67BFC1FA-4830-4409-8971-2983E35DE09F");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("3D898746-5140-4BCD-87A0-5CD2E927661B");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("49677619-DF1D-4ADE-A090-67EB10B07BD2");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("B13B9E8F-AE33-4CF7-8CAB-E52BB2528F67");

            // Database Timeout Attribute for BlockType: Exception List
            RockMigrationHelper.DeleteAttribute("771BFD26-310E-4C67-B057-5B58938F0E34");

            // Permanent Redirect Attribute for BlockType: Redirect
            RockMigrationHelper.DeleteAttribute("A61FF3BD-06F0-4AB4-AD6B-2BC7C83CB0FD");

            // Delete BlockType Cache Reader
            RockMigrationHelper.DeleteBlockType("B2859CA9-F796-4D83-A83B-62AA44FC6BC5"); // Cache Reader

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("8E3E2C15-29CE-45DA-80CA-A2B441D46646"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("9FFAC80C-F194-4063-9121-D54F24188ED8"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("7C1DB8A0-B373-4D25-B1AF-FC2AEBBA7045"); // Structured Content View

        }
    
        /// <summary>
        /// MB: Add Model Map Icons
        /// </summary>
        private void AddModelMapIcons()
        {
            RockMigrationHelper.AddBlockAttributeValue( "2583DE89-F028-4ACE-9E1F-2873340726AC", 
                "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A",
                @"CMS^fa fa-code|Communication^fa fa-comment|Connection^fa fa-plug|Core^fa fa-gear|Event^fa fa-clipboard|Finance^fa fa-money|Group^fa fa-users|Prayer^fa fa-cloud-upload|Reporting^fa fa-list-alt|Workflow^fa fa-gears|Other^fa fa-question-circle|CRM^fa fa-user|Meta^fa fa-table|Engagement^fa fa-cogs" );
        }

        /// <summary>
        /// MP: Remove SampleReactBlock 
        /// </summary>
        private void RemoveSampleReactBlock()
        {
            // Delete the 'SampleReactBlock' which no longer exists
            RockMigrationHelper.DeleteBlockType( "7dc6c490-e9af-407c-9716-991564f93ff6" );
        }

        /// <summary>
        /// MP: Show the Mobile Applications Page
        /// </summary>
        private void ShowMobileApplicationsPage()
        {
            Sql( @"
                UPDATE [Page]
                SET [DisplayInNavWhen] = 0
                WHERE [Guid] = '784259ec-46b7-4de3-ac37-e8bfdb0b90a6'" );
        }
    }
}

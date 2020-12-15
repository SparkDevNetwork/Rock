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
    public partial class Rollup_1215 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateParallaxDocumentation();
            UpdateStatementGeneratorDownloadLinkUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            UpdateStatementGeneratorDownloadLinkDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "356AA6B5-2D19-4496-B7EE-E0B13E36D75A");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "AEE9A39B-2945-4E86-9A2B-ED6ED49403E1");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "3830C4BF-09FE-4BD3-8137-5803EDDC4E6C");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "A8F43F38-7316-4C76-A1E8-4B55799BB41F");

            // Attribute for BlockType: My Connection Opportunities Lava:Exclude Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "E4E72958-4604-498F-956B-BA095976A60B", "Exclude Connection Types", "ExcludedConnectionTypes", "Exclude Connection Types", @"Optional list of connection types to exclude from the display to (None will be excluded by default).", 3, @"", "F300B10E-A005-4260-9146-D1FD4D5287A7" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AEE9A39B-2945-4E86-9A2B-ED6ED49403E1", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "C2B23AB9-1E78-497A-9338-BD1A1A2E0463" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AEE9A39B-2945-4E86-9A2B-ED6ED49403E1", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "B5160D1C-BB15-4E6C-8267-C844B190CD49" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3830C4BF-09FE-4BD3-8137-5803EDDC4E6C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "DF27B70C-BBEC-408F-B8BE-2DC39FE7C45B" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3830C4BF-09FE-4BD3-8137-5803EDDC4E6C", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "7A072EB1-FA6E-4603-A9A7-3BC1017060F8" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "54E777E3-CA59-4A2D-BD47-7DEBECB22BE9" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "17A0B3A0-3612-4EE3-A0FD-A712988FCEEA" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "5228E42F-3ED8-4EC8-9832-585591ED7534" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "1D1CA741-FD96-4076-9970-EEA451F464D4" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "723AC60A-D2E4-4664-9C6E-4596BC1D2989" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "B93BA07D-CE96-418E-8649-1EE6F4E54F76" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "AD562161-2856-4C00-A411-97C11E9FE914" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "B6791F9B-EABE-46DF-B0BC-E0590C1D8753" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "7FCF321D-2E91-421E-9F85-CF55907F6CCE" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8F43F38-7316-4C76-A1E8-4B55799BB41F", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "C281975D-524F-4ECB-9415-7D4252D4EE6A" );

            // Attribute for BlockType: Register:Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "Gender", "Gender", @"Determines the visibility and requirement of the Gender field.", 8, @"2", "AAB94EB6-AAD8-4D68-8B56-22F192DAA7E1" );

            // Attribute for BlockType: Account Entry:Require Email For Username
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Email For Username", "RequireEmailForUsername", "Require Email For Username", @"When enabled the label on the Username will be changed to Email and the field will validate to ensure that the input is formatted as an email.", 0, @"False", "B4EF9FB5-3EF2-46E1-A1DE-926E16016598" );

            // Attribute for BlockType: Account Entry:Username Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Username Field Label", "UsernameFieldLabel", "Username Field Label", @"The label to use for the username field.  For example, this allows an organization to customize it to 'Username / Email' in cases where both are supported.", 1, @"Username", "0B7D0399-6A04-4687-A41B-BA033489B12A" );

            // Attribute for BlockType: Step Program Detail:Key Performance Indicator Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Key Performance Indicator Lava", "KpiLava", "Key Performance Indicator Lava", @"The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>", 2, @"{[kpis]}
  [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing' color:'blue-700']][[ endkpi ]]
  [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete' color:'green-600']][[ endkpi ]]
  [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
  [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}", "824D334A-944E-4315-A0E9-35BB20EE40D2" );

            // Attribute for BlockType: Step Type Detail:Key Performance Indicator Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Key Performance Indicator Lava", "KpiLava", "Key Performance Indicator Lava", @"The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>", 9, @"{[kpis]}
    [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing' color:'blue-700']][[ endkpi ]]
    {% if StepType.HasEndDate %}
        [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete' color:'green-600']][[ endkpi ]]
        [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
    {% endif %}
    [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}", "3455C62B-C3E7-4902-A75C-A8CF62B78F2C" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Key Performance Indicator Lava Attribute for BlockType: Step Type Detail
            RockMigrationHelper.DeleteAttribute("3455C62B-C3E7-4902-A75C-A8CF62B78F2C");

            // Key Performance Indicator Lava Attribute for BlockType: Step Program Detail
            RockMigrationHelper.DeleteAttribute("824D334A-944E-4315-A0E9-35BB20EE40D2");

            // Username Field Label Attribute for BlockType: Account Entry
            RockMigrationHelper.DeleteAttribute("0B7D0399-6A04-4687-A41B-BA033489B12A");

            // Require Email For Username Attribute for BlockType: Account Entry
            RockMigrationHelper.DeleteAttribute("B4EF9FB5-3EF2-46E1-A1DE-926E16016598");

            // Gender Attribute for BlockType: Register
            RockMigrationHelper.DeleteAttribute("AAB94EB6-AAD8-4D68-8B56-22F192DAA7E1");

            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("C281975D-524F-4ECB-9415-7D4252D4EE6A");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("7FCF321D-2E91-421E-9F85-CF55907F6CCE");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("B6791F9B-EABE-46DF-B0BC-E0590C1D8753");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("AD562161-2856-4C00-A411-97C11E9FE914");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("B93BA07D-CE96-418E-8649-1EE6F4E54F76");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("723AC60A-D2E4-4664-9C6E-4596BC1D2989");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("1D1CA741-FD96-4076-9970-EEA451F464D4");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("5228E42F-3ED8-4EC8-9832-585591ED7534");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("17A0B3A0-3612-4EE3-A0FD-A712988FCEEA");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("54E777E3-CA59-4A2D-BD47-7DEBECB22BE9");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("7A072EB1-FA6E-4603-A9A7-3BC1017060F8");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("DF27B70C-BBEC-408F-B8BE-2DC39FE7C45B");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("B5160D1C-BB15-4E6C-8267-C844B190CD49");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("C2B23AB9-1E78-497A-9338-BD1A1A2E0463");

            // Exclude Connection Types Attribute for BlockType: My Connection Opportunities Lava
            RockMigrationHelper.DeleteAttribute("F300B10E-A005-4260-9146-D1FD4D5287A7");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("A8F43F38-7316-4C76-A1E8-4B55799BB41F"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("3830C4BF-09FE-4BD3-8137-5803EDDC4E6C"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("AEE9A39B-2945-4E86-9A2B-ED6ED49403E1"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("356AA6B5-2D19-4496-B7EE-E0B13E36D75A"); // Structured Content View
        }

        /// <summary>
        /// GJ: Update Parallax Documentation
        /// </summary>
        private void UpdateParallaxDocumentation()
        {
            Sql( MigrationSQL._202012151955324_Rollup_1215_parallaxupdate );
        }

        /// <summary>
        /// Updates the statement generator download link up.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.12.1/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// Updates the statement generator download link down.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.10.0/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }
    }
}

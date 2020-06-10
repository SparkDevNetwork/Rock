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
    public partial class Rollup_0609 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateStructuredContentEditorDefinedValues();
            UpdateWorkflowProcessingIntervals();
            UpdatespDbaRebuildIndexes();
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
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "EA048472-D4BD-4966-83B7-00695D458685");
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "F6131B1A-3464-4E8A-BB14-C558FF2D63EC");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6131B1A-3464-4E8A-BB14-C558FF2D63EC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "CD802FC7-448E-4DAF-854F-DFBC3F812C83" );
            // Attrib for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6131B1A-3464-4E8A-BB14-C558FF2D63EC", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "88FB2AD3-2120-4E82-A11A-61DB448B77C2" );
            // Attrib for BlockType: Group Edit:Show Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Header", "EnableHeader", "Show Header", @"If enabled, a 'Group Details' header will be displayed.", 0, @"True", "E5000EFA-D3A4-48C3-AB7D-7161AFA18FC7" );
            // Attrib for BlockType: Group Member Edit:Show Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Header", "EnableHeader", "Show Header", @"If enabled, a 'Group Member Edit' header will be displayed.", 0, @"True", "096EB064-699C-492E-A4A6-8DB5B4F288B1" );
            // Attrib for BlockType: Prayer Request Details:Show Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Header", "EnableHeader", "Show Header", @"If enabled, a 'Add/Edit Prayer Request' header will be displayed.", 2, @"True", "D847C303-4FF4-4A84-A1EB-4734DCA3F886" );
            // Attrib for BlockType: Service Metrics Entry:Insert 0 for Blank Items
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Insert 0 for Blank Items", "DefaultToZero", "Insert 0 for Blank Items", @"If enabled, a zero will be added to any metrics that are left empty when entering data.", 5, @"false", "2E147365-0BB3-4B1B-B18F-12CB262F8D24" );
            // Attrib for BlockType: Service Metrics Entry:Metric Date Determined By
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Metric Date Determined By", "MetricDateDeterminedBy", "Metric Date Determined By", @"This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.", 6, @"0", "62BA1A48-EEE3-4569-8F56-717087CB1B88" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Service Metrics Entry:Metric Date Determined By
            RockMigrationHelper.DeleteAttribute("62BA1A48-EEE3-4569-8F56-717087CB1B88");
            // Attrib for BlockType: Service Metrics Entry:Insert 0 for Blank Items
            RockMigrationHelper.DeleteAttribute("2E147365-0BB3-4B1B-B18F-12CB262F8D24");
            // Attrib for BlockType: Prayer Request Details:Show Header
            RockMigrationHelper.DeleteAttribute("D847C303-4FF4-4A84-A1EB-4734DCA3F886");
            // Attrib for BlockType: Group Member Edit:Show Header
            RockMigrationHelper.DeleteAttribute("096EB064-699C-492E-A4A6-8DB5B4F288B1");
            // Attrib for BlockType: Group Edit:Show Header
            RockMigrationHelper.DeleteAttribute("E5000EFA-D3A4-48C3-AB7D-7161AFA18FC7");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.DeleteAttribute("88FB2AD3-2120-4E82-A11A-61DB448B77C2");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.DeleteAttribute("CD802FC7-448E-4DAF-854F-DFBC3F812C83");
            RockMigrationHelper.DeleteBlockType("F6131B1A-3464-4E8A-BB14-C558FF2D63EC"); // Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("EA048472-D4BD-4966-83B7-00695D458685"); // Structured Content View
        }
    
        public void UpdateStructuredContentEditorDefinedValues()
        {
            RockMigrationHelper.UpdateDefinedValue("E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA","Default","{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     }, code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }","09B25845-B879-4E69-87E9-003F9380B8DD",false);
            RockMigrationHelper.UpdateDefinedValue("E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA","Message Notes","{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     },     fillin: {     class:  FillIn,     shortcut: 'CMD+SHIFT+F'     },     code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     note: {     class:  NoteTool,     shortcut: 'CMD+SHIFT+N'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }","31C63FB9-1365-4EEF-851D-8AB9A188A06C",false);
        }
    
        /// <summary>
        /// SK: Update ProcessingIntervalSeconds for few Workflow Types
        /// </summary>
        private void UpdateWorkflowProcessingIntervals()
        {
            // Update ProcessingIntervalSeconds for several Workflow Types and the logging level for one
            Sql(
                @"UPDATE
                    [WorkflowType]
                SET [ProcessingIntervalSeconds]=3600
                WHERE
                    [Guid] IN (
                                '655BE2A4-2735-4CF9-AEC8-7EF5BE92724C',
                                '221bf486-a82c-40a7-85b7-bb44da45582f',
                                '036f2f0b-c2dc-49d0-a17b-ccdac7fc71e2',
                                '16d12ef7-c546-4039-9036-b73d118edc90',
                                'f5af8224-44dc-4918-aab7-c7c9a5a6338d',
                                '9bc07356-3b2f-4bff-9320-fa8f3a28fc39',
                                '31ddc001-c91a-4418-b375-cab1475f7a62'
                                )
                    AND ([ProcessingIntervalSeconds] IS NULL OR [ProcessingIntervalSeconds] = 0)

                UPDATE
                    [WorkflowType]
                SET [LoggingLevel] = 0
                WHERE
                    [Guid] = '655BE2A4-2735-4CF9-AEC8-7EF5BE92724C'
                    AND [LoggingLevel] = 3" );
        }

        /// <summary>
        /// ED: Update sproc spDbaRebuildIndexes to fix rebuild index command options for index types of Spatial and XML
        /// </summary>
        private void UpdatespDbaRebuildIndexes()
        {
            Sql( MigrationSQL._202006091841047_Rollup_0609_spDbaRebuildIndexes );
        }
    }
}

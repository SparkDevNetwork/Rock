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
    public partial class CodeGenerated_20230420 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Utility.RealTimeVisualizer
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Utility.RealTimeVisualizer", "Real Time Visualizer", "Rock.Blocks.Utility.RealTimeVisualizer, Rock.Blocks, Version=1.16.0.1, Culture=neutral, PublicKeyToken=null", false, false, "77F4EA4A-CE87-4309-A7A0-2A1A75AB61CD" );

            // Add/Update Obsidian Block Type
            //   Name:RealTime Visualizer
            //   Category:Utility
            //   EntityType:Rock.Blocks.Utility.RealTimeVisualizer
            RockMigrationHelper.UpdateMobileBlockType( "RealTime Visualizer", "Displays RealTime events from Rock with custom formatting options.", "Rock.Blocks.Utility.RealTimeVisualizer", "Utility", "CE185083-DF13-48F9-8C97-83EDA1CA65C2" );

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "616A2217-0BCE-432E-8CC2-5B6B10D90869".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "ECDB533F-3DAC-43A3-9B25-734133B5C4F1" );

            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "AFB2AEFC-1BA0-4EB4-BC14-E876BD3677C6" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Additional Custom Actions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Custom Actions", "AdditionalCustomActions", "Additional Custom Actions", @"
Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current business's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an 
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>", 7, @"", "26632909-7199-4FB8-8480-7FB82F37A002" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Workflow Actions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Actions", "WorkflowActions", "Workflow Actions", @"The workflows to make available as actions.", 6, @"", "15F084CE-D4B2-4980-A58F-D3E23D635579" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Include Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Include Group Types", "IncludeGroupTypes", "Include Group Types", @"The group types to display in the list. If none are selected, all group types will be included.", 18, @"", "E5B8D1D4-2CE2-461F-BF42-84E86AC6D23B" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Exclude Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Exclude Group Types", "ExcludeGroupTypes", "Exclude Group Types", @"The group types to exclude from the list (only valid if including all groups).", 19, @"", "3DE0FC71-0527-4028-A892-C254A3093021" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Avatar Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Avatar Style", "AvatarStyle", "Avatar Style", @"Allows control of the person photo avatar to use either an icon to represent the person's gender and age classification, or first and last name initials when the person does not have a photo.", 7, @"0", "C6CC5868-5910-4AF5-811F-BAA4C3C61C37" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Theme
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE185083-DF13-48F9-8C97-83EDA1CA65C2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Theme", "Theme", "Theme", @"The theme of the visualizer when rendering items.", 0, @"", "EF267653-8A88-4754-B250-6A7038AF2C82" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Theme Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE185083-DF13-48F9-8C97-83EDA1CA65C2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Theme Settings", "ThemeSettings", "Theme Settings", @"The custom settings for the selected theme.", 0, @"", "779175EE-A4B0-4F06-B59C-13E5863FF66E" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE185083-DF13-48F9-8C97-83EDA1CA65C2", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 1, @"", "AD9DEB9A-CA7F-46BE-88D0-0455D3D605F3" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE185083-DF13-48F9-8C97-83EDA1CA65C2", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The lava template to use when processing the message for display. The 'Message' variable will contain the message name and 'Args' variable will be an array of the message arguments.", 0, @"", "BA508716-2639-4487-9A0F-06FB440EE5F1" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Channels
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE185083-DF13-48F9-8C97-83EDA1CA65C2", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Channels", "Channels", "Channels", @"The list of topics and channels to subscribe to.", 0, @"", "A4467C49-2C2C-40AA-9CF7-D34B861F4A92" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "ECDB533F-3DAC-43A3-9B25-734133B5C4F1", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Theme Settings
            RockMigrationHelper.DeleteAttribute( "779175EE-A4B0-4F06-B59C-13E5863FF66E" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Theme
            RockMigrationHelper.DeleteAttribute( "EF267653-8A88-4754-B250-6A7038AF2C82" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "AD9DEB9A-CA7F-46BE-88D0-0455D3D605F3" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Template
            RockMigrationHelper.DeleteAttribute( "BA508716-2639-4487-9A0F-06FB440EE5F1" );

            // Attribute for BlockType
            //   BlockType: RealTime Visualizer
            //   Category: Utility
            //   Attribute: Channels
            RockMigrationHelper.DeleteAttribute( "A4467C49-2C2C-40AA-9CF7-D34B861F4A92" );

            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute( "AFB2AEFC-1BA0-4EB4-BC14-E876BD3677C6" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Exclude Group Types
            RockMigrationHelper.DeleteAttribute( "3DE0FC71-0527-4028-A892-C254A3093021" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Include Group Types
            RockMigrationHelper.DeleteAttribute( "E5B8D1D4-2CE2-461F-BF42-84E86AC6D23B" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Additional Custom Actions
            RockMigrationHelper.DeleteAttribute( "26632909-7199-4FB8-8480-7FB82F37A002" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Workflow Actions
            RockMigrationHelper.DeleteAttribute( "15F084CE-D4B2-4980-A58F-D3E23D635579" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Avatar Style
            RockMigrationHelper.DeleteAttribute( "C6CC5868-5910-4AF5-811F-BAA4C3C61C37" );

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "ECDB533F-3DAC-43A3-9B25-734133B5C4F1" );

            // Delete BlockType 
            //   Name: RealTime Visualizer
            //   Category: Utility
            //   Path: -
            //   EntityType: Real Time Visualizer
            RockMigrationHelper.DeleteBlockType( "CE185083-DF13-48F9-8C97-83EDA1CA65C2" );
        }
    }
}

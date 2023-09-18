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
    public partial class CodeGenerated_20230824 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.BinaryFileDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.BinaryFileDetail", "Binary File Detail", "Rock.Blocks.Core.BinaryFileDetail, Rock.Blocks, Version=1.16.0.10, Culture=neutral, PublicKeyToken=null", false, false, "DE112A87-A1BC-46CC-BEA1-D5D658AB1E3A" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.InteractionChannelDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.InteractionChannelDetail", "Interaction Channel Detail", "Rock.Blocks.Core.InteractionChannelDetail, Rock.Blocks, Version=1.16.0.10, Culture=neutral, PublicKeyToken=null", false, false, "9438E0FE-F7AB-48D5-8AB8-D54336D30FBD" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Engagement.StreakTypeExclusionDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakTypeExclusionDetail", "Streak Type Exclusion Detail", "Rock.Blocks.Engagement.StreakTypeExclusionDetail, Rock.Blocks, Version=1.16.0.10, Culture=neutral, PublicKeyToken=null", false, false, "0667F91D-E7FC-44E6-A969-EEBBF99802B2" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialGatewayDetail", "Financial Gateway Detail", "Rock.Blocks.Finance.FinancialGatewayDetail, Rock.Blocks, Version=1.16.0.10, Culture=neutral, PublicKeyToken=null", false, false, "68CC9376-8123-4749-ACA0-1E7ED8459704" );

            // Add/Update Obsidian Block Type              
            //   Name:Binary File Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.BinaryFileDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Binary File Detail", "Shows the details of a particular binary file item.", "Rock.Blocks.Core.BinaryFileDetail", "Core", "D0D4FCB2-E21E-4287-9416-81BA60B90F40" );

            // Add/Update Obsidian Block Type              
            //   Name:Interaction Channel Detail              
            //   Category:Reporting              
            //   EntityType:Rock.Blocks.Core.InteractionChannelDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Interaction Channel Detail", "Displays the details of a particular interaction channel.", "Rock.Blocks.Core.InteractionChannelDetail", "Reporting", "2EFA1F9D-7062-466A-A8F3-9DCDBFF054E9" );

            // Add/Update Obsidian Block Type              
            //   Name:Streak Type Exclusion Detail              
            //   Category:Streaks              
            //   EntityType:Rock.Blocks.Engagement.StreakTypeExclusionDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak Type Exclusion Detail", "Displays the details of the given Exclusion for editing.", "Rock.Blocks.Engagement.StreakTypeExclusionDetail", "Streaks", "D8B2132D-8725-47FF-84CD-C86C163ABE4D" );

            // Add/Update Obsidian Block Type              
            //   Name:Gateway Detail              
            //   Category:Finance              
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Gateway Detail", "Displays the details of the given financial gateway.", "Rock.Blocks.Finance.FinancialGatewayDetail", "Finance", "C12C615C-384D-478E-892D-0F353E2EF180" );

            // Add Block               
            //  Block Name: Membership              
            //  Page Name: Extended Attributes V1              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "A0DD5CC1-E990-4230-A03A-D9270AF62C7A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "4A6B9FAE-2913-4F8A-AA76-C2DEFA8EFA69" );

            // Attribute for BlockType              
            //   BlockType: Event Registration Wizard              
            //   Category: Event              
            //   Attribute: Enable Existing Group Selection               
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Existing Group Selection ", "EnableExistingGroupSelection ", "Enable Existing Group Selection ", @"When enabled, an optional toggle switch will allow choosing an existing group or creating a new group.", 16, @"False", "8C0EE83B-A110-43F6-B519-1902D162784A" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Show Binary File Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0D4FCB2-E21E-4287-9416-81BA60B90F40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Binary File Type", "ShowBinaryFileType", "Show Binary File Type", @"", 0, @"False", "B2CD663E-7E40-446C-84B5-1ECFDC911C72" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Edit Label Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0D4FCB2-E21E-4287-9416-81BA60B90F40", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Label Page", "EditLabelPage", "Edit Label Page", @"Page used to edit and test the contents of a label file.", 0, @"", "D5AA7DC9-E4C3-48DB-8C55-DCA0990D5C68" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Workflow              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0D4FCB2-E21E-4287-9416-81BA60B90F40", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to activate for any new file uploaded", 0, @"", "D0852833-36CD-4E36-8A21-48D92D3B6FE5" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Workflow Button Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0D4FCB2-E21E-4287-9416-81BA60B90F40", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Button Text", "WorkflowButtonText", "Workflow Button Text", @"The button text to show for the rerun workflow button.", 1, @"Rerun Workflow", "9885549E-939E-4296-8CDD-FD6C92763083" );

            // Attribute for BlockType              
            //   BlockType: Interaction Channel Detail              
            //   Category: Reporting              
            //   Attribute: Default Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EFA1F9D-7062-466A-A8F3-9DCDBFF054E9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Template", "DefaultTemplate", "Default Template", @"Lava template to use to display content", 0, @"  <div class='row'>      {% if InteractionChannel.Name != '' %}          <div class='col-md-6'>              <dl><dt>Name</dt><dd>{{ InteractionChannel.Name }}<dd/></dl>          </div>      {% endif %}      {% if InteractionChannel.ChannelTypeMediumValue != null and InteractionChannel.ChannelTypeMediumValue != '' %}          <div class='col-md-6'>              <dl><dt>Medium</dt><dd>{{ InteractionChannel.ChannelTypeMediumValue.Value }}<dd/></dl>          </div>      {% endif %}      {% if InteractionChannel.EngagementStrength != null and InteractionChannel.EngagementStrength != '' %}        <div class='col-md-6'>            <dl><dt>Engagement Strength</dt><dd>{{ InteractionChannel.EngagementStrength }}<dd/></dl>         </div>      {% endif %}      {% if InteractionChannel.RetentionDuration != null %}          <div class='col-md-6'>              <dl><dt>Retention Duration</dt><dd>{{ InteractionChannel.RetentionDuration }}<dd/></dl>          </div>      {% endif %}      {% if InteractionChannel.ComponentCacheDuration != null %}          <div class='col-md-6'>              <dl><dt>Component Cache Duration</dt><dd>{{ InteractionChannel.ComponentCacheDuration }}<dd/></dl>          </div>      {% endif %}  </div>  ", "A597FCB2-3F81-4A53-A055-0529C98E8903" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Show SMS Opt-in              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show SMS Opt-in", "DisplaySmsOptIn", "Show SMS Opt-in", @"If this option will show the SMS Opt-In text for the selection.", 15, @"Hide", "0695094E-0FC8-4ADE-BC61-F4D35479D427" );

            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */              
            RockMigrationHelper.AddBlockAttributeValue( "4A6B9FAE-2913-4F8A-AA76-C2DEFA8EFA69", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Show SMS Opt-in              
            RockMigrationHelper.DeleteAttribute( "0695094E-0FC8-4ADE-BC61-F4D35479D427" );

            // Attribute for BlockType              
            //   BlockType: Interaction Channel Detail              
            //   Category: Reporting              
            //   Attribute: Default Template              
            RockMigrationHelper.DeleteAttribute( "A597FCB2-3F81-4A53-A055-0529C98E8903" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Workflow Button Text              
            RockMigrationHelper.DeleteAttribute( "9885549E-939E-4296-8CDD-FD6C92763083" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Workflow              
            RockMigrationHelper.DeleteAttribute( "D0852833-36CD-4E36-8A21-48D92D3B6FE5" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Edit Label Page              
            RockMigrationHelper.DeleteAttribute( "D5AA7DC9-E4C3-48DB-8C55-DCA0990D5C68" );

            // Attribute for BlockType              
            //   BlockType: Binary File Detail              
            //   Category: Core              
            //   Attribute: Show Binary File Type              
            RockMigrationHelper.DeleteAttribute( "B2CD663E-7E40-446C-84B5-1ECFDC911C72" );

            // Attribute for BlockType              
            //   BlockType: Event Registration Wizard              
            //   Category: Event              
            //   Attribute: Enable Existing Group Selection               
            RockMigrationHelper.DeleteAttribute( "8C0EE83B-A110-43F6-B519-1902D162784A" );

            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "4A6B9FAE-2913-4F8A-AA76-C2DEFA8EFA69" );

            // Delete BlockType               
            //   Name: Gateway Detail              
            //   Category: Finance              
            //   Path: -              
            //   EntityType: Financial Gateway Detail              
            RockMigrationHelper.DeleteBlockType( "C12C615C-384D-478E-892D-0F353E2EF180" );

            // Delete BlockType               
            //   Name: Streak Type Exclusion Detail              
            //   Category: Streaks              
            //   Path: -              
            //   EntityType: Streak Type Exclusion Detail              
            RockMigrationHelper.DeleteBlockType( "D8B2132D-8725-47FF-84CD-C86C163ABE4D" );

            // Delete BlockType               
            //   Name: Interaction Channel Detail              
            //   Category: Reporting              
            //   Path: -              
            //   EntityType: Interaction Channel Detail              
            RockMigrationHelper.DeleteBlockType( "2EFA1F9D-7062-466A-A8F3-9DCDBFF054E9" );

            // Delete BlockType               
            //   Name: Binary File Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Binary File Detail              
            RockMigrationHelper.DeleteBlockType( "D0D4FCB2-E21E-4287-9416-81BA60B90F40" );
        }
    }
}

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
    public partial class CodeGenerated_20250513 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaApplicationContent
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LavaApplicationContent", "Lava Application Content", "Rock.Blocks.Cms.LavaApplicationContent, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "ACB4674B-22EC-FE88-48D0-2EEDB6536B85" );

            // Add/Update Obsidian Block Type
            //   Name:Lava Application Content
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.LavaApplicationContent
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Application Content", "Displays the details of a particular lava application.", "Rock.Blocks.Cms.LavaApplicationContent", "CMS", "9D863719-3E92-8681-4DDC-DE63ACEFEDF1" );

            // Attribute for BlockType
            //   BlockType: Log In
            //   Category: Mobile > Cms
            //   Attribute: Use Embedded Web View For External Authentication
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Embedded Web View For External Authentication", "UseEmbeddedWebViewForExternalAuthentication", "Use Embedded Web View For External Authentication", @"When enabled, the application will use an embedded web view for the external authentication process. This must be disabled in cases where you want to offer Fido2/Passkey support.", 13, @"True", "0F343D85-817F-4321-9EEE-51F61F914FB9" );


            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: CMS
            //   Attribute: Application
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D863719-3E92-8681-4DDC-DE63ACEFEDF1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Application", "Application", "Application", @"The Lava application to target. This is optional, but if provided you'll have access to the application in your Lava.", 0, @"", "F776489E-7DEC-445C-853C-6FFFD91D0BEF" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: CMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D863719-3E92-8681-4DDC-DE63ACEFEDF1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"Your Lava template for the page. You can access your application's configuration rigging using the 'ConfigurationRigging' merge field.", 1, @"", "6AA40762-7516-43E8-9CE8-A5C6BDC4506A" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: CMS
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D863719-3E92-8681-4DDC-DE63ACEFEDF1", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this HTML block.", 2, @"", "8FE05D9D-50D7-4D74-83D4-B9B71285A5AA" );

            // Add Block Attribute Value
            //   Block: Rock Logs
            //   BlockType: Logs
            //   Category: Core
            //   Block Location: Page=Rock Logs, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "5CE211F3-C23D-4AE0-BB55-C248E1FBEEFE", "B8D9E0E6-7CE0-4E83-8353-25F6B6D97A73", @"" );

            // Add Block Attribute Value
            //   Block: Rock Logs
            //   BlockType: Logs
            //   Category: Core
            //   Block Location: Page=Rock Logs, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "5CE211F3-C23D-4AE0-BB55-C248E1FBEEFE", "81CB1455-878F-44A7-A58B-49957AEDB13D", @"False" );

            // Add Block Attribute Value
            //   Block: Asset Storage Provider List
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Block Location: Page=Asset Storage Providers, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "09A03EC6-AFD1-4F87-8634-04A8C7291ADA", "6608C2A6-183F-4C0E-9C1A-C222CBC90BFE", @"" );

            // Add Block Attribute Value
            //   Block: Asset Storage Provider List
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Block Location: Page=Asset Storage Providers, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "09A03EC6-AFD1-4F87-8634-04A8C7291ADA", "7660A9EC-BCFF-45D6-B102-EC02DEC6E41F", @"False" );

            // Add Block Attribute Value
            //   Block: Short Link Click List
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Block Location: Page=Link, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "BC211A4C-A89D-44B3-969F-4470F2118DE1", "37A8433A-2196-4611-AE4C-081C14CEE7D5", @"" );

            // Add Block Attribute Value
            //   Block: Short Link Click List
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Block Location: Page=Link, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "BC211A4C-A89D-44B3-969F-4470F2118DE1", "FA5512AE-B9A0-4813-ABE0-2C5CBF592FF4", @"False" );

            // Add Block Attribute Value
            //   Block: Achievement Attempt List
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Block Location: Page=Achievement Type, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B47F6BD8-7330-4839-8B9A-BDBF6F29FD39", "AD008988-0202-4F44-905F-1D2216CD34D3", @"" );

            // Add Block Attribute Value
            //   Block: Achievement Attempt List
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Block Location: Page=Achievement Type, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B47F6BD8-7330-4839-8B9A-BDBF6F29FD39", "B453DCBE-ABFD-421D-80FD-3E38F958B4C1", @"False" );

            // Add Block Attribute Value
            //   Block: Achievement Attempt List
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Block Location: Page=Achievement Attempts, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "81BCE077-4C4D-425E-BC76-DEB9530666CB", "AD008988-0202-4F44-905F-1D2216CD34D3", @"" );

            // Add Block Attribute Value
            //   Block: Achievement Attempt List
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Block Location: Page=Achievement Attempts, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "81BCE077-4C4D-425E-BC76-DEB9530666CB", "B453DCBE-ABFD-421D-80FD-3E38F958B4C1", @"False" );

            // Add Block Attribute Value
            //   Block: Achievement Type List
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Block Location: Page=Achievements, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "204BE8D7-6AD8-4A24-82F5-C8C0181EB070", "546AC175-EF15-4C5C-8992-9A34F9774B9F", @"" );

            // Add Block Attribute Value
            //   Block: Achievement Type List
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Block Location: Page=Achievements, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "204BE8D7-6AD8-4A24-82F5-C8C0181EB070", "9927E638-061F-452C-84AC-0A6871D01388", @"False" );

            // Add Block Attribute Value
            //   Block: Short Link List
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Block Location: Page=Short Links, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "ACE2308A-A7E9-4A97-83E8-F5D5B51D3741", "967656F1-50FC-4019-8BA3-02C57BEE5354", @"" );

            // Add Block Attribute Value
            //   Block: Short Link List
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Block Location: Page=Short Links, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "ACE2308A-A7E9-4A97-83E8-F5D5B51D3741", "91B21F23-C661-4540-AF9B-27D31F8C47C2", @"False" );
            RockMigrationHelper.UpdateFieldType( "Adaptive Message", "", "Rock", "Rock.Field.Types.AdaptiveMessageFieldType", "55CC533A-99D2-418F-B93E-73EA098ABDAE" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Log In
            //   Category: Mobile > Cms
            //   Attribute: Use Embedded Web View For External Authentication
            RockMigrationHelper.DeleteAttribute( "0F343D85-817F-4321-9EEE-51F61F914FB9" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: CMS
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "8FE05D9D-50D7-4D74-83D4-B9B71285A5AA" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: CMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "6AA40762-7516-43E8-9CE8-A5C6BDC4506A" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: CMS
            //   Attribute: Application
            RockMigrationHelper.DeleteAttribute( "F776489E-7DEC-445C-853C-6FFFD91D0BEF" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Asset Manager
            RockMigrationHelper.DeleteAttribute( "CC9E4935-7E4A-4259-868D-4E602CA0CA67" );

            // Delete BlockType 
            //   Name: Lava Application Content
            //   Category: CMS
            //   Path: -
            //   EntityType: Lava Application Content
            RockMigrationHelper.DeleteBlockType( "9D863719-3E92-8681-4DDC-DE63ACEFEDF1" );
        }
    }
}

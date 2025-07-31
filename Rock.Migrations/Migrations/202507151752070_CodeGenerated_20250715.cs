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
    public partial class CodeGenerated_20250715 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.RequestFilterList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.RequestFilterList", "Request Filter List", "Rock.Blocks.Cms.RequestFilterList, Rock.Blocks, Version=18.0.8.0, Culture=neutral, PublicKeyToken=null", false, false, "03A39945-3E6E-4A40-B537-4DD4516BC8BF" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.PowerBiAccountRegister
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.PowerBiAccountRegister", "Power Bi Account Register", "Rock.Blocks.Reporting.PowerBiAccountRegister, Rock.Blocks, Version=18.0.8.0, Culture=neutral, PublicKeyToken=null", false, false, "B96E7E86-64E5-4A37-9035-C62908A14E71" );

            // Add/Update Obsidian Block Type
            //   Name:Request Filter List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.RequestFilterList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Request Filter List", "Displays a list of request filters.", "Rock.Blocks.Cms.RequestFilterList", "CMS", "719542DF-4D02-4DF5-BAB0-7EB205540090" );

            // Add/Update Obsidian Block Type
            //   Name:Power Bi Account Register
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.PowerBiAccountRegister
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Power Bi Account Register", "This block registers a Power BI account for Rock to use.", "Rock.Blocks.Reporting.PowerBiAccountRegister", "Reporting", "6373C4CC-65CC-41E9-9B52-D93D0C2542A6" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Chat Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Chat Style", "ChatViewStyle", "Chat Style", @"Choose how chat conversations are displayed. 'Conversational' offers a simple, text-message feel that's great for direct messages and small groups. 'Community' provides a more structured layout, ideal for group discussions and larger conversations.", 0, @"0", "66403533-20B1-4685-A8B8-4E276AB26A62" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "719542DF-4D02-4DF5-BAB0-7EB205540090", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the request filter details.", 0, @"", "E5C23B38-0AD0-472D-9A47-0C54B5DFBFF7" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "719542DF-4D02-4DF5-BAB0-7EB205540090", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6BBFCF9D-0F3A-40BB-9E12-1E2E2247A074" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "719542DF-4D02-4DF5-BAB0-7EB205540090", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D104264B-143E-461D-83D0-8F11B2880701" );

            // Add Block Attribute Value
            //   Block: Audit Information List
            //   BlockType: Audit List
            //   Category: Core
            //   Block Location: Page=Audit Information, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D33DF767-FAF3-4C66-8CFC-263CB4C2F1D4", "CB08E2F1-21CB-4972-92C4-4DEAB02A6CD1", @"" );

            // Add Block Attribute Value
            //   Block: Audit Information List
            //   BlockType: Audit List
            //   Category: Core
            //   Block Location: Page=Audit Information, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D33DF767-FAF3-4C66-8CFC-263CB4C2F1D4", "7336C9B9-44FE-4098-B344-27588078F819", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Chat Style
            RockMigrationHelper.DeleteAttribute( "66403533-20B1-4685-A8B8-4E276AB26A62" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D104264B-143E-461D-83D0-8F11B2880701" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "6BBFCF9D-0F3A-40BB-9E12-1E2E2247A074" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E5C23B38-0AD0-472D-9A47-0C54B5DFBFF7" );

            // Delete BlockType 
            //   Name: Power Bi Account Register
            //   Category: Reporting
            //   Path: -
            //   EntityType: Power Bi Account Register
            RockMigrationHelper.DeleteBlockType( "6373C4CC-65CC-41E9-9B52-D93D0C2542A6" );

            // Delete BlockType 
            //   Name: Request Filter List
            //   Category: CMS
            //   Path: -
            //   EntityType: Request Filter List
            RockMigrationHelper.DeleteBlockType( "719542DF-4D02-4DF5-BAB0-7EB205540090" );
        }
    }
}

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

    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20231005 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Finance.FinancialBatchList              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialBatchList", "Financial Batch List", "Rock.Blocks.Finance.FinancialBatchList, Rock.Blocks, Version=1.16.1.11, Culture=neutral, PublicKeyToken=null", false, false, "A68DD358-1392-475F-92B4-DEA544FF219E" );

            // Add/Update Obsidian Block Type              
            //   Name:Financial Batch List              
            //   Category:Finance              
            //   EntityType:Rock.Blocks.Finance.FinancialBatchList              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Batch List", "Displays a list of financial batches.", "Rock.Blocks.Finance.FinancialBatchList", "Finance", "F1950524-E959-440F-9CF6-1A8B9B7527D8" );

            // Attribute for BlockType              
            //   BlockType: Person Profile               
            //   Category: Check-in > Manager              
            //   Attribute: Snippet Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Snippet Category", "SnippetCategory", "Snippet Category", @"The category to show SMS Snippets for (leave blank for all categories).", 7, @"", "5B60A3CA-D90C-454D-9C80-5891C7CCC819" );

            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Attribute: Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "Category", @"The Attribute Categories to display attributes from", 0, @"", "8142039C-13B6-4DEF-9338-0C65AFF03605" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "5D91EB38-C1BB-495D-8CC6-89F31776087E" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1950524-E959-440F-9CF6-1A8B9B7527D8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the financial batch details.", 0, @"", "0BBEEA8B-3F4E-4DA4-9025-08A12E783B3D" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: Show Accounting System Code              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1950524-E959-440F-9CF6-1A8B9B7527D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Accounting System Code", "ShowAccountingCode", "Show Accounting System Code", @"Should the code from the accounting system column be displayed.", 1, @"False", "F2EFC1F7-BF82-4F7B-B7EB-BA8BE667283A" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: Show Accounts Column              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1950524-E959-440F-9CF6-1A8B9B7527D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Accounts Column", "ShowAccountsColumn", "Show Accounts Column", @"Should the accounts column be displayed.", 2, @"True", "F09865EA-6F18-41F2-AE0E-CC1B7AD1C5B5" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1950524-E959-440F-9CF6-1A8B9B7527D8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "82FED51A-7896-4987-83C0-EC56D9D27A2F" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1950524-E959-440F-9CF6-1A8B9B7527D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D9256FCE-3646-4F40-9BEF-6D8B12FE0E33" );

            // Attribute for BlockType              
            //   BlockType: Group Member List              
            //   Category: Mobile > Groups              
            //   Attribute: Show Unknown as Gender Filter Option              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Unknown as Gender Filter Option", "ShowUnknownAsGenderFilterOption", "Show Unknown as Gender Filter Option", @"If enabled then 'Unknown' will be shown as a Gender filter option.", 9, @"True", "7F3E38F0-41E1-4DA9-BFDC-AEC067E1240B" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Group Member List              
            //   Category: Mobile > Groups              
            //   Attribute: Show Unknown as Gender Filter Option              
            RockMigrationHelper.DeleteAttribute( "7F3E38F0-41E1-4DA9-BFDC-AEC067E1240B" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.DeleteAttribute( "D9256FCE-3646-4F40-9BEF-6D8B12FE0E33" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.DeleteAttribute( "82FED51A-7896-4987-83C0-EC56D9D27A2F" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: Show Accounts Column              
            RockMigrationHelper.DeleteAttribute( "F09865EA-6F18-41F2-AE0E-CC1B7AD1C5B5" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: Show Accounting System Code              
            RockMigrationHelper.DeleteAttribute( "F2EFC1F7-BF82-4F7B-B7EB-BA8BE667283A" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Attribute: Detail Page              
            RockMigrationHelper.DeleteAttribute( "0BBEEA8B-3F4E-4DA4-9025-08A12E783B3D" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "5D91EB38-C1BB-495D-8CC6-89F31776087E" );

            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Attribute: Category              
            RockMigrationHelper.DeleteAttribute( "8142039C-13B6-4DEF-9338-0C65AFF03605" );

            // Attribute for BlockType              
            //   BlockType: Person Profile               
            //   Category: Check-in > Manager              
            //   Attribute: Snippet Category              
            RockMigrationHelper.DeleteAttribute( "5B60A3CA-D90C-454D-9C80-5891C7CCC819" );

            // Delete BlockType               
            //   Name: Financial Batch List              
            //   Category: Finance              
            //   Path: -              
            //   EntityType: Financial Batch List              
            RockMigrationHelper.DeleteBlockType( "F1950524-E959-440F-9CF6-1A8B9B7527D8" );
        }
    }
}

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
    public partial class CodeGenerated_20240606 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPledgeList", "Financial Pledge List", "Rock.Blocks.Finance.FinancialPledgeList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "8B1663EB-B5CB-4C78-B0C6-ED14E173E4C0" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Pledge List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Pledge List", "Displays a list of financial pledges.", "Rock.Blocks.Finance.FinancialPledgeList", "Finance", "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" );


            // Attribute for BlockType
            //   BlockType: Giving Type Context Setter
            //   Category: Finance
            //   Attribute: Display Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "57B00D03-1CDC-4492-95CF-7BD127CE61F0", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Type", "DisplayType", "Display Type", @"Determines how the picker options are displayed, either in a dropdown or as buttons.", 0, @"buttons", "41B2857C-73D7-4371-AAFD-2D1F61FD3C3A" );


            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "9BA6DDD6-E511-4CEB-8E65-3201FDE2F715" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "A7AE994B-4A18-48D9-9FBF-04CE9C00426A" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Column", "ShowAccountsColumn", "Show Account Column", @"Allows the account column to be hidden.", 1, @"True", "D685AFAE-3F10-4C4C-A19E-F483075774F0" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Date Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Date Column", "ShowLastModifiedDateColumn", "Show Last Modified Date Column", @"Allows the Last Modified Date column to be hidden.", 2, @"True", "E6FBE09B-437C-4281-89B4-2C323283BA64" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Group Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Column", "ShowGroupColumn", "Show Group Column", @"Allows the group column to be hidden.", 3, @"False", "189D6D31-8A92-43C9-A42D-DE44C663F1F9" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Limit Pledges To Current Person
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Pledges To Current Person", "LimitPledgesToCurrentPerson", "Limit Pledges To Current Person", @"Limit the results to pledges for the current person.", 4, @"False", "F2E7D073-ED8C-485B-8597-8F62203134F1" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Summary", "ShowAccountSummary", "Show Account Summary", @"Should the account summary be displayed at the bottom of the list?", 5, @"False", "8F62CD6A-B740-47E2-8B3F-83A3CF4E06B2" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"Limit the results to pledges that match the selected accounts.", 5, @"", "237658C7-0DED-4BE1-8026-613E155B23B5" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Hide Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Amount", "HideAmount", "Hide Amount", @"Allows the amount column to be hidden.", 6, @"False", "F9C562AE-EDC6-4B96-876D-933DBA58E675" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Person Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Person Filter", "ShowPersonFilter", "Show Person Filter", @"Allows person filter to be hidden.", 0, @"True", "A7F66BEA-9B90-40B4-9E86-03836EF9BF74" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Filter", "ShowAccountFilter", "Show Account Filter", @"Allows account filter to be hidden.", 1, @"True", "CEEE570C-013F-47DB-99F4-D3D00C5200DC" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Date Range Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "Show Date Range Filter", @"Allows date range filter to be hidden.", 2, @"True", "884EE556-66A1-4F65-B037-2CD6D0964315" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Filter", "ShowLastModifiedFilter", "Show Last Modified Filter", @"Allows last modified filter to be hidden.", 3, @"True", "6CE0FF15-C6E7-4667-ADA5-5F6D3AA71D90" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "711B6CCF-A999-4582-B1A9-770BD9BAF963" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "200AFB5A-655F-4206-858B-59376CB96856" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "200AFB5A-655F-4206-858B-59376CB96856" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "711B6CCF-A999-4582-B1A9-770BD9BAF963" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Filter
            RockMigrationHelper.DeleteAttribute( "6CE0FF15-C6E7-4667-ADA5-5F6D3AA71D90" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Date Range Filter
            RockMigrationHelper.DeleteAttribute( "884EE556-66A1-4F65-B037-2CD6D0964315" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Filter
            RockMigrationHelper.DeleteAttribute( "CEEE570C-013F-47DB-99F4-D3D00C5200DC" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Person Filter
            RockMigrationHelper.DeleteAttribute( "A7F66BEA-9B90-40B4-9E86-03836EF9BF74" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Hide Amount
            RockMigrationHelper.DeleteAttribute( "F9C562AE-EDC6-4B96-876D-933DBA58E675" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute( "237658C7-0DED-4BE1-8026-613E155B23B5" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Summary
            RockMigrationHelper.DeleteAttribute( "8F62CD6A-B740-47E2-8B3F-83A3CF4E06B2" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Limit Pledges To Current Person
            RockMigrationHelper.DeleteAttribute( "F2E7D073-ED8C-485B-8597-8F62203134F1" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Group Column
            RockMigrationHelper.DeleteAttribute( "189D6D31-8A92-43C9-A42D-DE44C663F1F9" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Date Column
            RockMigrationHelper.DeleteAttribute( "E6FBE09B-437C-4281-89B4-2C323283BA64" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Column
            RockMigrationHelper.DeleteAttribute( "D685AFAE-3F10-4C4C-A19E-F483075774F0" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "A7AE994B-4A18-48D9-9FBF-04CE9C00426A" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "9BA6DDD6-E511-4CEB-8E65-3201FDE2F715" );

            // Attribute for BlockType
            //   BlockType: Giving Type Context Setter
            //   Category: Finance
            //   Attribute: Display Type
            RockMigrationHelper.DeleteAttribute( "41B2857C-73D7-4371-AAFD-2D1F61FD3C3A" );

            // Delete BlockType 
            //   Name: Financial Pledge List
            //   Category: Finance
            //   Path: -
            //   EntityType: Financial Pledge List
            RockMigrationHelper.DeleteBlockType( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" );
        }
    }
}

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
    public partial class CodeGenerated_20250206 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.TransactionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Finance.TransactionList", "Transaction List", "Rock.Blocks.Types.Mobile.Finance.TransactionList, Rock, Version=17.0.36.0, Culture=neutral, PublicKeyToken=null", false, false, "4196280F-0204-4268-A19F-773336B8BEA2" );

            // Add/Update Mobile Block Type
            //   Name:Transaction List
            //   Category:Mobile > Finance
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.TransactionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Transaction List", "The Transaction List block.", "Rock.Blocks.Types.Mobile.Finance.TransactionList", "Mobile > Finance", "D29C24EA-A52B-4470-A8D9-D7082FFF19DE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialPersonSavedAccountDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPersonSavedAccountDetail", "Financial Person Saved Account Detail", "Rock.Blocks.Finance.FinancialPersonSavedAccountDetail, Rock.Blocks, Version=17.0.36.0, Culture=neutral, PublicKeyToken=null", false, false, "8E672306-427F-46D1-BF1B-08DD74CA2AF6" );

            // Add/Update Obsidian Block Type
            //   Name:Saved Account Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialPersonSavedAccountDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Saved Account Detail", "Displays the details of a particular financial person saved account.", "Rock.Blocks.Finance.FinancialPersonSavedAccountDetail", "Finance", "141278A4-EB96-4F4A-B936-AB1BACEF7AE4" );


            // Attribute for BlockType
            //   BlockType: Data View Detail
            //   Category: Reporting
            //   Attribute: Use Obsidian Components
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Obsidian Components", "UseObsidianComponents", "Use Obsidian Components", @"Switches the filter components to use Obsidian if supported.", 0, @"True", "B0B3468F-3FAF-4EEB-BA13-CEA32D2C80F3" );

            // Attribute for BlockType
            //   BlockType: Report Detail
            //   Category: Reporting
            //   Attribute: Use Obsidian Components
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Obsidian Components", "UseObsidianComponents", "Use Obsidian Components", @"Switches the filter components to use Obsidian if supported.", 0, @"True", "0B45C341-4735-41BA-88F0-1DEA21182277" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page used to view details of a saved account.", 0, @"", "5ED73BC4-3FA0-4552-8DC8-8B4BA1A5762C" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Past Years Filter Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D29C24EA-A52B-4470-A8D9-D7082FFF19DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Past Years Filter Limit", "PastYearsFilterLimit", "Past Years Filter Limit", @"Sets the maximum number of past years a user can filter when viewing transaction history.", 0, @"6", "FA0383F5-24BA-43CE-9433-F93512D90A83" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D29C24EA-A52B-4470-A8D9-D7082FFF19DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "TransactionDetail", "Detail Page", @"Page to link to when user taps on a Transaction List. TransactionDetailGuid is passed in the query string.", 1, @"", "0C4B2137-A35E-491B-95A4-23C396D8CAA7" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Give Now Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D29C24EA-A52B-4470-A8D9-D7082FFF19DE", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Give Now Action", "GiveNowAction", "Give Now Action", @"When no result are shown how should the 'Give Now' button behave.", 2, @"{""Type"": 4, ""PageGuid"": """"}", "70F30FAC-EACC-4A01-99DB-51784CE64C42" );


            // Add Block Attribute Value
            //   Block: Person Suggestion List
            //   BlockType: Person Suggestion List
            //   Category: Follow
            //   Block Location: Page=Following Suggestions, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "63709C69-076B-42F1-A04C-B7E0D4E4E4D9", "EBFF4FDD-2B25-4EDE-88D2-3BE49C3CB616", @"" );

            // Add Block Attribute Value
            //   Block: Person Suggestion List
            //   BlockType: Person Suggestion List
            //   Category: Follow
            //   Block Location: Page=Following Suggestions, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "63709C69-076B-42F1-A04C-B7E0D4E4E4D9", "0FD552A6-F665-4BFA-92BB-3BD646559373", @"False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5ED73BC4-3FA0-4552-8DC8-8B4BA1A5762C" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Give Now Action
            RockMigrationHelper.DeleteAttribute( "70F30FAC-EACC-4A01-99DB-51784CE64C42" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0C4B2137-A35E-491B-95A4-23C396D8CAA7" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Past Years Filter Limit
            RockMigrationHelper.DeleteAttribute( "FA0383F5-24BA-43CE-9433-F93512D90A83" );

            // Attribute for BlockType
            //   BlockType: Data View Detail
            //   Category: Reporting
            //   Attribute: Use Obsidian Components
            RockMigrationHelper.DeleteAttribute( "B0B3468F-3FAF-4EEB-BA13-CEA32D2C80F3" );

            // Attribute for BlockType
            //   BlockType: Report Detail
            //   Category: Reporting
            //   Attribute: Use Obsidian Components
            RockMigrationHelper.DeleteAttribute( "0B45C341-4735-41BA-88F0-1DEA21182277" );

            // Delete BlockType 
            //   Name: Transaction List
            //   Category: Mobile > Finance
            //   Path: -
            //   EntityType: Transaction List
            RockMigrationHelper.DeleteBlockType( "D29C24EA-A52B-4470-A8D9-D7082FFF19DE" );

            // Delete BlockType 
            //   Name: Saved Account Detail
            //   Category: Finance
            //   Path: -
            //   EntityType: Financial Person Saved Account Detail
            RockMigrationHelper.DeleteBlockType( "141278A4-EB96-4F4A-B936-AB1BACEF7AE4" );
        }
    }
}

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
    public partial class FinancialAccountCoreUpdates : Rock.Migrations.RockMigration
    {
        public override void Up()
        {
            // Add Page Internal Name: Financial Account Search Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.ADMINISTRATION_FINANCE, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Financial Account Search", "Financial Account Search Results Page.", SystemGuid.Page.FINANCIAL_ACCOUNT_SEARCH, "" );

            //Set Display in Nav to [Never=2]
            Sql( $"UPDATE [Page] SET DisplayInNavWhen = 2 WHERE [Guid] = '{SystemGuid.Page.FINANCIAL_ACCOUNT_SEARCH}'" );
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route Page:Financial Account Search Route:Account/Search/name
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.FINANCIAL_ACCOUNT_SEARCH, "Account/Search/name", SystemGuid.PageRoute.FINANCIAL_ACCOUNT_SEARCH );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add/Update BlockType Name: Account Search Category: Accounts Path: ~/Blocks/Finance/FinancialAccountSearch.ascx EntityType: -
            RockMigrationHelper.UpdateBlockType( "Account Search", "Handles displaying account search results and redirects to the accounts page when only one match is found.", "~/Blocks/Finance/FinancialAccountSearch.ascx", "Accounts", SystemGuid.BlockType.FINANCIAL_ACCOUNT_SEARCH );

            // Add Block Block Name: Account Search Page Name: Financial Account Search Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.FINANCIAL_ACCOUNT_SEARCH.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.FINANCIAL_ACCOUNT_SEARCH.AsGuid(), "Account Search", "Feature", @"", @"", 0, SystemGuid.Block.FINANCIAL_ACCOUNT_SEARCH );

            // Attribute for BlockType BlockType: Account Search Category: Accounts Attribute: Show Account Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.FINANCIAL_ACCOUNT_SEARCH, SystemGuid.FieldType.BOOLEAN, "Show Account Type", "ShowAccountType", "Show Account Type", @"Displays the account type in the grid.", 1, @"True", "AF14D51B-4AB8-4813-B661-DFFB1CCCAC78" );

            // Attribute for BlockType BlockType: Account Search Category: Accounts Attribute: Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.FINANCIAL_ACCOUNT_SEARCH, SystemGuid.FieldType.BOOLEAN, "Show Description", "ShowAccountDescription", "Show Description", @"Displays the account description in the grid.", 2, @"True", "8C63A7AC-4E45-4B33-93F8-63CB526CF2AD" );

            // Add Block Attribute Value Block: Account Search BlockType: Account Search Category: Accounts Block Location: Page=Financial Account Search, Site=Rock RMS Attribute: Show Account Type /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.FINANCIAL_ACCOUNT_SEARCH, "AF14D51B-4AB8-4813-B661-DFFB1CCCAC78", @"True" );

            // Add Block Attribute Value Block: Account Search BlockType: Account Search Category: Accounts Block Location: Page=Financial Account Search, Site=Rock RMS Attribute: Show Description              /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.FINANCIAL_ACCOUNT_SEARCH, "8C63A7AC-4E45-4B33-93F8-63CB526CF2AD", @"True" );

            //Account Search - Component Permissions
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Finance.FinancialAccount", 0, "View", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", 0, "62D28301-639B-46DE-80E0-EF2F533F165D" ); // EntityType:Rock.Search.Finance.FinancialAccount Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ),
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Finance.FinancialAccount", 1, "View", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", 0, "262565D8-4CAD-4584-AA5C-5B797A6F8C69" ); // EntityType:Rock.Search.Finance.FinancialAccount Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ),
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Finance.FinancialAccount", 2, "View", false, "", 1, "94F557B2-D1A4-4C5C-88BD-A10CA71C61F0" ); // EntityType:Rock.Search.Finance.FinancialAccount Group: <all users>
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType BlockType: Account Search Category: Accounts Attribute: Show Description
            RockMigrationHelper.DeleteAttribute( "8C63A7AC-4E45-4B33-93F8-63CB526CF2AD" );

            // Attribute for BlockType BlockType: Account Search Category: Accounts Attribute: Show Account Type
            RockMigrationHelper.DeleteAttribute( "AF14D51B-4AB8-4813-B661-DFFB1CCCAC78" );

            // Remove Block Name: Account Search, from Page: Financial Account Search, Site: Rock RMS from Page: Financial Account Search, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.FINANCIAL_ACCOUNT_SEARCH );

            // Delete BlockType Name: Account Search Category: Accounts Path: ~/Blocks/Finance/FinancialAccountSearch.ascx EntityType: -
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.FINANCIAL_ACCOUNT_SEARCH );

            // Delete Page Internal Name: Financial Account Search Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.FINANCIAL_ACCOUNT_SEARCH );

            //Account Search - Component Permissions
            RockMigrationHelper.DeleteSecurityAuth( "262565D8-4CAD-4584-AA5C-5B797A6F8C69" ); // EntityType:Rock.Search.Finance.FinancialAccount Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ),
            RockMigrationHelper.DeleteSecurityAuth( "62D28301-639B-46DE-80E0-EF2F533F165D" ); // EntityType:Rock.Search.Finance.FinancialAccount Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ),
            RockMigrationHelper.DeleteSecurityAuth( "94F557B2-D1A4-4C5C-88BD-A10CA71C61F0" ); // EntityType:Rock.Search.Finance.FinancialAccount Group: <all users>
        }
    }
}

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
    public partial class CodeGenerated_20250122 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.Giving
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Finance.Giving", "Giving", "Rock.Blocks.Types.Mobile.Finance.Giving, Rock, Version=17.0.35.0, Culture=neutral, PublicKeyToken=null", false, false, "A309C830-D373-4244-A8DC-7A69F9E263BE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.ScheduledTransactionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Finance.ScheduledTransactionList", "Scheduled Transaction List", "Rock.Blocks.Types.Mobile.Finance.ScheduledTransactionList, Rock, Version=17.0.35.0, Culture=neutral, PublicKeyToken=null", false, false, "7698E529-6834-46B0-BC5A-D466A6BCE4F6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.TransactionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Finance.TransactionDetail", "Transaction Detail", "Rock.Blocks.Types.Mobile.Finance.TransactionDetail, Rock, Version=17.0.35.0, Culture=neutral, PublicKeyToken=null", false, false, "3355006B-4C1F-4F85-8390-7C83C26D5C4A" );

            // Add/Update Mobile Block Type
            //   Name:Giving
            //   Category:Mobile > Finance
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.Giving
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Giving", "Allows an individual to give. Apple and Google Pay are supported.", "Rock.Blocks.Types.Mobile.Finance.Giving", "Mobile > Finance", "AE11559B-03C0-42CE-B8B5-CE9C1027E650" );

            // Add/Update Mobile Block Type
            //   Name:Scheduled Transaction List
            //   Category:Mobile > Finance
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.ScheduledTransactionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Transaction List", "The Scheduled Transaction List block.", "Rock.Blocks.Types.Mobile.Finance.ScheduledTransactionList", "Mobile > Finance", "CAFF9FD9-A5DD-472B-B303-A53D94183568" );

            // Add/Update Mobile Block Type
            //   Name:Transaction Detail
            //   Category:Mobile > Finance
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.TransactionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Transaction Detail", "The Transaction Detail block.", "Rock.Blocks.Types.Mobile.Finance.TransactionDetail", "Mobile > Finance", "01A68151-30CC-4FBC-9FE5-2F20A2C1BB4F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.Chat.ChatConfiguration
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.Chat.ChatConfiguration", "Chat Configuration", "Rock.Blocks.Communication.Chat.ChatConfiguration, Rock.Blocks, Version=17.0.35.0, Culture=neutral, PublicKeyToken=null", false, false, "4E1EF8E8-8984-47EA-A6FC-31125C3B6153" );

            // Add/Update Obsidian Block Type
            //   Name:Chat Configuration
            //   Category:Communication > Chat
            //   EntityType:Rock.Blocks.Communication.Chat.ChatConfiguration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Chat Configuration", "Used for making configuration changes to Rock's chat system.", "Rock.Blocks.Communication.Chat.ChatConfiguration", "Communication > Chat", "D5BE6AAE-70A2-4021-93F7-DD66A09B08CB" );

            // Attribute for BlockType
            //   BlockType: Entity Search List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "618265A6-1738-4B12-A9A8-153E260B8A79", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FC924FA4-E567-40EC-99EF-4E181ACB8CB1" );

            // Attribute for BlockType
            //   BlockType: Entity Search List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "618265A6-1738-4B12-A9A8-153E260B8A79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E1BACAB0-E2C3-487D-BD4E-77E477818130" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable ACH
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "Enable ACH", @"Determines if adding an ACH payment method and processing a transaction with an ACH payment method is enabled.", 0, @"False", "51A3BB08-5FED-4A55-BDE5-2CBDEE349160" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable Credit Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Credit Card", "EnableCreditCard", "Enable Credit Card", @"Determines if adding a credit card payment method and processing a transaction with a credit card payment method is enabled.", 1, @"True", "9051ABFC-9A8C-47D0-8458-09A52CE24826" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable Fee Coverage
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Fee Coverage", "EnableFeeCoverage", "Enable Fee Coverage", @"Determines if the fee coverage feature is enabled or not.", 2, @"False", "25BA1347-80DC-4712-AEB9-C9F706475770" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.", 3, @"", "DB93DB61-F96A-44DE-A9F1-A43E38B02AF7" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable Multi-Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Multi-Account", "EnableMultiAccount", "Enable Multi-Account", @"Should the person be able specify amounts for more than one account?", 4, @"True", "5824B035-A722-4615-B643-A272DC801EA3" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Scheduled Transactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduled Transactions", "AllowScheduled", "Scheduled Transactions", @"If the selected gateway(s) allow scheduled transactions, should that option be provided to user. This feature is not compatible when Text-to-Give mode is enabled.", 5, @"True", "693AB751-C455-422E-997B-26CCCC19514D" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Transaction List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction List Page", "TransactionListPage", "Transaction List Page", @"The page to link to when an individual wants to view their transaction history.", 6, @"", "AB1837A8-B61C-4065-9202-A67B964083B6" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Scheduled Transaction List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction List Page", "ScheduledTransactionListPage", "Scheduled Transaction List Page", @"The page to link to when an individual wants to view their scheduled transactions.", 7, @"", "5CFB91AE-8C49-4F1F-B612-A879A347BB2F" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Saved Account List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Saved Account List Page", "SavedAccountListPage", "Saved Account List Page", @"The page to link to when an individual wants to view their payment methods.", 8, @"", "6F1C40D8-0724-47E5-A1A9-43CEB7F59703" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "87C4CA98-1650-4ECE-B9D9-37589686745B" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 1, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "C74A9BC4-7FD9-4D0F-943D-FD577D3B7FA9" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Address Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "AddressType", "Address Type", @"The location type to use for the person's address.", 2, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "E4D32CB8-4F6F-4F75-AC5F-EEE4F128580E" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Ask for Campus if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ask for Campus if Known", "AskForCampusIfKnown", "Ask for Campus if Known", @"If the campus for the person is already known, should the campus still be prompted for?", 0, @"True", "29B3F5A5-3B45-48BD-B928-47367280B245" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Set this to true to include inactive campuses", 1, @"False", "DD733F46-03A5-47ED-8DD1-BEA0ECD053A0" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "IncludedCampusTypes", "Campus Types", @"Set this to limit campuses by campus type.", 2, @"", "A6E8A421-211A-4B4D-9B4A-EC33ED0F3757" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "IncludedCampusStatuses", "Campus Statuses", @"Set this to limit campuses by campus status.", 3, @"", "AB426997-FB8D-4A7A-869A-D48BCE38C0A0" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Use Account Campus Mapping Logic
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Account Campus Mapping Logic", "UseAccountCampusMappingLogic", "Use Account Campus Mapping Logic", @"If enabled, the accounts will be determined as follows:
        <ul>
          <li>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</li>
          <li>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</li>
          <li>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</li>
        <ul>", 4, @"False", "6D8546E0-FAA1-4D94-AECE-CDAB3299AD71" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Receipt Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Receipt Email", "ReceiptEmail", "Receipt Email", @"The system email to use to send the receipt.", 0, @"", "5B02728A-9D29-4427-9579-7FE8D8894D2F" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Success Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Template", "SuccessTemplate", "Success Template", @"The template to display when a transaction is successful.", 1, @"<Grid>
    <StackLayout HorizontalOptions=""Center""
        StyleClass=""mt-48, px-24""
        Spacing=""24"">
        <Rock:Icon IconClass=""circle-check""
            IconFamily=""TablerIcons""
            StyleClass=""text-success-strong""
            FontSize=""80""
            HorizontalOptions=""Center"" />

        <Label Text=""Thank you for your generosity!""
            HorizontalTextAlignment=""Center""
            HorizontalOptions=""Center""
            StyleClass=""title1, text-interface-strongest, bold"" /> 

        <StackLayout Spacing=""8"">
            <Label Text=""Your gift of ${{ Transaction.TotalAmount }} has been received.""
                HorizontalTextAlignment=""Center""
                HorizontalOptions=""Center""
                StyleClass=""text-interface-strong, body"" />

            <Label Text=""We sent a confirmation email to {{ Transaction.AuthorizedPersonAlias.Person.Email }}.""
                HorizontalTextAlignment=""Center""
                HorizontalOptions=""Center""
                StyleClass=""text-interface-medium, body"" />
        </StackLayout>
    </StackLayout>
</Grid>", "C3D77B37-FF6D-44EA-9D79-838CAED4EB68" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Transaction Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "TransactionType", "Transaction Type", @"", 0, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "35E04231-D135-4D88-AE9F-3BDFD74BD6BA" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch.", 1, @"Online Giving", "AE17FFA4-CB02-4B1B-8A61-269DCD55C538" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Account Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Account Campus Context", "AccountCampusContext", "Account Campus Context", @"Should any context be applied to the Account List", 2, @"-1", "616E7B3B-3C80-4FD8-A368-66DD1F7AC75C" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Result Item Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CAFF9FD9-A5DD-472B-B303-A53D94183568", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Result Item Template", "ResultItemTemplate", "Result Item Template", @"Lava template for rendering each result item. The Lava merge field will be 'Item'.", 0, @"AE0A060A-EDC6-43B2-86B9-5FAA4C148CF0", "D3F4D882-3588-4514-8E33-95DD000D5B28" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Include Inactive
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CAFF9FD9-A5DD-472B-B303-A53D94183568", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive", "IncludeInactive", "Include Inactive", @"Indicates whether to dispaly inactive scheduled transactions.", 1, @"False", "91DA4E82-76AB-452B-BCC0-4CC42AF7C4C9" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CAFF9FD9-A5DD-472B-B303-A53D94183568", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "Detail Page", @"Page to link to when user taps on a Scheduled Transaction List. ScheduledTransactionGuid is passed in the query string.", 2, @"", "B143FAAE-B604-4F76-B2D8-14FB3D8055A8" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CAFF9FD9-A5DD-472B-B303-A53D94183568", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"A selection of accounts to use for checking if transactions for the current user exist.", 3, @"", "B0F05761-D4BC-4ED0-B7E9-1F53E4B18912" );

            // Add Block Attribute Value
            //   Block: Check-in Schedule Builder
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Block Location: Page=Schedule Builder, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "BC2B145B-C373-4716-9C8C-D9E179085E67", "077DC692-E57C-4228-9F39-59801674CA68", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute( "B0F05761-D4BC-4ED0-B7E9-1F53E4B18912" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "B143FAAE-B604-4F76-B2D8-14FB3D8055A8" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Include Inactive
            RockMigrationHelper.DeleteAttribute( "91DA4E82-76AB-452B-BCC0-4CC42AF7C4C9" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Attribute: Result Item Template
            RockMigrationHelper.DeleteAttribute( "D3F4D882-3588-4514-8E33-95DD000D5B28" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Account Campus Context
            RockMigrationHelper.DeleteAttribute( "616E7B3B-3C80-4FD8-A368-66DD1F7AC75C" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Batch Name Prefix
            RockMigrationHelper.DeleteAttribute( "AE17FFA4-CB02-4B1B-8A61-269DCD55C538" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Transaction Type
            RockMigrationHelper.DeleteAttribute( "35E04231-D135-4D88-AE9F-3BDFD74BD6BA" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Success Template
            RockMigrationHelper.DeleteAttribute( "C3D77B37-FF6D-44EA-9D79-838CAED4EB68" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Receipt Email
            RockMigrationHelper.DeleteAttribute( "5B02728A-9D29-4427-9579-7FE8D8894D2F" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Use Account Campus Mapping Logic
            RockMigrationHelper.DeleteAttribute( "6D8546E0-FAA1-4D94-AECE-CDAB3299AD71" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "AB426997-FB8D-4A7A-869A-D48BCE38C0A0" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "A6E8A421-211A-4B4D-9B4A-EC33ED0F3757" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.DeleteAttribute( "DD733F46-03A5-47ED-8DD1-BEA0ECD053A0" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Ask for Campus if Known
            RockMigrationHelper.DeleteAttribute( "29B3F5A5-3B45-48BD-B928-47367280B245" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Address Type
            RockMigrationHelper.DeleteAttribute( "E4D32CB8-4F6F-4F75-AC5F-EEE4F128580E" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute( "C74A9BC4-7FD9-4D0F-943D-FD577D3B7FA9" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute( "87C4CA98-1650-4ECE-B9D9-37589686745B" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Saved Account List Page
            RockMigrationHelper.DeleteAttribute( "6F1C40D8-0724-47E5-A1A9-43CEB7F59703" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Scheduled Transaction List Page
            RockMigrationHelper.DeleteAttribute( "5CFB91AE-8C49-4F1F-B612-A879A347BB2F" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Transaction List Page
            RockMigrationHelper.DeleteAttribute( "AB1837A8-B61C-4065-9202-A67B964083B6" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Scheduled Transactions
            RockMigrationHelper.DeleteAttribute( "693AB751-C455-422E-997B-26CCCC19514D" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable Multi-Account
            RockMigrationHelper.DeleteAttribute( "5824B035-A722-4615-B643-A272DC801EA3" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute( "DB93DB61-F96A-44DE-A9F1-A43E38B02AF7" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable Fee Coverage
            RockMigrationHelper.DeleteAttribute( "25BA1347-80DC-4712-AEB9-C9F706475770" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable Credit Card
            RockMigrationHelper.DeleteAttribute( "9051ABFC-9A8C-47D0-8458-09A52CE24826" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Enable ACH
            RockMigrationHelper.DeleteAttribute( "51A3BB08-5FED-4A55-BDE5-2CBDEE349160" );

            // Delete BlockType 
            //   Name: Transaction Detail
            //   Category: Mobile > Finance
            //   Path: -
            //   EntityType: Transaction Detail
            RockMigrationHelper.DeleteBlockType( "01A68151-30CC-4FBC-9FE5-2F20A2C1BB4F" );

            // Delete BlockType 
            //   Name: Scheduled Transaction List
            //   Category: Mobile > Finance
            //   Path: -
            //   EntityType: Scheduled Transaction List
            RockMigrationHelper.DeleteBlockType( "CAFF9FD9-A5DD-472B-B303-A53D94183568" );

            // Delete BlockType 
            //   Name: Giving
            //   Category: Mobile > Finance
            //   Path: -
            //   EntityType: Giving
            RockMigrationHelper.DeleteBlockType( "AE11559B-03C0-42CE-B8B5-CE9C1027E650" );

            // Delete BlockType 
            //   Name: Chat Configuration
            //   Category: Communication > Chat
            //   Path: -
            //   EntityType: Chat Configuration
            RockMigrationHelper.DeleteBlockType( "D5BE6AAE-70A2-4021-93F7-DD66A09B08CB" );
        }
    }
}

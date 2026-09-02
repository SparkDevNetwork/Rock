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
    public partial class PerformAdditionalUtilityPaymentEntrySneakSteps : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddUtilityPaymentEntryObsidianBlockType_Up();
            JPH_AddPageSubNavToTextToGiveSetupPage_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddPageSubNavToTextToGiveSetupPage_Down();
            JPH_AddUtilityPaymentEntryObsidianBlockType_Down();
        }

        /// <summary>
        /// JPH: Add utility payment entry Obsidian block type - up.
        /// </summary>
        private void JPH_AddUtilityPaymentEntryObsidianBlockType_Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.UtilityPaymentEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.UtilityPaymentEntry", "Utility Payment Entry", "Rock.Blocks.Finance.UtilityPaymentEntry, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "BA5C3D24-FDAE-42FD-AEE6-032D0CA7405E" );

            // Add/Update Obsidian Block Type
            //   Name:Utility Payment Entry
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.UtilityPaymentEntry
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Utility Payment Entry", "Creates a new financial transaction or scheduled transaction.", "Rock.Blocks.Finance.UtilityPaymentEntry", "Finance", "7498E1EE-FB79-41FE-9685-6A3D29E3AA76" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Financial Gateway
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Financial Gateway", "FinancialGateway", "Financial Gateway", @"The payment gateway for credit card and ACH transactions.", 0, @"", "2128CDF1-E58A-4E62-8447-E959201A8784" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable ACH
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "Enable ACH", @"Whether ACH bank account payments are accepted.", 1, @"False", "ED0DE081-0BBA-443C-B2EF-F6761E5D49E2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable Credit Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Credit Card", "EnableCreditCard", "Enable Credit Card", @"Whether credit card payments are accepted.", 2, @"True", "8F1BEEDE-834E-438B-A310-4DCCF34FCC0E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The prefix applied to new batch names created by this block.", 3, @"Online Giving", "60C3D02D-B958-4F89-90B9-1B0C7BC46C40" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Source", "Source", "Transaction Source", @"The financial source type applied to transactions created by this block.", 4, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "506E65A8-23FE-4C93-866A-8D8493EA4A6F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Prompt for Campus When Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Campus When Known", "AskForCampusIfKnown", "Prompt for Campus When Known", @"Whether to prompt for campus even when the person's campus is already known.", 5, @"True", "ED31BAC3-188E-49EE-8041-F886D69CA744" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Whether inactive campuses are included in the campus list.", 6, @"False", "80B1D1D7-81E6-47D6-AD2B-EB1A53E33D9F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Type Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Type Filter", "IncludedCampusTypes", "Campus Type Filter", @"Limits the campus list to the selected campus types.", 7, @"", "7E42CDAA-D39F-449A-8C9B-8D913635A40E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Status Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Status Filter", "IncludedCampusStatuses", "Campus Status Filter", @"Limits the campus list to the selected campus statuses.", 8, @"", "B34F6974-680A-4856-B352-B319BE68B25E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Multiple Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Multiple Accounts", "EnableMultiAccount", "Allow Multiple Accounts", @"Whether the giver can split their gift across multiple accounts.", 9, @"True", "DBE6FEE0-D142-4416-B70F-7D5FBD4245A7" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Layout Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Layout Style", "LayoutStyle", "Layout Style", @"Controls whether the block's sections are stacked vertically or displayed in a fluid layout.", 10, @"Vertical", "C5CF7364-5B8F-4ADA-8E4A-9F75A6E67DBD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Accounts to Display
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts to Display", "AccountsToDisplay", "Accounts to Display", @"The accounts shown to the giver. When campus mapping is enabled, a matching child account for the selected campus will be used in place of the parent.", 11, @"", "82814389-E8B8-4420-B9CD-AB5B6C2CEDCD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Additional Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Additional Accounts", "AdditionalAccounts", "Allow Additional Accounts", @"Whether givers can add accounts beyond the configured list. Any active, publicly named account will be available.", 12, @"True", "D87D9054-58C6-41CF-8CB1-04DAE16CD854" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Group Additional Accounts by Hierarchy
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Group Additional Accounts by Hierarchy", "EnableAccountHierarchy", "Group Additional Accounts by Hierarchy", @"When additional accounts are enabled, groups them under their parent accounts. Note: campus-mapped accounts still appear in the hierarchy when campus mapping is on.", 13, @"False", "8AC01784-9407-42B7-ACE5-B1DFCF32B6BD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Account Mapping
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Account Mapping", "UseAccountCampusMappingLogic", "Campus Account Mapping", @"When enabled, the block selects child accounts that match the giver's campus. If no matching child exists, the parent account is used.", 14, @"False", "3C3DC311-0313-4E29-9AE8-8F3EAA6D6598" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Scheduled Gifts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Scheduled Gifts", "AllowScheduled", "Allow Scheduled Gifts", @"Whether givers can set up recurring scheduled gifts. Not compatible with Text-to-Give mode.", 15, @"True", "2FC8EE4C-D745-4A51-93C5-EACC78F0E177" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Scheduled End Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Scheduled End Date", "EnableEndDate", "Allow Scheduled End Date", @"Whether givers can set an optional end date for recurring scheduled gifts.", 16, @"False", "28B0A3CA-49C9-489E-934E-D37C01151E33" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Staff Impersonation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Staff Impersonation", "Impersonation", "Staff Impersonation", @"Allows staff to view and edit transactions on behalf of another person. Only enable this on internal pages secured to trusted individuals.", 17, @"False", "DAC0B237-58AE-4072-A4BA-153C1C6DC4D3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Confirmation Step
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Confirmation Step", "ShowConfirmationPage", "Show Confirmation Step", @"Whether a confirmation step is shown before the transaction is processed.", 18, @"True", "F0CA5B4D-0294-40C1-8117-3582F800A1AC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Prompt for Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Phone", "DisplayPhone", "Prompt for Phone", @"Whether givers are prompted to enter their phone number.", 0, @"False", "A1EE3DC4-02CE-4067-A863-D0B67DE0D194" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: SMS Opt-In
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "SMS Opt-In", "SmsOptIn", "SMS Opt-In", @"When phone prompting is enabled, displays an opt-in checkbox for SMS communications on the entered number.", 1, @"False", "04CB0CB2-4D8F-4687-97D6-7A57137CA7C3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Prompt for Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Email", "DisplayEmail", "Prompt for Email", @"Whether givers are prompted to enter their email address.", 2, @"True", "4AB82801-989A-4593-A970-0DF59DDDFF07" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Address Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "AddressType", "Address Type", @"The location type used when saving or updating the person's address.", 3, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "F6C8B3FE-22CA-421B-8057-59D12D1730A5" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Connection Status (New People)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status (New People)", "ConnectionStatus", "Connection Status (New People)", @"The connection status assigned to new individuals created through this block.", 4, @"368DD475-242C-49C4-A42C-7278BE690CC2", "72D2DF3D-3FBA-414C-96C4-3E0FF399BD0C" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Record Status (New People)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status (New People)", "RecordStatus", "Record Status (New People)", @"The record status assigned to new individuals created through this block.", 5, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "E55DF626-2E9A-4556-B1BD-550355A91FDB" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Record Source (New People)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Source (New People)", "RecordSource", "Record Source (New People)", @"The record source assigned to new individuals. Can be overridden by a RecordSource page parameter.", 6, @"A6677492-5AA5-4A09-9854-D9C54705C67D", "050964D4-5AF9-46E0-87F2-A788FC0D375F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Business Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Business Giving", "EnableBusinessGiving", "Allow Business Giving", @"Whether the option to give as a business is shown to the giver.", 7, @"True", "DC8B34DC-56F7-4B44-BA23-EED53CDE41FC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Anonymous Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Anonymous Giving", "EnableAnonymousGiving", "Allow Anonymous Giving", @"Whether givers can choose to give anonymously. Anonymous gifts appear as ""Anonymous"" on public-facing contribution lists.", 8, @"False", "43967169-A5AB-4E13-9B8E-6966517FA4C1" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Comment Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Comment Entry", "EnableCommentEntry", "Allow Comment Entry", @"Whether givers can enter a custom comment. The entered value is appended to the Payment Comment Template.", 9, @"False", "0D290BDB-4881-4218-A863-5A0AE03CC48C" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Disable CAPTCHA
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable CAPTCHA", "DisableCaptchaSupport", "Disable CAPTCHA", @"Skips the CAPTCHA verification step when enabled.", 10, @"False", "F91797B5-3D6C-4E38-9CF9-5122D3F1874A" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Account Confirmation Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Account Confirmation Email", "ConfirmAccountTemplate", "Account Confirmation Email", @"The system communication sent to confirm a new account.", 0, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "7F5D79E0-414A-4056-A9BF-85FCECA19737" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Receipt Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Receipt Email", "ReceiptEmail", "Receipt Email", @"The system communication used to send giving receipts.", 1, @"", "D9D7E5A1-9043-4F05-9612-0D3DA4BF3233" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Panel & Section Headings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Panel & Section Headings", "ShowPanelHeadings", "Show Panel & Section Headings", @"Whether the block panel title and section headings are visible. Note: if 'Show Block Header Section' is enabled, the block panel title will not be shown.", 0, @"True", "99F6B892-F7E6-40F6-B727-9D2AA48FE881" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "Panel Title", @"The heading text shown at the top of the block panel.", 1, @"Gifts", "CC6C36D0-DC76-4616-B598-A1B6C80564B9" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Block Header Section
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Block Header Section", "ShowBlockHeaderSection", "Show Block Header Section", @"When enabled, displays a title and description at the top of the block.", 0, @"True", "B70E5EF3-4B5E-4C0F-8A21-5F7AF4985111" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Transaction Header Template", "TransactionHeader", "Transaction Header Template", @"The Lava template displayed above the amount entry fields.", 2, @"", "1163093B-4267-40AA-9739-73F6C31E94A3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Comment Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Payment Comment Template", "PaymentCommentTemplate", "Payment Comment Template", @"The Lava template for the comment sent to the payment gateway with each transaction.", 3, @"", "B9AA0216-1531-4BB9-B688-BCD01F4C0692" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Title", "HeaderTitle", "Header Title", @"The title displayed at the top of the block.", 1, @"New Contribution", "E4FC2A5A-1E86-4189-AA81-19D1085828EB" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Header Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Description", "HeaderDescription", "Header Description", @"The supporting text displayed below the header title.", 2, @"Provide details to set up a new contribution.", "3B35660D-6962-4E14-9ED0-8207F675A499" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Header Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Icon", "HeaderIcon", "Header Icon", @"The icon displayed in the block header.", 3, @"ti ti-cash", "D9D1BBA3-33B8-46A8-B3C6-5326D69F2176" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Information Section Title", "CampusInformationSectionTitle", "Campus Information Section Title", @"The label displayed in the Campus Information section header.", 0, @"Campus Information", "DCF952EA-9E6F-46A6-9093-7E1EAA7BD5A2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Information Section Icon", "CampusInformationSectionIcon", "Campus Information Section Icon", @"The icon displayed in the Campus Information section header.", 1, @"ti ti-map-pin", "C8E15F85-34E1-4D83-9F82-1EA2069212AC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Information Section Description", "CampusInformationSectionDescription", "Campus Information Section Description", @"Supporting text below the section title.", 2, @"Select the campus that your gift should be associated with.", "A87CD5F7-8A06-4E15-8C8E-4A8EA42C1DB4" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contribution Information Section Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contribution Information Section Heading", "ContributionInfoTitle", "Contribution Information Section Heading", @"The heading for the account and amount selection section.", 0, @"Contribution Information", "D9FB4137-7E26-41D9-B477-00F8DFF9D766" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contribution Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contribution Information Section Icon", "ContributionInformationSectionIcon", "Contribution Information Section Icon", @"The icon displayed in the Contribution Information section header.", 1, @"ti ti-gift", "5D97C1C0-9567-4950-AC24-C1A33D66FDE8" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contribution Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contribution Information Section Description", "ContributionInformationSectionDescription", "Contribution Information Section Description", @"Supporting text below the section title.", 2, @"Specify how much to contribute, where it should go, and how often.", "977AAC9B-6A40-401C-B9CE-C809B4F276EF" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Add Account Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Add Account Button Text", "AddAccountText", "Add Account Button Text", @"The label on the button that adds another account.", 3, @"Add Another Account", "83468F45-95E8-4E56-9D0F-5ACFF58862FD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Account Label Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Account Label Template", "AccountHeaderTemplate", "Account Label Template", @"The Lava template used as the label for each account's amount input.", 4, @"{{ Account.PublicName }}", "F22E5EA4-573D-4876-8B89-4B496CB7E873" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Comment Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Field Label", "CommentEntryLabel", "Comment Field Label", @"The label shown on the comment input field (e.g., Trip Name).", 5, @"Comment", "698803DB-DEF1-4804-8C55-4951CBBD55F2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contact Information Section Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Section Heading", "PersonalInfoTitle", "Contact Information Section Heading", @"The heading for the contact information section.", 0, @"Contact Information", "F06455D1-26F4-4815-94E3-D77BD755D101" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contact Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Section Icon", "ContactInformationSectionIcon", "Contact Information Section Icon", @"The icon displayed in the Contact Information section header.", 1, @"ti ti-user-circle", "38735B3B-98F8-456A-A28D-4A1C80760F00" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contact Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Section Description", "ContactInformationSectionDescription", "Contact Information Section Description", @"Supporting text below the section title.", 2, @"Provide contact details to associate with this gift.", "5BE6F609-2409-4548-9977-57032177B6BE" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Anonymous Giving Tooltip
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Anonymous Giving Tooltip", "AnonymousGivingTooltip", "Anonymous Giving Tooltip", @"The tooltip text shown on the Give Anonymously checkbox.", 3, @"", "1119F5BF-92FA-4B36-8C04-C089973C9092" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Information Section Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Information Section Heading", "PaymentInfoTitle", "Payment Information Section Heading", @"The heading for the payment method section.", 0, @"Payment Information", "3F83D40A-EE82-426E-AB16-087D4B572014" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Information Section Icon", "PaymentInformationSectionIcon", "Payment Information Section Icon", @"The icon displayed in the Payment Information section header.", 1, @"ti ti-wallet", "E1265B14-D473-4CD4-AA40-E5ECAC46CE60" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Information Section Description", "PaymentInformationSectionDescription", "Payment Information Section Description", @"Supporting text below the section title.", 2, @"Enter the payment method and billing details used to process this gift.", "272D1FBA-502E-4201-942A-7FE8C2554C6E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Section Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmation Section Heading", "ConfirmationTitle", "Confirmation Section Heading", @"The heading for the confirmation review section.", 0, @"Confirm Information", "AA7F7FC6-9F06-40E3-B790-60006A5C61A3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Header", "ConfirmationHeader", "Confirmation Header", @"HTML displayed at the top of the confirmation section. Supports Lava.", 1, @"
<p>
    Please confirm the information below. Once you have confirmed that the information is accurate click the ""Finish"" button to complete your transaction.
</p>
", "92E3E363-03AC-44EF-AF5A-8A3C59A30E21" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Body
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Body", "ConfirmationBody", "Confirmation Body", @"Body content rendered on the confirmation step. Supports Lava.", 2, @"
<h5>Contribution Details</h5>
<div class='panel panel-default shadow-none'>
    <table class='table utility-payment-entry-summary'>
        <tbody>
            {% for accountDetail in AccountDetails %}
            <tr>
                <td>{{ accountDetail.PublicName }}</td>
                <td class='text-right'>{{ accountDetail.Amount | FormatAsCurrency }}</td>
            </tr>
            {% endfor %}
            <tr class='utility-payment-entry-summary-total'>
                <td><strong>Total</strong></td>
                <td class='text-right'><strong>{{ Total | FormatAsCurrency }}</strong></td>
            </tr>
        </tbody>
    </table>
</div>

<h5>Payment &amp; Confirmation</h5>
<div class='panel panel-default shadow-none'>
    <table class='table utility-payment-entry-summary'>
        <tbody>
            <tr>
                <td>When</td>
                <td class='text-right'>{{ When }}</td>
            </tr>
            <tr>
                <td>Name</td>
                <td class='text-right'>{{ Name }}</td>
            </tr>
            {% if Email and Email != '' %}
            <tr>
                <td>Email</td>
                <td class='text-right'>{{ Email }}</td>
            </tr>
            {% endif %}
            {% if Address %}
            <tr>
                <td>Address</td>
                <td class='text-right'>{{ Address.FormattedAddress }}</td>
            </tr>
            {% endif %}
        </tbody>
    </table>
</div>
", "2B127A10-C82A-4F7D-A1B5-B288DFC5B2FF" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Footer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Footer", "ConfirmationFooter", "Confirmation Footer", @"HTML displayed at the bottom of the confirmation section. Supports Lava.", 3, @"
<div class='alert alert-info'>
    By clicking the ""Finish"" button below I agree to allow {{ OrganizationName }} to transfer the amount above from my account. I acknowledge that I may update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions.
</div>
", "0EAA492F-2D51-446C-8704-CDA394863183" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Success Page Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Page Template", "FinishLavaTemplate", "Success Page Template", @"The Lava template rendered on the success page after a transaction completes.", 0, @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

{% if IsTextToGive %}
    {% assign successMessage = 'Thank you for your gift. Your next gift can be completed by texting the word ""give"" followed by the dollar amount (e.g., ""give $100"").' %}
{% else %}
    {% assign successMessage = 'The transaction has been submitted successfully.' %}
{% endif %}

<h5>Contribution Details</h5>
<div class='panel panel-default shadow-none'>
    <table class='table utility-payment-entry-summary'>
        <tbody>
            {% for transactionDetail in transactionDetails %}
            <tr>
                <td>{{ transactionDetail.Account.PublicName }}</td>
                <td class='text-right'>{{ transactionDetail.Amount | Minus: transactionDetail.FeeCoverageAmount | FormatAsCurrency }}</td>
            </tr>
            {% endfor %}
            {% if Transaction.TotalFeeCoverageAmount %}
            <tr>
                <td>Fee Coverage</td>
                <td class='text-right'>{{ Transaction.TotalFeeCoverageAmount | FormatAsCurrency }}</td>
            </tr>
            {% endif %}
            <tr class='utility-payment-entry-summary-total'>
                <td><strong>Total</strong></td>
                <td class='text-right'><strong>{{ Transaction.TotalAmount | FormatAsCurrency }}</strong></td>
            </tr>
        </tbody>
    </table>
</div>

<h5>Payment &amp; Confirmation</h5>
<div class='panel panel-default shadow-none'>
    <table class='table utility-payment-entry-summary'>
        <tbody>
            <tr>
                <td>Payment Method</td>
                <td class='text-right'>{{ PaymentDetail.CurrencyTypeValue.Description }}</td>
            </tr>
            {% if PaymentDetail.AccountNumberMasked and PaymentDetail.AccountNumberMasked != '' %}
            <tr>
                <td>Account Number</td>
                <td class='text-right'>{{ PaymentDetail.AccountNumberMasked }}</td>
            </tr>
            {% endif %}
            <tr>
                <td>When</td>
                <td class='text-right'>{% if Transaction.TransactionFrequencyValue %}{{ Transaction.TransactionFrequencyValue.Value }}{% if Transaction.EndDate %} starting on {{ Transaction.NextPaymentDate | Date:'sd' }} and ending on {{ Transaction.EndDate | Date:'sd' }}{% else %} starting on {{ Transaction.NextPaymentDate | Date:'sd' }}{% endif %}{% else %}Today{% endif %}</td>
            </tr>
            <tr>
                <td>Name</td>
                <td class='text-right'>{{ Person.FullName }}</td>
            </tr>
            {% if Person.Email and Person.Email != '' %}
            <tr>
                <td>Email</td>
                <td class='text-right'>{{ Person.Email }}</td>
            </tr>
            {% endif %}
            {% if BillingLocation %}
            <tr>
                <td>Address</td>
                <td class='text-right'>{{ BillingLocation.FormattedAddress }}</td>
            </tr>
            {% endif %}
            <tr>
                <td>Confirmation</td>
                <td class='text-right'><span class='label label-info'>{{ Transaction.TransactionCode }}</span></td>
            </tr>
        </tbody>
    </table>
</div>

<div class='alert alert-success'>
    {{ successMessage }}
</div>
", "CD348E94-5D77-4418-851E-BE3F28D9C4CE" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Save Payment Method Section Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Payment Method Section Heading", "SaveAccountTitle", "Save Payment Method Section Heading", @"The heading for the save payment method section.", 1, @"Make Giving Even Easier", "C24EFEF9-A43E-492D-B04B-B5CCF5E0A9D7" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Save Payment Method Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Payment Method Section Icon", "SavePaymentMethodSectionIcon", "Save Payment Method Section Icon", @"The icon displayed in the Save Payment Method section header.", 2, @"ti ti-bolt", "C76E4CCB-D074-44BC-AD2F-57C414FD4991" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Save Payment Method Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Payment Method Section Description", "SavePaymentMethodSectionDescription", "Save Payment Method Section Description", @"Supporting text below the section title.", 3, @"Save your payment details to make future giving faster.", "7FA95A6D-A1C2-49F0-91B1-177958963B7A" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Success Page Footer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Page Footer", "SuccessFooter", "Success Page Footer", @"HTML displayed at the bottom of the success page. Supports Lava.", 4, @"", "C245EF03-2102-4ECE-B679-E990A42A8773" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Account Options in URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Account Options in URL", "AllowAccountOptionsInURL", "Allow Account Options in URL", @"Whether account options (IDs, GL codes, amounts, editability) can be passed as URL parameters.", 0, @"False", "2D2C7B7F-F3E3-4D23-80BB-5A63E3D85EC0" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Restrict URL Accounts to Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Restrict URL Accounts to Public Only", "OnlyPublicAccountsInURL", "Restrict URL Accounts to Public Only", @"When URL account options are enabled, prevents non-public accounts from being specified in the URL.", 1, @"True", "FA9F71E2-68E8-4C03-B670-173B7B7B5C95" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Invalid Account Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Account Message", "InvalidAccountMessage", "Invalid Account Message", @"HTML error message shown when an invalid account ID or GL code is passed in the URL.", 2, @"The configured financial accounts are not valid for accepting financial transactions.", "EF792286-4739-4639-8C94-0B76507DAD78" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Account Campus Context Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Account Campus Context Filter", "AccountCampusContext", "Account Campus Context Filter", @"Whether and how the current campus context filters the account list.", 3, @"-1", "1E08252C-4164-4718-9B85-E75E9489C26F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Attributes from URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Transaction Attributes from URL", "AllowedTransactionAttributesFromURL", "Transaction Attributes from URL", @"Transaction attributes that can be set via URL parameters using the Attribute_ prefix.", 4, @"", "BD2C0A6E-5783-42F3-8F63-686A6AC87BB6" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "TransactionType", "Transaction Type", @"The financial transaction type applied to transactions created by this block.", 5, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "CB72A93A-523A-478A-8228-0C400246E79D" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Transaction Entity Type", "TransactionEntityType", "Transaction Entity Type", @"The entity type for the transaction detail record. Leave blank unless this block is linked to a specific entity.", 6, @"", "62FC18DE-4ED1-4854-BC8C-48D6C705357B" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Entity ID Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity ID Parameter", "EntityIdParam", "Entity ID Parameter", @"The page parameter used to populate the entity ID on the transaction detail record. Requires Transaction Entity Type to be set.", 7, @"", "38B80FE4-BA79-46CE-8AE1-4D352C8E7A6E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Initial Back Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Initial Back Button", "EnableInitialBackbutton", "Show Initial Back Button", @"Whether a Back button is shown on the first step, navigating the individual to the previous page.", 8, @"False", "E9D6E16A-9380-4D28-9BF2-3CF15783DD31" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Text-to-Give Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Text-to-Give Mode", "EnableTextToGiveSetup", "Text-to-Give Mode", @"Enables the Text-to-Give account setup flow. Not compatible with scheduled transactions.", 9, @"False", "86315AD5-BE7F-4964-BC27-7581F49D83C2" );
        }

        /// <summary>
        /// JPH: Add page sub nav to text to give setup page - up.
        /// </summary>
        private void JPH_AddPageSubNavToTextToGiveSetupPage_Up()
        {
            var textToGivePageGuid = "42CEEE52-ADEC-48BB-AF90-496DB2B272C7";

            var blockCount = ( int ) SqlScalar( $@"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{Rock.SystemGuid.BlockType.PAGE_MENU}');
DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{textToGivePageGuid}');

SELECT COUNT(1)
FROM [Block]
WHERE [BlockTypeId] = @BlockTypeId
    AND [PageId] = @PageId;" );

            if ( blockCount > 0 )
            {
                // The page already has a Page Menu block, so we don't need to add one.
                return;
            }

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Text To Give Setup
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "42CEEE52-ADEC-48BB-AF90-496DB2B272C7".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Sidebar1", @"", @"", 0, "C7673A35-8485-4238-A8B2-2EDC39E59B15" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Text To Give Setup, Site=External Website
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C7673A35-8485-4238-A8B2-2EDC39E59B15", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Text To Give Setup, Site=External Website
            //   Attribute: Include Current Parameters
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C7673A35-8485-4238-A8B2-2EDC39E59B15", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Text To Give Setup, Site=External Website
            //   Attribute: Root Page
            /*   Attribute Value: 8bb303af-743c-49dc-a7ff-cc1236b4b1d9,27203dd3-04fe-4607-8249-a301399c01c3 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C7673A35-8485-4238-A8B2-2EDC39E59B15", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9,27203dd3-04fe-4607-8249-a301399c01c3" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Text To Give Setup, Site=External Website
            //   Attribute: Number of Levels
            /*   Attribute Value: 3 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C7673A35-8485-4238-A8B2-2EDC39E59B15", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Text To Give Setup, Site=External Website
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageSubNav.lava'  %} */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C7673A35-8485-4238-A8B2-2EDC39E59B15", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava'  %}" );
        }

        /// <summary>
        /// JPH: Add page sub nav to text to give setup page - down.
        /// </summary>
        private void JPH_AddPageSubNavToTextToGiveSetupPage_Down()
        {
            // Remove Block
            //  Name: Page Menu, from Page: Text To Give Setup, Site: External Website
            //  from Page: Text To Give Setup, Site: External Website
            RockMigrationHelper.DeleteBlock( "C7673A35-8485-4238-A8B2-2EDC39E59B15" );
        }

        /// <summary>
        /// JPH: Add utility payment entry Obsidian block type - down.
        /// </summary>
        private void JPH_AddUtilityPaymentEntryObsidianBlockType_Down()
        {
            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Text-to-Give Mode
            RockMigrationHelper.DeleteAttribute( "86315AD5-BE7F-4964-BC27-7581F49D83C2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Initial Back Button
            RockMigrationHelper.DeleteAttribute( "E9D6E16A-9380-4D28-9BF2-3CF15783DD31" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Entity ID Parameter
            RockMigrationHelper.DeleteAttribute( "38B80FE4-BA79-46CE-8AE1-4D352C8E7A6E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Entity Type
            RockMigrationHelper.DeleteAttribute( "62FC18DE-4ED1-4854-BC8C-48D6C705357B" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Type
            RockMigrationHelper.DeleteAttribute( "CB72A93A-523A-478A-8228-0C400246E79D" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Attributes from URL
            RockMigrationHelper.DeleteAttribute( "BD2C0A6E-5783-42F3-8F63-686A6AC87BB6" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Account Campus Context Filter
            RockMigrationHelper.DeleteAttribute( "1E08252C-4164-4718-9B85-E75E9489C26F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Invalid Account Message
            RockMigrationHelper.DeleteAttribute( "EF792286-4739-4639-8C94-0B76507DAD78" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Restrict URL Accounts to Public Only
            RockMigrationHelper.DeleteAttribute( "FA9F71E2-68E8-4C03-B670-173B7B7B5C95" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Account Options in URL
            RockMigrationHelper.DeleteAttribute( "2D2C7B7F-F3E3-4D23-80BB-5A63E3D85EC0" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Success Page Footer
            RockMigrationHelper.DeleteAttribute( "C245EF03-2102-4ECE-B679-E990A42A8773" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Save Payment Method Section Description
            RockMigrationHelper.DeleteAttribute( "7FA95A6D-A1C2-49F0-91B1-177958963B7A" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Save Payment Method Section Icon
            RockMigrationHelper.DeleteAttribute( "C76E4CCB-D074-44BC-AD2F-57C414FD4991" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Save Payment Method Section Heading
            RockMigrationHelper.DeleteAttribute( "C24EFEF9-A43E-492D-B04B-B5CCF5E0A9D7" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Success Page Template
            RockMigrationHelper.DeleteAttribute( "CD348E94-5D77-4418-851E-BE3F28D9C4CE" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Footer
            RockMigrationHelper.DeleteAttribute( "0EAA492F-2D51-446C-8704-CDA394863183" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Body
            RockMigrationHelper.DeleteAttribute( "2B127A10-C82A-4F7D-A1B5-B288DFC5B2FF" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Header
            RockMigrationHelper.DeleteAttribute( "92E3E363-03AC-44EF-AF5A-8A3C59A30E21" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Confirmation Section Heading
            RockMigrationHelper.DeleteAttribute( "AA7F7FC6-9F06-40E3-B790-60006A5C61A3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Information Section Description
            RockMigrationHelper.DeleteAttribute( "272D1FBA-502E-4201-942A-7FE8C2554C6E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Information Section Icon
            RockMigrationHelper.DeleteAttribute( "E1265B14-D473-4CD4-AA40-E5ECAC46CE60" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Information Section Heading
            RockMigrationHelper.DeleteAttribute( "3F83D40A-EE82-426E-AB16-087D4B572014" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Anonymous Giving Tooltip
            RockMigrationHelper.DeleteAttribute( "1119F5BF-92FA-4B36-8C04-C089973C9092" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contact Information Section Description
            RockMigrationHelper.DeleteAttribute( "5BE6F609-2409-4548-9977-57032177B6BE" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contact Information Section Icon
            RockMigrationHelper.DeleteAttribute( "38735B3B-98F8-456A-A28D-4A1C80760F00" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contact Information Section Heading
            RockMigrationHelper.DeleteAttribute( "F06455D1-26F4-4815-94E3-D77BD755D101" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Comment Field Label
            RockMigrationHelper.DeleteAttribute( "698803DB-DEF1-4804-8C55-4951CBBD55F2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Account Label Template
            RockMigrationHelper.DeleteAttribute( "F22E5EA4-573D-4876-8B89-4B496CB7E873" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Add Account Button Text
            RockMigrationHelper.DeleteAttribute( "83468F45-95E8-4E56-9D0F-5ACFF58862FD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contribution Information Section Description
            RockMigrationHelper.DeleteAttribute( "977AAC9B-6A40-401C-B9CE-C809B4F276EF" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contribution Information Section Icon
            RockMigrationHelper.DeleteAttribute( "5D97C1C0-9567-4950-AC24-C1A33D66FDE8" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Contribution Information Section Heading
            RockMigrationHelper.DeleteAttribute( "D9FB4137-7E26-41D9-B477-00F8DFF9D766" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Information Section Description
            RockMigrationHelper.DeleteAttribute( "A87CD5F7-8A06-4E15-8C8E-4A8EA42C1DB4" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Information Section Icon
            RockMigrationHelper.DeleteAttribute( "C8E15F85-34E1-4D83-9F82-1EA2069212AC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Information Section Title
            RockMigrationHelper.DeleteAttribute( "DCF952EA-9E6F-46A6-9093-7E1EAA7BD5A2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Header Icon
            RockMigrationHelper.DeleteAttribute( "D9D1BBA3-33B8-46A8-B3C6-5326D69F2176" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Header Description
            RockMigrationHelper.DeleteAttribute( "3B35660D-6962-4E14-9ED0-8207F675A499" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Header Title
            RockMigrationHelper.DeleteAttribute( "E4FC2A5A-1E86-4189-AA81-19D1085828EB" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Comment Template
            RockMigrationHelper.DeleteAttribute( "B9AA0216-1531-4BB9-B688-BCD01F4C0692" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Header Template
            RockMigrationHelper.DeleteAttribute( "1163093B-4267-40AA-9739-73F6C31E94A3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Block Header Section
            RockMigrationHelper.DeleteAttribute( "B70E5EF3-4B5E-4C0F-8A21-5F7AF4985111" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Panel Title
            RockMigrationHelper.DeleteAttribute( "CC6C36D0-DC76-4616-B598-A1B6C80564B9" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Panel & Section Headings
            RockMigrationHelper.DeleteAttribute( "99F6B892-F7E6-40F6-B727-9D2AA48FE881" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Receipt Email
            RockMigrationHelper.DeleteAttribute( "D9D7E5A1-9043-4F05-9612-0D3DA4BF3233" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Account Confirmation Email
            RockMigrationHelper.DeleteAttribute( "7F5D79E0-414A-4056-A9BF-85FCECA19737" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Disable CAPTCHA
            RockMigrationHelper.DeleteAttribute( "F91797B5-3D6C-4E38-9CF9-5122D3F1874A" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Comment Entry
            RockMigrationHelper.DeleteAttribute( "0D290BDB-4881-4218-A863-5A0AE03CC48C" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Anonymous Giving
            RockMigrationHelper.DeleteAttribute( "43967169-A5AB-4E13-9B8E-6966517FA4C1" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Business Giving
            RockMigrationHelper.DeleteAttribute( "DC8B34DC-56F7-4B44-BA23-EED53CDE41FC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Record Source (New People)
            RockMigrationHelper.DeleteAttribute( "050964D4-5AF9-46E0-87F2-A788FC0D375F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Record Status (New People)
            RockMigrationHelper.DeleteAttribute( "E55DF626-2E9A-4556-B1BD-550355A91FDB" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Connection Status (New People)
            RockMigrationHelper.DeleteAttribute( "72D2DF3D-3FBA-414C-96C4-3E0FF399BD0C" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Address Type
            RockMigrationHelper.DeleteAttribute( "F6C8B3FE-22CA-421B-8057-59D12D1730A5" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Prompt for Email
            RockMigrationHelper.DeleteAttribute( "4AB82801-989A-4593-A970-0DF59DDDFF07" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: SMS Opt-In
            RockMigrationHelper.DeleteAttribute( "04CB0CB2-4D8F-4687-97D6-7A57137CA7C3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Prompt for Phone
            RockMigrationHelper.DeleteAttribute( "A1EE3DC4-02CE-4067-A863-D0B67DE0D194" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Show Confirmation Step
            RockMigrationHelper.DeleteAttribute( "F0CA5B4D-0294-40C1-8117-3582F800A1AC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Staff Impersonation
            RockMigrationHelper.DeleteAttribute( "DAC0B237-58AE-4072-A4BA-153C1C6DC4D3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Scheduled End Date
            RockMigrationHelper.DeleteAttribute( "28B0A3CA-49C9-489E-934E-D37C01151E33" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Scheduled Gifts
            RockMigrationHelper.DeleteAttribute( "2FC8EE4C-D745-4A51-93C5-EACC78F0E177" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Account Mapping
            RockMigrationHelper.DeleteAttribute( "3C3DC311-0313-4E29-9AE8-8F3EAA6D6598" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Group Additional Accounts by Hierarchy
            RockMigrationHelper.DeleteAttribute( "8AC01784-9407-42B7-ACE5-B1DFCF32B6BD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Additional Accounts
            RockMigrationHelper.DeleteAttribute( "D87D9054-58C6-41CF-8CB1-04DAE16CD854" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Accounts to Display
            RockMigrationHelper.DeleteAttribute( "82814389-E8B8-4420-B9CD-AB5B6C2CEDCD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Layout Style
            RockMigrationHelper.DeleteAttribute( "C5CF7364-5B8F-4ADA-8E4A-9F75A6E67DBD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Allow Multiple Accounts
            RockMigrationHelper.DeleteAttribute( "DBE6FEE0-D142-4416-B70F-7D5FBD4245A7" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Status Filter
            RockMigrationHelper.DeleteAttribute( "B34F6974-680A-4856-B352-B319BE68B25E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Type Filter
            RockMigrationHelper.DeleteAttribute( "7E42CDAA-D39F-449A-8C9B-8D913635A40E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.DeleteAttribute( "80B1D1D7-81E6-47D6-AD2B-EB1A53E33D9F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Prompt for Campus When Known
            RockMigrationHelper.DeleteAttribute( "ED31BAC3-188E-49EE-8041-F886D69CA744" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Transaction Source
            RockMigrationHelper.DeleteAttribute( "506E65A8-23FE-4C93-866A-8D8493EA4A6F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Batch Name Prefix
            RockMigrationHelper.DeleteAttribute( "60C3D02D-B958-4F89-90B9-1B0C7BC46C40" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable Credit Card
            RockMigrationHelper.DeleteAttribute( "8F1BEEDE-834E-438B-A310-4DCCF34FCC0E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable ACH
            RockMigrationHelper.DeleteAttribute( "ED0DE081-0BBA-443C-B2EF-F6761E5D49E2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Financial Gateway
            RockMigrationHelper.DeleteAttribute( "2128CDF1-E58A-4E62-8447-E959201A8784" );

            // Delete BlockType 
            //   Name: Utility Payment Entry
            //   Category: Finance
            //   Path: -
            //   EntityType: Utility Payment Entry
            RockMigrationHelper.DeleteBlockType( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.UtilityPaymentEntry
            RockMigrationHelper.DeleteEntityType( "BA5C3D24-FDAE-42FD-AEE6-032D0CA7405E" );
        }
    }
}

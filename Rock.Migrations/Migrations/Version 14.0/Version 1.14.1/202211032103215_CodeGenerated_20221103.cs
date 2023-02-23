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
    public partial class CodeGenerated_20221103 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
           // Add/Update BlockType 
           //   Name: Utility Payment Entry 
           //   Category: Finance
           //   Path: ~/Blocks/Finance/UtilityPaymentEntry.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Utility Payment Entry ","Creates a new financial transaction or scheduled transaction.","~/Blocks/Finance/UtilityPaymentEntry.ascx","Finance","997588CE-012D-4A9A-BEB4-A460B6A2090C");

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DE922F22-FABB-4092-86E8-B5C0EC1501C6".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"64EF8FE8-2348-4225-BF2C-3C8789DF4169"); 

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Financial Gateway
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Financial Gateway", "FinancialGateway", "Financial Gateway", @"The payment gateway to use for Credit Card and ACH transactions.", 0, @"", "04A2A3EF-A421-4D01-9623-F7EAEC3D565B" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable ACH
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "Enable ACH", @"", 1, @"False", "339F050F-2029-4D32-9F0B-B02C57CF727F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Credit Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Credit Card", "EnableCreditCard", "Enable Credit Card", @"", 2, @"True", "BA1056D9-583D-42B7-97CF-7A0FFBB8A937" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch.", 3, @"Online Giving", "53209D5D-EB38-4514-BCB2-BF570D0FFC6B" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "Source", @"The Financial Source Type to use when creating transactions.", 4, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "EC034C57-8673-4DA4-A644-C410A1AD99B9" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Impersonation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonation", "Impersonation", "Impersonation", @"Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.", 5, @"False", "46ABD47A-0764-4B10-BB24-67E4A2E23008" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Layout Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Layout Style", "LayoutStyle", "Layout Style", @"How the sections of this page should be displayed.", 6, @"Vertical", "B754B186-9393-4AA7-96BB-98F4DCC3F7B5" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Account Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Account Header Template", "AccountHeaderTemplate", "Account Header Template", @"The Lava Template to use as the amount input label for each account.", 7, @"{{ Account.PublicName }}", "BFA1B019-AE44-4EB7-B94F-09491DC742F7" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"The accounts to display.  By default all active accounts with a Public Name will be displayed.", 8, @"", "A4565ACC-CC7E-4341-AA63-D4E097CABB52" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Additional Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Additional Accounts", "AdditionalAccounts", "Additional Accounts", @"Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available.", 9, @"True", "C9F58154-5899-4A83-AD0E-AED2DB7749CA" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Scheduled Transactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduled Transactions", "AllowScheduled", "Scheduled Transactions", @"If the selected gateway(s) allow scheduled transactions, should that option be provided to user.", 10, @"True", "66377AA4-C948-4399-B16A-322A06588770" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Prompt for Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Phone", "DisplayPhone", "Prompt for Phone", @"Should the user be prompted for their phone number?", 11, @"False", "CC559701-4611-403E-8DC2-7F45C2BB367D" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Prompt for Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Email", "DisplayEmail", "Prompt for Email", @"Should the user be prompted for their email address?", 12, @"True", "B6C8F874-580C-4DCA-B605-F48499FFF291" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Address Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "AddressType", "Address Type", @"The location type to use for the person's address.", 13, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "CEF0EBFD-CA6C-43E8-940B-D78D2C6BFA71" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Prospect'.)", 14, @"368DD475-242C-49C4-A42C-7278BE690CC2", "39670D61-6267-4CA9-A6BB-4D51D6B19C47" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 15, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "9C1038F2-6E3C-471B-8D59-EA32051E288A" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Comment Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Comment Entry", "EnableCommentEntry", "Enable Comment Entry", @"Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment Template' setting)", 16, @"False", "FA847E1A-88E2-4561-B56F-32C2F47781BC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Comment Entry Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Entry Label", "CommentEntryLabel", "Comment Entry Label", @"The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", 17, @"Comment", "E6A0F338-2310-40AF-9089-0B8D2538E112" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Business Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Business Giving", "EnableBusinessGiving", "Enable Business Giving", @"Should the option to give as a business be displayed?", 18, @"True", "02B65889-8D7A-4B9D-9AD3-FD6555538F70" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Anonymous Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Anonymous Giving", "EnableAnonymousGiving", "Enable Anonymous Giving", @"Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fundraising contributors.", 19, @"False", "0E6BB29E-7C0B-4119-8CB8-7EB389C64E7D" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirm Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account", "ConfirmAccountTemplate", "Confirm Account", @"Confirm Account Email Template", 1, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "DC597056-84AE-43A4-BFEB-4C93DD2E0AEB" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Receipt Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Receipt Email", "ReceiptEmail", "Receipt Email", @"The system email to use to send the receipt.", 2, @"", "0DCB60DD-14DD-4BB1-8AFA-7638858A7099" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "Panel Title", @"The text to display in panel heading", 1, @"Gifts", "C1DF62FB-9FDF-4D3C-B940-0D9838B4BC7E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Contribution Info Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contribution Info Title", "ContributionInfoTitle", "Contribution Info Title", @"The text to display as heading of section for selecting account and amount.", 2, @"Contribution Information", "7B2B76D1-62C7-4E16-B3C5-AB6C2F77A5F2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Add Account Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Add Account Text", "AddAccountText", "Add Account Text", @"The button text to display for adding an additional account", 3, @"Add Another Account", "D2AA5423-521E-418A-82D9-211384852CBD" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Personal Info Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal Info Title", "PersonalInfoTitle", "Personal Info Title", @"The text to display as heading of section for entering personal information.", 4, @"Personal Information", "1DEC4DB5-2DCB-4BE0-A076-21DB50A486C3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Payment Info Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Info Title", "PaymentInfoTitle", "Payment Info Title", @"The text to display as heading of section for entering credit card or bank account information.", 5, @"Payment Information", "FFFD90A8-5514-4DA5-8BC3-B70766F1B56E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmation Title", "ConfirmationTitle", "Confirmation Title", @"The text to display as heading of section for confirming information entered.", 6, @"Confirm Information", "066202E7-807C-4EC2-AF5F-E3817AA94AC2" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirmation Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Header", "ConfirmationHeader", "Confirmation Header", @"The text (HTML) to display at the top of the confirmation section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 7, @"
<p>
    Please confirm the information below. Once you have confirmed that the information is
    accurate click the 'Finish' button to complete your transaction.
</p>
", "13619116-5AEF-433A-9BAC-AEF5FFC8B203" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirmation Footer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Footer", "ConfirmationFooter", "Confirmation Footer", @"The text (HTML) to display at the bottom of the confirmation section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 8, @"
<div class='alert alert-info'>
    By clicking the 'finish' button below I agree to allow {{ OrganizationName }}
    to transfer the amount above from my account. I acknowledge that I may
    update the transaction information at any time by returning to this website. Please
    call the Finance Office if you have any additional questions.
</div>
", "90745839-DE55-40FC-BF8D-E224F7E73E80" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Success Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Title", "SuccessTitle", "Success Title", @"The text to display as heading of section for displaying details of gift.", 9, @"Gift Information", "10432592-0CDE-4E51-A230-B7B2A443AE7C" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Success Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Header", "SuccessHeader", "Success Header", @"The text (HTML) to display at the top of the success section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 10, @"
<p>
    Thank you for your generous contribution.  Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively
    achieve our mission.  We are so grateful for your commitment.
</p>
", "156FC7DC-3375-4A18-B4FE-0F2FACC6C52C" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Success Footer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Footer", "SuccessFooter", "Success Footer", @"The text (HTML) to display at the bottom of the success section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 11, @"", "F5C6A155-DCC7-4A5E-AA97-6FE06C611965" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Save Account Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Account Title", "SaveAccountTitle", "Save Account Title", @"The text to display as heading of section for saving payment information.", 12, @"Make Giving Even Easier", "BE82D376-40E6-4D01-B176-24D1B9AC8E18" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Payment Comment Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Payment Comment Template", "PaymentComment", "Payment Comment Template", @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>", 13, @"", "665F39FE-4789-4816-B0E8-9807A6BD3780" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Anonymous Giving Tooltip
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Anonymous Giving Tooltip", "AnonymousGivingTooltip", "Anonymous Giving Tooltip", @"The tooltip for the 'Give Anonymously' checkbox.", 14, @"", "7A987876-C671-476D-9310-AC88D689BBE4" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Allow Account Options In URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Account Options In URL", "AllowAccountsInURL", "Allow Account Options In URL", @"Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true", 1, @"False", "AC0337AB-7E24-4551-B1F2-C9C611062351" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Only Public Accounts In URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Public Accounts In URL", "OnlyPublicAccountsInURL", "Only Public Accounts In URL", @"Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.", 2, @"True", "051F68CC-CD12-4CAD-8D16-66AF732BEC7B" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Invalid Account Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Account Message", "InvalidAccountMessage", "Invalid Account Message", @"Display this text (HTML) as an error alert if an invalid 'account' or 'glaccount' is passed through the URL.", 3, @"The configured financial accounts are not valid for accepting financial transactions.", "7DB43B98-C525-4040-8787-C1E983699D27" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Account Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Account Campus Context", "AccountCampusContext", "Account Campus Context", @"Should any context be applied to the Account List", 4, @"-1", "06DADD84-20D2-4F21-9509-A411AAF8A53E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Allowed Transaction Attributes From URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Allowed Transaction Attributes From URL", "AllowedTransactionAttributesFromURL", "Allowed Transaction Attributes From URL", @"Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world", 5, @"", "C7D3C3E7-E923-4F65-A1E4-7F53102A3E50" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Transaction Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "TransactionType", "Transaction Type", @"", 6, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "DB54410D-A859-468B-9117-0F281D8C369D" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Transaction Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Transaction Entity Type", "TransactionEntityType", "Transaction Entity Type", @"The Entity Type for the Transaction Detail Record (usually left blank)", 7, @"", "6EFFD345-01DB-4FBD-B865-15CAA7FDE39D" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Entity Id Param
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Id Param", "EntityIdParam", "Entity Id Param", @"The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)", 8, @"", "EF60A18C-89DC-4988-AD5D-3EEF5B171429" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Transaction Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Transaction Header", "TransactionHeader", "Transaction Header", @"The Lava template which will be displayed prior to the Amount entry", 9, @"", "E31770A3-550E-427B-880E-A658DB0834BC" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Initial Back button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "997588CE-012D-4A9A-BEB4-A460B6A2090C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Initial Back button", "EnableInitialBackbutton", "Enable Initial Back button", @"Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry", 10, @"False", "3A736073-7080-4D83-A6B7-4C892F9B26BE" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("64EF8FE8-2348-4225-BF2C-3C8789DF4169","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Initial Back button
            RockMigrationHelper.DeleteAttribute("3A736073-7080-4D83-A6B7-4C892F9B26BE");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Transaction Header
            RockMigrationHelper.DeleteAttribute("E31770A3-550E-427B-880E-A658DB0834BC");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Entity Id Param
            RockMigrationHelper.DeleteAttribute("EF60A18C-89DC-4988-AD5D-3EEF5B171429");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Transaction Entity Type
            RockMigrationHelper.DeleteAttribute("6EFFD345-01DB-4FBD-B865-15CAA7FDE39D");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Transaction Type
            RockMigrationHelper.DeleteAttribute("DB54410D-A859-468B-9117-0F281D8C369D");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Allowed Transaction Attributes From URL
            RockMigrationHelper.DeleteAttribute("C7D3C3E7-E923-4F65-A1E4-7F53102A3E50");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Account Campus Context
            RockMigrationHelper.DeleteAttribute("06DADD84-20D2-4F21-9509-A411AAF8A53E");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Invalid Account Message
            RockMigrationHelper.DeleteAttribute("7DB43B98-C525-4040-8787-C1E983699D27");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Only Public Accounts In URL
            RockMigrationHelper.DeleteAttribute("051F68CC-CD12-4CAD-8D16-66AF732BEC7B");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Allow Account Options In URL
            RockMigrationHelper.DeleteAttribute("AC0337AB-7E24-4551-B1F2-C9C611062351");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Anonymous Giving Tooltip
            RockMigrationHelper.DeleteAttribute("7A987876-C671-476D-9310-AC88D689BBE4");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Payment Comment Template
            RockMigrationHelper.DeleteAttribute("665F39FE-4789-4816-B0E8-9807A6BD3780");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Save Account Title
            RockMigrationHelper.DeleteAttribute("BE82D376-40E6-4D01-B176-24D1B9AC8E18");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Success Footer
            RockMigrationHelper.DeleteAttribute("F5C6A155-DCC7-4A5E-AA97-6FE06C611965");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Success Header
            RockMigrationHelper.DeleteAttribute("156FC7DC-3375-4A18-B4FE-0F2FACC6C52C");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Success Title
            RockMigrationHelper.DeleteAttribute("10432592-0CDE-4E51-A230-B7B2A443AE7C");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirmation Footer
            RockMigrationHelper.DeleteAttribute("90745839-DE55-40FC-BF8D-E224F7E73E80");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirmation Header
            RockMigrationHelper.DeleteAttribute("13619116-5AEF-433A-9BAC-AEF5FFC8B203");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirmation Title
            RockMigrationHelper.DeleteAttribute("066202E7-807C-4EC2-AF5F-E3817AA94AC2");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Payment Info Title
            RockMigrationHelper.DeleteAttribute("FFFD90A8-5514-4DA5-8BC3-B70766F1B56E");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Personal Info Title
            RockMigrationHelper.DeleteAttribute("1DEC4DB5-2DCB-4BE0-A076-21DB50A486C3");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Add Account Text
            RockMigrationHelper.DeleteAttribute("D2AA5423-521E-418A-82D9-211384852CBD");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Contribution Info Title
            RockMigrationHelper.DeleteAttribute("7B2B76D1-62C7-4E16-B3C5-AB6C2F77A5F2");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Panel Title
            RockMigrationHelper.DeleteAttribute("C1DF62FB-9FDF-4D3C-B940-0D9838B4BC7E");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Receipt Email
            RockMigrationHelper.DeleteAttribute("0DCB60DD-14DD-4BB1-8AFA-7638858A7099");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Confirm Account
            RockMigrationHelper.DeleteAttribute("DC597056-84AE-43A4-BFEB-4C93DD2E0AEB");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Anonymous Giving
            RockMigrationHelper.DeleteAttribute("0E6BB29E-7C0B-4119-8CB8-7EB389C64E7D");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Business Giving
            RockMigrationHelper.DeleteAttribute("02B65889-8D7A-4B9D-9AD3-FD6555538F70");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Comment Entry Label
            RockMigrationHelper.DeleteAttribute("E6A0F338-2310-40AF-9089-0B8D2538E112");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Comment Entry
            RockMigrationHelper.DeleteAttribute("FA847E1A-88E2-4561-B56F-32C2F47781BC");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute("9C1038F2-6E3C-471B-8D59-EA32051E288A");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute("39670D61-6267-4CA9-A6BB-4D51D6B19C47");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Address Type
            RockMigrationHelper.DeleteAttribute("CEF0EBFD-CA6C-43E8-940B-D78D2C6BFA71");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Prompt for Email
            RockMigrationHelper.DeleteAttribute("B6C8F874-580C-4DCA-B605-F48499FFF291");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Prompt for Phone
            RockMigrationHelper.DeleteAttribute("CC559701-4611-403E-8DC2-7F45C2BB367D");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Scheduled Transactions
            RockMigrationHelper.DeleteAttribute("66377AA4-C948-4399-B16A-322A06588770");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Additional Accounts
            RockMigrationHelper.DeleteAttribute("C9F58154-5899-4A83-AD0E-AED2DB7749CA");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute("A4565ACC-CC7E-4341-AA63-D4E097CABB52");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Account Header Template
            RockMigrationHelper.DeleteAttribute("BFA1B019-AE44-4EB7-B94F-09491DC742F7");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Layout Style
            RockMigrationHelper.DeleteAttribute("B754B186-9393-4AA7-96BB-98F4DCC3F7B5");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Impersonation
            RockMigrationHelper.DeleteAttribute("46ABD47A-0764-4B10-BB24-67E4A2E23008");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Source
            RockMigrationHelper.DeleteAttribute("EC034C57-8673-4DA4-A644-C410A1AD99B9");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Batch Name Prefix
            RockMigrationHelper.DeleteAttribute("53209D5D-EB38-4514-BCB2-BF570D0FFC6B");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable Credit Card
            RockMigrationHelper.DeleteAttribute("BA1056D9-583D-42B7-97CF-7A0FFBB8A937");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Enable ACH
            RockMigrationHelper.DeleteAttribute("339F050F-2029-4D32-9F0B-B02C57CF727F");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry 
            //   Category: Finance
            //   Attribute: Financial Gateway
            RockMigrationHelper.DeleteAttribute("04A2A3EF-A421-4D01-9623-F7EAEC3D565B");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("64EF8FE8-2348-4225-BF2C-3C8789DF4169");

            // Delete BlockType 
            //   Name: Utility Payment Entry 
            //   Category: Finance
            //   Path: ~/Blocks/Finance/UtilityPaymentEntry.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("997588CE-012D-4A9A-BEB4-A460B6A2090C");
        }
    }
}

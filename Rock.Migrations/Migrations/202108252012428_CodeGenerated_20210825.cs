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
    public partial class CodeGenerated_20210825 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add/Update BlockType Media Element Analytics
            RockMigrationHelper.UpdateBlockType("Media Element Analytics","Analytic details of a Media Element","~/Blocks/Cms/MediaElementAnalytics.ascx","CMS","016B007C-731A-4912-BAB1-B1E3D55D59F0");

            // Add/Update BlockType Media Element Play Analytics
            RockMigrationHelper.UpdateBlockType("Media Element Play Analytics","Shows detailed analytics of a Media Element","~/Blocks/Cms/MediaElementPlayAnalytics.ascx","CMS","D349D320-1F42-4858-9348-0DB72A479433");

            // Add/Update BlockType Prayer Card View
            RockMigrationHelper.UpdateBlockType("Prayer Card View","provides an additional experience to pray using a card based view.","~/Blocks/Prayer/PrayerCardView.ascx","Prayer","1FEE129E-E46A-4805-AF5A-6F98E1DA7A16");

            // Attribute for BlockType: Family Pre Registration:Parent Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Parent Workflow", "ParentWorkflow", "Parent Workflow", @"If set, this workflow type will launch for each parent provided. The parent will be passed to the workflow as the Entity.", 11, @"", "343FC1E4-F455-4D8D-830E-8B5EEA07BF7B" );

            // Attribute for BlockType: Family Pre Registration:Child Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Child Workflow", "ChildWorkflow", "Child Workflow", @"If set, this workflow type will launch for each child provided. The child will be passed to the workflow as the Entity.", 12, @"", "3A01EB96-4D42-45A6-9CE6-275D37C2751D" );

            // Attribute for BlockType: Family Pre Registration:Number of Columns
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Number of Columns", "Columns", "Number of Columns", @"How many columns should be used to display the form.", 15, @"4", "88806647-3277-42EB-ABFF-AE4C68F759F6" );

            // Attribute for BlockType: Family Pre Registration:Display Communication Preference
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Communication Preference", "AdultDisplayCommunicationPreference", "Display Communication Preference", @"How should Communication Preference be displayed for adults?", 7, @"Hide", "9204F4CE-1782-4783-BA3F-7D10BC4E0809" );

            // Attribute for BlockType: Family Pre Registration:Display Communication Preference
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Communication Preference", "ChildDisplayCommunicationPreference", "Display Communication Preference", @"How should Communication Preference be displayed for children?", 7, @"Hide", "6165AC82-6A3D-4C16-97DB-F6E4BB849804" );

            // Attribute for BlockType: Media Element List:Add Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Page", "AddPage", "Add Page", @"", 1, @"", "CDABD585-F195-456C-BEB4-070337B6756B" );

            // Attribute for BlockType: SMS Conversations:Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3497603B-3BE6-4262-B7E9-EC01FC7140EB", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "Note Types", @"Optional list of note types to limit the note editor to.", 7, @"", "07AC139A-DF55-434F-80F4-A5131535F62E" );

            // Attribute for BlockType: Rock Solid Church Sample Data:Random Number Seed
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Random Number Seed", "RandomNumberSeed", "Random Number Seed", @"If given, the randomizer used during the creation of attendance and financial transactions will be predictable. Use 0 to use a random seed.", 5, @"1", "CE4DF44B-BC8E-4EFA-9BA0-33CC5EF0A669" );

            // Attribute for BlockType: Group Schedule Status Board:Hide Group Picker
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1BFB72CC-A224-4A0B-B291-21733597738A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Group Picker", "HideGroupPicker", "Hide Group Picker", @"When enabled, the group picker will be hidden.", 4, @"False", "C0603277-1DEC-4316-BC0E-D5AAF721D19B" );

            // Attribute for BlockType: Group Schedule Status Board:Hide Date Setting
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1BFB72CC-A224-4A0B-B291-21733597738A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Date Setting", "HideDateSetting", "Hide Date Setting", @"When enabled, the Dates setting button will be hidden.", 5, @"False", "552B15D5-A9B5-4FB7-A2B4-CFC1EECD6073" );

            // Attribute for BlockType: Group Schedule Toolbox:Scheduler Receive Confirmation Emails
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "Scheduler Receive Confirmation Emails", @"If checked, the scheduler will receive an email response for each confirmation or decline.", 4, @"False", "4F44D7DE-BED9-4767-8373-759F86945A64" );

            // Attribute for BlockType: Group Schedule Toolbox:Scheduling Response Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Scheduling Response Email", "SchedulingResponseEmail", "Scheduling Response Email", @"The system email that will be used for sending responses back to the scheduler.", 5, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "EDFB7D55-17CC-4D5C-8709-01E72987198B" );

            // Attribute for BlockType: Metric Detail:Show Chart
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "Show Chart", "Show Chart", @"", 0, @"true", "227CF64A-CDC1-4413-98AD-1E672B5DBEDF" );

            // Attribute for BlockType: Attribute Values:Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F5482D1-F510-4055-A000-F432BDEF8D1F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "Category", @"The Attribute Categories to display attributes from", 0, @"", "F4B55A50-BF81-4300-BC79-28F6729313CA" );

            // Attribute for BlockType: Attribute Values:Attribute Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F5482D1-F510-4055-A000-F432BDEF8D1F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Order", "AttributeOrder", "Attribute Order", @"The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.", 1, @"", "5D1928B1-2CB8-4246-A302-6495F2EF13C6" );

            // Attribute for BlockType: Attribute Values:Use Abbreviated Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F5482D1-F510-4055-A000-F432BDEF8D1F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Abbreviated Name", "UseAbbreviatedName", "Use Abbreviated Name", @"Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.", 2, @"False", "83A3312B-DEEE-494A-B1F1-3D3B34B7BC69" );

            // Attribute for BlockType: Attribute Values:Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F5482D1-F510-4055-A000-F432BDEF8D1F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title", "BlockTitle", "Block Title", @"The text to display as the heading.", 3, @"", "C6FD2CFA-E982-4BB0-8FE1-3F94B2A6AE5C" );

            // Attribute for BlockType: Attribute Values:Block Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F5482D1-F510-4055-A000-F432BDEF8D1F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Icon", "BlockIcon", "Block Icon", @"The css class name to use for the heading icon.", 4, @"", "72904743-07BC-4139-B86B-E0BE1E7A3970" );

            // Attribute for BlockType: Attribute Values:Show Category Names as Separators
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F5482D1-F510-4055-A000-F432BDEF8D1F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category Names as Separators", "ShowCategoryNamesasSeparators", "Show Category Names as Separators", @"If enabled, attributes will be grouped by category and will include the category name as a heading separator.", 5, @"False", "A349DB34-E5F2-4C12-AFCF-1B42FE6B55C9" );

            // Attribute for BlockType: Registration Entry:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "D9C6B155-2FD5-4F57-BC3E-C4125E2679EB" );

            // Attribute for BlockType: Registration Entry:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 1, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "EA8FA165-64CA-4C02-8376-FBF56F13D250" );

            // Attribute for BlockType: Registration Entry:Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "Source", @"The Financial Source Type to use when creating transactions", 2, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "C3DFB7D1-9052-4127-9492-72CB0B3F3789" );

            // Attribute for BlockType: Registration Entry:Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch", 3, @"Event Registration", "62721EBE-18FB-4E49-A1D4-A59D4DE2EBF3" );

            // Attribute for BlockType: Registration Entry:Display Progress Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Progress Bar", "DisplayProgressBar", "Display Progress Bar", @"Display a progress bar for the registration.", 4, @"True", "8A2E2E5A-CFFD-47F8-98C3-CAFF669BFC0E" );

            // Attribute for BlockType: Registration Entry:Allow InLine Digital Signature Documents
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow InLine Digital Signature Documents", "SignInline", "Allow InLine Digital Signature Documents", @"Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline", 6, @"True", "023CFEE9-F8AE-43E1-A558-44FE44B73FF5" );

            // Attribute for BlockType: Registration Entry:Family Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Term", "FamilyTerm", "Family Term", @"The term to use for specifying which household or family a person is a member of.", 8, @"immediate family", "5076D569-A42A-4994-8F02-796B510C11B8" );

            // Attribute for BlockType: Registration Entry:Force Email Update
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Force Email Update", "ForceEmailUpdate", "Force Email Update", @"Force the email to be updated on the person's record.", 9, @"False", "A99E98C6-58D1-490E-AD9D-52B4FFE7D4A9" );

            // Attribute for BlockType: Registration Entry:Show Field Descriptions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Field Descriptions", "ShowFieldDescriptions", "Show Field Descriptions", @"Show the field description as help text", 10, @"True", "FBC3EA68-F9BF-4370-A40F-1C61564E47D3" );

            // Attribute for BlockType: Registration Entry:Enabled Saved Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F40C95D6-D765-40D5-AE98-A1B5B0994642", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enabled Saved Account", "EnableSavedAccount", "Enabled Saved Account", @"Set this to false to disable the using Saved Account as a payment option, and to also disable the option to create saved account for future use.", 11, @"True", "38E3EC7A-3AFD-495B-88C7-71DDA6286A00" );

            // Attribute for BlockType: Transaction Entry:Financial Gateway
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Financial Gateway", "FinancialGateway", "Financial Gateway", @"The payment gateway to use for Credit Card and ACH transactions.", 0, @"", "8959F6D5-B89A-467C-90DA-A12784AA21E9" );

            // Attribute for BlockType: Transaction Entry:Enable ACH
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "Enable ACH", @"", 1, @"False", "6EC6250E-DB96-4FA7-AD9A-359974EE89A2" );

            // Attribute for BlockType: Transaction Entry:Enable Credit Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Credit Card", "EnableCreditCard", "Enable Credit Card", @"", 2, @"True", "B8800DBF-F92E-4744-841D-4DC0C3A44C41" );

            // Attribute for BlockType: Transaction Entry:Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch.", 3, @"Online Giving", "A32860A4-0E35-4888-836E-1989BAAA7858" );

            // Attribute for BlockType: Transaction Entry:Financial Source Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Financial Source Type", "FinancialSourceType", "Financial Source Type", @"The Financial Source Type to use when creating transactions", 19, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "871BFFB3-5FE9-4448-BD63-0247FE6D23D4" );

            // Attribute for BlockType: Transaction Entry:Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "AccountsToDisplay", "Accounts", @"The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.", 5, @"", "D6ABFE08-C95C-486C-BDBC-6E934EFAC3CE" );

            // Attribute for BlockType: Transaction Entry:Ask for Campus if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ask for Campus if Known", "AskForCampusIfKnown", "Ask for Campus if Known", @"If the campus for the person is already known, should the campus still be prompted for?", 10, @"True", "8E986F12-9A62-4BC7-B38A-33147F778CA8" );

            // Attribute for BlockType: Transaction Entry:Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Set this to true to include inactive campuses", 10, @"False", "82173BA8-3A4B-4887-AD29-62ADC8475B78" );

            // Attribute for BlockType: Transaction Entry:Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "IncludedCampusTypes", "Campus Types", @"Set this to limit campuses by campus type.", 11, @"", "CCE7D1DD-6DBD-48F9-806C-AF4E48C43291" );

            // Attribute for BlockType: Transaction Entry:Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "IncludedCampusStatuses", "Campus Statuses", @"Set this to limit campuses by campus status.", 12, @"", "83FDBE95-841B-4A07-8313-A61B1834501A" );

            // Attribute for BlockType: Transaction Entry:Enable Multi-Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Multi-Account", "EnableMultiAccount", "Enable Multi-Account", @"Should the person be able specify amounts for more than one account?", 13, @"True", "5FDF9D34-E6C8-4E29-A38F-38C8B9504247" );

            // Attribute for BlockType: Transaction Entry:Enable Business Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Business Giving", "EnableBusinessGiving", "Enable Business Giving", @"Should the option to give as a business be displayed.", 999, @"True", "F8669093-188B-4B68-9D20-63A3FE467A82" );

            // Attribute for BlockType: Transaction Entry:Enable Anonymous Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Anonymous Giving", "EnableAnonymousGiving", "Enable Anonymous Giving", @"Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fund-raising contributors.", 24, @"False", "99ADE80F-48C7-440A-B2A1-3BB783CD339B" );

            // Attribute for BlockType: Transaction Entry:Anonymous Giving Tool-tip
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Anonymous Giving Tool-tip", "AnonymousGivingTooltip", "Anonymous Giving Tool-tip", @"The tool-tip for the 'Give Anonymously' check box.", 25, @"", "5B2A01D5-6A69-41FB-815E-42CFD4D4ACAF" );

            // Attribute for BlockType: Transaction Entry:Scheduled Transactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduled Transactions", "AllowScheduledTransactions", "Scheduled Transactions", @"If the selected gateway(s) allow scheduled transactions, should that option be provided to user.", 1, @"True", "C3BB3F7D-3D0C-4A75-8D3B-F3213EC62753" );

            // Attribute for BlockType: Transaction Entry:Show Scheduled Gifts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Scheduled Gifts", "ShowScheduledTransactions", "Show Scheduled Gifts", @"If the person has any scheduled gifts, show a summary of their scheduled gifts.", 2, @"True", "8901E6C5-161D-4C0A-A875-14694782C922" );

            // Attribute for BlockType: Transaction Entry:Scheduled Gifts Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Scheduled Gifts Template", "ScheduledTransactionsTemplate", "Scheduled Gifts Template", @"The Lava Template to use to display Scheduled Gifts.", 3, @"
<h4>Scheduled {{ GiftTerm | Pluralize }}</h4>

{% for scheduledTransaction in ScheduledTransactions %}
    <div class='scheduled-transaction js-scheduled-transaction' data-scheduled-transaction-id='{{ scheduledTransaction.Id }}' data-expanded='{{ ExpandedStates[scheduledTransaction.Id] }}'>
        <div class='panel panel-default'>
            <div class='panel-heading'>
                <span class='panel-title h1'>
                    <i class='fa fa-calendar'></i>
                    {{ scheduledTransaction.TransactionFrequencyValue.Value }}
                </span>

                <span class='js-scheduled-totalamount scheduled-totalamount margin-l-md'>
                    {{ scheduledTransaction.TotalAmount | FormatAsCurrency }}
                </span>

                <div class='panel-actions pull-right'>
                    <span class='js-toggle-scheduled-details toggle-scheduled-details clickable fa fa-chevron-down'></span>
                </div>
            </div>

            <div class='js-scheduled-details scheduled-details margin-l-lg'>
                <div class='panel-body'>
                    {% for scheduledTransactionDetail in scheduledTransaction.ScheduledTransactionDetails %}
                        <div class='account-details'>
                            <span class='scheduled-transaction-account control-label'>
                                {{ scheduledTransactionDetail.Account.PublicName }}
                            </span>
                            <br />
                            <span class='scheduled-transaction-amount'>
                                {{ scheduledTransactionDetail.Amount | FormatAsCurrency }}
                            </span>
                        </div>
                    {% endfor %}

                    <br />
                    <span class='scheduled-transaction-payment-detail'>
                        {% assign financialPaymentDetail = scheduledTransaction.FinancialPaymentDetail %}

                        {% if financialPaymentDetail.CurrencyTypeValue.Value != 'Credit Card' %}
                            {{ financialPaymentDetail.CurrencyTypeValue.Value }}
                        {% else %}
                            {{ financialPaymentDetail.CreditCardTypeValue.Value }} {{ financialPaymentDetail.AccountNumberMasked }}
                        {% endif %}
                    </span>
                    <br />

                    {% if scheduledTransaction.NextPaymentDate != null %}
                        Next Gift: {{ scheduledTransaction.NextPaymentDate | Date:'sd' }}.
                    {% endif %}


                    <div class='scheduled-details-actions margin-t-md'>
                        {% if LinkedPages.ScheduledTransactionEditPage != '' %}
                            <a href='{{ LinkedPages.ScheduledTransactionEditPage }}?ScheduledTransactionId={{ scheduledTransaction.Id }}'>Edit</a>
                        {% endif %}
                        <a class='margin-l-sm' onclick=""{{ scheduledTransaction.Id | Postback:'DeleteScheduledTransaction' }}"">Delete</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
{% endfor %}


<script type='text/javascript'>

    // Scheduled Transaction JavaScripts
    function setScheduledDetailsVisibility($container, animate) {
        var $scheduledDetails = $container.find('.js-scheduled-details');
        var expanded = $container.attr('data-expanded');
        var $totalAmount = $container.find('.js-scheduled-totalamount');
        var $toggle = $container.find('.js-toggle-scheduled-details');

        if (expanded == 1) {
            if (animate) {
                $scheduledDetails.slideDown();
                $totalAmount.fadeOut();
            } else {
                $scheduledDetails.show();
                $totalAmount.hide();
            }

            $toggle.removeClass('fa-chevron-down').addClass('fa-chevron-up');
        } else {
            if (animate) {
                $scheduledDetails.slideUp();
                $totalAmount.fadeIn();
            } else {
                $scheduledDetails.hide();
                $totalAmount.show();
            }

            $toggle.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        }
    };

    Sys.Application.add_load(function () {
        var $scheduleDetailsContainers = $('.js-scheduled-transaction');

        $scheduleDetailsContainers.each(function (index) {
            setScheduledDetailsVisibility($($scheduleDetailsContainers[index]), false);
        });

        var $toggleScheduledDetails = $('.js-toggle-scheduled-details');
        $toggleScheduledDetails.on('click', function () {
            var $scheduledDetailsContainer = $(this).closest('.js-scheduled-transaction');
            if ($scheduledDetailsContainer.attr('data-expanded') == 1) {
                $scheduledDetailsContainer.attr('data-expanded', 0);
            } else {
                $scheduledDetailsContainer.attr('data-expanded', 1);
            }

            setScheduledDetailsVisibility($scheduledDetailsContainer, true);
        });
    });
</script>
", "B5CDB2BF-C6BF-486B-A6EB-1AA0A31A0720" );

            // Attribute for BlockType: Transaction Entry:Scheduled Transaction Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Edit Page", "ScheduledTransactionEditPage", "Scheduled Transaction Edit Page", @"The page to use for editing scheduled transactions.", 4, @"", "3001F608-C906-4B17-AB33-85D5268F932B" );

            // Attribute for BlockType: Transaction Entry:Enable Comment Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Comment Entry", "EnableCommentEntry", "Enable Comment Entry", @"Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", 1, @"False", "30686EF0-D67F-4302-9075-ABF0A6D6E87F" );

            // Attribute for BlockType: Transaction Entry:Comment Entry Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Entry Label", "CommentEntryLabel", "Comment Entry Label", @"The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", 2, @"Comment", "643D862D-36B9-411D-9E63-08C89D789EAA" );

            // Attribute for BlockType: Transaction Entry:Payment Comment Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Payment Comment Template", "PaymentCommentTemplate", "Payment Comment Template", @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>", 3, @"", "BD095269-4739-4E88-A8D4-0046E712701E" );

            // Attribute for BlockType: Transaction Entry:Save Account Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Account Title", "SaveAccountTitle", "Save Account Title", @"The text to display as heading of section for saving payment information.", 1, @"Make Giving Even Easier", "16C5A32D-0491-42FB-8AAA-561DE326531E" );

            // Attribute for BlockType: Transaction Entry:Intro Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Intro Message", "IntroMessageTemplate", "Intro Message", @"The text to place at the top of the amount entry. <span class='tip tip-lava'></span>", 2, @"<h2>Your Generosity Changes Lives</h2>", "A389228B-D208-48D0-8EB0-A72B3B4BDAA5" );

            // Attribute for BlockType: Transaction Entry:Gift Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Term", "GiftTerm", "Gift Term", @"", 3, @"Gift", "9FF6C4FA-270E-4F63-A953-5272F5395DF3" );

            // Attribute for BlockType: Transaction Entry:Give Button Text - Now 
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text - Now ", "GiveButtonNowText", "Give Button Text - Now ", @"", 4, @"Give Now", "582BB0BD-FFA3-4043-B9B3-F18489A08D39" );

            // Attribute for BlockType: Transaction Entry:Give Button Text - Scheduled
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text - Scheduled", "GiveButtonScheduledText", "Give Button Text - Scheduled", @"", 5, @"Schedule Your Gift", "32AE5D26-B7CA-4D39-AC55-73B40E8B7C99" );

            // Attribute for BlockType: Transaction Entry:Amount Summary Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Amount Summary Template", "AmountSummaryTemplate", "Amount Summary Template", @"The text (HTML) to display on the amount summary page. <span class='tip tip-lava'></span>", 6, @"
{% assign sortedAccounts = Accounts | Sort:'Order,PublicName' %}

<span class='account-names'>{{ sortedAccounts | Map:'PublicName' | Join:', ' | ReplaceLast:',',' and' }}</span>
-
<span class='account-campus'>{{ Campus.Name }}</span>", "6701723C-4E5B-4289-B814-E7BBE4E0A7C0" );

            // Attribute for BlockType: Transaction Entry:Finish Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Finish Lava Template", "FinishLavaTemplate", "Finish Lava Template", @"The text (HTML) to display on the success page. <span class='tip tip-lava'></span>", 7, @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

<h1>Thank You!</h1>

<p>Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively achieve our
mission. We are so grateful for your commitment.</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>

    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount | FormatAsCurrency }}</dd>
    {% endfor %}
    <dd></dd>

    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>

    {% if Transaction.TransactionFrequencyValue %}
        {{ Transaction.TransactionFrequencyValue.Value }} starting on {{ Transaction.NextPaymentDate | Date:'sd' }}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
", "A1923237-07B7-4415-8BAA-FB24E7CFC6B4" );

            // Attribute for BlockType: Transaction Entry:Confirm Account Email Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account Email Template", "ConfirmAccountEmailTemplate", "Confirm Account Email Template", @"The Email Template to use when confirming a new account", 1, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "449EF45B-5503-47B1-8D10-32AAE252D07D" );

            // Attribute for BlockType: Transaction Entry:Receipt Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Receipt Email", "ReceiptEmail", "Receipt Email", @"The system email to use to send the receipt.", 2, @"", "0761DB59-005E-46F8-B428-146B4E405A63" );

            // Attribute for BlockType: Transaction Entry:Prompt for Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Phone", "PromptForPhone", "Prompt for Phone", @"Should the user be prompted for their phone number?", 1, @"False", "EA4AB682-6CD7-4AE4-9961-6B93F6EC8EE5" );

            // Attribute for BlockType: Transaction Entry:Prompt for Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Email", "PromptForEmail", "Prompt for Email", @"Should the user be prompted for their email address?", 2, @"True", "32D8AE91-14B8-48B7-964B-817FCA096273" );

            // Attribute for BlockType: Transaction Entry:Address Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "PersonAddressType", "Address Type", @"The location type to use for the person's address", 3, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "5A9AE8C6-900F-4388-B3B6-ED1251D0FD6D" );

            // Attribute for BlockType: Transaction Entry:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "PersonConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 4, @"368DD475-242C-49C4-A42C-7278BE690CC2", "5D99AAC8-32D4-4545-8AE7-B8B099D0FF22" );

            // Attribute for BlockType: Transaction Entry:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "PersonRecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 5, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "75403E54-0F12-4DC3-8D2A-135B9445299F" );

            // Attribute for BlockType: Transaction Entry:Transaction Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "Transaction Type", "Transaction Type", @"", 1, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "FFF97C7A-76B9-479F-9E9F-DE83810EAF76" );

            // Attribute for BlockType: Transaction Entry:Transaction Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Transaction Entity Type", "TransactionEntityType", "Transaction Entity Type", @"The Entity Type for the Transaction Detail Record (usually left blank)", 2, @"", "3C83EF84-FF46-44F8-90C2-216BFAC602C6" );

            // Attribute for BlockType: Transaction Entry:Entity Id Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Id Parameter", "EntityIdParam", "Entity Id Parameter", @"The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)", 3, @"", "EDF4F34A-BE10-438B-A975-38D38F3A2C28" );

            // Attribute for BlockType: Transaction Entry:Allowed Transaction Attributes From URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Allowed Transaction Attributes From URL", "AllowedTransactionAttributesFromURL", "Allowed Transaction Attributes From URL", @"Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world", 4, @"", "1CF4CB57-35F0-40D4-9417-FA6613F0F8EA" );

            // Attribute for BlockType: Transaction Entry:Allow Account Options In URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Account Options In URL", "AllowAccountOptionsInURL", "Allow Account Options In URL", @"Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true", 5, @"False", "152C9CC7-BEC9-4AC8-ABA8-6DDD24138C8D" );

            // Attribute for BlockType: Transaction Entry:Only Public Accounts In URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Public Accounts In URL", "OnlyPublicAccountsInURL", "Only Public Accounts In URL", @"Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.", 6, @"True", "3EF1267D-D97A-4C16-BCDA-19DFBE8DC241" );

            // Attribute for BlockType: Transaction Entry:Invalid Account Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Account Message", "InvalidAccountInURLMessage", "Invalid Account Message", @"Display this text (HTML) as an error alert if an invalid 'account' or 'GL account' is passed through the URL. Leave blank to just ignore the invalid accounts and not show a message.", 7, @"", "CE65F045-2411-4A50-83FA-A0921EBF1087" );

            // Attribute for BlockType: Transaction Entry:Enable Initial Back button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Initial Back button", "EnableInitialBackButton", "Enable Initial Back button", @"Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry", 8, @"False", "AB362A8F-D962-400F-9EC0-94D9C3E70116" );

            // Attribute for BlockType: Transaction Entry:Impersonation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "115681F4-C14A-4FA7-A2C0-E6B65C936940", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonation", "AllowImpersonation", "Impersonation", @"Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.", 9, @"False", "7CACC04F-196C-4E36-A7B4-062A1168064C" );

            // Attribute for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBF264F8-D7CA-452F-8AAD-9535E78A55AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "8890A7A4-3BB3-4E4C-BC5B-D4CF8AA7FDD8" );

            // Attribute for BlockType: Login:Forgot Password URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBF264F8-D7CA-452F-8AAD-9535E78A55AD", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password URL", "ForgotPasswordUrl", "Forgot Password URL", @"The URL to link the user to when they have forgotten their password.", 1, @"", "2FEA899F-E8C2-4544-A12E-63819C201E3B" );

            // Attribute for BlockType: Login:Locked Out Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBF264F8-D7CA-452F-8AAD-9535E78A55AD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Locked Out Caption", "LockedOutCaption", "Locked Out Caption", @"The text (HTML) to display when a user's account has been locked.", 2, @"{%- assign phone = Global' | Attribute:'OrganizationPhone' | Trim -%}
{%- assign email = Global' | Attribute:'OrganizationEmail' | Trim -%}
Sorry, your account has been locked.  Please
{% if phone != '' %}
    contact our office at {{ phone }} or email
{% else %}
    email us at
{% endif %}
<a href='mailto:{{ email }}'>{{ email }}</a>
for help. Thank you.", "A01A0D02-51B7-47A7-8C80-D85CC778ECAC" );

            // Attribute for BlockType: Login:Confirm Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBF264F8-D7CA-452F-8AAD-9535E78A55AD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirm Caption", "ConfirmCaption", "Confirm Caption", @"The text (HTML) to display when a user's account needs to be confirmed.", 3, @"Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We’ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "C1265A03-0023-4DFD-BBE8-ED9A4078871F" );

            // Attribute for BlockType: Login:Help Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBF264F8-D7CA-452F-8AAD-9535E78A55AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Help Page", "HelpPage", "Help Page", @"Page to navigate to when user selects 'Help' option (if blank will use 'ForgotUserName' page route)", 4, @"", "75AC44B4-F5FA-4EE0-A6B1-DC8B2BA5BA6B" );

            // Attribute for BlockType: Stark Detail:Show Email Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0FD3EF01-6349-4E46-BA63-D99E7F6A516B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Address", "ShowEmailAddress", "Show Email Address", @"Should the email address be shown?", 1, @"True", "EB06C29B-8036-4A84-8F56-0A8EB532EFAE" );

            // Attribute for BlockType: Stark Detail:Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0FD3EF01-6349-4E46-BA63-D99E7F6A516B", "3D045CAE-EA72-4A04-B7BE-7FD1D6214217", "Email", "Email", "Email", @"The Email address to show.", 2, @"ted@rocksolidchurchdemo.com", "9381D0A1-965D-4F0E-8D3A-DDC84AAD829C" );

            // Attribute for BlockType: Prayer Card View:Display Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Display Lava Template", "DisplayLavaTemplate", "Display Lava Template", @"The Lava template that layouts out the view of the prayer requests.", 0, @"
<div class=""row"">
  {% assign prayedButtonText = PrayedButtonText %}
  {% for item in PrayerRequestItems %}
  <div class=""col-md-4 col-sm-6"">
    <h4>{{ item.FirstName }} {{ item.LastName }}</h4>
    <p>
        {% if item.Campus != null %}
        <span class=""label label-campus"">{{ item.Campus.Name }}</span>
        {% endif %}
    </p>
    <p>
        {{ item.Text }}
    </p>
    
	<div class=""actions margin-v-md clearfix"">
	    {% if EnablePrayerTeamFlagging == true %}
	    <a href = ""#"" onclick=""{{ item.Id | Postback:'Flag' }}""><i class='fa fa-flag'></i> Flag</a>
	    {% endif %}
		<a class=""btn btn-primary btn-sm pull-right"" href=""#"" onclick=""iPrayed(this);{{ item.Id | Postback:'Pray' }}"">Pray</a>
	</div>
   </div>
  {% endfor -%}
</div>
<script>function iPrayed(elmnt) { 
        var iPrayedText = '{{PrayedButtonText}}';
        elmnt.innerHTML = iPrayedText;
    }
</script>
", "6FA393C8-753C-4ABB-8A94-06C2B9814048" );

            // Attribute for BlockType: Prayer Card View:Prayed Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Prayed Button Text", "PrayedButtonText", "Prayed Button Text", @"The text to display inside the Prayed button.", 1, @"Prayed!", "1997C49E-73EB-4FFF-8D75-01DEC98E7518" );

            // Attribute for BlockType: Prayer Card View:Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"A top level category. This controls which categories are shown when starting a prayer session.", 2, @"", "2A064E67-0DCC-4329-B533-112B14695FD0" );

            // Attribute for BlockType: Prayer Card View:Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public prayer request will be excluded.", 3, @"True", "9508B7AA-11D8-40B8-82E0-1D2ECFB1141D" );

            // Attribute for BlockType: Prayer Card View:Enable Prayer Team Flagging
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Prayer Team Flagging", "EnablePrayerTeamFlagging", "Enable Prayer Team Flagging", @"If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.", 4, @"False", "D5F3698A-5AA7-4CF8-9DE5-A9EE665388C3" );

            // Attribute for BlockType: Prayer Card View:Flag Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Flag Limit", "FlagLimit", "Flag Limit", @"The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", 5, @"1", "D6070E1C-13BD-4D4D-AC47-6B6E95434AD8" );

            // Attribute for BlockType: Prayer Card View:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Order", "Order", "Order", @"The order that the requests should be displayed.", 6, @"", "6A86FB66-D67D-4FF4-97A1-68FD0B48EEA6" );

            // Attribute for BlockType: Prayer Card View:Show Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "Show Campus Filter", @"Shows or hides the campus filter.", 7, @"False", "23270774-4965-40F8-90D8-DCB445CED193" );

            // Attribute for BlockType: Prayer Card View:Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"Allows selecting which campus types to filter campuses by.", 8, @"", "47352FD4-65C0-497B-8B11-F8D9A98127FC" );

            // Attribute for BlockType: Prayer Card View:Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This allows selecting which campus statuses to filter campuses by.", 9, @"", "C32EE977-2D17-4319-9680-64413836EA5A" );

            // Attribute for BlockType: Prayer Card View:Max Results
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "Max Results", @"The maximum number of requests to display. Leave blank for all.", 10, @"", "E41C4EC8-079A-4579-9AE6-17F237CDCED2" );

            // Attribute for BlockType: Prayer Card View:Prayed Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Prayed Workflow", "PrayedWorkflow", "Prayed Workflow", @"The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic ""Entity"" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonId.", 11, @"", "56E44F32-4AB3-40B5-8ECA-20DEB331C151" );

            // Attribute for BlockType: Prayer Card View:Flagged Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Flagged Workflow", "FlaggedWorkflow", "Flagged Workflow", @"The workflow type to launch when someone presses the Flag button. Prayer Request will be passed to the workflow as a generic ""Entity"" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FlaggedByPersonId.", 12, @"", "DE231FD2-8EFC-49E9-A33D-E6700C5AB990" );

            // Attribute for BlockType: Notes:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity", 0, @"", "007B0AFA-B70E-45AC-8FAA-8BF353F8E7D8" );

            // Attribute for BlockType: Notes:Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "Note Types", @"Optional list of note types to limit display to", 1, @"", "003ADC8F-326B-4ABB-AA81-DAC589C72D04" );

            // Attribute for BlockType: Notes:Default Note Image
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Default Note Image", "DefaultNoteImage", "Default Note Image", @"This image is displayed next to the note if the author has no profile image.", 2, @"", "A20A683B-E4C2-4491-9975-D705A6660AA0" );

            // Attribute for BlockType: Scheduled Transaction Edit (V2):Impersonator can see saved accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonator can see saved accounts", "ImpersonatorCanSeeSavedAccounts", "Impersonator can see saved accounts", @"Should the current user be able to view other people's saved accounts?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", 2, @"False", "F5E6393E-2D14-4E0A-AA24-0AC92514F66C" );

            // Attribute for BlockType: Dynamic Data:Grid Header Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Grid Header Content", "GridHeaderContent", "Grid Header Content", @"This Lava template will be rendered above the grid. It will have access to the same dataset as the grid.", 0, @"", "4710072D-6243-42E1-9398-223A857C3482" );

            // Attribute for BlockType: Dynamic Data:Grid Footer Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Grid Footer Content", "GridFooterContent", "Grid Footer Content", @"This Lava template will be rendered below the grid (best used for custom totaling). It will have access to the same dataset as the grid.", 0, @"", "D86D202B-98D7-4F1F-A856-0ADABB4AEFA8" );

            // Attribute for BlockType: Page Parameter Filter:Hide Filter Actions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Filter Actions", "HideFilterActions", "Hide Filter Actions", @"Hides the filter buttons. This is useful when the Selection action is set to reload the page. Be sure to use this only when the page re-load will be quick.", 10, @"False", "140880DB-30B1-4EE3-8E41-6995FD4B53CF" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Hide Filter Actions Attribute for BlockType: Page Parameter Filter
            RockMigrationHelper.DeleteAttribute("140880DB-30B1-4EE3-8E41-6995FD4B53CF");

            // Grid Footer Content Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("D86D202B-98D7-4F1F-A856-0ADABB4AEFA8");

            // Grid Header Content Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("4710072D-6243-42E1-9398-223A857C3482");

            // Impersonator can see saved accounts Attribute for BlockType: Scheduled Transaction Edit (V2)
            RockMigrationHelper.DeleteAttribute("F5E6393E-2D14-4E0A-AA24-0AC92514F66C");

            // Default Note Image Attribute for BlockType: Notes
            RockMigrationHelper.DeleteAttribute("A20A683B-E4C2-4491-9975-D705A6660AA0");

            // Note Types Attribute for BlockType: Notes
            RockMigrationHelper.DeleteAttribute("003ADC8F-326B-4ABB-AA81-DAC589C72D04");

            // Entity Type Attribute for BlockType: Notes
            RockMigrationHelper.DeleteAttribute("007B0AFA-B70E-45AC-8FAA-8BF353F8E7D8");

            // Flagged Workflow Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("DE231FD2-8EFC-49E9-A33D-E6700C5AB990");

            // Prayed Workflow Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("56E44F32-4AB3-40B5-8ECA-20DEB331C151");

            // Max Results Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("E41C4EC8-079A-4579-9AE6-17F237CDCED2");

            // Campus Statuses Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("C32EE977-2D17-4319-9680-64413836EA5A");

            // Campus Types Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("47352FD4-65C0-497B-8B11-F8D9A98127FC");

            // Show Campus Filter Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("23270774-4965-40F8-90D8-DCB445CED193");

            // Order Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("6A86FB66-D67D-4FF4-97A1-68FD0B48EEA6");

            // Flag Limit Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("D6070E1C-13BD-4D4D-AC47-6B6E95434AD8");

            // Enable Prayer Team Flagging Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("D5F3698A-5AA7-4CF8-9DE5-A9EE665388C3");

            // Public Only Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("9508B7AA-11D8-40B8-82E0-1D2ECFB1141D");

            // Category Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("2A064E67-0DCC-4329-B533-112B14695FD0");

            // Prayed Button Text Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("1997C49E-73EB-4FFF-8D75-01DEC98E7518");

            // Display Lava Template Attribute for BlockType: Prayer Card View
            RockMigrationHelper.DeleteAttribute("6FA393C8-753C-4ABB-8A94-06C2B9814048");

            // Email Attribute for BlockType: Stark Detail
            RockMigrationHelper.DeleteAttribute("9381D0A1-965D-4F0E-8D3A-DDC84AAD829C");

            // Show Email Address Attribute for BlockType: Stark Detail
            RockMigrationHelper.DeleteAttribute("EB06C29B-8036-4A84-8F56-0A8EB532EFAE");

            // Help Page Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("75AC44B4-F5FA-4EE0-A6B1-DC8B2BA5BA6B");

            // Confirm Caption Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("C1265A03-0023-4DFD-BBE8-ED9A4078871F");

            // Locked Out Caption Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("A01A0D02-51B7-47A7-8C80-D85CC778ECAC");

            // Forgot Password URL Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("2FEA899F-E8C2-4544-A12E-63819C201E3B");

            // Registration Page Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("8890A7A4-3BB3-4E4C-BC5B-D4CF8AA7FDD8");

            // Impersonation Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("7CACC04F-196C-4E36-A7B4-062A1168064C");

            // Enable Initial Back button Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("AB362A8F-D962-400F-9EC0-94D9C3E70116");

            // Invalid Account Message Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("CE65F045-2411-4A50-83FA-A0921EBF1087");

            // Only Public Accounts In URL Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("3EF1267D-D97A-4C16-BCDA-19DFBE8DC241");

            // Allow Account Options In URL Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("152C9CC7-BEC9-4AC8-ABA8-6DDD24138C8D");

            // Allowed Transaction Attributes From URL Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("1CF4CB57-35F0-40D4-9417-FA6613F0F8EA");

            // Entity Id Parameter Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("EDF4F34A-BE10-438B-A975-38D38F3A2C28");

            // Transaction Entity Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("3C83EF84-FF46-44F8-90C2-216BFAC602C6");

            // Transaction Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("FFF97C7A-76B9-479F-9E9F-DE83810EAF76");

            // Record Status Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("75403E54-0F12-4DC3-8D2A-135B9445299F");

            // Connection Status Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("5D99AAC8-32D4-4545-8AE7-B8B099D0FF22");

            // Address Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("5A9AE8C6-900F-4388-B3B6-ED1251D0FD6D");

            // Prompt for Email Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("32D8AE91-14B8-48B7-964B-817FCA096273");

            // Prompt for Phone Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("EA4AB682-6CD7-4AE4-9961-6B93F6EC8EE5");

            // Receipt Email Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("0761DB59-005E-46F8-B428-146B4E405A63");

            // Confirm Account Email Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("449EF45B-5503-47B1-8D10-32AAE252D07D");

            // Finish Lava Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("A1923237-07B7-4415-8BAA-FB24E7CFC6B4");

            // Amount Summary Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("6701723C-4E5B-4289-B814-E7BBE4E0A7C0");

            // Give Button Text - Scheduled Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("32AE5D26-B7CA-4D39-AC55-73B40E8B7C99");

            // Give Button Text - Now  Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("582BB0BD-FFA3-4043-B9B3-F18489A08D39");

            // Gift Term Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("9FF6C4FA-270E-4F63-A953-5272F5395DF3");

            // Intro Message Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("A389228B-D208-48D0-8EB0-A72B3B4BDAA5");

            // Save Account Title Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("16C5A32D-0491-42FB-8AAA-561DE326531E");

            // Payment Comment Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("BD095269-4739-4E88-A8D4-0046E712701E");

            // Comment Entry Label Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("643D862D-36B9-411D-9E63-08C89D789EAA");

            // Enable Comment Entry Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("30686EF0-D67F-4302-9075-ABF0A6D6E87F");

            // Scheduled Transaction Edit Page Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("3001F608-C906-4B17-AB33-85D5268F932B");

            // Scheduled Gifts Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("B5CDB2BF-C6BF-486B-A6EB-1AA0A31A0720");

            // Show Scheduled Gifts Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("8901E6C5-161D-4C0A-A875-14694782C922");

            // Scheduled Transactions Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("C3BB3F7D-3D0C-4A75-8D3B-F3213EC62753");

            // Anonymous Giving Tool-tip Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("5B2A01D5-6A69-41FB-815E-42CFD4D4ACAF");

            // Enable Anonymous Giving Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("99ADE80F-48C7-440A-B2A1-3BB783CD339B");

            // Enable Business Giving Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("F8669093-188B-4B68-9D20-63A3FE467A82");

            // Enable Multi-Account Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("5FDF9D34-E6C8-4E29-A38F-38C8B9504247");

            // Campus Statuses Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("83FDBE95-841B-4A07-8313-A61B1834501A");

            // Campus Types Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("CCE7D1DD-6DBD-48F9-806C-AF4E48C43291");

            // Include Inactive Campuses Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("82173BA8-3A4B-4887-AD29-62ADC8475B78");

            // Ask for Campus if Known Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("8E986F12-9A62-4BC7-B38A-33147F778CA8");

            // Accounts Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("D6ABFE08-C95C-486C-BDBC-6E934EFAC3CE");

            // Financial Source Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("871BFFB3-5FE9-4448-BD63-0247FE6D23D4");

            // Batch Name Prefix Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("A32860A4-0E35-4888-836E-1989BAAA7858");

            // Enable Credit Card Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("B8800DBF-F92E-4744-841D-4DC0C3A44C41");

            // Enable ACH Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("6EC6250E-DB96-4FA7-AD9A-359974EE89A2");

            // Financial Gateway Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("8959F6D5-B89A-467C-90DA-A12784AA21E9");

            // Enabled Saved Account Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("38E3EC7A-3AFD-495B-88C7-71DDA6286A00");

            // Show Field Descriptions Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("FBC3EA68-F9BF-4370-A40F-1C61564E47D3");

            // Force Email Update Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("A99E98C6-58D1-490E-AD9D-52B4FFE7D4A9");

            // Family Term Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("5076D569-A42A-4994-8F02-796B510C11B8");

            // Allow InLine Digital Signature Documents Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("023CFEE9-F8AE-43E1-A558-44FE44B73FF5");

            // Display Progress Bar Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("8A2E2E5A-CFFD-47F8-98C3-CAFF669BFC0E");

            // Batch Name Prefix Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("62721EBE-18FB-4E49-A1D4-A59D4DE2EBF3");

            // Source Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("C3DFB7D1-9052-4127-9492-72CB0B3F3789");

            // Record Status Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("EA8FA165-64CA-4C02-8376-FBF56F13D250");

            // Connection Status Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("D9C6B155-2FD5-4F57-BC3E-C4125E2679EB");

            // Show Category Names as Separators Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("A349DB34-E5F2-4C12-AFCF-1B42FE6B55C9");

            // Block Icon Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("72904743-07BC-4139-B86B-E0BE1E7A3970");

            // Block Title Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("C6FD2CFA-E982-4BB0-8FE1-3F94B2A6AE5C");

            // Use Abbreviated Name Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("83A3312B-DEEE-494A-B1F1-3D3B34B7BC69");

            // Attribute Order Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("5D1928B1-2CB8-4246-A302-6495F2EF13C6");

            // Category Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("F4B55A50-BF81-4300-BC79-28F6729313CA");

            // Show Chart Attribute for BlockType: Metric Detail
            RockMigrationHelper.DeleteAttribute("227CF64A-CDC1-4413-98AD-1E672B5DBEDF");

            // Scheduling Response Email Attribute for BlockType: Group Schedule Toolbox
            RockMigrationHelper.DeleteAttribute("EDFB7D55-17CC-4D5C-8709-01E72987198B");

            // Scheduler Receive Confirmation Emails Attribute for BlockType: Group Schedule Toolbox
            RockMigrationHelper.DeleteAttribute("4F44D7DE-BED9-4767-8373-759F86945A64");

            // Hide Date Setting Attribute for BlockType: Group Schedule Status Board
            RockMigrationHelper.DeleteAttribute("552B15D5-A9B5-4FB7-A2B4-CFC1EECD6073");

            // Hide Group Picker Attribute for BlockType: Group Schedule Status Board
            RockMigrationHelper.DeleteAttribute("C0603277-1DEC-4316-BC0E-D5AAF721D19B");

            // Random Number Seed Attribute for BlockType: Rock Solid Church Sample Data
            RockMigrationHelper.DeleteAttribute("CE4DF44B-BC8E-4EFA-9BA0-33CC5EF0A669");

            // Note Types Attribute for BlockType: SMS Conversations
            RockMigrationHelper.DeleteAttribute("07AC139A-DF55-434F-80F4-A5131535F62E");

            // Add Page Attribute for BlockType: Media Element List
            RockMigrationHelper.DeleteAttribute("CDABD585-F195-456C-BEB4-070337B6756B");

            // Display Communication Preference Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("6165AC82-6A3D-4C16-97DB-F6E4BB849804");

            // Display Communication Preference Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("9204F4CE-1782-4783-BA3F-7D10BC4E0809");

            // Number of Columns Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("88806647-3277-42EB-ABFF-AE4C68F759F6");

            // Child Workflow Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("3A01EB96-4D42-45A6-9CE6-275D37C2751D");

            // Parent Workflow Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("343FC1E4-F455-4D8D-830E-8B5EEA07BF7B");

            // Delete BlockType Prayer Card View
            RockMigrationHelper.DeleteBlockType("1FEE129E-E46A-4805-AF5A-6F98E1DA7A16"); // Prayer Card View

            // Delete BlockType Media Element Play Analytics
            RockMigrationHelper.DeleteBlockType("D349D320-1F42-4858-9348-0DB72A479433"); // Media Element Play Analytics

            // Delete BlockType Media Element Analytics
            RockMigrationHelper.DeleteBlockType("016B007C-731A-4912-BAB1-B1E3D55D59F0"); // Media Element Analytics

            // Delete BlockType Stark Detail
            RockMigrationHelper.DeleteBlockType("0FD3EF01-6349-4E46-BA63-D99E7F6A516B"); // Stark Detail

            // Delete BlockType Login
            RockMigrationHelper.DeleteBlockType("BBF264F8-D7CA-452F-8AAD-9535E78A55AD"); // Login

            // Delete BlockType Group Member List
            RockMigrationHelper.DeleteBlockType("E496CDB6-92E2-4D26-8219-8F59DF185FE3"); // Group Member List

            // Delete BlockType Transaction Entry
            RockMigrationHelper.DeleteBlockType("115681F4-C14A-4FA7-A2C0-E6B65C936940"); // Transaction Entry

            // Delete BlockType Person Secondary
            RockMigrationHelper.DeleteBlockType("B047F943-4870-4B3A-A5EF-D4AF075E729F"); // Person Secondary

            // Delete BlockType Person Detail
            RockMigrationHelper.DeleteBlockType("30065F1F-2954-40E8-9BE4-FEDE0CB21EC4"); // Person Detail

            // Delete BlockType Large Dataset Grid
            RockMigrationHelper.DeleteBlockType("EF4D6041-3802-441C-84FE-3D5D2E5A3591"); // Large Dataset Grid

            // Delete BlockType Field Type Gallery
            RockMigrationHelper.DeleteBlockType("7FEB172D-5470-4267-A679-1F03C94247C7"); // Field Type Gallery

            // Delete BlockType Control Gallery
            RockMigrationHelper.DeleteBlockType("49664A7B-318F-43C4-836A-6937C83BDFCE"); // Control Gallery

            // Delete BlockType Registration Entry
            RockMigrationHelper.DeleteBlockType("F40C95D6-D765-40D5-AE98-A1B5B0994642"); // Registration Entry

            // Delete BlockType Attribute Values
            RockMigrationHelper.DeleteBlockType("4F5482D1-F510-4055-A000-F432BDEF8D1F"); // Attribute Values

            // Delete BlockType Widget List
            RockMigrationHelper.DeleteBlockType("7A71FE03-F362-4732-A70E-B83717076DAE"); // Widget List

            // Delete BlockType Context Group
            RockMigrationHelper.DeleteBlockType("10CAAB49-307E-4B88-B141-FFD275374ABA"); // Context Group

            // Delete BlockType Context Entities
            RockMigrationHelper.DeleteBlockType("664E3928-C02E-40D2-9EF7-A7B7DA165690"); // Context Entities

        }
    }
}

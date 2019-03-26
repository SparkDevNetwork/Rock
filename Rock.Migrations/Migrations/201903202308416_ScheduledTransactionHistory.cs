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
    public partial class ScheduledTransactionHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Converts any Scheduled Transaction Notes into Scheduled Transaction History. This removes the need to use Scheduled Transaction to store redundant history in both Notes and History.
            Sql( MigrationSQL._201903202308416_ScheduledTransactionHistory_MigrateScheduledTransactionNotesToHistory );

            UpdateModifiedPagesAndBlocksUp();
        }

        /// <summary>
        /// Updates the modified pages and blocks up.
        /// </summary>
        private void UpdateModifiedPagesAndBlocksUp()
        {
            RockMigrationHelper.UpdateBlockType( "Scheduled Transaction Edit (V2)", "Edit an existing scheduled transaction.", "~/Blocks/Finance/ScheduledTransactionEditV2.ascx", "Finance", "F1ADF375-7442-4B30-BAC3-C387EA9B6C18" );
            RockMigrationHelper.UpdateBlockType( "Transaction Entry (V2)", "Creates a new financial transaction or scheduled transaction.", "~/Blocks/Finance/TransactionEntryV2.ascx", "Finance", "6316D801-40C0-4EED-A2AD-55C13870664D" );
            // Add Block to Page: Scheduled Transaction Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "996F5541-D2E1-47E4-8078-80A388203CEC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0".AsGuid(), "Scheduled Transaction History Log", "Main", @"", @"", 1, "98ACC61C-31DF-48D7-8901-94B7B7E8100D" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '909E5FAE-F8B9-4D3D-BFDC-68DD4F9ECEF2'" );  // Page: Scheduled Transaction,  Zone: Main,  Block: Scheduled Transaction View
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '98ACC61C-31DF-48D7-8901-94B7B7E8100D'" );  // Page: Scheduled Transaction,  Zone: Main,  Block: Scheduled Transaction History Log
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'E8A2E317-38AD-4FF0-A75B-C06D3087FCC4'" );  // Page: Scheduled Transaction,  Zone: Main,  Block: Transaction List
            // Attrib for BlockType: Scheduled Transaction View:Update Page for Hosted Gateways
            RockMigrationHelper.UpdateBlockTypeAttribute( "85753750-7465-4241-97A6-E5F27EA38C8B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Update Page for Hosted Gateways", "UpdatePageHosted", "", @"The page used to update an existing scheduled transaction for Gateways that support a hosted payment interface.", 0, @"", "0C612E1C-8205-40A2-8F83-801E5816B2F2" );
            // Attrib for BlockType: Scheduled Transaction List Liquid:Scheduled Transaction Edit Page for Hosted Gateways
            RockMigrationHelper.UpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Edit Page for Hosted Gateways", "ScheduledTransactionEditPageHosted", "", @"The page used to update an existing scheduled transaction for Gateways that support a hosted payment interface.", 10, @"", "DC84D98C-F53D-4C91-A0B1-D9AAD46395C1" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Accounts
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "AccountsToDisplay", "", @"The accounts to display. By default all active accounts with a Public Name will be displayed. If the account has a child account for the selected campus, the child account for that campus will be used.", 2, @"", "BE3C72F7-32A7-4D4F-B49E-814496597B7D" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Gift Term
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Term", "GiftTerm", "", @"", 1, @"Gift", "8CCB2B8A-B268-42F3-8208-498E595B8FA9" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Enable ACH
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "", @"", 1, @"False", "BCA2ACBF-24EE-4411-9436-B49AE711B752" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Impersonation
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonation", "AllowImpersonation", "", @"Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.", 1, @"False", "2E528219-770B-4992-A8E7-F4D11CF9D943" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Enable Multi-Account
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Multi-Account", "EnableMultiAccount", "", @"Should the person be able specify amounts for more than one account?", 4, @"True", "E9DC99D6-4A9F-4B2D-8F50-27FC17B389B3" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Ask for Campus if Known
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ask for Campus if Known", "AskForCampusIfKnown", "", @"If the campus for the person is already known, should the campus still be prompted for?", 3, @"True", "26DDA427-3309-4D11-B039-F5305B71DE90" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Finish Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Finish Lava Template", "FinishLavaTemplate", "", @"The text (HTML) to display on the success page. <span class='tip tip-lava'></span>", 2, @"
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
        <dd>{{ transactionDetail.Amount }}</dd>
    {% endfor %}
    <dd></dd>
    
    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked  }}</dd>
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
", "9F8D74CB-6E0D-47ED-B522-F6A3E3289326" );
            // Attrib for BlockType: Transaction Entry (V2):Scheduled Gifts Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Scheduled Gifts Template", "ScheduledTransactionsTemplate", "", @"The Lava Template to use to display Scheduled Gifts.", 3, @"
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
                    <span class='js-toggle-scheduled-details toggle-scheduled-details clickable fa fa-plus'></span>
                </div>
            </div>

            <div class='js-scheduled-details scheduled-details margin-l-lg'>
                <div class='panel-body'>
                    {% for scheduledTransactionDetail in scheduledTransaction.ScheduledTransactionDetails %}
                        <div class='account-details'>
                            <span class='scheduled-transaction-account control-label'>
                                {{ scheduledTransactionDetail.Account.Name }}
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

            $toggle.removeClass('fa-plus').addClass('fa-minus');
        } else {
            if (animate) {
                $scheduledDetails.slideUp();
                $totalAmount.fadeIn();
            } else {
                $scheduledDetails.hide();
                $totalAmount.show();
            }

            $toggle.removeClass('fa-minus').addClass('fa-plus');
        }
    };

    Sys.Application.add_load(function () {
        var $scheduleDetailsContainers = $('.js-scheduled-transaction');

        $scheduleDetailsContainers.each(function (index) {
            setScheduledDetailsVisibility($($scheduleDetailsContainers[index]), false);
        });

        var $toggleScheduledDetails = $('.js-toggle-scheduled-details');
        $toggleScheduledDetails.click(function () {
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
", "DED73A15-3338-42E8-A0A3-0F4F28E2C2E2" );
            // Attrib for BlockType: Transaction Entry (V2):Payment Comment Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Payment Comment Template", "PaymentCommentTemplate", "", @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>", 3, @"", "3F5C6B31-36C9-4259-B531-5A2747360779" );
            // Attrib for BlockType: Transaction Entry (V2):Intro Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Intro Message", "IntroMessageTemplate", "", @"The text to place at the top of the amount entry. <span class='tip tip-lava'></span>", 2, @"<h2>Your Generosity Changes Lives</h2>", "7B0E5E2B-3488-494F-9332-7B4F0C90A69E" );
            // Attrib for BlockType: Transaction Entry (V2):Finish Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Finish Lava Template", "FinishLavaTemplate", "", @"The text (HTML) to display on the success page. <span class='tip tip-lava'></span>", 5, @"
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
        <dd>{{ transactionDetail.Amount }}</dd>
    {% endfor %}
    <dd></dd>
    
    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked  }}</dd>
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
", "44DDFBF9-F63E-46E3-84A3-A9FC72D9F146" );
            // Attrib for BlockType: Transaction Entry (V2):Invalid Account Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Account Message", "InvalidAccountInURLMessage", "", @"Display this text (HTML) as an error alert if an invalid 'account' or 'GL account' is passed through the URL. Leave blank to just ignore the invalid accounts and not show a message.", 7, @"", "3FCEC97D-FF22-4B7B-924C-3E18263A387A" );
            // Attrib for BlockType: Transaction Entry (V2):Allowed Transaction Attributes From URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Allowed Transaction Attributes From URL", "AllowedTransactionAttributesFromURL", "", @"Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world", 4, @"", "6C5F8043-4F04-4057-A874-318A8FAA27E3" );
            // Attrib for BlockType: Transaction Entry (V2):Confirm Account Email Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Confirm Account Email Template", "ConfirmAccountEmailTemplate", "", @"The Email Template to use when confirming a new account", 1, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "0C58E545-6F1F-40C6-BEA2-589E3A49233C" );
            // Attrib for BlockType: Transaction Entry (V2):Receipt Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Receipt Email", "ReceiptEmail", "", @"The system email to use to send the receipt.", 2, @"", "C8D6A72A-F675-4941-9B37-A2BD52A165D0" );
            // Attrib for BlockType: Transaction Entry (V2):Financial Gateway
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Financial Gateway", "FinancialGateway", "", @"The payment gateway to use for Credit Card and ACH transactions.", 0, @"", "1182E3BF-0946-439C-9C7D-5CC7549B8209" );
            // Attrib for BlockType: Transaction Entry (V2):Show Scheduled Gifts
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Scheduled Gifts", "ShowScheduledTransactions", "", @"If the person has any scheduled gifts, show a summary of their scheduled gifts.", 2, @"True", "08468527-8232-4C75-A4C4-9CDE0DB62F8C" );
            // Attrib for BlockType: Transaction Entry (V2):Ask for Campus if Known
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ask for Campus if Known", "AskForCampusIfKnown", "", @"If the campus for the person is already known, should the campus still be prompted for?", 10, @"True", "687BD799-2917-4E78-A80B-A3BFFC466A4B" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Multi-Account
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Multi-Account", "EnableMultiAccount", "", @"Should the person be able specify amounts for more than one account?", 11, @"True", "01A6E269-EAE2-470D-B4CF-657B9EDB2D6A" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Business Giving
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Business Giving", "EnableBusinessGiving", "", @"Should the option to give as a business be displayed.", 999, @"True", "13995CFC-C904-4252-9550-B709ED15B7CE" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Anonymous Giving
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Anonymous Giving", "EnableAnonymousGiving", "", @"Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fund-raising contributors.", 24, @"False", "B081C437-1929-400E-ABA8-E1E61A57A8B3" );
            // Attrib for BlockType: Transaction Entry (V2):Scheduled Transactions
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduled Transactions", "AllowScheduledTransactions", "", @"If the selected gateway(s) allow scheduled transactions, should that option be provided to user.", 1, @"True", "53357D45-02D5-41F0-B4D0-58E1B1D793CB" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Comment Entry
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Comment Entry", "EnableCommentEntry", "", @"Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", 1, @"False", "A53EEE25-77DC-4218-BA23-7C47EA840715" );
            // Attrib for BlockType: Transaction Entry (V2):Prompt for Phone
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Phone", "PromptForPhone", "", @"Should the user be prompted for their phone number?", 1, @"False", "B5ACD5B0-7915-4235-A8C1-0B131A62DAC0" );
            // Attrib for BlockType: Transaction Entry (V2):Prompt for Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Email", "PromptForEmail", "", @"Should the user be prompted for their email address?", 2, @"True", "95F6F1EF-9837-47A6-A266-0438F0180FBF" );
            // Attrib for BlockType: Transaction Entry (V2):Allow Account Options In URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Account Options In URL", "AllowAccountOptionsInURL", "", @"Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true", 5, @"False", "7200428B-5684-462A-879D-061651A6ADF8" );
            // Attrib for BlockType: Transaction Entry (V2):Only Public Accounts In URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Public Accounts In URL", "OnlyPublicAccountsInURL", "", @"Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.", 6, @"True", "04E0D4AE-5278-49F4-9C3A-228C17A306FE" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Initial Back button
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Initial Back button", "EnableInitialBackButton", "", @"Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry", 8, @"False", "F0873A43-A230-4AB7-9424-E0ED50753351" );
            // Attrib for BlockType: Transaction Entry (V2):Impersonation
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonation", "AllowImpersonation", "", @"Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.", 9, @"False", "7F34DEC4-CADE-4594-BEA8-D4A07B2A0967" );
            // Attrib for BlockType: Transaction Entry (V2):Enable ACH
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "", @"", 1, @"False", "F5E61C3C-B796-44CE-A2B9-90D24912F697" );
            // Attrib for BlockType: Transaction Entry (V2):Anonymous Giving Tool-tip
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Anonymous Giving Tool-tip", "AnonymousGivingTooltip", "", @"The tool-tip for the 'Give Anonymously' check box.", 25, @"", "5ADA30DF-68CD-46CD-B6C8-71A39D3F66EB" );
            // Attrib for BlockType: Transaction Entry (V2):Comment Entry Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Entry Label", "CommentEntryLabel", "", @"The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", 2, @"Comment", "39479465-F2A0-4442-8E28-51A7746EF8FA" );
            // Attrib for BlockType: Transaction Entry (V2):Save Account Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Account Title", "SaveAccountTitle", "", @"The text to display as heading of section for saving payment information.", 1, @"Make Giving Even Easier", "BA292152-DF89-4555-9B64-BE44E7310049" );
            // Attrib for BlockType: Transaction Entry (V2):Gift Term
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Term", "GiftTerm", "", @"", 3, @"Gift", "05E6822A-75B6-41DE-BC36-99BB8E5DEDFB" );
            // Attrib for BlockType: Transaction Entry (V2):Give Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text", "Give Button Text", "", @"", 4, @"Give Now", "EF60D5A5-29FA-4934-8388-CDB57C46675A" );
            // Attrib for BlockType: Transaction Entry (V2):Entity Id Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Id Parameter", "EntityIdParam", "", @"The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)", 3, @"", "EC146690-034B-4222-9B59-8575D7A2F9FA" );
            // Attrib for BlockType: Transaction Entry (V2):Batch Name Prefix
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", @"The batch prefix name to use when creating a new batch.", 2, @"Online Giving", "233306F7-D0A2-482F-B10C-7440183AB238" );
            // Attrib for BlockType: Transaction Entry (V2):Accounts
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "AccountsToDisplay", "", @"The accounts to display. By default all active accounts with a Public Name will be displayed. If the account has a child account for the selected campus, the child account for that campus will be used.", 5, @"", "1551C5EA-44B4-4B61-92A8-F91A54CB9561" );
            // Attrib for BlockType: Transaction Entry (V2):Group Location Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Group Location Type", "PersonAddressType", "", @"The location type to use for the person's address", 3, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "D873D760-F176-4285-ABFD-EFE9096FE836" );
            // Attrib for BlockType: Transaction Entry (V2):Scheduled Transaction Edit Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Edit Page", "ScheduledTransactionEditPage", "", @"The page to use for editing scheduled transactions.", 4, @"", "6ED591F8-8D55-4ED5-9D89-38495B9DF7A6" );
            // Attrib for BlockType: Transaction Entry (V2):Financial Source Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Financial Source Type", "FinancialSourceType", "", @"The Financial Source Type to use when creating transactions", 19, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "F301ABD6-6C44-45E9-ACDA-F2F093D6477E" );
            // Attrib for BlockType: Transaction Entry (V2):Connection Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "PersonConnectionStatus", "", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 4, @"368DD475-242C-49C4-A42C-7278BE690CC2", "69C978D6-8CC5-47AD-B89F-9F97F55A9C7E" );
            // Attrib for BlockType: Transaction Entry (V2):Record Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "PersonRecordStatus", "", @"The record status to use for new individuals (default: 'Pending'.)", 5, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "FE54460B-1FFB-4DB8-AB91-E97A4EFF673C" );
            // Attrib for BlockType: Transaction Entry (V2):Transaction Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "Transaction Type", "", @"", 1, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "67CA1B64-7F8A-412C-BF6A-076D3CC2A3BC" );
            // Attrib for BlockType: Transaction Entry (V2):Transaction Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Transaction Entity Type", "TransactionEntityType", "", @"The Entity Type for the Transaction Detail Record (usually left blank)", 2, @"", "8930809B-E111-4C4F-A6C9-0B54D3A0C9C5" );
            // Attrib Value for Block:Scheduled Transaction History Log, Attribute:Entity Type Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "98ACC61C-31DF-48D7-8901-94B7B7E8100D", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"76824e8a-ccc4-4085-84d9-8af8c0807e20" );
            // Attrib Value for Block:Scheduled Transaction History Log, Attribute:Heading Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "98ACC61C-31DF-48D7-8901-94B7B7E8100D", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"History" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateModifiedPagesAndBlocksDown();
        }

        /// <summary>
        /// Updates the modified pages and blocks down.
        /// </summary>
        private void UpdateModifiedPagesAndBlocksDown()
        {
            // Attrib for BlockType: Scheduled Transaction View:Update Page for Hosted Gateways
            RockMigrationHelper.DeleteAttribute( "0C612E1C-8205-40A2-8F83-801E5816B2F2" );
            // Attrib for BlockType: Transaction Entry (V2):Batch Name Prefix
            RockMigrationHelper.DeleteAttribute( "233306F7-D0A2-482F-B10C-7440183AB238" );
            // Attrib for BlockType: Transaction Entry (V2):Enable ACH
            RockMigrationHelper.DeleteAttribute( "F5E61C3C-B796-44CE-A2B9-90D24912F697" );
            // Attrib for BlockType: Transaction Entry (V2):Financial Gateway
            RockMigrationHelper.DeleteAttribute( "1182E3BF-0946-439C-9C7D-5CC7549B8209" );
            // Attrib for BlockType: Transaction Entry (V2):Impersonation
            RockMigrationHelper.DeleteAttribute( "7F34DEC4-CADE-4594-BEA8-D4A07B2A0967" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Initial Back button
            RockMigrationHelper.DeleteAttribute( "F0873A43-A230-4AB7-9424-E0ED50753351" );
            // Attrib for BlockType: Transaction Entry (V2):Invalid Account Message
            RockMigrationHelper.DeleteAttribute( "3FCEC97D-FF22-4B7B-924C-3E18263A387A" );
            // Attrib for BlockType: Transaction Entry (V2):Only Public Accounts In URL
            RockMigrationHelper.DeleteAttribute( "04E0D4AE-5278-49F4-9C3A-228C17A306FE" );
            // Attrib for BlockType: Transaction Entry (V2):Allow Account Options In URL
            RockMigrationHelper.DeleteAttribute( "7200428B-5684-462A-879D-061651A6ADF8" );
            // Attrib for BlockType: Transaction Entry (V2):Allowed Transaction Attributes From URL
            RockMigrationHelper.DeleteAttribute( "6C5F8043-4F04-4057-A874-318A8FAA27E3" );
            // Attrib for BlockType: Transaction Entry (V2):Entity Id Parameter
            RockMigrationHelper.DeleteAttribute( "EC146690-034B-4222-9B59-8575D7A2F9FA" );
            // Attrib for BlockType: Transaction Entry (V2):Transaction Entity Type
            RockMigrationHelper.DeleteAttribute( "8930809B-E111-4C4F-A6C9-0B54D3A0C9C5" );
            // Attrib for BlockType: Transaction Entry (V2):Transaction Type
            RockMigrationHelper.DeleteAttribute( "67CA1B64-7F8A-412C-BF6A-076D3CC2A3BC" );
            // Attrib for BlockType: Transaction Entry (V2):Record Status
            RockMigrationHelper.DeleteAttribute( "FE54460B-1FFB-4DB8-AB91-E97A4EFF673C" );
            // Attrib for BlockType: Transaction Entry (V2):Connection Status
            RockMigrationHelper.DeleteAttribute( "69C978D6-8CC5-47AD-B89F-9F97F55A9C7E" );
            // Attrib for BlockType: Transaction Entry (V2):Group Location Type
            RockMigrationHelper.DeleteAttribute( "D873D760-F176-4285-ABFD-EFE9096FE836" );
            // Attrib for BlockType: Transaction Entry (V2):Prompt for Email
            RockMigrationHelper.DeleteAttribute( "95F6F1EF-9837-47A6-A266-0438F0180FBF" );
            // Attrib for BlockType: Transaction Entry (V2):Prompt for Phone
            RockMigrationHelper.DeleteAttribute( "B5ACD5B0-7915-4235-A8C1-0B131A62DAC0" );
            // Attrib for BlockType: Transaction Entry (V2):Receipt Email
            RockMigrationHelper.DeleteAttribute( "C8D6A72A-F675-4941-9B37-A2BD52A165D0" );
            // Attrib for BlockType: Transaction Entry (V2):Confirm Account Email Template
            RockMigrationHelper.DeleteAttribute( "0C58E545-6F1F-40C6-BEA2-589E3A49233C" );
            // Attrib for BlockType: Transaction Entry (V2):Finish Lava Template
            RockMigrationHelper.DeleteAttribute( "44DDFBF9-F63E-46E3-84A3-A9FC72D9F146" );
            // Attrib for BlockType: Transaction Entry (V2):Give Button Text
            RockMigrationHelper.DeleteAttribute( "EF60D5A5-29FA-4934-8388-CDB57C46675A" );
            // Attrib for BlockType: Transaction Entry (V2):Gift Term
            RockMigrationHelper.DeleteAttribute( "05E6822A-75B6-41DE-BC36-99BB8E5DEDFB" );
            // Attrib for BlockType: Transaction Entry (V2):Intro Message
            RockMigrationHelper.DeleteAttribute( "7B0E5E2B-3488-494F-9332-7B4F0C90A69E" );
            // Attrib for BlockType: Transaction Entry (V2):Save Account Title
            RockMigrationHelper.DeleteAttribute( "BA292152-DF89-4555-9B64-BE44E7310049" );
            // Attrib for BlockType: Transaction Entry (V2):Payment Comment Template
            RockMigrationHelper.DeleteAttribute( "3F5C6B31-36C9-4259-B531-5A2747360779" );
            // Attrib for BlockType: Transaction Entry (V2):Comment Entry Label
            RockMigrationHelper.DeleteAttribute( "39479465-F2A0-4442-8E28-51A7746EF8FA" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Comment Entry
            RockMigrationHelper.DeleteAttribute( "A53EEE25-77DC-4218-BA23-7C47EA840715" );
            // Attrib for BlockType: Transaction Entry (V2):Scheduled Transaction Edit Page
            RockMigrationHelper.DeleteAttribute( "6ED591F8-8D55-4ED5-9D89-38495B9DF7A6" );
            // Attrib for BlockType: Transaction Entry (V2):Scheduled Gifts Template
            RockMigrationHelper.DeleteAttribute( "DED73A15-3338-42E8-A0A3-0F4F28E2C2E2" );
            // Attrib for BlockType: Transaction Entry (V2):Scheduled Transactions
            RockMigrationHelper.DeleteAttribute( "53357D45-02D5-41F0-B4D0-58E1B1D793CB" );
            // Attrib for BlockType: Transaction Entry (V2):Anonymous Giving Tool-tip
            RockMigrationHelper.DeleteAttribute( "5ADA30DF-68CD-46CD-B6C8-71A39D3F66EB" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Anonymous Giving
            RockMigrationHelper.DeleteAttribute( "B081C437-1929-400E-ABA8-E1E61A57A8B3" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Business Giving
            RockMigrationHelper.DeleteAttribute( "13995CFC-C904-4252-9550-B709ED15B7CE" );
            // Attrib for BlockType: Transaction Entry (V2):Financial Source Type
            RockMigrationHelper.DeleteAttribute( "F301ABD6-6C44-45E9-ACDA-F2F093D6477E" );
            // Attrib for BlockType: Transaction Entry (V2):Enable Multi-Account
            RockMigrationHelper.DeleteAttribute( "01A6E269-EAE2-470D-B4CF-657B9EDB2D6A" );
            // Attrib for BlockType: Transaction Entry (V2):Ask for Campus if Known
            RockMigrationHelper.DeleteAttribute( "687BD799-2917-4E78-A80B-A3BFFC466A4B" );
            // Attrib for BlockType: Transaction Entry (V2):Accounts
            RockMigrationHelper.DeleteAttribute( "1551C5EA-44B4-4B61-92A8-F91A54CB9561" );
            // Attrib for BlockType: Transaction Entry (V2):Show Scheduled Gifts
            RockMigrationHelper.DeleteAttribute( "08468527-8232-4C75-A4C4-9CDE0DB62F8C" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Ask for Campus if Known
            RockMigrationHelper.DeleteAttribute( "26DDA427-3309-4D11-B039-F5305B71DE90" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Finish Lava Template
            RockMigrationHelper.DeleteAttribute( "9F8D74CB-6E0D-47ED-B522-F6A3E3289326" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Enable Multi-Account
            RockMigrationHelper.DeleteAttribute( "E9DC99D6-4A9F-4B2D-8F50-27FC17B389B3" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Accounts
            RockMigrationHelper.DeleteAttribute( "BE3C72F7-32A7-4D4F-B49E-814496597B7D" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Impersonation
            RockMigrationHelper.DeleteAttribute( "2E528219-770B-4992-A8E7-F4D11CF9D943" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Enable ACH
            RockMigrationHelper.DeleteAttribute( "BCA2ACBF-24EE-4411-9436-B49AE711B752" );
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Gift Term
            RockMigrationHelper.DeleteAttribute( "8CCB2B8A-B268-42F3-8208-498E595B8FA9" );
            // Attrib for BlockType: Scheduled Transaction List Liquid:Scheduled Transaction Edit Page for Hosted Gateways
            RockMigrationHelper.DeleteAttribute( "DC84D98C-F53D-4C91-A0B1-D9AAD46395C1" );
            // Remove Block: Scheduled Transaction History Log, from Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "98ACC61C-31DF-48D7-8901-94B7B7E8100D" );
            RockMigrationHelper.DeleteBlockType( "6316D801-40C0-4EED-A2AD-55C13870664D" ); // Transaction Entry (V2)
            RockMigrationHelper.DeleteBlockType( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18" ); // Scheduled Transaction Edit (V2)
        }
    }
}

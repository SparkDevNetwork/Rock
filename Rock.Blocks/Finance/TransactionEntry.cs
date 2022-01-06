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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Transactions;
using Rock.ViewModel;
using Rock.ViewModel.Controls;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Allows the user to try out various controls.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Transaction Entry" )]
    [Category( "Obsidian > Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]
    [IconCssClass( "fa fa-credit-card" )]

    #region Block Attributes

    [FinancialGatewayField(
        "Financial Gateway",
        Key = AttributeKey.FinancialGateway,
        Description = "The payment gateway to use for Credit Card and ACH transactions.",
        Category = AttributeCategory.None,
        Order = 0 )]

    [BooleanField(
        "Enable ACH",
        Key = AttributeKey.EnableACH,
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 1 )]

    [BooleanField(
        "Enable Credit Card",
        Key = AttributeKey.EnableCreditCard,
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 2 )]

    [TextField(
        "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch.",
        DefaultValue = "Online Giving",
        Category = AttributeCategory.None,
        Order = 3 )]

    [DefinedValueField(
        "Source",
        Key = AttributeKey.FinancialSourceType,
        Description = "The Financial Source Type to use when creating transactions.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Category = AttributeCategory.None,
        Order = 4 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.",
        Category = AttributeCategory.None,
        Order = 5 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 10 )]

    [BooleanField(
        "Include Inactive Campuses",
        Key = AttributeKey.IncludeInactiveCampuses,
        Description = "Set this to true to include inactive campuses",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 10 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.IncludedCampusTypes,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Description = "Set this to limit campuses by campus type.",
        Category = AttributeCategory.None,
        Order = 11 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.IncludedCampusStatuses,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Description = "Set this to limit campuses by campus status.",
        Category = AttributeCategory.None,
        Order = 12 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 13 )]

    [DefinedValueField(
        "Financial Source Type",
        Key = AttributeKey.FinancialSourceType,
        Description = "The Financial Source Type to use when creating transactions",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Category = AttributeCategory.None,
        Order = 19 )]

    [BooleanField(
        "Enable Business Giving",
        Key = AttributeKey.EnableBusinessGiving,
        Description = "Should the option to give as a business be displayed.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 999 )]

    [BooleanField(
        "Enable Anonymous Giving",
        Key = AttributeKey.EnableAnonymousGiving,
        Description = "Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fund-raising contributors.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 24 )]

    [TextField(
        "Anonymous Giving Tool-tip",
        Key = AttributeKey.AnonymousGivingTooltip,
        IsRequired = false,
        Description = "The tool-tip for the 'Give Anonymously' check box.",
        Category = AttributeCategory.None,
        Order = 25 )]

    #region Scheduled Transactions

    [BooleanField(
        "Scheduled Transactions",
        Key = AttributeKey.AllowScheduledTransactions,
        Description = "If the selected gateway(s) allow scheduled transactions, should that option be provided to user.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Category = AttributeCategory.ScheduleGifts,
        Order = 1 )]

    [BooleanField(
        "Show Scheduled Gifts",
        Key = AttributeKey.ShowScheduledTransactions,
        Description = "If the person has any scheduled gifts, show a summary of their scheduled gifts.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.ScheduleGifts,
        Order = 2 )]

    [CodeEditorField(
        "Scheduled Gifts Template",
        Key = AttributeKey.ScheduledTransactionsTemplate,
        Description = "The Lava Template to use to display Scheduled Gifts.",
        DefaultValue = DefaultScheduledTransactionsTemplate,
        EditorMode = CodeEditorMode.Lava,
        Category = AttributeCategory.ScheduleGifts,
        Order = 3 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page",
        Key = AttributeKey.ScheduledTransactionEditPage,
        Description = "The page to use for editing scheduled transactions.",
        Category = AttributeCategory.ScheduleGifts,
        Order = 4 )]

    #endregion

    #region Payment Comment Options

    [BooleanField(
        "Enable Comment Entry",
        Key = AttributeKey.EnableCommentEntry,
        Description = "Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment' setting)",
        IsRequired = false,
        Category = AttributeCategory.PaymentComments,
        Order = 1 )]

    [TextField(
        "Comment Entry Label",
        Key = AttributeKey.CommentEntryLabel,
        Description = "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).",
        DefaultValue = "Comment",
        IsRequired = false,
        Category = AttributeCategory.PaymentComments,
        Order = 2 )]

    [CodeEditorField(
        "Payment Comment Template",
        Key = AttributeKey.PaymentCommentTemplate,
        Description = @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>",
        IsRequired = false,
        EditorMode = CodeEditorMode.Lava,
        Category = AttributeCategory.PaymentComments,
        Order = 3 )]

    #endregion Payment Comment Options

    #region Text Options

    [TextField( "Save Account Title",
        Key = AttributeKey.SaveAccountTitle,
        Description = "The text to display as heading of section for saving payment information.",
        IsRequired = false,
        DefaultValue = "Make Giving Even Easier",
        Category = AttributeCategory.TextOptions,
        Order = 1 )]

    [CodeEditorField(
        "Intro Message",
        Key = AttributeKey.IntroMessageTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text to place at the top of the amount entry. <span class='tip tip-lava'></span>",
        DefaultValue = "<h2>Your Generosity Changes Lives</h2>",
        Category = AttributeCategory.TextOptions,
        Order = 2 )]

    [TextField(
        "Gift Term",
        Key = AttributeKey.GiftTerm,
        DefaultValue = "Gift",
        Category = AttributeCategory.TextOptions,
        Order = 3 )]

    [TextField(
        "Give Button Text - Now ",
        Key = AttributeKey.GiveButtonNowText,
        DefaultValue = "Give Now",
        Category = AttributeCategory.TextOptions,
        Order = 4 )]

    [TextField(
        "Give Button Text - Scheduled",
        Key = AttributeKey.GiveButtonScheduledText,
        DefaultValue = "Schedule Your Gift",
        Category = AttributeCategory.TextOptions,
        Order = 5 )]

    [CodeEditorField(
        "Amount Summary Template",
        Key = AttributeKey.AmountSummaryTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the amount summary page. <span class='tip tip-lava'></span>",
        DefaultValue = DefaultAmountSummaryTemplate,
        Category = AttributeCategory.TextOptions,
        Order = 6 )]

    [CodeEditorField(
        "Finish Lava Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the success page. <span class='tip tip-lava'></span>",
        DefaultValue = DefaultFinishLavaTemplate,
        Category = AttributeCategory.TextOptions,
        Order = 7 )]

    #endregion

    #region Email Templates

    [SystemCommunicationField(
        "Confirm Account Email Template",
        Key = AttributeKey.ConfirmAccountEmailTemplate,
        Description = "The Email Template to use when confirming a new account",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Category = AttributeCategory.EmailTemplates,
        Order = 1 )]

    [SystemCommunicationField(
        "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system email to use to send the receipt.",
        IsRequired = false,
        Category = AttributeCategory.EmailTemplates,
        Order = 2 )]

    #endregion Email Templates

    #region Person Options

    [BooleanField(
        "Prompt for Phone",
        Key = AttributeKey.PromptForPhone,
        Category = AttributeCategory.PersonOptions,
        Description = "Should the user be prompted for their phone number?",
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField(
        "Prompt for Email",
        Key = AttributeKey.PromptForEmail,
        Category = AttributeCategory.PersonOptions,
        Description = "Should the user be prompted for their email address?",
        DefaultBooleanValue = true,
        Order = 2 )]

    [GroupLocationTypeField(
        "Address Type",
        Key = AttributeKey.PersonAddressType,
        Category = AttributeCategory.PersonOptions,
        Description = "The location type to use for the person's address",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        IsRequired = false,
        Order = 3 )]

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKey.PersonConnectionStatus,
        Category = AttributeCategory.PersonOptions,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Web Prospect'.)",
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        IsRequired = true,
        Order = 4 )]

    [DefinedValueField(
        "Record Status",
        Key = AttributeKey.PersonRecordStatus,
        Category = AttributeCategory.PersonOptions,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 5 )]

    #endregion Person Options

    #region Advanced options

    [DefinedValueField(
        "Transaction Type",
        Key = AttributeKey.TransactionType,
        Description = "",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        Category = AttributeCategory.Advanced,
        Order = 1 )]

    [EntityTypeField(
        "Transaction Entity Type",
        Key = AttributeKey.TransactionEntityType,
        Description = "The Entity Type for the Transaction Detail Record (usually left blank)",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 2 )]

    [TextField( "Entity Id Parameter",
        Key = AttributeKey.EntityIdParam,
        Description = "The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 3 )]

    [AttributeField( "Allowed Transaction Attributes From URL",
        Key = AttributeKey.AllowedTransactionAttributesFromURL,
        EntityTypeGuid = Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION,
        Description = "Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world",
        IsRequired = false,
        AllowMultiple = true,
        Category = AttributeCategory.Advanced,
        Order = 4 )]

    [BooleanField(
        "Allow Account Options In URL",
        Key = AttributeKey.AllowAccountOptionsInURL,
        Description = "Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 5 )]

    [BooleanField(
        "Only Public Accounts In URL",
        Key = AttributeKey.OnlyPublicAccountsInURL,
        Description = "Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Advanced,
        Order = 6 )]

    [CodeEditorField(
        "Invalid Account Message",
        Key = AttributeKey.InvalidAccountInURLMessage,
        Description = "Display this text (HTML) as an error alert if an invalid 'account' or 'GL account' is passed through the URL. Leave blank to just ignore the invalid accounts and not show a message.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = AttributeCategory.Advanced,
        Order = 7 )]

    [BooleanField( "Enable Initial Back button",
        Key = AttributeKey.EnableInitialBackButton,
        Description = "Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 8 )]

    [BooleanField(
        "Impersonation",
        Key = AttributeKey.AllowImpersonation,
        Description = "Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 9 )]

    #endregion Advanced Options

    #endregion Block Attributes

    public class TransactionEntry : RockObsidianBlockType
    {
        #region constants

        private const string DefaultAmountSummaryTemplate = @"
{% assign sortedAccounts = Accounts | Sort:'Order,PublicName' %}

<span class='account-names'>{{ sortedAccounts | Map:'PublicName' | Join:', ' | ReplaceLast:',',' and' }}</span>
-
<span class='account-campus'>{{ Campus.Name }}</span>";

        private const string DefaultFinishLavaTemplate = @"
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
";

        private const string DefaultScheduledTransactionsTemplate = @"
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
";

        #endregion

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string AccountsToDisplay = "AccountsToDisplay";

            public const string AllowImpersonation = "AllowImpersonation";

            public const string AllowScheduledTransactions = "AllowScheduledTransactions";

            public const string BatchNamePrefix = "BatchNamePrefix";

            public const string FinancialGateway = "FinancialGateway";

            public const string EnableACH = "EnableACH";

            public const string EnableCreditCard = "EnableCreditCard";

            public const string EnableCommentEntry = "EnableCommentEntry";

            public const string CommentEntryLabel = "CommentEntryLabel";

            public const string EnableBusinessGiving = "EnableBusinessGiving";

            public const string EnableAnonymousGiving = "EnableAnonymousGiving";

            public const string AnonymousGivingTooltip = "AnonymousGivingTooltip";

            public const string PaymentCommentTemplate = "PaymentCommentTemplate";

            public const string EnableInitialBackButton = "EnableInitialBackButton";

            public const string FinancialSourceType = "FinancialSourceType";

            public const string ShowScheduledTransactions = "ShowScheduledTransactions";

            public const string ScheduledTransactionsTemplate = "ScheduledTransactionsTemplate";

            public const string ScheduledTransactionEditPage = "ScheduledTransactionEditPage";

            public const string GiftTerm = "GiftTerm";

            public const string GiveButtonNowText = "GiveButtonNowText";

            public const string GiveButtonScheduledText = "GiveButtonScheduledText";

            public const string AmountSummaryTemplate = "AmountSummaryTemplate";

            public const string AskForCampusIfKnown = "AskForCampusIfKnown";

            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";

            public const string IncludedCampusTypes = "IncludedCampusTypes";

            public const string IncludedCampusStatuses = "IncludedCampusStatuses";

            public const string EnableMultiAccount = "EnableMultiAccount";

            public const string IntroMessageTemplate = "IntroMessageTemplate";

            public const string FinishLavaTemplate = "FinishLavaTemplate";

            public const string SaveAccountTitle = "SaveAccountTitle";

            public const string ConfirmAccountEmailTemplate = "ConfirmAccountEmailTemplate";

            public const string TransactionType = "Transaction Type";

            public const string TransactionEntityType = "TransactionEntityType";

            public const string EntityIdParam = "EntityIdParam";

            public const string AllowedTransactionAttributesFromURL = "AllowedTransactionAttributesFromURL";

            public const string AllowAccountOptionsInURL = "AllowAccountOptionsInURL";

            public const string OnlyPublicAccountsInURL = "OnlyPublicAccountsInURL";

            public const string InvalidAccountInURLMessage = "InvalidAccountInURLMessage";

            public const string ReceiptEmail = "ReceiptEmail";

            public const string PromptForPhone = "PromptForPhone";

            public const string PromptForEmail = "PromptForEmail";

            public const string PersonAddressType = "PersonAddressType";

            public const string PersonConnectionStatus = "PersonConnectionStatus";

            public const string PersonRecordStatus = "PersonRecordStatus";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        private static class AttributeCategory
        {
            public const string None = "";
            public const string ScheduleGifts = "Scheduled Gifts";
            public const string PaymentComments = "Payment Comments";
            public const string TextOptions = "Text Options";
            public const string Advanced = "Advanced";
            public const string EmailTemplates = "Email Templates";
            public const string PersonOptions = "Person Options";
        }

        #endregion Attribute Categories

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string Person = "Person";

            public const string AttributeKeyPrefix = "Attribute_";

            public const string AccountIdsOptions = "AccountIds";

            public const string AccountGlCodesOptions = "AccountGlCodes";

            public const string AmountLimit = "AmountLimit";

            /// <summary>
            /// The frequency options in the form of &Frequency=DefinedValueId^UserEditable
            /// </summary>
            public const string FrequencyOptions = "Frequency";

            public const string StartDate = "StartDate";
        }

        #endregion

        #region enums

        /// <summary>
        ///
        /// </summary>
        private enum EntryStep
        {
            /// <summary>
            /// prompt for amounts (step 1)
            /// </summary>
            PromptForAmounts,

            /// <summary>
            /// Get payment information (step 2)
            /// </summary>
            GetPaymentInfo,

            /// <summary>
            /// Get/Update personal information (step 3)
            /// </summary>
            GetPersonalInformation,

            /// <summary>
            /// The show transaction summary (step 4)
            /// </summary>
            ShowTransactionSummary
        }

        #endregion enums

        #region Gateway

        /// <summary>
        /// Gets the financial gateway (model) that is configured for this block
        /// </summary>
        /// <returns></returns>
        private FinancialGateway FinancialGateway
        {
            get
            {
                if ( _financialGateway == null )
                {
                    var rockContext = new RockContext();
                    var financialGatewayGuid = GetAttributeValue( AttributeKey.FinancialGateway ).AsGuid();
                    _financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayGuid );
                }

                return _financialGateway;
            }
        }
        private FinancialGateway _financialGateway = null;

        /// <summary>
        /// Gets the financial gateway component that is configured for this block
        /// </summary>
        /// <returns></returns>
        private IObsidianHostedGatewayComponent FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null )
                {
                    var financialGateway = FinancialGateway;
                    if ( financialGateway != null )
                    {
                        _financialGatewayComponent = financialGateway.GetGatewayComponent() as IObsidianHostedGatewayComponent;
                    }
                }

                return _financialGatewayComponent;
            }
        }
        private IObsidianHostedGatewayComponent _financialGatewayComponent = null;

        #endregion Gateway

        #region Obsidian Overrides

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            var rockContext = new RockContext();

            var clientHelper = new Rock.ViewModel.Client.ClientHelper( rockContext, RequestContext.CurrentPerson );

            var controlOptions = new HostedPaymentInfoControlOptions
            {
                EnableACH = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean(),
                EnableCreditCard = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean()
            };

            return new
            {
                FinancialAccounts = GetAccountViewModels(),
                GatewayControl = new GatewayControlViewModel
                {
                    FileUrl = FinancialGatewayComponent?.GetObsidianControlFileUrl( FinancialGateway ),
                    Settings = FinancialGatewayComponent?.GetObsidianControlSettings( FinancialGateway, controlOptions )
                },
                Campuses = clientHelper.GetCampusesAsListItems(),
                Frequencies = clientHelper.GetDefinedValuesAsListItems( SystemGuid.DefinedType.FINANCIAL_FREQUENCY.AsGuid() )
            };
        }

        #endregion Obsidian Overrides

        #region Attribute Helpers

        /// <summary>
        /// Gets the account view models.
        /// </summary>
        /// <returns></returns>
        private List<FinancialAccountViewModel> GetAccountViewModels() {
            using ( var rockContext = new RockContext() )
            {
                var service = new FinancialAccountService( rockContext );
                var allowedGuids = GetAttributeValue( AttributeKey.AccountsToDisplay ).SplitDelimitedValues().AsGuidList();
                var accounts = service.GetByGuids( allowedGuids ).ToList();

                return accounts
                    .Select( a => a.ToViewModel( null, false ) )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the transaction entity.
        /// </summary>
        /// <returns></returns>
        private IEntity GetTransactionEntity( int? transactionEntityId )
        {
            IEntity transactionEntity = null;
            Guid? transactionEntityTypeGuid = GetAttributeValue( AttributeKey.TransactionEntityType ).AsGuidOrNull();
            if ( transactionEntityTypeGuid.HasValue )
            {
                var transactionEntityType = EntityTypeCache.Get( transactionEntityTypeGuid.Value );
                if ( transactionEntityType != null )
                {
                    if ( transactionEntityId.HasValue )
                    {
                        transactionEntity = Reflection.GetIEntityForEntityType( transactionEntityType.GetEntityType(), transactionEntityId.Value );
                    }
                }
            }

            return transactionEntity;
        }

        #endregion Attribute Helpers

        #region Transaction Helpers

        /// <summary>
        /// Populates the transaction details for a FinancialTransaction or ScheduledFinancialTransaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transactionDetails">The transaction details.</param>
        private void PopulateTransactionDetails<T>( ICollection<T> transactionDetails, ProcessTransactionArgs args )
            where T : ITransactionDetail, new()
        {
            var transactionEntity = GetTransactionEntity( args.TransactionEntityId );

            using ( var rockContext = new RockContext() )
            {
                var service = new FinancialAccountService( rockContext );
                var allowedGuids = GetAttributeValue( AttributeKey.AccountsToDisplay ).SplitDelimitedValues().AsGuidList();
                var accountGuidToIdMap = service.GetByGuids( allowedGuids ).ToDictionary( fa => fa.Guid, fa => fa.Id );

                foreach ( var arg in args.AccountAmounts.Where( kvp => kvp.Value > 0 ) )
                {
                    var accountId = accountGuidToIdMap.GetValueOrNull( arg.Key );

                    if ( !accountId.HasValue )
                    {
                        break;
                    }

                    var transactionDetail = new T
                    {
                        Amount = arg.Value,
                        AccountId = accountId.Value,
                        EntityTypeId = transactionEntity?.TypeId,
                        EntityId = transactionEntity?.Id
                    };

                    transactionDetails.Add( transactionDetail );
                }
            }
        }

        /// <summary>
        /// Creates the target person from the information collected (Name, Phone, Email, Address), or returns a matching person if they already exist.
        /// NOTE: Use <seealso cref="CreateBusiness" /> to creating a Business(Person) record
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private Person CreateTargetPerson( ProcessTransactionArgs args )
        {
            var firstName = args.FirstName;
            var lastName = args.LastName;
            var email = args.Email;

            if ( firstName.IsNotNullOrWhiteSpace() && lastName.IsNotNullOrWhiteSpace() && email.IsNotNullOrWhiteSpace() )
            {
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, args.PhoneNumber );
                var matchingPerson = new PersonService( new RockContext() ).FindPerson( personQuery, true );

                if ( matchingPerson != null )
                {
                    return matchingPerson;
                }
            }

            return CreatePersonOrBusiness( args );
        }

        /// <summary>
        /// Creates the business contact person.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private Person CreateBusinessContactPerson( ProcessTransactionArgs args )
        {
            var firstName = args.FirstName;
            var lastName = args.LastName;
            var email = args.Email;

            if ( firstName.IsNotNullOrWhiteSpace() && lastName.IsNotNullOrWhiteSpace() && email.IsNotNullOrWhiteSpace() )
            {
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, args.PhoneNumber );
                var matchingPerson = new PersonService( new RockContext() ).FindPerson( personQuery, true );

                if ( matchingPerson != null )
                {
                    return matchingPerson;
                }
            }

            return CreatePersonOrBusiness( args );
        }

        /// <summary>
        /// Creates a business (or returns an existing business if the person already has a business with the same business name)
        /// </summary>
        /// <param name="contactPerson">The contact person.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private Person CreateBusiness( Person contactPerson, ProcessTransactionArgs args )
        {
            var businessName = args.BusinessName;

            // Try to find existing business for person that has the same name
            var personBusinesses = contactPerson.GetBusinesses()
                .Where( b => b.LastName == businessName )
                .ToList();

            if ( personBusinesses.Count() == 1 )
            {
                return personBusinesses.First();
            }

            var email = args.Email;
            var business = CreatePersonOrBusiness( args );

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            personService.AddContactToBusiness( business.Id, contactPerson.Id );
            rockContext.SaveChanges();

            return business;
        }

        /// <summary>
        /// Creates the person or business.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private Person CreatePersonOrBusiness( ProcessTransactionArgs args )
        {
            var firstName = args.FirstName;
            var lastName = args.LastName;
            var email = args.Email;
            var createBusiness = !args.IsGivingAsPerson;

            var rockContext = new RockContext();
            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.PersonConnectionStatus ).AsGuid() );
            DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.PersonRecordStatus ).AsGuid() );

            // Create Person
            var newPersonOrBusiness = new Person();
            newPersonOrBusiness.FirstName = firstName;
            newPersonOrBusiness.LastName = lastName;

            newPersonOrBusiness.Email = email;
            newPersonOrBusiness.IsEmailActive = true;
            newPersonOrBusiness.EmailPreference = EmailPreference.EmailAllowed;
            if ( createBusiness )
            {
                newPersonOrBusiness.RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
            }
            else
            {
                newPersonOrBusiness.RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
            }

            if ( dvcConnectionStatus != null )
            {
                newPersonOrBusiness.ConnectionStatusValueId = dvcConnectionStatus.Id;
            }

            if ( dvcRecordStatus != null )
            {
                newPersonOrBusiness.RecordStatusValueId = dvcRecordStatus.Id;
            }

            var campus = args.CampusGuid.HasValue ? CampusCache.Get( args.CampusGuid.Value ) : null;
            var campusId = campus?.Id;

            // Create Person and Family, and set their primary campus to the one they gave money to
            PersonService.SaveNewPerson( newPersonOrBusiness, rockContext, campusId, false );

            // SaveNewPerson should have already done this, but just in case
            rockContext.SaveChanges();

            return newPersonOrBusiness;
        }

        /// <summary>
        /// Person Input Source
        /// </summary>
        private enum PersonInputSource
        {
            Person,
            Business,
            BusinessContact
        }

        /// <summary>
        /// Updates the person/business from the information collected (Phone, Email, Address) and saves changes (if any) to the database.
        /// </summary>
        /// <param name="personOrBusiness">The person or business.</param>
        /// <param name="personInputSource">The person input source.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="Exception">Unexpected PersonInputSource</exception>
        private void UpdatePersonOrBusinessFromInputInformation( Person personOrBusiness, PersonInputSource personInputSource, ProcessTransactionArgs args )
        {
            var promptForEmail = GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean();
            var promptForPhone = GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();
            int numberTypeId;
            Guid locationTypeGuid;

            switch ( personInputSource )
            {
                case PersonInputSource.Business:
                    numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                    locationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
                    break;
                case PersonInputSource.BusinessContact:
                    numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
                    locationTypeGuid = GetAttributeValue( AttributeKey.PersonAddressType ).AsGuid();
                    break;
                case PersonInputSource.Person:
                    numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
                    locationTypeGuid = GetAttributeValue( AttributeKey.PersonAddressType ).AsGuid();
                    break;
                default:
                    throw new Exception( "Unexpected PersonInputSource" );
            }

            if ( promptForPhone && args.PhoneNumber.IsNotNullOrWhiteSpace() )
            {
                var phone = personOrBusiness.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberTypeId );

                if ( phone == null )
                {
                    phone = new PhoneNumber();
                    personOrBusiness.PhoneNumbers.Add( phone );
                    phone.NumberTypeValueId = numberTypeId;
                }

                phone.CountryCode = PhoneNumber.CleanNumber( args.PhoneCountryCode );
                phone.Number = PhoneNumber.CleanNumber( args.PhoneNumber );
            }

            var primaryFamily = personOrBusiness.GetFamily();

            if ( primaryFamily != null )
            {
                var rockContext = new RockContext();

                // fetch primaryFamily using rockContext so that any changes will get saved
                primaryFamily = new GroupService( rockContext ).Get( primaryFamily.Id );

                GroupService.AddNewGroupAddress(
                    rockContext,
                    primaryFamily,
                    locationTypeGuid.ToString(),
                    args.Street1,
                    args.Street2,
                    args.City,
                    args.State,
                    args.PostalCode,
                    args.Country,
                    true );
            }
        }

        /// <summary>
        /// Creates a business (or returns an existing business if the person already has a business with the same business name)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contactPerson">The contact person.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private Person CreateBusiness( RockContext rockContext, Person contactPerson, ProcessTransactionArgs args )
        {
            var businessName = args.BusinessName;

            // Try to find existing business for person that has the same name
            var personBusinesses = contactPerson.GetBusinesses()
                .Where( b => b.LastName == businessName )
                .ToList();

            if ( personBusinesses.Count() == 1 )
            {
                return personBusinesses.First();
            }

            var email = args.Email;
            var business = CreatePersonOrBusiness( args );

            var personService = new PersonService( rockContext );
            personService.AddContactToBusiness( business.Id, contactPerson.Id );
            rockContext.SaveChanges();

            return business;
        }

        /// <summary>
        /// Determines whether a scheduled giving frequency was selected
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is scheduled transaction]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsScheduledTransaction( ProcessTransactionArgs args )
        {
            var oneTimeFrequencyGuid = Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid();

            if ( args.FrequencyValueGuid != oneTimeFrequencyGuid )
            {
                return true;
            }
            else
            {
                return args.GiftDate > RockDateTime.Now.Date;
            }
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="transactionGuid">The transaction unique identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="args">The arguments.</param>
        private void SaveTransaction( Guid transactionGuid, int personId, PaymentInfo paymentInfo, FinancialTransaction transaction, ProcessTransactionArgs args )
        {
            var rockContext = new RockContext();

            // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
            transaction.Guid = transactionGuid;
            transaction.AuthorizedPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId );
            transaction.ShowAsAnonymous = args.IsGiveAnonymously;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = FinancialGateway.Id;

            var txnType = DefinedValueCache.Get( this.GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = txnType.Id;
            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, FinancialGatewayComponent as GatewayComponent, rockContext );

            var sourceGuid = GetAttributeValue( AttributeKey.FinancialSourceType ).AsGuidOrNull();

            if ( sourceGuid.HasValue )
            {
                transaction.SourceTypeValueId = DefinedValueCache.GetId( sourceGuid.Value );
            }

            PopulateTransactionDetails( transaction.TransactionDetails, args );

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                GetAttributeValue( AttributeKey.BatchNamePrefix ),
                paymentInfo.CurrencyTypeValue,
                paymentInfo.CreditCardTypeValue,
                transaction.TransactionDateTime.Value,
                FinancialGateway.GetBatchTimeOffset() );

            var batchChanges = new History.HistoryChangeList();

            if ( batch.Id == 0 )
            {
                batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
            }

            transaction.LoadAttributes( rockContext );

            var allowedTransactionAttributes = GetAttributeValue( AttributeKey.AllowedTransactionAttributesFromURL ).Split( ',' ).AsGuidList().Select( x => AttributeCache.Get( x ).Key );

            /* TODO
            foreach ( KeyValuePair<string, AttributeValueCache> attr in transaction.AttributeValues )
            {
                if ( PageParameters().ContainsKey( PageParameterKey.AttributeKeyPrefix + attr.Key ) && allowedTransactionAttributes.Contains( attr.Key ) )
                {
                    attr.Value.Value = Server.UrlDecode( PageParameter( PageParameterKey.AttributeKeyPrefix + attr.Key ) );
                }
            }
            */

            var financialTransactionService = new FinancialTransactionService( rockContext );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add( transaction );
            rockContext.SaveChanges();
            transaction.SaveAttributeValues();

            batchService.IncrementControlAmount( batch.Id, transaction.TotalAmount, batchChanges );
            rockContext.SaveChanges();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges );

            SendReceipt( transaction.Id );
        }

        /// <summary>
        /// Saves the scheduled transaction.
        /// </summary>
        /// <param name="transactionGuid">The transaction unique identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="args">The arguments.</param>
        private void SaveScheduledTransaction(
            Guid transactionGuid,
            int personId,
            PaymentInfo paymentInfo,
            PaymentSchedule schedule,
            FinancialScheduledTransaction scheduledTransaction,
            ProcessTransactionArgs args )
        {
            var rockContext = new RockContext();

            // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
            scheduledTransaction.Guid = transactionGuid;

            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.AuthorizedPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId ).Value;
            scheduledTransaction.FinancialGatewayId = FinancialGateway.Id;

            scheduledTransaction.Summary = paymentInfo.Comment1;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, FinancialGatewayComponent as GatewayComponent, rockContext );

            Guid? sourceGuid = GetAttributeValue( AttributeKey.FinancialSourceType ).AsGuidOrNull();
            if ( sourceGuid.HasValue )
            {
                scheduledTransaction.SourceTypeValueId = DefinedValueCache.GetId( sourceGuid.Value );
            }

            PopulateTransactionDetails( scheduledTransaction.ScheduledTransactionDetails, args );

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            financialScheduledTransactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void SendReceipt( int transactionId )
        {
            var receiptEmail = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();

            if ( receiptEmail.HasValue )
            {
                // Queue a transaction to send receipts
                var transactionIds = new List<int> { transactionId };
                var sendPaymentReceiptsTxn = new SendPaymentReceipts( receiptEmail.Value, transactionIds );
                RockQueue.TransactionQueue.Enqueue( sendPaymentReceiptsTxn );
            }
        }

        #endregion Transaction Helpers

        #region Block Actions

        /// <summary>
        /// Processes the transaction.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ProcessTransaction( Guid transactionGuid, ProcessTransactionArgs args )
        {
            using ( var rockContext = new RockContext() )
            {
                // to make duplicate transactions impossible, make sure that our Transaction hasn't already been processed as a regular or scheduled transaction
                var transactionAlreadyExists =
                    (
                        new FinancialTransactionService( rockContext )
                        .Queryable()
                        .Any( a => a.Guid == transactionGuid )
                    ) ||
                    (
                        new FinancialScheduledTransactionService( rockContext )
                        .Queryable()
                        .Any( a => a.Guid == transactionGuid )
                    );

                if ( transactionAlreadyExists )
                {
                    return ActionBadRequest( "A transaction with the same unique identifier has already been created. This might occur when a duplicate charge was requested." );
                }
            }

            // Validation
            if ( args.AccountAmounts == null )
            {
                return ActionBadRequest( "At least one account must be selected to designate the funds." );
            }

            if ( args.AccountAmounts.Any( kvp => kvp.Value < 0 ) )
            {
                return ActionBadRequest( "Amounts designated cannot be less than $0." );
            }

            var totalAmount = args.AccountAmounts.Sum( kvp => kvp.Value );

            if ( totalAmount < 0 )
            {
                return ActionBadRequest( "The total amount must be greater than $0" );
            }

            // Convert financial saved account Guid to id
            FinancialPersonSavedAccount savedAccount = null;
            var currentPerson = GetCurrentPerson();

            if ( args.FinancialPersonSavedAccountGuid.HasValue && currentPerson != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new FinancialPersonSavedAccountService( rockContext );
                    savedAccount = service
                        .GetByPersonId( currentPerson.Id )
                        .FirstOrDefault( sa => sa.Guid == args.FinancialPersonSavedAccountGuid.Value );
                }

                if ( savedAccount == null )
                {
                    return ActionBadRequest( "The saved account unique identifier is not valid" );
                }
            }

            // Build transaction details
            var transactionDetails = new List<FinancialTransactionDetail>();
            PopulateTransactionDetails( transactionDetails, args );

            if ( transactionDetails.Sum( td => td.Amount ) != totalAmount )
            {
                return ActionBadRequest( "One of the designated account unique identifiers is not valid" );
            }

            // Build the payment info
            var paymentInfo = new ReferencePaymentInfo
            {
                Amount = totalAmount,
                Email = args.Email,
                Phone = PhoneNumber.FormattedNumber( args.PhoneCountryCode, args.PhoneNumber, true ),
                Street1 = args.Street1,
                Street2 = args.Street2,
                City = args.City,
                State = args.State,
                PostalCode = args.PostalCode,
                Country = args.Country,
                FirstName = args.FirstName,
                LastName = args.LastName,
                BusinessName = args.BusinessName,
                FinancialPersonSavedAccountId = savedAccount?.Id
            };

            // get the payment comment
            var transactionDateTime = RockDateTime.Now;
            var mergeFields = LavaHelper.GetCommonMergeFields( null, currentPerson );
            mergeFields.Add( "TransactionDateTime", transactionDateTime );
            mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );
            mergeFields.Add( "TransactionAccountDetails", transactionDetails );
            var paymentComment = GetAttributeValue( AttributeKey.PaymentCommentTemplate ).ResolveMergeFields( mergeFields );

            // Add comments if enabled
            if ( GetAttributeValue( AttributeKey.EnableCommentEntry ).AsBoolean() )
            {
                if ( paymentComment.IsNotNullOrWhiteSpace() )
                {
                    paymentInfo.Comment1 = string.Format( "{0}: {1}", paymentComment, args.Comment );
                }
                else
                {
                    paymentInfo.Comment1 = args.Comment;
                }
            }
            else
            {
                paymentInfo.Comment1 = paymentComment;
            }

            // use the paymentToken as the reference number for creating the customer account
            if ( savedAccount?.ReferenceNumber.IsNullOrWhiteSpace() == false )
            {
                paymentInfo.GatewayPersonIdentifier = savedAccount.ReferenceNumber;
            }
            else
            {
                paymentInfo.ReferenceNumber = args.ReferenceNumber;
                paymentInfo.GatewayPersonIdentifier = FinancialGatewayComponent.CreateCustomerAccount( FinancialGateway, paymentInfo, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() || paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage ?? "Unknown Error" );
                }
            }

            // determine or create the Person record that this transaction is for
            var targetPerson = currentPerson;
            int transactionPersonId;

            if ( targetPerson == null )
            {
                if ( !args.IsGivingAsPerson )
                {
                    targetPerson = CreateBusinessContactPerson( args );
                }
                else
                {
                    targetPerson = CreateTargetPerson( args );
                }
            }

            UpdatePersonOrBusinessFromInputInformation(
                targetPerson,
                args.IsGivingAsPerson ? PersonInputSource.Person : PersonInputSource.BusinessContact,
                args );

            using ( var rockContext = new RockContext() )
            {
                if ( !args.IsGivingAsPerson )
                {
                    var business = new PersonService( rockContext ).Get( args.BusinessGuid.Value );

                    if ( business == null )
                    {
                        business = CreateBusiness( rockContext, targetPerson, args );
                    }

                    UpdatePersonOrBusinessFromInputInformation( business, PersonInputSource.Business, args );
                    transactionPersonId = business.Id;
                }
                else
                {
                    transactionPersonId = targetPerson.Id;
                }

                // just in case anything about the person/business was updated (email or phone), save changes
                rockContext.SaveChanges();
            }

            if ( IsScheduledTransaction( args ) )
            {
                string gatewayScheduleId = null;
                try
                {
                    PaymentSchedule paymentSchedule = new PaymentSchedule
                    {
                        TransactionFrequencyValue = DefinedValueCache.Get( args.FrequencyValueGuid ),
                        StartDate = args.GiftDate,
                        PersonId = transactionPersonId
                    };

                    var financialScheduledTransaction = ( FinancialGatewayComponent as GatewayComponent ).AddScheduledPayment(
                        FinancialGateway,
                        paymentSchedule,
                        paymentInfo,
                        out var errorMessage );

                    if ( financialScheduledTransaction == null )
                    {
                        return ActionBadRequest( errorMessage ?? "Unknown Error" );
                    }

                    gatewayScheduleId = financialScheduledTransaction.GatewayScheduleId;
                    SaveScheduledTransaction( transactionGuid, transactionPersonId, paymentInfo, paymentSchedule, financialScheduledTransaction, args );
                }
                catch ( Exception ex )
                {
                    if ( gatewayScheduleId.IsNotNullOrWhiteSpace() )
                    {
                        // if we didn't get the gatewayScheduleId from AddScheduledPayment, see if the gateway paymentInfo.TransactionCode before the exception occurred
                        gatewayScheduleId = paymentInfo.TransactionCode;
                    }

                    // if an exception occurred, it is possible that an orphaned subscription might be on the Gateway server. Some gateway components will clean-up when there is exception, but log it just in case it needs to be resolved by a human
                    throw new Exception( string.Format( "Error occurred when saving financial scheduled transaction for gateway scheduled payment with a gatewayScheduleId of {0} and FinancialScheduledTransaction with Guid of {1}.", gatewayScheduleId, transactionGuid ), ex );
                }
            }
            else
            {
                string transactionCode = null;

                try
                {
                    var financialTransaction = ( FinancialGatewayComponent as GatewayComponent ).Charge( FinancialGateway, paymentInfo, out var errorMessage );

                    if ( financialTransaction == null )
                    {
                        return ActionBadRequest( errorMessage ?? "Unknown Error" );
                    }

                    transactionCode = financialTransaction.TransactionCode;
                    SaveTransaction( transactionGuid, transactionPersonId, paymentInfo, financialTransaction, args );
                }
                catch ( Exception ex )
                {
                    throw new Exception( string.Format( "Error occurred when saving financial transaction for gateway payment with a transactionCode of {0} and FinancialTransaction with Guid of {1}.", transactionCode, transactionGuid ), ex );
                }
            }

            return new BlockActionResult( System.Net.HttpStatusCode.Created );
        }

        #endregion Block Actions

        #region ViewModels

        /// <summary>
        /// Process Transaction Args
        /// </summary>
        public sealed class ProcessTransactionArgs
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is giving as person.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is giving as person; otherwise, <c>false</c>.
            /// </value>
            public bool IsGivingAsPerson { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the phone number.
            /// </summary>
            /// <value>
            /// The phone number.
            /// </value>
            public string PhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets the phone country code.
            /// </summary>
            /// <value>
            /// The phone country code.
            /// </value>
            public string PhoneCountryCode { get; set; }

            /// <summary>
            /// Gets or sets the account amount arguments.
            /// </summary>
            /// <value>
            /// The account amount arguments.
            /// </value>
            public Dictionary<Guid, decimal> AccountAmounts { get; set; }

            /// <summary>
            /// Gets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
            public string Street1 { get; set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            public string Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public string City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            public string PostalCode { get; set; }

            /// <summary>
            /// Gets or sets the country.
            /// </summary>
            /// <value>
            /// The country.
            /// </value>
            public string Country { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the name of the business.
            /// </summary>
            /// <value>
            /// The name of the business.
            /// </value>
            public string BusinessName { get; set; }

            /// <summary>
            /// Gets or sets the financial person saved account unique identifier.
            /// </summary>
            /// <value>
            /// The financial person saved account unique identifier.
            /// </value>
            public Guid? FinancialPersonSavedAccountGuid { get; set; }

            /// <summary>
            /// Gets or sets the comment.
            /// </summary>
            /// <value>
            /// The comment.
            /// </value>
            public string Comment { get; set; }

            /// <summary>
            /// Gets or sets the transaction entity identifier.
            /// </summary>
            /// <value>
            /// The transaction entity identifier.
            /// </value>
            public int? TransactionEntityId { get; set; }

            /// <summary>
            /// Gets or sets the reference number.
            /// </summary>
            /// <value>
            /// The reference number.
            /// </value>
            public string ReferenceNumber { get; set; }

            /// <summary>
            /// Gets or sets the campus unique identifier.
            /// </summary>
            /// <value>
            /// The campus unique identifier.
            /// </value>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets the business unique identifier.
            /// </summary>
            /// <value>
            /// The business unique identifier.
            /// </value>
            public Guid? BusinessGuid { get; set; }

            /// <summary>
            /// Gets or sets the frequency value unique identifier.
            /// </summary>
            /// <value>
            /// The frequency value unique identifier.
            /// </value>
            public Guid FrequencyValueGuid { get; set; }

            /// <summary>
            /// Gets or sets the gift date.
            /// </summary>
            /// <value>
            /// The gift date.
            /// </value>
            public DateTime GiftDate { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is give anonymously.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is give anonymously; otherwise, <c>false</c>.
            /// </value>
            public bool IsGiveAnonymously { get; set; }

            /// <summary>
            /// Gets or sets the campuses available for the user to select.
            /// </summary>
            /// <value>
            /// The campuses available for the user to select.
            /// </value>
            public List<ListItemViewModel> Campuses { get; set; }

            /// <summary>
            /// Gets or sets the available giving frequencies.
            /// </summary>
            /// <value>
            /// The available giving frequencies.
            /// </value>
            public List<ListItemViewModel> Frequencies { get; set; }
        }

        #endregion ViewModels
    }
}

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

using System.ComponentModel;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Obsidian.Util;
using Rock.Web.UI.Controls;

namespace Rock.Obsidian.Blocks.Finance
{
    /// <summary>
    /// Allows the user to try out various controls.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

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
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT,
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

    public class TransactionEntry : ObsidianBlockType
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
        private IHasObsidianControl FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null )
                {
                    var financialGateway = FinancialGateway;
                    if ( financialGateway != null )
                    {
                        _financialGatewayComponent = financialGateway.GetGatewayComponent() as IHasObsidianControl;
                    }
                }

                return _financialGatewayComponent;
            }
        }
        private IHasObsidianControl _financialGatewayComponent = null;

        #endregion Gateway

        #region Obsidian Overrides

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetBlockSettings()
        {
            if ( FinancialGateway == null || FinancialGatewayComponent == null )
            {
                return null;
            }

            return new
            {
                GatewayControlFileUrl = FinancialGatewayComponent.GetObsidianControlFileUrl( FinancialGateway ),
                GatewayControlSettings = FinancialGatewayComponent.GetObsidianControlSettings( FinancialGateway )
            };
        }

        #endregion Obsidian Overrides
    }
}

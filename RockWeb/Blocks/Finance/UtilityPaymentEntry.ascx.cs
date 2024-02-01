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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// </summary>
    [DisplayName( "Utility Payment Entry" )]
    [Category( "Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]

    #region Block Attributes

    #region Default Category

    [FinancialGatewayField(
        "Financial Gateway",
        Key = AttributeKey.FinancialGateway,
        Description = "The payment gateway to use for Credit Card and ACH transactions.",
        Order = 0 )]

    [BooleanField(
        "Enable ACH",
        Key = AttributeKey.EnableACH,
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField(
        "Enable Credit Card",
        Key = AttributeKey.EnableCreditCard,
        DefaultBooleanValue = true,
        Order = 2 )]

    [TextField( "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch.",
        IsRequired = false,
        DefaultValue = "Online Giving",
        Order = 3 )]

    [DefinedValueField( "Source",
        Key = AttributeKey.Source,
        Description = "The Financial Source Type to use when creating transactions.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Order = 4 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Order = 5 )]

    [BooleanField(
        "Include Inactive Campuses",
        Key = AttributeKey.IncludeInactiveCampuses,
        Description = "Set this to true to include inactive campuses",
        DefaultBooleanValue = false,
        Order = 6 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.IncludedCampusTypes,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Description = "Set this to limit campuses by campus type.",
        Order = 7 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.IncludedCampusStatuses,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Description = "Set this to limit campuses by campus status.",
        Order = 8 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Order = 9 )]

    [BooleanField( "Impersonation",
        Key = AttributeKey.Impersonation,
        Description = "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Order = 10 )]

    [CustomDropdownListField( "Layout Style",
        Key = AttributeKey.LayoutStyle,
        Description = "How the sections of this page should be displayed.",
        ListSource = "Vertical,Fluid",
        IsRequired = false,
        DefaultValue = "Vertical",
        Order = 11 )]

    [CodeEditorField( "Account Header Template",
        Key = AttributeKey.AccountHeaderTemplate,
        Description = "The Lava Template to use as the amount input label for each account.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 50,
        IsRequired = true,
        DefaultValue = "{{ Account.PublicName }}",
        Order = 12 )]

    [AccountsField( "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. If Account Campus mapping logic is enabled and the account has a child account for the selected campus, the child account for that campus will be used.",
        IsRequired = false,
        Order = 13 )]

    [BooleanField( "Additional Accounts",
        Key = AttributeKey.AdditionalAccounts,
        Description = "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available.",
        TrueText = "Display option for selecting additional accounts",
        FalseText = "Don't display option",
        DefaultBooleanValue = true,
        Order = 14 )]

    [BooleanField( "Enable Account Hierarchy for Additional Accounts",
        Key = AttributeKey.EnableAccountHierarchy,
        Description = "When \"Additional Accounts\" is enabled, this setting allows for the grouping of accounts under their respective parents, creating an account hierarchy. However, please note that if the \"Use Account Campus Mapping Logic\" setting is enabled, accounts mapped to campuses WILL BE displayed within the Account Hierarchy.",
        TrueText = "Enable",
        FalseText = "Disable",
        DefaultBooleanValue = false,
        Order = 15 )]

    [BooleanField(
        "Use Account Campus Mapping Logic",
        Description = @"If enabled, the accounts will be determined as follows:
        <ul>
          <li>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</li>
          <li>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</li>
          <li>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</li>
        <ul>",
        Key = AttributeKey.UseAccountCampusMappingLogic,
        DefaultBooleanValue = false,
        Order = 16 )]

    [BooleanField( "Scheduled Transactions",
        Key = AttributeKey.AllowScheduled,
        Description = "If the selected gateway(s) allow scheduled transactions, should that option be provided to user. This feature is not compatible when Text-to-Give mode is enabled.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Order = 17 )]

    [BooleanField( "Prompt for Phone",
        Key = AttributeKey.DisplayPhone,
        Description = "Should the user be prompted for their phone number?",
        DefaultBooleanValue = false,
        Order = 18 )]

    [BooleanField( "SMS Opt-in",
        Key = AttributeKey.DisplaySmsOptIn,
        Description = "If 'Prompt for Phone' is set to 'Yes' then selecting 'Show' here will allow a user to opt-into receiving SMS communications for that number.",
        TrueText = "Show",
        FalseText = "Hide",
        DefaultBooleanValue = false,
        Order = 19 )]

    [BooleanField( "Prompt for Email",
        Key = AttributeKey.DisplayEmail,
        Description = "Should the user be prompted for their email address?",
        DefaultBooleanValue = true,
        Order = 20 )]

    [GroupLocationTypeField( "Address Type",
        Key = AttributeKey.AddressType,
        Description = "The location type to use for the person's address.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Order = 21 )]

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status to use for new individuals (default: 'Prospect'.)",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 22 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 23 )]

    [BooleanField( "Enable Comment Entry",
        Key = AttributeKey.EnableCommentEntry,
        Description = "Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment Template' setting)",
        DefaultBooleanValue = false,
        Order = 24 )]

    [TextField( "Comment Entry Label",
        Key = AttributeKey.CommentEntryLabel,
        Description = "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).",
        IsRequired = false,
        DefaultValue = "Comment",
        Order = 25 )]

    [BooleanField( "Enable Business Giving",
        Key = AttributeKey.EnableBusinessGiving,
        Description = "Should the option to give as a business be displayed?",
        DefaultBooleanValue = true,
        Order = 26 )]

    [BooleanField( "Enable Anonymous Giving",
        Key = AttributeKey.EnableAnonymousGiving,
        Description = "Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fundraising contributors.",
        DefaultBooleanValue = false,
        Order = 27 )]

    [BooleanField(
        "Disable Captcha Support",
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        Key = AttributeKey.DisableCaptchaSupport,
        DefaultBooleanValue = false,
        Order = 28 )]

    #endregion Default Category

    #region Email Templates

    [SystemCommunicationField( "Confirm Account",
        Key = AttributeKey.ConfirmAccountTemplate,
        Description = "Confirm Account Email Template",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Category = CategoryKey.EmailTemplates,
        Order = 1 )]

    [SystemCommunicationField( "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system email to use to send the receipt.",
        IsRequired = false,
        Category = CategoryKey.EmailTemplates,
        Order = 2 )]

    #endregion Email Templates

    #region Text Options

    [TextField( "Panel Title",
        Key = AttributeKey.PanelTitle,
        Description = "The text to display in panel heading",
        IsRequired = false,
        DefaultValue = "Gifts",
        Category = CategoryKey.TextOptions,
        Order = 1 )]

    [TextField( "Contribution Info Title",
        Key = AttributeKey.ContributionInfoTitle,
        Description = "The text to display as heading of section for selecting account and amount.",
        IsRequired = false,
        DefaultValue = "Contribution Information",
        Category = CategoryKey.TextOptions,
        Order = 2 )]

    [TextField( "Personal Info Title",
        Key = AttributeKey.PersonalInfoTitle,
        Description = "The text to display as heading of section for entering personal information.",
        IsRequired = false,
        DefaultValue = "Personal Information",
        Category = CategoryKey.TextOptions,
        Order = 4 )]

    [TextField( "Payment Info Title",
        Key = AttributeKey.PaymentInfoTitle,
        Description = "The text to display as heading of section for entering credit card or bank account information.",
        IsRequired = false,
        DefaultValue = "Payment Information",
        Category = CategoryKey.TextOptions,
        Order = 5 )]

    [BooleanField( "Show Confirmation Page",
        Key = AttributeKey.ShowConfirmationPage,
        Description = "Show a confirmation page before processing the transaction.",
        DefaultBooleanValue = true,
        Category = CategoryKey.TextOptions,
        Order = 7 )]

    [TextField( "Confirmation Title",
        Key = AttributeKey.ConfirmationTitle,
        Description = "The text to display as heading of section for confirming information entered.",
        IsRequired = false,
        DefaultValue = "Confirm Information",
        Category = CategoryKey.TextOptions,
        Order = 8 )]

    [CodeEditorField( "Confirmation Header",
        Key = AttributeKey.ConfirmationHeader,
        Description = "The text (HTML) to display at the top of the confirmation section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = AttributeString.ConfirmationHeader,
        Category = CategoryKey.TextOptions,
        Order = 9 )]

    [CodeEditorField( "Confirmation Footer",
        Key = AttributeKey.ConfirmationFooter,
        Description = "The text (HTML) to display at the bottom of the confirmation section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = AttributeString.ConfirmationFooter,
        Category = CategoryKey.TextOptions,
        Order = 10 )]

    [CodeEditorField(
        "Finish Lava Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the success page. <span class='tip tip-lava'></span>",
        DefaultValue = AttributeString.DefaultFinishLavaTemplate,
        Category = CategoryKey.TextOptions,
        Order = 11 )]

    [CodeEditorField( "Success Footer",
        Key = AttributeKey.SuccessFooter,
        Description = "The text (HTML) to display at the bottom of the success section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = @"",
        Category = CategoryKey.TextOptions,
        Order = 12 )]

    [TextField( "Save Account Title",
        Key = AttributeKey.SaveAccountTitle,
        Description = "The text to display as heading of section for saving payment information.",
        IsRequired = false,
        DefaultValue = "Make Giving Even Easier",
        Category = CategoryKey.TextOptions,
        Order = 13 )]

    [TextField( "Add Account Text",
        Key = AttributeKey.AddAccountText,
        Description = "The button text to display for adding an additional account",
        IsRequired = false,
        DefaultValue = "Add Another Account",
        Category = CategoryKey.TextOptions,
        Order = 14 )]

    [CodeEditorField( "Payment Comment Template",
        Key = AttributeKey.PaymentCommentTemplate,
        Description = AttributeString.PaymentCommentDescription,
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        Category = CategoryKey.TextOptions,
        Order = 15 )]

    [TextField( "Anonymous Giving Tooltip",
        Key = AttributeKey.AnonymousGivingTooltip,
        Description = "The tooltip for the 'Give Anonymously' checkbox.",
        IsRequired = false,
        DefaultValue = "",
        Category = CategoryKey.TextOptions,
        Order = 16 )]

    #endregion Text Options

    #region Advanced

    [BooleanField(
        "Allow Account Options In URL",
        Key = AttributeKey.AllowAccountOptionsInURL,
        Description = "Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true",
        DefaultBooleanValue = false,
        Category = CategoryKey.Advanced,
        Order = 1 )]

    [BooleanField( "Only Public Accounts In URL",
        Key = AttributeKey.OnlyPublicAccountsInURL,
        Description = "Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.",
        DefaultBooleanValue = true,
        Category = CategoryKey.Advanced,
        Order = 2 )]

    [CodeEditorField( "Invalid Account Message",
        Key = AttributeKey.InvalidAccountMessage,
        Description = "Display this text (HTML) as an error alert if an invalid 'account' or 'glaccount' is passed through the URL.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = "The configured financial accounts are not valid for accepting financial transactions.",
        Category = CategoryKey.Advanced,
        Order = 3 )]

    [CustomDropdownListField( "Account Campus Context",
        Key = AttributeKey.AccountCampusContext,
        Description = "Should any context be applied to the Account List",
        ListSource = "-1^No Account Campus Context Filter Applied,0^Only Accounts with Current Campus Context,1^Accounts with No Campus and Current Campus Context",
        IsRequired = false,
        DefaultValue = "-1",
        Category = CategoryKey.Advanced,
        Order = 4 )]

    [AttributeField( "Allowed Transaction Attributes From URL",
        Key = AttributeKey.AllowedTransactionAttributesFromURL,
        Description = "Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world",
        EntityTypeGuid = Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION,
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = "",
        Category = CategoryKey.Advanced,
        Order = 5 )]

    [DefinedValueField( "Transaction Type",
        Key = AttributeKey.TransactionType,
        Description = "",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        Category = CategoryKey.Advanced,
        Order = 6 )]

    [EntityTypeField( "Transaction Entity Type",
        Key = AttributeKey.TransactionEntityType,
        Description = "The Entity Type for the Transaction Detail Record (usually left blank)",
        IsRequired = false,
        Category = CategoryKey.Advanced,
        Order = 7 )]

    [TextField( "Entity Id Param",
        Key = AttributeKey.EntityIdParam,
        Description = "The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)",
        IsRequired = false,
        DefaultValue = "",
        Category = CategoryKey.Advanced,
        Order = 8 )]

    [CodeEditorField( "Transaction Header",
        Key = AttributeKey.TransactionHeader,
        Description = "The Lava template which will be displayed prior to the Amount entry",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = CategoryKey.Advanced,
        Order = 9 )]

    [BooleanField( "Enable Initial Back button",
        Key = AttributeKey.EnableInitialBackbutton,
        Description = "Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry",
        DefaultBooleanValue = false,
        Category = CategoryKey.Advanced,
        Order = 10 )]

    [BooleanField( "Show Panel Headings",
        Key = AttributeKey.ShowPanelHeadings,
        Description = "Show the text headings at the top of the block and in panel sections.",
        DefaultBooleanValue = true,
        Category = CategoryKey.Advanced,
        Order = 11 )]

    [BooleanField( "Enable Text-To-Give Mode",
        Key = AttributeKey.EnableTextToGiveSetup,
        Description = "This setting enables specific behavior for setting up Text-To-Give accounts.",
        DefaultBooleanValue = false,
        Category = CategoryKey.Advanced,
        Order = 12 )]

    #endregion Advanced

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004" )]
    public partial class UtilityPaymentEntry : Rock.Web.UI.RockBlock
    {
        #region Block Keys

        private static class CategoryKey
        {
            public const string EmailTemplates = "Email Templates";
            public const string TextOptions = "Text Options";
            public const string Advanced = "Advanced";
        }

        private static class AttributeKey
        {
            // Default Category
            public const string FinancialGateway = "FinancialGateway";
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string Source = "Source";
            public const string Impersonation = "Impersonation";
            public const string LayoutStyle = "LayoutStyle";
            public const string AccountHeaderTemplate = "AccountHeaderTemplate";
            public const string AllowScheduled = "AllowScheduled";
            public const string DisplayPhone = "DisplayPhone";
            public const string DisplaySmsOptIn = "SmsOptIn";
            public const string DisplayEmail = "DisplayEmail";
            public const string AddressType = "AddressType";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string EnableCommentEntry = "EnableCommentEntry";
            public const string CommentEntryLabel = "CommentEntryLabel";
            public const string EnableBusinessGiving = "EnableBusinessGiving";
            public const string EnableAnonymousGiving = "EnableAnonymousGiving";
            public const string AdditionalAccounts = "AdditionalAccounts";
            public const string EnableAccountHierarchy = "EnableAccountHierarchy";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";

            // Email Templates Category
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";
            public const string ReceiptEmail = "ReceiptEmail";

            // Text Options Category
            public const string PanelTitle = "PanelTitle";
            public const string ContributionInfoTitle = "ContributionInfoTitle";
            public const string PersonalInfoTitle = "PersonalInfoTitle";
            public const string PaymentInfoTitle = "PaymentInfoTitle";
            public const string ConfirmationTitle = "ConfirmationTitle";
            public const string SaveAccountTitle = "SaveAccountTitle";
            public const string ConfirmationHeader = "ConfirmationHeader";
            public const string ConfirmationFooter = "ConfirmationFooter";
            public const string FinishLavaTemplate = "FinishLavaTemplate";
            public const string SuccessFooter = "SuccessFooter";
            public const string PaymentCommentTemplate = "PaymentCommentTemplate";
            public const string AnonymousGivingTooltip = "AnonymousGivingTooltip";
            public const string AddAccountText = "AddAccountText";

            // Advanced Category
            public const string AllowAccountOptionsInURL = "AllowAccountOptionsInURL";
            public const string OnlyPublicAccountsInURL = "OnlyPublicAccountsInURL";
            public const string InvalidAccountMessage = "InvalidAccountMessage";
            public const string AccountCampusContext = "AccountCampusContext";
            public const string AllowedTransactionAttributesFromURL = "AllowedTransactionAttributesFromURL";
            public const string TransactionType = "TransactionType";
            public const string TransactionEntityType = "TransactionEntityType";
            public const string EntityIdParam = "EntityIdParam";
            public const string TransactionHeader = "TransactionHeader";
            public const string EnableInitialBackbutton = "EnableInitialBackbutton";
            public const string ShowConfirmationPage = "ShowConfirmationPage";
            public const string AccountsToDisplay = "AccountsToDisplay";
            public const string UseAccountCampusMappingLogic = "UseAccountCampusMappingLogic";
            public const string AskForCampusIfKnown = "AskForCampusIfKnown";
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string IncludedCampusTypes = "IncludedCampusTypes";
            public const string IncludedCampusStatuses = "IncludedCampusStatuses";
            public const string EnableMultiAccount = "EnableMultiAccount";
            public const string ShowPanelHeadings = "ShowPanelHeadings";
            public const string EnableTextToGiveSetup = "EnableTextToGiveSetup";
        }

        private static class AttributeString
        {
            public const string DefaultFinishLavaTemplate = @"
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
        <dd>{{ transactionDetail.Amount | Minus: transactionDetail.FeeCoverageAmount | FormatAsCurrency }}</dd>
    {% endfor %}
    {% if Transaction.TotalFeeCoverageAmount %}
        <dt>Fee Coverage</dt>
        <dd>{{ Transaction.TotalFeeCoverageAmount | FormatAsCurrency }}</dd>
    {% endif %}
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
            public const string ConfirmationHeader = @"
<p>
    Please confirm the information below. Once you have confirmed that the information is
    accurate click the 'Finish' button to complete your transaction.
</p>
";
            public const string ConfirmationFooter = @"
<div class='alert alert-info'>
    By clicking the 'finish' button below I agree to allow {{ OrganizationName }}
    to transfer the amount above from my account. I acknowledge that I may
    update the transaction information at any time by returning to this website. Please
    call the Finance Office if you have any additional questions.
</div>
";
            public const string SuccessHeader = @"
<p>
    Thank you for your generous contribution.  Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively
    achieve our mission.  We are so grateful for your commitment.
</p>
";

            public const string PaymentCommentDescription = @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>";
        }

        private static class PageParameterKey
        {
            public const string AccountIdsOptions = "AccountIds";
            public const string AccountGlCodesOptions = "AccountGlCodes";
            public const string AmountLimit = "AmountLimit";
            public const string AttributePrefix = "Attribute_";
            public const string Frequency = "Frequency";
            public const string PersonActionIdentifier = "rckid";
            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
            public const string StartDate = "StartDate";
            public const string Transfer = "Transfer";
            public const string ParticipationMode = "ParticipationMode";
            public const string CampusId = "CampusId";
        }

        private static class ViewStateKey
        {
            public const string GroupLocationId = "GroupLocationId";
            public const string TransactionCode = "TransactionCode";
            public const string CreditCardTypeValueId = "CreditCardTypeValueId";
            public const string ScheduleId = "ScheduleId";
            public const string DisplayPhone = "DisplayPhone";
            public const string DisplaySmsOptIn = "DisplaySmsOptIn";
            public const string PersonId = "PersonId";
            public const string HostPaymentInfoSubmitScript = "HostPaymentInfoSubmitScript";
            public const string AvailableAccountsJSON = "AvailableAccountsJSON";
            public const string SelectedAccountsJSON = "SelectedAccountsJSON";
            public const string CaptchaFailCount = "CaptchaFailCount";
        }

        #endregion Block Keys

        #region Fields

        private Person _targetPerson = null;

        protected bool FluidLayout
        {
            get
            {
                return GetAttributeValue( AttributeKey.LayoutStyle ) == "Fluid";
            }
        }

        /// <summary>
        /// The scheduled transaction to be transferred.  This will get set if the
        /// page parameter "transfer" and the "ScheduledTransactionId" are passed in.
        /// </summary>
        private FinancialScheduledTransaction _scheduledTransactionToBeTransferred = null;

        

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the group location identifier.
        /// </summary>
        /// <value>
        /// The group location identifier.
        /// </value>
        protected int? GroupLocationId
        {
            get { return ViewState[ViewStateKey.GroupLocationId] as int?; }
            set { ViewState[ViewStateKey.GroupLocationId] = value; }
        }

        /// <summary>
        /// Gets or sets the payment transaction code.
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState[ViewStateKey.TransactionCode] as string ?? string.Empty; }
            set { ViewState[ViewStateKey.TransactionCode] = value; }
        }

        /// <summary>
        /// Gets or sets the payment schedule id.
        /// </summary>
        protected int? ScheduleId
        {
            get { return ViewState[ViewStateKey.ScheduleId] as int?; }
            set { ViewState[ViewStateKey.ScheduleId] = value; }
        }

        protected bool DisplayPhone
        {
            get { return ViewState[ViewStateKey.DisplayPhone].ToString().AsBoolean(); }
            set { ViewState[ViewStateKey.DisplayPhone] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the SMS opt-in checkbox to enable SMS Messaging for the phone number.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [SMS opt in]; otherwise, <c>false</c>.
        /// </value>
        protected bool DisplaySmsOptIn
        {
            get { return ViewState[ViewStateKey.DisplaySmsOptIn].ToString().AsBoolean(); }
            set { ViewState[ViewStateKey.DisplaySmsOptIn] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether is configured for partial postbacks. If partial postbacks disabled we'll need to not add history points.
        /// </summary>
        protected bool PartialPostbacksAllowed
        {
            get
            {
                return ScriptManager.GetCurrent( this.Page ).EnablePartialRendering;
            }
        }

        /// <summary>
        /// Gets or sets the accounts that are available for user to add to the list.
        /// </summary>
        protected List<AccountItem> AvailableAccounts { get; set; }

        #endregion

        #region enums

        /// <summary>
        ///
        /// </summary>
        private enum EntryStep
        {
            PromptForAmount = 1,
            ShowConfirmation = 2,
            ShowTransactionSummary = 3
        }

        #endregion enums

        #region fields

        private Control _hostedPaymentInfoControl;

        /// <summary>
        /// use FinancialGateway instead
        /// </summary>
        private Rock.Model.FinancialGateway _financialGateway = null;

        /// <summary>
        /// Gets the financial gateway (model) that is configured for this block
        /// </summary>
        /// <returns></returns>
        private Rock.Model.FinancialGateway FinancialGateway
        {
            get
            {
                if ( _financialGateway == null )
                {
                    RockContext rockContext = new RockContext();
                    var financialGatewayGuid = this.GetAttributeValue( AttributeKey.FinancialGateway ).AsGuid();
                    _financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayGuid );
                }

                return _financialGateway;
            }
        }

        private IHostedGatewayComponent _financialGatewayComponent = null;

        /// <summary>
        /// Gets the financial gateway component that is configured for this block
        /// </summary>
        /// <returns></returns>
        private IHostedGatewayComponent FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null )
                {
                    var financialGateway = FinancialGateway;
                    if ( financialGateway != null )
                    {
                        _financialGatewayComponent = financialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                    }
                }

                return _financialGatewayComponent;
            }
        }

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += TransactionEntry_BlockUpdated;

            // Add handler for page navigation
            RockPage page = Page as RockPage;
            if ( page != null )
            {
                page.PageNavigate += page_PageNavigate;
            }

            using ( var rockContext = new RockContext() )
            {
                SetTargetPerson( rockContext );
                SetGatewayOptions();
                BindSavedAccounts();
            }

            RegisterScript();

            var disableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable;
            cpCaptcha.Visible = !disableCaptchaSupport;
            cpCaptcha.TokenReceived += CpCaptcha_TokenReceived;

            InitializeFinancialGatewayControls();
        }

        /// <summary>
        /// Handles the TokenReceived event of the CpCaptcha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Captcha.TokenReceivedEventArgs"/> instance containing the event data.</param>
        private void CpCaptcha_TokenReceived( object sender, Captcha.TokenReceivedEventArgs e )
        {
            if ( e.IsValid )
            {
                nbPaymentTokenError.Visible= false;
                nbPaymentTokenError.Text = string.Empty;

                _hostedPaymentInfoControl.Visible = true;
                hfHostPaymentInfoSubmitScript.Value = this.FinancialGatewayComponent.GetHostPaymentInfoSubmitScript( this.FinancialGateway, _hostedPaymentInfoControl );
                cpCaptcha.Visible = false;
                return;
            }

            nbPaymentTokenError.Visible= true;
            nbPaymentTokenError.Text = "There was an issue processing your request. Please try again. If the issue persists please contact us.";
            cpCaptcha.Visible = false;
            btnHostedPaymentInfoNext.Visible = false;
        }

        private void InitializeFinancialGatewayControls()
        {
            if ( this.FinancialGatewayComponent == null || this.FinancialGateway == null )
            {
                return;
            }

            var hostedPaymentInfoControlOptions = new HostedPaymentInfoControlOptions
            {
                EnableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean(),
                EnableCreditCard = this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean()
            };

            _hostedPaymentInfoControl = this.FinancialGatewayComponent.GetHostedPaymentInfoControl( this.FinancialGateway, $"_hostedPaymentInfoControl_{this.FinancialGateway.Id}", hostedPaymentInfoControlOptions );
            _hostedPaymentInfoControl.Visible = false;
            phHostedPaymentControl.Controls.Add( _hostedPaymentInfoControl );

            nbPaymentTokenError.Text = "Loading...";
            nbPaymentTokenError.Visible = true;

            if ( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable )
            {
                hfHostPaymentInfoSubmitScript.Value = this.FinancialGatewayComponent.GetHostPaymentInfoSubmitScript( this.FinancialGateway, _hostedPaymentInfoControl );
                _hostedPaymentInfoControl.Visible = true;

                nbPaymentTokenError.Visible= false;
                nbPaymentTokenError.Text = string.Empty;
            }

            if ( _hostedPaymentInfoControl is IHostedGatewayPaymentControlTokenEvent )
            {
                ( _hostedPaymentInfoControl as IHostedGatewayPaymentControlTokenEvent ).TokenReceived += _hostedPaymentInfoControl_TokenReceived;
            }
        }

        /// <summary>
        /// Handles the TokenReceived event of the _hostedPaymentInfoControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _hostedPaymentInfoControl_TokenReceived( object sender, HostedGatewayPaymentControlTokenEventArgs e )
        {
            if ( !e.IsValid )
            {
                if ( e.ErrorMessage.IsNullOrWhiteSpace() )
                {
                    nbPaymentTokenError.Text = "Unknown error";
                }
                else
                {
                    nbPaymentTokenError.Text = e.ErrorMessage;
                }

                nbPaymentTokenError.Visible = true;
            }
            else
            {
                nbPaymentTokenError.Visible = false;
                btnHostedPaymentInfoNext_Click( sender, e );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the TransactionEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TransactionEntry_BlockUpdated( object sender, EventArgs e )
        {
            // if block settings have changed, reload the page
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Hide the messages on every postback
            nbMessage.Visible = false;
            nbSelectionMessage.Visible = false;
            nbConfirmationMessage.Visible = false;
            nbConfirmationMessage.Visible = false;

            pnlDupWarning.Visible = false;
            nbSaveAccount.Visible = false;

            if ( !LoadGatewayOptions() )
            {
                return;
            }

            // Check if this is a transfer and that the person is the authorized person on the transaction
            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Transfer ) ) && !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.ScheduledTransactionGuid ) ) )
            {
                InitializeTransfer( PageParameter( PageParameterKey.ScheduledTransactionGuid ).AsGuidOrNull() );
            }

            if ( !Page.IsPostBack )
            {
                hfTransactionGuid.Value = Guid.NewGuid().ToString();
                if ( this.Request.UrlReferrer != null )
                {
                    lHistoryBackButton.HRef = this.Request.UrlReferrer.ToString();
                }
                else
                {
                    lHistoryBackButton.HRef = "#";
                }

                SetControlOptions();

                if ( _scheduledTransactionToBeTransferred != null )
                {
                    // Was this NOT a personal gift? If so, we need to set the correct business in the Give As section.
                    if ( _scheduledTransactionToBeTransferred.AuthorizedPersonAlias.Person.GivingId != _targetPerson.GivingId )
                    {
                        tglGiveAsOption.Checked = false;
                        SetGiveAsOptions();
                        ShowBusiness();
                    }
                }

                SetPage( EntryStep.PromptForAmount );

                // If an invalid PersonToken was specified, hide everything except for the error message
                if ( nbInvalidPersonWarning.Visible )
                {
                    pnlSelection.Visible = false;
                }

                ConfigureCampusAccountAmountPicker();
            }
            else
            {
                string[] eventArgs = ( this.Page.Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( new[] { "=" }, StringSplitOptions.RemoveEmptyEntries );

                if ( eventArgs.Length == 2 && eventArgs[0] == "btnAddAccountLiteral" && int.TryParse( eventArgs[1], out int accountId ) )
                {
                    UpdateAvailableAccounts( accountId );
                }
                else
                {
                    UpdateAvailableAccounts( null );
                }
            }

            // Set the frequency date label based on if 'One Time' is selected or not
            if ( btnFrequency.Items.Count > 0 )
            {
                dtpStartDate.Label = btnFrequency.Items[0].Selected ? "When" : "First Gift";
                if ( _scheduledTransactionToBeTransferred != null && _scheduledTransactionToBeTransferred.NextPaymentDate.HasValue )
                {
                    dtpStartDate.Label = "Next Gift";
                }
            }

            // Show save account info based on if checkbox is checked
            divSaveAccount.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";

            ResolveHeaderFooterTemplates();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.AvailableAccountsJSON] = AvailableAccounts.ToJson();
            return base.SaveViewState();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            AvailableAccounts = ( ViewState[ViewStateKey.AvailableAccountsJSON] as string ).FromJsonOrNull<List<AccountItem>>() ?? new List<AccountItem>();
        }

        /// <summary>
        /// Configures the campus account amount picker.
        /// </summary>
        private void ConfigureCampusAccountAmountPicker()
        {
            var allowAccountsInUrl = this.GetAttributeValue( AttributeKey.AllowAccountOptionsInURL ).AsBoolean();
            var rockContext = new RockContext();
            List<int> selectableAccountIds = new FinancialAccountService( rockContext ).GetByGuids( this.GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList() )
                .OrderBy( a => a.Order )
                .Select( a => a.Id )
                .ToList();
            CampusAccountAmountPicker.AccountIdAmount[] accountAmounts = null;
            caapPromptForAccountAmounts.AccountHeaderTemplate = this.GetAttributeValue( AttributeKey.AccountHeaderTemplate );

            AvailableAccounts = new List<AccountItem>();

            bool enableMultiAccount = this.GetAttributeValue( AttributeKey.EnableMultiAccount ).AsBoolean();
            if ( enableMultiAccount )
            {
                caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.MultipleAccounts;
            }
            else
            {
                caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;
            }

            caapPromptForAccountAmounts.CampusId = GetCampusId( _targetPerson );
            caapPromptForAccountAmounts.UseAccountCampusMappingLogic = this.GetAttributeValue( AttributeKey.UseAccountCampusMappingLogic ).AsBooleanOrNull() ?? false;
            caapPromptForAccountAmounts.AskForCampusIfKnown = this.GetAttributeValue( AttributeKey.AskForCampusIfKnown ).AsBoolean();
            caapPromptForAccountAmounts.IncludeInactiveCampuses = this.GetAttributeValue( AttributeKey.IncludeInactiveCampuses ).AsBoolean();
            caapPromptForAccountAmounts.OrderBySelectableAccountsIndex = true;
            var includedCampusStatusIds = this.GetAttributeValues( AttributeKey.IncludedCampusStatuses )
                .ToList()
                .AsGuidList()
                .Select( a => DefinedValueCache.Get( a ) )
                .Where( a => a != null )
                .Select( a => a.Id ).ToArray();

            caapPromptForAccountAmounts.IncludedCampusStatusIds = includedCampusStatusIds;

            var includedCampusTypeIds = this.GetAttributeValues( AttributeKey.IncludedCampusTypes )
                .ToList()
                .AsGuidList()
                .Select( a => DefinedValueCache.Get( a ) )
                .Where( a => a != null )
                .Select( a => a.Id ).ToArray();

            caapPromptForAccountAmounts.IncludedCampusTypeIds = includedCampusTypeIds;
            caapPromptForAccountAmounts.AllowPrivateSelectableAccounts = !GetAttributeValue( AttributeKey.OnlyPublicAccountsInURL ).AsBoolean();

            if ( allowAccountsInUrl )
            {
                List<ParameterAccountOption> parameterAccountOptions = ParseAccountUrlOptions();
                if ( parameterAccountOptions.Any() )
                {
                    selectableAccountIds = parameterAccountOptions.Select( a => a.AccountId ).ToList();
                    string invalidAccountInURLMessage = this.GetAttributeValue( AttributeKey.InvalidAccountMessage );
                    if ( invalidAccountInURLMessage.IsNotNullOrWhiteSpace() )
                    {
                        var validAccountUrlIdsQuery = new FinancialAccountService( rockContext ).GetByIds( selectableAccountIds )
                            .Where( a =>
                                 a.IsActive &&
                                 ( a.StartDate == null || a.StartDate <= RockDateTime.Today ) &&
                                 ( a.EndDate == null || a.EndDate >= RockDateTime.Today ) );

                        if ( this.GetAttributeValue( AttributeKey.OnlyPublicAccountsInURL ).AsBooleanOrNull() ?? true )
                        {
                            validAccountUrlIdsQuery = validAccountUrlIdsQuery.Where( a => a.IsPublic == true );
                        }

                        var validAccountIds = validAccountUrlIdsQuery.Select( a => a.Id ).ToList();

                        if ( selectableAccountIds.Where( a => !validAccountIds.Contains( a ) ).Any() )
                        {
                            nbConfigurationNotification.Title = "";
                            nbConfigurationNotification.Text = invalidAccountInURLMessage;
                            nbConfigurationNotification.NotificationBoxType = NotificationBoxType.Validation;
                            nbConfigurationNotification.Visible = true;
                        }
                    }

                    var parameterAccountAmounts = parameterAccountOptions.Select( a => new CampusAccountAmountPicker.AccountIdAmount( a.AccountId, a.Amount ) { ReadOnly = !a.Enabled } );
                    accountAmounts = parameterAccountAmounts.ToArray();
                }
            }

            caapPromptForAccountAmounts.SelectableAccountIds = selectableAccountIds.ToArray();

            ConfigureAvailableAccounts( rockContext );

            if ( accountAmounts != null )
            {
                caapPromptForAccountAmounts.AccountAmounts = accountAmounts;
            }
        }

        /// <summary>
        /// Sets the selected CampusId from a CampusId url parameter or the target person of the transaction.
        /// </summary>
        private int? GetCampusId( Person person )
        {
            var campusId = this.PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            if ( !campusId.HasValue && person != null )
            {
                var personCampus = person.GetCampus();
                if ( personCampus != null )
                {
                    campusId = personCampus.Id;
                }
            }

            return campusId;
        }

        /// <summary>
        /// Configures the available accounts.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void ConfigureAvailableAccounts( RockContext rockContext )
        {
            // If there are no SelectableAccountIds on the CampusAccountAmountPicker, then all the available accounts will be displayed
            // so there is no need to configure the add account button
            if ( caapPromptForAccountAmounts.SelectableAccountIds.Length == 0 )
            {
                return;
            }

            var enableAccountHierarchy = GetAttributeValue( AttributeKey.EnableAccountHierarchy ).AsBoolean();

            GetAvailableAccounts( rockContext, enableAccountHierarchy );

            DatabindAddAccountsButton( enableAccountHierarchy );
        }

        /// <summary>
        /// Gets the available accounts in chunks to prevent SQL complexity errors.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="enableAccountHierarchy">if set to <c>true</c> [enable account hierarchy].</param>
        private void GetAvailableAccounts( RockContext rockContext, bool enableAccountHierarchy )
        {
            var financialAccountService = new FinancialAccountService( rockContext );
            var availableAccounts = financialAccountService.Queryable()
            .Where( f =>
                f.IsActive
                    && f.IsPublic.HasValue
                    && f.IsPublic.Value
                    && !caapPromptForAccountAmounts.SelectableAccountIds.Contains( f.Id )
                    && ( f.StartDate == null || f.StartDate <= RockDateTime.Today )
                    && ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) );

            if ( enableAccountHierarchy )
            {
                availableAccounts = availableAccounts.OrderBy( f => f.PublicName );
            }
            else
            {
                availableAccounts = availableAccounts.OrderBy( f => f.Order );
            }

            var accountIds = availableAccounts.Select( f => f.Id ).ToList();

            while ( accountIds.Any() )
            {
                // Process the accounts in chunks of 1000 to prevent any potential SQL complexity errors.
                List<int> accountIdsChunk = accountIds.Take( 1000 ).ToList();
                var accountsChunk = availableAccounts.Where( a => accountIdsChunk.Contains( a.Id ) );

                var childList = accountsChunk
                    .Where( f =>
                        f.ParentAccountId.HasValue
                        && accountIds.Contains( f.ParentAccountId.Value )
                        && !caapPromptForAccountAmounts.SelectableAccountIds.Contains( f.Id ) )
                    .ToList();

                // Enumerate through all active accounts that are public
                foreach ( var account in accountsChunk )
                {
                    var accountItem = new AccountItem() { Id = account.Id, PublicName = account.PublicName, ParentAccountId = account.ParentAccountId };

                    if ( enableAccountHierarchy )
                    {
                        accountItem.HasChildren = childList.Any( f => f.ParentAccountId == accountItem.Id && !availableAccounts.Any( fa => fa.ParentAccountId == f.Id ) );
                        accountItem.Children = childList.Where( f => f.ParentAccountId == accountItem.Id && !availableAccounts.Any( fa => fa.ParentAccountId == f.Id ) )
                            .Select( f => new AccountItem() { Id = f.Id, PublicName = f.PublicName, ParentAccountId = f.ParentAccountId } )
                            .ToList();
                        // An account is considered a root item in the hierarchical mode if it is a top level account without children or is a parent account to any other child account and has children.
                        accountItem.IsRootItem = ( !account.ParentAccountId.HasValue && !accountItem.HasChildren ) || ( availableAccounts.Any( f => f.ParentAccountId == account.Id && accountItem.HasChildren ) );
                    }

                    AvailableAccounts.Add( accountItem );
                }

                accountIds = accountIds.Where( a => !accountIdsChunk.Contains( a ) ).ToList();
            }
        }

        /// <summary>
        /// Databinds the add accounts button.
        /// </summary>
        /// <param name="enableAccountHierachy">if set to <c>true</c> [enable account hierachy].</param>
        private void DatabindAddAccountsButton( bool enableAccountHierachy )
        {
            // Further filter available accounts to return higher level accounts with any child accounts without children of their own.
            // If the child account has children of their own it will act as the root of a hierarchy, and should not be included in the parent's list of child accounts.
            var hierarchicalAccounts = AvailableAccounts.Where( a => a.IsRootItem || a.Children.Any( c => !c.HasChildren ) );
            phbtnAddAccount.Visible = GetAttributeValue( AttributeKey.AdditionalAccounts ).AsBoolean();

            if ( phbtnAddAccount.Visible )
            {
                phbtnAddAccount.Visible = enableAccountHierachy ? hierarchicalAccounts.Any() : AvailableAccounts.Any();
                phbtnAddAccount.Controls.Clear();

                var additionalAccounts = enableAccountHierachy ? hierarchicalAccounts : AvailableAccounts;

                var literal = new LiteralControl() { ID = "btnAddAccountLiteral" };
                var openingHtml = $@"
<div class=""btn-group js-button-dropdownlist"">
    <button type=""button"" class=""btn btn-default dropdown-toggle js-buttondropdown-btn-select"" data-toggle=""dropdown"" aria-expanded=""false"">{GetAttributeValue( AttributeKey.AddAccountText )} <span class=""fa fa-caret-down""></span></button>
    <ul class=""dropdown-menu"">
";

                const string closingHtml = @"
    </ul>
</div>
";
                var htmlBuilder = new StringBuilder( openingHtml );
                foreach ( var accountItem in additionalAccounts )
                {
                    if ( accountItem.HasChildren )
                    {
                        htmlBuilder.Append( "<li class=\"dropdown-submenu\"><a class=\"dropdown-submenu-toggle\">" );
                    }
                    else
                    {
                        htmlBuilder.Append( $"<li><a href=\"javascript:__doPostBack('{upPayment.ClientID}', '{literal.ID}={accountItem.Id}')\" data-id='{accountItem.Id}'>" );
                    }

                    if ( accountItem.HasChildren )
                    {
                        htmlBuilder.Append( $"{accountItem.PublicName}<span class=\"caret\"></span></a><ul class=\"dropdown-menu\">" );
                        foreach ( var listItemChild in accountItem.Children )
                        {
                            htmlBuilder.Append( $"<li><a " );
                            htmlBuilder.Append( $"href=\"javascript:__doPostBack('{upPayment.ClientID}', '{literal.ID}={listItemChild.Id}')\" data-id='{listItemChild.Id}'>" );
                            htmlBuilder.Append( $"{listItemChild.PublicName}</a></li>" );
                        }
                        htmlBuilder.Append( "</ul></li>" );
                    }
                    else
                    {
                        htmlBuilder.Append( $"{accountItem.PublicName}</a></li>" );
                    }
                }

                htmlBuilder.Append( closingHtml );

                literal.Text = htmlBuilder.ToString();

                phbtnAddAccount.Controls.Add( literal );
            }
        }

        /// <summary>
        /// Parses the account URL options.
        /// </summary>
        /// <returns></returns>
        private List<ParameterAccountOption> ParseAccountUrlOptions()
        {
            List<ParameterAccountOption> result = new List<ParameterAccountOption>();
            result.AddRange( ParseAccountUrlOptionsParameter( this.PageParameter( PageParameterKey.AccountIdsOptions ), false ) );
            result.AddRange( ParseAccountUrlOptionsParameter( this.PageParameter( PageParameterKey.AccountGlCodesOptions ), true ) );
            return result;
        }

        /// <summary>
        /// Parses the account URL options parameter.
        /// </summary>
        /// <param name="accountOptionsParameterValue">The account options parameter value.</param>
        /// <param name="parseAsAccountGLCode">if set to <c>true</c> [parse as account gl code].</param>
        /// <returns></returns>
        private List<ParameterAccountOption> ParseAccountUrlOptionsParameter( string accountOptionsParameterValue, bool parseAsAccountGLCode )
        {
            List<ParameterAccountOption> result = new List<ParameterAccountOption>();
            if ( accountOptionsParameterValue.IsNullOrWhiteSpace() )
            {
                return result;
            }

            var onlyPublicAccountsInURL = this.GetAttributeValue( AttributeKey.OnlyPublicAccountsInURL ).AsBoolean();

            var accountOptions = Server.UrlDecode( accountOptionsParameterValue ).Split( ',' );

            foreach ( var accountOption in accountOptions )
            {
                ParameterAccountOption parameterAccountOption = new ParameterAccountOption();
                var accountOptionParts = accountOption.Split( '^' ).ToList();
                if ( accountOptionParts.Count > 0 )
                {
                    while ( accountOptionParts.Count < 3 )
                    {
                        accountOptionParts.Add( null );
                    }

                    if ( parseAsAccountGLCode )
                    {
                        var accountGLCode = accountOptionParts[0];
                        if ( accountGLCode.IsNotNullOrWhiteSpace() )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                parameterAccountOption.AccountId = new FinancialAccountService( rockContext )
                                    .Queryable()
                                    .Where( a => a.GlCode == accountGLCode &&
                                    a.IsActive &&
                                    ( onlyPublicAccountsInURL ? ( a.IsPublic ?? false ) : true ) &&
                                    ( a.StartDate == null || a.StartDate <= RockDateTime.Today ) &&
                                    ( a.EndDate == null || a.EndDate >= RockDateTime.Today ) )
                                    .Select( a => a.Id ).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        parameterAccountOption.AccountId = accountOptionParts[0].AsInteger();
                    }

                    parameterAccountOption.Amount = accountOptionParts[1].AsDecimalOrNull();
                    parameterAccountOption.Enabled = accountOptionParts[2].AsBooleanOrNull() ?? true;
                    result.Add( parameterAccountOption );
                }
            }

            return result;
        }

        /// <summary>
        /// Updates the available accounts.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        private void UpdateAvailableAccounts( int? accountId )
        {
            var selected = AvailableAccounts.Where( a => a.Id == ( accountId ?? 0 ) ).ToList();
            AvailableAccounts = AvailableAccounts.Except( selected ).ToList();

            // If the selected account was a child account remove it from the parent's children list so it does not show up in the UI.
            foreach ( var accountItem in selected.Where( a => a.ParentAccountId.HasValue ) )
            {
                var parentAccount = AvailableAccounts.FirstOrDefault( a => a.Id == accountItem.ParentAccountId.Value );
                parentAccount?.UpdateChildItems( accountItem, AvailableAccounts );
            }

            DatabindAddAccountsButton( GetAttributeValue( AttributeKey.EnableAccountHierarchy ).AsBoolean() );

            if ( accountId.HasValue )
            {
                var selectableAccountIds = caapPromptForAccountAmounts.SelectableAccountIds.ToList();
                selectableAccountIds.Add( accountId.Value );
                caapPromptForAccountAmounts.SelectableAccountIds = selectableAccountIds.ToArray();
            }
        }

        #endregion

        #region Gateway Help Related

        /// <summary>
        /// Handles the ItemDataBound event of the rptInstalledGateways control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptInstalledGateways_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            IHostedGatewayComponent financialGatewayComponent = e.Item.DataItem as IHostedGatewayComponent;
            if ( financialGatewayComponent == null )
            {
                return;
            }

            var gatewayEntityType = EntityTypeCache.Get( financialGatewayComponent.TypeGuid );
            var gatewayEntityTypeType = gatewayEntityType.GetEntityType();

            HiddenField hfGatewayEntityTypeId = e.Item.FindControl( "hfGatewayEntityTypeId" ) as HiddenField;
            hfGatewayEntityTypeId.Value = gatewayEntityType.Id.ToString();

            Literal lGatewayName = e.Item.FindControl( "lGatewayName" ) as Literal;
            Literal lGatewayDescription = e.Item.FindControl( "lGatewayDescription" ) as Literal;

            lGatewayName.Text = Reflection.GetDisplayName( gatewayEntityTypeType );
            lGatewayDescription.Text = Reflection.GetDescription( gatewayEntityTypeType );

            HyperLink aGatewayConfigure = e.Item.FindControl( "aGatewayConfigure" ) as HyperLink;
            HyperLink aGatewayLearnMore = e.Item.FindControl( "aGatewayLearnMore" ) as HyperLink;
            aGatewayConfigure.Visible = financialGatewayComponent.ConfigureURL.IsNotNullOrWhiteSpace();
            aGatewayLearnMore.Visible = financialGatewayComponent.LearnMoreURL.IsNotNullOrWhiteSpace();

            aGatewayConfigure.NavigateUrl = financialGatewayComponent.ConfigureURL;
            aGatewayLearnMore.NavigateUrl = financialGatewayComponent.LearnMoreURL;
        }

        /// <summary>
        /// Loads and Validates the gateways, showing a message if the gateways aren't configured correctly
        /// </summary>
        private bool LoadGatewayOptions()
        {
            if ( this.FinancialGateway == null )
            {
                ShowGatewayHelp();
                return false;
            }
            else
            {
                HideGatewayHelp();
            }

            // get the FinancialGateway's GatewayComponent so we can show a warning if they have an unsupported gateway.
            var hostedGatewayComponent = FinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;

            var testGatewayGuid = Rock.SystemGuid.EntityType.FINANCIAL_GATEWAY_TEST_GATEWAY.AsGuid();

            bool enableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            bool enableCreditCard = this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();

            if ( enableACH == false && enableCreditCard == false )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Configuration", "Enable ACH and/or Enable Credit Card needs to be enabled." );
                pnlTransactionEntry.Visible = false;
                return false;
            }

            if ( hostedGatewayComponent == null )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Unsupported Gateway", "This block only supports Gateways that have a hosted payment interface." );
                pnlTransactionEntry.Visible = false;
                return false;
            }
            else if ( this.FinancialGatewayComponent.TypeGuid == testGatewayGuid )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Testing", "You are using the Test Financial Gateway. No actual amounts will be charged to your card or bank account." );
            }
            else
            {
                HideConfigurationMessage();
            }

            return true;
        }

        /// <summary>
        /// Shows the gateway help
        /// </summary>
        private void ShowGatewayHelp()
        {
            pnlGatewayHelp.Visible = true;
            pnlTransactionEntry.Visible = false;

            var hostedGatewayComponentList = Rock.Financial.GatewayContainer.Instance.Components
                .Select( a => a.Value.Value )
                .Where( a => a is IHostedGatewayComponent && !( a is TestGateway ) )
                .Select( a => a as IHostedGatewayComponent ).ToList();

            rptInstalledGateways.DataSource = hostedGatewayComponentList;
            rptInstalledGateways.DataBind();
        }

        /// <summary>
        /// Hides the gateway help.
        /// </summary>
        private void HideGatewayHelp()
        {
            pnlGatewayHelp.Visible = false;
        }

        /// <summary>
        /// Shows the configuration message.
        /// </summary>
        /// <param name="notificationBoxType">Type of the notification box.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowConfigurationMessage( NotificationBoxType notificationBoxType, string title, string message )
        {
            nbConfigurationNotification.NotificationBoxType = notificationBoxType;
            nbConfigurationNotification.Title = title;
            nbConfigurationNotification.Text = message;

            nbConfigurationNotification.Visible = true;
        }

        /// <summary>
        /// Hides the configuration message.
        /// </summary>
        private void HideConfigurationMessage()
        {
            nbConfigurationNotification.Visible = false;
        }

        #endregion Gateway Help Related

        #region Events

        /// <summary>
        /// Handles the PageNavigate event of the page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        protected void page_PageNavigate( object sender, HistoryEventArgs e )
        {
            EntryStep? entryStep = e.State["GivingDetail"].ConvertToEnumOrNull<EntryStep>();
            if ( entryStep.HasValue )
            {
                SetPage( entryStep.Value );
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the btnFrequency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFrequency_SelectionChanged( object sender, EventArgs e )
        {
            int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
            bool oneTime = ( btnFrequency.SelectedValueAsInt() ?? 0 ) == oneTimeFrequencyId;

            dtpStartDate.Label = oneTime ? "When" : "First Gift";

            var earliestScheduledStartDate = FinancialGatewayComponent.GetEarliestScheduledStartDate( FinancialGateway );

            // if scheduling recurring, it can't start today since the gateway will be taking care of automated giving, it might have already processed today's transaction. So make sure it is no earlier than the gateway's earliest start date.
            if ( !oneTime && ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate.Value.Date < earliestScheduledStartDate ) )
            {
                dtpStartDate.SelectedDate = earliestScheduledStartDate;
            }

            BindSavedAccounts();

            SetPage( EntryStep.PromptForAmount );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglGiveAsOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglGiveAsOption_CheckedChanged( object sender, EventArgs e )
        {
            SetGiveAsOptions();
            if ( tglGiveAsOption.Checked )
            {
                ShowPersonal( GetPerson( false ) );
            }
            else
            {
                ShowBusiness();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblBusinessOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblBusinessOption_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowBusiness();
        }

        /// <summary>
        /// Handles the Click event of the btnSavedAccountPaymentInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSavedAccountPaymentInfoNext_Click( object sender, EventArgs e )
        {
            HandlePaymentInfoNextButton();
        }

        /// <summary>
        /// Handles the Click event of the btnHostedPaymentInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnHostedPaymentInfoNext_Click( object sender, EventArgs e )
        {
            HandlePaymentInfoNextButton();
        }

        /// <summary>
        /// Handles the payment information next button.
        /// </summary>
        private void HandlePaymentInfoNextButton()
        {
            if ( ValidatePaymentInfo( out string errorMessage ) )
            {
                ReferencePaymentInfo paymentInfo = GetPaymentInfo( out errorMessage );
                if ( !string.IsNullOrEmpty( errorMessage ) )
                {
                    ShowMessage( NotificationBoxType.Validation, "Before we finish...", errorMessage );
                    return;
                }
                else if ( paymentInfo == null )
                {
                    errorMessage = "There was a problem creating the payment information";
                    ShowMessage( NotificationBoxType.Validation, "Before we finish...", errorMessage );
                    return;
                }

                SetConfirmationText( paymentInfo );
                if ( this.PartialPostbacksAllowed )
                {
                    this.AddHistory( "GivingDetail", EntryStep.PromptForAmount.ConvertToString( false ), null );
                }

                bool showConfirmationPage = this.GetAttributeValue( AttributeKey.ShowConfirmationPage ).AsBoolean();
                if ( showConfirmationPage )
                {
                    SetPage( EntryStep.ShowConfirmation );
                    pnlConfirmation.Focus();
                }
                else
                {
                    // Skip displaying the confirmation page, and process the transaction.
                    btnProcessTransactionFromConfirmationPage_Click( null, null );
                }
            }
            else
            {
                ShowMessage( NotificationBoxType.Validation, "Before we finish...", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmationPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmationPrev_Click( object sender, EventArgs e )
        {
            SetPage( EntryStep.PromptForAmount );
            pnlSelection.Focus();
        }

        /// <summary>
        /// Handles the Click event of the btnProcessTransactionFromConfirmationPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnProcessTransactionFromConfirmationPage_Click( object sender, EventArgs e )
        {
            if ( ProcessTransaction( out string errorMessage ) )
            {
                if ( this.PartialPostbacksAllowed )
                {
                    this.AddHistory( "GivingDetail", EntryStep.ShowConfirmation.ConvertToString( false ), null );
                }

                SetPage( EntryStep.ShowTransactionSummary );
                pnlSuccess.Focus();
            }
            else
            {
                ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmDuplicateTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmDuplicateTransaction_Click( object sender, EventArgs e )
        {
            // They are hitting Confirm on the "Possible Duplicate" warning, so reset the TransactionCode and Transaction.Guid which would have preventing them from doing a duplicate
            TransactionCode = string.Empty;
            hfTransactionGuid.Value = Guid.NewGuid().ToString();

            string errorMessage = string.Empty;
            if ( ProcessTransaction( out errorMessage ) )
            {
                SetPage( EntryStep.ShowTransactionSummary );
                pnlSuccess.Focus();
            }
            else
            {
                ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( phCreateLogin.Visible )
                {
                    if ( string.IsNullOrWhiteSpace( txtUserName.Text ) || string.IsNullOrWhiteSpace( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = "Missing Information";
                        nbSaveAccount.Text = "A username and password are required when saving an account";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                    {
                        nbSaveAccount.Title = "Invalid Username";
                        nbSaveAccount.Text = "The selected Username is already being used.  Please select a different Username";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( !UserLoginService.IsPasswordValid( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = string.Empty;
                        nbSaveAccount.Text = UserLoginService.FriendlyPasswordRules();
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( txtPasswordConfirm.Text != txtPassword.Text )
                    {
                        nbSaveAccount.Title = "Invalid Password";
                        nbSaveAccount.Text = "The password and password confirmation do not match";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
                {
                    CreateSavedAccount( txtSaveAccount.Text, rockContext );
                }
                else
                {
                    nbSaveAccount.Title = "Missing Account Name";
                    nbSaveAccount.Text = "Please enter a name to use for this account.";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                    nbSaveAccount.Visible = true;
                }
            }
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Gets the transaction entity.
        /// </summary>
        /// <returns></returns>
        private IEntity GetTransactionEntity()
        {
            IEntity transactionEntity = null;
            Guid? transactionEntityTypeGuid = GetAttributeValue( AttributeKey.TransactionEntityType ).AsGuidOrNull();
            if ( transactionEntityTypeGuid.HasValue )
            {
                var transactionEntityType = EntityTypeCache.Get( transactionEntityTypeGuid.Value );
                if ( transactionEntityType != null )
                {
                    var entityId = this.PageParameter( this.GetAttributeValue( AttributeKey.EntityIdParam ) ).AsIntegerOrNull();
                    if ( entityId.HasValue )
                    {
                        var dbContext = Reflection.GetDbContextForEntityType( transactionEntityType.GetEntityType() );
                        IService serviceInstance = Reflection.GetServiceForEntityType( transactionEntityType.GetEntityType(), dbContext );
                        if ( serviceInstance != null )
                        {
                            System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                            transactionEntity = getMethod.Invoke( serviceInstance, new object[] { entityId.Value } ) as Rock.Data.IEntity;
                        }
                    }
                }
            }

            return transactionEntity;
        }

        private void SetTargetPerson( RockContext rockContext )
        {
            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean(); // Must allow impersonation if setting up Text-To-Give.
            var allowImpersonation = enableTextToGiveSetup || ( GetAttributeValue( AttributeKey.Impersonation ).AsBooleanOrNull() ?? false );
            string personActionId = PageParameter( PageParameterKey.PersonActionIdentifier );

            if ( personActionId.IsNotNullOrWhiteSpace() )
            {
                // If a person key was supplied then try to get that person
                _targetPerson = new PersonService( rockContext ).GetByPersonActionIdentifier( personActionId, "transaction" );

                // Pre-load campus to avoid lazy loading later when the _targetPerson field is utilized.
                _targetPerson.GetCampus();

                if ( allowImpersonation )
                {
                    // If impersonation is allowed then ensure the supplied person key was valid
                    if ( _targetPerson == null )
                    {
                        nbInvalidPersonWarning.Text = "Invalid or Expired Person Token specified";
                        nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                        nbInvalidPersonWarning.Visible = true;
                        return;
                    }
                }
                else
                {
                    // If impersonation is not allowed show an error if the target and current user are not the same
                    if ( _targetPerson?.Id != CurrentPerson?.Id )
                    {
                        nbInvalidPersonWarning.Text = $"Impersonation is not allowed on this block.";
                        nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                        nbInvalidPersonWarning.Visible = true;
                        return;
                    }
                }
            }
            else
            {
                // If a person key was not provided then use the Current Person, which may be null
                _targetPerson = CurrentPerson;
            }
        }

        private void SetGatewayOptions()
        {
            bool allowScheduled = GetAttributeValue( AttributeKey.AllowScheduled ).AsBoolean();
            if ( allowScheduled && this.FinancialGatewayComponent != null )
            {
                var supportedFrequencies = this.FinancialGatewayComponent.SupportedPaymentSchedules;

                if ( supportedFrequencies.Any() )
                {
                    btnFrequency.DataSource = supportedFrequencies;
                    btnFrequency.DataBind();

                    // If gateway didn't specifically support one-time, add it anyway for immediate gifts
                    var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                    if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
                    {
                        btnFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Value, oneTimeFrequency.Id.ToString() ) );
                    }

                    btnFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
                    dtpStartDate.SelectedDate = RockDateTime.Today;

                    if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.StartDate ) ) )
                    {
                        dtpStartDate.SelectedDate = PageParameter( PageParameterKey.StartDate ).AsDateTime() ?? RockDateTime.Today;
                        if ( dtpStartDate.SelectedDate < RockDateTime.Today )
                        {
                            dtpStartDate.SelectedDate = RockDateTime.Today;
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Frequency ) ) )
                    {
                        var frequencyValues = PageParameter( PageParameterKey.Frequency ).Split( new char[] { '^' } );
                        if ( btnFrequency.Items.FindByValue( frequencyValues[0] ) != null )
                        {
                            btnFrequency.SelectedValue = frequencyValues[0];
                            if ( frequencyValues.Length >= 2 && frequencyValues[1].AsBoolean( true ) == false )
                            {
                                btnFrequency.Visible = false;
                                txtFrequency.Visible = true;
                                txtFrequency.Text = btnFrequency.SelectedItem.Text;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Binds the saved accounts.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindSavedAccounts()
        {
            rblSavedAccount.Items.Clear();

            if ( _targetPerson == null )
            {
                return;
            }

            rblSavedAccount.Visible = false;
            var currentSavedAccountSelection = rblSavedAccount.SelectedValue;

            var targetPersonId = _targetPerson.Id;
            var personSavedAccountsQuery = new FinancialPersonSavedAccountService( new RockContext() )
                .GetByPersonId( targetPersonId )
                .Where( a => !a.IsSystem )
                .AsNoTracking();

            var financialGateway = this.FinancialGateway;
            var financialGatewayComponent = this.FinancialGatewayComponent;
            if ( financialGateway == null || financialGatewayComponent == null )
            {
                return;
            }

            bool enableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            bool enableCreditCard = this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            List<DefinedValueCache> allowedCurrencyTypes = new List<DefinedValueCache>();

            if ( enableCreditCard && financialGatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency );
            }

            if ( enableACH && financialGatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency );
            }

            int[] allowedCurrencyTypeIds = allowedCurrencyTypes.Select( a => a.Id ).ToArray();

            personSavedAccountsQuery = personSavedAccountsQuery.Where( a =>
                a.FinancialGatewayId == financialGateway.Id
                && ( a.FinancialPaymentDetail.CurrencyTypeValueId != null )
                && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );

            var personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).AsNoTracking().Select( a => new
            {
                a.Id,
                a.Name,
                a.FinancialPaymentDetail
            } ).ToList();

            // Only show the SavedAccount picker if there are saved accounts. If there aren't any (or if they choose 'Use a different payment method'), a later step will prompt them to enter Payment Info (CC/ACH fields)
            rblSavedAccount.Visible = personSavedAccountList.Any();

            rblSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in personSavedAccountList )
            {
                string displayName;
                if ( personSavedAccount.FinancialPaymentDetail.ExpirationDate.IsNotNullOrWhiteSpace() )
                {
                    displayName = $"{personSavedAccount.Name} ({personSavedAccount.FinancialPaymentDetail.AccountNumberMasked} Expires: {personSavedAccount.FinancialPaymentDetail.ExpirationDate})";
                }
                else
                {
                    displayName = $"{personSavedAccount.Name} ({personSavedAccount.FinancialPaymentDetail.AccountNumberMasked})";
                }

                rblSavedAccount.Items.Add( new ListItem( displayName, personSavedAccount.Id.ToString() ) );
            }

            rblSavedAccount.Items.Add( new ListItem( "Use a different payment method", 0.ToString() ) );

            if ( currentSavedAccountSelection.IsNotNullOrWhiteSpace() )
            {
                rblSavedAccount.SetValue( currentSavedAccountSelection );
            }
            else
            {
                rblSavedAccount.SelectedIndex = 0;
            }

            rblSavedAccount_SelectedIndexChanged( null, null );
        }

        protected void rblSavedAccount_SelectedIndexChanged( object sender, EventArgs e )
        {
            bool isSavedAccount = rblSavedAccount.SelectedValue.AsInteger() > 0;
            btnSavedAccountPaymentInfoNext.Visible = isSavedAccount;
            btnHostedPaymentInfoNext.Visible = !isSavedAccount;
            pnlPaymentInfo.Visible = !isSavedAccount;
        }

        /// <summary>
        /// Resolves the lava merge fields for the various header and footer templates.
        /// </summary>
        private void ResolveHeaderFooterTemplates()
        {
            // Resolve the text field merge fields
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage );

            using ( var rockContext = new RockContext() )
            {
                IEntity transactionEntity = GetTransactionEntity();
                if ( transactionEntity != null )
                {
                    mergeFields.Add( "TransactionEntity", transactionEntity );
                    var transactionEntityTypeId = transactionEntity.TypeId;

                    // include any Transactions that are associated with the TransactionEntity for Lava
                    var transactionEntityTransactions = new FinancialTransactionService( rockContext ).Queryable()
                        .Include( a => a.TransactionDetails )
                        .Where( a => a.TransactionDetails.Any( d => d.EntityTypeId.HasValue && d.EntityTypeId == transactionEntityTypeId && d.EntityId == transactionEntity.Id ) )
                        .ToList();

                    var transactionEntityTransactionsTotal = transactionEntityTransactions.SelectMany( d => d.TransactionDetails )
                        .Where( d => d.EntityTypeId.HasValue && d.EntityTypeId == transactionEntityTypeId && d.EntityId == transactionEntity.Id )
                        .Sum( d => ( decimal? ) d.Amount );

                    mergeFields.Add( "TransactionEntityTransactions", transactionEntityTransactions );
                    mergeFields.Add( "TransactionEntityTransactionsTotal", transactionEntityTransactionsTotal );

                    var participationMode = PageParameters().ContainsKey( PageParameterKey.ParticipationMode ) ? PageParameter( PageParameterKey.ParticipationMode ).AsIntegerOrNull() ?? 1 : 1;

                    if ( EntityTypeCache.Get( transactionEntityTypeId ).Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() )
                    {
                        var groupMember = new GroupMemberService( rockContext ).Get( transactionEntity.Guid );
                        GroupService groupService = new GroupService( rockContext );
                        if ( participationMode == ( int ) ParticipationType.Family )
                        {
                            var familyMemberGroupMembersInCurrentGroup = groupService.GroupMembersInAnotherGroup( groupMember.Person.GetFamily(), groupMember.Group );
                            decimal groupFundraisingGoal = 0;
                            foreach ( var member in familyMemberGroupMembersInCurrentGroup )
                            {
                                member.LoadAttributes( rockContext );
                                member.Group.LoadAttributes( rockContext );
                                groupFundraisingGoal += member.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? member.Group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? 0;
                            }

                            var contributionTotal = new FinancialTransactionDetailService( rockContext )
                            .GetContributionsForGroupMemberList( transactionEntityTypeId, familyMemberGroupMembersInCurrentGroup.Select( m => m.Id ).ToList() );
                            mergeFields.Add( "FundraisingGoal", groupFundraisingGoal );
                            mergeFields.Add( "AmountRaised", contributionTotal );
                        }
                        else
                        {
                            groupMember.LoadAttributes( rockContext );
                            groupMember.Group.LoadAttributes( rockContext );
                            var memberFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? groupMember.Group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? 0;
                            mergeFields.Add( "FundraisingGoal", memberFundraisingGoal );
                            mergeFields.Add( "AmountRaised", transactionEntityTransactionsTotal );
                        }
                    }
                }

                mergeFields.Add( "AmountLimit", this.PageParameter( PageParameterKey.AmountLimit ).AsDecimalOrNull() );

                if ( hfTransactionGuid.Value.AsGuidOrNull().HasValue )
                {
                    var financialTransaction = new FinancialTransactionService( rockContext ).Get( hfTransactionGuid.Value.AsGuid() );
                    mergeFields.Add( "FinancialTransaction", financialTransaction );
                }

                lTransactionHeader.Text = GetAttributeValue( AttributeKey.TransactionHeader ).ResolveMergeFields( mergeFields );
                lConfirmationHeader.Text = GetAttributeValue( AttributeKey.ConfirmationHeader ).ResolveMergeFields( mergeFields );
                lConfirmationFooter.Text = GetAttributeValue( AttributeKey.ConfirmationFooter ).ResolveMergeFields( mergeFields );
                lSuccessFooter.Text = GetAttributeValue( AttributeKey.SuccessFooter ).ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Sets the control options.
        /// </summary>
        private void SetControlOptions()
        {
            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();

            // Set heading visibility
            var showPanelHeadings = GetAttributeValue( AttributeKey.ShowPanelHeadings ).AsBoolean();
            pnlHeadingSelection.Visible = showPanelHeadings;
            pnlHeadingConfirmation.Visible = showPanelHeadings;
            pnlHeadingContributionInfoTitle.Visible = showPanelHeadings;
            pnlHeadingPersonalInfoTitle.Visible = showPanelHeadings;
            pnlHeadingPaymentInfoTitle.Visible = showPanelHeadings;
            pnlHeadingConfirmationTitle.Visible = showPanelHeadings;

            // Set page/panel titles
            lPanelTitleSelection.Text = GetAttributeValue( AttributeKey.PanelTitle );
            lPanelTitleConfirmation.Text = GetAttributeValue( AttributeKey.PanelTitle );
            lContributionInfoTitle.Text = GetAttributeValue( AttributeKey.ContributionInfoTitle );
            lPersonalInfoTitle.Text = GetAttributeValue( AttributeKey.PersonalInfoTitle );
            lPaymentInfoTitle.Text = GetAttributeValue( AttributeKey.PaymentInfoTitle );
            lConfirmationTitle.Text = GetAttributeValue( AttributeKey.ConfirmationTitle );
            lSaveAccountTitle.Text = GetAttributeValue( AttributeKey.SaveAccountTitle );
            divRepeatingPayments.Visible = !enableTextToGiveSetup && btnFrequency.Items.Count > 0;

            bool displayEmail = GetAttributeValue( AttributeKey.DisplayEmail ).AsBoolean();
            txtEmail.Visible = displayEmail;
            tdEmailConfirm.Visible = displayEmail;

            DisplayPhone = GetAttributeValue( AttributeKey.DisplayPhone ).AsBoolean();
            DisplaySmsOptIn = GetAttributeValue( AttributeKey.DisplaySmsOptIn ).AsBoolean() && DisplayPhone;
            tdPhoneConfirm.Visible = DisplayPhone;

            pnbPhone.Visible = DisplayPhone;
            cbSmsOptIn.Visible = DisplaySmsOptIn;
            cbSmsOptIn.Text = DisplaySmsOptIn ? Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL ) : string.Empty;

            pnbBusinessContactPhone.Visible = DisplayPhone;
            cbBusinessContactSmsOptIn.Visible = DisplaySmsOptIn;
            cbBusinessContactSmsOptIn.Text = DisplaySmsOptIn ? Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL ) : string.Empty;

            var person = GetPerson( false );
            ShowPersonal( person );

            // Set personal display

            // If a record is created by the Give SMS action, it will not have a name assigned and the user needs to enter one.
            var nameEntryEnabled = ( person == null || person.IsNameless() );
            txtCurrentName.Visible = !nameEntryEnabled;
            txtFirstName.Visible = nameEntryEnabled;
            txtLastName.Visible = nameEntryEnabled;

            cbGiveAnonymously.Visible = GetAttributeValue( AttributeKey.EnableAnonymousGiving ).AsBoolean();
            cbGiveAnonymously.ToolTip = GetAttributeValue( AttributeKey.AnonymousGivingTooltip );

            if ( !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() )
            {
                tglGiveAsOption.Checked = true;
                SetGiveAsOptions();
            }
            else
            {
                phGiveAsOption.Visible = false;
            }

            // Evaluate if comment entry box should be displayed
            txtCommentEntry.Label = GetAttributeValue( AttributeKey.CommentEntryLabel );
            txtCommentEntry.Visible = GetAttributeValue( AttributeKey.EnableCommentEntry ).AsBoolean();

            bool showConfirmationPage = this.GetAttributeValue( AttributeKey.ShowConfirmationPage ).AsBoolean();
            if ( showConfirmationPage )
            {
                btnSavedAccountPaymentInfoNext.Text = "Next";
                btnHostedPaymentInfoNext.Text = "Next";
            }
            else
            {
                if ( enableTextToGiveSetup  )
                {
                    btnSavedAccountPaymentInfoNext.Text = "Give";
                    btnHostedPaymentInfoNext.Text = "Give";
                }
                else
                {
                    btnSavedAccountPaymentInfoNext.Text = "Finish";
                    btnHostedPaymentInfoNext.Text = "Finish";
                }
            }
        }

        #endregion

        #region Methods for the Payment Info Page (panel)

        /// <summary>
        /// Sets the give as options.
        /// </summary>
        private void SetGiveAsOptions()
        {
            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();
            bool givingAsBusiness = !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() && !tglGiveAsOption.Checked;
            bool userLoggedIn = CurrentPerson != null;

            acAddress.Label = givingAsBusiness ? "Business Address" : "Address";
            pnbPhone.Label = givingAsBusiness ? "Business Phone" : "Phone";
            txtEmail.Label = givingAsBusiness ? "Business Email" : "Email";

            phGiveAsPerson.Visible = !givingAsBusiness;
            phGiveAsBusiness.Visible = givingAsBusiness;
            phBusinessContact.Visible = givingAsBusiness && !userLoggedIn;
            int contactPersonId = userLoggedIn ? CurrentPerson.Id : 0;

            if ( givingAsBusiness )
            {
                if ( hfBusinessesLoaded.Value != contactPersonId.ToString() )
                {
                    cblBusiness.Items.Clear();
                    using ( var rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var businesses = personService.GetBusinesses( contactPersonId ).ToList();
                        if ( businesses.Any() )
                        {
                            foreach ( var business in businesses )
                            {
                                cblBusiness.Items.Add( new ListItem( business.LastName, business.Id.ToString() ) );
                            }

                            cblBusiness.Items.Add( new ListItem( "New Business", "" ) );

                            cblBusiness.Visible = true;

                            if ( _scheduledTransactionToBeTransferred != null )
                            {
                                var matchBusiness = businesses.Where( b => b.Id == _scheduledTransactionToBeTransferred.AuthorizedPersonAlias.PersonId ).FirstOrDefault();
                                if ( matchBusiness != null )
                                {
                                    cblBusiness.SetValue( matchBusiness.Id.ToString() );
                                }
                            }
                            else
                            {
                                cblBusiness.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            cblBusiness.Visible = false;
                        }
                    }

                    hfBusinessesLoaded.Value = contactPersonId.ToString();
                }

                lPersonalInfoTitle.Text = "Business Information";
            }
            else
            {
                lPersonalInfoTitle.Text = GetAttributeValue( AttributeKey.PersonalInfoTitle );
            }
        }

        private void ShowPersonal( Person person )
        {
            if ( person != null )
            {
                txtCurrentName.Text = person.FullName;
                txtEmail.Text = person.Email;

                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                if ( DisplayPhone )
                {
                    var phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );

                    // If person did not have a home phone number, read the cell phone number (which would then
                    // get saved as a home number also if they don't change it, which is ok ).
                    if ( phoneNumber == null || string.IsNullOrWhiteSpace( phoneNumber.Number ) || phoneNumber.IsUnlisted )
                    {
                        phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ) );
                    }

                    if ( phoneNumber != null )
                    {
                        if ( !phoneNumber.IsUnlisted )
                        {
                            pnbPhone.CountryCode = phoneNumber.CountryCode;
                            pnbPhone.Number = phoneNumber.ToString();
                        }
                        else
                        {
                            DisplayPhone = false;
                        }
                    }
                    else
                    {
                        pnbPhone.CountryCode = PhoneNumber.DefaultCountryCode();
                        pnbPhone.Number = string.Empty;
                    }

                    cbSmsOptIn.Visible = DisplaySmsOptIn && DisplayPhone;
                    if ( DisplaySmsOptIn && DisplayPhone )
                    {
                        cbSmsOptIn.Text = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );
                        cbSmsOptIn.Checked = phoneNumber?.IsMessagingEnabled ?? false;
                    }
                }

                Guid addressTypeGuid = Guid.Empty;
                if ( !Guid.TryParse( GetAttributeValue( AttributeKey.AddressType ), out addressTypeGuid ) )
                {
                    addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                }

                var groupLocation = personService.GetFirstLocation( person.Id, DefinedValueCache.Get( addressTypeGuid ).Id );
                if ( groupLocation != null )
                {
                    GroupLocationId = groupLocation.Id;
                    acAddress.SetValues( groupLocation.Location );
                }
                else
                {
                    acAddress.SetValues( null );
                }
            }
            else
            {
                txtLastName.Text = string.Empty;
                txtFirstName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                pnbPhone.CountryCode = PhoneNumber.DefaultCountryCode();
                pnbPhone.Number = string.Empty;
                acAddress.SetValues( null );
            }
        }

        private void ShowBusiness()
        {
            int? businessId = cblBusiness.SelectedValueAsInt();
            if ( businessId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var business = personService.Get( businessId.Value );
                    ShowBusiness( personService, business );
                }
            }
            else
            {
                ShowBusiness( null, null );
            }
        }

        private void ShowBusiness( PersonService personService, Person business )
        {
            txtBusinessContactFirstName.Text = string.Empty;
            txtBusinessContactLastName.Text = string.Empty;
            pnbBusinessContactPhone.Text = string.Empty;
            txtBusinessContactEmail.Text = string.Empty;

            if ( personService == null && business == null )
            {
                txtBusinessName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                GroupLocationId = null;
                acAddress.SetValues( null );
                return;
            }

            txtBusinessName.Text = business.LastName;
            txtEmail.Text = business.Email;

            Guid addressTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
            var groupLocation = personService.GetFirstLocation( business.Id, DefinedValueCache.Get( addressTypeGuid ).Id );
            if ( groupLocation != null )
            {
                GroupLocationId = groupLocation.Id;
                acAddress.SetValues( groupLocation.Location );
            }
            else
            {
                GroupLocationId = null;
                acAddress.SetValues( null );
            }

            if ( DisplayPhone )
            {
                var workNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                // Try to retrieve a work number and update it if it exists
                var workPhone = business.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == workNumberTypeId );
                if ( workPhone != null )
                {
                    pnbPhone.CountryCode = workPhone.CountryCode;
                    pnbPhone.Number = workPhone.ToString();
                }

                if ( DisplaySmsOptIn )
                {
                    cbSmsOptIn.Visible = true;
                    cbSmsOptIn.Text = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );
                    cbSmsOptIn.Checked = workPhone?.IsMessagingEnabled ?? false;
                }
            }
            
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        private Person GetPerson( bool create )
        {
            Person person = null;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            Group familyGroup = null;

            int personId = ViewState[ViewStateKey.PersonId] as int? ?? 0;
            if ( personId == 0 && _targetPerson != null )
            {
                personId = _targetPerson.Id;
            }

            if ( personId != 0 )
            {
                person = personService.Get( personId );
            }

            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();
            bool givingAsBusiness = !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() && !tglGiveAsOption.Checked;
            if ( create && !givingAsBusiness )
            {
                // If this is a nameless person, we need to make a new person record and merge it.
                Person namelessPerson = null;
                if ( person != null && person.IsNameless() )
                {
                    namelessPerson = person;
                    person = null;
                }

                if ( person == null )
                {
                    // Check to see if there's only one person with same email, first name, and last name
                    if ( !string.IsNullOrWhiteSpace( txtEmail.Text ) && 
                        !string.IsNullOrWhiteSpace( txtFirstName.Text ) &&
                        !string.IsNullOrWhiteSpace( txtLastName.Text ) )
                    {
                        // Same logic as PledgeEntry.ascx.cs
                        var personQuery = new PersonService.PersonMatchQuery( txtFirstName.Text, txtLastName.Text, txtEmail.Text, pnbPhone.Text.Trim() );
                        person = personService.FindPerson( personQuery, true );
                    }

                    if ( person == null )
                    {
                        DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                        DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

                        // Create Person
                        person = new Person();
                        person.FirstName = txtFirstName.Text;
                        person.LastName = txtLastName.Text;
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        if ( dvcConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        }

                        if ( dvcRecordStatus != null )
                        {
                            person.RecordStatusValueId = dvcRecordStatus.Id;
                        }

                        // Create Person/Family
                        familyGroup = PersonService.SaveNewPerson( person, rockContext, null, false );
                    }

                    if ( namelessPerson != null )
                    {
                        personService.MergeNamelessPersonToExistingPerson( namelessPerson, person );
                    }

                    ViewState[ViewStateKey.PersonId] = person != null ? person.Id : 0;
                }
            }

            // Person should never be null at this point.
            if ( create && person != null )
            {
                person.Email = txtEmail.Text;

                if ( DisplayPhone )
                {
                    // Get the NumberType IDs
                    var homeNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
                    var mobileNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Id;

                    // Clean the entered data
                    var cleanCountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    var cleanPhoneNumber = PhoneNumber.CleanNumber( pnbPhone.Number );

                    // First try to retrieve a home number and update it if it exists
                    var homePhone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == homeNumberTypeId );
                    if ( homePhone != null )
                    {
                        homePhone.CountryCode = cleanCountryCode;
                        homePhone.Number = cleanPhoneNumber;

                        if ( DisplaySmsOptIn )
                        {
                            homePhone.IsMessagingEnabled = cbSmsOptIn.Checked;
                        }
                    }
                    else
                    {
                        // No home number so try to match the mobile number to the one entered
                        var mobilePhone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == mobileNumberTypeId && p.Number == cleanPhoneNumber );

                        // if there is not a match on the mobile number than create a new home number
                        if ( mobilePhone == null )
                        {
                            homePhone = new PhoneNumber
                            {
                                NumberTypeValueId = homeNumberTypeId,
                                CountryCode = cleanCountryCode,
                                Number = cleanPhoneNumber
                            };

                            if ( DisplaySmsOptIn )
                            {
                                homePhone.IsMessagingEnabled = cbSmsOptIn.Checked;
                            }

                            person.PhoneNumbers.Add( homePhone );
                        }
                        else
                        {
                            if ( DisplaySmsOptIn )
                            {
                                mobilePhone.IsMessagingEnabled = cbSmsOptIn.Checked;
                            }
                        }
                    }
                }

                if ( familyGroup == null )
                {
                    var groupLocationService = new GroupLocationService( rockContext );
                    if ( GroupLocationId.HasValue )
                    {
                        familyGroup = groupLocationService.Queryable()
                            .Where( gl => gl.Id == GroupLocationId.Value )
                            .Select( gl => gl.Group )
                            .FirstOrDefault();
                    }
                    else
                    {
                        familyGroup = person.GetFamily( rockContext );
                    }
                }

                rockContext.SaveChanges();

                if ( familyGroup != null )
                {
                    GroupService.AddNewGroupAddress(
                        rockContext,
                        familyGroup,
                        GetAttributeValue( AttributeKey.AddressType ),
                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country,
                        true);
                }
            }

            return person;
        }

        /// <summary>
        /// Creates the business contact.
        /// </summary>
        /// <returns></returns>
        private Person GetBusinessContact()
        {
            Person person = null;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            // Check to see if there's only one person with same email, first name, and last name
            if ( !string.IsNullOrWhiteSpace( txtBusinessContactEmail.Text ) &&
                !string.IsNullOrWhiteSpace( txtBusinessContactFirstName.Text ) &&
                !string.IsNullOrWhiteSpace( txtBusinessContactLastName.Text ) )
            {
                // Find matching person. Intentionally not updating their primary email address as in this rare case it is likely to be their
                // business email which is more likely that they don't want updated
                person = personService.FindPerson( txtBusinessContactFirstName.Text, txtBusinessContactLastName.Text, txtBusinessContactEmail.Text, false );
            }

            if ( person == null )
            {
                DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

                // Create Person
                person = new Person();
                person.FirstName = txtBusinessContactFirstName.Text;
                person.LastName = txtBusinessContactLastName.Text;
                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }

                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                // Create Person/Family
                PersonService.SaveNewPerson( person, rockContext, null, false );
            }

            // Person should never be null at this point.
            if ( person != null )
            {
                person.Email = txtBusinessContactEmail.Text;

                if ( DisplayPhone )
                {
                    // Get the NumberType IDs
                    var workNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                    var mobileNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Id;

                    // Clean the entered data
                    var cleanCountryCode = PhoneNumber.CleanNumber( pnbBusinessContactPhone.CountryCode );
                    var cleanPhoneNumber = PhoneNumber.CleanNumber( pnbBusinessContactPhone.Number );

                    // First try to retrieve a work number and update it if it exists
                    var workPhone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == workNumberTypeId );
                    if ( workPhone != null )
                    {
                        workPhone.CountryCode = cleanCountryCode;
                        workPhone.Number = cleanPhoneNumber;

                        if ( DisplaySmsOptIn )
                        {
                            workPhone.IsMessagingEnabled = cbBusinessContactSmsOptIn.Checked;
                        }
                    }
                    else
                    {
                        // No work number so try to match the mobile number to the one entered
                        var mobilePhone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == mobileNumberTypeId && p.Number == cleanPhoneNumber );

                        // if this isn't the mobile number than create a new work number
                        if ( mobilePhone == null )
                        {
                            workPhone = new PhoneNumber
                            {
                                NumberTypeValueId = workNumberTypeId,
                                CountryCode = cleanCountryCode,
                                Number = cleanPhoneNumber
                            };

                            if ( DisplaySmsOptIn )
                            {
                                workPhone.IsMessagingEnabled = cbBusinessContactSmsOptIn.Checked;
                            }

                            person.PhoneNumbers.Add( workPhone );
                        }
                        else
                        {
                            if ( DisplaySmsOptIn )
                            {
                                mobilePhone.IsMessagingEnabled = cbBusinessContactSmsOptIn.Checked;
                            }
                        }
                    }
                }

                rockContext.SaveChanges();
            }

            return person;
        }

        private Person GetPersonOrBusiness( Person person )
        {
            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();
            bool givingAsBusiness = !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() && !tglGiveAsOption.Checked;
            if ( person != null && givingAsBusiness )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                Group familyGroup = null;

                Person business = null;
                int? businessId = cblBusiness.SelectedValueAsInt();
                if ( businessId.HasValue )
                {
                    business = personService.Get( businessId.Value );
                }

                if ( business == null )
                {
                    // Try to find existing business for person that has the same name
                    var personBusinesses = person.GetBusinesses()
                        .Where( b => b.LastName == txtBusinessName.Text )
                        .ToList();
                    if ( personBusinesses.Count() == 1 )
                    {
                        business = personBusinesses.First();
                    }
                }

                if ( business == null )
                {
                    DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                    DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

                    // Create Person
                    business = new Person();
                    business.LastName = txtBusinessName.Text;
                    business.IsEmailActive = true;
                    business.EmailPreference = EmailPreference.EmailAllowed;
                    business.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                    if ( dvcConnectionStatus != null )
                    {
                        business.ConnectionStatusValueId = dvcConnectionStatus.Id;
                    }

                    if ( dvcRecordStatus != null )
                    {
                        business.RecordStatusValueId = dvcRecordStatus.Id;
                    }

                    // Create Person/Family
                    familyGroup = PersonService.SaveNewPerson( business, rockContext, null, false );

                    personService.AddContactToBusiness( business.Id, person.Id );
                    rockContext.SaveChanges();
                }

                business.LastName = txtBusinessName.Text;
                business.Email = txtEmail.Text;

                if ( DisplayPhone )
                {
                    // Get the NumberType IDs
                    var workNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                    var mobileNumberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Id;

                    // Clean the entered data
                    var cleanCountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    var cleanPhoneNumber = PhoneNumber.CleanNumber( pnbPhone.Number );

                    // First try to retrieve a work number and update it if it exists
                    var workPhone = business.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == workNumberTypeId );
                    if ( workPhone != null )
                    {
                        workPhone.CountryCode = cleanCountryCode;
                        workPhone.Number = cleanPhoneNumber;

                        if ( DisplaySmsOptIn )
                        {
                            workPhone.IsMessagingEnabled = cbSmsOptIn.Checked;
                        }
                    }
                    else
                    {
                        // No work number so try to match the mobile number to the one entered
                        var mobilePhone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == mobileNumberTypeId );

                        // If there is not match on the mobile number than create a new work number
                        if ( mobilePhone == null )
                        {

                            workPhone = new PhoneNumber
                            {
                                NumberTypeValueId = workNumberTypeId,
                                CountryCode = cleanCountryCode,
                                Number = cleanPhoneNumber
                            };

                            if ( DisplaySmsOptIn )
                            {
                                workPhone.IsMessagingEnabled = cbSmsOptIn.Checked;
                            }

                            business.PhoneNumbers.Add( workPhone );
                        }
                        else
                        {
                            if ( DisplaySmsOptIn )
                            {
                                mobilePhone.IsMessagingEnabled = cbSmsOptIn.Checked;
                            }
                        }
                    }
                }

                if ( familyGroup == null )
                {
                    var groupLocationService = new GroupLocationService( rockContext );
                    if ( GroupLocationId.HasValue )
                    {
                        familyGroup = groupLocationService.Queryable()
                            .Where( gl => gl.Id == GroupLocationId.Value )
                            .Select( gl => gl.Group )
                            .FirstOrDefault();
                    }
                    else
                    {
                        familyGroup = business.GetFamily( rockContext );
                    }
                }

                rockContext.SaveChanges();

                if ( familyGroup != null )
                {
                    GroupService.AddNewGroupAddress(
                        rockContext,
                        familyGroup,
                        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK,
                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country,
                        true );
                }

                return business;
            }

            return person;
        }

        /// <summary>
        /// Fetches the old (to be transferred) scheduled transaction and verifies
        /// that the target person is the same on the scheduled transaction.  Then
        /// it puts it into the _scheduledTransactionToBeTransferred private field
        /// for use throughout the entry process so that its values can be used on
        /// the form for the new transaction.
        /// </summary>
        /// <param name="scheduledTransactionId">The scheduled transaction identifier.</param>
        private void InitializeTransfer( Guid? scheduledTransactionGuid )
        {
            if ( scheduledTransactionGuid == null )
            {
                return;
            }

            RockContext rockContext = new RockContext();
            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( scheduledTransactionGuid.Value );
            var personService = new PersonService( rockContext );

            // get business giving id
            var givingIds = personService.GetBusinesses( _targetPerson.Id ).Select( g => g.GivingId ).ToList();

            // add the person's regular giving id
            givingIds.Add( _targetPerson.GivingId );

            // Make sure the current person is the authorized person, otherwise return
            if ( scheduledTransaction == null || !givingIds.Contains( scheduledTransaction.AuthorizedPersonAlias.Person.GivingId ) )
            {
                return;
            }

            _scheduledTransactionToBeTransferred = scheduledTransaction;

            // Set the frequency to be the same on the initial page build
            if ( !IsPostBack )
            {
                btnFrequency.SelectedValue = scheduledTransaction.TransactionFrequencyValueId.ToString();
                dtpStartDate.SelectedDate = ( scheduledTransaction.NextPaymentDate.HasValue ) ? scheduledTransaction.NextPaymentDate : RockDateTime.Today.AddDays( 1 );
            }
        }

        /// <summary>
        /// Validates the payment information.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ValidatePaymentInfo( out string errorMessage )
        {
            errorMessage = string.Empty;

            var errorMessages = new List<string>();

            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();
            bool givingAsBusiness = !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() && !tglGiveAsOption.Checked;

            if ( caapPromptForAccountAmounts.IsValidAmountSelected() )
            {
                // get the accountId(s) that have an amount specified
                var amountAccountIds = caapPromptForAccountAmounts.AccountAmounts
                    .Where( a => a.Amount.HasValue && a.Amount != 0.00M ).Select( a => a.AccountId )
                    .ToList();

                var accounts = new FinancialAccountService( new RockContext() ).GetByIds( amountAccountIds ).ToList();
                var amountSummaryMergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                amountSummaryMergeFields.Add( "Accounts", accounts );

                if ( caapPromptForAccountAmounts.CampusId.HasValue )
                {
                    amountSummaryMergeFields.Add( "Campus", CampusCache.Get( caapPromptForAccountAmounts.CampusId.Value ) );
                }
            }
            else
            {
                errorMessages.Add( "Please specify an amount" );
            }

            var amountLimit = this.PageParameter( PageParameterKey.AmountLimit ).AsDecimalOrNull();
            if ( amountLimit.HasValue )
            {
                if ( caapPromptForAccountAmounts.AccountAmounts.Sum( a => a.Amount ) > amountLimit.Value )
                {
                    errorMessages.Add( string.Format( "The maximum amount is limited to {0}", amountLimit.FormatAsCurrency() ) );
                }
            }

            // Get the payment schedule
            PaymentSchedule schedule = GetSchedule();

            if ( schedule != null )
            {
                // Make sure a repeating payment starts in the future
                var earliestScheduledStartDate = FinancialGatewayComponent.GetEarliestScheduledStartDate( FinancialGateway );
                if ( schedule.StartDate < earliestScheduledStartDate )
                {
                    errorMessages.Add( $"When scheduling a repeating payment, the minimum start date is {earliestScheduledStartDate.ToShortDateString()}" );
                }
            }
            else
            {
                if ( dtpStartDate.SelectedDate < RockDateTime.Today )
                {
                    errorMessages.Add( "Make sure the date is not in the past" );
                }
            }

            if ( txtFirstName.Visible == true )
            {
                if ( string.IsNullOrWhiteSpace( txtFirstName.Text ) || string.IsNullOrWhiteSpace( txtLastName.Text ) )
                {
                    errorMessages.Add( "Make sure to enter both a first and last name" );
                }

                if ( !txtFirstName.IsValid )
                {
                    errorMessages.Add( txtFirstName.CustomValidator.ErrorMessage );
                }
            }

            if ( givingAsBusiness && string.IsNullOrWhiteSpace( txtBusinessName.Text ) )
            {
                errorMessages.Add( "Make sure to enter a Business Name" );
            }

            var location = new Location();
            acAddress.GetValues( location );
            if ( string.IsNullOrWhiteSpace( location.Street1 ) )
            {
                errorMessages.Add( "Make sure to enter a valid address.  An address is required for us to process this transaction" );
            }

            if ( DisplayPhone && string.IsNullOrWhiteSpace( pnbPhone.Number ) )
            {
                errorMessages.Add( "Make sure to enter a valid phone number.  A phone number is required for us to process this transaction" );
            }

            bool displayEmail = GetAttributeValue( AttributeKey.DisplayEmail ).AsBoolean();
            if ( displayEmail && string.IsNullOrWhiteSpace( txtEmail.Text ) )
            {
                errorMessages.Add( "Make sure to enter a valid email address.  An email address is required for us to send you a payment confirmation" );
            }

            if ( givingAsBusiness && phBusinessContact.Visible )
            {
                if ( string.IsNullOrWhiteSpace( txtBusinessContactFirstName.Text ) || string.IsNullOrWhiteSpace( txtBusinessContactLastName.Text ) )
                {
                    errorMessages.Add( "Make sure to enter both a first and last name for Business Contact" );
                }

                if ( DisplayPhone && string.IsNullOrWhiteSpace( pnbBusinessContactPhone.Number ) )
                {
                    errorMessages.Add( "Make sure to enter a valid Business Contact phone number." );
                }

                if ( displayEmail && string.IsNullOrWhiteSpace( txtBusinessContactEmail.Text ) )
                {
                    errorMessages.Add( "Make sure to enter a valid Business Contact email address." );
                }
            }

            if ( errorMessages.Any() )
            {
                errorMessage = errorMessages.AsDelimited( "<br/>" );
                return false;
            }

            btnConfirmationPrev.Visible = true;

            return true;
        }

        private void SetConfirmationText( ReferencePaymentInfo paymentInfo )
        {
            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();
            bool givingAsBusiness = !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() && !tglGiveAsOption.Checked;

            if ( !givingAsBusiness )
            {
                if ( txtCurrentName.Visible )
                {
                    Person person = GetPerson( false );
                    if ( person != null )
                    {
                        paymentInfo.FirstName = person.FirstName;
                        paymentInfo.LastName = person.LastName;
                    }
                }
                else
                {
                    paymentInfo.FirstName = txtFirstName.Text;
                    paymentInfo.LastName = txtLastName.Text;
                }
            }
            else
            {
                paymentInfo.BusinessName = txtBusinessName.Text;
            }

            if ( givingAsBusiness )
            {
                tdNameConfirm.Description = paymentInfo.BusinessName.Trim();
            }
            else
            {
                tdNameConfirm.Description = paymentInfo.FullName.Trim();
            }

            tdPhoneConfirm.Description = paymentInfo.Phone;
            tdEmailConfirm.Description = paymentInfo.Email;
            tdAddressConfirm.Description = string.Format( "{0} {1}, {2} {3}", paymentInfo.Street1, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode );

            List<AccountItem> accountList = caapPromptForAccountAmounts.AccountAmounts
                .Where( a => a.Amount.HasValue )
                .Select( a =>
                    new AccountItem
                    {
                        Id = a.AccountId,
                        PublicName = FinancialAccountCache.Get( a.AccountId )?.PublicName,
                        AmountFormatted = a.Amount.FormatAsCurrency()
                    } )
                .ToList();

            rptAccountListConfirmation.DataSource = accountList;
            rptAccountListConfirmation.DataBind();

            tdTotalConfirm.Description = paymentInfo.Amount.FormatAsCurrency();
            tdPaymentMethodConfirm.Description = paymentInfo.CurrencyTypeValue?.Description;
            tdAccountNumberConfirm.Description = paymentInfo.MaskedNumber;
            tdAccountNumberConfirm.Visible = !string.IsNullOrWhiteSpace( paymentInfo.MaskedNumber );

            PaymentSchedule schedule = GetSchedule();
            tdWhenConfirm.Description = schedule != null ? schedule.ToString() : "Today";
        }

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        private ReferencePaymentInfo GetPaymentInfo( out string errorMessage )
        {
            errorMessage = null;

            var isSavedAccount = ( rblSavedAccount.Items.Count > 0 && ( rblSavedAccount.SelectedValueAsId() ?? 0 ) > 0 );

            var paymentInfo = ( isSavedAccount ) ? GetReferenceInfo( rblSavedAccount.SelectedValueAsId().Value ) : new ReferencePaymentInfo();

            paymentInfo.Amount = caapPromptForAccountAmounts.AccountAmounts.Where( a => a.Amount.HasValue ).Sum( a => a.Amount.Value );
            paymentInfo.Email = txtEmail.Text;
            paymentInfo.Phone = PhoneNumber.FormattedNumber( pnbPhone.CountryCode, pnbPhone.Number, true );
            paymentInfo.Street1 = acAddress.Street1;
            paymentInfo.Street2 = acAddress.Street2;
            paymentInfo.City = acAddress.City;
            paymentInfo.State = acAddress.State;
            paymentInfo.PostalCode = acAddress.PostalCode;
            paymentInfo.Country = acAddress.Country;

            var transactionType = DefinedValueCache.Get( this.GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            paymentInfo.TransactionTypeValueId = transactionType.Id;

            if ( !isSavedAccount )
            {
                // If this is not a saved account, the Gateway may alter the data in paymentInfo (e.g., to use a separate billing address, if the gateway's hosted payment control permits this).
                var financialGatewayComponent = this.FinancialGatewayComponent;
                financialGatewayComponent.UpdatePaymentInfoFromPaymentControl( this.FinancialGateway, _hostedPaymentInfoControl, paymentInfo, out errorMessage );
            }

            return paymentInfo;
        }

        /// <summary>
        /// Gets the reference information.
        /// </summary>
        /// <param name="savedAccountId">The saved account unique identifier.</param>
        private ReferencePaymentInfo GetReferenceInfo( int savedAccountId )
        {
            var savedAccount = new FinancialPersonSavedAccountService( new RockContext() ).Get( savedAccountId );
            if ( savedAccount != null )
            {
                return savedAccount.GetReferencePayment();
            }

            return null;
        }

        /// <summary>
        /// Gets the payment schedule.
        /// </summary>
        private PaymentSchedule GetSchedule()
        {
            // Figure out if this is a one-time transaction or a future scheduled transaction
            if ( GetAttributeValue( AttributeKey.AllowScheduled ).AsBoolean() )
            {
                // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
                int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() && dtpStartDate.SelectedDate <= RockDateTime.Today )
                {
                    // one-time immediate payment
                    return null;
                }

                var schedule = new PaymentSchedule();
                schedule.TransactionFrequencyValue = DefinedValueCache.Get( btnFrequency.SelectedValueAsId().Value );
                if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > RockDateTime.Today )
                {
                    schedule.StartDate = dtpStartDate.SelectedDate.Value;
                }
                else
                {
                    schedule.StartDate = DateTime.MinValue;
                }

                return schedule;
            }

            return null;
        }

        #endregion

        #region Methods for the confirmation Page (panel)

        /// <summary>
        /// Processes the transaction.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private bool ProcessTransaction( out string errorMessage )
        {
            var rockContext = new RockContext();
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                var transactionGuid = hfTransactionGuid.Value.AsGuid();

                var gateway = this.FinancialGatewayComponent;
                var financialGateway = this.FinancialGateway;

                if ( gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();
                bool givingAsBusiness = !enableTextToGiveSetup && GetAttributeValue( AttributeKey.EnableBusinessGiving ).AsBoolean() && !tglGiveAsOption.Checked;

                // only create/update the person if they are giving as a person. If they are giving as a Business, the person shouldn't be created this way
                Person person = GetPerson( !givingAsBusiness );

                // Add contact person if giving as a business and current person is unknown
                if ( person == null && givingAsBusiness )
                {
                    person = GetBusinessContact();
                }

                if ( person == null )
                {
                    errorMessage = "There was a problem creating the person information";
                    return false;
                }

                if ( !person.PrimaryAliasId.HasValue )
                {
                    errorMessage = "There was a problem creating the person's primary alias";
                    return false;
                }

                Person BusinessOrPerson = GetPersonOrBusiness( person );

                var paymentInfo = GetTxnPaymentInfo( BusinessOrPerson, out errorMessage );
                if ( paymentInfo == null )
                {
                    return false;
                }

                PaymentSchedule schedule = GetSchedule();
                FinancialPaymentDetail paymentDetail = null;
                if ( schedule != null )
                {
                    schedule.PersonId = person.Id;

                    var scheduledTransactionAlreadyExists = new FinancialScheduledTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                    if ( scheduledTransactionAlreadyExists != null )
                    {
                        // Hopefully shouldn't happen, but just in case the scheduledtransaction already went through, show the success screen.
                        ShowSuccess( gateway, person, paymentInfo, givingAsBusiness );
                        return true;
                    }

                    var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                    if ( scheduledTransaction == null )
                    {
                        return false;
                    }

                    // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate scheduled transactions impossible
                    scheduledTransaction.Guid = transactionGuid;

                    SaveScheduledTransaction( financialGateway, gateway, BusinessOrPerson, paymentInfo, schedule, scheduledTransaction, rockContext );
                    paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                }
                else
                {
                    var transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                    if ( transactionAlreadyExists != null )
                    {
                        // hopefully shouldn't happen, but just in case the transaction already went thru, show the success screen
                        ShowSuccess( gateway, person, paymentInfo, givingAsBusiness );
                        return true;
                    }

                    var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                    if ( transaction == null )
                    {
                        return false;
                    }

                    // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
                    transaction.Guid = transactionGuid;

                    SaveTransaction( financialGateway, gateway, BusinessOrPerson, paymentInfo, transaction, rockContext );
                    paymentDetail = transaction.FinancialPaymentDetail.Clone( false );
                }

                ShowSuccess( gateway, person, paymentInfo, givingAsBusiness );

                return true;
            }
            else
            {
                pnlDupWarning.Visible = true;
                errorMessage = string.Empty;
                return false;
            }
        }

        private ReferencePaymentInfo GetTxnPaymentInfo( Person person, out string errorMessage )
        {
            errorMessage = null;

            ReferencePaymentInfo paymentInfo = GetPaymentInfo( out errorMessage );
            if ( !string.IsNullOrEmpty( errorMessage ) )
            {
                return null;
            }
            else if ( paymentInfo == null )
            {
                errorMessage = "There was a problem creating the payment information";
                return null;
            }
            else
            {
                paymentInfo.FirstName = person.FirstName;
                paymentInfo.LastName = person.LastName;
            }

            if ( paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var financialGatewayComponent = this.FinancialGatewayComponent;
                financialGatewayComponent.UpdatePaymentInfoFromPaymentControl( this.FinancialGateway, _hostedPaymentInfoControl, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    ShowMessage( NotificationBoxType.Danger, "", errorMessage ?? "Unknown Error" );
                    return null;
                }

                var customerToken = financialGatewayComponent.CreateCustomerAccount( this.FinancialGateway, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    ShowMessage( NotificationBoxType.Danger, "", errorMessage ?? "Unknown Error" );
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;
            }

            SetPaymentComment( paymentInfo, txtCommentEntry.Text );

            errorMessage = string.Empty;
            return paymentInfo;
        }

        private void SaveScheduledTransaction( FinancialGateway financialGateway, IHostedGatewayComponent gateway, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, RockContext rockContext )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            var transactionType = DefinedValueCache.Get( this.GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            scheduledTransaction.TransactionTypeValueId = transactionType.Id;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.Source ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    scheduledTransaction.SourceTypeValueId = source.Id;
                }
            }

            var transactionEntity = this.GetTransactionEntity();

            PopulateTransactionDetails( scheduledTransaction.ScheduledTransactionDetails );

            scheduledTransaction.Summary = paymentInfo.Comment1;

            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();

            // If this is a transfer, now we can delete the old transaction
            if ( _scheduledTransactionToBeTransferred != null )
            {
                DeleteOldTransaction( _scheduledTransactionToBeTransferred.Id );
            }

            rockContext.SaveChanges();

            ScheduleId = scheduledTransaction.Id;
            TransactionCode = scheduledTransaction.TransactionCode;

            Task.Run( () => ScheduledGiftWasModifiedMessage.PublishScheduledTransactionEvent( scheduledTransaction.Id, ScheduledGiftEventTypes.ScheduledGiftCreated ) );
        }

        private void DeleteOldTransaction( int scheduledTransactionId )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                FinancialScheduledTransactionService fstService = new FinancialScheduledTransactionService( rockContext );
                var currentTransaction = fstService.Get( scheduledTransactionId );
                if ( currentTransaction != null && currentTransaction.FinancialGateway != null )
                {
                    currentTransaction.FinancialGateway.LoadAttributes( rockContext );
                }

                string errorMessage = string.Empty;
                if ( fstService.Cancel( currentTransaction, out errorMessage ) )
                {
                    try
                    {
                        fstService.GetStatus( currentTransaction, out errorMessage );
                    }
                    catch
                    {
                    }

                    rockContext.SaveChanges();
                }
            }
        }

        private void SaveTransaction( FinancialGateway financialGateway, IHostedGatewayComponent gateway, Person person, PaymentInfo paymentInfo, FinancialTransaction transaction, RockContext rockContext )
        {
            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            transaction.ShowAsAnonymous = cbGiveAnonymously.Checked;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;

            var transactionType = DefinedValueCache.Get( this.GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = transactionType.Id;

            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.Source ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                }
            }

            var transactionEntity = this.GetTransactionEntity();

            PopulateTransactionDetails( transaction.TransactionDetails );

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.GetForNewTransaction( transaction, GetAttributeValue( AttributeKey.BatchNamePrefix ) );

            var batchChanges = new History.HistoryChangeList();
            FinancialBatchService.EvaluateNewBatchHistory( batch, batchChanges );

            transaction.LoadAttributes( rockContext );

            var allowedTransactionAttributes = GetAttributeValue( AttributeKey.AllowedTransactionAttributesFromURL ).Split( ',' ).AsGuidList().Select( x => AttributeCache.Get( x ).Key );

            foreach ( KeyValuePair<string, AttributeValueCache> attr in transaction.AttributeValues )
            {
                if ( PageParameters().ContainsKey( PageParameterKey.AttributePrefix + attr.Key ) && allowedTransactionAttributes.Contains( attr.Key ) )
                {
                    attr.Value.Value = Server.UrlDecode( PageParameter( PageParameterKey.AttributePrefix + attr.Key ) );
                }
            }

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

            Task.Run( () => GiftWasGivenMessage.PublishTransactionEvent( transaction.Id, GiftEventTypes.GiftSuccess ) );

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges
            );

            SendReceipt( transaction.Id );

            TransactionCode = transaction.TransactionCode;
        }

        /// <summary>
        /// Populates the transaction details for a FinancialTransaction or ScheduledFinancialTransaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transactionDetails">The transaction details.</param>
        private void PopulateTransactionDetails<T>( ICollection<T> transactionDetails ) where T : ITransactionDetail, new()
        {
            var transactionEntity = this.GetTransactionEntity();
            var selectedAccountAmounts = caapPromptForAccountAmounts.AccountAmounts.Where( a => a.Amount.HasValue && a.Amount != 0 ).ToArray();

            var totalSelectedAmounts = selectedAccountAmounts.Sum( a => a.Amount.Value );

            foreach ( var selectedAccountAmount in selectedAccountAmounts )
            {
                var transactionDetail = new T();

                transactionDetail.AccountId = selectedAccountAmount.AccountId;
                transactionDetail.Amount = selectedAccountAmount.Amount.Value;

                if ( transactionEntity != null )
                {
                    transactionDetail.EntityTypeId = transactionEntity.TypeId;
                    transactionDetail.EntityId = transactionEntity.Id;
                }

                transactionDetails.Add( transactionDetail );
            }
        }

        private void ShowSuccess( IHostedGatewayComponent gatewayComponent, Person person, ReferencePaymentInfo paymentInfo, bool givingAsBusiness )
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions() );
            var finishLavaTemplate = this.GetAttributeValue( AttributeKey.FinishLavaTemplate );
            IEntity transactionEntity = GetTransactionEntity();
            mergeFields.Add( "TransactionEntity", transactionEntity );

            var transactionGuid = hfTransactionGuid.Value.AsGuid();

            var rockContext = new RockContext();

            // the transactionGuid is either for a FinancialTransaction or a FinancialScheduledTransaction
            int? financialPaymentDetailId;
            FinancialPaymentDetail financialPaymentDetail;
            FinancialTransaction financialTransaction = new FinancialTransactionService( rockContext ).Get( transactionGuid );
            int? transactionPersonAliasId;
            int? textToGiveContributionAccountId = null;
            if ( financialTransaction != null )
            {
                textToGiveContributionAccountId = financialTransaction.TransactionDetails.FirstOrDefault()?.AccountId;
                mergeFields.Add( "Transaction", financialTransaction );
                transactionPersonAliasId = financialTransaction.AuthorizedPersonAliasId;
                financialPaymentDetail = financialTransaction.FinancialPaymentDetail;
                financialPaymentDetailId = financialTransaction.FinancialGatewayId;
            }
            else
            {
                FinancialScheduledTransaction financialScheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( transactionGuid );
                textToGiveContributionAccountId = financialScheduledTransaction?.ScheduledTransactionDetails.FirstOrDefault()?.AccountId;
                mergeFields.Add( "Transaction", financialScheduledTransaction );
                transactionPersonAliasId = financialScheduledTransaction.AuthorizedPersonAliasId;
                financialPaymentDetail = financialScheduledTransaction.FinancialPaymentDetail;
                financialPaymentDetailId = financialScheduledTransaction.FinancialGatewayId;
            }

            if ( transactionPersonAliasId.HasValue )
            {
                var transactionPerson = new PersonAliasService( rockContext ).GetPerson( transactionPersonAliasId.Value );
                mergeFields.Add( "Person", transactionPerson );
            }

            if ( financialPaymentDetail != null || financialPaymentDetailId.HasValue )
            {
                financialPaymentDetail = financialPaymentDetail ?? new FinancialPaymentDetailService( rockContext ).GetNoTracking( financialPaymentDetailId.Value );
                mergeFields.Add( "PaymentDetail", financialPaymentDetail );

                if ( financialPaymentDetail.BillingLocation != null || financialPaymentDetail.BillingLocationId.HasValue )
                {
                    var billingLocation = financialPaymentDetail.BillingLocation ?? new LocationService( rockContext ).GetNoTracking( financialPaymentDetail.BillingLocationId.Value );
                    mergeFields.Add( "BillingLocation", billingLocation );
                }
            }

            lTransactionSummaryHTML.Text = finishLavaTemplate.ResolveMergeFields( mergeFields );

            var savedAccountId = rblSavedAccount.SelectedValue.AsInteger();
            bool isSavedAccount = savedAccountId > 0;
            var enableTextToGiveSetup = GetAttributeValue( AttributeKey.EnableTextToGiveSetup ).AsBoolean();

            // If there was a transaction code returned and this was not already created from a previous saved account,
            // show the option to save the account.
            if ( enableTextToGiveSetup && !string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                if ( isSavedAccount )
                {
                    new PersonService( rockContext ).ConfigureTextToGive( person.Id, textToGiveContributionAccountId, savedAccountId, out _ );
                    rockContext.SaveChanges();
                }
                else
                {
                    var accountTitle = $"Text-To-Give";

                    var currencyTypeId = financialPaymentDetail?.CurrencyTypeValueId;
                    var creditCardCurrencyTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
                    var achCurrencyTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );

                    if ( currencyTypeId == creditCardCurrencyTypeId )
                    {
                        string cardType = financialPaymentDetail?.CreditCardTypeValue?.Value;
                        string accountNumber = financialPaymentDetail?.AccountNumberMasked;
                        string last4 = accountNumber.Right( 4 );
                        accountTitle = $"Text-To-Give - {cardType} (ending in {last4})";
                    }
                    else if ( currencyTypeId == achCurrencyTypeId )
                    {
                        string accountNumber = financialPaymentDetail?.AccountNumberMasked;
                        string last4 = accountNumber.Right( 4 );
                        accountTitle = $"Text-To-Give - ACH (ending in {last4})";
                    }
                    else if ( financialPaymentDetail?.CurrencyTypeValue != null )
                    {
                        accountTitle = $"Text-To-Give - {financialPaymentDetail.CurrencyTypeValue.Value}";
                    }

                    CreateSavedAccount( accountTitle, rockContext, true );
                }
            }
            else if ( !givingAsBusiness && !isSavedAccount && !string.IsNullOrWhiteSpace( TransactionCode ) && gatewayComponent.SupportsSavedAccount( paymentInfo.CurrencyTypeValue ) )
            {
                cbSaveAccount.Visible = true;
                pnlSaveAccount.Visible = true;
                txtSaveAccount.Visible = true;

                // If current person does not have a login, have them create a username and password
                phCreateLogin.Visible = !new UserLoginService( rockContext ).GetByPersonId( person.Id ).Any();
            }
            else
            {
                pnlSaveAccount.Visible = false;
            }

            // the merge fields for the header/footer includes the financialTransaction, so update them now that we have saved the transaction to the database
            ResolveHeaderFooterTemplates();
        }

        private void SendReceipt( int transactionId )
        {
            Guid? receiptEmail = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();
            if ( receiptEmail.HasValue )
            {
                // Queue a bus message to send receipts
                var sendPaymentReceiptsTask = new ProcessSendPaymentReceiptEmails.Message
                {
                    SystemEmailGuid = receiptEmail.Value,
                    TransactionId = transactionId
                };

                sendPaymentReceiptsTask.Send();
            }
        }

        private void CreateSavedAccount( string AccountTitle, RockContext rockContext, bool enableTextToGiveSetup = false )
        {
            var gateway = this.FinancialGatewayComponent;
            var financialGateway = this.FinancialGateway;

            if ( gateway == null )
            {
                nbSaveAccount.Title = "Invalid Gateway";
                nbSaveAccount.Text = "Sorry, the financial gateway information for this type of transaction is not valid.";
                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                nbSaveAccount.Visible = true;
                return;
            }

            string errorMessage = string.Empty;

            var person = GetPerson( false );
            string referenceNumber = string.Empty;
            FinancialPaymentDetail paymentDetail = null;
            int? textToGiveContributionAccountId = null;

            if ( !ScheduleId.HasValue )
            {
                var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( ( financialGateway != null ? financialGateway.Id : ( int? ) null ), TransactionCode );
                if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                {
                    if ( transaction.FinancialGateway != null )
                    {
                        transaction.FinancialGateway.LoadAttributes( rockContext );
                    }

                    referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                    paymentDetail = transaction.FinancialPaymentDetail;
                    textToGiveContributionAccountId = transaction.TransactionDetails.FirstOrDefault().AccountId;
                }
            }
            else
            {
                var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( ScheduleId.Value );
                if ( scheduledTransaction != null )
                {
                    if ( scheduledTransaction.FinancialGateway != null )
                    {
                        scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                    }

                    referenceNumber = gateway.GetReferenceNumber( scheduledTransaction, out errorMessage );
                    paymentDetail = scheduledTransaction.FinancialPaymentDetail;
                    textToGiveContributionAccountId = scheduledTransaction.ScheduledTransactionDetails.FirstOrDefault().AccountId;
                }
            }

            if ( person == null || paymentDetail == null )
            {
                nbSaveAccount.Title = "Invalid Transaction";
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference.";
                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                nbSaveAccount.Visible = true;
                return;
            }

            if ( errorMessage.Any() )
            {
                nbSaveAccount.Title = "Invalid Transaction";
                nbSaveAccount.Text = "Sorry, the account information cannot be saved. " + errorMessage;
                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                nbSaveAccount.Visible = true;
                return;
            }

            if ( phCreateLogin.Visible )
            {
                var user = UserLoginService.Create(
                    rockContext,
                    person,
                    Rock.Model.AuthenticationServiceType.Internal,
                    EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                    txtUserName.Text,
                    txtPassword.Text,
                    false );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "User", user );

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKey.ConfirmAccountTemplate ).AsGuid() );
                emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                emailMessage.CreateCommunicationRecord = false;
                emailMessage.Send();
            }

            var savedAccount = new FinancialPersonSavedAccount();
            savedAccount.PersonAliasId = person.PrimaryAliasId;
            savedAccount.ReferenceNumber = referenceNumber;
            savedAccount.Name = AccountTitle;
            savedAccount.TransactionCode = TransactionCode;
            savedAccount.GatewayPersonIdentifier = paymentDetail.GatewayPersonIdentifier;
            savedAccount.FinancialGatewayId = financialGateway.Id;
            savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
            savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
            savedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
            savedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
            savedAccount.FinancialPaymentDetail.NameOnCard = paymentDetail.NameOnCard;
            savedAccount.FinancialPaymentDetail.ExpirationMonth = paymentDetail.ExpirationMonth;
            savedAccount.FinancialPaymentDetail.ExpirationYear = paymentDetail.ExpirationYear;
            savedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

            var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
            savedAccountService.Add( savedAccount );
            rockContext.SaveChanges();

            // If we created a new saved account, update the transaction to say it that is used this saved account.
            paymentDetail.FinancialPersonSavedAccountId = savedAccount.Id;
            rockContext.SaveChanges();

            if ( enableTextToGiveSetup )
            {
                new PersonService( rockContext ).ConfigureTextToGive( person.Id, textToGiveContributionAccountId, savedAccount.Id, out errorMessage );
                rockContext.SaveChanges();
            }

            cbSaveAccount.Visible = false;
            txtSaveAccount.Visible = false;
            phCreateLogin.Visible = false;
            divSaveActions.Visible = false;

            nbSaveAccount.Title = "Success";
            nbSaveAccount.Text = "The account has been saved for future use";
            nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
            nbSaveAccount.Visible = true;
        }

        #endregion

        #region Methods used globally

        /// <summary>
        /// Sets the page.
        /// </summary>
        /// <param name="page">The page.</param>
        private void SetPage( EntryStep page )
        {
            pnlSelection.Visible = page == EntryStep.PromptForAmount;
            pnlContributionInfo.Visible = page == EntryStep.PromptForAmount;

            // only show the History back button if the previous URL was able to be determined and they have the EnableInitialBackbutton enabled;
            lHistoryBackButton.Visible = GetAttributeValue( AttributeKey.EnableInitialBackbutton ).AsBoolean() && lHistoryBackButton.HRef != "#" && page == EntryStep.PromptForAmount;

            pnlConfirmation.Visible = page == EntryStep.ShowConfirmation;
            pnlSuccess.Visible = page == EntryStep.ShowTransactionSummary;

            hfCurrentPage.Value = page.ConvertToString(false);
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        private void ShowMessage( NotificationBoxType type, string title, string text )
        {
            if ( !string.IsNullOrWhiteSpace( text ) )
            {
                NotificationBox nb = nbMessage;
                var entryStep = hfCurrentPage.Value.ConvertToEnumOrNull<EntryStep>() ?? EntryStep.PromptForAmount;
                switch ( entryStep )
                {
                    case EntryStep.PromptForAmount:
                        nb = nbSelectionMessage;
                        break;
                    case EntryStep.ShowConfirmation:
                        nb = nbConfirmationMessage;
                        break;
                    case EntryStep.ShowTransactionSummary:
                        nb = nbSuccessMessage;
                        break;
                }

                nb.Text = text;
                nb.Title = string.IsNullOrWhiteSpace( title ) ? "" : string.Format( "<p>{0}</p>", title );
                nb.NotificationBoxType = type;
                nb.Visible = true;
            }
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterScript()
        {

            var currencyCodeInfo = new RockCurrencyCodeInfo();

            string script = $@"
    Sys.Application.add_load(function () {{
        // As amounts are entered, validate that they are numeric and recalc total
        $('.account-amount').on('change', function() {{
            var totalAmt = Number(0);

            $('.account-amount .form-control').each(function (index) {{
                var itemValue = $(this).val();
                if (itemValue != null && itemValue != '') {{
                    if (isNaN(itemValue)) {{
                        $(this).parents('div.input-group').addClass('has-error');
                    }}
                    else {{
                        $(this).parents('div.input-group').removeClass('has-error');
                        var num = Number(itemValue);
                        $(this).val(num.toFixed({currencyCodeInfo.DecimalPlaces}));
                        totalAmt = totalAmt + num;
                    }}
                }}
                else {{
                    $(this).parents('div.input-group').removeClass('has-error');
                }}
            }});
            $('.total-amount').html('{currencyCodeInfo.Symbol}' + totalAmt.toLocaleString(undefined, {{ minimumFractionDigits: {currencyCodeInfo.DecimalPlaces}, maximumFractionDigits: {currencyCodeInfo.DecimalPlaces} }}));
            return false;
        }});

        // Hide or show a div based on selection of checkbox (Saved Account)
        $('input:checkbox.toggle-input').unbind('click').on('click', function () {{
            $(this).parents('.checkbox').next('.toggle-content').slideToggle();
        }});
    }});
";
            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
        }

        private void SetPaymentComment( PaymentInfo paymentInfo, string userComment )
        {
            // Create a payment comment using the Lava template specified in this block.
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "TransactionDateTime", RockDateTime.Now );

            if ( paymentInfo != null )
            {
                mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );
            }

            var accountDetails = caapPromptForAccountAmounts.AccountAmounts.Where( a => a.Amount.HasValue && a.Amount.Value != 0 )
                .Select( a => new TransactionAccountDetail( a.AccountId, a.Amount ?? 0.0M, caapPromptForAccountAmounts.CampusId ) )
                .ToList();

            mergeFields.Add( "TransactionAccountDetails", accountDetails );

            var paymentComment = GetAttributeValue( AttributeKey.PaymentCommentTemplate ).ResolveMergeFields( mergeFields );

            if ( GetAttributeValue( AttributeKey.EnableCommentEntry ).AsBoolean() )
            {
                if ( paymentComment.IsNotNullOrWhiteSpace() )
                {
                    // Append user comments to the block-specified payment comment.
                    paymentInfo.Comment1 = string.Format( "{0}: {1}", paymentComment, userComment );
                }
                else
                {
                    paymentInfo.Comment1 = userComment;
                }
            }
            else
            {
                paymentInfo.Comment1 = paymentComment;
            }
        }

        /// <summary>
        /// Sets the comment field for a payment, incorporating the Lava template specified in the block settings if appropriate.
        /// </summary>
        /// <param name="paymentInfo"></param>
        /// <param name="userComment"></param>
        private void SetPaymentComment( PaymentInfo paymentInfo, List<FinancialTransactionDetail> commentTransactionAccountDetails, string userComment )
        {
            // Create a payment comment using the Lava template specified in this block.
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "TransactionDateTime", RockDateTime.Now );

            if ( paymentInfo != null )
            {
                mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );
            }

            mergeFields.Add( "TransactionAccountDetails", commentTransactionAccountDetails.Where( a => a.Amount != 0 ).ToList() );

            var paymentComment = GetAttributeValue( AttributeKey.PaymentCommentTemplate ).ResolveMergeFields( mergeFields );

            if ( GetAttributeValue( AttributeKey.EnableCommentEntry ).AsBoolean() )
            {
                if ( paymentComment.IsNotNullOrWhiteSpace() )
                {
                    // Append user comments to the block-specified payment comment.
                    paymentInfo.Comment1 = string.Format( "{0}: {1}", paymentComment, userComment );
                }
                else
                {
                    paymentInfo.Comment1 = userComment;
                }
            }
            else
            {
                paymentInfo.Comment1 = paymentComment;
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Lightweight object for each contribution item
        /// </summary>
        protected class AccountItem
        {
            public int Id { get; set; }

            public string PublicName { get; set; }

            public string AmountFormatted { get; set; }

            public int? ParentAccountId { get; set; }

            public bool HasChildren { get; set; }

            public bool IsRootItem { get; set; }

            public List<AccountItem> Children { get; set; } = new List<AccountItem>();

            internal void UpdateChildItems( AccountItem accountItem, List<AccountItem> availableAccounts )
            {
                Children = Children.Where( c => c.Id != accountItem.Id ).ToList();
                HasChildren = Children.Any();
                IsRootItem = !ParentAccountId.HasValue && !accountItem.HasChildren;

                // If the account no longer has any children to display but is itself a child item then
                // try and add it to its parent account if the parent account is still part of the Available accounts.
                // This is to update the hierarchy on the UI.
                if ( ParentAccountId.HasValue && !HasChildren )
                {
                    var parent = availableAccounts.FirstOrDefault( m => m.Id == ParentAccountId && !Children.Any( c => c.Id == Id ) );
                    if ( parent != null )
                    {
                        parent.Children.Add( this );
                        parent.HasChildren = parent.Children.Any();
                    }
                }
            }
        }

        /// <summary>
        /// This POCO is intended to provide backwards compatibility to RockWeb.Blocks.Financial.TransactionEntry.AccountItem,
        /// which is used in parsing the Payment Comment Lava Template.  This is necessary so that Lava templates from that
        /// block can be used on this block.
        /// </summary>
        protected class TransactionAccountDetail : RockDynamic
        {
            public int Id { get; set; }

            public int Order { get; set; }

            public string Name { get; set; }

            public int? CampusId { get; set; }

            public decimal Amount { get; set; }

            public bool Enabled { get; set; }

            public string PublicName { get; set; }

            public string AmountFormatted
            {
                get
                {
                    return this.Amount > 0 ? this.Amount.FormatAsCurrency() : string.Empty;
                }
            }

            public TransactionAccountDetail( int accountId, decimal amount, int? campusId )
            {
                this.Id = accountId;
                this.Amount = amount;
                this.CampusId = campusId;

                var account = FinancialAccountCache.Get( this.Id );
                if ( account == null )
                {
                    return;
                }

                this.PublicName = account.PublicName;
                this.Order = account.Order;
                this.Name = account.Name;

                // this was used for tracking whether accounts passed in to the query string were enabled in the TransactionEntry block.
                // We (probably?) don't need it here, but it's being kept for backwards compatibilty.
                this.Enabled = true;
            }
        }

        /// <summary>
        /// Helper object for account options passed via the request string using <see cref="AttributeKey.AllowAccountOptionsInURL"/>
        /// </summary>
        private class ParameterAccountOption
        {
            public int AccountId { get; set; }

            public decimal? Amount { get; set; }

            public bool Enabled { get; set; }
        }

        #endregion
    }
}
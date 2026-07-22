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
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Bus.Message;
using Rock.ClientService.Finance.FinancialPersonSavedAccount;
using Rock.ClientService.Finance.FinancialPersonSavedAccount.Options;
using Rock.Constants;
using Rock.Crm.RecordSource;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Finance;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    ///Creates a new financial transaction or scheduled transaction.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Utility Payment Entry" )]
    [Category( "Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]
    [IconCssClass( "ti ti-cash" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    #region Basic Settings > General Settings

    [FinancialGatewayField( "Financial Gateway",
        Key = AttributeKey.FinancialGateway,
        Description = "The payment gateway for credit card and ACH transactions.",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 0,
        IsRequired = true )]

    [BooleanField( "Enable ACH",
        Key = AttributeKey.EnableACH,
        Description = "Whether ACH bank account payments are accepted.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Enable Credit Card",
        Key = AttributeKey.EnableCreditCard,
        Description = "Whether credit card payments are accepted.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 2,
        IsRequired = false )]

    [TextField( "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The prefix applied to new batch names created by this block.",
        DefaultValue = "Online Giving",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 3,
        IsRequired = false )]

    [DefinedValueField( "Transaction Source",
        Key = AttributeKey.TransactionSource,
        Description = "The financial source type applied to transactions created by this block.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        AllowMultiple = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 4,
        IsRequired = false )]

    [BooleanField( "Prompt for Campus When Known",
        Key = AttributeKey.PromptForCampusWhenKnown,
        Description = "Whether to prompt for campus even when the person's campus is already known.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 5,
        IsRequired = false )]

    [BooleanField( "Include Inactive Campuses",
        Key = AttributeKey.IncludeInactiveCampuses,
        Description = "Whether inactive campuses are included in the campus list.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 6,
        IsRequired = false )]

    [DefinedValueField( "Campus Type Filter",
        Key = AttributeKey.CampusTypeFilter,
        Description = "Limits the campus list to the selected campus types.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 7,
        IsRequired = false )]

    [DefinedValueField( "Campus Status Filter",
        Key = AttributeKey.CampusStatusFilter,
        Description = "Limits the campus list to the selected campus statuses.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 8,
        IsRequired = false )]

    [BooleanField( "Allow Multiple Accounts",
        Key = AttributeKey.AllowMultipleAccounts,
        Description = "Whether the giver can split their gift across multiple accounts.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 9,
        IsRequired = false )]

    [CustomDropdownListField( "Layout Style",
        Key = AttributeKey.LayoutStyle,
        Description = "Controls whether the block's sections are stacked vertically or displayed in a fluid layout.",
        ListSource = "Vertical,Fluid",
        DefaultValue = "Vertical",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 10,
        IsRequired = false )]

    [AccountsField( "Accounts to Display",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts shown to the giver. When campus mapping is enabled, a matching child account for the selected campus will be used in place of the parent.",
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 11,
        IsRequired = false )]

    [BooleanField( "Allow Additional Accounts",
        Key = AttributeKey.AllowAdditionalAccounts,
        Description = "Whether givers can add accounts beyond the configured list. Any active, publicly named account will be available.",
        TrueText = "Display option for selecting additional accounts",
        FalseText = "Don't display option",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 12,
        IsRequired = false )]

    [BooleanField( "Group Additional Accounts by Hierarchy",
        Key = AttributeKey.GroupAdditionalAccountsByHierarchy,
        Description = "When additional accounts are enabled, groups them under their parent accounts. Note: campus-mapped accounts still appear in the hierarchy when campus mapping is on.",
        TrueText = "Enable",
        FalseText = "Disable",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 13,
        IsRequired = false )]

    [BooleanField( "Campus Account Mapping",
        Key = AttributeKey.CampusAccountMapping,
        Description = "When enabled, the block selects child accounts that match the giver's campus. If no matching child exists, the parent account is used.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 14,
        IsRequired = false )]

    [BooleanField( "Allow Scheduled Gifts",
        Key = AttributeKey.AllowScheduledGifts,
        Description = "Whether givers can set up recurring scheduled gifts. Not compatible with Text-to-Give mode.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 15,
        IsRequired = false )]

    [BooleanField( "Allow Scheduled End Date",
        Key = AttributeKey.AllowScheduledEndDate,
        Description = "Whether givers can set an optional end date for recurring scheduled gifts.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 16,
        IsRequired = false )]

    [BooleanField( "Staff Impersonation",
        Key = AttributeKey.StaffImpersonation,
        Description = "Allows staff to view and edit transactions on behalf of another person. Only enable this on internal pages secured to trusted individuals.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 17,
        IsRequired = false )]

    [BooleanField( "Show Confirmation Step",
        Key = AttributeKey.ShowConfirmationStep,
        Description = "Whether a confirmation step is shown before the transaction is processed.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_GeneralSettings,
        Order = 18,
        IsRequired = false )]

    #endregion Basic Settings > General Settings

    #region Basic Settings > Payer Settings

    [BooleanField( "Prompt for Phone",
        Key = AttributeKey.PromptForPhone,
        Description = "Whether givers are prompted to enter their phone number.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "SMS Opt-In",
        Key = AttributeKey.SmsOptIn,
        Description = "When phone prompting is enabled, displays an opt-in checkbox for SMS communications on the entered number.",
        TrueText = "Show",
        FalseText = "Hide",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Prompt for Email",
        Key = AttributeKey.PromptForEmail,
        Description = "Whether givers are prompted to enter their email address.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 2,
        IsRequired = false )]

    [GroupLocationTypeField( "Address Type",
        Key = AttributeKey.AddressType,
        Description = "The location type used when saving or updating the person's address.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 3,
        IsRequired = false )]

    [DefinedValueField( "Connection Status (New People)",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status assigned to new individuals created through this block.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        AllowMultiple = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 4,
        IsRequired = true )]

    [DefinedValueField( "Record Status (New People)",
        Key = AttributeKey.RecordStatus,
        Description = "The record status assigned to new individuals created through this block.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        AllowMultiple = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 5,
        IsRequired = true )]

    [DefinedValueField( "Record Source (New People)",
        Key = AttributeKey.RecordSource,
        Description = "The record source assigned to new individuals. Can be overridden by a RecordSource page parameter.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING,
        AllowMultiple = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 6,
        IsRequired = true )]

    [BooleanField( "Allow Business Giving",
        Key = AttributeKey.AllowBusinessGiving,
        Description = "Whether the option to give as a business is shown to the giver.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 7,
        IsRequired = false )]

    [BooleanField( "Allow Anonymous Giving",
        Key = AttributeKey.AllowAnonymousGiving,
        Description = "Whether givers can choose to give anonymously. Anonymous gifts appear as \"Anonymous\" on public-facing contribution lists.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 8,
        IsRequired = false )]

    [BooleanField( "Allow Comment Entry",
        Key = AttributeKey.AllowCommentEntry,
        Description = "Whether givers can enter a custom comment. The entered value is appended to the Payment Comment Template.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 9,
        IsRequired = false )]

    [BooleanField( "Disable CAPTCHA",
        Key = AttributeKey.DisableCAPTCHA,
        Description = "Skips the CAPTCHA verification step when enabled.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings_PayerSettings,
        Order = 10,
        IsRequired = false )]

    #endregion Basic Settings > Payer Settings

    #region Basic Settings > Email Templates

    [SystemCommunicationField( "Account Confirmation Email",
        Key = AttributeKey.AccountConfirmationEmail,
        Description = "The system communication sent to confirm a new account.",
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Category = AttributeCategory.BasicSettings_EmailTemplates,
        Order = 0,
        IsRequired = false )]

    [SystemCommunicationField( "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system communication used to send giving receipts.",
        Category = AttributeCategory.BasicSettings_EmailTemplates,
        Order = 1,
        IsRequired = false )]

    #endregion Basic Settings > Email Templates

    #region Customize Text > General Settings

    [BooleanField( "Show Panel & Section Headings",
        Key = AttributeKey.ShowPanelAndSectionHeadings,
        Description = "Whether the block panel title and section headings are visible. Note: if 'Show Block Header Section' is enabled, the block panel title will not be shown.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomizeText_GeneralSettings,
        Order = 0,
        IsRequired = false )]

    [TextField( "Panel Title",
        Key = AttributeKey.PanelTitle,
        Description = "The heading text shown at the top of the block panel.",
        DefaultValue = "Gifts",
        Category = AttributeCategory.CustomizeText_GeneralSettings,
        Order = 1,
        IsRequired = false )]

    [CodeEditorField( "Transaction Header Template",
        Key = AttributeKey.TransactionHeaderTemplate,
        Description = "The Lava template displayed above the amount entry fields.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        DefaultValue = "",
        Category = AttributeCategory.CustomizeText_GeneralSettings,
        Order = 2,
        IsRequired = false )]

    [CodeEditorField( "Payment Comment Template",
        Key = AttributeKey.PaymentCommentTemplate,
        Description = "The Lava template for the comment sent to the payment gateway with each transaction.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        Category = AttributeCategory.CustomizeText_GeneralSettings,
        Order = 3,
        IsRequired = false )]

    #endregion Customize Text > General Settings

    #region Customize Text > Block Header Section

    [BooleanField( "Show Block Header Section",
        Key = AttributeKey.ShowBlockHeaderSection,
        Description = "When enabled, displays a title and description at the top of the block.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomizeText_BlockHeaderSection,
        Order = 0,
        IsRequired = false )]

    [TextField( "Header Title",
        Key = AttributeKey.HeaderTitle,
        Description = "The title displayed at the top of the block.",
        DefaultValue = "New Contribution",
        Category = AttributeCategory.CustomizeText_BlockHeaderSection,
        Order = 1,
        IsRequired = false )]

    [TextField( "Header Description",
        Key = AttributeKey.HeaderDescription,
        Description = "The supporting text displayed below the header title.",
        DefaultValue = "Provide details to set up a new contribution.",
        Category = AttributeCategory.CustomizeText_BlockHeaderSection,
        Order = 2,
        IsRequired = false )]

    [TextField( "Header Icon",
        Key = AttributeKey.HeaderIcon,
        Description = "The icon displayed in the block header.",
        DefaultValue = "ti ti-cash",
        Category = AttributeCategory.CustomizeText_BlockHeaderSection,
        Order = 3,
        IsRequired = false )]

    #endregion Customize Text > Block Header Section

    #region Customize Text > Campus Information Section

    [TextField( "Campus Information Section Title",
        Key = AttributeKey.CampusInformationSectionTitle,
        Description = "The label displayed in the Campus Information section header.",
        DefaultValue = "Campus Information",
        Category = AttributeCategory.CustomizeText_CampusInformationSection,
        Order = 0,
        IsRequired = false )]

    [TextField( "Campus Information Section Icon",
        Key = AttributeKey.CampusInformationSectionIcon,
        Description = "The icon displayed in the Campus Information section header.",
        DefaultValue = "ti ti-map-pin",
        Category = AttributeCategory.CustomizeText_CampusInformationSection,
        Order = 1,
        IsRequired = false )]

    [TextField( "Campus Information Section Description",
        Key = AttributeKey.CampusInformationSectionDescription,
        Description = "Supporting text below the section title.",
        DefaultValue = "Select the campus that your gift should be associated with.",
        Category = AttributeCategory.CustomizeText_CampusInformationSection,
        Order = 2,
        IsRequired = false )]

    #endregion Customize Text > Campus Information Section

    #region Customize Text > Contribution Information Section

    [TextField( "Contribution Information Section Heading",
        Key = AttributeKey.ContributionInformationSectionHeading,
        Description = "The heading for the account and amount selection section.",
        DefaultValue = "Contribution Information",
        Category = AttributeCategory.CustomizeText_ContributionInformationSection,
        Order = 0,
        IsRequired = false )]

    [TextField( "Contribution Information Section Icon",
        Key = AttributeKey.ContributionInformationSectionIcon,
        Description = "The icon displayed in the Contribution Information section header.",
        DefaultValue = "ti ti-gift",
        Category = AttributeCategory.CustomizeText_ContributionInformationSection,
        Order = 1,
        IsRequired = false )]

    [TextField( "Contribution Information Section Description",
        Key = AttributeKey.ContributionInformationSectionDescription,
        Description = "Supporting text below the section title.",
        DefaultValue = "Specify how much to contribute, where it should go, and how often.",
        Category = AttributeCategory.CustomizeText_ContributionInformationSection,
        Order = 2,
        IsRequired = false )]

    [TextField( "Add Account Button Text",
        Key = AttributeKey.AddAccountButtonText,
        Description = "The label on the button that adds another account.",
        DefaultValue = "Add Another Account",
        Category = AttributeCategory.CustomizeText_ContributionInformationSection,
        Order = 3,
        IsRequired = false )]

    [CodeEditorField( "Account Label Template",
        Key = AttributeKey.AccountLabelTemplate,
        Description = "The Lava template used as the label for each account's amount input.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 50,
        DefaultValue = "{{ Account.PublicName }}",
        Category = AttributeCategory.CustomizeText_ContributionInformationSection,
        Order = 4,
        IsRequired = true )]

    [TextField( "Comment Field Label",
        Key = AttributeKey.CommentFieldLabel,
        Description = "The label shown on the comment input field (e.g., Trip Name).",
        DefaultValue = "Comment",
        Category = AttributeCategory.CustomizeText_ContributionInformationSection,
        Order = 5,
        IsRequired = false )]

    #endregion Customize Text > Contribution Information Section

    #region Customize Text > Contact Information Section

    [TextField( "Contact Information Section Heading",
        Key = AttributeKey.ContactInformationSectionHeading,
        Description = "The heading for the contact information section.",
        DefaultValue = "Contact Information",
        Category = AttributeCategory.CustomizeText_ContactInformationSection,
        Order = 0,
        IsRequired = false )]

    [TextField( "Contact Information Section Icon",
        Key = AttributeKey.ContactInformationSectionIcon,
        Description = "The icon displayed in the Contact Information section header.",
        DefaultValue = "ti ti-user-circle",
        Category = AttributeCategory.CustomizeText_ContactInformationSection,
        Order = 1,
        IsRequired = false )]

    [TextField( "Contact Information Section Description",
        Key = AttributeKey.ContactInformationSectionDescription,
        Description = "Supporting text below the section title.",
        DefaultValue = "Provide contact details to associate with this gift.",
        Category = AttributeCategory.CustomizeText_ContactInformationSection,
        Order = 2,
        IsRequired = false )]

    [TextField( "Anonymous Giving Tooltip",
        Key = AttributeKey.AnonymousGivingTooltip,
        Description = "The tooltip text shown on the Give Anonymously checkbox.",
        DefaultValue = "",
        Category = AttributeCategory.CustomizeText_ContactInformationSection,
        Order = 3,
        IsRequired = false )]

    #endregion Customize Text > Contact Information Section

    #region Customize Text > Payment Information Section

    [TextField( "Payment Information Section Heading",
        Key = AttributeKey.PaymentInformationSectionHeading,
        Description = "The heading for the payment method section.",
        DefaultValue = "Payment Information",
        Category = AttributeCategory.CustomizeText_PaymentInformationSection,
        Order = 0,
        IsRequired = false )]

    [TextField( "Payment Information Section Icon",
        Key = AttributeKey.PaymentInformationSectionIcon,
        Description = "The icon displayed in the Payment Information section header.",
        DefaultValue = "ti ti-wallet",
        Category = AttributeCategory.CustomizeText_PaymentInformationSection,
        Order = 1,
        IsRequired = false )]

    [TextField( "Payment Information Section Description",
        Key = AttributeKey.PaymentInformationSectionDescription,
        Description = "Supporting text below the section title.",
        DefaultValue = "Enter the payment method and billing details used to process this gift.",
        Category = AttributeCategory.CustomizeText_PaymentInformationSection,
        Order = 2,
        IsRequired = false )]

    #endregion Customize Text > Payment Information Section

    #region Customize Text > Confirmation Page

    [TextField( "Confirmation Section Heading",
        Key = AttributeKey.ConfirmationSectionHeading,
        Description = "The heading for the confirmation review section.",
        DefaultValue = "Confirm Information",
        Category = AttributeCategory.CustomizeText_ConfirmationPage,
        Order = 0,
        IsRequired = false )]

    [CodeEditorField( "Confirmation Header",
        Key = AttributeKey.ConfirmationHeader,
        Description = "HTML displayed at the top of the confirmation section. Supports Lava.",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 200,
        DefaultValue = AttributeDefault.ConfirmationHeader,
        Category = AttributeCategory.CustomizeText_ConfirmationPage,
        Order = 1,
        IsRequired = true )]

    [CodeEditorField( "Confirmation Body",
        Key = AttributeKey.ConfirmationBody,
        Description = "Body content rendered on the confirmation step. Supports Lava.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        DefaultValue = AttributeDefault.ConfirmationBody,
        Category = AttributeCategory.CustomizeText_ConfirmationPage,
        Order = 2,
        IsRequired = false )]

    [CodeEditorField( "Confirmation Footer",
        Key = AttributeKey.ConfirmationFooter,
        Description = "HTML displayed at the bottom of the confirmation section. Supports Lava.",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 200,
        DefaultValue = AttributeDefault.ConfirmationFooter,
        Category = AttributeCategory.CustomizeText_ConfirmationPage,
        Order = 3,
        IsRequired = true )]

    #endregion Customize Text > Confirmation Page

    #region Customize Text > Success Page

    [CodeEditorField( "Success Page Template",
        Key = AttributeKey.SuccessPageTemplate,
        Description = "The Lava template rendered on the success page after a transaction completes.",
        EditorMode = CodeEditorMode.Lava,
        DefaultValue = AttributeDefault.SuccessPageTemplate,
        Category = AttributeCategory.CustomizeText_SuccessPage,
        Order = 0,
        IsRequired = false )]

    [TextField( "Save Payment Method Section Heading",
        Key = AttributeKey.SavePaymentMethodSectionHeading,
        Description = "The heading for the save payment method section.",
        DefaultValue = "Make Giving Even Easier",
        Category = AttributeCategory.CustomizeText_SuccessPage,
        Order = 1,
        IsRequired = false )]

    [TextField( "Save Payment Method Section Icon",
        Key = AttributeKey.SavePaymentMethodSectionIcon,
        Description = "The icon displayed in the Save Payment Method section header.",
        DefaultValue = "ti ti-bolt",
        Category = AttributeCategory.CustomizeText_SuccessPage,
        Order = 2,
        IsRequired = false )]

    [TextField( "Save Payment Method Section Description",
        Key = AttributeKey.SavePaymentMethodSectionDescription,
        Description = "Supporting text below the section title.",
        DefaultValue = "Save your payment details to make future giving faster.",
        Category = AttributeCategory.CustomizeText_SuccessPage,
        Order = 3,
        IsRequired = false )]

    [CodeEditorField( "Success Page Footer",
        Key = AttributeKey.SuccessPageFooter,
        Description = "HTML displayed at the bottom of the success page. Supports Lava.",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 200,
        DefaultValue = "",
        Category = AttributeCategory.CustomizeText_SuccessPage,
        Order = 4,
        IsRequired = false )]

    #endregion Customize Text > Success Page

    #region Advanced

    [BooleanField( "Allow Account Options in URL",
        Key = AttributeKey.AllowAccountOptionsInURL,
        Description = "Whether account options (IDs, GL codes, amounts, editability) can be passed as URL parameters.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "Restrict URL Accounts to Public Only",
        Key = AttributeKey.RestrictURLAccountsToPublicOnly,
        Description = "When URL account options are enabled, prevents non-public accounts from being specified in the URL.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Advanced,
        Order = 1,
        IsRequired = false )]

    [CodeEditorField( "Invalid Account Message",
        Key = AttributeKey.InvalidAccountMessage,
        Description = "HTML error message shown when an invalid account ID or GL code is passed in the URL.",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 200,
        DefaultValue = "The configured financial accounts are not valid for accepting financial transactions.",
        Category = AttributeCategory.Advanced,
        Order = 2,
        IsRequired = true )]

    [CustomDropdownListField( "Account Campus Context Filter",
        Key = AttributeKey.AccountCampusContextFilter,
        Description = "Whether and how the current campus context filters the account list.",
        ListSource = "-1^No Account Campus Context Filter Applied,0^Only Accounts with Current Campus Context,1^Accounts with No Campus and Current Campus Context",
        DefaultValue = "-1",
        Category = AttributeCategory.Advanced,
        Order = 3,
        IsRequired = false )]

    [AttributeField( "Transaction Attributes from URL",
        Key = AttributeKey.TransactionAttributesFromURL,
        Description = "Transaction attributes that can be set via URL parameters using the Attribute_ prefix.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION,
        AllowMultiple = true,
        DefaultValue = "",
        Category = AttributeCategory.Advanced,
        Order = 4,
        IsRequired = false )]

    [DefinedValueField( "Transaction Type",
        Key = AttributeKey.TransactionType,
        Description = "The financial transaction type applied to transactions created by this block.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        AllowMultiple = false,
        Category = AttributeCategory.Advanced,
        Order = 5,
        IsRequired = true )]

    [EntityTypeField( "Transaction Entity Type",
        Key = AttributeKey.TransactionEntityType,
        Description = "The entity type for the transaction detail record. Leave blank unless this block is linked to a specific entity.",
        Category = AttributeCategory.Advanced,
        Order = 6,
        IsRequired = false )]

    [TextField( "Entity ID Parameter",
        Key = AttributeKey.EntityIdParameter,
        Description = "The page parameter used to populate the entity ID on the transaction detail record. Requires Transaction Entity Type to be set.",
        DefaultValue = "",
        Category = AttributeCategory.Advanced,
        Order = 7,
        IsRequired = false )]

    [BooleanField( "Show Initial Back Button",
        Key = AttributeKey.ShowInitialBackButton,
        Description = "Whether a Back button is shown on the first step, navigating the individual to the previous page.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 8,
        IsRequired = false )]

    [BooleanField( "Text-to-Give Mode",
        Key = AttributeKey.TextToGiveMode,
        Description = "Enables the Text-to-Give account setup flow. Not compatible with scheduled transactions.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 9,
        IsRequired = false )]

    #endregion Advanced

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "BA5C3D24-FDAE-42FD-AEE6-032D0CA7405E" )]
    [Rock.SystemGuid.BlockTypeGuid( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76" )]
    //// was [Rock.SystemGuid.BlockTypeGuid( "7498E1EE-FB79-41FE-9685-6A3D29E3AA76" )]
    //[Rock.SystemGuid.BlockTypeGuid( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004" )]
    public class UtilityPaymentEntry : RockBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            // Basic Settings > General Settings
            public const string FinancialGateway = "FinancialGateway";
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string TransactionSource = "Source";
            public const string PromptForCampusWhenKnown = "AskForCampusIfKnown";
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string CampusTypeFilter = "IncludedCampusTypes";
            public const string CampusStatusFilter = "IncludedCampusStatuses";
            public const string AllowMultipleAccounts = "EnableMultiAccount";
            public const string LayoutStyle = "LayoutStyle";
            public const string AccountsToDisplay = "AccountsToDisplay";
            public const string AllowAdditionalAccounts = "AdditionalAccounts";
            public const string GroupAdditionalAccountsByHierarchy = "EnableAccountHierarchy";
            public const string CampusAccountMapping = "UseAccountCampusMappingLogic";
            public const string AllowScheduledGifts = "AllowScheduled";
            public const string AllowScheduledEndDate = "EnableEndDate";
            public const string StaffImpersonation = "Impersonation";
            public const string ShowConfirmationStep = "ShowConfirmationPage";

            // Basic Settings > Payer Settings
            public const string PromptForPhone = "DisplayPhone";
            public const string SmsOptIn = "SmsOptIn";
            public const string PromptForEmail = "DisplayEmail";
            public const string AddressType = "AddressType";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string RecordSource = "RecordSource";
            public const string AllowBusinessGiving = "EnableBusinessGiving";
            public const string AllowAnonymousGiving = "EnableAnonymousGiving";
            public const string AllowCommentEntry = "EnableCommentEntry";
            public const string DisableCAPTCHA = "DisableCaptchaSupport";

            // Basic Settings > Email Templates
            public const string AccountConfirmationEmail = "ConfirmAccountTemplate";
            public const string ReceiptEmail = "ReceiptEmail";

            // Customize Text > General Settings
            public const string ShowPanelAndSectionHeadings = "ShowPanelHeadings";
            public const string PanelTitle = "PanelTitle";
            public const string TransactionHeaderTemplate = "TransactionHeader";
            public const string PaymentCommentTemplate = "PaymentCommentTemplate";

            // Customize Text > Block Header Section
            public const string ShowBlockHeaderSection = "ShowBlockHeaderSection";
            public const string HeaderTitle = "HeaderTitle";
            public const string HeaderDescription = "HeaderDescription";
            public const string HeaderIcon = "HeaderIcon";

            // Customize Text > Campus Information Section
            public const string CampusInformationSectionTitle = "CampusInformationSectionTitle";
            public const string CampusInformationSectionIcon = "CampusInformationSectionIcon";
            public const string CampusInformationSectionDescription = "CampusInformationSectionDescription";

            // Customize Text > Contribution Information Section
            public const string ContributionInformationSectionHeading = "ContributionInfoTitle";
            public const string ContributionInformationSectionIcon = "ContributionInformationSectionIcon";
            public const string ContributionInformationSectionDescription = "ContributionInformationSectionDescription";
            public const string AddAccountButtonText = "AddAccountText";
            public const string AccountLabelTemplate = "AccountHeaderTemplate";
            public const string CommentFieldLabel = "CommentEntryLabel";

            // Customize Text > Contact Information Section
            public const string ContactInformationSectionHeading = "PersonalInfoTitle";
            public const string ContactInformationSectionIcon = "ContactInformationSectionIcon";
            public const string ContactInformationSectionDescription = "ContactInformationSectionDescription";
            public const string AnonymousGivingTooltip = "AnonymousGivingTooltip";

            // Customize Text > Payment Information Section
            public const string PaymentInformationSectionHeading = "PaymentInfoTitle";
            public const string PaymentInformationSectionIcon = "PaymentInformationSectionIcon";
            public const string PaymentInformationSectionDescription = "PaymentInformationSectionDescription";

            // Customize Text > Confirmation Page
            public const string ConfirmationSectionHeading = "ConfirmationTitle";
            public const string ConfirmationHeader = "ConfirmationHeader";
            public const string ConfirmationBody = "ConfirmationBody";
            public const string ConfirmationFooter = "ConfirmationFooter";

            // Customize Text > Success Page
            public const string SuccessPageTemplate = "FinishLavaTemplate";
            public const string SavePaymentMethodSectionHeading = "SaveAccountTitle";
            public const string SavePaymentMethodSectionIcon = "SavePaymentMethodSectionIcon";
            public const string SavePaymentMethodSectionDescription = "SavePaymentMethodSectionDescription";
            public const string SuccessPageFooter = "SuccessFooter";

            // Advanced
            public const string AllowAccountOptionsInURL = "AllowAccountOptionsInURL";
            public const string RestrictURLAccountsToPublicOnly = "OnlyPublicAccountsInURL";
            public const string InvalidAccountMessage = "InvalidAccountMessage";
            public const string AccountCampusContextFilter = "AccountCampusContext";
            public const string TransactionAttributesFromURL = "AllowedTransactionAttributesFromURL";
            public const string TransactionType = "TransactionType";
            public const string TransactionEntityType = "TransactionEntityType";
            public const string EntityIdParameter = "EntityIdParam";
            public const string ShowInitialBackButton = "EnableInitialBackbutton";
            public const string TextToGiveMode = "EnableTextToGiveSetup";
        }

        private static class AttributeCategory
        {
            public const string BasicSettings_GeneralSettings = "";
            public const string BasicSettings_PayerSettings = "Payer Settings";
            public const string BasicSettings_EmailTemplates = "Email Templates";

            public const string CustomizeText_GeneralSettings = "Customize Text^";
            public const string CustomizeText_BlockHeaderSection = "Customize Text^Block Header Section";
            public const string CustomizeText_CampusInformationSection = "Customize Text^Campus Information Section";
            public const string CustomizeText_ContributionInformationSection = "Customize Text^Contribution Information Section";
            public const string CustomizeText_ContactInformationSection = "Customize Text^Contact Information Section";
            public const string CustomizeText_PaymentInformationSection = "Customize Text^Payment Information Section";
            public const string CustomizeText_ConfirmationPage = "Customize Text^Confirmation Page";
            public const string CustomizeText_SuccessPage = "Customize Text^Success Page";

            public const string Advanced = "Advanced";
        }

        private static class AttributeDefault
        {
            public const string ConfirmationHeader = @"
<p>
    Please confirm the information below. Once you have confirmed that the information is accurate click the ""Finish"" button to complete your transaction.
</p>
";

            public const string ConfirmationBody = @"
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
";

            public const string ConfirmationFooter = @"
<div class='alert alert-info'>
    By clicking the ""Finish"" button below I agree to allow {{ OrganizationName }} to transfer the amount above from my account. I acknowledge that I may update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions.
</div>
";

            public const string SuccessPageTemplate = @"
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
";
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

        /// <summary>
        /// The ISO 8601 date format the DatePicker control consumes and round-trips (date only).
        /// </summary>
        private const string DatePickerDateFormat = "yyyy-MM-dd";

        /// <summary>
        /// The message shown at submit when the CAPTCHA token is missing or invalid.
        /// </summary>
        private const string CaptchaValidationMessage = "Please complete the verification again to continue.";

        #endregion Keys & Constants

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<UtilityPaymentEntryBag, UtilityPaymentEntryOptionsBag>();

            var targetPerson = GetTargetPerson( RockContext, out var targetPersonWarning );

            // An invalid impersonation token or a disallowed impersonation attempt hides the entry flow and
            // shows only the warning.
            if ( targetPersonWarning.IsNotNullOrWhiteSpace() )
            {
                box.ErrorMessage = targetPersonWarning;

                return box;
            }

            box.Options = GetBoxOptions( targetPerson );

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Validates the entered gift before the payment method is tokenized, so every error surfaces together before
        /// calling the payment gateway.
        /// </summary>
        /// <param name="request">The gift details entered so far.</param>
        /// <returns>The messages to correct, or an empty list when the entry is ready to submit.</returns>
        [BlockAction]
        public BlockActionResult Validate( UtilityPaymentEntryConfirmationRequestBag request )
        {
            var targetPerson = GetTargetPerson( RockContext, out var targetPersonWarning );

            if ( targetPersonWarning.IsNotNullOrWhiteSpace() )
            {
                return ActionOk( new List<string> { targetPersonWarning } );
            }

            return ActionOk( ValidateEntry( targetPerson, request ) );
        }

        /// <summary>
        /// Builds the confirmation-step review content for the gift the giver entered. Resolves the
        /// Confirmation Body Lava against the gift summary (account rows, total, when, name, email, and
        /// address) so the giver can review before the gift is processed. The payment method and masked
        /// account number are intentionally not part of the summary: the hosted gateway control returns
        /// only a token, so neither is known before the charge.
        /// </summary>
        /// <param name="request">The gift details the giver entered.</param>
        /// <returns>The resolved confirmation body, or an error to show on the entry step.</returns>
        [BlockAction]
        public BlockActionResult GetConfirmation( UtilityPaymentEntryConfirmationRequestBag request )
        {
            // Re-check that the gift may be entered on this individual's behalf before building the summary.
            var targetPerson = GetTargetPerson( RockContext, out var targetPersonWarning );

            if ( targetPersonWarning.IsNotNullOrWhiteSpace() )
            {
                return ActionOk( ConfirmationError( targetPersonWarning ) );
            }

            // The confirmation step is the entry submit ("Next"), so validate the CAPTCHA here. A
            // missing or stale token keeps the giver on the entry step to re-verify.
            if ( !IsCaptchaSatisfied() )
            {
                return ActionOk( ConfirmationError( CaptchaValidationMessage ) );
            }

            var validationErrors = ValidateEntry( targetPerson, request );

            if ( validationErrors.Any() )
            {
                return ActionOk( ConfirmationError( validationErrors ) );
            }

            var accountAllocations = GetMappedAccountAllocations( request.AccountAmounts, request.CampusGuid );

            var mergeFields = BuildConfirmationMergeFields( request, accountAllocations );

            return ActionOk( new UtilityPaymentEntryConfirmationResponseBag
            {
                IsSuccess = true,
                BodyHtml = GetAttributeValue( AttributeKey.ConfirmationBody ).ResolveMergeFields( mergeFields )
            } );
        }

        /// <summary>
        /// Processes the giver's gift and records it. The client tokenizes the payment method and sends the
        /// token here. An immediate one-time gift charges the token and saves the transaction and its batch;
        /// a recurring or future-dated gift is scheduled with the gateway and saved as a scheduled
        /// transaction. Either way, the resolved Success Page Lava is returned.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <returns>The processing result: the success HTML, or an error message to show on the entry step.</returns>
        [BlockAction]
        public BlockActionResult ProcessTransaction( UtilityPaymentEntryProcessRequestBag request )
        {
            // When there is no confirmation step, this action is the entry submit, so validate the CAPTCHA
            // here. With a confirmation step, GetConfirmation already validated it at the Next transition,
            // and the one-shot token would no longer be valid.
            if ( !GetAttributeValue( AttributeKey.ShowConfirmationStep ).AsBoolean() && !IsCaptchaSatisfied() )
            {
                return ActionOk( ProcessError( CaptchaValidationMessage ) );
            }

            var financialGateway = GetConfiguredFinancialGateway();
            var financialGatewayComponent = financialGateway?.GetGatewayComponent();

            if ( financialGatewayComponent == null )
            {
                return ActionOk( ProcessError( "There was a problem creating the payment gateway information." ) );
            }

            var targetPerson = GetTargetPerson( RockContext, out var targetPersonWarning );

            if ( targetPersonWarning.IsNotNullOrWhiteSpace() )
            {
                return ActionOk( ProcessError( targetPersonWarning ) );
            }

            var validationErrors = ValidateEntry( targetPerson, ToConfirmationRequest( request ) );

            if ( validationErrors.Any() )
            {
                return ActionOk( ProcessError( validationErrors ) );
            }

            // Resolve the giver: the business (Give As Business), or the signed-in / impersonated
            // individual, or an individual matched or created from the entered details. The contact
            // individual is tracked separately so a scheduled business gift can own its schedule while the
            // gift itself is authorized to the business.
            Person person;
            Person contactPerson;

            if ( IsGivingAsBusiness( request.IsGivingAsBusiness ) )
            {
                person = ResolveBusinessGiver( request, targetPerson, out contactPerson );
            }
            else
            {
                person = ResolveIndividualGiver( request, targetPerson );
                contactPerson = person;
            }

            if ( person?.PrimaryAliasId == null )
            {
                return ActionOk( ProcessError( "There was a problem creating the person information." ) );
            }

            var accountAllocations = GetMappedAccountAllocations( request.AccountAmounts, request.CampusGuid );

            financialGateway.LoadAttributes( RockContext );

            var paymentInfo = BuildPaymentInfo( request, accountAllocations, person, targetPerson, out var paymentInfoError );

            if ( paymentInfo == null )
            {
                return ActionOk( ProcessError( paymentInfoError ) );
            }

            ComposePaymentComment( paymentInfo, request );

            // Turn the payment token into a reusable customer reference before charging or scheduling.
            if ( financialGatewayComponent is IObsidianHostedGatewayComponent obsidianGatewayComponent
                && paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var customerToken = obsidianGatewayComponent.CreateCustomerAccount( financialGateway, paymentInfo, out var customerError );

                if ( customerError.IsNotNullOrWhiteSpace() )
                {
                    return ActionOk( ProcessError( customerError ) );
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;
            }

            // A recurring or future-dated gift is scheduled with the gateway instead of charged now.
            var schedule = GetSchedule( request.FrequencyGuid, request.StartDate, request.EndDate, financialGateway, financialGatewayComponent );

            if ( schedule != null )
            {
                var scheduledTransactionToTransfer = GetScheduledTransactionToTransfer( RockContext, targetPerson );

                return ActionOk( ProcessScheduledTransaction( request, schedule, financialGateway, financialGatewayComponent, person, contactPerson, accountAllocations, paymentInfo, scheduledTransactionToTransfer ) );
            }

            // Guard against a double charge: if a transaction with this Guid already exists, show success
            // without charging again.
            var existingTransaction = new FinancialTransactionService( RockContext ).Queryable()
                .FirstOrDefault( transaction => transaction.Guid == request.TransactionGuid );

            if ( existingTransaction != null )
            {
                return ActionOk( BuildSuccessResponse( request, financialGateway, paymentInfo, existingTransaction.TransactionCode ) );
            }

            var chargedTransaction = ChargePayment( financialGateway, financialGatewayComponent, request.GatewayToken, paymentInfo, out var chargeError );

            if ( chargedTransaction == null )
            {
                return ActionOk( ProcessError( chargeError.IsNotNullOrWhiteSpace() ? chargeError : "Unknown Error" ) );
            }

            // Assign the client-minted Guid so a retry cannot create a duplicate transaction.
            chargedTransaction.Guid = request.TransactionGuid;

            // Honor the anonymous flag only when the block allows anonymous giving, never on the client's
            // word alone.
            var isAnonymous = request.IsAnonymous && GetAttributeValue( AttributeKey.AllowAnonymousGiving ).AsBoolean();

            SaveTransaction( financialGateway, financialGatewayComponent, person, paymentInfo, accountAllocations, isAnonymous, chargedTransaction );

            // Text-to-Give sets up a reusable saved account automatically instead of offering the manual
            // save on the success step.
            if ( GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean() )
            {
                ConfigureTextToGiveAccount( chargedTransaction, person, request );
            }

            return ActionOk( BuildSuccessResponse( request, financialGateway, paymentInfo, chargedTransaction.TransactionCode ) );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Resolves the individual the gift is for. When the person action identifier ("rckid") page
        /// parameter is supplied, the gift is entered on that individual's behalf (impersonation); otherwise
        /// the target is the signed-in individual. Impersonation is allowed only when the block is in
        /// Text-to-Give mode or Staff Impersonation is enabled. An invalid token, or a disallowed
        /// impersonation attempt, yields a warning and a null result.
        /// </summary>
        /// <param name="rockContext">The context used to resolve the individual.</param>
        /// <param name="warningMessage">Set to the warning to display when resolution fails; empty otherwise.</param>
        /// <returns>The resolved target individual; null when the token is invalid, impersonation is
        /// disallowed, or no individual is signed in.</returns>
        private Person GetTargetPerson( RockContext rockContext, out string warningMessage )
        {
            warningMessage = string.Empty;

            // Text-to-Give setup always runs on another individual's behalf, so it forces impersonation on.
            var isImpersonationAllowed = GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean()
                || GetAttributeValue( AttributeKey.StaffImpersonation ).AsBoolean();

            var personActionIdentifier = PageParameter( PageParameterKey.PersonActionIdentifier );

            if ( personActionIdentifier.IsNullOrWhiteSpace() )
            {
                // No token was supplied, so the gift is for the signed-in individual (may be null).
                return RequestContext.CurrentPerson;
            }

            var targetPerson = new PersonService( rockContext ).GetByPersonActionIdentifier( personActionIdentifier, "transaction" );

            if ( isImpersonationAllowed )
            {
                if ( targetPerson == null )
                {
                    warningMessage = "Invalid or Expired Person Token specified";

                    return null;
                }
            }
            else if ( targetPerson?.Id != RequestContext.CurrentPerson?.Id )
            {
                // Impersonation is off, so the token must resolve to the signed-in individual.
                warningMessage = "Impersonation is not allowed on this block.";

                return null;
            }

            // Pre-load the campus so it is available without a later lazy load.
            targetPerson?.GetCampus();

            return targetPerson;
        }

        /// <summary>
        /// Resolves the scheduled transaction being transferred, from the Transfer and
        /// ScheduledTransactionGuid page parameters. Returns the schedule only when both parameters are
        /// present and the schedule is authorized to the target individual (their own giving id or one of
        /// their businesses'); otherwise null, so the entry proceeds as a normal new gift.
        /// </summary>
        /// <param name="rockContext">The context used to resolve the schedule.</param>
        /// <param name="targetPerson">The individual the gift is for, whose ownership authorizes the transfer.</param>
        /// <returns>The scheduled transaction to transfer, or null.</returns>
        private FinancialScheduledTransaction GetScheduledTransactionToTransfer( RockContext rockContext, Person targetPerson )
        {
            if ( targetPerson == null || PageParameter( PageParameterKey.Transfer ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            var scheduledTransactionGuid = PageParameter( PageParameterKey.ScheduledTransactionGuid ).AsGuidOrNull();

            if ( !scheduledTransactionGuid.HasValue )
            {
                return null;
            }

            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( scheduledTransactionGuid.Value );

            if ( scheduledTransaction?.AuthorizedPersonAlias?.Person == null )
            {
                return null;
            }

            // The giver may only transfer a schedule that belongs to them or one of their businesses.
            var personService = new PersonService( rockContext );
            var givingIds = personService.GetBusinesses( targetPerson.Id ).Select( business => business.GivingId ).ToList();
            givingIds.Add( targetPerson.GivingId );

            if ( !givingIds.Contains( scheduledTransaction.AuthorizedPersonAlias.Person.GivingId ) )
            {
                return null;
            }

            return scheduledTransaction;
        }

        /// <summary>
        /// Builds the configuration the block needs for its initial render.
        /// </summary>
        /// <param name="targetPerson">The individual the gift is for, used to resolve the default campus.</param>
        /// <returns>The populated options bag.</returns>
        private UtilityPaymentEntryOptionsBag GetBoxOptions( Person targetPerson )
        {
            var financialGateway = GetConfiguredFinancialGateway();
            var options = new UtilityPaymentEntryOptionsBag
            {
                IsGatewayConfigured = financialGateway != null
            };

            // The headings and block header show whether or not a gateway is configured.
            SetHeadingOptions( options );

            if ( financialGateway == null )
            {
                options.SupportedGateways = GetSupportedGateways();

                return options;
            }

            var financialGatewayComponent = financialGateway.GetGatewayComponent();

            // A gateway with both currency types disabled, or one without a hosted payment interface, cannot be used,
            // so a configuration warning replaces the entry flow instead of rendering an unusable payment section.
            if ( !GetAttributeValue( AttributeKey.EnableACH ).AsBoolean() && !GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean() )
            {
                options.ConfigurationWarningTitle = "Configuration";
                options.ConfigurationWarningMessage = "Enable ACH and/or Enable Credit Card needs to be enabled.";

                return options;
            }

            if ( !( financialGatewayComponent is IHostedGatewayComponent ) )
            {
                options.ConfigurationWarningTitle = "Unsupported Gateway";
                options.ConfigurationWarningMessage = "This block only supports Gateways that have a hosted payment interface.";

                return options;
            }

            // Fluid renders the entry sections in the two-column layout; Vertical (the default) stacks them.
            options.IsFluidLayout = GetAttributeValue( AttributeKey.LayoutStyle ) == "Fluid";

            options.IsTestGateway = financialGatewayComponent is TestGateway;
            options.TransactionHeaderHtml = ResolveTransactionHeaderHtml();
            options.InitialBackButtonUrl = ResolveInitialBackButtonUrl();

            // A transfer re-creates an existing scheduled gift: it seeds the frequency, start date, and
            // (for a business gift) the business, then cancels the old schedule at save.
            var scheduledTransactionToTransfer = GetScheduledTransactionToTransfer( RockContext, targetPerson );

            SetCampusOptions( options, targetPerson );
            SetContributionOptions( options );
            SetScheduleAndCommentOptions( options, financialGatewayComponent, financialGateway, scheduledTransactionToTransfer );
            SetContactOptions( options, targetPerson );
            SetBusinessOptions( options, targetPerson, scheduledTransactionToTransfer );
            SetPaymentOptions( options, financialGatewayComponent, financialGateway, targetPerson );
            SetConfirmationOptions( options );

            return options;
        }

        /// <summary>
        /// Gets the financial gateway configured for this block, or null when none is selected.
        /// </summary>
        /// <returns>The configured financial gateway, or null.</returns>
        private FinancialGateway GetConfiguredFinancialGateway()
        {
            var financialGatewayGuid = GetAttributeValue( AttributeKey.FinancialGateway ).AsGuid();

            return new FinancialGatewayService( RockContext ).GetNoTracking( financialGatewayGuid );
        }

        /// <summary>
        /// Adds the heading, panel-title, and block-header-section options. These apply whether or not a
        /// gateway is configured.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        private void SetHeadingOptions( UtilityPaymentEntryOptionsBag options )
        {
            options.IsPanelAndSectionHeadingsShown = GetAttributeValue( AttributeKey.ShowPanelAndSectionHeadings ).AsBoolean();
            options.PanelTitle = GetAttributeValue( AttributeKey.PanelTitle );
            options.IsHeaderSectionShown = GetAttributeValue( AttributeKey.ShowBlockHeaderSection ).AsBoolean();
            options.HeaderIcon = GetAttributeValue( AttributeKey.HeaderIcon );
            options.HeaderTitle = GetAttributeValue( AttributeKey.HeaderTitle );
            options.HeaderDescription = GetAttributeValue( AttributeKey.HeaderDescription );
        }

        /// <summary>
        /// Builds the list of installed hosted gateway components that have at least one active
        /// instance, shown as help when the block has no financial gateway configured. The Test
        /// Gateway is excluded.
        /// </summary>
        /// <returns>The supported gateway components.</returns>
        private List<SupportedGatewayBag> GetSupportedGateways()
        {
            var hostedGatewayComponents = GatewayContainer.Instance.Components
                .Select( component => component.Value.Value )
                .OfType<IHostedGatewayComponent>()
                .Where( component => !( component is TestGateway ) )
                .ToList();

            // Pre-fetch the entity type ids of all active gateway instances so the per-component
            // active check below does not hit the database in a loop.
            var activeGatewayEntityTypeIds = new FinancialGatewayService( RockContext )
                .Queryable()
                .Where( gateway => gateway.IsActive && gateway.EntityTypeId.HasValue )
                .Select( gateway => gateway.EntityTypeId.Value )
                .Distinct()
                .ToList();

            var supportedGateways = new List<SupportedGatewayBag>();

            foreach ( var component in hostedGatewayComponents )
            {
                var entityType = EntityTypeCache.Get( component.TypeGuid );

                if ( entityType == null || !activeGatewayEntityTypeIds.Contains( entityType.Id ) )
                {
                    continue;
                }

                var componentType = entityType.GetEntityType();

                supportedGateways.Add( new SupportedGatewayBag
                {
                    Name = Rock.Reflection.GetDisplayName( componentType ),
                    Description = Rock.Reflection.GetDescription( componentType ),
                    ConfigureUrl = component.ConfigureURL,
                    LearnMoreUrl = component.LearnMoreURL
                } );
            }

            return supportedGateways;
        }

        /// <summary>
        /// Resolves the Transaction Header Lava template against the common merge fields plus the
        /// transaction-entity, fundraising, and amount-limit merge fields.
        /// </summary>
        /// <returns>The resolved HTML, or an empty string when the template is blank.</returns>
        private string ResolveTransactionHeaderHtml()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            AddTransactionHeaderMergeFields( mergeFields );

            return GetAttributeValue( AttributeKey.TransactionHeaderTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Adds the transaction-entity, fundraising, and amount-limit merge fields shared by the Transaction
        /// Header, Confirmation Header and Footer, and Success Page templates. The entity fields are added only
        /// when a Transaction Entity Type and a matching entity are configured; the amount limit is always
        /// added from the AmountLimit page parameter.
        /// </summary>
        /// <param name="mergeFields">The merge field dictionary to add to.</param>
        private void AddTransactionHeaderMergeFields( Dictionary<string, object> mergeFields )
        {
            var transactionEntity = GetTransactionEntity();

            if ( transactionEntity != null )
            {
                mergeFields["TransactionEntity"] = transactionEntity;
                var transactionEntityTypeId = transactionEntity.TypeId;

                // The transactions already filed against this entity, exposed for Lava along with their total.
                var transactionEntityTransactions = new FinancialTransactionService( RockContext ).Queryable( "TransactionDetails" )
                    .Where( transaction =>
                        transaction.TransactionDetails.Any( detail =>
                            detail.EntityTypeId.HasValue
                            && detail.EntityTypeId == transactionEntityTypeId
                            && detail.EntityId == transactionEntity.Id
                        )
                    )
                    .ToList();

                var transactionEntityTransactionsTotal = transactionEntityTransactions.SelectMany( transaction => transaction.TransactionDetails )
                    .Where( detail =>
                        detail.EntityTypeId.HasValue
                        && detail.EntityTypeId == transactionEntityTypeId
                        && detail.EntityId == transactionEntity.Id
                    )
                    .Sum( detail => ( decimal? ) detail.Amount );

                mergeFields["TransactionEntityTransactions"] = transactionEntityTransactions;
                mergeFields["TransactionEntityTransactionsTotal"] = transactionEntityTransactionsTotal;

                AddFundraisingMergeFields( mergeFields, transactionEntity, transactionEntityTypeId, transactionEntityTransactionsTotal );
            }

            mergeFields["AmountLimit"] = PageParameter( PageParameterKey.AmountLimit ).AsDecimalOrNull();
        }

        /// <summary>
        /// Adds the FundraisingGoal and AmountRaised merge fields when the transaction entity is a group
        /// member on a fundraising opportunity. The ParticipationMode page parameter selects family totals
        /// (goal and raised summed across the giver's family members in the group) over the default individual
        /// totals. The goal reads each member's IndividualFundraisingGoal attribute, falling back to the
        /// group's.
        /// </summary>
        /// <param name="mergeFields">The merge field dictionary to add to.</param>
        /// <param name="transactionEntity">The resolved transaction entity.</param>
        /// <param name="transactionEntityTypeId">The transaction entity's type id.</param>
        /// <param name="transactionEntityTransactionsTotal">The total already raised for the entity, used as the individual amount raised.</param>
        private void AddFundraisingMergeFields( Dictionary<string, object> mergeFields, IEntity transactionEntity, int transactionEntityTypeId, decimal? transactionEntityTransactionsTotal )
        {
            if ( EntityTypeCache.Get( transactionEntityTypeId )?.Guid != Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() )
            {
                return;
            }

            var groupMember = new GroupMemberService( RockContext ).Get( transactionEntity.Guid );

            if ( groupMember == null )
            {
                return;
            }

            if ( GetParticipationMode() == ( int ) ParticipationType.Family )
            {
                var familyGroupMembers = new GroupService( RockContext ).GroupMembersInAnotherGroup( groupMember.Person.GetFamily(), groupMember.Group );
                decimal familyFundraisingGoal = 0;

                foreach ( var familyGroupMember in familyGroupMembers )
                {
                    familyGroupMember.LoadAttributes( RockContext );
                    familyGroupMember.Group.LoadAttributes( RockContext );
                    familyFundraisingGoal += familyGroupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull()
                        ?? familyGroupMember.Group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull()
                        ?? 0;
                }

                mergeFields["FundraisingGoal"] = familyFundraisingGoal;
                mergeFields["AmountRaised"] = new FinancialTransactionDetailService( RockContext )
                    .GetContributionsForGroupMemberList( transactionEntityTypeId, familyGroupMembers.Select( member => member.Id ).ToList() );
            }
            else
            {
                groupMember.LoadAttributes( RockContext );
                groupMember.Group.LoadAttributes( RockContext );

                mergeFields["FundraisingGoal"] = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull()
                    ?? groupMember.Group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull()
                    ?? 0;
                mergeFields["AmountRaised"] = transactionEntityTransactionsTotal;
            }
        }

        /// <summary>
        /// Gets the fundraising participation mode from the ParticipationMode page parameter, defaulting to
        /// individual when the parameter is absent or unparseable.
        /// </summary>
        /// <returns>The participation mode as its <see cref="ParticipationType"/> integer value.</returns>
        private int GetParticipationMode()
        {
            return PageParameter( PageParameterKey.ParticipationMode ).AsIntegerOrNull() ?? ( int ) ParticipationType.Individual;
        }

        /// <summary>
        /// Resolves the URL the entry-step Back button navigates to. Returns the request referrer
        /// when the Show Initial Back Button setting is on and a referrer is available; otherwise null
        /// so no Back button is shown.
        /// </summary>
        /// <returns>The referrer URL, or null when no Back button should be shown.</returns>
        private string ResolveInitialBackButtonUrl()
        {
            if ( !GetAttributeValue( AttributeKey.ShowInitialBackButton ).AsBoolean() )
            {
                return null;
            }

            var referrer = RequestContext.GetHeader( "Referer" )?.FirstOrDefault();

            // Match legacy Request.UrlReferrer: only an absolute URI counts as a referrer. A missing
            // or unparseable value leaves the Back button hidden (legacy's HRef == "#" case).
            if ( Uri.TryCreate( referrer, UriKind.Absolute, out var referrerUri ) )
            {
                return referrerUri.ToString();
            }

            return null;
        }

        /// <summary>
        /// Adds the Campus Information section options.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="targetPerson">The individual the gift is for, used to resolve the default campus.</param>
        private void SetCampusOptions( UtilityPaymentEntryOptionsBag options, Person targetPerson )
        {
            options.IsCampusPromptedWhenKnown = GetAttributeValue( AttributeKey.PromptForCampusWhenKnown ).AsBoolean();
            options.DefaultCampusGuid = GetDefaultCampusGuid( targetPerson );
            options.CampusSectionTitle = GetAttributeValue( AttributeKey.CampusInformationSectionTitle );
            options.CampusSectionIcon = GetAttributeValue( AttributeKey.CampusInformationSectionIcon );
            options.CampusSectionDescription = GetAttributeValue( AttributeKey.CampusInformationSectionDescription );
            options.AreInactiveCampusesIncluded = GetAttributeValue( AttributeKey.IncludeInactiveCampuses ).AsBoolean();
            options.CampusTypeFilter = GetAttributeValue( AttributeKey.CampusTypeFilter ).SplitDelimitedValues().AsGuidList();
            options.CampusStatusFilter = GetAttributeValue( AttributeKey.CampusStatusFilter ).SplitDelimitedValues().AsGuidList();
        }

        /// <summary>
        /// Whether an integer entity ID is accepted in a page parameter.
        /// </summary>
        private bool AllowIntegerIdentifiers => !PageCache.Layout.Site.DisablePredictableIds;

        /// <summary>
        /// Resolves the campus already known for the individual, used as the initial campus
        /// selection. Prefers the CampusId page parameter, then the target individual's own campus.
        /// </summary>
        /// <param name="targetPerson">The individual the gift is for, whose campus is the fallback.</param>
        /// <returns>The known campus Guid, or null when none can be resolved.</returns>
        private Guid? GetDefaultCampusGuid( Person targetPerson )
        {
            var campusKey = PageParameter( PageParameterKey.CampusId );

            if ( campusKey.IsNotNullOrWhiteSpace() )
            {
                return CampusCache.Get( campusKey, AllowIntegerIdentifiers )?.Guid;
            }

            return targetPerson?.GetCampus()?.Guid;
        }

        /// <summary>
        /// Adds the Contribution Information section options, including the addable-account pool.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        private void SetContributionOptions( UtilityPaymentEntryOptionsBag options )
        {
            options.ContributionSectionTitle = GetAttributeValue( AttributeKey.ContributionInformationSectionHeading );
            options.ContributionSectionIcon = GetAttributeValue( AttributeKey.ContributionInformationSectionIcon );
            options.ContributionSectionDescription = GetAttributeValue( AttributeKey.ContributionInformationSectionDescription );

            var accountResolution = ResolveContributionAccounts();

            options.UrlInvalidAccountMessage = accountResolution.UrlInvalidAccountMessage;
            options.PresetAccountAmounts = accountResolution.PresetAccountAmounts;
            options.Accounts = ResolveAccountListItems( accountResolution.AccountGuidsToDisplay, accountResolution.AllowPrivateAccounts );
            options.IsMultiAccountEntry = GetAttributeValue( AttributeKey.AllowMultipleAccounts ).AsBoolean();
            options.AddAccountButtonText = GetAttributeValue( AttributeKey.AddAccountButtonText );
            options.AdditionalAccounts = GetAvailableAdditionalAccounts( accountResolution.AccountGuidsToDisplay );
        }

        /// <summary>
        /// Resolves the accounts the Contribution Information section presents and any amounts URL account
        /// options preset or locked. Shared by the options builder, which presents the accounts, and the
        /// submit path, which re-enforces the same account rules server-side.
        /// </summary>
        /// <returns>The accounts to present, whether private accounts are allowed, the URL preset amounts,
        /// and the Invalid Account Message when a URL account was dropped.</returns>
        private ContributionAccountResolution ResolveContributionAccounts()
        {
            var accountResolution = new ContributionAccountResolution();
            var campusContextFilter = GetAccountCampusContextFilter();
            var urlAccountOptions = ParseUrlAccountOptions();

            accountResolution.HasUrlAccountOptions = urlAccountOptions.Any();

            if ( accountResolution.HasUrlAccountOptions )
            {
                // URL account options override the configured account list (legacy parity). Private
                // accounts are allowed only when the giver has not restricted the URL to public accounts.
                accountResolution.AllowPrivateAccounts = !GetAttributeValue( AttributeKey.RestrictURLAccountsToPublicOnly ).AsBoolean();

                // Drop options excluded by the campus context filter, keeping unresolved options so the
                // invalid-account message can still flag a bad id or GL code.
                var contextFilteredOptions = urlAccountOptions
                    .Where( option =>
                    {
                        if ( !option.AccountGuid.HasValue )
                        {
                            return true;
                        }

                        var account = FinancialAccountCache.Get( option.AccountGuid.Value );
                        return account != null && campusContextFilter( account );
                    } )
                    .ToList();

                // Keep only options that resolved to an active, in-date account that is public (or
                // private when allowed). Invalid or unresolvable options are dropped from the presented
                // accounts and presets, and drive the Invalid Account Message.
                var validOptions = contextFilteredOptions
                    .Where( option => IsValidUrlAccount( option.AccountGuid, accountResolution.AllowPrivateAccounts ) )
                    .ToList();

                if ( validOptions.Count < contextFilteredOptions.Count )
                {
                    var invalidAccountMessage = GetAttributeValue( AttributeKey.InvalidAccountMessage );
                    accountResolution.UrlInvalidAccountMessage = invalidAccountMessage.IsNullOrWhiteSpace() ? null : invalidAccountMessage;
                }

                accountResolution.AccountGuidsToDisplay = validOptions
                    .Select( option => option.AccountGuid.Value )
                    .ToList();

                accountResolution.PresetAccountAmounts = validOptions
                    .Select( option => new UtilityPaymentEntryPresetAccountAmountBag
                    {
                        AccountGuid = option.AccountGuid.Value,
                        Amount = option.Amount,
                        IsReadOnly = !option.IsEnabled
                    } )
                    .ToList();
            }
            else
            {
                accountResolution.AccountGuidsToDisplay = GetAttributeValue( AttributeKey.AccountsToDisplay )
                    .SplitDelimitedValues()
                    .AsGuidList()
                    .Where( guid =>
                    {
                        var account = FinancialAccountCache.Get( guid );
                        return account != null && campusContextFilter( account );
                    } )
                    .ToList();
            }

            return accountResolution;
        }

        /// <summary>
        /// Builds the server-authoritative account rules for a submitted gift when URL account options are
        /// in effect: the accounts the giver is permitted to give to (the URL accounts plus any Add Another
        /// Account pool) and the amounts locked by a read-only URL account option. Returns null when URL
        /// account options are not active, leaving the configured-account submit path unchanged.
        /// </summary>
        /// <returns>The account rules to re-enforce, or null when URL account options are not active.</returns>
        private AccountSubmitRules BuildUrlAccountSubmitRules()
        {
            var accountResolution = ResolveContributionAccounts();

            if ( !accountResolution.HasUrlAccountOptions )
            {
                return null;
            }

            // Reuse the exact display resolution so the accounts allowed at submit cannot drift from the
            // accounts presented on entry.
            var allowedAccountGuids = new HashSet<Guid>(
                ResolveAccountListItems( accountResolution.AccountGuidsToDisplay, accountResolution.AllowPrivateAccounts )
                    .Select( account => account.Value.AsGuidOrNull() )
                    .Where( guid => guid.HasValue )
                    .Select( guid => guid.Value ) );

            foreach ( var additionalAccountGuid in FlattenAccountTreeGuids( GetAvailableAdditionalAccounts( accountResolution.AccountGuidsToDisplay ) ) )
            {
                allowedAccountGuids.Add( additionalAccountGuid );
            }

            var lockedAmounts = accountResolution.PresetAccountAmounts
                .Where( preset => preset.IsReadOnly && preset.AccountGuid.HasValue )
                .GroupBy( preset => preset.AccountGuid.Value )
                .ToDictionary( group => group.Key, group => group.First().Amount ?? 0m );

            return new AccountSubmitRules
            {
                AllowedAccountGuids = allowedAccountGuids,
                LockedAmounts = lockedAmounts
            };
        }

        /// <summary>
        /// Collects the account Guids from an account tree, including nested child accounts.
        /// </summary>
        /// <param name="accountTree">The account tree to flatten.</param>
        /// <returns>Every account Guid in the tree.</returns>
        private static IEnumerable<Guid> FlattenAccountTreeGuids( List<TreeItemBag> accountTree )
        {
            foreach ( var item in accountTree )
            {
                var guid = item.Value.AsGuidOrNull();

                if ( guid.HasValue )
                {
                    yield return guid.Value;
                }

                if ( item.Children != null )
                {
                    foreach ( var childGuid in FlattenAccountTreeGuids( item.Children ) )
                    {
                        yield return childGuid;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the Account Campus Context Filter predicate from the AccountCampusContext setting and the
        /// page's current campus context.
        /// </summary>
        /// <returns>A predicate that returns true when an account passes the campus context filter.</returns>
        private Func<FinancialAccountCache, bool> GetAccountCampusContextFilter()
        {
            var contextFilterMode = GetAttributeValue( AttributeKey.AccountCampusContextFilter ).AsIntegerOrNull() ?? -1;

            // Mode -1 disables the campus context filter, so every account passes.
            if ( contextFilterMode < 0 )
            {
                return _ => true;
            }

            var contextCampusId = RequestContext.GetContextEntity<Campus>()?.Id;

            // If there is no campus context, there is nothing to filter against, so every account passes.
            if ( !contextCampusId.HasValue )
            {
                return _ => true;
            }

            // Mode 0 keeps only accounts on the context campus; mode 1 also keeps accounts with no campus.
            return account => account.CampusId == contextCampusId.Value
                || ( contextFilterMode == 1 && account.CampusId == null );
        }

        /// <summary>
        /// Parses the AccountIds and AccountGlCodes page parameters into URL account options (an
        /// account id or GL code, an optional preset amount, and an editable flag). Returns an empty
        /// list when Allow Account Options In URL is off or neither parameter is present.
        /// </summary>
        /// <returns>The parsed URL account options.</returns>
        private List<UrlAccountOption> ParseUrlAccountOptions()
        {
            if ( !GetAttributeValue( AttributeKey.AllowAccountOptionsInURL ).AsBoolean() )
            {
                return new List<UrlAccountOption>();
            }

            var result = new List<UrlAccountOption>();
            result.AddRange( ParseUrlAccountOptionsParameter( PageParameter( PageParameterKey.AccountIdsOptions ), false ) );
            result.AddRange( ParseUrlAccountOptionsParameter( PageParameter( PageParameterKey.AccountGlCodesOptions ), true ) );
            return result;
        }

        /// <summary>
        /// Parses one URL account options parameter (a comma-delimited list of
        /// "identifier^amount^editable" entries). An id or GL code that does not resolve yields an
        /// option with no AccountGuid, so it is left out of the picker but still counts as invalid for
        /// the Invalid Account Message (unlike the legacy block, no phantom account is seeded).
        /// </summary>
        /// <param name="accountOptionsParameterValue">The raw parameter value.</param>
        /// <param name="parseAsGlCode">When true the identifier is a GL code; otherwise an account id, IdKey, or Guid.</param>
        /// <returns>The parsed URL account options.</returns>
        private List<UrlAccountOption> ParseUrlAccountOptionsParameter( string accountOptionsParameterValue, bool parseAsGlCode )
        {
            var result = new List<UrlAccountOption>();
            if ( accountOptionsParameterValue.IsNullOrWhiteSpace() )
            {
                return result;
            }

            foreach ( var accountOption in accountOptionsParameterValue.Split( ',' ) )
            {
                var parts = accountOption.Split( '^' ).ToList();
                while ( parts.Count < 3 )
                {
                    parts.Add( null );
                }

                var account = parseAsGlCode
                    ? ResolveAccountByGlCode( parts[0] )
                    : FinancialAccountCache.Get( parts[0], AllowIntegerIdentifiers );

                result.Add( new UrlAccountOption
                {
                    AccountGuid = account?.Guid,
                    Amount = parts[1].AsDecimalOrNull(),
                    IsEnabled = parts[2].AsBooleanOrNull() ?? true
                } );
            }

            return result;
        }

        /// <summary>
        /// Resolves the account a GL code refers to, without validity filtering, so the id and
        /// GL-code paths resolve consistently. The caller judges validity.
        /// </summary>
        /// <param name="glCode">The GL code from the URL.</param>
        /// <returns>The matching account, or null when none matches.</returns>
        private FinancialAccountCache ResolveAccountByGlCode( string glCode )
        {
            if ( glCode.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return FinancialAccountCache.All()
                .Where( a => a.GlCode == glCode )
                .OrderBy( a => a.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Determines whether a URL account option may be presented: it resolved to an active, in-date
        /// account that is public, or private when private accounts are allowed.
        /// </summary>
        /// <param name="accountGuid">The resolved account Guid, or null when the id or GL code did not resolve.</param>
        /// <param name="allowPrivateAccounts">Whether non-public accounts are allowed.</param>
        /// <returns>True when the account is valid to present.</returns>
        private bool IsValidUrlAccount( Guid? accountGuid, bool allowPrivateAccounts )
        {
            if ( !accountGuid.HasValue )
            {
                return false;
            }

            var account = FinancialAccountCache.Get( accountGuid.Value );
            if ( account == null )
            {
                return false;
            }

            var today = RockDateTime.Today;

            return account.IsActive
                && ( allowPrivateAccounts || ( account.IsPublic ?? false ) )
                && ( account.StartDate == null || account.StartDate.Value <= today )
                && ( account.EndDate == null || account.EndDate.Value >= today );
        }

        /// <summary>
        /// Resolves the accounts presented to the giver (name and Guid), mirroring the shared accounts
        /// endpoint: active, in-date accounts ordered by account order, each label resolved from the
        /// Account Label Template. Public accounts only, unless private accounts are allowed (URL
        /// account options with the public-only restriction off). An empty selectable list resolves to
        /// every eligible account.
        /// </summary>
        /// <param name="accountGuidsToDisplay">The Guids of the accounts to resolve, or empty for all.</param>
        /// <param name="allowPrivateAccounts">Whether non-public accounts are included.</param>
        /// <returns>The resolved accounts as list items.</returns>
        private List<ListItemBag> ResolveAccountListItems( List<Guid> accountGuidsToDisplay, bool allowPrivateAccounts )
        {
            var today = RockDateTime.Today;
            var accountGuidSet = new HashSet<Guid>( accountGuidsToDisplay );
            var hasSpecificAccounts = accountGuidSet.Count > 0;

            var accounts = FinancialAccountCache.All()
                .Where( a => ( !hasSpecificAccounts || accountGuidSet.Contains( a.Guid ) )
                    && a.IsActive
                    && ( allowPrivateAccounts || ( a.IsPublic ?? false ) )
                    && ( a.StartDate == null || a.StartDate.Value <= today )
                    && ( a.EndDate == null || a.EndDate.Value >= today ) )
                .OrderBy( a => a.Order )
                .ToList();

            var resolveAccountLabel = GetAccountLabelResolver();

            return accounts
                .Select( account => new ListItemBag
                {
                    Text = resolveAccountLabel( account ),
                    Value = account.Guid.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Builds a resolver that renders an account's label from the Account Label Template, used for
        /// every account name the block presents (configured, URL, and addable).
        /// </summary>
        /// <returns>A function that resolves an account's label.</returns>
        private Func<FinancialAccountCache, string> GetAccountLabelResolver()
        {
            var accountLabelTemplate = GetAttributeValue( AttributeKey.AccountLabelTemplate );
            if ( accountLabelTemplate.IsNullOrWhiteSpace() )
            {
                accountLabelTemplate = "{{ Account.PublicName }}";
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            return account =>
            {
                mergeFields["Account"] = account;
                return accountLabelTemplate.ResolveMergeFields( mergeFields );
            };
        }

        /// <summary>
        /// Gets the accounts the giver may add beyond the configured list (the "Add Another Account"
        /// pool): active, public, in-date accounts that are not already configured. Returns an empty list
        /// unless additional accounts are allowed and a specific account list is configured (an empty
        /// configured list already shows every account). Applies to both single- and multiple-account
        /// entry. When Group Additional Accounts by Hierarchy is enabled the accounts are nested under
        /// their parent; otherwise they are returned as a flat list of roots ordered by account order.
        /// Every account, parents included, is selectable.
        /// </summary>
        /// <param name="configuredAccountGuids">The Guids of the accounts already shown to the giver.</param>
        /// <returns>The addable accounts as a tree: nested in hierarchy mode, flat roots otherwise.</returns>
        private List<TreeItemBag> GetAvailableAdditionalAccounts( List<Guid> configuredAccountGuids )
        {
            var isAdditionalAccountsAllowed = GetAttributeValue( AttributeKey.AllowAdditionalAccounts ).AsBoolean();

            if ( !isAdditionalAccountsAllowed || configuredAccountGuids == null || !configuredAccountGuids.Any() )
            {
                return new List<TreeItemBag>();
            }

            var today = RockDateTime.Today;
            var configuredAccountGuidSet = new HashSet<Guid>( configuredAccountGuids );
            var campusContextFilter = GetAccountCampusContextFilter();

            // The pool of accounts the giver may add: active, public, in-date, within the campus context
            // filter, and not already shown.
            var availableAccounts = FinancialAccountCache.All()
                .Where( account =>
                    account.IsActive
                    && account.IsPublic == true
                    && ( account.StartDate == null || account.StartDate <= today )
                    && ( account.EndDate == null || account.EndDate >= today )
                    && !configuredAccountGuidSet.Contains( account.Guid )
                    && campusContextFilter( account ) )
                .ToList();

            var resolveAccountLabel = GetAccountLabelResolver();
            var isHierarchyEnabled = GetAttributeValue( AttributeKey.GroupAdditionalAccountsByHierarchy ).AsBoolean();

            if ( !isHierarchyEnabled )
            {
                return availableAccounts
                    .OrderBy( account => account.Order )
                    .Select( account => new TreeItemBag
                    {
                        Value = account.Guid.ToString(),
                        Text = resolveAccountLabel( account ),
                        Children = new List<TreeItemBag>()
                    } )
                    .ToList();
            }

            // Hierarchy mode: nest each account under its parent. An account whose parent is not in the
            // pool is a root. The tree control makes every node selectable, parents included.
            var availableAccountIds = new HashSet<int>( availableAccounts.Select( account => account.Id ) );

            var childrenByParentId = availableAccounts
                .Where( account => account.ParentAccountId.HasValue && availableAccountIds.Contains( account.ParentAccountId.Value ) )
                .GroupBy( account => account.ParentAccountId.Value )
                .ToDictionary( group => group.Key, group => group.OrderBy( account => account.PublicName ).ToList() );

            return availableAccounts
                .Where( account => !account.ParentAccountId.HasValue || !availableAccountIds.Contains( account.ParentAccountId.Value ) )
                .OrderBy( account => account.PublicName )
                .Select( rootAccount => BuildAccountTreeItem( rootAccount, childrenByParentId, resolveAccountLabel ) )
                .ToList();
        }

        /// <summary>
        /// Builds a tree item for an account and, depth-first, its in-pool descendants.
        /// </summary>
        /// <param name="account">The account to build a tree item for.</param>
        /// <param name="childrenByParentId">In-pool child accounts grouped by parent account id, pre-sorted for display.</param>
        /// <param name="resolveAccountLabel">Resolves each account's display label.</param>
        /// <returns>The account as a tree item with its descendants nested beneath it.</returns>
        private static TreeItemBag BuildAccountTreeItem( FinancialAccountCache account, Dictionary<int, List<FinancialAccountCache>> childrenByParentId, Func<FinancialAccountCache, string> resolveAccountLabel )
        {
            var children = childrenByParentId.TryGetValue( account.Id, out var childAccounts )
                ? childAccounts.Select( child => BuildAccountTreeItem( child, childrenByParentId, resolveAccountLabel ) ).ToList()
                : new List<TreeItemBag>();

            return new TreeItemBag
            {
                Value = account.Guid.ToString(),
                Text = resolveAccountLabel( account ),
                Children = children,
                HasChildren = children.Count > 0,
                IsFolder = children.Count > 0
            };
        }

        /// <summary>
        /// Adds the scheduling ("how often") and comment options. Scheduling is shown only when scheduled
        /// gifts are allowed, the gateway supports a payment schedule, and Text-to-Give mode is off; the
        /// offered frequencies always include a One-Time option for immediate gifts. The StartDate and
        /// Frequency page parameters seed the defaults.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="financialGateway">The configured financial gateway, used to resolve the earliest
        /// scheduled start date.</param>
        private void SetScheduleAndCommentOptions( UtilityPaymentEntryOptionsBag options, GatewayComponent financialGatewayComponent, FinancialGateway financialGateway, FinancialScheduledTransaction scheduledTransactionToTransfer )
        {
            options.IsCommentShown = GetAttributeValue( AttributeKey.AllowCommentEntry ).AsBoolean();
            options.CommentLabel = GetAttributeValue( AttributeKey.CommentFieldLabel );

            var isSchedulingAllowed = GetAttributeValue( AttributeKey.AllowScheduledGifts ).AsBoolean();
            var isTextToGiveMode = GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean();
            var supportedFrequencies = financialGatewayComponent?.SupportedPaymentSchedules ?? new List<DefinedValueCache>();

            if ( !isSchedulingAllowed || isTextToGiveMode || !supportedFrequencies.Any() )
            {
                return;
            }

            // The gateway may not list One-Time; add it so immediate gifts are always possible.
            var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
            var frequencies = new List<DefinedValueCache>( supportedFrequencies );

            if ( frequencies.All( frequency => frequency.Id != oneTimeFrequency.Id ) )
            {
                frequencies.Insert( 0, oneTimeFrequency );
            }

            options.IsSchedulingShown = true;
            options.OneTimeFrequencyGuid = oneTimeFrequency.Guid;
            options.FrequencyOptions = frequencies.ToListItemBagList();
            options.IsScheduledEndDateAllowed = GetAttributeValue( AttributeKey.AllowScheduledEndDate ).AsBoolean();
            options.DefaultFrequencyGuid = oneTimeFrequency.Guid;
            options.DefaultStartDate = RockDateTime.Today.ToString( DatePickerDateFormat );

            var earliestScheduledStartDate = ( financialGatewayComponent as IHostedGatewayComponent )?.GetEarliestScheduledStartDate( financialGateway ) ?? RockDateTime.Today;
            options.EarliestScheduledStartDate = earliestScheduledStartDate.ToString( DatePickerDateFormat );

            ApplyScheduleUrlOptions( options, frequencies );
            ApplyTransferScheduleSeeding( options, frequencies, scheduledTransactionToTransfer );
        }

        /// <summary>
        /// Seeds the frequency and start date from the scheduled gift being transferred, overriding the
        /// defaults and any URL options. The start date is the schedule's next payment date (tomorrow when it
        /// has none). Sets the "Next Gift" start-date label when the schedule has a next payment date.
        /// </summary>
        /// <param name="options">The options bag to update.</param>
        /// <param name="frequencies">The frequencies offered to the giver, used to validate the schedule's frequency.</param>
        /// <param name="scheduledTransactionToTransfer">The schedule being transferred, or null when not transferring.</param>
        private void ApplyTransferScheduleSeeding( UtilityPaymentEntryOptionsBag options, List<DefinedValueCache> frequencies, FinancialScheduledTransaction scheduledTransactionToTransfer )
        {
            if ( scheduledTransactionToTransfer == null )
            {
                return;
            }

            var frequency = DefinedValueCache.Get( scheduledTransactionToTransfer.TransactionFrequencyValueId );
            if ( frequency != null && frequencies.Any( f => f.Id == frequency.Id ) )
            {
                options.DefaultFrequencyGuid = frequency.Guid;
            }

            var startDate = scheduledTransactionToTransfer.NextPaymentDate ?? RockDateTime.Today.AddDays( 1 );
            options.DefaultStartDate = startDate.ToString( DatePickerDateFormat );

            // The "Next Gift" label applies only when the schedule has a next payment date; otherwise the
            // label falls back to "When" / "First Gift".
            options.IsNextGiftLabelShown = scheduledTransactionToTransfer.NextPaymentDate.HasValue;
        }

        /// <summary>
        /// Applies the StartDate and Frequency page parameters to the scheduling options. StartDate is
        /// clamped to not before today. Frequency is "frequencyId^isEditable"; a false editable flag
        /// locks the frequency selection to the supplied value.
        /// </summary>
        /// <param name="options">The options bag to update.</param>
        /// <param name="frequencies">The frequencies offered to the giver, used to validate the parameter.</param>
        private void ApplyScheduleUrlOptions( UtilityPaymentEntryOptionsBag options, List<DefinedValueCache> frequencies )
        {
            var startDateParameter = PageParameter( PageParameterKey.StartDate );

            if ( startDateParameter.IsNotNullOrWhiteSpace() )
            {
                var startDate = startDateParameter.AsDateTime() ?? RockDateTime.Today;

                if ( startDate < RockDateTime.Today )
                {
                    startDate = RockDateTime.Today;
                }

                options.DefaultStartDate = startDate.ToString( DatePickerDateFormat );
            }

            var frequencyParameter = PageParameter( PageParameterKey.Frequency );

            if ( frequencyParameter.IsNullOrWhiteSpace() )
            {
                return;
            }

            var frequencyParts = frequencyParameter.Split( '^' );
            var frequency = DefinedValueCache.Get( frequencyParts[0], AllowIntegerIdentifiers );

            if ( frequency != null && frequencies.Any( f => f.Id == frequency.Id ) )
            {
                options.DefaultFrequencyGuid = frequency.Guid;
                options.IsFrequencyLocked = frequencyParts.Length >= 2 && !frequencyParts[1].AsBoolean( true );
            }
        }

        /// <summary>
        /// Adds the Contact Information section options, including the fields prefilled from the individual
        /// the gift is for.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="targetPerson">The individual the gift is for, whose contact details prefill the section.</param>
        private void SetContactOptions( UtilityPaymentEntryOptionsBag options, Person targetPerson )
        {
            options.ContactSectionTitle = GetAttributeValue( AttributeKey.ContactInformationSectionHeading );
            options.ContactSectionIcon = GetAttributeValue( AttributeKey.ContactInformationSectionIcon );
            options.ContactSectionDescription = GetAttributeValue( AttributeKey.ContactInformationSectionDescription );

            options.IsEmailPrompted = GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean();

            options.IsAnonymousGivingAllowed = GetAttributeValue( AttributeKey.AllowAnonymousGiving ).AsBoolean();
            options.AnonymousGivingTooltip = GetAttributeValue( AttributeKey.AnonymousGivingTooltip );

            // A known individual's name shows read-only; a new or nameless individual enters one.
            options.IsNameEntryShown = IsNameEntryShown( targetPerson );
            options.CurrentPersonFullName = targetPerson?.FullName ?? string.Empty;
            options.FirstName = targetPerson?.FirstName ?? string.Empty;
            options.LastName = targetPerson?.LastName ?? string.Empty;
            options.Email = targetPerson?.Email ?? string.Empty;
            options.PhoneCountryCode = PhoneNumber.DefaultCountryCode();

            var isPhonePrompted = GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();
            var isPhoneUnlisted = false;

            if ( targetPerson != null )
            {
                var personService = new PersonService( RockContext );

                SetAddressPrefill( options, personService, targetPerson );

                if ( isPhonePrompted )
                {
                    var givingPhoneNumber = ResolveGivingPhoneNumber( personService, targetPerson );

                    // An unlisted number hides the phone section instead of prefilling it.
                    isPhoneUnlisted = givingPhoneNumber?.IsUnlisted ?? false;

                    if ( givingPhoneNumber != null && !isPhoneUnlisted )
                    {
                        options.Phone = givingPhoneNumber.Number;
                        options.PhoneCountryCode = givingPhoneNumber.CountryCode;
                        options.IsSmsOptInChecked = givingPhoneNumber.IsMessagingEnabled;
                    }
                }
            }

            // The phone section hides when the individual's best number is unlisted; SMS opt-in follows the
            // phone, and its label is a system setting.
            options.IsPhonePrompted = isPhonePrompted && !isPhoneUnlisted;
            options.IsSmsOptInShown = GetAttributeValue( AttributeKey.SmsOptIn ).AsBoolean() && options.IsPhonePrompted;
            options.SmsOptInLabel = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );
        }

        /// <summary>
        /// Whether the first- and last-name entry fields are shown (a new or nameless individual). A known
        /// individual's name is read-only, so it is not entered.
        /// </summary>
        /// <param name="targetPerson">The individual resolved from the sign-in / impersonation context, or null.</param>
        /// <returns>True when the name entry fields are shown.</returns>
        private bool IsNameEntryShown( Person targetPerson )
        {
            return targetPerson == null || targetPerson.IsNameless();
        }

        /// <summary>
        /// Prefills the address options from the individual's address of the configured type (home when
        /// none is configured).
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="personService">The service used to read the individual's address.</param>
        /// <param name="targetPerson">The individual whose address is prefilled.</param>
        private void SetAddressPrefill( UtilityPaymentEntryOptionsBag options, PersonService personService, Person targetPerson )
        {
            var addressTypeGuid = GetAttributeValue( AttributeKey.AddressType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            var addressType = DefinedValueCache.Get( addressTypeGuid );

            if ( addressType == null )
            {
                return;
            }

            var location = personService.GetFirstLocation( targetPerson.Id, addressType.Id )?.Location;

            if ( location == null )
            {
                return;
            }

            options.Address = new AddressControlBag
            {
                Street1 = location.Street1,
                Street2 = location.Street2,
                City = location.City,
                State = location.State,
                PostalCode = location.PostalCode,
                Country = location.Country
            };
        }

        /// <summary>
        /// Resolves the phone number to prefill for the giver: the individual's home number, falling back
        /// to their mobile number when the home number is missing or unlisted.
        /// </summary>
        /// <param name="personService">The service used to read the individual's numbers.</param>
        /// <param name="targetPerson">The individual whose number is resolved.</param>
        /// <returns>The resolved phone number, or null when none is on file.</returns>
        private PhoneNumber ResolveGivingPhoneNumber( PersonService personService, Person targetPerson )
        {
            var phoneNumber = personService.GetPhoneNumber( targetPerson, DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ) );

            if ( phoneNumber == null || phoneNumber.Number.IsNullOrWhiteSpace() || phoneNumber.IsUnlisted )
            {
                phoneNumber = personService.GetPhoneNumber( targetPerson, DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ) );
            }

            return phoneNumber;
        }

        /// <summary>
        /// Adds the Payment Information section options: the hosted gateway control configuration and
        /// whether the CAPTCHA is shown.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="financialGateway">The configured financial gateway.</param>
        private void SetPaymentOptions( UtilityPaymentEntryOptionsBag options, GatewayComponent financialGatewayComponent, FinancialGateway financialGateway, Person targetPerson )
        {
            options.PaymentSectionTitle = GetAttributeValue( AttributeKey.PaymentInformationSectionHeading );
            options.PaymentSectionIcon = GetAttributeValue( AttributeKey.PaymentInformationSectionIcon );
            options.PaymentSectionDescription = GetAttributeValue( AttributeKey.PaymentInformationSectionDescription );
            // Account for the global CAPTCHA configuration, not just the block setting: when no CAPTCHA is
            // configured server-wide, it is effectively disabled and the widget must not gate the button.
            options.IsCaptchaShown = !Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCAPTCHA ).AsBoolean() );
            options.SavedAccounts = BuildSavedAccounts( targetPerson, financialGatewayComponent, financialGateway );

            // The Save Payment Method section on the success step is configured here alongside the payment
            // method it saves.
            options.SavePaymentMethodSectionHeading = GetAttributeValue( AttributeKey.SavePaymentMethodSectionHeading );
            options.SavePaymentMethodSectionIcon = GetAttributeValue( AttributeKey.SavePaymentMethodSectionIcon );
            options.SavePaymentMethodSectionDescription = GetAttributeValue( AttributeKey.SavePaymentMethodSectionDescription );
            options.AccountConfirmationEmailTemplateGuid = GetAttributeValue( AttributeKey.AccountConfirmationEmail ).AsGuidOrNull();

            // The billing address is collected in the Contact section, so the hosted gateway control
            // does not collect it.
            if ( financialGatewayComponent is IObsidianHostedGatewayComponent obsidianGatewayComponent )
            {
                options.GatewayControl = new GatewayControlBag
                {
                    FileUrl = obsidianGatewayComponent.GetObsidianControlFileUrl( financialGateway ),
                    Settings = obsidianGatewayComponent.GetObsidianControlSettings( financialGateway, new HostedPaymentInfoControlOptions
                    {
                        EnableACH = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean(),
                        EnableCreditCard = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean(),
                        EnableBillingAddressCollection = false
                    } )
                };
            }
        }

        /// <summary>
        /// Builds the target individual's reusable saved payment methods for the configured gateway,
        /// offered on the payment step, using the shared saved-account client service so the display
        /// (name, description, and card image) matches the other Obsidian giving blocks.
        /// </summary>
        /// <param name="targetPerson">The individual the gift is for, whose saved accounts are offered.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="financialGateway">The configured financial gateway.</param>
        /// <returns>The saved payment methods for display; empty when none apply.</returns>
        private List<SavedFinancialAccountListItemBag> BuildSavedAccounts( Person targetPerson, GatewayComponent financialGatewayComponent, FinancialGateway financialGateway )
        {
            if ( targetPerson == null || financialGateway == null || financialGatewayComponent == null )
            {
                return new List<SavedFinancialAccountListItemBag>();
            }

            var options = new SavedFinancialAccountOptions
            {
                FinancialGatewayGuids = new List<Guid> { financialGateway.Guid },
                CurrencyTypeGuids = GetAllowedSavedAccountCurrencyTypes( financialGatewayComponent ).Select( currencyType => currencyType.Guid ).ToList()
            };

            return new FinancialPersonSavedAccountClientService( RockContext, targetPerson )
                .GetSavedFinancialAccountsForPersonAsAccountListItems( targetPerson.Id, options );
        }

        /// <summary>
        /// The currency types whose saved payment methods may be offered on the configured gateway: credit
        /// card when Enable Credit Card is on and the gateway supports saving it, ACH when Enable ACH is on
        /// and the gateway supports saving it, and Apple Pay / Google Pay whenever the gateway supports
        /// saving them.
        /// </summary>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <returns>The allowed currency-type defined values.</returns>
        private List<DefinedValueCache> GetAllowedSavedAccountCurrencyTypes( GatewayComponent financialGatewayComponent )
        {
            var allowedCurrencyTypes = new List<DefinedValueCache>();

            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            if ( GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean() && financialGatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency );
            }

            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            if ( GetAttributeValue( AttributeKey.EnableACH ).AsBoolean() && financialGatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency );
            }

            var applePayCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_APPLE_PAY.AsGuid() );
            if ( financialGatewayComponent.SupportsSavedAccount( applePayCurrency ) )
            {
                allowedCurrencyTypes.Add( applePayCurrency );
            }

            var googlePayCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ANDROID_PAY.AsGuid() );
            if ( financialGatewayComponent.SupportsSavedAccount( googlePayCurrency ) )
            {
                allowedCurrencyTypes.Add( googlePayCurrency );
            }

            return allowedCurrencyTypes;
        }

        /// <summary>
        /// Adds the confirmation-step options. The Confirmation Header and Footer Lava are resolved here
        /// against the common merge fields (the same treatment as the Transaction Header); the gift-specific
        /// Confirmation Body is resolved per entry by the GetConfirmation action.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        private void SetConfirmationOptions( UtilityPaymentEntryOptionsBag options )
        {
            options.IsConfirmationStepShown = GetAttributeValue( AttributeKey.ShowConfirmationStep ).AsBoolean();
            options.ConfirmationSectionHeading = GetAttributeValue( AttributeKey.ConfirmationSectionHeading );

            var mergeFields = RequestContext.GetCommonMergeFields();
            AddTransactionHeaderMergeFields( mergeFields );
            options.ConfirmationHeaderHtml = GetAttributeValue( AttributeKey.ConfirmationHeader ).ResolveMergeFields( mergeFields );
            options.ConfirmationFooterHtml = GetAttributeValue( AttributeKey.ConfirmationFooter ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Validates the entered gift at submit time: an amount within the amount limit, then either the
        /// business name and business-contact fields (Give As Business) or the individual's name, plus the
        /// address, email, and phone required for the flow.
        /// </summary>
        /// <param name="targetPerson">The individual resolved from the sign-in / impersonation context, or null.</param>
        /// <param name="request">The entered gift details to validate.</param>
        /// <returns>The messages to correct, or an empty list when the entry is valid.</returns>
        private List<string> ValidateEntry( Person targetPerson, UtilityPaymentEntryConfirmationRequestBag request )
        {
            var errorMessages = new List<string>();

            // Re-enforce the URL account rules server-side before validating amounts, so a crafted request
            // cannot override a locked amount or give to an account that was not offered.
            EnforceUrlAccountRules( request.AccountAmounts, errorMessages );

            var enteredAmounts = request.AccountAmounts ?? new List<UtilityPaymentEntryAccountAmountBag>();

            if ( !enteredAmounts.Any( accountAmount => accountAmount.Amount > 0 ) )
            {
                errorMessages.Add( "Please enter an amount for at least one account." );
            }

            var amountLimit = PageParameter( PageParameterKey.AmountLimit ).AsDecimalOrNull();

            if ( amountLimit.HasValue && enteredAmounts.Sum( accountAmount => accountAmount.Amount ) > amountLimit.Value )
            {
                errorMessages.Add( $"The maximum amount is limited to {amountLimit.FormatAsCurrency()}." );
            }

            var isGivingAsBusiness = IsGivingAsBusiness( request.IsGivingAsBusiness );

            if ( isGivingAsBusiness )
            {
                ValidateBusinessEntry( request, errorMessages );
            }
            else if ( IsNameEntryShown( targetPerson ) )
            {
                ValidateEnteredName( request.FirstName, request.LastName, errorMessages );
            }

            var fieldPrefix = isGivingAsBusiness ? "Business " : "";

            if ( request.Address?.Street1.IsNullOrWhiteSpace() ?? true )
            {
                errorMessages.Add( $"Make sure to enter a valid {fieldPrefix}address. An address is required for us to process this transaction." );
            }

            if ( GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean() && request.Email.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( $"Make sure to enter a valid {fieldPrefix}email address. An email address is required for us to send you a payment confirmation." );
            }

            var isPhoneRequired = isGivingAsBusiness
                ? GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean()
                : IsPhoneFieldShown( targetPerson );

            if ( isPhoneRequired && request.Phone.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( $"Make sure to enter a valid {fieldPrefix}phone number. A phone number is required for us to process this transaction." );
            }

            ValidateScheduledDates( request, errorMessages );

            return errorMessages;
        }

        /// <summary>
        /// Re-enforces the URL account rules on a submitted gift server-side, independent of anything the
        /// client sent: an amount locked by a read-only URL account option is reset to its server value in
        /// place, and a gift to an account that was not offered yields an error. An offered account is a
        /// URL account or one from the Add Another Account pool, matching what the giver could select on
        /// entry; giving to a non-URL account is allowed only when that account is in the addable pool,
        /// just as it was in the legacy block. Does nothing when URL account options are not active.
        /// </summary>
        /// <param name="accountAmounts">The submitted per-account amounts; locked amounts are corrected in
        /// place.</param>
        /// <param name="errorMessages">The error list a disallowed account is reported to.</param>
        private void EnforceUrlAccountRules( List<UtilityPaymentEntryAccountAmountBag> accountAmounts, List<string> errorMessages )
        {
            var submitRules = BuildUrlAccountSubmitRules();

            if ( submitRules == null || accountAmounts == null )
            {
                return;
            }

            var hasDisallowedAccount = false;

            foreach ( var accountAmount in accountAmounts )
            {
                if ( !accountAmount.AccountGuid.HasValue )
                {
                    continue;
                }

                // Force an amount the URL locked back to its server value, whatever the client sent.
                if ( submitRules.LockedAmounts.TryGetValue( accountAmount.AccountGuid.Value, out var lockedAmount ) )
                {
                    accountAmount.Amount = lockedAmount;
                }

                // A zero-amount row is dropped before charging, so only a positive amount to a disallowed
                // account is a problem.
                if ( accountAmount.Amount > 0 && !submitRules.AllowedAccountGuids.Contains( accountAmount.AccountGuid.Value ) )
                {
                    hasDisallowedAccount = true;
                }
            }

            if ( hasDisallowedAccount )
            {
                errorMessages.Add( "One or more of the selected accounts is not available." );
            }
        }

        /// <summary>
        /// Whether the phone field is shown: the block prompts for phone and the individual's best number
        /// (home, then mobile) is not unlisted. Matches the runtime "unlisted phone hides the phone" flip.
        /// </summary>
        /// <param name="targetPerson">The individual resolved from the sign-in / impersonation context, or null.</param>
        /// <returns>True when the phone field is shown.</returns>
        private bool IsPhoneFieldShown( Person targetPerson )
        {
            if ( !GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean() )
            {
                return false;
            }

            if ( targetPerson == null )
            {
                return true;
            }

            var phoneNumber = ResolveGivingPhoneNumber( new PersonService( RockContext ), targetPerson );

            return phoneNumber == null || !phoneNumber.IsUnlisted;
        }

        /// <summary>
        /// Builds a failed confirmation result carrying the messages to show the giver.
        /// </summary>
        /// <param name="messages">The errors to show.</param>
        /// <returns>The failed confirmation result.</returns>
        private static UtilityPaymentEntryConfirmationResponseBag ConfirmationError( params string[] messages )
        {
            return ConfirmationError( messages.ToList() );
        }

        /// <summary>
        /// Builds a failed confirmation result carrying the messages to show the giver.
        /// </summary>
        /// <param name="messages">The errors to show.</param>
        /// <returns>The failed confirmation result.</returns>
        private static UtilityPaymentEntryConfirmationResponseBag ConfirmationError( List<string> messages )
        {
            return new UtilityPaymentEntryConfirmationResponseBag
            {
                IsSuccess = false,
                ErrorMessages = messages
            };
        }

        /// <summary>
        /// Builds a failed processing result carrying the messages to show the giver.
        /// </summary>
        /// <param name="messages">The errors to show.</param>
        /// <returns>The failed processing result.</returns>
        private static UtilityPaymentEntryProcessResponseBag ProcessError( params string[] messages )
        {
            return ProcessError( messages.ToList() );
        }

        /// <summary>
        /// Builds a failed processing result carrying the messages to show the giver.
        /// </summary>
        /// <param name="messages">The errors to show.</param>
        /// <returns>The failed processing result.</returns>
        private static UtilityPaymentEntryProcessResponseBag ProcessError( List<string> messages )
        {
            return new UtilityPaymentEntryProcessResponseBag
            {
                IsSuccess = false,
                ErrorMessages = messages
            };
        }

        /// <summary>
        /// Whether the CAPTCHA requirement is satisfied for a submit action: true when CAPTCHA is disabled
        /// (by the block setting or because none is configured server-wide), or when the token the client
        /// sent validated. The token rides in the block-action context and is validated by the action
        /// pipeline, which surfaces the result on <see cref="Rock.Net.RockRequestContext.IsCaptchaValid"/>.
        /// </summary>
        /// <returns>True when the submit may proceed with respect to the CAPTCHA.</returns>
        private bool IsCaptchaSatisfied()
        {
            var isCaptchaDisabled = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCAPTCHA ).AsBoolean() );

            return isCaptchaDisabled || RequestContext.IsCaptchaValid;
        }

        /// <summary>
        /// Resolves the giver's account amounts into charge allocations, mapping each account to its
        /// campus-specific child account when campus mapping applies. Amounts of zero or less are dropped.
        /// </summary>
        /// <param name="accountAmounts">The per-account amounts the giver entered.</param>
        /// <param name="campusGuid">The Guid of the selected campus, used for campus-child mapping.</param>
        /// <returns>The account allocations to charge.</returns>
        private List<FinancialTransactionService.AccountAllocation> GetMappedAccountAllocations( List<UtilityPaymentEntryAccountAmountBag> accountAmounts, Guid? campusGuid )
        {
            var allocations = new List<FinancialTransactionService.AccountAllocation>();

            if ( accountAmounts == null )
            {
                return allocations;
            }

            var campus = campusGuid.HasValue ? CampusCache.Get( campusGuid.Value ) : null;
            var isCampusAccountMappingEnabled = GetAttributeValue( AttributeKey.CampusAccountMapping ).AsBoolean();

            foreach ( var accountAmount in accountAmounts )
            {
                if ( !accountAmount.AccountGuid.HasValue || accountAmount.Amount <= 0 )
                {
                    continue;
                }

                var account = FinancialAccountCache.Get( accountAmount.AccountGuid.Value );

                if ( account == null )
                {
                    continue;
                }

                // Map to the campus-specific child account when campus mapping applies.
                var shouldMap = isCampusAccountMappingEnabled || account.UsesCampusChildAccounts;
                var mappedAccountId = shouldMap
                    ? account.GetMappedAccountForCampus( campus, forceChildAccounts: true ).Id
                    : account.Id;

                allocations.Add( new FinancialTransactionService.AccountAllocation( mappedAccountId, accountAmount.Amount ) );
            }

            return allocations;
        }

        /// <summary>
        /// Builds the payment info for the charge from the giver's token, amounts, name, and billing
        /// details.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <param name="accountAllocations">The resolved account allocations to charge.</param>
        /// <param name="person">The person authorizing the gift, used for the billing name.</param>
        /// <returns>The payment info to charge.</returns>
        private ReferencePaymentInfo BuildPaymentInfo( UtilityPaymentEntryProcessRequestBag request, List<FinancialTransactionService.AccountAllocation> accountAllocations, Person person, Person targetPerson, out string errorMessage )
        {
            errorMessage = null;
            var transactionType = DefinedValueCache.Get( GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );

            ReferencePaymentInfo paymentInfo;

            if ( request.SavedAccountGuid.HasValue )
            {
                // Charge a reusable saved payment method, verifying it belongs to the target individual
                // first (querying by the individual's id inherently enforces ownership). A saved account
                // deleted since the page loaded surfaces a friendly message instead of a null-reference
                // crash.
                var savedAccount = new FinancialPersonSavedAccountService( RockContext )
                    .GetByPersonId( targetPerson?.Id ?? 0 )
                    .FirstOrDefault( account => account.Guid == request.SavedAccountGuid.Value );

                paymentInfo = savedAccount?.GetReferencePayment();

                if ( paymentInfo == null )
                {
                    errorMessage = "The selected saved payment method is no longer available. Please choose another payment method.";

                    return null;
                }
            }
            else
            {
                paymentInfo = new ReferencePaymentInfo
                {
                    ReferenceNumber = request.GatewayToken
                };
            }

            paymentInfo.Amount = accountAllocations.Sum( allocation => allocation.Amount );
            paymentInfo.AccountAllocations = accountAllocations;
            paymentInfo.Email = request.Email;
            paymentInfo.Phone = PhoneNumber.FormattedNumber( request.PhoneCountryCode, request.Phone, true );
            paymentInfo.TransactionTypeValueId = transactionType.Id;

            // Always send FirstName/LastName (for a business, LastName holds the business name); the
            // gateway rejects a null last name on the billing address. A business also sends BusinessName.
            if ( IsGivingAsBusiness( request.IsGivingAsBusiness ) )
            {
                paymentInfo.BusinessName = person.LastName;
            }

            paymentInfo.FirstName = person.FirstName;
            paymentInfo.LastName = person.LastName;

            if ( request.Address != null )
            {
                paymentInfo.Street1 = request.Address.Street1;
                paymentInfo.Street2 = request.Address.Street2;
                paymentInfo.City = request.Address.City;
                paymentInfo.State = request.Address.State;
                paymentInfo.PostalCode = request.Address.PostalCode;
                paymentInfo.Country = request.Address.Country;
            }

            return paymentInfo;
        }

        /// <summary>
        /// Sets the payment comment from the Payment Comment Template, appending the giver's own comment
        /// when comment entry is enabled.
        /// </summary>
        /// <param name="paymentInfo">The payment info whose comment is set.</param>
        /// <param name="request">The gift details entered by the giver.</param>
        private void ComposePaymentComment( ReferencePaymentInfo paymentInfo, UtilityPaymentEntryProcessRequestBag request )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "TransactionDateTime", RockDateTime.Now );

            // TODO: {{ CurrencyType }} (whether the giver paid by card or bank) comes out blank here. We
            // don't find out how they paid until the payment company processes the charge, and we build
            // this comment beforehand so it can be sent along with that charge. To fill it in, we would
            // rebuild the saved copy of the comment after the charge goes through, once the type is known.
            mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );

            var campusId = request.CampusGuid.HasValue ? CampusCache.Get( request.CampusGuid.Value )?.Id : null;
            var accountDetails = paymentInfo.AccountAllocations
                .Select( allocation => new TransactionAccountDetailInfo( allocation.AccountId, allocation.Amount, campusId ) )
                .ToList();

            mergeFields.Add( "TransactionAccountDetails", accountDetails );

            var paymentComment = GetAttributeValue( AttributeKey.PaymentCommentTemplate ).ResolveMergeFields( mergeFields );

            if ( !GetAttributeValue( AttributeKey.AllowCommentEntry ).AsBoolean() )
            {
                paymentInfo.Comment1 = paymentComment;
            }
            else if ( paymentComment.IsNotNullOrWhiteSpace() )
            {
                paymentInfo.Comment1 = $"{paymentComment}: {request.Comment}";
            }
            else
            {
                paymentInfo.Comment1 = request.Comment;
            }
        }

        /// <summary>
        /// Builds the successful processing result: the resolved Success Page template and footer, the
        /// transaction code, and whether to offer saving the payment method on the success step, plus the
        /// gateway details the shared save-account control needs.
        /// </summary>
        /// <param name="request">The processed gift, used to detect a business or saved-account gift.</param>
        /// <param name="financialGateway">The gateway that processed the gift.</param>
        /// <param name="paymentInfo">The payment info that was charged, carrying the reusable customer reference.</param>
        /// <param name="transactionCode">The gateway's confirmation code for the completed transaction.</param>
        /// <returns>The success result.</returns>
        private UtilityPaymentEntryProcessResponseBag BuildSuccessResponse( UtilityPaymentEntryProcessRequestBag request, FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, string transactionCode )
        {
            // Reload the saved transaction from a fresh context with the Account and CurrencyTypeValue
            // navigations eager-loaded. The just-charged transaction is a new in-memory entity whose
            // reference navigations were never populated (only their foreign keys were set) and cannot
            // lazy-load, so the Success Lava's account names and payment method would otherwise be blank.
            using ( var rockContext = new RockContext() )
            {
                var transaction = new FinancialTransactionService( rockContext )
                    .Queryable( "TransactionDetails.Account,FinancialPaymentDetail.CurrencyTypeValue" )
                    .FirstOrDefault( t => t.Guid == request.TransactionGuid );
                var mergeFields = GetSuccessMergeFields( transaction, transaction?.AuthorizedPersonAliasId, transaction?.FinancialPaymentDetail );

                // Offer to save the payment method only for a personal gift entered with a new payment method
                // on a gateway that can store it, and never in Text-to-Give mode (which saves automatically).
                var isSaveAccountOffered = !GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean()
                    && !IsGivingAsBusiness( request.IsGivingAsBusiness )
                    && !request.SavedAccountGuid.HasValue
                    && transactionCode.IsNotNullOrWhiteSpace()
                    && CanCreateLoginIfNeeded()
                    && IsPaymentMethodSavable( financialGateway.GetGatewayComponent(), transaction?.FinancialPaymentDetail );

                return new UtilityPaymentEntryProcessResponseBag
                {
                    IsSuccess = true,
                    SuccessHtml = GetAttributeValue( AttributeKey.SuccessPageTemplate ).ResolveMergeFields( mergeFields ),
                    SuccessFooterHtml = GetAttributeValue( AttributeKey.SuccessPageFooter ).ResolveMergeFields( mergeFields ),
                    TransactionCode = transactionCode,
                    IsSaveAccountOffered = isSaveAccountOffered,
                    GatewayGuid = financialGateway.Guid,
                    GatewayPersonIdentifier = paymentInfo?.GatewayPersonIdentifier
                };
            }
        }

        /// <summary>
        /// Builds the successful processing result for a scheduled gift: the resolved Success Page template
        /// and footer, the schedule's transaction code, and whether to offer saving the payment method on the
        /// success step, plus the gateway details the shared save-account control needs.
        /// </summary>
        /// <param name="request">The processed gift, used to detect a business or saved-account gift.</param>
        /// <param name="financialGateway">The gateway the schedule was created on.</param>
        /// <param name="paymentInfo">The payment info that was scheduled, carrying the reusable customer reference.</param>
        /// <returns>The success result.</returns>
        private UtilityPaymentEntryProcessResponseBag BuildScheduledSuccessResponse( UtilityPaymentEntryProcessRequestBag request, FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo )
        {
            // Reload with the Account and CurrencyTypeValue navigations eager-loaded, for the same reason
            // as BuildSuccessResponse (a new in-memory entity's reference navigations are blank and cannot
            // lazy-load, which would leave the Success Lava's account names and payment method empty).
            using ( var rockContext = new RockContext() )
            {
                var scheduledTransaction = new FinancialScheduledTransactionService( rockContext )
                    .Queryable( "ScheduledTransactionDetails.Account,FinancialPaymentDetail.CurrencyTypeValue" )
                    .FirstOrDefault( t => t.Guid == request.TransactionGuid );
                var paymentDetail = scheduledTransaction?.FinancialPaymentDetail;
                var mergeFields = GetSuccessMergeFields( scheduledTransaction, scheduledTransaction?.AuthorizedPersonAliasId, paymentDetail );

                var isSaveAccountOffered = !GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean()
                    && !IsGivingAsBusiness( request.IsGivingAsBusiness )
                    && !request.SavedAccountGuid.HasValue
                    && scheduledTransaction?.TransactionCode.IsNotNullOrWhiteSpace() == true
                    && CanCreateLoginIfNeeded()
                    && IsPaymentMethodSavable( financialGateway.GetGatewayComponent(), paymentDetail );

                return new UtilityPaymentEntryProcessResponseBag
                {
                    IsSuccess = true,
                    SuccessHtml = GetAttributeValue( AttributeKey.SuccessPageTemplate ).ResolveMergeFields( mergeFields ),
                    SuccessFooterHtml = GetAttributeValue( AttributeKey.SuccessPageFooter ).ResolveMergeFields( mergeFields ),
                    TransactionCode = scheduledTransaction?.TransactionCode,
                    IsSaveAccountOffered = isSaveAccountOffered,
                    GatewayGuid = financialGateway.Guid,
                    GatewayPersonIdentifier = paymentInfo?.GatewayPersonIdentifier,
                    ScheduledTransactionGuid = request.TransactionGuid
                };
            }
        }

        /// <summary>
        /// Whether a login can be created for the giver if saving a payment method needs one. A signed-in
        /// giver already has a session; an anonymous giver can only be offered the save when Database
        /// authentication is active, since the shared save endpoint refuses to create an orphaned login on a
        /// Passwordless-only site (issue #6877).
        /// </summary>
        /// <returns>True when the save-account offer will not require a login the site cannot create.</returns>
        private bool CanCreateLoginIfNeeded()
        {
            var isAnonymousGiver = RequestContext.CurrentPerson == null;
            var isDatabaseAuthActive = AuthenticationContainer.GetComponent( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE )?.IsActive == true;

            return !isAnonymousGiver || isDatabaseAuthActive;
        }

        /// <summary>
        /// Determines whether the gateway can store the completed gift's payment method as a saved account.
        /// </summary>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="paymentDetail">The completed gift's payment detail whose method would be saved.</param>
        /// <returns>True when the payment method's currency type is savable on the gateway.</returns>
        private bool IsPaymentMethodSavable( GatewayComponent financialGatewayComponent, FinancialPaymentDetail paymentDetail )
        {
            var currencyTypeValueId = paymentDetail?.CurrencyTypeValueId;

            if ( currencyTypeValueId == null )
            {
                return false;
            }

            var currencyType = DefinedValueCache.Get( currencyTypeValueId.Value );

            return currencyType != null && financialGatewayComponent.SupportsSavedAccount( currencyType );
        }

        /// <summary>
        /// Configures Text-to-Give for the completed gift so the giver can repeat it by text. When the gift
        /// used an existing saved account, Text-to-Give points at it; otherwise a saved account is created
        /// automatically (no manual prompt) and configured.
        /// </summary>
        /// <param name="transaction">The completed transaction whose payment method backs Text-to-Give.</param>
        /// <param name="person">The individual Text-to-Give is set up for.</param>
        /// <param name="request">The processed gift, used to detect an existing saved-account gift.</param>
        private void ConfigureTextToGiveAccount( FinancialTransaction transaction, Person person, UtilityPaymentEntryProcessRequestBag request )
        {
            if ( person == null || transaction?.FinancialPaymentDetail == null )
            {
                return;
            }

            if ( request.SavedAccountGuid.HasValue )
            {
                // The giver charged an existing saved account, so point Text-to-Give at it rather than
                // saving a duplicate.
                var savedAccount = new FinancialPersonSavedAccountService( RockContext )
                    .GetByPersonId( person.Id )
                    .FirstOrDefault( account => account.Guid == request.SavedAccountGuid.Value );

                if ( savedAccount != null )
                {
                    var contributionAccountId = transaction.TransactionDetails.FirstOrDefault()?.AccountId;
                    new PersonService( RockContext ).ConfigureTextToGive( person.Id, contributionAccountId, savedAccount.Id, out _ );
                    RockContext.SaveChanges();
                }

                return;
            }

            // A new payment method: create an automatically named saved account and configure Text-to-Give.
            CreateTextToGiveSavedAccount( transaction, person, BuildTextToGiveAccountName( transaction.FinancialPaymentDetail ) );
        }

        /// <summary>
        /// Builds the automatic saved-account name for a Text-to-Give gift, describing the card or bank
        /// account used (matching the legacy "Text-To-Give - ..." naming).
        /// </summary>
        /// <param name="paymentDetail">The completed gift's payment detail.</param>
        /// <returns>The saved-account name.</returns>
        private static string BuildTextToGiveAccountName( FinancialPaymentDetail paymentDetail )
        {
            var currencyTypeValueId = paymentDetail?.CurrencyTypeValueId;
            var creditCardCurrencyTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrencyTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );

            if ( currencyTypeValueId == creditCardCurrencyTypeId )
            {
                var cardType = paymentDetail?.CreditCardTypeValue?.Value;
                var lastFour = paymentDetail?.AccountNumberMasked.Right( 4 );

                return $"Text-To-Give - {cardType} (ending in {lastFour})";
            }

            if ( currencyTypeValueId == achCurrencyTypeId )
            {
                var lastFour = paymentDetail?.AccountNumberMasked.Right( 4 );

                return $"Text-To-Give - ACH (ending in {lastFour})";
            }

            if ( paymentDetail?.CurrencyTypeValue != null )
            {
                return $"Text-To-Give - {paymentDetail.CurrencyTypeValue.Value}";
            }

            return "Text-To-Give";
        }

        /// <summary>
        /// Creates a reusable saved account from the completed transaction's payment method and configures
        /// Text-to-Give against it, for the Text-to-Give success path where the giver entered a new payment
        /// method (there is no manual prompt). The manual save-account offer instead uses the shared
        /// SaveFinancialAccountForm control and endpoint.
        /// </summary>
        /// <param name="transaction">The completed transaction whose payment method is saved.</param>
        /// <param name="person">The individual the saved account belongs to.</param>
        /// <param name="accountName">The name for the saved account.</param>
        private void CreateTextToGiveSavedAccount( FinancialTransaction transaction, Person person, string accountName )
        {
            var financialGateway = GetConfiguredFinancialGateway();
            var financialGatewayComponent = financialGateway?.GetGatewayComponent();

            if ( financialGatewayComponent == null || person?.PrimaryAliasId == null || transaction?.FinancialPaymentDetail == null )
            {
                return;
            }

            // GetReferenceNumber reads the transaction's gateway attributes, so load them on that gateway.
            transaction.FinancialGateway?.LoadAttributes( RockContext );
            var referenceNumber = financialGatewayComponent.GetReferenceNumber( transaction, out var referenceError );

            if ( referenceError.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            var paymentDetail = transaction.FinancialPaymentDetail;
            var savedAccount = new FinancialPersonSavedAccount
            {
                PersonAliasId = person.PrimaryAliasId,
                ReferenceNumber = referenceNumber,
                Name = accountName,
                TransactionCode = transaction.TransactionCode,
                GatewayPersonIdentifier = paymentDetail.GatewayPersonIdentifier,
                FinancialGatewayId = financialGateway.Id,
                FinancialPaymentDetail = new FinancialPaymentDetail
                {
                    AccountNumberMasked = paymentDetail.AccountNumberMasked,
                    CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId,
                    NameOnCard = paymentDetail.NameOnCard,
                    ExpirationMonth = paymentDetail.ExpirationMonth,
                    ExpirationYear = paymentDetail.ExpirationYear,
                    BillingLocationId = paymentDetail.BillingLocationId
                }
            };

            new FinancialPersonSavedAccountService( RockContext ).Add( savedAccount );

            // Save once here to assign the saved account's database Id, which the payment-detail link and
            // ConfigureTextToGive both need; the link and the Text-to-Give configuration then persist
            // together in the final save.
            RockContext.SaveChanges();

            paymentDetail.FinancialPersonSavedAccountId = savedAccount.Id;

            var contributionAccountId = transaction.TransactionDetails.FirstOrDefault()?.AccountId;
            new PersonService( RockContext ).ConfigureTextToGive( person.Id, contributionAccountId, savedAccount.Id, out _ );
            RockContext.SaveChanges();
        }

        /// <summary>
        /// Builds the merge fields the success-step templates resolve against: the common fields plus the
        /// completed transaction, its authorized person, payment detail, billing location, and the
        /// transaction entity. The completed transaction is a one-time <see cref="FinancialTransaction"/> or a
        /// <see cref="FinancialScheduledTransaction"/>; both are surfaced under the "Transaction" merge field,
        /// matching the legacy templates.
        /// </summary>
        /// <param name="transaction">The completed transaction, already fetched by the caller.</param>
        /// <param name="authorizedPersonAliasId">The alias id of the individual the gift is authorized to.</param>
        /// <param name="paymentDetail">The completed transaction's payment detail.</param>
        /// <returns>The merge fields for the success templates.</returns>
        private Dictionary<string, object> GetSuccessMergeFields( object transaction, int? authorizedPersonAliasId, FinancialPaymentDetail paymentDetail )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            AddTransactionHeaderMergeFields( mergeFields );

            // The success template switches its confirmation message to the Text-to-Give wording in that mode.
            mergeFields.Add( "IsTextToGive", GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean() );

            if ( transaction != null )
            {
                mergeFields.Add( "Transaction", transaction );

                // A completed one-time gift is also exposed under the legacy FinancialTransaction merge field
                // name, which customized success templates may reference (a scheduled gift has none).
                if ( transaction is FinancialTransaction financialTransaction )
                {
                    mergeFields["FinancialTransaction"] = financialTransaction;
                }

                if ( authorizedPersonAliasId.HasValue )
                {
                    mergeFields.Add( "Person", new PersonAliasService( RockContext ).GetPerson( authorizedPersonAliasId.Value ) );
                }

                if ( paymentDetail != null )
                {
                    mergeFields.Add( "PaymentDetail", paymentDetail );

                    if ( paymentDetail.BillingLocation != null || paymentDetail.BillingLocationId.HasValue )
                    {
                        mergeFields.Add( "BillingLocation", paymentDetail.BillingLocation ?? new LocationService( RockContext ).GetNoTracking( paymentDetail.BillingLocationId.Value ) );
                    }
                }
            }

            return mergeFields;
        }

        /// <summary>
        /// Charges the tokenized payment method. If the token was already charged (for example on a retry),
        /// the existing transaction is fetched instead of charging again.
        /// </summary>
        /// <param name="financialGateway">The configured financial gateway.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="gatewayToken">The payment token the client tokenized.</param>
        /// <param name="paymentInfo">The payment info to charge.</param>
        /// <param name="errorMessage">The error message when the charge fails.</param>
        /// <returns>The charged transaction, or null when the charge failed.</returns>
        private FinancialTransaction ChargePayment( FinancialGateway financialGateway, GatewayComponent financialGatewayComponent, string gatewayToken, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( financialGatewayComponent is IObsidianHostedGatewayComponent obsidianGatewayComponent
                && gatewayToken.IsNotNullOrWhiteSpace()
                && obsidianGatewayComponent.IsPaymentTokenCharged( financialGateway, gatewayToken ) )
            {
                /*
                    7/2/2026 - JPH

                    We only reach this branch when the gateway (the payment company) already charged the
                    card but Rock never saved a record of it, for example if the connection dropped right
                    after the charge went through. Instead of charging the card a second time, we fetch
                    the charge that already happened. That part is good: it prevents a double charge.

                    The per-account split we save comes from the giver's form, and that is the correct
                    split: the amounts and the payment token were sent together in one request, so the
                    charge was for the sum of those same amounts. We cannot ask the gateway for a split
                    (it only reports the single total), but we do not need to. The one thing that could
                    drift is the fetched total not matching the sum of those amounts (a replayed token, or
                    a gateway that changed the amount), so we compare the two and log a mismatch for staff
                    to reconcile rather than silently save numbers that do not add up to what was charged.

                    Reason: On charge-recovery the form's split is authoritative; log only on a total mismatch.
                */
                var recoveredTransaction = obsidianGatewayComponent.FetchPaymentTokenTransaction( RockContext, financialGateway, null, gatewayToken );

                if ( recoveredTransaction != null && recoveredTransaction.TotalAmount != paymentInfo.Amount )
                {
                    ExceptionLogService.LogException( new Exception( $"Utility Payment Entry recovered an already-charged payment totaling {recoveredTransaction.TotalAmount.FormatAsCurrency()}, but the entered gift totals {paymentInfo.Amount.FormatAsCurrency()}. The saved per-account amounts may not match the amount charged." ) );
                }

                return recoveredTransaction;
            }

            return financialGatewayComponent.Charge( financialGateway, paymentInfo, out errorMessage );
        }

        /// <summary>
        /// Saves the charged transaction: sets its authorized person, payment detail, source, and details,
        /// adds it to a batch, records batch history, and queues the receipt email.
        /// </summary>
        /// <param name="financialGateway">The configured financial gateway.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="person">The person authorizing the gift.</param>
        /// <param name="paymentInfo">The payment info that was charged.</param>
        /// <param name="accountAllocations">The account allocations to record as transaction details.</param>
        /// <param name="isAnonymous">Whether the gift should be recorded as anonymous.</param>
        /// <param name="transaction">The charged transaction to save.</param>
        private void SaveTransaction( FinancialGateway financialGateway, GatewayComponent financialGatewayComponent, Person person, ReferencePaymentInfo paymentInfo, List<FinancialTransactionService.AccountAllocation> accountAllocations, bool isAnonymous, FinancialTransaction transaction )
        {
            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            transaction.ShowAsAnonymous = isAnonymous;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;

            var transactionType = DefinedValueCache.Get( GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = transactionType.Id;
            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, financialGatewayComponent, RockContext );

            var sourceGuid = GetAttributeValue( AttributeKey.TransactionSource ).AsGuidOrNull();

            if ( sourceGuid.HasValue )
            {
                transaction.SourceTypeValueId = DefinedValueCache.Get( sourceGuid.Value )?.Id;
            }

            var transactionEntity = GetTransactionEntity();

            // Replace any details carried by a recovered charge with our authoritative allocations.
            transaction.TransactionDetails.Clear();

            foreach ( var allocation in accountAllocations )
            {
                var transactionDetail = new FinancialTransactionDetail
                {
                    AccountId = allocation.AccountId,
                    Amount = allocation.Amount
                };

                if ( transactionEntity != null )
                {
                    transactionDetail.EntityTypeId = transactionEntity.TypeId;
                    transactionDetail.EntityId = transactionEntity.Id;
                }

                transaction.TransactionDetails.Add( transactionDetail );
            }

            // Set any allow-listed transaction attributes supplied on the URL; their values are saved
            // once the transaction has an Id.
            var hasUrlTransactionAttributes = ApplyUrlTransactionAttributes( transaction );

            var batchService = new FinancialBatchService( RockContext );
            var batch = batchService.GetForNewTransaction( transaction, GetAttributeValue( AttributeKey.BatchNamePrefix ) );

            var batchChanges = new History.HistoryChangeList();
            FinancialBatchService.EvaluateNewBatchHistory( batch, batchChanges );

            // Save the batch first when it is new so it has an Id to attach the transaction to.
            if ( batch.Id == 0 )
            {
                RockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;
            new FinancialTransactionService( RockContext ).Add( transaction );
            RockContext.SaveChanges();

            if ( hasUrlTransactionAttributes )
            {
                transaction.SaveAttributeValues( RockContext );
            }

            batchService.IncrementControlAmount( batch.Id, transaction.TotalAmount, batchChanges );
            RockContext.SaveChanges();

            Task.Run( () => GiftWasGivenMessage.PublishTransactionEvent( transaction.Id, GiftEventTypes.GiftSuccess ) );

            HistoryService.SaveChanges(
                RockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges );

            SendReceipt( transaction.Id );
        }

        /// <summary>
        /// Sets the transaction attributes carried on the URL as "Attribute_{key}" page parameters onto
        /// the transaction. Only attributes named in the Transaction Attributes from URL setting are
        /// honored, so the URL cannot set an attribute the block did not allow. The attributes are loaded
        /// and their values set in memory here; the caller saves them after the transaction is persisted.
        /// </summary>
        /// <param name="transaction">The transaction to set attributes on; its attributes are loaded here.</param>
        /// <returns>True when the feature is configured (attributes were loaded and the caller should save
        /// their values); false when no transaction attributes are allow-listed.</returns>
        private bool ApplyUrlTransactionAttributes( FinancialTransaction transaction )
        {
            var allowedAttributeKeys = GetAttributeValue( AttributeKey.TransactionAttributesFromURL )
                .Split( ',' )
                .AsGuidList()
                .Select( attributeGuid => AttributeCache.Get( attributeGuid )?.Key )
                .Where( attributeKey => attributeKey.IsNotNullOrWhiteSpace() )
                .ToList();

            if ( !allowedAttributeKeys.Any() )
            {
                return false;
            }

            transaction.LoadAttributes( RockContext );

            var pageParameters = RequestContext.GetPageParameters();

            foreach ( var attributeKey in allowedAttributeKeys )
            {
                var parameterName = PageParameterKey.AttributePrefix + attributeKey;

                if ( pageParameters.ContainsKey( parameterName ) )
                {
                    // Page parameter values arrive already URL-decoded, so they are used as-is.
                    transaction.SetAttributeValue( attributeKey, pageParameters[parameterName] );
                }
            }

            return true;
        }

        /// <summary>
        /// Builds the payment schedule for the gift, or returns null for an immediate one-time gift. A
        /// one-time frequency dated today or earlier is immediate; a recurring frequency, or a future-dated
        /// one-time gift, is scheduled. Text-to-Give forces an immediate gift (scheduling is disabled in that
        /// mode).
        /// </summary>
        /// <param name="frequencyGuid">The selected transaction-frequency defined value Guid, or null for One-Time.</param>
        /// <param name="startDateValue">The entered start date as an ISO date string.</param>
        /// <param name="endDateValue">The entered end date as an ISO date string, applied only to a recurring gift.</param>
        /// <param name="financialGateway">The configured financial gateway.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component, used to resolve the earliest
        /// allowed scheduled start date.</param>
        /// <returns>The payment schedule, or null for an immediate one-time gift.</returns>
        private PaymentSchedule GetSchedule( Guid? frequencyGuid, string startDateValue, string endDateValue, FinancialGateway financialGateway, GatewayComponent financialGatewayComponent )
        {
            if ( !GetAttributeValue( AttributeKey.AllowScheduledGifts ).AsBoolean() || GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean() )
            {
                return null;
            }

            var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
            var frequency = ( frequencyGuid.HasValue ? DefinedValueCache.Get( frequencyGuid.Value ) : null ) ?? oneTimeFrequency;
            var isOneTime = frequency.Id == oneTimeFrequency.Id;
            var startDate = startDateValue.AsDateTime();

            // A one-time gift dated today or earlier is charged immediately, not scheduled.
            if ( isOneTime && ( !startDate.HasValue || startDate.Value <= RockDateTime.Today ) )
            {
                return null;
            }

            DateTime scheduleStartDate;

            if ( isOneTime )
            {
                // A future-dated one-time gift starts on the chosen date.
                scheduleStartDate = startDate.Value;
            }
            else
            {
                // A recurring gift cannot start before the gateway's earliest scheduled date (it may already
                // have run today's automated giving), so clamp the start forward to that date. Sending an
                // earlier date makes a hosted gateway reject the subscription.
                var earliestStartDate = ( financialGatewayComponent as IHostedGatewayComponent )?.GetEarliestScheduledStartDate( financialGateway ) ?? RockDateTime.Today;
                scheduleStartDate = startDate.HasValue && startDate.Value > earliestStartDate ? startDate.Value : earliestStartDate;
            }

            var schedule = new PaymentSchedule
            {
                TransactionFrequencyValue = frequency,
                StartDate = scheduleStartDate
            };

            if ( GetAttributeValue( AttributeKey.AllowScheduledEndDate ).AsBoolean() && !isOneTime )
            {
                var endDate = endDateValue.AsDateTime();

                if ( endDate.HasValue && endDate.Value > RockDateTime.Today )
                {
                    schedule.EndDate = endDate.Value;
                }
            }

            return schedule;
        }

        /// <summary>
        /// Creates the scheduled transaction with the gateway and saves it. The schedule is owned by the
        /// contact individual, while the scheduled transaction is authorized to the giver (the business for a
        /// business gift). A schedule already existing for this idempotency Guid short-circuits to success
        /// without scheduling again.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <param name="schedule">The payment schedule to create.</param>
        /// <param name="financialGateway">The configured financial gateway.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="person">The individual or business the gift is authorized to.</param>
        /// <param name="contactPerson">The individual who owns the schedule.</param>
        /// <param name="accountAllocations">The campus-mapped account allocations to record as scheduled details.</param>
        /// <param name="paymentInfo">The payment info to schedule.</param>
        /// <param name="scheduledTransactionToTransfer">The schedule being transferred, cancelled once the new schedule is saved; null when not transferring.</param>
        /// <returns>The processing result: the success HTML, or an error message to show on the entry step.</returns>
        private UtilityPaymentEntryProcessResponseBag ProcessScheduledTransaction( UtilityPaymentEntryProcessRequestBag request, PaymentSchedule schedule, FinancialGateway financialGateway, GatewayComponent financialGatewayComponent, Person person, Person contactPerson, List<FinancialTransactionService.AccountAllocation> accountAllocations, ReferencePaymentInfo paymentInfo, FinancialScheduledTransaction scheduledTransactionToTransfer )
        {
            // The schedule is owned by the contact individual even when the gift is authorized to a business.
            schedule.PersonId = contactPerson.Id;

            // Guard against a duplicate schedule: if one with this Guid already exists, show success without
            // scheduling again.
            var existingSchedule = new FinancialScheduledTransactionService( RockContext ).Queryable()
                .FirstOrDefault( scheduledTransaction => scheduledTransaction.Guid == request.TransactionGuid );

            if ( existingSchedule != null )
            {
                return BuildScheduledSuccessResponse( request, financialGateway, paymentInfo );
            }

            var scheduledTransaction = financialGatewayComponent.AddScheduledPayment( financialGateway, schedule, paymentInfo, out var scheduleError );

            if ( scheduledTransaction == null )
            {
                return ProcessError( scheduleError.IsNotNullOrWhiteSpace() ? scheduleError : "There was a problem scheduling the payment." );
            }

            // Assign the client-minted Guid so a retry cannot create a duplicate schedule.
            scheduledTransaction.Guid = request.TransactionGuid;

            SaveScheduledTransaction( financialGateway, financialGatewayComponent, person, paymentInfo, schedule, accountAllocations, scheduledTransaction );

            // A transfer replaces an existing schedule, so cancel the old one now that the new one is saved.
            if ( scheduledTransactionToTransfer != null )
            {
                CancelTransferredScheduledTransaction( scheduledTransactionToTransfer.Id );
            }

            return BuildScheduledSuccessResponse( request, financialGateway, paymentInfo );
        }

        /// <summary>
        /// Cancels the scheduled transaction that a transfer replaced, at the gateway and in Rock. Best-effort:
        /// a status-refresh failure is ignored so a gateway hiccup does not fail the completed new schedule.
        /// </summary>
        /// <param name="scheduledTransactionId">The id of the old scheduled transaction to cancel.</param>
        private void CancelTransferredScheduledTransaction( int scheduledTransactionId )
        {
            using ( var rockContext = new RockContext() )
            {
                var scheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                var scheduledTransaction = scheduledTransactionService.Get( scheduledTransactionId );

                if ( scheduledTransaction?.FinancialGateway != null )
                {
                    scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                }

                if ( scheduledTransactionService.Cancel( scheduledTransaction, out _ ) )
                {
                    try
                    {
                        scheduledTransactionService.GetStatus( scheduledTransaction, out _ );
                    }
                    catch
                    {
                        // Intentionally ignored: the cancel succeeded; a status refresh is best-effort.
                    }

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Saves the scheduled transaction: sets its frequency, dates, authorized person, payment detail,
        /// source, and details, then persists it and publishes the scheduled-gift event. Unlike a one-time
        /// gift, a schedule has no batch, receipt, or immediate charge.
        /// </summary>
        /// <param name="financialGateway">The configured financial gateway.</param>
        /// <param name="financialGatewayComponent">The resolved gateway component.</param>
        /// <param name="person">The individual or business the gift is authorized to.</param>
        /// <param name="paymentInfo">The payment info that was scheduled.</param>
        /// <param name="schedule">The payment schedule to record.</param>
        /// <param name="accountAllocations">The campus-mapped account allocations to record as scheduled details.</param>
        /// <param name="scheduledTransaction">The scheduled transaction the gateway created.</param>
        private void SaveScheduledTransaction( FinancialGateway financialGateway, GatewayComponent financialGatewayComponent, Person person, ReferencePaymentInfo paymentInfo, PaymentSchedule schedule, List<FinancialTransactionService.AccountAllocation> accountAllocations, FinancialScheduledTransaction scheduledTransaction )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.EndDate = schedule.EndDate;
            scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            var transactionType = DefinedValueCache.Get( GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            scheduledTransaction.TransactionTypeValueId = transactionType.Id;
            scheduledTransaction.Summary = paymentInfo.Comment1;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, financialGatewayComponent, RockContext );

            var sourceGuid = GetAttributeValue( AttributeKey.TransactionSource ).AsGuidOrNull();

            if ( sourceGuid.HasValue )
            {
                scheduledTransaction.SourceTypeValueId = DefinedValueCache.Get( sourceGuid.Value )?.Id;
            }

            var transactionEntity = GetTransactionEntity();

            // Replace any details the gateway seeded with our authoritative campus-mapped allocations.
            scheduledTransaction.ScheduledTransactionDetails.Clear();

            foreach ( var allocation in accountAllocations )
            {
                var scheduledTransactionDetail = new FinancialScheduledTransactionDetail
                {
                    AccountId = allocation.AccountId,
                    Amount = allocation.Amount
                };

                if ( transactionEntity != null )
                {
                    scheduledTransactionDetail.EntityTypeId = transactionEntity.TypeId;
                    scheduledTransactionDetail.EntityId = transactionEntity.Id;
                }

                scheduledTransaction.ScheduledTransactionDetails.Add( scheduledTransactionDetail );
            }

            new FinancialScheduledTransactionService( RockContext ).Add( scheduledTransaction );
            RockContext.SaveChanges();

            Task.Run( () => ScheduledGiftWasModifiedMessage.PublishScheduledTransactionEvent( scheduledTransaction.Id, ScheduledGiftEventTypes.ScheduledGiftCreated ) );
        }

        /// <summary>
        /// Resolves the entity a transaction detail is associated with, from the Transaction Entity Type
        /// setting and the configured entity-id page parameter. Returns null when no entity is configured
        /// or resolved.
        /// </summary>
        /// <returns>The resolved transaction entity, or null.</returns>
        private IEntity GetTransactionEntity()
        {
            var transactionEntityTypeGuid = GetAttributeValue( AttributeKey.TransactionEntityType ).AsGuidOrNull();

            if ( !transactionEntityTypeGuid.HasValue )
            {
                return null;
            }

            var transactionEntityType = EntityTypeCache.Get( transactionEntityTypeGuid.Value );

            if ( transactionEntityType == null )
            {
                return null;
            }

            var entityKey = PageParameter( GetAttributeValue( AttributeKey.EntityIdParameter ) );

            if ( entityKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return Rock.Reflection.GetIEntityForEntityType( transactionEntityType.GetEntityType(), entityKey, AllowIntegerIdentifiers, RockContext );
        }

        /// <summary>
        /// Queues the receipt email for the transaction when a Receipt Email is configured.
        /// </summary>
        /// <param name="transactionId">The id of the transaction to send a receipt for.</param>
        private void SendReceipt( int transactionId )
        {
            var receiptEmailGuid = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();

            if ( !receiptEmailGuid.HasValue )
            {
                return;
            }

            new ProcessSendPaymentReceiptEmails.Message
            {
                SystemEmailGuid = receiptEmailGuid.Value,
                TransactionId = transactionId
            }.Send();
        }

        /// <summary>
        /// Resolves the individual a personal (non-business) gift is authorized to: the target individual
        /// when one is resolved, otherwise an existing individual matched by the entered name, email, and
        /// phone, otherwise a newly created individual. A nameless placeholder record is replaced by a real
        /// individual and merged in, and the entered contact details are written back to the resolved
        /// individual.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <param name="targetPerson">The individual resolved from the sign-in / impersonation context, or null.</param>
        /// <returns>The individual the gift is authorized to, or null when none could be resolved or created.</returns>
        private Person ResolveIndividualGiver( UtilityPaymentEntryProcessRequestBag request, Person targetPerson )
        {
            var personService = new PersonService( RockContext );
            var person = targetPerson;

            // A nameless placeholder (e.g. from a Give-by-SMS record) is replaced by a real individual and
            // merged in.
            Person namelessPerson = null;

            if ( person != null && person.IsNameless() )
            {
                namelessPerson = person;
                person = null;
            }

            if ( person == null )
            {
                // Match an existing individual by the entered details before creating a new one.
                if ( request.FirstName.IsNotNullOrWhiteSpace() && request.LastName.IsNotNullOrWhiteSpace() && request.Email.IsNotNullOrWhiteSpace() )
                {
                    var personMatchQuery = new PersonService.PersonMatchQuery( request.FirstName, request.LastName, request.Email, request.Phone );
                    person = personService.FindPerson( personMatchQuery, true );
                }

                person = person ?? CreateIndividual( request );

                if ( person != null && namelessPerson != null )
                {
                    personService.MergeNamelessPersonToExistingPerson( namelessPerson, person );
                }
            }

            if ( person != null )
            {
                UpdateGiverContactInfo( person, request, targetPerson );
            }

            return person;
        }

        /// <summary>
        /// Creates a new individual from the entered name, with the configured connection status, record
        /// status, and record source.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <returns>The newly created individual.</returns>
        private Person CreateIndividual( UtilityPaymentEntryProcessRequestBag request )
        {
            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

            var person = new Person
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                ConnectionStatusValueId = connectionStatus?.Id,
                RecordStatusValueId = recordStatus?.Id,
                RecordSourceValueId = GetRecordSourceValueId()
            };

            PersonService.SaveNewPerson( person, RockContext, null, false );

            return person;
        }

        /// <summary>
        /// Gets the record source to assign to a newly created individual: the session record source when
        /// one is set, otherwise the configured Record Source setting.
        /// </summary>
        /// <returns>The record source defined value id, or null.</returns>
        private int? GetRecordSourceValueId()
        {
            return RecordSourceHelper.GetSessionRecordSourceValueId()
                ?? DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordSource ).AsGuid() )?.Id;
        }

        /// <summary>
        /// Writes the entered email, phone, and address back to the resolved individual's record and family.
        /// </summary>
        /// <param name="person">The individual to update.</param>
        /// <param name="request">The gift details entered by the giver.</param>
        private void UpdateGiverContactInfo( Person person, UtilityPaymentEntryProcessRequestBag request, Person targetPerson )
        {
            person.Email = request.Email;

            if ( IsPhoneFieldShown( targetPerson ) )
            {
                var isSmsOptInShown = GetAttributeValue( AttributeKey.SmsOptIn ).AsBoolean();

                UpdateGiverHomePhone( person, request, isSmsOptInShown );
            }

            RockContext.SaveChanges();

            if ( request.Address == null )
            {
                return;
            }

            // For a known individual, update the family whose address of this type is on file; fall back to
            // the primary family (a dedupe match, a new individual, or one with no address of this type).
            var isKnownTarget = targetPerson != null && person.Id == targetPerson.Id;
            var addressType = GetAttributeValue( AttributeKey.AddressType );
            var addressTypeId = DefinedValueCache.Get( addressType.AsGuid() )?.Id;
            var family = ( isKnownTarget && addressTypeId.HasValue ? new PersonService( RockContext ).GetFirstLocation( person.Id, addressTypeId.Value )?.Group : null )
                ?? person.GetFamily( RockContext );

            if ( family == null )
            {
                return;
            }

            GroupService.AddNewGroupAddress(
                RockContext,
                family,
                addressType,
                request.Address.Street1,
                request.Address.Street2,
                request.Address.City,
                request.Address.State,
                request.Address.PostalCode,
                request.Address.Country,
                true
            );
        }

        /// <summary>
        /// Updates the individual's home number from the entered phone, or creates one. Reuses an existing
        /// mobile number that matches the entered number so a duplicate home number is not added.
        /// </summary>
        /// <param name="person">The individual whose phone is updated.</param>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <param name="isSmsOptInShown">Whether the SMS opt-in choice was offered, so its value should be saved.</param>
        private void UpdateGiverHomePhone( Person person, UtilityPaymentEntryProcessRequestBag request, bool isSmsOptInShown )
        {
            var homeNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id;
            var mobileNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

            var cleanCountryCode = PhoneNumber.CleanNumber( request.PhoneCountryCode );
            var cleanNumber = PhoneNumber.CleanNumber( request.Phone );

            var homePhone = person.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == homeNumberTypeId );

            if ( homePhone != null )
            {
                homePhone.CountryCode = cleanCountryCode;
                homePhone.Number = cleanNumber;

                if ( isSmsOptInShown )
                {
                    homePhone.IsMessagingEnabled = request.IsSmsOptIn;
                }

                return;
            }

            // Reuse a matching mobile number rather than adding a duplicate as a home number.
            var mobilePhone = person.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == mobileNumberTypeId && phone.Number == cleanNumber );

            if ( mobilePhone != null )
            {
                if ( isSmsOptInShown )
                {
                    mobilePhone.IsMessagingEnabled = request.IsSmsOptIn;
                }

                return;
            }

            var newHomePhone = new PhoneNumber
            {
                NumberTypeValueId = homeNumberTypeId,
                CountryCode = cleanCountryCode,
                Number = cleanNumber
            };

            if ( isSmsOptInShown )
            {
                newHomePhone.IsMessagingEnabled = request.IsSmsOptIn;
            }

            person.PhoneNumbers.Add( newHomePhone );
        }

        /// <summary>
        /// Builds the merge fields the Confirmation Body Lava resolves against: the common fields plus the
        /// gift summary (per-account rows, total, when, name, email, and address). The account rows use the
        /// campus-mapped account so the names match what will be saved.
        /// </summary>
        /// <param name="request">The gift details the giver entered.</param>
        /// <param name="accountAllocations">The resolved, campus-mapped account allocations.</param>
        /// <returns>The merge fields for the Confirmation Body.</returns>
        private Dictionary<string, object> BuildConfirmationMergeFields( UtilityPaymentEntryConfirmationRequestBag request, List<FinancialTransactionService.AccountAllocation> accountAllocations )
        {
            var campusId = request.CampusGuid.HasValue ? CampusCache.Get( request.CampusGuid.Value )?.Id : null;

            var accountDetails = accountAllocations
                .Select( allocation => new TransactionAccountDetailInfo( allocation.AccountId, allocation.Amount, campusId ) )
                .ToList();

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "AccountDetails", accountDetails );
            mergeFields.Add( "Total", accountAllocations.Sum( allocation => allocation.Amount ) );

            mergeFields.Add( "When", BuildScheduleSummary( request.FrequencyGuid, request.StartDate, request.EndDate ) );
            mergeFields.Add( "Name", IsGivingAsBusiness( request.IsGivingAsBusiness )
                ? request.BusinessName?.Trim() ?? string.Empty
                : $"{request.FirstName} {request.LastName}".Trim() );
            mergeFields.Add( "Email", request.Email );

            var isPhonePrompted = GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();
            mergeFields.Add( "Phone", isPhonePrompted && request.Phone.IsNotNullOrWhiteSpace()
                ? PhoneNumber.FormattedNumber( request.PhoneCountryCode, request.Phone, true )
                : string.Empty );

            mergeFields.Add( "Address", BuildBillingLocation( request.Address ) );

            return mergeFields;
        }

        /// <summary>
        /// Builds the "when" summary for the confirmation: "Today" for an immediate one-time gift, or the
        /// frequency with its start (and optional end) date for a recurring gift.
        /// </summary>
        /// <param name="frequencyGuid">The selected transaction-frequency Guid.</param>
        /// <param name="startDate">The scheduled start date as an ISO date string.</param>
        /// <param name="endDate">The optional scheduled end date as an ISO date string.</param>
        /// <returns>The "when" summary text.</returns>
        private string BuildScheduleSummary( Guid? frequencyGuid, string startDate, string endDate )
        {
            var oneTimeFrequencyGuid = Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid();

            if ( !frequencyGuid.HasValue || frequencyGuid.Value == oneTimeFrequencyGuid )
            {
                return "Today";
            }

            var frequency = DefinedValueCache.Get( frequencyGuid.Value );
            var start = startDate.AsDateTime() ?? RockDateTime.Today;
            var summary = $"{frequency?.Value} starting on {start:d}";
            var end = endDate.AsDateTime();

            if ( end.HasValue )
            {
                summary += $" and ending on {end.Value:d}";
            }

            return summary;
        }

        /// <summary>
        /// Builds a Location from the entered billing address so the confirmation summary can format it
        /// the Rock way, honoring the country's address format (which covers the second street line, the
        /// locality, and non-US layouts). Returns null when no street was entered so the summary omits the
        /// address row. The country falls back to the organization's country when the giver did not choose
        /// one.
        /// </summary>
        /// <param name="address">The entered billing address.</param>
        /// <returns>The billing location, or null when no address was entered.</returns>
        private static Location BuildBillingLocation( AddressControlBag address )
        {
            if ( address == null || address.Street1.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new Location
            {
                Street1 = address.Street1,
                Street2 = address.Street2,
                City = address.City,
                County = address.Locality,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country.IsNullOrWhiteSpace() ? GlobalAttributesCache.Get().OrganizationCountry : address.Country
            };
        }

        /// <summary>
        /// Whether the gift is given on behalf of a business, honored only when business giving is enabled
        /// and the block is not in Text-to-Give mode (never on the client's word alone).
        /// </summary>
        /// <param name="isRequested">The client's Give As Business flag.</param>
        /// <returns>True when the gift should be treated as a business gift.</returns>
        private bool IsGivingAsBusiness( bool isRequested )
        {
            var isTextToGive = GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean();

            return isRequested && !isTextToGive && GetAttributeValue( AttributeKey.AllowBusinessGiving ).AsBoolean();
        }

        /// <summary>
        /// Sets the Give As Business options: whether the option is offered, whether the giver individual is
        /// known (which hides the business-contact fields), and the businesses the giver may give on
        /// behalf of, each with its prefill values.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="targetPerson">The individual the gift is for; their businesses are listed.</param>
        /// <param name="scheduledTransactionToTransfer">The schedule being transferred, used to preselect its business; null when not transferring.</param>
        private void SetBusinessOptions( UtilityPaymentEntryOptionsBag options, Person targetPerson, FinancialScheduledTransaction scheduledTransactionToTransfer )
        {
            var isTextToGive = GetAttributeValue( AttributeKey.TextToGiveMode ).AsBoolean();
            options.IsBusinessGivingAllowed = !isTextToGive && GetAttributeValue( AttributeKey.AllowBusinessGiving ).AsBoolean();

            // Under impersonation the giver is the target individual, not the signed-in admin.
            options.IsGiverIndividualKnown = targetPerson != null;

            // Business phone / SMS visibility follows the raw settings; the person-only unlisted-number flip
            // does not apply to a business.
            var isBusinessPhonePrompted = GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();
            options.IsBusinessPhonePrompted = isBusinessPhonePrompted;
            options.IsBusinessSmsOptInShown = isBusinessPhonePrompted && GetAttributeValue( AttributeKey.SmsOptIn ).AsBoolean();

            if ( !options.IsBusinessGivingAllowed || targetPerson == null )
            {
                return;
            }

            var personService = new PersonService( RockContext );
            var workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() )?.Id;
            var workPhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id;

            options.Businesses = personService.GetBusinesses( targetPerson.Id )
                .ToList()
                .Select( business =>
                {
                    var workPhone = business.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == workPhoneTypeId );
                    var location = workLocationTypeId.HasValue ? personService.GetFirstLocation( business.Id, workLocationTypeId.Value )?.Location : null;

                    return new UtilityPaymentEntryBusinessBag
                    {
                        Guid = business.Guid,
                        Name = business.LastName,
                        Email = business.Email,
                        Phone = workPhone?.Number ?? string.Empty,
                        PhoneCountryCode = workPhone?.CountryCode ?? PhoneNumber.DefaultCountryCode(),
                        IsSmsOptInChecked = workPhone?.IsMessagingEnabled ?? false,
                        Address = location == null ? null : new AddressControlBag
                        {
                            Street1 = location.Street1,
                            Street2 = location.Street2,
                            City = location.City,
                            State = location.State,
                            PostalCode = location.PostalCode,
                            Country = location.Country
                        }
                    };
                } )
                .ToList();

            // Transferring a business gift preselects that business and starts in Give As Business mode.
            var transferAuthorizedPerson = scheduledTransactionToTransfer?.AuthorizedPersonAlias?.Person;

            if ( transferAuthorizedPerson != null && transferAuthorizedPerson.GivingId != targetPerson.GivingId )
            {
                var matchingBusiness = options.Businesses.FirstOrDefault( business => business.Guid == transferAuthorizedPerson.Guid );

                if ( matchingBusiness != null )
                {
                    options.IsGivingAsBusinessDefault = true;
                    options.DefaultBusinessGuid = matchingBusiness.Guid;
                }
            }
        }

        /// <summary>
        /// Projects the process request's entered values onto a confirmation request so the charge path can
        /// reuse the same validation as the entry and confirmation steps.
        /// </summary>
        /// <param name="request">The process request to project.</param>
        /// <returns>The equivalent confirmation request.</returns>
        private static UtilityPaymentEntryConfirmationRequestBag ToConfirmationRequest( UtilityPaymentEntryProcessRequestBag request )
        {
            return new UtilityPaymentEntryConfirmationRequestBag
            {
                AccountAmounts = request.AccountAmounts,
                CampusGuid = request.CampusGuid,
                FrequencyGuid = request.FrequencyGuid,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PhoneCountryCode = request.PhoneCountryCode,
                Address = request.Address,
                IsGivingAsBusiness = request.IsGivingAsBusiness,
                BusinessName = request.BusinessName,
                BusinessContactFirstName = request.BusinessContactFirstName,
                BusinessContactLastName = request.BusinessContactLastName,
                BusinessContactEmail = request.BusinessContactEmail,
                BusinessContactPhone = request.BusinessContactPhone,
                BusinessContactPhoneCountryCode = request.BusinessContactPhoneCountryCode
            };
        }

        /// <summary>
        /// Resolves the business a gift is authorized to. The contact is the resolved (signed-in or
        /// impersonated) individual, or one matched or created from the entered business-contact fields when
        /// no one is signed in. The business is the selected one, else the contact's single name-matching
        /// business, else a newly created business the contact is added to. The entered email, phone (Work),
        /// and address (Work) are written to the business.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <param name="targetPerson">The resolved individual submitting on the business's behalf, or null when not signed in.</param>
        /// <param name="contactPerson">Set to the individual submitting on the business's behalf, used as the
        /// schedule owner for a scheduled business gift. Null when the contact could not be resolved.</param>
        /// <returns>The resolved business, or null when the contact could not be resolved.</returns>
        private Person ResolveBusinessGiver( UtilityPaymentEntryProcessRequestBag request, Person targetPerson, out Person contactPerson )
        {
            var personService = new PersonService( RockContext );

            // The contact is the resolved (signed-in or impersonated) individual, or one created from the
            // business-contact fields when no one is signed in.
            contactPerson = targetPerson ?? ResolveBusinessContact( request );

            if ( contactPerson == null )
            {
                return null;
            }

            var business = request.BusinessGuid.HasValue ? personService.Get( request.BusinessGuid.Value ) : null;

            // Fall back to the contact's single business whose name matches what was entered.
            if ( business == null )
            {
                var matchingBusinesses = contactPerson.GetBusinesses()
                    .Where( candidate => candidate.LastName == request.BusinessName )
                    .ToList();

                if ( matchingBusinesses.Count == 1 )
                {
                    business = matchingBusinesses.First();
                }
            }

            if ( business == null )
            {
                business = CreateBusiness( request );
                personService.AddContactToBusiness( business.Id, contactPerson.Id );
                RockContext.SaveChanges();
            }

            business.LastName = request.BusinessName;
            business.Email = request.Email;

            if ( GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean() )
            {
                UpdateBusinessWorkPhone( business, contactPerson, request );
            }

            RockContext.SaveChanges();

            UpdateBusinessAddress( business, request );

            return business;
        }

        /// <summary>
        /// Creates a new business record from the entered business name, with the configured connection
        /// status, record status, and record source.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <returns>The newly created business.</returns>
        private Person CreateBusiness( UtilityPaymentEntryProcessRequestBag request )
        {
            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

            var business = new Person
            {
                LastName = request.BusinessName,
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id,
                ConnectionStatusValueId = connectionStatus?.Id,
                RecordStatusValueId = recordStatus?.Id,
                RecordSourceValueId = GetRecordSourceValueId()
            };

            PersonService.SaveNewPerson( business, RockContext, null, false );

            return business;
        }

        /// <summary>
        /// Resolves the individual submitting on the business's behalf: an existing individual matched by the
        /// entered contact name and email, otherwise a newly created one. The matched individual's primary
        /// email is intentionally not updated, since it is likely their business email.
        /// </summary>
        /// <param name="request">The gift details entered by the giver.</param>
        /// <returns>The resolved business contact.</returns>
        private Person ResolveBusinessContact( UtilityPaymentEntryProcessRequestBag request )
        {
            var personService = new PersonService( RockContext );
            Person person = null;

            if ( request.BusinessContactEmail.IsNotNullOrWhiteSpace()
                && request.BusinessContactFirstName.IsNotNullOrWhiteSpace()
                && request.BusinessContactLastName.IsNotNullOrWhiteSpace() )
            {
                person = personService.FindPerson( request.BusinessContactFirstName, request.BusinessContactLastName, request.BusinessContactEmail, false );
            }

            if ( person == null )
            {
                var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

                person = new Person
                {
                    FirstName = request.BusinessContactFirstName,
                    LastName = request.BusinessContactLastName,
                    IsEmailActive = true,
                    EmailPreference = EmailPreference.EmailAllowed,
                    RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                    ConnectionStatusValueId = connectionStatus?.Id,
                    RecordStatusValueId = recordStatus?.Id,
                    RecordSourceValueId = GetRecordSourceValueId()
                };

                PersonService.SaveNewPerson( person, RockContext, null, false );
            }

            person.Email = request.BusinessContactEmail;

            if ( GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean() )
            {
                SetWorkPhone( person, request.BusinessContactPhone, request.BusinessContactPhoneCountryCode, GetAttributeValue( AttributeKey.SmsOptIn ).AsBoolean(), request.IsBusinessContactSmsOptIn );
            }

            RockContext.SaveChanges();

            return person;
        }

        /// <summary>
        /// Updates the business contact's Work number from the entered phone, or creates one. Reuses an
        /// existing mobile number that matches the entered number so a duplicate Work number is not added.
        /// </summary>
        /// <param name="person">The business contact whose Work number is set.</param>
        /// <param name="phoneNumber">The entered phone number.</param>
        /// <param name="countryCode">The entered phone country code.</param>
        /// <param name="isSmsOptInShown">Whether the SMS opt-in choice was offered, so its value should be saved.</param>
        /// <param name="isMessagingEnabled">The entered SMS opt-in value.</param>
        private void SetWorkPhone( Person person, string phoneNumber, string countryCode, bool isSmsOptInShown, bool isMessagingEnabled )
        {
            var workNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id;
            var mobileNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

            var cleanCountryCode = PhoneNumber.CleanNumber( countryCode );
            var cleanNumber = PhoneNumber.CleanNumber( phoneNumber );

            var workPhone = person.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == workNumberTypeId );

            if ( workPhone != null )
            {
                workPhone.CountryCode = cleanCountryCode;
                workPhone.Number = cleanNumber;

                if ( isSmsOptInShown )
                {
                    workPhone.IsMessagingEnabled = isMessagingEnabled;
                }

                return;
            }

            var mobilePhone = person.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == mobileNumberTypeId && phone.Number == cleanNumber );

            if ( mobilePhone != null )
            {
                if ( isSmsOptInShown )
                {
                    mobilePhone.IsMessagingEnabled = isMessagingEnabled;
                }

                return;
            }

            var newWorkPhone = new PhoneNumber
            {
                NumberTypeValueId = workNumberTypeId,
                CountryCode = cleanCountryCode,
                Number = cleanNumber
            };

            if ( isSmsOptInShown )
            {
                newWorkPhone.IsMessagingEnabled = isMessagingEnabled;
            }

            person.PhoneNumbers.Add( newWorkPhone );
        }

        /// <summary>
        /// Writes the entered phone to the business as a Work number: an existing Work number is updated;
        /// otherwise a new one is added, unless the submitting contact already has a mobile on file, in which
        /// case that mobile's opt-in is set and no Work number is added.
        /// </summary>
        /// <param name="business">The business whose Work number is set.</param>
        /// <param name="contact">The individual submitting on the business's behalf.</param>
        /// <param name="request">The gift details entered by the giver.</param>
        private void UpdateBusinessWorkPhone( Person business, Person contact, UtilityPaymentEntryProcessRequestBag request )
        {
            var workNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id;
            var mobileNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
            var isSmsOptInShown = GetAttributeValue( AttributeKey.SmsOptIn ).AsBoolean();

            var cleanCountryCode = PhoneNumber.CleanNumber( request.PhoneCountryCode );
            var cleanNumber = PhoneNumber.CleanNumber( request.Phone );

            var workPhone = business.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == workNumberTypeId );

            if ( workPhone != null )
            {
                workPhone.CountryCode = cleanCountryCode;
                workPhone.Number = cleanNumber;

                if ( isSmsOptInShown )
                {
                    workPhone.IsMessagingEnabled = request.IsSmsOptIn;
                }

                return;
            }

            // No Work number: a mobile already on the contact suppresses adding one to the business.
            var contactMobile = contact.PhoneNumbers.FirstOrDefault( phone => phone.NumberTypeValueId == mobileNumberTypeId );

            if ( contactMobile == null )
            {
                var newWorkPhone = new PhoneNumber
                {
                    NumberTypeValueId = workNumberTypeId,
                    CountryCode = cleanCountryCode,
                    Number = cleanNumber
                };

                if ( isSmsOptInShown )
                {
                    newWorkPhone.IsMessagingEnabled = request.IsSmsOptIn;
                }

                business.PhoneNumbers.Add( newWorkPhone );
            }
            else if ( isSmsOptInShown )
            {
                contactMobile.IsMessagingEnabled = request.IsSmsOptIn;
            }
        }

        /// <summary>
        /// Writes the entered address to the business as a Work location: to the family that already holds
        /// the business's Work address when it has one, otherwise the business's primary family. The Work
        /// type is used regardless of the configured Address Type.
        /// </summary>
        /// <param name="business">The business whose family address is updated.</param>
        /// <param name="request">The gift details entered by the giver.</param>
        private void UpdateBusinessAddress( Person business, UtilityPaymentEntryProcessRequestBag request )
        {
            if ( request.Address == null )
            {
                return;
            }

            var workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() )?.Id;
            var family = ( workLocationTypeId.HasValue ? new PersonService( RockContext ).GetFirstLocation( business.Id, workLocationTypeId.Value )?.Group : null )
                ?? business.GetFamily( RockContext );

            if ( family == null )
            {
                return;
            }

            GroupService.AddNewGroupAddress(
                RockContext,
                family,
                Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK,
                request.Address.Street1,
                request.Address.Street2,
                request.Address.City,
                request.Address.State,
                request.Address.PostalCode,
                request.Address.Country,
                true
            );
        }

        /// <summary>
        /// Adds name-entry errors when the entered first or last name is missing or contains disallowed
        /// special characters, emojis, or special fonts.
        /// </summary>
        /// <param name="firstName">The entered first name.</param>
        /// <param name="lastName">The entered last name.</param>
        /// <param name="errorMessages">The list the errors are added to.</param>
        private static void ValidateEnteredName( string firstName, string lastName, List<string> errorMessages )
        {
            if ( firstName.IsNullOrWhiteSpace() || lastName.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Make sure to enter both a first and last name." );
            }

            if ( System.Text.RegularExpressions.Regex.IsMatch( firstName ?? string.Empty, RegexPatterns.SpecialCharacterRemovalPattern ) || System.Text.RegularExpressions.Regex.IsMatch( lastName ?? string.Empty, RegexPatterns.SpecialCharacterRemovalPattern ) )
            {
                errorMessages.Add( "Make sure to enter a first and last name that does not contain special characters such as quotes, parentheses, Etc." );
            }

            if ( System.Text.RegularExpressions.Regex.IsMatch( firstName ?? string.Empty, RegexPatterns.EmojiAndSpecialFontRemovalPattern ) || System.Text.RegularExpressions.Regex.IsMatch( lastName ?? string.Empty, RegexPatterns.EmojiAndSpecialFontRemovalPattern ) )
            {
                errorMessages.Add( "Make sure to enter a first and last name that does not contain emojis or special fonts." );
            }
        }

        /// <summary>
        /// Adds the business-mode validation errors: the business name, and (when no individual is signed in)
        /// the business-contact name, phone, and email required to identify who is submitting.
        /// </summary>
        /// <param name="request">The entered gift details.</param>
        /// <param name="errorMessages">The list the errors are added to.</param>
        private void ValidateBusinessEntry( UtilityPaymentEntryConfirmationRequestBag request, List<string> errorMessages )
        {
            if ( request.BusinessName.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Make sure to enter a Business Name." );
            }

            // The business-contact fields are entered and validated only when no individual is signed in.
            if ( RequestContext.CurrentPerson != null )
            {
                return;
            }

            if ( request.BusinessContactFirstName.IsNullOrWhiteSpace() || request.BusinessContactLastName.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Make sure to enter both a first and last name for Business Contact." );
            }

            if ( System.Text.RegularExpressions.Regex.IsMatch( request.BusinessContactFirstName ?? string.Empty, RegexPatterns.SpecialCharacterRemovalPattern ) || System.Text.RegularExpressions.Regex.IsMatch( request.BusinessContactLastName ?? string.Empty, RegexPatterns.SpecialCharacterRemovalPattern ) )
            {
                errorMessages.Add( "Make sure to enter a first and last name that does not contain special characters such as quotes, parentheses, Etc. for Business Contact." );
            }

            if ( System.Text.RegularExpressions.Regex.IsMatch( request.BusinessContactFirstName ?? string.Empty, RegexPatterns.EmojiAndSpecialFontRemovalPattern ) || System.Text.RegularExpressions.Regex.IsMatch( request.BusinessContactLastName ?? string.Empty, RegexPatterns.EmojiAndSpecialFontRemovalPattern ) )
            {
                errorMessages.Add( "Make sure to enter a first and last name that does not contain emojis or special fonts for Business Contact." );
            }

            if ( GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean() && request.BusinessContactEmail.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Make sure to enter a valid Business Contact email address." );
            }

            if ( GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean() && request.BusinessContactPhone.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Make sure to enter a valid Business Contact phone number." );
            }
        }

        /// <summary>
        /// Adds an error when a recurring gift's end date falls before the schedule's start date. The start is
        /// clamped forward to the gateway's earliest scheduled date, so the end is validated against that
        /// clamped start rather than the entered start date.
        /// </summary>
        /// <param name="request">The entered gift details.</param>
        /// <param name="errorMessages">The list the errors are added to.</param>
        private void ValidateScheduledDates( UtilityPaymentEntryConfirmationRequestBag request, List<string> errorMessages )
        {
            // There is nothing to validate when scheduled end dates are disabled or none was entered.
            if ( !GetAttributeValue( AttributeKey.AllowScheduledEndDate ).AsBoolean() || !request.EndDate.AsDateTime().HasValue )
            {
                return;
            }

            var financialGateway = GetConfiguredFinancialGateway();
            var schedule = GetSchedule( request.FrequencyGuid, request.StartDate, request.EndDate, financialGateway, financialGateway?.GetGatewayComponent() );

            if ( schedule?.EndDate.HasValue == true && schedule.EndDate.Value < schedule.StartDate )
            {
                errorMessages.Add( $"When scheduling a repeating payment, the minimum end date is {schedule.StartDate.ToShortDateString()}." );
            }
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// The accounts and preset amounts the Contribution Information section resolved. Shared by the
        /// options builder and the submit-path re-enforcement so both draw the account rules from one source.
        /// </summary>
        private class ContributionAccountResolution
        {
            /// <summary>
            /// Gets or sets a value indicating whether URL account options drove the resolution. False when
            /// the configured Accounts to Display were used instead.
            /// </summary>
            public bool HasUrlAccountOptions { get; set; }

            /// <summary>
            /// Gets or sets the Guids of the accounts to present (the URL accounts, or the configured
            /// accounts when URL account options are not active). An empty list resolves to every eligible
            /// account.
            /// </summary>
            public List<Guid> AccountGuidsToDisplay { get; set; } = new List<Guid>();

            /// <summary>
            /// Gets or sets a value indicating whether non-public accounts may be presented (URL account
            /// options with the public-only restriction off).
            /// </summary>
            public bool AllowPrivateAccounts { get; set; }

            /// <summary>
            /// Gets or sets the preset (and optionally locked) amounts from URL account options.
            /// </summary>
            public List<UtilityPaymentEntryPresetAccountAmountBag> PresetAccountAmounts { get; set; } = new List<UtilityPaymentEntryPresetAccountAmountBag>();

            /// <summary>
            /// Gets or sets the Invalid Account Message to show when a URL account was dropped, or null when
            /// none was dropped.
            /// </summary>
            public string UrlInvalidAccountMessage { get; set; }
        }

        /// <summary>
        /// The server-authoritative account rules re-enforced on a submitted gift: the accounts the giver
        /// may give to, and the amounts locked by a read-only URL account option.
        /// </summary>
        private class AccountSubmitRules
        {
            /// <summary>
            /// Gets or sets the Guids of the accounts the giver is permitted to give to.
            /// </summary>
            public HashSet<Guid> AllowedAccountGuids { get; set; } = new HashSet<Guid>();

            /// <summary>
            /// Gets or sets the amounts locked by a read-only URL account option, keyed by account Guid.
            /// An account absent from the map is freely editable.
            /// </summary>
            public Dictionary<Guid, decimal> LockedAmounts { get; set; } = new Dictionary<Guid, decimal>();
        }

        /// <summary>
        /// A single account option parsed from the AccountIds or AccountGlCodes page parameter.
        /// </summary>
        private class UrlAccountOption
        {
            /// <summary>
            /// Gets or sets the Guid of the resolved account, or null when the id or GL code did not
            /// resolve to an account.
            /// </summary>
            public Guid? AccountGuid { get; set; }

            /// <summary>
            /// Gets or sets the preset amount, or null when the URL specified no amount.
            /// </summary>
            public decimal? Amount { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the giver may edit the amount. False locks it to
            /// the preset amount.
            /// </summary>
            public bool IsEnabled { get; set; }
        }

        /// <summary>
        /// A per-account contribution row exposed to the Payment Comment Template Lava.
        /// </summary>
        private class TransactionAccountDetailInfo : LavaDataObject
        {
            /// <summary>
            /// Gets or sets the account identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the account's display order.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the account's internal name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the account's public name.
            /// </summary>
            public string PublicName { get; set; }

            /// <summary>
            /// Gets or sets the id of the campus the gift is associated with.
            /// </summary>
            public int? CampusId { get; set; }

            /// <summary>
            /// Gets or sets the amount contributed to the account.
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Always true; kept for backward compatibility with existing comment templates.
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            /// Gets the amount formatted as currency, or an empty string when zero.
            /// </summary>
            public string AmountFormatted => Amount > 0 ? Amount.FormatAsCurrency() : string.Empty;

            public TransactionAccountDetailInfo( int accountId, decimal amount, int? campusId )
            {
                Id = accountId;
                Amount = amount;
                CampusId = campusId;
                Enabled = true;

                var account = FinancialAccountCache.Get( accountId );

                if ( account == null )
                {
                    return;
                }

                Name = account.Name;
                PublicName = account.PublicName;
                Order = account.Order;
            }
        }

        #endregion Supporting Classes
    }
}

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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.ScheduledTransactionEditV2;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Edit an existing scheduled transaction. This is the public/external block for
    /// editing scheduled transactions (also used internally by trusted staff).
    /// </summary>
    [DisplayName( "Scheduled Transaction Edit" )]
    [Category( "Finance" )]
    [Description( "Edit an existing scheduled transaction." )]
    [IconCssClass( "ti ti-cash" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "ACH",
        Key = AttributeKey.EnableACH,
        Description = "Whether ACH bank transfers are available as a payment method.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 1 )]

    [BooleanField(
        "Credit Card",
        Key = AttributeKey.EnableCreditCard,
        Description = "Whether credit card payments are available as a payment method.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 2 )]

    [AccountsField(
        "Display Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts available for selection. When Campus Account Mapping is active and a campus-specific child account exists, the child account is used instead.",
        IsRequired = false,
        Category = AttributeCategory.None,
        Order = 3 )]

    [BooleanField(
        "Show Additional Accounts",
        Key = AttributeKey.ShowAdditionalAccounts,
        Description = "When enabled, all active public accounts are also available to select unless Additional Accounts is set.",
        TrueText = "Display option for selecting additional accounts",
        FalseText = "Don't display option",
        Category = AttributeCategory.None,
        Order = 4 )]

    [AccountsField(
        "Additional Accounts",
        Key = AttributeKey.AdditionalAccounts,
        Description = "The specific accounts shown when Show Public Accounts is enabled. Leave blank to show all active public accounts.",
        IsRequired = false,
        Category = AttributeCategory.None,
        Order = 5 )]

    [CustomDropdownListField(
        "Campus Account Mapping",
        Key = AttributeKey.UseAccountCampusMappingLogic,
        Description = "How the selected account is mapped to the selected campus. \"Enabled\" always applies campus child account mapping. \"Disabled\" never applies it. \"Use Financial Account Setting\" applies mapping when any selected account has \"Use Campus Child Account Matching\" enabled. When mapping is active: if no campus is selected, the parent account is used; if an active child account matches the selected campus, that child account is used; if no matching child account exists, the parent account is used.",
        ListSource = "Enabled^Enabled,Disabled^Disabled,UseFinancialAccount^Use Financial Account Setting",
        IsRequired = false,
        DefaultValue = "Enabled",
        Category = AttributeCategory.None,
        Order = 6 )]

    [BooleanField(
        "Always Ask for Campus",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "Whether the campus field is shown even when the person's campus is already on file.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 7 )]

    [BooleanField(
        "Multi-Account Giving",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Whether the person can allocate amounts across more than one account.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 8 )]

    [BooleanField(
        "End Date",
        Key = AttributeKey.EnableEndDate,
        Description = "Whether the person can set an optional end date for their recurring gift.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 9 )]

    #region Editing Options

    [BooleanField(
        "Allow Impersonation",
        Key = AttributeKey.AllowImpersonation,
        Description = "Whether staff can view and edit scheduled transactions on behalf of another person. Only enable this on internal pages secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 10 )]

    [BooleanField(
        "Impersonator Can See Saved Accounts",
        Key = AttributeKey.ImpersonatorCanSeeSavedAccounts,
        Description = "Whether staff can view saved payment accounts belonging to another person. NOTE: Only enable this on internal pages secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 11 )]

    #endregion Editing Options

    #region Customize Text

    [TextField(
        "Panel Title",
        Key = AttributeKey.PanelTitle,
        Description = "The title displayed in the panel header.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 1 )]

    [BooleanField(
        "Show Block Header Section",
        Key = AttributeKey.ShowBlockHeader,
        Description = "When enabled, displays a title and description at the top of the block.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomizeText,
        Order = 2 )]

    [BooleanField(
        "Show Section Headers",
        Key = AttributeKey.ShowSectionHeaders,
        Description = "When enabled, displays a titled header for each section of the form.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomizeText,
        Order = 3 )]

    [BooleanField(
        "Show Section Descriptions",
        Key = AttributeKey.ShowSectionDescriptions,
        Description = "When enabled, displays the supporting description text below each section header.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomizeText,
        Order = 4 )]

    [TextField(
        "Transaction Term",
        Key = AttributeKey.GiftTerm,
        Description = "The word used throughout the block to refer to a financial contribution. Defaults to 'Gift'.",
        DefaultValue = "Gift",
        Category = AttributeCategory.CustomizeText,
        Order = 5 )]

    [TextField(
        "Header Title",
        Key = AttributeKey.HeaderTitle,
        Description = "The title displayed at the top of the block.",
        DefaultValue = "Edit Giving Profile",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 6 )]

    [TextField(
        "Header Description",
        Key = AttributeKey.HeaderDescription,
        Description = "The supporting text displayed below the header title.",
        DefaultValue = "Review and update your scheduled transaction details.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 7 )]

    [TextField(
        "Header Icon",
        Key = AttributeKey.HeaderIcon,
        Description = "The icon displayed in the block header.",
        DefaultValue = "ti ti-cash",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 8 )]

    [TextField(
        "Campus Information Section Title",
        Key = AttributeKey.CampusSectionTitle,
        Description = "The label displayed in the Campus Information section header.",
        DefaultValue = "Campus Information",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 9 )]

    [TextField(
        "Campus Information Section Icon",
        Key = AttributeKey.CampusSectionIcon,
        Description = "The icon displayed in the Campus Information section header.",
        DefaultValue = "ti ti-map-pin",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 10 )]

    [TextField(
        "Campus Information Section Description",
        Key = AttributeKey.CampusSectionDescription,
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Review and update the campus that your gift should be associated with.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 11 )]

    [TextField(
        "Gift Information Section Title",
        Key = AttributeKey.GiftSectionTitle,
        Description = "The label displayed in the Gift Information section header.",
        DefaultValue = "Gift Information",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 12 )]

    [TextField(
        "Gift Information Section Icon",
        Key = AttributeKey.GiftSectionIcon,
        Description = "The icon displayed in the Gift Information section header.",
        DefaultValue = "ti ti-gift",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 13 )]

    [TextField(
        "Gift Information Section Description",
        Key = AttributeKey.GiftSectionDescription,
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Review and update the details of your scheduled gift.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 14 )]

    [TextField(
        "Payment Information Section Title",
        Key = AttributeKey.PaymentSectionTitle,
        Description = "The label displayed in the Payment Information section header.",
        DefaultValue = "Payment Method",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 15 )]

    [TextField(
        "Payment Information Section Icon",
        Key = AttributeKey.PaymentSectionIcon,
        Description = "The icon displayed in the Payment Information section header.",
        DefaultValue = "ti ti-wallet",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 16 )]

    [TextField(
        "Payment Information Section Description",
        Key = AttributeKey.PaymentSectionDescription,
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Review and update the payment method your gift will be charged to.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeText,
        Order = 17 )]

    [TextField(
        "Add Payment Account Button Label",
        Key = AttributeKey.AddAccountText,
        Description = "The label on the button that adds another account row.",
        IsRequired = false,
        DefaultValue = "Add Another Account",
        Category = AttributeCategory.CustomizeText,
        Order = 18 )]

    [CodeEditorField(
        "Success Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The Lava-enabled HTML displayed after the transaction is saved.",
        DefaultValue = DefaultFinishLavaTemplate,
        Category = AttributeCategory.CustomizeText,
        IsRequired = false,
        Order = 19 )]

    #endregion Customize Text

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "3F0F2052-217B-4260-A350-35ACD3704B43" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "287B9484-72C6-4CBB-9F43-9DA30B6160BA" )]
    [Rock.SystemGuid.BlockTypeGuid( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18" )]
    public class ScheduledTransactionEditV2 : RockBlockType
    {
        #region Constants

        /// <summary>
        /// The default Lava template used to render the success/confirmation screen.
        /// </summary>
        protected const string DefaultFinishLavaTemplate = @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

{% assign total = 0 %}
{% for transactionDetail in transactionDetails %}
    {% assign total = total | Plus:transactionDetail.Amount %}
{% endfor %}

<div class='alert alert-success'>
    <p class='margin-b-none'>Success! Your scheduled transaction information has been updated.</p>
</div>

<h4>Gift Information</h4>
<div style='border:1px solid var(--color-interface-soft);border-radius:var(--rounded-small);overflow:hidden;margin-bottom:var(--spacing-large);'>
    <table style='width:100%;border-collapse:collapse;'>
        <tbody>
            {% for transactionDetail in transactionDetails %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ transactionDetail.Account.PublicName }}</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ transactionDetail.Amount | FormatAsCurrency }}</td>
                </tr>
            {% endfor %}
            <tr>
                <td style='padding:12px 16px;'><strong>Total</strong></td>
                <td class='text-right' style='padding:12px 16px;'><strong>{{ total | FormatAsCurrency }}</strong></td>
            </tr>
        </tbody>
    </table>
</div>

<h4>Payment &amp; Confirmation</h4>
<div style='border:1px solid var(--color-interface-soft);border-radius:var(--rounded-small);overflow:hidden;'>
    <table style='width:100%;border-collapse:collapse;'>
        <tbody>
            <tr>
                <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Payment Method</td>
                <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ PaymentDetail.CurrencyTypeValue.Value }}</td>
            </tr>

            {% if PaymentDetail.AccountNumberMasked != '' %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Account Number</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>
                        {% if PaymentDetail.CreditCardTypeValue %}
                            {{ PaymentDetail.CreditCardTypeValue.Value }} Ending in {{ PaymentDetail.AccountNumberMasked | Right:4 }}
                        {% else %}
                            {{ PaymentDetail.AccountNumberMasked }}
                        {% endif %}
                    </td>
                </tr>
            {% endif %}

            <tr>
                <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>When</td>
                <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>
                    {% if Transaction.TransactionFrequencyValue %}
                        {{ Transaction.TransactionFrequencyValue.Value }}
                        {% if Transaction.EndDate %}
                            starting on {{ Transaction.NextPaymentDate | Date:'sd' }} and ending on {{ Transaction.EndDate | Date:'sd' }}
                        {% else %}
                            starting on {{ Transaction.NextPaymentDate | Date:'sd' }}
                        {% endif %}
                    {% else %}
                        Today
                    {% endif %}
                </td>
            </tr>

            <tr>
                <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Name</td>
                <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ Person.FullName }}</td>
            </tr>

            {% if Person.Email != '' %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Email</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ Person.Email }}</td>
                </tr>
            {% endif %}

            {% if BillingLocation %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Address</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ BillingLocation.Street1 }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</td>
                </tr>
            {% endif %}

            <tr>
                <td class='text-muted' style='padding:12px 16px;'>Confirmation</td>
                <td class='text-right' style='padding:12px 16px;'><span class='label label-info'>{{ Transaction.TransactionCode }}</span></td>
            </tr>
        </tbody>
    </table>
</div>
";

        #endregion Constants

        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string AccountsToDisplay = "AccountsToDisplay";
            public const string ShowAdditionalAccounts = "ShowAdditionalAccounts";
            public const string UseAccountCampusMappingLogic = "UseAccountCampusMappingLogic";
            public const string AdditionalAccounts = "AdditionalAccounts";
            public const string AddAccountText = "AddAccountText";
            public const string AllowImpersonation = "AllowImpersonation";
            public const string ImpersonatorCanSeeSavedAccounts = "ImpersonatorCanSeeSavedAccounts";
            public const string GiftTerm = "GiftTerm";
            public const string AskForCampusIfKnown = "AskForCampusIfKnown";
            public const string EnableMultiAccount = "EnableMultiAccount";
            public const string FinishLavaTemplate = "FinishLavaTemplate";
            public const string EnableEndDate = "EnableEndDate";
            public const string ShowSectionHeaders = "ShowSectionHeaders";
            public const string ShowSectionDescriptions = "ShowSectionDescriptions";
            public const string PanelTitle = "PanelTitle";
            public const string ShowBlockHeader = "ShowBlockHeader";
            public const string HeaderTitle = "HeaderTitle";
            public const string HeaderDescription = "HeaderDescription";
            public const string HeaderIcon = "HeaderIcon";
            public const string CampusSectionTitle = "CampusSectionTitle";
            public const string CampusSectionIcon = "CampusSectionIcon";
            public const string CampusSectionDescription = "CampusSectionDescription";
            public const string GiftSectionTitle = "GiftSectionTitle";
            public const string GiftSectionIcon = "GiftSectionIcon";
            public const string GiftSectionDescription = "GiftSectionDescription";
            public const string PaymentSectionTitle = "PaymentSectionTitle";
            public const string PaymentSectionIcon = "PaymentSectionIcon";
            public const string PaymentSectionDescription = "PaymentSectionDescription";
        }

        /// <summary>
        /// Keys to use for Page Parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
        }

        /// <summary>
        /// Categories used to group Block Attributes in the settings UI.
        /// </summary>
        private static class AttributeCategory
        {
            /// <summary>
            /// Uncategorized — renders as a flat list on the default Basic Settings tab.
            /// </summary>
            public const string None = "";

            /// <summary>
            /// Places the attribute on the Customize Text tab. Section ordering within a tab
            /// cannot be controlled, so a single group is used and the attributes are ordered
            /// by their Order value.
            /// </summary>
            public const string CustomizeText = "Customize Text^Customize Text";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ScheduledTransactionEditV2Bag, ScheduledTransactionEditV2OptionsBag>();

            box.Options = GetBoxOptions();
            box.Bag = GetInitializationBag();

            return box;
        }

        /// <summary>
        /// Gets the configuration options for the block. These are the settings the client
        /// needs to render and behave; settings consumed only server-side (account selection,
        /// impersonation, gateway payment types, success template) are intentionally excluded.
        /// </summary>
        /// <returns>The options bag.</returns>
        private ScheduledTransactionEditV2OptionsBag GetBoxOptions()
        {
            return new ScheduledTransactionEditV2OptionsBag
            {
                // Payment method availability. Also folded into the gateway control settings,
                // but exposed here so the block's own payment UI can branch on it.
                IsAchEnabled = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean(),
                IsCreditCardEnabled = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean(),

                // Behavior flags that shape the form UI.
                IsEndDateEnabled = GetAttributeValue( AttributeKey.EnableEndDate ).AsBoolean(),
                IsCampusPrompted = GetAttributeValue( AttributeKey.AskForCampusIfKnown ).AsBoolean(),

                // Display toggles (Customize Text).
                IsBlockHeaderShown = GetAttributeValue( AttributeKey.ShowBlockHeader ).AsBoolean(),
                IsSectionHeaderShown = GetAttributeValue( AttributeKey.ShowSectionHeaders ).AsBoolean(),
                IsSectionDescriptionShown = GetAttributeValue( AttributeKey.ShowSectionDescriptions ).AsBoolean(),

                // Header text (Customize Text).
                PanelTitle = GetAttributeValue( AttributeKey.PanelTitle ),
                TransactionTerm = GetAttributeValue( AttributeKey.GiftTerm ),
                HeaderTitle = GetAttributeValue( AttributeKey.HeaderTitle ),
                HeaderDescription = GetAttributeValue( AttributeKey.HeaderDescription ),
                HeaderIcon = GetAttributeValue( AttributeKey.HeaderIcon ),

                // Campus Information section text (Customize Text).
                CampusSectionTitle = GetAttributeValue( AttributeKey.CampusSectionTitle ),
                CampusSectionIcon = GetAttributeValue( AttributeKey.CampusSectionIcon ),
                CampusSectionDescription = GetAttributeValue( AttributeKey.CampusSectionDescription ),

                // Gift Information section text (Customize Text).
                GiftSectionTitle = GetAttributeValue( AttributeKey.GiftSectionTitle ),
                GiftSectionIcon = GetAttributeValue( AttributeKey.GiftSectionIcon ),
                GiftSectionDescription = GetAttributeValue( AttributeKey.GiftSectionDescription ),

                // Payment Information section text (Customize Text).
                PaymentSectionTitle = GetAttributeValue( AttributeKey.PaymentSectionTitle ),
                PaymentSectionIcon = GetAttributeValue( AttributeKey.PaymentSectionIcon ),
                PaymentSectionDescription = GetAttributeValue( AttributeKey.PaymentSectionDescription ),
                AddAccountButtonLabel = GetAttributeValue( AttributeKey.AddAccountText )
            };
        }

        /// <summary>
        /// Builds the initial state for the block, mirroring the WebForms OnInit + ShowDetails flow.
        /// </summary>
        /// <returns>The initialization bag.</returns>
        private ScheduledTransactionEditV2Bag GetInitializationBag()
        {
            var bag = new ScheduledTransactionEditV2Bag();

            // Resolve and validate the transaction/gateway using the shared guard checks so
            // the initial load and the update action cannot drift apart.
            var scheduledTransactionGuid = PageParameter( PageParameterKey.ScheduledTransactionGuid ).AsGuidOrNull();
            var editContext = GetEditContext( scheduledTransactionGuid );
            if ( !editContext.IsValid )
            {
                bag.ErrorMessage = editContext.ErrorMessage;
                return bag;
            }

            var scheduledTransaction = editContext.ScheduledTransaction;
            var financialGateway = editContext.FinancialGateway;
            var hostedGatewayComponent = editContext.HostedGatewayComponent;
            var targetPerson = editContext.TargetPerson;

            // the gateway must be able to report the transaction's current status.
            if ( !new FinancialScheduledTransactionService( RockContext ).GetStatus( scheduledTransaction, out var statusErrorMessage ) )
            {
                bag.ErrorMessage = statusErrorMessage;
                return bag;
            }

            var isAchEnabled = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            var isCreditCardEnabled = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();

            bag.ScheduledTransactionGuid = scheduledTransaction.Guid.ToString();

            bag.GatewayControl = new GatewayControlBag
            {
                FileUrl = hostedGatewayComponent.GetObsidianControlFileUrl( financialGateway ),
                Settings = hostedGatewayComponent.GetObsidianControlSettings( financialGateway, new HostedPaymentInfoControlOptions
                {
                    EnableACH = isAchEnabled,
                    EnableCreditCard = isCreditCardEnabled
                } )
            };

            if ( !TryPopulateAccounts( scheduledTransaction, bag ) )
            {
                return bag;
            }

            var targetPersonCampus = targetPerson?.GetCampus();
            if ( targetPersonCampus != null )
            {
                bag.Campus = new ListItemBag { Value = targetPersonCampus.Guid.ToString(), Text = targetPersonCampus.Name };
            }

            // Frequency options + selected value, and the next/end payment dates.
            PopulateSchedule( scheduledTransaction, hostedGatewayComponent, financialGateway, bag );

            PopulatePaymentMethod( scheduledTransaction, financialGateway, targetPerson, bag );

            return bag;
        }

        /// <summary>
        /// Resolves and validates the transaction and gateway needed to edit a scheduled
        /// transaction, running the guard checks shared by the initial block load and the
        /// update action so the two paths cannot drift apart. The update action must not trust
        /// the load-time checks, since the block action is a separate request.
        /// </summary>
        /// <param name="scheduledTransactionGuid">The Guid of the scheduled transaction to edit.</param>
        /// <returns>
        /// A context whose <see cref="ScheduledTransactionEditContext.IsValid"/> is <c>true</c> with
        /// the resolved entities populated, or <c>false</c> with <see cref="ScheduledTransactionEditContext.ErrorMessage"/> set.
        /// </returns>
        private ScheduledTransactionEditContext GetEditContext( Guid? scheduledTransactionGuid )
        {
            var context = new ScheduledTransactionEditContext();

            var scheduledTransaction = GetScheduledTransaction( scheduledTransactionGuid );
            if ( scheduledTransaction == null )
            {
                context.ErrorMessage = "Scheduled Transaction not found.";
                return context;
            }

            if ( !IsAuthorizedToEditScheduledTransaction( scheduledTransaction ) )
            {
                context.ErrorMessage = "You are not authorized to edit this scheduled transaction.";
                return context;
            }

            if ( IsEventRegistrationTransactionType( scheduledTransaction ) )
            {
                context.ErrorMessage = "Event Registration Scheduled Transactions cannot be updated.";
                return context;
            }

            var isAchEnabled = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            var isCreditCardEnabled = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();
            if ( !isAchEnabled && !isCreditCardEnabled )
            {
                context.ErrorMessage = "Enable ACH and/or Enable Credit Card needs to be enabled.";
                return context;
            }

            // The transaction's gateway must be resolvable.
            var financialGateway = new FinancialGatewayService( RockContext ).GetNoTracking( scheduledTransaction.FinancialGatewayId ?? 0 );
            if ( financialGateway == null )
            {
                context.ErrorMessage = "Unable to determine the financial gateway for this scheduled transaction.";
                return context;
            }

            // The gateway must support the hosted (Obsidian) payment control.
            var hostedGatewayComponent = financialGateway.GetGatewayComponent() as IObsidianHostedGatewayComponent;
            if ( hostedGatewayComponent == null )
            {
                context.ErrorMessage = "This page is not configured to allow edits for the payment gateway associated with the selected transaction.";
                return context;
            }

            context.ScheduledTransaction = scheduledTransaction;
            context.FinancialGateway = financialGateway;
            context.HostedGatewayComponent = hostedGatewayComponent;
            context.TargetPerson = scheduledTransaction.AuthorizedPersonAlias?.Person;

            return context;
        }

        /// <summary>
        /// Loads the scheduled transaction with the given Guid, if it exists.
        /// Authorization is checked separately by <see cref="IsAuthorizedToEditScheduledTransaction"/>
        /// so that "not found" and "not authorized" can be reported distinctly.
        /// </summary>
        /// <param name="scheduledTransactionGuid">The Guid of the scheduled transaction to load.</param>
        /// <returns>The scheduled transaction, or <c>null</c> when the Guid is missing or no transaction matches.</returns>
        private FinancialScheduledTransaction GetScheduledTransaction( Guid? scheduledTransactionGuid )
        {
            if ( !scheduledTransactionGuid.HasValue )
            {
                return null;
            }

            return new FinancialScheduledTransactionService( RockContext ).Queryable()
                .Include( t => t.AuthorizedPersonAlias.Person )
                .Include( t => t.ScheduledTransactionDetails.Select( d => d.Account ) )
                .Include( t => t.FinancialPaymentDetail.CurrencyTypeValue )
                .Include( t => t.FinancialPaymentDetail.FinancialPersonSavedAccount )
                .Include( t => t.FinancialPaymentDetail.BillingLocation )
                .FirstOrDefault( t => t.Guid == scheduledTransactionGuid.Value );
        }

        /// <summary>
        /// Populates the account-related bag fields: the selectable accounts, the pool of
        /// additional accounts that can be added on demand, the current amount allocations,
        /// and whether multiple accounts may be used. Mirrors the account setup in the
        /// WebForms ShowDetails method.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction being edited.</param>
        /// <param name="bag">The bag to populate.</param>
        /// <returns><c>true</c> when at least one account is selectable; otherwise <c>false</c> with <see cref="ScheduledTransactionEditV2Bag.ErrorMessage"/> set.</returns>
        private bool TryPopulateAccounts( FinancialScheduledTransaction scheduledTransaction, ScheduledTransactionEditV2Bag bag )
        {
            var accountCampusMappingLogicSetting = GetAttributeValue( AttributeKey.UseAccountCampusMappingLogic );
            var currentTransactionAccountGuids = scheduledTransaction.ScheduledTransactionDetails.Select( d => d.Account.Guid ).ToList();

            bool useAccountCampusMappingLogic;
            if ( accountCampusMappingLogicSetting == "Enabled" )
            {
                useAccountCampusMappingLogic = true;
            }
            else if ( accountCampusMappingLogicSetting == "UseFinancialAccount" && currentTransactionAccountGuids.Any() )
            {
                useAccountCampusMappingLogic = currentTransactionAccountGuids.Any( accountGuid =>
                {
                    var account = FinancialAccountCache.Get( accountGuid );
                    return account?.UsesCampusChildAccounts == true
                        || account?.ParentAccount?.UsesCampusChildAccounts == true;
                } );
            }
            else
            {
                useAccountCampusMappingLogic = false;
            }

            var selectableAccountGuids = GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList();

            /* Match Webforms account selection behavior when campus mapping is enabled.
             *
             * When configured correctly, only parent Financial Accounts are added to the
             * selectable list. The Account Picker displays child account transactions under
             * their mapped parent account.
             *
             * If misconfigured (for example, the parent account is not included), a
             * transaction tied to a child account would otherwise have no selectable
             * account. In that case, the child account is added to the selectable list so
             * the transaction remains editable.
             */
            foreach ( var currentTransactionAccountGuid in currentTransactionAccountGuids )
            {
                var parentAccount = FinancialAccountCache.Get( currentTransactionAccountGuid )?.ParentAccount;

                var isParentSelectable = parentAccount != null && selectableAccountGuids.Contains( parentAccount.Guid );
                var requiresAccountCampusMappingLogicForThisAccount = parentAccount?.UsesCampusChildAccounts == true;

                var shouldAlwaysAddBecauseNoCampusChildLogic =
                    accountCampusMappingLogicSetting == "UseFinancialAccount"
                    && !requiresAccountCampusMappingLogicForThisAccount;

                var shouldAddCurrentAccount =
                    !useAccountCampusMappingLogic
                    || shouldAlwaysAddBecauseNoCampusChildLogic
                    || !isParentSelectable;

                if ( shouldAddCurrentAccount && !selectableAccountGuids.Contains( currentTransactionAccountGuid ) )
                {
                    selectableAccountGuids.Add( currentTransactionAccountGuid );
                }
            }

            var additionalAccountGuids = new List<Guid>();
            if ( GetAttributeValue( AttributeKey.ShowAdditionalAccounts ).AsBoolean() )
            {
                var publicAccountGuids = new FinancialAccountService( RockContext ).Queryable()
                    .Where( f =>
                        f.IsActive &&
                        f.IsPublic.HasValue &&
                        f.IsPublic.Value &&
                        ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                        ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
                    .Select( f => f.Guid )
                    .ToList();

                var configuredAdditionalAccountGuids = GetAttributeValues( AttributeKey.AdditionalAccounts ).AsGuidList();
                if ( configuredAdditionalAccountGuids.Any() )
                {
                    publicAccountGuids = publicAccountGuids.Where( v => configuredAdditionalAccountGuids.Contains( v ) ).ToList();
                }

                if ( !selectableAccountGuids.Any() )
                {
                    selectableAccountGuids = publicAccountGuids;
                }
                else
                {
                    additionalAccountGuids = publicAccountGuids.Where( g => !selectableAccountGuids.Contains( g ) ).ToList();
                }
            }

            if ( !selectableAccountGuids.Any() )
            {
                bag.ErrorMessage = "At least one Financial Account must be selected in the configuration for this block.";
                return false;
            }

            bag.SelectableAccountGuids = selectableAccountGuids.Select( g => g.ToString() ).ToList();

            bag.AdditionalAccounts = FinancialAccountCache.GetByGuids( additionalAccountGuids )
                .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.PublicName } )
                .ToList();

            bag.AccountAmounts = scheduledTransaction.ScheduledTransactionDetails
                .Select( d => new ScheduledTransactionAccountAmountBag
                {
                    AccountGuid = d.Account.Guid.ToString(),
                    AccountName = d.Account.PublicName,
                    Amount = d.Amount
                } )
                .ToList();

            // Multi-account mode is forced on when the transaction already spans multiple accounts,
            // otherwise it follows the block setting.
            var hasMultipleAccounts = bag.AccountAmounts.Count > 1;
            bag.IsMultiAccountMode = hasMultipleAccounts || GetAttributeValue( AttributeKey.EnableMultiAccount ).AsBoolean();

            return true;
        }

        /// <summary>
        /// Populates the schedule-related bag fields: the supported frequency options, the
        /// currently selected frequency, and the next/end payment dates. Mirrors the frequency
        /// and date setup in the WebForms ShowDetails method.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction being edited.</param>
        /// <param name="hostedGatewayComponent">The resolved Obsidian-hosted gateway component for the transaction.</param>
        /// <param name="financialGateway">The transaction's financial gateway.</param>
        /// <param name="bag">The bag to populate.</param>
        private void PopulateSchedule( FinancialScheduledTransaction scheduledTransaction, IObsidianHostedGatewayComponent hostedGatewayComponent, FinancialGateway financialGateway, ScheduledTransactionEditV2Bag bag )
        {
            var scheduleGatewayComponent = hostedGatewayComponent as IHostedGatewayComponent;
            if ( scheduleGatewayComponent == null )
            {
                return;
            }

            var oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;
            var isCurrentlyOneTime = scheduledTransaction.TransactionFrequencyValueId == oneTimeFrequencyId;

            // If currently one time, return all options. If recurring, exclude one time option.
            bag.FrequencyOptions = scheduleGatewayComponent.SupportedPaymentSchedules
                .Where( f => isCurrentlyOneTime || f.Id != oneTimeFrequencyId )
                .Select( f => new ListItemBag { Value = f.Guid.ToString(), Text = f.Value } )
                .ToList();

            bag.SelectedFrequencyValue = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId )?.Guid.ToString();

            // The gateway may require the next payment to be some number of days out; clamp
            // forward to that earliest allowed date rather than allowing an invalid date.
            var earliestScheduledStartDate = scheduleGatewayComponent.GetEarliestScheduledStartDate( financialGateway );
            var nextPaymentDate = scheduledTransaction.NextPaymentDate;
            if ( nextPaymentDate.HasValue && nextPaymentDate.Value < earliestScheduledStartDate )
            {
                nextPaymentDate = earliestScheduledStartDate;
            }

            bag.NextPaymentDate = nextPaymentDate;
            bag.EarliestPaymentDate = earliestScheduledStartDate;
            bag.EndDate = scheduledTransaction.EndDate;
        }

        /// <summary>
        /// Populates the payment-method bag fields: the selectable payment methods (the existing
        /// payment method followed by the person's saved accounts) and the billing address.
        /// Mirrors the payment prompt and billing setup in the WebForms ShowDetails method.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction being edited.</param>
        /// <param name="financialGateway">The transaction's financial gateway.</param>
        /// <param name="targetPerson">The person whose gift is being edited.</param>
        /// <param name="bag">The bag to populate.</param>
        private void PopulatePaymentMethod( FinancialScheduledTransaction scheduledTransaction, FinancialGateway financialGateway, Person targetPerson, ScheduledTransactionEditV2Bag bag )
        {
            var paymentDetail = scheduledTransaction.FinancialPaymentDetail;
            var paymentMethods = new List<ScheduledTransactionPaymentMethodBag>();

            if ( paymentDetail != null )
            {
                var existingPaymentName = paymentDetail.FinancialPersonSavedAccountId.HasValue
                    ? paymentDetail.FinancialPersonSavedAccount?.Name
                    : GetPaymentMethodCardType( paymentDetail );

                paymentMethods.Add( new ScheduledTransactionPaymentMethodBag
                {
                    Value = "existing",
                    IsExistingPaymentMethod = true,
                    IsCreditCard = IsCreditCardPaymentDetail( paymentDetail ),
                    Name = existingPaymentName,
                    CardType = GetPaymentMethodCardType( paymentDetail ),
                    AccountNumberMasked = paymentDetail.AccountNumberMasked,
                    ExpirationDate = paymentDetail.ExpirationDate
                } );
            }

            var existingSavedAccountId = paymentDetail?.FinancialPersonSavedAccountId;

            paymentMethods.AddRange( GetSavedAccounts( financialGateway, targetPerson, existingSavedAccountId ) );

            bag.PaymentMethods = paymentMethods;
        }

        /// <summary>
        /// Gets the target person's saved accounts for the transaction's gateway, limited to
        /// the allowed currency types (credit card and ACH), as selectable payment methods.
        /// When viewing another person's transaction (impersonation), saved accounts are only
        /// returned if the block is configured to allow it.
        /// </summary>
        /// <param name="financialGateway">The transaction's financial gateway.</param>
        /// <param name="targetPerson">The person whose gift is being edited.</param>
        /// <param name="excludedSavedAccountId">The id of a saved account to omit from the results (the one already shown as the existing payment method), or <c>null</c> to include all.</param>
        /// <returns>The saved account payment methods, or an empty list when none are available or permitted.</returns>
        private List<ScheduledTransactionPaymentMethodBag> GetSavedAccounts( FinancialGateway financialGateway, Person targetPerson, int? excludedSavedAccountId = null )
        {
            if ( targetPerson == null || financialGateway == null )
            {
                return new List<ScheduledTransactionPaymentMethodBag>();
            }

            var isImpersonating = targetPerson.Id != RequestContext.CurrentPerson?.Id;
            if ( isImpersonating && !GetAttributeValue( AttributeKey.ImpersonatorCanSeeSavedAccounts ).AsBoolean() )
            {
                return new List<ScheduledTransactionPaymentMethodBag>();
            }

            var allowedCurrencyTypeIds = new List<int>();
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            if ( creditCardCurrency != null )
            {
                allowedCurrencyTypeIds.Add( creditCardCurrency.Id );
            }
            if ( achCurrency != null )
            {
                allowedCurrencyTypeIds.Add( achCurrency.Id );
            }

            var savedAccounts = new FinancialPersonSavedAccountService( RockContext )
                .GetByPersonId( targetPerson.Id )
                .Where( a =>
                    !a.IsSystem
                    && a.FinancialGatewayId == financialGateway.Id
                    && ( excludedSavedAccountId == null || a.Id != excludedSavedAccountId.Value )
                    && a.FinancialPaymentDetail.CurrencyTypeValueId != null
                    && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) )
                .OrderBy( a => a.Name )
                .Include( a => a.FinancialPaymentDetail )
                .AsNoTracking()
                .ToList();

            return savedAccounts
                .Select( a => new ScheduledTransactionPaymentMethodBag
                {
                    Value = a.Guid.ToString(),
                    IsExistingPaymentMethod = false,
                    IsCreditCard = IsCreditCardPaymentDetail( a.FinancialPaymentDetail ),
                    Name = a.Name,
                    CardType = GetPaymentMethodCardType( a.FinancialPaymentDetail ),
                    AccountNumberMasked = a.FinancialPaymentDetail?.AccountNumberMasked,
                    ExpirationDate = a.FinancialPaymentDetail?.ExpirationDate
                } )
                .ToList();
        }

        /// <summary>
        /// Determines whether a payment detail represents a credit card (as opposed to ACH),
        /// based on its currency type.
        /// </summary>
        /// <param name="paymentDetail">The financial payment detail.</param>
        /// <returns><c>true</c> when the currency type is credit card; otherwise <c>false</c>.</returns>
        private bool IsCreditCardPaymentDetail( FinancialPaymentDetail paymentDetail )
        {
            if ( paymentDetail?.CurrencyTypeValueId == null )
            {
                return false;
            }

            var creditCardCurrencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );

            return creditCardCurrencyId.HasValue && paymentDetail.CurrencyTypeValueId == creditCardCurrencyId;
        }

        /// <summary>
        /// Gets the display card type for a payment detail: the credit card type when present,
        /// otherwise the currency type (for example, "Visa" or "ACH").
        /// </summary>
        /// <param name="paymentDetail">The financial payment detail.</param>
        /// <returns>The card/currency type value, or <c>null</c> when unavailable.</returns>
        private string GetPaymentMethodCardType( FinancialPaymentDetail paymentDetail )
        {
            if ( paymentDetail == null )
            {
                return null;
            }

            if ( paymentDetail.CreditCardTypeValueId.HasValue )
            {
                return DefinedValueCache.Get( paymentDetail.CreditCardTypeValueId.Value )?.Value;
            }

            if ( paymentDetail.CurrencyTypeValueId.HasValue )
            {
                return DefinedValueCache.Get( paymentDetail.CurrencyTypeValueId.Value )?.Value;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the current person may edit the given scheduled transaction.
        /// When impersonation is allowed, any transaction may be edited; otherwise the transaction
        /// must belong to the current person's giving unit (their own GivingId, or the GivingId of
        /// a business they manage).
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction to check.</param>
        /// <returns><c>true</c> if the current person may edit the transaction; otherwise <c>false</c>.</returns>
        private bool IsAuthorizedToEditScheduledTransaction( FinancialScheduledTransaction scheduledTransaction )
        {
            if ( GetAttributeValue( AttributeKey.AllowImpersonation ).AsBoolean() )
            {
                return true;
            }

            var currentPerson = RequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return false;
            }

            var transactionGivingId = scheduledTransaction.AuthorizedPersonAlias?.Person?.GivingId;
            if ( transactionGivingId.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var personService = new PersonService( RockContext );
            var validGivingIds = new List<string> { currentPerson.GivingId };
            validGivingIds.AddRange( personService.GetBusinesses( currentPerson.Id ).Select( b => b.GivingId ) );

            return validGivingIds.Contains( transactionGivingId );
        }

        /// <summary>
        /// Determines whether the scheduled transaction is an Event Registration transaction,
        /// which cannot be edited by this block.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction to check.</param>
        /// <returns><c>true</c> if the transaction is an Event Registration type; otherwise <c>false</c>.</returns>
        private bool IsEventRegistrationTransactionType( FinancialScheduledTransaction scheduledTransaction )
        {
            var eventRegistrationTransactionTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() );

            return eventRegistrationTransactionTypeValueId.HasValue
                && eventRegistrationTransactionTypeValueId == scheduledTransaction?.TransactionTypeValueId;
        }

        /// <summary>
        /// Validates the amounts and dates on an update request before any changes are applied.
        /// The client enforces the common cases for UX, but the block action is a separate,
        /// directly-callable request, so these are re-checked authoritatively on the server.
        /// </summary>
        /// <param name="request">The requested changes.</param>
        /// <param name="editContext">The resolved transaction/gateway context, used to determine the gateway's earliest allowed start date.</param>
        /// <param name="errorMessage">The first validation failure message, or <c>null</c> when valid.</param>
        /// <returns><c>true</c> when the request passes validation; otherwise <c>false</c>.</returns>
        private bool ValidateUpdateRequest( UpdateScheduledTransactionRequestBag request, ScheduledTransactionEditContext editContext, out string errorMessage )
        {
            errorMessage = null;

            var giftTerm = ( GetAttributeValue( AttributeKey.GiftTerm ).IsNotNullOrWhiteSpace() ? GetAttributeValue( AttributeKey.GiftTerm ) : "Gift" ).ToLower();

            var allocatedAmounts = request.AccountAmounts?
                .Where( a => a.Amount.HasValue && a.Amount.Value != 0 )
                .Select( a => a.Amount.Value )
                .ToList() ?? new List<decimal>();

            if ( !allocatedAmounts.Any() )
            {
                errorMessage = "Make sure you've entered an amount for at least one account.";
                return false;
            }

            if ( allocatedAmounts.Any( a => a < 0 ) )
            {
                errorMessage = "Make sure the amount you've entered for each account is a positive amount.";
                return false;
            }

            if ( !request.NextPaymentDate.HasValue )
            {
                errorMessage = $"Please select the next {giftTerm} date.";
                return false;
            }

            var scheduleGatewayComponent = editContext.HostedGatewayComponent as IHostedGatewayComponent;
            if ( scheduleGatewayComponent != null )
            {
                var earliestScheduledStartDate = scheduleGatewayComponent.GetEarliestScheduledStartDate( editContext.FinancialGateway );
                if ( request.NextPaymentDate.Value.Date < earliestScheduledStartDate.Date )
                {
                    errorMessage = $"The next {giftTerm} date must be on or after {earliestScheduledStartDate.ToShortDateString()}.";
                    return false;
                }
            }

            if ( request.EndDate.HasValue && request.EndDate.Value.Date < request.NextPaymentDate.Value.Date )
            {
                errorMessage = $"The end date must be on or after the next {giftTerm} date.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resolves the requested payment method into a <see cref="ReferencePaymentInfo"/>, covering
        /// the three cases the client sends: a newly entered method (gateway token), the transaction's
        /// existing method, or a selected saved account. Mirrors the WebForms payment resolution.
        /// For a new method this creates the gateway customer account, which is a gateway-side effect.
        /// </summary>
        /// <param name="request">The requested changes.</param>
        /// <param name="editContext">The resolved transaction/gateway context.</param>
        /// <param name="errorMessage">The failure message, or <c>null</c> when resolution succeeds.</param>
        /// <returns>The reference payment info, or <c>null</c> when it could not be resolved.</returns>
        private ReferencePaymentInfo BuildReferencePaymentInfo( UpdateScheduledTransactionRequestBag request, ScheduledTransactionEditContext editContext, out string errorMessage )
        {
            errorMessage = null;

            var scheduledTransaction = editContext.ScheduledTransaction;
            var gatewayComponent = editContext.HostedGatewayComponent as IHostedGatewayComponent;
            if ( gatewayComponent == null )
            {
                errorMessage = "This page is not configured to allow edits for the payment gateway associated with the selected transaction.";
                return null;
            }

            // New payment method path
            if ( request.GatewayToken.IsNotNullOrWhiteSpace() )
            {
                var targetPerson = editContext.TargetPerson;
                var newMethodPaymentInfo = new ReferencePaymentInfo
                {
                    FirstName = targetPerson?.FirstName,
                    LastName = targetPerson?.LastName,
                    ReferenceNumber = request.GatewayToken
                };

                var customerToken = gatewayComponent.CreateCustomerAccount( editContext.FinancialGateway, newMethodPaymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    errorMessage = errorMessage.IsNotNullOrWhiteSpace() ? errorMessage : "Unable to create a customer account for the new payment method.";
                    return null;
                }

                newMethodPaymentInfo.GatewayPersonIdentifier = customerToken;
                return newMethodPaymentInfo;
            }

            // Existing payment method path
            if ( request.UseExistingPaymentMethod )
            {
                var paymentDetail = scheduledTransaction.FinancialPaymentDetail;
                var referenceNumber = gatewayComponent.GetReferenceNumber( scheduledTransaction, out _ );

                return new ReferencePaymentInfo
                {
                    GatewayPersonIdentifier = paymentDetail?.GatewayPersonIdentifier,
                    FinancialPersonSavedAccountId = paymentDetail?.FinancialPersonSavedAccountId,
                    ReferenceNumber = referenceNumber
                };
            }

            // Pay with the selected Saved Account Path
            if ( request.SavedAccountGuid.HasValue )
            {
                var savedAccount = new FinancialPersonSavedAccountService( RockContext ).Get( request.SavedAccountGuid.Value );
                if ( savedAccount == null )
                {
                    errorMessage = "The selected saved account could not be found.";
                    return null;
                }

                return savedAccount.GetReferencePayment();
            }

            errorMessage = "Unable to determine the payment method to use.";
            return null;
        }

        /// <summary>
        /// Resolves the requested account/amount allocations into account ids. Rows with no (or zero)
        /// amount are skipped intentionally (they mean "remove this account"), but a non-zero amount
        /// whose account guid cannot be resolved is treated as an error rather than silently dropped,
        /// which would shrink the gift without the giver knowing.
        /// </summary>
        /// <param name="request">The requested changes.</param>
        /// <param name="selectedAccountAmounts">The resolved account id/amount pairs when successful.</param>
        /// <param name="errorMessage">The failure message, or <c>null</c> when resolution succeeds.</param>
        /// <returns><c>true</c> when every non-zero allocation resolved; otherwise <c>false</c>.</returns>
        private bool TryGetSelectedAccountAmounts( UpdateScheduledTransactionRequestBag request, out List<(int AccountId, decimal Amount)> selectedAccountAmounts, out string errorMessage )
        {
            selectedAccountAmounts = new List<(int AccountId, decimal Amount)>();
            errorMessage = null;

            if ( request.AccountAmounts == null )
            {
                return true;
            }

            foreach ( var accountAmount in request.AccountAmounts )
            {
                if ( !accountAmount.Amount.HasValue || accountAmount.Amount.Value == 0 )
                {
                    continue;
                }

                var accountId = FinancialAccountCache.Get( accountAmount.AccountGuid.AsGuid() )?.Id;
                if ( !accountId.HasValue )
                {
                    errorMessage = "One or more of the selected accounts could not be found.";
                    return false;
                }

                selectedAccountAmounts.Add( (accountId.Value, accountAmount.Amount.Value) );
            }

            return true;
        }

        /// <summary>
        /// Saves the newly entered payment method as a reusable saved account for the target person,
        /// copying the payment detail that was just applied to the scheduled transaction, and then
        /// links the scheduled transaction's payment method to that new saved account so the gift is
        /// recorded as being paid by it.
        /// </summary>
        /// <param name="scheduledTransaction">The updated scheduled transaction whose payment detail is copied and then linked to the new saved account.</param>
        /// <param name="targetPerson">The person the saved account belongs to.</param>
        /// <param name="financialGateway">The gateway the saved account is tied to.</param>
        /// <param name="savedAccountName">The name to give the saved account.</param>
        private void SaveNewFinancialPersonSavedAccount( FinancialScheduledTransaction scheduledTransaction, Person targetPerson, FinancialGateway financialGateway, string savedAccountName )
        {
            var paymentDetail = scheduledTransaction.FinancialPaymentDetail;

            var savedAccount = new FinancialPersonSavedAccount
            {
                PersonAliasId = targetPerson.PrimaryAliasId,
                ReferenceNumber = paymentDetail.GatewayPersonIdentifier,
                Name = savedAccountName,
                TransactionCode = scheduledTransaction.TransactionCode,
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

            var savedAccountService = new FinancialPersonSavedAccountService( RockContext );
            savedAccountService.Add( savedAccount );

            // Save first so the new saved account has an Id to reference below.
            RockContext.SaveChanges();

            savedAccount.FinancialPaymentDetail.FinancialPersonSavedAccountId = savedAccount.Id;
            scheduledTransaction.FinancialPaymentDetail.FinancialPersonSavedAccountId = savedAccount.Id;
            RockContext.SaveChanges();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Updates the scheduled transaction with the changes entered by the user and
        /// returns the resolved success Lava, or a validation/error message.
        /// </summary>
        /// <param name="request">The requested changes.</param>
        /// <returns>The result of the update.</returns>
        [BlockAction]
        public BlockActionResult UpdateScheduledTransaction( UpdateScheduledTransactionRequestBag request )
        {
            if ( request == null )
            {
                return ActionOk( new UpdateScheduledTransactionResponseBag
                {
                    IsSuccess = false,
                    ErrorMessage = "No changes were provided."
                } );
            }

            var editContext = GetEditContext( request.ScheduledTransactionGuid.AsGuidOrNull() );
            if ( !editContext.IsValid )
            {
                return ActionOk( new UpdateScheduledTransactionResponseBag
                {
                    IsSuccess = false,
                    ErrorMessage = editContext.ErrorMessage
                } );
            }

            if ( !ValidateUpdateRequest( request, editContext, out var validationErrorMessage ) )
            {
                return ActionOk( new UpdateScheduledTransactionResponseBag
                {
                    IsSuccess = false,
                    ErrorMessage = validationErrorMessage
                } );
            }

            if ( !TryGetSelectedAccountAmounts( request, out var selectedAccountAmounts, out var accountErrorMessage ) )
            {
                return ActionOk( new UpdateScheduledTransactionResponseBag
                {
                    IsSuccess = false,
                    ErrorMessage = accountErrorMessage
                } );
            }

            var referencePaymentInfo = BuildReferencePaymentInfo( request, editContext, out var paymentErrorMessage );
            if ( referencePaymentInfo == null )
            {
                return ActionOk( new UpdateScheduledTransactionResponseBag
                {
                    IsSuccess = false,
                    ErrorMessage = paymentErrorMessage
                } );
            }

            var scheduledTransaction = editContext.ScheduledTransaction;
            var gatewayComponent = editContext.HostedGatewayComponent as IHostedGatewayComponent;

            // Only set StartDate; NextPaymentDate is derived by the gateway/schedule, matching
            // the original WebForms behavior.
            scheduledTransaction.StartDate = request.NextPaymentDate.Value;
            scheduledTransaction.TransactionFrequencyValueId = DefinedValueCache.GetId( request.FrequencyValue.AsGuid() ) ?? scheduledTransaction.TransactionFrequencyValueId;
            scheduledTransaction.EndDate = request.EndDate;

            referencePaymentInfo.Amount = selectedAccountAmounts.Sum( a => a.Amount );
            referencePaymentInfo.AccountAllocations = selectedAccountAmounts
                .Select( a => new FinancialTransactionService.AccountAllocation( a.AccountId, a.Amount ) )
                .ToList();

            var originalGatewayScheduleId = scheduledTransaction.GatewayScheduleId;

            try
            {
                // Keep the stored payment info only when reusing the existing method; otherwise
                // clear it so the new method's details replace it.
                if ( !request.UseExistingPaymentMethod )
                {
                    scheduledTransaction.FinancialPaymentDetail.ClearPaymentInfo();
                }

                if ( !gatewayComponent.UpdateScheduledPayment( scheduledTransaction, referencePaymentInfo, out var updateErrorMessage ) )
                {
                    return ActionOk( new UpdateScheduledTransactionResponseBag
                    {
                        IsSuccess = false,
                        ErrorMessage = updateErrorMessage.IsNotNullOrWhiteSpace() ? updateErrorMessage : "Unable to update the scheduled payment."
                    } );
                }

                scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( referencePaymentInfo, gatewayComponent as GatewayComponent, RockContext );

                var scheduledTransactionDetailService = new FinancialScheduledTransactionDetailService( RockContext );
                var selectedAccountIds = selectedAccountAmounts.Select( a => a.AccountId ).ToList();

                var removedDetails = scheduledTransaction.ScheduledTransactionDetails
                    .Where( d => !selectedAccountIds.Contains( d.AccountId ) )
                    .ToList();

                foreach ( var removedDetail in removedDetails )
                {
                    scheduledTransaction.ScheduledTransactionDetails.Remove( removedDetail );
                    scheduledTransactionDetailService.Delete( removedDetail );
                }

                foreach ( var selectedAccountAmount in selectedAccountAmounts )
                {
                    var scheduledTransactionDetail = scheduledTransaction.ScheduledTransactionDetails
                        .FirstOrDefault( d => d.AccountId == selectedAccountAmount.AccountId );
                    if ( scheduledTransactionDetail == null )
                    {
                        scheduledTransactionDetail = new FinancialScheduledTransactionDetail
                        {
                            AccountId = selectedAccountAmount.AccountId
                        };
                        scheduledTransaction.ScheduledTransactionDetails.Add( scheduledTransactionDetail );
                    }

                    scheduledTransactionDetail.Amount = selectedAccountAmount.Amount;
                }

                RockContext.SaveChanges();
            }
            catch ( Exception )
            {
                // If the gateway assigned a new schedule id before failing, persist it so the
                // gateway schedule is not orphaned from the Rock record.
                if ( scheduledTransaction.GatewayScheduleId.IsNotNullOrWhiteSpace() && originalGatewayScheduleId != scheduledTransaction.GatewayScheduleId )
                {
                    RockContext.SaveChanges();
                }

                throw;
            }

            // If payment method is marked to save as new payment account
            var shouldSaveAccount = request.GatewayToken.IsNotNullOrWhiteSpace()
                && request.SaveMethodToAccount
                && request.SavedAccountName.IsNotNullOrWhiteSpace();

            if ( shouldSaveAccount )
            {
                SaveNewFinancialPersonSavedAccount( scheduledTransaction, editContext.TargetPerson, editContext.FinancialGateway, request.SavedAccountName );
            }

            Task.Run( () => ScheduledGiftWasModifiedMessage.PublishScheduledTransactionEvent( scheduledTransaction.Id, ScheduledGiftEventTypes.ScheduledGiftUpdated ) );

            var mergeFields = RequestContext.GetCommonMergeFields( RequestContext.CurrentPerson );
            mergeFields.Add( "Transaction", scheduledTransaction );
            mergeFields.Add( "Person", scheduledTransaction.AuthorizedPersonAlias?.Person );
            mergeFields.Add( "PaymentDetail", scheduledTransaction.FinancialPaymentDetail );
            mergeFields.Add( "BillingLocation", scheduledTransaction.FinancialPaymentDetail?.BillingLocation );

            var successHtml = GetAttributeValue( AttributeKey.FinishLavaTemplate ).ResolveMergeFields( mergeFields );

            return ActionOk( new UpdateScheduledTransactionResponseBag
            {
                IsSuccess = true,
                SuccessHtml = successHtml
            } );
        }

        #endregion Block Actions

        #region Classes

        /// <summary>
        /// The resolved and validated context needed to edit a scheduled transaction, shared
        /// by the initial block load and the update action so their guard checks cannot drift.
        /// </summary>
        private class ScheduledTransactionEditContext
        {
            /// <summary>
            /// Gets or sets the scheduled transaction being edited. Null when <see cref="IsValid"/> is false.
            /// </summary>
            public FinancialScheduledTransaction ScheduledTransaction { get; set; }

            /// <summary>
            /// Gets or sets the transaction's financial gateway. Null when <see cref="IsValid"/> is false.
            /// </summary>
            public FinancialGateway FinancialGateway { get; set; }

            /// <summary>
            /// Gets or sets the resolved Obsidian-hosted gateway component. Null when <see cref="IsValid"/> is false.
            /// </summary>
            public IObsidianHostedGatewayComponent HostedGatewayComponent { get; set; }

            /// <summary>
            /// Gets or sets the person whose gift is being edited. Null when <see cref="IsValid"/> is false.
            /// </summary>
            public Person TargetPerson { get; set; }

            /// <summary>
            /// Gets or sets the guard failure message, or null/empty when the context is valid.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets a value indicating whether all guard checks passed and the resolved entities are populated.
            /// </summary>
            public bool IsValid => ErrorMessage.IsNullOrWhiteSpace();

        }
        #endregion Classes
    }
}

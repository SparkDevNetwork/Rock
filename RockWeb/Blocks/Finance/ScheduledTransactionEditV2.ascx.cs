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
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Edit an existing scheduled transaction
    /// This is the *Public* block for editing scheduled transactions 
    /// </summary>
    [DisplayName( "Scheduled Transaction Edit (V2)" )]
    [Category( "Finance" )]
    [Description( "Edit an existing scheduled transaction." )]

    #region Block Attributes

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

    [AccountsField(
        "Display Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.",
        IsRequired = false,
        Category = AttributeCategory.None,
        Order = 3 )]

    [BooleanField(
        "Show Additional Accounts",
        Key = AttributeKey.ShowAdditionalAccounts,
        Description = "When enabled, all active financial accounts marked Public will be available for selection, or you can choose 'Additional Accounts' in the setting below to show only certain accounts.",
        TrueText = "Display option for selecting additional accounts",
        FalseText = "Don't display option",
        Category = AttributeCategory.None,
        Order = 4 )]

    [AccountsField(
        "Additional Accounts",
        Key = AttributeKey.AdditionalAccounts,
        Description = "When 'Show Additional Accounts' is enabled, the accounts you choose here will be available for selection.",
        IsRequired = false,
        Category = AttributeCategory.None,
        Order = 5 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 6 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 7 )]

    #region Text Options

    [TextField(
        "Gift Term",
        Key = AttributeKey.GiftTerm,
        DefaultValue = "Gift",
        Category = AttributeCategory.TextOptions,
        Order = 1 )]

    [CodeEditorField(
        "Finish Lava Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the success page. <span class='tip tip-lava'></span>",
        DefaultValue = DefaultFinishLavaTemplate,
        Category = AttributeCategory.TextOptions,
        Order = 2 )]

    [TextField( "Add Account Text",
        Key = AttributeKey.AddAccountText,
        Description = "The button text to display for adding an additional account",
        IsRequired = false,
        DefaultValue = "Add Another Account",
        Category = AttributeCategory.TextOptions,
        Order = 3 )]

    #endregion Text Options

    #region Editing Options

    [BooleanField(
        "Impersonator",
        Key = AttributeKey.AllowImpersonation,
        Description = "Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.EditingOptions,
        Order = 1 )]

    [BooleanField(
        "Impersonator can see saved accounts",
        Key = AttributeKey.ImpersonatorCanSeeSavedAccounts,
        Description = "Should the current user be able to view other people's saved accounts?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.EditingOptions,
        Order = 2 )]

    #endregion Advanced options

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18" )]
    public partial class ScheduledTransactionEditV2 : RockBlock
    {
        #region constants

        protected const string DefaultFinishLavaTemplate = @"
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
        #endregion constants

        #region fields

        private Control _hostedPaymentInfoControl;

        #endregion fields

        #region Properties

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
                    int financialGatewayId = hfFinancialGatewayId.Value.AsInteger();
                    _financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayId );
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

        /// <summary>
        /// Gets or sets the host payment information submit JavaScript.
        /// </summary>
        /// <value>
        /// The host payment information submit script.
        /// </value>
        protected string HostPaymentInfoSubmitScript
        {
            get
            {
                return ViewState["HostPaymentInfoSubmitScript"] as string;
            }

            set
            {
                ViewState["HostPaymentInfoSubmitScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the accounts that are available for user to add to the list.
        /// </summary>
        protected List<Guid> AvailableAccounts
        {
            get
            {
                var availableAccounts = ViewState["AvailableAccounts"] as List<Guid>;
                if ( availableAccounts == null )
                {
                    availableAccounts = new List<Guid>();
                }

                return availableAccounts;
            }

            set
            {
                ViewState["AvailableAccounts"] = value;
            }
        }

        #endregion Properties

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string AccountsToDisplay = "AccountsToDisplay";
            public const string ShowAdditionalAccounts = "ShowAdditionalAccounts";
            public const string AdditionalAccounts = "AdditionalAccounts";
            public const string AddAccountText = "AddAccountText";
            public const string AllowImpersonation = "AllowImpersonation";
            public const string ImpersonatorCanSeeSavedAccounts = "ImpersonatorCanSeeSavedAccounts";
            public const string GiftTerm = "GiftTerm";
            public const string AskForCampusIfKnown = "AskForCampusIfKnown";
            public const string EnableMultiAccount = "EnableMultiAccount";
            public const string FinishLavaTemplate = "FinishLavaTemplate";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        public static class AttributeCategory
        {
            public const string None = "";

            public const string TextOptions = "Text Options";

            public const string EditingOptions = "Editing Options";

            public const string Advanced = "Advanced";
        }

        #endregion Attribute Categories

        #region PageParameterKeys

        public static class PageParameterKey
        {
            [RockObsolete( "1.13.1" )]
            [Obsolete( "Pass the GUID instead using the key ScheduledTransactionGuid." )]
            public const string ScheduledTransactionId = "ScheduledTransactionId";

            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
        }

        #endregion PageParameterKeys

        #region Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            hfScheduledTransactionGuid.Value = GetScheduledTransactionGuidFromUrl()?.ToString();

            var scheduledTransaction = this.GetFinancialScheduledTransaction( new RockContext() );

            if ( scheduledTransaction == null )
            {
                // NOTE: Also verified in ShowDetails()
                ShowConfigurationMessage( NotificationBoxType.Warning, "Warning", "Scheduled Transaction not found." );
                pnlPromptForChanges.Visible = false;
                return;
            }
            else if ( IsEventRegistrationTransactionType( scheduledTransaction ) )
            {
                // NOTE: Also verified in ShowDetails()
                ShowConfigurationMessage( NotificationBoxType.Warning, "Warning", "Event Registration Scheduled Transactions cannot be updated." );
                pnlPromptForChanges.Visible = false;
                return;
            }

            hfFinancialGatewayId.Value = scheduledTransaction.FinancialGatewayId.ToString();
            var financialGateway = this.FinancialGateway;
            var financialGatewayComponent = this.FinancialGatewayComponent;
            if ( financialGateway == null || financialGatewayComponent == null )
            {
                return;
            }

            bool enableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            bool enableCreditCard = this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();

            if ( enableACH == false && enableCreditCard == false )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Configuration", "Enable ACH and/or Enable Credit Card needs to be enabled." );
                pnlPromptForChanges.Visible = false;
                return;
            }

            _hostedPaymentInfoControl = financialGatewayComponent.GetHostedPaymentInfoControl( financialGateway, "_hostedPaymentInfoControl", new HostedPaymentInfoControlOptions { EnableACH = enableACH, EnableCreditCard = enableCreditCard } );
            phHostedPaymentControl.Controls.Add( _hostedPaymentInfoControl );
            this.HostPaymentInfoSubmitScript = financialGatewayComponent.GetHostPaymentInfoSubmitScript( financialGateway, _hostedPaymentInfoControl );

            if ( _hostedPaymentInfoControl is IHostedGatewayPaymentControlTokenEvent )
            {
                ( _hostedPaymentInfoControl as IHostedGatewayPaymentControlTokenEvent ).TokenReceived += _hostedPaymentInfoControl_TokenReceived;
            }

            btnAddAccount.Title = GetAttributeValue( AttributeKey.AddAccountText );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // If block options where changed, reload the whole page since changing some of the options (Gateway ACH Control options ) requires a full page reload
            this.NavigateToCurrentPageReference();
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
                nbPaymentTokenError.Text = e.ErrorMessage;
                nbPaymentTokenError.Visible = true;
            }
            else
            {
                nbPaymentTokenError.Visible = false;
                UpdateScheduledPayment( true, e.Token );
            }
        }

        #region methods

        /// <summary>
        /// Gets the scheduled transaction Guid based on what is specified in the URL
        /// </summary>
        /// <param name="refresh">if set to <c>true</c> [refresh].</param>
        /// <returns></returns>
        private Guid? GetScheduledTransactionGuidFromUrl()
        {
            var financialScheduledTransactionGuid = PageParameter( PageParameterKey.ScheduledTransactionGuid ).AsGuidOrNull();

#pragma warning disable CS0618
            var financialScheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
#pragma warning restore CS0618

            if ( financialScheduledTransactionGuid.HasValue )
            {
                return financialScheduledTransactionGuid.Value;
            }

            if ( financialScheduledTransactionId.HasValue )
            {
                return new FinancialScheduledTransactionService( new RockContext() ).GetGuid( financialScheduledTransactionId.Value );
            }

            return null;
        }

        /// <summary>
        /// Gets the financial scheduled transaction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private FinancialScheduledTransaction GetFinancialScheduledTransaction( RockContext rockContext )
        {
            Guid? scheduledTransactionGuid = hfScheduledTransactionGuid.Value.AsGuidOrNull();
            if ( !scheduledTransactionGuid.HasValue )
            {
                return null;
            }

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var scheduledTransactionQuery = financialScheduledTransactionService
                .Queryable().Include( i => i.AuthorizedPersonAlias.Person )
                .Where( t => t.Guid == scheduledTransactionGuid );

            // If the block allows impersonation then just get the scheduled transaction, otherwise use the code below to filter by the current person
            if ( !GetAttributeValue( AttributeKey.AllowImpersonation ).AsBoolean() )
            {
                var personService = new PersonService( rockContext );
                var validGivingIds = new List<string> { CurrentPerson.GivingId };
                validGivingIds.AddRange( personService.GetBusinesses( CurrentPerson.Id ).Select( b => b.GivingId ) );

                scheduledTransactionQuery.Where( t =>
                     t.AuthorizedPersonAlias != null &&
                     t.AuthorizedPersonAlias.Person != null &&
                     validGivingIds.Contains( t.AuthorizedPersonAlias.Person.GivingId ) );
            }

            var scheduledTransaction = scheduledTransactionQuery.FirstOrDefault();
            return scheduledTransaction;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var rockContext = new RockContext();
            var scheduledTransaction = this.GetFinancialScheduledTransaction( rockContext );

            if ( scheduledTransaction == null )
            {
                // Note: Also verified in OnInit
                ShowConfigurationMessage( NotificationBoxType.Warning, "Warning", "Scheduled Transaction not found." );
                return;
            }            
            else if ( IsEventRegistrationTransactionType( scheduledTransaction ) )
            {
                // NOTE: Also verified in ShowDetails()
                ShowConfigurationMessage( NotificationBoxType.Warning, "Warning", "Event Registration Scheduled Transactions cannot be updated." );
                return;
            }

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            string errorMessages;
            if ( !financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessages ) )
            {
                ShowConfigurationMessage( NotificationBoxType.Danger, "Error", errorMessages );
                return;
            }

            hfScheduledTransactionGuid.Value = scheduledTransaction.Guid.ToString();

            var selectableAccountGuids = this.GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList();

            var currentTransactionAccountGuids = scheduledTransaction.ScheduledTransactionDetails.Select( d => d.Account.Guid ).ToList();
            foreach ( var currentTransactionAccountGuid in currentTransactionAccountGuids )
            {
                if ( !selectableAccountGuids.Contains( currentTransactionAccountGuid ) )
                {
                    selectableAccountGuids.Add( currentTransactionAccountGuid );
                }
            }

            if ( this.GetAttributeValue( AttributeKey.ShowAdditionalAccounts ).AsBoolean() )
            {
                var publicAccountGuids = new FinancialAccountService( rockContext ).Queryable()
                    .Where( f =>
                        f.IsActive &&
                        f.IsPublic.HasValue &&
                        f.IsPublic.Value &&
                        ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                        ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
                    .Select( f => f.Guid )
                    .ToList();

                // Limit to user selected additional accounts (if set).
                var additionalAccountGuids = this.GetAttributeValues( AttributeKey.AdditionalAccounts ).AsGuidList();
                if ( additionalAccountGuids.Any() )
                {
                    publicAccountGuids = publicAccountGuids.Where( v => additionalAccountGuids.Contains( v ) ).ToList();
                }

                if ( !selectableAccountGuids.Any() )
                {
                    selectableAccountGuids = publicAccountGuids;
                }
                else
                {
                    var unselectedPublicAccountGuids = publicAccountGuids.Where( g => !selectableAccountGuids.Contains( g ) ).ToList();
                    AvailableAccounts = unselectedPublicAccountGuids;
                }

                BindAddAccountButton();
            }

            List<int> selectableAccountIds = FinancialAccountCache.GetByGuids( selectableAccountGuids ).Select( a => a.Id ).ToList();

            CampusAccountAmountPicker.AccountIdAmount[] accountAmounts = scheduledTransaction.ScheduledTransactionDetails.Select( a => new CampusAccountAmountPicker.AccountIdAmount( a.AccountId, a.Amount ) ).ToArray();

            // if the scheduledTransaction already has Multiple Account, enabled multi account mode. Otherwise, only enabled multi account based on the block setting.
            var hasMultipleAccounts = accountAmounts.Length > 1;

            bool enableMultiAccount = hasMultipleAccounts || this.GetAttributeValue( AttributeKey.EnableMultiAccount ).AsBoolean();
            if ( enableMultiAccount )
            {
                caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.MultipleAccounts;
            }
            else
            {
                caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;
            }

            caapPromptForAccountAmounts.AskForCampusIfKnown = this.GetAttributeValue( AttributeKey.AskForCampusIfKnown ).AsBoolean();

            caapPromptForAccountAmounts.SelectableAccountIds = selectableAccountIds.ToArray();

            if ( !caapPromptForAccountAmounts.SelectableAccountIds.Any() )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Configuration", "At least one Financial Account must be selected in the configuration for this block." );
                pnlPromptForChanges.Visible = false;
                return;
            }

            if ( this.FinancialGateway == null )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Configuration", "Unable to determine the financial gateway for this scheduled transaction." );
                pnlPromptForChanges.Visible = false;
                return;
            }

            if ( this.FinancialGatewayComponent == null || !( this.FinancialGatewayComponent is IHostedGatewayComponent ) )
            {
                ShowConfigurationMessage( NotificationBoxType.Danger, "Configuration", "This page is not configured to allow edits for the payment gateway associated with the selected transaction." );
                pnlPromptForChanges.Visible = false;
                return;
            }

            caapPromptForAccountAmounts.AccountAmounts = accountAmounts;

            var targetPerson = scheduledTransaction.AuthorizedPersonAlias.Person;

            SetAccountPickerCampus( targetPerson );

            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;

            ddlFrequency.Items.Clear();
            var supportedFrequencies = this.FinancialGatewayComponent.SupportedPaymentSchedules;

            foreach ( var supportedFrequency in supportedFrequencies )
            {
                // If this isn't a one-time scheduled transaction, don't allow changing scheduled transaction to a one-time,
                if ( scheduledTransaction.TransactionFrequencyValueId == oneTimeFrequencyId || supportedFrequency.Id != oneTimeFrequencyId )
                {
                    ddlFrequency.Items.Add( new ListItem( supportedFrequency.Value, supportedFrequency.Id.ToString() ) );
                }
            }

            ddlFrequency.SetValue( scheduledTransaction.TransactionFrequencyValueId );

            /* 2020-02-26 MDP: Payment prompt behavior..
                - No Saved Accounts
                    - Show text with existing payment method with a 'Change' link.
                    - If 'Change' is clicked, existing payment info prompt will disappear and hosted payment will be displayed
                - Has Saved Accounts
                    - Show RadioButtons with first item with the existing payment as the option, followed by saved accounts
                    - Then under the RadioButtons show a 'Add Method'.
                    - If 'Add Method' is clicked, RadioButtons will disappear and hosted payment will be displayed
             */

            string paymentName;

            if ( scheduledTransaction.FinancialPaymentDetail.FinancialPersonSavedAccountId.HasValue )
            {
                paymentName = scheduledTransaction.FinancialPaymentDetail.FinancialPersonSavedAccount.Name;
            }
            else
            {
                paymentName = scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValue?.Value;
            }

            string existingPaymentInfoDisplayText;
            if ( scheduledTransaction.FinancialPaymentDetail.ExpirationDate.IsNotNullOrWhiteSpace() )
            {
                existingPaymentInfoDisplayText = $"Existing Payment Method - {paymentName} ({scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked} Expires: {scheduledTransaction.FinancialPaymentDetail.ExpirationDate})";
            }
            else
            {
                existingPaymentInfoDisplayText = $"Existing Payment Method - {paymentName} ({scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked})";
            }

            lUseExistingPaymentMethodNoSavedAccounts.Text = existingPaymentInfoDisplayText;

            var personSavedAccountList = GetSavedAccounts();

            pnlHostedPaymentControl.Visible = false;

            if ( personSavedAccountList.Any() )
            {
                pnlUseExistingPaymentWithSavedAccounts.Visible = true;
                pnlUseExistingPaymentNoSavedAccounts.Visible = false;
                BindPersonSavedAccounts( personSavedAccountList );
                rblExistingPaymentOrPersonSavedAccount.Items.Insert( 0, new ListItem( existingPaymentInfoDisplayText, "0" ) );

                // default to using existing payment method
                rblExistingPaymentOrPersonSavedAccount.SetValue( 0 );
            }
            else
            {
                // no saved account, so just prompt for payment info (or using existing payment info)
                pnlUseExistingPaymentNoSavedAccounts.Visible = true;
                pnlUseExistingPaymentWithSavedAccounts.Visible = false;
            }

            dtpStartDate.SelectedDate = scheduledTransaction.NextPaymentDate;

            // NOTE: Depending on the Gateway, the earliest date could be more than 1-2+ days in the future
            var earliestScheduledStartDate = FinancialGatewayComponent.GetEarliestScheduledStartDate( FinancialGateway );

            if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate.Value < earliestScheduledStartDate )
            {
                dtpStartDate.SelectedDate = earliestScheduledStartDate;
            }

            var person = scheduledTransaction.AuthorizedPersonAlias.Person;

            Location billingLocation = null;

            // default to the billing location of the scheduled transaction, or the mailing location if unable to get a billing location from the scheduled transaction.
            if ( scheduledTransaction.FinancialPaymentDetail != null )
            {
                billingLocation = scheduledTransaction.FinancialPaymentDetail.BillingLocation;
            }

            if ( billingLocation == null )
            {
                billingLocation = person.GetMailingLocation();
            }

            acBillingAddress.SetValues( billingLocation );
        }

        /// <summary>
        /// Binds the Add Account button.
        /// </summary>
        private void BindAddAccountButton()
        {
            var showAdditionalAccounts = AvailableAccounts.Any();
            btnAddAccount.Visible = showAdditionalAccounts;
            if ( showAdditionalAccounts )
            {
                var additionalAccounts = FinancialAccountCache.GetByGuids( AvailableAccounts ).Select( a => new { a.Guid, a.PublicName } ).ToList();
                btnAddAccount.DataSource = additionalAccounts;
                btnAddAccount.DataBind();
            }
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
        /// Determines if the financial scheduled transaction is an event registration.
        /// </summary>
        /// <param name="financialScheduledTransaction">The financial scheduled transaction.</param>
        private bool IsEventRegistrationTransactionType( FinancialScheduledTransaction financialScheduledTransaction )
        {
            var eventRegistrationTransactionTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() );

            return eventRegistrationTransactionTypeValueId.HasValue
                && eventRegistrationTransactionTypeValueId == financialScheduledTransaction?.TransactionTypeValueId;
        }

        /// <summary>
        /// Hides the configuration message.
        /// </summary>
        private void HideConfigurationMessage()
        {
            nbConfigurationNotification.Visible = false;
        }

        /// <summary>
        /// Sets the account picker campus from person
        /// </summary>
        private void SetAccountPickerCampus( Person person )
        {
            int? defaultCampusId = null;

            if ( person != null )
            {
                var personCampus = person.GetCampus();
                if ( personCampus != null )
                {
                    defaultCampusId = personCampus.Id;
                }
            }

            caapPromptForAccountAmounts.CampusId = defaultCampusId;
        }

        /// <summary>
        /// Gets the saved accounts.
        /// </summary>
        /// <returns></returns>
        private List<FinancialPersonSavedAccount> GetSavedAccounts()
        {
            var financialGateway = this.FinancialGateway;
            if ( financialGateway == null )
            {
                return new List<FinancialPersonSavedAccount>();
            }

            var rockContext = new RockContext();

            var scheduledTransaction = this.GetFinancialScheduledTransaction( rockContext );
            var targetPersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId;

            if ( targetPersonId != CurrentPersonId )
            {
                if ( GetAttributeValue( AttributeKey.ImpersonatorCanSeeSavedAccounts ).AsBoolean() == false )
                {
                    return new List<FinancialPersonSavedAccount>();
                }
            }

            var personSavedAccountsQuery = new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( targetPersonId )
                .Where( a => !a.IsSystem )
                .AsNoTracking();

            DefinedValueCache[] allowedCurrencyTypes = {
                DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()),
                DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid())
                };

            int[] allowedCurrencyTypeIds = allowedCurrencyTypes.Select( a => a.Id ).ToArray();

            personSavedAccountsQuery = personSavedAccountsQuery.Where( a =>
                a.FinancialGatewayId == financialGateway.Id
                && ( a.FinancialPaymentDetail.CurrencyTypeValueId != null )
                && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );

            var personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).Include( a => a.FinancialPaymentDetail ).AsNoTracking().ToList();

            return personSavedAccountList;
        }

        /// <summary>
        /// Binds the person saved accounts.
        /// </summary>
        private void BindPersonSavedAccounts( List<FinancialPersonSavedAccount> financialPersonSavedAccounts )
        {
            rblExistingPaymentOrPersonSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in financialPersonSavedAccounts )
            {
                string displayName;
                if ( personSavedAccount.FinancialPaymentDetail.ExpirationDate.IsNotNullOrWhiteSpace() )
                {
                    displayName = $"{personSavedAccount.Name} ({personSavedAccount.FinancialPaymentDetail.AccountNumberMasked} Expires: {personSavedAccount.FinancialPaymentDetail.ExpirationDate})";
                }
                else
                {
                    displayName = $"{personSavedAccount.Name} ({personSavedAccount.FinancialPaymentDetail.AccountNumberMasked}";
                }

                rblExistingPaymentOrPersonSavedAccount.Items.Add( new ListItem( displayName, personSavedAccount.Id.ToString() ) );
            }
        }

        #endregion methods

        /// <summary>
        /// Handles the Click event of the btnUpdateScheduledPayment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateScheduledPayment_Click( object sender, EventArgs e )
        {
            UpdateScheduledPayment( false );
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="usePaymentToken">if set to <c>true</c> [use payment token].</param>
        /// <param name="paymentToken">The payment token.</param>
        protected void UpdateScheduledPayment( bool usePaymentToken, string paymentToken = null )
        {
            var giftTerm = this.GetAttributeValue( AttributeKey.GiftTerm );

            if ( dtpStartDate.SelectedDate <= RockDateTime.Today )
            {
                nbUpdateScheduledPaymentWarning.Visible = true;
                nbUpdateScheduledPaymentWarning.Text = string.Format( "When scheduling a {0}, make sure the starting date is in the future (after today)", giftTerm.ToLower() );
                return;
            }

            var rockContext = new RockContext();

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var financialScheduledTransactionDetailService = new FinancialScheduledTransactionDetailService( rockContext );
            Guid scheduledTransactionGuid = hfScheduledTransactionGuid.Value.AsGuid();
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionGuid );

            if ( IsEventRegistrationTransactionType( financialScheduledTransaction ) )
            {
                // Prevent updating Event Registration scheduled transactions.
                nbUpdateScheduledPaymentWarning.Visible = true;
                nbUpdateScheduledPaymentWarning.Text = "Event Registration Scheduled Transactions cannot be updated.";
                return;
            }

            financialScheduledTransaction.StartDate = dtpStartDate.SelectedDate.Value;
            financialScheduledTransaction.TransactionFrequencyValueId = ddlFrequency.SelectedValue.AsInteger();

            ReferencePaymentInfo referencePaymentInfo;

            var person = financialScheduledTransaction.AuthorizedPersonAlias.Person;

            string errorMessage;

            var financialGateway = this.FinancialGateway;
            var financialGatewayComponent = this.FinancialGatewayComponent;
            var existingPaymentOrPersonSavedAccountId = rblExistingPaymentOrPersonSavedAccount.SelectedValue.AsInteger();

            bool useExistingPaymentMethod = pnlUseExistingPaymentNoSavedAccounts.Visible || existingPaymentOrPersonSavedAccountId == 0;
            bool useSavedAccount = pnlUseExistingPaymentWithSavedAccounts.Visible && existingPaymentOrPersonSavedAccountId > 0;

            if ( usePaymentToken )
            {
                referencePaymentInfo = new ReferencePaymentInfo();
                referencePaymentInfo.FirstName = person.FirstName;
                referencePaymentInfo.LastName = person.LastName;

                referencePaymentInfo.UpdateAddressFieldsFromAddressControl( acBillingAddress );

                referencePaymentInfo.ReferenceNumber = paymentToken;

                var customerToken = financialGatewayComponent.CreateCustomerAccount( this.FinancialGateway, referencePaymentInfo, out errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Text = errorMessage ?? "Unknown Error";
                    nbMessage.Visible = true;
                    return;
                }

                referencePaymentInfo.GatewayPersonIdentifier = customerToken;
            }
            else if ( useExistingPaymentMethod )
            {
                // use save payment method as original transaction
                referencePaymentInfo = new ReferencePaymentInfo();
                referencePaymentInfo.GatewayPersonIdentifier = financialScheduledTransaction.FinancialPaymentDetail.GatewayPersonIdentifier;
                referencePaymentInfo.FinancialPersonSavedAccountId = financialScheduledTransaction.FinancialPaymentDetail.FinancialPersonSavedAccountId;
                referencePaymentInfo.ReferenceNumber = financialGatewayComponent.GetReferenceNumber( financialScheduledTransaction, out errorMessage );
            }
            else if ( useSavedAccount )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( existingPaymentOrPersonSavedAccountId );
                if ( savedAccount != null )
                {
                    referencePaymentInfo = savedAccount.GetReferencePayment();
                }
                else
                {
                    // shouldn't happen
                    throw new Exception( "Unable to determine Saved Account" );
                }
            }
            else
            {
                // shouldn't happen
                throw new Exception( "Unable to determine payment method" );
            }

            var selectedAccountAmounts = caapPromptForAccountAmounts.AccountAmounts.Where( a => a.Amount.HasValue && a.Amount.Value != 0 ).Select( a => new { a.AccountId, Amount = a.Amount.Value } ).ToArray();
            referencePaymentInfo.Amount = selectedAccountAmounts.Sum( a => a.Amount );

            // Validate that an amount was entered
            if ( selectedAccountAmounts.Sum( a => a.Amount ) <= 0 )
            {
                nbUpdateScheduledPaymentWarning.Text = "Make sure you've entered an amount for at least one account";
                nbUpdateScheduledPaymentWarning.Visible = true;
                return;
            }

            // Validate that no negative amounts were entered
            if ( selectedAccountAmounts.Any( a => a.Amount < 0 ) )
            {
                nbUpdateScheduledPaymentWarning.Text = "Make sure the amount you've entered for each account is a positive amount";
                nbUpdateScheduledPaymentWarning.Visible = true;
                return;
            }

            var originalGatewayScheduleId = financialScheduledTransaction.GatewayScheduleId;
            try
            {
                // If we are using the existing payment method, DO NOT clear out the FinancialPaymentDetail record.
                if ( !useExistingPaymentMethod )
                {
                    financialScheduledTransaction.FinancialPaymentDetail.ClearPaymentInfo();
                }

                var successfullyUpdated = financialGatewayComponent.UpdateScheduledPayment( financialScheduledTransaction, referencePaymentInfo, out errorMessage );

                if ( !successfullyUpdated )
                {
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Text = errorMessage ?? "Unknown Error";
                    nbMessage.Visible = true;
                    return;
                }

                financialScheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( referencePaymentInfo, financialGatewayComponent as GatewayComponent, rockContext );

                var selectedAccountIds = selectedAccountAmounts.Select( a => a.AccountId ).ToArray();
                var deletedTransactionDetails = financialScheduledTransaction.ScheduledTransactionDetails.ToList().Where( a => !selectedAccountIds.Contains( a.AccountId ) ).ToList();

                foreach ( var deletedTransactionDetail in deletedTransactionDetails )
                {
                    financialScheduledTransaction.ScheduledTransactionDetails.Remove( deletedTransactionDetail );
                    financialScheduledTransactionDetailService.Delete( deletedTransactionDetail );
                }

                foreach ( var selectedAccountAmount in selectedAccountAmounts )
                {
                    var scheduledTransactionDetail = financialScheduledTransaction.ScheduledTransactionDetails.FirstOrDefault( a => a.AccountId == selectedAccountAmount.AccountId );
                    if ( scheduledTransactionDetail == null )
                    {
                        scheduledTransactionDetail = new FinancialScheduledTransactionDetail();
                        scheduledTransactionDetail.AccountId = selectedAccountAmount.AccountId;
                        financialScheduledTransaction.ScheduledTransactionDetails.Add( scheduledTransactionDetail );
                    }

                    scheduledTransactionDetail.Amount = selectedAccountAmount.Amount;
                }

                rockContext.SaveChanges();
                Task.Run( () => ScheduledGiftWasModifiedMessage.PublishScheduledTransactionEvent( financialScheduledTransaction.Id, ScheduledGiftEventTypes.ScheduledGiftUpdated ) );
            }
            catch ( Exception )
            {
                // if the GatewayScheduleId was updated, but there was an exception,
                // make sure we save the  financialScheduledTransaction record with the updated GatewayScheduleId so we don't orphan it
                if ( financialScheduledTransaction.GatewayScheduleId.IsNotNullOrWhiteSpace() && ( originalGatewayScheduleId != financialScheduledTransaction.GatewayScheduleId ) )
                {
                    rockContext.SaveChanges();
                }

                throw;
            }

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions() );
            var finishLavaTemplate = this.GetAttributeValue( AttributeKey.FinishLavaTemplate );

            // re-fetch financialScheduledTransaction with a new RockContext from database to ensure that lazy loaded fields will be populated
            using ( var rockContextForSummary = new RockContext() )
            {
                if ( pnlHostedPaymentControl.Visible && hfSaveNewAccount.Value.AsInteger() == 1 && tbSaveAccount.Text.IsNotNullOrWhiteSpace() )
                {
                    SaveNewFinancialPersonSavedAccount( financialScheduledTransaction );
                }

                financialScheduledTransaction = new FinancialScheduledTransactionService( rockContextForSummary ).Get( scheduledTransactionGuid );

                mergeFields.Add( "Transaction", financialScheduledTransaction );
                mergeFields.Add( "Person", financialScheduledTransaction.AuthorizedPersonAlias.Person );
                mergeFields.Add( "PaymentDetail", financialScheduledTransaction.FinancialPaymentDetail );
                mergeFields.Add( "BillingLocation", financialScheduledTransaction.FinancialPaymentDetail.BillingLocation );

                pnlPromptForChanges.Visible = false;
                pnlTransactionSummary.Visible = true;

                lTransactionSummaryHTML.Text = finishLavaTemplate.ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveNewFinancialPersonSavedAccount( FinancialScheduledTransaction financialScheduledTransaction )
        {
            var rockContext = new RockContext();

            var scheduledTransaction = this.GetFinancialScheduledTransaction( rockContext );
            var targetPerson = scheduledTransaction.AuthorizedPersonAlias.Person;

            var financialGatewayComponent = this.FinancialGatewayComponent;
            var financialGateway = this.FinancialGateway;

            var savedAccount = new FinancialPersonSavedAccount();

            var paymentDetail = financialScheduledTransaction.FinancialPaymentDetail;

            savedAccount.PersonAliasId = targetPerson.PrimaryAliasId;
            savedAccount.ReferenceNumber = paymentDetail.GatewayPersonIdentifier;
            savedAccount.Name = tbSaveAccount.Text;
            savedAccount.TransactionCode = financialScheduledTransaction.TransactionCode;
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

            savedAccount.FinancialPaymentDetail.FinancialPersonSavedAccountId = savedAccount.Id;
        }

        /// <summary>
        /// Handles the Click event of the btnChangeToHostedPayment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnChangeToHostedPayment_Click( object sender, EventArgs e )
        {
            pnlUseExistingPaymentNoSavedAccounts.Visible = false;
            pnlUseExistingPaymentWithSavedAccounts.Visible = false;
            pnlHostedPaymentControl.Visible = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the btnAddAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddAccount_SelectionChanged( object sender, EventArgs e )
        {
            var addAccountGuid = btnAddAccount.SelectedValueAsGuid();
            if ( addAccountGuid.HasValue )
            {
                var addAccountId = FinancialAccountCache.GetId( addAccountGuid.Value );
                if ( addAccountId .HasValue )
                {
                    var amounts = caapPromptForAccountAmounts.AccountAmounts;
                    var accountList = caapPromptForAccountAmounts.SelectableAccountIds.ToList();
                    accountList.Add( addAccountId.Value );
                    caapPromptForAccountAmounts.SelectableAccountIds = accountList.ToArray();
                    caapPromptForAccountAmounts.AccountAmounts = amounts;
                }

                AvailableAccounts.Remove( addAccountGuid.Value );
            }

            BindAddAccountButton();
        }
    }

    #endregion Events
}
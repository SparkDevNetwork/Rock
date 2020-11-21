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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    #region Block Attributes

    /// <summary>
    /// Edit an existing scheduled transaction
    /// </summary>
    [DisplayName( "Scheduled Transaction Edit (V2)" )]
    [Category( "Finance" )]
    [Description( "Edit an existing scheduled transaction." )]

    #endregion

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
        "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.",
        Category = AttributeCategory.None,
        Order = 3 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 4 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 5 )]

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

    #endregion Text Options

    #region Advanced options

    [BooleanField(
        "Impersonation",
        Key = AttributeKey.AllowImpersonation,
        Description = "Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 1 )]

    #endregion Advanced options

    #endregion Block Attributes
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

            public const string AllowImpersonation = "AllowImpersonation";

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

            public const string Advanced = "Advanced";
        }

        #endregion Attribute Categories

        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string ScheduledTransactionId = "ScheduledTransactionId";
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

            hfScheduledTransactionId.Value = this.PageParameter( PageParameterKey.ScheduledTransactionId );

            var scheduledTransaction = this.GetFinancialScheduledTransaction( new RockContext() );

            if ( scheduledTransaction == null )
            {
                // NOTE: Also verified in ShowDetails()
                ShowConfigurationMessage( NotificationBoxType.Warning, "Warning", "Scheduled Transaction not found." );
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
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
        /// Gets the financial scheduled transaction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private FinancialScheduledTransaction GetFinancialScheduledTransaction( RockContext rockContext )
        {
            int? scheduledTransactionId = hfScheduledTransactionId.Value.AsIntegerOrNull();

            if ( !scheduledTransactionId.HasValue )
            {
                return null;
            }

            FinancialScheduledTransaction scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).GetInclude( scheduledTransactionId.Value, i => i.AuthorizedPersonAlias.Person );

            if ( scheduledTransaction != null )
            {
                return scheduledTransaction;
            }

            return null;
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

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            string errorMessages;
            if ( !financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessages ) )
            {
                ShowConfigurationMessage( NotificationBoxType.Danger, "Error", errorMessages );
                return;
            }

            hfScheduledTransactionId.Value = scheduledTransaction.Id.ToString();

            List<int> selectableAccountIds = new FinancialAccountService( rockContext ).GetByGuids( this.GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList() ).Select( a => a.Id ).ToList();

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
                    - Then under the Radiobuttons show a 'Add Method'.
                    - If 'Add Method' is clicked, radiobuttons will disappear and hosted payment will be displayed
             */

            string existingPaymentInfoDisplayText;

            if ( scheduledTransaction.FinancialPaymentDetail.FinancialPersonSavedAccountId.HasValue )
            {
                existingPaymentInfoDisplayText = string.Format( "Existing Payment Method - {0} ({1})", scheduledTransaction.FinancialPaymentDetail.FinancialPersonSavedAccount.Name, scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked );
            }
            else
            {
                existingPaymentInfoDisplayText = string.Format( "Existing Payment Method - {0} ({1})", scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValue, scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked );
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

        private List<PersonSavedAccountInfo> GetSavedAccounts()
        {
            var financialGateway = this.FinancialGateway;
            if ( financialGateway == null )
            {
                return new List<PersonSavedAccountInfo>();
            }

            var rockContext = new RockContext();

            var scheduledTransaction = this.GetFinancialScheduledTransaction( rockContext );
            var targetPersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId;

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

            List<PersonSavedAccountInfo> personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).AsNoTracking().Select( a => new PersonSavedAccountInfo
            {
                Id = a.Id,
                Name = a.Name,
                GatewayPersonIdentifier = a.GatewayPersonIdentifier,
                AccountNumberMasked = a.FinancialPaymentDetail.AccountNumberMasked,
            } ).ToList();

            return personSavedAccountList;
        }

        /// <summary>
        /// Binds the person saved accounts.
        /// </summary>
        private void BindPersonSavedAccounts( List<PersonSavedAccountInfo> personSavedAccountInfoList )
        {
            rblExistingPaymentOrPersonSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in personSavedAccountInfoList )
            {
                var displayName = string.Format( "{0} ({1})", personSavedAccount.Name, personSavedAccount.AccountNumberMasked );
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
            int scheduledTransactionId = hfScheduledTransactionId.Value.AsInteger();
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionId );

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

            var originalGatewayScheduleId = financialScheduledTransaction.GatewayScheduleId;
            try
            {
                financialScheduledTransaction.FinancialPaymentDetail.ClearPaymentInfo();
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
            }
            catch ( Exception )
            {
                // if the GatewayScheduleId was updated, but there was an exception,
                // make sure we save the  financialScheduledTransaction record with the updated GatewaayScheduleId so we don't orphan it
                if ( financialScheduledTransaction.GatewayScheduleId.IsNotNullOrWhiteSpace() && ( originalGatewayScheduleId != financialScheduledTransaction.GatewayScheduleId ) )
                {
                    rockContext.SaveChanges();
                }

                throw;
            }

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            var finishLavaTemplate = this.GetAttributeValue( AttributeKey.FinishLavaTemplate );

            // refetch financialScheduledTransaction with a new rockcontex from database to ensure that lazy loaded fields will be populated
            using ( var rockContextForSummary = new RockContext() )
            {
                if ( pnlHostedPaymentControl.Visible && hfSaveNewAccount.Value.AsInteger() == 1 && tbSaveAccount.Text.IsNotNullOrWhiteSpace() )
                {
                    SaveNewFinancialPersonSavedAccount( financialScheduledTransaction );
                }

                financialScheduledTransaction = new FinancialScheduledTransactionService( rockContextForSummary ).Get( scheduledTransactionId );

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
            savedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
            savedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
            savedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
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

        private class PersonSavedAccountInfo
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string GatewayPersonIdentifier { get; set; }

            public string AccountNumberMasked { get; set; }
        }
    }

    #endregion Events
}
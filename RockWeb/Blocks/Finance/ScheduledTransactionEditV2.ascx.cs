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
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 2 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. By default all active accounts with a Public Name will be displayed. If the account has a child account for the selected campus, the child account for that campus will be used.",
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
        protected static class AttributeKey
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
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "Invalid Scheduled Transaction";
                nbMessage.Text = "The scheduled transaction you've selected either does not exist or is not valid.";

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
        private void _hostedPaymentInfoControl_TokenReceived( object sender, EventArgs e )
        {
            string errorMessage = null;
            string token = this.FinancialGatewayComponent.GetHostedPaymentInfoToken( this.FinancialGateway, _hostedPaymentInfoControl, out errorMessage );
            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                nbPaymentTokenError.Text = errorMessage;
                nbPaymentTokenError.Visible = true;
            }
            else
            {
                nbPaymentTokenError.Visible = false;
                UpdateScheduledPayment( true, token );
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

            FinancialScheduledTransaction scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( scheduledTransactionId.Value );

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
            var scheduledTransaction = this.GetFinancialScheduledTransaction( new RockContext() );

            if ( scheduledTransaction == null )
            {
                // todo: show a warning
                return;
            }

            hfScheduledTransactionId.Value = scheduledTransaction.Id.ToString();

            var accountAmounts = scheduledTransaction.ScheduledTransactionDetails.Select( a => new CampusAccountAmountPicker.AccountIdAmount( a.AccountId, a.Amount ) ).ToArray();

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

            caapPromptForAccountAmounts.AccountAmounts = accountAmounts;

            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;

            ddlFrequency.Items.Clear();
            var supportedFrequencies = this.FinancialGatewayComponent.SupportedPaymentSchedules;
            foreach ( var supportedFrequency in supportedFrequencies.Where( a => a.Id != oneTimeFrequencyId ) )
            {
                ddlFrequency.Items.Add( new ListItem( supportedFrequency.Value, supportedFrequency.Id.ToString() ) );
            }

            ddlFrequency.SetValue( scheduledTransaction.TransactionFrequencyValueId );

            BindPersonSavedAccounts();

            dtpStartDate.SelectedDate = scheduledTransaction.NextPaymentDate;

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
        /// Binds the person saved accounts.
        /// </summary>
        private void BindPersonSavedAccounts()
        {
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

            var financialGateway = this.FinancialGateway;
            if ( financialGateway == null )
            {
                return;
            }

            personSavedAccountsQuery = personSavedAccountsQuery.Where( a =>
                a.FinancialGatewayId == financialGateway.Id
                && ( a.FinancialPaymentDetail.CurrencyTypeValueId != null )
                && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );

            var personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).AsNoTracking().Select( a => new
            {
                a.Id,
                a.Name,
                a.GatewayPersonIdentifier,
                a.FinancialPaymentDetail.AccountNumberMasked,
            } ).ToList();

            ddlPersonSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in personSavedAccountList )
            {
                var displayName = string.Format( "{0} ({1})", personSavedAccount.Name, personSavedAccount.AccountNumberMasked );
                ddlPersonSavedAccount.Items.Add( new ListItem( displayName, personSavedAccount.Id.ToString() ) );
            }

            string errorMessage;
            var financialGateComponent = this.FinancialGatewayComponent;
            var gatewayPersonIdentifier = ( financialGateComponent as GatewayComponent ).GetReferenceNumber( scheduledTransaction, out errorMessage );

            int? selectedSavedAccountId = personSavedAccountList.Where( a => a.GatewayPersonIdentifier == gatewayPersonIdentifier ).Select( a => ( int? ) a.Id ).FirstOrDefault();

            ddlPersonSavedAccount.SetValue( selectedSavedAccountId );
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
            int scheduledTransactionId = hfScheduledTransactionId.Value.AsInteger();
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionId );

            financialScheduledTransaction.StartDate = dtpStartDate.SelectedDate.Value;
            financialScheduledTransaction.TransactionFrequencyValueId = ddlFrequency.SelectedValue.AsInteger();

            ReferencePaymentInfo referencePaymentInfo;

            var person = financialScheduledTransaction.AuthorizedPersonAlias.Person;

            string errorMessage;

            var financialGateway = this.FinancialGateway;
            var financialGatewayComponent = this.FinancialGatewayComponent;

            if ( usePaymentToken )
            {
                referencePaymentInfo = new ReferencePaymentInfo();
                referencePaymentInfo.FirstName = person.FirstName;
                referencePaymentInfo.LastName = person.LastName;

                referencePaymentInfo.UpdateAddressFieldsFromAddressControl( acBillingAddress );

                referencePaymentInfo.ReferenceNumber = paymentToken;

                var customerToken = financialGatewayComponent.CreateCustomerAccount( this.FinancialGateway, paymentToken, referencePaymentInfo, out errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Text = errorMessage ?? "Unknown Error";
                    nbMessage.Visible = true;
                    return;
                }

                referencePaymentInfo.GatewayPersonIdentifier = customerToken;
            }
            else
            {
                var savedAccountId = ddlPersonSavedAccount.SelectedValue.AsInteger();

                var savedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( savedAccountId );
                if ( savedAccount != null )
                {
                    referencePaymentInfo = savedAccount.GetReferencePayment();
                }
                else
                {
                    throw new Exception( "Unable to determine Saved Account" );
                }
            }

            var selectedAccountAmounts = caapPromptForAccountAmounts.AccountAmounts.Where( a => a.Amount.HasValue && a.Amount.Value != 0 ).Select( a => new { a.AccountId, Amount = a.Amount.Value } ).ToArray();
            referencePaymentInfo.Amount = selectedAccountAmounts.Sum( a => a.Amount );

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
            var deletedTransactionDetails = financialScheduledTransaction.ScheduledTransactionDetails.ToList().Where( a => selectedAccountIds.Contains( a.AccountId ) ).ToList();

            foreach ( var deletedTransactionDetail in deletedTransactionDetails )
            {
                financialScheduledTransaction.ScheduledTransactionDetails.Remove( deletedTransactionDetail );
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

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            var finishLavaTemplate = this.GetAttributeValue( AttributeKey.FinishLavaTemplate );

            mergeFields.Add( "Transaction", financialScheduledTransaction );
            mergeFields.Add( "Person", financialScheduledTransaction.AuthorizedPersonAlias.Person );
            mergeFields.Add( "PaymentDetail", financialScheduledTransaction.FinancialPaymentDetail );
            mergeFields.Add( "BillingLocation", financialScheduledTransaction.FinancialPaymentDetail.BillingLocation );

            pnlPromptForChanges.Visible = false;
            pnlTransactionSummary.Visible = true;

            lTransactionSummaryHTML.Text = finishLavaTemplate.ResolveMergeFields( mergeFields );
        }
    }

    #endregion Events
}
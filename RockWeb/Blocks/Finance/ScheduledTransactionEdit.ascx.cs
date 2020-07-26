﻿// <copyright>
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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    #region Block Attributes

    /// <summary>
    /// Edit an existing scheduled transaction.
    /// </summary>
    [DisplayName( "Scheduled Transaction Edit" )]
    [Category( "Finance" )]
    [Description( "Edit an existing scheduled transaction." )]

    [BooleanField(
        name: "Impersonation",
        trueText: "Allow (only use on an internal page used by staff)",
        falseText: "Don't Allow",
        description: "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users",
        defaultValue: false,
        key: AttributeKey.Impersonation )]

    [BooleanField(
        name: "Impersonator can see saved accounts",
        trueText: "Allow (only use on an internal page used by staff)",
        falseText: "Don't Allow",
        description: "Should the current user be able to view other people's saved accounts?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users",
        defaultValue: false,
        key: AttributeKey.ImpersonatorCanSeeSavedAccounts )]

    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 1 )]
    [BooleanField( "Additional Accounts", "Display option for selecting additional accounts", "Don't display option",
        "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 2 )]
    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "", 3 )]

    // Text Options

    [TextField( "Panel Title", "The text to display in panel heading", false, "Scheduled Transaction", "Text Options", 4 )]
    [TextField( "Contribution Info Title", "The text to display as heading of section for selecting account and amount.", false, "Contribution Information", "Text Options", 5 )]
    [TextField( "Add Account Text", "The button text to display for adding an additional account", false, "Add Another Account", "Text Options", 6 )]
    [TextField( "Payment Info Title", "The text to display as heading of section for entering credit card or bank account information.", false, "Payment Information", "Text Options", 7 )]
    [TextField( "Confirmation Title", "The text to display as heading of section for confirming information entered.", false, "Confirm Information", "Text Options", 8 )]
    [CodeEditorField( "Confirmation Header", "The text (HTML) to display at the top of the confirmation section.",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the 'Finish' button to complete your transaction.
</p>
", "Text Options", 9 )]
    [CodeEditorField( "Confirmation Footer", "The text (HTML) to display at the bottom of the confirmation section.",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<div class='alert alert-info'>
By clicking the 'finish' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions.
</div>
", "Text Options", 10 )]
    [CodeEditorField( "Success Header", "The text (HTML) to display at the top of the success section.",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
Thank you for your generous contribution.  Your support is helping {{ OrganizationName }} actively
achieve our mission.  We are so grateful for your commitment.
</p>
", "Text Options", 11 )]
    [CodeEditorField( "Success Footer", "The text (HTML) to display at the bottom of the success section.",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, @"", "Text Options", 12 )]

    [WorkflowTypeField(
        name: "Workflow Trigger",
        description: "Workflow types to trigger when an edit is submitted for a schedule.",
        allowMultiple: true,
        required: false,
        order: 13,
        key: AttributeKey.WorkflowType )]

    #endregion

    public partial class ScheduledTransactionEdit : RockBlock
    {
        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The workflow type
            /// </summary>
            public const string WorkflowType = "WorkflowType";

            /// <summary>
            /// Allow impersonation
            /// </summary>
            public const string Impersonation = "Impersonation";

            /// <summary>
            /// The impersonator can see saved accounts
            /// </summary>
            public const string ImpersonatorCanSeeSavedAccounts = "ImpersonatorCanSeeSavedAccounts";
        }

        #region Fields

        protected bool FluidLayout { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the gateway.
        /// </summary>
        protected GatewayComponent Gateway
        {
            get
            {
                if ( _gateway == null && _gatewayGuid.HasValue )
                {
                    _gateway = GatewayContainer.GetComponent( _gatewayGuid.ToString() );
                }

                return _gateway;
            }

            set
            {
                _gateway = value;
                _gatewayGuid = _gateway.TypeGuid;
            }
        }

        private GatewayComponent _gateway;
        private Guid? _gatewayGuid;

        /// <summary>
        /// Gets or sets the accounts that are available for user to add to the list.
        /// </summary>
        protected List<AccountItem> AvailableAccounts
        {
            get
            {
                if ( _availableAccounts == null )
                {
                    _availableAccounts = new List<AccountItem>();
                }

                return _availableAccounts;
            }

            set
            {
                _availableAccounts = value;
            }
        }

        private List<AccountItem> _availableAccounts;

        /// <summary>
        /// Gets or sets the accounts that are currently displayed to the user
        /// </summary>
        protected List<AccountItem> SelectedAccounts
        {
            get
            {
                if ( _selectedAccounts == null )
                {
                    _selectedAccounts = new List<AccountItem>();
                }

                return _selectedAccounts;
            }

            set
            {
                _selectedAccounts = value;
            }
        }

        private List<AccountItem> _selectedAccounts;

        /// <summary>
        /// Gets or sets the target person identifier.
        /// </summary>
        protected int? TargetPersonId { get; set; }

        /// <summary>
        /// Gets or sets the payment scheduled transaction Id.
        /// </summary>
        protected int? ScheduledTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the payment transaction code.
        /// </summary>
        protected string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the payment schedule id.
        /// </summary>
        protected string ScheduleId { get; set; }

        #endregion

        #region base control methods

        protected override object SaveViewState()
        {
            ViewState["Gateway"] = _gatewayGuid;
            ViewState["AvailableAccounts"] = AvailableAccounts;
            ViewState["SelectedAccounts"] = SelectedAccounts;
            ViewState["TargetPersonId"] = TargetPersonId;
            ViewState["ScheduledTransactionId"] = ScheduledTransactionId;
            ViewState["TransactionCode"] = TransactionCode;
            ViewState["ScheduleId"] = ScheduleId;

            return base.SaveViewState();
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _gatewayGuid = ViewState["Gateway"] as Guid?;
            AvailableAccounts = ViewState["AvailableAccounts"] as List<AccountItem>;
            SelectedAccounts = ViewState["SelectedAccounts"] as List<AccountItem>;
            TargetPersonId = ViewState["TargetPersonId"] as int?;
            ScheduledTransactionId = ViewState["ScheduledTransactionId"] as int?;
            TransactionCode = ViewState["TransactionCode"] as string ?? string.Empty;
            ScheduleId = ViewState["ScheduleId"] as string ?? string.Empty;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack )
            {
                lPanelTitle.Text = GetAttributeValue( "PanelTitle" );
                lContributionInfoTitle.Text = GetAttributeValue( "ContributionInfoTitle" );
                lPaymentInfoTitle.Text = GetAttributeValue( "PaymentInfoTitle" );
                lConfirmationTitle.Text = GetAttributeValue( "ConfirmationTitle" );

                var scheduledTransaction = GetScheduledTransaction( true );

                if ( scheduledTransaction != null )
                {
                    Gateway = scheduledTransaction.FinancialGateway.GetGatewayComponent();

                    GetAccounts( scheduledTransaction );
                    SetFrequency( scheduledTransaction );
                    SetSavedAccounts( scheduledTransaction );

                    dtpStartDate.SelectedDate = scheduledTransaction.NextPaymentDate;
                    tbSummary.Text = scheduledTransaction.Summary;

                    hfCurrentPage.Value = "1";
                    RockPage page = Page as RockPage;
                    if ( page != null )
                    {
                        page.PageNavigate += page_PageNavigate;
                        page.AddScriptLink( "~/Scripts/moment-with-locales.min.js" );
                    }

                    FluidLayout = GetAttributeValue( "LayoutStyle" ) == "Fluid";

                    btnAddAccount.Title = GetAttributeValue( "AddAccountText" );

                    RegisterScript();

                    // Resolve the text field merge fields
                    var configValues = new Dictionary<string, object>();
                    lConfirmationHeader.Text = GetAttributeValue( "ConfirmationHeader" ).ResolveMergeFields( configValues );
                    lConfirmationFooter.Text = GetAttributeValue( "ConfirmationFooter" ).ResolveMergeFields( configValues );
                    lSuccessHeader.Text = GetAttributeValue( "SuccessHeader" ).ResolveMergeFields( configValues );
                    lSuccessFooter.Text = ( GetAttributeValue( "SuccessFooter" ) ?? string.Empty ).ResolveMergeFields( configValues );

                    hfPaymentTab.Value = "None";

                    //// Temp values for testing...
                    /*
                    txtCreditCard.Text = "5105105105105100";
                    txtCVV.Text = "023";

                    txtRoutingNumber.Text = "111111118";
                    txtAccountNumber.Text = "1111111111";
                     */
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Hide the error box on every postback
            nbMessage.Visible = false;
            pnlDupWarning.Visible = false;

            if ( !ScheduledTransactionId.HasValue )
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Danger, "Invalid Transaction", "The transaction you've selected either does not exist or is not valid." );
                return;
            }

            var hostedGatewayComponent = this.Gateway as IHostedGatewayComponent;
            bool isHostedGateway = false;
            if ( hostedGatewayComponent != null )
            {
                var scheduledTransaction = GetScheduledTransaction( false );
                if ( scheduledTransaction != null )
                {
                    isHostedGateway = hostedGatewayComponent.GetSupportedHostedGatewayModes( scheduledTransaction.FinancialGateway ).Contains( HostedGatewayMode.Hosted );
                }
            }

            if ( isHostedGateway )
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Danger, "Configuration", "This page is not configured to allow edits for the payment gateway associated with the selected transaction." );
                return;
            }

            // Save amounts from controls to the viewstate list
            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                var hfAccountId = item.FindControl( "hfAccountId" ) as HiddenField;
                var txtAccountAmount = item.FindControl( "txtAccountAmount" ) as RockTextBox;
                if ( hfAccountId != null && txtAccountAmount != null )
                {
                    var selectedAccount = SelectedAccounts.FirstOrDefault( a => a.Id == hfAccountId.ValueAsInt() );
                    if ( selectedAccount != null )
                    {
                        selectedAccount.Amount = txtAccountAmount.Text.AsDecimal();
                    }
                }
            }

            // Update the total amount
            lblTotalAmount.Text = SelectedAccounts.Sum( f => f.Amount ).ToString( "F2" );

            liNone.RemoveCssClass( "active" );
            liCreditCard.RemoveCssClass( "active" );
            liACH.RemoveCssClass( "active" );
            divNonePaymentInfo.RemoveCssClass( "active" );
            divCCPaymentInfo.RemoveCssClass( "active" );
            divACHPaymentInfo.RemoveCssClass( "active" );

            if ( !Gateway.IsUpdatingSchedulePaymentMethodSupported || Gateway is IThreeStepGatewayComponent )
            {
                // This block doesn't support ThreeStepGateway payment entry, but the "No Change" option is OK
                divPaymentMethodModification.Visible = false;
            }

            switch ( hfPaymentTab.Value )
            {
                case "ACH":
                    {
                        liACH.AddCssClass( "active" );
                        divACHPaymentInfo.AddCssClass( "active" );
                        break;
                    }

                case "CreditCard":
                    {
                        liCreditCard.AddCssClass( "active" );
                        divCCPaymentInfo.AddCssClass( "active" );
                        break;
                    }

                default:
                    {
                        liNone.AddCssClass( "active" );
                        divNonePaymentInfo.AddCssClass( "active" );
                        break;
                    }
            }

            // Show or Hide the Credit card entry panel based on if a saved account exists and it's selected or not.
            divNewCard.Style[HtmlTextWriterStyle.Display] = ( rblSavedCC.Items.Count == 0 || rblSavedCC.Items[rblSavedCC.Items.Count - 1].Selected ) ? "block" : "none";

            if ( !Page.IsPostBack )
            {
                SetPage( 1 );

                // Get the list of accounts that can be used
                BindAccounts();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the btnAddAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddAccount_SelectionChanged( object sender, EventArgs e )
        {
            var selected = AvailableAccounts.Where( a => a.Id == ( btnAddAccount.SelectedValueAsId() ?? 0 ) ).ToList();
            AvailableAccounts = AvailableAccounts.Except( selected ).ToList();
            SelectedAccounts.AddRange( selected );

            BindAccounts();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;

            switch ( hfCurrentPage.Value.AsInteger() )
            {
                case 1:

                    if ( ProcessPaymentInfo( out errorMessage ) )
                    {
                        this.AddHistory( "GivingDetail", "1", null );
                        SetPage( 2 );
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Danger, "Oops!", errorMessage );
                    }

                    break;

                case 2:

                    if ( ProcessConfirmation( out errorMessage ) )
                    {
                        this.AddHistory( "GivingDetail", "2", null );
                        SetPage( 3 );
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPrev_Click( object sender, EventArgs e )
        {
            // Previous should only be enabled on the confirmation page (2)
            switch ( hfCurrentPage.Value.AsInteger() )
            {
                case 2:
                    SetPage( 1 );
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();

            string personParam = PageParameter( "Person" );
            if ( !string.IsNullOrWhiteSpace( personParam ) )
            {
                qryParams.Add( "Person", personParam );
            }

            string txnParam = PageParameter( "ScheduledTransactionId" );
            if ( !string.IsNullOrWhiteSpace( txnParam ) )
            {
                qryParams.Add( "ScheduledTransactionId", txnParam );
            }

            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            TransactionCode = string.Empty;

            string errorMessage = string.Empty;
            if ( ProcessConfirmation( out errorMessage ) )
            {
                SetPage( 3 );
            }
            else
            {
                ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
            }
        }

        /// <summary>
        /// Handles the PageNavigate event of the page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        protected void page_PageNavigate( object sender, HistoryEventArgs e )
        {
            int pageId = e.State["GivingDetail"].AsInteger();
            if ( pageId > 0 )
            {
                SetPage( pageId );
            }
        }

        #endregion

        #region  Methods

        #region Initialization

        /// <summary>
        /// Gets the scheduled transaction.
        /// </summary>
        /// <param name="refresh">if set to <c>true</c> [refresh].</param>
        /// <returns></returns>
        private FinancialScheduledTransaction GetScheduledTransaction( bool refresh = false )
        {
            Person targetPerson = null;
            using ( var rockContext = new RockContext() )
            {
                // If impersonation is allowed, and a valid person key was used, set the target to that person
                if ( IsImpersonationAllowed() )
                {
                    string personKey = PageParameter( "Person" );
                    if ( !string.IsNullOrWhiteSpace( personKey ) )
                    {
                        targetPerson = new PersonService( rockContext ).GetByUrlEncodedKey( personKey );
                    }
                }

                if ( targetPerson == null )
                {
                    targetPerson = CurrentPerson;
                }

                // Verify that transaction id is valid for selected person
                if ( targetPerson != null )
                {
                    int txnId = int.MinValue;
                    if ( int.TryParse( PageParameter( "ScheduledTransactionId" ), out txnId ) )
                    {
                        var personService = new PersonService( rockContext );

                        var validGivingIds = new List<string> { targetPerson.GivingId };
                        validGivingIds.AddRange( personService.GetBusinesses( targetPerson.Id ).Select( b => b.GivingId ) );

                        var service = new FinancialScheduledTransactionService( rockContext );
                        var scheduledTransaction = service
                            .Queryable( "AuthorizedPersonAlias.Person,ScheduledTransactionDetails,FinancialGateway,FinancialPaymentDetail.CurrencyTypeValue,FinancialPaymentDetail.CreditCardTypeValue" )
                            .Where( t =>
                                t.Id == txnId &&
                                t.AuthorizedPersonAlias != null &&
                                t.AuthorizedPersonAlias.Person != null &&
                                validGivingIds.Contains( t.AuthorizedPersonAlias.Person.GivingId ) )
                            .FirstOrDefault();

                        if ( scheduledTransaction != null )
                        {
                            if ( scheduledTransaction.AuthorizedPersonAlias != null )
                            {
                                TargetPersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId;
                            }
                            ScheduledTransactionId = scheduledTransaction.Id;

                            if ( scheduledTransaction.FinancialGateway != null )
                            {
                                scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                            }

                            if ( refresh )
                            {
                                string errorMessages = string.Empty;
                                service.GetStatus( scheduledTransaction, out errorMessages );
                                rockContext.SaveChanges();
                            }

                            return scheduledTransaction;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        private void GetAccounts( FinancialScheduledTransaction scheduledTransaction )
        {
            var selectedGuids = GetAttributeValues( "Accounts" ).Select( Guid.Parse ).ToList();
            bool showAll = !selectedGuids.Any();

            bool additionalAccounts = GetAttributeValue( "AdditionalAccounts" ).AsBoolean( true );

            SelectedAccounts = new List<AccountItem>();
            AvailableAccounts = new List<AccountItem>();

            // Enumerate through all active accounts that are public
            foreach ( var account in new FinancialAccountService( new RockContext() ).Queryable()
                .Where( f =>
                    f.IsActive &&
                    f.IsPublic.HasValue &&
                    f.IsPublic.Value &&
                    ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                    ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
                .OrderBy( f => f.Order ) )
            {
                var accountItem = new AccountItem( account.Id, account.Order, account.Name, account.CampusId, account.PublicName );
                if ( showAll )
                {
                    SelectedAccounts.Add( accountItem );
                }
                else
                {
                    if ( selectedGuids.Contains( account.Guid ) )
                    {
                        SelectedAccounts.Add( accountItem );
                    }
                    else
                    {
                        if ( additionalAccounts )
                        {
                            AvailableAccounts.Add( accountItem );
                        }
                    }
                }
            }

            foreach ( var txnDetail in scheduledTransaction.ScheduledTransactionDetails )
            {
                var selectedAccount = SelectedAccounts.Where( a => a.Id == txnDetail.AccountId ).FirstOrDefault();
                if ( selectedAccount != null )
                {
                    selectedAccount.Amount = txnDetail.Amount;
                }
                else
                {
                    var selected = AvailableAccounts.Where( a => a.Id == txnDetail.AccountId ).ToList();
                    if ( selected != null )
                    {
                        selected.ForEach( a => a.Amount = txnDetail.Amount );
                        AvailableAccounts = AvailableAccounts.Except( selected ).ToList();
                        SelectedAccounts.AddRange( selected );
                    }
                }
            }

            BindAccounts();
        }

        /// <summary>
        /// Sets the frequency.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        private void SetFrequency( FinancialScheduledTransaction scheduledTransaction )
        {
            // Enable payment options based on the configured gateways
            bool ccEnabled = false;
            bool achEnabled = false;

            if ( scheduledTransaction != null && Gateway != null )
            {
                if ( scheduledTransaction.FinancialPaymentDetail != null &&
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ).Id )
                {
                    ccEnabled = true;
                    txtCardFirstName.Visible = Gateway.SplitNameOnCard;
                    var authorizedPerson = scheduledTransaction.AuthorizedPersonAlias.Person;
                    txtCardFirstName.Text = authorizedPerson.FirstName;
                    txtCardLastName.Visible = Gateway.SplitNameOnCard;
                    txtCardLastName.Text = authorizedPerson.LastName;
                    txtCardName.Visible = !Gateway.SplitNameOnCard;
                    txtCardName.Text = authorizedPerson.FullName;

                    var groupLocation = new PersonService( new RockContext() ).GetFirstLocation(
                        authorizedPerson.Id, DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id );
                    if ( groupLocation != null )
                    {
                        acBillingAddress.SetValues( groupLocation.Location );
                    }
                    else
                    {
                        acBillingAddress.SetValues( null );
                    }

                    mypExpiration.MinimumYear = RockDateTime.Now.Year;
                }

                if ( scheduledTransaction.FinancialPaymentDetail != null &&
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ).Id )
                {
                    achEnabled = true;
                }

                if ( Gateway.SupportedPaymentSchedules.Any() )
                {
                    var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                    divRepeatingPayments.Visible = true;

                    btnFrequency.DataSource = Gateway.SupportedPaymentSchedules;
                    btnFrequency.DataBind();

                    btnFrequency.SelectedValue = scheduledTransaction.TransactionFrequencyValueId.ToString();
                }

                liCreditCard.Visible = ccEnabled;
                divCCPaymentInfo.Visible = ccEnabled;

                liACH.Visible = achEnabled;
                divACHPaymentInfo.Visible = achEnabled;

                if ( ccEnabled )
                {
                    divCCPaymentInfo.AddCssClass( "tab-pane" );
                }

                if ( achEnabled )
                {
                    divACHPaymentInfo.AddCssClass( "tab-pane" );
                }
            }
        }

        /// <summary>
        /// Binds the saved accounts.
        /// </summary>
        /// <param name="scheduledTransaction"></param>
        private void SetSavedAccounts( FinancialScheduledTransaction scheduledTransaction )
        {
            rblSavedCC.Items.Clear();
            rblSavedAch.Items.Clear();

            var isSelf = TargetPersonId.HasValue && CurrentPerson != null && TargetPersonId == CurrentPerson.Id;
            var savedAccountViewModels = new List<SavedAccountViewModel>();

            if ( isSelf || CanImpersonatorSeeSavedAccounts() )
            {
                // Get the saved accounts for the target person
                var savedAccountQuery = new FinancialPersonSavedAccountService( new RockContext() )
                    .GetByPersonId( TargetPersonId.Value );

                if ( Gateway != null && Gateway.SupportsSavedAccount( true ) )
                {
                    var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                    if ( Gateway.SupportsSavedAccount( ccCurrencyType ) )
                    {
                        var cards = savedAccountQuery
                            .Where( a =>
                                a.FinancialGateway.EntityTypeId == Gateway.TypeId &&
                                a.FinancialPaymentDetail != null &&
                                a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id )
                            .OrderBy( a => a.Name )
                            .Select( a => new SavedAccountViewModel
                            {
                                Id = a.Id,
                                Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")",
                                GatewayPersonIdentifier = a.GatewayPersonIdentifier,
                                ReferenceNumber = a.ReferenceNumber,
                                TransactionCode = a.TransactionCode,
                                IsCard = true
                            } ).ToList();

                        savedAccountViewModels.AddRange(cards);
                        rblSavedCC.DataSource = cards;
                        rblSavedCC.DataBind();

                        if ( rblSavedCC.Items.Count > 0 )
                        {
                            rblSavedCC.Items.Add( new ListItem( "Use a different card", "0" ) );
                        }
                    }

                    var achCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );
                    if ( Gateway.SupportsSavedAccount( achCurrencyType ) )
                    {
                        var bankAccounts = savedAccountQuery
                            .Where( a =>
                                a.FinancialGateway.EntityTypeId == Gateway.TypeId &&
                                a.FinancialPaymentDetail != null &&
                                a.FinancialPaymentDetail.CurrencyTypeValueId == achCurrencyType.Id )
                            .OrderBy( a => a.Name )
                            .Select( a => new SavedAccountViewModel
                            {
                                Id = a.Id,
                                Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")",
                                GatewayPersonIdentifier = a.GatewayPersonIdentifier,
                                ReferenceNumber = a.ReferenceNumber,
                                TransactionCode = a.TransactionCode,
                                IsCard = false
                            } ).ToList();

                        savedAccountViewModels.AddRange( bankAccounts );
                        rblSavedAch.DataSource = bankAccounts;
                        rblSavedAch.DataBind();

                        if ( rblSavedAch.Items.Count > 0 )
                        {
                            rblSavedAch.Items.Add( new ListItem( "Use a different bank account", "0" ) );
                        }
                    }
                }
            }

            if ( rblSavedCC.Items.Count > 0 )
            {
                rblSavedCC.Items[0].Selected = true;
                rblSavedCC.Visible = true;
                divNewCard.Style[HtmlTextWriterStyle.Display] = "none";

                var likelyCurrentCard = savedAccountViewModels.FirstOrDefault( sa =>
                    sa.IsCard &&
                    sa.ReferenceNumber == scheduledTransaction.TransactionCode ||
                    sa.TransactionCode == scheduledTransaction.TransactionCode ||
                    sa.GatewayPersonIdentifier == scheduledTransaction.TransactionCode );

                if ( likelyCurrentCard != null )
                {
                    rblSavedCC.SetValue( likelyCurrentCard.Id );
                }
            }
            else
            {
                rblSavedCC.Visible = false;
                divNewCard.Style[HtmlTextWriterStyle.Display] = "block";
            }

            if ( rblSavedAch.Items.Count > 0 )
            {
                rblSavedAch.Items[0].Selected = true;
                rblSavedAch.Visible = true;
                divNewBank.Style[HtmlTextWriterStyle.Display] = "none";

                var likelyCurrentBankAccount = savedAccountViewModels.FirstOrDefault( sa =>
                    !sa.IsCard &&
                    sa.ReferenceNumber == scheduledTransaction.TransactionCode ||
                    sa.TransactionCode == scheduledTransaction.TransactionCode ||
                    sa.GatewayPersonIdentifier == scheduledTransaction.TransactionCode );

                if ( likelyCurrentBankAccount != null )
                {
                    rblSavedAch.SetValue( likelyCurrentBankAccount.Id );
                }
            }
            else
            {
                rblSavedAch.Visible = false;
                divNewCard.Style[HtmlTextWriterStyle.Display] = "block";
            }
        }

        #endregion

        #region Process User Actions

        /// <summary>
        /// Processes the payment information.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPaymentInfo( out string errorMessage )
        {
            var rockContext = new RockContext();
            errorMessage = string.Empty;

            var errorMessages = new List<string>();

            // Validate that an amount was entered
            if ( SelectedAccounts.Sum( a => a.Amount ) <= 0 )
            {
                errorMessages.Add( "Make sure you've entered an amount for at least one account" );
            }

            // Validate that no negative amounts were entered
            if ( SelectedAccounts.Any( a => a.Amount < 0 ) )
            {
                errorMessages.Add( "Make sure the amount you've entered for each account is a positive amount" );
            }

            string howOften = DefinedValueCache.Get( btnFrequency.SelectedValueAsId().Value ).Value;

            // Make sure a repeating payment starts in the future
            if ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate <= RockDateTime.Today )
            {
                errorMessages.Add( "Make sure the Next Gift date is in the future (after today)" );
            }

            if ( hfPaymentTab.Value == "ACH" )
            {
                // Validate ach options
                if ( rblSavedAch.Items.Count > 0 && ( rblSavedAch.SelectedValueAsInt() ?? 0 ) > 0 )
                {
                    // TODO: Find saved account
                }
                else
                {
                    if ( string.IsNullOrWhiteSpace( txtRoutingNumber.Text ) )
                    {
                        errorMessages.Add( "Make sure to enter a valid routing number" );
                    }

                    if ( string.IsNullOrWhiteSpace( txtAccountNumber.Text ) )
                    {
                        errorMessages.Add( "Make sure to enter a valid account number" );
                    }
                }
            }
            else if ( hfPaymentTab.Value == "CreditCard" )
            {
                // validate cc options
                if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsInt() ?? 0 ) > 0 )
                {
                    // TODO: Find saved card
                }
                else
                {
                    if ( Gateway.SplitNameOnCard )
                    {
                        if ( string.IsNullOrWhiteSpace( txtCardFirstName.Text ) || string.IsNullOrWhiteSpace( txtCardLastName.Text ) )
                        {
                            errorMessages.Add( "Make sure to enter a valid first and last name as it appears on your credit card" );
                        }
                    }
                    else
                    {
                        if ( string.IsNullOrWhiteSpace( txtCardName.Text ) )
                        {
                            errorMessages.Add( "Make sure to enter a valid name as it appears on your credit card" );
                        }
                    }

                    if ( string.IsNullOrWhiteSpace( txtCreditCard.Text ) )
                    {
                        errorMessages.Add( "Make sure to enter a valid credit card number" );
                    }

                    var currentMonth = RockDateTime.Today;
                    currentMonth = new DateTime( currentMonth.Year, currentMonth.Month, 1 );
                    if ( !mypExpiration.SelectedDate.HasValue || mypExpiration.SelectedDate.Value.CompareTo( currentMonth ) < 0 )
                    {
                        errorMessages.Add( "Make sure to enter a valid credit card expiration date" );
                    }

                    if ( string.IsNullOrWhiteSpace( txtCVV.Text ) )
                    {
                        errorMessages.Add( "Make sure to enter a valid credit card security code" );
                    }
                }
            }

            if ( errorMessages.Any() )
            {
                errorMessage = errorMessages.AsDelimited( "<br/>" );
                return false;
            }

            FinancialScheduledTransaction scheduledTransaction = null;

            if ( ScheduledTransactionId.HasValue )
            {
                scheduledTransaction = new FinancialScheduledTransactionService( rockContext )
                    .Queryable( "AuthorizedPersonAlias.Person" ).FirstOrDefault( s => s.Id == ScheduledTransactionId.Value );
            }

            if ( scheduledTransaction == null )
            {
                errorMessage = "There was a problem getting the transaction information";
                return false;
            }

            if ( scheduledTransaction.AuthorizedPersonAlias == null || scheduledTransaction.AuthorizedPersonAlias.Person == null )
            {
                errorMessage = "There was a problem determining the person associated with the transaction";
                return false;
            }

            PaymentInfo paymentInfo = GetPaymentInfo( new PersonService( rockContext ), scheduledTransaction );
            if ( paymentInfo != null )
            {
                tdName.Description = paymentInfo.FullName;
                tdTotal.Description = paymentInfo.Amount.ToString( "C" );

                if ( paymentInfo.CurrencyTypeValue != null )
                {
                    tdPaymentMethod.Description = paymentInfo.CurrencyTypeValue.Description;
                    tdPaymentMethod.Visible = true;
                }
                else
                {
                    tdPaymentMethod.Visible = false;
                }

                if ( string.IsNullOrWhiteSpace( paymentInfo.MaskedNumber ) )
                {
                    tdAccountNumber.Visible = false;
                }
                else
                {
                    tdAccountNumber.Visible = true;
                    tdAccountNumber.Description = paymentInfo.MaskedNumber;
                }
            }

            rptAccountListConfirmation.DataSource = SelectedAccounts.Where( a => a.Amount != 0 );
            rptAccountListConfirmation.DataBind();

            string nextDate = dtpStartDate.SelectedDate.HasValue ? dtpStartDate.SelectedDate.Value.ToShortDateString() : "?";
            string frequency = DefinedValueCache.Get( btnFrequency.SelectedValueAsInt() ?? 0 ).Description;
            tdWhen.Description = frequency + " starting on " + nextDate;

            return true;
        }

        /// <summary>
        /// Processes the confirmation.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessConfirmation( out string errorMessage )
        {
            var rockContext = new RockContext();
            errorMessage = string.Empty;

            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                if ( Gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                var personService = new PersonService( rockContext );
                var transactionService = new FinancialScheduledTransactionService( rockContext );
                var transactionDetailService = new FinancialScheduledTransactionDetailService( rockContext );

                FinancialScheduledTransaction scheduledTransaction = null;

                if ( ScheduledTransactionId.HasValue )
                {
                    scheduledTransaction = transactionService
                        .Queryable( "AuthorizedPersonAlias.Person,FinancialGateway" )
                        .FirstOrDefault( s => s.Id == ScheduledTransactionId.Value );
                }

                if ( scheduledTransaction == null )
                {
                    errorMessage = "There was a problem getting the transaction information";
                    return false;
                }

                if ( scheduledTransaction.FinancialPaymentDetail == null )
                {
                    scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                }

                if ( scheduledTransaction.FinancialGateway != null )
                {
                    scheduledTransaction.FinancialGateway.LoadAttributes();
                }

                if ( scheduledTransaction.AuthorizedPersonAlias == null || scheduledTransaction.AuthorizedPersonAlias.Person == null )
                {
                    errorMessage = "There was a problem determining the person associated with the transaction";
                    return false;
                }

                // Get the payment schedule
                scheduledTransaction.TransactionFrequencyValueId = btnFrequency.SelectedValueAsId().Value;

                // ProcessPaymentInfo ensures that dtpStartDate.SelectedDate has a value and is after today
                scheduledTransaction.StartDate = dtpStartDate.SelectedDate.Value;
                scheduledTransaction.NextPaymentDate = Gateway.CalculateNextPaymentDate( scheduledTransaction, null );

                PaymentInfo paymentInfo = GetPaymentInfo( personService, scheduledTransaction );
                if ( paymentInfo == null )
                {
                    errorMessage = "There was a problem creating the payment information";
                    return false;
                }

                // If transaction is not active, attempt to re-activate it first
                if ( !scheduledTransaction.IsActive )
                {
                    if ( !transactionService.Reactivate( scheduledTransaction, out errorMessage ) )
                    {
                        return false;
                    }
                }

                scheduledTransaction.FinancialPaymentDetail.ClearPaymentInfo();
                if ( Gateway.UpdateScheduledPayment( scheduledTransaction, paymentInfo, out errorMessage ) )
                {
                    if ( hfPaymentTab.Value == "CreditCard" || hfPaymentTab.Value == "ACH" )
                    {
                        scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, Gateway, rockContext );
                    }

                    var selectedAccountIds = SelectedAccounts
                        .Where( a => a.Amount > 0 )
                        .Select( a => a.Id ).ToList();

                    var deletedAccounts = scheduledTransaction.ScheduledTransactionDetails
                        .Where( a => !selectedAccountIds.Contains( a.AccountId ) ).ToList();

                    foreach ( var deletedAccount in deletedAccounts )
                    {
                        scheduledTransaction.ScheduledTransactionDetails.Remove( deletedAccount );
                        transactionDetailService.Delete( deletedAccount );
                    }

                    foreach ( var account in SelectedAccounts
                        .Where( a => a.Amount > 0 ) )
                    {
                        var detail = scheduledTransaction.ScheduledTransactionDetails
                            .Where( d => d.AccountId == account.Id ).FirstOrDefault();
                        if ( detail == null )
                        {
                            detail = new FinancialScheduledTransactionDetail();
                            detail.AccountId = account.Id;
                            scheduledTransaction.ScheduledTransactionDetails.Add( detail );
                        }

                        detail.Amount = account.Amount;
                    }

                    scheduledTransaction.Summary = tbSummary.Text;

                    rockContext.SaveChanges();

                    ScheduleId = scheduledTransaction.GatewayScheduleId;
                    TransactionCode = scheduledTransaction.TransactionCode;

                    if ( transactionService.GetStatus( scheduledTransaction, out errorMessage ) )
                    {
                        rockContext.SaveChanges();
                    }
                }
                else
                {
                    return false;
                }

                tdTransactionCode.Description = TransactionCode;
                tdTransactionCode.Visible = !string.IsNullOrWhiteSpace( TransactionCode );

                tdScheduleId.Description = ScheduleId;
                tdScheduleId.Visible = !string.IsNullOrWhiteSpace( ScheduleId );

                TriggerWorkflows( scheduledTransaction );

                return true;
            }
            else
            {
                pnlDupWarning.Visible = true;
                return false;
            }
        }

        #endregion

        #region Build PaymentInfo

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        /// <returns></returns>
        private PaymentInfo GetPaymentInfo( PersonService personService, FinancialScheduledTransaction scheduledTransaction )
        {
            PaymentInfo paymentInfo = null;
            if ( hfPaymentTab.Value == "ACH" )
            {
                if ( rblSavedAch.Items.Count > 0 && ( rblSavedAch.SelectedValueAsId() ?? 0 ) > 0 )
                {
                    paymentInfo = GetReferenceInfo( rblSavedAch.SelectedValueAsId().Value );
                }
                else
                {
                    paymentInfo = GetACHInfo();
                }
            }
            else if ( hfPaymentTab.Value == "CreditCard" )
            {
                if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
                {
                    paymentInfo = GetReferenceInfo( rblSavedCC.SelectedValueAsId().Value );
                }
                else
                {
                    paymentInfo = GetCCInfo();
                }
            }
            else
            {
                // no change
                paymentInfo = new ReferencePaymentInfo();
            }

            if ( paymentInfo != null )
            {
                paymentInfo.Amount = SelectedAccounts.Sum( a => a.Amount );
                var authorizedPerson = scheduledTransaction.AuthorizedPersonAlias.Person;
                paymentInfo.FirstName = authorizedPerson.FirstName;
                paymentInfo.LastName = authorizedPerson.LastName;
                paymentInfo.Email = authorizedPerson.Email;

                bool displayPhone = GetAttributeValue( "DisplayPhone" ).AsBoolean();
                if ( displayPhone )
                {
                    var phoneNumber = personService.GetPhoneNumber( authorizedPerson, DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );
                    paymentInfo.Phone = phoneNumber != null ? phoneNumber.ToString() : string.Empty;
                }

                Guid addressTypeGuid = Guid.Empty;
                if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                {
                    addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                }

                var groupLocation = personService.GetFirstLocation( authorizedPerson.Id, DefinedValueCache.Get( addressTypeGuid ).Id );
                if ( groupLocation != null && groupLocation.Location != null )
                {
                    paymentInfo.Street1 = groupLocation.Location.Street1;
                    paymentInfo.Street2 = groupLocation.Location.Street2;
                    paymentInfo.City = groupLocation.Location.City;
                    paymentInfo.State = groupLocation.Location.State;
                    paymentInfo.PostalCode = groupLocation.Location.PostalCode;
                    paymentInfo.Country = groupLocation.Location.Country;
                }
            }

            return paymentInfo;
        }

        /// <summary>
        /// Gets the credit card information.
        /// </summary>
        /// <returns></returns>
        private CreditCardPaymentInfo GetCCInfo()
        {
            var cc = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
            cc.NameOnCard = Gateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            cc.LastNameOnCard = txtCardLastName.Text;
            cc.BillingStreet1 = acBillingAddress.Street1;
            cc.BillingStreet2 = acBillingAddress.Street2;
            cc.BillingCity = acBillingAddress.City;
            cc.BillingState = acBillingAddress.State;
            cc.BillingPostalCode = acBillingAddress.PostalCode;
            cc.BillingCountry = acBillingAddress.Country;

            return cc;
        }

        /// <summary>
        /// Gets the ACH information.
        /// </summary>
        /// <returns></returns>
        private ACHPaymentInfo GetACHInfo()
        {
            var ach = new ACHPaymentInfo( txtAccountNumber.Text, txtRoutingNumber.Text, rblAccountType.SelectedValue == "Savings" ? BankAccountType.Savings : BankAccountType.Checking );
            return ach;
        }

        /// <summary>
        /// Gets the reference information.
        /// </summary>
        /// <param name="savedAccountId">The saved account unique identifier.</param>
        /// <returns></returns>
        private ReferencePaymentInfo GetReferenceInfo( int savedAccountId )
        {
            var savedAccount = new FinancialPersonSavedAccountService( new RockContext() ).Get( savedAccountId );
            if ( savedAccount != null )
            {
                return savedAccount.GetReferencePayment();
            }

            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines whether Impersonation is allowed.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is impersonation allowed]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsImpersonationAllowed()
        {
            return GetAttributeValue( AttributeKey.Impersonation ).AsBoolean();
        }

        /// <summary>
        /// Determines whether the impersonator can see saved accounts.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can impersonator can see saved accounts]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanImpersonatorSeeSavedAccounts()
        {
            return GetAttributeValue( AttributeKey.ImpersonatorCanSeeSavedAccounts ).AsBoolean();
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            rptAccountList.DataSource = SelectedAccounts;
            rptAccountList.DataBind();

            btnAddAccount.Visible = AvailableAccounts.Any();
            btnAddAccount.DataSource = AvailableAccounts;
            btnAddAccount.DataBind();
        }

        /// <summary>
        /// Sets the page.
        /// </summary>
        /// <param name="page">The page.</param>
        private void SetPage( int page )
        {
            //// Page 1 = Payment Info
            //// Page 2 = Confirmation
            //// Page 3 = Success
            //// Page 0 = Only message box is displayed

            pnlPaymentInfo.Visible = page == 1;
            pnlConfirmation.Visible = page == 2;
            pnlSuccess.Visible = page == 3;
            divActions.Visible = page > 0;

            btnPrev.Visible = page == 2;
            btnNext.Visible = page < 3;
            btnNext.Text = page > 1 ? "Finish" : "Next";

            hfCurrentPage.Value = page.ToString();
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
                nbMessage.Text = text;
                nbMessage.Title = title;
                nbMessage.NotificationBoxType = type;
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Formats the value as currency (called from markup)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string FormatValueAsCurrency( decimal value )
        {
            return value.FormatAsCurrency();
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterScript()
        {
            RockPage.AddScriptLink( "~/Scripts/jquery.creditCardTypeDetector.js" );

            int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;

            string scriptFormat = @"
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
                        $(this).val(num.toFixed(2));
                        totalAmt = totalAmt + num;
                    }}
                }}
                else {{
                    $(this).parents('div.input-group').removeClass('has-error');
                }}
            }});
            $('.total-amount').html('{4}' + totalAmt.toFixed(2));
            return false;
        }});

        // Set the date prompt based on the frequency value entered
        $('#ButtonDropDown_btnFrequency .dropdown-menu a').on('click', function () {{
            var $when = $(this).parents('div.form-group').first().next();
            if ($(this).attr('data-id') == '{3}') {{
                $when.find('label').first().html('When');
            }} else {{
                $when.find('label').first().html('First Gift');

                // Set date to tomorrow if it is equal or less than today's date
                var $dateInput = $when.find('input');
                var locale = window.navigator.userLanguage || window.navigator.language;
                moment.locale(locale);
                var dt = moment($dateInput.val(), 'l');
                var curr = moment();
                if ( (dt-curr) <= 0 ) {{
                    curr = curr.add(1, 'day');

                    $dateInput.val(curr.format('l'));
                    //$dateInput.data('datePicker').value(curr.format('l'));
                }}
            }};
        }});

        // Save the state of the selected payment type pill to a hidden field so that state can
        // be preserved through postback
        $('a[data-toggle=""pill""]').on('shown.bs.tab', function (e) {{
            var tabHref = $(e.target).attr(""href"");
            if (tabHref == '#{0}') {{
                $('#{2}').val('CreditCard');
            }} else if (tabHref == '#{1}') {{
                $('#{2}').val('ACH');
            }} else {{
                $('#{2}').val('None');
            }}
        }});

        // Detect credit card type
        $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card-logos' }});

        // Toggle credit card display if saved card option is available
        $('div.radio-content').prev('.form-group').find('input:radio').unbind('click').on('click', function () {{
            var $content = $(this).parents('div.form-group').first().next('.radio-content')
            var radioDisplay = $content.css('display');
            if ($(this).val() == 0 && radioDisplay == 'none') {{
                $content.slideToggle();
            }}
            else if ($(this).val() != 0 && radioDisplay != 'none') {{
                $content.slideToggle();
            }}
        }});

        // Hide or show a div based on selection of checkbox
        $('input:checkbox.toggle-input').unbind('click').on('click', function () {{
            $(this).parents('.checkbox').next('.toggle-content').slideToggle();
        }});

        // Disable the submit button as soon as it's clicked to prevent double-clicking
        $('a[id$=""btnNext""]').on('click', function() {{
			$(this).addClass('disabled');
			$(this).unbind('click');
			$(this).on('click', function () {{
				return false;
			}});
        }});
    }});

";
            string script = string.Format(
                scriptFormat,
                divCCPaymentInfo.ClientID, // {0}
                divACHPaymentInfo.ClientID, // {1}
                hfPaymentTab.ClientID, // {2}
                oneTimeFrequencyId, // {3}
                GlobalAttributesCache.Value( "CurrencySymbol" ) // {4}
                );
            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
        }

        /// <summary>
        /// Trigger an instance of each active workflow type selected in the block attributes
        /// </summary>
        private void TriggerWorkflows( FinancialScheduledTransaction schedule )
        {
            if ( schedule == null )
            {
                return;
            }

            var workflowTypeGuids = GetAttributeValues( AttributeKey.WorkflowType ).AsGuidList();

            if ( workflowTypeGuids.Any() )
            {
                // Make sure the workflow types are active and then trigger an instance of each
                var rockContext = new RockContext();
                var service = new WorkflowTypeService( rockContext );
                var workflowTypes = service.Queryable()
                    .AsNoTracking()
                    .Where( wt => wt.IsActive == true && workflowTypeGuids.Contains( wt.Guid ) )
                    .ToList();

                foreach ( var workflowType in workflowTypes )
                {
                    schedule.LaunchWorkflow( workflowType.Guid );
                }
            }
        }

        #endregion

        #endregion

        #region Helper Classes

        /// <summary>
        /// Lightweight object for each contribution item
        /// </summary>
        [Serializable]
        protected class AccountItem
        {
            public int Id { get; set; }

            public int Order { get; set; }

            public string Name { get; set; }

            public int? CampusId { get; set; }

            public decimal Amount { get; set; }

            public string PublicName { get; set; }

            public string AmountFormatted
            {
                get
                {
                    return Amount > 0 ? Amount.ToString( "F2" ) : string.Empty;
                }
            }

            public AccountItem( int id, int order, string name, int? campusId, string publicName )
            {
                Id = id;
                Order = order;
                Name = name;
                CampusId = campusId;
                PublicName = publicName;
            }
        }

        /// <summary>
        /// Saved Account View Model
        /// </summary>
        private class SavedAccountViewModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Reference Number
            /// </summary>
            public string ReferenceNumber { get; set; }

            /// <summary>
            /// Transaction Code
            /// </summary>
            public string TransactionCode { get; set; }

            /// <summary>
            /// Gateway Person Identifier
            /// </summary>
            public string GatewayPersonIdentifier { get; set; }

            /// <summary>
            /// Is this a card?
            /// </summary>
            public bool IsCard { get; set; }
        }

        #endregion
    }
}
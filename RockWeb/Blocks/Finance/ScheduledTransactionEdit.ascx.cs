// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [FinancialGatewayField( "ACH Card Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]
    [BooleanField( "Impersonation", "Allow (only use on an internal page used by staff)", "Don't Allow",
        "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", false, "", 2 )]
    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 3 )]
    [BooleanField( "Additional Accounts", "Display option for selecting additional accounts", "Don't display option",
        "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 4 )]
    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "", 5 )]

    // Text Options

    [TextField( "Panel Title", "The text to display in panel heading", false, "Scheduled Transaction", "Text Options", 6 )]

    [TextField( "Contribution Info Title", "The text to display as heading of section for selecting account and amount.", false, "Contribution Information", "Text Options", 7 )]
    [TextField( "Add Account Text", "The button text to display for adding an additional account", false, "Add Another Account", "Text Options", 8 )]

    [TextField( "Payment Info Title", "The text to display as heading of section for entering credit card or bank account information.", false, "Payment Information", "Text Options", 9 )]

    [TextField( "Confirmation Title", "The text to display as heading of section for confirming information entered.", false, "Confirm Information", "Text Options", 10 )]
    [CodeEditorField( "Confirmation Header", "The text (HTML) to display at the top of the confirmation section.", 
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the 'Finish' button to complete your transaction. 
</p>
", "Text Options", 11 )]
    [CodeEditorField( "Confirmation Footer", "The text (HTML) to display at the bottom of the confirmation section.", 
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<div class='alert alert-info'>
By clicking the 'finish' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may 
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions. 
</div>
", "Text Options", 12 )]

    [CodeEditorField( "Success Header", "The text (HTML) to display at the top of the success section.", 
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
Thank you for your generous contribution.  Your support is helping {{ OrganizationName }} actively 
achieve our mission.  We are so grateful for your commitment. 
</p>
", "Text Options", 13 )]
    [CodeEditorField( "Success Footer", "The text (HTML) to display at the bottom of the success section.", 
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
", "Text Options", 14 )]

    #endregion

    public partial class ScheduledTransactionEdit : RockBlock
    {
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
                    SetSavedAccounts();

                    dtpStartDate.SelectedDate = scheduledTransaction.NextPaymentDate;

                    hfCurrentPage.Value = "1";
                    RockPage page = Page as RockPage;
                    if ( page != null )
                    {
                        page.PageNavigate += page_PageNavigate;
                    }

                    FluidLayout = GetAttributeValue( "LayoutStyle" ) == "Fluid";

                    btnAddAccount.Title = GetAttributeValue( "AddAccountText" );

                    RegisterScript();

                    // Resolve the text field merge fields
                    var configValues = new Dictionary<string, object>();
                    Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                        .Where( v => v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) )
                        .ToList()
                        .ForEach( v => configValues.Add( v.Key, v.Value ) );
                    lConfirmationHeader.Text = GetAttributeValue( "ConfirmationHeader" ).ResolveMergeFields( configValues );
                    lConfirmationFooter.Text = GetAttributeValue( "ConfirmationFooter" ).ResolveMergeFields( configValues );
                    lSuccessHeader.Text = GetAttributeValue( "SuccessHeader" ).ResolveMergeFields( configValues );
                    lSuccessFooter.Text = GetAttributeValue( "SuccessFooter" ).ResolveMergeFields( configValues );

                    hfPaymentTab.Value = "None";

                    //// Temp values for testing...
                    /*
                    txtCreditCard.Text = "5105105105105100";
                    txtCVV.Text = "023";

                    txtBankName.Text = "Test Bank";
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

            if ( ScheduledTransactionId.HasValue )
            {
                if ( Gateway != null )
                {
                    // Save amounts from controls to the viewstate list
                    foreach ( RepeaterItem item in rptAccountList.Items )
                    {
                        var accountAmount = item.FindControl( "txtAccountAmount" ) as RockTextBox;
                        if ( accountAmount != null )
                        {
                            if ( SelectedAccounts.Count > item.ItemIndex )
                            {
                                decimal amount = decimal.MinValue;
                                if ( !decimal.TryParse( accountAmount.Text, out amount ) )
                                {
                                    amount = 0.0M;
                                }

                                SelectedAccounts[item.ItemIndex].Amount = amount;
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
                else
                {
                    SetPage( 0 );
                    ShowMessage( NotificationBoxType.Danger, "Transaction/Configuration Error", "This page is not configured to allow edits for the payment gateway associated with the selected transaction." );
                }
            }
            else
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Danger, "Invalid Transaction", "The transaction you've selected either does not exist or is not valid." );
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
            if (!string.IsNullOrWhiteSpace(personParam))
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
                bool allowImpersonation = false;
                if ( bool.TryParse( GetAttributeValue( "Impersonation" ), out allowImpersonation ) && allowImpersonation )
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
                        var service = new FinancialScheduledTransactionService( rockContext );
                        var scheduledTransaction = service
                            .Queryable( "AuthorizedPersonAlias.Person,ScheduledTransactionDetails,FinancialGateway,CurrencyTypeValue,CreditCardTypeValue" )
                            .Where( t =>
                                t.Id == txnId &&
                                ( t.AuthorizedPersonAlias.PersonId == targetPerson.Id || t.AuthorizedPersonAlias.Person.GivingGroupId == targetPerson.GivingGroupId ) )
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

            bool additionalAccounts = true;
            if ( !bool.TryParse( GetAttributeValue( "AdditionalAccounts" ), out additionalAccounts ) )
            {
                additionalAccounts = true;
            }

            SelectedAccounts = new List<AccountItem>();
            AvailableAccounts = new List<AccountItem>();

            // Enumerate through all active accounts that have a public name
            foreach ( var account in new FinancialAccountService( new RockContext() ).Queryable()
                .Where( f =>
                    f.IsActive &&
                    f.PublicName != null &&
                    f.PublicName.Trim() != string.Empty &&
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

            if ( Gateway != null )
            {
                if ( Gateway.TypeGuid.Equals( GetAttributeValue( "CCGateway" ).AsGuid() ) )
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
                        authorizedPerson.Id, DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id );
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

                if ( Gateway.TypeGuid.Equals( GetAttributeValue( "ACHGateway" ).AsGuid() ) )
                {
                    achEnabled = true;
                }
            }

            if ( ccEnabled || achEnabled )
            {
                if ( ccEnabled )
                {
                    divCCPaymentInfo.AddCssClass( "tab-pane" );
                    divCCPaymentInfo.Visible = ccEnabled;
                }

                if ( achEnabled )
                {
                    divACHPaymentInfo.AddCssClass( "tab-pane" );
                    divACHPaymentInfo.Visible = achEnabled;
                }

                if ( Gateway.SupportedPaymentSchedules.Any() )
                {
                    var oneTimeFrequency = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                    divRepeatingPayments.Visible = true;

                    btnFrequency.DataSource = Gateway.SupportedPaymentSchedules;
                    btnFrequency.DataBind();

                    btnFrequency.SelectedValue = scheduledTransaction.TransactionFrequencyValueId.ToString();
                }
            }
        }

        /// <summary>
        /// Binds the saved accounts.
        /// </summary>
        private void SetSavedAccounts()
        {
            rblSavedCC.Items.Clear();

            if ( TargetPersonId.HasValue && CurrentPerson != null && TargetPersonId == CurrentPerson.Id )
            {
                // Get the saved accounts for the target person
                var savedAccounts = new FinancialPersonSavedAccountService( new RockContext() )
                    .GetByPersonId( TargetPersonId.Value );

                if ( Gateway != null )
                {
                    var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );

                    rblSavedCC.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialGateway.EntityTypeId == Gateway.TypeId &&
                            a.CurrencyTypeValueId == ccCurrencyType.Id )
                        .OrderBy( a => a.Name )
                        .Select( a => new
                        {
                            Id = a.Id,
                            Name = "Use " + a.Name + " (" + a.MaskedAccountNumber + ")"
                        } ).ToList();
                    rblSavedCC.DataBind();
                    if ( rblSavedCC.Items.Count > 0 )
                    {
                        rblSavedCC.Items.Add( new ListItem( "Use a different card", "0" ) );
                    }

                    var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );

                    rblSavedAch.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialGateway.EntityTypeId == Gateway.TypeId &&
                            a.CurrencyTypeValueId == achCurrencyType.Id )
                        .OrderBy( a => a.Name )
                        .Select( a => new
                        {
                            Id = a.Id,
                            Name = "Use " + a.Name + " (" + a.MaskedAccountNumber + ")"
                        } ).ToList();
                    rblSavedAch.DataBind();
                    if ( rblSavedAch.Items.Count > 0 )
                    {
                        rblSavedAch.Items.Add( new ListItem( "Use a different bank account", "0" ) );
                    }
                }
            }

            if ( rblSavedCC.Items.Count > 0 )
            {
                rblSavedCC.Items[0].Selected = true;
                rblSavedCC.Visible = true;
                divNewCard.Style[HtmlTextWriterStyle.Display] = "none";
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

            string howOften = DefinedValueCache.Read( btnFrequency.SelectedValueAsId().Value ).Value;
            DateTime when = DateTime.MinValue;

            // Make sure a repeating payment starts in the future
            if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > RockDateTime.Today )
            {
                when = dtpStartDate.SelectedDate.Value;
            }
            else
            {
                errorMessages.Add( "Make sure the Next  Gift date is in the future (after today)" );
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
                    if ( string.IsNullOrWhiteSpace( txtBankName.Text ) )
                    {
                        errorMessages.Add( "Make sure to enter a bank name" );
                    }

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
                    .Queryable("AuthorizedPersonAlias.Person").FirstOrDefault( s => s.Id == ScheduledTransactionId.Value );
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

                tdWhen.Description = string.Format( "{0} starting on {1}", howOften, when.ToShortDateString() );
            }

            rptAccountListConfirmation.DataSource = SelectedAccounts.Where( a => a.Amount != 0 );
            rptAccountListConfirmation.DataBind();

            string nextDate = dtpStartDate.SelectedDate.HasValue ? dtpStartDate.SelectedDate.Value.ToShortDateString() : "?";
            string frequency = DefinedValueCache.Read( btnFrequency.SelectedValueAsInt() ?? 0 ).Description;
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
                        .Queryable("AuthorizedPersonAlias.Person,FinancialGateway")
                        .FirstOrDefault( s => s.Id == ScheduledTransactionId.Value );
                }

                if ( scheduledTransaction == null )
                {
                    errorMessage = "There was a problem getting the transaction information";
                    return false;
                }

                if ( scheduledTransaction.FinancialGateway != null )
                {
                    scheduledTransaction.FinancialGateway.LoadAttributes();
                }

                if ( scheduledTransaction.AuthorizedPersonAlias == null || scheduledTransaction.AuthorizedPersonAlias.Person == null)
                {
                    errorMessage = "There was a problem determining the person associated with the transaction";
                    return false;
                }

                var changeSummary = new StringBuilder();

                // Get the payment schedule
                scheduledTransaction.TransactionFrequencyValueId = btnFrequency.SelectedValueAsId().Value;
                changeSummary.Append( DefinedValueCache.Read( scheduledTransaction.TransactionFrequencyValueId, rockContext ) );

                if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > RockDateTime.Today )
                {
                    scheduledTransaction.StartDate = dtpStartDate.SelectedDate.Value;
                    changeSummary.AppendFormat( " starting {0}", scheduledTransaction.StartDate.ToShortDateString() );
                }
                else
                {
                    scheduledTransaction.StartDate = DateTime.MinValue;
                }

                changeSummary.AppendLine();

                PaymentInfo paymentInfo = GetPaymentInfo( personService, scheduledTransaction );
                if ( paymentInfo == null )
                {
                    errorMessage = "There was a problem creating the payment information";
                    return false;
                }
                else
                {
                }

                // If transaction is not active, attempt to re-activate it first
                if ( !scheduledTransaction.IsActive )
                {
                    if ( !transactionService.Reactivate( scheduledTransaction, out errorMessage ) )
                    {
                        return false;
                    }
                }

                if ( Gateway.UpdateScheduledPayment( scheduledTransaction, paymentInfo, out errorMessage ) )
                {
                    if ( paymentInfo.CurrencyTypeValue != null )
                    {
                        changeSummary.Append( paymentInfo.CurrencyTypeValue.Value );
                        scheduledTransaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;

                        DefinedValueCache creditCardTypeValue = paymentInfo.CreditCardTypeValue;
                        if ( creditCardTypeValue != null )
                        {
                            changeSummary.AppendFormat( " - {0}", creditCardTypeValue.Value );
                            scheduledTransaction.CreditCardTypeValueId = creditCardTypeValue.Id;
                        }
                        else
                        {
                            scheduledTransaction.CreditCardTypeValueId = null;
                        }
                        changeSummary.AppendFormat( " {0}", paymentInfo.MaskedNumber );
                        changeSummary.AppendLine();
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

                    foreach ( var account in SelectedAccounts )
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

                        changeSummary.AppendFormat( "{0}: {1:C2}", account.Name, account.Amount );
                        changeSummary.AppendLine();
                    }

                    rockContext.SaveChanges();

                    // Add a note about the change
                    var noteTypeService = new NoteTypeService( rockContext );
                    var noteType = noteTypeService.Get( scheduledTransaction.TypeId, "Note" );

                    var noteService = new NoteService( rockContext );
                    var note = new Note();
                    note.NoteTypeId = noteType.Id;
                    note.EntityId = scheduledTransaction.Id;
                    note.Caption = "Updated Transaction";
                    note.Text = changeSummary.ToString();
                    noteService.Add( note );

                    rockContext.SaveChanges();

                    ScheduleId = scheduledTransaction.GatewayScheduleId;
                    TransactionCode = scheduledTransaction.TransactionCode;

                    if (transactionService.GetStatus( scheduledTransaction, out errorMessage ))
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
                paymentInfo = new PaymentInfo();
            }

            if ( paymentInfo != null )
            {
                paymentInfo.Amount = SelectedAccounts.Sum( a => a.Amount );
                var authorizedPerson = scheduledTransaction.AuthorizedPersonAlias.Person;
                paymentInfo.FirstName = authorizedPerson.FirstName;
                paymentInfo.LastName = authorizedPerson.LastName;
                paymentInfo.Email = authorizedPerson.Email;

                bool displayPhone = false;
                if ( bool.TryParse( GetAttributeValue( "DisplayPhone" ), out displayPhone ) && displayPhone )
                {
                    var phoneNumber = personService.GetPhoneNumber( authorizedPerson, DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );
                    paymentInfo.Phone = phoneNumber != null ? phoneNumber.ToString() : string.Empty;
                }

                Guid addressTypeGuid = Guid.Empty;
                if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                {
                    addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                }

                var groupLocation = personService.GetFirstLocation( authorizedPerson.Id, DefinedValueCache.Read( addressTypeGuid ).Id );
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
            ach.BankName = txtBankName.Text;
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
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            rptAccountList.DataSource = SelectedAccounts.OrderBy( a => a.Order ).ToList();
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
        /// Registers the startup script.
        /// </summary>
        private void RegisterScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;

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
            $('.total-amount').html('$ ' + totalAmt.toFixed(2));
            return false;
        }});

        // Set the date prompt based on the frequency value entered
        $('#ButtonDropDown_btnFrequency .dropdown-menu a').click( function () {{
            var $when = $(this).parents('div.form-group:first').next(); 
            if ($(this).attr('data-id') == '{3}') {{
                $when.find('label:first').html('When');
            }} else {{
                $when.find('label:first').html('First Gift');

                // Set date to tomorrow if it is equal or less than today's date
                var $dateInput = $when.find('input');
                var dt = new Date(Date.parse($dateInput.val()));
                var curr = new Date();
                if ( (dt-curr) <= 0 ) {{ 
                    curr.setDate(curr.getDate() + 1);
                    var dd = curr.getDate();
                    var mm = curr.getMonth()+1;
                    var yy = curr.getFullYear();
                    $dateInput.val(mm+'/'+dd+'/'+yy);
                    $dateInput.data('datePicker').value(mm+'/'+dd+'/'+yy);
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
            var $content = $(this).parents('div.form-group:first').next('.radio-content')
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
        $('a[id$=""btnNext""]').click(function() {{
			$(this).addClass('disabled');
			$(this).unbind('click');
			$(this).click(function () {{
				return false;
			}});
        }});
 
    }});

";
            string script = string.Format( scriptFormat, divCCPaymentInfo.ClientID, divACHPaymentInfo.ClientID, hfPaymentTab.ClientID, oneTimeFrequencyId );
            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
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

        #endregion
    }
}
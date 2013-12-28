//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

using System.Collections.Generic;

namespace RockWeb.Blocks.Finance
{
    #region Block Attributes

    /// <summary>
    /// Edit an existing scheduled transaction.
    /// </summary>
    [DisplayName( "Giving Profile Detail" )]
    [Category( "Financial" )]
    [Description( "Edit an existing scheduled transaction." )]

    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "ACH Card Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]

    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]
    [BooleanField( "Additional Accounts", "Display option for selecting additional accounts", "Don't display option",
        "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 7 )]
    [TextField( "Add Account Text", "The button text to display for adding an additional account", false, "Add Another Account", "", 8 )]

    [BooleanField( "Impersonation", "Allow (only use on an internal page used by staff)", "Don't Allow",
        "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", false, "", 10 )]

    [CodeEditorField( "Confirmation Header", "The text (HTML) to display at the top of the confirmation section.", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the 'Finish' button to complete your transaction. 
</p>
", "Text Options", 13 )]

    [CodeEditorField( "Confirmation Footer", "The text (HTML) to display at the bottom of the confirmation section.", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
<div class='alert alert-info'>
By clicking the 'finish' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may 
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions. 
</div>
", "Text Options", 14 )]

    [CodeEditorField( "Success Header", "The text (HTML) to display at the top of the success section.", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
<p>
Thank you for your generous contribution.  Your support is helping {{ OrganizationName }} actively 
achieve our mission.  We are so grateful for your commitment. 
</p>
", "Text Options", 15 )]

    [CodeEditorField( "Success Footer", "The text (HTML) to display at the bottom of the success section.", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
", "Text Options", 16 )]

    #endregion

    public partial class GivingProfileDetail : Rock.Web.UI.RockBlock
    {

        #region Properties

        /// <summary>
        /// Gets or sets the gateway.
        /// </summary>
        protected GatewayComponent Gateway
        {
            get 
            { 
                if (_gateway == null && _gatewayGuid.HasValue)
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

            if (!Page.IsPostBack)
            {
                var scheduledTransaction = GetScheduledTransaction( true );

                if ( scheduledTransaction != null )
                {
                    Gateway = GetGateway(scheduledTransaction);

                    GetAccounts( scheduledTransaction );
                    SetFrequency( scheduledTransaction );
                    SetSavedAccounts( );

                    dtpStartDate.SelectedDate = scheduledTransaction.NextPaymentDate;

                    hfCurrentPage.Value = "1";
                    RockPage page = Page as RockPage;
                    if ( page != null )
                    {
                        page.PageNavigate += page_PageNavigate;
                    }

                    btnAddAccount.Title = GetAttributeValue( "AddAccountText" );

                    RegisterScript();

                    // Resolve the text field merge fields
                    var configValues = new Dictionary<string, object>();
                    Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                        .Where( v => v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) )
                        .ToList()
                        .ForEach( v => configValues.Add( v.Key, v.Value.Value ) );
                    phConfirmationHeader.Controls.Add( new LiteralControl( GetAttributeValue( "ConfirmationHeader" ).ResolveMergeFields( configValues ) ) );
                    phConfirmationFooter.Controls.Add( new LiteralControl( GetAttributeValue( "ConfirmationFooter" ).ResolveMergeFields( configValues ) ) );
                    phSuccessHeader.Controls.Add( new LiteralControl( GetAttributeValue( "SuccessHeader" ).ResolveMergeFields( configValues ) ) );
                    phSuccessFooter.Controls.Add( new LiteralControl( GetAttributeValue( "SuccessFooter" ).ResolveMergeFields( configValues ) ) );

                    hfPaymentTab.Value = "None";

                    // Temp values for testing...
                    //txtCreditCard.Text = "5105105105105100";
                    //txtCVV.Text = "023";

                    //txtBankName.Text = "Test Bank";
                    //txtRoutingNumber.Text = "111111118";
                    //txtAccountNumber.Text = "1111111111";
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
                                if ( decimal.TryParse( accountAmount.Text, out amount ) )
                                {
                                    SelectedAccounts[item.ItemIndex].Amount = amount;
                                }
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

            switch ( hfCurrentPage.Value.AsInteger() ?? 0 )
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

            switch ( hfCurrentPage.Value.AsInteger() ?? 0 )
            {
                case 2:
                    SetPage( 1 );
                    break;
            }
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
            int pageId = e.State["GivingDetail"].AsInteger() ?? 0;
            if ( pageId > 0 )
            {
                SetPage( pageId );
            }
        }

        #endregion

        #region  Methods

        #region Initialization

        private FinancialScheduledTransaction GetScheduledTransaction(bool refresh = false)
        {
            Person targetPerson = null;

            // If impersonation is allowed, and a valid person key was used, set the target to that person
            bool allowImpersonation = false;
            if ( bool.TryParse( GetAttributeValue( "Impersonation" ), out allowImpersonation ) && allowImpersonation )
            {
                string personKey = PageParameter( "Person" );
                if ( !string.IsNullOrWhiteSpace( personKey ) )
                {
                    targetPerson = new PersonService().GetByUrlEncodedKey( personKey );
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
                if ( int.TryParse( PageParameter( "Txn" ), out txnId ) )
                {
                    var service = new FinancialScheduledTransactionService();
                    var scheduledTransaction = service.Queryable( "ScheduledTransactionDetails,GatewayEntityType" )
                        .Where( t =>
                            t.Id == txnId &&
                            ( t.AuthorizedPersonId == targetPerson.Id || t.AuthorizedPerson.GivingGroupId == targetPerson.GivingGroupId ) )
                        .FirstOrDefault();

                    if (scheduledTransaction != null)
                    {
                        TargetPersonId = scheduledTransaction.AuthorizedPersonId;
                        ScheduledTransactionId = scheduledTransaction.Id;

                        if ( refresh )
                        {
                            string errorMessages = string.Empty;
                            service.UpdateStatus( scheduledTransaction, CurrentPersonId, out errorMessages );
                        }

                        return scheduledTransaction;
                    }
                }
            }

            return null;
        }

        private GatewayComponent GetGateway(FinancialScheduledTransaction scheduledTransaction)
        {
            if (scheduledTransaction.GatewayEntityType != null)
            {
                Guid gatewayGuid = scheduledTransaction.GatewayEntityType.Guid;
                var gateway = GatewayContainer.GetComponent(gatewayGuid.ToString());
                if (gateway != null && gateway.IsActive)        
                {
                    return gateway;
                }
            }

            return null;
        }

        private void GetAccounts(FinancialScheduledTransaction scheduledTransaction)
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
            foreach ( var account in new FinancialAccountService().Queryable()
                .Where( f =>
                    f.IsActive &&
                    f.PublicName != null &&
                    f.PublicName.Trim() != "" &&
                    ( f.StartDate == null || f.StartDate <= DateTime.Today ) &&
                    ( f.EndDate == null || f.EndDate >= DateTime.Today ) )
                .OrderBy( f => f.Order ) )
            {
                var accountItem = new AccountItem( account.Id, account.Order, account.Name, account.CampusId );
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

        private void SetFrequency(FinancialScheduledTransaction scheduledTransaction)
        {
            // Enable payment options based on the configured gateways
            bool ccEnabled = false;
            bool achEnabled = false;

            if (Gateway != null)
            {
                if (Gateway.TypeGuid.Equals(GetAttributeValue( "CCGateway" ).AsGuid()))
                {
                    ccEnabled = true;
                    txtCardFirstName.Visible = Gateway.SplitNameOnCard;
                    txtCardLastName.Visible = Gateway.SplitNameOnCard;
                    txtCardName.Visible = !Gateway.SplitNameOnCard;
                    mypExpiration.MinimumYear = DateTime.Now.Year;
                }

                if (Gateway.TypeGuid.Equals(GetAttributeValue( "ACHGateway" ).AsGuid()))
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
             
            if ( TargetPersonId.HasValue && TargetPersonId == CurrentPersonId )
            {
                // Get the saved accounts for the target person
                var savedAccounts = new FinancialPersonSavedAccountService()
                    .GetByPersonId( TargetPersonId.Value );

                if ( Gateway != null )
                {
                    var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );

                    rblSavedCC.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialTransaction.GatewayEntityTypeId == Gateway.TypeId &&
                            a.FinancialTransaction.CurrencyTypeValueId == ccCurrencyType.Id )
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
                            a.FinancialTransaction.GatewayEntityTypeId == Gateway.TypeId &&
                            a.FinancialTransaction.CurrencyTypeValueId == achCurrencyType.Id )
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

            // Make sure a repeating payment starts in the future
            if ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate <= DateTime.Today )
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
            else if ( hfPaymentTab.Value == "CC" )
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

                    var currentMonth = DateTime.Today;
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

            PaymentInfo paymentInfo = GetPaymentInfo();
            if ( paymentInfo != null )
            {
                tdName.Visible = true;
                tdPaymentMethod.Visible = true;
                tdAccountNumber.Visible = true;

                tdName.Description = paymentInfo.FullName;
                tdTotal.Description = paymentInfo.Amount.ToString( "C" );
                tdPaymentMethod.Description = paymentInfo.CurrencyTypeValue.Description;
                tdAccountNumber.Description = paymentInfo.MaskedNumber;
            }
            else
            {
                tdName.Visible = false;
                tdPaymentMethod.Visible = false;
                tdAccountNumber.Visible = false;

                tdTotal.Description = SelectedAccounts.Sum( a => a.Amount ).ToString( "C" );
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
            errorMessage = string.Empty;

            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                if ( Gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                using ( new UnitOfWorkScope() )
                {
                    var personService = new PersonService();
                    var transactionService = new FinancialScheduledTransactionService();
                    var transactionDetailService = new FinancialScheduledTransactionDetailService();

                    FinancialScheduledTransaction scheduledTransaction = null;

                    if ( ScheduledTransactionId.HasValue )
                    {
                        scheduledTransaction = transactionService.Get( ScheduledTransactionId.Value );
                    }

                    if ( scheduledTransaction == null )
                    {
                        errorMessage = "There was a problem getting the transaction information";
                        return false;
                    }

                    if ( scheduledTransaction.AuthorizedPerson == null )
                    {
                        errorMessage = "There was a problem determining the person associated with the transaction";
                        return false;
                    }

                    // Get the payment schedule
                    scheduledTransaction.TransactionFrequencyValueId = btnFrequency.SelectedValueAsId().Value;
                    if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > DateTime.Today )
                    {
                        scheduledTransaction.StartDate = dtpStartDate.SelectedDate.Value;
                    }
                    else
                    {
                        scheduledTransaction.StartDate = DateTime.MinValue;
                    }

                    PaymentInfo paymentInfo = GetPaymentInfo();
                    if ( paymentInfo == null )
                    {
                        errorMessage = "There was a problem creating the payment information";
                        return false;
                    }
                    else
                    {
                        paymentInfo.FirstName = scheduledTransaction.AuthorizedPerson.FirstName;
                        paymentInfo.LastName = scheduledTransaction.AuthorizedPerson.LastName;
                        paymentInfo.Email = scheduledTransaction.AuthorizedPerson.Email;

                        bool displayPhone = false;
                        if ( bool.TryParse( GetAttributeValue( "DisplayPhone" ), out displayPhone ) && displayPhone )
                        {
                            var phoneNumber = personService.GetPhoneNumber( scheduledTransaction.AuthorizedPerson, DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );
                            paymentInfo.Phone = phoneNumber != null ? phoneNumber.NumberFormatted : string.Empty;
                        }

                        Guid addressTypeGuid = Guid.Empty;
                        if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                        {
                            addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                        }

                        var address = personService.GetFirstLocation( scheduledTransaction.AuthorizedPerson, DefinedValueCache.Read( addressTypeGuid ).Id );
                        if ( address != null )
                        {
                            paymentInfo.Street = address.Street1;
                            paymentInfo.City = address.City;
                            paymentInfo.State = address.State;
                            paymentInfo.Zip = address.Zip;
                        }
                    }

                    if ( Gateway.UpdateScheduledPayment( scheduledTransaction, paymentInfo, out errorMessage ) )
                    {
                        var selectedAccountIds = SelectedAccounts
                            .Where( a => a.Amount > 0 )
                            .Select( a => a.Id ).ToList();


                        var deletedAccounts = scheduledTransaction.ScheduledTransactionDetails
                            .Where( a => !selectedAccountIds.Contains( a.AccountId ) ).ToList();
                        foreach ( var deletedAccount in deletedAccounts )
                        {
                            scheduledTransaction.ScheduledTransactionDetails.Remove( deletedAccount );
                            transactionDetailService.Delete( deletedAccount, CurrentPersonId );
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
                        }

                        transactionService.Save( scheduledTransaction, CurrentPersonId );

                        ScheduleId = scheduledTransaction.GatewayScheduleId;
                        TransactionCode = scheduledTransaction.TransactionCode;
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
        private PaymentInfo GetPaymentInfo()
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
            else if ( hfPaymentTab.Value == "CC" )
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

            if ( paymentInfo != null )
            {
                paymentInfo.Amount = SelectedAccounts.Sum( a => a.Amount );
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
            cc.BillingStreet = txtBillingStreet.Text;
            cc.BillingCity = txtBillingCity.Text;
            cc.BillingState = ddlBillingState.SelectedValue;
            cc.BillingZip = txtBillingZip.Text;

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
            using ( new UnitOfWorkScope() )
            {
                var savedAccount = new FinancialPersonSavedAccountService().Get( savedAccountId );
                if ( savedAccount != null )
                {
                    var reference = new ReferencePaymentInfo();
                    reference.TransactionCode = savedAccount.FinancialTransaction.TransactionCode;
                    reference.ReferenceNumber = savedAccount.ReferenceNumber;
                    reference.MaskedAccountNumber = savedAccount.MaskedAccountNumber;
                    reference.InitialCurrencyTypeValue = DefinedValueCache.Read( savedAccount.FinancialTransaction.CurrencyTypeValue );
                    if ( reference.InitialCurrencyTypeValue.Guid.Equals( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ) )
                    {
                        reference.InitialCreditCardTypeValue = DefinedValueCache.Read( savedAccount.FinancialTransaction.CreditCardTypeValue );
                    }
                    return reference;
                }
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
            // Page 1 = Payment Info
            // Page 2 = Confirmation
            // Page 3 = Success
            // Page 0 = Only message box is displayed

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

            string script = string.Format( @"
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
            if ($(this).attr('data-id') == '{2}') {{
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
                $('#{1}').val('CreditCard');
            }} else {{
                $('#{1}').val('ACH');
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

", divCCPaymentInfo.ClientID, hfPaymentTab.ClientID, oneTimeFrequencyId );
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

            public string AmountFormatted
            {
                get
                {
                    return Amount > 0 ? Amount.ToString( "F2" ) : string.Empty;
                }

            }

            public AccountItem( int id, int order, string name, int? campusId )
            {
                Id = id;
                Order = order;
                Name = name;
                CampusId = campusId;
            }
        }

        #endregion

    }

}

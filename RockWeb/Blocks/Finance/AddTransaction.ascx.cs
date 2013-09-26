//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
    /// Add a new one-time or scheduled transaction
    /// </summary>
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "ACH Card Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving - ", "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, "", "", 3)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME, "", 4 )]

    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "", 5 )]

    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]
    [BooleanField( "Additional Accounts", "Display option for selecting additional accounts", "Don't display option", 
        "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 7 )]
    [TextField( "Add Account Text", "The button text to display for adding an additional account", false, "Add Another Account", "", 8 )]

    [BooleanField( "Scheduled Transactions", "Allow", "Don't Allow", 
        "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 9, "AllowScheduled" )]

    [BooleanField( "Impersonation", "Allow (only use on an internal page used by staff)", "Don't Allow", 
        "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", false, "", 10)]
    [BooleanField( "Prompt for Phone", "Should the user be prompted for their phone number?", false, "", 11, "DisplayPhone" )]
    [BooleanField( "Prompt for Email", "Should the user be prompted for their email address?", true, "", 12, "DisplayEmail" )]

    [MemoField( "Confirmation Header", "The text (HTML) to display at the top of the confirmation section.", true, @"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the 'Finish' button to complete your transaction. 
</p>
", "Text Options", 13 )]

    [MemoField( "Confirmation Footer", "The text (HTML) to display at the bottom of the confirmation section.", true, @"
<div class='alert alert-info'>
By clicking the 'finish' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may 
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions. 
</div>
", "Text Options", 14 )]

    [MemoField( "Success Header", "The text (HTML) to display at the top of the success section.", true, @"
<p>
Thank-you for your generous contribution.  Your support is helping {{ OrganizationName }} actively 
achieve our mission.  We are so grateful for your commitment. 
</p>
", "Text Options", 15 )]

    [MemoField( "Success Footer", "The text (HTML) to display at the bottom of the success section.", true, @"
", "Text Options", 16 )]

    #endregion

    public partial class AddTransaction : Rock.Web.UI.RockBlock
    {

        #region Fields

        protected bool FluidLayout = false;
        private bool _showRepeatingOptions = false;
        private GatewayComponent _ccGateway;
        private GatewayComponent _achGateway;

        #endregion 

        #region Properties

        /// <summary>
        /// Gets or sets the target person.
        /// </summary>
        protected Person TargetPerson { get; private set; }

        /// <summary>
        /// Gets or sets the accounts that are available for user to add to the list.
        /// </summary>
        protected List<AccountItem> AvailableAccounts
        {
            get
            {
                var accounts = ViewState["AvailableAccounts"] as List<AccountItem>;
                if ( accounts == null )
                {
                    accounts = new List<AccountItem>();
                }
                return accounts;
            }
            set
            {
                ViewState["AvailableAccounts"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the accounts that are currently displayed to the user
        /// </summary>
        protected List<AccountItem> SelectedAccounts
        {
            get
            {
                var accounts = ViewState["SelectedAccounts"] as List<AccountItem>;
                if ( accounts == null )
                {
                    accounts = new List<AccountItem>();
                }
                return accounts;
            }
            set
            {
                ViewState["SelectedAccounts"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment transaction code.
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        /// <summary>
        /// Gets or sets the payment schedule id.
        /// </summary>
        protected string ScheduleId
        {
            get { return ViewState["ScheduleId"] as string ?? string.Empty; }
            set { ViewState["ScheduleId"] = value; }
        }


        #endregion

        #region overridden control methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // If impersonation is allowed, and a valid person key was used, set the target to that person
            bool allowInpersonation = false;
            if ( bool.TryParse( GetAttributeValue( "Impersonation" ), out allowInpersonation ) && allowInpersonation )
            {
                string personKey = PageParameter( "Person" );
                if ( !string.IsNullOrWhiteSpace( personKey ) )
                {
                    TargetPerson = new PersonService().GetByUrlEncodedKey( personKey );
                }
            }
            if (TargetPerson == null)
            {
                TargetPerson = CurrentPerson;
            }

            // Enable payment options based on the configured gateways
            bool ccEnabled = false;
            bool achEnabled = false;
            var supportedFrequencies = new List<DefinedValueCache>();

            string ccGatewayGuid = GetAttributeValue( "CCGateway" );
            if ( !string.IsNullOrWhiteSpace( ccGatewayGuid ) )
            {
                _ccGateway = GatewayContainer.GetComponent( ccGatewayGuid );
                if ( _ccGateway != null )
                {
                    ccEnabled = true;
                    txtCardFirstName.Visible = _ccGateway.SplitNameOnCard;
                    txtCardLastName.Visible = _ccGateway.SplitNameOnCard;
                    txtCardName.Visible = !_ccGateway.SplitNameOnCard;
                }
            }

            string achGatewayGuid = GetAttributeValue( "ACHGateway" );
            if ( !string.IsNullOrWhiteSpace( achGatewayGuid ) )
            {
                _achGateway = GatewayContainer.GetComponent( achGatewayGuid );
                achEnabled = _achGateway != null;
            }

            hfCurrentPage.Value = "1";
            RockPage page = Page as RockPage;
            if ( page != null )
            {
                page.PageNavigate += page_PageNavigate;
            }

            if ( ccEnabled || achEnabled )
            {
                if ( ccEnabled )
                {
                    supportedFrequencies = _ccGateway.SupportedPaymentSchedules;
                    hfPaymentTab.Value = "CreditCard";
                }
                else
                {
                    supportedFrequencies = _achGateway.SupportedPaymentSchedules;
                    hfPaymentTab.Value = "ACH";
                }

                if ( ccEnabled && achEnabled )
                {
                    phPills.Visible = true;

                    // If CC and ACH gateways are different, only allow frequencies supported by both payment gateways (if different)
                    if ( _ccGateway.TypeId != _achGateway.TypeId )
                    {
                        supportedFrequencies = _ccGateway.SupportedPaymentSchedules
                            .Where( c =>
                                _achGateway.SupportedPaymentSchedules
                                    .Select( a => a.Id )
                                    .Contains( c.Id ) )
                            .ToList();
                    }
                    divCCPaymentInfo.AddCssClass( "tab-pane" );
                    divACHPaymentInfo.AddCssClass( "tab-pane" );
                }

                divCCPaymentInfo.Visible = ccEnabled;
                divACHPaymentInfo.Visible = achEnabled;

                if ( supportedFrequencies.Any() )
                {
                    bool allowScheduled = false;
                    if ( bool.TryParse( GetAttributeValue( "AllowScheduled" ), out allowScheduled ) && allowScheduled )
                    {
                        _showRepeatingOptions = true;
                        var oneTimeFrequency = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                        divRepeatingPayments.Visible = true;

                        btnFrequency.DataSource = supportedFrequencies;
                        btnFrequency.DataBind();

                        // If gateway didn't specifically support one-time, add it anyway for immediate gifts
                        if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
                        {
                            btnFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Name, oneTimeFrequency.Id.ToString() ) );
                        }
                        btnFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
                        dtpStartDate.SelectedDate = DateTime.Today;
                    };

                }

                // Display Options
                btnAddAccount.Title = GetAttributeValue( "AddAccountText" );

                bool display = false;
                
                bool.TryParse( GetAttributeValue( "DisplayEmail" ), out display );
                txtEmail.Visible = display;
                tdEmail.Visible = display;

                bool.TryParse( GetAttributeValue( "DisplayPhone" ), out display );
                txtPhone.Visible = display;
                tdPhone.Visible = display;

                FluidLayout = GetAttributeValue( "LayoutStyle" ) == "Fluid";

                BindSavedAccounts();

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

                // Temp values for testing...
                //txtCreditCard.Text = "5105105105105100";
                //txtCVV.Text = "023";

                //txtBankName.Text = "Test Bank";
                //txtRoutingNumber.Text = "111111118";
                //txtAccountNumber.Text = "1111111111";
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
            nbSaveAccount.Visible = false;

            if ( _ccGateway != null || _achGateway != null )
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
                lblTotalAmount.Text = SelectedAccounts.Sum( f => f.Amount).ToString("F2");

                // Set the frequency date label based on if 'One Time' is selected or not
                if ( btnFrequency.Items.Count > 0 )
                {
                    dtpStartDate.Label = btnFrequency.Items[0].Selected ? "When" : "First Gift";
                }

                // If there are both CC and ACH options, set the active tab based on the hidden field value that tracks the active tag
                if ( phPills.Visible )
                {
                    if ( hfPaymentTab.Value == "ACH" )
                    {
                        liCreditCard.RemoveCssClass( "active" );
                        liACH.AddCssClass( "active" );
                        divCCPaymentInfo.RemoveCssClass( "active" );
                        divACHPaymentInfo.AddCssClass( "active" );
                    }
                    else
                    {
                        liCreditCard.AddCssClass( "active" );
                        liACH.RemoveCssClass( "active" );
                        divCCPaymentInfo.AddCssClass( "active" );
                        divACHPaymentInfo.RemoveCssClass( "active" );
                    }
                }

                // Show or Hide the Credit card entry panel based on if a saved account exists and it's selected or not.
                divNewCard.Style[HtmlTextWriterStyle.Display] = ( rblSavedCC.Items.Count == 0 || rblSavedCC.Items[rblSavedCC.Items.Count - 1].Selected ) ? "block" : "none";

                // Show billing address based on if billing address checkbox is checked
                divBillingAddress.Style[HtmlTextWriterStyle.Display] = cbBillingAddress.Checked ? "block" : "none";

                // Show save account info based on if checkbox is checked
                divSaveAccount.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";

                if ( !Page.IsPostBack )
                {
                    SetPage( 1 );

                    // Get the list of accounts that can be used
                    GetAccounts();
                    BindAccounts();

                    // Set personal information if there is a currently logged in person
                    var person = GetPerson( false );
                    if ( person != null )
                    {
                        // If there is a currently logged in user, do not allow them to change their name.
                        txtCurrentName.Visible = true;
                        txtCurrentName.Text = person.FullName;
                        txtFirstName.Visible = false;
                        txtLastName.Visible = false;
                        txtEmail.Text = person.Email;

                        using ( new UnitOfWorkScope() )
                        {
                            var personService = new PersonService();

                            bool displayPhone = false;
                            if ( bool.TryParse( GetAttributeValue( "DisplayPhone" ), out displayPhone ) && displayPhone )
                            {
                                var phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );
                                txtPhone.Text = phoneNumber != null ? phoneNumber.NumberFormatted : string.Empty;
                            }

                            Guid addressTypeGuid = Guid.Empty;
                            if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                            {
                                addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME );
                            }

                            var address = personService.GetFirstLocation( person, DefinedValueCache.Read( addressTypeGuid ).Id );
                            if ( address != null )
                            {
                                txtStreet.Text = address.Street1;
                                txtCity.Text = address.City;
                                ddlState.SelectedValue = address.State;
                                txtZip.Text = address.Zip;
                            }
                        }
                    }
                    else
                    {
                        txtCurrentName.Visible = false;
                        txtFirstName.Visible = true;
                        txtLastName.Visible = true;
                    }

                    SetPage( 1 );
                }
            }
            else
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Error, "Configuration Error", "Please check the configuration of this block and make sure a valid Credit Card and/or ACH Finacial Gateway has been selected." );
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
            AvailableAccounts = AvailableAccounts.Except(selected).ToList();
            SelectedAccounts.AddRange(selected);

            BindAccounts();
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

                    if ( ProccessPaymentInfo( out errorMessage ) )
                    {
                        this.AddHistory( "GivingDetail", "1", null );
                        SetPage( 2 );
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Error, "Oops!", errorMessage );
                    }

                    break;

                case 2:

                    if ( ProccessConfirmation( out errorMessage ) )
                    {
                        this.AddHistory( "GivingDetail", "2", null );
                        SetPage( 3 );
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Error, "Payment Error", errorMessage );
                    }

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
            if ( ProccessConfirmation( out errorMessage ) )
            {
                SetPage( 3 );
            }
            else
            {
                ShowMessage( NotificationBoxType.Error, "Payment Error", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace(TransactionCode))
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            if ( phCreateLogin.Visible )
            {
                if ( string.IsNullOrWhiteSpace( txtUserName.Text ) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    nbSaveAccount.Title = "Missing Informaton";
                    nbSaveAccount.Text = "A username and password are required when saving an account";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Error;
                    nbSaveAccount.Visible = true;
                    return;
                }

                if ( new UserLoginService().GetByUserName( txtUserName.Text ) != null )
                {
                    nbSaveAccount.Title = "Invalid Username";
                    nbSaveAccount.Text = "The selected Username is already being used.  Please select a different Username";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Error;
                    nbSaveAccount.Visible = true;
                    return;
                }

                if ( txtPasswordConfirm.Text != txtPassword.Text )
                {
                    nbSaveAccount.Title = "Invalid Password";
                    nbSaveAccount.Text = "The password and password confirmation do not match";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Error;
                    nbSaveAccount.Visible = true;
                    return;
                }
            }

            if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
            {
                using ( new UnitOfWorkScope() )
                {
                    var transaction = new FinancialTransactionService().GetByTransactionCode( TransactionCode );
                    if ( transaction != null && transaction.AuthorizedPersonId.HasValue )
                    {
                        if ( phCreateLogin.Visible )
                        {
                            var userLoginService = new Rock.Model.UserLoginService();
                            var user = userLoginService.Create( transaction.AuthorizedPerson, Rock.Model.AuthenticationServiceType.Internal, "Rock.Security.Authentication.Database", txtUserName.Text, txtPassword.Text, false, CurrentPersonId );

                            var mergeObjects = new Dictionary<string, object>();
                            mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                            var personDictionary = transaction.AuthorizedPerson.ToDictionary();
                            mergeObjects.Add( "Person", personDictionary );

                            mergeObjects.Add( "User", user.ToDictionary() );

                            var recipients = new Dictionary<string, Dictionary<string, object>>();
                            recipients.Add( transaction.AuthorizedPerson.Email, mergeObjects );

                            Rock.Communication.Email email = new Rock.Communication.Email( Rock.SystemGuid.EmailTemplate.SECURITY_CONFIRM_ACCOUNT );
                            email.Send( recipients );
                        }

                        var paymentInfo = GetPaymentInfo();

                        var savedAccount = new FinancialPersonSavedAccount();
                        savedAccount.PersonId = transaction.AuthorizedPersonId.Value;
                        savedAccount.FinancialTransactionId = transaction.Id;
                        savedAccount.Name = txtSaveAccount.Text;
                        savedAccount.MaskedAccountNumber = paymentInfo.AccountNumber;

                        var savedAccountService = new FinancialPersonSavedAccountService();
                        savedAccountService.Add( savedAccount, CurrentPersonId );
                        savedAccountService.Save( savedAccount, CurrentPersonId );

                        cbSaveAccount.Visible = false;
                        txtSaveAccount.Visible = false;
                        phCreateLogin.Visible = false;
                        divSaveActions.Visible = false;

                        nbSaveAccount.Title = "Success";
                        nbSaveAccount.Text = "The account has been saved for future use";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
                        nbSaveAccount.Visible = true;

                    }
                    else
                    {
                        nbSaveAccount.Title = "Invalid Transaction";
                        nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Error;
                        nbSaveAccount.Visible = true;
                    }
                }
            }
            else
            {
                nbSaveAccount.Title = "Missing Account Name";
                nbSaveAccount.Text = "Please enter a name to use for this account";
                nbSaveAccount.NotificationBoxType = NotificationBoxType.Error;
                nbSaveAccount.Visible = true;
            }
        }

        #endregion

        #region Private Methods

        #region Methods for the Payment Info Page (panel)

        private void GetAccounts()
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
                .OrderBy( f => f.PublicName ) )
            {
                var accountItem = new AccountItem( account.Id, account.Name, account.CampusId );
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
        }

        private void BindAccounts()
        {
            rptAccountList.DataSource = SelectedAccounts;
            rptAccountList.DataBind();

            btnAddAccount.Visible = AvailableAccounts.Any();
            btnAddAccount.DataSource = AvailableAccounts;
            btnAddAccount.DataBind();
        }

        private Person GetPerson( bool create )
        {
            Person person = null;

            int personId = ViewState["PersonId"] as int? ?? 0;
            if ( personId == 0 && TargetPerson != null )
            {
                person = TargetPerson;
            }
            else
            {
                using ( new UnitOfWorkScope() )
                {
                    var personService = new PersonService();

                    if ( personId != 0 )
                    {
                        person = personService.Get( personId );
                    }

                    if ( person == null && create )
                    {
                        // Check to see if there's only one person with same email, first name, and last name
                        if ( !string.IsNullOrWhiteSpace( txtEmail.Text ) &&
                            !string.IsNullOrWhiteSpace( txtFirstName.Text ) &&
                            !string.IsNullOrWhiteSpace( txtLastName.Text ) )
                        {
                            var personMatches = personService.GetByEmail( txtEmail.Text ).Where( p =>
                                p.LastName.Equals( txtLastName.Text, StringComparison.OrdinalIgnoreCase ) &&
                                ( p.FirstName.Equals( txtFirstName.Text, StringComparison.OrdinalIgnoreCase ) ||
                                p.NickName.Equals( txtFirstName.Text, StringComparison.OrdinalIgnoreCase ) ) );
                            if ( personMatches.Count() == 1 )
                            {
                                person = personMatches.FirstOrDefault();
                            }
                        }

                        if ( person == null )
                        {
                            // Create Person
                            person = new Person();
                            person.GivenName = txtFirstName.Text;
                            person.LastName = txtLastName.Text;
                            person.Email = txtEmail.Text;

                            bool displayPhone = false;
                            if ( bool.TryParse( GetAttributeValue( "DisplayPhone" ), out displayPhone ) && displayPhone )
                            {
                                var phone = new PhoneNumber();
                                phone.Number = txtPhone.Text.AsNumeric();
                                phone.NumberTypeValueId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
                                person.PhoneNumbers.Add( phone );
                            }

                            // Create Family Role
                            var groupMember = new GroupMember();
                            groupMember.Person = person;
                            groupMember.GroupRole = new GroupRoleService().Get(new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) );

                            // Create Family
                            var group = new Group();
                            group.Members.Add( groupMember );
                            group.Name = person.LastName + " Family";
                            group.GroupType = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) );

                            var groupLocation = new GroupLocation();
                            var location = new LocationService().Get(
                                txtStreet.Text, string.Empty, txtCity.Text, ddlState.SelectedValue, txtZip.Text );
                            if ( location != null )
                            {
                                Guid addressTypeGuid = Guid.Empty;
                                if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                                {
                                    addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME );
                                }

                                groupLocation = new GroupLocation();
                                groupLocation.Location = location;
                                groupLocation.GroupLocationTypeValueId = DefinedValueCache.Read( addressTypeGuid ).Id;

                                group.GroupLocations.Add( groupLocation );
                            }

                            var groupService = new GroupService();
                            groupService.Add( group, CurrentPersonId );
                            groupService.Save( group, CurrentPersonId );
                        }

                        ViewState["PersonId"] = person != null ? person.Id : 0;

                    }
                }
            }

            return person;
        }

        /// <summary>
        /// Binds the saved accounts.
        /// </summary>
        private void BindSavedAccounts()
        {
            rblSavedCC.Items.Clear();

            if ( TargetPerson != null  )
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService()
                    .GetByPersonId( TargetPerson.Id );

                if ( _ccGateway != null )
                {
                    var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );

                    rblSavedCC.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialTransaction.GatewayEntityTypeId == _ccGateway.TypeId &&
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
                }

                if ( _achGateway != null )
                {
                    var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );

                    rblSavedAch.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialTransaction.GatewayEntityTypeId == _achGateway.TypeId &&
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
        }

        private bool ProccessPaymentInfo( out string errorMessage )
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

            // Get the payment schedule
            PaymentSchedule schedule = GetSchedule();

            if ( schedule != null )
            {
                // Make sure a repeating payment starts in the future
                if ( schedule.StartDate <= DateTime.Today )
                {
                    errorMessages.Add( "When scheduling a repeating payment, make sure the First Gift date is in the future (after today)" );
                }
            }

            if ( txtFirstName.Visible == true )
            {
                if ( string.IsNullOrWhiteSpace( txtFirstName.Text ) || string.IsNullOrWhiteSpace( txtLastName.Text ) )
                {
                    errorMessages.Add( "Make sure to enter both a first and last name" );
                }
            }

            if ( string.IsNullOrWhiteSpace( txtEmail.Text ) )
            {
                errorMessages.Add( "Make sure to enter a valid email address.  An email address is required for us to send you a payment confirmation" );
            }

            if ( hfPaymentTab.Value == "ACH" )
            {
                // Validate ach options
            }
            else
            {
                // validate cc options
                if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsInt() ?? 0 ) > 0 )
                {
                    // TODO: Find saved card
                }
                else
                {
                    if ( _ccGateway.SplitNameOnCard )
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
                    currentMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                    if ( !mypExpiration.SelectedDate.HasValue || mypExpiration.SelectedDate.Value.CompareTo(currentMonth) < 0)
                    {
                        errorMessages.Add( "Make sure to enter a valid credit card expiration date" );
                    }

                    if ( string.IsNullOrWhiteSpace(txtCVV.Text) )
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

            tdName.Description = paymentInfo.FullName;
            tdPhone.Description = paymentInfo.Phone;
            tdEmail.Description = paymentInfo.Email;
            tdAddress.Description = string.Format( "{0} {1}, {2} {3}",
                paymentInfo.Street, paymentInfo.City, paymentInfo.State, paymentInfo.Zip );

            rptAccountListConfirmation.DataSource = SelectedAccounts.Where( a => a.Amount != 0);
            rptAccountListConfirmation.DataBind();

            tdTotal.Description = paymentInfo.Amount.ToString( "C" );

            tdPaymentMethod.Description = paymentInfo.CurrencyTypeValue.Description;
            tdAccountNumber.Description = paymentInfo.AccountNumber;
            tdWhen.Description = schedule != null ? schedule.ToString() : "Today";

            return true;
        }

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
            else
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

            paymentInfo.Amount = SelectedAccounts.Sum( a => a.Amount );
            paymentInfo.Email = txtEmail.Text;
            paymentInfo.Phone = txtPhone.Text;
            paymentInfo.Street = txtStreet.Text;
            paymentInfo.City = txtCity.Text;
            paymentInfo.State = ddlState.SelectedValue;
            paymentInfo.Zip = txtZip.Text;

            return paymentInfo;
        }

        private CreditCardPaymentInfo GetCCInfo()
        {
            var cc = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
            cc.NameOnCard = _ccGateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            cc.LastNameOnCard = txtCardLastName.Text;

            if ( cbBillingAddress.Checked )
            {
                cc.BillingStreet = txtBillingStreet.Text;
                cc.BillingCity = txtBillingCity.Text;
                cc.BillingState = ddlBillingState.SelectedValue;
                cc.BillingZip = txtBillingZip.Text;
            }
            else
            {
                cc.BillingStreet = txtStreet.Text;
                cc.BillingCity = txtCity.Text;
                cc.BillingState = ddlState.SelectedValue;
                cc.BillingZip = txtZip.Text;
            }
            return cc;
        }

        private ACHPaymentInfo GetACHInfo()
        {
            var ach = new ACHPaymentInfo( txtAccountNumber.Text, txtRoutingNumber.Text, rblAccountType.SelectedValue == "Savings" ? BankAccountType.Savings : BankAccountType.Checking );
            ach.BankName = txtBankName.Text;
            return ach;
        }

        private ReferencePaymentInfo GetReferenceInfo( int savedAccountId )
        {
            using (new UnitOfWorkScope() )
            {
                var savedAccount = new FinancialPersonSavedAccountService().Get( savedAccountId );
                if ( savedAccount != null )
                {
                    var reference = new ReferencePaymentInfo( savedAccount.FinancialTransaction.TransactionCode );
                    reference.MaskedAccountNumber = savedAccount.MaskedAccountNumber;
                    reference.InitialCurrencyTypeValue = DefinedValueCache.Read( savedAccount.FinancialTransaction.CurrencyTypeValue );
                    reference.InitialCreditCardTypeValue = DefinedValueCache.Read( savedAccount.FinancialTransaction.CreditCardTypeValue );
                    return reference;
                }
            }

            return null;
        }

        private PaymentSchedule GetSchedule()
        {
            // Figure out if this is a one-time transaction or a future scheduled transaction
            bool repeating = _showRepeatingOptions;
            if ( repeating )
            {
                // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
                int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() && dtpStartDate.SelectedDate == DateTime.Today )
                {
                    // one-time immediate payment
                    return null;
                }

                var schedule = new PaymentSchedule();
                schedule.TransactionFrequencyValue = DefinedValueCache.Read( btnFrequency.SelectedValueAsId().Value );
                if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > DateTime.Today )
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

        private bool ProccessConfirmation( out string errorMessage )
        {
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                GatewayComponent gateway = hfPaymentTab.Value == "ACH" ? _achGateway : _ccGateway;
                if ( gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                Person person = GetPerson( true );
                if ( person == null )
                {
                    errorMessage = "There was a problem creating the person information";
                    return false;
                }

                PaymentInfo paymentInfo = GetPaymentInfo();
                if ( paymentInfo == null )
                {
                    errorMessage = "There was a problem creating the payment information";
                    return false;
                }
                else
                {
                    paymentInfo.FirstName = person.FirstName;
                    paymentInfo.LastName = person.LastName;
                }

                PaymentSchedule schedule = GetSchedule();
                if ( schedule != null )
                {
                    schedule.PersonId = person.Id;

                    var scheduledTransaction = gateway.AddScheduledPayment( schedule, paymentInfo, out errorMessage );
                    if ( scheduledTransaction != null )
                    {
                        scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
                        scheduledTransaction.AuthorizedPersonId = person.Id;

                        foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
                        {
                            var transactionDetail = new FinancialScheduledTransactionDetail();
                            transactionDetail.Amount = account.Amount;
                            transactionDetail.AccountId = account.Id;
                            scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
                        }

                        var transactionService = new FinancialScheduledTransactionService();
                        transactionService.Add( scheduledTransaction, CurrentPersonId );
                        transactionService.Save( scheduledTransaction, CurrentPersonId );

                        ScheduleId = scheduledTransaction.GatewayScheduleId;
                        TransactionCode = scheduledTransaction.TransactionCode;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var transaction = gateway.Charge( paymentInfo, out errorMessage );
                    if ( transaction != null )
                    {
                        transaction.TransactionDateTime = DateTime.Now;
                        transaction.AuthorizedPersonId = person.Id;
                        transaction.GatewayEntityTypeId = gateway.TypeId;
                        transaction.Amount = paymentInfo.Amount;
                        transaction.TransactionTypeValueId = DefinedValueCache.Read(new Guid(Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION)).Id;
                        transaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;

                        if (paymentInfo.CreditCardTypeValue != null)
                        {
                            transaction.CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
                        }

                        Guid sourceGuid = Guid.Empty;
                        if (Guid.TryParse(GetAttributeValue("Source"), out sourceGuid))
                        {
                            transaction.SourceTypeValueId = DefinedValueCache.Read(sourceGuid).Id;
                        }
                        foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
                        {
                            var transactionDetail = new FinancialTransactionDetail();
                            transactionDetail.Amount = account.Amount;
                            transactionDetail.AccountId = account.Id;
                            transaction.TransactionDetails.Add( transactionDetail );
                        }

                        // Get the batch name
                        string ccSuffix = string.Empty;
                        if ( paymentInfo.CreditCardTypeValue != null )
                        {
                            ccSuffix = paymentInfo.CreditCardTypeValue.GetAttributeValue( "BatchNameSuffix" );
                        }
                        if ( string.IsNullOrWhiteSpace( ccSuffix ) )
                        {
                            ccSuffix = paymentInfo.CurrencyTypeValue.Name;
                        }
                        string batchName = GetAttributeValue( "BatchNamePrefix" ).Trim() + " " + ccSuffix;

                        using ( new UnitOfWorkScope() )
                        {
                            var batchService = new FinancialBatchService();
                            var batch = batchService.Queryable()
                                .Where( b =>
                                    b.Status == BatchStatus.Open &&
                                    b.BatchStartDateTime <= transaction.TransactionDateTime &&
                                    b.BatchEndDateTime > transaction.TransactionDateTime &&
                                    b.Name == batchName )
                                .FirstOrDefault();
                            if ( batch == null )
                            {
                                batch = new FinancialBatch();
                                batch.Name = batchName;
                                batch.Status = BatchStatus.Open;
                                batch.BatchStartDateTime = transaction.TransactionDateTime.Value.Date.Add( gateway.BatchTimeOffset );
                                if ( batch.BatchStartDateTime > transaction.TransactionDateTime )
                                {
                                    batch.BatchStartDateTime.Value.AddDays( -1 );
                                }
                                batch.BatchEndDateTime = batch.BatchStartDateTime.Value.AddDays( 1 ).AddMilliseconds( -1 );
                                batch.CreatedByPersonId = person.Id;
                                batchService.Add( batch, CurrentPersonId );
                                batchService.Save( batch, CurrentPersonId );

                                batch = batchService.Get( batch.Id );
                            }

                            batch.ControlAmount += transaction.Amount;
                            batchService.Save( batch, CurrentPersonId );

                            var transactionService = new FinancialTransactionService();
                            transaction.BatchId = batch.Id;
                            transactionService.Add( transaction, CurrentPersonId );
                            transactionService.Save( transaction, CurrentPersonId );
                        }

                        TransactionCode = transaction.TransactionCode;
                    }
                    else
                    {
                        return false;
                    }
                }

                tdTransactionCode.Description = TransactionCode;
                tdTransactionCode.Visible = !string.IsNullOrWhiteSpace( TransactionCode );

                tdScheduleId.Description = ScheduleId;
                tdScheduleId.Visible = !string.IsNullOrWhiteSpace( ScheduleId );

                // If there was a transaction code returned and this was not already created from a previous saved account, 
                // show the option to save the account.
                if ( !( paymentInfo is ReferencePaymentInfo ) && !string.IsNullOrWhiteSpace( TransactionCode ) )
                {
                    cbSaveAccount.Visible = true;
                    pnlSaveAccount.Visible = true;
                    txtSaveAccount.Visible = true;

                    // If current person does not have a login, have them create a username and password
                    phCreateLogin.Visible = !new UserLoginService().GetByPersonId( person.Id ).Any();
                }
                else
                {
                    pnlSaveAccount.Visible = false;
                }

                return true;
            }
            else
            {
                pnlDupWarning.Visible = true;
                errorMessage = string.Empty;
                return false;
            }
        }

        #endregion

        #region Methods used globally

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

        private void RegisterScript()
        {
            CurrentPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;

          string script = string.Format( @"
    Sys.Application.add_load(function () {{

        // As amounts are entered, validate that they are numeric and recalc total
        $('.account-amount').on('change', function() {{
            var totalAmt = Number(0);
            $('input.account-amount').each(function (index) {{
                var itemValue = $(this).val();
                if (itemValue != null && itemValue != '') {{
                    if (isNaN(itemValue)) {{
                        $(this).parents('div.control-group').addClass('error');
                    }}
                    else {{
                        $(this).parents('div.control-group').removeClass('error');
                        var num = Number(itemValue);
                        $(this).val(num.toFixed(2));
                        totalAmt = totalAmt + num;
                    }}
                }}
                else {{
                    $(this).parents('div.control-group').removeClass('error');
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
                    $dateInput.data('kendoDatePicker').value(mm+'/'+dd+'/'+yy);
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

        // detect credit card type
        $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card_logos' }});

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
            $(this).parents('div.form-group:first').next('.toggle-content').slideToggle();
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

        #region Helper Classes 

        [Serializable]
        protected class AccountItem
        {
            public int Id { get; set; }
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

            public AccountItem( int id, string name, int? campusId )
            {
                Id = id;
                Name = name;
                CampusId = campusId;
            }
        }

        #endregion

        #endregion

    }
        
}

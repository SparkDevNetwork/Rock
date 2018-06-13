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
using Rock.Security;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RockWeb.Plugins.church_ccv.Finance
{
    #region Block Attributes

    /// <summary>
    /// Add a new one-time transaction with option 
    /// to save payment account and option 
    /// to convert transaction to a schedule after transaction has been processed
    /// </summary>
    [DisplayName( "CCV Transaction Entry" )]
    [Category( "CCV > Finance" )]
    [Description( "Creates a new financial transaction.  Customized to allow converting successful transaction to a schedule and/or saving payment account for future gifts. Designed around Payflow Pro type gateway" )]
    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [FinancialGatewayField( "ACH Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving", "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false,
        Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 3 )]
    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]
    [BooleanField( "Scheduled Transactions", "Allow", "Don't Allow",
        "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 8, "AllowScheduled" )]
    [BooleanField("Save Payment Accounts", "Allow", "Don't Allow", "Can the person save payment accounts for future gifts", true, "", 9, "AllowSavePaymentAccounts")]
    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 11 )]
    
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 25 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 26 )]

    [SystemEmailField( "Receipt Email", "The system email to use to send the receipt.", false, "", "Email Templates", 27 )]
    [CodeEditorField( "Payment Comment", @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>. Merge fields include: <pre>CurrentPerson: {},
PageParameters {},
TransactionDateTime: '8/29/2016',
CurrencyType: {
  'AttributeIds': [],
  'IsSystem': true,
  'DefinedTypeId': 10,
  'Order': 2,
  'Value': 'Credit Card',
  'Description': 'Credit Card',
  'TypeId': 31,
  'TypeName': 'Rock.Model.DefinedValue',
  'AttributeValues': {},
  'Id': 156,
  'Guid': '928a2e04-c77b-4282-888f-ec549cee026a',
  'ForeignId': null,
  'ForeignGuid': null,
  'ForeignKey': null
}
TransactionAcountDetails: [
  {
    'Id': 1,
    'Order': 0,
    'Name': 'General Fund',
    'CampusId': null,
    'Amount': 50.00,
    'PublicName': 'General Fund',
    'AmountFormatted': '$50.00'
  },
  {
    'Id': 2,
    'Order': 1,
    'Name': 'Building Fund',
    'CampusId': null,
    'Amount': 10.00,
    'PublicName': 'Building Fund',
    'AmountFormatted': '$10.00'
  }
]</pre>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false, "Online Contribution", "", 28 )]
    [BooleanField( "Enable Comment Entry", "Allows the guest to enter the the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", false, "", 29 )]
    [TextField( "Comment Entry Label", "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", false, "Comment", "", 30 )]
    [TextField( "Fund / Account Dropdown Placeholder", "The placeholder text to use in the account/fund dropdown (e.g. --Select a Fund-- or --Select a Trip--).", false, "--Select A Fund--", "", 31, "FundDropdownPlaceholder" )]

    #endregion

    public partial class CCVTransactionEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Person _person = null;
        private FinancialGateway _ccGateway;
        private GatewayComponent _ccGatewayComponent = null;
        private FinancialGateway _achGateway;
        private GatewayComponent _achGatewayComponent = null;
        
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
            get { return ViewState["GroupLocationId"] as int?; }
            set { ViewState["GroupLocationId"] = value; }
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
        /// Gets or sets the currency type value identifier.
        /// </summary>
        protected int? CreditCardTypeValueId
        {
            get { return ViewState["CreditCardTypeValueId"] as int?; }
            set { ViewState["CreditCardTypeValueId"] = value; }
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

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            using ( var rockContext = new RockContext() )
            {
                // set person if currently logged in
                _person = CurrentPerson;

                // setup gateway and persons save
                SetGatewayOptions( rockContext );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Check for required block settings
            if ( _ccGateway == null && _achGateway == null )
            {
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Please check the configuration of this block and make sure a valid Credit Card and/or ACH Financial Gateway has been selected." );
                return;
            }

            if ( !Page.IsPostBack )
            {
                // create new transaction guid
                hfTransactionGuid.Value = Guid.NewGuid().ToString();
                
                // Bind dropdown lists
                BindFundAccounts();
                BindCountries();

                // Evaluate if comment entry box should be displayed
                tbCommentEntry.Label = GetAttributeValue( "CommentEntryLabel" );
                tbCommentEntry.Visible = GetAttributeValue( "EnableCommentEntry" ).AsBoolean();

                // if person logged in, prepopulate form fields
                if ( _person != null )
                {
                    tbFirstName.Text = _person.FirstName;
                    tbLastName.Text = _person.LastName;
                    tbEmail.Text = _person.Email;

                     using ( var rockContext = new RockContext() )
                    {
                        // Set saved payment accounts
                        BindSavedAccounts( rockContext, true );

                        // if person has an account username and if save new payment account is allowed in block settings
                        // configure and enable save payment type account panel on success panel
                        if ( new UserLoginService( rockContext ).GetByPersonId( _person.Id ).Any() && GetAttributeValue( "AllowSavePaymentAccounts" ).AsBoolean() )
                        {
                            // configure save payment toggle
                            tglSavePaymentAccount.InputAttributes.Add( "data-toggle", "collapse" );
                            tglSavePaymentAccount.InputAttributes.Add( "data-target", "#pnlSavePaymentAccountInput" );
                            
                            // Enable save payment account panel
                            pnlSavePaymentAccount.Visible = true;

                            // Update success hyperlink
                            hlSuccessLink.Text = "Manage Automated Giving";
                            hlSuccessLink.NavigateUrl = "/dashboard/manage-automated-giving";
                        }                            
                    }                        
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Click event to start processing transaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnConfirmNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;

            if ( ProcessTransaction( out errorMessage ) )
            {
                // Success - hide Transaction panel and show payment success panel
                nbMessage.Visible = false;
                pnlTransaction.Visible = false;
                pnlPaymentSuccess.Visible = true;
            }
            else
            {
                // Failed - show error message
                ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );

                // Reenable submit button on confirmation page
                btnConfirmNext.Enabled = true;

                // set view to payment panel
                TogglePanel( "pnlPayment", true );
                TogglePanel( "pnlAmount", false );

                // set progress indicators
                ToggleProgressIndicator( "btnProgressAmount", true, true );
                ToggleProgressIndicator( "btnProgressPerson", true, true );
                ToggleProgressIndicator( "btnProgressPayment", true, false );

                // set payment type form panel view
                if (hfPaymentType.Value == "REF")
                {
                    // Show Saved Payment Form
                    TogglePanel( "pnlSavedPayment", true );
                    TogglePanel( "pnlCreditCard", false );
                    TogglePanel( "pnlBankAccount", false );
                    
                    // Set Saved Payment Button
                    ToggleButtonSelectedState( "btnSavedPayment", true );
                    ToggleButtonSelectedState( "btnCreditCard", false );
                    ToggleButtonSelectedState( "btnBankAccount", false );
                } else if (hfPaymentType.Value == "ACH")
                {
                    // Show Bank Account Form
                    TogglePanel( "pnlBankAccount", true );
                    TogglePanel( "pnlSavedPayment", false );
                    TogglePanel( "pnlCreditCard", false );

                    // Set Bank Account Button
                    ToggleButtonSelectedState( "btnBankAccount", true );
                    ToggleButtonSelectedState( "btnSavedPayment", false );
                    ToggleButtonSelectedState( "btnCreditCard", false );
                } else
                {
                    // Show Credit Card Form
                    TogglePanel( "pnlCreditCard", true );
                    TogglePanel( "pnlBankAccount", false );
                    TogglePanel( "pnlSavedPayment", false );

                    // Set Credit Card Button
                    ToggleButtonSelectedState( "btnCreditCard", true );
                    ToggleButtonSelectedState( "btnBankAccount", false );
                    ToggleButtonSelectedState( "btnSavedPayment", false );
                }
            }
        }

        /// <summary>
        /// Click event to save payment type or schdule transaction to persons account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSaveSuccessInputForm_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;

            // check if saved payment form is showing
            if ( tglSavePaymentAccount.Checked && pnlSavePaymentAccount.Visible == true )
            {
                string returnedErrorMessage = string.Empty;

                // Save payment account info
                if ( SavePaymentAccount( out returnedErrorMessage ) )
                {
                    // hide Save Payment Panel to prevent duplicate save
                    pnlSavePaymentAccount.Visible = false;

                    // Update Result - Remove error class
                    lblSavePaymentResult.Text = "New Payment Account has been successfully saved.";
                    lblSavePaymentResult.RemoveCssClass( "error" );

                } else
                {
                    // Failed: Update Error Message
                    errorMessage += "Save Payment Account Failed: " + returnedErrorMessage + "<br />";

                    // Resetup view so in same spot after post back
                    tglSavePaymentAccount.Checked = true;
                    pnlSavePaymentAccountInput.AddCssClass( "in" );
                    btnSaveSuccessInputForm.RemoveCssClass( "hidden" );
                    btnSaveSuccessInputForm.Enabled = true;
                }

                lblSavePaymentResult.Visible = true;
            } else
            {
                // saved payment is not toggled, resetup view in case its not up to date after postback
                pnlSavePaymentAccountInput.RemoveCssClass( "in" );
            }

            // check if schedule transaction form is toggled
            if ( tglScheduleTransaction.Checked && pnlScheduleTransaction.Visible == true )
            {
                string returnedErrorMessage = string.Empty;

                // Save transaction schedule
                if ( SaveSchedule( out returnedErrorMessage ) )
                {
                    // Hide Schedule Transaction to prevent duplicate schedule saving
                    pnlScheduleTransaction.Visible = false;

                    // Update Result - Remove error class
                    lblSaveScheduleTransactionResult.Text = "New Schedule has been successfully created.";
                    lblSaveScheduleTransactionResult.RemoveCssClass( "error" );
                } else
                {
                    // Failed: Update Error Message
                    errorMessage += "Schedule Transaction Save Failed: " + returnedErrorMessage + "<br />";

                    // Resetup view so in same spot after post back
                    tglScheduleTransaction.Checked = true;
                    pnlScheduleTransactionInput.AddCssClass( "in" );
                    btnSaveSuccessInputForm.RemoveCssClass( "hidden" );
                    btnSaveSuccessInputForm.Enabled = true;
                }

                lblSaveScheduleTransactionResult.Visible = true;
            } else
            {
                // schedule transaction form is not toggled, reset view in case its not up to date after postback
                pnlScheduleTransactionInput.RemoveCssClass( "in" );
            }

            if (!errorMessage.IsNullOrWhiteSpace())
            {
                // hide success checkmark and success text
                pnlSuccessCheckmark.Visible = false;
                lblSuccessMessage.Visible = false;
                // display error
                ShowMessage( NotificationBoxType.Danger, "Save Error", errorMessage );
            } else
            {
                // show success checkmark and success text
                pnlSuccessCheckmark.Visible = true;
                lblSuccessMessage.Visible = true;
                // Success: Update confirmation panel success message
                lblSuccessMessage.Text = "Save Successful";
            }
        }

        #endregion

        #region Methods

        #region Initialization Methods

        private void SetGatewayOptions( RockContext rockContext )
        {
            _ccGateway = GetGateway( rockContext, "CCGateway" );
            _ccGatewayComponent = GetGatewayComponent( rockContext, _ccGateway );
            bool ccEnabled = _ccGatewayComponent != null;

            _achGateway = GetGateway( rockContext, "ACHGateway" );
            _achGatewayComponent = GetGatewayComponent( rockContext, _achGateway );
            bool achEnabled = _achGatewayComponent != null;

            bool allowScheduled = GetAttributeValue( "AllowScheduled" ).AsBoolean();
            if ( allowScheduled && ( ccEnabled || achEnabled ) )
            {
                // setup supported schedule frequencies
                var supportedFrequencies = ccEnabled ? _ccGatewayComponent.SupportedPaymentSchedules : _achGatewayComponent.SupportedPaymentSchedules;

                // If CC and ACH gateways are both enabled, but different, only allow frequencies supported by both payment gateways (if different)
                if ( ccEnabled && achEnabled && _ccGatewayComponent.TypeId != _achGatewayComponent.TypeId )
                {
                    supportedFrequencies = _ccGatewayComponent.SupportedPaymentSchedules
                        .Where( c =>
                            _achGatewayComponent.SupportedPaymentSchedules
                                .Select( a => a.Id )
                                .Contains( c.Id ) )
                        .ToList();
                }

                if ( supportedFrequencies.Any() )
                {
                    ddlScheduleFrequency.Items.Clear();

                    // if more than one frequency add placeholder text
                    if (supportedFrequencies.Count != 1)
                    {
                        ddlScheduleFrequency.Items.Add( new ListItem( "--Select Frequency--", "-1" ) );
                    }

                    // add each frequency to the radio button list, except "One-Time"
                    foreach ( var frequency in supportedFrequencies )
                    {
                        if ( frequency.Value != "One-Time" )
                        {
                             ddlScheduleFrequency.Items.Add( new ListItem( frequency.Value, frequency.Guid.ToString() ) );
                        }
                    }

                    // configure schedule transaction toggle attributes and enable toggle
                    tglScheduleTransaction.InputAttributes.Add( "data-toggle", "collapse" );
                    tglScheduleTransaction.InputAttributes.Add( "data-target", "#pnlScheduleTransactionInput" );
                }

                // Enable schedule panel
                pnlScheduleTransaction.Visible = true;
            }
        }

        private FinancialGateway GetGateway( RockContext rockContext, string attributeName )
        {
            var financialGatewayService = new FinancialGatewayService( rockContext );
            Guid? ccGatewayGuid = GetAttributeValue( attributeName ).AsGuidOrNull();
            if ( ccGatewayGuid.HasValue )
            {
                return financialGatewayService.Get( ccGatewayGuid.Value );
            }
            return null;
        }

        private GatewayComponent GetGatewayComponent( RockContext rockContext, FinancialGateway gateway )
        {
            if ( gateway != null )
            {
                gateway.LoadAttributes( rockContext );
                return gateway.GetGatewayComponent();
            }
            return null;
        }

        /// <summary>
        /// Binds the saved accounts and if accounts exist it enables the saved accounts panel.
        /// </summary>
        private void BindSavedAccounts( RockContext rockContext, bool oneTime )
        {
            ddlSavedPaymentAccounts.Items.Clear();

            if ( _person != null )
            {
                // Get the saved payment accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService( rockContext )
                    .GetByPersonId( _person.Id )
                    .ToList();

                // Find the saved payment accounts that are valid for the selected CC gateway
                var ccSavedAccountIds = new List<int>();
                var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                if ( _ccGateway != null &&
                    _ccGatewayComponent != null &&
                    _ccGatewayComponent.SupportsSavedAccount( !oneTime ) &&
                    _ccGatewayComponent.SupportsSavedAccount( ccCurrencyType ) )
                {
                    ccSavedAccountIds = savedAccounts
                        .Where( a =>
                            a.FinancialGatewayId == _ccGateway.Id &&
                            a.FinancialPaymentDetail != null &&
                            a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id )
                        .Select( a => a.Id )
                        .ToList();
                }

                // If saved credit card accounts exist, populate dropdownlist, set placeholder text, and enable panel
                if ( ccSavedAccountIds.Count > 0 )
                {
                    // If more than 1 saved credit card account set placeholder item
                    if ( ccSavedAccountIds.Count != 1 )
                    {
                        ddlSavedPaymentAccounts.Items.Add( new ListItem( "--Select Saved Account--", "-1" ) );
                    }

                    // Loop through the saved accounts and add them to the dropdown
                    // if they have a matching id in ccSavedAccountId's
                    foreach ( var savedAccount in savedAccounts )
                    {
                        if ( ccSavedAccountIds.Contains( savedAccount.Id ) )
                        {
                            // Format account name
                            string accountName = String.Format( "{0} ({1})", savedAccount.Name, savedAccount.FinancialPaymentDetail.AccountNumberMasked.Substring( savedAccount.FinancialPaymentDetail.AccountNumberMasked.Length - 8 ) );

                            // add to dropdown
                            ddlSavedPaymentAccounts.Items.Add( new ListItem( accountName, savedAccount.Id.ToString() ) );
                        }
                    }

                    // Saved payment accounts exist, enable Saved Payment panel 
                    btnSavedPayment.Visible = true;

                    // set the hidden field since the saved panel will be the active panel
                    hfPaymentType.Value = "REF";

                    // set it as active payment panel
                    pnlCreditCard.AddCssClass( "hidden" );
                    pnlBankAccount.AddCssClass( "hidden" );
                    pnlSavedPayment.RemoveCssClass( "hidden" );

                    // set it as active payment button
                    btnCreditCard.RemoveCssClass( "btn-primary" );
                    btnBankAccount.RemoveCssClass( "btn-primary" );
                    btnSavedPayment.AddCssClass( "btn-primary" );
                }
                else
                {
                    // No saved payment accounts, hide Saved Payment button
                    btnSavedPayment.Visible = false;
                }
            }
        }

        /// <summary>
        /// Binds the Fund Accounts.
        /// </summary>
        protected void BindFundAccounts()
        {
            var rockContext = new RockContext();

            var selectedAccountGuids = GetAttributeValue( "Accounts" ).SplitDelimitedValues().ToList().AsGuidList();
            var today = RockDateTime.Today;

            var accountsQry = new FinancialAccountService( rockContext ).Queryable()
                    .Where( f =>
                        f.IsActive &&
                        f.IsPublic.HasValue &&
                        f.IsPublic.Value &&
                        ( f.StartDate == null || f.StartDate <= today ) &&
                        ( f.EndDate == null || f.EndDate >= today ) );

            if ( selectedAccountGuids.Any() )
            {
                accountsQry = accountsQry.Where( a => selectedAccountGuids.Contains( a.Guid ) );
            }

            var accounts = accountsQry
                    .OrderBy( f => f.Order )
                    .ThenBy( f => f.Name )
                    .Select( a => new
                    {
                        AccountId = a.Id.ToString(),
                        Name = a.Name,
                        CampusId = a.CampusId
                    } ).ToList();

            ddlAccounts.Items.Clear();

            // populate dropdownlist if fund accounts exist
            if ( accounts.Any() )
            {
                if ( accounts.Count != 1 )
                {
                    // add placeholder text
                    string placeholder = GetAttributeValue( "FundDropdownPlaceholder" );

                    if ( placeholder.IsNotNullOrWhitespace() )
                    {
                        ddlAccounts.Items.Add( new ListItem( placeholder, "-1" ) );
                    }
                    else
                    {
                        ddlAccounts.Items.Add( new ListItem( "--Select A Fund--", "-1" ) );
                    }
                }

                // add accounts to dropdown
                foreach ( var account in accounts )
                {
                    ddlAccounts.Items.Add( new ListItem( account.Name, account.AccountId.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Binds the countries.
        /// </summary>
        private void BindCountries()
        {
            string currentValue = ddlCountry.SelectedValue;

            ddlCountry.Items.Clear();
            ddlCountry.SelectedIndex = -1;
            ddlCountry.SelectedValue = null;
            ddlCountry.ClearSelection();

            var definedType = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES ) );
            var countryValues = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() )
                .DefinedValues
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();

            // Move default country to the top of the list
            string defaultCountryCode = GetDefaultCountry();
            if ( !string.IsNullOrWhiteSpace( defaultCountryCode ) )
            {
                var defaultCountry = countryValues
                    .Where( v => v.Value.Equals( defaultCountryCode, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
                if ( defaultCountry != null )
                {
                    ddlCountry.Items.Add( new ListItem( true ? defaultCountry.Value : defaultCountry.Description, defaultCountry.Value ) );
                    ddlCountry.Items.Add( new ListItem( "------------------------", string.Empty ) );
                }
            }

            foreach ( var country in countryValues )
            {
                ddlCountry.Items.Add( new ListItem( true ? country.Value : country.Description, country.Value ) );
            }

            bool? showCountry = GlobalAttributesCache.Read().GetValue( "SupportInternationalAddresses" ).AsBooleanOrNull();
            ddlCountry.Visible = showCountry.HasValue && showCountry.Value;

            if ( !string.IsNullOrWhiteSpace( currentValue ) )
            {
                ddlCountry.SetValue( currentValue );
            }
        }

        #endregion

        #region Process/Save Methods

        /// <summary>
        /// Processes the confirmation.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessTransaction( out string errorMessage )
        {
            var rockContext = new RockContext();
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                var transactionGuid = hfTransactionGuid.Value.AsGuid();

                bool isACHTxn = hfPaymentType.Value == "ACH";
                var financialGateway = isACHTxn ? _achGateway : _ccGateway;
                var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;

                if ( gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                // Set Person / Create if doesnt exist
                if ( _person == null )
                {
                    _person = GetPerson();

                    if ( _person == null )
                    {
                        errorMessage = "There was a problem creating the person information";
                        return false;
                    }
                }

                if ( !_person.PrimaryAliasId.HasValue )
                {
                    errorMessage = "There was a problem creating the person's primary alias";
                    return false;
                }

                // Set Account / Fund Id and Name
                int fundAccountId = ddlAccounts.SelectedValue.AsInteger();
                if ( fundAccountId == -1 )
                {
                    errorMessage = "There was a problem getting the account/fund id";
                    return false;
                }

                string fundAccountName = ddlAccounts.SelectedItem.Text;
                if ( fundAccountName.IsNullOrWhiteSpace() )
                {
                    errorMessage = "There was a problem getting the account/fund name";
                    return false;
                }

                // Set Payment Info
                PaymentInfo paymentInfo = GetTxnPaymentInfo( _person, out errorMessage );
                if ( paymentInfo == null )
                {
                    errorMessage = "There was a problem creating the payment transaction";
                    return false;
                }

                var transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                if ( transactionAlreadyExists != null )
                {
                    // hopefully shouldn't happen, but just in case the transaction already went thru, show the success screen
                    ShowTransactionSuccess( gateway, _person, paymentInfo, fundAccountName, transactionAlreadyExists.FinancialPaymentDetail, rockContext );
                    return true;
                }

                // charge transaction
                var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                if ( transaction == null )
                {
                    return false;
                }

                // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
                transaction.Guid = transactionGuid;

                SaveTransaction( financialGateway, gateway, _person, paymentInfo, fundAccountId, fundAccountName, transaction, rockContext );

                // check if reference transaction or ACH and hide save new payment account if so
                // **PayFlow Pro does not support saving ACH accounts
                if ( paymentInfo is ReferencePaymentInfo || paymentInfo is ACHPaymentInfo )
                {
                    pnlSavePaymentAccount.Visible = false;
                }

                ShowTransactionSuccess( gateway, _person, paymentInfo, fundAccountName, transaction.FinancialPaymentDetail, rockContext );

                // encrypt persisted data and save to view state
                PersistedPaymentInfo persistedPaymentInfo = CreatePersistedPaymentInfo( transaction, paymentInfo, fundAccountName, fundAccountId );

                string persistedPaymentInfoString = ObjectToString( persistedPaymentInfo );

                string encryptedPersistedPaymentInfo = Encryption.EncryptString( persistedPaymentInfoString );

                ViewState["PersistedInfo"] = encryptedPersistedPaymentInfo;

                return true;
            }
            else
            {
                errorMessage = string.Empty;
                return false;
            }
        }

        private void SaveTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, int accountId, string accountName, FinancialTransaction transaction, RockContext rockContext )
        {
            var txnChanges = new List<string>();
            txnChanges.Add( "Created Transaction" );

            History.EvaluateChange( txnChanges, "Transaction Code", string.Empty, transaction.TransactionCode );

            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            History.EvaluateChange( txnChanges, "Person", string.Empty, person.FullName );

            transaction.TransactionDateTime = RockDateTime.Now;
            History.EvaluateChange( txnChanges, "Date/Time", null, transaction.TransactionDateTime );

            transaction.FinancialGatewayId = financialGateway.Id;
            History.EvaluateChange( txnChanges, "Gateway", string.Empty, financialGateway.Name );

            var txnType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
            transaction.TransactionTypeValueId = txnType.Id;
            History.EvaluateChange( txnChanges, "Type", string.Empty, txnType.Value );

            transaction.Summary = paymentInfo.Comment1;
            History.EvaluateChange( txnChanges, "Summary", string.Empty, transaction.Summary );

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }
            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext, txnChanges );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Read( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                    History.EvaluateChange( txnChanges, "Source", string.Empty, source.Value );
                }
            }

            var transactionDetail = new FinancialTransactionDetail();
            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = accountId;
            transaction.TransactionDetails.Add( transactionDetail );
            History.EvaluateChange( txnChanges, accountName, 0.0M.FormatAsCurrency(), transactionDetail.Amount.FormatAsCurrency() );

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                GetAttributeValue( "BatchNamePrefix" ),
                paymentInfo.CurrencyTypeValue,
                paymentInfo.CreditCardTypeValue,
                transaction.TransactionDateTime.Value,
                financialGateway.GetBatchTimeOffset() );

            var batchChanges = new List<string>();

            if ( batch.Id == 0 )
            {
                batchChanges.Add( "Generated the batch" );
                History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
            }

            decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
            History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
            batch.ControlAmount = newControlAmount;

            transaction.BatchId = batch.Id;
            batch.Transactions.Add( transaction );

            rockContext.SaveChanges();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges
            );

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                batch.Id,
                txnChanges,
                person.FullName,
                typeof( FinancialTransaction ),
                transaction.Id
            );

            SendReceipt( transaction.Id );

            TransactionCode = transaction.TransactionCode;
        }

        /// <summary>
        /// Save a transaction schedule
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private bool SaveSchedule( out string errorMessage )
        {
            var rockContext = new RockContext();

            errorMessage = string.Empty;

            var transactionGuid = hfTransactionGuid.Value.AsGuid();

            bool isACHTxn = hfPaymentType.Value == "ACH";
            var financialGateway = isACHTxn ? _achGateway : _ccGateway;
            var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;

            PaymentSchedule schedule = GetSchedule();

            if ( schedule == null || schedule.TransactionFrequencyValue == null )
            {
                errorMessage = "There was a problem creating the schedule";
                return false;
            }


            // Get payment info from view state, decrypt and convert to usable type
            string encryptedPersistedPaymentInfo = ViewState["PersistedInfo"] as string;

            if ( encryptedPersistedPaymentInfo.IsNullOrWhiteSpace() )
            {
                errorMessage = "Missing persisted transaction data.";
                return false;
            }

            string decryptedPaymentInfo = Encryption.DecryptString( encryptedPersistedPaymentInfo );

            PersistedPaymentInfo persistedPaymentInfo = StringToObject( decryptedPaymentInfo ) as PersistedPaymentInfo;

            // set person from persisted Id
            var personService = new PersonService( rockContext );
            _person = personService.Get( persistedPaymentInfo.PersonId );

            if ( _person == null )
            {
                errorMessage = "Missing person";
                return false;
            }

            schedule.PersonId = _person.Id;

            // reload payment transaction info
            PaymentInfo paymentInfo = ReloadTxnPaymentInfo( _person, persistedPaymentInfo, out errorMessage );

            // check for payment info
            if ( paymentInfo == null )
            {
                return false;
            }

            var scheduledTransactionAlreadyExists = new FinancialScheduledTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
            if ( scheduledTransactionAlreadyExists != null )
            {
                // hopefully shouldn't happen, but just in case the scheduledtransaction already went thru, show the success screen
                ShowScheduleSuccess( gateway, _person, paymentInfo, persistedPaymentInfo.FundAccountName, schedule, scheduledTransactionAlreadyExists.FinancialPaymentDetail, rockContext );
                return true;
            }

            // Save the schedule with the gateway
            var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
            if ( scheduledTransaction == null )
            {
                return false;
            }

            // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate scheduled transactions impossible
            scheduledTransaction.Guid = transactionGuid;

            SaveScheduledTransaction( financialGateway, gateway, _person, paymentInfo, persistedPaymentInfo.FundAccountId, persistedPaymentInfo.FundAccountName, schedule, scheduledTransaction, rockContext );

            ShowScheduleSuccess( gateway, _person, paymentInfo, persistedPaymentInfo.FundAccountName, schedule, scheduledTransaction.FinancialPaymentDetail, rockContext );

            errorMessage = string.Empty;
            return true;
        }

        private void SaveScheduledTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, int fundAccountId, string fundAccountName, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, RockContext rockContext )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }
            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Read( sourceGuid );
                if ( source != null )
                {
                    scheduledTransaction.SourceTypeValueId = source.Id;
                }
            }

            var changeSummary = new StringBuilder();
            changeSummary.AppendFormat( "{0} starting {1}", schedule.TransactionFrequencyValue.Value, schedule.StartDate.ToShortDateString() );
            changeSummary.AppendLine();
            changeSummary.Append( paymentInfo.CurrencyTypeValue.Value );
            if ( paymentInfo.CreditCardTypeValue != null )
            {
                changeSummary.AppendFormat( " - {0}", paymentInfo.CreditCardTypeValue.Value );
            }
            changeSummary.AppendFormat( " {0}", paymentInfo.MaskedNumber );
            changeSummary.AppendLine();

            var transactionDetail = new FinancialScheduledTransactionDetail();
            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = fundAccountId;
            scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
            changeSummary.AppendFormat( "{0}: {1}", fundAccountName, paymentInfo.Amount.FormatAsCurrency() );
            changeSummary.AppendLine();

            if ( !string.IsNullOrWhiteSpace( paymentInfo.Comment1 ) )
            {
                changeSummary.Append( paymentInfo.Comment1 );
                changeSummary.AppendLine();
            }

            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();

            // Add a note about the change
            var noteType = NoteTypeCache.Read( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
            if ( noteType != null )
            {
                var noteService = new NoteService( rockContext );
                var note = new Note();
                note.NoteTypeId = noteType.Id;
                note.EntityId = scheduledTransaction.Id;
                note.Caption = "Created Transaction";
                note.Text = changeSummary.ToString();
                noteService.Add( note );
            }
            rockContext.SaveChanges();

            ScheduleId = scheduledTransaction.GatewayScheduleId;
            TransactionCode = scheduledTransaction.TransactionCode;
        }

        /// <summary>
        /// Save Payment Type to Peron's Account
        /// </summary>
        protected bool SavePaymentAccount( out string errorMessage )
        {
            errorMessage = string.Empty;

            // Get payment info from view state, decrypt and convert to usable type
            string encryptedPersistedPaymentInfo = ViewState["PersistedInfo"] as string;

            if (encryptedPersistedPaymentInfo.IsNullOrWhiteSpace())
            {
                errorMessage = "Missing persisted transaction data.";
                return false;
            }

            string decryptedPaymentInfo = Encryption.DecryptString( encryptedPersistedPaymentInfo );

            PersistedPaymentInfo persistedPaymentInfo = StringToObject( decryptedPaymentInfo ) as PersistedPaymentInfo;

            if ( persistedPaymentInfo.TransactionCode.IsNullOrWhiteSpace() )
            {
                errorMessage = "Missing transaction code.";
                return false;
            }

            using ( var rockContext = new RockContext() )
            {
                bool isACHTxn = hfPaymentType.Value == "ACH";
                var financialGateway = isACHTxn ? _achGateway : _ccGateway;
                var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;

                if ( gateway != null )
                {
                    var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                    var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );

                    string referenceNumber = string.Empty;
                    FinancialPaymentDetail paymentDetail = null;
                    int? currencyTypeValueId = isACHTxn ? achCurrencyType.Id : ccCurrencyType.Id;

                    var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( persistedPaymentInfo.TransactionCode );
                    if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                    {
                        if ( transaction.FinancialGateway != null )
                        {
                            transaction.FinancialGateway.LoadAttributes( rockContext );
                        }
                        referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                        paymentDetail = transaction.FinancialPaymentDetail;
                    }
                    else
                    {
                        errorMessage = "Failed to load transaction data";
                    }

                    if ( _person != null && paymentDetail != null )
                    {
                        var savedAccount = new FinancialPersonSavedAccount();
                        savedAccount.PersonAliasId = _person.PrimaryAliasId;
                        savedAccount.ReferenceNumber = referenceNumber;
                        savedAccount.Name = tbSavePaymentAccountName.Text;
                        savedAccount.TransactionCode = persistedPaymentInfo.TransactionCode;
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

                        return true;
                    }
                    else
                    {
                        errorMessage = "Missing person or payment data";
                        return false;
                    }
                }
                else
                {
                    errorMessage = "Missing payment gateway information";
                    return false;
                }
            }
        }

        #endregion

        #region Display Methods
        
        /// <summary>
        /// Display Transaction Success information
        /// </summary>
        /// <param name="gatewayComponent"></param>
        /// <param name="person"></param>
        /// <param name="paymentInfo"></param>
        /// <param name="accountName"></param>
        /// <param name="paymentDetail"></param>
        /// <param name="rockContext"></param>
        private void ShowTransactionSuccess( GatewayComponent gatewayComponent, Person person, PaymentInfo paymentInfo, string accountName, FinancialPaymentDetail paymentDetail, RockContext rockContext )
        {
            // hide schedule id and when label
            lblScheduleId.Visible = false;
            lblWhen.Visible = false;
            lblWhenTitle.Visible = false;

            // Update labels with transaction info and set visibility if needed
            lblTransactionCodeTitle.Text = "Transaction Id";
            lblTransactionCode.Text = TransactionCode;
            lblTransactionCode.Visible = true;     
            lblName.Text = paymentInfo.FullName;
            lblEmail.Text = paymentInfo.Email;

            if ( hfPaymentType.Value == "CC" )
            {
                // credit card transaction - show address info
                lblAddress.Text = string.Format( "{0} {1}, {2} {3}", paymentInfo.Street1, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode );
                lblAddressLabel.Visible = true;
                lblAddress.Visible = true;
            }

            lblFund.Text = accountName;
            lblAmount.Text = paymentInfo.Amount.ToString( "C" );
            lblPaymentMethod.Text = paymentInfo.CurrencyTypeValue.Description;

            string acctNumber = paymentInfo.MaskedNumber;
            if ( string.IsNullOrWhiteSpace( acctNumber ) && paymentDetail != null && !string.IsNullOrWhiteSpace( paymentDetail.AccountNumberMasked ) )
            {
                acctNumber = paymentDetail.AccountNumberMasked;
            }

            lblAccountNumber.Text = acctNumber;

            if ( hfPaymentType.Value == "REF" )
            {
                string savedAccountName = Encryption.DecryptString( hfSavedPaymentAccountName.Value );

                if ( !savedAccountName.IsNullOrWhiteSpace() )
                {
                    lblPaymentMethodTitle.Text = "Saved Payment Method";
                    lblPaymentMethod.Text = savedAccountName;
                    lblAccountNumber.Visible = false;
                    lblAccountNumberTitle.Visible = false;
                }
            }

            // Update schedule transaction amount
            lblScheduleTransactionAmount.Text = paymentInfo.Amount.ToString( "C" );
        }

        /// <summary>
        /// Display Schedule Success information
        /// </summary>
        /// <param name="gatewayComponent"></param>
        /// <param name="person"></param>
        /// <param name="paymentInfo"></param>
        /// <param name="accountName"></param>
        /// <param name="schedule"></param>
        /// <param name="paymentDetail"></param>
        /// <param name="rockContext"></param>
        private void ShowScheduleSuccess( GatewayComponent gatewayComponent, Person person, PaymentInfo paymentInfo, string accountName, PaymentSchedule schedule, FinancialPaymentDetail paymentDetail, RockContext rockContext )
        {
            // hide transaction id labal
            lblTransactionCode.Visible = false;

            // Update labels with confirmation info and set visibility if needed
            lblTransactionCodeTitle.Text = "Payment Schedule Id";
            lblScheduleId.Text = ScheduleId;
            lblScheduleId.Visible = true;
            lblName.Text = paymentInfo.FullName;
            lblEmail.Text = paymentInfo.Email;

            // check for address and credit card transaction
            if ( !paymentInfo.Street1.IsNullOrWhiteSpace() && hfPaymentType.Value == "CC" )
            {
                // show address info
                lblAddress.Text = string.Format( "{0} {1}, {2} {3}", paymentInfo.Street1, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode );
                lblAddressLabel.Visible = true;
                lblAddress.Visible = true;
            }
            else
            {
                // hide address fields
                lblAddressLabel.Visible = false;
                lblAddress.Visible = false;
            }

            lblFund.Text = accountName;
            lblAmount.Text = paymentInfo.Amount.ToString( "C" );
            lblPaymentMethod.Text = paymentInfo.CurrencyTypeValue.Description;

            string acctNumber = paymentInfo.MaskedNumber;
            if ( string.IsNullOrWhiteSpace( acctNumber ) && paymentDetail != null && !string.IsNullOrWhiteSpace( paymentDetail.AccountNumberMasked ) )
            {
                acctNumber = paymentDetail.AccountNumberMasked;
            }

            lblAccountNumber.Text = acctNumber;
            lblWhen.Text = schedule.ToString();
            lblWhen.Visible = true;
            lblWhenTitle.Visible = true;
        }

        /// <summary>
        /// Displays a message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        private void ShowMessage( NotificationBoxType type, string title, string text )
        {
            if ( !string.IsNullOrWhiteSpace( text ) )
            {
                nbMessage.Text = text;
                nbMessage.Title = string.IsNullOrWhiteSpace( title ) ? "" : string.Format( "<p>{0}</p>", title );
                nbMessage.NotificationBoxType = type;
                nbMessage.Visible = true;
            }
        }

        #endregion
        
        #region Methods used globally

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        private Person GetPerson()
        {
            Person person = null;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            Group familyGroup = null;

            int personId = ViewState["PersonId"] as int? ?? 0;
            if ( personId == 0 && _person != null )
            {
                personId = _person.Id;
            }

            if ( personId != 0 )
            {
                person = personService.Get( personId );
            }

            if ( person == null )
            {
                // Check to see if there's only one person with same email, first name, and last name
                if ( !string.IsNullOrWhiteSpace( tbEmail.Text ) &&
                    !string.IsNullOrWhiteSpace( tbFirstName.Text ) &&
                    !string.IsNullOrWhiteSpace( tbLastName.Text ) )
                {
                    // Same logic as CreatePledge.ascx.cs
                    var personMatches = personService.GetByMatch( tbFirstName.Text, tbLastName.Text, tbEmail.Text );
                    if ( personMatches.Count() == 1 )
                    {
                        person = personMatches.FirstOrDefault();
                    }
                    else
                    {
                        person = null;
                    }
                }

                if ( person == null )
                {
                    DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                    DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

                    // Create Person
                    person = new Person();
                    person.FirstName = tbFirstName.Text;
                    person.LastName = tbLastName.Text;
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
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

                ViewState["PersonId"] = person != null ? person.Id : 0;
            }


            if ( person != null ) // person should never be null at this point
            {
                person.Email = tbEmail.Text;

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
                        familyGroup = personService.GetFamilies( person.Id ).FirstOrDefault();
                    }
                }

                rockContext.SaveChanges();
            }

            return person;
        }

        /// <summary>
        /// Get Transaction Payment Info
        /// </summary>
        /// <param name="person"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private PaymentInfo GetTxnPaymentInfo( Person person, out string errorMessage )
        {
            PaymentInfo paymentInfo = GetPaymentInfo();
            if ( paymentInfo == null )
            {
                errorMessage = "There was a problem creating the payment information";
                return null;
            }
            else
            {
                paymentInfo.FirstName = person.FirstName;
                paymentInfo.LastName = person.LastName;
            }

            if ( paymentInfo.CreditCardTypeValue != null )
            {
                CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
            }

            // get the payment comment 
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "TransactionDateTime", RockDateTime.Now );

            if ( paymentInfo != null )
            {
                mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );
            }
            if ( SelectedAccounts != null )
            {
                mergeFields.Add( "TransactionAccountDetails", SelectedAccounts.Where( a => a.Amount != 0 ).ToList() );
            }

            string paymentComment = GetAttributeValue( "PaymentComment" ).ResolveMergeFields( mergeFields );

            paymentInfo.Comment1 = paymentComment;

            if ( GetAttributeValue( "EnableCommentEntry" ).AsBoolean() )
            {
                paymentInfo.Comment1 = !string.IsNullOrWhiteSpace( paymentComment ) ? string.Format( "{0}: {1}", paymentComment, tbCommentEntry.Text ) : tbCommentEntry.Text;
            }
            else
            {
                paymentInfo.Comment1 = paymentComment;
            }

            errorMessage = string.Empty;
            return paymentInfo;
        }

        /// <summary>
        /// Reload Transaction Payment Info
        /// </summary>
        /// <param name="person"></param>
        /// <param name="persistedPaymentInfo"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private PaymentInfo ReloadTxnPaymentInfo( Person person, PersistedPaymentInfo persistedPaymentInfo, out string errorMessage )
        {
            PaymentInfo paymentInfo = ReloadPaymentInfo( persistedPaymentInfo );
            if ( paymentInfo == null )
            {
                errorMessage = "There was a problem creating the payment information";
                return null;
            }
            else
            {
                paymentInfo.FirstName = person.FirstName;
                paymentInfo.LastName = person.LastName;
            }

            if ( paymentInfo.CreditCardTypeValue != null )
            {
                CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
            }

            // get the payment comment 
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "TransactionDateTime", RockDateTime.Now );

            if ( paymentInfo != null )
            {
                mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );
            }
            if ( SelectedAccounts != null )
            {
                mergeFields.Add( "TransactionAccountDetails", SelectedAccounts.Where( a => a.Amount != 0 ).ToList() );
            }

            string paymentComment = GetAttributeValue( "PaymentComment" ).ResolveMergeFields( mergeFields );

            paymentInfo.Comment1 = paymentComment;

            errorMessage = string.Empty;
            return paymentInfo;
        }

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        /// <returns></returns>
        private PaymentInfo GetPaymentInfo()
        {
            PaymentInfo paymentInfo = null;

            if ( ddlSavedPaymentAccounts.Items.Count > 0 && hfPaymentType.Value == "REF" && ( ddlSavedPaymentAccounts.SelectedValueAsId() ?? 0 ) > 0 )
            {
                paymentInfo = GetReferenceInfo( ddlSavedPaymentAccounts.SelectedValueAsId().Value );
            }
            else if ( hfPaymentType.Value == "ACH" )
            {
                paymentInfo = GetACHInfo();
            }
            else
            {
                paymentInfo = GetCCInfo();

                paymentInfo.Street1 = tbStreet.Text;
                paymentInfo.City = tbCity.Text;
                paymentInfo.State = ddlState.SelectedValue;
                paymentInfo.PostalCode = nbPostalCode.Text;
                paymentInfo.Country = ddlCountry.SelectedValue;
            }

            paymentInfo.Amount = nbAmount.Text.Replace( "$", "" ).Replace(",", "").AsDecimal();

            paymentInfo.Email = tbEmail.Text;

            return paymentInfo;
        }

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        /// <returns></returns>
        private PaymentInfo ReloadPaymentInfo( PersistedPaymentInfo persistedPaymentInfo )
        {
            PaymentInfo paymentInfo = null;
            if ( ddlSavedPaymentAccounts.Items.Count > 0 && hfPaymentType.Value == "REF" && ( ddlSavedPaymentAccounts.SelectedValueAsId() ?? 0 ) > 0 )
            {
                paymentInfo = GetReferenceInfo( ddlSavedPaymentAccounts.SelectedValueAsId().Value );
            }
            else if ( hfPaymentType.Value == "ACH" )
            {
                paymentInfo = ReloadACHInfo( persistedPaymentInfo );
            }
            else if ( hfPaymentType.Value == "CC" )
            {
                paymentInfo = ReloadCCInfo( persistedPaymentInfo );
            }
            else
            {
                // invalid paymentInfo
                return null;
            }

            paymentInfo.Amount = persistedPaymentInfo.Amount.Replace( "$", "" ).Replace( ",", "" ).AsDecimal();
            paymentInfo.Email = persistedPaymentInfo.Email;

            return paymentInfo;
        }

        /// <summary>
        /// Gets the credit card information.
        /// </summary>
        /// <returns></returns>
        private CreditCardPaymentInfo GetCCInfo()
        {
            var cc = new CreditCardPaymentInfo( nbCreditCard.Text, nbCVV.Text, mypExpirationDate.SelectedDate ?? DateTime.MinValue );
            cc.NameOnCard = tbName.Text;

            cc.BillingStreet1 = tbStreet.Text;
            cc.BillingCity = tbCity.Text;
            cc.BillingState = ddlState.SelectedValue;
            cc.BillingPostalCode = nbPostalCode.Text;
            cc.BillingCountry = ddlCountry.SelectedValue;

            return cc;
        }

        /// <summary>
        /// Gets the ACH information.
        /// </summary>
        /// <returns></returns>
        private ACHPaymentInfo GetACHInfo()
        {
            return new ACHPaymentInfo( nbAccountNumber.Text, nbRoutingNumber.Text, rblAccountType.SelectedValue == "Savings" ? BankAccountType.Savings : BankAccountType.Checking );
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
                hfSavedPaymentAccountName.Value = Rock.Security.Encryption.EncryptString( savedAccount.Name );
                return savedAccount.GetReferencePayment();
            }

            return null;
        }

        /// <summary>
        /// Reload the credit card information.
        /// </summary>
        /// <returns></returns>
        private CreditCardPaymentInfo ReloadCCInfo( PersistedPaymentInfo persistedPaymentInfo )
        {
            var cc = new CreditCardPaymentInfo( persistedPaymentInfo.CreditCardNumber, persistedPaymentInfo.CVV, persistedPaymentInfo.ExpirationDate );
            cc.NameOnCard = persistedPaymentInfo.NameOnCard;

            cc.BillingStreet1 = persistedPaymentInfo.Street;
            cc.BillingCity = persistedPaymentInfo.City;
            cc.BillingState = persistedPaymentInfo.State;
            cc.BillingPostalCode = persistedPaymentInfo.PostalCode;
            cc.BillingCountry = persistedPaymentInfo.Country;

            return cc;
        }

        /// <summary>
        /// Reload the ACH information.
        /// </summary>
        /// <returns></returns>
        private ACHPaymentInfo ReloadACHInfo( PersistedPaymentInfo persistedPaymentInfo )
        {
            return new ACHPaymentInfo( persistedPaymentInfo.BankAccountNumber, persistedPaymentInfo.BankRoutingNumber, persistedPaymentInfo.AccountType );
        }

        /// <summary>
        /// Gets the payment schedule.
        /// </summary>
        /// <returns></returns>
        private PaymentSchedule GetSchedule()
        {
            if ( GetAttributeValue( "AllowScheduled" ).AsBoolean() )
            {
                var schedule = new PaymentSchedule();
                schedule.TransactionFrequencyValue = DefinedValueCache.Read( ddlScheduleFrequency.SelectedValue );
                if ( dpScheduleStartDate.SelectedDate.HasValue && dpScheduleStartDate.SelectedDate > RockDateTime.Today )
                {
                    schedule.StartDate = dpScheduleStartDate.SelectedDate.Value;
                }
                else
                {
                    schedule.StartDate = DateTime.MinValue;
                }

                return schedule;
            }

            return null;
        }

        /// <summary>
        /// Create a Persisted Payment Info object
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="paymentInfo"></param>
        /// <param name="fundAccountName"></param>
        /// <param name="fundAccountId"></param>
        /// <returns></returns>
        private PersistedPaymentInfo CreatePersistedPaymentInfo( FinancialTransaction transaction, PaymentInfo paymentInfo, string fundAccountName, int fundAccountId )
        {
            PersistedPaymentInfo persistedPaymentInfo = new PersistedPaymentInfo();

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                // Create Credit Card Payment Info
                CreditCardPaymentInfo creditCardInfo = paymentInfo as CreditCardPaymentInfo;

                persistedPaymentInfo.NameOnCard = creditCardInfo.NameOnCard;
                persistedPaymentInfo.CreditCardNumber = creditCardInfo.Number;
                persistedPaymentInfo.CVV = creditCardInfo.Code;
                persistedPaymentInfo.ExpirationDate = creditCardInfo.ExpirationDate;
                persistedPaymentInfo.Street = creditCardInfo.Street1;
                persistedPaymentInfo.City = creditCardInfo.City;
                persistedPaymentInfo.State = creditCardInfo.State;
                persistedPaymentInfo.PostalCode = creditCardInfo.PostalCode;
                persistedPaymentInfo.Country = creditCardInfo.Country;
            }
            else if ( paymentInfo is ACHPaymentInfo )
            {
                // Create ACH Payment Info
                ACHPaymentInfo ACHInfo = paymentInfo as ACHPaymentInfo;

                persistedPaymentInfo.BankAccountNumber = ACHInfo.BankAccountNumber;
                persistedPaymentInfo.BankRoutingNumber = ACHInfo.BankRoutingNumber;
                persistedPaymentInfo.AccountType = ACHInfo.AccountType;
            }
            else if ( paymentInfo is ReferencePaymentInfo )
            {
                // Create Reference Payment Info
                ReferencePaymentInfo referenceInfo = paymentInfo as ReferencePaymentInfo;

                persistedPaymentInfo.ReferenceNumber = referenceInfo.ReferenceNumber;
                persistedPaymentInfo.MaskedAccountNumber = referenceInfo.MaskedAccountNumber;
                persistedPaymentInfo.InitialCurrencyTypeValue = referenceInfo.InitialCurrencyTypeValue;
                persistedPaymentInfo.InitialCreditCardTypeValue = referenceInfo.InitialCreditCardTypeValue;
            }
            else
            {
                // invalid paymentInfo
                return null;
            }

            // Add Transaction information
            persistedPaymentInfo.TransactionCode = transaction.TransactionCode;
            persistedPaymentInfo.Amount = paymentInfo.Amount.ToString();
            persistedPaymentInfo.Email = paymentInfo.Email;
            persistedPaymentInfo.PersonId = _person.Id;
            persistedPaymentInfo.FundAccountId = fundAccountId;
            persistedPaymentInfo.FundAccountName = fundAccountName;

            return persistedPaymentInfo;
        }        

        /// <summary>
        /// Gets the default country.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultCountry()
        {
            var globalAttributesCache = GlobalAttributesCache.Read();
            string country = globalAttributesCache.OrganizationCountry;

            return string.IsNullOrWhiteSpace( country ) ? "US" : country;
        }
   
        /// <summary>
        /// Convert object into string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string ObjectToString( object obj )
        {
            using ( MemoryStream memoryStream = new MemoryStream() )
            {
                new BinaryFormatter().Serialize( memoryStream, obj );
                return Convert.ToBase64String( memoryStream.ToArray() );
            }
        }

        /// <summary>
        /// Convert string back into an object
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public object StringToObject( string base64String )
        {
            byte[] bytes = Convert.FromBase64String( base64String );
            using ( MemoryStream memoryStream = new MemoryStream( bytes, 0, bytes.Length ) )
            {
                memoryStream.Write( bytes, 0, bytes.Length );
                memoryStream.Position = 0;
                return new BinaryFormatter().Deserialize( memoryStream );
            }
        }

        /// <summary>
        /// Send transaction receipt
        /// </summary>
        /// <param name="transactionId"></param>
        private void SendReceipt( int transactionId )
        {
            Guid? receiptEmail = GetAttributeValue( "ReceiptEmail" ).AsGuidOrNull();
            if ( receiptEmail.HasValue )
            {
                // Queue a transaction to send receipts
                var newTransactionIds = new List<int> { transactionId };
                var sendPaymentReceiptsTxn = new Rock.Transactions.SendPaymentReceipts( receiptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentReceiptsTxn );
            }
        }

        /// <summary>
        /// Set a Panels "hidden" css class
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="state"></param>
        private void TogglePanel( string panelName, bool show)
        {
            // Find the panel ton update
            Panel panel = upPayment.FindControl( panelName ) as Panel;

            if (panel != null)
            {
                if ( show == true )
                {
                    panel.RemoveCssClass( "hidden" );
                }
                else
                {
                    panel.AddCssClass( "hidden" );
                }
            }
        }

        /// <summary>
        /// Set a progress indicators "active and "complete" css classes
        /// </summary>
        /// <param name="indicatorName"></param>
        /// <param name="active"></param>
        /// <param name="complete"></param>
        private void ToggleProgressIndicator ( string indicatorName, bool active, bool complete)
        {
            // Find the progress indicator button to update
            Button button = pnlTransaction.FindControl( indicatorName ) as Button;

            if ( button != null)
            {
                if ( active == true )
                {
                    button.AddCssClass( "active" );
                }
                else
                {
                    button.RemoveCssClass( "active" );
                }

                if ( complete == true )
                {
                    button.AddCssClass( "complete" );
                    button.Enabled = true;
                }
                else
                {
                    button.RemoveCssClass( "complete" );
                }
            }
        }

        /// <summary>
        /// Sets a buttons "btn-primary" css class
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="selected"></param>
        private void ToggleButtonSelectedState ( string buttonName, bool selected)
        {
            // Find the button
            Button button = pnlPayment.FindControl( buttonName ) as Button;

            if (button != null)
            {
                if (selected == true)
                {
                    button.AddCssClass( "btn-primary" );
                } else
                {
                    button.RemoveCssClass( "btn-primary" );
                }
            }
        }

        #endregion

        #region Helper Classes

        [Serializable]
        protected class PersistedPaymentInfo
        {
            // Credit Card Properties
            public string CreditCardNumber { get; set; }
            public string CVV { get; set; }
            public DateTime ExpirationDate { get; set; }
            public string NameOnCard { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }

            // ACH Properties
            public string BankAccountNumber { get; set; }
            public string BankRoutingNumber { get; set; }
            public BankAccountType AccountType { get; set; }

            // Reference Properties
            public string ReferenceNumber { get; set; }
            public string MaskedAccountNumber { get; set; }
            public DefinedValueCache InitialCurrencyTypeValue { get; set; }
            public DefinedValueCache InitialCreditCardTypeValue { get; set; }

            // Transaction Properties
            public string TransactionCode { get; set; }
            public string Amount { get; set; }
            public string Email { get; set; }
            public int PersonId { get; set; }
            public int FundAccountId { get; set; }
            public string FundAccountName { get; set; }

            public PersistedPaymentInfo()
            {
            }

        }
        
        /// <summary>
        /// Lightweight object for each contribution item
        /// </summary>
        [Serializable]
        [DotLiquid.LiquidType( "Id", "Order", "Name", "CampusId", "Amount", "PublicName", "AmountFormatted" )]
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
                    return Amount > 0 ? Amount.FormatAsCurrency() : string.Empty;
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

        #endregion
    }
}


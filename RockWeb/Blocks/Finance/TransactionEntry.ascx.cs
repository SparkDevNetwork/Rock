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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
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
    /// Add a new one-time or scheduled transaction
    /// </summary>
    [DisplayName( "Transaction Entry" )]
    [Category( "Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]
    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [FinancialGatewayField( "ACH Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving", "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false,
        Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 3 )]
    [BooleanField( "Impersonation", "Allow (only use on an internal page used by staff)", "Don't Allow",
        "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", false, "", 4 )]
    [CodeEditorField( "Account Header Template", "The Lava Template to use as the amount input label for each account", CodeEditorMode.Lava, CodeEditorTheme.Rock, 50, true, "{{ Account.PublicName }}", order: 3 )]
    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 7 )]
    [BooleanField( "Additional Accounts", "Display option for selecting additional accounts", "Don't display option",
        "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 8 )]
    [BooleanField( "Scheduled Transactions", "Allow", "Don't Allow",
        "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 9, "AllowScheduled" )]
    [BooleanField( "Prompt for Phone", "Should the user be prompted for their phone number?", false, "", 10, "DisplayPhone" )]
    [BooleanField( "Prompt for Email", "Should the user be prompted for their email address?", true, "", 11, "DisplayEmail" )]
    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 12 )]
    [SystemEmailField( "Confirm Account", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "Email Templates", 13, "ConfirmAccountTemplate" )]
    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "", 5 )]

    // Text Options

    [TextField( "Panel Title", "The text to display in panel heading", false, "Gifts", "Text Options", 14 )]
    [TextField( "Contribution Info Title", "The text to display as heading of section for selecting account and amount.", false, "Contribution Information", "Text Options", 15 )]
    [TextField( "Add Account Text", "The button text to display for adding an additional account", false, "Add Another Account", "Text Options", 16 )]
    [TextField( "Personal Info Title", "The text to display as heading of section for entering personal information.", false, "Personal Information", "Text Options", 17 )]
    [TextField( "Payment Info Title", "The text to display as heading of section for entering credit card or bank account information.", false, "Payment Information", "Text Options", 18 )]
    [TextField( "Confirmation Title", "The text to display as heading of section for confirming information entered.", false, "Confirm Information", "Text Options", 19 )]
    [CodeEditorField( "Confirmation Header", "The text (HTML) to display at the top of the confirmation section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
    Please confirm the information below. Once you have confirmed that the information is
    accurate click the 'Finish' button to complete your transaction.
</p>
", "Text Options", 20 )]
    [CodeEditorField( "Confirmation Footer", "The text (HTML) to display at the bottom of the confirmation section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<div class='alert alert-info'>
    By clicking the 'finish' button below I agree to allow {{ OrganizationName }}
    to transfer the amount above from my account. I acknowledge that I may
    update the transaction information at any time by returning to this website. Please
    call the Finance Office if you have any additional questions.
</div>
", "Text Options", 21 )]
    [TextField( "Success Title", "The text to display as heading of section for displaying details of gift.", false, "Gift Information", "Text Options", 22 )]
    [CodeEditorField( "Success Header", "The text (HTML) to display at the top of the success section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
    Thank you for your generous contribution.  Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively
    achieve our mission.  We are so grateful for your commitment.
</p>
", "Text Options", 23 )]
    [CodeEditorField( "Success Footer", "The text (HTML) to display at the bottom of the success section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, @"
", "Text Options", 24 )]
    [TextField( "Save Account Title", "The text to display as heading of section for saving payment information.", false, "Make Giving Even Easier", "Text Options", 25 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 26 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 27 )]
    [SystemEmailField( "Receipt Email", "The system email to use to send the receipt.", false, "", "Email Templates", 28 )]
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
TransactionAccountDetails: [
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
    [BooleanField( "Enable Comment Entry", "Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", false, "", 29 )]
    [TextField( "Comment Entry Label", "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", false, "Comment", "", 30 )]
    [BooleanField( "Enable Business Giving", "Should the option to give as a business be displayed", true, "", 31 )]
    [BooleanField( "Enable Anonymous Giving", "Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fundraising contributors.", false, "", 32 )]
    [TextField( "Anonymous Giving Tooltip", "The tooltip for the 'Give Anonymously' checkbox.", false, "", order: 33 )]

    #endregion

    #region Advanced Block Attributes

    [BooleanField( "Allow Account Options In URL", "Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true", false, "Advanced", key: "AllowAccountsInURL", order: 1 )]
    [BooleanField( "Only Public Accounts In URL", "Set to true if using the 'Allow Account Options In Url' option to prevent non-public accounts to be specified.", true, "Advanced", 2 )]
    [CodeEditorField( "Invalid Account Message", "Display this text (HTML) as an error alert if an invalid 'account' or 'glaccount' is passed through the URL.",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, "", "Advanced", 3 )]
    [CustomDropdownListField( "Account Campus Context", "Should any context be applied to the Account List", "-1^No Account Campus Context Filter Applied,0^Only Accounts with Current Campus Context,1^Accounts with No Campus and Current Campus Context", false, "-1", "Advanced", 4 )]
    [AttributeField( Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION, "Allowed Transaction Attributes From URL", "Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world", false, true, "", "Advanced", 5 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Type", "", true, false, Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION, "Advanced", order: 6 )]
    [EntityTypeField( "Transaction Entity Type", "The Entity Type for the Transaction Detail Record (usually left blank)", false, "Advanced", order: 7 )]
    [TextField( "Entity Id Param", "The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)", false, "", "Advanced", order: 8 )]
    [CodeEditorField( "Transaction Header", "The Lava template which will be displayed prior to the Amount entry", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "Advanced", order: 9 )]
    [BooleanField( "Enable Initial Back button", "Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry", false, "Advanced", order: 10 )]

    #endregion

    public partial class TransactionEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Person _targetPerson = null;
        private FinancialGateway _ccGateway;
        private GatewayComponent _ccGatewayComponent = null;
        private FinancialGateway _achGateway;
        private GatewayComponent _achGatewayComponent = null;
        private bool _using3StepGateway = false;
        private bool _gatewaysIncompatible = false;
        private string _ccSavedAccountFreqSupported = "both";
        private string _achSavedAccountFreqSupported = "both";
        protected bool FluidLayout = false;
        private List<ParameterAccount> _parameterAccounts = new List<ParameterAccount>();
        private bool _allowAccountsInUrl = false;
        private bool _onlyPublicAccountsInUrl = true;
        private int _accountCampusContextFilter = -1;
        private int _currentCampusContextId = -1;

        /// <summary>
        /// The scheduled transaction to be transferred.  This will get set if the
        /// page parameter "transfer" and the "ScheduledTransactionId" are passed in.
        /// </summary>
        private FinancialScheduledTransaction _scheduledTransactionToBeTransferred = null;

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
        protected int? ScheduleId
        {
            get { return ViewState["ScheduleId"] as int?; }
            set { ViewState["ScheduleId"] = value; }
        }

        // The URL for the Step-2 Iframe Url
        protected string Step2IFrameUrl { get; set; }

        protected bool DisplayPhone
        {
            get {return ViewState["DisplayPhone"].ToString().AsBoolean(); }
            set { ViewState["DisplayPhone"] = value; }
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

            _allowAccountsInUrl = GetAttributeValue( "AllowAccountsInURL" ).AsBoolean( false );
            _onlyPublicAccountsInUrl = GetAttributeValue( "OnlyPublicAccountsInURL" ).AsBoolean( true );

            // Add handler for page navigation
            RockPage page = Page as RockPage;
            if ( page != null )
            {
                page.PageNavigate += page_PageNavigate;
            }

            using ( var rockContext = new RockContext() )
            {
                SetTargetPerson( rockContext );
                SetGatewayOptions( rockContext );
                BindSavedAccounts( rockContext, true );
            }

            // Determine account campus context mode
            _accountCampusContextFilter = GetAttributeValue( "AccountCampusContext" ).AsType<int>();
            if ( _accountCampusContextFilter > -1 )
            {
                var campusEntity = RockPage.GetCurrentContext( EntityTypeCache.Get( typeof( Campus ) ) );
                if ( campusEntity != null )
                {
                    _currentCampusContextId = campusEntity.Id;
                }
            }

            // Determine account campus context mode
            _accountCampusContextFilter = GetAttributeValue( "AccountCampusContext" ).AsType<int>();
            if ( _accountCampusContextFilter > -1 )
            {
                var campusEntity = RockPage.GetCurrentContext( EntityTypeCache.Get( typeof( Campus ) ) );
                if ( campusEntity != null )
                {
                    _currentCampusContextId = campusEntity.Id;
                }
            }

            RegisterScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Hide the messages on every postback
            nbMessage.Visible = false;
            nbSelectionMessage.Visible = false;
            nbConfirmationMessage.Visible = false;
            nbConfirmationMessage.Visible = false;
            hfStep2AutoSubmit.Value = "false";

            pnlDupWarning.Visible = false;
            nbSaveAccount.Visible = false;

            if ( _allowAccountsInUrl )
            {
                string accountParameterType = string.Empty;
                using ( var rockContext = new RockContext() )
                {
                    if ( !string.IsNullOrWhiteSpace( PageParameter( "AccountIds" ) ) )
                    {
                        var accountIds = Server.UrlDecode( PageParameter( "AccountIds" ) );
                        var financialAccountService = new FinancialAccountService( rockContext );

                        accountParameterType = "invalid";

                        foreach ( string account in accountIds.Split( ',' ) )
                        {
                            var parameterAccount = new ParameterAccount();
                            var accountValues = account.Split( '^' );
                            var accountId = accountValues[0].AsInteger();

                            parameterAccount.Account = financialAccountService.Queryable()
                                .Where( a =>
                                    a.Id == accountId &&
                                    a.IsActive &&
                                    ( _onlyPublicAccountsInUrl ? ( a.IsPublic ?? false ) : true ) &&
                                    ( a.StartDate == null || a.StartDate <= RockDateTime.Today ) &&
                                    ( a.EndDate == null || a.EndDate >= RockDateTime.Today ) )
                                    .FirstOrDefault();

                            if ( parameterAccount.Account != null )
                            {
                                parameterAccount.Amount = accountValues.Length >= 2 ? accountValues[1].AsDecimal() : 0;
                                parameterAccount.Enabled = accountValues.Length >= 3 ? accountValues[2].AsBoolean( true ) : true;

                                _parameterAccounts.Add( parameterAccount );
                            }
                        }

                        if ( _parameterAccounts.Count > 0 )
                        {
                            accountParameterType = "valid";
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( PageParameter( "AccountGlCodes" ) ) )
                    {
                        var accountCodes = Server.UrlDecode( PageParameter( "AccountGlCodes" ) );
                        var financialAccountService = new FinancialAccountService( rockContext );

                        Dictionary<string, decimal> glAccountParameter = new Dictionary<string, decimal>();
                        accountParameterType = "invalid";

                        foreach ( string account in accountCodes.Split( ',' ) )
                        {
                            var parameterAccount = new ParameterAccount();
                            var accountValues = account.Split( '^' );
                            var accountGlCode = accountValues[0];

                            parameterAccount.Account = financialAccountService.Queryable()
                                .Where( a =>
                                    a.GlCode == accountGlCode &&
                                    a.IsActive &&
                                    ( _onlyPublicAccountsInUrl ? ( a.IsPublic ?? false ) : true ) &&
                                    ( a.StartDate == null || a.StartDate <= RockDateTime.Today ) &&
                                    ( a.EndDate == null || a.EndDate >= RockDateTime.Today ) )
                                    .FirstOrDefault();

                            if ( parameterAccount.Account != null )
                            {
                                parameterAccount.Amount = accountValues.Length >= 2 ? accountValues[1].AsDecimal() : 0;
                                parameterAccount.Enabled = accountValues.Length >= 3 ? accountValues[2].AsBoolean( true ) : true;

                                _parameterAccounts.Add( parameterAccount );
                            }
                        }

                        if ( _parameterAccounts.Count > 0 )
                        {
                            accountParameterType = "valid";
                        }
                    }
                }

                if ( accountParameterType == "invalid" && !string.IsNullOrEmpty( GetAttributeValue( "InvalidAccountMessage" ) ) )
                {
                    SetPage( 0 );
                    ShowMessage( NotificationBoxType.Danger, "Invalid Account Provided", GetAttributeValue( "InvalidAccountMessage" ) );
                    return;
                }
            }

            if ( _ccGateway == null && _achGateway == null )
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Please check the configuration of this block and make sure a valid Credit Card and/or ACH Financial Gateway has been selected." );
                return;
            }

            if ( _gatewaysIncompatible )
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "The Credit Card and ACH Gateways are incompatible. If using a three-step gateway, both the Credit Card and ACH Gateways need to be the same." );
                return;
            }

            if ( ( _ccGatewayComponent is IHostedGatewayComponent ) || _achGatewayComponent is IHostedGatewayComponent )
            {
                SetPage( 0 );
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Unsupported Gateway. This block does not support Gateways that have a hosted payment interface." );
                return;
            }

            var testGatewayGuid = Rock.SystemGuid.EntityType.FINANCIAL_GATEWAY_TEST_GATEWAY.AsGuid();
            if ( ( _ccGatewayComponent != null && _ccGatewayComponent.TypeGuid == testGatewayGuid ) ||
                ( _achGatewayComponent != null && _achGatewayComponent.TypeGuid == testGatewayGuid ) )
            {
                ShowMessage( NotificationBoxType.Warning, "Testing", "You are using the Test Financial Gateway. No actual amounts will be charged to your card or bank account." );
            }

            // Check if this is a transfer and that the person is the authorized person on the transaction
            if ( !string.IsNullOrWhiteSpace( PageParameter( "transfer" ) ) && !string.IsNullOrWhiteSpace( PageParameter( "ScheduledTransactionId" ) ) )
            {
                InitializeTransfer( PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull() );
            }

            if ( !Page.IsPostBack )
            {
                hfTransactionGuid.Value = Guid.NewGuid().ToString();
                if ( this.Request.UrlReferrer != null )
                {
                    lHistoryBackButton.HRef = this.Request.UrlReferrer.ToString();
                }
                else
                {
                    lHistoryBackButton.HRef = "#";
                }

                SetControlOptions();

                if ( _scheduledTransactionToBeTransferred != null )
                {
                    // Was this NOT a personal gift? If so, we need to set the correct business in the Give As section.
                    if ( _scheduledTransactionToBeTransferred.AuthorizedPersonAlias.Person.GivingId != _targetPerson.GivingId )
                    {
                        tglGiveAsOption.Checked = false;
                        SetGiveAsOptions();
                        ShowBusiness();
                    }
                }

                SetPage( 1 );

                // If an invalid PersonToken was specified, hide everything except for the error message
                if ( nbInvalidPersonWarning.Visible )
                {
                    pnlSelection.Visible = false;
                }

                // Get the list of accounts that can be used
                GetAccounts();
                BindAccounts();
            }
            else
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
                            else
                            {
                                SelectedAccounts[item.ItemIndex].Amount = default( decimal );
                            }
                        }
                    }
                }
            }

            // Update the total amount
            lblTotalAmount.Text = GlobalAttributesCache.Value("CurrencySymbol") + SelectedAccounts.Sum( f => f.Amount ).ToString( "F2" );

            // Set the frequency date label based on if 'One Time' is selected or not
            if ( btnFrequency.Items.Count > 0 )
            {
                dtpStartDate.Label = btnFrequency.Items[0].Selected ? "When" : "First Gift";
                if ( _scheduledTransactionToBeTransferred != null && _scheduledTransactionToBeTransferred.NextPaymentDate.HasValue )
                {
                    dtpStartDate.Label = "Next Gift";
                }
            }

            // Show or Hide the Credit card entry panel based on if a saved account exists and it's selected or not.
            divNewPayment.Style[HtmlTextWriterStyle.Display] = ( rblSavedAccount.Items.Count == 0 || rblSavedAccount.Items[rblSavedAccount.Items.Count - 1].Selected ) ? "block" : "none";

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

            // Show billing address based on if billing address checkbox is checked
            divBillingAddress.Style[HtmlTextWriterStyle.Display] = cbBillingAddress.Checked ? "block" : "none";

            // Show save account info based on if checkbox is checked
            divSaveAccount.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";

            ResolveHeaderFooterTemplates();
        }

        #endregion

        #region Events

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

        protected void btnFrequency_SelectionChanged( object sender, EventArgs e )
        {
            int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
            bool oneTime = ( btnFrequency.SelectedValueAsInt() ?? 0 ) == oneTimeFrequencyId;

            dtpStartDate.Label = oneTime ? "When" : "First Gift";

            if ( !oneTime && ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate.Value.Date <= RockDateTime.Today ) )
            {
                dtpStartDate.SelectedDate = RockDateTime.Today.AddDays( 1 );
            }

            if ( oneTime && dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate.Value.Date != RockDateTime.Today )
            {
                // A future "one-time" transaction is not really a one-time transaction. It's processed as a scheduled transaction
                oneTime = false;
            }

            using ( var rockContext = new RockContext() )
            {
                BindSavedAccounts( rockContext, oneTime );
            }

            SetPage( 1 );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglGiveAsOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglGiveAsOption_CheckedChanged( object sender, EventArgs e )
        {
            SetGiveAsOptions();
            if ( tglGiveAsOption.Checked )
            {
                ShowPersonal( GetPerson( false ) );
            }
            else
            {
                ShowBusiness();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblBusinessOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblBusinessOption_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowBusiness();
        }

        /// <summary>
        /// Handles the Click event of the btnPaymentInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPaymentInfoNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;
            if ( ProcessPaymentInfo( out errorMessage ) )
            {
                if ( _using3StepGateway )
                {
                    if ( ProcessStep1( out errorMessage ) )
                    {
                        this.AddHistory( "GivingDetail", "1", null );
                        if ( rblSavedAccount.Items.Count > 0 && ( rblSavedAccount.SelectedValueAsId() ?? 0 ) > 0 )
                        {
                            hfStep2AutoSubmit.Value = "true";
                        }

                        SetPage( 2 );
                        lbStep2Return.Focus();
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Danger, "Before we finish...", errorMessage );
                    }
                }
                else
                {
                    this.AddHistory( "GivingDetail", "1", null );
                    SetPage( 3 );
                    pnlConfirmation.Focus();
                }
            }
            else
            {
                ShowMessage( NotificationBoxType.Danger, "Before we finish...", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStep2Payment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStep2PaymentPrev_Click( object sender, EventArgs e )
        {
            this.AddHistory( "GivingDetail", "2", null );
            SetPage( 1 );
            pnlSelection.Focus();
        }

        /// <summary>
        /// Handles the Click event of the lbStep2Return control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbStep2Return_Click( object sender, EventArgs e )
        {
            PaymentInfo paymentInfo = GetPaymentInfo();
            tdPaymentMethodConfirm.Description = paymentInfo.CurrencyTypeValue.Description;
            tdAccountNumberConfirm.Description = paymentInfo.MaskedNumber;
            tdAccountNumberConfirm.Visible = !string.IsNullOrWhiteSpace( paymentInfo.MaskedNumber );

            SetPage( 3 );
            pnlConfirmation.Focus();
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmationPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmationPrev_Click( object sender, EventArgs e )
        {
            SetPage( 1 );
            pnlSelection.Focus();
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmationNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmationNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;
            if ( _using3StepGateway )
            {
                string resultQueryString = hfStep2ReturnQueryString.Value;
                if ( ProcessStep3( resultQueryString, out errorMessage ) )
                {
                    this.AddHistory( "GivingDetail", "3", null );
                    SetPage( 4 );
                    pnlSuccess.Focus();
                }
                else
                {
                    ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
                }
            }
            else
            {
                if ( ProcessConfirmation( out errorMessage ) )
                {
                    this.AddHistory( "GivingDetail", "2", null );
                    SetPage( 4 );
                    pnlSuccess.Focus();
                }
                else
                {
                    ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            // They are hitting Confirm on the "Possible Duplicate" warning, so reset the TransactionCode and Transaction.Guid which would have preventing them from doing a duplicate
            TransactionCode = string.Empty;
            hfTransactionGuid.Value = Guid.NewGuid().ToString();

            string errorMessage = string.Empty;
            if ( ProcessConfirmation( out errorMessage ) )
            {
                SetPage( 4 );
                pnlSuccess.Focus();
            }
            else
            {
                ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( phCreateLogin.Visible )
                {
                    if ( string.IsNullOrWhiteSpace( txtUserName.Text ) || string.IsNullOrWhiteSpace( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = "Missing Information";
                        nbSaveAccount.Text = "A username and password are required when saving an account";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                    {
                        nbSaveAccount.Title = "Invalid Username";
                        nbSaveAccount.Text = "The selected Username is already being used.  Please select a different Username";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( !UserLoginService.IsPasswordValid( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = string.Empty;
                        nbSaveAccount.Text = UserLoginService.FriendlyPasswordRules();
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( txtPasswordConfirm.Text != txtPassword.Text )
                    {
                        nbSaveAccount.Title = "Invalid Password";
                        nbSaveAccount.Text = "The password and password confirmation do not match";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
                {
                    bool isACHTxn = hfPaymentTab.Value == "ACH";
                    var financialGateway = isACHTxn ? _achGateway : _ccGateway;
                    var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        var achCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );

                        string errorMessage = string.Empty;

                        var person = GetPerson( false );
                        string referenceNumber = string.Empty;
                        FinancialPaymentDetail paymentDetail = null;
                        int? currencyTypeValueId = isACHTxn ? achCurrencyType.Id : ccCurrencyType.Id;

                        if ( !ScheduleId.HasValue )
                        {
                            var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( (financialGateway != null ? financialGateway.Id : (int?)null), TransactionCode );
                            if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                            {
                                if ( transaction.FinancialGateway != null )
                                {
                                    transaction.FinancialGateway.LoadAttributes( rockContext );
                                }
                                referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                                paymentDetail = transaction.FinancialPaymentDetail;
                            }
                        }
                        else
                        {
                            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( ScheduleId.Value );
                            if ( scheduledTransaction != null )
                            {
                                if ( scheduledTransaction.FinancialGateway != null )
                                {
                                    scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                                }
                                referenceNumber = gateway.GetReferenceNumber( scheduledTransaction, out errorMessage );
                                paymentDetail = scheduledTransaction.FinancialPaymentDetail;
                            }
                        }

                        if ( person != null && paymentDetail != null )
                        {
                            if ( phCreateLogin.Visible )
                            {
                                var user = UserLoginService.Create(
                                    rockContext,
                                    person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    txtUserName.Text,
                                    txtPassword.Text,
                                    false );

                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                                mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                                mergeFields.Add( "Person", person );
                                mergeFields.Add( "User", user );

                                var emailMessage = new RockEmailMessage( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid() );
                                emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                                emailMessage.CreateCommunicationRecord = false;
                                emailMessage.Send();
                            }

                            if ( errorMessage.Any() )
                            {
                                nbSaveAccount.Title = "Invalid Transaction";
                                nbSaveAccount.Text = "Sorry, the account information cannot be saved. " + errorMessage;
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                                nbSaveAccount.Visible = true;
                            }
                            else
                            {
                                var savedAccount = new FinancialPersonSavedAccount();
                                savedAccount.PersonAliasId = person.PrimaryAliasId;
                                savedAccount.ReferenceNumber = referenceNumber;
                                savedAccount.Name = txtSaveAccount.Text;
                                savedAccount.TransactionCode = TransactionCode;
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

                                cbSaveAccount.Visible = false;
                                txtSaveAccount.Visible = false;
                                phCreateLogin.Visible = false;
                                divSaveActions.Visible = false;

                                nbSaveAccount.Title = "Success";
                                nbSaveAccount.Text = "The account has been saved for future use";
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
                                nbSaveAccount.Visible = true;
                            }
                        }
                        else
                        {
                            nbSaveAccount.Title = "Invalid Transaction";
                            nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference.";
                            nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                            nbSaveAccount.Visible = true;
                        }
                    }
                    else
                    {
                        nbSaveAccount.Title = "Invalid Gateway";
                        nbSaveAccount.Text = "Sorry, the financial gateway information for this type of transaction is not valid.";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                    }
                }
                else
                {
                    nbSaveAccount.Title = "Missing Account Name";
                    nbSaveAccount.Text = "Please enter a name to use for this account.";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                    nbSaveAccount.Visible = true;
                }
            }
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Gets the transaction entity.
        /// </summary>
        /// <returns></returns>
        private IEntity GetTransactionEntity()
        {
            IEntity transactionEntity = null;
            Guid? transactionEntityTypeGuid = GetAttributeValue( "TransactionEntityType" ).AsGuidOrNull();
            if ( transactionEntityTypeGuid.HasValue )
            {
                var transactionEntityType = EntityTypeCache.Get( transactionEntityTypeGuid.Value );
                if ( transactionEntityType != null )
                {
                    var entityId = this.PageParameter( this.GetAttributeValue( "EntityIdParam" ) ).AsIntegerOrNull();
                    if ( entityId.HasValue )
                    {
                        var dbContext = Reflection.GetDbContextForEntityType( transactionEntityType.GetEntityType() );
                        IService serviceInstance = Reflection.GetServiceForEntityType( transactionEntityType.GetEntityType(), dbContext );
                        if ( serviceInstance != null )
                        {
                            System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                            transactionEntity = getMethod.Invoke( serviceInstance, new object[] { entityId.Value } ) as Rock.Data.IEntity;
                        }
                    }
                }
            }

            return transactionEntity;
        }

        private void SetTargetPerson( RockContext rockContext )
        {
            // If impersonation is allowed, and a valid person key was used, set the target to that person
            if ( GetAttributeValue( "Impersonation" ).AsBooleanOrNull() ?? false )
            {
                string personKey = PageParameter( "Person" );
                if ( !string.IsNullOrWhiteSpace( personKey ) )
                {
                    var incrementKeyUsage = !this.IsPostBack;
                    _targetPerson = new PersonService( rockContext ).GetByImpersonationToken( personKey, incrementKeyUsage, this.PageCache.Id );

                    if ( _targetPerson == null )
                    {
                        nbInvalidPersonWarning.Text = "Invalid or Expired Person Token specified";
                        nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                        nbInvalidPersonWarning.Visible = true;
                        return;
                    }
                }
            }

            if ( _targetPerson == null )
            {
                _targetPerson = CurrentPerson;
            }
        }

        private void SetGatewayOptions( RockContext rockContext )
        {
            _ccGateway = GetGateway( rockContext, "CCGateway" );
            _ccGatewayComponent = GetGatewayComponent( rockContext, _ccGateway );
            bool ccEnabled = _ccGatewayComponent != null;

            _achGateway = GetGateway( rockContext, "ACHGateway" );
            _achGatewayComponent = GetGatewayComponent( rockContext, _achGateway );
            bool achEnabled = _achGatewayComponent != null;

            if ( _using3StepGateway && _ccGateway != null && _achGateway != null && _ccGateway.Id != _achGateway.Id )
            {
                _gatewaysIncompatible = true;
            }

            _ccSavedAccountFreqSupported = GetSavedAcccountFreqSupported( _ccGatewayComponent );
            _achSavedAccountFreqSupported = GetSavedAcccountFreqSupported( _achGatewayComponent );

            bool allowScheduled = GetAttributeValue( "AllowScheduled" ).AsBoolean();
            if ( allowScheduled && ( ccEnabled || achEnabled ) )
            {
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
                    btnFrequency.DataSource = supportedFrequencies;
                    btnFrequency.DataBind();

                    // If gateway didn't specifically support one-time, add it anyway for immediate gifts
                    var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                    if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
                    {
                        btnFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Value, oneTimeFrequency.Id.ToString() ) );
                    }

                    btnFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
                    dtpStartDate.SelectedDate = RockDateTime.Today;

                    if ( !string.IsNullOrWhiteSpace( PageParameter( "StartDate" ) ) )
                    {
                        dtpStartDate.SelectedDate = PageParameter( "StartDate" ).AsDateTime() ?? RockDateTime.Today;
                        if ( dtpStartDate.SelectedDate < RockDateTime.Today )
                        {
                            dtpStartDate.SelectedDate = RockDateTime.Today;
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( PageParameter( "Frequency" ) ) )
                    {
                        var frequencyValues = PageParameter( "Frequency" ).Split( new char[] { '^' } );
                        if ( btnFrequency.Items.FindByValue( frequencyValues[0] ) != null )
                        {
                            btnFrequency.SelectedValue = frequencyValues[0];
                            if ( frequencyValues.Length >= 2 && frequencyValues[1].AsBoolean( true ) == false )
                            {
                                btnFrequency.Visible = false;
                                txtFrequency.Visible = true;
                                txtFrequency.Text = btnFrequency.SelectedItem.Text;
                            }
                        }
                    }
                }
            }
        }

        private string GetSavedAcccountFreqSupported( GatewayComponent component )
        {
            if ( component != null )
            {
                if ( component.SupportsSavedAccount( true ) )
                {
                    if ( component.SupportsSavedAccount( false ) )
                    {
                        return "both";
                    }
                    else
                    {
                        return "repeating";
                    }
                }
                else
                {
                    if ( component.SupportsSavedAccount( false ) )
                    {
                        return "onetime";
                    }
                }
            }

            return "none";
        }

        private FinancialGateway GetGateway( RockContext rockContext, string attributeName )
        {
            var financialGatewayService = new FinancialGatewayService( rockContext );
            Guid? gatewayGuid = GetAttributeValue( attributeName ).AsGuidOrNull();
            if ( gatewayGuid.HasValue )
            {
                return financialGatewayService.Get( gatewayGuid.Value );
            }
            return null;
        }

        private GatewayComponent GetGatewayComponent( RockContext rockContext, FinancialGateway gateway )
        {
            if ( gateway != null )
            {
                gateway.LoadAttributes( rockContext );
                var gatewayComponent = gateway.GetGatewayComponent();
                if ( gatewayComponent != null )
                {
                    var threeStepGateway = gatewayComponent as IThreeStepGatewayComponent;
                    if ( threeStepGateway != null )
                    {
                        _using3StepGateway = true;
                        Step2IFrameUrl = ResolveRockUrl( threeStepGateway.Step2FormUrl );
                    }
                }

                return gatewayComponent;
            }
            return null;
        }

        /// <summary>
        /// Binds the saved accounts.
        /// </summary>
        private void BindSavedAccounts( RockContext rockContext, bool oneTime )
        {
            rblSavedAccount.Items.Clear();

            if ( _targetPerson != null )
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService( rockContext )
                    .GetByPersonId( _targetPerson.Id )
                    .Where( a => !a.IsSystem )
                    .ToList();

                // Find the saved accounts that are valid for the selected CC gateway
                var ccSavedAccountIds = new List<int>();
                var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
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

                // Find the saved accounts that are valid for the selected ACH gateway
                var achSavedAccountIds = new List<int>();
                var achCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );
                if ( _achGateway != null &&
                    _achGatewayComponent != null &&
                    _achGatewayComponent.SupportsSavedAccount( !oneTime ) &&
                    _achGatewayComponent.SupportsSavedAccount( achCurrencyType ) )
                {
                    achSavedAccountIds = savedAccounts
                        .Where( a =>
                            a.FinancialGatewayId == _achGateway.Id &&
                            a.FinancialPaymentDetail != null &&
                            a.FinancialPaymentDetail.CurrencyTypeValueId == achCurrencyType.Id )
                        .Select( a => a.Id )
                        .ToList();
                }

                // Bind the accounts
                rblSavedAccount.DataSource = savedAccounts
                    .Where( a =>
                        ccSavedAccountIds.Contains( a.Id ) ||
                        achSavedAccountIds.Contains( a.Id ) )
                    .OrderBy( a => a.Name )
                    .Select( a => new
                    {
                        Id = a.Id,
                        Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")"
                    } ).ToList();
                rblSavedAccount.DataBind();
                if ( rblSavedAccount.Items.Count > 0 )
                {
                    rblSavedAccount.Items.Add( new ListItem( "Use a different payment method", "0" ) );
                    if ( rblSavedAccount.SelectedValue == "" )
                    {
                        rblSavedAccount.Items[0].Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the lava merge fields for the various header and footer templates.
        /// </summary>
        private void ResolveHeaderFooterTemplates()
        {
            // Resolve the text field merge fields
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage );

            using ( var rockContext = new RockContext() )
            {
                IEntity transactionEntity = GetTransactionEntity();
                if ( transactionEntity != null )
                {
                    mergeFields.Add( "TransactionEntity", transactionEntity );
                    var transactionEntityTypeId = transactionEntity.TypeId;

                    // include any Transactions that are associated with the TransactionEntity for Lava
                    var transactionEntityTransactions = new FinancialTransactionService( rockContext ).Queryable()
                        .Include( a => a.TransactionDetails )
                        .Where( a => a.TransactionDetails.Any( d => d.EntityTypeId.HasValue && d.EntityTypeId == transactionEntityTypeId && d.EntityId == transactionEntity.Id ) )
                        .ToList();

                    var transactionEntityTransactionsTotal = transactionEntityTransactions.SelectMany( d => d.TransactionDetails )
                        .Where( d => d.EntityTypeId.HasValue && d.EntityTypeId == transactionEntityTypeId && d.EntityId == transactionEntity.Id )
                        .Sum( d => ( decimal? ) d.Amount );

                    mergeFields.Add( "TransactionEntityTransactions", transactionEntityTransactions );
                    mergeFields.Add( "TransactionEntityTransactionsTotal", transactionEntityTransactionsTotal );
                }

                mergeFields.Add( "AmountLimit", this.PageParameter( "AmountLimit" ).AsDecimalOrNull() );

                if ( hfTransactionGuid.Value.AsGuidOrNull().HasValue )
                {
                    var financialTransaction = new FinancialTransactionService( rockContext ).Get( hfTransactionGuid.Value.AsGuid() );
                    mergeFields.Add( "FinancialTransaction", financialTransaction );
                }

                lTransactionHeader.Text = GetAttributeValue( "TransactionHeader" ).ResolveMergeFields( mergeFields );
                lConfirmationHeader.Text = GetAttributeValue( "ConfirmationHeader" ).ResolveMergeFields( mergeFields );
                lConfirmationFooter.Text = GetAttributeValue( "ConfirmationFooter" ).ResolveMergeFields( mergeFields );
                lSuccessHeader.Text = GetAttributeValue( "SuccessHeader" ).ResolveMergeFields( mergeFields );
                lSuccessFooter.Text = GetAttributeValue( "SuccessFooter" ).ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Sets the control options.
        /// </summary>
        private void SetControlOptions()
        {
            FluidLayout = GetAttributeValue( "LayoutStyle" ) == "Fluid";

            // Set page/panel titles
            lPanelTitle1.Text = GetAttributeValue( "PanelTitle" );
            lPanelTitle2.Text = GetAttributeValue( "PanelTitle" );
            lContributionInfoTitle.Text = GetAttributeValue( "ContributionInfoTitle" );
            lPersonalInfoTitle.Text = GetAttributeValue( "PersonalInfoTitle" );
            lPaymentInfoTitle.Text = GetAttributeValue( "PaymentInfoTitle" );
            lConfirmationTitle.Text = GetAttributeValue( "ConfirmationTitle" );
            lSuccessTitle.Text = GetAttributeValue( "SuccessTitle" );
            lSaveAcccountTitle.Text = GetAttributeValue( "SaveAccountTitle" );

            btnAddAccount.Title = GetAttributeValue( "AddAccountText" );

            divRepeatingPayments.Visible = btnFrequency.Items.Count > 0;

            bool displayEmail = GetAttributeValue( "DisplayEmail" ).AsBoolean();
            txtEmail.Visible = displayEmail;
            tdEmailConfirm.Visible = displayEmail;
            tdEmailReceipt.Visible = displayEmail;

            DisplayPhone = GetAttributeValue( "DisplayPhone" ).AsBoolean();
            pnbPhone.Visible = DisplayPhone;
            pnbBusinessContactPhone.Visible = DisplayPhone;
            tdPhoneConfirm.Visible = DisplayPhone;
            tdPhoneReceipt.Visible = DisplayPhone;

            var person = GetPerson( false );
            ShowPersonal( person );

            // Set personal display
            txtCurrentName.Visible = person != null;
            txtFirstName.Visible = person == null;
            txtLastName.Visible = person == null;

            cbGiveAnonymously.Visible = GetAttributeValue( "EnableAnonymousGiving" ).AsBoolean();
            cbGiveAnonymously.ToolTip = GetAttributeValue( "AnonymousGivingTooltip" );

            if ( GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() )
            {
                tglGiveAsOption.Checked = true;
                SetGiveAsOptions();
            }
            else
            {
                phGiveAsOption.Visible = false;
            }

            // Evaluate if comment entry box should be displayed
            txtCommentEntry.Label = GetAttributeValue( "CommentEntryLabel" );
            txtCommentEntry.Visible = GetAttributeValue( "EnableCommentEntry" ).AsBoolean();

            // Set the payment method tabs
            bool ccEnabled = _ccGatewayComponent != null;
            bool achEnabled = _achGatewayComponent != null;
            divCCPaymentInfo.Visible = ccEnabled;
            divACHPaymentInfo.Visible = achEnabled;
            if ( ccEnabled || achEnabled )
            {
                hfPaymentTab.Value = ccEnabled ? "CreditCard" : "ACH";
                if ( ccEnabled && achEnabled )
                {
                    phPills.Visible = true;
                }
            }

            // Determine if and how Name on Card should be displayed
            txtCardFirstName.Visible = _ccGatewayComponent != null && _ccGatewayComponent.PromptForNameOnCard( _ccGateway ) && _ccGatewayComponent.SplitNameOnCard;
            txtCardLastName.Visible = _ccGatewayComponent != null && _ccGatewayComponent.PromptForNameOnCard( _ccGateway ) && _ccGatewayComponent.SplitNameOnCard;
            txtCardName.Visible = _ccGatewayComponent != null && _ccGatewayComponent.PromptForNameOnCard( _ccGateway ) && !_ccGatewayComponent.SplitNameOnCard;

            // Set cc expiration min/max
            mypExpiration.MinimumYear = RockDateTime.Now.Year;
            mypExpiration.MaximumYear = mypExpiration.MinimumYear + 15;

            // Determine if account name should be displayed for bank account
            txtAccountName.Visible = _achGatewayComponent != null && _achGatewayComponent.PromptForBankAccountName( _achGateway );

            // Determine if billing address should be displayed
            cbBillingAddress.Visible = _ccGatewayComponent != null && _ccGatewayComponent.PromptForBillingAddress( _ccGateway );
            divBillingAddress.Visible = _ccGatewayComponent != null && _ccGatewayComponent.PromptForBillingAddress( _ccGateway );
        }

        #endregion

        #region Methods for the Payment Info Page (panel)

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        private void GetAccounts()
        {
            var rockContext = new RockContext();
            var selectedGuids = GetAttributeValues( "Accounts" ).Select( Guid.Parse ).ToList();
            bool showAll = !selectedGuids.Any();

            bool additionalAccounts = GetAttributeValue( "AdditionalAccounts" ).AsBoolean( true );

            SelectedAccounts = new List<AccountItem>();
            AvailableAccounts = new List<AccountItem>();

            // Limit selections to accounts passed through URL
            if ( _allowAccountsInUrl && _parameterAccounts.Count > 0 )
            {
                foreach ( var acct in _parameterAccounts )
                {
                    var accountItem = new AccountItem( acct.Account.Id, acct.Account.Order, acct.Account.Name, acct.Account.CampusId, acct.Account.PublicName, acct.Amount, acct.Enabled );
                    SelectedAccounts.Add( accountItem );
                }
            }
            else
            {
                // Enumerate through all active accounts that are public
                foreach ( var account in new FinancialAccountService( rockContext ).Queryable()
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
            }

            // Set account item *amounts* using the existing transaction
            if ( _scheduledTransactionToBeTransferred != null )
            {
                foreach ( var item in _scheduledTransactionToBeTransferred.ScheduledTransactionDetails )
                {
                    // Find a matching account
                    var account = SelectedAccounts.Where( a => a.Id == item.AccountId ).FirstOrDefault();

                    // if not in the selected list, try the available list
                    if ( account == null )
                    {
                        account = AvailableAccounts.Where( a => a.Id == item.AccountId ).FirstOrDefault();
                        if ( account != null )
                        {
                            AvailableAccounts = AvailableAccounts.Except( new List<AccountItem>() { account } ).ToList();
                            SelectedAccounts.AddRange( new List<AccountItem>() { account } );
                        }
                    }

                    // if still not found, just use the first account
                    if ( account == null )
                    {
                        account = SelectedAccounts.First();
                    }

                    account.Amount += item.Amount;
                }
            }
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            if ( _currentCampusContextId > -1 )
            {
                SelectedAccounts.RemoveAll( a => ( _accountCampusContextFilter == 0 && a.CampusId != _currentCampusContextId ) || ( _accountCampusContextFilter == 1 && ( a.CampusId != null && a.CampusId != _currentCampusContextId ) ) );
            }

            rptAccountList.DataSource = SelectedAccounts.ToList();
            rptAccountList.DataBind();

            lblTotalAmount.Visible = SelectedAccounts.Count > 1;
            lblTotalAmountLabel.Visible = lblTotalAmount.Visible;

            if ( _currentCampusContextId > -1 )
            {
                AvailableAccounts.RemoveAll( a => ( _accountCampusContextFilter == 0 && a.CampusId != _currentCampusContextId ) || ( _accountCampusContextFilter == 1 && ( a.CampusId != null && a.CampusId != _currentCampusContextId ) ) );
            }

            btnAddAccount.Visible = AvailableAccounts.Any();
            btnAddAccount.DataSource = AvailableAccounts;
            btnAddAccount.DataBind();
        }

        /// <summary>
        /// Sets the give as options.
        /// </summary>
        private void SetGiveAsOptions()
        {
            bool givingAsBusiness = GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() && !tglGiveAsOption.Checked;
            bool userLoggedIn = CurrentPerson != null;

            acAddress.Label = givingAsBusiness ? "Business Address" : "Address";
            pnbPhone.Label = givingAsBusiness ? "Business Phone" : "Phone";
            txtEmail.Label = givingAsBusiness ? "Business Email" : "Email";

            phGiveAsPerson.Visible = !givingAsBusiness;
            phGiveAsBusiness.Visible = givingAsBusiness;
            phBusinessContact.Visible = givingAsBusiness && !userLoggedIn;
            int contactPersonId = userLoggedIn ? CurrentPerson.Id : 0;

            if ( givingAsBusiness )
            {
                if ( hfBusinessesLoaded.Value != contactPersonId.ToString() )
                {
                    cblBusiness.Items.Clear();
                    using ( var rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var businesses = personService.GetBusinesses( contactPersonId ).ToList();
                        if ( businesses.Any() )
                        {
                            foreach ( var business in businesses )
                            {
                                cblBusiness.Items.Add( new ListItem( business.LastName, business.Id.ToString() ) );
                            }

                            cblBusiness.Items.Add( new ListItem( "New Business", "" ) );

                            cblBusiness.Visible = true;

                            if ( _scheduledTransactionToBeTransferred != null )
                            {
                                var matchBusiness = businesses.Where( b => b.Id == _scheduledTransactionToBeTransferred.AuthorizedPersonAlias.PersonId ).FirstOrDefault();
                                if ( matchBusiness != null )
                                {
                                    cblBusiness.SetValue( matchBusiness.Id.ToString() );
                                }
                            }
                            else
                            {
                                cblBusiness.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            cblBusiness.Visible = false;
                        }
                    }

                    hfBusinessesLoaded.Value = contactPersonId.ToString();
                }

                lPersonalInfoTitle.Text = "Business Information";
            }
            else
            {
                lPersonalInfoTitle.Text = GetAttributeValue( "PersonalInfoTitle" );
            }
        }

        private void ShowPersonal( Person person )
        {
            if ( person != null )
            {
                txtCurrentName.Text = person.FullName;
                txtEmail.Text = person.Email;

                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                if ( DisplayPhone )
                {
                    var phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );

                    // If person did not have a home phone number, read the cell phone number (which would then
                    // get saved as a home number also if they don't change it, which is ok ).
                    if ( phoneNumber == null || string.IsNullOrWhiteSpace( phoneNumber.Number ) || phoneNumber.IsUnlisted )
                    {
                        phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ) );
                    }

                    if ( phoneNumber != null )
                    {
                        if ( !phoneNumber.IsUnlisted )
                        {

                            pnbPhone.CountryCode = phoneNumber.CountryCode;
                            pnbPhone.Number = phoneNumber.ToString();
                        }
                        else
                        {
                            DisplayPhone = false;
                        }
                    }
                    else
                    {
                        pnbPhone.CountryCode = PhoneNumber.DefaultCountryCode();
                        pnbPhone.Number = string.Empty;
                    }
                }
                Guid addressTypeGuid = Guid.Empty;
                if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                {
                    addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                }

                var groupLocation = personService.GetFirstLocation( person.Id, DefinedValueCache.Get( addressTypeGuid ).Id );
                if ( groupLocation != null )
                {
                    GroupLocationId = groupLocation.Id;
                    acAddress.SetValues( groupLocation.Location );
                }
                else
                {
                    acAddress.SetValues( null );
                }
            }
            else
            {
                txtLastName.Text = string.Empty;
                txtFirstName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                pnbPhone.CountryCode = PhoneNumber.DefaultCountryCode();
                pnbPhone.Number = string.Empty;
                acAddress.SetValues( null );
            }
        }

        private void ShowBusiness()
        {
            int? businessId = cblBusiness.SelectedValueAsInt();
            if ( businessId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var business = personService.Get( businessId.Value );
                    ShowBusiness( personService, business );
                }
            }
            else
            {
                ShowBusiness( null, null );
            }
        }

        private void ShowBusiness( PersonService personService, Person business )
        {
            if ( personService != null && business != null )
            {
                txtBusinessName.Text = business.LastName;
                txtEmail.Text = business.Email;

                Guid addressTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
                var groupLocation = personService.GetFirstLocation( business.Id, DefinedValueCache.Get( addressTypeGuid ).Id );
                if ( groupLocation != null )
                {
                    GroupLocationId = groupLocation.Id;
                    acAddress.SetValues( groupLocation.Location );
                }
                else
                {
                    GroupLocationId = null;
                    acAddress.SetValues( null );
                }
            }
            else
            {
                txtBusinessName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                GroupLocationId = null;
                acAddress.SetValues( null );
            }

            txtBusinessContactFirstName.Text = string.Empty;
            txtBusinessContactLastName.Text = string.Empty;
            pnbBusinessContactPhone.Text = string.Empty;
            txtBusinessContactEmail.Text = string.Empty;
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        private Person GetPerson( bool create )
        {
            Person person = null;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            Group familyGroup = null;

            int personId = ViewState["PersonId"] as int? ?? 0;
            if ( personId == 0 && _targetPerson != null )
            {
                personId = _targetPerson.Id;
            }

            if ( personId != 0 )
            {
                person = personService.Get( personId );
            }

            bool givingAsBusiness = GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() && !tglGiveAsOption.Checked;
            if ( create && !givingAsBusiness )
            {
                if ( person == null )
                {
                    // Check to see if there's only one person with same email, first name, and last name
                    if ( !string.IsNullOrWhiteSpace( txtEmail.Text ) &&
                        !string.IsNullOrWhiteSpace( txtFirstName.Text ) &&
                        !string.IsNullOrWhiteSpace( txtLastName.Text ) )
                    {
                        // Same logic as PledgeEntry.ascx.cs
                        var personQuery = new PersonService.PersonMatchQuery( txtFirstName.Text, txtLastName.Text, txtEmail.Text, pnbPhone.Text.Trim());
                        person = personService.FindPerson( personQuery, true );
                    }

                    if ( person == null )
                    {
                        DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                        DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );

                        // Create Person
                        person = new Person();
                        person.FirstName = txtFirstName.Text;
                        person.LastName = txtLastName.Text;
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
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
            }

            if ( create && person != null ) // person should never be null at this point
            {
                person.Email = txtEmail.Text;

                if ( DisplayPhone )
                {
                    var numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
                    var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberTypeId );
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberTypeId;
                    }
                    phone.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    phone.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                }

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
                        familyGroup = person.GetFamily( rockContext );
                    }
                }

                rockContext.SaveChanges();

                if ( familyGroup != null )
                {
                    GroupService.AddNewGroupAddress(
                        rockContext,
                        familyGroup,
                        GetAttributeValue( "AddressType" ),
                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country,
                        true );
                }
            }

            return person;
        }

        /// <summary>
        /// Creates the business contact.
        /// </summary>
        /// <returns></returns>
        private Person GetBusinessContact()
        {
            Person person = null;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            // Check to see if there's only one person with same email, first name, and last name
            if ( !string.IsNullOrWhiteSpace( txtBusinessContactEmail.Text ) &&
                !string.IsNullOrWhiteSpace( txtBusinessContactFirstName.Text ) &&
                !string.IsNullOrWhiteSpace( txtBusinessContactLastName.Text ) )
            {
                // Find matching person. Intentionally not updating their primary email address as in this rare case it is likely to be their
                // business email which is more likely that they don't want updated
                person = personService.FindPerson( txtBusinessContactFirstName.Text, txtBusinessContactLastName.Text, txtBusinessContactEmail.Text, false );
            }

            if ( person == null )
            {
                DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );

                // Create Person
                person = new Person();
                person.FirstName = txtBusinessContactFirstName.Text;
                person.LastName = txtBusinessContactLastName.Text;
                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }

                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                // Create Person/Family
                PersonService.SaveNewPerson( person, rockContext, null, false );
            }

            if ( person != null ) // person should never be null at this point
            {
                person.Email = txtBusinessContactEmail.Text;

                if ( DisplayPhone )
                {
                    var numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                    var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberTypeId );
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberTypeId;
                    }
                    phone.CountryCode = PhoneNumber.CleanNumber( pnbBusinessContactPhone.CountryCode );
                    phone.Number = PhoneNumber.CleanNumber( pnbBusinessContactPhone.Number );
                }

                rockContext.SaveChanges();
            }

            return person;
        }

        private Person GetPersonOrBusiness( Person person )
        {
            bool givingAsBusiness = GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() && !tglGiveAsOption.Checked;
            if ( person != null && givingAsBusiness )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                Group familyGroup = null;

                Person business = null;
                int? businessId = cblBusiness.SelectedValueAsInt();
                if ( businessId.HasValue )
                {
                    business = personService.Get( businessId.Value );
                }

                if ( business == null )
                {
                    // Try to find existing business for person that has the same name
                    var personBusinesses = person.GetBusinesses()
                        .Where( b => b.LastName == txtBusinessName.Text )
                        .ToList();
                    if ( personBusinesses.Count() == 1 )
                    {
                        business = personBusinesses.First();
                    }
                }

                if ( business == null )
                {
                    DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                    DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );

                    // Create Person
                    business = new Person();
                    business.LastName = txtBusinessName.Text;
                    business.IsEmailActive = true;
                    business.EmailPreference = EmailPreference.EmailAllowed;
                    business.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                    if ( dvcConnectionStatus != null )
                    {
                        business.ConnectionStatusValueId = dvcConnectionStatus.Id;
                    }

                    if ( dvcRecordStatus != null )
                    {
                        business.RecordStatusValueId = dvcRecordStatus.Id;
                    }

                    // Create Person/Family
                    familyGroup = PersonService.SaveNewPerson( business, rockContext, null, false );

                    personService.AddContactToBusiness( business.Id, person.Id );
                    rockContext.SaveChanges();
                }

                business.LastName = txtBusinessName.Text;
                business.Email = txtEmail.Text;

                if ( DisplayPhone )
                {
                    var numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                    var phone = business.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberTypeId );
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        business.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberTypeId;
                    }
                    phone.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    phone.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                }

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
                        familyGroup = business.GetFamily( rockContext );
                    }
                }

                rockContext.SaveChanges();

                if ( familyGroup != null )
                {
                    GroupService.AddNewGroupAddress(
                        rockContext,
                        familyGroup,
                        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK,
                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country,
                        false );
                }

                return business;
            }

            return person;
        }

        /// <summary>
        /// Fetches the old (to be transferred) scheduled transaction and verifies
        /// that the target person is the same on the scheduled transaction.  Then
        /// it puts it into the _scheduledTransactionToBeTransferred private field
        /// for use throughout the entry process so that its values can be used on
        /// the form for the new transaction.
        /// </summary>
        /// <param name="scheduledTransactionId">The scheduled transaction identifier.</param>
        private void InitializeTransfer( int? scheduledTransactionId )
        {
            if ( scheduledTransactionId == null )
            {
                return;
            }

            RockContext rockContext = new RockContext();
            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( scheduledTransactionId.Value );
            var personService = new PersonService( rockContext );

            // get business giving id
            var givingIds = personService.GetBusinesses( _targetPerson.Id ).Select( g => g.GivingId ).ToList();

            // add the person's regular giving id
            givingIds.Add( _targetPerson.GivingId );

            // Make sure the current person is the authorized person, otherwise return
            if ( scheduledTransaction == null || !givingIds.Contains( scheduledTransaction.AuthorizedPersonAlias.Person.GivingId ) )
            {
                return;
            }

            _scheduledTransactionToBeTransferred = scheduledTransaction;

            // Set the frequency to be the same on the initial page build
            if ( !IsPostBack )
            {
                btnFrequency.SelectedValue = scheduledTransaction.TransactionFrequencyValueId.ToString();
                dtpStartDate.SelectedDate = ( scheduledTransaction.NextPaymentDate.HasValue ) ? scheduledTransaction.NextPaymentDate : RockDateTime.Today.AddDays( 1 );
            }
        }

        /// <summary>
        /// Processes the payment information.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPaymentInfo( out string errorMessage )
        {
            errorMessage = string.Empty;

            var errorMessages = new List<string>();

            bool givingAsBusiness = GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() && !tglGiveAsOption.Checked;

            // Validate that an amount was entered
            if ( SelectedAccounts.Sum( a => a.Amount ) <= 0 )
            {
                errorMessages.Add( "Make sure you've entered an amount for at least one account" );
            }

            var amountLimit = this.PageParameter( "AmountLimit" ).AsDecimalOrNull();
            if ( amountLimit.HasValue && SelectedAccounts.Sum( a => a.Amount ) > amountLimit.Value )
            {
                errorMessages.Add( string.Format( "The maximum amount it limited to {0}", amountLimit.FormatAsCurrency() ) );
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
                if ( schedule.StartDate <= RockDateTime.Today )
                {
                    errorMessages.Add( "When scheduling a repeating payment, make sure the First Gift date is in the future (after today)" );
                }
            }
            else
            {
                if ( dtpStartDate.SelectedDate < RockDateTime.Today )
                {
                    errorMessages.Add( "Make sure the date is not in the past" );
                }
            }

            if ( txtFirstName.Visible == true )
            {
                if ( string.IsNullOrWhiteSpace( txtFirstName.Text ) || string.IsNullOrWhiteSpace( txtLastName.Text ) )
                {
                    errorMessages.Add( "Make sure to enter both a first and last name" );
                }
            }

            if ( givingAsBusiness && string.IsNullOrWhiteSpace( txtBusinessName.Text ) )
            {
                errorMessages.Add( "Make sure to enter a Business Name" );
            }

            var location = new Location();
            acAddress.GetValues( location );
            if ( string.IsNullOrWhiteSpace( location.Street1 ) )
            {
                errorMessages.Add( "Make sure to enter a valid address.  An address is required for us to process this transaction" );
            }

            if ( DisplayPhone && string.IsNullOrWhiteSpace( pnbPhone.Number ) )
            {
                errorMessages.Add( "Make sure to enter a valid phone number.  A phone number is required for us to process this transaction" );
            }

            bool displayEmail = GetAttributeValue( "DisplayEmail" ).AsBoolean();
            if ( displayEmail && string.IsNullOrWhiteSpace( txtEmail.Text ) )
            {
                errorMessages.Add( "Make sure to enter a valid email address.  An email address is required for us to send you a payment confirmation" );
            }

            if ( givingAsBusiness && phBusinessContact.Visible )
            {
                if ( string.IsNullOrWhiteSpace( txtBusinessContactFirstName.Text ) || string.IsNullOrWhiteSpace( txtBusinessContactLastName.Text ) )
                {
                    errorMessages.Add( "Make sure to enter both a first and last name for Business Contact" );
                }
                if ( DisplayPhone && string.IsNullOrWhiteSpace( pnbBusinessContactPhone.Number ) )
                {
                    errorMessages.Add( "Make sure to enter a valid Business Contact phone number." );
                }

                if ( displayEmail && string.IsNullOrWhiteSpace( txtBusinessContactEmail.Text ) )
                {
                    errorMessages.Add( "Make sure to enter a valid Business Contact email address." );
                }
            }

            if ( !_using3StepGateway )
            {
                if ( rblSavedAccount.Items.Count <= 0 || ( rblSavedAccount.SelectedValueAsInt() ?? 0 ) <= 0 )
                {
                    bool isACHTxn = hfPaymentTab.Value == "ACH";
                    if ( isACHTxn )
                    {
                        // validate ach options
                        if ( string.IsNullOrWhiteSpace( txtRoutingNumber.Text ) )
                        {
                            errorMessages.Add( "Make sure to enter a valid routing number" );
                        }

                        if ( string.IsNullOrWhiteSpace( txtAccountNumber.Text ) )
                        {
                            errorMessages.Add( "Make sure to enter a valid account number" );
                        }
                    }
                    else
                    {
                        // validate cc options
                        if ( _ccGatewayComponent.PromptForNameOnCard( _ccGateway ) )
                        {
                            if ( _ccGatewayComponent != null && _ccGatewayComponent.SplitNameOnCard )
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
                        }

                        var rgx = new System.Text.RegularExpressions.Regex( @"[^\d]" );
                        string ccNum = rgx.Replace( txtCreditCard.Text, "" );
                        if ( string.IsNullOrWhiteSpace( ccNum ) )
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
            }

            if ( errorMessages.Any() )
            {
                errorMessage = errorMessages.AsDelimited( "<br/>" );
                return false;
            }

            PaymentInfo paymentInfo = GetPaymentInfo();

            // Set the payment type. This needs to be done since if a saved card was selected, the payment tab was not set in the UI and is still evaluated
            // to determine the correct gateway to use on other places.
            hfPaymentTab.Value = paymentInfo.CurrencyTypeValue.Guid == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ? "CreditCard" : "ACH";

            if ( !givingAsBusiness )
            {
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
            }
            else
            {
                paymentInfo.LastName = txtBusinessName.Text;
            }

            tdNameConfirm.Description = paymentInfo.FullName.Trim();
            tdPhoneConfirm.Description = paymentInfo.Phone;
            tdEmailConfirm.Description = paymentInfo.Email;
            tdAddressConfirm.Description = string.Format( "{0} {1}, {2} {3}", paymentInfo.Street1, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode );

            rptAccountListConfirmation.DataSource = SelectedAccounts.Where( a => a.Amount != 0 );
            rptAccountListConfirmation.DataBind();

            tdTotalConfirm.Description = paymentInfo.Amount.ToString( "C" );

            if ( !_using3StepGateway )
            {
                tdPaymentMethodConfirm.Description = paymentInfo.CurrencyTypeValue.Description;

                tdAccountNumberConfirm.Description = paymentInfo.MaskedNumber;
                tdAccountNumberConfirm.Visible = !string.IsNullOrWhiteSpace( paymentInfo.MaskedNumber );
            }

            tdWhenConfirm.Description = schedule != null ? schedule.ToString() : "Today";

            btnConfirmationPrev.Visible = !_using3StepGateway;

            return true;
        }

        /// <summary>
        /// Processes the step1.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessStep1( out string errorMessage )
        {
            var rockContext = new RockContext();

            bool isACHTxn = hfPaymentTab.Value == "ACH";
            var financialGateway = isACHTxn ? _achGateway : _ccGateway;
            var gateway = ( isACHTxn ? _achGatewayComponent : _ccGatewayComponent ) as IThreeStepGatewayComponent;

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
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
                    paymentInfo.Email = person.Email;
                }
            }
            else
            {
                paymentInfo.FirstName = txtFirstName.Text;
                paymentInfo.LastName = txtLastName.Text;
                paymentInfo.Email = txtEmail.Text;
            }

            paymentInfo.IPAddress = GetClientIpAddress();
            paymentInfo.AdditionalParameters = gateway.GetStep1Parameters( ResolveRockUrlIncludeRoot( "~/GatewayStep2Return.aspx" ) );

            string result = string.Empty;

            PaymentSchedule schedule = GetSchedule();
            if ( schedule != null )
            {
                result = gateway.AddScheduledPaymentStep1( financialGateway, schedule, paymentInfo, out errorMessage );
            }
            else
            {
                result = gateway.ChargeStep1( financialGateway, paymentInfo, out errorMessage );
            }

            if ( string.IsNullOrWhiteSpace( errorMessage ) && !string.IsNullOrWhiteSpace( result ) )
            {
                hfStep2Url.Value = result;
            }

            return string.IsNullOrWhiteSpace( errorMessage );
        }

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        /// <returns></returns>
        private PaymentInfo GetPaymentInfo()
        {
            PaymentInfo paymentInfo = null;
            if ( rblSavedAccount.Items.Count > 0 && ( rblSavedAccount.SelectedValueAsId() ?? 0 ) > 0 )
            {
                paymentInfo = GetReferenceInfo( rblSavedAccount.SelectedValueAsId().Value );
            }
            else
            {
                if ( hfPaymentTab.Value == "ACH" )
                {
                    paymentInfo = GetACHInfo();
                }
                else
                {
                    paymentInfo = GetCCInfo();
                }
            }

            paymentInfo.Amount = SelectedAccounts.Sum( a => a.Amount );
            paymentInfo.Email = txtEmail.Text;
            paymentInfo.Phone = PhoneNumber.FormattedNumber( pnbPhone.CountryCode, pnbPhone.Number, true );
            paymentInfo.Street1 = acAddress.Street1;
            paymentInfo.Street2 = acAddress.Street2;
            paymentInfo.City = acAddress.City;
            paymentInfo.State = acAddress.State;
            paymentInfo.PostalCode = acAddress.PostalCode;
            paymentInfo.Country = acAddress.Country;

            return paymentInfo;
        }

        /// <summary>
        /// Gets the credit card information.
        /// </summary>
        /// <returns></returns>
        private CreditCardPaymentInfo GetCCInfo()
        {
            var cc = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate ?? DateTime.MinValue );
            cc.NameOnCard = _ccGatewayComponent != null && _ccGatewayComponent.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            cc.LastNameOnCard = txtCardLastName.Text;

            if ( cbBillingAddress.Checked )
            {
                cc.BillingStreet1 = acBillingAddress.Street1;
                cc.BillingStreet2 = acBillingAddress.Street2;
                cc.BillingCity = acBillingAddress.City;
                cc.BillingState = acBillingAddress.State;
                cc.BillingPostalCode = acBillingAddress.PostalCode;
                cc.BillingCountry = acBillingAddress.Country;
            }
            else
            {
                cc.BillingStreet1 = acAddress.Street1;
                cc.BillingStreet2 = acAddress.Street2;
                cc.BillingCity = acAddress.City;
                cc.BillingState = acAddress.State;
                cc.BillingPostalCode = acAddress.PostalCode;
                cc.BillingCountry = acAddress.Country;
            }

            return cc;
        }

        /// <summary>
        /// Gets the ACH information.
        /// </summary>
        /// <returns></returns>
        private ACHPaymentInfo GetACHInfo()
        {
            return new ACHPaymentInfo( txtAccountNumber.Text, txtRoutingNumber.Text, rblAccountType.SelectedValue == "Savings" ? BankAccountType.Savings : BankAccountType.Checking );
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

        /// <summary>
        /// Gets the payment schedule.
        /// </summary>
        /// <returns></returns>
        private PaymentSchedule GetSchedule()
        {
            // Figure out if this is a one-time transaction or a future scheduled transaction
            if ( GetAttributeValue( "AllowScheduled" ).AsBoolean() )
            {
                // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
                int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() && dtpStartDate.SelectedDate <= RockDateTime.Today )
                {
                    // one-time immediate payment
                    return null;
                }

                var schedule = new PaymentSchedule();
                schedule.TransactionFrequencyValue = DefinedValueCache.Get( btnFrequency.SelectedValueAsId().Value );
                if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > RockDateTime.Today )
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

        /// <summary>
        /// Processes the confirmation.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessConfirmation( out string errorMessage )
        {
            var rockContext = new RockContext();
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                var transactionGuid = hfTransactionGuid.Value.AsGuid();

                bool isACHTxn = hfPaymentTab.Value == "ACH";
                var financialGateway = isACHTxn ? _achGateway : _ccGateway;
                var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;

                if ( gateway == null )
                {
                    errorMessage = "There was a problem creating the payment gateway information";
                    return false;
                }

                bool givingAsBusiness = GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() && !tglGiveAsOption.Checked;

                // only create/update the person if they are giving as a person. If they are giving as a Business, the person shouldn't be created this way
                Person person = GetPerson( !givingAsBusiness );

                // Add contact person if giving as a business and current person is unknow
                if ( person == null && givingAsBusiness )
                {
                    person = GetBusinessContact();
                }

                if ( person == null )
                {
                    errorMessage = "There was a problem creating the person information";
                    return false;
                }

                if ( !person.PrimaryAliasId.HasValue )
                {
                    errorMessage = "There was a problem creating the person's primary alias";
                    return false;
                }

                Person BusinessOrPerson = GetPersonOrBusiness( person );

                PaymentInfo paymentInfo = GetTxnPaymentInfo( BusinessOrPerson, out errorMessage );
                if ( paymentInfo == null )
                {
                    return false;
                }

                PaymentSchedule schedule = GetSchedule();
                FinancialPaymentDetail paymentDetail = null;
                if ( schedule != null )
                {
                    schedule.PersonId = person.Id;

                    var scheduledTransactionAlreadyExists = new FinancialScheduledTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                    if ( scheduledTransactionAlreadyExists != null )
                    {
                        // hopefully shouldn't happen, but just in case the scheduledtransaction already went thru, show the success screen
                        ShowSuccess( gateway, person, paymentInfo, schedule, scheduledTransactionAlreadyExists.FinancialPaymentDetail, rockContext );
                        return true;
                    }

                    var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                    if ( scheduledTransaction == null )
                    {
                        return false;
                    }

                    // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate scheduled transactions impossible
                    scheduledTransaction.Guid = transactionGuid;

                    SaveScheduledTransaction( financialGateway, gateway, BusinessOrPerson, paymentInfo, schedule, scheduledTransaction, rockContext );
                    paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                }
                else
                {
                    var transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                    if ( transactionAlreadyExists != null )
                    {
                        // hopefully shouldn't happen, but just in case the transaction already went thru, show the success screen
                        ShowSuccess( gateway, person, paymentInfo, null, transactionAlreadyExists.FinancialPaymentDetail, rockContext );
                        return true;
                    }

                    var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                    if ( transaction == null )
                    {
                        return false;
                    }

                    // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
                    transaction.Guid = transactionGuid;

                    SaveTransaction( financialGateway, gateway, BusinessOrPerson, paymentInfo, transaction, rockContext );
                    paymentDetail = transaction.FinancialPaymentDetail.Clone( false );
                }

                ShowSuccess( gateway, person, paymentInfo, schedule, paymentDetail, rockContext );

                return true;
            }
            else
            {
                pnlDupWarning.Visible = true;
                errorMessage = string.Empty;
                return false;
            }
        }

        private bool ProcessStep3( string resultQueryString, out string errorMessage )
        {
            var rockContext = new RockContext();

            var transactionGuid = hfTransactionGuid.Value.AsGuid();

            bool isACHTxn = hfPaymentTab.Value == "ACH";
            var financialGateway = isACHTxn ? _achGateway : _ccGateway;
            var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;
            var threeStepGateway = gateway as IThreeStepGatewayComponent;

            if ( threeStepGateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            // only create/update the person if they are giving as a person. If they are giving as a Business, the person record already exists
            bool givingAsBusiness = GetAttributeValue( "EnableBusinessGiving" ).AsBoolean() && !tglGiveAsOption.Checked;
            Person person = GetPerson( !givingAsBusiness );

            if ( person == null )
            {
                errorMessage = "There was a problem creating the person information";
                return false;
            }

            if ( !person.PrimaryAliasId.HasValue )
            {
                errorMessage = "There was a problem creating the person's primary alias";
                return false;
            }

            Person BusinessOrPerson = GetPersonOrBusiness( person );

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

            if ( paymentInfo.CreditCardTypeValue != null )
            {
                CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
            }

            if ( GetAttributeValue( "EnableCommentEntry" ).AsBoolean() )
            {
                paymentInfo.Comment1 = !string.IsNullOrWhiteSpace( GetAttributeValue( "PaymentComment" ) ) ? string.Format( "{0}: {1}", GetAttributeValue( "PaymentComment" ), txtCommentEntry.Text ) : txtCommentEntry.Text;
            }
            else
            {
                paymentInfo.Comment1 = GetAttributeValue( "PaymentComment" );
            }

            var transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
            if ( transactionAlreadyExists != null )
            {
                // hopefully shouldn't happen, but just in case the transaction already went thru, show the success screen
                ShowSuccess( gateway, person, paymentInfo, null, transactionAlreadyExists.FinancialPaymentDetail, rockContext );
                errorMessage = string.Empty;
                return true;
            }

            PaymentSchedule schedule = GetSchedule();
            FinancialPaymentDetail paymentDetail = null;
            if ( schedule != null )
            {
                var scheduledTransaction = threeStepGateway.AddScheduledPaymentStep3( financialGateway, resultQueryString, out errorMessage );
                if ( scheduledTransaction == null )
                {
                    return false;
                }

                paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                SaveScheduledTransaction( financialGateway, gateway, BusinessOrPerson, paymentInfo, schedule, scheduledTransaction, rockContext );
            }
            else
            {
                var transaction = threeStepGateway.ChargeStep3( financialGateway, resultQueryString, out errorMessage );
                if ( transaction == null || !string.IsNullOrWhiteSpace( errorMessage ) )
                {
                    return false;
                }

                // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
                transaction.Guid = transactionGuid;

                paymentDetail = transaction.FinancialPaymentDetail.Clone( false );
                SaveTransaction( financialGateway, gateway, BusinessOrPerson, paymentInfo, transaction, rockContext );
            }

            ShowSuccess( gateway, person, paymentInfo, schedule, paymentDetail, rockContext );

            errorMessage = string.Empty;
            return true;
        }

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

            if ( GetAttributeValue( "EnableCommentEntry" ).AsBoolean() )
            {
                paymentInfo.Comment1 = !string.IsNullOrWhiteSpace( paymentComment ) ? string.Format( "{0}: {1}", paymentComment, txtCommentEntry.Text ) : txtCommentEntry.Text;
            }
            else
            {
                paymentInfo.Comment1 = paymentComment;
            }

            errorMessage = string.Empty;
            return paymentInfo;
        }

        private void SaveScheduledTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, RockContext rockContext )
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
                var source = DefinedValueCache.Get( sourceGuid );
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

            var transactionEntity = this.GetTransactionEntity();

            foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
            {
                var transactionDetail = new FinancialScheduledTransactionDetail();
                transactionDetail.Amount = account.Amount;
                transactionDetail.AccountId = account.Id;

                if ( transactionEntity != null )
                {
                    transactionDetail.EntityTypeId = transactionEntity.TypeId;
                    transactionDetail.EntityId = transactionEntity.Id;
                }

                scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
                changeSummary.AppendFormat( "{0}: {1}", account.Name, account.Amount.FormatAsCurrency() );
                changeSummary.AppendLine();
            }

            if ( !string.IsNullOrWhiteSpace( paymentInfo.Comment1 ) )
            {
                changeSummary.Append( paymentInfo.Comment1 );
                changeSummary.AppendLine();
            }

            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();

            // If this is a transfer, now we can delete the old transaction
            if ( _scheduledTransactionToBeTransferred != null )
            {
                DeleteOldTransaction( _scheduledTransactionToBeTransferred.Id );
            }

            rockContext.SaveChanges();

            ScheduleId = scheduledTransaction.Id;
            TransactionCode = scheduledTransaction.TransactionCode;
        }

        private void DeleteOldTransaction( int scheduledTransactionId )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                FinancialScheduledTransactionService fstService = new FinancialScheduledTransactionService( rockContext );
                var currentTransaction = fstService.Get( scheduledTransactionId );
                if ( currentTransaction != null && currentTransaction.FinancialGateway != null )
                {
                    currentTransaction.FinancialGateway.LoadAttributes( rockContext );
                }
                string errorMessage = string.Empty;
                if ( fstService.Cancel( currentTransaction, out errorMessage ) )
                {
                    try
                    {
                        fstService.GetStatus( currentTransaction, out errorMessage );
                    }
                    catch { }
                    rockContext.SaveChanges();
                    //content.Text = String.Format( "<div class='alert alert-success'>Your recurring {0} has been deleted.</div>", GetAttributeValue( "TransactionLabel" ).ToLower() );
                }
                else
                {
                    //content.Text = String.Format( "<div class='alert alert-danger'>An error occurred while deleting your scheduled transaction. Message: {0}</div>", errorMessage );
                }
            }
        }

        private void SaveTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, FinancialTransaction transaction, RockContext rockContext )
        {
            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            transaction.ShowAsAnonymous = cbGiveAnonymously.Checked;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;

            var txnType = DefinedValueCache.Get( this.GetAttributeValue( "TransactionType" ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = txnType.Id;

            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }
            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                }
            }

            var transactionEntity = this.GetTransactionEntity();

            foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
            {
                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = account.Amount;
                transactionDetail.AccountId = account.Id;
                if ( transactionEntity != null )
                {
                    transactionDetail.EntityTypeId = transactionEntity.TypeId;
                    transactionDetail.EntityId = transactionEntity.Id;
                }

                transaction.TransactionDetails.Add( transactionDetail );
            }

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                GetAttributeValue( "BatchNamePrefix" ),
                paymentInfo.CurrencyTypeValue,
                paymentInfo.CreditCardTypeValue,
                transaction.TransactionDateTime.Value,
                financialGateway.GetBatchTimeOffset() );

            var batchChanges = new History.HistoryChangeList();

            if ( batch.Id == 0 )
            {
                batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
            }

            decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
            History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
            batch.ControlAmount = newControlAmount;

            transaction.BatchId = batch.Id;
            transaction.LoadAttributes( rockContext );

            var allowedTransactionAttributes = GetAttributeValue( "AllowedTransactionAttributesFromURL" ).Split( ',' ).AsGuidList().Select( x => AttributeCache.Get( x ).Key );

            foreach ( KeyValuePair<string, AttributeValueCache> attr in transaction.AttributeValues )
            {
                if ( PageParameters().ContainsKey( "Attribute_" + attr.Key ) && allowedTransactionAttributes.Contains( attr.Key ) )
                {
                    attr.Value.Value = Server.UrlDecode( PageParameter( "Attribute_" + attr.Key ) );
                }
            }

            var financialTransactionService = new FinancialTransactionService( rockContext );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add( transaction );

            rockContext.SaveChanges();
            transaction.SaveAttributeValues();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges
            );

            SendReceipt( transaction.Id );

            TransactionCode = transaction.TransactionCode;
        }

        private void ShowSuccess( GatewayComponent gatewayComponent, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialPaymentDetail paymentDetail, RockContext rockContext )
        {
            tdTransactionCodeReceipt.Description = TransactionCode;
            tdTransactionCodeReceipt.Visible = !string.IsNullOrWhiteSpace( TransactionCode );

            if ( ScheduleId.HasValue )
            {
                var scheduledTxn = new FinancialScheduledTransactionService( rockContext ).Get( ScheduleId.Value );
                if ( scheduledTxn != null && !string.IsNullOrWhiteSpace( scheduledTxn.GatewayScheduleId ) )
                {
                    tdScheduleId.Description = scheduledTxn.GatewayScheduleId;
                    tdScheduleId.Visible = true;
                }
                else
                {
                    tdScheduleId.Visible = false;
                }
            }
            else
            {
                tdScheduleId.Visible = false;
            }

            tdNameReceipt.Description = paymentInfo.FullName;
            tdPhoneReceipt.Description = paymentInfo.Phone;
            tdEmailReceipt.Description = paymentInfo.Email;
            tdAddressReceipt.Description = string.Format( "{0} {1}, {2} {3}", paymentInfo.Street1, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode );

            rptAccountListReceipt.DataSource = SelectedAccounts.Where( a => a.Amount != 0 );
            rptAccountListReceipt.DataBind();

            tdTotalReceipt.Description = paymentInfo.Amount.ToString( "C" );

            tdPaymentMethodReceipt.Description = paymentInfo.CurrencyTypeValue.Description;

            string acctNumber = paymentInfo.MaskedNumber;
            if ( string.IsNullOrWhiteSpace( acctNumber ) && paymentDetail != null && !string.IsNullOrWhiteSpace( paymentDetail.AccountNumberMasked ) )
            {
                acctNumber = paymentDetail.AccountNumberMasked;
            }
            tdAccountNumberReceipt.Description = acctNumber;
            tdAccountNumberReceipt.Visible = !string.IsNullOrWhiteSpace( acctNumber );

            tdWhenReceipt.Description = schedule != null ? schedule.ToString() : "Today";

            // If there was a transaction code returned and this was not already created from a previous saved account,
            // show the option to save the account.
            if ( !( paymentInfo is ReferencePaymentInfo ) && !string.IsNullOrWhiteSpace( TransactionCode ) && gatewayComponent.SupportsSavedAccount( paymentInfo.CurrencyTypeValue ) )
            {
                cbSaveAccount.Visible = true;
                pnlSaveAccount.Visible = true;
                txtSaveAccount.Visible = true;

                // If current person does not have a login, have them create a username and password
                phCreateLogin.Visible = !new UserLoginService( rockContext ).GetByPersonId( person.Id ).Any();
            }
            else
            {
                pnlSaveAccount.Visible = false;
            }

            // the merge fields for the header/footer includes the financialTransaction, so update them now that we have saved the transaction to the database
            ResolveHeaderFooterTemplates();
        }

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

        #endregion

        #region Methods used globally

        /// <summary>
        /// Sets the page.
        /// </summary>
        /// <param name="page">The page.</param>
        private void SetPage( int page )
        {
            // Page 0 = Only message box is displayed
            // Page 1 = Selection
            // Page 2 = Step 2 (of three-step charge)
            // Page 3 = Confirmation
            // Page 4 = Success

            pnlSelection.Visible = page == 1 || page == 2;
            pnlContributionInfo.Visible = page == 1;

            pnlPayment.Visible = true;
            rblSavedAccount.Visible = page == 1 && rblSavedAccount.Items.Count > 0;
            bool usingSavedAccount = rblSavedAccount.Items.Count > 0 && ( rblSavedAccount.SelectedValueAsId() ?? 0 ) > 0;
            divNewPayment.Visible = ( page == 1 && !_using3StepGateway ) || ( page == 2 && !usingSavedAccount );
            pnlPayment.Visible = rblSavedAccount.Visible || divNewPayment.Visible;

            // only show the History back button if the previous URL was able to be determined and they have the EnableInitialBackbutton enabled;
            lHistoryBackButton.Visible = GetAttributeValue( "EnableInitialBackbutton" ).AsBoolean() && lHistoryBackButton.HRef != "#" && page == 1;
            btnPaymentInfoNext.Visible = page == 1;
            btnStep2PaymentPrev.Visible = page == 2 && !usingSavedAccount;
            aStep2Submit.Visible = page == 2 && !usingSavedAccount;

            pnlConfirmation.Visible = page == 3;
            pnlSuccess.Visible = page == 4;

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
                NotificationBox nb = nbMessage;
                switch ( hfCurrentPage.Value.AsInteger() )
                {
                    case 1: nb = nbSelectionMessage; break;
                    case 2: nb = nbSelectionMessage; break;
                    case 3: nb = nbConfirmationMessage; break;
                    case 4: nb = nbSuccessMessage; break;
                }

                nb.Text = text;
                nb.Title = string.IsNullOrWhiteSpace( title ) ? "" : string.Format( "<p>{0}</p>", title );
                nb.NotificationBoxType = type;
                nb.Visible = true;
            }
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
            $('.total-amount').html('{3}' + totalAmt.toFixed(2));
            return false;
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

        if ( typeof {21} != 'undefined' ) {{
            //// Toggle credit card display if saved card option is available
            $('input[name=""{22}""]').change(function () {{

                var radioDisplay = $('#{23}').css('display');
                var selectedVal = $('input[name=""{22}""]:checked').val();

                if ( selectedVal == 0 && radioDisplay == 'none') {{
                    $('#{23}').slideDown();
                }}
                else if (selectedVal != 0 && radioDisplay != 'none') {{
                    $('#{23}').slideUp();
                }}
            }});
        }}

        // Hide or show a div based on selection of checkbox
        $('input:checkbox.toggle-input').unbind('click').on('click', function () {{
            $(this).parents('.checkbox').next('.toggle-content').slideToggle();
        }});

        // Disable the submit button as soon as it's clicked to prevent double-clicking
        $('a[id$=""btnNext""]').click(function() {{
            $(this).unbind('click');
            if (typeof (Page_ClientValidate) == 'function') {{
                if (Page_IsValid) {{
                    Page_ClientValidate();
                }}
            }}
            if (Page_IsValid) {{
			    $(this).addClass('disabled');
			    $(this).click(function () {{
				    return false;
			    }});
            }}
        }});
    }});

    // Posts the iframe (step 2)
    $('#aStep2Submit').on('click', function(e) {{
        e.preventDefault();
        if (typeof (Page_ClientValidate) == 'function') {{
            if (Page_IsValid && Page_ClientValidate('{7}') ) {{
                $(this).prop('disabled', true);
                $('#updateProgress').show();
                var src = $('#{4}').val();
                var $form = $('#iframeStep2').contents().find('#Step2Form');

                if ( $('#{16}').is(':visible') && $('#{16}').prop('checked') ) {{
                    $form.find('.js-billing-address1').val( $('#{17}_tbStreet1').val() );
                    $form.find('.js-billing-city').val( $('#{17}_tbCity').val() );
                    if ( $('#{17}_ddlState').length ) {{
                        $form.find('.js-billing-state').val( $('#{17}_ddlState').val() );
                    }} else {{
                        $form.find('.js-billing-state').val( $('#{17}_tbState').val() );
                    }}
                    $form.find('.js-billing-postal').val( $('#{17}_tbPostalCode').val() );
                    $form.find('.js-billing-country').val( $('#{17}_ddlCountry').val() );
                }}

                if ( $('#{1}').val() == 'CreditCard' ) {{
                    $form.find('.js-cc-first-name').val( $('#{18}').val() );
                    $form.find('.js-cc-last-name').val( $('#{19}').val() );
                    $form.find('.js-cc-full-name').val( $('#{20}').val() );
                    $form.find('.js-cc-number').val( $('#{8}').val() );
                    var mm = $('#{9}_monthDropDownList').val();
                    var yy = $('#{9}_yearDropDownList_').val();
                    mm = mm.length == 1 ? '0' + mm : mm;
                    yy = yy.length == 4 ? yy.substring(2,4) : yy;
                    $form.find('.js-cc-expiration').val( mm + yy );
                    $form.find('.js-cc-cvv').val( $('#{10}').val() );
                }} else {{
                    $form.find('.js-account-name').val( $('#{11}').val() );
                    $form.find('.js-account-number').val( $('#{12}').val() );
                    $form.find('.js-routing-number').val( $('#{13}').val() );
                    $form.find('.js-account-type').val( $('#{14}').find('input:checked').val() );
                    $form.find('.js-entity-type').val( 'personal' );
                }}

                $form.attr('action', src );
                $form.submit();
            }}
        }}
    }});

    // Evaluates the current url whenever the iframe is loaded and if it includes a qrystring parameter
    // The qry parameter value is saved to a hidden field and a post back is performed
    $('#iframeStep2').on('load', function(e) {{
        var location = this.contentWindow.location;
        var qryString = this.contentWindow.location.search;
        if ( qryString && qryString != '' && qryString.startsWith('?token-id') ) {{
            $('#{5}').val(qryString);
            window.location = ""javascript:{6}"";
        }} else {{
            if ( $('#{15}').val() == 'true' ) {{
                $('#updateProgress').show();
                var src = $('#{4}').val();
                var $form = $('#iframeStep2').contents().find('#Step2Form');
                $form.attr('action', src );
                $form.submit();
                $('#updateProgress').hide();
            }}
        }}
    }});

";
            string script = string.Format(
                scriptFormat,
                divCCPaymentInfo.ClientID,      // {0}
                hfPaymentTab.ClientID,          // {1}
                oneTimeFrequencyId,             // {2}
                GlobalAttributesCache.Value( "CurrencySymbol" ), // {3)
                hfStep2Url.ClientID,            // {4}
                hfStep2ReturnQueryString.ClientID,   // {5}
                this.Page.ClientScript.GetPostBackEventReference( lbStep2Return, "" ), // {6}
                this.BlockValidationGroup,      // {7}
                txtCreditCard.ClientID,         // {8}
                mypExpiration.ClientID,         // {9}
                txtCVV.ClientID,                // {10}
                txtAccountName.ClientID,        // {11}
                txtAccountNumber.ClientID,      // {12}
                txtRoutingNumber.ClientID,      // {13}
                rblAccountType.ClientID,        // {14}
                hfStep2AutoSubmit.ClientID,     // {15}
                cbBillingAddress.ClientID,      // {16}
                acBillingAddress.ClientID,      // {17}
                txtCardFirstName.ClientID,      // {18}
                txtCardLastName.ClientID,       // {19}
                txtCardName.ClientID,           // {20}
                rblSavedAccount.ClientID,       // {21}
                rblSavedAccount.UniqueID,       // {22}
                divNewPayment.ClientID          // {23}
            );

            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );

            if ( _using3StepGateway )
            {
                string submitScript = string.Format( @"
    $('#{0}').val('');
    $('#{1}_monthDropDownList').val('');
    $('#{1}_yearDropDownList_').val('');
    $('#{2}').val('');
",
                txtCreditCard.ClientID,  // {0}
                mypExpiration.ClientID,  // {1}
                txtCVV.ClientID          // {2}
                );

                ScriptManager.RegisterOnSubmitStatement( Page, Page.GetType(), "clearCCFields", submitScript );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAccountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAccountList_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var accountItem = e.Item.DataItem as AccountItem;
            CurrencyBox txtAccountAmount = e.Item.FindControl( "txtAccountAmount" ) as CurrencyBox;
            RockLiteral txtAccountAmountLiteral = e.Item.FindControl( "txtAccountAmountLiteral" ) as RockLiteral;

            if ( accountItem != null && txtAccountAmount != null )
            {
                string accountHeaderTemplate = this.GetAttributeValue( "AccountHeaderTemplate" );
                var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                var account = new FinancialAccountService( new RockContext() ).Get( accountItem.Id );
                mergeFields.Add( "Account", account );
                txtAccountAmount.Label = accountHeaderTemplate.ResolveMergeFields( mergeFields );

                if ( accountItem.Amount != 0 )
                {
                    txtAccountAmount.Text = accountItem.Amount.ToString( "N2" );
                }

                if ( !accountItem.Enabled )
                {
                    txtAccountAmountLiteral.Visible = true;
                    txtAccountAmountLiteral.Label = txtAccountAmount.Label;
                    txtAccountAmountLiteral.Text = string.Format( "${0}", txtAccountAmount.Text );

                    // Javascript  needs the textbox, so disable it and hide it with CSS.
                    txtAccountAmount.Label = string.Empty;
                    txtAccountAmount.Enabled = false;
                    txtAccountAmount.AddCssClass( "hidden" );
                }
            }
        }

        #endregion

        #region Helper Classes

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

            public bool Enabled { get; set; }

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
                Enabled = true;
            }

            public AccountItem( int id, int order, string name, int? campusId, string publicName, decimal amount, bool enabled )
                : this( id, order, name, campusId, publicName )
            {
                Amount = amount;
                Enabled = enabled;
            }
        }

        /// <summary>
        /// Helper object for data passed via the request string.
        /// </summary>
        protected class ParameterAccount
        {
            public FinancialAccount Account { get; set; }

            public decimal Amount { get; set; }

            public bool Enabled { get; set; }
        }

        #endregion
    }
}

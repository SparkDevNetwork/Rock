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
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Communication;

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
    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]
    [BooleanField( "Additional Accounts", "Display option for selecting additional accounts", "Don't display option",
        "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 7 )]
    [BooleanField( "Scheduled Transactions", "Allow", "Don't Allow",
        "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 8, "AllowScheduled" )]
    [BooleanField( "Prompt for Phone", "Should the user be prompted for their phone number?", false, "", 9, "DisplayPhone" )]
    [BooleanField( "Prompt for Email", "Should the user be prompted for their email address?", true, "", 10, "DisplayEmail" )]
    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 11 )]
    [SystemEmailField( "Confirm Account", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "Email Templates", 12, "ConfirmAccountTemplate" )]
    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "", 5 )]

    // Text Options

    [TextField( "Panel Title", "The text to display in panel heading", false, "Gifts", "Text Options", 13 )]
    [TextField( "Contribution Info Title", "The text to display as heading of section for selecting account and amount.", false, "Contribution Information", "Text Options", 14 )]
    [TextField( "Add Account Text", "The button text to display for adding an additional account", false, "Add Another Account", "Text Options", 15 )]
    [TextField( "Personal Info Title", "The text to display as heading of section for entering personal information.", false, "Personal Information", "Text Options", 16 )]
    [TextField( "Payment Info Title", "The text to display as heading of section for entering credit card or bank account information.", false, "Payment Information", "Text Options", 17 )]
    [TextField( "Confirmation Title", "The text to display as heading of section for confirming information entered.", false, "Confirm Information", "Text Options", 18 )]
    [CodeEditorField( "Confirmation Header", "The text (HTML) to display at the top of the confirmation section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
    Please confirm the information below. Once you have confirmed that the information is
    accurate click the 'Finish' button to complete your transaction.
</p>
", "Text Options", 19 )]
    [CodeEditorField( "Confirmation Footer", "The text (HTML) to display at the bottom of the confirmation section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<div class='alert alert-info'>
    By clicking the 'finish' button below I agree to allow {{ OrganizationName }}
    to transfer the amount above from my account. I acknowledge that I may
    update the transaction information at any time by returning to this website. Please
    call the Finance Office if you have any additional questions.
</div>
", "Text Options", 20 )]
    [TextField( "Success Title", "The text to display as heading of section for displaying details of gift.", false, "Gift Information", "Text Options", 21 )]
    [CodeEditorField( "Success Header", "The text (HTML) to display at the top of the success section. <span class='tip tip-lava'></Fspan> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<p>
    Thank you for your generous contribution.  Your support is helping {{ OrganizationName }} actively
    achieve our mission.  We are so grateful for your commitment.
</p>
", "Text Options", 22 )]
    [CodeEditorField( "Success Footer", "The text (HTML) to display at the bottom of the success section. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, @"
", "Text Options", 23 )]
    [TextField( "Save Account Title", "The text to display as heading of section for saving payment information.", false, "Make Giving Even Easier", "Text Options", 24 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 25 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 26 )]

    [SystemEmailField( "Receipt Email", "The system email to use to send the receipt.", false, "", "Email Templates", 27 )]
    [TextField( "Payment Comment", "The comment to include with the payment transaction when sending to Gateway", false, "Online Contribution", "", 28 )]
    [BooleanField( "Enable Comment Entry", "Allows the guest to enter the the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", false, "", 29 )]
    [TextField( "Comment Entry Label", "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", false, "Comment", "", 30 )]
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
        protected string ScheduleId
        {
            get { return ViewState["ScheduleId"] as string ?? string.Empty; }
            set { ViewState["ScheduleId"] = value; }
        }

        // The URL for the Step-2 Iframe Url
        protected string Step2IFrameUrl { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            // Resolve the text field merge fields
            var configValues = new Dictionary<string, object>();
            lConfirmationHeader.Text = GetAttributeValue( "ConfirmationHeader" ).ResolveMergeFields( configValues );
            lConfirmationFooter.Text = GetAttributeValue( "ConfirmationFooter" ).ResolveMergeFields( configValues );
            lSuccessHeader.Text = GetAttributeValue( "SuccessHeader" ).ResolveMergeFields( configValues );
            lSuccessFooter.Text = GetAttributeValue( "SuccessFooter" ).ResolveMergeFields( configValues );

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

            if ( !Page.IsPostBack )
            {
                SetControlOptions();

                SetPage( 1 );

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
                        }
                    }
                }

            }

            // Update the total amount
            lblTotalAmount.Text = SelectedAccounts.Sum( f => f.Amount ).ToString( "F2" );

            // Set the frequency date label based on if 'One Time' is selected or not
            if ( btnFrequency.Items.Count > 0 )
            {
                dtpStartDate.Label = btnFrequency.Items[0].Selected ? "When" : "First Gift";
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
            int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
            bool oneTime = ( btnFrequency.SelectedValueAsInt() ?? 0 ) == oneTimeFrequencyId;

            dtpStartDate.Label = oneTime ? "When" : "First Gift";

            if ( !oneTime && ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate.Value.Date <= RockDateTime.Today ) )
            {
                dtpStartDate.SelectedDate = RockDateTime.Today.AddDays( 1 ); 
            }

            using ( var rockContext = new RockContext() )
            {
                BindSavedAccounts( rockContext, oneTime );
            }

            SetPage( 1 );

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
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmationPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmationPrev_Click( object sender, EventArgs e )
        {
            SetPage( 1 );
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
            TransactionCode = string.Empty;

            string errorMessage = string.Empty;
            if ( ProcessConfirmation( out errorMessage ) )
            {
                SetPage( 4 );
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
                        nbSaveAccount.Title = "Missing Informaton";
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
                        var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );

                        string errorMessage = string.Empty;

                        PersonAlias authorizedPersonAlias = null;
                        string referenceNumber = string.Empty;
                        FinancialPaymentDetail paymentDetail = null;
                        int? currencyTypeValueId = isACHTxn ? achCurrencyType.Id : ccCurrencyType.Id;

                        if ( string.IsNullOrWhiteSpace( ScheduleId ) )
                        {
                            var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( TransactionCode );
                            if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                            {
                                authorizedPersonAlias = transaction.AuthorizedPersonAlias;
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
                            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).GetByScheduleId( ScheduleId );
                            if ( scheduledTransaction != null )
                            {
                                authorizedPersonAlias = scheduledTransaction.AuthorizedPersonAlias;
                                if ( scheduledTransaction.FinancialGateway != null )
                                {
                                    scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                                }
                                referenceNumber = gateway.GetReferenceNumber( scheduledTransaction, out errorMessage );
                                paymentDetail = scheduledTransaction.FinancialPaymentDetail;
                            }
                        }

                        if ( authorizedPersonAlias != null && authorizedPersonAlias.Person != null && paymentDetail != null )
                        {
                            if ( phCreateLogin.Visible )
                            {
                                var user = UserLoginService.Create(
                                    rockContext,
                                    authorizedPersonAlias.Person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    txtUserName.Text,
                                    txtPassword.Text,
                                    false );

                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                                mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                                var personDictionary = authorizedPersonAlias.Person.ToLiquid() as Dictionary<string, object>;
                                mergeFields.Add( "Person", personDictionary );

                                mergeFields.Add( "User", user );

                                var recipients = new List<Rock.Communication.RecipientData>();
                                recipients.Add( new Rock.Communication.RecipientData( authorizedPersonAlias.Person.Email, mergeFields ) );

                                Rock.Communication.Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ), false );
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
                                if ( authorizedPersonAlias != null )
                                {
                                    var savedAccount = new FinancialPersonSavedAccount();
                                    savedAccount.PersonAliasId = authorizedPersonAlias.Id;
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

        #region Methods

        #region Initialization Methods

        private void SetTargetPerson( RockContext rockContext )
        {
            // If impersonation is allowed, and a valid person key was used, set the target to that person
            if ( GetAttributeValue( "Impersonation" ).AsBooleanOrNull() ?? false )
            {
                string personKey = PageParameter( "Person" );
                if ( !string.IsNullOrWhiteSpace( personKey ) )
                {
                    _targetPerson = new PersonService( rockContext ).GetByUrlEncodedKey( personKey );
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
                    var oneTimeFrequency = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                    if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
                    {
                        btnFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Value, oneTimeFrequency.Id.ToString() ) );
                    }

                    btnFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
                    dtpStartDate.SelectedDate = RockDateTime.Today;
                }
            }

        }

        private string GetSavedAcccountFreqSupported ( GatewayComponent component )
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
                var gatewayComponent = gateway.GetGatewayComponent();
                if ( gatewayComponent != null )
                {
                    var threeStepGateway = gatewayComponent as ThreeStepGatewayComponent;
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
                    .ToList();

                // Find the saved accounts that are valid for the selected CC gateway
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

                // Find the saved accounts that are valid for the selected ACH gateway
                var achSavedAccountIds = new List<int>();
                var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );
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

            bool displayPhone = GetAttributeValue( "DisplayPhone" ).AsBoolean();
            pnbPhone.Visible = displayPhone;
            tdPhoneConfirm.Visible = displayPhone;
            tdPhoneReceipt.Visible = displayPhone;

            var person = GetPerson( false );
            if ( person != null )
            {
                txtCurrentName.Text = person.FullName;
                txtEmail.Text = person.Email;

                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                if ( displayPhone )
                {
                    var phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ) );

                    // If person did not have a home phone number, read the cell phone number (which would then
                    // get saved as a home number also if they don't change it, which is ok ).
                    if ( phoneNumber == null || string.IsNullOrWhiteSpace( phoneNumber.Number ) )
                    {
                        phoneNumber = personService.GetPhoneNumber( person, DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ) );
                    }

                    if ( phoneNumber != null )
                    {
                        pnbPhone.CountryCode = phoneNumber.CountryCode;
                        pnbPhone.Number = phoneNumber.ToString();
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

                var groupLocation = personService.GetFirstLocation( person.Id, DefinedValueCache.Read( addressTypeGuid ).Id );
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

            txtCurrentName.Visible = person != null;
            txtFirstName.Visible = person == null;
            txtLastName.Visible = person == null;

            // Evaluate if comment entry box should be displayed
            txtCommentEntry.Label = GetAttributeValue( "CommentEntryLabel" );
            txtCommentEntry.Visible = GetAttributeValue( "EnableCommentEntry" ).AsBoolean();

            // Se the payment method tabs
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
            cbBillingAddress.Visible = _ccGatewayComponent.PromptForBillingAddress( _ccGateway );
            divBillingAddress.Visible = _ccGatewayComponent.PromptForBillingAddress( _ccGateway );

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

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            rptAccountList.DataSource = SelectedAccounts.ToList();
            rptAccountList.DataBind();

            btnAddAccount.Visible = AvailableAccounts.Any();
            btnAddAccount.DataSource = AvailableAccounts;
            btnAddAccount.DataBind();
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

            if ( create )
            {
                if ( person == null )
                {
                    // Check to see if there's only one person with same email, first name, and last name
                    if ( !string.IsNullOrWhiteSpace( txtEmail.Text ) &&
                        !string.IsNullOrWhiteSpace( txtFirstName.Text ) &&
                        !string.IsNullOrWhiteSpace( txtLastName.Text ) )
                    {
                        // Same logic as CreatePledge.ascx.cs
                        var personMatches = personService.GetByMatch( txtFirstName.Text, txtLastName.Text, txtEmail.Text );
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
                        person.FirstName = txtFirstName.Text;
                        person.LastName = txtLastName.Text;
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
            }

            if ( create && person != null ) // person should never be null at this point
            {
                person.Email = txtEmail.Text;

                if ( GetAttributeValue( "DisplayPhone" ).AsBooleanOrNull() ?? false )
                {
                    var numberTypeId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
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
                        familyGroup = personService.GetFamilies( person.Id ).FirstOrDefault();
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

            bool displayPhone = GetAttributeValue( "DisplayPhone" ).AsBoolean();
            if ( displayPhone && string.IsNullOrWhiteSpace( pnbPhone.Number ) )
            {
                errorMessages.Add( "Make sure to enter a valid phone number.  A phone number is required for us to process this transaction" );
            }

            bool displayEmail = GetAttributeValue( "DisplayEmail" ).AsBoolean();
            if ( displayEmail && string.IsNullOrWhiteSpace( txtEmail.Text ) )
            {
                errorMessages.Add( "Make sure to enter a valid email address.  An email address is required for us to send you a payment confirmation" );
            }

            var location = new Location();
            acAddress.GetValues( location );
            if ( string.IsNullOrWhiteSpace( location.Street1 ) )
            {
                errorMessages.Add( "Make sure to enter a valid address.  An address is required for us to process this transaction" );
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

            tdNameConfirm.Description = paymentInfo.FullName;
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
            var gateway = ( isACHTxn ? _achGatewayComponent : _ccGatewayComponent ) as ThreeStepGatewayComponent;

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
                int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() && dtpStartDate.SelectedDate <= RockDateTime.Today )
                {
                    // one-time immediate payment
                    return null;
                }

                var schedule = new PaymentSchedule();
                schedule.TransactionFrequencyValue = DefinedValueCache.Read( btnFrequency.SelectedValueAsId().Value );
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
                bool isACHTxn = hfPaymentTab.Value == "ACH";
                var financialGateway = isACHTxn ? _achGateway : _ccGateway;
                var gateway = isACHTxn ? _achGatewayComponent : _ccGatewayComponent;

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

                if ( !person.PrimaryAliasId.HasValue )
                {
                    errorMessage = "There was a problem creating the person's primary alias";
                    return false;
                }

                PaymentInfo paymentInfo = GetTxnPaymentInfo( person, out errorMessage );
                if ( paymentInfo == null )
                {
                    return false;
                }

                PaymentSchedule schedule = GetSchedule();
                FinancialPaymentDetail paymentDetail = null;
                if ( schedule != null )
                {
                    schedule.PersonId = person.Id;

                    var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                    if ( scheduledTransaction == null )
                    {
                        return false;
                    }

                    SaveScheduledTransaction( financialGateway, gateway, person, paymentInfo, schedule, scheduledTransaction, rockContext );
                    paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                }
                else
                {
                    var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                    if ( transaction == null )
                    {
                        return false;
                    }

                    SaveTransaction( financialGateway, gateway, person, paymentInfo, transaction, rockContext );
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

            bool isACHTxn = hfPaymentTab.Value == "ACH";
            var financialGateway = isACHTxn ? _achGateway : _ccGateway;
            var gateway = ( isACHTxn ? _achGatewayComponent : _ccGatewayComponent ) as ThreeStepGatewayComponent;

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

            if ( !person.PrimaryAliasId.HasValue )
            {
                errorMessage = "There was a problem creating the person's primary alias";
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

            PaymentSchedule schedule = GetSchedule();
            FinancialPaymentDetail paymentDetail = null;
            if ( schedule != null )
            {
                var scheduledTransaction = gateway.AddScheduledPaymentStep3( financialGateway, resultQueryString, out errorMessage );
                if ( scheduledTransaction == null )
                {
                    return false;
                }

                paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                SaveScheduledTransaction( financialGateway, gateway, person, paymentInfo, schedule, scheduledTransaction, rockContext );
            }
            else
            {
                var transaction = gateway.ChargeStep3( financialGateway, resultQueryString, out errorMessage );
                if ( transaction == null || !string.IsNullOrWhiteSpace( errorMessage ) )
                {
                    return false;
                }

                paymentDetail = transaction.FinancialPaymentDetail.Clone( false );
                SaveTransaction( financialGateway, gateway, person, paymentInfo, transaction, rockContext );
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

            if ( GetAttributeValue( "EnableCommentEntry" ).AsBoolean() )
            {
                paymentInfo.Comment1 = !string.IsNullOrWhiteSpace( GetAttributeValue( "PaymentComment" ) ) ? string.Format( "{0}: {1}", GetAttributeValue( "PaymentComment" ), txtCommentEntry.Text ) : txtCommentEntry.Text;
            }
            else
            {
                paymentInfo.Comment1 = GetAttributeValue( "PaymentComment" );
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

            foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
            {
                var transactionDetail = new FinancialScheduledTransactionDetail();
                transactionDetail.Amount = account.Amount;
                transactionDetail.AccountId = account.Id;
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

        private void SaveTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, FinancialTransaction transaction, RockContext rockContext )
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

            foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
            {
                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = account.Amount;
                transactionDetail.AccountId = account.Id;
                transaction.TransactionDetails.Add( transactionDetail );
                History.EvaluateChange( txnChanges, account.Name, 0.0M.FormatAsCurrency(), transactionDetail.Amount.FormatAsCurrency() );
            }

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

        private void ShowSuccess( GatewayComponent gatewayComponent, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialPaymentDetail paymentDetail, RockContext rockContext )
        {
            tdTransactionCodeReceipt.Description = TransactionCode;
            tdTransactionCodeReceipt.Visible = !string.IsNullOrWhiteSpace( TransactionCode );

            tdScheduleId.Description = ScheduleId;
            tdScheduleId.Visible = !string.IsNullOrWhiteSpace( ScheduleId );

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
        }


        private void SendReceipt( int transactionId )
        {
            Guid? recieptEmail = GetAttributeValue( "ReceiptEmail" ).AsGuidOrNull();
            if ( recieptEmail.HasValue )
            {
                // Queue a transaction to send reciepts
                var newTransactionIds = new List<int> { transactionId };
                var sendPaymentRecieptsTxn = new Rock.Transactions.SendPaymentReciepts( recieptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentRecieptsTxn );
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

        // Toggle credit card display if saved card option is available
        $('div.radio-content').prev('div.radio-list').find('input:radio').unbind('click').on('click', function () {{
            $content = $(this).parents('div.radio-list:first').next('.radio-content');
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

    // sets the scroll position to the top of the page after partial postbacks
    // without this the scroll position is the bottom of the page.
    setTimeout('window.scrollTo(0,0)',0);

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
            {6};
        }} else {{
            if ( $('#{15}').val() == 'true' ) {{
                $('#updateProgress').show();
                var src = $('#{4}').val();
                var $form = $('#iframeStep2').contents().find('#Step2Form');
                $form.attr('action', src );
                $form.submit();
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
                txtCardName.ClientID            // {20}
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

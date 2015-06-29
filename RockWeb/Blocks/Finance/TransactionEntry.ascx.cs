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
    /// Add a new one-time or scheduled transaction
    /// </summary>
    [DisplayName( "Transaction Entry" )]
    [Category( "Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]

    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [FinancialGatewayField( "ACH Card Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]
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
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
", "Text Options", 23 )]

    [TextField( "Save Account Title", "The text to display as heading of section for saving payment information.", false, "Make Giving Even Easier", "Text Options", 24 )]

    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 25 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 26 )]
    #endregion

    public partial class TransactionEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private bool _showRepeatingOptions = false;
        private FinancialGateway _ccGateway;
        private FinancialGateway _achGateway;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [fluid layout].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fluid layout]; otherwise, <c>false</c>.
        /// </value>
        protected bool FluidLayout { get; set; }

        /// <summary>
        /// Gets or sets the target person.
        /// </summary>
        protected Person TargetPerson { get; private set; }

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

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack )
            {
                lPanelTitle1.Text = GetAttributeValue( "PanelTitle" );
                lPanelTitle2.Text = GetAttributeValue( "PanelTitle" );
                lContributionInfoTitle.Text = GetAttributeValue( "ContributionInfoTitle" );
                lPersonalInfoTitle.Text = GetAttributeValue( "PersonalInfoTitle" );
                lPaymentInfoTitle.Text = GetAttributeValue( "PaymentInfoTitle" );
                lConfirmationTitle.Text = GetAttributeValue( "ConfirmationTitle" );
                lSuccessTitle.Text = GetAttributeValue( "SuccessTitle" );
                lSaveAcccountTitle.Text = GetAttributeValue( "SaveAccountTitle" );
            }

            // Enable payment options based on the configured gateways
            bool ccEnabled = false;
            bool achEnabled = false;
            GatewayComponent ccGatewayComponent = null;
            GatewayComponent achGatewayComponent = null;
            var supportedFrequencies = new List<DefinedValueCache>();

            using ( var rockContext = new RockContext() )
            {
                // If impersonation is allowed, and a valid person key was used, set the target to that person
                if ( GetAttributeValue( "Impersonation" ).AsBooleanOrNull() ?? false )
                {
                    string personKey = PageParameter( "Person" );
                    if ( !string.IsNullOrWhiteSpace( personKey ) )
                    {
                        TargetPerson = new PersonService( rockContext ).GetByUrlEncodedKey( personKey );
                    }
                }

                // if TargetPerson wasn't set by Impersonation, try to get it from the PersonId parameter (if specified)
                if ( TargetPerson == null )
                {
                    int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        TargetPerson = new PersonService( rockContext ).Get( personId.Value );
                    }
                }

                if ( TargetPerson == null )
                {
                    TargetPerson = CurrentPerson;
                }

                var financialGatewayService = new FinancialGatewayService( rockContext );
                Guid? ccGatewayGuid = GetAttributeValue( "CCGateway" ).AsGuidOrNull();
                if ( ccGatewayGuid.HasValue )
                {
                    _ccGateway = financialGatewayService.Get( ccGatewayGuid.Value );
                    if ( _ccGateway != null )
                    {
                        _ccGateway.LoadAttributes( rockContext );
                        ccGatewayComponent = _ccGateway.GetGatewayComponent();
                        if ( ccGatewayComponent != null )
                        {
                            ccEnabled = true;
                            txtCardFirstName.Visible = ccGatewayComponent.SplitNameOnCard;
                            txtCardLastName.Visible = ccGatewayComponent.SplitNameOnCard;
                            txtCardName.Visible = !ccGatewayComponent.SplitNameOnCard;
                            mypExpiration.MinimumYear = RockDateTime.Now.Year;
                        }
                    }
                }

                Guid? achGatewayGuid = GetAttributeValue( "ACHGateway" ).AsGuidOrNull();
                if ( achGatewayGuid.HasValue )
                {
                    _achGateway = financialGatewayService.Get( achGatewayGuid.Value );
                    if ( _achGateway != null )
                    {
                        _achGateway.LoadAttributes( rockContext );
                        achGatewayComponent = _achGateway.GetGatewayComponent();
                        achEnabled = achGatewayComponent != null;
                    }
                }
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
                    supportedFrequencies = ccGatewayComponent.SupportedPaymentSchedules;
                    hfPaymentTab.Value = "CreditCard";
                }
                else
                {
                    supportedFrequencies = achGatewayComponent.SupportedPaymentSchedules;
                    hfPaymentTab.Value = "ACH";
                }

                if ( ccEnabled && achEnabled )
                {
                    phPills.Visible = true;

                    // If CC and ACH gateways are different, only allow frequencies supported by both payment gateways (if different)
                    if ( ccGatewayComponent.TypeId != _achGateway.TypeId )
                    {
                        supportedFrequencies = ccGatewayComponent.SupportedPaymentSchedules
                            .Where( c =>
                                achGatewayComponent.SupportedPaymentSchedules
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
                            btnFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Value, oneTimeFrequency.Id.ToString() ) );
                        }

                        btnFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
                        dtpStartDate.SelectedDate = RockDateTime.Today;
                    }
                }

                // Display Options
                btnAddAccount.Title = GetAttributeValue( "AddAccountText" );

                bool displayEmail = GetAttributeValue( "DisplayEmail" ).AsBoolean();
                txtEmail.Visible = displayEmail;
                tdEmailConfirm.Visible = displayEmail;
                tdEmailReceipt.Visible = displayEmail;

                bool displayPhone = GetAttributeValue( "DisplayPhone" ).AsBoolean();
                pnbPhone.Visible = displayPhone;
                tdPhoneConfirm.Visible = displayPhone;
                tdPhoneReceipt.Visible = displayPhone;

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

                RegisterScript();

                // Resolve the text field merge fields
                var configValues = new Dictionary<string, object>();
                phConfirmationHeader.Controls.Add( new LiteralControl( GetAttributeValue( "ConfirmationHeader" ).ResolveMergeFields( configValues ) ) );
                phConfirmationFooter.Controls.Add( new LiteralControl( GetAttributeValue( "ConfirmationFooter" ).ResolveMergeFields( configValues ) ) );
                phSuccessHeader.Controls.Add( new LiteralControl( GetAttributeValue( "SuccessHeader" ).ResolveMergeFields( configValues ) ) );
                phSuccessFooter.Controls.Add( new LiteralControl( GetAttributeValue( "SuccessFooter" ).ResolveMergeFields( configValues ) ) );

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
                lblTotalAmount.Text = SelectedAccounts.Sum( f => f.Amount ).ToString( "F2" );

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
                    var rockContext = new RockContext();

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

                        var personService = new PersonService( rockContext );

                        bool displayPhone = false;
                        if ( bool.TryParse( GetAttributeValue( "DisplayPhone" ), out displayPhone ) && displayPhone )
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
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Please check the configuration of this block and make sure a valid Credit Card and/or ACH Financial Gateway has been selected." );
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
                        ShowMessage( NotificationBoxType.Danger, "Before we finish...", errorMessage );
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
                    GatewayComponent gateway = null;
                    var financialGateway = hfPaymentTab.Value == "ACH" ? _achGateway : _ccGateway;
                    if ( financialGateway != null )
                    {
                        gateway = financialGateway.GetGatewayComponent();
                    }

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );

                        string errorMessage = string.Empty;

                        PersonAlias authorizedPersonAlias = null;
                        string referenceNumber = string.Empty;
                        int? currencyTypeValueId = hfPaymentTab.Value == "ACH" ? achCurrencyType.Id : ccCurrencyType.Id;

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
                            }
                        }

                        if ( authorizedPersonAlias != null && authorizedPersonAlias.Person != null )
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

                                var mergeObjects = GlobalAttributesCache.GetMergeFields( null );
                                mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                                var personDictionary = authorizedPersonAlias.Person.ToLiquid() as Dictionary<string, object>;
                                mergeObjects.Add( "Person", personDictionary );

                                mergeObjects.Add( "User", user );

                                var recipients = new List<Rock.Communication.RecipientData>();
                                recipients.Add( new Rock.Communication.RecipientData( authorizedPersonAlias.Person.Email, mergeObjects ) );

                                Rock.Communication.Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
                            }

                            var paymentInfo = GetPaymentInfo();

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
                                    savedAccount.MaskedAccountNumber = paymentInfo.MaskedNumber;
                                    savedAccount.TransactionCode = TransactionCode;
                                    savedAccount.FinancialGatewayId = financialGateway.Id;
                                    savedAccount.CurrencyTypeValueId = currencyTypeValueId;
                                    savedAccount.CreditCardTypeValueId = CreditCardTypeValueId;

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

        #region Methods for the Payment Info Page (panel)

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        private void GetAccounts()
        {
            var rockContext = new RockContext();
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
            foreach ( var account in new FinancialAccountService( rockContext ).Queryable()
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
        }

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
            if ( personId == 0 && TargetPerson != null )
            {
                personId = TargetPerson.Id;
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
                    GroupService.AddNewFamilyAddress(
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
        /// Binds the saved accounts.
        /// </summary>
        private void BindSavedAccounts()
        {
            rblSavedCC.Items.Clear();

            if ( TargetPerson != null )
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService( new RockContext() )
                    .GetByPersonId( TargetPerson.Id );

                if ( _ccGateway != null )
                {
                    var ccGatewayComponent = _ccGateway.GetGatewayComponent(); 
                    var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                    if ( ccGatewayComponent != null && ccGatewayComponent.SupportsSavedAccount( ccCurrencyType ) )
                    {
                        rblSavedCC.DataSource = savedAccounts
                            .Where( a =>
                                a.FinancialGatewayId == _ccGateway.Id &&
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
                    }
                }

                if ( _achGateway != null )
                {
                    var achGatewayComponent = _ccGateway.GetGatewayComponent();
                    var achCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );
                    if ( achGatewayComponent != null && achGatewayComponent.SupportsSavedAccount( achCurrencyType ) )
                    {
                        rblSavedAch.DataSource = savedAccounts
                            .Where( a =>
                                a.FinancialGatewayId == _achGateway.Id &&
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
            if ( string.IsNullOrWhiteSpace( location.Street1 )  )
            {
                errorMessages.Add( "Make sure to enter a valid address.  An address is required for us to process this transaction" );
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
            else
            {
                // validate cc options
                if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsInt() ?? 0 ) > 0 )
                {
                    // TODO: Find saved card
                }
                else
                {
                    var ccGatewayComponent = _ccGateway.GetGatewayComponent();
                    if ( ccGatewayComponent != null && ccGatewayComponent.SplitNameOnCard )
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

            tdPaymentMethodConfirm.Description = paymentInfo.CurrencyTypeValue.Description;
            tdAccountNumberConfirm.Description = paymentInfo.MaskedNumber;
            tdWhenConfirm.Description = schedule != null ? schedule.ToString() : "Today";

            return true;
        }

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
            var ccGatewayComponent = _ccGateway.GetGatewayComponent();
            var cc = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
            cc.NameOnCard = ccGatewayComponent != null && ccGatewayComponent.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
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

        /// <summary>
        /// Gets the payment schedule.
        /// </summary>
        /// <returns></returns>
        private PaymentSchedule GetSchedule()
        {
            // Figure out if this is a one-time transaction or a future scheduled transaction
            bool repeating = _showRepeatingOptions;
            if ( repeating )
            {
                // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
                int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() && dtpStartDate.SelectedDate == RockDateTime.Today )
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
                GatewayComponent gateway = null;
                var financialGateway = hfPaymentTab.Value == "ACH" ? _achGateway : _ccGateway;
                if ( financialGateway != null )
                {
                    gateway = financialGateway.GetGatewayComponent();
                }

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

                if ( !person.PrimaryAliasId.HasValue)
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

                PaymentSchedule schedule = GetSchedule();
                if ( schedule != null )
                {
                    schedule.PersonId = person.Id;

                    var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                    if ( scheduledTransaction != null  )
                    {
                        scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
                        scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
                        scheduledTransaction.FinancialGatewayId = financialGateway.Id;
                        scheduledTransaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
                        scheduledTransaction.CreditCardTypeValueId = CreditCardTypeValueId;

                        var changeSummary = new StringBuilder();
                        changeSummary.AppendFormat( "{0} starting {1}", schedule.TransactionFrequencyValue.Value, schedule.StartDate.ToShortDateString() );
                        changeSummary.AppendLine();
                        changeSummary.Append( paymentInfo.CurrencyTypeValue.Value );
                        if (paymentInfo.CreditCardTypeValue != null)
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
                            changeSummary.AppendFormat( "{0}: {1:C2}", account.Name, account.Amount );
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
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                    if ( transaction != null )
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

                        transaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
                        History.EvaluateChange( txnChanges, "Currency Type", string.Empty, paymentInfo.CurrencyTypeValue.Value );

                        transaction.CreditCardTypeValueId = CreditCardTypeValueId;
                        if ( CreditCardTypeValueId.HasValue )
                        {
                            var ccType = DefinedValueCache.Read( CreditCardTypeValueId.Value );
                            History.EvaluateChange( txnChanges, "Credit Card Type", string.Empty, ccType.Value );
                        }

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
                            History.EvaluateChange( txnChanges, account.Name, 0.0M.ToString( "C2" ), transactionDetail.Amount.ToString("C2") );
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
                        History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.ToString("C2"), newControlAmount.ToString("C2") );
                        batch.ControlAmount = newControlAmount;

                        transaction.BatchId = batch.Id;
                        batch.Transactions.Add( transaction );

                        rockContext.WrapTransaction( () =>
                        {
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
                        } );

                        TransactionCode = transaction.TransactionCode;
                    }
                    else
                    {
                        return false;
                    }
                }

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
                tdAccountNumberReceipt.Description = paymentInfo.MaskedNumber;
                tdWhenReceipt.Description = schedule != null ? schedule.ToString() : "Today";


                // If there was a transaction code returned and this was not already created from a previous saved account,
                // show the option to save the account.
                if ( !( paymentInfo is ReferencePaymentInfo ) && !string.IsNullOrWhiteSpace( TransactionCode ) && gateway.SupportsSavedAccount( paymentInfo.CurrencyTypeValue ) )
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

                return true;
            }
            else
            {
                pnlDupWarning.Visible = true;
                divActions.Visible = false;
                errorMessage = string.Empty;
                return false;
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
                nbMessage.Title = string.Format( "<p>{0}</p>", title );
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
                    $dateInput.data('datepicker').update();
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

";
            string script = string.Format( scriptFormat, divCCPaymentInfo.ClientID, hfPaymentTab.ClientID, oneTimeFrequencyId );
            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
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
                    return Amount > 0 ? Amount.ToString( "C2" ) : string.Empty;
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

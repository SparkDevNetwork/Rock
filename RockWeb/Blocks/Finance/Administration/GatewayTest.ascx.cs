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

namespace RockWeb.Blocks.Finance.Administration
{
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "ACH Card Gateway", "The payment gateway to use for ACH (bank account) transactions", false, "", "", 1, "ACHGateway" )]

    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 2 )]
    [BooleanField( "Allow Other Accounts", "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", true, "", 3 )]

    [BooleanField( "Allow Scheduled Transactions", "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 2, "AllowScheduled" )]


    [DefinedValueField( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME, "", 3 )]

    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "Display Options", 4 )]
    [BooleanField( "Prompt for Email", "Should the user be prompted for their email address?", true, "Display Options", 5, "DisplayEmail" )]
    [BooleanField( "Prompt for Phone", "Should the user be prompted for their phone number?", false, "Display Options", 6, "DisplayPhone" )]

    public partial class GatewayTest : Rock.Web.UI.RockBlock
    {

        #region Fields

        protected bool FluidLayout = false;
        private bool _showRepeatingOptions = false;
        private GatewayComponent _ccGateway;
        private GatewayComponent _achGateway;

        #endregion 

        #region Properties


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

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                bool display = false;
                txtEmail.Visible = bool.TryParse( GetAttributeValue( "DisplayEmail" ), out display ) && display;
                txtPhone.Visible = bool.TryParse( GetAttributeValue( "DisplayPhone" ), out display ) && display;
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

                // Temp values for testing...
                txtCardName.Text = "David R Turner";
                txtCreditCard.Text = "5105105105105100";
                mypExpiration.SelectedDate = new DateTime( 2014, 1, 1 );
                txtCVV.Text = "023";

                txtBankName.Text = "Test Bank";
                txtRoutingNumber.Text = "111111118";
                txtAccountNumber.Text = "1111111111";
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
            pnlReport.Visible = false;

            if ( _ccGateway != null || _achGateway != null )
            {
                phSelection.Visible = true;
                pnlPaymentInfo.Visible = true;
                divActions.Visible = true;

                // Save amounts from controls to the viewstate list
                foreach ( RepeaterItem item in rptAccountList.Items )
                {
                    var accountAmount = item.FindControl( "txtAccountAmount" ) as LabeledTextBox;
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
                lblTotalAmount.Text = SelectedAccounts.Select( f => f.Amount).Sum().ToString("F2");

                // Set the frequency date label based on if 'One Time' is selected or not
                if ( btnFrequency.Items.Count > 0 )
                {
                    dtpStartDate.LabelText = btnFrequency.Items[0].Selected ? "When" : "First Gift";
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
                divNewCard.Style[HtmlTextWriterStyle.Display] = ( rblSavedCC.Items.Count > 0 && rblSavedCC.Items[0].Selected ) ? "none" : "block";

                // Show billing address based on if billing address checkbox is checked
                divBillingAddress.Style[HtmlTextWriterStyle.Display] = cbBillingAddress.Checked ? "block" : "none";

                if ( !Page.IsPostBack )
                {
                    // Get the list of accounts that can be used
                    GetAccounts();
                    BindAccounts();

                    // Set personal information if there is a currently logged in person
                    if ( CurrentPerson != null )
                    {
                        txtFirstName.Text = CurrentPerson.FirstName;
                        txtLastName.Text = CurrentPerson.LastName;
                        txtEmail.Text = CurrentPerson.Email;

                        Guid addressTypeGuid = Guid.Empty;
                        if ( !Guid.TryParse( GetAttributeValue( "AddressType" ), out addressTypeGuid ) )
                        {
                            addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME );
                        }

                        var address = new PersonService().GetFirstLocation( CurrentPerson, DefinedValueCache.Read( addressTypeGuid ).Id );
                        if ( address != null )
                        {
                            txtStreet.Text = address.Street1;
                            txtCity.Text = address.City;
                            ddlState.SelectedValue = address.State;
                            txtZip.Text = address.Zip;
                        }
                    }

                }
            }
            else
            {
                phSelection.Visible = false;
                pnlPaymentInfo.Visible = false;
                divActions.Visible = false;
                ShowMessage( NotificationBoxType.Error, "Configuration Error", "Please check the configuration of this block and make sure a valid Credit Card and/or ACH Finacial Gateway has been selected." );
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
            AvailableAccounts = AvailableAccounts.Except(selected).ToList();
            SelectedAccounts.AddRange(selected);

            BindAccounts();
        }

        protected void btnNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;

            GatewayComponent gateway = hfPaymentTab.Value == "ACH" ? _achGateway : _ccGateway;
            PaymentInfo paymentInfo = GetPaymentInfo();
            PaymentSchedule schedule = GetSchedule();

            if ( schedule != null )
            {
                if ( schedule.StartDate <= DateTime.Today )
                {
                    ShowMessage( NotificationBoxType.Error, "Payment Error", "First Gift must be a future date" );
                }
                else
                {
                    var scheduledTransaction = gateway.AddScheduledPayment( schedule, paymentInfo, out errorMessage );
                    if ( scheduledTransaction != null )
                    {
                        ShowMessage( NotificationBoxType.Success, "Success", "Schedule ID: " + scheduledTransaction.GatewayScheduleId );
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Error, "Payment Error", errorMessage );
                    }
                }
            }
            else
            {
                var transaction = gateway.Charge( paymentInfo, out errorMessage );
                if ( transaction != null )
                {
                    ShowMessage( NotificationBoxType.Success, "Success", "Transaction Code: " + transaction.TransactionCode );
                }
                else
                {
                    ShowMessage( NotificationBoxType.Error, "Payment Error", errorMessage );
                }
            }

        }

        protected void btnTest_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;

            var txns = _ccGateway.GetPayments( new DateTime( 2013, 9, 9 ), new DateTime( 2013, 9, 10 ), out errorMessage );
            if ( txns != null )
            {
                gReport.DataSource = txns;
                gReport.DataBind();
                pnlReport.Visible = true;
            }
            else
            {
                ShowMessage( NotificationBoxType.Error, "Report Error", errorMessage );
            }
        }

        protected void btnScheduleGo_Click( object sender, EventArgs e )
        {
            var transaction = new FinancialScheduledTransaction();
            transaction.GatewayScheduleId = txtScheduleId.Text;

            bool success = false;
            string errorMessage = string.Empty;
            switch ( ddlAction.SelectedValue )
            {
                case "Status":
                    success = _ccGateway.GetScheduledPaymentStatus( transaction, out errorMessage );
                    break;

                case "UpdateSchedule":
                case "UpdatePayment":

                    PaymentSchedule schedule = GetSchedule();

                    if ( schedule != null )
                    {
                        if ( schedule.StartDate <= DateTime.Today )
                        {
                            ShowMessage( NotificationBoxType.Error, "Payment Error", "Date must be a future date" );
                        }
                        else
                        {
                            PaymentInfo paymentInfo = null;
                            if ( ddlAction.SelectedValue == "UpdatePayment" )
                            {
                                paymentInfo = GetPaymentInfo();
                            }
                            success = _ccGateway.UpdateScheduledPayment( transaction, paymentInfo, out errorMessage );
                        }
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Error, "Payment Error", "Invalid Schedule" );
                    }

                    break;

                case "Cancel":
                    success = _ccGateway.CancelScheduledPayment( transaction, out errorMessage );
                    break;
            }

            if ( success )
            {
                ShowMessage( NotificationBoxType.Success, "Success", string.Format(
                    "<br/>TransactionCode: {0}<br/>ScheduleId: {1}<br/>Active: {2}<br/>Start Date: {3}<br/>Next Payment: {4}<br/>Number of Payments: {5}",
                    transaction.TransactionCode, transaction.GatewayScheduleId, transaction.IsActive.ToString(),
                    transaction.StartDate, transaction.NextPaymentDate, transaction.NumberOfPayments ) );
            }
            else
            {
                ShowMessage( NotificationBoxType.Error, "Payment Error", errorMessage );
            }
        }

        private void GetAccounts()
        {
            var selectedGuids = GetAttributeValues( "Accounts" ).Select( Guid.Parse ).ToList();
            bool showAll = !selectedGuids.Any();

            bool allowOtherAccounts = true;
            if ( !bool.TryParse( GetAttributeValue( "AllowOtherAccounts" ), out allowOtherAccounts ) )
            {
                allowOtherAccounts = true;
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
                        if ( allowOtherAccounts )
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

        private Person GetPerson()
        {
            if (CurrentPerson != null && CurrentPerson.FirstName == txtFirstName.Text && CurrentPerson.LastName == txtLastName.Text)
            {
                return CurrentPerson;
            }
            else
            {
                // TODO Create New Person
               return null;
            }
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

            paymentInfo.Amount = 0.02M;
            paymentInfo.FirstName = txtFirstName.Text;
            paymentInfo.LastName = txtLastName.Text;
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

        private ReferencePaymentInfo GetReferenceInfo(int savedAccountId)
        {
            var savedAccount = new FinancialPersonSavedAccountService().Get( savedAccountId );
            if ( savedAccount != null )
            {
                var reference = new ReferencePaymentInfo( savedAccount.TransactionCode );
                return reference;
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
                    schedule.PersonId = GetPerson().Id;
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
        /// Binds the saved accounts.
        /// </summary>
        private void BindSavedAccounts()
        {
            rblSavedCC.Items.Clear();

            if ( CurrentPersonId.HasValue )
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService()
                    .GetByPersonId( CurrentPersonId.Value ).ToList();

                if ( savedAccounts.Any() )
                {
                    if ( _ccGateway != null )
                    {
                        rblSavedCC.DataSource = savedAccounts
                            .Where( a =>
                                a.GatewayEntityTypeId == _ccGateway.TypeId &&
                                a.PaymentMethod == PaymentMethod.CreditCard )
                            .OrderBy( a => a.Name )
                            .Select( a => new
                            {
                                Id = a.Id,
                                Name = "Use " + a.Name + " (" + a.MaskedAccountNumber + ")"
                            } );
                        rblSavedCC.DataBind();
                        if ( rblSavedCC.Items.Count > 0 )
                        {
                            rblSavedCC.Items.Add( new ListItem( "Use a different card", "0" ) );
                        }
                    }

                    if ( _achGateway != null )
                    {
                        rblSavedAch.DataSource = savedAccounts
                            .Where( a =>
                                a.GatewayEntityTypeId == _achGateway.TypeId &&
                                a.PaymentMethod == PaymentMethod.ACH )
                            .OrderBy( a => a.Name )
                            .Select( a => new
                            {
                                Id = a.Id,
                                Name = "Use " + a.Name + " (" + a.MaskedAccountNumber + ")"
                            } );
                        rblSavedAch.DataBind();
                        if ( rblSavedAch.Items.Count > 0 )
                        {
                            rblSavedAch.Items.Add( new ListItem( "Use a different bank account", "0" ) );
                        }
                    }
                }
            }
        }

        private void RegisterScript()
        {
            CurrentPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;

          string script = string.Format( @"
    Sys.Application.add_load(function () {{

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
            }});
            $('.total-amount').html('$ ' + totalAmt.toFixed(2));
            return false;
        }});

        $('#ButtonDropDown_btnFrequency .dropdown-menu a').click( function () {{
            var $when = $(this).parents('div.control-group').next(); 
            if ($(this).attr('data-id') == '{2}') {{
                $when.find('div.control-label').html('When');
            }} else {{
                $when.find('div.control-label').html('First Gift');

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

        // Save the state of the selected pill to a hidden field so that state can 
        // be preserved through postback
        $('a[data-toggle=""pill""]').on('shown', function (e) {{
            var tabHref = $(e.target).attr(""href"");
            if (tabHref == '#{0}') {{
                $('#{1}').val('CreditCard');
            }} else {{
                $('#{1}').val('ACH');
            }}
        }});

        // detect credit card type
        $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card_logos' }});

        $('.radio-list input:radio').unbind('click').on('click', function () {{
            var $content = $(this).parents('.radio-list').next('.radio-content')
            var radioDisplay = $content.css('display');            
            if ($(this).val() == 0 && radioDisplay == 'none') {{
                $content.slideToggle();
            }}
            else if ($(this).val() != 0 && radioDisplay != 'none') {{
                $content.slideToggle();
            }}
        }});      

        // Hide or show a div based on selection of checkbox
        $('.toggle-input input:checkbox').unbind('click').on('click', function () {{
            $(this).parents('.toggle-input').next('.toggle-content').slideToggle();
        }});
 
    }});

    function calcTotalAmount() {{
    }}

", divCCPaymentInfo.ClientID, hfPaymentTab.ClientID, oneTimeFrequencyId );

            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
        }

        private void ShowMessage(NotificationBoxType type, string title, string text)
        {
            nbMessage.Text = text;
            nbMessage.Title = title;
            nbMessage.NotificationBoxType = type;
            nbMessage.Visible = true;
        }

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

}

    
}

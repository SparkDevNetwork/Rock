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
    [BooleanField( "Allow Scheduled Transactions", "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 2, "AllowScheduled" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME, "", 3 )]

    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "Display Options", 4 )]
    [BooleanField( "Prompt for Email", "Should the user be prompted for their email address?", true, "Display Options", 5, "DisplayEmail" )]
    [BooleanField( "Prompt for Phone", "Should the user be prompted for their phone number?", false, "Display Options", 6, "DisplayPhone" )]

    public partial class GatewayTest : Rock.Web.UI.RockBlock
    {
        protected bool FluidLayout = false;
        private bool _showRepeatingOptions = false;
        private GatewayComponent _ccGateway;
        private GatewayComponent _achGateway;

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
                    supportedFrequencies = _ccGateway.SupportedFrequencyValues;
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
                    supportedFrequencies = _ccGateway.SupportedFrequencyValues;
                    hfPaymentTab.Value = "CreditCard";
                }
                else
                {
                    supportedFrequencies = _achGateway.SupportedFrequencyValues;
                    hfPaymentTab.Value = "ACH";
                }

                if ( ccEnabled && achEnabled )
                {
                    phPills.Visible = true;

                    // If CC and ACH gateways are different, only allow frequencies supported by both payment gateways (if different)
                    if ( _ccGateway.TypeId != _achGateway.TypeId )
                    {
                        supportedFrequencies = _ccGateway.SupportedFrequencyValues
                            .Where( c =>
                                _achGateway.SupportedFrequencyValues
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

        protected void btnNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;
            bool success = false;

            FinancialTransaction transaction = null;
            FinancialScheduledTransaction scheduledTransaction = null;

            // Figure out if this is a one-time transaction or a future scheduled transaction
            bool repeating = _showRepeatingOptions;
            if ( repeating )
            {
                // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
                int oneTimeFrequencyId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() &&
                    dtpStartDate.SelectedDate == DateTime.Today )
                {
                    repeating = false;
                }
            }

            if ( repeating )
            {
                if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > DateTime.Today )
                {
                    scheduledTransaction = new FinancialScheduledTransaction();
                    scheduledTransaction.TransactionFrequencyValueId = btnFrequency.SelectedValueAsId().Value;
                    scheduledTransaction.StartDate = dtpStartDate.SelectedDate.Value;
                }
                else
                {
                    ShowMessage( NotificationBoxType.Error, "Payment Error", "First Gift must be a future date" );
                }
            }
            else
            {
                transaction = new FinancialTransaction();
            }

            if ( hfPaymentTab.Value == "CreditCard" )
            {
                var cc = new CreditCard( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
                cc.Amount = 0.02M;
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

                if ( repeating )
                {
                    scheduledTransaction.GatewayEntityTypeId = _ccGateway.TypeId;
                    success = _ccGateway.CreateScheduledTransaction( scheduledTransaction, cc, out errorMessage );
                }
                else
                {
                    transaction.GatewayEntityTypeId = _ccGateway.TypeId;
                    success = _ccGateway.Charge( transaction, cc, out errorMessage );
                }
            }
            else
            {
                var ach = new BankAccount( txtAccountNumber.Text, txtRoutingNumber.Text, rblAccountType.SelectedValue == "Savings" ? BankAccountType.Savings : BankAccountType.Checking );
                ach.Amount = 0.02M;
                ach.BankName = txtBankName.Text;

                if ( repeating )
                {
                    scheduledTransaction.GatewayEntityTypeId = _achGateway.TypeId;
                    success = _ccGateway.CreateScheduledTransaction( scheduledTransaction, ach, out errorMessage );
                }
                else
                {
                    transaction.GatewayEntityTypeId = _achGateway.TypeId;
                    success = _achGateway.Charge( transaction, ach, out errorMessage );
                }
            }

            if ( success )
            {
                string message = string.Empty;

                if ( repeating )
                {
                    message = "Profile ID: " + scheduledTransaction.TransactionCode;
                }
                else
                {
                    message = "Transaction Code: " + transaction.TransactionCode;
                }

                ShowMessage( NotificationBoxType.Success, "Success", message );
            }
            else
            {
                ShowMessage( NotificationBoxType.Error, "Payment Error", errorMessage );
            }
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

        $('#ButtonDropDown_btnFrequency .dropdown-menu a').click( function () {{
            var $lbl = $(this).parents('div.control-group').next().children('div.control-label');
            if ($(this).attr('data-id') == '{2}') {{
                $lbl.html('When');
            }} else {{
                $lbl.html('First Gift');
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

        protected void btnTest_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;

            var dt = _ccGateway.DownloadNewTransactions( 
                new DateTime(2013, 8, 29), new DateTime(2013, 8, 30), out errorMessage );
            if ( dt != null )
            {
                gReport.AutoGenerateColumns = true;
                gReport.DataSource = dt;
                gReport.DataBind();
                pnlReport.Visible = true;
            }
            else
            {
                ShowMessage( NotificationBoxType.Error, "Report Error", errorMessage );
            }
        }
    }
}

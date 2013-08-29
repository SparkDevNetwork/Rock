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

    [CustomDropdownListField( "Layout Style", "How the sections of this page should be displayed", "Vertical,Fluid", false, "Vertical", "Display Options", 2 )]
    [BooleanField( "Prompt for Email", "Should the user be prompted for their email address?", true, "Display Options", 4, "DisplayEmail" )]
    [BooleanField( "Prompt for Phone", "Should the user be prompted for their phone number?", false, "Display Options", 5, "DisplayPhone" )]

    public partial class GatewayTest : Rock.Web.UI.RockBlock
    {
        protected bool FluidLayout = false;
        
        private GatewayComponent _ccGateway;
        private GatewayComponent _achGateway;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Display Options
            bool display = false;
            txtEmail.Visible = bool.TryParse( GetAttributeValue( "DisplayEmail" ), out display ) && display;
            txtPhone.Visible = bool.TryParse( GetAttributeValue( "DisplayPhone" ), out display ) && display;
            FluidLayout = GetAttributeValue( "LayoutStyle" ) == "Fluid";

            bool ccEnabled = false;
            bool achEnabled = false;

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

            if ( ccEnabled || achEnabled)
            {
                if (ccEnabled && achEnabled)
                {
                    phPills.Visible = true;
                    divCCPaymentInfo.AddCssClass( "tab-pane" );
                    divACHPaymentInfo.AddCssClass( "tab-pane" );

                    hfPaymentTab.Value = "CreditCard";
                }
                else
                {
                    hfPaymentTab.Value = "ACH";
                }

                divCCPaymentInfo.Visible  = ccEnabled;
                divACHPaymentInfo.Visible = achEnabled;
            }
            else
            {
                pnlPaymentInfo.Visible = false;
                errorBox.Text = "Please check the configuration of this block and make sure a valid Credit Card and/or ACH Finacial Gateway has been selected.";
                errorBox.NotificationBoxType = NotificationBoxType.Error;
                errorBox.Visible = true;
            }

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
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
        }
        /// <summary>
        /// Binds the saved accounts.
        /// </summary>
        private void BindSavedAccounts()
        {
            rblSavedCC.Items.Clear();

            if (CurrentPersonId.HasValue)
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService()
                    .GetByPersonId(CurrentPersonId.Value).ToList();

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

            string script = string.Format( @"
    Sys.Application.add_load(function () {{

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
", divCCPaymentInfo.ClientID, hfPaymentTab.ClientID );

            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
        }

        protected void btnNext_Click( object sender, EventArgs e )
        {

        }
}        
}

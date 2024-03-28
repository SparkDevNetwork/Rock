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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Financial;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.NMI.Controls
{
    /// <summary>
    /// Control for hosting the NMI Gateway Payment Info HTML and scripts
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="System.Web.UI.INamingContainer" />
    public class NMIHostedPaymentControl : CompositeControl, INamingContainer, IHostedGatewayPaymentControlTokenEvent, IHostedGatewayPaymentControlCurrencyTypeEvent
    {
        #region Keys/Constants

        private static class ViewStateKey
        {
            public const string EnabledPaymentTypes = "EnabledPaymentTypes";
        }

        private static class PostbackKey
        {
            public const string TokenizerPostback = "TokenizerPostback";
            public const string CurrencyTypeChange = "CurrencyTypeChange";
        }

        #endregion Keys/Constants

        #region Private Control Declarations

        private HiddenFieldWithClass _hfPaymentInfoToken;
        private HiddenFieldWithClass _hfCollectJSRawResponse;
        private HiddenFieldWithClass _hfEnabledPaymentTypesJSON;
        private HiddenFieldWithClass _hfSelectedPaymentType;

        private HtmlGenericControl _divCreditCardNumber;
        private HtmlGenericControl _divCreditCardBreak;
        private HtmlGenericControl _divCreditCardExp;
        private HtmlGenericControl _divCreditCardCVV;
        private HtmlGenericControl _divCheckAccountNumber;
        private HtmlGenericControl _divCheckRoutingNumber;
        private HtmlGenericControl _divCheckFullName;
        private HtmlGenericControl _aPaymentButton;
        private HtmlGenericControl _divValidationMessage;

        private TextBox _tbCardFirstName;
        private TextBox _tbCardLastName;
        private HtmlGenericControl _divBillingAddressOption;
        private CheckBox _cbUseBillingAddress;
        private HtmlGenericControl _divBillingAddress;
        private AddressControl _acBillingAddress;

        private TextBox _hiddenInputStyleHook;

        private HtmlGenericControl _divInputInvalid;

        private Panel _paymentTypeSelector;
        private Panel _gatewayCreditCardContainer;
        private Panel _gatewayACHContainer;

        #endregion Private Control Declarations

        #region IHostedGatewayPaymentControlTokenEvent

        /// <summary>
        /// Occurs when a payment token is received from the hosted gateway
        /// </summary>
        public event EventHandler<HostedGatewayPaymentControlTokenEventArgs> TokenReceived;

        #endregion IHostedGatewayPaymentControlTokenEvent

        #region IHostedGatewayPaymentControlCurrencyTypeEvent

        /// <summary>
        /// Occurs when the CurrencyType option is changed (ACH or CreditCard)
        /// </summary>
        public event EventHandler<HostedGatewayPaymentControlCurrencyTypeEventArgs> CurrencyTypeChange;

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        /// <value>
        /// The currency type value.
        /// </value>
        public DefinedValueCache CurrencyTypeValue
        {
            get
            {
                EnsureChildControls();

                if ( this.SelectedPaymentType == NMIPaymentType.ach )
                {
                    return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                }

                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
            }
        }

        #endregion IHostedGatewayPaymentControlCurrencyTypeEvent

        #region Public Properties

        /// <summary>
        /// Gets or sets the enabled payment types.
        /// </summary>
        /// <value>
        /// The enabled payment types.
        /// </value>
        public NMIPaymentType[] EnabledPaymentTypes
        {
            set
            {
                ViewState[ViewStateKey.EnabledPaymentTypes] = value;
            }

            private get
            {
                return ViewState[ViewStateKey.EnabledPaymentTypes] as NMIPaymentType[];
            }
        }

        /// <summary>
        /// Gets or sets the tokenization key.
        /// </summary>
        /// <value>
        /// The tokenization key.
        /// </value>
        public string TokenizationKey { private get; set; }

        /// <summary>
        /// Gets or sets the nmi gateway.
        /// </summary>
        /// <value>
        /// The nmi gateway.
        /// </value>
        public Gateway NMIGateway
        {
            private get
            {
                return _nmiGateway;
            }
            set
            {
                _nmiGateway = value;
            }
        }
        private Gateway _nmiGateway;

        /// <summary>
        /// Gets the payment information token.
        /// </summary>
        /// <value>
        /// The payment information token.
        /// </value>
        public string PaymentInfoToken
        {
            get
            {
                EnsureChildControls();
                return _hfPaymentInfoToken.Value;
            }
        }

        /// <summary>
        /// Gets the payment information token raw.
        /// </summary>
        /// <value>
        /// The payment information token raw.
        /// </value>
        public string PaymentInfoTokenRaw
        {
            get
            {
                EnsureChildControls();
                return _hfCollectJSRawResponse.Value;
            }
        }

        /// <summary>
        /// Gets the selected payment type.
        /// </summary>
        /// <value>
        /// The selected payment type.
        /// </value>
        public NMIPaymentType SelectedPaymentType
        {
            get
            {
                EnsureChildControls();

                var selectedPaymentType = _hfSelectedPaymentType.Value.ConvertToEnumOrNull<NMIPaymentType>();

                if ( selectedPaymentType == null )
                {
                    if ( EnabledPaymentTypes.Contains( NMIPaymentType.card ) )
                    {
                        selectedPaymentType = NMIPaymentType.card;
                    }
                    else
                    {
                        selectedPaymentType = NMIPaymentType.ach;
                    }
                }

                return selectedPaymentType.Value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether this payment control should prompt for billing address.
        /// </summary>
        /// <value>
        /// A flag indicating whether this payment control should prompt for billing address.
        /// </value>
        public bool PromptForBillingAddress
        {
            get
            {
                return _promptForBillingAddress;
            }
            set
            {
                _promptForBillingAddress = value;
            }
        }
        private bool _promptForBillingAddress;

        /// <summary>
        /// Gets or sets a flag indicating whether this payment control should prompt for name on card.
        /// </summary>
        /// <value>
        /// A flag indicating whether this payment control should prompt for name on card.
        /// </value>
        public bool PromptForNameOnCard
        {
            get
            {
                return _promptForNameOnCard;
            }
            set
            {
                _promptForNameOnCard = value;
            }
        }
        private bool _promptForNameOnCard;

        /// <summary>
        /// Gets a value indicating whether the card holder name input is valid.
        /// </summary>
        public bool CardHolderNameIsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace( this.CardFirstName ) && !string.IsNullOrWhiteSpace( this.CardLastName );
            }
        }

        /// <summary>
        /// Gets the card first name.
        /// </summary>
        public string CardFirstName
        {
            get
            {
                EnsureChildControls();
                return _tbCardFirstName.Text;
            }
        }

        /// <summary>
        /// Gets the card last name.
        /// </summary>
        public string CardLastName
        {
            get
            {
                EnsureChildControls();
                return _tbCardLastName.Text;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user specified a billing address to use.
        /// </summary>
        public bool UseBillingAddress
        {
            get
            {
                EnsureChildControls();
                return PromptForBillingAddress && _cbUseBillingAddress.Checked;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the billing address input is valid.
        /// </summary>
        public bool BillingAddressIsValid
        {
            get
            {
                var isRequired = this.UseBillingAddress;
                _acBillingAddress.Required = isRequired;
                _acBillingAddress.Validate( out _ );
                return _acBillingAddress.IsValid;
            }
        }

        /// <summary>
        /// Gets the billing address street 1.
        /// </summary>
        public string BillingAddressStreet1
        {
            get
            {
                EnsureChildControls();
                return _acBillingAddress.Street1;
            }
        }

        /// <summary>
        /// Gets the billing address street 2.
        /// </summary>
        public string BillingAddressStreet2
        {
            get
            {
                EnsureChildControls();
                return _acBillingAddress.Street2;
            }
        }

        /// <summary>
        /// Gets the billing address city.
        /// </summary>
        public string BillingAddressCity
        {
            get
            {
                EnsureChildControls();
                return _acBillingAddress.City;
            }
        }

        /// <summary>
        /// Gets the billing address state.
        /// </summary>
        public string BillingAddressState
        {
            get
            {
                EnsureChildControls();
                return _acBillingAddress.State;
            }
        }

        /// <summary>
        /// Gets the billing address postal code.
        /// </summary>
        public string BillingAddressPostalCode
        {
            get
            {
                EnsureChildControls();
                return _acBillingAddress.PostalCode;
            }
        }

        /// <summary>
        /// Gets the billing address country.
        /// </summary>
        /// <value>
        /// The asdf.
        /// </value>
        public string BillingAddressCountry
        {
            get
            {
                EnsureChildControls();
                return _acBillingAddress.Country;
            }
        }

        #endregion Public Properties

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // Script that lets us use the CollectJS API (see https://secure.nmi.com/merchants/resources/integration/integration_portal.php?#cjs_methodology)
            var additionalAttributes = new Dictionary<string, string>();
            additionalAttributes.Add( "data-tokenization-key", this.TokenizationKey );
            additionalAttributes.Add( "data-variant", "inline" );
            RockPage.AddScriptSrcToHead( this.Page, "nmiCollectJS", $"https://secure.nmi.com/token/Collect.js", additionalAttributes );
            RockPage.AddStyleToHead( this.Page, "nmiCollectJSCSS", Css.gatewayCollect );

            // Script that contains the initializeTokenizer scripts for us to use on the client
            if ( !Page.IsPostBack )
            {
                ScriptManager.RegisterClientScriptBlock( this, this.GetType(), "nmiGatewayCollectJSBlock", Scripts.gatewayCollectJS, true );
            }

            ScriptManager.RegisterStartupScript( this, this.GetType(), "nmiGatewayCollectJSStartup", $"Rock.NMI.controls.gatewayCollectJS.initialize('{this.ClientID}');", true );

            base.OnInit( e );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            var updatePanel = this.ParentUpdatePanel();
            string postbackControlId;
            if ( updatePanel != null )
            {
                postbackControlId = updatePanel.ClientID;
            }
            else
            {
                postbackControlId = this.ID;
            }

            if ( TokenReceived != null )
            {
                this.Attributes["data-tokenizer-postback-script"] = $"javascript:__doPostBack('{postbackControlId}', '{this.ID}=TokenizerPostback')";
            }

            if ( CurrencyTypeChange != null )
            {
                this.Attributes["data-currencychange-postback-script"] = $"javascript:__doPostBack('{postbackControlId}', '{this.ID}={PostbackKey.CurrencyTypeChange}')";
            }

            base.Render( writer );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.Page.IsPostBack )
            {
                HandleCustomPostbackEvents();
            }
        }

        /// <summary>
        /// Handles the custom postback events.
        /// </summary>
        private void HandleCustomPostbackEvents()
        {
            string[] eventArgs = ( this.Page.Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( new[] { "=" }, StringSplitOptions.RemoveEmptyEntries );

            if ( eventArgs.Length < 2 )
            {
                // Nothing custom is in postback.
                return;
            }

            if ( eventArgs[0] != this.ID )
            {
                // Not from this control.
                return;
            }

            // The gatewayCollect script will pass back '{this.ID}=TokenizerPostback' in a postback. If so, we know this is a postback from that
            // For currency changes (ACH vs Card) The gatewayCollect script will pass back '{this.ID}=CurrencyTypeChange' in a postback. If so, we know this is a postback from that.
            if ( eventArgs[1] == "TokenizerPostback" )
            {
                HandleTokenizerPostback();
            }
            else if ( eventArgs[1] == PostbackKey.CurrencyTypeChange )
            {
                HandleCurrencyTypeChangePostback();
            }
        }

        /// <summary>
        /// Handles the currency type change postback.
        /// </summary>
        private void HandleCurrencyTypeChangePostback()
        {
            CurrencyTypeChange?.Invoke( this, new HostedGatewayPaymentControlCurrencyTypeEventArgs { hostedGatewayPaymentControl = this } );
        }

        /// <summary>
        /// Handles the tokenizer postback.
        /// </summary>
        private void HandleTokenizerPostback()
        {
            HostedGatewayPaymentControlTokenEventArgs hostedGatewayPaymentControlTokenEventArgs = new HostedGatewayPaymentControlTokenEventArgs();

            var tokenResponse = PaymentInfoTokenRaw.FromJsonOrNull<TokenizerResponse>();

            if ( tokenResponse?.IsSuccessStatus() != true )
            {
                hostedGatewayPaymentControlTokenEventArgs.IsValid = false;

                if ( tokenResponse.HasValidationError() )
                {
                    hostedGatewayPaymentControlTokenEventArgs.ErrorMessage = FriendlyMessageHelper.GetFriendlyMessage( tokenResponse.ValidationMessage );
                }
                else
                {
                    hostedGatewayPaymentControlTokenEventArgs.ErrorMessage = FriendlyMessageHelper.GetFriendlyMessage( tokenResponse?.ErrorMessage ?? "null response from GetHostedPaymentInfoToken" );
                }
            }
            else
            {
                hostedGatewayPaymentControlTokenEventArgs.IsValid = true;
                hostedGatewayPaymentControlTokenEventArgs.ErrorMessage = null;
            }

            hostedGatewayPaymentControlTokenEventArgs.Token = _hfPaymentInfoToken.Value;

            TokenReceived?.Invoke( this, hostedGatewayPaymentControlTokenEventArgs );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            _hfPaymentInfoToken = new HiddenFieldWithClass() { ID = "_hfPaymentInfoToken", CssClass = "js-response-token" };
            Controls.Add( _hfPaymentInfoToken );

            _hfCollectJSRawResponse = new HiddenFieldWithClass() { ID = "_hfTokenizerRawResponse", CssClass = "js-tokenizer-raw-response" };
            Controls.Add( _hfCollectJSRawResponse );

            _hfEnabledPaymentTypesJSON = new HiddenFieldWithClass() { ID = "_hfEnabledPaymentTypesJSON", CssClass = "js-enabled-payment-types" };
            Controls.Add( _hfEnabledPaymentTypesJSON );

            _hfEnabledPaymentTypesJSON.Value = this.EnabledPaymentTypes.ToJson();

            // This will have 'ach' or 'card' as a value.
            _hfSelectedPaymentType = new HiddenFieldWithClass() { ID = "_hfSelectedPaymentType", CssClass = "js-selected-payment-type" };
            Controls.Add( _hfSelectedPaymentType );


            /* Payment Type Selector*/
            if ( EnabledPaymentTypes.Length > 1 )
            {
                Literal lPaymentSelectorHTML = new Literal() { ID = "lPaymentSelectorHTML" };

                lPaymentSelectorHTML.Text = $@"
<div class='gateway-type-selector btn-group btn-group-justified' role='group'>
    <a class='btn btn-primary active js-payment-creditcard payment-creditcard' runat='server'>
        Card
    </a>
    <a class='btn btn-default js-payment-ach payment-ach' runat='server'>
        Bank Account
    </a>
</div>";

                _paymentTypeSelector = new Panel() { ID = "_paymentTypeSelector", CssClass = "js-gateway-paymenttype-selector gateway-paymenttype-selector" };
                _paymentTypeSelector.Controls.Add( lPaymentSelectorHTML );
                Controls.Add( _paymentTypeSelector );
            }

            var pnlPaymentInputs = new Panel { ID = "pnlPaymentInputs", CssClass = "js-nmi-payment-inputs nmi-payment-inputs" };

            /* Credit Card Inputs */
            if ( EnabledPaymentTypes.Contains( NMIPaymentType.card ) )
            {
                _gatewayCreditCardContainer = new Panel() { ID = "_gatewayCreditCardContainer", CssClass = "gateway-creditcard-container gateway-payment-container js-gateway-creditcard-container" };
                pnlPaymentInputs.Controls.Add( _gatewayCreditCardContainer );

                if ( this.PromptForNameOnCard || this.PromptForBillingAddress )
                {
                    string nameInputStyles = @"
                        .credit-card-name-input {
                            background-color: rgb(255, 255, 255);
                            border-width: 0.666667px;
                            border-style: solid;
                            border-radius: 4px;
                            border-color: rgb(204, 204, 204);
                            color: rgb(85, 85, 85);
                            font-family: system-ui, -apple-system, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, ""Noto Sans"", ""Liberation Sans"", sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"", ""Noto Color Emoji"";
                            font-size: 14px;
                            height: 34px;
                            margin-top: 0px;
                            margin-bottom: 5px;
                            padding: 6px 12px;
                            width: 100%;
                        }

                        .credit-card-name-input:focus {
                            border-color: rgb(102, 175, 233) !important;
                            outline-style: none !important;
                        }

                        .address-container {
                            margin-bottom: 10px;
                            padding: 0 3px;
                            overflow: hidden;
                            width: 100%;
                        }

                        .address-input {
                        }";

                    var inlineStyles = new HtmlGenericControl( "style" );
                    inlineStyles.Attributes.Add( "scoped", "scoped" );
                    inlineStyles.InnerText = nameInputStyles;
                    _gatewayCreditCardContainer.Controls.Add( inlineStyles );
                }

                var divCardFirstName = new HtmlGenericControl( "div" )
                {
                    Visible = this.PromptForNameOnCard
                };
                divCardFirstName.AddCssClass( "iframe-input credit-card-input" );
                _tbCardFirstName = new TextBox
                {
                    ID = "_tbCardFirstName",
                    CssClass = "js-creditcard-firstname credit-card-name-input",
                    Visible = this.PromptForNameOnCard
                };
                _tbCardFirstName.Attributes.Add( "placeholder", "First Name on Card" );
                divCardFirstName.Controls.Add( _tbCardFirstName );
                _gatewayCreditCardContainer.Controls.Add( divCardFirstName );

                var divCardLastName = new HtmlGenericControl( "div" )
                {
                    Visible = this.PromptForNameOnCard
                };
                divCardLastName.AddCssClass( "iframe-input credit-card-input" );
                _tbCardLastName = new TextBox
                {
                    ID = "_tbCardLastName",
                    CssClass = "js-creditcard-lastname credit-card-name-input",
                    Visible = this.PromptForNameOnCard
                };
                _tbCardLastName.Attributes.Add( "placeholder", "Last Name on Card" );
                divCardLastName.Controls.Add( _tbCardLastName );
                _gatewayCreditCardContainer.Controls.Add( divCardLastName );

                if ( this.PromptForNameOnCard )
                {
                    var breakDiv = new HtmlGenericControl( "div" );
                    breakDiv.Attributes.Add( "style", "flex-basis:100%;height:0" );
                    _gatewayCreditCardContainer.Controls.Add( breakDiv );
                }

                _divCreditCardNumber = new HtmlGenericControl( "div" );
                _divCreditCardNumber.AddCssClass( "js-credit-card-input iframe-input credit-card-input" );
                _gatewayCreditCardContainer.Controls.Add( _divCreditCardNumber );

                _divCreditCardBreak = new HtmlGenericControl( "div" );
                _divCreditCardBreak.AddCssClass( "break" );
                _gatewayCreditCardContainer.Controls.Add( _divCreditCardBreak );

                _divCreditCardExp = new HtmlGenericControl( "div" );
                _divCreditCardExp.AddCssClass( "js-credit-card-exp-input iframe-input credit-card-exp-input" );
                _gatewayCreditCardContainer.Controls.Add( _divCreditCardExp );

                _divCreditCardCVV = new HtmlGenericControl( "div" );
                _divCreditCardCVV.AddCssClass( "js-credit-card-cvv-input iframe-input credit-card-cvv-input" );
                _gatewayCreditCardContainer.Controls.Add( _divCreditCardCVV );

                if ( this.PromptForBillingAddress )
                {
                    var breakDiv = new HtmlGenericControl( "div" );
                    breakDiv.Attributes.Add( "style", "flex-basis:100%;height:0" );
                    _gatewayCreditCardContainer.Controls.Add( breakDiv );

                    string billingAddressControlScript = @"
// Hide or show a div based on selection of checkbox
$('input:checkbox.toggle-input').unbind('click').on('click', function () {{
    $(this).parents('.checkbox').next('.toggle-content').slideToggle();
}});

$('input:checkbox.toggle-input').each(function () {
    if ($(this).prop('checked')) {
        $(this).parents('.checkbox').next('.toggle-content').show();
    }
    else {
        $(this).parents('.checkbox').next('.toggle-content').hide();
    }
});
";

                    ScriptManager.RegisterStartupScript( this, this.GetType(), "nmi-billing-address", billingAddressControlScript, true );
                }

                _divBillingAddressOption = new HtmlGenericControl( "div" )
                {
                    Visible = this._promptForBillingAddress
                };
                _divBillingAddressOption.AddCssClass( "address-container" );
                _cbUseBillingAddress = new CheckBox
                {
                    ID = "_cbUseBillingAddress",
                    CssClass = "toggle-input js-billing-address-checkbox",
                    Text = "Enter a different billing address",
                    Visible = this.PromptForBillingAddress
                };
                _divBillingAddressOption.Controls.Add( _cbUseBillingAddress );

                _divBillingAddress = new HtmlGenericControl( "div" );
                _divBillingAddress.AddCssClass( "toggle-content" );
                _acBillingAddress = new AddressControl
                {
                    ID = "acBillingAddress",
                    UseStateAbbreviation = true,
                    UseCountryAbbreviation = false,
                    CssClass = "js-billingaddress-control",
                    Visible = this.PromptForBillingAddress
                };
                _divBillingAddress.Controls.Add( _acBillingAddress );
                _divBillingAddressOption.Controls.Add( _divBillingAddress );
                _gatewayCreditCardContainer.Controls.Add( _divBillingAddressOption );
            }

            /* ACH Inputs */
            if ( EnabledPaymentTypes.Contains( NMIPaymentType.ach ) )
            {
                _gatewayACHContainer = new Panel() { ID = "_gatewayACHContainer", CssClass = "gateway-ach-container gateway-payment-container js-gateway-ach-container" };
                pnlPaymentInputs.Controls.Add( _gatewayACHContainer );

                _divCheckAccountNumber = new HtmlGenericControl( "div" );
                _divCheckAccountNumber.AddCssClass( "js-check-account-number-input iframe-input check-account-number-input" );
                _gatewayACHContainer.Controls.Add( _divCheckAccountNumber );

                _divCheckRoutingNumber = new HtmlGenericControl( "div" );
                _divCheckRoutingNumber.AddCssClass( "js-check-routing-number-input iframe-input check-routing-number-input" );
                _gatewayACHContainer.Controls.Add( _divCheckRoutingNumber );

                _divCheckFullName = new HtmlGenericControl( "div" );
                _divCheckFullName.AddCssClass( "js-check-fullname-input iframe-input check-fullname-input" );
                _gatewayACHContainer.Controls.Add( _divCheckFullName );
            }

            /* Submit Payment */

            // The collectJs script needs a payment button to work, so add it but don't show it.
            _aPaymentButton = new HtmlGenericControl( "button" );
            _aPaymentButton.Attributes["type"] = "button";
            _aPaymentButton.Style[HtmlTextWriterStyle.Display] = "none";
            _aPaymentButton.AddCssClass( "js-payment-button payment-button" );
            pnlPaymentInputs.Controls.Add( _aPaymentButton );

            Controls.Add( pnlPaymentInputs );

            _divValidationMessage = new HtmlGenericControl( "div" );
            _divValidationMessage.AddCssClass( "alert alert-validation js-payment-input-validation" );
            _divValidationMessage.InnerHtml =
@"<span class='js-validation-message'></span>";
            this.Controls.Add( _divValidationMessage );

            _hiddenInputStyleHook = new TextBox();
            _hiddenInputStyleHook.Attributes["class"] = "js-input-style-hook form-control nmi-input-style-hook form-group";
            _hiddenInputStyleHook.Style["display"] = "none";

            Controls.Add( _hiddenInputStyleHook );

            _divInputInvalid = new HtmlGenericControl( "div" );
            _divInputInvalid.AddCssClass( "form-group has-error" );
            _divInputInvalid.InnerHtml =
@"<input type='text' class='js-input-invalid-style-hook form-control'>";
            _divInputInvalid.Style["display"] = "none";
            Controls.Add( _divInputInvalid );
        }

        #endregion Control Methods
    }
}

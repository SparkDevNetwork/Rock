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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Financial;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.MyWell.Controls
{
    /// <summary>
    /// Control for hosting the MyWell Gateway Payment Info HTML and scripts
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="System.Web.UI.INamingContainer" />
    public class MyWellHostedPaymentControl : CompositeControl,
        INamingContainer,
        Rock.Financial.IHostedGatewayPaymentControlTokenEvent,
        Rock.Financial.IHostedGatewayPaymentControlCurrencyTypeEvent
    {
        private static class PostbackKey
        {
            public const string TokenizerPostback = "TokenizerPostback";
            public const string CurrencyTypeChange = "CurrencyTypeChange";
        }

        #region Controls

        private HiddenFieldWithClass _hfPaymentInfoToken;
        private HiddenFieldWithClass _hfTokenizerRawResponse;
        private HiddenFieldWithClass _hfEnabledPaymentTypesJSON;
        private HiddenFieldWithClass _hfSelectedPaymentType;
        private HiddenFieldWithClass _hfPublicApiKey;
        private HiddenFieldWithClass _hfGatewayUrl;
        private TextBox _hiddenInputStyleHook;
        private Panel _paymentTypeSelector;
        private Panel _gatewayCreditCardIFrameContainer;
        private Panel _gatewayACHIFrameContainer;

        #endregion

        private MyWellGateway _myWellGateway;

        #region IHostedGatewayPaymentControlTokenEvent

        /// <summary>
        /// Occurs when a payment token is received from the hosted gateway
        /// </summary>
        public event EventHandler<Rock.Financial.HostedGatewayPaymentControlTokenEventArgs> TokenReceived;

        #endregion IHostedGatewayPaymentControlTokenEvent

        #region Rock.Financial.IHostedGatewayPaymentControlCurrencyTypeEvent

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
                var currencyTypeValue = _hfSelectedPaymentType.Value.ConvertToEnumOrNull<MyWellPaymentType>();
                if ( currencyTypeValue == null )
                {
                    if ( EnabledPaymentTypes.Contains( MyWellPaymentType.card ) )
                    {
                        currencyTypeValue = MyWellPaymentType.card;
                    }
                    else
                    {
                        currencyTypeValue = MyWellPaymentType.ach;
                    }
                }

                if ( currencyTypeValue == MyWellPaymentType.ach )
                {
                    return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                }

                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
            }
        }

        #endregion Rock.Financial.IHostedGatewayPaymentControlCurrencyTypeEvent        

        /// <summary>
        /// Gets or sets the gateway base URL.
        /// </summary>
        /// <value>
        /// The gateway base URL.
        /// </value>
        public string GatewayBaseUrl
        {
            get
            {
                EnsureChildControls();
                return _hfGatewayUrl.Value;
            }

            set
            {
                EnsureChildControls();
                _hfGatewayUrl.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the enabled payment types.
        /// </summary>
        /// <value>
        /// The enabled payment types.
        /// </value>
        public MyWellPaymentType[] EnabledPaymentTypes
        {
            set
            {
                EnsureChildControls();
                _hfEnabledPaymentTypesJSON.Value = value.Select( a => a.ConvertToString() ).ToJson();
            }

            private get
            {
                EnsureChildControls();
                return _hfEnabledPaymentTypesJSON.Value?.FromJsonOrNull<MyWellPaymentType[]>();
            }
        }

        /// <summary>
        /// Sets the public API key.
        /// </summary>
        /// <value>
        /// The public API key.
        /// </value>
        public string PublicApiKey
        {
            set
            {
                EnsureChildControls();
                _hfPublicApiKey.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the MyWell gateway.
        /// </summary>
        /// <value>
        /// The MyWell gateway.
        /// </value>
        public MyWellGateway MyWellGateway
        {
            private get
            {
                return _myWellGateway;
            }

            set
            {
                _myWellGateway = value;
            }
        }

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
                return _hfTokenizerRawResponse.Value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // Script that lets us use the Tokenizer API (see https://sandbox.gotnpgateway.com/docs/tokenizer/).
            RockPage.AddScriptSrcToHead( this.Page, "gotnpgatewayTokenizer", $"{GatewayBaseUrl}/tokenizer/tokenizer.js" );

            // Script that contains the initializeTokenizer scripts for us to use on the client.
            System.Web.UI.ScriptManager.RegisterClientScriptBlock( this, this.GetType(), "myWellGatewayTokenizerBlock", Scripts.gatewayTokenizer, true );

            System.Web.UI.ScriptManager.RegisterStartupScript( this, this.GetType(), "myWellGatewayTokenizerStartup", $"initializeTokenizer('{this.ClientID}');", true );

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
                this.Attributes["data-tokenizer-postback-script"] = $"javascript:__doPostBack('{postbackControlId}', '{this.ID}={PostbackKey.TokenizerPostback}')";
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

            // The gatewayTokenizer script will pass back '{this.ID}=TokenizerPostback' in a postback. If so, we know this is a postback from that.
            // For currency changes (ACH vs Card) The gatewayTokenizer script will pass back '{this.ID}=CurrencyTypeChange' in a postback. If so, we know this is a postback from that.
            if ( eventArgs[1] == PostbackKey.TokenizerPostback )
            {
                HandleTokenizePostback();
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
        /// Handles the tokenize postback.
        /// </summary>
        private void HandleTokenizePostback()
        {
            Rock.Financial.HostedGatewayPaymentControlTokenEventArgs hostedGatewayPaymentControlTokenEventArgs = new Financial.HostedGatewayPaymentControlTokenEventArgs();

            var tokenResponse = PaymentInfoTokenRaw.FromJsonOrNull<TokenizerResponse>();

            if ( tokenResponse?.IsSuccessStatus() != true )
            {
                hostedGatewayPaymentControlTokenEventArgs.IsValid = false;

                if ( tokenResponse.HasValidationError() )
                {
                    hostedGatewayPaymentControlTokenEventArgs.ErrorMessage = tokenResponse.ValidationMessage;
                }
                else
                {
                    hostedGatewayPaymentControlTokenEventArgs.ErrorMessage = tokenResponse?.Message ?? "null response from GetHostedPaymentInfoToken";
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

            _hfTokenizerRawResponse = new HiddenFieldWithClass() { ID = "_hfTokenizerRawResponse", CssClass = "js-tokenizer-raw-response" };
            Controls.Add( _hfTokenizerRawResponse );

            _hfEnabledPaymentTypesJSON = new HiddenFieldWithClass() { ID = "_hfEnabledPaymentTypesJSON", CssClass = "js-enabled-payment-types" };
            Controls.Add( _hfEnabledPaymentTypesJSON );

            // This will have 'ach' or 'card' as a value.
            _hfSelectedPaymentType = new HiddenFieldWithClass() { ID = "_hfSelectedPaymentType", CssClass = "js-selected-payment-type" };
            Controls.Add( _hfSelectedPaymentType );

            _hfPublicApiKey = new HiddenFieldWithClass() { ID = "_hfPublicApiKey", CssClass = "js-public-api-key" };
            Controls.Add( _hfPublicApiKey );

            _hiddenInputStyleHook = new TextBox();
            _hiddenInputStyleHook.Attributes["class"] = "js-input-style-hook form-control";
            _hiddenInputStyleHook.Style["display"] = "none";
            Controls.Add( _hiddenInputStyleHook );

            _hfGatewayUrl = new HiddenFieldWithClass() { ID = "_hfGatewayUrl", CssClass = "js-gateway-url" };
            Controls.Add( _hfGatewayUrl );

            _paymentTypeSelector = new Panel() { ID = "_paymentTypeSelector", CssClass = "js-gateway-paymenttype-selector gateway-paymenttype-selector" };
            Controls.Add( _paymentTypeSelector );

            Literal lPaymentSelectorHTML = new Literal() { ID = "lPaymentSelectorHTML" };

            lPaymentSelectorHTML.Text = $@"
<div class='gateway-type-selector btn-group btn-group-justified' role='group'>
    <a class='btn btn-primary active js-payment-creditcard payment-creditcard' runat='server'>
        Card
    </a>
    <a class='btn btn-default js-payment-ach payment-ach' runat='server'>
        Bank Account
    </a>
</div>
";

            _paymentTypeSelector.Controls.Add( lPaymentSelectorHTML );

            _gatewayCreditCardIFrameContainer = new Panel() { ID = "_gatewayCreditCardIFrameContainer", CssClass = "gateway-iframe-container js-gateway-creditcard-iframe-container" };
            Controls.Add( _gatewayCreditCardIFrameContainer );

            _gatewayACHIFrameContainer = new Panel() { ID = "_gatewayACHIFrameContainer", CssClass = "gateway-iframe-container js-gateway-ach-iframe-container" };
            Controls.Add( _gatewayACHIFrameContainer );
        }
    }
}

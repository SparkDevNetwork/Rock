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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.CurrencyBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:CurrencyBox runat=server></{0}:CurrencyBox>" )]
    public class CurrencyBox : NumberBoxBase
    {
        private readonly RockCurrencyCodeInfo _organizationCurrencyCodeInfo;
        
        private int _currencyDecimalPlaces;

        /// <summary>
        /// Gets or sets the currency code defined value identifier.
        /// </summary>
        /// <value>
        /// The currency code defined value identifier.
        /// </value>
        public int CurrencyCodeDefinedValueId
        {
            get => ( int ) ( ViewState["CurrencyCodeDefinedValueId"] ?? 0 );

            set
            {
                ViewState["CurrencyCodeDefinedValueId"] = value;
                UpdateCurrencyCode( value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the placeholder should be disabled. When disabled, the placeholder will be blank.
        /// </summary>
        /// <value>
        /// Sets the placeholder to an empty string when <see langword="true"/>;
        /// otherwise, uses the default placeholder value when <see langword="false"/>.
        /// </value>
        public bool DisablePlaceholder { get; set; }

        /// <summary>
        /// Updates the currency code.
        /// </summary>
        /// <param name="currencyCodeDefinedValueId">The currency code defined value identifier.</param>
        private void UpdateCurrencyCode( int currencyCodeDefinedValueId )
        {
            if ( currencyCodeDefinedValueId == 0 || currencyCodeDefinedValueId == _organizationCurrencyCodeInfo.CurrencyCodeDefinedValueId )
            {
                PrependText = _organizationCurrencyCodeInfo.Symbol;
                _currencyDecimalPlaces = _organizationCurrencyCodeInfo.DecimalPlaces;
                return;
            }

            var currencyCodeInfo = new RockCurrencyCodeInfo( currencyCodeDefinedValueId );
            PrependText = currencyCodeInfo.Symbol;

            _currencyDecimalPlaces = currencyCodeInfo.DecimalPlaces;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyBox" /> class.
        /// </summary>
        public CurrencyBox()
            : base()
        {
            this.NumberType = ValidationDataType.Currency;

            var organizationCurrencyCodeGuid = GlobalAttributesCache.Get().GetValue( SystemKey.SystemSetting.ORGANIZATION_CURRENCY_CODE ).AsGuidOrNull();
            if ( organizationCurrencyCodeGuid != null )
            {
                var organizationCurrencyCode = DefinedValueCache.Get( organizationCurrencyCodeGuid.Value );
                if ( organizationCurrencyCode != null )
                {
                    _organizationCurrencyCodeInfo = new RockCurrencyCodeInfo( organizationCurrencyCode.Id );
                }
            }

            UpdateCurrencyCode( 0 );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrencyCodeDefinedValueId > 0 )
            {
                UpdateCurrencyCode( CurrencyCodeDefinedValueId );
            }
            else
            {
                CurrencyCodeDefinedValueId = _organizationCurrencyCodeInfo.CurrencyCodeDefinedValueId;
            }
        }

        /// <summary>
        /// Sets the behavior mode of the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to number.
        /// </summary>
        public override TextBoxMode TextMode
        {
            get
            {
                return TextBoxMode.Number;
            }
        }

        /// <summary>
        /// Renders the base control and allows a decimal keypad to show on mobile keyboards
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( _currencyDecimalPlaces == 0 )
            {
                this.NumberType = ValidationDataType.Integer;
                this.Placeholder = this.DisablePlaceholder ? string.Empty : "0";
                this.Attributes.Remove( "step" );
            }
            else
            {
                var decimalSeperator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                this.Placeholder = this.DisablePlaceholder ? string.Empty : $"0{decimalSeperator}{ new string( '0', _currencyDecimalPlaces ) }";

                var step = $"0.{ new string( '0', _currencyDecimalPlaces - 1 ) }1";
                this.Attributes["step"] = step;
            }
            
            /* 2020-11-20 MDP
               inputmode tells the browser what type of input to expect. If we 
               see https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/inputmode

               This fixes an issue where some browsers (especially mobile phones) would allow non-decimal characters to be allowed in the input box
            */
            this.Attributes["inputmode"] = "decimal";

            base.RenderBaseControl( writer );
        }

        /// <summary>
        /// Gets or sets the currency value
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Value
        {
            get
            {
                return this.Text.AsDecimalInvariantCultureOrNull();
            }

            set
            {
                this.Text = value?.ToString( $"F{_currencyDecimalPlaces}", System.Globalization.CultureInfo.InvariantCulture );
            }
        }
    }
}
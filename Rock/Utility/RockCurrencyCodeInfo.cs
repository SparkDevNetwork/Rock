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

using System;
using System.Web;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Rock class to facilitate currency handling.
    /// </summary>
    public class RockCurrencyCodeInfo
    {
        private const string DEFAULT_SYMBOL_LOCATION = "Left";
        private const string DEFAULT_SYMBOL = "$";
        private const int DEFAULT_DECIMAL_PLACES = 2;

        private readonly Guid? _organizationCurrencyCodeGuid;
        private Guid? _currencyCodeGuid;
        private int _currencyCodeDefinedValueId;

        /// <summary>
        /// Gets or sets the symbol location left or right.
        /// </summary>
        /// <value>
        /// The symbol location.
        /// </value>
        public string SymbolLocation { get; private set; }

        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        /// <value>
        /// The symbol.
        /// </value>
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets or sets the decimal places.
        /// </summary>
        /// <value>
        /// The decimal places.
        /// </value>
        public int DecimalPlaces { get; private set; }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        /// <value>
        /// The currency code.
        /// </value>
        public string CurrencyCode { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is organization currency.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is organization currency; otherwise, <c>false</c>.
        /// </value>
        public bool IsOrganizationCurrency { get => _organizationCurrencyCodeGuid == _currencyCodeGuid; }

        /// <summary>
        /// Gets the currency code defined value identifier.
        /// </summary>
        /// <value>
        /// The currency code defined value identifier.
        /// </value>
        public int CurrencyCodeDefinedValueId { get => _currencyCodeDefinedValueId; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCurrencyCodeInfo"/> class.
        /// </summary>
        public RockCurrencyCodeInfo()
        {
            _organizationCurrencyCodeGuid = GlobalAttributesCache.Get().GetValue( SystemKey.SystemSetting.ORGANIZATION_CURRENCY_CODE ).AsGuidOrNull();
            SetCurrencyCodeProperties( _organizationCurrencyCodeGuid );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCurrencyCodeInfo"/> class.
        /// </summary>
        /// <param name="currencyCodeDefinedValueId">The currency code defined value identifier.</param>
        public RockCurrencyCodeInfo( int? currencyCodeDefinedValueId )
        {
            _organizationCurrencyCodeGuid = GlobalAttributesCache.Get().GetValue( SystemKey.SystemSetting.ORGANIZATION_CURRENCY_CODE ).AsGuidOrNull();

            if ( currencyCodeDefinedValueId != null )
            {
                var currencyCodeDefinedValueCache = DefinedValueCache.Get( currencyCodeDefinedValueId.Value );
                if ( currencyCodeDefinedValueCache != null )
                {
                    SetCurrencyCodeProperties( currencyCodeDefinedValueCache );
                    return;
                }
            }

            SetCurrencyCodeProperties( _organizationCurrencyCodeGuid );
        }

        /// <summary>
        /// Sets the currency code properties.
        /// </summary>
        /// <param name="currencyCodeDefinedValueGuid">The currency code defined value unique identifier.</param>
        private void SetCurrencyCodeProperties( Guid? currencyCodeDefinedValueGuid )
        {
            if ( currencyCodeDefinedValueGuid != null )
            {
                SetCurrencyCodeProperties( DefinedValueCache.Get( _organizationCurrencyCodeGuid.Value ) );
            }
            else
            {
                SymbolLocation = DEFAULT_SYMBOL_LOCATION;
                Symbol = DEFAULT_SYMBOL;
                DecimalPlaces = DEFAULT_DECIMAL_PLACES;
            }
        }

        /// <summary>
        /// Sets the currency code properties.
        /// </summary>
        /// <param name="currencyCodeDefinedValueCache">The currency code defined value cache.</param>
        private void SetCurrencyCodeProperties( DefinedValueCache currencyCodeDefinedValueCache )
        {
            if ( currencyCodeDefinedValueCache != null )
            {
                _currencyCodeDefinedValueId = currencyCodeDefinedValueCache.Id;
                _currencyCodeGuid = currencyCodeDefinedValueCache.Guid;
                Symbol = currencyCodeDefinedValueCache.GetAttributeValue( "Symbol" ) ?? DEFAULT_SYMBOL;
                DecimalPlaces = currencyCodeDefinedValueCache.GetAttributeValue( "DecimalPlaces" ).AsIntegerOrNull() ?? DEFAULT_DECIMAL_PLACES;
                SymbolLocation = currencyCodeDefinedValueCache.GetAttributeValue( "Position" ) ?? DEFAULT_SYMBOL_LOCATION;
                CurrencyCode = currencyCodeDefinedValueCache.Value;
                Description = currencyCodeDefinedValueCache.Description;
            }
        }

        /// <summary>
        /// Gets the currency symbol.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrencySymbol()
        {
            return HttpUtility.HtmlDecode( new RockCurrencyCodeInfo().Symbol );
        }

        /// <summary>
        /// Gets the currency symbol.
        /// </summary>
        /// <returns></returns>
        public static int GetDecimalPlaces()
        {
            return new RockCurrencyCodeInfo().DecimalPlaces;
        }
    }
}

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
using Rock.Utility;

namespace Rock
{
    /// <summary>
    /// Object and Stream Extensions that don't require any nuget packages
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Object Extensions

        /// <summary>
        /// If input is a string that has been formatted as currency, return the decimal value. Otherwise return the object unchanged.
        /// </summary>
        /// <param name="input">A value that may be a currency-formatted string.</param>
        /// <returns></returns>
        public static object ReverseCurrencyFormatting( this object input )
        {
            return input.ReverseCurrencyFormatting( null );
        }

        /// <summary>
        /// Reverses the currency formatting.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="currencyCodeDefinedValueId">The currency code defined value identifier.</param>
        /// <returns></returns>
        public static object ReverseCurrencyFormatting( this object input, int? currencyCodeDefinedValueId )
        {
            var exportValueString = input as string;

            // If the object is a string...
            if ( exportValueString != null )
            {
                var currencyInfo = new RockCurrencyCodeInfo( currencyCodeDefinedValueId );
                var currencySymbol = currencyInfo.Symbol;

                // ... that contains the currency symbol ...
                if ( exportValueString.Contains( currencySymbol ) )
                {
                    var decimalString = exportValueString.Replace( currencySymbol, string.Empty );
                    decimal exportValueDecimal;

                    // ... and the value without the currency symbol is a valid decimal value ...
                    if ( decimal.TryParse( decimalString, out exportValueDecimal ) )
                    {
                        // ... that matches the input string when formatted as currency ...
                        if ( exportValueDecimal.FormatAsCurrency( currencyCodeDefinedValueId ) == exportValueString )
                        {
                            // ... return the input as a decimal
                            return exportValueDecimal;
                        }
                    }
                }
            }

            // Otherwise just return the input back out
            return input;
        }

        #endregion
    }
}

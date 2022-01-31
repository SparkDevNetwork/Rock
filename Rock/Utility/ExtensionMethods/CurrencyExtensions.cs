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
using System.Web;
using Rock.Utility;
using System;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Currency (decimal,double) Extensions

        /// <summary>
        /// Formats as currency using the Currency Code information from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this decimal value )
        {
            return value.FormatAsCurrency( null );
        }

        /// <summary>
        /// Formats as currency with specified number of decimal places or the currency's decimal places which ever is small.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <returns></returns>
        public static string FormatAsCurrencyWithDecimalPlaces( this decimal value, int decimalPlaces )
        {
            return value.FormatAsCurrency( null, decimalPlaces );
        }

        /// <summary>
        /// Formats as currency.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="currencyCodeDefinedValueId">The currency code defined value identifier.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this decimal value, int? currencyCodeDefinedValueId )
        {
            return value.FormatAsCurrency( currencyCodeDefinedValueId, null );
        }

        /// <summary>
        /// Formats as currency.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="currencyCodeDefinedValueId">The currency code defined value identifier.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this decimal value, int? currencyCodeDefinedValueId, int? decimalPlaces )
        {
            var currencyCodeInfo = new RockCurrencyCodeInfo( currencyCodeDefinedValueId );
            var currencyCode = currencyCodeInfo.IsOrganizationCurrency ? string.Empty : currencyCodeInfo.CurrencyCode;
            var currencySymbol = HttpUtility.HtmlDecode( currencyCodeInfo.Symbol );
            if ( decimalPlaces == null || decimalPlaces > currencyCodeInfo.DecimalPlaces )
            {
                decimalPlaces = currencyCodeInfo.DecimalPlaces;
            }

            if ( currencyCodeInfo.SymbolLocation.Equals( "left", System.StringComparison.OrdinalIgnoreCase ) )
            {
                return string.Format( "{2} {0}{1:N" + decimalPlaces.ToString() + "}", currencySymbol, value, currencyCode );
            }

            return string.Format( "{1:N" + decimalPlaces.ToString() + "}{0}{2}", currencySymbol, value, currencyCode );
        }

        /// <summary>
        /// Formats as currency using the Currency Code information from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this decimal? value )
        {
            if ( value.HasValue )
            {
                return value.Value.FormatAsCurrency();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats as currency using the Currency Code information from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this double value )
        {
            return ( ( decimal ) value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency using the Currency Code information from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this double? value )
        {
            return ( ( decimal? ) value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency using the Currency Code information from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this int value )
        {
            return ( ( decimal ) value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency using the Currency Code information from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this int? value )
        {
            return ( ( decimal? ) value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this string input )
        {
            return input.FormatAsCurrency( null );
        }

        /// <summary>
        /// Formats as currency.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="currencyCodeDefinedValueId">The currency code defined value identifier.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this string input, int? currencyCodeDefinedValueId )
        {
            var exportValueString = input;

            if ( exportValueString != null )
            {
                var currencyInfo = new RockCurrencyCodeInfo( currencyCodeDefinedValueId );
                var currencySymbol = currencyInfo.Symbol;
                var decimalString = exportValueString;

                // Remove currency symbol if exist.
                if ( exportValueString.Contains( currencySymbol ) )
                {
                    decimalString = exportValueString.Replace( currencySymbol, string.Empty );
                }

                decimal exportValueDecimal;

                // Try to parse the string as decimal
                if ( decimal.TryParse( decimalString, out exportValueDecimal ) )
                {
                    // return the input as a currency formatted string.
                    return exportValueDecimal.FormatAsCurrency();
                }
            }

            // Otherwise just return the input back out
            return input;
        }

        /// <summary>
        /// Rounds the value to 2 decimal spaces. This is handy if you doing math on Money
        /// and don't want to worry about fractions of a penny.
        /// For example: 123456.55345 returns 123456.55.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static decimal ToMoney( this decimal value )
        {
            return Math.Round( value, 2 );
        }

        /// <summary>
        /// Formats the value to include commas and decimal point. But without
        /// a currency symbol. 
        /// For example: 123456.55345 returns "123,456.55" (depending on browser language settings).
        /// Use <seealso cref="FormatAsCurrency(decimal)"/> to include the currency symbol.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToMoneyString( this decimal value )
        {
            return string.Format( "{0:N}", value );
        }

        #endregion
    }
}

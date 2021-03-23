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
        /// Formats as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this decimal value )
        {
            var currencySymbol = GlobalAttributesCache.Value( "CurrencySymbol" );
            return string.Format( "{0}{1:N}", currencySymbol, value );
        }

        /// <summary>
        /// Formats as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this decimal? value )
        {
            if ( value.HasValue )
            {
                return FormatAsCurrency( value.Value );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this double value )
        {
            return ( (decimal)value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this double? value )
        {
            return ( (decimal?)value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this int value )
        {
            return ( (decimal)value ).FormatAsCurrency();
        }

        /// <summary>
        /// Formats as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( this int? value )
        {
            return ( (decimal?)value ).FormatAsCurrency();
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

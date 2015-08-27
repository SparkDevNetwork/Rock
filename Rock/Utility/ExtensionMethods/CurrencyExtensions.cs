// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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

        #endregion
    }
}

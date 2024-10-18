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

using Rock.Utility;

namespace Rock
{
    /// <summary>
    /// Handy decimal extensions that require Rock references or references to NuGet packages
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Rounds the <c>decimal</c> value to the nearest decimal place for the organization's configured currency.
        /// </summary>
        /// <param name="value">the <c>decimal</c> to round.</param>
        /// <returns>The rounded <c>decimal</c> value.</returns>
        public static decimal AsCurrency( this decimal value )
        {
            return Math.Round( value, RockCurrencyCodeInfo.GetDecimalPlaces() );
        }

        /// <summary>
        /// Applies the <paramref name="discountPercentage"/> as a discount to the <paramref name="value"/>
        /// and rounds the resulting value to the number of decimal places specified by the organization's configured currency.
        /// </summary>
        /// <remarks>
        /// Zero is the lowest number that will be returned.
        /// </remarks>
        /// <param name="value">The value to apply the discount to.</param>
        /// <param name="discountPercentage">The percentage to discount (0 - 1).</param>
        /// <returns>The new currency amount after applying the discount percentage.</returns>
        public static decimal AsDiscountedPercentage( this decimal value, decimal discountPercentage )
        {
            var asCurrency = value.AsCurrency();
            var discountedValue = ( asCurrency - ( asCurrency * discountPercentage ) ).AsCurrency();
            return discountedValue > 0 ? discountedValue : 0;
        }
    }
}

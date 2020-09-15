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

namespace Rock
{
    /// <summary>
    ///
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the number of digits following the decimal place. 5.68 would return 2. 17.9998 would return 4.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int GetDecimalPrecision( this decimal value )
        {
            return BitConverter.GetBytes( decimal.GetBits( value )[3] )[2];
        }

        /// <summary>
        /// Returns the floor (round down) value with the given decimal precision
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        public static decimal? Floor( this decimal? value, int precision = 0 )
        {
            if ( !value.HasValue )
            {
                return null;
            }

            var shiftFactor = Convert.ToDecimal( Math.Pow( 10, precision ) );
            var shiftedValue = value.Value * shiftFactor;
            var shiftedFloor = Math.Floor( shiftedValue );
            var unshiftedFloor = shiftedFloor / shiftFactor;
            return unshiftedFloor;
        }

        /// <summary>
        /// Converts a decimal to a percentage and returns a string with the percent sign
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatAsPercent( this decimal value )
        {
            return $"{( ( int ) Math.Round( value *= 100 ) ).ToString()}%";
        }
    }
}
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
using System.Globalization;

using Humanizer;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Converts the input to a Boolean(true/false) value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="resultIfNullOrEmpty"></param>
        /// <returns></returns>
        public static bool AsBoolean( object input, bool resultIfNullOrEmpty = false )
        {
            return input.ToStringSafe().AsBoolean( resultIfNullOrEmpty );
        }

        /// <summary>
        /// Converts the input to a decimal value, or 0 if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static decimal AsDecimal( object input )
        {
            return input.ToStringSafe().AsDecimal();
        }

        /// <summary>
        /// Converts the input to a double-precision value, or 0 if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double AsDouble( object input )
        {
            return input.ToStringSafe().AsDouble();
        }

        /// <summary>
        /// Converts the input to an integer value, or 0 if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int AsInteger( object input )
        {
            return input.ToStringSafe().AsInteger();
        }

        /// <summary>
        /// Formats the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string Format( object input, string format )
        {
            var inputString = input.ToString();
            
            if ( string.IsNullOrWhiteSpace( format ) )
            {
                return inputString;
            }

            var decimalValue = inputString.AsDecimalOrNull();

            if ( decimalValue == null )
            {
                return string.Format( "{0:" + format + "}", inputString );
            }
            else
            {
                return string.Format( "{0:" + format + "}", decimalValue );
            }
        }

        /// <summary>
        /// Formats the specified input as currency using the specified CurrencySymbol.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="currencySymbol">The currency symbol.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( object input, string currencySymbol = null )
        {
            if ( input == null )
            {
                return null;
            }

            currencySymbol = currencySymbol ?? CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

            // if the input an integer, decimal, double or anything else that can be parsed as a decimal, format that
            decimal? inputAsDecimal = input.ToString().AsDecimalOrNull();

            if ( inputAsDecimal.HasValue )
            {
                return string.Format( "{0}{1:N}", currencySymbol, inputAsDecimal );
            }
            else
            {
                if ( input is string )
                {
                    // if the input is a string, just append the currency symbol to the front, even if it can't be converted to a number
                    return string.Format( "{0}{1}", currencySymbol, input );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// takes 1, 2 and returns 1st, 2nd
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinal( object input )
        {
            var inputString = input.ToString();

            if ( string.IsNullOrWhiteSpace( inputString ) )
            {
                return inputString;
            }

            int number;

            if ( int.TryParse( inputString, out number ) )
            {
                return number.Ordinalize();
            }
            else
            {
                return inputString;
            }
        }

        /// <summary>
        /// takes 1,2 and returns one, two
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToWords( object input )
        {
            var inputString = input.ToString();

            if ( string.IsNullOrWhiteSpace( inputString ) )
            {
                return inputString;
            }

            int number;

            if ( int.TryParse( inputString, out number ) )
            {
                return number.ToWords();
            }
            else
            {
                return inputString;
            }
        }

        /// <summary>
        /// takes 1,2 and returns first, second
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinalWords( object input )
        {
            var stringInput = input.ToStringSafe();

            if ( string.IsNullOrWhiteSpace( stringInput ) )
            {
                return null;
            }

            int number;

            if ( int.TryParse( stringInput, out number ) )
            {
                return number.ToOrdinalWords();
            }
            else
            {
                return stringInput;
            }
        }

        /// <summary>
        /// takes 1,2 and returns I, II, IV
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToRomanNumerals( object input )
        {
            var stringInput = input.ToStringSafe();

            if ( string.IsNullOrWhiteSpace( stringInput ) )
            {
                return null;
            }

            int number;

            if ( int.TryParse( stringInput, out number ) )
            {
                return number.ToRoman();
            }
            else
            {
                return stringInput;
            }
        }

        /// <summary>
        /// formats string to be appropriate for a quantity
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public static string ToQuantity( object input, object quantity )
        {
            var inputString = input.ToString();

            if ( string.IsNullOrWhiteSpace( inputString ) )
            {
                return inputString;
            }

            int numericQuantity;
            if ( quantity is string )
            {
                numericQuantity = ( int ) ( ( quantity as string ).AsDecimal() );
            }
            else
            {
                numericQuantity = Convert.ToInt32( quantity );
            }

            return inputString.ToQuantity( numericQuantity );
        }

        /// <summary>
        /// Generates a random number greater than or equal to 0 and less than
        /// the input as a number.
        /// </summary>
        /// <param name="input">The input number to provide the upper range.</param>
        /// <returns>A random number.</returns>
        /// <exception cref="Exception">Must provide an integer value as input.</exception>
        /// <remarks>If you pass in a value of 100 as input, you will get a random number of 0-99.</remarks>
        public static int RandomNumber( object input )
        {
            var number = input.ToStringSafe().AsIntegerOrNull();

            if ( !number.HasValue )
            {
                throw new Exception( "Must provide an integer value as input." );
            }

            return _randomNumberGenerator.Next( number.Value );
        }
    }
}

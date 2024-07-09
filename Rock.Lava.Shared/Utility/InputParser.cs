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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

/*
 * [2021-10-17] DJL
 * This utility class is flagged for internal use as it will be subject to revision in the future.
 */
[assembly: InternalsVisibleTo( "Rock.Lava" )]
[assembly: InternalsVisibleTo( "Rock" )]
[assembly: InternalsVisibleTo( "Rock.Tests.Integration" )]

namespace Rock.Lava
{
    /// <summary>
    /// Parse end-user input in a variety of forms for specific argument types.
    /// Useful for validating external input so that it can be safely used for internal processing.
    /// </summary>
    /// <remarks>
    /// Functions in this utility class should take one of these forms:
    /// 1. TryConvertTo{Type}... - attempts to convert an input object to the named type.
    ///    The return value is a flag indicating if the conversion was successful, and the converted value is returned as an output parameter.
    /// 2. ConvertTo{Type}OrDefault - attempts to convert an input object to the named type, or returns a default value if unsuccessful.
    /// 3. ConvertTo{Type}OrThrow - attempts to convert an input object to the named type, or throws an Exception if unsuccessful.
    /// </remarks>
    internal static class InputParser
    {
        /// <summary>
        /// Valid input values for boolean conversion.
        /// </summary>
        private static string[] _trueStrings = new string[] { "true", "yes", "t", "y", "1" };
        private static string[] _falseStrings = new string[] { "false", "no", "f", "n", "0" };

        /// <summary>
        /// Try to convert an input object to a boolean value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input">an object</param>
        /// <param name="valueIfEmpty">the value to return if the input is null or whitespace</param>
        /// <param name="valueIfInvalid">the value to return if the input is not a recognized value</param>
        /// <returns></returns>
        /// <remarks>
        /// Returns True for 'True', 'Yes', 'T', 'Y', '1' (case-insensitive).
        /// Returns False for 'False', 'No', 'F', 'N', '0' (case-insensitive).
        /// </remarks>
        public static bool? ConvertToBooleanOrDefault( this object input, bool? valueIfEmpty = null, bool? valueIfInvalid = null )
        {
            // If the input is null or whitespace, return the empty value.
            if ( input == null )
            {
                return valueIfEmpty;
            }

            var testValue = input.ToString().Trim().ToLower();
            if ( string.IsNullOrEmpty( testValue ) )
            {
                return valueIfEmpty;
            }

            // Return True or False if the input matches a recognized value.
            if ( _trueStrings.Contains( testValue ) )
            {
                return true;
            }
            if ( _falseStrings.Contains( testValue ) )
            {
                return false;
            }

            // Parsing failed, so return the invalid value.
            return valueIfInvalid;
        }

        /// <summary>
        /// Try to convert an input object to a boolean value, or throw an exception if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="returnDefaultIfNullOrEmpty"></param>
        /// <returns></returns>
        public static bool ConvertToBooleanOrThrow( this object input, bool returnDefaultIfNullOrEmpty = true )
        {
            var value = ConvertToBooleanOrDefault( input, returnDefaultIfNullOrEmpty ? false : ( bool? ) null, null );
            if ( value == null )
            {
                throw new Exception( $"Invalid boolean value. [Value={input}]" );
            }

            return value.Value;
        }

        #region Integers

        /// <summary>
        /// Try to convert an input object to an integer value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="valueIfEmpty"></param>
        /// <param name="valueIfInvalid"></param>
        /// <returns></returns>
        public static int? ConvertToIntegerOrDefault( this object input, int? valueIfEmpty = null, int? valueIfInvalid = null )
        {
            // If the input is null or whitespace, return the empty value.
            if ( input is string inputString )
            {
                if ( string.IsNullOrWhiteSpace( inputString ) )
                {
                    return valueIfEmpty;
                }
            }
            else if ( input == null )
            {
                return valueIfEmpty;
            }

            int value;
            if ( int.TryParse( input.ToString(), out value ) )
            {
                // Return the parsed integer value.
                return value;
            }

            // Parsing failed, so return the invalid value.
            return valueIfInvalid;
        }

        /// <summary>
        /// Try to convert an input object to an integer value, or throw an exception if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="valueIfEmpty"></param>
        /// <returns></returns>
        public static int ConvertToIntegerOrThrow( this object input, bool returnDefaultIfNullOrEmpty = true )
        {
            var value = ConvertToIntegerOrDefault( input, returnDefaultIfNullOrEmpty ? 0 : ( int? ) null, null );
            if ( value == null )
            {
                throw new Exception( $"Invalid integer value. [Value={input}]" );
            }

            return value.Value;
        }

        /// <summary>
        /// Try to convert an input object to a list of integers, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static bool TryConvertToIntegerList( string input, out List<int> output, string delimiter )
        {
            // If the input is null or whitespace, return the empty value.
            if ( input == null )
            {
                output = new List<int>();
                return true;
            }

            var inputList = input.Split( new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            var isValid = TryConvertToIntegerList( inputList, out output );
            return isValid;
        }

        /// <summary>
        /// Try to convert an input object to a list of integers, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static bool TryConvertToIntegerList( IEnumerable<object> input, out List<int> output )
        {
            output = new List<int>();

            // If the input is null or whitespace, return the empty value.
            if ( input == null )
            {
                return true;
            }

            foreach ( var value in input )
            {
                var intValue = ConvertToIntegerOrDefault( value, 0, null );
                if ( intValue == null )
                {
                    return false;
                }

                output.Add( intValue.Value );
            }

            return true;
        }

        #endregion

        #region Guid

        /// <summary>
        /// Try to convert an input object to a Guid value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="valueIfEmpty"></param>
        /// <param name="valueIfInvalid"></param>
        /// <returns></returns>
        public static bool TryConvertToNullableGuid( object input, out Guid? value )
        {
            // If the input is null or whitespace, return the empty value.
            if ( input == null )
            {
                value = null;
                return false;
            }

            var inputString = input.ToString();
            if ( string.IsNullOrWhiteSpace( inputString ) )
            {
                value = null;
                return false;
            }

            Guid guidValue;
            if ( Guid.TryParse( inputString, out guidValue ) )
            {
                // Return the parsed value.
                value = guidValue;
                return true;
            }

            // Parsing failed, so return the invalid value.
            value = null;
            return false;
        }

        /// <summary>
        /// Convert an input object to a Guid value, or throw an exception.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputFieldName"></param>
        /// <returns></returns>
        public static Guid ParseToGuidOrThrow( this string inputFieldName, object input, Guid? defaultValueIfEmpty = null )
        {
            var isValid = TryConvertToNullableGuid( input, out Guid? value );
            if ( isValid )
            {
                value = value ?? defaultValueIfEmpty;
                if ( value != null )
                {
                    return value.Value;
                }
            }

            throw new Exception( $"Invalid Value. The input does not represent a valid Guid. [Name={inputFieldName}, Value={input}]" );
        }

        #endregion

        #region DateTime

        /// <summary>
        /// Try to convert an input object to a DateTimeOffset.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool TryConvertToDateTimeOffset( object input, out DateTimeOffset output, string format = null )
        {
            if ( input is DateTimeOffset dto )
            {
                output = dto;
                return true;
            }
            else if ( input is DateTime dt )
            {
                output = new DateTimeOffset( dt );
                return true;
            }
            else if ( input is string inputString )
            {
                if ( format != null )
                {
                    var isValid = DateTimeOffset.TryParseExact( inputString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out output );
                    return isValid;
                }
                else
                {
                    var isValid = DateTimeOffset.TryParse( inputString, out output );
                    return isValid;
                }
            }

            output = DateTimeOffset.MinValue;
            return false;
        }

        #endregion

        #region Obsolete

        /// <summary>
        /// Try to convert an input object to a boolean value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input">an object</param>
        /// <param name="valueIfEmpty">the value to return if the input is null or whitespace</param>
        /// <param name="valueIfInvalid">the value to return if the input is not a recognized value</param>
        /// <returns></returns>
        /// <remarks>
        /// Returns True for 'True', 'Yes', 'T', 'Y', '1' (case-insensitive).
        /// Returns False for 'False', 'No', 'F', 'N', '0' (case-insensitive).
        /// </remarks>
        [Obsolete( "Use ConvertToBooleanOrDefault instead." )]
        public static bool? TryConvertBoolean( this object input, bool? valueIfEmpty = null, bool? valueIfInvalid = null )
        {
            // If the input is null or whitespace, return the empty value.
            return ConvertToBooleanOrDefault( input, valueIfEmpty, valueIfInvalid );
        }

        /// <summary>
        /// Try to convert an input object to an integer value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="valueIfEmpty"></param>
        /// <param name="valueIfInvalid"></param>
        /// <returns></returns>
        [Obsolete( "Use ConvertToIntegerOrDefault instead." )]
        public static int? TryConvertInteger( this object input, int? valueIfEmpty = null, int? valueIfInvalid = null )
        {
            return ConvertToIntegerOrDefault( input, valueIfEmpty, valueIfInvalid );
        }

        #endregion
    }
}

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
        public static bool? TryConvertBoolean( this object input, bool? valueIfEmpty = null, bool? valueIfInvalid = null )
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
        /// Try to convert an input object to an integer value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="valueIfEmpty"></param>
        /// <param name="valueIfInvalid"></param>
        /// <returns></returns>
        public static int? TryConvertInteger( this object input, int? valueIfEmpty = null, int? valueIfInvalid = null )
        {
            // If the input is null or whitespace, return the empty value.
            if ( input == null )
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
            if ( string.IsNullOrWhiteSpace(inputString) )
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
        public static Guid ParseToGuidOrThrow( string inputFieldName, object input, Guid? defaultValueIfEmpty = null )
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
    }
}

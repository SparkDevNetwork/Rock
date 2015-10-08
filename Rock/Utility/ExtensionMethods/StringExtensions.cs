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
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rock
{
    /// <summary>
    /// Handy string extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region String Extensions

        /// <summary>
        /// Removes special characters from the string so that only Alpha, Numeric, '.' and '_' remain;
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters( this string str )
        {
            StringBuilder sb = new StringBuilder();
            foreach ( char c in str )
            {
                if ( ( c >= '0' && c <= '9' ) || ( c >= 'A' && c <= 'Z' ) || ( c >= 'a' && c <= 'z' ) || c == '.' || c == '_' )
                {
                    sb.Append( c );
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Splits a Camel or Pascal cased identifier into seperate words.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string SplitCase( this string str )
        {
            if ( str == null )
                return null;

            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited by any combination of whitespace, comma, semi-colon, or pipe characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="whitespace">if set to <c>true</c> whitespace will be treated as a delimiter</param>
        /// <returns></returns>
        public static string[] SplitDelimitedValues( this string str, bool whitespace = true )
        {
            if ( str == null )
                return new string[0];

            string regex = whitespace ? @"[\s\|,;]+" : @"[\|,;]+";

            char[] delimiter = new char[] { ',' };
            return Regex.Replace( str, regex, "," ).Split( delimiter, StringSplitOptions.RemoveEmptyEntries );
        }



        /// <summary>
        /// Replaces every instance of oldValue (regardless of case) with the newValue.
        /// (from http://www.codeproject.com/Articles/10890/Fastest-C-Case-Insenstive-String-Replace)
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The value to insert.</param>
        /// <returns></returns>
        public static string ReplaceCaseInsensitive( this string str, string oldValue, string newValue )
        {
            if ( str == null )
                return null;

            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = str.ToUpper();
            string upperPattern = oldValue.ToUpper();
            int inc = ( str.Length / oldValue.Length ) *
                      ( newValue.Length - oldValue.Length );
            char[] chars = new char[str.Length + Math.Max( 0, inc )];
            while ( ( position1 = upperString.IndexOf( upperPattern,
                                              position0 ) ) != -1 )
            {
                for ( int i = position0; i < position1; ++i )
                    chars[count++] = str[i];
                for ( int i = 0; i < newValue.Length; ++i )
                    chars[count++] = newValue[i];
                position0 = position1 + oldValue.Length;
            }
            if ( position0 == 0 ) return str;
            for ( int i = position0; i < str.Length; ++i )
                chars[count++] = str[i];
            return new string( chars, 0, count );
        }

        /// <summary>
        /// Replaces every instance of oldValue with newValue.  Will continue to replace
        /// values after each replace until the oldValue does not exist.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The value to insert.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceWhileExists( this string str, string oldValue, string newValue )
        {
            string newstr = str;

            if ( oldValue != newValue )
            {
                while ( newstr.Contains( oldValue ) )
                {
                    newstr = newstr.Replace( oldValue, newValue );
                }
            }

            return newstr;
        }

        /// <summary>
        /// Adds escape character for quotes in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeQuotes( this string str )
        {
            if ( str == null )
                return null;

            return str.Replace( "'", "\\'" ).Replace( "\"", "\\\"" );
        }

        /// <summary>
        /// Adds Quotes around the specified string and escapes any quotes that are already in the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="QuoteChar">The quote character.</param>
        /// <returns></returns>
        public static string Quoted( this string str, string QuoteChar = "'" )
        {
            var result = QuoteChar + str.EscapeQuotes() + QuoteChar;
            return result;
        }

        /// <summary>
        /// Returns the specified number of characters, starting at the left side of the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="length">The desired length.</param>
        /// <returns></returns>
        public static string Left( this string str, int length )
        {
            if ( str.Length <= length )
            {
                return str;
            }
            else
            {
                return str.Substring( 0, length );
            }
        }

        /// <summary>
        /// Truncates a string after a max length and adds ellipsis.  Truncation will occur at first space prior to maxLength.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate( this string str, int maxLength )
        {
            if ( str == null )
                return null;

            if ( str.Length <= maxLength )
                return str;

            maxLength -= 3;
            var truncatedString = str.Substring( 0, maxLength );
            var lastSpace = truncatedString.LastIndexOf( ' ' );
            if ( lastSpace > 0 )
                truncatedString = truncatedString.Substring( 0, lastSpace );

            return truncatedString + "...";
        }

        

        /// <summary>
        /// Removes any non-numeric characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AsNumeric( this string str )
        {
            return Regex.Replace( str, @"[^0-9]", "" );
        }

        /// <summary>
        /// Replaces the last occurrence of a given string with a new value
        /// </summary>
        /// <param name="Source">The string.</param>
        /// <param name="Find">The search parameter.</param>
        /// <param name="Replace">The replacement parameter.</param>
        /// <returns></returns>
        public static string ReplaceLastOccurrence( this string Source, string Find, string Replace )
        {
            int Place = Source.LastIndexOf( Find );
            string result = Source.Remove( Place, Find.Length ).Insert( Place, Replace );
            return result;
        }

        /// <summary>
        /// The true strings for AsBoolean and AsBooleanOrNull.
        /// </summary>
        private static string[] trueStrings = new string[] { "true", "yes", "t", "y", "1" };

        /// <summary>
        /// Returns True for 'True', 'Yes', 'T', 'Y', '1' (case-insensitive).
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="resultIfNullOrEmpty">if set to <c>true</c> [result if null or empty].</param>
        /// <returns></returns>
        public static bool AsBoolean( this string str, bool resultIfNullOrEmpty = false )
        {
            if ( string.IsNullOrWhiteSpace( str ) )
            {
                return resultIfNullOrEmpty;
            }

            return trueStrings.Contains( str.ToLower() );
        }

        /// <summary>
        /// Returns True for 'True', 'Yes', 'T', 'Y', '1' (case-insensitive), null for emptystring/null.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool? AsBooleanOrNull( this string str )
        {
            string[] trueStrings = new string[] { "true", "yes", "t", "y", "1" };

            if ( string.IsNullOrWhiteSpace( str ) )
            {
                return null;
            }

            return trueStrings.Contains( str.ToLower() );
        }

        /// <summary>
        /// Attempts to convert string to integer.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static int AsInteger( this string str )
        {
            return str.AsIntegerOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to an integer.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static int? AsIntegerOrNull( this string str )
        {
            int value;
            if ( int.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to Guid.  Returns Guid.Empty if unsuccessful.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static Guid AsGuid( this string str )
        {
            return str.AsGuidOrNull() ?? Guid.Empty;
        }

        /// <summary>
        /// Attempts to convert string to Guid.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static Guid? AsGuidOrNull( this string str )
        {
            Guid value;
            if ( Guid.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified unique identifier is Guid.Empty.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static bool IsEmpty( this Guid guid )
        {
            return guid.Equals( Guid.Empty );
        }

        /// <summary>
        /// Attempts to convert string to decimal.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static decimal AsDecimal( this string str )
        {
            return str.AsDecimalOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to decimal.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static decimal? AsDecimalOrNull( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // strip off non numeric and characters (for example, currency symbols)
                str = Regex.Replace( str, @"[^0-9\.-]", "" );
            }

            decimal value;
            if ( decimal.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to double.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static double AsDouble( this string str )
        {
            return str.AsDoubleOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to double.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static double? AsDoubleOrNull( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // strip off non numeric and characters (for example, currency symbols)
                str = Regex.Replace( str, @"[^0-9\.-]", "" );
            }

            double value;
            if ( double.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to DateTime.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static DateTime? AsDateTime( this string str )
        {
            DateTime value;
            if ( DateTime.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to TimeSpan.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static TimeSpan? AsTimeSpan( this string str )
        {
            TimeSpan value;
            if ( TimeSpan.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts the value to Type, or if unsuccessful, returns the default value of Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T AsType<T>( this string value )
        {
            var converter = TypeDescriptor.GetConverter( typeof( T ) );
            return converter.IsValid( value )
                ? (T)converter.ConvertFrom( value )
                : default( T );
        }

        /// <summary>
        /// Masks the specified value if greater than 4 characters (such as a credit card number).
        /// For example, the return string becomes "************6789".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Masked( this string value )
        {
            if ( value.Length > 4 )
            {
                return string.Concat( new string( '*', 12 ), value.Substring( value.Length - 4 ) );
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Ensures the trailing backslash. Handy when combining folder paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureTrailingBackslash( this string value )
        {
            return value.TrimEnd( new char[] { '\\', '/' } ) + "\\";
        }

        /// <summary>
        /// Ensures the trailing forward slash. Handy when combining url paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureTrailingForwardslash( this string value )
        {
            return value.TrimEnd( new char[] { '\\', '/' } ) + "/";
        }

        /// <summary>
        /// Ensures the leading forward slash. Handy when combining url paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string RemoveLeadingForwardslash( this string value )
        {
            return value.TrimStart( new char[] { '/' } );
        }

        /// <summary>
        /// Evaluates string, and if null or empty, returns nullValue instead.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="nullValue">The null value.</param>
        /// <returns></returns>
        public static string IfEmpty( this string value, string nullValue )
        {
            return !string.IsNullOrWhiteSpace( value ) ? value : nullValue;
        }

        /// <summary>
        /// Replaces special Microsoft Word chars with standard chars
        /// For example, smart quotes will be replaced with apostrophes
        /// from http://www.andornot.com/blog/post/Replace-MS-Word-special-characters-in-javascript-and-C.aspx
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string ReplaceWordChars( this string text )
        {
            var s = text;
            // smart single quotes and apostrophe
            s = Regex.Replace( s, "[\u2018\u2019\u201A]", "'" );
            // smart double quotes
            s = Regex.Replace( s, "[\u201C\u201D\u201E]", "\"" );
            // ellipsis
            s = Regex.Replace( s, "\u2026", "..." );
            // dashes
            s = Regex.Replace( s, "[\u2013\u2014]", "-" );
            // circumflex
            s = Regex.Replace( s, "\u02C6", "^" );
            // open angle bracket
            s = Regex.Replace( s, "\u2039", "<" );
            // close angle bracket
            s = Regex.Replace( s, "\u203A", ">" );
            // spaces
            s = Regex.Replace( s, "[\u02DC\u00A0]", " " );

            return s;
        }

        #endregion String Extensions
    }
}

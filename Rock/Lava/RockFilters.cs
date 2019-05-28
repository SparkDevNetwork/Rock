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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;

using DDay.iCal;

using DotLiquid;
using DotLiquid.Util;

using Humanizer;
using Humanizer.Localisation;

using ImageResizer;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

using UAParser;

namespace Rock.Lava
{
    /// <summary>
    ///
    /// </summary>
    public static class RockFilters
    {
        static Random _randomNumberGenerator = new Random();

        #region String Filters


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public static string ToString( object input )
        {
            return input?.ToString();
        }

        /// <summary>
        /// Uniques the identifier.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string UniqueIdentifier( object input )
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Withes the fallback.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="successText">The success text.</param>
        /// <param name="fallbackText">The fallback text.</param>
        /// <returns></returns>
        public static string WithFallback( object input, string successText, string fallbackText )
        {
            return WithFallback( input, successText, fallbackText, "prepend" );
        }

        /// <summary>
        /// Withes the fallback.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="successText">The success text.</param>
        /// <param name="fallbackText">The fallback text.</param>
        /// <param name="appendOrder">The append order.</param>
        /// <returns></returns>
        public static string WithFallback( object input, string successText, string fallbackText, string appendOrder )
        {
            if ( input == null )
            {
                return fallbackText;
            }
            else
            {
                var inputString = input.ToString();

                if ( string.IsNullOrWhiteSpace( inputString ) )
                {
                    return fallbackText;
                }
                else
                {
                    if ( appendOrder == "prepend" )
                    {
                        return successText + inputString;
                    }
                    else
                    {
                        return inputString + successText;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the right most part of a string of the given length.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string Right( object input, int length )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            var inputString = input.ToString();

            if (inputString.Length <= length )
            {
                return inputString;
            }

            return inputString.Right( length );
        }

        /// <summary>
        /// obfuscate a given email
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ObfuscateEmail( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                string[] emailParts = input.Split( '@' );

                if ( emailParts.Length != 2 )
                {
                    return input;
                }
                else
                {
                    return string.Format( "{0}xxxxx@{1}", emailParts[0].Substring( 0, 1 ), emailParts[1] );
                }
            }
        }

        /// <summary>
        /// pluralizes string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Pluralize( string input )
        {
            return input == null
                ? input
                : input.Pluralize();
        }

        /// <summary>
        /// convert string to possessive ('s)
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Possessive( string input )
        {
            if ( input == null )
            {
                return input;
            }

            return input.ToPossessive();
        }

        /// <summary>
        /// pluralizes string based on the value for quantity
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public static string PluralizeForQuantity( string input, object quantity )
        {
            if ( input == null )
            {
                return input;
            }

            decimal numericQuantity = 0.0M;
            if ( quantity is string )
            {
                numericQuantity = ( quantity as string ).AsDecimal();
            }
            else
            {
                try
                {
                    numericQuantity = Convert.ToDecimal( quantity );
                }
                catch { }
            }

            if ( numericQuantity > 1 )
            {
                return input.Pluralize();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// convert a integer to a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToString( int input )
        {
            return input.ToString();
        }

        /// <summary>
        /// singularize string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Singularize( string input )
        {
            return input == null
                ? input
                : input.Singularize();
        }

        /// <summary>
        /// takes computer-readible-formats and makes them human readable
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Humanize( string input )
        {
            return input == null
                ? input
                : input.Humanize();
        }

        /// <summary>
        /// returns sentence in 'Title Case'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TitleCase( string input )
        {
            return input == null
                ? input
                : input.Titleize();
        }

        /// <summary>
        /// returns sentence in 'PascalCase'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPascal( string input )
        {
            return input == null
                ? input
                : input.Dehumanize();
        }

        /// <summary>
        /// returns sentence in 'PascalCase'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCssClass( string input )
        {
            // list from: https://mathiasbynens.be/notes/css-escapes
            Regex ex = new Regex( @"[&*!""#$%'()+,.\/:;<=>?@\[\]\^`{\|}~\s]");

            if (input == null )
            {
                return input;
            }

            // replace unsupported characters
            input = ex.Replace( input, "-" ).ToLower();

            // remove duplicate instances of dashes (cleanliness is next to... well... it's good)
            input = Regex.Replace( input, "-+", "-" );

            // ensure the class name is valid (starts with a letter or - or _ and is at least 2 characters
            // if not add a x- to correct it and note that it is non-standard

            ex = new Regex( "-?[_a-zA-Z]+[_a-zA-Z0-9-]*");
            if ( !ex.IsMatch( input ) )
            {
                input = "-x-" + input;
            }

            return input;
        }

        /// <summary>
        /// returns sentence in 'Sentence case'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SentenceCase( string input )
        {
            return input == null
                ? input
                : input.Transform( To.SentenceCase );
        }

        /// <summary>
        /// takes 1, 2 and returns 1st, 2nd
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinal( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.Ordinalize();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes 1,2 and returns one, two
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToWords( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.ToWords();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes 1,2 and returns first, second
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinalWords( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.ToOrdinalWords();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes 1,2 and returns I, II, IV
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToRomanNumerals( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.ToRoman();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// formats string to be appropriate for a quantity
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public static string ToQuantity( string input, object quantity )
        {
            int numericQuantity;
            if ( quantity is string )
            {
                numericQuantity = (int)( ( quantity as string ).AsDecimal() );
            }
            else
            {
                numericQuantity = Convert.ToInt32( quantity );
            }

            return input == null
                ? input
                : input.ToQuantity( numericQuantity );
        }

        /// <summary>
        /// Replace occurrences of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace( object input, object @string, object replacement = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            string replacementString = ( replacement ?? string.Empty ).ToString();
            string pattern = Regex.Escape( @string.ToString() );

            /*// escape common regex meta characters
            var listOfRegExChars = new List<string> { ".", "$", "{", "}", "^", "[", "]", "*", @"\", "+", "|", "?", "<", ">" };
            if ( listOfRegExChars.Contains( @string ) )
            {
                @string = @"\" + @string;
            }*/

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( pattern ) )
            {
                return inputAsString;
            }

            return string.IsNullOrEmpty( inputAsString )
                ? inputAsString
                : Regex.Replace( inputAsString, pattern, replacementString );
        }

        /// <summary>
        /// Replace the first occurrence of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceFirst( object input, string @string, string replacement = "" )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( @string ) )
            {
                return inputAsString;
            }

            // escape common regex meta characters
            var listOfRegExChars = new List<string> { ".", "$", "{", "}", "^", "[", "]", "*", @"\", "+", "|", "?", "<", ">" };
            if ( listOfRegExChars.Contains( @string ) )
            {
                @string = @"\" + @string;
            }

            bool doneReplacement = false;
            return Regex.Replace( inputAsString, @string, m =>
            {
                if ( doneReplacement )
                {
                    return m.Value;
                }

                doneReplacement = true;
                return replacement;
            } );
        }

        /// <summary>
        /// Replace the last occurrence of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="search">The search.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public static string ReplaceLast( object input, string search, string replacement = "" )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( search ) )
            {
                return inputAsString;
            }

            int place = inputAsString.LastIndexOf( search );
            if ( place > 0 )
            {
                return inputAsString.Remove( place, search.Length ).Insert( place, replacement );
            }
            else
            {
                return input.ToString();
            }
        }

        /// <summary>
        /// Remove a substring - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Remove( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? inputAsString
                : inputAsString.Replace( @string, string.Empty );
        }

        /// <summary>
        /// Trims the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Trim( object input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            return input.ToString().Trim();
        }

        /// <summary>
        /// Remove the first occurrence of a substring - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string RemoveFirst( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? inputAsString
                : ReplaceFirst( inputAsString, @string, string.Empty );
        }

        /// <summary>
        /// Appends the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="string">The string.</param>
        /// <returns></returns>
        public static string Append( object input, object @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            if (@string == null )
            {
                return input.ToString();
            }

            string inputAsString = input.ToString();

            return inputAsString == null
                ? inputAsString
                : inputAsString + @string.ToString();
        }

        /// <summary>
        /// Prepend a string to another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Prepend( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return inputAsString == null
                ? inputAsString
                : @string + inputAsString;
        }

        /// <summary>
        /// Returns the passed default value if the value is undefined or empty, otherwise the value of the variable
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="defaultString">The default string.</param>
        /// <returns></returns>
        public static string Default( object input, string defaultString )
        {
            if ( input == null )
            {
                return defaultString;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? defaultString
                : inputAsString;
        }

        /// <summary>
        /// Decodes an HTML string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string HtmlDecode( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                return HttpUtility.HtmlDecode( input );
            }
        }

        /// <summary>
        /// Converts a string to its escaped representation using Uri.EscapeDataString
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string EscapeDataString( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                return Uri.EscapeDataString( input );
            }
        }

        /// <summary>
        /// URLs the encode.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string UrlEncode( string input )
        {
            return EscapeDataString( input );
        }

        /// <summary>
        /// Uns the escape data string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string UnescapeDataString( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                return Uri.UnescapeDataString( input );
            }
        }

        /// <summary>
        /// URLs the decode.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string UrlDecode( string input )
        {
            return UnescapeDataString( input );
        }

        /// <summary>
        /// Tests if the inputted string matches the regex
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expression">The regex expression.</param>
        /// <returns></returns>
        public static bool RegExMatch( string input, string expression )
        {
            if ( input == null )
            {
                return false;
            }

            Regex regex = new Regex( expression );
            Match match = regex.Match( input );

            return match.Success;
        }

        /// <summary>
        /// Returns matched RegEx string from inputted string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expression">The regex expression.</param>
        /// <returns></returns>
        public static string RegExMatchValue( string input, string expression )
        {
            if ( input == null )
            {
                return null;
            }

            Regex regex = new Regex( expression );
            Match match = regex.Match( input );

            return match.Success ? match.Value : null;
        }

        /// <summary>
        /// The slice filter returns a substring, starting at the specified index.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="start">If the passed index is negative, it is counted from the end of the string.</param>
        /// <param name="length">An optional second parameter can be passed to specify the length of the substring.  If no second parameter is given, a substring of one character will be returned.</param>
        /// <returns></returns>
        public static String Slice( string input, int start, int length = 1 )
        {
            // If a negative start, subtract if from the length
            if ( start < 0 )
            {
                start = input.Length + start;
            }
            // Make sure start is never < 0
            start = start >= 0 ? start : 0;

            // If length takes us off the end, fix it
            length = length > ( input.Length - start ) ? ( input.Length - start ) : length;

            return input.Substring(start, length);
        }

        /// <summary>
        /// Decrypts an encrypted string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Decrypt( string input )
        {
            if ( input == null )
            {
                return input;
            }

            return Rock.Security.Encryption.DecryptString( input );
        }

        /// <summary>
        /// Parse the input string as a URL and then return a specific part of the URL.
        /// </summary>
        /// <param name="input">The string to be parsed as a URL.</param>
        /// <param name="part">The part of the Uri object to retrieve.</param>
        /// <param name="key">Extra parameter used by the QueryParameter key for which query parameter to retrieve.</param>
        /// <returns>A string that identifies the part of the URL that was requested.</returns>
        public static object Url( string input, string part, string key = null )
        {
            if ( string.IsNullOrEmpty( input ) || string.IsNullOrEmpty( part ) )
            {
                return input;
            }

            Uri uri;
            if ( !Uri.TryCreate( input, UriKind.Absolute, out uri ) )
            {
                return string.Empty;
            }

            switch ( part.ToUpper() )
            {
                case "HOST":
                    return uri.Host;

                case "PORT":
                    return uri.Port;

                case "SEGMENTS":
                    return uri.Segments;

                case "SCHEME":
                case "PROTOCOL":
                    return uri.Scheme;

                case "LOCALPATH":
                    return uri.LocalPath;

                case "PATHANDQUERY":
                    return uri.PathAndQuery;

                case "QUERYPARAMETER":
                    if ( key != null )
                    {
                        var parameters = HttpUtility.ParseQueryString( uri.Query );

                        if ( parameters.AllKeys.Contains( key ) )
                        {
                            return parameters[key];
                        }
                    }

                    return string.Empty;

                case "URL":
                    return uri.ToString();

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Sanitizes a SQL string by replacing any occurrences of "'" with "''".
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Sanitized string that can be used in a SQL statement.</returns>
        /// <example>{% sql %}SELECT * FROM [Person] WHERE [LastName] = '{{ Name | SanitizeSql }}'{% endsql %}</example>
        public static string SanitizeSql( string input )
        {
            if ( input == null )
            {
                return input;
            }

            return input.Replace( "'", "''" );
        }

        #endregion

        #region DateTime Filters

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date( object input, string format )
        {
            if ( input == null )
            {
                return null;
            }

            if ( input.ToString() == "Now" )
            {
                input = RockDateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture );
            }

            if ( string.IsNullOrWhiteSpace( format ) )
            {
                return input.ToString();
            }

            // if format string is one character add a space since a format string can't be a single character http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx#UsingSingleSpecifiers
            if ( format.Length == 1 )
            {
                format = " " + format;
            }

            var inputDateTime = input.ToString().AsDateTime();

            // Check for invalid date
            if (! inputDateTime.HasValue )
            {
                return input.ToString().Trim();
            }

            // Consider special 'Standard Date' format
            if ( format == "sd" )
            {
                return inputDateTime.Value.ToShortDateString();
            }

            // Consider special 'Standard Time' format
            if ( format == "st" )
            {
                return inputDateTime.Value.ToShortTimeString();
            }

            return Liquid.UseRubyDateFormat ? inputDateTime.Value.ToStrFTime( format ).Trim() : inputDateTime.Value.ToString( format ).Trim();
        }

        /// <summary>
        /// Sundays the date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string SundayDate( object input )
        {
            if ( input == null )
            {
                return null;
            }

            DateTime date = DateTime.MinValue;

            if ( input.ToString() == "Now" )
            {
                date = RockDateTime.Now;
            }
            else
            {
                if ( !DateTime.TryParse( input.ToString(), out date ) )
                {
                    return null;
                }
            }

            if ( date != DateTime.MinValue )
            {
                return date.SundayDate().ToShortDateString();
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the occurrence Dates from an iCal string or list.
        /// </summary>
        /// <param name="input">The input is either an iCal string or a list of iCal strings.</param>
        /// <param name="option">The quantity option (either an integer or "all").</param>
        /// <returns>a list of datetimes</returns>
        public static List<DateTime> DatesFromICal( object input, object option = null )
        {
            return DatesFromICal( input, option, endDateTimeOption: null );
        }

        /// <summary>
        /// Returns the occurrence Dates from an iCal string or list.
        /// </summary>
        /// <param name="input">The input is either an iCal string or a list of iCal strings.</param>
        /// <param name="option">The quantity option (either an integer or "all").</param>
        /// <param name="endDateTimeOption">The 'enddatetime' option if supplied will return the ending datetime of the occurrence; otherwise the start datetime is returned.</param>
        /// <returns>a list of datetimes</returns>
        public static List<DateTime> DatesFromICal( object input, object option = null, object endDateTimeOption = null )
        {
            // if no option was specified, default to returning just 1 (to preserve previous behavior)
            option = option ?? 1;

            int returnCount = 1;
            if ( option.GetType() == typeof( int ) )
            {
                returnCount = ( int )option;
            }
            else if ( option.GetType() == typeof( string ) )
            {
                // if a string of "all" is specified for the option, return all of the dates
                if ( string.Equals( ( string )option, "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    returnCount = int.MaxValue;
                }
            }

            bool useEndDateTime = ( endDateTimeOption is string && ( string ) endDateTimeOption == "enddatetime" );

            List<DateTime> nextOccurrences = new List<DateTime>();

            if ( input is string )
            {
                nextOccurrences = GetOccurrenceDates( ( string )input, returnCount, useEndDateTime );
            }
            else if ( input is IList )
            {
                foreach ( var item in input as IList )
                {
                    if ( item is string )
                    {
                        nextOccurrences.AddRange( GetOccurrenceDates( ( string )item, returnCount, useEndDateTime ) );
                    }
                }
            }

            nextOccurrences.Sort( ( a, b ) => a.CompareTo( b ) );

            return nextOccurrences.Take( returnCount ).ToList();
        }

        /// <summary>
        /// Gets the occurrence dates.
        /// </summary>
        /// <param name="iCalString">The i cal string.</param>
        /// <param name="returnCount">The return count.</param>
        /// <param name="useEndDateTime">if set to <c>true</c> uses the EndTime in the returned dates; otherwise it uses the StartTime.</param>
        /// <returns>a list of datetimes</returns>
        private static List<DateTime> GetOccurrenceDates( string iCalString, int returnCount, bool useEndDateTime = false )
        {
            iCalendar calendar = iCalendar.LoadFromStream( new StringReader( iCalString ) ).First() as iCalendar;
            DDay.iCal.Event calendarEvent = calendar.Events[0] as Event;

            if ( ! useEndDateTime && calendarEvent.DTStart != null )
            {
                List<Occurrence> dates = calendar.GetOccurrences( RockDateTime.Now, RockDateTime.Now.AddYears( 1 ) ).Take( returnCount ).ToList();
                return dates.Select( d => d.Period.StartTime.Value ).ToList();
            }
            else if ( useEndDateTime && calendarEvent.DTEnd != null )
            {
                List<Occurrence> dates = calendar.GetOccurrences( RockDateTime.Now, RockDateTime.Now.AddYears( 1 ) ).Take( returnCount ).ToList();
                return dates.Select( d => d.Period.EndTime.Value ).ToList();
            }
            else
            {
                return new List<DateTime>();
            }
        }

        /// <summary>
        /// Adds a time interval to a date
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static DateTime? DateAdd( object input, int amount, string interval = "d" )
        {
            DateTime? date = null;

            if ( input == null )
            {
                return null;
            }

            if ( input.ToString() == "Now" )
            {
                date = RockDateTime.Now;
            }
            else
            {
                DateTime d;
                bool success = DateTime.TryParse( input.ToString(), out d );
                if ( success )
                {
                    date = d;
                }
            }

            if ( date.HasValue )
            {
                switch ( interval )
                {
                    case "y":
                        date = date.Value.AddYears( amount );
                        break;
                    case "M":
                        date = date.Value.AddMonths( amount );
                        break;
                    case "w":
                        date = date.Value.AddDays( amount * 7 );
                        break;
                    case "d":
                        date = date.Value.AddDays( amount );
                        break;
                    case "h":
                        date = date.Value.AddHours( amount );
                        break;
                    case "m":
                        date = date.Value.AddMinutes( amount );
                        break;
                    case "s":
                        date = date.Value.AddSeconds( amount );
                        break;
                }
            }

            return date;
        }

        /// <summary>
        /// takes a date time and compares it to RockDateTime.Now and returns a human friendly string like 'yesterday' or '2 hours ago'
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="compareDate">The compare date.</param>
        /// <returns></returns>
        public static string HumanizeDateTime( object input, object compareDate = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            DateTime dtInput;
            DateTime dtCompare;

            if ( input != null && input is DateTime )
            {
                dtInput = (DateTime)input;
            }
            else
            {
                if ( input == null || !DateTime.TryParse( input.ToString(), out dtInput ) )
                {
                    return string.Empty;
                }
            }

            if ( compareDate == null || !DateTime.TryParse( compareDate.ToString(), out dtCompare ) )
            {
                dtCompare = RockDateTime.Now;
            }

            return dtInput.Humanize( true, dtCompare );
        }

        /// <summary>
        /// Dayses from now.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string DaysFromNow( object input )
        {
            DateTime dtInputDate = GetDateFromObject( input ).Date;
            DateTime dtCompareDate = RockDateTime.Now.Date;

            int daysDiff = ( dtInputDate - dtCompareDate ).Days;

            string response = string.Empty;

            switch ( daysDiff )
            {
                case -1:
                    {
                        response = "yesterday";
                        break;
                    }

                case 0:
                    {
                        response = "today";
                        break;
                    }

                case 1:
                    {
                        response = "tomorrow";
                        break;
                    }

                default:
                    {
                        if ( daysDiff > 0 )
                        {
                            response = string.Format( "in {0} days", daysDiff );
                        }
                        else
                        {
                            response = string.Format( "{0} days ago", daysDiff * -1 );
                        }

                        break;
                    }
            }

            return response;
        }

        /// <summary>
        /// Days in month
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="oMonth">The o month.</param>
        /// <param name="oYear">The o year.</param>
        /// <returns></returns>
        public static int? DaysInMonth( object input, object oMonth = null, object oYear = null )
        {
            int? month;
            int? year;

            if (input.ToString().IsNotNullOrWhiteSpace() )
            {
                DateTime? date;

                if (input.ToString().ToLower() == "now" )
                {
                    date = RockDateTime.Now;
                }
                else
                {
                    date = input.ToString().AsDateTime();
                }

                if (date.HasValue )
                {
                    month = date.Value.Month;
                    year = date.Value.Year;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if ( oYear == null )
                {
                    year = RockDateTime.Now.Year;
                }
                else
                {
                    year = oYear.ToString().AsIntegerOrNull();
                }

                month = oMonth.ToString().AsIntegerOrNull();
            }

            if ( month.HasValue && year.HasValue )
            {
                return System.DateTime.DaysInMonth( year.Value, month.Value );
            }

            return null;
        }

        /// <summary>
        /// takes two datetimes and humanizes the difference like '1 day'. Supports 'Now' as end date
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, object precision )
        {
            if ( precision is String )
            {
                return HumanizeTimeSpan( sStartDate, sEndDate, precision.ToString(), "min" );
            }

            int precisionUnit = 1;

            if ( precision is int )
            {
                precisionUnit = (int)precision;
            }

            DateTime startDate = GetDateFromObject( sStartDate );
            DateTime endDate = GetDateFromObject( sEndDate );

            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;
                return difference.Humanize( precisionUnit );
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Humanizes the time span.
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="unit">The minimum unit.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, string unit = "Day", string direction = "min" )
        {
            DateTime startDate = GetDateFromObject( sStartDate );
            DateTime endDate = GetDateFromObject( sEndDate );

            TimeUnit unitValue = TimeUnit.Day;

            switch ( unit )
            {
                case "Year":
                    unitValue = TimeUnit.Year;
                    break;
                case "Month":
                    unitValue = TimeUnit.Month;
                    break;
                case "Week":
                    unitValue = TimeUnit.Week;
                    break;
                case "Day":
                    unitValue = TimeUnit.Day;
                    break;
                case "Hour":
                    unitValue = TimeUnit.Hour;
                    break;
                case "Minute":
                    unitValue = TimeUnit.Minute;
                    break;
                case "Second":
                    unitValue = TimeUnit.Second;
                    break;
            }

            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;

                if ( direction.ToLower() == "max" )
                {
                    return difference.Humanize( maxUnit: unitValue );
                }
                else
                {
                    return difference.Humanize( minUnit: unitValue );
                }
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Gets the date from object.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private static DateTime GetDateFromObject( object date )
        {
            DateTime oDateTime = DateTime.MinValue;

            if ( date is String )
            {
                if ( (string)date == "Now" )
                {
                    return RockDateTime.Now;
                }
                else
                {
                    if ( DateTime.TryParse( (string)date, out oDateTime ) )
                    {
                        return oDateTime;
                    }
                    else
                    {
                        return DateTime.MinValue;
                    }
                }
            }
            else if ( date is DateTime )
            {
                return (DateTime)date;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// takes two datetimes and returns the difference in the unit you provide
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        public static Int64? DateDiff( object sStartDate, object sEndDate, string unit )
        {
            DateTime startDate = GetDateFromObject( sStartDate );
            DateTime endDate = GetDateFromObject( sEndDate );

            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;

                switch ( unit )
                {
                    case "d":
                        return (Int64)difference.TotalDays;
                    case "h":
                        return (Int64)difference.TotalHours;
                    case "m":
                        return (Int64)difference.TotalMinutes;
                    case "M":
                        return (Int64)GetMonthsBetween( startDate, endDate );
                    case "Y":
                        return (Int64)( endDate.Year - startDate.Year );
                    case "s":
                        return (Int64)difference.TotalSeconds;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static int GetMonthsBetween( DateTime from, DateTime to )
        {
            if ( from > to )
            {
                return GetMonthsBetween( to, from );
            }

            var monthDiff = Math.Abs( ( to.Year * 12 + ( to.Month - 1 ) ) - ( from.Year * 12 + ( from.Month - 1 ) ) );

            if ( from.AddMonths( monthDiff ) > to || to.Day < from.Day )
            {
                return monthDiff - 1;
            }
            else
            {
                return monthDiff;
            }
        }

        /// <summary>
        /// Sets the time to midnight on the date provided.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static DateTime? ToMidnight( object input )
        {
            if (input == null )
            {
                return null;
            }

            if ( input is DateTime )
            {
                return ( ( DateTime ) input ).Date;
            }

            if (input.ToString() == "Now" )
            {
                return RockDateTime.Now.Date;
            }
            else
            {
                DateTime date;

                if (DateTime.TryParse( input.ToString(), out date ) )
                {
                    return date.Date;
                }
            }

            return null;
        }

        /// <summary>
        /// Nexts the day of the week.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="sDayOfWeek">The s day of week.</param>
        /// <param name="includeCurrentDay">if set to <c>true</c> [include current day].</param>
        /// <param name="numberOfWeeks">The number of weeks.</param>
        /// <returns></returns>
        public static DateTime? NextDayOfTheWeek( object input, string sDayOfWeek, bool includeCurrentDay = false, int numberOfWeeks = 1 )
        {
            DateTime date;
            DayOfWeek dayOfWeek;

            if ( input == null )
            {
                return null;
            }

            // Get the date value
            if ( input is DateTime )
            {
                date = ( DateTime ) input;
            }
            else
            {
                if ( input.ToString() == "Now" )
                {
                    date = RockDateTime.Now;
                }
                else
                {
                    DateTime.TryParse( input.ToString(), out date );
                }
            }

            if ( date == null )
            {
                return null;
            }

            // Get the day of week value
            if ( !Enum.TryParse( sDayOfWeek, out dayOfWeek ) )
            {
                return null;
            }

            // Calculate the offset
            int daysUntilWeekDay;

            if ( includeCurrentDay )
            {
                daysUntilWeekDay = ( (int) dayOfWeek - (int) date.DayOfWeek + 7 ) % 7;
            }
            else
            {
                daysUntilWeekDay = ( ( ( (int) dayOfWeek - 1 ) - (int) date.DayOfWeek + 7 ) % 7 ) + 1;
            }

            return date.AddDays( daysUntilWeekDay * numberOfWeeks );
        }

        /// <summary>
        /// Days until a given date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static int? DaysUntil( object input )
        {
            DateTime date;

            if ( input == null )
            {
                return null;
            }

            if ( input is DateTime )
            {
                date = ( DateTime ) input;
            }
            else
            {
                DateTime.TryParse( input.ToString(), out date );
            }

            if ( date == null )
            {
                return null;
            }

            return ( date - RockDateTime.Now ).Days;
        }

        /// <summary>
        /// Days the since a given date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static int? DaysSince( object input )
        {
            var days = DaysUntil( input );

            if ( days.HasValue )
            {
                return days.Value * -1;
            }

            return null;
        }

        #endregion

        #region Number Filters

        /// <summary>
        /// Formats the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string Format( object input, string format )
        {
            if ( input == null )
            {
                return null;
            }
            else if ( string.IsNullOrWhiteSpace( format ) )
            {
                return input.ToString();
            }

            return string.Format( "{0:" + format + "}", input );
        }

        /// <summary>
        /// Formats the specified input as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( object input )
        {
            if ( input == null )
            {
                return null;
            }

            if ( input is string )
            {
                // if the input is a string, just append the currency symbol to the front, even if it can't be converted to a number
                var currencySymbol = GlobalAttributesCache.Value( "CurrencySymbol" );
                return string.Format( "{0}{1}", currencySymbol, input );
            }
            else
            {
                // if the input an integer, decimal, double or anything else that can be parsed as a decimal, format that
                decimal? inputAsDecimal = input.ToString().AsDecimalOrNull();
                return inputAsDecimal.FormatAsCurrency();
            }
        }

        /// <summary>
        /// Addition - Overriding this to change the logic. The default filter will concat if the type is
        /// string. This one does the math if the input can be parsed as a int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Plus( object input, object operand )
        {
            if ( input == null || operand == null )
            {
                return input;
            }

            int intInput = -1;
            int intOperand = -1;
            decimal iInput = -1;
            decimal iOperand = -1;

            // If both input and operand are INTs keep the return an int.
            if ( int.TryParse( input.ToString(), out intInput ) && int.TryParse( operand.ToString(), out intOperand ) )
            {
                return intInput + intOperand;
            }
            else if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput + iOperand;
            }
            else
            {
                return string.Concat( input, operand );
            }
        }

        /// <summary>
        /// Minus - Overriding this to change the logic. This one does the math if the input can be parsed as a int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Minus( object input, object operand )
        {
            if ( input == null || operand == null  )
            {
                return input;
            }

            int intInput = -1;
            int intOperand = -1;
            decimal iInput = -1;
            decimal iOperand = -1;

            // If both input and operand are INTs keep the return an int.
            if ( int.TryParse( input.ToString(), out intInput ) && int.TryParse( operand.ToString(), out intOperand ) )
            {
                return intInput - intOperand;
            }
            else if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput - iOperand;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Times - Overriding this to change the logic. This one does the math if the input can be parsed as a int or decimal.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Times( object input, object operand )
        {
            if ( input == null || operand == null )
            {
                return input;
            }

            int intInput = -1;
            int intOperand = -1;
            decimal iInput = -1;
            decimal iOperand = -1;

            // If both input and operand are INTs keep the return an int.
            if ( int.TryParse( input.ToString(), out intInput ) && int.TryParse( operand.ToString(), out intOperand ) )
            {
                return intInput * intOperand;
            }
            else if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput * iOperand;
            }
            else
            {
                return Enumerable.Repeat( (string)input, (int)operand );
            }
        }

        /// <summary>
        /// Divideds the by.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="operand">The operand.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        public static object DividedBy( object input, object operand, int precision = 2 )
        {
            if ( input == null || operand == null )
            {
                return null;
            }

            try
            {
                decimal dInput = 0;
                decimal dOperand = 0;

                if ( decimal.TryParse( input.ToString(), out dInput ) && decimal.TryParse( operand.ToString(), out dOperand ) )
                {
                    decimal result = ( dInput / dOperand );
                    return decimal.Round( result, precision );
                }

                return "Could not convert input to number";
            }
            catch ( Exception ex )
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Floors the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object Floor( object input )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) )
            {
                return decimal.Floor( iInput );
            }
            else
            {
                return "Could not convert input to number to round";
            }
        }

        /// <summary>
        /// Ceilings the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object Ceiling( object input )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) )
            {
                return decimal.Ceiling( iInput );
            }
            else
            {
                return "Could not convert input to number to round";
            }
        }

        #endregion

        #region Attribute Filters

        /// <summary>
        /// DotLiquid Attribute Filter
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Attribute( DotLiquid.Context context, object input, string attributeKey, string qualifier = "" )
        {
            Attribute.IHasAttributes item = null;

            if ( input == null || attributeKey == null )
            {
                return string.Empty;
            }

            AttributeCache attribute = null;
            string rawValue = string.Empty;
            int? entityId = null;

            // If Input is "Global" then look for a global attribute with key
            if ( input.ToString().Equals( "Global", StringComparison.OrdinalIgnoreCase ) )
            {
                var globalAttributeCache = GlobalAttributesCache.Get();
                attribute = globalAttributeCache.Attributes
                    .FirstOrDefault( a => a.Key.Equals( attributeKey, StringComparison.OrdinalIgnoreCase ) );
                if ( attribute != null )
                {
                    // Get the value
                    string theValue = globalAttributeCache.GetValue( attributeKey );
                    if ( theValue.HasMergeFields() )
                    {
                        // Global attributes may reference other global attributes, so try to resolve this value again
                        var mergeFields = new Dictionary<string, object>();
                        if ( context.Environments.Count > 0 )
                        {
                            foreach( var keyVal in context.Environments[0] )
                            {
                                mergeFields.Add( keyVal.Key, keyVal.Value );
                            }
                        }
                        rawValue = theValue.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        rawValue = theValue;
                    }
                }
            }

            // If input is an object that has attributes, find its attribute value
            else
            {
                if ( input is Attribute.IHasAttributes )
                {
                    item = (Attribute.IHasAttributes)input;
                }
                else if ( input is IHasAttributesWrapper )
                {
                    item = ( (IHasAttributesWrapper)input ).HasAttributesEntity;
                }

                if ( item != null )
                {
                    if ( item.Attributes == null )
                    {
                        item.LoadAttributes();
                    }

                    if ( item.Attributes.ContainsKey( attributeKey ) )
                    {
                        attribute = item.Attributes[attributeKey];
                        rawValue = item.AttributeValues[attributeKey].Value;
                        entityId = item.Id;
                    }
                }
            }

            // If valid attribute and value were found
            if ( attribute != null && !string.IsNullOrWhiteSpace( rawValue ) )
            {
                Person currentPerson = GetCurrentPerson( context );

                if ( attribute.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    // Check qualifier for 'Raw' if present, just return the raw unformatted value
                    if ( qualifier.Equals( "RawValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return rawValue;
                    }

                    // Check qualifier for 'Url' and if present and attribute's field type is a ILinkableFieldType, then return the formatted url value
                    var field = attribute.FieldType.Field;
                    if ( qualifier.Equals( "Url", StringComparison.OrdinalIgnoreCase ) && field is Rock.Field.ILinkableFieldType )
                    {
                        return ( (Rock.Field.ILinkableFieldType)field ).UrlLink( rawValue, attribute.QualifierValues );
                    }

                    // check if attribute is a key value list and return a collection of key/value pairs
                    if ( field is Rock.Field.Types.KeyValueListFieldType )
                    {
                        var keyValueField = (Rock.Field.Types.KeyValueListFieldType)field;

                        return keyValueField.GetValuesFromString( null, rawValue, attribute.QualifierValues, false );
                    }

                    // If qualifier was specified, and the attribute field type is an IEntityFieldType, try to find a property on the entity
                    if ( !string.IsNullOrWhiteSpace( qualifier ) && field is Rock.Field.IEntityFieldType )
                    {
                        IEntity entity = ( (Rock.Field.IEntityFieldType)field ).GetEntity( rawValue );
                        if ( entity != null )
                        {
                            if ( qualifier.Equals( "object", StringComparison.OrdinalIgnoreCase ) )
                            {
                                return entity;
                            }
                            else
                            {
                                return entity.GetPropertyValue( qualifier ).ToStringSafe();
                            }
                        }
                    }

                    if ( qualifier.Equals( "Object", StringComparison.OrdinalIgnoreCase ) && field is Rock.Field.ICachedEntitiesFieldType )
                    {
                        var cachedEntitiesField = ( Rock.Field.ICachedEntitiesFieldType ) field;
                        var values = cachedEntitiesField.GetCachedEntities( rawValue );

                        if ( values == null || values.Count == 0 )
                        {
                            return null;
                        }

                        // If the attribute is configured to allow multiple then return a collection, otherwise just return a single value. You're welcome Lava developers :)
                        if ( attribute.QualifierValues != null && attribute.QualifierValues.ContainsKey( "allowmultiple") && attribute.QualifierValues["allowmultiple"].Value.AsBoolean() )
                        {
                            return values;
                        }
                        else
                        {
                            return values.FirstOrDefault();
                        }
                    }

                    // Otherwise return the formatted value
                    return field.FormatValue( null, attribute.EntityTypeId, entityId, rawValue, attribute.QualifierValues, false );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Properties the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Property( DotLiquid.Context context, object input, string propertyKey, string qualifier = "" )
        {
            if ( input != null )
            {
                if ( input is IDictionary<string, object> )
                {
                    var dictionaryObject = input as IDictionary<string, object>;
                    if ( dictionaryObject.ContainsKey( propertyKey ) )
                    {
                        return dictionaryObject[ propertyKey];
                    }
                }

                return input.GetPropertyValue( propertyKey );
            }

            return string.Empty;
        }

        #endregion

        #region Person Filters

        /// <summary>
        /// Sets the person preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void SetUserPreference( DotLiquid.Context context, object input, string settingKey, string settingValue )
        {
            Person person = null;

            if (input is int )
            {
                person = new PersonService( new RockContext() ).Get( (int)input );
            }
            else if (input is Person )
            {
                person = (Person)input;
            }

            if (person != null )
            {
                PersonService.SaveUserPreference( person, settingKey, settingValue );
            }
        }

        /// <summary>
        /// Gets the person preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <returns></returns>
        public static string GetUserPreference( DotLiquid.Context context, object input, string settingKey )
        {
            Person person = null;

            if ( input is int )
            {
                person = new PersonService( new RockContext() ).Get( (int)input );
            }
            else if ( input is Person )
            {
                person = (Person)input;
            }

            if ( person != null )
            {
                return PersonService.GetUserPreference( person, settingKey );
            }

            return string.Empty;
        }

        /// <summary>
        /// Deletes the user preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        public static void DeleteUserPreference( DotLiquid.Context context, object input, string settingKey )
        {
            Person person = null;

            if ( input is int )
            {
                person = new PersonService( new RockContext() ).Get( (int)input );
            }
            else if ( input is Person )
            {
                person = (Person)input;
            }

            if ( person != null )
            {
                PersonService.DeleteUserPreference( person, settingKey );
            }
        }

        /// <summary>
        /// Persons the by identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonById( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int personId = -1;

            if ( !Int32.TryParse( input.ToString(), out personId ) )
            {
                return null;
            }

            var rockContext = new RockContext();

            return new PersonService( rockContext ).Get( personId );
        }

        /// <summary>
        /// Persons the by unique identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByGuid( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? personGuid = input.ToString().AsGuidOrNull();

            if ( personGuid.HasValue )
            {
                var rockContext = new RockContext();

                return new PersonService( rockContext ).Get( personGuid.Value );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a person by their alias guid
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByAliasGuid( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? personAliasGuid = input.ToString().AsGuidOrNull();

            if ( personAliasGuid.HasValue )
            {
                var rockContext = new RockContext();

                return new PersonAliasService( rockContext ).Get( personAliasGuid.Value ).Person;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Persons the by alias identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByAliasId( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int personAliasId = -1;

            if ( !Int32.TryParse( input.ToString(), out personAliasId ) )
            {
                return null;
            }

            var rockContext = new RockContext();

            return new PersonAliasService( rockContext ).Get( personAliasId ).Person;
        }

        /// <summary>
        /// Returns a Person from the person alternate identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByPersonAlternateId( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            var alternateId = input.ToString();

            if ( alternateId.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var alternateIdSearchTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
            return new PersonSearchKeyService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( k =>
                        k.SearchValue == alternateId
                        && k.SearchTypeValueId == alternateIdSearchTypeValueId )
                    .Select( k => k.PersonAlias.Person )
                    .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person's alternate identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string GetPersonAlternateId( DotLiquid.Context context, object input )
        {
            Person person = null;

            if ( input is int )
            {
                person = new PersonService( new RockContext() ).Get( ( int ) input );
            }
            else if ( input is Person )
            {
                person = ( Person ) input;
            }

            if ( person != null )
            {
                var alternateIdSearchTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                return person.GetPersonSearchKeys().Where( k => k.SearchTypeValueId == alternateIdSearchTypeValueId ).FirstOrDefault().SearchValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the parents of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Parents( DotLiquid.Context context, object input )
        {
            Person person = GetPerson( input );

            if ( person != null )
            {
                Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                var parents = new PersonService( new RockContext() ).GetFamilyMembers( person.Id ).Where( m => m.GroupRole.Guid == adultGuid ).Select( a => a.Person );
                return parents.ToList();
            }

            return new List<Person>();
        }

        /// <summary>
        /// Gets the children of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Children( DotLiquid.Context context, object input )
        {
            Person person = GetPerson( input );

            if ( person != null )
            {
                Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                var children = new PersonService( new RockContext() ).GetFamilyMembers( person.Id ).Where( m => m.GroupRole.Guid == childGuid ).Select( a => a.Person );
                return children.ToList();
            }

            return new List<Person>();
        }

        /// <summary>
        /// Gets an address for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static string Address( DotLiquid.Context context, object input, string addressType, string qualifier = "" )
        {
            Person person = GetPerson( input );

            if ( person != null )
            {
                var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

                Location location = null;

                switch ( addressType )
                {
                    case "Mailing":
                        location = new GroupMemberService( GetRockContext( context ) )
                            .Queryable( "GroupLocations.Location" )
                            .AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.Group.GroupTypeId == familyGroupTypeId )
                            .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                            .SelectMany( m => m.Group.GroupLocations )
                            .Where( gl =>
                                gl.IsMailingLocation == true )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        break;
                    case "MapLocation":
                        location = new GroupMemberService( GetRockContext( context ) )
                            .Queryable( "GroupLocations.Location" )
                            .AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.Group.GroupTypeId == familyGroupTypeId )
                            .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                            .SelectMany( m => m.Group.GroupLocations )
                            .Where( gl =>
                                gl.IsMappedLocation == true )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        break;
                    default:
                        location = new GroupMemberService( GetRockContext( context ) )
                            .Queryable( "GroupLocations.Location" )
                            .AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.Group.GroupTypeId == familyGroupTypeId )
                            .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                            .SelectMany( m => m.Group.GroupLocations )
                            .Where( gl =>
                                gl.GroupLocationTypeValue.Value == addressType )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        break;
                }

                if ( location != null )
                {
                    if ( qualifier == "" )
                    {
                        return location.GetFullStreetAddress();
                    }
                    else
                    {
                        var matches = Regex.Matches( qualifier, @"\[\[([^\]]+)\]\]" );
                        foreach ( var match in matches )
                        {
                            string propertyKey = match.ToString().Replace( "[", "" );
                            propertyKey = propertyKey.ToString().Replace( "]", "" );
                            propertyKey = propertyKey.ToString().Replace( " ", "" );

                            switch ( propertyKey )
                            {
                                case "Street1":
                                    qualifier = qualifier.Replace( match.ToString(), location.Street1 );
                                    break;
                                case "Street2":
                                    qualifier = qualifier.Replace( match.ToString(), location.Street2 );
                                    break;
                                case "City":
                                    qualifier = qualifier.Replace( match.ToString(), location.City );
                                    break;
                                case "County":
                                    qualifier = qualifier.Replace( match.ToString(), location.County );
                                    break;
                                case "State":
                                    qualifier = qualifier.Replace( match.ToString(), location.State );
                                    break;
                                case "PostalCode":
                                case "Zip":
                                    qualifier = qualifier.Replace( match.ToString(), location.PostalCode );
                                    break;
                                case "Country":
                                    qualifier = qualifier.Replace( match.ToString(), location.Country );
                                    break;
                                case "GeoPoint":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), string.Format( "{0},{1}", location.GeoPoint.Latitude.ToString(), location.GeoPoint.Longitude.ToString() ) );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "Latitude":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), location.GeoPoint.Latitude.ToString() );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "Longitude":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), location.GeoPoint.Longitude.ToString() );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "FormattedAddress":
                                    qualifier = qualifier.Replace( match.ToString(), location.FormattedAddress );
                                    break;
                                case "FormattedHtmlAddress":
                                    qualifier = qualifier.Replace( match.ToString(), location.FormattedHtmlAddress );
                                    break;
                                default:
                                    qualifier = qualifier.Replace( match.ToString(), "" );
                                    break;
                            }
                        }

                        return qualifier;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the Spouse of the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person Spouse( DotLiquid.Context context, object input )
        {
            Person person = GetPerson( input );

            if ( person == null )
            {
                return null;
            }
            return person.GetSpouse();
        }

        /// <summary>
        /// Gets the Head of Household of the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person HeadOfHousehold( DotLiquid.Context context, object input )
        {
            Person person = GetPerson( input );

            if ( person == null )
            {
                return null;
            }
            return person.GetHeadOfHousehold();
        }

        /// <summary>
        /// Families the salutation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="includeChildren">if set to <c>true</c> [include children].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="useFormalNames">if set to <c>true</c> [use formal names].</param>
        /// <param name="finalfinalSeparator">The finalfinal separator.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string FamilySalutation( DotLiquid.Context context, object input, bool includeChildren = false, bool includeInactive = true, bool useFormalNames = false, string finalfinalSeparator = "&", string separator = "," )
        {
            Person person = GetPerson( input );

            if ( person == null )
            {
                return null;
            }

            return Person.GetFamilySalutation(person, includeChildren, includeInactive, useFormalNames, finalfinalSeparator, separator);
        }

        /// <summary>
        /// Gets an number for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="phoneType">Type of the phone number.</param>
        /// <param name="countryCode">Whether or not there should be a country code returned</param>
        /// <returns></returns>
        public static string PhoneNumber( DotLiquid.Context context, object input, string phoneType = "Home", bool countryCode = false )
        {
            Person person = GetPerson( input );


            string phoneNumber = null;

            if ( person != null )
            {
                var phoneNumberQuery = new PhoneNumberService( GetRockContext( context ) )
                            .Queryable()
                            .AsNoTracking()
                            .Where( p =>
                               p.PersonId == person.Id )
                            .Where( a => a.NumberTypeValue.Value == phoneType )
                            .FirstOrDefault();

                if ( phoneNumberQuery != null )
                {
                    if ( countryCode && !string.IsNullOrEmpty( phoneNumberQuery.CountryCode ) )
                    {
                        phoneNumber = phoneNumberQuery.NumberFormattedWithCountryCode;
                    }
                    else
                    {
                        phoneNumber = phoneNumberQuery.NumberFormatted;
                    }
                }
            }

            return phoneNumber;
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns>
        /// A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:{fileName},P,P,{contentLength},,{zplImageData} ^FD").
        /// </returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input )
        {
            return ZebraPhoto( context, input, "395" );
        }
        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input, which is the person.</param>
        /// <param name="size">The size.</param>
        /// <returns>
        /// A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:{fileName},P,P,{contentLength},,{zplImageData} ^FD").
        /// </returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input, string size )
        {
            return ZebraPhoto( context, input, size, 1.0, 1.0 );
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="size">The size.</param>
        /// <param name="brightness">The brightness.</param>
        /// <param name="contrast">The contrast.</param>
        /// <returns>
        /// A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:{fileName},P,P,{contentLength},,{zplImageData} ^FD").
        /// </returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input, string size, double brightness, double contrast )
        {
            return ZebraPhoto( context, input, size, brightness, contrast, "LOGO" );
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input, which is the person.</param>
        /// <param name="size">The size.</param>
        /// <param name="brightness">The brightness adjustment (-1.0 to 1.0).</param>
        /// <param name="contrast">The contrast adjustment (-1.0 to 1.0).</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:{fileName},P,P,{contentLength},,{zplImageData} ^FD").
        /// </returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input, string size, double brightness, double contrast, string fileName )
        {
            return ZebraPhoto( context, input, size, brightness, contrast, fileName, 0 );
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input, which is the person.</param>
        /// <param name="size">The size.</param>
        /// <param name="brightness">The brightness adjustment (-1.0 to 1.0).</param>
        /// <param name="contrast">The contrast adjustment (-1.0 to 1.0).</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="rotationDegree">The degree of rotation to apply to the image (0, 90, 180, 270).</param>
        /// <returns>
        /// A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:{fileName},P,P,{contentLength},,{zplImageData} ^FD").
        /// </returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input, string size, double brightness, double contrast, string fileName, int rotationDegree )
        {
            var person = GetPerson( input );
            try
            {
                if ( person != null )
                {
                    Stream initialPhotoStream;
                    if ( person.PhotoId.HasValue )
                    {
                        initialPhotoStream = new BinaryFileService( GetRockContext( context ) ).Get( person.PhotoId.Value ).ContentStream;
                    }
                    else
                    {
                        var photoUrl = new StringBuilder();
                        photoUrl.Append( HttpContext.Current.Server.MapPath( "~/" ) );

                        if ( person.Age.HasValue && person.Age.Value < 18 )
                        {
                            // it's a child
                            if ( person.Gender == Gender.Female )
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-child-female.png" );
                            }
                            else
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-child-male.png" );
                            }
                        }
                        else
                        {
                            // it's an adult
                            if ( person.Gender == Gender.Female )
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-adult-female.png" );
                            }
                            else
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-adult-male.png" );
                            }
                        }

                        initialPhotoStream = File.Open( photoUrl.ToString(), FileMode.Open );
                    }

                    Bitmap initialBitmap = new Bitmap( initialPhotoStream );

                    // Adjust the image if any of the parameters not default
                    if ( brightness != 1.0 || contrast != 1.0 )
                    {
                        initialBitmap = ImageAdjust( initialBitmap, (float)brightness, (float)contrast );
                    }

                    // Calculate rectangle to crop image into
                    int height = initialBitmap.Height;
                    int width = initialBitmap.Width;
                    Rectangle cropSection = new Rectangle( 0, 0, height, width );
                    if ( height < width )
                    {
                        cropSection = new Rectangle( ( width - height ) / 2, 0, ( width + height ) / 2, height ); // (width + height)/2 is a simplified version of the (width - height)/2 + height function
                    }
                    else if ( height > width )
                    {
                        cropSection = new Rectangle( 0, ( height - width ) / 2, width, ( height + width ) / 2 );
                    }

                    // Crop and resize image
                    int pixelSize = size.AsIntegerOrNull() ?? 395;
                    Bitmap resizedBitmap = new Bitmap( pixelSize, pixelSize );
                    using ( Graphics g = Graphics.FromImage( resizedBitmap ) )
                    {
                        g.DrawImage( initialBitmap, new Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), cropSection, GraphicsUnit.Pixel );
                    }

                    // Grayscale Image
                    var masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
                    var outputBitmap = new Bitmap( resizedBitmap.Width, resizedBitmap.Height, PixelFormat.Format1bppIndexed );
                    var data = new sbyte[resizedBitmap.Width, resizedBitmap.Height];
                    var inputData = resizedBitmap.LockBits( new Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb );
                    try
                    {
                        var scanLine = inputData.Scan0;
                        var line = new byte[inputData.Stride];
                        for ( var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride )
                        {
                            Marshal.Copy( scanLine, line, 0, line.Length );
                            for ( var x = 0; x < resizedBitmap.Width; x++ )
                            {
                                // Change to greyscale
                                data[x, y] = (sbyte)( 64 * ( ( ( line[x * 3 + 2] * 0.299 + line[x * 3 + 1] * 0.587 + line[x * 3 + 0] * 0.114 ) / 255 ) - 0.4 ) );
                            }
                        }
                    }
                    finally
                    {
                        resizedBitmap.UnlockBits( inputData );
                    }

                    //Dither Image
                    var outputData = outputBitmap.LockBits( new Rectangle( 0, 0, outputBitmap.Width, outputBitmap.Height ), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed );
                    try
                    {
                        var scanLine = outputData.Scan0;
                        for ( var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride )
                        {
                            var line = new byte[outputData.Stride];
                            for ( var x = 0; x < resizedBitmap.Width; x++ )
                            {
                                var j = data[x, y] > 0;
                                if ( j ) line[x / 8] |= masks[x % 8];
                                var error = (sbyte)( data[x, y] - ( j ? 32 : -32 ) );
                                if ( x < resizedBitmap.Width - 1 ) data[x + 1, y] += (sbyte)( 7 * error / 16 );
                                if ( y < resizedBitmap.Height - 1 )
                                {
                                    if ( x > 0 ) data[x - 1, y + 1] += (sbyte)( 3 * error / 16 );
                                    data[x, y + 1] += (sbyte)( 5 * error / 16 );
                                    if ( x < resizedBitmap.Width - 1 ) data[x + 1, y + 1] += (sbyte)( 1 * error / 16 );
                                }
                            }

                            Marshal.Copy( line, 0, scanLine, outputData.Stride );
                        }
                    }
                    finally
                    {
                        outputBitmap.UnlockBits( outputData );
                    }

                    // Rotate image
                    switch ( rotationDegree )
                    {
                        case 90:
                            {
                                outputBitmap.RotateFlip( RotateFlipType.Rotate90FlipNone );
                                break;
                            }
                        case 180:
                            {
                                outputBitmap.RotateFlip( RotateFlipType.Rotate180FlipNone );
                                break;
                            }
                        case 270:
                        case -90:
                            {
                                outputBitmap.RotateFlip( RotateFlipType.Rotate270FlipNone );
                                break;
                            }
                    }

                    // Convert from x to .png
                    MemoryStream convertedStream = new MemoryStream();
                    outputBitmap.Save( convertedStream, System.Drawing.Imaging.ImageFormat.Png );
                    convertedStream.Seek( 0, SeekOrigin.Begin );

                    // Convert the .png stream into a ZPL-readable Hex format
                    var content = convertedStream.ReadBytesToEnd();
                    StringBuilder zplImageData = new StringBuilder();

                    foreach ( Byte b in content )
                    {
                        string hexRep = string.Format( "{0:X}", b );
                        if ( hexRep.Length == 1 )
                            hexRep = "0" + hexRep;
                        zplImageData.Append( hexRep );
                    }

                    convertedStream.Dispose();
                    initialPhotoStream.Dispose();

                    return string.Format( "^FS ~DYR:{0},P,P,{1},,{2} ^FD", fileName, content.Length, zplImageData.ToString() );
                }
            }
            catch
            {
                // intentionally blank
            }

            return string.Empty;
        }

        /// <summary>
        /// Adjust the brightness, contrast or gamma of the given image.
        /// </summary>
        /// <param name="originalImage">The original image.</param>
        /// <param name="brightness">The brightness multiplier (-1.99 to 1.99 fully white).</param>
        /// <param name="contrast">The contrast multiplier (2.0 would be twice the contrast).</param>
        /// <param name="gamma">The gamma multiplier (1.0 would no change in gamma).</param>
        /// <returns>A new adjusted image.</returns>
        private static Bitmap ImageAdjust( Bitmap originalImage, float brightness = 1.0f, float contrast = 1.0f, float gamma = 1.0f )
        {
            Bitmap adjustedImage = originalImage;

            float adjustedBrightness = brightness - 1.0f;
            // Matrix used to effect the image
            float[][] ptsArray = {
                new float[] { contrast, 0, 0, 0, 0 }, // scale red
                new float[] { 0, contrast, 0, 0, 0 }, // scale green
                new float[] { 0, 0, contrast, 0, 0 }, // scale blue
                new float[] { 0, 0, 0, 1.0f, 0 },     // no change to alpha
                new float[] { adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1 }
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix( new ColorMatrix( ptsArray ), ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
            imageAttributes.SetGamma( gamma, ColorAdjustType.Bitmap );
            Graphics g = Graphics.FromImage( adjustedImage );
            g.DrawImage( originalImage, new Rectangle( 0, 0, adjustedImage.Width, adjustedImage.Height ),
                0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes );

            return adjustedImage;
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Groups( DotLiquid.Context context, object input, string groupTypeId )
        {
            return Groups( context, input, groupTypeId, "Active", "Active" );
        }

        /// <summary>
        /// Groupses the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Groups( DotLiquid.Context context, object input, string groupTypeId, string status )
        {
        	return Groups( context, input, groupTypeId, status, "Active" );
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="memberStatus">The member status.</param>
        /// <param name="groupStatus">The group status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Groups( DotLiquid.Context context, object input, string groupTypeId, string memberStatus, string groupStatus )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                var groupQuery = new GroupMemberService( GetRockContext( context ) )
                    .Queryable( "Group, GroupRole" )
                    .AsNoTracking()
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == numericalGroupTypeId.Value &&
                        m.Group.IsActive && !m.Group.IsArchived );

                if ( groupStatus != "All" )
                {
                    groupQuery = groupQuery.Where( m => m.Group.IsActive );
                }

                if ( memberStatus != "All" )
                {
                    GroupMemberStatus queryStatus = GroupMemberStatus.Active;
                    queryStatus = (GroupMemberStatus)Enum.Parse( typeof( GroupMemberStatus ), memberStatus, true );

                    groupQuery = groupQuery.Where( m => m.GroupMemberStatus == queryStatus );
                }

                return groupQuery.ToList();
            }

            return new List<Model.GroupMember>();
        }

        /// <summary>
        /// Groups the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Group( DotLiquid.Context context, object input, string groupId, string status = "Active" )
        {
            var person = GetPerson( input );
            int? numericalGroupId = groupId.AsIntegerOrNull();

            if ( string.IsNullOrWhiteSpace( status ) )
            {
                status = "All";
            }

            if ( person != null && numericalGroupId.HasValue )
            {
                var groupQuery = new GroupMemberService( GetRockContext( context ) )
                    .Queryable( "Group, GroupRole" )
                    .AsNoTracking()
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.Id == numericalGroupId.Value &&
                        m.Group.IsActive && !m.Group.IsArchived );

                if ( status != "All" )
                {
                    GroupMemberStatus queryStatus = GroupMemberStatus.Active;
                    queryStatus = (GroupMemberStatus)Enum.Parse( typeof( GroupMemberStatus ), status, true );

                    groupQuery = groupQuery.Where( m => m.GroupMemberStatus == queryStatus );
                }

                return groupQuery.ToList();
            }

            return new List<Model.GroupMember>();
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of which they have attended at least once
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Group> GroupsAttended( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new AttendanceService( GetRockContext( context ) ).Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.Occurrence.Group != null &&
                        a.Occurrence.Group.GroupTypeId == numericalGroupTypeId &&
                        a.PersonAlias.PersonId == person.Id &&
                        a.DidAttend == true )
                    .Select( a => a.Occurrence.Group ).Distinct().ToList();
            }

            return new List<Model.Group>();
        }

        /// <summary>
        /// Gets the last attendance item for a given person in a group of type provided
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Attendance LastAttendedGroupOfType( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person == null && !numericalGroupTypeId.HasValue )
            {
                return new Attendance();
            }

            return new AttendanceService( GetRockContext( context ) ).Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group != null &&
                    a.Occurrence.Group.GroupTypeId == numericalGroupTypeId &&
                    a.PersonAlias.PersonId == person.Id &&
                    a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the groups of selected type that geofence the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Group> GeofencingGroups( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new GroupService( GetRockContext( context ) )
                    .GetGeofencingGroups( person.Id, numericalGroupTypeId.Value )
                    .ToList();
            }

            return new List<Model.Group>();
        }

        /// <summary>
        /// Gets the groups of selected type that geofence the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="groupTypeRoleId">The group type role identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Person> GeofencingGroupMembers( DotLiquid.Context context, object input, string groupTypeId, string groupTypeRoleId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();
            int? numericalGroupTypeRoleId = groupTypeRoleId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue && numericalGroupTypeRoleId.HasValue )
            {
                return new GroupService( GetRockContext( context ) )
                    .GetGeofencingGroups( person.Id, numericalGroupTypeId.Value )
                    .SelectMany( g => g.Members.Where( m => m.GroupRole.Id == numericalGroupTypeRoleId ) )
                    .Select( m => m.Person )
                    .ToList();
            }

            return new List<Model.Person>();
        }

        /// <summary>
        /// Returnes the nearest group of a specific type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Rock.Model.Group NearestGroup( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new GroupService( GetRockContext( context ) )
                    .GetNearestGroup( person.Id, numericalGroupTypeId.Value );
            }

            return null;
        }

        /// <summary>
        /// Returns the Campus (or Campuses) that the Person belongs to
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public static object Campus( DotLiquid.Context context, object input, object option = null )
        {
            var person = GetPerson( input );

            bool getAll = false;
            if ( option != null && option.GetType() == typeof( string ) )
            {
                // if a string of "all" is specified for the option, return all of the campuses (if they are part of multiple families from different campuses)
                if ( string.Equals( (string)option, "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    getAll = true;
                }
            }

            if ( getAll )
            {
                return person.GetFamilies().Select( a => a.Campus ).OrderBy( a => a.Name );
            }
            else
            {
                return person.GetCampus();
            }

        }

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static RockContext GetRockContext( DotLiquid.Context context )
        {
            if ( context.Registers.ContainsKey( "rock_context" ) )
            {
                return context.Registers["rock_context"] as RockContext;
            }
            else
            {
                var rockContext = new RockContext();
                context.Registers["rock_context"] = rockContext;
                return rockContext;
            }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static Person GetPerson( object input )
        {
            if ( input != null )
            {
                if ( input is int )
                {
                    return new PersonService( new RockContext() ).Get( ( int ) input );
                }

                var person = input as Person;
                if ( person != null )
                {
                    return person;
                }

                var checkinPerson = input as CheckIn.CheckInPerson;
                if ( checkinPerson != null )
                {
                    return checkinPerson.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the Notes of the entity
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="documentTemplateId">The Id number of the signed document type to query for.</param>
        /// <param name="trueValue">The value to be returned if the person has signed the document.</param>
        /// <param name="falseValue">The value to be returned if the person has not signed the document.</param>
        /// <returns></returns>
        public static object HasSignedDocument( DotLiquid.Context context, object input, object documentTemplateId, object trueValue = null, object falseValue = null )
        {
            int personId;
            int templateId;

            trueValue = trueValue ?? true;
            falseValue = falseValue ?? false;

            if ( input == null || documentTemplateId == null )
            {
                return falseValue;
            }

            templateId = documentTemplateId.ToString().AsInteger();

            if ( input is Person )
            {
                personId = ( input as Person ).Id;
            }
            else
            {
                personId = input.ToString().AsInteger();
            }

            bool found = new SignatureDocumentService( new RockContext() )
                .Queryable().AsNoTracking()
                .Where( d =>
                    d.SignatureDocumentTemplateId == templateId &&
                    d.Status == SignatureDocumentStatus.Signed &&
                    d.BinaryFileId.HasValue &&
                    d.AppliesToPersonAlias.PersonId == personId )
                .Any();

            return found ? trueValue : falseValue;
        }

        /// <summary>
        /// Creates a Person Action Identifier (rckid) for the specified Person (person can be specified by Person, Guid, or Id) for specific action.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static string PersonActionIdentifier( DotLiquid.Context context, object input, string action )
        {
            Person person = GetPerson( input ) ?? PersonById( context, input ) ?? PersonByGuid( context, input );

            if ( person != null )
            {
                return person.GetPersonActionIdentifier( action );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a Person Token (rckipid) for the specified Person (person can be specified by Person, Guid, or Id). Specify ExpireMinutes, UsageLimit and PageId to
        /// limit the usage of the token for the specified number of minutes, usage count, and specific pageid
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="expireMinutes">The expire minutes.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static string PersonTokenCreate( DotLiquid.Context context, object input, int? expireMinutes = null, int? usageLimit = null, int? pageId = null )
        {
            Person person = GetPerson( input ) ?? PersonById( context, input ) ?? PersonByGuid( context, input );

            if ( person != null )
            {
                DateTime? expireDateTime = null;
                if ( expireMinutes.HasValue )
                {
                    expireDateTime = RockDateTime.Now.AddMinutes( expireMinutes.Value );
                }

                if ( pageId.HasValue )
                {
                    var page = new PageService( new RockContext() ).Get( pageId.Value );
                    if ( page == null )
                    {
                        // invalid page specified, so don't return a token
                        return null;
                    }
                }

                return person.GetImpersonationToken( expireDateTime, usageLimit, pageId );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Looks up a Person using an encrypted person token (rckipid) with an option to incrementUsage and to validate against a specific page
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="incrementUsage">if set to <c>true</c> [increment usage].</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static Person PersonTokenRead( DotLiquid.Context context, object input, bool incrementUsage = false, int? pageId = null )
        {
            string encryptedPersonToken = input as string;

            if ( !string.IsNullOrEmpty( encryptedPersonToken ) )
            {
                var rockContext = new RockContext();


                return new PersonService( rockContext ).GetByImpersonationToken( encryptedPersonToken, incrementUsage, pageId );
            }
            else
            {
                return null;
            }
        }

        #endregion Person

        #region Group Filters

        /// <summary>
        /// Loads a Group record from the database from it's GUID.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Rock.Model.Group GroupByGuid( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? groupGuid = input.ToString().AsGuidOrNull();

            if ( groupGuid.HasValue )
            {
                var rockContext = new RockContext();

                return new GroupService( rockContext ).Get( groupGuid.Value );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a Group record from the database from it's Identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Rock.Model.Group GroupById( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int groupId = -1;

            if ( !Int32.TryParse( input.ToString(), out groupId ) )
            {
                return null;
            }

            var rockContext = new RockContext();

            return new GroupService( rockContext ).Get( groupId );
        }

        #endregion

        #region Misc Filters

        /// <summary>
        /// Shows details about which Merge Fields are available
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">If a merge field is specified, only Debug info on that MergeField will be shown</param>
        /// <param name="option1">either userName or outputFormat</param>
        /// <param name="option2">either userName or outputFormat</param>
        /// <returns></returns>
        public static string Debug( DotLiquid.Context context, object input, string option1 = null, string option2 = null )
        {
            string[] outputFormats = new string[] { "Ascii", "Html" };
            string userName = null;
            string outputFormat = null;

            // detect if option1 or option2 is the outputFormat or userName parameter
            if ( outputFormats.Any( f => f.Equals( option1, StringComparison.OrdinalIgnoreCase ) ) )
            {
                outputFormat = option1;
                userName = option2;
            }
            else if ( outputFormats.Any( f => f.Equals( option2, StringComparison.OrdinalIgnoreCase ) ) )
            {
                outputFormat = option2;
                userName = option1;
            }
            else
            {
                userName = option1 ?? option2;
            }

            if ( userName.IsNotNullOrWhiteSpace() )
            {
                // if userName was specified, don't return anything if the currentPerson doesn't have a matching userName
                var currentPerson = GetCurrentPerson( context );
                if ( currentPerson != null )
                {
                    if ( !currentPerson.Users.Any( a => a.UserName.Equals( userName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        // currentUser doesn't have the specified userName, so return nothing
                        return null;
                    }
                }
                else
                {
                    // CurrentPerson is null so return nothing
                    return null;
                }
            }

            var mergeFields = context.Environments.SelectMany( a => a ).ToDictionary( k => k.Key, v => v.Value );

            var allFields = mergeFields.Union( context.Scopes.SelectMany( a => a ).DistinctBy(x => x.Key).ToDictionary( k => k.Key, v => v.Value ) );

            // if a specific MergeField was specified as the Input, limit the help to just that MergeField
            if ( input != null && allFields.Any( a => a.Value == input ) )
            {
                mergeFields = allFields.Where( a => a.Value == input ).ToDictionary( k => k.Key, v => v.Value );
            }

            // TODO: implement the outputFormat option to support ASCII
            return mergeFields.lavaDebugInfo();
        }

        /// <summary>
        /// Redirects the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string PageRedirect( string input )
        {
            // check for no redirect in query string
            string redirectValue = HttpContext.Current.Request.QueryString["Redirect"];

            if ( redirectValue != null && redirectValue == "false" )
            {
                return string.Format( "<p class='alert alert-warning'>Without the redirect query string parameter you would be redirected to: <a href='{0}'>{0}</a>.</p>", input );
            }

            if ( input != null )
            {
                // Don't call Redirect with a false -- we want it to throw the thread abort exception
                // so remaining lava does not continue to execute.  We'll catch the exception in the
                // LavaExtension's ResolveMergeFields method.
                HttpContext.Current.Response.Redirect( input, true );
            }

            return string.Empty;
        }

        /// <summary>
        /// Resolves the rock address.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ResolveRockUrl( string input )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( input.StartsWith( "~~" ) )
            {
                string theme = "Rock";
                if ( page.Theme.IsNotNullOrWhiteSpace() )
                {
                    theme = page.Theme;
                }
                else if ( page.Site != null && page.Site.Theme.IsNotNullOrWhiteSpace() )
                {
                    theme = page.Site.Theme;
                }

                input = "~/Themes/" + theme + (input.Length > 2 ? input.Substring( 2 ) : string.Empty);
            }

            return page.ResolveUrl( input );
        }

        /// <summary>
        /// From the cache.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="cacheType">Type of the cache.</param>
        /// <returns></returns>
        public static object FromCache( object input, string cacheType )
        {
            int? inputAsInt = null;
            Guid? inputAsGuid = null;

            // ensure they provided a cache type
            if ( input == null || cacheType.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // figure out the input type
            inputAsInt = input.ToString().AsIntegerOrNull();

            if ( !inputAsInt.HasValue ) // not an int try guid
            {
                inputAsGuid = input.ToString().AsGuidOrNull();
            }

            if ( inputAsGuid.HasValue || inputAsInt.HasValue )
            {
                switch ( cacheType )
                {
                    case "DefinedValue":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return DefinedValueCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return DefinedValueCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "DefinedType":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return DefinedTypeCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return DefinedTypeCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "Campus":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return CampusCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return CampusCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "Category":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return CategoryCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return CategoryCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "GroupType":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return GroupTypeCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return GroupTypeCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "Page":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return PageCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return PageCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "Block":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return BlockCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return BlockCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "BlockType":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return BlockTypeCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return BlockTypeCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "EventCalendar":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return EventCalendarCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return EventCalendarCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "Attribute":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return AttributeCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return AttributeCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "NoteType":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return NoteTypeCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return NoteTypeCache.Get( inputAsGuid.Value );
                            }
                        }
                    case "ContentChannel":
                        {
                            if ( inputAsInt.HasValue )
                            {
                                return ContentChannelCache.Get( inputAsInt.Value );
                            }
                            else
                            {
                                return ContentChannelCache.Get( inputAsGuid.Value );
                            }
                        }
                    default:
                        {
                            return $"Cache type {cacheType} not supported.";
                        }
                }
            }

            return null;
        }

        /// <summary>
        /// creates a postback javascript function
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public static string Postback( object input, string command )
        {
            if ( input != null )
            {
                return string.Format( "javascript:__doPostBack('[ClientId]','{0}^{1}'); return false;", command, input.ToString() );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns a JSON representation of the object.
        /// See https://www.rockrms.com/page/565#tojson
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ToJSON( object input )
        {
            return input.ToJson( Formatting.Indented );
        }

        /// <summary>
        /// Returns a dynamic object from a JSON string.
        /// See https://www.rockrms.com/page/565#fromjson
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object FromJSON( object input )
        {
            var converter = new ExpandoObjectConverter();
            object contentObject = null;
            var value = input as string;

            try
            {
                // first try to deserialize as straight ExpandoObject
                contentObject = JsonConvert.DeserializeObject<ExpandoObject>( value, converter );
            }
            catch
            {
                try
                {
                    // if it didn't deserialize as straight ExpandoObject, try it as a List of ExpandoObjects
                    contentObject = JsonConvert.DeserializeObject<List<ExpandoObject>>( value, converter );
                }
                catch
                {
                    // if it didn't deserialize as a List of ExpandoObject, try it as a List of plain objects
                    contentObject = JsonConvert.DeserializeObject<List<object>>( value, converter );
                }
            }

            return contentObject;
        }

        /// <summary>
        /// Converts Markdown to HTML
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string FromMarkdown( string input )
        {
            if ( input != null )
            {
                return input.ConvertMarkdownToHtml();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Convert strings within the text that appear to be http/ftp/https links into clickable html links
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Linkify( string input)
        {
            if ( input != null )
            {
                return input.Linkify();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// adds a meta tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the content attribute of the tag.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static string AddMetaTagToHead( string input, string attributeName, string attributeValue )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                HtmlMeta metaTag = new HtmlMeta();
                metaTag.Attributes.Add( attributeName, attributeValue );
                metaTag.Content = input;
                page.Header.Controls.Add( metaTag );
            }

            return null;
        }

        /// <summary>
        /// adds a link tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the href of the tag.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static string AddLinkTagToHead( string input, string attributeName, string attributeValue )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                HtmlLink imageLink = new HtmlLink();
                imageLink.Attributes.Add( attributeName, attributeValue );
                imageLink.Attributes.Add( "href", input );
                page.Header.Controls.Add( imageLink );
            }

            return null;
        }

        /// <summary>
        /// adds a link tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the href of the tag.</param>
        /// <param name="titleLocation">The title location.</param>
        /// <returns></returns>
        public static string SetPageTitle( string input, string titleLocation = "All" )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                

                if ( titleLocation.Equals("BrowserTitle", StringComparison.InvariantCultureIgnoreCase) || titleLocation.Equals( "All", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    page.BrowserTitle = input;
                    page.Header.Title = input;
                }

                if ( titleLocation.Equals( "PageTitle", StringComparison.InvariantCultureIgnoreCase ) || titleLocation.Equals( "All", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    // attempt to correct breadcrumbs
                    if ( page.BreadCrumbs != null && page.BreadCrumbs.Count != 0 )
                    {
                        var lastBookMark = page.BreadCrumbs.Last();

                        if ( lastBookMark != null && lastBookMark.Name == page.PageTitle )
                        {
                            lastBookMark.Name = input;
                        }
                    }

                    page.PageTitle = input;
                    page.Title = input;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the script link.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="fingerprintLink">if set to <c>true</c> [fingerprint link].</param>
        /// <returns></returns>
        public static string AddScriptLink( string input, bool fingerprintLink = false )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;
            RockPage.AddScriptLink( page, ResolveRockUrl( input ), fingerprintLink );

            return String.Empty;
        }

        /// <summary>
        /// Adds the CSS link.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="fingerprintLink">if set to <c>true</c> [fingerprint link].</param>
        /// <returns></returns>
        public static string AddCssLink( string input, bool fingerprintLink = false )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;
            RockPage.AddCSSLink( page, ResolveRockUrl( input ), fingerprintLink );

            return String.Empty;
        }

        /// <summary>
        /// Clients the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parm.</param>
        /// <returns></returns>
        public static object Client( string input, string parm )
        {
            parm = parm.ToUpper();

            switch ( parm )
            {
                case "IP":
                    {
                        string address = string.Empty;

                        // http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
                        string ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                        if ( !string.IsNullOrEmpty( ipAddress ) )
                        {
                            string[] addresses = ipAddress.Split( ',' );
                            if ( addresses.Length != 0 )
                            {
                                address = addresses[0];
                            }
                        }
                        else
                        {
                            address =  HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                        }

                        // nicely format localhost
                        if (address == "::1" )
                        {
                            address = "localhost";
                        }

                        return address;
                    }
                case "LOGIN": {
                        return HttpContext.Current.Request.ServerVariables["AUTH_USER"];
                    }
                case "BROWSER":
                    {
                        Parser uaParser = Parser.GetDefault();
                        ClientInfo client = uaParser.Parse( HttpContext.Current.Request.UserAgent );

                        return client;
                    }
                case "PARMLIST":
                    {
                        return string.Join( ", ", HttpContext.Current.Request.ServerVariables.AllKeys );
                    }
                default:
                    {
                        return HttpContext.Current.Request.ServerVariables[parm];
                    }
            }

        }

        /// <summary>
        /// Ratings the markup.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string RatingMarkup(object input )
        {
            var rating = 0;

            if (input!= null )
            {
                rating = input.ToString().AsInteger();
            }

            var starCounter = 0;
            StringBuilder starMarkup = new StringBuilder();

            for ( int i = 0; i < rating; i++ )
            {
                starMarkup.Append( "<i class='fa fa-rating-on'></i>" );
                starCounter++;
            }

            for ( int i = starCounter; i < 5; i++ )
            {
                starMarkup.Append( "<i class='fa fa-rating-off'></i>" );
            }

            return starMarkup.ToString();
        }

        /// <summary>
        /// Pages the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parm.</param>
        /// <returns></returns>
        public static object Page( string input, string parm )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                switch ( parm )
                {
                    case "Title":
                        {
                            return page.PageTitle;
                        }

                    case "BrowserTitle":
                        {
                            return page.BrowserTitle;
                        }

                    case "Url":
                        {
                            return HttpContext.Current.Request.Url.AbsoluteUri;
                        }

                    case "Id":
                        {
                            return page.PageId.ToString();
                        }

                    case "Host":
                        {
                            var host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );
                            return host;
                        }

                    case "Path":
                        {
                            return HttpContext.Current.Request.Url.AbsolutePath;
                        }

                    case "SiteName":
                        {
                            return page.Site.Name;
                        }

                    case "SiteId":
                        {
                            return page.Site.Id.ToString();
                        }

                    case "Theme":
                        {
                            if ( page.Theme != null )
                            {
                                return page.Theme;
                            }
                            else
                            {
                                return page.Site.Theme;
                            }
                        }
                    case "Description":
                        {
                            return page.MetaDescription;
                        }
                    case "Layout":
                        {
                            return page.Layout.Name;
                        }

                    case "Scheme":
                        {
                            return HttpContext.Current.Request.Url.Scheme;
                        }

                    case "QueryString":
                        {
                            return page.PageParameters();
                        }
                    case "Cookies":
                        {
                            var cookies = new List<HttpCookieDrop>();
                            foreach ( string cookieKey in System.Web.HttpContext.Current.Request.Cookies )
                            {
                                cookies.Add( new HttpCookieDrop( System.Web.HttpContext.Current.Request.Cookies[cookieKey] ) );
                            }

                            return cookies;
                        }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the specified page parm.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parm.</param>
        /// <returns></returns>
        public static object PageParameter( string input, string parm )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            var parmReturn = page.PageParameter( parm );

            if ( parmReturn == null )
            {
                return null;
            }

            if (parmReturn.AsIntegerOrNull().HasValue )
            {
                return parmReturn.AsIntegerOrNull();
            }

            return parmReturn;
        }

        /// <summary>
        /// Converts a lava property to a key value pair
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Dictionary<string, object> PropertyToKeyValue( object input )
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var type = input.GetType();

            if ( type == typeof( KeyValuePair<string, object> ) )
            {
                var key = type.GetProperty( "Key" );
                var value = type.GetProperty( "Value" );

                result.Add( "Key", key.GetValue( input, null ).ToString() );
                result.Add( "Value", value.GetValue( input, null ) );
            }

            return result;
        }

        /// <summary>
        /// Casts the input as a boolean value.
        /// </summary>
        /// <param name="input">The input value to be parsed into boolean form.</param>
        /// <returns>A boolean value or null if the cast could not be performed.</returns>
        public static bool? AsBoolean( object input )
        {
            if ( input == null )
            {
                return null;
            }

            return input.ToString().AsBooleanOrNull();
        }

        /// <summary>
        /// Casts the input as an integer value.
        /// </summary>
        /// <param name="input">The input value to be parsed into integer form.</param>
        /// <returns>An integer value or null if the cast could not be performed.</returns>
        public static int? AsInteger( object input )
        {
            if ( input == null )
            {
                return null;
            }
            return (int?)input.ToString().AsDecimalOrNull();
        }

        /// <summary>
        /// Casts the input as a decimal value.
        /// </summary>
        /// <param name="input">The input value to be parsed into decimal form.</param>
        /// <returns>A decimal value or null if the cast could not be performed.</returns>
        public static decimal? AsDecimal( object input )
        {
            if ( input == null )
            {
                return null;
            }

            return input.ToString().AsDecimalOrNull();
        }

        /// <summary>
        /// Casts the input as a double value.
        /// </summary>
        /// <param name="input">The input value to be parsed into double form.</param>
        /// <returns>A double value or null if the cast could not be performed.</returns>
        public static double? AsDouble( object input )
        {
            if ( input == null )
            {
                return null;
            }

            return input.ToString().AsDoubleOrNull();
        }

        /// <summary>
        /// Casts the input as a string value.
        /// </summary>
        /// <param name="input">The input value to be parsed into string form.</param>
        /// <returns>A string value or null if the cast could not be performed.</returns>
        public static string AsString( object input )
        {
            if ( input == null )
            {
                return null;
            }

            return input.ToString();
        }

        /// <summary>
        /// Casts the input as a DateTime value.
        /// </summary>
        /// <param name="input">The input value to be parsed into DateTime form.</param>
        /// <returns>A DateTime value or null if the cast could not be performed.</returns>
        public static DateTime? AsDateTime( object input )
        {
            if ( input == null )
            {
                return null;
            }

            return input.ToString().AsDateTime();
        }


        /// <summary>
        /// Creates the short link.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="token">The token.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="randomLength">The random length.</param>
        /// <returns></returns>
        public static string CreateShortLink( object input, string token = "", int? siteId = null, bool overwrite = false, int randomLength = 7 )
        {
            // Notes: This filter attempts to return a valid shortlink at all costs
            //        this means that if the configuration passed to it is invalid
            //        it will try to correct it with reasonable defaults. For instance
            //        if you pass in an invalid siteId, the first active site will be used.

            var rockContext = new RockContext();
            var shortLinkService = new PageShortLinkService( rockContext );

            // Ensure that the provided site exists
            if ( siteId.HasValue )
            {
                SiteCache site = SiteCache.Get( siteId.Value );
                if ( site == null )
                {
                    siteId = null;
                }
            }

            // If an invalid site was given use the first site enabled for shortlinks
            if ( !siteId.HasValue )
            {
                siteId = new SiteService( rockContext ).Queryable()
                    .AsNoTracking()
                    .OrderBy( s => s.EnabledForShortening )
                    .Take( 1 )
                    .Select( s => s.Id ).FirstOrDefault();
            }

            // still no site, guess we're dead in the water
            if ( !siteId.HasValue )
            {
                return string.Empty;
            }

            // Get the token
            if ( token.IsNullOrWhiteSpace() )
            {
                // No token, no problem we'll just make one ourselves
                token = shortLinkService.GetUniqueToken( siteId.Value, randomLength );
            }
            else
            {
                token = token.RemoveSpaces().UrlEncode();
            }

            // Check if the token exists by getting it
            var shortLink = shortLinkService.GetByToken( token, siteId.Value );
            if ( shortLink != null && overwrite == false )
            {
                // We can't use the provided shortlink because it's ready used, so get a random token
                // Garbage in Random out
                shortLink = null;
                token = shortLinkService.GetUniqueToken( siteId.Value, 7 );
            }

            if (shortLink == null )
            {
                shortLink = new PageShortLink();
                shortLinkService.Add( shortLink );
            }

            shortLink.Token = token;
            shortLink.SiteId = siteId.Value;
            shortLink.Url = input.ToString();
            rockContext.SaveChanges();

            return shortLink.ShortLinkUrl;
        }
        #endregion

        #region Array Filters

        /// <summary>
        /// Rearranges an array in a random order
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object Shuffle( object input )
        {
            if ( input == null )
            {
                return input;
            }

            if ( !( input is IList ) )
            {
                return input;
            }

            var inputList = input as IList;
            int n = inputList.Count;
            while ( n > 1 )
            {
                n--;
                int k = _randomNumberGenerator.Next( n + 1 );
                var value = inputList[k];
                inputList[k] = inputList[n];
                inputList[n] = value;
            }

            return inputList;
        }

        /// <summary>
        /// Determines whether [contains] [the specified input].
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="containValue">The contain value.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified input]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains( object input, object containValue)
        {
            var inputList = ( input as IList );
            if ( inputList != null )
            {
                return inputList.Contains( containValue );
            }

            return false;
        }

        /// <summary>
        /// Wheres the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterKey">The filter key.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <returns></returns>
        public static object Where( object input, string filterKey, object filterValue )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( (IEnumerable)input ) )
                {
                    if ( value is ILiquidizable )
                    {
                        var liquidObject = value as ILiquidizable;
                        var condition = DotLiquid.Condition.Operators["=="];

                        if ( liquidObject.ContainsKey( filterKey ) && condition( liquidObject[filterKey], filterValue ) )
                        {
                            result.Add( liquidObject );
                        }
                    }
                    else if (value is IDictionary<string, object>)
                    {
                        var dictionaryObject = value as IDictionary<string, object>;
                        if ( dictionaryObject.ContainsKey( filterKey ) && (dynamic) dictionaryObject[filterKey] == (dynamic) filterValue )
                        {
                            result.Add( dictionaryObject );
                        }
                    }
                }

                return result;
            }

            return input;
        }

        /// <summary>
        /// Selects the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="selectKey">The select key.</param>
        /// <returns></returns>
        public static object Select( object input, string selectKey )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( (IEnumerable)input ) )
                {
                    if ( value is ILiquidizable )
                    {
                        var liquidObject = value as ILiquidizable;
                        if ( liquidObject.ContainsKey( selectKey ) )
                        {
                            result.Add( liquidObject[selectKey] );
                        }
                    }
                    else if ( value is IDictionary<string, object> )
                    {
                        var dictionaryObject = value as IDictionary<string, object>;
                        if ( dictionaryObject.ContainsKey( selectKey ) )
                        {
                            result.Add( dictionaryObject[selectKey] );
                        }
                    }
                }

                return result;
            }

            return input;
        }

        /// <summary>
        /// Sorts the list of items by the specified attribute's value
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        public static object SortByAttribute( DotLiquid.Context context, object input, string attributeKey )
        {
            return SortByAttribute( context, input, attributeKey, "asc" );
        }

        /// <summary>
        /// Sorts the list of items by the specified attribute's value
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="sortOrder">asc or desc for sort order.</param>
        /// <returns></returns>
        public static object SortByAttribute( DotLiquid.Context context, object input, string attributeKey, string sortOrder )
        {
            if ( input is IEnumerable )
            {
                var rockContext = GetRockContext( context );
                var inputList = ( input as IEnumerable ).OfType<Attribute.IHasAttributes>().ToList();
                foreach ( var item in inputList )
                {
                    if ( item.Attributes == null )
                    {
                        item.LoadAttributes( rockContext );
                    }
                }

                if ( inputList.Count > 1 && inputList[0].Attributes.ContainsKey( attributeKey ) )
                {
                    var attributeCache = inputList[0].Attributes[attributeKey];

                    inputList.Sort( ( item1, item2 ) =>
                    {
                        var item1AttributeValue = item1.AttributeValues.Where( a => a.Key == attributeKey ).FirstOrDefault().Value.SortValue;
                        var item2AttributeValue = item2.AttributeValues.Where( a => a.Key == attributeKey ).FirstOrDefault().Value.SortValue;
                        if ( item1AttributeValue is IComparable && item2AttributeValue is IComparable )
                        {
                            if (sortOrder.ToLower() == "desc")
                            {
                                return ( item2AttributeValue as IComparable ).CompareTo( item1AttributeValue as IComparable );
                            }
                            else
                            {
                                return ( item1AttributeValue as IComparable ).CompareTo( item2AttributeValue as IComparable );
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    } );
                }

                return inputList;
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Extracts a single item from an array.
        /// </summary>
        /// <param name="input">The input object to extract one element from.</param>
        /// <param name="index">The index number of the object to extract.</param>
        /// <returns>The single object from the array or null if not found.</returns>
        public static object Index( object input, object index )
        {
            if ( input == null || index == null )
            {
                return input;
            }

            if ( !( input is IList ) )
            {
                return input;
            }

            var inputList = input as IList;
            var indexInt = index.ToString().AsIntegerOrNull();
            if ( !indexInt.HasValue || indexInt.Value < 0 || indexInt.Value >= inputList.Count )
            {
                return null;
            }

            return inputList[indexInt.Value];
        }

        /// <summary>
        /// Takes a collection and returns distinct values in that collection.
        /// </summary>
        /// <param name="input">A collection of objects.</param>
        /// <returns>A collection of objects with no repeating elements.</returns>
        /// <example>
        ///     {{ '["Banana","Orange","Banana","Apple"]' | FromJSON | Uniq | Join:',' }}
        /// </example>
        public static object Uniq( object input )
        {
            IEnumerable e = input as IEnumerable;

            if ( e == null )
            {
                return input;
            }

            return e.Distinct().Cast<object>().ToList();
        }

        #endregion

        #region Object Filters

        /// <summary>
        /// Gets the Notes of the entity
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="noteType">The noteType.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static List<Note> Notes( DotLiquid.Context context, object input, object noteType, string sortOrder = "desc", int? count = null )
        {
            int? entityId = null;

            if ( input is int )
            {
                entityId = Convert.ToInt32( input );
            }
            if ( input is IEntity )
            {
                IEntity entity = input as IEntity;
                entityId = entity.Id;
            }
            if ( !entityId.HasValue )
            {
                return null;
            }

            List<int> noteTypeIds = new List<int>();

            if ( noteType is int )
            {
                noteTypeIds.Add( (int)noteType );
            }

            if ( noteType is string )
            {
                noteTypeIds = ((string)noteType).Split( ',' ).Select( Int32.Parse ).ToList();
            }

            var notes = new NoteService( new RockContext() ).Queryable().AsNoTracking().Where( n => n.EntityId == entityId );

            if ( noteTypeIds.Count > 0 )
            {
                notes = notes.Where( n => noteTypeIds.Contains( n.NoteTypeId ) );
            }
            else
            {
                return null;
            }

            // add sort order
            if(sortOrder == "desc" )
            {
                notes = notes.OrderByDescending( n => n.CreatedDateTime );
            }
            else
            {
                notes = notes.OrderBy( n => n.CreatedDateTime );
            }

            var filterNotes = new List<Note>();
            foreach ( var note in notes )
            {
                if ( note.IsAuthorized( Authorization.VIEW, GetCurrentPerson( context ) ) )
                {
                    filterNotes.Add( note );
                }
            }

            if ( !count.HasValue )
            {
                return filterNotes;
            }
            else
            {
                return filterNotes.Take( count.Value ).ToList();
            }
        }

        /// <summary>
        /// Determines whether [has rights to] [the specified context].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Could not determine type for the input provided. Consider passing it in (e.g. 'Rock.Model.Person')</exception>
        public static bool HasRightsTo( DotLiquid.Context context, object input, string verb, string typeName = "" )
        {
            if (string.IsNullOrWhiteSpace( verb ) )
            {
                throw new Exception( "Could not determine the verb to check against (e.g. 'View', 'Edit'...)" );
            }

            if ( input == null )
            {
                return false;
            }
            else
            {
                // if the input is a model call IsAuthorized and get out of here
                if ( input is ISecured )
                {
                    var model = (ISecured)input;
                    return model.IsAuthorized( verb, GetCurrentPerson( context ) );
                }

                var type = input.GetType();
                if ( type.IsDynamicProxyType() )
                {
                    type = type.BaseType;
                }

                // not so easy then...
                if ( string.IsNullOrWhiteSpace( typeName ) )
                {
                    // attempt to read it from the input object
                    var propertyInfo = type.GetProperty( "TypeName" );
                    if ( propertyInfo != null )
                    {
                        typeName = propertyInfo.GetValue( input, null ).ToString();
                    }

                    if ( string.IsNullOrWhiteSpace( typeName ) )
                    {
                        throw new Exception( "Could not determine type for the input provided. Consider passing it in (e.g. 'Rock.Model.Person')" );
                    }
                }

                int? id = null;

                if ( type == typeof( int ) )
                {
                    id = (int)input;
                }
                else if ( type == typeof( string ) )
                {
                    id = input.ToString().AsIntegerOrNull();
                }
                else
                {
                    // check if it has an id property
                    var propertyInfo = type.GetProperty( "Id" );

                    if ( propertyInfo != null )
                    {
                        id = (int)propertyInfo.GetValue( input, null );
                    }
                }

                if ( id.HasValue )
                {
                    var entityTypes = EntityTypeCache.All();
                    var entityTypeCache = entityTypes.Where( e => String.Equals( e.Name, typeName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();

                    if ( entityTypeCache != null )
                    {
                        RockContext _rockContext = new RockContext();

                        Type entityType = entityTypeCache.GetEntityType();
                        if ( entityType != null )
                        {
                            Type[] modelType = { entityType };
                            Type genericServiceType = typeof( Rock.Data.Service<> );
                            Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                            Rock.Data.IService serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { _rockContext } ) as IService;

                            MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );

                            if ( getMethod != null )
                            {
                                var model = getMethod.Invoke( serviceInstance, new object[] { id.Value } ) as ISecured;

                                if ( model != null )
                                {
                                    return model.IsAuthorized( verb, GetCurrentPerson( context ) );
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception( "Could not determine the id of the entity." );
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( DotLiquid.Context context )
        {
            Person currentPerson = null;

            // First check for a person override value included in lava context
            if ( context.Scopes != null )
            {
                foreach ( var scopeHash in context.Scopes )
                {
                    if ( scopeHash.ContainsKey( "CurrentPerson" ) )
                    {
                        currentPerson = scopeHash["CurrentPerson"] as Person;
                    }
                }
            }

            if ( currentPerson == null )
            {
                var httpContext = System.Web.HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }

        /// <summary>
        /// Base64 encodes a binary file
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="resizeSettings">The resize settings.</param>
        /// <returns></returns>
        public static string Base64Encode( DotLiquid.Context context, object input, string resizeSettings )
        {
            BinaryFile binaryFile = null;

            if ( input is int )
            {
                binaryFile = new BinaryFileService( new RockContext() ).Get( (int)input );
            }
            else if ( input is BinaryFile )
            {
                binaryFile = (BinaryFile)input;
            }

            if ( binaryFile != null )
            {
                using ( var stream = GetResized( resizeSettings, binaryFile.ContentStream ) )
                {
                    byte[] imageBytes = stream.ReadBytesToEnd();
                    return Convert.ToBase64String( imageBytes );
                }
            }

            return string.Empty;
        }

        private static Stream GetResized( string resizeSettings, Stream fileContent )
        {
            try
            {
                if ( resizeSettings.IsNullOrWhiteSpace() )
                {
                    return fileContent;
                }

                ResizeSettings settings = new ResizeSettings( HttpUtility.ParseQueryString( resizeSettings ) );
                MemoryStream resizedStream = new MemoryStream();

                ImageBuilder.Current.Build( fileContent, resizedStream, settings );
                return resizedStream;
            }
            catch
            {
                // if resize failed, just return original content
                return fileContent;
            }
        }

        #endregion

        #region Color Filters

        /// <summary>
        /// Lightens the color by the specified percentage amount.
        /// </summary>
        /// <param name="input">The input color.</param>
        /// <param name="amount">The percentage to change.</param>
        /// <returns></returns>
        public static string Lighten( string input, string amount )
        {
            var color = new RockColor( input );
            color.Lighten( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Darkens the color by the provided percentage amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Darken( string input, string amount )
        {
            var color = new RockColor( input );
            color.Darken( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Saturates the color by the provided percentage amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Saturate( string input, string amount )
        {
            var color = new RockColor( input );
            color.Saturate( CleanColorAmount( amount ) );

            // return the color in a format that matched the input
            if ( input.StartsWith( "#" ) )
            {
                return color.ToHex();
            }

            return color.ToRGBA();
        }

        /// <summary>
        /// Desaturates the color by the provided percentage amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Desaturate( string input, string amount )
        {
            var color = new RockColor( input );
            color.Desaturate( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Decreases the opacity level by the given percentage. This makes the color less transparent (opaque).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string FadeIn( string input, string amount )
        {
            var color = new RockColor( input );
            color.FadeIn( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Increases the opacity level by the given percentage. This makes the color more transparent.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string FadeOut( string input, string amount )
        {
            var color = new RockColor( input );
            color.FadeOut( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Adjusts the hue by the specificed percentage (10%) or degrees (10deg).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string AdjustHue( string input, string amount )
        {
            amount = amount.Trim();

            // Adjust by percent
            if ( amount.EndsWith( "%" ) )
            {
                var color = new RockColor( input );
                color.AdjustHueByPercent( CleanColorAmount( amount ) );

                return GetColorString( color, input );
            }

            // Adjust by degrees
            if( amount.EndsWith( "deg" ) )
            {
                var color = new RockColor( input );
                color.AdjustHueByDegrees( CleanColorAmount( amount, "deg" ) );

                return GetColorString( color, input );
            }

            // They didn't provide a valid amount so give back the original color
            return input;
            
        }

        /// <summary>
        /// Tints (adds white) the specified color by the specified amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Tint( string input, string amount )
        {
            var color = new RockColor( input );
            color.Tint( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Shades (adds black) the specified color by the specified amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Shade( string input, string amount )
        {
            var color = new RockColor( input );
            color.Shade( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Mixes the specified color with the input color with the given amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="mixColorInput">The mix color input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Mix( string input, string mixColorInput, string amount )
        {
            var color = new RockColor( input );
            var mixColor = new RockColor( mixColorInput );

            color.Mix( mixColor, CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Returns the color in greyscale.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Grayscale( string input )
        {
            var color = new RockColor( input );
            color.Grayscale();

            return GetColorString( color, input );
        }

        /// <summary>
        /// Returns the amount string as a proper int for the color functions.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        private static int CleanColorAmount( string amount, string unit = "%" )
        {
            amount = amount.Replace( unit, "" ).Trim();

            var amountAsInt = amount.AsIntegerOrNull();

            if ( !amountAsInt.HasValue )
            {
                return 0;
            }

            return amountAsInt.Value;
        }

        /// <summary>
        /// Determines the proper return value of the color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string GetColorString( RockColor color, string input )
        {
            if (color.Alpha != 1 )
            {
                return color.ToRGBA();
            }

            if ( input.StartsWith( "#" ) )
            {
                return color.ToHex();
            }

            return color.ToRGBA();
        }

        #endregion

        #region POCOs
        /// <summary>
        /// POCO to translate an HTTP cookie in to a Liquidizable form
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class HttpCookieDrop : Drop
        {
            private readonly HttpCookie _cookie;

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name
            {
                get
                {
                    return _cookie.Name;
                }
            }

            /// <summary>
            /// Gets the path.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            public string Path
            {
                get
                {
                    return _cookie.Path;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="HttpCookieDrop"/> is secure.
            /// </summary>
            /// <value>
            ///   <c>true</c> if secure; otherwise, <c>false</c>.
            /// </value>
            public bool Secure
            {
                get
                {
                    return _cookie.Secure;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="HttpCookieDrop"/> is shareable.
            /// </summary>
            /// <value>
            ///   <c>true</c> if shareable; otherwise, <c>false</c>.
            /// </value>
            public bool Shareable
            {
                get
                {
                    return _cookie.Shareable;
                }
            }

            /// <summary>
            /// Gets the domain.
            /// </summary>
            /// <value>
            /// The domain.
            /// </value>
            public string Domain
            {
                get
                {
                    return _cookie.Domain;
                }
            }

            /// <summary>
            /// Gets the expire date/time.
            /// </summary>
            /// <value>
            /// The expires.
            /// </value>
            public DateTime Expires
            {
                get
                {
                    return _cookie.Expires;
                }
            }

            /// <summary>
            /// Gets the cookie's value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value
            {
                get
                {
                    return _cookie.Value;
                }
            }

            /// <summary>
            /// Gets the cookie's values.
            /// </summary>
            /// <value>
            /// The values.
            /// </value>
            public NameValueCollection Values
            {
                get
                {
                    return _cookie.Values;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HttpCookieDrop"/> class.
            /// </summary>
            /// <param name="cookie">The cookie.</param>
            public HttpCookieDrop( HttpCookie cookie )
            {
                _cookie = cookie;
            }
        }
        #endregion
    }
}


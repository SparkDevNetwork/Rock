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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Humanizer;
using Humanizer.Localisation;
using Ical.Net;
using ImageResizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Lava.Helpers;
using Rock.Logging;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Utilities;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using UAParser;

namespace Rock.Lava
{
    /// <summary>
    /// Defines filter methods available for use with the Lava library.
    /// </summary>
    /// <remarks>
    /// This class is marked for internal use because it should only be used in the context of resolving a Lava template.
    /// Filters should only be defined in this class if they are specific to the Rock web application, as these definitions
    /// override any implementation of the same name defined in the TemplateFilters class.
    /// Filters that are confirmed as suitable for use with both the Rock Web and Rock Mobile applications should be
    /// implemented in the TemplateFilters class.
    /// </remarks>
    internal static partial class LavaFilters
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
        /// <param name="appendOrder">The append order.</param>
        /// <returns></returns>
        public static string WithFallback( object input, string successText, string fallbackText, string appendOrder = "prepend" )
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

            if ( inputString.Length <= length )
            {
                return inputString;
            }

            return inputString.Right( length );
        }

        /// <summary>
        /// Reads the time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="wordPerMinute">The word per minute.</param>
        /// <param name="secondsPerImage">The seconds per image.</param>
        /// <returns></returns>
        public static string ReadTime( object input, int wordPerMinute = 275, int secondsPerImage = 12 )
        {

            if ( input == null )
            {
                return string.Empty;
            }

            var numOfImages = 0;
            var inputString = input.ToString();

            // Count images before we strip HTML
            if ( secondsPerImage > 0 )
            {
                numOfImages = Regex.Matches( inputString, "<img" ).Count;
            }

            inputString = inputString.StripHtml();

            var wordsPerSecond = wordPerMinute / 60;
            var wordsInString = inputString.WordCount();

            // Verify that the reading speed is set to a reasonable minimum.
            if ( wordsPerSecond < 1 )
            {
                wordsPerSecond = 1;
            }

            var readTimeInSeconds = wordsInString / wordsPerSecond;

            // Adjust the read time for images. We will start with the provided seconds per image (default 12) and subject a second for each additional image until we reach 10 then every image is 3 seconds.
            // https://blog.medium.com/read-time-and-you-bc2048ab620c
            var adjustedSecondsPerImage = secondsPerImage;

            for ( int i = 0; i < numOfImages; i++ )
            {
                if ( i < ( secondsPerImage - 2 ) && ( secondsPerImage - 2 ) > 0 )
                {
                    readTimeInSeconds = readTimeInSeconds + ( secondsPerImage - i );
                }
                else
                {
                    readTimeInSeconds = readTimeInSeconds + 3;
                }
            }

            // Format the results
            // 1 hr 23 mins
            // 23 mins
            // 30 secs
            TimeSpan readTime = TimeSpan.FromSeconds( readTimeInSeconds );

            if ( readTimeInSeconds > 3600 )
            {
                // Display in hrs
                if ( readTime.Minutes == 0 )
                {
                    return $"{"hr".ToQuantity( readTime.Hours )}";
                }
                else
                {
                    return $"{"hr".ToQuantity( readTime.Hours )} {"min".ToQuantity( readTime.Minutes )}";
                }
            }
            else if ( readTimeInSeconds > 60 )
            {
                // Display in mins

                var remainderSeconds = readTimeInSeconds - ( readTime.Minutes * 60 );

                if ( remainderSeconds > 30 )
                {
                    return $"{"min".ToQuantity( readTime.Minutes + 1 )}";
                }
                else
                {
                    return $"{"min".ToQuantity( readTime.Minutes )}";
                }
            }
            else
            {
                // Display in seconds
                return $"{"sec".ToQuantity( readTime.Seconds )}";
            }
        }

        /// <summary>
        /// Returns a MD5 hash of the string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Md5( string input )
        {
            return input.Md5Hash();
        }

        /// <summary>
        /// Returns a SHA1 hash of the string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Sha1( string input )
        {
            return input.Sha1Hash();
        }

        /// <summary>
        /// Returns a SHA254 hash of the string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Sha256( string input )
        {
            return input.Sha256Hash();
        }

        /// <summary>
        /// Returns a hash message authentication code using SHA1
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string HmacSha1( string input, string key )
        {
            return input.HmacSha1Hash( key );
        }

        /// <summary>
        /// Returns a hash message authentication code using SHA256
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string HmacSha256( string input, string key )
        {
            return input.HmacSha256Hash( key );
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
                : input.ApplyCase( LetterCasing.Title );
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
        /// Converts an input string to a well-formatted cascading style sheet (CSS) reference.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCssClass( string input )
        {
            // Ignore empty input.
            if ( string.IsNullOrWhiteSpace( input ) )
            {
                return string.Empty;
            }

            // list from: https://mathiasbynens.be/notes/css-escapes
            Regex ex = new Regex( @"[&*!""#$%'()+,.\/:;<=>?@\[\]\^`{\|}~\s]" );

            // replace unsupported characters
            input = ex.Replace( input, "-" ).ToLower();

            // remove duplicate instances of dashes (cleanliness is next to... well... it's good)
            input = Regex.Replace( input, "-+", "-" );

            // ensure the class name is valid (starts with a letter or - or _ and is at least 2 characters
            // if not add a x- to correct it and note that it is non-standard

            ex = new Regex( "-?[_a-zA-Z]+[_a-zA-Z0-9-]*" );
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
        public static string NumberToOrdinal( object input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            int number;

            if ( int.TryParse( input.ToString(), out number ) )
            {
                return number.Ordinalize();
            }
            else
            {
                return input.ToString();
            }
        }

        /// <summary>
        /// takes 1,2 and returns one, two
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToWords( object input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            int number;

            if ( int.TryParse( input.ToString(), out number ) )
            {
                return number.ToWords();
            }
            else
            {
                return input.ToString();
            }
        }

        /// <summary>
        /// takes 1,2 and returns first, second
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinalWords( object input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            int number;

            if ( int.TryParse( input.ToString(), out number ) )
            {
                return number.ToOrdinalWords();
            }
            else
            {
                return input.ToString();
            }
        }

        /// <summary>
        /// takes 1,2 and returns I, II, IV
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToRomanNumerals( object input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            int number;

            if ( int.TryParse( input.ToString(), out number ) )
            {
                return number.ToRoman();
            }
            else
            {
                return input.ToString();
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
            if ( input == null )
            {
                return string.Empty;
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

            return input.ToString().ToQuantity( numericQuantity );
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
            if ( place >= 0 )
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

            if ( @string == null )
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

        #region Regular Expressions

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

        #endregion

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

        #endregion String Filters

        #region DateTime Filters

        /// <summary>
        /// Returns a date range from the format of the sliding date range control.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static LavaDataObject DateRangeFromSlidingFormat( string input )
        {
            if ( input.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( input );

            var lavaDateRange = new LavaDataObject();
            lavaDateRange["StartDate"] = dateRange.Start;
            lavaDateRange["EndDate"] = dateRange.End;

            return lavaDateRange;
        }

        /// <summary>
        /// Sundays the date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string SundayDate( object input )
        {
            var startDto = GetDateTimeOffsetFromInputParameter( input, null );
            if ( startDto == null )
            {
                return null;
            }

            var rockStartDate = LavaDateTime.ConvertToRockDateTime( startDto.Value );
            var nextSundayDate = RockDateTime.GetSundayDate( rockStartDate );
            var output = nextSundayDate.ToShortDateString();

            return output;
        }

        /// <summary>
        /// Returns the occurrence Dates from an iCal string or list, expressed in UTC.
        /// </summary>
        /// <param name="input">The input is either an iCal string or a list of iCal strings.</param>
        /// <param name="option">The quantity option (either an integer or "all").</param>
        /// <param name="endDateTimeOption">The 'enddatetime' option if supplied will return the ending datetime of the occurrence; otherwise the start datetime is returned.</param>
        /// <param name="startDateTime">An optional date/time value that represents the start of the occurrence period.</param>
        /// <returns>A collection of DateTime values representing the next occurrence dates, expressed in UTC.</returns>
        public static List<DateTimeOffset> DatesFromICal( object input, object option = null, object endDateTimeOption = null, object startDateTime = null )
        {
            // if no option was specified, default to returning just 1 (to preserve previous behavior)
            option = option ?? 1;

            int returnCount = 1;
            if ( option.GetType() == typeof( int ) )
            {
                returnCount = ( int ) option;
            }
            else if ( option.GetType() == typeof( string ) )
            {
                // if a string of "all" is specified for the option, return all of the dates
                if ( string.Equals( ( string ) option, "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    returnCount = int.MaxValue;
                }
            }

            bool useEndDateTime = ( endDateTimeOption is string && ( string ) endDateTimeOption == "enddatetime" );

            var startDto = GetDateTimeOffsetFromInputParameter( startDateTime, null );

            var nextOccurrences = new List<DateTimeOffset>();

            if ( input is string )
            {
                nextOccurrences = GetOccurrenceDates( ( string ) input, returnCount, useEndDateTime, startDto );
            }
            else if ( input is IList )
            {
                foreach ( var item in input as IList )
                {
                    if ( item is string )
                    {
                        nextOccurrences.AddRange( GetOccurrenceDates( ( string ) item, returnCount, useEndDateTime, startDto ) );
                    }
                }
            }

            nextOccurrences.Sort( ( a, b ) => a.CompareTo( b ) );

            nextOccurrences = nextOccurrences.Take( returnCount ).ToList();

            return nextOccurrences;
        }

        /// <summary>
        /// Gets the occurrence dates from an iCalendar string, calculated in Rock time and expressed in UTC.
        /// </summary>
        /// <param name="iCalString">The iCal string.</param>
        /// <param name="returnCount">The return count.</param>
        /// <param name="useEndDateTime">if set to <c>true</c> uses the EndTime in the returned dates; otherwise it uses the StartTime.</param>
        /// <param name="startDateTime">An optional date/time value that specifies the start of the occurrence period.</param>
        /// <returns>A collection of DateTime values representing the next occurrence dates, expressed in UTC.</returns>
        private static List<DateTimeOffset> GetOccurrenceDates( string iCalString, int returnCount, bool useEndDateTime = false, DateTimeOffset? startDateTime = null )
        {
            // Convert the start and end dates to the Rock timezone.
            if ( startDateTime == null )
            {
                startDateTime = LavaDateTime.NowOffset;
            }
            else
            {
                startDateTime = LavaDateTime.ConvertToRockOffset( startDateTime.Value );
            }

            var endDate = startDateTime.Value.AddYears( 1 );

            // Load the calendar definition.
            // The calendar has no specified timezone, so dates and times are interpreted for the current Rock timezone.
            var calendar = CalendarCollection.Load( new StringReader( iCalString ) ).First();
            var calendarEvent = calendar.Events[0];

            // Get the UTC offset of the start date, expressed in the Rock timezone.
            // We apply this to the list of occurrence dates to ensure that the scheduled event time remains the same
            // even if the sequence of dates crosses a Daylight Saving Time (DST) boundary.
            List<DateTimeOffset> dates;

            var tsOffset = startDateTime.Value.Offset;
            if ( !useEndDateTime && calendarEvent.DtStart != null )
            {
                // The GetOccurrences() method returns a list of dates, to which we add the offset
                // for the Rock timezone.
                dates = calendar.GetOccurrences( startDateTime.Value.DateTime, endDate.DateTime )
                    .Take( returnCount )
                    .Select( d => new DateTimeOffset( d.Period.StartTime.Ticks, tsOffset ) )
                    .ToList();
            }
            else if ( useEndDateTime && calendarEvent.DtEnd != null )
            {
                dates = calendar.GetOccurrences( startDateTime.Value.DateTime, endDate.DateTime )
                    .Take( returnCount )
                    .Select( d => new DateTimeOffset( d.Period.EndTime.Ticks, tsOffset ) )
                    .ToList();
            }
            else
            {
                dates = new List<DateTimeOffset>();
            }

            return dates;
        }

        /// <summary>
        /// Adds a time interval to a date
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static DateTimeOffset? DateAdd( object input, object amount, string interval = "d" )
        {
            var date = GetDateTimeOffsetFromInputParameter( input, null );

            if ( date.HasValue )
            {
                var integerAmount = amount.ToStringSafe().AsInteger();

                switch ( interval )
                {
                    case "y":
                        date = date.Value.AddYears( integerAmount );
                        break;
                    case "M":
                        date = date.Value.AddMonths( integerAmount );
                        break;
                    case "w":
                        date = date.Value.AddDays( integerAmount * 7 );
                        break;
                    case "d":
                        date = date.Value.AddDays( integerAmount );
                        break;
                    case "h":
                        date = date.Value.AddHours( integerAmount );
                        break;
                    case "m":
                        date = date.Value.AddMinutes( integerAmount );
                        break;
                    case "s":
                        date = date.Value.AddSeconds( integerAmount );
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
            // We are calculating a difference in calendar days, so we must compare days from the Rock timezone calendar.
            var dtInput = GetDateTimeOffsetFromInputParameter( input, null );

            if ( dtInput == null )
            {
                return string.Empty;
            }

            var dtCompare = GetDateTimeOffsetFromInputParameter( compareDate, LavaDateTime.NowOffset );

            var humanized = LavaDateTime.ConvertToRockDateTime( dtInput.Value )
                .Humanize( true, LavaDateTime.ConvertToRockDateTime( dtCompare.Value ) );

            return humanized;
        }

        /// <summary>
        /// Returns a human-readable description of the number of days between the specified date and the current date.
        /// The difference is calculated for the Rock timezone, and the specified date will be converted to the Rock timezone if necessary.
        /// </summary>
        /// <param name="input">The date from which the difference to the current date will be measured.</param>
        /// <returns></returns>
        public static string DaysFromNow( object input )
        {
            // We are calculating a difference in calendar days, so we must compare days from the Rock timezone calendar.
            var dtInputDate = GetDateTimeOffsetFromInputParameter( input, null );

            if ( dtInputDate == null )
            {
                return string.Empty;
            }

            dtInputDate = LavaDateTime.ConvertToRockDateTime( dtInputDate.Value );

            var dtCompareDate = LavaDateTime.NowDateTime;

            var daysDiff = ( dtInputDate.Value.Date - dtCompareDate.Date ).Days;

            string response;

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

            if ( input.ToString().IsNotNullOrWhiteSpace() )
            {
                DateTime? date;

                if ( input.ToString().ToLower() == "now" )
                {
                    date = RockDateTime.Now;
                }
                else
                {
                    date = input.ToString().AsDateTime();
                }

                if ( date.HasValue )
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
        /// Humanizes the time span.
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="unitOrPrecision">The minimum unit.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, object unitOrPrecision = null, string direction = "min" )
        {
            if ( unitOrPrecision == null )
            {
                unitOrPrecision = "Day";
            }

            if ( unitOrPrecision.ToString().AsIntegerOrNull() != null )
            {
                return HumanizeTimeSpanWithPrecision( sStartDate, sEndDate, unitOrPrecision.ToString().AsInteger() );
            }

            var startDate = GetDateTimeOffsetFromInputParameter( sStartDate, null );
            var endDate = GetDateTimeOffsetFromInputParameter( sEndDate, null );

            TimeUnit unitValue = TimeUnit.Day;

            switch ( unitOrPrecision.ToString() )
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

            if ( startDate != null && endDate != null )
            {
                var difference = endDate.Value - startDate.Value;

                if ( difference.TotalSeconds >= 0 && difference.TotalSeconds < 1 )
                {
                    return "just now";
                }

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
        /// takes two datetimes and humanizes the difference like '1 day'. Supports 'Now' as end date
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        private static string HumanizeTimeSpanWithPrecision( object sStartDate, object sEndDate, object precision )
        {
            int precisionUnit = 1;

            if ( precision is int )
            {
                precisionUnit = ( int ) precision;
            }

            var startDate = GetDateTimeOffsetFromInputParameter( sStartDate, null );
            var endDate = GetDateTimeOffsetFromInputParameter( sEndDate, null );

            if ( startDate != null && endDate != null )
            {
                var difference = endDate.Value - startDate.Value;

                return difference.Humanize( precisionUnit );
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Converts an input value to a DateTimeOffset.
        /// </summary>
        /// <param name="input">The date.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static DateTimeOffset? GetDateTimeOffsetFromInputParameter( object input, DateTimeOffset? defaultValue )
        {
            if ( input is String inputString )
            {
                if ( inputString.Trim().ToLower() == "now" )
                {
                    return LavaDateTime.NowOffset;
                }
                else
                {
                    return LavaDateTime.ParseToOffset( inputString, defaultValue );
                }
            }
            else if ( input is DateTime dt )
            {
                return LavaDateTime.ConvertToDateTimeOffset( dt );
            }
            else if ( input is DateTimeOffset inputDateTimeOffset )
            {
                return inputDateTimeOffset;
            }

            return defaultValue;
        }

        private static int GetMonthsBetween( DateTimeOffset from, DateTimeOffset to )
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
        public static DateTimeOffset? ToMidnight( object input )
        {
            // Parse the input to a valid date.
            var inputDate = GetDateTimeOffsetFromInputParameter( input, null );

            if ( inputDate == null )
            {
                return null;
            }

            // Return a time of 12:00am on the specified date.
            var date = new DateTimeOffset( inputDate.Value.Year, inputDate.Value.Month, inputDate.Value.Day, 0, 0, 0, inputDate.Value.Offset );

            return date;
        }

        /// <summary>
        /// Advances the date to a specific day in the next 7 days.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="sDayOfWeek">The starting day of week.</param>
        /// <param name="includeCurrentDay">if set to <c>true</c> includes the current day as the current week.</param>
        /// <param name="numberOfWeeks">The number of weeks (must be non-zero).</param>
        /// <returns></returns>
        public static DateTimeOffset? NextDayOfTheWeek( object input, string sDayOfWeek, object includeCurrentDay = null, object numberOfWeeks = null )
        {
            int weeks = numberOfWeeks.ToStringSafe().AsIntegerOrNull() ?? 1;
            bool includeCurrent = includeCurrentDay.ToStringSafe().AsBoolean( false );

            DayOfWeek dayOfWeek;

            if ( input == null )
            {
                return null;
            }

            // Check for invalid number of weeks
            if ( weeks == 0 )
            {
                return null;
            }

            // Get the date value
            var date = GetDateTimeOffsetFromInputParameter( input, null );

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

            if ( includeCurrent )
            {
                daysUntilWeekDay = ( ( int ) dayOfWeek - ( int ) date.Value.DayOfWeek + 7 ) % 7;
            }
            else
            {
                daysUntilWeekDay = ( ( ( ( int ) dayOfWeek - 1 ) - ( int ) date.Value.DayOfWeek + 7 ) % 7 ) + 1;
            }

            // When a positive number of weeks is given, since the number of weeks defaults to 1
            // (which means the current week) we need to shift the numberOfWeeks down by 1 so
            // the calculation below is correct.
            if ( weeks >= 1 )
            {
                weeks--;
            }

            // Adjust the Rock time to the correct day, then return the result as UTC to avoid ambiguity.
            var rockAdjustedDate = date.Value.AddDays( daysUntilWeekDay + ( weeks * 7 ) );

            return rockAdjustedDate;
        }

        /// <summary>
        /// Days until a given date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static int? DaysUntil( object input )
        {
            var inputDate = GetDateTimeOffsetFromInputParameter( input, null );

            if ( inputDate == null )
            {
                return null;
            }

            // We want to calculate the difference in actual calendar days rather than 24-hour periods,
            // so convert the dates to Rock time before calculating the difference.
            var rockDateTime = LavaDateTime.ConvertToRockDateTime( inputDate.Value );

            var days = ( rockDateTime.Date - LavaDateTime.NowDateTime.Date ).Days;

            return days;
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

        #endregion DateTime Filters

        #region Number Filters

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

            var inputAsDecimal = input.ToString().AsDecimalOrNull();

            if ( inputAsDecimal == null
                 && input is string )
            {
                // if the input is a string, just append the currency symbol to the front, even if it can't be converted to a number
                var currencySymbol = GlobalAttributesCache.Value( "CurrencySymbol" );
                return string.Format( "{0}{1}", currencySymbol, input );
            }
            else
            {
                // if the input an integer, decimal, double or anything else that can be parsed as a decimal, format that
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
                return Enumerable.Repeat( input.ToString(), operand.ToString().AsInteger() );
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

        /// <summary>
        /// Limits a number to a maximum value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object AtMost( object input, object operand )
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
                return intInput > intOperand ? intOperand : input;
            }
            else if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput > iOperand ? iOperand : iInput;
            }
            else
            {
                return "Could not convert input to number";
            }
        }

        /// <summary>
        /// Limits a number to a minimum value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object AtLeast( object input, object operand )
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
                return intInput < intOperand ? intOperand : input;
            }
            else if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput < iOperand ? iOperand : iInput;
            }
            else
            {
                return "Could not convert input to number";
            }
        }

        /// <inheritdoc cref="Rock.Lava.Filters.TemplateFilters.RandomNumber(object)"/>
        public static int RandomNumber( object input )
        {
            return Rock.Lava.Filters.TemplateFilters.RandomNumber( input );
        }

        #endregion Number Filters

        #region Attribute Filters

        private const int _maxRecursionDepth = 10;

        /// <summary>
        /// DotLiquid Attribute Filter
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Attribute( ILavaRenderContext context, object input, string attributeKey, string qualifier = "" )
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
            if ( ( input is string ) && input.ToString().Equals( "Global", StringComparison.OrdinalIgnoreCase ) )
            {
                var globalAttributeCache = GlobalAttributesCache.Get();
                attribute = globalAttributeCache.Attributes
                    .FirstOrDefault( a => a.Key.Equals( attributeKey, StringComparison.OrdinalIgnoreCase ) );
                if ( attribute != null )
                {
                    // Get the value
                    string theValue = globalAttributeCache.GetValue( attributeKey );
                    if ( theValue.IsLavaTemplate() )
                    {
                        // Global attributes may reference other global attributes, so try to resolve this value again
                        var mergeFields = context.GetMergeFields();

                        // Verify that the recursion depth is not exceeded.
                        if ( !IncrementRecursionTracker( "internal.AttributeFilterRecursionDepth", mergeFields ) )
                        {
                            return "## Lava Error: Recursive reference ##";
                        }

                        rawValue = theValue.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        rawValue = theValue;
                    }
                }
            }

            /*
                04/28/2020 - Shaun
                The "SystemSetting" filter argument does not retrieve the Attribute from the database
                or perform any authorization checks.  It simply returns the value of the specified
                SystemSetting attribute (with any merge fields evaluated).  This is intentional.
            */

            // If Input is "SystemSetting" then look for a SystemSetting attribute with key
            else if ( ( input is string ) && input.ToString().Equals( "SystemSetting", StringComparison.OrdinalIgnoreCase ) )
            {
                string theValue = Rock.Web.SystemSettings.GetValue( attributeKey );
                if ( theValue.IsLavaTemplate() )
                {
                    // SystemSetting attributes may reference other global attributes, so try to resolve this value again
                    var mergeFields = context.GetMergeFields();

                    // Verify that the recursion depth has not been exceeded.
                    if ( !IncrementRecursionTracker( "internal.AttributeFilterRecursionDepth", mergeFields ) )
                    {
                        return "## Lava Error: Recursive reference ##";
                    }

                    rawValue = theValue.ResolveMergeFields( mergeFields );
                }
                else
                {
                    rawValue = theValue;
                }

                return rawValue;
            }

            // If input is an object that has attributes, find its attribute value
            else
            {
                if ( input is Attribute.IHasAttributes )
                {
                    item = ( Attribute.IHasAttributes ) input;
                }
                else if ( input is IHasAttributesWrapper )
                {
                    item = ( ( IHasAttributesWrapper ) input ).HasAttributesEntity;
                }
                else if ( input is RockDynamic inputDynamic )
                {
                    if ( inputDynamic.Instance != null && inputDynamic.Instance is IHasAttributes )
                    {
                        item = ( Attribute.IHasAttributes ) inputDynamic.Instance;
                    }
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

                    // Check qualifer for "TextValue" and if true return PersistedTextValue
                    if ( qualifier.Equals( "TextValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return item.AttributeValues[attributeKey].PersistedTextValue;
                    }

                    // Check qualifer for "HtmlValue" and if true return PersistedHtmlValue
                    if ( qualifier.Equals( "HtmlValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return item.AttributeValues[attributeKey].PersistedHtmlValue;
                    }

                    // Check qualifer for "CondensedTextValue" and if true return PersistedTextValue
                    if ( qualifier.Equals( "CondensedTextValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return item.AttributeValues[attributeKey].PersistedCondensedTextValue;
                    }

                    // Check qualifer for "CondensedHtmlValue" and if true return PersistedTextValue
                    if ( qualifier.Equals( "CondensedHtmlValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return item.AttributeValues[attributeKey].PersistedCondensedHtmlValue;
                    }

                    // Check qualifier for 'Url' and if present and attribute's field type is a ILinkableFieldType, then return the formatted url value
                    var field = attribute.FieldType.Field;
                    if ( qualifier.Equals( "Url", StringComparison.OrdinalIgnoreCase ) && field is Rock.Field.ILinkableFieldType )
                    {
                        return ( ( Rock.Field.ILinkableFieldType ) field ).UrlLink( rawValue, attribute.QualifierValues );
                    }

                    // check if attribute is a key value list and return a collection of key/value pairs
                    if ( field is Rock.Field.Types.KeyValueListFieldType )
                    {
                        var keyValueField = ( Rock.Field.Types.KeyValueListFieldType ) field;

                        return keyValueField.GetValuesFromString( null, rawValue, attribute.QualifierValues, false );
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
                        if ( attribute.QualifierValues != null && attribute.QualifierValues.ContainsKey( "allowmultiple" ) && attribute.QualifierValues["allowmultiple"].Value.AsBoolean() )
                        {
                            return values;
                        }
                        else
                        {
                            return values.FirstOrDefault();
                        }
                    }

                    // If qualifier was specified, and the attribute field type is an IEntityFieldType, try to find a property on the entity
                    if ( !string.IsNullOrWhiteSpace( qualifier ) && field is Rock.Field.IEntityFieldType )
                    {
                        IEntity entity = ( ( Rock.Field.IEntityFieldType ) field ).GetEntity( rawValue );
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

                    // Otherwise return the formatted value
                    return field.FormatValue( null, attribute.EntityTypeId, entityId, rawValue, attribute.QualifierValues, false );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Increment the specified recursion tracking key in the supplied Lava context
        /// and verify that the recursion limit has not been exceeded.
        /// </summary>
        /// <param name="recursionDepthKey"></param>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        private static bool IncrementRecursionTracker( string recursionDepthKey, IDictionary<string, object> mergeFields )
        {
            int currentRecursionDepth = mergeFields.GetValueOrDefault( recursionDepthKey, 0 ).ToStringSafe().AsInteger();

            currentRecursionDepth++;

            if ( currentRecursionDepth > _maxRecursionDepth )
            {
                return false;
            }

            mergeFields[recursionDepthKey] = currentRecursionDepth.ToString();

            return true;
        }

        /// <summary>
        /// Properties the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Property( ILavaRenderContext context, object input, string propertyKey, string qualifier = "" )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            var propertyNames = propertyKey.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            object propertyValue = input;
            var valueType = input.GetType();

            while ( propertyNames.Any() && propertyValue != null )
            {
                var propName = propertyNames.First();

                if ( propertyValue is IDictionary<string, object> )
                {
                    var dictionaryObject = propertyValue as IDictionary<string, object>;
                    if ( dictionaryObject.ContainsKey( propName ) )
                    {
                        propertyValue = dictionaryObject[propName];
                        if ( propertyValue != null )
                        {
                            valueType = propertyValue.GetType();
                        }
                    }
                    else
                    {
                        propertyValue = null;
                    }
                }
                else if ( propertyValue is ILavaDataDictionary dynamicObject )
                {
                    if ( dynamicObject.ContainsKey( propName ) )
                    {
                        propertyValue = dynamicObject.GetValue( propName );
                        if ( propertyValue != null )
                        {
                            valueType = propertyValue.GetType();
                        }
                    }
                    else
                    {
                        propertyValue = null;
                    }
                }
                else
                {
                    var property = valueType.GetProperty( propName );
                    if ( property != null )
                    {
                        propertyValue = property.GetValue( propertyValue );
                        valueType = property.PropertyType;
                    }
                    else
                    {
                        propertyValue = null;
                    }
                }

                propertyNames = propertyNames.Skip( 1 ).ToList();
            }

            return propertyValue;
        }

        #endregion Attribute Filters

        #region Group Filters

        /// <summary>
        /// Loads a Group record from the database from it's GUID.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Rock.Model.Group GroupByGuid( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? groupGuid = input.ToString().AsGuidOrNull();

            if ( groupGuid.HasValue )
            {
                return new GroupService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( groupGuid.Value );
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
        public static Rock.Model.Group GroupById( ILavaRenderContext context, object input )
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

            return new GroupService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( groupId );
        }

        #endregion Group Filters

        #region Misc Filters

        /// <summary>
        /// Shows details about which Merge Fields are available
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">If a merge field is specified, only Debug info on that MergeField will be shown</param>
        /// <param name="option1">either userName or outputFormat</param>
        /// <param name="option2">either userName or outputFormat</param>
        /// <returns></returns>
        public static string Debug( ILavaRenderContext context, object input, string option1 = null, string option2 = null )
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

            var mergeFields = context.GetMergeFields() as IDictionary<string, object>;

            if ( input != null
                 && mergeFields.Any( a => a.Value == input ) )
            {
                mergeFields = mergeFields.Where( a => a.Value == input ).ToDictionary( k => k.Key, v => v.Value );
            }

            // TODO: implement the outputFormat option to support ASCII
            return mergeFields.lavaDebugInfo();
        }

        /// <summary>
        /// This is an undocumented internal filter to add an object to the merge fields. This will be used by the
        /// Lava class to help formulate examples for the students to use. This will allow the class creators to
        /// wire up merge fields to be used by the debug filter.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The object to be added to the merge field.</param>
        /// <param name="key">The key to use to add the object.</param>
        public static void AddToMergeFields( ILavaRenderContext context, object input, string key )
        {
            // Make sure we have a key
            if ( key.IsNullOrWhiteSpace() )
            {
                return;
            }

            context.SetMergeField( key, input );
        }

        /// <summary>
        /// Xamls the wrap.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string XamlWrap( string input )
        {
            if ( input.IsNullOrWhiteSpace() )
            {
                return input;
            }

            return string.Format( "<![CDATA[{0}]]>", input );
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
                HttpContext.Current.Response.Redirect( input, false );

                // Having redirected to a new page, abort the rendering process for the current page.
                throw new LavaInterruptException( "Render aborted by PageRedirect filter." );
            }

            return string.Empty;
        }

        /// <summary>
        /// Resolves a relative URL to an absolute URL for the current Rock environment.
        /// </summary>
        /// <param name="context">The current Lava render context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ResolveRockUrl( ILavaRenderContext context, string input )
        {
            var url = input;

            var host = context.GetService<ILavaHost>();
            if ( host != null )
            {
                url = host.ResolveUrl( input );
            }

            return url;
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
            bool inputIsAll = false;

            // ensure they provided a cache type
            if ( input == null || cacheType.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // figure out the input type
            string inputString = input.ToString();
            inputAsInt = inputString.AsIntegerOrNull();

            if ( !inputAsInt.HasValue ) // not an int try guid
            {
                inputAsGuid = inputString.AsGuidOrNull();

                if ( !inputAsGuid.HasValue ) // not a guid try "All"
                {
                    inputIsAll = inputString.Equals( "All", StringComparison.OrdinalIgnoreCase );
                }
            }

            if ( inputAsGuid.HasValue || inputAsInt.HasValue || inputIsAll )
            {
                Type modelCacheType;

                switch ( cacheType )
                {
                    case "DefinedValue":
                        {
                            modelCacheType = typeof( DefinedValueCache );
                            break;
                        }
                    case "DefinedType":
                        {
                            modelCacheType = typeof( DefinedTypeCache );
                            break;
                        }
                    case "Campus":
                        {
                            modelCacheType = typeof( CampusCache );
                            break;
                        }
                    case "EntityType":
                        {
                            modelCacheType = typeof( EntityTypeCache );
                            break;
                        }
                    case "Category":
                        {
                            modelCacheType = typeof( CategoryCache );
                            break;
                        }
                    case "GroupType":
                        {
                            modelCacheType = typeof( GroupTypeCache );
                            break;
                        }
                    case "Page":
                        {
                            modelCacheType = typeof( PageCache );
                            break;
                        }
                    case "Block":
                        {
                            modelCacheType = typeof( BlockCache );
                            break;
                        }
                    case "BlockType":
                        {
                            modelCacheType = typeof( BlockTypeCache );
                            break;
                        }
                    case "EventCalendar":
                        {
                            modelCacheType = typeof( EventCalendarCache );
                            break;
                        }
                    case "Attribute":
                        {
                            modelCacheType = typeof( AttributeCache );
                            break;
                        }
                    case "NoteType":
                        {
                            modelCacheType = typeof( NoteTypeCache );
                            break;
                        }
                    case "ContentChannel":
                        {
                            modelCacheType = typeof( ContentChannelCache );
                            break;
                        }
                    default:
                        {
                            return $"Cache type {cacheType} not supported.";
                        }
                }

                try
                {
                    if ( inputAsInt.HasValue )
                    {
                        return modelCacheType
                            .GetMethod( "Get", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new[] { typeof( int ) }, null )
                            .Invoke( null, new object[] { inputAsInt.Value } );
                    }
                    else if ( inputAsGuid.HasValue )
                    {
                        return modelCacheType
                            .GetMethod( "Get", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new[] { typeof( Guid ) }, null )
                            .Invoke( null, new object[] { inputAsGuid.Value } );
                    }
                    else
                    {
                        return modelCacheType
                            .GetMethod( "All", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[] { }, null )
                            .Invoke( null, null );
                    }
                }
                catch ( Exception ex )
                {
                    RockLogger.Log.Error( RockLogDomains.Lava, ex, $"Unable to return object(s) from Cache (input = '{input}', cacheType = '{cacheType}')." );

                    return null;
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
            return input.ToJson( indentOutput: true, ignoreErrors: true );
        }

        /// <summary>
        /// Returns a dynamic object from a JSON string. The returned type parameter should be considered 'internal' at this point. It
        /// is not documented and could be removed if we can use the NestedDictionaryConverter as the default return type. 
        /// See https://www.rockrms.com/page/565#fromjson
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static object FromJSON( object input, string returnType = "ExpandoObject" )
        {
            switch ( returnType )
            {
                case "Dictionary":
                    {
                        var jsonSettings = new JsonSerializerSettings{
                            Converters = new List<JsonConverter> { new NestedDictionaryConverter() }
                        };

                        return JsonConvert.DeserializeObject<Dictionary<string, object>>( input.ToString(), jsonSettings );
                    }
                default:
                    {
                        return ( input as string ).FromJsonDynamicOrNull();
                    }
            }
        }

        /// <summary>
        /// Returns the dataset object from a <see cref="Rock.Model.PersistedDataset"/> specified by accessKey
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="accessKey">The access key.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static object PersistedDataset( ILavaRenderContext context, string accessKey, string options = null )
        {
            var persistedDataset = PersistedDatasetCache.GetFromAccessKey( accessKey );

            if ( persistedDataset == null )
            {
                return null;
            }

            var resultDataObject = persistedDataset.ResultDataObject;

            // NOTE This logic should not be used. Instead use the AppendFollowing filter
            // Will remove after RX2019.
            // Append following information
            // Parse filter options
            var optionsList = options?.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            var returnOnlyFollowedItems = optionsList?.Contains( "ReturnOnlyFollowedItems" ) == true;
            var returnOnlyNotFollowedItems = optionsList?.Contains( "ReturnOnlyNotFollowedItems" ) == true;
            var appendFollowing = optionsList?.Contains( "AppendFollowing" ) == true || returnOnlyFollowedItems || returnOnlyNotFollowedItems;

            if ( appendFollowing )
            {
                resultDataObject = AppendFollowing( context, resultDataObject );
            }

            return resultDataObject;
        }

        /// <summary>
        /// Appends Following information to entity/entities or a data object created from <see cref="PersistedDataset(ILavaRenderContext, string, string)" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">The data object.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns></returns>
        public static object AppendFollowing( ILavaRenderContext context, object dataObject, string purposeKey = null )
        {
            if ( dataObject == null )
            {
                return dataObject;
            }

            dynamic resultDataObject = null;

            var dataObjectType = dataObject.GetType();

            // Determine if dataset is a collection or a single object
            bool isCollection = false;
            bool isEntityCollection = false;
            int? dataObjectEntityTypeId = null;

            if ( dataObject is IEntity entity )
            {
                // The input object is a single Entity.
                resultDataObject = new RockDynamic( dataObject );
                resultDataObject.EntityTypeId = EntityTypeCache.GetId( dataObject.GetType() );
            }
            else if ( dataObject is ExpandoObject xo )
            {
                resultDataObject = ( resultDataObject as ExpandoObject )?.ShallowCopy() ?? resultDataObject;
            }
            else if ( dataObject is IEnumerable collection )
            {
                // Note: Since a single ExpandoObject actually is an IEnumerable (of fields), we'll have to see if this is an IEnumerable of ExpandoObjects
                // to see if we should treat it as a collection.
                isCollection = false;

                var enumerator = collection.GetEnumerator();

                var firstItem = enumerator.MoveNext() ? enumerator.Current : null;

                if ( firstItem == null )
                {
                    isCollection = false;
                }
                else
                {
                    isCollection = true;

                    if ( firstItem is IEntity firstEntity )
                    {
                        dataObjectEntityTypeId = EntityTypeCache.GetId( firstEntity.GetType() );

                        var dynamicEntityList = new List<RockDynamic>();

                        foreach ( var item in collection )
                        {
                            dynamic rockDynamicItem = new RockDynamic( item );
                            rockDynamicItem.EntityTypeId = EntityTypeCache.GetId( item.GetType() );
                            dynamicEntityList.Add( rockDynamicItem );
                        }

                        resultDataObject = dynamicEntityList;

                        isEntityCollection = true;
                    }
                    else if ( firstItem is ExpandoObject )
                    {
                        // If the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset.
                        var expandoCollection = collection.Cast<ExpandoObject>();

                        resultDataObject = expandoCollection.Select( a => a.ShallowCopy() ).ToList();
                    }
                    else
                    {
                        // if we are dealing with a persisted dataset, make a copy of the objects so we don't accidently modify the cached object
                        resultDataObject = ( resultDataObject as IEnumerable<ExpandoObject> ).Select( a => a.ShallowCopy() ).ToList();
                    }
                }
            }

            List<int> entityIdList;

            if ( dataObject is IEntity dataObjectAsEntity )
            {
                dataObjectEntityTypeId = EntityTypeCache.GetId( dataObject.GetType() );
                entityIdList = new List<int>();
                entityIdList.Add( dataObjectAsEntity.Id );
            }
            else if ( isEntityCollection )
            {
                var dataObjectAsEntityList = ( ( IEnumerable ) dataObject ).Cast<IEntity>();

                entityIdList = dataObjectAsEntityList.Select( a => a.Id ).ToList();
            }
            else
            {
                // if the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset 
                if ( isCollection )
                {
                    IEnumerable<dynamic> dataObjectAsCollection = dataObject as IEnumerable<dynamic>;

                    entityIdList = dataObjectAsCollection
                            .Select( x => ( int? ) x.Id )
                            .Where( e => e.HasValue )
                            .Select( e => e.Value ).ToList();

                    // the dataObjects will each have the same EntityTypeId (assuming they are from a persisted dataset, so we can determine EntityTypeId from the first one
                    dataObjectEntityTypeId = dataObjectAsCollection.Select( a => ( int? ) a.EntityTypeId ).FirstOrDefault();
                }
                else
                {
                    int? entityId = ( int? ) resultDataObject.Id;
                    dataObjectEntityTypeId = ( int? ) resultDataObject.EntityTypeId;
                    entityIdList = new List<int>();
                    if ( entityId.HasValue )
                    {
                        entityIdList.Add( entityId.Value );
                    }
                }
            }

            // if we don't know the EntityTypeId, we won't be able to figure out following, so just return the original object
            if ( !dataObjectEntityTypeId.HasValue )
            {
                return dataObject;
            }

            List<int> followedEntityIds;

            var currentPerson = GetCurrentPerson( context );

            if ( currentPerson != null )
            {
                if ( purposeKey.IsNotNullOrWhiteSpace() )
                {
                    // Get with purpose key
                    followedEntityIds = new FollowingService( LavaHelper.GetRockContextFromLavaContext( context ) ).GetFollowedItems( dataObjectEntityTypeId.Value, currentPerson.Id, purposeKey )
                        .Where( e => entityIdList.Contains( e.Id ) ).Select( a => a.Id ).ToList();
                }
                else
                {
                    // Get without purpose key
                    followedEntityIds = new FollowingService( LavaHelper.GetRockContextFromLavaContext( context ) ).GetFollowedItems( dataObjectEntityTypeId.Value, currentPerson.Id )
                        .Where( e => entityIdList.Contains( e.Id ) ).Select( a => a.Id ).ToList();
                }
            }
            else
            {
                followedEntityIds = new List<int>();
            }

            // Append new following properties if collection
            if ( isCollection )
            {
                foreach ( dynamic result in ( IEnumerable ) resultDataObject )
                {
                    int? entityId = ( int? ) result.Id;

                    if ( entityId.HasValue )
                    {
                        result.IsFollowing = followedEntityIds.Contains( entityId.Value );
                        result.FollowingEntityTypeId = dataObjectEntityTypeId;
                        result.FollowingEntityId = entityId;
                    }
                }
            }
            else
            {
                int? entityId = ( int? ) resultDataObject.Id;

                if ( entityId.HasValue )
                {
                    resultDataObject.IsFollowing = followedEntityIds.Contains( entityId.Value );
                    resultDataObject.FollowingEntityTypeId = dataObjectEntityTypeId;
                    resultDataObject.FollowingEntityId = entityId;
                }
            }

            return resultDataObject;
        }

        /// <summary>
        /// Appends watch data to various types of objects.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="startValue">The start value.</param>
        /// <returns></returns>
        public static object AppendWatches( ILavaRenderContext context, object source, string attributeKey = "", object startValue = null )
        {
            var currentPerson = GetCurrentPerson( context );
            DateTime? startDate = null;

            // Quick out if we have no data
            if ( source == null )
            {
                return source;
            }

            // Determine the start date. This can be either an integer (days since watch) or a datetime
            if ( startValue is int daysSinceWatch )
            {
                startDate = RockDateTime.Now.AddDays( daysSinceWatch * -1 );
            }
            else
            {
                startDate = startValue?.ToString().AsDateTime();
            }

            // Get a Rock Context
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            // Append the media information based on the object type

            // Entity
            if ( source is IEntity sourceAsEntity )
            {
                return LavaAppendWatchesHelper.AppendMediaForEntity( sourceAsEntity, attributeKey, startDate, currentPerson, rockContext );
            }

            // Collection of Entities
            if ( source is ICollection collection )
            {
                // Get this first item so we can determine it's type. Checking for IEnumerable<IEntity> directly does not work.
                var enumerator = collection.GetEnumerator();

                var firstItem = enumerator.MoveNext() ? enumerator.Current : null;

                if ( firstItem is MediaElement )
                {
                    return LavaAppendWatchesHelper.AppendMediaForMediaElements( collection, startDate, currentPerson, rockContext );
                }
                else if ( firstItem is IHasAttributes entityItem )
                {
                    // Make sure the collection has the provided attribute
                    if ( !entityItem.Attributes.ContainsKey( attributeKey ) )
                    {
                        return source;
                    }

                    return LavaAppendWatchesHelper.AppendMediaForEntities( collection, attributeKey, startDate, currentPerson, rockContext, ( ( IEntity ) firstItem ).TypeId );
                }
                else if ( firstItem is ExpandoObject )
                {
                    // If the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset.
                    var expandoCollection = collection.Cast<ExpandoObject>();

                    var dynamicCollection = expandoCollection.Select( a => a.ShallowCopy() ).ToList();

                    // Append watch information
                    return LavaAppendWatchesHelper.AppendMediaForExpandoCollection( dynamicCollection, startDate, currentPerson, rockContext );
                }
            }

            if ( source is Dictionary<string, object> dictionary )
            {
                // Try treating it as a dictionary 
                return LavaAppendWatchesHelper.AppendMediaForDictionary( dictionary, startDate, currentPerson, rockContext );
            }

            // ExpandoObject
            if ( source is ExpandoObject xo )
            {
                if ( LavaAppendWatchesHelper.DynamicContainsKey( xo, "MediaId" ) )
                {
                    return LavaAppendWatchesHelper.AppendMediaForExpando( xo, startDate, currentPerson, rockContext );
                }

                // If the expando didn't have the key, it could be a dictionary
                return LavaAppendWatchesHelper.AppendMediaForDictionary( xo, startDate, currentPerson, rockContext ) ;
            }
            

            return source;
        }

        /// <summary>
        /// Attempt to convert an object to an enumerable collection of the specified type if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataObject"></param>
        /// <returns>null if the supplied data object is not an enumerable collection of the specified type.</returns>
        private static IEnumerable<T> GetDataObjectAsEnumerableType<T>( object dataObject )
            where T : class
        {
            IEnumerable<T> dataObjectAsEntityList = null;

            if ( dataObject is IEnumerable entityList )
            {
                var enumerator = entityList.GetEnumerator();

                var firstEntity = enumerator.MoveNext() ? enumerator.Current as T : null;

                if ( firstEntity != null )
                {
                    dataObjectAsEntityList = ( ( IEnumerable ) dataObject ).Cast<T>();
                }
            }

            return dataObjectAsEntityList;
        }

        /// <summary>
        /// Filters results to items that are being followed by the current person. Items can be  entity/entities or a data object created from <see cref="PersistedDataset(ILavaRenderContext, string, string)"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">The data object.</param>
        /// <returns></returns>
        public static object FilterFollowed( ILavaRenderContext context, object dataObject )
        {
            return FilterFollowedOrNotFollowed( context, GetCurrentPerson( context ), dataObject, FollowFilterType.Followed );
        }

        /// <summary>
        /// Filters results to items that are not being followed by the current person. Items can be  entity/entities or a data object created from <see cref="PersistedDataset(ILavaRenderContext, string, string)"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">The data object.</param>
        /// <returns></returns>
        public static object FilterUnfollowed( ILavaRenderContext context, object dataObject )
        {
            return FilterFollowedOrNotFollowed( context, GetCurrentPerson( context ), dataObject, FollowFilterType.NotFollowed );
        }

        /// <summary>
        /// 
        /// </summary>
        private enum FollowFilterType
        {
            Followed,
            NotFollowed
        }

        /// <summary>
        /// Filters the followed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="dataObject">The data object.</param>
        /// <param name="followFilterType">Type of the follow filter.</param>
        /// <returns></returns>
        private static object FilterFollowedOrNotFollowed( ILavaRenderContext context, Person currentPerson, object dataObject, FollowFilterType followFilterType )
        {
            if ( dataObject == null )
            {
                return dataObject;
            }

            // if AppendFollowing wasn't already done on this, run it thru AppendFollowing so that all the objects are either ExpandoObject or RockDynamic
            var dataObjectAsEnumerable = GetDataObjectAsEnumerableType<IDynamicMetaObjectProvider>( dataObject );

            if ( !( ( dataObject is IDynamicMetaObjectProvider ) || ( dataObjectAsEnumerable != null ) ) )
            {
                dataObject = AppendFollowing( context, dataObject );
            }

            dynamic resultDataObject = dataObject;

            var resultDataObjectAsCollection = new List<IDynamicMetaObjectProvider>();

            // If requested only followed items filter
            if ( followFilterType == FollowFilterType.Followed )
            {
                if ( dataObjectAsEnumerable != null )
                {
                    foreach ( dynamic item in ( IEnumerable ) resultDataObject )
                    {
                        if ( item.IsFollowing == true )
                        {
                            resultDataObjectAsCollection.Add( item );
                        }
                    }

                    resultDataObject = resultDataObjectAsCollection;
                }
                else
                {
                    if ( resultDataObject.IsFollowing == false )
                    {
                        return null;
                    }
                }
            }

            // If requested only unfollowed items filter
            if ( followFilterType == FollowFilterType.NotFollowed )
            {
                if ( dataObjectAsEnumerable != null )
                {
                    foreach ( dynamic item in ( IEnumerable ) resultDataObject )
                    {
                        if ( item.IsFollowing == false )
                        {
                            resultDataObjectAsCollection.Add( item );
                        }
                    }

                    resultDataObject = resultDataObjectAsCollection;
                }
                else
                {
                    if ( resultDataObject.IsFollowing == true )
                    {
                        return null;
                    }
                }
            }

            return resultDataObject;
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
        public static string Linkify( string input )
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
            var page = HttpContext.Current?.Handler as RockPage;

            if ( page != null )
            {
                HtmlMeta metaTag = new HtmlMeta();
                metaTag.Attributes.Add( attributeName, attributeValue );
                metaTag.Content = input;
                page.AddMetaTag( metaTag );
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
            var page = HttpContext.Current?.Handler as RockPage;

            if ( page != null )
            {
                // If the link already exists, do not add it again.
                foreach ( var ctl in page.Header.Controls )
                {
                    if ( ctl is HtmlLink ctlLink )
                    {
                        if ( ctlLink.Attributes["href"] == input
                             && ctlLink.Attributes[attributeName] == attributeValue )
                        {
                            return null;
                        }
                    }
                }

                var imageLink = new HtmlLink();
                imageLink.Attributes.Add( attributeName, attributeValue );
                imageLink.Attributes.Add( "href", input );

                page.Header.Controls.Add( imageLink );
            }

            return null;
        }

        /// <summary>
        /// Set the page and browser title
        /// </summary>
        /// <param name="input">The input to use for the href of the tag.</param>
        /// <param name="titleLocation">The title location. "BrowserTitle", "PageTitle" or "All"</param>
        /// <returns></returns>
        public static string SetPageTitle( string input, string titleLocation = "All" )
        {
            var page = HttpContext.Current?.Handler as RockPage;

            if ( page != null )
            {
                if ( string.IsNullOrWhiteSpace( titleLocation ) )
                {
                    titleLocation = "All";
                }

                if ( titleLocation.Equals( "BrowserTitle", StringComparison.InvariantCultureIgnoreCase ) || titleLocation.Equals( "All", StringComparison.InvariantCultureIgnoreCase ) )
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
        /// <param name="context">The current Lava render context.</param>
        /// <param name="input">The input.</param>
        /// <param name="fingerprintLink">if set to <c>true</c> [fingerprint link].</param>
        /// <returns></returns>
        public static string AddScriptLink( ILavaRenderContext context, string input, bool fingerprintLink = false )
        {
            var page = HttpContext.Current?.Handler as RockPage;

            if ( page != null )
            {
                RockPage.AddScriptLink( page, ResolveRockUrl( context, input ), fingerprintLink );
            }

            return string.Empty;
        }

        /// <summary>
        /// Adds the CSS link.
        /// </summary>
        /// <param name="context">The current Lava render context.</param>
        /// <param name="input">The input.</param>
        /// <param name="fingerprintLink">if set to <c>true</c> [fingerprint link].</param>
        /// <returns></returns>
        public static string AddCssLink( ILavaRenderContext context, string input, bool fingerprintLink = false )
        {
            var page = HttpContext.Current?.Handler as RockPage;

            if ( page != null )
            {
                RockPage.AddCSSLink( page, ResolveRockUrl( context, input ), fingerprintLink );
            }

            return string.Empty;
        }

        /// <summary>
        /// Writes a cookie to the current HttpResponse.
        /// </summary>
        /// <param name="input">The cookie key.</param>
        /// <param name="value">The cookie value.</param>
        /// <param name="expiry">The number of minutes after which the cookie will expire.</param>
        /// <returns>An empty string if the action succeeds, or an error message.</returns>
        public static string WriteCookie( object input, object value, string expiry = null )
        {
            var key = input.ToStringSafe().Trim();

            // There is some inconsistency in the way various browsers store a cookie with an empty key.
            // To avoid unexpected results, we will disallow it here.
            if ( string.IsNullOrEmpty( key ) )
            {
                return "WriteCookie failed: A Key must be specified.";
            }

            var response = System.Web.HttpContext.Current?.Response;

            if ( response == null )
            {
                return "WriteCookie failed: A Http Session is required.";
            }

            var expiryMinutes = expiry.AsIntegerOrNull();

            var cookie = new HttpCookie( key, value.ToStringSafe() );

            if ( expiryMinutes.HasValue )
            {
                cookie.Expires = Rock.Utility.Settings.RockInstanceConfig.SystemDateTime.AddMinutes( expiryMinutes.Value );
            }

            response.Cookies.Set( cookie );

            // Return an empty string to indicate success, because this "filter" has a side-effect with no output.
            return string.Empty;
        }

        /// <summary>
        /// Reads a cookie value from the current HttpRequest.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The cookie value, or null if the cookie does not exist.</returns>
        public static string ReadCookie( object input )
        {
            var key = input.ToStringSafe().Trim();

            if ( string.IsNullOrEmpty( key ) )
            {
                return "ReadCookie failed: A Key must be specified.";
            }

            var request = System.Web.HttpContext.Current?.Request;

            if ( request != null )
            {
                if ( request.Cookies.AllKeys.Contains( key ) )
                {
                    return request.Cookies[key].Value;
                }
            }

            return null;
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
                            address = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                        }

                        // nicely format localhost
                        if ( address == "::1" )
                        {
                            address = "localhost";
                        }

                        return address;
                    }
                case "LOGIN":
                    {
                        return HttpContext.Current.Request.ServerVariables["AUTH_USER"];
                    }
                case "BROWSER":
                    {
                        Parser uaParser = Parser.GetDefault();
                        ClientInfo client = uaParser.Parse( HttpContext.Current.Request.UserAgent.ToStringSafe() );

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
        public static string RatingMarkup( object input )
        {
            var rating = 0;

            if ( input != null )
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
        /// Returns information about the current page.
        /// </summary>
        /// <param name="context">The current Lava render context.</param>
        /// <param name="input">The input.</param>
        /// <param name="parm">The type of information to return about the current page.</param>
        /// <returns>Information about the current page or null if not found.</returns>
        public static object Page( ILavaRenderContext context, string input, string parm )
        {
            PageCache pageCache = null;
            var page = HttpContext.Current?.Handler as RockPage;

            var rockRequestContext = context.GetRockRequestContext();
            if ( rockRequestContext != null )
            {
                pageCache = rockRequestContext.Page;
            }
            else if ( page != null )
            {
                pageCache = PageCache.Get( page.PageId );
            }

            if ( pageCache == null )
            {
                return null;
            }

            switch ( parm )
            {
                case "Title":
                    {
                        return pageCache.PageTitle;
                    }

                case "BrowserTitle":
                    {
                        return pageCache.BrowserTitle;
                    }

                case "Url":
                    {
                        return HttpContext.Current.Request.UrlProxySafe().AbsoluteUri;
                    }

                case "Id":
                    {
                        return pageCache.Id.ToString();
                    }

                case "Host":
                    {
                        var host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );
                        return host;
                    }

                case "Path":
                    {
                        return HttpContext.Current.Request.UrlProxySafe().AbsolutePath;
                    }

                case "SiteName":
                    {
                        return pageCache.Site;
                    }

                case "SiteId":
                    {
                        return pageCache.SiteId;
                    }

                case "Theme":
                    {
                        if ( page?.Theme != null )
                        {
                            return page.Theme;
                        }

                        return pageCache.SiteTheme;
                    }
                case "Description":
                    {
                        if ( page?.MetaDescription != null )
                        {
                            return page.MetaDescription;
                        }

                        return pageCache.Description;
                    }
                case "Layout":
                    {
                        return pageCache.Layout?.Name;
                    }

                case "Scheme":
                    {
                        return HttpContext.Current.Request.UrlProxySafe().Scheme;
                    }

                case "QueryString":
                    {
                        if ( rockRequestContext != null )
                        {
                            return rockRequestContext.PageParameters;
                        }
                        else if ( page != null )
                        {
                            return page.PageParameters();
                        }

                        return null;
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
                default:
                    return null;
            }
        }

        /// <summary>
        /// Adds the response header.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="headerName">Name of the header.</param>
        public static void AddResponseHeader( string input, string headerName )
        {
            // Check if header already exists, if so remove current value.
            if ( HttpContext.Current.Response.Headers.AllKeys.Contains( headerName ) )
            {
                HttpContext.Current.Response.Headers.Remove( headerName );
            }

            HttpContext.Current.Response.AddHeader( headerName, input );
        }

        /// <summary>
        /// Returns the specified page parameter.
        /// </summary>
        /// <param name="context">The current Lava render context.</param>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parameter name.</param>
        /// <returns>The page parameter or null if not found.</returns>
        public static object PageParameter( ILavaRenderContext context, string input, string parm )
        {
            string parmReturn;

            var rockRequestContext = context.GetRockRequestContext();
            if ( rockRequestContext != null )
            {
                parmReturn = rockRequestContext.GetPageParameter( parm );
            }
            else
            {
                var page = HttpContext.Current?.Handler as RockPage;
                parmReturn = page?.PageParameter( parm );
            }

            if ( parmReturn == null )
            {
                return null;
            }

            if ( parmReturn.AsIntegerOrNull().HasValue )
            {
                return parmReturn.AsIntegerOrNull();
            }

            return parmReturn;
        }

        /// <summary>
        /// Sets a parameter in the input URL string and returns the updated URL.
        /// </summary>
        /// <param name="inputUrl">The input URL to be modified.</param>
        /// <param name="parameterName">The name of the URL parameter to modify.</param>
        /// <param name="parameterValue">The new value parameter value.</param>
        /// <param name="outputUrlFormat">The format of the output URL, specified as {"absolute"|"relative"}. If not specified, the default value is "absolute".</param>
        /// <returns>The updated URL, or the unmodified input string if it cannot be parsed as a URL.</returns>
        public static string SetUrlParameter( object inputUrl, object parameterName, object parameterValue, object outputUrlFormat )
        {
            // Determine the input URL.
            var inputUrlString = inputUrl.ToStringSafe();
            if ( string.IsNullOrWhiteSpace( inputUrlString ) )
            {
                return string.Empty;
            }

            Uri inputUri = null;
            if ( inputUrlString.Trim().ToLower() == "current" )
            {
                inputUri = HttpContext.Current?.Request?.Url;
            }
            else
            {
                try
                {
                    inputUri = new Uri( inputUrlString );
                }
                catch
                {
                    // If the Uri failed to parse, it cannot be processed further.
                }
            }
            // If the input string cannot be parsed as a URL, return it unmodified.
            if ( inputUri == null )
            {
                return inputUrl.ToStringSafe();
            }

            // Determine if the input URL is associated with a Rock Page.
            var uriBuilder = new UriBuilder( inputUri );
            var inputPageReference = new PageReference( uriBuilder.Uri, null );

            var isRockPage = ( inputPageReference.PageId != 0 );

            // Determine the parameter type.
            var parameterNameString = parameterName.ToStringSafe().Trim();
            var isPageChange = isRockPage && parameterNameString.Equals( "pageid", StringComparison.OrdinalIgnoreCase );

            // Collate the route and query parameters.
            var allParameters = new Dictionary<string, string>();
            Dictionary<string, string> routeParameterValues = null;
            Dictionary<string, string> queryParameterValues = null;

            foreach ( var kvp in inputPageReference.Parameters )
            {
                allParameters.AddOrIgnore( kvp.Key, kvp.Value );
            }
            foreach ( var nv in inputPageReference.QueryString.AllKeys )
            {
                allParameters.AddOrIgnore( nv, inputPageReference.QueryString[nv] );
            }

            // Add, remove or modify the target parameter.
            var setValue = parameterValue?.ToString() ?? string.Empty;
            if ( isPageChange )
            {
                // If this is a Rock page change, it will be handled separately as a special route parameter.
                setValue = "";
            }
            if ( string.IsNullOrEmpty( setValue ) )
            {
                allParameters.Remove( parameterNameString );
            }
            else
            {
                allParameters[parameterNameString] = setValue;
            }

            if ( isRockPage )
            {
                // If the input URL refers to an existing Rock page, determine the correct output page.
                int outputPageId;
                if ( isPageChange )
                {
                    // If the input URL refers to an existing Rock page and the parameter to modify is "PageId",
                    // we are switching to a new target page.
                    // In this case, we will try to apply the route and query parameters from the input URL to the new page.
                    outputPageId = parameterValue.ToIntSafe();
                }
                else
                {
                    outputPageId = inputPageReference.PageId;
                }

                var outputPage = PageCache.Get( outputPageId );

                // Determine which route is the best fit for all of the available parameters,
                // including the parameter that is being added or modified.
                var outputPageReference = PageReference.GetBestMatchForParameters( outputPageId, allParameters );

                // Now that we have determined the correct route, redistribute the route and query parameters.
                var routeParameterNames = RouteUtils.ParseRouteParameters( outputPageReference.Route );
                routeParameterValues = allParameters.Where( x => routeParameterNames.Contains( x.Key ) )
                    .ToDictionary( k => k.Key, v => v.Value );
                queryParameterValues = allParameters.Where( x => !routeParameterNames.Contains( x.Key ) )
                    .ToDictionary( k => k.Key, v => v.Value );

                var path = outputPageReference.BuildRouteURL( routeParameterValues ).TrimEnd( '/' );
                if ( string.IsNullOrEmpty( path ) )
                {
                    path = $"/page/{outputPageId}";
                }

                uriBuilder.Path = path;
            }
            else
            {
                // If the URL does not reference a Rock page, all parameters are part of the query string.
                queryParameterValues = allParameters;
            }

            // Update or add the parameter in the query string of the URL.
            var queryValues = new NameValueCollection();
            if ( !string.IsNullOrWhiteSpace( parameterNameString ) )
            {
                foreach ( var qpv in queryParameterValues )
                {
                    queryValues[qpv.Key] = qpv.Value;
                }
            }
            uriBuilder.Query = queryValues.ToQueryString( false );

            // Construct the output URL.
            string url;
            if ( outputUrlFormat.ToStringSafe().ToLower() == "relative" )
            {
                var baseUri = new Uri( uriBuilder.Uri.GetLeftPart( UriPartial.Authority ) );
                url = "/" + baseUri.MakeRelativeUri( uriBuilder.Uri ).ToString();
            }
            else
            {
                url = uriBuilder.Uri.AbsoluteUri;
            }

            return url;
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
            else if ( input is List<object> keyValueList )
            {
                // If the input value is a list of two items, assume it is a key/value pair.
                if ( keyValueList.Count == 2 )
                {
                    result.Add( "Key", keyValueList[0].ToStringOrDefault( string.Empty ) );
                    result.Add( "Value", keyValueList[1] );
                }
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
            return ( int? ) input.ToString().AsDecimalOrNull();
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
        public static DateTimeOffset? AsDateTime( object input )
        {
            if ( input == null )
            {
                return null;
            }

            // If the input is a DateTime object, return it.
            if ( input is DateTime dt )
            {
                return dt;
            }

            // If the input is a DateTimeOffset object, return it unchanged.
            if ( input is DateTimeOffset dto )
            {
                return dto;
            }

            // Parse the input to a DateTime.
            var rockDateTime = LavaDateTime.ParseToOffset( input.ToString() );

            return rockDateTime;
        }

        /// <summary>
        /// Converts the input value to a DateTimeOffset value in Coordinated Universal Time (UTC).
        /// If the input value does not specify an offset, the current Rock time zone is assumed.
        /// </summary>
        /// <param name="input">The input value to be parsed into DateTime form.</param>
        /// <returns>A DateTimeOffset value with an offset of 0, or null if the conversion could not be performed.</returns>
        public static DateTimeOffset? AsDateTimeUtc( object input )
        {
            DateTimeOffset? utc;
            if ( input is DateTime dt )
            {
                utc = LavaDateTime.ConvertToDateTimeOffset( dt ).ToUniversalTime();
            }
            else if ( input is DateTimeOffset dto )
            {
                utc = dto.ToUniversalTime();
            }
            else
            {
                utc = LavaDateTime.ParseToUtc( input.ToStringSafe() );
            }

            return utc;
        }

        /// <summary>
        /// Creates the short link.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="token">The token.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="randomLength">The random length.</param>
        /// <returns></returns>
        public static string CreateShortLink( ILavaRenderContext context, object input, string token = "", int? siteId = null, bool overwrite = false, int randomLength = 10 )
        {
            // Notes: This filter attempts to return a valid shortlink at all costs
            //        this means that if the configuration passed to it is invalid
            //        it will try to correct it with reasonable defaults. For instance
            //        if you pass in an invalid siteId, the first active site will be used.

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
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
                // We can't use the provided shortlink because it's already used, so get a random token.
                // Garbage in Random out
                shortLink = null;
                token = shortLinkService.GetUniqueToken( siteId.Value, 10 );
            }

            if ( shortLink == null )
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

        /// <summary>
        /// Returns the image URL for the provided integer ID or Guid input, or a fallback URL if the input is not defined.
        /// </summary>
        /// <param name="input">The input. This may be an integer ID or a Guid.</param>
        /// <param name="fallbackUrl">The fallback URL to be used if the input is not defined.</param>
        /// <param name="rootUrl">
        /// This parameter is multipurpose:
        /// <para>If the string value 'rootUrl' or true, the application root URL will be prepended and the GetImage handler will be used to serve the image.</para>
        /// <para>If false or null, the application root URL will not be prepended but the GetImage handler will be used to serve the image.</para>
        /// </param>
        /// <returns>The image URL for the provided integer ID or Guid input, or a fallback URL if the input is not defined.</returns>
        public static string ImageUrl( object input, string fallbackUrl = null, object rootUrl = null )
        {
            string inputString = input?.ToString();
            var queryStringKey = "Id";
            var useFallbackUrl = false;

            if ( !inputString.AsIntegerOrNull().HasValue )
            {
                queryStringKey = "Guid";

                if ( !inputString.AsGuidOrNull().HasValue )
                {
                    RockLogger.Log.Information( RockLogDomains.Lava, $"The input value provided ('{( inputString ?? "null" )}') is neither an integer nor a Guid." );
                    useFallbackUrl = true;
                }
            }

            if ( useFallbackUrl )
            {
                return fallbackUrl ?? string.Empty;
            }

            /*
                8/12/2020 - JH
                The rootUrl parameter is multipurpose:

                1. If the object is a string whose value is "rootUrl" OR the object is a bool whose value is true:
                       prepend the application root URL and use the GetImage handler to serve the image.

                2. If the object is a bool whose value is false OR the object is null:
                       do not prepend the application root URL but use the GetImage handler to serve the image.

                3. Future dev will dictate (i.e. If the object is a string whose value is "cdnUrl" ...)
            */
            bool useGetImageHandler = false;
            bool prependAppRootUrl = false;

            string rootUrlString = rootUrl?.ToString();

            // Note that this will return false (and not null) for any string value other than a "truthy" value, meaning that any string comparisons below need to done BEFORE a rootUrlAsBool.HasValue check.
            bool? rootUrlAsBool = rootUrlString?.AsBooleanOrNull();

            if ( rootUrlString?.Equals( "rootUrl", StringComparison.OrdinalIgnoreCase ) == true )
            {
                useGetImageHandler = true;
                prependAppRootUrl = true;
            }
            else if ( rootUrlAsBool.HasValue || rootUrl == null )
            {
                useGetImageHandler = true;
                prependAppRootUrl = rootUrlAsBool ?? false;
            }

            string url = null;

            if ( useGetImageHandler )
            {
                string prefix = prependAppRootUrl ? GlobalAttributesCache.Value( "PublicApplicationRoot" ) : "/";

                url = $"{prefix}GetImage.ashx?{queryStringKey}={inputString}";
            }

            return url;
        }

        /// <summary>
        /// Returns a named configuration setting for the current Rock instance.
        /// </summary>
        /// <param name="input">The name of the configuration setting.</param>
        /// <returns>A configuration value.</returns>
        public static object RockInstanceConfig( object input )
        {
            var valueName = input.ToStringSafe().Trim().ToLower();

            if ( valueName == "applicationdirectory" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.ApplicationDirectory;
            }
            else if ( valueName == "isclustered" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.IsClustered;
            }
            else if ( valueName == "machinename" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.MachineName;
            }
            else if ( valueName == "physicaldirectory" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.PhysicalDirectory;
            }
            else if ( valueName == "systemdatetime" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.SystemDateTime;
            }
            else if ( valueName == "aspnetversion" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.AspNetVersion;
            }
            else if ( valueName == "lavaengine" )
            {
                return Rock.Utility.Settings.RockInstanceConfig.LavaEngineName;
            }

            return $"Configuration setting \"{ input }\" is not available.";
        }

        /// <summary>
        /// Adds a QuickReturn to PersonalLinks.
        /// Note that this is only supported for pages that have the PersonalLinks block on it.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeOrder">The type order.</param>
        public static void AddQuickReturn( string input, string typeName, int typeOrder = 0 )
        {
            var rockPage = HttpContext.Current?.Handler as RockPage;
            if ( rockPage == null )
            {
                return;
            }

            if ( input.IsNotNullOrWhiteSpace() )
            {
                /* 08-16-2021 MDP
                 * This is only supported for pages that have the PersonalLinks block on it.
                 * 
                 * 02-02-2022 SMC
                 * Added checks to prevent this script from being called if the personalLinks script hasn't been loaded,
                 * so that if this filter is used on a page without the Personal Links block, it will fail safely
                 * without doing anything.
                 */

                input = input.EscapeQuotes();

                if ( ScriptManager.GetCurrent( rockPage ).IsInAsyncPostBack )
                {
                    var quickReturnScript = "" +
                    $"Sys.Application.add_load( function () {{" + Environment.NewLine +
                    $"  if (typeof Rock !== 'undefined' && typeof Rock.personalLinks !== 'undefined') {{" + Environment.NewLine +
                    $"    Rock.personalLinks.addQuickReturn( '{typeName}', {typeOrder}, '{input}' );" + Environment.NewLine +
                    $"  }}" + Environment.NewLine +
                    $"}});";
                    ScriptManager.RegisterStartupScript( rockPage, rockPage.GetType(), "AddQuickReturn", quickReturnScript, true );
                }
                else
                {
                    var quickReturnScript = "" +
                  $"$( document ).ready( function () {{" + Environment.NewLine +
                  $"  if (typeof Rock !== 'undefined' && typeof Rock.personalLinks !== 'undefined') {{" + Environment.NewLine +
                  $"    Rock.personalLinks.addQuickReturn( '{typeName}', {typeOrder}, '{input}' );" + Environment.NewLine +
                  $"  }}" + Environment.NewLine +
                  $"}});";
                    RockPage.AddScriptToHead( rockPage, quickReturnScript, true );
                }
            }
        }

        /// <summary>
        /// Converts structured blocks designed with the <see cref="StructureContentEditor"/> control from JSON to HTML.
        /// <para>
        /// Note that this only works with JSON produced by the <see cref="StructureContentEditor"/> control as it
        /// contains metadata used in converting the JSON content to HTML.
        /// </para>
        /// </summary>
        /// <param name="content">JSON formatted string produced by the <see cref="StructureContentEditor"/> control.</param>
        /// <returns></returns>
        public static string RenderStructuredContentAsHtml( string content )
        {
            var helper = new StructuredContentHelper( content );
            return helper.Render();
        }

        #endregion Misc Filters

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
        public static bool Contains( object input, object containValue )
        {
            var inputList = ( input as IList );
            if ( inputList != null )
            {
                return inputList.Contains( containValue );
            }

            return false;
        }

        /// <summary>
        /// Filters a collection of items on a specified property and value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filter">The filter expression or filter property name.</param>
        /// <param name="filterValue">The filter value, if the filter parameter specifies a property name.</param>
        /// <param name="comparisonType">The type of comparison for the filter value, either "equal" (default) or "notequal".</param>
        /// <returns></returns>
        public static object Where( object input, string filter, object filterValue = null, string comparisonType = null )
        {
            if ( filter != null
                 && filterValue != null )
            {
                return WhereInternal( input, filter, filterValue, comparisonType );
            }
            else
            {
                return WhereInternal( input, filter );
            }
        }

        /// <summary>
        /// Filters a collection of items on a specified property and value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterKey">The filter key.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="comparisonType">The type of comparison for the filter value, either "equal" (default) or "notequal".</param>
        /// <returns></returns>
        private static object WhereInternal( object input, string filterKey, object filterValue, string comparisonType )
        {
            if ( input == null || !( input is IEnumerable ) )
            {
                return input;
            }

            comparisonType = ( comparisonType ?? "equal" ).ToLower();
            comparisonType = ( comparisonType == "equal" || comparisonType == "notequal" ) ? comparisonType : "equal";

            var result = new List<object>();

            foreach ( var value in ( ( IEnumerable ) input ) )
            {
                ILavaDataDictionary lavaObject = null;

                if ( value is ILavaDataDictionarySource lavaSource )
                {
                    lavaObject = lavaSource.GetLavaDataDictionary();
                }
                else if ( value is ILavaDataDictionary )
                {
                    lavaObject = value as ILavaDataDictionary;
                }

                if ( lavaObject != null )
                {
                    if ( lavaObject.ContainsKey( filterKey )
                            && ( ( comparisonType == "equal" && GetLavaCompareResult( lavaObject.GetValue( filterKey ), filterValue ) == 0 )
                                 || ( comparisonType == "notequal" && GetLavaCompareResult( lavaObject.GetValue( filterKey ), filterValue ) != 0 ) ) )
                    {
                        result.Add( lavaObject );
                    }
                }
                else if ( value is IDictionary<string, object> )
                {
                    var dictionaryObject = value as IDictionary<string, object>;
                    if ( dictionaryObject.ContainsKey( filterKey )
                             && ( ( dynamic ) dictionaryObject[filterKey] == ( dynamic ) filterValue && comparisonType == "equal"
                                    || ( ( dynamic ) dictionaryObject[filterKey] != ( dynamic ) filterValue && comparisonType == "notequal" ) ) )
                    {
                        result.Add( dictionaryObject );
                    }
                }
                else if ( value is object )
                {
                    var propertyValue = value.GetPropertyValue( filterKey );

                    // Allow for null checking as an empty string. Could be differing opinions on this...?!
                    if ( propertyValue == null )
                    {
                        propertyValue = string.Empty;
                    }

                    var compareResult = GetLavaCompareResult( propertyValue, filterValue );

                    if ( ( compareResult == 0 && comparisonType == "equal" )
                            || ( compareResult != 0 && comparisonType == "notequal" ) )
                    {
                        result.Add( value );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the result of a comparison between two values, indicating if the left value is less than, greater than or equal to the right value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>
        /// A signed integer that indicates the relative values of x and y.
        /// -2: x is not equal to y, but the comparison is indeterminate.
        /// -1: x is less than y.
        /// 0: x equals y.
        /// +1: x is greater than y.
        /// </returns>
        private static int GetLavaCompareResult( object left, object right )
        {
            if ( left == null || right == null )
            {
                if ( left == null && right == null )
                {
                    return 0;
                }

                // Return a result that indicates inqueality without specifying greater or less.
                return -2;
            }

            // Compare DateTimeOffset values by converting to DateTime to ignore differences in offset.
            if ( right is DateTimeOffset rightDto )
            {
                right = rightDto.DateTime;
            }

            if ( left is DateTimeOffset leftDto )
            {
                left = leftDto.DateTime;
            }

            // If the operand types are not the same, try to convert the right type to the left type.
            var leftType = left.GetType();
            var rightType = right.GetType();


            if ( leftType != rightType )
            {
                if ( leftType.IsEnum )
                {
                    right = Enum.Parse( leftType, right.ToString() );
                }
                else
                {
                    right = Convert.ChangeType( right, leftType );
                }
            }

            var compareResult = Comparer.Default.Compare( left, right );

            return compareResult;
        }

        /// <summary>
        /// Filters a collection of items by applying the specified Linq predicate.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        private static object WhereInternal( object input, string filter )
        {
            if ( input is IEnumerable )
            {
                IEnumerable enumerableInput;

                if ( input is List<object> objectList )
                {
                    if ( objectList.Any() )
                    {
                        var itemType = objectList.FirstOrDefault().GetType();

                        enumerableInput = ConvertListItemsToType( objectList, itemType ) as IEnumerable;
                    }
                    else
                    {
                        return new List<object>();
                    }
                }
                else
                {
                    enumerableInput = ( IEnumerable ) input;
                }

                // The new System.Linq.Dynamic.Core only works on Queryables
                return enumerableInput.AsQueryable().Where( filter );
            }

            return null;
        }

        /// <summary>
        /// Convert a generic list of objects to a list of the specified target type.
        /// </summary>
        /// <param name="items">The list of items to convert.</param>
        /// <param name="convertedItemType">The type to which the items will be converted.</param>
        /// <returns></returns>
        private static object ConvertListItemsToType( List<object> items, Type convertedItemType )
        {
            var enumerableType = typeof( System.Linq.Enumerable );

            var castMethod = enumerableType.GetMethod( nameof( System.Linq.Enumerable.Cast ) )
                .MakeGenericMethod( convertedItemType );
            var toListMethod = enumerableType.GetMethod( nameof( System.Linq.Enumerable.ToList ) )
                .MakeGenericMethod( convertedItemType );

            IEnumerable<object> itemsToConvert;

            itemsToConvert = items.Select( item => Convert.ChangeType( item, convertedItemType ) );

            var convertedItems = castMethod.Invoke( null, new[] { itemsToConvert } );

            return toListMethod.Invoke( null, new[] { convertedItems } );
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

                foreach ( var value in ( ( IEnumerable ) input ) )
                {
                    if ( value is ILavaDataDictionary lavaDictionary )
                    {
                        if ( lavaDictionary.ContainsKey( selectKey ) )
                        {
                            result.Add( lavaDictionary.GetValue( selectKey ) );
                        }
                    }
                    else if ( value is IDictionary<string, object> genericDictionary )
                    {
                        if ( genericDictionary.ContainsKey( selectKey ) )
                        {
                            result.Add( genericDictionary[selectKey] );
                        }
                    }
                    else if ( value is IDictionary simpleDictionary )
                    {
                        if ( simpleDictionary.Contains( selectKey ) )
                        {
                            result.Add( simpleDictionary[selectKey] );
                        }
                    }
                    else if ( value is DynamicObject dyo )
                    {
                        // Try to resolve the key as a property of the object.
                        // We can assume that all properties of a runtime type are available to Lava.
                        result.Add( dyo.GetPropertyValue( selectKey ) );
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
        /// <param name="sortOrder">asc or desc for sort order.</param>
        /// <returns></returns>
        public static object SortByAttribute( ILavaRenderContext context, object input, string attributeKey, string sortOrder = "asc" )
        {
            if ( input is IEnumerable )
            {
                var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
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
                            if ( sortOrder.ToLower() == "desc" )
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

            IList inputList;

            if ( ( input is IList ) )
            {
                inputList = input as IList;
            }
            else if ( ( input is IEnumerable ) )
            {
                inputList = ( input as IEnumerable ).Cast<object>().ToList();
            }
            else
            {
                return input;
            }

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

            // The new System.Linq.Dynamic.Core only works on Queryabless
            return e.AsQueryable().Distinct().Cast<object>().ToList();
        }

        /// <summary>
        /// Orders a collection of elements by the specified property (or properties)
        /// and returns a new collection in that order.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property">The property or properties to order the collection by.</param>
        /// <returns>A new collection sorted in the requested order.</returns>
        /// <example>
        ///     {% assign members = 287635 | GroupById | Property:'Members' | OrderBy:'GroupRole.IsLeader desc,Person.FullNameReversed' %}
        ///    <ul>
        ///    {% for member in members %}
        ///        <li>{{ member.Person.FullName }} - {{ member.GroupRole.Name }}</li>
        ///    {% endfor %}
        ///    </ul>
        /// </example>
        public static IEnumerable OrderBy( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object> { input };

            if ( !e.Any() || string.IsNullOrWhiteSpace( property ) )
            {
                return e;
            }

            //
            // Create a list of order by objects for the field to order by
            // and the ascending/descending flag.
            //
            var orderBy = property
                .Split( ',' )
                .Select( s => s.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries ) )
                .Select( a => new { Property = a[0], Descending = a.Length >= 2 && "desc" == a[1].ToLower() } )
                .ToList();

            //
            // Do initial ordering of first property requested.
            //
            IOrderedQueryable<object> qry;
            if ( orderBy[0].Descending )
            {
                qry = e.Cast<object>().AsQueryable().OrderByDescending( d => GetPropertyPathValue( d, orderBy[0].Property ) );
            }
            else
            {
                qry = e.Cast<object>().AsQueryable().OrderBy( d => GetPropertyPathValue( d, orderBy[0].Property ) );
            }

            //
            // For the rest use ThenBy and ThenByDescending.
            //
            for ( int i = 1; i < orderBy.Count; i++ )
            {
                var propertyName = orderBy[i].Property; // This can't be inlined. -dsh

                if ( orderBy[i].Descending )
                {
                    qry = qry.ThenByDescending( d => GetPropertyPathValue( d, propertyName ) );
                }
                else
                {
                    qry = qry.ThenBy( d => GetPropertyPathValue( d, propertyName ) );
                }
            }

            return qry.ToList();
        }

        #endregion Array Filters

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
        public static List<Note> Notes( ILavaRenderContext context, object input, object noteType, string sortOrder = "desc", int? count = null )
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
                noteTypeIds.Add( ( int ) noteType );
            }

            if ( noteType is string )
            {
                noteTypeIds = ( ( string ) noteType ).Split( ',' ).Select( Int32.Parse ).ToList();
            }

            var notes = new NoteService( LavaHelper.GetRockContextFromLavaContext( context ) )
                .Queryable().AsNoTracking()
                .Include( n => n.CreatedByPersonAlias )
                .Where( n => n.EntityId == entityId );

            if ( noteTypeIds.Count > 0 )
            {
                notes = notes.Where( n => noteTypeIds.Contains( n.NoteTypeId ) );
            }
            else
            {
                return null;
            }

            // add sort order
            if ( sortOrder == "desc" )
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
        public static bool HasRightsTo( ILavaRenderContext context, object input, string verb, string typeName = "" )
        {
            if ( string.IsNullOrWhiteSpace( verb ) )
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
                    var model = ( ISecured ) input;
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
                    id = ( int ) input;
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
                        id = ( int ) propertyInfo.GetValue( input, null );
                    }
                }

                if ( id.HasValue )
                {
                    var entityTypes = EntityTypeCache.All();
                    var entityTypeCache = entityTypes.Where( e => String.Equals( e.Name, typeName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();

                    if ( entityTypeCache != null )
                    {
                        RockContext _rockContext = LavaHelper.GetRockContextFromLavaContext( context );

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
        /// Determines whether a person is following an entity.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input entity to use for follow testing.</param>
        /// <param name="parameter1">The parameter1.</param>
        /// <param name="parameter2">The parameter2.</param>
        /// <returns><c>true</c> if the specified context is followed; otherwise, <c>false</c>.</returns>
        public static bool IsFollowed( ILavaRenderContext context, object input, object parameter1 = null, object parameter2 = null )
        {
            /*
                JME 4/4/2022
                This filter used to take one optional parameter 'AlternatePerson'. We however want to also be able to provide
                a 'PurposeKey'. The purpose key though should be the second paramter. We change the input names to be parameter1 and
                parameter2 so we can keep any old use cases (using alternate person) though unlikely working.
            */

            Person personObject = null;
            var purposeKey = string.Empty;

            // Check if the first parameter is a person (backwards compatibility). If it's
            // not then we can assume the second parameter is for the alternate person
            if ( parameter1 is Person )
            {
                personObject = ( Person ) parameter1;
            }
            else
            {
                personObject = ( Person ) parameter2;
            }

            // Check if first parameter is the purpose key
            if ( parameter1 is String )
            {
                purposeKey = ( string ) parameter1;
            }

            //
            // Ensure the input is an entity object.
            //
            if ( !( input is IEntity entity ) )
            {
                return false;
            }

            //
            // If the person was not specified in the parameters then use
            // the current person. If still not found then return not-followed
            // status.
            //
            if ( !( personObject is Person person ) )
            {
                person = GetCurrentPerson( context );

                if ( person == null )
                {
                    return false;
                }
            }

            using ( var rockContext = new RockContext() ) // Can't use LavaHelper.GetRockContextFromLavaContext( context) since it's wrapped in a using
            {
                int followingEntityTypeId = entity.TypeId;
                var followedQry = new FollowingService( rockContext ).Queryable()
                    .Where( f => f.EntityTypeId == followingEntityTypeId && f.EntityId == entity.Id )
                    .Where( f => f.PersonAlias.PersonId == person.Id );

                // Add purpose key logic
                if ( purposeKey.IsNotNullOrWhiteSpace() )
                {
                    followedQry = followedQry.Where( f => f.PurposeKey == purposeKey );
                }
                else
                {
                    followedQry = followedQry.Where( f => string.IsNullOrEmpty( f.PurposeKey ) );
                }

                return followedQry.Any();
            }
        }

        /// <summary>
        /// Translates a cached object into a fully database-backed entity object.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input cached object to be translated.</param>
        /// <returns>An <see cref="IEntity"/> object or the original <paramref name="input"/>.</returns>
        public static object EntityFromCachedObject( ILavaRenderContext context, object input )
        {
            //
            // Ensure the input is a cached object.
            //
            if ( input == null || !( input is IEntityCache cache ) )
            {
                return input;
            }

            //
            // Ensure the cache object actually inherits from EntityCache.
            //
            var entityCacheType = typeof( EntityCache<,> );
            if ( !cache.GetType().IsDescendentOf( entityCacheType ) )
            {
                return input;
            }

            var entityType = cache.GetType().GetGenericArgumentsOfBaseType( entityCacheType )[1];

            return Rock.Reflection.GetIEntityForEntityType( entityType, cache.Id );
        }

        /// <summary>
        /// Gets the IdKey hash from IEntity or an integer Id value.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns>An <see cref="IEntity"/> object or the original <paramref name="input"/>.</returns>
        public static string ToIdHash( object input )
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

            return IdHasher.Instance.GetHash( entityId.Value );
        }

        /// <summary>
        /// Gets the integer value from from a key-hash string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int? FromIdHash( string input )
        {
            if ( string.IsNullOrWhiteSpace( input ) )
            {
                return null;
            }

            return IdHasher.Instance.GetId( input );
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            if ( context.GetMergeField( "CurrentPerson" ) is Person currentPerson )
            {
                return currentPerson;
            }

            // Next check the RockRequestContext in the lava context.
            if ( context.GetInternalField( "RockRequestContext" ) is RockRequestContext currentRequest )
            {
                return currentRequest.CurrentPerson;
            }

            // Finally check the HttpContext.
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
            {
                return httpContext.Items["CurrentPerson"] as Person;
            }

            return null;
        }

        /// <summary>
        /// Converts a string or byte array into a Base64 encoded string.
        /// </summary>
        /// <param name="input">The string or byte array to be converted.</param>
        /// <example><![CDATA[
        /// {{ 'hello' | ToBase64 }}
        /// ]]></example>
        public static string Base64( object input )
        {
            if ( input is ICollection<byte> )
            {
                return Convert.ToBase64String( ( input as ICollection<byte> ).ToArray() );
            }
            else
            {
                return Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( input.ToString() ) );
            }
        }

        /// <summary>
        /// Base64 encodes a binary file
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="resizeSettings">The resize settings.</param>
        /// <returns></returns>
        public static string Base64Encode( ILavaRenderContext context, object input, string resizeSettings = null )
        {
            BinaryFile binaryFile = null;

            if ( input is int )
            {
                binaryFile = new BinaryFileService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( ( int ) input );
            }
            else if ( input is BinaryFile )
            {
                binaryFile = ( BinaryFile ) input;
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

        #endregion Object Filters

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
        /// Creates a color pair from a single color with logic for light and dark modes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="colorScheme"></param>
        /// <returns></returns>
        public static ColorPair CalculateColorPair( string input, string colorScheme = "" )
        {
            var color = new RockColor( input );

            var colorSchemeValue = ColorScheme.Light;

            if ( colorScheme.ToLower() == "dark" )
            {
                colorSchemeValue = ColorScheme.Dark;
            }

            // If a color scheme was not provided check the request object to use the clients preference
            if ( colorScheme.IsNotNullOrWhiteSpace() && HttpContext.Current.Request.Headers["Sec-CH-Prefers-Color-Scheme"] != null && HttpContext.Current.Request.Headers["Sec-CH-Prefers-Color-Scheme"] == "dark" )
            {
                colorSchemeValue = ColorScheme.Dark;
            }

            return RockColor.CalculateColorPair( color, colorSchemeValue );
        }

        /// <summary>
        /// Creates a recipe color from the provided color.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public static string CalculateRecipeColor( string input, string recipe )
        {
            var color = new RockColor( input );

            ColorRecipe recipeColor;

            if ( Enum.TryParse( recipe, true, out recipeColor ) )
            {
                return RockColor.CalculateColorRecipe( color, recipeColor ).Hex;
            }

            // There was an invalid recipe passed in so return the original color.
            return input;
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
            if ( amount.EndsWith( "deg" ) )
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
            if ( color.Alpha != 1 )
            {
                return color.ToRGBA();
            }

            if ( input.StartsWith( "#" ) )
            {
                return color.ToHex();
            }

            return color.ToRGBA();
        }

        /// <summary>Gets the hue from the provided color.</summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Double.</returns>
        public static int Hue( string input )
        {
            var color = new RockColor( input );

            return Convert.ToInt32( color.Hue );
        }

        /// <summary>Saturations the specified input.</summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Double.</returns>
        public static int Saturation( string input )
        {
            var color = new RockColor( input );

            return Convert.ToInt32( color.Saturation * 100 );
        }

        /// <summary>Luminosities the specified input.</summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Double.</returns>
        public static int Luminosity( string input )
        {
            var color = new RockColor( input );

            return Convert.ToInt32( color.Luminosity * 100 );
        }

        /// <summary>
        /// Returns a RockColor object from the input string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static RockColor AsColor( string input )
        {
            return new RockColor( input );
        }

        /// <summary>Calculates the contrast ratio.</summary>
        /// <param name="inputColor1">The input color1.</param>
        /// <param name="inputColor2">The input color2.</param>
        /// <returns>System.Double.</returns>
        public static double CalculateContrastRatio( string inputColor1, string inputColor2 )
        {
            if ( inputColor2.IsNullOrWhiteSpace() )
            {
                inputColor2 = "#ffffff";
            }

            var color1 = new RockColor( inputColor1 );
            var color2 = new RockColor( inputColor2 );

            return RockColor.CalculateContrastRatio( color1, color2 );
        }

        #endregion Color Filters

        /// <summary>
        /// Get the page route for the specified page Id or Guid.
        /// </summary>
        /// <param name="input">The page (and optional route) we are interested in.</param>
        /// <param name="parameters">Any parameters that should be included in the route URL.</param>
        /// <example><![CDATA[
        /// {{ 12 | PageRoute }}
        /// {{ '12' | PageRoute:'PersonID=10^GroupId=20' }}
        /// {{ 'Global' | Attribute:'PageAttrib','RawValue' | Pageroute:'PersonId=10' }}
        /// ]]></example>
        public static string PageRoute( object input, object parameters = null )
        {
            int pageId = 0;
            int routeId = 0;
            var parms = new Dictionary<string, string>();

            if ( input is int )
            {
                //
                // We were given a simple page Id number.
                //
                pageId = ( int ) input;
            }
            else
            {
                //
                // We have a string, it could be a page Id number as a string or a "Guid[,Guid]"
                // style page reference.
                //
                var pageString = input.ToString();

                if ( pageString.Contains( "," ) )
                {
                    //
                    // "Guid,Guid" style page reference.
                    //
                    var segments = pageString.Split( ',' );

                    var page = Rock.Web.Cache.PageCache.Get( segments[0].AsGuid() );

                    if ( page == null )
                    {
                        throw new Exception( "Page not found." );
                    }

                    pageId = page.Id;

                    var routeGuid = segments[1].AsGuid();
                    var route = page.PageRoutes.Where( r => r.Guid == routeGuid ).FirstOrDefault();

                    if ( route != null )
                    {
                        routeId = route.Id;
                    }
                }
                else
                {
                    //
                    // "Guid" or "int" style page reference.
                    //
                    var pageGuid = pageString.AsGuidOrNull();

                    if ( pageGuid.HasValue )
                    {
                        var page = Rock.Web.Cache.PageCache.Get( pageGuid.Value );

                        if ( page == null )
                        {
                            throw new Exception( "Page not found." );
                        }

                        pageId = page.Id;
                    }
                    else
                    {
                        pageId = pageString.AsInteger();
                    }
                }
            }

            //
            // Parse the parameters. They will either be a "key=value^key2=value2" style string
            // or a dictionary collection of key value pairs.
            //
            if ( parameters is string && !string.IsNullOrEmpty( ( string ) parameters ) )
            {
                var segments = parameters.ToString().Split( '^' );

                foreach ( string segment in segments )
                {
                    var kv = segment.Split( '=' );

                    if ( kv.Length == 2 )
                    {
                        parms.Add( kv[0], kv[1] );
                    }
                    else
                    {
                        throw new Exception( "Invalid page parameter specified." );
                    }
                }
            }
            else if ( parameters is IDictionary )
            {
                foreach ( DictionaryEntry kvp in ( IDictionary ) parameters )
                {
                    parms.Add( kvp.Key.ToString(), kvp.Value.ToString() );
                }
            }

            var pageReference = new Rock.Web.PageReference( pageId, routeId, parms );

            return pageReference.BuildUrl();
        }

        #region POCOs
        /// <summary>
        /// POCO to translate an HTTP cookie in to a Liquidizable form
        /// </summary>
        public class HttpCookieDrop : RockDynamic
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

        #endregion POCOs

        #region Private Methods

        /// <summary>
        /// Special use method to get the property value of some unknown object. Handles special
        /// cases like Liquid Drops and recursive property searches.
        /// </summary>
        /// <param name="obj">The object whose property value we want.</param>
        /// <param name="propertyPath">The path to the property.</param>
        /// <returns>The value at the given path or null.</returns>
        private static object GetPropertyPathValue( object obj, string propertyPath )
        {
            if ( string.IsNullOrWhiteSpace( propertyPath ) )
            {
                return obj;
            }

            var properties = propertyPath.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            while ( properties.Any() && obj != null )
            {
                if ( obj is ILavaDataDictionary lavaDictionary )
                {
                    obj = lavaDictionary.GetValue( properties.First() );
                }
                else if ( obj is IDictionary dictionary )
                {
                    obj = dictionary[properties.First()];
                }
                else if ( obj is IDictionary<string, object> stringDictionary )
                {
                    obj = stringDictionary[properties.First()];
                }
                else
                {
                    var property = obj.GetType().GetProperty( properties.First() );
                    obj = property?.GetValue( obj );
                }

                properties.RemoveAt( 0 );
            }

            return obj;
        }

        #endregion Private Methods
    }
}

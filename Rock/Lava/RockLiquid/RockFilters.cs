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
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;

using DotLiquid;
using Context = DotLiquid.Context;
using Condition = DotLiquid.Condition;

using Humanizer;
using Humanizer.Localisation;
using ImageResizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using UAParser;
using Ical.Net;
using Rock.Web.UI.Controls;
using System.Web.UI;
using Rock.Lava.DotLiquid;
using Rock.Cms.StructuredContent;

namespace Rock.Lava
{
    /// <summary>
    /// Defines filter methods available for use with the RockLiquid implementation of the Lava library.
    /// </summary>
    /// <remarks>
    /// This class is marked for internal use because it should only be used in the context of resolving a Lava template.
    /// </remarks>
    [RockInternal( "1.15", true )]
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
        /// takes computer-readable-formats and makes them human readable
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
        /// returns sentence in 'PascalCase'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCssClass( string input )
        {
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
            return NumberToOrdinal( input.ToStringOrDefault( null ) );
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
        public static string NumberToWords( object input )
        {
            return NumberToWords( input.ToStringOrDefault( null ) );
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
        public static string NumberToOrdinalWords( object input )
        {
            return NumberToOrdinalWords( input.ToStringOrDefault( null ) );
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
        public static string NumberToRomanNumerals( object input )
        {
            return NumberToRomanNumerals( input.ToStringOrDefault( null ) );
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
        /// Returns matched RegEx list of strings from inputted string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expression">The regex expression.</param>
        /// <returns></returns>
        public static List<string> RegExMatchValues( string input, string expression )
        {
            if ( input == null )
            {
                return null;
            }

            Regex regex = new Regex( expression );
            var matches = regex.Matches( input );

            // Flatten all matches to single list
            var captured = matches
                .Cast<Match>()
                .Where( m => m.Success == true )
                .SelectMany( o =>
                    o.Groups.Cast<Capture>()
                        .Select( c => c.Value ) );

            return captured.ToList();
        }

        /// <summary>
        /// Run RegEx replacing on a string.
        /// </summary>
        /// <param name="input">The lava source to process.</param>
        /// <param name="pattern">The regex pattern to use when matching.</param>
        /// <param name="replacement">The string to use when doing replacement.</param>
        /// <param name="options">The regex options to modify the matching.</param>
        /// <example><![CDATA[
        /// {{ 'The Rock is awesome.' | RegExReplace:'the rock','Rock','i' }}
        /// {{ 'Hello Ted, how are you?' | RegExReplace:'[Hh]ello (\w+)','Greetings $1' }}
        /// ]]></example>
        public static object RegExReplace( object input, object pattern, object replacement, string options = null )
        {
            RegexOptions regexOptions = RegexOptions.None;
            var inputString = input.ToString();

            options = options ?? string.Empty;

            if ( options.Contains( 'm' ) )
            {
                regexOptions |= RegexOptions.Multiline;
            }

            if ( options.Contains( 'i' ) )
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            return Regex.Replace( inputString, pattern.ToString(), replacement.ToString(), regexOptions );
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

            return input.Substring( start, length );
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
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date( object input, string format = null )
        {
            if ( input == null )
            {
                return null;
            }

            // If input is "now", use the current Rock date/time as the input value.
            if ( input.ToString().ToLower() == "now" )
            {
                // To correctly include the Rock configured timezone, we need to use a DateTimeOffset.
                // The DateTime object can only represent local server time or UTC time.
                input = new DateTimeOffset( RockDateTime.Now, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( RockDateTime.Now ) );
            }

            // Use the General Short Date/Long Time format by default.
            if ( string.IsNullOrWhiteSpace( format ) )
            {
                format = "G";
            }
            // Consider special 'Standard Date' and 'Standard Time' formats.
            else if ( format == "sd" )
            {
                format = "d";
            }
            else if ( format == "st" )
            {
                format = "t";
            }

            // If the format string is a single character, add a space to produce a valid custom format string.
            // (refer http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx#UsingSingleSpecifiers)
            else if ( format.Length == 1 )
            {
                format = " " + format;
            }

            string output;

            if ( input is DateTimeOffset inputDateTimeOffset )
            {
                // Preserve the value of the specified offset and return the formatted datetime value.
                output = inputDateTimeOffset.ToString( format ).Trim();
            }
            else if ( input is DateTime dt )
            {
                // Translate the DateTime to an offset
                if ( dt.Kind == DateTimeKind.Utc )
                {
                    // The input date is expressed in UTC, so convert it to a string expressed in Rock time.
                    var dtoUtc = new DateTimeOffset( dt, TimeSpan.Zero );
                    var dtoRock = TimeZoneInfo.ConvertTime( dtoUtc, RockDateTime.OrgTimeZoneInfo );

                    output = dtoRock.ToString( format ).Trim();
                }
                else
                {
                    // The input date kind is local or unspecified, so assume it is expressed in Rock time.
                    dt = DateTime.SpecifyKind( dt, DateTimeKind.Unspecified );
                    var rockDateTime = new DateTimeOffset( dt, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( dt ) );

                    output = rockDateTime.ToString( format ).Trim();
                }
            }
            else
            {
                // Convert the input to a valid Rock DateTime if possible.
                var outputDateTime = LavaDateTime.ParseToOffset( input.ToString() );

                if ( !outputDateTime.HasValue )
                {
                    // Not a valid date, so return the input unformatted.
                    return input.ToString().Trim();
                }

                output = LavaDateTime.ToString( outputDateTime.Value, format ).Trim();
            }

            return output;
        }

        /// <summary>
        /// Current datetime.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static DateTime? Date( string input )
        {
            if ( input != "Now" )
            {
                return null;
            }

            return RockDateTime.Now;
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
            }
            else
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
        public static List<DateTimeOffset> DatesFromICal( object input, object option = null )
        {
            return DatesFromICal( input, option, endDateTimeOption: null, startDateTime: null );
        }

        /// <summary>
        /// Returns the occurrence Dates from an iCal string or list, expressed in UTC.
        /// </summary>
        /// <param name="input">The input is either an iCal string or a list of iCal strings.</param>
        /// <param name="option">The quantity option (either an integer or "all").</param>
        /// <param name="endDateTimeOption">The 'enddatetime' option if supplied will return the ending datetime of the occurrence; otherwise the start datetime is returned.</param>
        /// <returns>a list of datetimes</returns>
        public static List<DateTimeOffset> DatesFromICal( object input, object option = null, object endDateTimeOption = null )
        {
            return DatesFromICal( input, option, endDateTimeOption, startDateTime: null );
        }

        /// <summary>
        /// Returns the occurrence Dates from an iCal string or list, expressed in UTC.
        /// </summary>
        /// <param name="input">The input is either an iCal string or a list of iCal strings.</param>
        /// <param name="option">The quantity option (either an integer or "all").</param>
        /// <param name="endDateTimeOption">The 'enddatetime' option if supplied will return the ending datetime of the occurrence; otherwise the start datetime is returned.</param>
        /// <param name="startDateTime">An optional date/time value that represents the start of the occurrence period.</param>
        /// <returns>a list of datetimes</returns>
        public static List<DateTimeOffset> DatesFromICal( object input, object option = null, object endDateTimeOption = null, object startDateTime = null )
        {
            return LavaFilters.DatesFromICal( input, option, endDateTimeOption, startDateTime );
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
            var integerAmount = amount.ToStringSafe().AsInteger();

            return DateAdd( input, integerAmount, interval );
        }

        /// <summary>
        /// Adds a time interval to a date
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static DateTimeOffset? DateAdd( object input, int amount, string interval = "d" )
        {
            var date = GetDateTimeOffsetFromInputParameter( input, null );

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
        /// Days from now.
        /// </summary>
        /// <param name="input">The input.</param>
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

            int daysDiff = ( dtInputDate.Value.Date - dtCompareDate.Date ).Days;

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

            var startDate = GetDateTimeOffsetFromInputParameter( sStartDate, null );
            var endDate = GetDateTimeOffsetFromInputParameter( sEndDate, null );

            if ( startDate != null && endDate != null )
            {
                TimeSpan difference = endDate.Value - startDate.Value;
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
            var startDate = GetDateTimeOffsetFromInputParameter( sStartDate, null );
            var endDate = GetDateTimeOffsetFromInputParameter( sEndDate, null );

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

            if ( startDate != null && endDate != null )
            {
                TimeSpan difference = endDate.Value - startDate.Value;

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

        /// <summary>
        /// Returns the difference between two datetime values in the specified units.
        /// </summary>
        /// <param name="sStartDate">The start date.</param>
        /// <param name="sEndDate">The end date.</param>
        /// <param name="unit">The unit of measurement.</param>
        /// <returns></returns>
        public static Int64? DateDiff( object sStartDate, object sEndDate, string unit )
        {
            return Rock.Lava.Filters.TemplateFilters.DateDiff( sStartDate, sEndDate, unit );
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
        public static DateTime? NextDayOfTheWeek( object input, string sDayOfWeek, object includeCurrentDay, object numberOfWeeks = null )
        {
            bool includeCurrent = includeCurrentDay.ToStringSafe().AsBoolean( false );
            int weeks = numberOfWeeks.ToStringSafe().AsIntegerOrNull() ?? 1;

            return NextDayOfTheWeek( input, sDayOfWeek, includeCurrent, weeks );
        }

        /// <summary>
        /// Advances the date to a specific day in the next 7 days.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="sDayOfWeek">The starting day of week.</param>
        /// <param name="includeCurrentDay">if set to <c>true</c> includes the current day as the current week.</param>
        /// <param name="numberOfWeeks">The number of weeks (must be non-zero).</param>
        /// <returns></returns>
        public static DateTime? NextDayOfTheWeek( object input, string sDayOfWeek, bool includeCurrentDay = false, int numberOfWeeks = 1 )
        {
            DateTime date;
            DayOfWeek dayOfWeek;

            if ( input == null )
            {
                return null;
            }

            // Check for invalid number of weeks
            if ( numberOfWeeks == 0 )
            {
                return null;
            }

            // Get the date value
            if ( input is DateTime )
            {
                date = (DateTime)input;
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
                daysUntilWeekDay = ( (int)dayOfWeek - (int)date.DayOfWeek + 7 ) % 7;
            }
            else
            {
                daysUntilWeekDay = ( ( ( (int)dayOfWeek - 1 ) - (int)date.DayOfWeek + 7 ) % 7 ) + 1;
            }

            // When a positive number of weeks is given, since the number of weeks defaults to 1
            // (which means the current week) we need to shift the numberOfWeeks down by 1 so
            // the calculation below is correct.
            if ( numberOfWeeks >= 1 )
            {
                numberOfWeeks--;
            }

            return date.AddDays( daysUntilWeekDay + ( numberOfWeeks * 7 ) );
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

            // We need to calculate the difference in actual calendar days, so we must compares dates converted to Rock time.
            var rockInputDate = LavaDateTime.ConvertToRockDateTime( inputDate.Value ).Date;

            var days = ( rockInputDate - RockDateTime.Now.Date ).Days;

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
        /// Formats the specified input as currency using the Currency Code information from Global Attributes
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
            if(inputAsDecimal != null )
            {
                // if the input an integer, decimal, double or anything else that can be parsed as a decimal, format that
                return inputAsDecimal.FormatAsCurrency();
            }

            // if the input is a string, just append the currency symbol to the front, even if it can't be converted to a number
            var currencyInfo = new RockCurrencyCodeInfo();
            if ( currencyInfo.SymbolLocation.Equals( "left", StringComparison.OrdinalIgnoreCase ) )
            {
                return string.Format( "{0}{1}", currencyInfo.Symbol, input );
            }

            return string.Format( "{1}{0}", currencyInfo.Symbol, input );
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
        public static object Attribute( Context context, object input, string attributeKey, string qualifier = "" )
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
            if ( ( input is string ) && input.ToStringSafe().Equals( "Global", StringComparison.OrdinalIgnoreCase ) )
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
                        var mergeFields = new Dictionary<string, object>();
                        if ( context.Environments.Count > 0 )
                        {
                            foreach ( var keyVal in context.Environments[0] )
                            {
                                mergeFields.Add( keyVal.Key, keyVal.Value );
                            }
                        }

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
            else if ( ( input is string ) && input.ToStringSafe().Equals( "SystemSetting", StringComparison.OrdinalIgnoreCase ) )
            {
                string theValue = Rock.Web.SystemSettings.GetValue( attributeKey );
                if ( theValue.IsLavaTemplate() )
                {
                    // SystemSetting attributes may reference other global attributes, so try to resolve this value again
                    var mergeFields = new Dictionary<string, object>();
                    if ( context.Environments.Count > 0 )
                    {
                        foreach ( var keyVal in context.Environments[0] )
                        {
                            mergeFields.Add( keyVal.Key, keyVal.Value );
                        }
                    }

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

                    // Check qualifer for "TextValue" and if true return PersistedTextValue
                    if (qualifier.Equals( "TextValue", StringComparison.OrdinalIgnoreCase ))
                    {
                        return item.AttributeValues[attributeKey].PersistedTextValue;
                    }

                    // Check qualifer for "HtmlValue" and if true return PersistedHtmlValue
                    if (qualifier.Equals( "HtmlValue", StringComparison.OrdinalIgnoreCase ))
                    {
                        return item.AttributeValues[attributeKey].PersistedHtmlValue;
                    }

                    // Check qualifer for "CondensedTextValue" and if true return PersistedTextValue
                    if (qualifier.Equals( "CondensedTextValue", StringComparison.OrdinalIgnoreCase ))
                    {
                        return item.AttributeValues[attributeKey].PersistedCondensedTextValue;
                    }

                    // Check qualifer for "CondensedHtmlValue" and if true return PersistedTextValue
                    if (qualifier.Equals( "CondensedHtmlValue", StringComparison.OrdinalIgnoreCase ))
                    {
                        return item.AttributeValues[attributeKey].PersistedCondensedHtmlValue;
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

                    if ( qualifier.Equals( "Object", StringComparison.OrdinalIgnoreCase ) && field is Rock.Field.ICachedEntitiesFieldType )
                    {
                        var cachedEntitiesField = (Rock.Field.ICachedEntitiesFieldType)field;
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
        public static object Property( Context context, object input, string propertyKey, string qualifier = "" )
        {
            if ( input != null )
            {
                if ( input is IDictionary<string, object> )
                {
                    var dictionaryObject = input as IDictionary<string, object>;
                    if ( dictionaryObject.ContainsKey( propertyKey ) )
                    {
                        return dictionaryObject[propertyKey];
                    }
                }

                return input.GetPropertyValue( propertyKey );
            }

            return string.Empty;
        }

        #endregion Attribute Filters

        #region Person Filters

        /// <summary>
        /// Sets the person preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void SetUserPreference( Context context, object input, string settingKey, string settingValue )
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
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

                preferences.SetValue( settingKey, settingValue );
                preferences.Save();
            }
        }

        /// <summary>
        /// Gets the person preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <returns></returns>
        public static string GetUserPreference( Context context, object input, string settingKey )
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
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

                return preferences.GetValue( settingKey );
            }

            return string.Empty;
        }

        /// <summary>
        /// Deletes the user preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        public static void DeleteUserPreference( Context context, object input, string settingKey )
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
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

                preferences.SetValue( settingKey, string.Empty );
                preferences.Save();
            }
        }

        /// <summary>
        /// Persons the by identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonById( Context context, object input )
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
        public static Person PersonByGuid( Context context, object input )
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
        public static Person PersonByAliasGuid( Context context, object input )
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
        public static Person PersonByAliasId( Context context, object input )
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
        public static Person PersonByPersonAlternateId( Context context, object input )
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
        public static string GetPersonAlternateId( Context context, object input )
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
        public static List<Person> Parents( Context context, object input )
        {
            var lavaContext = new RockLiquidRenderContext( context );
            return LavaFilters.Parents( lavaContext, input );
        }

        /// <summary>
        /// Gets the children of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Children( Context context, object input )
        {
            var lavaContext = new RockLiquidRenderContext( context );
            return LavaFilters.Children( lavaContext, input );
        }

        /// <summary>
        /// Gets an address for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static string Address( Context context, object input, string addressType, string qualifier = "" )
        {
            Person person = GetPerson( input );

            if ( person != null )
            {
                var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

                // Get all GroupMember records tied to this Person and the Family GroupType. Note that a given Person can belong to multiple families.
                var groupMemberQuery = new GroupMemberService( GetRockContext( context ) )
                    .Queryable( "GroupLocations.Location" )
                    .AsNoTracking()
                    .Where( m => m.PersonId == person.Id &&
                                 m.Group.GroupTypeId == familyGroupTypeId );

                /*
                    8/5/2020 - JH
                    If this Person has a primary family defined, use that to determine the Group whose Location should be used.
                    Otherwise, simply use the previous logic leveraging GroupMember.GroupOrder.

                    Reason: Change Lava 'Address' filter to use person's primary family property.
                */
                if ( person.PrimaryFamilyId.HasValue )
                {
                    groupMemberQuery = groupMemberQuery
                        .Where( m => m.GroupId == person.PrimaryFamilyId.Value );
                }

                // Get all GroupLocations tied to the GroupMembers queried up to this point.
                var groupLocationQuery = groupMemberQuery
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .SelectMany( m => m.Group.GroupLocations );

                switch ( addressType )
                {
                    case "Mailing":
                        groupLocationQuery = groupLocationQuery
                            .Where( gl => gl.IsMailingLocation == true );
                        break;
                    case "MapLocation":
                        groupLocationQuery = groupLocationQuery
                            .Where( gl => gl.IsMappedLocation == true );
                        break;
                    default:
                        groupLocationQuery = groupLocationQuery
                            .Where( gl => gl.GroupLocationTypeValue.Value == addressType );
                        break;
                }

                // Select the first GroupLocation's Location.
                Location location = groupLocationQuery
                    .Select( gl => gl.Location )
                    .FirstOrDefault();

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
                                case "Guid":
                                    qualifier = qualifier.Replace( match.ToString(), location.Guid.ToString() );
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
        public static Person Spouse( Context context, object input )
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
        public static Person HeadOfHousehold( Context context, object input )
        {
            Person person = GetPerson( input );

            if ( person == null )
            {
                return null;
            }
            return person.GetHeadOfHousehold();
        }

        /// <summary>
        /// Return's the FamilySalutation for the specified Person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="includeChildren">if set to <c>true</c> [include children].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="useFormalNames">if set to <c>true</c> [use formal names].</param>
        /// <param name="finalfinalSeparator">The final separator.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string FamilySalutation( Context context, object input, bool includeChildren = false, bool includeInactive = true, bool useFormalNames = false, string finalfinalSeparator = "&", string separator = "," )
        {
            Person person = GetPerson( input );

            if ( person == null )
            {
                return null;
            }

            string familySalutation = string.Empty;

            if ( includeInactive == false && useFormalNames == false && finalfinalSeparator == "&" && separator == "," && person.PrimaryFamilyId.HasValue )
            {
                // if default parameters are specified, we can get the family salutation from the GroupSalutationField of the person's PrimaryFamily
                if ( includeChildren )
                {
                    familySalutation = person.PrimaryFamily?.GroupSalutationFull;
                }
                else
                {
                    familySalutation = person.PrimaryFamily?.GroupSalutation;
                }
            }

            if ( familySalutation.IsNotNullOrWhiteSpace())
            {
                return familySalutation;
            }

            // if non-default parameters are specified, we'll have to calculate
            var args = new Person.CalculateFamilySalutationArgs( includeChildren )
            {
                IncludeInactive = includeInactive,
                UseFormalNames = useFormalNames,
                FinalSeparator = finalfinalSeparator,
                Separator = separator,
                RockContext = GetRockContext( context )
            };

            return Person.CalculateFamilySalutation( person, args );
        }

        /// <summary>
        /// Gets an number for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="phoneType">Type of the phone number.</param>
        /// <param name="countryCode">Whether or not there should be a country code returned</param>
        /// <returns></returns>
        public static string PhoneNumber( Context context, object input, string phoneType = "Home", bool countryCode = false )
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
        public static string ZebraPhoto( Context context, object input )
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
        public static string ZebraPhoto( Context context, object input, string size )
        {
            return ZebraPhoto( context, input, size, 1.0M, 1.0M, "LOGO", 0 );
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
        public static string ZebraPhoto( Context context, object input, string size, decimal brightness, decimal contrast )
        {
            return ZebraPhoto( context, input, size, brightness, contrast, "LOGO", 0 );
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
        public static string ZebraPhoto( Context context, object input, string size, decimal brightness, decimal contrast, string fileName )
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
        public static string ZebraPhoto( Context context, object input, string size, decimal brightness, decimal contrast, string fileName, int rotationDegree )
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
                    if ( brightness != 1.0M || contrast != 1.0M )
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
                                if ( j )
                                    line[x / 8] |= masks[x % 8];
                                var error = (sbyte)( data[x, y] - ( j ? 32 : -32 ) );
                                if ( x < resizedBitmap.Width - 1 )
                                    data[x + 1, y] += (sbyte)( 7 * error / 16 );
                                if ( y < resizedBitmap.Height - 1 )
                                {
                                    if ( x > 0 )
                                        data[x - 1, y + 1] += (sbyte)( 3 * error / 16 );
                                    data[x, y + 1] += (sbyte)( 5 * error / 16 );
                                    if ( x < resizedBitmap.Width - 1 )
                                        data[x + 1, y + 1] += (sbyte)( 1 * error / 16 );
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
        [RockObsolete( "1.13" )]
        [Obsolete( "Use ZebraPhoto( Context, object, string, decimal, decimal, string, int ) instead." )]
        public static string ZebraPhoto( Context context, object input, string size, double brightness, double contrast, string fileName, int rotationDegree )
        {
            return ZebraPhoto( context, input, size, ( decimal ) brightness, ( decimal ) contrast, fileName, rotationDegree );
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
        [RockObsolete( "1.13" )]
        [Obsolete( "Use ZebraPhoto( Context, object, string, decimal, decimal, string ) instead." )]
        public static string ZebraPhoto( Context context, object input, string size, double brightness, double contrast, string fileName )
        {
            return ZebraPhoto( context, input, size, ( decimal ) brightness, ( decimal ) contrast, fileName, 0 );
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
        [RockObsolete( "1.13" )]
        [Obsolete( "Use ZebraPhoto( Context, object, string, decimal, decimal ) instead." )]
        public static string ZebraPhoto( Context context, object input, string size, double brightness, double contrast )
        {
            return ZebraPhoto( context, input, size, ( decimal ) brightness, ( decimal ) contrast, "LOGO", 0 );
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
        public static List<Rock.Model.GroupMember> Groups( Context context, object input, string groupTypeId )
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
        public static List<Rock.Model.GroupMember> Groups( Context context, object input, string groupTypeId, string status )
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
        public static List<Rock.Model.GroupMember> Groups( Context context, object input, string groupTypeId, string memberStatus, string groupStatus )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                var groupQuery = new GroupMemberService( GetRockContext( context ) )
                    .Queryable( "Group, GroupRole" )
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == numericalGroupTypeId.Value &&
                        !m.Group.IsArchived );

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
        public static List<Rock.Model.GroupMember> Group( Context context, object input, string groupId, string status = "Active" )
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
        public static List<Rock.Model.Group> GroupsAttended( Context context, object input, string groupTypeId )
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
        public static Attendance LastAttendedGroupOfType( Context context, object input, string groupTypeId )
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
        public static List<Rock.Model.Group> GeofencingGroups( Context context, object input, string groupTypeId )
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
        public static List<Rock.Model.Person> GeofencingGroupMembers( Context context, object input, string groupTypeId, string groupTypeRoleId )
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
        /// Returns the nearest group of a specific type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Rock.Model.Group NearestGroup( Context context, object input, string groupTypeId )
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
        /// Returns the list of campuses closest to the target person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">A Person entity object or identifier.</param>
        /// <param name="maximumCount">The maximum number of campuses to return.</param>
        /// <returns>A single Campus, or a list of Campuses in ascending order of distance from the target Person.</returns>
        public static object NearestCampus( Context context, object input, object maximumCount = null )
        {
            // Call the newer Lava Filter implementation, and inject a null Lava context.
            return LavaFilters.NearestCampus( null, input, maximumCount );
        }

        /// <summary>
        /// Returns the Campus (or Campuses) that the Person belongs to
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public static object Campus( Context context, object input, object option = null )
        {
            // Call the newer Lava Filter implementation, and inject a null Lava context.
            // This is safe, because the Lava context is only used to retrieve the current data context,
            // and the fallback for that process creates a new data context instead.
            return LavaFilters.Campus( null, input, option );
        }

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static RockContext GetRockContext( Context context )
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
                    return new PersonService( new RockContext() ).Get( (int)input );
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
        public static object HasSignedDocument( Context context, object input, object documentTemplateId, object trueValue = null, object falseValue = null )
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
        public static string PersonActionIdentifier( Context context, object input, string action )
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
        public static string PersonTokenCreate( Context context, object input, int? expireMinutes = null, int? usageLimit = null, int? pageId = null )
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
        public static Person PersonTokenRead( Context context, object input, bool incrementUsage = false, int? pageId = null )
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

        /// <summary>
        /// Gets Steps associated with a specified person.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="stepProgram">The step program identifier, expressed as an Id or Guid.</param>
        /// <param name="stepStatus">The step status, expressed as an Id, Guid, or Name.</param>
        /// <param name="stepType">The step type identifier, expressed as an Id or Guid.</param>
        /// <returns></returns>
        public static List<Model.Step> Steps( object input, string stepProgram = "All", string stepStatus = "All", string stepType = "All" )
        {
            var person = GetPerson( input );

            if ( person == null )
            {
                return new List<Step>();
            }

            var rockContext = new RockContext();

            var stepsQuery = LavaFilters.GetPersonSteps( rockContext, person, stepProgram, stepStatus, stepType );

            return stepsQuery.ToList();
        }

        /// <summary>
        /// Determines whether [is in security role] [the specified context].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="roleId">The role Id.</param>
        /// <returns>
        ///   <c>true</c> if [is in security role] [the specified context]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInSecurityRole( Context context, object input, object roleId )
        {
            var lavaContext = new RockLiquidRenderContext( context );
            return LavaFilters.IsInSecurityRole( lavaContext, input, roleId );
        }

        #endregion Person Filters

        #region Personalize Filters

        /// <summary>
        /// Gets the set of personalization items that are relevant to the specified person.
        /// </summary>
        /// <param name="context">The DotLiquid context.</param>
        /// <param name="input">The filter input, a reference to a Person or a Person object.</param>
        /// <param name="itemTypeList">A comma-delimited list of item types to return.</param>
        /// <returns>The value of the user preference.</returns>
        public static List<PersonalizationItemInfo> PersonalizationItems( Context context, object input, string itemTypeList = "" )
        {
            // This filter implementation is only required for DotLiquid.
            // Create a compatible context and call the newer Lava Filter implementation.
            var lavaContext = new RockLiquidRenderContext( context );
            return LavaFilters.PersonalizationItems( lavaContext, input, itemTypeList );
        }

        /// <summary>
        /// Temporarily adds one or more personalization segments for the specified person.
        /// </summary>
        /// <remarks>
        /// If executed in the context of a HttpRequest, the result is stored in a session cookie and applies until the cookie expires.
        /// If no HttpRequest is active, the result is stored in the Lava context and applies only for the current render operation.
        /// </remarks>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, a reference to a Person or a Person object.</param>
        /// <param name="segmentKeyList">A comma-delimited list of segment keys to add.</param>
        public static void AddSegment( Context context, object input, string segmentKeyList )
        {
            var lavaContext = new RockLiquidRenderContext( context );
            LavaFilters.AddSegment( lavaContext, input, segmentKeyList );
        }

        #endregion

        #region Group Filters

        /// <summary>
        /// Loads a Group record from the database from it's GUID.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Rock.Model.Group GroupByGuid( Context context, object input )
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
        public static Rock.Model.Group GroupById( Context context, object input )
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
        public static string Debug( Context context, object input, string option1 = null, string option2 = null )
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

            var allFields = mergeFields.Union( context.Scopes.SelectMany( a => a ).DistinctBy( x => x.Key ).ToDictionary( k => k.Key, v => v.Value ) );

            // if a specific MergeField was specified as the Input, limit the help to just that MergeField
            if ( input != null && allFields.Any( a => a.Value == input ) )
            {
                mergeFields = allFields.Where( a => a.Value == input ).ToDictionary( k => k.Key, v => v.Value );
            }

            // TODO: implement the outputFormat option to support ASCII
            return mergeFields.lavaDebugInfo();
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
            if ( string.IsNullOrWhiteSpace(input) )
            {
                return string.Empty;
            }

            var page = HttpContext.Current?.Handler as RockPage;

            // Resolve theme references.
            if ( input.StartsWith( "~~" ) )
            {
                var theme = "Rock";
                if ( page != null )
                {
                    // Get the theme from the current page if we have one.
                    if ( page.Theme.IsNotNullOrWhiteSpace() )
                    {
                        theme = page.Theme;
                    }
                    else if ( page.Site != null && page.Site.Theme.IsNotNullOrWhiteSpace() )
                    {
                        theme = page.Site.Theme;
                    }
                }

                input = "~/Themes/" + theme + ( input.Length > 2 ? input.Substring( 2 ) : string.Empty );
            }

            // Resolve relative references.
            string url;
            if ( page != null )
            {
                url = page.ResolveUrl( input );
            }
            else
            {
                // In the absence of a HttpRequest, use the application root configuration setting as the base URL.
                var rootUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
                if ( string.IsNullOrWhiteSpace( rootUrl ) )
                {
                    rootUrl = "/";
                }

                if ( input.StartsWith( "~" ) )
                {
                    input = input.Trim( '~' );
                }

                var uri = new Uri( input, UriKind.RelativeOrAbsolute );
                if ( uri.IsAbsoluteUri )
                {
                    return uri.AbsoluteUri;
                }

                // Create an absolute Uri.
                uri = new Uri( new Uri( rootUrl ), uri );
                url = uri.AbsoluteUri;
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
        /// Returns a dynamic object from a JSON string.
        /// See https://www.rockrms.com/page/565#fromjson
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object FromJSON( object input )
        {
            var objectResult = ( input as string ).FromJsonDynamicOrNull();

            return objectResult;
        }

        /// <summary>
        /// Returns the dataset object from a <see cref="Rock.Model.PersistedDataset"/> specified by accessKey
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="accessKey">The access key.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static object PersistedDataset( Context context, string accessKey, string options = null )
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
        /// Appends Following information to entity/entities or a data object created from <see cref="PersistedDataset(Context, string, string)" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">The data object.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns></returns>
        public static object AppendFollowing( Context context, object dataObject, string purposeKey = null )
        {
            if ( dataObject == null )
            {
                return dataObject;
            }

            dynamic resultDataObject;

            // Determine if dataset is a collection or a single object
            bool isCollection;

            if ( dataObject is IEntity entity )
            {
                resultDataObject = new RockDynamic( dataObject );
                resultDataObject.EntityTypeId = EntityTypeCache.GetId( dataObject.GetType() );
                isCollection = false;
            }
            else if ( dataObject is IEnumerable<IEntity> entityList )
            {
                var firstEntity = entityList.FirstOrDefault();
                var dynamicEntityList = new List<RockDynamic>();
                if ( firstEntity != null )
                {
                    foreach ( var item in entityList )
                    {
                        dynamic rockDynamicItem = new RockDynamic( item );
                        rockDynamicItem.EntityTypeId = EntityTypeCache.GetId( item.GetType() );
                        dynamicEntityList.Add( rockDynamicItem );
                    }
                }

                resultDataObject = dynamicEntityList;

                isCollection = true;
            }
            else
            {
                // if the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset 
                resultDataObject = dataObject;

                // Note: Since a single ExpandoObject actually is an IEnumerable (of fields), we'll have to see if this is an IEnumerable of ExpandoObjects to see if we should treat it as a collection
                isCollection = resultDataObject is IEnumerable<ExpandoObject>;

                // if we are dealing with a persisted dataset, make a copy of the objects so we don't accidentally modify the cached object
                if ( isCollection )
                {
                    resultDataObject = ( resultDataObject as IEnumerable<ExpandoObject> ).Select( a => a.ShallowCopy() ).ToList();
                }
                else
                {
                    resultDataObject = ( resultDataObject as ExpandoObject )?.ShallowCopy() ?? resultDataObject;
                }
            }

            List<int> entityIdList;

            int? dataObjectEntityTypeId = null;

            if ( dataObject is IEntity dataObjectAsEntity )
            {
                dataObjectEntityTypeId = EntityTypeCache.GetId( dataObject.GetType() );
                entityIdList = new List<int>();
                entityIdList.Add( dataObjectAsEntity.Id );
            }
            else if ( dataObject is IEnumerable<IEntity> dataObjectAsEntityList )
            {
                var firstDataObject = dataObjectAsEntityList.FirstOrDefault();
                if ( firstDataObject != null )
                {
                    dataObjectEntityTypeId = EntityTypeCache.GetId( firstDataObject.GetType() );
                }

                entityIdList = dataObjectAsEntityList.Select( a => a.Id ).ToList();
            }
            else
            {
                // if the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset 
                if ( isCollection )
                {
                    IEnumerable<dynamic> dataObjectAsCollection = dataObject as IEnumerable<dynamic>;

                    entityIdList = dataObjectAsCollection
                            .Select( x => (int?)x.Id )
                            .Where( e => e.HasValue )
                            .Select( e => e.Value ).ToList();

                    // the dataObjects will each have the same EntityTypeId (assuming they are from a persisted dataset, so we can determine EntityTypeId from the first one
                    dataObjectEntityTypeId = dataObjectAsCollection.Select( a => (int?)a.EntityTypeId ).FirstOrDefault();
                }
                else
                {
                    int? entityId = (int?)resultDataObject.Id;
                    dataObjectEntityTypeId = (int?)resultDataObject.EntityTypeId;
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
                var rockContext = new RockContext();

                if( purposeKey.IsNotNullOrWhiteSpace() )
                {
                    // Get with purpose key
                    followedEntityIds = new FollowingService( rockContext ).GetFollowedItems( dataObjectEntityTypeId.Value, currentPerson.Id, purposeKey )
                    .Where( e => entityIdList.Contains( e.Id ) ).Select( a => a.Id ).ToList();
                }
                else
                {
                    // Get with out purpose key
                    followedEntityIds = new FollowingService( rockContext ).GetFollowedItems( dataObjectEntityTypeId.Value, currentPerson.Id )
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
                foreach ( dynamic result in (IEnumerable)resultDataObject )
                {
                    int? entityId = (int?)result.Id;

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
                int? entityId = (int?)resultDataObject.Id;

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
        /// Filters results to items that are being followed by the current person. Items can be  entity/entities or a data object created from <see cref="PersistedDataset(Context, string, string)"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">The data object.</param>
        /// <returns></returns>
        public static object FilterFollowed( Context context, object dataObject )
        {
            return FilterFollowedOrNotFollowed( context, GetCurrentPerson( context ), dataObject, FollowFilterType.Followed );
        }

        /// <summary>
        /// Filters results to items that are not being followed by the current person. Items can be  entity/entities or a data object created from <see cref="PersistedDataset(Context, string, string)"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">The data object.</param>
        /// <returns></returns>
        public static object FilterUnfollowed( Context context, object dataObject )
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
        private static object FilterFollowedOrNotFollowed( Context context, Person currentPerson, object dataObject, FollowFilterType followFilterType )
        {
            if ( dataObject == null )
            {
                return dataObject;
            }

            // if AppendFollowing wasn't already done on this, run it thru AppendFollowing so that all the objects are either ExpandoObject or RockDynamic
            if ( !( ( dataObject is IDynamicMetaObjectProvider ) || ( dataObject is IEnumerable<IDynamicMetaObjectProvider> ) ) )
            {
                dataObject = AppendFollowing( context, dataObject );
            }

            dynamic resultDataObject = dataObject;

            var isCollection = dataObject is IEnumerable<IDynamicMetaObjectProvider>;

            var resultDataObjectAsCollection = new List<IDynamicMetaObjectProvider>();

            // If requested only followed items filter
            if ( followFilterType == FollowFilterType.Followed )
            {
                if ( isCollection )
                {
                    foreach ( dynamic item in (IEnumerable)resultDataObject )
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
                if ( isCollection )
                {
                    foreach ( dynamic item in (IEnumerable)resultDataObject )
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
            RockPage page = HttpContext.Current?.Handler as RockPage;

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
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string SetPageTitle( string input )
        {
            return SetPageTitle( input, "All" );
        }

        /// <summary>
        /// adds a link tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the href of the tag.</param>
        /// <param name="titleLocation">The title location. "BrowserTitle", "PageTitle" or "All"</param>
        /// <returns></returns>
        public static string SetPageTitle( string input, string titleLocation )
        {
            RockPage page = HttpContext.Current?.Handler as RockPage;

            if ( page != null )
            {
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
        /// <param name="input">The input.</param>
        /// <param name="fingerprintLink">if set to <c>true</c> [fingerprint link].</param>
        /// <returns></returns>
        public static string AddScriptLink( string input, bool fingerprintLink = false )
        {
            var page = HttpContext.Current?.Handler as RockPage;
            if ( page != null )
            {
                RockPage.AddScriptLink( page, ResolveRockUrl( input ), fingerprintLink );
            }

            return string.Empty;
        }

        /// <summary>
        /// Adds the CSS link.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="fingerprintLink">if set to <c>true</c> [fingerprint link].</param>
        /// <returns></returns>
        public static string AddCssLink( string input, bool fingerprintLink = false )
        {
            var page = HttpContext.Current?.Handler as RockPage;
            if ( page != null )
            {
                RockPage.AddCSSLink( page, ResolveRockUrl( input ), fingerprintLink );
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
        /// Pages the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parm.</param>
        /// <returns></returns>
        public static object Page( string input, string parm )
        {
            RockPage page = HttpContext.Current?.Handler as RockPage;

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
                            return HttpContext.Current.Request.UrlProxySafe().AbsoluteUri;
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
                            return HttpContext.Current.Request.UrlProxySafe().AbsolutePath;
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
                            return HttpContext.Current.Request.UrlProxySafe().Scheme;
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
        /// Returns the specified page parm.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parm.</param>
        /// <returns></returns>
        public static object PageParameter( string input, string parm )
        {
            var page = HttpContext.Current?.Handler as RockPage;
            if ( page == null )
            {
                return null;
            }

            var parmReturn = page.PageParameter( parm );

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
        /// Sets a parameter in the input URL and returns a modified URL in the specified format.
        /// </summary>
        /// <param name="inputUrl">The input URL to be modified.</param>
        /// <param name="parameterName">The name of the URL parameter to modify.</param>
        /// <param name="parameterValue">The new value parameter value.</param>
        /// <param name="outputUrlFormat">The format of the output URL, specified as {"absolute"|"relative"}. If not specified, the default value is "absolute".</param>
        /// <returns></returns>
        public static string SetUrlParameter( object inputUrl, object parameterName, object parameterValue, object outputUrlFormat = null )
        {
            return LavaFilters.SetUrlParameter( inputUrl, parameterName, parameterValue, outputUrlFormat );
        }

        /// <summary>
        /// Sets a parameter in the input URL and returns a modified URL in the specified format.
        /// </summary>
        /// <param name="inputUrl">The input URL to be modified.</param>
        /// <param name="parameterName">The name of the URL parameter to modify.</param>
        /// <param name="parameterValue">The new value parameter value.</param>
        /// <returns></returns>
        public static string SetUrlParameter( object inputUrl, object parameterName = null, object parameterValue = null )
        {
            return LavaFilters.SetUrlParameter( inputUrl, parameterName, parameterValue, null );
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
            var offset = LavaDateTime.ParseToOffset( input.ToString() );

            if ( offset == null )
            {
                return null;
            }

            return offset;
        }

        /// <summary>
        /// Converts the input value to a DateTimeOffset value in Coordinated Universal Time (UTC).
        /// If the input value does not specify an offset, the current Rock time zone is assumed.
        /// </summary>
        /// <param name="input">The input value to be parsed into DateTime form.</param>
        /// <returns>A DateTimeOffset value with an offset of 0, or null if the conversion could not be performed.</returns>
        public static DateTimeOffset? AsDateTimeUtc( object input )
        {
            return LavaFilters.AsDateTimeUtc( input );
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
        public static string CreateShortLink( object input, string token = "", int? siteId = null, bool overwrite = false, int randomLength = 10 )
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
        /// Processes the Lava code in the source string.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input">The lava source to process.</param>
        /// <example><![CDATA[
        /// {% capture lava %}{% raw %}{% assign test = "hello" %}{{ test }}{% endraw %}{% endcapture %}
        /// {{ lava | RunLava }}
        /// ]]></example>
        public static string RunLava( Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            var template = LavaHelper.CreateDotLiquidTemplate( input.ToString() );

            //
            // Copy over any Registers, which often contain "internal" context information.
            //
            foreach ( var key in context.Registers.Keys )
            {
                template.Registers.Add( key, context.Registers[key] );
            }

            var mergeFields = context.Environments.SelectMany( a => a ).ToDictionary( k => k.Key, v => v.Value );

            System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();

            return template.Render( Hash.FromDictionary( mergeFields ) );
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
            if ( input.IsNullOrWhiteSpace() )
            {
                return;
            }

            var rockPage = HttpContext.Current?.Handler as RockPage;
            if ( rockPage == null )
            {
                return;
            }

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
                $"function addQuickReturnAjax(typeName, typeOrder, input) {{" + Environment.NewLine +
                $"  if (typeof Rock !== 'undefined' && typeof Rock.personalLinks !== 'undefined') {{" + Environment.NewLine +
                $"    Rock.personalLinks.addQuickReturn(typeName, typeOrder, input);" + Environment.NewLine +
                $"  }}" + Environment.NewLine +
                $"}};" + Environment.NewLine +
                $"addQuickReturnAjax('{typeName}', {typeOrder}, '{input}');";
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
        /// Filters a collection of items by applying the specified Linq predicate.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static object Where( object input, string filter )
        {
            if ( input is IEnumerable )
            {
                var enumerableInput = ( IEnumerable ) input;

                // The new System.Linq.Dynamic.Core only works on Queryables
                return enumerableInput.AsQueryable().Where( filter );
            }

            return null;
        }

        /// <summary>
        /// Filters a collection of items on a specified property and value with an equal comparison Type.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterKey">The filter key.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <returns></returns>
        [Obsolete("Use the override that specifies the comparisonType.")]
        [RockObsolete("1.13")]
        public static object Where( object input, string filterKey, object filterValue )
        {
            return Where( input, filterKey, filterValue, "equal" );
        }

        /// <summary>
        /// Filters a collection of items on a specified property and value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterKey">The filter key.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="comparisonType">The type of comparison for the filter value, either "equal" (default) or "notequal".</param>
        /// <returns></returns>
        public static object Where( object input, string filterKey, object filterValue, string comparisonType )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( ( IEnumerable )input ) )
                {
                    if ( value is ILiquidizable )
                    {
                        var liquidObject = value as ILiquidizable;
                        var condition = Condition.Operators["=="];

                        if ( liquidObject.ContainsKey( filterKey )
                                && ( ( condition( liquidObject[filterKey], filterValue ) && comparisonType == "equal" )
                                     || ( !condition( liquidObject[filterKey], filterValue ) && comparisonType == "notequal" ) ) )
                        {
                            result.Add( liquidObject );
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

            return input;
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
        /// <returns></returns>
        public static object SortByAttribute( Context context, object input, string attributeKey )
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
        public static object SortByAttribute( Context context, object input, string attributeKey, string sortOrder )
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

            // The new System.Linq.Dynamic.Core only works on Queryables
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
        public static List<Note> Notes( Context context, object input, object noteType, string sortOrder = "desc", int? count = null )
        {
            // Create a compatible context and call the newer Lava Filter implementation.
            var lavaContext = new RockLiquidRenderContext( context );
            return LavaFilters.Notes( lavaContext, input, noteType, sortOrder, count );
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
        public static bool HasRightsTo( Context context, object input, string verb, string typeName = "" )
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
        /// Determines whether a person is following an entity.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input entity to use for follow testing.</param>
        /// <param name="parameter1">The parameter1.</param>
        /// <param name="parameter2">The parameter2.</param>
        /// <returns><c>true</c> if the specified context is followed; otherwise, <c>false</c>.</returns>
        public static bool IsFollowed( Context context, object input, object parameter1 = null, object parameter2 = null )
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

            using ( var rockContext = new RockContext() )
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
        /// Gets a flag indicating if the input entity exists in the result set of the specified Data View.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, a reference to an Entity.</param>
        /// <param name="dataViewIdentifier">A reference to a Data View.</param>
        /// <returns><c>true</c> if the entity exists in the Data View result set.</returns>
        public static bool IsInDataView( Context context, object input, object dataViewIdentifier )
        {
            var lavaContext = new RockLiquidRenderContext( context );
            return LavaFilters.IsInDataView( lavaContext, input, dataViewIdentifier );
        }

        /// <summary>
        /// Translates a cached object into a fully database-backed entity object.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input cached object to be translated.</param>
        /// <returns>An <see cref="IEntity"/> object or the original <paramref name="input"/>.</returns>
        public static object EntityFromCachedObject( Context context, object input )
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
        private static Person GetCurrentPerson( Context context )
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
        /// <returns></returns>
        public static string Base64Encode( Context context, object input )
        {
            return Base64Encode( context, input, null );
        }

        /// <summary>
        /// Base64 encodes a binary file
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="resizeSettings">The resize settings.</param>
        /// <returns></returns>
        public static string Base64Encode( Context context, object input, string resizeSettings )
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
                if ( obj is Drop drop )
                {
                    obj = drop.InvokeDrop( properties.First() );
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

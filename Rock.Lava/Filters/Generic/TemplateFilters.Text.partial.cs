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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Humanizer;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Convert the input to a string value.
        /// </summary>
        /// <remarks>
        /// This filter exists for completeness, as an alias for the "ToString" filter.
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AsString( object input )
        {
            return input.ToStringSafe();
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

        #region Trim

        /// <summary>
        /// Trims the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="textToRemove"></param>
        /// <returns></returns>
        public static string Trim( object input, string textToRemove = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            // If text to remove is not specified, remove all whitespace.
            if ( textToRemove == null )
            {
                return input.ToString().Trim();
            }
            else
            {
                return TrimEnd( TrimStart( input, textToRemove ), textToRemove );
            }
        }

        /// <summary>
        /// Trims the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="textToRemove"></param>
        /// <returns></returns>
        public static string TrimStart( object input, string textToRemove = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            // If text to remove is not specified, remove whitespace.
            if ( textToRemove == null )
            {
                return input.ToString().TrimStart();
            }

            var inputString = input.ToString();
            while ( textToRemove != null && inputString.StartsWith( textToRemove ) )
            {
                inputString = inputString.Substring( textToRemove.Length );
            }
            return inputString;
        }

        /// <summary>
        /// Trims the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="textToRemove"></param>
        /// <returns></returns>
        public static string TrimEnd( object input, string textToRemove = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            // If text to remove is not specified, remove whitespace.
            if ( textToRemove == null )
            {
                return input.ToString().TrimEnd();
            }

            var inputString = input.ToString();
            while ( textToRemove != null && inputString.EndsWith( textToRemove ) )
            {
                inputString = inputString.Substring( 0, inputString.Length - textToRemove.Length );
            }
            return inputString;
        }

        #endregion

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
        /// Converts the input string from singular to plural.
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
        /// Calculates the approximate reading time for a given string.
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

            var wordsPerSecond = decimal.Divide( wordPerMinute, 60 );
            var wordsInString = inputString.WordCount();

            var readTimeInSeconds = decimal.Divide( wordsInString, wordsPerSecond );

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
            TimeSpan readTime = TimeSpan.FromSeconds( (int)readTimeInSeconds );

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
        /// Tests if the input string matches the provided regular expression.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expression">The regex expression.</param>
        /// <returns></returns>
        public static string RegExMatch( string input, string expression )
        {
            if ( input == null )
            {
                return false.ToString();
            }

            var regex = new Regex( expression );
            var match = regex.Match( input );

            return match.Success.ToString();
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
        /// <param name="options">Option flags that affect the match type. [m=multiline, i=ignore case]</param>
        /// <returns></returns>
        public static List<string> RegExMatchValues( string input, string expression, string options = null )
        {
            if ( input == null )
            {
                return null;
            }

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

            var regex = new Regex( expression, regexOptions );

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
        /// Searches the input string and replaces the last occurrence of the search string with the replacement string.
        /// </summary>
        /// <param name="source">The string.</param>
        /// <param name="find">The search parameter.</param>
        /// <param name="replace">The replacement parameter.</param>
        /// <returns></returns>
        public static string ReplaceLast( this string source, string find, string replace )
        {
            return source.ReplaceLastOccurrence( find, replace );
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
        /// singularize string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Singularize( string input )
        {
            return input == null
                ? input
                : input.Singularize( false );
        }

        /// <summary>
        /// Split the input string into an array of substrings separated by a given pattern.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="count">if specified, the maximum number of substrings to return. The remaining input text will be returned as the last entry in the collection.</param>
        /// <param name="removeEmpty">if set to <c>true</c>, empty values will be excluded from the results.</param>
        /// <returns></returns>
        public static List<string> Split( string input, string pattern, object removeEmpty = null, object count = null )
        {
            // If zero substrings have been requested, return an empty collection.
            var takeCount = count.ToIntSafe( -1 );
            if ( takeCount == 0 )
            {
                return new List<string>();
            }

            var shouldRemove = InputParser.ConvertToBooleanOrDefault( removeEmpty ) ?? true;
            var options = shouldRemove ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;

            List<string> parts;
            if ( takeCount != -1 )
            {
                parts = input.Split( new[] { pattern }, takeCount, options ).ToList();
            }
            else
            {
                parts = input.Split( new[] { pattern }, options ).ToList();
            }
            return parts;
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
        /// convert a integer to a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToString( object input )
        {
            return input.ToStringSafe();
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
        /// Returns both the input text and some additional specified text that is determined by whether the input text contains a value or not.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="successText">The success text.</param>
        /// <param name="fallbackText">The fallback text.</param>
        /// <param name="appendOrder">The append order.</param>
        /// <returns></returns>
        public static string WithFallback( object input, string successText, string fallbackText, string appendOrder = "append" )
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

    }
}

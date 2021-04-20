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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Humanizer;

using Rock.Common;

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
        /// Base64 encodes a binary file
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="resizeSettings">The resize settings.</param>
        /// <returns></returns>
        //public static string Base64Encode( object input, string resizeSettings )
        //{
        //    BinaryFile binaryFile = null;

        //    if ( input is int )
        //    {
        //        binaryFile = new BinaryFileService( new RockContext() ).Get( ( int ) input );
        //    }
        //    else if ( input is BinaryFile )
        //    {
        //        binaryFile = ( BinaryFile ) input;
        //    }

        //    if ( binaryFile != null )
        //    {
        //        using ( var stream = GetResized( resizeSettings, binaryFile.ContentStream ) )
        //        {
        //            byte[] imageBytes = stream.ReadBytesToEnd();
        //            return Convert.ToBase64String( imageBytes );
        //        }
        //    }

        //    return string.Empty;
        //}

        //private static Stream GetResized( string resizeSettings, Stream fileContent )
        //{
        //    try
        //    {
        //        if ( resizeSettings.IsNullOrWhiteSpace() )
        //        {
        //            return fileContent;
        //        }

        //        ResizeSettings settings = new ResizeSettings( HttpUtility.ParseQueryString( resizeSettings ) );
        //        MemoryStream resizedStream = new MemoryStream();

        //        ImageBuilder.Current.Build( fileContent, resizedStream, settings );
        //        return resizedStream;
        //    }
        //    catch
        //    {
        //        // if resize failed, just return original content
        //        return fileContent;
        //    }
        //}

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
        /// Removes all leading and trailing whitespace from the specified input string.
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
        /// Searches the input string and replaces the last occurrence of the search string with the replacement string.
        /// </summary>
        /// <param name="source">The string.</param>
        /// <param name="find">The search parameter.</param>
        /// <param name="replace">The replacement parameter.</param>
        /// <returns></returns>
        public static string ReplaceLast( this string source, string find, string replace )
        {
            return ExtensionMethods.ReplaceLastOccurrence( source, find, replace );
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

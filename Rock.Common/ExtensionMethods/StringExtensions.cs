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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Rock
{
    /// <summary>
    /// Handy string extensions that don't require any NuGet packages or Rock references
    /// </summary>
    public static class StringExtensions
    {
        #region String Extensions

        /// <summary>
        /// Prepends a character to a string if it doesn't already exist.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="prepend"></param>
        /// <returns></returns>
        public static string AddStringAtBeginningIfItDoesNotExist( this string text, string prepend )
        {
            if ( text == null )
                return prepend;
            if ( prepend == null )
                prepend = "";
            return text.StartsWith( prepend ) ? text : prepend + text;
        }

        /// <summary>
        /// Gets the nth occurrence of a string within a string. Pass 0 for the first occurrence, 1 for the second.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="nth"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int IndexOfNth( this string str, string value, int nth = 0, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase )
        {
            if ( nth < 0 )
                throw new ArgumentException( "Can not find a negative index of substring in string. Must start with 0" );

            int offset = str.IndexOf( value, comparisonType );
            for ( int i = 0; i < nth; i++ )
            {
                if ( offset == -1 )
                    return -1;
                offset = str.IndexOf( value, offset + 1, comparisonType );
            }

            return offset;
        }

        /// <summary>
        /// Converts string to MD5 hash
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string Md5Hash( this string str )
        {
            using ( var crypt = MD5.Create() )
            {
                var hash = crypt.ComputeHash( Encoding.UTF8.GetBytes( str ) );

                StringBuilder sb = new StringBuilder();
                foreach ( byte b in hash )
                {
                    // Can be "x2" if you want lowercase
                    sb.Append( b.ToString( "x2" ) );
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts string to Sha1 hash
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string Sha1Hash( this string str )
        {
            using ( var crypt = new SHA1Managed() )
            {
                var hash = crypt.ComputeHash( Encoding.UTF8.GetBytes( str ) );
                var sb = new StringBuilder( hash.Length * 2 );

                foreach ( byte b in hash )
                {
                    // Can be "x2" if you want lowercase
                    sb.Append( b.ToString( "x2" ) );
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts string to Sha256 hash
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string Sha256Hash( this string str )
        {
            using ( var crypt = new System.Security.Cryptography.SHA256Managed() )
            {
                var hash = crypt.ComputeHash( Encoding.UTF8.GetBytes( str ) );
                var sb = new StringBuilder();

                foreach ( byte b in hash )
                {
                    // Can be "x2" if you want lowercase
                    sb.Append( b.ToString( "x2" ) );
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts string to HMAC_SHA1 string using key
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="keyString">The key.</param>
        /// <returns></returns>
        public static string HmacSha1Hash( this string str, string keyString )
        {
            var key = Encoding.ASCII.GetBytes( keyString );

            using ( var crypt = new HMACSHA1( key ) )
            {
                var hash = crypt.ComputeHash( Encoding.ASCII.GetBytes( str ) );

                // Can be "x2" if you want lowercase
                return hash.Aggregate( "", ( s, e ) => s + String.Format( "{0:x2}", e ), s => s );
            }
        }

        /// <summary>
        /// Converts string to HMAC_SHA256 string using key
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="keyString">The key string.</param>
        /// <returns></returns>
        public static string HmacSha256Hash( this string str, string keyString )
        {
            var key = Encoding.ASCII.GetBytes( keyString );

            using ( var crypt = new HMACSHA256( key ) )
            {
                var hash = crypt.ComputeHash( Encoding.ASCII.GetBytes( str ) );

                // Can be "x2" if you want lowercase
                return hash.Aggregate( "", ( s, e ) => s + String.Format( "{0:x2}", e ), s => s );
            }
        }

        /// <summary>
        /// Reads the parameter to check for DOM objects and possible URLs
        /// Accepts an encoded string and returns an encoded string
        /// </summary>
        /// <param name="encodedString"></param>
        [RockObsolete( "1.16.1" )]
        [Obsolete( "This method is no longer suitable. Consider using RedirectUrlContainsXss." )]
        public static string ScrubEncodedStringForXSSObjects( this string encodedString )
        {
            var decodedString = encodedString.GetFullyUrlDecodedValue();

            if ( decodedString.HasXssObjects() )
            {
                return "%2f";
            }

            return encodedString;
        }

        /// <summary>
        /// Determines whether <paramref name="decodedString"/> has XSS objects.
        /// </summary>
        /// <param name="decodedString">The decoded string.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="decodedString"/> has XSS objects; otherwise, <c>false</c>.
        /// </returns>
        [RockObsolete( "1.16.1" )]
        [Obsolete( "This method is no longer suitable. Consider using RedirectUrlContainsXss." )]
        public static bool HasXssObjects( this string decodedString )
        {
            // Characters used by DOM Objects; javascript, document, window and URLs
            char[] badCharacters = new char[] { '<', '>', ':', '*' };

            if ( decodedString?.IndexOfAny( badCharacters ) >= 0 )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks a value intended to be a redirect URL for XSS injection methods.
        /// </summary>
        /// <param name="redirectUrl">The value intended to be used for a redirect URL.</param>
        /// <returns>
        ///   <c>true</c> if [redirectUrl contains XSS injection methods]; otherwise, <c>false</c>.
        /// </returns>
        public static bool RedirectUrlContainsXss( this string redirectUrl )
        {
            if ( string.IsNullOrWhiteSpace( redirectUrl ) )
            {
                return false; // Early exit, nothing to check.
            }

            // These are characters which we will not ever allow in a URL query string value.  We used to block colon (:), but that
            // has a valid use in DateTime values.
            char[] badCharacters = new char[] { '<', '>', '*' };

            // Ensure we URL and HTML decode all characters, even if they are double/triple/etc encoded.
            var decodedString = redirectUrl.GetFullyUrlDecodedValue().GetFullyHtmlDecodedValue();

            // Check for bad characters.
            if ( decodedString.IndexOfAny( badCharacters ) >= 0 )
            {
                return true;
            }

            // Some special whitespace characters are ignored by the browser when parsing script.  Remove all whitespace from the
            // string and make it lower case for easier comparison.
            decodedString = Regex.Replace( decodedString, @"\s+", "" ).ToLower();

            // These are strings that should never be allowed in a URL query string value.
            string[] blockedStrings = new string[] { "javascript:" };

            // Check for bad strings.
            foreach ( string blockedString in blockedStrings )
            {
                if ( decodedString.Contains( blockedString ) )
                {
                    return true;
                }
            }

            // Everything looks okay.
            return false;
        }

        /// <summary>
        /// Gets a fully URL-decoded string (or returns string.Empty if it cannot be decoded within 10 attempts).
        /// </summary>
        /// <param name="encodedString">The encoded string.</param>
        /// <returns></returns>
        public static string GetFullyUrlDecodedValue( this string encodedString )
        {
            if ( string.IsNullOrWhiteSpace( encodedString) )
            {
                return encodedString;
            }

            int loopCount = 0;
            var decodedString = encodedString;
            var testString = WebUtility.UrlDecode( encodedString );
            while ( testString != decodedString )
            {
                loopCount++;
                if ( loopCount >= 10 )
                {
                    return string.Empty;
                }

                decodedString = testString;
                testString = WebUtility.UrlDecode( testString );
            }

            return decodedString;
        }

        /// <summary>
        /// Gets a fully HTML-decoded string (or returns string.Empty if it cannot be decoded within 10 attempts).
        /// </summary>
        /// <param name="encodedString">The encoded string.</param>
        /// <returns></returns>
        public static string GetFullyHtmlDecodedValue( this string encodedString )
        {
            if ( string.IsNullOrWhiteSpace( encodedString ) )
            {
                return encodedString;
            }

            int loopCount = 0;
            var decodedString = encodedString;
            var testString = HtmlDecodeCharactersWithoutSeparators( encodedString );
            while ( testString != decodedString )
            {
                loopCount++;
                if ( loopCount >= 10 )
                {
                    return string.Empty;
                }

                decodedString = testString;
                testString = HtmlDecodeCharactersWithoutSeparators( testString );
            }

            return decodedString;
        }

        /// <summary>
        /// This method inserts missing semi-colon separators into HTML-encoded character strings that use hex or decimal character
        /// references before HTML decoding them. Browsers will render these strings without the separators, but
        /// <see cref="WebUtility.HtmlDecode"/> will not.
        /// </summary>
        /// <param name="encodedString">The encoded string.</param>
        /// <returns></returns>
        private static string HtmlDecodeCharactersWithoutSeparators( this string encodedString )
        {
            if ( string.IsNullOrWhiteSpace( encodedString ) )
            {
                return encodedString;
            }

            // Hex encoded HTML characters should follow the format "&#0000;" (for decimal) or &#x0000; (for hex).  If we don't have
            // an ampersand together with a pound symbol, we can just use the base method.
            if ( !encodedString.Contains( "&#" ) )
            {
                return WebUtility.HtmlDecode( encodedString );
            }

            // This loop will shift each segment of the string from encodedString to correctedEncodedString, adding any missing
            // semi-colons after each decimal or hex encoded character.
            var correctedEncodedString = string.Empty;
            while ( encodedString.Contains( "&#" ) )
            {
                var elementBoundary = encodedString.IndexOf( "&#" );

                // Start by putting everything before the next &# into the corrected string and removing it from the original.
                correctedEncodedString += encodedString.Substring( 0, elementBoundary ) + "&#";
                elementBoundary += 2;
                encodedString = encodedString.Substring( elementBoundary, encodedString.Length - elementBoundary );

                // If the next character in the string is an "x", move it to the corrected string.
                var nextChar = encodedString.FirstOrDefault();
                bool useHex = false;
                if ( nextChar == 'x' )
                {
                    useHex = true;
                    correctedEncodedString += nextChar;
                    encodedString = encodedString.Substring( 1, encodedString.Length - 1 );
                    nextChar = encodedString.FirstOrDefault();
                }

                // Keep moving any digits to the corrected string.  There's technically no limit on padding zeros.
                var isDigit = useHex ? nextChar.IsHexDigit() : char.IsDigit( nextChar );
                while ( isDigit )
                {
                    correctedEncodedString += nextChar;
                    encodedString = encodedString.Substring( 1, encodedString.Length - 1 );
                    nextChar = encodedString.FirstOrDefault();
                    isDigit = useHex ? nextChar.IsHexDigit() : char.IsDigit( nextChar );
                }

                // Add missing semi-colons.
                if ( nextChar != ';' )
                {
                    correctedEncodedString += ";";
                }
            }

            // Re-append any trailing characters left over in the original string.
            correctedEncodedString += encodedString;

            return WebUtility.HtmlDecode( correctedEncodedString );
        }

        /// <summary>
        /// Checks a digit to see if it's a valid hexadecimal character (0-9 or A-F).  Permits lowercase.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns></returns>
        private static bool IsHexDigit( this char c )
        {
            char[] nonNumericHexCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f' };
            return char.IsDigit( c ) || nonNumericHexCharacters.Contains( c );
        }

        /// <summary>
        /// Joins and array of strings using the provided separator.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Concatenated string.</returns>
        public static string JoinStrings( this IEnumerable<string> source, string separator )
        {
            return string.Join( separator, source.ToArray() );
        }

        /// <summary>
        /// Joins an array of English strings together with commas plus "and" for last element.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Concatenated string.</returns>
        public static string JoinStringsWithCommaAnd( this IEnumerable<String> source )
        {
            if ( source == null || source.Count() == 0 )
            {
                return string.Empty;
            }

            var output = string.Empty;

            var list = source.ToList();

            if ( list.Count > 1 )
            {
                var delimited = string.Join( ", ", list.Take( list.Count - 1 ) );

                output = string.Concat( delimited, " and ", list.LastOrDefault() );
            }
            else
            {
                // only one element, just use it
                output = list[0];
            }

            return output;
        }

        /// <summary>
        /// Joins an array of English strings together with a chosen delimiter, plus a final delimiter for last element, with a maximum length of results and truncation value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="repeatDelimiter">The delimiter for all but the final string.</param>
        /// <param name="finalDelimiter">The delimiter for only the final string.</param>
        /// <param name="maxLength">The maximum length allowed for the concatenated string from the source (not including the <paramref name="truncation"/> string).</param>
        /// <param name="truncation">The truncation string value (default is "..." for ellipsis).</param>
        /// <returns>Concatenated string.</returns>
        public static string JoinStringsWithRepeatAndFinalDelimiterWithMaxLength( this IEnumerable<String> source, string repeatDelimiter, string finalDelimiter, int? maxLength, string truncation = "..." )
        {
            if ( source == null || source.Count() == 0 )
            {
                return string.Empty;
            }

            var output = string.Empty;

            var list = source.ToList();

            if ( list.Count > 1 )
            {
                var delimited = string.Join( repeatDelimiter, list.Take( list.Count - 1 ) );

                output = string.Concat( delimited, finalDelimiter, list.LastOrDefault() );
            }
            else
            {
                // only one element, just use it.
                output = list[0];
            }

            if ( maxLength.HasValue && output.Length > maxLength.Value )
            {
                output = output.Substring( 0, maxLength.Value ) + truncation;
            }

            return output;
        }

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
        /// Replaces the special characters from the string with the supplied string so that only alpha-numeric, '.', and '_' remain.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="replacementCharacters">The characters to replace special character(s) with. No restrictions or validation.</param>
        /// <returns></returns>
        public static string ReplaceSpecialCharacters( this string str, string replacementCharacters )
        {
            StringBuilder sb = new StringBuilder();
            foreach ( char c in str )
            {
                if ( ( c >= '0' && c <= '9' ) || ( c >= 'A' && c <= 'Z' ) || ( c >= 'a' && c <= 'z' ) || c == '.' || c == '_' )
                {
                    sb.Append( c );
                }
                else
                {
                    sb.Append( replacementCharacters );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes all non alpha numeric characters from a string
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string RemoveAllNonAlphaNumericCharacters( this string str )
        {
            return string.Concat( str.Where( c => char.IsLetterOrDigit( c ) ) );
        }

        /// <summary>
        /// Removes all non numeric characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string RemoveAllNonNumericCharacters( this string str )
        {
            Regex digitsOnly = new Regex( @"[^\d]" );

            if ( !string.IsNullOrEmpty( str ) )
            {
                return digitsOnly.Replace( str, string.Empty );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Determines whether the string is not null or whitespace.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static bool IsNotNullOrWhiteSpace( this string str )
        {
            return !string.IsNullOrWhiteSpace( str );
        }

        /// <summary>
        /// Determines whether [is null or white space].
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static bool IsNullOrWhiteSpace( this string str )
        {
            return string.IsNullOrWhiteSpace( str );
        }

        /// <summary>
        /// Returns the right most part of a string of the given length.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string Right( this string str, int length )
        {
            return str.SubstringSafe( str.Length - length );
        }

        /// <summary>
        /// Strips HTML from the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string StripHtml( this string str )
        {
            return str.IsNullOrWhiteSpace()
                ? str
                : Regex.Replace( str, @"<.*?>|<!--(.|\r|\n)*?-->", string.Empty );
        }

        /// <summary>
        /// Determines whether the string is made up of only digits
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool IsDigitsOnly( this string str )
        {
            foreach ( char c in str )
            {
                if ( c < '0' || c > '9' )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the number of words in the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int WordCount( this string str )
        {
            // Attribution (aka future blame): https://stackoverflow.com/questions/8784517/counting-number-of-words-in-c-sharp
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            return str.Split( delimiters, StringSplitOptions.RemoveEmptyEntries ).Length;
        }

        /// <summary>
        /// Determines whether the string is valid mac address.
        /// Works with colons, dashes, or no seperators
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if valid mac address otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidMacAddress( this string str )
        {
            Regex regex = new Regex( "^([0-9a-fA-F]{2}(?:[:-]?[0-9a-fA-F]{2}){5})$" );
            return regex.IsMatch( str );
        }

        /// <summary>
        /// Determines whether the string is a valid http(s) URL
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if [is valid URL] [the specified string]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidUrl( this string str )
        {
            Uri uriResult;
            return Uri.TryCreate( str, UriKind.Absolute, out uriResult )
                && ( uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps );
        }

        /// <summary>
        /// Removes invalid, reserved, and unreccommended characters from strings that will be used in URLs.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string RemoveInvalidReservedUrlChars( this string str )
        {
            return str.Replace( " ", string.Empty )
                .Replace( ";", string.Empty )
                .Replace( "/", string.Empty )
                .Replace( "?", string.Empty )
                .Replace( ":", string.Empty )
                .Replace( "@", string.Empty )
                .Replace( "=", string.Empty )
                .Replace( "&", string.Empty )
                .Replace( "<", string.Empty )
                .Replace( ">", string.Empty )
                .Replace( "#", string.Empty )
                .Replace( "%", string.Empty )
                .Replace( "\"", string.Empty )
                .Replace( "{", string.Empty )
                .Replace( "}", string.Empty )
                .Replace( "|", string.Empty )
                .Replace( "\\", string.Empty )
                .Replace( "^", string.Empty )
                .Replace( "[", string.Empty )
                .Replace( "]", string.Empty )
                .Replace( "`", string.Empty )
                .Replace( "'", string.Empty );
        }

        /// <summary>
        /// Converts a comma delimited string into a List&lt;int&gt;
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static IEnumerable<int> StringToIntList( this string str )
        {
            // https://stackoverflow.com/questions/1763613/convert-comma-separated-string-of-ints-to-int-array
            if ( String.IsNullOrEmpty( str ) )
            {
                yield break;
            }

            foreach ( var s in str.Split( ',' ) )
            {
                int num;
                if ( int.TryParse( s, out num ) )
                {
                    yield return num;
                }
            }
        }

        /// <summary>
        /// Makes the Int64 hash code from the provided string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static long MakeInt64HashCode( this string str )
        {
            // http://www.codeproject.com/Articles/34309/Convert-String-to-64bit-Integer
            long hashCode = 0;
            if ( !string.IsNullOrEmpty( str ) )
            {
                // Unicode Encode Covering all characterset
                byte[] byteContents = Encoding.Unicode.GetBytes( str );
                System.Security.Cryptography.SHA256 hash =
                new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash( byteContents );

                long hashCodeStart = BitConverter.ToInt64( hashText, 0 );
                long hashCodeMedium = BitConverter.ToInt64( hashText, 8 );
                long hashCodeEnd = BitConverter.ToInt64( hashText, 24 );
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }

            return hashCode;
        }

        /// <summary>
        /// removes any invalid FileName chars in a filename
        /// from http://stackoverflow.com/a/14836763/1755417
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string MakeValidFileName( this string name )
        {
            string invalidChars = Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
            string invalidReStr = string.Format( @"[{0}]+", invalidChars );
            string replace = Regex.Replace( name, invalidReStr, "_" ).Replace( ";", string.Empty ).Replace( ",", string.Empty );
            return replace;
        }

        /// <summary>
        /// Splits a Camel or Pascal cased identifier into separate words.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string SplitCase( this string str )
        {
            if ( str == null )
            {
                return null;
            }

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
            {
                return new string[0];
            }

            string regex = whitespace ? @"[\s\|,;]+" : @"[\|,;]+";

            char[] delimiter = new char[] { ',' };
            return Regex.Replace( str, regex, "," ).Split( delimiter, StringSplitOptions.RemoveEmptyEntries );
        }

        /// <summary>
        /// Returns an array that contains substrings of the target string that are separated by the specified delimiter.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="delimiter">The delimiter string.</param>
        /// <returns></returns>
        public static string[] SplitDelimitedValues( this string str, string delimiter )
        {
            return SplitDelimitedValues( str, delimiter, StringSplitOptions.None );
        }

        /// <summary>
        /// Returns an array that contains substrings of the target string that are separated by the specified delimiter.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="delimiter">The delimiter string.</param>
        /// <param name="options">The split options.</param>
        /// <returns></returns>
        public static string[] SplitDelimitedValues( this string str, string delimiter, StringSplitOptions options )
        {
            if ( str == null )
            {
                return new string[0];
            }

            // Replace the custom delimiter string with a single unprintable character that will not appear in the target string, then use the default string split function.
            var newDelimiter = new char[] { '\x0001' };

            var replaceString = str.Replace( delimiter, new string( newDelimiter ) )
                                   .Split( newDelimiter, options );

            return replaceString;
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
            {
                return null;
            }

            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = str.ToUpper();
            string upperPattern = oldValue.ToUpper();
            int inc = ( str.Length / oldValue.Length ) *
                      ( newValue.Length - oldValue.Length );
            char[] chars = new char[str.Length + Math.Max( 0, inc )];
            while ( ( position1 = upperString.IndexOf( upperPattern, position0 ) ) != -1 )
            {
                for ( int i = position0; i < position1; ++i )
                {
                    chars[count++] = str[i];
                }

                for ( int i = 0; i < newValue.Length; ++i )
                {
                    chars[count++] = newValue[i];
                }

                position0 = position1 + oldValue.Length;
            }

            if ( position0 == 0 )
            {
                return str;
            }

            for ( int i = position0; i < str.Length; ++i )
            {
                chars[count++] = str[i];
            }

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
            {
                return null;
            }

            return str.Replace( "'", "\\'" ).Replace( "\"", "\\\"" );
        }

        /// <summary>
        /// Standardize quotes in a string. It replaces curly single quotes into the standard single quote character (ASCII 39).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StandardizeQuotes( this string str )
        {
            if ( str == null )
            {
                return null;
            }

            return str.Replace( "’", "'" );
        }

        /// <summary>
        /// Adds Quotes around the specified string and escapes any quotes that are already in the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="quoteChar">The quote character.</param>
        /// <returns></returns>
        public static string Quoted( this string str, string quoteChar = "'" )
        {
            var result = quoteChar + str.EscapeQuotes() + quoteChar;
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
            return str.SubstringSafe( 0, length );
        }

        /// <summary>
        /// Truncates from char 0 to the length and then add an ellipsis character char 8230.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string LeftWithEllipsis( this string str, int length )
        {
            if ( str.Length <= length )
            {
                return str;
            }

            return Left( str, length ) + ( char ) 8230;
        }

        /// <summary>
        /// Returns a substring of a string. Uses an empty string for any part that doesn't exist and will return a partial substring if the string isn't long enough for the requested length (the built-in method would throw an exception in these cases).
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The 0-based starting position.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        [Obsolete( "Use SubstringSafe() instead. Obsolete as of 1.12.0" )]
        [RockObsolete( "1.12" )]
        public static string SafeSubstring( this string str, int startIndex, int maxLength )
        {
            return str.SubstringSafe( startIndex, maxLength );
        }

        /// <summary>
        /// Returns a substring of a string. Uses an empty string for any part that doesn't exist and will return a partial substring if the string isn't long enough for the requested length (the built-in method would throw an exception in these cases).
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The 0-based starting position.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        public static string SubstringSafe( this string str, int startIndex, int maxLength )
        {
            if ( str == null || maxLength < 0 || startIndex < 0 || startIndex > str.Length )
            {
                return string.Empty;
            }

            return str.Substring( startIndex, Math.Min( maxLength, str.Length - startIndex ) );
        }

        /// <summary>
        /// Returns a substring of a string. Uses an empty string for any part that doesn't exist and will return a partial substring if the string isn't long enough for the requested length (the built-in method would throw an exception in these cases).
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The 0-based starting position.</param>
        /// <returns></returns>
        public static string SubstringSafe( this string str, int startIndex )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            return str.SubstringSafe( startIndex, Math.Max( str.Length - startIndex, 0 ) );
        }

        /// <summary>
        /// Truncates a string after a max length and adds ellipsis.  Truncation will occur at first space prior to maxLength.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate( this string str, int maxLength )
        {
            return Truncate( str, maxLength, true );
        }

        /// <summary>
        /// Truncates a string after a max length with an option to add an ellipsis at the end.  Truncation will occur at first space prior to maxLength.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="maxLength">The maximum length of the return value, including the ellipsis if added.</param>
        /// <param name="addEllipsis">if set to <c>true</c> add an ellipsis to the end of the truncated string.</param>
        /// <returns></returns>
        public static string Truncate( this string str, int maxLength, bool addEllipsis )
        {
            if ( str == null )
            {
                return null;
            }

            if ( str.Length <= maxLength )
            {
                return str;
            }

            // Since we include the ellipsis in the number of max characters
            // we need to disable ellipsis if they told us to have a max
            // length of 3 or less - which is the number of periods that
            // would be added.
            if ( maxLength <= 3 )
            {
                addEllipsis = false;
            }

            // If adding an ellipsis then reduce the maxlength by three to allow for the additional characters
            maxLength = addEllipsis ? maxLength - 3 : maxLength;

            var truncatedString = str.Substring( 0, maxLength );
            var lastSpace = truncatedString.LastIndexOf( ' ' );
            if ( lastSpace > 0 )
            {
                truncatedString = truncatedString.Substring( 0, lastSpace );
            }

            return addEllipsis ? truncatedString + "..." : truncatedString;
        }

        /// <summary>
        /// Removes any non-numeric characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AsNumeric( this string str )
        {
            return Regex.Replace( str, @"[^0-9]", string.Empty );
        }

        /// <summary>
        /// Replaces the last occurrence of a given string with a new value
        /// </summary>
        /// <param name="source">The string.</param>
        /// <param name="find">The search parameter.</param>
        /// <param name="replace">The replacement parameter.</param>
        /// <returns></returns>
        public static string ReplaceLastOccurrence( this string source, string find, string replace )
        {
            int place = source.LastIndexOf( find );
            return place > 0 ? source.Remove( place, find.Length ).Insert( place, replace ) : source;
        }

        /// <summary>
        /// Replaces the first occurrence of a given string with a new value
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="find">The find.</param>
        /// <param name="replace">The replace.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceFirstOccurrence( this string source, string find, string replace )
        {
            var regex = new Regex( Regex.Escape( find ) );
            return regex.Replace( source, replace, 1 );
        }

        /// <summary>
        /// Replaces string found at the very end of the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public static string ReplaceIfEndsWith( this string content, string suffix, string replacement )
        {
            if ( content.EndsWith( suffix ) )
            {
                return content.Substring( 0, content.Length - suffix.Length ) + replacement;
            }
            else
            {
                return content;
            }
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
        [System.Diagnostics.DebuggerStepThrough]
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
        /// Attempts to convert a "MM/dd", "M/dd", "M/d" or "MM/d" string to a datetime, with the year as the current year. Returns null if unsuccessful.
        /// </summary>
        /// <param name="monthDayString">The month day string.</param>
        /// <returns></returns>
        public static DateTime? MonthDayStringAsDateTime( this string monthDayString )
        {
            if ( !string.IsNullOrEmpty( monthDayString ) )
            {
                if ( monthDayString.Length <= 5 )
                {
                    if ( monthDayString.Contains( '/' ) )
                    {
                        DateTime value;
                        var monthDayYearString = $"{monthDayString}/{RockDateTime.Today.Year}";
                        if ( DateTime.TryParseExact(
                                monthDayYearString,
                                new[] { "MM/dd/yyyy", "M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy" },
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AllowWhiteSpaces,
                                out value ) )
                        {
                            return value;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to convert string to DateTime.  Returns null if unsuccessful.
        /// NOTE: If this is a '#[#]/#[#]' string it will be interpreted as a "MM/dd", "M/dd", "M/d" or "MM/d" string and will resolve to a datetime with the year as the current year.
        /// However, in those cases, it would be better to use <seealso cref="MonthDayStringAsDateTime(string)"/>
        /// Non-ASCI and control characters are stripped from the string to prevent invisible control characters from causing a null to return.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static DateTime? AsDateTime( this string str )
        {
            if ( str == null )
            {
                return null;
            }

            // Edge likes to put in 8206 when doing a toLocaleString(), which makes this method return null.
            // This will correct the error and any other caused by non-ASCI & control characters.
            str = new string( str.Where( c => c > 31 && c < 127 ).ToArray() );

            DateTime value;
            DateTime? valueFromMMDD = str.MonthDayStringAsDateTime();

            // first check if this is a "MM/dd", "M/dd", "M/d" or "MM/d" string ( We want Rock to treat "MM/dd", "M/dd", "M/d" or "MM/d" strings consistently regardless of culture )
            if ( valueFromMMDD.HasValue )
            {
                return valueFromMMDD;
            }
            else if ( DateTime.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to integer.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static int AsInteger( this string str )
        {
            return str.AsIntegerOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to an integer.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
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
        [System.Diagnostics.DebuggerStepThrough]
        public static Guid AsGuid( this string str )
        {
            return str.AsGuidOrNull() ?? Guid.Empty;
        }

        /// <summary>
        /// Attempts to convert string to Guid.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
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
        /// Attempts to convert string to decimal. Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static decimal? AsDecimalOrNull( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // strip off non numeric and characters at the beginning of the line (currency symbols)
                str = Regex.Replace( str, @"^[^0-9\.-]", string.Empty );
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
        /// Attempts to convert string to decimal with invariant culture. Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static decimal? AsDecimalInvariantCultureOrNull( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // strip off non numeric and characters at the beginning of the line (currency symbols)
                str = Regex.Replace( str, @"^[^0-9\.-]", string.Empty );
            }

            decimal value;
            if ( decimal.TryParse( str, NumberStyles.Number, CultureInfo.InvariantCulture, out value ) )
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
                // strip off non numeric and characters at the beginning of the line (currency symbols)
                str = Regex.Replace( str, @"^[^0-9\.-]", string.Empty );
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
                ? ( T ) converter.ConvertFrom( value )
                : default( T );
        }

        /// <summary>
        /// Masks the specified value if greater than 4 characters (such as a credit card number) showing only the last 4 chars prefixed with 12*s
        /// For example, the return string becomes "************6789".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Masked( this string value )
        {
            return value.Masked( false );
        }

        /// <summary>
        /// Masks the specified value if greater than 4 characters (such as a credit card number) showing only the last 4 chars and replacing the preceeding chars with *
        /// For example, the return string becomes "************6789".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="preserveLength">if set to <c>true</c> [preserve length]. If false, always put 12 *'s as the prefix</param>
        /// <returns></returns>
        public static string Masked( this string value, bool preserveLength )
        {
            if ( value != null && value.Length > 4 )
            {
                int maskedLength = preserveLength ? value.Length - 4 : 12;
                return string.Concat( new string( '*', maskedLength ), value.Substring( value.Length - 4 ) );
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
        /// Removes the trailing forwardslash.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public static string RemoveTrailingForwardslash( this string value )
        {
            return value.TrimEnd( new char[] { '/' } );
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

        /// <summary>
        /// Returns a list of KeyValuePairs from a serialized list of Rock KeyValuePairs (e.g. 'Item1^Value1|Item2^Value2')
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<KeyValuePair<string, object>> ToKeyValuePairList( this string input )
        {
            List<KeyValuePair<string, object>> keyPairs = new List<KeyValuePair<string, object>>();

            if ( !string.IsNullOrWhiteSpace( input ) )
            {
                var items = input.Split( '|' );

                foreach ( var item in items )
                {
                    var parts = item.Split( '^' );
                    if ( parts.Length == 2 )
                    {
                        keyPairs.Add( new KeyValuePair<string, object>( parts[0], parts[1] ) );
                    }
                }
            }

            return keyPairs;
        }

        /// <summary>
        /// Removes the spaces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string RemoveSpaces( this string input )
        {
            return input.Replace( " ", string.Empty );
        }

        /// <summary>
        /// Removes all whitespace in a string, including carriage return and line feed characters.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns></returns>
        public static string RemoveWhiteSpace( this string input )
        {
            return string.Concat( input.Where( c => !char.IsWhiteSpace( c ) ) );
        }

        /// <summary>
        /// Breaks a string into chunks. Handy for splitting a large string into smaller chunks
        /// from https://stackoverflow.com/a/1450889/1755417
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="maxChunkSize">Maximum size of the chunk.</param>
        /// <returns></returns>
        public static IEnumerable<string> SplitIntoChunks( this string str, int maxChunkSize )
        {
            for ( int i = 0; i < str.Length; i += maxChunkSize )
            {
                yield return str.Substring( i, Math.Min( maxChunkSize, str.Length - i ) );
            }
        }

        /// <summary>
        /// Removes any carriage return and/or line feed characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string RemoveCrLf( this string str )
        {
            return str.Replace( Environment.NewLine, " " ).Replace( "\x0A", " " );
        }

        /// <summary>
        /// Writes a string to a new memorystream
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static System.IO.MemoryStream ToMemoryStream( this string str )
        {
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter( stream );
            writer.Write( str );
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Creates a StreamReader with the string data
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static System.IO.StreamReader ToStreamReader( this string str )
        {
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter( stream );
            writer.Write( str );
            writer.Flush();
            stream.Position = 0;
            return new System.IO.StreamReader( stream );
        }

        /// <summary>
        /// Turns a string into a properly XML Encoded string.
        /// </summary>
        /// <param name="str">Plain text to convert to XML Encoded string</param>
        /// <param name="isAttribute">If <c>true</c> then additional encoding is done to ensure proper use in an XML attribute value.</param>
        /// <returns>XML encoded string</returns>
        public static string EncodeXml( this string str, bool isAttribute = false )
        {
            var sb = new StringBuilder( str.Length );

            foreach ( var chr in str )
            {
                if ( chr == '<' )
                {
                    sb.Append( "&lt;" );
                }
                else if ( chr == '>' )
                {
                    sb.Append( "&gt;" );
                }
                else if ( chr == '&' )
                {
                    sb.Append( "&amp;" );
                }
                else if ( isAttribute && chr == '\"' )
                {
                    sb.Append( "&quot;" );
                }
                else if ( isAttribute && chr == '\'' )
                {
                    sb.Append( "&apos;" );
                }
                else if ( chr == '\n' )
                {
                    sb.Append( isAttribute ? "&#xA;" : "\n" );
                }
                else if ( chr == '\r' )
                {
                    sb.Append( isAttribute ? "&#xD;" : "\r" );
                }
                else if ( chr == '\t' )
                {
                    sb.Append( isAttribute ? "&#x9;" : "\t" );
                }
                else
                {
                    sb.Append( chr );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true/false based on whether the provided string is in a valid hex color format.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidHexColor( this string str )
        {
            if (str == null )
            {
                return false;
            }

            return Regex.IsMatch( str, @"^#(([0-9a-fA-F]{2}){3}|([0-9a-fA-F]){3})$" );
        }

        /// <summary>
        /// Returns the hex color as a string if it is valid otherwise it will return the fallback or null.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static string AsHexColorString( this string str, string fallback = null )
        {
            if ( str.IsValidHexColor() )
            {
                return str;
            }

            // Return null if no fallback provided
            if ( fallback == null )
            {
                return null;
            }

            // Check if fallback is a valid hex color
            if ( fallback.IsValidHexColor() )
            {
                return fallback;
            }

            return null;
        }

        #endregion String Extensions
    }
}
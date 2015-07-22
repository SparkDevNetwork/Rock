using System;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace Rock
{
    /// <summary>
    /// String extensions for HTML things
    /// </summary>
    public static class StringHtmlExtensions
    {
        /// <summary>
        /// Converts string to a HTML title "<span class='first-word'>first-word</span> rest of string".
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string FormatAsHtmlTitle( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // Remove any HTML
                string encodedStr = HttpUtility.HtmlEncode( str );

                // split first word from rest of string
                int endOfFirstWord = encodedStr.IndexOf( " " );

                if ( endOfFirstWord != -1 )
                {
                    return "<span class='first-word'>" + encodedStr.Substring( 0, endOfFirstWord ) + " </span> " + encodedStr.Substring( endOfFirstWord, encodedStr.Length - endOfFirstWord );
                }
                else
                {
                    return "<span class='first-word'>" + encodedStr + " </span>";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts CR (carriage return) LF (line feed) to non-encoded html breaks (br).
        /// </summary>
        /// <param name="str">a string that contains CR LF</param>
        /// <returns>a string with CRLF replaced with html <code>br</code></returns>
        public static string ConvertCrLfToHtmlBr( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            return str.Replace( Environment.NewLine, "<br/>" ).Replace( "\x0A", "<br/>" );
        }

        /// <summary>
        /// Converts the HTML br to cr lf.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ConvertBrToCrLf( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            return str.Replace( "<br/>", Environment.NewLine ).Replace( "<br>", Environment.NewLine );
        }

        /// <summary>
        /// HTML Encodes the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string EncodeHtml( this string str )
        {
            return HttpUtility.HtmlEncode( str );
        }

        /// <summary>
        /// Sanitizes the HTML by removing tags.  If strict is true, all html tags will be removed, if false, only a blacklist of specific XSS dangerous tags and attribute values are removed.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="strict">if set to <c>true</c> [strict].</param>
        /// <returns></returns>
        public static string SanitizeHtml( this string html, bool strict = true )
        {
            if ( strict )
            {
                // from http://stackoverflow.com/a/18154152/
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml( html );
                return doc.DocumentNode.InnerText;
            }
            else
            {
                return Rock.Web.Utilities.HtmlSanitizer.SanitizeHtml( html );
            }
        }

        /// <summary>
        /// Scrubs any html from the string but converts carriage returns into html &lt;br/&gt; suitable for web display.
        /// </summary>
        /// <param name="str">a string that may contain unsanitized html and carriage returns</param>
        /// <returns>a string that has been scrubbed of any html with carriage returns converted to html br</returns>
        public static string ScrubHtmlAndConvertCrLfToBr( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            // Note: \u00A7 is the section symbol

            // First we convert newlines and carriage returns to a character that can
            // pass through the Sanitizer.
            str = str.Replace( Environment.NewLine, "\u00A7" ).Replace( "\x0A", "\u00A7" );

            // Now we pass it to sanitizer and then convert those section-symbols to <br/>
            return str.SanitizeHtml().Replace( "\u00A7", "<br/>" );
        }

        /// <summary>
        /// Returns true if the given string is a valid email address.
        /// </summary>
        /// <param name="email">The string to validate</param>
        /// <returns>true if valid email, false otherwise</returns>
        public static bool IsValidEmail( this string email )
        {
            return Regex.IsMatch( email, @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+" );
        }
    }
}

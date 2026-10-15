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

using HtmlAgilityPack;

namespace Rock
{
    /// <summary>
    /// String extensions for HTML things
    /// </summary>
    public static partial class ExtensionMethods
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
                return HttpUtility.HtmlEncode( str );
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts line endings ( CRLF or just LF ) to non-encoded HTML breaks &lt;br&gt;
        /// </summary>
        /// <param name="str">a string that contains CR LF</param>
        /// <returns>a string with CRLF replaced with HTML <code>br</code></returns>
        public static string ConvertCrLfToHtmlBr( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            // Normalize line breaks so this works with either CRLF or LF line endings.
            var result = str.Replace( "\r\n", "\n" );

            return result.Replace( "\n", "<br>" );
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
        /// URLs the encode.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string UrlEncode( this string str )
        {
            return Uri.EscapeDataString( str );
        }

        /// <summary>
        /// Sanitizes the HTML by removing tags.  If strict is true, all html tags will be removed, if false, only a blacklist of specific XSS dangerous tags and attribute values are removed.
        /// NOTE: This method will do things like strip the less-than symbol from strings like <![CDATA['in math 6 < 7.']]>
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="strict">if set to <c>true</c> [strict].</param>
        /// <returns></returns>
        public static string SanitizeHtml( this string html, bool strict = true )
        {
            // Don't choke on nulls
            if ( string.IsNullOrWhiteSpace( html ) )
            {
                return string.Empty;
            }

            if ( strict )
            {
                // from http://stackoverflow.com/a/18154152/
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml( html );
                // Yes, even strip out HTML comments. See https://github.com/zzzprojects/html-agility-pack/issues/80#issuecomment-331663090
                doc.DocumentNode.Descendants()
                 .Where( n => n.NodeType == HtmlAgilityPack.HtmlNodeType.Comment )
                 .ToList()
                 .ForEach( n => n.Remove() );
                return doc.DocumentNode.InnerText;
            }
            else
            {
                return Rock.Web.Utilities.HtmlSanitizer.SanitizeHtml( html );
            }
        }

        /// <summary>
        /// The maximum time each Lava pattern may run, and the total time the
        /// patterns may keep repeating, in <see cref="SanitizeLava(string)"/>.
        /// </summary>
        private static readonly TimeSpan _sanitizeLavaTimeLimit = TimeSpan.FromSeconds( 1 );

        /// <summary>
        /// Matches a Lava output such as <c>{{ value }}</c>.
        /// </summary>
        /// <remarks>
        /// The <c>(?&lt;!\{)</c> lookbehind skips match attempts that start inside
        /// a run of braces. Every brace in a run reaches the same first closing
        /// brace, so if the first brace of the run cannot match, none of the others
        /// can either, and the matches are the same as without the lookbehind.
        /// Without it a long run of braces is scanned once per brace.
        /// </remarks>
        private static readonly Regex _lavaDoubleBracesRegex = new Regex( @"(?<!\{)\{\{([^\}]+)\}\}", RegexOptions.Compiled, _sanitizeLavaTimeLimit );

        /// <summary>
        /// Matches a Lava tag such as <c>{% tag %}</c>.
        /// </summary>
        private static readonly Regex _lavaBracePercentRegex = new Regex( @"\{%([^\}]+)%\}", RegexOptions.Compiled, _sanitizeLavaTimeLimit );

        /// <summary>
        /// Matches a Lava shortcode such as <c>{[ shortcode ]}</c>.
        /// </summary>
        private static readonly Regex _lavaBraceBracketRegex = new Regex( @"\{\[([^\}]+)\]\}", RegexOptions.Compiled, _sanitizeLavaTimeLimit );

        /// <summary>
        /// Matches an opening brace that starts a Lava output, tag or shortcode opener,
        /// or that is directly followed by a "/" (which could start a Lava comment).
        /// </summary>
        private static readonly Regex _lavaOpenerBraceRegex = new Regex( @"\{(?=[{%\[/])", RegexOptions.Compiled );

        /// <summary>
        /// The maximum number of times the Lava patterns are repeated before the
        /// final opener removal pass in <see cref="SanitizeLava(string)"/>.
        /// </summary>
        private const int MaxSanitizeLavaPasses = 10;

        /// <summary>
        /// Removes all lava markup from the string including short codes.
        /// The Lava patterns are repeated a limited number of times to tidy up
        /// nested or split markup, then every brace directly followed by
        /// <c>{</c>, <c>%</c>, <c>[</c> or <c>/</c> is removed. The result never
        /// contains a Lava output, tag or shortcode opener (<c>{{</c>, <c>{%</c>
        /// or <c>{[</c>), and a brace is never left directly before a <c>/</c>.
        /// </summary>
        /// <param name="lava">The lava.</param>
        /// <returns></returns>
        public static string SanitizeLava( this string lava )
        {
            // Don't choke on nulls
            if ( string.IsNullOrWhiteSpace( lava ) )
            {
                return string.Empty;
            }

            /*
                10/15/2026 - MSE

                A single pass of these patterns could be bypassed. Removing an inner
                tag could join the text around it into a new tag ("{{% x %}{ 1 }}"),
                and a tag whose body held a closing brace was never matched at all
                ("{{ 1 | Append:'}' }}"). So we repeat the patterns until the text
                stops changing, but at most MaxSanitizeLavaPasses times. The cap
                matters: crafted deep nesting can need one pass per level, and each
                pass costs at least a full scan, so without it one request could tie
                up the CPU for minutes. The repeat only tidies up leftover text. The
                safety comes from the final pass, which always runs after the loop
                and deletes every brace that is still directly followed by "{", "%",
                "[" or "/". A brace that survives that last pass is followed by a
                character that also survives and is not "{", so no opener can remain
                or be created, however many passes the loop ran. For the same reason
                the patterns have a time limit: some inputs make a single pattern
                scan the text once per brace, so if the patterns run too long we stop
                repeating them and go straight to the final pass. The "/" case stops
                a Lava comment from joining two braces ("{/- x -/{ 1 }}") under the
                legacy RockLiquid engine, which strips comments before parsing.

                Reason: Enable Lava = No must not run Lava (RCVE-2026-2gmq).
            */
            var previous = string.Empty;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                for ( var pass = 0; pass < MaxSanitizeLavaPasses && lava != previous && stopwatch.Elapsed < _sanitizeLavaTimeLimit; pass++ )
                {
                    previous = lava;
                    lava = _lavaDoubleBracesRegex.Replace( lava, string.Empty );
                    lava = _lavaBracePercentRegex.Replace( lava, string.Empty );
                    lava = _lavaBraceBracketRegex.Replace( lava, string.Empty );
                }
            }
            catch ( RegexMatchTimeoutException )
            {
                // The final pass below still removes every opener.
            }

            return _lavaOpenerBraceRegex.Replace( lava, string.Empty );
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

            // Now we pass it to sanitizer and then convert those section-symbols to <br>
            return str.SanitizeHtml().Replace( "\u00A7", "<br>" );
        }

        /// <summary>
        /// Scrubs the HTML but retains "&lt;br/&gt;",changes "&lt;/p/&gt;" to "&lt;br//&gt;&lt;br/&gt;", and "\r\n" to "&lt;br/&gt;".
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ScrubHtmlForGridDisplay( this string str )
        {
            if ( string.IsNullOrWhiteSpace( str ) )
            {
                return string.Empty;
            }

            // Note: \u00A7 is the section symbol, \u00A6 is the broken bar symbol
            // First convert HTML breaks to a character that can pass through the Sanitizer.
            str = str.Replace( "<br/>", "\u00A7" ).Replace( "<br />", "\u00A7" ).Replace( "<br>", "\u00A7" );
            str = str.Replace( "</p>", "\u00A6" );

            // Now sanitize and convert the symbols to breaks
            str = str.SanitizeHtml().Replace( "\u00A7", "<br>" ).Replace( "\u00A6", "<br><br>" ).Replace( "\r\n", "<br>" );
            return str;
        }

        /// <summary>
        /// Returns true if the given string is a valid email address.
        /// </summary>
        /// <param name="email">The string to validate</param>
        /// <returns>true if valid email, false otherwise</returns>
        [Obsolete( "Use EmailAddressFieldValidator.IsValid() instead." )]
        [RockObsolete( "1.16" )]
        public static bool IsValidEmail( this string email )
        {
            Match match = Regex.Match( email, @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+" );
            if ( !match.Success || match.Index != 0 )
            {
                return false;
            }

            return match.Length == email.Length;
        }

        /// <summary>
        /// Convert strings within the text that appear to be http/ftp/https links into clickable html links
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string Linkify( this string text )
        {
            // from http://stackoverflow.com/a/4750468
            var result = Regex.Replace(
                text,
                @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                "<a target='_blank' rel='noopener noreferrer' href='$1'>$1</a>" );

            return result;
        }

        /// <summary>
        /// Uses the CommonMark Markdown library to convert Markdown into HTML
        /// </summary>
        /// <param name="markdown">The markdown.</param>
        /// <param name="renderSoftLineBreaksAsLineBreaks">if set to <c>true</c> [render soft line breaks as line breaks].</param>
        /// <returns></returns>
        public static string ConvertMarkdownToHtml( this string markdown, bool renderSoftLineBreaksAsLineBreaks = false )
        {
            if ( markdown == null )
            {
                return string.Empty;
            }

            // convert any Markdown into HTML
            var settings = CommonMark.CommonMarkSettings.Default.Clone();
            settings.RenderSoftLineBreaksAsLineBreaks = renderSoftLineBreaksAsLineBreaks;
            /*
                6/9/2020 - JME
                Added the .Trim() to the return below. Without it CommonMark was converting strings
                like 'Test' to '<p>Test</p>/r/n/r/n'. The adding of two line breaks was causing issues
                when other filters were being applied in Lava to make line breaks '<br>'.

                Reason: Notes Lava was having extra <br>'s at the end.
            */
            return CommonMark.CommonMarkConverter.Convert( markdown, settings ).Trim();
        }

        /// <summary>
        /// Moves the CSS inline using PreMailer.Net, which moves any stylesheets to inline style attributes, for maximum compatibility with email clients
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public static string ConvertHtmlStylesToInlineAttributes( this string html )
        {
            try
            {
                var rockHtmlMarkupFormatter = new Rock.Utility.RockHtmlMarkupFormatter();
                var result = PreMailer.Net.PreMailer.MoveCssInline( html, false, ".ignore", null, false, false, rockHtmlMarkupFormatter );
                return result.Html;
            }
            catch
            {
                return html;
            }
        }

        /// <summary>
        /// Moves the CSS inline using PreMailer.Net, which moves any stylesheets to inline style attributes, for maximum compatibility with email clients
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="removeStyleElements">If <see langword="true"/>, then style elements will be removed.</param>
        /// <returns></returns>
        internal static string ConvertHtmlStylesToInlineAttributes( this string html, bool removeStyleElements )
        {
            try
            {
                var rockHtmlMarkupFormatter = new Rock.Utility.RockHtmlMarkupFormatter();
                var result = PreMailer.Net.PreMailer.MoveCssInline( html, removeStyleElements, ".ignore", null, false, false, rockHtmlMarkupFormatter );
                return result.Html;
            }
            catch
            {
                return html;
            }
        }

        #region HTML Truncation Extensions 
        // Modified from:
        // https://blog.andycook.com/code/2016/02/11/String-truncation-of-HTML/

        /// <summary>
        /// Uses the node structure of HTML to intelligently truncate string length.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="maxCharacters"></param>
        /// <param name="trailingText"></param>
        /// <returns></returns>
        public static string TruncateHtml( this string html, int maxCharacters, string trailingText = "&hellip;" )
        {
            if ( string.IsNullOrEmpty( html ) || html.Length <= maxCharacters )
            {
                return html;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml( html );
            if ( htmlDocument.DocumentNode.InnerText.Length <= maxCharacters )
            {
                return html;
            }

            var textNodes = new LinkedList<HtmlNode>( htmlDocument.DocumentNode.TextDescendants() );

            var precedingText = 0;
            var lastNode = textNodes.First;
            while ( precedingText <= maxCharacters && lastNode != null )
            {
                var nodeTextLength = lastNode.Value.InnerText.Length;
                if ( precedingText + nodeTextLength > maxCharacters )
                {
                    var truncatedText = TruncateWords( lastNode.Value.InnerText, maxCharacters - precedingText );

                    if ( String.IsNullOrWhiteSpace( truncatedText ) && lastNode.Previous != null )
                    {
                        // Put the ellipsis in the previous node and remove the empty node.
                        lastNode.Previous.Value.InnerHtml = lastNode.Previous.Value.InnerText.Trim() + trailingText;
                        lastNode.Value.InnerHtml = String.Empty;
                        lastNode = lastNode.Previous;
                    }
                    else
                    {
                        lastNode.Value.InnerHtml = truncatedText + trailingText;
                    }

                    break;
                }

                if ( precedingText + nodeTextLength == maxCharacters )
                {
                    break;
                }

                precedingText += nodeTextLength;
                lastNode = lastNode.Next;
            }

            // Remove all the nodes after lastNode.
            if ( lastNode != null )
            {
                RemoveSubsequentNodes( lastNode.Value );
            }

            return htmlDocument.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Retrieve the HTML text items within an HTML node. 
        /// </summary>
        /// <param name="root">The HTML node root value. </param>
        /// <returns></returns>
        public static IEnumerable<HtmlNode> TextDescendants( this HtmlNode root )
        {
            return root.Descendants().Where( n => n.NodeType == HtmlNodeType.Text && !String.IsNullOrWhiteSpace( n.InnerText ) );
        }

        /// <summary>
        /// Recursively removes HTML nodes that exist after <paramref name="lastNode"/>.
        /// </summary>
        /// <param name="lastNode">The last node to include.</param>
        private static void RemoveSubsequentNodes( HtmlNode lastNode )
        {
            while ( lastNode.NextSibling != null )
            {
                lastNode.NextSibling.Remove();
            }

            if ( lastNode.ParentNode != null )
            {
                RemoveSubsequentNodes( lastNode.ParentNode );
            }
        }

        /// <summary>
        /// Removes extraneous characters to shorten the given <paramref name="value"/> without splitting any words.
        /// </summary>
        /// <param name="value">The string to potentially truncate.</param>
        /// <param name="length">The acceptable length of the return string.</param>
        /// <returns>A string of the correct length.</returns>
        private static string TruncateWords( string value, int length )
        {
            if ( String.IsNullOrWhiteSpace( value ) || length <= 0 )
            {
                return String.Empty;
            }

            if ( length > value.Length )
            {
                return value;
            }

            var endIndex = length;
            while ( char.IsLetterOrDigit( value[endIndex - 1] ) && char.IsLetterOrDigit( value[endIndex] ) && endIndex > 1 )
            {
                endIndex--;
            }

            if ( endIndex == 1 )
            {
                return String.Empty;
            }

            return value.Substring( 0, endIndex ).Trim();
        }

        #endregion
    }
}

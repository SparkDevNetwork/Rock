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
using System.Text;
using System.Web;
using System.Xml;

using HtmlAgilityPack;

namespace Rock.Web.Utilities
{
    /// <summary>
    /// Sanitation method from Rick Strahl's blog...
    /// http://weblog.west-wind.com/posts/2012/Jul/19/NET-HTML-Sanitation-for-rich-HTML-Input
    /// https://github.com/RickStrahl/HtmlSanitizer/blob/master/HtmlSanitizer/HtmlSanitizer/HtmlSanitizer.cs 
    /// </summary>
    public class HtmlSanitizer
    { 
        /// <summary>
        /// The black list
        /// </summary>
        public HashSet<string> BlackList = new HashSet<string>() 
        {
                { "script" },
                { "iframe" },
                { "form" },
                { "object" },
                { "embed" },
                { "link" },                
                { "head" },
                { "meta" },
                { "base" },
                { "style" },
                { "title" }
        };

        /// <summary>
        /// Cleans up an HTML string and removes HTML tags in blacklist
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="blackList">The black list.</param>
        /// <returns></returns>
        public static string SanitizeHtml( string html, params string[] blackList )
        {
            var sanitizer = new HtmlSanitizer();
            
            if ( blackList != null && blackList.Length > 0 )
            {
                sanitizer.BlackList.Clear();
                foreach ( string item in blackList )
                    sanitizer.BlackList.Add( item );
            }

            return sanitizer.Sanitize( html );
        }

        /// <summary>
        /// Cleans up an HTML string by removing elements
        /// on the blacklist and all elements that start
        /// with onXXX .
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string Sanitize( string html )
        {
            if ( string.IsNullOrWhiteSpace( html ) )
            {
                return html;
            }

            /*
                10/18/2026 - MSE

                The parser keeps the raw source text of a tag it cannot parse
                strictly, and the XML writer throws on some comment shapes. A form
                feed between the tag name and its attributes, an unclosed final tag,
                or a comment like "<!>" could carry event handlers through or fail
                the request. Control characters are removed before parsing, any
                parser or writer failure falls back to encoding the whole value so
                nothing is ever emitted raw, and elements without a plain tag name
                are dropped in SanitizeHtmlNode.

                Reason: Prevent sanitizer bypasses and exceptions on malformed markup.
            */
            try
            {
                var doc = new HtmlDocument();

                doc.LoadHtml( RemoveControlCharacters( html ).Replace( "&nbsp;", "<nbsp />" ).Replace( "&#39;", "<apos />" ).Replace( "&quot;", "<quote />" ) );

                SanitizeHtmlNode( doc.DocumentNode );

                //return doc.DocumentNode.WriteTo();

                string output = null;

                // Use an XmlTextWriter to create self-closing tags
                using ( StringWriter sw = new StringWriter() )
                {
                    XmlWriter writer = new XmlTextWriter( sw );
                    doc.DocumentNode.WriteTo( writer );
                    output = sw.ToString();

                    // strip off XML doc header
                    if ( !string.IsNullOrEmpty( output ) )
                    {
                        int at = output.IndexOf( "?>" );
                        output = output.Substring( at + 2 );
                    }

                    writer.Close();
                }
                doc = null;

                return output.Replace( "<nbsp />", "&nbsp;" ).Replace( "<apos />", "&#39;" ).Replace( "<quote />", "&quot;" );
            }
            catch ( Exception )
            {
                return HttpUtility.HtmlEncode( html );
            }
        }

        /// <summary>
        /// Removes control characters other than tab, carriage return and line
        /// feed. A form feed is replaced with a space because browsers treat it
        /// as whitespace, so the markup keeps the meaning a browser would give it.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value without control characters.</returns>
        private static string RemoveControlCharacters( string value )
        {
            var sb = new StringBuilder( value.Length );

            foreach ( var c in value )
            {
                if ( c == '\f' )
                {
                    sb.Append( ' ' );
                }
                else if ( c == '\t' || c == '\r' || c == '\n' || !char.IsControl( c ) )
                {
                    sb.Append( c );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the attribute value in the form a browser would read it when
        /// deciding the URL scheme: entities decoded, whitespace and control
        /// characters removed, and lower case.
        /// </summary>
        /// <param name="value">The raw attribute value.</param>
        /// <returns>The normalized value.</returns>
        private static string GetNormalizedAttributeValue( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return string.Empty;
            }

            // Decode twice so an entity that was itself entity encoded is also caught.
            var decoded = HttpUtility.HtmlDecode( HttpUtility.HtmlDecode( value ) );
            var sb = new StringBuilder( decoded.Length );

            foreach ( var c in decoded )
            {
                if ( !char.IsWhiteSpace( c ) && !char.IsControl( c ) )
                {
                    sb.Append( char.ToLowerInvariant( c ) );
                }
            }

            // HTML5 named entities that HtmlDecode does not know.
            return sb.ToString().Replace( "&colon;", ":" ).Replace( "&tab;", string.Empty ).Replace( "&newline;", string.Empty );
        }

        /// <summary>
        /// Determines whether the element name is a plain HTML tag name: a letter
        /// followed by letters, digits, hyphens, underscores or colons.
        /// </summary>
        /// <param name="elementName">The lower case element name.</param>
        /// <returns><c>true</c> if the name is a plain tag name; otherwise <c>false</c>.</returns>
        private static bool IsPlainHtmlElementName( string elementName )
        {
            if ( string.IsNullOrEmpty( elementName ) || elementName[0] < 'a' || elementName[0] > 'z' )
            {
                return false;
            }

            foreach ( var c in elementName )
            {
                if ( !( ( c >= 'a' && c <= 'z' ) || ( c >= '0' && c <= '9' ) || c == '-' || c == '_' || c == ':' ) )
                {
                    return false;
                }
            }

            return true;
        }

        private void SanitizeHtmlNode( HtmlNode node )
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                
            }

            if ( node.NodeType == HtmlNodeType.Element )
            {
                // check for blacklist items and remove
                if ( BlackList.Contains( node.Name ) )
                {
                    node.Remove();
                    return;
                }

                /*
                    10/18/2026 - MSE

                    An unclosed final tag or a stray less than sign is parsed as an
                    element whose name holds the raw source text (for example
                    "img src=x onerror=..."), and that text is written back out as is.
                    Only elements with a plain tag name are kept.

                    Reason: Raw tag text bypassed the attribute and blacklist checks.
                */
                if ( !IsPlainHtmlElementName( node.Name.ToLowerInvariant() ) )
                {
                    node.Remove();
                    return;
                }

                // remove CSS Expressions and embedded script links
                if ( node.Name == "style" )
                {
                    var val = node.InnerHtml;
                    if ( string.IsNullOrEmpty( node.InnerText ) )
                    {
                        if ( HasExpressionLinks( val ) || HasScriptLinks( val ) )
                            node.ParentNode.RemoveChild( node );
                    }
                }

                // remove script attributes
                if ( node.HasAttributes )
                {
                    for ( int i = node.Attributes.Count - 1; i >= 0; i-- )
                    {
                        HtmlAttribute currentAttribute = node.Attributes[i];

                        var attr = currentAttribute.Name.ToLower();

                        /*
                            10/18/2026 - MSE

                            Browsers ignore tabs, new lines and other whitespace inside
                            a URL scheme and decode entities before reading it, so a
                            value like "java[tab]script:" still runs. The value is
                            normalized the same way before the scheme checks.

                            Reason: Obfuscated script schemes passed the substring check.
                        */
                        var val = GetNormalizedAttributeValue( currentAttribute.Value );

                        // remove event handlers
                        if ( attr.StartsWith( "on" ) )
                            node.Attributes.Remove( currentAttribute );

                        // Remove CSS Expressions
                        else if ( attr == "style" &&
                                 val != null &&
                                 HasExpressionLinks( val ) || HasScriptLinks( val ) )
                            node.Attributes.Remove( currentAttribute );

                        // remove script links from all attributes
                        else if (
                            //(attr == "href" || attr== "src" || attr == "dynsrc" || attr == "lowsrc") &&
                                 val != null &&
                                 HasScriptLinks( val ) )
                            node.Attributes.Remove( currentAttribute );
                    }
                }
            }

            // Look through child nodes recursively
            if ( node.HasChildNodes )
            {
                for ( int i = node.ChildNodes.Count - 1; i >= 0; i-- )
                {
                    SanitizeHtmlNode( node.ChildNodes[i] );
                }
            }
        }

        private bool HasScriptLinks( string value )
        {
            return value.Contains( "javascript:" ) || value.Contains( "vbscript:" );
        }

        private bool HasExpressionLinks( string value )
        {
            return value.Contains( "expression" );
        }
    }
}
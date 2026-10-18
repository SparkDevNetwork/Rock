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
                { "title" },
                { "xmp" },
                { "noscript" },
                { "noembed" },
                { "noframes" },
                { "plaintext" },
                { "frame" },
                { "frameset" },
                { "portal" },
                { "applet" }
        };

        /// <summary>
        /// The deepest node nesting that will be sanitized. Deeper markup is
        /// returned HTML encoded instead.
        /// </summary>
        private const int MaxNestingDepth = 256;

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

                var source = RemoveControlCharacters( html ).Replace( "&nbsp;", "<nbsp />" ).Replace( "&#39;", "<apos />" ).Replace( "&quot;", "<quote />" );

                doc.LoadHtml( source );

                /*
                    10/18/2026 - MSE

                    Parsing is not recursive, but the sanitize walk was (it is now a
                    loop) and writing the document back out still is, so a few
                    kilobytes of nested tags overflowed the stack and ended
                    the whole process (a StackOverflowException cannot be caught).
                    The depth is checked without recursion right after parsing and
                    anything nested deeper than MaxNestingDepth is encoded instead.

                    Reason: Deeply nested markup crashed the process.
                */
                if ( IsNestedTooDeeply( doc.DocumentNode ) )
                {
                    return HttpUtility.HtmlEncode( html );
                }

                SanitizeHtmlNode( doc.DocumentNode, source );

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

        /// <summary>
        /// Gets the source text of the element's opening tag, from its less than
        /// sign to the next greater than sign, the next less than sign or the end
        /// of the input, whichever comes first.
        /// </summary>
        /// <param name="node">The element node.</param>
        /// <param name="source">The source text the document was loaded from.</param>
        /// <returns>The tag source text.</returns>
        private static string GetTagSourceText( HtmlNode node, string source )
        {
            var start = node.StreamPosition;

            if ( start < 0 || start >= source.Length || source[start] != '<' )
            {
                return "<" + node.Name;
            }

            // Stop at the next tag so its source is not repeated as text.
            var end = source.IndexOfAny( new[] { '<', '>' }, start + 1 );
            string text;

            if ( end < 0 )
            {
                text = source.Substring( start );
            }
            else
            {
                text = source.Substring( start, source[end] == '>' ? end - start + 1 : end - start );
            }

            return text;
        }

        /// <summary>
        /// Gets a value indicating whether the node tree is nested deeper than
        /// <see cref="MaxNestingDepth"/>. The tree is walked without recursion.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        /// <returns><c>true</c> if the tree is nested too deeply; otherwise <c>false</c>.</returns>
        private static bool IsNestedTooDeeply( HtmlNode rootNode )
        {
            var nodes = new Stack<KeyValuePair<HtmlNode, int>>();
            nodes.Push( new KeyValuePair<HtmlNode, int>( rootNode, 0 ) );

            while ( nodes.Count > 0 )
            {
                var item = nodes.Pop();

                if ( item.Value > MaxNestingDepth )
                {
                    return true;
                }

                foreach ( var childNode in item.Key.ChildNodes )
                {
                    nodes.Push( new KeyValuePair<HtmlNode, int>( childNode, item.Value + 1 ) );
                }
            }

            return false;
        }

        /// <summary>
        /// Sanitizes the node and all of its descendants. The tree is walked
        /// without recursion so the nesting depth does not grow the call stack.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        /// <param name="source">The source text the document was loaded from.</param>
        private void SanitizeHtmlNode( HtmlNode rootNode, string source )
        {
            var nodes = new Stack<HtmlNode>();
            nodes.Push( rootNode );

            while ( nodes.Count > 0 )
            {
                var node = nodes.Pop();

                // Look through child nodes of any node that was kept.
                if ( SanitizeSingleHtmlNode( node, nodes, source ) && node.HasChildNodes )
                {
                    for ( int i = node.ChildNodes.Count - 1; i >= 0; i-- )
                    {
                        nodes.Push( node.ChildNodes[i] );
                    }
                }
            }
        }

        /// <summary>
        /// Sanitizes a single node without looking at its child nodes.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="pendingNodes">The nodes still to be sanitized; nodes moved by this method are added to it.</param>
        /// <param name="source">The source text the document was loaded from.</param>
        /// <returns><c>true</c> if the node was kept; <c>false</c> if it was removed or replaced.</returns>
        private bool SanitizeSingleHtmlNode( HtmlNode node, Stack<HtmlNode> pendingNodes, string source )
        {
            if (node.NodeType == HtmlNodeType.Text)
            {

            }

            /*
                10/18/2026 - MSE

                Comment bodies are written out unescaped. Inside an element the
                browser reads as raw text (xmp, noscript, textarea and similar) a
                "comment" can close that element and start live markup the parser
                never saw as markup. Comments are removed, as strict mode does.

                Reason: Comments inside raw text elements bypassed the sanitizer.
            */
            if ( node.NodeType == HtmlNodeType.Comment )
            {
                node.Remove();
                return false;
            }

            if ( node.NodeType == HtmlNodeType.Element )
            {
                // check for blacklist items and remove
                if ( BlackList.Contains( node.Name ) )
                {
                    node.Remove();
                    return false;
                }

                /*
                    10/18/2026 - MSE

                    An unclosed final tag or a stray less than sign is parsed as an
                    element whose name holds the raw source text (for example
                    "img src=x onerror=..."), and that text is written back out as is.
                    Such an element is replaced by a text node of its tag source,
                    which the writer encodes so it shows as plain text, and its child
                    nodes move up to be sanitized as usual (so the sentence
                    "a < 10 and > 2" keeps its words).

                    Reason: Raw tag text bypassed the attribute and blacklist checks.
                */
                if ( !IsPlainHtmlElementName( node.Name.ToLowerInvariant() ) )
                {
                    var parentNode = node.ParentNode;
                    var childNodes = new List<HtmlNode>( node.ChildNodes );

                    node.RemoveAllChildren();
                    parentNode.InsertBefore( node.OwnerDocument.CreateTextNode( GetTagSourceText( node, source ) ), node );

                    foreach ( var childNode in childNodes )
                    {
                        parentNode.InsertBefore( childNode, node );
                        pendingNodes.Push( childNode );
                    }

                    parentNode.RemoveChild( node );
                    return false;
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

                        /*
                            10/18/2026 - MSE

                            These attributes point a button or input at another form or
                            submit address, which would send the page's form to any site.

                            Reason: Prevent form submission to another site.
                        */
                        else if ( attr == "formaction" || attr == "form" || attr == "action" )
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

            return true;
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
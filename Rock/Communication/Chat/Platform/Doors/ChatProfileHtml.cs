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
using System.Net;
using System.Text;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Rock.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// Makes the HTML a church's profile card template renders safe to draw inside the chat page,
    /// for every person who opens that card.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lava does not escape a merge value, so a person's own fields, a nickname typed as markup,
    /// reach the output as HTML. The output is drawn in another person's browser, so this is the
    /// guard against one person scripting everyone who looks at their card.
    /// </para>
    /// <para>
    /// It keeps what it knows and drops everything else, rather than removing what it knows to be
    /// dangerous: a list of bad things is always one encoding or one new element short. The input
    /// is parsed as a browser would parse it and the output is written fresh from that tree, so
    /// nothing the parser read differently from a browser survives into what is drawn.
    /// </para>
    /// </remarks>
    internal static class ChatProfileHtml
    {
        // The elements a card's details may use: text structure, lists, links and https images.
        private static readonly HashSet<string> AllowedElements = new HashSet<string>( StringComparer.Ordinal )
        {
            "p", "div", "span", "a", "strong", "em", "b", "i", "ul", "ol", "li", "br", "small",
            "h1", "h2", "h3", "h4", "h5", "h6", "img"
        };

        // Elements written without a closing tag.
        private static readonly HashSet<string> VoidElements = new HashSet<string>( StringComparer.Ordinal ) { "br", "img" };

        // Elements dropped with everything inside them. Their text is code, styling or a form's
        // contents, never words meant for the card, so keeping it as text would only show noise.
        private static readonly HashSet<string> DroppedWithContent = new HashSet<string>( StringComparer.Ordinal )
        {
            "script", "style", "template", "noscript", "iframe", "frame", "frameset", "object", "embed",
            "applet", "textarea", "select", "option", "title", "head", "meta", "link", "base", "xmp",
            "noembed", "noframes", "plaintext", "svg", "math"
        };

        // The schemes a link may carry. A link with no scheme is a path on this site.
        private static readonly HashSet<string> LinkSchemes = new HashSet<string>( StringComparer.Ordinal ) { "http", "https", "mailto", "tel" };

        // Nesting deeper than any card needs is written as its text alone, so a pathological input
        // cannot exhaust the stack.
        private const int MaxDepth = 32;

        /// <summary>
        /// Keeps only the allowed elements and attributes of rendered HTML. An element not allowed
        /// is replaced by its contents; script, style, embedded documents and foreign markup are
        /// dropped with theirs; a link keeps an http, https, mailto or tel address or a path, an
        /// image an https address; every link opens with no opener and no referrer.
        /// </summary>
        /// <param name="html">The rendered HTML.</param>
        /// <returns>The safe HTML, or an empty string for blank input.</returns>
        internal static string Sanitize( string html )
        {
            if ( string.IsNullOrWhiteSpace( html ) )
            {
                return string.Empty;
            }

            var parser = new HtmlParser();
            var document = parser.ParseDocument( string.Empty );
            var nodes = parser.ParseFragment( html, document.Body );

            var output = new StringBuilder( html.Length );
            foreach ( var node in nodes )
            {
                Write( node, output, 0 );
            }

            return output.ToString();
        }

        /// <summary>
        /// Writes one parsed node and what it holds, keeping only what is allowed.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="output">Where the safe HTML is written.</param>
        /// <param name="depth">How deep the node is.</param>
        private static void Write( INode node, StringBuilder output, int depth )
        {
            if ( node.NodeType == NodeType.Text )
            {
                output.Append( WebUtility.HtmlEncode( node.TextContent ) );
                return;
            }

            // Comments, processing instructions and anything else carry nothing for the card.
            if ( !( node is IElement element ) )
            {
                return;
            }

            // Markup in another namespace has its own rules for links and scripts, so none of it is kept.
            if ( element.NamespaceUri != NamespaceNames.HtmlUri )
            {
                return;
            }

            var name = element.LocalName;
            if ( DroppedWithContent.Contains( name ) )
            {
                return;
            }

            if ( depth >= MaxDepth )
            {
                output.Append( WebUtility.HtmlEncode( element.TextContent ) );
                return;
            }

            var isKept = AllowedElements.Contains( name );
            if ( isKept )
            {
                if ( !WriteStartTag( element, output ) )
                {
                    return;
                }

                if ( VoidElements.Contains( name ) )
                {
                    return;
                }
            }

            foreach ( var child in element.ChildNodes )
            {
                Write( child, output, isKept ? depth + 1 : depth );
            }

            if ( isKept )
            {
                output.Append( "</" ).Append( name ).Append( '>' );
            }
        }

        /// <summary>
        /// Writes an allowed element's start tag with its allowed attributes.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="output">Where the safe HTML is written.</param>
        /// <returns>False for an image with no safe address, which is dropped rather than drawn empty.</returns>
        private static bool WriteStartTag( IElement element, StringBuilder output )
        {
            var name = element.LocalName;
            var attributes = new StringBuilder();
            var hasSource = false;

            foreach ( var attribute in element.Attributes )
            {
                var value = attribute.Value ?? string.Empty;
                switch ( attribute.Name )
                {
                    case "class":
                    case "title":
                    case "alt":
                        AppendAttribute( attributes, attribute.Name, value );
                        break;

                    case "href" when name == "a":
                        var link = SafeUrl( value, LinkSchemes, isPathAllowed: true );
                        if ( link != null )
                        {
                            AppendAttribute( attributes, "href", link );
                        }

                        break;

                    case "src" when name == "img":
                        var source = SafeUrl( value, ImageSchemes, isPathAllowed: false );
                        if ( source != null )
                        {
                            AppendAttribute( attributes, "src", source );
                            hasSource = true;
                        }

                        break;
                }
            }

            if ( name == "img" && !hasSource )
            {
                return false;
            }

            // A link that opens a page must not hand that page this one, whatever the template asked.
            if ( name == "a" )
            {
                AppendAttribute( attributes, "rel", "noopener noreferrer" );
            }

            output.Append( '<' ).Append( name ).Append( attributes ).Append( '>' );
            return true;
        }

        // An image loads as the card opens, with no click, so it may come only from an https address.
        private static readonly HashSet<string> ImageSchemes = new HashSet<string>( StringComparer.Ordinal ) { "https" };

        /// <summary>
        /// The address as a browser would follow it, when its scheme is one allowed, or null.
        /// </summary>
        /// <param name="value">The attribute's value, entities already decoded by the parser.</param>
        /// <param name="schemes">The schemes allowed.</param>
        /// <param name="isPathAllowed">Whether an address with no scheme, a path on this site, is allowed.</param>
        /// <returns>The address to write, or null to leave the attribute out.</returns>
        internal static string SafeUrl( string value, ISet<string> schemes, bool isPathAllowed )
        {
            // A browser removes tabs and line breaks anywhere in an address and trims spaces and
            // control characters from its ends, so "java&#9;script:" is a script link to it. The
            // address is judged and written in that same form, so what is checked is what is followed.
            var cleaned = new StringBuilder( value.Length );
            foreach ( var c in value )
            {
                if ( c != '\t' && c != '\n' && c != '\r' )
                {
                    cleaned.Append( c );
                }
            }

            var url = cleaned.ToString().Trim( TrimmedFromUrls );
            if ( url.Length == 0 )
            {
                return null;
            }

            // The scheme is what comes before the first colon, when no slash, question mark or hash
            // comes earlier; otherwise the colon belongs to a path and there is no scheme.
            var colon = url.IndexOf( ':' );
            var firstDelimiter = url.IndexOfAny( new[] { '/', '?', '#' } );
            var hasScheme = colon >= 0 && ( firstDelimiter < 0 || colon < firstDelimiter );

            if ( !hasScheme )
            {
                // A backslash reads as a slash in a browser, so "\\host" would leave this site.
                return isPathAllowed && url.IndexOf( '\\' ) < 0 ? url : null;
            }

            var scheme = url.Substring( 0, colon ).ToLowerInvariant();
            return schemes.Contains( scheme ) ? url : null;
        }

        // Spaces and C0 controls, which a browser trims from an address's ends.
        private static readonly char[] TrimmedFromUrls = BuildTrimmed();

        /// <summary>
        /// Lists the characters a browser trims from an address's ends.
        /// </summary>
        /// <returns>Space and every C0 control.</returns>
        private static char[] BuildTrimmed()
        {
            var chars = new char[33];
            for ( var i = 0; i <= 32; i++ )
            {
                chars[i] = ( char ) i;
            }

            return chars;
        }

        /// <summary>
        /// Writes one attribute with its value escaped, so no value can close the tag it is in.
        /// </summary>
        /// <param name="attributes">Where the element's attributes are written.</param>
        /// <param name="name">The attribute's name, one of the allowed names.</param>
        /// <param name="value">The attribute's value.</param>
        private static void AppendAttribute( StringBuilder attributes, string name, string value )
        {
            attributes.Append( ' ' ).Append( name ).Append( "=\"" ).Append( WebUtility.HtmlEncode( value ) ).Append( '"' );
        }
    }
}

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
using System.Text;
using System.Text.RegularExpressions;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace Rock.Utility.HtmlSanitization
{
    /// <summary>
    /// The v17 allowlist sanitizer, built directly on AngleSharp. It parses the
    /// input as a document body the way Ganss.Xss does, removes everything the
    /// options and the fixed rules do not allow, and serializes the body content.
    /// </summary>
    /// <remarks>
    /// Stateless and safe to call concurrently: each call creates its own
    /// parser, and the options are read-only once a call starts.
    /// </remarks>
    internal sealed class HtmlAllowListSanitizer : IHtmlAllowListSanitizer
    {
        #region Fields

        private const string DocumentPrefix = "<!doctype html><html><body>";

        /// <summary>
        /// Input without any of these characters serializes to itself, so it
        /// skips the parser.
        /// </summary>
        private static readonly char[] MarkupCharacters = { '<', '>', '&', ' ', '\0' };

        #endregion

        #region Methods

        /// <inheritdoc/>
        public string Sanitize( string html, HtmlSanitizeOptions options )
        {
            if ( options == null )
            {
                throw new ArgumentNullException( nameof( options ) );
            }

            options.MakeReadOnly();

            if ( string.IsNullOrEmpty( html ) || html.IndexOfAny( MarkupCharacters ) < 0 )
            {
                return html;
            }

            if ( IsTooDeeplyNested( html ) )
            {
                return EncodeAsText( html );
            }

            // AngleSharp does not guarantee HtmlParser is thread-safe, so each call
            // creates its own. That is about 40% of a small sanitize; pooling parsers
            // (or another reuse pattern) would be a future performance boost.
            var parser = new HtmlParser( new HtmlParserOptions { IsScripting = true } );

            using ( var document = parser.ParseDocument( DocumentPrefix + html ) )
            {
                var body = document.Body;

                SanitizeChildNodes( body, options, 1 );

                return body.ChildNodes.ToHtml( HtmlMarkupFormatter.Instance );
            }
        }

        /// <summary>
        /// Estimates, without parsing, whether the input nests elements deeper
        /// than <see cref="HtmlAllowListTables.MaxEstimatedNestingDepth"/>. The
        /// scan follows the tokenizer closely enough that markup can not hide
        /// its depth: end tags close only an open element of the same name,
        /// quoted attribute values are skipped, and comments end where the
        /// parser ends them. It overestimates rather than underestimates.
        /// </summary>
        /// <param name="html">The input HTML.</param>
        /// <returns><c>true</c> if the input is too deeply nested to parse.</returns>
        private static bool IsTooDeeplyNested( string html )
        {
            var openElements = new List<string>();
            var openCounts = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );
            var position = html.IndexOf( '<' );

            while ( position >= 0 && position < html.Length )
            {
                var next = position + 1 < html.Length ? html[position + 1] : '\0';

                if ( next == '!' )
                {
                    position = SkipComment( html, position );
                }
                else if ( next == '?' )
                {
                    position = SkipPast( html, ">", position + 2 );
                }
                else if ( next == '/' && position + 2 < html.Length && IsAsciiLetter( html[position + 2] ) )
                {
                    var nameEnd = FindTagNameEnd( html, position + 2 );
                    var name = html.Substring( position + 2, nameEnd - position - 2 );

                    if ( openCounts.TryGetValue( name, out var count ) && count > 0 )
                    {
                        while ( openElements.Count > 0 )
                        {
                            var top = openElements[openElements.Count - 1];

                            openElements.RemoveAt( openElements.Count - 1 );
                            openCounts[top]--;

                            if ( string.Equals( top, name, StringComparison.OrdinalIgnoreCase ) )
                            {
                                break;
                            }
                        }
                    }

                    position = SkipTag( html, nameEnd );
                }
                else if ( IsAsciiLetter( next ) )
                {
                    var nameEnd = FindTagNameEnd( html, position + 1 );
                    var name = html.Substring( position + 1, nameEnd - position - 1 );

                    if ( !HtmlAllowListTables.VoidElements.Contains( name ) && !HtmlAllowListTables.ImpliedEndElements.Contains( name ) )
                    {
                        openElements.Add( name );
                        openCounts[name] = openCounts.TryGetValue( name, out var count ) ? count + 1 : 1;

                        if ( openElements.Count > HtmlAllowListTables.MaxEstimatedNestingDepth )
                        {
                            return true;
                        }
                    }

                    position = SkipTag( html, nameEnd );
                }
                else
                {
                    position++;
                }

                position = position < html.Length ? html.IndexOf( '<', position ) : -1;
            }

            return false;
        }

        /// <summary>
        /// Skips a comment or other <c>&lt;!</c> construct the way the parser
        /// ends it: a comment at the first <c>--&gt;</c> or <c>--!&gt;</c>
        /// (or at once for <c>&lt;!--&gt;</c> and <c>&lt;!---&gt;</c>), and
        /// anything else, such as a doctype, at the first <c>&gt;</c>.
        /// </summary>
        /// <param name="html">The input HTML.</param>
        /// <param name="start">The position of the <c>&lt;</c>.</param>
        /// <returns>The position after the construct.</returns>
        private static int SkipComment( string html, int start )
        {
            if ( string.CompareOrdinal( html, start, "<!--", 0, 4 ) != 0 )
            {
                return SkipPast( html, ">", start + 2 );
            }

            if ( string.CompareOrdinal( html, start + 4, ">", 0, 1 ) == 0 )
            {
                return start + 5;
            }

            if ( string.CompareOrdinal( html, start + 4, "->", 0, 2 ) == 0 )
            {
                return start + 6;
            }

            var end = SkipPast( html, "-->", start + 4 );
            var bangEnd = SkipPast( html, "--!>", start + 4 );

            return Math.Min( end, bangEnd );
        }

        /// <summary>
        /// Skips the rest of a start or end tag, stepping over quoted attribute
        /// values so a <c>&gt;</c> or tag inside one is not mistaken for markup.
        /// </summary>
        /// <param name="html">The input HTML.</param>
        /// <param name="position">The position after the tag name.</param>
        /// <returns>The position after the tag's <c>&gt;</c>.</returns>
        private static int SkipTag( string html, int position )
        {
            while ( position < html.Length )
            {
                var c = html[position];

                if ( c == '>' )
                {
                    return position + 1;
                }

                position++;

                if ( c != '=' )
                {
                    continue;
                }

                while ( position < html.Length && IsHtmlWhiteSpace( html[position] ) )
                {
                    position++;
                }

                if ( position < html.Length && ( html[position] == '"' || html[position] == '\'' ) )
                {
                    var close = html.IndexOf( html[position], position + 1 );

                    position = close < 0 ? html.Length : close + 1;
                }
            }

            return html.Length;
        }

        /// <summary>
        /// Finds the end of a tag name: the first whitespace, <c>/</c> or <c>&gt;</c>.
        /// </summary>
        /// <param name="html">The input HTML.</param>
        /// <param name="position">The position of the name's first character.</param>
        /// <returns>The position after the name.</returns>
        private static int FindTagNameEnd( string html, int position )
        {
            while ( position < html.Length && html[position] != '/' && html[position] != '>' && !IsHtmlWhiteSpace( html[position] ) )
            {
                position++;
            }

            return position;
        }

        /// <summary>
        /// Finds the position after the next occurrence of <paramref name="value"/>.
        /// </summary>
        /// <param name="html">The input HTML.</param>
        /// <param name="value">The text to find.</param>
        /// <param name="position">Where to start looking.</param>
        /// <returns>The position after it, or the end of the input when it is not found.</returns>
        private static int SkipPast( string html, string value, int position )
        {
            var index = position < html.Length ? html.IndexOf( value, position, StringComparison.Ordinal ) : -1;

            return index < 0 ? html.Length : index + value.Length;
        }

        /// <summary>
        /// Determines whether a character is an ASCII letter, which starts a tag name.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns><c>true</c> if it is <c>a</c> to <c>z</c> or <c>A</c> to <c>Z</c>.</returns>
        private static bool IsAsciiLetter( char c )
        {
            return ( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );
        }

        /// <summary>
        /// Determines whether a character is HTML whitespace inside a tag.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns><c>true</c> for tab, line feed, form feed, carriage return and space.</returns>
        private static bool IsHtmlWhiteSpace( char c )
        {
            return c == '\t' || c == '\n' || c == '\f' || c == '\r' || c == ' ';
        }

        /// <summary>
        /// Encodes input that is too deeply nested to parse as plain text,
        /// exactly as the serializer would write a text node holding it, so
        /// sanitizing the result again returns the same string.
        /// </summary>
        /// <param name="html">The input HTML.</param>
        /// <returns>The input as escaped text.</returns>
        private static string EncodeAsText( string html )
        {
            var sb = new StringBuilder( html.Length + html.Length / 4 );

            for ( var i = 0; i < html.Length; i++ )
            {
                var c = html[i];

                switch ( c )
                {
                    case '&':
                        sb.Append( "&amp;" );
                        break;

                    case '<':
                        sb.Append( "&lt;" );
                        break;

                    case '>':
                        sb.Append( "&gt;" );
                        break;

                    case '\u00a0':
                        sb.Append( "&nbsp;" );
                        break;

                    case '\0':
                        break;

                    case '\r':
                        sb.Append( '\n' );
                        if ( i + 1 < html.Length && html[i + 1] == '\n' )
                        {
                            i++;
                        }

                        break;

                    default:
                        sb.Append( c );
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sanitizes the child nodes of <paramref name="parent"/>, depth first.
        /// </summary>
        /// <param name="parent">The node whose children are sanitized.</param>
        /// <param name="options">The allow rules.</param>
        /// <param name="depth">How deep the children are nested below the body; the body's own children are at 1.</param>
        private static void SanitizeChildNodes( INode parent, HtmlSanitizeOptions options, int depth )
        {
            foreach ( var node in parent.ChildNodes.ToArray() )
            {
                var element = node as IElement;

                if ( element != null && depth > HtmlAllowListTables.MaxNestingDepth )
                {
                    parent.RemoveChild( node );
                    continue;
                }

                switch ( GetNodeAction( node, element, parent, options ) )
                {
                    case NodeAction.Keep:
                        break;

                    case NodeAction.Clean:
                        SanitizeAttributes( element, options );
                        SanitizeChildNodes( element, options, depth + 1 );
                        break;

                    case NodeAction.Unwrap:
                        SanitizeChildNodes( element, options, depth + 1 );
                        while ( element.FirstChild != null )
                        {
                            parent.InsertBefore( element.FirstChild, element );
                        }

                        parent.RemoveChild( element );
                        break;

                    default:
                        parent.RemoveChild( node );
                        break;
                }
            }
        }

        /// <summary>
        /// Decides what happens to one node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="element">The node as an element, or <c>null</c> when it is not one.</param>
        /// <param name="parent">The node's parent.</param>
        /// <param name="options">The allow rules.</param>
        /// <returns>The action to take.</returns>
        private static NodeAction GetNodeAction( INode node, IElement element, INode parent, HtmlSanitizeOptions options )
        {
            if ( node.NodeType == NodeType.Text )
            {
                return NodeAction.Keep;
            }

            // Comments, processing instructions, doctypes and any other node kind.
            if ( element == null
                || HtmlAllowListTables.NeverPassElements.Contains( element.LocalName )
                || !IsNamespaceValid( element, parent as IElement ) )
            {
                return NodeAction.Remove;
            }

            if ( options.IsTagAllowed( element.LocalName ) )
            {
                return NodeAction.Clean;
            }

            // Unwrapping inside SVG or MathML could move children into a
            // namespace the parser would not have put them in.
            if ( element.NamespaceUri != NamespaceNames.HtmlUri )
            {
                return NodeAction.Remove;
            }

            return options.GetRemovalMode( element.LocalName ) == HtmlTagRemovalMode.Unwrap
                ? NodeAction.Unwrap
                : NodeAction.Remove;
        }

        /// <summary>
        /// Determines whether an element is in a namespace its parent could
        /// have produced, the way DOMPurify checks namespaces. AngleSharp can
        /// build trees, such as an HTML <c>p</c> inside <c>svg</c>, that a
        /// browser parses differently once serialized, which is how
        /// namespace-confusion attacks get past sanitizers.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="parent">The element's parent, or <c>null</c> for the body's children.</param>
        /// <returns><c>true</c> if the element may stay where it is.</returns>
        private static bool IsNamespaceValid( IElement element, IElement parent )
        {
            var parentNamespace = parent?.NamespaceUri ?? NamespaceNames.HtmlUri;
            var parentName = parent?.LocalName ?? string.Empty;

            if ( element.NamespaceUri == NamespaceNames.HtmlUri )
            {
                if ( parentNamespace == NamespaceNames.SvgUri )
                {
                    return HtmlAllowListTables.SvgHtmlIntegrationPoints.Contains( parentName );
                }

                if ( parentNamespace == NamespaceNames.MathMlUri )
                {
                    return HtmlAllowListTables.MathMlTextIntegrationPoints.Contains( parentName );
                }

                return true;
            }

            if ( HtmlAllowListTables.ForeignContentBreakoutElements.Contains( element.LocalName ) )
            {
                return false;
            }

            if ( element.NamespaceUri == NamespaceNames.SvgUri )
            {
                if ( parentNamespace == NamespaceNames.SvgUri )
                {
                    return true;
                }

                return element.LocalName == "svg"
                    && ( parentNamespace == NamespaceNames.HtmlUri || HtmlAllowListTables.MathMlTextIntegrationPoints.Contains( parentName ) );
            }

            if ( element.NamespaceUri == NamespaceNames.MathMlUri )
            {
                return parentNamespace == NamespaceNames.MathMlUri
                    || ( element.LocalName == "math" && parentNamespace == NamespaceNames.HtmlUri );
            }

            return false;
        }

        /// <summary>
        /// Removes or rewrites the attributes of an allowed element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="options">The allow rules.</param>
        private static void SanitizeAttributes( IElement element, HtmlSanitizeOptions options )
        {
            foreach ( var attribute in element.Attributes.ToArray() )
            {
                var value = GetSafeAttributeValue( element.LocalName, attribute.Name, attribute.Value, options );

                if ( value == null )
                {
                    element.RemoveAttribute( attribute.Name );
                }
                else if ( value != attribute.Value )
                {
                    // Setting the value directly avoids SetAttribute's name validation.
                    attribute.Value = value;
                }
            }

            if ( string.Equals( element.GetAttribute( "target" ), "_blank", StringComparison.OrdinalIgnoreCase ) )
            {
                element.SetAttribute( "rel", "noopener noreferrer" );
            }
        }

        /// <summary>
        /// Gets the value an attribute keeps after sanitizing.
        /// </summary>
        /// <param name="tag">The element's tag name.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <param name="options">The allow rules.</param>
        /// <returns>The value to keep, or <c>null</c> to remove the attribute.</returns>
        private static string GetSafeAttributeValue( string tag, string name, string value, HtmlSanitizeOptions options )
        {
            if ( name.StartsWith( "on", StringComparison.OrdinalIgnoreCase )
                || !options.IsAttributeAllowed( tag, name )
                || value.Contains( "&{" ) )
            {
                return null;
            }

            if ( HtmlAllowListTables.UrlAttributes.Contains( name ) )
            {
                return GetSafeUrl( value, options );
            }

            if ( string.Equals( name, "style", StringComparison.OrdinalIgnoreCase ) )
            {
                return GetSafeStyle( value, options );
            }

            if ( string.Equals( name, "target", StringComparison.OrdinalIgnoreCase ) )
            {
                return HtmlAllowListTables.AllowedTargets.Contains( value ) ? value : null;
            }

            if ( string.Equals( name, "name", StringComparison.OrdinalIgnoreCase )
                || string.Equals( name, "id", StringComparison.OrdinalIgnoreCase ) )
            {
                return IsSafeFormMemberName( value ) ? value : null;
            }

            return value;
        }

        /// <summary>
        /// Determines whether a <c>name</c> or <c>id</c> value can not hide a
        /// method of, or inject a value into, the page-wide WebForms form.
        /// </summary>
        /// <param name="value">The attribute value.</param>
        /// <returns><c>true</c> if the value is safe to keep.</returns>
        private static bool IsSafeFormMemberName( string value )
        {
            return value.IndexOf( '$' ) < 0
                && !value.StartsWith( "__", StringComparison.Ordinal )
                && !HtmlAllowListTables.FormMethodNames.Contains( value );
        }

        /// <summary>
        /// Applies the URL rule, copied from Ganss.Xss <c>GetSafeIri</c>.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="options">The allow rules.</param>
        /// <returns>The URL without leading whitespace, or <c>null</c> when its scheme is not allowed.</returns>
        private static string GetSafeUrl( string url, HtmlSanitizeOptions options )
        {
            url = url.TrimStart();

            var match = HtmlAllowListTables.SchemeRegex.Match( url );

            if ( match.Success && !options.IsUrlSchemeAllowed( match.Groups[1].Value ) )
            {
                return null;
            }

            return url;
        }

        /// <summary>
        /// Applies the CSS rule to a <c>style</c> attribute value.
        /// </summary>
        /// <param name="style">The attribute value.</param>
        /// <param name="options">The allow rules.</param>
        /// <returns>The kept declarations joined by <c>"; "</c>, or <c>null</c> when none is kept.</returns>
        private static string GetSafeStyle( string style, HtmlSanitizeOptions options )
        {
            var declarations = new List<string>();

            foreach ( var declaration in SplitCssDeclarations( style ) )
            {
                var safeDeclaration = GetSafeCssDeclaration( declaration, options );

                if ( safeDeclaration != null )
                {
                    declarations.Add( safeDeclaration );
                }
            }

            return declarations.Count > 0 ? string.Join( "; ", declarations ) : null;
        }

        /// <summary>
        /// Splits a <c>style</c> value on the semicolons outside quotes and
        /// brackets. Malformed declarations are skipped, so every returned
        /// declaration stands on its own when the kept ones are joined again.
        /// </summary>
        /// <param name="style">The attribute value.</param>
        /// <returns>The well-formed declarations.</returns>
        private static List<string> SplitCssDeclarations( string style )
        {
            var declarations = new List<string>();
            var closers = new Stack<char>();
            var quote = '\0';
            var isMalformed = false;
            var start = 0;

            for ( var i = 0; i <= style.Length; i++ )
            {
                var isEnd = i == style.Length;

                if ( isEnd || ( style[i] == ';' && quote == '\0' && closers.Count == 0 ) )
                {
                    // An unclosed quote or bracket, a mismatched bracket or a
                    // newline in a string would make the browser end this
                    // declaration somewhere other than where we split it.
                    if ( !isMalformed && quote == '\0' && closers.Count == 0 )
                    {
                        declarations.Add( style.Substring( start, i - start ) );
                    }

                    closers.Clear();
                    quote = '\0';
                    isMalformed = false;
                    start = i + 1;
                    continue;
                }

                var c = style[i];

                if ( quote != '\0' )
                {
                    if ( c == quote )
                    {
                        quote = '\0';
                    }
                    else if ( c == '\n' || c == '\r' || c == '\f' )
                    {
                        isMalformed = true;
                        quote = '\0';
                    }
                }
                else if ( c == '"' || c == '\'' )
                {
                    quote = c;
                }
                else if ( c == '(' )
                {
                    closers.Push( ')' );
                }
                else if ( c == '[' )
                {
                    closers.Push( ']' );
                }
                else if ( c == '{' )
                {
                    closers.Push( '}' );
                }
                else if ( c == ')' || c == ']' || c == '}' )
                {
                    if ( closers.Count == 0 || closers.Pop() != c )
                    {
                        isMalformed = true;
                    }
                }
            }

            return declarations;
        }

        /// <summary>
        /// Applies the CSS rule to one declaration.
        /// </summary>
        /// <param name="declaration">The declaration text, without its semicolon.</param>
        /// <param name="options">The allow rules.</param>
        /// <returns>The declaration as <c>property: value</c>, or <c>null</c> when it is removed.</returns>
        private static string GetSafeCssDeclaration( string declaration, HtmlSanitizeOptions options )
        {
            var colonIndex = declaration.IndexOf( ':' );

            if ( colonIndex < 0 )
            {
                return null;
            }

            var property = declaration.Substring( 0, colonIndex ).Trim().ToLowerInvariant();
            var value = declaration.Substring( colonIndex + 1 ).Trim();

            if ( value.Length == 0
                || !options.IsCssPropertyAllowed( property )
                || IsFixedPosition( property, value )
                || HtmlAllowListTables.RejectedCssValueText.Any( text => value.IndexOf( text, StringComparison.Ordinal ) >= 0 )
                || HtmlAllowListTables.CssExpression.IsMatch( value ) )
            {
                return null;
            }

            foreach ( Match match in HtmlAllowListTables.CssUrl.Matches( value ) )
            {
                var url = RemoveWhitespace( match.Groups[2].Value );

                if ( url.Length > 0 && GetSafeUrl( url, options ) == null )
                {
                    return null;
                }
            }

            return $"{property}: {value}";
        }

        /// <summary>
        /// Determines whether a declaration is <c>position: fixed</c>, which
        /// would pin an element over the whole page wherever it is scrolled.
        /// </summary>
        /// <param name="property">The lower-cased property name.</param>
        /// <param name="value">The declaration value.</param>
        /// <returns><c>true</c> if the declaration fixes the element's position.</returns>
        private static bool IsFixedPosition( string property, string value )
        {
            if ( property != "position" )
            {
                return false;
            }

            var keyword = value.Replace( "!important", string.Empty ).Trim();

            return string.Equals( keyword, "fixed", StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Removes all whitespace from a string.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The string without whitespace.</returns>
        private static string RemoveWhitespace( string value )
        {
            var sb = new StringBuilder( value.Length );

            foreach ( var c in value )
            {
                if ( !char.IsWhiteSpace( c ) )
                {
                    sb.Append( c );
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// What the walk does with one node.
        /// </summary>
        private enum NodeAction
        {
            /// <summary>
            /// Removes the node and its content.
            /// </summary>
            Remove,

            /// <summary>
            /// Keeps the node as it is.
            /// </summary>
            Keep,

            /// <summary>
            /// Keeps the element, sanitizes its attributes and walks its children.
            /// </summary>
            Clean,

            /// <summary>
            /// Walks the element's children, then replaces the element with them.
            /// </summary>
            Unwrap
        }

        #endregion
    }
}

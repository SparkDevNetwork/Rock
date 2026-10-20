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
using System.Text.RegularExpressions;

namespace Rock.Utility.HtmlSanitization
{
    /// <summary>
    /// The data behind the allowlist sanitizer: the preset allowlists used to
    /// build <see cref="HtmlSanitizeOptions.Simple"/> and <see cref="HtmlSanitizeOptions.Rich"/>,
    /// and the fixed rules that no options object can loosen. These lists are
    /// the source of truth; the TypeScript tables in
    /// htmlSanitizationTables.partial.ts must match them, and the Jest tests
    /// compare the two.
    /// </summary>
    internal static class HtmlAllowListTables
    {
        #region Preset data

        /// <summary>
        /// The elements allowed by both presets.
        /// </summary>
        internal static readonly HashSet<string> SimpleElements = CreateSet(
            "span", "b", "strong", "i", "em", "u", "s", "strike", "small", "sub", "sup", "mark", "abbr", "q", "cite", "code", "kbd", "samp", "var",
            "a",
            "p", "br", "hr", "blockquote", "pre",
            "div", "center" );

        /// <summary>
        /// The elements allowed only by the Rich preset.
        /// </summary>
        internal static readonly HashSet<string> RichOnlyElements = CreateSet(
            "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li", "dl", "dt", "dd",
            "section", "article", "header", "footer", "aside", "nav", "main", "figure", "figcaption", "time", "details", "summary", "hgroup",
            "table", "thead", "tbody", "tfoot", "tr", "th", "td", "caption", "colgroup", "col",
            "img",
            "label", "input", "button", "select", "option", "optgroup", "textarea",
            "svg", "g", "path", "circle", "ellipse", "rect", "line", "polyline", "polygon" );

        /// <summary>
        /// The attributes allowed on every tag by both presets.
        /// </summary>
        internal static readonly HashSet<string> SimpleEveryTagAttributes = CreateSet(
            "class", "title", "dir", "lang", "style", "tabindex",
            "role", "aria-label", "aria-hidden", "aria-expanded", "aria-haspopup", "aria-controls", "aria-describedby", "aria-labelledby",
            "aria-valuemin", "aria-valuemax", "aria-valuenow" );

        /// <summary>
        /// The attributes allowed on every tag only by the Rich preset. The
        /// Rich preset also allows <c>data-*</c> attributes.
        /// </summary>
        internal static readonly HashSet<string> RichOnlyEveryTagAttributes = CreateSet(
            "id" );

        /// <summary>
        /// The tag-specific attributes allowed by both presets, keyed by tag.
        /// </summary>
        internal static readonly Dictionary<string, HashSet<string>> SimpleTagAttributes = new Dictionary<string, HashSet<string>>( StringComparer.OrdinalIgnoreCase )
        {
            ["a"] = CreateSet( "href", "target", "rel" )
        };

        /// <summary>
        /// The tag-specific attributes allowed only by the Rich preset, keyed by tag.
        /// </summary>
        internal static readonly Dictionary<string, HashSet<string>> RichOnlyTagAttributes = new Dictionary<string, HashSet<string>>( StringComparer.OrdinalIgnoreCase )
        {
            ["a"] = CreateSet( "name" ),
            ["img"] = CreateSet( "src", "alt", "width", "height", "loading" ),
            ["td"] = CreateSet( "colspan", "rowspan", "headers", "scope", "align", "valign" ),
            ["th"] = CreateSet( "colspan", "rowspan", "headers", "scope", "align", "valign" ),
            ["col"] = CreateSet( "span" ),
            ["colgroup"] = CreateSet( "span" ),
            ["ol"] = CreateSet( "start", "type", "reversed" ),
            ["time"] = CreateSet( "datetime" ),
            ["label"] = CreateSet( "for" ),
            ["input"] = CreateSet( "name", "type", "value", "placeholder", "disabled", "readonly", "checked", "multiple", "size", "maxlength", "minlength", "min", "max", "step", "required", "autofocus" ),
            ["button"] = CreateSet( "name", "type", "value", "disabled", "autofocus" ),
            ["select"] = CreateSet( "name", "disabled", "multiple", "size", "required", "autofocus" ),
            ["option"] = CreateSet( "value", "selected", "disabled", "label" ),
            ["optgroup"] = CreateSet( "label", "disabled" ),
            ["textarea"] = CreateSet( "name", "placeholder", "disabled", "readonly", "rows", "cols", "maxlength", "minlength", "required", "autofocus" ),
            ["svg"] = CreateSvgSet( "viewBox", "width", "height", "preserveAspectRatio" ),
            ["g"] = CreateSvgSet(),
            ["path"] = CreateSvgSet( "d" ),
            ["circle"] = CreateSvgSet( "cx", "cy", "r" ),
            ["ellipse"] = CreateSvgSet( "cx", "cy", "rx", "ry" ),
            ["rect"] = CreateSvgSet( "x", "y", "width", "height", "rx", "ry" ),
            ["line"] = CreateSvgSet( "x1", "y1", "x2", "y2" ),
            ["polyline"] = CreateSvgSet( "points" ),
            ["polygon"] = CreateSvgSet( "points" )
        };

        /// <summary>
        /// The URL schemes allowed by both presets.
        /// </summary>
        internal static readonly HashSet<string> UrlSchemes = CreateSet(
            "http", "https", "mailto", "tel" );

        /// <summary>
        /// The CSS properties allowed by both presets.
        /// </summary>
        internal static readonly HashSet<string> SimpleCssProperties = CreateSet(
            "color",
            "background", "background-color", "background-image", "background-position", "background-repeat", "background-size",
            "font", "font-family", "font-size", "font-style", "font-variant", "font-weight",
            "text-align", "text-indent", "text-transform", "text-shadow", "text-overflow",
            "text-decoration", "text-decoration-line", "text-decoration-color", "text-decoration-style", "text-decoration-thickness",
            "line-height", "letter-spacing", "word-spacing", "vertical-align", "white-space", "word-break", "overflow-wrap", "word-wrap",
            "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
            "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
            "border", "border-width", "border-style", "border-color",
            "border-top", "border-top-width", "border-top-style", "border-top-color",
            "border-right", "border-right-width", "border-right-style", "border-right-color",
            "border-bottom", "border-bottom-width", "border-bottom-style", "border-bottom-color",
            "border-left", "border-left-width", "border-left-style", "border-left-color",
            "border-radius", "border-top-left-radius", "border-top-right-radius", "border-bottom-right-radius", "border-bottom-left-radius",
            "width", "height", "min-width", "min-height", "max-width", "max-height", "box-sizing",
            "display", "float", "clear",
            "list-style", "list-style-type", "list-style-position",
            "box-shadow", "opacity", "visibility" );

        /// <summary>
        /// The CSS properties allowed only by the Rich preset.
        /// </summary>
        internal static readonly HashSet<string> RichOnlyCssProperties = CreateSet(
            "position", "top", "right", "bottom", "left", "z-index", "overflow", "overflow-x", "overflow-y", "transform",
            "border-collapse", "border-spacing", "table-layout", "caption-side", "empty-cells",
            "flex", "flex-grow", "flex-shrink", "flex-basis", "flex-direction", "flex-wrap",
            "justify-content", "align-items", "align-self", "gap", "row-gap", "column-gap", "order" );

        #endregion

        #region Fixed rules

        /// <summary>
        /// The elements that are always removed with their content, under any
        /// options, any removal mode and any namespace. Each entry says why.
        /// </summary>
        internal static readonly HashSet<string> NeverPassElements = CreateSet(
            // Runs JavaScript.
            "script",
            // A page-wide stylesheet: it can restyle Rock's own UI and, with
            // selectors plus url(), leak page data. Making its content safe
            // needs a CSS parser, which v17 does not have.
            "style",
            // Embeds another document or site, and srcdoc runs script.
            "iframe",
            // Loads a plugin or document (PDF, HTML, Flash) that can run code.
            "object",
            // Same as object.
            "embed",
            // Its action posts page data to another site, and a form inside
            // Rock's page-wide server form parses differently in the browser
            // than it did here.
            "form",
            // Loads an external stylesheet, with the same risks as style.
            "link",
            // http-equiv can redirect or refresh the page.
            "meta",
            // Changes the base URL of every relative link and script on the page.
            "base",
            // Its content is text with scripting on and markup with scripting
            // off, a known mutation-XSS source.
            "noscript",
            // Its content sits in a separate fragment that script can clone
            // into the page later.
            "template",
            // Same as iframe.
            "frame",
            // Replaces the page body with frames.
            "frameset",
            // Runs a Java applet.
            "applet",
            // Raw-text element: its content is parsed as text, so markup
            // inside it would be serialized unescaped.
            "xmp",
            // Raw-text element, parsed differently depending on plugin support.
            "noembed",
            // Raw-text element, parsed differently depending on frame support.
            "noframes",
            // Turns the rest of the page into text; it has no end tag.
            "plaintext",
            // Sets the page title; its content is text in HTML but markup in
            // SVG, a namespace-confusion point. Never useful in body content.
            "title",
            // SVG animation that rewrites an attribute, such as href to
            // javascript:, while the page runs, after the sanitizer's checks.
            "animate",
            // Same as animate.
            "set",
            // Switches back to HTML parsing inside SVG, a namespace-confusion point.
            "foreignObject",
            // Switches back to HTML parsing inside MathML, a namespace-confusion point.
            "annotation-xml" );

        /// <summary>
        /// The deepest an element may be nested below the body. A deeper
        /// element is removed with its content, under any options and any
        /// removal mode. This bounds the sanitizer's recursion so hostile
        /// input can not exhaust the stack, and stays below the depth at
        /// which browsers flatten the tree, so the sanitizer and the browser
        /// build the same tree. Real templates rarely pass 24 levels.
        /// </summary>
        internal const int MaxNestingDepth = 128;

        /// <summary>
        /// The nesting depth, estimated by a quick scan before parsing, past
        /// which the input is not parsed at all and comes back encoded as text.
        /// Deeply nested input is slow to parse. Set well above
        /// <see cref="MaxNestingDepth"/> so real content never reaches it.
        /// </summary>
        internal const int MaxEstimatedNestingDepth = 1024;

        /// <summary>
        /// The void elements, which never have content or an end tag, so the
        /// pre-parse scan does not count them as nesting.
        /// </summary>
        internal static readonly HashSet<string> VoidElements = CreateSet(
            "area", "base", "basefont", "bgsound", "br", "col", "embed", "frame", "hr", "img", "input", "keygen",
            "link", "meta", "param", "source", "track", "wbr" );

        /// <summary>
        /// The elements whose end tag the parser implies when a sibling starts
        /// (a new <c>li</c> closes the previous one, and so on). The pre-parse
        /// scan does not count them as nesting, so long lists and tables
        /// written without end tags do not look deep.
        /// </summary>
        internal static readonly HashSet<string> ImpliedEndElements = CreateSet(
            "p", "li", "dt", "dd", "td", "th", "tr", "thead", "tbody", "tfoot", "option", "optgroup",
            "rb", "rt", "rtc", "rp" );

        /// <summary>
        /// The elements that start foreign content (SVG and MathML). Options
        /// can allow them, but when they are not allowed they are always
        /// removed with their content: unwrapping them would move SVG or
        /// MathML elements into HTML, where the browser parses the output
        /// differently than the sanitizer saw it.
        /// </summary>
        internal static readonly HashSet<string> DropOnlyElements = CreateSet(
            "svg", "math" );

        /// <summary>
        /// The SVG elements whose children are parsed as HTML. The other two,
        /// <c>foreignObject</c> and <c>title</c>, never pass.
        /// Defined by the HTML Standard as "HTML integration points".
        /// </summary>
        internal static readonly HashSet<string> SvgHtmlIntegrationPoints = CreateSet(
            "desc" );

        /// <summary>
        /// The MathML elements whose children are parsed as HTML.
        /// Defined by the HTML Standard as "MathML text integration points".
        /// </summary>
        internal static readonly HashSet<string> MathMlTextIntegrationPoints = CreateSet(
            "mi", "mo", "mn", "ms", "mtext" );

        /// <summary>
        /// The start tags that end SVG or MathML content in a browser. An SVG
        /// or MathML element with one of these names can not come back from a
        /// browser parse, so it is removed.
        /// The list is from the HTML Standard, "parsing tokens in foreign content".
        /// </summary>
        internal static readonly HashSet<string> ForeignContentBreakoutElements = CreateSet(
            "b", "big", "blockquote", "body", "br", "center", "code", "dd", "div", "dl", "dt", "em", "embed",
            "h1", "h2", "h3", "h4", "h5", "h6", "head", "hr", "i", "img", "li", "listing", "menu", "meta",
            "nobr", "ol", "p", "pre", "ruby", "s", "small", "span", "strong", "strike", "sub", "sup", "table",
            "tt", "u", "ul", "var", "font" );

        /// <summary>
        /// The attributes that no options object can allow, beyond <c>on*</c>
        /// event handlers. <c>formaction</c> would send the page's server form,
        /// which wraps every Rock page, to another site when an injected button
        /// is clicked; <c>srcdoc</c> only works on <c>iframe</c>.
        /// </summary>
        internal static readonly HashSet<string> NeverAllowedAttributes = CreateSet(
            "formaction", "srcdoc" );

        /// <summary>
        /// The method names of a <c>form</c> element, including inherited
        /// ones. A control or image whose <c>name</c> or <c>id</c> equals one
        /// of them hides that method on its form, so a call such as
        /// <c>theForm.submit()</c> in WebForms' <c>__doPostBack</c> throws
        /// and every postback on the page stops working. The sanitizer removes
        /// a <c>name</c> or <c>id</c> with one of these values, a value starting
        /// with <c>__</c> (ASP.NET's hidden fields such as <c>__VIEWSTATE</c>),
        /// or a value containing <c>$</c> (nested WebForms control names, which
        /// would inject values into the page's own postback). Compared
        /// case-sensitively, as the DOM does. Generated from the function-valued
        /// members of <c>HTMLFormElement.prototype</c> and its prototype chain
        /// in Chrome 152. This defends the page-wide WebForms form; it can be
        /// removed once Rock no longer renders pages inside one.
        /// </summary>
        internal static readonly HashSet<string> FormMethodNames = new HashSet<string>( StringComparer.Ordinal )
        {
            "addEventListener", "after", "animate", "append", "appendChild", "ariaNotify", "attachInternals", "attachShadow",
            "before", "blur", "checkValidity", "checkVisibility", "click", "cloneNode", "closest", "compareDocumentPosition",
            "computedStyleMap", "constructor", "contains", "dispatchEvent", "focus", "getAnimations", "getAttribute",
            "getAttributeNS", "getAttributeNames", "getAttributeNode", "getAttributeNodeNS", "getBoundingClientRect",
            "getClientRects", "getElementsByClassName", "getElementsByTagName", "getElementsByTagNameNS", "getHTML",
            "getRootNode", "hasAttribute", "hasAttributeNS", "hasAttributes", "hasChildNodes", "hasOwnProperty",
            "hasPointerCapture", "hidePopover", "insertAdjacentElement", "insertAdjacentHTML", "insertAdjacentText",
            "insertBefore", "isDefaultNamespace", "isEqualNode", "isPrototypeOf", "isSameNode", "lookupNamespaceURI",
            "lookupPrefix", "matches", "moveBefore", "normalize", "prepend", "propertyIsEnumerable", "pseudo", "querySelector",
            "querySelectorAll", "releasePointerCapture", "remove", "removeAttribute", "removeAttributeNS", "removeAttributeNode",
            "removeChild", "removeEventListener", "replaceChild", "replaceChildren", "replaceWith", "reportValidity",
            "requestFullscreen", "requestPointerLock", "requestSubmit", "reset", "scroll", "scrollBy", "scrollIntoView",
            "scrollIntoViewIfNeeded", "scrollTo", "setAttribute", "setAttributeNS", "setAttributeNode", "setAttributeNodeNS",
            "setHTML", "setHTMLUnsafe", "setPointerCapture", "showPopover", "startViewTransition", "submit", "toLocaleString",
            "toString", "toggleAttribute", "togglePopover", "valueOf", "webkitMatchesSelector", "webkitRequestFullScreen",
            "webkitRequestFullscreen", "when"
        };

        /// <summary>
        /// The URL schemes that no options object can allow.
        /// </summary>
        internal static readonly HashSet<string> BlockedUrlSchemes = CreateSet(
            "javascript", "vbscript", "file", "data" );

        /// <summary>
        /// The CSS properties that no options object can allow.
        /// </summary>
        internal static readonly HashSet<string> NeverAllowedCssProperties = CreateSet(
            "behavior", "-moz-binding" );

        /// <summary>
        /// Text that removes a CSS declaration when its raw value contains it.
        /// <c>&lt;</c> and <c>&gt;</c> match Ganss.Xss's default <c>DisallowCssPropertyValue</c>.
        /// </summary>
        internal static readonly string[] RejectedCssValueText = { "\\", "@", "<", ">", "/*", "*/" };

        /// <summary>
        /// The attributes whose value is a URL and gets the URL rule.
        /// Copied from Ganss.Xss <c>HtmlSanitizerDefaults.UriAttributes</c>.
        /// </summary>
        internal static readonly HashSet<string> UrlAttributes = CreateSet(
            "action", "background", "cite", "codebase", "data", "dynsrc", "formaction", "href", "icon",
            "longdesc", "lowsrc", "manifest", "poster", "src", "xlink:href" );

        /// <summary>
        /// The attributes whose value is a list of URLs. The v17 sanitizer does
        /// not check list entries, so these can never be allowed.
        /// Copied from Ganss.Xss <c>HtmlSanitizerDefaults.UriListAttributes</c>.
        /// </summary>
        internal static readonly HashSet<string> UrlListAttributes = CreateSet(
            "archive", "ping", "srcset" );

        /// <summary>
        /// The <c>target</c> values that are kept, compared case-insensitively.
        /// </summary>
        internal static readonly HashSet<string> AllowedTargets = CreateSet(
            "_blank", "_self", "_parent", "_top" );

        /// <summary>
        /// Matches the scheme of a URL: the text before the first colon (or its
        /// numeric character reference) when no slash or hash comes before it.
        /// Copied from Ganss.Xss v9.2.1039 <c>HtmlSanitizer.SchemeRegex</c>.
        /// </summary>
        internal static readonly Regex SchemeRegex = new Regex( @"^([^\/#]*?)(?:\:|&#0*58|&#x0*3a)", RegexOptions.Compiled | RegexOptions.IgnoreCase );

        /// <summary>
        /// Matches the CSS <c>expression</c> keyword, including its full-width
        /// Unicode lookalikes.
        /// Copied from Ganss.Xss v9.2.1039 <c>HtmlSanitizer.CssExpression</c>.
        /// </summary>
        internal static readonly Regex CssExpression = new Regex( @"[eEＥｅ][xXＸｘ][pPＰｐ][rRʀＲｒ][eEＥｅ][sSＳｓ]{2}[iIɪＩｉ][oOＯｏ][nNɴＮｎ]", RegexOptions.Compiled );

        /// <summary>
        /// Matches a CSS <c>url(...)</c>; group 2 is the URL text.
        /// Copied from Ganss.Xss v9.2.1039 <c>HtmlSanitizer.CssUrl</c>.
        /// </summary>
        internal static readonly Regex CssUrl = new Regex( @"[Uu][Rrʀ][Llʟ](?>\s*)\((?>\s*)(['""]?)([^'"")]*)(['""]?)(?>\s*)\)?", RegexOptions.Compiled );

        #endregion

        /// <summary>
        /// Creates a case-insensitive set of the given names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>The new set.</returns>
        private static HashSet<string> CreateSet( params string[] names )
        {
            return new HashSet<string>( names, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Creates the attribute set for an SVG icon element: the paint
        /// attributes every icon element takes, plus its own geometry.
        /// </summary>
        /// <param name="geometryAttributes">The element's geometry attributes.</param>
        /// <returns>The new set.</returns>
        private static HashSet<string> CreateSvgSet( params string[] geometryAttributes )
        {
            var attributes = CreateSet( "fill", "fill-opacity", "fill-rule", "clip-rule", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin", "stroke-miterlimit", "stroke-opacity", "opacity", "transform" );

            attributes.UnionWith( geometryAttributes );

            return attributes;
        }
    }
}

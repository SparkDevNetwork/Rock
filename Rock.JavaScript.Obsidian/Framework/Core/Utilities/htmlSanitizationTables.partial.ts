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

// NOTICE!!!
// This file has a C# version in the Rock project (HtmlAllowListTables.cs).
// If changes are made to one the same changes must be made to the other.
// The Jest tests compare the two.
// NOTICE!!!

/**
 * The characters .NET's `char.IsWhiteSpace` and regex `\s` match, as a
 * regular expression character class. JavaScript's `\s` differs: it adds
 * U+FEFF and leaves out U+0085.
 */
export const whiteSpaceClass = "[\\t\\n\\v\\f\\r \\u0085\\u00a0\\u1680\\u2000-\\u200a\\u2028\\u2029\\u202f\\u205f\\u3000]";

function createSet(...names: string[]): Set<string> {
    return new Set(names.map(name => name.toLowerCase()));
}

/**
 * Creates the attribute set for an SVG icon element: the paint attributes
 * every icon element takes, plus its own geometry.
 */
function createSvgSet(...geometryAttributes: string[]): Set<string> {
    return createSet("fill", "fill-opacity", "fill-rule", "clip-rule", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin", "stroke-miterlimit", "stroke-opacity", "opacity", "transform", ...geometryAttributes);
}

// #region Preset data

/** The elements allowed by both presets. */
export const simpleElements = createSet(
    "span", "b", "strong", "i", "em", "u", "s", "strike", "small", "sub", "sup", "mark", "abbr", "q", "cite", "code", "kbd", "samp", "var",
    "a",
    "p", "br", "hr", "blockquote", "pre",
    "div", "center");

/** The elements allowed only by the Rich preset. */
export const richOnlyElements = createSet(
    "h1", "h2", "h3", "h4", "h5", "h6",
    "ul", "ol", "li", "dl", "dt", "dd",
    "section", "article", "header", "footer", "aside", "nav", "main", "figure", "figcaption", "time", "details", "summary", "hgroup",
    "table", "thead", "tbody", "tfoot", "tr", "th", "td", "caption", "colgroup", "col",
    "img",
    "label", "input", "button", "select", "option", "optgroup", "textarea",
    "svg", "g", "path", "circle", "ellipse", "rect", "line", "polyline", "polygon");

/** The attributes allowed on every tag by both presets. */
export const simpleEveryTagAttributes = createSet(
    "class", "title", "dir", "lang", "style", "tabindex",
    "role", "aria-label", "aria-hidden", "aria-expanded", "aria-haspopup", "aria-controls", "aria-describedby", "aria-labelledby",
    "aria-valuemin", "aria-valuemax", "aria-valuenow");

/**
 * The attributes allowed on every tag only by the Rich preset. The Rich
 * preset also allows `data-*` attributes.
 */
export const richOnlyEveryTagAttributes = createSet(
    "id");

/** The tag-specific attributes allowed by both presets, keyed by tag. */
export const simpleTagAttributes: ReadonlyMap<string, Set<string>> = new Map([
    ["a", createSet("href", "target", "rel")]
]);

/** The tag-specific attributes allowed only by the Rich preset, keyed by tag. */
export const richOnlyTagAttributes: ReadonlyMap<string, Set<string>> = new Map([
    ["a", createSet("name")],
    ["img", createSet("src", "alt", "width", "height", "loading")],
    ["td", createSet("colspan", "rowspan", "headers", "scope", "align", "valign")],
    ["th", createSet("colspan", "rowspan", "headers", "scope", "align", "valign")],
    ["col", createSet("span")],
    ["colgroup", createSet("span")],
    ["ol", createSet("start", "type", "reversed")],
    ["time", createSet("datetime")],
    ["label", createSet("for")],
    ["input", createSet("name", "type", "value", "placeholder", "disabled", "readonly", "checked", "multiple", "size", "maxlength", "minlength", "min", "max", "step", "required", "autofocus")],
    ["button", createSet("name", "type", "value", "disabled", "autofocus")],
    ["select", createSet("name", "disabled", "multiple", "size", "required", "autofocus")],
    ["option", createSet("value", "selected", "disabled", "label")],
    ["optgroup", createSet("label", "disabled")],
    ["textarea", createSet("name", "placeholder", "disabled", "readonly", "rows", "cols", "maxlength", "minlength", "required", "autofocus")],
    ["svg", createSvgSet("viewBox", "width", "height", "preserveAspectRatio")],
    ["g", createSvgSet()],
    ["path", createSvgSet("d")],
    ["circle", createSvgSet("cx", "cy", "r")],
    ["ellipse", createSvgSet("cx", "cy", "rx", "ry")],
    ["rect", createSvgSet("x", "y", "width", "height", "rx", "ry")],
    ["line", createSvgSet("x1", "y1", "x2", "y2")],
    ["polyline", createSvgSet("points")],
    ["polygon", createSvgSet("points")]
]);

/** The URL schemes allowed by both presets. */
export const urlSchemes = createSet(
    "http", "https", "mailto", "tel");

/** The CSS properties allowed by both presets. */
export const simpleCssProperties = createSet(
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
    "box-shadow", "opacity", "visibility");

/** The CSS properties allowed only by the Rich preset. */
export const richOnlyCssProperties = createSet(
    "position", "top", "right", "bottom", "left", "z-index", "overflow", "overflow-x", "overflow-y", "transform",
    "border-collapse", "border-spacing", "table-layout", "caption-side", "empty-cells",
    "flex", "flex-grow", "flex-shrink", "flex-basis", "flex-direction", "flex-wrap",
    "justify-content", "align-items", "align-self", "gap", "row-gap", "column-gap", "order");

// #endregion

// #region Fixed rules

/**
 * The elements that are always removed with their content, under any
 * options, any removal mode and any namespace. Each entry says why.
 */
export const neverPassElements = createSet(
    // Runs JavaScript.
    "script",
    // A page-wide stylesheet: it can restyle Rock's own UI and, with
    // selectors plus url(), leak page data.
    "style",
    // Embeds another document or site, and srcdoc runs script.
    "iframe",
    // Loads a plugin or document (PDF, HTML, Flash) that can run code.
    "object",
    // Same as object.
    "embed",
    // Its action posts page data to another site, and a form inside Rock's
    // page-wide server form parses differently in the browser.
    "form",
    // Loads an external stylesheet, with the same risks as style.
    "link",
    // http-equiv can redirect or refresh the page.
    "meta",
    // Changes the base URL of every relative link and script on the page.
    "base",
    // Its content is text with scripting on and markup with scripting off,
    // a known mutation-XSS source.
    "noscript",
    // Its content sits in a separate fragment that script can clone into
    // the page later.
    "template",
    // Same as iframe.
    "frame",
    // Replaces the page body with frames.
    "frameset",
    // Runs a Java applet.
    "applet",
    // Raw-text element: markup inside it would be serialized unescaped.
    "xmp",
    // Raw-text element, parsed differently depending on plugin support.
    "noembed",
    // Raw-text element, parsed differently depending on frame support.
    "noframes",
    // Turns the rest of the page into text; it has no end tag.
    "plaintext",
    // Its content is text in HTML but markup in SVG, a namespace-confusion point.
    "title",
    // SVG animation that rewrites an attribute, such as href to
    // javascript:, after the sanitizer's checks.
    "animate",
    // Same as animate.
    "set",
    // Switches back to HTML parsing inside SVG, a namespace-confusion point.
    "foreignObject",
    // Switches back to HTML parsing inside MathML, a namespace-confusion point.
    "annotation-xml");

/**
 * The deepest an element may be nested below the body. A deeper element is
 * removed with its content, under any options and any removal mode. This
 * bounds the sanitizer's recursion so hostile input can not exhaust the
 * stack, and stays below the depth at which browsers flatten the tree, so the
 * sanitizer and the browser build the same tree. Real templates rarely pass
 * 24 levels.
 */
export const maxNestingDepth = 128;

/**
 * The nesting depth, estimated by a quick scan before parsing, past which the
 * input is not parsed at all and comes back encoded as text. Deeply nested
 * input is slow to parse. Set well above {@link maxNestingDepth} so real
 * content never reaches it.
 */
export const maxEstimatedNestingDepth = 1024;

/**
 * The void elements, which never have content or an end tag, so the
 * pre-parse scan does not count them as nesting.
 */
export const voidElements = createSet(
    "area", "base", "basefont", "bgsound", "br", "col", "embed", "frame", "hr", "img", "input", "keygen",
    "link", "meta", "param", "source", "track", "wbr");

/**
 * The elements whose end tag the parser implies when a sibling starts, so the
 * pre-parse scan does not count them as nesting.
 */
export const impliedEndElements = createSet(
    "p", "li", "dt", "dd", "td", "th", "tr", "thead", "tbody", "tfoot", "option", "optgroup",
    "rb", "rt", "rtc", "rp");

/**
 * The elements that start foreign content (SVG and MathML). Options can
 * allow them, but when they are not allowed they are always removed with
 * their content.
 */
export const dropOnlyElements = createSet(
    "svg", "math");

/** The SVG elements whose children are parsed as HTML (HTML Standard "HTML integration points"). */
export const svgHtmlIntegrationPoints = createSet(
    "desc");

/** The MathML elements whose children are parsed as HTML (HTML Standard "MathML text integration points"). */
export const mathMlTextIntegrationPoints = createSet(
    "mi", "mo", "mn", "ms", "mtext");

/**
 * The start tags that end SVG or MathML content in a browser, from the HTML
 * Standard "parsing tokens in foreign content".
 */
export const foreignContentBreakoutElements = createSet(
    "b", "big", "blockquote", "body", "br", "center", "code", "dd", "div", "dl", "dt", "em", "embed",
    "h1", "h2", "h3", "h4", "h5", "h6", "head", "hr", "i", "img", "li", "listing", "menu", "meta",
    "nobr", "ol", "p", "pre", "ruby", "s", "small", "span", "strong", "strike", "sub", "sup", "table",
    "tt", "u", "ul", "var", "font");

/**
 * The attributes that no options object can allow, beyond `on*` event
 * handlers. `formaction` would send the page's server form to another site;
 * `srcdoc` only works on `iframe`.
 */
export const neverAllowedAttributes = createSet(
    "formaction", "srcdoc");

/**
 * The method names of a `form` element, including inherited ones. A `name`
 * or `id` equal to one of them hides that method on the page-wide WebForms
 * form and breaks every postback. Compared case-sensitively, as the DOM does.
 */
export const formMethodNames: ReadonlySet<string> = new Set([
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
]);

/** The URL schemes that no options object can allow. */
export const blockedUrlSchemes = createSet(
    "javascript", "vbscript", "file", "data");

/** The CSS properties that no options object can allow. */
export const neverAllowedCssProperties = createSet(
    "behavior", "-moz-binding");

/** Text that removes a CSS declaration when its raw value contains it. */
export const rejectedCssValueText: readonly string[] = ["\\", "@", "<", ">", "/*", "*/"];

/** The attributes whose value is a URL and gets the URL rule (Ganss.Xss `UriAttributes`). */
export const urlAttributes = createSet(
    "action", "background", "cite", "codebase", "data", "dynsrc", "formaction", "href", "icon",
    "longdesc", "lowsrc", "manifest", "poster", "src", "xlink:href");

/**
 * The attributes whose value is a list of URLs (Ganss.Xss `UriListAttributes`).
 * List entries are not checked, so these can never be allowed.
 */
export const urlListAttributes = createSet(
    "archive", "ping", "srcset");

/** The `target` values that are kept, compared case-insensitively. */
export const allowedTargets = createSet(
    "_blank", "_self", "_parent", "_top");

/**
 * Matches the scheme of a URL: the text before the first colon (or its
 * numeric character reference) when no slash or hash comes before it.
 * Copied from Ganss.Xss v9.2.1039 `HtmlSanitizer.SchemeRegex`.
 */
// eslint-disable-next-line no-useless-escape -- Copied as is; the tests compare it with the C# pattern.
export const schemeRegex = /^([^\/#]*?)(?:\:|&#0*58|&#x0*3a)/i;

/**
 * Matches the CSS `expression` keyword, including its full-width Unicode
 * lookalikes. Copied from Ganss.Xss v9.2.1039 `HtmlSanitizer.CssExpression`.
 */
export const cssExpression = /[eEＥｅ][xXＸｘ][pPＰｐ][rRʀＲｒ][eEＥｅ][sSＳｓ]{2}[iIɪＩｉ][oOＯｏ][nNɴＮｎ]/;

/**
 * Matches a CSS `url(...)`; group 2 is the URL text. Copied from Ganss.Xss
 * v9.2.1039 `HtmlSanitizer.CssUrl`, with each atomic `(?>\s*)` written as a
 * plain repeat of the .NET whitespace class. Nothing after those groups
 * needs them to give back characters, so dropping atomicity changes no match.
 */
export const cssUrl = new RegExp(`[Uu][Rrʀ][Llʟ]${whiteSpaceClass}*\\(${whiteSpaceClass}*(['"]?)([^'")]*)(['"]?)${whiteSpaceClass}*\\)?`, "g");

// #endregion

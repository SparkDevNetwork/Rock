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

namespace Rock.Enums.Security
{
    /// <summary>
    /// Individual content-policy rules that can be evaluated against a string value
    /// at the persistence boundary. Each member is a single rule that fires when its
    /// pattern is detected, or for the one allowlist rule, when the value contains
    /// any character outside the allowed set. Profiles compose these flags into named
    /// bundles; see <see cref="StringValidationProfile"/>.
    /// </summary>
    /// <remarks>
    /// Plugins compiled against an older Rock version continue to reference these
    /// flags by their declared bit positions. Existing values MUST NOT be renumbered
    /// or removed once shipped; new rules are added at the next available bit position.
    /// </remarks>
    [Flags]
    public enum StringValidationRule
    {
        /// <summary>
        /// No rule selected. Used as the empty bitmask; an attribute whose effective
        /// rule set is <see cref="None"/> performs no validation against the value.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Detects Lava output expressions ( <c>{{ ... }}</c> ) that render a merge-field
        /// value. Lower risk than <see cref="LavaCommands"/> because output expressions
        /// cannot invoke side effects, but still flagged as injection when the property
        /// is not meant to be Lava-evaluated.
        /// </summary>
        LavaFormatting = 0x0001,

        /// <summary>
        /// Detects Lava tags ( <c>{% ... %}</c> ) and Lava shortcodes ( <c>{[ ... ]}</c> ).
        /// These delimiters can invoke entity commands, web requests, file operations,
        /// and other side effects, and are higher risk than <see cref="LavaFormatting"/>.
        /// </summary>
        LavaCommands = 0x0002,

        /// <summary>
        /// Detects the literal sequence <c>&lt;script</c> followed by a word boundary,
        /// case-insensitive. Matches a browser-parseable script tag opening.
        /// </summary>
        ScriptTags = 0x0004,

        /// <summary>
        /// Detects the <c>javascript:</c> URL scheme when preceded by an HTML-attribute
        /// or CSS-url context character ( <c>=</c>, <c>"</c>, <c>'</c>, or <c>(</c> ),
        /// so prose mentioning "javascript:" is not flagged. Does not match
        /// entity-encoded forms; URL-typed fields should layer a separate scheme-allowlist
        /// validation on top to catch bare values.
        /// </summary>
        JavascriptProtocol = 0x0008,

        /// <summary>
        /// Detects HTML event-handler attribute names (e.g. <c>onclick=</c>,
        /// <c>onerror=</c>) drawn from the HTML Living Standard's enumerated list,
        /// with the common <c>on</c> prefix factored out for performance. Uses an
        /// explicit name list rather than a generic <c>on[a-z]+</c> pattern to avoid
        /// false positives on prose like "online =".
        /// </summary>
        EventHandlerAttributes = 0x0010,

        /// <summary>
        /// Detects any HTML tag start: <c>&lt;</c> immediately followed by an ASCII
        /// letter, <c>!</c>, or <c>/</c>. Matches what HTML5 browsers tokenize as a
        /// tag, so math/inequality usage like <c>A &lt; B</c> (with whitespace) is
        /// intentionally allowed and not flagged.
        /// </summary>
        AnyHtmlTags = 0x0020,

        /// <summary>
        /// Detects ASCII control characters: <c>\x00</c>–<c>\x08</c>, <c>\x0B</c>,
        /// <c>\x0C</c>, <c>\x0E</c>–<c>\x1F</c>, and <c>\x7F</c> (DEL). Tab, line-feed,
        /// and carriage-return are explicitly allowed so multi-line text fields work.
        /// The null byte is included in this range; there is no separate null-byte rule.
        /// </summary>
        ControlCharacters = 0x0040,

        /// <summary>
        /// Detects Unicode bidirectional-override characters ( <c>U+202A</c>–<c>U+202E</c>,
        /// <c>U+2066</c>–<c>U+2069</c> ) used in bidi-spoofing attacks. Real
        /// right-to-left script characters (Arabic, Hebrew, etc.) are different code
        /// points and trigger directionality without these override characters, so this
        /// rule does not interfere with international text.
        /// </summary>
        BidiOverrides = 0x0080,
    }
}

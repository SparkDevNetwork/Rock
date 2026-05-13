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
namespace Rock.Enums.Security
{
    /// <summary>
    /// Named bundles of <see cref="StringValidationRule"/> values describing the
    /// kind of content a property is intended to hold. Properties opt into a profile
    /// via the <c>StringValidationAttribute</c>; properties with no attribute fall
    /// back to <see cref="PlainText"/>.
    /// </summary>
    /// <remarks>
    /// Profiles are exposed as a plain enum (not as <c>public const</c> bitmask
    /// values) so that plugins compiled against an older Rock version continue to
    /// reference the profile by name. The mapping from a profile to its rule
    /// bitmask is resolved at runtime against Rock's current definition, so a
    /// plugin's <c>[StringValidation(StringValidationProfile.BasicHtml)]</c> always
    /// picks up the current <see cref="BasicHtml"/> rule set even if that rule set
    /// has changed since the plugin was compiled. Once shipped, members of this
    /// enum MUST NOT be renumbered or removed; new profiles are added at the next
    /// available ordinal.
    /// </remarks>
    public enum StringValidationProfile
    {
        /// <summary>
        /// No rules. The property's value is accepted as-is. Intended for admin-only
        /// fields that intentionally allow arbitrary content (e.g. fields edited only
        /// through the admin UI by trusted users).
        /// </summary>
        Unrestricted = 0,

        /// <summary>
        /// Simple inline formatting only. Blocks <c>&lt;script&gt;</c> tags, the
        /// <c>javascript:</c> protocol, event-handler attributes, and Lava. Intended
        /// for short rich-text fields that should accept basic markup like
        /// <c>&lt;b&gt;</c> or <c>&lt;a&gt;</c> but not full HTML or templating.
        /// </summary>
        BasicHtml = 1,

        /// <summary>
        /// No markup at all. Blocks any HTML tag start, Lava delimiters, and control
        /// characters. The secure default for any property that has not been
        /// explicitly decorated with a different profile.
        /// </summary>
        PlainText = 2,

        /// <summary>
        /// Short human-readable labels: person names, group names, schedule names,
        /// campus names, business names, and similar. Equivalent to
        /// <see cref="PlainText"/> plus Unicode bidi-override detection. Permissive
        /// enough for international names and common business-name punctuation
        /// (e.g. "ABC Company, Ltd.", "Smith and Sons' Painting", "Smith &amp; Wesson").
        /// </summary>
        Name = 3,

        /// <summary>
        /// URL slugs, codes, and keys. The only profile that uses an allowlist:
        /// accepts only <c>A-Z</c>, <c>a-z</c>, <c>0-9</c>, <c>-</c>, and <c>_</c>.
        /// Strictly more restrictive than <see cref="Name"/>.
        /// </summary>
        Identifier = 4,

        // Ordinal 5 is reserved for a future Html profile; see the Server-Side
        // Field Validation spec, "Future Steps".
    }
}

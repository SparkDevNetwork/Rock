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

using Rock.Enums.Security;

namespace Rock.Security
{
    /// <summary>
    /// Declares the <see cref="StringValidationProfile"/> that should be enforced
    /// against a string property's value when the entity is saved. Properties with
    /// no <see cref="StringValidationAttribute"/> fall back to
    /// <see cref="StringValidationProfile.PlainText"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    public class StringValidationAttribute : Attribute
    {
        /// <summary>
        /// Gets the validation profile this property is opting into. The effective
        /// rule bitmask is resolved at runtime from Rock's current definition of the
        /// profile, then narrowed by <see cref="ExcludedRules"/> and widened by
        /// <see cref="AdditionalRules"/>.
        /// </summary>
        public StringValidationProfile Profile { get; }

        /// <summary>
        /// Gets or sets rules to exclude from the resolved profile bitmask. Each flag
        /// set here removes the corresponding rule from the effective rule set, so
        /// the rule does not run for this property at all. Use this when a specific
        /// property has a legitimate need to hold content the profile would otherwise
        /// reject (for example a <see cref="StringValidationProfile.Name"/>-profile
        /// field that may legitimately contain a bidi-override character).
        /// </summary>
        public StringValidationRule ExcludedRules { get; set; } = StringValidationRule.None;

        /// <summary>
        /// Gets or sets rules to add on top of the resolved profile bitmask. Each
        /// flag set here is appended to the effective rule set for this property.
        /// Use this when a specific property needs stricter validation than its
        /// profile provides (for example a header field on the
        /// <see cref="StringValidationProfile.Unrestricted"/> profile that should
        /// still reject Lava commands).
        /// </summary>
        public StringValidationRule AdditionalRules { get; set; } = StringValidationRule.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringValidationAttribute"/>
        /// class declaring the given <paramref name="profile"/>. Per-property
        /// adjustments may be applied via <see cref="ExcludedRules"/> and
        /// <see cref="AdditionalRules"/>.
        /// </summary>
        /// <param name="profile">The validation profile this property opts into.</param>
        public StringValidationAttribute( StringValidationProfile profile )
        {
            Profile = profile;
        }
    }
}

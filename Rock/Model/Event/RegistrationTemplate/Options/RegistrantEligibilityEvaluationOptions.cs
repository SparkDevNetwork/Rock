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

namespace Rock.Model.Event.RegistrationTemplate.Options
{
    /// <summary>
    /// Options that control how a <see cref="RegistrantEligibilityEvaluator"/> evaluates a person
    /// against the configured eligibility requirements of a registration template.
    /// </summary>
    public class RegistrantEligibilityEvaluationOptions
    {
        /// <summary>
        /// Gets or sets the mode that controls how the evaluator treats a person who is missing
        /// data required by a configured eligibility requirement.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="RegistrantEligibilityEvaluationMode.Strict"/> (the default) is used,
        /// a missing field that is needed by a configured requirement (for example, an unspecified
        /// age when a minimum age is configured) causes the person to be reported as ineligible.
        /// </para>
        /// <para>
        /// When <see cref="RegistrantEligibilityEvaluationMode.Lax"/> is used, the person is
        /// treated as eligible for that specific requirement; this is appropriate when surfacing
        /// "potentially eligible" individuals in a UI where the missing data can be supplied later.
        /// The eligibility Data View check, when configured, is always applied regardless of mode
        /// because it's not currently possible to check whether a person is missing data that might
        /// make them eligible against an arbitrary Data View.
        /// </para>
        /// </remarks>
        public RegistrantEligibilityEvaluationMode Mode { get; set; } = RegistrantEligibilityEvaluationMode.Strict;
    }
}

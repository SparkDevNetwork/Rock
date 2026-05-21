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
    /// Controls how a registrant eligibility evaluator treats a person who is missing data
    /// required to fully evaluate an eligibility requirement (e.g. age, grade, gender, age classification).
    /// </summary>
    public enum RegistrantEligibilityEvaluationMode
    {
        /// <summary>
        /// A person who is missing data required by a configured eligibility requirement is
        /// considered ineligible. This is the default and is appropriate for the final
        /// eligibility check at registration submission.
        /// </summary>
        Strict = 0,

        /// <summary>
        /// A person who is missing data required by a configured eligibility requirement is
        /// considered eligible for that requirement. This is appropriate for surfacing
        /// "potentially eligible" individuals in user interfaces (for example, the family
        /// members dropdown on a registration entry block) where the person can supply the
        /// missing data later.
        /// </summary>
        Lax = 1
    }
}

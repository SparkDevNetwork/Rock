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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Represents a set of eligibility criteria that determine whether a registrant meets specific requirements, such as age, grade, or gender.
    /// </summary>
    public class RegistrantEligibilityBag
    {
        /// <summary>
        /// Gets or sets the minimum age required for eligibility.
        /// </summary>
        /// <value>
        /// The minimum age in years. If <c>null</c>, no minimum age restriction is applied.
        /// </value>
        public decimal? MinimumAge { get; set; }

        /// <summary>
        /// Gets or sets the maximum age allowed for eligibility.
        /// </summary>
        /// <value>
        /// The maximum age in years. If <c>null</c>, no maximum age restriction is applied.
        /// </value>
        public decimal? MaximumAge { get; set; }

        /// <summary>
        /// Gets or sets the minimum age required for eligibility.
        /// </summary>
        /// <value>
        /// The minimum age birthdate. If <c>null</c>, no minimum age restriction is applied.
        /// </value>
        public DateTime? MinimumAgeBirthDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum age allowed for eligibility.
        /// </summary>
        /// <value>
        /// The maximum age birthdate. If <c>null</c>, no maximum age restriction is applied.
        /// </value>
        public DateTime? MaximumAgeBirthDate { get; set; }

        /// <summary>
        /// Gets or sets the grades required for eligibility.
        /// </summary>
        /// <remarks>If <c>null</c>, no minimum GradeOffset restriction is applied.</remarks>
        public List<Guid> Grades { get; set; }

        /// <summary>
        /// Gets or sets the required gender for eligibility.
        /// </summary>
        /// <value>
        /// The required <see cref="Gender"/> value. If <c>null</c>, gender is not evaluated.
        /// </value>
        public Gender? Gender { get; set; }
    }
}

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

namespace Rock.Model
{
    /// <summary>
    ///
    /// </summary>
    public class GroupRequirementStatus
    {
        /// <summary>
        /// Gets or sets the group requirement.
        /// </summary>
        /// <value>
        /// The group requirement.
        /// </value>
        public GroupRequirement GroupRequirement { get; set; }

        /// <summary>
        /// Gets or sets the meets group requirement.
        /// </summary>
        /// <value>
        /// The meets group requirement.
        /// </value>
        public MeetsGroupRequirement MeetsGroupRequirement { get; set; }

        /// <summary>
        /// Gets or sets the requirement warning date time.
        /// </summary>
        /// <value>
        /// The requirement warning date time.
        /// </value>
        public DateTime? RequirementWarningDateTime { get; set; }

        /// <summary>
        /// Gets or sets the requirement due date.
        /// </summary>
        /// <value>
        /// The requirement due date.
        /// </value>
        public DateTime? RequirementDueDate { get; set; }

        /// <summary>
        /// Gets or sets the last requirement check date time.
        /// </summary>
        /// <value>
        /// The last requirement check date time.
        /// </value>
        public DateTime? LastRequirementCheckDateTime { get; set; }

        /// <summary>
        /// Gets or sets the group member requirement id.
        /// </summary>
        /// <value>
        /// The group member requirement id.
        /// </value>
        public int? GroupMemberRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the calculation exception.
        /// </summary>
        /// <value>
        /// The calculation exception.
        /// </value>
        public Exception CalculationException { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}:{1}", this.GroupRequirement, this.MeetsGroupRequirement );
        }
    }
}
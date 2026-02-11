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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// Represents a group requirement configured at the group type level.
    /// </summary>
    public class GroupTypeGroupRequirementBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group requirement.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the group requirement type.
        /// </summary>
        public ListItemBag GroupRequirementType { get; set; }

        /// <summary>
        /// Gets or sets the role that this requirement applies to.
        /// </summary>
        public ListItemBag Role { get; set; }

        /// <summary>
        /// Gets or sets the age classification to which this requirement applies.
        /// </summary>
        public AppliesToAgeClassification AppliesToAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the dataview used to determine applicability, if configured.
        /// </summary>
        public ListItemBag AppliesToDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether leaders are allowed to override the requirement.
        /// </summary>
        public bool AllowLeadersToOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requirement must be met to add a member.
        /// </summary>
        public bool MustMeetRequirementToAddMember { get; set; }

        /// <summary>
        /// Gets or sets the due date type of the associated <see cref="GroupRequirementType"/>, which controls how the due date is calculated or selected.
        /// </summary>
        public DueDateType DueDateType { get; set; }
        
        /// <summary>
        /// Gets or sets the static due date, if <see cref="DueDateType"/> is configured for a specific date.
        /// </summary>
        public DateTimeOffset? DueDateStaticDate { get; set; }

        /// <summary>
        /// Gets or sets the due date attribute, if <see cref="DueDateType"/> is configured to use a group attribute.
        /// </summary>
        public ListItemBag DueDateAttribute { get; set; }
    }
}



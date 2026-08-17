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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// A single Group Requirement row. Used both by the read-only "From Group Type" grid and
    /// the editable "Specific Group Requirements" grid on the edit panel.
    /// </summary>
    public class GroupRequirementBag
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
        /// Gets or sets the group role this requirement applies to. Null applies the
        /// requirement to all roles.
        /// </summary>
        public ListItemBag Role { get; set; }

        /// <summary>
        /// Gets or sets the age classification to which this requirement applies.
        /// </summary>
        public AppliesToAgeClassification AppliesToAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the data view that determines who the requirement applies to.
        /// Null = applies to all members.
        /// </summary>
        public ListItemBag AppliesToDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether leaders are allowed to override the
        /// requirement.
        /// </summary>
        public bool AllowLeadersToOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members must meet this requirement before
        /// being added.
        /// </summary>
        public bool MustMeetRequirementToAddMember { get; set; }

        /// <summary>
        /// Gets or sets the due date type sourced from the selected
        /// <see cref="GroupRequirementType"/>. Drives which of <see cref="DueDateStaticDate"/>
        /// or <see cref="DueDateAttribute"/> the modal renders.
        /// </summary>
        public DueDateType DueDateType { get; set; }

        /// <summary>
        /// Gets or sets the static due date when <see cref="DueDateType"/> is
        /// <c>ConfiguredDate</c>.
        /// </summary>
        public DateTimeOffset? DueDateStaticDate { get; set; }

        /// <summary>
        /// Gets or sets the group attribute holding the due date when
        /// <see cref="DueDateType"/> is <c>GroupAttribute</c>.
        /// </summary>
        public ListItemBag DueDateAttribute { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// A group requirement defined specifically for a sign-up project, as edited in the group
    /// requirements section.
    /// </summary>
    public class SignUpProjectRequirementBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group requirement. New requirements are
        /// assigned a unique identifier by the client.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the group requirement type. The value is the requirement type unique
        /// identifier.
        /// </summary>
        public ListItemBag GroupRequirementType { get; set; }

        /// <summary>
        /// Gets or sets the group role this requirement applies to. The value is the role unique
        /// identifier. Null applies the requirement to all roles.
        /// </summary>
        public ListItemBag Role { get; set; }

        /// <summary>
        /// Gets or sets the age classification this requirement applies to.
        /// </summary>
        public AppliesToAgeClassification AppliesToAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the data view that determines who the requirement applies to. Null
        /// applies the requirement to all members.
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
        /// Gets or sets the due date type of the selected requirement type. Determines whether
        /// <see cref="DueDateStaticDate"/> or <see cref="DueDateAttribute"/> is used.
        /// </summary>
        public DueDateType DueDateType { get; set; }

        /// <summary>
        /// Gets or sets the due date when <see cref="DueDateType"/> is <c>ConfiguredDate</c>.
        /// </summary>
        public DateTimeOffset? DueDateStaticDate { get; set; }

        /// <summary>
        /// Gets or sets the group attribute holding the due date when <see cref="DueDateType"/>
        /// is <c>GroupAttribute</c>. The value is the attribute unique identifier.
        /// </summary>
        public ListItemBag DueDateAttribute { get; set; }
    }
}

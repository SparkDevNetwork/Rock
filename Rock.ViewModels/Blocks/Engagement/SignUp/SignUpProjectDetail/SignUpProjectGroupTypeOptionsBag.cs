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

using System.Collections.Generic;

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// The options of the Sign-Up Project Detail block that depend on the project's group type.
    /// Returned with the initial options and from the <c>GetGroupTypeOptions</c> block action when
    /// the group type is changed while adding a project.
    /// </summary>
    public class SignUpProjectGroupTypeOptionsBag
    {
        /// <summary>
        /// Gets or sets the name of the group type.
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the group type's detail page.
        /// </summary>
        public string GroupTypeUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether groups of this type require a campus.
        /// </summary>
        public bool RequiresCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the record source override picker is displayed.
        /// </summary>
        public bool IsRecordSourceVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type allows member attributes to be
        /// defined for a specific group.
        /// </summary>
        public bool AllowSpecificGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type allows group requirements to be
        /// defined for a specific group.
        /// </summary>
        public bool EnableSpecificGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the schedule types the group type allows for opportunities.
        /// </summary>
        public ScheduleType AllowedScheduleTypes { get; set; }

        /// <summary>
        /// Gets or sets the location selection modes the group type allows for opportunities.
        /// </summary>
        public GroupLocationPickerMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the member attributes inherited from the group type and the group types it
        /// inherits from.
        /// </summary>
        public List<SignUpProjectInheritedMemberAttributeBag> InheritedMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group requirements defined on the group type.
        /// </summary>
        public List<SignUpProjectInheritedRequirementBag> InheritedGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the group type's roles, used when a group requirement applies to a single
        /// role. The value of each item is the role unique identifier.
        /// </summary>
        public List<ListItemBag> GroupRoleOptions { get; set; }
    }
}

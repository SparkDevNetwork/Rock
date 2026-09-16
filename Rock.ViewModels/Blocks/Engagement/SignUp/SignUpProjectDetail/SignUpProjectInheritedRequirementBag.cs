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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// A group requirement defined on the project's group type, displayed read-only in the group
    /// requirements section.
    /// </summary>
    public class SignUpProjectInheritedRequirementBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group requirement.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the requirement type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the group role the requirement applies to. Empty when the
        /// requirement applies to all roles.
        /// </summary>
        public string GroupRoleName { get; set; }

        /// <summary>
        /// Gets or sets the age classification the requirement applies to.
        /// </summary>
        public AppliesToAgeClassification AppliesToAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members must meet the requirement before
        /// being added.
        /// </summary>
        public bool MustMeetRequirementToAddMember { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type the requirement is defined on.
        /// </summary>
        public string InheritedFromGroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the detail page of the group type the requirement is defined
        /// on.
        /// </summary>
        public string InheritedFromGroupTypeUrl { get; set; }
    }
}

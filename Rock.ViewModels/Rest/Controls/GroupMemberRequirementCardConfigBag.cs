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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The GroupMemberRequirement information used by the GetData API action of
    /// the GroupMemberRequirementContainer control to pass on to the cards on the front end.
    /// </summary>
    public class GroupMemberRequirementCardConfigBag
    {
        /// <summary>
        /// The name of the GroupRequirementType
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// CSS Class specifying which icon should be displayed for this card
        /// </summary>
        public string TypeIconCssClass { get; set; }

        /// <summary>
        /// Whether or not this requirement has been met
        /// </summary>
        public MeetsGroupRequirement MeetsGroupRequirement { get; set; }

        /// <summary>
        /// Unique identifier for the GroupRequirment
        /// </summary>
        public Guid GroupRequirementGuid { get; set; }

        /// <summary>
        /// Unique identifier for the GroupRequirmentType
        /// </summary>
        public Guid GroupRequirementTypeGuid { get; set; }

        /// <summary>
        /// Unique identifier for the GroupMemberRequirment
        /// </summary>
        public Guid GroupMemberRequirementGuid { get; set; }

        /// <summary>
        /// The date in which this requirement myst be met by
        /// </summary>
        public string GroupMemberRequirementDueDate { get; set; }

        /// <summary>
        /// Whether or not the current user is allowed to override the "met" status for this requirement
        /// </summary>
        public bool CanOverride { get; set; }
    }
}

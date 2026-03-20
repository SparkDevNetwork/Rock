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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the details of a placement group available for assignment to a connection request.
    /// </summary>
    public class PlacementGroupDetailsBag
    {
        /// <summary>
        /// Gets or sets the group as a list item containing its identifier value and display name.
        /// </summary>
        public ListItemBag ListItemBag { get; set; }

        /// <summary>
        /// Gets or sets the Campus Guid of the Placement Group.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the list of available group member roles in this placement group.
        /// </summary>
        public List<ListItemBag> GroupMemberRoles { get; set; }

        /// <summary>
        /// Gets or sets the available group member statuses keyed by group member role identifier.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> GroupMemberStatuses { get; set; }

        /// <summary>
        /// Gets or sets the CSS icon class associated with this placement group.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requester already exists as a pending group member in this placement group.
        /// </summary>
        public bool? IsPendingGroupMember { get; set; }

        /// <summary>
        /// The Group Member IdKey of the placed group member.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets the list of group member requirements and their current fulfillment state for this placement group.
        /// </summary>
        public List<GroupMemberRequirementBag> GroupMemberRequirements { get; set; }

        /// <summary>
        /// Gets or sets the group member attributes for the selected placement group.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, PublicAttributeBag> GroupMemberAttributes { get; set; }

        /// <summary>
        /// The attribute values for the placement group.
        /// </summary>
        public Dictionary<string, string> GroupMemberAttributeValues { get; set; }
    }

    /// <summary>
    /// Represents a single group member requirement and its current fulfillment state for a placement group member.
    /// </summary>
    public class GroupMemberRequirementBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the group requirement definition.
        /// </summary>
        public string GroupRequirementIdKey { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the group member requirement record tracking fulfillment for this member.
        /// </summary>
        public string GroupMemberRequirementIdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of this group requirement.
        /// </summary>
        public string RequirementName { get; set; }

        /// <summary>
        /// Gets or sets the current state indicating whether the group member meets this requirement.
        /// </summary>
        public MeetsGroupRequirement GroupMemberRequirementState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement must be met before the member can be added to the group.
        /// </summary>
        public bool MustMeetRequirementToAddMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a manually verified requirement (as opposed to one checked automatically).
        /// </summary>
        public bool IsManualRequirement { get; set; }
    }
}

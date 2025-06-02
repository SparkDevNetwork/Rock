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

using Rock.Enums.Group;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents the data required to add one or more people to a group during the placement process.
    /// </summary>
    public class AddGroupMembersBag
    {
        /// <summary>
        /// A list of pending group members to be added, including person details, role, and attribute data.
        /// </summary>
        public List<GroupMemberBag> PendingGroupMembers { get; set; }

        /// <summary>
        /// The group to which the members should be added, including metadata such as name, ID, and capacity.
        /// </summary>
        public PlacementGroupBag TargetGroup { get; set; }

        /// <summary>
        /// Contextual keys that define the placement scope, such as registration template or instance information.
        /// </summary>
        public GroupPlacementKeysBag GroupPlacementKeys { get; set; }

        /// <summary>
        /// Indicates the mode of placement being performed (e.g., based on registration template, instance, or group).
        /// </summary>
        public PlacementMode PlacementMode { get; set; }
    }

}

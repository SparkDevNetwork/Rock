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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents the data used when adding a new group or selecting existing groups for placement.
    /// </summary>
    public class AddGroupBag
    {
        /// <summary>
        /// Indicates the selected option for group creation or addition.
        /// </summary>
        public string SelectedGroupOption { get; set; }

        /// <summary>
        /// The parent group under which the new group should be created.
        /// </summary>
        public ListItemBag ParentGroupForNewGroup { get; set; }

        /// <summary>
        /// The name of the new group to be created.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The campus associated with the new group.
        /// </summary>
        public ListItemBag GroupCampus { get; set; }

        /// <summary>
        /// The optional capacity limit for the group (maximum number of members).
        /// </summary>
        public int? GroupCapacity { get; set; }

        /// <summary>
        /// A brief description of the group's purpose or context.
        /// </summary>
        public string GroupDescription { get; set; }

        /// <summary>
        /// The encrypted identifier key for the group type
        /// </summary>
        public string GroupTypeIdKey { get; set; }

        /// <summary>
        /// Contextual placement keys used to scope how and where the group fits into the placement process.
        /// </summary>
        public GroupPlacementKeysBag GroupPlacementKeys { get; set; }

        /// <summary>
        /// Attribute values to set for the new group, keyed by attribute GUID or name.
        /// </summary>
        public Dictionary<string, string> NewGroupAttributeValues { get; set; }

        /// <summary>
        /// A list of existing groups selected for adding members or associating with placements.
        /// </summary>
        public List<ListItemBag> ExistingGroupsToAdd { get; set; }

        /// <summary>
        /// The selected parent group of the placement groups we are adding.
        /// </summary>
        public ListItemBag ParentGroupForChildren { get; set; }

        /// <summary>
        /// The realtime connection ID.
        /// </summary>
        public string ConnectionId { get; set; }
    }
}

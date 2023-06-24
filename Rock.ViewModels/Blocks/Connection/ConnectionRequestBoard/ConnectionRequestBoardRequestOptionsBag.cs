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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains connection request options information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardRequestOptionsBag
    {
        /// <summary>
        /// Gets or sets the "connector" people that can be assigned to a connection request.
        /// </summary>
        public List<ListItemBag> Connectors { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses that can be assigned to a connection request.
        /// </summary>
        public List<ListItemBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection states that can be assigned to a connection request.
        /// </summary>
        public List<ListItemBag> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the placement groups that can be assigned to a connection request.
        /// </summary>
        public List<ListItemBag> PlacementGroups { get; set; }

        /// <summary>
        /// Gets or sets the placement group member roles (by group ID) that can be assigned to a connection request.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> PlacementGroupMemberRoles { get; set; }

        /// <summary>
        /// Gets or sets the placement group member statuses (by group ID and group member role ID) that can be
        /// assigned to a connection request.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> PlacementGroupMemberStatuses { get; set; }

        /// <summary>
        /// Gets or sets the manual workflows that can be launched against a connection request.
        /// </summary>
        public List<ListItemBag> ManualWorkflows { get; set; }
    }
}

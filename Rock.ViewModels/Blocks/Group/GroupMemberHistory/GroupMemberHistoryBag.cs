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

using Rock.ViewModels.Blocks.Group.GroupHistory;
using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Group.GroupMemberHistory
{
    /// <summary>
    /// Holds initialization data for the Group Member History block.
    /// </summary>
    public class GroupMemberHistoryBag
    {
        /// <summary>
        /// Gets or sets the name of the group whose member history is
        /// displayed.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the timeline for a single
        /// group member is displayed.
        /// </summary>
        public bool IsMemberTimelineShown { get; set; }

        /// <summary>
        /// Gets or sets the full name of the group member whose timeline is
        /// displayed.
        /// </summary>
        public string GroupMemberName { get; set; }

        /// <summary>
        /// Gets or sets the timeline of history for the group member, one item
        /// per day that has activity, ordered newest day first.
        /// </summary>
        public List<GroupHistoryDayBag> Timeline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid of historical
        /// group members is available. When the timeline is displayed this
        /// indicates the grid can be navigated back to.
        /// </summary>
        public bool IsMembersGridShown { get; set; }

        /// <summary>
        /// Gets or sets the definition of the historical group members grid.
        /// Only populated when the grid is displayed.
        /// </summary>
        public GridDefinitionBag MembersGridDefinition { get; set; }
    }
}

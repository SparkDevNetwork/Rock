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

namespace Rock.ViewModels.Blocks.Group.GroupHistory
{
    /// <summary>
    /// Holds initialization data for the Group History block.
    /// </summary>
    public class GroupHistoryBag
    {
        /// <summary>
        /// Gets or sets the name of the group whose history is displayed.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page that displays individual member
        /// history for this group.
        /// </summary>
        public string GroupMemberHistoryPageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group member changes are
        /// included in the timeline alongside group changes.
        /// </summary>
        public bool IsGroupMemberHistoryIncluded { get; set; }

        /// <summary>
        /// Gets or sets the timeline of history, one item per day that has
        /// activity, ordered newest day first.
        /// </summary>
        public List<GroupHistoryDayBag> Timeline { get; set; }
    }
}

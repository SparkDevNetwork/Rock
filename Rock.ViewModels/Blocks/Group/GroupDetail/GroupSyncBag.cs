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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// A single Group Sync row on the edit panel.
    /// </summary>
    public class GroupSyncBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group sync.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the role synced members are assigned.
        /// </summary>
        public ListItemBag GroupTypeRole { get; set; }

        /// <summary>
        /// Gets or sets the data view that identifies group members.
        /// </summary>
        public ListItemBag SyncDataView { get; set; }

        /// <summary>
        /// Gets or sets the welcome system communication sent on add. Null to skip sending.
        /// </summary>
        public ListItemBag WelcomeSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the exit system communication sent on removal. Null to skip sending.
        /// </summary>
        public ListItemBag ExitSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a Rock login account should be created for
        /// synced members that do not yet have one.
        /// </summary>
        public bool AddUserAccountsDuringSync { get; set; }

        /// <summary>
        /// Gets or sets the interval (in minutes) between sync runs.
        /// </summary>
        public int? ScheduleIntervalMinutes { get; set; }

        /// <summary>
        /// Gets or sets the date/time of the last successful sync run. Populated by the
        /// Group Sync job and not written by this block.
        /// </summary>
        public DateTimeOffset? LastRefreshDateTime { get; set; }
    }
}

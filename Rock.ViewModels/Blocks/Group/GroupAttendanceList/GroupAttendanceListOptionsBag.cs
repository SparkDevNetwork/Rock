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

namespace Rock.ViewModels.Blocks.Group.GroupAttendanceList
{
    /// <summary>
    /// The additional configuration options for the Group Attendance List block.
    /// </summary>
    public class GroupAttendanceListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current person is authorized to
        /// view the target group. When <c>false</c>, the block renders a notification
        /// instead of the grid.
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the name of the group displayed in the panel heading.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the group identifier key used for navigation and preference scoping.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// Gets or sets the location items available for the location filter.
        /// </summary>
        public List<ListItemBag> LocationItems { get; set; }

        /// <summary>
        /// Gets or sets the schedule items available for the schedule filter.
        /// </summary>
        public List<ListItemBag> ScheduleItems { get; set; }

        /// <summary>
        /// Gets or sets the campus items available for the campus filter.
        /// </summary>
        public List<ListItemBag> CampusItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Notes column is visible.
        /// </summary>
        public bool IsNotesColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Attendance Type column is visible.
        /// </summary>
        public bool IsAttendanceTypeColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus filter is enabled.
        /// </summary>
        public bool IsCampusFilterEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Location column is visible.
        /// Hidden when the group has no named locations.
        /// </summary>
        public bool IsLocationColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Schedule column is visible.
        /// Hidden when the group has no schedules associated with its locations.
        /// </summary>
        public bool IsScheduleColumnVisible { get; set; }
    }
}

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
using Rock.Enums.Blocks.Group.Scheduling;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The information needed to update a group member's scheduling preference for the group scheduler.
    /// </summary>
    public class GroupSchedulerUpdatePreferenceBag
    {
        /// <summary>
        /// Gets or sets the attendance identifier.
        /// </summary>
        /// <value>
        /// The attendance identifier.
        /// </value>
        public int AttendanceId { get; set; }

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the preference that should be applied.
        /// </summary>
        /// <value>
        /// The preference that should be applied.
        /// </value>
        public GroupSchedulerPreferenceBag SchedulePreference { get; set; }

        /// <summary>
        /// Gets or sets the mode to be used when applying this update.
        /// </summary>
        /// <value>
        /// The mode to be used when applying this update.
        /// </value>
        public UpdateSchedulePreferenceMode UpdateMode { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The information describing a group member's scheduling preference for the group scheduler.
    /// </summary>
    public class GroupSchedulerPreferenceBag
    {
        /// <summary>
        /// Gets or sets the schedule template identifier.
        /// </summary>
        /// <value>
        /// The schedule template identifier.
        /// </value>
        public string ScheduleTemplate { get; set; }

        /// <summary>
        /// Gets or sets the date this schedule preference should take effect.
        /// </summary>
        /// <value>
        /// The date this schedule preference should take effect.
        /// </value>
        public DateTimeOffset? ScheduleStartDate { get; set; }
    }
}

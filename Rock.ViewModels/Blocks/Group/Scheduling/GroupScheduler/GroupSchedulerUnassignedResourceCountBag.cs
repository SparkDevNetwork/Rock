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
    /// The information about unassigned resource counts (attendance records without a location)
    /// for an occurrence date, schedule and group combo for the group scheduler.
    /// </summary>
    public class GroupSchedulerUnassignedResourceCountBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the occurrence date.
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTimeOffset OccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence identifier.
        /// </summary>
        /// <value>
        /// The attendance occurrence identifier.
        /// </value>
        public int AttendanceOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the count of unassigned resources.
        /// </summary>
        /// <value>
        /// The count of unassigned resources.
        /// </value>
        public int ResourceCount { get; set; }
    }
}

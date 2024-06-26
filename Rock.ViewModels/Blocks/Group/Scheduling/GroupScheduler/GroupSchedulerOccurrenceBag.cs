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
    /// The information needed to identify a specific occurrence to be scheduled within the Group Scheduler.
    /// </summary>
    public class GroupSchedulerOccurrenceBag
    {
        /// <summary>
        /// Gets or sets the attendance occurrence ID for this occurrence.
        /// </summary>
        /// <value>
        /// The attendance occurrence ID for this occurrence.
        /// </value>
        public int AttendanceOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the group order for this occurrence.
        /// </summary>
        /// <value>
        /// The group order for this occurrence.
        /// </value>
        public int GroupOrder { get; set; }

        /// <summary>
        /// Gets or sets the group ID for this occurrence.
        /// </summary>
        /// <value>
        /// The group ID for this occurrence.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group name for this occurrence.
        /// </summary>
        /// <value>
        /// The group name for this occurrence.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the parent group ID (if any) for this occurrence.
        /// </summary>
        /// <value>
        /// The parent group ID (if any) for this occurrence.
        /// </value>
        public int? ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the parent group name (if any) for this occurrence.
        /// </summary>
        /// <value>
        /// The parent group name (if any) for this occurrence.
        /// </value>
        public string ParentGroupName { get; set; }

        /// <summary>
        /// Gets or sets the group location order for this occurrence.
        /// </summary>
        /// <value>
        /// The group location order for this occurrence.
        /// </value>
        public int GroupLocationOrder { get; set; }

        /// <summary>
        /// Gets or sets the location ID for this occurrence.
        /// </summary>
        /// <value>
        /// The location ID for this occurrence.
        /// </value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the location name for this occurrence.
        /// </summary>
        /// <value>
        /// The location name for this occurrence.
        /// </value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the schedule ID for this occurrence.
        /// </summary>
        /// <value>
        /// The schedule ID for this occurrence.
        /// </value>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the schedule name for this occurrence.
        /// </summary>
        /// <value>
        /// The schedule name for this occurrence.
        /// </value>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the schedule order for this occurrence.
        /// </summary>
        /// <value>
        /// The schedule order for this occurrence.
        /// </value>
        public int ScheduleOrder { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date and time.
        /// </summary>
        /// <value>
        /// The occurrence date and time.
        /// </value>
        public DateTimeOffset OccurrenceDateTime { get; set; }

        /// <summary>
        /// Gets the occurrence date.
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTimeOffset OccurrenceDate => OccurrenceDateTime.Date;

        /// <summary>
        /// Gets or sets the ISO 8601 Sunday date for this occurrence.
        /// </summary>
        /// <value>
        /// The ISO 8601 Sunday date for this occurrence.
        /// </value>
        public string SundayDate { get; set; }

        /// <summary>
        /// Gets or sets the minimum capacity for this occurrence.
        /// </summary>
        /// <value>
        /// The minimum capacity for this occurrence.
        /// </value>
        public int? MinimumCapacity { get; set; }

        /// <summary>
        /// Gets or sets the desired capacity for this occurrence.
        /// </summary>
        /// <value>
        /// The desired capacity for this occurrence.
        /// </value>
        public int? DesiredCapacity { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity for this occurrence.
        /// </summary>
        /// <value>
        /// The maximum capacity for this occurrence.
        /// </value>
        public int? MaximumCapacity { get; set; }

        /// <summary>
        /// Gets or sets whether scheduling is enabled for this occurrence.
        /// </summary>
        /// <value>
        /// Whether scheduling is enabled for this occurrence.
        /// </value>
        public bool IsSchedulingEnabled { get; set; }
    }
}

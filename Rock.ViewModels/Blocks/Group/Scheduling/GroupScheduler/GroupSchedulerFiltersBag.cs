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
using System.Collections.Generic;

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The filters to limit what is shown on the Group scheduler.
    /// </summary>
    public class GroupSchedulerFiltersBag
    {
        /// <summary>
        /// Gets or sets the selected groups.
        /// </summary>
        /// <value>
        /// The selected groups.
        /// </value>
        public List<ListItemBag> Groups { get; set; }

        /// <summary>
        /// Gets or sets the available and selected locations.
        /// </summary>
        /// <value>
        /// The available and selected locations.
        /// </value>
        public GroupSchedulerLocationsBag Locations { get; set; }

        /// <summary>
        /// Gets or sets the available and selected schedules.
        /// </summary>
        /// <values>
        /// The available and selected schedules.
        /// </values>
        public GroupSchedulerSchedulesBag Schedules { get; set; }

        /// <summary>
        /// Gets or sets the selected date range.
        /// </summary>
        /// <value>
        /// The selected date range.
        /// </value>
        public SlidingDateRangeBag DateRange { get; set; }

        /// <summary>
        /// Gets or set sets the start date, based on the selected date range.
        /// </summary>
        /// <value>
        /// The start date, based on the selected date range.
        /// </value>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date, based on the selected date range.
        /// </summary>
        /// <value>
        /// The end date, based on the selected date range.
        /// </value>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days included in the date range.
        /// </summary>
        /// <value>
        /// The number of days included in the date range.
        /// </value>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Gets or sets the friendly date range, based on the selected date range.
        /// </summary>
        /// <value>
        /// The friendly date range, based on the selected date range.
        /// </value>
        public string FriendlyDateRange { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Reporting.ServiceMetricsEntry
{
    /// <summary>
    /// A bag that contains the information required to render the Obsidian Service Metrics Entry block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class ServiceMetricsEntryInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether duplicate metrics are included in category subtotals.
        /// </summary>
        public bool AreDuplicateMetricsIncludedInCategorySubtotals { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the schedule unique identifier.
        /// </summary>
        public Guid? ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show metric category subtotals.
        /// </summary>
        public bool ShowMetricCategorySubtotals { get; set; }

        /// <summary>
        /// Gets or sets the weekend date.
        /// </summary>
        public DateTimeOffset? WeekendDate { get; set; }

        /// <summary>
        /// Gets or sets the weeks ahead.
        /// </summary>
        /// <value>
        /// The weeks ahead.
        /// </value>
        public int WeeksAhead { get; set; }

        /// <summary>
        /// Gets or sets the weeks back.
        /// </summary>
        /// <value>
        /// The weeks back.
        /// </value>
        public int WeeksBack { get; set; }

        /// <summary>
        /// Gets or sets the number of active campuses.
        /// </summary>
        /// <value>
        /// The active campuses count.
        /// </value>
        public int ActiveCampusesCount { get; set; }
    }
}

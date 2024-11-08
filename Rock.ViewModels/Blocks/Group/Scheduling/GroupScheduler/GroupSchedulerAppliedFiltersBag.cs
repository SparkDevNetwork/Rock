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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The filters that were applied in response to an "ApplyFilters" request, and the resulting occurrences to be scheduled.
    /// </summary>
    public class GroupSchedulerAppliedFiltersBag
    {
        /// <summary>
        /// Gets or sets the filters that were applied.
        /// </summary>
        /// <value>
        /// The filters that were applied.
        /// </value>
        public GroupSchedulerFiltersBag Filters { get; set; }

        /// <summary>
        /// Gets or sets the occurrences to be scheduled, limited by the applied filters.
        /// </summary>
        /// <value>
        /// The occurrences to be scheduled, limited by the applied filters.
        ///</value>
        public List<GroupSchedulerOccurrenceBag> ScheduleOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the unique, ordered group, location and schedule name combinations.
        /// </summary>
        /// <value>
        /// The unique, ordered group, location and schedule name combinations.
        /// </value>
        public List<GroupSchedulerGroupLocationScheduleNamesBag> GroupLocationScheduleNames { get; set; }

        /// <summary>
        /// Gets or sets the unassigned resource counts, limited by the applied filters.
        /// </summary>
        /// <value>
        /// The unassigned resource counts, limited by the applied filters.
        /// </value>
        public List<GroupSchedulerUnassignedResourceCountBag> UnassignedResourceCounts { get; set; }

        /// <summary>
        /// Gets or sets the navigation urls.
        /// </summary>
        /// <value>The navigation urls.</value>
        public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();
    }
}

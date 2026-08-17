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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemMetrics
{
    /// <summary>
    /// The Overview KPI metrics for a content channel item over the selected date range.
    /// </summary>
    public class OverviewMetricsBag
    {
        /// <summary>
        /// Gets or sets the total number of views (interactions) in the selected period.
        /// </summary>
        public int TotalViews { get; set; }

        /// <summary>
        /// Gets or sets the percent change in total views versus the previous period.
        /// Null when there is no previous-period baseline to compare against.
        /// </summary>
        public double? TotalViewsDeltaPercent { get; set; }

        /// <summary>
        /// Gets or sets the number of distinct browsing sessions in the selected period.
        /// </summary>
        public int UniqueViews { get; set; }

        /// <summary>
        /// Gets or sets the percent change in unique views versus the previous period.
        /// Null when there is no previous-period baseline to compare against.
        /// </summary>
        public double? UniqueViewsDeltaPercent { get; set; }

        /// <summary>
        /// Gets or sets the number of distinct identified people in the selected period.
        /// </summary>
        public int KnownPeople { get; set; }

        /// <summary>
        /// Gets or sets the percent change in known people versus the previous period.
        /// Null when there is no previous-period baseline to compare against.
        /// </summary>
        public double? KnownPeopleDeltaPercent { get; set; }

        /// <summary>
        /// Gets or sets the per-day view counts for the selected period, ordered by date.
        /// </summary>
        public List<ViewsOverTimePointBag> ViewsOverTime { get; set; }

        /// <summary>
        /// Gets or sets the view counts broken down by device type for the selected period.
        /// </summary>
        public List<MetricSliceBag> DeviceBreakdown { get; set; }

        /// <summary>
        /// Gets or sets the distinct known-people counts broken down by connection status for the selected period.
        /// </summary>
        public List<MetricSliceBag> ConnectionStatusBreakdown { get; set; }

        /// <summary>
        /// Gets or sets the top referrer hosts by view count for the selected period, ordered highest to lowest.
        /// </summary>
        public List<MetricSliceBag> TopReferrers { get; set; }

        /// <summary>
        /// Gets or sets the per-dimension UTM breakdowns for the selected period. Only dimensions that
        /// are enabled by the block setting and have captured data are included.
        /// </summary>
        public List<UtmDimensionMetricsBag> UtmBreakdowns { get; set; }
    }
}

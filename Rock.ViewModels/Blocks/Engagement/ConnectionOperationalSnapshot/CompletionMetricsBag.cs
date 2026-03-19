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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOperationalSnapshot
{
    /// <summary>
    /// Metrics related to request completion performance.
    /// </summary>
    public class CompletionMetricsBag
    {
        /// <summary>
        /// Gets or sets the percentage of requests completed on time.
        /// </summary>
        public decimal TimelinessPercent { get; set; }

        /// <summary>
        /// Gets or sets the percentage change in timeliness compared to the previous period.
        /// </summary>
        public decimal TimelinessPercentDelta { get; set; }

        /// <summary>
        /// Gets or sets the average number of days to first response.
        /// </summary>
        public decimal AverageResponsivenessDays { get; set; }

        /// <summary>
        /// Gets or sets the percentage change in responsiveness compared to the previous period.
        /// </summary>
        public decimal AverageResponsivenessDaysDelta { get; set; }

        /// <summary>
        /// Gets or sets the total number of completed requests.
        /// </summary>
        public int RequestsCompletedCount { get; set; }

        /// <summary>
        /// Gets or sets the percentage change in completed requests compared to the previous period.
        /// </summary>
        public decimal RequestsCompletedCountDelta { get; set; }

        /// <summary>
        /// Gets or sets the average number of days required to complete a request.
        /// </summary>
        public decimal AverageCompletionDays { get; set; }

        /// <summary>
        /// Gets or sets the percentage change in average completion time compared to the previous period.
        /// </summary>
        public decimal AverageCompletionDaysDelta { get; set; }
    }
}

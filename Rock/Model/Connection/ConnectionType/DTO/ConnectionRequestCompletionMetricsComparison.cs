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

namespace Rock.Model.Connection.ConnectionType.DTO
{
    /// <summary>
    /// Represents completion metrics along with deltas compared
    /// to a previous equivalent time period.
    /// </summary>
    internal class ConnectionRequestCompletionMetricsComparison
    {
        /// <summary>
        /// The identifier of the connection type this comparison applies to.
        /// </summary>
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// Metrics for the current period.
        /// </summary>
        public ConnectionRequestCompletionMetricsSummary Current { get; set; }

        /// <summary>
        /// Metrics for the previous equivalent period.
        /// </summary>
        public ConnectionRequestCompletionMetricsSummary Previous { get; set; }

        /// <summary>
        /// Difference in timeliness percentage between periods.
        /// </summary>
        public decimal TimelinessPercentDelta { get; set; }

        /// <summary>
        /// Difference in average responsiveness days between periods.
        /// </summary>
        public decimal AverageResponsivenessDaysDelta { get; set; }

        /// <summary>
        /// Difference in completed request count between periods.
        /// </summary>
        public int RequestsCompletedCountDelta { get; set; }

        /// <summary>
        /// Difference in average completion days between periods.
        /// </summary>
        public decimal AverageCompletionDaysDelta { get; set; }
    }
}

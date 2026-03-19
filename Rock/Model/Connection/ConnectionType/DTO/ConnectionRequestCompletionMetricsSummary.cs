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
    /// Represents completion related metrics for connection requests
    /// within a specified date range for a single connection type.
    /// </summary>
    internal class ConnectionRequestCompletionMetricsSummary
    {
        /// <summary>
        /// The identifier of the connection type this summary applies to.
        /// </summary>
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// The percentage of completed connection requests that were
        /// completed on or before their due date.
        /// </summary>
        public decimal TimelinessPercent { get; set; }

        /// <summary>
        /// The average number of days between request creation and
        /// first logged activity.
        /// </summary>
        public decimal AverageResponsivenessDays { get; set; }

        /// <summary>
        /// The total number of connection requests completed
        /// within the date range.
        /// </summary>
        public int RequestsCompletedCount { get; set; }

        /// <summary>
        /// The average number of days between request creation and
        /// completion.
        /// </summary>
        public decimal AverageCompletionDays { get; set; }
    }
}

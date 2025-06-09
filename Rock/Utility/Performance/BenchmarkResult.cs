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

namespace Rock.Utility.Performance
{
    /// <summary>
    /// Contains the results of a single benchmark measurement.
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// Gets the overall statistics from the measurement. This is normalized
        /// as best as possible to account for overhead and other elements that
        /// might otherwise skew the results.
        /// </summary>
        /// <value>The overall statistics.</value>
        public BenchmarkStatistics NormalizedStatistics { get; }

        /// <summary>
        /// Gets the statistics for the total run. This includes all overhead as well.
        /// </summary>
        public BenchmarkStatistics Statistics { get; }

        /// <summary>
        /// Gets the statistics from the overhead calculation.
        /// </summary>
        public BenchmarkStatistics Overhead { get; }

        /// <summary>
        /// Creates a new instance of <see cref="BenchmarkResult"/>.
        /// </summary>
        /// <param name="normalizedStatistics">The normalized statistics from the measurement.</param>
        /// <param name="statistics">The overall statistics from the measurement.</param>
        /// <param name="overhead">The overhead statistics from the measurement.</param>
        internal BenchmarkResult( BenchmarkStatistics normalizedStatistics, BenchmarkStatistics statistics, BenchmarkStatistics overhead )
        {
            NormalizedStatistics = normalizedStatistics;
            Statistics = statistics;
            Overhead = overhead;
        }
    }
}

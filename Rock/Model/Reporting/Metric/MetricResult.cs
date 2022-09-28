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

namespace Rock.Model
{
    /// <summary>
    /// POCO for <see cref="Metric"/> results.
    /// </summary>
    public class MetricResult
    {
        /// <summary>
        /// Integer count of <see cref="MetricValue">MetricValues</see> that were calculated.
        /// </summary>
        public int MetricValuesCalculated { get; set; }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public MetricResult()
        {
        }

        /// <summary>
        /// Potential exception from calculating <see cref="MetricValue">MetricValues</see>.
        /// </summary>
        public Exception MetricException { get; set; }

        /// <summary>
        /// Constructor when values calculated are available.
        /// </summary>
        /// <param name="metricsValuesCalculated"></param>
        public MetricResult( int metricsValuesCalculated )
        {
            MetricValuesCalculated = metricsValuesCalculated;
        }
    }
}

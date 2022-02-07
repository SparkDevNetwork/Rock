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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Web Farm Node Metric Service
    /// </summary>
    public partial class WebFarmNodeMetricService
    {
        /// <summary>
        /// Calculates the metric samples. This method calculates a sample timespan based on the min and max dates with the
        /// number of desired samples (ex: Between 1am and 2am with 60 samples would yield a sample timespan of 1 minute).
        /// For each sample timespan, the average metric value is calculated and returned as part of the list.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="sampleCount">The sample count.</param>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="maxDate">The maximum date.</param>
        /// <returns></returns>
        public static decimal[] CalculateMetricSamples(
            IEnumerable<MetricViewModel> metrics,
            int sampleCount,
            DateTime minDate,
            DateTime maxDate )
        {
            var secondsPerSample = ( maxDate - minDate ).TotalSeconds / sampleCount;
            var samples = new decimal[sampleCount];

            for ( var i = 0; i < sampleCount; i++ )
            {
                var sampleMinDate = minDate.AddSeconds( i * secondsPerSample );
                var sampleMaxDate = sampleMinDate.AddSeconds( secondsPerSample );

                var metricValues = metrics
                    .Where( wfnm =>
                        wfnm.MetricValueDateTime >= sampleMinDate &&
                        wfnm.MetricValueDateTime <= sampleMaxDate )
                    .Select( wfnm => wfnm.MetricValue );

                samples[i] = metricValues.Any() ? metricValues.Max() : 0;
            }

            return samples;
        }

        #region ViewModels

        /// <summary>
        /// Metric date and value
        /// </summary>
        public sealed class MetricViewModel
        {
            /// <summary>
            /// Gets or sets the metric date time.
            /// </summary>
            /// <value>
            /// The metric date time.
            /// </value>
            public DateTime MetricValueDateTime { get; set; }

            /// <summary>
            /// Gets or sets the metric value.
            /// </summary>
            /// <value>
            /// The metric value.
            /// </value>
            public decimal MetricValue { get; set; }
        }

        #endregion ViewModels
    }
}

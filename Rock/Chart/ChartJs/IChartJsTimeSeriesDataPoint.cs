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

namespace Rock.Chart
{
    /// <summary>
    /// A chart data point that represents a value at a specific instant in time, suitable for use with a value-over-time chart.
    /// Time series datapoints must support these properties, and may also include additional custom properties that can
    /// be referenced in client-side Javascript.
    /// </summary>
    public interface IChartJsTimeSeriesDataPoint
    {
        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        decimal Value { get; set; }
    }
}
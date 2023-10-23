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

namespace Rock.ViewModels.Blocks.WebFarm.WebFarmNodeDetail
{
    /// <summary>
    /// Identifies a WebFarm metric item.
    /// </summary>
    public class WebFarmMetricBag
    {
        /// <summary>
        /// Gets or sets the metric value date time.
        /// </summary>
        /// <value>
        /// The metric value date time.
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
}

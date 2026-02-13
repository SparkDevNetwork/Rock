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

using Rock.ViewModels.Blocks.Reporting;

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// Defines the contract for a chart bag, which contains the labels, series data, and chart options for a chart.
    /// </summary>
    public class ChartBag
    {
        /// <summary>
        /// Gets or sets the labels for the chart.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="string"/> values representing the labels for the chart.
        /// </value>
        public List<string> Labels { get; set; }

        /// <summary>
        /// Gets or sets the series bags for the chart.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="IChartSeriesBag"/> objects representing the data series for the chart.
        /// </value>
        public List<IChartSeriesBag> SeriesBags { get; set; }

        /// <summary>
        /// Gets or sets the chart options bag.
        /// </summary>
        /// <value>
        /// A <see cref="ChartOptionsBag"/> object containing configuration options for the chart.
        /// </value>
        public ChartOptionsBag ChartOptionsBag { get; set; }
    }
}

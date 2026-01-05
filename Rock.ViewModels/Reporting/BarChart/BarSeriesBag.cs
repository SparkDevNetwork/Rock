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

using Rock.ViewModels.Blocks.Reporting;

namespace Rock.ViewModels.Reporting.BarChart
{
    /// <summary>
    /// Represents a single bar series in a bar chart, including its label, data points, colors, opacity, and label configuration.
    /// </summary>
    public class BarSeriesBag : IChartSeriesBag
    {
        /// <summary>
        /// Gets or sets the label for the bar series.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the data points for the bar series.
        /// </summary>
        public List<double> Data { get; set; }

        /// <summary>
        /// Gets or sets the color of the bars.
        /// Can be a single color value or a comma-separated list for each data point.
        /// </summary>
        public List<string> Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bars are unfilled.
        /// If true, bars are rendered as outlines; otherwise, they are filled.
        /// </summary>
        public bool? IsUnfilled { get; set; }
    }
}
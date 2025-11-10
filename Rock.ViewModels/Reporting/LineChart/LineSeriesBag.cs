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

namespace Rock.ViewModels.Reporting.LineChart
{
    /// <summary>
    /// Represents a single line series in a line chart, including its label, data points, color, opacity, and label configuration.
    /// </summary>
    public class LineSeriesBag : IChartSeriesBag
    {
        /// <summary>
        /// Gets or sets the label for the line series.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the data points for the line series.
        /// </summary>
        public List<double> Data { get; set; }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the underside of the line has a fill.
        /// </summary>
        public bool? IsUnfilled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transformation is linear.
        /// </summary>
        public bool IsLinear { get; set; }

        /// <summary>
        /// Gets or sets the style of the line used in rendering. Allowed values: "solid", "dashed", "dotted".
        /// </summary>
        public string LineStyle { get; set; }
    }
}
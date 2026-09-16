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

namespace Rock.ViewModels.Blocks.Reporting.DynamicChart
{
    /// <summary>
    /// The configuration options and chart data for the Dynamic Chart block.
    /// </summary>
    public class DynamicChartOptionsBag
    {
        /// <summary>
        /// Gets or sets the configuration or data-retrieval error to display
        /// instead of the chart. When set, no chart is rendered.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the title of the chart widget.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the subtitle of the chart widget.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the type of chart to render. One of "line", "bar" or "pie".
        /// </summary>
        public string ChartType { get; set; }

        /// <summary>
        /// Gets or sets the height of the chart in pixels.
        /// </summary>
        public int ChartHeight { get; set; }

        /// <summary>
        /// Gets or sets whether the chart legend is shown.
        /// </summary>
        public bool IsLegendShown { get; set; }

        /// <summary>
        /// Gets or sets the position of the chart legend as a compass point
        /// ("n", "ne", "e", "se", "s", "sw", "w" or "nw").
        /// </summary>
        public string LegendPosition { get; set; }

        /// <summary>
        /// Gets or sets the inner radius of a pie chart as a fraction (0-1) of
        /// the outer radius. A value greater than zero renders a donut hole.
        /// </summary>
        public double PieInnerRadius { get; set; }

        /// <summary>
        /// Gets or sets whether a pie chart renders a label on each slice.
        /// </summary>
        public bool ArePieLabelsShown { get; set; }

        /// <summary>
        /// Gets or sets the width of the widget as a Bootstrap column count
        /// (1-12), or null to fill the available width.
        /// </summary>
        public int? ColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets whether the labels are ISO 8601 date/time values that
        /// should be plotted as a time series.
        /// </summary>
        public bool IsTimeSeries { get; set; }

        /// <summary>
        /// Gets or sets the category (or date/time) labels shared by every series.
        /// </summary>
        public List<string> Labels { get; set; }

        /// <summary>
        /// Gets or sets the data series to plot. Each series' values are aligned
        /// by index with <see cref="Labels"/>.
        /// </summary>
        public List<DynamicChartSeriesBag> Series { get; set; }
    }
}

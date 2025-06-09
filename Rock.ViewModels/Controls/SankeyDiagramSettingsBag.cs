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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Model of the settings for a Sankey Diagram
    /// </summary>
    public class SankeyDiagramSettingsBag
    {
        /// <summary>
        /// Gets or sets the Node width
        /// </summary>
        /// <value>
        /// How many pixels wide the Nodes should be
        /// </value>
        public int? NodeWidth { get; set; }

        /// <summary>
        /// Gets or sets the Node vertical spacing
        /// </summary>
        /// <value>
        /// The number of pixels that separate the Nodes vertically (gap)
        /// </value>
        public int? NodeVerticalSpacing { get; set; }

        /// <summary>
        /// Gets or sets the chart width
        /// </summary>
        /// <value>
        /// The number of pixels wide the chart should be
        /// </value>
        public int? ChartWidth { get; set; }

        /// <summary>
        /// Gets or sets the chart height
        /// </summary>
        /// <value>
        /// The number of pixels high the chart should be
        /// </value>
        public int? ChartHeight { get; set; }

        /// <summary>
        /// Gets or sets the legend HTML
        /// </summary>
        /// <value>
        /// The generated HTML string for the chart legend
        /// </value>
        public string LegendHtml { get; set; }
    }
}

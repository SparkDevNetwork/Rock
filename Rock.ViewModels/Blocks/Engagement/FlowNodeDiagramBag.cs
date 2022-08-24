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

namespace Rock.ViewModels.Blocks.Engagement.Steps
{
    /// <summary>
    /// Type for a Flow Node Diagram's Node definitions.
    /// For Step Flow diagrams, this represents a Step
    /// </summary>
    public class FlowNodeDiagramNodeBag
    {
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        /// <value>
        /// ID of the Node
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// <value>
        /// Display name of the Node
        /// </value>
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the color
        /// </summary>
        /// <value>
        /// Color of the Node on the diagram
        /// </value>
        public String Color { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        /// <value>
        /// Order that the Nodes should be displayed in.
        /// </value>
        public int Order { get; set; }
    }

    /// <summary>
    /// A description of the number of items flowing from a Node on
    /// one level to a node on the next level
    /// </summary>
    public class FlowNodeDiagramEdgeBag
    {
        /// <summary>
        /// Gets or sets the target ID
        /// </summary>
        /// <value>
        /// The ID of the Node that items are flowing into.
        /// </value>
        public int TargetId { get; set; }

        /// <summary>
        /// Gets or sets the source ID
        /// </summary>
        /// <value>
        /// The ID of the Node the items are flowing from.
        /// If null, this represents the data that exists in the first level Nodes.
        /// </value>
        public int? SourceId { get; set; }

        /// <summary>
        /// Gets or sets the level
        /// </summary>
        /// <value>
        /// The level of target Node. Think of it like a step number. The first level
        /// is the starting place, then level 2 is the first step from that place and so on.
        /// </value>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the units
        /// </summary>
        /// <value>
        /// The number of items flowing between the Nodes at this level
        /// </value>
        public int Units { get; set; }

        /// <summary>
        /// Gets or sets the tooltip
        /// </summary>
        /// <value>
        /// HTML code for a tooltip to show on hover of this Edge
        /// </value>
        public string Tooltip { get; set; }
    }

    /// <summary>
    /// Model of the settings for the Flow Node Diagram
    /// </summary>
    public class FlowNodeDiagramSettingsBag
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
    }
}

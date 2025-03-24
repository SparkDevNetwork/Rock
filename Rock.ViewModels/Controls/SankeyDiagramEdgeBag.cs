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
    /// A description of the number of items flowing from a Node on
    /// one level to a node on the next level
    /// </summary>
    public class SankeyDiagramEdgeBag
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
}

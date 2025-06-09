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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Type for a Sankey Diagram's Node definitions.
    /// For Step Flow diagrams, this represents a Step
    /// </summary>
    public class SankeyDiagramNodeBag
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
}
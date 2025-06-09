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
using Rock.ViewModels.Utility;
using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Engagement.Steps
{
    /// <summary>
    /// Box of properties for initializing the StepNode block
    /// </summary>
    public class StepFlowInitializationBox : SankeyDiagramSettingsBag
    {
        /// <summary>
        /// Gets or sets the list of campuses
        /// </summary>
        /// <value>
        /// List of campuses to show in the campus selection dropdown
        /// </value>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the name of the step program
        /// </summary>
        /// <value>
        /// Name of the step program
        /// </value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the step type count
        /// </summary>
        /// <value>
        /// The number of step types in this step program
        /// </value>
        public int? StepTypeCount { get; set; }
    }

    /// <summary>
    /// Class StepFlowGetDataBag.
    /// </summary>
    public class StepFlowGetDataBag
    {
        /// <summary>
        /// Gets or sets the list of edges
        /// </summary>
        /// <value>
        /// List of Flow Diagram Edges
        /// </value>
        public List<SankeyDiagramEdgeBag> Edges { get; set; }

        /// <summary>
        /// Gets or sets the list of nodes
        /// </summary>
        /// <value>
        /// List of Flow Diagram Nodes
        /// </value>
        public List<SankeyDiagramNodeBag> Nodes { get; set; }
    }
}

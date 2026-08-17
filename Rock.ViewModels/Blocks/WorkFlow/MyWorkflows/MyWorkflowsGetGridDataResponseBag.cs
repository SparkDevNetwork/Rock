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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Workflow.MyWorkflows
{
    /// <summary>
    /// The response data for the workflows grid in the My Workflows block.
    /// Contains both the grid definition and data since columns change per workflow type.
    /// </summary>
    public class MyWorkflowsGetGridDataResponseBag
    {
        /// <summary>
        /// Gets or sets the grid data containing the rows of workflows.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets the grid definition describing the columns for the selected workflow type.
        /// This changes dynamically based on the type's grid attributes.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the display name of the selected workflow type.
        /// </summary>
        public string WorkflowTypeName { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the selected workflow type, used to build the entry page URL.
        /// </summary>
        public string WorkflowTypeIdKey { get; set; }
    }
}

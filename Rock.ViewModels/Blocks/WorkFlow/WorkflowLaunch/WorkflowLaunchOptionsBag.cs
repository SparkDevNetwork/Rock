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

namespace Rock.ViewModels.Blocks.WorkFlow.WorkflowLaunch
{
    /// <summary>
    /// The configuration-derived options for the Workflow Launch block.
    /// </summary>
    public class WorkflowLaunchOptionsBag
    {
        /// <summary>
        /// Gets or sets the title to display in the block panel header.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the icon shown before the panel title.
        /// </summary>
        public string PanelIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual may launch another
        /// workflow after one has already been launched.
        /// </summary>
        public bool AllowMultipleWorkflowLaunches { get; set; }

        /// <summary>
        /// Gets or sets the workflow types to choose from when two or more are configured.
        /// Empty when a picker (any type) or a single locked type is used instead.
        /// </summary>
        public List<ListItemBag> WorkflowTypeOptions { get; set; }
    }
}

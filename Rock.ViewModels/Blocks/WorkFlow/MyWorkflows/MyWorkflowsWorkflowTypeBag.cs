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

namespace Rock.ViewModels.Blocks.Workflow.MyWorkflows
{
    /// <summary>
    /// Represents a single workflow type tile in the My Workflows block.
    /// </summary>
    public class MyWorkflowsWorkflowTypeBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the workflow type.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display name of the workflow type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the workflow type's icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the count shown on the tile's badge. For "Initiated By Me" this is
        /// the number of active workflows; for "Assigned To Me" it is the number of active form actions.
        /// </summary>
        public int Count { get; set; }
    }
}

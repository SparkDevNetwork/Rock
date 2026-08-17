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

namespace Rock.ViewModels.Blocks.Workflow.ShareWorkflow
{
    /// <summary>
    /// A single entity that will be included when a workflow type is exported,
    /// shown as a row in the export preview grid.
    /// </summary>
    public class ShareWorkflowPreviewEntityBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the entity, used as the grid row key.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the friendly name used to identify the entity to the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short entity type name (for example, "WorkflowActivityType").
        /// </summary>
        public string ShortType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is critical to the export,
        /// meaning the export cannot complete without it.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity will be assigned a new
        /// unique identifier when it is imported.
        /// </summary>
        public bool IsNewGuid { get; set; }

        /// <summary>
        /// Gets or sets the reference paths that describe how this entity is reached
        /// from the root workflow type being exported.
        /// </summary>
        public List<string> Paths { get; set; }
    }
}

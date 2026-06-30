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

namespace Rock.ViewModels.Blocks.WorkFlow.WorkflowLaunch
{
    /// <summary>
    /// The runtime data for the Workflow Launch block.
    /// </summary>
    public class WorkflowLaunchBag
    {
        /// <summary>
        /// Gets or sets the error message to display when the entity set is missing or
        /// invalid. When set, the launcher UI is hidden.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the pluralized friendly name of the entity type contained in the
        /// entity set (for example, "People").
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the preview items, limited to the configured maximum number to show.
        /// </summary>
        public List<WorkflowLaunchItemBag> Items { get; set; }

        /// <summary>
        /// Gets or sets the total number of items in the entity set, used to build the
        /// "...and N more" summary.
        /// </summary>
        public int TotalItemCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow type when the selection is locked (a single
        /// configured type or a workflow type page parameter). Null when the individual
        /// chooses the type.
        /// </summary>
        public string LockedWorkflowTypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether workflows have already been launched (for
        /// example, via the bypass-confirm page parameter).
        /// </summary>
        public bool HasLaunched { get; set; }

        /// <summary>
        /// Gets or sets the success message shown after workflows have been launched.
        /// </summary>
        public string SuccessMessage { get; set; }
    }
}

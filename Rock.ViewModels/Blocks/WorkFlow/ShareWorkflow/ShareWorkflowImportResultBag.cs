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
    /// The result of importing a workflow type file.
    /// </summary>
    public class ShareWorkflowImportResultBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the import completed successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the messages produced during the import, describing what was
        /// (or would be) created or updated.
        /// </summary>
        public List<string> Messages { get; set; }
    }
}

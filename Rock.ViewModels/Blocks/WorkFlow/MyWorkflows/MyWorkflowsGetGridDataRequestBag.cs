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
    /// The request data for retrieving the workflows grid in the My Workflows block.
    /// </summary>
    public class MyWorkflowsGetGridDataRequestBag
    {
        /// <summary>
        /// Gets or sets the Guid of the selected workflow type to load workflows for.
        /// </summary>
        public Guid? WorkflowTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role filter is "Initiated By Me"
        /// (<c>true</c>) rather than "Assigned To Me" (<c>false</c>). This determines which
        /// workflows of the selected type are returned.
        /// </summary>
        public bool IsInitiatedByMe { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Workflow.MyWorkflows
{
    /// <summary>
    /// The initial state for the My Workflows block. Toggle values are resolved
    /// server-side from the query string (which overrides) or person preferences.
    /// </summary>
    public class MyWorkflowsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the role filter is set to
        /// "Initiated By Me" (<c>true</c>) rather than "Assigned To Me" (<c>false</c>).
        /// </summary>
        public bool IsInitiatedByMe { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the display filter is limited to
        /// "Active Types" (<c>true</c>) rather than "All Types" (<c>false</c>).
        /// </summary>
        public bool IsActiveTypesOnly { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Workflow.WorkflowNavigation
{
    /// <summary>
    /// The additional configuration options for the Workflow Navigation block.
    /// </summary>
    public class WorkflowNavigationOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current person has
        /// block-level Edit authorization, which allows managing every workflow
        /// type regardless of the per-type permissions.
        /// </summary>
        public bool IsBlockEditAuthorized { get; set; }
    }
}

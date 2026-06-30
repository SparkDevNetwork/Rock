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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Workflow.ShareWorkflow
{
    /// <summary>
    /// The initialization state for the Share Workflow block.
    /// </summary>
    public class ShareWorkflowBag
    {
        /// <summary>
        /// Gets or sets the workflow type to pre-select when the block loads,
        /// resolved from the page parameter. <c>null</c> when no workflow type
        /// was supplied or it could not be found.
        /// </summary>
        public ListItemBag InitialWorkflowType { get; set; }
    }
}

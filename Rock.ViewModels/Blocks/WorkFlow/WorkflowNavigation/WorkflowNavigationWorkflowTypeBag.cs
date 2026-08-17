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
    /// Represents a single workflow type within a category, including the
    /// information needed to render its launch and manage links.
    /// </summary>
    public class WorkflowNavigationWorkflowTypeBag
    {
        /// <summary>
        /// Gets or sets the encoded identifier key of the workflow type.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CSS class used for the workflow type's icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new workflow of this type
        /// can be launched. This is <c>true</c> only when the type is active and
        /// has at least one active entry form.
        /// </summary>
        public bool IsLaunchEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is
        /// authorized to manage (edit) workflows of this type.
        /// </summary>
        public bool CanManage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is
        /// authorized to view the list of workflows of this type.
        /// </summary>
        public bool CanViewList { get; set; }
    }
}

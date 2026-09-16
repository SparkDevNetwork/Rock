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

namespace Rock.ViewModels.Blocks.Workflow.WorkflowNavigation
{
    /// <summary>
    /// Represents a single workflow category node in the navigation tree,
    /// including its workflow types and any nested child categories.
    /// </summary>
    public class WorkflowNavigationCategoryBag
    {
        /// <summary>
        /// Gets or sets the encoded identifier key of the category.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CSS class used for the category's icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the workflow types that belong directly to this category.
        /// </summary>
        public List<WorkflowNavigationWorkflowTypeBag> WorkflowTypes { get; set; }

        /// <summary>
        /// Gets or sets the child categories nested beneath this category.
        /// </summary>
        public List<WorkflowNavigationCategoryBag> ChildCategories { get; set; }
    }
}

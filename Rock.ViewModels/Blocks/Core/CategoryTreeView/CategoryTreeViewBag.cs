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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.CategoryTreeView
{
    /// <summary>
    /// The runtime data the Category Tree View block ships to its Obsidian component.
    /// </summary>
    public class CategoryTreeViewBag
    {
        /// <summary>
        /// Gets or sets the categories selected on load, resolved from the page parameter for deep-linking.
        /// </summary>
        public List<Guid> SelectedCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the categories to expand on load.
        /// </summary>
        public List<Guid> ExpandedCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets an error message to display in place of the tree.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether the current person may edit, which gates the tree's add affordances.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets whether inactive items are hidden, reflecting the person's saved preference.
        /// Only meaningful when the active filter is shown.
        /// </summary>
        public bool HideInactiveItems { get; set; }

        /// <summary>
        /// Gets or sets whether the active/inactive filter toggle is shown, which is true when the
        /// configured entity type tracks an active flag.
        /// </summary>
        public bool IsActiveFilterVisible { get; set; }
    }
}

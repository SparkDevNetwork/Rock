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

namespace Rock.ViewModels.Blocks.Store
{
    /// <summary>
    /// The box that contains the initialization information for the Package Category List block.
    /// Implements the <see cref="Rock.ViewModels.Blocks.BlockBox" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class PackageCategoryListBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the layout used to render the category list. Either
        /// "Sidebar" (vertical list) or "Header" (horizontal pill bar).
        /// </summary>
        public string DisplayStyle { get; set; }

        /// <summary>
        /// Gets or sets the store package categories to display.
        /// </summary>
        public List<PackageCategoryListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the configured detail page. Category links
        /// append the CategoryId and CategoryName query string parameters to this value.
        /// </summary>
        public string DetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the store error message. When non-empty, the store could not be
        /// reached and the component renders the "Store Currently Not Available" panel
        /// instead of the category list.
        /// </summary>
        public string StoreErrorMessage { get; set; }
    }
}

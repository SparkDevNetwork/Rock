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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// Identifies a single search filter that should be displayed to the
    /// individual for them to limit the results that will be returned.
    /// </summary>
    public class SearchFilterBag
    {
        /// <summary>
        /// Gets or sets the label to identify the search filter.
        /// </summary>
        /// <value>The label to identify the search filter.</value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the control type to use when rendering the filter.
        /// </summary>
        /// <value>The control type to use when rendering the filter.</value>
        public Rock.Enums.Cms.ContentCollectionFilterControl Control { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this filter supports
        /// multiple selection.
        /// </summary>
        /// <value><c>true</c> if this filter supports multiple selection; otherwise, <c>false</c>.</value>
        public bool IsMultipleSelection { get; set; }

        /// <summary>
        /// Gets or sets the items to allow the individual to pick from.
        /// </summary>
        /// <value>The items to allow the individual to pick from.</value>
        public List<ListItemBag> Items { get; set; }

        /// <summary>
        /// Gets or sets the markup to display above the filter control.
        /// </summary>
        /// <value>The markup to display above the filter control.</value>
        public string HeaderMarkup { get; set; }
    }
}

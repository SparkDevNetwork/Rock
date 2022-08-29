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

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// The box that contains all the initialization information for the
    /// Content Collection View block.
    /// </summary>
    public class ContentCollectionInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the full text search panel
        /// should be visible.
        /// </summary>
        /// <value><c>true</c> if the full text search panel should be visible; otherwise, <c>false</c>.</value>
        public bool ShowFullTextSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filters panel
        /// should be visible.
        /// </summary>
        /// <value><c>true</c> if the filters panel should be visible; otherwise, <c>false</c>.</value>
        public bool ShowFiltersPanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sort option should be shown.
        /// </summary>
        /// <value><c>true</c> if the sort option should be shown; otherwise, <c>false</c>.</value>
        public bool ShowSort { get; set; }

        /// <summary>
        /// Gets or sets the enabled sort orders options.
        /// </summary>
        /// <value>The enabled sort orders options.</value>
        public List<string> EnabledSortOrders { get; set; }

        /// <summary>
        /// Gets or sets the trending term to use when sorting.
        /// </summary>
        /// <value>The trending term to use when sorting.</value>
        public string TrendingTerm { get; set; }

        /// <summary>
        /// Gets or sets the filters that should be displayed in the filters panel.
        /// </summary>
        /// <value>The filters that should be displayed in the filters panel.</value>
        public List<SearchFilterBag> Filters { get; set; }

        /// <summary>
        /// Gets or sets the content to be displayed before the first search.
        /// </summary>
        /// <value>The content to be displayed before the first search.</value>
        public string PreSearchContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show search results
        /// on initial load.
        /// </summary>
        /// <value><c>true</c> if to show search results on initial load; otherwise, <c>false</c>.</value>
        public bool SearchOnLoad { get; set; }

        /// <summary>
        /// Gets or sets the initial results to be displayed when the block loads.
        /// </summary>
        /// <value>The initial results to be displayed when the block loads.</value>
        public SearchResultBag InitialResults { get; set; }
    }
}

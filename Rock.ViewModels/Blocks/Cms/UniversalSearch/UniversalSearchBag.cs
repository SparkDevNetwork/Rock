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

using Rock.ViewModels.Blocks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.UniversalSearch
{
    /// <summary>
    /// Contains the initialization data for the Universal Search block.
    /// </summary>
    public class UniversalSearchBag : BlockBox
    {
        /// <summary>
        /// Gets or sets the current query text.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the effective search type value.
        /// </summary>
        public int SearchTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the number of results to show per page.
        /// </summary>
        public int ResultsPerPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the model filter should be shown.
        /// </summary>
        public bool ShowModelFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the refine search toggle should be shown.
        /// </summary>
        public bool ShowRefinedSearchToggle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the refine search panel is visible.
        /// </summary>
        public bool IsRefinedSearchVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current request originated from smart search.
        /// </summary>
        public bool IsSmartSearchRequest { get; set; }

        /// <summary>
        /// Gets or sets the HTML that should be rendered before the search input.
        /// </summary>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML that should be rendered after the search input.
        /// </summary>
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets or sets the available entity type options.
        /// </summary>
        public List<ListItemBag> AvailableModels { get; set; }

        /// <summary>
        /// Gets or sets the currently selected entity type identifiers.
        /// </summary>
        public List<string> SelectedModelIds { get; set; }

        /// <summary>
        /// Gets or sets the available field filters.
        /// </summary>
        public List<UniversalSearchFilterBag> Filters { get; set; }

        /// <summary>
        /// Gets or sets the currently selected filter values keyed by filter field.
        /// </summary>
        public Dictionary<string, List<string>> SelectedFilters { get; set; }

        /// <summary>
        /// Gets or sets the initial search results.
        /// </summary>
        public UniversalSearchResultsBag InitialResults { get; set; }

        /// <summary>
        /// Gets or sets the URL to redirect to when the request identifies a specific document.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}

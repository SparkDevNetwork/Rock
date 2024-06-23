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

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// The settings that will be edited in the custom settings panel for the
    /// Content Collection View block.
    /// </summary>
    public class CustomSettingsBag
    {
        /// <summary>
        /// Gets or sets the content collection that is currently selected.
        /// </summary>
        /// <value>The content collection that is currently selected.</value>
        public Guid? ContentCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filters panel
        /// should be visible.
        /// </summary>
        /// <value><c>true</c> if the filters panel should be visible; otherwise, <c>false</c>.</value>
        public bool ShowFiltersPanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the full text search
        /// panel should be visible.
        /// </summary>
        /// <value><c>true</c> if the full text search panel should be visible; otherwise, <c>false</c>.</value>
        public bool ShowFullTextSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sorting control
        /// should be shown.
        /// </summary>
        /// <value><c>true</c> if the sorting control should be shown; otherwise, <c>false</c>.</value>
        public bool ShowSort { get; set; }

        /// <summary>
        /// Gets or sets the number of results to return per source. If grouping
        /// is not enabled then this controls the total number of results.
        /// </summary>
        /// <value>The number of results to return per source.</value>
        public int? NumberOfResults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to perform an initial
        /// search if no query string parameters are provided. If query string
        /// parameters are provided then search will always be performed on load.
        /// </summary>
        /// <value><c>true</c> if to perform an initial search if no query string parameters are provided; otherwise, <c>false</c>.</value>
        public bool SearchOnLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether results should be grouped
        /// by source.
        /// </summary>
        /// <value><c>true</c> if results should be grouped by source; otherwise, <c>false</c>.</value>
        public bool GroupResultsBySource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which sort orders are available to
        /// be used by the individual
        /// </summary>
        /// <value>The enabled sort orders available to use.</value>
        public List<string> EnabledSortOrders { get; set; }

        /// <summary>
        /// Gets or sets the trending term to use when displaying the sort option.
        /// </summary>
        /// <value>The trending term to use when displaying the sort option.</value>
        public string TrendingTerm { get; set; }

        /// <summary>
        /// Gets or sets the filters that are configured.
        /// </summary>
        /// <value>The filters that are configured.</value>
        public List<FilterOptionsBag> Filters { get; set; }

        /// <summary>
        /// Gets or sets the template that will be used to render the search
        /// results container. It must have a div with result-items class
        /// and an optional element with show-more class to act as a show more
        /// button.
        /// </summary>
        /// <value>
        /// The template that will be used to render the search results container.
        /// </value>
        public string ResultsTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template that will be used to render each individual
        /// search result item.
        /// </summary>
        /// <value>
        /// The template that will be used to render each individual search
        /// result item.
        /// </value>
        public string ItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template that will be used to render the content
        /// to display in the search results area before a search happens.
        /// </summary>
        /// <value>
        /// Teh template that will be used to render the content to display
        /// in the search results area before a search happens.
        /// </value>
        public string PreSearchTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template that will be used to render above each content
        /// collection source group (mobile only).
        /// </summary>
        public string GroupHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether results with matching
        /// personalization segments should be boosted.
        /// </summary>
        /// <value><c>true</c> if results with matching personalization segments should be boosted; otherwise, <c>false</c>.</value>
        public bool BoostMatchingSegments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether results with matching
        /// request segments should be boosted.
        /// </summary>
        /// <value><c>true</c> if results with matching request segments should be boosted; otherwise, <c>false</c>.</value>
        public bool BoostMatchingRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the segment boost amount.
        /// </summary>
        /// <value>The segment boost amount.</value>
        public decimal? SegmentBoostAmount { get; set; }

        /// <summary>
        /// Gets or sets the request filter boost amount.
        /// </summary>
        /// <value>The request filter boost amount.</value>
        public decimal? RequestFilterBoostAmount { get; set; }

        /// <summary>
        /// Gets or sets the site type for the block.
        /// </summary>
        public string SiteType { get; set; }
    }
}

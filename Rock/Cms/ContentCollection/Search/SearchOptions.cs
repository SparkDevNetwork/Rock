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

namespace Rock.Cms.ContentCollection.Search
{
    /// <summary>
    /// Optional search options to apply to the search request.
    /// </summary>
    internal class SearchOptions
    {
        /// <summary>
        /// Gets or sets the starting result number to retrieve when paging results.
        /// </summary>
        /// <value>The starting result number to retrieve when paging results.</value>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results to return. If not specified
        /// then the index engine will pick a value.
        /// </summary>
        /// <value>The maximum number of results to return.</value>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the results.
        /// </summary>
        /// <value>The sort order of the results.</value>
        public SearchSortOrder Order { get; set; } = SearchSortOrder.Relevance;

        /// <summary>
        /// Gets or sets a value indicating whether the ordering is descending.
        /// </summary>
        /// <value><c>true</c> if the ordering is descending; otherwise, <c>false</c>.</value>
        public bool IsDescending { get; set; } = true;
    }
}

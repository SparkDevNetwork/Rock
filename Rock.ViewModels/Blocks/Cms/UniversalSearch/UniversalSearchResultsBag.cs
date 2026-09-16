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

namespace Rock.ViewModels.Blocks.Cms.UniversalSearch
{
    /// <summary>
    /// Contains the rendered results for a Universal Search request.
    /// </summary>
    public class UniversalSearchResultsBag
    {
        /// <summary>
        /// Gets or sets the rendered search result markup.
        /// </summary>
        public string ResultsHtml { get; set; }

        /// <summary>
        /// Gets or sets the warning message to display.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the total number of results available for the query.
        /// </summary>
        public long TotalResultsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the zero-based page number that produced these results.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the number of results shown per page.
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL to follow when a specific document was identified.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}

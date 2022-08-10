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
    /// The ordering of the results from a search request.
    /// </summary>
    internal enum SearchSortOrder
    {
        /// <summary>
        /// Sort by the native score returned by the index engine.
        /// </summary>
        Relevance = 0,

        /// <summary>
        /// Sort by the date the item's relevant date and time.
        /// </summary>
        RelevantDate = 1,

        /// <summary>
        /// Sort by the trending rank of the item with fallback to score.
        /// </summary>
        Trending = 2,

        /// <summary>
        /// Sort the items alphabetically with fallback to score.
        /// </summary>
        Alphabetical = 3
    }
}

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

using Rock.Cms.ContentCollection.IndexDocuments;

namespace Rock.Cms.ContentCollection.Search
{
    /// <summary>
    /// The results of a content collection search request.
    /// </summary>
    internal class SearchResults
    {
        /// <summary>
        /// An empty result set.
        /// </summary>
        public static readonly SearchResults Empty = new SearchResults
        {
            Documents = new List<IndexDocumentBase>()
        };

        /// <summary>
        /// Gets or sets the total results available that match the query.
        /// </summary>
        /// <value>
        /// The total results available that match the query.
        /// </value>
        public int TotalResultsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the documents that matched the query and pagination.
        /// </summary>
        /// <value>
        /// The documents that matched the query and pagination.
        /// </value>
        public List<IndexDocumentBase> Documents { get; set; }
    }
}
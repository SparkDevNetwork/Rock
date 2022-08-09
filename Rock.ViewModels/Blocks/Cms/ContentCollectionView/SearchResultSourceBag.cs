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
    /// A single source that contains a number of results in response
    /// to a search query.
    /// </summary>
    public class SearchResultSourceBag
    {
        /// <summary>
        /// Gets or sets the source unique identifier. This will be an empty
        /// Guid if grouping has not been enabled in the block settings.
        /// </summary>
        /// <value>The source unique identifier.</value>
        public Guid SourceGuid { get; set; }

        /// <summary>
        /// Gets or sets the template to use when rendering the search
        /// results container.
        /// </summary>
        /// <value>The template to use when rendering the search results container.</value>
        public string Template { get; set; }

        /// <summary>
        /// Gets or sets the rendered HTML that describes each of the results.
        /// </summary>
        /// <value>The rendered HTML that describes each of the results.</value>
        public List<string> Results { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this source has more results
        /// available to load.
        /// </summary>
        /// <value><c>true</c> if this instance has more results; otherwise, <c>false</c>.</value>
        public bool HasMore { get; set; }
    }
}

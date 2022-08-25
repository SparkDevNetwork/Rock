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

using Rock.Enums.Blocks.Cms.ContentCollectionView;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// Defines a query against the content collection for the Content Collection
    /// View block.
    /// </summary>
    public class SearchQueryBag
    {
        /// <summary>
        /// Gets or sets the text to use when searching for content.
        /// </summary>
        /// <value>The text to use when searching for content.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the source unique identifier to load results for. This
        /// is used by the show more feature.
        /// </summary>
        /// <value>The source unique identifier to load results for.</value>
        public Guid? SourceGuid { get; set; }

        /// <summary>
        /// Gets or sets the offset to start loading results for. This is used
        /// by the show more feature.
        /// </summary>
        /// <value>The offset to start loading results for.</value>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the filter values to use when searching for results.
        /// </summary>
        /// <value>The filter values to use when searching for results.</value>
        public Dictionary<string, string> Filters { get; set; }

        /// <summary>
        /// Gets or sets the order to sort results into.
        /// </summary>
        /// <value>The order to sort results into.</value>
        public SearchOrder? Order { get; set; }
    }
}

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

namespace Rock.Enums.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// The search order options available in the Content Collection View block.
    /// </summary>
    public enum SearchOrder
    {
        /// <summary>
        /// The most relevant will be displayed first, this is based on the score.
        /// </summary>
        Relevance = 0,

        /// <summary>
        /// The document with the highest relevance date time will be displayed first.
        /// </summary>
        Newest = 1,

        /// <summary>
        /// The document with the oldest relevance date time will be displayed first.
        /// </summary>
        Oldest = 2,

        /// <summary>
        /// The most popular documents will be displayed first. This is determined
        /// by the trending values and will then fall back to best match.
        /// </summary>
        Trending = 3,

        /// <summary>
        /// The documents will be displayed alphabetically, A-Z.
        /// </summary>
        Alphabetical = 4
    }
}

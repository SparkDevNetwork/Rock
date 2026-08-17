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

namespace Rock.ViewModels.Blocks.Cms.CacheManager
{
    /// <summary>
    /// Contains aggregated cache statistics for the Cache Manager block.
    /// </summary>
    public class CacheStatisticsBag
    {
        /// <summary>
        /// Gets or sets the total number of cache hits.
        /// </summary>
        public long Hits { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache misses.
        /// </summary>
        public long Misses { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache add and put operations.
        /// </summary>
        public long Adds { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache get operations.
        /// </summary>
        public long Gets { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache clear operations.
        /// </summary>
        public long Clears { get; set; }
    }
}

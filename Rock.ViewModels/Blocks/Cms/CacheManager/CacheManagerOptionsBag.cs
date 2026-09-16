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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.CacheManager
{
    /// <summary>
    /// The options bag for the Cache Manager block.
    /// </summary>
    public class CacheManagerOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of cache types available for selection
        /// in the cache type dropdown. Includes an "All Cached Items" option.
        /// </summary>
        public List<ListItemBag> CacheTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cache statistics
        /// tracking is currently enabled in the web.config.
        /// </summary>
        public bool IsStatisticsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user
        /// is authorized to add and edit cache tags.
        /// </summary>
        public bool IsEditAuthorized { get; set; }
    }
}

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

namespace Rock.Web.Cache
{
    /// <summary>
    /// Interface for RockCacheManager
    /// </summary>
    public interface IRockCacheManager
    {
        /// <summary>
        /// Clears a cache instance.
        /// </summary>
        [Obsolete( "This can cause performance issues, especially with distributed caches. If you really need to clear, use ClearAll() " )]
        void Clear();

        /// <summary>
        /// If using a Distributed Cache, this will flush the entire cache. Otherwise, it will just flush the cache for this type
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Gets the statistics for the cache instance.
        /// </summary>
        /// <returns></returns>
        CacheItemStatistics GetStatistics();
    }
}

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
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of the Rock web cache for Lava Templates
    /// </summary>
    public class WebsiteLavaTemplateCache : ItemCache<WebsiteLavaTemplateCache>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Template object.
        /// </summary>
        public ILavaTemplate Template { get; set; }

        #endregion

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// The CACHE_CONTROL_COOKIE will be inspected to see if cached value should be ignored.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="keyFactory">The key factory to create a list of keys for the type. This will only be used if a list does not already exist.</param>
        /// <returns></returns>
        internal protected static new WebsiteLavaTemplateCache GetOrAddExisting( string key, Func<WebsiteLavaTemplateCache> itemFactory, Func<List<string>> keyFactory = null )
        {
            if ( System.Web.HttpContext.Current != null )
            {
                var isCachedEnabled = System.Web.HttpContext.Current.Request.Cookies.Get( RockCache.CACHE_CONTROL_COOKIE );
                if ( isCachedEnabled != null && !isCachedEnabled.Value.AsBoolean() )
                {
                    return null;
                }
            }

            return ItemCache<WebsiteLavaTemplateCache>.GetOrAddExisting( key, itemFactory, keyFactory );
        }
    }
}
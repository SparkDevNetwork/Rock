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
using Newtonsoft.Json;

namespace Rock.Cache
{
    /// <summary>
    /// Class for adding generic items to cache
    /// </summary>
    public class RockCache
    {
        private static readonly object _obj = new object();
        private static List<IRockCacheManager> _allManagers;

        #region Private Static Methods

        /// <summary>
        /// Adds a new cache manager to the collection of all managers.
        /// </summary>
        /// <param name="manager">The manager.</param>
        internal static void AddManager( IRockCacheManager manager )
        {
            if ( manager == null ) return;

            lock ( _obj )
            {
                if ( _allManagers == null )
                {
                    _allManagers = new List<IRockCacheManager>();
                }
                _allManagers.Add( manager );
            }
        }

        /// <summary>
        /// Clears the cache for every manager in the collection.
        /// </summary>
        private static void ClearAll()
        {
            if ( _allManagers == null ) return;
            foreach ( var cacheManager in _allManagers )
            {
                cacheManager?.Clear();
            }
        }

        /// <summary>
        /// Updates the cache hit miss.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="hit">if set to <c>true</c> [hit].</param>
        internal static void UpdateCacheHitMiss( string key, bool hit )
        {
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext == null || !httpContext.Items.Contains( "Cache_Hits" ) ) return;

            var cacheHits = httpContext.Items["Cache_Hits"] as Dictionary<string, bool>;
            cacheHits?.AddOrIgnore( key, hit );
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Converts item to json string
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject( this, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets an item from cache using the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static object Get( string key )
        {
            return Get( key, null );
        }

        /// <summary>
        /// Gets an item from cache using the specified key and region.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public static object Get( string key, string region )
        {
            return GetOrAddExisting( key, region, null );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <returns></returns>
        public static object GetOrAddExisting( string key, Func<object> itemFactory )
        {
            return GetOrAddExisting( key, null, itemFactory );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <returns></returns>
        public static object GetOrAddExisting( string key, string region, Func<object> itemFactory )
        {
            return GetOrAddExisting( key, region, itemFactory, TimeSpan.MaxValue );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache with an expiration timespan.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        public static object GetOrAddExisting( string key, string region, Func<object> itemFactory, TimeSpan expiration )
        {
            var value = region.IsNotNullOrWhitespace() ?
                RockCacheManager<object>.Instance.Cache.Get( key, region ) :
                RockCacheManager<object>.Instance.Cache.Get( key );

            UpdateCacheHitMiss( key, value != null );

            if ( value != null )
            {
                return value;
            }

            if ( itemFactory == null ) return null;

            value = itemFactory();
            if ( value == null ) return null;

            if ( region.IsNotNullOrWhitespace() )
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, region, value, expiration );
            }
            else
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, value, expiration );
            }

            return value;
        }

        /// <summary>
        /// Adds or updates an item in cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="obj">The object.</param>
        public static void AddOrUpdate( string key, object obj )
        {
            AddOrUpdate( key, null, obj );
        }

        /// <summary>
        /// Adds or updates an item in cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="obj">The object.</param>
        public static void AddOrUpdate( string key, string region, object obj )
        {
            AddOrUpdate( key, region, obj, TimeSpan.MaxValue );
        }

        /// <summary>
        /// Adds or updates an item in cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="obj">The object.</param>
        /// <param name="expiration">The expiration.</param>
        public static void AddOrUpdate( string key, string region, object obj, DateTime expiration )
        {
            var timespan = expiration.Subtract( RockDateTime.Now );
            AddOrUpdate( key, region, obj, timespan );
        }

        /// <summary>
        /// Adds or updates an item in cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="obj">The object.</param>
        /// <param name="expiration">The expiration.</param>
        public static void AddOrUpdate( string key, string region, object obj, TimeSpan expiration )
        {
            if ( region.IsNotNullOrWhitespace() )
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, region, obj, expiration );
            }
            else
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, obj, expiration );
            }
        }

        /// <summary>
        /// Creates a cache object from Json string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static object FromJson( string json )
        {
            return JsonConvert.DeserializeObject( json );
        }

        /// <summary>
        /// Removes the specified key from cache.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Remove( string key )
        {
            Remove( key, null );
        }

        /// <summary>
        /// Removes the specified key from cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        public static void Remove( string key, string region )
        {
            if ( region.IsNotNullOrWhitespace() )
            {
                RockCacheManager<object>.Instance.Cache.Remove( key, region );
            }
            else
            {
                RockCacheManager<object>.Instance.Cache.Remove( key );
            }
        }

        /// <summary>
        /// Clears all cached items (MemoryCache, Authorizations, EntityAttributes, CacheSite, TriggerCache).
        /// </summary>
        /// <returns></returns>
        public static List<string> ClearAllCachedItems()
        {
            // TODO: Clear all items

            var msgs = new List<string>();

            // Clear all cached items
            ClearAll();
            msgs.Add( "RockCacheManager has been cleared" );

            //// Clear workflow trigger cache
            CacheWorkflowTriggers.Refresh();
            //msgs.Add( "TriggerCache has been cleared" );

            return msgs;
        }

        /// <summary>
        /// Gets all statistics fore each cache instance/handle
        /// </summary>
        /// <returns></returns>
        public static List<CacheItemStatistics> GetAllStatistics()
        {
            var cacheStats = new List<CacheItemStatistics>();

            if ( _allManagers == null ) return cacheStats;

            foreach ( var cacheManager in _allManagers )
            {
                if ( cacheManager != null )
                {
                    cacheStats.Add( cacheManager.GetStatistics() );
                }
            }

            return cacheStats;
        }

        #endregion
    }
}

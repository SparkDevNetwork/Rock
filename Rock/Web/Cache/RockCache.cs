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
using System.Linq;

using Newtonsoft.Json;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Class for adding generic items to cache
    /// </summary>
    public class RockCache
    {
        private static readonly object Obj = new object();
        private static List<IRockCacheManager> _allManagers;
        private const string CACHE_TAG_REGION_NAME = "cachetags";

        #region Private Static Methods

        /// <summary>
        /// Adds a new cache manager to the collection of all managers.
        /// </summary>
        /// <param name="manager">The manager.</param>
        internal static void AddManager( IRockCacheManager manager )
        {
            if ( manager == null )
            {
                return;
            }

            lock ( Obj )
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
            if ( _allManagers == null )
            {
                return;
            }

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
            if ( httpContext == null || !httpContext.Items.Contains( "Cache_Hits" ) )
            {
                return;
            }

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
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject( this, Formatting.None, jsonSerializerSettings );
        }

        #endregion

        #region Public Static Properties

        private static bool? _isCacheSerialized = null;

        /// <summary>
        /// Gets an indicator of whether cache manager is configured in a way that items will be serialized (i.e. if using Redis)
        /// </summary>
        /// <value>
        /// Flag indicating if cache items are serialized
        /// </value>
        public static bool IsCacheSerialized
        {
            get
            {
                if ( _isCacheSerialized == null )
                {
                    if ( Rock.Web.SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENABLE_CACHE_CLUSTER )?.AsBoolean() == true )
                    {
                        _isCacheSerialized = true;
                    }
                    else
                    {
                        // not using Redis, so it is safe to cache non-serializable things (like CacheLavaTemplate)
                        _isCacheSerialized = false;
                    }
                }

                return _isCacheSerialized.Value;
            }
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
            var value = region.IsNotNullOrWhiteSpace() ?
                RockCacheManager<object>.Instance.Cache.Get( key, region ) :
                RockCacheManager<object>.Instance.Cache.Get( key );

            UpdateCacheHitMiss( key, value != null );

            if ( value != null )
            {
                return value;
            }

            if ( itemFactory == null )
            {
                return null;
            }

            value = itemFactory();
            if ( value == null )
            {
                return null;
            }

            if ( region.IsNotNullOrWhiteSpace() )
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
            AddOrUpdate( key, region, obj, timespan, string.Empty );
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
            AddOrUpdate( key, region, obj, expiration, string.Empty );
        }

        /// <summary>
        /// Adds or updates an item in cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="obj">The object.</param>
        /// <param name="expiration">The expiration.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public static void AddOrUpdate( string key, string region, object obj, DateTime expiration, string cacheTags )
        {
            var timespan = expiration.Subtract( RockDateTime.Now );
            AddOrUpdate( key, region, obj, timespan, cacheTags );
        }

        /// <summary>
        /// Adds or updates an item in cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="obj">The object.</param>
        /// <param name="expiration">The expiration.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public static void AddOrUpdate( string key, string region, object obj, TimeSpan expiration, string cacheTags )
        {
            if ( region.IsNotNullOrWhiteSpace() )
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, region, obj, expiration );
            }
            else
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, obj, expiration );
            }

            if ( cacheTags.IsNotNullOrWhiteSpace() )
            {
                // trim the results since the tag name could come from lava and not from a prevalidated value stored in DefinedValue.
                var cacheTagList = cacheTags.Split( ',' ).Select( t => t.Trim() );
                foreach ( var cacheTag in cacheTagList )
                {
                    // Don't save the tag if it is not valid.
                    int cacheTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
                    Rock.Model.DefinedValueService definedValueService = new Rock.Model.DefinedValueService( new Rock.Data.RockContext() );
                    var validCacheTags = definedValueService.Queryable().Where( v => v.DefinedTypeId == cacheTagDefinedTypeId && v.Value == cacheTag ).ToList();
                    if ( validCacheTags.Count == 0 )
                    {
                        return;
                    }

                    var value = RockCacheManager<List<string>>.Instance.Cache.Get( cacheTag, CACHE_TAG_REGION_NAME ) ?? new List<string>();
                    if ( !value.Contains(key) )
                    {
                        value.Add( key );
                        RockCacheManager<List<string>>.Instance.AddOrUpdate( cacheTag, CACHE_TAG_REGION_NAME, value );
                    }
                }
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
            if ( region.IsNotNullOrWhiteSpace() )
            {
                RockCacheManager<object>.Instance.Cache.Remove( key, region );
            }
            else
            {
                RockCacheManager<object>.Instance.Cache.Remove( key );
            }
        }

        /// <summary>
        /// Removes all items from cache for comma separated list of cache tags
        /// </summary>
        /// <param name="cacheTags">The cache tags.</param>
        public static void RemoveForTags( string cacheTags )
        {
            var cacheTagList = cacheTags.Split( ',' );
            foreach ( var cacheTag in cacheTagList )
            {
                var cachedItemKeys = RockCacheManager<List<string>>.Instance.Cache.Get( cacheTag, CACHE_TAG_REGION_NAME ) ?? new List<string>();
                foreach ( var key in cachedItemKeys )
                {
                    Remove( key );
                }
            }
        }

        /// <summary>
        /// Clears all cached items (MemoryCache, Authorizations, EntityAttributes, SiteCache, TriggerCache).
        /// </summary>
        /// <param name="refreshWorkflowTriggers">if set to <c>true</c> [refresh workflow triggers].</param>
        /// <returns></returns>
        public static List<string> ClearAllCachedItems( bool refreshWorkflowTriggers )
        {
            var msgs = new List<string>();

            // Clear all cached items
            ClearAll();
            msgs.Add( "RockCacheManager has been cleared" );

            if ( refreshWorkflowTriggers )
            {
                // Clear workflow trigger cache
                WorkflowTriggersCache.Refresh();
                ////msgs.Add( "TriggerCache has been cleared" );
            }

            return msgs;
        }

        /// <summary>
        /// Clears all cached items (MemoryCache, Authorizations, EntityAttributes, SiteCache, TriggerCache)
        /// and refreshes the workflow trigger cache.
        /// </summary>
        /// <returns></returns>
        public static List<string> ClearAllCachedItems()
        {
            return ClearAllCachedItems( true );
        }

        /// <summary>
        /// Clears all of the cached items for the type name.
        /// </summary>
        /// <param name="cacheTypeName">Name of the cache type.</param>
        /// <returns></returns>
        public static string ClearCachedItemsForType( string cacheTypeName )
        {
            if ( _allManagers == null )
            {
                return "Nothing to clear";
            }

            if ( cacheTypeName.Contains( "Cache" ) )
            {
                return ClearCachedItemsForType( Type.GetType( $"Rock.Web.Cache.{cacheTypeName},Rock" ) );
            }

            return ClearCachedItemsForSystemType( cacheTypeName );
        }

        /// <summary>
        /// Clears all of the cached items for the type.
        /// </summary>
        /// <param name="cacheType">Type of the cache.</param>
        /// <returns></returns>
        public static string ClearCachedItemsForType( Type cacheType )
        {
            Type rockCacheManagerType = typeof( RockCacheManager<> ).MakeGenericType( new Type[] { cacheType } );

            foreach ( var manager in _allManagers )
            {
                if ( manager.GetType() == rockCacheManagerType )
                {
                    manager.Clear();
                    return $"Cache for {cacheType.Name} cleared.";
                }
            }

            return $"Nothing to clear for {cacheType.Name}.";
        }

        /// <summary>
        /// Clears all cached items for a non-rock cache types string, int, and object
        /// </summary>
        /// <param name="cacheTypeName">Name of the cache type.</param>
        /// <returns></returns>
        public static string ClearCachedItemsForSystemType( string cacheTypeName )
        {
            switch ( cacheTypeName )
            {
                case "System.String":
                    RockCacheManager<List<string>>.Instance.Clear();
                    return $"Cache for {cacheTypeName} cleared.";

                case "System.Int32":
                    RockCacheManager<int?>.Instance.Clear();
                    return $"Cache for {cacheTypeName} cleared.";

                case "Object":
                    RockCacheManager<object>.Instance.Clear();
                    return $"Cache for {cacheTypeName} cleared.";

                default:
                    return $"Nothing to clear for {cacheTypeName}.";
            }
        }

        /// <summary>
        /// Gets the count of cached items for tag.
        /// </summary>
        /// <param name="cacheTag">The cache tag.</param>
        /// <returns></returns>
        public static long GetCountOfCachedItemsForTag( string cacheTag )
        {
            var cachedItemKeys = RockCacheManager<List<string>>.Instance.Cache.Get( cacheTag, CACHE_TAG_REGION_NAME ) ?? new List<string>();
            return cachedItemKeys.Count();
        }

        /// <summary>
        /// Gets all statistics fore each cache instance/handle
        /// </summary>
        /// <returns></returns>
        public static List<CacheItemStatistics> GetAllStatistics()
        {
            var cacheStats = new List<CacheItemStatistics>();

            if ( _allManagers == null )
            {
                return cacheStats;
            }

            foreach ( var cacheManager in _allManagers )
            {
                if ( cacheManager != null )
                {
                    cacheStats.Add( cacheManager.GetStatistics() );
                }
            }

            return cacheStats;
        }

        /// <summary>
        /// Gets the statistics for the given type.
        /// </summary>
        /// <param name="cacheType">Type of the cache.</param>
        /// <returns></returns>
        public static CacheItemStatistics GetStatisticsForType( Type cacheType )
        {
            var cacheStats = new CacheItemStatistics( string.Empty );
            if ( _allManagers == null )
            {
                return cacheStats;
            }

            Type rockCacheManagerType = typeof( RockCacheManager<> ).MakeGenericType( new Type[] { cacheType } );

            foreach ( var manager in _allManagers )
            {
                if ( manager.GetType() == rockCacheManagerType )
                {
                    cacheStats = manager.GetStatistics();
                    break;
                }
            }

            return cacheStats;
        }

        /// <summary>
        /// Gets the cache statistics for a cache type
        /// </summary>
        /// <param name="cacheTypeName">Name of the cache type.</param>
        /// <returns></returns>
        public static CacheItemStatistics GetStatForSystemType( string cacheTypeName )
        {
            var cacheStats = new CacheItemStatistics( string.Empty );
            if ( _allManagers == null )
            {
                return cacheStats;
            }

            if ( cacheTypeName == "System.String" )
            {
                return RockCacheManager<List<string>>.Instance.GetStatistics();
            }
            else if ( cacheTypeName == "System.Int32" )
            {
                return RockCacheManager<int?>.Instance.GetStatistics();
            }
            else if ( cacheTypeName == "Object" )
            {
                return RockCacheManager<object>.Instance.GetStatistics();
            }

            return cacheStats;
        }

        /// <summary>
        /// Gets the statistics for the given type name.
        /// </summary>
        /// <param name="cacheTypeName">Name of the cache type.</param>
        /// <returns></returns>
        public static CacheItemStatistics GetStatisticsForType( string cacheTypeName )
        {
            if ( cacheTypeName.Contains( "Cache" ) )
            {
                return GetStatisticsForType( Type.GetType( $"Rock.Web.Cache.{cacheTypeName},Rock" ) );
            }

            return GetStatForSystemType( cacheTypeName );
        }

        /// <summary>
        /// Determines whether the end point is available.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        ///   <c>true</c> if [is end point available] [the specified socket]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEndPointAvailable( string socket, string password )
        {
            try
            {
                var configurationOptions = StackExchange.Redis.ConfigurationOptions.Parse( socket );
                configurationOptions.ConnectRetry = 1;
                configurationOptions.ConnectTimeout = 500;

                if ( password.IsNotNullOrWhiteSpace() )
                {
                    configurationOptions.Password = password;
                }
                
                var redisConnection = StackExchange.Redis.ConnectionMultiplexer.Connect( configurationOptions );
                return redisConnection.IsConnected;
            }
            catch(Exception)
            {
                return false;
            }
        }
        
        #endregion
    }
}

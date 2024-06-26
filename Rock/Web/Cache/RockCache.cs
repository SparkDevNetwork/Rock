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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Utility.ExtensionMethods;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Class for adding generic items to cache
    /// </summary>
    public class RockCache
    {
        /// <summary>
        /// The cache control cookie
        /// </summary>
        public const string CACHE_CONTROL_COOKIE = ".rock-web-cache-enabled";

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

            IRockCacheManager[] cacheManagerArray;
            lock ( Obj )
            {
                cacheManagerArray = _allManagers.ToArray();
            }

            foreach ( var cacheManager in cacheManagerArray )
            {
                cacheManager?.Clear();
            }

            // Clear object cache keys
            _objectConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();
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

        /// <summary>
        /// Gets an indicator of whether cache manager is configured in a way that items will be serialized.
        /// This will always return false since we no longer support cache managers that require serialization.
        /// </summary>
        /// <value>
        /// Flag indicating if cache items are serialized
        /// </value>
        [Obsolete( "Rock Cache doesn't have a requirement to be serializable anymore. However, we should try to keep it serializable." )]
        [RockObsolete( "1.15" )]
        public static bool IsCacheSerialized => false;

        /// <summary>
        /// Gets or sets the keys for items stored in the object cache. The region is optional, but the key
        /// is required. This list of keys is not guaranteed to be up to date. Some of the items represented
        /// by the keys could have expired and therefore not be available any longer. All item keys though should
        /// be in the list.
        /// </summary>
        /// <value>
        /// The object cache key references.
        /// </value>
        [Obsolete( "Use thread safe ObjectConcurrentCacheKeyReferences instead." )]
        [RockObsolete( "1.14" )]
        public static List<CacheKeyReference> ObjectCacheKeyReferences
        {
            get
            {
                if ( _objectCacheKeyReferences == null )
                {
                    _objectCacheKeyReferences = new List<CacheKeyReference>();
                }

                return _objectCacheKeyReferences;
            }
            set
            {
                _objectCacheKeyReferences = value;
            }
        }
        private static List<CacheKeyReference> _objectCacheKeyReferences = new List<CacheKeyReference>();

        /// <summary>
        /// Gets or sets the keys for items stored in the string cache. The region is optional, but the key
        /// is required. This list of keys is not guaranteed to be up to date. Some of the items represented
        /// by the keys could have expired and therefore not be available any longer. All item keys though should
        /// be in the list.
        /// </summary>
        /// <value>
        /// The string cache key references.
        /// </value>
        [Obsolete( "Use thread safe StringConcurrentCacheKeyReferences instead." )]
        [RockObsolete( "1.14" )]
        public static List<CacheKeyReference> StringCacheKeyReferences
        {
            get
            {
                if ( _stringCacheKeyReferences == null )
                {
                    _stringCacheKeyReferences = new List<CacheKeyReference>();
                }
                return _stringCacheKeyReferences;
            }
            set
            {
                _stringCacheKeyReferences = value;
            }
        }
        private static List<CacheKeyReference> _stringCacheKeyReferences = new List<CacheKeyReference>();

        /// <summary>
        /// Gets or sets the keys for items stored in the object cache. The region is optional, but the key
        /// is required. This list of keys is not guaranteed to be up to date. Some of the items represented
        /// by the keys could have expired and therefore not be available any longer. All item keys though should
        /// be in the list.
        /// </summary>
        /// <value>
        /// The object cache key references.
        /// </value>
        [RockInternal( "1.14" )]
        public static ConcurrentDictionary<string, CacheKeyReference> ObjectConcurrentCacheKeyReferences
        {
            get
            {
                if ( _objectConcurrentCacheKeyReferences == null )
                {
                    _objectConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();
                }

                return _objectConcurrentCacheKeyReferences;
            }
        }

        private static ConcurrentDictionary<string, CacheKeyReference> _objectConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();

        /// <summary>
        /// Gets or sets the keys for items stored in the string cache. The region is optional, but the key
        /// is required. This list of keys is not guaranteed to be up to date. Some of the items represented
        /// by the keys could have expired and therefore not be available any longer. All item keys though should
        /// be in the list.
        /// </summary>
        /// <value>
        /// The string cache key references.
        /// </value>
        [RockInternal( "1.14" )]
        public static ConcurrentDictionary<string, CacheKeyReference> StringConcurrentCacheKeyReferences
        {
            get
            {
                if ( _stringConcurrentCacheKeyReferences == null )
                {
                    _stringConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();
                }

                return _stringConcurrentCacheKeyReferences;
            }
        }

        private static ConcurrentDictionary<string, CacheKeyReference> _stringConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();

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
        /// Gets an item from cache using the specified key. If allowCacheBypass is true the CACHE_CONTROL_COOKIE will be
        /// inspected to see if cached value should be ignored.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="allowCacheBypass">if set to <c>true</c> the cache can be ignored based on the cache control cookie.</param>
        /// <returns></returns>
        public static object Get( string key, bool allowCacheBypass )
        {
            var args = new RockCacheGetOrAddExistingArgs
            {
                Key = key,
                Region = null,
                ItemFactory = null,
                Expiration = TimeSpan.MaxValue,
                AllowCacheBypass = allowCacheBypass
            };

            return GetOrAddExisting( args );
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
            var args = new RockCacheGetOrAddExistingArgs
            {
                Key = key,
                Region = region,
                ItemFactory = itemFactory,
                Expiration = expiration,
                AllowCacheBypass = false
            };

            return GetOrAddExisting( args );
        }

        /// <summary>
        /// Gets or adds an item from cache using the specified args. If allowCacheBypass is true the CACHE_CONTROL_COOKIE will be
        /// inspected to see if cached value should be ignored.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static object GetOrAddExisting( RockCacheGetOrAddExistingArgs args )
        {
            if ( args.AllowCacheBypass && System.Web.HttpContext.Current != null )
            {
                var isCachedEnabled = System.Web.HttpContext.Current.Request.Cookies.Get( CACHE_CONTROL_COOKIE );
                if ( isCachedEnabled != null && !isCachedEnabled.Value.AsBoolean() )
                {
                    return null;
                }
            }

            var value = args.Region.IsNotNullOrWhiteSpace() ?
                RockCacheManager<object>.Instance.Get( args.Key, args.Region ) :
                RockCacheManager<object>.Instance.Get( args.Key );

            if ( value != null )
            {
                return value;
            }

            if ( args.ItemFactory == null )
            {
                return null;
            }

            value = args.ItemFactory();
            if ( value == null )
            {
                return null;
            }

            if ( args.Region.IsNotNullOrWhiteSpace() )
            {
                RockCacheManager<object>.Instance.AddOrUpdate( args.Key, args.Region, value, args.Expiration );
            }
            else
            {
                RockCacheManager<object>.Instance.AddOrUpdate( args.Key, value, args.Expiration );
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
                AddOrUpdateObjectCacheKey( region, key );
            }
            else
            {
                RockCacheManager<object>.Instance.AddOrUpdate( key, obj, expiration );
                AddOrUpdateObjectCacheKey( region, key );
            }

            if ( cacheTags.IsNullOrWhiteSpace() )
            {
                return;
            }

            // trim the results since the tag name could come from lava and not from a prevalidated value stored in DefinedValue.
            var cacheTagList = cacheTags.Split( ',' ).Select( t => t.Trim() );
            if ( !cacheTagList.Any() )
            {
                return;
            }

            // Track the keys associated with defined cache tags, and ignore undefined tags.
            var cacheTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
            var definedValueService = new Rock.Model.DefinedValueService( new Rock.Data.RockContext() );
            var validCacheTags = definedValueService.Queryable().Where( v => v.DefinedTypeId == cacheTagDefinedTypeId && cacheTagList.Contains( v.Value ) ).ToList();

            foreach ( var cacheTag in validCacheTags )
            {
                var cacheTagKey = cacheTag.Value;
                var value = RockCacheManager<List<string>>.Instance.Get( cacheTagKey, CACHE_TAG_REGION_NAME ) ?? new List<string>();
                if ( !value.Contains( key ) )
                {
                    // Add the key to the list of keys associated with this tag.
                    value.Add( key );
                    RockCacheManager<List<string>>.Instance.AddOrUpdate( cacheTagKey, CACHE_TAG_REGION_NAME, value );
                    _stringConcurrentCacheKeyReferences.TryAdd( $"{cacheTag}{region}", new CacheKeyReference { Key = cacheTagKey, Region = region } );
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
                RockCacheManager<object>.Instance.Remove( key, region );
            }
            else
            {
                RockCacheManager<object>.Instance.Remove( key );
            }

            RemoveObjectCacheKey( region, key );
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
                // Get the collection of keys associated with this tag.
                var cachedItemKeys = RockCacheManager<List<string>>.Instance.Get( cacheTag, CACHE_TAG_REGION_NAME );
                if ( cachedItemKeys == null )
                {
                    continue;
                }

                // Remove the keys from the cache.
                foreach ( var key in cachedItemKeys )
                {
                    Remove( key );
                }

                // Reset the collection of keys associated with this tag.
                RockCacheManager<List<string>>.Instance.AddOrUpdate( cacheTag, CACHE_TAG_REGION_NAME, new List<string>() );
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
                return ClearCachedItemsForType( Type.GetType( cacheTypeName ) );
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

                    // Clear string cache keys
                    _stringConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();

                    return $"Cache for {cacheTypeName} cleared.";

                case "System.Int32":
                    RockCacheManager<int?>.Instance.Clear();
                    return $"Cache for {cacheTypeName} cleared.";

                case "Object":
                    RockCacheManager<object>.Instance.Clear();

                    // Clear object cache keys
                    _objectConcurrentCacheKeyReferences = new ConcurrentDictionary<string, CacheKeyReference>();

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
            var cachedItemKeys = RockCacheManager<List<string>>.Instance.Get( cacheTag, CACHE_TAG_REGION_NAME ) ?? new List<string>();
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
            var cacheStats = new CacheItemStatistics( string.Empty, string.Empty );
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
            var cacheStats = new CacheItemStatistics( string.Empty, string.Empty );
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
                return GetStatisticsForType( Type.GetType( cacheTypeName ) );
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
        [Obsolete( "No longer needed since we no longer support Redis." )]
        [RockObsolete( "1.15" )]
        public static bool IsEndPointAvailable( string socket, string password )
        {
            return false;
        }

        /// <summary>
        /// Gets all model cache types.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetAllModelCacheTypes()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetTypesSafe().Where( t =>
                t.BaseType != null &&
                t.BaseType.IsGenericType &&
                t.BaseType.GetGenericTypeDefinition() == typeof( ModelCache<,> ) ).ToList();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Adds the or updates object cache key.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="key">The key.</param>
        private static void AddOrUpdateObjectCacheKey( string region, string key )
        {
            var objectCacheReference = new CacheKeyReference { Region = region, Key = key };
            _objectConcurrentCacheKeyReferences.TryAdd( objectCacheReference.ToString(), objectCacheReference );
        }

        /// <summary>
        /// Removes the object cache key.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="key">The key.</param>
        private static void RemoveObjectCacheKey( string region, string key )
        {
            var objectCacheReference = new CacheKeyReference { Region = region, Key = key };
            _objectConcurrentCacheKeyReferences.TryRemove( objectCacheReference.ToString(), out _ );
        }
        #endregion

        #region POCO Classes
        /// <summary>
        /// Class for storing a full reference to a cached item - Region (optional) and Key (required)
        /// </summary>
        public class CacheKeyReference
        {
            /// <summary>
            /// Gets or sets the region.
            /// </summary>
            /// <value>
            /// The region.
            /// </value>
            public string Region { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; } = string.Empty;

            /// <summary>
            /// Concatenates key and region into a string without seperation.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return $"{Region}{Key}";
            }
        }

        /// <summary>
        /// Class for passing parameters to GetOrAddExisting methods.
        /// </summary>
        public class RockCacheGetOrAddExistingArgs
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the region.
            /// </summary>
            /// <value>
            /// The region.
            /// </value>
            public string Region { get; set; }

            /// <summary>
            /// Gets or sets the item factory.
            /// </summary>
            /// <value>
            /// The item factory.
            /// </value>
            public Func<object> ItemFactory { get; set; }

            /// <summary>
            /// Gets or sets the expiration.
            /// </summary>
            /// <value>
            /// The expiration.
            /// </value>
            public TimeSpan Expiration { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can bypass.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can bypass; otherwise, <c>false</c>.
            /// </value>
            public bool AllowCacheBypass { get; set; }
        }
        #endregion
    }
}

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
using System.Collections.Specialized;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;

using StackExchange.Redis;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Custom implementation of MemoryCache 
    /// https://github.com/ironyx/sharpmemorycache
    /// 
    /// </summary>
    [RockObsolete( "1.8" )]
    [Obsolete( "use RockCache instead" )]
    public class RockMemoryCache : MemoryCache
    {
        const string REDIS_CHANNEL_NAME = "rock-cache-instructions";

        // object used for locking
        private static object s_initLock;

        // singleton instance of RockMemoryCache
        private static RockMemoryCache s_defaultCache;

        // The function that sets the memory cache's last trim gen 2 count to a specific value
        private readonly Action<int> _setMemoryCacheLastTrimGen2CountFunc = null;

        // The timer that calls the last trim gen 2 count function
        private readonly Timer _setMemoryCacheLastTrimGen2CountTimer = null;

        // The integer incremented as the gen 2 count value
        private int _lastTrimGen2Count = 0;

        // Whether or not the polling interval has been set
        private bool _isPollingIntervalSet = false;

        // Whether caching is being disabled or not
        private bool _isCachingDisabled = false;

        /// <summary>
        /// Gets a value indicating whether Redis is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is redis connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsRedisConnected
        {
            get
            {
                return _isRedisConnected;
            }
        }
        private bool _isRedisConnected = false;

        /// <summary>
        /// Gets the redis connection.
        /// </summary>
        /// <value>
        /// The redis connection.
        /// </value>
        public ConnectionMultiplexer RedisConnection
        {
            get
            {
                return _redisConnection;
            }
        }
        private ConnectionMultiplexer _redisConnection;

        /// <summary>
        /// Gets a value indicating whether the redis cache cluster feature is enabled.
        /// </summary>
        /// <value>
        /// bool <c>true</c> if the redis cache cluster feature is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsRedisClusterEnabled
        {
            get
            {
                return _isRedisClusterEnabled;
            }
        }
        private bool _isRedisClusterEnabled = false;

        /// <summary>
        /// Initializes the <see cref="RockMemoryCache"/> class.
        /// </summary>
        static RockMemoryCache()
        {
            RockMemoryCache.s_initLock = new object();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockMemoryCache"/> class.
        /// </summary>
        private RockMemoryCache()
            : this( "RockDefault", null )
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockMemoryCache"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="config">The configuration.</param>
        private RockMemoryCache( string name, NameValueCollection config = null )
            : base( name, config )
        {
            // Use lambda expressions to create a set method for MemoryCache._stats._lastTrimGen2Count to circumvent poor functionality of MemoryCache
            // The default MemoryCache does not check for memory pressure except after a Gen 2 Garbage Collection. We want to do this more often than that.
            // So this method allows us to reset the field the MemoryCacheStatistics object uses periodically to a new value, to force the trim to be checked.

            // Define the types
            var memoryCacheType = typeof( MemoryCache );
            var memoryCacheStatisticsType = memoryCacheType.Assembly.GetType( "System.Runtime.Caching.MemoryCacheStatistics", true );

            // Define the _stats field on MemoryCache
            var statsField = memoryCacheType.GetField( "_stats", BindingFlags.Instance | BindingFlags.NonPublic );

            // Define the _lastTrimGen2Count field on MemoryCacheStatistics
            var lastTrimGen2CountField = memoryCacheStatisticsType.GetField( "_lastTrimGen2Count", BindingFlags.Instance | BindingFlags.NonPublic );

            // Get a reference to this memory cache instance
            var targetExpression = Expression.Constant( this, typeof( MemoryCache ) );

            // Define the parameters to the method
            var valueExpression = Expression.Parameter( typeof( int ), "value" );

            // Create the field expressions
            var statsFieldExpression = Expression.Field( targetExpression, statsField );
            var lastTrimGen2CountFieldExpression = Expression.Field( statsFieldExpression, lastTrimGen2CountField );

            // Create the field value assignment expression
            var fieldValueAssignmentExpression = Expression.Assign( lastTrimGen2CountFieldExpression, valueExpression );

            // Compile to function
            _setMemoryCacheLastTrimGen2CountFunc = Expression.Lambda<Action<int>>( fieldValueAssignmentExpression, valueExpression ).Compile();

            // Fire this method initially after a 1000 ms delay
            _setMemoryCacheLastTrimGen2CountTimer = new Timer( SetMemoryCacheLastTrimGen2Count, null, 1000, Timeout.Infinite );

            // Check to see if caching has been disabled
            _isCachingDisabled = ConfigurationManager.AppSettings["DisableCaching"].AsBoolean();

            // setup redis cache clustering if needed
            _isRedisClusterEnabled = ConfigurationManager.AppSettings["EnableRedisCacheCluster"].AsBoolean();

            if ( _isRedisClusterEnabled && _isCachingDisabled == false )
            {
                string connectionString = ConfigurationManager.AppSettings["RedisConnectionString"];

                if ( !string.IsNullOrWhiteSpace( connectionString ) )
                {
                    try
                    {
                        _redisConnection = ConnectionMultiplexer.Connect( connectionString );
                        _redisConnection.PreserveAsyncOrder = false; // enable concurrent processing of pub/sub messages https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/PubSubOrder.md
                        _redisConnection.ConnectionRestored += _redisConnection_ConnectionRestored;
                        _redisConnection.ConnectionFailed += _redisConnection_ConnectionFailed;

                        _isRedisConnected = true;
                    }
                    catch ( Exception ex )
                    {
                        Model.ExceptionLogService.LogException( ex, null );
                    }

                    if ( _redisConnection != null )
                    {
                        // setup the subscription to listen for published instructions on caching
                        ISubscriber sub = _redisConnection.GetSubscriber();

                        sub.Subscribe( REDIS_CHANNEL_NAME, ( channel, message ) => {
                            ProcessRedisCacheInstruction( channel, message );
                        } );
                    }
                    else
                    {
                        _isRedisClusterEnabled = false;
                    }
                }
                else
                {
                    _isRedisClusterEnabled = false;
                }
            }
        }

        private void _redisConnection_ConnectionFailed( object sender, ConnectionFailedEventArgs e )
        {
            if ( e.ConnectionType == ConnectionType.Subscription )
            {
                _isRedisConnected = false;
            }
        }

        private void _redisConnection_ConnectionRestored( object sender, ConnectionFailedEventArgs e )
        {
            // flush the cache when the connection is restored
            if ( e.ConnectionType == ConnectionType.Subscription && _isRedisConnected == false )
            {
                FlushMemoryCache();
                _isRedisConnected = true;
            }
        }

        private void ProcessRedisCacheInstruction(RedisChannel channel, RedisValue message )
        {
            string[] messageParts = message.ToString().Split( ',' );
            if (messageParts.Length > 0 )
            {
                var action = messageParts[0];
                switch ( action )
                {
                    case "REMOVE":
                        {
                            if (messageParts.Length > 1 )
                            {
                                RemoveFromMemoryCache( messageParts[1] );
                            }
                            break;
                        }
                    case "FLUSH":
                        {
                            FlushMemoryCache();
                            break;
                        }
                    case "REFRESH_AUTH_ENTITY":
                        {
                            if ( messageParts.Length > 2 )
                            {
                                Rock.Security.Authorization.RefreshEntity( messageParts[1].AsInteger(), messageParts[2].AsInteger() );
                            }
                            break;
                        }
                    case "REFRESH_AUTH_ACTION":
                        {
                            if ( messageParts.Length > 3 )
                            {
                                Rock.Security.Authorization.RefreshAction( messageParts[1].AsInteger(), messageParts[2].AsInteger(), messageParts[3] );
                            }
                            break;
                        }
                    case "FLUSH_AUTH":
                        {
                            //Rock.Security.Authorization.FlushAuth();
                            break;
                        }
                    case "REMOVE_ENTITY_ATTRIBUTES":
                        {
                            Rock.Web.Cache.AttributeCache.RemoveEntityAttributes();
                            break;
                        }
                    case "PING":
                        {
                            var response = string.Format( "PONG: From {0}: {1}", System.Environment.MachineName, System.AppDomain.CurrentDomain.FriendlyName );
                            SendRedisCommand( response );
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Sets the MemoryCache._stats._lastTrimGen2Count field to a value.
        /// </summary>
        /// <param name="state">The state. Ignored.</param>
        private void SetMemoryCacheLastTrimGen2Count( object state )
        {
            // Set the value to force the trim to be executed instead of waiting for then next Gen 2 Garbage Collection
            _setMemoryCacheLastTrimGen2CountFunc( _lastTrimGen2Count++ % 10 );

            // Check if we need to configure the timer's interval
            if ( !_isPollingIntervalSet )
            {
                // Get the polling interval in seconds and divide it in half
                var halfPollingIntervalMilliseconds = (int)PollingInterval.TotalMilliseconds / 2;

                // Configure the timer to fire at half of the polling interval - this ensures the value is different when the MemoryCache code looks at it via polling
                _setMemoryCacheLastTrimGen2CountTimer.Change( halfPollingIntervalMilliseconds, halfPollingIntervalMilliseconds );

                // Mark set
                _isPollingIntervalSet = true;
            }
        }

        /// <summary>
        /// Updates the cache hit miss.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="hit">if set to <c>true</c> [hit].</param>
        private void UpdateCacheHitMiss( string key, bool hit )
        {
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items.Contains( "Cache_Hits" ) )
            {
                var cacheHits = httpContext.Items["Cache_Hits"] as System.Collections.Generic.Dictionary<string, bool>;
                if ( cacheHits != null )
                {
                    cacheHits.AddOrIgnore( key, hit );
                }
            }
        }


        /// <summary>
        /// Gets or sets a value in the cache by using the default indexer property for an instance of the <see cref="T:System.Runtime.Caching.MemoryCache" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override object this[string key]
        {
            get
            {
                if ( _isCachingDisabled )
                {
                    return null;
                }

                object obj = base[key];
                UpdateCacheHitMiss( key, obj != null );
                return obj;
            }
            set
            {
                base[key] = value;
            }
        }

        /// <summary>
        /// Inserts a cache entry into the cache using the specified key and value and the specified details for how it is to be evicted.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add or get.</param>
        /// <param name="value">The data for the cache entry.</param>
        /// <param name="policy">An object that contains eviction details for the cache entry. This object provides more options for eviction than a simple absolute expiration.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry can be added. Do not pass a value for this parameter. By default, this parameter is null, because the <see cref="T:System.Runtime.Caching.MemoryCache" /> class does not implement regions.</param>
        /// <returns>
        /// If a matching cache entry already exists, a cache entry; otherwise, null.
        /// </returns>
        public override object AddOrGetExisting( string key, object value, CacheItemPolicy policy, string regionName = null )
        {
            if ( _isCachingDisabled )
            {
                return null;
            }

            UpdateCacheHitMiss( key, Contains( key ) );
            return base.AddOrGetExisting( key, value, policy, regionName );
        }

        /// <summary>
        /// Adds a cache entry into the cache using the specified key and a value and an absolute expiration value.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="value">The data for the cache entry.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry can be added. Do not pass a value for this parameter. This parameter is null by default, because the <see cref="T:System.Runtime.Caching.MemoryCache" /> class does not implement regions.</param>
        /// <returns>
        /// If a cache entry with the same key exists, the existing cache entry; otherwise, null.
        /// </returns>
        public override object AddOrGetExisting( string key, object value, DateTimeOffset absoluteExpiration, string regionName = null )
        {
            if ( _isCachingDisabled )
            {
                return null;
            }

            UpdateCacheHitMiss( key, Contains( key ) );
            return base.AddOrGetExisting( key, value, absoluteExpiration, regionName );
        }

        /// <summary>
        /// Returns an entry from the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry was added. Do not pass a value for this parameter. This parameter is null by default, because the <see cref="T:System.Runtime.Caching.MemoryCache" /> class does not implement regions.</param>
        /// <returns>
        /// A reference to the cache entry that is identified by <paramref name="key" />, if the entry exists; otherwise, null.
        /// </returns>
        public override object Get( string key, string regionName = null )
        {
            if ( _isCachingDisabled )
            {
                return null;
            }

            object obj = base.Get( key, regionName );
            UpdateCacheHitMiss( key, obj != null );
            return obj;
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <returns></returns>
        public new object Remove( string key, string regionName = null )
        {
            if ( _isRedisClusterEnabled && _isRedisConnected  )
            {
                // tell all servers listening to redis to remove the cached item (including us)
                SendRedisCommand( "REMOVE," + key );
                return new object();
            }
            else
            {
                return RemoveFromMemoryCache( key, regionName );
            }
        }

        /// <summary>
        /// Sends the redis command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void SendRedisCommand(string command )
        {
            if ( _isRedisConnected )
            {
                ISubscriber sub = _redisConnection.GetSubscriber();
                sub.PublishAsync( REDIS_CHANNEL_NAME, command );
            }
        }

        /// <summary>
        /// Removes from memory cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <returns></returns>
        private object RemoveFromMemoryCache(string key, string regionName = null )
        {
            return base.Remove( key, regionName );
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public new void Dispose()
        {
            // unsubscribe from redis commands
            if ( _isRedisClusterEnabled )
            {
                ISubscriber sub = _redisConnection.GetSubscriber();
                sub.UnsubscribeAll();

                if ( _redisConnection != null )
                {
                    _redisConnection.Dispose();
                }
            }

            base.Dispose();
        }

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static new RockMemoryCache Default
        {
            get
            {
                if ( RockMemoryCache.s_defaultCache == null )
                {
                    lock ( RockMemoryCache.s_initLock )
                    {
                        if ( RockMemoryCache.s_defaultCache == null )
                        {
                            RockMemoryCache.s_defaultCache = new RockMemoryCache();
                        }
                    }
                }

                return RockMemoryCache.s_defaultCache;
            }
        }

        /// <summary>
        /// Clears all items from MemoryCache.
        /// </summary>
        public static void Clear()
        {
            if ( RockMemoryCache.s_defaultCache != null && RockMemoryCache.s_defaultCache.IsRedisClusterEnabled && RockMemoryCache.s_defaultCache.IsRedisConnected)
            {
                // tell all servers listening to redis to flush the cached (including us)
                RockMemoryCache.s_defaultCache.SendRedisCommand( "FLUSH" );               
            }
            else
            {
                FlushMemoryCache();
            }
        }

        /// <summary>
        /// Clears all cached items (MemoryCache, Authorizations, EntityAttributes, SiteCache, TriggerCache).
        /// </summary>
        /// <returns></returns>
        public static List<string> ClearAllCachedItems()
        {
            var msgs = new List<string>();

            // Clear the static object that contains all auth rules (so that it will be refreshed)
            Rock.Security.Authorization.Flush();
            msgs.Add( "Authorizations have been cleared" );

            // Flush the static entity attributes cache
            //Rock.Web.Cache.AttributeCache.FlushEntityAttributes();
            msgs.Add( "EntityAttributes have been cleared" );

            // Clear all cached items
            Clear();
            msgs.Add( "RockMemoryCache has been cleared" );

            // Flush Site Domains
            //Rock.Web.Cache.SiteCache.Flush();
            msgs.Add( "SiteCache has been cleared" );

            // Clear workflow trigger cache
            Rock.Workflow.TriggerCache.Refresh();
            msgs.Add( "TriggerCache has been cleared" );

            return msgs;
        }

        /// <summary>
        /// Flushes the memory cache.
        /// </summary>
        private static void FlushMemoryCache()
        {
            lock ( RockMemoryCache.s_initLock )
            {
                if ( RockMemoryCache.s_defaultCache != null )
                {
                    RockMemoryCache.s_defaultCache.Dispose();
                }

                RockMemoryCache.s_defaultCache = new RockMemoryCache();
            }
        }
    }
}

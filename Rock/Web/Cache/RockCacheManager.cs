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

using CacheManager.Core;
using CacheManager.Core.Internal;

using Rock.Bus;
using Rock.Bus.Message;
using Rock.Logging;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Singleton class for managing cache
    /// </summary>
    /// <remarks>
    /// Follows Singleton Pattern #2 Here: http://csharpindepth.com/Articles/General/Singleton.aspx 
    /// (Did not use #4 as we need to know when instance is instantiated for adding to collection)
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IRockCacheManager" />
    public sealed class RockCacheManager<T> : IRockCacheManager
    {
        private static RockCacheManager<T> instance;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object _obj = new object();

        private static BaseCacheManager<T> _cacheManager;

        static RockCacheManager()
        {
            RockCache.AddManager( Instance );
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RockCacheManager{T}"/> class from being created.
        /// </summary>
        private RockCacheManager()
        {
        }

        /// <summary>
        /// Gets the singleton instance of this class
        /// </summary>
        public static RockCacheManager<T> Instance
        {
            get
            {
                lock ( _obj )
                {
                    if ( instance == null )
                    {
                        instance = new RockCacheManager<T>();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        [RockObsolete( "1.12" )]
        [Obsolete( "Do not access the cache manager directly. Instead use the method available on this class." )]
        public BaseCacheManager<T> Cache => CacheManager;

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        private BaseCacheManager<T> CacheManager
        {
            get
            {
                if ( _cacheManager != null )
                {
                    return _cacheManager;
                }

                lock ( _obj )
                {
                    return _cacheManager ?? ( _cacheManager = new BaseCacheManager<T>( GetCacheConfig() ) );
                }
            }
        }

        /// <summary>
        /// Gets the cache configuration.
        /// </summary>
        /// <returns></returns>
        private static ICacheManagerConfiguration GetCacheConfig()
        {
            bool cacheStatisticsEnabled = Rock.Web.SystemSettings.GetValueFromWebConfig( SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS )?.AsBoolean() ?? false;


            var config = new ConfigurationBuilder( "InProcess" ).WithDictionaryHandle();

            if ( cacheStatisticsEnabled )
            {
                config = config.EnableStatistics().EnablePerformanceCounters();
            }
            else
            {
                config = config.DisablePerformanceCounters().DisableStatistics();
            }

            return config.Build();
        }

        /// <summary>
        /// Adds or updates an item in cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="updateValue">The update value.</param>
        internal void AddOrUpdate( string key, T updateValue )
        {
            AddOrUpdate( key, null, updateValue );
        }

        /// <summary>
        /// Adds or updates an item in cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="updateValue">The update value.</param>
        /// <param name="expiration">The expiration.</param>
        internal void AddOrUpdate( string key, T updateValue, TimeSpan expiration )
        {
            AddOrUpdate( key, null, updateValue, expiration );
        }

        /// <summary>
        /// Adds or updates an item in cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="updateValue">The update value.</param>
        internal void AddOrUpdate( string key, string region, T updateValue )
        {
            AddOrUpdate( key, region, updateValue, TimeSpan.MaxValue );
        }

        /// <summary>
        /// Adds or updates an item in cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="updateValue">The update value.</param>
        /// <param name="expiration">The expiration.</param>
        internal void AddOrUpdate( string key, string region, T updateValue, TimeSpan expiration )
        {
            // If an expiration timespan was specific, will need to use a CacheItem to add item to cache.
            if ( expiration != TimeSpan.MaxValue )
            {
                var cacheItem = region.IsNotNullOrWhiteSpace() ? CacheManager.GetCacheItem( key, region ) : CacheManager.GetCacheItem( key );
                if ( cacheItem != null )
                {
                    CacheManager.Put( cacheItem.WithAbsoluteExpiration( expiration ) );
                }
                else
                {
                    cacheItem = region.IsNotNullOrWhiteSpace() ?
                        new CacheItem<T>( key, region, updateValue, ExpirationMode.Absolute, expiration ) :
                        new CacheItem<T>( key, updateValue, ExpirationMode.Absolute, expiration );
                    CacheManager.Add( cacheItem );
                }
            }

            if ( region.IsNotNullOrWhiteSpace() )
            {
                CacheManager.AddOrUpdate( key, region, updateValue, v => updateValue );
                UpdateCacheReferences( key, region, updateValue );
            }
            else
            {
                CacheManager.AddOrUpdate( key, updateValue, v => updateValue );
                UpdateCacheReferences( key, region, updateValue );
            }
        }

        /// <summary>
        /// Gets a value for the specified key and will cast it to the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get( string key )
        {
            return CacheManager.Get( key );
        }

        /// <summary>
        /// Gets a value for the specified key and region and will cast it to the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public T Get( string key, string region )
        {
            return CacheManager.Get( key, region );
        }

        /// <summary>
        /// Clears the cache instance.
        /// </summary>
        public void Clear()
        {
            /* 07-29-2021 MDP
             We want to clear the local cache immediately for this Instance of Rock.
             Then send a CacheWasUpdatedMessage so that other Instances of Rock know that the cache was Updated.
             This instance of Rock will also receive this CacheWasUpdatedMessage, but we already took care of that in this instance.
             So, if we detect CacheWasUpdatedMessage is from this Instance, we can ignore it since we already took care of it.
             */

            CacheManager.Clear();
            CacheWasUpdatedMessage.Publish<T>();

            // This is somewhat temporary. In the future this should be updated
            // to use it's own domain.
            RockLogger.Log.WriteToLog( RockLogLevel.Debug, RockLogDomains.Other, $"Cache was cleared for {typeof(T).Name}. StackTrace: {Environment.StackTrace}" );
        }

        /// <summary>
        /// Receives the clear message from the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void ReceiveClearMessage( CacheWasUpdatedMessage message )
        {
            if ( RockMessageBus.IsFromSelf( message ) )
            {
                // We already took care of Clearing the cache for our instance, so
                // we can ignore this message.
                RockLogger.Log.Debug( RockLogDomains.Bus, $"Cache ClearMessage was from ourselves( {message.SenderNodeName} ). Skipping. {message.ToDebugString()}." );
                return;
            }

            CacheManager.Clear();
        }

        /// <summary>
        /// Removes a value from the cache for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove( string key )
        {
            /* 07-29-2021 MDP
             We want to remove this item from local cache immediately for this Instance of Rock.
             Then send a CacheWasUpdatedMessage so that other Instances of Rock know that the cache was Updated.
             This instance of Rock will also receive this CacheWasUpdatedMessage, but we already took care of that in this instance.
             So, if we detect CacheWasUpdatedMessage is from this Instance, we can ignore it since we already took care of it.
             */

            CacheManager.Remove( key );
            CacheWasUpdatedMessage.Publish<T>( key );
            return true;
        }

        /// <summary>
        /// Removes a value from the cache for the specified key and region.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public bool Remove( string key, string region )
        {
            /* 07-29-2021 MDP
             We want to remove this item from local cache immediately for this Instance of Rock.
             Then send a CacheWasUpdatedMessage so that other Instances of Rock know that the cache was Updated.
             This instance of Rock will also receive this CacheWasUpdatedMessage, but we already took care of that in this instance.
             So, if we detect CacheWasUpdatedMessage is from this Instance, we can ignore it since we already took care of it.
             */

            CacheManager.Remove( key, region );
            CacheWasUpdatedMessage.Publish<T>( key, region );
            return true;
        }

        /// <summary>
        /// Receives the remove message from the bus.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void ReceiveRemoveMessage( CacheWasUpdatedMessage message )
        {
            if ( RockMessageBus.IsFromSelf( message ) )
            {
                // We already took care of Clearing the cache for our instance, so
                // we can ignore this message.
                RockLogger.Log.Debug( RockLogDomains.Bus, $"Cache RemoveMessage was from ourselves( {message.SenderNodeName} ). Skipping. {message.ToDebugString()}." );
                return;
            }

            if ( message.Key.IsNullOrWhiteSpace() )
            {
                // A Key needs to be specified
                return;
            }

            if ( message.Region.IsNotNullOrWhiteSpace() )
            {
                CacheManager.Remove( message.Key, message.Region );
            }
            else
            {
                CacheManager.Remove( message.Key );
            }
        }

        /// <summary>
        /// Gets the statistics for the cache instance.
        /// </summary>
        /// <returns></returns>
        public CacheItemStatistics GetStatistics()
        {
            var type = typeof( T );

            string name = type.Name;
            string fullname = type.FullName;
            if ( type.IsGenericType && type.GenericTypeArguments[0] != null )
            {
                name = type.GenericTypeArguments[0].ToString();
                fullname = type.GenericTypeArguments[0].ToString();
            }

            var cacheStatistics = new CacheItemStatistics( name, fullname );

            foreach ( var handle in CacheManager.CacheHandles )
            {
                var handleStatistics = new CacheHandleStatistics( handle.Configuration.HandleType.Name );
                cacheStatistics.HandleStats.Add( handleStatistics );

                var stats = handle.Stats;
                foreach ( CacheStatsCounterType statType in Enum.GetValues( typeof( CacheStatsCounterType ) ) )
                {
                    handleStatistics.Stats.Add( new CacheStatistic( statType, stats.GetStatistic( statType ) ) );
                }
            }

            return cacheStatistics;
        }

        #region Private Methods
        /// <summary>
        /// Updates the cache references for lists of strings and objects. This allows us to retrieve all items from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <param name="item">Type of the item.</param>
        private void UpdateCacheReferences( string key, string region, T item )
        {
            var cacheReferenceItem = new RockCache.CacheKeyReference { Key = key, Region = region };

            if ( item is List<string> )
            {
                RockCache.StringConcurrentCacheKeyReferences.TryAdd( cacheReferenceItem.ToString(), cacheReferenceItem );
            }

            if ( item is List<object> )
            {
                RockCache.ObjectConcurrentCacheKeyReferences.TryAdd( cacheReferenceItem.ToString(), cacheReferenceItem );
            }
        }

        #endregion
    }

    /// <summary>
    /// Statistics for a cache item
    /// </summary>
    public class CacheItemStatistics
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the handle stats.
        /// </summary>
        /// <value>
        /// The handle stats.
        /// </value>
        public List<CacheHandleStatistics> HandleStats { get; set; } = new List<CacheHandleStatistics>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemStatistics"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CacheItemStatistics( string name )
        {
            Name = name;
            FullName = $"Rock.Web.Cache.{name},Rock";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemStatistics"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fullname">The full name.</param>
        public CacheItemStatistics( string name, string fullname )
        {
            Name = name;
            FullName = fullname;
        }
    }

    /// <summary>
    /// Statistics for a cache handle
    /// </summary>
    public class CacheHandleStatistics
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stats.
        /// </summary>
        /// <value>
        /// The stats.
        /// </value>
        public List<CacheStatistic> Stats { get; set; } = new List<CacheStatistic>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHandleStatistics"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CacheHandleStatistics( string name )
        {
            Name = name;
        }
    }

    /// <summary>
    /// Cache Statistic Count
    /// </summary>
    public class CacheStatistic
    {
        /// <summary>
        /// Gets or sets the type of the counter.
        /// </summary>
        /// <value>
        /// The type of the counter.
        /// </value>
        public CacheStatsCounterType CounterType { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public long Count { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheStatistic"/> class.
        /// </summary>
        /// <param name="counterType">Type of the counter.</param>
        /// <param name="count">The count.</param>
        public CacheStatistic( CacheStatsCounterType counterType, long count )
        {
            CounterType = counterType;
            Count = count;
        }
    }
}

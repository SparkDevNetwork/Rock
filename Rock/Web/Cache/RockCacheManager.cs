﻿// <copyright>
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
using Rock.Bus.Message;

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
        RockCacheManager()
        {
        }


        /// <summary>
        /// Gets the singletone instance of this class
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
            bool cacheStatisticsEnabled = Rock.Web.SystemSettings.GetValueFromWebConfig( SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS )?.AsBoolean()?? false;

            bool redisEnabled = Rock.Web.SystemSettings.GetValueFromWebConfig( SystemKey.SystemSetting.REDIS_ENABLE_CACHE_CLUSTER )?.AsBoolean()?? false;
            if ( redisEnabled == false )
            {
                var config = new ConfigurationBuilder( "InProcess" )
                .WithDictionaryHandle();
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

            string redisPassword = Web.SystemSettings.GetValueFromWebConfig( SystemKey.SystemSetting.REDIS_PASSWORD ) ?? string.Empty;
            string[] redisEndPointList = Web.SystemSettings.GetValueFromWebConfig( SystemKey.SystemSetting.REDIS_ENDPOINT_LIST )?.Split( ',' );
            int redisDbIndex = Web.SystemSettings.GetValueFromWebConfig( SystemKey.SystemSetting.REDIS_DATABASE_NUMBER )?.AsIntegerOrNull() ?? 0;

            var cacheConfig = new ConfigurationBuilder( "InProcess With Redis Backplane" )
                .WithJsonSerializer()
                .WithDictionaryHandle()
                .And
                .WithRedisConfiguration( "redis", redisConfig =>
                {
                    redisConfig.WithAllowAdmin().WithDatabase( redisDbIndex );

                    if( redisPassword.IsNotNullOrWhiteSpace() )
                    {
                        redisConfig.WithPassword( redisPassword );
                    }

                    foreach ( var redisEndPoint in redisEndPointList )
                    {
                        string[] info = redisEndPoint.Split( ':' );
                        if ( info.Length == 2 )
                        {
                            redisConfig.WithEndpoint( info[0], info[1].AsIntegerOrNull() ?? 6379 );
                        }
                        else
                        {
                            redisConfig.WithEndpoint( info[0], 6379 );
                        }
                    }
                } )
                .WithMaxRetries( 100 )
                .WithRetryTimeout( 10 )
                .WithRedisBackplane( "redis" )
                .WithRedisCacheHandle( "redis", true );

            if ( cacheStatisticsEnabled )
            {
                cacheConfig = cacheConfig.EnableStatistics().EnablePerformanceCounters();
            }
            else
            {
                cacheConfig = cacheConfig.DisablePerformanceCounters().DisableStatistics();
            }

            return cacheConfig.Build();
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
            return CacheManager.Get<T>( key );
        }

        /// <summary>
        /// Gets a value for the specified key and region and will cast it to the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public T Get( string key, string region )
        {
            return CacheManager.Get<T>( key, region );
        }

        /// <summary>
        /// Clears the cache instance.
        /// </summary>
        public void Clear()
        {
            CacheWasUpdatedMessage.Publish<T>();
        }

        /// <summary>
        /// Receives the clear message from the message bus.
        /// </summary>
        internal void ReceiveClearMessage()
        {
            CacheManager.Clear();
        }

        /// <summary>
        /// Removes a value from the cache for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove( string key )
        {
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
            CacheWasUpdatedMessage.Publish<T>( key, region );
            return true;
        }

        /// <summary>
        /// Receives the remove message from the bus.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal void ReceiveRemoveMessage( string key )
        {
            CacheManager.Remove( key );
        }

        /// <summary>
        /// Receives the remove message from the bus.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        internal void ReceiveRemoveMessage( string key, string region )
        {
            CacheManager.Remove( key, region );
        }

        /// <summary>
        /// Gets the statistics for the cache instance.
        /// </summary>
        /// <returns></returns>
        public CacheItemStatistics GetStatistics()
        {
            //type.IsGenericType && type.GenericTypeArguments[0] == typeof( Rock.Model.Person );
            var type = typeof( T );

            string name = type.Name;
            if( type.IsGenericType && type.GenericTypeArguments[0] != null )
            {
                name = type.GenericTypeArguments[0].ToString();
            }

            var cacheStatistics = new CacheItemStatistics( name );

            //var cacheStatistics = new CacheItemStatistics( typeof( T ).Name );

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
            if ( item is List<string> )
            {
                RockCache.StringCacheKeyReferences.Add( new RockCache.CacheKeyReference { Key = key, Region = region } );
            }

            if ( item is List<object> )
            {
                RockCache.ObjectCacheKeyReferences.Add( new RockCache.CacheKeyReference { Key = key, Region = region } );
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

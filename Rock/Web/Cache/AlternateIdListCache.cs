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

namespace Rock.Web.Cache
{
    /// <summary>
    /// Handles tracking an alternate list of identifiers beyond the standard
    /// "all items" cached list.
    /// </summary>
    /// <typeparam name="TCache">The type of cached object.</typeparam>
    /// <typeparam name="TListKey">The type of alternate identifier key.</typeparam>
    internal class AlternateIdListCache<TCache, TListKey>
    {
        /// <summary>
        /// The key prefix that will be used for the set of alternate
        /// identifier lists.
        /// </summary>
        private readonly string _keyPrefix;

        /// <summary>
        /// The lock object that will be used for synchronizing the
        /// modification of the key lists.
        /// </summary>
        private readonly object _keyListLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateIdListCache{TCache, TListKey}"/> class.
        /// </summary>
        /// <param name="keyPrefix">The key prefix for these alternate identifier lists.</param>
        public AlternateIdListCache( string keyPrefix )
        {
            _keyPrefix = keyPrefix;
        }

        /// <summary>
        /// Adds the key to the id list.
        /// </summary>
        /// <param name="key">The key to be added.</param>
        /// <param name="listKey">The key for the specific alternate identifier list.</param>
        /// <param name="keyFactory">The key factory to load all keys if it has not already been cached.</param>
        public void Add( string key, TListKey listKey, Func<TListKey, List<string>> keyFactory = null )
        {
            // Get the list of all item ids.
            var allKeys = RockCacheManager<AllIdList<TCache>>.Instance.Get( $"{_keyPrefix}:{listKey}" );

            if ( allKeys != null && allKeys.Keys.Contains( key ) )
            {
                // Already has it so no need to update the cache.
                return;
            }

            if ( allKeys == null )
            {
                // If the list doesn't exist then see if we can get it using the delegate
                if ( keyFactory != null )
                {
                    allKeys = new AllIdList<TCache>( keyFactory( listKey ) );

                    if ( allKeys != null )
                    {
                        RockCacheManager<AllIdList<TCache>>.Instance.AddOrUpdate( _keyPrefix, allKeys );
                    }
                }

                // At this point the method has all the data that is possible
                // to get if there are no current keys stored in the cache, so return.
                return;
            }

            // The key is not part of the list so add it and update the cache.
            lock ( _keyListLock )
            {
                // Add it.
                allKeys.Keys.Add( key, true );
                RockCacheManager<AllIdList<TCache>>.Instance.AddOrUpdate( _keyPrefix, allKeys );
            }
        }

        /// <summary>
        /// Adds the key to the id list.
        /// </summary>
        /// <param name="listKey">The key for the specific alternate identifier list.</param>
        /// <param name="keyFactory">The key factory to load all keys if it has not already been cached.</param>
        public IReadOnlyCollection<string> GetOrAddKeys( TListKey listKey, Func<TListKey, List<string>> keyFactory )
        {
            // Get the list of all item ids.
            var allKeys = RockCacheManager<AllIdList<TCache>>.Instance.Get( $"{_keyPrefix}:{listKey}" );

            if ( allKeys != null )
            {
                return allKeys.Keys;
            }

            var keys = keyFactory( listKey );

            if ( keys != null )
            {
                RockCacheManager<AllIdList<TCache>>.Instance.AddOrUpdate( $"{_keyPrefix}:{listKey}", new AllIdList<TCache>( keys ) );

                return keys;
            }

            return new List<string>();
        }

        /// <summary>
        /// Removes the specified key from id list. This should be called
        /// when the key is no longer valid in the database and will not
        /// return.
        /// </summary>
        /// <param name="key">The key to be removed.</param>
        /// <param name="listKey">The key for the specific alternate identifier list.</param>
        public void Remove( string key, TListKey listKey )
        {
            var allIds = RockCacheManager<AllIdList<TCache>>.Instance.Get( $"{_keyPrefix}:{listKey}" );

            if ( allIds == null || !allIds.Keys.Contains( key ) )
            {
                return;
            }

            lock ( _keyListLock )
            {
                allIds.Keys.Remove( key );
                RockCacheManager<AllIdList<TCache>>.Instance.AddOrUpdate( $"{_keyPrefix}:{listKey}", allIds );
            }
        }

        /// <summary>
        /// Clears all alternate list keys. This will also trigger a bus
        /// message so other nodes clear the related cache.
        /// </summary>
        public void Clear()
        {
            RockCacheManager<AllIdList<TCache>>.Instance.Clear();
        }

        /// <summary>
        /// Clears the list of keys from the cached list. This will also
        /// trigger a bus message so other nodes clear the related cache.
        /// </summary>
        /// <param name="listKey">The key for the specific alternate identifier list.</param>
        public void Clear( TListKey listKey )
        {
            RockCacheManager<AllIdList<TCache>>.Instance.Remove( $"{_keyPrefix}:{listKey}" );
        }

        /// <summary>
        /// This is a helper class to ensure that all our cached items are
        /// scoped to us. If we just cached a generic List&lt;string&gt;
        /// we would conflict with other cached items.
        /// </summary>
        /// <typeparam name="TItemCache">The type of the cached item.</typeparam>
        private class AllIdList<TItemCache>
        {
            public List<string> Keys { get; }

            public AllIdList()
            {
                Keys = new List<string>();
            }

            public AllIdList( List<string> keys )
            {
                Keys = keys;
            }
        }
    }
}

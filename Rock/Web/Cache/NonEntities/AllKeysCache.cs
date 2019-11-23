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
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Internal cache for storing the list of Keys that <see cref="EntityCache.All()" /> returns.
    /// </summary>
    [Serializable]
    [DataContract]
    internal class AllKeysCache : ItemCache<AllKeysCache>
    {
        /// <summary>
        /// The list of Keys
        /// </summary>
        [DataMember]
        private ConcurrentDictionary<string, byte> AllKeys { get; set; }

        /// <summary>
        /// Gets the AllKeysCache for the specified key
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <returns></returns>
        private static AllKeysCache Get( string allItemsKey )
        {
            return GetOrAddExisting( allItemsKey, () => null );
        }

        private AllKeysCache( List<string> keys )
        {
            AllKeys = new ConcurrentDictionary<string, byte>( keys.ToDictionary( k => k, b => ( byte ) 0 ) );
        }

        /// <summary>
        /// Gets the keys or add existing.
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        internal static List<string> GetAllKeysOrAddExisting( string allItemsKey, Func<List<string>> factory )
        {
            Func<AllKeysCache> allKeysCacheFactory = () =>
            {
                return new AllKeysCache( factory.Invoke() );
            };

            return GetOrAddExisting( allItemsKey, allKeysCacheFactory )?.GetAllKeys();
        }

        /// <summary>
        /// Adds the keys.
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <param name="factory">The factory.</param>
        internal static List<string> AddAllKeys( string allItemsKey, Func<List<string>> factory )
        {
            var allKeysCache = new AllKeysCache( factory.Invoke() );
            UpdateCacheItem( allItemsKey, allKeysCache, TimeSpan.MaxValue );

            return allKeysCache.GetAllKeys();
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <returns></returns>
        internal static List<string> GetAllKeys( string allItemsKey )
        {
            return Get( allItemsKey )?.GetAllKeys();
        }

        /// <summary>
        /// Gets all keys.
        /// </summary>
        /// <returns></returns>
        internal List<string> GetAllKeys()
        {
            return AllKeys?.Select( a => a.Key ).ToList();
        }

        /// <summary>
        /// Returns true of the AllKeys cache has been populated for the specified allItemsKey
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <returns></returns>
        internal static bool Exists( string allItemsKey )
        {
            return RockCacheManager<AllKeysCache>.Instance.Cache.Exists( allItemsKey );
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        internal bool ContainsKey( string itemKey )
        {
            return AllKeys?.ContainsKey( itemKey ) == true;
        }

        /// <summary>
        /// Removes the key.
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <param name="itemKey">The item key.</param>
        internal static void RemoveKey( string allItemsKey, string itemKey )
        {
            bool allKeysPopulated = AllKeysCache.Exists( allItemsKey );

            if ( !allKeysPopulated )
            {
                // All hasn't been asked for yet, so it doesn't need to be maintained. Leave it null
                return;
            }

            var allKeysCache = AllKeysCache.Get( allItemsKey );
            if ( allKeysCache == null )
            {
                // All hasn't been asked for yet (or was set to null), so it doesn't need to be maintained. Leave it null
            }

            if ( !allKeysCache.ContainsKey( itemKey ) )
            {
                // doesn't contain it, so no need to remove it
                return;
            }

            allKeysCache.AllKeys.TryRemove( itemKey, out _ );
            AllKeysCache.UpdateCacheItem( allItemsKey, allKeysCache, TimeSpan.MaxValue );
        }

        /// <summary>
        /// Removes the key.
        /// </summary>
        /// <param name="allItemsKey">All items key.</param>
        /// <param name="itemKey">The item key.</param>
        internal static void AddKey( string allItemsKey, string itemKey )
        {
            bool allKeysPopulated = AllKeysCache.Exists( allItemsKey );

            if ( !allKeysPopulated )
            {
                // All hasn't been asked for yet, so it doesn't need to be maintained. Leave it null
                return;
            }

            var allKeysCache = AllKeysCache.Get( allItemsKey );
            if ( allKeysCache == null )
            {
                // All hasn't been asked for yet (or was set to null), so it doesn't need to be maintained. Leave it null
            }

            if ( allKeysCache.ContainsKey( itemKey ) )
            {
                // already contains it, so no need to add it
                return;
            }

            allKeysCache.AllKeys.TryAdd( itemKey, 0 );
            AllKeysCache.UpdateCacheItem( allItemsKey, allKeysCache, TimeSpan.MaxValue );
        }
    }
}
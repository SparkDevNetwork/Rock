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
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Generic Item Cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public abstract class ItemCache<T> : IItemCache
        where T : IItemCache
    {
        private const string _AllRegion = "AllItems";

        // This static field will be different for each generic type. See (https://www.jetbrains.com/help/resharper/2018.1/StaticMemberInGenericType.html)
        // This is intentional behavior in this case.
        private static readonly object _obj = new object();

        private static readonly string KeyPrefix = $"{typeof( T ).Name}";
        private static string AllKey => $"{typeof( T ).Name}:All";

        #region Protected Methods

        /// <summary>
        /// Returns the key prefixed with the type of object being cached.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal protected static string QualifiedKey( string key )
        {
            return $"{KeyPrefix}:{key}";
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <returns></returns>
        internal protected static T GetOrAddExisting( int key, Func<T> itemFactory )
        {
            return GetOrAddExisting( key.ToString(), itemFactory );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache with an expiration timespan.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        internal protected static T GetOrAddExisting( int key, Func<T> itemFactory, TimeSpan expiration )
        {
            return GetOrAddExisting( key.ToString(), itemFactory, expiration );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <returns></returns>
        internal protected static T GetOrAddExisting( string key, Func<T> itemFactory )
        {
            return GetOrAddExisting( key, itemFactory, TimeSpan.MaxValue );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache with an expiration timespan.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        internal protected static T GetOrAddExisting( string key, Func<T> itemFactory, TimeSpan expiration )
        {
            string qualifiedKey = QualifiedKey( key );

            var value = RockCacheManager<T>.Instance.Cache.Get( qualifiedKey );

            RockCache.UpdateCacheHitMiss( key, value != null );

            if ( value != null )
            {
                return value;
            }

            if ( itemFactory == null ) return default( T );

            value = itemFactory();
            if ( value != null )
            {
                UpdateCacheItem( key, value, expiration );
            }

            return value;
        }

        /// <summary>
        /// Updates the cache item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="expiration">The expiration.</param>
        internal protected static void UpdateCacheItem( string key, T item, TimeSpan expiration )
        {
            string qualifiedKey = QualifiedKey( key );

            // Add the item to cache
            RockCacheManager<T>.Instance.AddOrUpdate( qualifiedKey, item, expiration );

            // Do any postcache processing that this item cache type may need to do
            item.PostCached();

            AddToAllIds( key );
        }

        /// <summary>
        /// Ensure that the Key is part of the AllIds list
        /// </summary>
        /// <param name="key">The key.</param>
        private static void AddToAllIds( string key )
        {
            // Get the dictionary of all item ids
            var allKeys = RockCacheManager<List<string>>.Instance.Cache.Get( AllKey, _AllRegion );
            if ( allKeys == null )
            {
                // All hasn't been asked for yet, so it doesn't need to be updated. Leave it null
                return;
            }

            if ( allKeys.Contains( key ) )
            {
                // already has it so no need to update the cache
                return;
            }

            // If the key is not part of the dictionary all ready
            lock ( _obj )
            {
                // Add it.
                allKeys.Add( key, true );
                RockCacheManager<List<string>>.Instance.AddOrUpdate( AllKey, _AllRegion, allKeys );
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <returns></returns>
        /// <param name="keyFactory">All keys factory.</param>
        internal protected static List<string> GetOrAddKeys( Func<List<string>> keyFactory )
        {
            var value = RockCacheManager<List<string>>.Instance.Cache.Get( AllKey, _AllRegion );
            if ( value != null )
            {
                return value;
            }

            return keyFactory == null ? new List<string>() : AddKeys( keyFactory );
        }

        /// <summary>
        /// Adds the keys.
        /// </summary>
        /// <param name="keyFactory">All keys factory.</param>
        internal protected static List<string> AddKeys( Func<List<string>> keyFactory )
        {
            var allKeys = keyFactory?.Invoke();
            if ( allKeys != null )
            {
                RockCacheManager<List<string>>.Instance.AddOrUpdate( AllKey, _AllRegion, allKeys );
            }

            return allKeys;
        }

        #endregion

        #region Public Methods

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

        /// <summary>
        /// Creates a cache object from Json string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static T FromJson( string json )
        {
            return JsonConvert.DeserializeObject<T>( json );
        }

        /// <summary>
        /// Removes the specified key from cache and from AllIds. Call this if Deleting the object from the database.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Remove( int key )
        {
            Remove( key.ToString() );
        }

        /// <summary>
        /// Flushes the object from the cache without removing it from AllIds.
        /// Call this to force the cache to reload the object from the database the next time it is requested. 
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void FlushItem( int key )
        {
            FlushItem( key.ToString() );
        }

        internal static void AddToAllIds( int key)
        {
            AddToAllIds( key.ToString() );
        }

        /// <summary>
        /// Flushes the object from the cache without removing it from AllIds.
        /// Call this to force the cache to reload the object from the database the next time it is requested. 
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void FlushItem( string key )
        {
            var qualifiedKey = QualifiedKey( key );

            RockCacheManager<T>.Instance.Cache.Remove( qualifiedKey );
        }

        /// <summary>
        /// Removes the specified key from cache and from AllIds. Call this if Deleting the object from the database.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Remove( string key )
        {
            FlushItem( key );

            var allIds = RockCacheManager<List<string>>.Instance.Cache.Get( AllKey, _AllRegion ) ?? new List<string>();
            if ( !allIds.Contains( key ) )
                return;

            lock ( _obj )
            {
                allIds.Remove( key );
                RockCacheManager<List<string>>.Instance.AddOrUpdate( AllKey, _AllRegion, allIds );
            }
        }

        /// <summary>
        /// Removes all items of this type from cache.
        /// </summary>
        public static void Clear()
        {
            RockCacheManager<T>.Instance.Cache.Clear();
            RockCacheManager<List<string>>.Instance.Cache.Remove( AllKey, _AllRegion );
        }


        /// <summary>
        /// Method that is called by the framework immediately after being added to cache
        /// </summary>
        public virtual void PostCached()
        {
        }

        #endregion
    }
}

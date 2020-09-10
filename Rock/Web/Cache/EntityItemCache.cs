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
    /// Generic Item Cache for Entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public abstract class EntityItemCache<T> : IItemCache
        where T : IItemCache
    {
        /***********
          2018-07-20  MDP
            IMPORTANT NOTE!: The properties (Id, Guid, ForeignKey, etc) need to be declared here (on the bottom-most class) to support backward binary-compatibility with pre-v8 plugins.
            It seems to be related to issues discussed in https://blogs.msdn.microsoft.com/ericlippert/2010/03/29/putting-a-base-in-the-middle/

            So, this EntityItemCache<T> is intentionally somewhat of a duplicate of ItemCache<T> because of this issue. However, the duplicate methods call ItemCache<T> manually
            to minimize the amount of duplicate code.
        
        Update:
           2020-06-26 MDP
            Do not add or remove any properties from this class! This could break any plugins compiled against a pre-v1.8 Rock.dll. 
            
         
         ************/

        #region Properties

        /// <summary>
        /// Gets or sets the identifier 
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember]
        public virtual int Id { get; protected set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [DataMember]
        public virtual Guid Guid { get; protected set; }

        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        public virtual int? ForeignId { get; protected set; }

        /// <summary>
        /// Gets or sets the foreign unique identifier.
        /// </summary>
        /// <value>
        /// The foreign unique identifier.
        /// </value>
        [DataMember]
        public virtual Guid? ForeignGuid { get; protected set; }

        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        [DataMember]
        public virtual string ForeignKey { get; protected set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Returns the key prefixed with the type of object being cached.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected static string QualifiedKey( string key )
        {
            return ItemCache<T>.QualifiedKey( key );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <returns></returns>
        protected static T GetOrAddExisting( int key, Func<T> itemFactory )
        {
            return ItemCache<T>.GetOrAddExisting( key, itemFactory );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache with an expiration timespan.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        [RockObsolete( "1.11" )]
        [Obsolete( "Use the Lifespan properties instead of the expiration parameter." )]
        protected static T GetOrAddExisting( int key, Func<T> itemFactory, TimeSpan expiration )
        {
            return GetOrAddExisting( key, itemFactory );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <returns></returns>
        protected static T GetOrAddExisting( string key, Func<T> itemFactory )
        {
            return ItemCache<T>.GetOrAddExisting( key, itemFactory );
        }

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create item and add to cache with an expiration timespan.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        [RockObsolete( "1.11" )]
        [Obsolete( "Use the Lifespan properties instead of the expiration parameter." )]
        protected static T GetOrAddExisting( string key, Func<T> itemFactory, TimeSpan expiration )
        {
            return GetOrAddExisting( key, itemFactory );
        }

        /// <summary>
        /// Updates the cache item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        protected static void UpdateCacheItem( string key, T item )
        {
            ItemCache<T>.UpdateCacheItem( key, item );
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <returns></returns>
        /// <param name="keyFactory">All keys factory.</param>
        protected static List<string> GetOrAddKeys( Func<List<string>> keyFactory )
        {
            return ItemCache<T>.GetOrAddKeys( keyFactory );
        }

        /// <summary>
        /// Adds the keys.
        /// </summary>
        /// <param name="keyFactory">All keys factory.</param>
        protected static List<string> AddKeys( Func<List<string>> keyFactory )
        {
            return ItemCache<T>.AddKeys( keyFactory );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.GetType().Name} [Id={this.Id},Guid={this.Guid}]";
        }

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
            ItemCache<T>.Remove( key );
        }

        /// <summary>
        /// Flushes the object from the cache without removing it from AllIds.
        /// Call this to force the cache to reload the object from the database the next time it is requested. 
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void FlushItem( int key )
        {
            ItemCache<T>.FlushItem( key );
        }

        internal static void AddToAllIds( int key )
        {
            ItemCache<T>.AddToAllIds( key );
        }

        internal static void AddToAllIds( string key )
        {
            ItemCache<T>.AddToAllIds( key );
        }
        /// <summary>
        /// Flushes the object from the cache without removing it from AllIds.
        /// Call this to force the cache to reload the object from the database the next time it is requested. 
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void FlushItem( string key )
        {
            ItemCache<T>.FlushItem( key );
        }

        /// <summary>
        /// Removes the specified key from cache and from AllIds. Call this if Deleting the object from the database.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Remove( string key )
        {
            ItemCache<T>.Remove( key );
        }

        /// <summary>
        /// Removes all items of this type from cache.
        /// </summary>
        public static void Clear()
        {
            ItemCache<T>.Clear();
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

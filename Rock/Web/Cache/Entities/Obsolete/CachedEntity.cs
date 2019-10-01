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
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// A Secured data transfer object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    [RockObsolete( "1.8" )]
    [Obsolete("Use EntityCache instead")]
    public abstract class CachedEntity<T>  
        where T : Rock.Data.Entity<T>, new()
    {
    
        /// <summary>0
        /// Gets the existing or a new item from cache
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static TT GetOrAddExisting<TT>( string key, Func<TT> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            object cacheValue = cache.Get( key );
            if ( cacheValue != null )
            {
                return (TT)cacheValue;
            }

            TT value = valueFactory();
            if ( value != null )
            {
                cache.Set( key, value, new CacheItemPolicy() );
            }
            return value;
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static int GetOrAddExisting( string key, Func<int> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            object cacheValue = cache.Get( key );
            if ( cacheValue != null )
            {
                return (int)cacheValue;
            }

            int value = valueFactory();
            cache.Set( key, value, new CacheItemPolicy() );
            return value;
        }

        /// <summary>
        /// Gets the or add all.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static List<int> GetOrAddAll( string key, Func<List<int>> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            var value = cache.Get( key ) as List<int>;
            if ( value != null )
            {
                return value;
            }

            value = valueFactory();
            if ( value != null )
            {
                cache.Set( key, value, new CacheItemPolicy() );
            }
            return value;
        }

        /// <summary>
        /// Sets the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="policy">The policy.</param>
        public static void SetCache( string key, object item, CacheItemPolicy policy )
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Set( key, item, policy );
        }

        /// <summary>
        /// Caches the contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static bool CacheContainsKey( string key )
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            return cache.Contains( key );
        }

        /// <summary>
        /// Flushes the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void FlushCache( string key )
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Remove( key );
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [DataMember]
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [DataMember]
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets an optional int foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        public int? ForeignId { get; set; }

        /// <summary>
        /// Gets or sets an optional Guid foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        public Guid? ForeignGuid { get; set; }

        /// <summary>
        /// Gets or sets an optional string foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        public string ForeignKey { get; set; }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public virtual void CopyFromModel( Rock.Data.IEntity model )
        {
            this.Id = model.Id;
            this.Guid = model.Guid;
            this.ForeignId = model.ForeignId;
            this.ForeignGuid = model.ForeignGuid;
            this.ForeignKey = model.ForeignKey;

            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Set( model.Guid.ToString(), model.Id, new CacheItemPolicy() );
        }

    }
}
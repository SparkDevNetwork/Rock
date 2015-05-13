// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// A Secured data transfer object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public abstract class CachedEntity<T>  
        where T : Rock.Data.Entity<T>, new()
    {
    
        /// <summary>
        /// Gets the existing or a new item from cache
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static TT GetOrAddExisting<TT>( string key, Func<TT> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            var newValue = new Lazy<TT>( valueFactory );

            var oldValue = cache.AddOrGetExisting( key, newValue, new CacheItemPolicy() ) as Lazy<TT>;
            try
            {
                return ( oldValue ?? newValue ).Value;
            }
            catch
            {
                cache.Remove( key );
                throw;
            }
        }

        /// <summary>
        /// Ases the lazy.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static TT AsLazy<TT>( TT item )
        {
            return item;
        }

        /// <summary>
        /// Ases the lazy.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static int AsLazy( int item )
        {
            return item;
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

            var newValue = new Lazy<int>( valueFactory );
            var oldValue = cache.AddOrGetExisting( key, newValue, new CacheItemPolicy() ) as Lazy<int>;
            try
            {
                return ( oldValue ?? newValue ).Value;
            }
            catch
            {
                cache.Remove( key );
                throw;
            }
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

            var newValue = new Lazy<List<int>>( valueFactory );
            var oldValue = cache.AddOrGetExisting( key, newValue, new CacheItemPolicy() ) as Lazy<List<int>>;
            try
            {
                return ( oldValue ?? newValue ).Value;
            }
            catch
            {
                cache.Remove( key );
                throw;
            }
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
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public virtual void CopyFromModel( Rock.Data.IEntity model )
        {
            this.Id = model.Id;
            this.Guid = model.Guid;

            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Set( model.Guid.ToString(), new Lazy<int>( () => AsLazy( model.Id ) ), new CacheItemPolicy() );
        }

    }
}
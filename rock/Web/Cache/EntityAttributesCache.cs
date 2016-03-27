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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Attributes for each entity type
    /// </summary>
    [Serializable]
    public class EntityAttributesCache
    {
        #region Properties

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<EntityAttributes> EntityAttributes { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="EntityAttributesCache"/> class from being created.
        /// </summary>
        private EntityAttributesCache() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAttributesCache" /> class.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityAttributes">The attributes.</param>
        private EntityAttributesCache( int? entityTypeId, List<EntityAttributes> entityAttributes )
        {
            EntityTypeId = entityTypeId;
            EntityAttributes = entityAttributes;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Reads the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityAttributesCache Read( int? entityTypeId, RockContext rockContext = null )
        {
            return GetOrAddExisting( EntityAttributesCache.CacheKey( entityTypeId ),
                () => Load( entityTypeId, rockContext ) );
        }

        private static EntityAttributesCache Load( int? entityTypeId, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return Load2( entityTypeId, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return Load2( entityTypeId, rockContext2 );
            }
        }

        private static EntityAttributesCache Load2( int? entityTypeId, RockContext rockContext )
        {
            var attributeQry = new AttributeService( rockContext )
                .Queryable().AsNoTracking();

            if ( entityTypeId.HasValue )
            {
                attributeQry = attributeQry
                    .Where( a =>
                        a.EntityTypeId.HasValue &&
                        a.EntityTypeId.Value == entityTypeId.Value );
            }
            else
            {
                attributeQry = attributeQry
                    .Where( a => !a.EntityTypeId.HasValue );
            }

            var attributes = attributeQry
                .GroupBy( a => new
                {
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue
                } )
                .Select( a => new EntityAttributes()
                {
                    EntityTypeQualifierColumn = a.Key.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = a.Key.EntityTypeQualifierValue,
                    AttributeIds = a.Select( v => v.Id ).ToList()
                } )
                .ToList();

            return new EntityAttributesCache( entityTypeId, attributes );
        }

        /// <summary>
        /// Gets the existing or a new item from cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        private static EntityAttributesCache GetOrAddExisting( string key, Func<EntityAttributesCache> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            object cacheValue = cache.Get( key );
            if ( cacheValue != null )
            {
                return (EntityAttributesCache)cacheValue;
            }

            EntityAttributesCache value = valueFactory();
            if ( value != null )
            {
                cache.Set( key, value, new CacheItemPolicy() );
            }
            return value;
        }

        /// <summary>
        /// Gets the cache key for the selected entity type id.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        private static string CacheKey( int? entityTypeId )
        {
            return string.Format( "Rock:EntityAttributes:{0}", ( entityTypeId.HasValue ? entityTypeId.Value.ToString() : "" ) );
        }

        /// <summary>
        /// Flushes the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        public static void Flush( int? entityTypeId )
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Remove( CacheKey( entityTypeId ) );
        }

        #endregion

    }

    #region Helper class for entity attributes

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EntityAttributes
    {
        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        public List<int> AttributeIds { get; set; }
    }

    #endregion
}
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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Collection of all entity attribute Ids for each EntityType
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityAttributesCache : ItemCache<EntityAttributesCache>
    {
        private const string KEY = "AllEntityAttributes";

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private EntityAttributesCache()
        {
        }

        /// <summary>
        /// Gets or sets the entity attributes.
        /// </summary>
        /// <value>
        /// The entity attributes.
        /// </value>
        [DataMember]
        public List<EntityAttributes> EntityAttributes { get; set; }

        /// <summary>
        /// Gets or sets the entity attributes by entity type identifier.
        /// </summary>
        /// <value>
        /// The entity attributes by entity type identifier.
        /// </value>
        [DataMember]
        public Dictionary<int, List<EntityAttributes>> EntityAttributesByEntityTypeId { get; set; }

        #region Public Methods

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static EntityAttributesCache Get()
        {
            return Get( null );
        }

        /// <summary>
        /// Gets the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityAttributesCache Get( RockContext rockContext )
        {
            return GetOrAddExisting( KEY, () => QueryDb( rockContext ) );
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public static void Remove()
        {
            Remove( KEY );
        }

        #endregion

        #region Private Methods 

        private static EntityAttributesCache QueryDb( RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return QueryDbWithContext( rockContext2 );
            }
        }

        private static EntityAttributesCache QueryDbWithContext( RockContext rockContext )
        {
            var entityAttributes = new AttributeService( rockContext )
                .Queryable().AsNoTracking()
                .GroupBy( a => new
                {
                    a.EntityTypeId,
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue
                } )
                .Select( a => new EntityAttributes()
                {
                    EntityTypeId = a.Key.EntityTypeId,
                    EntityTypeQualifierColumn = a.Key.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = a.Key.EntityTypeQualifierValue,
                    AttributeIds = a.Select( v => v.Id ).ToList()
                } )
                .ToList();

            var value = new EntityAttributesCache
            {
                EntityAttributes = entityAttributes,
                EntityAttributesByEntityTypeId = entityAttributes.Where( a => a.EntityTypeId.HasValue ).GroupBy( g => g.EntityTypeId.Value ).ToDictionary( k => k.Key, v => v.ToList() ?? new List<EntityAttributes>() )
            };

            return value;
        }

        /// <summary>
        /// Updates the <see cref="EntityAttributesCache"/> based on the attribute and entityState
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="entityState">State of the entity.</param>
        internal static void UpdateCacheEntityAttributes( Rock.Model.Attribute attribute, EntityState entityState )
        {
            var entityAttributesList = EntityAttributesCache.Get().EntityAttributes.ToList();
            if ( entityAttributesList == null || attribute == null )
            {
                return;
            }

            bool listUpdated = false;

            if ( entityState == EntityState.Deleted )
            {
                foreach ( var entityAttributesItem in entityAttributesList.Where( a => a.AttributeIds.Contains( attribute.Id ) ) )
                {
                    entityAttributesItem.AttributeIds.Remove( attribute.Id );
                    listUpdated = true;
                }
            }

            if ( attribute?.EntityTypeId.HasValue == true )
            {
                if ( entityState == EntityState.Modified )
                {
                    foreach ( var entityAttributesItem in entityAttributesList.Where( a => a.AttributeIds.Contains( attribute.Id ) ) )
                    {
                        if ( entityAttributesItem.EntityTypeId != attribute.EntityTypeId || entityAttributesItem.EntityTypeQualifierColumn != attribute.EntityTypeQualifierColumn || entityAttributesItem.EntityTypeQualifierValue != attribute.EntityTypeQualifierValue )
                        {
                            entityAttributesItem.AttributeIds.Remove( attribute.Id );
                            listUpdated = true;
                        }
                    }
                }

                if ( entityState == EntityState.Added || entityState == EntityState.Modified )
                {
                    var entityTypeEntityAttributesList = entityAttributesList.Where( a =>
                        a.EntityTypeId == attribute.EntityTypeId.Value
                        && a.EntityTypeQualifierColumn == attribute.EntityTypeQualifierColumn
                        && a.EntityTypeQualifierValue == attribute.EntityTypeQualifierValue );

                    if ( entityTypeEntityAttributesList.Any() )
                    {
                        foreach ( var entityAttributes in entityTypeEntityAttributesList )
                        {
                            if ( !entityAttributes.AttributeIds.Contains( attribute.Id ) )
                            {
                                entityAttributes.AttributeIds.Add( attribute.Id );
                                listUpdated = true;
                            }
                        }
                    }
                    else
                    {
                        entityAttributesList.Add( new EntityAttributes()
                        {
                            EntityTypeId = attribute.EntityTypeId,
                            EntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn,
                            EntityTypeQualifierValue = attribute.EntityTypeQualifierValue,
                            AttributeIds = new List<int>( new int[] { attribute.Id } )
                        } );

                        listUpdated = true;
                    }
                }
            }

            if ( listUpdated )
            {
                var cache = EntityAttributesCache.Get();
                cache.EntityAttributes = entityAttributesList;
                cache.EntityAttributesByEntityTypeId = entityAttributesList.Where( a => a.EntityTypeId.HasValue ).GroupBy( g => g.EntityTypeId.Value ).ToDictionary( k => k.Key, v => v.ToList() ?? new List<EntityAttributes>() );
                EntityAttributesCache.UpdateCacheItem( KEY, cache, TimeSpan.MaxValue );
            }

        }

        #endregion
    }

    /// <summary>
    /// Helper Class
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityAttributes
    {
        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
		[DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        [DataMember]
        public List<int> AttributeIds { get; set; }
    }
}

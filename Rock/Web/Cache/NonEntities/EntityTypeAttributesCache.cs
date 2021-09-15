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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Collection of all entity attribute Ids for the EntityType
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityTypeAttributesCache : ItemCache<EntityTypeAttributesCache>
    {
        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private EntityTypeAttributesCache()
        {
        }

        [DataMember]
        private AttributeIdsForEntityQualifier[] AttributeIdListForEntityQualifier { get; set; }

        /// <summary>
        /// Gets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        [DataMember]
        internal int[] AttributeIds { get; private set; }

        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="EntityTypeAttributesCache" /> for the specified <paramref name="entityTypeId" />
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static EntityTypeAttributesCache Get( int? entityTypeId )
        {
            return Get( entityTypeId, null );
        }

        /// <summary>
        /// Gets the entity type identifier key.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        private static string GetEntityTypeIdCacheKey( int? entityTypeId )
        {
            string key;
            if ( entityTypeId.HasValue )
            {
                key = entityTypeId.Value.ToString();
            }
            else
            {
                key = "null_entity_type_id";
            }

            return key;
        }

        /// <summary>
        /// Returns a <see cref="EntityTypeAttributesCache"/> for the specified <paramref name="entityTypeId"/>
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeAttributesCache Get( int? entityTypeId, RockContext rockContext )
        {
            var key = GetEntityTypeIdCacheKey( entityTypeId );
            return GetOrAddExisting( GetEntityTypeIdCacheKey( entityTypeId ), () => QueryDb( entityTypeId, rockContext ) );
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public static void FlushItem( int? entityTypeId )
        {
            ItemCache<EntityTypeAttributesCache>.FlushItem( GetEntityTypeIdCacheKey( entityTypeId ) );
        }

        /// <summary>
        /// Gets the by entity type qualifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        internal static AttributeCache[] GetByEntityTypeQualifier( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, bool includeInactive )
        {
            IEnumerable<AttributeIdsForEntityQualifier> attributeIdListForEntityQualifier = Get( entityTypeId ).AttributeIdListForEntityQualifier;

            if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
            {
                attributeIdListForEntityQualifier = attributeIdListForEntityQualifier.Where( t => t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == string.Empty ).ToList();
            }
            else
            {
                attributeIdListForEntityQualifier = attributeIdListForEntityQualifier.Where( t => t.EntityTypeQualifierColumn == entityQualifierColumn ).ToList();
            }

            if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
            {
                attributeIdListForEntityQualifier = attributeIdListForEntityQualifier.Where( t => t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == string.Empty ).ToList();
            }
            else
            {
                attributeIdListForEntityQualifier = attributeIdListForEntityQualifier.Where( t => t.EntityTypeQualifierValue == entityQualifierValue ).ToList();
            }

            var attributeIds = attributeIdListForEntityQualifier.SelectMany( t => t.AttributeIds );
            var attributes = attributeIds.Select( x => AttributeCache.Get( x ) );

            if ( !includeInactive )
            {
                attributes = attributes.Where( a => a.IsActive == true );
            }

            return attributes.ToArray();
        }

        #endregion

        #region Private Methods 

        /// <summary>
        /// Queries the database.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static EntityTypeAttributesCache QueryDb( int? entityTypeId, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( entityTypeId, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return QueryDbWithContext( entityTypeId, rockContext2 );
            }
        }

        /// <summary>
        /// Queries the database with context.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static EntityTypeAttributesCache QueryDbWithContext( int? entityTypeId, RockContext rockContext )
        {
            var entityTypeAttributeQuery = new AttributeService( rockContext ).Queryable();

            if ( entityTypeId.HasValue )
            {
                entityTypeAttributeQuery = entityTypeAttributeQuery.Where( a => a.EntityTypeId.HasValue && a.EntityTypeId.Value == entityTypeId.Value );
            }
            else
            {
                entityTypeAttributeQuery = entityTypeAttributeQuery.Where( a => !a.EntityTypeId.HasValue );
            }

            var entityTypeAttributeIds = entityTypeAttributeQuery
                .Select( a => new
                {
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue,
                    a.Id
                } )
                .ToList()
                .GroupBy( a => new
                {
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue
                } ).Select( a => new AttributeIdsForEntityQualifier()
                {
                    EntityTypeQualifierColumn = a.Key.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = a.Key.EntityTypeQualifierValue,
                    AttributeIds = a.Select( v => v.Id ).ToArray()
                } )
            .ToArray();

            return new EntityTypeAttributesCache
            {
                AttributeIdListForEntityQualifier = entityTypeAttributeIds,
                AttributeIds = entityTypeAttributeIds.SelectMany( a => a.AttributeIds ).ToArray(),
                EntityTypeId = entityTypeId
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private class AttributeIdsForEntityQualifier
        {
            public string EntityTypeQualifierColumn { get; internal set; }

            public string EntityTypeQualifierValue { get; internal set; }

            public int[] AttributeIds { get; internal set; }
        }

        #endregion
    }
}
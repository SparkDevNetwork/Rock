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

namespace Rock.Cache
{
    /// <summary>
    /// Collection of all entity attribute Ids
    /// </summary>
    [Serializable]
    [DataContract]
    public class CacheEntityAttributes : ItemCache<CacheEntityAttributes>
    {
        private const string KEY = "AllEntityAttributes";

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private CacheEntityAttributes()
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

        #region Public Methods

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static CacheEntityAttributes Get()
        {
            return Get( null );
        }

        /// <summary>
        /// Gets the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CacheEntityAttributes Get( RockContext rockContext )
        {
            return GetOrAddExisting( KEY, () => QueryDb( rockContext ) );
        }

        ///// <summary>
        ///// Refreshes the entity.
        ///// </summary>
        ///// <param name="entityTypeId">The entity type identifier.</param>
        ///// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        ///// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        //public static void RefreshEntity( int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        //{
        //    var value = Get();
        //    if ( value == null ) return;

        //    var attributeQry = new AttributeService( new RockContext() )
        //        .Queryable().AsNoTracking();

        //    attributeQry = entityTypeId.HasValue ?
        //        attributeQry.Where( a => a.EntityTypeId.HasValue && a.EntityTypeId.Value == entityTypeId.Value ) :
        //        attributeQry.Where( a => !a.EntityTypeId.HasValue );

        //    attributeQry = attributeQry
        //        .Where( a => 
        //            a.EntityTypeQualifierColumn == entityTypeQualifierColumn &&
        //            a.EntityTypeQualifierValue == entityTypeQualifierValue );

        //    var attributeIds = attributeQry.Select(v => v.Id).ToList();

        //    var entityAttributes = value.EntityAttributes
        //        .FirstOrDefault(a =>
        //            (a.EntityTypeId ?? 0) == (entityTypeId ?? 0) &&
        //            a.EntityTypeQualifierColumn == entityTypeQualifierColumn &&
        //            a.EntityTypeQualifierValue == entityTypeQualifierValue);

        //    if ( attributeIds.Any( ) )
        //    {
        //        if (entityAttributes == null)
        //        {
        //            entityAttributes = new EntityAttributes
        //            {
        //                EntityTypeId = entityTypeId,
        //                EntityTypeQualifierColumn = entityTypeQualifierColumn,
        //                EntityTypeQualifierValue = entityTypeQualifierValue
        //            };
        //            value.EntityAttributes.Add( entityAttributes );
        //        }

        //        entityAttributes.AttributeIds = attributeIds;
        //    }
        //    else
        //    {
        //        if (entityAttributes != null)
        //        {
        //            value.EntityAttributes.Remove(entityAttributes);
        //        }
        //    }

        //    UpdateCacheItem( KEY, value, TimeSpan.MaxValue );
        //}

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public static void Remove()
        {
            Remove( KEY );
        }

        #endregion

        #region Private Methods 

        private static CacheEntityAttributes QueryDb( RockContext rockContext )
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

        private static CacheEntityAttributes QueryDbWithContext( RockContext rockContext )
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

            var value = new CacheEntityAttributes { EntityAttributes = entityAttributes };
            return value;

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
        public int? EntityTypeId { get; set; }

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
}

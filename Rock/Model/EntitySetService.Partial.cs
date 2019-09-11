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
using System.Linq;
using System.Reflection;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// EntitySetItem POCO Service class
    /// </summary>
    public partial class EntitySetService
    {
        /// <summary>
        /// Gets the entity query, ordered by the EntitySetItem.Order
        /// For example: If the EntitySet.EntityType is Person, this will return a Person Query of the items in this set
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetEntityQuery( int entitySetId )
        {
            var entitySet = this.Get( entitySetId );
            if ( !entitySet.EntityTypeId.HasValue )
            {
                // the EntitySet Items are not IEntity items
                return null;
            }

            EntityTypeCache itemEntityType = EntityTypeCache.Get( entitySet.EntityTypeId.Value );

            var rockContext = this.Context as RockContext;
            var entitySetItemsService = new EntitySetItemService( rockContext );
            var entityItemQry = entitySetItemsService.Queryable().Where( a => a.EntitySetId == entitySetId ).OrderBy( a => a.Order );

            bool isPersonEntitySet = itemEntityType.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();

            if ( itemEntityType.AssemblyName != null )
            {
                Type entityType = itemEntityType.GetEntityType();
                if ( entityType != null )
                {
                    Type[] modelType = { entityType };
                    Type genericServiceType = typeof( Rock.Data.Service<> );
                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                    Rock.Data.IService serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;

                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    var joinQry = entityItemQry.Join( entityQry, k => k.EntityId, i => i.Id, ( setItem, item ) => new
                    {
                        Item = item,
                        ItemOrder = setItem.Order
                    }
                    ).OrderBy( a => a.ItemOrder ).ThenBy( a => a.Item.Id );

                    return joinQry.Select( a => a.Item );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the entity query based for Items that are in the EntitySet
        /// Note that this does not include duplicates and isn't sorted by EntitySetItem.Order
        /// For example: If the EntitySet.EntityType is Person, this will return a Person Query of the items in this set
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns></returns>
        public IQueryable<T> GetEntityQuery<T>( int entitySetId ) where T : Rock.Data.Entity<T>, new()
        {
            var rockContext = this.Context as RockContext;
            var entitySetItemsService = new EntitySetItemService( rockContext );
            var entityItemEntityIdQry = entitySetItemsService.Queryable().Where( a => a.EntitySetId == entitySetId ).Select( a => a.EntityId );

            var entityQry = new Service<T>( rockContext ).Queryable();

            entityQry = entityQry.Where( a => entityItemEntityIdQry.Contains( a.Id ) );

            return entityQry;
        }

        /// <summary>
        /// Gets the entity items with the Entity when the Type is known at design time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<EntityQueryResult<T>> GetEntityItems<T>() where T : Rock.Data.Entity<T>, new()
        {
            var rockContext = this.Context as RockContext;
            var entitySetItemsService = new EntitySetItemService( rockContext );
            var entityItemQry = entitySetItemsService.Queryable();

            var entityQry = new Service<T>( this.Context as RockContext ).Queryable();

            var joinQry = entityItemQry.Join( entityQry, k => k.EntityId, i => i.Id, ( setItem, item ) => new
            {
                Item = item,
                EntitySetId = setItem.EntitySetId,
                ItemOrder = setItem.Order
            } ).OrderBy( a => a.ItemOrder ).ThenBy( a => a.Item.Id );

            return joinQry.Select( a => new EntityQueryResult<T>
            {
                EntitySetId = a.EntitySetId,
                Item = a.Item
            } );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class EntityQueryResult<T>
        {
            /// <summary>
            /// Gets or sets the entity set identifier.
            /// </summary>
            /// <value>
            /// The entity set identifier.
            /// </value>
            public int EntitySetId { get; set; }

            /// <summary>
            /// Gets or sets the item.
            /// </summary>
            /// <value>
            /// The item.
            /// </value>
            public T Item { get; set; }
        }

    }
}

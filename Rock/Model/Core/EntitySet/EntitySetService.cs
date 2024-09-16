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
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// EntitySet Service class.
    /// </summary>
    public partial class EntitySetService
    {
        /// <summary>
        /// Create a new Entity Set for the specified entities.
        /// </summary>
        /// <remarks>
        /// This method uses a bulk insert to improve performance when creating large Entity Sets.
        /// </remarks>
        /// <param name="options"></param>
        /// <returns></returns>
        public int AddEntitySet( AddEntitySetActionOptions options )
        {
            // Create a new Entity Set.
            var entitySet = new Rock.Model.EntitySet();
            entitySet.Name = options.Name;
            entitySet.EntityTypeId = options.EntityTypeId;
            entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( options.ExpiryInMinutes ?? 20 );
            entitySet.Note = options.Note;

            entitySet.ParentEntitySetId = options.ParentEntitySetId;
            
            // Set the Entity Set Purpose.
             if ( options.PurposeValueId != null )
            {
                var purposeDefinedType = DefinedTypeCache.Get( SystemGuid.DefinedType.ENTITY_SET_PURPOSE );

                var purposeIsValid = purposeDefinedType.DefinedValues.Any( v => v.Id == options.PurposeValueId );
                if ( !purposeIsValid )
                {
                    throw new Exception( $"Invalid Entity Set Purpose Value. [Value={options.PurposeValueId}]" );
                }

                entitySet.EntitySetPurposeValueId = options.PurposeValueId;
            }

            Add( entitySet );

            var rockContext = ( RockContext ) this.Context;
            rockContext.SaveChanges();

            // Add items to the new Entity Set, using a bulk insert to optimize performance.
            var entityIdList = options.EntityIdList;

            if ( entityIdList != null
                 && entityIdList.Any() )
            {
                var entitySetItems = new List<Rock.Model.EntitySetItem>();

                foreach ( var key in entityIdList )
                {
                    var item = new Rock.Model.EntitySetItem();
                    item.EntityId = key;
                    item.EntitySetId = entitySet.Id;

                    entitySetItems.Add( item );
                }

                rockContext.BulkInsert( entitySetItems );
            }

            return entitySet.Id;
        }

        /// <summary>
        /// Create a new Entity Set for the specified entities.
        /// </summary>
        /// <remarks>
        /// This method uses a bulk insert to improve performance when creating large Entity Sets.
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="entityTypeId"></param>
        /// <param name="entityIdList"></param>
        /// <param name="expiryMinutes"></param>
        /// <returns></returns>
        [Obsolete( "Use AddEntitySet(options) instead." )]
        [RockObsolete( "1.16" )]
        public int AddEntitySet( string name, int entityTypeId, IEnumerable<int> entityIdList, int expiryMinutes = 20 )
        {
            var options = new AddEntitySetActionOptions()
            {
                Name = name,
                EntityTypeId = entityTypeId,
                ExpiryInMinutes = expiryMinutes,
                EntityIdList = entityIdList
            };

            return AddEntitySet( options );
        }

        /// <summary>
        /// Gets the entity query, ordered by the EntitySetItem.Order
        /// For example: If the EntitySet.EntityType is Person, this will return a Person Query of the items in this set
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetEntityQuery( int entitySetId )
        {
            var entitySet = this.Get( entitySetId );
            if ( entitySet?.EntityTypeId == null )
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
        /// Gets the entity query based for Items that are in the EntitySet
        /// Note that this does not include duplicates and isn't sorted by EntitySetItem.Order
        /// For example: If the EntitySet.EntityType is Person, this will return a Person Query of the items in this set
        /// </summary>
        /// <param name="entitySetGuid">The entity set unique identifier.</param>
        /// <returns></returns>
        [RockInternal( "1.15.2" )]
        public IQueryable<T> GetEntityQuery<T>( Guid entitySetGuid ) where T : Rock.Data.Entity<T>, new()
        {
            var rockContext = this.Context as RockContext;
            var entitySetItemsService = new EntitySetItemService( rockContext );
            var entityItemEntityIdQuery = entitySetItemsService.Queryable().Where( a => a.EntitySet.Guid == entitySetGuid ).Select( a => a.EntityId );

            var entityQry = new Service<T>( rockContext ).Queryable();

            entityQry = entityQry.Where( a => entityItemEntityIdQuery.Contains( a.Id ) );

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
        /// Launch a workflow for each item in the set using a Rock transaction.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        public void LaunchWorkflows( int entitySetId, int workflowTypeId )
        {
            LaunchWorkflows( entitySetId, workflowTypeId, null );
        }

        /// <summary>
        /// Launch a workflow for each item in the set using a Rock transaction.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="attributeValues">The attribute values.</param>
        public void LaunchWorkflows( int entitySetId, int workflowTypeId, Dictionary<string, string> attributeValues )
        {
            var query = GetEntityQuery( entitySetId ).AsNoTracking();
            var entities = query.ToList();
            var launchWorkflowDetails = entities.Select( e => new LaunchWorkflowDetails( e, attributeValues ) ).ToList();

            // Queue a transaction to launch workflow
            new LaunchWorkflowsTransaction( workflowTypeId, launchWorkflowDetails ).Enqueue();
        }

        /// <summary>
        /// Launch a workflow for each item in the set using a Rock transaction.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="initiatorPersonAliasId">The initiator person alias identifier.</param>
        /// <param name="attributeValues">The attribute values.</param>
        public void LaunchWorkflows( int entitySetId, int workflowTypeId, int? initiatorPersonAliasId, Dictionary<string, string> attributeValues )
        {
            var query = GetEntityQuery( entitySetId ).AsNoTracking();
            var entities = query.ToList();
            var launchWorkflowDetails = entities.Select( e => new LaunchWorkflowDetails( e, attributeValues ) ).ToList();
            var launchWorkflowsTransaction = new LaunchWorkflowsTransaction( workflowTypeId, launchWorkflowDetails );
            launchWorkflowsTransaction.InitiatorPersonAliasId = initiatorPersonAliasId;
            // Queue a transaction to launch workflow
            launchWorkflowsTransaction.Enqueue();
        }

        /// <summary>
        /// Creates an entity set from a list of entity item IDs and an entity type ID.
        /// </summary>
        /// <param name="entityItemIds">The list of entity item IDs to include in the entity set.</param>
        /// <param name="entityTypeId">The ID of the entity type of the entity set.</param>
        /// <param name="timeToExpire">The amount of time (in minutes) before the entity set is expired.</param>
        /// <param name="rockContext">The optional rock context to use for the operation.</param>
        /// <returns>The ID of the newly created entity set, or null if it was unable to create.</returns>
        [RockInternal("1.15")]
        internal static int? CreateEntitySetFromItems( List<int> entityItemIds, int entityTypeId, int timeToExpire = 15, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return CreateEntitySetFromItemIds( entityItemIds, entityTypeId, timeToExpire, rockContext )?.Id;
        }

        /// <summary>
        /// Creates an entity set from a list of entity item GUIDs and an entity type GUID.
        /// </summary>
        /// <param name="entityItemGuids">The list of entity item GUIDs to include in the entity set.</param>
        /// <param name="entityTypeGuid">The GUID of the entity type of the entity set.</param>
        /// <param name="timeToExpire">The amount of times in minutes until the entity set expires. 0 to disable.</param>
        /// <param name="rockContext">The optional rock context to use for the operation.</param>
        /// <returns>The GUID of the newly created entity set, or null if the entity service for the entity type was not found.</returns>
        [RockInternal( "1.15" )]
        internal static Guid? CreateEntitySetFromItems( List<Guid> entityItemGuids, Guid entityTypeGuid, int timeToExpire = 15, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            // Get the entity type from its GUID.
            var entityType = EntityTypeCache.Get( entityTypeGuid );

            // Dynamically get the IService for the entity type and then get a queryable to load the entities.
            var entityService = Rock.Reflection.GetServiceForEntityType( entityType.GetEntityType(), rockContext );
            var asQueryableMethod = entityService?.GetType().GetMethod( "Queryable", Array.Empty<Type>() );

            // If the entity service is null, then the entity type is not a valid IEntity type.
            if ( asQueryableMethod == null )
            {
                return null;
            }

            // Get a queryable for the IEntity type.
            var entityQry = ( IQueryable<IEntity> ) asQueryableMethod.Invoke( entityService, Array.Empty<object>() );

            var entityIds = new List<int>();
            while ( entityItemGuids.Any() )
            {
                // Load at most 1,000 entities at a time since it performs better than loading all of them at once.
                var guidsToProcess = entityItemGuids.Take( 1_000 ).ToList();
                entityItemGuids = entityItemGuids.Skip( 1_000 ).ToList();

                // Load all the entities from the GUIDs.
                var ids = entityQry
                    .AsNoTracking()
                    .Where( e => guidsToProcess.Contains( e.Guid ) )
                    .Select( e => e.Id )
                    .ToList();

                entityIds.AddRange( ids );
            }

            // Create an entity set from the entity item IDs.
            return CreateEntitySetFromItemIds( entityIds, entityType.Id, timeToExpire, rockContext )?.Guid;
        }

        /// <summary>
        /// Creates an entity set and returns the ID and Guid of that entity set.
        /// </summary>
        /// <param name="entityItemIds">The entity item ids.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="timeToExpire">The amount of time in minutes before the entity set expires.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The Guid and the Id of the entity set that was created, or null if it was unable to create.</returns>
        private static (int Id, Guid Guid)? CreateEntitySetFromItemIds( List<int> entityItemIds, int entityTypeId, int timeToExpire = 15, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            // Create the entity set and set the default expiration date.
            var entitySet = new Rock.Model.EntitySet();
            entitySet.EntityTypeId = entityTypeId;

            if ( timeToExpire > 0 )
            {
                entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( timeToExpire );
            }

            // For each entity item id, add a new entity set item to the entity set.
            List<Rock.Model.EntitySetItem> entitySetItems = new List<Rock.Model.EntitySetItem>();
            foreach ( var entityItemId in entityItemIds )
            {
                try
                {
                    var item = new Rock.Model.EntitySetItem();
                    item.EntityId = ( int ) entityItemId;
                    entitySetItems.Add( item );
                }
                catch
                {
                    // ignore
                }
            }

            if ( entitySetItems.Any() )
            {
                var service = new Rock.Model.EntitySetService( rockContext );
                service.Add( entitySet );
                rockContext.SaveChanges();
                entitySetItems.ForEach( a =>
                {
                    a.EntitySetId = entitySet.Id;
                } );

                rockContext.BulkInsert( entitySetItems );

                return (entitySet.Id, entitySet.Guid);
            }

            return null;
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

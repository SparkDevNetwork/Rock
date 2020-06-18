﻿// <copyright>
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
using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using Rock.Web.Cache;
using Z.EntityFramework.Plus;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RelatedEntityHelper<T> where T : Rock.Data.Entity<T>, new()
    {
        private Service<T> Service { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelatedEntityHelper{T}"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public RelatedEntityHelper( Service<T> service )
        {
            Service = service;
        }

        private DbContext Context => Service.Context;

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> related target entities (related to the given source entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <typeparam name="TT">The type of the Related Entities.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns></returns>
        public IQueryable<TT> GetRelatedToSourceEntity<TT>( int entityId, string purposeKey ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var relatedEntities = GetRelatedToSourceEntity( entityId, relatedEntityTypeId.Value, purposeKey ).Cast<TT>();

            return relatedEntities;
        }

        /// <summary>
        /// Deletes any related entities that reference the specified entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void DeleteRelatedEntities( T entity )
        {
            var relatedEntityService = new RelatedEntityService( this.Context as RockContext );
            var sourceOrTargetEntityTypeId = EntityTypeCache.GetId<T>();
            var relatedEntityRecords = relatedEntityService.Queryable().Where( a => ( a.SourceEntityTypeId == sourceOrTargetEntityTypeId && a.SourceEntityId == entity.Id ) || ( a.TargetEntityTypeId == sourceOrTargetEntityTypeId && a.TargetEntityId == entity.Id ) ).ToList();
            relatedEntityService.DeleteRange( relatedEntityRecords );
        }

        /// <summary>
        /// Adds a relationship between the relatedEntity and the entity
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntity">The related entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        public void AddRelatedToSourceEntity<TT>( int entityId, TT relatedEntity, string purposeKey ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();

            var relatedEntityRecord = new RelatedEntity();
            relatedEntityRecord.SourceEntityTypeId = sourceEntityTypeId.Value;
            relatedEntityRecord.SourceEntityId = entityId;

            relatedEntityRecord.TargetEntityTypeId = relatedEntityTypeId.Value;
            relatedEntityRecord.TargetEntityId = relatedEntity.Id;

            relatedEntityRecord.PurposeKey = purposeKey;
            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );

            relatedEntityService.Add( relatedEntityRecord );
        }

        /// <summary>
        /// Deletes (deletes relationship) the relationship between the relatedEntity and the entity
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntity">The related entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        public void DeleteRelatedToSourceEntity<TT>( int entityId, TT relatedEntity, string purposeKey ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();

            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            var relatedEntityRecords = relatedEntityService.GetRelatedEntityRecordsToSource( entityId, sourceEntityTypeId.Value, relatedEntityTypeId.Value, purposeKey );

            relatedEntityService.DeleteRange( relatedEntityRecords );
        }

        /// <summary>
        /// Determines if relationship to the source entity already exists.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntity">The related entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns></returns>
        public bool RelatedToSourceEntityAlreadyExists<TT>( int entityId, TT relatedEntity, string purposeKey ) where TT : IEntity
        {
            if ( entityId == 0 || relatedEntity.Id == 0 )
            {
                return false;
            }

            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();

            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            if ( relatedEntityService.Queryable().Any( a =>
                      a.SourceEntityTypeId == sourceEntityTypeId.Value
                      && a.SourceEntityId == entityId
                      && a.TargetEntityTypeId == relatedEntityTypeId.Value
                      && a.TargetEntityId == relatedEntity.Id
                      && a.PurposeKey == purposeKey
                    )
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the related target entities (related to the given source entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntities">The related entities.</param>
        /// <param name="purposeKey">The purpose key.</param>
        public void SetRelatedToSourceEntity<TT>( int entityId, List<TT> relatedEntities, string purposeKey ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();
            var currentRelatedEntities = GetRelatedToSourceEntity<TT>( entityId, purposeKey ).ToList();

            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            var relatedEntityIds = relatedEntities.Select( a => a.Id ).ToList();

            // delete related entities that are no longer in the list
            foreach ( var currentRelatedEntity in currentRelatedEntities.Where( a => !relatedEntityIds.Contains( a.Id ) ) )
            {
                // get related entity record(s) that need to be deleted since the relatedEntity is no longer in the list
                var relatedEntityToDelete = relatedEntityService.Queryable()
                    .Where( a => a.SourceEntityTypeId == sourceEntityTypeId.Value
                            && a.TargetEntityTypeId == relatedEntityTypeId.Value
                            && a.SourceEntityId == entityId
                            && a.TargetEntityId == currentRelatedEntity.Id
                            && a.PurposeKey == purposeKey ).ToList();
                relatedEntityService.DeleteRange( relatedEntityToDelete );
            }

            // add related entity record for related entities that that don't have a related entity record
            foreach ( var relatedEntityId in relatedEntityIds.Where( a => !currentRelatedEntities.Any( r => r.Id == a ) ) )
            {
                var relatedEntityRecord = new RelatedEntity();
                relatedEntityRecord.SourceEntityTypeId = sourceEntityTypeId.Value;
                relatedEntityRecord.SourceEntityId = entityId;

                relatedEntityRecord.TargetEntityTypeId = relatedEntityTypeId.Value;
                relatedEntityRecord.TargetEntityId = relatedEntityId;

                relatedEntityRecord.PurposeKey = purposeKey;
                relatedEntityService.Add( relatedEntityRecord );
            }
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity"/> entities of the given entity type that (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="entityId">The Id of the entity you want to get the list of related entities for</param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32"/> representing the related entity type identifier.</param>
        /// <param name="purposeKey">A <see cref="System.String"/> representing the purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity"/> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedSourceOrTargetEntities( int entityId, int relatedEntityTypeId, string purposeKey )
        {
            var rockContext = this.Context as RockContext;

            var entityType = EntityTypeCache.Get( typeof( T ), false, rockContext );

            var srcQuery = new Rock.Model.RelatedEntityService( rockContext ).GetRelatedToSource( entityId, entityType.Id, relatedEntityTypeId, purposeKey );

            var tgtQuery = new Rock.Model.RelatedEntityService( rockContext ).GetRelatedToTarget( entityId, entityType.Id, relatedEntityTypeId, purposeKey );

            if ( srcQuery != null && tgtQuery != null )
            {
                return srcQuery.Union( tgtQuery );
            }
            else if ( srcQuery != null && tgtQuery == null )
            {
                return srcQuery;
            }
            else if ( srcQuery == null && tgtQuery != null )
            {
                return tgtQuery;
            }

            return null;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> target entities (related to the given source entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32" /> representing the related entity type identifier.</param>
        /// <param name="purposeKey">A <see cref="System.String" /> representing the purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedToSourceEntity( int sourceEntityId, int relatedEntityTypeId, string purposeKey )
        {
            var rockContext = this.Context as RockContext;

            var entityType = EntityTypeCache.Get( typeof( T ), false, rockContext );

            var srcQuery = new Rock.Model.RelatedEntityService( rockContext ).GetRelatedToSource( sourceEntityId, entityType.Id, relatedEntityTypeId, purposeKey );

            return srcQuery;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> source entities (related to the given target entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="targetEntityId">The target entity identifier.</param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32" /> representing the related entity type identifier.</param>
        /// <param name="purposeKey">A <see cref="System.String" /> representing the purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedToTargetEntity( int targetEntityId, int relatedEntityTypeId, string purposeKey )
        {
            var rockContext = this.Context as RockContext;

            var entityType = EntityTypeCache.Get( typeof( T ), false, rockContext );

            var tgtQuery = new Rock.Model.RelatedEntityService( rockContext ).GetRelatedToTarget( targetEntityId, entityType.Id, relatedEntityTypeId, purposeKey );

            return tgtQuery;
        }
    }

}
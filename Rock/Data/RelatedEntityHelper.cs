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
    public sealed class RelatedEntityHelper<T> where T : class, Rock.Data.IEntity, new()
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
        /// <typeparam name="TT">The type of the Related (Target) Entities.</typeparam>
        /// <param name="entityId">The (Source) entity identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns></returns>
        public IQueryable<TT> GetRelatedToSourceEntity<TT>( int entityId, string purposeKey ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var relatedEntities = GetRelatedToSourceEntity( entityId, relatedEntityTypeId.Value, purposeKey ).Cast<TT>();

            return relatedEntities;
        }

        /// <summary>
        /// Gets the <see cref="RelatedEntity.PurposeKey"/>s that are in the database where the SourceEntityType is T and has the specified Id.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>IQueryable&lt;System.String&gt;.</returns>
        public IQueryable<string> GetUsedPurposeKeys( int entityId )
        {
            var sourceEntityTypeId = EntityTypeCache.GetId<T>() ?? 0;

            var usedPurposeKeys = new RelatedEntityService( this.Context as RockContext ).Queryable()
                .Where( a => a.SourceEntityId == entityId && a.SourceEntityTypeId == sourceEntityTypeId ).Select( a => a.PurposeKey ).Distinct();

            return usedPurposeKeys;

        }

        /// <summary>
        /// Gets the related to source entity qualifier.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        public IQueryable<TT> GetRelatedToSourceEntityQualifier<TT>( int entityId, string purposeKey, string qualifierValue ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var relatedEntities = GetRelatedToSourceEntity( entityId, relatedEntityTypeId.Value, purposeKey, qualifierValue ).Cast<TT>();

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
            AddRelatedToSourceEntity<TT>( entityId, relatedEntity, purposeKey, null );
        }

        /// <summary>
        /// Adds a relationship between the relatedEntity and the entity
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntity">The related entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        public void AddRelatedToSourceEntity<TT>( int entityId, TT relatedEntity, string purposeKey, string qualifierValue ) where TT : IEntity
        {
            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();

            relatedEntityService.Add(
                new RelatedEntity
                {
                    SourceEntityTypeId = sourceEntityTypeId.Value,
                    SourceEntityId = entityId,
                    TargetEntityTypeId = relatedEntityTypeId.Value,
                    TargetEntityId = relatedEntity.Id,
                    PurposeKey = purposeKey,
                    QualifierValue = qualifierValue
                }
            );
        }

        /// <summary>
        /// Deletes the relationship between the relatedEntity and the entity. This method will delete all relationships for the two entities and purpose key regardless of the QualifierValue.
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
        /// Deletes the relationship between the relatedEntity and the entity for the given QualifierValue.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntity">The related entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        public void DeleteRelatedToSourceEntity<TT>( int entityId, TT relatedEntity, string purposeKey, string qualifierValue ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();

            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            var relatedEntityRecords = relatedEntityService.GetRelatedEntityRecordsToSource( entityId, sourceEntityTypeId.Value, relatedEntityTypeId.Value, purposeKey, qualifierValue );

            relatedEntityService.DeleteRange( relatedEntityRecords );
        }

        /// <summary>
        /// Deletes a RelatedEntity for the given source entity, target entity, purpose key, and qualifier value.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        public void DeleteTargetEntityFromSourceEntity<TT>( int entityId, TT targetEntity, string purposeKey, string qualifierValue = null ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();

            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            var relatedEntity = relatedEntityService.GetRelatedEntityRecordToSource( entityId, sourceEntityTypeId.Value, relatedEntityTypeId.Value, targetEntity.Id, purposeKey, qualifierValue );
            if ( relatedEntity != null )
            {
                relatedEntityService.Delete( relatedEntity );
            }
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
        /// Determines if relationship to the source entity already exists.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntity">The related entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        public bool RelatedToSourceEntityAlreadyExists<TT>( int entityId, TT relatedEntity, string purposeKey, string qualifierValue ) where TT : IEntity
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
                      && a.QualifierValue == qualifierValue
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
        /// Sets the related target entities for the given source entity type, purpose key, and qualifier value.
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntities">The related entities.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        public void SetRelatedToSourceEntity<TT>( int entityId, List<TT> relatedEntities, string purposeKey, string qualifierValue ) where TT : IEntity
        {
            var relatedEntityTypeId = EntityTypeCache.GetId<TT>();
            var sourceEntityTypeId = EntityTypeCache.GetId<T>();
            var currentRelatedEntities = GetRelatedToSourceEntityQualifier<TT>( entityId, purposeKey, qualifierValue ).ToList();

            var relatedEntityService = new Rock.Model.RelatedEntityService( this.Context as RockContext );
            var relatedEntityIds = relatedEntities.Select( a => a.Id ).ToList();

            // delete related entities that are no longer in the list
            foreach ( var currentRelatedEntity in currentRelatedEntities.Where( a => !relatedEntityIds.Contains( a.Id ) ) )
            {
                // get related entity record(s) that need to be deleted since the relatedEntity is no longer in the list
                var relatedEntityToDelete = relatedEntityService
                    .Queryable()
                    .Where( a => a.SourceEntityTypeId == sourceEntityTypeId.Value
                            && a.TargetEntityTypeId == relatedEntityTypeId.Value
                            && a.SourceEntityId == entityId
                            && a.TargetEntityId == currentRelatedEntity.Id
                            && a.PurposeKey == purposeKey
                            && a.QualifierValue == qualifierValue )
                    .ToList();
                relatedEntityService.DeleteRange( relatedEntityToDelete );
            }

            // add related entity record for related entities that that don't have a related entity record
            foreach ( var relatedEntityId in relatedEntityIds.Where( a => !currentRelatedEntities.Any( r => r.Id == a ) ) )
            {
                var relatedEntityRecord = new RelatedEntity
                {
                    SourceEntityTypeId = sourceEntityTypeId.Value,
                    SourceEntityId = entityId,
                    TargetEntityTypeId = relatedEntityTypeId.Value,
                    TargetEntityId = relatedEntityId,
                    PurposeKey = purposeKey,
                    QualifierValue = qualifierValue
                };

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
            return GetRelatedToSourceEntity( sourceEntityId, relatedEntityTypeId, purposeKey, null );
        }

        /// <summary>
        /// Sets the related target entities for the given source entity type, purpose key, and qualifier value. If qualifierValue then it is not used to filter the results.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="relatedEntityTypeId">The related (Target) entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetRelatedToSourceEntity( int sourceEntityId, int relatedEntityTypeId, string purposeKey, string qualifierValue )
        {
            var rockContext = this.Context as RockContext;
            var sourceEntityTypeId = EntityTypeCache.Get( typeof( T ), false, rockContext ).Id;

            return qualifierValue.IsNotNullOrWhiteSpace()
                ? new RelatedEntityService( rockContext ).GetRelatedToSource( sourceEntityId, sourceEntityTypeId, relatedEntityTypeId, purposeKey, qualifierValue )
                : new RelatedEntityService( rockContext ).GetRelatedToSource( sourceEntityId, sourceEntityTypeId, relatedEntityTypeId, purposeKey );
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
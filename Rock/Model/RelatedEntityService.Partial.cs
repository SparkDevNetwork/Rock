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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Related Entity
    /// </summary>
    public partial class RelatedEntityService
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> target entities (related to the given source entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="sourceEntityId">A <see cref="System.Int32" /> representing the source entity identifier.</param>
        /// <param name="sourceEntityTypeId">A <see cref="System.Int32" /> representing the source entity type identifier.</param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32" /> representing the related target entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedToSource( int sourceEntityId, int sourceEntityTypeId, int relatedEntityTypeId, string purposeKey )
        {
            return GetRelatedToSource( sourceEntityId, sourceEntityTypeId, relatedEntityTypeId, purposeKey, null );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> target entities for the given entity types, purpose key, and qualifier value.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="sourceEntityTypeId">The source entity type identifier.</param>
        /// <param name="relatedEntityTypeId">The related entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetRelatedToSource( int sourceEntityId, int sourceEntityTypeId, int relatedEntityTypeId, string purposeKey, string qualifierValue )
        {
            EntityTypeCache relatedEntityTypeCache = EntityTypeCache.Get( relatedEntityTypeId );
            if ( relatedEntityTypeCache.AssemblyName != null )
            {
                IQueryable<RelatedEntity> query;
                if ( qualifierValue.IsNotNullOrWhiteSpace() )
                {
                    query = GetRelatedEntityRecordsToSource( sourceEntityId, sourceEntityTypeId, relatedEntityTypeId, purposeKey, qualifierValue );
                }
                else
                {
                    query = GetRelatedEntityRecordsToSource( sourceEntityId, sourceEntityTypeId, relatedEntityTypeId, purposeKey );
                }

                var rockContext = this.Context as RockContext;
                Type relatedEntityType = relatedEntityTypeCache.GetEntityType();
                if ( relatedEntityType != null )
                {
                    Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( relatedEntityType, rockContext );
                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    entityQry = query.Join(
                        entityQry,
                        f => f.TargetEntityId,
                        e => e.Id,
                        ( f, e ) => e );

                    return entityQry;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> target entities (related to the given source entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="sourceEntityTypeId">The source entity type identifier.</param>
        /// <param name="relatedEntityTypeId">The related (target) entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns></returns>
        public IQueryable<RelatedEntity> GetRelatedEntityRecordsToSource( int sourceEntityId, int sourceEntityTypeId, int relatedEntityTypeId, string purposeKey )
        {
            var query = Queryable()
                                    .Where( a => a.SourceEntityTypeId == sourceEntityTypeId
                                        && a.SourceEntityId == sourceEntityId
                                        && a.TargetEntityTypeId == relatedEntityTypeId );

            if ( purposeKey.IsNullOrWhiteSpace() )
            {
                query = query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) );
            }
            else
            {
                query = query.Where( a => a.PurposeKey == purposeKey );
            }

            return query;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> target entities (related to the given source entity) for the given entity types, purpose key, and qualifier value.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="sourceEntityTypeId">The source entity type identifier.</param>
        /// <param name="relatedEntityTypeId">The related (target) entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        public IQueryable<RelatedEntity> GetRelatedEntityRecordsToSource( int sourceEntityId, int sourceEntityTypeId, int relatedEntityTypeId, string purposeKey, string qualifierValue )
        {
            var query = Queryable()
            .Where( a => a.SourceEntityTypeId == sourceEntityTypeId
                && a.SourceEntityId == sourceEntityId
                && a.TargetEntityTypeId == relatedEntityTypeId
                && a.QualifierValue == qualifierValue );

            query = purposeKey.IsNullOrWhiteSpace() ? query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) ) : query.Where( a => a.PurposeKey == purposeKey );

            return query;
        }

        /// <summary>
        /// Returns a RelatedEntity for the given source entity, target entity, purpose key, and qualifier value.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="sourceEntityTypeId">The source entity type identifier.</param>
        /// <param name="relatedEntityTypeId">The related (target) entity type identifier.</param>
        /// <param name="relatedEntityId">The related (target) entity identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        public RelatedEntity GetRelatedEntityRecordToSource( int sourceEntityId, int sourceEntityTypeId, int relatedEntityTypeId, int relatedEntityId, string purposeKey, string qualifierValue )
        {
            var query = Queryable()
            .Where( a => a.SourceEntityTypeId == sourceEntityTypeId
                && a.SourceEntityId == sourceEntityId
                && a.TargetEntityTypeId == relatedEntityTypeId
                && a.TargetEntityId == relatedEntityId
                && a.QualifierValue == qualifierValue );

            query = purposeKey.IsNullOrWhiteSpace() ? query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) ) : query.Where( a => a.PurposeKey == purposeKey );
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity"/> source entities (related to the given target entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="targetEntityId">A <see cref="System.Int32" /> representing the target entity identifier.</param>
        /// <param name="targetEntityTypeId">A <see cref="System.Int32" /> representing the <see cref="RelatedEntity.TargetEntityTypeId"/></param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32" /> representing the related <see cref="RelatedEntity.SourceEntityTypeId"/></param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Data.IEntity" /> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedToTarget( int targetEntityId, int targetEntityTypeId, int relatedEntityTypeId, string purposeKey )
        {
            EntityTypeCache relatedEntityTypeCache = EntityTypeCache.Get( relatedEntityTypeId );
            if ( relatedEntityTypeCache.AssemblyName != null )
            {
                var query = Queryable()
                        .Where( a => a.TargetEntityTypeId == targetEntityTypeId
                            && a.TargetEntityId == targetEntityId
                            && a.SourceEntityTypeId == relatedEntityTypeId );

                if ( purposeKey.IsNullOrWhiteSpace() )
                {
                    query = query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) );
                }
                else
                {
                    query = query.Where( a => a.PurposeKey == purposeKey );
                }

                var rockContext = this.Context as RockContext;
                Type relatedEntityType = relatedEntityTypeCache.GetEntityType();
                if ( relatedEntityType != null )
                {
                    Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( relatedEntityType, rockContext );
                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    entityQry = query.Join(
                        entityQry,
                        f => f.SourceEntityId,
                        e => e.Id,
                        ( f, e ) => e );

                    return entityQry;
                }
            }

            return null;
        }
    }
}

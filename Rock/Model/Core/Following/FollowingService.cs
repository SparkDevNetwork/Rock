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
    /// Service/Data access class for <see cref="Rock.Model.Following"/> entity objects.
    /// </summary>
    public partial class FollowingService
    {
        /// <summary>
        /// If the person is following, then removes the follow. If the person is not following, then adds. The value
        /// returned indicates if the person is now following.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns><c>true</c> if the entity is now followed; otherwise <c>false</c>.</returns>
        /// <remarks>Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.</remarks>
        public bool ToggleFollowing( int entityTypeId, int entityId, int personAliasId )
        {
            return ToggleFollowing( entityTypeId, entityId, personAliasId, string.Empty );
        }

        /// <summary>
        /// If the person is following, then removes the follow. If the person is not following, then adds. The value
        /// returned indicates if the person is now following.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="purposeKey">A purpose that defines how this following will be used.</param>
        /// <returns><c>true</c> if the entity is now followed; otherwise <c>false</c>.</returns>
        /// <remarks>Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.</remarks>
        public bool ToggleFollowing( int entityTypeId, int entityId, int personAliasId, string purposeKey )
        {
            purposeKey = purposeKey ?? string.Empty;

            var followings = Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAliasId == personAliasId &&
                    ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) )
                .ToList();

            if ( followings.Any() )
            {
                DeleteRange( followings );
                return false;
            }

            Add( new Following
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                PersonAliasId = personAliasId,
                PurposeKey = purposeKey
            } );

            return true;
        }

        /// <summary>
        /// Gets or adds a following for the specified person and entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="purposeKey">A purpose that defines how this following will be used.</param>
        /// <returns>An existing or new Following record.</returns>
        /// <remarks>Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.</remarks>
        public Following GetOrAddFollowing( int entityTypeId, int entityId, int personAliasId, string purposeKey )
        {
            purposeKey = purposeKey ?? string.Empty;

            var followings = Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAliasId == personAliasId &&
                    ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) )
                .ToList();

            if ( followings.Any() )
            {
                return followings.First();
            }

            var following = new Following
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                PersonAliasId = personAliasId,
                PurposeKey = purposeKey
            };

            Add( following );

            return following;
        }

        /// <summary>
        /// Gets the entity query
        /// For example: If the EntityTypeId is GroupMember, this will return a GroupMember query of group members that the person is following
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetFollowedItems( int entityTypeId, int personId )
        {
            return GetFollowedItems( entityTypeId, personId, string.Empty );
        }

        /// <summary>
        /// Gets the entity query
        /// For example: If the EntityTypeId is GroupMember, this will return a GroupMember query of group members that the person is following
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="purposeKey">A purpose that defines how this following will be used.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetFollowedItems( int entityTypeId, int personId, string purposeKey )
        {
            EntityTypeCache itemEntityType = EntityTypeCache.Get( entityTypeId );
            var rockContext = this.Context as RockContext;

            purposeKey = purposeKey ?? string.Empty;

            var followedItemsQry = this.Queryable()
                .Where( a => a.PersonAlias.PersonId == personId && a.EntityTypeId == entityTypeId )
                .Where( f => ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey );

            if ( itemEntityType.AssemblyName != null )
            {
                Type entityType = itemEntityType.GetEntityType();
                if ( entityType != null )
                {
                    Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, rockContext );
                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    entityQry = followedItemsQry.Join(
                        entityQry,
                        f => f.EntityId,
                        e => e.Id,
                        ( f, e ) => e );

                    int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
                    int personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;

                    // if requesting persons that the person is following, it is probably recorded as the PersonAlias records that the person is following, so get Person from that
                    if ( entityTypeId == personEntityTypeId )
                    {
                        var followedItemsPersonAliasQry = this.Queryable().Where( a => a.PersonAlias.PersonId == personId && a.EntityTypeId == personAliasEntityTypeId );
                        var entityPersonAliasQry = new PersonAliasService( rockContext ).Queryable();

                        var entityFollowedPersons = followedItemsPersonAliasQry.Join(
                            entityPersonAliasQry,
                            f => f.EntityId,
                            e => e.Id,
                            ( f, e ) => e ).Select( a => a.Person );

                        entityQry = entityQry.Union( entityFollowedPersons as IQueryable<IEntity> );
                    }

                    return entityQry;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the by entity and person.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Following> GetByEntityAndPerson( int entityTypeId, int entityId, int personId )
        {
            return this.Queryable()
                .Where( f => f.EntityTypeId == entityTypeId
                    && f.PersonAlias.PersonId == personId
                    && f.EntityId == entityId );
        }

        /// <summary>
        /// Returns a queryable of followers for the specified entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>IQueryable&lt;Following&gt;.</returns>
        public IQueryable<Following> GetByEntity( IEntity entity )
        {
            return GetByEntity( entity, null );
        }

        /// <summary>
        /// Returns a queryable of followers for the specified entity and purpose key
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns>IQueryable&lt;Following&gt;.</returns>
        public IQueryable<Following> GetByEntity( IEntity entity, string purposeKey )
        {
            if ( entity == null )
            {
                return this.Queryable().Where( a => false );
            }

            var entityTypeId = EntityTypeCache.GetId( entity.GetType() );
            if ( entityTypeId == null )
            {
                return this.Queryable().Where( a => false );
            }

            var entityId = entity.Id;

            var qry = this.Queryable().Where( f => f.EntityTypeId == entityTypeId && f.EntityId == entityId );

            if ( purposeKey.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( a => a.PurposeKey == purposeKey );
            }

            return qry;
        }
    }
}

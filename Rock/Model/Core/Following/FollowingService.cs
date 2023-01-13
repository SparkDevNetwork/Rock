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
        /// <remarks>
        /// Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.
        /// If toggling the following of a Person, use the specific <see cref="TogglePersonFollowing"/> method instead.
        /// </remarks>
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
        /// <remarks>
        /// Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.
        /// If toggling the following of a Person, use the specific <see cref="TogglePersonFollowing"/> method instead.
        /// </remarks>
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
        /// <remarks>
        /// Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.
        /// If adding or retrieving a Person following, use the specific <see cref="GetOrAddPersonFollowing"/> method instead.
        /// </remarks>
        public Following GetOrAddFollowing( int entityTypeId, int entityId, int personAliasId, string purposeKey )
        {
            purposeKey = purposeKey ?? string.Empty;

            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            int personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;

            Following following;
            if ( entityTypeId == personEntityTypeId )
            {
                // Followings for a Person entity are processed as a special case.
                // They are stored as a following for the PersonAlias associated with the Person, to ensure that they survive merge operations.
                var personAliasService = new PersonAliasService( (RockContext)this.Context );
                var followedPersonAliasId = personAliasService.Queryable()
                    .Where( p => p.AliasPersonId == entityId )
                    .Select( p => p.Id )
                    .FirstOrDefault();

                following = Queryable()
                    .FirstOrDefault( f =>
                        f.EntityTypeId == personAliasEntityTypeId
                        && f.EntityId == followedPersonAliasId
                        && f.PersonAliasId == personAliasId
                        && ( ( string.IsNullOrEmpty( f.PurposeKey ) && string.IsNullOrEmpty( purposeKey ) ) || f.PurposeKey == purposeKey ) );

                entityTypeId = personAliasEntityTypeId;
                entityId = followedPersonAliasId;
            }
            else
            {
                following = Queryable()
                    .FirstOrDefault( f =>
                        f.EntityTypeId == entityTypeId &&
                        f.EntityId == entityId &&
                        f.PersonAliasId == personAliasId &&
                        ( ( string.IsNullOrEmpty( f.PurposeKey ) && string.IsNullOrEmpty( purposeKey ) ) || f.PurposeKey == purposeKey ) );
            }

            if ( following != null )
            {
                return following;
            }

            following = new Following
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                PersonAliasId = personAliasId
            };

            // Avoid adding an empty string for PurposeKey.
            if ( !string.IsNullOrEmpty( purposeKey ) )
            {
                following.PurposeKey = purposeKey;
            }

            Add( following );

            return following;
        }

        /// <summary>
        /// Gets the entity query
        /// For example: If the EntityTypeId is GroupMember, this will return a GroupMember query of group members that the person is following
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns>
        /// If retrieving Person followings, use the specific <see cref="GetFollowedPersonItems(int)"/> method instead.
        /// </returns>
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
        /// <param name="purposeKey">
        /// A key that identifies the specific purpose of the following.
        /// If this parameter is not specified, all followings without a defined purpose key will be returned;
        /// this includes most followings, such as user favourites and followed people.
        /// </param>
        /// <returns>If retrieving Person followings, use the specific <see cref="GetFollowedPersonItems(int,string)"/> method instead.</returns>
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

                    // If the request is for followed people, the followed entity may be either a Person or PersonAlias.
                    // In that case, we need to combine the followings for both entity types.
                    if ( entityTypeId == personEntityTypeId )
                    {
                        // The followed items are a set of Person records, so add the Person records
                        // associated with any followed PersonAlias records.
                        var followedItemsPersonAliasQry = this.Queryable().Where( a => a.PersonAlias.PersonId == personId && a.EntityTypeId == personAliasEntityTypeId );
                        var entityPersonAliasQry = new PersonAliasService( rockContext ).Queryable();

                        var entityFollowedPersons = followedItemsPersonAliasQry.Join(
                            entityPersonAliasQry,
                            f => f.EntityId,
                            e => e.Id,
                            ( f, e ) => e ).Select( a => a.Person );

                        entityQry = entityQry.Union( entityFollowedPersons );
                    }
                    else if ( entityTypeId == personAliasEntityTypeId )
                    {
                        // The followed items are a set of PersonAlias records, so add the PersonAlias records
                        // associated with any followed Person records.
                        var followedItemsPersonQry = this.Queryable()
                            .Where( a => a.PersonAlias.PersonId == personId && a.EntityTypeId == personEntityTypeId );
                        var entityPersonAliasQry = new PersonAliasService( rockContext ).Queryable();

                        var entityFollowedPersonAliases = followedItemsPersonQry.Join(
                            entityPersonAliasQry,
                            f => f.EntityId,
                            e => e.PersonId,
                            ( f, e ) => e ).Select( pa => pa );

                        entityQry = entityQry.Union( entityFollowedPersonAliases );
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

        #region Person Followings

        /*
         * Methods to manage the following of Person entities are defined separately here.
         * A Person following is internally recorded as the following of a PersonAlias entity to ensure that it persists
         * even if the target person is merged to another record.
         * These methods exist to manage this necessary implementation detail.
         */

        /// <summary>
        /// Get the list of people followed by the specified person.
        /// </summary>
        /// <param name="followerPersonAliasId">The identifier of the PersonAlias for the follower.</param>
        /// <returns></returns>
        public IQueryable<Person> GetFollowedPersonItems( int followerPersonAliasId )
        {
            return GetFollowedPersonItems( followerPersonAliasId, null );
        }

        /// <summary>
        /// Get the list of people followed by the specified person.
        /// </summary>
        /// <param name="followerPersonAliasId">The identifier of the PersonAlias for the follower.</param>
        /// <param name="purposeKey"></param>
        /// <returns></returns>
        public IQueryable<Person> GetFollowedPersonItems( int followerPersonAliasId, string purposeKey )
        {
            var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;

            var rockContext = this.Context as RockContext;

            purposeKey = purposeKey ?? string.Empty;

            // Get Followings for PersonAlias entities.
            var personAliasService = new PersonAliasService( rockContext );
            var personAliasQry = personAliasService.Queryable();

            var followedPersonAliasQry = this.Queryable()
                .Where( f => f.PersonAlias.Id == followerPersonAliasId
                        && ( f.EntityTypeId == personAliasEntityTypeId )
                        && ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) )
                .Join( personAliasQry,
                    f => f.EntityId,
                    e => e.Id,
                    ( f, e ) => e )
                .Select( a => a.Person );

            // Add Followings for Person entities.
            // Followings where the target is a person should be recorded as a following of the associated PersonAlias entity,
            // but we include Followings for Person entities here in case they have been recorded incorrectly by some other means.
            var personService = new PersonService( rockContext );
            var personQry = personService.Queryable();

            var followedPersonQry = this.Queryable()
                .Where( f => f.PersonAlias.Id == followerPersonAliasId
                        && ( f.EntityTypeId == personEntityTypeId )
                        && ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) )
                .Join( personQry,
                    f => f.EntityId,
                    e => e.Id,
                    ( f, e ) => e );

            var resultQry = followedPersonAliasQry.Union( followedPersonQry );

            return resultQry;
        }

        /// <summary>
        /// Gets or adds a following for the specified person.
        /// </summary>
        /// <param name="followedPersonAliasId">The identifier of the PersonAlias for the person being followed.</param>
        /// <param name="followerPersonAliasId">The identifier of the PersonAlias for the follower.</param>
        /// <param name="purposeKey">A purpose that defines how this following will be used.</param>
        /// <returns>An existing or new Following record.</returns>
        /// <remarks>Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.</remarks>
        public Following GetOrAddPersonFollowing( int followedPersonAliasId, int followerPersonAliasId, string purposeKey )
        {
            var personAliasEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.PersonAlias ) ).GetValueOrDefault();

            var following = GetOrAddFollowing( personAliasEntityTypeId, followedPersonAliasId, followerPersonAliasId, purposeKey );
            return following;
        }

        /// <summary>
        /// If the person is following, then removes the follow. If the person is not following, then adds. The value
        /// returned indicates if the person is now following.
        /// </summary>
        /// <param name="followedPersonAliasId">The identifier of the PersonAlias for the person being followed.</param>
        /// <param name="followerPersonAliasId">The identifier of the PersonAlias for the follower.</param>
        /// <returns><c>true</c> if the entity is now followed; otherwise <c>false</c>.</returns>
        /// <remarks>Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.</remarks>
        public bool TogglePersonFollowing( int followedPersonAliasId, int followerPersonAliasId )
        {
            var personAliasEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.PersonAlias ) ).GetValueOrDefault();

            var isFollowed = ToggleFollowing( personAliasEntityTypeId, followedPersonAliasId, followerPersonAliasId, string.Empty );
            return isFollowed;
        }

        #endregion
    }
}

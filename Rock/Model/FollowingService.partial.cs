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
    /// Following POCO Service class
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
        public bool ToggleFollowing( int entityTypeId, int entityId, int personAliasId )
        {
            var followings = Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAliasId == personAliasId )
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
                PersonAliasId = personAliasId
            } );

            return true;
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
            EntityTypeCache itemEntityType = EntityTypeCache.Get( entityTypeId );
            var rockContext = this.Context as RockContext;
            var followedItemsQry = this.Queryable().Where( a => a.PersonAlias.PersonId == personId && a.EntityTypeId == entityTypeId );

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
    }
}

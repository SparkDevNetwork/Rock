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
                    Type[] modelType = { entityType };
                    Type genericServiceType = typeof( Rock.Data.Service<> );
                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                    Rock.Data.IService serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;

                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    entityQry = followedItemsQry.Join(
                        entityQry,
                        f => f.EntityId,
                        e => e.Id,
                        ( f, e ) => e );

                    return entityQry;
                }
            }

            return null;
        }
    }
}

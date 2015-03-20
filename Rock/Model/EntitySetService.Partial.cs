// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// Gets the entity query
        /// For example: If the EntitySet.EntityType is Person, this will return a Person Query of the items in this set
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetEntityQuery( int entitySetId )
        {
            var rockContext = this.Context as RockContext;
            
            var entitySet = this.Get( entitySetId );
            EntityTypeCache itemEntityType = EntityTypeCache.Read( entitySet.EntityTypeId ?? 0 );

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

                    var joinQry = entityItemQry.Join( entityQry, k => k.EntityId, i => i.Id, ( setItem, item ) => item );

                    return joinQry;
                }
            }

            return null;
        }
    }
}

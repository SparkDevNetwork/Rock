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
using System.Net;
using System.Security.AccessControl;

using Microsoft.AspNetCore.Mvc;

using Rock.Data;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Rest.Utilities;
using Rock.Web.Cache;

#if WEBFORMS
using HttpDeleteAttribute = System.Web.Http.HttpDeleteAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#endif

namespace Rock.Rest.v2
{
    public partial class UtilitiesController : ApiControllerBase
    {
        #region Person Prefernce API Methods

        /// <summary>
        /// Retrieves all context entities for the browser session.
        /// </summary>
        /// <returns>A collection of context entity values.</returns>
        [HttpGet]
        [Route( "ContextEntities" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( List<ContextEntityItemBag> ) )]
        [Rock.SystemGuid.RestActionGuid( "4269b4f4-881d-4139-a661-8d467222ea9d" )]
        public IActionResult GetContextEntities()
        {
            var contextEntities = new List<ContextEntityItemBag>();

            using ( var rockContext = new RockContext() )
            {
                foreach ( var entityType in RockRequestContext.GetContextEntityTypes() )
                {
                    var entity = RockRequestContext.GetContextEntity( entityType );
                    var cacheType = EntityTypeCache.Get( entityType );

                    if ( entity != null && cacheType != null )
                    {
                        contextEntities.Add( new ContextEntityItemBag
                        {
                            EntityType = cacheType.Name,
                            EntityTypeGuid = cacheType.Guid,
                            EntityGuid = entity.Guid,
                            EntityName = entity.ToString()
                        } );
                    }
                }
            }

            return Ok( contextEntities );
        }

        /// <summary>
        /// Sets the context entity of the specified entity type to the entity
        /// represented by <paramref name="entityKey"/>.
        /// </summary>
        /// <param name="entityTypeKey">The entity type unique identifier, encoded identifier, or the C# full class name.</param>
        /// <param name="entityKey">The entity unique identifier or the encoded identifier.</param>
        /// <param name="pageKey">The page unique identifier, or encoded identifier if this should be set as a page specific context value.</param>
        /// <returns>A response that indicates if the context was set.</returns>
        [HttpPost]
        [Route( "ContextEntities/{entityTypeKey}/{entityKey}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( ContextEntityItemBag ), Description = "The context entity was set." )]
        [ProducesResponseType( HttpStatusCode.BadRequest, Description = "One of the values provided was not valid." )]
        [Rock.SystemGuid.RestActionGuid( "002e3602-fc1a-4bba-915e-bd7e344cceb2" )]
        public IActionResult PostContextEntity( string entityTypeKey, string entityKey, string pageKey = null )
        {
            using ( var rockContext = new RockContext() )
            {
                EntityTypeCache entityType;

                if ( Guid.TryParse( entityTypeKey, out var entityTypeGuid ) )
                {
                    entityType = EntityTypeCache.Get( entityTypeGuid, rockContext );
                }
                else if ( IdHasher.Instance.TryGetId( entityTypeKey, out var entityTypeId ) )
                {
                    entityType = EntityTypeCache.Get( entityTypeId, rockContext );
                }
                else
                {
                    entityType = EntityTypeCache.Get( entityTypeKey, false, rockContext );
                }

                if ( entityType == null )
                {
                    return BadRequest( $"The entity type '{entityTypeKey}' could not be found." );
                }

                PageCache page = null;

                if ( pageKey.IsNotNullOrWhiteSpace() )
                {
                    page = PageCache.Get( pageKey, false );

                    if ( page == null )
                    {
                        return BadRequest( $"The page '{pageKey}' could not be found." );
                    }
                }

                var entity = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), entityKey, false, rockContext );

                if ( entity == null )
                {
                    return BadRequest( $"The entity '{entityKey}' could not be found." );
                }

                RockRequestContext.SetContextEntity( entity, page );

                return Ok( new ContextEntityItemBag
                {
                    EntityType = entityType.Name,
                    EntityTypeGuid = entityType.Guid,
                    EntityGuid = entity.Guid,
                    EntityName = entity.ToString()
                } );
            }
        }

        /// <summary>
        /// Deletes the context entity of the specified entity type.
        /// </summary>
        /// <param name="entityTypeKey">The entity type unique identifier, encoded identifier, or the C# full class name.</param>
        /// <param name="pageKey">The page unique identifier, or encoded identifier if this should be set as a page specific context value.</param>
        /// <returns>A response that indicates if the context was set.</returns>
        [HttpDelete]
        [Route( "ContextEntities/{entityTypeKey}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.NoContent, Description = "The context entity was removed." )]
        [ProducesResponseType( HttpStatusCode.BadRequest, Description = "One of the values provided was not valid." )]
        [Rock.SystemGuid.RestActionGuid( "b7945401-be95-47ab-946a-acc7b5b7dab0" )]
        public IActionResult DeleteContextEntity( string entityTypeKey, string pageKey = null )
        {
            using ( var rockContext = new RockContext() )
            {
                EntityTypeCache entityType;

                if ( Guid.TryParse( entityTypeKey, out var entityTypeGuid ) )
                {
                    entityType = EntityTypeCache.Get( entityTypeGuid, rockContext );
                }
                else if ( IdHasher.Instance.TryGetId( entityTypeKey, out var entityTypeId ) )
                {
                    entityType = EntityTypeCache.Get( entityTypeId, rockContext );
                }
                else
                {
                    entityType = EntityTypeCache.Get( entityTypeKey, false, rockContext );
                }

                if ( entityType == null )
                {
                    return BadRequest( $"The entity type '{entityTypeKey}' could not be found." );
                }

                PageCache page = null;

                if ( pageKey.IsNotNullOrWhiteSpace() )
                {
                    page = PageCache.Get( pageKey, false );

                    if ( page == null )
                    {
                        return BadRequest( $"The page '{pageKey}' could not be found." );
                    }
                }

                RockRequestContext.RemoveContextEntity( entityType.GetEntityType(), page );

                return NoContent();
            }
        }

        #endregion
    }
}

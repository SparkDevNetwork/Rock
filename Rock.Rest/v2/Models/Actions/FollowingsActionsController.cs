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
using System.Net;

using Microsoft.AspNetCore.Mvc;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.ViewModels.Rest.Models;
using Rock.Security;


#if WEBFORMS
using IActionResult = System.Web.Http.IHttpActionResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
#endif

namespace Rock.Rest.v2.Models.Actions
{
    /// <summary>
    /// Provides action API endpoints for Followings.
    /// </summary>
    [RoutePrefix( "api/v2/models/followings/actions" )]
    [Rock.SystemGuid.RestControllerGuid( "27c9a2aa-70a3-4afe-9599-aeda2db80794" )]
    public class FollowingsActionsController : ApiControllerBase
    {
        /// <summary>
        /// Gets the identifiers of all items being followed by the current
        /// person for the specified entity type.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier as either an Id, Guid, IdKey value or name.</param>
        /// <returns>The identifiers of the items being followed.</returns>
        [HttpGet]
        [Route( "followed/{entityTypeId}" )]
        [Authenticate]
        [Secured( Security.Authorization.VIEW )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( List<ItemIdentifierBag> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "802af5c2-c880-42d4-8043-33a43ad27965" )]
        public IActionResult GetFollowed( string entityTypeId )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityType = EntityCache<EntityTypeCache, EntityType>.Get( entityTypeId, true );

                // If we couldn't find the entity type by normal Id/Guid/IdKey
                // lookup, then see if we can find it by entity type name.
                // This is not normally done, but makes sense for friendly entity
                // type lookups.
                if ( entityType == null )
                {
                    entityType = EntityTypeCache.All( rockContext )
                        .Where( et => et.Name?.Equals( entityTypeId, StringComparison.OrdinalIgnoreCase ) == true
                            || et.FriendlyName?.Equals( entityTypeId, StringComparison.OrdinalIgnoreCase ) == true )
                        .FirstOrDefault();
                }

                if ( entityType?.GetEntityType() == null )
                {
                    return NotFound( "The entity type was not found." );
                }

                // If the person isn't logged in then they can't have
                // anything followed.
                if ( RockRequestContext.CurrentPerson == null )
                {
                    return BadRequest( "Must be logged in." );
                }

                var followingIdsQry = new FollowingService( rockContext ).Queryable()
                    .Where( f => f.EntityTypeId == entityType.Id
                        && string.IsNullOrEmpty( f.PurposeKey )
                        && f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id )
                    .Select( f => f.EntityId );

                var entityQry = Reflection.GetQueryableForEntityType( entityType.GetEntityType(), rockContext )
                    .Where(  a => followingIdsQry.Contains( a.Id ) );

                // We don't need to check item security because we are only
                // returning identifiers and not full entity objects. If they
                // request the full object from the CRUD endpoint it will
                // handle the security check.
                //
                // NOTE: In the future we may add the "ToString()" value to the
                // returned type so they have the common name to work with as
                // it was decided that would likely be acceptable risk.
                var results = entityQry
                    .Select( a => new
                    {
                        a.Id,
                        a.Guid
                    } )
                    .ToList()
                    .Select( a => new ItemIdentifierBag
                    {
                        Id = a.Id,
                        Guid = a.Guid,
                        IdKey = IdHasher.Instance.GetHash( a.Id )
                    } )
                    .ToList();

                return Ok( results );
            }
        }
    }
}

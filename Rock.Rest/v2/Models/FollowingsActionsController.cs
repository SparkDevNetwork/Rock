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
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.ViewModels.Rest.Models;

#if WEBFORMS
using IActionResult = System.Web.Http.IHttpActionResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
#endif

namespace Rock.Rest.v2.Models
{
    /// <summary>
    /// Provides action API endpoints for Followings.
    /// </summary>
    [RoutePrefix( "api/v2/models/followings/actions" )]
    [SecurityAction( "UnrestrictedView", "Allows viewing entities regardless of per-entity security authorization." )]
    [SecurityAction( "UnrestrictedEdit", "Allows editing entities regardless of per-entity security authorization." )]
    [Rock.SystemGuid.RestControllerGuid( "27c9a2aa-70a3-4afe-9599-aeda2db80794" )]
    public class FollowingsActionsController : ApiControllerBase
    {
        /// <summary>
        /// Gets the identifiers of all items being followed by the current
        /// person for the specified entity type.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier as either an Id, Guid, IdKey value or name.</param>
        /// <returns>The action result.</returns>
        [HttpGet]
        [Authenticate]
        [Secured]
        [Route( "followed/{entityTypeId}" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( List<object> ) )]
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

                // If the person isn't logged in, then can't have anything followed.
                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Ok( new List<object>() );
                }

                var followingIdsQry = new FollowingService( rockContext ).Queryable()
                    .Where( f => f.EntityTypeId == entityType.Id
                        && string.IsNullOrEmpty( f.PurposeKey )
                        && f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id )
                    .Select( f => f.EntityId );

                var entityQry = Reflection.GetQueryableForEntityType( entityType.GetEntityType(), rockContext )
                    .Where(  a => followingIdsQry.Contains( a.Id ) );

                List<ItemIdentifierBag> results;

                // See if we can bypass the per-entity security checks.
                if ( IsCurrentPersonUnrestrictedView() || !entityType.IsSecured )
                {
                    results = entityQry
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
                else
                {
                    var currentPerson = RockRequestContext.CurrentPerson;

                    results = entityQry
                        .ToList()
                        .Where( a => ( ( ISecured ) a ).IsAuthorized( Security.Authorization.VIEW, currentPerson ) )
                        .Select( a => new ItemIdentifierBag
                        {
                            Id = a.Id,
                            Guid = a.Guid,
                            IdKey = a.IdKey
                        } )
                        .ToList();
                }

                return Ok( results );
            }
        }
    }
}

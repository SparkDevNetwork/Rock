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

#if WEBFORMS
using System.Data.Entity;

using IActionResult = System.Web.Http.IHttpActionResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
#endif

namespace Rock.Rest.v2.Models
{
    /// <summary>
    /// Provides action API endpoints for DataViews.
    /// </summary>
    [RoutePrefix( "api/v2/models/dataviews/actions" )]
    [SecurityAction( "UnrestrictedView", "Allows viewing entities regardless of per-entity security authorization." )]
    [SecurityAction( "UnrestrictedEdit", "Allows editing entities regardless of per-entity security authorization." )]
    [Rock.SystemGuid.RestControllerGuid( "4a4d3972-248d-4482-bf99-6e0719ab122f" )]
    public class DataViewsActionsController : ApiControllerBase
    {
        /// <summary>
        /// Checks if a dataview contains the specified item.
        /// </summary>
        /// <param name="id">The data view identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="entityId">The entity identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>The action result.</returns>
        [HttpGet]
        [Authenticate]
        [Secured]
        [Route( "contains/{id}/{entityId}" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( bool ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "348856c3-478e-4da8-b7d9-e0f47d254376" )]
        public IActionResult GetContains( string id, string entityId )
        {
            using ( var rockContext = new RockContext() )
            {
                var dataView = new DataViewService( rockContext ).Get( id );

                // Verify the data view exists and the person has access to it.
                if ( dataView == null )
                {
                    return NotFound( "The data view was not found." );
                }

                if ( !dataView.IsAuthorized( Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) && !IsCurrentPersonUnrestrictedView() )
                {
                    return Unauthorized( $"You are not authorized to view this data view." );
                }

                var qry = dataView.GetQuery();

                // Filter to the specified entityId.
                if ( int.TryParse( entityId, out var intId ) || IdHasher.Instance.TryGetId( entityId, out intId ) )
                {
                    qry = qry.Where( a => a.Id == intId );
                }
                else if ( Guid.TryParse( entityId, out var guidId ) )
                {
                    qry = qry.Where( a => a.Guid == guidId );
                }
                else
                {
                    return BadRequest( "Invalid entity identifier specified." );
                }

                return Ok( qry.Any() );
            }
        }

        /// <summary>
        /// Gets the items that are contained in the data view.
        /// </summary>
        /// <param name="id">The data view identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>The action result.</returns>
        [HttpGet]
        [Authenticate]
        [Secured]
        [Route( "contents/{id}" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( List<object> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "295c3416-34b2-4be7-bb4f-c0ee3c38b86c" )]
        public IActionResult GetContents( string id )
        {
            using ( var rockContext = new RockContext() )
            {
                var dataView = new DataViewService( rockContext ).Get( id );

                // Verify the data view exists and the person has access to it.
                if ( dataView == null )
                {
                    return NotFound( "The data view was not found." );
                }

                if ( !dataView.IsAuthorized( Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) && !IsCurrentPersonUnrestrictedView() )
                {
                    return Unauthorized( $"You are not authorized to view this data view." );
                }

                // Get the results from the data view.
                var items = dataView.GetQuery().AsNoTracking().ToList();

                // Check if the entity supports security.
                var entityType = dataView.EntityTypeId.HasValue
                    ? EntityTypeCache.Get( dataView.EntityTypeId.Value )?.GetEntityType()
                    : null;
                var hasSecurity = entityType != null && typeof( ISecured ).IsAssignableFrom( entityType );

                // If the current person has unrestricted view access then we
                // don't need to run security checks on the items.
                if ( !hasSecurity || IsCurrentPersonUnrestrictedView() )
                {
                    return Ok( items );
                }

                var filteredItems = new List<IEntity>( items.Count );

                foreach ( var item in items )
                {
                    if ( ( item as ISecured ).IsAuthorized( Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                    {
                        filteredItems.Add( item );
                    }
                }

                return Ok( filteredItems );
            }
        }
    }
}

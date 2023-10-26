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
using System.Net;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Rest.Models.Workflows;
using Rock.Web.Cache;

#if WEBFORMS
using FromBody = System.Web.Http.FromBodyAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest.v2.Models
{
    /// <summary>
    /// Provides action API endpoints for Workflows.
    /// </summary>
    [RoutePrefix( "api/v2/models/workflows/actions" )]
    [SecurityAction( "UnrestrictedView", "Allows viewing entities regardless of per-entity security authorization." )]
    [SecurityAction( "UnrestrictedEdit", "Allows editing entities regardless of per-entity security authorization." )]
    [Rock.SystemGuid.RestControllerGuid( "cf6d6938-40df-446c-8f91-cb45beef7357" )]
    public class WorkflowsActionsController : ApiControllerBase
    {
        /// <summary>
        /// Launches a new workflow of the specified type. The workflow will be
        /// queued up in Rock to be launched shortly after the API call has
        /// completed.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="request">The details describing how the workflow should be launched.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [Authenticate]
        [Secured]
        [Route( "launch/{workflowTypeId}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "44fbcc9f-f6fa-43ff-adef-c7be0fb196fa" )]
        public IActionResult PostLaunch( string workflowTypeId, [FromBody] LaunchWorkflowBag request )
        {
            // Either both entity type id and entity id must be specified or
            // both must not be specified.
            if ( ( request?.EntityTypeId.IsNotNullOrWhiteSpace() ?? false ) != ( request?.EntityId.IsNotNullOrWhiteSpace() ?? false ) )
            {
                return BadRequest( $"If either {nameof( request.EntityTypeId )} or {nameof( request.EntityId )} are specified then both must be specified." );
            }

            using ( var rockContext = new RockContext() )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeId, true );

                // Verify the workflow type exists and the person has access to it.
                if ( workflowType == null )
                {
                    return NotFound( "The workflow type was not found." );
                }

                if ( !workflowType.IsAuthorized( Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) && !IsCurrentPersonUnrestrictedEdit() )
                {
                    return Unauthorized( $"You are not authorized to launch workflows of this type." );
                }

                if ( request?.EntityTypeId.IsNotNullOrWhiteSpace() == true )
                {
                    var entityType = EntityCache<EntityTypeCache, EntityType>.Get( request.EntityTypeId, true );
                    IEntity entity = null;

                    // If we couldn't find the entity type by normal Id/Guid/IdKey
                    // lookup, then see if we can find it by entity type name.
                    // This is not normally done, but it would be common for a
                    // remote service to want to launch a workflow with a person.
                    // They know the person id, but not necessarily the entity
                    // type identifier.
                    if ( entityType == null )
                    {
                        entityType = EntityTypeCache.All( rockContext )
                            .Where( et => et.Name?.Equals( request.EntityTypeId, StringComparison.OrdinalIgnoreCase ) == true
                                || et.FriendlyName?.Equals( request.EntityTypeId, StringComparison.OrdinalIgnoreCase ) == true )
                            .FirstOrDefault();
                    }

                    // If the entity type is fully known then attempt to load
                    // the entity.
                    if ( entityType?.GetEntityType() != null )
                    {
                        entity = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), request.EntityId, rockContext );
                    }

                    if ( entity == null && request?.IsEntityRequired == true )
                    {
                        return NotFound( "Entity was not found." );
                    }

                    if ( entity is ISecured secured && !secured.IsAuthorized( Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) && !IsCurrentPersonUnrestrictedEdit() )
                    {
                        return Unauthorized( $"You are not authorized to view this entity." );
                    }

                    Rock.Transactions.LaunchWorkflowTransaction transaction;

                    // If we have an entity then we need to create an instance of the
                    // generic LaunchWorkflowTransaction<> class, otherwise we can use
                    // the non-generic one.
                    if ( entity != null )
                    {
                        var type = entity.GetType();

                        if ( type.IsDynamicProxyType() )
                        {
                            type = type.BaseType;
                        }

                        var transactionType = typeof( Rock.Transactions.LaunchWorkflowTransaction<> ).MakeGenericType( type );

                        transaction = ( Rock.Transactions.LaunchWorkflowTransaction ) Activator.CreateInstance( transactionType, workflowType.Guid, entity.Id );
                    }
                    else
                    {
                        transaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowType.Guid, request?.Name );
                    }

                    transaction.WorkflowName = request?.Name;

                    if ( request?.AttributeValues != null )
                    {
                        transaction.WorkflowAttributeValues = request.AttributeValues;
                    }

                    transaction.Enqueue();
                }

                return NoContent();
            }
        }
    }
}

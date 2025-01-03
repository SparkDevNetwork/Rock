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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

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
    [Rock.SystemGuid.RestControllerGuid( "cf6d6938-40df-446c-8f91-cb45beef7357" )]
    public class WorkflowsActionsController : ApiControllerBase
    {
        /// <summary>
        /// Launches a new workflow of the specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the Wait or Immediate options are specified then the workflow will
        /// be launched immediately. Otherwise the workflow will be queued up to
        /// be launched in the background.
        ///</para>
        ///<para>
        /// Workflow Type security will be checked for View permissions unless
        /// Unrestricted Edit permissions are granted to the current person.
        /// </para>
        /// <para>
        /// If entity is provided and supports security, then it will be checked
        /// for View permission unless Unrestricted Edit permissions are
        /// granted to the current person.
        /// </para>
        /// </remarks>
        /// <param name="workflowTypeId">The workflow type identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="request">The details describing how the workflow should be launched.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [Authenticate]
        [Secured( Security.Authorization.EDIT )]
        [SecurityAction( Security.Authorization.UNRESTRICTED_EDIT, "Allows launching any workflow type regardless of per-workflow type security authorization." )]
        [ExcludeSecurityActions( Security.Authorization.VIEW )]
        [Route( "launch/{workflowTypeId}" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( LaunchWorkflowResponseBag ), Description = "Returned when the request specifies that the workflow should be awaited." )]
        [ProducesResponseType( HttpStatusCode.NoContent, Description = "Returned when the workflow is launched in the background." )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "44fbcc9f-f6fa-43ff-adef-c7be0fb196fa" )]
        public IActionResult PostLaunch( string workflowTypeId, [FromBody] LaunchWorkflowOptionsBag request )
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

                if ( workflowType.IsActive == false )
                {
                    return BadRequest( "The workflow type is not active." );
                }

                IEntity entity = null;

                if ( request?.EntityTypeId.IsNotNullOrWhiteSpace() == true )
                {
                    var entityType = EntityCache<EntityTypeCache, EntityType>.Get( request.EntityTypeId, true );

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
                }

                // If Immediate or Wait was specified then launch the workflow
                // right now.
                if ( request.Wait == true )
                {
                    var responseBag = LaunchWorkflowNow( workflowType, entity, request, RockRequestContext.CurrentPerson?.PrimaryAliasId );

                    return Ok( responseBag );
                }
                else if ( request?.Immediate == true )
                {
                    Task.Run( () =>
                    {
                        try
                        {
                            LaunchWorkflowNow( workflowType, entity, request, RockRequestContext.CurrentPerson?.PrimaryAliasId );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                        }
                    } );

                    return NoContent();
                }
                else
                {
                    LaunchWorkflowInBackground( workflowType, entity, request, RockRequestContext.CurrentPerson?.PrimaryAliasId );

                    return NoContent();
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Launches the workflow and waits until it has finished processing
        /// before returning.
        /// </summary>
        /// <param name="workflowType">The type of workflow to launch.</param>
        /// <param name="entity">The entity to be passed to the workflow.</param>
        /// <param name="request">The options that describe the original request.</param>
        /// <param name="initiatorPersonAliasId">The person alias identifier of the person that is currently logged in.</param>
        /// <returns>A response bag that describes the results of the workflow.</returns>
        private static LaunchWorkflowResponseBag LaunchWorkflowNow( WorkflowTypeCache workflowType, IEntity entity, LaunchWorkflowOptionsBag request, int? initiatorPersonAliasId )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, request?.Name, rockContext );
                workflow.InitiatorPersonAliasId = initiatorPersonAliasId;

                if ( request?.AttributeValues != null )
                {
                    foreach ( var keyVal in request.AttributeValues )
                    {
                        workflow.SetAttributeValue( keyVal.Key, keyVal.Value );
                    }
                }

                new WorkflowService( rockContext ).Process( workflow, entity, out var workflowErrors );

                return new LaunchWorkflowResponseBag
                {
                    Id = workflow.Id,
                    Guid = workflow.Guid,
                    IdKey = workflow.Id != 0 ? workflow.IdKey : null,
                    IsActive = workflow.IsActive,
                    Status = workflow.Status,
                    Errors = workflowErrors ?? new List<string>()
                };
            }
        }

        /// <summary>
        /// Queues the workflow to be launched at some point in the future.
        /// </summary>
        /// <param name="workflowType">The type of workflow to launch.</param>
        /// <param name="entity">The entity to be passed to the workflow.</param>
        /// <param name="request">The options that describe the original request.</param>
        /// <param name="initiatorPersonAliasId">The person alias identifier of the person that is currently logged in.</param>
        private static void LaunchWorkflowInBackground( WorkflowTypeCache workflowType, IEntity entity, LaunchWorkflowOptionsBag request, int? initiatorPersonAliasId )
        {
            Transactions.LaunchWorkflowTransaction transaction;

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

                var transactionType = typeof( Transactions.LaunchWorkflowTransaction<> ).MakeGenericType( type );

                transaction = ( Transactions.LaunchWorkflowTransaction ) Activator.CreateInstance( transactionType, workflowType.Guid, entity.Id );
            }
            else
            {
                transaction = new Transactions.LaunchWorkflowTransaction( workflowType.Guid, request?.Name );
            }

            transaction.WorkflowName = request?.Name;
            transaction.InitiatorPersonAliasId = initiatorPersonAliasId;

            if ( request?.AttributeValues != null )
            {
                transaction.WorkflowAttributeValues = request.AttributeValues;
            }

            transaction.Enqueue();
        }

        #endregion
    }
}

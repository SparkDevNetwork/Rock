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
using System.Web.Http;

using Microsoft.AspNet.OData;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class PrayerRequestsController
    {
        /// <summary>
        /// Queryable GET of PrayerRequest records that Public, Active, Approved and not expired
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [RockEnableQuery]
        [System.Web.Http.Route( "api/PrayerRequests/Public" )]
        [Rock.SystemGuid.RestActionGuid( "2FDAA0CF-37F4-4B1F-9847-8E028759871D" )]
        public IQueryable<PrayerRequest> Public()
        {
            var now = RockDateTime.Now;
            return base.Get()
                .Where( p =>
                    ( p.IsActive.HasValue && p.IsActive.Value == true ) &&
                    ( p.IsPublic.HasValue && p.IsPublic.Value == true ) &&
                    ( p.IsApproved.HasValue && p.IsApproved == true ) &&
                    ( !p.ExpirationDate.HasValue || p.ExpirationDate.Value > now )
                );
        }

        /// <summary>
        /// Queryable GET of Prayer Requests for the specified top-level category
        /// Prayer Requests that are in categories that are decendents of the specified category will also be included
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [RockEnableQuery]
        [System.Web.Http.Route( "api/PrayerRequests/GetByCategory/{categoryId}" )]
        [Rock.SystemGuid.RestActionGuid( "1EA8EFAA-1481-4FC0-9681-CA940AB25309" )]
        public IQueryable<PrayerRequest> GetByCategory( int categoryId )
        {
            var rockContext = ( this.Service.Context as RockContext ) ?? new RockContext();
            var decendentsCategoriesQry = new CategoryService( rockContext ).GetAllDescendents( categoryId ).Select( a => a.Id );
            return this.Get().Where( a => a.CategoryId.HasValue ).Where( a => decendentsCategoriesQry.Contains( a.CategoryId.Value ) || ( a.CategoryId.Value == categoryId ) );
        }

        /// <summary>
        /// Increment the prayer count for a prayer request
        /// </summary>
        /// <param name="prayerId">The prayer identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{prayerId}/{personAliasId}" )]
        [Rock.SystemGuid.RestActionGuid( "B5A3A2C4-A841-4309-8952-34A86B872478" )]
        public virtual void Prayed( int prayerId, int personAliasId )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( prayerId, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );
            Service.Context.SaveChanges();
        }


        /// <summary>
        /// Flags the specified prayer request.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Flag/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "170FBFD5-CB6A-4A4C-88F0-E730E8AF7321" )]
        public virtual void Flag( int id )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( id, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.FlagCount = ( prayerRequest.FlagCount ?? 0 ) + 1;
            if ( prayerRequest.FlagCount >= 1 )
            {
                prayerRequest.IsApproved = false;
            }

            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Increment the prayer count for a prayer request
        /// </summary>
        /// <param name="id">The prayer identifier.</param>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{id:int}" )]
        [Rock.SystemGuid.RestActionGuid( "86675411-D420-4D8B-A421-2BD8C92F4069" )]
        public virtual void Prayed( int id )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( id, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Prays for the specified prayer request unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier of the prayer request being prayed for.</param>
        /// <param name="workflowTypeGuid">The unique identifier of the workflow type to launch for this prayer request.</param>
        /// <param name="recordInteraction">If <c>true</c> then an interaction will be recorded for this prayed action.</param>
        /// <returns>The action result.</returns>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{guid:guid}" )]
        [Rock.SystemGuid.RestActionGuid( "9696DB0A-4CCC-4530-BC8D-E4E54A438BFA" )]
        public IHttpActionResult Prayed( Guid guid, Guid? workflowTypeGuid = null, bool recordInteraction = true )
        {
            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            var found = PrayForRequest( guid, RockRequestContext, workflowTypeGuid, recordInteraction );

            if ( !found )
            {
                return NotFound();
            }

            return Ok();
        }

        #region Helper Methods

        /*
         * These methods should be moved to the PrayerRequestsController after
         * they have been tweaked to be a bit more general use approved.
         *
         * Daniel Hazelbaker 9/29/2021
         */

        /// <summary>
        /// Prays for the specified request and optionally launches a workflow
        /// and/or records an interaction.
        /// </summary>
        /// <param name="prayerRequestGuid">The prayer request unique identifier that is being prayed for.</param>
        /// <param name="requestContext">The request context that describes the current request in process.</param>
        /// <param name="launchWorkflowGuid">The workflow type unique identifier to be launched.</param>
        /// <param name="recordInteraction">If set to <c>true</c> then an interaction will be recorded.</param>
        private bool PrayForRequest( Guid prayerRequestGuid, RockRequestContext requestContext, Guid? launchWorkflowGuid, bool recordInteraction )
        {
            return PrayForRequest( prayerRequestGuid,
                requestContext.CurrentPerson,
                launchWorkflowGuid,
                recordInteraction,
                string.Empty,
                requestContext.ClientInformation?.UserAgent,
                requestContext.ClientInformation?.IpAddress,
                null );
        }

        /// <summary>
        /// Prays for the specified request and optionally launches a workflow
        /// and/or records an interaction.
        /// </summary>
        /// <param name="prayerRequestGuid">The prayer request unique identifier that is being prayed for.</param>
        /// <param name="currentPerson">The current person whom is performing the action.</param>
        /// <param name="launchWorkflowGuid">The workflow type unique identifier to be launched.</param>
        /// <param name="recordInteraction">If set to <c>true</c> then an interaction will be recorded.</param>
        /// <param name="interactionSummary">The interaction summary text.</param>
        /// <param name="userAgent">The user agent for the interaction.</param>
        /// <param name="clientIpAddress">The client IP address for the interaction.</param>
        /// <param name="sessionGuid">The session unique identifier for the interaction.</param>
        /// <exception cref="System.ArgumentNullException">prayerRequest</exception>
        private static bool PrayForRequest( Guid prayerRequestGuid, Person currentPerson, Guid? launchWorkflowGuid, bool recordInteraction, string interactionSummary, string userAgent, string clientIpAddress, Guid? sessionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );
                var prayerRequest = prayerRequestService.Get( prayerRequestGuid );

                if ( prayerRequest == null )
                {
                    return false;
                }

                prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

                rockContext.SaveChanges();

                if ( launchWorkflowGuid.HasValue )
                {
                    PrayerRequestService.LaunchPrayedForWorkflow( prayerRequest, launchWorkflowGuid.Value, currentPerson );
                }

                if ( recordInteraction )
                {
                    PrayerRequestService.EnqueuePrayerInteraction( prayerRequest, currentPerson, interactionSummary, userAgent, clientIpAddress, sessionGuid );
                }

                return true;
            }
        }

        #endregion
    }
}
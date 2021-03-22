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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Connection Requests Controller
    /// </summary>
    public partial class ConnectionRequestsController
    {
        #region Standard Method Overrides

        /// <summary>
        /// DELETE endpoint. To delete the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        public override void Delete( int id )
        {
            var rockContext = Service.Context as RockContext;
            var service = Service as ConnectionRequestService;
            var connectionRequest = service.Queryable()
                .Include( cr => cr.ConnectionRequestActivities )
                .FirstOrDefault( cr => cr.Id == id );

            if ( connectionRequest == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            if ( !service.CanDelete( connectionRequest, out var errorMessage ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            rockContext.WrapTransaction( () =>
            {
                var activityService = new ConnectionRequestActivityService( rockContext );
                activityService.DeleteRange( connectionRequest.ConnectionRequestActivities );
                service.Delete( connectionRequest );
                rockContext.SaveChanges();
            } );
        }

        #endregion Standard Method Overrides

        #region Connection Request Picker Endpoints

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ConnectionRequests/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren( string id )
        {
            // Enable proxy creation since child collections need to be navigated
            SetProxyCreation( true );

            var rockContext = ( RockContext ) Service.Context;
            var list = new List<TreeViewItem>();

            if ( id.StartsWith( "T" ) )
            {
                int connectionTypeId = id.Substring( 1 ).AsInteger();
                foreach ( var opportunity in new ConnectionOpportunityService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( o => o.ConnectionTypeId == connectionTypeId )
                    .OrderBy( o => o.Name ) )
                {
                    var item = new TreeViewItem();
                    item.Id = string.Format( "O{0}", opportunity.Id );
                    item.Name = opportunity.Name;
                    item.HasChildren = opportunity.ConnectionRequests
                        .Any( r =>
                            r.ConnectionState == ConnectionState.Active ||
                            r.ConnectionState == ConnectionState.FutureFollowUp );
                    item.IconCssClass = opportunity.IconCssClass;
                    list.Add( item );
                }
            }

            else if ( id.StartsWith( "O" ) )
            {
                int opportunityId = id.Substring( 1 ).AsInteger();
                foreach ( var request in Service
                    .Queryable().AsNoTracking()
                    .Where( r =>
                        r.ConnectionOpportunityId == opportunityId &&
                        r.PersonAlias != null &&
                        r.PersonAlias.Person != null )
                    .OrderBy( r => r.PersonAlias.Person.LastName )
                    .ThenBy( r => r.PersonAlias.Person.NickName ) )
                {
                    var item = new TreeViewItem();
                    item.Id = request.Id.ToString();
                    item.Name = request.PersonAlias.Person.FullName;
                    item.HasChildren = false;
                    item.IconCssClass = "fa fa-user";
                    list.Add( item );
                }
            }

            else
            {
                int? requestId = id.AsIntegerOrNull();
                if ( !requestId.HasValue || requestId.Value == 0 )
                {
                    foreach ( var connectionType in new ConnectionTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .OrderBy( t => t.Name ) )
                    {
                        var item = new TreeViewItem();
                        item.Id = string.Format( "T{0}", connectionType.Id );
                        item.Name = connectionType.Name;
                        item.HasChildren = connectionType.ConnectionOpportunities.Any();
                        item.IconCssClass = connectionType.IconCssClass;
                        list.Add( item );
                    }
                }
            }

            return list.AsQueryable();
        }

        #endregion Connection Request Picker Endpoints

        #region Connection Request Board Endpoints

        /// <summary>
        /// Does the status change cause workflows to be triggered?
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="fromStatusId">From status identifier.</param>
        /// <param name="toStatusId">To status identifier.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured, HttpGet]
        [System.Web.Http.Route( "api/ConnectionRequests/DoesStatusChangeCauseWorkflows/{connectionOpportunityId}/{fromStatusId}/{toStatusId}" )]
        public WorkflowCheckViewModel DoesStatusChangeCauseWorkflows( int connectionOpportunityId, int fromStatusId, int toStatusId )
        {
            var connectionRequestService = Service as ConnectionRequestService;
            return connectionRequestService.DoesStatusChangeCauseWorkflows( connectionOpportunityId, fromStatusId, toStatusId );
        }

        /// <summary>
        /// Gets the connection board status view models.
        /// </summary>
        /// <param name="connectionRequestId">The connection request identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="connectorPersonAliasId">The connector person alias identifier.</param>
        /// <param name="requesterPersonAliasId">The requester person alias identifier.</param>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="maxDate">The maximum date.</param>
        /// <param name="delimitedStatusIds">The delimited status ids.</param>
        /// <param name="delimitedConnectionStates">The delimited connection states.</param>
        /// <param name="delimitedLastActivityTypeIds">The delimited last activity type ids.</param>
        /// <param name="statusIconsTemplate">The status icons template.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ConnectionRequests/ConnectionBoardRequestViewModel/{connectionRequestId}" )]
        public ConnectionRequestViewModel GetConnectionBoardStatusViewModel(
            int connectionRequestId,
            int? campusId = null,
            int? connectorPersonAliasId = null,
            int? requesterPersonAliasId = null,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            string delimitedStatusIds = null,
            string delimitedConnectionStates = null,
            string delimitedLastActivityTypeIds = null,
            string statusIconsTemplate = null )
        {
            var personAliasId = GetPersonAliasId();

            if ( !personAliasId.HasValue )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The current person alias did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            var connectionRequestService = Service as ConnectionRequestService;
            return connectionRequestService.GetConnectionRequestViewModel(
                personAliasId.Value,
                connectionRequestId,
                new ConnectionRequestViewModelQueryArgs
                {
                    CampusId = campusId,
                    ConnectorPersonAliasId = connectorPersonAliasId,
                    RequesterPersonAliasId = requesterPersonAliasId,
                    MinDate = minDate,
                    MaxDate = maxDate,
                    StatusIds = delimitedStatusIds.SplitDelimitedValues().AsIntegerList(),
                    ConnectionStates = delimitedConnectionStates
                        .SplitDelimitedValues()
                        .Select( s => ( ConnectionState ) Enum.Parse( typeof( ConnectionState ), s ) )
                        .ToList(),
                    LastActivityTypeIds = delimitedLastActivityTypeIds.SplitDelimitedValues().AsIntegerList()
                },
                statusIconsTemplate );
        }

        /// <summary>
        /// Gets the connection board status view models.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="connectorPersonAliasId">The connector person alias identifier.</param>
        /// <param name="requesterPersonAliasId">The requester person alias identifier.</param>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="maxDate">The maximum date.</param>
        /// <param name="delimitedStatusIds">The delimited status ids.</param>
        /// <param name="delimitedConnectionStates">The delimited connection states.</param>
        /// <param name="delimitedLastActivityTypeIds">The delimited last activity type ids.</param>
        /// <param name="statusIconsTemplate">The status icons template.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="maxRequestsPerStatus">The maximum requests per status.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ConnectionRequests/ConnectionBoardStatusViewModels/{connectionOpportunityId}" )]
        public List<ConnectionStatusViewModel> GetConnectionBoardStatusViewModels(
            int connectionOpportunityId,
            int? campusId = null,
            int? connectorPersonAliasId = null,
            int? requesterPersonAliasId = null,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            string delimitedStatusIds = null,
            string delimitedConnectionStates = null,
            string delimitedLastActivityTypeIds = null,
            string statusIconsTemplate = null,
            ConnectionRequestViewModelSortProperty? sortProperty = null,
            int? maxRequestsPerStatus = null )
        {
            var personAliasId = GetPersonAliasId();

            if ( !personAliasId.HasValue )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The current person alias did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            var connectionRequestService = Service as ConnectionRequestService;
            var connectionStatusViewModels = connectionRequestService.GetConnectionBoardStatusViewModels(
                personAliasId.Value,
                connectionOpportunityId,
                new ConnectionRequestViewModelQueryArgs {
                    CampusId = campusId,
                    ConnectorPersonAliasId = connectorPersonAliasId,
                    RequesterPersonAliasId = requesterPersonAliasId,
                    MinDate = minDate,
                    MaxDate = maxDate,
                    StatusIds = delimitedStatusIds.SplitDelimitedValues().AsIntegerList(),
                    ConnectionStates = delimitedConnectionStates
                        .SplitDelimitedValues()
                        .Select( s => ( ConnectionState ) Enum.Parse( typeof( ConnectionState ), s ) )
                        .ToList(),
                    LastActivityTypeIds = delimitedLastActivityTypeIds.SplitDelimitedValues().AsIntegerList(),
                    SortProperty = sortProperty
                },
                statusIconsTemplate,
                maxRequestsPerStatus );

            return connectionStatusViewModels;
        }

        #endregion Connection Request Board Endpoints
    }
}

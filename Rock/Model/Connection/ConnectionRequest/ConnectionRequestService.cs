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

using Rock.Data;
using Rock.Lava;
using Rock.Model.Connection.ConnectionRequest.Options;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Connection Request Service
    /// </summary>
    public partial class ConnectionRequestService
    {
        #region Default Options

        /// <summary>
        /// The default options to use if not specified. This saves a few
        /// CPU cycles from having to create a new one each time.
        /// </summary>
        private static readonly ConnectionRequestQueryOptions DefaultGetConnectionTypesOptions = new ConnectionRequestQueryOptions();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the connection requests queryable that is filtered correctly
        /// to the provided options.
        /// </summary>
        /// <param name="options">The filter options to apply to the query.</param>
        /// <returns>A queryable of <see cref="ConnectionRequest"/> objects.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public IQueryable<ConnectionRequest> GetConnectionRequestsQuery( ConnectionRequestQueryOptions options = null )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new InvalidOperationException( "Context is not a RockContext." );
            }

            options = options ?? DefaultGetConnectionTypesOptions;

            var qry = Queryable();

            if ( options.ConnectionOpportunityGuids != null && options.ConnectionOpportunityGuids.Any() )
            {
                qry = qry.Where( r => options.ConnectionOpportunityGuids.Contains( r.ConnectionOpportunity.Guid ) );
            }

            if ( options.ConnectorPersonIds != null && options.ConnectorPersonIds.Any() )
            {
                qry = qry.Where( r => options.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) );
            }

            if ( options.ConnectionStates != null && options.ConnectionStates.Any() )
            {
                qry = qry.Where( r => options.ConnectionStates.Contains( r.ConnectionState ) );
            }

            if ( options.CampusGuid != null )
            {
                var campusId = CampusCache.GetId( options.CampusGuid.Value );
                if ( campusId != null )
                {
                    qry = qry.Where( r => r.CampusId == campusId );
                }
            }

            // Filter past due: Allow other states to go through, but "future follow-up" must be due today or already past due
            if ( options.IsFutureFollowUpPastDueOnly )
            {
                var midnight = RockDateTime.Today.AddDays( 1 );

                qry = qry.Where( cr =>
                    cr.ConnectionState != ConnectionState.FutureFollowUp ||
                    (
                        cr.FollowupDate.HasValue &&
                        cr.FollowupDate.Value < midnight
                    ) );
            }

            return qry;
        }

        #endregion

        #region Connection Board Helper Methods

        /// <summary>
        /// Determines whether this request can be connected.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="connectionType">Type of the connection.</param>
        /// <returns>
        ///   <c>true</c> if this instance can connect; otherwise, <c>false</c>.
        /// </returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use CanConnect( ConnectionRequestViewModel request, ConnectionOpportunity connectionOpportunity, ConnectionTypeCache connectionType )" )]
        public bool CanConnect( ConnectionRequestViewModel request, ConnectionTypeCache connectionType )
        {
            var rockContext = Context as RockContext;
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );

            var connectionOpportunity = connectionOpportunityService.Queryable()
                .AsNoTracking()
                .FirstOrDefault( co => co.Id == request.ConnectionOpportunityId );

            return CanConnect( request, connectionOpportunity, connectionType );
        }

        /// <summary>
        /// Determines whether this request can be connected.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>true</c> if this instance can connect; otherwise, <c>false</c>.
        /// </returns>
        public bool CanConnect( ConnectionRequest request )
        {
            if ( request == null || ( !request.AssignedGroupId.HasValue && request.ConnectionOpportunity.ConnectionType.RequiresPlacementGroupToConnect ) )
            {
                return false;
            }

            return
                request.ConnectionState != ConnectionState.Inactive &&
                request.ConnectionState != ConnectionState.Connected &&
                request.ConnectionOpportunity.ShowConnectButton;
        }

        /// <summary>
        /// Determines whether this request can be connected.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="connectionOpportunity">The connection opportunity.</param>
        /// <param name="connectionType">Type of the connection.</param>
        /// <returns>
        ///   <c>true</c> if this instance can connect; otherwise, <c>false</c>.
        /// </returns>
        public bool CanConnect( ConnectionRequestViewModel request, ConnectionOpportunity connectionOpportunity, ConnectionTypeCache connectionType )
        {
            if ( request == null || ( !request.PlacementGroupId.HasValue && connectionType.RequiresPlacementGroupToConnect ) )
            {
                return false;
            }

            return
                request.ConnectionState != ConnectionState.Inactive &&
                request.ConnectionState != ConnectionState.Connected &&
                connectionOpportunity.ShowConnectButton;
        }

        /// <summary>
        /// Checks if the status change would cause workflows to launch.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="fromStatusId">From status identifier.</param>
        /// <param name="toStatusId">To status identifier.</param>
        /// <returns></returns>
        public WorkflowCheckViewModel DoesStatusChangeCauseWorkflows( int connectionOpportunityId, int fromStatusId, int toStatusId )
        {
            var rockContext = Context as RockContext;
            var connectionStatusService = new ConnectionStatusService( rockContext );
            var connectionWorkflowService = new ConnectionWorkflowService( rockContext );

            // Query for the names of the statuses
            var statusQuery = connectionStatusService.Queryable()
                .AsNoTracking()
                .Where( cs => cs.Id == fromStatusId || cs.Id == toStatusId )
                .Select( cs => new { cs.Id, cs.Name } )
                .ToList();

            var fromStatus = statusQuery.FirstOrDefault( cs => cs.Id == fromStatusId );
            var toStatus = statusQuery.FirstOrDefault( cs => cs.Id == toStatusId );

            // Query for if there are connection workflows for these statuses
            var workflowQuery = connectionWorkflowService.Queryable()
                .AsNoTracking()
                .Where( cw => cw.TriggerType == ConnectionWorkflowTriggerType.StatusChanged )
                .Where( cw =>
                    cw.ConnectionType.ConnectionOpportunities.Any( co => co.Id == connectionOpportunityId ) ||
                    cw.ConnectionOpportunityId == connectionOpportunityId );

            var startsWith = string.Format( "|{0}|", fromStatusId );
            workflowQuery = workflowQuery.Where( cw =>
                 cw.QualifierValue.StartsWith( startsWith ) ||
                 cw.QualifierValue.StartsWith( "||" ) ||
                 cw.QualifierValue == string.Empty ||
                 cw.QualifierValue == null );

            var endsWith = string.Format( "|{0}|", toStatusId );
            workflowQuery = workflowQuery.Where( cw =>
                cw.QualifierValue.EndsWith( endsWith ) ||
                cw.QualifierValue.EndsWith( "||" ) ||
                cw.QualifierValue == string.Empty ||
                cw.QualifierValue == null );

            // Return the results
            return new WorkflowCheckViewModel
            {
                DoesCauseWorkflows = workflowQuery.Any(),
                FromStatusName = fromStatus?.Name,
                ToStatusName = toStatus?.Name
            };
        }

        /// <summary>
        /// Gets the connection board status view models.
        /// </summary>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="statusIconsTemplate">The status icons template.</param>
        /// <param name="maxRequestsPerStatus">The maximum requests per status.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The connection type did not resolve</exception>
        public List<ConnectionStatusViewModel> GetConnectionBoardStatusViewModels(
            int currentPersonAliasId,
            int connectionOpportunityId,
            ConnectionRequestViewModelQueryArgs args,
            string statusIconsTemplate = null,
            int? maxRequestsPerStatus = null )
        {
            ValidateArgs( args );

            var rockContext = Context as RockContext;
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );


            var connectionOpportunity = connectionOpportunityService.Queryable()
                .AsNoTracking()
                .FirstOrDefault( co => co.Id == connectionOpportunityId );

            var connectionType = connectionOpportunity != null ?
                ConnectionTypeCache.Get( connectionOpportunity.ConnectionTypeId ) :
                null;

            if ( connectionType == null )
            {
                throw new ArgumentException( "The connection type did not resolve" );
            }

            // Begin querying for requests
            var connectionRequestViewModelQuery = GetConnectionRequestViewModelQuery(
                currentPersonAliasId,
                connectionOpportunityId,
                args );

            var connectionStatusQuery = GetConnectionStatusQuery( connectionOpportunityId )
                .Select( cs => new ConnectionStatusViewModel
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    HighlightColor = cs.HighlightColor
                } );

            if ( args.StatusIds?.Any() == true )
            {
                connectionStatusQuery = connectionStatusQuery.Where( cs => args.StatusIds.Contains( cs.Id ) );
            }

            var connectionStatusViewModels = connectionStatusQuery.ToList();

            var connectionRequestService = new ConnectionRequestService( rockContext );
            var requestIds = connectionRequestViewModelQuery.Select( cr => cr.Id );
            var statusRequests = connectionRequestService.Queryable().Where( cr => requestIds.Contains( cr.Id ) ).ToList();
            statusRequests.LoadFilteredAttributes( rockContext, attribute => attribute.IsGridColumn );
            var currentPerson = new PersonAliasService( rockContext ).GetPersonNoTracking( currentPersonAliasId );

            foreach ( var statusViewModel in connectionStatusViewModels )
            {
                var requestsOfStatusQuery = connectionRequestViewModelQuery.Where( cr => cr.StatusId == statusViewModel.Id );
                statusViewModel.RequestCount = requestsOfStatusQuery.Count();

                if ( maxRequestsPerStatus.HasValue )
                {
                    requestsOfStatusQuery = requestsOfStatusQuery.Take( maxRequestsPerStatus.Value );
                }

                statusViewModel.Requests = requestsOfStatusQuery.ToList();

                if ( statusViewModel.HighlightColor.IsNullOrWhiteSpace() )
                {
                    statusViewModel.HighlightColor = ConnectionStatus.DefaultHighlightColor;
                }

                if ( !statusIconsTemplate.IsNullOrWhiteSpace() )
                {
                    foreach ( var requestViewModel in statusViewModel.Requests )
                    {
                        requestViewModel.StatusIconsHtml = GetStatusIconHtml( requestViewModel, connectionType, statusIconsTemplate );
                    }
                }

                foreach ( var requestViewModel in statusViewModel.Requests )
                {
                    requestViewModel.CanConnect = CanConnect( requestViewModel, connectionOpportunity, connectionType );

                    var connectionRequest = statusRequests.First( cr => cr.Id == requestViewModel.Id );
                    var attributeValues = connectionRequest.AttributeValues
                        .Where( a => IsAuthorizedToViewAndNotEmpty( a, currentPerson, connectionRequest ) )
                        .Select( a => $"<strong>{a.Value.AttributeName}:</strong> {connectionRequest.GetAttributeTextValue( a.Key )}" );
                    requestViewModel.RequestAttributes = string.Join( "<br>", attributeValues );
                }
            }

            return connectionStatusViewModels;
        }

        /// <summary>
        /// Checks if the attribute value is not empty, is a grid column and the current person is authorized to view it.
        /// </summary>
        /// <param name="a">The attribute value attribute key dictionary</param>
        /// <param name="currentPerson">The current person</param>
        /// <param name="connectionRequest">The connection request</param>
        /// <returns></returns>
        private static bool IsAuthorizedToViewAndNotEmpty( KeyValuePair<string, AttributeValueCache> a, Person currentPerson, ConnectionRequest connectionRequest )
        {
            return !string.IsNullOrWhiteSpace( a.Value.Value ) && connectionRequest.Attributes[a.Key].IsAuthorized( Authorization.VIEW, currentPerson );
        }

        /// <summary>
        /// Gets the connection request view model.
        /// </summary>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="connectionRequestId">The connection request identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="statusIconsTemplate">The status icons template.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">An args object is required</exception>
        public ConnectionRequestViewModel GetConnectionRequestViewModel(
            int currentPersonAliasId,
            int connectionRequestId,
            ConnectionRequestViewModelQueryArgs args,
            string statusIconsTemplate = null )
        {
            ValidateArgs( args );

            // Set the Connection Request Id here so that the GetConnectionRequestViewModelQuery method can filter correctly.
            args.ConnectionRequestId = connectionRequestId;

            var connectionRequest = Queryable()
                .Include( cr => cr.ConnectionOpportunity )
                .AsNoTracking()
                .Where( cr => cr.Id == connectionRequestId )
                .FirstOrDefault();
            var connectionOpportunity = connectionRequest.ConnectionOpportunity;

            var query = GetConnectionRequestViewModelQuery( currentPersonAliasId, connectionOpportunity.Id, args );
            var viewModel = query.FirstOrDefault();

            if ( viewModel == null )
            {
                return null;
            }

            var connectionType = ConnectionTypeCache.Get( viewModel.ConnectionTypeId );
            viewModel.CanConnect = CanConnect( viewModel, connectionOpportunity, connectionType );

            var rockContext = Context as RockContext;
            var currentPerson = new PersonAliasService( rockContext ).GetPersonNoTracking( currentPersonAliasId );
            connectionRequest.LoadAttributes( rockContext );
            var attributeValues = connectionRequest.AttributeValues
                .Where( a => connectionRequest.Attributes[a.Key].IsGridColumn && IsAuthorizedToViewAndNotEmpty( a, currentPerson, connectionRequest ) )
                .Select( a => $"<strong>{a.Value.AttributeName}:</strong> {connectionRequest.GetAttributeTextValue( a.Key )}" );
            viewModel.RequestAttributes = string.Join( "<br>", attributeValues );

            if ( !statusIconsTemplate.IsNullOrWhiteSpace() )
            {
                viewModel.StatusIconsHtml = GetStatusIconHtml( viewModel, connectionType, statusIconsTemplate );
            }

            return viewModel;
        }

        /// <summary>
        /// Gets the connection request view model query.
        /// </summary>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">An args object is required</exception>
        public IQueryable<ConnectionRequestViewModel> GetConnectionRequestViewModelQuery(
            int currentPersonAliasId,
            int connectionOpportunityId,
            ConnectionRequestViewModelQueryArgs args )
        {
            var query = GetConnectionRequestViewModelSecurityQuery( currentPersonAliasId, connectionOpportunityId, args );

            return query.Select( cr => new ConnectionRequestViewModel
            {
                Id = cr.Id,
                ConnectionOpportunityId = cr.ConnectionOpportunityId,
                ConnectionTypeId = cr.ConnectionTypeId,
                PlacementGroupId = cr.PlacementGroupId,
                PlacementGroupRoleId = cr.PlacementGroupRoleId,
                PlacementGroupMemberStatus = cr.PlacementGroupMemberStatus,
                PlacementGroupRoleName = cr.PlacementGroupRoleName,
                Comments = cr.Comments,
                StatusId = cr.StatusId,
                PersonId = cr.PersonId,
                PersonAliasId = cr.PersonAliasId,
                PersonEmail = cr.PersonEmail,
                PersonNickName = cr.PersonNickName,
                PersonLastName = cr.PersonLastName,
                PersonPhotoId = cr.PersonPhotoId,
                PersonPhones = cr.PersonPhones,
                CampusId = cr.CampusId,
                CampusName = cr.CampusName,
                Campus = cr.Campus,
                CampusCode = cr.CampusCode,
                ConnectorPersonNickName = cr.ConnectorPersonNickName,
                ConnectorPersonLastName = cr.ConnectorPersonLastName,
                ConnectorPersonId = cr.ConnectorPersonId,
                ConnectorPhotoId = cr.ConnectorPhotoId,
                ConnectorPersonAliasId = cr.ConnectorPersonAliasId,
                ActivityCount = cr.ActivityCount,
                DateOpened = cr.DateOpened,
                GroupName = cr.GroupName,
                StatusName = cr.StatusName,
                StatusHighlightColor = cr.StatusHighlightColor,
                IsStatusCritical = cr.IsStatusCritical,
                IsAssignedToYou = cr.IsAssignedToYou,
                IsCritical = cr.IsCritical,
                IsIdle = cr.IsIdle,
                IsUnassigned = cr.IsUnassigned,
                ConnectionState = cr.ConnectionState,
                LastActivityTypeName = cr.LastActivityTypeName,
                Order = cr.Order,
                LastActivityTypeId = cr.LastActivityTypeId,
                LastActivityDate = cr.LastActivityDate,
                FollowupDate = cr.FollowupDate,
                CanCurrentUserEdit = cr.CanCurrentUserEdit,
            } );
        }

        /// <summary>
        /// Gets the connection request view model with full model query.
        /// </summary>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">An args object is required</exception>
        public IQueryable<ConnectionRequestViewModelWithModel> GetConnectionRequestViewModelWithFullModelQuery(
            int currentPersonAliasId,
            int connectionOpportunityId,
            ConnectionRequestViewModelQueryArgs args )
        {
            var query = GetConnectionRequestViewModelSecurityQuery( currentPersonAliasId, connectionOpportunityId, args );

            return query.Select( cr => new ConnectionRequestViewModelWithModel
            {
                Id = cr.Id,
                ConnectionRequest = cr.ConnectionRequest,
                ConnectionOpportunityId = cr.ConnectionOpportunityId,
                ConnectionTypeId = cr.ConnectionTypeId,
                PlacementGroupId = cr.PlacementGroupId,
                PlacementGroupRoleId = cr.PlacementGroupRoleId,
                PlacementGroupMemberStatus = cr.PlacementGroupMemberStatus,
                PlacementGroupRoleName = cr.PlacementGroupRoleName,
                Comments = cr.Comments,
                StatusId = cr.StatusId,
                PersonId = cr.PersonId,
                PersonAliasId = cr.PersonAliasId,
                PersonEmail = cr.PersonEmail,
                PersonNickName = cr.PersonNickName,
                PersonLastName = cr.PersonLastName,
                PersonPhotoId = cr.PersonPhotoId,
                PersonPhones = cr.PersonPhones,
                CampusId = cr.CampusId,
                CampusName = cr.CampusName,
                CampusCode = cr.CampusCode,
                ConnectorPersonNickName = cr.ConnectorPersonNickName,
                ConnectorPersonLastName = cr.ConnectorPersonLastName,
                ConnectorPersonId = cr.ConnectorPersonId,
                ConnectorPhotoId = cr.ConnectorPhotoId,
                ConnectorPersonAliasId = cr.ConnectorPersonAliasId,
                ActivityCount = cr.ActivityCount,
                DateOpened = cr.DateOpened,
                GroupName = cr.GroupName,
                StatusName = cr.StatusName,
                StatusHighlightColor = cr.StatusHighlightColor,
                IsStatusCritical = cr.IsStatusCritical,
                IsAssignedToYou = cr.IsAssignedToYou,
                IsCritical = cr.IsCritical,
                IsIdle = cr.IsIdle,
                IsUnassigned = cr.IsUnassigned,
                ConnectionState = cr.ConnectionState,
                LastActivityTypeName = cr.LastActivityTypeName,
                Order = cr.Order,
                LastActivityTypeId = cr.LastActivityTypeId,
                LastActivityDate = cr.LastActivityDate,
                FollowupDate = cr.FollowupDate,
                CanCurrentUserEdit = cr.CanCurrentUserEdit,
            } );
        }

        /// <summary>
        /// Gets the connection request query.
        /// </summary>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">An args object is required</exception>
        private IQueryable<ConnectionRequestViewModelSecurity> GetConnectionRequestViewModelSecurityQuery(
            int currentPersonAliasId,
            int connectionOpportunityId,
            ConnectionRequestViewModelQueryArgs args )
        {
            ValidateArgs( args );

            var currentDateTime = RockDateTime.Now;
            var midnightToday = RockDateTime.Today.AddDays( 1 );

            var rockContext = Context as RockContext;
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );

            /*
             * 13-May-2022 DMV
             *
             * When using the ConnectionOpportunityService, if the connection type
             * is not included, there are cases were the ParentAuthority will
             * no be set correctly. See https://github.com/SparkDevNetwork/Rock/issues/5009
             *
             */
            var connectionOpportunity = connectionOpportunityService.GetInclude( connectionOpportunityId, co => co.ConnectionType );

            var connectionType = connectionOpportunity == null ?
                null :
                ConnectionTypeCache.Get( connectionOpportunity.ConnectionTypeId );

            var daysUntilIdle = connectionType == null ? 1 : connectionType.DaysUntilRequestIdle;
            var idleDate = currentDateTime.AddDays( 0 - daysUntilIdle );
            var currentPerson = new PersonAliasService( rockContext ).GetPerson( currentPersonAliasId );
            var canViewAllRequests = currentPerson != null && connectionOpportunity.IsAuthorized( Authorization.VIEW, currentPerson );
            var canEditAllRequest = currentPerson != null && connectionOpportunity.IsAuthorized( Authorization.EDIT, currentPerson );

            // Query the statuses and requests in such a way that we get all statuses, even if there
            // are no requests in that column at this time
            var connectionRequestsQuery = Queryable()
                .Where( cr =>
                    cr.ConnectionOpportunityId == connectionOpportunityId &&
                    ( !args.CampusId.HasValue || args.CampusId.Value == cr.CampusId.Value ) )
                .Select( cr => new ConnectionRequestViewModelSecurity
                {
                    ConnectionRequest = cr,
                    ConnectionType = cr.ConnectionOpportunity.ConnectionType,
                    Id = cr.Id,
                    ConnectionOpportunityId = cr.ConnectionOpportunityId,
                    ConnectionTypeId = cr.ConnectionOpportunity.ConnectionTypeId,
                    PlacementGroupId = cr.AssignedGroupId,
                    PlacementGroupRoleId = cr.AssignedGroupMemberRoleId,
                    PlacementGroupMemberStatus = cr.AssignedGroupMemberStatus,
                    PlacementGroupRoleName = cr.AssignedGroup.GroupType.DefaultGroupRole.Name,
                    Comments = cr.Comments,
                    StatusId = cr.ConnectionStatusId,
                    PersonId = cr.PersonAlias.PersonId,
                    PersonAliasId = cr.PersonAliasId,
                    PersonEmail = cr.PersonAlias.Person.Email,
                    PersonNickName = cr.PersonAlias.Person.NickName,
                    PersonLastName = cr.PersonAlias.Person.LastName,
                    PersonPhotoId = cr.PersonAlias.Person.PhotoId,
                    PersonPhones = cr.PersonAlias.Person.PhoneNumbers.Select( pn => new ConnectionRequestViewModel.PhoneViewModel
                    {
                        PhoneType = pn.NumberTypeValue.Value,
                        FormattedPhoneNumber = pn.NumberFormatted,
                        IsMessagingEnabled = pn.IsMessagingEnabled
                    } ).ToList(),
                    CampusId = cr.CampusId,
                    CampusName = cr.Campus.Name,
                    Campus = cr.Campus,
                    CampusCode = cr.Campus.ShortCode,
                    ConnectorPersonNickName = cr.ConnectorPersonAlias.Person.NickName,
                    ConnectorPersonLastName = cr.ConnectorPersonAlias.Person.LastName,
                    ConnectorPersonId = cr.ConnectorPersonAlias.PersonId,
                    ConnectorPhotoId = cr.ConnectorPersonAlias.Person.PhotoId,
                    ConnectorPersonAliasId = cr.ConnectorPersonAliasId,
                    ActivityCount = cr.ConnectionRequestActivities.Count,
                    DateOpened = cr.CreatedDateTime,
                    GroupName = cr.AssignedGroup.Name,
                    StatusName = cr.ConnectionStatus.Name,
                    StatusHighlightColor = cr.ConnectionStatus.HighlightColor,
                    IsStatusCritical = cr.ConnectionStatus != null && cr.ConnectionStatus.IsCritical,
                    IsAssignedToYou = cr.ConnectorPersonAliasId == currentPersonAliasId,
                    IsCritical =
                        cr.ConnectionStatus != null &&
                        cr.ConnectionStatus.IsCritical &&
                        (
                            cr.ConnectionState == ConnectionState.Active ||
                            (
                                cr.ConnectionState == ConnectionState.FutureFollowUp &&
                                cr.FollowupDate.HasValue && cr.FollowupDate.Value < midnightToday
                            )
                        ),
                    IsIdle =
                        (
                            cr.ConnectionState == ConnectionState.Active ||
                            (
                                cr.ConnectionState == ConnectionState.FutureFollowUp &&
                                cr.FollowupDate.HasValue &&
                                cr.FollowupDate.Value < midnightToday
                            )
                        ) && (
                            (
                                cr.ConnectionRequestActivities.Any( ra => ra.CreatedDateTime.HasValue ) &&
                                cr.ConnectionRequestActivities.Where( ra => ra.CreatedDateTime.HasValue ).Max( ra => ra.CreatedDateTime.Value ) < idleDate
                            ) || (
                                !cr.ConnectionRequestActivities.Any( ra => ra.CreatedDateTime.HasValue ) &&
                                cr.CreatedDateTime.HasValue &&
                                cr.CreatedDateTime.Value < idleDate
                            )
                        ),
                    IsUnassigned = !cr.ConnectorPersonAliasId.HasValue,
                    ConnectionState = cr.ConnectionState,
                    LastActivityTypeName = cr.ConnectionRequestActivities
                        .OrderByDescending( a => a.CreatedDateTime )
                        .FirstOrDefault()
                        .ConnectionActivityType
                        .Name,
                    Order = cr.Order,
                    LastActivityTypeId = cr.ConnectionRequestActivities
                        .OrderByDescending( a => a.CreatedDateTime )
                        .FirstOrDefault()
                        .ConnectionActivityType
                        .Id,
                    LastActivityDate = cr.ConnectionRequestActivities
                        .Select( cra => cra.CreatedDateTime )
                        .OrderByDescending( d => d )
                        .FirstOrDefault(),
                    FollowupDate = cr.FollowupDate,
                    UserHasDirectAccess = false,
                    CanCurrentUserEdit = canEditAllRequest
                } );

            // Filter by connector
            if ( args.ConnectionRequestId.HasValue )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => cr.Id == args.ConnectionRequestId );
            }

            // Filter by connector
            if ( args.ConnectorPersonAliasId.HasValue )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var connectorPersonId = personAliasService.GetPersonId( args.ConnectorPersonAliasId.Value );
                var aliasList = personAliasService.Queryable().Where( pa => pa.PersonId == connectorPersonId ).Select( pa => pa.Id ).ToList();
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => aliasList.Contains( cr.ConnectorPersonAliasId.Value ) );
            }

            // Filter by date range
            if ( args.MinDate.HasValue )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => cr.LastActivityDate >= args.MinDate.Value );
            }

            if ( args.MaxDate.HasValue )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => cr.LastActivityDate <= args.MaxDate.Value );
            }

            // Filter requester
            if ( args.RequesterPersonAliasId.HasValue )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var requesterPersonId = personAliasService.GetPersonId( args.RequesterPersonAliasId.Value );
                var aliasList = personAliasService.Queryable().Where( pa => pa.PersonId == requesterPersonId ).Select( pa => pa.Id ).ToList();
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => aliasList.Contains( cr.PersonAliasId ) );
            }

            // Filter statuses
            if ( args.StatusIds?.Any() == true )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => args.StatusIds.Contains( cr.StatusId ) );
            }

            // Filter state
            if ( args.ConnectionStates?.Any() == true )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => args.ConnectionStates.Contains( cr.ConnectionState ) );
            }

            // Filter past due: Allow other states to go through, but "future follow-up" must be due today or already past due
            if ( args.IsFutureFollowUpPastDueOnly )
            {
                var midnight = RockDateTime.Today.AddDays( 1 );

                connectionRequestsQuery = connectionRequestsQuery.Where( cr =>
                    cr.ConnectionState != ConnectionState.FutureFollowUp ||
                    (
                        cr.FollowupDate.HasValue &&
                        cr.FollowupDate.Value < midnight
                    ) );
            }

            // Filter last activity
            if ( args.LastActivityTypeIds?.Any() == true )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr =>
                    cr.LastActivityTypeId.HasValue &&
                    args.LastActivityTypeIds.Contains( cr.LastActivityTypeId.Value ) );
            }

            if ( connectionType.EnableRequestSecurity )
            {
                var currentPersonId = currentPerson?.Id;
                var campusIdQuery = GetGlobalConnectorGroupCampusIds( connectionOpportunityId, currentPersonId );

                var connectionRequestsList = connectionRequestsQuery
                    .Select( cr => new ConnectionRequestViewModelSecurity
                    {
                        ConnectionRequest = cr.ConnectionRequest,
                        ConnectionType = cr.ConnectionType,
                        Id = cr.Id,
                        ConnectionOpportunityId = cr.ConnectionOpportunityId,
                        ConnectionTypeId = cr.ConnectionTypeId,
                        PlacementGroupId = cr.PlacementGroupId,
                        PlacementGroupRoleId = cr.PlacementGroupRoleId,
                        PlacementGroupMemberStatus = cr.PlacementGroupMemberStatus,
                        PlacementGroupRoleName = cr.PlacementGroupRoleName,
                        Comments = cr.Comments,
                        StatusId = cr.StatusId,
                        PersonId = cr.PersonId,
                        PersonAliasId = cr.PersonAliasId,
                        PersonEmail = cr.PersonEmail,
                        PersonNickName = cr.PersonNickName,
                        PersonLastName = cr.PersonLastName,
                        PersonPhotoId = cr.PersonPhotoId,
                        PersonPhones = cr.PersonPhones,
                        CampusId = cr.CampusId,
                        CampusName = cr.CampusName,
                        Campus = cr.Campus,
                        CampusCode = cr.CampusCode,
                        ConnectorPersonNickName = cr.ConnectorPersonNickName,
                        ConnectorPersonLastName = cr.ConnectorPersonLastName,
                        ConnectorPersonId = cr.ConnectorPersonId,
                        ConnectorPhotoId = cr.ConnectorPhotoId,
                        ConnectorPersonAliasId = cr.ConnectorPersonAliasId,
                        ActivityCount = cr.ActivityCount,
                        DateOpened = cr.DateOpened,
                        GroupName = cr.GroupName,
                        StatusName = cr.StatusName,
                        StatusHighlightColor = cr.StatusHighlightColor,
                        IsStatusCritical = cr.IsStatusCritical,
                        IsAssignedToYou = cr.IsAssignedToYou,
                        IsCritical = cr.IsCritical,
                        IsIdle = cr.IsIdle,
                        IsUnassigned = cr.IsUnassigned,
                        ConnectionState = cr.ConnectionState,
                        LastActivityTypeName = cr.LastActivityTypeName,
                        Order = cr.Order,
                        LastActivityTypeId = cr.LastActivityTypeId,
                        LastActivityDate = cr.LastActivityDate,
                        FollowupDate = cr.FollowupDate,
                        UserHasDirectAccess = cr.ConnectorPersonAliasId == currentPersonAliasId,
                        CanCurrentUserEdit = cr.CanCurrentUserEdit
                    } )
                    .ToList()
                    .Select( cr => new ConnectionRequestViewModelSecurity
                    {
                        ConnectionRequest = cr.ConnectionRequest,
                        ConnectionType = cr.ConnectionType,
                        Id = cr.Id,
                        ConnectionOpportunityId = cr.ConnectionOpportunityId,
                        ConnectionTypeId = cr.ConnectionTypeId,
                        PlacementGroupId = cr.PlacementGroupId,
                        PlacementGroupRoleId = cr.PlacementGroupRoleId,
                        PlacementGroupMemberStatus = cr.PlacementGroupMemberStatus,
                        PlacementGroupRoleName = cr.PlacementGroupRoleName,
                        Comments = cr.Comments,
                        StatusId = cr.StatusId,
                        PersonId = cr.PersonId,
                        PersonAliasId = cr.PersonAliasId,
                        PersonEmail = cr.PersonEmail,
                        PersonNickName = cr.PersonNickName,
                        PersonLastName = cr.PersonLastName,
                        PersonPhotoId = cr.PersonPhotoId,
                        PersonPhones = cr.PersonPhones,
                        CampusId = cr.CampusId,
                        CampusName = cr.CampusName,
                        Campus = cr.Campus,
                        CampusCode = cr.CampusCode,
                        ConnectorPersonNickName = cr.ConnectorPersonNickName,
                        ConnectorPersonLastName = cr.ConnectorPersonLastName,
                        ConnectorPersonId = cr.ConnectorPersonId,
                        ConnectorPhotoId = cr.ConnectorPhotoId,
                        ConnectorPersonAliasId = cr.ConnectorPersonAliasId,
                        ActivityCount = cr.ActivityCount,
                        DateOpened = cr.DateOpened,
                        GroupName = cr.GroupName,
                        StatusName = cr.StatusName,
                        StatusHighlightColor = cr.StatusHighlightColor,
                        IsStatusCritical = cr.IsStatusCritical,
                        IsAssignedToYou = cr.IsAssignedToYou,
                        IsCritical = cr.IsCritical,
                        IsIdle = cr.IsIdle,
                        IsUnassigned = cr.IsUnassigned,
                        ConnectionState = cr.ConnectionState,
                        LastActivityTypeName = cr.LastActivityTypeName,
                        Order = cr.Order,
                        LastActivityTypeId = cr.LastActivityTypeId,
                        LastActivityDate = cr.LastActivityDate,
                        FollowupDate = cr.FollowupDate,
                        UserHasDirectAccess = cr.UserHasDirectAccess,
                        CanCurrentUserEdit = cr.UserHasDirectAccess ||
                            cr.ConnectionRequest.IsAuthorized( Authorization.EDIT, currentPerson )
                    } )
                    .Where( cr => cr.UserHasDirectAccess ||
                             cr.ConnectionRequest.IsAuthorized( Authorization.VIEW, currentPerson ) );

                connectionRequestsQuery = connectionRequestsList.AsQueryable();
            }
            else if ( !canViewAllRequests )
            {
                // There are some scenarios where the current person can see the request even if the permissions say otherwise:
                // 1) Connection Request Security is not enabled, and the person is in a global (no campus) connector group
                // 2) Connection Request Security is not enabled, and the person is in the campus specific connector group
                var currentPersonId = currentPerson?.Id;
                var campusIdQuery = GetGlobalConnectorGroupCampusIds( connectionOpportunityId, currentPersonId );

                connectionRequestsQuery = connectionRequestsQuery
                    .Select( cr => new ConnectionRequestViewModelSecurity
                    {
                        ConnectionRequest = cr.ConnectionRequest,
                        ConnectionType = cr.ConnectionType,
                        Id = cr.Id,
                        ConnectionOpportunityId = cr.ConnectionOpportunityId,
                        ConnectionTypeId = cr.ConnectionTypeId,
                        PlacementGroupId = cr.PlacementGroupId,
                        PlacementGroupRoleId = cr.PlacementGroupRoleId,
                        PlacementGroupMemberStatus = cr.PlacementGroupMemberStatus,
                        PlacementGroupRoleName = cr.PlacementGroupRoleName,
                        Comments = cr.Comments,
                        StatusId = cr.StatusId,
                        PersonId = cr.PersonId,
                        PersonAliasId = cr.PersonAliasId,
                        PersonEmail = cr.PersonEmail,
                        PersonNickName = cr.PersonNickName,
                        PersonLastName = cr.PersonLastName,
                        PersonPhotoId = cr.PersonPhotoId,
                        PersonPhones = cr.PersonPhones,
                        CampusId = cr.CampusId,
                        CampusName = cr.CampusName,
                        Campus = cr.Campus,
                        CampusCode = cr.CampusCode,
                        ConnectorPersonNickName = cr.ConnectorPersonNickName,
                        ConnectorPersonLastName = cr.ConnectorPersonLastName,
                        ConnectorPersonId = cr.ConnectorPersonId,
                        ConnectorPhotoId = cr.ConnectorPhotoId,
                        ConnectorPersonAliasId = cr.ConnectorPersonAliasId,
                        ActivityCount = cr.ActivityCount,
                        DateOpened = cr.DateOpened,
                        GroupName = cr.GroupName,
                        StatusName = cr.StatusName,
                        StatusHighlightColor = cr.StatusHighlightColor,
                        IsStatusCritical = cr.IsStatusCritical,
                        IsAssignedToYou = cr.IsAssignedToYou,
                        IsCritical = cr.IsCritical,
                        IsIdle = cr.IsIdle,
                        IsUnassigned = cr.IsUnassigned,
                        ConnectionState = cr.ConnectionState,
                        LastActivityTypeName = cr.LastActivityTypeName,
                        Order = cr.Order,
                        LastActivityTypeId = cr.LastActivityTypeId,
                        LastActivityDate = cr.LastActivityDate,
                        FollowupDate = cr.FollowupDate,
                        UserHasDirectAccess = campusIdQuery.Contains( null ) || // Global campus connector
                                campusIdQuery.Contains( cr.CampusId ) || // In a connector group of the appropriate campus
                                cr.ConnectorPersonAliasId == currentPersonAliasId,
                        CanCurrentUserEdit = campusIdQuery.Contains( null ) || // Global campus connector
                                campusIdQuery.Contains( cr.CampusId ) || // In a connector group of the appropriate campus
                                cr.ConnectorPersonAliasId == currentPersonAliasId
                    } )
                    .Where( r =>
                        campusIdQuery.Contains( null ) || // Global campus connector
                        campusIdQuery.Contains( r.CampusId ) // In a connector group of the appropriate campus
                );
            }

            // Sort by the selected sorting property
            switch ( args.SortProperty )
            {
                case ConnectionRequestViewModelSortProperty.Requestor:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.PersonLastName )
                        .ThenBy( cr => cr.PersonNickName )
                        .ThenBy( cr => cr.Order )
                        .ThenBy( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.RequestorDesc:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderByDescending( cr => cr.PersonLastName )
                        .ThenByDescending( cr => cr.PersonNickName )
                        .ThenByDescending( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.Connector:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.ConnectorPersonLastName )
                        .ThenBy( cr => cr.ConnectorPersonNickName )
                        .ThenBy( cr => cr.Order )
                        .ThenBy( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.ConnectorDesc:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderByDescending( cr => cr.ConnectorPersonLastName )
                        .ThenByDescending( cr => cr.ConnectorPersonNickName )
                        .ThenByDescending( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.DateAdded:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.DateOpened )
                        .ThenBy( cr => cr.Order )
                        .ThenBy( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.DateAddedDesc:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderByDescending( cr => cr.DateOpened )
                        .ThenByDescending( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.LastActivity:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.LastActivityDate )
                        .ThenBy( cr => cr.Order )
                        .ThenBy( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.LastActivityDesc:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderByDescending( cr => cr.LastActivityDate )
                        .ThenByDescending( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.Campus:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.CampusName )
                        .ThenBy( cr => cr.Order )
                        .ThenBy( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.CampusDesc:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderByDescending( cr => cr.CampusName )
                        .ThenByDescending( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.Group:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.GroupName )
                        .ThenBy( cr => cr.Order )
                        .ThenBy( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.GroupDesc:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderByDescending( cr => cr.GroupName )
                        .ThenByDescending( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
                case ConnectionRequestViewModelSortProperty.Order:
                default:
                    connectionRequestsQuery = connectionRequestsQuery
                        .OrderBy( cr => cr.Order )
                        .ThenByDescending( cr => cr.Id );
                    break;
            }

            return connectionRequestsQuery;
        }

        /// <summary>
        /// Gets the global connector group campus ids.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="currentPersonId">The current person identifier.</param>
        /// <returns></returns>
        private IQueryable<int?> GetGlobalConnectorGroupCampusIds( int connectionOpportunityId, int? currentPersonId )
        {
            var rockContext = Context as RockContext;
            var connectionOpportunityConnectorGroupService = new ConnectionOpportunityConnectorGroupService( rockContext );
            return connectionOpportunityConnectorGroupService.Queryable()
                .AsNoTracking()
                .Where( cocg =>
                    cocg.ConnectionOpportunityId == connectionOpportunityId &&
                    cocg.ConnectorGroup.Members.Any( m => m.GroupMemberStatus == GroupMemberStatus.Active && m.PersonId == currentPersonId ) )
                .Select( cocg => cocg.CampusId );
        }

        /// <summary>
        /// Determines whether the currentPerson is authorized to edit the specified connection request.
        /// </summary>
        /// <param name="connectionRequest">The connection request.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>
        ///   <c>true</c> if the currentPerson is authorized to edit the specified connection request; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthorizedToEdit( ConnectionRequest connectionRequest, Person currentPerson )
        {
            var connectionOpportunity = connectionRequest.ConnectionOpportunity;
            var isConnectionRequestSecurityEnabled = connectionOpportunity.ConnectionType.EnableRequestSecurity;
            var canEditAllRequests = currentPerson != null
                                        && connectionOpportunity.IsAuthorized( Authorization.EDIT, currentPerson )
                                        && !isConnectionRequestSecurityEnabled;

            if ( canEditAllRequests )
            {
                return true;
            }

            // There are some scenarios where the current person can see the request even if the permissions say otherwise:
            if ( !isConnectionRequestSecurityEnabled )
            {
                // 1) Connection Request Security is not enabled, and the person is in a global (no campus) connector group
                // 2) Connection Request Security is not enabled, and the person is in the campus specific connector group
                var campusIdQuery = GetGlobalConnectorGroupCampusIds( connectionOpportunity.Id, currentPerson.Id );
                if ( campusIdQuery.Contains( null ) || // Global campus connector
                    campusIdQuery.Contains( connectionRequest.CampusId ) ) // In a connector group of the appropriate campus
                {
                    return true;
                }
            }

            // 3) The person is assigned to the request or the request security allows it and the connection type has EnableRequestSecurity
            return ( connectionRequest.ConnectorPersonAlias != null && connectionRequest.ConnectorPersonAlias.PersonId == currentPerson.Id )
                    || connectionRequest.IsAuthorized( Authorization.EDIT, currentPerson );
        }

        /// <summary>
        /// Gets the connection status query.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <returns></returns>
        public IOrderedQueryable<ConnectionStatus> GetConnectionStatusQuery( int connectionOpportunityId )
        {
            var rockContext = Context as RockContext;
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );

            return connectionOpportunityService.Queryable()
                .AsNoTracking()
                .Where( co => co.Id == connectionOpportunityId )
                .SelectMany( co => co.ConnectionType.ConnectionStatuses )
                .Where( cs => cs.IsActive )
                .OrderBy( cs => cs.Order )
                .ThenByDescending( cs => cs.IsDefault )
                .ThenBy( cs => cs.Name );
        }

        /// <summary>
        /// Gets the status icon HTML.
        /// </summary>
        /// <returns></returns>
        private string GetStatusIconHtml( ConnectionRequestViewModel viewModel, ConnectionTypeCache connectionType, string template )
        {
            if ( viewModel == null || connectionType == null || template.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var mergeFields = new Dictionary<string, object>();

            var connectionRequestStatusIcons = new
            {
                viewModel.IsAssignedToYou,
                viewModel.IsCritical,
                viewModel.IsIdle,
                viewModel.IsUnassigned
            };

            if ( LavaService.RockLiquidIsEnabled )
            {
                mergeFields.Add( "ConnectionRequestStatusIcons", DotLiquid.Hash.FromAnonymousObject( connectionRequestStatusIcons ) );
            }
            else
            {
                mergeFields.Add( "ConnectionRequestStatusIcons", connectionRequestStatusIcons );
            }

            mergeFields.Add( "IdleTooltip", string.Format( "Idle (no activity in {0} days)", connectionType.DaysUntilRequestIdle ) );
            return template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentException">An args object is required</exception>
        private void ValidateArgs( ConnectionRequestViewModelQueryArgs args )
        {
            if ( args == null )
            {
                throw new ArgumentException( "An args object is required" );
            }
        }

        /// <summary>
        /// Creates a The <see cref="ConnectionRequest"/> from the provided input. If for some reason, the Connection Request
        /// was not able to be created, null would be returned.
        /// </summary>
        /// <param name="connectionOpportunityId">The Connection Opportunity Id of the Connection Request.</param>
        /// <param name="personAliasId">The Person Alias Id of the Connection Request.</param>
        /// <param name="campusId">The optional Campus Id which would determine which campus the Connection Request should be
        /// linked to. If no campus is provided, then it defaults to the Main campus of the person linked to the personAliasId.</param>
        /// <param name="status">The optional Status of the Connection Request. If not provided, it will default to the
        /// provided Connection Opportunity's default status.</param>
        /// <param name="rockContext">An optional <see cref="RockContext" />. A new context would be created if not provided.</param>
        /// <returns></returns>
        internal ConnectionRequest CreateConnectionRequestWithDefaultConnector( int connectionOpportunityId, int personAliasId, int? campusId = null, ConnectionStatus status = null, RockContext rockContext = null )
        {
            // create a new RockContent if null was provided
            rockContext = rockContext ?? new RockContext();

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            status = status ?? connectionOpportunityService.GetStatuses( connectionOpportunityId )
                    .FirstOrDefault( cs => cs.IsDefault )
                        ?? connectionOpportunityService.GetStatuses( connectionOpportunityId )
                    .FirstOrDefault();
            if ( status == null )
            {
                return null;
            }

            var opportunity = connectionOpportunityService.Get( connectionOpportunityId );
            campusId = campusId ?? new PersonAliasService( rockContext )
                .Get( personAliasId ).Person?.PrimaryCampusId;

            return new ConnectionRequest
            {
                ConnectionOpportunityId = opportunity.Id,
                PersonAliasId = personAliasId,
                ConnectionStatusId = status.Id,
                ConnectionState = ConnectionState.Active,
                CampusId = campusId,
                ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId )
            };
        }

        #endregion Connection Board Helper Methods
    }
}
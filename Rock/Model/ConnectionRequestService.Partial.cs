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
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Connection Request Service
    /// </summary>
    public partial class ConnectionRequestService
    {
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
        /// Doeses the status change cause workflows.
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
                }
            }

            return connectionStatusViewModels;
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

            var connectionOpportunity = Queryable()
                .AsNoTracking()
                .Where( cr => cr.Id == connectionRequestId )
                .Select( cr => cr.ConnectionOpportunity )
                .FirstOrDefault();

            var query = GetConnectionRequestViewModelQuery( currentPersonAliasId, connectionOpportunity.Id, args );
            var viewModel = query.FirstOrDefault();

            if ( viewModel == null )
            {
                return null;
            }

            var connectionType = ConnectionTypeCache.Get( viewModel.ConnectionTypeId );
            viewModel.CanConnect = CanConnect( viewModel, connectionOpportunity, connectionType );

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
            ValidateArgs( args );

            var currentDateTime = RockDateTime.Now;
            var midnightToday = RockDateTime.Today.AddDays( 1 );

            var rockContext = Context as RockContext;
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var connectionOpportunity = connectionOpportunityService.Get( connectionOpportunityId );
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
                    CanCurrentUserEdit = canViewAllRequests,
                } );

            // Filter by connector
            if ( args.ConnectionRequestId.HasValue )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => cr.Id == args.ConnectionRequestId );
            }

            // Filter by connector
            if ( args.ConnectorPersonAliasId.HasValue )
            {
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => cr.ConnectorPersonAliasId == args.ConnectorPersonAliasId );
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
                connectionRequestsQuery = connectionRequestsQuery.Where( cr => cr.PersonAliasId == args.RequesterPersonAliasId.Value );
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

            return connectionRequestsQuery.Select( cr => new ConnectionRequestViewModel
            {
                Id = cr.Id,
                ConnectionOpportunityId = cr.ConnectionOpportunityId,
                ConnectionTypeId = cr.ConnectionTypeId,
                PlacementGroupId = cr.PlacementGroupId,
                PlacementGroupRoleId = cr.PlacementGroupRoleId,
                PlacementGroupMemberStatus = cr.PlacementGroupMemberStatus,
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
            return (connectionRequest.ConnectorPersonAlias != null && connectionRequest.ConnectorPersonAlias.PersonId == currentPerson.Id )
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

            mergeFields.Add( "ConnectionRequestStatusIcons", DotLiquid.Hash.FromAnonymousObject( connectionRequestStatusIcons ) );
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

        #endregion Connection Board Helper Methods
    }

    #region Models

    /// <summary>
    /// View Model Query Args
    /// </summary>
    public sealed class ConnectionRequestViewModelQueryArgs
    {
        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the connector person alias identifier.
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the minimum date.
        /// </summary>
        public DateTime? MinDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum date.
        /// </summary>
        public DateTime? MaxDate { get; set; }

        /// <summary>
        /// Gets or sets the requester person alias identifier.
        /// </summary>
        public int? RequesterPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the status ids.
        /// </summary>
        public List<int> StatusIds { get; set; }

        /// <summary>
        /// Gets or sets the connection states.
        /// </summary>
        public List<ConnectionState> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the last activity type ids.
        /// </summary>
        public List<int> LastActivityTypeIds { get; set; }

        /// <summary>
        /// Gets or sets the sort property.
        /// </summary>
        public ConnectionRequestViewModelSortProperty? SortProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is future follow up past due only.
        /// </summary>
        public bool IsFutureFollowUpPastDueOnly { get; set; }

        /// <summary>
        /// Gets or sets the connection request identifier.
        /// </summary>
        /// <value>
        /// The connection request identifier.
        /// </value>
        public int? ConnectionRequestId { get; set; }
    }

    /// <summary>
    /// Connection Status View Model (columns)
    /// </summary>
    public sealed class ConnectionStatusViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the actual number of requests, since the Requests property may be a limited set.
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Gets or sets the requests. This may be a limited sub-set of the actual set of requests.
        /// </summary>
        public List<ConnectionRequestViewModel> Requests { get; set; }
    }

    /// <summary>
    /// This model is used to include the ConnectionRequest and ConnectionType so security can be checked if needed.
    /// </summary>
    /// <seealso cref="Rock.Model.ConnectionRequestViewModel" />
    internal class ConnectionRequestViewModelSecurity : ConnectionRequestViewModel
    {
        public ConnectionRequest ConnectionRequest { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public bool UserHasDirectAccess { get; set; }
    }

    /// <summary>
    /// Connection Request View Model (cards)
    /// </summary>
    public class ConnectionRequestViewModel
    {
        #region Properties

        /// <summary>
        /// Connection Request Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the placement group identifier.
        /// </summary>
        public int? PlacementGroupId { get; set; }

        /// <summary>
        /// Gets or sets the placement group role identifier.
        /// </summary>
        public int? PlacementGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the placement group member status.
        /// </summary>
        public GroupMemberStatus? PlacementGroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Requester Person Id
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the person email.
        /// </summary>
        public string PersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonNickName { get; set; }

        /// <summary>
        /// Person Last Name
        /// </summary>
        public string PersonLastName { get; set; }

        /// <summary>
        /// Person Photo Id
        /// </summary>
        public int? PersonPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the person phones.
        /// </summary>
        public List<PhoneViewModel> PersonPhones { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Campus Name
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Campus Code
        /// </summary>
        public string CampusCode { get; set; }

        /// <summary>
        /// Gets or sets the connector photo identifier.
        /// </summary>
        public int? ConnectorPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector person.
        /// </summary>
        public string ConnectorPersonNickName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the connector person.
        /// </summary>
        public string ConnectorPersonLastName { get; set; }

        /// <summary>
        /// Connector Person Id
        /// </summary>
        public int? ConnectorPersonId { get; set; }

        /// <summary>
        /// Connector person alias id
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Connection Status Id
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity identifier.
        /// </summary>
        public int ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the connection type identifier.
        /// </summary>
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the status.
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Gets or sets the color of the status highlight.
        /// </summary>
        public string StatusHighlightColor
        {
            get
            {
                return _statusHighlightColor.IsNullOrWhiteSpace() ? ConnectionStatus.DefaultHighlightColor : _statusHighlightColor;
            }

            set
            {
                _statusHighlightColor = value;
            }
        }

        private string _statusHighlightColor = null;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is status critical.
        /// </summary>
        public bool IsStatusCritical { get; set; }

        /// <summary>
        /// Activity count
        /// </summary>
        public int ActivityCount { get; set; }

        /// <summary>
        /// Last activity date
        /// </summary>
        public DateTime? LastActivityDate { get; set; }

        /// <summary>
        /// Date Opened
        /// </summary>
        public DateTime? DateOpened { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the last activity.
        /// </summary>
        public string LastActivityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the last activity type identifier.
        /// </summary>
        public int? LastActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the state of the connection.
        /// </summary>
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is assigned to you.
        /// </summary>
        public bool IsAssignedToYou { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is critical.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is idle.
        /// </summary>
        public bool IsIdle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unassigned.
        /// </summary>
        public bool IsUnassigned { get; set; }

        /// <summary>
        /// Gets or sets the followup date.
        /// </summary>
        public DateTime? FollowupDate { get; set; }

        /// <summary>
        /// Gets or sets the status icons HTML.
        /// </summary>
        public string StatusIconsHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can connect.
        /// </summary>
        public bool CanConnect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current person can edit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current person can edit]; otherwise, <c>false</c>.
        /// </value>
        public bool CanCurrentUserEdit { get; set; }
        #endregion Properties

        #region Computed

        /// <summary>
        /// The state label
        /// </summary>
        public string StateLabel
        {
            get
            {
                var css = string.Empty;

                switch ( ConnectionState )
                {
                    case ConnectionState.Active:
                        css = "success";
                        break;
                    case ConnectionState.Inactive:
                        css = "danger";
                        break;
                    case ConnectionState.FutureFollowUp:
                        css = ( FollowupDate.HasValue && FollowupDate.Value > RockDateTime.Today ) ? "info" : "danger";
                        break;
                    case ConnectionState.Connected:
                        css = "success";
                        break;
                }

                var text = ConnectionState.ConvertToString();

                if ( ConnectionState == ConnectionState.FutureFollowUp && FollowupDate.HasValue )
                {
                    text += string.Format( " ({0})", FollowupDate.Value.ToShortDateString() );
                }

                return string.Format( "<span class='label label-{0}'>{1}</span>", css, text );
            }
        }

        /// <summary>
        /// Gets or sets the status label.
        /// </summary>
        public string StatusLabelClass
        {
            get
            {
                return IsStatusCritical ? "warning" : "info";
            }
        }

        /// <summary>
        /// Activity Count Text
        /// </summary>
        public string ActivityCountText
        {
            get
            {
                if ( ActivityCount == 1 )
                {
                    return "1 Activity";
                }

                return string.Format( "{0} Activities", ActivityCount );
            }
        }

        /// <summary>
        /// Gets the last activity text.
        /// </summary>
        public string LastActivityText
        {
            get
            {
                if ( !LastActivityTypeName.IsNullOrWhiteSpace() && LastActivityDate.HasValue )
                {
                    return string.Format(
                        "{0} (<span class='small'>{1}</small>)",
                        LastActivityTypeName,
                        LastActivityDate.ToRelativeDateString() );
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Connector Person Fullname
        /// </summary>
        public string ConnectorPersonFullname
        {
            get
            {
                return string.Format( "{0} {1}", ConnectorPersonNickName, ConnectorPersonLastName );
            }
        }

        /// <summary>
        /// Person Fullname
        /// </summary>
        public string PersonFullname
        {
            get
            {
                return string.Format( "{0} {1}", PersonNickName, PersonLastName );
            }
        }

        /// <summary>
        /// Person Photo Html
        /// </summary>
        public string PersonPhotoUrl
        {
            get
            {
                if ( PersonPhotoId.HasValue )
                {
                    return string.Format( "/GetImage.ashx?id={0}", PersonPhotoId.Value );
                }
                else
                {
                    Person person = new PersonService( new RockContext() ).Get( PersonId );
                    return Person.GetPersonPhotoUrl( person.Id, person.PhotoId, person.Age, person.Gender, person.RecordTypeValue?.Guid, person.AgeClassification );
                }
            }
        }

        /// <summary>
        /// Gets the connector photo URL.
        /// </summary>
        public string ConnectorPhotoUrl
        {
            get
            {
                if ( ConnectorPhotoId.HasValue )
                {
                    return string.Format( "/GetImage.ashx?id={0}", ConnectorPhotoId.Value );
                }
                else
                {
                    if ( ConnectorPersonId.HasValue )
                    {
                        Person person = new PersonService( new RockContext() ).Get( ConnectorPersonId.Value );
                        return Person.GetPersonPhotoUrl( person.Id, person.PhotoId, person.Age, person.Gender, person.RecordTypeValue?.Guid, person.AgeClassification );
                    }
                    else
                    {
                        return "/Assets/Images/person-no-photo-unknown.svg";
                    }
                }
            }
        }

        /// <summary>
        /// Has Campus
        /// </summary>
        public string CampusHtml
        {
            get
            {
                if ( CampusCode.IsNullOrWhiteSpace() )
                {
                    return string.Empty;
                }

                return string.Format(
                    @"<span class=""badge badge-info font-weight-normal"" title=""{0}"">{1}</span>",
                    CampusName,
                    CampusCode );
            }
        }

        /// <summary>
        /// Days Since Opening
        /// </summary>
        public int? DaysSinceOpening
        {
            get
            {
                if ( !DateOpened.HasValue )
                {
                    return null;
                }

                return ( RockDateTime.Today - DateOpened.Value.Date ).Days;
            }
        }

        /// <summary>
        /// Days Since Opening Short Text
        /// </summary>
        public string DaysSinceOpeningShortText
        {
            get
            {
                if ( !DaysSinceOpening.HasValue )
                {
                    return "No Opening";
                }

                return string.Format( "{0}d", DaysSinceOpening.Value );
            }
        }

        /// <summary>
        /// Gets the days or weeks since opening text.
        /// </summary>
        public string DaysOrWeeksSinceOpeningText
        {
            get
            {
                if ( !DaysSinceOpening.HasValue )
                {
                    return "No Opening";
                }

                if ( DaysSinceOpening.Value == 1 )
                {
                    return "1 day";
                }

                if ( DaysSinceOpening.Value < 14 )
                {
                    return string.Format( "{0} days", DaysSinceOpening.Value );
                }

                return string.Format( "{0} weeks", DaysSinceOpening.Value / 7 );
            }
        }

        /// <summary>
        /// Days Since Opening Long Text
        /// </summary>
        public string DaysSinceOpeningLongText
        {
            get
            {
                if ( !DaysSinceOpening.HasValue )
                {
                    return "No Opening";
                }

                if ( DaysSinceOpening.Value == 1 )
                {
                    return string.Format( "Opened 1 Day Ago ({0})", DateOpened.Value.ToShortDateString() );
                }

                return string.Format( "Opened {0} Days Ago ({1})", DaysSinceOpening.Value, DateOpened.Value.ToShortDateString() );
            }
        }

        /// <summary>
        /// Days Since Last Activity
        /// </summary>
        public int? DaysSinceLastActivity
        {
            get
            {
                if ( !LastActivityDate.HasValue )
                {
                    return null;
                }

                return ( RockDateTime.Now - LastActivityDate.Value ).Days;
            }
        }

        /// <summary>
        /// Days Since Last Activity Short Text
        /// </summary>
        public string DaysSinceLastActivityShortText
        {
            get
            {
                if ( !DaysSinceLastActivity.HasValue )
                {
                    return "No Activity";
                }

                return string.Format( "{0}d", DaysSinceLastActivity.Value );
            }
        }

        /// <summary>
        /// Days Since Last Activity Long Text
        /// </summary>
        public string DaysSinceLastActivityLongText
        {
            get
            {
                if ( !DaysSinceLastActivity.HasValue )
                {
                    return "No Activity";
                }

                if ( DaysSinceLastActivity.Value == 1 )
                {
                    return "1 Day Since Last Activity";
                }

                return string.Format( "{0} Days Since Last Activity", DaysSinceLastActivity.Value );
            }
        }

        #endregion Computed

        /// <summary>
        /// Phone View Model
        /// </summary>
        public sealed class PhoneViewModel
        {
            /// <summary>
            /// Gets or sets the type of the phone.
            /// </summary>
            public string PhoneType { get; set; }

            /// <summary>
            /// Gets or sets the formatted phone number.
            /// </summary>
            public string FormattedPhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is messaging enabled.
            /// </summary>
            public bool IsMessagingEnabled { get; set; }
        }
    }

    /// <summary>
    /// Workflow Check View Model
    /// </summary>
    public sealed class WorkflowCheckViewModel
    {
        /// <summary>
        /// Converts to statusname.
        /// </summary>
        public string ToStatusName { get; set; }

        /// <summary>
        /// Gets or sets the name of from status.
        /// </summary>
        public string FromStatusName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [does cause workflows].
        /// </summary>
        public bool DoesCauseWorkflows { get; set; }
    }

    #endregion Models

    #region Enums

    /// <summary>
    /// The sort property
    /// </summary>
    public enum ConnectionRequestViewModelSortProperty
    {
        /// <summary>
        /// The requestor
        /// </summary>
        Requestor,

        /// <summary>
        /// The requestor desc
        /// </summary>
        RequestorDesc,

        /// <summary>
        /// The connector
        /// </summary>
        Connector,

        /// <summary>
        /// The connector desc
        /// </summary>
        ConnectorDesc,

        /// <summary>
        /// The date added
        /// </summary>
        DateAdded,

        /// <summary>
        /// The date added desc
        /// </summary>
        DateAddedDesc,

        /// <summary>
        /// The last activity
        /// </summary>
        LastActivity,

        /// <summary>
        /// The last activity desc
        /// </summary>
        LastActivityDesc,

        /// <summary>
        /// The order
        /// </summary>
        Order,

        /// <summary>
        /// The campus
        /// </summary>
        Campus,

        /// <summary>
        /// The campus desc
        /// </summary>
        CampusDesc,

        /// <summary>
        /// The group
        /// </summary>
        Group,

        /// <summary>
        /// The group desc
        /// </summary>
        GroupDesc,

        /// <summary>
        /// The status
        /// </summary>
        Status,

        /// <summary>
        /// The status desc
        /// </summary>
        StatusDesc,

        /// <summary>
        /// The state
        /// </summary>
        State,

        /// <summary>
        /// The state desc
        /// </summary>
        StateDesc
    }

    #endregion Enums
}
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
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetailV2;
using Rock.Common.Mobile.ViewModel;
using Rock.Data;
using Rock.Enums.Connection;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

using MobileConnectionState = Rock.Common.Mobile.Enums.ConnectionState;
using MobileDueStatus = Rock.Common.Mobile.Enums.DueStatus;
using MobileGroupMemberStatus = Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetailV2.GroupMemberStatus;
using MobileMeetsGroupRequirement = Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetailV2.MeetsGroupRequirement;
using MobileActivityEntryType = Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetailV2.ActivityEntryType;
using MobileSystemUpdateType = Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetailV2.SystemUpdateType;

using ConnectionState = Rock.Model.ConnectionState;
using GroupMemberStatus = Rock.Model.GroupMemberStatus;
using MeetsGroupRequirement = Rock.Model.MeetsGroupRequirement;

namespace Rock.Blocks.Mobile.Connection
{
    /// <summary>
    /// Displays a single connection request for viewing and editing: the
    /// requester header, status / state, connector, campus, request source,
    /// placement group, comments, custom attributes, manual workflows,
    /// celebration, reminders and the activity count, with per-field edits,
    /// targeted status / state / connector quick-actions, and a Connect
    /// action. Adapted from the web Connections Hub docked request panel.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Request Detail V2" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays a single connection request for viewing and editing." )]
    [IconCssClass( "ti ti-id" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Person Profile Page",
        Description = "Page to link to when the requester is tapped. The requester's PersonGuid is passed.",
        IsRequired = false,
        Key = AttributeKey.PersonProfilePage,
        Order = 0 )]

    [LinkedPage( "Group Detail Page",
        Description = "Page to link to when the placement group is tapped. The group's Guid is passed.",
        IsRequired = false,
        Key = AttributeKey.GroupDetailPage,
        Order = 1 )]

    [LinkedPage( "Workflow Page",
        Description = "Page to link to when a launched manual workflow needs an interactive entry form. The workflow Guid is passed.",
        IsRequired = false,
        Key = AttributeKey.WorkflowPage,
        Order = 2 )]

    [LinkedPage( "Reminder Page",
        Description = "Page that hosts the Reminder block, opened in a cover sheet by the Reminder quick-action. When empty, the Reminder card is hidden.",
        IsRequired = false,
        Key = AttributeKey.ReminderPage,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "8B53B246-526F-4B3E-AF5B-4C36763E9DC9" )]
    [Rock.SystemGuid.BlockTypeGuid( "74DDC1A2-2025-4072-8F47-DF7A5A76CF83" )]
    public class ConnectionRequestDetailV2 : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string WorkflowPage = "WorkflowPage";
            public const string ReminderPage = "ReminderPage";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 20 );

        #endregion

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            var campuses = CampusCache.All( false )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .Select( c => new ListItemViewModel
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .ToList();

            var reminderPageGuid = GetAttributeValue( AttributeKey.ReminderPage ).AsGuidOrNull();

            return new Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetailV2.Configuration
            {
                Campuses = campuses,
                PersonProfilePageGuid = GetAttributeValue( AttributeKey.PersonProfilePage ).AsGuidOrNull(),
                GroupDetailPageGuid = GetAttributeValue( AttributeKey.GroupDetailPage ).AsGuidOrNull(),
                WorkflowPageGuid = GetAttributeValue( AttributeKey.WorkflowPage ).AsGuidOrNull(),
                ReminderPageGuid = reminderPageGuid,
                AreRemindersConfigured = reminderPageGuid.HasValue
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Loads the full display detail for one connection request, plus the
        /// celebration text and the current person's edit permission.
        /// </summary>
        /// <param name="request">The request carrying the connection request IdKey.</param>
        /// <returns>The request display detail.</returns>
        [BlockAction]
        public BlockActionResult GetRequestDetail( GetRequestDetailRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            // Read auth is a single VIEW check on the request entity. Rock's
            // auth inheritance honors ConnectionType.EnableRequestSecurity for
            // free (request-level when on, opportunity/type inheritance when
            // off), so no explicit branching on the flag is needed here.
            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this connection request." );
            }

            if ( !connectionRequest.ConnectionOpportunity.IsActive || !connectionRequest.ConnectionOpportunity.ConnectionType.IsActive )
            {
                return ActionBadRequest( $"The specified {ConnectionOpportunity.FriendlyTypeName} is not active." );
            }

            connectionRequest.LoadAttributes( RockContext );

            return ActionOk( BuildDetailResponse( connectionRequest ) );
        }

        /// <summary>
        /// Lazily loads the edit option lists for a connection request, fetched
        /// when the first field editor opens.
        /// </summary>
        /// <param name="request">The request carrying the connection request IdKey.</param>
        /// <returns>The edit option lists and the type's edit flags.</returns>
        [BlockAction]
        public BlockActionResult GetEditOptions( GetEditOptionsRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this connection request." );
            }

            var connectionType = connectionRequest.ConnectionOpportunity.ConnectionType;
            var canEdit = CanEditConnectionRequest( connectionRequest, out _ );

            var options = new GetEditOptionsResponseBag
            {
                CanEdit = canEdit,
                Statuses = connectionType.ConnectionStatuses
                    .OrderBy( s => s.Order )
                    .ThenBy( s => s.Name )
                    .Select( s => new ConnectionStatusItemBag
                    {
                        Value = s.Guid,
                        Name = s.Name,
                        Color = s.HighlightColor,
                        Order = s.Order,
                        IsNoteRequiredOnCompletion = s.IsNoteRequiredOnCompletion,
                        IsDefault = s.IsDefault,
                        IsDisabled = false
                    } )
                    .ToList(),
                States = GetSelectableStates( connectionType ),
                Connectors = GetAvailableConnectors( connectionRequest ),
                ActivityTypes = GetActivityTypes( connectionType ),
                PlacementGroups = GetRequestPlacementGroups( connectionRequest ),
                RequestSources = connectionType.ConnectionTypeSources
                    .OrderBy( s => s.Name )
                    .Select( s => new ListItemViewModel
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name
                    } )
                    .ToList(),
                AreCelebrationsEnabled = connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Celebration ),
                IsFutureFollowUpEnabled = connectionType.EnableFutureFollowup,
                RequiresPlacementGroupToConnect = connectionType.RequiresPlacementGroupToConnect,
                IsSequentialStatusMode = false
            };

            return ActionOk( options );
        }

        /// <summary>
        /// Gets the group-member attributes that apply to a chosen placement
        /// group and role, with the request's saved member-attribute values
        /// seeded in. Called by the Placement Group editor as the group / role
        /// selection changes. Adapted from the existing block's
        /// GetPlacementGroupMemberAttributesAndValues.
        /// </summary>
        /// <param name="request">The payload carrying the request IdKey plus the chosen group + role.</param>
        /// <returns>The member attributes to edit.</returns>
        [BlockAction]
        public BlockActionResult GetPlacementGroupMemberAttributes( GetPlacementGroupMemberAttributesRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            // Validate the chosen group + role are a real placement option for
            // the request's opportunity.
            var validPlacementGroups = GetRequestPlacementGroups( connectionRequest );
            var placementGroup = validPlacementGroups.FirstOrDefault( g => g.Value == request.GroupGuid );
            var placementRole = placementGroup?.Roles.FirstOrDefault( r => r.Value == request.GroupMemberRoleGuid );

            if ( placementGroup == null || placementRole == null )
            {
                return ActionBadRequest( "Invalid placement group selection." );
            }

            // Resolve the group + role identifiers so an empty group member can
            // be created to load the member attributes from.
            var groupInfo = new GroupService( RockContext ).Queryable()
                .Where( g => g.Guid == request.GroupGuid )
                .Select( g => new { g.Id, g.GroupTypeId } )
                .FirstOrDefault();

            if ( groupInfo == null )
            {
                return ActionBadRequest( "Invalid placement group selection." );
            }

            var groupMemberRoleId = GroupTypeCache.Get( groupInfo.GroupTypeId )?.Roles
                .FirstOrDefault( r => r.Guid == request.GroupMemberRoleGuid )
                ?.Id;

            if ( !groupMemberRoleId.HasValue )
            {
                return ActionBadRequest( "Invalid placement group selection." );
            }

            var groupMember = new GroupMember
            {
                GroupId = groupInfo.Id,
                GroupRoleId = groupMemberRoleId.Value
            };

            groupMember.LoadAttributes( RockContext );

            // Restore the request's saved member-attribute values so the editor
            // opens with the current values when the assigned group + role are
            // reselected.
            var savedMemberAttributeValues = connectionRequest.AssignedGroupMemberAttributeValues?.FromJsonOrNull<Dictionary<string, string>>();

            if ( savedMemberAttributeValues != null )
            {
                foreach ( var item in savedMemberAttributeValues )
                {
                    groupMember.SetAttributeValue( item.Key, item.Value );
                }
            }

            return ActionOk( new GetPlacementGroupMemberAttributesResponseBag
            {
                Attributes = GetClientAttributeValuesForView( groupMember )
            } );
        }

        /// <summary>
        /// Saves the editable fields of a connection request that do not have a
        /// dedicated quick-action (campus, request source, comments, placement
        /// group, custom attributes). Carries the full field set; unedited
        /// fields pass through their current values.
        /// </summary>
        /// <param name="request">The save payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult SaveRequest( SaveConnectionRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            connectionRequest.LoadAttributes( RockContext );

            var connectionStatusService = new ConnectionStatusService( RockContext );
            var personAliasService = new PersonAliasService( RockContext );
            var connectionType = connectionRequest.ConnectionOpportunity.ConnectionType;
            var originalConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;

            // Basic values.
            connectionRequest.Comments = request.Comments;

            // Status (must belong to the request's type). A note is required
            // when leaving a status that requires one on completion (web
            // parity with ChangeRequestStatus, replicated so a bulk save
            // cannot bypass the guard).
            var status = connectionStatusService.Queryable()
                .FirstOrDefault( s => s.ConnectionTypeId == connectionType.Id && s.Guid == request.StatusGuid );

            if ( status == null )
            {
                return ActionBadRequest( "Invalid connection status." );
            }

            var isStatusChanging = connectionRequest.ConnectionStatusId != status.Id;

            if ( isStatusChanging
                && connectionRequest.ConnectionStatus != null
                && connectionRequest.ConnectionStatus.IsNoteRequiredOnCompletion
                && request.StatusHistoryNote.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A note is required." );
            }

            connectionRequest.ConnectionStatusId = status.Id;

            if ( isStatusChanging && request.StatusHistoryNote.IsNotNullOrWhiteSpace() )
            {
                connectionRequest.ConnectionStatusHistoryNote = request.StatusHistoryNote;
            }

            // State + follow-up date. Completion (Connected) is handled by the
            // UpdateState action, not here; the per-field State editor never
            // sends Connected through SaveRequest. Reject it server-side so a
            // crafted or buggy client cannot persist Connected and bypass the
            // placement-group, group-requirement, group-member, and activity
            // handling that TryMarkRequestConnected performs (web parity with
            // AddConnectionRequestV2, which likewise refuses Connected).
            var newState = ( ConnectionState ) ( int ) request.State;

            if ( newState == ConnectionState.Connected )
            {
                return ActionBadRequest( "A request cannot be connected here. Use the Connect action." );
            }

            connectionRequest.ConnectionState = newState;

            if ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
            {
                if ( !request.FollowUpDate.HasValue )
                {
                    return ActionBadRequest( "A Follow-Up Date is required." );
                }

                connectionRequest.FollowupDate = request.FollowUpDate.Value.DateTime;
            }
            else
            {
                connectionRequest.FollowupDate = null;
            }

            // Campus.
            if ( request.CampusGuid.HasValue )
            {
                var campusId = CampusCache.GetId( request.CampusGuid.Value );

                if ( !campusId.HasValue )
                {
                    return ActionBadRequest( "Invalid campus." );
                }

                connectionRequest.CampusId = campusId;
            }
            else
            {
                connectionRequest.CampusId = null;
            }

            // Request source (a ConnectionTypeSource row of the request's type).
            if ( request.RequestSourceGuid.HasValue )
            {
                var sourceId = connectionType.ConnectionTypeSources
                    .Where( s => s.Guid == request.RequestSourceGuid.Value )
                    .Select( s => ( int? ) s.Id )
                    .FirstOrDefault();

                if ( !sourceId.HasValue )
                {
                    return ActionBadRequest( "Invalid request source." );
                }

                connectionRequest.ConnectionTypeSourceId = sourceId;
            }
            else
            {
                connectionRequest.ConnectionTypeSourceId = null;
            }

            // Connector (validated against the available connectors).
            if ( request.ConnectorPersonGuid.HasValue )
            {
                var isValidConnector = GetAvailableConnectors( connectionRequest )
                    .Any( c => c.PersonGuid == request.ConnectorPersonGuid.Value );

                var connectorPersonAliasId = personAliasService.GetPrimaryAliasId( request.ConnectorPersonGuid.Value );

                if ( !isValidConnector || !connectorPersonAliasId.HasValue )
                {
                    return ActionBadRequest( "Invalid connector." );
                }

                connectionRequest.ConnectorPersonAliasId = connectorPersonAliasId;
            }
            else
            {
                connectionRequest.ConnectorPersonAliasId = null;
            }

            // Placement group (group + role + status must all be present or all
            // absent).
            var setPlacementError = ApplyPlacementGroup( connectionRequest, request );

            if ( setPlacementError != null )
            {
                return setPlacementError;
            }

            // Custom attribute values.
            if ( request.AttributeValues != null )
            {
                connectionRequest.SetPublicAttributeValues( request.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
            }

            // Record the connector-assigned activity when the connector changed.
            if ( originalConnectorPersonAliasId != connectionRequest.ConnectorPersonAliasId )
            {
                var activity = CreateAssignedActivity( connectionRequest, RockContext );

                if ( activity != null )
                {
                    new ConnectionRequestActivityService( RockContext ).Add( activity );
                }
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                connectionRequest.SaveAttributeValues( RockContext );
            } );

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Targeted status change (the status pill quick-action). Validates the
        /// status belongs to the request's type and enforces note-required-on-
        /// completion. Ports the web ChangeRequestStatus.
        /// </summary>
        /// <param name="request">The status-change payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult ChangeStatus( ChangeStatusRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var newStatus = new ConnectionStatusService( RockContext ).Queryable()
                .FirstOrDefault( s => s.ConnectionTypeId == connectionRequest.ConnectionOpportunity.ConnectionTypeId
                    && s.Guid == request.ConnectionStatusGuid );

            if ( newStatus == null )
            {
                return ActionBadRequest( "Invalid connection status." );
            }

            // The web enforces the note requirement against the CURRENT status
            // before the change is applied.
            if ( connectionRequest.ConnectionStatus != null
                && connectionRequest.ConnectionStatus.IsNoteRequiredOnCompletion
                && request.Note.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A note is required." );
            }

            connectionRequest.ConnectionStatusId = newStatus.Id;
            connectionRequest.ConnectionStatusHistoryNote = request.Note;

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Targeted state update (the State row quick-action and the footer
        /// Connect button). Requires a follow-up date for Future Follow Up and
        /// runs the completion handling when transitioning to Connected. Ports
        /// the web UpdateRequestStates.
        /// </summary>
        /// <param name="request">The state-update payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult UpdateState( UpdateStateRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var newState = ( ConnectionState ) ( int ) request.ConnectionState;

            if ( newState == ConnectionState.FutureFollowUp && !request.FollowUpDate.HasValue )
            {
                return ActionBadRequest( "A Follow-Up Date is required." );
            }

            // Completing a request is a state change to Connected, with the
            // placement-group and manual-requirement handling.
            if ( newState == ConnectionState.Connected )
            {
                var connectionType = connectionRequest.ConnectionOpportunity.ConnectionType;

                // Web parity (ConnectionRequestService.CanConnect): an Inactive
                // request cannot be connected. The shell hides the Connect button
                // for this state, but guard here so a stale or crafted client
                // cannot connect a request that has been marked inactive.
                if ( connectionRequest.ConnectionState == ConnectionState.Inactive )
                {
                    return ActionBadRequest( "An inactive request cannot be connected." );
                }

                if ( connectionType.RequiresPlacementGroupToConnect && !connectionRequest.AssignedGroupId.HasValue )
                {
                    return ActionBadRequest( "This connection type requires a placement group before the request can be connected." );
                }

                var manualRequirementsMet = ( request.GroupMemberRequirements ?? new List<GroupMemberRequirementBag>() )
                    .ToDictionary(
                        r => r.GroupRequirementGuid,
                        r => r.GroupMemberRequirementState == MobileMeetsGroupRequirement.Meets
                            || r.GroupMemberRequirementState == MobileMeetsGroupRequirement.MeetsWithWarning );

                if ( !TryMarkRequestConnected( connectionRequest, manualRequirementsMet, RequestContext.CurrentPerson, RockContext, out var connectError ) )
                {
                    return ActionBadRequest( connectError );
                }

                return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
            }

            connectionRequest.ConnectionState = newState;

            if ( newState == ConnectionState.FutureFollowUp )
            {
                connectionRequest.FollowupDate = request.FollowUpDate.Value.DateTime;
            }
            else
            {
                connectionRequest.FollowupDate = null;
            }

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Targeted connector reassignment (the Connector row quick-action).
        /// </summary>
        /// <param name="request">The reassign payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult ReassignConnector( ReassignConnectorRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var originalConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;

            if ( request.ConnectorPersonGuid.HasValue )
            {
                var isValidConnector = GetAvailableConnectors( connectionRequest )
                    .Any( c => c.PersonGuid == request.ConnectorPersonGuid.Value );

                var connectorPersonAliasId = new PersonAliasService( RockContext ).GetPrimaryAliasId( request.ConnectorPersonGuid.Value );

                if ( !isValidConnector || !connectorPersonAliasId.HasValue )
                {
                    return ActionBadRequest( "Invalid connector." );
                }

                connectionRequest.ConnectorPersonAliasId = connectorPersonAliasId;
            }
            else
            {
                connectionRequest.ConnectorPersonAliasId = null;
            }

            if ( originalConnectorPersonAliasId != connectionRequest.ConnectorPersonAliasId )
            {
                var activity = CreateAssignedActivity( connectionRequest, RockContext );

                if ( activity != null )
                {
                    new ConnectionRequestActivityService( RockContext ).Add( activity );
                }
            }

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Reassigns the request to a different requester (the overflow Update
        /// Requestor action). Mirrors the web Connections Hub edit panel's
        /// required Requester person picker, which sets the request's
        /// PersonAliasId to the chosen person; the request always has a
        /// requester, so the person cannot be cleared.
        /// </summary>
        /// <param name="request">The update-requester payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult UpdateRequester( UpdateRequesterRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            if ( request.RequesterPersonGuid == Guid.Empty )
            {
                return ActionBadRequest( "A requester is required." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            // Resolve the chosen person to their primary alias - the requester is
            // stored as a PersonAlias, matching the web edit panel.
            var requesterPersonAliasId = new PersonAliasService( RockContext ).GetPrimaryAliasId( request.RequesterPersonGuid );

            if ( !requesterPersonAliasId.HasValue )
            {
                return ActionBadRequest( "Invalid requester." );
            }

            connectionRequest.PersonAliasId = requesterPersonAliasId.Value;

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Adds an activity to the request (the overflow Add Activity action and
        /// the Activity Feed New Activity action). Ports the single-request slice
        /// of the web AddActivityForRequests, including the optional person-note
        /// creation.
        /// </summary>
        /// <param name="request">The add-activity payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult AddActivity( AddActivityRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var activityType = new ConnectionActivityTypeService( RockContext ).Get( request.ActivityTypeGuid );

            // The activity type must be active and either global (no specific
            // type) or belong to the request's type - the same set the Add
            // Activity picker is built from.
            if ( activityType == null
                || !activityType.IsActive
                || ( activityType.ConnectionTypeId.HasValue
                    && activityType.ConnectionTypeId.Value != connectionRequest.ConnectionOpportunity.ConnectionTypeId ) )
            {
                return ActionBadRequest( "Invalid activity type." );
            }

            // Resolve the optional connector to a person alias, validating it
            // against the available connectors (web parity).
            int? connectorPersonAliasId = null;

            if ( request.ConnectorPersonGuid.HasValue )
            {
                var isValidConnector = GetAvailableConnectors( connectionRequest )
                    .Any( c => c.PersonGuid == request.ConnectorPersonGuid.Value );

                connectorPersonAliasId = new PersonAliasService( RockContext ).GetPrimaryAliasId( request.ConnectorPersonGuid.Value );

                if ( !isValidConnector || !connectorPersonAliasId.HasValue )
                {
                    return ActionBadRequest( "Invalid connector." );
                }
            }

            // A person note is created when the activity type always creates one,
            // or when it asks and the connector opted in (web parity).
            var shouldCreatePersonNote = activityType.PersonNoteCreationBehavior == PersonNoteCreationBehavior.AlwaysCreateAPersonNote
                || ( activityType.PersonNoteCreationBehavior == PersonNoteCreationBehavior.AskAtActivityCreation && request.AddPersonNote );

            if ( shouldCreatePersonNote && !activityType.PersonNoteTypeId.HasValue )
            {
                return ActionBadRequest( "The selected activity type is missing a required person note type." );
            }

            var activity = new ConnectionRequestActivity
            {
                ConnectionRequestId = connectionRequest.Id,
                ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                ConnectionActivityTypeId = activityType.Id,
                ConnectorPersonAliasId = connectorPersonAliasId,
                Note = request.Note
            };

            new ConnectionRequestActivityService( RockContext ).Add( activity );

            if ( shouldCreatePersonNote )
            {
                var personNote = new Note
                {
                    NoteTypeId = activityType.PersonNoteTypeId.Value,
                    EntityId = connectionRequest.PersonAlias.PersonId,
                    Caption = connectionRequest.ConnectionOpportunity.Name,
                    Text = request.Note,
                    CreatedByPersonAliasId = connectorPersonAliasId
                };

                new NoteService( RockContext ).Add( personNote );
            }

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Updates an existing activity's note, connector and (unless it is a
        /// global/system activity type) its activity type. The Edit Activity
        /// sheet reuses the Add Activity sheet, prefilled. Ports the
        /// single-request slice of the web Connections Hub UpdateActivity.
        /// </summary>
        /// <param name="request">The update-activity payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult UpdateActivity( UpdateActivityRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() || request.ActivityIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var activity = new ConnectionRequestActivityService( RockContext )
                .Get( request.ActivityIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            // The activity must exist and belong to the request being viewed;
            // cross-request activities are edited from their own request.
            if ( activity == null || activity.ConnectionRequestId != connectionRequest.Id )
            {
                return ActionBadRequest( $"{ConnectionRequestActivity.FriendlyTypeName} not found." );
            }

            // Resolve inherited authorization against the loaded request.
            activity.ConnectionRequest = connectionRequest;

            if ( !CanCurrentPersonEditActivity( activity ) )
            {
                return ActionForbidden( "You are not authorized to edit this activity." );
            }

            var activityType = new ConnectionActivityTypeService( RockContext ).Get( request.ActivityTypeGuid );

            // The activity type must be active and either global (no specific
            // type) or belong to the request's type - the same set the sheet is
            // built from.
            if ( activityType == null
                || !activityType.IsActive
                || ( activityType.ConnectionTypeId.HasValue
                    && activityType.ConnectionTypeId.Value != connectionRequest.ConnectionOpportunity.ConnectionTypeId ) )
            {
                return ActionBadRequest( "Invalid activity type." );
            }

            // Resolve the optional connector to a person alias. Unlike AddActivity,
            // the connector is not re-validated against the available connectors
            // here (web parity): an activity may already credit a connector who is
            // no longer in the available set, and editing only the note must not
            // fail because of it.
            int? connectorPersonAliasId = null;

            if ( request.ConnectorPersonGuid.HasValue )
            {
                connectorPersonAliasId = new PersonAliasService( RockContext ).GetPrimaryAliasId( request.ConnectorPersonGuid.Value );

                if ( !connectorPersonAliasId.HasValue )
                {
                    return ActionBadRequest( "Invalid connector." );
                }
            }

            activity.Note = request.Note;
            activity.ConnectorPersonAliasId = connectorPersonAliasId;

            // The activity type may only be changed when the current type is not
            // a global/system activity type (web parity).
            if ( activity.ConnectionActivityType != null && activity.ConnectionActivityType.ConnectionTypeId.HasValue )
            {
                activity.ConnectionActivityTypeId = activityType.Id;
            }

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Deletes an activity from the request's activity feed. Ports the web
        /// Connections Hub DeleteActivity (no confirmation - the shell deletes
        /// on the overflow action).
        /// </summary>
        /// <param name="request">The delete-activity payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult DeleteActivity( DeleteActivityRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() || request.ActivityIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var activityService = new ConnectionRequestActivityService( RockContext );
            var activity = activityService.Get( request.ActivityIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            // The activity must exist and belong to the request being viewed;
            // cross-request activities are deleted from their own request.
            if ( activity == null || activity.ConnectionRequestId != connectionRequest.Id )
            {
                return ActionBadRequest( $"{ConnectionRequestActivity.FriendlyTypeName} not found." );
            }

            // Resolve inherited authorization against the loaded request.
            activity.ConnectionRequest = connectionRequest;

            if ( !CanCurrentPersonEditActivity( activity ) )
            {
                return ActionForbidden( "You are not authorized to delete this activity." );
            }

            activityService.Delete( activity );

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Gets the options needed to render the Transfer sheet: the
        /// opportunities of the request's type, the type's statuses, and the
        /// request's current opportunity / campus / status / connector. Ports
        /// the web Connections Hub <c>GetTransferDetails</c> (the opportunity
        /// search/browse mode and its attributes are omitted on mobile).
        /// </summary>
        /// <param name="request">The transfer-details request payload.</param>
        /// <returns>The transfer options.</returns>
        [BlockAction]
        public BlockActionResult GetTransferDetails( GetTransferDetailsRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            // A transfer is an edit; gate it with the same EDIT authorization
            // the web uses (CanEditSpecifiedConnectionRequest).
            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var connectionType = connectionRequest.ConnectionOpportunity.ConnectionType;

            var opportunities = new ConnectionOpportunityService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( "ConnectionOpportunityCampuses.Campus" )
                .Where( o => o.ConnectionTypeId == connectionType.Id )
                .ToList();

            var opportunityItems = opportunities
                .OrderBy( o => o.Order )
                .ThenBy( o => o.Name )
                .Select( o => new TransferOpportunityItemBag
                {
                    Value = o.Guid,
                    Name = o.Name,
                    ShowCampusOnTransfer = o.ShowCampusOnTransfer,
                    ShowStatusOnTransfer = o.ShowStatusOnTransfer,
                    Campuses = o.ConnectionOpportunityCampuses
                        .Where( c => c.Campus != null && c.Campus.IsActive == true )
                        .Select( c => c.Campus )
                        .OrderBy( c => c.Order )
                        .ThenBy( c => c.Name )
                        .Select( c => new ListItemViewModel
                        {
                            Value = c.Guid.ToString(),
                            Text = c.Name
                        } )
                        .ToList()
                } )
                .ToList();

            var statuses = connectionType.ConnectionStatuses
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .Select( s => new ListItemViewModel
                {
                    Value = s.Guid.ToString(),
                    Text = s.Name
                } )
                .ToList();

            var details = new GetTransferDetailsResponseBag
            {
                CurrentOpportunityGuid = connectionRequest.ConnectionOpportunity.Guid,
                CurrentCampusGuid = connectionRequest.Campus?.Guid,
                CurrentStatusGuid = connectionRequest.ConnectionStatus.Guid,
                CurrentConnectorName = connectionRequest.ConnectorPersonAlias?.Person?.FullName ?? "No Connector",
                Opportunities = opportunityItems,
                Statuses = statuses
            };

            return ActionOk( details );
        }

        /// <summary>
        /// Gets the connectors available for a transfer target opportunity,
        /// narrowed to the chosen campus. Fetched lazily by the Transfer sheet
        /// when the "Select Connector" option is used and as the opportunity /
        /// campus change (mirrors the web FetchConnectorOptions call).
        /// </summary>
        /// <param name="request">The connector-fetch payload.</param>
        /// <returns>The available connectors for the target opportunity.</returns>
        [BlockAction]
        public BlockActionResult GetTransferConnectors( GetTransferConnectorsRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var opportunityId = new ConnectionOpportunityService( RockContext ).GetId( request.OpportunityGuid );

            if ( !opportunityId.HasValue )
            {
                return ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            return ActionOk( GetTransferConnectorItems( opportunityId.Value, request.CampusGuid ) );
        }

        /// <summary>
        /// Transfers the request to a different opportunity, optionally updating
        /// the status, campus, and connector based on the new opportunity's
        /// transfer configuration. Placement group assignments are cleared and a
        /// Transferred activity is logged. Ports the web Connections Hub
        /// <c>TransferConnectionRequest</c> (single-request slice); the connector
        /// is identified by person Guid (resolved to a PersonAlias) to match the
        /// other V2 connector actions.
        /// </summary>
        /// <param name="request">The transfer payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult TransferRequest( TransferRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var sourceOpportunityId = connectionRequest.ConnectionOpportunityId;
            var newOpportunity = new ConnectionOpportunityService( RockContext ).Get( request.NewOpportunityGuid );

            if ( newOpportunity == null )
            {
                return ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            if ( newOpportunity.Id == sourceOpportunityId )
            {
                return ActionBadRequest( "This request already belongs to the selected opportunity. Please choose a different opportunity to transfer to." );
            }

            connectionRequest.ConnectionOpportunityId = newOpportunity.Id;
            connectionRequest.ConnectionTypeId = newOpportunity.ConnectionTypeId;

            // Apply the status only when the new opportunity surfaces it on transfer.
            if ( newOpportunity.ShowStatusOnTransfer && request.StatusGuid.HasValue )
            {
                var connectionStatusId = new ConnectionStatusService( RockContext ).Queryable()
                    .Where( s => s.ConnectionTypeId == connectionRequest.ConnectionTypeId && s.Guid == request.StatusGuid.Value )
                    .Select( s => s.Id )
                    .FirstOrDefault();

                if ( connectionStatusId == 0 )
                {
                    return ActionBadRequest( $"{ConnectionStatus.FriendlyTypeName} not found." );
                }

                connectionRequest.ConnectionStatusId = connectionStatusId;
            }

            // Apply the campus only when the new opportunity surfaces it on
            // transfer; clear it when shown but nothing was chosen.
            if ( newOpportunity.ShowCampusOnTransfer && request.CampusGuid.HasValue )
            {
                var campus = CampusCache.Get( request.CampusGuid.Value );

                var campusId = campus == null ? 0 : new ConnectionOpportunityCampusService( RockContext ).Queryable()
                    .Where( c => c.ConnectionOpportunityId == newOpportunity.Id && c.CampusId == campus.Id )
                    .Select( c => c.CampusId )
                    .FirstOrDefault();

                if ( campusId == 0 )
                {
                    return ActionBadRequest( $"{ConnectionOpportunityCampus.FriendlyTypeName} not found." );
                }

                connectionRequest.CampusId = campusId;
            }
            else if ( newOpportunity.ShowCampusOnTransfer )
            {
                connectionRequest.CampusId = null;
            }

            // Placement group assignments do not carry across a transfer.
            connectionRequest.AssignedGroupId = null;
            connectionRequest.AssignedGroupMemberRoleId = null;
            connectionRequest.AssignedGroupMemberStatus = null;

            // Assign the connector according to the chosen option.
            if ( request.ConnectorOption == "default" )
            {
                connectionRequest.ConnectorPersonAliasId = newOpportunity.GetDefaultConnectorPersonAliasId( connectionRequest.CampusId );
            }
            else if ( request.ConnectorOption == "none" )
            {
                connectionRequest.ConnectorPersonAliasId = null;
            }
            else if ( request.ConnectorOption == "select" )
            {
                if ( !request.ConnectorPersonGuid.HasValue )
                {
                    return ActionBadRequest( "Connector not found." );
                }

                var newConnectorId = new PersonAliasService( RockContext ).GetPrimaryAliasId( request.ConnectorPersonGuid.Value );

                if ( !newConnectorId.HasValue )
                {
                    return ActionBadRequest( "Connector not found." );
                }

                connectionRequest.ConnectorPersonAliasId = newConnectorId.Value;
            }

            // The "current" option leaves the existing connector untouched.

            // Log a transfer activity when the standard Transferred activity
            // type is configured (web parity).
            var transferredActivityGuid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
            var transferredActivityId = new ConnectionActivityTypeService( RockContext ).Queryable()
                .Where( t => t.Guid == transferredActivityGuid )
                .Select( t => t.Id )
                .FirstOrDefault();

            if ( transferredActivityId > 0 )
            {
                new ConnectionRequestActivityService( RockContext ).Add( new ConnectionRequestActivity
                {
                    ConnectionRequestId = connectionRequest.Id,
                    ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                    ConnectionActivityTypeId = transferredActivityId,
                    Note = request.Note,
                    ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId
                } );
            }

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Launches a manual connection workflow for the request and reports
        /// whether it needs an interactive entry form.
        /// </summary>
        /// <param name="request">The launch payload.</param>
        /// <returns>The launch result.</returns>
        [BlockAction]
        public BlockActionResult LaunchWorkflow( LaunchWorkflowRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();
            var connectionWorkflow = new ConnectionWorkflowService( RockContext ).Get( request.ConnectionWorkflowGuid );

            if ( connectionRequest == null || connectionWorkflow == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this connection request." );
            }

            // Verify the workflow is one the person may launch for this request.
            var workflows = GetConnectionOpportunityManualWorkflowTypes( connectionRequest.ConnectionOpportunity, RequestContext.CurrentPerson );

            if ( !workflows.Any( w => w.Guid == connectionWorkflow.Guid ) )
            {
                return ActionBadRequest( "That workflow is not available for this request." );
            }

            var result = LaunchConnectionRequestWorkflow( connectionRequest, connectionWorkflow, RequestContext.CurrentPerson, RockContext );

            return ActionOk( result );
        }

        /// <summary>
        /// Creates, updates, or clears the celebration note for the request.
        /// Ports the web UpsertCelebrationText.
        /// </summary>
        /// <param name="request">The celebration payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult UpsertCelebration( UpsertCelebrationRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var celebrationNoteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() );

            if ( celebrationNoteType == null )
            {
                return ActionBadRequest( "The celebration note type is not configured." );
            }

            var noteService = new NoteService( RockContext );
            var existingNote = noteService.Queryable()
                .FirstOrDefault( n => n.NoteTypeId == celebrationNoteType.Id && n.EntityId == connectionRequest.Id );

            var text = request.CelebrationText?.Trim();

            if ( text.IsNullOrWhiteSpace() )
            {
                // Clearing removes the note so the badge disappears in the list
                // blocks.
                if ( existingNote != null )
                {
                    noteService.Delete( existingNote );
                    RockContext.SaveChanges();
                }
            }
            else if ( existingNote != null )
            {
                if ( existingNote.Text != text )
                {
                    existingNote.Text = text;
                    RockContext.SaveChanges();
                }
            }
            else
            {
                noteService.Add( new Note
                {
                    NoteTypeId = celebrationNoteType.Id,
                    EntityId = connectionRequest.Id,
                    Text = text,
                    CreatedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId
                } );
                RockContext.SaveChanges();
            }

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Adds or edits a connection request note (a CONNECTION_REQUEST_NOTE).
        /// Ports the web SaveNote. Requires VIEW on the request and EDIT on the
        /// note.
        /// </summary>
        /// <param name="request">The note payload.</param>
        /// <returns>The refreshed request detail.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( ConnectionRequestNoteBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this connection request." );
            }

            var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE.AsGuid() );

            if ( noteType == null )
            {
                return ActionBadRequest( "The connection request note type is not configured." );
            }

            var noteService = new NoteService( RockContext );
            Note note;

            if ( request.NoteIdKey.IsNullOrWhiteSpace() )
            {
                note = new Note
                {
                    NoteTypeId = noteType.Id,
                    EntityId = connectionRequest.Id,
                    CreatedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId
                };

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "You are not authorized to add a note." );
                }

                noteService.Add( note );
            }
            else
            {
                note = noteService.Get( request.NoteIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( note == null )
                {
                    return ActionBadRequest( $"{Note.FriendlyTypeName} not found." );
                }

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "You are not authorized to edit this note." );
                }
            }

            note.Text = request.NoteText;

            RockContext.SaveChanges();

            return ActionOk( BuildRefreshedResponse( connectionRequest.Id ) );
        }

        /// <summary>
        /// Gets the full activity feed for a connection request: logged
        /// activities, the requester's activities on other requests of the same
        /// type (when the type enables it), system updates parsed from history,
        /// sent communications, and request notes - ordered newest first. Ports
        /// the web Connections Hub GetActivityEntries.
        /// </summary>
        /// <param name="request">The request carrying the connection request IdKey.</param>
        /// <returns>The activity feed entries.</returns>
        [BlockAction]
        public BlockActionResult GetActivities( GetActivitiesRequestBag request )
        {
            if ( request == null || request.ConnectionRequestIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionRequest = GetRequestQueryable( request.ConnectionRequestIdKey ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this connection request." );
            }

            // Merge fields are only needed to resolve SMS message content (web parity).
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.AddOrReplace( "ConnectionRequest", connectionRequest );
            mergeFields.AddOrReplace( "Person", connectionRequest.PersonAlias?.Person );

            return ActionOk( new GetActivitiesResponseBag
            {
                Activities = GetActivityEntries( connectionRequest, mergeFields )
            } );
        }

        #endregion

        #region Private Methods - Resolution and Auth

        /// <summary>
        /// Gets the queryable for a single connection request by IdKey (also
        /// accepts a Guid or integer Id), with the navigation properties the
        /// load and edit paths need.
        /// </summary>
        /// <param name="idKey">The connection request IdKey.</param>
        /// <returns>A queryable that resolves to the single request.</returns>
        private IQueryable<ConnectionRequest> GetRequestQueryable( string idKey )
        {
            return new ConnectionRequestService( RockContext )
                .GetQueryableByKey( idKey, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.ConnectorPersonAlias.Person )
                .Include( r => r.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
                .Include( r => r.ConnectionOpportunity.ConnectionType.ConnectionTypeSources )
                .Include( r => r.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes )
                .Include( r => r.ConnectionStatus )
                .Include( r => r.Campus )
                .Include( r => r.ConnectionTypeSource )
                .Include( r => r.AssignedGroup.GroupType );
        }

        /// <summary>
        /// Determines whether the current person may edit the request, porting
        /// the web Connections Hub <c>CanEditSpecifiedConnectionRequest</c>: an
        /// EnableRequestSecurity-aware EDIT check plus the connector and
        /// connector-group fallback grant (with the campus rule). Sets
        /// <paramref name="error"/> to an <see cref="ActionForbidden"/> result
        /// when the person may not edit.
        /// </summary>
        /// <param name="connectionRequest">The request being authorized.</param>
        /// <param name="error">On return, the forbidden result when not authorized.</param>
        /// <returns><c>true</c> when the current person may edit the request.</returns>
        private bool CanEditConnectionRequest( ConnectionRequest connectionRequest, out BlockActionResult error )
        {
            error = null;

            var currentPerson = RequestContext.CurrentPerson;
            var opportunity = connectionRequest.ConnectionOpportunity;
            var enableRequestSecurity = opportunity?.ConnectionType?.EnableRequestSecurity == true;

            bool canEdit;

            if ( enableRequestSecurity )
            {
                canEdit = connectionRequest.IsAuthorized( Authorization.EDIT, currentPerson );
            }
            else
            {
                canEdit = opportunity.IsAuthorized( Authorization.EDIT, currentPerson );

                // Fall back to the connector / connector-group grant.
                if ( !canEdit && currentPerson != null )
                {
                    if ( connectionRequest.ConnectorPersonAlias != null
                        && connectionRequest.ConnectorPersonAlias.PersonId == currentPerson.Id )
                    {
                        canEdit = true;
                    }
                    else
                    {
                        var connectorGroups = new ConnectionOpportunityConnectorGroupService( RockContext ).Queryable()
                            .AsNoTracking()
                            .Where( g => g.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId
                                && g.ConnectorGroup != null
                                && g.ConnectorGroup.IsActive
                                && !g.ConnectorGroup.IsArchived
                                && g.ConnectorGroup.Members.Any( m => m.PersonId == currentPerson.Id
                                    && m.GroupMemberStatus == GroupMemberStatus.Active
                                    && !m.IsArchived ) )
                            .Select( g => new { g.CampusId } )
                            .ToList();

                        if ( connectorGroups.Any() )
                        {
                            var activeCampusCount = CampusCache.All().Count( c => c.IsActive ?? true );

                            canEdit = activeCampusCount == 1
                                || connectorGroups.Any( g => !g.CampusId.HasValue )
                                || !connectionRequest.CampusId.HasValue
                                || connectorGroups.Any( g => g.CampusId == connectionRequest.CampusId.Value );
                        }
                    }
                }
            }

            if ( !canEdit )
            {
                error = ActionForbidden( $"You are not authorized to edit this {ConnectionRequest.FriendlyTypeName}." );
            }

            return canEdit;
        }

        /// <summary>
        /// Determines whether the current person owns the activity - they either
        /// created it or are its credited connector. Ports the web Connections
        /// Hub <c>IsCurrentPersonActivityOwner</c>.
        /// </summary>
        /// <param name="activity">The activity to evaluate.</param>
        /// <returns><c>true</c> when the current person created the activity or is its connector.</returns>
        private bool IsCurrentPersonActivityOwner( ConnectionRequestActivity activity )
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( currentPerson == null || activity == null )
            {
                return false;
            }

            var isActivityCreator = activity.CreatedByPersonAlias?.PersonId == currentPerson.Id;
            var isActivityConnector = activity.ConnectorPersonAlias?.PersonId == currentPerson.Id;

            return isActivityCreator || isActivityConnector;
        }

        /// <summary>
        /// Determines whether the current person may edit or delete the activity:
        /// the single check shared by the UpdateActivity / DeleteActivity actions
        /// and by the activity feed's per-entry editability, so the shell never
        /// offers an action the server would reject. The person may modify an
        /// activity when they own it (creator or connector) or hold edit
        /// authorization on it (inherited from the parent request). Ports the web
        /// Connections Hub <c>CanCurrentPersonEditActivity</c>.
        /// </summary>
        /// <param name="activity">The activity to evaluate. Its <see cref="ConnectionRequestActivity.ConnectionRequest"/> must be loaded so inherited authorization resolves.</param>
        /// <returns><c>true</c> when the current person may edit or delete the activity.</returns>
        private bool CanCurrentPersonEditActivity( ConnectionRequestActivity activity )
        {
            if ( activity == null )
            {
                return false;
            }

            // System activity types (Assigned, Connected, Transferred, etc.) have a
            // null ConnectionTypeId and must never be edited or deleted - they are the
            // audit trail of the request's state changes. This ports the web
            // Connections Hub, which requires ConnectionTypeId.HasValue before
            // allowing an activity to be modified or removed.
            if ( activity.ConnectionActivityType?.ConnectionTypeId.HasValue != true )
            {
                return false;
            }

            return IsCurrentPersonActivityOwner( activity )
                || activity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        #endregion

        #region Private Methods - Response Building

        /// <summary>
        /// Re-resolves a request by id (with fresh navigation properties) and
        /// builds its display response. Used after a mutation so the returned
        /// detail reflects the saved values.
        /// </summary>
        /// <param name="connectionRequestId">The request identifier.</param>
        /// <returns>The refreshed display response.</returns>
        private GetRequestDetailResponseBag BuildRefreshedResponse( int connectionRequestId )
        {
            var connectionRequest = GetRequestQueryable( connectionRequestId.ToString() ).FirstOrDefault();

            if ( connectionRequest == null )
            {
                return null;
            }

            connectionRequest.LoadAttributes( RockContext );

            return BuildDetailResponse( connectionRequest );
        }

        /// <summary>
        /// Builds the full display response for a loaded request (attributes
        /// must already be loaded).
        /// </summary>
        /// <param name="connectionRequest">The loaded request.</param>
        /// <returns>The display response.</returns>
        private GetRequestDetailResponseBag BuildDetailResponse( ConnectionRequest connectionRequest )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var requester = connectionRequest.PersonAlias?.Person;
            var connectionType = connectionRequest.ConnectionOpportunity.ConnectionType;
            var canEdit = CanEditConnectionRequest( connectionRequest, out _ );

            // Resolve the assigned role's Guid (when placed) so the per-field
            // SaveRequest can pass through the current placement.
            Guid? assignedRoleGuid = null;

            if ( connectionRequest.AssignedGroupMemberRoleId.HasValue )
            {
                assignedRoleGuid = new GroupTypeRoleService( RockContext ).Queryable()
                    .Where( r => r.Id == connectionRequest.AssignedGroupMemberRoleId.Value )
                    .Select( r => ( Guid? ) r.Guid )
                    .FirstOrDefault();
            }

            var bag = new ConnectionRequestDetailBag
            {
                ConnectionRequestIdKey = connectionRequest.IdKey,

                RequesterFullName = requester?.FullName,
                RequesterPhotoUrl = BuildPersonPhotoUrl( requester?.PhotoId ),
                RequesterPersonGuid = requester?.Guid ?? Guid.Empty,
                RequesterPersonAliasGuid = connectionRequest.PersonAlias?.Guid ?? Guid.Empty,
                PersonConnectionStatusName = requester?.ConnectionStatusValue?.Value,
                Gender = requester != null && requester.Gender != Gender.Unknown ? requester.Gender.ToString() : null,
                RequesterGender = requester != null ? ( Rock.Common.Mobile.Enums.Gender ) ( int ) requester.Gender : Rock.Common.Mobile.Enums.Gender.Unknown,
                PhoneNumber = requester?.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.NumberFormatted,
                Email = requester?.Email,

                ConnectionOpportunityName = connectionRequest.ConnectionOpportunity.Name,
                ConnectionOpportunityIconCssClass = connectionRequest.ConnectionOpportunity.IconCssClass,
                ConnectionTypeName = connectionType.Name,

                ConnectionState = ( MobileConnectionState ) ( int ) connectionRequest.ConnectionState,
                FollowUpDate = connectionRequest.FollowupDate?.ToRockDateTimeOffset(),
                StatusName = connectionRequest.ConnectionStatus?.Name,
                StatusColor = connectionRequest.ConnectionStatus?.HighlightColor,
                StatusGuid = connectionRequest.ConnectionStatus?.Guid ?? Guid.Empty,
                CampusName = connectionRequest.Campus?.Name,
                CampusGuid = connectionRequest.Campus?.Guid,
                ConnectorName = connectionRequest.ConnectorPersonAlias?.Person?.FullName,
                ConnectorPersonGuid = connectionRequest.ConnectorPersonAlias?.Person?.Guid,
                RequestSourceName = connectionRequest.ConnectionTypeSource?.Name,
                RequestSourceGuid = connectionRequest.ConnectionTypeSource?.Guid,
                HasRequestSources = connectionType.ConnectionTypeSources.Any(),
                DueStatus = GetRequestDueStatus( connectionRequest ),
                CreatedDateTime = connectionRequest.CreatedDateTime?.ToRockDateTimeOffset(),
                DueDate = connectionRequest.DueDate?.ToRockDateTimeOffset(),
                Comments = connectionRequest.Comments?.ConvertMarkdownToHtml().StripHtml(),

                HasPlacementGroups = GetRequestPlacementGroups( connectionRequest ).Any(),
                PlacementGroup = BuildPlacementGroupSummary( connectionRequest ),
                PlacementGroupGuid = connectionRequest.AssignedGroup?.Guid,
                PlacementGroupRoleGuid = assignedRoleGuid,
                PlacementGroupMemberStatus = connectionRequest.AssignedGroupMemberStatus.HasValue
                    ? ( MobileGroupMemberStatus ) ( int ) connectionRequest.AssignedGroupMemberStatus.Value
                    : ( MobileGroupMemberStatus? ) null,
                PlacementGroupRequirements = canEdit
                    ? GetPlacementGroupRequirements( connectionRequest, RockContext )
                    : new List<GroupMemberRequirementBag>(),

                Attributes = GetClientAttributeValuesForView( connectionRequest ),
                EditableAttributeKeys = canEdit
                    ? connectionRequest.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true ).Keys.ToList()
                    : new List<string>(),
                ManualWorkflows = GetConnectionOpportunityManualWorkflowTypes( connectionRequest.ConnectionOpportunity, currentPerson )
                    .Where( w => w.ManualTriggerFilterConnectionStatusId == null
                        || w.ManualTriggerFilterConnectionStatusId == connectionRequest.ConnectionStatusId )
                    .Select( w => new ManualWorkflowItemBag
                    {
                        Value = w.Guid,
                        Name = w.WorkflowType.Name,
                        IconCssClass = w.WorkflowType.IconCssClass
                    } )
                    .ToList(),

                ActivityCount = new ConnectionRequestActivityService( RockContext ).Queryable()
                    .Count( a => a.ConnectionRequestId == connectionRequest.Id ),
                ReminderCount = GetReminderCount( connectionRequest ),
                AreRemindersEnabled = connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Reminder ),
                CanEditConnectionRequestNote = CanEditConnectionRequestNote( connectionRequest )
            };

            return new GetRequestDetailResponseBag
            {
                Request = bag,
                CelebrationText = GetCelebrationText( connectionRequest.Id ),
                CanEdit = canEdit
            };
        }

        /// <summary>
        /// Builds the assigned placement group summary, or <c>null</c> when no
        /// group is assigned.
        /// </summary>
        /// <param name="connectionRequest">The request.</param>
        /// <returns>The placement group summary, or <c>null</c>.</returns>
        private PlacementGroupSummaryBag BuildPlacementGroupSummary( ConnectionRequest connectionRequest )
        {
            if ( connectionRequest.AssignedGroup == null )
            {
                return null;
            }

            string roleName = null;

            if ( connectionRequest.AssignedGroupMemberRoleId.HasValue )
            {
                roleName = new GroupTypeRoleService( RockContext ).Queryable()
                    .Where( r => r.Id == connectionRequest.AssignedGroupMemberRoleId.Value )
                    .Select( r => r.Name )
                    .FirstOrDefault();
            }

            // Load the assigned group member's attribute values (e.g. "Hours
            // Serving") so they can be shown beneath the row. Mirrors the editor's
            // GetPlacementGroupMemberAttributes: build an empty member for the
            // group + role, seed the request's saved values, then format for view.
            var memberAttributeValues = new List<ListItemViewModel>();

            if ( connectionRequest.AssignedGroupMemberRoleId.HasValue )
            {
                var groupMember = new GroupMember
                {
                    GroupId = connectionRequest.AssignedGroupId.Value,
                    GroupRoleId = connectionRequest.AssignedGroupMemberRoleId.Value
                };

                groupMember.LoadAttributes( RockContext );

                var savedMemberAttributeValues = connectionRequest.AssignedGroupMemberAttributeValues?.FromJsonOrNull<Dictionary<string, string>>();

                if ( savedMemberAttributeValues != null )
                {
                    foreach ( var item in savedMemberAttributeValues )
                    {
                        groupMember.SetAttributeValue( item.Key, item.Value );
                    }
                }

                // Only attributes the person may view, with a non-empty formatted
                // value, are surfaced.
                memberAttributeValues = GetClientAttributeValuesForView( groupMember )
                    .Where( a => a.TextValue.IsNotNullOrWhiteSpace() )
                    .Select( a => new ListItemViewModel
                    {
                        Text = a.Name,
                        Value = a.TextValue
                    } )
                    .ToList();
            }

            return new PlacementGroupSummaryBag
            {
                GroupGuid = connectionRequest.AssignedGroup.Guid,
                Name = connectionRequest.AssignedGroup.Name,
                IconCssClass = connectionRequest.AssignedGroup.GroupType?.IconCssClass,
                RoleName = roleName,
                StatusName = connectionRequest.AssignedGroupMemberStatus?.ToString(),
                MemberAttributeValues = memberAttributeValues
            };
        }

        /// <summary>
        /// Gets the current person's count of incomplete, past-due reminders on
        /// the request entity.
        /// </summary>
        /// <param name="connectionRequest">The request.</param>
        /// <returns>The reminder count.</returns>
        private int GetReminderCount( ConnectionRequest connectionRequest )
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return 0;
            }

            var now = RockDateTime.Now;

            // Web parity: reminders are tracked against the requester's
            // PersonAlias, so the count matches reminders whose EntityId is the
            // request's PersonAliasId (ConnectionsHub.GetConnectionRequestDetails).
            return new ReminderService( RockContext ).Queryable()
                .Count( r => !r.IsComplete
                    && r.ReminderDate < now
                    && r.PersonAlias.PersonId == currentPerson.Id
                    && r.EntityId == connectionRequest.PersonAliasId );
        }

        /// <summary>
        /// Determines whether the current person may add a connection request
        /// note, mirroring the web's temp-note EDIT check.
        /// </summary>
        /// <param name="connectionRequest">The request.</param>
        /// <returns><c>true</c> when a note may be added.</returns>
        private bool CanEditConnectionRequestNote( ConnectionRequest connectionRequest )
        {
            var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE.AsGuid() );

            if ( noteType == null )
            {
                return false;
            }

            var tempNote = new Note
            {
                NoteTypeId = noteType.Id,
                EntityId = connectionRequest.Id
            };

            return tempNote.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the non-empty celebration note text for the request, or
        /// <c>null</c> when none exists.
        /// </summary>
        /// <param name="connectionRequestId">The request identifier.</param>
        /// <returns>The celebration text, or <c>null</c>.</returns>
        private string GetCelebrationText( int connectionRequestId )
        {
            var celebrationNoteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() );

            if ( celebrationNoteType == null )
            {
                return null;
            }

            return new NoteService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( n => n.NoteTypeId == celebrationNoteType.Id
                    && n.EntityId == connectionRequestId
                    && n.Text != null
                    && n.Text != "" )
                .Select( n => n.Text )
                .FirstOrDefault();
        }

        /// <summary>
        /// Maps the request's custom attributes (for view) into the mobile
        /// editable attribute view model.
        /// </summary>
        /// <param name="entity">The attribute-bearing entity.</param>
        /// <returns>The ordered attribute values for display.</returns>
        private List<ClientEditableAttributeValueViewModel> GetClientAttributeValuesForView( Rock.Attribute.IHasAttributes entity )
        {
            var attributes = entity.GetPublicAttributesForView( RequestContext.CurrentPerson )
                .ToDictionary( kvp => kvp.Key, kvp => new ClientEditableAttributeValueViewModel
                {
                    AttributeGuid = kvp.Value.AttributeGuid,
                    Categories = kvp.Value.Categories?
                        .Select( c => new ClientAttributeValueCategoryViewModel
                        {
                            Guid = c.Guid,
                            Name = c.Name,
                            Order = c.Order
                        } )
                        .ToList(),
                    ConfigurationValues = kvp.Value.ConfigurationValues,
                    Description = kvp.Value.Description,
                    FieldTypeGuid = kvp.Value.FieldTypeGuid,
                    IsRequired = kvp.Value.IsRequired,
                    Key = kvp.Value.Key,
                    Name = kvp.Value.Name,
                    Order = kvp.Value.Order,
                    Value = "",
                    TextValue = entity.GetAttributeTextValue( kvp.Value.Key )
                } );

            // Seed Value from the public EDIT values, not the view values. The
            // shell editor feeds Value straight into the mobile FieldType's
            // SetEditValue, which expects the edit-value shape (File / Image /
            // DefinedValue carry a {value,text} JSON for edit but a plain
            // friendly string for view, so the view value would fail to parse).
            // Read-only display uses TextValue, so non-editable attributes do
            // not need a Value. Mirrors the v1 block's GetPublicEditableAttributeValues.
            foreach ( var kvp in entity.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: true ) )
            {
                if ( attributes.TryGetValue( kvp.Key, out var attribute ) )
                {
                    attribute.Value = kvp.Value;
                }
            }

            // Also overlay ConfigurationValues from the EDIT-usage bag rather
            // than the VIEW-usage one above. Field types like SelectMulti trim
            // their option list down to only the currently selected values
            // under View usage (it just needs to render what's checked), which
            // leaves the shell editor with an empty (or incomplete) option list
            // to choose from instead of the full set.
            foreach ( var kvp in entity.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true ) )
            {
                if ( attributes.TryGetValue( kvp.Key, out var attribute ) )
                {
                    attribute.ConfigurationValues = kvp.Value.ConfigurationValues;
                }
            }

            return attributes.Values.OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Computes the request's due status using the same model as the web
        /// Connections Hub (an Inactive request, or one with no due date, is
        /// always On Track).
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The mobile due status.</returns>
        private static MobileDueStatus GetRequestDueStatus( ConnectionRequest request )
        {
            var now = RockDateTime.Now.Date;
            var dueDate = request.DueDate;
            var connectionState = request.ConnectionState;
            var completedDateTime = request.ConnectedDateTime;
            var dueSoonDate = request.DueSoonDate;

            if ( !dueDate.HasValue || connectionState == ConnectionState.Inactive )
            {
                return MobileDueStatus.DueLater;
            }

            var due = dueDate.Value.Date;

            if ( connectionState == ConnectionState.Connected )
            {
                if ( !completedDateTime.HasValue )
                {
                    return MobileDueStatus.DueLater;
                }

                return completedDateTime.Value.Date > due ? MobileDueStatus.Overdue : MobileDueStatus.DueLater;
            }

            if ( now > due )
            {
                return MobileDueStatus.Overdue;
            }

            if ( dueSoonDate.HasValue && now >= dueSoonDate.Value.Date )
            {
                return MobileDueStatus.DueSoon;
            }

            return MobileDueStatus.DueLater;
        }

        #endregion

        #region Private Methods - Edit Options

        /// <summary>
        /// Gets the selectable connection states for the type, honoring whether
        /// Future Follow Up is enabled. Connected is never offered (completion
        /// is the Connect action). Each item's value is the integer state.
        /// </summary>
        /// <param name="connectionType">The connection type.</param>
        /// <returns>The selectable states as list items.</returns>
        private static List<ListItemViewModel> GetSelectableStates( ConnectionType connectionType )
        {
            var states = new List<ListItemViewModel>
            {
                new ListItemViewModel { Value = ( ( int ) ConnectionState.Active ).ToString(), Text = "Active" },
                new ListItemViewModel { Value = ( ( int ) ConnectionState.Inactive ).ToString(), Text = "Inactive" }
            };

            if ( connectionType.EnableFutureFollowup )
            {
                states.Add( new ListItemViewModel { Value = ( ( int ) ConnectionState.FutureFollowUp ).ToString(), Text = "Future Follow Up" } );
            }

            return states;
        }

        /// <summary>
        /// Gets the connectors available for assignment to the request, mirrors
        /// the existing block (all opportunity connectors plus the current
        /// connector and the current person).
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The connectors available for selection.</returns>
        private List<ConnectorItemBag> GetAvailableConnectors( ConnectionRequest request )
        {
            var connectorGroupService = new ConnectionOpportunityConnectorGroupService( RockContext );
            var personAliasService = new PersonAliasService( RockContext );
            var opportunityId = request.ConnectionOpportunityId;

            var connectorList = connectorGroupService.Queryable()
                .Where( a => a.ConnectionOpportunityId == opportunityId )
                .SelectMany( g => g.ConnectorGroup.Members )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( m => new
                {
                    m.Person.Guid,
                    m.Person.NickName,
                    m.Person.LastName,
                    m.Person.PhotoId,
                    CampusGuid = ( Guid? ) m.Group.Campus.Guid
                } )
                .ToList();

            var additionalPersonAliasIds = new List<int>();

            if ( request.ConnectorPersonAliasId.HasValue )
            {
                additionalPersonAliasIds.Add( request.ConnectorPersonAliasId.Value );
            }

            if ( RequestContext.CurrentPerson?.PrimaryAliasId != null )
            {
                additionalPersonAliasIds.Add( RequestContext.CurrentPerson.PrimaryAliasId.Value );
            }

            if ( additionalPersonAliasIds.Any() )
            {
                var additionalPeople = personAliasService.Queryable()
                    .Where( pa => additionalPersonAliasIds.Contains( pa.Id ) )
                    .Select( pa => new
                    {
                        pa.Person.Guid,
                        pa.Person.NickName,
                        pa.Person.LastName,
                        pa.Person.PhotoId,
                        CampusGuid = ( Guid? ) null
                    } )
                    .ToList();

                connectorList.AddRange( additionalPeople );
            }

            return connectorList
                .GroupBy( c => new { c.Guid, c.CampusGuid } )
                .Select( g => g.First() )
                .OrderBy( c => c.LastName )
                .ThenBy( c => c.NickName )
                .Select( c => new ConnectorItemBag
                {
                    PersonGuid = c.Guid,
                    FullName = $"{c.NickName} {c.LastName}".Trim(),
                    PhotoUrl = BuildPersonPhotoUrl( c.PhotoId ),
                    CampusGuid = c.CampusGuid
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the connectors of a target opportunity for a transfer, narrowed
        /// to the chosen campus (a connector with no campus is always kept).
        /// Unlike <see cref="GetAvailableConnectors"/> this is keyed by an
        /// arbitrary opportunity (the transfer target) and does not inject the
        /// request's current connector or the current person, since the
        /// "Current Connector" and "Default Connector" options cover those.
        /// </summary>
        /// <param name="opportunityId">The transfer target opportunity.</param>
        /// <param name="campusGuid">The chosen campus to narrow to, or <c>null</c> for no filter.</param>
        /// <returns>The connectors available for the target opportunity and campus.</returns>
        private List<ConnectorItemBag> GetTransferConnectorItems( int opportunityId, Guid? campusGuid )
        {
            var connectorList = new ConnectionOpportunityConnectorGroupService( RockContext ).Queryable()
                .Where( a => a.ConnectionOpportunityId == opportunityId )
                .SelectMany( g => g.ConnectorGroup.Members )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( m => new
                {
                    m.Person.Guid,
                    m.Person.NickName,
                    m.Person.LastName,
                    m.Person.PhotoId,
                    CampusGuid = ( Guid? ) m.Group.Campus.Guid
                } )
                .ToList();

            return connectorList
                .Where( c => !campusGuid.HasValue || !c.CampusGuid.HasValue || c.CampusGuid.Value == campusGuid.Value )
                .GroupBy( c => c.Guid )
                .Select( g => g.First() )
                .OrderBy( c => c.LastName )
                .ThenBy( c => c.NickName )
                .Select( c => new ConnectorItemBag
                {
                    PersonGuid = c.Guid,
                    FullName = $"{c.NickName} {c.LastName}".Trim(),
                    PhotoUrl = BuildPersonPhotoUrl( c.PhotoId ),
                    CampusGuid = c.CampusGuid
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the active activity types for a connection type, used by the Add
        /// Activity sheet. Mirrors the web Connections Hub activity-type options
        /// (the type's active <see cref="ConnectionActivityType"/> rows).
        /// </summary>
        /// <param name="connectionType">The request's connection type.</param>
        /// <returns>The active activity types, ordered by name.</returns>
        private List<ActivityTypeItemBag> GetActivityTypes( ConnectionType connectionType )
        {
            return connectionType.ConnectionActivityTypes
                .Where( at => at.IsActive )
                .OrderBy( at => at.Name )
                .Select( at => new ActivityTypeItemBag
                {
                    Guid = at.Guid,
                    Name = at.Name,
                    AsksForPersonNote = at.PersonNoteCreationBehavior == PersonNoteCreationBehavior.AskAtActivityCreation
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the placement groups available for the request's opportunity,
        /// with their roles and per-role statuses. Adapted from the existing
        /// block's GetRequestPlacementGroups.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The available placement groups.</returns>
        private List<PlacementGroupItemBag> GetRequestPlacementGroups( ConnectionRequest request )
        {
            var connectionOpportunityGroupConfigQuery = new ConnectionOpportunityGroupConfigService( RockContext ).Queryable()
                .Where( c => c.ConnectionOpportunityId == request.ConnectionOpportunityId );

            var availablePlacementGroups = GetAvailablePlacementGroupsQuery( request.ConnectionOpportunityId, request.AssignedGroupId, RockContext )
                .Select( g => new
                {
                    g.Id,
                    g.Guid,
                    g.Name,
                    CampusGuid = ( Guid? ) g.Campus.Guid,
                    CampusName = g.Campus.Name,
                    Configs = connectionOpportunityGroupConfigQuery
                        .Where( c => c.GroupTypeId == g.GroupTypeId )
                        .Select( c => new
                        {
                            c.GroupMemberRole.Guid,
                            c.GroupMemberRole.Name,
                            Status = c.GroupMemberStatus
                        } )
                        .ToList()
                } )
                .ToList();

            return availablePlacementGroups
                .Select( g => new PlacementGroupItemBag
                {
                    Value = g.Guid,
                    Name = $"{g.Name} ({( g.CampusName.IsNotNullOrWhiteSpace() ? g.CampusName : "No Campus" )})",
                    CampusGuid = g.CampusGuid,
                    Roles = g.Configs
                        .GroupBy( c => new { c.Guid, c.Name } )
                        .Select( cgrp => new PlacementGroupRoleItemBag
                        {
                            Value = cgrp.Key.Guid,
                            Name = cgrp.Key.Name,
                            Statuses = cgrp.Select( s => ( MobileGroupMemberStatus ) ( int ) s.Status ).ToList()
                        } )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets a query of all groups that are possible placement groups for
        /// the request's opportunity. Adapted from the existing block.
        /// </summary>
        /// <param name="connectionOpportunityId">The opportunity identifier.</param>
        /// <param name="additionalGroupId">An optional currently-assigned group to include.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A queryable of the placement groups.</returns>
        private static IQueryable<Rock.Model.Group> GetAvailablePlacementGroupsQuery( int connectionOpportunityId, int? additionalGroupId, RockContext rockContext )
        {
            var opportunityService = new ConnectionOpportunityService( rockContext );
            var groupService = new GroupService( rockContext );

            var specificConfigQuery = opportunityService.Queryable()
                .AsNoTracking()
                .Where( o => o.Id == connectionOpportunityId )
                .SelectMany( o => o.ConnectionOpportunityGroups )
                .Select( cog => cog.Group );

            var allGroupsOfTypeQuery = opportunityService.Queryable()
                .AsNoTracking()
                .Where( o => o.Id == connectionOpportunityId )
                .SelectMany( o => o.ConnectionOpportunityGroupConfigs )
                .Where( gc => gc.UseAllGroupsOfType )
                .SelectMany( gc => gc.GroupType.Groups );

            var allGroupsQuery = specificConfigQuery.Union( allGroupsOfTypeQuery );

            if ( additionalGroupId.HasValue )
            {
                var additionalGroupQuery = groupService.Queryable()
                    .AsNoTracking()
                    .Where( g => g.Id == additionalGroupId );

                allGroupsQuery = allGroupsQuery.Union( additionalGroupQuery );
            }

            return allGroupsQuery
                .Where( g => g.IsActive && !g.IsArchived )
                .Distinct();
        }

        #endregion

        #region Private Methods - Placement, Workflows, Requirements

        /// <summary>
        /// Applies the placement group fields (group + role + status + member
        /// attribute values) to the request, or clears them when no group is
        /// provided. Returns an error result when the data is invalid.
        /// Adapted from the existing block's UpdateRequest.
        /// </summary>
        /// <param name="request">The request being saved.</param>
        /// <param name="saveBag">The save payload.</param>
        /// <returns>An error result, or <c>null</c> on success.</returns>
        private BlockActionResult ApplyPlacementGroup( ConnectionRequest request, SaveConnectionRequestBag saveBag )
        {
            var hasAnyPlacementValue = saveBag.PlacementGroupGuid.HasValue
                || saveBag.PlacementGroupRoleGuid.HasValue
                || saveBag.PlacementGroupMemberStatus.HasValue;

            if ( !hasAnyPlacementValue )
            {
                request.AssignedGroupId = null;
                request.AssignedGroupMemberRoleId = null;
                request.AssignedGroupMemberStatus = null;
                request.AssignedGroupMemberAttributeValues = null;

                return null;
            }

            // All three are required together.
            if ( !saveBag.PlacementGroupGuid.HasValue || !saveBag.PlacementGroupRoleGuid.HasValue || !saveBag.PlacementGroupMemberStatus.HasValue )
            {
                return ActionBadRequest( "A placement group requires a group, a role and a status." );
            }

            var validPlacementGroups = GetRequestPlacementGroups( request );
            var placementGroup = validPlacementGroups.FirstOrDefault( g => g.Value == saveBag.PlacementGroupGuid.Value );
            var placementRole = placementGroup?.Roles.FirstOrDefault( r => r.Value == saveBag.PlacementGroupRoleGuid.Value );

            if ( placementGroup == null || placementRole == null || !placementRole.Statuses.Contains( saveBag.PlacementGroupMemberStatus.Value ) )
            {
                return ActionBadRequest( "Invalid placement group selection." );
            }

            var groupId = new GroupService( RockContext ).GetId( placementGroup.Value );
            var roleId = new GroupTypeRoleService( RockContext ).GetId( placementRole.Value );

            if ( !groupId.HasValue || !roleId.HasValue )
            {
                return ActionBadRequest( "Invalid placement group selection." );
            }

            request.AssignedGroupId = groupId;
            request.AssignedGroupMemberRoleId = roleId;
            request.AssignedGroupMemberStatus = ( GroupMemberStatus ) ( int ) saveBag.PlacementGroupMemberStatus.Value;

            if ( saveBag.PlacementGroupMemberAttributeValues != null )
            {
                var groupMember = new GroupMember
                {
                    GroupId = request.AssignedGroupId.Value,
                    GroupRoleId = request.AssignedGroupMemberRoleId.Value
                };

                groupMember.LoadAttributes( RockContext );

                var memberAttributeValues = new Dictionary<string, string>();

                foreach ( var memberValue in saveBag.PlacementGroupMemberAttributeValues )
                {
                    if ( !groupMember.Attributes.TryGetValue( memberValue.Key, out var attribute )
                        || !attribute.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        return ActionBadRequest( "Invalid placement group member attribute." );
                    }

                    memberAttributeValues.Add( memberValue.Key, PublicAttributeHelper.GetPrivateValue( attribute, memberValue.Value ) );
                }

                request.AssignedGroupMemberAttributeValues = memberAttributeValues.ToJson();
            }
            else
            {
                request.AssignedGroupMemberAttributeValues = null;
            }

            return null;
        }

        /// <summary>
        /// Gets the manually triggered workflows for the opportunity that the
        /// current person may launch. Replicated from the existing block.
        /// </summary>
        /// <param name="connectionOpportunity">The opportunity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>The manual workflows.</returns>
        private static IEnumerable<ConnectionWorkflow> GetConnectionOpportunityManualWorkflowTypes( ConnectionOpportunity connectionOpportunity, Person currentPerson )
        {
            return connectionOpportunity.ConnectionWorkflows
                .Union( connectionOpportunity.ConnectionType.ConnectionWorkflows )
                .Where( w => w.TriggerType == ConnectionWorkflowTriggerType.Manual
                    && w.WorkflowType != null
                    && ( w.WorkflowType.IsActive ?? true ) )
                .OrderBy( w => w.WorkflowType.Name )
                .Distinct()
                .Where( w => w.WorkflowType.IsAuthorized( Authorization.VIEW, currentPerson ) );
        }

        /// <summary>
        /// Launches a connection workflow for the request and returns the
        /// result. Replicated from the existing block.
        /// </summary>
        /// <param name="connectionRequest">The request the workflow runs against.</param>
        /// <param name="connectionWorkflow">The connection workflow to launch.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>The launch result.</returns>
        private static LaunchWorkflowResponseBag LaunchConnectionRequestWorkflow( ConnectionRequest connectionRequest, ConnectionWorkflow connectionWorkflow, Person currentPerson, RockContext rockContext )
        {
            var workflowService = new WorkflowService( rockContext );
            var workflowType = connectionWorkflow.WorkflowTypeCache;

            if ( workflowType == null || workflowType.IsActive == false )
            {
                return new LaunchWorkflowResponseBag
                {
                    WorkflowTypeGuid = workflowType?.Guid,
                    Errors = new List<string> { "Workflow was not found or is not active." }
                };
            }

            var workflow = Rock.Model.Workflow.Activate( workflowType, connectionWorkflow.WorkflowType.WorkTerm, rockContext );

            if ( !workflowService.Process( workflow, connectionRequest, out var workflowErrors ) )
            {
                return new LaunchWorkflowResponseBag
                {
                    WorkflowTypeGuid = workflowType.Guid,
                    WorkflowGuid = workflow.Guid,
                    Errors = workflowErrors?.ToList()
                };
            }

            if ( workflow.Id != 0 )
            {
                new ConnectionRequestWorkflowService( rockContext ).Add( new ConnectionRequestWorkflow
                {
                    ConnectionRequestId = connectionRequest.Id,
                    WorkflowId = workflow.Id,
                    ConnectionWorkflowId = connectionWorkflow.Id,
                    TriggerType = connectionWorkflow.TriggerType,
                    TriggerQualifier = connectionWorkflow.QualifierValue
                } );

                rockContext.SaveChanges();
            }

            return new LaunchWorkflowResponseBag
            {
                WorkflowTypeGuid = workflowType.Guid,
                WorkflowGuid = workflow.Guid,
                HasActiveEntryForm = workflow.HasActiveEntryForm( currentPerson ),
                Message = $"A '{workflowType.Name}' workflow was started."
            };
        }

        /// <summary>
        /// Creates the connector-assigned activity for the request, or
        /// <c>null</c> when there is no connector. Replicated from the existing
        /// block.
        /// </summary>
        /// <param name="connectionRequest">The request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>The activity to add, or <c>null</c>.</returns>
        private static ConnectionRequestActivity CreateAssignedActivity( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            if ( !connectionRequest.ConnectorPersonAliasId.HasValue )
            {
                return null;
            }

            var guid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
            var assignedActivityId = new ConnectionActivityTypeService( rockContext ).Queryable()
                .Where( t => t.Guid == guid )
                .Select( t => t.Id )
                .FirstOrDefault();

            if ( assignedActivityId == 0 )
            {
                return null;
            }

            return new ConnectionRequestActivity
            {
                ConnectionRequestId = connectionRequest.Id,
                ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                ConnectionActivityTypeId = assignedActivityId,
                ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId
            };
        }

        /// <summary>
        /// Gets the placement-group member requirements for the assigned group,
        /// mapped into the mobile bag. Adapted from the existing block.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>The member requirements (empty when no group is assigned).</returns>
        private static List<GroupMemberRequirementBag> GetPlacementGroupRequirements( ConnectionRequest request, RockContext rockContext )
        {
            var requirementResults = GetGroupRequirementStatuses( request, rockContext );
            var requirements = new List<GroupMemberRequirementBag>();

            foreach ( var requirementResult in requirementResults )
            {
                var isManual = requirementResult.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual;
                var label = isManual
                    ? requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel.IfEmpty( requirementResult.GroupRequirement.GroupRequirementType.Name )
                    : requirementResult.GroupRequirement.GroupRequirementType.Name;

                requirements.Add( new GroupMemberRequirementBag
                {
                    GroupRequirementGuid = requirementResult.GroupRequirement.Guid,
                    Label = label,
                    IsManual = isManual,
                    MustMeetRequirementToAddMember = requirementResult.GroupRequirement.MustMeetRequirementToAddMember,
                    GroupMemberRequirementState = ( MobileMeetsGroupRequirement ) ( int ) requirementResult.MeetsGroupRequirement
                } );
            }

            return requirements;
        }

        /// <summary>
        /// Gets the group requirement statuses for the request's assigned group.
        /// Replicated from the existing block.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>The requirement statuses (excludes NotApplicable).</returns>
        private static List<PersonGroupRequirementStatus> GetGroupRequirementStatuses( ConnectionRequest request, RockContext rockContext )
        {
            if ( request == null || !request.AssignedGroupId.HasValue || request.PersonAlias == null )
            {
                return new List<PersonGroupRequirementStatus>();
            }

            var group = new GroupService( rockContext ).GetNoTracking( request.AssignedGroupId.Value );

            if ( group == null )
            {
                return new List<PersonGroupRequirementStatus>();
            }

            var requirementResults = group.PersonMeetsGroupRequirements( rockContext, request.PersonAlias.PersonId, request.AssignedGroupMemberRoleId );

            if ( requirementResults == null )
            {
                return new List<PersonGroupRequirementStatus>();
            }

            return requirementResults
                .Where( r => r.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable )
                .ToList();
        }

        /// <summary>
        /// Attempts to mark the request connected, performing the group
        /// placement and manual-requirement checks. Replicated from the
        /// existing block's TryMarkRequestConnected. Calls SaveChanges on
        /// success.
        /// </summary>
        /// <param name="connectionRequest">The request to connect.</param>
        /// <param name="manualRequirementsMet">The manual requirement acknowledgements keyed by requirement Guid.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <param name="errorMessage">On return, any error message.</param>
        /// <returns><c>true</c> when the request was connected.</returns>
        private static bool TryMarkRequestConnected( ConnectionRequest connectionRequest, Dictionary<Guid, bool> manualRequirementsMet, Person currentPerson, RockContext rockContext, out string errorMessage )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

            if ( connectionRequest?.PersonAlias == null || connectionRequest.ConnectionOpportunity == null )
            {
                errorMessage = "Connection request is not in a valid state.";
                return false;
            }

            if ( connectionRequest.ConnectionState == ConnectionState.Connected )
            {
                errorMessage = null;
                return true;
            }

            GroupMember groupMember = null;
            var hasGroupAssignment = connectionRequest.AssignedGroupId.HasValue
                && connectionRequest.AssignedGroupMemberRoleId.HasValue
                && connectionRequest.AssignedGroupMemberStatus.HasValue
                && connectionRequest.AssignedGroup != null;

            if ( hasGroupAssignment )
            {
                var group = connectionRequest.AssignedGroup;

                groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                    connectionRequest.AssignedGroupId.Value,
                    connectionRequest.PersonAlias.PersonId,
                    connectionRequest.AssignedGroupMemberRoleId.Value );

                if ( groupMember == null )
                {
                    groupMember = new GroupMember
                    {
                        PersonId = connectionRequest.PersonAlias.PersonId,
                        GroupId = connectionRequest.AssignedGroupId.Value,
                        GroupRoleId = connectionRequest.AssignedGroupMemberRoleId.Value,
                        GroupMemberStatus = connectionRequest.AssignedGroupMemberStatus.Value
                    };

                    var manualGroupRequirements = group.GetGroupRequirements( rockContext )
                        .Where( r => r.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                        .ToList();

                    foreach ( var requirement in manualGroupRequirements )
                    {
                        var meetsRequirement = manualRequirementsMet.GetValueOrDefault( requirement.Guid, false );

                        if ( !meetsRequirement && requirement.MustMeetRequirementToAddMember )
                        {
                            errorMessage = "Group Requirements have not been met. Please verify all of the requirements.";
                            return false;
                        }

                        if ( meetsRequirement )
                        {
                            groupMember.GroupMemberRequirements.Add( new GroupMemberRequirement
                            {
                                GroupRequirementId = requirement.Id,
                                RequirementMetDateTime = RockDateTime.Now,
                                LastRequirementCheckDateTime = RockDateTime.Now
                            } );
                        }
                    }

                    groupMemberService.Add( groupMember );

                    if ( connectionRequest.AssignedGroupMemberAttributeValues.IsNotNullOrWhiteSpace() )
                    {
                        var savedValues = connectionRequest.AssignedGroupMemberAttributeValues.FromJsonOrNull<Dictionary<string, string>>();

                        if ( savedValues != null )
                        {
                            groupMember.LoadAttributes();

                            foreach ( var item in savedValues )
                            {
                                groupMember.SetAttributeValue( item.Key, item.Value );
                            }
                        }
                    }
                }
            }

            var connectedGuid = Rock.SystemGuid.ConnectionActivityType.CONNECTED.AsGuid();
            var connectedActivityId = connectionActivityTypeService.Queryable()
                .Where( t => t.Guid == connectedGuid )
                .Select( t => t.Id )
                .FirstOrDefault();

            if ( connectedActivityId > 0 )
            {
                connectionRequestActivityService.Add( new ConnectionRequestActivity
                {
                    ConnectionRequestId = connectionRequest.Id,
                    ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                    ConnectionActivityTypeId = connectedActivityId,
                    ConnectorPersonAliasId = currentPerson.PrimaryAliasId
                } );
            }

            connectionRequest.ConnectionState = ConnectionState.Connected;

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                if ( groupMember != null && connectionRequest.AssignedGroupMemberAttributeValues.IsNotNullOrWhiteSpace() )
                {
                    groupMember.SaveAttributeValues( rockContext );
                }
            } );

            errorMessage = null;
            return true;
        }

        #endregion

        #region Private Methods - Activity Feed

        /// <summary>
        /// Builds the connection request's activity feed - logged activities,
        /// the requester's activities on other requests of the same type (when
        /// the type enables the full activity list), system updates parsed from
        /// history, sent communications, and viewable request notes - ordered
        /// newest first. Ports the web Connections Hub GetActivityEntries into
        /// the mobile activity bag.
        /// </summary>
        /// <param name="connectionRequest">The request, with its opportunity, type and requester loaded.</param>
        /// <param name="mergeFields">Lava merge fields used to resolve SMS message content.</param>
        /// <returns>The activity feed entries, newest first.</returns>
        private List<ConnectionRequestActivityBag> GetActivityEntries( ConnectionRequest connectionRequest, Dictionary<string, object> mergeFields )
        {
            var connectionRequestActivityService = new ConnectionRequestActivityService( RockContext );
            var connectionRequestActivities = connectionRequestActivityService.Queryable()
                .AsNoTracking()
                .Include( a => a.CreatedByPersonAlias.Person )
                .Include( a => a.ConnectorPersonAlias.Person )
                .Include( a => a.ConnectionActivityType )
                .Where( a => a.ConnectionRequestId == connectionRequest.Id )
                .ToList();

            // The activities are read no-tracking, so their ConnectionRequest
            // navigation is not populated. Point each at the loaded request so
            // the inherited edit authorization (CanCurrentPersonEditActivity)
            // resolves without a per-row round-trip (web parity).
            foreach ( var activity in connectionRequestActivities )
            {
                activity.ConnectionRequest = connectionRequest;
            }

            var entries = new List<ConnectionRequestActivityBag>();

            // Activities logged against this request.
            entries.AddRange( connectionRequestActivities.Select( a => new ConnectionRequestActivityBag
            {
                Key = $"{MobileActivityEntryType.Activity}_{IdHasher.Instance.GetHash( a.Id )}",
                EntryType = MobileActivityEntryType.Activity,
                EntryDateTime = a.CreatedDateTime?.ToRockDateTimeOffset(),
                PersonName = a.CreatedByPersonAlias?.Person?.FullName ?? "Rock",
                PhotoUrl = BuildPersonPhotoUrl( a.CreatedByPersonAlias?.Person?.PhotoId ),
                AuthorGender = ( Rock.Common.Mobile.Enums.Gender ) ( int ) ( a.CreatedByPersonAlias?.Person?.Gender ?? Gender.Unknown ),
                Title = string.Format( "Activity: {0}", a.ConnectionActivityType?.Name ),
                Content = a.Note,

                // Edit / delete are offered only for activities on this request
                // the current person may modify, prefilled from the activity's
                // current type and connector (web parity).
                IsEditable = CanCurrentPersonEditActivity( a ),
                ActivityTypeGuid = a.ConnectionActivityType?.Guid,
                ConnectorPersonGuid = a.ConnectorPersonAlias?.Person?.Guid
            } ) );

            // When the type enables it, also surface the requester's activities
            // on their other requests of the same type, tagged with the
            // originating opportunity and status for the meta line.
            if ( connectionRequest.ConnectionOpportunity.ConnectionType.EnableFullActivityList )
            {
                var connectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;
                var requesterPersonId = connectionRequest.PersonAlias.PersonId;

                var otherRequestActivities = connectionRequestActivityService.Queryable()
                    .AsNoTracking()
                    .Where( a => a.ConnectionRequest != null
                        && a.ConnectionRequest.PersonAlias != null
                        && a.ConnectionRequest.ConnectionOpportunity.ConnectionTypeId == connectionTypeId
                        && a.ConnectionRequestId != connectionRequest.Id
                        && a.ConnectionRequest.PersonAlias.PersonId == requesterPersonId )
                    .Select( a => new
                    {
                        ActivityId = a.Id,
                        EntryDateTime = a.CreatedDateTime,
                        Content = a.Note,
                        CreatedByPerson = a.CreatedByPersonAlias != null ? a.CreatedByPersonAlias.Person : null,
                        ActivityTypeName = a.ConnectionActivityType.Name,
                        OpportunityName = a.ConnectionRequest.ConnectionOpportunity.Name,
                        StatusName = a.ConnectionRequest.ConnectionStatus.Name
                    } )
                    .ToList();

                entries.AddRange( otherRequestActivities.Select( a => new ConnectionRequestActivityBag
                {
                    Key = $"{MobileActivityEntryType.Activity}_{IdHasher.Instance.GetHash( a.ActivityId )}",
                    EntryType = MobileActivityEntryType.Activity,
                    EntryDateTime = a.EntryDateTime?.ToRockDateTimeOffset(),
                    PersonName = a.CreatedByPerson?.FullName ?? "Rock",
                    PhotoUrl = BuildPersonPhotoUrl( a.CreatedByPerson?.PhotoId ),
                    AuthorGender = ( Rock.Common.Mobile.Enums.Gender ) ( int ) ( a.CreatedByPerson?.Gender ?? Gender.Unknown ),
                    Title = string.Format( "Activity: {0}", a.ActivityTypeName ),
                    Content = a.Content,
                    OpportunityName = a.OpportunityName,
                    RequestStatus = a.StatusName
                } ) );
            }

            var categoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_CONNECTION_REQUEST.AsGuid() ).Id;
            var connectionRequestEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONNECTION_REQUEST.AsGuid() ).Id;
            var communicationEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION.AsGuid() ).Id;

            // System updates are read from history via raw SQL for performance
            // (web parity).
            var historySql = @"
SELECT
    h.[Id],
    LTRIM( RTRIM( CONCAT( COALESCE( p.[NickName], '' ), ' ', COALESCE( p.[LastName], '' ) ) ) ) AS CreatedBy,
    h.[CreatedDateTime],
    h.[Verb],
    h.[ValueName],
    h.[NewValue],
    h.[OldValue]
FROM [History] h
LEFT JOIN [PersonAlias] pa
    ON pa.[Id] = h.[CreatedByPersonAliasId]
LEFT JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
WHERE h.[CategoryId] = @CategoryId
  AND h.[EntityTypeId] = @ConnectionRequestEntityTypeId
  AND h.[EntityId] = @RequestId;
";

            var historyRows = RockContext.Database
                .SqlQuery<HistoryRow>( historySql,
                    new SqlParameter( "@CategoryId", categoryId ),
                    new SqlParameter( "@ConnectionRequestEntityTypeId", connectionRequestEntityTypeId ),
                    new SqlParameter( "@RequestId", connectionRequest.Id ) )
                .ToList();

            entries.AddRange( historyRows.Select( r =>
            {
                var systemUpdateType = MobileSystemUpdateType.Creation;
                var previousValue = StripBracketId( r.OldValue );
                var newValue = StripBracketId( r.NewValue );

                if ( r.Verb == "Add" )
                {
                    systemUpdateType = MobileSystemUpdateType.Creation;
                }
                else
                {
                    switch ( r.ValueName )
                    {
                        case "Connector":
                            if ( r.NewValue.IsNotNullOrWhiteSpace() && r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = MobileSystemUpdateType.Reassignment;
                            }
                            else if ( r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = MobileSystemUpdateType.Unassignment;
                            }
                            else
                            {
                                systemUpdateType = MobileSystemUpdateType.Assignment;
                            }
                            break;
                        case "ConnectionStatus":
                            if ( r.NewValue.IsNotNullOrWhiteSpace() && r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = MobileSystemUpdateType.StatusUpdated;
                            }
                            else if ( r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = MobileSystemUpdateType.StatusCleared;
                            }
                            else
                            {
                                systemUpdateType = MobileSystemUpdateType.StatusSet;
                            }
                            break;
                        case "ConnectionState":
                            if ( newValue == ConnectionState.Connected.ToString() )
                            {
                                systemUpdateType = MobileSystemUpdateType.Completion;
                            }
                            else
                            {
                                systemUpdateType = MobileSystemUpdateType.StateChange;

                                if ( Enum.TryParse( previousValue, out ConnectionState previousConnectionState ) )
                                {
                                    previousValue = GetConnectionStateDisplay( previousConnectionState );
                                }

                                if ( Enum.TryParse( newValue, out ConnectionState newConnectionState ) )
                                {
                                    newValue = GetConnectionStateDisplay( newConnectionState );
                                }
                            }
                            break;
                        case "DueDate":
                            systemUpdateType = MobileSystemUpdateType.DueDateChange;
                            break;
                        case "DueSoonDate":
                            systemUpdateType = MobileSystemUpdateType.DueSoonDateChange;
                            break;
                    }
                }

                return new ConnectionRequestActivityBag
                {
                    Key = $"{MobileActivityEntryType.SystemUpdate}_{IdHasher.Instance.GetHash( r.Id )}",
                    EntryType = MobileActivityEntryType.SystemUpdate,
                    EntryDateTime = r.CreatedDateTime?.ToRockDateTimeOffset(),
                    PersonName = r.CreatedBy,
                    UpdateType = systemUpdateType,
                    PreviousValue = previousValue,
                    NewValue = newValue
                };
            } ) );

            // Communications (email / SMS) related to this request via raw SQL.
            var communicationSql = @"
SELECT
    c.[Id],
    c.[CommunicationType],
    c.[Subject],
    c.[SMSMessage],
    c.[CreatedDateTime],
    p.[NickName],
    p.[LastName],
    p.[PhotoId],
    p.[Gender],
    bf.[Guid] AS BinaryFileGuid,
    bf.[FileName]
FROM [RelatedEntity] re
INNER JOIN [Communication] c
    ON c.[Id] = re.[TargetEntityId]
LEFT JOIN [PersonAlias] pa
    ON pa.[Id] = c.[CreatedByPersonAliasId]
LEFT JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
LEFT JOIN [CommunicationAttachment] ca
    ON ca.[CommunicationId] = c.[Id]
LEFT JOIN [BinaryFile] bf
    ON bf.[Id] = ca.[BinaryFileId]
WHERE re.[SourceEntityTypeId] = @SourceEntityTypeId
  AND re.[SourceEntityId] = @SourceEntityId
  AND re.[TargetEntityTypeId] = @TargetEntityTypeId;
";

            var communicationRows = RockContext.Database
                .SqlQuery<CommunicationRow>( communicationSql,
                    new SqlParameter( "@SourceEntityTypeId", connectionRequestEntityTypeId ),
                    new SqlParameter( "@SourceEntityId", connectionRequest.Id ),
                    new SqlParameter( "@TargetEntityTypeId", communicationEntityTypeId ) )
                .ToList();

            // The attachment join can return one row per attachment, so group by
            // communication and fold the distinct attachments back together.
            entries.AddRange( communicationRows
                .Where( r => r.CommunicationType == CommunicationType.Email || r.CommunicationType == CommunicationType.SMS )
                .GroupBy( r => new
                {
                    r.Id,
                    r.CommunicationType,
                    r.Subject,
                    r.SMSMessage,
                    r.CreatedDateTime,
                    r.NickName,
                    r.LastName,
                    r.PhotoId,
                    r.Gender
                } )
                .Select( g =>
                {
                    var createdBy = $"{g.Key.NickName ?? string.Empty} {g.Key.LastName ?? string.Empty}".Trim();

                    if ( createdBy.IsNullOrWhiteSpace() )
                    {
                        createdBy = "Unknown Person";
                    }

                    string title;
                    string content = null;

                    if ( g.Key.CommunicationType == CommunicationType.SMS )
                    {
                        title = "SMS";
                        content = g.Key.SMSMessage.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        title = $"Email: {g.Key.Subject}";
                    }

                    var attachments = g
                        .Where( x => x.BinaryFileGuid.HasValue )
                        .GroupBy( x => x.BinaryFileGuid.Value )
                        .Select( fileGroup => fileGroup.First() )
                        .Select( x => new ListItemViewModel
                        {
                            Value = MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetFileUrl( x.BinaryFileGuid.Value ) ),
                            Text = x.FileName
                        } )
                        .ToList();

                    return new ConnectionRequestActivityBag
                    {
                        Key = $"{MobileActivityEntryType.Communication}_{IdHasher.Instance.GetHash( g.Key.Id )}",
                        EntryType = MobileActivityEntryType.Communication,
                        EntryDateTime = g.Key.CreatedDateTime?.ToRockDateTimeOffset(),
                        PersonName = createdBy,
                        PhotoUrl = BuildPersonPhotoUrl( g.Key.PhotoId ),
                        AuthorGender = ( Rock.Common.Mobile.Enums.Gender ) ( g.Key.Gender ?? 0 ),
                        Title = title,
                        Content = content,
                        Attachments = attachments
                    };
                } ) );

            // Request notes the current person is permitted to view.
            var connectionRequestNoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE.AsGuid() ).Id;
            var noteService = new NoteService( RockContext );

            var connectionRequestNoteQuery = noteService.Queryable()
                .Include( n => n.CreatedByPersonAlias.Person )
                .Where( n => n.NoteTypeId == connectionRequestNoteTypeId && n.EntityId == connectionRequest.Id );

            connectionRequestNoteQuery = connectionRequestNoteQuery.AreViewableBy( RequestContext.CurrentPerson?.Id ?? 0 );

            var connectionRequestNotes = connectionRequestNoteQuery.ToList()
                .Where( n => n.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) );

            entries.AddRange( connectionRequestNotes.Select( n => new ConnectionRequestActivityBag
            {
                Key = $"{MobileActivityEntryType.RequestNote}_{IdHasher.Instance.GetHash( n.Id )}",
                EntryType = MobileActivityEntryType.RequestNote,
                EntryDateTime = n.CreatedDateTime?.ToRockDateTimeOffset(),
                PersonName = n.CreatedByPersonAlias?.Person?.FullName,
                PhotoUrl = BuildPersonPhotoUrl( n.CreatedByPersonAlias?.Person?.PhotoId ),
                AuthorGender = ( Rock.Common.Mobile.Enums.Gender ) ( int ) ( n.CreatedByPersonAlias?.Person?.Gender ?? Gender.Unknown ),
                Title = "Request Note",
                Content = n.Text
            } ) );

            return entries.OrderByDescending( e => e.EntryDateTime ).ToList();
        }

        /// <summary>
        /// Removes a trailing bracketed id (e.g. " [123]") that Rock appends to
        /// some history values, leaving only the human-readable label. Ports the
        /// web Connections Hub StripBracketId helper.
        /// </summary>
        /// <param name="value">The history value to clean.</param>
        /// <returns>The value without a trailing bracketed id, or <c>null</c>.</returns>
        private static string StripBracketId( string value )
        {
            return value.IsNullOrWhiteSpace()
                ? null
                : Regex.Replace( value, @"\s*\[\d+\]\s*$", "" );
        }

        /// <summary>
        /// Builds the absolute photo URL for a person's photo, or <c>null</c>
        /// when they have none (callers that show an avatar render the initials
        /// fallback in that case).
        /// </summary>
        /// <param name="photoId">The person's photo binary file id.</param>
        /// <returns>The absolute photo URL, or <c>null</c>.</returns>
        private static string BuildPersonPhotoUrl( int? photoId )
        {
            return photoId.HasValue
                ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( photoId.Value ) )
                : null;
        }

        /// <summary>
        /// Gets the friendly display text for a connection state, matching the
        /// web's enum display names (only Future Follow Up differs from the enum
        /// name). Used when composing a state-change system update.
        /// </summary>
        /// <param name="state">The connection state.</param>
        /// <returns>The friendly display text.</returns>
        private static string GetConnectionStateDisplay( ConnectionState state )
        {
            return state == ConnectionState.FutureFollowUp ? "Future Follow Up" : state.ToString();
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A history row read by <see cref="GetActivityEntries"/> and mapped to a
        /// system update feed entry.
        /// </summary>
        private class HistoryRow
        {
            public int Id { get; set; }

            public string CreatedBy { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public string Verb { get; set; }

            public string ValueName { get; set; }

            public string NewValue { get; set; }

            public string OldValue { get; set; }
        }

        /// <summary>
        /// A communication row (one per attachment, folded together in memory)
        /// read by <see cref="GetActivityEntries"/> and mapped to a communication
        /// feed entry.
        /// </summary>
        private class CommunicationRow
        {
            public int Id { get; set; }

            public CommunicationType CommunicationType { get; set; }

            public string Subject { get; set; }

            public string SMSMessage { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? PhotoId { get; set; }

            // Nullable because the author Person is LEFT JOINed and may be absent.
            public int? Gender { get; set; }

            public Guid? BinaryFileGuid { get; set; }

            public string FileName { get; set; }
        }

        #endregion
    }
}

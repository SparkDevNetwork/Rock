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
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Connection.AddConnectionRequest;
using Rock.Common.Mobile.ViewModel;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Model.Connection.ConnectionOpportunity.Options;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

using Detail = Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetail;
using MobileConnectionState = Rock.Common.Mobile.Enums.ConnectionState;

using ConnectionState = Rock.Model.ConnectionState;
using GroupMemberStatus = Rock.Model.GroupMemberStatus;

namespace Rock.Blocks.Mobile.Connection
{
    /// <summary>
    /// Creates a new connection request through a multi-step wizard: type,
    /// opportunity, main details (requester, state, status, campus, source),
    /// additional details (connector, placement group, comments) and custom
    /// attributes. Adapted from the web Connections Hub Add Connection Request
    /// modal, enhanced with placement groups and custom attributes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Add Connection Request" )]
    [Category( "Mobile > Connection" )]
    [Description( "Creates a new connection request through a multi-step wizard." )]
    [IconCssClass( "ti ti-plus" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [MobileNavigationActionField( "Post Save Action",
        Description = "The navigation action to perform after the request is saved. 'ConnectionRequest' is passed as a route parameter with the new request's IdKey.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKey.PostSaveAction,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "553609B0-49E3-4E52-9D63-7F10C03D249E" )]
    [Rock.SystemGuid.BlockTypeGuid( "5A198A75-177C-4A2A-8558-BFB5A4EFCB30" )]
    public class AddConnectionRequest : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string PostSaveAction = "PostSaveAction";
        }

        /// <summary>
        /// The page parameter keys this block reads.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The IdKey of a connection opportunity the wizard is locked to,
            /// passed by the Connection Opportunity Detail block's Add button.
            /// When present the shell skips the Type and Opportunity steps.
            /// </summary>
            public const string ConnectionOpportunity = "ConnectionOpportunity";
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
            return new Rock.Common.Mobile.Blocks.Connection.AddConnectionRequest.Configuration
            {
                PostSaveAction = GetPostSaveAction()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the cascading options for the wizard's current state. Called
        /// once per step transition with progressively more context. Returns
        /// only the fields relevant to the supplied context; the shell merges
        /// each response into its cached options.
        /// </summary>
        /// <param name="request">The cascading options request.</param>
        /// <returns>The options for the supplied context.</returns>
        [BlockAction]
        public BlockActionResult GetOptions( GetOptionsRequestBag request )
        {
            request = request ?? new GetOptionsRequestBag();

            var currentPerson = RequestContext.CurrentPerson;

            // The set of connection types the person can actually add a request
            // under, resolved once and reused for both the Type step's list and
            // the authorization checks below so the wizard's gate cannot drift
            // from the one the list blocks use to offer the Add button.
            var addAuthorizedTypeIds = ConnectionRequestAuthorization.GetAddAuthorizedConnectionTypeIds( RockContext, currentPerson );

            var response = new GetOptionsResponseBag
            {
                // Always returned.
                Types = GetAuthorizedTypeOptions( addAuthorizedTypeIds ),
                Campuses = GetActiveCampuses()
            };

            var typeIdKey = request.TypeIdKey;
            var opportunityIdKey = request.OpportunityIdKey;

            // On the first call (no type/opportunity supplied) honor a locked
            // ConnectionOpportunity page parameter, resolving the type and
            // opportunity and returning the locked context so the shell can skip
            // the Type and Opportunity steps.
            ConnectionOpportunity lockedOpportunity = null;
            var lockedOpportunityIdKey = PageParameter( PageParameterKey.ConnectionOpportunity );

            if ( lockedOpportunityIdKey.IsNotNullOrWhiteSpace()
                && typeIdKey.IsNullOrWhiteSpace()
                && opportunityIdKey.IsNullOrWhiteSpace() )
            {
                lockedOpportunity = new ConnectionOpportunityService( RockContext ).Get( lockedOpportunityIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( lockedOpportunity == null || !lockedOpportunity.IsActive || !lockedOpportunity.ConnectionType.IsActive )
                {
                    return ActionBadRequest( "The specified connection opportunity is not available." );
                }

                if ( !ConnectionRequestAuthorization.CanAddRequest( RockContext, lockedOpportunity, currentPerson ) )
                {
                    return ActionUnauthorized( "You are not authorized to add a request for this opportunity." );
                }

                typeIdKey = lockedOpportunity.ConnectionType.IdKey;
                opportunityIdKey = lockedOpportunity.IdKey;

                response.LockedContext = new LockedContextBag
                {
                    TypeIdKey = typeIdKey,
                    OpportunityIdKey = opportunityIdKey
                };
            }

            // The requester's primary campus, used to default the Campus field.
            if ( request.RequesterPersonGuid.HasValue )
            {
                response.RequesterPrimaryCampusGuid = GetRequesterPrimaryCampusGuid( request.RequesterPersonGuid.Value );
            }

            // Resolve the opportunity (and its type) when supplied; otherwise
            // resolve just the type when supplied.
            ConnectionOpportunity opportunity = null;
            ConnectionType connectionType = null;

            if ( opportunityIdKey.IsNotNullOrWhiteSpace() )
            {
                opportunity = lockedOpportunity ?? new ConnectionOpportunityService( RockContext ).Get( opportunityIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( opportunity == null || !opportunity.IsActive )
                {
                    return ActionBadRequest( "The specified connection opportunity is not available." );
                }

                connectionType = opportunity.ConnectionType;
            }
            else if ( typeIdKey.IsNotNullOrWhiteSpace() )
            {
                connectionType = new ConnectionTypeService( RockContext ).Get( typeIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( connectionType == null )
                {
                    return ActionBadRequest( "The specified connection type is not available." );
                }
            }

            // Type-level options (opportunities, sources, default state).
            if ( connectionType != null )
            {
                if ( !addAuthorizedTypeIds.Contains( connectionType.Id ) )
                {
                    return ActionUnauthorized( "You are not authorized to add a request for this connection type." );
                }

                response.Opportunities = GetOpportunityOptions( connectionType, currentPerson );
                response.RequestSources = connectionType.ConnectionTypeSources
                    .OrderBy( s => s.Name )
                    .Select( s => new ListItemViewModel
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name
                    } )
                    .ToList();
                response.DefaultState = MobileConnectionState.Active;
            }

            // Opportunity-level options (statuses, connectors, placement, attributes).
            if ( opportunity != null && connectionType != null )
            {
                response.AvailableStatuses = connectionType.ConnectionStatuses
                    .OrderBy( s => s.Order )
                    .ThenBy( s => s.Name )
                    .Select( s => new Detail.ConnectionStatusItemBag
                    {
                        Value = s.Guid,
                        Name = s.Name,
                        Color = s.HighlightColor,
                        Order = s.Order,
                        IsNoteRequiredOnCompletion = s.IsNoteRequiredOnCompletion,
                        IsDefault = s.IsDefault,
                        IsDisabled = false
                    } )
                    .ToList();

                response.DefaultStatusGuid = connectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => ( Guid? ) s.Guid )
                    .FirstOrDefault()
                    ?? connectionType.ConnectionStatuses
                        .OrderBy( s => s.Order )
                        .Select( s => ( Guid? ) s.Guid )
                        .FirstOrDefault();

                response.AvailableConnectors = GetAvailableConnectors( opportunity.Id, request.CampusGuid );
                response.PlacementGroups = GetPlacementGroupItems( opportunity.Id );
                response.CustomAttributes = GetNewRequestAttributes( opportunity );
            }

            return ActionOk( response );
        }

        /// <summary>
        /// Gets the group-member attributes that apply to a chosen placement
        /// group and role, fetched lazily by the wizard's Placement section as
        /// the group / role selection changes. Adapted from the Connection
        /// Request Detail V2 block's equivalent action.
        /// </summary>
        /// <param name="request">The payload carrying the opportunity, group and role.</param>
        /// <returns>The member attributes to edit.</returns>
        [BlockAction]
        public BlockActionResult GetPlacementGroupMemberAttributes( GetPlacementGroupMemberAttributesRequestBag request )
        {
            if ( request == null || request.OpportunityIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var opportunity = new ConnectionOpportunityService( RockContext ).Get( request.OpportunityIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( opportunity == null )
            {
                return ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            if ( !ConnectionRequestAuthorization.CanAddRequest( RockContext, opportunity, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to add a request for this opportunity." );
            }

            // Validate the chosen group + role are a real placement option for
            // the opportunity.
            var validPlacementGroups = GetPlacementGroupItems( opportunity.Id );
            var placementGroup = validPlacementGroups.FirstOrDefault( g => g.Value == request.GroupGuid );
            var placementRole = placementGroup?.Roles.FirstOrDefault( r => r.Value == request.GroupMemberRoleGuid );

            if ( placementGroup == null || placementRole == null )
            {
                return ActionBadRequest( "Invalid placement group selection." );
            }

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

            return ActionOk( new GetPlacementGroupMemberAttributesResponseBag
            {
                Attributes = GetEditableAttributeValues( groupMember )
            } );
        }

        /// <summary>
        /// Validates and saves a new connection request, including placement and
        /// custom attribute values, and returns the new request's IdKey with the
        /// post-save navigation action.
        /// </summary>
        /// <param name="request">The save payload.</param>
        /// <returns>The new request's IdKey and the post-save action.</returns>
        [BlockAction]
        public BlockActionResult SaveConnectionRequest( SaveConnectionRequestBag request )
        {
            if ( request == null || request.OpportunityIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing request options." );
            }

            if ( request.RequesterPersonGuid == Guid.Empty )
            {
                return ActionBadRequest( "A requester is required." );
            }

            var opportunity = new ConnectionOpportunityService( RockContext ).Get( request.OpportunityIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( opportunity == null || !opportunity.IsActive || !opportunity.ConnectionType.IsActive )
            {
                return ActionBadRequest( "The specified connection opportunity is not available." );
            }

            var connectionType = opportunity.ConnectionType;

            if ( !ConnectionRequestAuthorization.CanAddRequest( RockContext, opportunity, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to add a request for this opportunity." );
            }

            var personAliasService = new PersonAliasService( RockContext );

            // Requester (stored as a person alias).
            var requesterPersonAliasId = personAliasService.GetPrimaryAliasId( request.RequesterPersonGuid );

            if ( !requesterPersonAliasId.HasValue )
            {
                return ActionBadRequest( "Invalid requester." );
            }

            // Status (must belong to the request's type).
            var status = new ConnectionStatusService( RockContext ).Queryable()
                .FirstOrDefault( s => s.ConnectionTypeId == connectionType.Id && s.Guid == request.StatusGuid );

            if ( status == null )
            {
                return ActionBadRequest( "Invalid connection status." );
            }

            // State (Active / Inactive / Future Follow Up; Connected is not a
            // valid initial state).
            var state = ( ConnectionState ) ( int ) request.State;

            if ( state == ConnectionState.Connected )
            {
                return ActionBadRequest( "A new request cannot be created as connected." );
            }

            // Campus.
            int? campusId = null;

            if ( request.CampusGuid.HasValue )
            {
                campusId = CampusCache.GetId( request.CampusGuid.Value );

                if ( !campusId.HasValue )
                {
                    return ActionBadRequest( "Invalid campus." );
                }
            }

            // Connector (validated against the available connectors).
            int? connectorPersonAliasId = null;

            if ( request.ConnectorPersonGuid.HasValue )
            {
                var isValidConnector = GetAvailableConnectors( opportunity.Id, null )
                    .Any( c => c.PersonGuid == request.ConnectorPersonGuid.Value );

                connectorPersonAliasId = personAliasService.GetPrimaryAliasId( request.ConnectorPersonGuid.Value );

                if ( !isValidConnector || !connectorPersonAliasId.HasValue )
                {
                    return ActionBadRequest( "Invalid connector." );
                }
            }

            // Request source (a ConnectionTypeSource of the request's type).
            int? sourceId = null;

            if ( request.ConnectionTypeSourceGuid.HasValue )
            {
                sourceId = connectionType.ConnectionTypeSources
                    .Where( s => s.Guid == request.ConnectionTypeSourceGuid.Value )
                    .Select( s => ( int? ) s.Id )
                    .FirstOrDefault();

                if ( !sourceId.HasValue )
                {
                    return ActionBadRequest( "Invalid request source." );
                }
            }

            var connectionRequest = new ConnectionRequest
            {
                ConnectionTypeId = connectionType.Id,
                ConnectionOpportunityId = opportunity.Id,
                PersonAliasId = requesterPersonAliasId.Value,
                ConnectionStatusId = status.Id,
                ConnectionState = state,
                CampusId = campusId,
                ConnectorPersonAliasId = connectorPersonAliasId,
                ConnectionTypeSourceId = sourceId,
                Comments = request.Comments
            };

            if ( state == ConnectionState.FutureFollowUp )
            {
                if ( !request.FollowupDate.HasValue )
                {
                    return ActionBadRequest( "A Follow-Up Date is required." );
                }

                connectionRequest.FollowupDate = request.FollowupDate.Value.DateTime;
            }

            // Placement group (group + role + status must all be present or all
            // absent).
            var placementError = ApplyPlacementGroup( connectionRequest, opportunity, request );

            if ( placementError != null )
            {
                return placementError;
            }

            new ConnectionRequestService( RockContext ).Add( connectionRequest );
            RockContext.SaveChanges();

            // Custom attribute values (the request now has an id so attributes
            // can be loaded against it).
            if ( request.AttributeValues != null )
            {
                connectionRequest.LoadAttributes( RockContext );
                connectionRequest.SetPublicAttributeValues( request.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
            }

            // Record the connector-assigned activity when a connector was set.
            if ( connectionRequest.ConnectorPersonAliasId.HasValue )
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

            return ActionOk( new SaveConnectionRequestResponseBag
            {
                ConnectionRequestIdKey = connectionRequest.IdKey,
                PostSaveAction = GetPostSaveAction()
            } );
        }

        #endregion

        #region Private Methods - Options

        /// <summary>
        /// Gets the connection types the current person may actually add a
        /// request under, ordered. A type qualifies when at least one of its
        /// active opportunities passes the same add gate the list blocks use to
        /// decide whether to offer the Add button at all, so the wizard never
        /// lists a type whose Opportunity step would come back empty.
        /// </summary>
        /// <remarks>
        /// The add gate is the only filter applied here. Layering
        /// <c>ConnectionTypeService.GetViewAuthorizedConnectionTypes</c> on top
        /// would be wrong: that method resolves VIEW on the type (plus a
        /// self-assigned-connector grant for request-secured types) and knows
        /// nothing about connector groups, so it would filter out exactly the
        /// connectors the connector-group fallback just authorized.
        /// </remarks>
        /// <param name="addAuthorizedTypeIds">The identifiers of the connection types the person may add a request under.</param>
        /// <returns>The authorized connection type options.</returns>
        private List<ConnectionTypeOptionBag> GetAuthorizedTypeOptions( HashSet<int> addAuthorizedTypeIds )
        {
            if ( addAuthorizedTypeIds.Count == 0 )
            {
                return new List<ConnectionTypeOptionBag>();
            }

            var types = new ConnectionTypeService( RockContext )
                .GetConnectionTypesQuery( new ConnectionTypeQueryOptions { IncludeInactive = false } )
                .ToList();

            return types
                .Where( t => addAuthorizedTypeIds.Contains( t.Id ) )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new ConnectionTypeOptionBag
                {
                    IdKey = t.IdKey,
                    Name = t.Name,
                    IconCssClass = t.IconCssClass,

                    // The type has no Summary field, so the long Description is
                    // the only descriptive text; strip its HTML so the plain
                    // card label does not render raw tags.
                    Description = t.Description.StripHtml()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the active opportunities of a type the current person may add a
        /// request to, ordered. This is the same add gate the list blocks use,
        /// so the wizard cannot offer an opportunity whose save would come back
        /// Forbidden.
        /// </summary>
        /// <param name="connectionType">The connection type.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>The authorized opportunity options.</returns>
        private List<ConnectionOpportunityOptionBag> GetOpportunityOptions( ConnectionType connectionType, Person currentPerson )
        {
            var opportunityService = new ConnectionOpportunityService( RockContext );
            var query = opportunityService.GetConnectionOpportunitiesQuery( new ConnectionOpportunityQueryOptions
            {
                IncludeInactive = false,
                ConnectionTypeGuids = new List<Guid> { connectionType.Guid }
            } );

            var authorizedOpportunities = ConnectionRequestAuthorization.FilterToAddAuthorized( RockContext, query.ToList(), currentPerson );

            return authorizedOpportunities
                .OrderBy( o => o.Order )
                .ThenBy( o => o.Name )
                .Select( o => new ConnectionOpportunityOptionBag
                {
                    IdKey = o.IdKey,
                    Name = o.Name,
                    IconCssClass = o.IconCssClass,

                    // Show the public Summary (short intro), not the long
                    // Description (the full Lava/HTML body); strip any basic
                    // HTML so the plain card label does not render raw tags.
                    Description = o.Summary.StripHtml()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the active campuses as list items, ordered.
        /// </summary>
        /// <returns>The active campuses.</returns>
        private static List<ListItemViewModel> GetActiveCampuses()
        {
            return CampusCache.All( false )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .Select( c => new ListItemViewModel
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets a requester's primary campus Guid, or <c>null</c> when they have
        /// none, used to default the Campus field.
        /// </summary>
        /// <param name="requesterPersonGuid">The requester's person Guid.</param>
        /// <returns>The requester's primary campus Guid, or <c>null</c>.</returns>
        private Guid? GetRequesterPrimaryCampusGuid( Guid requesterPersonGuid )
        {
            var primaryCampusId = new PersonService( RockContext ).Queryable().AsNoTracking()
                .Where( p => p.Guid == requesterPersonGuid )
                .Select( p => p.PrimaryCampusId )
                .FirstOrDefault();

            return primaryCampusId.HasValue
                ? CampusCache.Get( primaryCampusId.Value )?.Guid
                : null;
        }

        /// <summary>
        /// Gets the connectors available for an opportunity, narrowed to the
        /// supplied campus, plus the current person (so the creator can assign
        /// themselves). Keyed by person Guid (the V2 convention).
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        /// <param name="campusGuid">The campus to narrow to, or <c>null</c> for no filter.</param>
        /// <returns>The available connectors.</returns>
        private List<ConnectorOptionBag> GetAvailableConnectors( int opportunityId, Guid? campusGuid )
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

            // Include the current person so the creator can assign themselves
            // (no campus, so always selectable).
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( currentPersonId.HasValue )
            {
                var currentPerson = new PersonService( RockContext ).Queryable().AsNoTracking()
                    .Where( p => p.Id == currentPersonId.Value )
                    .Select( p => new
                    {
                        p.Guid,
                        p.NickName,
                        p.LastName,
                        p.PhotoId,
                        CampusGuid = ( Guid? ) null
                    } )
                    .ToList();

                connectorList.AddRange( currentPerson );
            }

            return connectorList
                .Where( c => !campusGuid.HasValue || !c.CampusGuid.HasValue || c.CampusGuid.Value == campusGuid.Value )
                .GroupBy( c => c.Guid )
                .Select( g => g.First() )
                .OrderBy( c => c.LastName )
                .ThenBy( c => c.NickName )
                .Select( c => new ConnectorOptionBag
                {
                    PersonGuid = c.Guid,
                    FullName = $"{c.NickName} {c.LastName}".Trim(),
                    PhotoUrl = BuildPersonPhotoUrl( c.PhotoId ),
                    CampusGuid = c.CampusGuid
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the placement groups available for an opportunity, with their
        /// roles and per-role statuses. Adapted from the Connection Request
        /// Detail V2 block's placement query.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        /// <returns>The available placement groups.</returns>
        private List<Detail.PlacementGroupItemBag> GetPlacementGroupItems( int opportunityId )
        {
            var connectionOpportunityGroupConfigQuery = new ConnectionOpportunityGroupConfigService( RockContext ).Queryable()
                .Where( c => c.ConnectionOpportunityId == opportunityId );

            var availablePlacementGroups = GetAvailablePlacementGroupsQuery( opportunityId, RockContext )
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
                .Select( g => new Detail.PlacementGroupItemBag
                {
                    Value = g.Guid,
                    Name = $"{g.Name} ({( g.CampusName.IsNotNullOrWhiteSpace() ? g.CampusName : "No Campus" )})",
                    CampusGuid = g.CampusGuid,
                    Roles = g.Configs
                        .GroupBy( c => new { c.Guid, c.Name } )
                        .Select( cgrp => new Detail.PlacementGroupRoleItemBag
                        {
                            Value = cgrp.Key.Guid,
                            Name = cgrp.Key.Name,
                            Statuses = cgrp.Select( s => ( Detail.GroupMemberStatus ) ( int ) s.Status ).ToList()
                        } )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets a query of all groups that are possible placement groups for an
        /// opportunity. Adapted from the Connection Request Detail V2 block.
        /// </summary>
        /// <param name="connectionOpportunityId">The opportunity identifier.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A queryable of the placement groups.</returns>
        private static IQueryable<Rock.Model.Group> GetAvailablePlacementGroupsQuery( int connectionOpportunityId, RockContext rockContext )
        {
            var opportunityService = new ConnectionOpportunityService( rockContext );

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

            return specificConfigQuery.Union( allGroupsOfTypeQuery )
                .Where( g => g.IsActive && !g.IsArchived );
        }

        /// <summary>
        /// Gets the editable custom attributes for a new request of an
        /// opportunity's type (no saved values yet).
        /// </summary>
        /// <param name="opportunity">The opportunity.</param>
        /// <returns>The editable custom attributes.</returns>
        private List<ClientEditableAttributeValueViewModel> GetNewRequestAttributes( ConnectionOpportunity opportunity )
        {
            var connectionRequest = new ConnectionRequest
            {
                ConnectionOpportunityId = opportunity.Id,
                ConnectionOpportunity = opportunity,
                ConnectionTypeId = opportunity.ConnectionTypeId
            };

            connectionRequest.LoadAttributes( RockContext );

            return GetEditableAttributeValues( connectionRequest );
        }

        #endregion

        #region Private Methods - Persistence

        /// <summary>
        /// Applies the chosen placement group, role, status and member-attribute
        /// values to a new request (or clears them when none is chosen). All
        /// three of group, role and status are required together. Adapted from
        /// the Connection Request Detail V2 block's ApplyPlacementGroup.
        /// </summary>
        /// <param name="request">The request being built.</param>
        /// <param name="opportunity">The request's opportunity.</param>
        /// <param name="saveBag">The save payload.</param>
        /// <returns>An error result when the placement is invalid; otherwise <c>null</c>.</returns>
        private BlockActionResult ApplyPlacementGroup( ConnectionRequest request, ConnectionOpportunity opportunity, SaveConnectionRequestBag saveBag )
        {
            var hasAnyPlacementValue = saveBag.PlacementGroupGuid.HasValue
                || saveBag.PlacementGroupMemberRoleGuid.HasValue
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
            if ( !saveBag.PlacementGroupGuid.HasValue || !saveBag.PlacementGroupMemberRoleGuid.HasValue || !saveBag.PlacementGroupMemberStatus.HasValue )
            {
                return ActionBadRequest( "A placement group requires a group, a role and a status." );
            }

            var validPlacementGroups = GetPlacementGroupItems( opportunity.Id );
            var placementGroup = validPlacementGroups.FirstOrDefault( g => g.Value == saveBag.PlacementGroupGuid.Value );
            var placementRole = placementGroup?.Roles.FirstOrDefault( r => r.Value == saveBag.PlacementGroupMemberRoleGuid.Value );

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
        /// Creates a new <see cref="ConnectionRequestActivity"/> recording that
        /// the request was assigned to a connector, or <c>null</c> when no
        /// connector is set or the activity type is missing. Adapted from the
        /// existing Add Connection Request block.
        /// </summary>
        /// <param name="connectionRequest">The request that was assigned a connector.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>The new activity, or <c>null</c>.</returns>
        private static ConnectionRequestActivity CreateAssignedActivity( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            if ( !connectionRequest.ConnectorPersonAliasId.HasValue )
            {
                return null;
            }

            var assignedActivityGuid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
            var assignedActivityId = new ConnectionActivityTypeService( rockContext ).Queryable()
                .Where( t => t.Guid == assignedActivityGuid )
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

        #endregion

        #region Private Methods - Helpers

        /// <summary>
        /// Maps an entity's editable attributes (for a new, unsaved entity) into
        /// the mobile editable attribute view model, seeding the editor value
        /// from the public edit value. The shell filters out field types it
        /// cannot render. Mirrors the Connection Request Detail V2 attribute
        /// mapping.
        /// </summary>
        /// <param name="entity">The attribute-bearing entity.</param>
        /// <returns>The ordered editable attribute values.</returns>
        private List<ClientEditableAttributeValueViewModel> GetEditableAttributeValues( Rock.Attribute.IHasAttributes entity )
        {
            var attributes = entity.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true )
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

            foreach ( var kvp in entity.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: true ) )
            {
                if ( attributes.TryGetValue( kvp.Key, out var attribute ) )
                {
                    attribute.Value = kvp.Value;
                }
            }

            return attributes.Values.OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Builds the absolute photo URL for a person's photo, or <c>null</c>
        /// when they have none.
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
        /// Gets the configured post-save navigation action, defaulting to
        /// popping the wizard.
        /// </summary>
        /// <returns>The post-save navigation action.</returns>
        private MobileNavigationActionViewModel GetPostSaveAction()
        {
            return GetAttributeValue( AttributeKey.PostSaveAction ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel();
        }

        #endregion
    }
}

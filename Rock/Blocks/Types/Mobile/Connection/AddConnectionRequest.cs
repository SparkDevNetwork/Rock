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

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rock.Common.Mobile.ViewModel;
using System.Linq;
using Rock.ClientService.Connection.ConnectionOpportunity;
using Rock.Model.Connection.ConnectionOpportunity.Options;
using Rock.Security;
using Rock.Common.Mobile.Blocks.Connection.AddConnectionRequest;
using Rock.Web.Cache;
using Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetail;
using GroupMemberStatus = Rock.Model.GroupMemberStatus;
using System.Data.Entity;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// A block used to create a new Connection Request.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Add Connection Request" )]
    [Category( "Mobile > Connection" )]
    [Description( "Allows an individual to create and add a new Connection Request." )]
    [IconCssClass( "fa fa-plus" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [ConnectionTypesField( "Connection Types",
        Description = "The connection types to limit this block to. Will only display if the person has access to see them. None selected means all will be available.",
        Key = AttributeKey.ConnectionTypes,
        IsRequired = false,
        Order = 0 )]

    [MobileNavigationActionField( "Post Save Action",
        Description = "The navigation action to perform on save. 'ConnectionRequestIdKey' will be passed as a query string parameter.",
        IsRequired = true,
        DefaultValue = MobileNavigationActionFieldAttribute.PushPageValue,
        Key = AttributeKey.PostSaveAction,
        Order = 1 )]

    [MobileNavigationActionField( "Post Cancel Action",
        Description = "The navigation action to perform when the cancel button is pressed.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKey.PostCancelAction,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_ADD_CONNECTION_REQUEST )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_ADD_CONNECTION_REQUEST )]
    public class AddConnectionRequest : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The page parameter keys for this block.
        /// </summary>
        public static class PageParameterKey
        {
            /// <summary>
            /// The requester id key key.
            /// </summary>
            public const string RequesterIdKey = "RequesterIdKey";
        }

        /// <summary>
        /// The attribute keys for the <see cref="AddConnectionRequest"/> block.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The connection types key.
            /// </summary>
            public const string ConnectionTypes = "ConnectionTypes";

            /// <summary>
            /// The post save action key.
            /// </summary>
            public const string PostSaveAction = "PostSaveAction";

            /// <summary>
            /// The post cancel action key.
            /// </summary>
            public const string PostCancelAction = "PostCancelAction";
        }

        #endregion

        #region Block Attributes

        /// <summary>
        /// The list of connection types that this block is limited to.
        /// </summary>
        protected List<Guid> ConnectionTypes => GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// The post save navigation action.
        /// </summary>
        internal MobileNavigationAction PostSaveAction => GetAttributeValue( AttributeKey.PostSaveAction ).FromJsonOrNull<MobileNavigationAction>() ?? new MobileNavigationAction();

        /// <summary>
        /// The post cancel navigation action.
        /// </summary>
        internal MobileNavigationAction PostCancelAction => GetAttributeValue( AttributeKey.PostCancelAction ).FromJsonOrNull<MobileNavigationAction>() ?? new MobileNavigationAction();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                PostSaveAction = PostSaveAction,
                PostCancelAction = PostCancelAction
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a requester from the IdKey.
        /// </summary>
        /// <param name="idKey"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private ListItemViewModel GetRequester( string idKey, RockContext rockContext )
        {
            var person = new PersonService( rockContext ).Get( idKey );

            return new ListItemViewModel
            {
                Text = person.FullName,
                Value = person.IdKey
            };
        }

        /// <summary>
        /// Gets the connection types for the current person.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<ConnectionTypeListItemBag> GetConnectionTypes( RockContext rockContext )
        {
            var connectionTypeService = new ConnectionTypeService( rockContext );
            var filterOptions = new ConnectionTypeQueryOptions
            {
                IncludeInactive = false
            };

            // Get the connection types.
            var qry = connectionTypeService.GetConnectionTypesQuery( filterOptions );
            var types = connectionTypeService.GetViewAuthorizedConnectionTypes( qry, RequestContext.CurrentPerson );

            // Check the Connection Types block setting to see if we should filter
            // down even more.
            if ( ConnectionTypes.Any() )
            {
                types = types.Where( ct => ConnectionTypes.Contains( ct.Guid ) ).ToList();
            }

            return types.Select( ct => new ConnectionTypeListItemBag
            {
                Text = ct.Name,
                Value = ct.IdKey,
                EnableFutureFollowup = ct.EnableFutureFollowup,
            } ).ToList();
        }

        /// <summary>
        /// Gets the connection opportunities for the specified connection type.
        /// </summary>
        /// <param name="connectionType"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<ListItemViewModel> GetConnectionOpportunities( ConnectionType connectionType, RockContext rockContext )
        {
            var opportunityService = new ConnectionOpportunityService( rockContext );
            var opportunityClientService = new ConnectionOpportunityClientService( rockContext, RequestContext.CurrentPerson );

            var filterOptions = new ConnectionOpportunityQueryOptions
            {
                IncludeInactive = false,
                ConnectionTypeGuids = new List<Guid> { connectionType.Guid }
            };

            // Put all the opportunities in memory so we can check security.
            var qry = opportunityService.GetConnectionOpportunitiesQuery( filterOptions );
            var opportunities = qry.ToList()
                .Where( o => o.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) );

            return opportunities.ToList().Select( o => new ListItemViewModel
            {
                Text = o.Name,
                Value = o.IdKey
            } ).ToList();
        }

        /// <summary>
        /// Gets the opportunity status list items for the given connection type.
        /// </summary>
        /// <param name="connectionType">Connection type to query.</param>
        /// <returns>A list of list items that can be displayed.</returns>
        private static List<ListItemViewModel> GetOpportunityStatusListItems( ConnectionType connectionType )
        {
            return connectionType.ConnectionStatuses
                .OrderBy( s => s.Order )
                .OrderByDescending( s => s.IsDefault )
                .ThenBy( s => s.Name )
                .Select( s => new ListItemViewModel
                {
                    Value = s.IdKey,
                    Text = s.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the possible connectors for the specified connection request.
        /// All possible connectors are returned, campus filtering is not applied.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity id.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A list of connectors that are valid for the request.</returns>
        private List<ListItemViewModel> GetAvailableConnectors( int connectionOpportunityId, RockContext rockContext )
        {
            var additionalConnectorAliasIds = new List<int>();

            // Add the logged in person.
            if ( RequestContext.CurrentPerson != null )
            {
                additionalConnectorAliasIds.Add( RequestContext.CurrentPerson.PrimaryAliasId.Value );
            }

            return GetConnectionOpportunityConnectors( connectionOpportunityId, null, additionalConnectorAliasIds, rockContext )
                .Select( connector => new ListItemViewModel
                {
                    Text = connector.FullName,
                    Value = connector.Id
                } ).ToList();
        }

        /// <summary>
        /// Gets a list of connectors that match the specified criteria.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="campusGuid">The campus to limit connectors to.</param>
        /// <param name="additionalPersonAliasIds">The additional person alias ids.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A list of connectors that are valid for the request.</returns>
        private static List<ConnectorItemViewModel> GetConnectionOpportunityConnectors( int connectionOpportunityId, Guid? campusGuid, List<int> additionalPersonAliasIds, RockContext rockContext )
        {
            var connectorGroupService = new ConnectionOpportunityConnectorGroupService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            // Get the primary list of connectors for this connection opportunity.
            // Include all the currently active members of the groups and then
            // build the connector view model
            var connectorList = connectorGroupService.Queryable()
                .Where( a => a.ConnectionOpportunityId == connectionOpportunityId
                    && ( !campusGuid.HasValue || a.ConnectorGroup.Campus.Guid == campusGuid ) )
                .SelectMany( g => g.ConnectorGroup.Members )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .AsEnumerable()
                .Select( m => new ConnectorItemViewModel
                {
                    Guid = m.Person.Guid,
                    FullName = m.Person.FullName,
                    FirstName = m.Person.NickName,
                    LastName = m.Person.LastName,
                    Id = m.Person.IdKey
                } )
                .ToList();

            // If they specified any additional people to load then execute
            // a query to find just those people.
            if ( additionalPersonAliasIds != null && additionalPersonAliasIds.Any() )
            {
                var additionalPeople = personAliasService.Queryable()
                    .Where( pa => additionalPersonAliasIds.Contains( pa.Id ) )
                    .AsEnumerable()
                    .Select( pa => new ConnectorItemViewModel
                    {
                        Guid = pa.Guid,
                        FirstName = pa.Person.NickName,
                        FullName = pa.Person.FullName,
                        LastName = pa.Person.LastName,
                        CampusGuid = null,
                        Id = pa.Person.IdKey
                    } )
                    .ToList();

                connectorList.AddRange( additionalPeople );
            }

            // Distinct by both the person Guid and the CampusGuid. We could
            // still have duplicate people, but that will be up to the client
            // to sort out. Then apply final sorting.
            return connectorList.GroupBy( c => new { c.Guid, c.CampusGuid } )
                .Select( g => g.First() )
                .OrderBy( c => c.LastName )
                .ThenBy( c => c.FirstName )
                .ToList();
        }

        /// <summary>
        /// Gets all the attributes and values for the entity in a form
        /// suitable to use for editing.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <returns>A list of editable attribute values.</returns>
        private List<ClientEditableAttributeValueViewModel> GetPublicEditableAttributeValues( IHasAttributes request )
        {
            var attributes = request.GetPublicAttributesForEdit( RequestContext.CurrentPerson )
                .ToDictionary( kvp => kvp.Key, kvp => new ClientEditableAttributeValueViewModel
                {
                    AttributeGuid = kvp.Value.AttributeGuid,
                    // Categories = kvp.Value.Categories,
                    ConfigurationValues = kvp.Value.ConfigurationValues,
                    Description = kvp.Value.Description,
                    FieldTypeGuid = kvp.Value.FieldTypeGuid,
                    IsRequired = kvp.Value.IsRequired,
                    Key = kvp.Value.Key,
                    Name = kvp.Value.Name,
                    Order = kvp.Value.Order,
                    Value = ""
                } );

            request.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson )
                .ToList()
                .ForEach( kvp =>
                {
                    if ( attributes.ContainsKey( kvp.Key ) )
                    {
                        attributes[kvp.Key].Value = kvp.Value;
                    }
                } );

            return attributes.Select( kvp => kvp.Value ).OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Gets the placement groups for the specified connection request.
        /// </summary>
        /// <param name="connectionRequest"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<PlacementGroupListItemBag> GetPlacementGroups( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            var placementGroups = new List<PlacementGroupListItemBag>();
            var groups = new List<Group>();

            // First add any groups specifically configured for the opportunity
            var opportunityGroupIds = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroups.Select( o => o.Id ).ToList();
            if ( opportunityGroupIds.Any() )
            {
                groups = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroups
                    .Where( g =>
                        g.Group != null &&
                        g.Group.IsActive &&
                        !g.Group.IsArchived )
                .Select( g => g.Group )
                .ToList();
            }

            // Then get any groups that are configured with 'all groups of type'
            foreach ( var groupConfig in connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroupConfigs )
            {
                if ( groupConfig.UseAllGroupsOfType )
                {
                    var existingGroupIds = groups.Select( g => g.Id ).ToList();

                    groups.AddRange( new GroupService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( g =>
                            !existingGroupIds.Contains( g.Id ) &&
                            g.IsActive && !g.IsArchived &&
                            g.GroupTypeId == groupConfig.GroupTypeId )
                        .ToList() );
                }
            }

            // For each group, populate the roles and statuses.
            foreach ( var group in groups )
            {
                var groupConfigs = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroupConfigs.Where( g => g.GroupTypeId == group.GroupTypeId );

                List<RoleItemBag> roles = new List<RoleItemBag>();

                // Find each role that is configured for this group.
                foreach ( var groupConfig in groupConfigs )
                {
                    if ( groupConfig.GroupMemberRole != null )
                    {
                        roles.Add( new RoleItemBag
                        {
                            Text = groupConfig.GroupMemberRole.Name,
                            Value = groupConfig.GroupMemberRole.IdKey,
                            GroupTypeId = GroupTypeCache.Get( groupConfig.GroupTypeId ).IdKey
                        } );
                    }
                }

                // For each role, find the statuses that are configured for this group.
                foreach ( var role in roles )
                {
                    foreach ( var groupConfig in groupConfigs.Where( c => c.GroupMemberRole.IdKey == role.Value ) )
                    {
                        role.Statuses = role.Statuses ?? new List<ListItemViewModel>();

                        if ( !role.Statuses.Any( s => s.Value == groupConfig.GroupMemberStatus.ConvertToInt().ToString() ) )
                        {
                            role.Statuses.Add( new ListItemViewModel
                            {
                                Text = groupConfig.GroupMemberStatus.ToStringSafe(),
                                Value = groupConfig.GroupMemberStatus.ConvertToInt().ToString()
                            } );
                        }
                    }
                }

                placementGroups.Add( new PlacementGroupListItemBag
                {
                    Text = string.Format( "{0} ({1})", group.Name, group.Campus != null ? group.Campus.Name : "No Campus" ),
                    Value = group.IdKey,
                    Roles = roles,
                    GroupTypeId = group.GroupType.IdKey,
                    Campus = group.Campus != null ? new ListItemViewModel
                    {
                        Text = group.Campus.Name,
                        Value = group.Campus.IdKey
                    } : null
                } );
            }

            return placementGroups;
        }

        /// <summary>
        /// Saves a new connection request.
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="rockContext"></param>
        /// <returns>The IdKey of the new Connection Request.</returns>
        private string SaveConnectionRequest( SaveConnectionRequestRequestBag bag, RockContext rockContext )
        {
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var personService = new PersonService( rockContext );
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var connectionTypeService = new ConnectionTypeService( rockContext );

            // Structure the data for the connection request.
            var requesterId = personService.Get( bag.RequesterId )?.PrimaryAliasId;

            if ( !requesterId.HasValue )
            {
                return string.Empty;
            }

            var typeId = connectionTypeService.Get( bag.ConnectionTypeId ).Id;
            var opportunityId = connectionOpportunityService.Get( bag.ConnectionOpportunityId ).Id;
            var campusId = CampusCache.Get( bag.CampusId, false )?.Id;
            var statusId = DefinedValueCache.Get( bag.StatusId, false ).Id;

            int? connectorId = null;
            if ( bag.ConnectorId.IsNotNullOrWhiteSpace() )
            {
                connectorId = personService.Get( bag.ConnectorId )?.PrimaryAliasId;
            }

            int? placementGroupId = null;
            int? placementGroupMemberRoleId = null;
            GroupMemberStatus? placementGroupMemberStatus = null;

            if ( bag.PlacementGroupId.IsNotNullOrWhiteSpace() )
            {
                placementGroupId = new GroupService( rockContext ).Get( bag.PlacementGroupId )?.Id;

                if ( placementGroupId.HasValue && bag.PlacementGroupMemberRoleId.IsNotNullOrWhiteSpace() )
                {
                    placementGroupMemberRoleId = new GroupTypeRoleService( rockContext ).Get( bag.PlacementGroupMemberRoleId )?.Id;
                }

                placementGroupMemberStatus = ( Rock.Model.GroupMemberStatus ) bag.PlacementGroupMemberStatusValue;
            }

            var connectionState = bag.State.ToNative();
            var comments = bag.Comments;

            var connectionRequest = new ConnectionRequest
            {
                ConnectionTypeId = typeId,
                ConnectionOpportunityId = opportunityId,
                CampusId = campusId,
                Comments = comments,
                ConnectionState = connectionState,
                ConnectionStatusId = statusId,
                ConnectorPersonAliasId = connectorId,
                PersonAliasId = requesterId.Value,
            };

            if ( placementGroupId.HasValue )
            {
                connectionRequest.AssignedGroupId = placementGroupId.Value;
                connectionRequest.AssignedGroupMemberRoleId = placementGroupMemberRoleId;
                connectionRequest.AssignedGroupMemberStatus = placementGroupMemberStatus.HasValue ? placementGroupMemberStatus.Value : GroupMemberStatus.Active;
            }

            if ( connectionState == ConnectionState.FutureFollowUp && bag.FutureFollowupDate.HasValue )
            {
                connectionRequest.FollowupDate = bag.FutureFollowupDate.Value.DateTime;
            }

            connectionRequestService.Add( connectionRequest );
            rockContext.SaveChanges();

            // Set any custom request attribute values.
            if ( bag.AttributeValues != null )
            {
                connectionRequest.LoadAttributes();
                connectionRequest.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson );
            }

            // Add an activity that the connector was assigned or changed.
            if ( connectionRequest.ConnectorPersonAliasId.HasValue )
            {
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var activity = CreateAssignedActivity( connectionRequest, rockContext );

                if ( activity != null )
                {
                    connectionRequestActivityService.Add( activity );
                }
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                connectionRequest.SaveAttributeValues( rockContext );
            } );

            return connectionRequest.IdKey;
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionRequestActivity"/> for when
        /// a request is assigned to a connector.
        /// </summary>
        /// <remarks>
        /// This does not attach the new entity or save the changes.
        /// </remarks>
        /// <param name="connectionRequest">The connection request that was assigned a connector.</param>
        /// <param name="rockContext">The Rock database context to use for data lookups.</param>
        /// <returns>A new <see cref="ConnectionRequestActivity"/> or <c>null</c> if one is not needed.</returns>
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

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the connection types for the current person.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetConnectionTypes( GetConnectionTypesRequestBag requestBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                // Get the connection types and grab the name and guid.
                var connectionTypes = GetConnectionTypes( rockContext );

                return ActionOk( new GetConnectionTypesResponseBag
                {
                    ConnectionTypes = connectionTypes.ToList(),
                    Requester = GetRequester( requestBag.RequesterId, rockContext ),
                } );
            }
        }

        /// <summary>
        /// Gets the connection opportunities for the specified connection type.
        /// </summary>
        /// <param name="requestBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetConnectionOpportunities( GetConnectionOpportunitiesRequestBag requestBag )
        {
            if ( requestBag.ConnectionTypeId.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The connection type identifier key is required." );
            }

            using ( var rockContext = new RockContext() )
            {
                var connectionType = new ConnectionTypeService( rockContext ).GetNoTracking( requestBag.ConnectionTypeId );

                if ( connectionType == null )
                {
                    return ActionNotFound( "The connection type for that Id Key was not found." );
                }

                return ActionOk( new GetConnectionOpportunitiesResponseBag
                {
                    ConnectionOpportunities = GetConnectionOpportunities( connectionType, rockContext ),
                    ConnectionType = new ConnectionTypeListItemBag
                    {
                        Text = connectionType.Name,
                        Value = connectionType.IdKey,
                        EnableFutureFollowup = connectionType.EnableFutureFollowup
                    },
                    Requester = GetRequester( requestBag.RequesterId, rockContext )
                } );
            }
        }

        /// <summary>
        /// Gets additional data for the connection request.
        /// </summary>
        /// <param name="requestBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetConnectionRequestData( GetConnectionRequestDataRequestBag requestBag )
        {
            if ( requestBag.ConnectionOpportunityId.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The connection opportunity identifier key is required." );
            }

            using ( var rockContext = new RockContext() )
            {
                var opportunity = new ConnectionOpportunityService( rockContext )
                    .Get( requestBag.ConnectionOpportunityId );

                if ( opportunity == null )
                {
                    return ActionNotFound( "The opportunity for that Id Key was not found." );
                }

                var statuses = GetOpportunityStatusListItems( opportunity.ConnectionType );
                var connectors = GetAvailableConnectors( opportunity.Id, rockContext );

                // Create a new in-memory connection request to load the editable attributes.
                var connectionRequest = new Rock.Model.ConnectionRequest
                {
                    ConnectionOpportunityId = opportunity.Id,
                    ConnectionOpportunity = opportunity,
                    ConnectionTypeId = opportunity.ConnectionTypeId,
                };
                connectionRequest.LoadAttributes();
                rockContext.SaveChanges();

                var placementGroups = GetPlacementGroups( connectionRequest, rockContext );
                var attributes = GetPublicEditableAttributeValues( connectionRequest );

                return ActionOk( new GetConnectionRequestDataResponseBag
                {
                    Statuses = statuses,
                    Connectors = connectors,
                    Attributes = attributes,
                    PlacementGroups = placementGroups,
                    ConnectionOpportunity = new ListItemViewModel
                    {
                        Text = opportunity.Name,
                        Value = opportunity.IdKey
                    },
                    ConnectionType = new ConnectionTypeListItemBag
                    {
                        Text = opportunity.ConnectionType.Name,
                        Value = opportunity.ConnectionType.IdKey,
                        EnableFutureFollowup = opportunity.ConnectionType.EnableFutureFollowup
                    },
                    Requester = GetRequester( requestBag.RequesterId, rockContext )
                } );
            }
        }

        /// <summary>
        /// Saves a new connection request.
        /// </summary>
        /// <param name="requestBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveConnectionRequest( SaveConnectionRequestRequestBag requestBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var saveResult = SaveConnectionRequest( requestBag, rockContext );

                if( saveResult.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "There was an error creating the connection request." );
                }

                return ActionOk( new SaveConnectionRequestResponseBag
                {
                    Id = saveResult
                } );
            }
        }

        #endregion
    }
}

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
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.ClientService.Core.Campus;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetail;
using Rock.Core.NotificationMessageTypes;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using GroupMemberStatus = Rock.Model.GroupMemberStatus;
using MeetsGroupRequirement = Rock.Model.MeetsGroupRequirement;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the details of the given connection request for editing state, status, etc.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Request Detail" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the details of the given connection request for editing state, status, etc." )]
    [IconCssClass( "fa fa-id-card" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CodeEditorField( "Header Template",
        Description = "Lava template used to render the header above the connection request.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        Order = 0 )]

    [BlockTemplateField( "Activity Template (Legacy)",
        Description = "This is unused in shell V4. Previous to that version, this was the template used to render the activity history for the connection request.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL,
        DefaultValue = "D19A6D1A-BB4F-45FB-92DE-17EB97479F40",
        IsRequired = true,
        Key = AttributeKey.ActivityTemplate,
        Order = 1 )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page to link to when user taps on the profile button. PersonGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.PersonProfilePage,
        Order = 2 )]

    [LinkedPage(
        "Group Detail Page",
        Description = "Page to link to when user taps on the group. GroupGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.GroupDetailPage,
        Order = 3 )]

    [LinkedPage(
        "Workflow Page",
        Description = "Page to link to when user launches a workflow that requires interaction. WorkflowTypeGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.WorkflowPage,
        Order = 4 )]

    [LinkedPage(
        "Reminder Page",
        Description = "Page to link to when the reminder button is tapped.",
        IsRequired = false,
        Key = AttributeKey.ReminderPage,
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL )]
    public class ConnectionRequestDetail : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="ConnectionRequestDetail"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string HeaderTemplate = "HeaderTemplate";

            public const string ActivityTemplate = "ActivityTemplate";

            public const string PersonProfilePage = "PersonProfilePage";

            public const string GroupDetailPage = "GroupDetailPage";

            public const string WorkflowPage = "WorkflowPage";

            public const string ReminderPage = "ReminderPage";
        }

        /// <summary>
        /// Gets the header template.
        /// </summary>
        /// <value>
        /// The header template.
        /// </value>
        protected string HeaderTemplate => GetAttributeValue( AttributeKey.HeaderTemplate );

        /// <summary>
        /// Gets the activity template.
        /// </summary>
        /// <value>
        /// The activity template.
        /// </value>
        protected string ActivityTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.ActivityTemplate ) );

        /// <summary>
        /// Gets the person profile page unique identifier.
        /// </summary>
        /// <value>
        /// The person profile page unique identifier.
        /// </value>
        protected Guid? PersonProfilePageGuid => GetAttributeValue( AttributeKey.PersonProfilePage ).AsGuidOrNull();

        /// <summary>
        /// Gets the group detail page unique identifier.
        /// </summary>
        /// <value>
        /// The group detail page unique identifier.
        /// </value>
        protected Guid? GroupDetailPageGuid => GetAttributeValue( AttributeKey.GroupDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the workflow page unique identifier.
        /// </summary>
        /// <value>
        /// The workflow page unique identifier.
        /// </value>
        protected Guid? WorkflowPageGuid => GetAttributeValue( AttributeKey.WorkflowPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the reminder page unique identifier.
        /// </summary>
        protected Guid? ReminderPageGuid => GetAttributeValue( AttributeKey.ReminderPage ).AsGuidOrNull();


       #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 3 );

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Connection.ConnectionRequestDetail.Configuration
            {
                PersonProfilePageGuid = PersonProfilePageGuid,
                GroupDetailPageGuid = GroupDetailPageGuid,
                WorkflowPageGuid = WorkflowPageGuid,
                ReminderPageGuid = ReminderPageGuid,
                AreRemindersConfigured = CheckReminderConfiguration()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if there's any reminder with the ConnectionRequest entity type.
        /// </summary>
        private static bool CheckReminderConfiguration()
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestEntityTypeId = EntityTypeCache.Get( typeof( ConnectionRequest ) ).Id;

                var reminderTypesExist = new ReminderTypeService( rockContext )
                    .Queryable()
                    .Where( rt => rt.EntityTypeId == connectionRequestEntityTypeId )
                    .Any();

                return reminderTypesExist;
            }
        }

        /// <summary>
        /// Determines whether the connection request is critical.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>true</c> if the connection request is critical; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRequestCritical( ConnectionRequest request )
        {
            // Only a connection request with a status of type critical can be
            // considered critical.
            if ( !request.ConnectionStatus.IsCritical )
            {
                return false;
            }

            // Past due means it is a future follow-up state with a date on or
            // before today.
            var isPastDue = request.ConnectionState == ConnectionState.FutureFollowUp
                && request.FollowupDate.HasValue
                && request.FollowupDate.Value < RockDateTime.Today.AddDays( 1 );

            // If the status is critical and the state is either active or
            // past due then the request is critical.
            return request.ConnectionState == ConnectionState.Active || isPastDue;
        }

        /// <summary>
        /// Determines whether the connection request is considered to be idle.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>true</c> if the connection request is considered to be idle; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRequestIdle( ConnectionRequest request )
        {
            var daysUntilIdle = request.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle;
            var idleDate = RockDateTime.Now.AddDays( -daysUntilIdle );

            // Past due means it is a future follow-up state with a date on or
            // before today.
            var isPastDue = request.ConnectionState == ConnectionState.FutureFollowUp
                && request.FollowupDate.HasValue
                && request.FollowupDate.Value < RockDateTime.Today.AddDays( 1 );

            // A request can only be idle if it is the active state or a
            // past due follow up state.
            if ( request.ConnectionState == ConnectionState.Active || isPastDue )
            {
                var mostRecentActivityDateTime = request.ConnectionRequestActivities
                    .Where( ra => ra.CreatedDateTime.HasValue )
                    .Max( ra => ( DateTime? ) ra.CreatedDateTime.Value );

                // If we have an activity with a created date then use the most
                // recent one to check if it older than the idle date. Otherwise
                // try to use the date when the request was created.
                if ( mostRecentActivityDateTime.HasValue )
                {
                    return mostRecentActivityDateTime.Value < idleDate;
                }
                else if ( request.CreatedDateTime.HasValue )
                {
                    return request.CreatedDateTime.Value < idleDate;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the manually triggered workflows that should be used with the
        /// connection opportunity.
        /// </summary>
        /// <param name="connectionOpportunity">The connection opportunity.</param>
        /// <param name="currentPerson">The current person for security checks.</param>
        /// <returns>An enumeration of all manually triggered workflow types.</returns>
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
        /// Gets the activity view models.
        /// </summary>
        /// <returns></returns>
        private static IQueryable<ConnectionRequestActivity> GetConnectionRequestActivities( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            var connectionType = connectionRequest.ConnectionOpportunity.ConnectionType;
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
            var query = connectionRequestActivityService.Queryable()
                .AsNoTracking()
                .Include( a => a.ConnectionRequest.ConnectionOpportunity )
                .Include( a => a.ConnectorPersonAlias.Person )
                .Include( a => a.ConnectionActivityType )
                .Where( a => a.ConnectionRequest.PersonAliasId == connectionRequest.PersonAliasId );

            if ( connectionType.EnableFullActivityList )
            {
                query = query.Where( a => a.ConnectionOpportunity.ConnectionTypeId == connectionType.Id );
            }
            else
            {
                query = query.Where( a => a.ConnectionRequestId == connectionRequest.Id );
            }

            return query.OrderByDescending( a => a.CreatedDateTime );
        }

        /// <summary>
        /// Determines whether [is activity modifiable] [the specified activity].
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns><c>true</c> if [is activity modifiable] [the specified activity]; otherwise, <c>false</c>.</returns>
        private bool IsActivityModifiable( ConnectionRequestActivity activity )
        {
            if ( activity != null &&
                ( activity.CreatedByPersonAliasId.Equals( RequestContext.CurrentPerson?.PrimaryAliasId ) || activity.ConnectorPersonAliasId.Equals( RequestContext.CurrentPerson?.PrimaryAliasId ) ) &&
                activity.ConnectionActivityType.ConnectionTypeId.HasValue )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the request view model that represents the request in a way the
        /// client can properly display.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>The view model that represents the request.</returns>
        private RequestViewModel GetRequestViewModel( ConnectionRequest request, RockContext rockContext )
        {
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "ConnectionRequest", request );

            // Generate the content that will be displayed above the connection request.
            var headerContent = HeaderTemplate.ResolveMergeFields( mergeFields );

            // Get our list of available connection request activities.
            var activities = GetConnectionRequestActivities( request, rockContext );

            var deviceData = RequestContext.GetHeader( "X-Rock-DeviceData" )
                .FirstOrDefault()
                ?.FromJsonOrNull<DeviceData>();

            // We udpdated this in shell V4 to use a hard-coded view instead of the ActivityTemplate,
            // so we only want to parse the Lava if the Shell Version > 1.4.24, when we updated
            // that.
            string activityContent = "";
            if ( new Version( deviceData?.ShellVersion ?? "0") <= new Version( 1, 4, 0, 24 ) )
            {
                // Generate the content that will be used to display the activities.
                mergeFields.Add( "Activities", activities );
                activityContent = ActivityTemplate.ResolveMergeFields( mergeFields );
            }

            // Get all the workflows that can be manually triggered by the person.
            var connectionWorkflows = GetConnectionOpportunityManualWorkflowTypes( request.ConnectionOpportunity, RequestContext.CurrentPerson )
                .Where( w => w.ManualTriggerFilterConnectionStatusId == null || w.ManualTriggerFilterConnectionStatusId == request.ConnectionStatusId )
                .Select( w => new WorkflowTypeItemViewModel
                {
                    Guid = w.Guid,
                    Name = w.WorkflowType.Name,
                    IconClass = w.WorkflowType.IconCssClass
                } )
                .ToList();

            var isEditable = request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            var activitiesViewModel = activities.ToList()
                .Select( a => new ActivityViewModel
                {
                    ActivityTypeGuid = a.ConnectionActivityType.Guid,
                    ConnectorGuid = a.ConnectorPersonAlias?.Person.Guid,
                    CreatedDateTime = ( DateTimeOffset ) a.CreatedDateTime,
                    IsModifiable = IsActivityModifiable(a),
                    Note = a.Note.StripHtml(),
                    Guid = a.Guid,
                    ActivityType = a.ConnectionActivityType.ToString(),
                    Connector = a.ConnectorPersonAlias?.Person != null ?
                        new ConnectorItemViewModel
                        {
                            FirstName = a.ConnectorPersonAlias.Person.FirstName,
                            LastName = a.ConnectorPersonAlias.Person.LastName,
                            PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( a.ConnectorPersonAlias.Person.PhotoUrl )
                        }
                        : null
                } )
                .ToList();

            var viewModel = new RequestViewModel
            {
                ActivityContent = activityContent,
                Attributes = GetPublicAttributeValues( request ),
                CampusGuid = request.Campus?.Guid,
                CampusName = request.Campus?.Name,
                Comments = request.Comments,
                ConnectorGuid = request.ConnectorPersonAlias?.Person.Guid,
                ConnectorFullName = request.ConnectorPersonAlias?.Person.FullName,
                HeaderContent = headerContent,
                IsEditable = isEditable,
                IsCritical = IsRequestCritical( request ),
                IsIdle = IsRequestIdle( request ),
                CanConnect = isEditable && connectionRequestService.CanConnect( request ),
                OpportunityName = request.ConnectionOpportunity.Name,
                PersonGuid = request.PersonAlias.Person.Guid,
                PersonFullName = request.PersonAlias.Person.FullName,
                PersonEmail = request.PersonAlias.Person.Email,
                PersonMobileNumber = request.PersonAlias.Person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.NumberFormatted,
                PersonConnectionStatusName = request.PersonAlias.Person.ConnectionStatusValue?.Value,
                PersonProfilePhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( request.PersonAlias.Person.PhotoUrl ),
                PlacementGroupGuid = request.AssignedGroup?.Guid,
                PlacementGroupName = request.AssignedGroup?.Name,
                RequestDate = request.CreatedDateTime?.ToRockDateTimeOffset(),
                State = request.ConnectionState,
                StatusGuid = request.ConnectionStatus.Guid,
                StatusName = request.ConnectionStatus.Name,
                WorkflowTypes = connectionWorkflows,
                Activities = activitiesViewModel,
                ConnectionRequestGuid = request.Guid
            };

            if ( isEditable )
            {
                viewModel.PlacementGroupRequirements = GetPlacementGroupRequirements( request, rockContext, out _ );
            }

            return viewModel;
        }

        /// <summary>
        /// Gets the request edit view model that represents the request in a way
        /// the client can use to display an edit interface.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="rockContext">The context to use when needing to load any data.</param>
        /// <returns>The edit view model that represents the request.</returns>
        private RequestEditViewModel GetRequestEditViewModel( ConnectionRequest request, RockContext rockContext )
        {
            var campusClientService = new CampusClientService( rockContext, RequestContext.CurrentPerson );

            // Get the list of connectors that are available to pick from
            // for the client to use.
            var connectors = GetAvailableConnectors( request, rockContext );

            var viewModel = new RequestEditViewModel
            {
                Attributes = GetPublicEditableAttributeValues( request ),
                CampusGuid = request.Campus?.Guid,
                Comments = request.Comments,
                ConnectorGuid = request.ConnectorPersonAlias?.Person.Guid,
                PlacementGroupGuid = request.AssignedGroup?.Guid,
                State = request.ConnectionState,
                FutureFollowUpDate = request.FollowupDate?.ToRockDateTimeOffset(),
                StatusGuid = request.ConnectionStatus.Guid,
                Connectors = connectors,
                Campuses = campusClientService.GetCampusesAsListItems(),
                PlacementGroups = GetRequestPlacementGroups( request ),
                Statuses = GetOpportunityStatusListItems( request.ConnectionOpportunity.ConnectionType )
            };

            return viewModel;
        }

        /// <summary>
        /// Gets all the attributes and values for the connection request.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <returns>A list of editable attribute values.</returns>
        private List<PublicEditableAttributeValueViewModel> GetPublicAttributeValues( ConnectionRequest request )
        {
            // Build the basic attributes.
            var attributes = request.GetPublicAttributesForView( RequestContext.CurrentPerson )
                .ToDictionary( kvp => kvp.Key, kvp => new PublicEditableAttributeValueViewModel
                {
                    AttributeGuid = kvp.Value.AttributeGuid,
                    Categories = kvp.Value.Categories,
                    ConfigurationValues = kvp.Value.ConfigurationValues,
                    Description = kvp.Value.Description,
                    FieldTypeGuid = kvp.Value.FieldTypeGuid,
                    IsRequired = kvp.Value.IsRequired,
                    Key = kvp.Value.Key,
                    Name = kvp.Value.Name,
                    Order = kvp.Value.Order,
                    Value = ""
                } );

            // Add all the values to those attributes.
            request.GetPublicAttributeValuesForView( RequestContext.CurrentPerson )
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
        /// Gets all the attributes and values for the entity in a form
        /// suitable to use for editing.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <returns>A list of editable attribute values.</returns>
        private List<PublicEditableAttributeValueViewModel> GetPublicEditableAttributeValues( IHasAttributes request )
        {
            var attributes = request.GetPublicAttributesForEdit( RequestContext.CurrentPerson )
                .ToDictionary( kvp => kvp.Key, kvp => new PublicEditableAttributeValueViewModel
                {
                    AttributeGuid = kvp.Value.AttributeGuid,
                    Categories = kvp.Value.Categories,
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
        /// Gets the possible connectors for the specified connection request.
        /// All possible connectors are returned, campus filtering is not applied.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A list of connectors that are valid for the request.</returns>
        private List<ConnectorItemViewModel> GetAvailableConnectors( ConnectionRequest request, RockContext rockContext )
        {
            var additionalConnectorAliasIds = new List<int>();

            // Add the current connector if there is one.
            if ( request.ConnectorPersonAliasId.HasValue )
            {
                additionalConnectorAliasIds.Add( request.ConnectorPersonAliasId.Value );
            }

            // Add the logged in person.
            if ( RequestContext.CurrentPerson != null )
            {
                additionalConnectorAliasIds.Add( RequestContext.CurrentPerson.PrimaryAliasId.Value );
            }

            return GetConnectionOpportunityConnectors( request.ConnectionOpportunityId, null, additionalConnectorAliasIds, rockContext );
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
                .Select( m => new ConnectorItemViewModel
                {
                    Guid = m.Person.Guid,
                    FirstName = m.Person.NickName,
                    LastName = m.Person.LastName,
                    CampusGuid = m.Group.Campus.Guid
                } )
                .ToList();

            // If they specified any additional people to load then execute
            // a query to find just those people.
            if ( additionalPersonAliasIds != null && additionalPersonAliasIds.Any() )
            {
                var additionalPeople = personAliasService.Queryable()
                    .Where( pa => additionalPersonAliasIds.Contains( pa.Id ) )
                    .Select( pa => new ConnectorItemViewModel
                    {
                        Guid = pa.Person.Guid,
                        FirstName = pa.Person.NickName,
                        LastName = pa.Person.LastName,
                        CampusGuid = null
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
        /// Gets the opportunity status list items for the given connection type.
        /// </summary>
        /// <param name="connectionType">Connection type to query.</param>
        /// <returns>A list of list items that can be displayed.</returns>
        private static List<ListItemBag> GetOpportunityStatusListItems( ConnectionType connectionType )
        {
            return connectionType.ConnectionStatuses
                .OrderBy( s => s.Order )
                .OrderByDescending( s => s.IsDefault )
                .ThenBy( s => s.Name )
                .Select( s => new ListItemBag
                {
                    Value = s.Guid.ToString(),
                    Text = s.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets a query of all groups that are possible placement groups for
        /// the connection request.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="additionalGroupId">An optional additional group to include, such as the currently assigned group for a request.</param>
        /// <param name="rockContext">The rock database context.</param>
        /// <returns>A queryable of all the Group objects that can be used with the request.</returns>
        private static IQueryable<Group> GetAvailablePlacementGroupsQuery( int connectionOpportunityId, int? additionalGroupId, RockContext rockContext )
        {
            var opportunityService = new ConnectionOpportunityService( rockContext );
            var groupService = new GroupService( rockContext );

            // First add any groups specifically configured for the opportunity
            var specificConfigQuery = opportunityService.Queryable()
                .AsNoTracking()
                .Where( o => o.Id == connectionOpportunityId )
                .SelectMany( o => o.ConnectionOpportunityGroups )
                .Select( cog => cog.Group );

            // Then get any groups that are configured with 'all groups of type'
            var allGroupsOfTypeQuery = opportunityService.Queryable()
                .AsNoTracking()
                .Where( o => o.Id == connectionOpportunityId )
                .SelectMany( o => o.ConnectionOpportunityGroupConfigs )
                .Where( gc => gc.UseAllGroupsOfType )
                .SelectMany( gc => gc.GroupType.Groups );

            var allGroupsQuery = specificConfigQuery.Union( allGroupsOfTypeQuery );

            // Add the currently assigned group.
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

        /// <summary>
        /// Gets the request placement group view models. This takes into account
        /// the existing assigned group view model
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <returns>A list of group placement view models.</returns>
        private static List<PlacementGroupItemViewModel> GetRequestPlacementGroups( ConnectionRequest request )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionOpportunityGroupConfigQuery = new ConnectionOpportunityGroupConfigService( rockContext ).Queryable()
                    .Where( c => c.ConnectionOpportunityId == request.ConnectionOpportunityId );

                // Translate the query of available placement groups
                // into data we can use to construct our view models.
                var availablePlacementGroups = GetAvailablePlacementGroupsQuery( request.ConnectionOpportunityId, request.AssignedGroupId, rockContext )
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
                                c.GroupMemberRole.Id,
                                c.GroupMemberRole.Guid,
                                c.GroupMemberRole.Name,
                                Status = c.GroupMemberStatus
                            } )
                            .ToList()
                    } )
                    .ToList();

                // Translate the data into the actual view models. The
                // configs could have duplicated Guid and Name properties
                // so we group those and then put all resulting status
                // values in the object.
                var availableGroupViewModels = availablePlacementGroups
                    .Select( g => new PlacementGroupItemViewModel
                    {
                        Guid = g.Guid,
                        Name = $"{g.Name} ({( g.CampusName.IsNotNullOrWhiteSpace() ? g.CampusName : "No Campus" )})",
                        CampusGuid = g.CampusGuid,
                        Roles = g.Configs
                            .GroupBy( c => new { c.Guid, c.Name } )
                            .Select( cgrp => new PlacementGroupRoleItemViewModel
                            {
                                Guid = cgrp.Key.Guid,
                                Name = cgrp.Key.Name,
                                Statuses = cgrp.Select( s => s.Status ).ToList()
                            } )
                            .ToList()
                    } )
                    .ToList();

                // Check if we have fully qualified assigned values.
                var hasAssignedValues = request.AssignedGroupId.HasValue
                        && request.AssignedGroupMemberRoleId.HasValue
                        && request.AssignedGroupMemberStatus.HasValue;

                // If we do then try to load the group type role they are
                // supposed to be assigned to.
                var assignedGroupMemberRole = hasAssignedValues
                    ? new GroupTypeRoleService( rockContext ).Queryable()
                        .Where( r => r.Id == request.AssignedGroupMemberRoleId.Value )
                        .Select( r => new
                        {
                            r.Guid,
                            r.Name
                        } )
                        .SingleOrDefault()
                    : null;

                // Now, if we have an assigned role, that means we have assigned
                // values. Check to make sure the existing values exist and if
                // not add then in.
                if ( assignedGroupMemberRole != null )
                {
                    var existingGroupViewModel = availableGroupViewModels.Single( g => g.Guid == request.AssignedGroup.Guid );
                    var existingRole = existingGroupViewModel.Roles.SingleOrDefault( r => r.Guid == assignedGroupMemberRole.Guid );

                    // Check if the group member role is already present.
                    if ( existingRole != null )
                    {
                        // If the currently assigned status doesn't exist as
                        // an option in the role configuration then add it to the
                        // list of available options.
                        if ( !existingRole.Statuses.Contains( request.AssignedGroupMemberStatus.Value ) )
                        {
                            existingRole.Statuses.Add( request.AssignedGroupMemberStatus.Value );
                        }
                    }
                    else
                    {
                        // Add the role as a valid option to select.
                        existingGroupViewModel.Roles.Add( new PlacementGroupRoleItemViewModel
                        {
                            Guid = assignedGroupMemberRole.Guid,
                            Name = assignedGroupMemberRole.Name,
                            Statuses = new List<GroupMemberStatus> { request.AssignedGroupMemberStatus.Value }
                        } );
                    }
                }

                return availableGroupViewModels;
            }
        }

        /// <summary>
        /// Launches a workflow for the connection request. All linkages are automatically
        /// configured.
        /// </summary>
        /// <param name="connectionRequest">The connection request that will be passed to the workflow as the Entity.</param>
        /// <param name="connectionWorkflow">The connection workflow to be launched.</param>
        /// <param name="currentPerson">The logged in person that is launching the workflow.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A view model that describes the result of the operation.</returns>
        private static ConnectionWorkflowLaunchedViewModel LaunchConnectionRequestWorkflow( ConnectionRequest connectionRequest, ConnectionWorkflow connectionWorkflow, Person currentPerson, RockContext rockContext )
        {
            if ( connectionRequest == null )
            {
                throw new ArgumentNullException( nameof( connectionRequest ) );
            }

            if ( connectionWorkflow == null )
            {
                throw new ArgumentNullException( nameof( connectionWorkflow ) );
            }

            // Validate that the workflow type is configured properly.
            var workflowService = new WorkflowService( rockContext );
            var workflowType = connectionWorkflow.WorkflowTypeCache;

            if ( workflowType == null || workflowType.IsActive == false )
            {
                return new ConnectionWorkflowLaunchedViewModel
                {
                    WorkflowTypeGuid = workflowType?.Guid,
                    Errors = new List<string> { "Workflow was not found or is not active." }
                };
            }

            // Do the initial activation of the workflow.
            var workflow = Rock.Model.Workflow.Activate( workflowType, connectionWorkflow.WorkflowType.WorkTerm, rockContext );

            // Attempt to process the workflow, check if an error has prevented
            // the workflow from processing correctly.
            if ( !workflowService.Process( workflow, connectionRequest, out var workflowErrors ) )
            {
                return new ConnectionWorkflowLaunchedViewModel
                {
                    WorkflowTypeGuid = workflowType.Guid,
                    WorkflowGuid = workflow.Guid,
                    Errors = workflowErrors
                };
            }

            // The workflow is persisted, so we need to create the link between
            // the workflow and this connection request.
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

            // Check if there is an entry form waiting for this person to enter
            // data into.
            var hasEntryForm = workflow.HasActiveEntryForm( currentPerson );

            return new ConnectionWorkflowLaunchedViewModel
            {
                WorkflowTypeGuid = workflowType.Guid,
                WorkflowGuid = workflow.Guid,
                HasActiveEntryForm = hasEntryForm,
                Message = $"A '{workflowType.Name}' workflow was started."
            };
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

        /// <summary>
        /// Gets the placement group member requirements for the connection request.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <param name="requirementErrors">On return will contain any requirement errors that were encountered.</param>
        /// <returns>A list of requirements that the person must meet before being connected.</returns>
        private static List<GroupMemberRequirementViewModel> GetPlacementGroupRequirements( ConnectionRequest request, RockContext rockContext, out List<string> requirementErrors )
        {
            // Get the requirements
            var requirementsResults = GetGroupRequirementStatuses( request, rockContext );
            var requirementViewModels = new List<GroupMemberRequirementViewModel>();

            foreach ( var requirementResult in requirementsResults )
            {
                if ( requirementResult.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                {
                    // A manual requirement that must be acknowledged before the
                    // person can be placed in the group.
                    requirementViewModels.Add( new GroupMemberRequirementViewModel
                    {
                        Guid = requirementResult.GroupRequirement.Guid,
                        Label = requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel.IfEmpty( requirementResult.GroupRequirement.GroupRequirementType.Name ),
                        IsManual = true,
                        MustMeetRequirementToAddMember = requirementResult.GroupRequirement.MustMeetRequirementToAddMember,
                        MeetsGroupRequirement = requirementResult.MeetsGroupRequirement
                    } );
                }
                else
                {
                    string labelText;
                    DateTimeOffset? lastCheckedDateTime = null;

                    // Determine the label text to use for this requirement.
                    if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.PositiveLabel;
                    }
                    else if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.WarningLabel;
                    }
                    else
                    {
                        if ( requirementResult.GroupRequirement.MustMeetRequirementToAddMember )
                        {
                            labelText = requirementResult.GroupRequirement.GroupRequirementType.NegativeLabel;
                        }
                        else
                        {
                            labelText = string.Empty;
                        }
                    }

                    if ( string.IsNullOrEmpty( labelText ) )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.Name;
                    }

                    // Determine the last checked date time for this requirement.
                    if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                    {
                        lastCheckedDateTime = requirementResult.RequirementWarningDateTime;
                    }
                    else
                    {
                        lastCheckedDateTime = requirementResult.LastRequirementCheckDateTime;
                    }

                    requirementViewModels.Add( new GroupMemberRequirementViewModel
                    {
                        Guid = requirementResult.GroupRequirement.Guid,
                        Label = labelText,
                        MustMeetRequirementToAddMember = requirementResult.GroupRequirement.MustMeetRequirementToAddMember,
                        MeetsGroupRequirement = requirementResult.MeetsGroupRequirement,
                        LastCheckedDateTime = lastCheckedDateTime
                    } );
                }
            }

            // Set any errors we may have run into.
            requirementErrors = requirementsResults
                .Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.Error )
                .Select( a => a.ToString() )
                .ToList();

            return requirementViewModels;
        }

        /// <summary>
        /// Gets the group requirement statuses for the connection request.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns></returns>
        private static List<PersonGroupRequirementStatus> GetGroupRequirementStatuses( ConnectionRequest request, RockContext rockContext )
        {
            if ( request == null || !request.AssignedGroupId.HasValue || request.PersonAlias == null )
            {
                return new List<PersonGroupRequirementStatus>();
            }

            var groupId = request.AssignedGroupId.Value;
            var group = new GroupService( rockContext )
                .GetNoTracking( request.AssignedGroupId.Value );

            if ( group == null )
            {
                return new List<PersonGroupRequirementStatus>();
            }

            var requirementsResults = group.PersonMeetsGroupRequirements( rockContext, request.PersonAlias.PersonId, request.AssignedGroupMemberRoleId );

            if ( requirementsResults != null )
            {
                // Ignore the NotApplicable requirements.
                requirementsResults = requirementsResults.Where( r => r.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable );
            }

            return requirementsResults.ToList();
        }

        /// <summary>
        /// Attempts to mark the connection request as connected.
        /// </summary>
        /// <param name="connectionRequest">The connection request to be updated.</param>
        /// <param name="manualRequirementsMet">A dictionary of manual requirements that have been met.</param>
        /// <param name="currentPerson">The person that is performing the operation.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <param name="errorMessage">On return, contains any error message.</param>
        /// <returns><c>true</c> if the request has been marked as connected; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// This method will call SaveChanges() on <paramref name="rockContext"/> before returning <c>true</c>.
        /// </remarks>
        private static bool TryMarkRequestConnected( ConnectionRequest connectionRequest, Dictionary<Guid, bool> manualRequirementsMet, Person currentPerson, RockContext rockContext, out string errorMessage )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

            if ( connectionRequest == null || connectionRequest.PersonAlias == null || connectionRequest.ConnectionOpportunity == null )
            {
                errorMessage = "Connection request is not in a valid state.";
                return false;
            }

            // If it is already connected then nothing to do.
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

            // Only do group member placement if the request has an assigned
            // placement group, role, and status.
            if ( hasGroupAssignment )
            {
                var group = connectionRequest.AssignedGroup;

                // Only attempt the add if person does not already exist in group with same role
                groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                    connectionRequest.AssignedGroupId.Value,
                    connectionRequest.PersonAlias.PersonId,
                    connectionRequest.AssignedGroupMemberRoleId.Value );

                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = connectionRequest.PersonAlias.PersonId;
                    groupMember.GroupId = connectionRequest.AssignedGroupId.Value;
                    groupMember.GroupRoleId = connectionRequest.AssignedGroupMemberRoleId.Value;
                    groupMember.GroupMemberStatus = connectionRequest.AssignedGroupMemberStatus.Value;

                    // Load all the manual group requirements for this group.
                    var manualGroupRequirements = group.GetGroupRequirements( rockContext )
                        .Where( r => r.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                        .ToList();

                    // Walk through each manual requirement to ensure they
                    // have been explicitly marked as meeting the requirement.
                    foreach ( var requirement in manualGroupRequirements )
                    {
                        var meetsRequirement = manualRequirementsMet.GetValueOrDefault( requirement.Guid, false );

                        if ( !meetsRequirement && requirement.MustMeetRequirementToAddMember )
                        {
                            errorMessage = "Group Requirements have not been met. Please verify all of the requirements.";
                            return false;
                        }

                        // If they meet the requirement, create the record
                        // that notes it has been met.
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

                    // All requirements have been met, add the group member.
                    groupMemberService.Add( groupMember );

                    // If there are any assigned group member attribute values
                    // that should be filled in then do so.
                    if ( !string.IsNullOrWhiteSpace( connectionRequest.AssignedGroupMemberAttributeValues ) )
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

            // Always record the connection activity and change the state to connected.
            var connectedGuid = Rock.SystemGuid.ConnectionActivityType.CONNECTED.AsGuid();
            var connectedActivityId = connectionActivityTypeService.Queryable()
                .Where( t => t.Guid == connectedGuid )
                .Select( t => t.Id )
                .FirstOrDefault();

            if ( connectedActivityId > 0 )
            {
                var connectionRequestActivity = new ConnectionRequestActivity();
                connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                connectionRequestActivity.ConnectionActivityTypeId = connectedActivityId;
                connectionRequestActivity.ConnectorPersonAliasId = currentPerson.PrimaryAliasId;
                connectionRequestActivityService.Add( connectionRequestActivity );
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

        /// <summary>
        /// This gets a specific set of information about a connection activity that is ultimately passed down to the mobile shell.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionRequestIdKey">The connection request unique identifier.</param>
        /// <param name="activityGuid">The activity unique identifier.</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        /// <returns>System.ValueTuple&lt;ConnectionRequestActivity, List&lt;ConnectorItemViewModel&gt;, List&lt;Common.Mobile.ViewModel.ListItemViewModel&gt;, BlockActionResult&gt;.</returns>
        private (ConnectionRequestActivity Activity, List<ConnectorItemViewModel> Connectors, List<Common.Mobile.ViewModel.ListItemViewModel> ActivityTypes, BlockActionResult Error) GetConnectionRequestActivityBag( RockContext rockContext, string connectionRequestIdKey, Guid? activityGuid, bool readOnly )
        {
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );

            // Load the connection request and include the opportunity and type
            // to speed up the security check.
            var request = connectionRequestService.GetQueryableByKey( connectionRequestIdKey )
                .Include( r => r.ConnectionOpportunity.ConnectionType )
                .AsNoTracking()
                .FirstOrDefault();

            if ( request == null )
            {
                return (null, null, null, ActionNotFound());
            }
            else if ( !readOnly && !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                // Require edit access in order to see the available activity types
                // since they are only required when editing.
                return (null, null, null, ActionUnauthorized());
            }
            else if( readOnly && !request.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return (null, null, null, ActionUnauthorized());
            }
            
            // Load up the activity types for this connection request and pull
            // in the Guid an Name to send to the client.
            var activityTypes = connectionActivityTypeService.Queryable()
                .Where( a => a.ConnectionTypeId == request.ConnectionOpportunity.ConnectionTypeId )
                .Select( a => new Common.Mobile.ViewModel.ListItemViewModel
                {
                    Value = a.Guid.ToString(),
                    Text = a.Name
                } )
                .ToList();

            // Get the list of connectors that are available to pick from
            // for the client to use.
            var connectors = GetAvailableConnectors( request, rockContext );

            ConnectionRequestActivity activity = null;
            if ( activityGuid != null )
            {
                activity = new ConnectionRequestActivityService( rockContext ).Get( activityGuid.Value );
            }

            return (activity, connectors, activityTypes, null);
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the request details that should be displayed to the user.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request as either an IdKey, Guid or depending on site configuration integer Id.</param>
        /// <returns>A model that contains the connection request details.</returns>
        [BlockAction]
        public BlockActionResult GetRequestDetails( string connectionRequestGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                // Load the connection request and include the opportunity and type
                // to speed up the security check.
                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .Include( r => r.Campus )
                    .Include( r => r.ConnectorPersonAlias.Person )
                    .Include( r => r.ConnectionStatus )
                    .AsNoTracking()
                    .FirstOrDefault();

                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                request.LoadAttributes( rockContext );

                return ActionOk( GetRequestViewModel( request, rockContext ) );
            }
        }

        /// <summary>
        /// Gets the details that describe an edit operation for the connection request.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <returns>The edit view model for the connection request.</returns>
        [BlockAction]
        public BlockActionResult GetRequestEditDetails( string connectionRequestGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                // Load the connection request and include the opportunity and type
                // to speed up the security check.
                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .Include( r => r.Campus )
                    .Include( r => r.ConnectorPersonAlias.Person )
                    .Include( r => r.ConnectionStatus )
                    .AsNoTracking()
                    .FirstOrDefault();

                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                request.LoadAttributes( rockContext );

                return ActionOk( GetRequestEditViewModel( request, rockContext ) );
            }
        }

        /// <summary>
        /// Updates the connection request to match the data provided.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier to be updated.</param>
        /// <param name="requestDetails">The details that will be updated on the connection request.</param>
        /// <returns>A model that contains the updated connection request details to be displayed.</returns>
        [BlockAction]
        public BlockActionResult UpdateRequest( string connectionRequestGuid, RequestSaveViewModel requestDetails )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionStatusService = new ConnectionStatusService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                // Load the connection request and include the opportunity and type
                // to speed up the security check.
                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .FirstOrDefault();

                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                request.LoadAttributes( rockContext );

                var originalConnectorPersonAliasId = request.ConnectorPersonAliasId;

                // Set the basic values that don't require lookups.
                request.Comments = requestDetails.Comments;
                request.ConnectionState = requestDetails.State;

                // Set the future follow up date.
                if ( request.ConnectionState == ConnectionState.FutureFollowUp )
                {
                    if ( !requestDetails.FutureFollowUpDate.HasValue )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    // Drop the timezone since we are just setting a "raw" date.
                    request.FollowupDate = requestDetails.FutureFollowUpDate.Value.DateTime;
                }
                else
                {
                    request.FollowupDate = null;
                }

                // Perform a lookup and validate the campus then set it.
                CampusCache campus;
                if ( requestDetails.CampusGuid.HasValue )
                {
                    campus = CampusCache.Get( requestDetails.CampusGuid.Value );

                    if ( campus == null )
                    {
                        return ActionBadRequest( "Invalid Data." );
                    }

                    request.CampusId = campus.Id;
                }
                else
                {
                    request.CampusId = null;
                    campus = null;
                }

                // Perform a lookup and validate the connector then set it.
                if ( requestDetails.ConnectorGuid.HasValue )
                {
                    var isValidConnector = GetAvailableConnectors( request, rockContext )
                        .Where( c => campus == null || !c.CampusGuid.HasValue || campus.Guid == c.CampusGuid )
                        .Any( c => c.Guid == requestDetails.ConnectorGuid );

                    var connectorPersonAliasId = personAliasService.GetPrimaryAliasId( requestDetails.ConnectorGuid.Value );

                    if ( !connectorPersonAliasId.HasValue || !isValidConnector )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    request.ConnectorPersonAliasId = connectorPersonAliasId;
                }
                else
                {
                    request.ConnectorPersonAliasId = null;
                }

                // Set the status which requires a lookup.
                var status = connectionStatusService.Queryable()
                    .Where( s => s.ConnectionTypeId == request.ConnectionOpportunity.ConnectionTypeId
                        && s.Guid == requestDetails.StatusGuid )
                    .SingleOrDefault();

                if ( status == null )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                request.ConnectionStatusId = status.Id;

                // Perform a lookup and validate the placement group then set it.
                if ( requestDetails.PlacementGroupGuid.HasValue || requestDetails.PlacementGroupMemberRoleGuid.HasValue || requestDetails.PlacementGroupMemberStatus.HasValue )
                {
                    // Check if they gave us any one of the three placement values
                    // but not all three. That is an error.
                    if ( !requestDetails.PlacementGroupGuid.HasValue || !requestDetails.PlacementGroupMemberRoleGuid.HasValue || !requestDetails.PlacementGroupMemberStatus.HasValue )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    // Validate that the information they provided is valid
                    // group placement options.
                    var validPlacementGroups = GetRequestPlacementGroups( request );
                    var placementGroup = validPlacementGroups.SingleOrDefault( g => g.Guid == requestDetails.PlacementGroupGuid );
                    var placementRole = placementGroup?.Roles.SingleOrDefault( r => r.Guid == requestDetails.PlacementGroupMemberRoleGuid );

                    if ( placementGroup == null || placementRole == null || !placementRole.Statuses.Contains( requestDetails.PlacementGroupMemberStatus.Value ) )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    var groupId = new GroupService( rockContext ).GetId( placementGroup.Guid );
                    var roleId = new GroupTypeRoleService( rockContext ).GetId( placementRole.Guid );

                    request.AssignedGroupId = groupId;
                    request.AssignedGroupMemberRoleId = roleId;
                    request.AssignedGroupMemberStatus = requestDetails.PlacementGroupMemberStatus.Value;

                    var memberAttributeValues = new Dictionary<string, string>();
                    if ( requestDetails.PlacementGroupMemberAttributeValues != null )
                    {
                        // Load the attribute data for an empty group member so we can
                        // decode the data from the client.
                        var groupMember = new GroupMember
                        {
                            GroupId = request.AssignedGroupId.Value,
                            GroupRoleId = request.AssignedGroupMemberRoleId.Value
                        };

                        groupMember.LoadAttributes( rockContext );

                        foreach ( var memberValue in requestDetails.PlacementGroupMemberAttributeValues )
                        {
                            if ( !groupMember.Attributes.TryGetValue( memberValue.Key, out var attribute ) )
                            {
                                return ActionBadRequest( "Invalid data." );
                            }

                            if ( !attribute.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                            {
                                return ActionBadRequest( "Invalid data." );
                            }

                            var value = PublicAttributeHelper.GetPrivateValue( attribute, memberValue.Value );

                            memberAttributeValues.Add( memberValue.Key, value );
                        }

                        request.AssignedGroupMemberAttributeValues = memberAttributeValues.ToJson();
                    }
                    else
                    {
                        request.AssignedGroupMemberAttributeValues = null;
                    }
                }
                else
                {
                    request.AssignedGroupId = null;
                    request.AssignedGroupMemberRoleId = null;
                    request.AssignedGroupMemberStatus = null;
                    request.AssignedGroupMemberAttributeValues = null;
                }

                // Set any custom request attribute values.
                if ( requestDetails.AttributeValues != null )
                {
                    request.SetPublicAttributeValues( requestDetails.AttributeValues, RequestContext.CurrentPerson );
                }

                // Add an activity that the connector was assigned or changed.
                if ( originalConnectorPersonAliasId != request.ConnectorPersonAliasId )
                {
                    var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                    var activity = CreateAssignedActivity( request, rockContext );

                    if ( activity != null )
                    {
                        connectionRequestActivityService.Add( activity );
                    }
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    request.SaveAttributeValues( rockContext );
                } );
            }

            // Even though calling this method will load a whole new entity
            // that is what we want anyway. If we changed any values then
            // all the in-memory navigation properties are probably incorrect.
            return GetRequestDetails( connectionRequestGuid );
        }

        /// <summary>
        /// Gets the activity options available for the connection request.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <returns>A description of options available when adding a new activity.</returns>
        [BlockAction]
        public BlockActionResult GetActivityOptions( string connectionRequestGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var requestActivityBag = GetConnectionRequestActivityBag( rockContext, connectionRequestGuid, null, false );

                if ( requestActivityBag.Error != null )
                {
                    return requestActivityBag.Error;
                }

                return ActionOk( new Common.Mobile.Blocks.Connection.ConnectionRequestDetail.ActivityOptionsViewModel
                {
                    ActivityTypes = requestActivityBag.ActivityTypes,
                    Connectors = requestActivityBag.Connectors
                } );
            }
        }

        /// <summary>
        /// Gets the activity options available for the connection request with an already existing activity.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="activityGuid">The activity unique identifier.</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetExistingActivityOptions( string connectionRequestGuid, Guid activityGuid, bool readOnly )
        {
            using ( var rockContext = new RockContext() )
            {
                var requestActivityBag = GetConnectionRequestActivityBag( rockContext, connectionRequestGuid, activityGuid, readOnly );

                if( requestActivityBag.Error != null )
                {
                    return requestActivityBag.Error;
                }

                Guid? connectorGuid = null;
                string activityNote = "";
                Guid? activityTypeGuid = null;
                string activityType = "";

                if ( activityGuid != null )
                {
                    var activity = new ConnectionRequestActivityService( rockContext ).Get( activityGuid );

                    if ( activity != null )
                    {
                        if ( activity.ConnectorPersonAliasId.HasValue )
                        {
                            connectorGuid = new PersonAliasService( rockContext ).Get( activity.ConnectorPersonAliasId.Value ).Guid;
                        }

                        activityType = activity.ConnectionActivityType.ToString();
                        activityNote = activity.Note;
                        activityTypeGuid = activity.ConnectionActivityType.Guid;
                    }
                }

                return ActionOk( new Common.Mobile.Blocks.Connection.ConnectionRequestDetail.ConnectionRequestActivityDetailBag
                {
                    ActivityOptions = new ActivityOptionsViewModel
                    {
                        ActivityTypes = requestActivityBag.ActivityTypes,
                        Connectors = requestActivityBag.Connectors
                    },
                    ActivityCurrentConnectorGuid = connectorGuid,
                    Note = activityNote,
                    ActivityTypeGuid = activityTypeGuid,
                    ActivityType = activityType
                } );
            }
        }

        /// <summary>
        /// Adds a new activity to the connection request.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="activity">The activity details.</param>
        /// <returns>The view model data that should be displayed.</returns>
        [BlockAction]
        public BlockActionResult AddActivity( string connectionRequestGuid, ActivityViewModel activity )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var noteService = new NoteService( rockContext );
                int? connectorAliasId = null;

                // Load the connection request. Include the connection opportunity
                // and type for security check.
                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .FirstOrDefault();

                // Validate the request exists and the current person has permission
                // to make changes to it.
                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                // Load the activity identifier from the database, making sure
                // the Guid they gave us is actually for the correct connection type.
                var activityTypeId = connectionActivityTypeService.Queryable()
                    .Where( a => a.Guid == activity.ActivityTypeGuid
                        && a.ConnectionTypeId == request.ConnectionOpportunity.ConnectionTypeId )
                    .Select( a => a.Id )
                    .FirstOrDefault();

                if ( activityTypeId == 0 )
                {
                    return ActionBadRequest( "Invalid activity type specified." );
                }

                // Load the connector primary alias from the database and verify
                // that it is valid option.
                if ( activity.ConnectorGuid.HasValue )
                {
                    var connectors = GetAvailableConnectors( request, rockContext );

                    if ( !connectors.Any( c => c.Guid == activity.ConnectorGuid.Value ) )
                    {
                        return ActionBadRequest( "Invalid connector was specified." );
                    }

                    connectorAliasId = personAliasService.GetPrimaryAliasId( activity.ConnectorGuid.Value );

                    if ( !connectorAliasId.HasValue )
                    {
                        return ActionBadRequest( "Invalid connector was specified." );
                    }
                }

                // Load attributes since we need them to generate the view model
                // later.
                request.LoadAttributes( rockContext );

                // Create the new activity via the Create() method so that lazy
                // loading will work after it has been saved. This gets used by
                // the lava template.
                var requestActivity = rockContext.ConnectionRequestActivities.Create();
                requestActivity.ConnectionOpportunityId = request.ConnectionOpportunityId;
                requestActivity.ConnectionActivityTypeId = activityTypeId;
                requestActivity.ConnectorPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                var mentionedPersonIds = noteService.GetNewPersonIdsMentionedInContent( activity.Note, requestActivity.Note );
                requestActivity.Note = activity.Note;
                requestActivity.ConnectorPersonAliasId = connectorAliasId;

                request.ConnectionRequestActivities.Add( requestActivity );
                connectionRequestActivityService.Add( requestActivity );

                rockContext.SaveChanges();

                // If we have any new mentioned person ids, start a background
                // task to create the notifications.
                if ( mentionedPersonIds.Any() )
                {
                    Task.Run( () =>
                    {
                        foreach ( var personId in mentionedPersonIds )
                        {
                            ConnectionRequestMention.CreateNotificationMessage( request, personId, RequestContext.CurrentPerson.Id, PageCache.Id, RequestContext.GetPageParameters() );
                        }
                    } );
                }

                return ActionOk( GetRequestViewModel( request, rockContext ) );
            }
        }

        /// <summary>
        /// Updates a connection request activity.
        /// </summary>
        /// <param name="activityGuid">The activity unique identifier.</param>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="activity">The activity.</param>
        /// <returns>The view model data that should be displayed.</returns>
        [BlockAction]
        public BlockActionResult UpdateActivity( Guid activityGuid, string connectionRequestGuid, ActivityViewModel activity )
        {
            using ( var rockContext = new RockContext() )
            {
                var activityService = new ConnectionRequestActivityService( rockContext );
                var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var noteService = new NoteService( rockContext );

                var activityToUpdate = activityService.Get( activityGuid );

                if ( activityToUpdate == null )
                {
                    return ActionNotFound();
                }

                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                // Load the connection request. Include the connection opportunity
                // and type for security check.
                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .FirstOrDefault();

                // Validate the request exists and the current person has permission
                // to make changes to it.
                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                var connectionActivityType = connectionActivityTypeService.Get( activity.ActivityTypeGuid );
                if ( connectionActivityType == null )
                {
                    return ActionBadRequest( "Unable to find that connection activity type." );
                }

                if( !activity.ConnectorGuid.HasValue )
                {
                    return ActionBadRequest( "Invalid connector was specified." );

                }
                var connectorAliasId = personAliasService.GetPrimaryAliasId( activity.ConnectorGuid.Value );

                if ( !connectorAliasId.HasValue )
                {
                    return ActionBadRequest( "Invalid connector was specified." );
                }

                activityToUpdate.ConnectionActivityTypeId = connectionActivityType.Id;
                activityToUpdate.ConnectorPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                var mentionedPersonIds = noteService.GetNewPersonIdsMentionedInContent( activity.Note, activityToUpdate.Note );
                activityToUpdate.Note = activity.Note;
                activityToUpdate.ConnectorPersonAliasId = connectorAliasId;

                rockContext.SaveChanges();

                // If we have any new mentioned person ids, start a background
                // task to create the notifications.
                if ( mentionedPersonIds.Any() )
                {
                    Task.Run( () =>
                    {
                        foreach ( var personId in mentionedPersonIds )
                        {
                            ConnectionRequestMention.CreateNotificationMessage( request, personId, RequestContext.CurrentPerson.Id, PageCache.Id, RequestContext.GetPageParameters() );
                        }
                    } );
                }

                return ActionOk( GetRequestViewModel( request, rockContext ) );
            }
        }

        /// <summary>
        /// Deletes a connection request activity.
        /// </summary>
        /// <param name="activityGuid">The activity unique identifier.</param>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <returns>The view model data that should be displayed.</returns>
        [BlockAction]
        public BlockActionResult DeleteActivity( Guid activityGuid, string connectionRequestGuid, int currentPersonAliasId )
        {
            using ( var rockContext = new RockContext() )
            {
                // only allow deleting if current user created the activity, and not a system activity
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );

                var activity = connectionRequestActivityService.Get( activityGuid );

                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;
                var request = connectionRequestService.Get( connectionRequestGuid, !disablePredictableIds );

                if( activity == null )
                {
                    return ActionNotFound( "Unable to find that specific activity." );
                }

                if( request == null )
                {
                    return ActionNotFound( "Unable to find that specific connection request." );
                }

                // Make sure we have permission to delete this activity.
                if ( ( activity.CreatedByPersonAliasId.Equals( currentPersonAliasId )
                    || activity.ConnectorPersonAliasId.Equals( currentPersonAliasId ) )
                    && activity.ConnectionActivityType.ConnectionTypeId.HasValue )
                {
                    connectionRequestActivityService.Delete( activity );
                    rockContext.SaveChanges();
                }
                // If we don't have permission.
                else
                {
                    return ActionForbidden( "You don't have permission to delete that activity." );
                }

                return ActionOk( GetRequestViewModel( request, rockContext ) );
            }
        }

        /// <summary>
        /// Launches a new workflow for the connection request.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="connectionWorkflowGuid">The connection workflow unique identifier, the value from <see cref="WorkflowTypeItemViewModel.Guid"/>.</param>
        /// <returns>A response that determines if the workflow launched and if the user needs to be directed to the workflow entry page.</returns>
        [BlockAction]
        public BlockActionResult LaunchWorkflow( string connectionRequestGuid, Guid connectionWorkflowGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionWorkflowService = new ConnectionWorkflowService( rockContext );

                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;
                var connectionRequest = connectionRequestService.Get( connectionRequestGuid, !disablePredictableIds );
                var connectionWorkflow = connectionWorkflowService.Get( connectionWorkflowGuid );

                // Make sure we found the connection request and then check if they
                // even have permission to see this connection request.
                if ( connectionRequest == null || connectionWorkflow == null )
                {
                    return ActionNotFound();
                }

                if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                // Verify that the workflow they specified matches one that they
                // are allowed to launch.
                var workflows = GetConnectionOpportunityManualWorkflowTypes( connectionRequest.ConnectionOpportunity, RequestContext.CurrentPerson );

                if ( !workflows.Any( w => w.Guid == connectionWorkflow.Guid ) )
                {
                    return ActionNotFound();
                }

                var result = LaunchConnectionRequestWorkflow( connectionRequest, connectionWorkflow, RequestContext.CurrentPerson, rockContext );

                // Even if result returned an error, send back an OK response
                // since the workflow at least partially ran. The client can
                // decide how to proceed.
                return ActionOk( result );
            }
        }

        /// <summary>
        /// Get the placement group member attributes that should be set when
        /// the client changes either the placement group or the member role.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="groupGuid">The unique identifier of the group the person will be placed into.</param>
        /// <param name="groupMemberRoleGuid">The unique identifier of the role the person will be assigned.</param>
        /// <returns>The attributes that can be filled in by the individual.</returns>
        [BlockAction]
        [RockObsolete( "1.13.3" )]
        public BlockActionResult GetPlacementGroupMemberAttributes( string connectionRequestGuid, Guid groupGuid, Guid groupMemberRoleGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var groupService = new GroupService( rockContext );

                // Load the connection request. Include the connection opportunity
                // and type for security check.
                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .FirstOrDefault();

                // Validate the request exists and the current person has permission
                // to make changes to it.
                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                // Validate that the information they provided is valid
                // group placement options.
                var validPlacementGroups = GetRequestPlacementGroups( request );
                var placementGroup = validPlacementGroups.SingleOrDefault( g => g.Guid == groupGuid );
                var placementRole = placementGroup?.Roles.SingleOrDefault( r => r.Guid == groupMemberRoleGuid );

                if ( placementGroup == null || placementRole == null )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Try to load the group identifier along with the group type
                // identifier so we can load the role from cache.
                var groupInfo = groupService.Queryable()
                    .Where( g => g.Guid == groupGuid )
                    .Select( g => new
                    {
                        g.Id,
                        g.GroupTypeId
                    } )
                    .FirstOrDefault();

                if ( groupInfo == null )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Try to load the group member role identifier. This also ensures
                // the unique identifier they provided belongs to the correct
                // group type.
                var groupTypeCache = GroupTypeCache.Get( groupInfo.GroupTypeId );
                var groupMemberRoleId = groupTypeCache?.Roles
                    .FirstOrDefault( r => r.Guid == groupMemberRoleGuid )
                    ?.Id;

                if ( !groupMemberRoleId.HasValue )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Load the attribute data for an empty group member so we can
                // send the data to the client.
                var groupMember = new GroupMember
                {
                    GroupId = groupInfo.Id,
                    GroupRoleId = groupMemberRoleId.Value
                };

                groupMember.LoadAttributes( rockContext );

                // Restore the saved group member attribute values if we have any.
                var savedMemberAttributeValues = request.AssignedGroupMemberAttributeValues?.FromJsonOrNull<Dictionary<string, string>>();
                if ( savedMemberAttributeValues != null )
                {
                    foreach ( var item in savedMemberAttributeValues )
                    {
                        groupMember.SetAttributeValue( item.Key, item.Value );
                    }
                }

                var attributes = GetPublicEditableAttributeValues( groupMember );

                return ActionOk( attributes );
            }
        }

        /// <summary>
        /// Get the placement group member attributes that should be set when
        /// the client changes either the placement group or the member role.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request unique identifier.</param>
        /// <param name="groupGuid">The unique identifier of the group the person will be placed into.</param>
        /// <param name="groupMemberRoleGuid">The unique identifier of the role the person will be assigned.</param>
        /// <returns>The attributes that can be filled in by the individual.</returns>
        [BlockAction]
        public BlockActionResult GetPlacementGroupMemberAttributesAndValues( string connectionRequestGuid, Guid groupGuid, Guid groupMemberRoleGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var groupService = new GroupService( rockContext );

                // Load the connection request. Include the connection opportunity
                // and type for security check.
                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .FirstOrDefault();

                // Validate the request exists and the current person has permission
                // to make changes to it.
                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                // Validate that the information they provided is valid
                // group placement options.
                var validPlacementGroups = GetRequestPlacementGroups( request );
                var placementGroup = validPlacementGroups.SingleOrDefault( g => g.Guid == groupGuid );
                var placementRole = placementGroup?.Roles.SingleOrDefault( r => r.Guid == groupMemberRoleGuid );

                if ( placementGroup == null || placementRole == null )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Try to load the group identifier along with the group type
                // identifier so we can load the role from cache.
                var groupInfo = groupService.Queryable()
                    .Where( g => g.Guid == groupGuid )
                    .Select( g => new
                    {
                        g.Id,
                        g.GroupTypeId
                    } )
                    .FirstOrDefault();

                if ( groupInfo == null )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Try to load the group member role identifier. This also ensures
                // the unique identifier they provided belongs to the correct
                // group type.
                var groupTypeCache = GroupTypeCache.Get( groupInfo.GroupTypeId );
                var groupMemberRoleId = groupTypeCache?.Roles
                    .FirstOrDefault( r => r.Guid == groupMemberRoleGuid )
                    ?.Id;

                if ( !groupMemberRoleId.HasValue )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Load the attribute data for an empty group member so we can
                // send the data to the client.
                var groupMember = new GroupMember
                {
                    GroupId = groupInfo.Id,
                    GroupRoleId = groupMemberRoleId.Value
                };

                groupMember.LoadAttributes( rockContext );

                // Restore the saved group member attribute values if we have any.
                var savedMemberAttributeValues = request.AssignedGroupMemberAttributeValues?.FromJsonOrNull<Dictionary<string, string>>();
                if ( savedMemberAttributeValues != null )
                {
                    foreach ( var item in savedMemberAttributeValues )
                    {
                        groupMember.SetAttributeValue( item.Key, item.Value );
                    }
                }

                var attributes = GetPublicEditableAttributeValues( groupMember );

                return ActionOk( new AttributesAndValuesViewModel
                {
                    Attributes = groupMember.GetPublicAttributesForEdit( RequestContext.CurrentPerson ),
                    Values = groupMember.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson )
                } );
            }
        }

        /// <summary>
        /// Attempts to mark the connection request as connected.
        /// </summary>
        /// <param name="connectionRequestGuid">The connection request to be marked as connected.</param>
        /// <param name="manualRequirements"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult MarkRequestConnected( string connectionRequestGuid, Dictionary<Guid, bool> manualRequirements )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionStatusService = new ConnectionStatusService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                // Load the connection request and include the opportunity and type
                // to speed up the security check.
                var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

                var request = connectionRequestService.GetQueryableByKey( connectionRequestGuid, !disablePredictableIds )
                    .Include( r => r.ConnectionOpportunity.ConnectionType )
                    .FirstOrDefault();

                if ( request == null )
                {
                    return ActionNotFound();
                }
                else if ( !request.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized();
                }

                if ( !TryMarkRequestConnected( request, manualRequirements, RequestContext.CurrentPerson, rockContext, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }
            }

            // Even though calling this method will load a whole new entity
            // that is what we want anyway. If we changed any values then
            // all the in-memory navigation properties are probably incorrect.
            return GetRequestDetails( connectionRequestGuid );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Common details about a connection request.
        /// </summary>
        public abstract class RequestViewModelBase
        {
            /// <summary>
            /// Gets or sets the connector unique identifier.
            /// </summary>
            /// <value>
            /// The connector unique identifier.
            /// </value>
            public Guid? ConnectorGuid { get; set; }

            /// <summary>
            /// Gets or sets the comments.
            /// </summary>
            /// <value>
            /// The comments.
            /// </value>
            public string Comments { get; set; }

            /// <summary>
            /// Gets or sets the campus unique identifier.
            /// </summary>
            /// <value>
            /// The campus unique identifier.
            /// </value>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets the placement group unique identifier.
            /// </summary>
            /// <value>
            /// The placement group unique identifier.
            /// </value>
            public Guid? PlacementGroupGuid { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public ConnectionState State { get; set; }

            /// <summary>
            /// Gets or sets the status unique identifier.
            /// </summary>
            /// <value>
            /// The status unique identifier.
            /// </value>
            public Guid StatusGuid { get; set; }
        }

        /// <summary>
        /// Contains all the detail information required to display a connection request.
        /// </summary>
        /// <seealso cref="RequestViewModelBase" />
        public class RequestViewModel : RequestViewModelBase
        {
            /// <summary>
            /// Gets or sets the content of the header.
            /// </summary>
            /// <value>
            /// The content of the header.
            /// </value>
            public string HeaderContent { get; set; }

            /// <summary>
            /// Gets or sets the person unique identifier this connection request is for.
            /// </summary>
            /// <value>
            /// The person unique identifier this connection request is for.
            /// </value>
            public Guid PersonGuid { get; set; }

            /// <summary>
            /// Gets or sets the full name of the person this connection request is for.
            /// </summary>
            /// <value>
            /// The full name of the person this connection request is for.
            /// </value>
            public string PersonFullName { get; set; }

            /// <summary>
            /// Gets or sets the person profile photo URL.
            /// </summary>
            /// <value>
            /// The person profile photo URL.
            /// </value>
            public string PersonProfilePhotoUrl { get; set; }

            /// <summary>
            /// Gets or sets the name of person connection status.
            /// </summary>
            /// <value>
            /// The name of person connection status.
            /// </value>
            public string PersonConnectionStatusName { get; set; }

            /// <summary>
            /// Gets or sets the person mobile number.
            /// </summary>
            /// <value>
            /// The person mobile number.
            /// </value>
            public string PersonMobileNumber { get; set; }

            /// <summary>
            /// Gets or sets the person email.
            /// </summary>
            /// <value>
            /// The person email.
            /// </value>
            public string PersonEmail { get; set; }

            /// <summary>
            /// Gets or sets the full name of the connector.
            /// </summary>
            /// <value>
            /// The full name of the connector.
            /// </value>
            public string ConnectorFullName { get; set; }

            /// <summary>
            /// Gets or sets the name of the opportunity.
            /// </summary>
            /// <value>
            /// The name of the opportunity.
            /// </value>
            public string OpportunityName { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is critical.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is critical; otherwise, <c>false</c>.
            /// </value>
            public bool IsCritical { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is idle.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is idle; otherwise, <c>false</c>.
            /// </value>
            public bool IsIdle { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is editable.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is editable; otherwise, <c>false</c>.
            /// </value>
            public bool IsEditable { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can be connected.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can be connected; otherwise, <c>false</c>.
            /// </value>
            public bool CanConnect { get; set; }

            /// <summary>
            /// Gets or sets the name of the campus.
            /// </summary>
            /// <value>
            /// The name of the campus.
            /// </value>
            public string CampusName { get; set; }

            /// <summary>
            /// Gets or sets the name of the placement group.
            /// </summary>
            /// <value>
            /// The name of the placement group.
            /// </value>
            public string PlacementGroupName { get; set; }

            /// <summary>
            /// Gets or sets the placement group requirements.
            /// </summary>
            /// <value>
            /// The placement group requirements.
            /// </value>
            public List<GroupMemberRequirementViewModel> PlacementGroupRequirements { get; set; }

            /// <summary>
            /// Gets or sets the name of the status.
            /// </summary>
            /// <value>
            /// The name of the status.
            /// </value>
            public string StatusName { get; set; }

            /// <summary>
            /// Gets or sets the request date.
            /// </summary>
            /// <value>
            /// The request date.
            /// </value>
            public DateTimeOffset? RequestDate { get; set; }

            /// <summary>
            /// Gets or sets the attributes.
            /// </summary>
            /// <value>
            /// The attributes.
            /// </value>
            public List<PublicEditableAttributeValueViewModel> Attributes { get; set; }

            /// <summary>
            /// Gets or sets the workflow types.
            /// </summary>
            /// <value>
            /// The workflow types.
            /// </value>
            public List<WorkflowTypeItemViewModel> WorkflowTypes { get; set; }

            /// <summary>
            /// Gets or sets the content of the activity.
            /// </summary>
            /// <value>
            /// The content of the activity.
            /// </value>
            public string ActivityContent { get; set; }

            /// <summary>
            /// Gets or sets the activities.
            /// </summary>
            /// <value>The activities.</value>
            public List<ActivityViewModel> Activities { get; set; }

            /// <summary>
            /// Gets or sets the connection request guid.
            /// </summary>
            public Guid ConnectionRequestGuid { get; set; }
        }

        /// <summary>
        /// Identifies a single requirement that must be met before a person
        /// is placed in a placement group.
        /// </summary>
        public class GroupMemberRequirementViewModel
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this requirement is manual.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this requirement is manual; otherwise, <c>false</c>.
            /// </value>
            public bool IsManual { get; set; }

            /// <summary>
            /// Gets or sets the meets group requirement state.
            /// </summary>
            /// <value>
            /// The meets group requirement state.
            /// </value>
            public MeetsGroupRequirement MeetsGroupRequirement { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the requirement must be met before being placed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if the requirement must be met before being placed; otherwise, <c>false</c>.
            /// </value>
            public bool MustMeetRequirementToAddMember { get; set; }

            /// <summary>
            /// Gets or sets the last checked date time.
            /// </summary>
            /// <value>
            /// The last checked date time.
            /// </value>
            public DateTimeOffset? LastCheckedDateTime { get; set; }
        }

        /// <summary>
        /// A workflow type item to be displayed on the connection request.
        /// </summary>
        public class WorkflowTypeItemViewModel
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the icon class.
            /// </summary>
            /// <value>
            /// The icon class.
            /// </value>
            public string IconClass { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }
        }

        /// <summary>
        /// Additional details about the connection request that will be used
        /// when going into edit mode.
        /// </summary>
        /// <seealso cref="RequestViewModelBase" />
        public class RequestEditViewModel : RequestViewModelBase
        {
            /// <summary>
            /// Gets or sets the connectors available.
            /// </summary>
            /// <value>
            /// The connectors available.
            /// </value>
            public List<ConnectorItemViewModel> Connectors { get; set; }

            /// <summary>
            /// Gets or sets the campuses available to pick from.
            /// </summary>
            /// <value>
            /// The campuses available to pick from.
            /// </value>
            public List<ListItemBag> Campuses { get; set; }

            /// <summary>
            /// Gets or sets the placement groups available to pick from.
            /// </summary>
            /// <value>
            /// The placement groups available to pick from.
            /// </value>
            public List<PlacementGroupItemViewModel> PlacementGroups { get; set; }

            /// <summary>
            /// Gets or sets the statuses available to pick from.
            /// </summary>
            /// <value>
            /// The statuses available to pick from.
            /// </value>
            public List<ListItemBag> Statuses { get; set; }

            /// <summary>
            /// Gets or sets the future follow up date.
            /// </summary>
            /// <value>
            /// The future follow up date.
            /// </value>
            public DateTimeOffset? FutureFollowUpDate { get; set; }

            /// <summary>
            /// Gets or sets the attributes that can be edited.
            /// </summary>
            /// <value>
            /// The attributes that can be edited.
            /// </value>
            public List<PublicEditableAttributeValueViewModel> Attributes { get; set; }
        }

        /// <summary>
        /// The object that contains all the information to be updated during
        /// a save operation.
        /// </summary>
        /// <seealso cref="RequestViewModelBase" />
        public class RequestSaveViewModel : RequestViewModelBase
        {
            /// <summary>
            /// Gets or sets the future follow up date.
            /// </summary>
            /// <value>
            /// The future follow up date.
            /// </value>
            public DateTimeOffset? FutureFollowUpDate { get; set; }

            /// <summary>
            /// Gets or sets the attribute values to be saved.
            /// </summary>
            /// <value>
            /// The attribute values to be saved.
            /// </value>
            public Dictionary<string, string> AttributeValues { get; set; }

            /// <summary>
            /// Gets or sets the placement group member role unique identifier.
            /// </summary>
            /// <value>
            /// The placement group member role unique identifier.
            /// </value>
            public Guid? PlacementGroupMemberRoleGuid { get; set; }

            /// <summary>
            /// Gets or sets the placement group member status.
            /// </summary>
            /// <value>
            /// The placement group member status.
            /// </value>
            public GroupMemberStatus? PlacementGroupMemberStatus { get; set; }

            /// <summary>
            /// Gets or sets the placement group member attribute values.
            /// </summary>
            /// <value>
            /// The placement group member attribute values.
            /// </value>
            public Dictionary<string, string> PlacementGroupMemberAttributeValues { get; set; }
        }

        /// <summary>
        /// The data used when adding a new activity to a connection request.
        /// </summary>
        public class ActivityViewModel
        {
            /// <summary>
            /// Gets or sets the activity type unique identifier.
            /// </summary>
            /// <value>
            /// The activity type unique identifier.
            /// </value>
            public Guid ActivityTypeGuid { get; set; }

            /// <summary>
            /// Gets or sets the type of the activity.
            /// </summary>
            /// <value>The type of the activity.</value>
            public string ActivityType { get; set; }

            /// <summary>
            /// Gets or sets the note to save with the activity.
            /// </summary>
            /// <value>
            /// The note to save with the activity.
            /// </value>
            public string Note { get; set; }

            /// <summary>
            /// Gets or sets the connector unique identifier.
            /// </summary>
            /// <value>
            /// The connector unique identifier.
            /// </value>
            public Guid? ConnectorGuid { get; set; }

            /// <summary>
            /// Gets or sets the connector.
            /// </summary>
            /// <value>The connector.</value>
            public ConnectorItemViewModel Connector { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>The unique identifier.</value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the created date time.
            /// </summary>
            /// <value>The created date time.</value>
            public DateTimeOffset CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is modifiable.
            /// </summary>
            /// <value><c>true</c> if this instance is modifiable; otherwise, <c>false</c>.</value>
            public bool IsModifiable { get; set; }
        }

        /// <summary>
        /// Contains details about a placement group will be made available
        /// to the user to select.
        /// </summary>
        public class PlacementGroupItemViewModel
        {
            /// <summary>
            /// Gets or sets the group unique identifier.
            /// </summary>
            /// <value>
            /// The group unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the display name of the group.
            /// </summary>
            /// <value>
            /// The display name of the group.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the campus unique identifier to limit this group to.
            /// </summary>
            /// <value>
            /// The campus unique identifier to limit this group to.
            /// </value>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets the roles that are available on this group.
            /// </summary>
            /// <value>
            /// The roles that are available on this group.
            /// </value>
            public List<PlacementGroupRoleItemViewModel> Roles { get; set; }
        }

        /// <summary>
        /// Contains details about a group member role that is available to
        /// choose from on a placement group.
        /// </summary>
        public class PlacementGroupRoleItemViewModel
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the statuses that can be chosen for this role.
            /// </summary>
            /// <value>
            /// The statuses that can be chosen for this role.
            /// </value>
            public List<GroupMemberStatus> Statuses { get; set; }
        }

        /// <summary>
        /// Contains details about the result of a request to launch a connection
        /// request workflow.
        /// </summary>
        public class ConnectionWorkflowLaunchedViewModel
        {
            /// <summary>
            /// Gets or sets the workflow type unique identifier.
            /// </summary>
            /// <value>
            /// The workflow type unique identifier.
            /// </value>
            public Guid? WorkflowTypeGuid { get; set; }

            /// <summary>
            /// Gets or sets the workflow unique identifier.
            /// </summary>
            /// <value>
            /// The workflow unique identifier.
            /// </value>
            public Guid? WorkflowGuid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance has active entry form.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has active entry form; otherwise, <c>false</c>.
            /// </value>
            public bool HasActiveEntryForm { get; set; }

            /// <summary>
            /// Gets or sets the message to be displayed to the user after
            /// successful completion.
            /// </summary>
            /// <value>
            /// The message to be displayed to the user after successful completion.
            /// </value>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the errors messages generated by the workflow.
            /// </summary>
            /// <value>
            /// The errors messages generated by the workflow.
            /// </value>
            public IList<string> Errors { get; set; }
        }

        /// <summary>
        /// Custom class to store the value along with the attribute. This is for
        /// backwards compatibility with Mobile Shell.
        /// </summary>
        public class PublicEditableAttributeValueViewModel : PublicAttributeBag
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public string Value { get; set; }
        }

        /// <summary>
        /// Used to return the attributes and values from the action
        /// "GetPlacementGroupMemberAttributesAndValues".
        /// </summary>
        public class AttributesAndValuesViewModel
        {
            /// <summary>
            /// Gets or sets the attributes.
            /// </summary>
            /// <value>The attributes.</value>
            public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

            /// <summary>
            /// Gets or sets the values for the attributes.
            /// </summary>
            /// <value>The values for the attributes.</value>
            public Dictionary<string, string> Values { get; set; }
        }

        #endregion
    }
}

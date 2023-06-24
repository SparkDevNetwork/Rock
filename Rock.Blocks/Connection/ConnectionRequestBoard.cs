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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Display the Connection Requests for a selected Connection Opportunity as a list or board view.
    /// </summary>

    [DisplayName( "Connection Request Board" )]
    [Category( "Obsidian > Connection" )]
    [Description( "Display the Connection Requests for a selected Connection Opportunity as a list or board view." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        "Max Cards per Column",
        Key = AttributeKey.MaxCards,
        Description = "The maximum number of cards to display per column. This is to prevent performance issues caused by rendering too many cards at a time.",
        DefaultIntegerValue = DefaultMaxCards,
        IsRequired = true,
        Order = 0 )]

    [LinkedPage(
        "Person Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each grid item.",
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES,
        IsRequired = true,
        Order = 1 )]

    [LinkedPage(
        "Workflow Detail Page",
        Key = AttributeKey.WorkflowDetailPage,
        Description = "Page used to display details about a workflow.",
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL,
        IsRequired = true,
        Order = 2 )]

    [LinkedPage(
        "Workflow Entry Page",
        Key = AttributeKey.WorkflowEntryPage,
        Description = "Page used to launch a new workflow of the selected type.",
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY,
        IsRequired = true,
        Order = 3 )]

    [CodeEditorField(
        "Connection Request Status Icons Template",
        Key = AttributeKey.ConnectionRequestStatusIconsTemplate,
        Description = "Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = ConnectionRequestStatusIconsTemplateDefaultValue,
        IsRequired = true,
        Order = 4 )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "Page used to display group details.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 5 )]

    [LinkedPage(
        "SMS Link Page",
        Key = AttributeKey.SmsLinkPage,
        Description = "Page that will be linked for SMS enabled phones.",
        DefaultValue = Rock.SystemGuid.Page.NEW_COMMUNICATION,
        IsRequired = true,
        Order = 6 )]

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The badges to display in this block.",
        IsRequired = false,
        Order = 7 )]

    [CodeEditorField(
        "Lava Heading Template",
        Key = AttributeKey.LavaHeadingTemplate,
        Description = "The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        IsRequired = false,
        Order = 8 )]

    [CodeEditorField(
        "Lava Badge Bar",
        Key = AttributeKey.LavaBadgeBar,
        Description = "The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        IsRequired = false,
        Order = 9 )]

    [ConnectionTypesField(
        "Connection Types",
        Key = AttributeKey.ConnectionTypes,
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 10 )]

    [BooleanField(
        "Limit to Assigned Connections",
        Key = AttributeKey.OnlyShowAssigned,
        Description = "When enabled, only requests assigned to the current person will be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 11 )]

    [LinkedPage(
        "Connection Request History Page",
        Key = AttributeKey.ConnectionRequestHistoryPage,
        Description = "Page used to display history details.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 12 )]

    [LinkedPage(
        "Bulk Update Requests",
        Key = AttributeKey.BulkUpdateRequestsPage,
        Description = "Page used to update selected connection requests",
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE,
        IsRequired = true,
        Order = 13 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4C517EE6-B440-415B-9D0A-6573AC9EBACB" )]
    [Rock.SystemGuid.BlockTypeGuid( "AEC71B37-5498-47BC-939C-E2C102999D5C" )]
    public class ConnectionRequestBoard : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string Badges = "Badges";
            public const string BulkUpdateRequestsPage = "BulkUpdateRequestsPage";
            public const string ConnectionRequestHistoryPage = "ConnectionRequestHistoryPage";
            public const string ConnectionRequestStatusIconsTemplate = "ConnectionRequestStatusIconsTemplate";
            public const string ConnectionTypes = "ConnectionTypes";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string LavaBadgeBar = "LavaBadgeBar";
            public const string LavaHeadingTemplate = "LavaHeadingTemplate";
            public const string MaxCards = "MaxCards";
            public const string OnlyShowAssigned = "OnlyShowAssigned";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string SmsLinkPage = "SmsLinkPage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";                                  // Incoming
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";    // Incoming
            public const string ConnectionRequestGuid = "ConnectionRequestGuid";                        // Outgoing
            public const string ConnectionRequestId = "ConnectionRequestId";            // Incoming
            public const string ConnectionTypeId = "ConnectionTypeId";                                  // Outgoing
            public const string EntitySetId = "EntitySetId";                                            // Outgoing
            public const string WorkflowId = "WorkflowId";                                              // Outgoing
        }

        /// <summary>
        /// Keys for person preferences.
        /// </summary>
        private static class PersonPreferenceKey
        {
            public const string CampusFilter = "CampusFilter";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
            public const string ConnectorPersonAliasId = "ConnectorPersonAliasId";
            public const string DateRange = "DateRange";
            public const string LastActivities = "LastActivities";
            public const string PastDueOnly = "PastDueOnly";
            public const string Requester = "Requester";
            public const string SortBy = "SortBy";
            public const string States = "States";
            public const string Statuses = "Statuses";
            public const string ViewMode = "ViewMode";
        }

        #endregion Keys

        #region Defaults

        /// <summary>
        /// The default maximum cards per status column.
        /// </summary>
        private const int DefaultMaxCards = 100;

        /// <summary>
        /// The initial count of activities to show in grid. There is a "show more" button for the user to
        /// click if the actual count exceeds this.
        /// </summary>
        private const int InitialActivitiesToShowInGrid = 10;

        /// <summary>
        /// The default connection request status icons template. Used at the top of each connection request card (in card view mode),
        /// the first column of each row (in grid view mode) + the top of the connection request modal.
        /// </summary>
        private const string ConnectionRequestStatusIconsTemplateDefaultValue = @"
<div class='board-card-pills'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='board-card-pill badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='board-card-pill badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='board-card-pill badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='board-card-pill badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
    {% endif %}
</div>
";

        /// <summary>
        /// The default delimiter.
        /// </summary>
        private const string DefaultDelimiter = "|";

        /// <summary>
        /// The default sort property.
        /// </summary>
        private const ConnectionRequestViewModelSortProperty DefaultSortProperty = ConnectionRequestViewModelSortProperty.Order;

        #endregion Defaults

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        private readonly List<ConnectionRequestViewModelSortProperty> _allowedSortProperties = new List<ConnectionRequestViewModelSortProperty>
        {
            ConnectionRequestViewModelSortProperty.Order,
            ConnectionRequestViewModelSortProperty.Requestor,
            ConnectionRequestViewModelSortProperty.Connector,
            ConnectionRequestViewModelSortProperty.DateAdded,
            ConnectionRequestViewModelSortProperty.DateAddedDesc,
            ConnectionRequestViewModelSortProperty.LastActivity,
            ConnectionRequestViewModelSortProperty.LastActivityDesc
        };

        #endregion Fields

        #region Properties

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        public Person CurrentPerson => this.RequestContext.CurrentPerson;

        /// <summary>
        /// Gets the current person identifier, or zero if the person is not defined.
        /// </summary>
        public int CurrentPersonId => this.CurrentPerson?.Id ?? 0;

        /// <summary>
        /// Gets the current person's primary alias identifier, or zero if the person or primary alias identifier is not defined.
        /// </summary>
        public int CurrentPersonAliasId => this.CurrentPerson?.PrimaryAliasId ?? 0;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new ConnectionRequestBoardInitializationBox();

                SetBoxInitialState( rockContext, box );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="box">The box.</param>
        private void SetBoxInitialState( RockContext rockContext, ConnectionRequestBoardInitializationBox box )
        {
            var boardData = GetConnectionRequestBoardData( rockContext );

            box.ConnectionTypes = boardData.AllowedConnectionTypeBags;
            box.SelectedOpportunity = GetSelectedConnectionOpportunity( boardData );

            if ( boardData.ConnectionRequest != null )
            {
                box.SelectedRequest = GetSelectedConnectionRequest( rockContext, new ConnectionRequestBoardSelectRequestBag
                {
                    ConnectionRequestId = boardData.ConnectionRequest.Id,
                    ConnectionOpportunityId = boardData.ConnectionOpportunity.Id
                }, boardData );
            }

            box.MaxCardsPerColumn = GetAttributeValue( AttributeKey.MaxCards ).AsIntegerOrNull() ?? DefaultMaxCards;

            var statusIconsTemplate = GetAttributeValue( AttributeKey.ConnectionRequestStatusIconsTemplate );
            if ( statusIconsTemplate.IsNullOrWhiteSpace() )
            {
                statusIconsTemplate = ConnectionRequestStatusIconsTemplateDefaultValue;
            }

            box.StatusIconsTemplate = Regex.Replace( statusIconsTemplate, @"\s+", " " );
            box.CurrentPersonAliasId = CurrentPersonAliasId;
            box.SecurityGrantToken = GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the connection request board data needed for the board to operate.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="config">A configuration object to dictate how connection request board data should be loaded.</param>
        /// <returns>The connection request board data needed for the board to operate.</returns>
        private ConnectionRequestBoardData GetConnectionRequestBoardData( RockContext rockContext, GetConnectionRequestBoardDataConfig config = null )
        {
            var boardData = new ConnectionRequestBoardData();

            config = config ?? new GetConnectionRequestBoardDataConfig();

            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            GetAllowedConnectionTypes( rockContext, boardData );
            GetConnectionAndCampusSelections( rockContext, boardData, config.IdOverrides );

            if ( !config.IsFilterOptionsLoadingDisabled )
            {
                GetFilterOptions( rockContext, boardData );
            }

            if ( !config.IsPersonPreferenceFilterLoadingDisabled )
            {
                var filters = GetFiltersFromPersonPreferences( boardData );
                ValidateAndApplySelectedFilters( rockContext, boardData, filters );
            }

            if ( !config.IsPersonPreferenceSavingDisabled )
            {
                this.PersonPreferences.Save();
            }

            return boardData;
        }

        /// <summary>
        /// Gets the allowed connection types for the current user and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the allowed connection types.</param>
        private void GetAllowedConnectionTypes( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            var allowedConnectionTypeBags = new List<ConnectionRequestBoardConnectionTypeBag>();

            var opportunitiesQuery = new ConnectionOpportunityService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( o => o.ConnectionType )
                .Include( o => o.ConnectionOpportunityCampuses )
                .Where( o => o.IsActive && o.ConnectionType.IsActive );

            var typeFilter = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();
            if ( typeFilter.Any() )
            {
                opportunitiesQuery = opportunitiesQuery.Where( o => typeFilter.Contains( o.ConnectionType.Guid ) );
            }

            var selfAssignedOpportunityIds = new List<int>();
            var wasSelfAssignedOpportunityIdsQueried = false;

            // Get this person's favorite opportunity IDs so we can mark them as such below.
            var entityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;
            var favoriteOpportunityIds = new FollowingService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( f =>
                    f.EntityTypeId == entityTypeId
                    && string.IsNullOrEmpty( f.PurposeKey )
                    && f.PersonAliasId == CurrentPersonAliasId
                )
                .Select( f => f.EntityId )
                .ToList();

            foreach ( var opportunity in opportunitiesQuery.ToList() )
            {
                if ( opportunity.ConnectionType.EnableRequestSecurity && !wasSelfAssignedOpportunityIdsQueried )
                {
                    selfAssignedOpportunityIds = new ConnectionRequestService( rockContext )
                        .Queryable()
                        .Where( r => r.ConnectorPersonAlias.PersonId == CurrentPersonId )
                        .Select( r => r.ConnectionOpportunityId )
                        .Distinct()
                        .ToList();

                    wasSelfAssignedOpportunityIdsQueried = true;
                }

                var canView = opportunity.IsAuthorized( Authorization.VIEW, CurrentPerson )
                    || (
                        opportunity.ConnectionType.EnableRequestSecurity
                        && selfAssignedOpportunityIds.Contains( opportunity.Id )
                    );

                if ( !canView )
                {
                    continue;
                }

                // Add the opportunity's type if it hasn't already been added.
                var connectionTypeBag = allowedConnectionTypeBags.FirstOrDefault( t => t.Id == opportunity.ConnectionType.Id );
                if ( connectionTypeBag == null )
                {
                    connectionTypeBag = new ConnectionRequestBoardConnectionTypeBag
                    {
                        Id = opportunity.ConnectionType.Id,
                        Name = opportunity.ConnectionType.Name,
                        IconCssClass = opportunity.ConnectionType.IconCssClass,
                        Order = opportunity.ConnectionType.Order,
                        ConnectionOpportunities = new List<ConnectionRequestBoardConnectionOpportunityBag>()
                    };

                    allowedConnectionTypeBags.Add( connectionTypeBag );
                }

                // Add the opportunity.
                connectionTypeBag.ConnectionOpportunities.Add( new ConnectionRequestBoardConnectionOpportunityBag
                {
                    Id = opportunity.Id,
                    PublicName = opportunity.PublicName,
                    IconCssClass = opportunity.IconCssClass,
                    ConnectionTypeName = connectionTypeBag.Name,
                    PhotoUrl = opportunity.PhotoUrl,
                    Order = opportunity.Order,
                    IsFavorite = favoriteOpportunityIds.Contains( opportunity.Id )
                } );
            }

            // Sort each type's opportunities.
            foreach ( var connectionTypeBag in allowedConnectionTypeBags )
            {
                connectionTypeBag.ConnectionOpportunities = connectionTypeBag.ConnectionOpportunities
                    .OrderBy( co => co.Order )
                    .ThenBy( co => co.PublicName )
                    .ToList();
            }

            // Sort and add the allowed types.
            boardData.AllowedConnectionTypeBags = allowedConnectionTypeBags
                .Where( ct => ct.ConnectionOpportunities.Any() )
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .ThenBy( ct => ct.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the selected connection request, connection opportunity and campus, loading them onto the supplied
        /// <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the selected connection request, connection opportunity and campus.</param>
        /// <param name="idOverrides">Optional entity identifiers to override page parameters and person preferences.</param>
        private void GetConnectionAndCampusSelections( RockContext rockContext, ConnectionRequestBoardData boardData, Dictionary<string, int?> idOverrides = null )
        {
            int? connectionOpportunityId = null;

            var availableOpportunityIds = boardData.AllowedConnectionTypeBags
                .SelectMany( ct => ct.ConnectionOpportunities.Select( co => co.Id ) )
                .ToList();

            // If a connection request selection was provided via page parameter or override, it takes priority since it's more specific.
            var selectedConnectionRequestId = GetEntityIdFromPageParameterOrOverride<ConnectionRequest>( PageParameterKey.ConnectionRequestId, rockContext, idOverrides );
            if ( selectedConnectionRequestId.HasValue )
            {
                // Get the connection request instance from the database.
                // Make sure we preload all entities needed for a proper authorization check + collections needed for filters & options.
                var connectionRequest = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( cr => cr.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes )
                    .Include( cr => cr.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
                    .Include( cr => cr.ConnectionOpportunity.ConnectionType.ConnectionWorkflows )
                    .Include( cr => cr.ConnectionOpportunity.ConnectionWorkflows )
                    .Include( cr => cr.ConnectorPersonAlias )
                    .FirstOrDefault( cr => cr.Id == selectedConnectionRequestId.Value );

                if ( connectionRequest != null && availableOpportunityIds.Contains( connectionRequest.ConnectionOpportunityId ) )
                {
                    connectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                    boardData.ConnectionOpportunity = connectionRequest.ConnectionOpportunity;

                    // This means we'll be auto-opening a specific connection request modal.
                    boardData.ConnectionRequest = connectionRequest;
                }
            }

            // If not, was a connection opportunity selection provided via page parameter or override?
            var selectedConnectionOpportunityId = GetEntityIdFromPageParameterOrOverride<ConnectionOpportunity>( PageParameterKey.ConnectionOpportunityId, rockContext, idOverrides );
            if ( !connectionOpportunityId.HasValue
                && selectedConnectionOpportunityId.HasValue
                && availableOpportunityIds.Contains( selectedConnectionOpportunityId.Value ) )
            {
                connectionOpportunityId = selectedConnectionOpportunityId;
            }

            // If not, does this person have a connection opportunity preference?
            var personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.ConnectionOpportunityId );
            int? personPrefConnectionOpportunityId = this.PersonPreferences.GetValue( personPrefKey ).AsIntegerOrNull();
            if ( !connectionOpportunityId.HasValue
                && personPrefConnectionOpportunityId.HasValue
                && availableOpportunityIds.Contains( personPrefConnectionOpportunityId.Value ) )
            {
                connectionOpportunityId = personPrefConnectionOpportunityId;
            }

            // If set (and different than the current preference), update preferences with this connection opportunity.
            if ( connectionOpportunityId.HasValue && connectionOpportunityId != personPrefConnectionOpportunityId )
            {
                this.PersonPreferences.SetValue( personPrefKey, connectionOpportunityId.ToString() );
            }

            // If we didn't already load a connection opportunity instance along with the request above, try to get it from the database.
            if ( boardData.ConnectionOpportunity == null )
            {
                // Make sure we preload all entities needed for a proper authorization check + collections needed for filters & options.
                var query = new ConnectionOpportunityService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( co => co.ConnectionType.ConnectionActivityTypes )
                    .Include( co => co.ConnectionType.ConnectionStatuses )
                    .Include( co => co.ConnectionType.ConnectionWorkflows )
                    .Include( co => co.ConnectionWorkflows )
                    .Where( co =>
                        co.IsActive
                        && boardData.AllowedConnectionTypeIds.Contains( co.ConnectionTypeId )
                    )
                    .OrderBy( co => co.Order )
                    .ThenBy( co => co.Name );

                // Fall back to the first record if one was not explicitly selected.
                boardData.ConnectionOpportunity = connectionOpportunityId.HasValue
                    ? query.FirstOrDefault( co => co.Id == connectionOpportunityId.Value )
                    : query.FirstOrDefault();
            }

            // Does this person have a campus preference for this connection type?
            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.CampusFilter );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                boardData.Filters.CampusId = this.PersonPreferences.GetValue( personPrefKey ).AsIntegerOrNull();
            }

            // If we're not loading a specific connection request, and a campus selection was provided via page parameter or override,
            // it overrules any previous preference.
            if ( boardData.ConnectionRequest == null )
            {
                var selectedCampusId = GetEntityIdFromPageParameterOrOverride<Campus>( PageParameterKey.CampusId, rockContext, idOverrides );
                if ( selectedCampusId.HasValue )
                {
                    boardData.Filters.CampusId = selectedCampusId;
                }

                // The campus filter will be saved to person preferences along with other filters in a later step.
            }
        }

        /// <summary>
        /// Gets the available filter options, some of which are based on the currently-selected connection opportunity,
        /// and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the filter options.</param>
        private void GetFilterOptions( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.Connectors = GetConnectors( rockContext, boardData.ConnectionOpportunity, false, boardData.Filters.CampusId );
            boardData.FilterOptions.Campuses = GetCampuses( rockContext );
            boardData.FilterOptions.ConnectionStatuses = GetConnectionStatuses( boardData.ConnectionOpportunity );
            boardData.FilterOptions.ConnectionStates = GetConnectionStates();
            boardData.FilterOptions.ConnectionActivityTypes = GetConnectionActivityTypes( boardData.ConnectionOpportunity );
            boardData.FilterOptions.SortProperties = GetSortProperties();
        }

        /// <summary>
        /// Gets the available "connector" people.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionOpportunity">The connection opportunity for which to load connectors.</param>
        /// <param name="includeCurrentPerson">Whether to include the current person in the returned list of connectors.</param>
        /// <param name="campusId">The optional campus for which to load connectors.</param>
        /// <param name="personAliasIdToInclude">An optional, additional person to include in the list of connectors.</param>
        /// <returns>The available "connector" people.</returns>
        private List<ListItemBag> GetConnectors( RockContext rockContext, ConnectionOpportunity connectionOpportunity, bool includeCurrentPerson, int? campusId = null, int? personAliasIdToInclude = null )
        {
            var connectors = new List<ListItemBag>();

            if ( connectionOpportunity != null )
            {
                var connectorPeople = new ConnectionOpportunityConnectorGroupService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( g =>
                            g.ConnectionOpportunityId == connectionOpportunity.Id
                            && (
                                !campusId.HasValue
                                || !g.CampusId.HasValue
                                || g.CampusId.Value == campusId.Value
                            )
                        )
                        .SelectMany( g => g.ConnectorGroup.Members )
                        .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( m => m.Person )
                        .Distinct()
                        .Where( p => includeCurrentPerson || p.Id != CurrentPersonId )
                        .Where( p => p.Aliases.Any( a => a.AliasPersonId == p.Id ) )
                        .OrderBy( p => p.LastName )
                        .ThenBy( p => p.NickName )
                        .Select( p => new
                        {
                            p.Aliases.FirstOrDefault( a => a.AliasPersonId == p.Id ).Id,
                            p.NickName,
                            p.LastName
                        } )
                        .ToList();

                connectors.AddRange( connectorPeople
                    .Select( c => new ListItemBag
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.NickName} {c.LastName}"
                    } )
                );
            }

            var personAliasIdString = CurrentPersonAliasId.ToString();
            if ( includeCurrentPerson && CurrentPersonAliasId > 0 && !connectors.Any( c => c.Value == personAliasIdString ) )
            {
                connectors.Add( new ListItemBag
                {
                    Value = personAliasIdString,
                    Text = $"{CurrentPerson.NickName} {CurrentPerson.LastName}"
                } );
            }

            personAliasIdString = personAliasIdToInclude?.ToString();
            if ( personAliasIdToInclude.GetValueOrDefault() > 0 && !connectors.Any( c => c.Value == personAliasIdString ) )
            {
                var person = new PersonAliasService( rockContext ).GetPersonNoTracking( personAliasIdToInclude.Value );
                if ( person != null )
                {
                    connectors.Add( new ListItemBag
                    {
                        Value = personAliasIdString,
                        Text = $"{person.NickName} {person.LastName}"
                    } );
                }
            }

            return connectors;
        }

        /// <summary>
        /// Gets the available campuses.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private List<ListItemBag> GetCampuses( RockContext rockContext )
        {
            return CampusCache.All( rockContext )
                .Where( c => c.IsActive != false )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .Select( c => new ListItemBag
                {
                    Value = c.Id.ToString(),
                    Text = c.ShortCode.IsNullOrWhiteSpace()
                        ? c.Name
                        : c.ShortCode
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the available connection statuses for the specified connection opportunity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionOpportunity">The connection opportunity for which to load connection statuses.</param>
        private List<ListItemBag> GetConnectionStatuses( ConnectionOpportunity connectionOpportunity )
        {
            return ( connectionOpportunity?.ConnectionType?.ConnectionStatuses ?? new List<ConnectionStatus>() )
                .Where( cs => cs.IsActive )
                .OrderBy( cs => cs.Order )
                .ThenByDescending( cs => cs.IsDefault )
                .ThenBy( cs => cs.Name )
                .Select( cs => new ListItemBag
                {
                    Value = cs.Id.ToString(),
                    Text = cs.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the available connection states.
        /// </summary>
        private List<ListItemBag> GetConnectionStates()
        {
            return SettingsExtensions.GetListItemBagList<ConnectionState>();
        }

        /// <summary>
        /// Gets the available connection activity types for the specified connection opportunity.
        /// </summary>
        /// <param name="connectionOpportunity">The connection opportunity for which to load connection activity types.</param>
        private List<ListItemBag> GetConnectionActivityTypes( ConnectionOpportunity connectionOpportunity )
        {
            return ( connectionOpportunity?.ConnectionType?.ConnectionActivityTypes ?? new List<ConnectionActivityType>() )
                .Where( t => t.IsActive )
                .OrderBy( t => t.Name )
                .ThenBy( t => t.Id )
                .Select( t => new ListItemBag
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the available sort properties and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        private List<ConnectionRequestBoardSortPropertyBag> GetSortProperties()
        {
            return new List<ConnectionRequestBoardSortPropertyBag>
            {
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.Order, Title = string.Empty },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.Requestor, Title = "Requestor" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.Connector, Title = "Connector" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.DateAdded, Title = "Date Added", SubTitle = "Oldest First" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.DateAddedDesc, Title = "Date Added", SubTitle = "Newest First" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.LastActivity, Title = "Last Activity", SubTitle = "Oldest First" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.LastActivityDesc, Title = "Last Activity", SubTitle = "Newest First" }
            };
        }

        /// <summary>
        /// Gets any previously-saved filters from person preferences.
        /// </summary>
        /// <param name="boardData">The board data.</param>
        private ConnectionRequestBoardFiltersBag GetFiltersFromPersonPreferences( ConnectionRequestBoardData boardData )
        {
            var filters = new ConnectionRequestBoardFiltersBag();

            if ( boardData.ConnectionOpportunity == null )
            {
                // These preferences are all connection type-specific, so if we don't have a connection opportunity (which will have a type)
                // loaded at this point, we can't load preferences.
                return filters;
            }

            // Simply set any previously-saved values here; they will be validated against currently-available filter values in a later step.

            filters.ConnectorPersonAliasId = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.ConnectorPersonAliasId ) )
                .AsIntegerOrNull();

            filters.RequesterPersonId = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.Requester ) )
                .AsIntegerOrNull();

            // Campus ID will have already been set if applicable, and shouldn't be overridden.
            filters.CampusId = boardData.Filters.CampusId;

            filters.DateRange = RockDateTimeHelper.CreateSlidingDateRangeBagFromDelimitedValues(
                this.PersonPreferences.GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.DateRange ) )
            );

            filters.PastDueOnly = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.PastDueOnly ) )
                .AsBoolean();

            filters.ConnectionStatuses = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.Statuses ) )
                .SplitDelimitedValues()
                .ToList();

            filters.ConnectionStates = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.States ) )
                .SplitDelimitedValues()
                .ToList();

            filters.ConnectionActivityTypes = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.LastActivities ) )
                .SplitDelimitedValues()
                .ToList();

            filters.SortProperty = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.SortBy ) )
                .ConvertToEnumOrNull<ConnectionRequestViewModelSortProperty>() ?? DefaultSortProperty;

            return filters;
        }

        /// <summary>
        /// Validates and applies the provided filters to the board data instance as well as person preferences.
        /// <para>
        /// Person preferences will not be saved to the database here; it's up the caller of this method to save them at the appropriate time.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to apply the selected filters.</param>
        /// <param name="filters">The filters to validate and apply.</param>
        private void ValidateAndApplySelectedFilters( RockContext rockContext, ConnectionRequestBoardData boardData, ConnectionRequestBoardFiltersBag filters )
        {
            // Connector person alias ID.
            // Ensure the selected value exists in the list of available connectors (or matches the current person alias ID).
            var connectorPersonAliasIdString = filters.ConnectorPersonAliasId.ToString();
            if ( connectorPersonAliasIdString.IsNotNullOrWhiteSpace()
                && (
                        connectorPersonAliasIdString == CurrentPersonAliasId.ToString()
                        || boardData.FilterOptions.Connectors.Any( c => c.Value == connectorPersonAliasIdString )
                    )
                )
            {
                boardData.Filters.ConnectorPersonAliasId = filters.ConnectorPersonAliasId;
            }
            else
            {
                boardData.Filters.ConnectorPersonAliasId = null;
            }

            var personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.ConnectorPersonAliasId );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, boardData.Filters.ConnectorPersonAliasId.ToString() );
            }

            // Requester person alias list item bag, person alias ID & person ID.
            // This one's a bit unique, in that all 3 of these values are needed to manage the selected "requester" person:
            //
            //  1) The list item bag is used with an Obsidian person [alias] picker.
            //  2) The person alias ID is what's actually sent to a preexisting v1 API endpoint in order to perform filtering when
            //     retrieving connection requests.
            //  3) The person ID is stored as a person preference (to maintain compatibility with person preferences previously
            //     stored via the Web Forms version of this block).
            //
            // IF the list item bag value is defined on the selected filters, assume a selection was made on the client and use it to:
            //  1) Ensure the selected value represents a valid person.
            //  2) Set the same list item bag value on the outgoing filters (which we'll send back to the client to preselect the
            //     person in the Obsidian person picker).
            //  3) Look up and set the person alias ID on the outgoing filters (which we'll send back to the client for use with the v1 API).
            //  4) Look up and set the person ID on person preferences.
            //
            // ELSE IF the person ID is defined on the selected filters, assume it was loaded from person preferences and use it to:
            //  1) Ensure the selected value represents a valid person.
            //  2) Look up and set the corresponding list item bag value on the outgoing filters (which we'll send back to the client
            //     to preselect the person in the Obsidian person picker).
            //  3) Look up and set the person alias ID on the outgoing filters (which we'll send back to the client for use with the v1 API).
            //
            // ELSE (neither the list item bag nor the person ID values are defined):
            //  1) Ensure any list item bag and person alias ID values are cleared on the outgoing filters.
            //  2) Clear out any existing person ID person preference value.
            PersonAlias requesterPersonAlias = null;
            var personAliasService = new PersonAliasService( rockContext );
            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.Requester );
            if ( !string.IsNullOrWhiteSpace( filters.Requester?.Value ) )
            {
                requesterPersonAlias = personAliasService.GetNoTracking( filters.Requester.Value );
                if ( requesterPersonAlias != null )
                {
                    if ( personPrefKey.IsNotNullOrWhiteSpace() )
                    {
                        this.PersonPreferences.SetValue( personPrefKey, requesterPersonAlias.PersonId.ToString() );
                    }
                }
            }
            else if ( filters.RequesterPersonId.HasValue )
            {
                requesterPersonAlias = personAliasService.GetPrimaryAlias( filters.RequesterPersonId.Value );
            }

            if ( requesterPersonAlias != null )
            {
                boardData.Filters.Requester = requesterPersonAlias.ToListItemBag();
                boardData.Filters.RequesterPersonAliasId = requesterPersonAlias.Id;
            }
            else
            {
                boardData.Filters.Requester = null;
                boardData.Filters.RequesterPersonAliasId = null;

                if ( personPrefKey.IsNotNullOrWhiteSpace() )
                {
                    this.PersonPreferences.SetValue( personPrefKey, null );
                }
            }

            // This value never needs to go to the client, so lets ensure it's cleared out.
            boardData.Filters.RequesterPersonId = null;

            // Campus ID.
            // Ensure the selected value exists in the list of available campuses AND we have 2 or more campuses.
            var campusIdString = filters.CampusId.ToString();
            if ( campusIdString.IsNotNullOrWhiteSpace()
                && boardData.FilterOptions.Campuses.Count > 1
                && boardData.FilterOptions.Campuses.Any( c => c.Value == campusIdString ) )
            {
                boardData.Filters.CampusId = filters.CampusId;
            }
            else
            {
                boardData.Filters.CampusId = null;
            }

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.CampusFilter );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, boardData.Filters.CampusId.ToString() );
            }

            // Date range.
            boardData.Filters.DateRange = filters.DateRange;

            var delimitedDateRangeValues = RockDateTimeHelper.GetDelimitedValues( filters.DateRange );
            if ( delimitedDateRangeValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( delimitedDateRangeValues );
                boardData.Filters.MinDate = dateRange.Start;
                boardData.Filters.MaxDate = dateRange.End;
            }

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.DateRange );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, delimitedDateRangeValues );
            }

            // Past due only.
            boardData.Filters.PastDueOnly = filters.PastDueOnly;

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.PastDueOnly );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, boardData.Filters.PastDueOnly.ToString() );
            }

            // Connection statuses.
            // Ensure the selected values exist in the list of available statuses.
            var statuses = ( filters.ConnectionStatuses ?? new List<string>() )
                .Where( status => boardData.FilterOptions.ConnectionStatuses.Any( s => s.Value == status ) )
                .ToList();

            boardData.Filters.ConnectionStatuses = statuses;

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.Statuses );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, statuses.AsDelimited( DefaultDelimiter ) );
            }

            // Connection states.
            // Ensure the selected values exist in the list of available states.
            var states = ( filters.ConnectionStates ?? new List<string>() )
                .Where( state => boardData.FilterOptions.ConnectionStates.Any( s => s.Value == state ) )
                .ToList();

            boardData.Filters.ConnectionStates = states;

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.States );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, states.AsDelimited( DefaultDelimiter ) );
            }

            // Connection activity types.
            // Ensure the selected values exist in the list of available activity types.
            var activityTypes = ( filters.ConnectionActivityTypes ?? new List<string>() )
                .Where( activityType => boardData.FilterOptions.ConnectionActivityTypes.Any( a => a.Value == activityType ) )
                .ToList();

            boardData.Filters.ConnectionActivityTypes = activityTypes;

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.LastActivities );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, activityTypes.AsDelimited( DefaultDelimiter ) );
            }

            // Sort property.
            // Ensure the selected value exists in the list of [allowed] sort properties.
            var sortProperty = _allowedSortProperties.Contains( filters.SortProperty )
                ? filters.SortProperty
                : DefaultSortProperty;

            boardData.Filters.SortProperty = sortProperty;

            personPrefKey = boardData.GetPersonPreferenceKey( PersonPreferenceKey.SortBy );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, sortProperty.ConvertToStringSafe() );
            }
        }

        /// <summary>
        /// Gets the selected connection opportunity and supporting information from the supplied <see cref="ConnectionRequestBoardData"/> instance
        /// and person preferences.
        /// </summary>
        /// <param name="boardData">The board data.</param>
        /// <returns>The selected connection opportunity and supporting information.</returns>
        private ConnectionRequestBoardSelectedOpportunityBag GetSelectedConnectionOpportunity( ConnectionRequestBoardData boardData )
        {
            var selectedOpportunity = new ConnectionRequestBoardSelectedOpportunityBag
            {
                ConnectionOpportunity = boardData.ConnectionOpportunityBag,
                FilterOptions = boardData.FilterOptions,
                Filters = boardData.Filters,
                IsRequestSecurityEnabled = boardData.GetIsRequestSecurityEnabled( CurrentPerson )
            };

            if ( selectedOpportunity.ConnectionOpportunity != null )
            {
                selectedOpportunity.IsCardViewMode = this.PersonPreferences
                    .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.ViewMode ) )
                    .AsBooleanOrNull() ?? true;
            }

            return selectedOpportunity;
        }

        //private bool IsConnectionRequestAddingEnabled( ConnectionOpportunity connectionOpportunity )
        //{

        //}

        /// <summary>
        /// Gets the selected connection request and supporting information (options for drop down menus, Etc.).
        /// <para>
        /// If a <see cref="ConnectionRequestBoardSelectRequestBag.ConnectionRequestId"/> of 0 is provided, this indicates the individual
        /// is attempting to add a new connection request; only the drop down menu options, Etc. will be loaded in this case.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectRequest">An object containing information needed to select (or add a new) connection request.</param>
        /// <param name="boardData">The board data, if we already have it; will be built on demand within this method if not.</param>
        /// <returns>The selected connection request and supporting information (options for drop down menus, Etc.).</returns>
        private ConnectionRequestBoardSelectedRequestBag GetSelectedConnectionRequest( RockContext rockContext, ConnectionRequestBoardSelectRequestBag selectRequest, ConnectionRequestBoardData boardData = null )
        {
            // If, for some reason, a connection opportunity wasn't specified, simply send back null.
            if ( selectRequest == null || selectRequest.ConnectionOpportunityId <= 0 )
            {
                return null;
            }

            if ( boardData == null )
            {
                // No need to load board-level filters or person preferences in this case.
                var config = new GetConnectionRequestBoardDataConfig
                {
                    IsFilterOptionsLoadingDisabled = true,
                    IsPersonPreferenceFilterLoadingDisabled = true,
                    IsPersonPreferenceSavingDisabled = true,
                    IdOverrides = new Dictionary<string, int?>
                    {
                        { PageParameterKey.ConnectionOpportunityId, selectRequest.ConnectionOpportunityId },
                        { PageParameterKey.ConnectionRequestId, selectRequest.ConnectionRequestId }
                    }
                };

                boardData = GetConnectionRequestBoardData( rockContext, config );
            }

            // Start by validating the specified connection opportunity and/or request. Does this person have access?
            if ( boardData.ConnectionOpportunity?.Id != selectRequest.ConnectionOpportunityId )
            {
                // The specified connection opportunity wasn't successfully loaded; it may not be allowed, Etc.
                return null;
            }



            var selectedRequest = new ConnectionRequestBoardSelectedRequestBag
            {
                RequestOptions = GetRequestOptions( rockContext, boardData )
            };

            return selectedRequest;
        }

        //private bool IsConnectionRequestViewEnabled( ConnectionRequest connectionRequest, ConnectionOpportunity connectionOpportunity )
        //{

        //}

        //private bool IsConnectionRequestEditEnabled( ConnectionRequest connectionRequest, ConnectionOpportunity connectionOpportunity )
        //{

        //}

        /// <summary>
        /// Gets the available request options.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data to help decide which options should be presented.</param>
        /// <returns>The available request options.</returns>
        private ConnectionRequestBoardRequestOptionsBag GetRequestOptions( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            return new ConnectionRequestBoardRequestOptionsBag
            {
                Connectors = GetConnectors( rockContext, boardData.ConnectionOpportunity, true, boardData.ConnectionRequest?.CampusId )
            };
        }

        /// <summary>
        /// Validates and selects the connection opportunity with the specified identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionOpportunityId">The identifier of the connection opportunity to select.</param>
        /// <returns>An object containing the validated and selected connection opportunity and supporting information.</returns>
        private ConnectionRequestBoardSelectedOpportunityBag ValidateAndSelectConnectionOpportunity( RockContext rockContext, int connectionOpportunityId )
        {
            var config = new GetConnectionRequestBoardDataConfig
            {
                IdOverrides = new Dictionary<string, int?>
                {
                    { PageParameterKey.ConnectionOpportunityId, connectionOpportunityId },
                    { PageParameterKey.ConnectionRequestId, null } // Overwrite any connection request ID that might be in the query string.
                }
            };

            var boardData = GetConnectionRequestBoardData( rockContext, config );

            return GetSelectedConnectionOpportunity( boardData );
        }

        /// <summary>
        /// Tries to save the specified person preference.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionOpportunityId">The identifier of the connection opportunity to which this preference relates.</param>
        /// <param name="personPreferenceSubkey">The person preference subkey (most preferences within this block are connection type-specific).</param>
        /// <param name="value">The person preference value to save.</param>
        private void TrySavePersonPreference( RockContext rockContext, int connectionOpportunityId, string personPreferenceSubkey, string value )
        {
            // Get the minimum required board data to ensure this individual is allowed to save the specified person preference.
            var config = new GetConnectionRequestBoardDataConfig
            {
                IsFilterOptionsLoadingDisabled = true,
                IsPersonPreferenceFilterLoadingDisabled = true,
                IsPersonPreferenceSavingDisabled = true,
                IdOverrides = new Dictionary<string, int?>
                {
                    { PageParameterKey.CampusId, null },
                    { PageParameterKey.ConnectionOpportunityId, connectionOpportunityId },
                    { PageParameterKey.ConnectionRequestId, null }
                }
            };

            var boardData = GetConnectionRequestBoardData( rockContext, config );
            if ( boardData.ConnectionOpportunity?.Id != connectionOpportunityId )
            {
                // The specified connection opportunity wasn't successfully loaded; it may not be allowed, Etc.
                return;
            }

            var personPrefKey = boardData.GetPersonPreferenceKey( personPreferenceSubkey );
            if ( personPrefKey.IsNotNullOrWhiteSpace() )
            {
                this.PersonPreferences.SetValue( personPrefKey, value );
                this.PersonPreferences.Save();
            }
        }

        /// <summary>
        /// Validates and saves filters to person preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="applyFilters">The filters to validate and save.</param>
        /// <returns>An object containing the validated and saved filters.</returns>
        private ConnectionRequestBoardFiltersBag ValidateAndSaveFilters( RockContext rockContext, ConnectionRequestBoardApplyFiltersBag applyFilters )
        {
            applyFilters = applyFilters ?? new ConnectionRequestBoardApplyFiltersBag
            {
                Filters = new ConnectionRequestBoardFiltersBag()
            };

            // Don't load current filter selections from person preferences, as the provided filters object is the source of truth for this call.
            // Also, we'll manually save person preferences after applying the provided filters selections.
            var config = new GetConnectionRequestBoardDataConfig
            {
                IsPersonPreferenceFilterLoadingDisabled = true,
                IsPersonPreferenceSavingDisabled = true,
                IdOverrides = new Dictionary<string, int?>
                {
                    { PageParameterKey.ConnectionOpportunityId, applyFilters.ConnectionOpportunityId },
                    { PageParameterKey.CampusId, applyFilters.Filters.CampusId }
                }
            };

            // This call will preload the available filter options, against which we can validate the provided filter selections.
            var boardData = GetConnectionRequestBoardData( rockContext, config );
            if ( boardData.ConnectionOpportunity?.Id != applyFilters.ConnectionOpportunityId )
            {
                // The specified connection opportunity wasn't successfully loaded; it may not be allowed, Etc.
                // We'll just return an empty filters object.
                return new ConnectionRequestBoardFiltersBag();
            }

            ValidateAndApplySelectedFilters( rockContext, boardData, applyFilters.Filters );

            this.PersonPreferences.Save();

            return boardData.Filters;
        }

        /// <summary>
        /// Gets the grid data and definition for displaying the connection request board in grid view mode.
        /// <para>
        /// The provided filters will be validated and saved before being used to get grid data.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="applyFilters">The filters that should be used when getting grid data.</param>
        /// <returns>An object containing the grid data and definition as well as the validated and saved filters for the connection request board
        /// to operate in grid view mode.</returns>
        private ConnectionRequestBoardGridDataBag GetGridData( RockContext rockContext, ConnectionRequestBoardApplyFiltersBag applyFilters )
        {
            var gridDataBag = new ConnectionRequestBoardGridDataBag
            {
                Filters = ValidateAndSaveFilters( rockContext, applyFilters )
            };

            var connectionRequestService = new ConnectionRequestService( rockContext );

            // TODO (Jason): build grid data and definition objects.

            return gridDataBag;
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> integer ID value if it exists in the override collection or can be parsed from page parameters,
        /// or <see langword="null"/> if not.
        /// <para>
        /// The page parameter's value may be an integer ID (if predictable IDs are allowed by site settings), a Guid, or an IdKey.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The <see cref="IEntity"/> type whose ID should be parsed.</typeparam>
        /// <param name="pageParameterKey">The key of the page parameter from which to parse the ID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="idOverrides">Optional entity identifiers to override page parameters and person preferences.</param>
        /// <returns>The <see cref="IEntity"/> integer ID value if it exists in the override collection or can be parsed from page parameters,
        /// or <see langword="null"/> if not.</returns>
        private int? GetEntityIdFromPageParameterOrOverride<T>( string pageParameterKey, RockContext rockContext, Dictionary<string, int?> idOverrides = null ) where T : IEntity
        {
            if ( idOverrides?.TryGetValue( pageParameterKey, out int? id ) == true )
            {
                return id;
            }

            var entityKey = PageParameter( pageParameterKey );
            if ( entityKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var entityTypeId = EntityTypeCache.GetId( typeof( T ) );
            if ( !entityTypeId.HasValue )
            {
                return null;
            }

            return Reflection.GetEntityIdForEntityType( entityTypeId.Value, entityKey, !PageCache.Layout.Site.DisablePredictableIds, rockContext );
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant().ToToken();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Selects the specified connection opportunity.
        /// </summary>
        /// <param name="connectionOpportunityId">The identifier of the connection opportunity to select.</param>
        /// <returns>An object containing the validated and selected connection opportunity and supporting information.</returns>
        [BlockAction]
        public BlockActionResult SelectConnectionOpportunity( int connectionOpportunityId )
        {
            using ( var rockContext = new RockContext() )
            {
                var selectedOpportunity = ValidateAndSelectConnectionOpportunity( rockContext, connectionOpportunityId );

                return ActionOk( selectedOpportunity );
            }
        }

        /// <summary>
        /// Gets the allowed connection types and their respective connection opportunities.
        /// </summary>
        /// <returns>The allowed connection types and their respective connection opportunities.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionTypes()
        {
            using ( var rockContext = new RockContext() )
            {
                var boardData = new ConnectionRequestBoardData();

                GetAllowedConnectionTypes( rockContext, boardData );

                return ActionOk( boardData.AllowedConnectionTypeBags );
            }
        }

        /// <summary>
        /// Saves the provided view mode to person preferences.
        /// </summary>
        /// <param name="bag">The view mode preference to save.</param>
        /// <returns>200-OK response with no content.</returns>
        [BlockAction]
        public BlockActionResult SaveViewMode( ConnectionRequestBoardApplyViewModeBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( bag != null )
                {
                    TrySavePersonPreference( rockContext, bag.ConnectionOpportunityId, PersonPreferenceKey.ViewMode, bag.IsCardViewMode.ToString() );
                }

                return ActionOk();
            }
        }

        /// <summary>
        /// Saves the provided filters to person preferences.
        /// </summary>
        /// <param name="bag">The filters to save.</param>
        /// <returns>An object containing the validated and saved filters.</returns>
        [BlockAction]
        public BlockActionResult SaveFilters( ConnectionRequestBoardApplyFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var savedFilters = ValidateAndSaveFilters( rockContext, bag );

                return ActionOk( savedFilters );
            }
        }

        /// <summary>
        /// Gets the grid data and definition for displaying the connection request board in grid view mode.
        /// </summary>
        /// <param name="bag">The filters that should be used when getting grid data.</param>
        /// <returns>An object containing the grid data and definition as well as the validated and saved filters for the connection request board
        /// to operate in grid view mode.</returns>
        [BlockAction]
        public BlockActionResult GetGridData( ConnectionRequestBoardApplyFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var gridData = GetGridData( rockContext, bag );

                return ActionOk( gridData );
            }
        }

        /// <summary>
        /// Gets the specified connection request and supporting information (options for drop down menus, Etc.).
        /// </summary>
        /// <param name="bag">an object containing information needed to select (or add a new) connection request.</param>
        /// <returns>An object containing the specified connection request and supporting information.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionRequest( ConnectionRequestBoardSelectRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var selectedRequest = GetSelectedConnectionRequest( rockContext, bag );

                return ActionOk( selectedRequest );
            }
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A runtime object representing the data needed for the block to operate.
        /// <para>
        /// This object is intended to be assembled using a combination of page parameter values, person preferences and existing
        /// database records; to be passed between private helper methods as needed, and ultimately sent back out the door in the
        /// form of view models.
        /// </para>
        /// </summary>
        private class ConnectionRequestBoardData
        {
            /// <summary>
            /// Gets or sets the allowed connection type bags.
            /// </summary>
            public List<ConnectionRequestBoardConnectionTypeBag> AllowedConnectionTypeBags { get; set; } = new List<ConnectionRequestBoardConnectionTypeBag>();

            /// <summary>
            /// Gets the allowed connection type IDs.
            /// </summary>
            public IEnumerable<int> AllowedConnectionTypeIds => this.AllowedConnectionTypeBags?.Select( ct => ct.Id ) ?? new List<int>();

            /// <summary>
            /// Gets or sets the selected connection opportunity.
            /// </summary>
            public ConnectionOpportunity ConnectionOpportunity { get; set; }

            /// <summary>
            /// Gets the selected connection opportunity bag.
            /// </summary>
            public ConnectionRequestBoardConnectionOpportunityBag ConnectionOpportunityBag => this.ConnectionOpportunity == null
                ? null
                : this.AllowedConnectionTypeBags
                    ?.SelectMany( ct => ct.ConnectionOpportunities )
                    ?.FirstOrDefault( co => co.Id == this.ConnectionOpportunity.Id );

            /// <summary>
            /// Gets or sets the selected connection request, if a specific request should be opened.
            /// </summary>
            public ConnectionRequest ConnectionRequest { get; set; }

            /// <summary>
            /// Gets or sets the available filter options.
            /// </summary>
            public ConnectionRequestBoardFilterOptionsBag FilterOptions { get; } = new ConnectionRequestBoardFilterOptionsBag();

            /// <summary>
            /// Gets or sets the [selected] filters.
            /// </summary>
            public ConnectionRequestBoardFiltersBag Filters { get; } = new ConnectionRequestBoardFiltersBag();

            /// <summary>
            /// Gets the appropriate person preference key for the specified subkey. Most keys will be connection type-specific.
            /// </summary>
            /// <param name="subkey">The subkey.</param>
            /// <returns>The appropriate person preference key for the specified subkey.</returns>
            public string GetPersonPreferenceKey( string subkey )
            {
                if ( subkey == PersonPreferenceKey.ConnectionOpportunityId )
                {
                    // This key - in particular - should span all connection types.
                    return subkey;
                }

                // The rest of the keys are connection type-specific.
                if ( this.ConnectionOpportunity == null )
                {
                    return null;
                }

                return $"{this.ConnectionOpportunity.ConnectionTypeId}-{subkey}";
            }

            /// <summary>
            /// Gets whether security is enabled for the current connection request and person combination.
            /// </summary>
            /// <param name="currentPerson">The current person viewing this block.</param>
            /// <returns>Whether security is enabled for the current connection request and person combination.</returns>
            public bool GetIsRequestSecurityEnabled( Person currentPerson )
            {
                var isSecurityEnabledForType = this.ConnectionOpportunity?.ConnectionType?.EnableRequestSecurity ?? false;
                var isPersonAuthorized = this.ConnectionOpportunity?.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) ?? false;

                return isSecurityEnabledForType && isPersonAuthorized;
            }
        }

        /// <summary>
        /// A runtime object to dictate how connection request board data should be loaded.
        /// </summary>
        private class GetConnectionRequestBoardDataConfig
        {
            /// <summary>
            /// Gets or sets whether to disable the loading of filter options.
            /// </summary>
            public bool IsFilterOptionsLoadingDisabled { get; set; }

            /// <summary>
            /// Gets or sets whether to disable the loading of current filter selections from person preferences.
            /// </summary>
            public bool IsPersonPreferenceFilterLoadingDisabled { get; set; }

            /// <summary>
            /// Gets or sets whether to disable the saving of any person preferences changes, as a final step of loading connection request board data.
            /// </summary>
            public bool IsPersonPreferenceSavingDisabled { get; set; }

            /// <summary>
            /// Gets or sets optional entity identifiers to override page parameters and person preferences.
            /// </summary>
            public Dictionary<string, int?> IdOverrides { get; set; }
        }

        #endregion
    }
}

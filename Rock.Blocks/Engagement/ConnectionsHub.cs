using System.ComponentModel;
using System.Linq;
using Rock.Attribute;
using Rock.Web.UI;

using Rock.Model;
using Rock.ViewModels.Blocks.Engagement.ConnectionsHub;
using Rock.ViewModels.Blocks;
using Rock.Obsidian.UI;
using Rock.Web.Cache;
using System.Data.Entity;
using Rock.SystemGuid;
using Rock.ViewModels.Core.Grid;
using Rock.Utility;
using System;
using Rock.ViewModels.Utility;
using Rock.Security;
using System.Collections.Generic;
using Rock.Data;
using Newtonsoft.Json;
using Rock.SystemKey;
using Rock.Web;
using Rock.Enums.Connection;
using Rock.Tasks;
using Rock.Web.UI.Controls;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rock.AI.Classes.ChatCompletions;
using static Rock.Model.ConnectionType.ConnectionTypeAdditionalSettings;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the Connections Hub.
    /// </summary>

    [DisplayName( "Connections Hub" )]
    [Category( "Engagement" )]
    [Description( "Displays the Connections Hub." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [LinkedPage(
        "Person Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each grid item.",
        Order = 0,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "Page used to display group details.",
        IsRequired = true,
        Order = 1,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER )]

    [LinkedPage(
        "Workflow Detail Page",
        Key = AttributeKey.WorkflowDetailPage,
        Description = "Page used to display details about a workflow.",
        Order = 2,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL )]

    [LinkedPage(
        "Workflow Entry Page",
        Key = AttributeKey.WorkflowEntryPage,
        Description = "Page used to launch a new workflow of the selected type.",
        Order = 3,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY )]

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The badges to display in this block.",
        IsRequired = false,
        Order = 4 )]

    [CodeEditorField(
        "Lava Heading Template",
        Key = AttributeKey.LavaHeadingTemplate,
        Description = "The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        IsRequired = false,
        EditorMode = CodeEditorMode.Lava,
        Order = 5 )]

    [CodeEditorField(
        "Lava Badge Bar",
        Key = AttributeKey.LavaBadgeBar,
        Description = "The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        IsRequired = false,
        EditorMode = CodeEditorMode.Lava,
        Order = 6 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "CEE15B88-3B23-4378-9CB1-E59A97A94D1B" )]
    [Rock.SystemGuid.BlockTypeGuid( "8674FB3A-9E0E-421C-821C-2DA862A20ED2" )]
    public class ConnectionsHub : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string Badges = "Badges";
            public const string LavaHeadingTemplate = "LavaHeadingTemplate";
            public const string LavaBadgeBar = "LavaBadgeBar";
        }

        private static class NavigationUrlKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string GroupDetailPage = "GroupDetailPage";
        }

        private static class PageParameterKey
        {
            public const string ConnectionType = "ConnectionType";
            public const string Connector = "Connector";
            public const string ConnectionOpportunity = "ConnectionOpportunity";
            public const string Request = "Request";
        }

        private static class PreferenceKey
        {
            public const string ConnectionmOpportunityFilterConnectionTypeIdKey = "ConnectionOpportunityFilter_ConnectionTypeIdKey_{0}";
            public const string SelectedGroupByMode = "SelectedGroupByMode";
            public const string AreOnlyMyRequestsVisible = "AreOnlyMyRequestsVisible";
            public const string SelectedConnector = "SelectedConnector";
            public const string FilterStateConnectionTypeIdKey = "FilterState_ConnectionTypeIdKey_{0}";
        }

        private static class SqlParamKey
        {
            public const string ConnectionRequestEntityTypeId = "@ConnectionRequestEntityTypeId";
            public const string RequestId = "@RequestId";
            public const string CategoryId = "@CategoryId";
            public const string SourceEntityTypeId = "@SourceEntityTypeId";
            public const string SourceEntityId = "@SourceEntityId";
            public const string TargetEntityTypeId = "@TargetEntityTypeId";
        }

        #endregion Keys

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        #endregion Fields

        #region Properties

        protected bool AreOnlyMyRequestsVisible => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.AreOnlyMyRequestsVisible )
            .AsBoolean( true );

        protected Guid? SelectedConnector => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedConnector )
            .AsGuidOrNull();

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

        #endregion Properties

        #region Methods

        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ConnectionsHubOptionsBag>();
            var builder = GetGridBuilder();
            box.Options = GetOptions();
            box.NavigationUrls = GetBoxNavigationUrls();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Sets default person preference values for this block if they have not yet been configured.
        /// This runs during block initialization so that the client receives the correct defaults
        /// on first page load without needing fallback logic on the client side.
        /// </summary>
        private void SetDefaultPreferences( string connectionTypeIdKey )
        {
            if ( connectionTypeIdKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var stateFilterKey = string.Format( PreferenceKey.FilterStateConnectionTypeIdKey, connectionTypeIdKey );

            var existingValue = this.PersonPreferences.GetValue( stateFilterKey );
            var hasValue = existingValue.FromJsonOrNull<List<int>>()?.Count > 0;

            if ( !hasValue )
            {
                this.PersonPreferences.SetValue( stateFilterKey, new List<string> { ConnectionState.Active.ToString( "D" ) }.ToJson() );
                this.PersonPreferences.Save();
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.PersonProfilePage] = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, "PersonId", "((Key))" ),
                [NavigationUrlKey.GroupDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupDetailPage, "GroupId", "((Key))" )
            };
        }

        /// <summary>
        /// Gets the various options needed to display the Connections Hub Block.
        /// </summary>
        /// <returns></returns>
        private ConnectionsHubOptionsBag GetOptions()
        {
            var options = new ConnectionsHubOptionsBag();
            ConnectionType connectionType;

            if ( PageParameter( PageParameterKey.ConnectionType ).IsNullOrWhiteSpace() && PageParameter( PageParameterKey.ConnectionOpportunity ).IsNullOrWhiteSpace() && PageParameter( PageParameterKey.Request ).IsNotNullOrWhiteSpace() )
            {
                options.ConnectionRequestIdKey = new ConnectionRequestService( RockContext ).Get( PageParameter( PageParameterKey.Request ), !PageCache.Layout.Site.DisablePredictableIds )?.IdKey ?? string.Empty;
                return options;
            }

            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).GetInclude( PageParameter( PageParameterKey.ConnectionOpportunity ), o => o.ConnectionType, !PageCache.Layout.Site.DisablePredictableIds );

            if ( connectionOpportunity != null )
            {
                options.ConnectionOpportunityGuidFromPageParameter = connectionOpportunity.Guid;
                connectionType = connectionOpportunity.ConnectionType;
            }
            else
            {
                connectionType = new ConnectionTypeService( RockContext ).GetInclude( PageParameter( PageParameterKey.ConnectionType ), a => a.ConnectionStatuses, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( connectionType == null )
            {
                return options;
            }

            options.Title = connectionType.Name + " Requests";
            options.IconCssClass = connectionType.IconCssClass;

            if ( PageParameter( PageParameterKey.Request ).IsNotNullOrWhiteSpace() )
            {
                options.ConnectionRequestIdKey = new ConnectionRequestService( RockContext ).Get( PageParameter( PageParameterKey.Request ), !PageCache.Layout.Site.DisablePredictableIds )?.IdKey ?? string.Empty;
            }

            var connectionTypeIdKey = IdHasher.Instance.GetHash( connectionType.Id );
            SetDefaultPreferences( connectionTypeIdKey );

            options.ConnectionTypeIdKey = connectionTypeIdKey;
            options.RequiresPlacementGroupToComplete = connectionType.RequiresPlacementGroupToConnect;
            options.IsSequentialStatusMode = connectionType.IsSequentialStatusEnforced;

            // If a Connection Opportunity was provided as a page parameter, seed the person preference
            // so that GetGridData only needs to read from the preference (not the page parameter).
            // This allows the user to subsequently clear the filter and have the server respect that.
            if ( connectionOpportunity != null )
            {
                this.PersonPreferences.SetValue( string.Format( PreferenceKey.ConnectionmOpportunityFilterConnectionTypeIdKey, connectionTypeIdKey ), connectionOpportunity.Guid.ToString() );
                this.PersonPreferences.Save();
            }

            var connectionOpportunityFilter = GetConnectionOpportunityFilter( connectionTypeIdKey );
            if ( connectionOpportunity == null && connectionOpportunityFilter.HasValue )
            {
                connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( connectionOpportunityFilter.Value );
            }
            if ( connectionOpportunity != null )
            {
                options.ConnectionOpportunityDetailsFromFilter = GetConnectionOpportunityDetailBag( connectionOpportunity );
            }

            options.CanEditConnectionRequests = CanEditConnectionRequests( connectionType, connectionOpportunity );

            var connectorPerson = new PersonService( RockContext ).Get( PageParameter( PageParameterKey.Connector ), !PageCache.Layout.Site.DisablePredictableIds );
            if ( connectorPerson != null )
            {
                var connectorListItemBag = new ListItemBag
                {
                    Text = $"{connectorPerson.FullName.ToPossessive()} Requests",
                    Value = connectorPerson.PrimaryAliasGuid.ToString()
                };

                options.SelectedConnector = connectorListItemBag;
                this.PersonPreferences.SetValue( PreferenceKey.SelectedConnector, connectorPerson.PrimaryAliasGuid.ToString() );
                this.PersonPreferences.SetValue( PreferenceKey.SelectedGroupByMode, "connectorGrouping" );
                this.PersonPreferences.Save();
            }

            List<ConnectionState> ignoredConnectionStates = new List<ConnectionState>();

            if ( !connectionType.EnableFutureFollowup )
            {
                ignoredConnectionStates.Add( ConnectionState.FutureFollowUp );
            }

            var connectors = connectionType.ConnectionOpportunities
                .Where( o => o.IsActive )
                .SelectMany( o => o.ConnectionOpportunityConnectorGroups )
                .SelectMany( g => g.ConnectorGroup.Members )
                .DistinctBy( m => m.Person.PrimaryAlias.Guid )
                .Select( m => new ListItemBag
                {
                    Value = m.Person.PrimaryAlias.Guid.ToString(),
                    Text = m.Person.FullName
                } )
                .ToList();

            var currentPersonAliasGuid = RequestContext.CurrentPerson.PrimaryAlias?.Guid;

            // Add current person to the connector list to mirror Webforms
            if ( currentPersonAliasGuid.HasValue && !connectors.Any( c => c.Value == currentPersonAliasGuid.Value.ToString() ) )
            {
                connectors.Add( new ListItemBag
                {
                    Text = RequestContext.CurrentPerson.FullName,
                    Value = currentPersonAliasGuid.Value.ToString()
                } );
            }

            options.AllPossibleConnectors = connectors;
            options.ConnectionOpportunities = connectionType.ConnectionOpportunities.Where( o => o.IsActive ).ToListItemBagList();
            options.ConnectionStates = typeof( ConnectionState ).ToEnumListItemBag()
                .Where( i => !ignoredConnectionStates.Contains( ( ConnectionState ) i.Value.AsInteger() ) )
                .ToList();
            options.RequestSourceItems = connectionType.ConnectionTypeSources.ToListItemBagList();
            options.IsFutureFollowUpEnabled = connectionType.EnableFutureFollowup;
            options.IsRequestSecurityEnabled = connectionType.EnableRequestSecurity;
            options.AreCelebrationsEnabled = connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Celebration );
            options.AreRemindersEnabled = connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Reminder );
            options.AreGroupPlacementsEnabled = connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.GroupPlacement );

            var delimitedBadgeGuids = GetAttributeValue( AttributeKey.Badges );
            options.BadgeGuids = delimitedBadgeGuids.SplitDelimitedValues().AsGuidList();

            var manualWorkflows = new List<ConnectionWorkflow>();
            var authorizedWorkflowItems = new List<ListItemBag>();

            manualWorkflows.AddRange( connectionType.ConnectionWorkflows
                .Where( w => w.TriggerType == ConnectionWorkflowTriggerType.Manual && ( w.WorkflowType.IsActive ?? true ) ) // Mirroring Webforms by setting IsActive to true by default.
                .ToList() );

            manualWorkflows.AddRange( connectionType.ConnectionOpportunities
                .SelectMany( o => o.ConnectionWorkflows )
                .Where( w => w.TriggerType == ConnectionWorkflowTriggerType.Manual && ( w.WorkflowType.IsActive ?? true ) ) // Mirroring Webforms by setting IsActive to true by default.
                .ToList()
            );

            foreach ( var manualWorkflow in manualWorkflows )
            {
                if ( manualWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    authorizedWorkflowItems.Add( new ListItemBag
                    {
                        Text = manualWorkflow.WorkflowType.Name,
                        Value = manualWorkflow.Guid.ToString(),
                        Category = manualWorkflow.ConnectionTypeId.HasValue ? "Connection Type Workflows" : "Connection Opportunity Workflows"
                    } );
                }
            }

            options.WorkflowItems = authorizedWorkflowItems;

            options.ConnectionStatuses = connectionType.ConnectionStatuses.Where( s => s.IsActive )
                .Select( s => new ConnectionStatusBag
                {
                    Guid = s.Guid,
                    Name = s.Name,
                    HighlightColor = s.HighlightColor,
                    Order = s.Order,
                    IsNoteRequiredOnCompletion = s.IsNoteRequiredOnCompletion,
                    IsDefaultStatus = s.IsDefault
                } )
                .OrderBy( s => s.Order )
                .ToList();

            var tempConnectionRequest = new ConnectionRequest
            {
                ConnectionTypeId = connectionType.Id
            };

            tempConnectionRequest.LoadAttributes();

            options.ConnectionTypeRequestAttributes = tempConnectionRequest.GetPublicAttributesForEdit( RequestContext.CurrentPerson );

            // The values should equal the field names for each respective column.
            options.GridDataToShowItems = new List<ListItemBag>
            {
                new ListItemBag
                {
                    Text = "Due Date",
                    Value = "dueDate"
                },
                new ListItemBag
                {
                    Text = "Opportunity",
                    Value = "connectionOpportunity"
                },
                new ListItemBag
                {
                    Text = "Activity Count / Days",
                    Value = "activity"
                }
            };

            options.GridDataToShowItems.AddRange(
                tempConnectionRequest.Attributes
                    .Select( a => a.Value )
                    .Where( a => a.IsGridColumn )
                    .Select( a =>
                    {
                        var item = a.ToListItemBag();
                        // Overwrite the value with the field key that will be populated in the grid.
                        item.Value = $"attr_{a.Key}";
                        return item;
                    } )
            );

            options.ConnectionActivities = connectionType.ConnectionActivityTypes.Where( at => at.IsActive )
                .Select( a => new ConnectionActivityTypeBag
                {
                    ActivityType = a.ToListItemBag(),
                    PersonNoteCreationBehavior = a.PersonNoteCreationBehavior
                } ).ToList();

            return options;
        }

        #region Helper Methods


        /// <summary>
        /// Gets the appropriate authorization target for the specified Connection Opportunity
        /// based on whether Request-level security is enabled on the Connection Type.
        /// </summary>
        /// <param name="connectionType">The Connection Type to check for Request Security configuration.</param>
        /// <param name="connectionOpportunity">The Connection Opportunity to get the authorization target for.</param>
        /// <returns>
        /// A <see cref="ConnectionRequest"/> if <see cref="ConnectionType.EnableRequestSecurity"/> is enabled;
        /// otherwise the <see cref="ConnectionOpportunity"/> itself.
        /// </returns>
        private ISecured GetAuthorizationTarget( ConnectionType connectionType, ConnectionOpportunity connectionOpportunity )
        {
            if ( connectionType.EnableRequestSecurity )
            {
                return new ConnectionRequest
                {
                    ConnectionTypeId = connectionType.Id,
                    ConnectionOpportunityId = connectionOpportunity.Id,
                    ConnectionOpportunity = connectionOpportunity
                };
            }

            return connectionOpportunity;
        }

        /// <summary>
        /// Gets the Connection Opportunity Filter from the Person Preference
        /// </summary>
        /// <param name="connectionTypeIdKey">The conneciton type that the preference is set on</param>
        /// <returns>A nullable Connection Opportunity Guid</returns>
        private Guid? GetConnectionOpportunityFilter( string connectionTypeIdKey )
        {
            var preferences = GetBlockPersonPreferences();

            return preferences.GetValue( string.Format( PreferenceKey.ConnectionmOpportunityFilterConnectionTypeIdKey, connectionTypeIdKey ) ).AsGuidOrNull();
        }

        /// <summary>
        /// Gets the Connection State Filter from the Person Preference. The values are stored
        /// as a JSON array of integers corresponding to the <see cref="ConnectionState"/> enum.
        /// </summary>
        /// <param name="connectionTypeIdKey">The connection type that the preference is set on.</param>
        /// <returns>A list of <see cref="ConnectionState"/> values to filter by, or an empty list if no filter is set.</returns>
        private List<ConnectionState> GetStateFilter( string connectionTypeIdKey )
        {
            var preferences = GetBlockPersonPreferences();

            return preferences.GetValue( string.Format( PreferenceKey.FilterStateConnectionTypeIdKey, connectionTypeIdKey ) )
                .FromJsonOrNull<List<int>>()
                ?.Select( i => ( ConnectionState ) i )
                .ToList() ?? new List<ConnectionState>();
        }

        /// <summary>
        /// Gets a Grouping Field Bag for the specified entity. If the ID is null,
        /// returns an "Unassigned" bag with a default no-picture URL for person types.
        /// </summary>
        /// <param name="id">The ID of the entity to create the bag for. If null, an unassigned bag is returned.</param>
        /// <param name="type">The type of the grouping field (e.g. "person", "group").</param>
        /// <param name="label">The display label for the grouping field.</param>
        /// <param name="order">The optional sort order of the grouping field.</param>
        /// <param name="iconCssClass">The optional CSS class for the icon associated with the grouping field.</param>
        /// <param name="photoUrl">The optional URL of the photo for the grouping field. Overridden with a default no-picture URL for unassigned persons.</param>
        /// <param name="textColorCssClass">The optional CSS class used to set the text color of the grouping field.</param>
        /// <returns>A <see cref="GroupingFieldBag"/> populated with either the entity's details or unassigned defaults.</returns>
        private GroupingFieldBag GetGroupingFieldBag( int? id, string type, string label, int? order = null, string iconCssClass = null, string photoUrl = null, string textColorCssClass = null )
        {
            if ( !id.HasValue )
            {
                if ( type == "person" )
                {
                    photoUrl = Rock.Model.Person.GetPersonNoPictureUrl( new Rock.Model.Person() );
                }

                var unassignedBag = new GroupingFieldBag
                {
                    Key = "unassigned",
                    Type = type,
                    Label = "Unassigned",
                    PhotoUrl = photoUrl,
                    Order = order,
                    TextColorCssClass = textColorCssClass
                };

                return unassignedBag;
            }

            var groupingKey = IdHasher.Instance.GetHash( id.Value );

            var assignedBag = new GroupingFieldBag
            {
                Key = groupingKey,
                Type = type,
                Label = label,
                IconCssClass = iconCssClass,
                PhotoUrl = photoUrl,
                Order = order,
                TextColorCssClass = textColorCssClass
            };

            return assignedBag;
        }

        /// <summary>
        /// Gets the text color CSS class corresponding to the given due status.
        /// </summary>
        /// <param name="dueStatus">The due status to get the text color CSS class for.</param>
        /// <returns>A CSS class string representing the text color for the given due status.</returns>
        private string GetDueStatusTextColorCssClass( DueStatus dueStatus )
        {
            switch ( dueStatus )
            {
                case DueStatus.DueLater:
                    return "text-interface-strong";
                case DueStatus.DueSoon:
                    return "text-warning-strong";
                case DueStatus.Overdue:
                    return "text-danger-strong";
                default:
                    return "text-interface-strong";
            }
        }

        /// <summary>
        /// Gets the icon CSS class corresponding to the given connection state.
        /// </summary>
        /// <param name="state">The connection state to get the icon CSS class for.</param>
        /// <returns>A CSS class string representing the icon for the given connection state.</returns>
        private string GetStateIconCssClass( ConnectionState state )
        {
            switch ( state )
            {
                case ConnectionState.Active:
                    return "ti ti-bolt";
                case ConnectionState.Inactive:
                    return "ti ti-bolt-off";
                case ConnectionState.FutureFollowUp:
                    return "ti ti-calendar-clock";
                case ConnectionState.Connected:
                    return "ti ti-circle-check-filled";
                default:
                    return "ti ti-bolt";
            }
        }

        /// <summary>
        /// Determines the due status of a request based on its due date and optional due-soon threshold date.
        /// Returns <see cref="DueStatus.Overdue"/> if past due, <see cref="DueStatus.DueSoon"/> if within
        /// the due-soon window, and <see cref="DueStatus.DueLater"/> if not yet approaching or if no due date is set.
        /// </summary>
        /// <param name="dueDate">The date the item is due. If null, the status defaults to DueLater.</param>
        /// <param name="dueSoonDate">The optional date from which the item is considered due soon. If null, DueSoon will never be returned.</param>
        /// <returns>A <see cref="DueStatus"/> indicating whether the item is overdue, due soon, or due later.</returns>
        private DueStatus GetDueStatus( DateTime? dueDate, DateTime? dueSoonDate, ConnectionState connectionState, DateTime? completedDateTime )
        {
            var now = RockDateTime.Now.Date;

            if ( !dueDate.HasValue || connectionState == ConnectionState.Inactive )
            {
                return DueStatus.DueLater;
            }

            var due = dueDate.Value.Date;

            if ( connectionState == ConnectionState.Connected )
            {
                if ( !completedDateTime.HasValue )
                {
                    return DueStatus.DueLater;
                }

                var completedDate = completedDateTime.Value.Date;

                if ( completedDate > due )
                {
                    return DueStatus.Overdue;
                }

                return DueStatus.DueLater;
            }

            if ( now > due )
            {
                return DueStatus.Overdue;
            }

            if ( dueSoonDate.HasValue && now >= dueSoonDate.Value.Date )
            {
                return DueStatus.DueSoon;
            }

            return DueStatus.DueLater;
        }

        /// <summary>
        /// Gets the Connection Type Cache from the current page parameters.
        /// If a Connection Type IdKey is provided directly, it is used first. Otherwise,
        /// the Connection Opportunity page parameter is resolved to derive the Connection Type.
        /// Falls back to the Connection Type page parameter if no opportunity is found.
        /// </summary>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey to look up directly, bypassing page parameter resolution.</param>
        /// <returns>The <see cref="ConnectionTypeCache"/> resolved from the available page parameters, or null if none could be found.</returns>
        private ConnectionTypeCache GetConnectionTypeCacheFromPageParameters( string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType;

            if ( connectionTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                connectionType = ConnectionTypeCache.Get( connectionTypeIdKey, !PageCache.Layout.Site.DisablePredictableIds );
                return connectionType;
            }

            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( PageParameter( PageParameterKey.ConnectionOpportunity ), !PageCache.Layout.Site.DisablePredictableIds );

            if ( connectionOpportunity != null )
            {
                connectionType = ConnectionTypeCache.Get( connectionOpportunity.ConnectionTypeId );
            }
            else
            {
                connectionType = ConnectionTypeCache.Get( PageParameter( PageParameterKey.ConnectionType ), !PageCache.Layout.Site.DisablePredictableIds );
            }

            return connectionType;
        }

        /// <summary>
        /// Strips a trailing bracketed numeric ID from a string, returning the clean text value.
        /// </summary>
        /// <remarks>
        /// Used when processing History values for the Connection Request activity feed, where
        /// entity references are stored with a trailing bracketed ID (e.g. "In Progress [5]").
        /// This method removes that suffix so only the human-readable label is displayed.
        /// </remarks>
        /// <param name="value">The string to strip the bracketed ID from.</param>
        /// <returns>The string with the trailing bracketed ID removed, or null if the input is null or whitespace.</returns>
        private string StripBracketId( string value )
        {
            return value.IsNullOrWhiteSpace()
                ? null
                : Regex.Replace( value, @"\s*\[\d+\]\s*$", "" );
        }

        #endregion Helper Methods

        #region Connection Workflow Methods

        /// <summary>
        /// Gets the list of entity IDs returned by the specified Data View.
        /// If the Data View is persisted and has been refreshed, the persisted values are used
        /// for performance; otherwise the Data View query is executed directly with a 30-second timeout.
        /// Returns an empty list if the Data View cannot be found.
        /// </summary>
        /// <remarks>
        /// This method is intended for use with the include and exclude Data Views associated
        /// with connection workflows, to determine which Connection Requests should be included
        /// in or excluded from workflow processing.
        /// </remarks>
        /// <param name="dataViewId">The ID of the Data View to retrieve entity IDs from.</param>
        /// <returns>A list of entity IDs returned by the Data View, or an empty list if the Data View could not be found.</returns>
        private List<int> GetDataViewValues( int dataViewId )
        {
            var dataView = DataViewCache.Get( dataViewId );
            if ( dataView == null )
            {
                return new List<int>();
            }

            if ( dataView.IsPersisted() && dataView.PersistedLastRefreshDateTime.HasValue )
            {
                return RockContext.Set<DataViewPersistedValue>().Select( e => e.EntityId ).ToList();
            }
            else
            {
                var dataViewGetQueryArgs = new Rock.Reporting.GetQueryableOptions { DbContext = RockContext, DatabaseTimeoutSeconds = 30 };
                return dataView.GetQuery( dataViewGetQueryArgs ).Select( e => e.Id ).ToList();
            }
        }

        /// <summary>
        /// Determines whether a Connection Request is eligible to trigger the given Connection Workflow.
        /// Checks the workflow's manual trigger status filter, age classification filter,
        /// and include/exclude Data View membership of the requester.
        /// </summary>
        /// <param name="cw">The Connection Workflow to evaluate eligibility for.</param>
        /// <param name="request">The Connection Request being evaluated.</param>
        /// <param name="includeIds">An optional list of Person IDs from the include Data View. If provided, the requester must be in this list to be eligible.</param>
        /// <param name="excludeIds">An optional list of Person IDs from the exclude Data View. If provided, the requester must not be in this list to be eligible.</param>
        /// <returns>True if the Connection Request meets all criteria for the Connection Workflow; otherwise false.</returns>
        private bool IsEligibleForWorkflow( ConnectionWorkflow cw, ConnectionRequest request, List<int> includeIds, List<int> excludeIds )
        {
            if ( cw.ManualTriggerFilterConnectionStatusId.HasValue && cw.ManualTriggerFilterConnectionStatusId != request.ConnectionStatusId )
            {
                return false;
            }

            var person = request.PersonAlias?.Person;
            if ( person == null )
            {
                return false;
            }

            if ( cw.AppliesToAgeClassification == AppliesToAgeClassification.Adults && person.AgeClassification != AgeClassification.Adult )
            {
                return false;
            }

            if ( cw.AppliesToAgeClassification == AppliesToAgeClassification.Children && person.AgeClassification != AgeClassification.Child )
            {
                return false;
            }

            if ( includeIds != null && !includeIds.Contains( person.Id ) )
            {
                return false;
            }

            if ( excludeIds != null && excludeIds.Contains( person.Id ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Launches workflows for a list of eligible Connection Requests in a background thread,
        /// activating and processing the specified workflow type for each request. Persisted workflows
        /// are recorded as Connection Request Workflows with the trigger type and qualifier.
        /// Each request is processed in its own Rock Context to ensure proper isolation,
        /// and any per-request exceptions are logged without interrupting the remaining requests.
        /// </summary>
        /// <param name="eligibleRequests">The list of Connection Requests to launch the workflow for.</param>
        /// <param name="connectionWorkflow">The Connection Workflow definition providing the trigger type, qualifier value, and workflow type.</param>
        /// <param name="workflowType">The Workflow Type Cache entry used to activate each workflow instance.</param>
        private void LaunchWorkflowsInBackground( List<ConnectionRequest> eligibleRequests, ConnectionWorkflow connectionWorkflow, WorkflowTypeCache workflowType )
        {
            var requestIds = eligibleRequests.Select( r => r.Id ).ToList();
            var connectionWorkflowId = connectionWorkflow.Id;
            var workflowTypeId = workflowType.Id;
            var triggerType = connectionWorkflow.TriggerType;
            var qualifierValue = connectionWorkflow.QualifierValue;
            var workTerm = connectionWorkflow.WorkflowType.WorkTerm;

            Task.Run( () =>
            {
                foreach ( var requestId in requestIds )
                {
                    try
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var request = new ConnectionRequestService( rockContext ).Get( requestId );
                            var wfType = WorkflowTypeCache.Get( workflowTypeId );
                            var workflow = Rock.Model.Workflow.Activate( wfType, workTerm, rockContext );

                            if ( workflow == null )
                            {
                                continue;
                            }

                            var bgWorkflowService = new WorkflowService( rockContext );

                            if ( !bgWorkflowService.Process( workflow, request, out _ ) )
                            {
                                continue;
                            }

                            if ( workflow.Id != 0 )
                            {
                                new ConnectionRequestWorkflowService( rockContext ).Add( new ConnectionRequestWorkflow
                                {
                                    ConnectionRequestId = requestId,
                                    WorkflowId = workflow.Id,
                                    ConnectionWorkflowId = connectionWorkflowId,
                                    TriggerType = triggerType,
                                    TriggerQualifier = qualifierValue
                                } );
                            }

                            rockContext.SaveChanges();
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                    }
                }
            } );
        }

        #endregion Connection Workflow Methods

        #region Placement Group Methods

        /// <summary>
        /// Serializes the Placement Group Member Attribute values into a JSON string and returns it
        /// </summary>
        /// <returns>A JSON string of the Placement Group Member Attribute Values</returns>
        private string GetGroupMemberAttributeValuesFromBag( Dictionary<string, string> attributeValues, int? groupId, int? groupMemberRoleId, GroupMemberStatus? groupMemberStatus )
        {
            var values = new Dictionary<string, string>();

            if ( !groupId.HasValue || !groupMemberRoleId.HasValue || !groupMemberStatus.HasValue )
            {
                return string.Empty;
            }

            var groupMember = new Rock.Model.GroupMember
            {
                GroupId = groupId.Value,
                GroupRoleId = groupMemberRoleId.Value,
                GroupMemberStatus = groupMemberStatus.Value
            };

            groupMember.LoadAttributes();
            groupMember.SetPublicAttributeValues( attributeValues, RequestContext.CurrentPerson );

            foreach ( var attrValue in groupMember.AttributeValues )
            {
                values.Add( attrValue.Key, attrValue.Value.Value );
            }

            return JsonConvert.SerializeObject( values, Formatting.None );
        }

        /// <summary>
        /// Attempts to place the requester into the assigned placement group for the given Connection Request.
        /// Handles new group member creation, restoration of archived members, group requirement validation,
        /// and assignment of any saved group member attribute values.
        /// </summary>
        /// <remarks>
        /// This method is called whenever a Connection Request moves to a Connected (completed) state and
        /// has an assigned placement group, role, and status. If any of those three assignments are missing,
        /// the method returns true without attempting placement.
        /// </remarks>
        /// <param name="connectionRequest">The Connection Request containing the placement group, role, status, and attribute value assignments.</param>
        /// <param name="error">When this method returns, contains a Block Action Result error if any group requirements were not met; otherwise null.</param>
        /// <param name="groupMemberRequirements">An optional list of manually checked group member requirement states passed in from the client, used to validate manual group requirements.</param>
        /// <returns>True if placement was successful or no placement was needed; false if any group requirements were not met.</returns>
        private bool TryAssignPlacementGroup( ConnectionRequest connectionRequest, out BlockActionResult error, List<GroupMemberRequirementBag> groupMemberRequirements = null )
        {
            error = null;
            var group = connectionRequest.AssignedGroup;

            // Only attempt group member placement if the request has an assigned placement group, role, and status.
            if ( !connectionRequest.AssignedGroupId.HasValue ||
                !connectionRequest.AssignedGroupMemberRoleId.HasValue ||
                !connectionRequest.AssignedGroupMemberStatus.HasValue ||
                group == null )
            {
                return true;
            }

            var groupService = new GroupService( RockContext );
            var groupMemberService = new GroupMemberService( RockContext );

            // Does this person already exist in this group with the same role?
            var groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                connectionRequest.AssignedGroupId.Value,
                connectionRequest.PersonAlias.PersonId,
                connectionRequest.AssignedGroupMemberRoleId.Value );

            if ( groupMember == null )
            {
                // Double-check to make sure they weren't previously archived; if so, restore them.
                if ( groupService.ExistsAsArchived( group, connectionRequest.PersonAlias.PersonId, connectionRequest.AssignedGroupMemberRoleId.Value, out groupMember ) )
                {
                    groupMemberService.Restore( groupMember );
                }
                else
                {
                    // If we still don't have a group member, create a new one.
                    groupMember = new GroupMember();
                    groupMember.PersonId = connectionRequest.PersonAlias.PersonId;
                    groupMember.GroupId = connectionRequest.AssignedGroupId.Value;
                    groupMember.GroupRoleId = connectionRequest.AssignedGroupMemberRoleId.Value;
                }
            }

            // Always set the assigned status, for both new and preexisting members.
            groupMember.GroupMemberStatus = connectionRequest.AssignedGroupMemberStatus.Value;

            var requirementsToCheck = connectionRequest.AssignedGroup.GetGroupRequirements( RockContext ).Where( gr => gr.MustMeetRequirementToAddMember ).ToList();
            var manualRequirements = requirementsToCheck.Where( gr => gr.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual );
            var nonManualRequirements = requirementsToCheck.Where( gr => gr.GroupRequirementType.RequirementCheckType != RequirementCheckType.Manual );

            var meetsAllNonManualRequirements = nonManualRequirements.All( gr =>
            {
                var status = gr.PersonMeetsGroupRequirement( RockContext, connectionRequest.PersonAlias.PersonId, connectionRequest.AssignedGroupId.Value, connectionRequest.AssignedGroupMemberRoleId );
                return status.MeetsGroupRequirement == MeetsGroupRequirement.Meets
                    || status.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning
                    || status.MeetsGroupRequirement == MeetsGroupRequirement.NotApplicable;
            } );

            if ( !meetsAllNonManualRequirements )
            {
                error = ActionBadRequest( "Group Requirements have not been met. Please verify all of the requirements." );
                return false;
            }

            // Ensure this person meets any manual group requirements (driven by checkboxes passed in from the client).
            foreach ( var manualRequirement in manualRequirements )
            {
                var bagEntry = groupMemberRequirements?.FirstOrDefault( r => r.GroupRequirementIdKey == manualRequirement.IdKey );
                var isMet = bagEntry != null
                    && ( bagEntry.GroupMemberRequirementState == MeetsGroupRequirement.Meets
                        || bagEntry.GroupMemberRequirementState == MeetsGroupRequirement.MeetsWithWarning );

                if ( !isMet && manualRequirement.MustMeetRequirementToAddMember )
                {
                    // Check if there is already an existing record of the person meeting the manual requirement.
                    var existingGroupRequirementStatus = manualRequirement.PersonMeetsGroupRequirement( RockContext, connectionRequest.PersonAlias.PersonId, connectionRequest.AssignedGroupId.Value, connectionRequest.AssignedGroupMemberRoleId );
                    if ( existingGroupRequirementStatus.MeetsGroupRequirement == MeetsGroupRequirement.Meets
                        || existingGroupRequirementStatus.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning
                        || existingGroupRequirementStatus.MeetsGroupRequirement == MeetsGroupRequirement.NotApplicable )
                    {
                        continue;
                    }

                    error = ActionBadRequest( "Group Requirements have not been met. Please verify all of the requirements." );
                    return false;
                }

                if ( isMet )
                {
                    var groupMemberRequirement = groupMember.GroupMemberRequirements.FirstOrDefault( r => r.GroupRequirementId == manualRequirement.Id )
                        ?? new GroupMemberRequirement
                        {
                            GroupRequirementId = manualRequirement.Id
                        };

                    groupMemberRequirement.RequirementMetDateTime = groupMemberRequirement.RequirementMetDateTime ?? RockDateTime.Now;
                    groupMemberRequirement.LastRequirementCheckDateTime = RockDateTime.Now;

                    if ( groupMemberRequirement.Id == 0 )
                    {
                        groupMember.GroupMemberRequirements.Add( groupMemberRequirement );
                    }
                }
            }

            if ( groupMember.Id == 0 )
            {
                groupMemberService.Add( groupMember );
            }

            if ( !string.IsNullOrWhiteSpace( connectionRequest.AssignedGroupMemberAttributeValues ) )
            {
                var savedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( connectionRequest.AssignedGroupMemberAttributeValues );
                if ( savedValues != null )
                {
                    groupMember.LoadAttributes();
                    foreach ( var kvp in savedValues )
                    {
                        groupMember.SetAttributeValue( kvp.Key, kvp.Value );
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the IDs of Connection Requests whose requesters do not meet the mandatory requirements
        /// of their assigned placement group. Only evaluates requests that are not yet in a Connected
        /// state and have an assigned placement group.
        /// </summary>
        /// <param name="connectionRequestIds">The list of Connection Request IDs to evaluate against their assigned placement group requirements.</param>
        /// <returns>A list of Connection Request IDs where the requester fails to meet one or more mandatory group requirements.</returns>
        private List<int> GetConnectionRequestIdsNotMeetingGroupRequirements( List<int> connectionRequestIds )
        {
            var requestsToCheck = new ConnectionRequestService( RockContext ).Queryable()
                .Where( cr => connectionRequestIds.Contains( cr.Id )
                    && cr.ConnectionState != ConnectionState.Connected
                    && cr.AssignedGroup != null )
                .Select( cr => new
                {
                    cr.Id,
                    cr.AssignedGroup,
                    cr.AssignedGroupMemberRoleId,
                    cr.PersonAlias.PersonId,
                    GroupRequirements = cr.AssignedGroup.GroupRequirements
                        .Where( gr => gr.MustMeetRequirementToAddMember )
                        .ToList()
                } )
                .ToList();

            var idsNotMeeting = new List<int>();

            foreach ( var request in requestsToCheck )
            {
                var meetsAllRequirements = request.GroupRequirements.All( gr =>
                {
                    var status = gr.PersonMeetsGroupRequirement( RockContext, request.PersonId, request.AssignedGroup.Id, request.AssignedGroupMemberRoleId );
                    return status.MeetsGroupRequirement == MeetsGroupRequirement.Meets
                        || status.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning
                        || status.MeetsGroupRequirement == MeetsGroupRequirement.NotApplicable;
                } );

                if ( !meetsAllRequirements )
                {
                    idsNotMeeting.Add( request.Id );
                }
            }

            return idsNotMeeting;
        }

        /// <summary>
        /// Determines whether the requester of a single Connection Request meets all mandatory
        /// requirements of their assigned placement group.
        /// </summary>
        /// <param name="connectionRequestId">The ID of the Connection Request to evaluate.</param>
        /// <returns>True if the requester meets all mandatory group requirements or no evaluation was needed; otherwise false.</returns>
        private bool ConnectionRequestMeetsGroupRequirements( int connectionRequestId )
        {
            return !GetConnectionRequestIdsNotMeetingGroupRequirements( new List<int> { connectionRequestId } ).Any();
        }

        #endregion Placement Group Methods

        /// <summary>
        /// Gets the Connection Opportunity Detail Bag for the specified Connection Opportunity.
        /// </summary>
        /// <remarks>
        /// This method is used to fetch Connection Opportunity specific details needed by the client. It is currently
        /// called when fetching block options if a Connection Opportunity filter is set, and when fetching Opportunity
        /// details upon selection in the Add Connection Request modal.
        /// If no <paramref name="campusId"/> is provided, the method will attempt to resolve one from the current campus context.
        /// </remarks>
        /// <param name="connectionOpportunity">The Connection Opportunity to build the bag from.</param>
        /// <param name="campusId">The optional campus ID to filter results by. If not provided, the campus context will be used if available.</param>
        /// <returns>A <see cref="ConnectionOpportunityDetailBag"/> populated with the relevant details.</returns>
        private ConnectionOpportunityDetailBag GetConnectionOpportunityDetailBag( ConnectionOpportunity connectionOpportunity, int? campusId = null )
        {
            var campusContext = RequestContext.GetContextEntity<Campus>();
            var campusContextId = campusContext?.Id;

            // If a campus Id was not passed in then get one from the campus context.
            if ( !campusId.HasValue )
            {
                campusId = campusContextId;
            }

            var campusItems = connectionOpportunity.ConnectionOpportunityCampuses.Where( c => c.Campus != null && c.Campus.IsActive == true )
                .Select( c => c.Campus )
                .ToListItemBagList();

            var connectorOptionsBag = GetConnectorOptionsBag( connectionOpportunity, campusId );

            if ( campusContextId.HasValue )
            {
                // Add the selected campus context to the campus list if it is not already included.
                var campus = CampusCache.Get( campusContextId.Value );
                if ( campus != null && !campusItems.Any( c => c.Value == campus.Guid.ToString() ) )
                {
                    campusItems.Add( new ListItemBag
                    {
                        Text = campus.Name,
                        Value = campus.Guid.ToString()
                    } );
                }
            }

            var placementGroups = GetPlacementGroupItems( connectionOpportunity, campusId );

            var tempConnectionRequest = new ConnectionRequest
            {
                ConnectionOpportunityId = connectionOpportunity.Id
            };

            tempConnectionRequest.LoadAttributes();

            return new ConnectionOpportunityDetailBag
            {
                IdKey = IdHasher.Instance.GetHash( connectionOpportunity.Id ),
                ConnectorOptions = connectorOptionsBag,
                PlacementGroups = placementGroups,
                Campuses = campusItems,
                ConnectionOpportunityRequestAttributes = tempConnectionRequest.GetPublicAttributesForEdit( RequestContext.CurrentPerson )
            };
        }

        /// <summary>
        /// Builds the list of available connectors for the specified Connection Opportunity,
        /// optionally filtered by campus. Always includes the current logged-in person and,
        /// if provided, the currently assigned connector — even if they fall outside the campus filter.
        /// </summary>
        /// <param name="connectionOpportunity">The Connection Opportunity to retrieve connectors for.</param>
        /// <param name="campusId">The optional campus ID to filter connectors by. If not provided, all connectors are returned.</param>
        /// <param name="currentConnector">The optional currently assigned connector to ensure is present in the list.</param>
        /// <param name="addUnassignedConnector">If <c>true</c>, prepends an "Unassigned" option to the list.</param>
        /// <returns>A list of <see cref="ConnectorItemBag"/> representing the available connectors, including photo URLs.</returns>
        private List<ConnectorItemBag> GetConnectorItems( int connectionOpportunityId, int? campusId, Rock.Model.PersonAlias currentConnector = null, bool addUnassignedConnector = false )
        {
            Guid? selectedCampusGuid = null;
            if ( campusId.HasValue )
            {
                selectedCampusGuid = CampusCache.Get( campusId.Value )?.Guid;
            }

            // Apply campus filtering at the database level. A null campus on the connector group
            // means the connector is available to all campuses and is always included.
            var connectorGroupQuery = new ConnectionOpportunityConnectorGroupService( RockContext ).Queryable()
                .Where( g => g.ConnectionOpportunityId == connectionOpportunityId );

            if ( selectedCampusGuid.HasValue )
            {
                connectorGroupQuery = connectorGroupQuery
                    .Where( g => g.Campus == null || g.Campus.Guid == selectedCampusGuid.Value );
            }

            // Project one flat row per group member so the database returns a single result set.
            var memberRows = connectorGroupQuery
                .SelectMany( g => g.ConnectorGroup.Members
                    .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( m => new
                    {
                        m.Person,
                        CampusGuid = ( Guid? ) g.Campus.Guid
                    } ) )
                .ToList();

            // Ensure the current logged-in person appears in the list even if outside the campus filter.
            var currentPerson = RequestContext.CurrentPerson;
            if ( currentPerson?.PrimaryAlias != null && !memberRows.Any( r => r.Person.PrimaryAliasGuid == currentPerson.PrimaryAlias.Guid ) )
            {
                memberRows.Add( new { Person = currentPerson, CampusGuid = ( Guid? ) null } );
            }

            // Ensure the currently assigned connector appears in the list even if outside the campus filter.
            if ( currentConnector?.Person != null && !memberRows.Any( r => r.Person.PrimaryAliasGuid == currentConnector.Person.PrimaryAlias?.Guid ) )
            {
                memberRows.Add( new { Person = currentConnector.Person, CampusGuid = ( Guid? ) null } );
            }

            // Group by person to deduplicate connectors    who belong to multiple connector groups,
            // and to aggregate the campuses each connector is assigned to.
            var connectors = memberRows
                .GroupBy( r => r.Person.PrimaryAliasGuid )
                .OrderBy( g => g.First().Person.LastName )
                .ThenBy( g => g.First().Person.FirstName )
                .Select( g => new ConnectorItemBag
                {
                    ListItemBag = new ListItemBag
                    {
                        Value = g.Key.ToString(),
                        Text = g.First().Person.FullName
                    },
                    PhotoUrl = g.First().Person.PhotoUrl,
                    IsAvailableToAllCampuses = g.Any( r => !r.CampusGuid.HasValue ),
                    CampusGuids = g.Where( r => r.CampusGuid.HasValue )
                                  .Select( r => r.CampusGuid.Value )
                                  .Distinct()
                                  .ToList()
                } )
                .ToList();

            // Add Unassigned Connector to the beggining of the list.
            if ( addUnassignedConnector )
            {
                connectors.Insert( 0, new ConnectorItemBag
                {
                    ListItemBag = new ListItemBag
                    {
                        Text = "Unassigned",
                        Value = "unassigned"
                    },
                    PhotoUrl = Rock.Model.Person.GetPersonNoPictureUrl( new Rock.Model.Person() ),
                    IsAvailableToAllCampuses = true
                } );
            }

            return connectors;
        }

        /// <summary>
        /// Builds a <see cref="ConnectorOptionsBag"/> containing the available connectors and default connector
        /// for the specified Connection Opportunity, optionally filtered by campus.
        /// </summary>
        /// <param name="connectionOpportunity">The Connection Opportunity to retrieve connector options for.</param>
        /// <param name="campusId">The optional campus ID to filter connectors by. If not provided, all connectors are returned.</param>
        /// <returns>A <see cref="ConnectorOptionsBag"/> containing the connector list and default connector GUID.</returns>
        private ConnectorOptionsBag GetConnectorOptionsBag( ConnectionOpportunity connectionOpportunity, int? campusId )
        {
            var connectors = GetConnectorItems( connectionOpportunity.Id, campusId ).Select( c => c.ListItemBag ).ToList();

            string defaultConnectorPersonAliasGuid = string.Empty;

            if ( campusId.HasValue )
            {
                var defaultConnector = connectionOpportunity.ConnectionOpportunityCampuses.Where( c => c.CampusId == campusId.Value && c.DefaultConnectorPersonAliasId.HasValue )
                    .Select( c => c.DefaultConnectorPersonAlias )
                    .FirstOrDefault();

                if ( defaultConnector != null )
                {
                    defaultConnectorPersonAliasGuid = defaultConnector.Guid.ToString();
                }
            }

            var bag = new ConnectorOptionsBag
            {
                DefaultConnectorPersonAliasGuid = defaultConnectorPersonAliasGuid,
                Connectors = connectors
            };

            return bag;
        }

        /// <summary>
        /// Gets the available placement groups for the specified Connection Opportunity,
        /// optionally filtered by campus. Returns null if the Group Placement feature is not enabled for the Connection Type.
        /// </summary>
        /// <param name="connectionOpportunity">The Connection Opportunity to retrieve placement groups for.</param>
        /// <param name="campusId">The optional campus ID to filter placement groups by. If not provided, all placement groups are returned.</param>
        /// <returns>A list of <see cref="ListItemBag"/> representing the available placement groups, or null if Group Placement is not enabled.</returns>
        private List<ListItemBag> GetPlacementGroupItems( ConnectionOpportunity connectionOpportunity, int? campusId )
        {
            var connectionType = ConnectionTypeCache.Get( connectionOpportunity.ConnectionTypeId );

            if ( !connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.GroupPlacement ) )
            {
                return null;
            }

            var placementGroups = connectionOpportunity.ConnectionOpportunityGroups.Select( g => g.Group )
                .Where( g => !campusId.HasValue || !g.CampusId.HasValue || g.CampusId.Value == campusId.Value )
                .Select( g => new ListItemBag
                {
                    Text = g.CampusId.HasValue ? $"{g.Name} ({g.Campus.Name})" : $"{g.Name} (No Campus)",
                    Value = g.Guid.ToString()
                } )
                .ToList();

            return placementGroups;
        }

        /// <summary>
        /// Gets the Connection Request Bag for Edit Mode
        /// </summary>
        /// <param name="entity">The Connection Request entity</param>
        /// <returns>The Connection Request Bag</returns>
        private ConnectionRequestBag GetEntityBagForEdit( ConnectionRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new ConnectionRequestBag
            {
                IdKey = entity.IdKey,
                Requester = new ListItemBag
                {
                    Text = entity.PersonAlias.Person.FullName,
                    Value = entity.PersonAlias.Guid.ToString(),
                },
                CampusGuid = entity.Campus?.Guid.ToString(),
                ConnectorPersonAliasGuid = entity.ConnectorPersonAlias?.Guid.ToString(),
                ConnectionState = entity.ConnectionState,
                FollowUpDate = entity.FollowupDate?.ToRockDateTimeOffset(),
                ConnectionStatusGuid = entity.ConnectionStatus?.Guid.ToString(),
                Comments = entity.Comments,
                RequestSourceGuid = entity.ConnectionTypeSource?.Guid.ToString()
            };

            var currentPerson = RequestContext.CurrentPerson;

            if ( entity.AssignedGroupMemberRoleId.HasValue && entity.AssignedGroupId.HasValue )
            {
                var role = new GroupTypeRoleService( RockContext ).Get( entity.AssignedGroupMemberRoleId.Value );
                bag.GroupMemberRoleGuid = role?.Guid.ToString();
                bag.PlacementGroupGuid = entity.AssignedGroup?.Guid.ToString();
                bag.GroupMemberStatus = entity.AssignedGroupMemberStatus;

                var tempGroupMember = new GroupMember
                {
                    GroupId = entity.AssignedGroupId.Value,
                    GroupRoleId = entity.AssignedGroupMemberRoleId.Value
                };

                tempGroupMember.LoadAttributes();

                var savedMemberAttributeValues = entity.AssignedGroupMemberAttributeValues?.FromJsonOrNull<Dictionary<string, string>>();
                if ( savedMemberAttributeValues != null )
                {
                    foreach ( var item in savedMemberAttributeValues )
                    {
                        tempGroupMember.SetAttributeValue( item.Key, item.Value );
                    }
                }

                bag.PlacementGroupMemberAttributes = tempGroupMember.GetPublicAttributesForEdit( currentPerson );
                bag.PlacementGroupMemberAttributeValues = tempGroupMember.GetPublicAttributeValuesForEdit( currentPerson );
            }

            entity.LoadAttributes();

            bag.ConnectionRequestAttributes = entity.GetPublicAttributesForEdit( currentPerson );
            bag.ConnectionRequestAttributeValues = entity.GetPublicAttributeValuesForEdit( currentPerson );

            if ( entity.AssignedGroup != null )
            {
                entity.AssignedGroup.LoadAttributes();

                bag.PlacementGroupMemberAttributes = entity.AssignedGroup.GetPublicAttributesForEdit( currentPerson );
            }

            return bag;
        }

        private bool TryGetEntityForEditAction( string idKey, out ConnectionRequest entity, out BlockActionResult error, Guid? connectionOpportunityGuid = null )
        {
            var entityService = new ConnectionRequestService( RockContext );
            error = null;
            bool? enableRequestSecurity = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                var connectionType = GetConnectionTypeCacheFromPageParameters();
                if ( connectionType == null )
                {
                    error = ActionBadRequest( $"{ConnectionType.FriendlyTypeName} not found." );
                    entity = null;
                    return false;
                }

                // Resolve the opportunity now so the security check can evaluate against the
                // correct opportunity rather than the default Id of 0 on the unsaved entity.
                if ( !connectionOpportunityGuid.HasValue )
                {
                    error = ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
                    entity = null;
                    return false;
                }

                var connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( connectionOpportunityGuid.Value );
                if ( connectionOpportunity == null )
                {
                    error = ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
                    entity = null;
                    return false;
                }

                entity = new ConnectionRequest();
                entity.ConnectionTypeId = connectionType.Id;
                entity.ConnectionOpportunityId = connectionOpportunity.Id;
                entityService.Add( entity );

                enableRequestSecurity = connectionType.EnableRequestSecurity;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
                return false;
            }

            if ( !CanEditSpecifiedConnectionRequest( entity, out error, enableRequestSecurity ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {ConnectionRequest.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the Conneciton Request entity
        /// </summary>
        /// <param name="entity">The Connection Request entity that is being updated</param>
        /// <param name="box">The properties box we are using to update the Connection Request</param>
        /// <returns></returns>
        private bool UpdateEntityFromBox( ConnectionRequest entity, ValidPropertiesBox<ConnectionRequestBag> box )
        {
            if ( box?.Bag == null || box.ValidProperties == null )
            {
                return false;
            }

            var originalConnectionState = entity.ConnectionState;

            // If we are creating a new connection request, then we need to set the Connection Opportunity. If we are editing an existing connection request, then we should not change the Connection Opportunity.
            if ( entity.Id == 0 )
            {
                var isConnectionOpportunityValid = box.IfValidProperty( nameof( box.Bag.ConnectionOpportunityGuid ), () =>
                {
                    Guid? connectionOpportunityGuid = box.Bag.ConnectionOpportunityGuid.AsGuidOrNull();

                    if ( !connectionOpportunityGuid.HasValue )
                    {
                        return false;
                    }

                    var connectionOpportunityId = new ConnectionOpportunityService( RockContext ).GetId( connectionOpportunityGuid.Value );

                    if ( !connectionOpportunityId.HasValue )
                    {
                        return false;
                    }

                    entity.ConnectionOpportunityId = connectionOpportunityId.Value;
                    return true;
                }, false);

                if ( !isConnectionOpportunityValid )
                {
                    return false;
                }
            }
            else
            {
                // We only need to handle adding the connection status history note when editing a connection request.
                box.IfValidProperty( nameof( box.Bag.ConnectionStatusHistoryNote ),
                    () => entity.ConnectionStatusHistoryNote = box.Bag.ConnectionStatusHistoryNote );
            }

            box.IfValidProperty( nameof( box.Bag.CampusGuid ),
                () => entity.CampusId = CampusCache.GetId( box.Bag.CampusGuid.AsGuid() ) );

            box.IfValidProperty( nameof( box.Bag.Requester ),
                () => entity.PersonAliasId = box.Bag.Requester.GetEntityId<PersonAlias>( RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.ConnectorPersonAliasGuid ),
                () => entity.ConnectorPersonAliasId = new PersonAliasService( RockContext ).GetId( box.Bag.ConnectorPersonAliasGuid.AsGuid() ) );

            box.IfValidProperty( nameof( box.Bag.ConnectionState ), () =>
            {
                var state = box.Bag.ConnectionState ?? ConnectionState.Active;

                entity.ConnectionState = state;
            } );

            if ( entity.ConnectionState == ConnectionState.FutureFollowUp )
            {
                box.IfValidProperty( nameof( box.Bag.FollowUpDate ),
                    () => entity.FollowupDate = box.Bag.FollowUpDate?.DateTime );
            }

            var isConnectionStatusValid = box.IfValidProperty( nameof( box.Bag.ConnectionStatusGuid ), () =>
            {
                var connectionStatusId = new ConnectionStatusService( RockContext ).GetId( box.Bag.ConnectionStatusGuid.AsGuid() );

                if ( !connectionStatusId.HasValue )
                {
                    return false;
                }

                entity.ConnectionStatusId = connectionStatusId.Value;
                return true;
            }, true );

            if ( !isConnectionStatusValid )
            {
                return false;
            }

            // The placement group details cannot be set if the request's original connection state is connected. (mirrors webforms)
            if ( originalConnectionState != ConnectionState.Connected )
            {
                box.IfValidProperty( nameof( box.Bag.PlacementGroupGuid ),
                    () => entity.AssignedGroupId = GroupCache.GetId( box.Bag.PlacementGroupGuid.AsGuid() ) );

                box.IfValidProperty( nameof( box.Bag.GroupMemberRoleGuid ),
                    () => entity.AssignedGroupMemberRoleId = GroupTypeRoleCache.GetId( box.Bag.GroupMemberRoleGuid.AsGuid() ) );

                box.IfValidProperty( nameof( box.Bag.GroupMemberStatus ),
                    () => entity.AssignedGroupMemberStatus = box.Bag.GroupMemberStatus );

                box.IfValidProperty( nameof( box.Bag.PlacementGroupMemberAttributeValues ),
                    () => entity.AssignedGroupMemberAttributeValues = GetGroupMemberAttributeValuesFromBag( box.Bag.PlacementGroupMemberAttributeValues, entity.AssignedGroupId, entity.AssignedGroupMemberRoleId, entity.AssignedGroupMemberStatus ) );
            }

            box.IfValidProperty( nameof( box.Bag.Comments ),
                () => entity.Comments = box.Bag.Comments );

            box.IfValidProperty( nameof( box.Bag.RequestSourceGuid ),
                () => entity.ConnectionTypeSourceId = new ConnectionTypeSourceService( RockContext ).GetId( box.Bag.RequestSourceGuid.AsGuid() ) );

            box.IfValidProperty( nameof( box.Bag.ConnectionRequestAttributeValues ), () =>
            {
                entity.LoadAttributes( RockContext );
                entity.SetPublicAttributeValues( box.Bag.ConnectionRequestAttributeValues, RequestContext.CurrentPerson );
            } );

            return true;
        }

        /// <summary>
        /// Checks whether the logged in user can edit Connection Requests.
        /// </summary>
        /// <param name="connectionType">The specified Connection Type</param>
        /// <param name="connectionOpportunity">An optional Connection Opportunity</param>
        /// <returns>true if the user can edit a Connection Request.</returns>
        private bool CanEditConnectionRequests( ConnectionType connectionType, ConnectionOpportunity connectionOpportunity = null )
        {
            bool userCanEditConnectionRequests = false;

            if ( connectionOpportunity != null )
            {
                userCanEditConnectionRequests = GetAuthorizationTarget( connectionType, connectionOpportunity ).IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            }
            else
            {
                userCanEditConnectionRequests = connectionType.ConnectionOpportunities.All( co => GetAuthorizationTarget( connectionType, co ).IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
            }

            if ( !userCanEditConnectionRequests )
            {
                if ( connectionOpportunity != null )
                {
                    // Checks if the current person is a connector for the specified Connection Opportunity.
                    userCanEditConnectionRequests = new ConnectionOpportunityConnectorGroupService( RockContext )
                        .Queryable()
                        .Where( cg => cg.ConnectionOpportunityId == connectionOpportunity.Id )
                        .SelectMany( cg => cg.ConnectorGroup.Members )
                        .Any( m => m.PersonId == RequestContext.CurrentPerson.Id );
                }
                else
                {
                    // Checks if all Connection Opportunities for the Connection Type have the current person as a connector.
                    var opportunityIds = connectionType.ConnectionOpportunities.Select( o => o.Id ).ToList();

                    userCanEditConnectionRequests = new ConnectionOpportunityConnectorGroupService( RockContext )
                        .Queryable()
                        .Where( cg => opportunityIds.Contains( cg.ConnectionOpportunityId )
                                   && cg.ConnectorGroup.Members.Any( m => m.PersonId == RequestContext.CurrentPerson.Id ) )
                        .Select( cg => cg.ConnectionOpportunityId )
                        .Distinct()
                        .Count() == opportunityIds.Count;
                }
            }

            return userCanEditConnectionRequests;
        }

        /// <summary>
        /// Determines if the logged in user can edit a specific Connection Request.
        /// </summary>
        /// <param name="connectionType">The Connection Type Cache tied to the Connection Request.</param>
        /// <param name="connectionRequestIdKey">The Connection Request IdKey that we check edit permissions for.</param>
        /// <param name="connectionRequest">When this method returns, contains the resolved Connection Request, or null if it could not be found.</param>
        /// <param name="error">When this method returns, contains a Block Action Result error if the user lacks permission or the request could not be found; otherwise null.</param>
        /// <returns>True if the logged in user has edit permissions for the Connection Request; otherwise false.</returns>
        private bool CanEditSpecifiedConnectionRequest( ConnectionTypeCache connectionType, string connectionRequestIdKey, out ConnectionRequest connectionRequest, out BlockActionResult error )
        {
            var canEdit = CanEditSpecifiedConnectionRequests( connectionType, new List<string> { connectionRequestIdKey }, out var connectionRequests, out error );
            connectionRequest = connectionRequests.FirstOrDefault();
            return canEdit;
        }

        /// <summary>
        /// Determines if the logged in user can edit the specified Connection Request entity.
        /// Use this overload when the entity is already loaded; no database lookup is performed.
        /// </summary>
        /// <param name="connectionRequest">
        /// The already-loaded Connection Request to check. For saved requests where
        /// <paramref name="enableRequestSecurity"/> is not provided, the
        /// <see cref="ConnectionRequest.ConnectionOpportunity"/> and its
        /// <see cref="ConnectionOpportunity.ConnectionType"/> navigation properties must be loaded.
        /// </param>
        /// <param name="error">When this method returns, contains a Block Action Result error if the user lacks permission; otherwise null.</param>
        /// <param name="enableRequestSecurity">
        /// When provided, overrides the <see cref="ConnectionType.EnableRequestSecurity"/> value read
        /// from the navigation property. Pass this explicitly when checking a new (unsaved) request
        /// or when the navigation property is not loaded.
        /// </param>
        /// <returns>True if the logged in user has edit permissions for the Connection Request; otherwise false.</returns>
        private bool CanEditSpecifiedConnectionRequest( ConnectionRequest connectionRequest, out BlockActionResult error, bool? enableRequestSecurity = null )
        {
            return CanEditSpecifiedConnectionRequests( new List<ConnectionRequest> { connectionRequest }, out error, enableRequestSecurity );
        }

        /// <summary>
        /// Determines if the logged in user can edit a list of Connection Request entities.
        /// Use this overload when the entities are already loaded; no database lookup is performed.
        /// Also grants edit access if the current user is the assigned connector, or is an active
        /// member of a connector group that covers the request's campus.
        /// </summary>
        /// <param name="connectionRequests">
        /// The already-loaded Connection Requests to check. All requests are assumed to belong to
        /// the same Connection Type. For saved requests where <paramref name="enableRequestSecurity"/>
        /// is not provided, the <see cref="ConnectionRequest.ConnectionOpportunity"/> and its
        /// <see cref="ConnectionOpportunity.ConnectionType"/> navigation properties must be loaded.
        /// </param>
        /// <param name="error">When this method returns, contains a Block Action Result error if the user lacks permission; otherwise null.</param>
        /// <param name="enableRequestSecurity">
        /// When provided, overrides the <see cref="ConnectionType.EnableRequestSecurity"/> value read
        /// from the navigation property. If null, the value is read from the first request's
        /// <see cref="ConnectionRequest.ConnectionOpportunity"/>.<see cref="ConnectionOpportunity.ConnectionType"/>
        /// navigation property. Pass this explicitly when checking new (unsaved) requests or when
        /// the navigation property is not loaded.
        /// </param>
        /// <returns>True if the logged in user has edit permissions for all specified Connection Requests; otherwise false.</returns>
        private bool CanEditSpecifiedConnectionRequests( List<ConnectionRequest> connectionRequests, out BlockActionResult error, bool? enableRequestSecurity = null )
        {
            error = null;

            var opportunityIds = connectionRequests.Select( r => r.ConnectionOpportunityId )
                .Distinct()
                .ToList();

            bool userCanEditConnectionRequest;
            var resolvedEnableRequestSecurity = enableRequestSecurity ?? connectionRequests.FirstOrDefault()?.ConnectionOpportunity?.ConnectionType?.EnableRequestSecurity == true;

            if ( resolvedEnableRequestSecurity )
            {
                userCanEditConnectionRequest = connectionRequests.All( cr => cr.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
            }
            else
            {
                var opportunities = new ConnectionOpportunityService( RockContext ).Queryable()
                    .AsNoTracking()
                    .Where( o => opportunityIds.Contains( o.Id ) )
                    .ToList();

                userCanEditConnectionRequest = opportunities.All( co => co.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
            }

            if ( !userCanEditConnectionRequest )
            {
                var connectorGroupsByOpportunity = new ConnectionOpportunityConnectorGroupService( RockContext ).Queryable()
                    .AsNoTracking()
                    .Where( g => opportunityIds.Contains( g.ConnectionOpportunityId ) )
                    .Where( g =>
                        g.ConnectorGroup != null &&
                        g.ConnectorGroup.Members.Any( m =>
                            m.PersonId == RequestContext.CurrentPerson.Id &&
                            m.GroupMemberStatus == GroupMemberStatus.Active ) )
                    .ToList()
                    .GroupBy( g => g.ConnectionOpportunityId )
                    .ToDictionary( g => g.Key, g => g.ToList() );

                var activeCampusCount = CampusCache.All().Count( c => c.IsActive ?? true );

                userCanEditConnectionRequest = connectionRequests.All( cr =>
                {
                    if ( cr.ConnectorPersonAlias != null && cr.ConnectorPersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    {
                        return true;
                    }

                    if ( !connectorGroupsByOpportunity.TryGetValue( cr.ConnectionOpportunityId, out var groups ) )
                    {
                        return false;
                    }

                    // Non campus-specific connector groups
                    if ( activeCampusCount == 1 || groups.Any( g => !g.CampusId.HasValue ) )
                    {
                        return true;
                    }

                    // Campus-specific logic: if the request has no campus (including new unsaved requests)
                    // it is not campus-restricted; otherwise the connector group must match the request's campus.
                    if ( !cr.CampusId.HasValue || groups.Any( g => g.CampusId == cr.CampusId.Value ) )
                    {
                        return true;
                    }

                    return false;
                } );
            }

            if ( !userCanEditConnectionRequest )
            {
                error = ActionForbidden( $"Not authorized to edit {ConnectionRequest.FriendlyTypeName}." );
            }

            return userCanEditConnectionRequest;
        }

        /// <summary>
        /// Determines if the logged in user can edit a list of Connection Requests.
        /// Checks edit authorization either at the request level (if EnableRequestSecurity is set on
        /// the Connection Type) or at the opportunity level. Also grants edit access if the current
        /// user is the assigned connector or is a member of an applicable connector group for the
        /// request's campus.
        /// </summary>
        /// <param name="connectionType">The Connection Type Cache tied to the Connection Requests, used to determine the security model and validate that all requests belong to this type.</param>
        /// <param name="connectionRequestIdKeys">The list of IdKeys for the Connection Requests to check edit permissions for.</param>
        /// <param name="connectionRequests">When this method returns, contains the resolved list of Connection Requests; empty if any IdKeys could not be resolved.</param>
        /// <param name="error">When this method returns, contains a Block Action Result error if any requests could not be found or the user lacks permission; otherwise null.</param>
        /// <param name="queryModifier">An optional function to apply additional filtering or modification to the Connection Request query before it is executed.</param>
        /// <returns>True if the logged in user has edit permissions for all specified Connection Requests; otherwise false.</returns>
        private bool CanEditSpecifiedConnectionRequests( ConnectionTypeCache connectionType, List<string> connectionRequestIdKeys, out List<ConnectionRequest> connectionRequests, out BlockActionResult error, Func<IQueryable<ConnectionRequest>, IQueryable<ConnectionRequest>> queryModifier = null )
        {
            error = null;
            var decodedIds = connectionRequestIdKeys.Select( key => Rock.Utility.IdHasher.Instance.GetId( key ) ).ToList();

            if ( decodedIds.Any( id => !id.HasValue ) )
            {
                connectionRequests = new List<ConnectionRequest>();
                error = ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
                return false;
            }

            var connectionRequestIds = decodedIds
                .Select( id => id.Value )
                .Distinct()
                .ToList();

            if ( !connectionRequestIds.Any() )
            {
                connectionRequests = new List<ConnectionRequest>();
                error = ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
                return false;
            }

            var connectionRequestsQry = new ConnectionRequestService( RockContext ).GetByIds( connectionRequestIds )
                .Where(c => c.ConnectionTypeId == connectionType.Id); // Confirm that all requests match the Connection Type that we are checking security for.

            if ( queryModifier != null )
            {
                connectionRequestsQry = queryModifier( connectionRequestsQry );
            }

            connectionRequests = connectionRequestsQry.ToList();

            if ( connectionRequests.Count != connectionRequestIds.Count )
            {
                error = ActionBadRequest( $"{ConnectionRequest.FriendlyTypeName} not found." );
                return false;
            }

            return CanEditSpecifiedConnectionRequests( connectionRequests, out error );
        }

        #region UI Refresh Helpers

        /// <summary>
        /// Gets a Connection List Update Box for the specified Connection Request, containing
        /// the updated grid row data and optionally the full request detail box.
        /// </summary>
        /// <param name="connectionRequestId">The ID of the Connection Request to build the update box for.</param>
        /// <param name="includeRequestDetails">If true, the detail box is populated in addition to the grid row; used when the request detail panel is currently visible.</param>
        /// <returns>A <see cref="ConnectionListUpdateBox"/> containing the refreshed grid row and, if requested, the full detail box for the Connection Request.</returns>
        private ConnectionListUpdateBox GetConnectionListUpdateBox( int connectionRequestId, bool includeRequestDetails )
        {
            var box = new ConnectionListUpdateBox();

            var connectionRequest = new ConnectionRequestService( RockContext )
                .Queryable()
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.ConnectorPersonAlias.Person )
                .Include( r => r.ConnectionOpportunity.ConnectionOpportunityCampuses )
                .Include( r => r.ConnectionTypeSource )
                .Include( r => r.ConnectionStatus )
                .Include( r => r.Campus )
                .Include( r => r.AssignedGroup )
                .Include( r => r.ConnectionRequestActivities.Select( a => a.ConnectionActivityType ) )
                .FirstOrDefault( r => r.Id == connectionRequestId );

            box.GridRow = GetConnectionRequestGridRow( connectionRequest );

            if ( includeRequestDetails )
            {
                box.DetailBox = GetConnectionRequestDetailBox( connectionRequest );
            }

            return box;
        }

        /// <summary>
        /// Builds a single grid row dictionary for the given Connection Request, containing
        /// all display fields needed to render or refresh the request's row in the connection list grid.
        /// Includes grouping fields for connector, opportunity, campus, status, due status, and state,
        /// as well as requester details, activity counts, placement group info, and reminder count.
        /// </summary>
        /// <param name="connectionRequest">The Connection Request to build the grid row for. Must have related entities eager-loaded, including PersonAlias, ConnectorPersonAlias, ConnectionOpportunity, ConnectionStatus, Campus, AssignedGroup, and ConnectionRequestActivities.</param>
        /// <returns>A dictionary representing the grid row data for the Connection Request, keyed by field name.</returns>
        /// <summary>
        /// Gets the celebration text for the given Connection Request by fetching the first
        /// Celebration Note associated with it. Returns null if no such note exists.
        /// </summary>
        /// <param name="connectionRequestId">The Id of the Connection Request.</param>
        /// <returns>The text of the first Celebration Note, or null if none exists.</returns>
        private string GetCelebrationText( int connectionRequestId )
        {
            var celebrationNoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() ).Id;

            return new NoteService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( n => n.NoteTypeId == celebrationNoteTypeId && n.EntityId == connectionRequestId )
                .Select( n => n.Text )
                .FirstOrDefault();
        }

        private Dictionary<string, object> GetConnectionRequestGridRow( ConnectionRequest connectionRequest )
        {
            var dueStatus = GetDueStatus( connectionRequest.DueDate, connectionRequest.DueSoonDate, connectionRequest.ConnectionState, connectionRequest.ConnectedDateTime );
            var connectorItem = new ListItemBag();

            if ( connectionRequest.ConnectorPersonAliasId.HasValue )
            {
                connectorItem.Value = connectionRequest.ConnectorPersonAlias.Person.IdKey;
                connectorItem.Text = connectionRequest.ConnectorPersonAlias.Person.FullName;
            }
            else
            {
                connectorItem.Value = "unassigned";
                connectorItem.Text = "Unassigned";
            }

            var connectionStatusBag = new ConnectionStatusBag
            {
                Guid = connectionRequest.ConnectionStatus.Guid,
                Name = connectionRequest.ConnectionStatus.Name,
                Order = connectionRequest.ConnectionStatus.Order,
                HighlightColor = connectionRequest.ConnectionStatus.HighlightColor,
                IsNoteRequiredOnCompletion = connectionRequest.ConnectionStatus.IsNoteRequiredOnCompletion,
                IsDefaultStatus = connectionRequest.ConnectionStatus.IsDefault
            };

            var reminderCount = new ReminderService( RockContext ).Queryable()
                .Include( r => r.PersonAlias )
                .AsNoTracking()
                .Where( r => !r.IsComplete && r.ReminderDate < RockDateTime.Now && r.PersonAlias.PersonId == RequestContext.CurrentPerson.Id && r.EntityId == connectionRequest.Id )
                .Count();

            var celebrationText = GetCelebrationText( connectionRequest.Id );

            var requesterPerson = new PersonFieldBag
            {
                IdKey = connectionRequest.PersonAlias.Person.IdKey,
                NickName = connectionRequest.PersonAlias.Person.NickName,
                LastName = connectionRequest.PersonAlias.Person.LastName,
                PhotoUrl = connectionRequest.PersonAlias.Person.PhotoUrl,
            };

            if ( connectionRequest.PersonAlias.Person.ConnectionStatusValueId.HasValue )
            {
                var connectionStatusValue = DefinedValueCache.Get( connectionRequest.PersonAlias.Person.ConnectionStatusValueId.Value );
                if ( connectionStatusValue != null )
                {
                    requesterPerson.ConnectionStatus = connectionStatusValue.Value;
                }
            }

            var newConnection = new ConnectionRow
            {
                ConnectionRequest = connectionRequest,
                ConnectionRequestId = connectionRequest.Id,
                ConnectorGrouping = GetGroupingFieldBag( connectionRequest.ConnectorPersonAliasId, "person", connectionRequest.ConnectorPersonAlias?.Person?.FullName, null, null, connectionRequest.ConnectorPersonAlias?.Person?.PhotoUrl ),
                OpportunityGrouping = GetGroupingFieldBag( connectionRequest.ConnectionOpportunityId, "text", connectionRequest.ConnectionOpportunity.Name, connectionRequest.ConnectionOpportunity.Order, connectionRequest.ConnectionOpportunity.IconCssClass ),
                CampusGrouping = GetGroupingFieldBag( connectionRequest.CampusId, "text", connectionRequest.Campus?.Name, connectionRequest.Campus?.Order ),
                StatusGrouping = GetGroupingFieldBag( connectionRequest.ConnectionStatusId, "text", connectionRequest.ConnectionStatus?.Name, connectionRequest.ConnectionStatus?.Order ),
                DueStatusGrouping = GetGroupingFieldBag( ( int ) dueStatus, "text", dueStatus.GetDisplayName(), dueStatus.GetOrder(), "ti ti-calendar", null, GetDueStatusTextColorCssClass( dueStatus ) ),
                StateGrouping = new GroupingFieldBag
                {
                    Key = connectionRequest.ConnectionState.ToString(),
                    Type = "text",
                    Label = connectionRequest.ConnectionState.GetDisplayName(),
                    IconCssClass = GetStateIconCssClass( connectionRequest.ConnectionState ),
                    Order = ( int ) connectionRequest.ConnectionState
                },
                ConnectorDetails = connectorItem,
                Person = requesterPerson,
                RequesterPersonAliasGuid = connectionRequest.PersonAlias.Guid,
                ConnectionOpportunity = connectionRequest.ConnectionOpportunity.Name,
                ConnectionOpportunityGuid = connectionRequest.ConnectionOpportunity.Guid,
                ConnectionTypeSource = connectionRequest.ConnectionTypeSource?.Name,
                Campus = connectionRequest.Campus?.Name,
                CampusGuid = connectionRequest.Campus?.Guid,
                Group = connectionRequest.AssignedGroup?.Name,
                ConnectionStatus = connectionStatusBag,
                LastActivityDateTime = connectionRequest.ConnectionRequestActivities.Select( cra => cra.CreatedDateTime )
                    .OrderByDescending( d => d )
                    .FirstOrDefault(),
                ActivityCount = connectionRequest.ConnectionRequestActivities.Count(),
                CreatedDateTime = connectionRequest.CreatedDateTime,
                DueDate = connectionRequest.DueDate,
                DueSoonDate = connectionRequest.DueSoonDate,
                CompletedDateTime = connectionRequest.ConnectedDateTime,
                DueStatus = dueStatus,
                FollowUpDate = connectionRequest.FollowupDate,
                ConnectionState = connectionRequest.ConnectionState,
                CelebrationText = celebrationText,
                ReminderCount = reminderCount,
                HasPlacementGroup = connectionRequest.AssignedGroup != null,
                HasRequiredGroupRequirements = connectionRequest.AssignedGroup?.GroupRequirements?.Any( r => r.MustMeetRequirementToAddMember ) ?? false
            };

            var builder = GetGridBuilder();
            var row = builder.Build( new[] { newConnection } ).Rows[0];

            return row;
        }

        /// <summary>
        /// Gets a Connection Request Detail Box for the given Connection Request,
        /// containing both the detail options and the fully populated entity bag
        /// needed to render the request detail panel.
        /// </summary>
        /// <param name="connectionRequest">The Connection Request to build the detail box for.</param>
        /// <returns>A <see cref="ConnectionRequestDetailBox"/> containing the options and entity bag for the Connection Request detail panel.</returns>
        private ConnectionRequestDetailBox GetConnectionRequestDetailBox( ConnectionRequest connectionRequest )
        {
            var box = new ConnectionRequestDetailBox
            {
                Options = GetConnectionRequestDetailOptionsBag( connectionRequest, out var mergeFields ),
                Entity = GetConnectionRequestDetailsBag( connectionRequest, mergeFields )
            };

            return box;
        }

        /// <summary>
        /// Builds the Connection Request Detail Options Bag for the given Connection Request,
        /// containing all configuration and display options needed to render the request detail panel.
        /// This includes connection type settings, available connectors, statuses, activity types,
        /// connection states, placement group configurations, badge guids, AI summary visibility,
        /// and resolved Lava templates.
        /// </summary>
        /// <remarks>
        /// Connector items are built from the opportunity's connector groups, with the unassigned option,
        /// the request's current connector, and the current user each guaranteed to be present in the list.
        /// Placement group details — including available roles, statuses per role, and group member attributes
        /// — are only populated if group placements are enabled on the Connection Type.
        /// </remarks>
        /// <param name="connectionRequest">The Connection Request to build the options bag for. Must have related entities eager-loaded, including ConnectionOpportunity, ConnectionType, and ConnectionOpportunityGroups.</param>
        /// <param name="mergeFields">When this method returns, contains the Lava merge fields resolved for the request, including the Connection Request and requester Person.</param>
        /// <returns>A <see cref="ConnectionRequestDetailOptionsBag"/> populated with all options required to render the Connection Request detail panel.</returns>
        private ConnectionRequestDetailOptionsBag GetConnectionRequestDetailOptionsBag( ConnectionRequest connectionRequest, out Dictionary<string, object> mergeFields )
        {
            var optionsBag = new ConnectionRequestDetailOptionsBag
            {
                ConnectionTypeIdKey = connectionRequest.ConnectionOpportunity.ConnectionType.IdKey,
                CanEditConnectionRequest = CanEditSpecifiedConnectionRequest( connectionRequest, out _ ),
                RequiresPlacementGroupToComplete = connectionRequest.ConnectionOpportunity.ConnectionType.RequiresPlacementGroupToConnect,
                ConnectionStatuses = connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses.Where( s => s.IsActive )
                .Select( s => new ConnectionStatusBag
                {
                    Guid = s.Guid,
                    Name = s.Name,
                    HighlightColor = s.HighlightColor,
                    Order = s.Order,
                    IsNoteRequiredOnCompletion = s.IsNoteRequiredOnCompletion,
                    IsDefaultStatus = s.IsDefault,
                    Disabled = connectionRequest.ConnectionOpportunity.ConnectionType.IsSequentialStatusEnforced
                        ? s.Order < connectionRequest.ConnectionStatus.Order
                        : false
                } ).OrderBy( s => s.Order ).ToList(),
                IsFutureFollowUpEnabled = connectionRequest.ConnectionOpportunity.ConnectionType.EnableFutureFollowup,
                IsRequestSecurityEnabled = connectionRequest.ConnectionOpportunity.ConnectionType.EnableRequestSecurity,
                IsSequentialStatusMode = connectionRequest.ConnectionOpportunity.ConnectionType.IsSequentialStatusEnforced,
                AreCelebrationsEnabled = connectionRequest.ConnectionOpportunity.ConnectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Celebration ),
                AreRemindersEnabled = connectionRequest.ConnectionOpportunity.ConnectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Reminder ),
                AreGroupPlacementsEnabled = connectionRequest.ConnectionOpportunity.ConnectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.GroupPlacement ),
                RequestSourceItems = connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionTypeSources.ToListItemBagList()
            };

            optionsBag.ConnectorItems = GetConnectorItems( connectionRequest.ConnectionOpportunityId, connectionRequest.CampusId, connectionRequest.ConnectorPersonAlias, true );
            optionsBag.ConnectorItemsForEdit = GetConnectorItems( connectionRequest.ConnectionOpportunityId, null, connectionRequest.ConnectorPersonAlias, false );

            var tempNote = new Note
            {
                EntityId = connectionRequest.Id,
                NoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE.AsGuid() ).Id,
            };

            optionsBag.CanEditConnectionRequestNote = tempNote.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            var campusItems = connectionRequest.ConnectionOpportunity.ConnectionOpportunityCampuses.Where( c => c.Campus != null && c.Campus.IsActive == true )
                .Select( c => c.Campus )
                .ToListItemBagList();

            if ( connectionRequest.CampusId.HasValue )
            {
                // Add the selected campus context to the campus list if it is not already included.
                var campus = CampusCache.Get( connectionRequest.CampusId.Value );
                if ( campus != null && !campusItems.Any( c => c.Value == campus.Guid.ToString() ) )
                {
                    campusItems.Add( new ListItemBag
                    {
                        Text = campus.Name,
                        Value = campus.Guid.ToString()
                    } );
                }
            }

            optionsBag.Campuses = campusItems;

            optionsBag.ConnectionActivities = connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes.Where( at => at.IsActive )
            .Select( a => new ConnectionActivityTypeBag
            {
                ActivityType = a.ToListItemBag(),
                PersonNoteCreationBehavior = a.PersonNoteCreationBehavior
            } ).ToList();

            var delimitedBadgeGuids = GetAttributeValue( AttributeKey.Badges );
            optionsBag.BadgeGuids = delimitedBadgeGuids.SplitDelimitedValues().AsGuidList();

            var connectionTypeAdditionalSettings = connectionRequest.ConnectionOpportunity.ConnectionType.GetConnectionTypeAdditionalSettings();

            optionsBag.IsAISummaryVisible = new AIProviderService( RockContext ).GetActiveProvider() != null
                && ( connectionTypeAdditionalSettings?.AIInsightsPrompt.IsNotNullOrWhiteSpace() == true );
            optionsBag.AISummaryTrigger = connectionTypeAdditionalSettings?.AISummaryTrigger ?? AISummaryTriggerMode.Manual;

            List<ConnectionState> ignoredConnectionStates = new List<ConnectionState>();

            if ( !connectionRequest.ConnectionOpportunity.ConnectionType.EnableFutureFollowup )
            {
                ignoredConnectionStates.Add( ConnectionState.FutureFollowUp );
            }

            optionsBag.ConnectionStates = typeof( ConnectionState ).ToEnumListItemBag()
                .Where( i => !ignoredConnectionStates.Contains( ( ConnectionState ) i.Value.AsInteger() ) )
                .ToList();

            if ( optionsBag.AreGroupPlacementsEnabled )
            {
                var groupConfigs = new ConnectionOpportunityGroupConfigService( RockContext )
                    .Queryable()
                    .Include( c => c.GroupMemberRole )
                    .Where( c => c.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId )
                    .ToList();

                var opportunityGroups = new ConnectionOpportunityGroupService( RockContext )
                    .Queryable()
                    .Include( g => g.Group.Campus )
                    .Where( g => g.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId )
                    .ToList();

                var configsByGroupTypeId = groupConfigs
                    .GroupBy( c => c.GroupTypeId )
                    .ToDictionary(
                        grp => grp.Key,
                        grp => grp.Select( c => new { Role = c.GroupMemberRole, Status = c.GroupMemberStatus } ).ToList()
                    );

                optionsBag.PlacementGroups = opportunityGroups.Where( g => configsByGroupTypeId.ContainsKey( g.Group.GroupTypeId ) )
                    .Select( g =>
                    {
                        var configs = configsByGroupTypeId[g.Group.GroupTypeId];

                        var tempGroupMember = new Rock.Model.GroupMember { GroupId = g.GroupId };
                        tempGroupMember.LoadAttributes();

                        return new PlacementGroupDetailsBag
                        {
                            ListItemBag = new ListItemBag
                            {
                                Text = g.Group.CampusId.HasValue ? $"{g.Group.Name} ({g.Group.Campus.Name})" : $"{g.Group.Name} (No Campus)",
                                Value = g.Group.Guid.ToString()
                            },
                            CampusGuid = g.Group.Campus?.Guid,
                            GroupMemberRoles = configs
                                .DistinctBy( c => c.Role.Guid )
                                .Select( c => c.Role.ToListItemBag() )
                                .ToList(),
                            GroupMemberStatuses = configs
                                .GroupBy( c => c.Role.Guid.ToString() )
                                .ToDictionary(
                                    grp => grp.Key,
                                    grp => grp.OrderBy( c => c.Status )
                                        .DistinctBy( c => c.Status )
                                        .Select( c => new ListItemBag
                                        {
                                            Text = c.Status.ToString(),
                                            Value = ( ( int ) c.Status ).ToString()
                                        } )
                                        .ToList()
                                ),
                            GroupMemberAttributes = tempGroupMember.GetPublicAttributesForEdit( RequestContext.CurrentPerson )
                        };
                    } ).ToList();
            }

            var requesterPerson = connectionRequest.PersonAlias.Person;

            // Add the lava header
            // Resolve the text field merge fields
            mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( "ConnectionRequest", connectionRequest );
            mergeFields.Add( "Person", requesterPerson );

            optionsBag.LavaHeadingTemplate = GetAttributeValue( AttributeKey.LavaHeadingTemplate ).ResolveMergeFields( mergeFields );
            optionsBag.LavaBadgeBar = GetAttributeValue( AttributeKey.LavaBadgeBar ).ResolveMergeFields( mergeFields );

            return optionsBag;
        }

        /// <summary>
        /// Builds the Connection Request Details Bag for the given Connection Request,
        /// containing all data needed to render the request detail panel. This includes
        /// request state and status, requester and connector details, placement group
        /// membership and requirements, eligible manual workflows, person notes,
        /// activity entries, additional connection requests, and public attributes.
        /// </summary>
        /// <remarks>
        /// Placement group details are only populated if group placements are enabled on the
        /// Connection Type. If the requester is not yet a member of the assigned placement group,
        /// a pending member is constructed to derive attribute definitions and requirement statuses.
        /// Manual workflows are filtered by eligibility, active status, view authorization, and
        /// optional include/exclude Data Views, and ordered according to the opportunity's saved
        /// workflow type order.
        /// </remarks>
        /// <param name="connectionRequest">The Connection Request to build the details bag for. Must have related entities eager-loaded and attributes will be loaded internally.</param>
        /// <param name="mergeFields">The Lava merge fields to use when resolving activity entry templates, typically obtained from <see cref="GetConnectionRequestDetailOptionsBag"/>.</param>
        /// <returns>A <see cref="ConnectionRequestDetailsBag"/> populated with all data required to render the Connection Request detail panel.</returns>
        private ConnectionRequestDetailsBag GetConnectionRequestDetailsBag( ConnectionRequest connectionRequest, Dictionary<string, object> mergeFields )
        {
            connectionRequest.LoadAttributes();

            var detailsBag = new ConnectionRequestDetailsBag
            {
                ConnectionRequestIdKey = connectionRequest.IdKey,
                RequesterPersonAliasGuid = connectionRequest.PersonAlias.Guid,
                ConnectionState = connectionRequest.ConnectionState,
                ConnectionStatus = connectionRequest.ConnectionStatus != null ? new ConnectionStatusBag
                {
                    Guid = connectionRequest.ConnectionStatus.Guid,
                    Name = connectionRequest.ConnectionStatus.Name,
                    HighlightColor = connectionRequest.ConnectionStatus.HighlightColor,
                    Order = connectionRequest.ConnectionStatus.Order,
                    IsNoteRequiredOnCompletion = connectionRequest.ConnectionStatus.IsNoteRequiredOnCompletion,
                    IsDefaultStatus = connectionRequest.ConnectionStatus.IsDefault
                } : null,
                FollowUpDate = connectionRequest.FollowupDate?.ToRockDateTimeOffset(),
                ConnectionOpportunityName = connectionRequest.ConnectionOpportunity.Name,
                ConnectionOpportunityIcon = connectionRequest.ConnectionOpportunity.IconCssClass,
                Campus = connectionRequest.Campus?.Name,
                ConnectorPerson = connectionRequest.ConnectorPersonAlias?.Guid.ToString() ?? "unassigned",
                CreatedDateTime = connectionRequest.CreatedDateTime?.ToRockDateTimeOffset(),
                DueDate = connectionRequest.DueDate?.ToRockDateTimeOffset(),
                CompletedDateTime = connectionRequest.ConnectedDateTime?.ToRockDateTimeOffset(),
                DueStatus = GetDueStatus(connectionRequest.DueDate, connectionRequest.DueSoonDate, connectionRequest.ConnectionState, connectionRequest.ConnectedDateTime ),
                Comments = connectionRequest.Comments,
                ConnectionTypeSource = connectionRequest.ConnectionTypeSource?.Name,
                CelebrationText = GetCelebrationText( connectionRequest.Id ),
                ActionItems = new List<ListItemBag>(),
                Attributes = connectionRequest.GetPublicAttributesForView( RequestContext.CurrentPerson ),
                AttributeValues = connectionRequest.GetPublicAttributeValuesForView( RequestContext.CurrentPerson ),
            };

            var reminderQry = new ReminderService( RockContext ).Queryable()
                .Include( r => r.PersonAlias )
                .AsNoTracking()
                .Where( r => !r.IsComplete
                    && r.ReminderDate < RockDateTime.Now
                    && r.PersonAlias.PersonId == RequestContext.CurrentPerson.Id
                    && r.EntityId == connectionRequest.PersonAliasId );

            detailsBag.ReminderCount = reminderQry.Count();

            var areGroupPlacementsEnabled = connectionRequest.ConnectionOpportunity.ConnectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.GroupPlacement );

            if ( areGroupPlacementsEnabled && connectionRequest.AssignedGroup != null )
            {
                var placementGroup = connectionRequest.AssignedGroup;

                detailsBag.PlacementGroup = new PlacementGroupDetailsBag
                {
                    ListItemBag = placementGroup.ToListItemBag(),
                    IconCssClass = placementGroup.GroupType.IconCssClass ?? "ti ti-users",
                    GroupId = placementGroup.Id.ToString()
                };

                var groupMember = placementGroup.Members.FirstOrDefault( m => m.PersonId == connectionRequest.PersonAlias.PersonId );

                if ( groupMember == null )
                {
                    detailsBag.PlacementGroup.IsPendingGroupMember = true;

                    var pendingGroupMember = new GroupMember
                    {
                        GroupId = placementGroup.Id
                    };

                    pendingGroupMember.LoadAttributes();
                    detailsBag.PlacementGroup.GroupMemberAttributes = pendingGroupMember.GetPublicAttributesForView( RequestContext.CurrentPerson );

                    var pendingGroupMemberAttributeValues = connectionRequest.AssignedGroupMemberAttributeValues.FromJsonOrNull<Dictionary<string, string>>();
                    detailsBag.PlacementGroup.GroupMemberAttributeValues = pendingGroupMemberAttributeValues;

                    var personGroupRequirementStatus = placementGroup.PersonMeetsGroupRequirements( RockContext, connectionRequest.PersonAlias.PersonId, connectionRequest.AssignedGroupMemberRoleId );

                    detailsBag.PlacementGroup.GroupMemberRequirements = new List<GroupMemberRequirementBag>( personGroupRequirementStatus.Select( s =>
                    {
                        var bag = new GroupMemberRequirementBag
                        {
                            GroupRequirementIdKey = s.GroupRequirement.IdKey,
                            RequirementName = s.GroupRequirement.GroupRequirementType.Name,
                            IsManualRequirement = s.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual,
                            GroupMemberRequirementState = s.MeetsGroupRequirement,
                            MustMeetRequirementToAddMember = s.GroupRequirement.MustMeetRequirementToAddMember
                        };

                        return bag;
                    } ).ToList() );
                }
                else
                {
                    groupMember.LoadAttributes();

                    detailsBag.PlacementGroup.IsPendingGroupMember = false;
                    detailsBag.PlacementGroup.GroupMemberIdKey = groupMember.IdKey;
                    detailsBag.PlacementGroup.GroupMemberAttributes = groupMember.GetPublicAttributesForView( RequestContext.CurrentPerson );
                    detailsBag.PlacementGroup.GroupMemberAttributeValues = groupMember.GetPublicAttributeValuesForView( RequestContext.CurrentPerson );

                    var groupMemberRequirementStatuses = groupMember.GetGroupRequirementsStatuses( RockContext );

                    detailsBag.PlacementGroup.GroupMemberRequirements = new List<GroupMemberRequirementBag>( groupMemberRequirementStatuses.Select( s =>
                    {
                        var bag = new GroupMemberRequirementBag
                        {
                            GroupRequirementIdKey = s.GroupRequirement.IdKey,
                            RequirementName = s.GroupRequirement.GroupRequirementType.Name,
                            IsManualRequirement = s.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual,
                            GroupMemberRequirementState = s.MeetsGroupRequirement,
                            MustMeetRequirementToAddMember = s.GroupRequirement.MustMeetRequirementToAddMember
                        };

                        if ( s.GroupMemberRequirementId.HasValue )
                        {
                            bag.GroupMemberRequirementIdKey = IdHasher.Instance.GetHash( s.GroupMemberRequirementId.Value );
                        }

                        return bag;
                    } ).ToList() );
                }
            }

            var manualConnectionWorkflows = new ConnectionWorkflowService( RockContext ).Queryable()
                .Include( w => w.WorkflowType.Category )
                .Where( w => w.TriggerType == ConnectionWorkflowTriggerType.Manual
                    && w.WorkflowTypeId != null
                    && ( w.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId || w.ConnectionTypeId == connectionRequest.ConnectionTypeId )
                    && ( w.ManualTriggerFilterConnectionStatus == null || w.ManualTriggerFilterConnectionStatusId == connectionRequest.ConnectionStatusId ) )
                .ToList();

            var workflowTypeOrder = connectionRequest.ConnectionOpportunity.GetAdditionalSettingsOrNull<List<int>>( "WorkflowTypeOrder" ) ?? new List<int>();

            var orderedManualWorkflows = manualConnectionWorkflows
                .OrderBy( w =>
                {
                    var index = workflowTypeOrder.IndexOf( w.WorkflowTypeId ?? -1 );
                    return index == -1 ? int.MaxValue : index;
                } )
                .ThenBy( w => w.WorkflowType.Name );

            foreach ( var workflow in orderedManualWorkflows )
            {
                if ( !( workflow.WorkflowType.IsActive ?? true ) || !workflow.WorkflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                List<int> includedDataViewValues = null;
                List<int> excludedDataViewValues = null;

                if ( workflow.IncludeDataViewId.HasValue )
                {
                    includedDataViewValues = GetDataViewValues( workflow.IncludeDataViewId.Value );
                }
                if ( workflow.ExcludeDataViewId.HasValue )
                {
                    excludedDataViewValues = GetDataViewValues( workflow.ExcludeDataViewId.Value );
                }

                if ( IsEligibleForWorkflow( workflow, connectionRequest, includedDataViewValues, excludedDataViewValues ) )
                {
                    detailsBag.ActionItems.Add( new ListItemBag
                    {
                        Text = workflow.WorkflowType?.Name,
                        Value = workflow.Guid.ToString()
                    } );
                }
            }

            var requesterPerson = connectionRequest.PersonAlias.Person;
            string connectionStatus = null;
            if ( requesterPerson.ConnectionStatusValueId.HasValue )
            {
                var connectionStatusValue = DefinedValueCache.Get( requesterPerson.ConnectionStatusValueId.Value );
                if ( connectionStatusValue != null )
                {
                    connectionStatus = connectionStatusValue.Value;
                }
            }
            string maritalStatus = null;
            if ( requesterPerson.MaritalStatusValueId.HasValue )
            {
                var maritalStatusValue = DefinedValueCache.Get( requesterPerson.MaritalStatusValueId.Value );
                if ( maritalStatusValue != null )
                {
                    maritalStatus = maritalStatusValue.Value;
                }
            }

            detailsBag.RequesterPerson = new RequesterPersonBag
            {
                IdKey = requesterPerson.IdKey,
                NickName = requesterPerson.NickName,
                LastName = requesterPerson.LastName,
                PhotoUrl = requesterPerson.PhotoUrl,
                ConnectionStatus = connectionStatus,
                Age = requesterPerson.Age,
                Gender = requesterPerson.Gender.ToString(),
                MaritalStatus = maritalStatus
            };

            var additionalRequestSettings = connectionRequest.ConnectionOpportunity.ConnectionType.GetConnectionTypeAdditionalSettings()?.AdditionalRequestsToShow;

            detailsBag.AdditionalRequests = GetAdditionalConnectionRequests( additionalRequestSettings, requesterPerson );

            detailsBag.PersonNotes = GetPersonNotesForPerson( connectionRequest.PersonAlias.PersonId );

            detailsBag.ActivityEntries = GetActivityEntries( connectionRequest, mergeFields );

            return detailsBag;
        }

        /// <summary>
        /// Gets a list of additional Connection Requests to display in the request detail panel,
        /// based on a list of configured display settings. Each setting defines a Connection Type,
        /// optional state filters, an optional recency cutoff, and whether to include requests
        /// from family members of the requester.
        /// </summary>
        /// <remarks>
        /// Each settings entry is queried independently and the results are combined via union.
        /// Returns an empty list if no settings are provided.
        /// </remarks>
        /// <param name="settingsList">The list of additional request display settings that define which Connection Types, states, and persons to include.</param>
        /// <param name="requesterPerson">The requester person whose Connection Requests (and optionally their family members') are retrieved.</param>
        /// <returns>A list of <see cref="AdditionalRequestBag"/> objects representing the matching Connection Requests across all configured settings.</returns>
        public List<AdditionalRequestBag> GetAdditionalConnectionRequests( List<AdditionalRequestToShowSettings> settingsList, Rock.Model.Person requesterPerson )
        {
            if ( settingsList == null || !settingsList.Any() )
            {
                return new List<AdditionalRequestBag>();
            }

            var familyMemberPersonIds = requesterPerson.GetFamilyMembers( includeSelf: true, RockContext )
                .Select( gm => gm.PersonId )
                .ToList();

            var connectionRequestService = new ConnectionRequestService( RockContext );

            IQueryable<ConnectionRequest> combinedQuery = null;

            foreach ( var setting in settingsList )
            {
                var query = connectionRequestService
                    .Queryable()
                    .Where( r => r.ConnectionOpportunity.ConnectionType.Guid == setting.ConnectionTypeGuid );

                if ( setting.StatesToShow.Any() )
                {
                    var states = setting.StatesToShow;
                    query = query.Where( r => states.Contains( r.ConnectionState ) );
                }

                if ( setting.LimitToRecentRequestsDays.HasValue )
                {
                    var cutoff = RockDateTime.Now.AddDays( -setting.LimitToRecentRequestsDays.Value );
                    query = query.Where( r => r.CreatedDateTime >= cutoff );
                }

                if ( setting.IncludeFamilyMemberRequests )
                {
                    query = query.Where( r => familyMemberPersonIds.Contains( r.PersonAlias.PersonId ) );
                }
                else
                {
                    query = query.Where( r => r.PersonAlias.PersonId == requesterPerson.Id );
                }

                combinedQuery = combinedQuery == null
                    ? query
                    : combinedQuery.Union( query );
            }

            var additionalRequestsProjection = combinedQuery.Select( r => new
            {
                RequestId = r.Id,
                ConnectionOpportunityId = r.ConnectionOpportunity.Id,
                ConnectionOpportunityName = r.ConnectionOpportunity.Name,
                ConnectionStatus = r.ConnectionStatus.Name,
                r.ConnectorPersonAlias,
                ConnectorNickName = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.NickName : string.Empty,
                ConnectorLastName = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.LastName : string.Empty,
                RequestCreatedDateTime = r.CreatedDateTime,
                RequesterNickName = r.PersonAlias.Person.NickName,
                RequesterLastName = r.PersonAlias.Person.LastName
            } ).ToList();

            return additionalRequestsProjection.Select( r => new AdditionalRequestBag
            {
                RequestIdKey = IdHasher.Instance.GetHash( r.RequestId ),
                ConnectionOpportunityIdKey = IdHasher.Instance.GetHash( r.ConnectionOpportunityId ),
                ConnectionOpportunityName = r.ConnectionOpportunityName,
                ConnectionStatus = r.ConnectionStatus,
                Connector = r.ConnectorPersonAlias == null ? "Unassigned" : r.ConnectorNickName + " " + r.ConnectorLastName,
                RequestCreatedDateTime = r.RequestCreatedDateTime?.ToRockDateTimeOffset(),
                Requester = r.RequesterNickName + " " + r.RequesterLastName
            } ).ToList();
        }

        /// <summary>
        /// Builds the chronologically ordered list of activity feed entries for the given Connection
        /// Request. Aggregates entries from four sources: Connection Request Activities, History records,
        /// related Communications (Email and SMS), and Request Notes.
        /// </summary>
        /// <remarks>
        /// If the Connection Type has the full activity list enabled, activities from all other
        /// Connection Requests of the same type for the same person are also included, with their
        /// originating opportunity and status indicated on the entry.
        /// History entries are retrieved via raw SQL for performance and mapped to a
        /// <see cref="SystemUpdateType"/> based on the verb and value name of each record.
        /// Bracketed IDs are stripped from history values using <see cref="StripBracketId"/> before display.
        /// Communication entries are grouped by communication ID to deduplicate attachment rows
        /// returned by the join, and SMS content is resolved against the provided merge fields.
        /// Only notes viewable and authorized for the current person are included.
        /// </remarks>
        /// <param name="connectionRequest">The Connection Request to retrieve activity entries for. Must have ConnectionRequestActivities and related entities loaded.</param>
        /// <param name="mergeFields">The Lava merge fields used to resolve SMS message content, typically obtained from <see cref="GetConnectionRequestDetailOptionsBag"/>.</param>
        /// <returns>A list of <see cref="ActivityEntryBag"/> objects representing all activity feed entries, ordered by entry date descending.</returns>
        private List<ActivityEntryBag> GetActivityEntries( ConnectionRequest connectionRequest, Dictionary<string, object> mergeFields )
        {
            var connectionRequestActivityService = new ConnectionRequestActivityService( RockContext );
            var connectionRequestActivities = connectionRequestActivityService.Queryable()
                .AsNoTracking()
                .Include( a => a.CreatedByPersonAlias.Person )
                .Include( a => a.ConnectorPersonAlias )
                .Include( a => a.ConnectionActivityType )
                .Where( a => a.ConnectionRequestId == connectionRequest.Id )
                .ToList();

            var entries = new List<ActivityEntryBag>();

            entries.AddRange( connectionRequestActivities.Select( a => new ActivityEntryBag
            {
                Key = $"{ActivityEntryType.Activity}_{IdHasher.Instance.GetHash( a.Id )}",
                EntryType = ActivityEntryType.Activity,
                EntryDateTime = a.CreatedDateTime?.ToRockDateTimeOffset(),
                CreatedBy = a.CreatedByPersonAlias?.Person?.FullName ?? "Rock",
                CardEntry = new CardEntryBag
                {
                    Title = string.Format( "Activity: {0}", a.ConnectionActivityType?.Name ),
                    Content = a.Note,
                    PhotoUrl = a.CreatedByPersonAlias?.Person?.PhotoUrl,
                    ActivityTypeGuid = a.ConnectionActivityType?.Guid.ToString(),
                    ActivityTypeName = a.ConnectionActivityType?.Name,
                    IsSystemActivityType = a.ConnectionActivityType != null && a.ConnectionActivityType.ConnectionTypeId == null,
                    ConnectorPersonAliasGuid = a.ConnectorPersonAlias?.Guid.ToString()
                }
            } ) );

            if ( connectionRequest.ConnectionOpportunity.ConnectionType.EnableFullActivityList )
            {
                var otherRequestActivities = connectionRequestActivityService.Queryable()
                    .AsNoTracking()
                    .Where( a => a.ConnectionRequest != null
                        && a.ConnectionRequest.PersonAlias != null
                        && a.ConnectionRequest.ConnectionTypeId == connectionRequest.ConnectionTypeId
                        && a.ConnectionRequestId != connectionRequest.Id
                        && a.ConnectionRequest.PersonAlias.PersonId == connectionRequest.PersonAlias.PersonId )
                    .Select( a => new
                    {
                        ActivityId = a.Id,
                        EntryDateTime = a.CreatedDateTime,
                        Content = a.Note,
                        CreatedByPerson = a.CreatedByPersonAlias != null ? a.CreatedByPersonAlias.Person : null,
                        ActivityTypeGuid = a.ConnectionActivityType.Guid,
                        ActivityTypeName = a.ConnectionActivityType.Name,
                        IsSystemActivityType = a.ConnectionActivityType != null && a.ConnectionActivityType.ConnectionTypeId == null,
                        ConnectorPersonAliasGuid = a.ConnectorPersonAlias != null ? a.ConnectorPersonAlias.Guid : ( Guid? ) null,
                        ConnectionRequestId = a.ConnectionRequestId,
                        ConnectionOpportunityId = a.ConnectionRequest.ConnectionOpportunityId,
                        ConnectionOpportunityName = a.ConnectionRequest.ConnectionOpportunity.Name,
                        ConnectionStatusName = a.ConnectionRequest.ConnectionStatus.Name
                    } )
                    .ToList();

                entries.AddRange( otherRequestActivities.Select( a => new ActivityEntryBag
                {
                    Key = $"{ActivityEntryType.Activity}_{IdHasher.Instance.GetHash( a.ActivityId )}",
                    EntryType = ActivityEntryType.Activity,
                    EntryDateTime = a.EntryDateTime?.ToRockDateTimeOffset(),
                    CreatedBy = a.CreatedByPerson?.FullName ?? "Rock",
                    CardEntry = new CardEntryBag
                    {
                        Title = string.Format( "Activity: {0}", a.ActivityTypeName ),
                        Content = a.Content,
                        PhotoUrl = a.CreatedByPerson?.PhotoUrl,
                        ActivityTypeGuid = a.ActivityTypeGuid.ToString(),
                        ActivityTypeName = a.ActivityTypeName,
                        IsSystemActivityType = a.IsSystemActivityType,
                        ConnectorPersonAliasGuid = a.ConnectorPersonAliasGuid.HasValue ? a.ConnectorPersonAliasGuid.Value.ToString() : string.Empty,
                        ConnectionRequestIdKey = IdHasher.Instance.GetHash( a.ConnectionRequestId ),
                        ConnectionOpportunityIdKey = IdHasher.Instance.GetHash( a.ConnectionOpportunityId ),
                        ConnectionOpportunityName = a.ConnectionOpportunityName,
                        ConnectionRequestStatus = a.ConnectionStatusName
                    }
                } ) );
            }

            var categoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_CONNECTION_REQUEST.AsGuid() ).Id;
            var connectionRequestEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.CONNECTION_REQUEST.AsGuid() ).Id;
            var communicationEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION.AsGuid() ).Id;

            var historySQL = @"
SELECT
    h.[Id],
    LTRIM(RTRIM(CONCAT(COALESCE(p.[NickName], ''), ' ', COALESCE(p.[LastName], '')))) AS CreatedBy,
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
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter( SqlParamKey.ConnectionRequestEntityTypeId, connectionRequestEntityTypeId ),
                new SqlParameter( SqlParamKey.RequestId, connectionRequest.Id ),
                new SqlParameter( SqlParamKey.CategoryId, categoryId ),
            };

            var historyRows = RockContext.Database
                .SqlQuery<HistoryRow>( historySQL, sqlParams.ToArray() )
                .ToList();

            var communicationSQL = @"
SELECT
	c.[Id],
	c.[CommunicationType],
	c.[Subject],
	c.[SMSMessage],
	c.[CreatedDateTime],
	p.[NickName],
	p.[LastName],
	p.[PhotoId],
	p.[Age],
	p.[Gender],
	p.[RecordTypeValueId],
	p.[AgeClassification],
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
	AND re.[TargetEntityTypeId] = @TargetEntityTypeId
";

            sqlParams = new List<SqlParameter>
            {
                new SqlParameter( SqlParamKey.SourceEntityTypeId, connectionRequestEntityTypeId ),
                new SqlParameter( SqlParamKey.SourceEntityId, connectionRequest.Id ),
                new SqlParameter( SqlParamKey.TargetEntityTypeId, communicationEntityTypeId ),
            };

            var communicationRows = RockContext.Database
                .SqlQuery<CommunicationRow>( communicationSQL, sqlParams.ToArray() )
                .ToList();

            entries.AddRange( historyRows.Select( r =>
            {
                SystemUpdateType systemUpdateType = SystemUpdateType.Creation;
                var createdBy = r.CreatedBy;
                var previousValue = StripBracketId( r.OldValue );
                var newValue = StripBracketId( r.NewValue );

                if ( r.Verb == "Add" )
                {
                    systemUpdateType = SystemUpdateType.Creation;
                }
                else
                {
                    switch ( r.ValueName )
                    {
                        case "Connector":
                            if ( r.NewValue.IsNotNullOrWhiteSpace() && r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = SystemUpdateType.Reassignment;
                            }
                            else if ( r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = SystemUpdateType.Unassignment;
                            }
                            else
                            {
                                systemUpdateType = SystemUpdateType.Assignment;
                            }
                            break;
                        case "ConnectionStatus":
                            if ( r.NewValue.IsNotNullOrWhiteSpace() && r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = SystemUpdateType.StatusUpdated;
                            }
                            else if ( r.OldValue.IsNotNullOrWhiteSpace() )
                            {
                                systemUpdateType = SystemUpdateType.StatusCleared;
                            }
                            else
                            {
                                systemUpdateType = SystemUpdateType.StatusSet;
                            }
                            break;
                        case "ConnectionState":
                            if ( newValue == ConnectionState.Connected.ToString() )
                            {
                                systemUpdateType = SystemUpdateType.Completion;
                            }
                            else
                            {
                                systemUpdateType = SystemUpdateType.StateChange;

                                if ( Enum.TryParse( previousValue, out ConnectionState previousConnectionState ) )
                                {
                                    previousValue = previousConnectionState.GetDisplayName();
                                }

                                if ( Enum.TryParse( newValue, out ConnectionState newConnectionState ) )
                                {
                                    newValue = newConnectionState.GetDisplayName();
                                }
                            }
                            break;
                        case "DueDate":
                            systemUpdateType = SystemUpdateType.DueDateChange;
                            break;
                        case "DueSoonDate":
                            systemUpdateType = SystemUpdateType.DueSoonDateChange;
                            break;
                    }
                }

                // Get unique key.
                var key = $"{ActivityEntryType.SystemUpdate}_{IdHasher.Instance.GetHash( r.Id )}";

                return new ActivityEntryBag
                {
                    Key = key,
                    EntryType = ActivityEntryType.SystemUpdate,
                    EntryDateTime = r.CreatedDateTime?.ToRockDateTimeOffset(),
                    CreatedBy = createdBy,
                    SystemUpdate = new SystemUpdateBag
                    {
                        SystemUpdateType = systemUpdateType,
                        PreviousValue = previousValue,
                        NewValue = newValue
                    }
                };
            } ) );

            entries.AddRange( communicationRows.Where( r => r.CommunicationType == CommunicationType.Email || r.CommunicationType == CommunicationType.SMS )
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
                    r.Age,
                    r.Gender,
                    r.RecordTypeValueId,
                    r.AgeClassification
                } ).Select( g =>
                {
                    string photoUrl = null;
                    string title = null;
                    string content = null;
                    string createdBy = $"{g.Key.NickName ?? ""} {g.Key.LastName ?? ""}".Trim();

                    if ( createdBy.IsNullOrWhiteSpace() )
                    {
                        createdBy = "Unknown Person";
                        photoUrl = Rock.Model.Person.GetPersonNoPictureUrl( new Rock.Model.Person() );
                    }
                    else
                    {
                        string initials = $"{g.Key.NickName?.Truncate( 1, false )}{g.Key.LastName?.Truncate( 1, false )}";
                        photoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                            initials,
                            g.Key.PhotoId,
                            g.Key.Age,
                            g.Key.Gender ?? Gender.Unknown,
                            g.Key.RecordTypeValueId,
                            g.Key.AgeClassification
                        );
                    }

                    if ( g.Key.CommunicationType == CommunicationType.SMS )
                    {
                        title = "SMS";
                        content = g.Key.SMSMessage.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        title = $"Email: {g.Key.Subject}";
                    }

                    // Get unique key.
                    var key = $"{ActivityEntryType.Communication}_{IdHasher.Instance.GetHash( g.Key.Id )}";

                    return new ActivityEntryBag
                    {
                        Key = key,
                        EntryType = ActivityEntryType.Communication,
                        EntryDateTime = g.Key.CreatedDateTime?.ToRockDateTimeOffset(),
                        CreatedBy = createdBy,
                        CardEntry = new CardEntryBag
                        {
                            Title = title,
                            Content = content,
                            PhotoUrl = photoUrl,
                            Attachments = g.Where( x => x.BinaryFileGuid.HasValue )
                                .DistinctBy( x => x.BinaryFileGuid.Value )
                                .Select( x => new ListItemBag
                                {
                                    Value = x.BinaryFileGuid.Value.ToString(),
                                    Text = x.FileName
                                } )
                                .ToList()
                        }
                    };
                } ) );

            var connectionRequestNoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE.AsGuid() ).Id;
            var noteService = new NoteService( RockContext );

            var connectionRequestNoteQry = noteService.Queryable()
                .Where( n => n.NoteTypeId == connectionRequestNoteTypeId && n.EntityId == connectionRequest.Id );

            connectionRequestNoteQry = connectionRequestNoteQry.AreViewableBy( RequestContext.CurrentPerson.Id );

            var connectionRequestNotes = connectionRequestNoteQry.ToList()
                .Where( n => n.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) );

            entries.AddRange( connectionRequestNotes.Select( n => new ActivityEntryBag
            {
                Key = $"{ActivityEntryType.RequestNote}_{IdHasher.Instance.GetHash( n.Id )}",
                EntryType = ActivityEntryType.RequestNote,
                EntryDateTime = n.CreatedDateTime?.ToRockDateTimeOffset(),
                CreatedBy = n.CreatedByPersonAlias?.Person?.FullName,
                CardEntry = new CardEntryBag
                {
                    Title = "Request Note",
                    Content = n.Text,
                    PhotoUrl = n.CreatedByPersonAlias?.Person?.PhotoUrl
                }
            } ) );

            return new List<ActivityEntryBag>( entries.OrderByDescending( e => e.EntryDateTime ) );
        }

        /// <summary>
        /// Gets the list of Person Notes for the specified person that are viewable
        /// by the current user, used to display the person's notes in the Connection
        /// Request detail panel.
        /// </summary>
        /// <param name="personId">The ID of the person whose notes should be retrieved.</param>
        /// <returns>A list of <see cref="PersonNoteBag"/> objects representing the viewable notes for the person, ordered by the underlying query's default ordering.</returns>
        private List<PersonNoteBag> GetPersonNotesForPerson( int personId )
        {
            var personEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.PERSON ).Id;

            var noteQry = new NoteService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( n => n.NoteType )
                .Include( n => n.CreatedByPersonAlias.Person )
                .Where( n => n.NoteType.EntityTypeId == personEntityTypeId && n.EntityId == personId );

            noteQry = noteQry.AreViewableBy( RequestContext.CurrentPerson.Id );

            var personNotes = noteQry.ToList()
                .Where( n => n.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( n => new PersonNoteBag
                {
                    IdKey = n.IdKey,
                    CreatedByPhotoUrl = n.CreatedByPersonAlias?.Person.PhotoUrl,
                    NoteTypeName = n.NoteType.Name,
                    CreatedByName = n.CreatedByPersonAlias?.Person.FullName,
                    CreatedDateTime = n.CreatedDateTime?.ToRockDateTimeOffset(),
                    IsAlert = n.IsAlert ?? false,
                    IsPinned = n.IsPinned,
                    Text = n.Text,
                } ).ToList();

            return personNotes;
        }

        #endregion UI Refresh Helpers

        private string AttachAIPromptContext( string prompt, ConnectionRequest connectionRequest )
        {
            var sb = new StringBuilder();

            sb.AppendLine( $"Requester: {connectionRequest.PersonAlias?.Person?.FullName ?? "Unknown"}" );
            sb.AppendLine( $"Connector: {connectionRequest.ConnectorPersonAlias?.Person?.FullName ?? "Unassigned"}" );
            sb.AppendLine( $"Opportunity: {connectionRequest.ConnectionOpportunity?.Name}" );
            sb.AppendLine( $"Status: {connectionRequest.ConnectionStatus?.Name}" );
            sb.AppendLine( $"State: {connectionRequest.ConnectionState.GetDisplayName()}" );

            var dueStatus = GetDueStatus( connectionRequest.DueDate, connectionRequest.DueSoonDate, connectionRequest.ConnectionState, connectionRequest.ConnectedDateTime );
            sb.AppendLine( $"Due Status: {dueStatus.GetDisplayName()}" );

            if ( connectionRequest.CreatedDateTime.HasValue )
            {
                sb.AppendLine( $"Created Date: {connectionRequest.CreatedDateTime}" );
            }

            if ( connectionRequest.Campus != null )
            {
                sb.AppendLine( $"Campus: {connectionRequest.Campus.Name}" );
            }

            if ( connectionRequest.ConnectionOpportunity.ConnectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.GroupPlacement ) && connectionRequest.AssignedGroup != null )
            {
                sb.AppendLine( $"Placement Group: {connectionRequest.AssignedGroup.Name}" );
            }

            if ( connectionRequest.DueDate.HasValue )
            {
                sb.AppendLine( $"Due Date: {connectionRequest.DueDate}" );
            }

            if ( connectionRequest.DueSoonDate.HasValue )
            {
                sb.AppendLine( $"Due Soon Date: {connectionRequest.DueSoonDate}" );
            }

            if ( connectionRequest.ConnectionTypeSourceId.HasValue && connectionRequest.ConnectionTypeSource != null )
            {
                sb.AppendLine( $"Source of Request: {connectionRequest.ConnectionTypeSource.Name}" );
            }

            if ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
            {
                sb.AppendLine( $"Follow-Up Date: {connectionRequest.FollowupDate}" );
            }

            if ( connectionRequest.ConnectionState == ConnectionState.Connected )
            {
                sb.AppendLine( $"Completed Date: {connectionRequest.ConnectedDateTime}" );
                sb.AppendLine( $"Was Completed On Time: {connectionRequest.WasCompletedOnTime}" );
            }

            var celebrationText = GetCelebrationText( connectionRequest.Id );
            if ( !celebrationText.IsNullOrWhiteSpace() )
            {
                sb.AppendLine( $"Celebration: {celebrationText}" );
            }

            if ( !connectionRequest.Comments.IsNullOrWhiteSpace() )
            {
                sb.AppendLine( $"Comments: {connectionRequest.Comments}" );
            }

            var activities = connectionRequest.ConnectionRequestActivities
                .Where( a => a.CreatedByPersonAliasId.HasValue )
                .OrderByDescending( a => a.CreatedDateTime )
                .Take( 5 )
                .ToList();

            if ( activities.Any() )
            {
                sb.AppendLine( "\nActivities:" );
                foreach ( var activity in activities )
                {
                    sb.AppendLine( $"- [{activity.ConnectionActivityType?.Name}] by {activity.CreatedByPersonAlias?.Person?.FullName}: {activity.Note}" );
                }
            }

            return $"Based on the following connection request data, write a 2-3 sentence human-friendly response following the instructions of the prompt. Prompt: {prompt}\n\n{sb}";
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the grid data for the Connections Hub using a plain SQL query for improved
        /// performance. A pre-aggregated derived table replaces the correlated per-row subqueries
        /// for activity counts that EF6 emits, reducing total query cost significantly for large
        /// data sets.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            ConnectionType connectionType;

            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).GetInclude( PageParameter( PageParameterKey.ConnectionOpportunity ), o => o.ConnectionType, !PageCache.Layout.Site.DisablePredictableIds );

            if ( connectionOpportunity != null )
            {
                connectionType = connectionOpportunity.ConnectionType;
            }
            else
            {
                connectionType = new ConnectionTypeService( RockContext ).GetInclude( PageParameter( PageParameterKey.ConnectionType ), a => a.ConnectionStatuses, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( connectionType == null )
            {
                return ActionOk();
            }

            var celebrationNoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() ).Id;

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter( "@ConnectionTypeId", connectionType.Id ),
                new SqlParameter( "@CurrentPersonId", RequestContext.CurrentPerson.Id ),
                new SqlParameter( "@Now", RockDateTime.Now ),
                new SqlParameter( "@CelebrationNoteTypeId", celebrationNoteTypeId )
            };

            var sql = new System.Text.StringBuilder( @"
SELECT
    cr.[Id]                                         AS [Id],
    co.[Id]                                         AS [ConnectionOpportunityId],
    co.[Guid]                                       AS [ConnectionOpportunityGuid],
    co.[Name]                                       AS [ConnectionOpportunityName],
    co.[IconCssClass]                               AS [ConnectionOpportunityIconCssClass],
    co.[Order]                                      AS [ConnectionOpportunityOrder],
    cts.[Name]                                      AS [ConnectionTypeSourceName],
    cr.[CampusId]                                   AS [CampusId],
    cam.[Name]                                      AS [CampusName],
    cam.[Guid]                                      AS [CampusGuid],
    cam.[Order]                                     AS [CampusOrder],
    cr.[AssignedGroupId]                            AS [AssignedGroupId],
    ag.[Name]                                       AS [AssignedGroupName],
    cs.[Id]                                         AS [ConnectionStatusId],
    cs.[Guid]                                       AS [ConnectionStatusGuid],
    cs.[Name]                                       AS [ConnectionStatusName],
    cs.[Order]                                      AS [ConnectionStatusOrder],
    cs.[HighlightColor]                             AS [ConnectionStatusHighlightColor],
    cs.[IsNoteRequiredOnCompletion]                 AS [ConnectionStatusIsNoteRequiredOnCompletion],
    cs.[IsDefault]                                  AS [ConnectionStatusIsDefault],
    cr.[ConnectionState]                            AS [ConnectionState],
    cr.[FollowupDate]                               AS [FollowUpDate],
    cr.[CreatedDateTime]                            AS [CreatedDateTime],
    cr.[DueDate]                                    AS [DueDate],
    cr.[DueSoonDate]                                AS [DueSoonDate],
    cr.[ConnectedDateTime]                          AS [ConnectedDateTime],
    cel_note.[Text]                                 AS [CelebrationText],
    cr.[PersonAliasId]                              AS [PersonAliasId],
    rpa.[Guid]                                      AS [RequesterPersonAliasGuid],
    rp.[Id]                                         AS [RequesterPersonId],
    rp.[NickName]                                   AS [RequesterNickName],
    rp.[LastName]                                   AS [RequesterLastName],
    rp.[PhotoId]                                    AS [RequesterPhotoId],
    rp.[Age]                                        AS [RequesterAge],
    rp.[Gender]                                     AS [RequesterGender],
    rp.[RecordTypeValueId]                          AS [RequesterRecordTypeValueId],
    rp.[AgeClassification]                          AS [RequesterAgeClassification],
    rp.[ConnectionStatusValueId]                    AS [RequesterConnectionStatusValueId],
    cr.[ConnectorPersonAliasId]                     AS [ConnectorPersonAliasId],
    cpa.[Guid]                                      AS [ConnectorPersonAliasGuid],
    cp.[Id]                                         AS [ConnectorPersonId],
    cp.[NickName]                                   AS [ConnectorNickName],
    cp.[LastName]                                   AS [ConnectorLastName],
    cp.[PhotoId]                                    AS [ConnectorPhotoId],
    cp.[Age]                                        AS [ConnectorAge],
    cp.[Gender]                                     AS [ConnectorGender],
    cp.[RecordTypeValueId]                          AS [ConnectorRecordTypeValueId],
    cp.[AgeClassification]                          AS [ConnectorAgeClassification],
    cp.[ConnectionStatusValueId]                    AS [ConnectorConnectionStatusValueId],
    COALESCE(cra_agg.[ActivityCount], 0)            AS [ActivityCount],
    cra_agg.[LastActivityDateTime]                  AS [LastActivityDateTime],
    COALESCE(rem_agg.[ReminderCount], 0)            AS [ReminderCount],
    CAST(CASE WHEN grp_req.[GroupId] IS NOT NULL THEN 1 ELSE 0 END AS BIT)
                                                    AS [HasRequiredGroupRequirements]
FROM [ConnectionRequest] cr
INNER JOIN [ConnectionOpportunity] co
    ON co.[Id] = cr.[ConnectionOpportunityId]
    AND co.[IsActive] = 1
    AND co.[ConnectionTypeId] = @ConnectionTypeId
INNER JOIN [ConnectionStatus] cs
    ON cs.[Id] = cr.[ConnectionStatusId]
    AND cs.[IsActive] = 1
LEFT JOIN [Campus] cam
    ON cam.[Id] = cr.[CampusId]
LEFT JOIN [ConnectionTypeSource] cts
    ON cts.[Id] = cr.[ConnectionTypeSourceId]
LEFT JOIN [Group] ag
    ON ag.[Id] = cr.[AssignedGroupId]
LEFT JOIN (
    SELECT DISTINCT [GroupId]
    FROM [GroupRequirement]
    WHERE [MustMeetRequirementToAddMember] = 1
      AND [GroupId] IS NOT NULL
) grp_req
    ON grp_req.[GroupId] = cr.[AssignedGroupId]
LEFT JOIN [PersonAlias] cpa
    ON cpa.[Id] = cr.[ConnectorPersonAliasId]
LEFT JOIN [Person] cp
    ON cp.[Id] = cpa.[PersonId]
INNER JOIN [PersonAlias] rpa
    ON rpa.[Id] = cr.[PersonAliasId]
INNER JOIN [Person] rp
    ON rp.[Id] = rpa.[PersonId]
LEFT JOIN (
    SELECT [ConnectionRequestId],
           COUNT(1)               AS [ActivityCount],
           MAX([CreatedDateTime]) AS [LastActivityDateTime]
    FROM [ConnectionRequestActivity]
    GROUP BY [ConnectionRequestId]
) cra_agg
    ON cra_agg.[ConnectionRequestId] = cr.[Id]
LEFT JOIN (
    SELECT rem.[EntityId], COUNT(1) AS [ReminderCount]
    FROM [Reminder] rem
    INNER JOIN [PersonAlias] rem_pa ON rem_pa.[Id] = rem.[PersonAliasId]
    WHERE rem.[IsComplete] = 0
      AND rem.[ReminderDate] < @Now
      AND rem_pa.[PersonId] = @CurrentPersonId
    GROUP BY rem.[EntityId]
) rem_agg
    ON rem_agg.[EntityId] = cr.[PersonAliasId]
OUTER APPLY (
    SELECT TOP 1 [Text]
    FROM [Note]
    WHERE [NoteTypeId] = @CelebrationNoteTypeId
      AND [EntityId] = cr.[Id]
) cel_note
WHERE 1 = 1" );

            // Campus context filter.
            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                sql.Append( "\n  AND cr.[CampusId] = @CampusId" );
                sqlParams.Add( new SqlParameter( "@CampusId", campusContext.Id ) );
            }

            // Opportunity and state filters come from person preferences keyed by connection type.
            var connectionTypeIdKey = IdHasher.Instance.GetHash( connectionType.Id );

            // Connection Opportunity Filter
            var connectionOpportunityFilter = GetConnectionOpportunityFilter( connectionTypeIdKey );
            if ( connectionOpportunityFilter.HasValue )
            {
                sql.Append( "\n  AND co.[Guid] = @OpportunityGuid" );
                sqlParams.Add( new SqlParameter( "@OpportunityGuid", connectionOpportunityFilter.Value ) );
            }

            // Connection State Filter
            var stateFilter = GetStateFilter( connectionTypeIdKey );
            if ( stateFilter.Count > 0 )
            {
                var statePlaceholders = stateFilter
                    .Select( ( _, i ) => $"@state{i}" )
                    .ToList();

                sql.Append( $"\n  AND cr.[ConnectionState] IN ({string.Join( ", ", statePlaceholders )})" );

                for ( var i = 0; i < stateFilter.Count; i++ )
                {
                    sqlParams.Add( new SqlParameter( $"@state{i}", ( int ) stateFilter[i] ) );
                }
            }

            // Connector filter
            if ( PageParameter( PageParameterKey.Connector ).IsNotNullOrWhiteSpace() && SelectedConnector.HasValue )
            {
                sql.Append( "\n  AND cpa.[Guid] = @ConnectorGuid" );
                sqlParams.Add( new SqlParameter( "@ConnectorGuid", SelectedConnector.Value ) );
            }
            else if ( AreOnlyMyRequestsVisible )
            {
                // @CurrentPersonId is already in sqlParams for the reminder subquery.
                sql.Append( "\n  AND cp.[Id] = @CurrentPersonId" );
            }

            var sqlRows = RockContext.Database
                .SqlQuery<ConnectionRequestSqlRow>( sql.ToString(), sqlParams.ToArray() )
                .ToList();

            var connectionRequests = new List<ConnectionRow>( sqlRows.Count );

            var photoUrlByPersonId = new Dictionary<int, string>();
            var connectorByPersonId = new Dictionary<int, (ListItemBag Item, string FullName, string PhotoUrl)>();
            var connectorGroupingByPersonAliasId = new Dictionary<int, GroupingFieldBag>();
            var opportunityGroupingById = new Dictionary<int, GroupingFieldBag>();
            var campusGroupingById = new Dictionary<int, GroupingFieldBag>();
            var statusGroupingById = new Dictionary<int, GroupingFieldBag>();
            var dueStatusGroupingByValue = new Dictionary<DueStatus, GroupingFieldBag>();
            var stateGroupingByValue = new Dictionary<ConnectionState, GroupingFieldBag>();

            var unassignedConnectorItem = new ListItemBag { Value = "unassigned", Text = "Unassigned" };
            var unassignedConnectorGrouping = GetGroupingFieldBag( null, "person", string.Empty, null, null, string.Empty );
            var noCampusGrouping = GetGroupingFieldBag( null, "text", string.Empty, null );

            foreach ( var row in sqlRows )
            {
                var connectionState = ( ConnectionState ) row.ConnectionState;

                var request = new ConnectionRow
                {
                    ConnectionRequestId = row.Id,
                    ConnectionOpportunityId = row.ConnectionOpportunityId,
                    ConnectionOpportunityGuid = row.ConnectionOpportunityGuid,
                    ConnectionOpportunity = row.ConnectionOpportunityName,
                    ConnectionOpportunityIcon = row.ConnectionOpportunityIconCssClass,
                    ConnectionTypeSource = row.ConnectionTypeSourceName ?? string.Empty,
                    CampusId = row.CampusId,
                    Campus = row.CampusName ?? string.Empty,
                    CampusGuid = row.CampusGuid,
                    AssignedGroupId = row.AssignedGroupId,
                    Group = row.AssignedGroupName ?? string.Empty,
                    ConnectionState = connectionState,
                    FollowUpDate = row.FollowUpDate,
                    CreatedDateTime = row.CreatedDateTime,
                    DueDate = row.DueDate,
                    DueSoonDate = row.DueSoonDate,
                    CompletedDateTime = row.ConnectedDateTime,
                    CelebrationText = row.CelebrationText,
                    PersonAliasId = row.PersonAliasId,
                    RequesterPersonAliasGuid = row.RequesterPersonAliasGuid,
                    ActivityCount = row.ActivityCount,
                    LastActivityDateTime = row.LastActivityDateTime,
                    ConnectorPersonAliasGuid = row.ConnectorPersonAliasGuid,
                    HasPlacementGroup = row.AssignedGroupId.HasValue,
                    HasRequiredGroupRequirements = false,
                    ReminderCount = row.ReminderCount,
                    ConnectionStatus = new ConnectionStatusBag
                    {
                        Guid = row.ConnectionStatusGuid,
                        Name = row.ConnectionStatusName,
                        Order = row.ConnectionStatusOrder,
                        HighlightColor = row.ConnectionStatusHighlightColor,
                        IsNoteRequiredOnCompletion = row.ConnectionStatusIsNoteRequiredOnCompletion,
                        IsDefaultStatus = row.ConnectionStatusIsDefault
                    }
                };

                var requesterIdKey = IdHasher.Instance.GetHash( row.RequesterPersonId );

                request.Person = new PersonFieldBag
                {
                    IdKey = requesterIdKey,
                    NickName = row.RequesterNickName,
                    LastName = row.RequesterLastName
                };

                if ( !photoUrlByPersonId.TryGetValue( row.RequesterPersonId, out var requesterPhotoUrl ) )
                {
                    var initials = $"{row.RequesterNickName.Truncate( 1, false )}{row.RequesterLastName.Truncate( 1, false )}";
                    requesterPhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                        initials,
                        row.RequesterPhotoId,
                        row.RequesterAge,
                        row.RequesterGender.HasValue ? ( Gender ) row.RequesterGender.Value : Gender.Unknown,
                        row.RequesterRecordTypeValueId,
                        row.RequesterAgeClassification.HasValue ? ( AgeClassification ) row.RequesterAgeClassification.Value : ( AgeClassification? ) null
                    );
                    photoUrlByPersonId[row.RequesterPersonId] = requesterPhotoUrl;
                }

                request.Person.PhotoUrl = requesterPhotoUrl;

                if ( row.RequesterConnectionStatusValueId.HasValue )
                {
                    var connectionStatusValue = DefinedValueCache.Get( row.RequesterConnectionStatusValueId.Value );
                    if ( connectionStatusValue != null )
                    {
                        request.Person.ConnectionStatus = connectionStatusValue.Value;
                    }
                }

                ListItemBag connectorItem;
                string connectorFullName;
                string connectorPhotoUrl;

                if ( row.ConnectorPersonId.HasValue )
                {
                    if ( !connectorByPersonId.TryGetValue( row.ConnectorPersonId.Value, out var connectorData ) )
                    {
                        var fullName = Rock.Model.Person.FormatFullName( row.ConnectorNickName, row.ConnectorLastName, null );
                        var connectorIdKey = IdHasher.Instance.GetHash( row.ConnectorPersonId.Value );
                        var connectorInitials = $"{row.ConnectorNickName.Truncate( 1, false )}{row.ConnectorLastName.Truncate( 1, false )}";
                        var connectorPhoto = Rock.Model.Person.GetPersonPhotoUrl(
                            connectorInitials,
                            row.ConnectorPhotoId,
                            row.ConnectorAge,
                            row.ConnectorGender.HasValue ? ( Gender ) row.ConnectorGender.Value : Gender.Unknown,
                            row.ConnectorRecordTypeValueId,
                            row.ConnectorAgeClassification.HasValue ? ( AgeClassification ) row.ConnectorAgeClassification.Value : ( AgeClassification? ) null
                        );

                        connectorData = (
                            Item: new ListItemBag { Value = connectorIdKey, Text = fullName },
                            FullName: fullName,
                            PhotoUrl: connectorPhoto
                        );
                        connectorByPersonId[row.ConnectorPersonId.Value] = connectorData;
                    }

                    connectorItem = connectorData.Item;
                    connectorFullName = connectorData.FullName;
                    connectorPhotoUrl = connectorData.PhotoUrl;
                }
                else
                {
                    connectorItem = unassignedConnectorItem;
                    connectorFullName = string.Empty;
                    connectorPhotoUrl = string.Empty;
                }

                request.HasRequiredGroupRequirements = row.HasRequiredGroupRequirements;

                var dueStatus = GetDueStatus( row.DueDate, row.DueSoonDate, connectionState, row.ConnectedDateTime );
                request.DueStatus = dueStatus;
                request.ConnectorDetails = connectorItem;

                if ( row.ConnectorPersonId.HasValue )
                {
                    if ( !connectorGroupingByPersonAliasId.TryGetValue( row.ConnectorPersonAliasId.Value, out var connectorGrouping ) )
                    {
                        connectorGrouping = GetGroupingFieldBag( row.ConnectorPersonAliasId, "person", connectorFullName, null, null, connectorPhotoUrl );
                        connectorGroupingByPersonAliasId[row.ConnectorPersonAliasId.Value] = connectorGrouping;
                    }

                    request.ConnectorGrouping = connectorGrouping;
                }
                else
                {
                    request.ConnectorGrouping = unassignedConnectorGrouping;
                }

                if ( !opportunityGroupingById.TryGetValue( row.ConnectionOpportunityId, out var opportunityGrouping ) )
                {
                    opportunityGrouping = GetGroupingFieldBag( row.ConnectionOpportunityId, "text", row.ConnectionOpportunityName, row.ConnectionOpportunityOrder, row.ConnectionOpportunityIconCssClass );
                    opportunityGroupingById[row.ConnectionOpportunityId] = opportunityGrouping;
                }

                request.OpportunityGrouping = opportunityGrouping;

                if ( row.CampusId.HasValue )
                {
                    if ( !campusGroupingById.TryGetValue( row.CampusId.Value, out var campusGrouping ) )
                    {
                        campusGrouping = GetGroupingFieldBag( row.CampusId, "text", row.CampusName ?? string.Empty, row.CampusOrder );
                        campusGroupingById[row.CampusId.Value] = campusGrouping;
                    }

                    request.CampusGrouping = campusGrouping;
                }
                else
                {
                    request.CampusGrouping = noCampusGrouping;
                }

                if ( !statusGroupingById.TryGetValue( row.ConnectionStatusId, out var statusGrouping ) )
                {
                    statusGrouping = GetGroupingFieldBag( row.ConnectionStatusId, "text", row.ConnectionStatusName, row.ConnectionStatusOrder );
                    statusGroupingById[row.ConnectionStatusId] = statusGrouping;
                }

                request.StatusGrouping = statusGrouping;

                if ( !dueStatusGroupingByValue.TryGetValue( dueStatus, out var dueStatusGrouping ) )
                {
                    dueStatusGrouping = GetGroupingFieldBag( ( int ) dueStatus, "text", dueStatus.GetDisplayName(), dueStatus.GetOrder(), "ti ti-calendar", null, GetDueStatusTextColorCssClass( dueStatus ) );
                    dueStatusGroupingByValue[dueStatus] = dueStatusGrouping;
                }

                request.DueStatusGrouping = dueStatusGrouping;

                if ( !stateGroupingByValue.TryGetValue( connectionState, out var stateGrouping ) )
                {
                    stateGrouping = new GroupingFieldBag
                    {
                        Key = connectionState.ToString(),
                        Type = "text",
                        Label = connectionState.GetDisplayName(),
                        IconCssClass = GetStateIconCssClass( connectionState ),
                        Order = ( int ) connectionState
                    };
                    stateGroupingByValue[connectionState] = stateGrouping;
                }

                request.StateGrouping = stateGrouping;

                connectionRequests.Add( request );
            }

            // Load attribute values for any grid-configured attributes. When the attribute
            // list is empty this is a no-op. When attributes are present we fetch only the
            // raw ConnectionRequest entities needed for Attribute Value hydration rather than
            // re-running the full projection query.
            var gridAttributes = GetGridAttributes();
            if ( gridAttributes.Count > 0 )
            {
                var connectionRequestIds = connectionRequests
                    .Select( r => r.ConnectionRequestId )
                    .ToList();

                var connectionRequestEntities = new ConnectionRequestService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( cr => connectionRequestIds.Contains( cr.Id ) )
                    .ToDictionary( cr => cr.Id );

                foreach ( var request in connectionRequests )
                {
                    if ( connectionRequestEntities.TryGetValue( request.ConnectionRequestId, out var entity ) )
                    {
                        request.ConnectionRequest = entity;
                    }
                }
            }

            GridAttributeLoader.LoadFor( connectionRequests, a => a.ConnectionRequest, gridAttributes, RockContext );

            var gridDataBag = GetGridBuilder().Build( connectionRequests );
            return ActionOk( gridDataBag );
        }

        /// <summary>
        /// Gets a boolean value indicating whether the current user can edit the Connection Requests.
        /// </summary>
        /// <param name="connectionTypeIdKey">The Connection Type IdKey</param>
        /// <returns>true if the user can edit</returns>
        [BlockAction]
        public BlockActionResult RefreshUserEditPermissions( string connectionTypeIdKey )
        {
            var connectionType = new ConnectionTypeService( RockContext ).Get( connectionTypeIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            ConnectionOpportunity connectionOpportunity = null;
            var connectionOpportunityFilter = GetConnectionOpportunityFilter( connectionTypeIdKey );
            if ( connectionOpportunityFilter.HasValue )
            {
                connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( connectionOpportunityFilter.Value );
            }

            var canEditRequests = CanEditConnectionRequests( connectionType, connectionOpportunity );

            return ActionOk( canEditRequests );
        }

        /// <summary>
        /// Gets the Connection Opportunity Detail Bag for the specified Connection Opportunity,
        /// used to populate the opportunity details without a full page reload.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The GUID of the Connection Opportunity to retrieve details for.</param>
        /// <returns>A Block Action Result containing the <see cref="ConnectionOpportunityDetailBag"/> if found; otherwise a not found result.</returns>
        [BlockAction]
        public BlockActionResult FetchConnectionOpportunityDetails( string connectionOpportunityGuid, string campus )
        {
            Guid? opportunityGuid = connectionOpportunityGuid.AsGuidOrNull();
            Guid? campusGuid = campus.AsGuidOrNull();

            if ( !opportunityGuid.HasValue )
            {
                return ActionNotFound( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( opportunityGuid.Value );

            if ( connectionOpportunity == null )
            {
                return ActionNotFound( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            int? campusId = null;

            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.GetId( campusGuid.Value );
            }

            var bag = GetConnectionOpportunityDetailBag( connectionOpportunity, campusId );

            if ( bag == null )
            {
                return ActionNotFound();
            }

            return ActionOk( bag );
        }

        /// <summary>
        /// Gets the available connectors for the specified Connection Opportunity,
        /// optionally filtered by campus.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The GUID of the Connection Opportunity to retrieve connectors for.</param>
        /// <param name="campus">The GUID of the campus to filter connectors by, or null to return all connectors.</param>
        /// <returns>A Block Action Result containing the list of connector items if found; otherwise a not found result.</returns>
        [BlockAction]
        public BlockActionResult FetchConnectorOptions( string connectionOpportunityGuid, string campus )
        {
            Guid? opportunityGuid = connectionOpportunityGuid.AsGuidOrNull();
            Guid? campusGuid = campus.AsGuidOrNull();

            if ( !opportunityGuid.HasValue )
            {
                return ActionNotFound( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found.");
            }

            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( opportunityGuid.Value );

            if ( connectionOpportunity == null )
            {
                return ActionNotFound( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            int? campusId = null;

            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.GetId( campusGuid.Value );
            }

            var bag = GetConnectorOptionsBag( connectionOpportunity, campusId );

            return ActionOk( bag );
        }

        /// <summary>
        /// Gets the available placement groups for the specified Connection Opportunity,
        /// optionally filtered by campus.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The GUID of the Connection Opportunity to retrieve placement groups for.</param>
        /// <param name="campus">The GUID of the campus to filter placement groups by, or null to return all placement groups.</param>
        /// <returns>A Block Action Result containing the list of placement group items if found; otherwise a not found result.</returns>
        [BlockAction]
        public BlockActionResult FetchPlacementGroupItems( string connectionOpportunityGuid, string campus )
        {
            Guid? opportunityGuid = connectionOpportunityGuid.AsGuidOrNull();
            Guid? campusGuid = campus.AsGuidOrNull();

            if ( !opportunityGuid.HasValue )
            {
                return ActionNotFound( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( opportunityGuid.Value );

            if ( connectionOpportunity == null )
            {
                return ActionNotFound( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            int? campusId = null;

            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.GetId( campusGuid.Value );
            }

            var placementGroups = GetPlacementGroupItems( connectionOpportunity, campusId );

            return ActionOk( placementGroups );
        }

        #region Placement Group Block Actions

        /// <summary>
        /// Gets the Placement Group Details Bag for the specified Connection Opportunity and placement
        /// group, containing the available member roles, statuses per role, and group member attributes.
        /// Used to populate placement group configuration in the detail panel without a full page reload.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The GUID of the Connection Opportunity used to look up the applicable group configs.</param>
        /// <param name="placementGroupGuid">The GUID of the placement group to retrieve role, status, and attribute details for.</param>
        /// <returns>A Block Action Result containing the <see cref="PlacementGroupDetailsBag"/> if both the opportunity and group are found; otherwise a not found result.</returns>
        [BlockAction]
        public BlockActionResult FetchPlacementGroupDetails( string connectionOpportunityGuid, string placementGroupGuid )
        {
            var connectionOpportunity = new ConnectionOpportunityService( RockContext ).Get( connectionOpportunityGuid.AsGuid() );
            var placementGroup = new GroupService( RockContext ).Get( placementGroupGuid.AsGuid() );

            if ( connectionOpportunity == null || placementGroup == null )
            {
                return ActionNotFound();
            }

            var configs = new ConnectionOpportunityGroupConfigService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( c =>
                    c.ConnectionOpportunityId == connectionOpportunity.Id &&
                    c.GroupTypeId == placementGroup.GroupTypeId )
                .Select( c => new
                {
                    Role = c.GroupMemberRole,
                    Status = c.GroupMemberStatus
                } )
                .ToList();

            var tempGroupMember = new Rock.Model.GroupMember
            {
                GroupId = placementGroup.Id
            };

            tempGroupMember.LoadAttributes();

            var bag = new PlacementGroupDetailsBag
            {
                GroupMemberRoles = configs
                    .DistinctBy( c => c.Role.Guid )
                    .Select( c => c.Role.ToListItemBag() )
                    .ToList(),

                GroupMemberStatuses = configs.GroupBy( c => c.Role.Guid.ToString() )
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy( c => c.Status )
                            .DistinctBy( c => c.Status )
                            .Select( c => new ListItemBag
                            {
                                Text = c.Status.ToString(),
                                Value = ( ( int ) c.Status ).ToString()
                            } )
                            .ToList()
                    ),


                GroupMemberAttributes = tempGroupMember.GetPublicAttributesForEdit( RequestContext.CurrentPerson )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Checks whether the specified Connection Requests meet the mandatory group requirements
        /// of their assigned placement groups, returning the IdKeys of any requests that do not.
        /// </summary>
        /// <param name="connectionRequestIdKeys">The list of Connection Request IdKeys to evaluate against their assigned placement group requirements.</param>
        /// <returns>A Block Action Result containing a list of IdKeys for the Connection Requests whose requesters do not meet one or more mandatory group requirements.</returns>
        [BlockAction]
        public BlockActionResult CheckIfRequestsMeetRequirements( List<string> connectionRequestIdKeys )
        {
            var connectionRequestIds = connectionRequestIdKeys
                .Select( idKey => IdHasher.Instance.GetId( idKey ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var idsNotMeeting = GetConnectionRequestIdsNotMeetingGroupRequirements( connectionRequestIds );

            var idKeysNotMeeting = idsNotMeeting.Select( id => IdHasher.Instance.GetHash( id ) ).ToList();

            return ActionOk( idKeysNotMeeting );
        }

        /// <summary>
        /// Checks whether a single Connection Request meets the mandatory group requirements
        /// of its assigned placement group.
        /// </summary>
        /// <param name="connectionRequestIdKey">The IdKey of the Connection Request to evaluate against its assigned placement group requirements.</param>
        /// <returns>A Block Action Result containing true if the requester meets all mandatory group requirements; otherwise false. Returns a bad request result if the IdKey cannot be resolved.</returns>
        [BlockAction]
        public BlockActionResult CheckIfRequestMeetsRequirements( string connectionRequestIdKey )
        {
            var connectionRequestId = IdHasher.Instance.GetId( connectionRequestIdKey );

            if ( !connectionRequestId.HasValue )
            {
                return ActionBadRequest( "Connection Request not found." );
            }

            var isMeetingRequirements = ConnectionRequestMeetsGroupRequirements( connectionRequestId.Value );

            return ActionOk( isMeetingRequirements );
        }

        /// <summary>
        /// Inserts or updates a manual Group Member Requirement, marking it as met or unmet
        /// for the specified group member. If no existing requirement IdKey is provided, a new
        /// requirement is created using the supplied Group Member and Group Requirement IdKeys.
        /// Returns the saved requirement's IdKey and current state to update the client,
        /// since a newly created requirement will not yet be present on the client.
        /// </summary>
        /// <param name="bag">A bag containing the optional Group Member Requirement IdKey, Group Member IdKey, Group Requirement IdKey, and whether the requirement is met.</param>
        /// <returns>A Block Action Result containing a <see cref="GroupMemberRequirementBag"/> with the saved IdKey and updated requirement state. Returns a bad request result if the requirement cannot be found or created, or if the current user is not authorized to edit it.</returns>
        [BlockAction]
        public BlockActionResult UpdateManualRequirement( UpdateGroupMemberRequirementBag bag )
        {
            GroupMemberRequirement groupMemberRequirement;
            var groupMemberRequirementService = new GroupMemberRequirementService( RockContext );

            if ( bag.GroupMemberRequirementIdKey.IsNullOrWhiteSpace() )
            {
                if ( bag.GroupMemberIdKey.IsNullOrWhiteSpace() || bag.GroupRequirementIdKey.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( $"{Rock.Model.GroupMemberRequirement.FriendlyTypeName} not found." );
                }

                groupMemberRequirement = new GroupMemberRequirement
                {
                    GroupMemberId = IdHasher.Instance.GetId( bag.GroupMemberIdKey ).Value,
                    GroupRequirementId = IdHasher.Instance.GetId( bag.GroupRequirementIdKey ).Value,
                };

                groupMemberRequirementService.Add( groupMemberRequirement );
            }
            else
            {
                groupMemberRequirement = groupMemberRequirementService.Get( bag.GroupMemberRequirementIdKey, !PageCache.Layout.Site.DisablePredictableIds );
                if ( groupMemberRequirement == null )
                {
                    return ActionBadRequest( $"{Rock.Model.GroupMemberRequirement.FriendlyTypeName} not found." );
                }
            }

            if ( !groupMemberRequirement.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to edit this Group Member Requirement." );
            }

            groupMemberRequirement.WasManuallyCompleted = bag.IsMet;

            if ( bag.IsMet )
            {
                groupMemberRequirement.ManuallyCompletedByPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
                groupMemberRequirement.ManuallyCompletedDateTime = RockDateTime.Now;
                groupMemberRequirement.RequirementMetDateTime = RockDateTime.Now;
            }
            else
            {
                groupMemberRequirement.ManuallyCompletedByPersonAliasId = null;
                groupMemberRequirement.ManuallyCompletedDateTime = null;
                groupMemberRequirement.RequirementMetDateTime = null;
            }

            RockContext.SaveChanges();

            // We'll need to return the new state along with the group member requirement id key because it may have just been created, and therfore not present on the client.
            var result = new GroupMemberRequirementBag
            {
                GroupMemberRequirementIdKey = groupMemberRequirement.IdKey,
                GroupMemberRequirementState = groupMemberRequirement.GroupMemberRequirementState
            };

            return ActionOk( result );
        }

        #endregion Placement Group Block Actions

        #region Bulk Update Block Actions

        /// <summary>
        /// Reassigns the connector for a list of Connection Requests to the specified person,
        /// or clears the connector if no valid Person Alias GUID is provided.
        /// Returns updated grid data for each affected request to refresh the UI without a full page reload.
        /// </summary>
        /// <remarks>
        /// Bulk update is intentionally avoided here so that each Connection Request's save hook
        /// logic is executed individually during the save.
        /// </remarks>
        /// <param name="connectionRequestIdKeys">The list of IdKeys of the Connection Requests to reassign.</param>
        /// <param name="connectorPersonAliasGuid">The GUID of the Person Alias to assign as the new connector, or an empty/invalid GUID to clear the connector.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing a list of <see cref="ConnectionListGridUpdateBag"/> objects to refresh the connector columns in the grid. Returns a bad request result if the Connection Type cannot be resolved or the current user lacks edit permissions.</returns>
        [BlockAction]
        public BlockActionResult ReassignConnector( List<string> connectionRequestIdKeys, string connectorPersonAliasGuid, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );

            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequests = CanEditSpecifiedConnectionRequests( connectionType, connectionRequestIdKeys, out var connectionRequests, out var actionError );

            if ( !canEditRequests )
            {
                return actionError;
            }

            var newConnectorPersonAlias = new PersonAliasService( RockContext ).GetInclude( connectorPersonAliasGuid.AsGuid(), c => c.Person );

            List<ConnectionListGridUpdateBag> gridUpdateBags = new List<ConnectionListGridUpdateBag>();
            ListItemBag connectorItem = null;

            if ( newConnectorPersonAlias != null )
            {
                connectorItem = new ListItemBag
                {
                    Value = newConnectorPersonAlias.Person.IdKey,
                    Text = newConnectorPersonAlias.Person.FullName
                };
            }
            else
            {
                connectorItem = new ListItemBag
                {
                    Value = "unassigned",
                    Text = "Unassigned"
                };
            }

            // Can't use Bulk Update becase we need the Save Hook logic to run.
            foreach ( var connectionRequest in connectionRequests )
            {
                connectionRequest.ConnectorPersonAliasId = newConnectorPersonAlias?.Id;

                gridUpdateBags.Add( new ConnectionListGridUpdateBag
                {
                    IdKey = connectionRequest.IdKey,
                    ConnectorGrouping = GetGroupingFieldBag( newConnectorPersonAlias?.Id, "person", newConnectorPersonAlias?.Person?.FullName, null, null, newConnectorPersonAlias?.Person?.PhotoUrl ),
                    ConnectorDetails = connectorItem
                } );
            }

            RockContext.SaveChanges();

            return ActionOk( gridUpdateBags );
        }

        /// <summary>
        /// Updates the statuses of a list of Connection Requests, and optionally marks a separate
        /// list of requests as Connected (completed). For completed requests, placement group
        /// assignment is attempted and the connection state is set to Connected. For status updates,
        /// the new status is validated and a note is required if the current status mandates it.
        /// Returns updated grid data for all affected requests to refresh the UI without a full page reload.
        /// </summary>
        /// <remarks>
        /// Both status update and completion requests are authorized together in a single edit
        /// permission check before any changes are applied. Completed requests are processed first
        /// in the loop via a continue, skipping the status update logic for those requests.
        /// </remarks>
        /// <param name="statusUpdateBags">A list of status update bags, each pairing a Connection Request IdKey with its new status GUID and an optional note.</param>
        /// <param name="completedRequestIdKeys">A list of Connection Request IdKeys to mark as Connected (completed), triggering placement group assignment if configured.</param>
        /// <returns>A Block Action Result containing a list of <see cref="ConnectionListGridUpdateBag"/> objects to refresh the state, status, and due status columns in the grid. Returns a bad request result if the Connection Type cannot be resolved, the user lacks edit permissions, a required note is missing, or placement group assignment fails.</returns>
        [BlockAction]
        public BlockActionResult UpdateRequestStatuses( List<ConnectionRequestUpdateBag> statusUpdateBags, List<string> completedRequestIdKeys )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters();
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var allRequestIdKeys = statusUpdateBags.Select( r => r.ConnectionRequestIdKey )
                .Concat( completedRequestIdKeys )
                .Distinct()
                .ToList();


            var canEditRequests = CanEditSpecifiedConnectionRequests( connectionType, allRequestIdKeys, out var connectionRequests, out var actionError );

            if ( !canEditRequests )
            {
                return actionError;
            }

            var connectionStatuses = new ConnectionStatusService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( s => s.ConnectionTypeId == connectionType.Id )
                .ToList();

            var statusBagByIdKey = statusUpdateBags.ToDictionary( b => b.ConnectionRequestIdKey );

            foreach ( var request in connectionRequests )
            {
                var currentStatus = connectionStatuses.Where( s => s.Id == request.ConnectionStatusId ).FirstOrDefault();

                // If Completed request, then apply state update.
                if ( completedRequestIdKeys.Contains( request.IdKey ) )
                {
                    request.ConnectionState = ConnectionState.Connected;

                    if ( request.ConnectionOpportunity.ConnectionType.RequiresPlacementGroupToConnect && !request.AssignedGroupId.HasValue )
                    {
                        return ActionBadRequest( "This Connection Type requires a Placement Group to be configured in order to complete the request." );
                    }

                    if ( !TryAssignPlacementGroup( request, out var error ) )
                    {
                        return error;
                    }

                    continue;
                }
                if ( !statusBagByIdKey.TryGetValue( request.IdKey, out var updateBag ) )
                {
                    continue;
                }
                var newStatus = connectionStatuses.Where( s => s.Guid == updateBag.ConnectionStatusGuid.AsGuid() ).FirstOrDefault();
                if ( newStatus == null )
                {
                    return ActionBadRequest( $"{ConnectionStatus.FriendlyTypeName} not found." );
                }
                if ( currentStatus.IsNoteRequiredOnCompletion &&
                        updateBag.Note.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "A note is required." );
                }

                request.ConnectionStatusId = newStatus.Id;
                request.ConnectionStatusHistoryNote = statusUpdateBags.First().Note;
            }

            RockContext.SaveChanges();

            List<ConnectionListGridUpdateBag> gridUpdateBags = new List<ConnectionListGridUpdateBag>();

            foreach ( var request in connectionRequests )
            {
                var dueStatus = GetDueStatus( request.DueDate, request.DueSoonDate, request.ConnectionState, request.ConnectedDateTime );

                gridUpdateBags.Add( new ConnectionListGridUpdateBag
                {
                    IdKey = request.IdKey,
                    StateGrouping = new GroupingFieldBag
                    {
                        Key = request.ConnectionState.ToString(),
                        Type = "text",
                        Label = request.ConnectionState.GetDisplayName(),
                        IconCssClass = GetStateIconCssClass( request.ConnectionState ),
                        Order = ( int ) request.ConnectionState
                    },
                    StatusGrouping = GetGroupingFieldBag( request.ConnectionStatus.Id, "text", request.ConnectionStatus.Name, request.ConnectionStatus.Order ),
                    ConnectionState = request.ConnectionState,
                    ConnectionStatusBag = new ConnectionStatusBag
                    {
                        Guid = request.ConnectionStatus.Guid,
                        Order = request.ConnectionStatus.Order,
                        Name = request.ConnectionStatus.Name,
                        HighlightColor = request.ConnectionStatus.HighlightColor,
                        IsNoteRequiredOnCompletion = request.ConnectionStatus.IsNoteRequiredOnCompletion,
                        IsDefaultStatus = request.ConnectionStatus.IsDefault
                    },
                    DueStatusGrouping = GetGroupingFieldBag( ( int ) dueStatus, "text", dueStatus.GetDisplayName(), dueStatus.GetOrder(), "ti ti-calendar", null, GetDueStatusTextColorCssClass( dueStatus ) ),
                    DueStatus = dueStatus,
                    DueDate = request.DueDate,
                    DueSoonDate = request.DueSoonDate,
                    FollowUpDate = request.FollowupDate,
                    CompletedDateTime = request.ConnectedDateTime
                } );
            }

            return ActionOk( gridUpdateBags );
        }

        /// <summary>
        /// Updates the connection state of one or more Connection Requests to the specified state.
        /// For Future Follow-Up transitions, a follow-up date is required. For Connected (completed)
        /// transitions, placement group assignment is attempted if a group is configured, and a
        /// placement group is required if the Connection Type mandates it.
        /// Returns updated grid data for all affected requests to refresh the UI without a full page reload.
        /// </summary>
        /// <param name="bag">A bag containing the target Connection State, the list of Connection Request IdKeys to update, an optional follow-up date, and optional group member requirements for placement validation.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing a list of <see cref="ConnectionListGridUpdateBag"/> objects to refresh the state and follow-up date columns in the grid. Returns a bad request result if the Connection Type cannot be resolved, the user lacks edit permissions, a required follow-up date is missing, a required placement group is not assigned, or placement group assignment fails.</returns>
        [BlockAction]
        public BlockActionResult UpdateRequestStates( UpdateConnectionRequestStatesBag bag, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequest = CanEditSpecifiedConnectionRequests( connectionType, bag.ConnectionRequestIdKeys, out var connectionRequests, out var actionError );

            if ( !canEditRequest )
            {
                return actionError;
            }

            if ( bag.ConnectionState == ConnectionState.FutureFollowUp && !bag.FollowUpDate.HasValue )
            {
                return ActionBadRequest( "A Follow-Up Date is required." );
            }

            foreach ( var request in connectionRequests )
            {
                if ( bag.ConnectionState == ConnectionState.FutureFollowUp )
                {
                    request.FollowupDate = bag.FollowUpDate?.DateTime;
                }

                if ( request.ConnectionState != ConnectionState.Connected && bag.ConnectionState == ConnectionState.Connected )
                {
                    if ( request.ConnectionOpportunity.ConnectionType.RequiresPlacementGroupToConnect && !request.AssignedGroupId.HasValue )
                    {
                        return ActionBadRequest( "This Connection Type requires a Placemnt Group to be configured in order to complete the request." );
                    }

                    if ( !TryAssignPlacementGroup( request, out var error, bag.GroupMemberRequirements ) )
                    {
                        return error;
                    }
                }

                request.ConnectionState = bag.ConnectionState;
            }

            RockContext.SaveChanges();

            List<ConnectionListGridUpdateBag> gridUpdateBags = new List<ConnectionListGridUpdateBag>();
            foreach ( var request in connectionRequests )
            {
                gridUpdateBags.Add( new ConnectionListGridUpdateBag
                {
                    IdKey = request.IdKey,
                    StateGrouping = new GroupingFieldBag
                    {
                        Key = request.ConnectionState.ToString(),
                        Type = "text",
                        Label = request.ConnectionState.GetDisplayName(),
                        IconCssClass = GetStateIconCssClass( request.ConnectionState ),
                        Order = ( int ) request.ConnectionState
                    },
                    ConnectionState = request.ConnectionState,
                    FollowUpDate = request.FollowupDate,
                    CompletedDateTime = request.ConnectedDateTime
                } );
            }

            return ActionOk( gridUpdateBags );
        }

        /// <summary>
        /// Deletes the specified Connection Requests after verifying that the current user
        /// has edit permissions for each request and that each request is eligible for deletion.
        /// Activities are deleted alongside each request within a wrapped transaction.
        /// </summary>
        /// <param name="connectionRequestIdKeys">The list of IdKeys of the Connection Requests to delete.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result indicating success if all requests were deleted. Returns a bad request result if the Connection Type cannot be resolved, the user lacks edit permissions, or any request cannot be deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteRequests( List<string> connectionRequestIdKeys, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequest = CanEditSpecifiedConnectionRequests( connectionType, connectionRequestIdKeys, out var connectionRequests, out var actionError );

            if ( !canEditRequest )
            {
                return actionError;
            }

            var connectionRequestService = new ConnectionRequestService( RockContext );

            foreach ( var request in connectionRequests )
            {
                if ( !connectionRequestService.CanDelete( request, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                RockContext.WrapTransaction( () =>
                {
                    new ConnectionRequestActivityService( RockContext ).DeleteRange( request.ConnectionRequestActivities );
                    connectionRequestService.Delete( request );
                    RockContext.SaveChanges();
                } );
            }

            return ActionOk();
        }

        /// <summary>
        /// Adds a Connection Request Activity to one or more Connection Requests, optionally
        /// creating a Person Note on the requester if the selected activity type requires it
        /// or the user has opted in. Returns updated grid data to refresh the activity count
        /// and last activity date columns without a full page reload.
        /// </summary>
        /// <remarks>
        /// Person Note creation is driven by the activity type's <see cref="PersonNoteCreationBehavior"/>:
        /// notes are always created if set to <see cref="PersonNoteCreationBehavior.AlwaysCreateAPersonNote"/>,
        /// and conditionally created if set to <see cref="PersonNoteCreationBehavior.AskAtActivityCreation"/>
        /// based on the <see cref="ActivityBag.AddPersonNote"/> flag. A Person Note Type must be configured
        /// on the activity type if note creation is required.
        /// </remarks>
        /// <param name="bag">A bag containing the activity type, connector, note text, optional person note flag, and the list of Connection Request IdKeys to add the activity to.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing a list of <see cref="ConnectionListGridUpdateBag"/> objects to refresh the activity count and last activity date columns in the grid. Returns a bad request result if the Connection Type cannot be resolved, the user lacks edit permissions, the activity type is invalid, or a required Person Note Type is not configured.</returns>
        [BlockAction]
        public BlockActionResult AddActivityForRequests( ActivityBag bag, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequest = CanEditSpecifiedConnectionRequests( connectionType, bag.ConnectionRequestIdKeys, out var connectionRequests, out var actionError, q => q.Include( r => r.ConnectionRequestActivities ).Include( r => r.PersonAlias ) );

            if ( !canEditRequest )
            {
                return actionError;
            }

            var connectionRequestActivityService = new ConnectionRequestActivityService( RockContext );
            var noteService = new NoteService( RockContext );
            var connectorPersonAlias = new PersonAliasService( RockContext ).GetInclude( bag.ConnectorPersonAliasGuid.AsGuid(), c => c.Person );
            var activityType = new ConnectionActivityTypeService( RockContext ).Get( bag.ActivityTypeGuid.AsGuid() );

            if ( activityType == null )
            {
                return ActionBadRequest( "Invalid Activity Type" );
            }

            var shouldCreatePersonNote = activityType.PersonNoteCreationBehavior == PersonNoteCreationBehavior.AlwaysCreateAPersonNote || ( activityType.PersonNoteCreationBehavior == PersonNoteCreationBehavior.AskAtActivityCreation && bag.AddPersonNote );

            if ( shouldCreatePersonNote && !activityType.PersonNoteTypeId.HasValue )
            {
                return ActionBadRequest( "The selected Activity Type is missing a required Person Note Type." );
            }

            List<ConnectionListGridUpdateBag> gridUpdateBags = new List<ConnectionListGridUpdateBag>();

            foreach ( var request in connectionRequests )
            {
                var activity = new ConnectionRequestActivity();

                activity.ConnectionRequestId = request.Id;
                activity.ConnectorPersonAliasId = connectorPersonAlias?.Id;
                activity.Note = bag.Note;
                activity.ConnectionActivityTypeId = activityType.Id;
                activity.ConnectionOpportunityId = request.ConnectionOpportunityId;

                connectionRequestActivityService.Add( activity );

                if ( shouldCreatePersonNote )
                {
                    var personNote = new Note
                    {
                        NoteTypeId = activityType.PersonNoteTypeId.Value,
                        EntityId = request.PersonAlias.PersonId,
                        Caption = request.ConnectionOpportunity.Name,
                        Text = bag.Note,
                        CreatedByPersonAliasId = connectorPersonAlias?.Id,
                    };
                    noteService.Add( personNote );
                }

                gridUpdateBags.Add( new ConnectionListGridUpdateBag
                {
                    IdKey = request.IdKey,
                    LastActivityDateTime = RockDateTime.Now,
                    ActivityCount = request.ConnectionRequestActivities?.Count ?? 1
                } );
            }

            RockContext.SaveChanges();
            return ActionOk( gridUpdateBags );
        }

        /// <summary>
        /// Launches a manual Connection Workflow for one or more Connection Requests.
        /// For a single request, the workflow is processed synchronously and a workflow
        /// entry form URL is returned if the activated workflow has an active entry form.
        /// For multiple requests, eligible requests are processed in a background thread
        /// via <see cref="LaunchWorkflowsInBackground"/>. In both cases, requests are
        /// pre-filtered for eligibility against the workflow's include and exclude Data Views.
        /// </summary>
        /// <remarks>
        /// Only workflows with a Manual trigger type and an active Workflow Type are permitted.
        /// If none of the provided requests are eligible after Data View filtering, a status
        /// message is returned without launching any workflows.
        /// </remarks>
        /// <param name="bag">A bag containing the Connection Workflow GUID and the list of Connection Request IdKeys to launch the workflow for.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing a <see cref="LaunchWorkflowResultBag"/> with a status message and, for single requests with an active entry form, a workflow entry page URL. Returns a bad request result if the Connection Type cannot be resolved, the workflow is invalid or inactive, the trigger type is not manual, or the current user is not authorized to view the workflow type.</returns>
        [BlockAction]
        public BlockActionResult LaunchWorkflowForRequests( LaunchWorkflowBag bag, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var connectionWorkflow = new ConnectionWorkflowService( RockContext ).GetInclude( bag.ConnectionWorkflowGuid.AsGuid(), w => w.WorkflowType );

            if ( !connectionWorkflow.WorkflowTypeId.HasValue )
            {
                return ActionBadRequest( "Invalid Workflow." );
            }

            var workflowType = WorkflowTypeCache.Get( connectionWorkflow.WorkflowTypeId.Value );

            if ( workflowType == null )
            {
                return ActionBadRequest( "Invalid Workflow Type." );
            }

            if ( !connectionWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to launch the selected workflow." );
            }

            if ( connectionWorkflow.TriggerType != ConnectionWorkflowTriggerType.Manual || !( connectionWorkflow.WorkflowType.IsActive ?? true ) )
            {
                return ActionBadRequest( "The selected Workflow must be active with a Manual Trigger Type." );
            }

            var workflowService = new WorkflowService( RockContext );
            var connectionRequestWorkflowService = new ConnectionRequestWorkflowService( RockContext );
            List<int> includedDataViewValues = null;
            List<int> excludedDataViewValues = null;

            if ( connectionWorkflow.IncludeDataViewId.HasValue )
            {
                includedDataViewValues = GetDataViewValues( connectionWorkflow.IncludeDataViewId.Value );
            }
            if ( connectionWorkflow.ExcludeDataViewId.HasValue )
            {
                excludedDataViewValues = GetDataViewValues( connectionWorkflow.ExcludeDataViewId.Value );
            }

            var decodedIds = bag.ConnectionRequestIdKeys.Select( key => Rock.Utility.IdHasher.Instance.GetId( key ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var connectionRequests = new ConnectionRequestService( RockContext ).GetByIds( decodedIds ).Include( r => r.PersonAlias.Person ).ToList();
            var isSingleRequest = connectionRequests.Count == 1;

            var eligibleRequests = connectionRequests
                .Where( r => IsEligibleForWorkflow( connectionWorkflow, r, includedDataViewValues, excludedDataViewValues ) )
                .ToList();

            var launchWorkflowResultBag = new LaunchWorkflowResultBag();

            if ( eligibleRequests.Count == 0 )
            {
                launchWorkflowResultBag.StatusMessage = $"The '{workflowType.Name}' workflow was not started for any of the selected connection requests due to its configuration.";
                return ActionOk( launchWorkflowResultBag );
            }

            // Single request: process synchronously so we can return an entry form URL if needed.
            if ( isSingleRequest )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, connectionWorkflow.WorkflowType.WorkTerm, RockContext );

                if ( workflow == null )
                {
                    return ActionBadRequest( "An error occurred while activating the Workflow." );
                }

                List<string> workflowErrors;
                if ( !workflowService.Process( workflow, eligibleRequests[0], out workflowErrors ) )
                {
                    return ActionBadRequest( "Workflow Processing Error(s):<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>" );
                }

                if ( workflow.Id != 0 )
                {
                    connectionRequestWorkflowService.Add( new ConnectionRequestWorkflow
                    {
                        ConnectionRequestId = eligibleRequests[0].Id,
                        WorkflowId = workflow.Id,
                        ConnectionWorkflowId = connectionWorkflow.Id,
                        TriggerType = connectionWorkflow.TriggerType,
                        TriggerQualifier = connectionWorkflow.QualifierValue
                    } );
                }

                if ( workflow.HasActiveEntryForm( RequestContext.CurrentPerson ) )
                {
                    var qryParam = new Dictionary<string, string>
                    {
                        { "WorkflowType", workflowType.IdKey },
                        { "WorkflowGuid", workflow.Guid.ToString() }
                    };

                    launchWorkflowResultBag.WorkflowEntryPageUrl = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, qryParam );
                    launchWorkflowResultBag.StatusMessage = $"A '{workflowType.Name}' workflow has been started. The new workflow has an active form that is ready for input.";
                }
                else
                {
                    launchWorkflowResultBag.StatusMessage = $"A '{workflowType.Name}' workflow has been started.";
                }

                RockContext.SaveChanges();
                return ActionOk( launchWorkflowResultBag );
            }

            LaunchWorkflowsInBackground( eligibleRequests, connectionWorkflow, workflowType );

            launchWorkflowResultBag.StatusMessage = $"The '{workflowType.Name}' workflow is being started for {eligibleRequests.Count} of the {connectionRequests.Count} selected connection requests.";
            return ActionOk( launchWorkflowResultBag );
        }

        #endregion Bulk Update Block Actions

        /// <summary>
        /// Checks whether the specified person has any active Connection Requests
        /// within the Connection Type resolved from the current page parameters.
        /// </summary>
        /// <param name="requesterPersonAliasGuid">The GUID of the Person Alias to check for active Connection Requests.</param>
        /// <returns>A Block Action Result containing true if the person has at least one active Connection Request for the resolved Connection Type; otherwise false. Returns a bad request result if the Connection Type cannot be resolved.</returns>
        [BlockAction]
        public BlockActionResult CheckForActiveRequest( Guid requesterPersonAliasGuid )
        {
            var connectionType = GetConnectionTypeCacheFromPageParameters();
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var hasActiveRequests = new ConnectionRequestService( RockContext ).Queryable().Any( cr => cr.ConnectionState == ConnectionState.Active
                && cr.ConnectionTypeId == connectionType.Id
                && cr.PersonAlias.Guid == requesterPersonAliasGuid );

            return ActionOk( hasActiveRequests );
        }

        /// <summary>
        /// Saves a Connection Request from the provided valid properties box, handling both
        /// creation of new requests and updates to existing ones. If a campus context is present
        /// on the page, it is applied to new requests. After saving, a Connection List Update Box
        /// is returned to refresh the grid row and, for existing requests, the detail panel.
        /// </summary>
        /// <param name="box">A valid properties box containing the Connection Request Bag with the data to save.</param>
        /// <returns>A Block Action Result containing a <see cref="ConnectionListUpdateBox"/> to update the UI without a full page reload. Returns a bad request result if the entity cannot be found or the provided data is invalid.</returns>
        [BlockAction]
        public BlockActionResult SaveConnectionRequest( ValidPropertiesBox<ConnectionRequestBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError, box.Bag.ConnectionOpportunityGuid.AsGuidOrNull() ) )
            {
                return actionError;
            }

            var isInEditMode = entity.Id != 0;

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            var updateBox = GetConnectionListUpdateBox( entity.Id, isInEditMode );

            return ActionOk( updateBox );
        }

        /// <summary>
        /// Updates the status of a single Connection Request, validating that the new status
        /// belongs to the same Connection Type and that a note is provided if the current
        /// status requires one. Returns updated grid data to refresh the request's status
        /// and due status columns without a full page reload.
        /// </summary>
        /// <param name="bag">A status update bag containing the Connection Request IdKey, the new status GUID, and an optional note.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing a <see cref="ConnectionListGridUpdateBag"/> to refresh the status and due status columns in the grid. Returns a bad request result if the Connection Type cannot be resolved, the user lacks edit permissions, the status is invalid, or a required note is missing.</returns>
        [BlockAction]
        public BlockActionResult ChangeRequestStatus( ConnectionRequestUpdateBag bag, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequest = CanEditSpecifiedConnectionRequest( connectionType, bag.ConnectionRequestIdKey, out var connectionRequest, out var actionError );

            if ( !canEditRequest )
            {
                return actionError;
            }

            var connectionRequestStatus = new ConnectionStatusService( RockContext ).Get( bag.ConnectionStatusGuid );
            if ( connectionRequestStatus == null || connectionRequestStatus.ConnectionTypeId != connectionRequest.ConnectionTypeId )
            {
                return ActionBadRequest( "Invalid Connection Status" );
            }

            if ( connectionRequest.ConnectionStatus.IsNoteRequiredOnCompletion && bag.Note.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A note is required." );
            }

            // Save status history for previous status

            var connectionRequestStatusHistoryService = new ConnectionRequestStatusHistoryService( RockContext );

            // Update to new status
            connectionRequest.ConnectionStatusId = connectionRequestStatus.Id;
            connectionRequest.ConnectionStatusHistoryNote = bag.Note;

            RockContext.SaveChanges();

            var dueStatus = GetDueStatus( connectionRequest.DueDate, connectionRequest.DueSoonDate, connectionRequest.ConnectionState, connectionRequest.ConnectedDateTime );

            var gridUpdateBag = new ConnectionListGridUpdateBag
            {
                IdKey = connectionRequest.IdKey,
                StateGrouping = new GroupingFieldBag
                {
                    Key = connectionRequest.ConnectionState.ToString(),
                    Type = "text",
                    Label = connectionRequest.ConnectionState.GetDisplayName(),
                    IconCssClass = GetStateIconCssClass( connectionRequest.ConnectionState ),
                    Order = ( int ) connectionRequest.ConnectionState
                },
                StatusGrouping = GetGroupingFieldBag( connectionRequestStatus.Id, "text", connectionRequestStatus.Name, connectionRequestStatus.Order ),
                ConnectionStatusBag = new ConnectionStatusBag
                {
                    Guid = connectionRequestStatus.Guid,
                    Order = connectionRequestStatus.Order,
                    Name = connectionRequestStatus.Name,
                    HighlightColor = connectionRequestStatus.HighlightColor,
                    IsNoteRequiredOnCompletion = connectionRequestStatus.IsNoteRequiredOnCompletion,
                    IsDefaultStatus = connectionRequestStatus.IsDefault
                },
                ConnectionState = connectionRequest.ConnectionState,
                DueStatusGrouping = GetGroupingFieldBag( ( int ) dueStatus, "text", dueStatus.GetDisplayName(), dueStatus.GetOrder(), "ti ti-calendar", null, GetDueStatusTextColorCssClass( dueStatus ) ),
                DueStatus = dueStatus,
                DueDate = connectionRequest.DueDate,
                DueSoonDate = connectionRequest.DueSoonDate,
                FollowUpDate = connectionRequest.FollowupDate,
                CompletedDateTime = connectionRequest.ConnectedDateTime
            };

            return ActionOk( gridUpdateBag );
        }

        /// <summary>
        /// Inserts or updates the Celebration Note on a single Connection Request.
        /// If a Celebration Note already exists for the request and the text has changed,
        /// the note text and created-by person are updated. If the text is unchanged, no
        /// write is performed. Returns updated grid data to refresh the celebration text
        /// column without a full page reload.
        /// </summary>
        /// <param name="bag">A bag containing the Connection Request IdKey and the celebration text to save.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing a <see cref="ConnectionListGridUpdateBag"/> to refresh the celebration text column in the grid. Returns a bad request result if the Connection Type cannot be resolved or the user lacks edit permissions.</returns>
        [BlockAction]
        public BlockActionResult UpsertCelebrationText( UpsertCelebrationBag bag, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequest = CanEditSpecifiedConnectionRequest( connectionType, bag.ConnectionRequestIdKey, out var connectionRequest, out var actionError );

            if ( !canEditRequest )
            {
                return actionError;
            }

            var celebrationNoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() ).Id;
            var noteService = new NoteService( RockContext );

            var existingNote = noteService.Queryable()
                .Where( n => n.NoteTypeId == celebrationNoteTypeId && n.EntityId == connectionRequest.Id )
                .FirstOrDefault();

            if ( existingNote != null )
            {
                // Only write if the text actually changed.
                if ( existingNote.Text != bag.CelebrationText )
                {
                    existingNote.Text = bag.CelebrationText;
                    existingNote.CreatedByPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
                    RockContext.SaveChanges();
                }
            }
            else
            {
                var newNote = new Note
                {
                    NoteTypeId = celebrationNoteTypeId,
                    EntityId = connectionRequest.Id,
                    Text = bag.CelebrationText,
                    CreatedByPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId
                };

                noteService.Add( newNote );
                RockContext.SaveChanges();
            }

            var gridUpdateBag = new ConnectionListGridUpdateBag
            {
                IdKey = connectionRequest.IdKey,
                CelebrationText = bag.CelebrationText
            };

            return ActionOk( gridUpdateBag );
        }

        /// <summary>
        /// Updates an existing Connection Request Activity's note, connector, and activity type.
        /// The activity type may only be changed if it is not a system activity type.
        /// Returns an updated activity entry bag to refresh the activity in the detail panel
        /// without a full page reload.
        /// </summary>
        /// <remarks>
        /// Edit authorization is checked both at the Connection Request level via
        /// <see cref="CanEditSpecifiedConnectionRequests"/> and at the activity level via Rock's
        /// standard authorization check, requiring both to pass before any changes are applied.
        /// </remarks>
        /// <param name="bag">A bag containing the Activity IdKey, updated note, connector Person Alias GUID, activity type GUID, and the associated Connection Request IdKeys.</param>
        /// <param name="connectionTypeIdKey">An optional Connection Type IdKey used to resolve the Connection Type, in addition to the standard page parameter resolution.</param>
        /// <returns>A Block Action Result containing an updated <see cref="ActivityEntryBag"/> to refresh the activity entry in the detail panel. Returns a bad request result if the Connection Type cannot be resolved, the user lacks edit permissions on the request or activity, the activity cannot be found, or the activity type is invalid.</returns>
        [BlockAction]
        public BlockActionResult UpdateActivity( ActivityBag bag, string connectionTypeIdKey = null )
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters( connectionTypeIdKey );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionType.FriendlyTypeName} not found." );
            }

            var canEditRequest = CanEditSpecifiedConnectionRequests( connectionType, bag.ConnectionRequestIdKeys, out var connectionRequests, out var actionError );

            if ( !canEditRequest )
            {
                return actionError;
            }

            var activityService = new ConnectionRequestActivityService( RockContext );

            var activity = activityService.Get( bag.ActivityIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( activity == null )
            {
                return ActionBadRequest( $"{ConnectionRequestActivity.FriendlyTypeName} not found." );
            }

            if ( !activity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to edit this Activity." );
            }

            var activityType = new ConnectionActivityTypeService( RockContext ).Get( bag.ActivityTypeGuid.AsGuid() );
            if ( activityType == null )
            {
                return ActionBadRequest( "Invalid Activity Type." );
            }

            var connectorPersonAlias = bag.ConnectorPersonAliasGuid.IsNullOrWhiteSpace()
                ? null
                : new PersonAliasService( RockContext ).GetInclude( bag.ConnectorPersonAliasGuid.AsGuid(), c => c.Person );

            activity.Note = bag.Note;
            activity.ConnectorPersonAliasId = connectorPersonAlias?.Id;

            // The activity type can only be updated if it is not a "system activity type"
            if ( activity.ConnectionActivityType != null && activity.ConnectionActivityType.ConnectionTypeId.HasValue )
            {
                activity.ConnectionActivityTypeId = activityType.Id;
            }

            RockContext.SaveChanges();

            var updatedEntry = new ActivityEntryBag
            {
                Key = $"{ActivityEntryType.Activity}_{bag.ActivityIdKey}",
                EntryType = ActivityEntryType.Activity,
                EntryDateTime = activity.CreatedDateTime?.ToRockDateTimeOffset(),
                CreatedBy = activity.CreatedByPersonAlias?.Person?.FullName,
                CardEntry = new CardEntryBag
                {
                    Title = string.Format( "Activity: {0}", activityType.Name ),
                    Content = bag.Note,
                    PhotoUrl = activity.CreatedByPersonAlias?.Person?.PhotoUrl,

                    ActivityTypeGuid = activityType.Guid.ToString(),
                    ActivityTypeName = activityType.Name,
                    IsSystemActivityType = activityType.ConnectionTypeId == null,
                    ConnectorPersonAliasGuid = connectorPersonAlias?.Guid.ToString()
                }
            };

            return ActionOk( updatedEntry );
        }

        /// <summary>
        /// Gets the active Campaign Connection items available to the current user,
        /// partitioned by Connection Opportunity GUID. Only campaigns associated with
        /// opportunities where the current user is a connector group member are returned.
        /// </summary>
        /// <returns>A Block Action Result containing a dictionary of Connection Opportunity GUIDs mapped to their list of <see cref="ConnectionCampaignBag"/> objects, each including the pending request count and default daily limit. Returns an empty OK result if the Connection Type cannot be resolved.</returns>
        [BlockAction]
        public BlockActionResult FetchConnectionCampaigns()
        {
            ConnectionTypeCache connectionType = GetConnectionTypeCacheFromPageParameters();
            if ( connectionType == null )
            {
                return ActionOk();
            }

            var campaignConnectionItems = SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();

            // Gets a filtered list of opportunity guids for the current Connection Type where the Current Person is a Connector on the opportunity.
            var opportunityGuids = new ConnectionOpportunityService( RockContext ).Queryable()
                .Where( o =>
                    o.ConnectionTypeId == connectionType.Id &&
                    o.ConnectionOpportunityConnectorGroups.Any( cg =>
                        cg.ConnectorGroup.Members.Any( gm => gm.PersonId == RequestContext.CurrentPerson.Id )
                    )
                )
                .Select( o => o.Guid )
                .ToList();


            campaignConnectionItems = campaignConnectionItems
                .Where( ci => opportunityGuids.Contains( ci.OpportunityGuid ) && ci.IsActive )
                .ToList();

            var opportunityPartitionedCampaigns = campaignConnectionItems
                .GroupBy( ci => ci.OpportunityGuid )
                .ToDictionary(
                    g => g.Key,
                    g => g.Select( ci => new ConnectionCampaignBag
                    {
                        Guid = ci.Guid,
                        Name = ci.Name,
                        PendingCount = CampaignConnectionHelper
                            .GetPendingConnectionCount( ci, RequestContext.CurrentPerson ),
                        DefaultNumberOfRequests = ci.DailyLimitAssigned
                    } ).ToList()
                );

            return ActionOk( opportunityPartitionedCampaigns );
        }

        /// <summary>
        /// Assigns Connection Requests from a Campaign Connection item to the current user,
        /// updating existing unassigned requests and creating new ones as needed via
        /// <see cref="CampaignConnectionHelper.AddConnectionRequestsForPerson"/> up to the requested count.
        /// </summary>
        /// <param name="bag">A bag containing the Campaign Connection GUID and the number of requests to assign to the current user.</param>
        /// <returns>A Block Action Result indicating success once the requests have been assigned. Returns null if the specified campaign cannot be found.</returns>
        [BlockAction]
        public BlockActionResult AssignConnectionRequestsFromCampaign( AssignFromCampaignBag bag )
        {
            var campaignConnectionItems = SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            var selectedCampaignConnectionItem = campaignConnectionItems.Where( a => a.Guid == bag.ConnectionCampaignGuid.AsGuid() ).FirstOrDefault();

            if ( selectedCampaignConnectionItem == null )
            {
                // shouldn't happen
                return null;
            }

            var isAuthorized = CampaignConnectionHelper.GetConnectorCampusIds( selectedCampaignConnectionItem, RequestContext.CurrentPerson ).Any();
            if ( !isAuthorized )
            {
                return ActionBadRequest( "You are not authorized to assign requests from this campaign." );
            }

            int numberOfRequestsRemaining;

            // Updates any existing requests that do not have a connector and creates new requests via the campaign.
            CampaignConnectionHelper.AddConnectionRequestsForPerson( selectedCampaignConnectionItem, RequestContext.CurrentPerson, bag.NumberOfRequests, out numberOfRequestsRemaining );

            return ActionOk( );
        }

        #region Detail View Block Actions

        /// <summary>
        /// Gets the Connection Request Detail Box for the specified Connection Request,
        /// used to populate the request detail panel without a full page reload.
        /// </summary>
        /// <param name="connectionRequestIdKey">The IdKey of the Connection Request to retrieve details for.</param>
        /// <returns>A Block Action Result containing the <see cref="ConnectionRequestDetailBox"/> if the request is found and the current user is authorized to view it; otherwise a bad request result.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionRequestDetails( string connectionRequestIdKey )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );
            var connectionRequestQry = connectionRequestService.GetQueryableByKey( connectionRequestIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            var connectionRequest = connectionRequestQry.Include( r => r.ConnectionOpportunity.ConnectionType.ConnectionTypeSources )
                .Include( r => r.ConnectionOpportunity.ConnectionOpportunityCampuses.Select( c => c.Campus ) )
                .Include( r => r.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes )
                .Include( r => r.ConnectionStatus )
                .FirstOrDefault();

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to view this Connection Request." );
            }

            var box = GetConnectionRequestDetailBox( connectionRequest ); 

            return ActionOk( box );
        }

        [BlockAction]
        public async Task<BlockActionResult> GetAISummary( string connectionRequestIdKey, bool overrideCache )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );
            var connectionRequest = connectionRequestService.Get( connectionRequestIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to view this Connection Request." );
            }

            // Retrieve additional settings once so they can be used for both the prompt and cache duration.
            var additionalSettings = connectionRequest.ConnectionOpportunity?.ConnectionType?.GetConnectionTypeAdditionalSettings();
            var cacheDurationMinutes = additionalSettings?.AISummaryCacheDurationMinutes;
            var cacheKey = $"ConnectionRequestAISummary:{connectionRequest.Id}";

            // Return the cached summary if one exists and caching is configured and the user is not overriding the cache.
            if ( cacheDurationMinutes > 0 && !overrideCache )
            {
                var cachedSummary = RockCache.Get( cacheKey ) as string;
                if ( cachedSummary != null )
                {
                    return ActionOk( new GetAISummaryResponseBag { Summary = cachedSummary } );
                }
            }

            var aiProvider = new AIProviderService( RockContext ).GetActiveProvider();
            if ( aiProvider == null )
            {
                return ActionBadRequest( "No active AI provider is configured." );
            }

            var prompt = additionalSettings?.AIInsightsPrompt;
            if ( prompt.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "An AI Insights Prompt must be defined on the Connection Type." );
            }

            var connectionRequestId = connectionRequest.Id;

            // Re-fetch the connection request with all navigation properties
            // needed to build a rich JSON context for the AI prompt.
            connectionRequest = connectionRequestService.Queryable()
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.ConnectorPersonAlias.Person )
                .Include( r => r.ConnectionOpportunity.ConnectionType )
                .Include( r => r.ConnectionStatus )
                .Include( r => r.Campus )
                .Include( r => r.AssignedGroup )
                .Include( r => r.ConnectionRequestActivities.Select( a => a.ConnectionActivityType ) )
                .Include( r => r.ConnectionRequestActivities.Select( a => a.CreatedByPersonAlias.Person ) )
                .FirstOrDefault( r => r.Id == connectionRequestId );

            var aiProviderComponent = aiProvider.GetAIComponent();
            var promptWithContext = AttachAIPromptContext( prompt, connectionRequest );

            var completionsRequest = new ChatCompletionsRequest
            {
                Messages = new List<ChatCompletionsRequestMessage>
                {
                    new ChatCompletionsRequestMessage
                    {
                        Role = Rock.Enums.AI.ChatMessageRole.User,
                        Content = promptWithContext
                    }
                }
            };

            var response = await aiProviderComponent.GetChatCompletions( aiProvider, completionsRequest );
            var summary = response.Choices?.FirstOrDefault()?.Text ?? string.Empty;

            // Cache the generated summary for the configured duration so subsequent requests
            // can be served without consuming additional AI credits.
            if ( cacheDurationMinutes > 0 )
            {
                RockCache.AddOrUpdate( cacheKey, null, summary, RockDateTime.Now.AddMinutes( cacheDurationMinutes.Value ) );
            }

            return ActionOk( new GetAISummaryResponseBag { Summary = summary } );
        }

        /// <summary>
        /// Inserts or updates a Connection Request Note. If no Note IdKey is provided, a new note
        /// is created on the Connection Request for the current user. If an existing Note IdKey is
        /// provided, that note is updated. Edit authorization is checked on both the note and the
        /// Connection Request before any changes are applied.
        /// Returns an updated activity entry bag to add or refresh the note in the activity feed
        /// without a full page reload.
        /// </summary>
        /// <param name="bag">A bag containing the Connection Request IdKey, optional Note IdKey, and the note text to save.</param>
        /// <returns>A Block Action Result containing an <see cref="ActivityEntryBag"/> representing the saved note for display in the activity feed. Returns a bad request result if the Connection Request or Note cannot be found, or if the current user is not authorized to view the request or edit the note.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( ConnectionRequestNoteBag bag )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );
            var connectionRequest = connectionRequestService.Get( bag.ConnectionRequestIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to view this Connection Request." );
            }

            var noteService = new NoteService( RockContext );
            Note note;

            if ( bag.NoteIdKey.IsNullOrWhiteSpace() )
            {
                note = new Note
                {
                    EntityId = connectionRequest.Id,
                    NoteTypeId = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE.AsGuid() ).Id,
                    CreatedByPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId
                };

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "You are not authorized to add a note to this Connection Request." );
                }

                noteService.Add( note );
            }
            else
            {
                note = noteService.Get( bag.NoteIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( note == null )
                {
                    return ActionBadRequest( $"{Rock.Model.Note.FriendlyTypeName} not found." );
                }

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "You are not authorized to edit this Note." );
                }
            }

            note.Text = bag.NoteText;

            RockContext.SaveChanges();

            var activityEntryBag = new ActivityEntryBag
            {
                Key = $"{ActivityEntryType.RequestNote}_{IdHasher.Instance.GetHash( note.Id )}",
                EntryType = ActivityEntryType.RequestNote,
                EntryDateTime = note.CreatedDateTime?.ToRockDateTimeOffset(),
                CreatedBy = note.CreatedByPersonName,
                CardEntry = new CardEntryBag
                {
                    Title = "Request Note",
                    Content = bag.NoteText,
                    PhotoUrl = note.CreatedByPersonPhotoUrl
                }
            };

            return ActionOk( activityEntryBag );
        }

        /// <summary>
        /// Deletes the specified Connection Request Note after verifying that the current user
        /// is authorized to edit it. Returns the activity feed entry key of the deleted note
        /// so the client can remove it from the activity feed without a full page reload.
        /// </summary>
        /// <param name="noteIdKey">The IdKey of the Note to delete.</param>
        /// <returns>A Block Action Result containing the activity feed entry key of the deleted note. Returns a bad request result if the note cannot be found or the current user is not authorized to edit it.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( string noteIdKey )
        {
            var noteService = new NoteService( RockContext );
            var note = noteService.Get( noteIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( note == null )
            {
                return ActionBadRequest( $"{Rock.Model.Note.FriendlyTypeName} not found." );
            }

            if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to edit this Note." );
            }

            noteService.Delete( note );

            RockContext.SaveChanges();

            var key = $"{ActivityEntryType.RequestNote}_{noteIdKey}";

            return ActionOk( key );
        }

        /// <summary>
        /// Deletes the specified Connection Request Activity after verifying that the current user
        /// is authorized to edit the associated Connection Request. Returns the activity feed entry
        /// key of the deleted activity so the client can remove it from the activity feed
        /// without a full page reload.
        /// </summary>
        /// <param name="activityIdKey">The IdKey of the Connection Request Activity to delete.</param>
        /// <returns>A Block Action Result containing the activity feed entry key of the deleted activity. Returns a bad request result if the activity cannot be found or the current user is not authorized to edit the associated Connection Request.</returns>
        [BlockAction]
        public BlockActionResult DeleteActivity( string activityIdKey )
        {
            var activityService = new ConnectionRequestActivityService( RockContext );

            var activity = activityService.Get( activityIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( activity == null )
            {
                return ActionBadRequest( $"{ConnectionRequestActivity.FriendlyTypeName} not found." );
            }

            if ( !activity.ConnectionRequest.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to delete this activity." );
            }

            activityService.Delete( activity );

            RockContext.SaveChanges();

            var key = $"{ActivityEntryType.Activity}_{activityIdKey}";

            return ActionOk( key );
        }

        /// <summary>
        /// Gets the Connection Request entity bag for editing the specified Connection Request,
        /// loading all attributes and returning a valid properties box populated with all
        /// editable fields.
        /// </summary>
        /// <param name="key">The IdKey of the Connection Request to retrieve for editing.</param>
        /// <returns>A Block Action Result containing a <see cref="ValidPropertiesBox{ConnectionRequestBag}"/> with all editable properties populated. Returns a bad request result if the entity cannot be found or the current user is not authorized to edit it.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<ConnectionRequestBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Gets the transfer details for the specified Connection Request, including the current
        /// request state, available campuses, connection statuses, and all Connection Opportunities
        /// within the same Connection Type. Each opportunity is populated with its available
        /// connectors, campuses, and attribute values to support the transfer workflow.
        /// </summary>
        /// <remarks>
        /// Connector lists for each opportunity are built from connector group members, with the
        /// request's current connector and the current user each guaranteed to be present.
        /// Opportunity attributes are filtered to only those that are searchable and authorized
        /// for the current user. The current request's connector is carried forward as the default
        /// connector selection on the transfer form.
        /// </remarks>
        /// <param name="key">The IdKey of the Connection Request to retrieve transfer details for.</param>
        /// <returns>A Block Action Result containing a <see cref="TransferConnectionRequestDetailsBag"/> with all data needed to render the transfer form. Returns a bad request result if the Connection Request cannot be found or the current user is not authorized to edit it.</returns>
        [BlockAction]
        public BlockActionResult GetTransferDetails( string key )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );
            var connectionRequest = connectionRequestService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditSpecifiedConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var connectionType = ConnectionTypeCache.Get( connectionRequest.ConnectionTypeId );

            var tempOpportunity = new ConnectionOpportunity
            {
                ConnectionTypeId = connectionType.Id
            };

            tempOpportunity.LoadAttributes();

            var connectionOpportunities = new ConnectionOpportunityService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( "ConnectionOpportunityConnectorGroups.ConnectorGroup.Members" )
                .Include( "ConnectionOpportunityCampuses.Campus" )
                .Where( o => o.ConnectionTypeId == connectionType.Id )
                .ToList();

            var campuses = connectionOpportunities
                .SelectMany( o => o.ConnectionOpportunityCampuses )
                .Where( c => c.Campus != null && c.Campus.IsActive == true )
                .DistinctBy( c => c.CampusId )
                .Select( c => new ListItemBag
                {
                    Value = c.Campus.Guid.ToString(),
                    Text = c.Campus.Name
                } )
                .ToList();

            bool? entityAuthorized = null;

            var transferDetailsBag = new TransferConnectionRequestDetailsBag
            {
                ConnectionRequestIdKey = connectionRequest.IdKey,
                CurrentConnectorName = connectionRequest.ConnectorPersonAlias?.Person?.FullName ?? "No Connector",
                CurrentConnectorPersonAliasGuid = connectionRequest.ConnectorPersonAlias?.Guid,
                CurrentConnectionOpportunityGuid = connectionRequest.ConnectionOpportunity.Guid,
                CurrentCampusGuid = connectionRequest.Campus?.Guid,
                CurrentConnectionStatusGuid = connectionRequest.ConnectionStatus.Guid,
                Campuses = campuses,
                Statuses = connectionType.OrderedStatuses.ToListItemBagList(),
                ConnectionOpportunities = new List<ConnectionOpportunityBag>(),
                Attributes = tempOpportunity.GetPublicAttributesForEdit( RequestContext.CurrentPerson, false, a => a.AllowSearch && Rock.ExtensionMethods.IsAttributeAuthorized( tempOpportunity, ref entityAuthorized, a, Authorization.VIEW, RequestContext.CurrentPerson ) )
            };

            foreach( var opportunity in connectionOpportunities )
            {
                opportunity.LoadAttributes();
                bool? otherEntityAuthorized = null;

                var opportunityBag = new ConnectionOpportunityBag
                {
                    Name = opportunity.Name,
                    Guid = opportunity.Guid,
                    Campuses = opportunity.ConnectionOpportunityCampuses.Where( c => c.Campus != null && c.Campus.IsActive == true )
                        .Select( c => c.Campus )
                        .ToListItemBagList(),
                    PhotoUrl = ConnectionOpportunity.GetPhotoUrl( opportunity.PhotoId ),
                    IconCssClass = opportunity.IconCssClass,
                    Description = opportunity.Description,
                    ShowCampusOnTransfer = opportunity.ShowCampusOnTransfer,
                    ShowStatusOnTransfer = opportunity.ShowStatusOnTransfer,
                    AttributeValues = opportunity.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, false, a => a.AllowSearch && Rock.ExtensionMethods.IsAttributeAuthorized( tempOpportunity, ref otherEntityAuthorized, a, Authorization.VIEW, RequestContext.CurrentPerson ) )
                };

                transferDetailsBag.ConnectionOpportunities.Add( opportunityBag );
            }

            return ActionOk( transferDetailsBag );
        }

        /// <summary>
        /// Transfers a Connection Request to a new Connection Opportunity, optionally updating
        /// the status, campus, connector, and due date based on the new opportunity's transfer
        /// configuration and the provided bag values. Placement group assignments are cleared
        /// on transfer. A transfer activity is logged if the standard Transferred activity type
        /// is configured. Returns an updated Connection List Update Box to refresh both the
        /// grid row and detail panel without a full page reload.
        /// </summary>
        /// <remarks>
        /// Connector assignment is driven by the <see cref="TransferConnectionRequestBag.ConnectorOption"/>
        /// value: "default" assigns the opportunity's default connector for the selected campus,
        /// "none" clears the connector, and "select" assigns the explicitly chosen connector.
        /// Campus and status updates are only applied if the new opportunity has
        /// <see cref="ConnectionOpportunity.ShowCampusOnTransfer"/> or
        /// <see cref="ConnectionOpportunity.ShowStatusOnTransfer"/> enabled respectively.
        /// </remarks>
        /// <param name="bag">A bag containing the Connection Request IdKey, target opportunity GUID, connector option, optional campus, status, connector, due date, and transfer note.</param>
        /// <returns>A Block Action Result containing a <see cref="ConnectionListUpdateBox"/> to refresh the grid row and detail panel. Returns a bad request result if the Connection Request or target opportunity cannot be found, the request is already assigned to the selected opportunity, the current user is not authorized to edit the request, or any selected status, campus, or connector is invalid.</returns>
        [BlockAction]
        public BlockActionResult TransferConnectionRequest( TransferConnectionRequestBag bag )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );
            var connectionRequest = connectionRequestService.Get( bag.ConnectionRequestIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !CanEditSpecifiedConnectionRequest( connectionRequest, out var error ) )
            {
                return error;
            }

            var connectionActivityTypeService = new ConnectionActivityTypeService( RockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( RockContext );
            var connectionStatusService = new ConnectionStatusService( RockContext );
            var connectionOpportunityCampusService = new ConnectionOpportunityCampusService( RockContext );
            var personAliasService = new PersonAliasService( RockContext );

            Guid? newOpportunityGuid = bag.NewConnectionOpportunityGuid;
            int? sourceConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
            int sourceOpportunityId = connectionRequest.ConnectionOpportunityId;

            if ( !newOpportunityGuid.HasValue )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            var newOpportunity = new ConnectionOpportunityService( RockContext ).Get( newOpportunityGuid.Value );

            if ( newOpportunity == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            connectionRequest.ConnectionOpportunityId = newOpportunity.Id;
            connectionRequest.ConnectionTypeId = newOpportunity.ConnectionTypeId;

            // If the Opportunity has not "transferred" then return an error
            if ( connectionRequest.ConnectionOpportunityId == sourceOpportunityId )
            {
                return ActionBadRequest( "This request already belongs to the selected opportunity. Please choose a different opportunity to transfer to." );
            }

            if ( newOpportunity.ShowStatusOnTransfer && bag.StatusGuid.HasValue )
            {
                var connectionStatusId = connectionStatusService.Queryable()
                    .Where( s => s.ConnectionTypeId == connectionRequest.ConnectionTypeId && s.Guid == bag.StatusGuid.Value )
                    .Select( s => s.Id )
                    .FirstOrDefault();

                if ( connectionStatusId == 0 )
                {
                    return ActionBadRequest( $"{Rock.Model.ConnectionStatus.FriendlyTypeName} not found." );
                }

                connectionRequest.ConnectionStatusId = connectionStatusId;
            }

            if ( newOpportunity.ShowCampusOnTransfer && bag.CampusGuid.HasValue )
            {
                var campus = CampusCache.Get( bag.CampusGuid.Value );

                // Stricter check to verify that the selected campus is an option for the selected opportunity
                var campusId = connectionOpportunityCampusService.Queryable()
                    .Where( c => c.ConnectionOpportunityId == newOpportunity.Id && c.CampusId == campus.Id )
                    .Select( c => c.CampusId )
                    .FirstOrDefault();

                if ( campusId == 0 )
                {
                    return ActionBadRequest( $"{Rock.Model.ConnectionOpportunityCampus.FriendlyTypeName} not found." );
                }

                connectionRequest.CampusId = campusId;
            }
            else if ( newOpportunity.ShowCampusOnTransfer )
            {
                // If the campus dropdown was shown but there was no value selected then set the campus id to null.
                connectionRequest.CampusId = null;
            }

            // Clear anything related to placement groups on transfer.
            connectionRequest.AssignedGroupId = null;
            connectionRequest.AssignedGroupMemberRoleId = null;
            connectionRequest.AssignedGroupMemberStatus = null;

            // assign the connector based on the selected option
            if ( bag.ConnectorOption == "default" )
            {
                connectionRequest.ConnectorPersonAliasId = newOpportunity.GetDefaultConnectorPersonAliasId( connectionRequest.CampusId );
            }
            else if ( bag.ConnectorOption == "none" )
            {
                connectionRequest.ConnectorPersonAliasId = null;
            }
            else if ( bag.ConnectorOption == "select" )
            {
                if ( !bag.ConnectorPersonAliasGuid.HasValue )
                {
                    return ActionBadRequest( "Connector not found." );
                }

                var newConnectorId = personAliasService.GetId( bag.ConnectorPersonAliasGuid.Value );

                if ( !newConnectorId.HasValue )
                {
                    return ActionBadRequest( "Connector not found." );
                }

                connectionRequest.ConnectorPersonAliasId = newConnectorId.Value;
            }

            var activityTransferGuid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
            var transferredActivityId = connectionActivityTypeService.Queryable()
                .Where( t => t.Guid == activityTransferGuid )
                .Select( t => t.Id )
                .FirstOrDefault();

            if ( transferredActivityId > 0 )
            {
                // Add a new request activity to log the transfer
                connectionRequestActivityService.Add( new ConnectionRequestActivity
                {
                    ConnectionRequestId = connectionRequest.Id,
                    ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                    ConnectionActivityTypeId = transferredActivityId,
                    Note = bag.Note,
                    ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId
                } );
            }

            RockContext.SaveChanges();

            var updateBox = GetConnectionListUpdateBox( connectionRequest.Id, true );

            return ActionOk( updateBox );
        }

        /// <summary>
        /// Gets the full list of activity feed entries for the specified Connection Request,
        /// including activities, history records, communications, and request notes,
        /// used to populate or refresh the activity feed in the detail panel without a full page reload.
        /// </summary>
        /// <param name="connectionRequestIdKey">The IdKey of the Connection Request to retrieve activity entries for.</param>
        /// <returns>A Block Action Result containing a list of <see cref="ActivityEntryBag"/> objects ordered by entry date descending. Returns a bad request result if the Connection Request cannot be found or the current user is not authorized to view it.</returns>
        [BlockAction]
        public BlockActionResult GetActivityEntries( string connectionRequestIdKey )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );
            var connectionRequest = connectionRequestService.GetInclude( connectionRequestIdKey, c => c.ConnectionRequestActivities.Select( a => a.ConnectionActivityType ), !PageCache.Layout.Site.DisablePredictableIds );
            if ( connectionRequest == null )
            {
                return ActionBadRequest( $"{Rock.Model.ConnectionRequest.FriendlyTypeName} not found." );
            }

            if ( !connectionRequest.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to view this Connection Request." );
            }

            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( "ConnectionRequest", connectionRequest );
            mergeFields.Add( "Person", connectionRequest.PersonAlias.Person );

            var activityEntries = GetActivityEntries( connectionRequest, mergeFields );

            return ActionOk( activityEntries );
        }

        #endregion Detail View Block Actions

        #region Communication Block Actions

        [BlockAction]
        public BlockActionResult GetSmsConfiguration( GetSmsConfigurationRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Request is required" );
            }

            if ( bag.ConnectionTypeIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Connection Type is required" );
            }

            if ( bag.ConnectionRequestIdKeys == null )
            {
                return ActionBadRequest( "Connection Requests are required" );
            }

            var connectionType = new ConnectionTypeService( RockContext )
                .GetQueryableByKey( bag.ConnectionTypeIdKey )
                .FirstOrDefault();

            if ( connectionType == null )
            {
                return ActionBadRequest( "Invalid Connection Type." );
            }

            // Convert the connection request id keys to integer ids so we can query them.
            var connectionRequestIds = bag.ConnectionRequestIdKeys
                .Select( idKey => IdHasher.Instance.GetId( idKey ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var connectionRequestService = new ConnectionRequestService( RockContext );
            var mobilePhoneDefinedValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            var personalDeviceQuery = new PersonalDeviceService( RockContext ).Queryable().AsNoTracking();
            
            var communicationRecipients = connectionRequestService
                .GetByIds( connectionRequestIds )
                .Where( cr => cr.ConnectionTypeId == connectionType.Id ) // Ensure these Connection Requests match the Connection Type.
                .AsNoTracking()
                .Select( cr => new
                {
                    PersonAliasGuid = cr.PersonAlias.Guid,
                    cr.Guid,
                    cr.PersonAlias.Person,
                    MobilePhone = cr
                        .PersonAlias
                        .Person
                        .PhoneNumbers
                        .FirstOrDefault( pn => pn.NumberTypeValueId == mobilePhoneDefinedValueId ),
                } )
                .ToList() // materialize query before projecting because some properties require the full Person entity.
                .Select( cr => new CommunicationRecipientBag
                {
                    ConnectionRequestGuid = cr.Guid,
                    IsDeceased = cr.Person.IsDeceased,
                    IsSmsAllowed = cr.MobilePhone != null
                        && cr.MobilePhone.Number.IsNotNullOrWhiteSpace() == true
                        && cr.MobilePhone.IsMessagingEnabled
                        && !cr.MobilePhone.IsMessagingOptedOut,
                    Name = cr.Person.FullName,
                    PersonAliasGuid = cr.PersonAliasGuid,
                    PhotoUrl = cr.Person.PhotoUrl,
                    SmsNumber = cr.MobilePhone?.NumberFormatted
                } )
                .ToList();

            var currentPerson = GetCurrentPerson();
            var currentPersonAliasIds = currentPerson?.Aliases?.Select( a => a.Id ).ToList() ?? new List<int>();
            var smsFromSystemPhoneNumbers = SystemPhoneNumberCache
                    .All( includeInactive: false )
                    .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    .OrderByDescending( spn => spn.AssignedToPersonAliasId.HasValue && currentPersonAliasIds.Contains( spn.AssignedToPersonAliasId.Value ) )
                    .ThenBy( spn => spn.Order )
                    .ThenBy( spn => spn.Name )
                    .ThenBy( spn => spn.Id )
                    .ToListItemBagList();

            var snippetTypeGuid = SystemGuid.SnippetType.SMS.AsGuid();
            var smsSnippetTypeId = new SnippetTypeService( RockContext )
                .Queryable()
                .Where( st => st.Guid == snippetTypeGuid )
                .Select( st => ( int? ) st.Id )
                .FirstOrDefault();

            var smsSnippetCategoryGuid = connectionType.GetConnectionTypeAdditionalSettings()?.CommunicationSettings?.SmsSnippetCategoryGuid;
            var snippetCategoryId = smsSnippetCategoryGuid.HasValue
                    ? CategoryCache.GetId( smsSnippetCategoryGuid.Value )
                    : ( int? ) null;

            var smsSnippets = new SnippetService( RockContext )
                .GetAuthorizedSnippets(
                    currentPerson,
                    s => s.SnippetTypeId == smsSnippetTypeId // This will return an empty list if smsSnippetTypeId is null.
                        && ( !snippetCategoryId.HasValue || s.CategoryId == snippetCategoryId.Value )
                )
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .ThenBy( s => s.Id )
                .Select( s => new ListItemBag
                {
                    Text = s.Name,
                    Value = s.Content
                } )
                .ToList();

            return ActionOk( new GetSmsConfigurationResponseBag
            {
                CommunicationRecipients = communicationRecipients,
                SmsFromSystemPhoneNumbers = smsFromSystemPhoneNumbers,
                SmsSnippets = smsSnippets
            } );
        }

        [BlockAction]
        public BlockActionResult GetEmailConfiguration( GetEmailConfigurationRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Request is required" );
            }

            if ( bag.ConnectionTypeIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Connection Type is required" );
            }

            if ( bag.ConnectionRequestIdKeys == null )
            {
                return ActionBadRequest( "Connection Requests are required" );
            }

            var connectionType = new ConnectionTypeService( RockContext )
                .GetQueryableByKey( bag.ConnectionTypeIdKey )
                .FirstOrDefault();

            if ( connectionType == null )
            {
                return ActionBadRequest( "Invalid Connection Type." );
            }

            // Convert the connection request id keys to integer ids so we can query them.
            var connectionRequestIds = bag.ConnectionRequestIdKeys
                .Select( idKey => IdHasher.Instance.GetId( idKey ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var connectionRequestService = new ConnectionRequestService( RockContext );
            var mobilePhoneDefinedValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            //var personalDeviceQuery = new PersonalDeviceService( RockContext ).Queryable().AsNoTracking();
            
            var communicationRecipients = connectionRequestService
                .GetByIds( connectionRequestIds )
                .Where( cr => cr.ConnectionTypeId == connectionType.Id ) // Ensure these Connection Requests match the Connection Type.
                .AsNoTracking()
                .Select( cr => new
                {
                    PersonAliasGuid = cr.PersonAlias.Guid,
                    cr.PersonAlias.Person,
                    cr.Guid
                } )
                .ToList() // materialize query before projecting because some properties require the full Person entity.
                .Select( cr => new CommunicationRecipientBag
                {
                    ConnectionRequestGuid = cr.Guid,
                    Email = cr.Person.Email,
                    EmailPreference = cr.Person.EmailPreference,
                    IsEmailAllowed = cr.Person.CanReceiveEmail( isBulk: false ),
                    IsEmailActive = cr.Person.IsEmailActive, // Used for customized error messages.
                    IsDeceased = cr.Person.IsDeceased,
                    Name = cr.Person.FullName,
                    PersonAliasGuid = cr.PersonAliasGuid,
                    PhotoUrl = cr.Person.PhotoUrl,
                } )
                .ToList();

            var currentPerson = GetCurrentPerson();
            var mergeFields = this.RequestContext.GetCommonMergeFields();
            var communicationTemplateGuid = connectionType.GetConnectionTypeAdditionalSettings()?.CommunicationSettings?.CommunicationTemplateCategoryGuid;
            var communicationTemplates = new CommunicationTemplateService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( ct => ct.Attachments.Select( b => b.BinaryFile ) )
                .Where( ct =>
                    ct.IsActive
                    && !ct.UsageType.HasValue // Exclude templates that have an explicit usage type (e.g., Communication Flow templates)
                    && ( !communicationTemplateGuid.HasValue || ct.Category.Guid == communicationTemplateGuid.Value )
                )
                .ToList()
                .Where( ct =>
                    ct.IsAuthorized( Authorization.VIEW, currentPerson )
                    && ct.HasEmailTemplate()
                    && !ct.SupportsEmailWizard() // Exclude wizard templates because we are showing a simple HTML editor that can't handle complex wizard templates.
                )
                .OrderBy( t => t.Name )
                .ThenBy( t => t.Id )
                .Select( t => new CommunicationTemplateBag
                {
                    IdKey = t.IdKey,
                    Name = t.Name,
                    FromEmail = t.FromEmail?.ResolveMergeFields( mergeFields ),
                    FromName = t.FromName?.ResolveMergeFields( mergeFields ),
                    Subject = t.Subject,
                    Message = t.Message?.ResolveMergeFields( mergeFields ),
                    EmailAttachmentBinaryFiles = t.GetAttachments( CommunicationType.Email )?.Select( cta => cta.BinaryFile )?.ToListItemBagList(),
                } )
                .ToList();

            return ActionOk( new GetEmailConfigurationResponseBag
            {
                CommunicationRecipients = communicationRecipients,
                CommunicationTemplates = communicationTemplates
            } );
        }

        [BlockAction]
        public BlockActionResult SendCommunication( CommunicationBag bag )
        {
            var communication = CreateCommunication( bag );

            var msg = new ProcessSendCommunication.Message
            {
                CommunicationId = communication.Id
            };
            msg.Send();

            return ActionOk();
        }

        #endregion Communication Block Actions

        #endregion Block Actions

        private Model.Communication CreateCommunication( CommunicationBag bag )
        {
            var senderPersonAliasId = GetCurrentPerson().PrimaryAliasId;
            var personAliasIds = bag.CommunicationRecipients
                .Select( r => r.PersonAliasGuid )
                .ToList();
            IQueryable<int> activeRecipientPersonAliasIdQuery = new PersonAliasService( RockContext )
                .Queryable()
                .Where( pa => personAliasIds.Contains( pa.Guid ) )
                .Select( pa => pa.Id );

            var communicationService = new CommunicationService( RockContext );
            var communicationTemplateService = new CommunicationTemplateService( RockContext );
            var binaryFileService = new BinaryFileService( RockContext );
            Model.Communication communication = null;

            if ( bag.CommunicationType == Enums.Communication.CommunicationType.Email )
            {
                var communicationTemplateId = new CommunicationTemplateService( RockContext )
                    .GetQueryableByKey( bag.CommunicationTemplateIdKey, !PageCache.Layout.Site.DisablePredictableIds )
                    .Select( ct => new
                    {
                        ct.Id
                    } )
                    .FirstOrDefault()?.Id;

                communication = communicationService.CreateEmailCommunication( new CommunicationService.CreateEmailCommunicationArgs
                {
                    BulkCommunication = true,
                    CommunicationTemplateId = communicationTemplateId,
                    FromAddress = bag.FromEmail,
                    FromName = bag.FromName,
                    FutureSendDateTime = null,
                    Message = bag.Message,
                    Name = bag.Subject,
                    RecipientPrimaryPersonAliasIds = activeRecipientPersonAliasIdQuery.ToList(),
                    RecipientStatus = CommunicationRecipientStatus.Pending,
                    ReplyTo = null,
                    SendDateTime = null, // This is actually the "sent" value and must be null here.
                    SenderPersonAliasId = senderPersonAliasId,
                    Subject = bag.Subject,
                    SystemCommunicationId = null
                } );

                var emailAttachmentGuids = bag.EmailAttachments
                    .Select( a => a.Value.AsGuidOrNull() )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .ToList();
                var attachmentIdMap = binaryFileService
                    .Queryable()
                    .Where( bf => emailAttachmentGuids.Contains( bf.Guid ) )
                    .Select( bf => new
                    {
                        bf.Id,
                        bf.Guid
                    } )
                    .ToList()
                    .ToDictionary( bf => bf.Guid, bf => bf.Id );

                foreach ( var attachment in bag.EmailAttachments )
                {
                    if ( attachmentIdMap.TryGetValue( attachment.Value.AsGuid(), out var id ) )
                    {
                        var newAttachment = new CommunicationAttachment
                        {
                            BinaryFileId = id,
                            CommunicationType = Model.CommunicationType.Email
                        };
                        communication.Attachments.Add( newAttachment );
                    }
                }
            }
            else if ( bag.CommunicationType == Enums.Communication.CommunicationType.SMS )
            {
                communication = communicationService.CreateSMSCommunication( new CommunicationService.CreateSMSCommunicationArgs
                {
                    CommunicationName = $"{bag.Subject}",
                    CommunicationTemplateId = null,
                    FromPrimaryPersonAliasId = senderPersonAliasId,
                    FromSystemPhoneNumber = SystemPhoneNumberCache.Get( bag.SmsFromSystemPhoneNumberGuid.Value ),
                    FutureSendDateTime = null,
                    Message = bag.SmsMessage,
                    ResponseCode = null,
                    SystemCommunicationId = null,
                    ToPrimaryPersonAliasIds = activeRecipientPersonAliasIdQuery.ToList()
                } );

                var smsAttachmentGuids = bag.SmsAttachments
                    .Select( a => a.Value.AsGuidOrNull() )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .ToList();
                var attachmentIdMap = binaryFileService
                    .Queryable()
                    .Where( bf => smsAttachmentGuids.Contains( bf.Guid ) )
                    .Select( bf => new
                    {
                        bf.Id,
                        bf.Guid
                    } )
                    .ToList()
                    .ToDictionary( bf => bf.Guid, bf => bf.Id );

                foreach ( var attachment in bag.SmsAttachments )
                {
                    if ( attachmentIdMap.TryGetValue( attachment.Value.AsGuid(), out var id ) )
                    {
                        var newAttachment = new CommunicationAttachment
                        {
                            BinaryFileId = id,
                            CommunicationType = Model.CommunicationType.SMS
                        };
                        communication.Attachments.Add( newAttachment );
                    }
                }
            }

            if ( communication != null )
            {
                // Always set Connection Request communications as NOT bulk
                // since they are being sent directly to each person to help
                // move their connection request along in its process,
                // not a bulk communication.
                communication.IsBulkCommunication = false;

                // Separate the CommunicationRecipients from the Communication
                // so the Communication can be saved without recipients.
                // Then we'll save the recipients using a bulk insert.
                var communicationRecipients = communication.Recipients;
                communication.Recipients = new List<CommunicationRecipient>();
                foreach ( var communicationRecipient in communicationRecipients )
                {
                    communicationRecipient.Communication = null;
                    communicationService.Context.Entry( communicationRecipient ).State = EntityState.Detached;
                }

                // Save the communication.
                communicationService.Add( communication );
                RockContext.SaveChanges();

                // Bulk insert the recipients for better performance.
                foreach ( var communicationRecipient in communicationRecipients )
                {
                    communicationRecipient.CommunicationId = communication.Id;
                }

                RockContext.BulkInsert( communicationRecipients );

                // Create RelatedEntity records to link the Communication to the Connection Requests.
                // This allows us to show the communication in the timeline of the related Connection Request.
                var connectionRequestEntityTypeId = EntityTypeCache.GetId<ConnectionRequest>();
                var communicationEntityTypeId = EntityTypeCache.GetId<Model.Communication>();
                if ( connectionRequestEntityTypeId.HasValue && communicationEntityTypeId.HasValue )
                {
                    // This WHERE IN query is expected to be efficient because ConnectionRequest.Guid is indexed
                    // and we are only querying for a small number of Connection Requests that match the communication recipients.
                    var connectionRequestGuids = bag.CommunicationRecipients.Select( cr => cr.ConnectionRequestGuid ).ToHashSet();
                    var relatedEntities = new ConnectionRequestService( RockContext ).Queryable()
                        .Where( cr => connectionRequestGuids.Contains( cr.Guid ) )
                        .Select( cr => cr.Id )
                        .ToList()
                        .Select( crId => new RelatedEntity
                        {
                            SourceEntityTypeId = connectionRequestEntityTypeId.Value,
                            SourceEntityId = crId,
                            TargetEntityTypeId = communicationEntityTypeId.Value,
                            TargetEntityId = communication.Id,
                        } )
                        .ToList();

                    RockContext.BulkInsert( relatedEntities );
                }
            }

            return communication;
        }

        /// <summary>
        /// Gets the grid builder for the communication list grid.
        /// </summary>
        /// <returns>The grid builder for the communication list grid.</returns>
        private GridBuilder<ConnectionRow> GetGridBuilder()
        {
            return new GridBuilder<ConnectionRow>()
                .WithBlock( this )
                .AddField( "idKey", a => a.ConnectionRequestId.AsIdKey() )
                .AddField( "connectorGrouping", a => a.ConnectorGrouping )
                .AddField( "campusGrouping", a => a.CampusGrouping )
                .AddField( "opportunityGrouping", a => a.OpportunityGrouping )
                .AddField( "statusGrouping", a => a.StatusGrouping )
                .AddField( "stateGrouping", a => a.StateGrouping )
                .AddField( "dueStatusGrouping", a => a.DueStatusGrouping )
                .AddField( "connectorDetails", a => a.ConnectorDetails )
                .AddField( "requestDetails", a => a.Person )
                .AddField( "requesterPersonAliasGuid", a => a.RequesterPersonAliasGuid )
                .AddTextField( "connectionOpportunity", a => a.ConnectionOpportunity )
                .AddField( "connectionOpportunityGuid", a => a.ConnectionOpportunityGuid)
                .AddTextField( "connectionTypeSource", a => a.ConnectionTypeSource )
                .AddTextField( "campus", a => a.Campus )
                .AddField( "campusGuid", a => a.CampusGuid )
                .AddTextField( "group", a => a.Group )
                .AddField( "connectionStatus", a => a.ConnectionStatus )
                .AddDateTimeField( "lastActivityDateTime", a => a.LastActivityDateTime )
                .AddField( "activityCount", a => a.ActivityCount )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddDateTimeField( "dueDate", a => a.DueDate )
                .AddDateTimeField( "dueSoonDate", a => a.DueSoonDate )
                .AddDateTimeField( "completedDateTime", a => a.CompletedDateTime )
                .AddField( "dueStatus", a => a.DueStatus )
                .AddDateTimeField( "followUpDate", a => a.FollowUpDate )
                .AddField( "connectionState", a => a.ConnectionState )
                .AddTextField( "celebrationText", a => a.CelebrationText )
                .AddField( "reminderCount", a => a.ReminderCount )
                .AddField( "hasPlacementGroup", a => a.HasPlacementGroup )
                .AddField( "hasRequiredGroupRequirements", a => a.HasRequiredGroupRequirements )
                .AddAttributeFieldsFrom( a => a.ConnectionRequest, GetGridAttributes() );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private List<AttributeCache> GetGridAttributes()
        {
            if ( _gridAttributes == null )
            {
                var availableAttributes = new List<AttributeCache>();
                var connectionTypeId = ConnectionTypeCache.Get( PageParameter( PageParameterKey.ConnectionType ), !PageCache.Layout.Site.DisablePredictableIds )?.Id;

                if ( connectionTypeId.HasValue )
                {
                    var entityTypeId = EntityTypeCache.Get<ConnectionRequest>( false )?.Id;
                    availableAttributes.AddRange( AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, "ConnectionTypeId", connectionTypeId.Value.ToString() ) );
                }

                _gridAttributes = availableAttributes;
            }

            return _gridAttributes;
        }

        private List<AttributeCache> _gridAttributes = null;

        #region Supporting Classes

        public class ConnectionRow
        {
            public int ConnectionRequestId { get; set; }

            public ConnectionRequest ConnectionRequest { get; set; }

            public GroupingFieldBag ConnectorGrouping { get; set; }

            public GroupingFieldBag OpportunityGrouping { get; set; }

            public GroupingFieldBag CampusGrouping { get; set; }

            public GroupingFieldBag StateGrouping { get; set; }

            public GroupingFieldBag StatusGrouping { get; set; }

            public GroupingFieldBag DueStatusGrouping { get; set; }

            public ListItemBag ConnectorDetails { get; set; }

            public PersonFieldBag Person { get; set; }

            public Guid RequesterPersonAliasGuid { get; set; }

            public int PersonAliasId { get; set; }

            public Guid? ConnectorPersonAliasGuid { get; set; }

            public int ConnectionOpportunityId { get; set; }

            public Guid ConnectionOpportunityGuid { get; set; }

            public string ConnectionOpportunity { get; set; }

            public string ConnectionOpportunityIcon { get; set; }

            public string ConnectionTypeSource { get; set; }

            public int? CampusId { get; set; }

            public string Campus { get; set; }

            public Guid? CampusGuid { get; set; }

            public int? AssignedGroupId { get; set; }

            public string Group { get; set; }

            public ConnectionStatusBag ConnectionStatus { get; set; }

            public ConnectionState ConnectionState { get; set; }

            public DateTime? LastActivityDateTime { get; set; }

            public int ActivityCount { get; set; }

            public DateTime? FollowUpDate { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public DateTime? DueDate { get; set; }

            public DateTime? DueSoonDate { get; set; }

            public DateTime? CompletedDateTime { get; set; }

            public DueStatus DueStatus { get; set; }

            public string CelebrationText { get; set; }

            public int ReminderCount { get; set; }

            /// <summary>
            /// Gets or sets whether this Connection Request has a Placement Group.
            /// </summary>
            public bool HasPlacementGroup { get; set; }

            /// <summary>
            /// Gets or sets whether this Connection Request has a placement group that requires certain group requirements to be met
            /// before the person can be added as a group member.
            /// </summary>
            public bool HasRequiredGroupRequirements { get; set; }
        }


        /// <summary>
        /// A flat data transfer object that receives results from the raw SQL query in
        /// <see cref="GetOtherGridData"/>. Property names must exactly match the column
        /// aliases returned by the query so that EF6 can map them automatically.
        /// </summary>
        public class ConnectionRequestSqlRow
        {
            /// <summary>Gets or sets the ConnectionRequest Id.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the ConnectionOpportunity Id.</summary>
            public int ConnectionOpportunityId { get; set; }

            /// <summary>Gets or sets the ConnectionOpportunity Guid.</summary>
            public Guid ConnectionOpportunityGuid { get; set; }

            /// <summary>Gets or sets the ConnectionOpportunity display name.</summary>
            public string ConnectionOpportunityName { get; set; }

            /// <summary>Gets or sets the CSS class for the ConnectionOpportunity icon.</summary>
            public string ConnectionOpportunityIconCssClass { get; set; }

            /// <summary>Gets or sets the sort order of the ConnectionOpportunity.</summary>
            public int ConnectionOpportunityOrder { get; set; }

            /// <summary>Gets or sets the name of the ConnectionTypeSource, or null when unset.</summary>
            public string ConnectionTypeSourceName { get; set; }

            /// <summary>Gets or sets the Campus Id, or null when none is assigned.</summary>
            public int? CampusId { get; set; }

            /// <summary>Gets or sets the Campus name, or null when none is assigned.</summary>
            public string CampusName { get; set; }

            /// <summary>Gets or sets the Campus Guid, or null when none is assigned.</summary>
            public Guid? CampusGuid { get; set; }

            /// <summary>Gets or sets the sort order of the Campus, or null when none is assigned.</summary>
            public int? CampusOrder { get; set; }

            /// <summary>Gets or sets the AssignedGroup Id, or null when none is assigned.</summary>
            public int? AssignedGroupId { get; set; }

            /// <summary>Gets or sets the AssignedGroup name, or null when none is assigned.</summary>
            public string AssignedGroupName { get; set; }

            /// <summary>Gets or sets the ConnectionStatus Id.</summary>
            public int ConnectionStatusId { get; set; }

            /// <summary>Gets or sets the ConnectionStatus Guid.</summary>
            public Guid ConnectionStatusGuid { get; set; }

            /// <summary>Gets or sets the ConnectionStatus display name.</summary>
            public string ConnectionStatusName { get; set; }

            /// <summary>Gets or sets the sort order of the ConnectionStatus.</summary>
            public int ConnectionStatusOrder { get; set; }

            /// <summary>Gets or sets the highlight color of the ConnectionStatus.</summary>
            public string ConnectionStatusHighlightColor { get; set; }

            /// <summary>Gets or sets whether a note is required when completing requests in this status.</summary>
            public bool ConnectionStatusIsNoteRequiredOnCompletion { get; set; }

            /// <summary>Gets or sets whether this is the default ConnectionStatus.</summary>
            public bool ConnectionStatusIsDefault { get; set; }

            /// <summary>Gets or sets the ConnectionState as its raw integer value for reliable SqlQuery mapping.</summary>
            public int ConnectionState { get; set; }

            /// <summary>Gets or sets the follow-up date, or null when none is set.</summary>
            public DateTime? FollowUpDate { get; set; }

            /// <summary>Gets or sets the date and time the request was created.</summary>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>Gets or sets the due date, or null when none is set.</summary>
            public DateTime? DueDate { get; set; }

            /// <summary>Gets or sets the "due soon" threshold date, or null when none is set.</summary>
            public DateTime? DueSoonDate { get; set; }

            /// <summary>Gets or sets the date and time the request was connected (completed), or null when still open.</summary>
            public DateTime? ConnectedDateTime { get; set; }

            /// <summary>Gets or sets the celebration text sourced from the first Celebration Note on the request.</summary>
            public string CelebrationText { get; set; }

            /// <summary>Gets or sets the PersonAlias Id of the requester.</summary>
            public int PersonAliasId { get; set; }

            /// <summary>Gets or sets the Guid of the requester's PersonAlias.</summary>
            public Guid RequesterPersonAliasGuid { get; set; }

            /// <summary>Gets or sets the Person Id of the requester.</summary>
            public int RequesterPersonId { get; set; }

            /// <summary>Gets or sets the requester's nick name.</summary>
            public string RequesterNickName { get; set; }

            /// <summary>Gets or sets the requester's last name.</summary>
            public string RequesterLastName { get; set; }

            /// <summary>Gets or sets the requester's photo binary file Id, or null when none is set.</summary>
            public int? RequesterPhotoId { get; set; }

            /// <summary>Gets or sets the requester's calculated age, or null when unknown.</summary>
            public int? RequesterAge { get; set; }

            /// <summary>Gets or sets the requester's Gender as its raw integer value, or null when unknown.</summary>
            public int? RequesterGender { get; set; }

            /// <summary>Gets or sets the requester's RecordType DefinedValue Id, or null when unknown.</summary>
            public int? RequesterRecordTypeValueId { get; set; }

            /// <summary>Gets or sets the requester's AgeClassification as its raw integer value, or null when unknown.</summary>
            public int? RequesterAgeClassification { get; set; }

            /// <summary>Gets or sets the requester's ConnectionStatus DefinedValue Id, or null when unknown.</summary>
            public int? RequesterConnectionStatusValueId { get; set; }

            /// <summary>Gets or sets the PersonAlias Id of the connector, or null when unassigned.</summary>
            public int? ConnectorPersonAliasId { get; set; }

            /// <summary>Gets or sets the Guid of the connector's PersonAlias, or null when unassigned.</summary>
            public Guid? ConnectorPersonAliasGuid { get; set; }

            /// <summary>Gets or sets the Person Id of the connector, or null when unassigned.</summary>
            public int? ConnectorPersonId { get; set; }

            /// <summary>Gets or sets the connector's nick name, or null when unassigned.</summary>
            public string ConnectorNickName { get; set; }

            /// <summary>Gets or sets the connector's last name, or null when unassigned.</summary>
            public string ConnectorLastName { get; set; }

            /// <summary>Gets or sets the connector's photo binary file Id, or null when unassigned or no photo.</summary>
            public int? ConnectorPhotoId { get; set; }

            /// <summary>Gets or sets the connector's calculated age, or null when unassigned or unknown.</summary>
            public int? ConnectorAge { get; set; }

            /// <summary>Gets or sets the connector's Gender as its raw integer value, or null when unassigned.</summary>
            public int? ConnectorGender { get; set; }

            /// <summary>Gets or sets the connector's RecordType DefinedValue Id, or null when unassigned or unknown.</summary>
            public int? ConnectorRecordTypeValueId { get; set; }

            /// <summary>Gets or sets the connector's AgeClassification as its raw integer value, or null when unassigned.</summary>
            public int? ConnectorAgeClassification { get; set; }

            /// <summary>Gets or sets the connector's ConnectionStatus DefinedValue Id, or null when unassigned or unknown.</summary>
            public int? ConnectorConnectionStatusValueId { get; set; }

            /// <summary>Gets or sets the total number of activities on this request; 0 when none exist.</summary>
            public int ActivityCount { get; set; }

            /// <summary>Gets or sets the date and time of the most recent activity, or null when there are none.</summary>
            public DateTime? LastActivityDateTime { get; set; }

            /// <summary>
            /// Gets or sets the number of incomplete, past-due reminders the current user has on the
            /// requester's PersonAlias; 0 when there are none.
            /// </summary>
            public int ReminderCount { get; set; }

            /// <summary>
            /// Gets or sets whether the assigned placement group has at least one mandatory group
            /// requirement. False when there is no placement group or no mandatory requirements exist.
            /// </summary>
            public bool HasRequiredGroupRequirements { get; set; }
        }

        public class HistoryRow
        {
            public int Id { get; set; }

            public string CreatedBy { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public string Verb { get; set; }

            public string ValueName { get; set; }

            public string NewValue { get; set; }

            public string OldValue { get; set; }
        }

        public class CommunicationRow
        {
            public int Id { get; set; }

            public CommunicationType CommunicationType { get; set; }

            public string Subject { get; set; }

            public string SMSMessage { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? PhotoId { get; set; }

            public int? Age { get; set; }

            public Gender? Gender { get; set; }

            public int? RecordTypeValueId { get; set; }

            public AgeClassification? AgeClassification { get; set; }

            public Guid? BinaryFileGuid { get; set; }

            public string FileName { get; set; }
        }

        #endregion Supporting Classes
    }
}

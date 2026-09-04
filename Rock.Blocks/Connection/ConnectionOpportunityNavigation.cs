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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Enums.Connection;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Connection.ConnectionOpportunityNavigation;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Displays metrics of a connection type's combined opportunities and provides easy navigation into each opportunity's connection requests.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Opportunity Navigation" )]
    [Category( "Connection" )]
    [Description( "Displays metrics of a connection type's combined opportunities and provides easy navigation into each opportunity's connection requests." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [LinkedPage( "Connections Hub Page",
        Key = AttributeKey.ConnectionsHubPage,
        Description = @"Select the page that the ""View Requests"", list, board, and grid buttons should open to view the connections hub.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_HUB,
        Order = 0,
        IsRequired = true )]

    [LinkedPage( "Operational Snapshot Page",
        Key = AttributeKey.OperationalSnapshotPage,
        Description = "Select the page that the snapshot button should open to view the operational snapshot.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT,
        Order = 1,
        IsRequired = true )]

    [LinkedPage( "My Connections Page",
        Key = AttributeKey.MyConnectionsPage,
        Description = "Select the page that the My Connections button should open to view a personal Connections workspace.",
        DefaultValue = Rock.SystemGuid.Page.MY_CONNECTIONS,
        Order = 2,
        IsRequired = true )]

    [LinkedPage( "Celebrations Report Page",
        Key = AttributeKey.CelebrationsReportPage,
        Description = "Select the page that the celebrations button should open to view the celebrations report.",
        Order = 2,
        IsRequired = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "6A3E1450-486E-45CF-8979-E280DACAEFEA" )]
    [Rock.SystemGuid.BlockTypeGuid( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE" )]
    public class ConnectionOpportunityNavigation : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ConnectionsHubPage = "ConnectionsHubPage";
            public const string OperationalSnapshotPage = "OperationalSnapshotPage";
            public const string MyConnectionsPage = "MyConnectionsPage";
            public const string CelebrationsReportPage = "CelebrationsReportPage";

        }

        private static class NavigationUrlKey
        {
            // Connection Type-level URLs.
            public const string TypeConnectionsHubListViewPage = "TypeConnectionsHubListViewPage";
            public const string TypeOperationalSnapshotPage = "TypeOperationalSnapshotPage";
            public const string TypeCelebrationsReportPage = "TypeCelebrationsReportPage";

            // Connection Opportunity-level URLs.
            public const string OpportunityConnectionsHubListViewPage = "OpportunityConnectionsHubListViewPage";
            public const string OpportunityConnectionsHubBoardViewPage = "OpportunityConnectionsHubBoardViewPage";
            public const string OpportunityConnectionsHubGridViewPage = "OpportunityConnectionsHubGridViewPage";

            // My Connections-level URLs.
            public const string MyConnectionsPage = "MyConnectionsPage";
        }

        private static class PageParameterKey
        {
            public const string ConnectionType = "ConnectionType";
            public const string ConnectionOpportunity = "ConnectionOpportunity";
            public const string Connector = "Connector";
            public const string IsMyConnectionsView = "IsMyConnectionsView";
            public const string CelebrationsReportConnectionTypeId = "ConnectionTypeId";
        }

        private static class PersonPreferenceKey
        {
            public const string OpportunityVisibility = "opportunity-visibility";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The list of opportunity visibility items the individual may select.
        /// </summary>
        private List<ListItemBag> _opportunityVisibilityItems;

        /// <summary>
        /// The identifiers of opportunities within the connection type for which the current person is a connector.
        /// </summary>
        private HashSet<int> _selfAssignedOpportunityIds;

        /// <summary>
        /// The identifiers of active opportunities within the connection type that the current person is not authorized to view.
        /// </summary>
        private HashSet<int> _unauthorizedOpportunityIds;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the list of opportunity visibility items the individual may select.
        /// </summary>
        private List<ListItemBag> OpportunityVisibilityItems
        {
            get
            {
                if ( _opportunityVisibilityItems == null )
                {
                    _opportunityVisibilityItems = new List<ListItemBag>
                    {
                        OpportunityVisibility.MyOpportunities,
                        OpportunityVisibility.AllOpportunites
                    };
                }

                return _opportunityVisibilityItems;
            }
        }

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets or sets the current person's opportunity visibility preference.
        /// </summary>
        private string OpportunityVisibilityPreference
        {
            get
            {
                var opportunityVisibility = BlockPersonPreferences
                    .GetValue( PersonPreferenceKey.OpportunityVisibility );

                if ( opportunityVisibility.IsNotNullOrWhiteSpace() )
                {
                    return opportunityVisibility;
                }

                return OpportunityVisibility.MyOpportunitiesValue;
            }
        }

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ConnectionOpportunityNavigationInitializationBox();

            var connectionType = GetConnectionTypeFromPageParameterOrOverride();
            if ( connectionType == null )
            {
                // Return early if unable to find the connection type.
                box.ErrorMessage = $"Unable to find the specified {ConnectionType.FriendlyTypeName}.";
                return box;
            }

            if ( !GetIsAuthorizedToView( connectionType ) )
            {
                // Return early if the current person is not authorized to view the connection type.
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( ConnectionType.FriendlyTypeName );
                return box;
            }

            if ( !connectionType.IsActive )
            {
                // Return early if the connection type is not active.
                box.ErrorMessage = $"The specified {ConnectionType.FriendlyTypeName} is not active.";
                return box;
            }

            box.ConnectionTypeItems = GetConnectionTypeItems();
            box.OpportunityVisibilityItems = OpportunityVisibilityItems;
            box.NavigationDetails = LoadNavigationDetails( connectionType );
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the connection opportunity metrics and summaries.
        /// </summary>
        /// <param name="bag">The information needed to get navigation details.</param>
        /// <returns>An object containing information about the connection opportunity metrics and summaries.</returns>
        [BlockAction]
        public BlockActionResult GetNavigationDetails( GetNavigationDetailsRequestBag bag )
        {
            var connectionType = GetConnectionTypeFromPageParameterOrOverride( bag?.ConnectionTypeIdKeyOverride );
            if ( connectionType == null )
            {
                return ActionBadRequest( $"Unable to find the specified {ConnectionType.FriendlyTypeName}." );
            }

            if ( !GetIsAuthorizedToView( connectionType ) )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( ConnectionType.FriendlyTypeName ) );
            }

            if ( !connectionType.IsActive )
            {
                return ActionBadRequest( $"The specified {ConnectionType.FriendlyTypeName} is not active." );
            }

            var response = LoadNavigationDetails( connectionType );

            return ActionOk( response );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the list of <see cref="ConnectionType"/> items the individual may select.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetConnectionTypeItems()
        {
            var currentPerson = GetCurrentPerson();
            return ConnectionTypeCache.All()
                .Where( ct =>
                    ct.IsActive
                    && (
                         ct.IsAuthorized( Authorization.EDIT, currentPerson )
                        || ct.IsAuthorized( Authorization.VIEW, currentPerson )
                    )
                )
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .Select( ct => new ListItemBag
                {
                    Text = ct.Name,
                    Value = ct.IdKey
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the <see cref="ConnectionTypeCache"/> based on the page parameter or identifier key override.
        /// </summary>
        /// <param name="connectionTypeIdKey">
        /// The optional <see cref="ConnectionType"/> identifier key that should override the page parameter.
        /// </param>
        /// <returns>An <see cref="ConnectionTypeCache"/> based on the page parameter or identifier key override.</returns>
        private ConnectionTypeCache GetConnectionTypeFromPageParameterOrOverride( string connectionTypeIdKey = null )
        {
            if ( connectionTypeIdKey.IsNullOrWhiteSpace() )
            {
                connectionTypeIdKey = PageParameter( PageParameterKey.ConnectionType );
            }

            return connectionTypeIdKey.IsNotNullOrWhiteSpace()
                ? ConnectionTypeCache.Get( connectionTypeIdKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;
        }

        /// <summary>
        /// Gets whether the current person is authorized to view [or edit] the <see cref="ConnectionTypeCache"/>.
        /// </summary>
        /// <param name="connectionType">The <see cref="ConnectionTypeCache"/> to check.</param>
        /// <returns>Whether the current person is authorized to view [or edit] the <see cref="ConnectionTypeCache"/>.</returns>
        private bool GetIsAuthorizedToView( ConnectionTypeCache connectionType )
        {
            var currentPerson = GetCurrentPerson();
            return connectionType.IsAuthorized( Authorization.VIEW, currentPerson )
                || connectionType.IsAuthorized( Authorization.EDIT, currentPerson );
        }

        /// <summary>
        /// Gets whether the current person is authorized to view [or edit] the specified <see cref="ConnectionOpportunity"/>.
        /// When the connection type has request security enabled, being a connector for any of the opportunity's
        /// requests also grants visibility.
        /// </summary>
        /// <param name="connectionType">The <see cref="ConnectionTypeCache"/> to which the opportunity belongs.</param>
        /// <param name="connectionOpportunityId">The <see cref="ConnectionOpportunity"/> identifier to check.</param>
        /// <returns>Whether the current person is authorized to view [or edit] the opportunity.</returns>
        private bool GetIsAuthorizedToView( ConnectionTypeCache connectionType, int connectionOpportunityId )
        {
            var currentPerson = GetCurrentPerson();

            // The authorization checks operate only against IDs, so we can create runtime instances with just the IDs
            // populated for efficiency instead of needing to load full entities from the database.
            var opportunity = new ConnectionOpportunity
            {
                Id = connectionOpportunityId,
                ConnectionTypeId = connectionType.Id,
                ConnectionType = new ConnectionType { Id = connectionType.Id }
            };

            return opportunity.IsAuthorized( Authorization.VIEW, currentPerson )
                || opportunity.IsAuthorized( Authorization.EDIT, currentPerson )
                || GetSelfAssignedOpportunityIds( connectionType ).Contains( connectionOpportunityId );
        }

        /// <summary>
        /// Gets the identifiers of opportunities within the connection type for which the current person is a connector.
        /// Empty unless the connection type has request security enabled.
        /// </summary>
        /// <param name="connectionType">The <see cref="ConnectionTypeCache"/> whose opportunities to check.</param>
        /// <returns>A <see cref="HashSet{T}"/> of self-assigned <see cref="ConnectionOpportunity"/> identifiers.</returns>
        private HashSet<int> GetSelfAssignedOpportunityIds( ConnectionTypeCache connectionType )
        {
            if ( _selfAssignedOpportunityIds == null )
            {
                var personId = GetCurrentPerson()?.Id;

                if ( !connectionType.EnableRequestSecurity || !personId.HasValue )
                {
                    _selfAssignedOpportunityIds = new HashSet<int>();
                }
                else
                {
                    _selfAssignedOpportunityIds = new ConnectionRequestService( RockContext )
                        .Queryable()
                        .Where( cr =>
                            cr.ConnectionOpportunity.ConnectionTypeId == connectionType.Id
                            && cr.ConnectorPersonAlias.PersonId == personId.Value
                        )
                        .Select( cr => cr.ConnectionOpportunityId )
                        .Distinct()
                        .ToHashSet();
                }
            }

            return _selfAssignedOpportunityIds;
        }

        /// <summary>
        /// Gets the identifiers of the connection type's active opportunities that the current person is not
        /// authorized to view [or edit].
        /// </summary>
        /// <param name="connectionType">The <see cref="ConnectionTypeCache"/> whose opportunities to check.</param>
        /// <returns>A <see cref="HashSet{T}"/> of unauthorized <see cref="ConnectionOpportunity"/> identifiers.</returns>
        private HashSet<int> GetUnauthorizedOpportunityIds( ConnectionTypeCache connectionType )
        {
            if ( _unauthorizedOpportunityIds == null )
            {
                _unauthorizedOpportunityIds = new ConnectionOpportunityService( RockContext )
                    .Queryable()
                    .Where( co =>
                        co.ConnectionTypeId == connectionType.Id
                        && co.IsActive
                    )
                    .Select( co => co.Id )
                    .ToList()
                    .Where( id => !GetIsAuthorizedToView( connectionType, id ) )
                    .ToHashSet();
            }

            return _unauthorizedOpportunityIds;
        }

        /// <summary>
        /// Loads connection opportunity metrics and summaries for the provided <paramref name="connectionTypeId"/>.
        /// </summary>
        /// <param name="connectionType">
        /// The <see cref="ConnectionTypeCache"/> for which to load navigation details.
        /// </param>
        /// <returns>A <see cref="ConnectionOpportunityNavigationDetailsBag"/>.</returns>
        private ConnectionOpportunityNavigationDetailsBag LoadNavigationDetails( ConnectionTypeCache connectionType )
        {
            return new ConnectionOpportunityNavigationDetailsBag
            {
                ConnectionTypeSummary = LoadConnectionTypeSummary( connectionType ),
                ConnectionOpportunitySummaries = LoadConnectionOpportunitySummaries( connectionType ),
                RequestCountsPerDay = LoadRequestsCountsPerDay( connectionType )
            };
        }

        /// <summary>
        /// Loads connection type summary information into a <see cref="ConnectionTypeSummaryBag"/>.
        /// </summary>
        /// <param name="connectionType">
        /// The <see cref="ConnectionTypeCache"/> for which to load summary information.
        /// </param>
        /// <returns>A <see cref="ConnectionTypeSummaryBag"/>.</returns>
        private ConnectionTypeSummaryBag LoadConnectionTypeSummary( ConnectionTypeCache connectionType )
        {
            return new ConnectionTypeSummaryBag
            {
                IconCssClass = connectionType.IconCssClass,
                Name = connectionType.Name,
                EnabledViews = connectionType.EnabledViews,
                IsCelebrationEnabled = connectionType.EnabledFeatures.HasFlag( EnabledFeatureFlags.Celebration )
            };
        }

        /// <summary>
        /// Loads <see cref="ConnectionOpportunity"/> data from the database and uses this data to buld a list of
        /// <see cref="ConnectionOpportunitySummaryBag"/>s.
        /// </summary>
        /// <param name="connectionType">
        /// The <see cref="ConnectionTypeCache"/> for which to load <see cref="ConnectionOpportunity"/> data.
        /// </param>
        /// <returns>A list of <see cref="ConnectionOpportunitySummaryBag"/>s.</returns>
        private List<ConnectionOpportunitySummaryBag> LoadConnectionOpportunitySummaries( ConnectionTypeCache connectionType )
        {
            var connectionTypeId = connectionType.Id;
            var personId = GetCurrentPerson()?.Id ?? 0;
            var campusId = RequestContext.GetContextEntity<Campus>()?.Id;
            var today = RockDateTime.Today;

            var connectionRequestQry = new ConnectionRequestService( RockContext )
                .Queryable()
                .Where( cr =>
                    cr.ConnectionState == ConnectionState.Active
                    && ( !campusId.HasValue || cr.CampusId == campusId.Value )
                    && cr.ConnectionOpportunity.ConnectionTypeId == connectionTypeId
                    && cr.ConnectionOpportunity.ConnectionType.IsActive
                    && cr.ConnectionOpportunity.IsActive
                );

            var connectionOpportunityQry = new ConnectionOpportunityService( RockContext )
                .Queryable()
                .Where( co =>
                    co.ConnectionTypeId == connectionTypeId
                    && co.ConnectionType.IsActive
                    && co.IsActive
                );

            if ( OpportunityVisibilityPreference == OpportunityVisibility.MyOpportunitiesValue )
            {
                connectionRequestQry = connectionRequestQry
                    .Where( cr =>
                        cr.ConnectorPersonAliasId.HasValue
                        && cr.ConnectorPersonAlias.PersonId == personId
                    );

                connectionOpportunityQry = connectionOpportunityQry
                    .Where( co => connectionRequestQry.Any( cr =>
                            cr.ConnectionOpportunityId == co.Id
                        )
                    );
            }

            var requestCountsQry = connectionRequestQry
                .GroupBy( cr => cr.ConnectionOpportunityId )
                .Select( g => new
                {
                    ConnectionOpportunityId = g.Key,
                    ActiveRequestCount = g.Count(), // They're all active because of the filter above.
                    DueSoonRequestCount = g.Count( r =>
                        r.DueSoonDate.HasValue
                        && DbFunctions.TruncateTime( r.DueSoonDate.Value ) <= today
                        && !(
                            r.DueDate.HasValue
                            && DbFunctions.TruncateTime( r.DueDate.Value ) < today
                        )
                    ),
                    OverdueRequestCount = g.Count( r =>
                        r.DueDate.HasValue
                        && DbFunctions.TruncateTime( r.DueDate.Value ) < today
                    ),
                    UnassignedRequestCount = g.Count( r => !r.ConnectorPersonAliasId.HasValue ),
                    AssignedToYouRequestCount = g.Count( r =>
                        r.ConnectorPersonAliasId.HasValue
                        && r.ConnectorPersonAlias.PersonId == personId
                    )
                } );

            var summaries = connectionOpportunityQry
                .GroupJoin(
                    requestCountsQry,
                    co => co.Id,
                    counts => counts.ConnectionOpportunityId,
                    ( co, counts ) => new
                    {
                        ConnectionOpportunity = co,
                        RequestCounts = counts
                    }
                )
                .SelectMany(
                    x => x.RequestCounts.DefaultIfEmpty(),
                    ( x, counts ) => new ConnectionOpportunitySummaryBag
                    {
                        Id = x.ConnectionOpportunity.Id,
                        IconCssClass = x.ConnectionOpportunity.IconCssClass,
                        Name = x.ConnectionOpportunity.Name,
                        Summary = x.ConnectionOpportunity.Summary,
                        Order = x.ConnectionOpportunity.Order,
                        ActiveRequestCount = counts == null ? 0 : counts.ActiveRequestCount,
                        DueSoonRequestCount = counts == null ? 0 : counts.DueSoonRequestCount,
                        OverdueRequestCount = counts == null ? 0 : counts.OverdueRequestCount,
                        UnassignedRequestCount = counts == null ? 0 : counts.UnassignedRequestCount,
                        AssignedToYouRequestCount = counts == null ? 0 : counts.AssignedToYouRequestCount
                    }
                )
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .ToList();

            // Filter out any opportunities that the current person is not authorized to view.
            var unauthorizedOpportunityIds = GetUnauthorizedOpportunityIds( connectionType );
            summaries.RemoveAll( s => unauthorizedOpportunityIds.Contains( s.Id ?? 0 ) );

            var currentPerson = GetCurrentPerson();

            var followedOpportunityIds = GetFollowedConnectionOpportunityIds( currentPerson );

            summaries.ForEach( s =>
            {
                s.IsFollowed = followedOpportunityIds.Contains( s.Id ?? 0 );
                s.TranslateIdToIdKey();

                // We might want to resolve merge fields on the Summary in the future, at which point we'll need to:
                //  1. Consider the merge fields being made "available" in the Connection Opportunity Detail block.
                //  2. Consider the merge fields being added in the Connection Opportunity Search block.
                //  3. Load more supporting data from the database.
                // But for now, we'll just strip any HTML to keep things simple.
                s.Summary = s.Summary.StripHtml();
            } );

            return summaries;
        }

        /// <summary>
        /// Gets the set of <see cref="ConnectionOpportunity"/> identifiers that the current person is following.
        /// Returns an empty set if the person is not authenticated or the entity type cannot be resolved.
        /// </summary>
        /// <param name="currentPerson">The currently authenticated person.</param>
        /// <returns>A <see cref="HashSet{T}"/> of followed <see cref="ConnectionOpportunity"/> IDs for O(1) lookup.</returns>
        private HashSet<int> GetFollowedConnectionOpportunityIds( Person currentPerson )
        {
            if ( currentPerson == null )
            {
                return new HashSet<int>();
            }

            var connectionOpportunityEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY.AsGuid() )?.Id;

            if ( !connectionOpportunityEntityTypeId.HasValue )
            {
                return new HashSet<int>();
            }

            return new FollowingService( RockContext )
                .Queryable()
                .Where( f =>
                    f.EntityTypeId == connectionOpportunityEntityTypeId.Value
                    && f.PersonAlias.PersonId == currentPerson.Id )
                .Select( f => f.EntityId )
                .ToHashSet();
        }

        /// <summary>
        /// Loads the counts of <see cref="ConnectionRequest"/>s per day for the past 28 days.
        /// </summary>
        /// <param name="connectionType">
        /// The <see cref="ConnectionTypeCache"/> for which to load request counts.
        /// </param>
        /// <returns>Counts of <see cref="ConnectionRequest"/>s per day.</returns>
        private ConnectionRequestCountsPerDayBag LoadRequestsCountsPerDay( ConnectionTypeCache connectionType )
        {
            var connectionTypeId = connectionType.Id;
            var campusId = RequestContext.GetContextEntity<Campus>()?.Id;

            var startDate = RockDateTime.Today.AddDays( -27 ); // 28 days including today.
            var endDate = RockDateTime.Today.AddDays( 1 );

            var connectionRequestQry = new ConnectionRequestService( RockContext )
                .Queryable()
                .Where( cr =>
                    ( !campusId.HasValue || cr.CampusId == campusId.Value )
                    && cr.ConnectionOpportunity.ConnectionTypeId == connectionTypeId
                    && cr.ConnectionOpportunity.ConnectionType.IsActive
                    && cr.ConnectionOpportunity.IsActive
                );

            // Exclude any opportunities that the current person is not authorized to view.
            var unauthorizedOpportunityIds = GetUnauthorizedOpportunityIds( connectionType );

            if ( unauthorizedOpportunityIds.Any() )
            {
                connectionRequestQry = connectionRequestQry
                    .Where( cr => !unauthorizedOpportunityIds.Contains( cr.ConnectionOpportunityId ) );
            }

            if ( OpportunityVisibilityPreference == OpportunityVisibility.MyOpportunitiesValue )
            {
                var personId = GetCurrentPerson()?.Id ?? 0;
                connectionRequestQry = connectionRequestQry
                    .Where( cr =>
                        cr.ConnectorPersonAliasId.HasValue
                        && cr.ConnectorPersonAlias.PersonId == personId
                    );
            }

            var createdRequestCountsQry = connectionRequestQry
                .Where( cr =>
                    cr.CreatedDateTime >= startDate
                    && cr.CreatedDateTime < endDate
                )
                .GroupBy( cr => DbFunctions.TruncateTime( cr.CreatedDateTime ) )
                .Select( g => new
                {
                    Date = g.Key.Value,
                    NewCount = g.Count(),
                    ConnectedCount = 0
                } );

            var connectedRequestCountsQry = connectionRequestQry
                .Where( cr =>
                    cr.ConnectedDateTime >= startDate
                    && cr.ConnectedDateTime < endDate
                )
                .GroupBy( cr => DbFunctions.TruncateTime( cr.ConnectedDateTime ) )
                .Select( g => new
                {
                    Date = g.Key.Value,
                    NewCount = 0,
                    ConnectedCount = g.Count()
                } );

            var requestCountsByDate = createdRequestCountsQry
                .Concat( connectedRequestCountsQry ) // Union all.
                .GroupBy( x => x.Date )
                .Select( g => new
                {
                    Date = g.Key,
                    NewCount = g.Sum( x => x.NewCount ),
                    ConnectedCount = g.Sum( x => x.ConnectedCount )
                } )
                .ToList()
                .ToDictionary(
                    c => c.Date.Date,
                    c => new { c.NewCount, c.ConnectedCount }
                );

            var requestCountsPerDay = new ConnectionRequestCountsPerDayBag
            {
                StartDate = startDate,
                NewRequestCounts = new List<int>(),
                CompletedRequestCounts = new List<int>()
            };

            var currentDate = startDate;

            while ( currentDate < endDate )
            {
                var newCount = 0;
                var completedCount = 0;

                if ( requestCountsByDate.TryGetValue( currentDate.Date, out var counts ) )
                {
                    newCount = counts.NewCount;
                    completedCount = counts.ConnectedCount;
                }

                requestCountsPerDay.NewRequestCounts.Add( newCount );
                requestCountsPerDay.CompletedRequestCounts.Add( completedCount );

                currentDate = currentDate.AddDays( 1 );
            }

            return requestCountsPerDay;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var connectionTypeKey = RequestContext.GetPageParameter( PageParameterKey.ConnectionType );

            // The list-view and board-view URLs resolve to the same Connections Hub
            // page; they differ only in the SelectedView query parameter that tells
            // the hub which view to open with.
            var typeListViewQueryParams = new Dictionary<string, string>
            {
                [PageParameterKey.ConnectionType] = connectionTypeKey,
                ["SelectedView"] = EnabledViewFlags.List.ToString().ToLower()
            };

            var opportunityQueryParams = new Dictionary<string, string>
            {
                { PageParameterKey.ConnectionType, connectionTypeKey },
                { PageParameterKey.ConnectionOpportunity, "((Key))" }
            };

            var opportunityListViewQueryParams = new Dictionary<string, string>( opportunityQueryParams )
            {
                { "SelectedView", EnabledViewFlags.List.ToString().ToLower() }
            };

            var opportunityBoardViewQueryParams = new Dictionary<string, string>( opportunityQueryParams )
            {
                { "SelectedView", EnabledViewFlags.Board.ToString().ToLower() }
            };

            var opportunityGridViewQueryParams = new Dictionary<string, string>( opportunityQueryParams )
            {
                { "SelectedView", EnabledViewFlags.Grid.ToString().ToLower() }
            };

            return new Dictionary<string, string>
            {
                // Connection Type-level URLs.
                [NavigationUrlKey.TypeConnectionsHubListViewPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionsHubPage, typeListViewQueryParams ),
                [NavigationUrlKey.TypeOperationalSnapshotPage] = this.GetLinkedPageUrl( AttributeKey.OperationalSnapshotPage, PageParameterKey.ConnectionType, connectionTypeKey ),
                [NavigationUrlKey.TypeCelebrationsReportPage] = this.GetLinkedPageUrl( AttributeKey.CelebrationsReportPage, PageParameterKey.CelebrationsReportConnectionTypeId, connectionTypeKey ),

                // Connection Opportunity-level URLs.
                [NavigationUrlKey.OpportunityConnectionsHubListViewPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionsHubPage, opportunityListViewQueryParams ),
                [NavigationUrlKey.OpportunityConnectionsHubBoardViewPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionsHubPage, opportunityBoardViewQueryParams ),
                [NavigationUrlKey.OpportunityConnectionsHubGridViewPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionsHubPage, opportunityGridViewQueryParams ),

                // My Connections-level URLs.
                [NavigationUrlKey.MyConnectionsPage] = this.GetLinkedPageUrl(
                    AttributeKey.MyConnectionsPage,
                    new Dictionary<string, string>
                    {
                        [PageParameterKey.IsMyConnectionsView] = "true",
                        [PageParameterKey.Connector] = GetCurrentPerson()?.IdKey ?? string.Empty,
                        [PageParameterKey.ConnectionType] = connectionTypeKey
                    }
                )
            };
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent available connection opportunity visibility options.
        /// </summary>
        private class OpportunityVisibility
        {
            public const string MyOpportunitiesValue = "my-opportunities";
            public const string AllOpportunitiesValue = "all-opportunities";

            private static readonly ListItemBag _myOpportunities = new ListItemBag { Text = "My Opportunities", Value = MyOpportunitiesValue };
            public static ListItemBag MyOpportunities => _myOpportunities;

            private static readonly ListItemBag _allOpportunities = new ListItemBag { Text = "All Opportunities", Value = AllOpportunitiesValue };
            public static ListItemBag AllOpportunites => _allOpportunities;
        }

        #endregion Supporting Classes
    }
}

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
using Rock.Model;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.ConnectionOperationalSnapshot;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.UI;

namespace Rock.Blocks.Engagement
{
    [DisplayName( "Connection Operational Snapshot" )]
    [Category( "Engagement" )]
    [Description( "Displays analytics and operational metrics for Connection Requests and Connectors." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [LinkedPage( "Connections Hub Page",
        Key = AttributeKey.ConnectionsHubPage,
        DefaultValue = SystemGuid.Page.CONNECTIONS_LIST,
        Description = "The page to navigate to if a Connectors grid row is clicked.",
        IsRequired = true,
        Order = 0 )]

    #endregion Block Attributes

    [SystemGuid.EntityTypeGuid( "92236EAD-C18C-4484-9685-6792B51FB7F7" )]
    [SystemGuid.BlockTypeGuid( "B5FAF2A4-8195-4972-AA09-F65615939EA8" )]

    public class ConnectionOperationalSnapshot : RockBlockType
    {   
        #region Keys

        private static class AttributeKey
        {
            public const string ConnectionsHubPage = "ConnectionsHubPage";
        }

        private static class PageParameterKey
        {
            public const string ConnectionType = "ConnectionType";
        }

        private static class PreferenceKey
        {
            public const string ConnectionOpportunityFilterTemplate = "ConnectionOpportunityFilter_ForConnectionTypeIdKey_{0}";
            public const string SelectedDateRangeFilter = "SelectedDateRangeFilter";
        }

        private static class NavigationUrlKey
        {
            public const string ConnectionsHub = "ConnectionsHub";
        }

        #endregion Keys

        #region Properties

        private ConnectionType _connectionType = null;
        /// <summary>
        /// Gets the connection type based on the current page parameter.
        /// <para>This entity is used in multiple places in a request, especially on load,
        /// so it is cached in a private variable to avoid multiple queries.</para>
        /// </summary>
        private ConnectionType ConnectionType
        {
            get
            {
                _connectionType ??= ConnectionTypeQuery.FirstOrDefault();

                return _connectionType;
            }
        }

        private IQueryable<ConnectionType> _connectionTypeQuery = null;
        /// <summary>
        /// Gets a queryable that returns the connection type based on the current page parameter.
        /// <para>This is used in cases where we want to further filter or shape the connection type query before executing it,
        /// such as when getting the list of filtered connection opportunities.</para>
        /// </summary>
        private IQueryable<ConnectionType> ConnectionTypeQuery
        {
            get
            {
                if ( _connectionTypeQuery is null )
                {
                    var connectionTypeService = new ConnectionTypeService( RockContext );
                    _connectionTypeQuery = connectionTypeService.GetQueryableByKey( PageParameter( PageParameterKey.ConnectionType ), !RequestContext.Page.Layout.Site.DisablePredictableIds );
                }

                return _connectionTypeQuery;
            }
        }

        #endregion

        public override object GetObsidianBlockInitialization()
        {
            var builder = GetGridBuilder();

            var box = new ListBlockBox<OptionsBag>
            {
                Options = GetOptions(),
                GridDefinition = builder.BuildDefinition(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            // Validate the box and return any errors to the client
            // so they can be displayed in the UI and addressed.
            if ( !IsBoxValid( box, out var errorMessage ) )
            {
                // Return an empty box with just the error message if the box is not valid
                // to avoid leaking invalid data that may be displayed incorrectly.
                return new ListBlockBox<OptionsBag>
                {
                    ErrorMessage = errorMessage
                };
            }

            return box;
        }

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetRowData( GetMetricsBag bag )
        {
            var builder = GetGridBuilder();
            var gridDataBag = builder.Build( GetConnectors( bag ) );

            return ActionOk( gridDataBag );
        }

        [BlockAction]
        public BlockActionResult GetMetrics( GetMetricsBag bag )
        {
            var metrics = new MetricsBag
            {
                CompletionMetrics = GetCompletionMetrics( bag ),
                RequestState = GetRequestState( bag ),
                RequestTimeline = GetRequestTimeline( bag )
            };

            return ActionOk( metrics );
        }

        [BlockAction( "GetCompletionMetrics" )]
        public BlockActionResult GetCompletionMetricsBlockAction()
        {
            var completionMetrics = GetCompletionMetrics();
            return ActionOk( completionMetrics );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Determines whether the specified box is valid and provides an error message if it is not.
        /// </summary>
        /// <param name="box">The box to validate. Must not be null.</param>
        /// <param name="errorMessage">When this method returns, contains an error message describing why the box is invalid; otherwise, null if
        /// the box is valid.</param>
        /// <returns>true if the box is valid; otherwise, false.</returns>
        private bool IsBoxValid( ListBlockBox<OptionsBag> box, out string errorMessage )
        {
            errorMessage = null;

            if ( box.Options.ConnectionTypeIdKey.IsNullOrWhiteSpace() )
            {
                errorMessage = "The Connection Type page parameter is missing.";
                return false;
            }

            if ( !box.NavigationUrls.TryGetValue( NavigationUrlKey.ConnectionsHub, out var connectionTypeUrl )
                 || connectionTypeUrl.IsNullOrWhiteSpace() )
            {
                if ( this.BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
                {
                    errorMessage = "The block is not configured correctly. Please select a Connections Hub Page in the block settings.";
                }
                else
                {
                    errorMessage = "The block is not configured correctly. Please contact an administrator.";
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                {
                    // Setup the partial navigation URL that is used when
                    // a connector is clicked in the Connectors grid.
                    // The ConnectionType from this current page will be
                    // passed, and the Connector's ID Key "((Key))" will be
                    // replaced with the idKey of the selected grid row.
                    NavigationUrlKey.ConnectionsHub,
                    this.GetLinkedPageUrl(
                        AttributeKey.ConnectionsHubPage,
                        new Dictionary<string, string>
                        {
                            { "ConnectionType", ConnectionType?.IdKey },
                            { "Connector", "((Key))" },
                            { "ConnectionOpportunity", "((ConnectionOpportunityKey))" }
                        }
                    )
                }
            };
        }

        /// <summary>
        /// Creates and returns a new instance of the options bag containing the current configuration values.
        /// </summary>
        /// <returns>An <see cref="OptionsBag"/> object populated with the current completion metrics, filters, request state,
        /// and request timeline.</returns>
        private OptionsBag GetOptions()
        {
            return new OptionsBag
            {
                ConnectionTypeIdKey = ConnectionType?.IdKey,
                ConnectionTypeName = ConnectionType?.Name,
                ConnectionOpportunities = GetConnectionOpportunities(),
                CompletionMetrics = GetCompletionMetrics(),
                Filters = GetFilters(),
                RequestState = GetRequestState(),
                RequestTimeline = GetRequestTimeline()
            };
        }

        private List<IdKeyListItemBag> GetConnectionOpportunities()
        {
            return ConnectionTypeQuery
                .SelectMany( ct => ct.ConnectionOpportunities )
                .Where( co => co.IsActive )
                .OrderBy( co => co.Order )
                .ThenBy( co => co.PublicName )
                .ToList()
                .Select( co => new IdKeyListItemBag
                {
                    // Use IdKeys instead of guid so they can be used as page parameters when navigating to the Connections Hub
                    // instead of having to IdKeys by Guid later.
                    Value = co.Guid.ToString(),
                    Text = co.PublicName,
                    IdKey = co.IdKey
                } )
                .ToList();
        }

        private CompletionMetricsBag GetCompletionMetrics( GetMetricsBag bag = null )
        {
            var blockPersonPreferences = GetBlockPersonPreferences();
            var lastNDays = blockPersonPreferences.GetValue( PreferenceKey.SelectedDateRangeFilter ).AsIntegerOrNull() ?? 7;
            var connectionOpportunityGuid = bag?.ConnectionOpportunityGuid;

            var connectionTypeService = new ConnectionTypeService( RockContext );
            var completionMetricsComparison = connectionTypeService
                .GetConnectionRequestCompletionMetricsComparison(
                    ConnectionTypeQuery,
                    RockDateTime.Today.AddDays( -lastNDays ),
                    RockDateTime.Today,
                    new ConnectionRequestCompletionMetricsQueryOptions
                    {
                        CampusGuid = RequestContext.GetContextEntity<Campus>()?.Guid,
                        ConnectionOpportunityGuid = connectionOpportunityGuid
                    } )
                .Select( c => new CompletionMetricsBag
                {
                    AverageCompletionDays = c.Current.AverageCompletionDays,
                    AverageCompletionDaysDelta = c.AverageCompletionDaysDelta,

                    RequestsCompletedCount = c.Current.RequestsCompletedCount,
                    RequestsCompletedCountDelta = c.RequestsCompletedCountDelta,

                    AverageResponsivenessDays = c.Current.AverageResponsivenessDays,
                    AverageResponsivenessDaysDelta = c.AverageResponsivenessDaysDelta,

                    TimelinessPercent = c.Current.TimelinessPercent,
                    TimelinessPercentDelta = c.TimelinessPercentDelta
                } )
                .FirstOrDefault();

            return completionMetricsComparison;
        }

        private FiltersBag GetFilters()
        {
            var blockPersonPreferences = GetBlockPersonPreferences();
            var lastNDays = blockPersonPreferences.GetValue( PreferenceKey.SelectedDateRangeFilter ).AsIntegerOrNull() ?? 7;

            return new FiltersBag
            {
                DateRanges = new List<ListItemBag>
                    {
                        new ListItemBag { Value = "7", Text = "Last 7 Days" },
                        new ListItemBag { Value = "28", Text = "Last 28 Days" },
                    },
                DefaultDateRangeValue = lastNDays.ToString()
            };
        }

        private RequestStateBag GetRequestState( GetMetricsBag bag = null )
        {
            var connectionTypeService = new ConnectionTypeService( RockContext );
            var connectionStatusService = new ConnectionStatusService( RockContext );
            var connectionOpportunityGuid = bag?.ConnectionOpportunityGuid;

            var connectionRequestHealthSnapshot = connectionTypeService
                .GetConnectionRequestHealthSnapshot(
                    ConnectionTypeQuery,
                    new ConnectionRequestHealthSnapshotQueryOptions
                    {
                        CampusGuid = RequestContext.GetContextEntity<Campus>()?.Guid,
                        ConnectionOpportunityGuid = connectionOpportunityGuid
                    }
                )
                .FirstOrDefault();

            var connectionRequestStatusDistributions = connectionTypeService
                .GetConnectionRequestStatusDistributions(
                    ConnectionTypeQuery,
                    new ConnectionRequestStatusDistributionQueryOptions
                    {
                        CampusGuid = RequestContext.GetContextEntity<Campus>()?.Guid,
                        ConnectionOpportunityGuid = connectionOpportunityGuid
                    }
                )
                .Select( sd => new RequestStatusCountBag
                {
                    Status = sd.Status,
                    Color = sd.Color,
                    Count = sd.Count
                } )
                .ToList();

            return new RequestStateBag
            {
                ByStatus = connectionRequestStatusDistributions,

                TotalActive = connectionRequestHealthSnapshot?.ActiveCount ?? 0,
                TotalUnassigned = connectionRequestHealthSnapshot?.UnassignedCount ?? 0,
                TotalDueSoon = connectionRequestHealthSnapshot?.DueSoonCount ?? 0,
                TotalOverdue = connectionRequestHealthSnapshot?.OverdueCount ?? 0,
                TotalOnTrack = connectionRequestHealthSnapshot?.OnTrackCount ?? 0
            };
        }

        private RequestTimelineBag GetRequestTimeline( GetMetricsBag bag = null )
        {
            var connectionOpportunityGuid = bag?.ConnectionOpportunityGuid;

            var connectionTypeService = new ConnectionTypeService( RockContext );
            var upcomingFollowUps = connectionTypeService
                .GetConnectionRequestUpcomingFollowUpWindows(
                    ConnectionTypeQuery,
                    new ConnectionRequestUpcomingFollowUpWindowQueryOptions
                    {
                        CampusGuid = RequestContext.GetContextEntity<Campus>()?.Guid,
                        ConnectionOpportunityGuid = connectionOpportunityGuid
                    }
                )
                .Select( w => new UpcomingFollowUpBag
                {
                    DaysAhead = w.EndOffsetDays,
                    Count = w.Count
                } )
                .ToList();

            return new RequestTimelineBag
            {
                 UpcomingFollowUps = upcomingFollowUps
            };
        }

        private GridBuilder<ConnectorBag> GetGridBuilder()
        {
            return new GridBuilder<ConnectorBag>()
                .WithBlock( this )
                .AddField( nameof( ConnectorBag.IdKey ).ToCamelCase(), row => row.IdKey )

                // Don't use AddPersonField here because we want to hijack the connection status field to show campus name instead.
                .AddField( nameof( ConnectorBag.Person ).ToCamelCase(), row => row.Person )

                .AddField( nameof( ConnectorBag.ActiveRequestCount ).ToCamelCase(), row => row.ActiveRequestCount )
                .AddField( nameof( ConnectorBag.OverdueRequestCount ).ToCamelCase(), row => row.OverdueRequestCount )

                // These 2 are not actually grid columns.
                // They will be displayed as a custom column that will display metrics stacked vertically in a single row.
                .AddField( nameof( ConnectorBag.CompletedRequestCount ).ToCamelCase(), row => row.CompletedRequestCount )
                .AddField( nameof( ConnectorBag.AverageCompletionDays ).ToCamelCase(), row => row.AverageCompletionDays );
        }

        private List<ConnectorBag> GetConnectors( GetMetricsBag bag = null )
        {
            var today = RockDateTime.Today;
            var twentyEightDaysAgo = today.AddDays( -28 );
            var campusGuid = RequestContext.GetContextEntity<Campus>()?.Guid;
            var connectionOpportunityGuid = bag?.ConnectionOpportunityGuid;

            // 1. Aggregate metrics per connector in SQL
            var metrics = ConnectionTypeQuery
                .SelectMany( ct => ct.ConnectionOpportunities
                    .Where( co => !connectionOpportunityGuid.HasValue || co.Guid == connectionOpportunityGuid.Value )
                    .SelectMany( co => co.ConnectionRequests )
                )
                .Where( cr =>
                    cr.ConnectionState == ConnectionState.Active
                    || cr.ConnectionState == ConnectionState.Connected
                    || cr.ConnectionState == ConnectionState.FutureFollowUp )
                .Where( cr => cr.ConnectorPersonAliasId.HasValue )
                .Where( cr => !campusGuid.HasValue
                    || cr.Campus.Guid == campusGuid.Value )
                .GroupBy( cr => cr.ConnectorPersonAlias.PersonId )
                .Select( crGrouping => new
                {
                    PersonId = crGrouping.Key,

                    // The total number of Connection Requests for this connector
                    // that are currently in the active state.
                    ActiveRequestCount = crGrouping.Count( cr =>
                        cr.ConnectionState == ConnectionState.Active ),

                    // The total number of Connection Requests for this connector
                    // that are overdue and still need to be connected.
                    OverdueRequestCount = crGrouping.Count( cr =>
                        cr.ConnectionState != ConnectionState.Connected
                        && cr.ConnectionState != ConnectionState.Inactive
                        && cr.DueDate.HasValue
                        && DbFunctions.TruncateTime( cr.DueDate.Value ) < today ),

                    // The total number of Connection Requests for this connector
                    // that have been connected in the last 28 days.
                    CompletedRequestCount = crGrouping.Count( cr =>
                        cr.ConnectionState == ConnectionState.Connected
                        && cr.ConnectedDateTime >= twentyEightDaysAgo ),

                    // The average number of days it takes this connector
                    // to connect its Connection Requests created in the last 28 days.
                    AverageCompletionDays = crGrouping
                        .Where( cr =>
                            cr.ConnectionState == ConnectionState.Connected
                            && cr.ConnectedDateTime.HasValue
                            && cr.CreatedDateTime.HasValue
                            && cr.CreatedDateTime.Value >= twentyEightDaysAgo )
                        .Average( cr => ( decimal? ) DbFunctions.DiffDays(
                            cr.CreatedDateTime.Value,
                            cr.ConnectedDateTime.Value ) )
                } )
                .ToList();

            if ( !metrics.Any() )
            {
                return new List<ConnectorBag>();
            }

            // 2. Load all people in one query
            var personIds = metrics.Select( m => m.PersonId ).Distinct().ToList();

            var people = new PersonService( RockContext )
                .Queryable()
                .Include( p => p.PrimaryCampus ) // This is needed to get the primary campus of the connector person to display under their name.
                .Where( p => personIds.Contains( p.Id ) )
                .AsNoTracking()
                .ToDictionary( p => p.Id );

            // 3. Compose ConnectorBags in memory
            var connectors = metrics
                .Where( m => people.ContainsKey( m.PersonId ) )
                .Select( m =>
                {
                    var person = people[m.PersonId];

                    return new ConnectorBag
                    {
                        IdKey = person.IdKey,

                        Person = new PersonFieldBag
                        {
                            IdKey = person.IdKey,
                            // Hijack the connection status text to display campus name
                            ConnectionStatus = person.GetCampus()?.Name,
                            LastName = person.LastName,
                            NickName = person.NickName,
                            PhotoUrl = person.PhotoUrl
                        },
                        ActiveRequestCount = m.ActiveRequestCount,
                        OverdueRequestCount = m.OverdueRequestCount,
                        CompletedRequestCount = m.CompletedRequestCount,
                        AverageCompletionDays = m.AverageCompletionDays ?? 0m
                    };
                } )
                .OrderByDescending( c => c.OverdueRequestCount )
                .ThenBy( c => c.Person.NickName )
                .ThenBy( c => c.Person.LastName )
                .ToList();

            return connectors;
        }

        #endregion Private Methods
    }
}

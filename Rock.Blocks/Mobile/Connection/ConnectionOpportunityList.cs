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
using Rock.Common.Mobile.Blocks.Connection.ConnectionOpportunityList;
using Rock.Common.Mobile.ViewModel;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

using ConnectionOpportunityVisibility = Rock.Common.Mobile.Enums.ConnectionOpportunityVisibility;

namespace Rock.Blocks.Mobile.Connection
{
    /// <summary>
    /// Displays the opportunities of a single connection type with
    /// per-opportunity request count summaries for the requested visibility
    /// and campus filter, plus the type-level count-by-status distribution
    /// and 7-day completion metrics for the Details segment.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Opportunity List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the opportunities of a connection type with request count summaries and type metrics." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "Page to link to when the individual taps an opportunity. The connection opportunity IdKey is passed as the ConnectionOpportunity page parameter.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [LinkedPage( "Add Connection Request Page",
        Description = "Page that hosts the Add Connection Request block, opened by the floating Add button. No page parameters are passed, so the Add block starts at its Type step. When empty, the floating button is not shown.",
        IsRequired = false,
        Key = AttributeKey.AddPage,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "8DD07282-8470-426C-8F89-7390599DB37F" )]
    [Rock.SystemGuid.BlockTypeGuid( "039AB104-FDFE-4BB0-944A-2C02F4C1D73A" )]
    public class ConnectionOpportunityList : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string AddPage = "AddPage";
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

            return new Rock.Common.Mobile.Blocks.Connection.ConnectionOpportunityList.Configuration
            {
                Campuses = campuses,
                DetailPageGuid = GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull(),
                AddPageGuid = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the connection type header and the connection opportunity
        /// summaries for the requested visibility and campus filter.
        /// </summary>
        /// <param name="options">The options that describe which connection type to load and which requests to count.</param>
        /// <returns>The connection type header and the opportunity summaries.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionOpportunitySummaries( GetConnectionOpportunitySummariesRequestBag options )
        {
            if ( options == null )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionType = GetConnectionType( options.ConnectionTypeIdKey );

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

            // The Add button opens the wizard at its Type step and carries no
            // page parameters, so the gate asks whether the person can add
            // anywhere rather than scoping to the type being listed here.
            // Short-circuited on the block setting so an unconfigured block does
            // not pay for it.
            var addPageConfigured = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull().HasValue;

            return ActionOk( new GetConnectionOpportunitySummariesResponseBag
            {
                ConnectionType = new ConnectionTypeHeaderBag
                {
                    Name = connectionType.Name,
                    IconCssClass = connectionType.IconCssClass,
                    Description = connectionType.Description
                },
                Opportunities = LoadConnectionOpportunitySummaries( RockContext, connectionType, options ),
                IsAddEnabled = addPageConfigured
                    && ConnectionRequestAuthorization.CanAddRequestAnywhere( RockContext, GetCurrentPerson() )
            } );
        }

        /// <summary>
        /// Gets the connection type's count-by-status distribution and the
        /// completion metrics for the last 7 days compared to the prior 7
        /// days, shown on the Details segment.
        /// </summary>
        /// <param name="options">The options that describe which connection type to load.</param>
        /// <returns>The count-by-status distribution and the completion metrics.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionTypeMetrics( GetConnectionTypeMetricsRequestBag options )
        {
            if ( options == null )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var connectionType = GetConnectionType( options.ConnectionTypeIdKey );

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

            var connectionTypeService = new ConnectionTypeService( RockContext );
            var connectionTypeQuery = connectionTypeService
                .Queryable()
                .Where( ct => ct.Id == connectionType.Id );

            // The Details metrics reuse the same internal ConnectionTypeService
            // methods the web Connection Operational Snapshot block calls, so
            // no query logic is duplicated here. Campus scoping mirrors the
            // web block, which passes the context campus into both queries;
            // opportunity scoping is not applied (the mobile Details sheet is
            // type-wide).
            var countByStatus = connectionTypeService
                .GetConnectionRequestStatusDistributions(
                    connectionTypeQuery,
                    new ConnectionRequestStatusDistributionQueryOptions
                    {
                        CampusGuid = options.CampusGuid
                    } )
                .Select( sd => new ConnectionRequestStatusCountBag
                {
                    Status = sd.Status,
                    Color = sd.Color,
                    Count = sd.Count
                } )
                .ToList();

            // The comparison covers the last 7 days against the immediately
            // preceding 7 days; the service derives the previous window
            // itself. TimelinessPercent and its delta are 0-1 ratios.
            var completionMetrics = connectionTypeService
                .GetConnectionRequestCompletionMetricsComparison(
                    connectionTypeQuery,
                    RockDateTime.Today.AddDays( -7 ),
                    RockDateTime.Today,
                    new ConnectionRequestCompletionMetricsQueryOptions
                    {
                        CampusGuid = options.CampusGuid
                    } )
                .Select( c => new ConnectionCompletionMetricsBag
                {
                    TimelinessPercent = c.Current.TimelinessPercent,
                    TimelinessPercentDelta = c.TimelinessPercentDelta,
                    AverageResponsivenessDays = c.Current.AverageResponsivenessDays,
                    AverageResponsivenessDaysDelta = c.AverageResponsivenessDaysDelta,
                    RequestsCompletedCount = c.Current.RequestsCompletedCount,
                    RequestsCompletedCountDelta = c.RequestsCompletedCountDelta,
                    AverageCompletionDays = c.Current.AverageCompletionDays,
                    AverageCompletionDaysDelta = c.AverageCompletionDaysDelta
                } )
                .FirstOrDefault() ?? new ConnectionCompletionMetricsBag(); // Zeroed when no requests were modified in the period.

            return ActionOk( new GetConnectionTypeMetricsResponseBag
            {
                CountByStatus = countByStatus,
                CompletionMetrics = completionMetrics
            } );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the <see cref="ConnectionTypeCache"/> for the IdKey carried
        /// on the request bag.
        /// </summary>
        /// <param name="connectionTypeIdKey">The IdKey of the connection type.</param>
        /// <returns>The <see cref="ConnectionTypeCache"/>, or <c>null</c> when not found.</returns>
        private ConnectionTypeCache GetConnectionType( string connectionTypeIdKey )
        {
            return connectionTypeIdKey.IsNotNullOrWhiteSpace()
                ? ConnectionTypeCache.Get( connectionTypeIdKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;
        }

        /// <summary>
        /// Gets whether the current person is authorized to view [or edit]
        /// the <see cref="ConnectionTypeCache"/>. This is the type-level gate;
        /// opportunities within an authorized type are additionally
        /// security-filtered in <see cref="LoadConnectionOpportunitySummaries"/>.
        /// </summary>
        /// <param name="connectionType">The <see cref="ConnectionTypeCache"/> to check.</param>
        /// <returns>Whether the current person is authorized to view [or edit] the connection type.</returns>
        private bool GetIsAuthorizedToView( ConnectionTypeCache connectionType )
        {
            var currentPerson = GetCurrentPerson();

            return connectionType.IsAuthorized( Authorization.VIEW, currentPerson )
                || connectionType.IsAuthorized( Authorization.EDIT, currentPerson );
        }

        /// <summary>
        /// Gets the Ids of the active <see cref="ConnectionOpportunity"/>s under
        /// the specified type that the current person is authorized to view. This
        /// mirrors the web hub (<c>ConnectionsHub.GetViewAuthorizedConnectionOpportunityIds</c>)
        /// and the legacy Connection Request Board: an opportunity is viewable when
        /// the person has native VIEW authorization on it, or (only when Request
        /// Security is enabled on the type) when the person is the assigned connector
        /// on at least one request in that opportunity. <c>IsAuthorized</c> cannot be
        /// translated to SQL, so the opportunities are materialized for the check.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="connectionType">The connection type whose opportunities are evaluated.</param>
        /// <returns>The set of opportunity Ids the current person is authorized to view.</returns>
        private HashSet<int> GetViewAuthorizedConnectionOpportunityIds( RockContext rockContext, ConnectionTypeCache connectionType )
        {
            var currentPerson = GetCurrentPerson();
            var personId = currentPerson?.Id ?? 0;

            // When Request Security is enabled, a person may also view any
            // opportunity where they are the assigned connector on a request.
            // Query these once up front so the cost is independent of request volume.
            var selfAssignedOpportunityIds = new HashSet<int>();
            if ( connectionType.EnableRequestSecurity )
            {
                selfAssignedOpportunityIds = new HashSet<int>( new ConnectionRequestService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( r => r.ConnectorPersonAlias.PersonId == personId
                        && r.ConnectionOpportunity.ConnectionTypeId == connectionType.Id )
                    .Select( r => r.ConnectionOpportunityId )
                    .Distinct() );
            }

            var opportunities = new ConnectionOpportunityService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( co => co.ConnectionTypeId == connectionType.Id && co.IsActive )
                .ToList();

            var authorizedOpportunityIds = new HashSet<int>();
            foreach ( var opportunity in opportunities )
            {
                // Check native VIEW authorization directly (matching the board),
                // falling back to the self-assigned connector check only when
                // Request Security is enabled.
                var canView = opportunity.IsAuthorized( Authorization.VIEW, currentPerson )
                    || ( connectionType.EnableRequestSecurity && selfAssignedOpportunityIds.Contains( opportunity.Id ) );

                if ( canView )
                {
                    authorizedOpportunityIds.Add( opportunity.Id );
                }
            }

            return authorizedOpportunityIds;
        }

        /// <summary>
        /// Loads <see cref="ConnectionOpportunity"/> data from the database
        /// and uses this data to build a list of
        /// <see cref="ConnectionOpportunitySummaryBag"/>s with the
        /// per-opportunity request counts.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="connectionType">The connection type whose opportunities to load.</param>
        /// <param name="options">The options that describe which requests to count.</param>
        /// <returns>A list of <see cref="ConnectionOpportunitySummaryBag"/>s ordered by Order then Name.</returns>
        private List<ConnectionOpportunitySummaryBag> LoadConnectionOpportunitySummaries( RockContext rockContext, ConnectionTypeCache connectionType, GetConnectionOpportunitySummariesRequestBag options )
        {
            /*
                6/12/2026 - CLAUDE

                This count query is an intentional duplicate of the web block
                ConnectionOpportunityNavigation.LoadConnectionOpportunitySummaries
                (Rock.Blocks/Connection). The logic is block-private on the web,
                so it is copied here rather than refactored into a shared
                service, per the mobile Connections port spec
                (specs/260608-mobile-connection-type-detail.md). Campus and
                visibility come from the mobile request bag instead of the web
                block's context entity and person preference, and the follow
                lookup is dropped (follow is deferred on mobile). Keep the two
                in sync when the counting rules change.

                Reason: Mobile parity with the web Connection Opportunity Navigation counts.
            */

            var connectionTypeId = connectionType.Id;
            var personId = GetCurrentPerson()?.Id ?? 0;
            var today = RockDateTime.Today;
            var limitToMyOpportunities = options.Visibility == ConnectionOpportunityVisibility.MyOpportunities;

            // Resolve the campus filter to an identifier so the query does not
            // compare Guids.
            int? campusId = options.CampusGuid.HasValue
                ? CampusCache.Get( options.CampusGuid.Value )?.Id
                : null;

            // Active requests in this (active) type's (active) opportunities,
            // optionally scoped to a campus.
            var connectionRequestQry = new ConnectionRequestService( rockContext )
                .Queryable()
                .Where( cr =>
                    cr.ConnectionState == ConnectionState.Active
                    && ( !campusId.HasValue || cr.CampusId == campusId.Value )
                    && cr.ConnectionOpportunity.ConnectionTypeId == connectionTypeId
                    && cr.ConnectionOpportunity.ConnectionType.IsActive
                    && cr.ConnectionOpportunity.IsActive
                );

            // Security-filter the opportunity set to those the current person is
            // authorized to view, matching the web Connection Request Board and
            // hub. This honors native VIEW authorization and, when the type has
            // Request Security enabled, the self-assigned connector fallback.
            // Unauthorized opportunities are dropped from the GroupJoin below, so
            // their request counts never surface.
            var authorizedOpportunityIds = GetViewAuthorizedConnectionOpportunityIds( rockContext, connectionType );

            // Active opportunities in this (active) type that the person may view.
            var connectionOpportunityQry = new ConnectionOpportunityService( rockContext )
                .Queryable()
                .Where( co =>
                    co.ConnectionTypeId == connectionTypeId
                    && co.ConnectionType.IsActive
                    && co.IsActive
                    && authorizedOpportunityIds.Contains( co.Id )
                );

            if ( limitToMyOpportunities )
            {
                connectionRequestQry = connectionRequestQry
                    .Where( cr =>
                        cr.ConnectorPersonAliasId.HasValue
                        && cr.ConnectorPersonAlias.PersonId == personId
                    );

                // Reduce the opportunity set to those having at least one of
                // my requests.
                connectionOpportunityQry = connectionOpportunityQry
                    .Where( co => connectionRequestQry.Any( cr => cr.ConnectionOpportunityId == co.Id ) );
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

            // GroupJoin so opportunities with zero matching requests still
            // appear (under All Opportunities).
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
                    ( x, counts ) => new
                    {
                        x.ConnectionOpportunity.Id,
                        x.ConnectionOpportunity.IconCssClass,
                        x.ConnectionOpportunity.Name,
                        x.ConnectionOpportunity.Summary,
                        x.ConnectionOpportunity.Order,
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

            return summaries
                .Select( s => new ConnectionOpportunitySummaryBag
                {
                    IdKey = IdHasher.Instance.GetHash( s.Id ),
                    IconCssClass = s.IconCssClass,
                    Name = s.Name,
                    Summary = s.Summary.StripHtml(),
                    Order = s.Order,
                    ActiveRequestCount = s.ActiveRequestCount,
                    DueSoonRequestCount = s.DueSoonRequestCount,
                    OverdueRequestCount = s.OverdueRequestCount,
                    UnassignedRequestCount = s.UnassignedRequestCount,
                    AssignedToYouRequestCount = s.AssignedToYouRequestCount
                } )
                .ToList();
        }

        #endregion
    }
}

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

using Rock;
using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Connection.ConnectionRequestList;
using Rock.Common.Mobile.ViewModel;
using Rock.Constants;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

using MobileConnectionState = Rock.Common.Mobile.Enums.ConnectionState;
using MobileConnectorScope = Rock.Common.Mobile.Enums.ConnectionRequestConnectorScope;
using MobileDueStatus = Rock.Common.Mobile.Enums.DueStatus;
using MobileSortOption = Rock.Common.Mobile.Enums.ConnectionRequestSortOption;

namespace Rock.Blocks.Mobile.Connection
{
    /// <summary>
    /// Displays a paged, server-filtered, server-sorted list of the
    /// connection requests for a single connection opportunity, with a
    /// per-request celebration indicator, due status, and status pill, plus
    /// a floating "Add Connection Request" launcher.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Request List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the connection requests of a single connection opportunity with search, filtering, sorting and infinite-scroll paging." )]
    [IconCssClass( "ti ti-list-details" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "Page to link to when the individual taps a request. The connection request IdKey is passed as the ConnectionRequest page parameter.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [LinkedPage( "Add Connection Request Page",
        Description = "Page that hosts the Add Connection Request block, opened by the floating Add button. The current ConnectionOpportunity IdKey is passed as a page parameter so the Add block prefills and locks the Type and Opportunity. When empty, the floating button is not shown.",
        IsRequired = false,
        Key = AttributeKey.AddPage,
        Order = 1 )]

    [IntegerField( "Page Size",
        Description = "The number of requests fetched per load (the infinite-scroll page size).",
        IsRequired = false,
        DefaultIntegerValue = 15,
        Key = AttributeKey.PageSize,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "CC91A1ED-7FB0-43B3-A8B4-A050DBF6BA6D" )]
    [Rock.SystemGuid.BlockTypeGuid( "117ADAF8-8173-4A88-8C88-2C97F88985DC" )]
    public class ConnectionRequestList : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string AddPage = "AddPage";
            public const string PageSize = "PageSize";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The default number of requests returned per page when the Page
        /// Size setting is not configured.
        /// </summary>
        private const int DefaultPageSize = 15;

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

            return new Rock.Common.Mobile.Blocks.Connection.ConnectionRequestList.Configuration
            {
                Campuses = campuses,
                DetailPageGuid = GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull(),
                AddPageGuid = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull(),
                PageSize = GetAttributeValue( AttributeKey.PageSize ).AsIntegerOrNull() ?? DefaultPageSize
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets one page of the connection requests for a single connection
        /// opportunity, applying the connector scope, campus, state, status
        /// and due filters, the name search and the alphabetical sort, all
        /// server-side. The opportunity header, the available statuses and
        /// the Add-button gate are populated only on the offset-0 response.
        /// </summary>
        /// <param name="request">The filter, search, sort and paging options.</param>
        /// <returns>One page of request summaries plus the offset-0 header data.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionRequests( GetConnectionRequestsRequestBag request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var rockContext = RockContext;
            var currentPerson = GetCurrentPerson();

            var connectionOpportunity = request.ConnectionOpportunityIdKey.IsNotNullOrWhiteSpace()
                ? new ConnectionOpportunityService( rockContext ).Get( request.ConnectionOpportunityIdKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;

            if ( connectionOpportunity == null )
            {
                return ActionBadRequest( $"Unable to find the specified {ConnectionOpportunity.FriendlyTypeName}." );
            }

            var connectionType = ConnectionTypeCache.Get( connectionOpportunity.ConnectionTypeId );

            if ( connectionType == null )
            {
                return ActionBadRequest( $"Unable to find the specified {ConnectionType.FriendlyTypeName}." );
            }

            // Authorization mirrors the web Connections Hub: VIEW is evaluated at
            // the opportunity level, and when the type enables per-request
            // security the assigned connector is additionally allowed in even
            // without opportunity-level VIEW. The per-request filtering that
            // actually hides other people's secured requests happens in
            // GetRequestSummaries.
            var canView = connectionOpportunity.IsAuthorized( Authorization.VIEW, currentPerson );

            if ( !canView && connectionType.EnableRequestSecurity )
            {
                canView = IsCurrentPersonConnectorInOpportunity( rockContext, connectionOpportunity.Id, currentPerson );
            }

            if ( !canView )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( ConnectionRequest.FriendlyTypeName ) );
            }

            if ( !connectionType.IsActive || !connectionOpportunity.IsActive )
            {
                return ActionBadRequest( $"The specified {ConnectionOpportunity.FriendlyTypeName} is not active." );
            }

            var pageSizeSetting = GetAttributeValue( AttributeKey.PageSize ).AsIntegerOrNull() ?? DefaultPageSize;
            var offset = Math.Max( 0, request.Offset );
            var limit = request.Limit > 0 ? request.Limit : pageSizeSetting;

            var requests = GetRequestSummaries( rockContext, connectionOpportunity, connectionType, request, currentPerson, offset, limit );

            var response = new GetConnectionRequestsResponseBag
            {
                Requests = requests
            };

            // The header, the status filter options and the Add gate are only
            // needed once; the shell caches them from the first (offset-0)
            // load.
            if ( offset == 0 )
            {
                response.Opportunity = new ConnectionOpportunityHeaderBag
                {
                    IconCssClass = connectionOpportunity.IconCssClass,
                    Name = connectionOpportunity.Name
                };

                response.AvailableStatuses = connectionOpportunity.ConnectionType.ConnectionStatuses
                    .OrderBy( s => s.Order )
                    .Select( s => new ConnectionStatusItemBag
                    {
                        Name = s.Name,
                        Value = s.Guid,
                        Color = s.HighlightColor
                    } )
                    .ToList();

                var addPageGuid = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull();
                response.IsAddEnabled = addPageGuid.HasValue
                    && connectionType.IsAuthorized( Authorization.EDIT, currentPerson );
            }

            return ActionOk( response );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the current person is the assigned connector on at
        /// least one request in the opportunity. Used only when the type enables
        /// per-request security to let a connector into an opportunity they do
        /// not otherwise have VIEW on, mirroring the web Connections Hub. This is
        /// a single existence query keyed on ConnectorPersonAlias.PersonId, so
        /// the cost does not grow with request volume.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for the query.</param>
        /// <param name="opportunityId">The opportunity being viewed.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns><c>true</c> if the current person is a connector on a request in the opportunity.</returns>
        private static bool IsCurrentPersonConnectorInOpportunity( RockContext rockContext, int opportunityId, Person currentPerson )
        {
            if ( currentPerson == null )
            {
                return false;
            }

            var currentPersonId = currentPerson.Id;

            return new ConnectionRequestService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Any( cr => cr.ConnectionOpportunityId == opportunityId
                    && cr.ConnectorPersonAlias.PersonId == currentPersonId );
        }

        /// <summary>
        /// Builds the paged, filtered, sorted list of request summaries for
        /// the opportunity.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="connectionOpportunity">The opportunity whose requests are loaded.</param>
        /// <param name="connectionType">The opportunity's connection type cache, used for status resolution.</param>
        /// <param name="request">The filter, search, sort and paging options.</param>
        /// <param name="currentPerson">The current person, used for the My Requests scope.</param>
        /// <param name="offset">The number of rows to skip.</param>
        /// <param name="limit">The number of rows to take.</param>
        /// <returns>A list of <see cref="ConnectionRequestSummaryBag"/> for the requested page.</returns>
        private List<ConnectionRequestSummaryBag> GetRequestSummaries( RockContext rockContext, ConnectionOpportunity connectionOpportunity, ConnectionTypeCache connectionType, GetConnectionRequestsRequestBag request, Person currentPerson, int offset, int limit )
        {
            /*
                6/16/2026 - CLAUDE

                This request query is an intentional duplicate of the web
                Connections Hub request grid (Rock.Blocks/Engagement/
                ConnectionsHub.cs). That logic is block-private (raw SQL with
                board/grouping concerns mobile does not need), so it is copied
                here as a LINQ query for this single opportunity rather than
                refactored into a shared service, per the mobile Connections
                port spec (specs/260609-mobile-connection-opportunity-detail.md).
                The Due filter buckets MUST stay aligned with GetDueStatus
                (an Inactive request is always On Track) so the row Due badge
                and the Due filter never disagree.

                Reason: Mobile parity with the web Connections Hub request grid.
            */

            var opportunityId = connectionOpportunity.Id;
            var personId = currentPerson?.Id ?? 0;
            var today = RockDateTime.Today;
            var limitToMyRequests = request.ConnectorScope == MobileConnectorScope.MyRequests;

            // The shell only offers Active / Inactive / FutureFollowUp;
            // Connected is never shown here. Convert the mobile states to the
            // model enum (the integer values line up) and clamp to the offered
            // set; an empty selection means all three offered states.
            var offeredStates = new[] { ConnectionState.Active, ConnectionState.Inactive, ConnectionState.FutureFollowUp };
            var states = ( request.States != null && request.States.Count > 0 )
                ? request.States.Select( s => ( ConnectionState ) ( int ) s ).Where( s => offeredStates.Contains( s ) ).ToList()
                : offeredStates.ToList();

            if ( states.Count == 0 )
            {
                states = offeredStates.ToList();
            }

            // Resolve the campus and status filters to identifiers so the
            // query compares ids rather than Guids.
            int? campusId = request.CampusGuid.HasValue
                ? CampusCache.Get( request.CampusGuid.Value )?.Id
                : null;

            int? statusId = request.StatusGuid.HasValue
                ? connectionOpportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.Guid == request.StatusGuid.Value )
                    .Select( s => ( int? ) s.Id )
                    .FirstOrDefault()
                : null;

            var requestsQry = new ConnectionRequestService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( cr => cr.ConnectionOpportunityId == opportunityId )
                .Where( cr => states.Contains( cr.ConnectionState ) );

            if ( limitToMyRequests )
            {
                requestsQry = requestsQry.Where( cr =>
                    cr.ConnectorPersonAliasId.HasValue
                    && cr.ConnectorPersonAlias.PersonId == personId );
            }

            if ( campusId.HasValue )
            {
                requestsQry = requestsQry.Where( cr => cr.CampusId == campusId.Value );
            }

            if ( statusId.HasValue )
            {
                requestsQry = requestsQry.Where( cr => cr.ConnectionStatusId == statusId.Value );
            }

            // Due filter — these buckets MUST match GetDueStatus below, which
            // treats an Inactive request as always "On Track" regardless of
            // its dates. So Overdue/DueSoon exclude Inactive, and On Track
            // includes it.
            switch ( request.DueStatus )
            {
                case MobileDueStatus.Overdue:
                    requestsQry = requestsQry.Where( cr =>
                        cr.ConnectionState != ConnectionState.Inactive
                        && cr.DueDate.HasValue
                        && DbFunctions.TruncateTime( cr.DueDate.Value ) < today );
                    break;
                case MobileDueStatus.DueSoon:
                    requestsQry = requestsQry.Where( cr =>
                        cr.ConnectionState != ConnectionState.Inactive
                        && cr.DueSoonDate.HasValue
                        && DbFunctions.TruncateTime( cr.DueSoonDate.Value ) <= today
                        && !( cr.DueDate.HasValue && DbFunctions.TruncateTime( cr.DueDate.Value ) < today ) );
                    break;
                case MobileDueStatus.DueLater: // "On Track"
                    requestsQry = requestsQry.Where( cr =>
                        cr.ConnectionState == ConnectionState.Inactive
                        || ( !( cr.DueDate.HasValue && DbFunctions.TruncateTime( cr.DueDate.Value ) < today )
                            && !( cr.DueSoonDate.HasValue && DbFunctions.TruncateTime( cr.DueSoonDate.Value ) <= today ) ) );
                    break;
                    // null => no due filter (All).
            }

            // Server-side name search across nick name, last name and the
            // combined full name so partial and full-name searches both work.
            if ( request.SearchTerm.IsNotNullOrWhiteSpace() )
            {
                var term = request.SearchTerm.Trim();
                requestsQry = requestsQry.Where( cr =>
                    cr.PersonAlias.Person.NickName.Contains( term )
                    || cr.PersonAlias.Person.LastName.Contains( term )
                    || ( cr.PersonAlias.Person.NickName + " " + cr.PersonAlias.Person.LastName ).Contains( term ) );
            }

            // Server-side alphabetical sort by requester (the only sort).
            requestsQry = request.SortOrder == MobileSortOption.NameDescending
                ? requestsQry.OrderByDescending( cr => cr.PersonAlias.Person.LastName )
                             .ThenByDescending( cr => cr.PersonAlias.Person.NickName )
                : requestsQry.OrderBy( cr => cr.PersonAlias.Person.LastName )
                             .ThenBy( cr => cr.PersonAlias.Person.NickName );

            // Project the fields needed for the summary plus the campus and
            // connector identity used by the per-request security filter.
            var projectedQry = requestsQry
                .Select( cr => new
                {
                    cr.Id,
                    cr.ConnectionState,
                    cr.ConnectionStatusId,
                    cr.DueDate,
                    cr.DueSoonDate,
                    cr.Comments,
                    cr.CampusId,
                    ConnectorPersonId = ( int? ) cr.ConnectorPersonAlias.PersonId,
                    Requester = cr.PersonAlias.Person
                } );

            // When per-request security is enabled the assigned connector always
            // sees their own request, but every other request must pass a VIEW
            // check that cannot be expressed in SQL (explicit request-level rules
            // plus opportunity/type inheritance). Materialize the ordered
            // candidates, authorize each in memory, then page — mirroring the web
            // Connections Hub (ConnectionsHub.FilterRowsByViewAuthorization). When
            // security is disabled the caller has already authorized VIEW at the
            // opportunity level, so the database performs the paging.
            var pageRows = connectionType.EnableRequestSecurity
                ? projectedQry
                    .ToList()
                    .Where( r => IsRequestViewable( r.Id, r.CampusId, r.ConnectorPersonId, connectionOpportunity, personId, currentPerson ) )
                    .Skip( offset )
                    .Take( limit )
                    .ToList()
                : projectedQry
                    .Skip( offset )
                    .Take( limit )
                    .ToList();

            // Build a status lookup once from the opportunity's type so each
            // row's status name + color is an in-memory dictionary hit.
            var statusLookup = connectionOpportunity.ConnectionType.ConnectionStatuses
                .ToDictionary( s => s.Id, s => new { s.Name, Color = s.HighlightColor } );

            // Fetch the celebration flags for this page in a single batched
            // query keyed by EntityId (never one query per row).
            var requestIds = pageRows.Select( r => r.Id ).ToList();
            var celebratedRequestIds = GetCelebratedRequestIds( rockContext, requestIds );

            return pageRows.Select( r =>
            {
                var status = statusLookup.TryGetValue( r.ConnectionStatusId, out var s ) ? s : null;

                return new ConnectionRequestSummaryBag
                {
                    IdKey = IdHasher.Instance.GetHash( r.Id ),
                    RequesterName = r.Requester.FullName,
                    PhotoUrl = r.Requester.PhotoId.HasValue
                        ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( r.Requester.PhotoId.Value ) )
                        : null,
                    Gender = ( Rock.Common.Mobile.Enums.Gender ) ( int ) r.Requester.Gender,
                    Comment = r.Comments?.ConvertMarkdownToHtml().StripHtml(),
                    ConnectionState = ( MobileConnectionState ) ( int ) r.ConnectionState,
                    DueStatus = GetDueStatus( r.DueDate, r.DueSoonDate, r.ConnectionState ),
                    HasCelebration = celebratedRequestIds.Contains( r.Id ),
                    StatusName = status?.Name,
                    StatusColor = status?.Color
                };
            } ).ToList();
        }

        /// <summary>
        /// Determines whether the current person may view a single request when
        /// the type enables per-request security. The assigned connector always
        /// has access; everyone else is evaluated with a lightweight request stub
        /// so explicit request-level rules and the opportunity/type inheritance
        /// chain are honored without loading the full entity graph. Mirrors the
        /// web Connections Hub (ConnectionsHub.FilterRowsByViewAuthorization).
        /// </summary>
        /// <param name="requestId">The request's identifier.</param>
        /// <param name="campusId">The request's campus, used by campus-scoped rules.</param>
        /// <param name="connectorPersonId">The request's connector person id, or <c>null</c> when unassigned.</param>
        /// <param name="connectionOpportunity">The opportunity the request belongs to, used as the parent authority.</param>
        /// <param name="currentPersonId">The current person's id, used for the connector shortcut.</param>
        /// <param name="currentPerson">The current person, used for the authorization check.</param>
        /// <returns><c>true</c> if the current person is authorized to view the request.</returns>
        private static bool IsRequestViewable( int requestId, int? campusId, int? connectorPersonId, ConnectionOpportunity connectionOpportunity, int currentPersonId, Person currentPerson )
        {
            // The assigned connector always has direct access to their own request.
            if ( connectorPersonId.HasValue && connectorPersonId.Value == currentPersonId )
            {
                return true;
            }

            var requestStub = new ConnectionRequest
            {
                Id = requestId,
                ConnectionTypeId = connectionOpportunity.ConnectionTypeId,
                ConnectionOpportunityId = connectionOpportunity.Id,
                ConnectionOpportunity = connectionOpportunity,
                CampusId = campusId
            };

            return requestStub.IsAuthorized( Authorization.VIEW, currentPerson );
        }

        /// <summary>
        /// Gets the set of request ids (from the supplied page of ids) that
        /// have a non-empty celebration note, in a single batched query.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for the query.</param>
        /// <param name="requestIds">The request ids for the current page.</param>
        /// <returns>A set of request ids that have a celebration note.</returns>
        private static HashSet<int> GetCelebratedRequestIds( RockContext rockContext, List<int> requestIds )
        {
            if ( requestIds.Count == 0 )
            {
                return new HashSet<int>();
            }

            var celebrationNoteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() );

            if ( celebrationNoteType == null )
            {
                return new HashSet<int>();
            }

            var celebrationNoteTypeId = celebrationNoteType.Id;

            var celebratedIds = new NoteService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( n => n.NoteTypeId == celebrationNoteTypeId
                    && n.EntityId.HasValue
                    && requestIds.Contains( n.EntityId.Value )
                    && n.Text != null
                    && n.Text != "" )
                .Select( n => n.EntityId.Value )
                .Distinct()
                .ToList();

            return new HashSet<int>( celebratedIds );
        }

        /// <summary>
        /// Computes a request's due status using the same buckets as the web
        /// Connections Hub: an Inactive request (or one with no due date) is
        /// always On Track; otherwise a past due date is Overdue, a due-soon
        /// date reached is Due Soon, and anything else is On Track.
        /// </summary>
        /// <param name="dueDate">The request's due date.</param>
        /// <param name="dueSoonDate">The request's due-soon date.</param>
        /// <param name="state">The request's connection state.</param>
        /// <returns>The computed <see cref="MobileDueStatus"/>.</returns>
        private static MobileDueStatus GetDueStatus( DateTime? dueDate, DateTime? dueSoonDate, ConnectionState state )
        {
            var today = RockDateTime.Today;

            if ( !dueDate.HasValue || state == ConnectionState.Inactive )
            {
                return MobileDueStatus.DueLater; // "On Track"
            }

            if ( dueDate.Value.Date < today )
            {
                return MobileDueStatus.Overdue;
            }

            if ( dueSoonDate.HasValue && dueSoonDate.Value.Date <= today )
            {
                return MobileDueStatus.DueSoon;
            }

            return MobileDueStatus.DueLater;
        }

        #endregion
    }
}

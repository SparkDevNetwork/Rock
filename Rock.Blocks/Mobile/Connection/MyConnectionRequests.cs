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
using Rock.Common.Mobile.Blocks.Connection.MyConnectionRequests;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

using MobileConnectionState = Rock.Common.Mobile.Enums.ConnectionState;
using MobileDueStatus = Rock.Common.Mobile.Enums.DueStatus;

namespace Rock.Blocks.Mobile.Connection
{
    /// <summary>
    /// Displays, in one unpaged call, every connection request assigned to the
    /// current person as connector across all connection types and
    /// opportunities (states Active / Inactive / Future Follow Up only), with
    /// each row carrying its display fields plus every key the shell needs to
    /// group, search, sort and filter client-side, a batched celebration
    /// indicator, the due status, and a floating "Add Connection Request"
    /// launcher gate.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "My Connection Requests" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the current person's connection requests across all opportunities, grouped, searchable, sortable and filterable client-side." )]
    [IconCssClass( "ti ti-list-check" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "Page to link to when the individual taps a request. The connection request IdKey is passed as the ConnectionRequest page parameter.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [LinkedPage( "Add Page",
        Description = "Page that hosts the Add Connection Request block, opened by the floating Add button. No page parameter is passed (this is a cross-opportunity worklist), so the Add screen opens with its Type and Opportunity pickers unlocked. When empty, the floating button is not shown.",
        IsRequired = false,
        Key = AttributeKey.AddPage,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "1160B498-50D7-4E8F-9B23-BFD87B7E7F22" )]
    [Rock.SystemGuid.BlockTypeGuid( "C6C6A0A3-D381-4A13-A5D0-EAA4302E78F1" )]
    public class MyConnectionRequests : RockBlockType
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
            // No campus list (the Campus filter options are derived client-side
            // from the loaded rows) and no page size (this block does not page).
            return new Rock.Common.Mobile.Blocks.Connection.MyConnectionRequests.Configuration
            {
                DetailPageGuid = GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull(),
                AddPageGuid = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets, in one unpaged response, the full set of the current person's
        /// assigned connection requests (states Active / Inactive / Future
        /// Follow Up) across all types and opportunities, plus the floating
        /// Add button gate. The shell does all grouping, searching, sorting and
        /// filtering in memory, so this action takes no parameters.
        /// </summary>
        /// <returns>The full request set plus the Add-button gate.</returns>
        [BlockAction]
        public BlockActionResult GetMyConnectionRequests()
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized( "You must be logged in to view your connection requests." );
            }

            var rockContext = RockContext;

            var requests = GetRequestSummaries( rockContext, currentPerson );

            var addPageConfigured = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull().HasValue;

            // Cross-opportunity gate: the floating Add button shows when an Add
            // page is configured AND the person can add a request somewhere (has
            // EDIT on at least one active connection type). ConnectionTypeCache
            // is cached, so this is no extra DB hit.
            var canAddSomewhere = ConnectionTypeCache.All()
                .Any( ct => ct.IsActive && ct.IsAuthorized( Authorization.EDIT, currentPerson ) );

            var response = new GetMyConnectionRequestsResponseBag
            {
                Requests = requests,
                IsAddEnabled = addPageConfigured && canAddSomewhere
            };

            return ActionOk( response );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds the full, unpaged list of request summaries for the current
        /// person, honoring per-request VIEW security.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="currentPerson">The current person (the connector this worklist is scoped to).</param>
        /// <returns>A list of <see cref="ConnectionRequestSummaryBag"/> the current person may view.</returns>
        private List<ConnectionRequestSummaryBag> GetRequestSummaries( RockContext rockContext, Person currentPerson )
        {
            /*
                6/22/2026 - CLAUDE

                This query is an intentional duplicate of the web Connections
                Hub "My Connections" mode (Rock.Blocks/Engagement/
                ConnectionsHub.cs, IsMyConnectionsMode). That logic is
                block-private (raw SQL with board/grouping concerns mobile does
                not need), so it is copied here as a LINQ query rather than
                refactored into a shared service, per the mobile Connections
                port spec (specs/260609-mobile-my-connection-requests.md).

                Unlike the single-opportunity request list (block 3), this block
                loads the full set in one call and honors per-request VIEW
                security with an in-memory IsAuthorized pass over the
                materialized list. Rock's auth inheritance handles
                ConnectionType.EnableRequestSecurity transparently: when on, the
                request's own rules are consulted; when off, the request defers
                up to the opportunity and then the type, so no branching on the
                flag is needed.

                Reason: Mobile parity with the web Connections Hub My Connections
                mode, plus per-request VIEW security (Panha 2026-06-22).
            */

            var personId = currentPerson.Id;

            // The shell only offers Active / Inactive / Future Follow Up;
            // Connected ("completed") is never returned.
            var offeredStates = new[]
            {
                ConnectionState.Active,
                ConnectionState.Inactive,
                ConnectionState.FutureFollowUp
            };

            // Materialize the candidate entities (with the navigations needed
            // for both the projection and the auth-inheritance chain) so the
            // per-request IsAuthorized( VIEW ) pass can run in memory.
            var candidates = new ConnectionRequestService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( cr => cr.ConnectionOpportunity.ConnectionType )
                .Include( cr => cr.PersonAlias.Person )
                .Include( cr => cr.Campus )
                .Include( cr => cr.ConnectionStatus )
                // ConnectorPersonAlias must be eagerly loaded: AsNoTracking
                // disables lazy loading, and the per-request IsAuthorized pass
                // below relies on it for the EnableRequestSecurity connector
                // self-view fast-path (it grants VIEW when the connector alias
                // belongs to the current person). Without it that navigation is
                // null on the detached entities and a connector's own requests
                // would be wrongly filtered out for request-secured types.
                .Include( cr => cr.ConnectorPersonAlias )
                .Where( cr => cr.ConnectorPersonAliasId.HasValue
                    && cr.ConnectorPersonAlias.PersonId == personId )
                .Where( cr => offeredStates.Contains( cr.ConnectionState ) )
                .ToList();

            // Honor per-request security. EnableRequestSecurity is handled
            // transparently by Rock's authorization inheritance.
            var authorized = candidates
                .Where( cr => cr.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();

            // Batched celebration flags for the whole set (never per row).
            var requestIds = authorized.Select( cr => cr.Id ).ToList();
            var celebratedRequestIds = GetCelebratedRequestIds( rockContext, requestIds );

            return authorized.Select( cr =>
            {
                var person = cr.PersonAlias?.Person;
                var opportunity = cr.ConnectionOpportunity;
                var connectionType = opportunity?.ConnectionType;
                var campus = cr.Campus;
                var status = cr.ConnectionStatus;

                var opportunityName = opportunity == null
                    ? null
                    : ( opportunity.PublicName.IsNotNullOrWhiteSpace() ? opportunity.PublicName : opportunity.Name );

                return new ConnectionRequestSummaryBag
                {
                    IdKey = IdHasher.Instance.GetHash( cr.Id ),
                    RequesterName = person?.FullName,
                    RequesterNickName = person?.NickName,
                    RequesterLastName = person?.LastName,
                    PhotoUrl = person?.PhotoId != null
                        ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( person.PhotoId.Value ) )
                        : null,
                    Gender = person != null ? ( Rock.Common.Mobile.Enums.Gender ) ( int ) person.Gender : Rock.Common.Mobile.Enums.Gender.Unknown,
                    Comment = cr.Comments?.ConvertMarkdownToHtml().StripHtml(),
                    ConnectionState = ( MobileConnectionState ) ( int ) cr.ConnectionState,
                    DueStatus = GetDueStatus( cr.DueDate, cr.DueSoonDate, cr.ConnectionState ),
                    HasCelebration = celebratedRequestIds.Contains( cr.Id ),
                    OpportunityGuid = opportunity?.Guid ?? Guid.Empty,
                    OpportunityName = opportunityName,
                    OpportunityIconCssClass = opportunity?.IconCssClass,
                    TypeGuid = connectionType?.Guid ?? Guid.Empty,
                    TypeName = connectionType?.Name,
                    CampusGuid = campus?.Guid,
                    CampusName = campus?.Name,
                    StatusGuid = status?.Guid ?? Guid.Empty,
                    StatusName = status?.Name,
                    StatusColor = status?.HighlightColor
                };
            } ).ToList();
        }

        /// <summary>
        /// Gets the set of request ids (from the supplied set of ids) that have
        /// a non-empty celebration note, in a single batched query.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for the query.</param>
        /// <param name="requestIds">The request ids to check.</param>
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

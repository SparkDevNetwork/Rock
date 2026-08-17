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
using Rock.Common.Mobile.Blocks.Connection.ConnectionTypeList;
using Rock.Common.Mobile.ViewModel;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

using ConnectionTypeConnectorScope = Rock.Common.Mobile.Enums.ConnectionTypeConnectorScope;

namespace Rock.Blocks.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection types the individual is authorized to
    /// view, with per-type request count summaries for the requested
    /// connector scope and campus filter.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Type List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection types with request count summaries." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "Page to link to when the individual taps a connection type. The connection type IdKey is passed as the ConnectionType page parameter.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [LinkedPage( "Add Connection Request Page",
        Description = "Page that hosts the Add Connection Request block, opened by the floating Add button. No page parameters are passed, so the Add block starts at its Type step. When empty, the floating button is not shown.",
        IsRequired = false,
        Key = AttributeKey.AddPage,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "88E9C088-5CCE-41F9-B99E-C3B03E123316" )]
    [Rock.SystemGuid.BlockTypeGuid( "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A" )]
    public class ConnectionTypeList : RockBlockType
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

            return new Rock.Common.Mobile.Blocks.Connection.ConnectionTypeList.Configuration
            {
                Campuses = campuses,
                DetailPageGuid = GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull(),
                AddPageGuid = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the connection type summaries for the requested connector
        /// scope and campus filter.
        /// </summary>
        /// <param name="options">The options that describe which requests to count.</param>
        /// <returns>The connection type summaries the current person is authorized to view.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionTypeSummaries( GetConnectionTypeSummariesRequestBag options )
        {
            if ( options == null )
            {
                return ActionBadRequest( "Missing request options." );
            }

            var currentPerson = GetCurrentPerson();
            var authorizedConnectionTypeIds = GetAuthorizedConnectionTypeIds( currentPerson );

            var summaries = LoadConnectionTypeSummaries( RockContext, currentPerson, authorizedConnectionTypeIds, options );

            // The Add button opens the wizard at its Type step and carries no
            // page parameters, so the gate asks whether the person can add
            // anywhere rather than scoping to a single type. Short-circuited on
            // the block setting so an unconfigured block does not pay for it.
            var addPageConfigured = GetAttributeValue( AttributeKey.AddPage ).AsGuidOrNull().HasValue;

            return ActionOk( new GetConnectionTypeSummariesResponseBag
            {
                Summaries = summaries,
                IsAddEnabled = addPageConfigured
                    && ConnectionRequestAuthorization.CanAddRequestAnywhere( RockContext, currentPerson )
            } );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the list of <see cref="ConnectionType"/> identifiers the current person is authorized
        /// to view or edit. Results are drawn from the in-memory cache and involve no database queries.
        /// </summary>
        /// <param name="currentPerson">The currently authenticated person.</param>
        /// <returns>A list of authorized <see cref="ConnectionType"/> identifiers.</returns>
        private List<int> GetAuthorizedConnectionTypeIds( Person currentPerson )
        {
            return ConnectionTypeCache.All()
                .Where( ct =>
                    ct.IsActive
                    && (
                        ct.IsAuthorized( Authorization.EDIT, currentPerson )
                        || ct.IsAuthorized( Authorization.VIEW, currentPerson )
                    )
                )
                .Select( ct => ct.Id )
                .ToList();
        }

        /// <summary>
        /// Loads <see cref="ConnectionType"/> data from the database and uses this data to build a
        /// list of <see cref="ConnectionTypeSummaryBag"/>s with the per-type request counts.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="currentPerson">The currently authenticated person.</param>
        /// <param name="authorizedConnectionTypeIds">The pre-computed list of authorized connection type IDs.</param>
        /// <param name="options">The options that describe which requests to count.</param>
        /// <returns>A list of <see cref="ConnectionTypeSummaryBag"/>s ordered by Order then Name.</returns>
        private List<ConnectionTypeSummaryBag> LoadConnectionTypeSummaries( RockContext rockContext, Person currentPerson, List<int> authorizedConnectionTypeIds, GetConnectionTypeSummariesRequestBag options )
        {
            /*
                6/10/2026 - CLAUDE

                This count query is an intentional duplicate of the web block
                ConnectionTypeNavigation.LoadConnectionTypeSummaries (Rock.Blocks/Connection).
                The logic is block-private on the web, so it is copied here rather
                than refactored into a shared service, per the mobile Connections
                port spec (specs/260608-mobile-connection-type-list.md). Keep the
                two in sync when the counting rules change.

                Reason: Mobile parity with the web Connection Type Navigation counts.
            */

            if ( !authorizedConnectionTypeIds.Any() )
            {
                return new List<ConnectionTypeSummaryBag>();
            }

            var personId = currentPerson?.Id ?? 0;
            var today = RockDateTime.Today;
            var limitToMyTypes = options.ConnectorScope == ConnectionTypeConnectorScope.MyTypes;

            // Resolve the campus filter to an identifier so the query does not
            // compare Guids.
            int? campusId = options.CampusGuid.HasValue
                ? CampusCache.Get( options.CampusGuid.Value )?.Id
                : null;

            var requestCountsQry = new ConnectionRequestService( rockContext )
                .Queryable()
                .Where( cr =>
                    cr.ConnectionState == ConnectionState.Active
                    && ( !campusId.HasValue || cr.CampusId == campusId.Value )
                    && authorizedConnectionTypeIds.Contains( cr.ConnectionOpportunity.ConnectionTypeId )
                    && (
                        !limitToMyTypes
                        || (
                            cr.ConnectorPersonAliasId.HasValue
                            && cr.ConnectorPersonAlias.PersonId == personId
                        )
                    )
                )
                .GroupBy( cr => cr.ConnectionOpportunity.ConnectionTypeId )
                .Select( g => new
                {
                    ConnectionTypeId = g.Key,
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

            var summaries = new ConnectionTypeService( rockContext )
                .Queryable()
                .Where( ct =>
                    authorizedConnectionTypeIds.Contains( ct.Id )
                    && (
                        !limitToMyTypes
                        || requestCountsQry.Any( a => a.ConnectionTypeId == ct.Id )
                    )
                )
                .GroupJoin(
                    requestCountsQry,
                    ct => ct.Id,
                    counts => counts.ConnectionTypeId,
                    ( ct, counts ) => new
                    {
                        ConnectionType = ct,
                        RequestCounts = counts
                    }
                )
                .SelectMany(
                    x => x.RequestCounts.DefaultIfEmpty(),
                    ( x, counts ) => new
                    {
                        x.ConnectionType.Id,
                        x.ConnectionType.IconCssClass,
                        x.ConnectionType.Name,
                        x.ConnectionType.Description,
                        x.ConnectionType.Order,
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
                .Select( s => new ConnectionTypeSummaryBag
                {
                    IdKey = IdHasher.Instance.GetHash( s.Id ),
                    IconCssClass = s.IconCssClass,
                    Name = s.Name,
                    Description = s.Description,
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

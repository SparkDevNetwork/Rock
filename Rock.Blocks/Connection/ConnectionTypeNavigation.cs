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
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Connection.ConnectionTypeNavigation;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Displays connection types that the user is authorized to view and provides easy navigation into each type's connection opportunities and requests.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Type Navigation" )]
    [Category( "Connection" )]
    [Description( "Displays connection types that the user is authorized to view and provides easy navigation into each type's connection opportunities and requests." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [LinkedPage( "Configuration Page",
        Key = AttributeKey.ConfigurationPage,
        Description = "Select the page that the configuration button should open to create and modify connection types.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_TYPES,
        Order = 0,
        IsRequired = true )]

    [LinkedPage( "Opportunities Page",
        Key = AttributeKey.OpportunitiesPage,
        Description = "Select the page that should open to view opportunities when a connection type is selected.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES,
        Order = 1,
        IsRequired = true )]

    [LinkedPage( "Connections List Page",
        Key = AttributeKey.ConnectionsListPage,
        Description = "Select the page that the list button should open to view the connections list.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_LIST,
        Order = 2,
        IsRequired = true )]

    [LinkedPage( "Connection Board Page",
        Key = AttributeKey.ConnectionBoardPage,
        Description = "Select the page that the board and grid buttons should open to view the connection board in board or grid view.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_BOARD,
        Order = 3,
        IsRequired = true )]

    [LinkedPage( "Operational Snapshot Page",
        Key = AttributeKey.OperationalSnapshotPage,
        Description = "Select the page that the snapshot button should open to view the operational snapshot.",
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT,
        Order = 4,
        IsRequired = true )]

    [ConnectionTypesField( "Connection Types",
        Key = AttributeKey.ConnectionTypes,
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        Order = 5,
        IsRequired = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E8C57557-31B7-4846-8F63-36BDDBB88719" )]
    [Rock.SystemGuid.BlockTypeGuid( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643" )]
    public class ConnectionTypeNavigation : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string OpportunitiesPage = "OpportunitiesPage";
            public const string ConnectionsListPage = "ConnectionsListPage";
            public const string ConnectionBoardPage = "ConnectionBoardPage";
            public const string OperationalSnapshotPage = "OperationalSnapshotPage";
            public const string ConnectionTypes = "ConnectionTypes";
        }

        private static class NavigationUrlKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string OpportunitiesPage = "OpportunitiesPage";
            public const string ConnectionsListPage = "ConnectionsListPage";
            public const string ConnectionBoardPage = "ConnectionBoardPage";
            public const string OperationalSnapshotPage = "OperationalSnapshotPage";
        }

        private static class PageParameterKey
        {
            public const string ConnectionType = "ConnectionType";
        }

        private static class PersonPreferenceKey
        {
            public const string TypeVisibility = "type-visibility";
        }

        #endregion Keys

        #region Fields

        private List<ListItemBag> _typeVisibilityItems;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the list of connection type visibility items the individual may select.
        /// </summary>
        private List<ListItemBag> TypeVisibilityItems
        {
            get
            {
                if ( _typeVisibilityItems == null )
                {
                    _typeVisibilityItems = new List<ListItemBag>
                    {
                        TypeVisibility.MyTypes,
                        TypeVisibility.AllTypes
                    };
                }

                return _typeVisibilityItems;
            }
        }

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets or sets the current person's connection type visibility preference.
        /// </summary>
        private string TypeVisibilityPreference
        {
            get
            {
                var typeVisibility = BlockPersonPreferences
                    .GetValue( PersonPreferenceKey.TypeVisibility );

                if ( typeVisibility.IsNotNullOrWhiteSpace() )
                {
                    return typeVisibility;
                }

                return TypeVisibility.MyTypesValue;
            }
        }

        /// <summary>
        /// Gets whether the current person is authorized to administrate this block.
        /// </summary>
        public bool CanAdministrate => BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() );

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ConnectionTypeNavigationInitializationBox
            {
                TypeVisibilityItems = TypeVisibilityItems,
                ShowConfigureConnectionTypesButton = CanAdministrate,
                ConnectionTypeSummaries = LoadConnectionTypeSummaries(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the connection type summaries.
        /// </summary>
        /// <returns>An object containing information about the connection type summaries.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionTypeSummaries()
        {
            var response = LoadConnectionTypeSummaries();

            return ActionOk( response );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Loads <see cref="ConnectionType"/> data from the database and uses this data to build a list of
        /// <see cref="ConnectionTypeSummaryBag"/>s.
        /// </summary>
        /// <returns>A list of <see cref="ConnectionTypeSummaryBag"/>s.</returns>
        private List<ConnectionTypeSummaryBag> LoadConnectionTypeSummaries()
        {
            /*
                1/27/2026 - JPH (Discussed with JME and KBH)

                While the Connection Opportunity Select block had a much more complicated approach to data loading and
                security, this block will instead follow Rock's traditional Entity-based security model while displaying
                all connection types the individual is authorized to view [or edit], rather than digging deeper (e.g.,
                into the type's connection opportunities and requests) to make these determinations.

                Reason: Simplify data loading and follow Rock's traditional Entity-based security model.
            */

            var connectionTypeFilterGuids = GetAttributeValue( AttributeKey.ConnectionTypes )
                .SplitDelimitedValues()
                .AsGuidList();

            var currentPerson = GetCurrentPerson();
            var authorizedConnectionTypeIds = ConnectionTypeCache.All()
                .Where( ct =>
                    ct.IsActive
                    && (
                        !connectionTypeFilterGuids.Any()
                        || connectionTypeFilterGuids.Contains( ct.Guid )
                    )
                    && (
                        ct.IsAuthorized( Authorization.EDIT, currentPerson )
                        || ct.IsAuthorized( Authorization.VIEW, currentPerson )
                    )
                )
                .Select( ct => ct.Id )
                .ToList();

            if ( !authorizedConnectionTypeIds.Any() )
            {
                return new List<ConnectionTypeSummaryBag>();
            }

            var personId = currentPerson?.Id ?? 0;
            var campusId = RequestContext.GetContextEntity<Campus>()?.Id;
            var today = RockDateTime.Today;
            var limitToMyTypes = TypeVisibilityPreference == TypeVisibility.MyTypesValue;

            var requestCountsQry = new ConnectionRequestService( RockContext )
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

            var summaries = new ConnectionTypeService( RockContext )
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
                    ( x, counts ) => new ConnectionTypeSummaryBag
                    {
                        Id = x.ConnectionType.Id,
                        IconCssClass = x.ConnectionType.IconCssClass,
                        Name = x.ConnectionType.Name,
                        Description = x.ConnectionType.Description,
                        Order = x.ConnectionType.Order,
                        ActiveRequestCount = counts == null ? 0 : counts.ActiveRequestCount,
                        DueSoonRequestCount = counts == null ? 0 : counts.DueSoonRequestCount,
                        OverdueRequestCount = counts == null ? 0 : counts.OverdueRequestCount,
                        UnassignedRequestCount = counts == null ? 0 : counts.UnassignedRequestCount,
                        AssignedToYouRequestCount = counts == null ? 0 : counts.AssignedToYouRequestCount,
                        EnabledViews = x.ConnectionType.EnabledViews
                    }
                )
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .ToList();

            summaries.ForEach( s => s.TranslateIdToIdKey() );

            return summaries;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ConfigurationPage] = this.GetLinkedPageUrl( AttributeKey.ConfigurationPage ),
                [NavigationUrlKey.OpportunitiesPage] = this.GetLinkedPageUrl( AttributeKey.OpportunitiesPage, PageParameterKey.ConnectionType, "((Key))" ),
                [NavigationUrlKey.ConnectionsListPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionsListPage, PageParameterKey.ConnectionType, "((Key))" ),
                [NavigationUrlKey.ConnectionBoardPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionBoardPage, PageParameterKey.ConnectionType, "((Key))" ),
                [NavigationUrlKey.OperationalSnapshotPage] = this.GetLinkedPageUrl( AttributeKey.OperationalSnapshotPage, PageParameterKey.ConnectionType, "((Key))" )
            };
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent available connection type visibility options.
        /// </summary>
        private class TypeVisibility
        {
            public const string MyTypesValue = "my-types";
            public const string AllTypesValue = "all-types";

            private static readonly ListItemBag _myTypes = new ListItemBag { Text = "My Types", Value = MyTypesValue };
            public static ListItemBag MyTypes => _myTypes;

            private static readonly ListItemBag _allTypes = new ListItemBag { Text = "All Types", Value = AllTypesValue };
            public static ListItemBag AllTypes => _allTypes;
        }

        /// <summary>
        /// A POCO to represent a <see cref="ConnectionRequest"/>'s relevant properties needed for aggregation.
        /// </summary>
        private class ConnectionRequestInfo
        {
            /// <summary>
            /// The identifier of the <see cref="ConnectionType"/> to which this request belongs.
            /// </summary>
            public int ConnectionTypeId { get; set; }

            /// <inheritdoc cref="ConnectionRequest.ConnectionState"/>
            public ConnectionState ConnectionState { get; set; }

            /// <inheritdoc cref="ConnectionRequest.ConnectorPersonAliasId"/>
            public int? ConnectorPersonAliasId { get; set; }

            /// <inheritdoc cref="ConnectionRequest.DueDate"/>
            public DateTime? DueDate { get; set; }

            /// <inheritdoc cref="ConnectionRequest.DueSoonDate"/>
            public DateTime? DueSoonDate { get; set; }
        }

        #endregion Supporting Classes
    }
}

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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model.Connection.ConnectionOpportunity.Options;

namespace Rock.Model
{
    public partial class ConnectionOpportunityService
    {
        #region Default Options

        /// <summary>
        /// The default options to use if not specified. This saves a few
        /// CPU cycles from having to create a new one each time.
        /// </summary>
        private static readonly ConnectionOpportunityQueryOptions DefaultGetConnectionTypesOptions = new ConnectionOpportunityQueryOptions();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the connection opportunities queryable that matches the specified
        /// options.
        /// </summary>
        /// <param name="options">The filter to apply to the query.</param>
        /// <returns>A queryable of <see cref="ConnectionOpportunity"/> objects.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public IQueryable<ConnectionOpportunity> GetConnectionOpportunitiesQuery( ConnectionOpportunityQueryOptions options = null )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new InvalidOperationException( "Context is not a RockContext." );
            }

            options = options ?? DefaultGetConnectionTypesOptions;

            var qry = Queryable();

            if ( options.ConnectionTypeGuids != null && options.ConnectionTypeGuids.Any() )
            {
                qry = qry.Where( o => options.ConnectionTypeGuids.Contains( o.ConnectionType.Guid ) );
            }

            if ( options.ConnectorPersonIds != null && options.ConnectorPersonIds.Any() )
            {
                var connectorRequestsQry = new ConnectionRequestService( rockContext ).Queryable()
                    .Where( r => r.ConnectionState != ConnectionState.Connected
                        && r.ConnectorPersonAliasId.HasValue
                        && options.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) )
                    .Select( r => r.Id );

                qry = qry.Where( o => o.ConnectionRequests.Any( r => connectorRequestsQry.Contains( r.Id ) ) );
            }

            if ( !options.IncludeInactive )
            {
                qry = qry.Where( o => o.IsActive && o.ConnectionType.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Get all the Connection Statuses for a Connection Opportunity
        /// </summary>
        /// <param name="connectionOpportunityId"></param>
        /// <returns></returns>
        public List<ConnectionStatus> GetStatuses( int connectionOpportunityId )
        {
            return Queryable()
                .AsNoTracking()
                .Where( co => co.Id == connectionOpportunityId  )
                .SelectMany( co => co.ConnectionType.ConnectionStatuses )
                .Where( cs => cs.IsActive )
                .ToList()
                .OrderBy( cs => cs.Order )
                .ToList();
        }

        /// <summary>
        /// Gets the active, non-archived groups available for placement on the specified
        /// Connection Opportunity. This combines both placement configurations the opportunity
        /// supports: groups explicitly assigned to the opportunity (see <see cref="ConnectionOpportunityGroup"/>)
        /// and every group belonging to a Group Type whose configuration has
        /// <see cref="ConnectionOpportunityGroupConfig.UseAllGroupsOfType"/> set to <c>true</c>.
        /// An <see cref="IQueryable{T}"/> is returned so callers can add their own includes,
        /// filters, or projections without materializing more groups than they need.
        /// </summary>
        /// <param name="connectionOpportunityId">The identifier of the Connection Opportunity to retrieve placement groups for.</param>
        /// <returns>A queryable of the <see cref="Group"/> objects available for placement on the opportunity.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public IQueryable<Group> GetPlacementGroups( int connectionOpportunityId )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new InvalidOperationException( "Context is not a RockContext." );
            }

            // Group Ids explicitly assigned to this opportunity.
            var explicitGroupIdsQuery = new ConnectionOpportunityGroupService( rockContext )
                .Queryable()
                .Where( g => g.ConnectionOpportunityId == connectionOpportunityId )
                .Select( g => g.GroupId );

            // Group Type Ids configured with "use all groups of this type" for this opportunity.
            var allGroupTypeIdsQuery = new ConnectionOpportunityGroupConfigService( rockContext )
                .Queryable()
                .Where( c => c.ConnectionOpportunityId == connectionOpportunityId && c.UseAllGroupsOfType )
                .Select( c => c.GroupTypeId );

            // A single query returns groups from both sources, deduplicated (each group is one
            // row), excluding inactive and archived groups.
            return new GroupService( rockContext )
                .Queryable()
                .Where( g => g.IsActive && !g.IsArchived )
                .Where( g => explicitGroupIdsQuery.Contains( g.Id ) || allGroupTypeIdsQuery.Contains( g.GroupTypeId ) );
        }

        /// <summary>
        /// Gets the active, non-archived groups available for placement on the specified
        /// Connection Opportunity, filtered to those available for the given campus. A group is
        /// included when no campus is specified, when the group has no campus, or when the group's
        /// campus matches the specified campus. See <see cref="GetPlacementGroups(int)"/> for how
        /// the available groups are determined.
        /// </summary>
        /// <param name="connectionOpportunityId">The identifier of the Connection Opportunity to retrieve placement groups for.</param>
        /// <param name="campusId">The identifier of the campus to filter the groups by, or <c>null</c> to return groups for all campuses.</param>
        /// <returns>A queryable of the <see cref="Group"/> objects available for placement on the opportunity for the specified campus.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public IQueryable<Group> GetPlacementGroups( int connectionOpportunityId, int? campusId )
        {
            var qry = GetPlacementGroups( connectionOpportunityId );

            if ( campusId.HasValue )
            {
                qry = qry.Where( g => !g.CampusId.HasValue || g.CampusId.Value == campusId.Value );
            }

            return qry;
        }

        #endregion
    }
}

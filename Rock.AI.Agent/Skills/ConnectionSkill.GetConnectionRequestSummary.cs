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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ConnectionSkill
    {
        #region Tool(s)

        [Description( "Returns a summary of connection requests for the user." )]
        [AgentPurpose( "Retrieves summary counts of connection requests." )]
        [AgentUsage( "Connectors are people who are assigned a request." )]
        [AgentToolGuid( "b3df0351-aa63-44bf-98fd-16fc56ad2d39" )]
        public RockToolResult GetConnectionRequestSummary(
            string connectionTypeIdKey = null,
            string connectionOpportunityIdKey = null,
            string campusIdKey = null,
            string connectorPersonIdKey = null,

            [Description( "Must be blank or exactly one of these values (do not infer additional values): ConnectionType, ConnectionOpportunity, Campus, ConnectionStatus" )]
            string primaryDimension = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var dimensions = new List<string> { "ConnectionType", "ConnectionOpportunity", "Campus", "ConnectionStatus" };

            var query = new ConnectionRequestService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( cr => !cr.ConnectedDateTime.HasValue );

            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionTypeId, connectionTypeIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionOpportunityId, connectionOpportunityIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.CampusId, campusIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectorPersonAlias.PersonId, connectorPersonIdKey );

            helper.SetPrimaryDimension( primaryDimension, dimensions );

            // If we are grouping by opportunity, then it doesn't make sense
            // to still group by type.
            if ( primaryDimension.Equals( "ConnectionOpportunity", StringComparison.OrdinalIgnoreCase ) )
            {
                dimensions.Remove( "ConnectionType" );
            }

            // Remove any dimensions that have already been satisfied by
            // filter options.
            helper.RemoveSatisfiedDimensions( connectionTypeIdKey, dimensions, ["ConnectionType"] );
            helper.RemoveSatisfiedDimensions( connectionOpportunityIdKey, dimensions, ["ConnectionOpportunity", "ConnectionType"] );
            helper.RemoveSatisfiedDimensions( campusIdKey, dimensions, ["Campus"] );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // Perform the SQL level grouping of all data into a set of rows
            // that contain the count of each possible combination of grouped
            // values.
            var flatCounts = query
                .GroupBy( cr => new
                {
                    cr.ConnectionOpportunity.ConnectionTypeId,
                    cr.ConnectionOpportunityId,
                    cr.CampusId,
                    cr.ConnectionStatusId,
                } )
                .Select( cr => new SummaryFlatCount
                {
                    ConnectionTypeId = cr.Key.ConnectionTypeId,
                    ConnectionOpportunityId = cr.Key.ConnectionOpportunityId,
                    CampusId = cr.Key.CampusId,
                    ConnectionStatusId = cr.Key.ConnectionStatusId,
                    Count = cr.Count(),
                } )
                .ToList();

            var summary = GetSummaryResult( helper, dimensions, flatCounts );

            return Success( summary );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the <see cref="SummaryResult"/> object that contains all the
        /// summarized row counts across all the dimensions.
        /// </summary>
        /// <param name="helper">The helper that will be used to process the data.</param>
        /// <param name="dimensions">The dimensions that will be constructed in order.</param>
        /// <param name="flatCounts">The flat row counts of all possible group combinations.</param>
        /// <returns>The <see cref="SummaryResult"/> object that can be returned to the language model.</returns>
        private SummaryResult GetSummaryResult( AgentToolHelper helper, List<string> dimensions, List<SummaryFlatCount> flatCounts )
        {
            List<SummaryGroupResult> groups = null;
            var state = GetSummaryState( flatCounts );
            var summary = new SummaryResult
            {
                GroupingDimensions = dimensions,
            };

            foreach ( var groupLevel in dimensions )
            {
                switch ( groupLevel )
                {
                    case "ConnectionType":
                        if ( groups == null )
                        {
                            groups = GetConnectionTypeSummaries( null, state );
                        }
                        else
                        {
                            groups = helper.PopulateSummaryGroupings( groups, state, GetConnectionTypeSummaries );
                        }

                        break;

                    case "ConnectionOpportunity":
                        if ( groups == null )
                        {
                            groups = GetConnectionOpportunitySummaries( null, state );
                        }
                        else
                        {
                            groups = helper.PopulateSummaryGroupings( groups, state, GetConnectionOpportunitySummaries );
                        }

                        break;

                    case "Campus":
                        if ( groups == null )
                        {
                            groups = GetCampusSummaries( null, state );
                        }
                        else
                        {
                            groups = helper.PopulateSummaryGroupings( groups, state, GetCampusSummaries );
                        }

                        break;

                    case "ConnectionStatus":
                        if ( groups == null )
                        {
                            groups = GetConnectionStatusSummaries( null, state );
                        }
                        else
                        {
                            groups = helper.PopulateSummaryGroupings( groups, state, GetConnectionStatusSummaries );
                        }

                        break;
                }

                summary.Groups ??= groups;
            }

            summary.Total = summary.Groups.Sum( g => g.Total );

            return summary;
        }

        /// <summary>
        /// Builds the state object that contains all the information required
        /// to build the summary. This includes the flat counts and any cached
        /// data.
        /// </summary>
        /// <param name="flatCounts">The flat row counts of all possible group combinations.</param>
        /// <returns>A <see cref="SummaryState"/> that can be used to build the results.</returns>
        private SummaryState GetSummaryState( List<SummaryFlatCount> flatCounts )
        {
            var connectionOpportunityIds = flatCounts.Select( fc => fc.ConnectionOpportunityId ).Distinct().ToList();
            var connectionOpportunities = new ConnectionOpportunityService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( co => connectionOpportunityIds.Contains( co.Id ) )
                .Select( co => new
                {
                    co.Id,
                    co.Name,
                } )
                .ToDictionary( co => co.Id, co => co.Name );

            var connectionStatusIds = flatCounts.Select( fc => fc.ConnectionStatusId ).Distinct().ToList();
            var connectionStatuses = new ConnectionStatusService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( co => connectionStatusIds.Contains( co.Id ) )
                .Select( co => new
                {
                    co.Id,
                    co.Name,
                } )
                .ToDictionary( co => co.Id, co => co.Name );

            return new SummaryState
            {
                ConnectionTypes = ConnectionTypeCache.All( AgentRequestContext.RockContext ).ToDictionary( ct => ct.Id, ct => ct.Name ),
                ConnectionOpportunities = connectionOpportunities,
                ConnectionStatuses = connectionStatuses,
                Campuses = CampusCache.All( AgentRequestContext.RockContext ).ToDictionary( c => c.Id, c => c.Name ),
                Source = flatCounts,
            };
        }

        /// <summary>
        /// Generates the summary groups by connection type.
        /// </summary>
        /// <param name="parentGroup">The parent group to generate the sub-group summaries for.</param>
        /// <param name="state">The state object that contains our cached information.</param>
        /// <returns>A list of <see cref="SummaryGroupResult"/> that represent the summary for each sub-group.</returns>
        private List<SummaryGroupResult> GetConnectionTypeSummaries( SummaryGroupResult parentGroup, SummaryState state )
        {
            var source = ( IEnumerable<SummaryFlatCount> ) parentGroup?.Source ?? state.Source;

            return source.GroupBy( fc => fc.ConnectionTypeId )
                .Select( g =>
                {
                    return new SummaryGroupResult
                    {
                        Id = g.Key,
                        Name = state.ConnectionTypes[g.Key],
                        Total = g.Sum( fc => fc.Count ),
                        Source = g,
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Generates the summary groups by connection opportunity.
        /// </summary>
        /// <param name="parentGroup">The parent group to generate the sub-group summaries for.</param>
        /// <param name="state">The state object that contains our cached information.</param>
        /// <returns>A list of <see cref="SummaryGroupResult"/> that represent the summary for each sub-group.</returns>
        private List<SummaryGroupResult> GetConnectionOpportunitySummaries( SummaryGroupResult parentGroup, SummaryState cache )
        {
            var source = ( IEnumerable<SummaryFlatCount> ) parentGroup?.Source ?? cache.Source;

            return source.GroupBy( fc => fc.ConnectionOpportunityId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key,
                    Name = cache.ConnectionOpportunities[g.Key],
                    Total = g.Sum( fc => fc.Count ),
                    Source = g,
                } )
                .ToList();
        }

        /// <summary>
        /// Generates the summary groups by campus.
        /// </summary>
        /// <param name="parentGroup">The parent group to generate the sub-group summaries for.</param>
        /// <param name="state">The state object that contains our cached information.</param>
        /// <returns>A list of <see cref="SummaryGroupResult"/> that represent the summary for each sub-group.</returns>
        private List<SummaryGroupResult> GetCampusSummaries( SummaryGroupResult parentGroup, SummaryState cache )
        {
            var source = ( IEnumerable<SummaryFlatCount> ) parentGroup?.Source ?? cache.Source;

            return source.GroupBy( fc => fc.CampusId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key ?? 0,
                    Name = g.Key.HasValue ? cache.Campuses[g.Key.Value] : "No Campus",
                    Total = g.Sum( fc => fc.Count ),
                    Source = g,
                } )
                .ToList();
        }

        /// <summary>
        /// Generates the summary groups by connection status.
        /// </summary>
        /// <param name="parentGroup">The parent group to generate the sub-group summaries for.</param>
        /// <param name="state">The state object that contains our cached information.</param>
        /// <returns>A list of <see cref="SummaryGroupResult"/> that represent the summary for each sub-group.</returns>
        private List<SummaryGroupResult> GetConnectionStatusSummaries( SummaryGroupResult parentGroup, SummaryState cache )
        {
            var source = ( IEnumerable<SummaryFlatCount> ) parentGroup?.Source ?? cache.Source;

            return source.GroupBy( fc => fc.ConnectionStatusId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key,
                    Name = cache.ConnectionStatuses[g.Key],
                    Total = g.Sum( fc => fc.Count ),
                    Source = g,
                } )
                .ToList();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The counts from SQL for a single combination of all possible group-by
        /// values. In SQL we group-by all the properties in here except Count
        /// and then use Count to store those values. This lets us quickly
        /// in-memory sum the Count values by any dimension we need.
        /// </summary>
        private class SummaryFlatCount
        {
            public int ConnectionTypeId { get; set; }

            public int ConnectionOpportunityId { get; set; }

            public int? CampusId { get; set; }

            public int ConnectionStatusId { get; set; }

            public int Count { get; set; }
        }

        /// <summary>
        /// The state object that contains all the cached information required
        /// to build the summary results. This includes the flat count rows
        /// from SQL and any cached name lookups.
        /// </summary>
        private class SummaryState
        {
            public Dictionary<int, string> ConnectionOpportunities { get; set; }

            public Dictionary<int, string> ConnectionStatuses { get; set; }

            public Dictionary<int, string> ConnectionTypes { get; set; }

            public Dictionary<int, string> Campuses { get; set; }

            public IEnumerable<SummaryFlatCount> Source { get; set; }
        }

        #endregion
    }
}

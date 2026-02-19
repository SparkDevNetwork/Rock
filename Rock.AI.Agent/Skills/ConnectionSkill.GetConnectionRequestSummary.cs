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
#warning Implement dueStatus filter
            //[Description( "Must be blank or exactly one of these values (do not infer additional values): PastDue, DueSoon, NotDue." )]
            //string dueStatus = null,
            DateTime? startDate = null,
            DateTime? endDate = null,

            [Description( "Must be blank or exactly one of these values (do not infer additional values): ConnectionType, ConnectionOpportunity, Campus, ConnectionStatus" )]
            string primaryDimension = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var dimensions = new List<string> { "ConnectionType", "ConnectionOpportunity", "Campus", "ConnectionStatus" };

            var query = new ConnectionRequestService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( cr => !cr.ConnectedDateTime.HasValue );

            if ( startDate.HasValue )
            {
                query = query.Where( cr => cr.CreatedDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                query = query.Where( cr => cr.CreatedDateTime < endDate.Value );
            }

            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionTypeId, connectionTypeIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionOpportunityId, connectionOpportunityIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.CampusId, campusIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectorPersonAlias.PersonId, connectorPersonIdKey );

            helper.SetPrimaryDimension( primaryDimension, dimensions );

            // If we are grouping by opportunity, then it doesn't make sense
            // to still group by type.
            if ( "ConnectionOpportunity".Equals( primaryDimension, StringComparison.OrdinalIgnoreCase ) )
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
            var groupCounts = query
                .GroupBy( cr => new
                {
                    cr.ConnectionOpportunity.ConnectionTypeId,
                    cr.ConnectionOpportunityId,
                    cr.CampusId,
                    cr.ConnectionStatusId,
                } )
                .Select( cr => new SummaryGroupCount
                {
                    ConnectionTypeId = cr.Key.ConnectionTypeId,
                    ConnectionOpportunityId = cr.Key.ConnectionOpportunityId,
                    CampusId = cr.Key.CampusId,
                    ConnectionStatusId = cr.Key.ConnectionStatusId,
                    Count = cr.Count(),
                } )
                .ToList();

            var summary = GetSummaryResult( helper, dimensions, groupCounts );

            return Success( summary ).WithoutHistoryContent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the <see cref="SummaryResult"/> object that contains all the
        /// summarized row counts across all the dimensions.
        /// </summary>
        /// <param name="helper">The helper that will be used to process the data.</param>
        /// <param name="dimensions">The dimensions that will be constructed in order.</param>
        /// <param name="groupCounts">The row counts of all possible group combinations.</param>
        /// <returns>The <see cref="SummaryResult"/> object that can be returned to the language model.</returns>
        private SummaryResult GetSummaryResult( AgentToolHelper helper, List<string> dimensions, List<SummaryGroupCount> groupCounts )
        {
            List<SummaryGroupResult> groups = null;
            var state = GetSummaryState( groupCounts );
            var summary = new SummaryResult
            {
                GroupingDimensions = dimensions,
            };

            foreach ( var dimension in dimensions )
            {
                switch ( dimension )
                {
                    case "ConnectionType":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.ConnectionTypeId, state.ConnectionTypes );
                        break;

                    case "ConnectionOpportunity":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.ConnectionOpportunityId, state.ConnectionOpportunities );
                        break;

                    case "Campus":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.CampusId, state.Campuses );
                        break;

                    case "ConnectionStatus":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.ConnectionStatusId, state.ConnectionStatuses );
                        break;
                }

                summary.Groups ??= groups;
            }

            summary.Total = summary.Groups.Sum( g => g.Total );

            return summary;
        }

        /// <summary>
        /// Builds the state object that contains all the information required
        /// to build the summary. This includes the group counts and any cached
        /// data.
        /// </summary>
        /// <param name="groupCounts">The row counts of all possible group combinations.</param>
        /// <returns>A <see cref="SummaryState"/> that can be used to build the results.</returns>
        private SummaryState GetSummaryState( List<SummaryGroupCount> groupCounts )
        {
            var connectionOpportunityIds = groupCounts.Select( fc => fc.ConnectionOpportunityId ).Distinct().ToList();
            var connectionOpportunities = new ConnectionOpportunityService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( co => connectionOpportunityIds.Contains( co.Id ) )
                .Select( co => new
                {
                    co.Id,
                    co.Name,
                } )
                .ToDictionary( co => co.Id, co => co.Name );

            var connectionStatusIds = groupCounts.Select( fc => fc.ConnectionStatusId ).Distinct().ToList();
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
            };
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The counts from SQL for a single combination of all possible group-by
        /// values. In SQL we group-by all the properties in here except Count
        /// and then use Count to store those values. This lets us quickly
        /// in-memory sum the Count values by any dimension we need.
        /// </summary>
        private class SummaryGroupCount : ISummaryGroupCount
        {
            public int ConnectionTypeId { get; set; }

            public int ConnectionOpportunityId { get; set; }

            public int? CampusId { get; set; }

            public int ConnectionStatusId { get; set; }

            public int Count { get; set; }
        }

        /// <summary>
        /// The state object that contains all the cached information required
        /// to build the summary results. This includes the grouped row counts
        /// from SQL and any cached name lookups.
        /// </summary>
        private class SummaryState
        {
            public Dictionary<int, string> ConnectionOpportunities { get; set; }

            public Dictionary<int, string> ConnectionStatuses { get; set; }

            public Dictionary<int, string> ConnectionTypes { get; set; }

            public Dictionary<int, string> Campuses { get; set; }
        }

        #endregion
    }
}

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ConnectionSkill
{
    #region Tool(s)

    [Description( "Returns the insights of connection requests." )]
    [AgentPurpose( "Retrieves an set of insights into connection requests." )]
    [AgentToolGuid( "51e14e2d-09a4-440e-9e7d-df1bf22bd918" )]
    public AgentToolResult GetConnectionRequestInsights(
        string connectionOpportunityIdKey = null,
        string campusIdKey = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var query = new ConnectionRequestService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( cr => !cr.ConnectedDateTime.HasValue );

        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionOpportunityId, connectionOpportunityIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.CampusId, campusIdKey );

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
                cr.ConnectorPersonAliasId,
                cr.WasCompletedOnTime,
                cr.ConnectionStatusId,
            } )
            .Select( cr => new InsightsGroupCount
            {
                ConnectorPersonAliasId = cr.Key.ConnectorPersonAliasId,
                WasCompletedOnTime = cr.Key.WasCompletedOnTime,
                ConnectionStatusId = cr.Key.ConnectionStatusId,
                Count = cr.Count(),
            } )
            .ToList();

        var summary = GetInsightsResult( groupCounts );

        return Success( summary ).WithoutHistoryContent();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get the result object for the insights from the counts obtained
    /// from SQL.
    /// </summary>
    /// <param name="groupCounts">The counts from SQL.</param>
    /// <returns>The result to return to the language model.</returns>
    private ConnectionRequestInsightsResult GetInsightsResult( List<InsightsGroupCount> groupCounts )
    {
        var state = GetInsightsState( groupCounts );

        var summary = new ConnectionRequestInsightsResult
        {
            ActiveCount = groupCounts.Sum( gc => gc.Count ),
            UnassignedCount = groupCounts.Where( gc => !gc.ConnectorPersonAliasId.HasValue ).Sum( gc => gc.Count ),
            CountByStatus = groupCounts
                .GroupBy( gc => gc.ConnectionStatusId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key,
                    Name = state.ConnectionStatuses[g.Key],
                    Total = g.Sum( gc => gc.Count ),
                } )
                .ToList(),
        };

        var topConnectorPersonAliasIds = groupCounts
            .Where( gc => gc.ConnectorPersonAliasId.HasValue )
            .GroupBy( gc => gc.ConnectorPersonAliasId.Value )
            .Select( g => new
            {
                ConnectorPersonAliasId = g.Key,
                ActiveCount = g.Sum( gc => gc.Count ),
            } )
            .OrderByDescending( g => g.ActiveCount )
            .Select( g => g.ConnectorPersonAliasId )
            .Take( 10 )
            .ToList();

        var people = new PersonAliasService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( pa => topConnectorPersonAliasIds.Contains( pa.Id ) )
            .ToDictionary( pa => pa.Id, pa => pa.Person );

        summary.TopConnectors = people
            .Select( kvp => new InsightsConnectorResult
            {
                Connector = PersonResult.Basic( kvp.Value ),
                ActiveCount = groupCounts
                    .Where( gc => gc.ConnectorPersonAliasId == kvp.Key )
                    .Sum( gc => gc.Count ),
            } )
            .OrderByDescending( c => c.ActiveCount )
            .ToList();

        return summary;
    }

    /// <summary>
    /// Get the state object that contains all the cached information
    /// required to build the result.
    /// </summary>
    /// <param name="groupCounts">The group count objects from SQL.</param>
    /// <returns>A new instance of <see cref="InsightsState"/>.</returns>
    private InsightsState GetInsightsState( List<InsightsGroupCount> groupCounts )
    {
        var connectionStatusIds = groupCounts
            .Select( gc => gc.ConnectionStatusId )
            .Distinct()
            .ToList();

        var state = new InsightsState
        {
            ConnectionStatuses = new ConnectionStatusService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( cs => connectionStatusIds.Contains( cs.Id ) )
                .ToDictionary( cs => cs.Id, cs => cs.Name ),
        };

        return state;
    }

    #endregion

    #region Support Classes

    /// <summary>
    /// The counts from SQL for a single combination of all possible group-by
    /// values. In SQL we group-by all the properties in here except Count
    /// and then use Count to store those values. This lets us quickly
    /// in-memory sum the Count values by any dimension we need.
    /// </summary>
    private class InsightsGroupCount
    {
        public int? ConnectorPersonAliasId { get; set; }

        public bool WasCompletedOnTime { get; set; }

        public int ConnectionStatusId { get; set; }

        public int Count { get; set; }
    }

    /// <summary>
    /// The state object that contains all the cached information required
    /// to build the summary results. This includes the grouped row counts
    /// from SQL and any cached name lookups.
    /// </summary>
    private class InsightsState
    {
        public Dictionary<int, string> ConnectionStatuses { get; set; }
    }

    #endregion
}

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
using System.Text.Json.Serialization;
using System.Web.Caching;

using DocumentFormat.OpenXml.Spreadsheet;

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
        public RockToolResult SummarizeConnectionRequests(
            string connectionTypeIdKey = null,
            string connectionOpportunityIdKey = null,
            string campusIdKey = null,
            string connectorPersonIdKey = null,

            [Description( "The primary grouping to use for the results: ConnectionType, ConnectionOpportunity, Campus, Connector" )]
            string primaryGrouping = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            var groupingOperations = new List<string> { "ConnectionType", "ConnectionOpportunity", "Campus", "ConnectionStatus" };

            var query = new ConnectionRequestService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( cr => !cr.ConnectedDateTime.HasValue );

            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionTypeId, connectionTypeIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionOpportunityId, connectionOpportunityIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.CampusId, campusIdKey );
            query = helper.WhereOptionalIdKey( query, cr => cr.ConnectorPersonAlias.PersonId, connectorPersonIdKey );

            if ( primaryGrouping.IsNotNullOrWhiteSpace() )
            {
                var operationIndex = groupingOperations.FindIndex( o =>
                    o.Equals( primaryGrouping, StringComparison.OrdinalIgnoreCase ) );

                if ( operationIndex >= 0 )
                {
                    // Move the specified grouping to the front of the list so
                    // it will be the primary grouping.
                    var operation = groupingOperations[operationIndex];

                    groupingOperations.RemoveAt( operationIndex );
                    groupingOperations.Insert( 0, operation );
                }
                else
                {
                    helper.AddError( $"The specified primary grouping '{primaryGrouping}' is not valid. Valid options are: {string.Join( ", ", groupingOperations )}." );
                }
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( connectionTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                groupingOperations.Remove( "ConnectionType" );
            }

            if ( connectionOpportunityIdKey.IsNotNullOrWhiteSpace() )
            {
                groupingOperations.Remove( "ConnectionOpportunity" );
            }

            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                groupingOperations.Remove( "Campus" );
            }

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

            var cache = new SummaryCache
            {
                ConnectionTypes = ConnectionTypeCache.All( AgentRequestContext.RockContext ).ToDictionary( ct => ct.Id, ct => ct.Name ),
                ConnectionOpportunities = connectionOpportunities,
                ConnectionStatuses = connectionStatuses,
                Campuses = CampusCache.All( AgentRequestContext.RockContext ).ToDictionary( c => c.Id, c => c.Name ),
            };

            var summary = new SummaryResult
            {
                GroupingDimensions = groupingOperations,
            };
            List<SummaryGroupResult> groups = null;

            foreach ( var groupLevel in groupingOperations )
            {
                switch ( groupLevel )
                {
                    case "ConnectionType":
                        if ( groups == null )
                        {
                            groups = GetConnectionTypeSummaries( flatCounts, cache );
                        }
                        else
                        {
                            //groups = AddChildGroupings( groups, GetConnectionTypeSummaries );
                            groups = groups.SelectMany( g =>
                            {
                                var fc = ( IEnumerable<SummaryFlatCount> ) g.CustomContext;
                                var newGroups = GetConnectionTypeSummaries( fc, cache );

                                g.Groups = newGroups;

                                return newGroups;
                            } ).ToList();
                        }

                        break;

                    case "ConnectionOpportunity":
                        if ( groups == null )
                        {
                            groups = GetConnectionOpportunitySummaries( flatCounts, cache );
                        }
                        else
                        {
                            groups = groups.SelectMany( g =>
                            {
                                var fc = ( IEnumerable<SummaryFlatCount> ) g.CustomContext;
                                var newGroups = GetConnectionOpportunitySummaries( fc, cache );

                                g.Groups = newGroups;

                                return newGroups;
                            } ).ToList();
                        }

                        break;

                    case "Campus":
                        if ( groups == null )
                        {
                            groups = GetCampusSummaries( flatCounts, cache );
                        }
                        else
                        {
                            groups = groups.SelectMany( g =>
                            {
                                var fc = ( IEnumerable<SummaryFlatCount> ) g.CustomContext;
                                var newGroups = GetCampusSummaries( fc, cache );

                                g.Groups = newGroups;

                                return newGroups;
                            } ).ToList();
                        }

                        break;

                    case "ConnectionStatus":
                        if ( groups == null )
                        {
                            groups = GetConnectionStatusSummaries( flatCounts, cache );
                        }
                        else
                        {
                            groups = groups.SelectMany( g =>
                            {
                                var fc = ( IEnumerable<SummaryFlatCount> ) g.CustomContext;
                                var newGroups = GetConnectionStatusSummaries( fc, cache );

                                g.Groups = newGroups;

                                return newGroups;
                            } ).ToList();
                        }

                        break;
                }

                if ( summary.Groups == null )
                {
                    summary.Groups = groups;
                }
            }

            summary.Total = summary.Groups.Sum( g => g.Total );

            return Success( summary );
        }

        //private static List<SummaryGroupResult> AddChildGroupings<TSource>( IEnumerable<SummaryGroupResult> parentGroups, Func<TSource, List<SummaryGroupResult>> callback )
        //    where TSource : class
        //{
        //    return parentGroups.SelectMany( g =>
        //    {
        //        var newGroups = callback( ( TSource ) g.CustomContext, context );

        //        g.Groups = newGroups;

        //        return newGroups;
        //    } ).ToList();
        //}


        private class SummaryCache
        {
            public Dictionary<int, string> ConnectionOpportunities { get; set; }

            public Dictionary<int, string> ConnectionStatuses { get; set; }

            public Dictionary<int, string> ConnectionTypes { get; set; }

            public Dictionary<int, string> Campuses { get; set; }
        }

        private List<SummaryGroupResult> GetConnectionTypeSummaries( IEnumerable<SummaryFlatCount> flatCounts, SummaryCache cache )
        {
            return flatCounts.GroupBy( fc => fc.ConnectionTypeId )
                .Select( g =>
                {
                    return new SummaryGroupResult
                    {
                        Id = g.Key,
                        Name = cache.ConnectionTypes[g.Key],
                        Total = g.Sum( fc => fc.Count ),
                        CustomContext = g,
                    };
                } )
                .ToList();
        }

        private List<SummaryGroupResult> GetConnectionOpportunitySummaries( IEnumerable<SummaryFlatCount> flatCounts, SummaryCache cache )
        {
            return flatCounts.GroupBy( fc => fc.ConnectionOpportunityId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key,
                    Name = cache.ConnectionOpportunities[g.Key],
                    Total = g.Sum( fc => fc.Count ),
                    CustomContext = g,
                } )
                .ToList();
        }

        private List<SummaryGroupResult> GetCampusSummaries( IEnumerable<SummaryFlatCount> flatCounts, SummaryCache cache )
        {
            return flatCounts.GroupBy( fc => fc.CampusId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key ?? 0,
                    Name = g.Key.HasValue ? cache.Campuses[g.Key.Value] : "No Campus",
                    Total = g.Sum( fc => fc.Count ),
                    CustomContext = g,
                } )
                .ToList();
        }

        private List<SummaryGroupResult> GetConnectionStatusSummaries( IEnumerable<SummaryFlatCount> flatCounts, SummaryCache cache )
        {
            return flatCounts.GroupBy( fc => fc.ConnectionStatusId )
                .Select( g => new SummaryGroupResult
                {
                    Id = g.Key,
                    Name = cache.ConnectionStatuses[g.Key],
                    Total = g.Sum( fc => fc.Count ),
                    CustomContext = g,
                } )
                .ToList();
        }

        private class SummaryFlatCount
        {
            public int ConnectionTypeId { get; set; }

            public int ConnectionOpportunityId { get; set; }

            public int? CampusId { get; set; }

            public int ConnectionStatusId { get; set; }

            public int Count { get; set; }
        }

        private class SummaryResult
        {
            /// <summary>
            /// The total number of items in the summary result.
            /// </summary>
            public int Total { get; set; }

            /// <summary>
            /// Child summary groups that further break down the summary results.
            /// </summary>
            public List<SummaryGroupResult> Groups { get; set; }

            /// <summary>
            /// The dimensions used to group the data in the summary results.
            /// The first item in the array represents the primary grouping,
            /// the second item represents the secondary grouping, and so on.
            /// </summary>
            public List<string> GroupingDimensions { get; set; }
        }
        private class SummaryGroupResult
        {
            /// <summary>
            /// The entity id. This will not be show in the JSON output.
            /// </summary>
            [JsonIgnore]
            internal int? Id { get; set; }

            /// <summary>
            /// Internal identifier of the item.
            /// </summary>
            public string IdKey => Id?.AsIdKey();

            /// <summary>
            /// The name of the item at this level.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The total number of items at this level of the summary, including
            /// all child groups.
            /// </summary>
            public int Total { get; set; }

            /// <summary>
            /// Child summary groups that further break down the summary results.
            /// </summary>
            public List<SummaryGroupResult> Groups { get; set; }

            [JsonIgnore]
            public object CustomContext { get; set; }
        }

        #endregion
    }
}

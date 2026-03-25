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
    internal partial class AttendanceSkill
    {
        #region Tool(s)

        [Description( "Returns a summary of attendance records." )]
        [AgentPurpose( "Retrieves summary of attendance records." )]
        [AgentToolGuid( "cb7ce9aa-93eb-4d42-af86-1170582b8bb1" )]
        public IAgentToolResult GetAttendanceSummary(
            string personIdKey = null,
            string checkInConfigurationIdKey = null,
            [Description( "Areas are group types in the database, but they are referred to as areas in the UI." )]
            string areaIdKey = null,
            string groupIdKey = null,
            string campusIdKey = null,
            string locationIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,

            [Description( "ONLY set this value if the user implies a breakdown by a specific dimension. Must be blank or exactly one of these values (do not infer additional values): CheckInConfiguration, Area, Group, Campus, Location" )]
            string primaryDimension = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var dimensions = new List<string> { "CheckInConfiguration", "Area", "Group", "Campus", "Location" };

            if ( CampusCache.All( AgentRequestContext.RockContext ).Count( c => c.IsActive == true ) == 1 )
            {
                if ( "Campus".Equals( primaryDimension, StringComparison.OrdinalIgnoreCase ) )
                {
                    return Error( "There is only one active campus, so the Campus dimension will be removed from the results. Please retry without setting a primary dimension." );
                }

                dimensions.Remove( "Campus" );
            }

            var query = new AttendanceService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( a => a.DidAttend == true );

            query = helper.WhereOptionalIdKey( query, a => a.PersonAlias.PersonId, personIdKey );
            query = helper.WhereOptionalIdKey( query, a => a.Occurrence.RootGroupTypeId, checkInConfigurationIdKey );
            query = helper.WhereOptionalIdKey( query, a => a.Occurrence.Group.GroupTypeId, areaIdKey );
            query = helper.WhereOptionalIdKey( query, a => a.Occurrence.GroupId, groupIdKey );
            query = helper.WhereOptionalIdKey( query, a => a.CampusId, campusIdKey );
            query = helper.WhereOptionalIdKey( query, a => a.Occurrence.LocationId, locationIdKey );
            query = helper.WhereOptionalPropertyBetween( query, a => a.StartDateTime, startDate, endDate );

            helper.RequireAtLeastOneFilter( [
                personIdKey,
                checkInConfigurationIdKey,
                areaIdKey,
                groupIdKey,
                campusIdKey,
                startDate,
                endDate
            ] );

            helper.SetPrimaryDimension( primaryDimension, dimensions );

            // If we are grouping by area, then it doesn't make sense to still
            // group by configuration.
            if ( "Area".Equals( primaryDimension, StringComparison.OrdinalIgnoreCase ) )
            {
                dimensions.Remove( "CheckInConfiguration" );
            }

            // If we are grouping by group, then it doesn't make sense to still
            // group by configuration or area.
            if ( "Area".Equals( primaryDimension, StringComparison.OrdinalIgnoreCase ) )
            {
                dimensions.Remove( "CheckInConfiguration" );
                dimensions.Remove( "Area" );
            }

            // Remove any dimensions that have already been satisfied by
            // filter options.
            helper.RemoveSatisfiedDimensions( checkInConfigurationIdKey, dimensions, ["CheckInConfiguration"] );
            helper.RemoveSatisfiedDimensions( areaIdKey, dimensions, ["Area", "CheckInConfiguration"] );
            helper.RemoveSatisfiedDimensions( groupIdKey, dimensions, ["Group", "Area", "CheckInConfiguration"] );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // Perform the SQL level grouping of all data into a set of rows
            // that contain the count of each possible combination of grouped
            // values.
            var groupCounts = query
                .GroupBy( a => new
                {
                    a.Occurrence.RootGroupTypeId,
                    a.Occurrence.Group.GroupTypeId,
                    a.Occurrence.GroupId,
                    a.CampusId,
                    a.Occurrence.LocationId,
                } )
                .Select( a => new SummaryGroupCount
                {
                    CheckInConfigurationId = a.Key.RootGroupTypeId,
                    AreaId = a.Key.GroupTypeId,
                    GroupId = a.Key.GroupId,
                    CampusId = a.Key.CampusId,
                    LocationId = a.Key.LocationId,
                    Count = a.Count(),
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
                    case "CheckInConfiguration":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.CheckInConfigurationId, state.CheckInConfigurations );
                        break;

                    case "Area":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.AreaId, state.Areas );
                        break;

                    case "Group":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.GroupId, state.Groups );
                        break;

                    case "Campus":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.CampusId, state.Campuses );
                        break;

                    case "Location":
                        groups = helper.BuildDimension( groups, groupCounts, c => c.LocationId, state.Locations );
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
            var checkInConfigurationIds = groupCounts
                .Where( gc => gc.CheckInConfigurationId.HasValue )
                .Select( gc => gc.CheckInConfigurationId.Value )
                .Distinct()
                .ToList();
            var areaIds = groupCounts
                .Where( gc => gc.AreaId.HasValue )
                .Select( gc => gc.AreaId.Value )
                .Distinct()
                .ToList();
            var groupIds = groupCounts
                .Where( gc => gc.GroupId.HasValue )
                .Select( gc => gc.GroupId.Value )
                .Distinct()
                .ToList();
            var locationIds = groupCounts
                .Where( gc => gc.LocationId.HasValue )
                .Select( gc => gc.LocationId.Value )
                .Distinct()
                .ToList();

            return new SummaryState
            {
                CheckInConfigurations = GroupTypeCache.GetMany( checkInConfigurationIds, AgentRequestContext.RockContext )
                    .ToDictionary( gt => gt.Id, gt => gt.Name ),
                Areas = GroupTypeCache.GetMany( areaIds, AgentRequestContext.RockContext )
                    .ToDictionary( gt => gt.Id, gt => gt.Name ),
                Groups = GroupCache.GetMany( groupIds, AgentRequestContext.RockContext )
                    .ToDictionary( g => g.Id, g => g.Name ),
                Campuses = CampusCache.All( AgentRequestContext.RockContext ).ToDictionary( c => c.Id, c => c.Name ),
                Locations = NamedLocationCache.GetMany( locationIds, AgentRequestContext.RockContext )
                    .ToDictionary( l => l.Id, l => l.Name ),
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
            public int? CheckInConfigurationId { get; set; }

            public int? AreaId { get; set; }

            public int? GroupId { get; set; }

            public int? CampusId { get; set; }

            public int? LocationId { get; set; }

            public int Count { get; set; }
        }

        /// <summary>
        /// The state object that contains all the cached information required
        /// to build the summary results. This includes the grouped row counts
        /// from SQL and any cached name lookups.
        /// </summary>
        private class SummaryState
        {
            public Dictionary<int, string> CheckInConfigurations { get; set; }

            public Dictionary<int, string> Areas { get; set; }

            public Dictionary<int, string> Groups { get; set; }

            public Dictionary<int, string> Campuses { get; set; }

            public Dictionary<int, string> Locations { get; set; }
        }

        #endregion
    }
}

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

using Rock.AI.Agent.Annotations;
using Rock.Enums.Geography;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Utility.GroupFinder;

namespace Rock.AI.Agent.Skills;

internal partial class GroupSkill
{
    #region Tool(s)

    /// <summary>
    /// The largest number of results <see cref="FindNearbyGroupsForMe"/> will
    /// return.
    /// </summary>
    private const int FindNearbyGroupsForMeMaxResults = 10;

    [Description( "Finds groups near the current person, filtered by meeting day, meeting time, and meeting style. Results are ordered by distance from the person's home." )]
    [AgentPurpose( "Searches the configured group finder group types for public groups near the current person." )]
    [AgentToolGuid( "c7cec6bc-7309-426b-8409-24236adfa841" )]
    public AgentToolResult FindNearbyGroupsForMe(
        [Description( "Limits the search to a single group type. Must be one of the group types configured for the group finder tools." )]
        string groupTypeIdKey = null,

        MeetingStyle? meetingStyle = null,

        [Description( "A comma separated list of day names to include, e.g. 'Monday,Wednesday'." )]
        string daysOfWeek = null,

        string earliestTime = null,

        string latestTime = null,

        double? maxDistanceMiles = null,

        [Description( "When provided, adds travel distance and time and orders results by travel distance. Calls an external mapping service." )]
        TravelMode? travelMode = null,

        int maxResults = 10 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        // This tool always searches for the current person, so there must be one.
        if ( currentPerson == null )
        {
            return Error( "This tool needs a signed-in person to search for. There is no current person on this request." );
        }

        var finderGroupTypeIds = GetFinderGroupTypeIds();

        if ( finderGroupTypeIds.Count == 0 )
        {
            return Error( "The Group Finder tool is not configured correctly. No group types have been selected for it to search." );
        }

        // Narrow to a single group type when requested, validating it is one of
        // the configured finder group types.
        var groupTypeIds = finderGroupTypeIds;
        if ( groupTypeIdKey.IsNotNullOrWhiteSpace() )
        {
            var groupTypeId = IdHasher.Instance.GetId( groupTypeIdKey );

            if ( !groupTypeId.HasValue || !finderGroupTypeIds.Contains( groupTypeId.Value ) )
            {
                helper.AddError( "The specified group type is not available for the group finder." );
            }
            else
            {
                groupTypeIds = new List<int> { groupTypeId.Value };
            }
        }

        ValidateMeetingTimes( helper, earliestTime, latestTime );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The origin is always the current person's mapped address.
        var originString = currentPerson.Id.ToString();
        var originPoint = ResolveOriginPoint( originString, currentPerson );

        if ( originPoint == null )
        {
            return Error( "We could not determine your location because you do not have a mapped address on file." );
        }

        var cappedMaxResults = Math.Min( Math.Max( maxResults, 1 ), FindNearbyGroupsForMeMaxResults );

        var options = new GroupFinderOptions
        {
            GroupTypeIds = groupTypeIds,
            Include = "Group.Schedule",
            MaxResults = cappedMaxResults,
            MaxDistance = MilesToMeters( maxDistanceMiles ),
            ReturnOnlyClosestLocationPerGroup = true,
            Origin = originString,
            OriginPoint = originPoint,
            TravelMode = travelMode,
            HideOvercapacityGroups = true,
            EnableStrictCampusFiltering = false,
            EnablePublicFilter = true
        };

        // Campus comes from the current person's primary campus, when they have one.
        var filters = BuildGroupFinderFilters( currentPerson.PrimaryCampusId, meetingStyle, daysOfWeek, earliestTime, latestTime );
        var results = ExecuteGroupFinderSearch( options, filters, out var travelWarning );

        return BuildFinderResult( results, travelWarning, maxResults, FindNearbyGroupsForMeMaxResults );
    }

    #endregion
}

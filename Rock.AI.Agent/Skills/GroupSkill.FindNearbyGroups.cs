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
using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.Enums.AI.Agent;
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
    /// The largest number of results <see cref="FindNearbyGroups"/> will return.
    /// </summary>
    private const int FindNearbyGroupsMaxResults = 25;

    [Description( "Finds groups near a location, filtered by campus, meeting day, meeting time, and meeting style. Results are ordered by distance from the location. For staff use." )]
    [AgentPurpose( "Searches the configured group finder group types for groups near a location or person." )]
    [AgentToolGuid( "22b07a58-396e-4901-be1a-2571053319f8" )]
    public AgentToolResult FindNearbyGroups(
        [Description( "The person to find groups for. When provided and no origin is given, the person's mapped address is used as the search origin." )]
        string personIdKey = null,

        [Description( "The location to search from. Accepts a postal code (e.g. 'postalcode 85383'), a street address, cross streets, a city and state, a named place, or a latitude/longitude. Takes precedence over personIdKey when both are provided." )]
        string origin = null,

        [Description( "Limits the search to a single group type. Must be one of the group types configured for the group finder tools." )]
        string groupTypeIdKey = null,

        string campusIdKey = null,

        MeetingStyle? meetingStyle = null,

        [Description( "A comma separated list of day names to include, e.g. 'Monday,Wednesday'." )]
        string daysOfWeek = null,

        string earliestTime = null,

        string latestTime = null,

        double? maxDistanceMiles = null,

        [Description( "When provided, adds travel distance and time and orders results by travel distance. Calls an external mapping service." )]
        TravelMode? travelMode = null,

        int maxResults = 10,

        bool includeGroupsOverCapacity = false,

        bool includeNonPublicGroups = false )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        // This tool can look up groups on behalf of any person, so it is not
        // appropriate for a public-facing agent. Point the administrator at a
        // public-appropriate alternative without naming a specific tool, since
        // one may not be configured.
        if ( AgentRequestContext.AudienceType != AudienceType.Internal )
        {
            return Error( "This tool can't be used by a public-facing agent because it can look up groups on behalf of any person. A different group finder tool intended for public use may need to be enabled for this agent." );
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
                groupTypeIds = new System.Collections.Generic.List<int> { groupTypeId.Value };
            }
        }

        // Resolve the optional target person and campus. Any bad values add
        // their own errors to the helper.
        var person = personIdKey.IsNotNullOrWhiteSpace() ? helper.GetOptionalEntity<Rock.Model.Person>( personIdKey ) : null;
        var campus = campusIdKey.IsNotNullOrWhiteSpace() ? helper.GetOptionalEntity<Campus>( campusIdKey ) : null;

        // The origin is required. An explicit origin wins; otherwise fall back to
        // the target person's mapped address. There is no fallback to the current
        // person for this tool.
        var originString = origin;
        if ( originString.IsNullOrWhiteSpace() && person != null )
        {
            originString = person.Id.ToString();
        }

        if ( originString.IsNullOrWhiteSpace() )
        {
            helper.AddError( "You must provide either an origin or a personIdKey to search from." );
        }

        ValidateMeetingTimes( helper, earliestTime, latestTime );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var originPoint = ResolveOriginPoint( originString, null );

        if ( originPoint == null )
        {
            return Error( "A location could not be determined from the provided origin or person." );
        }

        var cappedMaxResults = Math.Min( Math.Max( maxResults, 1 ), FindNearbyGroupsMaxResults );

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
            HideOvercapacityGroups = !includeGroupsOverCapacity,
            EnableStrictCampusFiltering = false,
            EnablePublicFilter = !includeNonPublicGroups
        };

        var filters = BuildGroupFinderFilters( campus?.Id, meetingStyle, daysOfWeek, earliestTime, latestTime );
        var results = ExecuteGroupFinderSearch( options, filters, out var travelWarning );

        return BuildFinderResult( results, travelWarning, maxResults, FindNearbyGroupsMaxResults );
    }

    #endregion
}

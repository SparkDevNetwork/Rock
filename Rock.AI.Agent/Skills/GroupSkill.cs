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

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.GroupSkill;
using Rock.Attribute;
using Rock.Core.Geography.Classes;
using Rock.Enums.AI.Agent;
using Rock.Lava.Filters.Internal;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility.GroupFinder;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to group related data.
/// </summary>

[Description( "This skill provides access to group related data." )]
[AgentPurpose( "Provides access to groups and group membership for people." )]

[GroupTypesField( "Group Types",
        Description = "The group types that will be managed by this skill. If none are selected then all group types will be available.",
        IsRequired = false,
        EnhancedSelection = true,
        Key = ConfigurationKey.GroupTypes,
        Order = 0 )]

[GroupTypesField( "Group Finder Group Types",
        Description = "The group types that the group finder tools (FindNearbyGroups and FindNearbyGroupsForMe) will search. Unlike the general Group Types setting, these tools return an error when none are selected.",
        IsRequired = false,
        EnhancedSelection = true,
        Key = ConfigurationKey.FinderGroupTypes,
        Order = 1 )]

[AgentSkillGuid( "fa40a5e9-df52-4645-b3ed-cf9bbf79b12f" )]
[EntityTypeGuid( "ec39756f-44ba-4000-bb75-4335ddc95bc6" )]
internal sealed partial class GroupSkill : AgentSkillComponent
{
    #region Keys

    private static class ConfigurationKey
    {
        public const string GroupTypes = "GroupTypes";

        public const string FinderGroupTypes = "FinderGroupTypes";
    }

    #endregion

    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Group Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public GroupSkill( ILogger<GroupSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Methods

    private IEnumerable<GroupTypeCache> GetAvailableGroupTypes()
    {
        var groupTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.GroupTypes, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        if ( groupTypeGuids.Count == 0 )
        {
            return GroupTypeCache.All( AgentRequestContext.RockContext )
                .Where( gt => gt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
        }

        return GroupTypeCache.GetMany( groupTypeGuids, AgentRequestContext.RockContext )
            .Where( gt => gt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
    }

    /// <summary>
    /// Determines whether the specified group type can be made available to
    /// the agent for this request.
    /// </summary>
    /// <param name="groupTypeId">The ID of the group type.</param>
    /// <param name="helper">The agent tool helper, an error message will be added if <c>true</c> is returned.</param>
    /// <returns>True if the group type can be configured for the request; otherwise, false.</returns>
    private bool CanGroupTypeBeConfiguredForRequest( int groupTypeId, AgentToolHelper helper )
    {
        // Only let people know about additional group types if this is an
        // agent meant for internal use.
        if ( AgentRequestContext.AudienceType != AudienceType.Internal )
        {
            return false;
        }

        var groupType = GroupTypeCache.Get( groupTypeId, AgentRequestContext.RockContext );

        var canBeConfigured = groupType != null
            && groupType.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson );

        if ( canBeConfigured )
        {
            helper.AddError( $"The group type '{groupType.Name}' is not available, but it can be configured on this agent by your administrator." );
        }

        return canBeConfigured;
    }

    #endregion

    #region Group Finder Shared

    /*
        9/15/26 - CLAUDE

        FindNearbyGroups and FindNearbyGroupsForMe are two adapters over the same
        Rock.Utility.GroupFinder.GroupFinderHelper that powers the Group Finder
        Lava shortcode. The shared code below builds the filter list, runs the
        proximity search, and projects the results so both tools stay in sync.

        Reason: The two finder tools differ only in audience and a few inputs.
    */

    /// <summary>
    /// The number of miles in a single meter, used to convert the helper's
    /// meter-based distances into the miles the tools report.
    /// </summary>
    private const double MilesPerMeter = 0.000621371;

    /// <summary>
    /// Resolves the origin point for a search, returning <c>null</c> when it
    /// cannot be determined.
    /// </summary>
    /// <remarks>
    /// Geocoding an address or place requires a configured Google API key. When
    /// that key is missing <see cref="GroupFinderHelper.GetOriginPoint"/> throws,
    /// which is treated the same as an origin that could not be resolved so the
    /// caller can return its usual "location could not be determined" error
    /// instead of surfacing an exception.
    /// </remarks>
    /// <param name="originString">The origin string to resolve (address, postal code, lat/long, or person id).</param>
    /// <param name="currentPerson">The person to fall back to when the origin string is blank, or <c>null</c>.</param>
    /// <returns>The resolved origin point, or <c>null</c> when it could not be determined.</returns>
    private GeographyPoint ResolveOriginPoint( string originString, Rock.Model.Person currentPerson )
    {
        try
        {
            return new GroupFinderHelper( AgentRequestContext.RockContext ).GetOriginPoint( originString, currentPerson );
        }
        catch ( Exception ex )
        {
            _logger.LogWarning( ex, "Unable to resolve the group finder origin point." );

            return null;
        }
    }

    /// <summary>
    /// Gets the group type IDs the group finder tools are configured to search.
    /// This uses a dedicated configuration setting, separate from the general
    /// <c>GroupTypes</c> setting the other tools use, and is empty when nothing
    /// has been selected.
    /// </summary>
    /// <returns>The authorized group type IDs the finder tools may search.</returns>
    private List<int> GetFinderGroupTypeIds()
    {
        var groupTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.FinderGroupTypes, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        if ( groupTypeGuids.Count == 0 )
        {
            return new List<int>();
        }

        return GroupTypeCache.GetMany( groupTypeGuids, AgentRequestContext.RockContext )
            .Where( gt => gt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( gt => gt.Id )
            .ToList();
    }

    /// <summary>
    /// Builds the list of <see cref="GroupFinderFilter"/> values that both finder
    /// tools apply. Only the criteria that were actually provided produce a filter.
    /// </summary>
    /// <param name="campusId">The campus to filter on, or <c>null</c> for no campus filter.</param>
    /// <param name="meetingStyle">The meeting style to filter on, or <c>null</c> for no meeting-style filter.</param>
    /// <param name="daysOfWeek">A comma separated list of day names to filter on, or <c>null</c>/blank for no day filter.</param>
    /// <param name="earliestTime">The earliest meeting time (inclusive) to filter on, or <c>null</c>/blank for no lower bound.</param>
    /// <param name="latestTime">The latest meeting time (inclusive) to filter on, or <c>null</c>/blank for no upper bound.</param>
    /// <returns>The filters to pass to <see cref="GroupFinderHelper.ApplyFilters"/>.</returns>
    private List<GroupFinderFilter> BuildGroupFinderFilters( int? campusId, MeetingStyle? meetingStyle, string daysOfWeek, string earliestTime, string latestTime )
    {
        var filters = new List<GroupFinderFilter>();

        if ( campusId.HasValue )
        {
            filters.Add( new GroupFinderFilter( "campus", null, null, campusId.Value.ToString() ) );
        }

        if ( meetingStyle.HasValue )
        {
            filters.Add( new GroupFinderFilter( "meetingstyle", null, null, meetingStyle.Value.ToString() ) );
        }

        if ( daysOfWeek.IsNotNullOrWhiteSpace() )
        {
            filters.Add( new GroupFinderFilter( "dayofweek", null, null, daysOfWeek ) );
        }

        if ( earliestTime.IsNotNullOrWhiteSpace() )
        {
            filters.Add( new GroupFinderFilter( "timeofday", null, "gte", earliestTime ) );
        }

        if ( latestTime.IsNotNullOrWhiteSpace() )
        {
            filters.Add( new GroupFinderFilter( "timeofday", null, "lte", latestTime ) );
        }

        return filters;
    }

    /// <summary>
    /// Runs the proximity search using the fully prepared options and filters,
    /// then projects the matches into the trimmed result shape the tools return.
    /// Travel mode details are appended when a travel mode was requested; a
    /// failure there is reported through <paramref name="travelWarning"/> rather
    /// than losing the whole result.
    /// </summary>
    /// <param name="options">The prepared group finder options. The origin point must already be resolved.</param>
    /// <param name="filters">The filters to apply.</param>
    /// <param name="travelWarning">On exit, a message describing why travel details could not be calculated, or <c>null</c> when none was requested or it succeeded.</param>
    /// <returns>The matching groups ordered by distance from the origin.</returns>
    private List<NearbyGroupResult> ExecuteGroupFinderSearch( GroupFinderOptions options, List<GroupFinderFilter> filters, out string travelWarning )
    {
        travelWarning = null;

        var rockContext = AgentRequestContext.RockContext;
        var finderHelper = new GroupFinderHelper( rockContext );

        var groupQuery = finderHelper.GetGroupLocationQueryable( options );
        groupQuery = finderHelper.ApplyFilters( groupQuery, options, filters );

        var sourcePoint = options.OriginPoint.ToDatabase();

        var results = groupQuery
            .Select( x => new GroupProximityResult
            {
                StraightLineDistanceInMeters = x.Location.GeoPoint.Distance( sourcePoint ),
                Group = x.Group,
                Location = x.Location
            } )
            .OrderBy( x => x.StraightLineDistanceInMeters )
            .Take( options.MaxResults )
            .ToList();

        if ( options.TravelMode.HasValue && results.Count > 0 )
        {
            try
            {
                results = finderHelper.AppendTravelModeDetails( options.OriginPoint, options.TravelMode.Value, results );
            }
            catch ( Exception ex )
            {
                // The travel matrix is an external Google Routes call. When it
                // fails we still have useful straight-line results, so surface a
                // warning instead of failing the whole search.
                _logger.LogWarning( ex, "Unable to calculate group finder travel distances." );

                travelWarning = "Travel distance and time could not be calculated, so results are ordered by straight-line distance only.";
            }
        }

        return results.Select( r => ToNearbyGroupResult( r, rockContext ) ).ToList();
    }

    /// <summary>
    /// Projects a single proximity match into the trimmed result shape.
    /// </summary>
    /// <param name="proximityResult">The proximity match to project.</param>
    /// <param name="rockContext">The context used to resolve cached lookups.</param>
    /// <returns>The projected result.</returns>
    private NearbyGroupResult ToNearbyGroupResult( GroupProximityResult proximityResult, Data.RockContext rockContext )
    {
        var group = proximityResult.Group;
        var campus = group.CampusId.HasValue ? CampusCache.Get( group.CampusId.Value, rockContext ) : null;
        var groupType = GroupTypeCache.Get( group.GroupTypeId, rockContext );

        var meetingTime = group.Schedule?.WeeklyTimeOfDay.HasValue == true
            ? RockDateTime.Today.Add( group.Schedule.WeeklyTimeOfDay.Value ).ToString( "h:mm tt" )
            : null;

        return new NearbyGroupResult
        {
            Id = group.Id,
            Guid = group.Guid,
            Name = group.Name,
            GroupType = KeyNameResultOrNull( groupType ),
            Campus = KeyNameResultOrNull( campus ),
            MeetingStyle = group.MeetingStyle,
            MeetingDay = group.Schedule?.WeeklyDayOfWeek,
            MeetingTime = meetingTime,
            Address = proximityResult.Location.GetFullStreetAddress().Trim(),
            StraightLineDistanceInMiles = MetersToMiles( proximityResult.StraightLineDistanceInMeters ),
            TravelDistanceInMiles = MetersToMiles( proximityResult.TravelDistanceInMeters ),
            TravelTimeInMinutes = proximityResult.TravelTimeInMinutes,
            TravelMode = proximityResult.TravelMode
        };
    }

    /// <summary>
    /// Converts a distance in meters to miles, rounded to one decimal place.
    /// </summary>
    /// <param name="meters">The distance in meters, or <c>null</c>.</param>
    /// <returns>The distance in miles, or <c>null</c> when <paramref name="meters"/> was <c>null</c>.</returns>
    private static double? MetersToMiles( double? meters )
    {
        if ( !meters.HasValue )
        {
            return null;
        }

        return Math.Round( meters.Value * MilesPerMeter, 1 );
    }

    /// <summary>
    /// Builds a <see cref="Classes.Common.KeyNameResult"/> from a cached entity,
    /// returning <c>null</c> when the entity is <c>null</c>.
    /// </summary>
    /// <param name="cache">The cached entity, or <c>null</c>.</param>
    /// <returns>The result, or <c>null</c>.</returns>
    private static Classes.Common.KeyNameResult KeyNameResultOrNull( IEntityCache cache )
    {
        return cache != null ? Classes.Common.KeyNameResult.FromCache( cache ) : null;
    }

    /// <summary>
    /// Converts a distance in miles to a whole number of meters for use with
    /// <see cref="GroupFinderOptions.MaxDistance"/>.
    /// </summary>
    /// <param name="miles">The distance in miles, or <c>null</c>.</param>
    /// <returns>The distance in meters, or <c>null</c> when <paramref name="miles"/> was <c>null</c>.</returns>
    private static int? MilesToMeters( double? miles )
    {
        if ( !miles.HasValue )
        {
            return null;
        }

        return ( int ) Math.Round( miles.Value / MilesPerMeter );
    }

    /// <summary>
    /// Validates the optional earliest and latest meeting time inputs, adding an
    /// error to <paramref name="helper"/> for any value that cannot be parsed as
    /// a time.
    /// </summary>
    /// <param name="helper">The helper that collects any errors.</param>
    /// <param name="earliestTime">The earliest meeting time input, or <c>null</c>/blank.</param>
    /// <param name="latestTime">The latest meeting time input, or <c>null</c>/blank.</param>
    private static void ValidateMeetingTimes( AgentToolHelper helper, string earliestTime, string latestTime )
    {
        if ( earliestTime.IsNotNullOrWhiteSpace() && !DateTime.TryParse( earliestTime, out _ ) )
        {
            helper.AddError( $"The earliestTime value '{earliestTime}' is not a valid time." );
        }

        if ( latestTime.IsNotNullOrWhiteSpace() && !DateTime.TryParse( latestTime, out _ ) )
        {
            helper.AddError( $"The latestTime value '{latestTime}' is not a valid time." );
        }
    }

    /// <summary>
    /// Builds the final tool result from a set of matched groups, attaching any
    /// travel warning and a note when the requested result count was capped.
    /// </summary>
    /// <param name="results">The matched groups.</param>
    /// <param name="travelWarning">A travel calculation warning, or <c>null</c>.</param>
    /// <param name="requestedMaxResults">The number of results the caller asked for.</param>
    /// <param name="maxResultsCap">The hard cap that was applied.</param>
    /// <returns>The tool result.</returns>
    private AgentToolResult BuildFinderResult( List<NearbyGroupResult> results, string travelWarning, int requestedMaxResults, int maxResultsCap )
    {
        if ( results.Count == 0 )
        {
            return NoData();
        }

        var result = Success( results );

        if ( travelWarning.IsNotNullOrWhiteSpace() )
        {
            result.WithInstructions( travelWarning );
        }

        if ( requestedMaxResults > maxResultsCap )
        {
            result.WithInstructions( $"maxResults was capped at {maxResultsCap}." );
        }

        return result;
    }

    #endregion
}

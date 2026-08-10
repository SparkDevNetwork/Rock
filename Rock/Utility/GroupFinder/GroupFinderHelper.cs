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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Core.Geography;
using Rock.Core.Geography.Classes;
using Rock.Data;
using Rock.Enums.Geography;
using Rock.Lava.Filters.Internal;
using Rock.Model;

namespace Rock.Utility.GroupFinder
{
    /// <summary>
    /// Helper class for finding groups based on various criteria. This was
    /// originally used by just the Lava shortcode, but is now also being used
    /// by AI tooling. This is why some of the logic is a bit more complex than
    /// you might expect, because it was originally designed to be drived by
    /// Lava parameters rather than structured code.
    /// </summary>
    /// <remarks>
    /// Exposed as a RockInternal surface so the Group Finder block and the group
    /// finder Lava shortcode share one filtering path. The namespace is not ideal
    /// (Rock.Group would conflict with the Group model name); relocating this to a
    /// GroupFinderService is a possible later cleanup.
    /// </remarks>
    [RockInternal( "20.0" )]
    public class GroupFinderHelper
    {
        private readonly RockContext _rockContext;

        public GroupFinderHelper( RockContext rockContext )
        {
            _rockContext = rockContext;
        }

        /// <summary>
        /// Creates an IQueryable for the search based on the existence of an origin point
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IQueryable<GroupLocation> GetGroupLocationQueryable( GroupFinderOptions options )
        {
            // If we don't have an origin, or the person does not have a mapped location, then we'll just provide the filtered list of groups.
            if ( options.Origin.IsNotNullOrWhiteSpace() && options.OriginPoint != null )
            {
                return new GroupService( _rockContext )
                    .GetNearestGroups( options.OriginPoint, options.GroupTypeIds, options.ReturnOnlyClosestLocationPerGroup, options.MaxDistance )
                    .Include( options.Include );
            }
            else
            {
                return new GroupLocationService( _rockContext ).Queryable()
                    .Where( gl => options.GroupTypeIds.Contains( gl.Group.GroupTypeId ) )
                    .Include( options.Include );
            }
        }

        #region Filter Logic

        /// <summary>
        /// Applies the filters to the search
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="options"></param>
        /// <param name="childElements"></param>
        /// <returns></returns>
        public IQueryable<GroupLocation> ApplyFilters( IQueryable<GroupLocation> groupQuery, GroupFinderOptions options, List<GroupFinderFilter> childElements )
        {
            groupQuery = ApplyFilterGroupOvercapacity( groupQuery, options );

            // Filter out inactive groups
            groupQuery = groupQuery.Where( g => g.Group.IsActive == true );

            // Filter out non-public groups
            if ( options.EnablePublicFilter == true )
            {
                groupQuery = groupQuery.Where( g => g.Group.IsPublic == true );
            }

            // Process each of the settings they provided in the child elements.
            foreach ( var setting in childElements )
            {
                var value = setting.Content;

                switch ( setting.Type )
                {
                    // Campus(es)
                    case "campus":
                        {
                            groupQuery = ApplyFilterCampus( groupQuery, setting, options );
                            break;
                        }
                    // Attributes
                    case "attribute":
                        {
                            groupQuery = ApplyFilterAttributes( groupQuery, setting );
                            break;
                        }
                    // Day of week
                    case "dayofweek":
                        {
                            groupQuery = ApplyFilterDayOfWeek( groupQuery, setting );
                            break;
                        }
                    // Time of day
                    case "timeofday":
                        {
                            groupQuery = ApplyFilterTimeOfDay( groupQuery, setting );
                            break;
                        }
                    // Meeting style (in-person / online / hybrid)
                    case "meetingstyle":
                        {
                            groupQuery = ApplyFilterMeetingStyle( groupQuery, setting );
                            break;
                        }
                }
            }

            return groupQuery;
        }

        /// <summary>
        /// Applies the meeting style filter to the group query.
        /// </summary>
        /// <param name="groupQuery">The group location query to filter.</param>
        /// <param name="setting">The filter whose <see cref="GroupFinderFilter.Content"/> is a comma separated list of <see cref="MeetingStyle"/> values.</param>
        /// <returns>The query filtered to groups whose meeting style is one of the specified values, or the original query when none are valid.</returns>
        private IQueryable<GroupLocation> ApplyFilterMeetingStyle( IQueryable<GroupLocation> groupQuery, GroupFinderFilter setting )
        {
            // Content is a comma separated list of MeetingStyle values (names or numeric values).
            var meetingStyles = setting.Content.SplitDelimitedValues().AsEnumList<MeetingStyle>();

            if ( !meetingStyles.Any() )
            {
                return groupQuery;
            }

            return groupQuery.Where( gl => gl.Group.MeetingStyle.HasValue && meetingStyles.Contains( gl.Group.MeetingStyle.Value ) );
        }

        /// <summary>
        /// Applies the campus filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterAttributes( IQueryable<GroupLocation> groupQuery, GroupFinderFilter setting )
        {
            var key = setting.Key;
            var value = setting.Content;

            // No key specified, return unfiltered
            if ( key.IsNullOrWhiteSpace() )
            {
                return groupQuery;
            }

            // Default the operator to 'eq' if not specified
            var filterOperator = setting.Operator.ToStringOrDefault( "eq" );

            switch ( filterOperator )
            {
                case "con":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value.Contains( value ) ) );
                    }
                case "sw":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value.StartsWith( value ) ) );
                    }
                case "ew":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value.EndsWith( value ) ) );
                    }
                case "in":
                    {
                        /*  Here we want to support an input value of 1,3 for an attribute with the key of "MultiValue'
                            matching to an attribute values of 1,2,3,4,5

                            To do this we'll create an expression tree for our query like:

                            { gl => 
                                !gl.Group.GroupAttributeValues.Any(a => a.Key == "MultiValue") ||
                                gl.Group.GroupAttributeValues.Any(a => 
                                    a.Key == "MultiValue" &&
                                    (
                                        a.Value == "1" ||
                                        a.Value.StartsWith("1,") ||
                                        a.Value.EndsWith(",1") ||
                                        a.Value.Contains(",1,") ||
                                        a.Value == "3" ||
                                        a.Value.StartsWith("3,") ||
                                        a.Value.EndsWith(",3") ||
                                        a.Value.Contains(",3,")
                                    )
                                )
                            )}

                            Note this will return true if the attribute key does not exist. This allows us to search across
                            multiple group types that might have differing attributes.
                        */

                        // Split the input
                        var valueList = value
                                .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                                .ToList();

                        var aParam = Expression.Parameter( typeof( QueryableAttributeValue ), "a" );
                        var keyProperty = Expression.Property( aParam, "Key" );
                        var keyCheck = Expression.Equal( keyProperty, Expression.Constant( key ) );

                        // Build the OR conditions for a.Value
                        var valueProperty = Expression.Property( aParam, "Value" );
                        Expression valueConditions = null;

                        // Create a where expression that checks for each value in the list.
                        foreach ( var valueItem in valueList )
                        {
                            var eq = Expression.Equal( valueProperty, Expression.Constant( valueItem ) );
                            var starts = Expression.Call( valueProperty, nameof( string.StartsWith ), null, Expression.Constant( valueItem + "," ) );
                            var ends = Expression.Call( valueProperty, nameof( string.EndsWith ), null, Expression.Constant( "," + valueItem ) );
                            var contains = Expression.Call( valueProperty, nameof( string.Contains ), null, Expression.Constant( "," + valueItem + "," ) );

                            var orBlock = Expression.OrElse(
                                Expression.OrElse( eq, starts ),
                                Expression.OrElse( ends, contains )
                            );

                            valueConditions = valueConditions == null ? orBlock : Expression.OrElse( valueConditions, orBlock );
                        }

                        // a => a.Key == "MultiValue"
                        var keyOnlyLambda = Expression.Lambda<Func<QueryableAttributeValue, bool>>( keyCheck, aParam );

                        // a => a.Key == "MultiValue" && valueConditions
                        var keyAndValueConditions = Expression.AndAlso( keyCheck, valueConditions );
                        var keyAndValueLambda = Expression.Lambda<Func<QueryableAttributeValue, bool>>( keyAndValueConditions, aParam );

                        // gl.Group.GroupAttributeValues
                        var glParam = Expression.Parameter( typeof( GroupLocation ), "gl" );
                        var groupProperty = Expression.Property( glParam, "Group" );
                        var gavProperty = Expression.Property( groupProperty, "GroupAttributeValues" );

                        // !gl.Group.GroupAttributeValues.Any(a => a.Key == "MultiValue")
                        var anyKeyOnlyCall = Expression.Call(
                            typeof( Enumerable ),
                            "Any",
                            new[] { typeof( QueryableAttributeValue ) },
                            gavProperty,
                            keyOnlyLambda
                        );
                        var notAnyKeyOnly = Expression.Not( anyKeyOnlyCall );

                        // gl.Group.GroupAttributeValues.Any(a => a.Key == "MultiValue" && valueConditions)
                        var anyKeyAndValueCall = Expression.Call(
                            typeof( Enumerable ),
                            "Any",
                            new[] { typeof( QueryableAttributeValue ) },
                            gavProperty,
                            keyAndValueLambda
                        );

                        // Combine: !Any(keyOnly) || Any(key && value)
                        var finalExpression = Expression.OrElse( notAnyKeyOnly, anyKeyAndValueCall );
                        var lambda = Expression.Lambda<Func<GroupLocation, bool>>( finalExpression, glParam );

                        // Apply to query
                        return groupQuery.Where( lambda );
                    }
                case "ne":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value != value ) );
                    }
                case "eq":
                    {
                        return groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( a => a.Key == key && a.Value == value ) );
                    }
                default:
                    {
                        throw new Exception( "Incorrect filter operator provided. Valid values are eq,ne,sw,ew,con,in." );
                    }
            }
        }

        /// <summary>
        /// Applies the campus filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterCampus( IQueryable<GroupLocation> groupQuery, GroupFinderFilter setting, GroupFinderOptions options )
        {
            var valueList = setting.Content
                    .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( v => int.Parse( v.Trim() ) )
                    .ToList();

            if ( options.EnableStrictCampusFiltering )
            {
                return groupQuery.Where( gl => valueList.Contains( gl.Group.CampusId.Value ) );
            }
            else
            {
                return groupQuery.Where( gl => gl.Group.CampusId == null || valueList.Contains( gl.Group.CampusId.Value ) );
            }
        }

        /// <summary>
        /// Applies the day of week filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterDayOfWeek( IQueryable<GroupLocation> groupQuery, GroupFinderFilter setting )
        {
            var daysOfWeek = setting.Content
                    .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( s => Enum.TryParse<DayOfWeek>( s.Trim(), true, out var d ) ? d : ( DayOfWeek? ) null )
                    .Where( d => d.HasValue )
                    .Select( d => d.Value )
                    .ToList();

            return groupQuery.Where( g => g.Group.Schedule.WeeklyDayOfWeek.HasValue && daysOfWeek.Contains( g.Group.Schedule.WeeklyDayOfWeek.Value ) );
        }

        /// <summary>
        /// Applies the time of day filter to the group query.
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterTimeOfDay( IQueryable<GroupLocation> groupQuery, GroupFinderFilter setting )
        {
            var time = DateTime.Parse( setting.Content );
            var timeSpan = time.TimeOfDay;

            // Default the operator to 'gte' if not specified
            var filterOperator = setting.Operator.ToStringOrDefault( "gte" );

            switch ( filterOperator )
            {
                case "lte":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value <= timeSpan );
                    }
                case "lt":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value < timeSpan );
                    }
                case "gt":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value > timeSpan );
                    }
                case "eq":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value == timeSpan );
                    }
                case "ne":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value != timeSpan );
                    }
                case "gte":
                    {
                        return groupQuery.Where( g => g.Group.Schedule.WeeklyTimeOfDay.HasValue && g.Group.Schedule.WeeklyTimeOfDay.Value >= timeSpan );
                    }
                default:
                    {
                        throw new Exception( "Incorrect filter operator provided. Valid values are eq,ne,lte,lt,gte,gt." );
                    }
            }
        }

        /// <summary>
        /// Applies the filters for group overcapacity
        /// </summary>
        /// <param name="groupQuery"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> ApplyFilterGroupOvercapacity( IQueryable<GroupLocation> groupQuery, GroupFinderOptions options )
        {
            // Hide overcapacity groups
            // This hides the groups that are at or over capacity by doing two things:
            // 1) If the group has a GroupCapacity, check that we haven't met or exceeded that.
            // 2) When someone registers for a group on the front-end website, they automatically get added with the group's default
            //    GroupTypeRole. If that role exists and has a MaxCount, check that we haven't met or exceeded it yet.
            if ( options.HideOvercapacityGroups )
            {
                groupQuery = groupQuery.Where(
                    g => g.Group.GroupCapacity == null ||
                    g.Group.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count() < g.Group.GroupCapacity );

                groupQuery = groupQuery.Where( g =>
                     g.Group.GroupType == null ||
                     g.Group.GroupType.DefaultGroupRole == null ||
                     g.Group.GroupType.DefaultGroupRole.MaxCount == null ||
                     g.Group.Members.Where( m => m.GroupRoleId == g.Group.GroupType.DefaultGroupRole.Id ).Count() < g.Group.GroupType.DefaultGroupRole.MaxCount );
            }

            return groupQuery;
        }

        #endregion

        /// <summary>
        /// Appends the travel time and distances to the results.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="travelMode"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public List<GroupProximityResult> AppendTravelModeDetails( GeographyPoint origin, TravelMode travelMode, List<GroupProximityResult> results )
        {
            // Get driving distances from location extensions
            var destinations = results
                .Where( r => r.Location?.Latitude != null && r.Location?.Longitude != null )
                .Select( r => new GeographyPoint { Latitude = r.Location.Latitude.Value, Longitude = r.Location.Longitude.Value } )
                .ToList();

            var travelDistances = Task.Run( () => GeographyHelpers.GetDrivingMatrixAsync( origin, destinations, travelMode ) ).Result;

            // Merge travel distances into group results
            foreach ( var travelDistance in travelDistances )
            {
                // Find matching group result
                var matches = results.Where( r => r.LocationPoint == travelDistance.DestinationPoint ).ToList();

                foreach ( var match in matches )
                {
                    match.TravelDistanceInMeters = travelDistance.DistanceInMeters;
                    match.TravelTimeInMinutes = travelDistance.TravelTimeInMinutes;
                    match.TravelMode = travelMode;
                }
            }

            return results.OrderBy( g => g.TravelDistanceInMeters ).ToList();
        }

        /// <summary>
        /// Gets the origin point from the settings. If the origin is not a lat/long, it will be geocoded.
        /// </summary>
        /// <param name="originString"></param>
        /// <param name="currentPerson">The person to use as the origin if the origin string is blank.</param>
        /// <returns></returns>
        public GeographyPoint GetOriginPoint( string originString, Person currentPerson )
        {
            originString = originString.Trim();

            // If blank then assume current person
            if ( originString.IsNullOrWhiteSpace() )
            {
                originString = currentPerson?.Id.ToString();
            }

            // Check if it's an int, if so this will be a person id and we'll use their mapped address
            if ( Int32.TryParse( originString, out int personId ) )
            {
                var personLocation = new PersonService( _rockContext ).GetGeopoints( personId )?.FirstOrDefault();

                if ( personLocation == null )
                {
                    return null;
                }
                return GeographyPoint.FromDatabase( personLocation );
            }

            // Check if it's a lat/long if so return it
            if ( GeographyPoint.TryParse( originString, out var point ) )
            {
                return point;
            }

            // To search by postal code the user will append postalcode to the front. This prevents zip codes
            // from being confused with a person id. So we need to remove this so as not to confuse
            // the Google maps API.
            if ( originString.StartsWith( "postalcode" ) )
            {
                originString = originString.Substring( 3 ).Trim();
            }

            // Otherwise, run it through the geocoder
            return Task.Run( () => ( GeographyHelpers.Geocode( originString ) ) ).Result;
        }
    }
}

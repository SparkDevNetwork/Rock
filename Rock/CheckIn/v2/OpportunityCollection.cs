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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Contains a set of check-in opportunities.
    /// </summary>
    internal class OpportunityCollection
    {
        #region Properties

        /// <summary>
        /// Gets or sets the ability levels available to select from.
        /// </summary>
        /// <value>The list of ability levels.</value>
        public List<AbilityLevelOpportunity> AbilityLevels { get; set; }

        /// <summary>
        /// Gets or sets the areas that are available for check-in.
        /// </summary>
        /// <value>The list of areas.</value>
        public List<AreaOpportunity> Areas { get; set; }

        /// <summary>
        /// Gets or sets the groups that are available for check-in.
        /// </summary>
        /// <value>The list of groups.</value>
        public List<GroupOpportunity> Groups { get; set; }

        /// <summary>
        /// Gets or sets the locations that are available for check-in.
        /// </summary>
        /// <value>The list of locations.</value>
        public List<LocationOpportunity> Locations { get; set; }

        /// <summary>
        /// Gets or sets the schedules that are available for check-in.
        /// </summary>
        /// <value>The list of schedules.</value>
        public List<ScheduleOpportunity> Schedules { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// <para>
        /// Creates the the check-in opportunities that represents all possible values
        /// for the kiosk or locations. 
        /// </para>
        /// <para>
        /// If you provide an array of locations they will be used, otherwise
        /// the locations of the kiosk will be used. If you provide a kiosk
        /// then it will be used to determine the current timestamp when
        /// checking if locations are open or not.
        /// </para>
        /// </summary>
        /// <param name="possibleAreas">The possible areas that are to be considered when generating the opportunities.</param>
        /// <param name="kiosk">The optional kiosk to use.</param>
        /// <param name="locations">The list of locations to use.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>An instance of <see cref="OpportunityCollection"/> that describes the available opportunities.</returns>
        /// <exception cref="System.ArgumentNullException">kiosk - Kiosk must be specified unless locations are specified.</exception>
        internal static OpportunityCollection Create( IReadOnlyCollection<GroupTypeCache> possibleAreas, DeviceCache kiosk, IReadOnlyCollection<NamedLocationCache> locations, RockContext rockContext )
        {
            if ( kiosk == null && locations == null )
            {
                throw new ArgumentNullException( nameof( kiosk ), "Kiosk must be specified unless locations are specified." );
            }

            // Get the primary campus for this kiosk.
            var kioskCampusId = kiosk?.GetCampusId();
            var kioskCampus = kioskCampusId.HasValue ? CampusCache.Get( kioskCampusId.Value, rockContext ) : null;

            // Get the current timestamp as well as today's date for filtering
            // in later logic.
            var now = kioskCampus?.CurrentDateTime ?? RockDateTime.Now;
            var today = now.Date;

            // Get all areas that don't have exclusions for today.
            var activeAreas = possibleAreas
                .Where( a => !a.GroupScheduleExclusions.Any( e => today >= e.Start && today <= e.End ) )
                .ToList();

            // Get all area identifiers as a HashSet for faster lookups.
            var activeAreaIds = new HashSet<int>( activeAreas.Select( a => a.Id ) );

            // Get the active locations to work with.
            if ( locations != null )
            {
                locations = locations
                    .Where( nlc => nlc.IsActive )
                    .ToList();
            }
            else
            {
                // Get all locations for the kiosk that are active.
                locations = kiosk.GetAllLocationIds()
                    .Select( id => NamedLocationCache.Get( id ) )
                    .Where( nlc => nlc != null && nlc.IsActive )
                    .ToList();
            }

            // Get all the group locations for these locations. This also
            // filters down to only groups in an active area.
            var groupLocations = locations
                .SelectMany( l => GroupLocationCache.AllForLocationId( l.Id ) )
                .DistinctBy( glc => glc.Id )
                .Where( glc => activeAreaIds.Contains( GroupCache.Get( glc.GroupId, rockContext )?.GroupTypeId ?? 0 ) )
                .ToList();

            // Get all the schedules that are active.
            var activeSchedules = groupLocations
                .SelectMany( gl => gl.ScheduleIds )
                .Distinct()
                .Select( id => NamedScheduleCache.Get( id, rockContext ) )
                .Where( s => s != null
                    && s.IsActive
                    && s.WasCheckInActive( now ) )
                .ToList();

            // Get just the schedule identifiers in a hash set for faster lookups.
            var activeScheduleIds = new HashSet<int>( activeSchedules.Select( s => s.Id ) );

            // Get just the group locations with active schedules.
            var activeGroupLocations = groupLocations
                .Where( gl => gl.ScheduleIds.Any( sid => activeScheduleIds.Contains( sid ) ) )
                .ToList();

            // Load all the counts for any locations that are still up for
            // consideration.
            var locationIdsForCount = activeGroupLocations
                .Select( gl => gl.LocationId )
                .Distinct()
                .ToList();
            var locationCounts = GetCurrentCountsForLocations( locationIdsForCount, now, rockContext );

            // Construct the initial opportunities collection.
            var opportunities = new OpportunityCollection
            {
                AbilityLevels = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_ABILITY_LEVEL_TYPE.AsGuid(), rockContext )
                    ?.DefinedValues
                    .Select( dv => new AbilityLevelOpportunity
                    {
                        Guid = dv.Guid,
                        Name = dv.Value
                    } )
                    .ToList() ?? new List<AbilityLevelOpportunity>(),
                Areas = activeAreas
                    .Select( a => new AreaOpportunity
                    {
                        Guid = a.Guid,
                        Name = a.Name
                    } )
                    .ToList(),
                Groups = new List<GroupOpportunity>(),
                Locations = new List<LocationOpportunity>(),
                Schedules = activeSchedules
                    .Select( s => new ScheduleOpportunity
                    {
                        Guid = s.Guid,
                        Name = s.Name
                    } )
                    .ToList()
            };

            var locationIdsOverCapacity = new HashSet<int>();

            // Add in all the locations to the opportunities.
            foreach ( var grp in activeGroupLocations.GroupBy( gl => gl.LocationId ) )
            {
                var location = NamedLocationCache.Get( grp.Key, rockContext );
                var locationScheduleIds = new HashSet<int>( grp.SelectMany( gl => gl.ScheduleIds ).Distinct() );
                var attendeeGuids = locationCounts.GetValueOrDefault( location.Guid, new HashSet<Guid>() );

                // Check if this room is at all valid. If it is over the firm
                // threshold then not even an override is allowed.
                var isThresholdExceeded = location.FirmRoomThreshold.HasValue
                    && attendeeGuids.Count > location.FirmRoomThreshold.Value;

                if ( isThresholdExceeded )
                {
                    locationIdsOverCapacity.Add( location.Id );

                    continue;
                }

                opportunities.Locations.Add( new LocationOpportunity
                {
                    Guid = location.Guid,
                    Name = location.Name,
                    CurrentCount = attendeeGuids.Count,
                    Capacity = location.SoftRoomThreshold,
                    CurrentPersonGuids = attendeeGuids,
                    ScheduleGuids = activeSchedules.Where( s => locationScheduleIds.Contains( s.Id ) ).Select( s => s.Guid ).ToList()
                } );
            }

            // Add in all the Groups to the opportunities.
            var activeGroupLocationsUnderCapacity = activeGroupLocations
                .Where( gl => !locationIdsOverCapacity.Contains( gl.LocationId ) );

            foreach ( var grp in activeGroupLocationsUnderCapacity.GroupBy( gl => gl.GroupId ) )
            {
                var group = GroupCache.Get( grp.Key, rockContext );
                var groupType = group?.GroupType;

                if ( groupType == null )
                {
                    continue;
                }

                opportunities.Groups.Add( new GroupOpportunity
                {
                    Guid = group.Guid,
                    Name = group.Name,
                    AbilityLevelGuid = null,
                    AreaGuid = groupType.Guid,
                    CheckInData = group.GetCheckInData( rockContext ),
                    CheckInAreaData = groupType.GetCheckInAreaData( rockContext ),
                    LocationGuids = grp.OrderBy( gl => gl.Order )
                        .Select( gl => NamedLocationCache.Get( gl.LocationId ) )
                        .Where( l => l != null )
                        .Select( l => l.Guid )
                        .ToList()
                } );
            }

            return opportunities;
        }

        /// <summary>
        /// Clones this instance. This creates an entirely new opportunities instance
        /// as well as new instances of every object it contains. The new opportunities
        /// can be modified at will without affecting the original. It seems
        /// like we are doing a lot, but this is insanely fast, clocking in at
        /// 6ns per call.
        /// </summary>
        /// <returns>A new instance of <see cref="OpportunityCollection"/>.</returns>
        public OpportunityCollection Clone()
        {
            var clonedOpportunities = new OpportunityCollection
            {
                AbilityLevels = AbilityLevels
                    .Select( al => new AbilityLevelOpportunity
                    {
                        Guid = al.Guid,
                        Name = al.Name
                    } )
                    .ToList(),
                Areas = Areas
                    .Select( a => new AreaOpportunity
                    {
                        Guid = a.Guid,
                        Name = a.Name
                    } )
                    .ToList(),
                Groups = Groups
                    .Select( g => new GroupOpportunity
                    {
                        Guid = g.Guid,
                        Name = g.Name,
                        AbilityLevelGuid = g.AbilityLevelGuid,
                        AreaGuid = g.AreaGuid,
                        CheckInData = g.CheckInData,
                        CheckInAreaData = g.CheckInAreaData,
                        LocationGuids = g.LocationGuids.ToList()
                    } )
                    .ToList(),
                Locations = Locations
                    .Select( l => new LocationOpportunity
                    {
                        Guid = l.Guid,
                        Name = l.Name,
                        CurrentCount = l.CurrentCount,
                        Capacity = l.Capacity,
                        CurrentPersonGuids = new HashSet<Guid>( l.CurrentPersonGuids ),
                        ScheduleGuids = l.ScheduleGuids.ToList()
                    } )
                    .ToList(),
                Schedules = Schedules
                    .Select( s => new ScheduleOpportunity
                    {
                        Guid = s.Guid,
                        Name = s.Name
                    } )
                    .ToList()
            };

            return clonedOpportunities;
        }

        /// <summary>
        /// Removes any opportunity items that are "empty". Meaning, if a group has
        /// no locations then it can't be available as a choice so it will be
        /// removed.
        /// </summary>
        public void RemoveEmptyOpportunities()
        {
            // We use .RemoveAll() instead of Where().ToList() because this is
            // a hot path in check-in. RemoveAll() is about 30% faster and it
            // also causes 0 allocations unlike Where().ToList().
            //
            // This is why you must call Clone() before calling this method if
            // you plan to use the original opportunities again.

            // Start at the "bottom" and work our way up. So first remove all
            // locations without schedules.
            var allScheduleGuids = new HashSet<Guid>( Schedules.Select( s => s.Guid ) );
            var allReferencedLocationGuids = new HashSet<Guid>( Groups.SelectMany( g => g.LocationGuids ) );

            foreach ( var location in Locations )
            {
                location.ScheduleGuids.RemoveAll( scheduleGuid => !allScheduleGuids.Contains( scheduleGuid ) );
            }

            Locations.RemoveAll( l => l.ScheduleGuids.Count == 0
                || !allReferencedLocationGuids.Contains( l.Guid ) );

            // Next remove all schedules without locations.
            var allReferencedScheduleGuids = new HashSet<Guid>( Locations.SelectMany( l => l.ScheduleGuids ) );

            Schedules.RemoveAll( s => !allReferencedScheduleGuids.Contains( s.Guid ) );

            // Next remove all groups without locations.
            var allLocationGuids = new HashSet<Guid>( Locations.Select( l => l.Guid ) );

            foreach ( var group in Groups )
            {
                group.LocationGuids.RemoveAll( locationGuid => !allLocationGuids.Contains( locationGuid ) );
            }

            Groups.RemoveAll( g => g.LocationGuids.Count == 0 );

            // Finally remove all areas without groups.
            var allReferencedAreaGuids = new HashSet<Guid>( Groups.Select( g => g.AreaGuid ) );

            Areas.RemoveAll( a => !allReferencedAreaGuids.Contains( a.Guid ) );
        }

        /// <summary>
        /// Gets the counts for all the locations in one query.
        /// </summary>
        /// <param name="locationIds">The location identifiers.</param>
        /// <param name="now">The current timestamp to use for attendance calculation.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>
        /// A dictionary of location unique identifier keys and the unique
        /// identifiers of the people in the location. No value will be available
        /// if there are not any attendance records for the location.
        /// </returns>
        private static Dictionary<Guid, HashSet<Guid>> GetCurrentCountsForLocations( IReadOnlyCollection<int> locationIds, DateTime now, RockContext rockContext )
        {
            if ( locationIds.Count == 0 )
            {
                return new Dictionary<Guid, HashSet<Guid>>();
            }

            var attendances = CheckInDirector.GetCurrentAttendance( now, locationIds, rockContext );

            // We now have all the attendance records for these locations that
            // have check-in today but not yet checked out. Now we need to
            // filter out any that have schedules where check-in is no longer
            // active.

            var activeAttendances = attendances
                .GroupBy( a => new { a.ScheduleGuid, a.CampusGuid } )
                .SelectMany( grp =>
                {
                    // The vast majority of attendance records for a single
                    // location should have the same schedule and campus.
                    var scheduleCache = NamedScheduleCache.Get( grp.Key.ScheduleGuid, rockContext );
                    var campusCache = grp.Key.CampusGuid.HasValue
                        ? CampusCache.Get( grp.Key.CampusGuid.Value, rockContext )
                        : null;

                    return grp.Where( a => Attendance.CalculateIsCurrentlyCheckedIn( a.StartDateTime, a.EndDateTime, campusCache, scheduleCache ) );
                } );

            return activeAttendances
                .GroupBy( a => a.LocationGuid )
                .ToDictionary( grp => grp.Key, grp => new HashSet<Guid>( grp.Select( a => a.PersonGuid ) ) );
        }

        #endregion
    }
}

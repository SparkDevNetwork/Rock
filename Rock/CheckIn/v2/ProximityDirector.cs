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
using System.Diagnostics;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Observability;
using Rock.ViewModels.Rest.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Handles check-in and checkout requests for proximity attendance.
    /// </summary>
    internal class ProximityDirector
    {
        #region Fields

        /// <summary>
        /// The context to use when accessing the database.
        /// </summary>
        private readonly RockContext _rockContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximityDirector"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="rockContext"/> is <c>null</c>.</exception>
        public ProximityDirector( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            _rockContext = rockContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks in a person using the specified proximity beacon information.
        /// If the person is already checked in for the found location then the
        /// existing <see cref="Attendance"/> record will be updated.
        /// </summary>
        /// <param name="person">The person that is being checked in.</param>
        /// <param name="beacon">The object that contains the beacon details.</param>
        /// <returns><c>true</c> if the check-in was successful, otherwise <c>false</c> if the beacon was not valid.</returns>
        public bool CheckIn( Person person, ProximityBeaconBag beacon )
        {
            using ( ObservabilityHelper.StartActivity( "Proximity Check-in" ) )
            {
                var now = RockDateTime.Now;
                var option = GetBestCheckInOption( beacon, now );

                if ( option.LocationId == 0 )
                {
                    return false;
                }

                // Look for an existing attendance record for today.
                var attendanceService = new AttendanceService( _rockContext );
                var attendance = GetCurrentAttendance( person.Id, option, attendanceService, now );

                // If there is no existing attendance record then create one.
                if ( attendance == null )
                {
                    var occurrenceService = new AttendanceOccurrenceService( _rockContext );

                    Activity.Current?.AddEvent( new ActivityEvent( "Get Or Add Occurrence" ) );
                    var occurrence = occurrenceService.GetOrAdd(
                        now,
                        option.GroupId,
                        option.LocationId,
                        option.ScheduleId );

                    Activity.Current?.AddEvent( new ActivityEvent( "Add Attendance" ) );
                    // Create it as a proxy so that navigation properties will work.
#if REVIEW_WEBFORMS
                    attendance = _rockContext.Set<Attendance>().Create();
#else
                    attendance = _rockContext.Set<Attendance>().CreateProxy();
#endif
                    attendance.Occurrence = occurrence;
                    attendance.OccurrenceId = occurrence.Id;
                    attendance.PersonAliasId = person.PrimaryAliasId;

                    attendanceService.Add( attendance );

                    Activity.Current?.AddEvent( new ActivityEvent( "Start IsFirstTime" ) );
                    attendance.IsFirstTime = !attendanceService
                        .Queryable()
                        .Where( a => a.PersonAlias.PersonId == person.Id )
                        .Any();
                    Activity.Current?.AddEvent( new ActivityEvent( "Complete IsFirstTime" ) );
                }

                if ( attendance.StartDateTime > now || attendance.StartDateTime == DateTime.MinValue )
                {
                    attendance.StartDateTime = now;
                }

                attendance.EndDateTime = null;
                attendance.CheckInStatus = Enums.Event.CheckInStatus.Present;
                attendance.DidAttend = true;
                attendance.CampusId = option.CampusId;

                _rockContext.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Checks out a person using the specified proximity beacon information.
        /// If no <see cref="Attendance"/> is found for the matched location then
        /// no action will be performed and <c>true</c> will be returned.
        /// </summary>
        /// <param name="person">The person that is being checked out.</param>
        /// <param name="beacon">The object that contains the beacon details.</param>
        /// <returns><c>true</c> if the checkout was successful, otherwise <c>false</c> if the beacon was not valid.</returns>
        public bool Checkout( Person person, ProximityBeaconBag beacon )
        {
            using ( ObservabilityHelper.StartActivity( "Proximity Checkout" ) )
            {
                var now = RockDateTime.Now;
                var option = GetBestCheckInOption( beacon, now );

                if ( option.LocationId == 0 )
                {
                    return false;
                }

                // Look for an existing attendance record for today.
                var attendanceService = new AttendanceService( _rockContext );
                var attendance = GetCurrentAttendance( person.Id, option, attendanceService, now );

                // If no attendance record and they are leaving the area,
                // don't do anything.
                if ( attendance == null )
                {
                    return true;
                }

                // If they are already checked out, then we don't need to do anything.
                if ( attendance.EndDateTime.HasValue )
                {
                    return true;
                }

                attendance.EndDateTime = now;
                attendance.CheckInStatus = Enums.Event.CheckInStatus.CheckedOut;

                _rockContext.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Find the best matching check-in option for the beacon. This will
        /// an empty struct if no valid option was found.
        /// </summary>
        /// <param name="beacon">The beacon that describes where the person should be checked in.</param>
        /// <param name="now">The current date and time to use for calculations.</param>
        /// <returns>The check-in options that was found.</returns>
        private CheckInOption GetBestCheckInOption( ProximityBeaconBag beacon, DateTime now )
        {
            // Eventually this stopwatch and event message can probably be removed.
            // This is here initially to let us check a few production servers that
            // have lots of group types to make sure that this remains fast.
            var sw = Stopwatch.StartNew();
            var areaIds = GetProximityAreaIds();
            sw.Stop();
            Activity.Current?.AddEvent( new ActivityEvent( $"Get Proximity Area Ids took {sw.Elapsed.TotalMilliseconds}ms" ) );

            var today = now.Date;
            var campus = CampusCache.Get( beacon.Major, _rockContext );

            if ( campus == null )
            {
                return default;
            }

            var locationId = new LocationService( _rockContext )
                .Queryable()
                .Where( l => l.BeaconId == beacon.Minor )
                .Select( l => ( int? ) l.Id )
                .FirstOrDefault();

            if ( !locationId.HasValue )
            {
                return default;
            }

            // Only include groups that are active and belong to an area
            // that is enabled for proximity check-in.
            var groupLocations = GroupLocationCache.AllForLocationId( locationId.Value, _rockContext )
                .Select( gl => new
                {
                    Group = GroupCache.Get( gl.GroupId, _rockContext ),
                    gl.ScheduleIds,
                    gl.LocationId,

                } )
                .Where( gl => gl.Group != null
                    && gl.Group.IsActive == true
                    && areaIds.Contains( gl.Group.GroupTypeId ) );

            // Only include schedules that have a valid schedule for today
            // and have not already ended.
            var groupLocationSchedules = groupLocations
                .SelectMany( gl => gl.ScheduleIds, ( gl, scheduleId ) =>
                {
                    var schedule = NamedScheduleCache.Get( scheduleId, _rockContext );
                    var nextStartDateTime = schedule?.GetNextCheckInStartTime( today );

                    return new
                    {
                        gl.Group,
                        gl.LocationId,
                        Schedule = schedule,
                        StartDateTime = nextStartDateTime,
                        EndDateTime = nextStartDateTime?.AddMinutes( schedule.DurationInMinutes ),
                    };
                } )
                .Where( gl => gl.Schedule != null
                    && gl.Schedule.IsActive == true
                    && gl.StartDateTime.HasValue
                    && gl.StartDateTime.Value.Date == RockDateTime.Today
                    && gl.EndDateTime.Value >= now );

            // Find the best matching schedule for check-in.
            return groupLocationSchedules
                .Select( gl => new CheckInOption( campus.Id, gl.Group.Id, gl.LocationId, gl.Schedule.Id, CalculatedDelta( gl.StartDateTime.Value, now ) ) )
                .OrderBy( gl => gl.Delta )
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the list of check-in area identifiers that are enabled for
        /// proximity check-in.
        /// </summary>
        /// <returns>A list of integer identifiers.</returns>
        private List<int> GetProximityAreaIds()
        {
            // GetCheckInConfiguration() will already check if it is a check-in
            // template group type, so we don't need to check the purpose.
            var rootTypes = GroupTypeCache.All( _rockContext )
                .Where( gt => gt.GetCheckInConfiguration( _rockContext )?.IsProximityEnabled == true )
                .SelectMany( gt => gt.ChildGroupTypes );

            return rootTypes
                .SelectMany( gt => gt.GetDescendentGroupTypes() )
                .Where( gt => gt.TakesAttendance )
                .Select( gt => gt.Id )
                .ToList();
        }

        /// <summary>
        /// Calculates a delta used to determine the best matching check-in option.
        /// This is a positive number where the smaller the number the better the
        /// match.
        /// </summary>
        /// <param name="start">The start date and time of the schedule.</param>
        /// <param name="now">The current date and time used for calculation.</param>
        /// <returns>A delta value representing the best match having a smaller value.</returns>
        private static int CalculatedDelta( DateTime start, DateTime now )
        {
            var delta = start.Subtract( now ).TotalMinutes;

            return Math.Abs( ( int ) delta );
        }

        /// <summary>
        /// Get the current attendance record for the person in the check-in
        /// location option.
        /// </summary>
        /// <param name="personId">The identifier of the person.</param>
        /// <param name="option">The object that describes the check-in location information.</param>
        /// <param name="attendanceService">The service object to use when querying the database.</param>
        /// <param name="now">The current date and time.</param>
        /// <returns>The existing <see cref="Attendance"/> record or <c>null</c> if one was not found.</returns>
        private static Attendance GetCurrentAttendance( int personId, CheckInOption option, AttendanceService attendanceService, DateTime now )
        {
            var todayDateKey = now.Date.ToDateKey();

            return attendanceService.Queryable()
                .Where( a => a.PersonAlias.PersonId == personId
                    && a.Occurrence.LocationId == option.LocationId
                    && a.Occurrence.GroupId == option.GroupId
                    && a.Occurrence.ScheduleId == option.ScheduleId
                    && a.Occurrence.OccurrenceDateKey == todayDateKey )
                .FirstOrDefault();

        }

        #endregion

        private readonly struct CheckInOption
        {
            public int CampusId { get; }

            public int GroupId { get; }

            public int LocationId { get; }

            public int ScheduleId { get; }

            public int Delta { get; }

            public CheckInOption( int campusId, int groupId, int locationId, int scheduleId, int delta )
            {
                CampusId = campusId;
                GroupId = groupId;
                LocationId = locationId;
                ScheduleId = scheduleId;
                Delta = delta;
            }
        }
    }
}

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
using System.Runtime.Serialization;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class for storing the current attendance for a given kiosk location
    /// </summary>
    [DataContract]
    public class KioskLocationAttendance : ItemCache<KioskLocationAttendance>
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="KioskLocationAttendance"/> class from being created.
        /// </summary>
        private KioskLocationAttendance()
        {
        }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        [DataMember]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        /// <value>
        /// The name of the location.
        /// </value>
        [DataMember]
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        [DataMember]
        public List<KioskGroupAttendance> Groups { get; set; }

        /// <summary>
        /// Gets the distinct person ids.
        /// </summary>
        /// <value>
        /// The distinct person ids.
        /// </value>
        public List<int> DistinctPersonIds
        {
            get
            {
                if ( Groups != null )
                {
                    return Groups.SelectMany( g => g.Schedules.Where( s => s.IsActive).SelectMany( s => s.PersonIds ) ).Distinct().ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        public int CurrentCount
        {
            get
            {
                var people = DistinctPersonIds;
                return people != null ? people.Count() : 0;
            }
        }

        #region Static Methods

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get( int id ) instead.")]
        public static KioskLocationAttendance Read( int id )
        {
            return Get( id );
        }

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static KioskLocationAttendance Get( int id )
        {
            return GetOrAddExisting( id, () => Create( id ), new TimeSpan( 0, 2, 0 ) );
        }

        private static KioskLocationAttendance Create( int id )
        { 
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var location = new LocationService( rockContext ).Get( id );
                if ( location != null )
                {
                    var locationAttendance = new KioskLocationAttendance();
                    locationAttendance.LocationId = location.Id;
                    locationAttendance.LocationName = location.Name;
                    locationAttendance.Groups = new List<KioskGroupAttendance>();

                    var attendanceService = new AttendanceService( rockContext );
                    foreach ( var attendance in attendanceService
                        .GetByDateAndLocation( RockDateTime.Today, location.Id )
                        .Where( a => 
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            !a.EndDateTime.HasValue ) )
                    {
                        AddAttendanceRecord( locationAttendance, attendance );
                    }

                    return locationAttendance;
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Remove( int id ) instead.")]
        public static void Flush( int id )
        {
            Remove( id );
        }

        /// <summary>
        /// Adds the attendance.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        public static void AddAttendance( Attendance attendance )
        {
            if ( attendance.Occurrence?.LocationId != null )
            {
                var location = Get(attendance.Occurrence.LocationId.Value);
                if (location != null)
                {
                    AddAttendanceRecord( location, attendance );
                }
            }
        }

        /// <summary>
        /// Adds the attendance record.
        /// </summary>
        /// <param name="kioskLocationAttendance">The kiosk location attendance.</param>
        /// <param name="attendance">The attendance.</param>
        private static void AddAttendanceRecord( KioskLocationAttendance kioskLocationAttendance, Attendance attendance )
        {
            if ( attendance.Occurrence?.GroupId != null && 
                attendance.Occurrence?.ScheduleId != null  &&
                attendance.PersonAlias != null )
            {
                var groupAttendance = kioskLocationAttendance.Groups.Where( g => g.GroupId == attendance.Occurrence.GroupId ).FirstOrDefault();
                if ( groupAttendance == null )
                {
                    groupAttendance = new KioskGroupAttendance();
                    groupAttendance.GroupId = attendance.Occurrence.GroupId.Value;
                    groupAttendance.GroupName = attendance.Occurrence.Group.Name;
                    groupAttendance.Schedules = new List<KioskScheduleAttendance>();
                    kioskLocationAttendance.Groups.Add( groupAttendance );
                }

                var scheduleAttendance = groupAttendance.Schedules.Where( s => s.ScheduleId == attendance.Occurrence.ScheduleId ).FirstOrDefault();
                if ( scheduleAttendance == null )
                {
                    scheduleAttendance = new KioskScheduleAttendance();
                    scheduleAttendance.ScheduleId = attendance.Occurrence.ScheduleId.Value;
                    scheduleAttendance.ScheduleName = attendance.Occurrence.Schedule.Name;
                    scheduleAttendance.IsActive = attendance.IsCurrentlyCheckedIn;
                    scheduleAttendance.PersonIds = new List<int>();
                    groupAttendance.Schedules.Add( scheduleAttendance );
                }

                if ( !scheduleAttendance.PersonIds.Contains( attendance.PersonAlias.PersonId ) )
                {
                    scheduleAttendance.PersonIds.Add( attendance.PersonAlias.PersonId );
                }
            }
        }

        #endregion
    }
}
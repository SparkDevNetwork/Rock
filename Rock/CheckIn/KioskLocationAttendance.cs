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
using System.Data.Entity;
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
            DefaultLifespan = TimeSpan.FromMinutes( 2 );
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
                    return Groups.SelectMany( g => g.Schedules.Where( s => s.IsActive ).SelectMany( s => s.PersonIds ) ).Distinct().ToList();
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
        [Obsolete( "Use Get( int id ) instead.", true )]
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
            return GetOrAddExisting( id, () => Create( id ) );
        }

        private static KioskLocationAttendance Create( int id )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var location = NamedLocationCache.Get( id );
                if ( location == null )
                {
                    return null;
                }

                var locationAttendance = new KioskLocationAttendance();
                locationAttendance.LocationId = location.Id;
                locationAttendance.LocationName = location.Name;
                locationAttendance.Groups = new List<KioskGroupAttendance>();

                var attendanceService = new AttendanceService( rockContext );

                var todayDate = RockDateTime.Today.Date;

                var attendanceList = attendanceService.Queryable()
                    .Where( a =>
                    a.Occurrence.OccurrenceDate == todayDate &&
                    a.Occurrence.LocationId == location.Id &&
                    a.Occurrence.GroupId.HasValue &&
                    a.Occurrence.LocationId.HasValue &&
                    a.Occurrence.ScheduleId.HasValue &&
                    a.PersonAliasId.HasValue &&
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value &&
                    !a.EndDateTime.HasValue )
                    .Select( a => new AttendanceInfo
                    {
                        EndDateTime = a.EndDateTime,
                        StartDateTime = a.StartDateTime,
                        CampusId = a.CampusId,
                        GroupId = a.Occurrence.GroupId,
                        GroupName = a.Occurrence.Group.Name,
                        Schedule = a.Occurrence.Schedule,
                        PersonId = a.PersonAlias.PersonId
                    } )
                    .AsNoTracking()
                    .ToList();

                foreach ( var attendance in attendanceList )
                {
                    AddAttendanceRecord( locationAttendance, attendance );
                }

                return locationAttendance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class AttendanceInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AttendanceInfo"/> class.
            /// Use this if getting AttendanceInfo from a query
            /// </summary>
            public AttendanceInfo()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AttendanceInfo" /> class
            /// Use this if adding an Attendance record
            /// </summary>
            /// <param name="attendance">The attendance.</param>
            /// <param name="locationId">The location identifier.</param>
            public AttendanceInfo( Attendance attendance, int locationId )
            {
                KioskLocationAttendance kioskLocationAttendance = KioskLocationAttendance.Get( locationId );
                var groupId = attendance.Occurrence?.GroupId;
                if ( groupId == null )
                {
                    return;
                }

                // If we can get GroupName from kioskLocationAttendance.Groups, we can avoid lazy loading attendance.Occurrence.Group.
                var groupName = kioskLocationAttendance.Groups.Where( g => g.GroupId == groupId.Value ).FirstOrDefault()?.GroupName;
                if ( groupName == null )
                {
                    groupName = attendance.Occurrence?.Group?.Name;
                }

                EndDateTime = attendance.EndDateTime;
                StartDateTime = attendance.StartDateTime;
                CampusId = attendance.CampusId;
                GroupId = groupId;
                GroupName = groupName;
                Schedule = attendance.Occurrence?.Schedule;
                PersonId = attendance.PersonAlias?.PersonId;
            }

            public DateTime? EndDateTime { get; internal set; }

            public DateTime StartDateTime { get; internal set; }

            public int? CampusId { get; internal set; }

            public int? GroupId { get; internal set; }

            public string GroupName { get; internal set; }

            public Schedule Schedule { get; internal set; }

            public int? PersonId { get; internal set; }
        }

        /// <summary>
        /// Flushes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Remove( int id ) instead.", true )]
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
                var location = Get( attendance.Occurrence.LocationId.Value );
                if ( location != null )
                {
                    AddAttendanceRecord( location, new AttendanceInfo( attendance, location.LocationId ) );
                }
            }
        }

        /// <summary>
        /// Adds the attendance record.
        /// </summary>
        /// <param name="kioskLocationAttendance">The kiosk location attendance.</param>
        /// <param name="attendance">The attendance.</param>
        private static void AddAttendanceRecord( KioskLocationAttendance kioskLocationAttendance, AttendanceInfo attendance )
        {
            if ( attendance.GroupId == null && attendance.Schedule == null )
            {
                // Shouldn't happen, but just in case...
                return;
            }

            if ( attendance.PersonId == null )
            {
                // Shouldn't happen, but just in case...
                return;
            }

            var groupAttendance = kioskLocationAttendance.Groups.Where( g => g.GroupId == attendance.GroupId ).FirstOrDefault();
            if ( groupAttendance == null )
            {
                groupAttendance = new KioskGroupAttendance();
                groupAttendance.GroupId = attendance.GroupId.Value;
                groupAttendance.GroupName = attendance.GroupName;
                groupAttendance.Schedules = new List<KioskScheduleAttendance>();
                kioskLocationAttendance.Groups.Add( groupAttendance );
            }

            var scheduleAttendance = groupAttendance.Schedules.Where( s => s.ScheduleId == attendance.Schedule.Id ).FirstOrDefault();
            if ( scheduleAttendance == null )
            {
                scheduleAttendance = new KioskScheduleAttendance();
                scheduleAttendance.ScheduleId = attendance.Schedule.Id;
                scheduleAttendance.ScheduleName = attendance.Schedule.Name;
                scheduleAttendance.IsActive = Attendance.CalculateIsCurrentlyCheckedIn( attendance.StartDateTime, attendance.EndDateTime, attendance.CampusId, attendance.Schedule );
                scheduleAttendance.PersonIds = new List<int>();
                groupAttendance.Schedules.Add( scheduleAttendance );
            }

            if ( !scheduleAttendance.PersonIds.Contains( attendance.PersonId.Value ) )
            {
                scheduleAttendance.PersonIds.Add( attendance.PersonId.Value );
            }
        }

        #endregion
    }
}
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Caching;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class for storing the current attendance for a given kiosk location
    /// </summary>
    public class KioskLocationAttendance
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
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        /// <value>
        /// The name of the location.
        /// </value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
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
                    return Groups.SelectMany( g => g.Schedules.SelectMany( s => s.PersonIds ) ).Distinct().ToList();
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
        /// Caches the key.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static string CacheKey( int id )
        {
            return string.Format( "Rock:CheckIn:KioskLocationAttendance:{0}", id );
        }

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static KioskLocationAttendance Read( int id )
        {
            string cacheKey = KioskLocationAttendance.CacheKey( id );

            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            KioskLocationAttendance locationAttendance = cache[cacheKey] as KioskLocationAttendance;

            if ( locationAttendance != null )
            {
                return locationAttendance;
            }
            else
            {
                using ( var rockContext = new Rock.Data.RockContext() )
                {
                    var location = new LocationService( rockContext ).Get( id );
                    if ( location != null )
                    {
                        locationAttendance = new KioskLocationAttendance();
                        locationAttendance.LocationId = location.Id;
                        locationAttendance.LocationName = location.Name;
                        locationAttendance.Groups = new List<KioskGroupAttendance>();

                        var attendanceService = new AttendanceService( rockContext );
                        foreach ( var attendance in attendanceService.GetByDateAndLocation( RockDateTime.Today, location.Id ) )
                        {
                            AddAttendanceRecord( locationAttendance, attendance );
                        }

                        var cachePolicy = new CacheItemPolicy();
                        cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( 60 );
                        cache.Set( cacheKey, locationAttendance, cachePolicy );

                        return locationAttendance;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        public static void Flush( int id )
        {
            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            cache.Remove( KioskLocationAttendance.CacheKey( id ) );
        }

        /// <summary>
        /// Adds the attendance.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        public static void AddAttendance( Attendance attendance )
        {
            if ( attendance.LocationId.HasValue )
            {
                var location = KioskLocationAttendance.Read(attendance.LocationId.Value);
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
            if ( attendance.GroupId.HasValue && attendance.ScheduleId.HasValue && attendance.PersonAlias != null )
            {
                var groupAttendance = kioskLocationAttendance.Groups.Where( g => g.GroupId == attendance.GroupId ).FirstOrDefault();
                if ( groupAttendance == null )
                {
                    groupAttendance = new KioskGroupAttendance();
                    groupAttendance.GroupId = attendance.GroupId.Value;
                    groupAttendance.GroupName = attendance.Group.Name;
                    groupAttendance.Schedules = new List<KioskScheduleAttendance>();
                    kioskLocationAttendance.Groups.Add( groupAttendance );
                }

                var scheduleAttendance = groupAttendance.Schedules.Where( s => s.ScheduleId == attendance.ScheduleId ).FirstOrDefault();
                if ( scheduleAttendance == null )
                {
                    scheduleAttendance = new KioskScheduleAttendance();
                    scheduleAttendance.ScheduleId = attendance.ScheduleId.Value;
                    scheduleAttendance.ScheduleName = attendance.Schedule.Name;
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
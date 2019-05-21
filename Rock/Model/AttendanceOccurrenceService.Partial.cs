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
using System.Data;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.AttendanceOccurrence"/> entity objects
    /// </summary>
    public partial class AttendanceOccurrenceService
    {
        /// <summary>
        /// Gets the specified occurrence record.
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date, the time wil be removed.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns></returns>
        public AttendanceOccurrence Get( DateTime occurrenceDate, int? groupId, int? locationId, int? scheduleId )
        {
            // We only want the date. Time need not apply.
            occurrenceDate = occurrenceDate.Date;

            var qry = Queryable().Where( o => o.OccurrenceDate == occurrenceDate );

            qry = groupId.HasValue ?
                qry.Where( o => o.GroupId.HasValue && o.GroupId.Value == groupId.Value ) :
                qry.Where( o => !o.GroupId.HasValue );

            qry = locationId.HasValue ?
                qry.Where( o => o.LocationId.HasValue && o.LocationId.Value == locationId.Value ) :
                qry.Where( o => !o.LocationId.HasValue );

            qry = scheduleId.HasValue ?
                qry.Where( o => o.ScheduleId.HasValue && o.ScheduleId.Value == scheduleId.Value ) :
                qry.Where( o => !o.ScheduleId.HasValue );

            return qry.FirstOrDefault();
        }

        /// <summary>
        /// Gets occurrence data for the selected group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <param name="locationIds">The location ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        public List<AttendanceOccurrence> GetGroupOccurrences( Group group, DateTime? fromDateTime, DateTime? toDateTime, List<int> locationIds, List<int> scheduleIds )
        {
            var qry = Queryable().Where( a => a.GroupId == group.Id );

            // Filter by date range
            if ( fromDateTime.HasValue )
            {
                var fromDate = fromDateTime.Value.Date;
                qry = qry.Where( a => a.OccurrenceDate >= ( fromDate ) );
            }

            if ( toDateTime.HasValue )
            {
                var toDate = toDateTime.Value.Date;
                qry = qry.Where( a => a.OccurrenceDate < ( toDate ) );
            }

            // Location Filter
            if ( locationIds.Any() )
            {
                qry = qry.Where( a => locationIds.Contains( a.LocationId ?? 0 ) );
            }

            // Schedule Filter
            if ( scheduleIds.Any() )
            {
                qry = qry.Where( a => scheduleIds.Contains( a.ScheduleId ?? 0 ) );
            }

            var occurrences = qry.ToList();

            // Create any missing occurrences from the group's schedule (not location schedules)
            Schedule groupSchedule = null;
            if ( group.ScheduleId.HasValue )
            {
                groupSchedule = group.Schedule ?? new ScheduleService( ( RockContext ) Context ).Get( group.ScheduleId.Value );
            }

            if ( groupSchedule == null )
            {
                return occurrences;
            }

            var newOccurrences = new List<AttendanceOccurrence>();

            var existingDates = occurrences
                .Where( o => o.ScheduleId.Equals( groupSchedule.Id ) )
                .Select( o => o.OccurrenceDate.Date )
                .Distinct()
                .ToList();

            var startDate = fromDateTime ?? RockDateTime.Today.AddMonths( -2 );
            var endDate = toDateTime ?? RockDateTime.Today.AddDays( 1 );

            if ( !string.IsNullOrWhiteSpace( groupSchedule.iCalendarContent ) )
            {
                // If schedule has an iCal schedule, get all the past occurrences 
                foreach ( var occurrence in groupSchedule.GetOccurrences( startDate, endDate ) )
                {
                    var newOccurrence = new AttendanceOccurrence
                    {
                        OccurrenceDate = occurrence.Period.StartTime.Date,
                        GroupId = group.Id,
                        Group = group,
                        ScheduleId = groupSchedule.Id,
                        Schedule = groupSchedule
                    };

                    if ( existingDates.Contains( newOccurrence.OccurrenceDate.Date ) )
                    {
                        continue;
                    }

                    newOccurrences.Add( newOccurrence );
                    existingDates.Add( newOccurrence.OccurrenceDate.Date );
                }
            }
            else
            {
                // if schedule does not have an iCal, then check for weekly schedule and calculate occurrences starting with first attendance or current week
                if ( groupSchedule.WeeklyDayOfWeek.HasValue )
                {
                    // default to start with date 2 months earlier
                    startDate = fromDateTime ?? RockDateTime.Today.AddMonths( -2 );
                    if ( existingDates.Any( d => d < startDate ) )
                    {
                        startDate = existingDates.Min();
                    }

                    // Back up start time to the correct day of week
                    while ( startDate.DayOfWeek != groupSchedule.WeeklyDayOfWeek.Value )
                    {
                        startDate = startDate.AddDays( -1 );
                    }

                    // Add the start time
                    if ( groupSchedule.WeeklyTimeOfDay.HasValue )
                    {
                        startDate = startDate.Add( groupSchedule.WeeklyTimeOfDay.Value );
                    }

                    // Create occurrences up to current time
                    while ( startDate < endDate )
                    {
                        if ( !existingDates.Contains( startDate.Date ) )
                        {
                            var newOccurrence = new AttendanceOccurrence
                            {
                                OccurrenceDate = startDate,
                                GroupId = group.Id,
                                Group = group,
                                ScheduleId = groupSchedule.Id,
                                Schedule = groupSchedule
                            };

                            newOccurrences.Add( newOccurrence );
                        }
                        startDate = startDate.AddDays( 7 );
                    }
                }
            }

            if ( newOccurrences.Any() )
            {
                // Filter Exclusions
                var groupType = GroupTypeCache.Get( group.GroupTypeId );
                foreach ( var exclusion in groupType.GroupScheduleExclusions )
                {
                    if ( !exclusion.Start.HasValue || !exclusion.End.HasValue )
                    {
                        continue;
                    }

                    foreach ( var occurrence in newOccurrences.ToList() )
                    {
                        if ( occurrence.OccurrenceDate >= exclusion.Start.Value.Date &&
                            occurrence.OccurrenceDate < exclusion.End.Value.Date.AddDays( 1 ) )
                        {
                            newOccurrences.Remove( occurrence );
                        }
                    }
                }
            }

            foreach ( var occurrence in newOccurrences )
            {
                occurrences.Add( occurrence );
            }

            return occurrences;
        }

        /// <summary>
        /// Ensures that an AttendanceOccurrence record exists for the specified date, schedule, locationId and group. If it doesn't exist, it is created and saved to the database
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>AttendanceOccurrence</returns>
        public AttendanceOccurrence GetOrCreateAttendanceOccurrence( DateTime occurrenceDate, int scheduleId, int? locationId, int groupId )
        {
            // There is a unique constraint on OccurrenceDate, ScheduleId, LocationId and GroupId. So there is at most one record.
            var attendanceOccurrenceQuery = this.Queryable().Where( a =>
                     a.OccurrenceDate == occurrenceDate.Date
                     && a.ScheduleId.HasValue && a.ScheduleId == scheduleId
                     && a.GroupId.HasValue && a.GroupId == groupId );

            if ( locationId.HasValue )
            {
                attendanceOccurrenceQuery = attendanceOccurrenceQuery.Where( a => a.LocationId.HasValue && a.LocationId.Value == locationId.Value );
            }
            else
            {
                attendanceOccurrenceQuery = attendanceOccurrenceQuery.Where( a => a.LocationId.HasValue == false );
            }

            var attendanceOccurrence = attendanceOccurrenceQuery.FirstOrDefault();

            if ( attendanceOccurrence != null )
            {
                return attendanceOccurrence;
            }
            else
            {
                // if the attendance occurrence is not found, create and save it using a separate context, then get it with this context using the created attendanceOccurrence.Id
                int attendanceOccurrenceId;
                using ( var rockContext = new RockContext() )
                {
                    var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                    if ( attendanceOccurrence == null )
                    {
                        attendanceOccurrence = new AttendanceOccurrence
                        {
                            GroupId = groupId,
                            LocationId = locationId,
                            ScheduleId = scheduleId,
                            OccurrenceDate = occurrenceDate
                        };

                        attendanceOccurrenceService.Add( attendanceOccurrence );
                        rockContext.SaveChanges();
                    }

                    attendanceOccurrenceId = attendanceOccurrence.Id;
                }

                return this.Get( attendanceOccurrence.Id );
            }
        }

        /// <summary>
        /// Creates and returns a list of missing attendance occurrences for the specified date, scheduleId and groupLocationIds.
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="groupLocationIds">The group location ids.</param>
        /// <returns></returns>
        public List<AttendanceOccurrence> CreateMissingAttendanceOccurrences( DateTime occurrenceDate, int scheduleId, List<int> groupLocationIds )
        {
            var groupLocationQuery = new GroupLocationService( this.Context as RockContext ).GetByIds( groupLocationIds );

            var attendanceOccurrencesQuery = this.Queryable()
                .Where( a => a.GroupId.HasValue
                        && a.LocationId.HasValue
                        && groupLocationQuery.Any( gl => gl.GroupId == a.GroupId && gl.LocationId == gl.LocationId )
                        && a.ScheduleId == scheduleId
                        && a.OccurrenceDate == occurrenceDate );

            var missingAttendanceOccurrences = groupLocationQuery.Where( gl => !attendanceOccurrencesQuery.Any( ao => ao.LocationId == gl.LocationId && ao.GroupId == gl.GroupId ) )
                            .ToList()
                            .Select( gl => new AttendanceOccurrence
                            {
                                GroupId = gl.GroupId,
                                Group = gl.Group,
                                LocationId = gl.LocationId,
                                Location = gl.Location,
                                ScheduleId = scheduleId,
                                OccurrenceDate = occurrenceDate
                            } ).ToList();

            return missingAttendanceOccurrences;
        }

        /// <summary>
        /// Gets the join queryable of AttendanceOccurrence, GroupLocation, and GroupLocationScheduleConfig for the specified occurrenceDate, scheduleId and groupLocationIds
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="groupLocationIds">The group location ids.</param>
        /// <returns></returns>
        public IQueryable<AttendanceOccurrenceGroupLocationScheduleConfigJoinResult> AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery( DateTime occurrenceDate, int scheduleId, List<int> groupLocationIds )
        {
            var groupLocationQuery = new GroupLocationService( this.Context as RockContext ).GetByIds( groupLocationIds );

            var attendanceOccurrencesQuery = this.Queryable()
                .Where( a => a.GroupId.HasValue
                        && a.LocationId.HasValue
                        && groupLocationQuery.Any( gl => gl.GroupId == a.GroupId && gl.LocationId == a.LocationId )
                        && a.ScheduleId == scheduleId
                        && a.OccurrenceDate == occurrenceDate );

            // join with the GroupLocation 
            var joinQuery = from ao in attendanceOccurrencesQuery
                            join gl in groupLocationQuery
                            on new { LocationId = ao.LocationId.Value, GroupId = ao.GroupId.Value } equals new { gl.LocationId, gl.GroupId }
                            select new AttendanceOccurrenceGroupLocationScheduleConfigJoinResult
                            {
                                AttendanceOccurrence = ao,
                                GroupLocation = gl,
                                GroupLocationScheduleConfig = gl.GroupLocationScheduleConfigs
                                   .Where( c => c.ScheduleId == ao.ScheduleId ).FirstOrDefault()
                            };

            return joinQuery;
        }

        /// <summary>
        /// Return class for <see cref="AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery"/>
        /// </summary>
        public class AttendanceOccurrenceGroupLocationScheduleConfigJoinResult
        {
            /// <summary>
            /// Gets or sets the attendance occurrence.
            /// </summary>
            /// <value>
            /// The attendance occurrence.
            /// </value>
            public AttendanceOccurrence AttendanceOccurrence { get; set; }

            /// <summary>
            /// Gets or sets the group location.
            /// </summary>
            /// <value>
            /// The group location.
            /// </value>
            public GroupLocation GroupLocation { get; set; }

            /// <summary>
            /// Gets or sets the group location schedule configuration.
            /// </summary>
            /// <value>
            /// The group location schedule configuration.
            /// </value>
            public GroupLocationScheduleConfig GroupLocationScheduleConfig { get; set; }
        }
    }
}
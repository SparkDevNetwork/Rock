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
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Schedule"/> entity. This inherits from the Service class
    /// </summary>
    public partial class ScheduleService 
    {
        /// <summary>
        /// Gets occurrence data for the selected group
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="loadAttendanceData">if set to <c>true</c> [load attendance data].</param>
        /// <returns></returns>
        public List<ScheduleOccurrence> GetGroupOccurrences( Group group, bool loadAttendanceData = true )
        {
            var occurrences = new List<ScheduleOccurrence>();

            if ( group != null )
            {
                var rockContext = (RockContext)this.Context;

                var attendanceService = new AttendanceService( rockContext );

                var uniqueStartDates = attendanceService
                    .Queryable().AsNoTracking()
                    .Where( a => a.GroupId == group.Id )
                    .Select( a => a.StartDateTime )
                    .Distinct()
                    .ToList()
                    .Select( dt => dt.Date )
                    .Distinct()
                    .ToList();

                Schedule schedule = null;
                if ( group.ScheduleId.HasValue )
                {
                    schedule = group.Schedule;
                    if ( schedule == null )
                    {
                        schedule = new ScheduleService( rockContext ).Get( group.ScheduleId.Value );
                    }
                }

                if ( schedule != null )
                {
                    var endDate = RockDateTime.Today.AddDays( 1 );

                    DDay.iCal.Event calEvent = schedule.GetCalenderEvent();
                    if ( calEvent != null )
                    {
                        // If schedule has an iCal schedule, get all the past occurrences 
                        foreach ( var occurrence in calEvent.GetOccurrences( DateTime.MinValue, endDate ) )
                        {
                            occurrences.Add( new ScheduleOccurrence( occurrence, schedule.Id ) );
                        }
                    }
                    else
                    {
                        // if schedule does not have an iCal, then check for weekly schedule and calculate occurrences starting with first attendance or current week
                        if ( schedule.WeeklyDayOfWeek.HasValue )
                        {
                            // default to start with date 2 months earlier
                            DateTime startDateTime = RockDateTime.Today.AddMonths( -2 );
                            if ( uniqueStartDates.Any( d => d < startDateTime ) )
                            {
                                startDateTime = uniqueStartDates.Min();
                            }

                            // Back up start time to the correct day of week
                            while ( startDateTime.DayOfWeek != schedule.WeeklyDayOfWeek.Value )
                            {
                                startDateTime = startDateTime.AddDays( -1 );
                            }

                            // Add the start time
                            if ( schedule.WeeklyTimeOfDay.HasValue )
                            {
                                startDateTime = startDateTime.Add( schedule.WeeklyTimeOfDay.Value );
                            }

                            // Create occurrences up to current time
                            while ( startDateTime < endDate )
                            {
                                occurrences.Add( new ScheduleOccurrence( startDateTime, startDateTime.AddDays( 1 ), schedule.Id ) );
                                startDateTime = startDateTime.AddDays( 7 );
                            }
                        }
                    }
                }

                // Add occurrences for any attendance dates that do not fall into a logical occurrence based on the schedule
                var occurrenceDates = occurrences.Select( o => o.StartDateTime.Date ).ToList();
                foreach( var date in uniqueStartDates.Where( d => !occurrenceDates.Contains( d ) ) )
                {
                    occurrences.Add( new ScheduleOccurrence( date, date.AddDays( 1 ) ) );
                }

                if ( occurrences.Any() )
                {
                    // Filter Exclusions
                    var groupType = GroupTypeCache.Read( group.GroupTypeId );
                    foreach ( var exclusion in groupType.GroupScheduleExclusions )
                    {
                        if ( exclusion.Start.HasValue && exclusion.End.HasValue )
                        {
                            foreach ( var occurrence in occurrences.ToList() )
                            {
                                if ( occurrence.StartDateTime >= exclusion.Start.Value &&
                                    occurrence.StartDateTime < exclusion.End.Value.AddDays( 1 ) )
                                {
                                    occurrences.Remove( occurrence );
                                }
                            }
                        }
                    }


                    if ( loadAttendanceData )
                    {
                        var minStartValue = occurrences.Min( o => o.StartDateTime );
                        var maxEndValue = occurrences.Max( o => o.EndDateTime );

                        var attendanceQry = attendanceService
                            .Queryable( "PersonAlias" ).AsNoTracking()
                            .Where( a =>
                                a.GroupId == group.Id &&
                                a.StartDateTime >= minStartValue &&
                                a.StartDateTime < maxEndValue );

                        var attendanceData = attendanceQry.ToList();

                        foreach ( var occurrence in occurrences )
                        {
                            occurrence.Attendance = attendanceData
                                .Where( a =>
                                    a.StartDateTime >= occurrence.StartDateTime &&
                                    a.StartDateTime < occurrence.EndDateTime )
                                .ToList();
                        }
                    }
                }

            }

            return occurrences;

        }

        /// <summary>
        /// Loads the attendance data.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="occurrence">The occurrence.</param>
        public void LoadAttendanceData( Group group, ScheduleOccurrence occurrence )
        {
            if ( group != null && occurrence != null )
            {
                var attendanceData = new AttendanceService( (RockContext)this.Context )
                    .Queryable( "PersonAlias" ).AsNoTracking()
                    .Where( a =>
                        a.GroupId == group.Id &&
                        a.StartDateTime >= occurrence.StartDateTime &&
                        a.StartDateTime < occurrence.EndDateTime )
                    .ToList();

                occurrence.Attendance = attendanceData;
            }
        }
    }
}

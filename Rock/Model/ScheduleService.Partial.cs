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
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <param name="locationId">The location identifier. Use a value of 0 to limit occurrences to those without a location ( a null value will not filter by location ).</param>
        /// <param name="scheduleId">The schedule identifier. Use a value of 0 to limit occurrences to those without a schedule ( a null value will not filter by schedule ).</param>
        /// <param name="loadAttendanceData">if set to <c>true</c> [load attendance data].</param>
        /// <returns></returns>
        public List<ScheduleOccurrence> GetGroupOccurrences( Group group, DateTime? fromDateTime = null, DateTime? toDateTime = null, 
            int? locationId = null, int? scheduleId = null, bool loadAttendanceData = true )
        {
            var occurrences = new List<ScheduleOccurrence>();

            if ( group != null )
            {
                var rockContext = (RockContext)this.Context;
                var attendanceService = new AttendanceService( rockContext );
                var scheduleService = new ScheduleService( rockContext );

                // Get existing 'occurrences'
                var qry = attendanceService
                    .Queryable().AsNoTracking()
                    .Where( a => a.GroupId == group.Id );

                if ( fromDateTime.HasValue )
                {
                    var fromDate = fromDateTime.Value.Date;
                    qry = qry.Where( a => DbFunctions.TruncateTime( a.StartDateTime ) >= ( fromDate ) );
                }

                if ( toDateTime.HasValue )
                {
                    var toDate = toDateTime.Value.Date;
                    qry = qry.Where( a => DbFunctions.TruncateTime( a.StartDateTime ) < ( toDate ) );
                }

                if ( locationId.HasValue )
                {
                    qry = qry.Where( a =>
                        ( a.LocationId.HasValue && a.LocationId.Value == locationId.Value ) ||
                        ( !a.LocationId.HasValue && locationId.Value == 0 ) );
                }

                if ( scheduleId.HasValue )
                {
                    qry = qry.Where( a =>
                        ( a.ScheduleId.HasValue && a.ScheduleId.Value == scheduleId.Value ) ||
                        ( !a.ScheduleId.HasValue && scheduleId.Value == 0 ) );
                }

                foreach ( var occurrence in qry
                    .Select( a => new
                    {
                        a.LocationId,
                        LocationName = a.Location != null ? a.Location.Name : "",
                        a.ScheduleId,
                        ScheduleName = a.Schedule != null ? a.Schedule.Name : "",
                        Date = DbFunctions.TruncateTime( a.StartDateTime )
                    } )
                    .Distinct()
                    .ToList() )
                {
                    if ( occurrence.Date.HasValue )
                    {
                        occurrences.Add(
                            new ScheduleOccurrence(
                                occurrence.Date.Value,
                                occurrence.Date.Value.AddDays( 1 ),
                                occurrence.ScheduleId,
                                occurrence.ScheduleName,
                                occurrence.LocationId, 
                                occurrence.LocationName ) );
                    }
                }

                // Load the attendance data for each occurrence
                if ( loadAttendanceData && occurrences.Any())
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
                            .ToList()
                            .Where( a => 
                                a.LocationId.Equals( occurrence.LocationId ) &&
                                a.ScheduleId.Equals( occurrence.ScheduleId ) )
                            .ToList();
                    }
                }

                // Create any missing occurrences from the group's schedule (not location schedules)
                Schedule groupSchedule = null;
                if ( group.ScheduleId.HasValue )
                {
                    groupSchedule = group.Schedule;
                    if ( groupSchedule == null )
                    {
                        groupSchedule = new ScheduleService( rockContext ).Get( group.ScheduleId.Value );
                    }
                }

                if ( groupSchedule != null )
                {
                    var newOccurrences = new List<ScheduleOccurrence>();

                    var existingDates = occurrences
                        .Where( o => o.ScheduleId.Equals( groupSchedule.Id ) )
                        .Select( o => o.StartDateTime.Date )
                        .Distinct()
                        .ToList();

                    var startDate = fromDateTime.HasValue ? fromDateTime.Value : RockDateTime.Today.AddMonths( -2 );
                    var endDate = toDateTime.HasValue ? toDateTime.Value : RockDateTime.Today.AddDays( 1 );

                    DDay.iCal.Event calEvent = groupSchedule.GetCalenderEvent();
                    if ( calEvent != null )
                    {
                        // If schedule has an iCal schedule, get all the past occurrences 
                        foreach ( var occurrence in calEvent.GetOccurrences( startDate, endDate ) )
                        {
                            var scheduleOccurrence = new ScheduleOccurrence( occurrence, groupSchedule.Id, groupSchedule.Name );
                            if ( !existingDates.Contains( scheduleOccurrence.StartDateTime.Date ) )
                            {
                                newOccurrences.Add( scheduleOccurrence );
                            }
                        }
                    }
                    else
                    {
                        // if schedule does not have an iCal, then check for weekly schedule and calculate occurrences starting with first attendance or current week
                        if ( groupSchedule.WeeklyDayOfWeek.HasValue )
                        {

                            // default to start with date 2 months earlier
                            startDate = fromDateTime.HasValue ? fromDateTime.Value : RockDateTime.Today.AddMonths( -2 );
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
                                    var scheduleOccurrence = new ScheduleOccurrence( startDate, startDate.AddDays( 1 ), groupSchedule.Id, groupSchedule.Name );
                                    newOccurrences.Add( scheduleOccurrence );
                                }

                                startDate = startDate.AddDays( 7 );
                            }
                        }
                    }

                    if ( newOccurrences.Any() )
                    {
                        // Filter Exclusions
                        var groupType = GroupTypeCache.Read( group.GroupTypeId );
                        foreach ( var exclusion in groupType.GroupScheduleExclusions )
                        {
                            if ( exclusion.Start.HasValue && exclusion.End.HasValue )
                            {
                                foreach ( var occurrence in newOccurrences.ToList() )
                                {
                                    if ( occurrence.StartDateTime >= exclusion.Start.Value &&
                                        occurrence.StartDateTime < exclusion.End.Value.AddDays( 1 ) )
                                    {
                                        newOccurrences.Remove( occurrence );
                                    }
                                }
                            }
                        }
                    }

                    foreach( var occurrence in newOccurrences )
                    {
                        occurrence.Attendance = new List<Attendance>();
                        occurrences.Add( occurrence );
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
                    .ToList()
                    .Where( a =>
                        a.LocationId.Equals( occurrence.LocationId ) &&
                        a.ScheduleId.Equals( occurrence.ScheduleId ) )
                    .ToList();

                occurrence.Attendance = attendanceData;
            }
        }
    }
}

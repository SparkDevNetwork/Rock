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

using Rock.Data;
using Rock.Web.Cache;
using System.Data;

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
        /// <param name="locationIds">The location ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="loadSummaryData">if set to <c>true</c> [load summary data].</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use AttendanceService class methods instead" )]
        public List<ScheduleOccurrence> GetGroupOccurrences( Group group, DateTime? fromDateTime, DateTime? toDateTime,
            List<int> locationIds, List<int> scheduleIds, bool loadSummaryData )
        {
            return GetGroupOccurrences( group, fromDateTime, toDateTime, locationIds, scheduleIds, loadSummaryData, null );
        }

        /// <summary>
        /// Gets occurrence data for the selected group
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <param name="locationIds">The location ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="loadSummaryData">if set to <c>true</c> [load summary data].</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use AttendanceService class methods instead" )]
        public List<ScheduleOccurrence> GetGroupOccurrences( Group group, DateTime? fromDateTime, DateTime? toDateTime, 
            List<int> locationIds, List<int> scheduleIds, bool loadSummaryData, int? campusId )
        {
            var occurrences = new List<ScheduleOccurrence>();

            if ( group != null )
            {
                var rockContext = (RockContext)this.Context;
                var attendanceService = new AttendanceService( rockContext );
                var scheduleService = new ScheduleService( rockContext );
                var locationService = new LocationService( rockContext );

                using ( new Rock.Data.QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
                {

                    // Set up an 'occurrences' query for the group
                    var qry = attendanceService
                    .Queryable().AsNoTracking()
                    .Where( a => a.GroupId == group.Id );

                    // Filter by date range
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

                    // Get the unique combination of location/schedule/date for the selected group
                    var occurrenceDates = qry
                    .Select( a => new
                    {
                        a.LocationId,
                        a.ScheduleId,
                        Date = DbFunctions.TruncateTime( a.StartDateTime )
                    } )
                    .Distinct()
                    .ToList();

                    // Get the locations for each unique location id
                    var selectedlocationIds = occurrenceDates.Select( o => o.LocationId ).Distinct().ToList();

                    var locations = locationService
                        .Queryable().AsNoTracking()
                        .Where( l => selectedlocationIds.Contains( l.Id ) )
                        .Select( l => new { l.Id, l.ParentLocationId, l.Name } )
                        .ToList();
                    var locationNames = new Dictionary<int, string>();
                    locations.ForEach( l => locationNames.Add( l.Id, l.Name ) );

                    // Get the parent location path for each unique location
                    var parentlocationPaths = new Dictionary<int, string>();
                    locations
                        .Where( l => l.ParentLocationId.HasValue )
                        .Select( l => l.ParentLocationId.Value )
                        .Distinct()
                        .ToList()
                        .ForEach( l => parentlocationPaths.Add( l, locationService.GetPath( l ) ) );
                    var locationPaths = new Dictionary<int, string>();
                    locations
                        .Where( l => l.ParentLocationId.HasValue )
                        .ToList()
                        .ForEach( l => locationPaths.Add( l.Id, parentlocationPaths[l.ParentLocationId.Value] ) );

                    // Get the schedules for each unique schedule id
                    var selectedScheduleIds = occurrenceDates.Select( o => o.ScheduleId ).Distinct().ToList();
                    var schedules = scheduleService
                        .Queryable().AsNoTracking()
                        .Where( s => selectedScheduleIds.Contains( s.Id ) )
                        .ToList();
                    var scheduleNames = new Dictionary<int, string>();
                    var scheduleStartTimes = new Dictionary<int, TimeSpan>();
                    schedules
                        .ForEach( s =>
                        {
                            scheduleNames.Add( s.Id, s.Name );
                            scheduleStartTimes.Add( s.Id, s.StartTimeOfDay );
                        } );

                    foreach ( var occurrence in occurrenceDates.Where( o => o.Date.HasValue ) )
                    {
                        occurrences.Add(
                            new ScheduleOccurrence(
                                occurrence.Date.Value,
                                occurrence.ScheduleId.HasValue && scheduleStartTimes.ContainsKey( occurrence.ScheduleId.Value ) ?
                                    scheduleStartTimes[occurrence.ScheduleId.Value] : new TimeSpan(),
                                occurrence.ScheduleId,
                                occurrence.ScheduleId.HasValue && scheduleNames.ContainsKey( occurrence.ScheduleId.Value ) ?
                                    scheduleNames[occurrence.ScheduleId.Value] : string.Empty,
                                occurrence.LocationId,
                                occurrence.LocationId.HasValue && locationNames.ContainsKey( occurrence.LocationId.Value ) ?
                                    locationNames[occurrence.LocationId.Value] : string.Empty,
                                occurrence.LocationId.HasValue && locationPaths.ContainsKey( occurrence.LocationId.Value ) ?
                                    locationPaths[occurrence.LocationId.Value] : string.Empty
                            ) );
                    }
                }

                // Load the attendance data for each occurrence
                if ( loadSummaryData && occurrences.Any())
                {
                    var minDate = occurrences.Min( o => o.Date );
                    var maxDate = occurrences.Max( o => o.Date ).AddDays( 1 );
                    var attendanceQry = attendanceService
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.GroupId.HasValue &&
                            a.GroupId == group.Id &&
                            a.StartDateTime >= minDate &&
                            a.StartDateTime < maxDate && 
                            a.PersonAlias != null &&
                            a.PersonAliasId.HasValue )
                        .Select( a => new
                        {
                            a.LocationId,
                            a.ScheduleId,
                            a.StartDateTime,
                            a.DidAttend,
                            a.DidNotOccur,
                            a.PersonAliasId,
                            PersonId = a.PersonAlias.PersonId
                        } );

                    if ( campusId.HasValue )
                    {
                        var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                        var campusQry = new GroupMemberService( rockContext )
                            .Queryable()
                            .Where( g =>
                                g.Group != null &&
                                g.Group.GroupTypeId == familyGroupType.Id &&
                                g.Group.CampusId.HasValue &&
                                g.Group.CampusId.Value == campusId.Value
                            )
                            .Select( m => m.PersonId );

                        attendanceQry = attendanceQry
                            .Where( s => campusQry.Contains( s.PersonId ) );
                    }

                    var attendances = attendanceQry.ToList();

                    foreach ( var summary in attendances
                        .GroupBy( a => new
                        {
                            a.LocationId,
                            a.ScheduleId,
                            Date = a.StartDateTime.Date
                        } )
                        .Select( a => new
                        {
                            a.Key.LocationId,
                            a.Key.ScheduleId,
                            a.Key.Date,
                            DidAttendCount = a
                                .Where( t => t.DidAttend.HasValue && t.DidAttend.Value )
                                .Select( t => t.PersonAliasId.Value )
                                .Distinct()
                                .Count(),
                            DidNotOccurCount = a
                                .Where( t => t.DidNotOccur.HasValue && t.DidNotOccur.Value )
                                .Select( t => t.PersonAliasId.Value )
                                .Distinct()
                                .Count(),
                            TotalCount = a
                                .Select( t => t.PersonAliasId )
                                .Distinct()
                                .Count()
                        } ) )
                    {
                        var occurrence = occurrences
                            .Where( o =>
                                o.ScheduleId.Equals( summary.ScheduleId ) &&
                                o.LocationId.Equals( summary.LocationId ) &&
                                o.Date.Equals( summary.Date ) )
                            .FirstOrDefault();
                        if ( occurrence != null )
                        {
                            occurrence.DidAttendCount = summary.DidAttendCount;
                            occurrence.DidNotOccurCount = summary.DidNotOccurCount;
                            occurrence.TotalCount = summary.TotalCount;
                        }
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
                        .Select( o => o.Date )
                        .Distinct()
                        .ToList();

                    var startDate = fromDateTime.HasValue ? fromDateTime.Value : RockDateTime.Today.AddMonths( -2 );
                    var endDate = toDateTime.HasValue ? toDateTime.Value : RockDateTime.Today.AddDays( 1 );

                    if ( !string.IsNullOrWhiteSpace( groupSchedule.iCalendarContent ) )
                    {
                        // If schedule has an iCal schedule, get all the past occurrences 
                        foreach ( var occurrence in groupSchedule.GetOccurrences( startDate, endDate ) )
                        {
                            var scheduleOccurrence = new ScheduleOccurrence(
                                occurrence.Period.StartTime.Date, occurrence.Period.StartTime.TimeOfDay, groupSchedule.Id, groupSchedule.Name );
                            if ( !existingDates.Contains( scheduleOccurrence.Date ) )
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
                                    var scheduleOccurrence = new ScheduleOccurrence( startDate.Date, startDate.TimeOfDay, groupSchedule.Id, groupSchedule.Name );
                                    newOccurrences.Add( scheduleOccurrence );
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
                            if ( exclusion.Start.HasValue && exclusion.End.HasValue )
                            {
                                foreach ( var occurrence in newOccurrences.ToList() )
                                {
                                    if ( occurrence.Date >= exclusion.Start.Value &&
                                        occurrence.Date < exclusion.End.Value.AddDays( 1 ) )
                                    {
                                        newOccurrences.Remove( occurrence );
                                    }
                                }
                            }
                        }
                    }

                    foreach( var occurrence in newOccurrences )
                    {
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
        [RockObsolete( "1.8" )]
        [Obsolete( "Use AttendanceService class methods instead" )]
        public void LoadSummaryData( Group group, ScheduleOccurrence occurrence )
        {
            if ( group != null && occurrence != null )
            {
                var attendances = new AttendanceService( (RockContext)this.Context )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.PersonAliasId.HasValue &&
                    a.GroupId.HasValue &&
                    a.GroupId == group.Id &&
                    a.LocationId.Equals( occurrence.LocationId ) &&
                    a.ScheduleId.Equals( occurrence.ScheduleId ) &&
                    DbFunctions.TruncateTime( a.StartDateTime ).Equals( occurrence.Date ) )
                .ToList();

                if ( attendances.Any() )
                {
                    occurrence.DidAttendCount = attendances
                        .Where( t => t.DidAttend.HasValue && t.DidAttend.Value )
                        .Select( t => t.PersonAliasId.Value )
                        .Distinct()
                        .Count();

                    occurrence.DidNotOccurCount = attendances
                        .Where( t => t.DidNotOccur.HasValue && t.DidNotOccur.Value )
                        .Select( t => t.PersonAliasId.Value )
                        .Distinct()
                        .Count();

                    occurrence.TotalCount = attendances
                        .Select( t => t.PersonAliasId )
                        .Distinct()
                        .Count();
                }
            }
        }

        /// <summary>
        /// Gets the attendance.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use AttendanceService class methods instead" )]
        public IQueryable<Attendance> GetAttendance( Group group, ScheduleOccurrence occurrence )
        {
            if ( group != null && occurrence != null )
            {
                DateTime startDate = occurrence.Date;
                DateTime endDate = occurrence.Date.AddDays( 1 );

                return new AttendanceService( (RockContext)this.Context )
                    .Queryable( "PersonAlias" ).AsNoTracking()
                    .Where( a =>
                        a.GroupId == group.Id &&
                        a.LocationId == occurrence.LocationId &&
                        a.ScheduleId == occurrence.ScheduleId &&
                        a.StartDateTime >= startDate &&
                        a.StartDateTime < endDate );
            }

            return null;
        }
    }
}

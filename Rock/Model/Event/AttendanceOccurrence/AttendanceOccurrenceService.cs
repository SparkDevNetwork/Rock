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
using System.Data;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.RealTime.Topics;
using Rock.RealTime;
using Rock.ViewModels.Event;
using Rock.Web.Cache;
using System.Threading.Tasks;
using Rock.Logging;
using Microsoft.Extensions.Logging;

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
        /// <param name="occurrenceDate">The occurrence date, the time will be removed.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns></returns>
        public AttendanceOccurrence Get( DateTime occurrenceDate, int? groupId, int? locationId, int? scheduleId )
        {
            return Get( occurrenceDate, groupId, locationId, scheduleId, null );
        }

        /// <summary>
        /// Gets the specified occurrence record.
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date, the time will be removed.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="includes">Allows including attendance occurrence virtual properties like Attendees.</param>
        /// <returns></returns>
        public AttendanceOccurrence Get( DateTime occurrenceDate, int? groupId, int? locationId, int? scheduleId, string includes )
        {
            // We only want the date. Time need not apply.
            occurrenceDate = occurrenceDate.Date;

            var qry = Queryable( includes ).Where( o => o.OccurrenceDate == occurrenceDate );

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
        /// Gets the specified occurrence record, creating it if necessary. Ensures that an AttendanceOccurrence
        /// record exists for the specified date, schedule, locationId and group. If it doesn't exist, it is
        /// created and saved to the database.
        /// NOTE: When looking for a matching occurrence, if null groupId, locationId or scheduleId is given
        /// any matching record must also not have a group, location or schedule.
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="includes">Allows including attendance occurrence virtual properties like Attendees.</param>
        /// <returns>An existing or new attendance occurrence</returns>
        public AttendanceOccurrence GetOrAdd( DateTime occurrenceDate, int? groupId, int? locationId, int? scheduleId, string includes )
        {
            return GetOrAdd( occurrenceDate, groupId, locationId, scheduleId, includes, null );
        }

        /// <summary>
        /// Gets the specified occurrence record, creating it if necessary. Ensures that an AttendanceOccurrence
        /// record exists for the specified date, schedule, locationId and group. If it doesn't exist, it is
        /// created and saved to the database.
        /// NOTE: When looking for a matching occurrence, if null groupId, locationId or scheduleId is given
        /// any matching record must also not have a group, location or schedule.
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="includes">Allows including attendance occurrence virtual properties like Attendees.</param>
        /// <param name="attendanceTypeValueId">The attendance type identifier.</param>
        /// <returns>
        /// An existing or new attendance occurrence
        /// </returns>
        public AttendanceOccurrence GetOrAdd( DateTime occurrenceDate, int? groupId, int? locationId, int? scheduleId, string includes, int? attendanceTypeValueId )
        {
            var occurrence = Get( occurrenceDate, groupId, locationId, scheduleId, includes );

            if ( occurrence == null )
            {
                // If occurrence does not yet exist, create it
                // A new context is used so the occurrence can be saved and used on multiple new attendance records that will be saved at once.
                using ( var newContext = new RockContext() )
                {
                    occurrence = new AttendanceOccurrence
                    {
                        OccurrenceDate = occurrenceDate,
                        GroupId = groupId,
                        LocationId = locationId,
                        ScheduleId = scheduleId,
                        AttendanceTypeValueId = attendanceTypeValueId
                    };

                    var newOccurrenceService = new AttendanceOccurrenceService( newContext );
                    newOccurrenceService.Add( occurrence );
                    newContext.SaveChanges();

                    // Query for the new occurrence using original context.
                    occurrence = Get( occurrence.Id );
                }
            }

            return occurrence;
        }

        /// <summary>
        /// Gets the specified occurrence record, creating it if necessary. Ensures that an AttendanceOccurrence
        /// record exists for the specified date, schedule, locationId and group. If it doesn't exist, it is
        /// created and saved to the database.
        /// NOTE: When looking for a matching occurrence, if null groupId, locationId or scheduleId is given
        /// any matching record must also not have a group, location or schedule.
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns>An existing or new attendance occurrence</returns>
        public AttendanceOccurrence GetOrAdd( DateTime occurrenceDate, int? groupId, int? locationId, int? scheduleId )
        {
            return GetOrAdd( occurrenceDate, groupId, locationId, scheduleId, null );
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
                foreach ( var occurrence in groupSchedule.GetICalOccurrences( startDate, endDate ) )
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

                    // Roll forward to the first start time that matches the day of the week.
                    while ( startDate.DayOfWeek != groupSchedule.WeeklyDayOfWeek.Value )
                    {
                        startDate = startDate.AddDays( 1 );
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
        /// Gets future occurrence data for the selected group (including all scheduled dates), sorted by occurrence data in ascending order.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="toDateTime">To date time.  If not supplied, this will default to 6 months from the current date.</param>
        /// <param name="locationIds">The location ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        public List<AttendanceOccurrence> GetFutureGroupOccurrences( Group group, DateTime? toDateTime, string locationIds = null, string scheduleIds = null )
        {
            var locationIdList = new List<int>();
            if ( !string.IsNullOrWhiteSpace( locationIds ) )
            {
                locationIdList = locationIds.Split( ',' ).Select( int.Parse ).ToList();
            }

            var scheduleIdList = new List<int>();
            if ( !string.IsNullOrWhiteSpace( scheduleIds ) )
            {
                scheduleIdList = scheduleIds.Split( ',' ).Select( int.Parse ).ToList();
            }

            var qry = Queryable( "Group,Schedule" ).AsNoTracking().Where( a => a.GroupId == group.Id );

            // Filter by date range
            var fromDate = RockDateTime.Now.Date;
            var toDate = fromDate.AddMonths( 6 ); // Default to 6 months in the future.
            if ( toDateTime.HasValue )
            {
                toDate = toDateTime.Value.Date;
            }
            qry = qry
                .Where( a => a.OccurrenceDate >= ( fromDate ) )
                .Where( a => a.OccurrenceDate < ( toDate ) );

            // Location Filter
            if ( locationIdList.Any() )
            {
                qry = qry.Where( a => locationIdList.Contains( a.LocationId ?? 0 ) );
            }

            // Schedule Filter
            if ( scheduleIdList.Any() )
            {
                qry = qry.Where( a => scheduleIdList.Contains( a.ScheduleId ?? 0 ) );
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
                // If there's no group schedule, sort the occurrences and return them.
                occurrences = occurrences.OrderBy( o => o.OccurrenceDate ).ToList();
                return occurrences;
            }

            var newOccurrences = new List<AttendanceOccurrence>();

            var existingDates = occurrences
                .Where( o => o.ScheduleId.Equals( groupSchedule.Id ) )
                .Select( o => o.OccurrenceDate.Date )
                .Distinct()
                .ToList();

            if ( !string.IsNullOrWhiteSpace( groupSchedule.iCalendarContent ) )
            {
                // If schedule has an iCal schedule, get all the past occurrences 
                foreach ( var occurrence in groupSchedule.GetICalOccurrences( fromDate, toDate ) )
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
                    var startDate = fromDate;
                    // Move start time forward to the correct day of week.
                    while ( startDate.DayOfWeek != groupSchedule.WeeklyDayOfWeek.Value )
                    {
                        startDate = startDate.AddDays( 1 );
                    }

                    // Add the start time
                    if ( groupSchedule.WeeklyTimeOfDay.HasValue )
                    {
                        startDate = startDate.Add( groupSchedule.WeeklyTimeOfDay.Value );
                    }

                    // Create occurrences up to current time
                    while ( startDate < toDate )
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

            // Sort occurs here to include any new occurrences added by the group schedule.
            occurrences = occurrences.OrderBy( o => o.OccurrenceDate ).ToList();

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
        [Obsolete( "Use GetOrAdd instead", true )]
        [RockObsolete( "1.10" )]
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
        /// Creates and returns a list of missing attendance occurrences for the specified dates, scheduleId and groupLocationIds.
        /// These will be new AttendanceOccurrence records that haven't been saved to the database yet.
        /// </summary>
        /// <param name="occurrenceDateList">The occurrence date list.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="groupLocationIds">The group location ids.</param>
        /// <returns></returns>
        public List<AttendanceOccurrence> CreateMissingAttendanceOccurrences( List<DateTime> occurrenceDateList, int scheduleId, List<int> groupLocationIds )
        {
            if ( !groupLocationIds.Any() )
            {
                return new List<AttendanceOccurrence>();
            }

            var groupLocationQuery = new GroupLocationService( this.Context as RockContext ).GetByIds( groupLocationIds );
            int? groupId = null;
            int? locationId = null;
            if ( groupLocationIds.Count == 1 )
            {
                // if there is only one group location, we can optimize the attendanceOccurrencesQuery to use a simpler LINQ expression
                var groupLocationInfo = groupLocationQuery.Select( a => new { a.GroupId, a.LocationId } ).FirstOrDefault();
                groupId = groupLocationInfo.GroupId;
                locationId = groupLocationInfo?.LocationId;
            }

            List<AttendanceOccurrence> missingAttendanceOccurrenceList = new List<AttendanceOccurrence>();
            foreach ( var occurrenceDate in occurrenceDateList )
            {
                var attendanceOccurrencesQuery = this.Queryable()
                    .Where( a => a.GroupId.HasValue
                            && a.LocationId.HasValue
                            && a.ScheduleId == scheduleId
                            && a.OccurrenceDate == occurrenceDate );

                if ( groupId.HasValue && locationId.HasValue )
                {
                    // since we have just group location id (and date and schedule), we just have to check if the attendance occurrence exists
                    attendanceOccurrencesQuery = attendanceOccurrencesQuery.Where( a => a.GroupId == groupId.Value && a.LocationId == locationId.Value );
                    if ( attendanceOccurrencesQuery.Any() )
                    {
                        continue;
                    }
                    else
                    {
                        missingAttendanceOccurrenceList.Add( new AttendanceOccurrence
                        {
                            GroupId = groupId.Value,
                            LocationId = locationId.Value,
                            ScheduleId = scheduleId,
                            OccurrenceDate = occurrenceDate
                        } );
                    }

                    continue;
                }
                else
                {
                    attendanceOccurrencesQuery = attendanceOccurrencesQuery.Where( a => groupLocationQuery.Any( gl => gl.GroupId == a.GroupId && gl.LocationId == a.LocationId ) );
                    List<AttendanceOccurrence> missingAttendanceOccurrencesForOccurrenceDate =
                        groupLocationQuery.Where( gl => !attendanceOccurrencesQuery.Any( ao => ao.LocationId == gl.LocationId && ao.GroupId == gl.GroupId ) )
                        .Select( gl => new
                        {
                            gl.GroupId,
                            gl.LocationId,
                        } )
                        .AsNoTracking()
                        .ToList()
                        .Select( gl => new AttendanceOccurrence
                        {
                            GroupId = gl.GroupId,
                            LocationId = gl.LocationId,
                            ScheduleId = scheduleId,
                            OccurrenceDate = occurrenceDate
                        } ).ToList();

                    if ( missingAttendanceOccurrencesForOccurrenceDate.Any() )
                    {
                        missingAttendanceOccurrenceList.AddRange( missingAttendanceOccurrencesForOccurrenceDate );
                    }
                }
            }

            return missingAttendanceOccurrenceList;
        }

        /// <summary>
        /// Gets the join queryable of AttendanceOccurrence, GroupLocation, and GroupLocationScheduleConfig for the specified occurrenceDate, scheduleId and groupLocationIds
        /// </summary>
        /// <param name="occurrenceDateList">The occurrence date list.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="groupLocationIds">The group location ids.</param>
        /// <returns></returns>
        public IQueryable<AttendanceOccurrenceGroupLocationScheduleConfigJoinResult> AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery( List<DateTime> occurrenceDateList, int scheduleId, List<int> groupLocationIds )
        {
            var scheduleIds = new List<int>();
            scheduleIds.Add( scheduleId );
            return AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery( occurrenceDateList, scheduleIds, groupLocationIds );
        }

        /// <summary>
        /// Gets the join queryable of AttendanceOccurrence, GroupLocation, and GroupLocationScheduleConfig for the specified occurrenceDate, scheduleId and groupLocationIds
        /// </summary>
        /// <param name="occurrenceDateList">The occurrence date list.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="groupLocationIds">The group location ids.</param>
        /// <returns></returns>
        public IQueryable<AttendanceOccurrenceGroupLocationScheduleConfigJoinResult> AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery( List<DateTime> occurrenceDateList, List<int> scheduleIds, List<int> groupLocationIds )
        {
            var groupLocationQuery = new GroupLocationService( this.Context as RockContext ).GetByIds( groupLocationIds );

            var attendanceOccurrencesQuery = this.Queryable()
                .Where( a => a.GroupId.HasValue
                        && a.LocationId.HasValue
                        && a.ScheduleId.HasValue
                        && groupLocationQuery.Any( gl => gl.GroupId == a.GroupId && gl.LocationId == a.LocationId )
                        && scheduleIds.Contains( a.ScheduleId.Value )
                        && occurrenceDateList.Contains( a.OccurrenceDate ) );

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
        /// Return class for <see cref="AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery(List{DateTime}, List{int}, List{int})"/>
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

        #region RealTime Related

        /// <summary>
        /// Sends the attendance occurrence updated real time notifications for the specified
        /// attendance records.
        /// </summary>
        /// <param name="attendanceOccurrenceGuids">The attendance occurrence unique identifiers.</param>
        /// <returns>A task that represents this operation.</returns>
        internal static async Task SendAttendanceOccurrenceUpdatedRealTimeNotificationsAsync( IEnumerable<Guid> attendanceOccurrenceGuids )
        {
            var guids = attendanceOccurrenceGuids.ToList();

            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                while ( guids.Any() )
                {
                    // Work with at most 1,000 records at a time since it
                    // translates to an IN query which doesn't perform well
                    // on large sets.
                    var guidsToProcess = guids.Take( 1_000 ).ToList();
                    guids = guids.Skip( 1_000 ).ToList();

                    try
                    {
                        var qry = attendanceOccurrenceService
                            .Queryable()
                            .AsNoTracking()
                            .Where( a => attendanceOccurrenceGuids.Contains( a.Guid ) );

                        await SendAttendanceOccurrenceUpdatedRealTimeNotificationsAsync( qry );
                    }
                    catch ( Exception ex )
                    {
                        RockLogger.LoggerFactory.CreateLogger<AttendanceOccurrenceService>()
                            .LogError( ex, ex.Message );
                    }
                }
            }
        }

        /// <summary>
        /// Send attendance occurrence updated real time notifications for the AttendanceOccurrence
        /// records returned by the query.
        /// </summary>
        /// <param name="qry">The query that provides the attendance occurrence records.</param>
        /// <returns>A task that represents this operation.</returns>
        private static async Task SendAttendanceOccurrenceUpdatedRealTimeNotificationsAsync( IQueryable<AttendanceOccurrence> qry )
        {
            var bags = GetAttendanceOccurrenceUpdatedMessageBags( qry );

            if ( !bags.Any() )
            {
                return;
            }

            var topicClients = RealTimeHelper.GetTopicContext<IEntityUpdated>().Clients;

            var tasks = bags
                .Select( b =>
                {
                    return Task.Run( () =>
                    {
                        var channels = EntityUpdatedTopic.GetAttendanceOccurrenceChannelsForBag( b );

                        return topicClients
                            .Channels( channels )
                            .AttendanceOccurrenceUpdated( b );
                    } );
                } )
                .ToArray();

            try
            {
                await Task.WhenAll( tasks );
            }
            catch ( Exception ex )
            {
                RockLogger.LoggerFactory.CreateLogger<AttendanceOccurrenceService>()
                    .LogError( ex, ex.Message );
            }
        }

        /// <summary>
        /// Gets the attendance occurrence updated message bags from the query using the
        /// most optimal pattern.
        /// </summary>
        /// <param name="qry">The query that provides the attendance occurrence records.</param>
        /// <returns>A list of <see cref="AttendanceOccurrenceUpdatedMessageBag"/> objects that represent the attendance occurrence records.</returns>
        private static List<AttendanceOccurrenceUpdatedMessageBag> GetAttendanceOccurrenceUpdatedMessageBags( IQueryable<AttendanceOccurrence> qry )
        {
            var publicApplicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );

            var records = qry
                .Select( a => new
                {
                    a.Guid,
                    OccurrenceGuid = a.Guid,
                    GroupGuid = ( Guid? ) a.Group.Guid,
                    LocationGuid = ( Guid? ) a.Location.Guid,
                    a.DidNotOccur,
                    a.AttendanceTypeValueId
                } )
                .ToList();

            return records
                .Select( a => new AttendanceOccurrenceUpdatedMessageBag
                {
                    OccurrenceGuid = a.OccurrenceGuid,
                    GroupGuid = a.GroupGuid,
                    LocationGuid = a.LocationGuid,
                    AttendanceOccurrenceTypeGuid = a.AttendanceTypeValueId.HasValue ? DefinedValueCache.GetGuid( a.AttendanceTypeValueId.Value ) : ( Guid? )null,
                    DidNotOccur = a.DidNotOccur
                } )
                .ToList();
        }

        #endregion
    }

    #region Extension Methods
    public partial class AttendanceOccurrenceExtensionMethods
    {
        /// <summary>
        /// AttendanceOccurrence in the specified date range.
        /// </summary>
        /// <param name="attendanceOccurrences">The attendance occurrences.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> DateInRange( this IQueryable<AttendanceOccurrence> attendanceOccurrences, DateTime startDate, DateTime endDate )
        {
            return attendanceOccurrences
                    .Where( a => a.OccurrenceDate >= startDate )
                    .Where( a => a.OccurrenceDate < endDate );
        }

        /// <summary>
        /// AttendanceOccurrence with the specified Group Ids.
        /// </summary>
        /// <param name="attendanceOccurrences">The attendance occurrences.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> GroupIdsInList( this IQueryable<AttendanceOccurrence> attendanceOccurrences, List<int> groupIds )
        {
            return attendanceOccurrences
                    .Where( a => a.GroupId.HasValue )
                    .Where( a => groupIds.Contains( a.GroupId.Value ) );
        }

        /// <summary>
        /// AttendanceOccurrence with a Schedule Id.
        /// </summary>
        /// <param name="attendanceOccurrences">The attendance occurrences.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> HasScheduleId( this IQueryable<AttendanceOccurrence> attendanceOccurrences )
        {
            return attendanceOccurrences
                    .Where( a => a.ScheduleId.HasValue );
        }

        /// <summary>
        /// AttendanceOccurrence that either have attendees or are marked as "Did not occur".
        /// </summary>
        /// <param name="attendanceOccurrences">The attendance occurrences.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> HasAttendeesOrDidNotOccur( this IQueryable<AttendanceOccurrence> attendanceOccurrences )
        {
            return attendanceOccurrences
                    .Where( a => a.Attendees.Any() || ( a.DidNotOccur.HasValue && a.DidNotOccur.Value ) );
        }

        /// <summary>
        /// AttendanceOccurrence that either have attendees, or are marked as "Did not occur", or has already had an attendance reminder sent today.
        /// </summary>
        /// <param name="attendanceOccurrences">The attendance occurrences.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> HasAttendeesOrDidNotOccurOrRemindersAlreadySentToday( this IQueryable<AttendanceOccurrence> attendanceOccurrences )
        {
            return attendanceOccurrences
                    .Where(
                        a => a.Attendees.Any()
                        || ( a.DidNotOccur.HasValue && a.DidNotOccur.Value )
                        || a.AttendanceReminderLastSentDateTime >= RockDateTime.Today
                    );
        }

        /// <summary>
        /// Where the attendance occurred at an active location (or the location is null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> WhereHasActiveLocation( this IQueryable<AttendanceOccurrence> query )
        {
            // Null is allowed since the location relationship is not required
            return query.Where( ao => ao.Location == null || ao.Location.IsActive );
        }

        /// <summary>
        /// Where the attendance occurred with an active schedule (or the schedule is null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> WhereHasActiveSchedule( this IQueryable<AttendanceOccurrence> query )
        {
            // Null is allowed since the schedule relationship is not required
            return query.Where( ao => ao.Schedule == null || ao.Schedule.IsActive );
        }

        /// <summary>
        /// Where the attendance occurred with an active group (or the group is null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> WhereHasActiveGroup( this IQueryable<AttendanceOccurrence> query )
        {
            // Null is allowed since the group relationship is not required
            return query.Where( ao => ao.Group == null || ao.Group.IsActive );
        }

        /// <summary>
        /// Where the entities are active (deduced from the group, schedule, and location all being active or null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceOccurrence> WhereDeducedIsActive( this IQueryable<AttendanceOccurrence> query )
        {
            return query
                .WhereHasActiveGroup()
                .WhereHasActiveLocation()
                .WhereHasActiveSchedule();
        }
    }
    #endregion
}
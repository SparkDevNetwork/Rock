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
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;

using Rock.BulkImport;
using Rock.Chart;
using Rock.Communication;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.Attendance"/> entity objects
    /// </summary>
    public partial class AttendanceService
    {
        /// <summary>
        /// Adds or updates an attendance record and will create the occurrence if needed
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public Attendance AddOrUpdate( int personAliasId, DateTime occurrenceDate, int? groupId )
        {
            return AddOrUpdate( personAliasId, occurrenceDate, groupId, null, null, null );
        }

        /// <summary>
        /// Adds or updates an attendance record and will create the occurrence if needed
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        public Attendance AddOrUpdate( int personAliasId, DateTime occurrenceDate,
                    int? groupId, int? locationId, int? scheduleId, int? campusId )
        {
            return AddOrUpdate( personAliasId, occurrenceDate, groupId, locationId, scheduleId, campusId, null, null, null, null, null );
        }

        /// <summary>
        /// Adds or updates an attendance record and will create the occurrence if needed
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="checkinDateTime.Date">The check-in date time.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="searchTypeValueId">The search type value identifier.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="searchResultGroupId">The search result group identifier.</param>
        /// <param name="attendanceCodeId">The attendance code identifier.</param>
        /// <returns></returns>
        public Attendance AddOrUpdate( int? personAliasId, DateTime checkinDateTime,
                    int? groupId, int? locationId, int? scheduleId, int? campusId, int? deviceId,
                    int? searchTypeValueId, string searchValue, int? searchResultGroupId, int? attendanceCodeId )
        {
            return AddOrUpdate( personAliasId, checkinDateTime, groupId, locationId, scheduleId, campusId, deviceId,
                searchTypeValueId, searchValue, searchResultGroupId, attendanceCodeId, null );
        }

        /// <summary>
        /// Adds or updates an attendance record and will create the occurrence if needed
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="checkinDateTime">The check-in date time.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="searchTypeValueId">The search type value identifier.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="searchResultGroupId">The search result group identifier.</param>
        /// <param name="attendanceCodeId">The attendance code identifier.</param>
        /// <param name="checkedInByPersonAliasId">The checked in by person alias identifier.</param>
        /// <returns></returns>
        public Attendance AddOrUpdate( int? personAliasId, DateTime checkinDateTime,
                    int? groupId, int? locationId, int? scheduleId, int? campusId, int? deviceId,
                    int? searchTypeValueId, string searchValue, int? searchResultGroupId, int? attendanceCodeId, int? checkedInByPersonAliasId )
        {
            return AddOrUpdate( personAliasId, checkinDateTime, groupId, locationId, scheduleId, campusId, deviceId,
                     searchTypeValueId, searchValue, searchResultGroupId, attendanceCodeId, checkedInByPersonAliasId, null );
        }

        /// <summary>
        /// Adds or updates an attendance record and will create the occurrence if needed
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="checkinDateTime">The check-in date time.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="searchTypeValueId">The search type value identifier.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="searchResultGroupId">The search result group identifier.</param>
        /// <param name="attendanceCodeId">The attendance code identifier.</param>
        /// <param name="checkedInByPersonAliasId">The checked in by person alias identifier.</param>
        /// <param name="attendanceTypeValueId">The attendance type identifier.</param>
        /// <returns></returns>
        public Attendance AddOrUpdate( int? personAliasId, DateTime checkinDateTime,
                    int? groupId, int? locationId, int? scheduleId, int? campusId, int? deviceId,
                    int? searchTypeValueId, string searchValue, int? searchResultGroupId, int? attendanceCodeId,
                    int? checkedInByPersonAliasId, int? attendanceTypeValueId )
        {
            // Check to see if an occurrence exists already
            var occurrenceService = new AttendanceOccurrenceService( ( RockContext ) Context );
            var occurrence = occurrenceService.GetOrAdd( checkinDateTime.Date, groupId, locationId, scheduleId, "Attendees", attendanceTypeValueId );

            // If we still don't have an occurrence record (i.e. validation failed) return null 
            if ( occurrence == null )
            {
                return null;
            }

            // Query for existing attendance record
            Attendance attendance = null;
            if ( personAliasId.HasValue )
            {
                attendance = occurrence.Attendees
                .FirstOrDefault( a =>
                    a.PersonAliasId.HasValue &&
                    a.PersonAliasId.Value == personAliasId.Value );
            }

            // If an attendance record doesn't exist for the occurrence, add a new record
            if ( attendance == null )
            {
                attendance = new Attendance
                {
                    Occurrence = occurrence,
                    OccurrenceId = occurrence.Id,
                    PersonAliasId = personAliasId
                };

                Add( attendance );
            }

            // Update details of the attendance (do not overwrite an existing value with an empty value)
            if ( campusId.HasValue )
                attendance.CampusId = campusId.Value;
            if ( deviceId.HasValue )
                attendance.DeviceId = deviceId.Value;
            if ( searchTypeValueId.HasValue )
                attendance.SearchTypeValueId = searchTypeValueId;
            if ( searchValue.IsNotNullOrWhiteSpace() )
                attendance.SearchValue = searchValue;
            if ( checkedInByPersonAliasId.HasValue )
                attendance.CheckedInByPersonAliasId = checkedInByPersonAliasId.Value;
            if ( searchResultGroupId.HasValue )
                attendance.SearchResultGroupId = searchResultGroupId;
            if ( attendanceCodeId.HasValue )
                attendance.AttendanceCodeId = attendanceCodeId;
            attendance.StartDateTime = checkinDateTime;
            attendance.DidAttend = true;

            return attendance;
        }

        /// <summary>
        /// Adds or updates an attendance record and will create the occurrence if needed
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="checkinDateTime">The check-in date time.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="searchTypeValueId">The search type value identifier.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="searchResultGroupId">The search result group identifier.</param>
        /// <param name="attendanceCodeId">The attendance code identifier.</param>
        /// <param name="checkedInByPersonAliasId">The checked in by person alias identifier.</param>
        /// <param name="syncMatchingStreaks">Should matching <see cref="StreakType"/> models be synchronized.</param>
        /// <returns></returns>
        [Obsolete( "The syncMatchingStreaks param is no longer used" )]
        [RockObsolete( "1.10" )]
        public Attendance AddOrUpdate( int? personAliasId, DateTime checkinDateTime,
                int? groupId, int? locationId, int? scheduleId, int? campusId, int? deviceId,
                int? searchTypeValueId, string searchValue, int? searchResultGroupId, int? attendanceCodeId, int? checkedInByPersonAliasId,
                bool syncMatchingStreaks )
        {
            return AddOrUpdate( personAliasId, checkinDateTime, groupId, locationId, scheduleId, campusId, deviceId, searchTypeValueId,
                searchValue, searchResultGroupId, attendanceCodeId, checkedInByPersonAliasId );
        }

        /// <summary>
        /// Returns a specific <see cref="Rock.Model.Attendance"/> record.
        /// </summary>
        /// <param name="date">A <see cref="System.DateTime"/> representing the date attended.</param>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> </param>
        /// <param name="scheduleId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/></param>
        /// <param name="groupId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/>.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/></param>
        /// <returns>The first <see cref="Rock.Model.Attendance"/> entity that matches the provided values.</returns>
        public Attendance Get( DateTime date, int locationId, int scheduleId, int groupId, int personId )
        {
            return Queryable( "Occurrence.Group,Occurrence.Schedule,PersonAlias.Person" )
                .FirstOrDefault( a =>
                     a.Occurrence.OccurrenceDate == date.Date &&
                     a.Occurrence.LocationId == locationId &&
                     a.Occurrence.ScheduleId == scheduleId &&
                     a.Occurrence.GroupId == groupId &&
                     a.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Determines whether the person is scheduled for the specified date and schedule for any group/location
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified date is scheduled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsScheduled( DateTime date, int scheduleId, int personId )
        {
            return Queryable()
                .Where( a =>
                     a.Occurrence.OccurrenceDate == date.Date &&
                     a.Occurrence.ScheduleId == scheduleId &&
                     ( a.ScheduledToAttend == true || a.RequestedToAttend == true ) &&
                     a.RSVP != RSVP.No &&
                     a.PersonAlias.PersonId == personId )
                .Any();
        }

        /// <summary>
        /// Returns a Queryable collection of <see cref="Rock.Model.Attendance"/> for a <see cref="Rock.Model.Location"/> on a specified date.
        /// </summary>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/></param>
        /// <param name="date">A <see cref="System.DateTime"/> representing the date attended.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Attendance"/> entities for a specific date and location.</returns>
        public IQueryable<Attendance> GetByDateAndLocation( DateTime date, int locationId )
        {
            return Queryable( "Occurrence.Group,Occurrence.Schedule,PersonAlias" )
                .Where( a =>
                    a.Occurrence.OccurrenceDate == date.Date &&
                    a.Occurrence.LocationId == locationId &&
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Attendance"/> for specific <see cref="Rock.Model.Schedule"/> in a <see cref="Rock.Model.Location"/> on a specified date.
        /// </summary>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/></param>
        /// <param name="date">A <see cref="System.DateTime"/> representing the date attended.</param>
        /// <param name="scheduleId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/></param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Attendance"/> entities for a specific date and location.</returns>
        public IQueryable<Attendance> GetByDateOnLocationAndSchedule( DateTime date, int locationId, int scheduleId )
        {
            return GetByDateAndLocation( date, locationId )
                .Where( a => a.Occurrence.ScheduleId == scheduleId );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Attendance" /> for the given date, location, schedule, and RSVP. No other filtering is done.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="rsvp">The RSVP.</param>
        /// <returns></returns>
        public IQueryable<Attendance> GetAttendances( DateTime date, int locationId, int scheduleId, Rock.Model.RSVP rsvp )
        {
            return Queryable( "Occurrence.Group,Occurrence.Schedule,PersonAlias" )
                .Where( a => a.Occurrence.OccurrenceDate == date.Date
                    && a.Occurrence.LocationId == locationId
                    && a.Occurrence.ScheduleId == scheduleId
                    && a.RSVP == rsvp );
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        /// <param name="graphBy">The graph by.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="campusIds">The campus ids. Include the keyword 'null' in the list to include CampusId is null</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        public IEnumerable<IChartData> GetChartData( ChartGroupBy groupBy, AttendanceGraphBy graphBy, DateTime? startDate, DateTime? endDate, string groupIds, string campusIds, int? dataViewId )
        {
            return GetChartData( groupBy, graphBy, startDate, endDate, groupIds, campusIds, dataViewId, null );
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        /// <param name="graphBy">The graph by.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="campusIds">The campus ids. Include the keyword 'null' in the list to include CampusId is null</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        public IEnumerable<IChartData> GetChartData( ChartGroupBy groupBy = ChartGroupBy.Week, AttendanceGraphBy graphBy = AttendanceGraphBy.Total, DateTime? startDate = null, DateTime? endDate = null, string groupIds = null, string campusIds = null, int? dataViewId = null, string scheduleIds = null )
        {
            var qryAttendance = Queryable().AsNoTracking()
                .Where( a =>
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value &&
                    a.PersonAlias != null );

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
                qryAttendance = qryAttendance.Where( a => a.Occurrence.SundayDate >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                endDate = endDate.Value.Date.AddDays( 1 );
                qryAttendance = qryAttendance.Where( a => a.Occurrence.SundayDate < endDate.Value );
            }

            if ( dataViewId.HasValue )
            {
                var rockContext = ( RockContext ) this.Context;

                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var personService = new PersonService( rockContext );

                    var paramExpression = personService.ParameterExpression;
                    var whereExpression = dataView.GetExpression( personService, paramExpression );

                    Rock.Web.UI.Controls.SortProperty sort = null;
                    var dataViewPersonIdQry = personService
                        .Queryable().AsNoTracking()
                        .Where( paramExpression, whereExpression, sort )
                        .Select( p => p.Id );

                    qryAttendance = qryAttendance.Where( a => dataViewPersonIdQry.Contains( a.PersonAlias.PersonId ) );
                }
            }

            if ( !string.IsNullOrWhiteSpace( groupIds ) )
            {
                var groupIdList = groupIds.Split( ',' ).AsIntegerList();
                qryAttendance = qryAttendance
                    .Where( a =>
                        a.Occurrence.GroupId.HasValue &&
                        groupIdList.Contains( a.Occurrence.GroupId.Value ) );
            }

            // If campuses were included, filter attendances by those that have selected campuses
            // if 'null' is one of the campuses, treat that as a 'CampusId is Null'
            var includeNullCampus = ( campusIds ?? "" ).Split( ',' ).ToList().Any( a => a.Equals( "null", StringComparison.OrdinalIgnoreCase ) );
            var campusIdList = ( campusIds ?? "" ).Split( ',' ).AsIntegerList();

            // remove 0 from the list, just in case it is there 
            campusIdList.Remove( 0 );

            if ( campusIdList.Any() )
            {
                if ( includeNullCampus )
                {
                    // show records that have a campusId in the campusIdsList + records that have a null campusId
                    qryAttendance = qryAttendance.Where( a => ( a.CampusId.HasValue && campusIdList.Contains( a.CampusId.Value ) ) || !a.CampusId.HasValue );
                }
                else
                {
                    // only show records that have a campusId in the campusIdList
                    qryAttendance = qryAttendance.Where( a => a.CampusId.HasValue && campusIdList.Contains( a.CampusId.Value ) );
                }
            }
            else if ( includeNullCampus )
            {
                // 'null' was the only campusId in the campusIds parameter, so only show records that have a null CampusId
                qryAttendance = qryAttendance.Where( a => !a.CampusId.HasValue );
            }

            // If schedules were included, filter attendances by those that have selected schedules
            var scheduleIdList = ( scheduleIds ?? "" ).Split( ',' ).AsIntegerList();
            scheduleIdList.Remove( 0 );
            if ( scheduleIdList.Any() )
            {
                qryAttendance = qryAttendance.Where( a => a.Occurrence.ScheduleId.HasValue && scheduleIdList.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var qryAttendanceWithSummaryDateTime = qryAttendance.GetAttendanceWithSummaryDateTime( groupBy );

            var summaryQry = qryAttendanceWithSummaryDateTime.Select( a => new
            {
                a.SummaryDateTime,
                Campus = new
                {
                    Id = a.Attendance.CampusId,
                    Name = a.Attendance.Campus.Name
                },
                Group = new
                {
                    Id = a.Attendance.Occurrence.GroupId,
                    Name = a.Attendance.Occurrence.Group.Name
                },
                Schedule = new
                {
                    Id = a.Attendance.Occurrence.ScheduleId,
                    Name = a.Attendance.Occurrence.Schedule.Name
                },
                Location = new
                {
                    Id = a.Attendance.Occurrence.LocationId,
                    Name = a.Attendance.Occurrence.Location.Name
                }
            } );

            List<SummaryData> result = null;

            if ( graphBy == AttendanceGraphBy.Total )
            {
                var groupByQry = summaryQry.GroupBy( a => new { a.SummaryDateTime } ).Select( s => new { s.Key, Count = s.Count() } ).OrderBy( o => o.Key );

                result = groupByQry.ToList().Select( a => new SummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesName = "Total",
                    YValue = a.Count
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Campus )
            {
                var groupByQry = summaryQry.GroupBy( a => new { a.SummaryDateTime, Series = a.Campus } ).Select( s => new { s.Key, Count = s.Count() } ).OrderBy( o => o.Key );

                result = groupByQry.ToList().Select( a => new SummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesName = a.Key.Series.Name,
                    YValue = a.Count
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Group )
            {
                var groupByQry = summaryQry.GroupBy( a => new { a.SummaryDateTime, Series = a.Group } ).Select( s => new { s.Key, Count = s.Count() } ).OrderBy( o => o.Key );

                result = groupByQry.ToList().Select( a => new SummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesName = a.Key.Series.Name,
                    YValue = a.Count
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Schedule )
            {
                var groupByQry = summaryQry.GroupBy( a => new { a.SummaryDateTime, Series = a.Schedule } ).Select( s => new { s.Key, Count = s.Count() } ).OrderBy( o => o.Key );

                result = groupByQry.ToList().Select( a => new SummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesName = a.Key.Series.Name,
                    YValue = a.Count
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Location )
            {
                var groupByQry = summaryQry.GroupBy( a => new { a.SummaryDateTime, Series = a.Location } ).Select( s => new { s.Key, Count = s.Count() } ).OrderBy( o => o.Key );

                result = groupByQry.ToList().Select( a => new SummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesName = a.Key.Series.Name,
                    YValue = a.Count
                } ).ToList();
            }

            return result;
        }

        /// <summary>
        /// Gets the attendance analytics attendee dates.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        public DataSet GetAttendanceAnalyticsAttendeeDatesDataSet( List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds )
        {
            var parameters = GetAttendanceAnalyticsParameters( null, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds );
            return new DbService( this.Context ).GetDataSetFromSqlCommand( "spCheckin_AttendanceAnalyticsQuery_AttendeeDates", System.Data.CommandType.StoredProcedure, parameters );
        }

        /// <summary>
        /// Gets the attendance analytics attendee first dates.
        /// </summary>
        /// <param name="GroupTypeIds">The group type ids.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        public DataSet GetAttendanceAnalyticsAttendeeFirstDatesDataSet( List<int> GroupTypeIds, List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds )
        {
            var parameters = GetAttendanceAnalyticsParameters( GroupTypeIds, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds );
            return new DbService( this.Context ).GetDataSetFromSqlCommand( "spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates", System.Data.CommandType.StoredProcedure, parameters );
        }

        /// <summary>
        /// Gets the attendance analytics attendee last attendance.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        public DataSet GetAttendanceAnalyticsAttendeeLastAttendanceDataSet( List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds )
        {
            var parameters = GetAttendanceAnalyticsParameters( null, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds );
            return new DbService( this.Context ).GetDataSetFromSqlCommand( "spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance", System.Data.CommandType.StoredProcedure, parameters );
        }

        /// <summary>
        /// Gets the attendance analytics attendees.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="IncludeParentsWithChild">The include parents with child.</param>
        /// <param name="IncludeChildrenWithParents">The include children with parents.</param>
        /// <returns></returns>
        public DataSet GetAttendanceAnalyticsAttendeesDataSet( List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds, bool? IncludeParentsWithChild, bool? IncludeChildrenWithParents )
        {
            var parameters = GetAttendanceAnalyticsParameters( null, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds, IncludeParentsWithChild, IncludeChildrenWithParents );
            return new DbService( this.Context ).GetDataSetFromSqlCommand( "spCheckin_AttendanceAnalyticsQuery_Attendees", System.Data.CommandType.StoredProcedure, parameters );
        }

        /// <summary>
        /// Gets the attendance analytics non attendees.
        /// </summary>
        /// <param name="GroupTypeIds">The group type ids.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="IncludeParentsWithChild">The include parents with child.</param>
        /// <param name="IncludeChildrenWithParents">The include children with parents.</param>
        /// <returns></returns>
        public DataSet GetAttendanceAnalyticsNonAttendeesDataSet( List<int> GroupTypeIds, List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds, bool? IncludeParentsWithChild, bool? IncludeChildrenWithParents )
        {
            var parameters = GetAttendanceAnalyticsParameters( GroupTypeIds, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds, IncludeParentsWithChild, IncludeChildrenWithParents );
            return new DbService( this.Context ).GetDataSetFromSqlCommand( "spCheckin_AttendanceAnalyticsQuery_NonAttendees", System.Data.CommandType.StoredProcedure, parameters );
        }

        /// <summary>
        /// Gets the attendance analytics parameters.
        /// </summary>
        /// <param name="GroupTypeIds">The group type ids.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="IncludeParentsWithChild">The include parents with child.</param>
        /// <param name="IncludeChildrenWithParents">The include children with parents.</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetAttendanceAnalyticsParameters( List<int> GroupTypeIds, List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds, bool? IncludeParentsWithChild = null, bool? IncludeChildrenWithParents = null )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if ( GroupTypeIds != null && GroupTypeIds.Any() )
            {
                parameters.Add( "GroupTypeIds", GroupTypeIds.AsDelimited( "," ) );
            }

            if ( groupIds != null && groupIds.Any() )
            {
                parameters.Add( "GroupIds", groupIds.AsDelimited( "," ) );
            }

            if ( start.HasValue )
            {
                parameters.Add( "StartDate", start.Value );
            }

            if ( end.HasValue )
            {
                parameters.Add( "EndDate", end.Value );
            }

            if ( campusIds != null )
            {
                parameters.Add( "CampusIds", campusIds.AsDelimited( "," ) );
            }

            if ( includeNullCampusIds.HasValue )
            {
                parameters.Add( "includeNullCampusIds", includeNullCampusIds.Value );
            }

            if ( scheduleIds != null )
            {
                parameters.Add( "ScheduleIds", scheduleIds.AsDelimited( "," ) );
            }

            if ( IncludeParentsWithChild.HasValue )
            {
                parameters.Add( "IncludeParentsWithChild", IncludeParentsWithChild.Value );
            }

            if ( IncludeChildrenWithParents.HasValue )
            {
                parameters.Add( "IncludeChildrenWithParents", IncludeChildrenWithParents.Value );
            }

            return parameters;
        }

        #region Obsolete

        /// <summary>
        /// Gets the attendance analytics attendee dates.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use non-static GetAttendanceAnalyticsAttendeeDatesDataSet instead." )]
        public static DataSet GetAttendanceAnalyticsAttendeeDates( List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds )
        {
            var parameters = GetAttendanceAnalyticsParameters( null, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds );
            return DbService.GetDataSet( "spCheckin_AttendanceAnalyticsQuery_AttendeeDates", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the attendance analytics attendee first dates.
        /// </summary>
        /// <param name="GroupTypeIds">The group type ids.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use non-static GetAttendanceAnalyticsAttendeeFirstDatesDataSet instead." )]
        public static DataSet GetAttendanceAnalyticsAttendeeFirstDates( List<int> GroupTypeIds, List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds )
        {
            var parameters = GetAttendanceAnalyticsParameters( GroupTypeIds, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds );
            return DbService.GetDataSet( "spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the attendance analytics attendee last attendance.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use non-static GetAttendanceAnalyticsAttendeeLastAttendanceDataSet instead." )]
        public static DataSet GetAttendanceAnalyticsAttendeeLastAttendance( List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds )
        {
            var parameters = GetAttendanceAnalyticsParameters( null, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds );
            return DbService.GetDataSet( "spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the attendance analytics attendees.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="IncludeParentsWithChild">The include parents with child.</param>
        /// <param name="IncludeChildrenWithParents">The include children with parents.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use non-static GetAttendanceAnalyticsAttendeesDataSet instead." )]
        public static DataSet GetAttendanceAnalyticsAttendees( List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds, bool? IncludeParentsWithChild, bool? IncludeChildrenWithParents )
        {
            var parameters = GetAttendanceAnalyticsParameters( null, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds, IncludeParentsWithChild, IncludeChildrenWithParents );
            return DbService.GetDataSet( "spCheckin_AttendanceAnalyticsQuery_Attendees", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the attendance analytics non attendees.
        /// </summary>
        /// <param name="GroupTypeIds">The group type ids.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="includeNullCampusIds">The include null campus ids.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="IncludeParentsWithChild">The include parents with child.</param>
        /// <param name="IncludeChildrenWithParents">The include children with parents.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use non-static GetAttendanceAnalyticsNonAttendeesDataSet instead." )]
        public static DataSet GetAttendanceAnalyticsNonAttendees( List<int> GroupTypeIds, List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds, bool? IncludeParentsWithChild, bool? IncludeChildrenWithParents )
        {
            var parameters = GetAttendanceAnalyticsParameters( GroupTypeIds, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds, IncludeParentsWithChild, IncludeChildrenWithParents );
            return DbService.GetDataSet( "spCheckin_AttendanceAnalyticsQuery_NonAttendees", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        #endregion Obsolete

        /// <summary>
        /// 
        /// </summary>
        public class AttendanceWithSummaryDateTime
        {
            /// <summary>
            /// Gets or sets the summary date time.
            /// </summary>
            /// <value>
            /// The summary date time.
            /// </value>
            public DateTime SummaryDateTime { get; set; }

            /// <summary>
            /// Gets or sets the attendance.
            /// </summary>
            /// <value>
            /// The attendance.
            /// </value>
            public Attendance Attendance { get; set; }
        }

        /// <summary>
        /// Sends the scheduled attendance confirmation emails and marks ScheduleConfirmationSent = true, then returns the number of emails sent.
        /// Make sure to call rockContext.SaveChanges() after running this.
        /// NOTE: This doesn't check <see cref="Attendance.ScheduleConfirmationSent" />, so you'll need to add that condition to the sendConfirmationAttendancesQuery parameter
        /// </summary>
        /// <param name="sendConfirmationAttendancesQuery">The send confirmation attendances query.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [Obsolete( "Use SendScheduleConfirmationCommunication instead." )]
        [RockObsolete( "1.13" )]
        public int SendScheduleConfirmationSystemEmails( IQueryable<Attendance> sendConfirmationAttendancesQuery, out List<string> errorMessages )
        {
            int emailsSent = 0;
            errorMessages = new List<string>();

            sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery.Where( a =>
                a.PersonAlias.Person.Email != null
                && a.PersonAlias.Person.Email != string.Empty
                && a.PersonAlias.Person.EmailPreference != EmailPreference.DoNotEmail
                && a.PersonAlias.Person.IsEmailActive );

            var sendConfirmationAttendancesQueryList = sendConfirmationAttendancesQuery.ToList();
            var attendancesBySystemEmailTypeList = sendConfirmationAttendancesQueryList
                .GroupBy( a => a.Occurrence.Group.GroupType.ScheduleConfirmationSystemCommunicationId )
                .Where( a => a.Key.HasValue )
                .Select( s => new
                {
                    ScheduleConfirmationSystemCommunicationId = s.Key.Value,
                    Attendances = s.ToList()
                } ).ToList();

            var rockContext = this.Context as RockContext;

            List<Exception> exceptionList = new List<Exception>();

            foreach ( var attendancesBySystemEmailType in attendancesBySystemEmailTypeList )
            {
                var scheduleConfirmationSystemEmail = new SystemCommunicationService( rockContext ).GetNoTracking( attendancesBySystemEmailType.ScheduleConfirmationSystemCommunicationId );

                var attendancesByPersonList = attendancesBySystemEmailType.Attendances.GroupBy( a => a.PersonAlias.Person ).Select( s => new
                {
                    Person = s.Key,
                    Attendances = s.ToList()
                } );

                foreach ( var attendancesByPerson in attendancesByPersonList )
                {
                    try
                    {
                        var emailMessage = new RockEmailMessage( scheduleConfirmationSystemEmail );
                        var recipient = attendancesByPerson.Person;
                        var attendances = attendancesByPerson.Attendances;

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Attendance", attendances.FirstOrDefault() );
                        mergeFields.Add( "Attendances", attendances );
                        emailMessage.AddRecipient( new RockEmailMessageRecipient( recipient, mergeFields ) );
                        List<string> sendErrors;
                        bool sendSuccess = emailMessage.Send( out sendErrors );

                        if ( sendSuccess )
                        {
                            emailsSent++;
                            foreach ( var attendance in attendances )
                            {
                                attendance.ScheduleConfirmationSent = true;
                            }
                        }
                        else
                        {
                            errorMessages.AddRange( sendErrors );
                        }
                    }
                    catch ( Exception ex )
                    {
                        var emailException = new Exception( $"Exception occurred when trying to send Schedule Confirmation Email to { attendancesByPerson.Person }", ex );
                        errorMessages.Add( emailException.Message );
                        exceptionList.Add( emailException );
                    }
                }
            }

            // group messages that are exactly the same and put a count of those in the message
            errorMessages = errorMessages.GroupBy( a => a ).Select( s => s.Count() > 1 ? $"{s.Key}  ({s.Count()})" : s.Key ).ToList();

            if ( exceptionList.Any() )
            {
                ExceptionLogService.LogException( new AggregateException( "Errors Occurred sending schedule confirmation emails", exceptionList ) );
            }

            return emailsSent;
        }

        /// <summary>
        /// Sends the scheduled attendance reminder emails and marks ScheduleReminderSent = true, then returns the number of emails sent.
        /// </summary>
        /// <param name="sendReminderAttendancesQuery">The send reminder attendances query.</param>
        /// <returns></returns>
        [Obsolete( "Use SendScheduleReminderSystemCommunication instead." )]
        [RockObsolete( "1.13" )]
        public int SendScheduleReminderSystemEmails( IQueryable<Attendance> sendReminderAttendancesQuery )
        {
            int emailsSent = 0;
            var sendReminderAttendancesQueryList = sendReminderAttendancesQuery.ToList();
            var attendancesBySystemEmailTypeList = sendReminderAttendancesQueryList
                .GroupBy( a => a.Occurrence.Group.GroupType.ScheduleReminderSystemCommunicationId )
                .Where( a => a.Key.HasValue ).Select( s => new
                {
                    ScheduleReminderSystemCommunicationId = s.Key.Value,
                    Attendances = s.ToList()
                } )
                .ToList();

            var rockContext = this.Context as RockContext;

            foreach ( var attendancesBySystemEmailType in attendancesBySystemEmailTypeList )
            {
                var scheduleReminderSystemEmail = new SystemCommunicationService( rockContext ).GetNoTracking( attendancesBySystemEmailType.ScheduleReminderSystemCommunicationId );

                var attendancesByPersonList = attendancesBySystemEmailType.Attendances.GroupBy( a => a.PersonAlias.Person ).Select( s => new
                {
                    Person = s.Key,
                    Attendances = s.ToList()
                } );

                foreach ( var attendancesByPerson in attendancesByPersonList )
                {
                    try
                    {
                        var emailMessage = new RockEmailMessage( scheduleReminderSystemEmail );
                        var recipient = attendancesByPerson.Person;
                        var attendances = attendancesByPerson.Attendances;

                        foreach ( var attendance in attendances )
                        {
                            attendance.ScheduleReminderSent = true;
                        }

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Attendance", attendances.FirstOrDefault() );
                        mergeFields.Add( "Attendances", attendances );
                        emailMessage.AddRecipient( new RockEmailMessageRecipient( recipient, mergeFields ) );
                        emailMessage.Send();
                        emailsSent++;
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to send SendScheduleReminderSystemEmails to { attendancesByPerson.Person }", ex ) );
                    }
                }
            }

            return emailsSent;
        }

        /// <summary>
        /// Sends the schedule confirmation communication.
        /// </summary>
        /// <param name="sendConfirmationAttendancesQuery">The send confirmation attendances query.</param>
        /// <returns></returns>
        public SendMessageResult SendScheduleConfirmationCommunication( IQueryable<Attendance> sendConfirmationAttendancesQuery )
        {
            sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery
                .Where( a =>
                    ( a.PersonAlias.Person.Email != null
                    && a.PersonAlias.Person.Email != string.Empty
                    && a.PersonAlias.Person.EmailPreference != EmailPreference.DoNotEmail
                    && a.PersonAlias.Person.IsEmailActive )
                    || a.PersonAlias.Person.PhoneNumbers.Any( ph => ph.IsMessagingEnabled ) )
                .Where( a => a.Occurrence.Group.GroupType.ScheduleConfirmationSystemCommunicationId.HasValue );

            var sendConfirmationAttendancesQueryList = sendConfirmationAttendancesQuery.ToList();

            var sendConfirmationIndividuals = sendConfirmationAttendancesQueryList
                .GroupBy( a =>
                {
                    {
                        CommunicationType communicationPreference;
                        var groupMember = a.Occurrence.Group.Members.Where( gm => gm.PersonId == a.PersonAlias.PersonId ).FirstOrDefault();
                        if ( groupMember != null )
                        {
                            communicationPreference = groupMember.CommunicationPreference;
                        }
                        else
                        {
                            // person isn't a member of the group, so just use their Person.CommunicationPreference
                            communicationPreference = a.PersonAlias?.Person?.CommunicationPreference ?? CommunicationType.Email;
                        }

                        var groupBy = new
                        {
                            a.PersonAlias.Person,
                            CommunicationPreference = communicationPreference,
                            a.Occurrence.Group.GroupType.ScheduleConfirmationSystemCommunicationId
                        };

                        return groupBy;
                    }
                } )
                .Select( s => new SendSystemCommunicationIndividual
                {
                    SystemCommunicationId = s.Key.ScheduleConfirmationSystemCommunicationId.Value,
                    Individual = s.Key.Person,
                    GroupCommunicationPreference = s.Key.CommunicationPreference,
                    Attendances = s.ToList()
                } );

            var sendMessageResults = SendSystemCommunications( sendConfirmationIndividuals, ( attendance ) => attendance.ScheduleConfirmationSent = true );

            // group messages that are exactly the same and put a count of those in the message
            sendMessageResults.Errors = sendMessageResults.Errors.GroupBy( a => a ).Select( s => s.Count() > 1 ? $"{s.Key}  ({s.Count()})" : s.Key ).ToList();

            if ( sendMessageResults.Exceptions.Any() )
            {
                ExceptionLogService.LogException( new AggregateException( "Errors Occurred sending schedule confirmations", sendMessageResults.Exceptions ) );
            }

            return sendMessageResults;
        }

        /// <summary>
        /// Sends the schedule reminder system communication.
        /// </summary>
        /// <param name="sendReminderAttendancesQuery">The send reminder attendances query.</param>
        /// <returns></returns>
        public SendMessageResult SendScheduleReminderSystemCommunication( IQueryable<Attendance> sendReminderAttendancesQuery )
        {
            sendReminderAttendancesQuery = sendReminderAttendancesQuery
                .Where( a =>
                    ( a.PersonAlias.Person.Email != null
                    && a.PersonAlias.Person.Email != string.Empty
                    && a.PersonAlias.Person.EmailPreference != EmailPreference.DoNotEmail
                    && a.PersonAlias.Person.IsEmailActive )
                    || a.PersonAlias.Person.PhoneNumbers.Any( ph => ph.IsMessagingEnabled ) )
                .Where( a => a.Occurrence.Group.GroupType.ScheduleReminderSystemCommunicationId.HasValue );

            var sendReminderAttendancesQueryList = sendReminderAttendancesQuery.ToList();
            var sendReminderIndividuals = sendReminderAttendancesQueryList
                .GroupBy( a =>
                {
                    {
                        CommunicationType communicationPreference;
                        var groupMember = a.Occurrence.Group.Members.Where( gm => gm.PersonId == a.PersonAlias.PersonId ).FirstOrDefault();
                        if ( groupMember != null )
                        {
                            communicationPreference = groupMember.CommunicationPreference;
                        }
                        else
                        {
                            // person isn't a member of the group, so just use their Person.CommunicationPreference
                            communicationPreference = a.PersonAlias?.Person?.CommunicationPreference ?? CommunicationType.Email;
                        }

                        var groupBy = new
                        {
                            a.PersonAlias.Person,
                            CommunicationPreference = communicationPreference,
                            a.Occurrence.Group.GroupType.ScheduleReminderSystemCommunicationId
                        };

                        return groupBy;
                    }
                } )
                .Select( s => new SendSystemCommunicationIndividual
                {
                    SystemCommunicationId = s.Key.ScheduleReminderSystemCommunicationId.Value,
                    Individual = s.Key.Person,
                    GroupCommunicationPreference = s.Key.CommunicationPreference,
                    Attendances = s.ToList()
                } );

            var sendMessageResults = SendSystemCommunications( sendReminderIndividuals, ( attendance ) => attendance.ScheduleReminderSent = true );

            // group messages that are exactly the same and put a count of those in the message
            sendMessageResults.Errors = sendMessageResults.Errors.GroupBy( a => a ).Select( s => s.Count() > 1 ? $"{s.Key}  ({s.Count()})" : s.Key ).ToList();

            if ( sendMessageResults.Exceptions.Any() )
            {
                ExceptionLogService.LogException( new AggregateException( "Errors Occurred sending schedule reminders.", sendMessageResults.Exceptions ) );
            }

            return sendMessageResults;
        }

        private SendMessageResult SendSystemCommunications( IEnumerable<SendSystemCommunicationIndividual> sendSystemCommunicationIndividuals, Action<Attendance> updateAttendanceRecord )
        {
            var communicationMap = new Dictionary<int, SystemCommunication>();
            var rockContext = this.Context as RockContext;
            var sendMessageResults = new SendMessageResult();

            foreach ( var individualNotification in sendSystemCommunicationIndividuals )
            {
                SystemCommunication communicationMessage = null;
                if ( !communicationMap.TryGetValue( individualNotification.SystemCommunicationId, out communicationMessage ) )
                {
                    var scheduleConfirmationSystemCommunicaiton = new SystemCommunicationService( rockContext )
                        .GetNoTracking( individualNotification.SystemCommunicationId );
                    communicationMessage = scheduleConfirmationSystemCommunicaiton;
                    communicationMap.Add( individualNotification.SystemCommunicationId, communicationMessage );
                }

                var recipient = individualNotification.Individual;
                var attendances = individualNotification.Attendances;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Attendance", attendances.FirstOrDefault() );
                mergeFields.Add( "Attendances", attendances );

                var forceCommunicationType = CommunicationType.RecipientPreference;
                var validSmsTemplateExists = communicationMessage.SMSMessage.IsNotNullOrWhiteSpace() && communicationMessage.SMSFromDefinedValueId != null;
                var individualHasValidSmsNumber = recipient.PhoneNumbers.Any( ph => ph.IsMessagingEnabled );
                if ( !validSmsTemplateExists || !individualHasValidSmsNumber )
                {
                    forceCommunicationType = CommunicationType.Email;
                }

                var mediumType = Communication.DetermineMediumEntityTypeId(
                                   ( int ) CommunicationType.Email,
                                   ( int ) CommunicationType.SMS,
                                   ( int ) CommunicationType.PushNotification,
                                   forceCommunicationType,
                                   individualNotification.GroupCommunicationPreference,
                                   individualNotification.Individual.CommunicationPreference );
                try
                {
                    var sendIndividualMessageResult = CommunicationHelper.SendMessage( individualNotification.Individual, mediumType, communicationMessage, mergeFields );

                    sendMessageResults.Errors.AddRange( sendIndividualMessageResult.Errors );
                    sendMessageResults.Warnings.AddRange( sendIndividualMessageResult.Warnings );

                    if ( sendIndividualMessageResult.MessagesSent > 0 )
                    {
                        sendMessageResults.MessagesSent += sendIndividualMessageResult.MessagesSent;
                        foreach ( var attendance in attendances )
                        {
                            updateAttendanceRecord( attendance );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    var emailException = new Exception( $"Exception occurred when trying to send Schedule Confirmation Email to { individualNotification.Individual }", ex );
                    sendMessageResults.Errors.Add( emailException.Message );
                    sendMessageResults.Exceptions.Add( emailException );
                }
            }

            return sendMessageResults;
        }

        /// <summary>
        /// This class is used by the send schedule communication functions so the data can be passed to a single shared method.
        /// </summary>
        private class SendSystemCommunicationIndividual
        {
            /// <summary>
            /// Gets or sets the individual.
            /// </summary>
            /// <value>
            /// The individual.
            /// </value>
            public Person Individual { get; set; }

            /// <summary>
            /// Gets or sets the group communication preference.
            /// </summary>
            /// <value>
            /// The group communication preference.
            /// </value>
            public CommunicationType GroupCommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets the system communication identifier.
            /// </summary>
            /// <value>
            /// The system communication identifier.
            /// </value>
            public int SystemCommunicationId { get; set; }

            /// <summary>
            /// Gets or sets the attendances.
            /// </summary>
            /// <value>
            /// The attendances.
            /// </value>
            public List<Attendance> Attendances { get; set; }
        }

        #region GroupScheduling Related

        /// <summary>
        /// Gets the person schedule exclusion query for occurrence dates.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="occurrenceDateList">The occurrence date list.</param>
        /// <returns></returns>
        private IQueryable<PersonScheduleExclusion> GetPersonScheduleExclusionQueryForOccurrenceDates( int groupId, List<DateTime> occurrenceDateList )
        {
            var rockContext = this.Context as RockContext;
            var personScheduleExclusionQueryForOccurrenceDates = new PersonScheduleExclusionService( rockContext ).Queryable()
                .Where( a => !a.GroupId.HasValue || ( a.GroupId.Value == groupId ) );

            if ( occurrenceDateList.Count == 1 )
            {
                var occurrenceDate = occurrenceDateList.First();
                personScheduleExclusionQueryForOccurrenceDates = personScheduleExclusionQueryForOccurrenceDates.Where( a => occurrenceDate >= a.StartDate && occurrenceDate <= a.EndDate );
            }
            else
            {
                personScheduleExclusionQueryForOccurrenceDates = personScheduleExclusionQueryForOccurrenceDates.Where( a => occurrenceDateList.Any( occurrenceDate => occurrenceDate >= a.StartDate && occurrenceDate <= a.EndDate ) );
            }

            return personScheduleExclusionQueryForOccurrenceDates;
        }

        /// <summary>
        /// Gets a list of available resources (people) based on the options specified in schedulerResourceParameters
        /// <para>If the source of resources is a group, only active group members will be included</para>
        /// </summary>
        /// <param name="schedulerResourceParameters">The scheduler resource parameters.</param>
        /// <returns></returns>
        public IEnumerable<SchedulerResource> GetSchedulerResources( SchedulerResourceParameters schedulerResourceParameters )
        {
            var rockContext = this.Context as RockContext;

            var occurrenceSchedules = new ScheduleService( rockContext )
                .GetByIds( schedulerResourceParameters.AttendanceOccurrenceScheduleIds.ToList() )
                .AsNoTracking()
                .ToList();

            List<SchedulerResource> schedulerResourceList = new List<SchedulerResource>();

            if ( !occurrenceSchedules.Any() )
            {
                // no schedules found, return empty list
                return schedulerResourceList;
            }

            var occurrenceFirstDayOfWeek = schedulerResourceParameters.AttendanceOccurrenceSundayDate.StartOfWeek( RockDateTime.FirstDayOfWeek );
            var occurrenceLastDayOfWeek = schedulerResourceParameters.AttendanceOccurrenceSundayDate.EndOfWeek( RockDateTime.FirstDayOfWeek );

            // don't include schedule dates in the past
            if ( occurrenceFirstDayOfWeek <= RockDateTime.Today )
            {
                occurrenceFirstDayOfWeek = RockDateTime.Today;
            }

            // get all the occurrences for the selected week for the selected schedules (It could be more than once a week if it is a daily scheduled, or it might not be in the selected week if it is every 2 weeks, etc)
            List<DateTime> scheduleOccurrenceDateTimeList = new List<DateTime>();
            Dictionary<int, int> scheduleOccurrenceCountByScheduleId = new Dictionary<int, int>();
            foreach ( var occurrenceSchedule in occurrenceSchedules )
            {
                var occurrenceStartTimes = occurrenceSchedule.GetScheduledStartTimes( occurrenceFirstDayOfWeek, occurrenceLastDayOfWeek.AddDays( 1 ) );
                scheduleOccurrenceCountByScheduleId.AddOrReplace( occurrenceSchedule.Id, occurrenceStartTimes.Count );
                scheduleOccurrenceDateTimeList.AddRange( occurrenceStartTimes );
            }

            int timeSlotCount = scheduleOccurrenceDateTimeList.Count;

            scheduleOccurrenceDateTimeList = scheduleOccurrenceDateTimeList.Distinct().ToList();

            if ( !scheduleOccurrenceDateTimeList.Any() )
            {
                // no occurrences for the selected week, so just return an empty list
                return schedulerResourceList;
            }

            var groupMemberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );
            var attendanceService = new AttendanceService( rockContext );
            IQueryable<GroupMember> groupMemberQry = null;
            IQueryable<Person> personQry = null;

            HashSet<int> groupMemberIdsThatLackGroupRequirements;

            groupMemberIdsThatLackGroupRequirements = null;
            GroupTypeCache resourceGroupGroupType = null;

            if ( schedulerResourceParameters.ResourceGroupId.HasValue )
            {
                groupMemberQry = groupMemberService.Queryable()
                    .Where( a => a.GroupId == schedulerResourceParameters.ResourceGroupId.Value )
                    .Where( a => a.GroupMemberStatus == GroupMemberStatus.Active );

                var resourceGroup = groupService.GetNoTracking( schedulerResourceParameters.ResourceGroupId.Value );
                if ( resourceGroup != null )
                {
                    resourceGroupGroupType = GroupTypeCache.Get( resourceGroup.GroupTypeId );
                }

                if ( resourceGroup?.SchedulingMustMeetRequirements == true )
                {
                    groupMemberIdsThatLackGroupRequirements = new HashSet<int>( new GroupService( rockContext ).GroupMembersNotMeetingRequirements( resourceGroup, false ).Select( a => a.Key.Id ).ToList().Distinct() );
                }
            }

            if ( schedulerResourceParameters.ResourceDataViewId.HasValue )
            {
                var dataView = new DataViewService( rockContext ).Get( schedulerResourceParameters.ResourceDataViewId.Value );

                if ( dataView != null )
                {
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs
                    {
                        DbContext = rockContext
                    };

                    personQry = dataView.GetQuery( dataViewGetQueryArgs ) as IQueryable<Person>;
                }
            }

            var scheduleOccurrenceDateList = scheduleOccurrenceDateTimeList.Select( a => a.Date ).Distinct().ToList();

            if ( groupMemberQry != null )
            {
                if ( schedulerResourceParameters.LimitToPersonId.HasValue )
                {
                    groupMemberQry = groupMemberQry.Where( a => a.PersonId == schedulerResourceParameters.LimitToPersonId.Value );
                }

                var resourceListQuery = groupMemberQry.AsNoTracking().Select( a => new ResourceListItem
                {
                    GroupMemberId = a.Id,
                    GroupRoleId = a.GroupRoleId,
                    PersonId = a.PersonId,
                    Note = a.Note,
                    NickName = a.Person.NickName,
                    LastName = a.Person.LastName,
                    SuffixValueId = a.Person.SuffixValueId,
                    RecordTypeValueId = a.Person.RecordTypeValueId,
                    ScheduleTemplateId = a.ScheduleTemplateId,
                    ScheduleStartDate = a.ScheduleStartDate,
                    MemberAssignments = a.GroupMemberAssignments
                        .Where( x => x.ScheduleId.HasValue ).Select( s => new MemberAssignmentInfo
                        {
                            Schedule = s.Schedule,
                            LocationId = s.LocationId,
                            LocationName = s.LocationId.HasValue ? s.Location.Name : null
                        } )
                } );

                // if using thes filters, limit to people that have ScheduleTemplates that would include the scheduled date
                if ( schedulerResourceParameters.GroupMemberFilterType == SchedulerResourceGroupMemberFilterType.ShowMatchingPreference
                    || schedulerResourceParameters.ResourceListSourceType == GroupSchedulerResourceListSourceType.GroupMatchingPreference
                    || schedulerResourceParameters.ResourceListSourceType == GroupSchedulerResourceListSourceType.GroupMatchingAssignment )
                {
                    resourceListQuery = resourceListQuery.Where( a => a.ScheduleTemplateId.HasValue && a.ScheduleStartDate.HasValue );
                }

                // For the GroupMatchingAssignment option filter by the provided location and schedule criteria.
                if ( schedulerResourceParameters.ResourceListSourceType == GroupSchedulerResourceListSourceType.GroupMatchingAssignment )
                {
                    var locationParam = schedulerResourceParameters.AttendanceOccurrenceLocationIds?.ToList() ?? new List<int>();
                    if ( locationParam.Any() )
                    {
                        resourceListQuery = resourceListQuery.Where( a => a.MemberAssignments.All( ma => locationParam.Contains( ma.LocationId.Value ) ) || a.MemberAssignments.All( ma => ma.LocationId == null ) );
                    }

                    var scheduleParam = schedulerResourceParameters.AttendanceOccurrenceScheduleIds?.ToList() ?? new List<int>();
                    if ( scheduleParam.Any() )
                    {
                        resourceListQuery = resourceListQuery.Where( a => a.MemberAssignments.All( ma => scheduleParam.Contains( ma.Schedule.Id ) ) || a.MemberAssignments.All( ma => ma.Schedule == null ) );
                    }
                }

                var resourceList = resourceListQuery.ToList();

                // if ShowMatchingPreference, narrow it down even more to ones where their Schedule Preference (EveryWeek, EveryOtherWeek, etc) lands during this sunday week
                if ( schedulerResourceParameters.GroupMemberFilterType == SchedulerResourceGroupMemberFilterType.ShowMatchingPreference )
                {
                    // get the scheduleTemplateIds that the groupMemberList has (so we only fetch the ones we need)
                    Dictionary<int, Schedule> scheduleTemplateLookup;

                    List<int> scheduleTemplateIdList = resourceList.Where( a => a.ScheduleTemplateId.HasValue ).Select( a => a.ScheduleTemplateId.Value ).Distinct().ToList();

                    if ( scheduleTemplateIdList.Any() )
                    {
                        scheduleTemplateLookup = new GroupMemberScheduleTemplateService( rockContext )
                            .GetByIds( scheduleTemplateIdList )
                            .Select( a => new { a.Id, a.Schedule } )
                            .AsNoTracking()
                            .ToList().ToDictionary( a => a.Id, k => k.Schedule );
                    }
                    else
                    {
                        scheduleTemplateLookup = new Dictionary<int, Schedule>();
                    }

                    TimeSpan occurrenceScheduledTime = scheduleOccurrenceDateList.First().TimeOfDay;

                    List<int> matchingScheduleGroupMemberIdList = new List<int>();

                    // get first scheduled occurrence for the selected "sunday week", which would be from the start of the configured RockDateTime.FirstDayOfWeek
                    var beginDateTime = occurrenceFirstDayOfWeek;
                    var endDateTime = occurrenceLastDayOfWeek.AddDays( 1 );

                    foreach ( var groupMember in resourceList )
                    {
                        Schedule schedule = scheduleTemplateLookup.GetValueOrNull( groupMember.ScheduleTemplateId.Value );
                        if ( schedule != null )
                        {
                            var scheduleStartDateTimeOverride = groupMember.ScheduleStartDate.Value.Add( occurrenceScheduledTime );
                            var matches = schedule.GetICalOccurrences( beginDateTime, endDateTime, scheduleStartDateTimeOverride );
                            if ( matches.Any() )
                            {
                                matchingScheduleGroupMemberIdList.Add( groupMember.GroupMemberId );
                            }
                        }
                    }

                    resourceList = resourceList.Where( a => matchingScheduleGroupMemberIdList.Contains( a.GroupMemberId ) ).ToList();
                }

                var groupTypeRoleCacheLookup = resourceGroupGroupType?.Roles.ToDictionary( k => k.Id, v => v );

                schedulerResourceList = resourceList.Select( a => new SchedulerResource
                {
                    PersonId = a.PersonId,

                    /* 2020-07-17 MDP  
                     *  This is their GroupMember information from the *ResourceGroup*, and not always the Occurrence Group. 
                     *  This is how the Group Member information displayed in the Resources Lists is determined:
                     *  - Group Member (Or Group Member Matching Preference)
                     *    - Occurrence Group
                     *  - Alternate Group
                     *    - Alternate Group
                     *  - Parent Group
                     *    - Parent Group
                     *   - Dataview
                     *     - NULL (no group member information)
                     *   - Additional Person Ids
                     *     -  NULL (no group member information)
                     */
                    GroupMemberId = a.GroupMemberId,
                    GroupRole = groupTypeRoleCacheLookup.GetValueOrNull( a.GroupRoleId ),
                    ResourcePreferenceList = a.MemberAssignments?
                        .OrderBy( r => r.Schedule.Order )
                        .ThenBy( r => r.Schedule.GetNextStartDateTime( RockDateTime.Now.Date ) )
                        .Select( x => new SchedulerResourcePreference
                        {
                            ScheduleId = x.Schedule.Id,
                            ScheduleName = x.Schedule.Name,
                            LocationId = x.LocationId,
                            LocationName = x.LocationName
                        } )
                    .ToList(),

                    Note = a.Note,
                    PersonNickName = a.NickName,
                    PersonLastName = a.LastName,
                    PersonName = Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    HasGroupRequirementsConflict = groupMemberIdsThatLackGroupRequirements?.Contains( a.GroupMemberId ) ?? false,
                } ).ToList();
            }
            else if ( personQry != null )
            {
                if ( schedulerResourceParameters.LimitToPersonId.HasValue )
                {
                    personQry = personQry.Where( a => a.Id == schedulerResourceParameters.LimitToPersonId.Value );
                }

                var resourceList = personQry.Select( a => new
                {
                    a.Id,
                    a.NickName,
                    a.LastName,
                    a.SuffixValueId,
                    a.RecordTypeValueId,
                } ).ToList();

                schedulerResourceList = resourceList.Select( a => new SchedulerResource
                {
                    PersonId = a.Id,

                    // Since this is a dataview of Person, there isn't a MemberRecord, so there is no MemberId, ResourceAssignment or Note
                    GroupMemberId = null,
                    GroupRole = null,
                    ResourcePreferenceList = null,
                    Note = null,

                    PersonNickName = a.NickName,
                    PersonLastName = a.LastName,
                    PersonName = Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    HasGroupRequirementsConflict = false,
                } ).ToList();
            }

            // get any additionalPersonIds that aren't already included in the resource list
            var additionalPersonIds = schedulerResourceParameters.ResourceAdditionalPersonIds?.Where( a => !schedulerResourceList.Any( r => r.PersonId == a ) ).ToList();

            if ( additionalPersonIds?.Any() == true )
            {
                var additionalSchedulerResources = new PersonService( rockContext ).GetByIds( additionalPersonIds ).ToList().Select( a => new SchedulerResource
                {
                    PersonId = a.Id,

                    // Since this is a from manually selected PersonIds, there isn't a MemberRecord, so there is no MemberId, ResourceAssignment or Note
                    GroupMemberId = null,
                    GroupRole = null,
                    ResourcePreferenceList = null,
                    PersonNickName = a.NickName,
                    PersonLastName = a.LastName,
                    PersonName = a.FullName,
                    HasGroupRequirementsConflict = false,
                } ).ToList();

                schedulerResourceList.AddRange( additionalSchedulerResources );
            }

            if ( !schedulerResourceList.Any() )
            {
                return schedulerResourceList;
            }
            else
            {
                IQueryable<PersonScheduleExclusion> personScheduleExclusionQueryForOccurrenceDates = GetPersonScheduleExclusionQueryForOccurrenceDates( schedulerResourceParameters.AttendanceOccurrenceGroupId, scheduleOccurrenceDateList );

                // Get the last time they attended the group (in any location or schedule)
                var lastAttendedDateTimeQuery = attendanceService.Queryable()
                    .Where( a => a.DidAttend == true
                        && a.Occurrence.GroupId == schedulerResourceParameters.AttendanceOccurrenceGroupId
                        && a.PersonAliasId.HasValue );

                if ( groupMemberQry != null )
                {
                    lastAttendedDateTimeQuery = lastAttendedDateTimeQuery.Where( a => groupMemberQry.Any( m => m.PersonId == a.PersonAlias.PersonId ) );
                    personScheduleExclusionQueryForOccurrenceDates = personScheduleExclusionQueryForOccurrenceDates.Where( a => groupMemberQry.Any( m => m.PersonId == a.PersonAlias.PersonId ) );
                }
                else if ( personQry != null )
                {
                    lastAttendedDateTimeQuery = lastAttendedDateTimeQuery.Where( a => personQry.Any( p => p.Id == a.PersonAlias.PersonId ) );
                    personScheduleExclusionQueryForOccurrenceDates = personScheduleExclusionQueryForOccurrenceDates.Where( a => personQry.Any( p => p.Id == a.PersonAlias.PersonId ) );
                }

                var personIdLastAttendedDateTimeLookup = lastAttendedDateTimeQuery
                    .GroupBy( a => a.PersonAlias.PersonId )
                    .Select( a => new
                    {
                        PersonId = a.Key,
                        LastScheduledDate = a.Max( x => x.StartDateTime )
                    } )
                    .ToDictionary( k => k.PersonId, v => v.LastScheduledDate );

                /*
                 * BJW 03-20-2020
                 *
                 * I added the where clauses in the following query that limit to active schedules or locations. The intent was to
                 * prevent people from being rescheduled that are scheduled for a location or schedule where that location or
                 * schedule has since become inactive.
                 *
                 * Consider this scenario:
                 * - Ted is scheduled for AV on Sunday at 12
                 * - The Sunday schedule for 12 is marked inactive
                 * - Alisha comes to the scheduler block to put Ted in another schedule but she cannot because:
                 *      - Ted is "already scheduled" and doesn't show in the available people panel
                 *      - Alisha cannot remove Ted's now invalid assignment because she can no longer see the inactive schedule
                 */

                var scheduleIds = schedulerResourceParameters.AttendanceOccurrenceScheduleIds;

                var scheduledAttendanceOccurrencesLookupQuery = attendanceService.Queryable()
                   .Where( a =>
                   ( a.RequestedToAttend == true || a.ScheduledToAttend == true )

                    && a.Occurrence.GroupId.HasValue
                    && a.Occurrence.ScheduleId.HasValue )
                    .WhereDeducedIsActive();

                if ( scheduleIds.Count() == 1 )
                {
                    // if there is only 1 schedule, so an equal instead of a contains so that the LINQ expression is a little simplier
                    var scheduleId = scheduleIds[0];
                    scheduledAttendanceOccurrencesLookupQuery = scheduledAttendanceOccurrencesLookupQuery.Where( a => a.Occurrence.ScheduleId == scheduleId );
                }
                else
                {
                    scheduledAttendanceOccurrencesLookupQuery = scheduledAttendanceOccurrencesLookupQuery.Where( a => scheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
                }

                if ( scheduleOccurrenceDateList.Count == 1 )
                {
                    // if there is only 1 scheduleOccurrenceDate, so an equal instead of a contains so that the LINQ expression is a little simplier
                    var scheduleOccurrenceDate = scheduleOccurrenceDateList[0];
                    scheduledAttendanceOccurrencesLookupQuery = scheduledAttendanceOccurrencesLookupQuery.Where( a => a.Occurrence.OccurrenceDate == scheduleOccurrenceDate );
                }
                else
                {
                    scheduledAttendanceOccurrencesLookupQuery = scheduledAttendanceOccurrencesLookupQuery.Where( a => scheduleOccurrenceDateList.Contains( a.Occurrence.OccurrenceDate ) );
                }

                /* MP 2020-06-18
                 * Updated query to use an inner join against PersonAlias/Person to improve performance.
                 * Also updated personScheduleExclusionLookupByPersonId so that it gets the data from SQL
                 * but does the GroupBy in C#. This improves the performance quite a bit. For example, in a case where
                 * there are a large number of people (40,000+) scheduled,
                 * the query returns in less than a second vs timing out
                 */

                // Create explicit joins to person alias and person tables so that rendered SQL has an INNER Joins vs OUTER joins on Person and PersonAlias
                var personAliasQry = new PersonAliasService( rockContext ).Queryable();
                var personQryForJoin = new PersonService( rockContext ).Queryable();

                // load personId/occurrence into a list before doing a GroupBy
                var scheduledAttendancePersonIdOccurrencesQuery = scheduledAttendanceOccurrencesLookupQuery
                    .Join(
                        personAliasQry,
                        a => a.PersonAliasId,
                        pa => pa.Id,
                        ( a, pa ) => new
                        {
                            PersonId = pa.PersonId,
                            GroupId = a.Occurrence.GroupId,
                            GroupName = a.Occurrence.Group.Name,
                            ScheduleId = a.Occurrence.ScheduleId.Value,
                            ScheduleName = a.Occurrence.Schedule.Name,
                            LocationId = a.Occurrence.LocationId,
                            LocationName = a.Occurrence.Location.Name,
                            OccurrenceDate = a.Occurrence.OccurrenceDate
                        } );

                var scheduledAttendancePersonIdOccurrencesList = scheduledAttendancePersonIdOccurrencesQuery.ToList();

                // now that we have a list from the database, use C# to have a dictionary of groupIds for each person
                var scheduledAttendancePersonIdOccurrencesLookup = scheduledAttendancePersonIdOccurrencesList
                    .GroupBy( a => a.PersonId )
                    .ToDictionary(
                        k => k.Key,
                        v => v.Select( x => new SchedulerResourceScheduled
                        {
                            GroupId = x.GroupId.Value,
                            GroupName = x.GroupName,
                            LocationId = x.LocationId,
                            LocationName = x.LocationName,
                            ScheduleId = x.ScheduleId,
                            ScheduleName = x.ScheduleName,
                            OccurrenceDate = x.OccurrenceDate
                        } ).ToList() );

                var personScheduleExclusionLookupByPersonId = personScheduleExclusionQueryForOccurrenceDates
                    .Select( s => new { s.PersonAlias.PersonId, s.StartDate, s.EndDate } ).ToList()
                    .GroupBy( a => a.PersonId )
                    .ToDictionary( k => k.Key, v => new DateRange { Start = v.Min( x => x.StartDate ), End = v.Max( x => x.EndDate ) } );

                foreach ( var schedulerResource in schedulerResourceList )
                {
                    schedulerResource.LastAttendanceDateTime = personIdLastAttendedDateTimeLookup.GetValueOrNull( schedulerResource.PersonId );
                    List<SchedulerResourceScheduled> scheduledForOccurrences = scheduledAttendancePersonIdOccurrencesLookup.GetValueOrNull( schedulerResource.PersonId );

                    schedulerResource.ConfirmationStatus = ScheduledAttendanceItemStatus.Unscheduled.ConvertToString( false ).ToLower();

                    // see if they are scheduled for some other group during this occurrence
                    schedulerResource.SchedulingConflicts = scheduledForOccurrences?.Where( ao => ao.GroupId != schedulerResourceParameters.AttendanceOccurrenceGroupId ).ToList();

                    // get list of places where this person is already scheduled for this Group
                    List<SchedulerResourceScheduled> schedulerResourceScheduledList = scheduledForOccurrences
                        ?.Where( ao => ao.GroupId == schedulerResourceParameters.AttendanceOccurrenceGroupId )
                        .Select( ao => new SchedulerResourceScheduled
                        {
                            GroupId = ao.GroupId,
                            GroupName = ao.GroupName,
                            LocationId = ao.LocationId,
                            LocationName = ao.LocationName,
                            ScheduleId = ao.ScheduleId,
                            ScheduleName = ao.ScheduleName,
                            OccurrenceDate = ao.OccurrenceDate
                        } ).ToList();

                    if ( schedulerResourceScheduledList != null )
                    {
                        // just in case there is a schedule that occurs multiple times a week, include the Day Name in the Schedule to clarify which day the person is scheduled for
                        var multidayScheduledList = schedulerResourceScheduledList.Where( s => scheduleOccurrenceCountByScheduleId.GetValueOrDefault( s.ScheduleId, 0 ) > 1 ).ToList();
                        foreach ( var multidayScheduled in multidayScheduledList )
                        {
                            // format as 'Daily at 7pm (Mon)'
                            multidayScheduled.ScheduleName = $"{multidayScheduled.ScheduleName} ({multidayScheduled.OccurrenceDate.ToString( "ddd" )})";
                        }
                    }

                    schedulerResource.ResourceScheduledList = schedulerResourceScheduledList;

                    // see if they are scheduled for this group already for this occurrence
                    schedulerResource.IsAlreadyScheduledForGroup = schedulerResourceScheduledList?.Any() ?? false;

                    DateRange personExclusionDateRange = personScheduleExclusionLookupByPersonId.GetValueOrNull( schedulerResource.PersonId );

                    if ( personExclusionDateRange != null )
                    {
                        schedulerResource.BlackoutDates = scheduleOccurrenceDateList.Where( d => personExclusionDateRange.Contains( d ) ).ToList();
                    }

                    // if there is more than one time slot in the view, they'll stay in the resource list after
                    schedulerResource.DisplayedDaysCount = scheduleOccurrenceDateList.Count();
                    schedulerResource.DisplayedTimeSlotCount = timeSlotCount;
                }

                // If there is only one occurrence for the selected week, remove anybody that is already scheduled for this group, and sort by person
                // However, if there are multiple occurrences, don't exclude them so the same person can be scheduled for multiple days. (if get dragged into the same occurrence more than once, the logic will see they are already scheduled and ignore it)
                schedulerResourceList = schedulerResourceList
                    .Where( a => a.DisplayedTimeSlotCount > 1 || ( a.IsAlreadyScheduledForGroup != true ) ).ToList();

                // sort the ResourceList first by the GroupRole.Order (so that Leaders are listed at the top, followed by Members, followed by non-group members)
                // then sort by LastName/FirstName
                schedulerResourceList = schedulerResourceList
                    .OrderBy( a => a.GroupRole?.Order ?? int.MaxValue )
                    .ThenBy( a => a.PersonLastName )
                    .ThenBy( a => a.PersonNickName )
                    .ThenBy( a => a.PersonId ).ToList();

                return schedulerResourceList;
            }
        }

        /// <summary>
        /// Gets a list of SchedulerResourceAttend records 
        /// </summary>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        /// <returns></returns>
        public IEnumerable<SchedulerResourceAttend> GetAttendingSchedulerResources( int attendanceOccurrenceId )
        {
            // Only count occurrences with active locations and active schedules as conflicting.
            var conflictingScheduledAttendancesQuery = this.Queryable().WhereDeducedIsActive();

            var attendanceOccurrenceInfo = new AttendanceOccurrenceService( this.Context as RockContext )
                .GetSelect(
                    attendanceOccurrenceId,
                    s => new
                    {
                        s.GroupId,
                        s.ScheduleId,
                        s.Schedule,
                        s.LocationId,
                        s.OccurrenceDate
                    } );

            if ( attendanceOccurrenceInfo == null )
            {
                // if an invalid occurrenceId was specified, just send back an empty list
                return new List<SchedulerResourceAttend>();
            }

            int scheduleId = attendanceOccurrenceInfo.ScheduleId ?? 0;
            var attendanceOccurrenceGroupId = attendanceOccurrenceInfo.GroupId ?? 0;
            DateTime occurrenceDate = attendanceOccurrenceInfo.OccurrenceDate;

            var occurrenceFirstDayOfWeek = attendanceOccurrenceInfo.OccurrenceDate.StartOfWeek( RockDateTime.FirstDayOfWeek );
            var occurrenceLastDayOfWeek = attendanceOccurrenceInfo.OccurrenceDate.EndOfWeek( RockDateTime.FirstDayOfWeek );

            //var occurrenceSundayDate = attendanceOccurrenceInfo.OccurrenceDate.SundayDate();
            //var occurrenceSundayWeekStartDate = occurrenceSundayDate.AddDays( -6 );

            // don't include schedule dates in the past
            if ( occurrenceFirstDayOfWeek <= RockDateTime.Today )
            {
                occurrenceFirstDayOfWeek = RockDateTime.Today;
            }

            // get all the occurrences for the selected week for this schedule (It could be more than once a week if it is a daily scheduled, or it might not be in the selected week if it is every 2 weeks, etc)
            var scheduleOccurrenceDateList = attendanceOccurrenceInfo.Schedule.GetScheduledStartTimes( occurrenceFirstDayOfWeek, occurrenceLastDayOfWeek.AddDays( 1 ) ).Select( a => a.Date ).ToList();

            IQueryable<PersonScheduleExclusion> personScheduleExclusionQueryForOccurrence = GetPersonScheduleExclusionQueryForOccurrenceDates( attendanceOccurrenceGroupId, scheduleOccurrenceDateList );

            var rockContext = this.Context as RockContext;

            // Create explicit joins to person alias and person tables so that rendered SQL has INNER Joins vs OUTER joins on Person and PersonAlias
            var personAliasQry = new PersonAliasService( rockContext ).Queryable();
            var personQryForJoin = new PersonService( rockContext ).Queryable();

            var attendancePersonQuery = this.Queryable()
                .Where( a => a.OccurrenceId == attendanceOccurrenceId )
                .Where( a => a.PersonAliasId.HasValue )
                .Where( a => a.RequestedToAttend == true || a.ScheduledToAttend == true )
                .Join(
                    personAliasQry,
                    a => a.PersonAliasId,
                    pa => pa.Id,
                    ( a, pa ) => new
                    {
                        Attendance = a,
                        PersonId = pa.PersonId
                    } )
                .Join(
                    personQryForJoin,
                    j1 => j1.PersonId,
                    p => p.Id,
                    ( j1, p ) => new
                    {
                        Attendance = j1.Attendance,
                        Person = p
                    } );

            var scheduledAttendancesQuery = attendancePersonQuery.Select( ap => new
            {
                AttendanceId = ap.Attendance.Id,
                ap.Attendance.RSVP,
                ap.Attendance.DeclineReasonValueId,
                ap.Attendance.ScheduledToAttend,
                PersonId = ap.Person.Id,
                NickName = ap.Person.NickName,
                LastName = ap.Person.LastName,
                SuffixValueId = ap.Person.SuffixValueId,
                RecordTypeValueId = ap.Person.RecordTypeValueId,

                // set HasSchedulingConflict = true if the same person is requested/scheduled for another attendance within the same ScheduleId/Date
                SchedulingConflicts = conflictingScheduledAttendancesQuery
                    .Where( c => c.Id != ap.Attendance.Id
                    && c.PersonAlias.PersonId == ap.Person.Id
                    && ( c.RequestedToAttend == true || c.ScheduledToAttend == true )
                    && c.Occurrence.ScheduleId == scheduleId
                    && c.Occurrence.OccurrenceDate == occurrenceDate )
                    .Select( x => new SchedulerResourceScheduled
                    {
                        GroupId = x.Occurrence.GroupId.Value,
                        GroupName = x.Occurrence.Group.Name,
                        LocationId = x.Occurrence.LocationId,
                        LocationName = x.Occurrence.Location.Name,
                        ScheduleId = attendanceOccurrenceInfo.ScheduleId.Value,
                        ScheduleName = attendanceOccurrenceInfo.Schedule.Name,
                        OccurrenceDate = occurrenceDate
                    } ).ToList(),

                BlackoutDateRanges = personScheduleExclusionQueryForOccurrence.Where( e => e.PersonAlias.PersonId == ap.Person.Id ).Select( s => new { s.StartDate, s.EndDate } ).ToList()
            } );

            var scheduledAttendancesList = scheduledAttendancesQuery.ToList();
            var personIds = scheduledAttendancesList.Select( a => a.PersonId ).Distinct().ToList();

            // create a lookup so we can find out which role the person has in the occurrence group (if they are a member)
            var attendanceOccurrenceGroupMemberLookupQuery = new GroupMemberService( rockContext )
                .Queryable().Where( a => a.GroupId == attendanceOccurrenceGroupId );

            Dictionary<int, IEnumerable<MemberLookupValue>> attendanceOccurrenceGroupMemberLookup;

            if ( personIds.Any() )
            {
                if ( personIds.Count == 1 )
                {
                    var personId = personIds[0];
                    attendanceOccurrenceGroupMemberLookupQuery = attendanceOccurrenceGroupMemberLookupQuery.Where( a => a.PersonId == personId );
                }
                else if ( personIds.Count < 1000 )
                {
                    // if there are less than 1000, just get the member records for people that are scheduled, otherwise the SQL will get too complex
                    attendanceOccurrenceGroupMemberLookupQuery = attendanceOccurrenceGroupMemberLookupQuery.Where( a => personIds.Contains( a.PersonId ) );
                }

                var attendanceOccurrenceGroupGroupTypeId = new GroupService( rockContext ).GetSelect( attendanceOccurrenceGroupId, s => s.GroupTypeId );
                var attendanceOccurrenceGroupGroupType = GroupTypeCache.Get( attendanceOccurrenceGroupGroupTypeId );

                var attendanceOccurrenceGroupTypeRoleCacheLookup = attendanceOccurrenceGroupGroupType?.Roles.ToDictionary( k => k.Id, v => v );

                attendanceOccurrenceGroupMemberLookup = attendanceOccurrenceGroupMemberLookupQuery.Select( a => new
                {
                    a.Id,
                    a.PersonId,
                    a.GroupRoleId,
                    MemberAssignments = a.GroupMemberAssignments.Where( x => x.ScheduleId.HasValue ).Select( s => new MemberLookupValueAssignment
                    {
                        Schedule = s.Schedule,
                        LocationId = s.LocationId,
                        LocationName = s.LocationId.HasValue ? s.Location.Name : null
                    } ),
                } )
                    .ToList()
                    .GroupBy( a => a.PersonId )
                    .ToDictionary(
                        k => k.Key,
                        v => v.Select( s => new MemberLookupValue
                        {
                            MemberId = s.Id,
                            GroupRole = attendanceOccurrenceGroupTypeRoleCacheLookup.GetValueOrNull( s.GroupRoleId ),
                            MemberAssignments = s.MemberAssignments.ToList()
                        } ) );
            }
            else
            {
                attendanceOccurrenceGroupMemberLookup = null;
            }

            IEnumerable<SchedulerResourceAttend> schedulerResourceAttendList = scheduledAttendancesList.Select( a =>
            {
                ScheduledAttendanceItemStatus status = Attendance.GetScheduledAttendanceItemStatus( a.RSVP, a.ScheduledToAttend );

                List<DateTime> personBlackoutDates = null;
                if ( a.BlackoutDateRanges.Any() )
                {
                    var personExclusionDateRange = new DateRange( a.BlackoutDateRanges.Min( d => d.StartDate ), a.BlackoutDateRanges.Max( d => d.EndDate ) );
                    personBlackoutDates = scheduleOccurrenceDateList.Where( d => personExclusionDateRange.Contains( d ) ).ToList();
                }

                var attendanceOccurrenceGroupMemberInfo = attendanceOccurrenceGroupMemberLookup
                    ?.GetValueOrNull( a.PersonId )
                    ?.OrderBy( x => x.GroupRole?.Order ?? int.MaxValue )
                    ?.FirstOrDefault();

                var memberAssignments = attendanceOccurrenceGroupMemberInfo?.MemberAssignments;
                ScheduledAttendanceItemMatchesPreference matchesPreference = ScheduledAttendanceItemMatchesPreference.NoPreference;
                if ( memberAssignments?.Any() == true )
                {
                    if ( memberAssignments.Any( x => x.Schedule.Id == attendanceOccurrenceInfo.ScheduleId ) )
                    {
                        // they have this schedule as a preference, now check if the location preference is for this location
                        if ( memberAssignments.Any( x =>
                                x.Schedule.Id == attendanceOccurrenceInfo.ScheduleId && x.LocationId.HasValue
                                && ( !x.LocationId.HasValue || x.LocationId == attendanceOccurrenceInfo.LocationId ) ) )
                        {
                            // they this schedule as a preference, and for this location
                            matchesPreference = ScheduledAttendanceItemMatchesPreference.MatchesPreference;
                        }
                        else
                        {
                            // they this schedule as a preference, but for a different location 
                            matchesPreference = ScheduledAttendanceItemMatchesPreference.NotMatchesPreference;
                        }
                    }
                    else if ( memberAssignments.Any( x => x.Schedule.Id != attendanceOccurrenceInfo.ScheduleId ) )
                    {
                        // they don't have this schedule and location as a preference, but they have a preference for a different schedule
                        matchesPreference = ScheduledAttendanceItemMatchesPreference.NotMatchesPreference;
                    }
                }

                // to get this formatted as an html data-attribute
                // - split case so we get a ' ' char
                // - replace ' ' with '-'
                // - lower case it
                // for example, this will convert 'NotMatchesPreference' to 'not-matches-preference'
                var matchesPreferenceAsDataAttribute = matchesPreference
                    .ConvertToString( true )
                    .Replace( ' ', '-' )
                    .ToLower();

                return new SchedulerResourceAttend
                {
                    AttendanceId = a.AttendanceId,
                    OccurrenceDate = occurrenceDate,
                    ConfirmationStatus = status.ConvertToString( false ).ToLower(),
                    MatchesPreference = matchesPreferenceAsDataAttribute,
                    PersonId = a.PersonId,
                    DeclinedReason = DefinedValueCache.GetValue( a.DeclineReasonValueId ).EncodeHtml(),

                    GroupMemberId = attendanceOccurrenceGroupMemberInfo?.MemberId,
                    GroupRole = attendanceOccurrenceGroupMemberInfo?.GroupRole,

                    // not needed for resource that is getting listed in an occurrence
                    ResourcePreferenceList = null,
                    ResourceScheduledList = null,
                    Note = null,

                    PersonNickName = a.NickName,
                    PersonLastName = a.LastName,
                    PersonName = Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    SchedulingConflicts = a.SchedulingConflicts,
                    BlackoutDates = personBlackoutDates,

                    // not needed for resource that is getting listed in an occurrence
                    DisplayedTimeSlotCount = null,
                    DisplayedDaysCount = null
                };
            } );

            // sort the schedulerResourceAttendList first by the GroupRole.Order (so that Leaders are listed at the top, followed by Members, followed by non-group members)
            // then sort by LastName/FirstName
            schedulerResourceAttendList = schedulerResourceAttendList
                .OrderBy( a => a.GroupRole?.Order ?? int.MaxValue )
                .ThenBy( a => a.PersonLastName )
                .ThenBy( a => a.PersonNickName )
                .ThenBy( a => a.PersonId ).ToList();

            return schedulerResourceAttendList;
        }

        /// <summary>
        /// Automatically schedules people for attendance for the specified groupId for the week identified by sundayDate
        /// It'll do this by looking at <see cref="GroupMemberAssignment" />s and <see cref="GroupMemberScheduleTemplate" />.
        /// The most specific assignments will be assigned first (both schedule and location are specified), followed by less specific assignments (just schedule or just location).
        /// If neither Schedule Or Location are specified, they won't be auto scheduled.
        /// The assignments will be done in random order until all available spots have been filled (in case there are a limited number of spots available).
        /// Starting with the first scheduled occurrence, each location (sorted by GroupLocation.Order, then by Name) will get filled up to the minimum capacity before filling spots for the next Location.
        /// For example, if the first occurrence is on Friday, the automatic assignment will fill the first location on Friday first before moving onto the next location.
        /// So, in the case of a shortage of assigned people, the automatic assignment will try to fully staffing the first occurrence(s) instead leaving all the occurrences as short.
        /// Any remaining understaffed occurrences can be filled manually using the group scheduler tool by a human that knows how they want to take care of shortages.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="sundayDate">The Sunday date.</param>
        /// <param name="scheduledByPersonAlias">The scheduled by person alias.</param>
        public void SchedulePersonsAutomatically( int groupId, DateTime sundayDate, PersonAlias scheduledByPersonAlias )
        {
            var rockContext = this.Context as RockContext;

            //var startDate = sundayDate.Date.AddDays( -6 );
            //var endDate = sundayDate.Date;

            var startDate = sundayDate.Date.StartOfWeek( RockDateTime.FirstDayOfWeek );
            var endDate = sundayDate.Date.EndOfWeek( RockDateTime.FirstDayOfWeek );

            var groupLocationQry = new GroupLocationService( rockContext ).Queryable().Where( a => a.GroupId == groupId );
            var scheduleList = groupLocationQry.SelectMany( a => a.Schedules ).WhereIsActive().Distinct().AsNoTracking().ToList();
            var scheduleOccurrenceDateList = scheduleList
                .Select( s => new
                {
                    OccurrenceDate = s.GetNextStartDateTime( startDate )?.Date,
                    ScheduleId = s.Id
                } )
                .Where( a => a.OccurrenceDate.HasValue )
                .OrderBy( a => a.OccurrenceDate )
                .ToList();

            var locationIdOrderedList = groupLocationQry.OrderBy( a => a.Order ).ThenBy( a => a.Location.Name ).Select( s => s.LocationId ).ToList();

            var attendanceOccurrenceIdList = new List<int>();

            foreach ( var scheduleOccurrenceDate in scheduleOccurrenceDateList )
            {
                foreach ( var locationId in locationIdOrderedList )
                {
                    var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).GetOrAdd( scheduleOccurrenceDate.OccurrenceDate.Value, groupId, locationId, scheduleOccurrenceDate.ScheduleId );
                    attendanceOccurrenceIdList.Add( attendanceOccurrence.Id );
                }
            }

            SchedulePersonsAutomaticallyForAttendanceOccurrences( attendanceOccurrenceIdList, scheduledByPersonAlias );
        }

        /// <summary>
        /// Automatically schedules people for attendance for the specified attendanceOccurrences. See summary of <see cref="SchedulePersonsAutomatically(int, DateTime, PersonAlias)" /> for how this works,
        /// </summary>
        /// <param name="attendanceOccurrenceIdList">The attendance occurrence identifier list.</param>
        /// <param name="scheduledByPersonAlias">The scheduled by person alias.</param>
        public void SchedulePersonsAutomaticallyForAttendanceOccurrences( List<int> attendanceOccurrenceIdList, PersonAlias scheduledByPersonAlias )
        {
            var rockContext = this.Context as RockContext;
            var groupLocationQry = new GroupLocationService( rockContext ).Queryable();
            var attendanceOccurrencesQry = new AttendanceOccurrenceService( rockContext )
                .GetByIds( attendanceOccurrenceIdList )
                .Where( ao =>
                    ao.GroupId.HasValue &&
                    ao.LocationId.HasValue &&
                    ao.ScheduleId.HasValue )
                .WhereDeducedIsActive();

            // sort the occurrences by the Date, then by the associated GroupLocation.Order then by Location.name
            var sortedAttendanceOccurrenceList = attendanceOccurrencesQry
                .Join(
                    groupLocationQry,
                    ao => new { GroupId = ao.GroupId.Value, LocationId = ao.LocationId.Value },
                    gl => new { gl.GroupId, gl.LocationId },
                    ( ao, gl ) => new
                    {
                        AttendanceOccurrence = ao,
                        GroupLocation = gl
                    } )
                    .OrderBy( a => a.AttendanceOccurrence.OccurrenceDate )
                    .ThenBy( a => a.GroupLocation.Order )
                    .ThenBy( a => a.GroupLocation.Location.Name )
                    .Select( a => a.AttendanceOccurrence )
                    .AsNoTracking()
                    .ToList();

            foreach ( var attendanceOccurrence in sortedAttendanceOccurrenceList )
            {
                SchedulePersonsAutomaticallyForAttendanceOccurrence( attendanceOccurrence, scheduledByPersonAlias );
            }
        }

        /// <summary>
        /// Automatically schedules people for attendance for the specified attendanceOccurrence. See summary of <see cref="SchedulePersonsAutomatically(int, DateTime, PersonAlias)"/> for how this works,
        /// </summary>
        /// <param name="attendanceOccurrence">The attendance occurrence.</param>
        /// <param name="scheduledByPersonAlias">The scheduled by person alias.</param>
        private void SchedulePersonsAutomaticallyForAttendanceOccurrence( AttendanceOccurrence attendanceOccurrence, PersonAlias scheduledByPersonAlias )
        {
            var rockContext = this.Context as RockContext;
            int locationId = attendanceOccurrence.LocationId.Value;
            int scheduleId = attendanceOccurrence.ScheduleId.Value;
            int attendanceOccurrenceId = attendanceOccurrence.Id;

            // get the capacity settings for the Group/Location/Schedule. NOTE: The Database and UI don't prevent duplicate LocationIds for a group,
            // but since that really doesn't make sense, just grab the first one
            int? desiredCapacity = new GroupLocationService( rockContext ).Queryable()
                .Where( gl => gl.GroupId == attendanceOccurrence.GroupId )
                .Where( gl => gl.LocationId == locationId )
                .SelectMany( gl => gl.GroupLocationScheduleConfigs )
                .Where( sc => sc.ScheduleId == scheduleId )
                .Select( a => a.DesiredCapacity )
                .FirstOrDefault();

            if ( !desiredCapacity.HasValue )
            {
                // We don't want to do auto scheduling if there isn't a desired capacity set
                return;
            }

            // get the count of people that are either pending or confirmed for this occurrence so that we know how close we are to the desired count
            // start with the number of people in the database, and add as we auto-schedule (so we don't have to re-query)
            int currentScheduledCount = this.Queryable().Where( a => a.OccurrenceId == attendanceOccurrence.Id && ( a.ScheduledToAttend == true || a.RequestedToAttend == true ) && ( a.RSVP != RSVP.No ) ).Count();

            if ( currentScheduledCount >= desiredCapacity )
            {
                // already have enough people assigned
                return;
            }

            // First, get any attendance records for members that signed up for an unspecified location for selected Group and Schedule
            var unspecifiedLocationResourceAttendanceList = this.Queryable().Where( a =>
                 a.Occurrence.ScheduleId == attendanceOccurrence.ScheduleId
                 && a.Occurrence.GroupId == attendanceOccurrence.GroupId
                 && a.Occurrence.OccurrenceDate == attendanceOccurrence.OccurrenceDate
                 && a.Occurrence.LocationId == null )
                .ToList();

            if ( unspecifiedLocationResourceAttendanceList.Any() )
            {
                foreach ( var attendanceWithNoLocation in unspecifiedLocationResourceAttendanceList )
                {
                    // move the attendance from the No-Location occurrence to this occurrence
                    attendanceWithNoLocation.OccurrenceId = attendanceOccurrence.Id;
                    currentScheduledCount++;

                    if ( currentScheduledCount >= desiredCapacity )
                    {
                        // Have enough people assigned not, so done
                        return;
                    }
                }
            }

            var scheduleIds = new List<int>();
            scheduleIds.Add( attendanceOccurrence.ScheduleId.Value );

            // get all available resources (group members that have a schedule template matching the occurrence date and schedule)
            var schedulerResourceParameters = new SchedulerResourceParameters
            {
                AttendanceOccurrenceGroupId = attendanceOccurrence.GroupId.Value,
                AttendanceOccurrenceScheduleIds = scheduleIds.ToArray(),
                AttendanceOccurrenceSundayDate = attendanceOccurrence.OccurrenceDate.SundayDate(),
                ResourceGroupId = attendanceOccurrence.GroupId.Value,
                GroupMemberFilterType = SchedulerResourceGroupMemberFilterType.ShowMatchingPreference
            };

            var schedulerResources = this.GetSchedulerResources( schedulerResourceParameters );

            // only grab resources that haven't been scheduled already, and don't have a conflict (blackout or scheduled for some other group)
            // Also, only include resources that have a GroupMemberId since those would be the only ones that would be auto-schedulable
            var scheduleResourcesGroupMemberIds = schedulerResources
                .Where( a => a.GroupMemberId.HasValue && a.IsAlreadyScheduledForGroup == false && a.HasConflict == false )
                .Select( a => a.GroupMemberId.Value ).ToList();

            if ( !scheduleResourcesGroupMemberIds.Any() )
            {
                // nobody to add as scheduled resource, so return
                return;
            }

            // get GroupMemberAssignmments for the groupMembers returned from schedulerResources for this occurrence's locationId and scheduleId
            var groupMemberAssignmentsQuery = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .Where( a => scheduleResourcesGroupMemberIds.Contains( a.GroupMemberId ) )
                .Where( a => ( !a.LocationId.HasValue || a.LocationId == locationId )
                 && ( !a.ScheduleId.HasValue || a.ScheduleId == scheduleId ) );

            // Exclude GroupMemberAssignments that don't have a schedule or location set (since that means they don't want to be auto-scheduled)
            groupMemberAssignmentsQuery = groupMemberAssignmentsQuery.Where( a => a.LocationId.HasValue || a.ScheduleId.HasValue );

            var endOfOccurrenceDay = attendanceOccurrence.OccurrenceDate.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
            var groupMemberAssignmentsList = groupMemberAssignmentsQuery
                .Select( a => new
                {
                    GroupMemberId = a.GroupMember.Id,
                    a.GroupMember.PersonId,
                    a.LocationId,
                    a.ScheduleId
                } )
                .GroupJoin( rockContext.Attendances, gma => gma.PersonId, a => a.PersonAlias.PersonId,
                    ( gma, a ) => new GroupMemberAssignmentInfo
                    {
                        GroupMemberId = gma.GroupMemberId,
                        PersonId = gma.PersonId,
                        LocationId = gma.LocationId,
                        ScheduleId = gma.ScheduleId,
                        SpecificLocationAndSchedule = gma.LocationId.HasValue && gma.ScheduleId.HasValue,
                        SpecificScheduleOnly = !gma.LocationId.HasValue && gma.ScheduleId.HasValue,
                        SpecificLocationOnly = !gma.LocationId.HasValue && !gma.ScheduleId.HasValue,
                        LastScheduledDate = a
                            .Where( att => ( att.ScheduledToAttend != null && att.ScheduledToAttend.Value ) || ( att.RequestedToAttend != null && att.RequestedToAttend.Value ) )
                            .Where( att => att.StartDateTime <= endOfOccurrenceDay )
                            .Select( att => att.StartDateTime )
                            .DefaultIfEmpty()
                            .Max()
                    } ).ToList();

            if ( !groupMemberAssignmentsList.Any() )
            {
                // nobody with assignments to assign, so return
                return;
            }

            // use a new RockContext to use for adding attending resources so that get can get saved to the database without saving any changes associated with the current rockContext
            var groupAssignmentAttendanceRockContext = new RockContext();
            var groupAssignmentAttendanceService = new AttendanceService( groupAssignmentAttendanceRockContext );

            /* 2020-08-03 MDP
             The rules for autoschedule
               - Order by Last Scheduled date (Last Scheduled across all groups)
               - Randomize order of List (order by Guid.NewGuid())

               - then to schedule in this order
               1) Person has both Schedule and Location specified
               2) Person has Schedule specified but 'No Location Specified' for location
               3) Person has Location specified but no schedule specified. Note that the UI doesn't currently support this option

               *Note: We will sort by bool, but make true = 0 and false = 1 so that True comes before False

               - This will end up with the list as follows
               1)  People with Schedule And Location (in random order)
               2)  People with Schedule only (in random order)
               3)  People with Location only (in random order)
             */

            // order group member assignments by the last time they served and then randomly after that.
            groupMemberAssignmentsList = groupMemberAssignmentsList
                .OrderBy( a => a.LastScheduledDate )
                .ThenBy( a => Guid.NewGuid() )
                .ToList();

            // assign based on specificity rule (see above engineering note)
            var groupMemberAssignmentSortedBySpecificity = groupMemberAssignmentsList
                .OrderBy( a => a.SpecificLocationAndSchedule ? 0 : 1 )
                .ThenBy( a => a.SpecificScheduleOnly ? 0 : 1 )
                .ThenBy( a => a.SpecificLocationOnly ? 0 : 1 ).ToList();

            foreach ( var groupMemberAssignment in groupMemberAssignmentSortedBySpecificity )
            {
                if ( currentScheduledCount < desiredCapacity )
                {
                    groupAssignmentAttendanceService.ScheduledPersonAddPending( groupMemberAssignment.PersonId, attendanceOccurrenceId, scheduledByPersonAlias );
                    groupAssignmentAttendanceRockContext.SaveChanges();

                    // person is assigned, so remove them from the list
                    groupMemberAssignmentsList.Remove( groupMemberAssignment );
                    currentScheduledCount++;
                }
                else
                {
                    break;
                }
            }

            // Note: If neither Schedule and Location are specified, don't auto-schedule
        }

        /// <summary>
        /// Add/Updates an attendance record to indicate person is Requested to Attend marks them pending
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        /// <param name="scheduledByPersonAlias">The scheduled by person alias.</param>
        /// <returns></returns>
        public Attendance ScheduledPersonAddPending( int personId, int attendanceOccurrenceId, PersonAlias scheduledByPersonAlias )
        {
            var rockContext = this.Context as RockContext;
            var scheduledAttendance = this.Queryable()
                .FirstOrDefault( a => a.PersonAlias.PersonId == personId
                    && a.OccurrenceId == attendanceOccurrenceId );

            if ( scheduledAttendance == null )
            {
                var personAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId );
                var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).Get( attendanceOccurrenceId );
                var scheduledDateTime = attendanceOccurrence.OccurrenceDate.Add( attendanceOccurrence.Schedule.StartTimeOfDay );
                int? campusId = new LocationService( rockContext ).GetCampusIdForLocation( attendanceOccurrence.LocationId ) ?? new GroupService( rockContext ).Get( attendanceOccurrence.GroupId.Value ).CampusId;
                scheduledAttendance = new Attendance
                {
                    CampusId = campusId,
                    PersonAliasId = personAliasId,
                    OccurrenceId = attendanceOccurrenceId,
                    ScheduledByPersonAliasId = scheduledByPersonAlias?.Id,
                    StartDateTime = scheduledDateTime,
                    DidAttend = false,
                    RequestedToAttend = true,
                    ScheduledToAttend = false,
                    RSVP = RSVP.Unknown
                };

                this.Add( scheduledAttendance );
            }
            else
            {
                if ( scheduledAttendance.RequestedToAttend != true )
                {
                    scheduledAttendance.RequestedToAttend = true;
                }

                // if they previously declined, set RSVP back to Unknown if they are added as pending again
                if ( scheduledAttendance.RSVP == RSVP.No )
                {
                    scheduledAttendance.RSVP = RSVP.Unknown;
                }
            }

            return scheduledAttendance;
        }

        /// <summary>
        /// Updates attendance record to indicate person is not pending, or confirmed, or declined
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        public void ScheduledPersonRemove( int attendanceId )
        {
            var scheduledAttendance = this.Get( attendanceId );
            if ( scheduledAttendance != null )
            {
                scheduledAttendance.ScheduledToAttend = false;
                scheduledAttendance.RequestedToAttend = false;
                scheduledAttendance.RSVP = RSVP.Unknown;
            }
            else
            {
                // ignore if there is no attendance record
            }
        }

        /// <summary>
        /// Updates attendance record to indicate person is Scheduled To Attend (Confirmed)
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        public void ScheduledPersonConfirm( int attendanceId )
        {
            var scheduledAttendance = this.Get( attendanceId );
            if ( scheduledAttendance != null )
            {
                scheduledAttendance.ScheduledToAttend = true;
                scheduledAttendance.RSVPDateTime = RockDateTime.Now;
                scheduledAttendance.RSVP = RSVP.Yes;
            }
            else
            {
                // ignore if there is no attendance record
            }
        }

        /// <summary>
        /// Updates attendance record to indicate person canceled a confirmed attendance
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        public void ScheduledPersonConfirmCancel( int attendanceId )
        {
            var scheduledAttendance = this.Get( attendanceId );
            if ( scheduledAttendance != null )
            {
                scheduledAttendance.ScheduledToAttend = null;
                scheduledAttendance.RSVP = RSVP.Unknown;
            }
            else
            {
                // ignore if there is no attendance record
            }
        }

        /// <summary>
        /// Updates attendance record to indicate person is back to a pending state
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        public void ScheduledPersonPending( int attendanceId )
        {
            var scheduledAttendance = this.Get( attendanceId );
            if ( scheduledAttendance != null )
            {
                scheduledAttendance.RequestedToAttend = true;
                scheduledAttendance.ScheduledToAttend = false;
                scheduledAttendance.RSVP = RSVP.Unknown;
            }
            else
            {
                // ignore if there is no attendance record
            }
        }

        /// <summary>
        /// Updates attendance record to indicate person declined a scheduled attendance
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        /// <param name="declineReasonValueId">The decline reason value identifier.</param>
        public void ScheduledPersonDecline( int attendanceId, int? declineReasonValueId )
        {
            var scheduledAttendance = this.Get( attendanceId );
            if ( scheduledAttendance != null )
            {
                scheduledAttendance.DeclineReasonValueId = declineReasonValueId;
                scheduledAttendance.RSVPDateTime = RockDateTime.Now;
                scheduledAttendance.RSVP = RSVP.No;
            }
            else
            {
                // ignore if there is no attendance record
            }
        }

        /// <summary>
        /// Sends a response (accept or decline) email to the <see cref="Attendance.ScheduledByPersonAlias">Scheduler Person</see>
        /// for this <see cref="Attendance"/>
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        /// <param name="schedulingResponseEmailGuid">The scheduling response email unique identifier.</param>
        public void SendScheduledPersonResponseEmailToScheduler( int attendanceId, Guid? schedulingResponseEmailGuid )
        {
            var attendance = new AttendanceService( new RockContext() ).Get( attendanceId );
            var recipientPerson = attendance.ScheduledByPersonAlias?.Person;
            SendSendScheduledPersonResponseEmail( attendance, schedulingResponseEmailGuid, recipientPerson );
        }

        /// <summary>
        /// Sends a decline email to the <see cref="Group.ScheduleCancellationPersonAlias">Scheduling Cancellation Person</see>
        /// for this attendance's <see cref="AttendanceOccurrence.Group"/>
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        /// <param name="schedulingResponseEmailGuid">The scheduling response email unique identifier.</param>
        public void SendScheduledPersonDeclineEmail( int attendanceId, Guid? schedulingResponseEmailGuid )
        {
            var attendance = new AttendanceService( new RockContext() ).Get( attendanceId );
            var recipientPerson = attendance.Occurrence?.Group?.ScheduleCancellationPersonAlias?.Person;
            SendSendScheduledPersonResponseEmail( attendance, schedulingResponseEmailGuid, recipientPerson );
        }

        /// <summary>
        /// Sends the send scheduled person response email.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <param name="schedulingResponseEmailGuid">The scheduling response email unique identifier.</param>
        /// <param name="recipientPerson">The recipient person.</param>
        private void SendSendScheduledPersonResponseEmail( Attendance attendance, Guid? schedulingResponseEmailGuid, Person recipientPerson )
        {
            if ( !schedulingResponseEmailGuid.HasValue )
            {
                return;
            }

            if ( attendance == null )
            {
                return;
            }

            if ( recipientPerson?.IsEmailActive != true )
            {
                return;
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            var group = attendance.Occurrence.Group;

            mergeFields.Add( "Group", group );
            mergeFields.Add( "ScheduledItem", attendance );
            mergeFields.Add( "OccurrenceDate", attendance.Occurrence.OccurrenceDate );
            mergeFields.Add( "ScheduledStartTime", DateTime.Today.Add( attendance.Occurrence.Schedule.StartTimeOfDay ).ToString( "h:mm tt" ) );
            mergeFields.Add( "Person", attendance.PersonAlias.Person );
            mergeFields.Add( "Scheduler", attendance.ScheduledByPersonAlias.Person );
            mergeFields.Add( "Recipient", recipientPerson );

            var emailMessage = new RockEmailMessage( schedulingResponseEmailGuid.Value );
            emailMessage.AddRecipient( new RockEmailMessageRecipient( recipientPerson, mergeFields ) );
            emailMessage.CreateCommunicationRecord = false;
            emailMessage.Send();
        }

        /// <summary>
        /// Gets a Queryable of Attendance Records that are Scheduled Attendances that are pending confirmation.
        /// This includes attendance records that have RequestedToAttend, not yet confirmed (not confirmed as scheduled and not declined), and didn't attend yet.
        /// This doesn't include attendance records that occurred in prior dates.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Attendance> GetPendingScheduledConfirmations()
        {
            var currentDate = RockDateTime.Now.Date;
            return Queryable()
                .Where( a => a.RequestedToAttend == true )
                .Where( a => a.ScheduledToAttend != true )
                .Where( a => a.DeclineReasonValueId == null )
                .Where( a => a.DidAttend != true )
                .Where( a => a.Occurrence.OccurrenceDate >= currentDate )

                // RSVP.Maybe is not used by the Group Scheduler. But, just in case, treat it as that the person has not responded.
                .Where( a => a.RSVP == RSVP.Maybe || a.RSVP == RSVP.Unknown )

                // Explicitly include Group and Location objects to prevent issues with lazy loading after databinding (e.g., in GroupScheduleToolbox block).
                .Include( a => a.Occurrence.Group )
                .Include( a => a.Occurrence.Location );
        }

        /// <summary>
        /// Gets a Queryable of Attendance Records that are scheduled and confirmed.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Attendance> GetConfirmedScheduled()
        {
            return this.Queryable().Where( a => a.ScheduledToAttend == true && a.RSVP != RSVP.No && a.DidAttend != true );
        }

        /// <summary>
        /// 
        /// </summary>
        private class GroupMemberAssignmentInfo
        {
            /// <summary>
            /// Gets or sets the group member identifier.
            /// </summary>
            /// <value>
            /// The group member identifier.
            /// </value>
            public int GroupMemberId { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [specific location and schedule].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [specific location and schedule]; otherwise, <c>false</c>.
            /// </value>
            public bool SpecificLocationAndSchedule { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [specific schedule only].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [specific schedule only]; otherwise, <c>false</c>.
            /// </value>
            public bool SpecificScheduleOnly { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [specific location only].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [specific location only]; otherwise, <c>false</c>.
            /// </value>
            public bool SpecificLocationOnly { get; set; }

            /// <summary>
            /// Gets or sets the location identifier.
            /// </summary>
            /// <value>
            /// The location identifier.
            /// </value>
            public int? LocationId { get; internal set; }

            /// <summary>
            /// Gets or sets the schedule identifier.
            /// </summary>
            /// <value>
            /// The schedule identifier.
            /// </value>
            public int? ScheduleId { get; internal set; }

            /// <summary>
            /// Gets or sets the last scheduled date.
            /// </summary>
            /// <value>
            /// The last scheduled date.
            /// </value>
            public DateTime LastScheduledDate { get; internal set; }
        }

        #endregion GroupScheduling Related

        #region RSVP Related

        /// <summary>
        /// Creates attendance records if they don't exist for a designated occurrence and list of person IDs.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence record.</param>
        /// <param name="personIds">A comma-delimited list of Person IDs.</param>
        [Obsolete( "Use the method which accepts a List<int> parameter instead." )]
        [RockObsolete( "1.10.4" )]
        public void RegisterRSVPRecipients( int occurrenceId, string personIds )
        {
            var personIdList = personIds.Split( ',' ).Select( int.Parse ).ToList();
            RegisterRSVPRecipients( occurrenceId, personIdList );
        }

        /// <summary>
        /// Creates attendance records if they don't exist for a designated occurrence and list of person IDs.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence record.</param>
        /// <param name="personIdList">A list of Person IDs.</param>
        public void RegisterRSVPRecipients( int occurrenceId, List<int> personIdList )
        {
            var rockContext = this.Context as RockContext;

            // Get Occurrence.
            var occurrence = new AttendanceOccurrenceService( rockContext ).Queryable().AsNoTracking()
                .Where( o => o.Id == occurrenceId ).FirstOrDefault();
            DateTime startDateTime = occurrence.Schedule != null && occurrence.Schedule.HasSchedule() ? occurrence.OccurrenceDate.Date.Add( occurrence.Schedule.StartTimeOfDay ) : occurrence.OccurrenceDate;

            // Get PersonAliasIDs from PersonIDs
            var people = new PersonService( rockContext ).Queryable().AsNoTracking()
                .Where( p => personIdList.Contains( p.Id ) )
                .ToList();
            var personAliasIds = people.Select( p => p.PrimaryAliasId ).ToList();

            // Check for existing records.
            var attendanceService = new AttendanceService( rockContext );
            var existingAttendanceRecords = attendanceService.Queryable().AsNoTracking()
                .Where( a => personAliasIds.Contains( a.PersonAliasId ) )
                .Where( a => a.OccurrenceId == occurrenceId )
                .ToList();

            var newAttendanceRecords = new List<Attendance>();
            foreach ( int personAliasId in personAliasIds )
            {
                // If record doesn't exist, create a new attendance record and set RSVP to "Unknown".
                if ( !existingAttendanceRecords.Where( a => a.PersonAliasId == personAliasId ).Any() )
                {
                    newAttendanceRecords.Add(
                        new Attendance()
                        {
                            OccurrenceId = occurrenceId,
                            PersonAliasId = personAliasId,
                            StartDateTime = startDateTime,
                            RSVP = Rock.Model.RSVP.Unknown,
                            DidAttend = false
                        } );
                }
            }

            attendanceService.AddRange( newAttendanceRecords );
            rockContext.SaveChanges();
        }

        #endregion RSVP Related

        #region BulkImport related

        /// <summary>
        /// BulkInserts Attendance Records
        /// </summary>
        /// <param name="attendancesImport">The attendances import.</param>
        public static void BulkAttendanceImport( AttendancesImport attendancesImport )
        {
            if ( attendancesImport == null )
            {
                throw new ArgumentNullException( "AttendancesImport must be assigned a value." );
            }

            if ( attendancesImport.Attendances.Any( a => !a.PersonAliasId.HasValue && !a.PersonId.HasValue ) )
            {
                throw new Exception( "All Attendance records must have either a PersonId or PersonAliasId assigned." );
            }

            var attendanceImportList = attendancesImport.Attendances;
            RockContext rockContext = new RockContext();

            DateTime importDateTime = RockDateTime.Now;

            var occurrenceMinDate = attendanceImportList.Min( a => a.OccurrenceDate );
            var occurrenceMaxDate = attendanceImportList.Max( a => a.OccurrenceDate );

            var occurrenceIdLookupList = new AttendanceOccurrenceService( rockContext ).Queryable()
                .Select( o => new
                {
                    Id = o.Id,
                    GroupId = o.GroupId,
                    LocationId = o.LocationId,
                    ScheduleId = o.ScheduleId,
                    OccurrenceDate = o.OccurrenceDate
                } ).Where( a => a.OccurrenceDate >= occurrenceMinDate && a.OccurrenceDate <= occurrenceMaxDate )
                .ToList();

            // Get list of existing occurrence records that have already been created, using the combination of pipe delimited GroupId,LocationId,ScheduleId,OccurenceDate (for quick lookup).
            // We'll use this to see what occurrences need to be added, then we'll rebuild this lookup to use for bulk inserting Attendances
            var occurrenceIdLookup = occurrenceIdLookupList.ToDictionary( k => AttendanceImport.GetOccurrenceLookupKey( k.GroupId, k.LocationId, k.ScheduleId, k.OccurrenceDate ), v => v.Id );

            // Create a list of *imported* occurrence records, using the combination of pipe delimited GroupId,LocationId,ScheduleId,OccurenceDate (for quick lookup).
            // We'll compare this to what is already in the database to figure out what occurrences need to be bulk inserted
            var importedAttendancesOccurrenceList = attendanceImportList
                .GroupBy( a => new
                {
                    a.GroupId,
                    a.LocationId,
                    a.ScheduleId,
                    a.OccurrenceDate
                } ).Select( a => new
                {
                    a.Key,
                    AttendanceImportOccurrence = a.FirstOrDefault()
                } ).ToList();

            var importedAttendancesOccurrenceKeys = importedAttendancesOccurrenceList.ToDictionary( k => $"{k.Key.GroupId}|{k.Key.LocationId}|{k.Key.ScheduleId}|{k.Key.OccurrenceDate}", v => v.AttendanceImportOccurrence );

            // Create list of occurrences to be bulk inserted
            List<AttendanceOccurrence> occurrencesToBulkImport = importedAttendancesOccurrenceKeys.Where( a => !occurrenceIdLookup.ContainsKey( a.Key ) )
                .Select( a => new AttendanceOccurrence
                {
                    GroupId = a.Value.GroupId,
                    LocationId = a.Value.LocationId,
                    ScheduleId = a.Value.ScheduleId,
                    OccurrenceDate = a.Value.OccurrenceDate
                } ).ToList();

            // Add all the new occurrences
            rockContext.BulkInsert( occurrencesToBulkImport );

            // Load all the existing occurrences again.
            occurrenceIdLookup = new AttendanceOccurrenceService( rockContext ).Queryable()
                .Select( o => new
                {
                    Id = o.Id,
                    GroupId = o.GroupId,
                    LocationId = o.LocationId,
                    ScheduleId = o.ScheduleId,
                    OccurrenceDate = o.OccurrenceDate
                } ).Where( a => a.OccurrenceDate >= occurrenceMinDate && a.OccurrenceDate <= occurrenceMaxDate )
                .ToDictionary( k => $"{k.GroupId}|{k.LocationId}|{k.ScheduleId}|{k.OccurrenceDate}", v => v.Id );

            var importedPersonIds = attendanceImportList.Where( a => a.PersonId.HasValue ).Select( a => a.PersonId.Value ).Distinct().ToList();

            // create a PersonAliasId to PersonId lookup so we don't have to individually query for each primary alias
            var primaryAliasIdFromPersonIdLookup = new PersonAliasService( rockContext )
                .GetPrimaryAliasQuery()
                .Where( a => importedPersonIds.Contains( a.PersonId ) )
                .Select( a => new { PersonAliasId = a.Id, a.PersonId } )
                .ToDictionary( k => k.PersonId, v => ( int? ) v.PersonAliasId );

            // create a list of Attendances to BulkImport (specify the number of records we are going to put in the list initialize the list 
            List<Attendance> attendancesToBulkInsert = attendanceImportList.Select( attendanceImport =>
             {
                 var attendance = new Attendance
                 {
                     StartDateTime = attendanceImport.StartDateTime,
                     PersonAliasId = attendanceImport.PersonAliasId
                 };

                 // If PersonId was specified, we'll use that (and ignore the imported PersonAliasId if there is one)
                 // Then we'll use the PrimaryAliasId associated with the specified PersonId 
                 if ( attendanceImport.PersonId.HasValue )
                 {
                     var personAliasId = primaryAliasIdFromPersonIdLookup.GetValueOrNull( attendanceImport.PersonId.Value );
                     if ( !personAliasId.HasValue )
                     {
                         // primaryAliasIdFromPersonIdLookup.GetValueOrNull(personId) returned NULL.
                         // This would be due to the import specifying a PersonId that doesn't exist.
                         // So let's set personAliasId to -1 to let the ForeignKey exception happen
                         personAliasId = -1;
                     }

                     attendance.PersonAliasId = personAliasId;
                 }

                 var occurrenceKey = attendanceImport.GetOccurrenceLookupKey();

                 var occurrenceId = occurrenceIdLookup.GetValueOrNull( occurrenceKey );

                 // occurrenceId from the lookup should never return null since we just created all the ones we needed.
                 // So if it is null, throw an exception, since that would be a bug
                 attendance.OccurrenceId = occurrenceId.Value;

                 return attendance;
             } ).ToList();

            rockContext.BulkInsert( attendancesToBulkInsert );
        }

        private class MemberLookupValue
        {
            public int MemberId { get; set; }

            public GroupTypeRoleCache GroupRole { get; set; }

            public List<MemberLookupValueAssignment> MemberAssignments { get; set; }
        }

        private class MemberLookupValueAssignment
        {
            public Schedule Schedule { get; set; }

            public int? LocationId { get; set; }

            public string LocationName { get; set; }
        }

        private class ResourceListItem
        {
            public int GroupMemberId { get; set; }
            public int GroupRoleId { get; set; }
            public int PersonId { get; set; }
            public string Note { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public int? SuffixValueId { get; set; }
            public int? RecordTypeValueId { get; set; }
            public int? ScheduleTemplateId { get; set; }
            public DateTime? ScheduleStartDate { get; set; }
            public IEnumerable<MemberAssignmentInfo> MemberAssignments { get; internal set; }
        }

        private class MemberAssignmentInfo
        {
            public Schedule Schedule { get; set; }
            public int? LocationId { get; set; }
            public string LocationName { get; set; }
        }

        #endregion BulkImport related
    }

    #region Group Scheduling related classes and types

    /// <summary>
    /// A Scheduler Resource (Person) that has been associated with Attendance Occurrence in some sort of scheduled state (Pending, Confirmed or Declined)
    /// </summary>
    public class SchedulerResourceAttend : SchedulerResource
    {
        /// <summary>
        /// Gets or sets the attendance identifier.
        /// </summary>
        /// <value>
        /// The attendance identifier.
        /// </value>
        public int AttendanceId { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date.
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTime OccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ScheduledAttendanceItemMatchesPreference"/> as a lowercase '-' delimited string
        /// </summary>
        /// <value>
        /// The <see cref="ScheduledAttendanceItemMatchesPreference">Matching Preference</see> state
        /// </value>
        public string MatchesPreference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this scheduled resource has a blackout conflict for the occurrence date
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has blackout conflict; otherwise, <c>false</c>.
        /// </value>
        public override bool HasBlackoutConflict => this.BlackoutDates?.Contains( this.OccurrenceDate ) == true;

        /// <summary>
        /// Gets or sets the declined reason.
        /// </summary>
        /// <value>
        /// The declined reason.
        /// </value>
        public string DeclinedReason { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Model.SchedulerResourceAssignment" />
    public class SchedulerResourcePreference : SchedulerResourceAssignment
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Model.SchedulerResourceAssignment" />
    public class SchedulerResourceScheduled : SchedulerResourceAssignment
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class SchedulerResourceAssignment
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the schedule.
        /// </summary>
        /// <value>
        /// The name of the schedule.
        /// </value>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        /// <value>
        /// The name of the location.
        /// </value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets the occurrence date.
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTime OccurrenceDate { get; internal set; }
    }

    /// <summary>
    /// Information about a potential scheduler resource (Person) for the GroupScheduler
    /// </summary>
    public class SchedulerResource
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ScheduledAttendanceItemStatus"/> as a lowercase string
        /// </summary>
        /// <value>
        /// The <see cref="ScheduledAttendanceItemStatus"/> status.
        /// </value>
        public string ConfirmationStatus { get; set; }

        /// <summary>
        /// Gets or sets the GroupMemberId.
        /// NOTE: This will be NULL if the resource list has manually added personIds and/or comes from a Person DataView.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the name of the person nick.
        /// </summary>
        /// <value>
        /// The name of the person nick.
        /// </value>
        public string PersonNickName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        /// <value>
        /// The last name of the person.
        /// </value>
        public string PersonLastName { get; set; }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the last attendance date time.
        /// </summary>
        /// <value>
        /// The last attendance date time.
        /// </value>
        public DateTime? LastAttendanceDateTime { get; set; }

        /// <summary>
        /// Gets the last attendance date time formatted.
        /// </summary>
        /// <value>
        /// The last attendance date time formatted.
        /// </value>
        public string LastAttendanceDateTimeFormatted
        {
            get
            {
                if ( LastAttendanceDateTime != null )
                {
                    if ( LastAttendanceDateTime.Value.Year == RockDateTime.Now.Year )
                    {
                        return LastAttendanceDateTime?.ToString( "MMM d" );
                    }
                    else
                    {
                        return LastAttendanceDateTime?.ToString( "MMM d, yyyy" );
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }

        /// <summary>
        /// Gets a value indicating whether this Person has conflict.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasConflict => HasGroupRequirementsConflict || HasSchedulingConflict || HasBlackoutConflict;

        /// <summary>
        /// Gets or sets the conflict note.
        /// </summary>
        /// <value>
        /// The conflict note.
        /// </value>
        public string ConflictNote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Person has blackout conflict for all the occurrences
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has blackout conflict; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasBlackoutConflict => BlackoutDates?.Any() == true && BlackoutDates.Count() >= DisplayedDaysCount;

        /// <summary>
        /// Gets or sets a value indicating whether this Person has partial blackout conflict (blackout for some of the occurrences, but not all of them)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has partial blackout conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasPartialBlackoutConflict => BlackoutDates?.Any() == true && BlackoutDates.Count() < DisplayedDaysCount;

        /// <summary>
        /// Gets or sets the number of days are shown in the Group Scheduler. This could be more than
        /// one if there are multiple schedules selected, or a schedule occurs more than one a week (like a Daily Schedule).
        /// We'll store this in each Scheduled Resource (person) that we show in the Resource list
        /// </summary>
        /// <value>
        /// The occurrence date count.
        /// </value>
        public int? DisplayedDaysCount { get; set; }

        /// <summary>
        /// Gets or sets the number of occurrences for the selected week
        /// </summary>
        /// <value>
        /// The occurrence date count.
        /// </value>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use DisplayedDaysCount instead" )]
        public int OccurrenceDateCount { get; set; }

        /// <summary>
        /// Gets or sets the displayed time slot count. This could be more than
        /// one if there are multiple schedules selected, or a schedule occurs more than once a week (like a Daily Schedule).
        /// We'll store this in each Scheduled Resource (person) that we show in the Resource list
        /// </summary>
        /// <value>
        /// The displayed time slot count.
        /// </value>
        public int? DisplayedTimeSlotCount { get; set; }

        /// <summary>
        /// Gets or sets the blackout dates.
        /// </summary>
        /// <value>
        /// The blackout dates.
        /// </value>
        public List<DateTime> BlackoutDates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Person has group requirements conflict.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has group requirements conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasGroupRequirementsConflict { get; set; }

        /// <summary>
        /// Gets a value indicating whether this Person has scheduling conflict with some other group for this schedule+date
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has scheduling conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasSchedulingConflict => SchedulingConflicts?.Any() == true;

        /// <summary>
        /// Gets or sets the scheduling conflicts.
        /// </summary>
        /// <value>
        /// The scheduling conflicts.
        /// </value>
        public List<SchedulerResourceScheduled> SchedulingConflicts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Person is already scheduled for this group+schedule+date
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Person has scheduling conflict; otherwise, <c>false</c>.
        /// </value>
        public bool? IsAlreadyScheduledForGroup { get; set; }

        /// <summary>
        /// Gets or sets the group role, if the Person is a member of the Occurrence Group
        /// </summary>
        /// <value>
        /// The group role.
        /// </value>
        public GroupTypeRoleCache GroupRole { get; set; }

        /// <summary>
        /// Gets the name of the group role.
        /// </summary>
        /// <value>
        /// The name of the group role.
        /// </value>
        public string GroupRoleName => GroupRole?.Name;

        /// <summary>
        /// Gets the resource's Preferences (from <seealso cref="GroupMemberAssignment"/>) for t
        /// </summary>
        /// <value>
        /// The resource preferences.
        /// </value>
        public List<SchedulerResourcePreference> ResourcePreferenceList { get; set; }

        /// <summary>
        /// Gets the resource's Scheduled List (from <seealso cref="Attendance"/>)
        /// </summary>
        /// <value>
        /// The resource scheduled list.
        /// </value>
        public List<SchedulerResourceScheduled> ResourceScheduledList { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.PersonName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ScheduledAttendanceItemMatchesPreference
    {
        /// <summary>
        /// Person (group member) has a scheduling preference for the scheduled schedule and location
        /// </summary>
        MatchesPreference,

        /// <summary>
        /// Person (group member) has a scheduling preference for a different schedule (or the selected schedule but different location)
        /// </summary>
        NotMatchesPreference,

        /// <summary>
        /// Person (group member) has no scheduling preferences for the group (or the person isn't a member of the group )
        /// </summary>
        NoPreference
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ScheduledAttendanceItemStatus
    {
        /// <summary>
        /// pending
        /// </summary>
        Pending,

        /// <summary>
        /// confirmed
        /// </summary>
        Confirmed,

        /// <summary>
        /// declined
        /// </summary>
        Declined,

        /// <summary>
        /// Person isn't Scheduled (they would be in the list of Unscheduled resources)
        /// </summary>
        Unscheduled,
    }

    /// <summary>
    /// 
    /// </summary>
    [RockObsolete( "1.12" )]
    [Obsolete( "Use GroupSchedulerResourceListSourceType instead" )]
    public enum SchedulerResourceListSourceType
    {
        /// <summary>
        /// The group
        /// </summary>
        Group,

        /// <summary>
        /// The alternate group
        /// </summary>
        [Description( "Alt Group" )]
        AlternateGroup,

        /// <summary>
        /// The data view
        /// </summary>
        DataView
    }

    /// <summary>
    /// 
    /// </summary>
    public enum GroupSchedulerResourceListSourceType
    {
        /// <summary>
        /// Show all members of the selected group
        /// </summary>
        [Description( "Group Members - All" )]
        GroupMembers,

        /// <summary>
        /// Show all members of the selected group that have a scheduling preference set for the selected week
        /// </summary>
        [Description( "Group Members - Matching Week" )]
        GroupMatchingPreference,

        /// <summary>
        /// All group members from another group
        /// </summary>
        [Description( "Alternate Group" )]
        AlternateGroup,

        /// <summary>
        /// The Parent group of the select group (only show if the selected group has a parent group)
        /// </summary>
        [Description( "Parent Group" )]
        ParentGroup,

        /// <summary>
        /// People that exist in a selected dataview
        /// </summary>
        [Description( "Data View" )]
        DataView,
    
        /// <summary>
        /// Group members whose week to work is the selected week AND whose assignment (location/schedule) matches the filters OR they have no assignment.
        /// </summary>
        [Description( "Group Members - Matching Assignment" )]
        GroupMatchingAssignment,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SchedulerResourceGroupMemberFilterType
    {
        /// <summary>
        /// The show matching preference
        /// </summary>
        ShowMatchingPreference,

        /// <summary>
        /// The show all group members
        /// </summary>
        ShowAllGroupMembers
    }

    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Use this as the Content of a ~/api/Attendances/GetSchedulerResources POST" )]
    public class SchedulerResourceParameters
    {
        /// <summary>
        /// Gets or sets the attendance occurrence group identifier.
        /// </summary>
        /// <value>
        /// The attendance occurrence group identifier.
        /// </value>
        public int AttendanceOccurrenceGroupId { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence schedule identifier.
        /// </summary>
        /// <value>
        /// The attendance occurrence schedule identifier.
        /// </value>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use AttendanceOccurrenceScheduleIds instead" )]
        public int AttendanceOccurrenceScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence schedules
        /// </summary>
        /// <value>
        /// The attendance occurrence schedules
        /// </value>
        public int[] AttendanceOccurrenceScheduleIds { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence location ids.
        /// </summary>
        /// <value>The attendance occurrence location ids.</value>
        public int[] AttendanceOccurrenceLocationIds { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence sunday date.
        /// </summary>
        /// <value>
        /// The attendance occurrence sunday date.
        /// </value>
        public DateTime AttendanceOccurrenceSundayDate { get; set; }

        /// <summary>
        /// Gets or sets the resource group identifier.
        /// </summary>
        /// <value>
        /// The resource group identifier.
        /// </value>
        public int? ResourceGroupId { get; set; }

        /// <summary>
        /// If we just need the data for a specific person id
        /// </summary>
        /// <value>
        /// The limit to person identifier.
        /// </value>
        public int? LimitToPersonId { get; set; }

        /// <summary>
        /// Gets or sets the type of the group member filter.
        /// </summary>
        /// <value>
        /// The type of the group member filter.
        /// </value>
        public SchedulerResourceGroupMemberFilterType? GroupMemberFilterType { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource list source.
        /// </summary>
        /// <value>The type of the resource list source.</value>
        public GroupSchedulerResourceListSourceType? ResourceListSourceType { get; set; }

        /// <summary>
        /// Gets or sets the resource data view identifier.
        /// </summary>
        /// <value>
        /// The resource data view identifier.
        /// </value>
        public int? ResourceDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the resource additional person ids.
        /// </summary>
        /// <value>
        /// The resource additional person ids.
        /// </value>
        public List<int> ResourceAdditionalPersonIds { get; set; }
    }

    #endregion Group Scheduling related classes and types

    /// <summary>
    /// 
    /// </summary>
    public static class AttendanceQryExtensions
    {
        /// <summary>
        /// Gets the attendance with a SummaryDateTime column by Week, Month, or Year
        /// </summary>
        /// <param name="qryAttendance">The attendance queryable.</param>
        /// <param name="summarizeBy">The group by.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceService.AttendanceWithSummaryDateTime> GetAttendanceWithSummaryDateTime( this IQueryable<Attendance> qryAttendance, ChartGroupBy summarizeBy )
        {
            IQueryable<AttendanceService.AttendanceWithSummaryDateTime> qryAttendanceGroupedBy;

            if ( summarizeBy == ChartGroupBy.Week )
            {
                qryAttendanceGroupedBy = qryAttendance.Select( a => new AttendanceService.AttendanceWithSummaryDateTime
                {
                    SummaryDateTime = a.Occurrence.SundayDate,
                    Attendance = a
                } );
            }
            else if ( summarizeBy == ChartGroupBy.Month )
            {
                qryAttendanceGroupedBy = qryAttendance.Select( a => new AttendanceService.AttendanceWithSummaryDateTime
                {
                    SummaryDateTime = ( DateTime ) SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "day", a.Occurrence.SundayDate ) + 1, a.Occurrence.SundayDate ),
                    Attendance = a
                } );
            }
            else if ( summarizeBy == ChartGroupBy.Year )
            {
                qryAttendanceGroupedBy = qryAttendance.Select( a => new AttendanceService.AttendanceWithSummaryDateTime
                {
                    SummaryDateTime = ( DateTime ) SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "dayofyear", a.Occurrence.SundayDate ) + 1, a.Occurrence.SundayDate ),
                    Attendance = a
                } );
            }
            else
            {
                // shouldn't happen
                qryAttendanceGroupedBy = qryAttendance.Select( a => new AttendanceService.AttendanceWithSummaryDateTime
                {
                    SummaryDateTime = a.Occurrence.SundayDate,
                    Attendance = a
                } );
            }

            return qryAttendanceGroupedBy;
        }

        /// <summary>
        /// Where the attendance occurred at an active location (or the location is null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> WhereHasActiveLocation( this IQueryable<Attendance> query )
        {
            // Null is allowed since the location relationship is not required
            return query.Where( a => a.Occurrence.Location == null || a.Occurrence.Location.IsActive );
        }

        /// <summary>
        /// Where the attendance occurred with an active schedule (or the schedule is null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> WhereHasActiveSchedule( this IQueryable<Attendance> query )
        {
            // Null is allowed since the schedule relationship is not required
            return query.Where( a => a.Occurrence.Schedule == null || a.Occurrence.Schedule.IsActive );
        }

        /// <summary>
        /// Where the attendance occurred with an active group (or the group is null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> WhereHasActiveGroup( this IQueryable<Attendance> query )
        {
            // Null is allowed since the group relationship is not required
            return query.Where( a => a.Occurrence.Group == null || a.Occurrence.Group.IsActive );
        }

        /// <summary>
        /// Where the entities are active (deduced from the group, schedule, and location all being active or null).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> WhereDeducedIsActive( this IQueryable<Attendance> query )
        {
            return query
                .WhereHasActiveLocation()
                .WhereHasActiveSchedule()
                .WhereHasActiveGroup();
        }

        /// <summary>
        /// Returns a queryable of Attendance where the person is scheduled, or attended even if they weren't scheduled
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> WhereScheduledOrCheckedIn( this IQueryable<Attendance> query )
        {
            return query.Where( a => ( a.RequestedToAttend == true || a.ScheduledToAttend == true ) || a.DidAttend == true );
        }

        /// <summary>
        /// Returns a queryable of Attendance where the person is scheduled (RequestedToAttend), limited to scheduled persons with the specified <see cref="ScheduledAttendanceItemStatus">scheduledAttendanceItemStatuses</see> 
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="scheduledAttendanceItemStatuses">The scheduled attendance item statuses.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> WhereHasScheduledAttendanceItemStatus( this IQueryable<Attendance> query, ScheduledAttendanceItemStatus[] scheduledAttendanceItemStatuses )
        {
            Expression whereExpression = null;
            var parameterExpression = Expression.Parameter( typeof( Attendance ), "a" );
            var requestedToAttendProperty = Expression.Property( parameterExpression, "RequestedToAttend" );
            var scheduledToAttendProperty = Expression.Property( parameterExpression, "ScheduledToAttend" );
            var rsvpProperty = Expression.Property( parameterExpression, "RSVP" );
            var trueConstant = Expression.Constant( true, typeof( bool? ) );

            foreach ( var scheduledAttendanceItemStatus in scheduledAttendanceItemStatuses )
            {
                Expression statusExpression = null;
                if ( scheduledAttendanceItemStatus == ScheduledAttendanceItemStatus.Pending )
                {
                    statusExpression = Expression.Equal( requestedToAttendProperty, trueConstant );
                    var rsvpExpressionMaybe = Expression.Equal( rsvpProperty, Expression.Constant( RSVP.Maybe ) );
                    var rsvpExpressionUnknown = Expression.Equal( rsvpProperty, Expression.Constant( RSVP.Unknown ) );
                    var rsvpExpression = Expression.OrElse( rsvpExpressionMaybe, rsvpExpressionUnknown );
                    statusExpression = Expression.AndAlso( statusExpression, rsvpExpression );
                }
                else if ( scheduledAttendanceItemStatus == ScheduledAttendanceItemStatus.Declined )
                {
                    statusExpression = Expression.Equal( requestedToAttendProperty, trueConstant );
                    statusExpression = Expression.AndAlso( statusExpression, Expression.Equal( rsvpProperty, Expression.Constant( RSVP.No ) ) );
                }
                else
                {
                    statusExpression = Expression.Equal( scheduledToAttendProperty, trueConstant );
                }

                if ( whereExpression == null )
                {
                    whereExpression = statusExpression;
                }
                else
                {
                    whereExpression = Expression.OrElse( whereExpression, statusExpression );
                }
            }

            var scheduledAttendanceQuery = query.Where( parameterExpression, whereExpression );

            return scheduledAttendanceQuery;
        }
    }
}

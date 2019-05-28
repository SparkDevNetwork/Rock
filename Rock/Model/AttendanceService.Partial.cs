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
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using Rock.Chart;
using Rock.Communication;
using Rock.Data;

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
            // Check to see if an occurrence exists already
            var occurrenceService = new AttendanceOccurrenceService( ( RockContext ) Context );
            var occurrence = occurrenceService.Get( checkinDateTime.Date, groupId, locationId, scheduleId );

            if ( occurrence == null )
            {
                // If occurrence does not yet exists, use a new context and create it
                using ( var newContext = new RockContext() )
                {
                    occurrence = new AttendanceOccurrence
                    {
                        OccurrenceDate = checkinDateTime.Date,
                        GroupId = groupId,
                        LocationId = locationId,
                        ScheduleId = scheduleId,
                    };

                    var newOccurrenceService = new AttendanceOccurrenceService( newContext );
                    newOccurrenceService.Add( occurrence );
                    newContext.SaveChanges();

                    // Query for the new occurrence using original context.
                    occurrence = occurrenceService.Get( occurrence.Id );
                }
            }

            // If we still don't have an occurrence record (i.e. validation failed) return null 
            if ( occurrence == null )
                return null;

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
                attendance = ( ( RockContext ) Context ).Attendances.Create();
                {
                    attendance.Occurrence = occurrence;
                    attendance.OccurrenceId = occurrence.Id;
                    attendance.PersonAliasId = personAliasId;
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
                qryAttendance = qryAttendance.Where( a => a.Occurrence.OccurrenceDate >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                qryAttendance = qryAttendance.Where( a => a.Occurrence.OccurrenceDate < endDate.Value );
            }

            if ( dataViewId.HasValue )
            {
                var rockContext = ( RockContext ) this.Context;

                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var personService = new PersonService( rockContext );

                    var errorMessages = new List<string>();
                    var paramExpression = personService.ParameterExpression;
                    var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

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
        public static DataSet GetAttendanceAnalyticsNonAttendees( List<int> GroupTypeIds, List<int> groupIds, DateTime? start, DateTime? end,
            List<int> campusIds, bool? includeNullCampusIds, List<int> scheduleIds, bool? IncludeParentsWithChild, bool? IncludeChildrenWithParents )
        {
            var parameters = GetAttendanceAnalyticsParameters( GroupTypeIds, groupIds, start, end, campusIds, includeNullCampusIds, scheduleIds, IncludeParentsWithChild, IncludeChildrenWithParents );
            return DbService.GetDataSet( "spCheckin_AttendanceAnalyticsQuery_NonAttendees", System.Data.CommandType.StoredProcedure, parameters, 300 );
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
            var attendancesBySystemEmailTypeList = sendConfirmationAttendancesQueryList.GroupBy( a => a.Occurrence.Group.GroupType.ScheduleConfirmationSystemEmailId ).Where( a => a.Key.HasValue ).Select( s => new
            {
                ScheduleConfirmationSystemEmailId = s.Key.Value,
                Attendances = s.ToList()
            } ).ToList();

            var rockContext = this.Context as RockContext;

            List<Exception> exceptionList = new List<Exception>();

            foreach ( var attendancesBySystemEmailType in attendancesBySystemEmailTypeList )
            {
                var scheduleConfirmationSystemEmail = new SystemEmailService( rockContext ).GetNoTracking( attendancesBySystemEmailType.ScheduleConfirmationSystemEmailId );

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
                        var recipient = attendancesByPerson.Person.Email;
                        var attendances = attendancesByPerson.Attendances;

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Attendance", attendances.FirstOrDefault() );
                        mergeFields.Add( "Attendances", attendances );
                        emailMessage.AddRecipient( new RecipientData( recipient, mergeFields ) );
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
        public int SendScheduleReminderSystemEmails( IQueryable<Attendance> sendReminderAttendancesQuery )
        {
            int emailsSent = 0;
            var sendReminderAttendancesQueryList = sendReminderAttendancesQuery.ToList();
            var attendancesBySystemEmailTypeList = sendReminderAttendancesQueryList.GroupBy( a => a.Occurrence.Group.GroupType.ScheduleReminderSystemEmailId ).Where( a => a.Key.HasValue ).Select( s => new
            {
                ScheduleReminderSystemEmailId = s.Key.Value,
                Attendances = s.ToList()
            } ).ToList();

            var rockContext = this.Context as RockContext;

            foreach ( var attendancesBySystemEmailType in attendancesBySystemEmailTypeList )
            {
                var scheduleReminderSystemEmail = new SystemEmailService( rockContext ).GetNoTracking( attendancesBySystemEmailType.ScheduleReminderSystemEmailId );

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
                        var recipient = attendancesByPerson.Person.Email;
                        var attendances = attendancesByPerson.Attendances;

                        foreach ( var attendance in attendances )
                        {
                            attendance.ScheduleReminderSent = true;
                        }

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Attendance", attendances.FirstOrDefault() );
                        mergeFields.Add( "Attendances", attendances );
                        emailMessage.AddRecipient( new RecipientData( recipient, mergeFields ) );
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

        #region GroupScheduling Related

        /// <summary>
        /// Gets the person schedule exclusion query for the specified Group and OccurrenceDate
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <returns></returns>
        private IQueryable<PersonScheduleExclusion> GetPersonScheduleExclusionQuery( int groupId, DateTime occurrenceDate )
        {
            var rockContext = this.Context as RockContext;
            var personScheduleExclusionQueryForOccurrence = new PersonScheduleExclusionService( rockContext ).Queryable()
                .Where( a => !a.GroupId.HasValue || ( a.GroupId.Value == groupId ) )
                    .Where( a => occurrenceDate >= a.StartDate && occurrenceDate <= a.EndDate );

            return personScheduleExclusionQueryForOccurrence;
        }

        /// <summary>
        /// Gets a list of available the scheduler resources (people) based on the options specified in schedulerResourceParameters 
        /// </summary>
        /// <param name="schedulerResourceParameters">The scheduler resource parameters.</param>
        /// <returns></returns>
        public IEnumerable<SchedulerResource> GetSchedulerResources( SchedulerResourceParameters schedulerResourceParameters )
        {
            var rockContext = this.Context as RockContext;

            var occurrenceSchedule = new ScheduleService( rockContext ).GetNoTracking( schedulerResourceParameters.AttendanceOccurrenceScheduleId );

            List<SchedulerResource> schedulerResourceList = new List<SchedulerResource>();

            if ( occurrenceSchedule == null )
            {
                // schedule not found, return empty list
                return schedulerResourceList;
            }

            var occurrenceSundayDate = schedulerResourceParameters.AttendanceOccurrenceSundayDate;
            var occurrenceSundayWeekStartDate = occurrenceSundayDate.AddDays( -6 );

            var scheduleOccurrenceDateTime = occurrenceSchedule.GetNextStartDateTime( occurrenceSundayWeekStartDate );

            if ( scheduleOccurrenceDateTime == null )
            {
                // no next start date for schedule, return empty list
                return schedulerResourceList;
            }

            var groupMemberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );
            var attendanceService = new AttendanceService( rockContext );
            IQueryable<GroupMember> groupMemberQry = null;
            IQueryable<Person> personQry = null;

            HashSet<int> groupMemberIdsThatLackGroupRequirements;

            groupMemberIdsThatLackGroupRequirements = null;

            if ( schedulerResourceParameters.ResourceGroupId.HasValue )
            {
                groupMemberQry = groupMemberService.Queryable().Where( a => a.GroupId == schedulerResourceParameters.ResourceGroupId.Value );

                var resourceGroup = groupService.GetNoTracking( schedulerResourceParameters.ResourceGroupId.Value );
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
                    List<string> errorMessages;
                    personQry = dataView.GetQuery( null, rockContext, null, out errorMessages ) as IQueryable<Person>;
                }
            }

            var scheduleOccurrenceDate = scheduleOccurrenceDateTime.Value.Date;

            if ( groupMemberQry != null )
            {
                var resourceListQuery = groupMemberQry.Select( a => new
                {
                    GroupMemberId = a.Id,
                    a.PersonId,
                    a.Note,
                    a.Person.NickName,
                    a.Person.LastName,
                    a.Person.SuffixValueId,
                    a.Person.RecordTypeValueId,
                    a.ScheduleTemplateId,
                    a.ScheduleStartDate
                } );

                if ( schedulerResourceParameters.GroupMemberFilterType == SchedulerResourceGroupMemberFilterType.ShowMatchingPreference )
                {
                    // if using the MatchingPreference filter, limit to people that have ScheduleTemplates that would include the scheduled date
                    resourceListQuery = resourceListQuery.Where( a => a.ScheduleTemplateId.HasValue && a.ScheduleStartDate.HasValue );
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

                    TimeSpan occurrenceScheduledTime = scheduleOccurrenceDateTime.Value.TimeOfDay;

                    List<int> matchingScheduleGroupMemberIdList = new List<int>();

                    // get first scheduled occurrence for the selected "sunday week", which would be from the start of the preceding Monday to the end of the Sunday
                    var beginDateTime = occurrenceSundayWeekStartDate;
                    var endDateTime = occurrenceSundayDate.AddDays( 1 );

                    foreach ( var groupMember in resourceList )
                    {
                        Schedule schedule = scheduleTemplateLookup.GetValueOrNull( groupMember.ScheduleTemplateId.Value );
                        if ( schedule != null )
                        {
                            var scheduleStartDateTimeOverride = groupMember.ScheduleStartDate.Value.Add( occurrenceScheduledTime );
                            var matches = schedule.GetOccurrences( beginDateTime, endDateTime, scheduleStartDateTimeOverride );
                            if ( matches.Any() )
                            {
                                matchingScheduleGroupMemberIdList.Add( groupMember.GroupMemberId );
                            }
                        }
                    }

                    resourceList = resourceList.Where( a => matchingScheduleGroupMemberIdList.Contains( a.GroupMemberId ) ).ToList();
                }

                schedulerResourceList = resourceList.Select( a => new SchedulerResource
                {
                    PersonId = a.PersonId,
                    GroupMemberId = a.GroupMemberId,
                    Note = a.Note,
                    PersonNickName = a.NickName,
                    PersonLastName = a.LastName,
                    PersonName = Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    HasGroupRequirementsConflict = groupMemberIdsThatLackGroupRequirements?.Contains( a.GroupMemberId ) ?? false,
                } ).ToList();
            }
            else if ( personQry != null )
            {
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
                    GroupMemberId = null,
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
                    GroupMemberId = null,
                    Note = null,
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
                IQueryable<PersonScheduleExclusion> personScheduleExclusionQueryForOccurrence = GetPersonScheduleExclusionQuery( schedulerResourceParameters.AttendanceOccurrenceGroupId, scheduleOccurrenceDate );

                // Get the last time they attended the group (in any location or schedule)
                var lastAttendedDateTimeQuery = attendanceService.Queryable()
                    .Where( a => a.DidAttend == true
                        && a.Occurrence.GroupId == schedulerResourceParameters.AttendanceOccurrenceGroupId
                        && a.PersonAliasId.HasValue );

                if ( groupMemberQry != null )
                {
                    lastAttendedDateTimeQuery = lastAttendedDateTimeQuery.Where( a => groupMemberQry.Any( m => m.PersonId == a.PersonAlias.PersonId ) );
                    personScheduleExclusionQueryForOccurrence = personScheduleExclusionQueryForOccurrence.Where( a => groupMemberQry.Any( m => m.PersonId == a.PersonAlias.PersonId ) );
                }
                else if ( personQry != null )
                {
                    lastAttendedDateTimeQuery = lastAttendedDateTimeQuery.Where( a => personQry.Any( p => p.Id == a.PersonAlias.PersonId ) );
                    personScheduleExclusionQueryForOccurrence = personScheduleExclusionQueryForOccurrence.Where( a => personQry.Any( p => p.Id == a.PersonAlias.PersonId ) );
                }

                var personIdLastAttendedDateTimeLookup = lastAttendedDateTimeQuery
                    .GroupBy( a => a.PersonAlias.PersonId )
                    .Select( a => new
                    {
                        PersonId = a.Key,
                        LastScheduledDate = a.Max( x => x.StartDateTime )
                    } )
                    .ToDictionary( k => k.PersonId, v => v.LastScheduledDate );

                var scheduledAttendanceGroupIdsLookup = attendanceService.Queryable()
                    .Where( a => ( a.RequestedToAttend == true || a.ScheduledToAttend == true )
                              && a.Occurrence.ScheduleId == schedulerResourceParameters.AttendanceOccurrenceScheduleId
                              && a.Occurrence.OccurrenceDate == scheduleOccurrenceDate
                              && a.Occurrence.GroupId.HasValue )
                    .GroupBy( a => a.PersonAlias.PersonId )
                    .Select( a => new
                    {
                        PersonId = a.Key,
                        ScheduledOccurrenceGroupIds = a.Select( x => x.Occurrence.GroupId.Value ).ToList()
                    } )
                    .ToDictionary( k => k.PersonId, v => v.ScheduledOccurrenceGroupIds );

                // select personScheduleExclusionQueryForOccurrence into a List just in case resourcePersonIds is large (10000+ ids) , so we don't an exception when building personScheduleExclusionQueryForOccurrencePersonIds
                var personScheduleExclusionForOccurrencePersonIdList = personScheduleExclusionQueryForOccurrence.Select( s => s.PersonAlias.PersonId ).ToList();

                HashSet<int> personScheduleExclusionQueryForOccurrencePersonIds;

                if ( true || personScheduleExclusionForOccurrencePersonIdList.Any() )
                {
                    var resourcePersonIdHash = new HashSet<int>( schedulerResourceList.Select( a => a.PersonId ).Distinct().ToArray() );
                    personScheduleExclusionQueryForOccurrencePersonIds = new HashSet<int>( personScheduleExclusionForOccurrencePersonIdList.Where( personId => resourcePersonIdHash.Contains( personId ) ).Distinct().ToList() );
                }
                else
                {
                    personScheduleExclusionQueryForOccurrencePersonIds = new HashSet<int>();
                }

                foreach ( var schedulerResource in schedulerResourceList )
                {
                    schedulerResource.LastAttendanceDateTime = personIdLastAttendedDateTimeLookup.GetValueOrNull( schedulerResource.PersonId );
                    var scheduledForGroupIds = scheduledAttendanceGroupIdsLookup.GetValueOrNull( schedulerResource.PersonId );

                    schedulerResource.ConfirmationStatus = ScheduledAttendanceItemStatus.Unscheduled.ConvertToString( false ).ToLower();

                    // see if they are scheduled for some other group during this occurrence
                    schedulerResource.HasSchedulingConflict = scheduledForGroupIds?.Any( groupId => groupId != schedulerResourceParameters.AttendanceOccurrenceGroupId ) ?? false;

                    // see if they are scheduled for this group already for this occurrence
                    schedulerResource.IsAlreadyScheduledForGroup = scheduledForGroupIds?.Any( groupId => groupId == schedulerResourceParameters.AttendanceOccurrenceGroupId ) ?? false;

                    schedulerResource.HasBlackoutConflict = personScheduleExclusionQueryForOccurrencePersonIds.Contains( schedulerResource.PersonId );
                }

                // remove anybody that is already scheduled for this group, and sort by person
                schedulerResourceList = schedulerResourceList
                    .Where( a => a.IsAlreadyScheduledForGroup != true )
                    .OrderBy( a => a.PersonLastName ).ThenBy( a => a.PersonNickName ).ThenBy( a => a.PersonId ).ToList();

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
            var conflictingScheduledAttendancesQuery = this.Queryable();

            var attendanceOccurrenceInfo = new AttendanceOccurrenceService( this.Context as RockContext ).GetSelect( attendanceOccurrenceId, s => new
            {
                s.GroupId,
                s.ScheduleId,
                s.OccurrenceDate
            } );

            int scheduleId = attendanceOccurrenceInfo.ScheduleId ?? 0;
            var groupId = attendanceOccurrenceInfo.GroupId ?? 0;
            DateTime occurrenceDate = attendanceOccurrenceInfo.OccurrenceDate;

            IQueryable<PersonScheduleExclusion> personScheduleExclusionQueryForOccurrence = GetPersonScheduleExclusionQuery( groupId, occurrenceDate );

            var scheduledAttendancesQuery = this.Queryable()
                .Where( a => a.OccurrenceId == attendanceOccurrenceId
                 && a.PersonAliasId.HasValue
                 && ( a.RequestedToAttend == true || a.ScheduledToAttend == true ) )
                .Select( a => new
                {
                    a.Id,
                    a.RSVP,
                    a.ScheduledToAttend,
                    a.PersonAlias.PersonId,
                    a.PersonAlias.Person.NickName,
                    a.PersonAlias.Person.LastName,
                    a.PersonAlias.Person.SuffixValueId,
                    a.PersonAlias.Person.RecordTypeValueId,
                    // set HasSchedulingConflict = true if the same person is requested/scheduled for another attendance within the same ScheduleId/Date
                    HasSchedulingConflict = conflictingScheduledAttendancesQuery.Any( c => c.Id != a.Id
                                                                                    && c.PersonAlias.PersonId == a.PersonAlias.PersonId
                                                                                    && ( c.RequestedToAttend == true || c.ScheduledToAttend == true )
                                                                                    && c.Occurrence.ScheduleId == scheduleId
                                                                                    && c.Occurrence.OccurrenceDate == occurrenceDate ),
                    HasBlackoutConflict = personScheduleExclusionQueryForOccurrence.Where( e => e.PersonAlias.PersonId == a.PersonAlias.PersonId ).Any()

                } );

            var result = scheduledAttendancesQuery.ToList().Select( a =>
            {
                ScheduledAttendanceItemStatus status = ScheduledAttendanceItemStatus.Pending;
                if ( a.RSVP == RSVP.No )
                {
                    status = ScheduledAttendanceItemStatus.Declined;
                }
                else if ( a.ScheduledToAttend == true )
                {
                    status = ScheduledAttendanceItemStatus.Confirmed;
                }

                return new SchedulerResourceAttend
                {
                    AttendanceId = a.Id,
                    ConfirmationStatus = status.ConvertToString( false ).ToLower(),
                    PersonId = a.PersonId,
                    PersonName = Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    HasSchedulingConflict = a.HasSchedulingConflict,
                    HasBlackoutConflict = a.HasBlackoutConflict
                };
            } );

            return result;
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

            var startDate = sundayDate.Date.AddDays( -6 );
            var endDate = sundayDate.Date;

            var groupLocationQry = new GroupLocationService( rockContext ).Queryable().Where( a => a.GroupId == groupId );
            var scheduleList = groupLocationQry.SelectMany( a => a.Schedules ).Distinct().AsNoTracking().ToList();
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
                    var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).GetOrCreateAttendanceOccurrence( scheduleOccurrenceDate.OccurrenceDate.Value, scheduleOccurrenceDate.ScheduleId, locationId, groupId );
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
            var attendanceOccurrencesQry = new AttendanceOccurrenceService( rockContext ).GetByIds( attendanceOccurrenceIdList )
                .Where( a => a.GroupId.HasValue && a.LocationId.HasValue && a.ScheduleId.HasValue );

            // sort the occurrences by the Date, then by the associated GroupLocation.Order then by Location.name
            var sortedAttendanceOccurrenceList = attendanceOccurrencesQry
                .Join( groupLocationQry,
                        ao => new { GroupId = ao.GroupId.Value, LocationId = ao.LocationId.Value },
                        gl => new { gl.GroupId, gl.LocationId }, ( ao, gl ) => new
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
            int locationId = attendanceOccurrence.LocationId.Value;
            int scheduleId = attendanceOccurrence.ScheduleId.Value;
            int attendanceOccurrenceId = attendanceOccurrence.Id;

            // get the capacity settings for the Group/Location/Schedule. NOTE: The Database and UI don't prevent duplicate LocationIds for a group,
            // but since that really doesn't make sense, just grab the first one
            int? desiredCapacity = new GroupLocationService( this.Context as RockContext ).Queryable()
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
                && a.Occurrence.LocationId == null ).ToList();

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

            // get all available resources (group members that have a schedule template matching the occurrence date and schedule)
            var schedulerResourceParameters = new SchedulerResourceParameters
            {
                AttendanceOccurrenceGroupId = attendanceOccurrence.GroupId.Value,
                AttendanceOccurrenceScheduleId = attendanceOccurrence.ScheduleId.Value,
                AttendanceOccurrenceSundayDate = attendanceOccurrence.OccurrenceDate.SundayDate(),
                ResourceGroupId = attendanceOccurrence.GroupId.Value,
                GroupMemberFilterType = SchedulerResourceGroupMemberFilterType.ShowMatchingPreference
            };

            var schedulerResources = this.GetSchedulerResources( schedulerResourceParameters );

            // only grab resources that haven't been scheduled already, and don't have a conflict (blackout or scheduled for some other group)
            // Also, only include resources that have a GroupMemberId since those would be the only ones that would be auto-schedulable
            var scheduleResourcesGroupMemberIds = schedulerResources
                .Where( a => a.GroupMemberId.HasValue && a.IsAlreadyScheduledForGroup == false && a.HasConflict == false )
                .Select( a => a.GroupMemberId ).ToList();

            if ( !scheduleResourcesGroupMemberIds.Any() )
            {
                // nobody to add as scheduled resource, so return
                return;
            }

            // get GroupMemberAssignmments for the groupMembers returned from schedulerResources for this occurrence's locationId and scheduleId
            var groupMemberAssignmentsQuery = new GroupMemberAssignmentService( this.Context as RockContext )
                .Queryable()
                .Where( a => scheduleResourcesGroupMemberIds.Contains( a.GroupMemberId ) )
                .Where( a => ( !a.LocationId.HasValue || a.LocationId == locationId )
                 && ( !a.ScheduleId.HasValue || a.ScheduleId == scheduleId ) );

            // Exclude GroupMemberAssignments that don't have a schedule or location set (since that means they don't want to be auto-scheduled)
            groupMemberAssignmentsQuery = groupMemberAssignmentsQuery.Where( a => a.LocationId.HasValue || a.ScheduleId.HasValue );

            var groupMemberAssignmentsList = groupMemberAssignmentsQuery.Select( a => new GroupMemberAssignmentInfo
            {
                GroupMemberId = a.GroupMember.Id,
                PersonId = a.GroupMember.PersonId,
                LocationId = a.LocationId,
                ScheduleId = a.ScheduleId,
                SpecificLocationAndSchedule = ( a.LocationId.HasValue && a.ScheduleId.HasValue ),
                SpecificScheduleOnly = ( !a.LocationId.HasValue && a.ScheduleId.HasValue ),
                SpecificLocationOnly = ( !a.LocationId.HasValue && !a.ScheduleId.HasValue ),
            } ).ToList();

            if ( !groupMemberAssignmentsList.Any() )
            {
                // nobody with assignments to assign, so return
                return;
            }



            // randomize order of group member assignments
            groupMemberAssignmentsList = groupMemberAssignmentsList.OrderBy( a => Guid.NewGuid() ).ToList();

            // use a new RockContext to use for adding attending resources so that get can get saved to the database without saving any changes associated with the current rockContext
            var groupAssignmentAttendanceRockContext = new RockContext();
            var groupAssignmentAttendanceService = new AttendanceService( groupAssignmentAttendanceRockContext );

            // loop through the most specific assignments first (both LocationId and ScheduleId are assigned), then where only Schedule is specified, followed by ones where Location is specified
            var groupMemberAssignmentSortedBySpecificity = groupMemberAssignmentsList
                .OrderBy( a => a.SpecificLocationAndSchedule )
                .ThenBy( a => a.SpecificScheduleOnly )
                .ThenBy( a => a.SpecificLocationOnly ).ToList();

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
                scheduledAttendance = new Attendance
                {
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
                .Where( a => a.RSVP == RSVP.Maybe || a.RSVP == RSVP.Unknown );
        }

        /// <summary>
        /// Gets a Queryable of Attendance Records that are scheduled and confirmed.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Attendance> GetConfirmedScheduled()
        {
            return this.Queryable().Where( a => a.ScheduledToAttend == true && a.RSVP != RSVP.No && a.DidAttend != true );
        }

        private class GroupMemberAssignmentInfo
        {
            public int GroupMemberId { get; set; }
            public int PersonId { get; set; }
            public bool SpecificLocationAndSchedule { get; set; }
            public bool SpecificScheduleOnly { get; set; }
            public bool SpecificLocationOnly { get; set; }
            public int? LocationId { get; internal set; }
            public int? ScheduleId { get; internal set; }
        }

        #endregion GroupScheduling Related
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
        /// The status.
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
        /// Gets a value indicating whether this instance has conflict.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has conflict; otherwise, <c>false</c>.
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
        /// Gets or sets a value indicating whether this instance has blackout conflict.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has blackout conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasBlackoutConflict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has group requirements conflict.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has group requirements conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasGroupRequirementsConflict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has scheduling conflict with some other group for this schedule+date
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has scheduling conflict; otherwise, <c>false</c>.
        /// </value>
        public bool HasSchedulingConflict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is already scheduled for this group+schedule+date
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has scheduling conflict; otherwise, <c>false</c>.
        /// </value>
        public bool? IsAlreadyScheduledForGroup { get; set; }
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
    public enum SchedulerResourceListSourceType
    {
        /// <summary>
        /// The group
        /// </summary>
        Group,

        /// <summary>
        /// The alternate group
        /// </summary>
        [Description("Alt Group")]
        AlternateGroup,

        /// <summary>
        /// The data view
        /// </summary>
        DataView
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
        public int AttendanceOccurrenceScheduleId { get; set; }

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
        /// Gets or sets the type of the group member filter.
        /// </summary>
        /// <value>
        /// The type of the group member filter.
        /// </value>
        public SchedulerResourceGroupMemberFilterType? GroupMemberFilterType { get; set; }

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
    }
}

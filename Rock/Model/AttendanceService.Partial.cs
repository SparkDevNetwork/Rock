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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;

using Rock.Chart;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.Attendance"/> entity objects
    /// </summary>
    public partial class AttendanceService
    {
        /// <summary>
        /// Returns a specific <see cref="Rock.Model.Attendance"/> record.
        /// </summary>
        /// <param name="date">A <see cref="System.DateTime"/> representing the the date attended.</param>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> </param>
        /// <param name="scheduleId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/></param>
        /// <param name="groupId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/>.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/></param>
        /// <returns>The first <see cref="Rock.Model.Attendance"/> entity that matches the provided values.</returns>
        public Attendance Get( DateTime date, int locationId, int scheduleId, int groupId, int personId )
        {
            DateTime beginDate = date.Date;
            DateTime endDate = beginDate.AddDays( 1 );

            return Queryable( "Group,Schedule,PersonAlias.Person" )
                .Where( a =>
                    a.StartDateTime >= beginDate &&
                    a.StartDateTime < endDate &&
                    a.LocationId == locationId &&
                    a.ScheduleId == scheduleId &&
                    a.GroupId == groupId &&
                    a.PersonAlias.PersonId == personId )
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Attendance"/> for a <see cref="Rock.Model.Location"/> on a specified date.
        /// </summary>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/></param>
        /// <param name="date">A <see cref="System.DateTime"/> representing the date attended.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Attendance"/> entities for a specific date and location.</returns>
        public IQueryable<Attendance> GetByDateAndLocation( DateTime date, int locationId )
        {
            DateTime beginDate = date.Date;
            DateTime endDate = beginDate.AddDays( 1 );

            return Queryable( "Group,Schedule,PersonAlias.Person" )
                .Where( a =>
                    a.StartDateTime >= beginDate &&
                    a.StartDateTime < endDate &&
                    !a.EndDateTime.HasValue &&
                    a.LocationId == locationId &&
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value );
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        /// <param name="graphBy">The graph by.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <returns></returns>
        public IEnumerable<IChartData> GetChartData( ChartGroupBy groupBy = ChartGroupBy.Week, AttendanceGraphBy graphBy = AttendanceGraphBy.Total, DateTime? startDate = null, DateTime? endDate = null, string groupIds = null, string campusIds = null )
        {
            var qryAttendance = Queryable().AsNoTracking().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );

            if ( startDate.HasValue )
            {
                qryAttendance = qryAttendance.Where( a => a.StartDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                qryAttendance = qryAttendance.Where( a => a.StartDateTime < endDate.Value );
            }

            if ( !string.IsNullOrWhiteSpace( groupIds ) )
            {
                var groupIdList = groupIds.Split( ',' ).AsIntegerList();
                qryAttendance = qryAttendance.Where( a => a.GroupId.HasValue && groupIdList.Contains( a.GroupId.Value ) );
            }

            if ( !string.IsNullOrWhiteSpace( campusIds ) )
            {
                var campusIdList = campusIds.Split( ',' ).AsIntegerList();
                qryAttendance = qryAttendance.Where( a => a.CampusId.HasValue && campusIdList.Contains( a.CampusId.Value ) );
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
                    Id = a.Attendance.GroupId,
                    Name = a.Attendance.Group.Name
                },
                Schedule = new
                {
                    Id = a.Attendance.ScheduleId,
                    Name = a.Attendance.Schedule.Name
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
                    SeriesId = "Total",
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
                    SeriesId = a.Key.Series.Name,
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
                    SeriesId = a.Key.Series.Name,
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
                    SeriesId = a.Key.Series.Name,
                    YValue = a.Count
                } ).ToList();
            }

            return result;
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
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AttendanceQryExtensions
    {
        /// <summary>
        /// Gets the attendance with a SummaryDateTime column by Week, Month, or Year
        /// </summary>
        /// <param name="qryAttendance">The qry attendance.</param>
        /// <param name="summarizeBy">The group by.</param>
        /// <returns></returns>
        public static IQueryable<AttendanceService.AttendanceWithSummaryDateTime> GetAttendanceWithSummaryDateTime( this IQueryable<Attendance> qryAttendance, ChartGroupBy summarizeBy )
        {
            //// for Date SQL functions, borrowed some ideas from http://stackoverflow.com/a/1177529/1755417 and http://stackoverflow.com/a/133101/1755417 and http://stackoverflow.com/a/607837/1755417

            var knownSunday = new DateTime( 1966, 1, 30 );    // Because we can't use the @@DATEFIRST option in Linq to query how DATEPART("weekday",) will work, use a known Sunday date instead.
            var qryWithSundayDate = qryAttendance.Select( a => new
            {
                Attendance = a,
                SundayDate = SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day",
                            "1900-01-01",
                            SqlFunctions.DateAdd( "day",
                                ( ( ( SqlFunctions.DatePart( "weekday", knownSunday ) + 7 ) - SqlFunctions.DatePart( "weekday", a.StartDateTime ) ) % 7 ),
                                a.StartDateTime
                            )
                        ),
                        "1900-01-01"
                    )
            } );

            var qryAttendanceGroupedBy = qryWithSundayDate.Select( a => new AttendanceService.AttendanceWithSummaryDateTime
            {
                // Build a CASE statement to group by week, or month, or year
                SummaryDateTime = (DateTime)(

                    // GroupBy Week with Monday as FirstDayOfWeek ( +1 ) and Sunday as Summary Date ( +6 )
                    summarizeBy == ChartGroupBy.Week ? a.SundayDate :

                    // GroupBy Month 
                    summarizeBy == ChartGroupBy.Month ? SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "day", a.SundayDate ) + 1, a.SundayDate ) :

                    // GroupBy Year
                    summarizeBy == ChartGroupBy.Year ? SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "dayofyear", a.SundayDate ) + 1, a.SundayDate ) :

                    // shouldn't happen
                    null
                ),
                Attendance = a.Attendance
            } );

            return qryAttendanceGroupedBy;
        }
    }


}

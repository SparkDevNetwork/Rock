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
using Rock.Data;

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

            return Queryable()
                .Where( a =>
                    a.StartDateTime >= beginDate &&
                    a.StartDateTime < endDate &&
                    a.LocationId == locationId &&
                    a.ScheduleId == scheduleId &&
                    a.GroupId == groupId &&
                    a.PersonId == personId )
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

            return Queryable()
                .Where( a =>
                    a.StartDateTime >= beginDate &&
                    a.StartDateTime < endDate &&
                    !a.EndDateTime.HasValue &&
                    a.LocationId == locationId &&
                    a.DidAttend );
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        /// <param name="graphBy">The graph by.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <returns></returns>
        public IEnumerable<IChartData> GetChartData( AttendanceGroupBy groupBy = AttendanceGroupBy.Week, AttendanceGraphBy graphBy = AttendanceGraphBy.Total, DateTime? startDate = null, DateTime? endDate = null, string groupTypeIds = null, string campusIds = null )
        {
            var qry = Queryable().AsNoTracking().Where( a => a.DidAttend );

            if ( startDate.HasValue )
            {
                qry = qry.Where( a => a.StartDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                qry = qry.Where( a => a.StartDateTime < endDate.Value );
            }

            if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
            {
                var groupTypeIdList = groupTypeIds.Split( ',' ).Select( a => a.AsInteger() ).ToList();
                qry = qry.Where( a => a.GroupId.HasValue && groupTypeIdList.Contains( a.Group.GroupTypeId ) );
            }

            if ( !string.IsNullOrWhiteSpace( campusIds ) )
            {
                var campusIdList = campusIds.Split( ',' ).Select( a => a.AsInteger() ).ToList();
                qry = qry.Where( a => a.CampusId.HasValue && campusIdList.Contains( a.CampusId.Value ) );
            }

            var summaryQry = qry.Select( a => new
            {
                //// for Date SQL functions, borrowed some ideas from http://stackoverflow.com/a/1177529/1755417 and http://stackoverflow.com/a/133101/1755417 and http://stackoverflow.com/a/607837/1755417

                // Build a CASE statement to group by week, or month, or year
                SummaryDateTime = (DateTime)(
                    // GroupBy Week with Monday as FirstDayOfWeek ( +1 ) and Sunday as Summary Date ( +6 )
                    groupBy == AttendanceGroupBy.Week ? SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day", "1900-01-01", SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "weekday", a.StartDateTime ) + 1 + 1 + 6, a.StartDateTime ) ),
                        "1900-01-01" ) :

                   // GroupBy Month 
                   groupBy == AttendanceGroupBy.Month ? SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day", "1900-01-01", SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "day", a.StartDateTime ) + 1, a.StartDateTime ) ),
                        "1900-01-01" ) :

                    // GroupBy Year
                    groupBy == AttendanceGroupBy.Year ? SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day", "1900-01-01", SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "dayofyear", a.StartDateTime ) + 1, a.StartDateTime ) ),
                        "1900-01-01" ) :

                    // shouldn't happen
                    null
                ),
                Campus = new
                {
                    Id = a.CampusId,
                    Name = a.Campus.Name
                },
                GroupType = new
                {
                    Id = a.Group.GroupTypeId,
                    Name = a.Group.GroupType.Name
                },
                Schedule = new
                {
                    Id = a.ScheduleId,
                    Name = a.Schedule.Name
                }
            } );

            IList<AttendanceSummaryData> result = null;

            if ( graphBy == AttendanceGraphBy.Total )
            {
                result = summaryQry.GroupBy( a => a.SummaryDateTime ).ToList().Select( a => new AttendanceSummaryData
                {
                    DateTimeStamp = a.Key.ToJavascriptMilliseconds(),
                    DateTime = a.Key,
                    SeriesId = "Total",
                    YValue = a.Count()
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Campus )
            {
                result = summaryQry.GroupBy( a => new { a.SummaryDateTime, a.Campus } ).ToList().Select( a => new AttendanceSummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesId = a.Key.Campus.Name,
                    YValue = a.Count()
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.GroupType )
            {
                result = summaryQry.GroupBy( a => new { a.SummaryDateTime, a.GroupType } ).ToList().Select( a => new AttendanceSummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesId = a.Key.GroupType.Name,
                    YValue = a.Count()
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Schedule )
            {
                result = summaryQry.GroupBy( a => new { a.SummaryDateTime, a.Schedule } ).ToList().Select( a => new AttendanceSummaryData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = a.Key.SummaryDateTime,
                    SeriesId = a.Key.Schedule.Name,
                    YValue = a.Count()
                } ).ToList();
            }

            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AttendanceSummaryData : IChartData
    {
        /// <summary>
        /// Gets the date time stamp.
        /// </summary>
        /// <value>
        /// The date time stamp.
        /// </value>
        public long DateTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets the series identifier.
        /// </summary>
        /// <value>
        /// The series identifier.
        /// </value>
        public string SeriesId { get; set; }
    }
}

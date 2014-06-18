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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Http;
using Rock.Data;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AttendancesController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "AttendancesGetChartData",
                routeTemplate: "api/Attendances/GetChartData",
                defaults: new
                {
                    controller = "Attendances",
                    action = "GetChartData"
                } );
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IChartData> GetChartData( AttendanceGroupBy groupBy = AttendanceGroupBy.Week, AttendanceGraphBy graphBy = AttendanceGraphBy.Total, DateTime? startDate = null, DateTime? endDate = null, string groupTypeIds = null, string campusIds = null )
        {
            var qry = Get().Where( a => a.DidAttend );

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
                SummaryDateTime = (
                    groupBy == AttendanceGroupBy.Week ? SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day", "1900-01-01", SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "weekday", a.StartDateTime ) + 2, a.StartDateTime ) ),
                        "1900-01-01" ) ?? DateTime.MinValue :

                    groupBy == AttendanceGroupBy.Month ? SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day", "1900-01-01", SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "day", a.StartDateTime ) + 1, a.StartDateTime ) ),
                        "1900-01-01" ) ?? DateTime.MinValue :

                    groupBy == AttendanceGroupBy.Year ? SqlFunctions.DateAdd(
                        "day",
                        SqlFunctions.DateDiff( "day", "1900-01-01", SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "dayofyear", a.StartDateTime ) + 1, a.StartDateTime ) ),
                        "1900-01-01" ) ?? DateTime.MinValue : 
                        
                    DateTime.MinValue
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

            IList<AttendanceChartData> result = null;

            if ( graphBy == AttendanceGraphBy.Total )
            {
                result = summaryQry.GroupBy( a => a.SummaryDateTime ).ToList().Select( a => new AttendanceChartData
                {
                    DateTimeStamp = a.Key.ToJavascriptMilliseconds(),
                    SeriesId = "Total",
                    YValue = a.Count()
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Campus )
            {
                result = summaryQry.GroupBy( a => new { a.SummaryDateTime, a.Campus } ).ToList().Select( a => new AttendanceChartData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    SeriesId = a.Key.Campus.Name,
                    YValue = a.Count()
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.GroupType )
            {
                result = summaryQry.GroupBy( a => new { a.SummaryDateTime, a.GroupType } ).ToList().Select( a => new AttendanceChartData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    SeriesId = a.Key.GroupType.Name,
                    YValue = a.Count()
                } ).ToList();
            }
            else if ( graphBy == AttendanceGraphBy.Schedule )
            {
                result = summaryQry.GroupBy( a => new { a.SummaryDateTime, a.Schedule } ).ToList().Select( a => new AttendanceChartData
                {
                    DateTimeStamp = a.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    SeriesId = a.Key.Schedule.Name,
                    YValue = a.Count()
                } ).ToList();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public class AttendanceChartData : IChartData
        {
            /// <summary>
            /// Gets the date time stamp.
            /// </summary>
            /// <value>
            /// The date time stamp.
            /// </value>
            public long DateTimeStamp { get; set; }

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
}

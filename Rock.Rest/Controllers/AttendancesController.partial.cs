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
using System.Net;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

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
        public IEnumerable<IChartData> GetChartData( int? groupBy = null, int? graphBy = null, DateTime? startDate = null, DateTime? endDate = null, string groupTypeIds = null, string campusIds = null )
        {
            /* create Linq for this SQL
select 
    count(*) [count],
    convert(date, StartDateTime - DatePart(weekday, StartDateTime)+2) [WeekStart] 
from 
    Attendance 
group by 
    convert(date, StartDateTime - DatePart(weekday, StartDateTime)+2)
order by 
    convert(date, StartDateTime - DatePart(weekday, StartDateTime)+2) desc
             
             */

            // Determine the WeekStartDate of each attendance records grouping using Monday as the First Day Of Week
            var groupedQry = Get().GroupBy( a => new
            {
                // partially from http://stackoverflow.com/a/1177529/1755417 and http://stackoverflow.com/a/133101/1755417 and http://stackoverflow.com/a/607837/1755417
                WeekStartDate = SqlFunctions.DateAdd( 
                    "day",
                    SqlFunctions.DateDiff( 
                        "day", 
                        "1900-01-01",
                        SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "weekday", a.StartDateTime ) + 2, a.StartDateTime )),
                    "1900-01-01" ),
            } ).OrderBy( a => a.Key.WeekStartDate );

            var list = groupedQry.ToList();

            var result = list.Select( a => new AttendanceChartData
            {
                DateTimeStamp = a.Key.WeekStartDate.Value.ToJavascriptMilliseconds(),
                DateTime = a.Key.WeekStartDate.Value,
                SeriesId = "Total",
                YValue = a.Count()
            } ).ToList();


            //TODO lots of stuff...

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

            public DateTime DateTime { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return this.DateTime.ToString();
            }
        }
    }
}

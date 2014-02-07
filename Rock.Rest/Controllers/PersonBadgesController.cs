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
using System.Net;
using System.Web.Http;
using System.Linq;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PersonBadgesController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "GetFamilyAttendance",
                routeTemplate: "api/PersonBadges/FamilyAttendance/{personId}",
                defaults: new
                {
                    controller = "PersonBadges",
                    action = "GetFamilyAttendance"
                } );
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attenance badge 
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<MonthlyAttendanceSummary> GetFamilyAttendance(int personId)
        {
            List<MonthlyAttendanceSummary> attendanceSummary = new List<MonthlyAttendanceSummary>();
            
            Service service = new Service();

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PersonId", personId);

            var results = service.GetDataSet("spCheckin_BadgeAttendance", System.Data.CommandType.StoredProcedure, parameters);

            if (results.Tables.Count > 0)
            {
                foreach (DataRow row in results.Tables[0].Rows)
                {
                    MonthlyAttendanceSummary item = new MonthlyAttendanceSummary();
                    item.AttendanceCount = Int32.Parse(row["AttendanceCount"].ToString());
                    item.SundaysInMonth = Int32.Parse(row["SundaysInMonth"].ToString());
                    item.Month = Int32.Parse(row["Month"].ToString());
                    item.Year = Int32.Parse(row["Year"].ToString());

                    attendanceSummary.Add(item);
                }
            }

            return attendanceSummary.AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        public class MonthlyAttendanceSummary
        {
            /// <summary>
            /// Gets or sets the number of times the unit attended.
            /// </summary>
            /// <value>
            /// The number of attendances.
            /// </value>
            public int AttendanceCount { get; set; }

            /// <summary>
            /// Gets or sets the number of Sundays in the month
            /// </summary>
            /// <value>
            /// The number of Sundays.
            /// </value>
            public int SundaysInMonth { get; set; }

            /// <summary>
            /// Gets or sets the month.
            /// </summary>
            /// <value>
            /// The month of the attendance.
            /// </value>
            public int Month { get; set; }

            /// <summary>
            /// Gets or sets the year.
            /// </summary>
            /// <value>The year.</value>
            public int Year { get; set; }

        }
    }
}

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
        public IEnumerable<IChartData> GetChartData( AttendanceGroupBy groupBy = AttendanceGroupBy.Week, AttendanceGraphBy graphBy = AttendanceGraphBy.Total, DateTime? startDate = null, DateTime? endDate = null, string groupIds = null, string campusIds = null )
        {
            return new AttendanceService( new RockContext() ).GetChartData( groupBy, graphBy, startDate, endDate, groupIds, campusIds );
        }
    }
}

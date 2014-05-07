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
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Query;
using System.Web.Routing;
using Newtonsoft.Json;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MetricValuesController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "MetricValuesGetChartData",
                routeTemplate: "api/MetricValues/GetChartData/{metricId}",
                defaults: new
                {
                    controller = "MetricValues",
                    action = "GetChartData"
                } );

            routes.MapHttpRoute(
                name: "MetricValuesGetByMetricId",
                routeTemplate: "api/MetricValues/GetByMetricId/{metricId}",
                defaults: new
                {
                    controller = "MetricValues",
                    action = "GetByMetricId"
                } );
        }

        /// <summary>
        /// Gets the by metric identifier.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="metricValueType">Type of the metric value.</param>
        /// <returns></returns>
        //[Authenticate, Secured]
        [Queryable( AllowedQueryOptions = AllowedQueryOptions.All )]
        public IQueryable<MetricValue> GetByMetricId( int metricId, MetricValueType? metricValueType = null )
        {
            var result = base.Get().Where( a => a.MetricId == metricId );
            if ( metricValueType.HasValue )
            {
                result = result.Where( a => a.MetricValueType == metricValueType );
            }

            return result;
        }
    }
}

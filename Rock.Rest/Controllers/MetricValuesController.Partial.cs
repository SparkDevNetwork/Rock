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
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData.Query;
using System.Web.Routing;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

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
                name: "MetricValuesGetByMetricId",
                routeTemplate: "api/MetricValues/GetByMetricId/{metricId}",
                defaults: new
                {
                    controller = "MetricValues",
                    action = "GetByMetricId"
                } );

            routes.MapHttpRoute(
                name: "MetricValuesGetSummaryByMetricID",
                routeTemplate: "api/MetricValues/GetSummary",
                defaults: new
                {
                    controller = "MetricValues",
                    action = "GetSummary"
                } );

            routes.MapHttpRoute(
                name: "MetricValuesGetSeriesName",
                routeTemplate: "api/MetricValues/GetSeriesName/{metricId}/{seriesId}",
                defaults: new
                {
                    controller = "MetricValues",
                    action = "GetSeriesName"
                } );
        }

        /// <summary>
        /// Gets the by metric identifier.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="metricValueType">Type of the metric value.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [Queryable( AllowedQueryOptions = AllowedQueryOptions.All )]
        public IQueryable<MetricValue> GetByMetricId( int metricId, MetricValueType? metricValueType = null )
        {
            var metric = new MetricService( new RockContext() ).Get( metricId );

            var result = Get().Where( a => a.MetricId == metricId );
            if ( metricValueType.HasValue )
            {
                result = result.Where( a => a.MetricValueType == metricValueType );
            }

            return result.OrderBy( a => a.MetricValueDateTime );
        }

        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <param name="metricIdList">The metric identifier list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="metricValueType">Type of the metric value.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public IEnumerable<MetricSummary> GetSummary( string metricIdList, DateTime? startDate = null, DateTime? endDate = null, MetricValueType? metricValueType = null, int? entityTypeId = null, int? entityId = null )
        {
            List<int> metricIds = metricIdList.SplitDelimitedValues().Select(a=> a.AsInteger()).ToList();
            var qry = Get().Where( a => metricIds.Contains( a.MetricId ) );
            if ( metricValueType.HasValue )
            {
                qry = qry.Where( a => a.MetricValueType == metricValueType );
            }

            if ( startDate.HasValue )
            {
                qry = qry.Where( a => a.MetricValueDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                qry = qry.Where( a => a.MetricValueDateTime < endDate.Value );
            }

            // if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metrics.EntityTypeId, filter the values to the specified entityId
            if ( entityTypeId.HasValue )
            {
                if ( entityId.HasValue )
                {
                    qry = qry.Where( a => a.Metric.EntityTypeId == entityTypeId && a.EntityId == entityId );
                }
            }

            var groupBySum = qry
                .GroupBy( a => a.Metric )
                .Select( g => new
                {
                    MetricId = g.Key.Id,
                    MetricTitle = g.Key.Title,
                    YValueTotal = g.Sum( s => s.YValue )
                } ).ToList();

            return groupBySum.Select( s => new MetricSummary
            {
                MetricId = s.MetricId,
                MetricTitle = s.MetricTitle,
                YValueTotal = s.YValueTotal,
                StartDateTimeStamp = startDate.HasValue ? startDate.Value.ToJavascriptMilliseconds() : 0,
                EndDateTimeStamp = endDate.HasValue ? endDate.Value.ToJavascriptMilliseconds() : 0
            } );
        }

        /// <summary>
        /// 
        /// </summary>
        public class MetricSummary
        {
            public int MetricId { get; set; }

            public string MetricTitle { get; set; }

            public decimal? YValueTotal { get; set; }

            public long StartDateTimeStamp { get; set; }

            public long EndDateTimeStamp { get; set; }
        }

        /// <summary>
        /// Gets the name of the series.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="seriesId">The series identifier.</param>
        /// <returns></returns>
        public string GetSeriesName( int metricId, int seriesId )
        {
            var rockContext = new RockContext();
            int? entityTypeId = new MetricService( rockContext ).Queryable().Where( a => a.Id == metricId ).Select( s => s.EntityTypeId ).FirstOrDefault();
            if ( entityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Read( entityTypeId.Value );
                if ( entityTypeCache != null )
                {
                    Type[] modelType = { entityTypeCache.GetEntityType() };
                    Type genericServiceType = typeof( Rock.Data.Service<> );
                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                    var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                    MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                    var result = getMethod.Invoke( serviceInstance, new object[] { seriesId } );
                    if ( result != null )
                    {
                        return result.ToString();
                    }
                }
            }

            return null;
        }
    }
}

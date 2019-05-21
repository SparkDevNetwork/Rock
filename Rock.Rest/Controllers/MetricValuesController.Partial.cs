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
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MetricValuesController
    {
        /// <summary>
        /// Gets the by metric identifier.
        /// NOTE: The Chart blocks use ODATA to further filter this
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="metricValueType">Type of the metric value.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        [System.Web.Http.Route( "api/MetricValues/GetByMetricId/{metricId}" )]
        public IQueryable<MetricValue> GetByMetricId( int metricId, MetricValueType? metricValueType = null )
        {
            // include MetricValuePartitions and each MetricValuePartition's MetricPartition so that MetricValuePartitionEntityIds doesn't have to lazy load
            var result = Get().Include( a => a.MetricValuePartitions.Select( b => b.MetricPartition ) ).Where( a => a.MetricId == metricId );
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
        [System.Web.Http.Route( "api/MetricValues/GetSummary" )]
        public IEnumerable<MetricSummary> GetSummary( string metricIdList, DateTime? startDate = null, DateTime? endDate = null, MetricValueType? metricValueType = null, int? entityTypeId = null, int? entityId = null )
        {
            List<int> metricIds = metricIdList.SplitDelimitedValues().AsIntegerList();
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

            //// if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
            //// Note: if a Metric or it's Metric Value doesn't have a context, include it regardless of Context setting
            if ( entityTypeId.HasValue && entityId.HasValue )
            {
                qry = qry.Where( a => a.MetricValuePartitions.Any( p => p.EntityId == entityId.Value && p.MetricPartition.EntityTypeId == entityTypeId ) );
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
            /// <summary>
            /// Gets or sets the metric identifier.
            /// </summary>
            /// <value>
            /// The metric identifier.
            /// </value>
            public int MetricId { get; set; }

            /// <summary>
            /// Gets or sets the metric title.
            /// </summary>
            /// <value>
            /// The metric title.
            /// </value>
            public string MetricTitle { get; set; }

            /// <summary>
            /// Gets or sets the y value total.
            /// </summary>
            /// <value>
            /// The y value total.
            /// </value>
            public decimal? YValueTotal { get; set; }

            /// <summary>
            /// Gets or sets the start date time stamp.
            /// </summary>
            /// <value>
            /// The start date time stamp.
            /// </value>
            public long StartDateTimeStamp { get; set; }

            /// <summary>
            /// Gets or sets the end date time stamp.
            /// </summary>
            /// <value>
            /// The end date time stamp.
            /// </value>
            public long EndDateTimeStamp { get; set; }
        }

        /// <summary>
        /// Gets the name of the series partition using POST
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="metricValuePartitionEntityIdList">The metric value partition entity identifier list.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/MetricValues/GetSeriesPartitionName/{metricId}" )]
        [HttpPost]
        public string GetSeriesPartitionName( int metricId, [FromBody] List<string> metricValuePartitionEntityIdList )
        {
            var entityTypeEntityIdList = metricValuePartitionEntityIdList.Select( a => a.Split( '|' ) ).Select( a =>
                new
                {
                    EntityTypeId = a[0].AsIntegerOrNull(),
                    EntityId = a[1].AsIntegerOrNull()
                } );

            var rockContext = new RockContext();

            List<string> seriesPartitionValues = new List<string>();

            foreach ( var entityTypeEntity in entityTypeEntityIdList )
            {
                if ( entityTypeEntity.EntityTypeId.HasValue && entityTypeEntity.EntityId.HasValue )
                {
                    var entityTypeCache = EntityTypeCache.Get( entityTypeEntity.EntityTypeId.Value );
                    if ( entityTypeCache != null )
                    {
                        if ( entityTypeCache.Id == EntityTypeCache.GetId<Campus>() )
                        {
                            var campus = CampusCache.Get( entityTypeEntity.EntityId.Value );
                            if ( campus != null )
                            {
                                seriesPartitionValues.Add( campus.Name );
                            }
                        }
                        else if ( entityTypeCache.Id == EntityTypeCache.GetId<DefinedValue>() )
                        {
                            var definedValue = DefinedValueCache.Get( entityTypeEntity.EntityId.Value );
                            if ( definedValue != null )
                            {
                                seriesPartitionValues.Add( definedValue.ToString() );
                            }
                        }
                        else
                        {
                            Type[] modelType = { entityTypeCache.GetEntityType() };
                            Type genericServiceType = typeof( Rock.Data.Service<> );
                            Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                            var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                            MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                            var result = getMethod.Invoke( serviceInstance, new object[] { entityTypeEntity.EntityId } );
                            if ( result != null )
                            {
                                seriesPartitionValues.Add( result.ToString() );
                            }
                        }
                    }
                }
            }

            if ( seriesPartitionValues.Any() )
            {
                return seriesPartitionValues.AsDelimited( "," );
            }
            else
            {
                return null;
            }
        }
    }
}

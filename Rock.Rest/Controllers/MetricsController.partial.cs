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
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MetricsController
    {
        /// <summary>
        /// Gets the HTML for a LiquidDashboardWidget block
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Metrics/GetHtmlForBlock/{blockId}" )]
        public string GetHtmlForBlock( int blockId, int? entityTypeId = null, int? entityId = null )
        {
            RockContext rockContext = this.Service.Context as RockContext ?? new RockContext();
            Block block = new BlockService( rockContext ).Get( blockId );
            if ( block != null )
            {
                block.LoadAttributes();

                string liquidTemplate = block.GetAttributeValue( "LiquidTemplate" );

                var metricCategoryPairList = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( block.GetAttributeValue( "MetricCategories" ) );

                var metricGuids = metricCategoryPairList.Select( a => a.MetricGuid ).ToList();

                bool roundYValues = block.GetAttributeValue( "RoundValues" ).AsBooleanOrNull() ?? true;

                MetricService metricService = new MetricService( rockContext );
                var metrics = metricService.GetByGuids( metricGuids );
                List<object> metricsData = new List<object>();

                if ( metrics.Count() == 0 )
                {
                    return @"<div class='alert alert-warning'> 
								Please select a metric in the block settings.
							</div>";
                }

                MetricValueService metricValueService = new MetricValueService( rockContext );

                DateTime firstDayOfYear = new DateTime( RockDateTime.Now.Year, 1, 1 );
                DateTime currentDateTime = RockDateTime.Now;
                DateTime firstDayOfNextYear = new DateTime( RockDateTime.Now.Year + 1, 1, 1 );

                foreach ( var metric in metrics )
                {
                    var metricYTDData = JsonConvert.DeserializeObject( metric.ToJson(), typeof( MetricYTDData ) ) as MetricYTDData;
                    var qryMeasureValues = metricValueService.Queryable()
                        .Where( a => a.MetricId == metricYTDData.Id )
                        .Where( a => a.MetricValueDateTime >= firstDayOfYear && a.MetricValueDateTime < currentDateTime )
                        .Where( a => a.MetricValueType == MetricValueType.Measure );

                    //// if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metrics.EntityTypeId, filter the values to the specified entityId
                    //// Note: if a Metric or it's Metric Value doesn't have a context, include it regardless of Context setting
                    if ( entityTypeId.HasValue && ( metric.EntityTypeId == entityTypeId || metric.EntityTypeId == null ) )
                    {
                        if ( entityId.HasValue )
                        {
                            qryMeasureValues = qryMeasureValues.Where( a => a.EntityId == entityId || a.EntityId == null );
                        }
                    }

                    var lastMetricValue = qryMeasureValues.OrderByDescending( a => a.MetricValueDateTime ).FirstOrDefault();
                    if ( lastMetricValue != null )
                    {
                        metricYTDData.LastValue = lastMetricValue.YValue.HasValue ? Math.Round( lastMetricValue.YValue.Value, roundYValues ? 0 : 2 ) : (decimal?)null;
                        metricYTDData.LastValueDate = lastMetricValue.MetricValueDateTime.HasValue ? lastMetricValue.MetricValueDateTime.Value.Date : DateTime.MinValue;
                    }

                    decimal? sum = qryMeasureValues.Sum( a => a.YValue );
                    metricYTDData.CumulativeValue = sum.HasValue ? Math.Round( sum.Value, roundYValues ? 0 : 2 ) : (decimal?)null;

                    // figure out goal as of current date time by figuring out the slope of the goal
                    var qryGoalValuesCurrentYear = metricValueService.Queryable()
                        .Where( a => a.MetricId == metricYTDData.Id )
                        .Where( a => a.MetricValueDateTime >= firstDayOfYear && a.MetricValueDateTime < firstDayOfNextYear )
                        .Where( a => a.MetricValueType == MetricValueType.Goal );

                    // if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metrics.EntityTypeId, filter the values to the specified entityId
                    if ( entityTypeId.HasValue && metric.EntityTypeId == entityTypeId )
                    {
                        if ( entityId.HasValue )
                        {
                            qryGoalValuesCurrentYear = qryGoalValuesCurrentYear.Where( a => a.EntityId == entityId );
                        }
                    }

                    MetricValue goalLineStartPoint = qryGoalValuesCurrentYear.Where( a => a.MetricValueDateTime <= currentDateTime ).OrderByDescending( a => a.MetricValueDateTime ).FirstOrDefault();
                    MetricValue goalLineEndPoint = qryGoalValuesCurrentYear.Where( a => a.MetricValueDateTime >= currentDateTime ).FirstOrDefault();
                    if ( goalLineStartPoint != null && goalLineEndPoint != null )
                    {
                        var changeInX = goalLineEndPoint.DateTimeStamp - goalLineStartPoint.DateTimeStamp;
                        var changeInY = goalLineEndPoint.YValue - goalLineStartPoint.YValue;
                        if ( changeInX != 0 )
                        {
                            decimal? slope = changeInY / changeInX;
                            decimal goalValue = ( ( slope * ( currentDateTime.ToJavascriptMilliseconds() - goalLineStartPoint.DateTimeStamp ) ) + goalLineStartPoint.YValue ).Value;
                            metricYTDData.GoalValue = Math.Round( goalValue, roundYValues ? 0 : 2 );
                        }
                    }
                    else
                    {
                        // if there isn't a both a start goal and end goal within the date range, there wouldn't be a goal line shown in a line chart, so don't display a goal in liquid either
                        metricYTDData.GoalValue = null;
                    }

                    metricsData.Add( metricYTDData.ToLiquid() );
                }

                Dictionary<string, object> mergeValues = new Dictionary<string, object>();
                mergeValues.Add( "Metrics", metricsData );

                string resultHtml = liquidTemplate.ResolveMergeFields( mergeValues );

                // show liquid help for debug
                if ( block.GetAttributeValue( "EnableDebug" ).AsBoolean() )
                {
                    resultHtml += mergeValues.lavaDebugInfo();
                }

                return resultHtml;
            }

            return string.Format(
                @"<div class='alert alert-danger'> 
                    unable to find block_id: {0}
                </div>",
                blockId );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MetricYTDData : Metric
    {
        /// <summary>
        /// Gets or sets the last value.
        /// </summary>
        /// <value>
        /// The last value.
        /// </value>
        [DataMember]
        public object LastValue { get; set; }

        /// <summary>
        /// Gets or sets the last value date.
        /// </summary>
        /// <value>
        /// The last value date.
        /// </value>
        [DataMember]
        public DateTime LastValueDate { get; set; }

        /// <summary>
        /// Gets or sets the cumulative value.
        /// </summary>
        /// <value>
        /// The cumulative value.
        /// </value>
        [DataMember]
        public object CumulativeValue { get; set; }

        /// <summary>
        /// Gets or sets the goal value.
        /// </summary>
        /// <value>
        /// The goal value.
        /// </value>
        [DataMember]
        public object GoalValue { get; set; }
    }
}

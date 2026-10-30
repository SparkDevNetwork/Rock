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
using HttpStatusCode = System.Net.HttpStatusCode;
using System.Web.Http;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class MetricsController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( Metric entity )
        {
            base.CheckCanEdit( entity );

            /*
                10/25/2026 - MSE

                Staff inherit EDIT on v1 REST controllers, and metric EDIT is also used for
                value entry. New SQL or Lava metrics can run code as the application, so their
                creation needs controller ADMINISTRATE, like block ADMINISTRATE in Metric Detail.
                Check dormant source text too, so it cannot be planted through a different type.

                Reason: Value editors must not author metric source code through v1 REST.
            */
            if ( entity != null && entity.Id == 0 )
            {
                var sourceTypeGuid = DefinedValueCache.Get( entity.SourceValueTypeId ?? 0 )?.Guid;
                bool hasExecutableSource = sourceTypeGuid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid()
                    || sourceTypeGuid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_LAVA.AsGuid()
                    || !string.IsNullOrEmpty( entity.SourceSql )
                    || !string.IsNullOrEmpty( entity.SourceLava );

                if ( hasExecutableSource )
                {
                    CheckCanAdministrateController();
                }
            }
        }

        /// <inheritdoc/>
        protected override void CheckCanEditAfterUpdate( Metric entity )
        {
            base.CheckCanEditAfterUpdate( entity );

            /*
                10/25/2026 - MSE

                Only source changes need ADMINISTRATE. Full updates that echo unchanged source
                and ordinary metadata updates must still work for metric editors. Authorize
                against the stored metric so posted values cannot supply their own permission.

                Reason: Source authoring must have the same authority through every v1 write.
            */
            using ( var rockContext = new RockContext() )
            {
                var storedMetric = new MetricService( rockContext ).Get( entity.Id );
                if ( storedMetric == null )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }

                bool sourceChanged = entity.SourceValueTypeId != storedMetric.SourceValueTypeId
                    || !string.Equals( entity.SourceSql ?? string.Empty, storedMetric.SourceSql ?? string.Empty, StringComparison.Ordinal )
                    || !string.Equals( entity.SourceLava ?? string.Empty, storedMetric.SourceLava ?? string.Empty, StringComparison.Ordinal );

                if ( sourceChanged && !storedMetric.IsAuthorized( Authorization.ADMINISTRATE, GetPerson() ) )
                {
                    CheckCanAdministrateController();
                }
            }
        }

        /// <summary>
        /// Gets the HTML for a LiquidDashboardWidget block
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        [Authenticate]
        [System.Web.Http.Route( "api/Metrics/GetHtmlForBlock/{blockId}" )]
        [Rock.SystemGuid.RestActionGuid( "B67ED75B-94AB-49B7-9E36-84B845FB20D3" )]
        public string GetHtmlForBlock( int blockId, int? entityTypeId = null, int? entityId = null )
        {
            if ( !CanViewDashboardWidgetBlock( BlockCache.Get( blockId ), GetPerson() ) )
            {
                return GetBlockNotFoundHtml( blockId );
            }

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
                var metrics = metricService.GetByGuids( metricGuids ).Include( a => a.MetricPartitions );
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

                    //// if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
                    //// Note: if a Metric or it's Metric Value doesn't have a context, include it regardless of Context setting
                    if ( entityTypeId.HasValue && entityId.HasValue )
                    {
                        if ( metric.MetricPartitions.Any( a => a.EntityTypeId == entityTypeId.Value ) )
                        {
                            qryMeasureValues = qryMeasureValues.Where( a => a.MetricValuePartitions.Any( p => p.EntityId == entityId.Value && p.MetricPartition.EntityTypeId == entityTypeId ) );
                        }
                    }

                    var lastMetricValue = qryMeasureValues.OrderByDescending( a => a.MetricValueDateTime ).FirstOrDefault();
                    if ( lastMetricValue != null )
                    {
                        metricYTDData.LastValueDate = lastMetricValue.MetricValueDateTime.HasValue ? lastMetricValue.MetricValueDateTime.Value.Date : DateTime.MinValue;

                        // get a sum of the values that for whole 24 hour day of the last Date
                        DateTime lastValueDateEnd = metricYTDData.LastValueDate.AddDays( 1 );
                        var lastMetricCumulativeValues = qryMeasureValues.Where( a => a.MetricValueDateTime.HasValue && a.MetricValueDateTime.Value >= metricYTDData.LastValueDate && a.MetricValueDateTime.Value < lastValueDateEnd );
                        metricYTDData.LastValue = lastMetricCumulativeValues.Sum( a => a.YValue ).HasValue ? Math.Round( lastMetricCumulativeValues.Sum( a => a.YValue ).Value, roundYValues ? 0 : 2 ) : ( decimal? ) null;
                    }

                    var previousMetricValue = qryMeasureValues.OrderByDescending( a => a.MetricValueDateTime ).Skip( 1 ).FirstOrDefault();
                    if ( previousMetricValue != null )
                    {
                        metricYTDData.PreviousValueDate = previousMetricValue.MetricValueDateTime.HasValue ? previousMetricValue.MetricValueDateTime.Value.Date : DateTime.MinValue;

                        // get a sum of the values that for whole 24 hour day of the previous Date
                        DateTime previousValueDateEnd = metricYTDData.PreviousValueDate.AddDays( 1 );
                        var previousMetricCumulativeValues = qryMeasureValues.Where( a => a.MetricValueDateTime.HasValue && a.MetricValueDateTime.Value >= metricYTDData.PreviousValueDate && a.MetricValueDateTime.Value < previousValueDateEnd );
                        metricYTDData.PreviousValue = previousMetricCumulativeValues.Sum( a => a.YValue ).HasValue ? Math.Round( previousMetricCumulativeValues.Sum( a => a.YValue ).Value, roundYValues ? 0 : 2 ) : ( decimal? ) null;
                    }

                    decimal? sum = qryMeasureValues.Sum( a => a.YValue );
                    metricYTDData.CumulativeValue = sum.HasValue ? Math.Round( sum.Value, roundYValues ? 0 : 2 ) : ( decimal? ) null;

                    // figure out goal as of current date time by figuring out the slope of the goal
                    var qryGoalValuesCurrentYear = metricValueService.Queryable()
                        .Where( a => a.MetricId == metricYTDData.Id )
                        .Where( a => a.MetricValueDateTime >= firstDayOfYear && a.MetricValueDateTime < firstDayOfNextYear )
                        .Where( a => a.MetricValueType == MetricValueType.Goal );

                    // if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
                    if ( entityTypeId.HasValue && entityId.HasValue )
                    {
                        if ( metric.MetricPartitions.Any( a => a.EntityTypeId == entityTypeId.Value ) )
                        {
                            qryGoalValuesCurrentYear = qryGoalValuesCurrentYear.Where( a => a.MetricValuePartitions.Any( p => p.EntityId == entityId.Value && p.MetricPartition.EntityTypeId == entityTypeId ) );
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

                    metricsData.Add( metricYTDData );
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

            return GetBlockNotFoundHtml( blockId );
        }

        /// <summary>
        /// Determines whether the person can view the block as a Lava Dashboard Widget. Matches
        /// the page rendering rules: block VIEW, EDIT or ADMINISTRATE plus page VIEW.
        /// </summary>
        /// <param name="blockCache">The block, or <c>null</c> if it was not found.</param>
        /// <param name="person">The current person, or <c>null</c> if anonymous.</param>
        /// <returns><c>true</c> if the block is a Lava Dashboard Widget the person can view; otherwise <c>false</c>.</returns>
        private static bool CanViewDashboardWidgetBlock( BlockCache blockCache, Person person )
        {
            if ( blockCache == null || blockCache.BlockType?.Guid != Rock.SystemGuid.BlockType.REPORTING_LAVA_DASHBOARD_WIDGET.AsGuid() )
            {
                return false;
            }

            var canViewBlock = blockCache.IsAuthorized( Authorization.VIEW, person )
                || blockCache.IsAuthorized( Authorization.EDIT, person )
                || blockCache.IsAuthorized( Authorization.ADMINISTRATE, person );

            if ( !canViewBlock )
            {
                return false;
            }

            if ( !blockCache.PageId.HasValue )
            {
                return true;
            }

            return PageCache.Get( blockCache.PageId.Value )?.IsAuthorized( Authorization.VIEW, person ) == true;
        }

        /// <summary>
        /// Gets the alert HTML returned when a block cannot be found or cannot be viewed.
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <returns>The alert HTML.</returns>
        private static string GetBlockNotFoundHtml( int blockId )
        {
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
    [RockDomain( "Reporting" )]
    [Rock.SystemGuid.EntityTypeGuid( "7C878AFD-DF74-4CDF-8491-31671559BD30" )]
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
        /// Gets or sets the previous value.
        /// </summary>
        /// <value>
        /// The last value.
        /// </value>
        [DataMember]
        public object PreviousValue { get; set; }

        /// <summary>
        /// Gets or sets the last value date.
        /// </summary>
        /// <value>
        /// The last value date.
        /// </value>
        [DataMember]
        public DateTime PreviousValueDate { get; set; }

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
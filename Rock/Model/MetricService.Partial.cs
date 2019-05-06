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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MetricService
    {
        /// <summary>
        /// Ensures that each Metric that has EnableAnalytics has a SQL View for it, and also deletes any AnalyticsFactMetric** views that no longer have metric (based on Metric.Name)
        /// </summary>
        public void EnsureMetricAnalyticsViews()
        {
            string analyticMetricViewsPrefix = "AnalyticsFactMetric";
            string getAnalyticMetricViewsSQL = string.Format(
@"SELECT 
    OBJECT_NAME(sm.object_id) [view_name]
    ,sm.DEFINITION [view_definition]
FROM sys.sql_modules AS sm
JOIN sys.objects AS o ON sm.object_id = o.object_id
WHERE o.type = 'V'
    AND OBJECT_NAME(sm.object_id) LIKE '{0}%'",
        analyticMetricViewsPrefix );

            var dataTable = DbService.GetDataTable( getAnalyticMetricViewsSQL, System.Data.CommandType.Text, null );
            var databaseAnalyticMetricViews = dataTable.Rows.OfType<DataRow>()
                .Select( row => new
                {
                    ViewName = row["view_name"] as string,
                    ViewDefinition = row["view_definition"] as string
                } ).ToList();

            var metricsWithAnalyticsEnabled = this.Queryable().Where( a => a.EnableAnalytics ).Include( a => a.MetricPartitions ).AsNoTracking().ToList();

            var metricViewNames = metricsWithAnalyticsEnabled.Select( a => $"{analyticMetricViewsPrefix}{a.Title.RemoveSpecialCharacters()}" ).ToList();
            var orphanedDatabaseViews = databaseAnalyticMetricViews.Where( a => !metricViewNames.Contains( a.ViewName ) ).ToList();

            // DROP any Metric Analytic Views that are orphaned.  In other words, there are views named 'AnalyticsFactMetric***' that don't have a metric. 
            // This could happen if Metric.EnableAnalytics changed from True to False, Metric Title changed, or if a Metric was deleted.
            foreach ( var orphanedView in orphanedDatabaseViews )
            {
                this.Context.Database.ExecuteSqlCommand( $"DROP VIEW [{orphanedView.ViewName}]" );
            }

            // Make sure that each Metric with EnableAnalytics=True has a SQL View and that the View Definition is correct
            foreach ( var metric in metricsWithAnalyticsEnabled )
            {
                string metricViewName = $"{analyticMetricViewsPrefix}{metric.Title.RemoveSpecialCharacters()}";
                var metricEntityPartitions = metric.MetricPartitions.Where(a => a.EntityTypeId.HasValue).OrderBy( a => a.Order ).ThenBy( a => a.Label ).Select( a => new
                {
                    a.Label,
                    a.EntityTypeId
                } );

                var viewPartitionSELECTClauses = metricEntityPartitions.Select( a => $"      ,pvt.[{a.EntityTypeId}] as [{a.Label}Id]" ).ToList().AsDelimited( "\n" );
                List<string> partitionEntityLookupSELECTs = new List<string>();
                List<string> partitionEntityLookupJOINs = new List<string>();
                foreach ( var metricPartition in metricEntityPartitions )
                {
                    var metricPartitionEntityType = EntityTypeCache.Get( metricPartition.EntityTypeId.Value );
                    if ( metricPartitionEntityType != null )
                    {
                        var tableAttribute = metricPartitionEntityType.GetEntityType().GetCustomAttribute<TableAttribute>();
                        if ( tableAttribute != null )
                        {
                            if ( metricPartitionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                            {
                                partitionEntityLookupSELECTs.Add( $"j{metricPartition.EntityTypeId}.Value [{metricPartition.Label.RemoveSpecialCharacters()}Name]" );
                            }
                            else if ( metricPartitionEntityType.GetEntityType().GetProperty( "Name" ) != null )
                            {
                                partitionEntityLookupSELECTs.Add( $"j{metricPartition.EntityTypeId}.Name [{metricPartition.Label.RemoveSpecialCharacters()}Name]" );
                            }

                            partitionEntityLookupJOINs.Add( $"LEFT JOIN [{tableAttribute.Name}] j{metricPartition.EntityTypeId} ON p.{metricPartition.Label.RemoveSpecialCharacters()}Id = j{metricPartition.EntityTypeId}.Id" );
                        }
                    }
                }

                var viewPIVOTInClauses = metricEntityPartitions.Select( a => $"  [{a.EntityTypeId}]" ).ToList().AsDelimited( ",\n" );
                if ( string.IsNullOrEmpty(viewPIVOTInClauses) )
                {
                    // This metric only has the default partition, and with no EntityTypeId, so put in a dummy Pivot Clause 
                    viewPIVOTInClauses = "[0]";
                }

                var viewJoinsSELECT = partitionEntityLookupSELECTs.Select( a => $"  ,{a}" ).ToList().AsDelimited( "\n" );
                var viewJoinsFROM = partitionEntityLookupJOINs.AsDelimited( "\n" );

                var viewDefinition = $@"
/*
<auto-generated>
    This view was generated by the Rock's MetricService.EnsureMetricAnalyticsViews() which gets called when a Metric is saved.
    Changes to this view definition will be lost when the view is regenerated.
    NOTE: Any Views with the prefix '{analyticMetricViewsPrefix}' are assumed to be Code Generated and may be deleted if there isn't a Metric associated with it
</auto-generated>
<doc>
	<summary>
 		This VIEW helps present the data for the {metric.Title} metric. 
	</summary>
</doc>
*/
CREATE VIEW [{metricViewName}] AS
SELECT p.*
{viewJoinsSELECT} 
FROM (
    SELECT pvt.Id
      ,cast(pvt.MetricValueDateTime AS DATE) AS [MetricValueDateTime]
      ,pvt.YValue
{viewPartitionSELECTClauses}
    FROM (
        SELECT 
	      mv.Id
          ,mv.YValue
          ,mv.MetricValueDateTime
          ,mvp.EntityId
          ,mp.EntityTypeId
        FROM MetricValue mv
        JOIN MetricValuePartition mvp ON mvp.MetricValueId = mv.Id
        JOIN MetricPartition mp ON mvp.MetricPartitionId = mp.Id
        WHERE mv.MetricId = {metric.Id}
        ) src
    pivot(min(EntityId) FOR EntityTypeId IN ({viewPIVOTInClauses})) pvt
) p
{viewJoinsFROM}
";

                var databaseViewDefinition = databaseAnalyticMetricViews.Where( a => a.ViewName == metricViewName ).FirstOrDefault();
                try
                {
                    if ( databaseViewDefinition != null )
                    {
                        if ( databaseViewDefinition.ViewDefinition != viewDefinition )
                        {
                            // view already exists, but something has changed, so drop and recreate it
                            this.Context.Database.ExecuteSqlCommand( $"DROP VIEW [{metricViewName}]" );
                            this.Context.Database.ExecuteSqlCommand( viewDefinition );
                        }
                    }
                    else
                    {
                        this.Context.Database.ExecuteSqlCommand( viewDefinition );
                    }
                }
                catch ( Exception ex )
                {
                    // silently log the exception
                    ExceptionLogService.LogException( new Exception("Error creating Analytics view for " + metric.Title, ex), System.Web.HttpContext.Current );
                }
            }
        }
    }
}

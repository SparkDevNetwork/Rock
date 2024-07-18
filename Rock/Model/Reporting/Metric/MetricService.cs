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
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Rock.Chart;
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

            var metricViewNames = metricsWithAnalyticsEnabled.Select( a => $"{analyticMetricViewsPrefix}{a.Title.RemoveSpecialCharacters().RemoveSpaces() }" ).ToList();
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
                string metricViewName = $"{analyticMetricViewsPrefix}{metric.Title.RemoveSpecialCharacters().RemoveSpaces() }";
                var metricEntityPartitions = metric.MetricPartitions.Where( a => a.EntityTypeId.HasValue ).OrderBy( a => a.Order ).ThenBy( a => a.Label ).Select( a => new
                {
                    a.Label,
                    PartitionEntityTypeId = a.EntityTypeId.Value,
                    PartitionId = a.Id
                } );

                var viewPartitionSELECTClauses = metricEntityPartitions.Select( a => $"      ,pvt.[{a.PartitionId}] as [{a.Label.RemoveSpecialCharacters().RemoveSpaces()}Id]" ).ToList().AsDelimited( "\n" );
                List<string> partitionEntityLookupSELECTs = new List<string>();
                List<string> partitionEntityLookupJOINs = new List<string>();
                foreach ( var metricPartition in metricEntityPartitions )
                {
                    var metricPartitionEntityType = EntityTypeCache.Get( metricPartition.PartitionEntityTypeId );
                    if ( metricPartitionEntityType != null )
                    {
                        var tableAttribute = metricPartitionEntityType.GetEntityType().GetCustomAttribute<TableAttribute>();
                        if ( tableAttribute != null )
                        {
                            if ( metricPartitionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                            {
                                partitionEntityLookupSELECTs.Add( $"j{metricPartition.PartitionId}.Value [{metricPartition.Label.RemoveSpecialCharacters().RemoveSpaces()}Name]" );
                            }
                            else if ( metricPartitionEntityType.GetEntityType().GetProperty( "Name" ) != null )
                            {
                                partitionEntityLookupSELECTs.Add( $"j{metricPartition.PartitionId}.Name [{metricPartition.Label.RemoveSpecialCharacters().RemoveSpaces()}Name]" );
                            }

                            partitionEntityLookupJOINs.Add( $"LEFT JOIN [{tableAttribute.Name}] j{metricPartition.PartitionId} ON p.{metricPartition.Label.RemoveSpecialCharacters().RemoveSpaces()}Id = j{metricPartition.PartitionId}.Id" );
                        }
                    }
                }

                var viewPIVOTInClauses = metricEntityPartitions.Select( a => $"  [{a.PartitionId}]" ).ToList().AsDelimited( ",\n" );
                if ( string.IsNullOrEmpty( viewPIVOTInClauses ) )
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
      ,pvt.IsGoal
{viewPartitionSELECTClauses}
    FROM (
        SELECT 
	      mv.Id
          ,mv.YValue
          ,mv.MetricValueDateTime
          ,case when MetricValueType = 1 then 1 else 0 end as IsGoal
          ,mvp.EntityId
          ,mp.Id [PartitionId]
        FROM MetricValue mv
        JOIN MetricValuePartition mvp ON mvp.MetricValueId = mv.Id
        JOIN MetricPartition mp ON mvp.MetricPartitionId = mp.Id
        WHERE mv.MetricId = {metric.Id}
        ) src
    pivot(min(EntityId) FOR PartitionId IN ({viewPIVOTInClauses})) pvt
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
                    Debug.WriteLine( ex.Message );
                    ExceptionLogService.LogException( new Exception( "Error creating Analytics view for " + metric.Title, ex ), System.Web.HttpContext.Current );
                }
            }
        }

        /// <summary>
        /// Calculates the metric values for the given scheduled or manually run metric.
        /// </summary>
        /// <param name="metricId">The integer value for the chosen metric, as long as it has a schedule or was manually run.</param>
        /// <param name="commandTimeout">The SQL command timeout value (in seconds).</param>
        /// <param name="isManualRun">if set to <c>true</c>, method treats current date time as scheduled date time.</param>
        /// <returns></returns>
        public MetricResult CalculateMetric( int metricId, int commandTimeout, bool isManualRun )
        {
            Metric metric = null;
            MetricResult metricResult = new MetricResult();
            var metricSourceValueTypeDataviewGuid = SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid();
            var metricSourceValueTypeSqlGuid = SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid();
            var metricSourceValueTypeLavaGuid = SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_LAVA.AsGuid();
            try
            {
                using ( var rockContextForMetricEntity = new RockContext() )
                {
                    rockContextForMetricEntity.Database.CommandTimeout = commandTimeout;

                    var metricService = new MetricService( rockContextForMetricEntity );
                    metric = metricService.Get( metricId );

                    /*  Get the last time the metric has run, if it has never run then set it as the first day of the current week to give it a chance to run now.
                        NOTE: This is being used instead of Min Date to prevent a schedule with a start date of months or years back from running for each period since then. 
                        This would be the case for the "out-of-the-box" Rock daily metrics "Active Records", "Active Families", and "Active Connection Requests" if they
                        have never run before. Or if a new user created metric uses a schedule that has an older start date.
                    */
                    DateTime currentDateTime = RockDateTime.Now;
                    DateTime? startOfWeek = currentDateTime.StartOfWeek( RockDateTime.FirstDayOfWeek );
                    DateTime? lastRunDateTime = metric.LastRunDateTime ?? startOfWeek;

                    // Get all the scheduled times that the Metric was scheduled to run since that last time it was run.
                    var scheduledDateTimesToProcess = metric.Schedule?.GetScheduledStartTimes( lastRunDateTime.Value, currentDateTime ).Where( a => a > lastRunDateTime.Value ).ToList() ?? new List<DateTime>();

                    if ( isManualRun )
                    {
                        // If this is a manual run, there should not be any scheduled date times to process.
                        scheduledDateTimesToProcess.Clear();

                        // Manually add the current date time to the list.
                        scheduledDateTimesToProcess.Add( currentDateTime );
                    }

                    foreach ( var scheduleDateTime in scheduledDateTimesToProcess )
                    {
                        using ( var rockContextForMetricValues = new RockContext() )
                        {
                            rockContextForMetricValues.Database.CommandTimeout = commandTimeout;
                            var metricPartitions = new MetricPartitionService( rockContextForMetricValues ).Queryable().Where( a => a.MetricId == metric.Id ).ToList();
                            var metricValueService = new MetricValueService( rockContextForMetricValues );
                            var metricValuePartitionService = new MetricValuePartitionService( rockContextForMetricValues );
                            List<ResultValue> resultValues = new List<ResultValue>();
                            bool getMetricValueDateTimeFromResultSet = false;
                            if ( metric.SourceValueType.Guid == metricSourceValueTypeDataviewGuid )
                            {
                                // get the metric value from the DataView
                                if ( metric.DataView != null )
                                {
                                    bool parseCampusPartition = metricPartitions.Count == 1
                                        && metric.AutoPartitionOnPrimaryCampus
                                        && metric.DataView.EntityTypeId == Web.Cache.EntityTypeCache.GetId( SystemGuid.EntityType.PERSON )
                                        && metricPartitions[0].EntityTypeId == Web.Cache.EntityTypeCache.GetId( SystemGuid.EntityType.CAMPUS );

                                    // Dataview metrics can be partitioned by campus only and AutoPartitionOnPrimaryCampus must be selected.
                                    if ( metricPartitions.Count > 1
                                        || ( metricPartitions[0].EntityTypeId.HasValue && parseCampusPartition == false ) )
                                    {
                                        throw new NotImplementedException( "Partitioned Metrics using DataViews is only supported for Person data views using a single partition of type 'Campus' with 'Auto Partition on Primary Campus' checked. Any other dataview partition configuration is not supported." );
                                    }

                                    Stopwatch stopwatch = Stopwatch.StartNew();
                                    var qry = metric.DataView.GetQuery();

                                    if ( parseCampusPartition )
                                    {
                                        // Create a dictionary of campus IDs with a person count
                                        var campusPartitionValues = new Dictionary<int, int>();
                                        foreach ( var person in qry.ToList() )
                                        {
                                            var iPerson = ( Person ) person;
                                            var campusId = iPerson.GetCampus()?.Id ?? -1;
                                            campusPartitionValues.TryGetValue( campusId, out var currentCount );
                                            campusPartitionValues[campusId] = currentCount + 1;
                                        }

                                        // Use the dictionary to create the ResultValues for each campus (partition)
                                        foreach ( var campusPartitionValue in campusPartitionValues )
                                        {
                                            var resultValue = new ResultValue
                                            {
                                                MetricValueDateTime = scheduleDateTime,
                                                Partitions = new List<ResultValuePartition>(),
                                                Value = campusPartitionValue.Value
                                            };

                                            int? entityId = campusPartitionValue.Key;
                                            if ( entityId == -1 )
                                            {
                                                entityId = null;
                                            }

                                            resultValue.Partitions.Add( new ResultValuePartition
                                            {
                                                PartitionPosition = 0,
                                                EntityId = entityId
                                            } );

                                            resultValues.Add( resultValue );
                                        }
                                    }
                                    else
                                    {
                                        // Put the entire set in one result since there is no partition
                                        resultValues.Add( new ResultValue
                                        {
                                            Value = Convert.ToDecimal( qry.Count() ),
                                            MetricValueDateTime = scheduleDateTime,
                                            Partitions = new List<ResultValuePartition>()
                                        } );
                                    }

                                    stopwatch.Stop();
                                    DataViewService.AddRunDataViewTransaction( metric.DataView.Id, Convert.ToInt32( stopwatch.Elapsed.TotalMilliseconds ) );
                                }
                            }
                            else if ( metric.SourceValueType.Guid == metricSourceValueTypeSqlGuid )
                            {
                                //// calculate the metricValue using the results from the SQL
                                //// assume SQL is in one of the following forms:
                                //// -- "SELECT Count(*) FROM ..."
                                //// -- "SELECT Count(*), [MetricValueDateTime] FROM ..."
                                //// -- "SELECT Count(*), Partition0EntityId, Partition1EntityId, Partition2EntityId,.. FROM ..."
                                //// -- "SELECT Count(*), [MetricValueDateTime], Partition0EntityId, Partition1EntityId, Partition2EntityId,.. FROM ..."
                                if ( !string.IsNullOrWhiteSpace( metric.SourceSql ) )
                                {
                                    string formattedSql = metric.SourceSql.ResolveMergeFields( metric.GetMergeObjects( scheduleDateTime ) );
                                    var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null, commandTimeout );

                                    if ( tableResult.Columns.Count >= 2 && tableResult.Columns[1].ColumnName == "MetricValueDateTime" )
                                    {
                                        getMetricValueDateTimeFromResultSet = true;
                                    }

                                    foreach ( var row in tableResult.Rows.OfType<System.Data.DataRow>() )
                                    {
                                        var resultValue = new ResultValue();

                                        resultValue.Value = Convert.ToDecimal( row[0] == DBNull.Value ? 0 : row[0] );
                                        if ( getMetricValueDateTimeFromResultSet )
                                        {
                                            resultValue.MetricValueDateTime = Convert.ToDateTime( row[1] );
                                        }
                                        else
                                        {
                                            resultValue.MetricValueDateTime = scheduleDateTime;
                                        }

                                        resultValue.Partitions = new List<ResultValuePartition>();
                                        int partitionPosition = 0;
                                        int partitionFieldIndex = getMetricValueDateTimeFromResultSet ? 2 : 1;
                                        int partitionColumnCount = tableResult.Columns.Count - 1;
                                        while ( partitionFieldIndex <= partitionColumnCount )
                                        {
                                            resultValue.Partitions.Add( new ResultValuePartition
                                            {
                                                PartitionPosition = partitionPosition,
                                                EntityId = row[partitionFieldIndex] as int?
                                            } );

                                            partitionPosition++;
                                            partitionFieldIndex++;
                                        }

                                        resultValues.Add( resultValue );
                                    }
                                }
                            }
                            else if ( metric.SourceValueType.Guid == metricSourceValueTypeLavaGuid )
                            {
                                //// calculate the metricValue using the results from Lava
                                //// assume Lava Output is in one of the following forms:
                                //// A single Count
                                //// 42
                                //// A List of Count, MetricValueDateTime
                                //// 42, 1/1/2017
                                //// 40, 1/2/2017
                                //// 49, 1/3/2017
                                //// A List of Count, Partition0EntityId, Partition1EntityId, Partition2EntityId
                                //// 42, 201, 450, 654
                                //// 42, 202, 450, 654
                                //// 42, 203, 450, 654
                                //// A List of Count, MetricValueDateTime,  Partition0EntityId, Partition1EntityId, Partition2EntityId
                                //// 42, 1/1/2017, 201, 450, 654
                                //// 42, 1/2/2017, 202, 450, 654
                                //// 42, 1/3/2017, 203, 450, 654
                                if ( !string.IsNullOrWhiteSpace( metric.SourceLava ) )
                                {
                                    var mergeObjects = metric.GetMergeObjects( scheduleDateTime );
                                    string lavaResult = metric.SourceLava.ResolveMergeFields( mergeObjects, enabledLavaCommands: "All", throwExceptionOnErrors: true );
                                    List<string> resultLines = lavaResult.Split( new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries )
                                        .Select( a => a.Trim() ).Where( a => !string.IsNullOrEmpty( a ) ).ToList();
                                    List<string[]> resultList = resultLines.Select( a => a.SplitDelimitedValues() ).ToList();

                                    if ( resultList.Any() )
                                    {
                                        if ( resultList[0].Length >= 2 )
                                        {
                                            // if the value of the data in the 2nd column is a Date, assume that is is the MetricValueDateTime
                                            if ( resultList[0][1].AsDateTime().HasValue )
                                            {
                                                getMetricValueDateTimeFromResultSet = true;
                                            }
                                        }
                                    }

                                    foreach ( var row in resultList )
                                    {
                                        var resultValue = new ResultValue();

                                        resultValue.Value = row[0].AsDecimal();
                                        if ( getMetricValueDateTimeFromResultSet )
                                        {
                                            resultValue.MetricValueDateTime = row[1].AsDateTime() ?? scheduleDateTime;
                                        }
                                        else
                                        {
                                            resultValue.MetricValueDateTime = scheduleDateTime;
                                        }

                                        resultValue.Partitions = new List<ResultValuePartition>();
                                        int partitionPosition = 0;
                                        int partitionFieldIndex = getMetricValueDateTimeFromResultSet ? 2 : 1;
                                        int partitionColumnCount = row.Length - 1;
                                        while ( partitionFieldIndex <= partitionColumnCount )
                                        {
                                            resultValue.Partitions.Add( new ResultValuePartition
                                            {
                                                PartitionPosition = partitionPosition,
                                                EntityId = row[partitionFieldIndex].AsIntegerOrNull()
                                            } );

                                            partitionPosition++;
                                            partitionFieldIndex++;
                                        }

                                        resultValues.Add( resultValue );
                                    }
                                }
                            }

                            metric.LastRunDateTime = scheduleDateTime;
                            metricResult.MetricValuesCalculated += resultValues.Count();

                            if ( resultValues.Any() )
                            {
                                List<MetricValue> metricValuesToAdd = new List<MetricValue>();
                                foreach ( var resultValue in resultValues )
                                {
                                    var metricValue = new MetricValue();
                                    metricValue.MetricId = metric.Id;
                                    metricValue.MetricValueDateTime = resultValue.MetricValueDateTime;
                                    metricValue.MetricValueType = MetricValueType.Measure;
                                    metricValue.YValue = resultValue.Value;
                                    metricValue.CreatedDateTime = RockDateTime.Now;
                                    metricValue.ModifiedDateTime = RockDateTime.Now;

                                    // Add a note to the metric value if the metric was run manually.
                                    if ( isManualRun )
                                    {
                                        metricValue.Note = "Run manually";
                                    }

                                    metricValue.MetricValuePartitions = new List<MetricValuePartition>();
                                    var metricPartitionsByPosition = metricPartitions.OrderBy( a => a.Order ).ToList();

                                    if ( !resultValue.Partitions.Any() && metricPartitionsByPosition.Count() == 1 && !metricPartitionsByPosition[0].EntityTypeId.HasValue )
                                    {
                                        // A metric with just the default partition (not partitioned by Entity).
                                        var metricPartition = metricPartitionsByPosition[0];
                                        var metricValuePartition = new MetricValuePartition();
                                        metricValuePartition.MetricPartition = metricPartition;
                                        metricValuePartition.MetricPartitionId = metricPartition.Id;
                                        metricValuePartition.MetricValue = metricValue;
                                        metricValuePartition.EntityId = null;
                                        metricValue.MetricValuePartitions.Add( metricValuePartition );
                                    }
                                    else
                                    {
                                        foreach ( var partitionResult in resultValue.Partitions )
                                        {
                                            if ( metricPartitionsByPosition.Count > partitionResult.PartitionPosition )
                                            {
                                                var metricPartition = metricPartitionsByPosition[partitionResult.PartitionPosition];
                                                var metricValuePartition = new MetricValuePartition();
                                                metricValuePartition.MetricPartition = metricPartition;
                                                metricValuePartition.MetricPartitionId = metricPartition.Id;
                                                metricValuePartition.MetricValue = metricValue;
                                                metricValuePartition.EntityId = partitionResult.EntityId;
                                                metricValue.MetricValuePartitions.Add( metricValuePartition );
                                            }
                                        }
                                    }

                                    if ( metricValue.MetricValuePartitions == null || !metricValue.MetricValuePartitions.Any() )
                                    {
                                        // This shouldn't happen, but just in case, throw an exception.
                                        throw new Exception( "MetricValue requires at least one Partition Value" );
                                    }
                                    else
                                    {
                                        metricValuesToAdd.Add( metricValue );
                                    }
                                }

                                var dbTransaction = rockContextForMetricValues.Database.BeginTransaction();
                                var measureMetricValueType = MetricValueType.Measure;

                                if ( getMetricValueDateTimeFromResultSet )
                                {
                                    var metricValueDateTimes = metricValuesToAdd.Select( a => a.MetricValueDateTime ).Distinct().ToList();
                                    foreach ( var metricValueDateTime in metricValueDateTimes )
                                    {
                                        bool alreadyHasMetricValues = metricValueService.Queryable()
                                            .Where( a => a.MetricId == metric.Id && a.MetricValueDateTime == metricValueDateTime && a.MetricValueType == measureMetricValueType ).Any();
                                        if ( alreadyHasMetricValues )
                                        {
                                            // Use direct SQL to remove any existing metric values.
                                            rockContextForMetricValues.Database.ExecuteSqlCommand(
                                                @"
                                                    DELETE
                                                    FROM MetricValuePartition
                                                    WHERE MetricValueId IN (
                                                        SELECT Id
                                                        FROM MetricValue
                                                        WHERE MetricId = @metricId
                                                        AND MetricValueDateTime = @metricValueDateTime
                                                        AND MetricValueType = @measureMetricValueType
                                                    )
                                                ",
                                                new SqlParameter( "@metricId", metric.Id ),
                                                new SqlParameter( "@metricValueDateTime", metricValueDateTime ),
                                                new SqlParameter( "@measureMetricValueType", measureMetricValueType ) );

                                            rockContextForMetricValues.Database.ExecuteSqlCommand(
                                                @"
                                                    DELETE
                                                    FROM MetricValue
                                                    WHERE MetricId = @metricId
                                                    AND MetricValueDateTime = @metricValueDateTime
                                                    AND MetricValueType = @measureMetricValueType
                                                ",
                                                new SqlParameter( "@metricId", metric.Id ),
                                                new SqlParameter( "@metricValueDateTime", metricValueDateTime ),
                                                new SqlParameter( "@measureMetricValueType", measureMetricValueType ) );
                                        }
                                    }
                                }

                                metricValueService.AddRange( metricValuesToAdd );

                                // Disable SaveChanges PrePostProcessing since there could be hundreds or thousands of metric values getting inserted or updated.
                                rockContextForMetricValues.SaveChanges( true );

                                dbTransaction.Commit();
                            }

                            /*
                                5/11/2023 - KA
                                We are calling the SaveChanges( true ) overload that disables pre/post processing hooks
                                because the LastRunDateTime property of the Metric is updated above. If we don't disable
                                these hooks, the [ModifiedDateTime] value will also be updated every time a metric is
                                calculated, which is not what we want here.
                            */
                            rockContextForMetricEntity.SaveChanges( true );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                metricResult.MetricException = new Exception( $"Exception when calculating metric for {metric}: {ex.Message}", ex );
            }

            return metricResult;
        }

        /// <summary>
        /// 
        /// </summary>
        private struct ResultValue
        {
            public List<ResultValuePartition> Partitions { get; set; }

            public decimal Value { get; set; }

            public DateTime MetricValueDateTime { get; set; }
        }

        private struct ResultValuePartition
        {
            // Zero-based partition position
            public int PartitionPosition { get; set; }

            public int? EntityId { get; set; }
        }

        #region Metric Reporting

        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <param name="metricIds">The metric identifier list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="metricValueType">Type of the metric value.</param>
        /// <param name="partitionValues">
        /// A collection of identifiers that specify the metric partition values to be included.
        /// If not specified, values from all partitions will be included.
        /// </param>
        /// <returns></returns>
        public IQueryable<MetricValue> GetMetricValuesQuery( List<int> metricIds, MetricValueType? metricValueType = null, DateTime? startDate = null, DateTime? endDate = null, List<EntityIdentifierByTypeAndId> partitionValues = null )
        {
            var valuesService = new MetricValueService( ( RockContext ) this.Context );
            var qry = valuesService.Queryable()
                .AsNoTracking()
                .Include( a => a.MetricValuePartitions.Select( b => b.MetricPartition ) )
                .Where( a => metricIds.Contains( a.MetricId ) );

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

            // If partition filters are specified, ensure that the MetricValue matches the entity instance and is for the same Entity Type as the partition.
            if ( partitionValues != null )
            {
                partitionValues = partitionValues.Where( pv => pv.EntityTypeId != 0 && pv.EntityId != 0 ).ToList();
                foreach ( var partitionValue in partitionValues )
                {
                    qry = qry.Where( a => a.MetricValuePartitions.Any( p => p.EntityId == partitionValue.EntityId && p.MetricPartition.EntityTypeId == partitionValue.EntityTypeId ) );
                }
            }

            return qry;
        }

        /// <summary>
        /// Gets a set of values for the specified Metrics, and summarizes the values for all of the specified partitions.
        /// </summary>
        /// <param name="metricIds">The metric identifier list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="metricValueType">Type of the metric value.</param>
        /// <param name="partitionValues">
        /// A collection of identifiers that specify the metric partition values to be included.
        /// If not specified, values from all partitions will be included.
        /// </param>
        /// <returns></returns>
        public List<MetricValueSummary> GetMetricValueSummaries( List<int> metricIds, MetricValueType? metricValueType = null, DateTime? startDate = null, DateTime? endDate = null, List<EntityIdentifierByTypeAndId> partitionValues = null )
        {
            var qry = GetMetricValuesQuery( metricIds,
                metricValueType,
                startDate,
                endDate,
                partitionValues );

            string seriesName = null;
            if ( partitionValues != null && partitionValues.Any() )
            {
                var partitionNames = GetSeriesPartitionNames( partitionValues );
                if ( partitionNames.Any() )
                {
                    seriesName = partitionNames.AsDelimited( "," );
                }

            }

            var groupBySum = qry
                .GroupBy( a => a.Metric )
                .Select( g => new
                {
                    MetricId = g.Key.Id,
                    MetricTitle = g.Key.Title,
                    YValueTotal = g.Sum( s => s.YValue ),
                    SeriesName = seriesName,
                    MetricValueType = metricValueType ?? MetricValueType.Measure
                } )
                .ToList();

            var summaries = groupBySum.Select( s => new MetricValueSummary
            {
                MetricId = s.MetricId,
                MetricTitle = s.MetricTitle,
                SeriesName = seriesName,
                ValueTotal = s.YValueTotal,
                MetricType = s.MetricValueType,
                StartDateTimeStamp = startDate.HasValue ? startDate.Value.ToJavascriptMilliseconds() : 0,
                EndDateTimeStamp = endDate.HasValue ? endDate.Value.ToJavascriptMilliseconds() : 0
            } )
                .ToList();

            return summaries;
        }

        /// <summary>
        /// Gets a list of metric partition names.
        /// </summary>
        /// <param name="partitionValues">The metric value partitions list.</param>
        /// <returns></returns>
        private List<string> GetSeriesPartitionNames( List<EntityIdentifierByTypeAndId> partitionValues = null )
        {
            var rockContext = new RockContext();

            List<string> seriesPartitionValues = new List<string>();

            foreach ( var partitionValue in partitionValues )
            {
                if ( partitionValue.EntityTypeId == 0 || partitionValue.EntityId == 0 )
                {
                    continue;
                }

                var entityTypeCache = EntityTypeCache.Get( partitionValue.EntityTypeId );
                if ( entityTypeCache != null )
                {
                    if ( entityTypeCache.Id == EntityTypeCache.GetId<Campus>() )
                    {
                        var campus = CampusCache.Get( partitionValue.EntityId );
                        if ( campus != null )
                        {
                            seriesPartitionValues.Add( campus.Name );
                        }
                    }
                    else if ( entityTypeCache.Id == EntityTypeCache.GetId<DefinedValue>() )
                    {
                        var definedValue = DefinedValueCache.Get( partitionValue.EntityId );
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
                        var result = getMethod.Invoke( serviceInstance, new object[] { partitionValue.EntityId } );
                        if ( result != null )
                        {
                            seriesPartitionValues.Add( result.ToString() );
                        }
                    }
                }
            }

            return seriesPartitionValues;
        }

        /// <summary>
        /// Summary information about the value of a metric in a specified time period.
        /// </summary>
        public class MetricValueSummary
        {
            /// <summary>
            /// Gets or sets the metric identifier.
            /// </summary>
            /// <value>
            /// The metric identifier.
            /// </value>
            public int MetricId { get; set; }

            /// <summary>
            /// Gets or sets the name of the specific series or partitions represented by this value.
            /// </summary>
            public string SeriesName { get; set; }

            /// <summary>
            /// Gets or sets the type of value recorded by this metric, either a measure or a goal.
            /// </summary>
            public MetricValueType MetricType { get; set; }

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
            public decimal? ValueTotal { get; set; }

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
        /// Identifies an entity value that identifies a metric partition.
        /// </summary>
        public class EntityIdentifierByTypeAndId
        {
            /// <summary>
            /// The Entity Type identifier.
            /// </summary>
            public int EntityTypeId;

            /// <summary>
            /// The Entity instance identifier.
            /// </summary>
            public int EntityId;
        }
    }

    #endregion
}

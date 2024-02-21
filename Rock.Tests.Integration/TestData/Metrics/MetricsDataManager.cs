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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Metrics
        {

        }
    }
}

namespace Rock.Tests.Integration.Metrics
{
    /// <summary>
    /// Provides actions to manage Metrics data.
    /// </summary>
    public class MetricsDataManager
    {
        private static Lazy<MetricsDataManager> _dataManager = new Lazy<MetricsDataManager>();
        public static MetricsDataManager Instance => _dataManager.Value;

        private const string TestDataForeignKey = "test_data";

        #region Metric Value

        public class MetricValueInfo
        {
            public string MetricIdentifier;
            public string MetricType;
            public string ValueDate;
            public string Campus;
            public string Value;

            public List<MetricValuePartitionInfo> Partitions;
        }

        public class MetricValuePartitionInfo
        {
            /// <summary>
            /// The identifier of the entity type that is used to partition the values.
            /// </summary>
            public string PartitionEntityTypeIdentifier;

            /// <summary>
            /// The identifier of the entity that is used to qualify values for inclusion in this partition.
            /// </summary>
            public string EntityIdentifier;
        }

        public class CreateMetricValueActionArgs : CreateEntityActionArgsBase<MetricValueInfo>
        {
        }

        public class UpdateMetricValueActionArgs : UpdateEntityActionArgsBase<MetricValueInfo>
        {
        }

        public bool DeleteMetricValue( string metricValueIdentifier, RockContext context )
        {
            var metricValueService = new MetricValueService( context );
            var metricValue = metricValueService.Get( metricValueIdentifier );

            if ( metricValue == null )
            {
                return false;
            }

            return metricValueService.Delete( metricValue );
        }

        /// <summary>
        /// Add a new MetricValue.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int AddMetricValue( CreateMetricValueActionArgs args )
        {
            MetricValue newMetricValue = null;

            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var metricValueService = new MetricValueService( rockContext );
                if ( args.Guid != null )
                {
                    newMetricValue = metricValueService.Get( args.Guid.Value );
                    if ( newMetricValue != null )
                    {
                        if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteMetricValue( args.Guid.Value.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }

                            newMetricValue = null;
                        }
                    }
                }

                if ( newMetricValue == null )
                {
                    newMetricValue = new MetricValue();

                    if ( args.Guid.HasValue )
                    {
                        newMetricValue.Guid = args.Guid.Value;
                    }

                    newMetricValue.MetricValueType = MetricValueType.Measure;

                    metricValueService.Add( newMetricValue );
                }

                if ( args.ForeignKey != null )
                {
                    newMetricValue.ForeignKey = args.ForeignKey;
                }

                UpdateMetricValuePropertiesFromInfo( newMetricValue, args.Properties, rockContext );

                rockContext.SaveChanges();
            } );

            return newMetricValue.Id;
        }

        /// <summary>
        /// Update an existing MetricValue.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void UpdateMetricValue( UpdateMetricValueActionArgs args )
        {
            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var MetricValueService = new MetricValueService( rockContext );
                var MetricValue = MetricValueService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                UpdateMetricValuePropertiesFromInfo( MetricValue, args.Properties, rockContext );

                rockContext.SaveChanges();
            } );
        }

        private void UpdateMetricValuePropertiesFromInfo( MetricValue newValue, MetricValueInfo actionInfo, RockContext rockContext )
        {
            var metric = newValue.Metric;
            if ( actionInfo.MetricIdentifier != null )
            {
                var metricService = new MetricService( rockContext );
                metric = metricService.GetByIdentifierOrThrow( actionInfo.MetricIdentifier );

                newValue.MetricId = metric.Id;
            }

            if ( actionInfo.MetricType != null )
            {
                newValue.MetricValueType = actionInfo.MetricType.ConvertToEnum<MetricValueType>( MetricValueType.Measure );
            }
            if ( actionInfo.ValueDate != null )
            {
                newValue.MetricValueDateTime = actionInfo.ValueDate.AsDateTime();
            }
            if ( actionInfo.Value != null )
            {
                newValue.YValue = actionInfo.Value.AsDecimalOrNull();
            }
            if ( actionInfo.Value != null )
            {
                newValue.YValue = actionInfo.Value.AsDecimalOrNull();
            }

            // Assign the Partitions for the Metric Value.
            if ( actionInfo.Partitions != null )
            {
                if ( metric == null )
                {
                    var metricService = new MetricService( rockContext );
                    metric = metricService.Get( newValue.MetricId );
                }

                var entityTypeService = new EntityTypeService( rockContext );

                foreach ( var newPartitionInfo in actionInfo.Partitions )
                {
                    var partitionEntityType = entityTypeService.GetByIdentifierOrThrow( newPartitionInfo.PartitionEntityTypeIdentifier );
                    var partitionId = metric.MetricPartitions
                        .Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == partitionEntityType.Id )
                        .Select( p => p.Id )
                        .FirstOrDefault();
                    var entityId = Reflection.GetEntityIdForEntityType( partitionEntityType.Id,
                        newPartitionInfo.EntityIdentifier,
                        allowIntegerIdentifier: true,
                        rockContext );

                    var newPartition = new MetricValuePartition
                    {
                        MetricPartitionId = partitionId,
                        EntityId = entityId,
                        ForeignKey = newValue.ForeignKey
                    };
                    newValue.MetricValuePartitions.Add( newPartition );
                }
            }
        }

        #endregion

        #region Test Data

        public void DeleteDataForWeeklyAttendanceMetrics()
        {
            LogHelper.Log( $"Weekly Attendance Metrics: removing sample data..." );

            //            var sql = $@"
            //DELETE FROM [MetricValuePartition] WHERE [ForeignKey] = '{TestDataForeignKey}';
            //DELETE FROM [MetricValue] WHERE [ForeignKey] = '{TestDataForeignKey}'
            //";
            string sql;
            int recordsAffected;

            sql = $@"
DELETE FROM [MetricValuePartition] WHERE [ForeignKey] = '{TestDataForeignKey}'
";
            recordsAffected = DbService.ExecuteCommand( sql );

            sql = $@"
DELETE FROM [MetricValue] WHERE [ForeignKey] = '{TestDataForeignKey}'
";
            recordsAffected = DbService.ExecuteCommand( sql );

            LogHelper.Log( $"Weekly Attendance Metrics: removed {recordsAffected} entries." );
        }

        /// <summary>
        /// Adds test data for the Metric "Adult Weekly Attendance".
        /// </summary>
        public void AddDataForWeeklyAttendanceMetrics()
        {
            DeleteDataForWeeklyAttendanceMetrics();

            LogHelper.Log( $"Weekly Attendance Metrics: adding sample data..." );

            var rockContext = new RockContext();
            var campusSteppingStone = TestDataHelper.GetOrAddCampusSteppingStone( rockContext );
            var steppingStoneCampusGuidString = campusSteppingStone.Guid.ToString();

            var asAtDate = RockDateTime.Now;

            // Add X weeks of attendance data with a trend of increasing attendance.
            int maxWeeks = 5;
            for ( int i = maxWeeks; i > 0; i-- )
            {
                var thisDate = asAtDate.AddDays( -7 * i );
                var baseAttendanceStep = 100 * ( maxWeeks - i + 1 );
                var baseAttendanceMain = 1000 * ( maxWeeks - i + 1 );

                // Add Services for Stepping Stone Campus.
                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceStep, steppingStoneCampusGuidString, TestGuids.Schedules.ScheduleSat1800Guid ) );
                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceStep + 50, steppingStoneCampusGuidString, TestGuids.Schedules.ScheduleSun1030Guid ) );

                // Add Services for Main Campus.
                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceMain, TestDataHelper.MainCampusGuidString, TestGuids.Schedules.ScheduleSat1630Guid ) );
                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceMain + 200, TestDataHelper.MainCampusGuidString, TestGuids.Schedules.ScheduleSat1800Guid ) );

                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceMain + 400, TestDataHelper.MainCampusGuidString, TestGuids.Schedules.ScheduleSun0900Guid ) );
                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceMain + 600, TestDataHelper.MainCampusGuidString, TestGuids.Schedules.ScheduleSun1030Guid ) );
                AddMetricValue( NewMetricValueAction( thisDate, baseAttendanceMain + 800, TestDataHelper.MainCampusGuidString, TestGuids.Schedules.ScheduleSun1200Guid ) );
            }

            LogHelper.Log( $"Weekly Attendance Metrics: sample data added." );
        }

        private CreateMetricValueActionArgs NewMetricValueAction( DateTime dateTime, int value, string campusGuid, string scheduleGuidString )
        {
            const string adultAttendanceMetricGuid = "0D126800-2FDA-4B34-96FD-9BAE76F3A89A";

            var metricValueArgs = new CreateMetricValueActionArgs()
            {
                ForeignKey = TestDataForeignKey,
                Properties = new MetricValueInfo
                {
                    MetricIdentifier = adultAttendanceMetricGuid,
                    MetricType = "Measure",
                    ValueDate = dateTime.ToISO8601DateString(),
                    Value = value.ToString(),
                    Partitions = new List<MetricValuePartitionInfo>
                    {
                        new MetricValuePartitionInfo { PartitionEntityTypeIdentifier = "Rock.Model.Campus", EntityIdentifier = campusGuid },
                        new MetricValuePartitionInfo { PartitionEntityTypeIdentifier = "Rock.Model.Schedule", EntityIdentifier = scheduleGuidString }
                    }
                },
                ExistingItemStrategy = CreateExistingItemStrategySpecifier.Update
            };

            return metricValueArgs;
        }

        #endregion
    }
}

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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CollectHostingMetrics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add "Hosting Metrics" Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Hosting Metrics", "fa fa-tachometer-alt", "A few hosting metrics regarding the usage of resources such as the database connection pool.", SystemGuid.Category.METRIC_HOSTING_METRICS );
            Sql( $@"UPDATE [Category]
SET [IsSystem] = 1
WHERE ([Guid] = '{SystemGuid.Category.METRIC_HOSTING_METRICS}');" );

            // Add "Hosting Metrics" Metrics
            AddHostingMetric( SystemGuid.Metric.HOSTING_HARD_CONNECTS_PER_SECOND, "Hard Connects Per Second", null, "The number of connections per second that are being made to a database server." );
            AddHostingMetric( SystemGuid.Metric.HOSTING_NUMBER_OF_ACTIVE_CONNECTIONS, "Number of Active Connections", null, "The number of active connections that are currently in use." );
            AddHostingMetric( SystemGuid.Metric.HOSTING_NUMBER_OF_FREE_CONNECTIONS, "Number of Free Connections", null, " The number of connections available for use in the connection pools." );
            AddHostingMetric( SystemGuid.Metric.HOSTING_SOFT_CONNECTS_PER_SECOND, "Soft Connects Per Second", null, "The number of active connections being pulled from the connection pool." );

            // Add "Collect Hosting Metrics" ServiceJob
            AddServiceJob();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete "Collect Hosting Metrics" ServiceJob
            Sql( $@"DELETE FROM [ServiceJob]
WHERE ([Guid] = '{SystemGuid.ServiceJob.COLLECT_HOSTING_METRICS}');" );

            // Delete "Hosting Metrics" Metrics
            DeleteHostingMetric( SystemGuid.Metric.HOSTING_HARD_CONNECTS_PER_SECOND );
            DeleteHostingMetric( SystemGuid.Metric.HOSTING_NUMBER_OF_ACTIVE_CONNECTIONS );
            DeleteHostingMetric( SystemGuid.Metric.HOSTING_NUMBER_OF_FREE_CONNECTIONS );
            DeleteHostingMetric( SystemGuid.Metric.HOSTING_SOFT_CONNECTS_PER_SECOND );

            // Delete "Hosting Metrics" Category
            RockMigrationHelper.DeleteCategory( SystemGuid.Category.METRIC_HOSTING_METRICS );
        }

        /// <summary>
        /// Adds the hosting metric.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="subtitle">The subtitle.</param>
        /// <param name="description">The description.</param>
        /// <exception cref="ArgumentNullException">title</exception>
        private void AddHostingMetric( string guid, string title, string subtitle = null, string description = null )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );

            Sql( $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '{guid}'))
    , @SourceValueTypeId [int] = (SELECT [Id] FROM [DefinedValue] WHERE ([Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL}'))
    , @MetricCategoryId [int] = (SELECT [Id] FROM [Category] WHERE ([Guid] = '{SystemGuid.Category.METRIC_HOSTING_METRICS}'))
    , @Subtitle [varchar] (100) = {(string.IsNullOrWhiteSpace( subtitle ) ? "NULL" : $"'{subtitle.Replace( "'", "''" ).Substring(0, subtitle.Length > 100 ? 100 : subtitle.Length)}'")}
    , @Description [varchar] (max) = {(string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'")};

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();

    INSERT INTO [Metric]
    (
        [IsSystem]
        , [Title]
        , [Subtitle]
        , [Description]
        , [IsCumulative]
        , [SourceValueTypeId]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
        , [NumericDataType]
        , [EnableAnalytics]
    )
    VALUES
    (
        1
        , '{formattedTitle}'
        , @Subtitle
        , @Description
        , 0
        , @SourceValueTypeId
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
    );

    SET @MetricId = SCOPE_IDENTITY();

    INSERT INTO [MetricCategory]
    (
        [MetricId]
        , [CategoryId]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , @MetricCategoryId
        , 0
        , NEWID()
    );

    INSERT INTO [MetricPartition]
    (
        [MetricId]
        , [IsRequired]
        , [Order]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 1
        , 0
        , @Now
        , @Now
        , NEWID()
    );
END" );
        }
        
        /// <summary>
        /// Adds the service job.
        /// </summary>
        private void AddServiceJob()
        {
            Sql( $@"IF NOT EXISTS (
    SELECT [Id]
    FROM [ServiceJob]
    WHERE ([Class] = 'Rock.Jobs.CollectHostingMetrics')
        AND ([Guid] = '{SystemGuid.ServiceJob.COLLECT_HOSTING_METRICS}')
)
BEGIN
    INSERT INTO [ServiceJob] (
        [IsSystem]
        , [IsActive]
        , [Name]
        , [Description]
        , [Class]
        , [CronExpression]
        , [NotificationStatus]
        , [Guid]
    )
    VALUES (
        1
        , 0
        , 'Collect Hosting Metrics'
        , 'This job will collect a few hosting metrics regarding the usage of resources such as the database connection pool. Note that this Job can be activated/deactivated by navigating to ""Admin Tools > System Settings > System Configuration > Web.Config Settings"" and toggling the ""Enable Database Performance Counters"" setting.'
        , 'Rock.Jobs.CollectHostingMetrics'
        , '0 0/5 * 1/1 * ? *'
        , 1
        , '{SystemGuid.ServiceJob.COLLECT_HOSTING_METRICS}'
    );
END" );
        }

        /// <summary>
        /// Deletes the hosting metric.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void DeleteHostingMetric( string guid )
        {
            Sql( $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '{guid}'));

IF (@MetricId IS NOT NULL)
BEGIN
    DELETE FROM [MetricPartition] WHERE ([MetricId] = @MetricId);
    DELETE FROM [MetricCategory] WHERE ([MetricId] = @MetricId);
    DELETE FROM [Metric] WHERE ([Id] = @MetricId);
END" );
        }
    }
}

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
    public partial class MediaElementModelChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelChangesUp();
            AddSyncMediaJob();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveSyncMediaJob();
            ModelChangesDown();
        }

        private void ModelChangesUp()
        {
            AddColumn( "dbo.MediaElement", "MetricData", c => c.String() );
            DropColumn( "dbo.MediaElement", "Duration" );
            AddColumn( "dbo.MediaElement", "Duration", c => c.Int() );
            DropColumn( "dbo.MediaElement", "SourceMetric" );
        }

        private void ModelChangesDown()
        {
            AddColumn( "dbo.MediaElement", "SourceMetric", c => c.String() );
            AlterColumn( "dbo.MediaElement", "Duration", c => c.Decimal( precision: 18, scale: 2 ) );
            DropColumn( "dbo.MediaElement", "MetricData" );
        }

        private void AddSyncMediaJob()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{SystemGuid.ServiceJob.SYNC_MEDIA}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[Guid]
                ) VALUES (
                    0
                    ,1
                    ,'Sync Media'
                    ,'Synchronizes media content from configured Media Accounts.'
                    ,'Rock.Jobs.SyncMedia'
                    ,'0 15 0/2 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.SYNC_MEDIA}'
                );
            END" );
        }

        private void RemoveSyncMediaJob()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Guid] = '{SystemGuid.ServiceJob.SYNC_MEDIA}'
                " );
        }
    }
}

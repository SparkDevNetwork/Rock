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
    public partial class UpdateInteractionSessionIndices : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractionSession", "InteractionChannelId", c => c.Int());
            AddForeignKey("dbo.InteractionSession", "InteractionChannelId", "dbo.InteractionChannel", "Id");

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_SESSION_START_DATE_KEY}'
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
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v16.0 - Update InteractionSession SessionStartDateKey columns.'
                    , 'This job update all empty SessionStartDateKey columns on the InteractionSession table with their corresponding InteractionDateKey values.'
                    , 'Rock.Jobs.PostV16UpdateInteractionSessionStartDateKey'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_SESSION_START_DATE_KEY}'
                );
            END" );

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_INTERACTION_CHANNEL_ID}'
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
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v16.0 - Update InteractionSession InteractionChannelId columns.'
                    , 'This job update all empty InteractionChannelId columns on the InteractionSession table with their corresponding InteractionChannelId values.'
                    , 'Rock.Jobs.PostV16UpdateInteractionSessionChannelId'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_INTERACTION_CHANNEL_ID}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.InteractionSession", "InteractionChannelId", "dbo.InteractionChannel");
            DropColumn("dbo.InteractionSession", "InteractionChannelId");
        }
    }
}

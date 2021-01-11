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
    public partial class AddCommunicationRecipientAddIndexJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddJobToUpdateCommunicationRecipientIndex();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveJobToUpdateCommunicationRecipientIndex();
        }

        private void AddJobToUpdateCommunicationRecipientIndex()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV12DataMigrationsAddStatusIndexToCommunicationRecipient'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATIONRECIPIENT_INDEX}'
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
                    1
                    ,1
                    ,'Rock Update Helper v12.0 - Add index for Status column of the CommunicationRecipient table.'
                    ,'This job will add a new index for the Status column of the CommunicationRecipient table.'
                    ,'Rock.Jobs.PostV12DataMigrationsAddStatusIndexToCommunicationRecipient'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATIONRECIPIENT_INDEX}'
                );
            END" );
        }

        private void RemoveJobToUpdateCommunicationRecipientIndex()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV12DataMigrationsAddInteractionIndexes'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATIONRECIPIENT_INDEX}'
                " );
        }
    }
}

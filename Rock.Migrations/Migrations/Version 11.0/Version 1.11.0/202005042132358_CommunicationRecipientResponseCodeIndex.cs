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
    public partial class CommunicationRecipientResponseCodeIndex : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* NOTE: Since shorting ResponseCode to nvarchar(6), and adding an index could take a few minutes, we'll do this in a runonce job instead */
            // AlterColumn("dbo.CommunicationRecipient", "ResponseCode", c => c.String(maxLength: 6));
            
            AddJobToAddResponseCodeIndex();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveJobToAddResponseCodeIndex();
        }

        private void AddJobToAddResponseCodeIndex()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV110DataMigrationsResponseCodeIndex'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_COMMUNICATIONRECIPIENT_RESPONSECODE_INDEX}'
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
                    ,'Does Post-V11 Migration for Communication Recipient ResponseCode Index'
                    ,'This job will update Communication Recipient ResponseCode to have an index on ReponseCode and CreatedDateTime.'
                    ,'Rock.Jobs.PostV110DataMigrationsResponseCodeIndex'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_COMMUNICATIONRECIPIENT_RESPONSECODE_INDEX}'
                );
            END" );
        }

        private void RemoveJobToAddResponseCodeIndex()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV110DataMigrationsResponseCodeIndex'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_COMMUNICATIONRECIPIENT_RESPONSECODE_INDEX}'
                " );
        }
    }
}

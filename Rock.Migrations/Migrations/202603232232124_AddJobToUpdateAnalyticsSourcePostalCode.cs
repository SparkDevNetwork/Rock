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
    public partial class AddJobToUpdateAnalyticsSourcePostalCode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Adds the job that processes the AnalyticsSourcePostalCode census data.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.UpdateAnalyticsSourcePostalCode'
                                AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_POSTAL_CODE}'
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
                    ,[HistoryCount]
                    ,[Guid]
                ) VALUES (
                    0
                    ,1
                    ,'Update Analytics Source PostalCode v19'
                    ,'Job to update the UpdateAnalyticsSourcePostalCode table with geographical and census data.'
                    ,'Rock.Jobs.UpdateAnalyticsSourcePostalCode'
                    ,'0 20 1 1/1 * ? *'
                    ,1
                    ,500 
                    ,'{SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_POSTAL_CODE}'
                );
            END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

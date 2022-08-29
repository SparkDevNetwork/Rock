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
    public partial class Rollup_20220715 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Update_spCrm_FamilyAnalyticsEraDataset();
            //FixIncorrectERAStartDate();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Fixes the incorrect era start date. The error was introduced in 13.4 and fixed in 13.6.
        /// This does not need to be run again in 14.0 and is not "run n safe".
        /// For Rock instances that pull from the repo instead of applying the updates they can run this job manually if needed.
        /// </summary>
        private void FixIncorrectERAStartDate()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV136FixIncorrectERAStartDate'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_136_FIX_INCORRECT_ERA_START_DATE}'
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
                    ,'Rock Update Helper v13.6 - Fix Incorrect eRA Start Dates'
                    ,'This job fixes eRA Start Dates (broken in v13.4) for people who are currently eRA.'
                    ,'Rock.Jobs.PostV136FixIncorrectERAStartDate'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_136_FIX_INCORRECT_ERA_START_DATE}'
                );
            END" );
        }

        private void Update_spCrm_FamilyAnalyticsEraDataset()
        {
            Sql( MigrationSQL._202207152352391_Rollup_20220715_spCrm_FamilyAnalyticsEraDataset );
        }
    }
}

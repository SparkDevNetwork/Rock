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
    public partial class Rollup_20230629 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixBreadcrumbDisplay();
            AddRunOnceJobUpdateInteractionAndInteractionSessionTablesIndexes();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// GJ: Fix Breadcrumb Display
        /// </summary>
        private void FixBreadcrumbDisplay()
        {
            Sql( @"UPDATE [Page] SET [BreadCrumbDisplayName] = '0' WHERE [Guid] = 'F3C96663-1079-4F20-BABA-3F3203AFCFF3'" );
        }

        /// <summary>
        /// KA: Migration to Add Run Once Job to Update the indexes on the interaction and interaction session tables
        /// This goes along with commit 878c565
        /// SQL to add/register a Post Update (run once) job to add/drop indexes on Interaction and InteractionSession
        /// </summary>
        private void AddRunOnceJobUpdateInteractionAndInteractionSessionTablesIndexes()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_AND_INTERACTION_INDICES}'
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
                    , 'Rock Update Helper v16.0 - Update Interaction and InteractionSession indices.'
                    , 'This job will update the indices on the Interaction and InteractionSession tables.'
                    , 'Rock.Jobs.PostV16UpdateInteractionAndInteractionSessionIndices'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_AND_INTERACTION_INDICES}'
                );
            END" );
        }
    }
}

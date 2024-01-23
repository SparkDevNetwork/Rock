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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 194, "1.16.3" )]
    public class MigratoinRollupForV17_0_3 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveLegacyPreferences();
            AddUpdatePersistedDatasetServiceUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// DH: Add Run-Once job for Rock.Jobs.PostUpdateJobs.PostV17RemoveLegacyPreferencesPostMigration
        /// </summary>
        private void RemoveLegacyPreferences()
        {
            // note: the cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - Remove Legacy Preferences Post Migration Job.",
                description: "This job removes the legacy user preferences from the Attribute and AttributeValue tables.",
                jobType: "Rock.Jobs.PostUpdateJobs.PostV17RemoveLegacyPreferencesPostMigration",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_LEGACY_PREFERENCES );
        }

        /// <summary>
        /// JDR: Add the UpdatePersistedDataset job to ServiceJobs out of the box
        /// If the job is already there, update the cron expression to run every minute
        /// </summary>
        private void AddUpdatePersistedDatasetServiceUp()
        {
            Sql( $@"
          IF NOT EXISTS (
              SELECT 1
              FROM [ServiceJob]
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}'
          )
          BEGIN
              INSERT INTO [ServiceJob] (
                  [IsSystem],
                  [IsActive],
                  [Name],
                  [Description],
                  [Class],
                  [CronExpression],
                  [NotificationStatus],
                  [Guid]
              ) VALUES (
                  1, -- IsSystem
                  1, -- IsActive
                  'Update Persisted Datasets', -- Name
                  'This job will update persisted datasets.', -- Description
                  'Rock.Jobs.UpdatePersistedDatasets', -- Class
                  '0 * * * * ?', -- CronExpression to run every minute
                  1, -- NotificationStatus
                  '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}' -- Guid
              );
          END
          ELSE
          BEGIN
              -- Update the cron expression if the job already exists
              UPDATE [ServiceJob]
              SET [CronExpression] = '0 * * * * ?' -- CronExpression to run every minute
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}';
          END" );

        }
    }
}

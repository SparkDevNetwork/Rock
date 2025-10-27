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
    public partial class Rollup_20251021 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixForIssue6491();
            JPH_UpdateChoppedCommunicationBlockSettings_20251020_Up();
            UpdateDefaultNightlySchedule();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region NA: Data Migration Fix for Issue 6491

        private void FixForIssue6491()
        {
            Sql( @"UPDATE [DefinedValue] SET [Value] = 'Obsoleted Contribution Statement Lava block will be removed in the future. Please replace it with Contribution Statement Generator block.' WHERE [Guid] = '85A4AE6F-8B52-484B-B594-C9A15EEDEBE1'" );
        }

        #endregion

        #region JPH: Update Block Settings for Recently-Chopped Communication Blocks

        /// <summary>
        /// JPH - Update block settings for recently-chopped Communication blocks.
        /// </summary>
        private void JPH_UpdateChoppedCommunicationBlockSettings_20251020_Up()
        {
            // Because these blocks will have been chopped in a Rock update job, we're adding a follow-up job to update
            // the block settings AFTER these chops have taken place. This is the second time we're running this
            // particular job, because it has been improved to include the "Detail Page" setting for the Communication
            // List block. The actual name of the job class (PostV18UpdateEmailPreferenceEntryBlockManageMyAccountPage)
            // has been left in place since it was referenced as such in a previous rollup migration.
            // https://github.com/SparkDevNetwork/Rock/commit/b22862d70ba4671a7a028d8fac272dd8b2155a3a
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Update Block Settings for Recently-Chopped Communication Blocks",
                description: "This job will update block settings for recently-chopped Communication blocks.",
                jobType: "Rock.Jobs.PostV18UpdateEmailPreferenceEntryBlockManageMyAccountPage",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_180_UPDATE_EMAIL_PREFERENCE_ENTRY_BLOCK_MANAGE_MY_ACCOUNT_PAGE );
        }

        #endregion

        #region ME: UpdateDefaultNightlySchedule

        private void UpdateDefaultNightlySchedule()
        {
            Sql( $@"UPDATE Schedule
SET EffectiveStartDate = CAST('2025-07-16' AS DATE)
WHERE [Guid] = '26708BFB-B645-4C59-848B-E64A2C4BD5B8';" );
        }

        #endregion
    }
}

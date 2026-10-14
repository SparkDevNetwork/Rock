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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Adds a post update job to clean up Social Media Account attribute values
    /// that are not safe to display.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 327, "17.8" )]
    public class CleanSocialMediaAccountValues : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddCleanSocialMediaAccountValuesJobUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Adds a post update job to clean up Social Media Account attribute values.
        /// This is done in a job rather than in the migration because the values
        /// need to be checked in C# and large databases can have many of them.
        /// </summary>
        private void AddCleanSocialMediaAccountValuesJobUp()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.11 - Clean Social Media Account Values",
                description: "This job will clean up Social Media Account attribute values that are not safe to display.",
                jobType: "Rock.Jobs.PostV1711CleanSocialMediaAccountValues",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_1711_CLEAN_SOCIAL_MEDIA_ACCOUNT_VALUES );
        }
    }
}

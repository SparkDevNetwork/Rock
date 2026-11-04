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
    /// Adds a post update job to add the indexes that personal device lookups
    /// use. Fix for issue #7146.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 342, "17.8" )]
    public class AddPersonalDeviceIndexes : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPersonalDeviceIndexesJobUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Adds a post update job to add the personal device indexes.
        /// This is done in a job rather than in the migration because changing
        /// the DeviceRegistrationId column rewrites every row, which can take
        /// longer than a migration is allowed to run on a large PersonalDevice table.
        /// It runs at 2 AM because the work locks the table.
        /// </summary>
        private void AddPersonalDeviceIndexesJobUp()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v20.1 - Add Personal Device Indexes",
                description: "This job will add indexes that keep personal device lookups fast.",
                jobType: "Rock.Jobs.PostV201AddPersonalDeviceIndexes",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_201_ADD_PERSONAL_DEVICE_INDEXES );
        }
    }
}

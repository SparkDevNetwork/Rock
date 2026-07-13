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
    /// Updates the [spCrm_FamilyAnalyticsEraDataset], [spCrm_FamilyAnalyticsGiving],
    /// and [spCrm_FamilyAnalyticsAttendance] stored procedures so that the
    /// weekly evaluation window uses the SundayDate column for its boundary
    /// comparisons. Previously the boundaries compared Attendance.StartDateTime
    /// (which includes a time-of-day) against a Sunday-midnight variable, which
    /// caused check-ins on the final Sunday of the window to be excluded and
    /// caused check-ins on the starting boundary Sunday to be incorrectly
    /// included. The mis-count could complete an eRA Core Step for people who
    /// were still actively attending and could distort the First/Last CheckIn,
    /// First/Last Gift, and gift/attendance count attributes. Fix for issue #6902.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 306, "19.3" )]
    public class FixEraFamilyAnalyticsWeekBoundaries6902 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( HotFixMigrationResource._306_FixEraFamilyAnalyticsWeekBoundaries6902_spCrm_FamilyAnalyticsEraDataset );
            Sql( HotFixMigrationResource._306_FixEraFamilyAnalyticsWeekBoundaries6902_spCrm_FamilyAnalyticsGiving );
            Sql( HotFixMigrationResource._306_FixEraFamilyAnalyticsWeekBoundaries6902_spCrm_FamilyAnalyticsAttendance );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}

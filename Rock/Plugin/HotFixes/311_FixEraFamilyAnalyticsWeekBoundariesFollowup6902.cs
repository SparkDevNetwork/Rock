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
    /// Follow-up to hotfix 306 for issue #6902. Two cleanup items remained in
    /// the [spCrm_FamilyAnalyticsEraDataset] stored procedure after that fix:
    /// the outer pre-filter WHERE clauses that populate the giving and
    /// attendance temp tables still compared the raw TransactionDateTime /
    /// StartDateTime columns against a Sunday-midnight variable, which is the
    /// same buggy pattern that was corrected elsewhere. Those two
    /// lines are now aligned to use SundayDate week-boundary logic. This
    /// migration also switches the two GETDATE() calls in the procedure to
    /// dbo.RockGetDate() so eRA start/created timestamps honor Rock's
    /// configured time zone (often matters for Azure-hosted SQL databases whose
    /// server time zone may not match the organization's).
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 311, "19.5" )]
    public class FixEraFamilyAnalyticsWeekBoundariesFollowup6902 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( HotFixMigrationResource._311_FixEraFamilyAnalyticsWeekBoundariesFollowup6902_spCrm_FamilyAnalyticsEraDataset );
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

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

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Repairs "Activity Added" ConnectionWorkflow triggers whose qualifier was stored
    /// as a ConnectionActivityType Guid instead of the integer Id. Fix for issue #6986.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 315, "19.5" )]
    public class FixConnectionWorkflowActivityQualifier6986 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MSE_FixActivityAddedQualifierGuids_6986_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        /// <summary>
        /// Rewrites broken "Activity Added" qualifier values from "|Guid||" to "|Id||".
        /// </summary>
        private void MSE_FixActivityAddedQualifierGuids_6986_Up()
        {
            /*
                8/20/2026 - MSE

                The Obsidian Connection Opportunity Detail block saved the Activity Added
                qualifier as "|<ActivityTypeGuid>||" instead of "|<ActivityTypeId>||", so
                those triggers never launched their workflow. This rebuilds the exact broken
                string for each activity type and replaces it with the correct Id form by
                whole-value equality, so no other value can match and re-running is a no-op.
                LOWER() is needed because casting a uniqueidentifier to varchar produces
                uppercase while the saved values are lowercase.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6986
            */

            const int activityAddedTriggerType = ( int ) ConnectionWorkflowTriggerType.ActivityAdded;

            Sql( $@"
UPDATE cw
SET cw.[QualifierValue] = CONCAT( '|', cat.[Id], '||' )
FROM [ConnectionWorkflow] cw
INNER JOIN [ConnectionActivityType] cat
    ON cw.[QualifierValue] = CONCAT( '|', LOWER( CAST( cat.[Guid] AS varchar(36) ) ), '||' )
WHERE cw.[TriggerType] = {activityAddedTriggerType}
" );
        }
    }
}

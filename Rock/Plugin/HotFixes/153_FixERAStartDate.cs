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
    [MigrationNumber( 153, "1.13.4" )]
    public class FixERAStartDate : Migration
    {
        /* 07/14/2022 MDP
         
         The migration number jumps from 147 to 153 because we had to add this hotfix migration
         because 147-152 were moved to 154 so that 153 will run before those.
         
         */


        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Update_spCrm_FamilyAnalyticsEraDataset();
            FixIncorrectERAStartDate();
        }

        private void FixIncorrectERAStartDate()
        {
            Sql( HotFixMigrationResource._153_FixERAStartDate_RecoverERAStartDate_Update );
        }

        private void Update_spCrm_FamilyAnalyticsEraDataset()
        {
            Sql( HotFixMigrationResource._153_FixERAStartDate_spCrm_FamilyAnalyticsEraDataset );
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

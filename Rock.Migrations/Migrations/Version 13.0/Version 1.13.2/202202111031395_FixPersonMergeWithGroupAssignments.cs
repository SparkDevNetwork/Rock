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
    /// <summary>
    /// Fixes an issue with the person merge failing if the merge candidates have group scheduling preferences for the same group. (Fixes #4881)
    /// </summary>
    public partial class FixPersonMergeWithGroupAssignments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._202202111031395_FixPersonMergeWithGroupAssignments_spCrm_PersonMerge );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Reinstate the previous version of the stored procedure.
            Sql( MigrationSQL._202107081553024_UpdateMergePersonStoredProcedureToSetIsFirstTime_spCrm_PersonMerge );
        }
    }
}

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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />

    /*
        10/9/2026 - KH

        This hotfix was added on 18.2+ alongside the Workflow Type "View List" fix for
        GitHub issue #6712. 17.9 picked up the WorkflowType ParentAuthority => Category
        change (via the WorkflowTypeCache fix cherry-picked for a security issue) but not
        this hotfix, so it now also runs on 17.x with the same number and Guid. Once it
        has run on 17.x it is recorded as 280 and will not run again after upgrading to 18.x.

        Reason: Restore View List on categorized Workflow Types in 17.x (GitHub issue #7088).
    */
    [MigrationNumber( 280, "17.8" )]
    public class AuthFixForIssue6712 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_Fix_ViewListAuthForCategory_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Updates the Category EntityType to add "ViewList" that goes along with the fix to issue 6712.
        /// https://github.com/SparkDevNetwork/Rock/issues/6712
        /// </summary>
        private void NA_Fix_ViewListAuthForCategory_Up()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Category", 0, "ViewList", true, null, 1, "E59FFA9D-1CE9-409B-B70E-F50F5008970C" );
        }
    }
}

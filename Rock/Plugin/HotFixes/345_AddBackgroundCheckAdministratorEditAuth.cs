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
using Rock.Security;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Adds a default rule allowing Rock Administrators to edit background checks.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 345, "17.8" )]
    public class AddBackgroundCheckAdministratorEditAuth : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/10/26 - CH

                Saved background checks now take their security from the Background Check
                entity type and then GlobalRestrictedDefault, which only grants Administrate.
                Without this rule, Rock Administrators would lose the Edit access they had
                before. The rule is appended after any existing rules so an administrator's
                own Background Check rules still win, and the helper skips it if it exists.

                Reason: Keep Rock Administrators' existing Edit access on background checks.
            */
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.BackgroundCheck",
                int.MaxValue,
                Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "27CB13F8-290A-4000-8581-2104D61F2F88" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
